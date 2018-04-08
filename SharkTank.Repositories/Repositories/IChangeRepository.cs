using SharkTank.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharkTank.Interfaces.Repositories
{
    public interface IChangeRepository
    {
        IChange CreateChange(Guid recordId, string group, string path, Guid deviceId, DateTime modifiedDateTime, string value);
        Task UpsertChangesAsync(Guid appId, IEnumerable<IChange> changes);
        Task<List<IChange>> ListChangesAsync(Guid appId, string group, string tidemark);
        Task CreateChangeTableForApp(Guid appId);
        Task DeleteChangeTableForApp(Guid appId);
    }
}
