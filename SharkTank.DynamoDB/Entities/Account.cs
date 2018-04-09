using Amazon.DynamoDBv2.DataModel;
using Newtonsoft.Json;
using SharkTank.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkTank.Repositories.Entities
{
    public class Account : IAccount
    {
        [DynamoDBHashKey]
        public Guid Id { get; set; }

        [DynamoDBProperty]
        public string Name { get; set; }

        [DynamoDBProperty]
        public string EmailAddress { get; set; }

        [DynamoDBProperty]
        public int? GitHubId { get; set; }

        [DynamoDBProperty]
        public string AvatarUrl { get; set; }
    }
}
