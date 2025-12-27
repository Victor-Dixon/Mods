using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CitiesRegional.Models;

namespace CitiesRegional.Services;

/// <summary>
/// Central manager for regional functionality.
/// Orchestrates data collection, sync, and effect application.
/// </summary>
public class RegionalManager : IDisposable
{
    private readonly IRegionalSync _syncService;
    private readonly CityDataCollector _dataCollector;
    private readonly RegionalEffectsApplicator _effectsApplicator;
    
    private Region? _currentRegion;
    private RegionalCityData? _localCityData;
    private CancellationTokenSource? _syncCancellation;
    private Task? _syncTask;
    
    private bool _isConnected;
    private DateTime _lastSyncTime;
    
    /// <summary>
    /// Event fired when region data is updated
    /// </summary>
    public event Action<Region>? OnRegionUpdated;
    
    /// <summary>
    /// Event fired when connection status changes
    /// </summary>
    public event Action<bool>? OnConnectionStatusChanged;
    
    /// <summary>
    /// Event fired when a regional event occurs
    /// </summary>
    public event Action<RegionalEvent>? OnRegionalEvent;
    
    public RegionalManager()
    {
        // Initialize with cloud sync by default
        _syncService = new CloudRegionalSync();
        _dataCollector = new CityDataCollector();
        _effectsApplicator = new RegionalEffectsApplicator();
        
        // Subscribe to sync service events
        _syncService.OnEventReceived += HandleRegionalEvent;
    }
    
    #region Public Properties
    
    /// <summary>Is connected to a region?</summary>
    public bool IsConnected => _isConnected;
    
    /// <summary>Current region (if connected)</summary>
    public Region? CurrentRegion => _currentRegion;
    
    /// <summary>Local city data</summary>
    public RegionalCityData? LocalCityData => _localCityData;
    
    /// <summary>Last successful sync time</summary>
    public DateTime LastSyncTime => _lastSyncTime;
    
    #endregion
    
    #region Region Management
    
    /// <summary>
    /// Create a new region and become the host
    /// </summary>
    public async Task<Region> CreateRegion(string regionName, int maxCities = 4)
    {
        CitiesRegional.Logging.LogInfo($"Creating region: {regionName}");
        
        var region = await _syncService.CreateRegion(regionName, maxCities);
        _currentRegion = region;
        
        // Add our city to the region
        _localCityData = _dataCollector.CollectCurrentData();
        await _syncService.PushCityData(_localCityData);
        
        _isConnected = true;
        OnConnectionStatusChanged?.Invoke(true);
        
        StartSyncLoop();
        
        CitiesRegional.Logging.LogInfo($"Region created: {region.RegionCode}");
        return region;
    }
    
    /// <summary>
    /// Join an existing region using a region code
    /// </summary>
    public async Task<bool> JoinRegion(string regionCode)
    {
        CitiesRegional.Logging.LogInfo($"Joining region: {regionCode}");
        
        var success = await _syncService.ConnectToRegion(regionCode);
        if (!success)
        {
            CitiesRegional.Logging.LogWarning($"Failed to join region: {regionCode}");
            return false;
        }
        
        // Get initial region data
        var cities = await _syncService.PullRegionData();
        _currentRegion = new Region
        {
            RegionCode = regionCode,
            Cities = cities
        };
        
        // Add our city
        _localCityData = _dataCollector.CollectCurrentData();
        await _syncService.PushCityData(_localCityData);
        
        _isConnected = true;
        OnConnectionStatusChanged?.Invoke(true);
        
        StartSyncLoop();
        
        CitiesRegional.Logging.LogInfo($"Joined region successfully");
        return true;
    }
    
    /// <summary>
    /// Leave the current region
    /// </summary>
    public async Task LeaveRegion()
    {
        if (!_isConnected) return;
        
        CitiesRegional.Logging.LogInfo("Leaving region...");
        
        StopSyncLoop();
        
        await _syncService.LeaveRegion();
        
        _currentRegion = null;
        _isConnected = false;
        OnConnectionStatusChanged?.Invoke(false);
        
        CitiesRegional.Logging.LogInfo("Left region");
    }
    
    #endregion
    
    #region Sync Loop
    
    private void StartSyncLoop()
    {
        _syncCancellation = new CancellationTokenSource();
        _syncTask = RunSyncLoop(_syncCancellation.Token);
    }
    
    private void StopSyncLoop()
    {
        _syncCancellation?.Cancel();
        try
        {
            _syncTask?.Wait(TimeSpan.FromSeconds(5));
        }
        catch (AggregateException) { }
        _syncCancellation?.Dispose();
        _syncCancellation = null;
        _syncTask = null;
    }
    
    private async Task RunSyncLoop(CancellationToken cancellationToken)
    {
        // Sync interval (2 minutes default)
        var syncInterval = TimeSpan.FromSeconds(120);
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await PerformSync();
            }
            catch (Exception ex)
            {
                CitiesRegional.Logging.LogError($"Sync error: {ex.Message}");
            }
            
            try
            {
                await Task.Delay(syncInterval, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }
    
    private async Task PerformSync()
    {
        if (_currentRegion == null) return;
        
        CitiesRegional.Logging.LogDebug("Performing regional sync...");
        
        // 1. Collect current city data
        _localCityData = _dataCollector.CollectCurrentData();
        _localCityData.IsOnline = true;
        _localCityData.LastSync = DateTime.UtcNow;
        
        // 2. Push our data
        await _syncService.PushCityData(_localCityData);
        
        // 3. Pull others' data
        var cities = await _syncService.PullRegionData();
        _currentRegion.Cities = cities;
        
        // 4. Calculate trade flows with enhanced calculator
        var tradeCalculator = new TradeFlowCalculator();
        var tradeResult = tradeCalculator.CalculateTradeFlows(_currentRegion);
        
        // Validate trade flows
        var validationErrors = tradeCalculator.ValidateTradeFlows(tradeResult.Flows, _currentRegion);
        if (validationErrors.Count > 0)
        {
            CitiesRegional.Logging.LogWarn($"Trade flow validation found {validationErrors.Count} errors:");
            foreach (var error in validationErrors)
            {
                CitiesRegional.Logging.LogWarn($"  - {error}");
            }
        }
        
        // 5. Apply effects to our city
        _effectsApplicator.ApplyTradeEffects(tradeResult.Flows, _localCityData.CityId);
        _effectsApplicator.ApplyCommuterEffects(_currentRegion, _localCityData.CityId);
        
        _lastSyncTime = DateTime.UtcNow;
        OnRegionUpdated?.Invoke(_currentRegion);
        
        CitiesRegional.Logging.LogDebug($"Sync complete. {cities.Count} cities in region.");
    }
    
    /// <summary>
    /// Force an immediate sync
    /// </summary>
    public async Task ForceSyncNow()
    {
        if (!_isConnected) return;
        await PerformSync();
    }
    
    #endregion
    
    #region Connection Management
    
    /// <summary>
    /// Propose a new connection to another city
    /// </summary>
    public async Task<bool> ProposeConnection(string targetCityId, ConnectionType type)
    {
        if (_currentRegion == null || _localCityData == null)
            return false;
            
        var connection = new RegionalConnection
        {
            FromCityId = _localCityData.CityId,
            ToCityId = targetCityId,
            Type = type,
            Capacity = GetDefaultCapacity(type),
            TravelTimeMinutes = EstimateTravelTime(type)
        };
        
        return await _syncService.ProposeConnection(connection);
    }
    
    private int GetDefaultCapacity(ConnectionType type)
    {
        return type switch
        {
            ConnectionType.Highway2Lane => 1000,
            ConnectionType.Highway4Lane => 2500,
            ConnectionType.Highway6Lane => 5000,
            ConnectionType.RegionalRail => 2000,
            ConnectionType.HighSpeedRail => 5000,
            ConnectionType.CargoRail => 500,
            ConnectionType.Ferry => 300,
            ConnectionType.AirRoute => 1000,
            _ => 1000
        };
    }
    
    private float EstimateTravelTime(ConnectionType type)
    {
        return type switch
        {
            ConnectionType.Highway2Lane => 30,
            ConnectionType.Highway4Lane => 25,
            ConnectionType.Highway6Lane => 20,
            ConnectionType.RegionalRail => 20,
            ConnectionType.HighSpeedRail => 10,
            ConnectionType.CargoRail => 45,
            ConnectionType.Ferry => 60,
            ConnectionType.AirRoute => 30,
            _ => 30
        };
    }
    
    #endregion
    
    #region Shared Services
    
    /// <summary>
    /// Offer a local service to be shared with the region
    /// </summary>
    public async Task<bool> OfferSharedService(SharedServiceInfo service)
    {
        if (_localCityData == null) return false;
        
        service.ServiceId = Guid.NewGuid().ToString();
        _localCityData.HostedServices.Add(service);
        
        return await _syncService.OfferService(service);
    }
    
    /// <summary>
    /// Request access to a service hosted by another city
    /// </summary>
    public async Task<bool> RequestServiceAccess(string serviceId)
    {
        return await _syncService.RequestServiceAccess(serviceId);
    }
    
    #endregion
    
    #region Event Handling
    
    private void HandleRegionalEvent(RegionalEvent evt)
    {
        CitiesRegional.Logging.LogInfo($"Regional event: {evt.Title}");
        _currentRegion?.AddEvent(evt);
        OnRegionalEvent?.Invoke(evt);
    }
    
    #endregion
    
    #region Data Access
    
    public RegionalCityData[] GetAllCities() => 
        _currentRegion?.Cities.ToArray() ?? Array.Empty<RegionalCityData>();
    
    public RegionalCityData? GetCity(string cityId) => 
        _currentRegion?.GetCity(cityId);
    
    public RegionalConnection[] GetAllConnections() => 
        _currentRegion?.Connections.ToArray() ?? Array.Empty<RegionalConnection>();
    
    public RegionalEvent[] GetRecentEvents(int count = 20) => 
        _currentRegion?.RecentEvents.Take(count).ToArray() ?? Array.Empty<RegionalEvent>();
    
    public (RegionalCityData City, float Value)[] GetLeaderboard(
        Func<RegionalCityData, float> metric, bool descending = true)
    {
        if (_currentRegion == null)
            return Array.Empty<(RegionalCityData, float)>();
            
        var ordered = descending
            ? _currentRegion.Cities.OrderByDescending(metric)
            : _currentRegion.Cities.OrderBy(metric);
            
        return ordered.Select(c => (c, metric(c))).ToArray();
    }
    
    public void UpdateLocalData(RegionalCityData data) => _localCityData = data;
    
    #endregion
    
    public void Dispose()
    {
        StopSyncLoop();
        _syncService.OnEventReceived -= HandleRegionalEvent;
        
        if (_syncService is IDisposable disposable)
            disposable.Dispose();
    }
}
