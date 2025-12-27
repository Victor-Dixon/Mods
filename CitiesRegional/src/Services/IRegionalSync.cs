using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CitiesRegional.Models;

namespace CitiesRegional.Services;

/// <summary>
/// Interface for regional synchronization services.
/// Can be implemented by cloud service, P2P, or self-hosted server.
/// </summary>
public interface IRegionalSync
{
    #region Connection
    
    /// <summary>
    /// Connect to an existing region using a region code
    /// </summary>
    /// <param name="regionCode">The code to join (e.g., "METRO-7X4K")</param>
    /// <returns>True if connection successful</returns>
    Task<bool> ConnectToRegion(string regionCode);
    
    /// <summary>
    /// Create a new region and become the host
    /// </summary>
    /// <param name="name">Region name</param>
    /// <param name="maxCities">Maximum number of cities allowed</param>
    /// <returns>The created region</returns>
    Task<Region> CreateRegion(string name, int maxCities);
    
    /// <summary>
    /// Leave the current region
    /// </summary>
    Task LeaveRegion();
    
    /// <summary>
    /// Check if currently connected to a region
    /// </summary>
    bool IsConnected { get; }
    
    #endregion
    
    #region Data Sync
    
    /// <summary>
    /// Push local city data to the region
    /// </summary>
    /// <param name="cityData">Current city data</param>
    Task PushCityData(RegionalCityData cityData);
    
    /// <summary>
    /// Pull all city data from the region
    /// </summary>
    /// <returns>List of all cities in the region</returns>
    Task<List<RegionalCityData>> PullRegionData();
    
    #endregion
    
    #region Connections
    
    /// <summary>
    /// Propose a new connection between cities
    /// </summary>
    Task<bool> ProposeConnection(RegionalConnection connection);
    
    /// <summary>
    /// Accept a proposed connection
    /// </summary>
    Task<bool> AcceptConnection(string connectionId);
    
    /// <summary>
    /// Upgrade an existing connection
    /// </summary>
    Task<bool> UpgradeConnection(string connectionId, ConnectionType newType);
    
    #endregion
    
    #region Shared Services
    
    /// <summary>
    /// Offer a service to be shared with the region
    /// </summary>
    Task<bool> OfferService(SharedServiceInfo service);
    
    /// <summary>
    /// Request access to a shared service
    /// </summary>
    Task<bool> RequestServiceAccess(string serviceId);
    
    /// <summary>
    /// Update usage of a shared service
    /// </summary>
    Task UpdateServiceUsage(string serviceId, int usage);
    
    #endregion
    
    #region Events
    
    /// <summary>
    /// Broadcast an event to all cities in the region
    /// </summary>
    Task BroadcastEvent(RegionalEvent evt);
    
    /// <summary>
    /// Event fired when a regional event is received
    /// </summary>
    event Action<RegionalEvent> OnEventReceived;
    
    #endregion
}

