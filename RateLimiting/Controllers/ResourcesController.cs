using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RateLimiting.Filters;
using RateLimiting.Services;
using RateLimiting.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace RateLimiting.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourcesController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<ResourcesController> _logger;
        private readonly IRequestStore _requestStore;

        public ResourcesController(ILogger<ResourcesController> logger,
            IRequestStore requestStore)
        {
            _logger = logger;
            _requestStore = requestStore;
        }

        [HttpGet("token-bucket")]
        [ServiceFilter(typeof(TokenBucketLimit))]
        public IEnumerable<string> GetTokenBucket([FromHeader] string apiKey)
        {
            var rng = new Random();
            return Enumerable
                .Range(1, 5)
                .Select(index => Summaries[rng.Next(Summaries.Length)])
                .ToArray();
        }

        [HttpPost("leaky-bucket")]
        [ServiceFilter(typeof(LeakyBucketLimit))]
        public IActionResult PostLeakyBucket([FromBody] string data,
            [FromHeader] string apiKey)
        {
            string requestId = HttpHelper.GetHttpRequestId(HttpContext);

            _requestStore.AddRequestToStore(requestId, data);

            return StatusCode((int)HttpStatusCode.Created);
        }

        [HttpGet("fixed-window-counter")]
        [ServiceFilter(typeof(FixedWindowCounterLimit))]
        public IEnumerable<string> GetFixedWindowCounter([FromHeader] string apiKey)
        {
            var rng = new Random();
            return Enumerable
                .Range(1, 5)
                .Select(index => Summaries[rng.Next(Summaries.Length)])
                .ToArray();
        }

        [HttpGet("sliding-window-logs")]
        [ServiceFilter(typeof(SlidingWindowLogsLimit))]
        public IEnumerable<string> GetSlidingWindowLogs([FromHeader] string apiKey)
        {
            var rng = new Random();
            return Enumerable
                .Range(1, 5)
                .Select(index => Summaries[rng.Next(Summaries.Length)])
                .ToArray();
        }

        [HttpGet("sliding-window-counter-1")]
        [ServiceFilter(typeof(SlidingWindowCounter1Limit))]
        public IEnumerable<string> GetSlidingWindowCounter1([FromHeader] string apiKey)
        {
            var rng = new Random();
            return Enumerable
                .Range(1, 5)
                .Select(index => Summaries[rng.Next(Summaries.Length)])
                .ToArray();
        }

        [HttpGet("sliding-window-counter-2")]
        [ServiceFilter(typeof(SlidingWindowCounter2Limit))]
        public IEnumerable<string> GetSlidingWindowCounter2([FromHeader] string apiKey)
        {
            var rng = new Random();
            return Enumerable
                .Range(1, 5)
                .Select(index => Summaries[rng.Next(Summaries.Length)])
                .ToArray();
        }
    }
}
