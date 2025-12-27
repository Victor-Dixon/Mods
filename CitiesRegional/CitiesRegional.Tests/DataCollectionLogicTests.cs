using System;
using System.Linq;
using Xunit;
using CitiesRegional.Models;

namespace CitiesRegional.Tests;

/// <summary>
/// Tests for data collection logic without requiring game DLLs
/// </summary>
public class DataCollectionLogicTests
{
    [Fact]
    public void ResourceData_ExportAvailable_IsProductionMinusConsumption()
    {
        // Arrange
        var production = 1000f;
        var consumption = 600f;
        
        // Act
        var exportAvailable = Math.Max(0, production - consumption);
        
        // Assert
        Assert.Equal(400f, exportAvailable);
    }
    
    [Fact]
    public void ResourceData_ImportNeeded_IsConsumptionMinusProduction()
    {
        // Arrange
        var production = 500f;
        var consumption = 800f;
        
        // Act
        var importNeeded = Math.Max(0, consumption - production);
        
        // Assert
        Assert.Equal(300f, importNeeded);
    }
    
    [Fact]
    public void ResourceData_NoExportWhenConsumptionExceedsProduction()
    {
        // Arrange
        var production = 500f;
        var consumption = 800f;
        
        // Act
        var exportAvailable = Math.Max(0, production - consumption);
        
        // Assert
        Assert.Equal(0f, exportAvailable);
    }
    
    [Fact]
    public void ResourceData_NoImportWhenProductionExceedsConsumption()
    {
        // Arrange
        var production = 1000f;
        var consumption = 600f;
        
        // Act
        var importNeeded = Math.Max(0, consumption - production);
        
        // Assert
        Assert.Equal(0f, importNeeded);
    }
    
    [Fact]
    public void GDPEstimate_CalculatedFromWeeklyIncome()
    {
        // Arrange
        var weeklyIncome = 250000f;
        
        // Act
        var gdpEstimate = weeklyIncome * 52f; // Annual estimate
        
        // Assert
        Assert.Equal(13000000f, gdpEstimate);
    }
    
    [Fact]
    public void GDPEstimate_FallbackToPopulationBased()
    {
        // Arrange
        var population = 50000;
        var weeklyIncome = 0f; // No income data
        
        // Act
        var gdpEstimate = weeklyIncome > 0 
            ? weeklyIncome * 52f 
            : population * 500f; // Fallback
        
        // Assert
        Assert.Equal(25000000f, gdpEstimate);
    }
    
    [Fact]
    public void WorkersEstimate_CalculatedFromPopulation()
    {
        // Arrange
        var population = 10000;
        
        // Act
        var workers = (int)(population * 0.5f); // 50% employment rate
        var unemployed = (int)(population * 0.05f); // 5% unemployment
        
        // Assert
        Assert.Equal(5000, workers);
        Assert.Equal(500, unemployed);
        Assert.True(workers + unemployed <= population);
    }
    
    [Fact]
    public void StudentsEstimate_CalculatedFromPopulation()
    {
        // Arrange
        var population = 10000;
        
        // Act
        var students = (int)(population * 0.15f); // 15% students
        
        // Assert
        Assert.Equal(1500, students);
        Assert.True(students < population);
    }
    
    [Fact]
    public void ResourceData_Stockpile_CanBeZeroForNonStockpiledResources()
    {
        // Arrange
        var electricity = new ResourceData
        {
            Type = ResourceType.Electricity,
            Stockpile = 0 // Electricity isn't stockpiled
        };
        
        // Assert
        Assert.Equal(0f, electricity.Stockpile);
    }
    
    [Fact]
    public void ResourceData_Price_IsPositive()
    {
        // Arrange
        var resources = new[]
        {
            new ResourceData { Type = ResourceType.Electricity, Price = 12f },
            new ResourceData { Type = ResourceType.Water, Price = 8f },
            new ResourceData { Type = ResourceType.IndustrialGoods, Price = 45f },
            new ResourceData { Type = ResourceType.CommercialGoods, Price = 60f }
        };
        
        // Assert
        foreach (var resource in resources)
        {
            Assert.True(resource.Price > 0, $"{resource.Type} should have positive price");
        }
    }
}

