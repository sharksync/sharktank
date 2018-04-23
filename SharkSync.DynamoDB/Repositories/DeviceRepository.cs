using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Logging;
using SharkSync.Interfaces.Entities;
using SharkSync.Interfaces.Repositories;
using SharkSync.Repositories;
using SharkSync.Repositories.Entities;
using System;
using System.Threading.Tasks;

namespace SharkSync.DynamoDB.Repositories
{
    public class DeviceRepository : IDeviceRepository
    {
        ILogger Logger { get; set; }

        IAmazonDynamoDB DynamoDBClient { get; set; }

        DynamoDBContext DynamoDBContext { get; set; }

        public DeviceRepository(ILogger<DeviceRepository> logger, IAmazonDynamoDB dynamoDBClient)
        {
            Logger = logger;
            DynamoDBClient = dynamoDBClient;
            DynamoDBContext = new DynamoDBContext(dynamoDBClient);
        }

        public async Task<IDevice> GetByIdAsync(Guid appId, Guid deviceId)
        {
            return await DynamoDBContext.LoadAsync<Device>(appId, deviceId);
        }
    }
}
