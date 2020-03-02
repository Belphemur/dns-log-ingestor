using LogIngester.DnsIngest.Models;

namespace LogIngester.DnsIngest.Services
{
    public interface IIngestWorker
    {
        /// <summary>
        /// Add DNS log to be processed
        /// </summary>
        /// <param name="log"></param>
        void AddToProcess(DnsLog log);
    }
}