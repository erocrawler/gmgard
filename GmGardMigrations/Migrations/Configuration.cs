using System.Data.Entity.Migrations;
using GmGardMigrations;

namespace MyMVCWeb.Migrations
{
    public class Configuration : DbMigrationsConfiguration<BlogContext>
	{
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            CommandTimeout = 300;
        }
    }
}
