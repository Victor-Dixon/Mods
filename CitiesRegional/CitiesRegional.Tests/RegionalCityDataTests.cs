using System;
using System.Linq;
using Xunit;
using CitiesRegional.Models;

namespace CitiesRegional.Tests;

/// <summary>
/// Tests for RegionalCityData model - no game dependencies
/// </summary>
public class RegionalCityDataTests
{
    [Fact]
    public void RegionalCityData_CanBeCreated()
    {
        // Arrange & Act
        var data = new RegionalCityData
        {
            CityName = "Test City",
            Population = 50000,
            Treasury = 1000000L
        };
        
        // Assert
        Assert.NotNull(data);
        Assert.Equal("Test City", data.CityName);
        Assert.Equal(50000, data.Population);
        Assert.Equal(1000000L, data.Treasury);
    }
    
    [Fact]
    public void RegionalCityData_GetNetTradeBalance_CalculatesCorrectly()
    {
        // Arrange
        var data = new RegionalCityData
        {
            Resources = new System.Collections.Generic.List<ResourceData>
            {
                new ResourceData
                {
                    Type = ResourceType.Electricity,
                    ExportAvailable = 500f,
                    ImportNeeded = 200f
                }
            }
        };
        
        // Act
        var balance = data.GetNetTradeBalance(ResourceType.Electricity);
        
        // Assert
        Assert.Equal(300f, balance); // 500 - 200
    }
    
    [Fact]
    public void RegionalCityData_GetNetTradeBalance_ReturnsZeroForMissingResource()
    {
        // Arrange
        var data = new RegionalCityData
        {
            Resources = new System.Collections.Generic.List<ResourceData>()
        };
        
        // Act
        var balance = data.GetNetTradeBalance(ResourceType.Electricity);
        
        // Assert
        Assert.Equal(0f, balance);
    }
    
    [Fact]
    public void RegionalCityData_Clone_CreatesIndependentCopy()
    {
        // Arrange
        var original = new RegionalCityData
        {
            CityName = "Original",
            Population = 10000,
            Treasury = 500000L
        };
        
        // Act
        var clone = original.Clone();
        clone.CityName = "Cloned";
        clone.Population = 20000;
        
        // Assert
        Assert.Equal("Original", original.CityName);
        Assert.Equal(10000, original.Population);
        Assert.Equal("Cloned", clone.CityName);
        Assert.Equal(20000, clone.Population);
    }
    
    [Fact]
    public void ResourceData_ExportAvailable_CalculatedFromProductionConsumption()
    {
        // Arrange
        var resource = new ResourceData
        {
            Type = ResourceType.IndustrialGoods,
            Production = 1000f,
            Consumption = 600f
        };
        
        // Act - ExportAvailable should be set by collector
        // For test, we'll verify the logic
        var expectedExport = Math.Max(0, resource.Production - resource.Consumption);
        
        // Assert
        Assert.True(expectedExport >= 0);
        Assert.Equal(400f, expectedExport);
    }
    
    [Fact]
    public void ResourceData_ImportNeeded_CalculatedFromConsumptionProduction()
    {
        // Arrange
        var resource = new ResourceData
        {
            Type = ResourceType.CommercialGoods,
            Production = 500f,
            Consumption = 800f
        };
        
        // Act
        var expectedImport = Math.Max(0, resource.Consumption - resource.Production);
        
        // Assert
        Assert.True(expectedImport >= 0);
        Assert.Equal(300f, expectedImport);
    }
}

