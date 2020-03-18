using System.Data.Entity.Migrations;
using GmGard.Models;
using System.Data.Entity.Infrastructure;
using System.IO;
using System;
using System.Configuration;

namespace MyMVCWeb.Migrations
{
    public class Configuration : DbMigrationsConfiguration<BlogContext>
	{
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
        }
    }
}
