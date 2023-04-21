using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RateLimiting.Filters;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public ResourcesController(ILogger<ResourcesController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [ServiceFilter(typeof(TokenBucketLimit))]
        public IEnumerable<string> Get()
        {
            var rng = new Random();
            return Enumerable
                .Range(1, 5)
                .Select(index => Summaries[rng.Next(Summaries.Length)])
                .ToArray();
        }
    }
}
