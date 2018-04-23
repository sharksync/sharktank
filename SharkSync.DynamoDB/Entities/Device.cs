using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;
using SharkSync.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.Repositories.Entities
{
    public class Device : IDevice
    {
        [DynamoDBHashKey]
        public Guid ApplicationId { get; set; }

        [DynamoDBRangeKey]
        public Guid Id { get; set; }
    }
}
