using SharkSync.Interfaces.Entities;
using System;
using System.Threading.Tasks;

namespace SharkSync.Interfaces.Repositories
{
    public interface IAccountRepository
    {
        Task<IAccount> GetByIdAsync(Guid id);
        Task<IAccount> AddOrGetAsync(string name, string emailAddress, string avatarUrl, string gitHubId = null, string googleId = null, string microsoftId = null);
    }
}
