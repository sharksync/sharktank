using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.Web.Api
{
    public class AppSettings
    {
        public string ClientAppRootUrl { get; set; }
        public string AuthSecretId { get; set; }
        public string ConnectionSecretId { get; set; }
    }
}
