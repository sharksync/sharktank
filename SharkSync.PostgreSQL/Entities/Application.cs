using Newtonsoft.Json;
using SharkSync.Interfaces.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.PostgreSQL.Entities
{
    public class Application : IApplication
    {
        public Guid Id { get; set; }

        public Guid AccessKey { get; set; }

        public Guid AccountId { get; set; }

        public string Name { get; set; }
    }
}
