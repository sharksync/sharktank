using SharkSync.Api.Controllers;
using SharkSync.Api.Scale.Tables;
using SharkSync.Api.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SharkSync.Api.Scale;

namespace SharkSync.Api.Tests.Controllers
{
    [TestFixture]
    public class SyncControllerTests
    {
        Application app;
        Device device;
        Mock<IScaleContext> scaleContext;
        Mock<IQueryCache> queryCache;
        Mock<ILogger<SyncController>> logger;

        [SetUp]
        public void SetUp()
        {
            app = new Application { Id = Guid.NewGuid(), AccessKey = Guid.NewGuid() };
            device = new Device { Id = Guid.NewGuid(), AppId = app.Id, LastSeen = DateTime.UtcNow.ToString() };

            scaleContext = new Mock<IScaleContext>();

            queryCache = new Mock<IQueryCache>();

            logger = new Mock<ILogger<SyncController>>();
        }

        [Test]
        public async Task Fail_Empty_Request()
        {
            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(null);

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("app_id missing or invalid request", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);
        }

        [Test]
        public async Task Fail_Missing_AppId()
        {
            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("app_id missing or invalid request", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);
        }

        [Test]
        public async Task Fail_Incorrect_AppId()
        {
            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = Guid.NewGuid()
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("No application found for app_id", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);
        }

        //[Test]
        //public async Task Fail_Missing_AppApiAccessKey()
        //{
        //    var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
        //    var response = await controller.Post(new SyncRequestViewModel()
        //    {
        //        AppId = app.Id
        //    });

        //    var syncResponse = response.Value as SyncResponseViewModel;

        //    Assert.NotNull(syncResponse);
        //    Assert.NotNull(syncResponse.Errors);
        //    Assert.AreEqual(1, syncResponse.Errors.Count());
        //    Assert.AreEqual("app_api_access_key incorrect for app_id", syncResponse.Errors.First());
        //    Assert.False(syncResponse.Success);
        //}

        //[Test]
        //public async Task Fail_Invalid_AppApiAccessKey()
        //{
        //    var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
        //    var response = await controller.Post(new SyncRequestViewModel()
        //    {
        //        AppId = app.Id,
        //        AppApiAccessKey = Guid.NewGuid()
        //    });

        //    Assert.IsType(typeof(SyncResponseViewModel), response.Value);

        //    var syncResponse = response.Value as SyncResponseViewModel;

        //    Assert.NotNull(syncResponse);
        //    Assert.NotNull(syncResponse.Errors);
        //    Assert.Equal(1, syncResponse.Errors.Count());
        //    Assert.Equal("app_api_access_key incorrect for app_id", syncResponse.Errors.First());
        //    Assert.False(syncResponse.Success);
        //}

        //[Test]
        //public async Task Fail_Missing_DeviceId()
        //{
        //    var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
        //    var response = await controller.Post(new SyncRequestViewModel()
        //    {
        //        AppId = app.Id,
        //        AppApiAccessKey = app.ApiAccessKey
        //    });

        //    Assert.IsType(typeof(SyncResponseViewModel), response.Value);

        //    var syncResponse = response.Value as SyncResponseViewModel;

        //    Assert.NotNull(syncResponse);
        //    Assert.NotNull(syncResponse.Errors);
        //    Assert.Equal(1, syncResponse.Errors.Count());
        //    Assert.Equal("No device found for device_id", syncResponse.Errors.First());
        //    Assert.False(syncResponse.Success);
        //}

        //[Test]
        //public async Task Fail_Invalid_DeviceId()
        //{
        //    var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
        //    var response = await controller.Post(new SyncRequestViewModel()
        //    {
        //        AppId = app.Id,
        //        AppApiAccessKey = app.ApiAccessKey,
        //        DeviceId = Guid.NewGuid()
        //    });

        //    Assert.IsType(typeof(SyncResponseViewModel), response.Value);

        //    var syncResponse = response.Value as SyncResponseViewModel;

        //    Assert.NotNull(syncResponse);
        //    Assert.NotNull(syncResponse.Errors);
        //    Assert.Equal(1, syncResponse.Errors.Count());
        //    Assert.Equal("No device found for device_id", syncResponse.Errors.First());
        //    Assert.False(syncResponse.Success);
        //}

        //[Test]
        //public async Task Success_Basic_NoChanges_NoGroups()
        //{
        //    var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
        //    var response = await controller.Post(new SyncRequestViewModel()
        //    {
        //        AppId = app.Id,
        //        AppApiAccessKey = app.ApiAccessKey,
        //        DeviceId = device.Id
        //    });

        //    Assert.IsType(typeof(SyncResponseViewModel), response.Value);

        //    var syncResponse = response.Value as SyncResponseViewModel;

        //    Assert.NotNull(syncResponse);
        //    Assert.Null(syncResponse.Errors);
        //    Assert.True(syncResponse.Success);
        //}

        //[Test]
        //public async Task Success_Basic_EmptyChange()
        //{
        //    var request = new SyncRequestViewModel()
        //    {
        //        AppId = app.Id,
        //        AppApiAccessKey = app.ApiAccessKey,
        //        DeviceId = device.Id,
        //        Changes = new List<SyncRequestViewModel.ChangeViewModel>
        //        {
        //            new SyncRequestViewModel.ChangeViewModel
        //            {

        //            }
        //        }
        //    };
        //    var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
        //    var response = await controller.Post(request);

        //    Assert.IsType(typeof(SyncResponseViewModel), response.Value);

        //    var syncResponse = response.Value as SyncResponseViewModel;

        //    Assert.NotNull(syncResponse);
        //    Assert.Null(syncResponse.Errors);
        //    Assert.True(syncResponse.Success);
        //}

        //[Test]
        //public async Task Success_Basic_Badly_Formatted_Path()
        //{
        //    var request = new SyncRequestViewModel()
        //    {
        //        AppId = app.Id,
        //        AppApiAccessKey = app.ApiAccessKey,
        //        DeviceId = device.Id,
        //        Changes = new List<SyncRequestViewModel.ChangeViewModel>
        //        {
        //            new SyncRequestViewModel.ChangeViewModel
        //            {
        //                Group = "Group",
        //                Path = "bad format"
        //            }
        //        }
        //    };
        //    var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
        //    var response = await controller.Post(request);

        //    Assert.IsType(typeof(SyncResponseViewModel), response.Value);

        //    var syncResponse = response.Value as SyncResponseViewModel;

        //    Assert.NotNull(syncResponse);
        //    Assert.Null(syncResponse.Errors);
        //    Assert.True(syncResponse.Success);

        //    dynamoDb.Verify(t => t.ExecuteBatchWriteAsync(It.IsAny<BatchWrite[]>(), It.IsAny<CancellationToken>()));
        //}
    }
}
