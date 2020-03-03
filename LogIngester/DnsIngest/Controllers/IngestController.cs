using System;
using System.Threading;
using LogIngester.DnsIngest.Models;
using LogIngester.DnsIngest.Services;
using Microsoft.AspNetCore.Mvc;

namespace LogIngester.DnsIngest.Controllers
{
    [ApiController]
    [Route("ingest")]
    public class IngestController : Controller
    {
        private readonly IIngestWorker _ingestWorker;

        public IngestController(IIngestWorker ingestWorker)
        {
            _ingestWorker = ingestWorker;
        }

        [HttpPost]
        public IActionResult Ingest([FromBody] DnsLog dnsLog, CancellationToken token)
        {
            if (!dnsLog.Timestamp.HasValue)
            {
                dnsLog.Timestamp = DateTime.UtcNow;
            }

            var response = new
            {
                InQueue = _ingestWorker.AddToProcess(dnsLog)
            };
            return Ok(response);
        }
    }
}