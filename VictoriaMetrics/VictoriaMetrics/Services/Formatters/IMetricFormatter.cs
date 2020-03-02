using VictoriaMetrics.VictoriaMetrics.Models.Metrics;

namespace VictoriaMetrics.VictoriaMetrics.Services.Formatters
{
    public interface IMetricFormatter<T>
    {
        /// <summary>
        /// Format to a line
        /// </summary>
        /// <param name="metric"></param>
        /// <returns></returns>
        T ToLine(Metric metric);
    }
}