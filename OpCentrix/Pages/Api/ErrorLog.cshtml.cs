using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace OpCentrix.Pages.Api
{
    public class ErrorLogModel : PageModel
    {
        private readonly ILogger<ErrorLogModel> _logger;

        public ErrorLogModel(ILogger<ErrorLogModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnPostAsync()
        {
            var operationId = Guid.NewGuid().ToString("N")[..8];
            
            try
            {
                // Read the JSON body
                string requestBody;
                using (var reader = new StreamReader(Request.Body))
                {
                    requestBody = reader.ReadToEnd();
                }
                
                if (string.IsNullOrEmpty(requestBody))
                {
                    _logger.LogWarning("?? [ERROR-API-{OperationId}] Empty request body received", operationId);
                    return BadRequest(new { error = "Empty request body", operationId });
                }

                // Parse the error log
                var errorLog = JsonSerializer.Deserialize<ClientErrorLog>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (errorLog == null)
                {
                    _logger.LogWarning("?? [ERROR-API-{OperationId}] Failed to parse error log JSON", operationId);
                    return BadRequest(new { error = "Invalid JSON format", operationId });
                }

                // Extract user information
                var userId = User.Identity?.Name ?? "Anonymous";
                var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "Unknown";

                // Log the client-side error with full context
                _logger.LogError("?? [CLIENT-ERROR-{OperationId}] User: {UserId} ({Role}) | {Category}.{Operation}: {Message} | URL: {Url} | Browser: {UserAgent} | Context: {Context}", 
                    operationId,
                    userId,
                    userRole,
                    errorLog.Category ?? "UNKNOWN",
                    errorLog.Operation ?? "unknown",
                    errorLog.Error?.Message ?? "No message",
                    errorLog.Context?.GetValueOrDefault("url") ?? "unknown",
                    errorLog.Context?.GetValueOrDefault("userAgent") ?? "unknown",
                    JsonSerializer.Serialize(errorLog.Context ?? new Dictionary<string, object>())
                );

                // Log browser details separately for analysis
                if (errorLog.Browser != null)
                {
                    _logger.LogDebug("??? [CLIENT-ERROR-{OperationId}] Browser Details: {BrowserInfo}", 
                        operationId, JsonSerializer.Serialize(errorLog.Browser));
                }

                // Log error stack trace if available
                if (!string.IsNullOrEmpty(errorLog.Error?.Stack))
                {
                    _logger.LogDebug("?? [CLIENT-ERROR-{OperationId}] Stack Trace: {StackTrace}", 
                        operationId, errorLog.Error.Stack);
                }

                return new JsonResult(new { 
                    success = true, 
                    operationId, 
                    message = "Error logged successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "? [ERROR-API-{OperationId}] JSON parsing error: {ErrorMessage}", 
                    operationId, jsonEx.Message);
                
                return BadRequest(new { 
                    error = "JSON parsing failed", 
                    operationId, 
                    details = jsonEx.Message 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? [ERROR-API-{OperationId}] Failed to log client error: {ErrorMessage}", 
                    operationId, ex.Message);
                
                return StatusCode(500, new { 
                    error = "Failed to log error", 
                    operationId, 
                    details = ex.Message 
                });
            }
        }
    }

    // Model for client-side error logging
    public class ClientErrorLog
    {
        public string? Id { get; set; }
        public string? Timestamp { get; set; }
        public string? Category { get; set; }
        public string? Operation { get; set; }
        public ClientError? Error { get; set; }
        public Dictionary<string, object>? Context { get; set; }
        public Dictionary<string, object>? Browser { get; set; }
    }

    public class ClientError
    {
        public string? Message { get; set; }
        public string? Stack { get; set; }
        public string? Type { get; set; }
    }
}