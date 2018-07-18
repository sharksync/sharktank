using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharkSync.PostgreSQL;

namespace SharkSync.Web.Api
{
    /// <summary>
    /// The Main function can be used to run the ASP.NET Core application locally using the Kestrel webserver.
    /// </summary>
    public class LocalEntryPoint
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args).Build();

            host.Run();
        }

        public static IWebHostBuilder BuildWebHost(string[] args)
            => WebHost.CreateDefaultBuilder(args)
                        .UseStartup<Startup>();
    }
}
