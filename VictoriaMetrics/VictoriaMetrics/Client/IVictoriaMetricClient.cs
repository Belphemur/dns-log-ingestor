using System.Threading;
using System.Threading.Tasks;
using VictoriaMetrics.VictoriaMetrics.Models.Metrics;

namespace VictoriaMetrics.VictoriaMetrics.Client
{
    public interface IVictoriaMetricClient
    {
        /// <summary>
        /// Send a metric to victoria metrics server
        /// </summary>
        /// <param name="metric"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task SendMetricAsync(Metric metric, CancellationToken cancellationToken);

        /// <summary>
        /// Send a metric to victoria metrics server
        /// </summary>
        /// <param name="toSend"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task SendMetricAsync<T>(T toSend, CancellationToken cancellationToken) where T : class;
    }
}