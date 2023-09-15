using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DigitalPrinting.Models
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private static readonly NLog.Logger _nlog = LogManager.GetCurrentClassLogger();

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // Log the request path and method
                _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");
                _nlog.Info($"Request: {context.Request.Method} {context.Request.Path}");

                // Capture the response stream to intercept the response content
                var originalBodyStream = context.Response.Body;
                using (var responseBody = new MemoryStream())
                {
                    context.Response.Body = responseBody;

                    // Continue processing the request
                    await _next(context);

                    // Log the response status code
                    _logger.LogInformation($"Response: {context.Response.StatusCode}");
                    _nlog.Info($"Response: {context.Response.StatusCode}");

                    // Log the response content (if not too large)
                    if (context.Response.ContentLength <= 2048)
                    {
                        responseBody.Seek(0, SeekOrigin.Begin);
                        var responseContent = new StreamReader(responseBody).ReadToEnd();
                        _logger.LogInformation($"Response Content: {responseContent}");
                        _nlog.Info($"Response Content: {responseContent}");
                    }

                    // Restore the original response stream
                    responseBody.Seek(0, SeekOrigin.Begin);
                    await responseBody.CopyToAsync(originalBodyStream);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex}");
                _nlog.Error($"Error: {ex}");
                throw;
            }
        }
    }
}
