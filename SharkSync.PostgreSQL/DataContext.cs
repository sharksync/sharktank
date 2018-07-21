using Microsoft.EntityFrameworkCore;
using SharkSync.Interfaces;
using SharkSync.PostgreSQL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharkSync.PostgreSQL
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options, ISettingsService settingsService)
            : base(options)
        {
            var task = settingsService.Get<ConnectionStringSettings>();
            task.Wait();
            Settings = task.Result;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (Settings == null)
                throw new Exception("No ConnectionStringSettings to configure DataContext with");

            optionsBuilder.UseNpgsql(Settings.GetConnectionString());
        }

        public ConnectionStringSettings Settings { get; set; }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Application> Applications { get; set; }

        public DbSet<Change> Changes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Integration tests data
            var testAccount = new Account
            {
                Id = new Guid("250c6f28-4611-4c28-902c-8464fabc510b"),
                Name = "Integration Tests Account"
            };

            modelBuilder.Entity<Account>().HasData(testAccount);

            modelBuilder.Entity<Application>().HasData(new Application
            {
                Id = new Guid("afd8db1e-73b8-4d5f-9cb1-6b49d205555a"),
                Name = "Integration Test App",
                AccessKey = new Guid("3d65a27c-9d1d-48a3-a888-89cc0f7851d0"),
                AccountId = testAccount.Id
            });

            modelBuilder.Entity<Application>().HasData(new Application
            {
                Id = new Guid("59eadf1b-c4bf-4ded-8a2b-b80305b960fe"),
                Name = "Integration Test App 2",
                AccessKey = new Guid("e7b40cf0-2781-4dc7-9545-91fd812fc506"),
                AccountId = testAccount.Id
            });

            modelBuilder.Entity<Application>().HasData(new Application
            {
                Id = new Guid("b858ceb1-00d0-4427-b45d-e9890b77da36"),
                Name = "Integration Test App 3",
                AccessKey = new Guid("03172495-6158-44ae-b5b4-6ea5163f02d8"),
                AccountId = testAccount.Id
            });

            modelBuilder.Entity<Application>().HasData(new Application
            {
                Id = new Guid("19d8856c-a439-46ae-9932-c81fd0fe5556"),
                Name = "Integration Test App 4",
                AccessKey = new Guid("0f458ce8-1a0e-450c-a2c4-2b50b3c4f41d"),
                AccountId = testAccount.Id
            });
        }
    }
}
