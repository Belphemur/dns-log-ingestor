using VictoriaMetrics.VictoriaMetrics.Model;

namespace VictoriaMetrics.VictoriaMetrics.Formatter
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