using Xunit;
using CitiesRegional.Models;

namespace CitiesRegional.Tests.ValidationTests;

/// <summary>
/// Validates trade data accuracy and integration
/// </summary>
public class TradeDataValidationTests
{
    [Fact]
    public void TradeData_ExportImport_ShouldBeNonNegative()
    {
        // Arrange
        var resource = new ResourceData
        {
            Type = ResourceType.IndustrialGoods,
            Production = 1000f,
            Consumption = 800f
        };
        
        // Act
        var export = resource.ExportAvailable;
        var import = resource.ImportNeeded;
        
        // Assert
        Assert.True(export >= 0, "Export should be non-negative");
        Assert.True(import >= 0, "Import should be non-negative");
    }
    
    [Fact]
    public void TradeData_ExportAndImport_ShouldNotBothBePositive()
    {
        // Arrange
        var resource = new ResourceData
        {
            Type = ResourceType.CommercialGoods,
            Production = 1000f,
            Consumption = 800f
        };
        
        // Act
        var export = resource.ExportAvailable;
        var import = resource.ImportNeeded;
        
        // Assert - A resource should either export OR import, not both
        Assert.True(export == 0 || import == 0,
            $"Resource should either export ({export}) OR import ({import}), not both");
    }
    
    [Fact]
    public void TradeData_NetBalance_ShouldMatchExportMinusImport()
    {
        // Arrange
        var data = new RegionalCityData();
        data.Resources.Add(new ResourceData
        {
            Type = ResourceType.Electricity,
            Production = 1000f,
            Consumption = 800f
        });
        
        // Act
        var netBalance = data.GetNetTradeBalance(ResourceType.Electricity);
        var resource = data.Resources.First(r => r.Type == ResourceType.Electricity);
        
        // Assert
        var expectedBalance = resource.ExportAvailable - resource.ImportNeeded;
        Assert.Equal(expectedBalance, netBalance, 0.01f);
    }
    
    [Fact]
    public void TradeData_MultipleResources_ShouldCalculateIndependently()
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
        var electricityBalance = data.GetNetTradeBalance(ResourceType.Electricity);
        var waterBalance = data.GetNetTradeBalance(ResourceType.Water);
        
        // Assert
        Assert.True(electricityBalance > 0, "Electricity should have positive balance (export)");
        Assert.True(waterBalance < 0, "Water should have negative balance (import)");
    }
}

