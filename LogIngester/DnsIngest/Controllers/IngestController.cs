using System;
using System.Threading;
using System.Threading.Tasks;
using LogIngester.DnsIngest.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LogIngester.DnsIngest.Controllers
{
    [ApiController]
    [Route("ingest")]
    public class IngestController : Controller
    {
        private readonly ILogger<IngestController> _logger;

        public IngestController(ILogger<IngestController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Ingest([FromBody] DnsLog dnsLog, CancellationToken token)
        {
            _logger.Log(LogLevel.Information, "Request received", dnsLog);
            dnsLog.Timestamp = DateTime.UtcNow;
            dnsLog.Query++;
            return Ok(dnsLog);
        }
    }
}