using System;
using System.Collections.Generic;
using System.Linq;
using CitiesRegional.Models;
using CitiesRegional.Services;
using Xunit;

namespace CitiesRegional.Tests.IntegrationTests;

/// <summary>
/// Integration tests for multi-city trade scenarios
/// </summary>
public class MultiCityTradeTests
{
    [Fact]
    public void TwoCityTrade_ShouldCreateFlow()
    {
        // Arrange
        var region = new Region { RegionName = "Test Region" };
        
        var cityA = CreateTestCity("CityA", 10000);
        var cityB = CreateTestCity("CityB", 8000);
        
        // City A exports industrial goods
        var cityAResource = cityA.Resources.FirstOrDefault(r => r.Type == ResourceType.IndustrialGoods);
        if (cityAResource != null)
        {
            cityAResource.Production = 1000;
            cityAResource.Consumption = 500;
            cityAResource.Price = 15f;
        }
        
        // City B imports industrial goods
        var cityBResource = cityB.Resources.FirstOrDefault(r => r.Type == ResourceType.IndustrialGoods);
        if (cityBResource != null)
        {
            cityBResource.Production = 200;
            cityBResource.Consumption = 800;
            cityBResource.Price = 18f;
        }
        
        region.AddCity(cityA);
        region.AddCity(cityB);
        
        // Create connection
        var connection = new RegionalConnection
        {
            FromCityId = cityA.CityId,
            ToCityId = cityB.CityId,
            Type = ConnectionType.Highway4Lane,
            Capacity = 1000,
            TravelTimeMinutes = 30f
        };
        region.AddConnection(connection);
        
        // Act
        var calculator = new TradeFlowCalculator();
        var result = calculator.CalculateTradeFlows(region);
        
        // Assert
        Assert.NotNull(result);
        Assert.True(result.Flows.Count > 0, "Should have at least one trade flow");
        
        var industrialFlow = result.Flows.FirstOrDefault(f => f.ResourceType == ResourceType.IndustrialGoods);
        Assert.NotNull(industrialFlow);
        Assert.Equal(cityA.CityId, industrialFlow.FromCityId);
        Assert.Equal(cityB.CityId, industrialFlow.ToCityId);
        Assert.True(industrialFlow.Amount > 0);
        Assert.Equal(connection.ConnectionId, industrialFlow.ConnectionId);
        
        // Validate statistics
        Assert.True(result.Statistics.TradeCount > 0);
        Assert.True(result.Statistics.TotalTradeValue > 0);
    }
    
    [Fact]
    public void ThreeCityTrade_ShouldDistributeExports()
    {
        // Arrange
        var region = new Region { RegionName = "Test Region" };
        
        var cityA = CreateTestCity("CityA", 10000);
        var cityB = CreateTestCity("CityB", 8000);
        var cityC = CreateTestCity("CityC", 6000);
        
        // City A exports industrial goods
        var cityAResource = cityA.Resources.FirstOrDefault(r => r.Type == ResourceType.IndustrialGoods);
        if (cityAResource != null)
        {
            cityAResource.Production = 2000;
            cityAResource.Consumption = 500;
            cityAResource.Price = 15f;
        }
        
        // City B imports industrial goods
        var cityBResource = cityB.Resources.FirstOrDefault(r => r.Type == ResourceType.IndustrialGoods);
        if (cityBResource != null)
        {
            cityBResource.Production = 200;
            cityBResource.Consumption = 600;
            cityBResource.Price = 18f;
        }
        
        // City C imports industrial goods
        var cityCResource = cityC.Resources.FirstOrDefault(r => r.Type == ResourceType.IndustrialGoods);
        if (cityCResource != null)
        {
            cityCResource.Production = 100;
            cityCResource.Consumption = 500;
            cityCResource.Price = 17f;
        }
        
        region.AddCity(cityA);
        region.AddCity(cityB);
        region.AddCity(cityC);
        
        // Create connections
        var connectionAB = new RegionalConnection
        {
            FromCityId = cityA.CityId,
            ToCityId = cityB.CityId,
            Type = ConnectionType.Highway4Lane,
            Capacity = 1000,
            TravelTimeMinutes = 25f
        };
        region.AddConnection(connectionAB);
        
        var connectionAC = new RegionalConnection
        {
            FromCityId = cityA.CityId,
            ToCityId = cityC.CityId,
            Type = ConnectionType.Highway4Lane,
            Capacity = 800,
            TravelTimeMinutes = 35f
        };
        region.AddConnection(connectionAC);
        
        // Act
        var calculator = new TradeFlowCalculator();
        var result = calculator.CalculateTradeFlows(region);
        
        // Assert
        Assert.NotNull(result);
        
        var industrialFlows = result.Flows.Where(f => f.ResourceType == ResourceType.IndustrialGoods).ToList();
        Assert.True(industrialFlows.Count >= 2, "Should have flows to both cities");
        
        var totalExported = industrialFlows.Sum(f => f.Amount);
        Assert.True(totalExported > 0, "Should export goods to both cities");
        
        // City B should get priority (closer, better connection)
        var flowToB = industrialFlows.FirstOrDefault(f => f.ToCityId == cityB.CityId);
        var flowToC = industrialFlows.FirstOrDefault(f => f.ToCityId == cityC.CityId);
        
        if (flowToB != null && flowToC != null)
        {
            // City B should get more (closer, better connection)
            Assert.True(flowToB.Amount >= flowToC.Amount, "City B should get at least as much as City C");
        }
    }
    
    [Fact]
    public void MultiResourceTrade_ShouldHandleMultipleResources()
    {
        // Arrange
        var region = new Region { RegionName = "Test Region" };
        
        var cityA = CreateTestCity("CityA", 10000);
        var cityB = CreateTestCity("CityB", 8000);
        
        // City A exports industrial goods
        var cityAIndustrial = cityA.Resources.FirstOrDefault(r => r.Type == ResourceType.IndustrialGoods);
        if (cityAIndustrial != null)
        {
            cityAIndustrial.Production = 1000;
            cityAIndustrial.Consumption = 500;
            cityAIndustrial.Price = 15f;
        }
        
        // City A exports commercial goods
        var cityACommercial = cityA.Resources.FirstOrDefault(r => r.Type == ResourceType.CommercialGoods);
        if (cityACommercial != null)
        {
            cityACommercial.Production = 800;
            cityACommercial.Consumption = 400;
            cityACommercial.Price = 12f;
        }
        
        // City B imports both
        var cityBIndustrial = cityB.Resources.FirstOrDefault(r => r.Type == ResourceType.IndustrialGoods);
        if (cityBIndustrial != null)
        {
            cityBIndustrial.Production = 200;
            cityBIndustrial.Consumption = 600;
            cityBIndustrial.Price = 18f;
        }
        
        var cityBCommercial = cityB.Resources.FirstOrDefault(r => r.Type == ResourceType.CommercialGoods);
        if (cityBCommercial != null)
        {
            cityBCommercial.Production = 150;
            cityBCommercial.Consumption = 500;
            cityBCommercial.Price = 14f;
        }
        
        region.AddCity(cityA);
        region.AddCity(cityB);
        
        var connection = new RegionalConnection
        {
            FromCityId = cityA.CityId,
            ToCityId = cityB.CityId,
            Type = ConnectionType.Highway4Lane,
            Capacity = 2000,
            TravelTimeMinutes = 30f
        };
        region.AddConnection(connection);
        
        // Act
        var calculator = new TradeFlowCalculator();
        var result = calculator.CalculateTradeFlows(region);
        
        // Assert
        Assert.NotNull(result);
        
        var industrialFlows = result.Flows.Where(f => f.ResourceType == ResourceType.IndustrialGoods).ToList();
        var commercialFlows = result.Flows.Where(f => f.ResourceType == ResourceType.CommercialGoods).ToList();
        
        Assert.True(industrialFlows.Count > 0, "Should have industrial goods trade");
        Assert.True(commercialFlows.Count > 0, "Should have commercial goods trade");
        
        // Check statistics
        Assert.True(result.Statistics.TradeByResource.ContainsKey(ResourceType.IndustrialGoods));
        Assert.True(result.Statistics.TradeByResource.ContainsKey(ResourceType.CommercialGoods));
    }
    
    [Fact]
    public void CapacityConstrainedTrade_ShouldRespectLimits()
    {
        // Arrange
        var region = new Region { RegionName = "Test Region" };
        
        var cityA = CreateTestCity("CityA", 10000);
        var cityB = CreateTestCity("CityB", 8000);
        
        // City A exports large amount
        var cityAResource = cityA.Resources.FirstOrDefault(r => r.Type == ResourceType.IndustrialGoods);
        if (cityAResource != null)
        {
            cityAResource.Production = 10000;
            cityAResource.Consumption = 1000;
            cityAResource.Price = 15f;
        }
        
        // City B imports large amount
        var cityBResource = cityB.Resources.FirstOrDefault(r => r.Type == ResourceType.IndustrialGoods);
        if (cityBResource != null)
        {
            cityBResource.Production = 500;
            cityBResource.Consumption = 8000;
            cityBResource.Price = 18f;
        }
        
        region.AddCity(cityA);
        region.AddCity(cityB);
        
        // Create connection with limited capacity
        var connection = new RegionalConnection
        {
            FromCityId = cityA.CityId,
            ToCityId = cityB.CityId,
            Type = ConnectionType.Highway4Lane,
            Capacity = 500, // Limited capacity
            TravelTimeMinutes = 30f
        };
        region.AddConnection(connection);
        
        // Act
        var calculator = new TradeFlowCalculator();
        var result = calculator.CalculateTradeFlows(region);
        
        // Assert
        Assert.NotNull(result);
        
        var flow = result.Flows.FirstOrDefault(f => f.ResourceType == ResourceType.IndustrialGoods);
        Assert.NotNull(flow);
        
        // Trade should be limited by capacity (500 * 0.85 * 10 = 4250 max)
        Assert.True(flow.Amount <= 4250, $"Trade amount {flow.Amount} should be limited by capacity");
    }
    
    [Fact]
    public void NoConnectionTrade_ShouldNotCreateFlow()
    {
        // Arrange
        var region = new Region { RegionName = "Test Region" };
        
        var cityA = CreateTestCity("CityA", 10000);
        var cityB = CreateTestCity("CityB", 8000);
        
        // City A exports
        var cityAResource = cityA.Resources.FirstOrDefault(r => r.Type == ResourceType.IndustrialGoods);
        if (cityAResource != null)
        {
            cityAResource.Production = 1000;
            cityAResource.Consumption = 500;
        }
        
        // City B imports
        var cityBResource = cityB.Resources.FirstOrDefault(r => r.Type == ResourceType.IndustrialGoods);
        if (cityBResource != null)
        {
            cityBResource.Production = 200;
            cityBResource.Consumption = 800;
        }
        
        region.AddCity(cityA);
        region.AddCity(cityB);
        
        // No connection created
        
        // Act
        var calculator = new TradeFlowCalculator();
        var result = calculator.CalculateTradeFlows(region);
        
        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Flows);
    }
    
    [Fact]
    public void TradeFlowValidation_ShouldDetectErrors()
    {
        // Arrange
        var region = new Region { RegionName = "Test Region" };
        
        var cityA = CreateTestCity("CityA", 10000);
        region.AddCity(cityA);
        
        // Create invalid trade flow
        var invalidFlow = new TradeFlow
        {
            ResourceType = ResourceType.IndustrialGoods,
            FromCityId = "NonExistentCity",
            ToCityId = cityA.CityId,
            Amount = 100,
            PricePerUnit = 15f
        };
        
        var flows = new List<TradeFlow> { invalidFlow };
        
        // Act
        var calculator = new TradeFlowCalculator();
        var errors = calculator.ValidateTradeFlows(flows, region);
        
        // Assert
        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.Contains("NonExistentCity"));
    }
    
    private RegionalCityData CreateTestCity(string name, int population)
    {
        return new RegionalCityData
        {
            CityId = Guid.NewGuid().ToString(),
            CityName = name,
            Population = population,
            Treasury = 100000L,
            Resources = new List<ResourceData>
            {
                new ResourceData { Type = ResourceType.IndustrialGoods, Production = 0, Consumption = 0, Price = 15f },
                new ResourceData { Type = ResourceType.CommercialGoods, Production = 0, Consumption = 0, Price = 12f },
                new ResourceData { Type = ResourceType.Electricity, Production = 0, Consumption = 0, Price = 0.1f },
                new ResourceData { Type = ResourceType.Water, Production = 0, Consumption = 0, Price = 0.05f }
            }
        };
    }
}

