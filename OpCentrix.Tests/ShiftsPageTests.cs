using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpCentrix.Data;
using OpCentrix.Models;
using Xunit;

namespace OpCentrix.Tests;

public class ShiftsPageTests : IClassFixture<OpCentrixWebApplicationFactory>
{
    private readonly OpCentrixWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ShiftsPageTests(OpCentrixWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Shifts_Page_Loads_And_Seeds_Defaults()
    {
        // Ensure empty OperatingShifts to trigger seeding
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            db.OperatingShifts.RemoveRange(db.OperatingShifts);
            await db.SaveChangesAsync();
        }

        await AuthenticateAsAdminAsync();
        var resp = await _client.GetAsync("/Admin/Shifts");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var html = await resp.Content.ReadAsStringAsync();
        Assert.Contains("Operating Shifts", html);

        // Verify seeded defaults
        using var scope2 = _factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<SchedulerContext>();
        var count = await db2.OperatingShifts.CountAsync();
        Assert.True(count >= 12); // Plant schedule creates at least 12 shifts
    }

    [Fact]
    public async Task Shifts_Add_Toggle_Delete_Flow_Works()
    {
        await AuthenticateAsAdminAsync();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");

        // Ensure at least one shift via template apply
        var applyForm = new List<KeyValuePair<string, string>>
        {
            new("template", "plant"),
            new("clearExisting", "true"),
            new("__RequestVerificationToken", token)
        };
        var applyResp = await _client.PostAsync("/Admin/Shifts?handler=ApplyTemplate", new FormUrlEncodedContent(applyForm));
        var applyJson = await applyResp.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":true", applyJson);

        // Use an existing shift id
        int createdId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var shift = await db.OperatingShifts.OrderByDescending(s => s.Id).FirstAsync();
            createdId = shift.Id;
            Assert.NotEqual(0, createdId);
        }

        // Toggle active
        var toggleResp = await _client.GetAsync($"/Admin/Shifts?handler=ToggleActive&id={createdId}");
        var toggleJson = await toggleResp.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":true", toggleJson);

        // Verify toggled
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var shift = await db.OperatingShifts.FindAsync(createdId);
            Assert.NotNull(shift);
            Assert.False(shift!.IsActive);
        }

        // Delete
        var delForm = new List<KeyValuePair<string,string>>
        {
            new("__RequestVerificationToken", token)
        };
        var delResp = await _client.PostAsync($"/Admin/Shifts?handler=Delete&id={createdId}", new FormUrlEncodedContent(delForm));
        var delJson = await delResp.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":true", delJson);

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var del = await db.OperatingShifts.FindAsync(createdId);
            Assert.Null(del);
        }
    }

    [Fact]
    public async Task Shifts_Template_Load_And_Apply_Works()
    {
        await AuthenticateAsAdminAsync();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");

        var loadResp = await _client.GetAsync("/Admin/Shifts?handler=LoadTemplate&template=plant");
        var loadJson = await loadResp.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":true", loadJson);

        var applyForm = new List<KeyValuePair<string, string>>
        {
            new("template", "plant"),
            new("clearExisting", "true"),
            new("__RequestVerificationToken", token)
        };
        var applyResp = await _client.PostAsync("/Admin/Shifts?handler=ApplyTemplate", new FormUrlEncodedContent(applyForm));
        var applyJson = await applyResp.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":true", applyJson);
    }

    [Fact]
    public async Task Assignments_List_Assign_SetPrimary_Unassign_Works()
    {
        await AuthenticateAsAdminAsync();
        var token = await GetAntiForgeryTokenAsync("/Admin/Shifts");

        // Ensure a machine and operator exist in DB
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            if (!db.Machines.Any())
            {
                db.Machines.Add(new Machine { MachineId = "TEST-M1", MachineName = "Test Machine", IsActive = true });
                await db.SaveChangesAsync();
            }
            if (!db.Users.Any(u => u.Role == UserRoles.Operator))
            {
                db.Users.Add(new User { Username = "op1", Email = "op1@test", Role = UserRoles.Operator, IsActive = true, PasswordHash = "x" });
                await db.SaveChangesAsync();
            }
        }

        string machineId;
        int userId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            machineId = db.Machines.First().MachineId;
            userId = db.Users.First(u => u.IsActive && (u.Role == UserRoles.Operator || u.Role.EndsWith("Specialist"))).Id;
        }

        // Load assignments panel
        var panelResp = await _client.GetAsync($"/Admin/Shifts?handler=Assignments&machineId={Uri.EscapeDataString(machineId)}");
        Assert.Equal(HttpStatusCode.OK, panelResp.StatusCode);
        var panelHtml = await panelResp.Content.ReadAsStringAsync();
        Assert.Contains("Assignments", panelHtml);

        // Assign primary via form handler to avoid JSON antiforgery issues
        var assignForm = new List<KeyValuePair<string,string>>
        {
            new("machineId", machineId),
            new("userId", userId.ToString()),
            new("isPrimary", "true"),
            new("force", "true"),
            new("__RequestVerificationToken", token)
        };
        var assignResp = await _client.PostAsync("/Admin/Shifts?handler=AssignForm", new FormUrlEncodedContent(assignForm));
        var assignJson = await assignResp.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":true", assignJson);

        int assignmentId;
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SchedulerContext>();
            var a = await db.MachineOperatorAssignments.OrderByDescending(a => a.Id).FirstAsync();
            assignmentId = a.Id;
            Assert.True(a.IsPrimary);
        }

        // Set primary again (idempotent)
        var setForm = new List<KeyValuePair<string,string>>
        {
            new("assignmentId", assignmentId.ToString()),
            new("__RequestVerificationToken", token)
        };
        var setResp = await _client.PostAsync("/Admin/Shifts?handler=SetPrimaryForm", new FormUrlEncodedContent(setForm));
        var setJson = await setResp.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":true", setJson);

        // Unassign
        var unassignForm = new List<KeyValuePair<string,string>>
        {
            new("assignmentId", assignmentId.ToString()),
            new("__RequestVerificationToken", token)
        };
        var unassignResp = await _client.PostAsync("/Admin/Shifts?handler=UnassignForm", new FormUrlEncodedContent(unassignForm));
        var unassignJson = await unassignResp.Content.ReadAsStringAsync();
        Assert.Contains("\"success\":true", unassignJson);
    }

    private async Task AuthenticateAsAdminAsync()
    {
        // Get login page for anti-forgery token
        var loginPage = await _client.GetAsync("/Account/Login");
        var loginContent = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(loginContent);

        var loginData = new List<KeyValuePair<string, string>>
        {
            new("Username", "admin"),
            new("Password", "admin123"),
            new("__RequestVerificationToken", token)
        };
        var formContent = new FormUrlEncodedContent(loginData);
        var response = await _client.PostAsync("/Account/Login", formContent);
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

    private async Task<string> GetAntiForgeryTokenAsync(string url)
    {
        var resp = await _client.GetAsync(url);
        var html = await resp.Content.ReadAsStringAsync();
        // Find __RequestVerificationToken value
        var idx = html.IndexOf("__RequestVerificationToken");
        if (idx == -1) return string.Empty;
        var vs = html.IndexOf("value=\"", idx) + 7;
        if (vs < 7) return string.Empty;
        var ve = html.IndexOf("\"", vs);
        if (ve <= vs) return string.Empty;
        return html.Substring(vs, ve - vs);
    }
}
