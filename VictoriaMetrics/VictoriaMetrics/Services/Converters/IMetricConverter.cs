using VictoriaMetrics.VictoriaMetrics.Exceptions;
using VictoriaMetrics.VictoriaMetrics.Models.Metrics;

namespace VictoriaMetrics.VictoriaMetrics.Services.Converters
{
    public interface IMetricConverter
    {
        /// <summary>
        /// Convert an Object into a metric
        /// </summary>
        /// <param name="toConvert"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="AttributeNotFound">Not a valid object to be transformed into a Metric</exception>
        Metric ToMetric<T>(T toConvert) where T : class;
    }
}