using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using LogIngester.Configuration;
using LogIngester.DnsIngest.Models;
using Microsoft.Extensions.Logging;
using Vibrant.InfluxDB.Client;
using Timer = System.Timers.Timer;


namespace LogIngester.DnsIngest.Services
{
    public class IngestWorker : IIngestWorker
    {
        private readonly InfluxClient            _influxClient;
        private readonly WorkerConfig            _workerConfig;
        private readonly InfluxDbConfig          _influxDbConfig;
        private readonly ILogger<IngestWorker>   _logger;
        private readonly ConcurrentQueue<DnsLog> _logsToProcess = new ConcurrentQueue<DnsLog>();
        private readonly Timer                   _timer;

        public IngestWorker(InfluxClient influxClient, WorkerConfig workerConfig, InfluxDbConfig influxDbConfig, ILogger<IngestWorker> logger)
        {
            _influxClient   = influxClient;
            _workerConfig   = workerConfig;
            _influxDbConfig = influxDbConfig;
            _logger         = logger;

            _timer = new Timer(workerConfig.Interval.TotalMilliseconds)
            {
                AutoReset = true
            };
            _timer.Elapsed += async (sender, args) => await DoWork();
            _timer.Start();
        }

        /// <summary>
        /// Add DNS log to be processed
        /// </summary>
        /// <param name="log"></param>
        public void AddToProcess(DnsLog log)
        {
            _logsToProcess.Enqueue(log);
        }

        private async Task DoWork()
        {
            var domainToSend = new ConcurrentDictionary<DnsLog.DomainKey, DnsLog>();

            long count = _logsToProcess.Count;
            var tasks = new List<Task>();

            for (var i = 0; i < _workerConfig.Workers; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    while (Interlocked.Read(ref count) > 0)
                    {
                        if (!_logsToProcess.TryDequeue(out var log))
                        {
                            _logger.Log(LogLevel.Critical, "Can't get a log to process");
                            return;
                        }

                        Interlocked.Decrement(ref count);

                        if (domainToSend.TryGetValue(log.Domain, out var existingValue))
                        {
                            log += existingValue;
                            if (!domainToSend.TryUpdate(log.Domain, log, existingValue))
                            {
                                _logger.Log(LogLevel.Critical, "Can't update entry {Domain}", log.Domain);
                            }

                            continue;
                        }

                        if (!domainToSend.TryAdd(log.Domain, log))
                        {
                            _logger.Log(LogLevel.Critical, "Can't add entry {Domain}", log.Domain);
                        }
                    }
                }));
            }

            await Task.WhenAll(tasks);
            await _influxClient.WriteAsync(_influxDbConfig.Db, domainToSend.Values);
        }
    }
}