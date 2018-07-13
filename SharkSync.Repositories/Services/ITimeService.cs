using System;
using System.Collections.Generic;
using System.Text;

namespace SharkSync.Interfaces
{
    public interface ITimeService
    {
        DateTime GetUtcNow();
    }
}
