﻿using System;

namespace SharkTank.Interfaces.Entities
{
    public interface IApplication
    {
        Guid Id { get; set; }
        Guid AccessKey { get; set; }
        string Name { get; set; }
        Guid AccountId { get; set; }
    }
}
