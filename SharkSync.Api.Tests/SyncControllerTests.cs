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
        List<Change> changes;

        Mock<IScaleContext> scaleContext;
        Mock<IQueryCache> queryCache;
        Mock<ILogger<SyncController>> logger;

        [SetUp]
        public void SetUp()
        {
            app = new Application { Id = Guid.NewGuid(), AccessKey = Guid.NewGuid() };
            device = new Device { Id = Guid.NewGuid(), AppId = app.Id, LastSeen = DateTime.UtcNow.ToString() };
            changes = new List<Change>();

            scaleContext = new Mock<IScaleContext>();
            scaleContext.Setup(x => x.Query<Change>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<object>>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>())).ReturnsAsync(changes);

            queryCache = new Mock<IQueryCache>();
            queryCache.Setup(x => x.GetByPrimaryKeyFromCacheOrQuery<Application>(It.IsAny<string>(), "application", It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<TimeSpan>())).ReturnsAsync(app);
            queryCache.Setup(x => x.GetByPrimaryKeyFromCacheOrQuery<Device>(It.IsAny<string>(), "device", It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<TimeSpan>())).ReturnsAsync(device);

            logger = new Mock<ILogger<SyncController>>();
        }

        [Test]
        public async Task SyncController_Post_Fail_Empty_Request()
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
        public async Task SyncController_Post_Fail_Missing_AppId()
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
        public async Task SyncController_Post_Fail_Incorrect_AppId()
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
        public async Task SyncController_Post_Fail_Missing_AppApiAccessKey()
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
        public async Task SyncController_Post_Fail_Invalid_AppApiAccessKey()
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
        public async Task SyncController_Post_Fail_Missing_DeviceId()
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
        public async Task SyncController_Post_Fail_Invalid_DeviceId()
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
        public async Task SyncController_Post_Success_Basic_NoChanges_NoGroups()
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
        public async Task SyncController_Post_Fail_Basic_Badly_Formatted_Path()
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
                        Group = "Group",
                        Path = "bad format"
                    }
                }
            };
            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(request);

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("Path is incorrectly formatted, should be formatted <guid>/property.name", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);
        }

        [Test]
        public async Task SyncController_Post_Success_Single_Change()
        {
            string partition = null;
            string table = null;
            Change changeValue = null;
            string propertyName = "name";
            string group = "group";
            Guid recordId = Guid.NewGuid();
            int modifiedSecondsAgo = 10;
            string value = "Neil";
            List<SendContextModel<UpsetModel<Change>>> changes = null;
            var model = new SendContextModel<UpsetModel<Change>>();

            scaleContext
                .Setup(x => x.MakeUpsertModel(It.IsAny<string>(), "change", It.IsAny<Change>()))
                .Returns(model)
                .Callback<string, string, Change>((p, t, v) =>
                {
                    partition = p;
                    table = t;
                    changeValue = v;
                });

            scaleContext
                .Setup(x => x.UpsertBulk(It.IsAny<List<SendContextModel<UpsetModel<Change>>>>()))
                .Returns(() => Task.FromResult((string)null))
                .Callback<List<SendContextModel<UpsetModel<Change>>>>((l) =>
                {
                    changes = l;
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
                        Group = group,
                        Path = $"{recordId}/{propertyName}",
                        SecondsAgo = modifiedSecondsAgo,
                        Value = value
                    }
                }
            };

            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(request);
            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);
            Assert.AreEqual($"{app.Id}-{group}", partition);
            Assert.AreEqual($"change", table);
            Assert.NotNull(changeValue);
            Assert.AreEqual(recordId, changeValue.RecordId);
            Assert.AreEqual(propertyName, changeValue.Path);
            Assert.AreEqual(device.Id, changeValue.DeviceId);
            Assert.AreEqual(DateTime.Now.AddSeconds(-modifiedSecondsAgo).ToLongTimeString(), changeValue.Modified.ToLongTimeString());
            Assert.AreEqual("%clustertime%", changeValue.Tidemark);
            Assert.AreEqual(value, changeValue.Value);

            Assert.NotNull(changes);
            Assert.AreEqual(1, changes.Count);
            Assert.AreEqual(model, changes[0]);

            scaleContext.Verify(t => t.MakeUpsertModel(It.IsAny<string>(), "change", It.IsAny<Change>()), Times.Once);
            scaleContext.Verify(t => t.UpsertBulk(It.IsAny<List<SendContextModel<UpsetModel<Change>>>>()), Times.Once);
        }

        [Test]
        public async Task SyncController_Post_Success_Two_Changes()
        {
            string partition = null;
            string table = null;
            Change changeValue = null;
            Change changeValue2 = null;
            string propertyName = "name";
            string propertyName2 = "age";
            string group = "group";
            Guid recordId = Guid.NewGuid();
            int modifiedSecondsAgo = 10;
            string value = "Neil";
            string value2 = "10";

            List<SendContextModel<UpsetModel<Change>>> changes = null;
            var model = new SendContextModel<UpsetModel<Change>>();

            scaleContext
                .Setup(x => x.MakeUpsertModel(It.IsAny<string>(), "change", It.IsAny<Change>()))
                .Returns(model)
                .Callback<string, string, Change>((p, t, v) =>
                {
                    partition = p;
                    table = t;
                    if (changeValue == null)
                        changeValue = v;
                    else
                        changeValue2 = v;
                });

            scaleContext
                .Setup(x => x.UpsertBulk(It.IsAny<List<SendContextModel<UpsetModel<Change>>>>()))
                .Returns(() => Task.FromResult((string)null))
                .Callback<List<SendContextModel<UpsetModel<Change>>>>((l) =>
                {
                    changes = l;
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
                        Group = group,
                        Path = $"{recordId}/{propertyName}",
                        SecondsAgo = modifiedSecondsAgo,
                        Value = value
                    },
                    new SyncRequestViewModel.ChangeViewModel
                    {
                        Group = group,
                        Path = $"{recordId}/{propertyName2}",
                        SecondsAgo = modifiedSecondsAgo,
                        Value = value2
                    }
                }
            };

            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(request);
            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);
            Assert.AreEqual($"{app.Id}-{group}", partition);
            Assert.AreEqual($"change", table);

            Assert.NotNull(changeValue);
            Assert.AreEqual(recordId, changeValue.RecordId);
            Assert.AreEqual(propertyName, changeValue.Path);
            Assert.AreEqual(device.Id, changeValue.DeviceId);
            Assert.AreEqual(DateTime.Now.AddSeconds(-modifiedSecondsAgo).ToLongTimeString(), changeValue.Modified.ToLongTimeString());
            Assert.AreEqual("%clustertime%", changeValue.Tidemark);
            Assert.AreEqual(value, changeValue.Value);

            Assert.NotNull(changeValue2);
            Assert.AreEqual(recordId, changeValue2.RecordId);
            Assert.AreEqual(propertyName2, changeValue2.Path);
            Assert.AreEqual(device.Id, changeValue2.DeviceId);
            Assert.AreEqual(DateTime.Now.AddSeconds(-modifiedSecondsAgo).ToLongTimeString(), changeValue2.Modified.ToLongTimeString());
            Assert.AreEqual("%clustertime%", changeValue2.Tidemark);
            Assert.AreEqual(value2, changeValue2.Value);

            Assert.NotNull(changes);
            Assert.AreEqual(2, changes.Count);
            Assert.AreEqual(model, changes[0]);

            scaleContext.Verify(t => t.MakeUpsertModel(It.IsAny<string>(), "change", It.IsAny<Change>()), Times.Exactly(2));
            scaleContext.Verify(t => t.UpsertBulk(It.IsAny<List<SendContextModel<UpsetModel<Change>>>>()), Times.Once);
        }

        [Test]
        public async Task SyncController_Post_Success_Null_Tidemark_With_No_Changes()
        {
            string group = "group";
            string partition = null;
            string table = null;
            string whereClause = null;
            List<object> whereClauseValues = null;
            string orderBy = null;
            int? limit = null;
            int? offset = null;

            scaleContext
                .Setup(x => x.Query<Change>(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<List<object>>(), It.IsAny<string>(), It.IsAny<int?>(), It.IsAny<int?>()))
                .ReturnsAsync(changes)
                .Callback<string, string, string, List<object>, string, int?, int?>((p, t, w, v, o, l, of) =>
                {
                    partition = p;
                    table = t;
                    whereClause = w;
                    whereClauseValues = v;
                    orderBy = o;
                    limit = l;
                    offset = of;
                });

            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Id,
                AppApiAccessKey = app.AccessKey,
                DeviceId = device.Id,
                Groups = new List<SyncRequestViewModel.GroupViewModel>
                {
                    new SyncRequestViewModel.GroupViewModel
                    {
                        Group = group,
                        Tidemark = null
                    }
                }
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);

            Assert.AreEqual($"{app.Id}-{group}", partition);
            Assert.AreEqual($"change", table);
            Assert.AreEqual(null, whereClause);

            Assert.NotNull(whereClauseValues);
            Assert.AreEqual(0, whereClauseValues.Count);
            Assert.AreEqual("tidemark", orderBy);
            Assert.AreEqual(50, limit);
            Assert.AreEqual(null, offset);
        }

        [Test]
        public async Task SyncController_Post_Success_Tidemark_With_Single_Change()
        {
            string group = "group";
            string tidemark = "tidemark";
            Change change = new Change() { Id = Guid.NewGuid(), Tidemark = tidemark, Modified = DateTime.Now, Path = "name", DeviceId = device.Id, RecordId = Guid.NewGuid(), Value = "Neil" };

            changes.Add(change);

            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Id,
                AppApiAccessKey = app.AccessKey,
                DeviceId = device.Id,
                Groups = new List<SyncRequestViewModel.GroupViewModel>
                {
                    new SyncRequestViewModel.GroupViewModel
                    {
                        Group = group,
                        Tidemark = tidemark
                    }
                }
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);

            Assert.NotNull(syncResponse.Groups);
            Assert.AreEqual(1, syncResponse.Groups.Count);
            Assert.AreEqual(tidemark, syncResponse.Groups[0].Tidemark);
            Assert.AreEqual(group, syncResponse.Groups[0].Group);

            Assert.NotNull(syncResponse.Groups[0].Changes);
            Assert.AreEqual(1, syncResponse.Groups[0].Changes.Count);

            Assert.AreEqual(change.Modified, syncResponse.Groups[0].Changes[0].Modified);
            Assert.AreEqual(change.Path, syncResponse.Groups[0].Changes[0].Path);
            Assert.AreEqual(change.Value, syncResponse.Groups[0].Changes[0].Value);
        }

        [Test]
        public async Task SyncController_Post_Success_Single_Group_With_Two_Changes()
        {
            string group = "group";
            string tidemark = "tidemark";
            Change change = new Change() { Id = Guid.NewGuid(), Tidemark = tidemark, Modified = DateTime.Now, Path = "name", DeviceId = device.Id, RecordId = Guid.NewGuid(), Value = "Neil" };
            Change change2 = new Change() { Id = Guid.NewGuid(), Tidemark = tidemark, Modified = DateTime.Now, Path = "age", DeviceId = device.Id, RecordId = Guid.NewGuid(), Value = "35" };

            changes.Add(change);
            changes.Add(change2);

            var controller = new SyncController(logger.Object, queryCache.Object, scaleContext.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Id,
                AppApiAccessKey = app.AccessKey,
                DeviceId = device.Id,
                Groups = new List<SyncRequestViewModel.GroupViewModel>
                {
                    new SyncRequestViewModel.GroupViewModel
                    {
                        Group = group,
                        Tidemark = tidemark
                    }
                }
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);

            Assert.NotNull(syncResponse.Groups);
            Assert.AreEqual(1, syncResponse.Groups.Count);
            Assert.AreEqual(tidemark, syncResponse.Groups[0].Tidemark);
            Assert.AreEqual(group, syncResponse.Groups[0].Group);

            Assert.NotNull(syncResponse.Groups[0].Changes);
            Assert.AreEqual(2, syncResponse.Groups[0].Changes.Count);

            Assert.AreEqual(change.Modified, syncResponse.Groups[0].Changes[0].Modified);
            Assert.AreEqual(change.Path, syncResponse.Groups[0].Changes[0].Path);
            Assert.AreEqual(change.Value, syncResponse.Groups[0].Changes[0].Value);

            Assert.AreEqual(change2.Modified, syncResponse.Groups[0].Changes[1].Modified);
            Assert.AreEqual(change2.Path, syncResponse.Groups[0].Changes[1].Path);
            Assert.AreEqual(change2.Value, syncResponse.Groups[0].Changes[1].Value);
        }
    }
}
