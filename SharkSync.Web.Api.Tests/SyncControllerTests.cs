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
using SharkTank.Interfaces.Repositories;
using SharkTank.Interfaces.Entities;

namespace SharkSync.Web.Api.Tests.Controllers
{
    [TestFixture]
    public class SyncControllerTests
    {
        Mock<IApplication> app;
        Mock<IDevice> device;
        Mock<IChange> change;
        Mock<IChange> change2;
        List<IChange> changes;

        Mock<IApplicationRepository> applicationRepository;
        Mock<IDeviceRepository> deviceRepository;
        Mock<IChangeRepository> changeRepository;

        Mock<ILogger<SyncController>> logger;

        [SetUp]
        public void SetUp()
        {
            app = new Mock<IApplication>();
            app.Setup(a => a.Id).Returns(Guid.NewGuid());
            app.Setup(a => a.AccessKey).Returns(Guid.NewGuid());

            device = new Mock<IDevice>();
            device.Setup(a => a.Id).Returns(Guid.NewGuid());

            change = new Mock<IChange>();
            change2 = new Mock<IChange>();
            changes = new List<IChange>();

            applicationRepository = new Mock<IApplicationRepository>();
            applicationRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(app.Object);

            deviceRepository = new Mock<IDeviceRepository>();
            deviceRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(device.Object);

            changeRepository = new Mock<IChangeRepository>();
            changeRepository.Setup(x => x.CreateChange(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<string>()))
                .Returns(delegate (Guid recordId, string group, string path, Guid deviceId, DateTime modified, string value)
                {
                    if (change.Object.RecordId == Guid.Empty)
                    {
                        change.Setup(c => c.RecordId).Returns(recordId);
                        change.Setup(c => c.Group).Returns(group);
                        change.Setup(c => c.DeviceId).Returns(deviceId);
                        change.Setup(c => c.Path).Returns(path);
                        change.Setup(c => c.Value).Returns(value);
                        change.Setup(c => c.Modified).Returns(modified);
                        return change.Object;
                    }
                    else
                    {
                        change2.Setup(c => c.RecordId).Returns(recordId);
                        change2.Setup(c => c.Group).Returns(group);
                        change2.Setup(c => c.DeviceId).Returns(deviceId);
                        change2.Setup(c => c.Path).Returns(path);
                        change2.Setup(c => c.Value).Returns(value);
                        change2.Setup(c => c.Modified).Returns(modified);
                        return change2.Object;
                    }
                });

            changeRepository.Setup(x => x.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(changes);

            logger = new Mock<ILogger<SyncController>>();
        }

        [Test]
        public async Task SyncController_Post_Fail_Empty_Request()
        {
            var controller = new SyncController(logger.Object, applicationRepository.Object, deviceRepository.Object, changeRepository.Object);
            var response = await controller.Post(null);

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("app_id missing or invalid request", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Fail_Missing_AppId()
        {
            var controller = new SyncController(logger.Object, applicationRepository.Object, deviceRepository.Object, changeRepository.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("app_id missing or invalid request", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Fail_Incorrect_AppId()
        {
            applicationRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((IApplication)null);

            var controller = new SyncController(logger.Object, applicationRepository.Object, deviceRepository.Object, changeRepository.Object);
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

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Fail_Missing_AppApiAccessKey()
        {
            var controller = new SyncController(logger.Object, applicationRepository.Object, deviceRepository.Object, changeRepository.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("app_api_access_key incorrect for app_id", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Fail_Invalid_AppApiAccessKey()
        {
            var controller = new SyncController(logger.Object, applicationRepository.Object, deviceRepository.Object, changeRepository.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = Guid.NewGuid()
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("app_api_access_key incorrect for app_id", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Fail_Missing_DeviceId()
        {
            deviceRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((IDevice)null);

            var controller = new SyncController(logger.Object, applicationRepository.Object, deviceRepository.Object, changeRepository.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("No device found for device_id", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Fail_Invalid_DeviceId()
        {
            deviceRepository.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync((IDevice)null);

            var controller = new SyncController(logger.Object, applicationRepository.Object, deviceRepository.Object, changeRepository.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey,
                DeviceId = Guid.NewGuid()
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("No device found for device_id", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Success_Basic_NoChanges_NoGroups()
        {
            var controller = new SyncController(logger.Object, applicationRepository.Object, deviceRepository.Object, changeRepository.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey,
                DeviceId = device.Object.Id
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Fail_Single_Change_Badly_Formatted_Path()
        {
            var request = new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey,
                DeviceId = device.Object.Id,
                Changes = new List<SyncRequestViewModel.ChangeViewModel>
                {
                    new SyncRequestViewModel.ChangeViewModel
                    {
                        Group = "Group",
                        Path = "bad format"
                    }
                }
            };
            var controller = new SyncController(logger.Object, applicationRepository.Object, deviceRepository.Object, changeRepository.Object);
            var response = await controller.Post(request);

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("Path is incorrectly formatted, should be formatted <guid>/property.name", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Success_Single_Change()
        {
            string propertyName = "name";
            string group = "group";
            Guid recordId = Guid.NewGuid();
            int modifiedSecondsAgo = 10;
            string value = "Neil";
            List<IChange> returnChanges = null;

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
                DeviceId = device.Object.Id,
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

            var controller = new SyncController(logger.Object, applicationRepository.Object, deviceRepository.Object, changeRepository.Object);
            var response = await controller.Post(request);
            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);
            Assert.NotNull(syncResponse.Groups);
            Assert.AreEqual(0, syncResponse.Groups.Count);

            Assert.NotNull(returnChanges);
            Assert.AreEqual(1, returnChanges.Count);
            Assert.AreEqual(group, returnChanges[0].Group);
            Assert.AreEqual(propertyName, returnChanges[0].Path);
            Assert.AreEqual(recordId, returnChanges[0].RecordId);
            Assert.AreEqual(value, returnChanges[0].Value);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Once);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task SyncController_Post_Success_Two_Changes()
        {
            string propertyName = "name";
            string propertyName2 = "age";
            string group = "group";
            Guid recordId = Guid.NewGuid();
            int modifiedSecondsAgo = 10;
            string value = "Neil";
            string value2 = "10";
            List<IChange> returnChanges = null;

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
                DeviceId = device.Object.Id,
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

            var controller = new SyncController(logger.Object, applicationRepository.Object, deviceRepository.Object, changeRepository.Object);
            var response = await controller.Post(request);
            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);
            Assert.NotNull(syncResponse.Groups);
            Assert.AreEqual(0, syncResponse.Groups.Count);

            Assert.NotNull(returnChanges);
            Assert.AreEqual(2, returnChanges.Count);
            Assert.AreEqual(group, returnChanges[0].Group);
            Assert.AreEqual(propertyName, returnChanges[0].Path);
            Assert.AreEqual(recordId, returnChanges[0].RecordId);
            Assert.AreEqual(value, returnChanges[0].Value);
            Assert.AreEqual(group, returnChanges[1].Group);
            Assert.AreEqual(propertyName2, returnChanges[1].Path);
            Assert.AreEqual(recordId, returnChanges[1].RecordId);
            Assert.AreEqual(value2, returnChanges[1].Value);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Once);
        }

        [Test]
        public async Task SyncController_Post_Success_Null_Tidemark_With_No_Changes()
        {
            Guid appId;
            string listReturnGroup = null;
            string listReturnTidemark = null;

            string tidemark = null;
            string group = "group";

            changeRepository
                .Setup(x => x.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(changes)
                .Callback<Guid, string, string>((a, g, t) =>
                {
                    appId = a;
                    listReturnGroup = g;
                    listReturnTidemark = t;
                });

            var controller = new SyncController(logger.Object, applicationRepository.Object, deviceRepository.Object, changeRepository.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey,
                DeviceId = device.Object.Id,
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

            Assert.AreEqual(app.Object.Id, appId);
            Assert.AreEqual(group, listReturnGroup);
            Assert.AreEqual(tidemark, listReturnTidemark);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task SyncController_Post_Success_Tidemark_With_Single_Change()
        {
            var changeObject = change.Object;
            changes.Add(changeObject);

            var controller = new SyncController(logger.Object, applicationRepository.Object, deviceRepository.Object, changeRepository.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey,
                DeviceId = device.Object.Id,
                Groups = new List<SyncRequestViewModel.GroupViewModel>
                {
                    new SyncRequestViewModel.GroupViewModel
                    {
                        Group = changeObject.Group,
                        Tidemark = changeObject.Tidemark
                    }
                }
            });

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);

            Assert.NotNull(syncResponse.Groups);
            Assert.AreEqual(1, syncResponse.Groups.Count);
            Assert.AreEqual(changeObject.Tidemark, syncResponse.Groups[0].Tidemark);
            Assert.AreEqual(changeObject.Group, syncResponse.Groups[0].Group);

            Assert.NotNull(syncResponse.Groups[0].Changes);
            Assert.AreEqual(1, syncResponse.Groups[0].Changes.Count);

            Assert.AreEqual(changeObject.Modified, syncResponse.Groups[0].Changes[0].Modified);
            Assert.AreEqual(changeObject.Path, syncResponse.Groups[0].Changes[0].Path);
            Assert.AreEqual(changeObject.Value, syncResponse.Groups[0].Changes[0].Value);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task SyncController_Post_Success_Single_Group_With_Two_Changes()
        {
            string tidemark = "tidemark";
            string group = "group";

            var changeObject = change.Object;
            change.Setup(c => c.Tidemark).Returns(tidemark);
            changes.Add(changeObject);

            var change2 = new Mock<IChange>();
            change2.Setup(c => c.Tidemark).Returns(tidemark);
            var changeObject2 = change2.Object;
            changes.Add(changeObject2);

            var controller = new SyncController(logger.Object, applicationRepository.Object, deviceRepository.Object, changeRepository.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey,
                DeviceId = device.Object.Id,
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

            Assert.AreEqual(changeObject.Modified, syncResponse.Groups[0].Changes[0].Modified);
            Assert.AreEqual(changeObject.Path, syncResponse.Groups[0].Changes[0].Path);
            Assert.AreEqual(changeObject.Value, syncResponse.Groups[0].Changes[0].Value);

            Assert.AreEqual(changeObject2.Modified, syncResponse.Groups[0].Changes[1].Modified);
            Assert.AreEqual(changeObject2.Path, syncResponse.Groups[0].Changes[1].Path);
            Assert.AreEqual(changeObject2.Value, syncResponse.Groups[0].Changes[1].Value);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task SyncController_Post_Success_Two_Groups_With_Two_Changes()
        {
            string tidemark = "tidemark";
            string group = "group";
            string group2 = "group2";

            var changeObject = change.Object;
            change.Setup(c => c.Tidemark).Returns(tidemark);

            var change2 = new Mock<IChange>();
            change2.Setup(c => c.Tidemark).Returns(tidemark);
            change2.Setup(c => c.Group).Returns(group2);
            var changeObject2 = change2.Object;

            changeRepository.Setup(x => x.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(delegate (Guid appId, string g, string t)
                {
                    if (g == group)
                        return new List<IChange>() { changeObject };
                    else
                        return new List<IChange>() { changeObject2 };
                });

            var controller = new SyncController(logger.Object, applicationRepository.Object, deviceRepository.Object, changeRepository.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Object.Id,
                AppApiAccessKey = app.Object.AccessKey,
                DeviceId = device.Object.Id,
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
            });

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

            Assert.AreEqual(changeObject.Modified, syncResponse.Groups[0].Changes[0].Modified);
            Assert.AreEqual(changeObject.Path, syncResponse.Groups[0].Changes[0].Path);
            Assert.AreEqual(changeObject.Value, syncResponse.Groups[0].Changes[0].Value);

            Assert.AreEqual(changeObject2.Modified, syncResponse.Groups[1].Changes[0].Modified);
            Assert.AreEqual(changeObject2.Path, syncResponse.Groups[1].Changes[0].Path);
            Assert.AreEqual(changeObject2.Value, syncResponse.Groups[1].Changes[0].Value);

            changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
            changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        }
    }
}
