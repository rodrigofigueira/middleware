using Microsoft.AspNetCore.Mvc.Controllers;
using System.Diagnostics;
using System.Text;

namespace API.Middlewares
{
    public class LoggingMiddleware(
        RequestDelegate next,
        ILogger<LoggingMiddleware> logger)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            var timer = new Stopwatch();
            timer.Start();
            TimeSpan timeTaken;
            string traceIdentifier = context.TraceIdentifier;
            string key = CreateKeyForLookup(context);
            var _eventId = _eventLookup.TryGetValue(key, out var foundEventId) ? foundEventId : new EventId(-1, key);
            string method = context.Request.Method;


            if (method == "POST" || method == "PUT" || method == "PATCH")
            {
                var payload = await ReadRequestBodyAsync(context.Request);
                logger.LogInformation(_eventId, "Payload: {payload}", payload);
                context.Request.Body.Position = 0;
            }

            await next(context);
            timer.Stop();
            timeTaken = timer.Elapsed;
            logger.LogInformation(_eventId, "The request {traceIdentifier} took {timeTaken}", traceIdentifier, timeTaken);
            logger.LogInformation(_eventId, "Status Code Response: {statusCode}", context.Response.StatusCode);
        }

        private static string CreateKeyForLookup(HttpContext context)
        {
            Endpoint endpoint = context.GetEndpoint()!;
            var controller = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>()?.ControllerName;
            var action = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>()?.ActionName;
            return $"{controller}_{action}";
        }

        private static readonly Dictionary<string, EventId> _eventLookup = new()
        {
            { "WeatherForecast_Get", new EventId(1, "WeatherForecast_Get") },
            { "WeatherForecast_GetWithRandomTimeResponse", new EventId(2, "WeatherForecast_GetWithRandomTimeResponse") },
            { "WeatherForecast_Post", new EventId(2, "WeatherForecast_Post") },
        };

        private static async Task<string> ReadRequestBodyAsync(HttpRequest request)
        {
            request.EnableBuffering();
            request.Body.Position = 0;

            using var reader = new StreamReader(request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            var body = await reader.ReadToEndAsync();
            return body;
        }

    }
}
