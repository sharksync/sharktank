using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharkSync.Api.ViewModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using SharkTank.Repositories.Entities;
using SharkTank.Repositories;

namespace SharkSync.Api.Controllers
{
    [Route("sync")]
    public class SyncController : Controller
    {
        ILogger Logger { get; set; }

        IApplicationRepository ApplicationRepository { get; set; }

        IDeviceRepository DeviceRepository { get; set; }

        IChangeRepository ChangeRepository { get; set; }

        DateTime requestStartTimeUTC { get; set; }

        public SyncController(ILogger<SyncController> logger, IApplicationRepository appRepository, IDeviceRepository deviceRepository, IChangeRepository changeRepository)
        {
            Logger = logger;
            ApplicationRepository = appRepository;
            DeviceRepository = deviceRepository;
            ChangeRepository = changeRepository;
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
                var app = await ApplicationRepository.GetByIdAsync(request.AppId);

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
            var device = await DeviceRepository.GetByIdAsync(request.DeviceId);

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
                    var changes = new List<ChangeWithGroup>();

                    foreach (var change in batch)
                    {
                        if (change != null)
                        {
                            Guid recordId = Guid.Empty;
                            string path = null;

                            // Path should contain a / in format <guid>/property.name
                            if (!string.IsNullOrWhiteSpace(change.Path) && change.Path.IndexOf("/") > -1)
                            {
                                recordId = Guid.Parse(change.Path.Substring(0, change.Path.IndexOf("/")));
                                path = change.Path.Substring(change.Path.IndexOf("/") + 1);

                                DateTime modifiedUTC = requestStartTimeUTC.AddSeconds(-change.SecondsAgo);

                                var dbChange = new ChangeWithGroup()
                                {
                                    Id = Guid.NewGuid(),
                                    RecordId = recordId,
                                    Path = path,
                                    DeviceId = device.Id,
                                    Modified = modifiedUTC,
                                    Tidemark = "%clustertime%",
                                    Value = change.Value,
                                    Group = change.Group
                                };

                                changes.Add(dbChange);
                            }
                            else
                            {
                                ModelState.AddModelError("Path", "Path is incorrectly formatted, should be formatted <guid>/property.name");
                            }
                        }
                    }

                    Logger.LogInformation($"Generated changes in {sw.ElapsedMilliseconds}ms count: {changes.Count}");

                    if (changes.Any())
                    {
                        sw.Restart();

                        await ChangeRepository.UpsertChangesAsync(app.Id, changes);

                        Logger.LogInformation($"Saved changes to database in {sw.ElapsedMilliseconds}ms count: {changes.Count}");
                    }
                }

                sw.Stop();
            }
        }

        private async Task<SyncResponseViewModel> GetChanges(Application app, Device device, SyncRequestViewModel request)
        {
            var response = new SyncResponseViewModel() { Groups = new List<SyncResponseViewModel.GroupViewModel>() };

            if (request.Groups != null)
            {
                foreach (var group in request.Groups)
                {
                    List<Change> results = await ChangeRepository.ListChangesAsync(app.Id, group.Group, group.Tidemark);

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
            }

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
