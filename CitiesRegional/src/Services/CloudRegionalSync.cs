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

    // Event for future use when implementing event notifications
#pragma warning disable CS0067
    public event Action<RegionalEvent>? OnEventReceived;
#pragma warning restore CS0067

    public bool IsConnected => !string.IsNullOrEmpty(_regionId);

    // ---------------------------
    // Debug/Tracing helpers
    // ---------------------------
    private void LogHttpRequest(HttpRequestMessage request, string description)
    {
        var msg = $"HTTP {request.Method} {request.RequestUri} | Desc: {description}";
        if (request.Content != null)
        {
            try
            {
                var body = request.Content.ReadAsStringAsync().Result;
                msg += $" | Body: {body}";
            }
            catch { /* ignore sync read issues */ }
        }
        CitiesRegional.Logging.LogDebug(msg);
    }

    private void LogHttpResponse(HttpResponseMessage response, TimeSpan elapsed, string description)
    {
        var msg = $"HTTP {(int)response.StatusCode} ({response.StatusCode}) after {elapsed.TotalMilliseconds:F0} ms | Desc: {description}";
        CitiesRegional.Logging.LogDebug(msg);
    }

    public CloudRegionalSync()
    {
        _client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        // Default server URL - can be changed via config/env.
        // Our in-repo server prototype uses `/api` + `/regions` routes, so default to local dev.
        var envUrl = Environment.GetEnvironmentVariable("CITIESREGIONAL_BASE_URL");
        _baseUrl = string.IsNullOrWhiteSpace(envUrl)
            ? "http://localhost:5000/api"
            : envUrl.Trim();
        _baseUrl = _baseUrl.TrimEnd('/');
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
        if (string.IsNullOrWhiteSpace(regionCode))
        {
            CitiesRegional.Logging.LogWarning("Cannot connect: region code is null or empty");
            return false;
        }

        try
        {
            var requestUrl = $"{_baseUrl}/regions/code/{regionCode}";
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var response = await _client.GetAsync(requestUrl);
            sw.Stop();
            LogHttpResponse(response, sw.Elapsed, "ConnectToRegion GET");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                CitiesRegional.Logging.LogWarning($"Region not found: {regionCode} (HTTP {response.StatusCode})");
                if (!string.IsNullOrEmpty(errorContent))
                {
                    CitiesRegional.Logging.LogDebug($"Server response: {errorContent}");
                }
                return false;
            }

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json))
            {
                CitiesRegional.Logging.LogWarning($"Empty response when connecting to region: {regionCode}");
                return false;
            }

            var region = JsonConvert.DeserializeObject<Region>(json);

            if (region == null)
            {
                CitiesRegional.Logging.LogWarning($"Failed to deserialize region data for code: {regionCode}");
                return false;
            }

            if (string.IsNullOrEmpty(region.RegionId))
            {
                CitiesRegional.Logging.LogWarning($"Region has no ID: {regionCode}");
                return false;
            }

            _regionId = region.RegionId;

            CitiesRegional.Logging.LogInfo($"Connected to region: {region.RegionName} ({region.RegionCode})");
            return true;
        }
        catch (HttpRequestException ex)
        {
            CitiesRegional.Logging.LogError($"Network error connecting to region {regionCode}: {ex.Message}");
            return false;
        }
        catch (JsonException ex)
        {
            CitiesRegional.Logging.LogError($"JSON parsing error connecting to region {regionCode}: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            CitiesRegional.Logging.LogError($"Connection failed for region {regionCode}: {ex.Message}");
            return false;
        }
    }

    public async Task<Region> CreateRegion(string name, int maxCities)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Region name cannot be null or empty", nameof(name));
        }

        if (maxCities < 2 || maxCities > 20)
        {
            throw new ArgumentException("Max cities must be between 2 and 20", nameof(maxCities));
        }

        try
        {
            var request = new
            {
                name = name.Trim(),
                maxCities = maxCities
            };

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($"{_baseUrl}/regions", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = $"Failed to create region (HTTP {response.StatusCode})";
                if (!string.IsNullOrEmpty(errorContent))
                {
                    errorMsg += $": {errorContent}";
                }
                throw new HttpRequestException(errorMsg);
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(responseJson))
            {
                throw new InvalidOperationException("Server returned empty response when creating region");
            }

            var region = JsonConvert.DeserializeObject<Region>(responseJson);
            if (region == null)
            {
                throw new InvalidOperationException("Failed to deserialize region from server response");
            }

            if (string.IsNullOrEmpty(region.RegionId))
            {
                throw new InvalidOperationException("Created region has no ID");
            }

            _regionId = region.RegionId;

            CitiesRegional.Logging.LogInfo($"Created region: {region.RegionName} ({region.RegionCode})");
            return region;
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HTTP exceptions
        }
        catch (JsonException ex)
        {
            CitiesRegional.Logging.LogError($"JSON parsing error creating region: {ex.Message}");
            throw new InvalidOperationException("Failed to parse server response", ex);
        }
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

        if (cityData == null)
            throw new ArgumentNullException(nameof(cityData));

        if (string.IsNullOrEmpty(cityData.CityId))
            throw new ArgumentException("City ID cannot be null or empty", nameof(cityData));

        try
        {
            _cityId = cityData.CityId;

            var json = JsonConvert.SerializeObject(cityData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PutAsync(
                $"{_baseUrl}/regions/{_regionId}/cities/{cityData.CityId}",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = $"Failed to push city data (HTTP {response.StatusCode})";
                if (!string.IsNullOrEmpty(errorContent))
                {
                    errorMsg += $": {errorContent}";
                }
                throw new HttpRequestException(errorMsg);
            }

            CitiesRegional.Logging.LogDebug($"Pushed city data: {cityData.CityName} ({cityData.CityId})");
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HTTP exceptions
        }
        catch (JsonException ex)
        {
            CitiesRegional.Logging.LogError($"JSON serialization error pushing city data: {ex.Message}");
            throw new InvalidOperationException("Failed to serialize city data", ex);
        }
    }

    public async Task<List<RegionalCityData>> PullRegionData()
    {
        if (_regionId == null)
            throw new InvalidOperationException("Not connected to a region");

        try
        {
            var response = await _client.GetAsync($"{_baseUrl}/regions/{_regionId}/cities");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorMsg = $"Failed to pull region data (HTTP {response.StatusCode})";
                if (!string.IsNullOrEmpty(errorContent))
                {
                    errorMsg += $": {errorContent}";
                }
                throw new HttpRequestException(errorMsg);
            }

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(json))
            {
                CitiesRegional.Logging.LogWarning("Server returned empty response when pulling region data");
                return new List<RegionalCityData>();
            }

            var cities = JsonConvert.DeserializeObject<List<RegionalCityData>>(json);
            if (cities == null)
            {
                CitiesRegional.Logging.LogWarning("Failed to deserialize region data, returning empty list");
                return new List<RegionalCityData>();
            }

            CitiesRegional.Logging.LogDebug($"Pulled {cities.Count} cities from region");
            return cities;
        }
        catch (HttpRequestException)
        {
            throw; // Re-throw HTTP exceptions
        }
        catch (JsonException ex)
        {
            CitiesRegional.Logging.LogError($"JSON parsing error pulling region data: {ex.Message}");
            throw new InvalidOperationException("Failed to parse region data from server", ex);
        }
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

