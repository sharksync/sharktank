using SharkSync.Api.Controllers;
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
using SharkSync.Scale;
using SharkSync.Scale.Tables;

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
            queryCache.Setup(x => x.GetByPrimaryKeyFromCacheOrQuery<Application>(It.IsAny<string>(), "application", It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<TimeSpan>())).ReturnsAsync(app);
            queryCache.Setup(x => x.GetByPrimaryKeyFromCacheOrQuery<Device>(It.IsAny<string>(), "device", It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<TimeSpan>())).ReturnsAsync(device);

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
            queryCache.Setup(x => x.GetByPrimaryKeyFromCacheOrQuery<Application>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<TimeSpan>())).ReturnsAsync((Application)null);

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

        [Test]
        public async Task Fail_Missing_AppApiAccessKey()
        {
            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Id
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("app_api_access_key incorrect for app_id", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);
        }

        [Test]
        public async Task Fail_Invalid_AppApiAccessKey()
        {
            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Id,
                AppApiAccessKey = Guid.NewGuid()
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("app_api_access_key incorrect for app_id", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);
        }

        [Test]
        public async Task Fail_Missing_DeviceId()
        {
            queryCache.Setup(x => x.GetByPrimaryKeyFromCacheOrQuery<Device>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<TimeSpan>())).ReturnsAsync((Device)null);

            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Id,
                AppApiAccessKey = app.AccessKey
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("No device found for device_id", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);
        }

        [Test]
        public async Task Fail_Invalid_DeviceId()
        {
            queryCache.Setup(x => x.GetByPrimaryKeyFromCacheOrQuery<Device>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<TimeSpan>())).ReturnsAsync((Device)null);

            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Id,
                AppApiAccessKey = app.AccessKey,
                DeviceId = Guid.NewGuid()
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("No device found for device_id", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);
        }

        [Test]
        public async Task Success_Basic_NoChanges_NoGroups()
        {
            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Id,
                AppApiAccessKey = app.AccessKey,
                DeviceId = device.Id
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);
        }

        [Test]
        public async Task Success_Basic_EmptyChange()
        {
            var request = new SyncRequestViewModel()
            {
                AppId = app.Id,
                AppApiAccessKey = app.AccessKey,
                DeviceId = device.Id,
                Changes = new List<SyncRequestViewModel.ChangeViewModel>
                {
                    new SyncRequestViewModel.ChangeViewModel
                    {

                    }
                }
            };
            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(request);

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);
        }

        [Test]
        public async Task Success_Basic_Badly_Formatted_Path()
        {
            string partition = null;
            string table = null;
            Change value = null;

            scaleContext
                .Setup(x => x.MakeUpsertModel(It.IsAny<string>(), "change", It.IsAny<Change>()))
                .Callback<string, string, Change>((p, t, v) =>
                {
                    partition = p;
                    table = t;
                    value = v;
                });

            var request = new SyncRequestViewModel()
            {
                AppId = app.Id,
                AppApiAccessKey = app.AccessKey,
                DeviceId = device.Id,
                Changes = new List<SyncRequestViewModel.ChangeViewModel>
                {
                    new SyncRequestViewModel.ChangeViewModel
                    {
                        Group = "Group",
                        Path = "bad format"
                    }
                }
            };
            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(request);

            var syncResponse = response.Value as SyncResponseViewModel;


            //var dbChange = new Change()
            //{
            //    Id = Guid.NewGuid(),
            //    RecordId = recordId,
            //    Path = change.Path,
            //    DeviceId = device.Id,
            //    Modified = modifiedUTC,
            //    Tidemark = "%clustertime%",
            //    Value = change.Value
            //};


            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);
            Assert.AreEqual($"{app.Id}-Group", partition);
            Assert.AreEqual($"change", table);
            Assert.NotNull(value);

            //// Path should contain a / in format <guid>/property.name
            //if (!string.IsNullOrWhiteSpace(change.Path) && change.Path.IndexOf("/") > -1)
            //{
            //    recordId = Guid.Parse(change.Path.Substring(0, change.Path.IndexOf("/")));
            //    path = change.Path.Substring(change.Path.IndexOf("/") + 1);
            //}


            scaleContext.Verify(t => t.MakeUpsertModel(It.IsAny<string>(), "change", It.IsAny<Change>()));
        }
    }
}
