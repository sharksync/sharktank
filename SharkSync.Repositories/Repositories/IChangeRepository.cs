using SharkSync.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharkSync.Interfaces.Repositories
{
    public interface IChangeRepository
    {
        IChange CreateChange(Guid recordId, string group, string path, DateTime modifiedDateTime, string value);
        Task UpsertChangesAsync(Guid appId, IEnumerable<IChange> changes);
        Task<List<IChange>> ListChangesAsync(Guid appId, string group, long? tidemark);
        Task CreateChangeTableForApp(Guid appId);
        Task DeleteChangeTableForApp(Guid appId);
    }
}
