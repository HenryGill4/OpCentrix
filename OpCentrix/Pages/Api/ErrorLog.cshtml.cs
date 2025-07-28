using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace OpCentrix.Pages.Api
{
    /// <summary>
    /// API endpoint for receiving client-side error logs
    /// Enhanced with comprehensive error tracking for click-through testing
    /// FIXED: Disable antiforgery validation for API endpoints
    /// </summary>
    [AllowAnonymous]
    [IgnoreAntiforgeryToken] // FIXED: API endpoints don't need CSRF protection
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorLogModel : PageModel
    {
        private readonly ILogger<ErrorLogModel> _logger;

        public ErrorLogModel(ILogger<ErrorLogModel> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Handle client-side error reports via POST
        /// </summary>
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Read the request body
                using var reader = new StreamReader(Request.Body);
                var requestBody = await reader.ReadToEndAsync();

                if (string.IsNullOrEmpty(requestBody))
                {
                    _logger.LogWarning("?? [CLIENT-ERROR-API] Empty request body received");
                    return BadRequest("Empty request body");
                }

                // Parse the error data
                var errorData = JsonSerializer.Deserialize<ClientErrorData>(requestBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (errorData == null)
                {
                    _logger.LogWarning("?? [CLIENT-ERROR-API] Failed to parse error data");
                    return BadRequest("Invalid error data format");
                }

                // Generate operation ID for tracking
                var operationId = Guid.NewGuid().ToString("N")[..8];

                // Get user information
                var userName = User?.Identity?.Name ?? "Anonymous";
                var userAgent = Request.Headers["User-Agent"].ToString();
                var ipAddress = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

                // Log the error with comprehensive details
                _logger.LogError("? [CLIENT-ERROR-{OperationId}] {Category} error in {Operation}: {ErrorMessage}\n" +
                    "?? Context: URL={Url}, User={User}, IP={IP}\n" +
                    "?? Browser: {UserAgent}\n" +
                    "?? Details: {ErrorDetails}\n" +
                    "?? Stack: {ErrorStack}\n" +
                    "?? Additional Context: {AdditionalContext}",
                    operationId,
                    errorData.Category,
                    errorData.Operation,
                    errorData.Error?.Message,
                    errorData.Context?.ContainsKey("url") == true ? errorData.Context["url"] : "Unknown",
                    userName,
                    ipAddress,
                    userAgent,
                    JsonSerializer.Serialize(errorData.Error),
                    errorData.Error?.Stack ?? "No stack trace",
                    JsonSerializer.Serialize(errorData.Context));

                // Log page monitoring data if available
                if (errorData.Context?.ContainsKey("page") == true)
                {
                    _logger.LogInformation("?? [PAGE-MONITOR-{OperationId}] Page activity logged for {Page}",
                        operationId, errorData.Context["page"]);
                }

                // Log interaction data if available
                if (errorData.Context?.ContainsKey("interactions") == true)
                {
                    _logger.LogInformation("?? [INTERACTION-{OperationId}] User interactions: {Interactions}",
                        operationId, errorData.Context["interactions"]);
                }

                // Return success response
                return new JsonResult(new { 
                    success = true, 
                    operationId = operationId,
                    timestamp = DateTime.UtcNow 
                });
            }
            catch (Exception ex)
            {
                var errorId = Guid.NewGuid().ToString("N")[..8];
                _logger.LogError(ex, "?? [CLIENT-ERROR-API-{ErrorId}] Failed to process client error log", errorId);
                
                return StatusCode(500, new { 
                    success = false, 
                    error = "Failed to process error log",
                    errorId = errorId 
                });
            }
        }

        /// <summary>
        /// Handle GET requests (should return method not allowed)
        /// </summary>
        public IActionResult OnGet()
        {
            return StatusCode(405, "Method not allowed. Use POST to submit error logs.");
        }
    }

    /// <summary>
    /// Data model for client-side error reports
    /// Enhanced for comprehensive click-through testing
    /// </summary>
    public class ClientErrorData
    {
        public string? Id { get; set; }
        public string? Timestamp { get; set; }
        public string? Category { get; set; }
        public string? Operation { get; set; }
        public ClientErrorDetails? Error { get; set; }
        public Dictionary<string, object>? Context { get; set; }
        public ClientBrowserInfo? Browser { get; set; }
    }

    /// <summary>
    /// Error details from client
    /// </summary>
    public class ClientErrorDetails
    {
        public string? Message { get; set; }
        public string? Stack { get; set; }
        public string? Type { get; set; }
    }

    /// <summary>
    /// Browser information from client
    /// </summary>
    public class ClientBrowserInfo
    {
        public string? Viewport { get; set; }
        public string? Screen { get; set; }
        public double? PixelRatio { get; set; }
        public bool? Online { get; set; }
    }
}