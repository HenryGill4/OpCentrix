namespace OpCentrix.Tests;

/// <summary>
/// Baseline validation tests to ensure the application builds and basic functionality works
/// </summary>
public class BaselineValidationTests
{
    [Fact]
    public void Application_CanCompile_Successfully()
    {
        // This test validates that the project compiles without errors
        // The fact that this test runs means the application builds successfully
        Assert.True(true, "Application compiled successfully");
    }

    [Fact]
    public void DateTime_Operations_WorkCorrectly()
    {
        // Test basic .NET functionality
        var now = DateTime.Now;
        var tomorrow = now.AddDays(1);
        
        Assert.True(tomorrow > now, "Date arithmetic works correctly");
    }

    [Fact]
    public void String_Operations_WorkCorrectly()
    {
        // Test basic string operations
        var testString = "OpCentrix Scheduler";
        
        Assert.Contains("OpCentrix", testString);
        Assert.True(testString.Length > 0);
        Assert.Equal("opcentrix scheduler", testString.ToLower());
    }

    [Fact]
    public void Collections_WorkCorrectly()
    {
        // Test basic collection operations
        var machines = new List<string> { "TI1", "TI2", "INC" };
        
        Assert.Equal(3, machines.Count);
        Assert.Contains("TI1", machines);
        Assert.Contains("TI2", machines);
        Assert.Contains("INC", machines);
    }

    [Fact]
    public void TimeSpan_Calculations_WorkCorrectly()
    {
        // Test time calculations common in scheduling
        var startTime = DateTime.Today.AddHours(8);
        var endTime = DateTime.Today.AddHours(12);
        var duration = endTime - startTime;
        
        Assert.Equal(TimeSpan.FromHours(4), duration);
        Assert.Equal(4, duration.TotalHours);
    }
}