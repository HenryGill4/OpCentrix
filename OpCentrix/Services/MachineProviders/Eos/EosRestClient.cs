using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace OpCentrix.Services.MachineProviders.Eos;

/// <summary>
/// HTTP client for EOS REST API with OAuth2 authentication.
/// </summary>
public class EosRestClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EosRestClient> _logger;
    private readonly EosConnectionConfig _config;
    private readonly SemaphoreSlim _tokenLock = new(1, 1);
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public EosRestClient(EosConnectionConfig config, ILogger<EosRestClient> logger, HttpClient? httpClient = null)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? new HttpClient();

        if (!string.IsNullOrWhiteSpace(_config.RestApiBaseUrl))
            _httpClient.BaseAddress = new Uri(_config.RestApiBaseUrl.TrimEnd('/') + "/");

        _httpClient.Timeout = TimeSpan.FromSeconds(_config.OperationTimeoutSeconds);
    }

    public async Task<EosMachineStatusResponse?> GetMachineStatusAsync(CancellationToken ct = default)
        => await ExecuteAsync<EosMachineStatusResponse>("machine/status", ct);

    public async Task<EosBuildStatusResponse?> GetBuildStatusAsync(CancellationToken ct = default)
        => await ExecuteAsync<EosBuildStatusResponse>("build/current", ct);

    public async Task<List<EosAlarm>> GetActiveAlarmsAsync(CancellationToken ct = default)
        => (await ExecuteAsync<EosAlarmsResponse>("alarms/active", ct))?.Alarms ?? new();

    private async Task<T?> ExecuteAsync<T>(string endpoint, CancellationToken ct) where T : class
    {
        await EnsureAuthenticatedAsync(ct);
        using var request = CreateRequest(HttpMethod.Get, endpoint);
        using var response = await _httpClient.SendAsync(request, ct);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound) return null;
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
    }

    private async Task EnsureAuthenticatedAsync(CancellationToken ct)
    {
        if (DateTime.UtcNow < _tokenExpiry.AddMinutes(-1) && !string.IsNullOrEmpty(_accessToken)) return;
        await _tokenLock.WaitAsync(ct);
        try
        {
            if (DateTime.UtcNow < _tokenExpiry.AddMinutes(-1) && !string.IsNullOrEmpty(_accessToken)) return;
            if (string.IsNullOrWhiteSpace(_config.OAuth2TokenEndpoint)) return;

            var tokenRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _config.OAuth2ClientId,
                ["client_secret"] = _config.OAuth2ClientSecret,
                ["scope"] = _config.OAuth2Scopes
            };
            using var resp = await _httpClient.PostAsync(_config.OAuth2TokenEndpoint, new FormUrlEncodedContent(tokenRequest), ct);
            if (!resp.IsSuccessStatusCode) throw new EosApiException($"OAuth2 failed: {resp.StatusCode}");
            var data = await resp.Content.ReadFromJsonAsync<OAuth2TokenResponse>(cancellationToken: ct);
            _accessToken = data?.AccessToken ?? throw new EosApiException("No token");
            _tokenExpiry = DateTime.UtcNow.AddSeconds((data?.ExpiresIn ?? 3600) - 60);
        }
        finally { _tokenLock.Release(); }
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
    {
        var request = new HttpRequestMessage(method, endpoint);
        if (!string.IsNullOrEmpty(_accessToken))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return request;
    }

    public async Task<bool> TestConnectionAsync(CancellationToken ct = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_config.RestApiBaseUrl)) return false;
            return await GetMachineStatusAsync(ct) != null;
        }
        catch { return false; }
    }

    public void Dispose()
    {
        _tokenLock.Dispose();
        _httpClient.Dispose();
    }
}

public class OAuth2TokenResponse
{
    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = "";
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; } = 3600;
}

public class EosMachineStatusResponse
{
    [JsonPropertyName("state")] public string State { get; set; } = "Unknown";
    [JsonPropertyName("isRunning")] public bool IsRunning { get; set; }
    [JsonPropertyName("hasError")] public bool HasError { get; set; }
}

public class EosBuildStatusResponse
{
    [JsonPropertyName("jobName")] public string JobName { get; set; } = "";
    [JsonPropertyName("progress")] public double Progress { get; set; }
    [JsonPropertyName("currentLayer")] public int CurrentLayer { get; set; }
    [JsonPropertyName("totalLayers")] public int TotalLayers { get; set; }
    [JsonPropertyName("estimatedRemainingSeconds")] public int EstimatedRemainingSeconds { get; set; }
}

public class EosAlarmsResponse
{
    [JsonPropertyName("alarms")] public List<EosAlarm> Alarms { get; set; } = new();
}

public class EosAlarm
{
    [JsonPropertyName("id")] public string Id { get; set; } = "";
    [JsonPropertyName("message")] public string Message { get; set; } = "";
    [JsonPropertyName("severity")] public string Severity { get; set; } = "Warning";
}

public class EosApiException : Exception
{
    public EosApiException(string message) : base(message) { }
}
