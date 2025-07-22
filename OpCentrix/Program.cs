//Program.cs:
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpCentrix.Data;
using OpCentrix.Models;
using OpCentrix.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// EF Core SQLite setup
builder.Services.AddDbContext<SchedulerContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=scheduler.db"));

// Register custom services
builder.Services.AddScoped<ISchedulerService, SchedulerService>();

var app = builder.Build();

// Seed sample data for TI and INC jobs/parts with basic data first
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
        
        // Ensure database is created
        db.Database.EnsureCreated();

        // Check if Parts exist before adding
        if (!db.Parts.Any())
        {
            var parts = new[]
            {
                new Part { 
                    PartNumber = "TI-1001", 
                    Description = "Titanium Widget A", 
                    Material = "Titanium Ti-6Al-4V", 
                    AvgDuration = "8h 0m", 
                    AvgDurationDays = 1,
                    EstimatedHours = 8.0,
                    MaterialCostPerUnit = 45.50m,
                    StandardLaborCostPerHour = 75.00m,
                    SetupCost = 150.00m,
                    IsActive = true,
                    CreatedBy = "System"
                },
                new Part { 
                    PartNumber = "TI-1002", 
                    Description = "Titanium Widget B", 
                    Material = "Titanium Ti-6Al-4V", 
                    AvgDuration = "16h 0m", 
                    AvgDurationDays = 2,
                    EstimatedHours = 16.0,
                    MaterialCostPerUnit = 89.75m,
                    StandardLaborCostPerHour = 75.00m,
                    SetupCost = 150.00m,
                    IsActive = true,
                    CreatedBy = "System"
                },
                new Part { 
                    PartNumber = "INC-2001", 
                    Description = "Inconel Gizmo A", 
                    Material = "Inconel 718", 
                    AvgDuration = "6h 0m", 
                    AvgDurationDays = 1,
                    EstimatedHours = 6.0,
                    MaterialCostPerUnit = 125.00m,
                    StandardLaborCostPerHour = 85.00m,
                    SetupCost = 200.00m,
                    IsActive = true,
                    CreatedBy = "System"
                },
                new Part { 
                    PartNumber = "INC-2002", 
                    Description = "Inconel Gizmo B", 
                    Material = "Inconel 718", 
                    AvgDuration = "24h 0m", 
                    AvgDurationDays = 3,
                    EstimatedHours = 24.0,
                    MaterialCostPerUnit = 275.50m,
                    StandardLaborCostPerHour = 85.00m,
                    SetupCost = 200.00m,
                    IsActive = true,
                    CreatedBy = "System"
                }
            };
            
            db.Parts.AddRange(parts);
            db.SaveChanges();
            
            Console.WriteLine("Sample parts added to database.");
        }

        // Check if Jobs exist before adding
        if (!db.Jobs.Any() && db.Parts.Any())
        {
            try
            {
                var ti1 = db.Parts.FirstOrDefault(p => p.PartNumber == "TI-1001");
                var ti2 = db.Parts.FirstOrDefault(p => p.PartNumber == "TI-1002");
                var inc1 = db.Parts.FirstOrDefault(p => p.PartNumber == "INC-2001");
                var inc2 = db.Parts.FirstOrDefault(p => p.PartNumber == "INC-2002");

                if (ti1 != null && ti2 != null && inc1 != null && inc2 != null)
                {
                    var jobs = new[]
                    {
                        new Job { 
                            MachineId = "TI1", 
                            PartId = ti1.Id, 
                            PartNumber = ti1.PartNumber, 
                            ScheduledStart = DateTime.Today.AddHours(8), 
                            ScheduledEnd = DateTime.Today.AddHours(16), 
                            Status = "Scheduled", 
                            Quantity = 10,
                            EstimatedHours = 8.0,
                            LaborCostPerHour = 75.00m,
                            MaterialCostPerUnit = 45.50m,
                            OverheadCostPerHour = 25.00m,
                            Priority = 2,
                            Operator = "John Smith",
                            CreatedBy = "System"
                        },
                        new Job { 
                            MachineId = "TI2", 
                            PartId = ti2.Id, 
                            PartNumber = ti2.PartNumber, 
                            ScheduledStart = DateTime.Today.AddDays(1).AddHours(8), 
                            ScheduledEnd = DateTime.Today.AddDays(2).AddHours(16), 
                            Status = "Scheduled", 
                            Quantity = 5,
                            EstimatedHours = 16.0,
                            LaborCostPerHour = 75.00m,
                            MaterialCostPerUnit = 89.75m,
                            OverheadCostPerHour = 25.00m,
                            Priority = 1,
                            Operator = "Sarah Johnson",
                            CreatedBy = "System"
                        },
                        new Job { 
                            MachineId = "INC", 
                            PartId = inc1.Id, 
                            PartNumber = inc1.PartNumber, 
                            ScheduledStart = DateTime.Today.AddHours(9), 
                            ScheduledEnd = DateTime.Today.AddHours(15), 
                            Status = "Scheduled", 
                            Quantity = 8,
                            EstimatedHours = 6.0,
                            LaborCostPerHour = 85.00m,
                            MaterialCostPerUnit = 125.00m,
                            OverheadCostPerHour = 30.00m,
                            Priority = 3,
                            Operator = "Mike Wilson",
                            CreatedBy = "System"
                        },
                        new Job { 
                            MachineId = "INC", 
                            PartId = inc2.Id, 
                            PartNumber = inc2.PartNumber, 
                            ScheduledStart = DateTime.Today.AddDays(2).AddHours(8), 
                            ScheduledEnd = DateTime.Today.AddDays(5).AddHours(8), 
                            Status = "Scheduled", 
                            Quantity = 3,
                            EstimatedHours = 24.0,
                            LaborCostPerHour = 85.00m,
                            MaterialCostPerUnit = 275.50m,
                            OverheadCostPerHour = 30.00m,
                            Priority = 2,
                            Operator = "Lisa Davis",
                            CreatedBy = "System"
                        }
                    };
                    
                    db.Jobs.AddRange(jobs);
                    db.SaveChanges();
                    
                    Console.WriteLine("Sample jobs added to database.");
                }
                else
                {
                    Console.WriteLine("Could not find all required parts for job seeding.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding jobs: {ex.Message}");
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error during database initialization: {ex.Message}");
    // Don't crash the application, just log the error
}

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();

// Make Program accessible for testing
public partial class Program { }
