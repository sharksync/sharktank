using Microsoft.Extensions.Logging;
using SharkTank.Repositories;
using SharkTank.Repositories.Entities;
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

        public async Task<Device> GetByIdAsync(Guid id)
        {
            return await Cache.GetByPrimaryKeyFromCacheOrQuery<Device>(ScaleContext.SystemPartition, "device", "device_id", id);
        }
    }
}
