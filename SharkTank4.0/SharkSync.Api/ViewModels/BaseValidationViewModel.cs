using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.Api.ViewModels
{
    public class BaseValidationViewModel
    {
        public bool Success { get; set; }
        public IEnumerable<string> Errors { get; set; }
    }
}
