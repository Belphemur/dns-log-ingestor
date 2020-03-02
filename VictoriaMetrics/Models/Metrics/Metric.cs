using System;
using System.Collections.Generic;

namespace VictoriaMetrics.Models.Metrics
{
    /// <summary>
    /// Metric to register
    /// </summary>
    public class Metric
    {
        public string                    Name      { get; set; }
        public DateTime?                 Timestamp { get; set; }
        public Dictionary<string, Tag>   Tags      { get; set; }
        public Dictionary<string, Field> Fields    { get; set; }
    }
}