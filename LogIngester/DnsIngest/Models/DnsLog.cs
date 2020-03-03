using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VictoriaMetrics.Models.Attributes;

namespace LogIngester.DnsIngest.Models
{
    [Measurement("dns")]
    public class DnsLog
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public enum QueryState
        {
            Normal,
            Blocked
        }

        [Timestamp]
        public DateTime? Timestamp { get; set; }

        [Tag("domain")]
        public string Domain { get; set; }

        [Tag("state")]
        public QueryState State { get; set; }

        [Field("query")]
        public long Query { get; set; }

        public static DnsLog operator +(DnsLog a) => a;

        public static DnsLog operator +(DnsLog a, DnsLog b)
        {
            if (a.Timestamp.HasValue && b.Timestamp.HasValue)
                a.Timestamp = a.Timestamp.Value < b.Timestamp.Value ? a.Timestamp.Value : b.Timestamp.Value;
            else if (!a.Timestamp.HasValue && b.Timestamp.HasValue)
                a.Timestamp = b.Timestamp;
            a.Query += b.Query;
            return a;
        }
    }
}