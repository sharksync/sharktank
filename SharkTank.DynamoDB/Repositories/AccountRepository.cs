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

        public async Task<IAccount> AddOrGetAsync(string name, string emailAddress, int? githubId, string avatarUrl)
        {
            if (githubId == null)
                throw new Exception("GithubId required for new accounts");

            var scanCondition = new ScanCondition(nameof(Account.GithubId), ScanOperator.Equal, githubId);
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
                    GithubId = githubId,
                    AvatarUrl = avatarUrl
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
