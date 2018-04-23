using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;
using SharkSync.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.Repositories.Entities
{
    public class Application : IApplication
    {
        [DynamoDBHashKey]
        public Guid Id { get; set; }

        [DynamoDBProperty]
        public Guid AccessKey { get; set; }

        [DynamoDBProperty]
        public Guid AccountId { get; set; }

        [DynamoDBProperty]
        public string Name { get; set; }
    }
}
