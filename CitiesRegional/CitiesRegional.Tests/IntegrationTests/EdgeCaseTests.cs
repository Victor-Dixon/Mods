using Xunit;
using CitiesRegional.Models;
using CitiesRegional.Tests.Tools;

namespace CitiesRegional.Tests.IntegrationTests;

/// <summary>
/// Tests for edge cases and boundary conditions
/// </summary>
public class EdgeCaseTests : IntegrationTestBase
{
    [Fact]
    public void RegionalCityData_EmptyCity_ShouldHandleGracefully()
    {
        // Arrange
        var data = new RegionalCityData
        {
            Population = 0,
            Workers = 0,
            UnemployedWorkers = 0,
            Treasury = 0L
        };
        
        // Act & Assert
        Assert.Equal(0, data.Population);
        Assert.Equal(0, data.Workers);
        Assert.Equal(0, data.UnemployedWorkers);
        Assert.Equal(0L, data.Treasury);
        
        // Should not throw when accessing resources
        Assert.Empty(data.Resources);
        Assert.Equal(0f, data.GetNetTradeBalance(ResourceType.Electricity));
    }
    
    [Fact]
    public void RegionalCityData_NegativeTreasury_ShouldBeHandled()
    {
        // Arrange
        var data = new RegionalCityData
        {
            Population = 50000,
            Treasury = -1000000L // City in debt
        };
        
        // Act & Assert
        Assert.True(data.Treasury < 0, "Treasury should allow negative values (debt)");
        Assert.True(data.Treasury >= -1000000000L, "Treasury should be within reasonable bounds");
    }
    
    [Fact]
    public void RegionalCityData_ExtremePopulation_ShouldHandle()
    {
        // Arrange
        var data = new RegionalCityData
        {
            Population = 2000000, // Very large city
            Workers = 1000000,
            UnemployedWorkers = 100000
        };
        
        // Act & Assert
        Assert.True(data.Population > 0);
        Assert.True(data.Workers <= data.Population);
        Assert.True(data.Workers + data.UnemployedWorkers <= data.Population);
    }
    
    [Fact]
    public void ResourceData_ZeroProductionConsumption_ShouldCalculateCorrectly()
    {
        // Arrange
        var resource = new ResourceData
        {
            Type = ResourceType.Electricity,
            Production = 0f,
            Consumption = 0f
        };
        
        // Act
        var export = resource.ExportAvailable;
        var import = resource.ImportNeeded;
        
        // Assert
        Assert.Equal(0f, export);
        Assert.Equal(0f, import);
    }
    
    [Fact]
    public void ResourceData_ProductionExceedsConsumption_ShouldExport()
    {
        // Arrange
        var resource = new ResourceData
        {
            Type = ResourceType.Electricity,
            Production = 1000f,
            Consumption = 500f
        };
        
        // Act
        var export = resource.ExportAvailable;
        var import = resource.ImportNeeded;
        
        // Assert
        Assert.True(export > 0, "Should have export available");
        Assert.Equal(0f, import, 0.01f);
    }
    
    [Fact]
    public void ResourceData_ConsumptionExceedsProduction_ShouldImport()
    {
        // Arrange
        var resource = new ResourceData
        {
            Type = ResourceType.Water,
            Production = 500f,
            Consumption = 1000f
        };
        
        // Act
        var export = resource.ExportAvailable;
        var import = resource.ImportNeeded;
        
        // Assert
        Assert.Equal(0f, export, 0.01f);
        Assert.True(import > 0, "Should need imports");
    }
    
    [Fact]
    public void RegionalCityData_MetricsAtBoundaries_ShouldBeValid()
    {
        // Arrange
        var dataMin = new RegionalCityData
        {
            Happiness = 0f,
            Health = 0f,
            Education = 0f,
            TrafficFlow = 0f,
            Pollution = 100f, // Max pollution (worst)
            CrimeRate = 100f  // Max crime (worst)
        };
        
        var dataMax = new RegionalCityData
        {
            Happiness = 100f,
            Health = 100f,
            Education = 100f,
            TrafficFlow = 100f,
            Pollution = 0f,   // Min pollution (best)
            CrimeRate = 0f    // Min crime (best)
        };
        
        // Assert
        Assert.InRange(dataMin.Happiness, 0f, 100f);
        Assert.InRange(dataMin.Health, 0f, 100f);
        Assert.InRange(dataMin.Pollution, 0f, 100f);
        Assert.InRange(dataMin.CrimeRate, 0f, 100f);
        
        Assert.InRange(dataMax.Happiness, 0f, 100f);
        Assert.InRange(dataMax.Health, 0f, 100f);
        Assert.InRange(dataMax.Pollution, 0f, 100f);
        Assert.InRange(dataMax.CrimeRate, 0f, 100f);
    }
    
    [Fact]
    public void RegionalCityData_MissingResource_ShouldReturnZero()
    {
        // Arrange
        var data = new RegionalCityData();
        // No resources added
        
        // Act
        var balance = data.GetNetTradeBalance(ResourceType.Electricity);
        
        // Assert
        Assert.Equal(0f, balance);
    }
    
    [Fact]
    public void RegionalCityData_Clone_ShouldCreateIndependentCopy()
    {
        // Arrange
        var original = new RegionalCityData
        {
            Population = 50000,
            Happiness = 75f,
            Treasury = 5000000L
        };
        original.Resources.Add(new ResourceData
        {
            Type = ResourceType.Electricity,
            Production = 1000f,
            Consumption = 800f
        });
        
        // Act
        var cloned = original.Clone();
        cloned.Population = 60000;
        cloned.Happiness = 80f;
        cloned.Resources[0].Production = 2000f;
        
        // Assert
        Assert.NotEqual(original.Population, cloned.Population);
        Assert.NotEqual(original.Happiness, cloned.Happiness);
        Assert.NotEqual(original.Resources[0].Production, cloned.Resources[0].Production);
    }
}

