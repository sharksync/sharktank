using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkTank.Interfaces.Entities
{
    public interface IApplication
    {
        Guid Id { get; set; }
        Guid AccessKey { get; set; }
        Guid AccountId { get; set; }
        string AppSettings { get; set; }
    }
}
