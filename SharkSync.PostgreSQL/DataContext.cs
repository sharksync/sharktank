using Microsoft.EntityFrameworkCore;
using SharkSync.PostgreSQL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharkSync.PostgreSQL
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Application> Applications { get; set; }

        public DbSet<Change> Changes { get; set; }
    }

}
