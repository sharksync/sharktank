using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xunit;
using Amazon.Lambda.Core;
using Amazon.Lambda.TestUtilities;
using Amazon.Lambda.APIGatewayEvents;

using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

using Amazon;

using AWSServerless;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using AWSServerless.ViewModels;
using Moq;
using AWSServerless.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using AWSServerless.DynamoDB.Tables;
using System.Threading;

namespace AWSServerless.Tests
{
    public class SyncControllerTests
    {
        public SyncControllerTests()
        {
        }

        [Fact]
        public async Task TestSuccessWorkFlow()
        {
            var app = new Application { Id = Guid.NewGuid(), ApiAccessKey = Guid.NewGuid() };

            var dynamoDbCacheLogger = new Mock<ILogger<DynamoDbCache>>();
            var dynamoDb = new Mock<IDynamoDBContext>();
            dynamoDb.Setup(x => x.LoadAsync<Application>(app.Id, default(CancellationToken))).Returns(Task.FromResult<Application>(app));

            var cache = new Mock<IMemoryCache>();
            cache.Setup(x => x.Set(It.IsAny<string>(), It.IsAny<object>()));

            var dynamoDbCache = new Mock<DynamoDbCache>(dynamoDbCacheLogger.Object, dynamoDb.Object, cache.Object);

            var logger = new Mock<ILogger<SyncController>>();
            var controller = new SyncController(logger.Object, dynamoDb.Object, dynamoDbCache.Object);
            var response = await controller.Post(new SyncRequestViewModel()
            {
                AppId = app.Id,
                AppApiAccessKey = app.ApiAccessKey
            });

            Assert.IsType(typeof(SyncResponseViewModel), response.Value);

            var syncResponse = response.Value as SyncResponseViewModel;

            Assert.True(syncResponse.Success);
            Assert.Equal(null, syncResponse.Errors);
        }
    }
}
