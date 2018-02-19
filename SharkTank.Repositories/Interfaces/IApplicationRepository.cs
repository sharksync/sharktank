using SharkTank.Repositories.Entities;
using System;
using System.Threading.Tasks;

namespace SharkTank.Repositories
{
    public interface IApplicationRepository
    {
        Task<Application> GetByIdAsync(Guid id);
    }
}
