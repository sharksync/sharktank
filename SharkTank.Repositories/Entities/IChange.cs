using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharkTank.Interfaces.Entities
{
    public interface IChange
    {
        Guid Id { get; set; }
        Guid RecordId { get; set; }
        string Group { get; set; }
        string Path { get; set; }
        Guid DeviceId { get; set; }
        string Tidemark { get; set; }
        DateTime Modified { get; set; }
        string Value { get; set; }
    }
}
