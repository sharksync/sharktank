using Newtonsoft.Json;
using SharkSync.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.PostgreSQL.Entities
{
    public class Account : IAccount
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string EmailAddress { get; set; }

        public string AvatarUrl { get; set; }

        public string GitHubId { get; set; }

        public string GoogleId { get; set; }

        public string MicrosoftId { get; set; }
    }
}
