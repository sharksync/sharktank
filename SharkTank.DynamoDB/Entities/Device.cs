using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;
using SharkTank.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkTank.Repositories.Entities
{
    public class Device : IDevice
    {
        [DynamoDBHashKey]
        public Guid Id { get; set; }
        [DynamoDBProperty]
        public Guid AppId { get; set; }
        [DynamoDBProperty]
        public Guid AccountId { get; set; }
        [DynamoDBProperty]
        public Guid SyncId { get; set; }
        [DynamoDBProperty]
        public string LastSeen { get; set; }
    }
}
