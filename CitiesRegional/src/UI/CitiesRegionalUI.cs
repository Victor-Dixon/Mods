using System;
using System.Linq;
using CitiesRegional.Services;
using CitiesRegional.Models;

namespace CitiesRegional.UI;

/// <summary>
/// Main UI plugin for CitiesRegional mod using Gooee framework
/// </summary>
public class CitiesRegionalUI
{
    private RegionalManager? _regionalManager;
    
    /// <summary>
    /// Initialize the UI plugin
    /// </summary>
    public void Initialize(RegionalManager regionalManager)
    {
        _regionalManager = regionalManager;
        
        CitiesRegional.Logging.LogInfo("CitiesRegional UI initialized");
        
        // GooeePlugin integration will be handled separately
        // This class provides data access methods for UI components
    }
    
    /// <summary>
    /// Toggle trade dashboard visibility
    /// </summary>
    public void ShowTradeDashboard()
    {
        if (_regionalManager == null)
        {
            CitiesRegional.Logging.LogWarn("Cannot show trade dashboard: RegionalManager not initialized");
            return;
        }
        
        CitiesRegional.Logging.LogInfo("Trade dashboard requested");
    }
    
    /// <summary>
    /// Toggle region panel visibility
    /// </summary>
    public void ShowRegionPanel()
    {
        if (_regionalManager == null)
        {
            CitiesRegional.Logging.LogWarn("Cannot show region panel: RegionalManager not initialized");
            return;
        }
        
        CitiesRegional.Logging.LogInfo("Region panel requested");
    }
    
    /// <summary>
    /// Update UI with latest regional data
    /// </summary>
    public void UpdateUI()
    {
        // Called periodically to refresh UI data
    }
    
    /// <summary>
    /// Get current trade statistics for UI display
    /// </summary>
    public TradeFlowStatistics? GetTradeStatistics()
    {
        if (_regionalManager == null) return null;
        
        var region = _regionalManager.CurrentRegion;
        if (region == null) return null;
        
        var calculator = new TradeFlowCalculator();
        var result = calculator.CalculateTradeFlows(region);
        return result.Statistics;
    }
    
    /// <summary>
    /// Get current trade flows for UI display
    /// </summary>
    public System.Collections.Generic.List<TradeFlow>? GetTradeFlows()
    {
        if (_regionalManager == null) return null;
        
        var region = _regionalManager.CurrentRegion;
        if (region == null) return null;
        
        var calculator = new TradeFlowCalculator();
        var result = calculator.CalculateTradeFlows(region);
        return result.Flows;
    }
    
    /// <summary>
    /// Get current region for UI display
    /// </summary>
    public Region? GetCurrentRegion()
    {
        return _regionalManager?.CurrentRegion;
    }
}
