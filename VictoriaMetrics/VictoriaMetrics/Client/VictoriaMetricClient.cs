using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using VictoriaMetrics.VictoriaMetrics.Exceptions;
using VictoriaMetrics.VictoriaMetrics.Models.Attributes;
using VictoriaMetrics.VictoriaMetrics.Models.Configuration;
using VictoriaMetrics.VictoriaMetrics.Models.Metrics;
using VictoriaMetrics.VictoriaMetrics.Services.Converters;
using VictoriaMetrics.VictoriaMetrics.Services.Formatters;
using Tag = VictoriaMetrics.VictoriaMetrics.Models.Attributes.Tag;

namespace VictoriaMetrics.VictoriaMetrics.Client
{
    public class VictoriaMetricClient : IVictoriaMetricClient
    {
        private readonly IMetricFormatter<string> _metricFormatter;
        private readonly IMetricConverter         _converter;
        private readonly HttpClient               _httpClient;

        public VictoriaMetricClient(IMetricFormatter<string> metricFormatter, VictoriaConfig config, IMetricConverter converter, HttpClient httpClient)
        {
            _metricFormatter        = metricFormatter;
            _converter              = converter;
            _httpClient             = httpClient;
            _httpClient.BaseAddress = new Uri(config.Uri);
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

            return _httpClient.PostAsync("write", new StringContent(_metricFormatter.ToLine(metric)), cancellationToken);
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
    }
}