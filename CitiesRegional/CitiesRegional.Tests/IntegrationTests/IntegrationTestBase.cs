using System;
using System.IO;
using Xunit;
using CitiesRegional.Tests.Tools;

namespace CitiesRegional.Tests.IntegrationTests;

/// <summary>
/// Base class for integration tests
/// Provides common functionality for log analysis and validation
/// </summary>
public abstract class IntegrationTestBase : IDisposable
{
    protected LogAnalyzer LogAnalyzer { get; }
    protected LogAnalysisResult? LastAnalysisResult { get; private set; }
    
    protected IntegrationTestBase()
    {
        LogAnalyzer = new LogAnalyzer();
    }
    
    /// <summary>
    /// Analyzes logs and stores result for validation
    /// </summary>
    protected LogAnalysisResult AnalyzeLogs()
    {
        LastAnalysisResult = LogAnalyzer.Analyze();
        return LastAnalysisResult;
    }

    /// <summary>
    /// These tests validate real in-game behavior by analyzing Cities: Skylines 2 logs.
    /// They are only meaningful when the game has been run with the mod enabled and the logs contain markers.
    ///
    /// Enable explicitly by setting env var: CITIESREGIONAL_VALIDATE_GAME_LOGS=1
    /// </summary>
    protected bool RequireGameLogValidation(LogAnalysisResult result,
        bool requireCitiesRegionalEntries = true,
        bool requireFirstCall = false,
        bool requireHeartbeats = false,
        bool requireDataUpdates = false)
    {
        var enabled = string.Equals(
            Environment.GetEnvironmentVariable("CITIESREGIONAL_VALIDATE_GAME_LOGS"),
            "1",
            StringComparison.OrdinalIgnoreCase);

        if (!enabled)
            return false;

        if (!result.PlayerLogExists && !result.BepInExLogExists)
            return false;

        if (requireCitiesRegionalEntries && result.CitiesRegionalLogEntries == 0)
            return false;

        if (requireFirstCall && !result.FirstCallDetected)
            return false;

        if (requireHeartbeats && result.Heartbeats.Count == 0)
            return false;

        if (requireDataUpdates && result.DataUpdates.Count == 0)
            return false;

        return true;
    }
    
    /// <summary>
    /// Validates that first call was detected
    /// </summary>
    protected void AssertFirstCallDetected(LogAnalysisResult? result = null)
    {
        result ??= LastAnalysisResult ?? throw new InvalidOperationException("No analysis result available");
        Assert.True(result.FirstCallDetected, "First OnUpdate call should be detected in logs");
    }
    
    /// <summary>
    /// Validates that heartbeats are present
    /// </summary>
    protected void AssertHeartbeatsPresent(LogAnalysisResult? result = null, int minCount = 1)
    {
        result ??= LastAnalysisResult ?? throw new InvalidOperationException("No analysis result available");
        Assert.True(result.Heartbeats.Count >= minCount, 
            $"Expected at least {minCount} heartbeat(s), found {result.Heartbeats.Count}");
    }
    
    /// <summary>
    /// Validates that data updates are present
    /// </summary>
    protected void AssertDataUpdatesPresent(LogAnalysisResult? result = null, int minCount = 1)
    {
        result ??= LastAnalysisResult ?? throw new InvalidOperationException("No analysis result available");
        Assert.True(result.DataUpdates.Count >= minCount, 
            $"Expected at least {minCount} data update(s), found {result.DataUpdates.Count}");
    }
    
    /// <summary>
    /// Validates that no errors were found
    /// </summary>
    protected void AssertNoErrors(LogAnalysisResult? result = null)
    {
        result ??= LastAnalysisResult ?? throw new InvalidOperationException("No analysis result available");
        Assert.Empty(result.Errors);
    }
    
    /// <summary>
    /// Validates that population values are reasonable
    /// </summary>
    protected void AssertPopulationReasonable(LogAnalysisResult? result = null)
    {
        result ??= LastAnalysisResult ?? throw new InvalidOperationException("No analysis result available");
        
        foreach (var heartbeat in result.Heartbeats)
        {
            Assert.True(heartbeat.Population >= 0, $"Population should be non-negative, found {heartbeat.Population}");
        }
        
        foreach (var dataUpdate in result.DataUpdates)
        {
            Assert.True(dataUpdate.Population >= 0, $"Population should be non-negative, found {dataUpdate.Population}");
        }
    }
    
    /// <summary>
    /// Validates that treasury values are reasonable
    /// </summary>
    protected void AssertTreasuryReasonable(LogAnalysisResult? result = null)
    {
        result ??= LastAnalysisResult ?? throw new InvalidOperationException("No analysis result available");
        
        foreach (var dataUpdate in result.DataUpdates)
        {
            // Treasury can be negative in game, but should be within reasonable bounds
            Assert.True(dataUpdate.Treasury >= -1000000000L && dataUpdate.Treasury <= 100000000000L,
                $"Treasury should be within reasonable bounds, found {dataUpdate.Treasury}");
        }
    }
    
    /// <summary>
    /// Validates heartbeat frequency (should be approximately every 512 frames)
    /// </summary>
    protected void AssertHeartbeatFrequency(LogAnalysisResult? result = null, int expectedInterval = 512, int tolerance = 50)
    {
        result ??= LastAnalysisResult ?? throw new InvalidOperationException("No analysis result available");
        
        if (result.Heartbeats.Count < 2)
        {
            return; // Need at least 2 heartbeats to check frequency
        }
        
        var sortedHeartbeats = result.Heartbeats.OrderBy(h => h.Frame).ToList();
        
        for (int i = 1; i < sortedHeartbeats.Count; i++)
        {
            var interval = sortedHeartbeats[i].Frame - sortedHeartbeats[i - 1].Frame;
            var expectedMin = expectedInterval - tolerance;
            var expectedMax = expectedInterval + tolerance;
            
            Assert.True(interval >= expectedMin && interval <= expectedMax,
                $"Heartbeat interval should be approximately {expectedInterval} frames, found {interval}");
        }
    }
    
    public virtual void Dispose()
    {
        // Cleanup if needed
    }
}

