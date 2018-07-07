using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkSync.Web.Api
{
    public class ConnectionStringSecret
    {
        public string Username { get; set; }
        public string Engine { get; set; }
        public string DBName { get; set; }
        public string Host { get; set; }
        public string Password { get; set; }
        public string Port { get; set; }
        public string DBInstanceIdentifier { get; set; }

        public string GetConnectionString()
        {
            return $"Host={Host};Database={DBName};Username={Username};Password={Password}";
        }
    }
}
