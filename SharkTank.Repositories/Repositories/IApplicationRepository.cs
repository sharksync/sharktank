using SharkTank.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharkTank.Interfaces.Repositories
{
    public interface IApplicationRepository
    {
        Task<IEnumerable<IApplication>> ListByAccountIdAsync(Guid accountId);
        Task<IApplication> GetByIdAsync(Guid id);
        Task DeleteAsync(Guid id);
    }
}
