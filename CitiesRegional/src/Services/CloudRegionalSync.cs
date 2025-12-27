using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CitiesRegional.Models;

namespace CitiesRegional.Services;

/// <summary>
/// Cloud-based regional sync using a REST API.
/// This is the simplest implementation - just HTTP calls to a server.
/// </summary>
public class CloudRegionalSync : IRegionalSync, IDisposable
{
    private readonly HttpClient _client;
    private string _baseUrl;
    private string? _regionId;
    private string? _cityId;
    private string? _authToken;
    
    public event Action<RegionalEvent>? OnEventReceived;
    
    public bool IsConnected => !string.IsNullOrEmpty(_regionId);
    
    public CloudRegionalSync()
    {
        _client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        
        // Default server URL - can be changed via config
        _baseUrl = "https://api.citiesregional.com";
    }
    
    /// <summary>
    /// Set custom server URL (for self-hosted servers)
    /// </summary>
    public void SetServerUrl(string url)
    {
        _baseUrl = url.TrimEnd('/');
    }
    
    /// <summary>
    /// Set authentication token
    /// </summary>
    public void SetAuthToken(string token)
    {
        _authToken = token;
        _client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }
    
    #region Connection
    
    public async Task<bool> ConnectToRegion(string regionCode)
    {
        try
        {
            var response = await _client.GetAsync($"{_baseUrl}/regions/code/{regionCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                CitiesRegional.Logging.LogWarning($"Region not found: {regionCode}");
                return false;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            var region = JsonConvert.DeserializeObject<Region>(json);
            
            if (region == null)
                return false;
                
            _regionId = region.RegionId;
            
            CitiesRegional.Logging.LogInfo($"Connected to region: {region.RegionName}");
            return true;
        }
        catch (Exception ex)
        {
            CitiesRegional.Logging.LogError($"Connection failed: {ex.Message}");
            return false;
        }
    }
    
    public async Task<Region> CreateRegion(string name, int maxCities)
    {
        var request = new
        {
            name = name,
            maxCities = maxCities
        };
        
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync($"{_baseUrl}/regions", content);
        response.EnsureSuccessStatusCode();
        
        var responseJson = await response.Content.ReadAsStringAsync();
        var region = JsonConvert.DeserializeObject<Region>(responseJson)!;
        
        _regionId = region.RegionId;
        
        return region;
    }
    
    public async Task LeaveRegion()
    {
        if (_regionId == null || _cityId == null)
            return;
            
        try
        {
            await _client.DeleteAsync($"{_baseUrl}/regions/{_regionId}/cities/{_cityId}");
        }
        catch (Exception ex)
        {
            CitiesRegional.Logging.LogWarning($"Leave region failed: {ex.Message}");
        }
        
        _regionId = null;
        _cityId = null;
    }
    
    #endregion
    
    #region Data Sync
    
    public async Task PushCityData(RegionalCityData cityData)
    {
        if (_regionId == null)
            throw new InvalidOperationException("Not connected to a region");
            
        _cityId = cityData.CityId;
        
        var json = JsonConvert.SerializeObject(cityData);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PutAsync(
            $"{_baseUrl}/regions/{_regionId}/cities/{cityData.CityId}", 
            content
        );
        
        response.EnsureSuccessStatusCode();
        
        CitiesRegional.Logging.LogDebug($"Pushed city data: {cityData.CityName}");
    }
    
    public async Task<List<RegionalCityData>> PullRegionData()
    {
        if (_regionId == null)
            throw new InvalidOperationException("Not connected to a region");
            
        var response = await _client.GetAsync($"{_baseUrl}/regions/{_regionId}/cities");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var cities = JsonConvert.DeserializeObject<List<RegionalCityData>>(json) 
            ?? new List<RegionalCityData>();
            
        CitiesRegional.Logging.LogDebug($"Pulled {cities.Count} cities from region");
        return cities;
    }
    
    #endregion
    
    #region Connections
    
    public async Task<bool> ProposeConnection(RegionalConnection connection)
    {
        if (_regionId == null)
            return false;
            
        var json = JsonConvert.SerializeObject(connection);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync(
            $"{_baseUrl}/regions/{_regionId}/connections", 
            content
        );
        
        return response.IsSuccessStatusCode;
    }
    
    public async Task<bool> AcceptConnection(string connectionId)
    {
        if (_regionId == null)
            return false;
            
        var response = await _client.PostAsync(
            $"{_baseUrl}/regions/{_regionId}/connections/{connectionId}/accept",
            null
        );
        
        return response.IsSuccessStatusCode;
    }
    
    public async Task<bool> UpgradeConnection(string connectionId, ConnectionType newType)
    {
        if (_regionId == null)
            return false;
            
        var request = new { type = newType.ToString() };
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PatchAsync(
            $"{_baseUrl}/regions/{_regionId}/connections/{connectionId}",
            content
        );
        
        return response.IsSuccessStatusCode;
    }
    
    #endregion
    
    #region Shared Services
    
    public async Task<bool> OfferService(SharedServiceInfo service)
    {
        if (_regionId == null)
            return false;
            
        var json = JsonConvert.SerializeObject(service);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync(
            $"{_baseUrl}/regions/{_regionId}/services",
            content
        );
        
        return response.IsSuccessStatusCode;
    }
    
    public async Task<bool> RequestServiceAccess(string serviceId)
    {
        if (_regionId == null || _cityId == null)
            return false;
            
        var request = new { cityId = _cityId };
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync(
            $"{_baseUrl}/regions/{_regionId}/services/{serviceId}/access",
            content
        );
        
        return response.IsSuccessStatusCode;
    }
    
    public async Task UpdateServiceUsage(string serviceId, int usage)
    {
        if (_regionId == null || _cityId == null)
            return;
            
        var request = new { cityId = _cityId, usage = usage };
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        await _client.PatchAsync(
            $"{_baseUrl}/regions/{_regionId}/services/{serviceId}/usage",
            content
        );
    }
    
    #endregion
    
    #region Events
    
    public async Task BroadcastEvent(RegionalEvent evt)
    {
        if (_regionId == null)
            return;
            
        var json = JsonConvert.SerializeObject(evt);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        await _client.PostAsync(
            $"{_baseUrl}/regions/{_regionId}/events",
            content
        );
    }
    
    // Note: In a production implementation, consider using WebSockets or 
    // long-polling to receive real-time events.
    // For simplicity, events are pulled during the regular sync cycle.
    
    public async Task<List<RegionalEvent>> PullEvents(DateTime since)
    {
        if (_regionId == null)
            return new List<RegionalEvent>();
            
        var response = await _client.GetAsync(
            $"{_baseUrl}/regions/{_regionId}/events?since={since:O}"
        );
        
        if (!response.IsSuccessStatusCode)
            return new List<RegionalEvent>();
            
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<RegionalEvent>>(json) 
            ?? new List<RegionalEvent>();
    }
    
    #endregion
    
    public void Dispose()
    {
        _client.Dispose();
    }
}

