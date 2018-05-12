using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.Web.Api.ViewModels
{
    public class SyncRequestViewModel
    {
        [JsonProperty("app_id")]
        public Guid AppId { get; set; }

        [JsonProperty("app_api_access_key")]
        public Guid AppApiAccessKey { get; set; }

        [JsonProperty("groups")]
        public List<GroupViewModel> Groups { get; set; }

        [JsonProperty("changes")]
        public List<ChangeViewModel> Changes { get; set; }


        public class GroupViewModel
        {
            [JsonProperty("group")]
            public string Group { get; set; }

            [JsonProperty("tidemark")]
            public long? Tidemark { get; set; }
        }

        public class ChangeViewModel
        {
            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }

            [JsonProperty("secondsAgo")]
            public double SecondsAgo { get; set; }

            [JsonProperty("operation")]
            public int Operation { get; set; }

            [JsonProperty("group")]
            public string Group { get; set; }
        }
    }
}
