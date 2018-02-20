using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;
using SharkTank.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkTank.Repositories.Entities
{
    public class Change : IChange
    {
        [DynamoDBHashKey]
        public Guid Id { get; set; }
        [DynamoDBProperty]
        public Guid RecordId { get; set; }
        [DynamoDBProperty]
        public string Path { get; set; }
        [DynamoDBProperty]
        public Guid DeviceId { get; set; }
        [DynamoDBProperty]
        public string Tidemark { get; set; }
        [DynamoDBProperty]
        public DateTime Modified { get; set; }
        [DynamoDBProperty]
        public string Value { get; set; }

        public string Group { get; set; }
    }
}
