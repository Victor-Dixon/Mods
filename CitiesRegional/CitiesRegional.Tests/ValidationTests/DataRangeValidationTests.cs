using Xunit;
using CitiesRegional.Models;
namespace CitiesRegional.Tests.ValidationTests;

/// <summary>
/// Validates that data values are within expected ranges
/// </summary>
public class DataRangeValidationTests
{
    [Fact]
    public void RegionalCityData_Metrics_ShouldBeInRange0To100()
    {
        // Arrange
        var data = new RegionalCityData
        {
            Happiness = 75f,
            Health = 80f,
            Education = 70f,
            TrafficFlow = 85f,
            Pollution = 25f,
            CrimeRate = 15f
        };
        
        // Assert
        Assert.InRange(data.Happiness, 0f, 100f);
        Assert.InRange(data.Health, 0f, 100f);
        Assert.InRange(data.Education, 0f, 100f);
        Assert.InRange(data.TrafficFlow, 0f, 100f);
        Assert.InRange(data.Pollution, 0f, 100f);
        Assert.InRange(data.CrimeRate, 0f, 100f);
    }
    
    [Fact]
    public void RegionalCityData_Population_ShouldBeNonNegative()
    {
        // Arrange
        var data = new RegionalCityData
        {
            Population = 50000,
            Workers = 25000,
            UnemployedWorkers = 2000
        };
        
        // Assert
        Assert.True(data.Population >= 0);
        Assert.True(data.Workers >= 0);
        Assert.True(data.UnemployedWorkers >= 0);
    }
    
    [Fact]
    public void RegionalCityData_ResourceData_ExportImport_ShouldBeNonNegative()
    {
        // Arrange
        var data = new RegionalCityData();
        
        // Add some resource data
        data.Resources.Add(new ResourceData
        {
            Type = ResourceType.Electricity,
            Production = 1000f,
            Consumption = 800f
        });
        
        // Assert
        foreach (var resource in data.Resources)
        {
            Assert.True(resource.ExportAvailable >= 0, 
                $"{resource.Type} export should be non-negative");
            Assert.True(resource.ImportNeeded >= 0, 
                $"{resource.Type} import should be non-negative");
        }
    }
    
    [Fact]
    public void RegionalCityData_Clone_ShouldPreserveRanges()
    {
        // Arrange
        var original = new RegionalCityData
        {
            Population = 50000,
            Happiness = 75f,
            Health = 80f,
            Treasury = 5000000L
        };
        
        // Act
        var cloned = original.Clone();
        
        // Assert
        Assert.InRange(cloned.Happiness, 0f, 100f);
        Assert.InRange(cloned.Health, 0f, 100f);
        Assert.True(cloned.Population >= 0);
    }
}

