﻿using Amazon.DynamoDBv2.DataModel;
using SharkSync.Interfaces.Entities;
using System;

namespace SharkSync.Repositories.Entities
{
    public class Change : IChange
    {
        [DynamoDBHashKey]
        public string Group { get; set; }

        [DynamoDBRangeKey]
        public long Tidemark { get; set; }

        [DynamoDBProperty]
        public Guid RecordId { get; set; }

        [DynamoDBProperty]
        public string Path { get; set; }

        [DynamoDBProperty]
        public DateTime Modified { get; set; }

        [DynamoDBProperty]
        public string Value { get; set; }
    }
}
