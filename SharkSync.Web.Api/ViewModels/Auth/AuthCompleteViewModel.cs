using Newtonsoft.Json;
using SharkTank.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.Web.Api.ViewModels
{
    public class AuthCompleteViewModel : BaseValidationViewModel
    {
        public string Name { get; set; }
        public string Login { get; set; }
        public string AvatarUrl { get; set; }
        public string GitHubUrl { get; set; }
    }
}
