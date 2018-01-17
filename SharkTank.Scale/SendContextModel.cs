using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.Scale
{
    public class SendContextModel<T> where T : BasePayloadModel
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("payload")]
        public T Payload { get; set; }
    }

    public class BasePayloadModel
    {
        [JsonProperty("command")]
        public string Command { get; set; }

        [JsonProperty("keyspace")]
        public string Keyspace { get; set; }

        [JsonProperty("partition")]
        public string Partition { get; set; }

        [JsonProperty("table")]
        public string Table { get; set; }
    }

    public class QueryModel : BasePayloadModel
    {
        [JsonProperty("columns")]
        public List<string> Columns { get; set; }

        [JsonProperty("where")]
        public string Where { get; set; }

        [JsonProperty("parameters")]
        public List<object> Parameters { get; set; }

        [JsonProperty("offset")]
        public int? Offset { get; set; }

        [JsonProperty("limit")]
        public int? Limit { get; set; }

        [JsonProperty("order")]
        public string Order { get; set; }
    }

    public class UpsetModel<T> : BasePayloadModel
    {
        [JsonProperty("values")]
        public T Value { get; set; }
    }
}
