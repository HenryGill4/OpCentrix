using Microsoft.Playwright.MSTest;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OpCentrix.E2ETests
{
    [TestClass]
    public class SchedulerE2ETests : PlaywrightTest
    {
        private const string BaseUrl = "http://localhost:5000/Scheduler";

        [TestInitialize]
        public async Task SetUp()
        {
            // Optionally, start the app here if not already running
        }

        [TestMethod]
        public async Task Modal_Open_And_Close_Works()
        {
            await Page.GotoAsync(BaseUrl);
            // Simulate clicking "+ Add Job" button (assume button exists for test)
            await Page.ClickAsync("button[data-htmx-get*='ShowAddModal']");
            // Modal should be visible
            var modal = Page.Locator("#modal-content");
            await Expect(modal).Not.ToHaveClassAsync("hidden");
            // Close modal (simulate close button or background click)
            await modal.PressAsync("Escape");
            await Expect(modal).ToHaveClassAsync("hidden");
        }

        [TestMethod]
        public async Task Add_Job_Form_Validation_Works()
        {
            await Page.GotoAsync(BaseUrl);
            await Page.ClickAsync("button[data-htmx-get*='ShowAddModal']");
            // Try to submit with empty fields
            await Page.ClickAsync("button[type='submit']");
            // Should see validation errors
            await Expect(Page.Locator(".validation-summary-errors, .error")).ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task Add_Edit_Delete_Job_Flow_Works()
        {
            await Page.GotoAsync(BaseUrl);
            await Page.ClickAsync("button[data-htmx-get*='ShowAddModal']");
            // Fill out form (selectors may need adjustment)
            await Page.FillAsync("input[name='MachineId']", "TI1");
            await Page.FillAsync("input[name='PartId']", "1");
            await Page.FillAsync("input[name='ScheduledStart']", "2024-01-01T08:00");
            await Page.FillAsync("input[name='Quantity']", "1");
            await Page.ClickAsync("button[type='submit']");
            // Should see job in grid
            await Expect(Page.Locator(".job-block")).ToBeVisibleAsync();
            // Edit job (simulate clicking job block)
            await Page.ClickAsync(".job-block");
            await Page.FillAsync("input[name='Notes']", "Updated");
            await Page.ClickAsync("button[type='submit']");
            await Expect(Page.Locator(".job-block:has-text('Updated')")).ToBeVisibleAsync();
            // Delete job (simulate delete button in modal)
            await Page.ClickAsync(".job-block");
            await Page.ClickAsync("button[data-action='delete']");
            await Expect(Page.Locator(".job-block")).Not.ToBeVisibleAsync();
        }

        [TestMethod]
        public async Task Zoom_Controls_Change_Grid_And_Persist()
        {
            await Page.GotoAsync(BaseUrl);
            var grid = Page.Locator("#schedulerGrid");
            // Get initial width
            var initialWidth = await grid.EvaluateAsync<string>("el => getComputedStyle(el).getPropertyValue('--day-width')");
            // Click zoom in
            await Page.ClickAsync("#zoomIn");
            var zoomedInWidth = await grid.EvaluateAsync<string>("el => getComputedStyle(el).getPropertyValue('--day-width')");
            Assert.AreNotEqual(initialWidth, zoomedInWidth);
            // Click zoom out
            await Page.ClickAsync("#zoomOut");
            var zoomedOutWidth = await grid.EvaluateAsync<string>("el => getComputedStyle(el).getPropertyValue('--day-width')");
            Assert.AreEqual(initialWidth, zoomedOutWidth);
            // Reload and check persistence
            await Page.ReloadAsync();
            var persistedWidth = await grid.EvaluateAsync<string>("el => getComputedStyle(el).getPropertyValue('--day-width')");
            Assert.AreEqual(zoomedOutWidth, persistedWidth);
        }

        [TestMethod]
        public async Task Time_Granularity_Zoom_Updates_Grid()
        {
            await Page.GotoAsync(BaseUrl);
            // Click zoom in to change granularity
            await Page.ClickAsync("#zoomIn");
            await Page.WaitForURLAsync("**?zoom=hour");
            // Check that grid columns increased (slots per day > 1)
            var columns = await Page.Locator(".scheduler-grid-header").CountAsync();
            Assert.IsTrue(columns > 1);
        }
    }
}
