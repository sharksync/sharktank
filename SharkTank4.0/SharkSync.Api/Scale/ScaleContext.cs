using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SharkSync.Api.Scale
{
    public interface IScaleContext
    {
        Task<List<T>> Query<T>(string partition, string table, string whereClause = null, List<object> whereClauseValues = null, string orderBy = null, int? limit = null, int? offset = null) where T : class;

        SendContextModel<UpsetModel<T>> MakeUpsertModel<T>(string partition, string table, T value) where T : class;

        Task Upsert<T>(string partition, string table, T value) where T : class;
        Task UpsertBulk<T>(List<SendContextModel<UpsetModel<T>>> items) where T : class;
    }

    public class ScaleContext : IScaleContext
    {
        //private static readonly string serverUrl = "http://db.sharksync.com:5555";
        private static readonly string serverUrl = "http://localhost:5555";
        private static readonly string keyspace = "dev";

        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        ILogger Logger { get; set; }

        public ScaleContext(ILogger<QueryCache> logger)
        {
            Logger = logger;
        }

        public async Task<List<T>> Query<T>(string partition, string table, string whereClause = null, List<object> whereClauseValues = null, string orderBy = null, int? limit = null, int? offset = null) where T : class
        {
            var request = new SendContextModel<QueryModel>
            {
                Type = "read",
                Payload = new QueryModel
                {
                    Command = "query",
                    Keyspace = keyspace,
                    Partition = partition,
                    Table = table,
                    Where = whereClause,
                    Parameters = whereClauseValues,
                    Order = orderBy,
                    Limit = limit,
                    Offset = offset,
                }
            };

            var response = await Send<QueryModel, T>(new List<SendContextModel<QueryModel>> { request });
            var firstResult = response.FirstOrDefault();

            return firstResult?.Results ?? new List<T>();
        }

        public async Task Upsert<T>(string partition, string table, T value) where T : class
        {
            var request = MakeUpsertModel(partition, table, value);

            await Send<UpsetModel<T>, T>(new List<SendContextModel<UpsetModel<T>>> { request });
        }

        public async Task UpsertBulk<T>(List<SendContextModel<UpsetModel<T>>> items) where T : class
        {
            await Send<UpsetModel<T>, T>(items);
        }

        public SendContextModel<UpsetModel<T>> MakeUpsertModel<T>(string partition, string table, T value) where T : class
        {
            return new SendContextModel<UpsetModel<T>>
            {
                Type = "write",
                Payload = new UpsetModel<T>
                {
                    Command = "update",
                    Keyspace = keyspace,
                    Partition = partition,
                    Table = table,
                    Value = value
                }
            };
        }

        private async Task<List<ScaleResponse<T>>> Send<Y, T>(List<SendContextModel<Y>> request) where Y : BasePayloadModel where T : class
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            string requestBody = JsonConvert.SerializeObject(request, Formatting.Indented, jsonSettings);
            Logger.LogInformation($"Scale request SerializeObject in {sw.ElapsedMilliseconds}ms length: {requestBody.Length}");
            sw.Restart();

            var client = new HttpClient();
            var content = new StringContent(requestBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(serverUrl, content);

            response.EnsureSuccessStatusCode();

            Logger.LogInformation($"Scale request post in {sw.ElapsedMilliseconds}ms");
            sw.Restart();

            Type table = typeof(T);
            string responseString = await response.Content.ReadAsStringAsync();
            var scaleResponse = JsonConvert.DeserializeObject<List<ScaleResponse<T>>>(responseString);

            Logger.LogInformation($"Scale request post response DeserializeObject in {sw.ElapsedMilliseconds}ms");

            if (scaleResponse == null || scaleResponse.Any(e => e.Error != null))
                throw new Exception($"Failed to complete query: {requestBody} \n Error response: {JsonConvert.SerializeObject(scaleResponse)}");

            sw.Stop();
            return scaleResponse;
        }
    }

    public class ScaleResponse<T>
    {
        public string Error { get; set; }
        public Guid RequestId { get; set; }
        public string Message { get; set; }
        public List<T> Results { get; set; }
    }
}
