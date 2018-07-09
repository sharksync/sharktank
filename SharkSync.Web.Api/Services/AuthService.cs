using Microsoft.Extensions.Logging;
using SharkSync.Interfaces;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SharkSync.Web.Api.Services
{
    public class AuthService
    {
        ILogger Logger { get; set; }

        IAccountRepository AccountRepository { get; set; }

        private IAccount loggedInAccount;

        public AuthService(ILogger<AuthService> logger, IAccountRepository accountRepository)
        {
            Logger = logger;
            AccountRepository = accountRepository;
        }

        public async Task<IAccount> GetLoggedInAccountAsync(ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated == true)
            {
                var accountIdString = user.FindFirst(c => c.Type == ClaimTypes.PrimarySid)?.Value;

                if (!Guid.TryParse(accountIdString, out Guid accountId))
                    throw new Exception("Failed to parse accountId from claims ticket");

                if (loggedInAccount != null)
                {
                    // Double check the cached user matches the principal user
                    if (loggedInAccount.Id != accountId)
                        throw new Exception("Cached user does not match cookie");

                    return loggedInAccount;
                }
                else
                {
                    loggedInAccount = await AccountRepository.GetByIdAsync(accountId);

                    return loggedInAccount;
                }
            }

            return null;
        }
    }
}
