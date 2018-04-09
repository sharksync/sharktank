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
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public int? GitHubId { get; set; }
        public string AvatarUrl { get; set; }
    }
}
