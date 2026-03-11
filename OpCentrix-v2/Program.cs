using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using OpCentrix.Data;
using OpCentrix.Middleware;
using OpCentrix.MultiTenancy;

var builder = WebApplication.CreateBuilder(args);

// ?? Platform database (shared across tenants) ???????????????????
var platformDb = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "platform.db");
Directory.CreateDirectory(Path.GetDirectoryName(platformDb)!);

builder.Services.AddDbContext<PlatformDbContext>(options =>
    options.UseSqlite($"Data Source={platformDb}"));

// ?? Multi-tenancy ???????????????????????????????????????????????
builder.Services.AddScoped<ITenantContext, TenantContext>();
builder.Services.AddSingleton<TenantDbContextFactory>();
builder.Services.AddScoped<TenantDbContext>(sp =>
{
    var tenantCtx = sp.GetRequiredService<ITenantContext>();
    var factory = sp.GetRequiredService<TenantDbContextFactory>();

    return tenantCtx.IsResolved
        ? factory.Create(tenantCtx.TenantCode!)
        : throw new InvalidOperationException("Tenant not resolved. Ensure TenantMiddleware runs before accessing TenantDbContext.");
});

// ?? Authentication (cookie-based, same as v1) ???????????????????
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";
        options.LogoutPath = "/Logout";
        options.AccessDeniedPath = "/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(12);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ?? Razor Pages ?????????????????????????????????????????????????
builder.Services.AddRazorPages();

var app = builder.Build();

// ?? Ensure platform DB is created ???????????????????????????????
using (var scope = app.Services.CreateScope())
{
    var platformContext = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
    platformContext.Database.EnsureCreated();
}

// ?? HTTP pipeline ???????????????????????????????????????????????
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<TenantMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
