using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkTank.Repositories.Entities
{
    public class Change
    {
        [JsonProperty("change_id")]
        public Guid Id { get; set; }
        [JsonProperty("rec_id")]
        public Guid RecordId { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("device_id")]
        public Guid DeviceId { get; set; }
        [JsonProperty("tidemark")]
        public string Tidemark { get; set; }
        [JsonProperty("modified")]
        public DateTime Modified { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    // ChangeWithGroup is used when storing changes to know which group it belongs to
    public class ChangeWithGroup : Change
    {
        public string Group { get; set; }
    }
}
