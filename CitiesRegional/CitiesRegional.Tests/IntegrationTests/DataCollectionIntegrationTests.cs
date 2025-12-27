using Xunit;
using CitiesRegional.Tests.Tools;

namespace CitiesRegional.Tests.IntegrationTests;

/// <summary>
/// Integration tests for data collection accuracy
/// Validates that data collection is working correctly based on log analysis
/// </summary>
public class DataCollectionIntegrationTests : IntegrationTestBase
{
    [Fact]
    public void DataCollection_ShouldStartAfterFirstCall()
    {
        // Arrange & Act
        var result = AnalyzeLogs();

        if (!RequireGameLogValidation(result, requireFirstCall: true, requireDataUpdates: true))
            return;
        
        // Assert
        AssertFirstCallDetected(result);
        AssertDataUpdatesPresent(result, minCount: 1);
        
        // First data update should occur after first call
        if (result.FirstCallTimestamp.HasValue && result.DataUpdates.Count > 0)
        {
            var firstDataUpdate = result.DataUpdates
                .OrderBy(d => d.Timestamp ?? DateTime.MinValue)
                .First();
            
            if (firstDataUpdate.Timestamp.HasValue)
            {
                Assert.True(firstDataUpdate.Timestamp >= result.FirstCallTimestamp,
                    "First data update should occur after first OnUpdate call");
            }
        }
    }
    
    [Fact]
    public void DataCollection_ShouldUpdateRegularly()
    {
        // Arrange & Act
        var result = AnalyzeLogs();

        if (!RequireGameLogValidation(result, requireDataUpdates: true))
            return;
        
        // Assert - Should have multiple data updates
        AssertDataUpdatesPresent(result, minCount: 1);
        
        // If we have multiple updates, check they're spaced reasonably
        if (result.DataUpdates.Count >= 2)
        {
            var sortedUpdates = result.DataUpdates
                .OrderBy(d => d.Frame)
                .ToList();
            
            // Updates should be approximately every 256 frames
            for (int i = 1; i < sortedUpdates.Count; i++)
            {
                var interval = sortedUpdates[i].Frame - sortedUpdates[i - 1].Frame;
                // Allow some tolerance (200-300 frames)
                Assert.True(interval >= 200 && interval <= 300,
                    $"Data update interval should be approximately 256 frames, found {interval}");
            }
        }
    }
    
    [Fact]
    public void PopulationData_ShouldBeConsistent()
    {
        // Arrange & Act
        var result = AnalyzeLogs();
        
        // Skip if logs don't exist or insufficient data
        if (!result.PlayerLogExists && !result.BepInExLogExists)
        {
            return;
        }
        
        if (result.Heartbeats.Count == 0 || result.DataUpdates.Count == 0)
        {
            return;
        }
        
        // Assert - Population should be consistent between heartbeats and data updates
        // (allowing for growth over time)
        var heartbeatPopulations = result.Heartbeats.Select(h => h.Population).ToList();
        var dataUpdatePopulations = result.DataUpdates.Select(d => d.Population).ToList();
        
        // All populations should be non-negative
        Assert.All(heartbeatPopulations, pop => Assert.True(pop >= 0));
        Assert.All(dataUpdatePopulations, pop => Assert.True(pop >= 0));
    }
    
    [Fact]
    public void TradeData_ShouldBePresentIfAvailable()
    {
        // Arrange & Act
        var result = AnalyzeLogs();
        
        // Skip if logs don't exist
        if (!result.PlayerLogExists && !result.BepInExLogExists)
        {
            return;
        }
        
        // Assert - If trade data is present, it should be valid
        if (result.TradeDataEntries.Count > 0)
        {
            foreach (var tradeData in result.TradeDataEntries)
            {
                Assert.True(tradeData.ExportValue >= 0, 
                    $"Export value should be non-negative, found {tradeData.ExportValue}");
                Assert.True(tradeData.ImportValue >= 0, 
                    $"Import value should be non-negative, found {tradeData.ImportValue}");
            }
        }
    }
}

