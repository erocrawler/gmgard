using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO.Pipes;
using Microsoft.Extensions.Hosting;

namespace GmGard.Services
{
    public class BackgroundJobService : IDisposable
    {
        private Process _backgroundService;
        private ILogger _logger;
        private bool _disposed = false;
        private bool _connected = false;
        private NamedPipeClientStream _client;
        private StreamWriter _streamWriter;
        private IWebHostEnvironment _env;
        private string PipeName => _env.IsStaging() ? "GmJobRunnerStaging" : "GmJobRunner";

        public BackgroundJobService(IWebHostEnvironment env, ILoggerFactory loggerFactory, string blogConnectionString, string userConnectionString)
        {
            _logger = loggerFactory.CreateLogger<BackgroundJobService>();
            _env = env;
            Process p = new Process();
            p.StartInfo.Arguments = $"\"{blogConnectionString}\" \"{userConnectionString}\" \"{env.EnvironmentName}\"";
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.WorkingDirectory = env.ContentRootPath;
            p.StartInfo.UseShellExecute = false;
            var curPath = env.ContentRootPath;
            if (env.IsDevelopment())
            {
                curPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            }
            p.StartInfo.FileName = Path.Combine(curPath, "GmGard.JobRunner.exe");
            p.EnableRaisingEvents = true;
            p.Exited += ProgramExited;
            p.Start();
            _backgroundService = p;
            _logger.LogInformation("Creating pipe {0}", PipeName);
            _client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
        }

        public void Connect()
        {
            _client.Connect(6000);
            _connected = true;
            _logger.LogInformation("Pipe Connected");
            _streamWriter = new StreamWriter(_client);
        }

        private void ProgramExited(object sender, EventArgs e)
        {
            if (!_disposed)
            {
                _logger.LogError("Background Job Runner exited unexpectedly. Attempting to restart...");
                _backgroundService.Start();
                _client.Dispose();
                _client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                _client.Connect();
                _streamWriter = new StreamWriter(_client);
            }
        }

        public void Dispose()
        {
            _disposed = true;
            if (_connected)
            {
                _streamWriter.Dispose();
            }
            if (!_backgroundService.HasExited)
            {
                _backgroundService.Kill();
            }
        }

        public void RunJob(JobRunner.Job job)
        {
            if (!_connected)
            {
                throw new InvalidOperationException("Pipe not connected.");
            }
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("sending job to background: {0}", job.ToString());
            }
            IFormatter formatter = new BinaryFormatter();

            try
            {
                var input = TextWriter.Synchronized(_streamWriter);
                var memstream = new MemoryStream();
                formatter.Serialize(memstream, job);
                input.WriteLine(Convert.ToBase64String(memstream.GetBuffer()));
                input.Flush();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error sending job: {0}", job.ToString());
            }
        }
    }
}
