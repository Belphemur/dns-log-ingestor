using VictoriaMetrics.Models.Attributes;

namespace LogIngester.DnsIngest.Models.Metrics
{
    [Measurement("dns_ingestion")]
    public class IngestionRate
    {
        [Tag]
        public string Hostname => System.Environment.MachineName;

        /// <summary>
        /// Number of entry processed
        /// </summary>
        [Field]
        public long Processed { get; }

        /// <summary>
        /// Aggregated result
        /// </summary>
        [Field]
        public long Aggregated { get; }

        public IngestionRate(long processed, long aggregated)
        {
            Processed  = processed;
            Aggregated = aggregated;
        }
    }
}