using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkTank.Repositories.Entities
{
    public class Device
    {
        [JsonProperty("device_id")]
        public Guid Id { get; set; }
        [JsonProperty("app_id")]
        public Guid AppId { get; set; }
        [JsonProperty("account_id")]
        public Guid AccountId { get; set; }
        [JsonProperty("sync_id")]
        public Guid SyncId { get; set; }
        [JsonProperty("last_seen")]
        public string LastSeen { get; set; }
    }
}
