using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerless.DynamoDB.Tables
{
    public class Device
    {
        public Guid Id { get; set; }
        public Guid AppId { get; set; }
        public Guid SyncId { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
