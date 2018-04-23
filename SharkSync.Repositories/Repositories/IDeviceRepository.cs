using SharkSync.Interfaces.Entities;
using System;
using System.Threading.Tasks;

namespace SharkSync.Interfaces.Repositories
{
    public interface IDeviceRepository
    {
        Task<IDevice> GetByIdAsync(Guid appId, Guid deviceId);
    }
}
