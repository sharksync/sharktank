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
using SharkSync.Interfaces.Repositories;
using SharkSync.Interfaces.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Net;
using Newtonsoft.Json;
using SharkSync.Repositories.Entities;

namespace SharkSync.Web.Api.Tests.IntegrationTests
{
    [TestFixture]
    public class SyncControllerIntegrationTests
    {
        private const string SyncRequestUrl = "https://5j4kepan7c.execute-api.eu-west-1.amazonaws.com/Prod/Api/Sync";

        private static readonly HttpClient HttpClient = new HttpClient();
        private static readonly IApplication testApp = new Application
        {
            Id = new Guid("afd8db1e-73b8-4d5f-9cb1-6b49d205555a"),
            AccountId = new Guid("250c6f28-4611-4c28-902c-8464fabc510b"),
            AccessKey = new Guid("3d65a27c-9d1d-48a3-a888-89cc0f7851d0"),
            Name = "Integration Test App"
        };

        [Test]
        public async Task SyncController_Post_Fail_Missing_Request()
        {
            var jsonPayload = new StringContent("", Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(SyncRequestUrl, jsonPayload);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(response.Content);

            var syncResponse = JsonConvert.DeserializeObject<SyncResponseViewModel>(await response.Content.ReadAsStringAsync());

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("A non-empty request body is required.", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);
        }

        [Test]
        public async Task SyncController_Post_Fail_Empty_Request()
        {
            SyncRequestViewModel request = new SyncRequestViewModel();

            var jsonPayload = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(SyncRequestUrl, jsonPayload);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(response.Content);

            var syncResponse = JsonConvert.DeserializeObject<SyncResponseViewModel>(await response.Content.ReadAsStringAsync());

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("app_id missing or invalid request", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);
        }

        [Test]
        public async Task SyncController_Post_Fail_Incorrect_AppId()
        {
            SyncRequestViewModel request = new SyncRequestViewModel()
            {
                AppId = Guid.NewGuid()
            };

            var jsonPayload = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(SyncRequestUrl, jsonPayload);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(response.Content);

            var syncResponse = JsonConvert.DeserializeObject<SyncResponseViewModel>(await response.Content.ReadAsStringAsync());

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("No application found for app_id", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);
        }

        [Test]
        public async Task SyncController_Post_Fail_Missing_AppApiAccessKey()
        {
            SyncRequestViewModel request = new SyncRequestViewModel()
            {
                AppId = testApp.Id
            };

            var jsonPayload = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(SyncRequestUrl, jsonPayload);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.NotNull(response.Content);

            var syncResponse = JsonConvert.DeserializeObject<SyncResponseViewModel>(await response.Content.ReadAsStringAsync());

            Assert.NotNull(syncResponse);
            Assert.NotNull(syncResponse.Errors);
            Assert.AreEqual(1, syncResponse.Errors.Count());
            Assert.AreEqual("app_api_access_key incorrect for app_id", syncResponse.Errors.First());
            Assert.False(syncResponse.Success);
        }

        [Test]
        public async Task SyncController_Post_Success_Basic_NoChanges_NoGroups()
        {
            SyncRequestViewModel request = new SyncRequestViewModel()
            {
                AppId = testApp.Id,
                AppApiAccessKey = testApp.AccessKey
            };

            var jsonPayload = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(SyncRequestUrl, jsonPayload);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(response.Content);

            var syncResponse = JsonConvert.DeserializeObject<SyncResponseViewModel>(await response.Content.ReadAsStringAsync());

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);
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

            SyncRequestViewModel request = new SyncRequestViewModel()
            {
                AppId = testApp.Id,
                AppApiAccessKey = testApp.AccessKey,
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

            var jsonPayload = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(SyncRequestUrl, jsonPayload);

            var responsePayload = await response.Content?.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responsePayload);

            var syncResponse = JsonConvert.DeserializeObject<SyncResponseViewModel>(responsePayload);

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
        }

        //[Test]
        //public async Task SyncController_Post_Success_Two_Changes()
        //{
        //    string propertyName = "name";
        //    string propertyName2 = "age";
        //    string group = "group";
        //    Guid recordId = Guid.NewGuid();
        //    int modifiedSecondsAgo = 10;
        //    string value = "Neil";
        //    string value2 = "10";
        //    List<IChange> returnChanges = null;

        //    changeRepository
        //        .Setup(x => x.UpsertChangesAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<IChange>>()))
        //        .Returns(() => Task.FromResult((string)null))
        //        .Callback<Guid, IEnumerable<IChange>>((a, l) =>
        //        {
        //            returnChanges = l.ToList();
        //        });

        //    var request = new SyncRequestViewModel()
        //    {
        //        AppId = app.Object.Id,
        //        AppApiAccessKey = app.Object.AccessKey,
        //        Changes = new List<SyncRequestViewModel.ChangeViewModel>
        //        {
        //            new SyncRequestViewModel.ChangeViewModel
        //            {
        //                Group = group,
        //                Path = $"{recordId}/{propertyName}",
        //                SecondsAgo = modifiedSecondsAgo,
        //                Value = value
        //            },
        //            new SyncRequestViewModel.ChangeViewModel
        //            {
        //                Group = group,
        //                Path = $"{recordId}/{propertyName2}",
        //                SecondsAgo = modifiedSecondsAgo,
        //                Value = value2
        //            }
        //        }
        //    };

        //    var controller = new SyncController(logger.Object, applicationRepository.Object, changeRepository.Object);
        //    var response = await controller.Post(request) as JsonResult;
        //    var syncResponse = response.Value as SyncResponseViewModel;

        //    Assert.NotNull(syncResponse);
        //    Assert.Null(syncResponse.Errors);
        //    Assert.True(syncResponse.Success);
        //    Assert.NotNull(syncResponse.Groups);
        //    Assert.AreEqual(0, syncResponse.Groups.Count);

        //    Assert.NotNull(returnChanges);
        //    Assert.AreEqual(2, returnChanges.Count);
        //    Assert.AreEqual(group, returnChanges[0].Group);
        //    Assert.AreEqual(propertyName, returnChanges[0].Path);
        //    Assert.AreEqual(recordId, returnChanges[0].RecordId);
        //    Assert.AreEqual(value, returnChanges[0].Value);
        //    Assert.AreEqual(group, returnChanges[1].Group);
        //    Assert.AreEqual(propertyName2, returnChanges[1].Path);
        //    Assert.AreEqual(recordId, returnChanges[1].RecordId);
        //    Assert.AreEqual(value2, returnChanges[1].Value);

        //    changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Once);
        //}

        //[Test]
        //public async Task SyncController_Post_Success_Null_Tidemark_With_No_Changes()
        //{
        //    Guid appId;
        //    string listReturnGroup = null;
        //    string listReturnTidemark = null;

        //    string tidemark = null;
        //    string group = "group";

        //    changeRepository
        //        .Setup(x => x.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
        //        .ReturnsAsync(changes)
        //        .Callback<Guid, string, string>((a, g, t) =>
        //        {
        //            appId = a;
        //            listReturnGroup = g;
        //            listReturnTidemark = t;
        //        });

        //    var controller = new SyncController(logger.Object, applicationRepository.Object, changeRepository.Object);
        //    var response = await controller.Post(new SyncRequestViewModel()
        //    {
        //        AppId = app.Object.Id,
        //        AppApiAccessKey = app.Object.AccessKey,
        //        Groups = new List<SyncRequestViewModel.GroupViewModel>
        //        {
        //            new SyncRequestViewModel.GroupViewModel
        //            {
        //                Group = group,
        //                Tidemark = tidemark
        //            }
        //        }
        //    }) as JsonResult;

        //    var syncResponse = response.Value as SyncResponseViewModel;

        //    Assert.NotNull(syncResponse);
        //    Assert.Null(syncResponse.Errors);
        //    Assert.True(syncResponse.Success);

        //    Assert.AreEqual(app.Object.Id, appId);
        //    Assert.AreEqual(group, listReturnGroup);
        //    Assert.AreEqual(tidemark, listReturnTidemark);

        //    changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
        //    changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        //}

        //[Test]
        //public async Task SyncController_Post_Success_Tidemark_With_Single_Change()
        //{
        //    var changeObject = change.Object;
        //    changes.Add(changeObject);

        //    var controller = new SyncController(logger.Object, applicationRepository.Object, changeRepository.Object);
        //    var response = await controller.Post(new SyncRequestViewModel()
        //    {
        //        AppId = app.Object.Id,
        //        AppApiAccessKey = app.Object.AccessKey,
        //        Groups = new List<SyncRequestViewModel.GroupViewModel>
        //        {
        //            new SyncRequestViewModel.GroupViewModel
        //            {
        //                Group = changeObject.Group,
        //                Tidemark = changeObject.Tidemark
        //            }
        //        }
        //    }) as JsonResult;

        //    var syncResponse = response.Value as SyncResponseViewModel;

        //    Assert.NotNull(syncResponse);
        //    Assert.Null(syncResponse.Errors);
        //    Assert.True(syncResponse.Success);

        //    Assert.NotNull(syncResponse.Groups);
        //    Assert.AreEqual(1, syncResponse.Groups.Count);
        //    Assert.AreEqual(changeObject.Tidemark, syncResponse.Groups[0].Tidemark);
        //    Assert.AreEqual(changeObject.Group, syncResponse.Groups[0].Group);

        //    Assert.NotNull(syncResponse.Groups[0].Changes);
        //    Assert.AreEqual(1, syncResponse.Groups[0].Changes.Count);

        //    Assert.AreEqual(changeObject.Modified, syncResponse.Groups[0].Changes[0].Modified);
        //    Assert.AreEqual(changeObject.Path, syncResponse.Groups[0].Changes[0].Path);
        //    Assert.AreEqual(changeObject.Value, syncResponse.Groups[0].Changes[0].Value);

        //    changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
        //    changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        //}

        //[Test]
        //public async Task SyncController_Post_Success_Single_Group_With_Two_Changes()
        //{
        //    string tidemark = "tidemark";
        //    string group = "group";

        //    var changeObject = change.Object;
        //    change.Setup(c => c.Tidemark).Returns(tidemark);
        //    changes.Add(changeObject);

        //    var change2 = new Mock<IChange>();
        //    change2.Setup(c => c.Tidemark).Returns(tidemark);
        //    var changeObject2 = change2.Object;
        //    changes.Add(changeObject2);

        //    var controller = new SyncController(logger.Object, applicationRepository.Object, changeRepository.Object);
        //    var response = await controller.Post(new SyncRequestViewModel()
        //    {
        //        AppId = app.Object.Id,
        //        AppApiAccessKey = app.Object.AccessKey,
        //        Groups = new List<SyncRequestViewModel.GroupViewModel>
        //        {
        //            new SyncRequestViewModel.GroupViewModel
        //            {
        //                Group = group,
        //                Tidemark = tidemark
        //            }
        //        }
        //    }) as JsonResult;

        //    var syncResponse = response.Value as SyncResponseViewModel;

        //    Assert.NotNull(syncResponse);
        //    Assert.Null(syncResponse.Errors);
        //    Assert.True(syncResponse.Success);

        //    Assert.NotNull(syncResponse.Groups);
        //    Assert.AreEqual(1, syncResponse.Groups.Count);
        //    Assert.AreEqual(tidemark, syncResponse.Groups[0].Tidemark);
        //    Assert.AreEqual(group, syncResponse.Groups[0].Group);

        //    Assert.NotNull(syncResponse.Groups[0].Changes);
        //    Assert.AreEqual(2, syncResponse.Groups[0].Changes.Count);

        //    Assert.AreEqual(changeObject.Modified, syncResponse.Groups[0].Changes[0].Modified);
        //    Assert.AreEqual(changeObject.Path, syncResponse.Groups[0].Changes[0].Path);
        //    Assert.AreEqual(changeObject.Value, syncResponse.Groups[0].Changes[0].Value);

        //    Assert.AreEqual(changeObject2.Modified, syncResponse.Groups[0].Changes[1].Modified);
        //    Assert.AreEqual(changeObject2.Path, syncResponse.Groups[0].Changes[1].Path);
        //    Assert.AreEqual(changeObject2.Value, syncResponse.Groups[0].Changes[1].Value);

        //    changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
        //    changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        //}

        //[Test]
        //public async Task SyncController_Post_Success_Two_Groups_With_Two_Changes()
        //{
        //    string tidemark = "tidemark";
        //    string group = "group";
        //    string group2 = "group2";

        //    var changeObject = change.Object;
        //    change.Setup(c => c.Tidemark).Returns(tidemark);

        //    var change2 = new Mock<IChange>();
        //    change2.Setup(c => c.Tidemark).Returns(tidemark);
        //    change2.Setup(c => c.Group).Returns(group2);
        //    var changeObject2 = change2.Object;

        //    changeRepository.Setup(x => x.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
        //        .ReturnsAsync(delegate (Guid appId, string g, string t)
        //        {
        //            if (g == group)
        //                return new List<IChange>() { changeObject };
        //            else
        //                return new List<IChange>() { changeObject2 };
        //        });

        //    var controller = new SyncController(logger.Object, applicationRepository.Object, changeRepository.Object);
        //    var response = await controller.Post(new SyncRequestViewModel()
        //    {
        //        AppId = app.Object.Id,
        //        AppApiAccessKey = app.Object.AccessKey,
        //        Groups = new List<SyncRequestViewModel.GroupViewModel>
        //        {
        //            new SyncRequestViewModel.GroupViewModel
        //            {
        //                Group = group,
        //                Tidemark = tidemark
        //            },
        //            new SyncRequestViewModel.GroupViewModel
        //            {
        //                Group = group2,
        //                Tidemark = null
        //            }
        //        }
        //    }) as JsonResult;

        //    var syncResponse = response.Value as SyncResponseViewModel;

        //    Assert.NotNull(syncResponse);
        //    Assert.Null(syncResponse.Errors);
        //    Assert.True(syncResponse.Success);

        //    Assert.NotNull(syncResponse.Groups);
        //    Assert.AreEqual(2, syncResponse.Groups.Count);
        //    Assert.AreEqual(tidemark, syncResponse.Groups[0].Tidemark);
        //    Assert.AreEqual(group, syncResponse.Groups[0].Group);
        //    Assert.NotNull(syncResponse.Groups[0].Changes);
        //    Assert.AreEqual(1, syncResponse.Groups[0].Changes.Count);

        //    Assert.AreEqual(tidemark, syncResponse.Groups[1].Tidemark);
        //    Assert.AreEqual(group2, syncResponse.Groups[1].Group);
        //    Assert.NotNull(syncResponse.Groups[1].Changes);
        //    Assert.AreEqual(1, syncResponse.Groups[1].Changes.Count);

        //    Assert.AreEqual(changeObject.Modified, syncResponse.Groups[0].Changes[0].Modified);
        //    Assert.AreEqual(changeObject.Path, syncResponse.Groups[0].Changes[0].Path);
        //    Assert.AreEqual(changeObject.Value, syncResponse.Groups[0].Changes[0].Value);

        //    Assert.AreEqual(changeObject2.Modified, syncResponse.Groups[1].Changes[0].Modified);
        //    Assert.AreEqual(changeObject2.Path, syncResponse.Groups[1].Changes[0].Path);
        //    Assert.AreEqual(changeObject2.Value, syncResponse.Groups[1].Changes[0].Value);

        //    changeRepository.Verify(t => t.UpsertChangesAsync(app.Object.Id, It.IsAny<IEnumerable<IChange>>()), Times.Never);
        //    changeRepository.Verify(t => t.ListChangesAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
        //}
    }
}
