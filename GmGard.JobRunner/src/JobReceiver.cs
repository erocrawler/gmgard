using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace GmGard.JobRunner
{
    public class JobReceiver
    {
        public const string NAME = "GmJobRunner";
        public const string STAGING_NAME = "GmJobRunnerStaging";
        private string PipeName => _environment == "Staging" ? STAGING_NAME : NAME;
        private JobTaskRunner _taskRunner;
        private long jobCount = 0;
        private readonly string _environment;

        public JobReceiver(string BlogConnectionString, string UserConnectionString, string Environment)
        {
            _environment = Environment;
            _taskRunner = new JobTaskRunner(new BlogContextFactory(BlogConnectionString), new UsersContextFactory(UserConnectionString));
        }

        public void Start()
        {
            IFormatter formatter = new BinaryFormatter();
            Log.Information("starting NamedPipeServerStream: {0}", PipeName);
            using (var server = new NamedPipeServerStream(PipeName, PipeDirection.In))
            {
                Log.Information("waiting for connect on pipe: {0}", PipeName);
                server.WaitForConnection();

                using (StreamReader sr = new StreamReader(server))
                {
                    while (server.IsConnected)
                    {
                        var input = sr.ReadLine();
                        if (input == null)
                        {
                            Log.Information("Received null, exiting.");
                            continue;
                        }
                        try
                        {
                            var job = (Job)formatter.Deserialize(new MemoryStream(Convert.FromBase64String(input.Trim())));
                            Interlocked.Increment(ref jobCount);
                            Log.Information("Starting Job");
                            _taskRunner.RunJob(job).ContinueWith(t =>
                            {
                                Interlocked.Decrement(ref jobCount);
                                Log.Information("Job finished. Current job count: {0}", jobCount);
                            });
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "Error running job {0}", input);
                        }
                    }
                    Log.Information("Disconnected. Active job count: {0}", jobCount);
                }
            }
        }
    }
}
