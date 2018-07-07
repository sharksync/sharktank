using Newtonsoft.Json;
using SharkSync.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.PostgreSQL.Entities
{
    [Table("account")]
    public class Account : IAccount
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("emailaddress")]
        public string EmailAddress { get; set; }

        [Column("avatarurl")]
        public string AvatarUrl { get; set; }

        [Column("githubid")]
        public string GitHubId { get; set; }

        [Column("googleid")]
        public string GoogleId { get; set; }
        
        [Column("microsoftid")]
        public string MicrosoftId { get; set; }
        
        [Column("balance")]
        public int Balance { get; set; }
    }
}
