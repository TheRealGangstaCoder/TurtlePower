using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Turtle.Services;

namespace Turtle.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TurtleController : ControllerBase
    {

        private readonly ILogger<TurtleController> _logger;
        private readonly HttpClient httpClient;

        TurtlesConfiguration _config { get; }

        public TurtleController(ILogger<TurtleController> logger, TurtlesConfiguration config, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _config = config;
            httpClient = clientFactory.CreateClient("HttpClientWithSSLUntrusted");
        }

        public dynamic Any()
        {
            ProcessXForwardedForHeaders();
            
            // determine next hop
            Turtle turtle;
            if (HttpContext.Request.Headers.ContainsKey("destination"))
            {
                var destination = new Uri(HttpContext.Request.Headers["destination"]);
                string path = destination.GetLeftPart(UriPartial.Path);

                try
                {
                    turtle = _config.Turtle.Where(x => x.DestinationUri.GetLeftPart(UriPartial.Path).ToLower().StartsWith(path.ToLower())).SingleOrDefault();
                }
                catch
                {
                    return StatusCode(500, "more than 1 hop configuration found for " + destination);
                }

                if (turtle is null)
                {
                    return NotFound("Turtle cannot doesn't know where to go, update configuration for " + destination);
                }
            }
            else
            {
                return NotFound("Turtle cannot doesn't know where to go, no destination passed in the header");
            }
            var nextHop = turtle.NextHopUri ?? turtle.DestinationUri;

            // proxy request
            var upstreamRequest = ProxyService.CreateProxyHttpRequest(HttpContext.Request, nextHop);
            var proxy = new ProxyService(HttpContext, upstreamRequest, httpClient);

            var result = proxy.Send().Result;

            return new ObjectResult(result.Content.ReadAsStringAsync().Result) { StatusCode = (int)result.StatusCode };

        }

        private void ProcessXForwardedForHeaders()
        {
            var serverIp = HttpContext.Connection.RemoteIpAddress;

            // good netizen stuff
            if (HttpContext.Request.Headers.ContainsKey("x-forwarded-for"))
            {
                HttpContext.Request.Headers["X-Forwarded-For"] = ";" + serverIp.ToString();
            }
            else
            {
                HttpContext.Request.Headers.Add("X-Forwarded-For", serverIp.ToString());
            }
        }
    }
}
