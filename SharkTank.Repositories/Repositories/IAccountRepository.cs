using SharkTank.Interfaces.Entities;
using System;
using System.Threading.Tasks;

namespace SharkTank.Interfaces.Repositories
{
    public interface IAccountRepository
    {
        Task<IAccount> AddAsync(string name);
    }
}
