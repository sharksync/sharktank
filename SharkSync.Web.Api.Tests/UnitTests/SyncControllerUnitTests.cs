using SharkSync.Web.Api.Controllers;
using SharkSync.Web.Api.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SharkSync.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace SharkSync.Web.Api.Tests.UnitTests
{
    [TestFixture]
    public class SyncControllerUnitTests
    {
        Mock<IApplication> app;
        Mock<IChange> change;
        Mock<IChange> change2;
        List<IChange> changes;

        Mock<IApplicationRepository> applicationRepository;
        Mock<IChangeRepository> changeRepository;
        Mock<ITimeService> timeService;

        Mock<ILogger<SyncController>> logger;

        [SetUp]
        public void SetUp()
        {
            app = new Mock<IApplication>();
            app.Setup(a => a.Id).Returns(Guid.NewGuid());
            app.Setup(a => a.AccessKey).Returns(Guid.NewGuid());

            change = new Mock<IChange>();
            change2 = new Mock<IChange>();
            changes = new List<IChange>();

            timeService = new Mock<ITimeService>();
            timeService.Setup(x => x.GetUtcNow()).Returns(DateTime.UtcNow);

            applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(app.Object);

            changeRepository = new Mock<IChangeRepository>();
            changeRepository.Setup(x => x.CreateChange(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<string>()))
                .Returns(delegate (Guid accountId, Guid appId, Guid recordId, string group, string entity, string property, long modified, string value)
                {
                    if (change.Object.RecordId == Guid.Empty)
                    {
                        change.Setup(c => c.RecordId).Returns(recordId);
                        change.Setup(c => c.GroupId).Returns(group);
                        change.Setup(c => c.Entity).Returns(entity);
                        change.Setup(c => c.Property).Returns(property);
                        change.Setup(c => c.RecordValue).Returns(value);
                        change.Setup(c => c.ClientModified).Returns(modified);
                        return change.Object;
                    }
                    else
                    {
                        change2.Setup(c => c.RecordId).Returns(recordId);
                        change2.Setup(c => c.GroupId).Returns(group);
                        change2.Setup(c => c.Entity).Returns(entity);
                        change2.Setup(c => c.Property).Returns(property);
                        change2.Setup(c => c.RecordValue).Returns(value);
                        change2.Setup(c => c.ClientModified).Returns(modified);
                        return change2.Object;
                    }
                });

            changeRepository.Setup(x => x.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>())).ReturnsAsync(changes);

            logger = new Mock<ILogger<SyncController>>();
        }

        private SyncController CreateSyncController()
        {
            return new SyncController(logger.Object, applicationRepository.Object, changeRepository.Object, timeService.Object);
        }


        [Test]
        public async Task SyncController_Post_Fail_Empty_Request()
        {
            SyncController controller = CreateSyncController();
            var response = await controller.Post(null) as JsonResult;

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("AppId missing or invalid request", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Fail_Missing_AppId()
        {
            SyncController controller = CreateSyncController();
            var response = await controller.Post(new SyncRequestViewModel()
            {
            }) as JsonResult;

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("AppId missing or invalid request", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Fail_Incorrect_AppId()
        {
            applicationRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((IApplication)null);

            SyncController controller = CreateSyncController();
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = Guid.NewGuid()
            }) as JsonResult;

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("No application found for AppId", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Fail_Missing_AppApiAccessKey()
        {
            SyncController controller = CreateSyncController();
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id
            }) as JsonResult;

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("AppApiAccessKey incorrect for AppId", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Fail_Invalid_AppApiAccessKey()
        {
            SyncController controller = CreateSyncController();
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = Guid.NewGuid()
            }) as JsonResult;

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("AppApiAccessKey incorrect for AppId", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Success_Basic_NoChanges_NoGroups()
        {
            SyncController controller = CreateSyncController();
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey
            }) as JsonResult;

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Success_Send_Single_Change()
        {
            string propertyName = "name";
            string group = "group";
            string entity = "person";
            Guid recordId = Guid.NewGuid();
            long modifiedMillisecondsAgo = 10000;
            string value = "Neil";
            List<IChange> returnChanges = null;

            DateTime requestStart = DateTime.UtcNow;
            timeService = new Mock<ITimeService>();
            timeService.Setup(x => x.GetUtcNow()).Returns(requestStart);

            changeRepository
                .Setup(x => x.UpsertChangesAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<IChange>>()))
                .Returns(() => Task.FromResult((string)null))
                .Callback<Guid, IEnumerable<IChange>>((a, l) =>
                {
                    returnChanges = l.ToList();
                });

            var request = new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey,
                Changes = new List<SyncRequestViewModel.ChangeViewModel>
                {
                    new SyncRequestViewModel.ChangeViewModel
                    {
                        Group = group,
                        RecordId = recordId,
                        Entity = entity,
                        Property = propertyName,
                        MillisecondsAgo = modifiedMillisecondsAgo,
                        Value = value
                    }
                }
            };

            SyncController controller = CreateSyncController();
            var response = await controller.Post(request) as JsonResult;
            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);
            Assert.NotNull(syncResponse.Groups);
            Assert.AreEqual(0, syncResponse.Groups.Count);

            Assert.NotNull(returnChanges);
            Assert.AreEqual(1, returnChanges.Count);
            Assert.AreEqual(group, returnChanges[0].GroupId);
            Assert.AreEqual(propertyName, returnChanges[0].Property);
            Assert.AreEqual(entity, returnChanges[0].Entity);
            Assert.AreEqual(recordId, returnChanges[0].RecordId);
            Assert.AreEqual(value, returnChanges[0].RecordValue);

            var millisecondsSinceEpoch = new DateTimeOffset(requestStart.AddMilliseconds(-modifiedMillisecondsAgo)).ToUnixTimeMilliseconds();

            IEnumerable<IChange> changes = new List<IChange>() { change.Object };

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, changes), Times.Once);
            changeRepository.Verify(t => t.CreateChange(app.Object.AccountId, app.Object.Id, recordId, group, entity, propertyName, millisecondsSinceEpoch, value), Times.Once);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Success_Send_Two_Changes()
        {
            string propertyName = "name";
            string propertyName2 = "age";
            string group = "group";
            string entity = "person";
            Guid recordId = Guid.NewGuid();
            long modifiedMillisecondsAgo = 10000;
            string value = "Neil";
            string value2 = "10";
            List<IChange> returnChanges = null;

            DateTime requestStart = DateTime.UtcNow;
            timeService = new Mock<ITimeService>();
            timeService.Setup(x => x.GetUtcNow()).Returns(requestStart);

            changeRepository
                .Setup(x => x.UpsertChangesAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<IChange>>()))
                .Returns(() => Task.FromResult((string)null))
                .Callback<Guid, IEnumerable<IChange>>((a, l) =>
                {
                    returnChanges = l.ToList();
                });

            var request = new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey,
                Changes = new List<SyncRequestViewModel.ChangeViewModel>
                {
                    new SyncRequestViewModel.ChangeViewModel
                    {
                        Group = group,
                        RecordId = recordId,
                        Entity = entity,
                        Property = propertyName,
                        MillisecondsAgo = modifiedMillisecondsAgo,
                        Value = value
                    },
                    new SyncRequestViewModel.ChangeViewModel
                    {
                        Group = group,
                        RecordId = recordId,
                        Entity = entity,
                        Property = propertyName2,
                        MillisecondsAgo = modifiedMillisecondsAgo,
                        Value = value2
                    }
                }
            };

            SyncController controller = CreateSyncController();
            var response = await controller.Post(request) as JsonResult;
            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);
            Assert.NotNull(syncResponse.Groups);
            Assert.AreEqual(0, syncResponse.Groups.Count);

            Assert.NotNull(returnChanges);
            Assert.AreEqual(2, returnChanges.Count);
            Assert.AreEqual(group, returnChanges[0].GroupId);
            Assert.AreEqual(propertyName, returnChanges[0].Property);
            Assert.AreEqual(entity, returnChanges[0].Entity);
            Assert.AreEqual(recordId, returnChanges[0].RecordId);
            Assert.AreEqual(value, returnChanges[0].RecordValue);
            Assert.AreEqual(group, returnChanges[1].GroupId);
            Assert.AreEqual(propertyName2, returnChanges[1].Property);
            Assert.AreEqual(entity, returnChanges[1].Entity);
            Assert.AreEqual(recordId, returnChanges[1].RecordId);
            Assert.AreEqual(value2, returnChanges[1].RecordValue);

            var millisecondsSinceEpoch = new DateTimeOffset(requestStart.AddMilliseconds(-modifiedMillisecondsAgo)).ToUnixTimeMilliseconds();

            IEnumerable<IChange> changes = new List<IChange>() { change.Object, change2.Object };

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, changes), Times.Once);
            changeRepository.Verify(t => t.CreateChange(app.Object.AccountId, app.Object.Id, recordId, group, entity, propertyName, millisecondsSinceEpoch, value), Times.Once);
            changeRepository.Verify(t => t.CreateChange(app.Object.AccountId, app.Object.Id, recordId, group, entity, propertyName2, millisecondsSinceEpoch, value2), Times.Once);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Success_Send_Two_Changes_FilterOutChangesForTheSameRecordEntityAndProperty()
        {
            string propertyName = "name";
            string group = "group";
            string entity = "person";
            Guid recordId = Guid.NewGuid();
            long modifiedMillisecondsAgo = 10000;
            long modifiedMillisecondsAgo2 = 20000;
            string value = "Neil";
            string value2 = "Adrian";
            List<IChange> returnChanges = null;

            DateTime requestStart = DateTime.UtcNow;
            timeService = new Mock<ITimeService>();
            timeService.Setup(x => x.GetUtcNow()).Returns(requestStart);

            changeRepository
                .Setup(x => x.UpsertChangesAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<IChange>>()))
                .Returns(() => Task.FromResult((string)null))
                .Callback<Guid, IEnumerable<IChange>>((a, l) =>
                {
                    returnChanges = l.ToList();
                });

            var request = new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey,
                Changes = new List<SyncRequestViewModel.ChangeViewModel>
                {
                    new SyncRequestViewModel.ChangeViewModel
                    {
                        Group = group,
                        RecordId = recordId,
                        Entity = entity,
                        Property = propertyName,
                        MillisecondsAgo = modifiedMillisecondsAgo,
                        Value = value
                    },
                    new SyncRequestViewModel.ChangeViewModel
                    {
                        Group = group,
                        RecordId = recordId,
                        Entity = entity,
                        Property = propertyName,
                        MillisecondsAgo = modifiedMillisecondsAgo2,
                        Value = value2
                    }
                }
            };

            SyncController controller = CreateSyncController();
            var response = await controller.Post(request) as JsonResult;
            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);
            Assert.NotNull(syncResponse.Groups);
            Assert.AreEqual(0, syncResponse.Groups.Count);

            Assert.NotNull(returnChanges);
            Assert.AreEqual(1, returnChanges.Count);
            Assert.AreEqual(group, returnChanges[0].GroupId);
            Assert.AreEqual(propertyName, returnChanges[0].Property);
            Assert.AreEqual(entity, returnChanges[0].Entity);
            Assert.AreEqual(recordId, returnChanges[0].RecordId);
            Assert.AreEqual(value, returnChanges[0].RecordValue);

            var millisecondsSinceEpoch = new DateTimeOffset(requestStart.AddMilliseconds(-modifiedMillisecondsAgo)).ToUnixTimeMilliseconds();

            IEnumerable<IChange> changes = new List<IChange>() { change.Object };

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, changes), Times.Once);
            changeRepository.Verify(t => t.CreateChange(app.Object.AccountId, app.Object.Id, recordId, group, entity, propertyName, millisecondsSinceEpoch, value), Times.Once);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Success_Null_Tidemark_With_No_Changes()
        {
            Guid appId = Guid.Empty;
            string listReturnGroup = null;
            string listReturnTidemark = null;

            long? tidemark = null;
            string group = "group";

            changeRepository
                .Setup(x => x.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>()))
                .ReturnsAsync(changes)
                .Callback<Guid, string, string>((a, g, t) =>
                {
                    appId = a;
                    listReturnGroup = g;
                    listReturnTidemark = t;
                });

            SyncController controller = CreateSyncController();
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey,
                Groups = new List<SyncRequestViewModel.GroupViewModel>
                {
                    new SyncRequestViewModel.GroupViewModel
                    {
                        Group = group,
                        Tidemark = tidemark
                    }
                }
            }) as JsonResult;

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);

            Assert.AreEqual(app.Object.Id, appId);
            Assert.AreEqual(group, listReturnGroup);
            Assert.AreEqual(tidemark, listReturnTidemark);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Once);
        }

        [Test]
        public async Task SyncController_Post_Success_Tidemark_With_Single_Group()
        {
            var changeObject = change.Object;
            changes.Add(changeObject);

            SyncController controller = CreateSyncController();
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey,
                Groups = new List<SyncRequestViewModel.GroupViewModel>
                {
                    new SyncRequestViewModel.GroupViewModel
                    {
                        Group = changeObject.GroupId,
                        Tidemark = changeObject.Id
                    }
                }
            }) as JsonResult;

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);

            Assert.NotNull(syncResponse.Groups);
            Assert.AreEqual(1, syncResponse.Groups.Count);
            Assert.AreEqual(changeObject.Id, syncResponse.Groups[0].Tidemark);
            Assert.AreEqual(changeObject.GroupId, syncResponse.Groups[0].Group);

            Assert.NotNull(syncResponse.Groups[0].Changes);
            Assert.AreEqual(1, syncResponse.Groups[0].Changes.Count);

            Assert.AreEqual(changeObject.ClientModified, syncResponse.Groups[0].Changes[0].Modified);
            Assert.AreEqual(changeObject.RecordId, syncResponse.Groups[0].Changes[0].RecordId);
            Assert.AreEqual(changeObject.Entity, syncResponse.Groups[0].Changes[0].Entity);
            Assert.AreEqual(changeObject.Property, syncResponse.Groups[0].Changes[0].Property);
            Assert.AreEqual(changeObject.RecordValue, syncResponse.Groups[0].Changes[0].Value);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Once);
        }

        [Test]
        public async Task SyncController_Post_Success_Tidemark_With_Single_Group2()
        {
            long tidemark = DateTime.UtcNow.Ticks;
            string group = "group";

            var changeObject = change.Object;
            change.Setup(c => c.Id).Returns(tidemark);
            changes.Add(changeObject);

            var change2 = new Mock<IChange>();
            change2.Setup(c => c.Id).Returns(tidemark);
            var changeObject2 = change2.Object;
            changes.Add(changeObject2);

            SyncController controller = CreateSyncController();
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey,
                Groups = new List<SyncRequestViewModel.GroupViewModel>
                {
                    new SyncRequestViewModel.GroupViewModel
                    {
                        Group = group,
                        Tidemark = tidemark
                    }
                }
            }) as JsonResult;

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

            Assert.AreEqual(changeObject.ClientModified, syncResponse.Groups[0].Changes[0].Modified);
            Assert.AreEqual(changeObject.RecordId, syncResponse.Groups[0].Changes[0].RecordId);
            Assert.AreEqual(changeObject.Entity, syncResponse.Groups[0].Changes[0].Entity);
            Assert.AreEqual(changeObject.Property, syncResponse.Groups[0].Changes[0].Property);
            Assert.AreEqual(changeObject.RecordValue, syncResponse.Groups[0].Changes[0].Value);

            Assert.AreEqual(changeObject2.ClientModified, syncResponse.Groups[0].Changes[1].Modified);
            Assert.AreEqual(changeObject2.RecordId, syncResponse.Groups[0].Changes[1].RecordId);
            Assert.AreEqual(changeObject2.Entity, syncResponse.Groups[0].Changes[1].Entity);
            Assert.AreEqual(changeObject2.Property, syncResponse.Groups[0].Changes[1].Property);
            Assert.AreEqual(changeObject2.RecordValue, syncResponse.Groups[0].Changes[1].Value);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Once);
        }

        [Test]
        public async Task SyncController_Post_Success_Tidemark_With_Two_Groups()
        {
            long tidemark = DateTime.UtcNow.Ticks;
            string group = "group";
            string group2 = "group2";

            var changeObject = change.Object;
            change.Setup(c => c.Id).Returns(tidemark);

            var change2 = new Mock<IChange>();
            change2.Setup(c => c.Id).Returns(tidemark);
            change2.Setup(c => c.GroupId).Returns(group2);
            var changeObject2 = change2.Object;

            changeRepository.Setup(x => x.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>()))
                .ReturnsAsync(delegate (Guid appId, string g, long? t)
                {
                    if (g == group)
                        return new List<IChange>() { changeObject };
                    else
                        return new List<IChange>() { changeObject2 };
                });

            SyncController controller = CreateSyncController();
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey,
                Groups = new List<SyncRequestViewModel.GroupViewModel>
                {
                    new SyncRequestViewModel.GroupViewModel
                    {
                        Group = group,
                        Tidemark = tidemark
                    },
                    new SyncRequestViewModel.GroupViewModel
                    {
                        Group = group2,
                        Tidemark = null
                    }
                }
            }) as JsonResult;

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);

            Assert.NotNull(syncResponse.Groups);
            Assert.AreEqual(2, syncResponse.Groups.Count);
            Assert.AreEqual(tidemark, syncResponse.Groups[0].Tidemark);
            Assert.AreEqual(group, syncResponse.Groups[0].Group);
            Assert.NotNull(syncResponse.Groups[0].Changes);
            Assert.AreEqual(1, syncResponse.Groups[0].Changes.Count);

            Assert.AreEqual(tidemark, syncResponse.Groups[1].Tidemark);
            Assert.AreEqual(group2, syncResponse.Groups[1].Group);
            Assert.NotNull(syncResponse.Groups[1].Changes);
            Assert.AreEqual(1, syncResponse.Groups[1].Changes.Count);

            Assert.AreEqual(changeObject.ClientModified, syncResponse.Groups[0].Changes[0].Modified);
            Assert.AreEqual(changeObject.RecordId, syncResponse.Groups[0].Changes[0].RecordId);
            Assert.AreEqual(changeObject.Entity, syncResponse.Groups[0].Changes[0].Entity);
            Assert.AreEqual(changeObject.Property, syncResponse.Groups[0].Changes[0].Property);
            Assert.AreEqual(changeObject.RecordValue, syncResponse.Groups[0].Changes[0].Value);

            Assert.AreEqual(changeObject2.ClientModified, syncResponse.Groups[1].Changes[0].Modified);
            Assert.AreEqual(changeObject2.RecordId, syncResponse.Groups[1].Changes[0].RecordId);
            Assert.AreEqual(changeObject2.Entity, syncResponse.Groups[1].Changes[0].Entity);
            Assert.AreEqual(changeObject2.Property, syncResponse.Groups[1].Changes[0].Property);
            Assert.AreEqual(changeObject2.RecordValue, syncResponse.Groups[1].Changes[0].Value);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<long?>()), Times.Exactly(2));
        }
    }
}
