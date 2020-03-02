using System.Dynamic;

namespace LogIngester.Configuration
{
    public class InfluxDbConfig
    {
        public string  Db       { get; set; }
        public string  Uri      { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}