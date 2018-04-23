using Amazon.DynamoDBv2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using SharkSync.Api.Controllers;
using SharkSync.Api.ViewModels;
using SharkTank.DynamoDB.Repositories;
using SharkTank.Interfaces.Repositories;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharkTank.IntegrationTests
{
    [TestFixture]
    public class DynamoDbSyncTests
    {
        ServiceProvider Services { get; set; }

        IConfigurationRoot Configuration { get; set; }

        ILogger<SyncController> Logger { get; set; }

        IApplicationRepository AppRepository { get; set; }

        IDeviceRepository DeviceRepository { get; set; }

        IChangeRepository ChangeRepository { get; set; }

        [SetUp]
        public void SetUp()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", false, true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(new LoggerFactory());
            serviceCollection.AddLogging();

            serviceCollection.AddTransient(typeof(IApplicationRepository), typeof(ApplicationRepository));
            serviceCollection.AddTransient(typeof(IDeviceRepository), typeof(DeviceRepository));
            serviceCollection.AddTransient(typeof(IChangeRepository), typeof(ChangeRepository));

            serviceCollection.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            serviceCollection.AddAWSService<IAmazonDynamoDB>();

            Services = serviceCollection.BuildServiceProvider();

            Logger = Services.GetService<ILogger<SyncController>>();
            AppRepository = Services.GetService<IApplicationRepository>();
            DeviceRepository = Services.GetService<IDeviceRepository>();
            ChangeRepository = Services.GetService<IChangeRepository>();
        }

        [Test]
        public async Task DynamoDB_Sync_Integration_Empty_Request()
        {
            var controller = new SyncController(Logger, AppRepository, DeviceRepository, ChangeRepository);
            var response = await controller.Post(null);

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("app_id missing or invalid request", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);
        }

        [Test]
        public async Task SyncController_Post_Fail_Invalid_AppId()
        {
            var controller = new SyncController(Logger, AppRepository, DeviceRepository, ChangeRepository);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = Guid.NewGuid()
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("app_id missing or invalid request", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);
        }

    }
}
