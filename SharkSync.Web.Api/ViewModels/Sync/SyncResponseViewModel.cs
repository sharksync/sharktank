using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.Web.Api.ViewModels
{
    public class SyncResponseViewModel : BaseValidationViewModel
    {
        public List<GroupViewModel> Groups { get; set; }

        public class GroupViewModel
        {
            public long Tidemark { get; set; }
            public string Group { get; set; }
            public List<ChangeViewModel> Changes { get; set; }
        }

        public class ChangeViewModel
        {
            public Guid RecordId { get; set; }
            public string Entity { get; set; }
            public string Property { get; set; }
            public string Value { get; set; }
            public DateTime Modified { get; set; }
        }
    }
}
