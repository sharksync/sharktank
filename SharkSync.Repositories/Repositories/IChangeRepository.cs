using SharkSync.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharkSync.Interfaces.Repositories
{
    public interface IChangeRepository
    {
        IChange CreateChange(Guid accountId, Guid appId, Guid recordId, string groupId, string entity, DateTime modifiedDateTime, string value);
        Task UpsertChangesAsync(Guid appId, IEnumerable<IChange> changes);
        Task<List<IChange>> ListChangesAsync(Guid appId, string groupId, long? tidemark);
    }
}
