// OpCentrix/Services/ErrorLoggingMiddleware.cs
using System.Diagnostics;
using System.Text.Json;

namespace OpCentrix.Services
{
    /// <summary>
    /// Comprehensive error logging middleware for click-through testing
    /// Captures all server-side errors across all pages
    /// </summary>
    public class ErrorLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorLoggingMiddleware> _logger;

        public ErrorLoggingMiddleware(RequestDelegate next, ILogger<ErrorLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            var stopwatch = Stopwatch.StartNew();
            
            // Log request start
            _logger.LogInformation("?? [REQUEST-{OperationId}] Starting {Method} {Path} from {IP} - User: {User}",
                operationId,
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress,
                context.User?.Identity?.Name ?? "Anonymous");

            try
            {
                await _next(context);
                
                stopwatch.Stop();
                
                // Log successful completion
                _logger.LogInformation("? [REQUEST-{OperationId}] Completed {Method} {Path} in {ElapsedMs}ms - Status: {StatusCode}",
                    operationId,
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Comprehensive error logging
                await LogError(context, ex, operationId, stopwatch.ElapsedMilliseconds);
                
                // Don't re-throw - let other error handlers deal with it
                throw;
            }
        }

        private async Task LogError(HttpContext context, Exception ex, string operationId, long elapsedMs)
        {
            try
            {
                // Gather comprehensive context
                var errorContext = new
                {
                    Request = new
                    {
                        Method = context.Request.Method,
                        Path = context.Request.Path.Value,
                        QueryString = context.Request.QueryString.Value,
                        Headers = context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                        ContentType = context.Request.ContentType,
                        ContentLength = context.Request.ContentLength
                    },
                    User = new
                    {
                        Name = context.User?.Identity?.Name ?? "Anonymous",
                        IsAuthenticated = context.User?.Identity?.IsAuthenticated ?? false,
                        Claims = context.User?.Claims?.ToDictionary(c => c.Type, c => c.Value) ?? new Dictionary<string, string>()
                    },
                    Connection = new
                    {
                        RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
                        LocalIpAddress = context.Connection.LocalIpAddress?.ToString(),
                        RemotePort = context.Connection.RemotePort,
                        LocalPort = context.Connection.LocalPort
                    },
                    Response = new
                    {
                        StatusCode = context.Response.StatusCode,
                        ContentType = context.Response.ContentType,
                        HasStarted = context.Response.HasStarted
                    },
                    Timing = new
                    {
                        ElapsedMilliseconds = elapsedMs,
                        Timestamp = DateTime.UtcNow
                    }
                };

                // Log the detailed error
                _logger.LogError(ex, 
                    "?? [ERROR-{OperationId}] Unhandled exception in {Method} {Path}\n" +
                    "?? User: {User} (Authenticated: {IsAuthenticated})\n" +
                    "?? Remote IP: {RemoteIP}, UserAgent: {UserAgent}\n" +
                    "?? Request Duration: {ElapsedMs}ms\n" +
                    "?? Exception Type: {ExceptionType}\n" +
                    "?? Exception Message: {ExceptionMessage}\n" +
                    "?? Context: {Context}\n" +
                    "?? Stack Trace: {StackTrace}",
                    operationId,
                    context.Request.Method,
                    context.Request.Path,
                    context.User?.Identity?.Name ?? "Anonymous",
                    context.User?.Identity?.IsAuthenticated ?? false,
                    context.Connection.RemoteIpAddress,
                    context.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown",
                    elapsedMs,
                    ex.GetType().Name,
                    ex.Message,
                    JsonSerializer.Serialize(errorContext, new JsonSerializerOptions { WriteIndented = true }),
                    ex.StackTrace);

                // Log inner exceptions if present
                var innerEx = ex.InnerException;
                var innerLevel = 1;
                while (innerEx != null && innerLevel <= 5) // Limit to 5 levels
                {
                    _logger.LogError("?? [ERROR-{OperationId}-INNER-{Level}] Inner Exception: {Type} - {Message}\n" +
                        "?? Inner Stack: {StackTrace}",
                        operationId,
                        innerLevel,
                        innerEx.GetType().Name,
                        innerEx.Message,
                        innerEx.StackTrace);
                    
                    innerEx = innerEx.InnerException;
                    innerLevel++;
                }

                // Try to read request body for POST requests (if not already read)
                if (context.Request.Method == "POST" && context.Request.ContentLength > 0)
                {
                    try
                    {
                        context.Request.EnableBuffering();
                        context.Request.Body.Position = 0;
                        
                        using var reader = new StreamReader(context.Request.Body);
                        var body = await reader.ReadToEndAsync();
                        
                        if (!string.IsNullOrEmpty(body))
                        {
                            // Sanitize potentially sensitive data
                            var sanitizedBody = SanitizeRequestBody(body);
                            _logger.LogInformation("?? [ERROR-{OperationId}-BODY] Request Body (sanitized): {Body}",
                                operationId, sanitizedBody);
                        }
                    }
                    catch (Exception bodyEx)
                    {
                        _logger.LogWarning("?? [ERROR-{OperationId}] Failed to read request body: {BodyError}",
                            operationId, bodyEx.Message);
                    }
                }
            }
            catch (Exception loggingEx)
            {
                // Fallback logging if our comprehensive logging fails
                _logger.LogCritical(loggingEx, 
                    "?? [ERROR-{OperationId}] Failed to log error details for original exception: {OriginalException}",
                    operationId, ex.Message);
            }
        }

        private string SanitizeRequestBody(string body)
        {
            // Remove potentially sensitive information from request body
            var sensitiveFields = new[] { "password", "token", "secret", "key", "credential" };
            
            foreach (var field in sensitiveFields)
            {
                // Simple regex to replace values of sensitive fields
                body = System.Text.RegularExpressions.Regex.Replace(
                    body, 
                    $@"(""{field}""?\s*[:=]\s*[""]?)([^"",\s}}]+)(["",\s}}]?)",
                    $"$1***SANITIZED***$3",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            
            // Limit body size in logs
            if (body.Length > 1000)
            {
                body = body.Substring(0, 1000) + "... [TRUNCATED]";
            }
            
            return body;
        }
    }
}