using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SharkTank.Scale.ScaleApi
{
    public class QueryCache
    {
        private static readonly TimeSpan defaultCacheDuration = new TimeSpan(hours: 0, minutes: 10, seconds: 0);

        ILogger Logger { get; set; }

        IMemoryCache Cache { get; set; }

        ScaleContext ScaleContext { get; set; }

        public QueryCache(ILogger<QueryCache> logger, IMemoryCache cache, ScaleContext scaleContext)
        {
            Logger = logger;
            Cache = cache;
            ScaleContext = scaleContext;
        }

        public async Task<T> GetByPrimaryKeyFromCacheOrQuery<T>(string partition, string table, string primaryKey, Guid id) where T : class
        {
            string typeName = typeof(T).Name;

            Logger.LogInformation($"Getting {typeName} for id: {id}");

            Stopwatch sw = new Stopwatch();
            sw.Start();

            T item;
            string key = $"{typeName}-{id}";
            if (!Cache.TryGetValue(key, out item))
            {
                var apps = await ScaleContext.Query<T>(partition, table, primaryKey + " = ?", new List<object> { id });
                item = apps.FirstOrDefault();

                Logger.LogInformation($"Retrieved {typeName} from database in {sw.ElapsedMilliseconds}ms");

                if (item != null)
                    Cache.Set(key, item, defaultCacheDuration);
            }

            Logger.LogInformation($"Retrieved {typeName} in {sw.ElapsedMilliseconds}ms");
            sw.Stop();

            return item;
        }
    }
}
