using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using SharkTank.DynamoDB.Utilities;
using SharkTank.Interfaces.Entities;
using SharkTank.Interfaces.Repositories;
using SharkTank.Repositories;
using SharkTank.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkTank.DynamoDB.Repositories
{
    public class AccountRepository : IAccountRepository
    {
        ILogger Logger { get; set; }

        IAmazonDynamoDB DynamoDBClient { get; set; }

        DynamoDBContext DynamoDBContext { get; set; }

        public AccountRepository(ILogger<ApplicationRepository> logger, IAmazonDynamoDB dynamoDBClient)
        {
            Logger = logger;
            DynamoDBClient = dynamoDBClient;
            DynamoDBContext = new DynamoDBContext(dynamoDBClient);
        }

        public async Task<IAccount> GetByIdAsync(Guid id)
        {
            return await DynamoDBContext.LoadAsync<Account>(id);
        }

        public async Task<IAccount> AddOrGetAsync(string name, string emailAddress, string avatarUrl, string gitHubId = null, string googleId = null, string microsoftId = null)
        {
            if (gitHubId == null && googleId == null && microsoftId == null)
                throw new Exception("GitHubId, GoogleId or MicrosoftId required for new accounts");

            ScanCondition scanCondition = null;

            if (gitHubId != null)
                scanCondition = new ScanCondition(nameof(Account.GitHubId), ScanOperator.Equal, gitHubId);
            else if (googleId != null)
                scanCondition = new ScanCondition(nameof(Account.GoogleId), ScanOperator.Equal, googleId);
            else if (microsoftId != null)
                scanCondition = new ScanCondition(nameof(Account.MicrosoftId), ScanOperator.Equal, microsoftId);

            var query = DynamoDBContext.ScanAsync<Account>(new[] { scanCondition });
            var accounts = await query.GetNextSetAsync();

            Account account = null;

            if (!accounts.Any())
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
                await DynamoDBContext.SaveAsync(account);
            }
            else
            {
                account = accounts.First();
            }

            return account;
        }
    }
}
