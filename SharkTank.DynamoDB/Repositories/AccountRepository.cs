using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using SharkTank.DynamoDB.Utilities;
using SharkTank.Interfaces.Entities;
using SharkTank.Interfaces.Repositories;
using SharkTank.Repositories;
using SharkTank.Repositories.Entities;
using System;
using System.Collections.Generic;
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

        public async Task<IAccount> AddAsync(string name)
        {
            return await Task.FromResult((IAccount)null);
        }
    }
}
