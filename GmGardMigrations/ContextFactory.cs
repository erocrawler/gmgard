using GmGard.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmGardMigrations
{
    public static class ContextHelper
    {
        public static string GetLocalDB(string connectionString)
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../GmGard/App_Data");
            return connectionString.Replace("|DataDirectory|", dbPath);
        }
    }

    public class UsersContextFactory : IDbContextFactory<UsersContext>
    {
        public UsersContext Create()
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../App.config");
            var config = ConfigurationManager.OpenMappedMachineConfiguration(new ConfigurationFileMap(configPath));
            var user = config.ConnectionStrings.ConnectionStrings["GmGardUser"].ConnectionString;
            return new UsersContext(ContextHelper.GetLocalDB(user));
        }
    }

    public class UsersContext : GmGard.Models.UsersContext
    {
        public UsersContext(string connectionString) : base(connectionString)
        {
        }
    }

    public class BlogContextFactory : IDbContextFactory<BlogContext>
    {
        public BlogContext Create()
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../App.config");
            var config = ConfigurationManager.OpenMappedMachineConfiguration(new ConfigurationFileMap(configPath));
            var conn = config.ConnectionStrings.ConnectionStrings["GmGardData"].ConnectionString;
            return new BlogContext(ContextHelper.GetLocalDB(conn));
        }
    }

    public class BlogContext : GmGard.Models.BlogContext
    {
        public BlogContext(string connectionString) : base(connectionString)
        {
            Database.CommandTimeout = 3600;
        }
    }

}
