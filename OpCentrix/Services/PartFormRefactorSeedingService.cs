using OpCentrix.Data;
using OpCentrix.Models;
using Microsoft.EntityFrameworkCore;

namespace OpCentrix.Services
{
    /// <summary>
    /// Seeding service for Part Form Refactor lookup tables
    /// Seeds ComponentTypes, ComplianceCategories, and LegacyFlagToStageMap data
    /// </summary>
    public class PartFormRefactorSeedingService
    {
        private readonly SchedulerContext _context;
        private readonly ILogger<PartFormRefactorSeedingService> _logger;

        public PartFormRefactorSeedingService(
            SchedulerContext context,
            ILogger<PartFormRefactorSeedingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Seed all Part Form Refactor lookup data
        /// </summary>
        public async Task SeedAllDataAsync()
        {
            try
            {
                await SeedComponentTypesAsync();
                await SeedComplianceCategoriesAsync();
                //await SeedLegacyFlagToStageMappingAsync();
                
                _logger.LogInformation("? Part Form Refactor seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error during Part Form Refactor seeding");
                // Don't re-throw - log and continue
                _logger.LogWarning("?? Part Form Refactor seeding failed, continuing without lookup tables");
            }
        }

        /// <summary>
        /// Seed ComponentTypes lookup table
        /// </summary>
        public async Task SeedComponentTypesAsync()
        {
            try
            {
                // Check if already seeded
                if (await _context.ComponentTypes.AnyAsync())
                {
                    _logger.LogInformation("ComponentTypes already seeded, skipping...");
                    return;
                }

                var componentTypes = new List<ComponentType>
                {
                    new ComponentType
                    {
                        Name = "General",
                        Description = "General components that do not require serialization or special tracking",
                        IsActive = true,
                        SortOrder = 1,
                        CreatedBy = "System",
                        LastModifiedBy = "System"
                    },
                    new ComponentType
                    {
                        Name = "Serialized",
                        Description = "Components that require unique serial numbers and individual tracking",
                        IsActive = true,
                        SortOrder = 2,
                        CreatedBy = "System",
                        LastModifiedBy = "System"
                    }
                };

                _context.ComponentTypes.AddRange(componentTypes);
                await _context.SaveChangesAsync();

                _logger.LogInformation("? ComponentTypes seeded: {Count} types added", componentTypes.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error seeding ComponentTypes");
                // Don't re-throw
                _logger.LogWarning("?? ComponentTypes seeding failed, continuing without component types");
            }
        }

        /// <summary>
        /// Seed ComplianceCategories lookup table
        /// </summary>
        public async Task SeedComplianceCategoriesAsync()
        {
            try
            {
                // Check if already seeded
                if (await _context.ComplianceCategories.AnyAsync())
                {
                    _logger.LogInformation("ComplianceCategories already seeded, skipping...");
                    return;
                }

                var complianceCategories = new List<ComplianceCategory>
                {
                    new ComplianceCategory
                    {
                        Name = "Non NFA",
                        Description = "Standard manufacturing components with normal regulatory requirements",
                        RegulatoryLevel = "Standard",
                        RequiresSpecialHandling = false,
                        IsActive = true,
                        SortOrder = 1,
                        CreatedBy = "System",
                        LastModifiedBy = "System"
                    },
                    new ComplianceCategory
                    {
                        Name = "NFA",
                        Description = "National Firearms Act regulated components requiring special compliance",
                        RegulatoryLevel = "Restricted",
                        RequiresSpecialHandling = true,
                        IsActive = true,
                        SortOrder = 2,
                        CreatedBy = "System",
                        LastModifiedBy = "System"
                    }
                };

                _context.ComplianceCategories.AddRange(complianceCategories);
                await _context.SaveChangesAsync();

                _logger.LogInformation("? ComplianceCategories seeded: {Count} categories added", complianceCategories.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error seeding ComplianceCategories");
                // Don't re-throw
                _logger.LogWarning("?? ComplianceCategories seeding failed, continuing without compliance categories");
            }
        }

        /// <summary>
        /// Seed LegacyFlagToStageMap table for automated migration
        /// </summary>
        //public async Task SeedLegacyFlagToStageMappingAsync()
        //{
        //    try
        //    {
        //        // Check if already seeded
        //        if (await _context.LegacyFlagToStageMaps.AnyAsync())
        //        {
        //            _logger.LogInformation("LegacyFlagToStageMap already seeded, skipping...");
        //            return;
        //        }

        //        var mappings = new List<LegacyFlagToStageMap>
        //        {
        //            new LegacyFlagToStageMap
        //            {
        //                LegacyFieldName = "RequiresSLSPrinting",
        //                ProductionStageName = "SLS Printing",
        //                ExecutionOrder = 1,
        //                DefaultSetupMinutes = 45,
        //                DefaultTeardownMinutes = 30,
        //                IsActive = true
        //            },
        //            new LegacyFlagToStageMap
        //            {
        //                LegacyFieldName = "RequiresEDMOperations",
        //                ProductionStageName = "EDM Operations",
        //                ExecutionOrder = 2,
        //                DefaultSetupMinutes = 30,
        //                DefaultTeardownMinutes = 15,
        //                IsActive = true
        //            },
        //            new LegacyFlagToStageMap
        //            {
        //                LegacyFieldName = "RequiresCNCMachining",
        //                ProductionStageName = "CNC Machining",
        //                ExecutionOrder = 3,
        //                DefaultSetupMinutes = 60,
        //                DefaultTeardownMinutes = 20,
        //                IsActive = true
        //            },
        //            new LegacyFlagToStageMap
        //            {
        //                LegacyFieldName = "RequiresAssembly",
        //                ProductionStageName = "Assembly",
        //                ExecutionOrder = 4,
        //                DefaultSetupMinutes = 15,
        //                DefaultTeardownMinutes = 10,
        //                IsActive = true
        //            },
        //            new LegacyFlagToStageMap
        //            {
        //                LegacyFieldName = "RequiresFinishing",
        //                ProductionStageName = "Finishing",
        //                ExecutionOrder = 5,
        //                DefaultSetupMinutes = 30,
        //                DefaultTeardownMinutes = 15,
        //                IsActive = true
        //            }
        //        };

        //        _context.LegacyFlagToStageMaps.AddRange(mappings);
        //        await _context.SaveChangesAsync();

        //        _logger.LogInformation("? LegacyFlagToStageMap seeded: {Count} mappings added", mappings.Count);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "? Error seeding LegacyFlagToStageMap");
        //        throw;
        //    }
        //}

        /// <summary>
        /// Migrate existing Parts data to use new lookup tables
        /// This should be run after seeding lookup tables
        /// </summary>
        public async Task MigrateExistingPartsDataAsync()
        {
            try
            {
                _logger.LogInformation("?? Starting migration of existing Parts data...");

                // Get lookup table IDs
                var generalComponentType = await _context.ComponentTypes
                    .FirstOrDefaultAsync(ct => ct.Name == "General");
                var serializedComponentType = await _context.ComponentTypes
                    .FirstOrDefaultAsync(ct => ct.Name == "Serialized");
                var nonNfaCompliance = await _context.ComplianceCategories
                    .FirstOrDefaultAsync(cc => cc.Name == "Non NFA");
                var nfaCompliance = await _context.ComplianceCategories
                    .FirstOrDefaultAsync(cc => cc.Name == "NFA");

                if (generalComponentType == null || serializedComponentType == null ||
                    nonNfaCompliance == null || nfaCompliance == null)
                {
                    _logger.LogWarning("?? Lookup tables not properly seeded, skipping data migration");
                    return;
                }

                // Update parts with ComponentTypeId
                var partsToUpdate = await _context.Parts
                    .Where(p => p.ComponentTypeId == null)
                    .ToListAsync();

                foreach (var part in partsToUpdate)
                {
                    // Map BTComponentType string to ComponentTypeId
                    if (string.IsNullOrEmpty(part.BTComponentType) || 
                        string.Equals(part.BTComponentType, "General", StringComparison.OrdinalIgnoreCase))
                    {
                        part.ComponentTypeId = generalComponentType.Id;
                    }
                    else if (string.Equals(part.BTComponentType, "Serialized", StringComparison.OrdinalIgnoreCase))
                    {
                        part.ComponentTypeId = serializedComponentType.Id;
                    }
                    else
                    {
                        part.ComponentTypeId = generalComponentType.Id; // Default to General
                    }

                    // Map compliance data based on regulatory fields
                    if (part.RequiresATFCompliance || part.RequiresITARCompliance || 
                        part.RequiresSerialization || part.IsControlledItem)
                    {
                        part.ComplianceCategoryId = nfaCompliance.Id;
                    }
                    else
                    {
                        part.ComplianceCategoryId = nonNfaCompliance.Id;
                    }

                    // Mark as migrated
                    part.IsLegacyForm = false;
                    part.LastModifiedBy = "System Migration";
                    part.LastModifiedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("? Parts data migration completed: {Count} parts updated", partsToUpdate.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error during Parts data migration");
                // Don't re-throw
                _logger.LogWarning("?? Parts data migration failed, existing parts may need manual update");
            }
        }

        /// <summary>
        /// Get migration status summary
        /// </summary>
        public async Task<Dictionary<string, object>> GetMigrationStatusAsync()
        {
            try
            {
                var status = new Dictionary<string, object>();

                // Count lookup table data
                status["ComponentTypesCount"] = await _context.ComponentTypes.CountAsync();
                status["ComplianceCategoriesCount"] = await _context.ComplianceCategories.CountAsync();
                status["LegacyMappingsCount"] = await _context.LegacyFlagToStageMaps.CountAsync();

                // Count parts migration status
                var totalParts = await _context.Parts.CountAsync();
                var migratedParts = await _context.Parts.CountAsync(p => !p.IsLegacyForm);
                var legacyParts = totalParts - migratedParts;

                status["TotalParts"] = totalParts;
                status["MigratedParts"] = migratedParts;
                status["LegacyParts"] = legacyParts;
                status["MigrationProgress"] = totalParts > 0 ? (double)migratedParts / totalParts * 100 : 0;

                return status;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "? Error getting migration status");
                return new Dictionary<string, object> { ["Error"] = ex.Message };
            }
        }
    }
}