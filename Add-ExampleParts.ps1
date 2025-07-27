# OpCentrix Parts Database Management Script
# Adds example parts to test the parts page functionality

Write-Host "?? OpCentrix Parts Database Management" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

# Check if we're in the correct directory
if (-not (Test-Path "OpCentrix.csproj")) {
    Write-Host "? Error: Please run this script from the OpCentrix project directory" -ForegroundColor Red
    Write-Host "Current directory: $(Get-Location)" -ForegroundColor Yellow
    exit 1
}

# Function to run dotnet commands with error handling
function Invoke-DotnetCommand {
    param([string]$Command, [string]$Description)
    
    Write-Host "?? $Description..." -ForegroundColor Yellow
    
    try {
        $result = Invoke-Expression $Command
        if ($LASTEXITCODE -eq 0) {
            Write-Host "? $Description completed successfully" -ForegroundColor Green
            return $true
        } else {
            Write-Host "? $Description failed with exit code: $LASTEXITCODE" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "? $Description failed: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

# Main menu
Write-Host ""
Write-Host "Choose an option:" -ForegroundColor White
Write-Host "1. Add example parts for testing" -ForegroundColor Green
Write-Host "2. Remove example parts" -ForegroundColor Red
Write-Host "3. Check example parts count" -ForegroundColor Cyan
Write-Host "4. Build and run application" -ForegroundColor Blue
Write-Host "5. Exit" -ForegroundColor Gray
Write-Host ""

$choice = Read-Host "Enter your choice (1-5)"

switch ($choice) {
    "1" {
        Write-Host ""
        Write-Host "?? Adding Example Parts for Testing" -ForegroundColor Green
        Write-Host "====================================" -ForegroundColor Green
        
        # Build the project first
        if (-not (Invoke-DotnetCommand "dotnet build" "Building project")) {
            exit 1
        }
        
        # Create a temporary C# script to add example parts
        $tempScript = @"
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Services;

// Create host and services
var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddDbContext<SchedulerContext>(options =>
            options.UseSqlite("Data Source=scheduler.db"));
        services.AddScoped<SlsDataSeedingService>();
        services.AddLogging();
    })
    .Build();

try
{
    using var scope = host.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
    var seedingService = scope.ServiceProvider.GetRequiredService<SlsDataSeedingService>();
    
    Console.WriteLine("?? Checking database connection...");
    await context.Database.EnsureCreatedAsync();
    
    Console.WriteLine("?? Adding example parts...");
    await seedingService.AddExamplePartsForSchedulerTestingAsync();
    
    var count = await seedingService.GetExamplePartsCountAsync();
    Console.WriteLine(`$"? Successfully added example parts. Total example parts: {count}");
}
catch (Exception ex)
{
    Console.WriteLine(`$"? Error: {ex.Message}");
    Environment.Exit(1);
}
"@
        
        # Write the script to a temporary file
        $scriptPath = "AddExampleParts.cs"
        Set-Content -Path $scriptPath -Value $tempScript
        
        try {
            # Run the script
            $result = Invoke-Expression "dotnet script $scriptPath"
            
            if ($LASTEXITCODE -eq 0) {
                Write-Host "? Example parts added successfully!" -ForegroundColor Green
                Write-Host ""
                Write-Host "?? Example parts include:" -ForegroundColor Cyan
                Write-Host "   • EX-1001: Small Bracket Component (2.5h)" -ForegroundColor White
                Write-Host "   • EX-1002: Mini Housing (3.75h)" -ForegroundColor White
                Write-Host "   • EX-2001: Aerospace Test Fitting (8.25h)" -ForegroundColor White
                Write-Host "   • EX-2002: Medical Test Device (10.5h)" -ForegroundColor White
                Write-Host "   • EX-3001: Complex Inconel Manifold (18.75h)" -ForegroundColor White
                Write-Host "   • EX-3002: Large Titanium Assembly (22.25h)" -ForegroundColor White
                Write-Host "   • EX-4001: Multi-Day Test Build (32.5h)" -ForegroundColor White
                Write-Host "   • EX-5001: Override Test Part (with admin override)" -ForegroundColor White
                Write-Host "   • EX-6001: Rush Job Test Part (6.75h)" -ForegroundColor White
                Write-Host ""
                Write-Host "?? You can now test your parts page at: http://localhost:5090/Admin/Parts" -ForegroundColor Yellow
            } else {
                Write-Host "? Failed to add example parts" -ForegroundColor Red
            }
        } catch {
            Write-Host "? Error running example parts script: $($_.Exception.Message)" -ForegroundColor Red
        } finally {
            # Clean up temporary file
            if (Test-Path $scriptPath) {
                Remove-Item $scriptPath -Force
            }
        }
    }
    
    "2" {
        Write-Host ""
        Write-Host "??? Removing Example Parts" -ForegroundColor Red
        Write-Host "=========================" -ForegroundColor Red
        
        $confirm = Read-Host "Are you sure you want to remove all example parts? (y/N)"
        if ($confirm -eq "y" -or $confirm -eq "Y") {
            # Create script to remove example parts
            $tempScript = @"
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Services;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddDbContext<SchedulerContext>(options =>
            options.UseSqlite("Data Source=scheduler.db"));
        services.AddScoped<SlsDataSeedingService>();
        services.AddLogging();
    })
    .Build();

try
{
    using var scope = host.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
    var seedingService = scope.ServiceProvider.GetRequiredService<SlsDataSeedingService>();
    
    Console.WriteLine("?? Removing example parts...");
    await seedingService.RemoveExamplePartsAsync();
    Console.WriteLine("? Example parts removed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine(`$"? Error: {ex.Message}");
    Environment.Exit(1);
}
"@
            
            $scriptPath = "RemoveExampleParts.cs"
            Set-Content -Path $scriptPath -Value $tempScript
            
            try {
                $result = Invoke-Expression "dotnet script $scriptPath"
                if ($LASTEXITCODE -eq 0) {
                    Write-Host "? Example parts removed successfully!" -ForegroundColor Green
                }
            } finally {
                if (Test-Path $scriptPath) {
                    Remove-Item $scriptPath -Force
                }
            }
        } else {
            Write-Host "? Operation cancelled" -ForegroundColor Yellow
        }
    }
    
    "3" {
        Write-Host ""
        Write-Host "?? Checking Example Parts Count" -ForegroundColor Cyan
        Write-Host "===============================" -ForegroundColor Cyan
        
        # Create script to check count
        $tempScript = @"
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpCentrix.Data;
using OpCentrix.Services;

var host = Host.CreateDefaultBuilder()
    .ConfigureServices(services =>
    {
        services.AddDbContext<SchedulerContext>(options =>
            options.UseSqlite("Data Source=scheduler.db"));
        services.AddScoped<SlsDataSeedingService>();
        services.AddLogging();
    })
    .Build();

try
{
    using var scope = host.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
    var seedingService = scope.ServiceProvider.GetRequiredService<SlsDataSeedingService>();
    
    var exampleCount = await seedingService.GetExamplePartsCountAsync();
    var totalCount = await context.Parts.CountAsync();
    
    Console.WriteLine(`$"?? Example parts: {exampleCount}");
    Console.WriteLine(`$"?? Total parts: {totalCount}");
}
catch (Exception ex)
{
    Console.WriteLine(`$"? Error: {ex.Message}");
    Environment.Exit(1);
}
"@
        
        $scriptPath = "CheckPartsCount.cs"
        Set-Content -Path $scriptPath -Value $tempScript
        
        try {
            $result = Invoke-Expression "dotnet script $scriptPath"
        } finally {
            if (Test-Path $scriptPath) {
                Remove-Item $scriptPath -Force
            }
        }
    }
    
    "4" {
        Write-Host ""
        Write-Host "?? Building and Running Application" -ForegroundColor Blue
        Write-Host "===================================" -ForegroundColor Blue
        
        # Build first
        if (Invoke-DotnetCommand "dotnet build" "Building project") {
            Write-Host ""
            Write-Host "?? Starting application..." -ForegroundColor Green
            Write-Host "?? Access your parts page at: http://localhost:5090/Admin/Parts" -ForegroundColor Cyan
            Write-Host "?? Login with: admin / admin123" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "Press Ctrl+C to stop the application" -ForegroundColor Gray
            Write-Host ""
            
            # Run the application
            Invoke-Expression "dotnet run"
        }
    }
    
    "5" {
        Write-Host "?? Goodbye!" -ForegroundColor Green
        exit 0
    }
    
    default {
        Write-Host "? Invalid choice. Please run the script again." -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "? Script completed!" -ForegroundColor Green