using System;
using System.Diagnostics.CodeAnalysis;
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

        public struct DomainKey : IEquatable<DomainKey>
        {
            [NotNull]
            public string Name { get; }

            public DomainKey(string name)
            {
                Name = name;
            }

            public bool Equals(DomainKey other)
            {
                return Name == other.Name;
            }

            public override bool Equals(object? obj)
            {
                return obj is DomainKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                return Name.GetHashCode();
            }

            public static implicit operator DomainKey(string domainName) => new DomainKey(domainName);
            public static implicit operator string(DomainKey key)        => key.Name;
        }

        [InfluxTimestamp]
        public DateTime Timestamp { get; } = DateTime.UtcNow;

        [InfluxTag("domain")]
        public DomainKey Domain { get; }

        [InfluxTag("state")]
        public QueryState State { get;}

        [InfluxField("query")]
        public long Query { get; private set; }

        public DnsLog(DomainKey domain, QueryState state, long query)
        {
            Domain = domain;
            State = state;
            Query = query;
        }

        public DnsLog()
        {
        }

        public static DnsLog operator +(DnsLog a) => a;

        public static DnsLog operator +(DnsLog a, DnsLog b)
        {
            a.Query += b.Query;
            return a;
        }
    }
}