using Newtonsoft.Json;
using SharkSync.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.PostgreSQL.Entities
{
    [Table("application")]
    public class Application : IApplication
    {
        [Column("id")]
        public Guid Id { get; set; }
        
        [Column("accesskey")]
        public Guid AccessKey { get; set; }
        
        [Column("accountid")]
        public Guid AccountId { get; set; }
        
        [Column("name")]
        public string Name { get; set; }
    }
}
