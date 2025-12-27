using Xunit;
using CitiesRegional.Tests.Tools;

namespace CitiesRegional.Tests.IntegrationTests;

/// <summary>
/// Integration tests that validate mod behavior via log analysis
/// These tests can run without the game being active
/// </summary>
public class LogBasedValidationTests : IntegrationTestBase
{
    [Fact]
    public void Logs_ShouldExist()
    {
        // Arrange & Act
        var result = AnalyzeLogs();

        if (!RequireGameLogValidation(result, requireCitiesRegionalEntries: false))
            return;
        
        // Assert
        Assert.True(result.PlayerLogExists || result.BepInExLogExists, 
            "At least one log file should exist (Player.log or BepInEx log)");
    }
    
    [Fact]
    public void FirstCall_ShouldBeDetected()
    {
        // Arrange & Act
        var result = AnalyzeLogs();

        if (!RequireGameLogValidation(result, requireFirstCall: true))
            return;
        
        // Assert
        AssertFirstCallDetected(result);
    }
    
    [Fact]
    public void Heartbeats_ShouldBePresent()
    {
        // Arrange & Act
        var result = AnalyzeLogs();

        if (!RequireGameLogValidation(result, requireHeartbeats: true))
            return;
        
        // Assert
        AssertHeartbeatsPresent(result, minCount: 1);
    }
    
    [Fact]
    public void DataUpdates_ShouldBePresent()
    {
        // Arrange & Act
        var result = AnalyzeLogs();

        if (!RequireGameLogValidation(result, requireDataUpdates: true))
            return;
        
        // Assert
        AssertDataUpdatesPresent(result, minCount: 1);
    }
    
    [Fact]
    public void NoErrors_ShouldBeInLogs()
    {
        // Arrange & Act
        var result = AnalyzeLogs();

        if (!RequireGameLogValidation(result))
            return;
        
        // Assert
        AssertNoErrors(result);
    }
    
    [Fact]
    public void Population_ShouldBeReasonable()
    {
        // Arrange & Act
        var result = AnalyzeLogs();

        if (!RequireGameLogValidation(result))
            return;
        
        if (result.Heartbeats.Count == 0 && result.DataUpdates.Count == 0)
        {
            return;
        }
        
        // Assert
        AssertPopulationReasonable(result);
    }
    
    [Fact]
    public void Treasury_ShouldBeReasonable()
    {
        // Arrange & Act
        var result = AnalyzeLogs();

        if (!RequireGameLogValidation(result))
            return;
        
        if (result.DataUpdates.Count == 0)
        {
            return;
        }
        
        // Assert
        AssertTreasuryReasonable(result);
    }
    
    [Fact]
    public void HeartbeatFrequency_ShouldBeApproximately512Frames()
    {
        // Arrange & Act
        var result = AnalyzeLogs();

        if (!RequireGameLogValidation(result, requireHeartbeats: true))
            return;
        
        if (result.Heartbeats.Count < 2)
        {
            return;
        }
        
        // Assert
        AssertHeartbeatFrequency(result, expectedInterval: 512, tolerance: 50);
    }
    
    [Fact]
    public void LogAnalysis_ShouldBeValid()
    {
        // Arrange & Act
        var result = AnalyzeLogs();

        if (!RequireGameLogValidation(result, requireFirstCall: true, requireHeartbeats: true, requireDataUpdates: true))
            return;
        
        // Assert
        Assert.True(result.IsValid, 
            $"Log analysis should be valid. FirstCall: {result.FirstCallDetected}, " +
            $"Heartbeats: {result.Heartbeats.Count}, DataUpdates: {result.DataUpdates.Count}, " +
            $"Errors: {result.Errors.Count}");
    }
}

