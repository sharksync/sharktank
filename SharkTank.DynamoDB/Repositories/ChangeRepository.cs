using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using SharkTank.Interfaces.Entities;
using SharkTank.Interfaces.Repositories;
using SharkTank.Repositories;
using SharkTank.Repositories.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SharkTank.DynamoDB.Repositories
{
    public class ChangeRepository : IChangeRepository
    {
        ILogger Logger { get; set; }

        IAmazonDynamoDB DynamoDBClient { get; set; }

        DynamoDBContext DynamoDBContext { get; set; }

        public ChangeRepository(ILogger<ChangeRepository> logger, IAmazonDynamoDB dynamoDBClient)
        {
            Logger = logger;
            DynamoDBClient = dynamoDBClient;
            DynamoDBContext = new DynamoDBContext(dynamoDBClient);
        }

        public IChange CreateChange(Guid recordId, string group, string path, Guid deviceId, DateTime modifiedDateTime, string value)
        {
            return new Change
            {
                RecordId = recordId,
                Group = group,
                Path = path,
                DeviceId = deviceId,
                Modified = modifiedDateTime,
                Value = value,
                Tidemark = HiResDateTime.UtcNowTicks
            };
        }

        public async Task UpsertChangesAsync(Guid appId, IEnumerable<IChange> changes)
        {
            var tableConfig = GetChangeTableConfig(appId);
            var changeBatch = DynamoDBContext.CreateBatchWrite<Change>(tableConfig);

            changeBatch.AddPutItems(changes.Cast<Change>());

            await changeBatch.ExecuteAsync();
        }

        public async Task<List<IChange>> ListChangesAsync(Guid appId, string group, string tidemark)
        {
            var tableConfig = GetChangeTableConfig(appId);
            AsyncSearch<Change> query;

            if (string.IsNullOrWhiteSpace(tidemark) || !long.TryParse(tidemark, out long tidemarkLong) || tidemarkLong <= 0)
            {
                Logger.LogInformation($"Getting all changes for group: {group}");

                query = DynamoDBContext.QueryAsync<Change>(group, tableConfig);
            }
            else
            {
                Logger.LogInformation($"Getting changes for group: {group} after tidemark: {tidemarkLong}");

                query = DynamoDBContext.QueryAsync<Change>(group, QueryOperator.GreaterThan, new[] { (object)tidemarkLong }, tableConfig);
            }

            var changes = await query.GetNextSetAsync();

            // Limit changes to 50 at a time
            return changes.Take(50).Cast<IChange>().ToList();
        }

        public async Task CreateChangeTableForApp(Guid appId)
        {
            await DynamoDBClient.CreateTableAsync(
                GetChangeTableName(appId), 
                new List<KeySchemaElement>()
                {
                    new KeySchemaElement()
                    {
                        KeyType = KeyType.HASH,
                        AttributeName = nameof(Change.Group),
                    },
                    new KeySchemaElement()
                    {
                        KeyType = KeyType.RANGE,
                        AttributeName = nameof(Change.Tidemark),
                    }
                }, 
                new List<AttributeDefinition>()
                {
                    new AttributeDefinition()
                    {
                        AttributeName = nameof(Change.Group),
                        AttributeType = ScalarAttributeType.S,
                    },
                    new AttributeDefinition()
                    {
                        AttributeName = nameof(Change.Tidemark),
                        AttributeType = ScalarAttributeType.S,
                    }
                }, 
                new ProvisionedThroughput() { ReadCapacityUnits = 1, WriteCapacityUnits = 1 });
        }

        public async Task DeleteChangeTableForApp(Guid appId)
        {
            await DynamoDBClient.DeleteTableAsync(GetChangeTableName(appId));
        }

        private static string GetChangeTableName(Guid appId)
        {
            return $"{appId}-Change";
        }

        private static DynamoDBOperationConfig GetChangeTableConfig(Guid appId)
        {
            return new DynamoDBOperationConfig { OverrideTableName = GetChangeTableName(appId) };
        }
    }
}
