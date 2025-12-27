using Xunit;
using CitiesRegional.Models;

namespace CitiesRegional.Tests.ValidationTests;

/// <summary>
/// Validates data consistency (e.g., export <= production, import <= consumption)
/// </summary>
public class DataConsistencyTests
{
    [Fact]
    public void ResourceData_ExportAvailable_ShouldNotExceedProduction()
    {
        // Arrange
        var resource = new ResourceData
        {
            Type = ResourceType.Electricity,
            Production = 1000f,
            Consumption = 800f
        };
        
        // Act
        var exportAvailable = resource.ExportAvailable;
        
        // Assert
        Assert.True(exportAvailable <= resource.Production,
            $"Export available ({exportAvailable}) should not exceed production ({resource.Production})");
    }
    
    [Fact]
    public void ResourceData_ImportNeeded_ShouldNotExceedConsumption()
    {
        // Arrange
        var resource = new ResourceData
        {
            Type = ResourceType.Water,
            Production = 500f,
            Consumption = 1000f
        };
        
        // Act
        var importNeeded = resource.ImportNeeded;
        
        // Assert
        Assert.True(importNeeded <= resource.Consumption,
            $"Import needed ({importNeeded}) should not exceed consumption ({resource.Consumption})");
    }
    
    [Fact]
    public void RegionalCityData_Workers_ShouldNotExceedPopulation()
    {
        // Arrange
        var data = new RegionalCityData
        {
            Population = 50000,
            Workers = 25000,
            UnemployedWorkers = 2000
        };
        
        // Assert
        Assert.True(data.Workers <= data.Population,
            $"Workers ({data.Workers}) should not exceed population ({data.Population})");
        
        Assert.True(data.Workers + data.UnemployedWorkers <= data.Population,
            $"Workers + UnemployedWorkers ({data.Workers + data.UnemployedWorkers}) should not exceed population ({data.Population})");
    }
    
    [Fact]
    public void RegionalCityData_NetTradeBalance_ShouldBeCalculatedCorrectly()
    {
        // Arrange
        var data = new RegionalCityData();
        data.Resources.Add(new ResourceData
        {
            Type = ResourceType.Electricity,
            Production = 1000f,
            Consumption = 800f
        });
        data.Resources.Add(new ResourceData
        {
            Type = ResourceType.Water,
            Production = 500f,
            Consumption = 1000f
        });
        
        // Act
        var netBalance = data.GetNetTradeBalance(ResourceType.Electricity);
        var missingBalance = data.GetNetTradeBalance(ResourceType.Oil);
        
        // Assert
        Assert.True(netBalance > 0, "Electricity should have positive net balance (export)");
        Assert.Equal(0f, missingBalance); // Missing resource should return 0
    }
}

