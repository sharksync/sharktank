using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SharkSync.Web.Api.ViewModels;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Diagnostics;
using SharkSync.Interfaces;
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

        public SyncController(ILogger<SyncController> logger, IApplicationRepository appRepository, IChangeRepository changeRepository, ITimeService timeService)
        {
            Logger = logger;
            ApplicationRepository = appRepository;
            ChangeRepository = changeRepository;
            requestStartTimeUTC = timeService.GetUtcNow();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]SyncRequestViewModel request)
        {
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
                ModelState.AddModelError("AppId", "AppId missing or invalid request");
            else
            {
                var app = await ApplicationRepository.GetByIdAsync(request.AppId);

                if (app == null)
                    ModelState.AddModelError("AppId", "No application found for AppId");
                else if (app.AccessKey != request.AppApiAccessKey)
                    ModelState.AddModelError("AppApiAccessKey", "AppApiAccessKey incorrect for AppId");

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

                // Dedup any changes with the same Record, Entity and Property
                var uniqueChanges = new Dictionary<string, SyncRequestViewModel.ChangeViewModel>();
                foreach (var change in request.Changes)
                {
                    string key = $"{change.RecordId}-{change.Entity}-{change.Property}";
                    if (uniqueChanges.TryGetValue(key, out var existingChange))
                    {
                        if (change.MillisecondsAgo < existingChange.MillisecondsAgo)
                            uniqueChanges[key] = change;
                    }
                    else
                        uniqueChanges.Add(key, change);
                }

                Logger.LogInformation($"Deduped changes in {sw.ElapsedMilliseconds}ms count: {dbChanges.Count}");
                sw.Restart();

                foreach (var change in uniqueChanges.Values)
                {
                    if (change != null)
                    {
                        DateTime modifiedUTC = requestStartTimeUTC.AddMilliseconds(-change.MillisecondsAgo);
                        var millisecondsSinceEpoch = new DateTimeOffset(modifiedUTC).ToUnixTimeMilliseconds();

                        var dbChange = ChangeRepository.CreateChange(app.AccountId, app.Id, change.RecordId, change.Group, change.Entity, change.Property, millisecondsSinceEpoch, change.Value);
                        dbChanges.Add(dbChange);
                    }
                }

                Logger.LogInformation($"Generated changes in {sw.ElapsedMilliseconds}ms count: {dbChanges.Count}");
                sw.Restart();

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
                            Tidemark = results.Last().Id,
                            Changes = results.Select(r => new SyncResponseViewModel.ChangeViewModel()
                            {
                                Modified = r.ClientModified,
                                Value = r.RecordValue,
                                Entity = r.Entity,
                                RecordId = r.RecordId,
                                Property = r.Property
                            }).ToList()
                        });
                    }
                }
            }

            return response;
        }
    }
}
