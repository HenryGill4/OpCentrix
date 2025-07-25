using Microsoft.Extensions.FileProviders;
using System.Text.RegularExpressions;

namespace OpCentrix.Services.Admin;

/// <summary>
/// Service for viewing and managing application logs
/// Task 2.5: Global Logging and Error Handling - Log viewer
/// </summary>
public interface ILogViewerService
{
    Task<List<LogEntry>> GetLogsAsync(int page = 1, int pageSize = 50, string? level = null, string? search = null);
    Task<List<string>> GetLogLevelsAsync();
    Task<List<string>> GetLogFilesAsync();
    Task<string> GetLogFileContentAsync(string fileName);
    Task<bool> ClearLogsAsync();
    Task<LogStatistics> GetLogStatisticsAsync();
}

public class LogViewerService : ILogViewerService
{
    private readonly ILogger<LogViewerService> _logger;
    private readonly string _logDirectory;
    private readonly IFileProvider _fileProvider;

    public LogViewerService(ILogger<LogViewerService> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _logDirectory = Path.Combine(environment.ContentRootPath, "logs");
        _fileProvider = new PhysicalFileProvider(_logDirectory);
        
        // Ensure logs directory exists
        Directory.CreateDirectory(_logDirectory);
    }

    public async Task<List<LogEntry>> GetLogsAsync(int page = 1, int pageSize = 50, string? level = null, string? search = null)
    {
        try
        {
            var logEntries = new List<LogEntry>();
            var logFiles = await GetLogFilesAsync();
            
            // Process most recent files first
            foreach (var logFile in logFiles.OrderByDescending(f => f))
            {
                var filePath = Path.Combine(_logDirectory, logFile);
                if (!File.Exists(filePath))
                    continue;

                var lines = await File.ReadAllLinesAsync(filePath);
                
                foreach (var line in lines.Reverse())
                {
                    var logEntry = ParseLogLine(line);
                    if (logEntry == null)
                        continue;

                    // Apply filters
                    if (!string.IsNullOrEmpty(level) && !logEntry.Level.Equals(level, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (!string.IsNullOrEmpty(search) && !logEntry.Message.Contains(search, StringComparison.OrdinalIgnoreCase))
                        continue;

                    logEntries.Add(logEntry);
                }
            }

            // Apply pagination
            var skip = (page - 1) * pageSize;
            return logEntries.OrderByDescending(l => l.Timestamp)
                           .Skip(skip)
                           .Take(pageSize)
                           .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs");
            return new List<LogEntry>();
        }
    }

    public async Task<List<string>> GetLogLevelsAsync()
    {
        return await Task.FromResult(new List<string> 
        { 
            "Information", "Warning", "Error", "Debug", "Verbose", "Fatal" 
        });
    }

    public async Task<List<string>> GetLogFilesAsync()
    {
        try
        {
            if (!Directory.Exists(_logDirectory))
                return new List<string>();

            var files = Directory.GetFiles(_logDirectory, "opcentrix-*.log")
                                .Select(Path.GetFileName)
                                .Where(f => f != null)
                                .Cast<string>()
                                .OrderByDescending(f => f)
                                .ToList();

            return await Task.FromResult(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving log files");
            return new List<string>();
        }
    }

    public async Task<string> GetLogFileContentAsync(string fileName)
    {
        try
        {
            var filePath = Path.Combine(_logDirectory, fileName);
            if (!File.Exists(filePath))
                return string.Empty;

            return await File.ReadAllTextAsync(filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading log file {FileName}", fileName);
            return string.Empty;
        }
    }

    public async Task<bool> ClearLogsAsync()
    {
        try
        {
            var logFiles = await GetLogFilesAsync();
            
            foreach (var logFile in logFiles)
            {
                var filePath = Path.Combine(_logDirectory, logFile);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }

            _logger.LogInformation("Log files cleared");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing log files");
            return false;
        }
    }

    public async Task<LogStatistics> GetLogStatisticsAsync()
    {
        try
        {
            var stats = new LogStatistics();
            var logFiles = await GetLogFilesAsync();

            foreach (var logFile in logFiles)
            {
                var filePath = Path.Combine(_logDirectory, logFile);
                if (!File.Exists(filePath))
                    continue;

                var fileInfo = new FileInfo(filePath);
                stats.TotalFileSizeBytes += fileInfo.Length;

                var lines = await File.ReadAllLinesAsync(filePath);
                foreach (var line in lines)
                {
                    var logEntry = ParseLogLine(line);
                    if (logEntry == null)
                        continue;

                    stats.TotalEntries++;

                    switch (logEntry.Level.ToLower())
                    {
                        case "information":
                        case "info":
                            stats.InformationCount++;
                            break;
                        case "warning":
                        case "warn":
                            stats.WarningCount++;
                            break;
                        case "error":
                            stats.ErrorCount++;
                            break;
                        case "debug":
                            stats.DebugCount++;
                            break;
                        case "fatal":
                            stats.FatalCount++;
                            break;
                    }

                    if (logEntry.Timestamp.Date == DateTime.Today)
                        stats.TodayEntries++;
                }
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating log statistics");
            return new LogStatistics();
        }
    }

    private LogEntry? ParseLogLine(string line)
    {
        try
        {
            // Parse Serilog format: [timestamp level] message
            var pattern = @"\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} [+-]\d{2}:\d{2}) (\w+)\] (.+)";
            var match = Regex.Match(line, pattern);

            if (!match.Success)
                return null;

            return new LogEntry
            {
                Timestamp = DateTime.Parse(match.Groups[1].Value),
                Level = match.Groups[2].Value,
                Message = match.Groups[3].Value,
                RawLine = line
            };
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Represents a log entry from the application logs
/// </summary>
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string RawLine { get; set; } = string.Empty;
    public string Exception { get; set; } = string.Empty;

    public string LevelClass => Level.ToLower() switch
    {
        "information" or "info" => "text-blue-600",
        "warning" or "warn" => "text-yellow-600",
        "error" => "text-red-600",
        "debug" => "text-gray-600",
        "fatal" => "text-red-800 font-bold",
        _ => "text-gray-500"
    };

    public string LevelIcon => Level.ToLower() switch
    {
        "information" or "info" => "??",
        "warning" or "warn" => "??",
        "error" => "?",
        "debug" => "??",
        "fatal" => "??",
        _ => "??"
    };
}

/// <summary>
/// Statistics about the application logs
/// </summary>
public class LogStatistics
{
    public int TotalEntries { get; set; }
    public int TodayEntries { get; set; }
    public int InformationCount { get; set; }
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }
    public int DebugCount { get; set; }
    public int FatalCount { get; set; }
    public long TotalFileSizeBytes { get; set; }

    public string TotalFileSizeFormatted
    {
        get
        {
            var bytes = TotalFileSizeBytes;
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}