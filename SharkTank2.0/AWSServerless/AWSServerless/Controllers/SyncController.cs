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

        IDynamoDBContext DynamoDB { get; set; }

        public SyncController(ILogger<SyncController> logger)
        {
            Logger = logger;

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            DynamoDB = new DynamoDBContext(new AmazonDynamoDBClient(RegionEndpoint.EUWest1), config);
        }

        [HttpPost]
        public async Task<JsonResult> Post([FromBody]SyncRequestViewModel request)
        {
            var response = new SyncResponseViewModel();

            try
            {
                Logger.LogInformation($"Sync POST request: {JsonConvert.SerializeObject(request)}");

                var app = await ValidateApplication(request);

                if (!ModelState.IsValid)
                    return JsonResultWithValidationErrors(response);


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
                    ModelState.AddModelError("app_id", "no application found for app_id");
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
