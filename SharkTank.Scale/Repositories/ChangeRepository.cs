using Microsoft.Extensions.Logging;
using SharkTank.Interfaces.Entities;
using SharkTank.Interfaces.Repositories;
using SharkTank.Scale.Entities;
using SharkTank.Scale.ScaleApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharkTank.Scale.Repositories
{
    public class ChangeRepository : IChangeRepository
    {
        ILogger Logger { get; set; }

        ScaleContext ScaleContext { get; set; }

        QueryCache Cache { get; set; }

        public ChangeRepository(ILogger<ChangeRepository> logger, ScaleContext scaleContext, QueryCache queryCache)
        {
            Logger = logger;
            ScaleContext = scaleContext;
            Cache = queryCache;
        }

        public IChange CreateChange(Guid recordId, string group, string path, Guid deviceId, DateTime modifiedDateTime, string value)
        {
            return new Change
            {
                Id = Guid.NewGuid(),
                RecordId = recordId,
                Group = group,
                Path = path,
                DeviceId = deviceId,
                Modified = modifiedDateTime,
                Value = value,
                Tidemark = "%clustertime%"
            };
        }

        public async Task UpsertChangesAsync(Guid appId, IEnumerable<IChange> changes)
        {
            foreach (var batch in changes.Batch(50))
            {
                var models = batch.Select(c => ScaleContext.MakeUpsertModel($"{appId}-{c.Group}", "change", (Change)c)).ToList();
                await ScaleContext.UpsertBulk(models);
            }
        }

        public async Task<List<IChange>> ListChangesAsync(Guid appId, string group, string tidemark)
        {
            string partition = $"{appId}-{group}";
            var queryParams = new List<object>();
            string whereClause = null;

            if (!string.IsNullOrWhiteSpace(tidemark))
            {
                Logger.LogInformation($"Getting changes for group: {group} after tidemark: {tidemark}");

                queryParams.Add(tidemark);
                whereClause = "tidemark > ?";
            }
            else
                Logger.LogInformation($"Getting all changes for group: {group}");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            List<Change> results = await ScaleContext.Query<Change>(partition, "change", whereClause, queryParams, orderBy: "tidemark", limit: 50);

            Logger.LogInformation($"Retrieved changes from database in {sw.ElapsedMilliseconds}ms count: {results?.Count}");

            sw.Stop();

            return results.Cast<IChange>().ToList();
        }
    }
}
