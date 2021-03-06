using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Cors.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SharkSync.Web.Api.Services;
using SharkSync.Web.Api.ViewModels;
using SharkSync.PostgreSQL.Repositories;
using SharkSync.Interfaces;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using SharkSync.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using SharkSync.Services;
using Microsoft.AspNetCore.DataProtection.Repositories;

namespace SharkSync.Web.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; private set; }

        public IHostingEnvironment Environment { get; private set; }

        public ApplicationSettings ApplicationSettings { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(options => Configuration.GetSection("AppSettings").Bind(options));

            services.AddAWSService<IAmazonSecretsManager>();
            services.AddSingleton<ISettingsService, SettingsService>();

            var serviceProvider = services.BuildServiceProvider();
            var settings = serviceProvider.GetService<ISettingsService>();
            ApplicationSettings = settings.Get<ApplicationSettings>().Result;

            services.AddMvc();

            services.AddCors();
            services.AddHttpClient();

            services.AddDbContext<DataContext>();

            services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

            AddAuthenticationOptions(services);

            services.AddTransient<IAccountRepository, AccountRepository>();
            services.AddTransient<IApplicationRepository, ApplicationRepository>();
            services.AddTransient<IChangeRepository, ChangeRepository>();

            services.AddScoped<AuthService, AuthService>();
            services.AddSingleton<ITimeService, TimeService>();

            services.AddDataProtection();
            services.Configure<KeyManagementOptions>(o =>
            {
                o.XmlRepository = new AmazonSecretXmlRepository(serviceProvider.GetService<IAmazonSecretsManager>(), serviceProvider.GetService<IOptions<AppSettings>>());
            });
        }

        private void AddAuthenticationOptions(IServiceCollection services)
        {
            var authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.Cookie.SecurePolicy = Environment.IsProduction() ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.Path = null;
                options.LoginPath = "/Api/Auth/Login";
                options.AccessDeniedPath = "/Api/Auth/AccessDenied";
            });

            if (!string.IsNullOrEmpty(ApplicationSettings.GitHubClientId))
                authBuilder = authBuilder.AddOAuth("GitHub", options =>
                {
                    options.ClientId = ApplicationSettings.GitHubClientId;
                    options.ClientSecret = ApplicationSettings.GitHubClientSecret;
                    options.CallbackPath = new PathString("/signin-github");

                    // Include the users email address in the scope
                    options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize?scope=user:email";
                    options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                    options.UserInformationEndpoint = "https://api.github.com/user";

                    options.ClaimActions.MapJsonKey(ClaimTypes.PrimarySid, "accountId");

                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            JObject user = await RequestUserDetailsFromProvider(context);

                            var accountRepository = context.HttpContext.RequestServices.GetRequiredService<IAccountRepository>();

                            var gitHubId = (string)user["id"];
                            var name = (string)user["name"];
                            var email = (string)user["email"];
                            var avatarUrl = (string)user["avatar_url"];

                            var account = await accountRepository.AddOrGetAsync(name, email, avatarUrl, gitHubId: gitHubId);

                            user["accountId"] = account.Id.ToString();
                            context.RunClaimActions(user);
                        }
                    };
                });


            if (!string.IsNullOrEmpty(ApplicationSettings.GoogleClientId))
                authBuilder = authBuilder.AddGoogle(options =>
                {
                    options.ClientId = ApplicationSettings.GoogleClientId;
                    options.ClientSecret = ApplicationSettings.GoogleClientSecret;

                    options.ClaimActions.MapJsonKey(ClaimTypes.PrimarySid, "accountId");

                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            JObject user = await RequestUserDetailsFromProvider(context);

                            var accountRepository = context.HttpContext.RequestServices.GetRequiredService<IAccountRepository>();

                            var googleId = (string)user["id"];
                            var name = (string)user["displayName"];
                            var imageObject = (JObject)user["image"];
                            string avatarUrl = null;
                            if (imageObject != null)
                                avatarUrl = (string)imageObject["url"];
                            var emailsObject = (JArray)user["emails"];
                            string email = null;
                            if (emailsObject != null && emailsObject.Any())
                                email = (string)emailsObject.First()["value"];

                            var account = await accountRepository.AddOrGetAsync(name, email, avatarUrl, googleId: googleId);

                            user["accountId"] = account.Id.ToString();
                            context.RunClaimActions(user);
                        }
                    };
                });

            if (!string.IsNullOrEmpty(ApplicationSettings.MicrosoftApplicationId))
                authBuilder = authBuilder.AddMicrosoftAccount(options =>
                {
                    options.ClientId = ApplicationSettings.MicrosoftApplicationId;
                    options.ClientSecret = ApplicationSettings.MicrosoftPassword;

                    options.ClaimActions.MapJsonKey(ClaimTypes.PrimarySid, "accountId");

                    options.Events = new OAuthEvents
                    {
                        OnCreatingTicket = async context =>
                        {
                            JObject user = await RequestUserDetailsFromProvider(context);

                            var accountRepository = context.HttpContext.RequestServices.GetRequiredService<IAccountRepository>();

                            var microsoftId = (string)user["id"];
                            var name = (string)user["displayName"];
                            var email = (string)user["userPrincipalName"];

                            // userPrincipalName might not be an email, depending on the account type
                            if (!email.Contains("@"))
                                email = null;

                            var account = await accountRepository.AddOrGetAsync(name, email, null, microsoftId: microsoftId);

                            user["accountId"] = account.Id.ToString();
                            context.RunClaimActions(user);
                        }
                    };
                });
        }

        private static async Task<JObject> RequestUserDetailsFromProvider(OAuthCreatingTicketContext context)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

            var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            var user = JObject.Parse(await response.Content.ReadAsStringAsync());
            return user;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHsts(hsts => hsts.MaxAge(days: 365).IncludeSubdomains().Preload());
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());
            app.UseCsp(opts => opts
                .DefaultSources(s => s.Self())
                .BlockAllMixedContent()
            );
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXfo(xfo => xfo.Deny());

            if (!string.IsNullOrWhiteSpace(ApplicationSettings.ClientAppRootUrl))
                app.UseCors(builder =>
                    builder.WithOrigins(ApplicationSettings.ClientAppRootUrl)
                            .AllowCredentials()
                            .AllowAnyHeader()
                            .AllowAnyMethod());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(builder =>
                {
                    builder.Run(async context =>
                    {
                        context.Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                        context.Response.ContentType = "application/json";

                        var error = context.Features.Get<IExceptionHandlerFeature>();
                        if (error != null)
                        {
                            var validation = new BaseValidationViewModel();
                            validation.Errors = new[] { error.Error.Message };
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(validation)).ConfigureAwait(false);
                        }
                    });
                });
            }

            app.UseAuthentication();

            app.UseMvc();
        }

        public static void ApplyMigrations(IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                context.Database.Migrate(); // apply all migrations
            }

        }
    }
}
