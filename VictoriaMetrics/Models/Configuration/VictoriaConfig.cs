namespace VictoriaMetrics.Models.Configuration
{
    public class VictoriaConfig
    {
        /// <summary>
        /// Where to send the data
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Size of chunk for batch requests.
        /// Minimum 50
        /// </summary>
        public int ChunkSize { get; set; }
    }
}