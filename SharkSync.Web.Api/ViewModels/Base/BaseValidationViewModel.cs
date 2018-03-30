using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.Web.Api.ViewModels
{
    public class BaseValidationViewModel
    {
        public IEnumerable<string> Errors { get; set; }

        public bool Success
        {
            get { return Errors == null || !Errors.Any(); }
        }
    }
}
