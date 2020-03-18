using GmGard.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmGard.JobRunner
{

    internal class UsersContextFactory : IDbContextFactory<UsersContext>
    {
        private string _conn;

        public UsersContextFactory(string connectionString)
        {
            _conn = connectionString;
        }

        public UsersContext Create()
        {
            var db = new UsersContext(_conn);
            db.Database.Log = l => Log.Debug(l);
            return db;
        }
    }


    internal class BlogContextFactory : IDbContextFactory<BlogContext>
    {
        private string _conn;

        public BlogContextFactory(string connectionString)
        {
            _conn = connectionString;
        }

        public BlogContext Create()
        {
            var db = new BlogContext(_conn);
            db.Database.Log = l => Log.Debug(l);
            return db;
        }
    }

}
