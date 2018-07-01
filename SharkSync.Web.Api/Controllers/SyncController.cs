using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharkSync.Web.Api.ViewModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using SharkSync.Interfaces.Repositories;
using SharkSync.Interfaces.Entities;
using System.Net;

namespace SharkSync.Web.Api.Controllers
{
    [Route("Api/Sync")]
    public class SyncController : Controller
    {
        ILogger Logger { get; set; }

        IApplicationRepository ApplicationRepository { get; set; }

        IChangeRepository ChangeRepository { get; set; }

        DateTime requestStartTimeUTC { get; set; }

        public SyncController(ILogger<SyncController> logger, IApplicationRepository appRepository, IChangeRepository changeRepository)
        {
            Logger = logger;
            ApplicationRepository = appRepository;
            ChangeRepository = changeRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]SyncRequestViewModel request)
        {
            requestStartTimeUTC = DateTime.UtcNow;
            var response = new SyncResponseViewModel();

            if (!ModelState.IsValid)
                return ModelState.GetJsonResultWithValidationErrors(response);

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var app = await ValidateApplication(request);
                Logger.LogInformation($"ValidateApplication in {sw.ElapsedMilliseconds}ms");

                if (!ModelState.IsValid)
                    return ModelState.GetJsonResultWithValidationErrors(response);

                sw.Restart();
                await ProcessChanges(app, request);
                Logger.LogInformation($"ProcessChanges in {sw.ElapsedMilliseconds}ms");

                sw.Restart();
                response = await GetChanges(app, request);
                Logger.LogInformation($"GetChanges in {sw.ElapsedMilliseconds}ms");

                sw.Stop();
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to complete Sync Post: {ex.ToString()}");
                ModelState.AddModelError("", $"Unhandled exception: {ex.ToString()}");
            }

            return ModelState.GetJsonResultWithValidationErrors(response);
        }

        private async Task<IApplication> ValidateApplication(SyncRequestViewModel request)
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

        private async Task ProcessChanges(IApplication app, SyncRequestViewModel request)
        {
            if (request.Changes != null && request.Changes.Any())
            {
                var dbChanges = new List<IChange>();

                Stopwatch sw = new Stopwatch();
                sw.Start();

                foreach (var change in request.Changes)
                {
                    if (change != null)
                    {
                        // Path should contain a / in format <guid>/property.name
                        if (!string.IsNullOrWhiteSpace(change.Path) && change.Path.IndexOf("/") > -1)
                        {
                            Guid recordId = Guid.Parse(change.Path.Substring(0, change.Path.IndexOf("/")));
                            string path = change.Path.Substring(change.Path.IndexOf("/") + 1);
                            DateTime modifiedUTC = requestStartTimeUTC.AddSeconds(-change.SecondsAgo);

                            var dbChange = ChangeRepository.CreateChange(recordId, change.Group, path, modifiedUTC, change.Value);
                            dbChanges.Add(dbChange);
                        }
                        else
                        {
                            ModelState.AddModelError("Path", "Path is incorrectly formatted, should be formatted <guid>/property.name");
                        }
                    }
                }

                Logger.LogInformation($"Generated changes in {sw.ElapsedMilliseconds}ms count: {dbChanges.Count}");

                if (dbChanges.Any())
                {
                    sw.Restart();

                    await ChangeRepository.UpsertChangesAsync(app.Id, dbChanges);

                    Logger.LogInformation($"Saved changes to database in {sw.ElapsedMilliseconds}ms count: {dbChanges.Count}");
                }

                sw.Stop();
            }
        }

        private async Task<SyncResponseViewModel> GetChanges(IApplication app, SyncRequestViewModel request)
        {
            var response = new SyncResponseViewModel() { Groups = new List<SyncResponseViewModel.GroupViewModel>() };

            if (request.Groups != null)
            {
                foreach (var group in request.Groups)
                {
                    List<IChange> results = await ChangeRepository.ListChangesAsync(app.Id, group.Group, group.Tidemark);

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
                                Path = r.RecordId.ToString() + "/" + r.Path
                            }).ToList()
                        });
                    }
                }
            }

            return response;
        }
    }
}
