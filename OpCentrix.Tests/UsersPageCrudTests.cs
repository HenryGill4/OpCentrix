using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpCentrix.Data;
using OpCentrix.Models;
using Xunit;

namespace OpCentrix.Tests;

public class UsersPageCrudTests : IClassFixture<OpCentrixWebApplicationFactory>
{
    private readonly OpCentrixWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public UsersPageCrudTests(OpCentrixWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Edit_And_Delete_User_Flow_Works()
    {
        await AuthenticateAsAdminAsync();

        // Create a user first
        var createToken = await GetAntiForgeryTokenAsync("/Admin/Users");
        var uname = $"crud_{Guid.NewGuid():N}".Substring(0,12);
        var createForm = new List<KeyValuePair<string,string>>
        {
            new("__RequestVerificationToken", createToken),
            new("UserInput.Username", uname),
            new("UserInput.FullName", "Crud Test"),
            new("UserInput.Email", $"{uname}@test.local"),
            new("UserInput.Password", "Passw0rd!"),
            new("UserInput.Role", UserRoles.Operator),
            new("UserInput.Department", "Operations"),
            new("UserInput.IsActive", "true")
        };
        var createResp = await _client.PostAsync("/Admin/Users?handler=CreateUser", new FormUrlEncodedContent(createForm));
        Assert.Equal(HttpStatusCode.Redirect, createResp.StatusCode);

        int createdId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var u = await db.Users.FirstAsync(u => u.Username == uname);
            createdId = u.Id;
        }

        // Edit the user: change role, department, inactive
        var editToken = await GetAntiForgeryTokenAsync("/Admin/Users");
        var editForm = new List<KeyValuePair<string,string>>
        {
            new("__RequestVerificationToken", editToken),
            new("EditingUserId", createdId.ToString()),
            new("UserInput.Username", uname),
            new("UserInput.FullName", "Crud Test Updated"),
            new("UserInput.Email", $"{uname}@test.local"),
            new("UserInput.Role", UserRoles.Manager),
            new("UserInput.Department", "Administration"),
            new("UserInput.IsActive", "false")
        };
        var editResp = await _client.PostAsync("/Admin/Users?handler=EditUser", new FormUrlEncodedContent(editForm));
        Assert.Equal(HttpStatusCode.Redirect, editResp.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var u = await db.Users.FindAsync(createdId);
            Assert.NotNull(u);
            Assert.Equal(UserRoles.Manager, u!.Role);
            Assert.Equal("Administration", u.Department);
            Assert.False(u.IsActive);
            Assert.Equal("Crud Test Updated", u.FullName);
        }

        // Delete the user
        var delToken = await GetAntiForgeryTokenAsync("/Admin/Users");
        var delForm = new List<KeyValuePair<string,string>>
        {
            new("__RequestVerificationToken", delToken),
            new("userId", createdId.ToString())
        };
        var delResp = await _client.PostAsync("/Admin/Users?handler=DeleteUser", new FormUrlEncodedContent(delForm));
        Assert.Equal(HttpStatusCode.Redirect, delResp.StatusCode);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var u = await db.Users.FindAsync(createdId);
            Assert.Null(u);
        }
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

    private async Task<string> GetAntiForgeryTokenAsync(string url)
    {
        var resp = await _client.GetAsync(url);
        var html = await resp.Content.ReadAsStringAsync();
        var idx = html.IndexOf("__RequestVerificationToken");
        if (idx == -1) return string.Empty;
        var vs = html.IndexOf("value=\"", idx) + 7;
        if (vs < 7) return string.Empty;
        var ve = html.IndexOf("\"", vs);
        if (ve <= vs) return string.Empty;
        return html.Substring(vs, ve - vs);
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
