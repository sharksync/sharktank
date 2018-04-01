using Microsoft.Extensions.Logging;
using SharkTank.Interfaces.Entities;
using SharkTank.Interfaces.Repositories;
using SharkTank.Scale.Entities;
using SharkTank.Scale.ScaleApi;
using System;
using System.Collections.Generic;
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

        public async Task<IApplication> GetByIdAsync(Guid id)
        {
            return await Cache.GetByPrimaryKeyFromCacheOrQuery<Application>(ScaleContext.SystemPartition, "application", "app_id", id);
        }

        public Task<IEnumerable<IApplication>> ListByAccountIdAsync(Guid accountId)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
