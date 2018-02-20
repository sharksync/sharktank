using SharkTank.Interfaces.Entities;
using System;
using System.Threading.Tasks;

namespace SharkTank.Interfaces.Repositories
{
    public interface IApplicationRepository
    {
        Task<IApplication> GetByIdAsync(Guid id);
    }
}
