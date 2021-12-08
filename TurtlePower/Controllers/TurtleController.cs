using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Turtle.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TurtleController : ControllerBase
    {

        private readonly ILogger<TurtleController> _logger;
        TurtlesConfiguration _config { get; }

        public TurtleController(ILogger<TurtleController> logger, TurtlesConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public dynamic Any()
        {

            var headers = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            var serverIp = HttpContext.Connection.RemoteIpAddress;
            NextHop nextHop = NextHop.Turtle; // assume we are the first turtle

            foreach (var header in HttpContext.Request.Headers)
            {
                headers.Add(header.Key, header.Value);
            }

            // good netizen stuff
            if (headers.ContainsKey("x-forwarded-for"))
            {
                headers["X-Forwarded-For"] = ";" + serverIp.ToString();
            }
            else
            {
                headers.Add("X-Forwarded-For", serverIp.ToString());
            }



            // compare with config to determine next hop

            if (headers.ContainsKey("destination"))
            {
                var destination = new Uri(headers["destination"]);
                string path = destination.GetLeftPart(UriPartial.Path);

                var turtle = _config.Turtle.Where(x => x.Uri.ToLower() == path.ToLower()).SingleOrDefault();

                if (turtle is null)
                {
                    return NotFound("Turtle cannot doesn't know where to go, update configuration for " + destination);
                }

                nextHop = turtle.NextHop;

            }
            else
            {
                    return NotFound("Turtle cannot doesn't know where to go, no destination passed in the header");
            }


            // now we are in business

            var queryString = HttpContext.Request.QueryString;
            var method = HttpContext.Request.Method;
            var body = method.ToLower() != "get" ? HttpContext.Request.Body : null;
            var host = HttpContext.Request.Host.Host; // dont need the port here
            var contentType = HttpContext.Request.ContentType;
            var cookies = HttpContext.Request.Cookies; // only for the same of completeness, it would be a crazy world if api are sending cookies


            var isTurtle = headers.ContainsKey("turtle");


            if (nextHop == NextHop.Turtle && !isTurtle)
            {
                // this is the first turtle, and we dont have a shell

                // lets make a shell
            }
            else if (nextHop == NextHop.Turtle && isTurtle)
            {
                // we have a shell, and we are turtle!
                // send the shell downstream
            }
            if (nextHop == NextHop.Destination && isTurtle)
            {
                // unpack shell and send downstream
            }
            else
            {
                // this is the first turtle, and next hop is destination, and we dont have a shell
                // just send downstream
            }


            // add headers and body to Shell
            // update x-forwarded-for
            // send to next hop


            // parse response and send upstream

            return "response " + DateTime.Now.ToString();
        }
    }
}
