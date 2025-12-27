using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CitiesRegional.Services;
using CitiesRegional.Models;

namespace CitiesRegional.UI.Panels;

/// <summary>
/// Region Panel - Displays region information, cities, and connections
/// 
/// This is a placeholder structure for the Gooee React panel.
/// Will be implemented once GooeePlugin API is verified.
/// 
/// Expected Gooee Pattern:
/// - Inherit from Gooee panel base class
/// - Use React components for UI
/// - Bind to RegionalManager data
/// - Handle region actions (create/join/leave)
/// </summary>
public class RegionPanel
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
        
        CitiesRegional.Logging.LogInfo("RegionPanel initialized");
    }
    
    /// <summary>
    /// Get current region data
    /// </summary>
    public Region? GetCurrentRegion()
    {
        return _uiController?.GetCurrentRegion();
    }
    
    /// <summary>
    /// Get formatted region data for UI display
    /// </summary>
    public RegionPanelData GetPanelData()
    {
        var region = GetCurrentRegion();
        
        if (region == null)
        {
            return new RegionPanelData
            {
                IsInRegion = false,
                RegionName = "Not in a region",
                RegionCode = "",
                CityCount = 0,
                ConnectionCount = 0,
                Cities = new List<CityDisplayData>(),
                Connections = new List<ConnectionDisplayData>()
            };
        }
        
        return new RegionPanelData
        {
            IsInRegion = true,
            RegionName = region.RegionName,
            RegionCode = region.RegionCode,
            CityCount = region.Cities.Count,
            ConnectionCount = region.Connections.Count,
            Cities = region.Cities.Select(c => new CityDisplayData
            {
                CityId = c.CityId,
                CityName = c.CityName,
                Population = c.Population,
                Status = c.IsOnline ? "Online" : "Offline"
            }).ToList(),
            Connections = region.Connections.Select(c => new ConnectionDisplayData
            {
                ConnectionId = c.ConnectionId,
                FromCityId = c.FromCityId,
                ToCityId = c.ToCityId,
                ConnectionType = c.Type.ToString(),
                Status = DetermineConnectionStatus(c)
            }).ToList()
        };
    }
    
    /// <summary>
    /// Create a new region
    /// </summary>
    public async Task<Region?> CreateRegion(string regionName, int maxCities = 4)
    {
        if (_regionalManager == null)
        {
            CitiesRegional.Logging.LogError("Cannot create region: RegionalManager not initialized");
            return null;
        }
        
        if (string.IsNullOrWhiteSpace(regionName))
        {
            CitiesRegional.Logging.LogWarning("Cannot create region: region name is null or empty");
            return null;
        }
        
        try
        {
            CitiesRegional.Logging.LogInfo($"CreateRegion requested: {regionName} (max {maxCities} cities)");
            var region = await _regionalManager.CreateRegion(regionName, maxCities);
            CitiesRegional.Logging.LogInfo($"Region created successfully: {region.RegionCode}");
            return region;
        }
        catch (Exception ex)
        {
            CitiesRegional.Logging.LogError($"Failed to create region: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Join an existing region
    /// </summary>
    public async Task<bool> JoinRegion(string regionCode)
    {
        if (_regionalManager == null)
        {
            CitiesRegional.Logging.LogError("Cannot join region: RegionalManager not initialized");
            return false;
        }
        
        if (string.IsNullOrWhiteSpace(regionCode))
        {
            CitiesRegional.Logging.LogWarning("Cannot join region: region code is null or empty");
            return false;
        }
        
        try
        {
            CitiesRegional.Logging.LogInfo($"JoinRegion requested: {regionCode}");
            var success = await _regionalManager.JoinRegion(regionCode);
            if (success)
            {
                CitiesRegional.Logging.LogInfo($"Successfully joined region: {regionCode}");
            }
            else
            {
                CitiesRegional.Logging.LogWarning($"Failed to join region: {regionCode}");
            }
            return success;
        }
        catch (Exception ex)
        {
            CitiesRegional.Logging.LogError($"Error joining region: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Determine connection status based on RegionalConnection properties
    /// </summary>
    private string DetermineConnectionStatus(RegionalConnection connection)
    {
        if (connection.IsCongested)
        {
            return "Congested";
        }
        
        if (connection.UsagePercent > 0)
        {
            return "Active";
        }
        
        return "Idle";
    }
    
    /// <summary>
    /// Leave current region
    /// </summary>
    public async Task LeaveRegion()
    {
        if (_regionalManager == null)
        {
            CitiesRegional.Logging.LogError("Cannot leave region: RegionalManager not initialized");
            return;
        }
        
        if (!_regionalManager.IsConnected)
        {
            CitiesRegional.Logging.LogWarning("Cannot leave region: not currently in a region");
            return;
        }
        
        try
        {
            CitiesRegional.Logging.LogInfo("LeaveRegion requested");
            await _regionalManager.LeaveRegion();
            CitiesRegional.Logging.LogInfo("Successfully left region");
        }
        catch (Exception ex)
        {
            CitiesRegional.Logging.LogError($"Error leaving region: {ex.Message}");
        }
    }
}

/// <summary>
/// Data structure for Region Panel display
/// </summary>
public class RegionPanelData
{
    public bool IsInRegion { get; set; }
    public string RegionName { get; set; } = "";
    public string RegionCode { get; set; } = "";
    public int CityCount { get; set; }
    public int ConnectionCount { get; set; }
    public List<CityDisplayData> Cities { get; set; } = new();
    public List<ConnectionDisplayData> Connections { get; set; } = new();
}

/// <summary>
/// City data for display in Region Panel
/// </summary>
public class CityDisplayData
{
    public string CityId { get; set; } = "";
    public string CityName { get; set; } = "";
    public int Population { get; set; }
    public string Status { get; set; } = "Unknown";
}

/// <summary>
/// Connection data for display in Region Panel
/// </summary>
public class ConnectionDisplayData
{
    public string ConnectionId { get; set; } = "";
    public string FromCityId { get; set; } = "";
    public string ToCityId { get; set; } = "";
    public string ConnectionType { get; set; } = "";
    public string Status { get; set; } = "Unknown";
}

