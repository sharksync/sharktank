using SharkSync.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharkSync.PostgreSQL.Services
{
    public class TimeService : ITimeService
    {
        public DateTime GetUtcNow()
        {
            return DateTime.UtcNow;
        }
    }
}
