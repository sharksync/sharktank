using Microsoft.Extensions.Logging;
using SharkTank.Interfaces;
using SharkTank.Interfaces.Entities;
using SharkTank.Interfaces.Repositories;
using SharkTank.Scale.Entities;
using SharkTank.Scale.ScaleApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SharkTank.Scale.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        ILogger Logger { get; set; }

        ScaleContext ScaleContext { get; set; }

        QueryCache Cache { get; set; }

        public DeviceRepository(ILogger<DeviceRepository> logger, ScaleContext scaleContext, QueryCache queryCache)
        {
            Logger = logger;
            ScaleContext = scaleContext;
            Cache = queryCache;
        }

        public async Task<IDevice> GetByIdAsync(Guid id)
        {
            return await Cache.GetByPrimaryKeyFromCacheOrQuery<Device>(ScaleContext.SystemPartition, "device", "device_id", id);
        }
    }
}
