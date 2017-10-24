using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerless.Scale.Tables
{
    public class Application
    {
        [JsonProperty("app_id")]
        public Guid Id { get; set; }
        [JsonProperty("app_api_access_key")]
        public Guid AccessKey { get; set; }
        [JsonProperty("account_id")]
        public Guid AccountId { get; set; }
        [JsonProperty("app_settings")]
        public string AppSettings { get; set; }
    }
}
