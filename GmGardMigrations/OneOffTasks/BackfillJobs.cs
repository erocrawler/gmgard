using GmGard.JobRunner;
using GmGard.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GmGardMigrations.OneOffTasks
{
    class BackfillJobs
    {
        public static void RunJobLog()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            Regex contentRe = new Regex("\\[Error\\] Error running job \"(.+)\"");
            IFormatter formatter = new BinaryFormatter();
            JobTaskRunner runner = new JobTaskRunner(new BlogContextFactory(), new UsersContextFactory());
            var reader = File.OpenText("job-20180321.log");
            while(!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var m = contentRe.Match(line);
                if (m.Success)
                {
                    Log.Information("Processing Line: {0}", line);
                    var content = m.Groups[1];
                    var job = (Job)formatter.Deserialize(new MemoryStream(Convert.FromBase64String(content.Value.Trim())));
                    runner.RunJob(job).Wait();
                }
            }
        }

        public static void RunSeverLog()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
            Regex contentRe = new Regex("\\[Error\\] Error sending job: \"(.+)\"$");
            IFormatter formatter = new BinaryFormatter();
            JobTaskRunner runner = new JobTaskRunner(new BlogContextFactory(), new UsersContextFactory());
            var reader = File.OpenText("log-20181212.txt");
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var m = contentRe.Match(line);
                if (m.Success)
                {
                    Log.Information("Processing Line: {0}", line);
                    var content = m.Groups[1];
                    var jobstring = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(content.Value.Replace(@"\\", @"\\\").Replace(@"\\\r", @"\\r").Replace(@"\\\n", @"\\n"));
                    var job = Newtonsoft.Json.JsonConvert.DeserializeObject<Job>(jobstring);
                    runner.RunJob(job).Wait();
                }
            }
        }
    }
}
