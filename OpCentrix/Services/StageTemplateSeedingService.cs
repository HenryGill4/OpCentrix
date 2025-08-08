using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Models;

namespace OpCentrix.Services
{
    /// <summary>
    /// Seeding service for Stage Templates
    /// Phase 6: Advanced Stage Management - Default Templates
    /// </summary>
    public class StageTemplateSeedingService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<StageTemplateSeedingService> _logger;
        
        public StageTemplateSeedingService(SchedulerContext context, ILogger<StageTemplateSeedingService> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task SeedDefaultTemplatesAsync()
        {
            try
            {
                _logger.LogInformation("?? Seeding default stage templates...");
                
                // Check if production stages exist first
                var productionStagesCount = await _context.ProductionStages.CountAsync();
                if (productionStagesCount == 0)
                {
                    _logger.LogWarning("?? No production stages found. Stage templates require production stages to exist first.");
                    _logger.LogInformation("?? Create production stages via /Admin/ProductionStages first, then run template seeding again");
                    return;
                }
                
                // Create default categories
                await SeedCategoriesAsync();
                
                // Create default templates
                await SeedTemplatesAsync();
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("? Stage template seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error seeding stage templates");
                // Don't re-throw - log and continue
                _logger.LogWarning("?? Stage template seeding failed, continuing without templates");
            }
        }
        
        private async Task SeedCategoriesAsync()
        {
            var categories = new List<StageTemplateCategory>
            {
                new StageTemplateCategory
                {
                    Name = "B&T Manufacturing",
                    Description = "Templates for B&T firearms and suppressor manufacturing",
                    Icon = "fas fa-shield-alt",
                    ColorCode = "#ff6b35",
                    SortOrder = 1
                },
                new StageTemplateCategory
                {
                    Name = "General Manufacturing",
                    Description = "Standard manufacturing templates for general parts",
                    Icon = "fas fa-cogs",
                    ColorCode = "#007bff",
                    SortOrder = 2
                },
                new StageTemplateCategory
                {
                    Name = "Prototyping",
                    Description = "Templates for prototype and development parts",
                    Icon = "fas fa-flask",
                    ColorCode = "#6f42c1",
                    SortOrder = 3
                },
                new StageTemplateCategory
                {
                    Name = "High Volume",
                    Description = "Templates optimized for high volume production",
                    Icon = "fas fa-industry",
                    ColorCode = "#28a745",
                    SortOrder = 4
                }
            };
            
            foreach (var category in categories)
            {
                if (!await _context.StageTemplateCategories.AnyAsync(c => c.Name == category.Name))
                {
                    _context.StageTemplateCategories.Add(category);
                    _logger.LogInformation("? Added template category: {CategoryName}", category.Name);
                }
            }
        }
        
        private async Task SeedTemplatesAsync()
        {
            var templates = new List<(StageTemplate template, List<(string stageName, int order, double hours, decimal rate, decimal materialCost, int setupMin, int teardownMin)> steps)>
            {
                // B&T Suppressor Template
                (new StageTemplate
                {
                    Name = "B&T Suppressor Manufacturing",
                    Description = "Complete workflow for B&T suppressor components including ATF compliance",
                    Industry = "Firearms",
                    MaterialType = "Titanium",
                    ComplexityLevel = "Complex",
                    EstimatedTotalHours = 16.5m,
                    EstimatedTotalCost = 1850.00m,
                    IsDefault = true,
                    SortOrder = 1,
                    CreatedBy = "System"
                }, new List<(string, int, double, decimal, decimal, int, int)>
                {
                    ("SLS Printing", 1, 8.0, 85.00m, 150.00m, 60, 30),
                    ("EDM Operations", 2, 4.0, 95.00m, 50.00m, 45, 15),
                    ("CNC Machining", 3, 3.0, 90.00m, 25.00m, 30, 15),
                    ("Finishing", 4, 1.5, 75.00m, 75.00m, 30, 30)
                }),
                
                // B&T Firearm Component Template
                (new StageTemplate
                {
                    Name = "B&T Firearm Component",
                    Description = "Standard workflow for B&T firearm components with serialization",
                    Industry = "Firearms",
                    MaterialType = "Steel",
                    ComplexityLevel = "Complex",
                    EstimatedTotalHours = 14.0m,
                    EstimatedTotalCost = 1650.00m,
                    IsDefault = false,
                    SortOrder = 2,
                    CreatedBy = "System"
                }, new List<(string, int, double, decimal, decimal, int, int)>
                {
                    ("SLS Printing", 1, 6.0, 85.00m, 120.00m, 60, 30),
                    ("CNC Machining", 2, 5.0, 90.00m, 30.00m, 45, 20),
                    ("EDM Operations", 3, 2.0, 95.00m, 25.00m, 30, 15),
                    ("Assembly", 4, 1.0, 80.00m, 10.00m, 15, 10)
                }),
                
                // General Titanium Parts Template
                (new StageTemplate
                {
                    Name = "Titanium Parts - Standard",
                    Description = "Standard workflow for titanium parts manufacturing",
                    Industry = "General",
                    MaterialType = "Titanium",
                    ComplexityLevel = "Medium",
                    EstimatedTotalHours = 10.0m,
                    EstimatedTotalCost = 1200.00m,
                    IsDefault = true,
                    SortOrder = 3,
                    CreatedBy = "System"
                }, new List<(string, int, double, decimal, decimal, int, int)>
                {
                    ("SLS Printing", 1, 8.0, 85.00m, 100.00m, 45, 30),
                    ("CNC Machining", 2, 2.0, 90.00m, 20.00m, 30, 15)
                }),
                
                // Simple Prototype Template
                (new StageTemplate
                {
                    Name = "Simple Prototype",
                    Description = "Basic workflow for simple prototype parts",
                    Industry = "General",
                    MaterialType = "Metal",
                    ComplexityLevel = "Simple",
                    EstimatedTotalHours = 6.0m,
                    EstimatedTotalCost = 720.00m,
                    IsDefault = true,
                    SortOrder = 4,
                    CreatedBy = "System"
                }, new List<(string, int, double, decimal, decimal, int, int)>
                {
                    ("SLS Printing", 1, 6.0, 85.00m, 80.00m, 30, 30)
                }),
                
                // Complex Multi-Stage Template
                (new StageTemplate
                {
                    Name = "Complex Multi-Stage Manufacturing",
                    Description = "Comprehensive workflow for complex parts requiring all manufacturing stages",
                    Industry = "Aerospace",
                    MaterialType = "Inconel",
                    ComplexityLevel = "VeryComplex",
                    EstimatedTotalHours = 24.0m,
                    EstimatedTotalCost = 2800.00m,
                    IsDefault = false,
                    SortOrder = 5,
                    CreatedBy = "System"
                }, new List<(string, int, double, decimal, decimal, int, int)>
                {
                    ("SLS Printing", 1, 12.0, 85.00m, 200.00m, 90, 45),
                    ("EDM Operations", 2, 6.0, 95.00m, 75.00m, 60, 30),
                    ("CNC Machining", 3, 4.0, 90.00m, 50.00m, 45, 20),
                    ("Assembly", 4, 1.5, 80.00m, 15.00m, 30, 15),
                    ("Finishing", 5, 0.5, 75.00m, 25.00m, 15, 15)
                }),
                
                // High Volume Production Template
                (new StageTemplate
                {
                    Name = "High Volume Production",
                    Description = "Optimized workflow for high volume production runs",
                    Industry = "General",
                    MaterialType = "Steel",
                    ComplexityLevel = "Medium",
                    EstimatedTotalHours = 4.0m,
                    EstimatedTotalCost = 580.00m,
                    IsDefault = false,
                    SortOrder = 6,
                    CreatedBy = "System"
                }, new List<(string, int, double, decimal, decimal, int, int)>
                {
                    ("SLS Printing", 1, 3.0, 85.00m, 60.00m, 15, 15),
                    ("Finishing", 2, 1.0, 75.00m, 30.00m, 15, 15)
                })
            };
            
            foreach (var (template, steps) in templates)
            {
                if (!await _context.StageTemplates.AnyAsync(t => t.Name == template.Name))
                {
                    _context.StageTemplates.Add(template);
                    await _context.SaveChangesAsync(); // Save to get ID
                    
                    _logger.LogInformation("? Added template: {TemplateName}", template.Name);
                    
                    // Add template steps
                    foreach (var (stageName, order, hours, rate, materialCost, setupMin, teardownMin) in steps)
                    {
                        var stage = await _context.ProductionStages.FirstOrDefaultAsync(s => s.Name == stageName);
                        if (stage != null)
                        {
                            var step = new StageTemplateStep
                            {
                                StageTemplateId = template.Id,
                                ProductionStageId = stage.Id,
                                ExecutionOrder = order,
                                EstimatedHours = hours,
                                HourlyRate = rate,
                                MaterialCost = materialCost,
                                SetupTimeMinutes = setupMin,
                                TeardownTimeMinutes = teardownMin,
                                IsRequired = true,
                                IsParallel = false,
                                StageConfiguration = "{}",
                                QualityRequirements = "",
                                SpecialInstructions = ""
                            };
                            
                            _context.StageTemplateSteps.Add(step);
                        }
                        else
                        {
                            _logger.LogWarning("?? Production stage not found: {StageName}", stageName);
                        }
                    }
                }
            }
        }
        
        public async Task<int> GetTemplateCountAsync()
        {
            return await _context.StageTemplates.CountAsync(t => t.IsActive);
        }
        
        public async Task<List<string>> GetTemplateNamesAsync()
        {
            return await _context.StageTemplates
                .Where(t => t.IsActive)
                .OrderBy(t => t.SortOrder)
                .Select(t => t.Name)
                .ToListAsync();
        }
    }
}