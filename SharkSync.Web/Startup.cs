using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SharkSync.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // This is only used locally, Cloudfront lambda@edge jobs deliver the real headers
            app.UseHsts(hsts => hsts.MaxAge(days: 365).IncludeSubdomains().Preload());
            app.UseXContentTypeOptions();
            app.UseReferrerPolicy(opts => opts.NoReferrer());
            app.UseCsp(opts => opts
                .DefaultSources(s => s.Self().CustomSources("data:"))
                .ScriptSources(s => s.Self())
                .ConnectSources(s => s.Self().CustomSources("https://localhost:44325"))
                .ImageSources(s => s.Self().CustomSources("data:", "https://localhost:44325"))
                .FontSources(s => s.Self().CustomSources("data:"))
                .StyleSources(s => s.Self().UnsafeInline())
                .BlockAllMixedContent()
            );
            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            app.UseXfo(xfo => xfo.Deny());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    HotModuleReplacement = true,
                    ReactHotModuleReplacement = true
                });
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "Home", action = "Index" });
            });
        }
    }
}