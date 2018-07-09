using Microsoft.Extensions.Logging;
using SharkSync.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SharkSync.PostgreSQL.Entities;

namespace SharkSync.PostgreSQL.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        ILogger Logger { get; set; }

        DataContext DataContext { get; set; }
        
        public AccountRepository(ILogger<AccountRepository> logger, DataContext dataContext)
        {
            Logger = logger;
            DataContext = dataContext;
        }

        public async Task<IAccount> GetByIdAsync(Guid id)
        {
            return await DataContext.Accounts.SingleOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IAccount> AddOrGetAsync(string name, string emailAddress, string avatarUrl, string gitHubId = null, string googleId = null, string microsoftId = null)
        {
            if (gitHubId == null && googleId == null && microsoftId == null)
                throw new Exception("GitHubId, GoogleId or MicrosoftId required for new accounts");

            Account account = null;

            if (gitHubId != null)
                account = await DataContext.Accounts.SingleOrDefaultAsync(a => a.GitHubId == gitHubId);
            else if (googleId != null)
                account = await DataContext.Accounts.SingleOrDefaultAsync(a => a.GoogleId == googleId);
            else if (microsoftId != null)
                account = await DataContext.Accounts.SingleOrDefaultAsync(a => a.MicrosoftId == microsoftId);

            if (account == null)
            {
                account = new Account()
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    EmailAddress = emailAddress,
                    AvatarUrl = avatarUrl,
                    GitHubId = gitHubId,
                    GoogleId = googleId,
                    MicrosoftId = microsoftId
                };

                DataContext.Add(account);
                await DataContext.SaveChangesAsync();
            }
            
            return account;
        }
    }
}
