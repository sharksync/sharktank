using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AWSServerless.DynamoDB.Tables
{
    public class Application
    {
        public Guid Id { get; set; }
        public Guid ApiAccessKey { get; set; }
    }
}
