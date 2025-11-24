using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using GmGard.Models;
using System.IO;

namespace GmGard
{
    public class BlogContextFactory : IDesignTimeDbContextFactory<BlogContext>
    {
        public BlogContext CreateDbContext(string[] args)
        {
            System.AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.Production.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<BlogContext>();
            var connectionString = configuration.GetConnectionString("GmGardData")
                .Replace("|DataDirectory|", Path.Combine(Directory.GetCurrentDirectory(), "App_Data"));

            // Auto-detect database provider
            if (connectionString.Contains("Host=") || (connectionString.Contains("Server=") && connectionString.Contains("Username=")))
            {
                optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("GmGard"));
            }
            else
            {
                optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("GmGard"));
            }

            return new BlogContext(optionsBuilder.Options);
        }
    }

    public class UsersContextFactory : IDesignTimeDbContextFactory<UsersContext>
    {
        public UsersContext CreateDbContext(string[] args)
        {
            System.AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddJsonFile("appsettings.Production.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<UsersContext>();
            var connectionString = configuration.GetConnectionString("GmGardUser")
                .Replace("|DataDirectory|", Path.Combine(Directory.GetCurrentDirectory(), "App_Data"));

            // Auto-detect database provider
            if (connectionString.Contains("Host=") || (connectionString.Contains("Server=") && connectionString.Contains("Username=")))
            {
                optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("GmGard"));
            }
            else
            {
                optionsBuilder.UseSqlServer(connectionString, b => b.MigrationsAssembly("GmGard"));
            }

            return new UsersContext(optionsBuilder.Options);
        }
    }
}
