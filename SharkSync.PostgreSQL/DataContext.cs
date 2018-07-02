using Microsoft.EntityFrameworkCore;
using SharkSync.PostgreSQL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharkSync.PostgreSQL
{
    public class DataContext : DbContext
    {
        // When used with ASP.net core, add these lines to Startup.cs
        //   var connectionString = Configuration.GetConnectionString("BlogContext");
        //   services.AddEntityFrameworkNpgsql().AddDbContext<BlogContext>(options => options.UseNpgsql(connectionString));
        // and add this to appSettings.json
        // "ConnectionStrings": { "BlogContext": "Server=localhost;Database=blog" }

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Application> Applications { get; set; }

        public DbSet<Change> Changes { get; set; }
    }

}
