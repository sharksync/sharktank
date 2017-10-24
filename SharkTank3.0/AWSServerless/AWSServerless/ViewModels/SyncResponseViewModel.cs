using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerless.ViewModels
{
    public class SyncResponseViewModel : BaseValidationViewModel
    {
        public List<GroupViewModel> Groups { get; set; }

        public class GroupViewModel
        {
            public string Tidemark { get; set; }
            public string Group { get; set; }
            public List<ChangeViewModel> Changes { get; set; }
        }

        public class ChangeViewModel
        {
            public string Path { get; set; }
            public string Value { get; set; }
            public DateTime Modified { get; set; }
        }
    }
}
