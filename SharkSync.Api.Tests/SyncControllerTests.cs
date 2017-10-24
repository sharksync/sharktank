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

namespace SharkSync.Api.Tests.Controllers
{
    //public class SyncControllerTests
    //{
    //    Application app;
    //    Device device;
    //    Mock<IDynamoDBContextWithBatch> dynamoDb;
    //    Mock<IMemoryCache> cache;
    //    Mock<DynamoDbCache> dynamoDbCache;
    //    Mock<ILogger<SyncController>> logger;

    //    public SyncControllerTests()
    //    {
    //        app = new Application { Id = Guid.NewGuid(), ApiAccessKey = Guid.NewGuid() };
    //        device = new Device { Id = Guid.NewGuid(), AppId = app.Id, LastSeen = DateTime.UtcNow };

    //        var dynamoDbCacheLogger = new Mock<ILogger<DynamoDbCache>>();
    //        dynamoDb = new Mock<IDynamoDBContextWithBatch>();
    //        dynamoDb.Setup(x => x.LoadAsync<Application>(app.Id, default(CancellationToken))).Returns(Task.FromResult(app));
    //        dynamoDb.Setup(x => x.LoadAsync<Device>(device.Id, default(CancellationToken))).Returns(Task.FromResult(device));
    //        dynamoDb.Setup(x => x.CreateBatchWrite<Change>(It.IsAny<DynamoDBOperationConfig>())).Returns();

    //        cache = new Mock<IMemoryCache>();
    //        cache.Setup(x => x.CreateEntry(It.IsAny<object>())).Returns(new Mock<ICacheEntry>().Object);

    //        dynamoDbCache = new Mock<DynamoDbCache>(dynamoDbCacheLogger.Object, dynamoDb.Object, cache.Object);

    //        logger = new Mock<ILogger<SyncController>>();
    //    }

    //    [Test]
    //    public async Task Fail_Empty_Request()
    //    {
    //        var controller = new SyncController(logger.Object, dynamoDb.Object, dynamoDbCache.Object);
    //        var response = await controller.Post(null);

    //        Assert.IsType(typeof(SyncResponseViewModel), response.Value);

    //        var syncResponse = response.Value as SyncResponseViewModel;

    //        Assert.NotNull(syncResponse);
    //        Assert.NotNull(syncResponse.Errors);
    //        Assert.Equal(1, syncResponse.Errors.Count());
    //        Assert.Equal("app_id missing or invalid request", syncResponse.Errors.First());
    //        Assert.False(syncResponse.Success);
    //    }

    //    [Test]
    //    public async Task Fail_Missing_AppId()
    //    {
    //        var controller = new SyncController(logger.Object, dynamoDb.Object, dynamoDbCache.Object);
    //        var response = await controller.Post(new SyncRequestViewModel()
    //        {
    //        });

    //        Assert.IsType(typeof(SyncResponseViewModel), response.Value);

    //        var syncResponse = response.Value as SyncResponseViewModel;

    //        Assert.NotNull(syncResponse);
    //        Assert.NotNull(syncResponse.Errors);
    //        Assert.Equal(1, syncResponse.Errors.Count());
    //        Assert.Equal("app_id missing or invalid request", syncResponse.Errors.First());
    //        Assert.False(syncResponse.Success);
    //    }

    //    [Test]
    //    public async Task Fail_Incorrect_AppId()
    //    {
    //        var controller = new SyncController(logger.Object, dynamoDb.Object, dynamoDbCache.Object);
    //        var response = await controller.Post(new SyncRequestViewModel()
    //        {
    //            AppId = Guid.NewGuid()
    //        });

    //        Assert.IsType(typeof(SyncResponseViewModel), response.Value);

    //        var syncResponse = response.Value as SyncResponseViewModel;

    //        Assert.NotNull(syncResponse);
    //        Assert.NotNull(syncResponse.Errors);
    //        Assert.Equal(1, syncResponse.Errors.Count());
    //        Assert.Equal("No application found for app_id", syncResponse.Errors.First());
    //        Assert.False(syncResponse.Success);
    //    }

    //    [Test]
    //    public async Task Fail_Missing_AppApiAccessKey()
    //    {
    //        var controller = new SyncController(logger.Object, dynamoDb.Object, dynamoDbCache.Object);
    //        var response = await controller.Post(new SyncRequestViewModel()
    //        {
    //            AppId = app.Id
    //        });

    //        Assert.IsType(typeof(SyncResponseViewModel), response.Value);

    //        var syncResponse = response.Value as SyncResponseViewModel;

    //        Assert.NotNull(syncResponse);
    //        Assert.NotNull(syncResponse.Errors);
    //        Assert.Equal(1, syncResponse.Errors.Count());
    //        Assert.Equal("app_api_access_key incorrect for app_id", syncResponse.Errors.First());
    //        Assert.False(syncResponse.Success);
    //    }

    //    [Test]
    //    public async Task Fail_Invalid_AppApiAccessKey()
    //    {
    //        var controller = new SyncController(logger.Object, dynamoDb.Object, dynamoDbCache.Object);
    //        var response = await controller.Post(new SyncRequestViewModel()
    //        {
    //            AppId = app.Id,
    //            AppApiAccessKey = Guid.NewGuid()
    //        });

    //        Assert.IsType(typeof(SyncResponseViewModel), response.Value);

    //        var syncResponse = response.Value as SyncResponseViewModel;

    //        Assert.NotNull(syncResponse);
    //        Assert.NotNull(syncResponse.Errors);
    //        Assert.Equal(1, syncResponse.Errors.Count());
    //        Assert.Equal("app_api_access_key incorrect for app_id", syncResponse.Errors.First());
    //        Assert.False(syncResponse.Success);
    //    }

    //    [Test]
    //    public async Task Fail_Missing_DeviceId()
    //    {
    //        var controller = new SyncController(logger.Object, dynamoDb.Object, dynamoDbCache.Object);
    //        var response = await controller.Post(new SyncRequestViewModel()
    //        {
    //            AppId = app.Id,
    //            AppApiAccessKey = app.ApiAccessKey
    //        });

    //        Assert.IsType(typeof(SyncResponseViewModel), response.Value);

    //        var syncResponse = response.Value as SyncResponseViewModel;

    //        Assert.NotNull(syncResponse);
    //        Assert.NotNull(syncResponse.Errors);
    //        Assert.Equal(1, syncResponse.Errors.Count());
    //        Assert.Equal("No device found for device_id", syncResponse.Errors.First());
    //        Assert.False(syncResponse.Success);
    //    }

    //    [Test]
    //    public async Task Fail_Invalid_DeviceId()
    //    {
    //        var controller = new SyncController(logger.Object, dynamoDb.Object, dynamoDbCache.Object);
    //        var response = await controller.Post(new SyncRequestViewModel()
    //        {
    //            AppId = app.Id,
    //            AppApiAccessKey = app.ApiAccessKey,
    //            DeviceId = Guid.NewGuid()
    //        });

    //        Assert.IsType(typeof(SyncResponseViewModel), response.Value);

    //        var syncResponse = response.Value as SyncResponseViewModel;

    //        Assert.NotNull(syncResponse);
    //        Assert.NotNull(syncResponse.Errors);
    //        Assert.Equal(1, syncResponse.Errors.Count());
    //        Assert.Equal("No device found for device_id", syncResponse.Errors.First());
    //        Assert.False(syncResponse.Success);
    //    }

    //    [Test]
    //    public async Task Success_Basic_NoChanges_NoGroups()
    //    {
    //        var controller = new SyncController(logger.Object, dynamoDb.Object, dynamoDbCache.Object);
    //        var response = await controller.Post(new SyncRequestViewModel()
    //        {
    //            AppId = app.Id,
    //            AppApiAccessKey = app.ApiAccessKey,
    //            DeviceId = device.Id
    //        });

    //        Assert.IsType(typeof(SyncResponseViewModel), response.Value);

    //        var syncResponse = response.Value as SyncResponseViewModel;

    //        Assert.NotNull(syncResponse);
    //        Assert.Null(syncResponse.Errors);
    //        Assert.True(syncResponse.Success);
    //    }

    //    [Test]
    //    public async Task Success_Basic_EmptyChange()
    //    {
    //        var request = new SyncRequestViewModel()
    //        {
    //            AppId = app.Id,
    //            AppApiAccessKey = app.ApiAccessKey,
    //            DeviceId = device.Id,
    //            Changes = new List<SyncRequestViewModel.ChangeViewModel>
    //            {
    //                new SyncRequestViewModel.ChangeViewModel
    //                {

    //                }
    //            }
    //        };
    //        var controller = new SyncController(logger.Object, dynamoDb.Object, dynamoDbCache.Object);
    //        var response = await controller.Post(request);

    //        Assert.IsType(typeof(SyncResponseViewModel), response.Value);

    //        var syncResponse = response.Value as SyncResponseViewModel;

    //        Assert.NotNull(syncResponse);
    //        Assert.Null(syncResponse.Errors);
    //        Assert.True(syncResponse.Success);
    //    }

    //    [Test]
    //    public async Task Success_Basic_Badly_Formatted_Path()
    //    {
    //        var request = new SyncRequestViewModel()
    //        {
    //            AppId = app.Id,
    //            AppApiAccessKey = app.ApiAccessKey,
    //            DeviceId = device.Id,
    //            Changes = new List<SyncRequestViewModel.ChangeViewModel>
    //            {
    //                new SyncRequestViewModel.ChangeViewModel
    //                {
    //                    Group = "Group",
    //                    Path = "bad format"
    //                }
    //            }
    //        };
    //        var controller = new SyncController(logger.Object, dynamoDb.Object, dynamoDbCache.Object);
    //        var response = await controller.Post(request);

    //        Assert.IsType(typeof(SyncResponseViewModel), response.Value);

    //        var syncResponse = response.Value as SyncResponseViewModel;

    //        Assert.NotNull(syncResponse);
    //        Assert.Null(syncResponse.Errors);
    //        Assert.True(syncResponse.Success);

    //        dynamoDb.Verify(t => t.ExecuteBatchWriteAsync(It.IsAny<BatchWrite[]>(), It.IsAny<CancellationToken>()));
    //    }
    //}
}
