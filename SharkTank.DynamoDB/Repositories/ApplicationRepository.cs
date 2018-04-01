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
using System.Threading.Tasks;

namespace SharkTank.DynamoDB.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        ILogger Logger { get; set; }

        IAmazonDynamoDB DynamoDBClient { get; set; }

        DynamoDBContext DynamoDBContext { get; set; }

        public ApplicationRepository(ILogger<ApplicationRepository> logger, IAmazonDynamoDB dynamoDBClient)
        {
            Logger = logger;
            DynamoDBClient = dynamoDBClient;
            DynamoDBContext = new DynamoDBContext(dynamoDBClient);
        }

        public async Task<IEnumerable<IApplication>> ListByAccountIdAsync(Guid accountId)
        {
            var scanCondition = new ScanCondition(nameof(Application.AccountId), ScanOperator.Equal, accountId);
            var query = DynamoDBContext.ScanAsync<Application>(new[] { scanCondition });
            var apps = await query.GetNextSetAsync();
            return apps;
        }

        public async Task<IApplication> GetByIdAsync(Guid id)
        {
            return await DynamoDBContext.LoadAsync<Application>(id);
        }

        public async Task DeleteAsync(Guid id)
        {
            await DynamoDBContext.DeleteAsync<Application>(id);
        }
    }
}
