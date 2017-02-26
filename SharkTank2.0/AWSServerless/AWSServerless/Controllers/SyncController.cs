using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AWSServerless.ViewModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Amazon.DynamoDBv2.DataModel;
using AWSServerless.DynamoDB.Tables;
using Amazon.DynamoDBv2;
using Amazon;
using Amazon.DynamoDBv2.DocumentModel;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace AWSServerless.Controllers
{
    [Route("sync")]
    public class SyncController : Controller
    {
        ILogger Logger { get; set; }

        IDynamoDBContext DynamoDB { get; set; }

        DynamoDbCache Cache { get; set; }

        DateTime requestStartTimeUTC { get; set; }

        public SyncController(ILogger<SyncController> logger, IDynamoDBContext dynamoDB, DynamoDbCache cache)
        {
            Logger = logger;
            DynamoDB = dynamoDB;
            Cache = cache;
        }

        [HttpPost]
        public async Task<JsonResult> Post([FromBody]SyncRequestViewModel request)
        {
            requestStartTimeUTC = DateTime.UtcNow;
            var response = new SyncResponseViewModel();

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var app = await ValidateApplication(request);
                Logger.LogInformation($"ValidateApplication in {sw.ElapsedMilliseconds}ms");

                if (!ModelState.IsValid)
                    return JsonResultWithValidationErrors(response);

                sw.Restart();
                var device = await ValidateDevice(request);
                Logger.LogInformation($"ValidateDevice in {sw.ElapsedMilliseconds}ms");

                if (!ModelState.IsValid)
                    return JsonResultWithValidationErrors(response);

                sw.Restart();
                await ProcessChanges(app, device, request);
                Logger.LogInformation($"ProcessChanges in {sw.ElapsedMilliseconds}ms");

                sw.Restart();
                response = await GetChanges(app, device, request);
                Logger.LogInformation($"GetChanges in {sw.ElapsedMilliseconds}ms");

                UpdateDeviceLastSeen(device);

                sw.Stop();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to complete Sync Post: {ex.ToString()}");
                ModelState.AddModelError("", $"Unhandled exception: {ex.ToString()}");
            }

            return JsonResultWithValidationErrors(response);
        }

        private void UpdateDeviceLastSeen(Device device)
        {
            // Doesn't need to be done before returning to the client so run it later
            Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                device.LastSeen = DateTime.UtcNow;
                DynamoDB.SaveAsync(device);

                Logger.LogInformation($"UpdateDeviceLastSeen in {sw.ElapsedMilliseconds}ms");
                sw.Stop();
            });
        }

        private async Task<Application> ValidateApplication(SyncRequestViewModel request)
        {
            if (request == null || request.AppId == Guid.Empty)
                ModelState.AddModelError("app_id", "app_id missing or invalid request");
            else
            {
                var app = await Cache.GetFromCacheOrDynamoDb<Application>(request.AppId);

                if (app == null)
                    ModelState.AddModelError("app_id", "No application found for app_id");
                else
                {
                    if (app.ApiAccessKey != request.AppApiAccessKey)
                        ModelState.AddModelError("app_api_access_key", "app_api_access_key incorrect for app_id");
                }

                return app;
            }

            return null;
        }

        private async Task<Device> ValidateDevice(SyncRequestViewModel request)
        {
            var device = await Cache.GetFromCacheOrDynamoDb<Device>(request.DeviceId);
            
            if (device == null)
                ModelState.AddModelError("device_id", "No device found for device_id");

            return device;
        }

        private async Task ProcessChanges(Application app, Device device, SyncRequestViewModel request)
        {
            var batchWrites = new Dictionary<string, BatchWrite<Change>>();

            if (request.Changes != null && request.Changes.Any())
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                // Dynamo batches are limited to 20 odd records at time
                foreach (var batch in request.Changes.Batch(20))
                {
                    foreach (var change in batch)
                    {
                        Guid recordId = Guid.Empty;
                        string path = null;

                        // Path should contain a / in format <guid>/property.name
                        if (change.Path.IndexOf("/") > -1)
                        {
                            recordId = Guid.Parse(change.Path.Substring(0, change.Path.IndexOf("/")));
                            path = change.Path.Substring(change.Path.IndexOf("/") + 1);
                        }

                        DateTime modifiedUTC = requestStartTimeUTC.AddSeconds(-change.SecondsAgo);

                        var dbChange = new Change()
                        {
                            Group = change.Group,
                            Tidemark = HiResDateTime.UtcNowTicks,
                            DeviceId = device.Id,
                            Path = change.Path,
                            Modified = modifiedUTC,
                            Value = change.Value
                        };

                        string tableName = $"{app.Id}-Change";
                        BatchWrite<Change> batchWrite = null;

                        if (!batchWrites.TryGetValue(tableName, out batchWrite))
                        {
                            batchWrite = ((DynamoDBContext)DynamoDB).CreateBatchWrite<Change>(new DynamoDBOperationConfig { OverrideTableName = tableName });
                            batchWrites.Add(tableName, batchWrite);
                        }

                        batchWrite.AddPutItem(dbChange);
                    }

                    sw.Restart();
                    await DynamoDB.ExecuteBatchWriteAsync(batchWrites.Values.ToArray());
                    Logger.LogInformation($"Saved changes to DynamoDB in {sw.ElapsedMilliseconds}ms count: {batchWrites.Values.Count}");
                }

                sw.Stop();
            }
        }

        private async Task<SyncResponseViewModel> GetChanges(Application app, Device device, SyncRequestViewModel request)
        {
            var response = new SyncResponseViewModel() { Groups = new List<SyncResponseViewModel.GroupViewModel>() };

            Stopwatch sw = new Stopwatch();
            sw.Start();
            
            foreach (var group in request.Groups)
            {
                List<Change> results;

                sw.Restart();
                if (group.Tidemark == null || group.Tidemark <= 0)
                {
                    Logger.LogInformation($"Getting all changes for group: {group.Group}");
                    var query = DynamoDB.QueryAsync<Change>(group.Group, new DynamoDBOperationConfig { OverrideTableName = $"{app.Id}-Change", IndexName = "Group-Tidemark-index" });
                    results = await query.GetNextSetAsync();
                }
                else
                {
                    Logger.LogInformation($"Getting changes for group: {group.Group} after tidemark: {group.Tidemark}");
                    var query = DynamoDB.QueryAsync<Change>(group.Group, QueryOperator.GreaterThan, new[] { (object)group.Tidemark.Value }, new DynamoDBOperationConfig { OverrideTableName = $"{app.Id}-Change", IndexName = "Group-Tidemark-index" });
                    results = await query.GetNextSetAsync();
                }
                Logger.LogInformation($"Retrieved changes from DynamoDB in {sw.ElapsedMilliseconds}ms count: {results.Count}");

                if (results != null && results.Any())
                {
                    response.Groups.Add(new SyncResponseViewModel.GroupViewModel()
                    {
                        Group = group.Group,
                        Tidemark = results.Last().Tidemark,
                        Changes = results.Select(r => new SyncResponseViewModel.ChangeViewModel()
                        {
                            Modified = r.Modified,
                            Value = r.Value,
                            Path = r.Path
                        }).ToList()
                    });
                }
            }

            sw.Stop();

            return response;
        }
        
        private JsonResult JsonResultWithValidationErrors(BaseValidationViewModel response)
        {
            if (response == null)
                return JsonResultWithValidationErrors(new BaseValidationViewModel());

            if (!ModelState.IsValid)
                response.Errors = ModelState.Values.SelectMany(ms => ms.Errors).Select(me => me.ErrorMessage);

            response.Success = (response.Errors == null);

            return new JsonResult(response, new JsonSerializerSettings { Formatting = Formatting.Indented });
        }

    }
}
