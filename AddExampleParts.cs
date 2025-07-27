using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Services;

// Create a simple console application to add example parts for scheduler testing
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<SchedulerContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<SlsDataSeedingService>();

var app = builder.Build();

// Create scope and add example parts
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<SchedulerContext>();
    var seedingService = services.GetRequiredService<SlsDataSeedingService>();
    
    Console.WriteLine("?? Adding example parts for scheduler UI testing...");
    
    try
    {
        // Ensure database exists
        await context.Database.EnsureCreatedAsync();
        
        // Check if example parts already exist
        var existingCount = await seedingService.GetExamplePartsCountAsync();
        
        if (existingCount > 0)
        {
            Console.WriteLine($"? Found {existingCount} existing example parts");
            Console.WriteLine("To view them, go to Admin ? Parts and search for 'EX-'");
        }
        else
        {
            // Add the example parts
            await seedingService.AddExamplePartsForSchedulerTestingAsync();
            
            var addedCount = await seedingService.GetExamplePartsCountAsync();
            Console.WriteLine($"? Successfully added {addedCount} example parts!");
            
            Console.WriteLine("\n? Example parts added:");
            Console.WriteLine("  • EX-1001, EX-1002: Quick parts (2-4h) - Good for filling gaps");
            Console.WriteLine("  • EX-2001, EX-2002: Medium parts (6-12h) - Standard jobs");
            Console.WriteLine("  • EX-3001, EX-3002: Long parts (18-24h) - Complex builds");
            Console.WriteLine("  • EX-4001: Very long part (32h+) - Multi-day production");
            Console.WriteLine("  • EX-5001: Part with admin override testing");
            Console.WriteLine("  • EX-6001: Rush job testing part");
            
            Console.WriteLine("\n? Ready to test the scheduler!");
            Console.WriteLine("  1. Go to Admin ? Parts to view all example parts");
            Console.WriteLine("  2. Go to Scheduler to create jobs using these parts");
            Console.WriteLine("  3. Test different scheduling scenarios with varied durations");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"? Error: {ex.Message}");
        Console.WriteLine("Please check the database connection and try again.");
    }
}

Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();