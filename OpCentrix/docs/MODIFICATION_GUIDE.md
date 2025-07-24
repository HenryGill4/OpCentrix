# ????? OpCentrix Modification Guide for New Programmers

## ?? Welcome to OpCentrix Development!

This comprehensive guide will help new programmers understand how to modify, extend, and maintain the OpCentrix SLS Print Job Scheduler. Whether you're fixing bugs, adding features, or customizing the system, this guide provides step-by-step instructions and best practices.

## ?? Getting Started

### **Prerequisites**
Before you begin, ensure you have:
- **.NET 8.0 SDK** installed
- **Visual Studio 2022** or **VS Code** with C# extension
- **Git** for version control
- **Basic understanding** of C#, HTML, CSS, and JavaScript
- **Familiarity** with ASP.NET Core and Entity Framework Core

### **Development Environment Setup**
```bash
# 1. Clone the repository
git clone [repository-url]
cd OpCentrix

# 2. Restore NuGet packages
dotnet restore

# 3. Build the solution
dotnet build

# 4. Run database migrations
dotnet ef database update

# 5. Start the application
dotnet run
```

### **Test the Installation**
- Navigate to `http://localhost:5000`
- Login with `admin` / `admin123`
- Verify all modules are working correctly

## ??? Architecture Understanding

### **Project Structure Overview**
```
OpCentrix/
??? ?? Data/                    # Database layer
?   ??? SchedulerContext.cs    # EF Core context
??? ?? Models/                  # Data models
?   ??? Job.cs                 # Core entities
?   ??? Part.cs
?   ??? ViewModels/            # Page-specific models
??? ?? Services/               # Business logic
?   ??? SchedulerService.cs    # Core scheduling
?   ??? AuthenticationService.cs
??? ?? Pages/                  # UI layer (Razor Pages)
?   ??? Scheduler/             # Main scheduling interface
?   ??? Admin/                 # Administrative functions
?   ??? Shared/                # Layouts and components
??? ?? wwwroot/                # Static files
?   ??? css/site.css          # Main stylesheet
?   ??? js/site.js            # Main JavaScript
??? Program.cs                 # Application startup
```

### **Key Concepts**
- **Razor Pages**: Page-based architecture with code-behind files
- **Entity Framework Core**: ORM for database operations
- **Dependency Injection**: Services registered in `Program.cs`
- **HTMX**: For seamless partial page updates
- **Role-Based Security**: Different access levels for different users

## ?? Common Modification Scenarios

## 1?? Adding a New Page

### **Scenario**: Add a new "Materials" management page

#### **Step 1: Create the Model**
```csharp
// Models/Material.cs
public class Material
{
    public int Id { get; set; }
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;
    public decimal CostPerKg { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}
```

#### **Step 2: Update Database Context**
```csharp
// Data/SchedulerContext.cs
public class SchedulerContext : DbContext
{
    // Add this property
    public DbSet<Material> Materials { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Add configuration
        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.CostPerKg).HasPrecision(10, 2);
        });
    }
}
```

#### **Step 3: Create Migration**
```bash
dotnet ef migrations add AddMaterialsTable
dotnet ef database update
```

#### **Step 4: Create Page Model**
```csharp
// Pages/Admin/Materials.cshtml.cs
[Authorize(Policy = "AdminOnly")]
public class MaterialsModel : PageModel
{
    private readonly SchedulerContext _context;
    private readonly ILogger<MaterialsModel> _logger;

    public MaterialsModel(SchedulerContext context, ILogger<MaterialsModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public List<Material> Materials { get; set; } = new();

    [BindProperty]
    public Material NewMaterial { get; set; } = new();

    public async Task OnGetAsync()
    {
        Materials = await _context.Materials
            .Where(m => m.IsActive)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        _context.Materials.Add(NewMaterial);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Material created: {MaterialName}", NewMaterial.Name);
        
        return RedirectToPage();
    }
}
```

#### **Step 5: Create Razor Page**
```html
<!-- Pages/Admin/Materials.cshtml -->
@page
@model OpCentrix.Pages.Admin.MaterialsModel
@{
    ViewData["Title"] = "Materials Management";
}

<div class="opcentrix-card">
    <div class="opcentrix-card-header">
        <h2 class="text-xl font-semibold">Materials Management</h2>
    </div>
    
    <div class="opcentrix-card-body">
        <!-- Create Material Form -->
        <form method="post" asp-page-handler="Create" class="mb-6">
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                    <label asp-for="NewMaterial.Name" class="opcentrix-label">Name</label>
                    <input asp-for="NewMaterial.Name" class="opcentrix-input" />
                    <span asp-validation-for="NewMaterial.Name" class="text-danger"></span>
                </div>
                <div>
                    <label asp-for="NewMaterial.CostPerKg" class="opcentrix-label">Cost per Kg</label>
                    <input asp-for="NewMaterial.CostPerKg" class="opcentrix-input" type="number" step="0.01" />
                    <span asp-validation-for="NewMaterial.CostPerKg" class="text-danger"></span>
                </div>
                <div class="flex items-end">
                    <button type="submit" class="opcentrix-button opcentrix-button-primary">
                        Add Material
                    </button>
                </div>
            </div>
        </form>
        
        <!-- Materials List -->
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            @foreach (var material in Model.Materials)
            {
                <div class="opcentrix-card">
                    <div class="opcentrix-card-body">
                        <h3 class="font-semibold">@material.Name</h3>
                        <p class="text-gray-600">@material.Description</p>
                        <p class="font-medium">$@material.CostPerKg.ToString("F2")/kg</p>
                    </div>
                </div>
            }
        </div>
    </div>
</div>
```

#### **Step 6: Add Navigation**
```html
<!-- Pages/Shared/_AdminLayout.cshtml -->
<!-- Add to admin navigation -->
<a href="/Admin/Materials" class="admin-nav-item">
    <svg class="admin-nav-icon"><!-- Material icon --></svg>
    Materials
</a>
```

## 2?? Adding Database Fields

### **Scenario**: Add tracking fields to Job entity

#### **Step 1: Update Entity Model**
```csharp
// Models/Job.cs
public class Job
{
    // ... existing properties ...
    
    // NEW: Add these properties
    public string? CustomerPO { get; set; }
    public DateTime? RequestedDeliveryDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public string? SpecialInstructions { get; set; }
}
```

#### **Step 2: Update Database Context Configuration**
```csharp
// Data/SchedulerContext.cs - in OnModelCreating method
modelBuilder.Entity<Job>(entity =>
{
    // ... existing configuration ...
    
    // NEW: Add these configurations
    entity.Property(e => e.CustomerPO).HasMaxLength(100);
    entity.Property(e => e.SpecialInstructions).HasMaxLength(1000);
    entity.Property(e => e.EstimatedCost).HasPrecision(10, 2);
    
    // Add index for customer PO lookup
    entity.HasIndex(e => e.CustomerPO);
});
```

#### **Step 3: Create and Apply Migration**
```bash
dotnet ef migrations add AddJobTrackingFields
dotnet ef database update
```

#### **Step 4: Update ViewModels**
```csharp
// Models/ViewModels/AddEditJobViewModel.cs
public class AddEditJobViewModel
{
    // ... existing properties ...
    
    [StringLength(100)]
    public string? CustomerPO { get; set; }
    
    [Display(Name = "Requested Delivery")]
    public DateTime? RequestedDeliveryDate { get; set; }
    
    [Display(Name = "Estimated Cost")]
    [Range(0, 999999.99)]
    public decimal EstimatedCost { get; set; }
    
    [StringLength(1000)]
    [Display(Name = "Special Instructions")]
    public string? SpecialInstructions { get; set; }
}
```

#### **Step 5: Update Form UI**
```html
<!-- Pages/Scheduler/_AddEditJobModal.cshtml -->
<!-- Add these fields to the form -->
<div class="form-group">
    <label asp-for="CustomerPO" class="opcentrix-label">Customer PO</label>
    <input asp-for="CustomerPO" class="opcentrix-input" />
    <span asp-validation-for="CustomerPO" class="text-danger"></span>
</div>

<div class="form-group">
    <label asp-for="RequestedDeliveryDate" class="opcentrix-label">Requested Delivery</label>
    <input asp-for="RequestedDeliveryDate" type="datetime-local" class="opcentrix-input" />
    <span asp-validation-for="RequestedDeliveryDate" class="text-danger"></span>
</div>

<div class="form-group">
    <label asp-for="EstimatedCost" class="opcentrix-label">Estimated Cost</label>
    <input asp-for="EstimatedCost" type="number" step="0.01" class="opcentrix-input" />
    <span asp-validation-for="EstimatedCost" class="text-danger"></span>
</div>

<div class="form-group">
    <label asp-for="SpecialInstructions" class="opcentrix-label">Special Instructions</label>
    <textarea asp-for="SpecialInstructions" rows="3" class="opcentrix-input"></textarea>
    <span asp-validation-for="SpecialInstructions" class="text-danger"></span>
</div>
```

#### **Step 6: Update Service Layer**
```csharp
// Services/SchedulerService.cs
public async Task<Job> CreateJobAsync(AddEditJobViewModel model, string username)
{
    var job = new Job
    {
        // ... existing mappings ...
        
        // NEW: Map new properties
        CustomerPO = model.CustomerPO,
        RequestedDeliveryDate = model.RequestedDeliveryDate,
        EstimatedCost = model.EstimatedCost,
        SpecialInstructions = model.SpecialInstructions,
    };
    
    // ... rest of method
}
```

## 3?? Adding New User Roles

### **Scenario**: Add "QualityManager" role with specific permissions

#### **Step 1: Update Role Constants**
```csharp
// Create a constants file: Models/UserRoles.cs
public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string Scheduler = "Scheduler";
    public const string Operator = "Operator";
    public const string QualityManager = "QualityManager"; // NEW
    // ... other roles
}
```

#### **Step 2: Update Authorization Policies**
```csharp
// Program.cs - in authorization configuration section
builder.Services.AddAuthorization(options =>
{
    // ... existing policies ...
    
    // NEW: Add quality management policy
    options.AddPolicy("QualityManagement", policy =>
        policy.RequireRole(UserRoles.Admin, UserRoles.Manager, UserRoles.QualityManager));
        
    // NEW: Quality-specific access
    options.AddPolicy("QualityReports", policy =>
        policy.RequireRole(UserRoles.Admin, UserRoles.QualityManager));
});
```

#### **Step 3: Create Role-Specific Pages**
```csharp
// Pages/Quality/Index.cshtml.cs
[Authorize(Policy = "QualityManagement")]
public class QualityIndexModel : PageModel
{
    public async Task OnGetAsync()
    {
        // Quality dashboard logic
    }
}
```

#### **Step 4: Update Navigation**
```html
<!-- Pages/Shared/_Layout.cshtml -->
@if (User.IsInRole(UserRoles.QualityManager) || User.IsInRole(UserRoles.Admin))
{
    <a href="/Quality" class="nav-item">
        <svg class="nav-icon"><!-- Quality icon --></svg>
        Quality
    </a>
}
```

#### **Step 5: Seed New Role User**
```csharp
// Services/SlsDataSeedingService.cs - in SeedUsersAsync method
var users = new List<User>
{
    // ... existing users ...
    
    new User
    {
        Username = "qualitymanager",
        PasswordHash = BCrypt.Net.BCrypt.HashPassword("quality123"),
        FullName = "Quality Manager",
        Email = "quality@opcentrix.local",
        Role = UserRoles.QualityManager,
        Department = "Quality Control",
        IsActive = true
    }
};
```

## 4?? Customizing the Design System

### **Scenario**: Change color scheme and add custom components

#### **Step 1: Update CSS Variables**
```css
/* wwwroot/css/site.css */
:root {
    /* Original colors commented out */
    /* --opcentrix-primary: #3B82F6; */
    
    /* NEW: Custom brand colors */
    --opcentrix-primary: #7C3AED;        /* Purple */
    --opcentrix-primary-dark: #5B21B6;   /* Dark purple */
    --opcentrix-secondary: #EC4899;      /* Pink */
    --opcentrix-accent: #06B6D4;         /* Cyan */
}
```

#### **Step 2: Create Custom Component**
```css
/* Add to wwwroot/css/site.css */
.status-badge {
    display: inline-flex;
    align-items: center;
    padding: var(--opcentrix-spacing-1) var(--opcentrix-spacing-3);
    border-radius: var(--opcentrix-radius-full);
    font-size: var(--opcentrix-text-xs);
    font-weight: var(--opcentrix-font-semibold);
    text-transform: uppercase;
    letter-spacing: 0.05em;
}

.status-badge.active {
    background: var(--opcentrix-success-light);
    color: var(--opcentrix-success-dark);
}

.status-badge.pending {
    background: var(--opcentrix-warning-light);
    color: var(--opcentrix-warning-dark);
}

.status-badge.completed {
    background: var(--opcentrix-gray-200);
    color: var(--opcentrix-gray-700);
}
```

#### **Step 3: Use Custom Component**
```html
<!-- In any Razor page -->
<span class="status-badge active">Active</span>
<span class="status-badge pending">Pending</span>
<span class="status-badge completed">Completed</span>
```

## 5?? Adding JavaScript Functionality

### **Scenario**: Add real-time notifications

#### **Step 1: Create JavaScript Module**
```javascript
// wwwroot/js/notifications.js
window.NotificationSystem = {
    container: null,
    
    init: function() {
        // Create notification container
        this.container = document.createElement('div');
        this.container.className = 'notification-container';
        this.container.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
            max-width: 400px;
        `;
        document.body.appendChild(this.container);
    },
    
    show: function(message, type = 'info', duration = 5000) {
        const notification = document.createElement('div');
        notification.className = `notification notification-${type}`;
        notification.innerHTML = `
            <div class="notification-content">
                <span class="notification-message">${message}</span>
                <button class="notification-close" onclick="this.parentElement.parentElement.remove()">×</button>
            </div>
        `;
        
        // Add styles
        notification.style.cssText = `
            background: white;
            border-radius: var(--opcentrix-radius-lg);
            box-shadow: var(--opcentrix-shadow-lg);
            margin-bottom: 10px;
            padding: 16px;
            border-left: 4px solid var(--opcentrix-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'primary'});
            animation: slideInRight 0.3s ease-out;
        `;
        
        this.container.appendChild(notification);
        
        // Auto-remove after duration
        if (duration > 0) {
            setTimeout(() => {
                if (notification.parentElement) {
                    notification.remove();
                }
            }, duration);
        }
    },
    
    success: function(message, duration) {
        this.show(message, 'success', duration);
    },
    
    error: function(message, duration) {
        this.show(message, 'error', duration);
    },
    
    warning: function(message, duration) {
        this.show(message, 'warning', duration);
    }
};

// Initialize on page load
document.addEventListener('DOMContentLoaded', function() {
    NotificationSystem.init();
});
```

#### **Step 2: Include in Layout**
```html
<!-- Pages/Shared/_Layout.cshtml -->
<script src="~/js/notifications.js"></script>
```

#### **Step 3: Use in Page Models**
```csharp
// Any page model
public async Task<IActionResult> OnPostAsync()
{
    try
    {
        // Your logic here
        await SomeOperation();
        
        // Add success message to TempData
        TempData["SuccessMessage"] = "Operation completed successfully!";
        return RedirectToPage();
    }
    catch (Exception ex)
    {
        TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
        return Page();
    }
}
```

#### **Step 4: Display Notifications in Layout**
```html
<!-- Pages/Shared/_Layout.cshtml -->
@if (TempData["SuccessMessage"] != null)
{
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            NotificationSystem.success('@TempData["SuccessMessage"]');
        });
    </script>
}

@if (TempData["ErrorMessage"] != null)
{
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            NotificationSystem.error('@TempData["ErrorMessage"]');
        });
    </script>
}
```

## 6?? Adding API Endpoints

### **Scenario**: Create REST API for mobile app integration

#### **Step 1: Create API Models**
```csharp
// Models/Api/JobApiModel.cs
public class JobApiModel
{
    public int Id { get; set; }
    public string PartNumber { get; set; } = string.Empty;
    public string MachineId { get; set; } = string.Empty;
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
}

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

#### **Step 2: Create API Controller**
```csharp
// Controllers/Api/JobsApiController.cs
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobsApiController : ControllerBase
{
    private readonly SchedulerContext _context;
    private readonly ILogger<JobsApiController> _logger;

    public JobsApiController(SchedulerContext context, ILogger<JobsApiController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<JobApiModel>>>> GetJobs(
        [FromQuery] string? machineId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var query = _context.Jobs.AsQueryable();

            if (!string.IsNullOrEmpty(machineId))
                query = query.Where(j => j.MachineId == machineId);

            if (startDate.HasValue)
                query = query.Where(j => j.ScheduledStart >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(j => j.ScheduledEnd <= endDate.Value);

            var jobs = await query
                .Select(j => new JobApiModel
                {
                    Id = j.Id,
                    PartNumber = j.PartNumber,
                    MachineId = j.MachineId,
                    ScheduledStart = j.ScheduledStart,
                    ScheduledEnd = j.ScheduledEnd,
                    Status = j.Status,
                    Operator = j.Operator
                })
                .ToListAsync();

            return Ok(new ApiResponse<List<JobApiModel>>
            {
                Success = true,
                Message = "Jobs retrieved successfully",
                Data = jobs
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving jobs");
            return StatusCode(500, new ApiResponse<List<JobApiModel>>
            {
                Success = false,
                Message = "An error occurred while retrieving jobs"
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<JobApiModel>>> GetJob(int id)
    {
        var job = await _context.Jobs.FindAsync(id);
        
        if (job == null)
        {
            return NotFound(new ApiResponse<JobApiModel>
            {
                Success = false,
                Message = "Job not found"
            });
        }

        var jobModel = new JobApiModel
        {
            Id = job.Id,
            PartNumber = job.PartNumber,
            MachineId = job.MachineId,
            ScheduledStart = job.ScheduledStart,
            ScheduledEnd = job.ScheduledEnd,
            Status = job.Status,
            Operator = job.Operator
        };

        return Ok(new ApiResponse<JobApiModel>
        {
            Success = true,
            Message = "Job retrieved successfully",
            Data = jobModel
        });
    }
}
```

#### **Step 3: Register API Services**
```csharp
// Program.cs - add before var app = builder.Build();
builder.Services.AddControllers(); // Add this for API controllers
```

#### **Step 4: Configure API Routing**
```csharp
// Program.cs - add before app.Run();
app.MapControllers(); // Add this for API routes
```

## ?? Testing Your Changes

### **1. Unit Testing**
```csharp
// Tests/Services/SchedulerServiceTests.cs
public class SchedulerServiceTests
{
    [Fact]
    public async Task CreateJobAsync_ValidModel_CreatesJob()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<SchedulerContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new SchedulerContext(options);
        var service = new SchedulerService(context, Mock.Of<ILogger<SchedulerService>>());

        var model = new AddEditJobViewModel
        {
            PartId = 1,
            MachineId = "TI1",
            ScheduledStart = DateTime.Now.AddHours(1),
            ScheduledEnd = DateTime.Now.AddHours(9)
        };

        // Act
        var result = await service.CreateJobAsync(model, "testuser");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("TI1", result.MachineId);
        Assert.Equal("testuser", result.CreatedBy);
    }
}
```

### **2. Integration Testing**
```csharp
// Tests/Integration/PagesTests.cs
public class PagesTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PagesTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AdminPage_RequiresAuthentication()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/Admin");

        // Assert
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString());
    }
}
```

### **3. Manual Testing Checklist**
- [ ] **Functionality**: Does the feature work as expected?
- [ ] **Responsive Design**: Does it work on mobile, tablet, and desktop?
- [ ] **Browser Compatibility**: Test in Chrome, Firefox, Edge, Safari
- [ ] **Role-Based Access**: Verify proper authorization
- [ ] **Error Handling**: Test with invalid inputs
- [ ] **Performance**: Check for slow queries or large payload sizes

## ?? Debugging Tips

### **1. Using the Error Logging System**
OpCentrix has a comprehensive error logging system. Use these commands in the browser console:

```javascript
// View current errors
debugErrors();

// View detailed error report
console.log(OpCentrixErrorLogger.getErrorReport());

// Clear error log
clearErrors();

// Log custom errors
window.ErrorLogger.log('CUSTOM', 'my-operation', new Error('Test error'), {
    context: 'testing'
});
```

### **2. Server-Side Debugging**
```csharp
// Use the enhanced logging system
public async Task<IActionResult> OnPostAsync()
{
    var operationId = Guid.NewGuid().ToString("N")[..8];
    _logger.LogInformation("?? [MYFEATURE-{OperationId}] Starting operation", operationId);
    
    try
    {
        // Your code here
        _logger.LogInformation("? [MYFEATURE-{OperationId}] Operation completed successfully", operationId);
        return RedirectToPage();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "? [MYFEATURE-{OperationId}] Operation failed: {ErrorMessage}", 
            operationId, ex.Message);
        throw;
    }
}
```

### **3. Database Debugging**
```bash
# View database schema
dotnet ef dbcontext info

# Generate SQL script
dotnet ef migrations script

# View pending migrations
dotnet ef migrations list
```

## ?? Best Practices

### **1. Code Organization**
- **Follow existing patterns**: Look at similar files for consistency
- **Separation of concerns**: Keep business logic in services
- **Use meaningful names**: Classes, methods, and variables should be descriptive
- **Add comments**: Explain complex business logic

### **2. Database Design**
- **Use migrations**: Never modify the database directly
- **Add indexes**: For frequently queried columns
- **Validate data**: Use data annotations and business rules
- **Consider performance**: Avoid N+1 queries

### **3. Security**
- **Always authorize**: Use `[Authorize]` attributes on sensitive pages
- **Validate inputs**: Both client and server-side
- **Log security events**: Track authentication and authorization attempts
- **Use HTTPS**: In production environments

### **4. Performance**
- **Optimize queries**: Use projections and filtering
- **Cache appropriately**: For frequently accessed data
- **Minimize JavaScript**: Keep client-side code efficient
- **Test with realistic data**: Performance can degrade with large datasets

### **5. Error Handling**
- **Use try-catch blocks**: Around database operations and external calls
- **Log meaningful messages**: Include operation IDs and context
- **Provide user feedback**: Show friendly error messages
- **Handle edge cases**: Consider what happens with invalid data

## ?? Deployment Guidelines

### **1. Pre-Deployment Checklist**
- [ ] **All tests pass**: Run the full test suite
- [ ] **No compilation errors**: Clean build successful
- [ ] **Database migrations**: Applied and tested
- [ ] **Configuration updated**: Connection strings, API keys, etc.
- [ ] **Security review**: Authentication and authorization working
- [ ] **Performance tested**: No major slowdowns
- [ ] **Documentation updated**: README and guides current

### **2. Environment Configuration**
```json
// appsettings.Production.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=OpCentrix;Trusted_Connection=true;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### **3. Database Migration in Production**
```bash
# Generate migration script for production
dotnet ef migrations script --output migrations.sql

# Apply in production (after backup!)
sqlcmd -S server -d OpCentrix -i migrations.sql
```

## ?? Learning Resources

### **OpCentrix-Specific**
- **Project Documentation**: `/docs/` folder contains comprehensive guides
- **Design System**: Study `/wwwroot/css/site.css` for styling patterns
- **Error Logging**: Review `/ProjectNotes/enhanced-error-logging-complete.md`
- **Architecture**: See `/ProjectNotes/refactor-completion-summary.md`

### **External Resources**
- **ASP.NET Core**: [Microsoft Documentation](https://docs.microsoft.com/aspnet/core)
- **Entity Framework Core**: [EF Core Documentation](https://docs.microsoft.com/ef/core)
- **Razor Pages**: [Razor Pages Guide](https://docs.microsoft.com/aspnet/core/razor-pages)
- **HTMX**: [HTMX Documentation](https://htmx.org/docs/)

## ?? Getting Help

### **When You're Stuck**
1. **Check the logs**: Use the error logging system to understand what's happening
2. **Review similar code**: Look at existing implementations for patterns
3. **Read the documentation**: Project notes contain detailed explanations
4. **Search the codebase**: Use Visual Studio's search to find similar implementations
5. **Test incrementally**: Make small changes and test frequently

### **Common Issues and Solutions**

#### **"Migration fails to apply"**
```bash
# Check migration status
dotnet ef migrations list

# Rollback to previous migration
dotnet ef database update PreviousMigrationName

# Remove problematic migration
dotnet ef migrations remove
```

#### **"Page returns 404"**
- Check the `@page` directive in the `.cshtml` file
- Verify the file is in the correct folder structure
- Ensure proper authorization attributes

#### **"Database connection fails"**
- Check connection string in `appsettings.json`
- Verify database file exists (for SQLite)
- Check database server is running (for SQL Server)

#### **"JavaScript errors"**
- Use browser developer tools console
- Check that all scripts are loaded
- Verify HTMX is working with network tab

#### **"CSS styles not applying"**
- Clear browser cache
- Check CSS file is included in layout
- Verify CSS selector specificity
- Use browser developer tools to inspect elements

---

## ?? Quick Reference

### **File Locations**
- **Database**: `Data/SchedulerContext.cs`
- **Models**: `Models/` folder
- **Pages**: `Pages/` folder with `.cshtml` and `.cshtml.cs` files
- **Services**: `Services/` folder
- **Styles**: `wwwroot/css/site.css`
- **Scripts**: `wwwroot/js/site.js`
- **Configuration**: `Program.cs` and `appsettings.json`

### **Common Commands**
```bash
# Build project
dotnet build

# Run project
dotnet run

# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Run tests
dotnet test

# Restore packages
dotnet restore
```

### **Useful URLs** (when running locally)
- **Main App**: `http://localhost:5000`
- **Admin**: `http://localhost:5000/Admin`
- **Scheduler**: `http://localhost:5000/Scheduler`
- **Login**: `http://localhost:5000/Account/Login`

This guide provides a solid foundation for modifying and extending OpCentrix. Remember to follow existing patterns, test thoroughly, and don't hesitate to refer to the comprehensive documentation in the `ProjectNotes/` folder!

---

*Last Updated: December 2024*  
*Version: 2.0.0*