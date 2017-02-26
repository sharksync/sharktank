using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerless
{
    public class DynamoDbCache
    {
        ILogger Logger { get; set; }

        IDynamoDBContext DynamoDB { get; set; }

        IMemoryCache Cache { get; set; }

        public DynamoDbCache(ILogger<DynamoDbCache> logger, IDynamoDBContext dynamoDB, IMemoryCache cache)
        {
            Logger = logger;
            DynamoDB = dynamoDB;
            Cache = cache;
        }

        public async Task<T> GetFromCacheOrDynamoDb<T>(Guid id) where T : class
        {
            string typeName = typeof(T).Name;

            Logger.LogInformation($"Getting {typeName} for id: {id}");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            T item;
            string key = $"{typeName}-{id}";
            if (!Cache.TryGetValue(key, out item))
            {
                item = await DynamoDB.LoadAsync<T>(id);
                Logger.LogInformation($"Retrieved {typeName} from DynamoDB in {sw.ElapsedMilliseconds}ms");

                Cache.Set(key, item);
            }

            Logger.LogInformation($"Retrieved {typeName} in {sw.ElapsedMilliseconds}ms");
            sw.Stop();

            return item;
        }
    }
}
