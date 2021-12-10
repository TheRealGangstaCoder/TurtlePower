using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Turtle.Services
{
    public class ProxyService
    {
        private readonly HttpClient _httpClient;

        public HttpContext HttpContext { get; }
        public HttpRequestMessage UpstreamRequest { get; }

        internal ProxyService(HttpContext httpContext, HttpRequestMessage upstreamRequest, HttpClient httpClient)
        {
            _httpClient = httpClient;
            HttpContext = httpContext;
            UpstreamRequest = upstreamRequest;
        }

        public async Task<HttpResponseMessage> Send()
        {
            try
            {
                return await _httpClient
                    .SendAsync(
                        UpstreamRequest,
                        HttpCompletionOption.ResponseHeadersRead,
                        HttpContext.RequestAborted)
                    .ConfigureAwait(false);
            }
            catch (TaskCanceledException ex) when (ex.InnerException is IOException)
            {
                return new HttpResponseMessage(HttpStatusCode.GatewayTimeout);
            }
            catch (OperationCanceledException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadGateway);
            }
            catch (HttpRequestException ex)
                when (ex.InnerException is IOException || ex.InnerException is SocketException)
            {
                return new HttpResponseMessage(HttpStatusCode.BadGateway);
            }
        }

        public static HttpRequestMessage CreateProxyHttpRequest(HttpRequest request, Uri ProxyTo)
        {
            var requestMessage = new HttpRequestMessage();
            if (request.ContentLength > 0 || request.Headers.ContainsKey("Transfer-Encoding"))
            {
                var streamContent = new StreamContent(request.Body);
                requestMessage.Content = streamContent;
            }

            foreach (var header in request.Headers)
            {
                var headerName = header.Key;
                var value = header.Value;

                if (string.Equals(headerName, HeaderNames.Cookie, StringComparison.OrdinalIgnoreCase) && value.Count > 1)
                {
                    value = string.Join("; ", value);
                }

                if (value.Count == 1)
                {
                    string headerValue = value;
                    if (!requestMessage.Headers.TryAddWithoutValidation(headerName, headerValue))
                    {
                        requestMessage.Content?.Headers.TryAddWithoutValidation(headerName, headerValue);
                    }
                }
                else
                {
                    string[] headerValues = value;
                    if (!requestMessage.Headers.TryAddWithoutValidation(headerName, headerValues))
                    {
                        requestMessage.Content?.Headers.TryAddWithoutValidation(headerName, headerValues);
                    }
                }
                if (headerName == "destination")
                {
                    requestMessage.Content?.Headers.Add(headerName, value.ToString());
                }
            }
            requestMessage.Method = new HttpMethod(request.Method);

            requestMessage.RequestUri = ProxyTo;
            requestMessage.Headers.Host = ProxyTo.Host;

            return requestMessage;
        }
    }
}
