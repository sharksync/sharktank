using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharkSync.Interfaces.Entities;
using SharkSync.Interfaces.Repositories;
using SharkSync.PostgreSQL.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.PostgreSQL.Repositories
{
    public class ChangeRepository : IChangeRepository
    {
        ILogger Logger { get; set; }

        DataContext DataContext { get; set; }

        public ChangeRepository(ILogger<ChangeRepository> logger, DataContext dataContext)
        {
            Logger = logger;
            DataContext = dataContext;
        }

        public IChange CreateChange(Guid accountId, Guid appId, Guid recordId, string groupId, string entity, DateTime modifiedDateTime, string value)
        {
            return new Change
            {
                AccountId = accountId,
                ApplicationId = appId,
                RecordId = recordId,
                GroupId = groupId,
                Entity = entity,
                ClientModified = modifiedDateTime.Ticks,
                RecordValue = value
            };
        }

        public async Task UpsertChangesAsync(Guid appId, IEnumerable<IChange> changes)
        {
            DataContext.AddRange(changes);

            await DataContext.SaveChangesAsync();
        }

        public async Task<List<IChange>> ListChangesAsync(Guid appId, string groupId, long? tidemark)
        {
            IQueryable<Change> query = DataContext.Changes.Where(c => c.ApplicationId == appId && c.GroupId == groupId);

            if (tidemark > 0)
            {
                Logger.LogInformation($"Getting changes for app: {appId} and group: {groupId} after tidemark: {tidemark}");
                
                query = query.Where(c => c.Id > tidemark);
            }
            else
                Logger.LogInformation($"Getting all changes for app: {appId} and group: {groupId}");

            var changes = await query.ToListAsync();

            // Limit changes to 50 at a time
            return changes.Take(50).Cast<IChange>().ToList();
        }
    }
}
