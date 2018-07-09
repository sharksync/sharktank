using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharkSync.Interfaces;
using SharkSync.PostgreSQL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.PostgreSQL.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        ILogger Logger { get; set; }
        
        DataContext DataContext { get; set; }
        
        public ApplicationRepository(ILogger<ApplicationRepository> logger, DataContext dataContext)
        {
            Logger = logger;
            DataContext = dataContext;
        }

        public async Task<IEnumerable<IApplication>> ListByAccountIdAsync(Guid accountId)
        {
            return await DataContext.Applications.Where(a => a.AccountId == accountId).ToListAsync();
        }

        public async Task<IApplication> GetByIdAsync(Guid id)
        {
            return await DataContext.Applications.SingleOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IApplication> AddAsync(string name, Guid accountId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("You must provide a value for name", nameof(name));

            Application app = new Application() { Id = Guid.NewGuid(), Name = name, AccessKey = Guid.NewGuid(), AccountId = accountId };
            DataContext.Add(app);
            await DataContext.SaveChangesAsync();

            return app;
        }

        public async Task DeleteAsync(Guid id)
        {
            var app = await DataContext.Applications.SingleOrDefaultAsync(a => a.Id == id);
            if (app != null)
            {
                DataContext.Remove(app);
                await DataContext.SaveChangesAsync();
            }
        }
    }
}
