﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharkSync.Interfaces
{
    public interface IApplicationRepository
    {
        Task<IEnumerable<IApplication>> ListByAccountIdAsync(Guid accountId);
        Task<IApplication> GetByIdAsync(Guid id);
        Task<IApplication> AddAsync(string name, Guid accountId);
        Task DeleteAsync(Guid id);
    }
}
