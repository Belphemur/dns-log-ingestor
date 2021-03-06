using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VictoriaMetrics.Client.Content;
using VictoriaMetrics.Exceptions;
using VictoriaMetrics.Extensions;
using VictoriaMetrics.Models.Configuration;
using VictoriaMetrics.Models.Metrics;
using VictoriaMetrics.Services.Converters;
using VictoriaMetrics.Services.Formatters;

namespace VictoriaMetrics.Client
{
    public class VictoriaMetricClient : IVictoriaMetricClient
    {
        private readonly IMetricFormatter<string> _metricFormatter;
        private readonly IMetricConverter         _converter;
        private readonly HttpClient               _httpClient;
        private readonly int                     _chunkSize;

        public VictoriaMetricClient(IMetricFormatter<string> metricFormatter, VictoriaConfig config, IMetricConverter converter, HttpClient httpClient)
        {
            _metricFormatter        = metricFormatter;
            _converter              = converter;
            _httpClient             = httpClient;
            _httpClient.BaseAddress = new Uri(config.Uri);
            _chunkSize              = config.ChunkSize < 50 ? 50 : config.ChunkSize;
        }

        /// <summary>
        /// Send a metric to victoria metrics server
        /// </summary>
        /// <param name="metric"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SendMetricAsync(Metric metric, CancellationToken cancellationToken)
        {
            if (metric.Fields.Count == 0)
            {
                throw new FieldException("Need to have minimum one field");
            }

            var content = _metricFormatter.ToLine(metric);
            return WriteAsync(content, cancellationToken);
        }


        /// <summary>
        /// Send a metric to victoria metrics server
        /// </summary>
        /// <param name="toSend"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task SendMetricAsync<T>(T toSend, CancellationToken cancellationToken) where T : class
        {
            return SendMetricAsync(_converter.ToMetric(toSend), cancellationToken);
        }

        /// <summary>
        /// Send metrics in batch
        /// </summary>
        /// <param name="metrics"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SendBatchMetricsAsync(IEnumerable<Metric> metrics, CancellationToken cancellationToken)
        {
            var chunks = metrics.Where(metric => metric.Fields.Count > 0).Chunk(_chunkSize);
            var tasks = chunks.Select(chunk => string.Join("\n", chunk.Select(_metricFormatter.ToLine)))
                              .Select(content => WriteAsync(content, cancellationToken));

            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Send metrics in batch
        /// </summary>
        /// <param name="metrics"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task SendBatchMetricsAsync<T>(IEnumerable<T> metrics, CancellationToken cancellationToken) where T : class
        {
            return SendBatchMetricsAsync(metrics.Select(_converter.ToMetric), cancellationToken);
        }

        private Task WriteAsync(string content, CancellationToken cancellationToken)
        {
            var httpContent       = new StringContent(content, Encoding.UTF8, "text/plain");
            var compressedContent = new CompressedContent(httpContent, CompressedContent.Compression.gzip);

            return _httpClient.PostAsync($"write?precision={_metricFormatter.Precision}", compressedContent, cancellationToken);
        }
    }
}