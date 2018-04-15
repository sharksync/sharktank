using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
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
using SharkTank.DynamoDB.Repositories;
using SharkTank.Interfaces.Repositories;

namespace SharkSync.Web.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public static IConfiguration Configuration { get; private set; }

        public static IHostingEnvironment Environment { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            var appSettings = new AppSettings();
            var appSettingSection = Configuration.GetSection(nameof(AppSettings));

            services.Configure<AppSettings>(appSettingSection);
            appSettingSection.Bind(appSettings);

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder
                        .WithOrigins(appSettings.ClientAppRootUrl)
                        .AllowCredentials()
                        .AllowAnyMethod());
            });
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.Add(new CorsAuthorizationFilterFactory("AllowSpecificOrigin"));
            });
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "";
            })
            .AddCookie(options =>
            {
                options.Cookie.SecurePolicy = Environment.IsProduction() ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest; 
            })
            .AddOAuth("GitHub", options =>
            {
                options.ClientId = Configuration["Authentication:GitHub:ClientId"];
                options.ClientSecret = Configuration["Authentication:GitHub:ClientSecret"];
                options.CallbackPath = new PathString("/signin-github");

                // Include the users email address in the scope
                options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize?scope=user:email";
                options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                options.UserInformationEndpoint = "https://api.github.com/user";

                options.SaveTokens = true;

                options.ClaimActions.MapJsonKey(ClaimTypes.PrimarySid, "accountId");

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                        var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();

                        var user = JObject.Parse(await response.Content.ReadAsStringAsync());

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
            })
            .AddGoogle(options =>
            {
                options.ClientId = Configuration["Authentication:Google:ClientId"];
                options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];

                options.ClaimActions.MapJsonKey(ClaimTypes.PrimarySid, "accountId");

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                        var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();

                        var user = JObject.Parse(await response.Content.ReadAsStringAsync());

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
            })
            .AddMicrosoftAccount(options =>
            {
                options.ClientId = Configuration["Authentication:Microsoft:ApplicationId"];
                options.ClientSecret = Configuration["Authentication:Microsoft:Password"];

                options.ClaimActions.MapJsonKey(ClaimTypes.PrimarySid, "accountId");

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                        var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();

                        var user = JObject.Parse(await response.Content.ReadAsStringAsync());

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

            services.AddTransient(typeof(IAccountRepository), typeof(AccountRepository));
            services.AddTransient(typeof(IApplicationRepository), typeof(ApplicationRepository));
            services.AddTransient(typeof(IDeviceRepository), typeof(DeviceRepository));
            services.AddTransient(typeof(IChangeRepository), typeof(ChangeRepository));

            services.AddScoped(typeof(AuthService));

            services.AddAWSService<IAmazonDynamoDB>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
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
    }
}
