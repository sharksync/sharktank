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
    public class ApplicationRepository : IApplicationRepository
    {
        ILogger Logger { get; set; }

        ScaleContext ScaleContext { get; set; }

        QueryCache Cache { get; set; }

        public ApplicationRepository(ILogger<ApplicationRepository> logger, ScaleContext scaleContext, QueryCache queryCache)
        {
            Logger = logger;
            ScaleContext = scaleContext;
            Cache = queryCache;
        }

        public async Task<Application> GetByIdAsync(Guid id)
        {
            return await Cache.GetByPrimaryKeyFromCacheOrQuery<Application>(ScaleContext.SystemPartition, "application", "app_id", id);
        }
    }
}
