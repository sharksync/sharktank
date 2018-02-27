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
        public Guid ApplicationId { get; set; }

        [DynamoDBRangeKey]
        public Guid Id { get; set; }
    }
}
