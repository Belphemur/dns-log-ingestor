using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Vibrant.InfluxDB.Client;

namespace LogIngester.DnsIngest.Models
{
    public class DnsLog
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum QueryState
        {
            Normal,
            Blocked
        }

        [InfluxTimestamp]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        [InfluxTag("domain")]
        public string Domain { get; set; }

        [InfluxTag("state")]
        public QueryState State { get; set; }

        [InfluxField("query")]
        public long Query { get; set; }
    }
}