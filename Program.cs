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