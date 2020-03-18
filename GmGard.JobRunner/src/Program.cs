using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace GmGard.JobRunner
{
    public class Program
    {
        /// <summary>
        /// Entry point for JobRunner.
        /// </summary>
        /// <param name="args">The program argument. Should be: "Blog DB connection string" "User DB connection string" "environment"</param>
        public static void Main(string[] args)
        {
            var loglevel = new Serilog.Core.LoggingLevelSwitch(Serilog.Events.LogEventLevel.Debug);
            Log.Logger = new LoggerConfiguration()
                .WriteTo.RollingFile(@"Log\job-{Date}.log")
                .MinimumLevel.ControlledBy(loglevel)
                .CreateLogger();
            if (args.Length < 2)
            {
                Log.Fatal("Invalid usage. args must provide connection string to db.");
                return;
            }
            if (args.Length > 2 && args[2] == "Production")
            {
                loglevel.MinimumLevel = Serilog.Events.LogEventLevel.Error;
            }
            Log.Information("JobRunner Started. Connection Strings are: {0}, {1}", args[0], args[1]);
            var receiver = new JobReceiver(args[0], args[1], args[2]);
            receiver.Start();
        }
    }
}
