using System;
using System.Diagnostics;
using System.Linq;
using Xunit;
using CitiesRegional.Models;

namespace CitiesRegional.Tests.PerformanceTests;

/// <summary>
/// Performance benchmarks for data collection and processing
/// </summary>
public class PerformanceBenchmarkTests
{
    [Fact]
    public void RegionalCityData_Clone_ShouldBeFast()
    {
        // Arrange
        var data = CreateLargeRegionalCityData();
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        var cloned = data.Clone();
        stopwatch.Stop();
        
        // Assert
        Assert.NotNull(cloned);
        Assert.True(stopwatch.ElapsedMilliseconds < 100, 
            $"Clone should complete in <100ms, took {stopwatch.ElapsedMilliseconds}ms");
    }
    
    [Fact]
    public void RegionalCityData_GetNetTradeBalance_ShouldBeFast()
    {
        // Arrange
        var data = CreateLargeRegionalCityData();
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            data.GetNetTradeBalance(ResourceType.Electricity);
            data.GetNetTradeBalance(ResourceType.Water);
            data.GetNetTradeBalance(ResourceType.IndustrialGoods);
        }
        stopwatch.Stop();
        
        // Assert
        var avgTime = stopwatch.ElapsedMilliseconds / 3000.0; // 3000 calls total
        Assert.True(avgTime < 1.0, 
            $"GetNetTradeBalance should average <1ms per call, averaged {avgTime:F3}ms");
    }
    
    [Fact]
    public void ResourceData_ExportImport_Calculations_ShouldBeFast()
    {
        // Arrange
        var resource = new ResourceData
        {
            Type = ResourceType.Electricity,
            Production = 1000f,
            Consumption = 800f
        };
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 10000; i++)
        {
            var export = resource.ExportAvailable;
            var import = resource.ImportNeeded;
        }
        stopwatch.Stop();
        
        // Assert
        var avgTime = stopwatch.ElapsedMilliseconds / 20000.0; // 20000 property accesses
        Assert.True(avgTime < 0.1, 
            $"Export/Import calculations should average <0.1ms per access, averaged {avgTime:F3}ms");
    }
    
    [Fact]
    public void RegionalCityData_WithManyResources_ShouldHandleEfficiently()
    {
        // Arrange
        var data = new RegionalCityData
        {
            Population = 1000000,
            Treasury = 1000000000L
        };
        
        // Add many resources
        foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
        {
            data.Resources.Add(new ResourceData
            {
                Type = resourceType,
                Production = 1000f,
                Consumption = 800f
            });
        }
        
        // Act
        var stopwatch = Stopwatch.StartNew();
        foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
        {
            data.GetNetTradeBalance(resourceType);
        }
        stopwatch.Stop();
        
        // Assert
        var resourceCount = Enum.GetValues(typeof(ResourceType)).Length;
        var avgTime = stopwatch.ElapsedMilliseconds / (double)resourceCount;
        Assert.True(avgTime < 5.0, 
            $"GetNetTradeBalance should average <5ms per resource with {resourceCount} resources, averaged {avgTime:F3}ms");
    }
    
    private RegionalCityData CreateLargeRegionalCityData()
    {
        var data = new RegionalCityData
        {
            Population = 500000,
            Workers = 250000,
            UnemployedWorkers = 20000,
            Treasury = 50000000L,
            WeeklyIncome = 2500000f,
            WeeklyExpenses = 2000000f,
            Happiness = 75f,
            Health = 80f,
            Education = 70f,
            TrafficFlow = 85f,
            Pollution = 25f,
            CrimeRate = 15f
        };
        
        // Add multiple resources
        foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
        {
            data.Resources.Add(new ResourceData
            {
                Type = resourceType,
                Production = 1000f,
                Consumption = 800f,
                Price = 10f,
                Stockpile = 5000f
            });
        }
        
        return data;
    }
}

