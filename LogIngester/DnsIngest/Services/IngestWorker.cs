using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using LogIngester.Configuration;
using LogIngester.DnsIngest.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Vibrant.InfluxDB.Client;
using Timer = System.Timers.Timer;


namespace LogIngester.DnsIngest.Services
{
    public class IngestWorker : IIngestWorker
    {
        private readonly InfluxClient             _influxClient;
        private readonly WorkerConfig             _workerConfig;
        private readonly InfluxDbConfig           _influxDbConfig;
        private readonly ILogger<IngestWorker>    _logger;
        private readonly ConcurrentQueue<DnsLog>  _logsToProcess = new ConcurrentQueue<DnsLog>();
        private readonly Timer                    _timer;
        private readonly CancellationTokenSource  _tokenSource = new CancellationTokenSource();

        public IngestWorker(InfluxClient             influxClient,
                            WorkerConfig             workerConfig,
                            InfluxDbConfig           influxDbConfig,
                            ILogger<IngestWorker>    logger,
                            IHostApplicationLifetime lifetime)
        {
            _influxClient   = influxClient;
            _workerConfig   = workerConfig;
            _influxDbConfig = influxDbConfig;
            _logger         = logger;
            _timer = BuildTimer(workerConfig);

            lifetime.ApplicationStopping.Register(async () =>
            {
                _timer.Stop();
                _timer.Dispose();
                _tokenSource.Cancel();
                await DoWork(default);
            });
        }

        private Timer BuildTimer(WorkerConfig workerConfig)
        {
            var timer = new Timer(workerConfig.Interval.TotalMilliseconds)
            {
                AutoReset = true
            };
            _timer.Elapsed += async (sender, args) => await DoWork(_tokenSource.Token);
            _timer.Start();
            return timer;
        }

        /// <summary>
        /// Add DNS log to be processed
        /// </summary>
        /// <param name="log"></param>
        public int AddToProcess(DnsLog log)
        {
            _logsToProcess.Enqueue(log);
            return _logsToProcess.Count;
        }

        private void Stop()
        {
            _tokenSource.Cancel();
        }

        private async Task DoWork(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            var domainToSend = new ConcurrentDictionary<DnsLog.DomainKey, DnsLog>();

            long count = _logsToProcess.Count;
            var  tasks = new List<Task>();

            for (var i = 0; i < _workerConfig.Workers; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    while (Interlocked.Read(ref count) > 0)
                    {
                        if (token.IsCancellationRequested)
                        {
                            _logger.Log(LogLevel.Information, "Cancellation Requested");
                            return;
                        }

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
                }, token));
            }

            await Task.WhenAll(tasks);
            await _influxClient.WriteAsync(_influxDbConfig.Db, domainToSend.Values);
        }
    }
}