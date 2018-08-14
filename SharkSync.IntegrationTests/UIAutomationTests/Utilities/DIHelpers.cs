using Amazon.SecretsManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharkSync.Interfaces;
using SharkSync.PostgreSQL;
using SharkSync.Services;
using System.IO;

namespace SharkSync.IntegrationTests
{
    public static class DIHelpers
    {
        public static ServiceProvider GetServiceProvider()
        {
            var services = new ServiceCollection();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false)
                .Build();
            services.Configure<AppSettings>(options => configuration.GetSection("AppSettings").Bind(options));

            services.AddDefaultAWSOptions(configuration.GetAWSOptions());

            services.AddAWSService<IAmazonSecretsManager>();
            services.AddSingleton<ISettingsService, SettingsService>();

            services.AddDbContext<DataContext>();

            return services.BuildServiceProvider();
        }
    }
}
