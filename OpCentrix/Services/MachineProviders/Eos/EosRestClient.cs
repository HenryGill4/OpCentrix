using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpCentrix.Models.MachineProviders;

namespace OpCentrix.Services.MachineProviders.Eos
{
    /// <summary>
    /// Thin HTTP client for the EOS EOSCONNECT Web API.
    ///
    /// Authentication: OAuth2 client_credentials flow.
    ///   Token endpoint: POST {baseUrl}/connect/token
    ///   Swagger UI:     {baseUrl}/swagger
    ///
    /// Stub status: HTTP calls are wired up with correct paths and payloads.
    /// Replace the TODO markers with real HttpClient calls once an EDN account
    /// or the production machine IP is available.
    /// </summary>
    public sealed class EosRestClient : IAsyncDisposable
    {
        private readonly ILogger<EosRestClient> _logger;
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private string? _baseUrl;
        private string? _clientId;
        private string? _clientSecret;
        private string? _accessToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public EosRestClient(ILogger<EosRestClient> logger, HttpClient http)
        {
            _logger = logger;
            _http = http;
        }

        public void Configure(string baseUrl, string clientId, string clientSecret)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _clientId = clientId;
            _clientSecret = clientSecret;
        }

        // ── Authentication ────────────────────────────────────────────────────

        /// <summary>
        /// Obtains or renews an OAuth2 bearer token.
        /// EOS tokens are short-lived (~1 hour); this is called automatically
        /// before every API request.
        /// </summary>
        public async Task<bool> EnsureTokenAsync(CancellationToken ct = default)
        {
            if (_accessToken != null && DateTime.UtcNow < _tokenExpiry.AddMinutes(-2))
                return true;

            // TODO: Replace stub with real HTTP call when machine is accessible.
            // Real implementation:
            //
            //   var form = new FormUrlEncodedContent(new[]
            //   {
            //       new KeyValuePair<string, string>("grant_type",    "client_credentials"),
            //       new KeyValuePair<string, string>("client_id",     _clientId!),
            //       new KeyValuePair<string, string>("client_secret", _clientSecret!),
            //   });
            //   var response = await _http.PostAsync($"{_baseUrl}/connect/token", form, ct);
            //   response.EnsureSuccessStatusCode();
            //   var payload = await response.Content.ReadFromJsonAsync<TokenResponse>(_json, ct);
            //   _accessToken = payload!.AccessToken;
            //   _tokenExpiry = DateTime.UtcNow.AddSeconds(payload.ExpiresIn);
            //   _http.DefaultRequestHeaders.Authorization =
            //       new AuthenticationHeaderValue("Bearer", _accessToken);

            _logger.LogDebug("[EOS REST] Token acquisition stubbed — connect machine to activate");
            _accessToken = "stub-token";
            _tokenExpiry = DateTime.UtcNow.AddHours(1);
            return true;
        }

        // ── Machine Status ────────────────────────────────────────────────────

        /// <summary>GET /status — current machine operational state.</summary>
        public async Task<EosMachineStatusDto?> GetMachineStatusAsync(CancellationToken ct = default)
        {
            await EnsureTokenAsync(ct);

            // TODO: var response = await _http.GetAsync($"{_baseUrl}/status", ct);
            //       response.EnsureSuccessStatusCode();
            //       return await response.Content.ReadFromJsonAsync<EosMachineStatusDto>(_json, ct);

            _logger.LogDebug("[EOS REST] GetMachineStatus stubbed");
            return null; // Stub returns null; EosMachineProvider falls back to mock data.
        }

        /// <summary>GET /status/sensors — live sensor readings.</summary>
        public async Task<EosSensorDataDto?> GetSensorDataAsync(CancellationToken ct = default)
        {
            await EnsureTokenAsync(ct);

            // TODO: var response = await _http.GetAsync($"{_baseUrl}/status/sensors", ct);
            //       return await response.Content.ReadFromJsonAsync<EosSensorDataDto>(_json, ct);

            _logger.LogDebug("[EOS REST] GetSensorData stubbed");
            return null;
        }

        // ── Jobs ──────────────────────────────────────────────────────────────

        /// <summary>GET /jobs — list all jobs on the machine.</summary>
        public async Task<IReadOnlyList<EosJobDto>> GetJobsAsync(CancellationToken ct = default)
        {
            await EnsureTokenAsync(ct);

            // TODO: var response = await _http.GetAsync($"{_baseUrl}/jobs", ct);
            //       var result = await response.Content.ReadFromJsonAsync<List<EosJobDto>>(_json, ct);
            //       return result ?? [];

            _logger.LogDebug("[EOS REST] GetJobs stubbed");
            return [];
        }

        /// <summary>GET /jobs/{jobId} — single job detail.</summary>
        public async Task<EosJobDto?> GetJobAsync(string jobId, CancellationToken ct = default)
        {
            await EnsureTokenAsync(ct);

            // TODO: var response = await _http.GetAsync($"{_baseUrl}/jobs/{jobId}", ct);
            //       if (response.StatusCode == HttpStatusCode.NotFound) return null;
            //       return await response.Content.ReadFromJsonAsync<EosJobDto>(_json, ct);

            _logger.LogDebug("[EOS REST] GetJob {JobId} stubbed", jobId);
            return null;
        }

        /// <summary>GET /jobs/{jobId}/processparameters — parameters used in build.</summary>
        public async Task<EosProcessParametersDto?> GetJobProcessParametersAsync(
            string jobId, CancellationToken ct = default)
        {
            await EnsureTokenAsync(ct);

            // TODO: var response = await _http.GetAsync($"{_baseUrl}/jobs/{jobId}/processparameters", ct);
            //       if (!response.IsSuccessStatusCode) return null;
            //       return await response.Content.ReadFromJsonAsync<EosProcessParametersDto>(_json, ct);

            _logger.LogDebug("[EOS REST] GetJobProcessParameters {JobId} stubbed", jobId);
            return null;
        }

        public ValueTask DisposeAsync()
        {
            _http.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    // ── EOS API DTOs ──────────────────────────────────────────────────────────
    // Shape matches the EOSCONNECT Web API JSON schema.
    // Swagger: https://{printer-ip}/gui/webapi/swagger

    internal sealed class TokenResponse
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("expires_in")]   public int ExpiresIn { get; set; } = 3600;
        [JsonPropertyName("token_type")]   public string TokenType { get; set; } = "Bearer";
    }

    public sealed class EosMachineStatusDto
    {
        public string? MachineState { get; set; }       // "Idle", "Building", "Error", etc.
        public string? ActiveJobId { get; set; }
        public double? BuildProgressPercent { get; set; }
        public DateTime? EstimatedJobEnd { get; set; }
        public string? SystemMessage { get; set; }
    }

    public sealed class EosSensorDataDto
    {
        public double? BuildChamberTempC { get; set; }
        public double? OxygenContentPpm { get; set; }
        public double? ArgonFlowRateLpm { get; set; }
        public double? LaserPowerW { get; set; }
        public double? PowderLevelPercent { get; set; }
        public int? CurrentLayer { get; set; }
        public int? TotalLayers { get; set; }
    }

    public sealed class EosJobDto
    {
        public string? JobId { get; set; }
        public string? JobName { get; set; }
        public string? Status { get; set; }
        public string? Material { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public double? EstimatedDurationHours { get; set; }
        public bool? Success { get; set; }
        public string? FailureReason { get; set; }
    }

    public sealed class EosProcessParametersDto
    {
        public string? JobId { get; set; }
        public double? LaserPowerW { get; set; }
        public double? ScanSpeedMmPerSec { get; set; }
        public double? LayerThicknessMicrons { get; set; }
        public string? ParameterSetName { get; set; }
        public string? Material { get; set; }
    }
}
