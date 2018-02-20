using SharkTank.Interfaces.Entities;
using System;
using System.Threading.Tasks;

namespace SharkTank.Interfaces.Repositories
{
    public interface IDeviceRepository
    {
        Task<IDevice> GetByIdAsync(Guid id);
    }
}
