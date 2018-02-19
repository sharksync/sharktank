using SharkTank.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharkTank.Repositories
{
    public interface IChangeRepository
    {
        Task UpsertChangesAsync(Guid appId, IEnumerable<ChangeWithGroup> changes);
        Task<List<Change>> ListChangesAsync(Guid appId, string group, string tidemark);
    }
}
