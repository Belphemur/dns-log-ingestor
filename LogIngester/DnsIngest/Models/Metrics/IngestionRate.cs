using VictoriaMetrics.Models.Attributes;

namespace LogIngester.DnsIngest.Models.Metrics
{
    [Measurement("dns_ingestion")]
    public class IngestionRate
    {
        [Tag]
        public string Hostname { get; }
        [Field]
        public long Processed { get;  }

        public IngestionRate(long processed)
        {
            Hostname = System.Environment.MachineName;
            Processed = processed;
        }
    }
}