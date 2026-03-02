using GmGard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;
using System;
using System.Configuration;
using System.IO;

namespace GmGardMigrations
{
    public class UsersContextFactory : IDesignTimeDbContextFactory<UsersContext>
    {
        public UsersContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<UsersContext>();
            
            // Read connection string from App.config
            var connectionString = ConfigurationManager.ConnectionStrings["GmGardUser"]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'GmGardUser' not found in App.config");
            }
            
            optionsBuilder.UseNpgsql(connectionString);

            return new UsersContext(optionsBuilder.Options);
        }
    }

    public class BlogContextFactory : IDesignTimeDbContextFactory<BlogContext>
    {
        public BlogContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BlogContext>();
            
            // Read connection string from App.config
            var connectionString = ConfigurationManager.ConnectionStrings["GmGardData"]?.ConnectionString;
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'GmGardData' not found in App.config");
            }

            var csb = new NpgsqlConnectionStringBuilder(connectionString)
            {
                Timeout = 300,        // connection-open timeout in seconds
                CommandTimeout = 600, // command execution timeout in seconds
                KeepAlive = 30,       // send keepalive every 30s to prevent idle drop
            };
            optionsBuilder.UseNpgsql(csb.ToString());

            return new BlogContext(optionsBuilder.Options);
        }
    }
}
