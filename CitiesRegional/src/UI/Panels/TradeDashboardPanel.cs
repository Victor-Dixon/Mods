using System;
using System.Collections.Generic;
using System.Linq;
using CitiesRegional.Services;
using CitiesRegional.Models;

namespace CitiesRegional.UI.Panels;

/// <summary>
/// Trade Dashboard Panel - Displays trade statistics and active trades
/// 
/// This is a placeholder structure for the Gooee React panel.
/// Will be implemented once GooeePlugin API is verified.
/// 
/// Expected Gooee Pattern:
/// - Inherit from Gooee panel base class
/// - Use React components for UI
/// - Bind to RegionalManager data
/// - Auto-update when trades change
/// </summary>
public class TradeDashboardPanel
{
    private RegionalManager? _regionalManager;
    private CitiesRegionalUI? _uiController;
    
    /// <summary>
    /// Initialize the panel with RegionalManager
    /// </summary>
    public void Initialize(RegionalManager regionalManager, CitiesRegionalUI uiController)
    {
        _regionalManager = regionalManager;
        _uiController = uiController;
        
        CitiesRegional.Logging.LogInfo("TradeDashboardPanel initialized");
    }
    
    /// <summary>
    /// Get trade statistics for display
    /// </summary>
    public TradeFlowStatistics? GetStatistics()
    {
        return _uiController?.GetTradeStatistics();
    }
    
    /// <summary>
    /// Get active trade flows for display
    /// </summary>
    public List<TradeFlow>? GetActiveTrades()
    {
        return _uiController?.GetTradeFlows();
    }
    
    /// <summary>
    /// Get formatted trade statistics for UI display
    /// </summary>
    public TradeDashboardData GetDashboardData()
    {
        var stats = GetStatistics();
        var trades = GetActiveTrades();
        
        // Calculate net trade balance from current city's stats
        float netBalance = 0f;
        if (stats != null && _regionalManager != null)
        {
            var currentCityId = _regionalManager.CurrentRegion?.Cities.FirstOrDefault()?.CityId;
            if (currentCityId != null && stats.TradeByCity.TryGetValue(currentCityId, out var cityStats))
            {
                netBalance = cityStats.NetTradeValue;
            }
        }
        
        return new TradeDashboardData
        {
            TotalTradeValue = stats?.TotalTradeValue ?? 0f,
            ActiveTradesCount = trades?.Count ?? 0,
            NetTradeBalance = netBalance,
            Trades = trades ?? new List<TradeFlow>(),
            LastUpdated = DateTime.UtcNow
        };
    }
    
    /// <summary>
    /// Format trade flow for display
    /// </summary>
    public string FormatTradeFlow(TradeFlow flow)
    {
        return $"{flow.ResourceType}: {flow.Amount:F1} from {flow.FromCityId} to {flow.ToCityId} (${flow.TotalValue:F2})";
    }
}

/// <summary>
/// Data structure for Trade Dashboard display
/// </summary>
public class TradeDashboardData
{
    public float TotalTradeValue { get; set; }
    public int ActiveTradesCount { get; set; }
    public float NetTradeBalance { get; set; }
    public List<TradeFlow> Trades { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

