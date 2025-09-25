using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpCentrix.Data;
using OpCentrix.Models;
using Xunit;

namespace OpCentrix.Tests;

public class UsersPageTests : IClassFixture<OpCentrixWebApplicationFactory>
{
    private readonly OpCentrixWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UsersPageTests(OpCentrixWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Create_User_Works_Via_Form()
    {
        await AuthenticateAsAdminAsync();

        // Get token
        var page = await _client.GetAsync("/Admin/Users");
        var html = await page.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(html);
        Assert.False(string.IsNullOrEmpty(token));

        var unique = Guid.NewGuid().ToString("N").Substring(0,8);
        var form = new List<KeyValuePair<string,string>>
        {
            new("__RequestVerificationToken", token),
            new("UserInput.Username", $"user_{unique}"),
            new("UserInput.FullName", "Test User"),
            new("UserInput.Email", $"user_{unique}@test.local"),
            new("UserInput.Password", "TestPass123!"),
            new("UserInput.Role", UserRoles.Operator),
            new("UserInput.Department", "Operations"),
            new("UserInput.IsActive", "true")
        };

        var resp = await _client.PostAsync("/Admin/Users?handler=CreateUser", new FormUrlEncodedContent(form));
        Assert.Equal(HttpStatusCode.Redirect, resp.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
        var created = await db.Users.FirstOrDefaultAsync(u => u.Username == $"user_{unique}");
        Assert.NotNull(created);
        Assert.Equal(UserRoles.Operator, created!.Role);
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
