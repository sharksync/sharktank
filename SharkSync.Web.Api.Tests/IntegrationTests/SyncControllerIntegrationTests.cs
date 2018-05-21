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
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using Amazon;
using Amazon.DynamoDBv2.Model;

namespace SharkSync.Web.Api.Tests.IntegrationTests
{
    [TestFixture]
    public class SyncControllerIntegrationTests
    {
        private const string SyncRequestUrl = "https://5j4kepan7c.execute-api.eu-west-1.amazonaws.com/Prod/Api/Sync";
        //private const string SyncRequestUrl = "http://localhost:57829/Api/Sync";

        private static readonly HttpClient HttpClient = new HttpClient();

        private AmazonDynamoDBClient dynamoDBClient = null;
        private DynamoDBContext dynamoDBContext = null;

        [SetUp]
        public void SetUp()
        {
            var credentialProfileStoreChain = new CredentialProfileStoreChain();
            AWSCredentials awsCredentials;
            if (!credentialProfileStoreChain.TryGetAWSCredentials("silvergames", out awsCredentials))
                throw new AmazonClientException("Unable to find a profile named silvergames");

            dynamoDBClient = new AmazonDynamoDBClient(awsCredentials, RegionEndpoint.EUWest1);
            dynamoDBContext = new DynamoDBContext(dynamoDBClient);
        }

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
                AppId = new Guid("afd8db1e-73b8-4d5f-9cb1-6b49d205555a")
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
            var testApp = new Application
            {
                Id = new Guid("afd8db1e-73b8-4d5f-9cb1-6b49d205555a"),
                AccountId = new Guid("250c6f28-4611-4c28-902c-8464fabc510b"),
                AccessKey = new Guid("3d65a27c-9d1d-48a3-a888-89cc0f7851d0"),
                Name = "Integration Test App"
            };

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
            var testApp = new Application
            {
                Id = new Guid("59eadf1b-c4bf-4ded-8a2b-b80305b960fe"),
                AccountId = new Guid("250c6f28-4611-4c28-902c-8464fabc510b"),
                AccessKey = new Guid("e7b40cf0-2781-4dc7-9545-91fd812fc506"),
                Name = "Integration Test App 2"
            };

            string propertyName = "name";
            string group = "group";
            Guid recordId = Guid.NewGuid();
            int modifiedSecondsAgo = 10;
            string value = "Neil";

            await DeleteChangeRows(testApp.Id, group);

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
                },
                Groups = new List<SyncRequestViewModel.GroupViewModel>
                {
                    new SyncRequestViewModel.GroupViewModel
                    {
                        Group = group,
                        Tidemark = null
                    }
                }
            };

            var jsonPayload = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(SyncRequestUrl, requestContent);

            var responsePayload = await response.Content?.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responsePayload);

            var syncResponse = JsonConvert.DeserializeObject<SyncResponseViewModel>(responsePayload);

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);
            Assert.NotNull(syncResponse.Groups);
            Assert.AreEqual(1, syncResponse.Groups.Count);

            Assert.NotNull(syncResponse.Groups[0].Changes);
            Assert.AreEqual(1, syncResponse.Groups[0].Changes.Count);

            var dynamoRows = await GetChangeRows(testApp.Id, group);

            Assert.NotNull(dynamoRows);
            Assert.AreEqual(1, dynamoRows.Count);
            Assert.AreEqual(group, dynamoRows[0].Group);
            Assert.AreEqual(propertyName, dynamoRows[0].Path);
            Assert.AreEqual(recordId, dynamoRows[0].RecordId);
            Assert.AreEqual(value, dynamoRows[0].Value);

            Assert.AreEqual(dynamoRows[0].Modified, syncResponse.Groups[0].Changes[0].Modified);
            Assert.AreEqual(propertyName, syncResponse.Groups[0].Changes[0].Path);
            Assert.AreEqual(value, syncResponse.Groups[0].Changes[0].Value);
        }

        [Test]
        public async Task SyncController_Post_Success_Single_Group_With_Two_Changes()
        {
            var testApp = new Application
            {
                Id = new Guid("b858ceb1-00d0-4427-b45d-e9890b77da36"),
                AccountId = new Guid("250c6f28-4611-4c28-902c-8464fabc510b"),
                AccessKey = new Guid("03172495-6158-44ae-b5b4-6ea5163f02d8"),
                Name = "Integration Test App 3"
            };

            string propertyName = "name";
            string propertyName2 = "age";
            string group = "group";
            Guid recordId = Guid.NewGuid();
            int modifiedSecondsAgo = 10;
            string value = "Neil";
            string value2 = "10";

            await DeleteChangeRows(testApp.Id, group);

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
                    },
                    new SyncRequestViewModel.ChangeViewModel
                    {
                        Group = group,
                        Path = $"{recordId}/{propertyName2}",
                        SecondsAgo = modifiedSecondsAgo,
                        Value = value2
                    }
                }, 
                Groups = new List<SyncRequestViewModel.GroupViewModel>
                {
                    new SyncRequestViewModel.GroupViewModel
                    {
                        Group = group,
                        Tidemark = null
                    }
                }
            };

            var jsonPayload = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
            var response = await HttpClient.PostAsync(SyncRequestUrl, requestContent);

            var responsePayload = await response.Content?.ReadAsStringAsync();
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(responsePayload);

            var syncResponse = JsonConvert.DeserializeObject<SyncResponseViewModel>(responsePayload);

            Assert.NotNull(syncResponse);
            Assert.Null(syncResponse.Errors);
            Assert.True(syncResponse.Success);

            var dynamoRows = await GetChangeRows(testApp.Id, group);

            Assert.NotNull(dynamoRows);
            Assert.AreEqual(2, dynamoRows.Count);
            Assert.AreEqual(group, dynamoRows[0].Group);
            Assert.AreEqual(propertyName, dynamoRows[0].Path);
            Assert.AreEqual(recordId, dynamoRows[0].RecordId);
            Assert.AreEqual(value, dynamoRows[0].Value);
            Assert.AreEqual(group, dynamoRows[1].Group);
            Assert.AreEqual(propertyName2, dynamoRows[1].Path);
            Assert.AreEqual(recordId, dynamoRows[1].RecordId);
            Assert.AreEqual(value2, dynamoRows[1].Value);

            Assert.NotNull(syncResponse.Groups);
            Assert.AreEqual(1, syncResponse.Groups.Count);
            Assert.AreEqual(dynamoRows.Last().Tidemark, syncResponse.Groups[0].Tidemark);
            Assert.AreEqual(group, syncResponse.Groups[0].Group);

            Assert.NotNull(syncResponse.Groups[0].Changes);
            Assert.AreEqual(2, syncResponse.Groups[0].Changes.Count);

            Assert.AreEqual(dynamoRows[0].Modified, syncResponse.Groups[0].Changes[0].Modified);
            Assert.AreEqual(propertyName, syncResponse.Groups[0].Changes[0].Path);
            Assert.AreEqual(value, syncResponse.Groups[0].Changes[0].Value);

            Assert.AreEqual(dynamoRows[1].Modified, syncResponse.Groups[0].Changes[1].Modified);
            Assert.AreEqual(propertyName2, syncResponse.Groups[0].Changes[1].Path);
            Assert.AreEqual(value2, syncResponse.Groups[0].Changes[1].Value);
        }

        public async Task<List<Change>> GetChangeRows(Guid appId, string group)
        {
            var appConfig = GetConfig(appId);

            var query = dynamoDBContext.QueryAsync<Change>(group, appConfig);
            var storedChanges = await query.GetNextSetAsync();
            return storedChanges;
        }

        public async Task DeleteChangeRows(Guid appId, string group)
        {
            var appConfig = GetConfig(appId);

            var query = dynamoDBContext.QueryAsync<Change>(group, appConfig);
            var storedChanges = await query.GetNextSetAsync();
            foreach (var change in storedChanges)
                await dynamoDBContext.DeleteAsync(change, appConfig);
        }

        private DynamoDBOperationConfig GetConfig(Guid appId)
        {
            return new DynamoDBOperationConfig { OverrideTableName = $"{appId}-Change" };
        }
    }
}
