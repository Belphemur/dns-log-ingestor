using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VictoriaMetrics.VictoriaMetrics.Models.Metrics;

namespace VictoriaMetrics.VictoriaMetrics.Services.Formatters
{
    public class LineFormatter : IMetricFormatter<string>
    {
        /// <summary>
        /// Precision of the Timestamp
        /// </summary>
        public string Precision { get; } = "ns";

        private string ToLine<T>(BaseModel<T> model)
        {
            return $"{model.Name}={model.Value}";
        }

        private string ToLine<T>(IEnumerable<BaseModel<T>> models)
        {
            return string.Join(",", models.Select(ToLine));
        }

        /// <summary>
        /// Format to a line
        /// </summary>
        /// <param name="metric"></param>
        /// <returns></returns>
        public string ToLine(Metric metric)
        {
            var stringBuilder = new StringBuilder(metric.Name);
            if (metric.Tags.Count > 0)
            {
                stringBuilder.Append(',');
                stringBuilder.Append(ToLine(metric.Tags.Values));
            }

            stringBuilder.Append(' ');
            stringBuilder.Append(ToLine(metric.Fields.Values));

            if (metric.Timestamp.HasValue)
            {
                stringBuilder.Append(' ');
                var epoch = metric.Timestamp.Value
                                  .ToUniversalTime()
                                  .Subtract(
                                      new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                                  );

                stringBuilder.Append(epoch.Ticks * 100);
            }

            return stringBuilder.ToString();
        }
    }
}