using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.Web.Api.ViewModels
{
    public class SyncRequestViewModel
    {
        public Guid AppId { get; set; }
        public Guid AppApiAccessKey { get; set; }
        public List<GroupViewModel> Groups { get; set; }
        public List<ChangeViewModel> Changes { get; set; }

        public class GroupViewModel
        {
            public string Group { get; set; }
            public long? Tidemark { get; set; }
        }

        public class ChangeViewModel
        {
            public Guid RecordId { get; set; }
            public string Entity { get; set; }
            public string Group { get; set; }
            public string Value { get; set; }
            public double MillisecondsAgo { get; set; }
            public string Property { get; set; }
        }
    }
}
