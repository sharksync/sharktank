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

namespace AWSServerless.Controllers
{
    [Route("sync")]
    public class SyncController : Controller
    {
        ILogger Logger { get; set; }

        DynamoDBContext DynamoDB { get; set; }

        DateTime requestStartTimeUTC { get; set; }

        public SyncController(ILogger<SyncController> logger)
        {
            Logger = logger;

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            DynamoDB = new DynamoDBContext(new AmazonDynamoDBClient(RegionEndpoint.EUWest1), config);
        }

        [HttpPost]
        public async Task<JsonResult> Post([FromBody]SyncRequestViewModel request)
        {
            requestStartTimeUTC = DateTime.UtcNow;
            var response = new SyncResponseViewModel();

            try
            {
                Logger.LogInformation($"Sync POST request: {JsonConvert.SerializeObject(request)}");

                var app = await ValidateApplication(request);

                if (!ModelState.IsValid)
                    return JsonResultWithValidationErrors(response);
                
                var device = await ValidateDevice(request);

                if (!ModelState.IsValid)
                    return JsonResultWithValidationErrors(response);

                await ProcessChanges(app, device, request);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to complete Sync Post: {ex.ToString()}");
                ModelState.AddModelError("", $"Unhandled exception: {ex.ToString()}");
            }

            return JsonResultWithValidationErrors(response);
        }

        private async Task<Application> ValidateApplication(SyncRequestViewModel request)
        {
            if (request == null || request.AppId == Guid.Empty)
                ModelState.AddModelError("app_id", "app_id missing or invalid request");
            else
            {
                Logger.LogInformation($"Getting app for id: {request.AppId}");
                var app = await DynamoDB.LoadAsync<Application>(request.AppId);

                if (app == null)
                    ModelState.AddModelError("app_id", "No application found for app_id");
                else
                {
                    Logger.LogInformation($"Found app: {JsonConvert.SerializeObject(app)}");

                    if (app.ApiAccessKey != request.AppApiAccessKey)
                        ModelState.AddModelError("app_api_access_key", "app_api_access_key incorrect for app_id");
                }

                return app;
            }

            return null;
        }

        private async Task<Device> ValidateDevice(SyncRequestViewModel request)
        {
            Logger.LogInformation($"Getting device for id: {request.DeviceId}");
            var device = await DynamoDB.LoadAsync<Device>(request.DeviceId);

            if (device == null)
                ModelState.AddModelError("device_id", "No device found for device_id");
            else
            {
                Logger.LogInformation($"Found device: {JsonConvert.SerializeObject(device)}");

                device.LastSeen = DateTime.UtcNow;
                await DynamoDB.SaveAsync(device);
            }

            return device;
        }

        private async Task ProcessChanges(Application app, Device device, SyncRequestViewModel request)
        {
            var batchWrites = new Dictionary<string, BatchWrite<Change>>();

            if (request.Changes != null && request.Changes.Any())
            {
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
                            var parts = change.Path.Split('/');
                            recordId = Guid.Parse(parts[0]);
                            path = parts[1];
                        }

                        DateTime modifiedUTC = requestStartTimeUTC.AddSeconds(-change.SecondsAgo);

                        var dbChange = new Change()
                        {
                            Tidemark = DateTime.UtcNow,
                            DeviceId = device.Id,
                            RecordId = recordId,
                            Path = path,
                            Group = change.Group,
                            Modified = modifiedUTC,
                            Value = change.Value
                        };
                        
                        string tableName = $"{app.Id}-Change";
                        BatchWrite<Change> batchWrite = null;

                        if (!batchWrites.TryGetValue(tableName, out batchWrite))
                        {
                            batchWrite = DynamoDB.CreateBatchWrite<Change>(new DynamoDBOperationConfig { OverrideTableName = tableName });
                            batchWrites.Add(tableName, batchWrite);
                        }

                        batchWrite.AddPutItem(dbChange);
                    }

                    await DynamoDB.ExecuteBatchWriteAsync(batchWrites.Values.ToArray());
                }
            }
        }

        private async Task<Device> GetChanges(Application app, Device device, SyncRequestViewModel request)
        {
            //Logger.LogInformation($"Getting device for id: {request.DeviceId}");
            //var device = await DynamoDB.LoadAsync<Change>(request.DeviceId, new DynamoDBOperationConfig { OverrideTableName = $"{app.Id}" });

            //if (device == null)
            //    ModelState.AddModelError("device_id", "No device found for device_id");
            //else
            //{
            //    Logger.LogInformation($"Found device: {JsonConvert.SerializeObject(device)}");

            //    device.LastSeen = DateTime.UtcNow;
            //    await DynamoDB.SaveAsync(device);
            //}

            //return device;
        }


        //var query = "SELECT * FROM change";
        //var params = [];

        //if (tidemark != "" && tidemark != undefined) {

        //    query += " WHERE tidemark > ?";
        //    params.push(tidemark);
        //}

        //query += " ORDER BY tidemark LIMIT 20";

        //Scale.query(environment, appId + group, query, params)
        //    .then(function (data) {

        //        if (data.error != null)
        //            return reject("Querying change table failed with: " + data.error);

        //        resolve({ group: group, changes: data.results });
        //    })
        //    .catch(err => {
        //        return reject(err);
        //    });

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
