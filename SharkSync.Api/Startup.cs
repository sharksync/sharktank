using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharkTank.Repositories;
using SharkTank.Scale.Repositories;
using SharkTank.Scale.ScaleApi;

namespace SharkSync.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddMemoryCache();

            services.AddTransient(typeof(IApplicationRepository), typeof(ApplicationRepository));
            services.AddTransient(typeof(IDeviceRepository), typeof(DeviceRepository));
            services.AddTransient(typeof(IChangeRepository), typeof(ChangeRepository));

            services.AddTransient(typeof(ScaleContext), typeof(ScaleContext));
            services.AddTransient(typeof(QueryCache), typeof(QueryCache));
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
