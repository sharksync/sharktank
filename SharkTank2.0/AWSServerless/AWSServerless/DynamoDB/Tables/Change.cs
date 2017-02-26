using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerless.DynamoDB.Tables
{
    public class Change
    {
        public string Group { get; set; }
        public long Tidemark { get; set; }
        public string Path { get; set; }
        public string Value { get; set; }
        public Guid DeviceId { get; set; }
        public DateTime Modified { get; set; }
    }
}
