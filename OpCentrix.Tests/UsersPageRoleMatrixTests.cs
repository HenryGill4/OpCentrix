using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpCentrix.Data;
using OpCentrix.Models;
using Xunit;

namespace OpCentrix.Tests;

public class UsersPageRoleMatrixTests : IClassFixture<OpCentrixWebApplicationFactory>
{
    private readonly OpCentrixWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UsersPageRoleMatrixTests(OpCentrixWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Theory]
    [InlineData(UserRoles.Admin)]
    [InlineData(UserRoles.Manager)]
    [InlineData(UserRoles.Scheduler)]
    [InlineData(UserRoles.Operator)]
    [InlineData(UserRoles.PrintingSpecialist)]
    [InlineData(UserRoles.CoatingSpecialist)]
    [InlineData(UserRoles.ShippingSpecialist)]
    [InlineData(UserRoles.EDMSpecialist)]
    [InlineData(UserRoles.MachiningSpecialist)]
    [InlineData(UserRoles.QCSpecialist)]
    [InlineData(UserRoles.Analyst)]
    public async Task Create_User_Works_For_All_Roles(string role)
    {
        await AuthenticateAsAdminAsync();

        // Get token
        var page = await _client.GetAsync("/Admin/Users");
        var html = await page.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(html);
        Assert.False(string.IsNullOrEmpty(token));

        var unique = Guid.NewGuid().ToString("N").Substring(0,8);
        var username = $"{role.ToLower()}_{unique}";
        var form = new List<KeyValuePair<string,string>>
        {
            new("__RequestVerificationToken", token),
            new("UserInput.Username", username),
            new("UserInput.FullName", $"{role} Test"),
            new("UserInput.Email", $"{username}@test.local"),
            new("UserInput.Password", "TestPass123!"),
            new("UserInput.Role", role),
            new("UserInput.Department", "Operations"),
            new("UserInput.IsActive", "true")
        };

        var resp = await _client.PostAsync("/Admin/Users?handler=CreateUser", new FormUrlEncodedContent(form));
        Assert.Equal(HttpStatusCode.Redirect, resp.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
        var created = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        Assert.NotNull(created);
        Assert.Equal(role, created!.Role);
        Assert.True(created.IsActive);
        Assert.False(string.IsNullOrWhiteSpace(created.PasswordHash));
    }

    private async Task AuthenticateAsAdminAsync()
    {
        var loginPage = await _client.GetAsync("/Account/Login");
        var loginContent = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(loginContent);

        var loginData = new List<KeyValuePair<string, string>>
        {
            new("Username", "admin"),
            new("Password", "admin123"),
            new("__RequestVerificationToken", token)
        };
        var response = await _client.PostAsync("/Account/Login", new FormUrlEncodedContent(loginData));
        Assert.True(response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.OK);
    }

    private static string ExtractAntiForgeryToken(string html)
    {
        var tokenStart = html.IndexOf("__RequestVerificationToken");
        if (tokenStart == -1) return string.Empty;
        var valueStart = html.IndexOf("value=\"", tokenStart) + 7;
        var valueEnd = html.IndexOf("\"", valueStart);
        return valueEnd > valueStart ? html.Substring(valueStart, valueEnd - valueStart) : string.Empty;
    }
}
