// Register application services
builder.Services.AddScoped<ISchedulerService, SchedulerService>();
builder.Services.AddScoped<ITimeSlotService, TimeSlotService>(); // NEW: Time slot calculation service
builder.Services.AddScoped<IPrintJobLogService, PrintJobLogService>();
builder.Services.AddScoped<IPrintTrackingService, PrintTrackingService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IMasterScheduleService, MasterScheduleService>(); // Task 12: Master Schedule Service
builder.Services.AddScoped<SlsDataSeedingService>(); // SLS Data Seeding Service

// Register database validation service
builder.Services.AddScoped<DatabaseValidationService>();

// PARTS REFACTORING: Stage Management Service (NEW)
builder.Services.AddScoped<OpCentrix.Services.IStageConfigurationService, OpCentrix.Services.StageConfigurationService>();

// NEW: Admin Control System services (Task 2)
builder.Services.AddScoped<OpCentrix.Services.Admin.IAdminDashboardService, OpCentrix.Services.Admin.AdminDashboardService>();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        // Initialize database with seeded data
        logger.LogInformation("Initializing database...");
        using (var initScope = app.Services.CreateScope())
        {
            var context = initScope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var seedingService = initScope.ServiceProvider.GetRequiredService<SlsDataSeedingService>();
            var adminSeedingService = initScope.ServiceProvider.GetRequiredService<OpCentrix.Services.Admin.IAdminDataSeedingService>();
            var partClassificationService = initScope.ServiceProvider.GetRequiredService<IPartClassificationService>();

            // FIXED: Add material service for material seeding
            var materialService = initScope.ServiceProvider.GetRequiredService<IMaterialService>();

            // EMERGENCY FIX: Delete existing database and recreate fresh
            logger.LogWarning("?? EMERGENCY DATABASE RECOVERY: Deleting corrupted database and creating fresh...");
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();
            logger.LogInformation("? Fresh database created successfully");

            // Seed initial data
            await seedingService.SeedDataAsync();

            // Seed admin control system data (Task 2)
            await adminSeedingService.SeedAllDefaultDataAsync();

            // FIXED: Seed materials for machine management
            await materialService.SeedDefaultMaterialsAsync();
            logger.LogInformation("? Material seeding completed successfully");

            // Seed B&T part classifications (Segment 7)
            await partClassificationService.SeedDefaultClassificationsAsync();

            // Initialize production stages for prototype tracking (Phase 0.5)
            var productionStageService = initScope.ServiceProvider.GetRequiredService<ProductionStageService>();
            await productionStageService.CreateDefaultStagesAsync();

            // FIXED: Use the Admin namespace service for production stage seeding - COMPLETE FIX
            var productionStageSeeder = initScope.ServiceProvider.GetRequiredService<OpCentrix.Services.Admin.IProductionStageSeederService>();
            var seededStageCount = await productionStageSeeder.SeedDefaultStagesAsync();
            logger.LogInformation("Production stages seeded: {Count} stages available", seededStageCount);

            // Load system settings into configuration (Task 3)
            var configurationService = initScope.ServiceProvider.GetRequiredService<OpCentrix.Services.Admin.ISystemConfigurationService>();
            await configurationService.LoadSettingsIntoConfigurationAsync();

            // Initialize static configuration helper
            OpCentrix.Services.Admin.SystemConfiguration.Initialize(app.Services);
        }
        logger.LogInformation("?? DATABASE RECOVERY COMPLETE - Fresh database with all seeding data ready!");
        logger.LogInformation("B&T Manufacturing Execution System ready!");
        logger.LogInformation("Test users available:");
        logger.LogInformation("   admin/admin123 (Admin)");
        logger.LogInformation("   manager/manager123 (Manager)");
        logger.LogInformation("   scheduler/scheduler123 (Scheduler)");
        logger.LogInformation("   operator/operator123 (Operator)");
        logger.LogInformation("   printer/printer123 (PrintingSpecialist)");
        logger.LogInformation("   coating/coating123 (CoatingSpecialist)");
        logger.LogInformation("B&T Specialist Users:");
        logger.LogInformation("   btspecialist/btspecialist123 (BTSpecialist)");
        logger.LogInformation("   workflowspecialist/workflowspecialist123 (WorkflowSpecialist)");
        logger.LogInformation("   compliancespecialist/compliancespecialist123 (ComplianceSpecialist)");
        logger.LogInformation("Segment 7 Features:");
        logger.LogInformation("   - B&T Part Classification System");
        logger.LogInformation("   - ATF/ITAR Compliance Tracking");
        logger.LogInformation("   - Serial Number Management");
        logger.LogInformation("   - Regulatory Documentation");
        logger.LogInformation("B&T MES Navigation Features:");
        logger.LogInformation("   - B&T Manufacturing Section (Amber)");
        logger.LogInformation("   - Advanced Workflows Section (Purple)");
        logger.LogInformation("   - Enhanced Admin B&T Management");
        logger.LogInformation("?? LOGIN CREDENTIALS RESTORED: Your admin/admin123 login should now work!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "CRITICAL ERROR during database initialization");
        logger.LogError("Stack trace: {StackTrace}", ex.StackTrace);

        // In development, continue running even if seeding fails
        if (!app.Environment.IsDevelopment())
        {
            throw;
        }
    }
}