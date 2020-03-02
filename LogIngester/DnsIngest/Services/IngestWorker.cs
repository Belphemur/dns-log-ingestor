using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LogIngester.DnsIngest.Configuration;
using LogIngester.DnsIngest.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VictoriaMetrics.VictoriaMetrics.Client;
using Timer = System.Timers.Timer;


namespace LogIngester.DnsIngest.Services
{
    public class IngestWorker : IIngestWorker
    {
        private readonly IVictoriaMetricClient   _victoriaMetricClient;
        private readonly WorkerConfig            _workerConfig;
        private readonly ILogger<IngestWorker>   _logger;
        private readonly ConcurrentQueue<DnsLog> _logsToProcess = new ConcurrentQueue<DnsLog>();
        private readonly Timer                   _timer;
        private readonly CancellationTokenSource _tokenSource = new CancellationTokenSource();

        public IngestWorker(IVictoriaMetricClient    victoriaMetricClient,
                            WorkerConfig             workerConfig,
                            ILogger<IngestWorker>    logger,
                            IHostApplicationLifetime lifetime)
        {
            _victoriaMetricClient = victoriaMetricClient;
            _workerConfig         = workerConfig;
            _logger               = logger;
            _timer                = BuildTimer(workerConfig);

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
            timer.Elapsed += async (sender, args) => await DoWork(_tokenSource.Token);
            timer.Start();
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

            var domainToSend = new ConcurrentDictionary<string, DnsLog>();

            long count = _logsToProcess.Count;
            var  tasks = new List<Task>();

            Task BuildTask()
            {
                return Task.Run(() =>
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
                            var toLog = log + existingValue;
                            if (!domainToSend.TryUpdate(toLog.Domain, toLog, existingValue))
                            {
                                _logger.Log(LogLevel.Warning, "Can't update entry {Domain}. Re-queuing entry.", log.Domain);
                                _logsToProcess.Enqueue(log);
                                Interlocked.Increment(ref count);
                            }

                            continue;
                        }

                        if (!domainToSend.TryAdd(log.Domain, log))
                        {
                            _logger.Log(LogLevel.Warning, "Can't add entry {Domain}. Re-queuing entry.", log.Domain);
                            _logsToProcess.Enqueue(log);
                            Interlocked.Increment(ref count);
                        }
                    }
                }, token);
            }

            for (var i = 0; i < _workerConfig.Workers; i++)
            {
                tasks.Add(BuildTask());
            }

            await Task.WhenAll(tasks);

            if (domainToSend.IsEmpty) return;

            await _victoriaMetricClient.SendBatchMetricsAsync(domainToSend.Values, token);
        }
    }
}