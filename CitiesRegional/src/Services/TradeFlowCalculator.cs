using System;
using System.Collections.Generic;
using System.Linq;
using CitiesRegional.Models;

namespace CitiesRegional.Services;

/// <summary>
/// Enhanced trade flow calculator with statistics, logging, and validation
/// </summary>
public class TradeFlowCalculator
{
    /// <summary>
    /// Calculate trade flows with enhanced statistics and logging
    /// </summary>
    public TradeFlowResult CalculateTradeFlows(Region region, float maxTravelTimeMinutes = 60f, float capacityUtilizationLimit = 0.85f)
    {
        if (region == null)
        {
            throw new ArgumentNullException(nameof(region), "Region cannot be null");
        }
        
        if (maxTravelTimeMinutes <= 0)
        {
            throw new ArgumentException("MaxTravelTimeMinutes must be greater than 0", nameof(maxTravelTimeMinutes));
        }
        
        if (capacityUtilizationLimit <= 0 || capacityUtilizationLimit > 1)
        {
            throw new ArgumentException("CapacityUtilizationLimit must be between 0 and 1", nameof(capacityUtilizationLimit));
        }
        
        var startTime = DateTime.UtcNow;
        var flows = region.CalculateTradeFlows(maxTravelTimeMinutes, capacityUtilizationLimit);
        var endTime = DateTime.UtcNow;
        
        // Calculate statistics
        var stats = CalculateStatistics(flows, region);
        
        // Log trade flow calculation
        LogTradeFlowCalculation(flows, stats, endTime - startTime);
        
        return new TradeFlowResult
        {
            Flows = flows,
            Statistics = stats,
            CalculationTime = endTime - startTime,
            CalculatedAt = endTime
        };
    }
    
    /// <summary>
    /// Calculate trade statistics from flows
    /// </summary>
    private TradeFlowStatistics CalculateStatistics(List<TradeFlow> flows, Region region)
    {
        var stats = new TradeFlowStatistics();
        
        if (flows.Count == 0)
        {
            return stats;
        }
        
        // Total trade value
        stats.TotalTradeValue = flows.Sum(f => f.TotalValue);
        
        // Total trade volume
        stats.TotalTradeVolume = flows.Sum(f => f.Amount);
        
        // Trade count
        stats.TradeCount = flows.Count;
        
        // Per-resource statistics
        stats.TradeByResource = flows
            .GroupBy(f => f.ResourceType)
            .ToDictionary(
                g => g.Key,
                g => new ResourceTradeStats
                {
                    Count = g.Count(),
                    TotalAmount = g.Sum(f => f.Amount),
                    TotalValue = g.Sum(f => f.TotalValue),
                    AveragePrice = g.Average(f => f.PricePerUnit)
                }
            );
        
        // Per-city statistics
        var cityExports = flows
            .GroupBy(f => f.FromCityId)
            .ToDictionary(
                g => g.Key,
                g => new CityTradeStats
                {
                    ExportCount = g.Count(),
                    ExportValue = g.Sum(f => f.TotalValue),
                    ExportVolume = g.Sum(f => f.Amount)
                }
            );
        
        var cityImports = flows
            .GroupBy(f => f.ToCityId)
            .ToDictionary(
                g => g.Key,
                g => new CityTradeStats
                {
                    ImportCount = g.Count(),
                    ImportValue = g.Sum(f => f.TotalValue),
                    ImportVolume = g.Sum(f => f.Amount)
                }
            );
        
        // Combine export and import stats
        stats.TradeByCity = region.Cities.ToDictionary(
            c => c.CityId,
            c =>
            {
                var exportStats = cityExports.GetValueOrDefault(c.CityId, new CityTradeStats());
                var importStats = cityImports.GetValueOrDefault(c.CityId, new CityTradeStats());
                
                return new CityTradeStats
                {
                    ExportCount = exportStats.ExportCount,
                    ExportValue = exportStats.ExportValue,
                    ExportVolume = exportStats.ExportVolume,
                    ImportCount = importStats.ImportCount,
                    ImportValue = importStats.ImportValue,
                    ImportVolume = importStats.ImportVolume,
                    NetTradeValue = exportStats.ExportValue - importStats.ImportValue
                };
            }
        );
        
        // Connection statistics
        stats.TradeByConnection = flows
            .Where(f => !string.IsNullOrEmpty(f.ConnectionId))
            .GroupBy(f => f.ConnectionId!)
            .ToDictionary(
                g => g.Key,
                g => new ConnectionTradeStats
                {
                    TradeCount = g.Count(),
                    TotalVolume = g.Sum(f => f.Amount),
                    TotalValue = g.Sum(f => f.TotalValue),
                    AverageTravelTime = g.Average(f => f.TravelTimeMinutes)
                }
            );
        
        // Average trade size
        stats.AverageTradeSize = stats.TotalTradeVolume / stats.TradeCount;
        
        // Average trade value
        stats.AverageTradeValue = stats.TotalTradeValue / stats.TradeCount;
        
        return stats;
    }
    
    /// <summary>
    /// Log trade flow calculation results
    /// </summary>
    private void LogTradeFlowCalculation(List<TradeFlow> flows, TradeFlowStatistics stats, TimeSpan calculationTime)
    {
        CitiesRegional.Logging.LogInfo($"=== Trade Flow Calculation ===");
        CitiesRegional.Logging.LogInfo($"Calculation time: {calculationTime.TotalMilliseconds:F2}ms");
        CitiesRegional.Logging.LogInfo($"Total trades: {stats.TradeCount}");
        CitiesRegional.Logging.LogInfo($"Total volume: {stats.TotalTradeVolume:F2} units");
        CitiesRegional.Logging.LogInfo($"Total value: {stats.TotalTradeValue:C}");
        CitiesRegional.Logging.LogInfo($"Average trade size: {stats.AverageTradeSize:F2} units");
        CitiesRegional.Logging.LogInfo($"Average trade value: {stats.AverageTradeValue:C}");
        
        if (stats.TradeByResource.Count > 0)
        {
            CitiesRegional.Logging.LogInfo($"Resources traded: {stats.TradeByResource.Count}");
            foreach (var resource in stats.TradeByResource)
            {
                CitiesRegional.Logging.LogInfo(
                    $"  {resource.Key}: {resource.Value.Count} trades, " +
                    $"{resource.Value.TotalAmount:F2} units, {resource.Value.TotalValue:C}"
                );
            }
        }
        
        if (stats.TradeByCity.Count > 0)
        {
            CitiesRegional.Logging.LogInfo($"Cities involved: {stats.TradeByCity.Count}");
            foreach (var city in stats.TradeByCity.Where(c => c.Value.ExportCount > 0 || c.Value.ImportCount > 0))
            {
                CitiesRegional.Logging.LogInfo(
                    $"  City {city.Key}: " +
                    $"Exports: {city.Value.ExportCount} ({city.Value.ExportValue:C}), " +
                    $"Imports: {city.Value.ImportCount} ({city.Value.ImportValue:C}), " +
                    $"Net: {city.Value.NetTradeValue:C}"
                );
            }
        }
        
        if (stats.TradeByConnection.Count > 0)
        {
            CitiesRegional.Logging.LogInfo($"Connections used: {stats.TradeByConnection.Count}");
            foreach (var connection in stats.TradeByConnection)
            {
                CitiesRegional.Logging.LogInfo(
                    $"  Connection {connection.Key}: " +
                    $"{connection.Value.TradeCount} trades, " +
                    $"{connection.Value.TotalVolume:F2} units, " +
                    $"Avg travel: {connection.Value.AverageTravelTime:F1} min"
                );
            }
        }
        
        CitiesRegional.Logging.LogInfo($"=== End Trade Flow Calculation ===");
    }
    
    /// <summary>
    /// Validate trade flows for consistency
    /// </summary>
    public List<string> ValidateTradeFlows(List<TradeFlow> flows, Region region)
    {
        var errors = new List<string>();
        
        foreach (var flow in flows)
        {
            if (flow == null)
            {
                errors.Add("Trade flow is null");
                continue;
            }
            
            // Validate city IDs
            if (string.IsNullOrEmpty(flow.FromCityId) || string.IsNullOrEmpty(flow.ToCityId))
            {
                errors.Add($"Trade flow has invalid city IDs: {flow.ResourceType}");
                continue;
            }
            
            if (flow.FromCityId == flow.ToCityId)
            {
                errors.Add($"Trade flow has same source and destination: {flow.FromCityId}");
                continue;
            }
            
            // Validate cities exist
            var fromCity = region.GetCity(flow.FromCityId);
            var toCity = region.GetCity(flow.ToCityId);
            
            if (fromCity == null)
            {
                errors.Add($"Trade flow references non-existent city: {flow.FromCityId}");
                continue;
            }
            
            if (toCity == null)
            {
                errors.Add($"Trade flow references non-existent city: {flow.ToCityId}");
                continue;
            }
            
            // Validate connection exists
            if (!string.IsNullOrEmpty(flow.ConnectionId))
            {
                var connection = region.Connections.FirstOrDefault(c => c.ConnectionId == flow.ConnectionId);
                if (connection == null)
                {
                    errors.Add($"Trade flow references non-existent connection: {flow.ConnectionId}");
                }
            }
            
            // Validate amounts
            if (flow.Amount <= 0)
            {
                errors.Add($"Trade flow has invalid amount: {flow.Amount} for {flow.ResourceType}");
            }
            
            if (flow.PricePerUnit < 0)
            {
                errors.Add($"Trade flow has negative price: {flow.PricePerUnit} for {flow.ResourceType}");
            }
        }
        
        return errors;
    }
}

/// <summary>
/// Result of trade flow calculation with statistics
/// </summary>
public class TradeFlowResult
{
    public List<TradeFlow> Flows { get; set; } = new();
    public TradeFlowStatistics Statistics { get; set; } = new();
    public TimeSpan CalculationTime { get; set; }
    public DateTime CalculatedAt { get; set; }
}

/// <summary>
/// Trade flow statistics
/// </summary>
public class TradeFlowStatistics
{
    public int TradeCount { get; set; }
    public float TotalTradeVolume { get; set; }
    public float TotalTradeValue { get; set; }
    public float AverageTradeSize { get; set; }
    public float AverageTradeValue { get; set; }
    public Dictionary<ResourceType, ResourceTradeStats> TradeByResource { get; set; } = new();
    public Dictionary<string, CityTradeStats> TradeByCity { get; set; } = new();
    public Dictionary<string, ConnectionTradeStats> TradeByConnection { get; set; } = new();
}

/// <summary>
/// Trade statistics for a resource type
/// </summary>
public class ResourceTradeStats
{
    public int Count { get; set; }
    public float TotalAmount { get; set; }
    public float TotalValue { get; set; }
    public float AveragePrice { get; set; }
}

/// <summary>
/// Trade statistics for a city
/// </summary>
public class CityTradeStats
{
    public int ExportCount { get; set; }
    public float ExportValue { get; set; }
    public float ExportVolume { get; set; }
    public int ImportCount { get; set; }
    public float ImportValue { get; set; }
    public float ImportVolume { get; set; }
    public float NetTradeValue { get; set; }
}

/// <summary>
/// Trade statistics for a connection
/// </summary>
public class ConnectionTradeStats
{
    public int TradeCount { get; set; }
    public float TotalVolume { get; set; }
    public float TotalValue { get; set; }
    public float AverageTravelTime { get; set; }
}

