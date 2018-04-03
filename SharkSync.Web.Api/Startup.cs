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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static IConfiguration Configuration { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder
                        .WithOrigins("http://localhost:62377", "https://z923hkq2sg.execute-api.eu-west-1.amazonaws.com")
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
                options.DefaultChallengeScheme = "GitHub";
            })
            .AddCookie()
            .AddOAuth("GitHub", options =>
            {
                options.ClientId = Configuration["GitHub:ClientId"];
                options.ClientSecret = Configuration["GitHub:ClientSecret"];
                options.CallbackPath = new PathString("/signin-github");

                options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
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

                        var githubId = (int)user["id"];
                        var name = (string)user["name"];
                        var email = (string)user["email"];
                        var avatarUrl = (string)user["avatar_url"];

                        var account = await accountRepository.AddOrGetAsync(name, email, githubId, avatarUrl);

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
