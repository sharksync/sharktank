using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using SharkSync.DynamoDB.Utilities;
using SharkSync.Interfaces.Entities;
using SharkSync.Interfaces.Repositories;
using SharkSync.Repositories;
using SharkSync.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharkSync.DynamoDB.Repositories
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

        public async Task<IApplication> AddAsync(string name, Guid accountId)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("You must provide a value for name", nameof(name));

            Application newApp = new Application() { Id = Guid.NewGuid(), Name = name, AccessKey = Guid.NewGuid(), AccountId = accountId };
            await DynamoDBContext.SaveAsync(newApp);
            return newApp;
        }

        public async Task DeleteAsync(Guid id)
        {
            await DynamoDBContext.DeleteAsync<Application>(id);
        }
    }
}
