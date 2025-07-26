// OpCentrix/Services/ErrorLoggingMiddleware.cs
using System.Diagnostics;
using System.Text.Json;

namespace OpCentrix.Services
{
    /// <summary>
    /// Comprehensive error logging middleware for production-ready error tracking
    /// Captures all server-side errors across all pages with detailed context
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
            
            // Enhanced request logging with more context
            _logger.LogInformation("?? [REQUEST-{OperationId}] {Method} {Path} | User: {User} | IP: {IP} | UserAgent: {UserAgent}",
                operationId,
                context.Request.Method,
                context.Request.Path,
                context.User?.Identity?.Name ?? "Anonymous",
                GetClientIP(context),
                GetUserAgent(context));

            try
            {
                await _next(context);
                
                stopwatch.Stop();
                
                // Log successful completion with performance metrics
                var statusClass = GetStatusClass(context.Response.StatusCode);
                _logger.LogInformation("{StatusIcon} [REQUEST-{OperationId}] Completed {Method} {Path} | Status: {StatusCode} | Duration: {ElapsedMs}ms | Size: {ContentLength}",
                    statusClass.Icon,
                    operationId,
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    context.Response.ContentLength ?? 0);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                // Comprehensive error logging with full context
                await LogError(context, ex, operationId, stopwatch.ElapsedMilliseconds);
                
                // Re-throw to let other error handlers deal with it
                throw;
            }
        }

        private async Task LogError(HttpContext context, Exception ex, string operationId, long elapsedMs)
        {
            try
            {
                // Gather comprehensive error context
                var errorContext = await BuildErrorContext(context, ex, elapsedMs);
                
                // Primary error log with structured data
                _logger.LogError(ex, 
                    "?? [ERROR-{OperationId}] {ExceptionType} in {Method} {Path}\n" +
                    "?? User: {User} (Auth: {IsAuthenticated}) | IP: {RemoteIP}\n" +
                    "?? UserAgent: {UserAgent}\n" +
                    "?? Duration: {ElapsedMs}ms | Status: {StatusCode}\n" +
                    "?? Message: {ExceptionMessage}\n" +
                    "?? Context: {Context}",
                    operationId,
                    ex.GetType().Name,
                    context.Request.Method,
                    context.Request.Path,
                    context.User?.Identity?.Name ?? "Anonymous",
                    context.User?.Identity?.IsAuthenticated ?? false,
                    GetClientIP(context),
                    GetUserAgent(context),
                    elapsedMs,
                    context.Response.StatusCode,
                    ex.Message,
                    JsonSerializer.Serialize(errorContext, new JsonSerializerOptions { WriteIndented = true }));

                // Log stack trace separately for better readability
                _logger.LogError("?? [ERROR-{OperationId}-STACK] Stack Trace:\n{StackTrace}", 
                    operationId, ex.StackTrace ?? "No stack trace available");

                // Log inner exceptions with hierarchy
                await LogInnerExceptions(ex, operationId);

                // Log request body for POST/PUT requests
                await LogRequestBody(context, operationId);

                // Log additional diagnostics
                LogDiagnostics(context, operationId, ex);

            }
            catch (Exception loggingEx)
            {
                // Fallback logging if our comprehensive logging fails
                _logger.LogCritical(loggingEx, 
                    "?? [ERROR-{OperationId}-FALLBACK] Failed to log error details. Original error: {OriginalException}",
                    operationId, ex.Message);
            }
        }

        private async Task<object> BuildErrorContext(HttpContext context, Exception ex, long elapsedMs)
        {
            var headers = new Dictionary<string, string>();
            foreach (var header in context.Request.Headers)
            {
                // Sanitize sensitive headers
                var value = IsSensitiveHeader(header.Key) ? "***REDACTED***" : header.Value.ToString();
                headers[header.Key] = value;
            }

            var userClaims = new Dictionary<string, string>();
            if (context.User?.Claims != null)
            {
                foreach (var claim in context.User.Claims)
                {
                    userClaims[claim.Type] = claim.Value;
                }
            }

            return new
            {
                Timestamp = DateTime.UtcNow,
                OperationInfo = new
                {
                    ElapsedMilliseconds = elapsedMs,
                    MachineName = Environment.MachineName,
                    ProcessId = Environment.ProcessId,
                    ThreadId = Environment.CurrentManagedThreadId
                },
                Request = new
                {
                    Method = context.Request.Method,
                    Scheme = context.Request.Scheme,
                    Host = context.Request.Host.Value,
                    Path = context.Request.Path.Value,
                    PathBase = context.Request.PathBase.Value,
                    QueryString = context.Request.QueryString.Value,
                    ContentType = context.Request.ContentType,
                    ContentLength = context.Request.ContentLength,
                    Headers = headers,
                    Cookies = context.Request.Cookies.ToDictionary(c => c.Key, c => IsSensitiveCookie(c.Key) ? "***REDACTED***" : c.Value)
                },
                User = new
                {
                    Name = context.User?.Identity?.Name ?? "Anonymous",
                    IsAuthenticated = context.User?.Identity?.IsAuthenticated ?? false,
                    AuthenticationType = context.User?.Identity?.AuthenticationType,
                    Claims = userClaims
                },
                Connection = new
                {
                    Id = context.Connection.Id,
                    RemoteIpAddress = GetClientIP(context),
                    RemotePort = context.Connection.RemotePort,
                    LocalIpAddress = context.Connection.LocalIpAddress?.ToString(),
                    LocalPort = context.Connection.LocalPort
                },
                Response = new
                {
                    StatusCode = context.Response.StatusCode,
                    ContentType = context.Response.ContentType,
                    ContentLength = context.Response.ContentLength,
                    HasStarted = context.Response.HasStarted
                },
                Exception = new
                {
                    Type = ex.GetType().FullName,
                    Message = ex.Message,
                    Source = ex.Source,
                    HelpLink = ex.HelpLink,
                    HResult = ex.HResult,
                    Data = ex.Data.Count > 0 ? ex.Data.Cast<System.Collections.DictionaryEntry>()
                        .ToDictionary(entry => entry.Key.ToString(), entry => entry.Value?.ToString()) : null
                }
            };
        }

        private async Task LogInnerExceptions(Exception ex, string operationId)
        {
            var innerEx = ex.InnerException;
            var level = 1;
            
            while (innerEx != null && level <= 10) // Limit to prevent infinite loops
            {
                _logger.LogError("?? [ERROR-{OperationId}-INNER-{Level}] {Type}: {Message}",
                    operationId, level, innerEx.GetType().Name, innerEx.Message);
                
                if (!string.IsNullOrEmpty(innerEx.StackTrace))
                {
                    _logger.LogDebug("?? [ERROR-{OperationId}-INNER-{Level}-STACK] {StackTrace}",
                        operationId, level, innerEx.StackTrace);
                }
                
                innerEx = innerEx.InnerException;
                level++;
            }
        }

        private async Task LogRequestBody(HttpContext context, string operationId)
        {
            if (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "PATCH")
            {
                try
                {
                    if (context.Request.ContentLength > 0 && context.Request.ContentLength < 1024 * 1024) // Limit to 1MB
                    {
                        context.Request.EnableBuffering();
                        context.Request.Body.Position = 0;
                        
                        using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
                        var body = await reader.ReadToEndAsync();
                        context.Request.Body.Position = 0;
                        
                        if (!string.IsNullOrEmpty(body))
                        {
                            var sanitizedBody = SanitizeRequestBody(body);
                            _logger.LogInformation("?? [ERROR-{OperationId}-BODY] Request Body: {Body}",
                                operationId, sanitizedBody);
                        }
                    }
                }
                catch (Exception bodyEx)
                {
                    _logger.LogWarning("?? [ERROR-{OperationId}-BODY] Failed to read request body: {BodyError}",
                        operationId, bodyEx.Message);
                }
            }
        }

        private void LogDiagnostics(HttpContext context, string operationId, Exception ex)
        {
            try
            {
                // Log server diagnostics
                _logger.LogInformation("??? [ERROR-{OperationId}-DIAGNOSTICS] " +
                    "Memory: {WorkingSet}MB | " +
                    "Threads: {ThreadCount} | " +
                    "Handles: {HandleCount} | " +
                    "CPU Time: {TotalProcessorTime}",
                    operationId,
                    Environment.WorkingSet / 1024 / 1024,
                    Process.GetCurrentProcess().Threads.Count,
                    Process.GetCurrentProcess().HandleCount,
                    Process.GetCurrentProcess().TotalProcessorTime);

                // Log assembly and environment info
                var entryAssembly = System.Reflection.Assembly.GetEntryAssembly();
                _logger.LogInformation("??? [ERROR-{OperationId}-ENVIRONMENT] " +
                    "Assembly: {AssemblyName} v{AssemblyVersion} | " +
                    "Runtime: {RuntimeVersion} | " +
                    "OS: {OSDescription}",
                    operationId,
                    entryAssembly?.GetName().Name ?? "Unknown",
                    entryAssembly?.GetName().Version?.ToString() ?? "Unknown",
                    Environment.Version,
                    Environment.OSVersion.VersionString);
            }
            catch (Exception diagEx)
            {
                _logger.LogWarning("?? [ERROR-{OperationId}] Failed to collect diagnostics: {DiagError}",
                    operationId, diagEx.Message);
            }
        }

        private string SanitizeRequestBody(string body)
        {
            if (string.IsNullOrEmpty(body)) return body;

            // Remove potentially sensitive information
            var sensitiveFields = new[] 
            { 
                "password", "token", "secret", "key", "credential", "auth", 
                "apikey", "api_key", "access_token", "refresh_token", "sessionId",
                "ssn", "social", "card", "account", "routing"
            };
            
            foreach (var field in sensitiveFields)
            {
                // Handle JSON format
                body = System.Text.RegularExpressions.Regex.Replace(
                    body, 
                    $@"(""{field}""?\s*:\s*[""]?)([^"",\s}}]+)(["",\s}}]?)",
                    "$1***REDACTED***$3",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                // Handle form data format
                body = System.Text.RegularExpressions.Regex.Replace(
                    body,
                    $@"({field}=)([^&\s]+)",
                    "$1***REDACTED***",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            
            // Limit body size in logs
            if (body.Length > 2000)
            {
                body = body.Substring(0, 2000) + "... [TRUNCATED]";
            }
            
            return body;
        }

        private string GetClientIP(HttpContext context)
        {
            // Try to get real IP from various headers (load balancer, proxy, etc.)
            var headers = new[] { "X-Forwarded-For", "X-Real-IP", "CF-Connecting-IP", "X-Client-IP" };
            
            foreach (var header in headers)
            {
                if (context.Request.Headers.ContainsKey(header))
                {
                    var value = context.Request.Headers[header].FirstOrDefault();
                    if (!string.IsNullOrEmpty(value))
                    {
                        // X-Forwarded-For can contain multiple IPs, take the first one
                        return value.Split(',')[0].Trim();
                    }
                }
            }
            
            return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }

        private string GetUserAgent(HttpContext context)
        {
            return context.Request.Headers["User-Agent"].FirstOrDefault() ?? "Unknown";
        }

        private (string Icon, string Name) GetStatusClass(int statusCode)
        {
            return statusCode switch
            {
                >= 200 and < 300 => ("?", "Success"),
                >= 300 and < 400 => ("??", "Redirect"),
                >= 400 and < 500 => ("??", "Client Error"),
                >= 500 => ("??", "Server Error"),
                _ => ("?", "Unknown")
            };
        }

        private bool IsSensitiveHeader(string headerName)
        {
            var sensitiveHeaders = new[] 
            { 
                "authorization", "cookie", "x-api-key", "x-auth-token", 
                "x-access-token", "authentication", "proxy-authorization" 
            };
            
            return sensitiveHeaders.Contains(headerName.ToLowerInvariant());
        }

        private bool IsSensitiveCookie(string cookieName)
        {
            var sensitiveCookies = new[] 
            { 
                "auth", "session", "token", "identity", "AspNetCore.Identity.Application",
                "AspNetCore.Antiforgery", "login", "jwt", "bearer"
            };
            
            return sensitiveCookies.Any(sensitive => 
                cookieName.ToLowerInvariant().Contains(sensitive));
        }
    }
}