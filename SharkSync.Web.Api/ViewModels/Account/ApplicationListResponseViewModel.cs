using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.Web.Api.ViewModels
{
    public class ApplicationListResponseViewModel : BaseValidationViewModel
    {
        public IEnumerable<ApplicationViewModel> Applications { get; set; }
    }
}
