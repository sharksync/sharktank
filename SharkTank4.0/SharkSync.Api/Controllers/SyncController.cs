using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharkSync.Api.ViewModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using SharkSync.Api.Scale;
using SharkSync.Api.Scale.Tables;

namespace SharkSync.Api.Controllers
{
    [Route("sync")]
    public class SyncController : Controller
    {
        public static readonly string SystemPartition = "shark_sync";
        public static readonly TimeSpan defaultCacheDuration = new TimeSpan(hours: 0, minutes: 10, seconds: 0);

        ILogger Logger { get; set; }

        IQueryCache Cache { get; set; }

        IScaleContext ScaleContext { get; set; }

        DateTime requestStartTimeUTC { get; set; }

        public SyncController(ILogger<SyncController> logger, IQueryCache cache, IScaleContext scaleContext)
        {
            Logger = logger;
            Cache = cache;
            ScaleContext = scaleContext;
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

                //sw.Restart();
                //UpdateDeviceLastSeen(device);
                //Logger.LogInformation($"UpdateDeviceLastSeen in {sw.ElapsedMilliseconds}ms");

                sw.Stop();
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
                var app = await Cache.GetByPrimaryKeyFromCacheOrQuery<Application>(SystemPartition, "application", "app_id", request.AppId, defaultCacheDuration);

                if (app == null)
                    ModelState.AddModelError("app_id", "No application found for app_id");
                else if (app.AccessKey != request.AppApiAccessKey)
                    ModelState.AddModelError("app_api_access_key", "app_api_access_key incorrect for app_id");

                return app;
            }

            return null;
        }

        private async Task<Device> ValidateDevice(SyncRequestViewModel request)
        {
            var device = await Cache.GetByPrimaryKeyFromCacheOrQuery<Device>(SystemPartition, "device", "device_id", request.DeviceId, defaultCacheDuration);

            if (device == null)
                ModelState.AddModelError("device_id", "No device found for device_id");

            return device;
        }

        private async Task ProcessChanges(Application app, Device device, SyncRequestViewModel request)
        {
            if (request.Changes != null && request.Changes.Any())
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                foreach (var batch in request.Changes.Batch(50))
                {
                    var changes = new List<SendContextModel<UpsetModel<Change>>>();

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
                            Id = Guid.NewGuid(),
                            RecordId = recordId,
                            Path = change.Path,
                            DeviceId = device.Id,
                            Modified = modifiedUTC,
                            Tidemark = "%clustertime%",
                            Value = change.Value
                        };

                        string partition = $"{app.Id}-{change.Group}";

                        changes.Add(ScaleContext.MakeUpsertModel(partition, "change", dbChange));
                    }

                    Logger.LogInformation($"Generated changes in {sw.ElapsedMilliseconds}ms count: {changes.Count}");

                    sw.Restart();
                    await ScaleContext.UpsertBulk(changes);
                    Logger.LogInformation($"Saved changes to database in {sw.ElapsedMilliseconds}ms count: {changes.Count}");
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
                sw.Restart();

                string partition = $"{app.Id}-{group.Group}";
                var queryParams = new List<object>();
                string whereClause = null;

                if (!string.IsNullOrWhiteSpace(group.Tidemark))
                {
                    Logger.LogInformation($"Getting changes for group: {group.Group} after tidemark: {group.Tidemark}");
                    queryParams.Add(group.Tidemark);
                    whereClause = "tidemark > ?";
                }
                else
                    Logger.LogInformation($"Getting all changes for group: {group.Group}");

                List<Change> results = await ScaleContext.Query<Change>(partition, "change", whereClause, queryParams, orderBy: "tidemark", limit: 50);

                Logger.LogInformation($"Retrieved changes from database in {sw.ElapsedMilliseconds}ms count: {results.Count}");

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

        //private void UpdateDeviceLastSeen(Device device)
        //{
        //    // Doesn't need to be done before returning to the client so run it later
        //    Task.Run(() =>
        //    {
        //        Stopwatch sw = new Stopwatch();
        //        sw.Start();

        //        device.LastSeen = DateTime.UtcNow;
        //        DynamoDB.SaveAsync(device);

        //        Logger.LogInformation($"UpdateDeviceLastSeen in {sw.ElapsedMilliseconds}ms");
        //        sw.Stop();
        //    });
        //}

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
