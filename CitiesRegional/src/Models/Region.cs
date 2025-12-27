using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace CitiesRegional.Models;

/// <summary>
/// Represents a region containing multiple connected cities.
/// This is the top-level container for regional play.
/// </summary>
[Serializable]
public class Region
{
    #region Identity

    /// <summary>Unique identifier for this region</summary>
    [JsonProperty("regionId")]
    public string RegionId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Human-readable region name</summary>
    [JsonProperty("regionName")]
    public string RegionName { get; set; } = "New Region";

    /// <summary>Short code for joining (e.g., "METRO-7X4K")</summary>
    [JsonProperty("regionCode")]
    public string RegionCode { get; set; } = GenerateRegionCode();

    /// <summary>Player ID of the region creator/host</summary>
    [JsonProperty("hostPlayerId")]
    public string HostPlayerId { get; set; } = "";

    /// <summary>Maximum number of cities allowed</summary>
    [JsonProperty("maxCities")]
    public int MaxCities { get; set; } = 4;

    #endregion

    #region State

    /// <summary>All cities in the region</summary>
    [JsonProperty("cities")]
    public List<RegionalCityData> Cities { get; set; } = new();

    /// <summary>Connections between cities</summary>
    [JsonProperty("connections")]
    public List<RegionalConnection> Connections { get; set; } = new();

    /// <summary>Active trade agreements</summary>
    [JsonProperty("tradeAgreements")]
    public List<TradeAgreement> TradeAgreements { get; set; } = new();

    /// <summary>Recent events/milestones</summary>
    [JsonProperty("events")]
    public List<RegionalEvent> RecentEvents { get; set; } = new();

    #endregion

    #region Timestamps

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [JsonProperty("lastActivity")]
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    #endregion

    #region Computed Properties

    /// <summary>Total population across all cities</summary>
    [JsonIgnore]
    public int TotalPopulation => Cities.Sum(c => c.Population);

    /// <summary>Total GDP across all cities</summary>
    [JsonIgnore]
    public float TotalGDP => Cities.Sum(c => c.GDPEstimate);

    /// <summary>Number of cities currently online</summary>
    [JsonIgnore]
    public int OnlineCities => Cities.Count(c => c.IsOnline);

    /// <summary>Average happiness across all cities</summary>
    [JsonIgnore]
    public float AverageHappiness => Cities.Count > 0
        ? Cities.Average(c => c.Happiness)
        : 0;

    #endregion

    #region Methods

    /// <summary>
    /// Add a city to the region
    /// </summary>
    public bool AddCity(RegionalCityData city)
    {
        if (city == null)
            return false;
        
        if (string.IsNullOrEmpty(city.CityId))
            return false;
        
        if (Cities == null)
            return false;
        
        if (Cities.Count >= MaxCities)
            return false;

        if (Cities.Any(c => c.CityId == city.CityId))
            return false;

        Cities.Add(city);
        LastActivity = DateTime.UtcNow;

        AddEvent(new RegionalEvent
        {
            Type = RegionalEventType.CityJoined,
            Title = $"{city.CityName} joined the region!",
            Description = $"{city.PlayerName}'s city has joined {RegionName}.",
            SourceCityId = city.CityId
        });

        return true;
    }

    /// <summary>
    /// Remove a city from the region
    /// </summary>
    public bool RemoveCity(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
            return false;
        
        if (Cities == null || Connections == null)
            return false;
        
        var city = Cities.FirstOrDefault(c => c.CityId == cityId);
        if (city == null)
            return false;

        Cities.Remove(city);

        // Remove connections involving this city
        Connections.RemoveAll(c => c.FromCityId == cityId || c.ToCityId == cityId);

        AddEvent(new RegionalEvent
        {
            Type = RegionalEventType.CityLeft,
            Title = $"{city.CityName} left the region",
            Description = $"{city.PlayerName}'s city has left {RegionName}.",
            SourceCityId = cityId
        });

        LastActivity = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Update a city's data
    /// </summary>
    public void UpdateCity(RegionalCityData updatedCity)
    {
        if (updatedCity == null)
            return;
        
        if (string.IsNullOrEmpty(updatedCity.CityId))
            return;
        
        if (Cities == null)
            return;
        
        var existing = Cities.FirstOrDefault(c => c.CityId == updatedCity.CityId);
        if (existing != null)
        {
            var index = Cities.IndexOf(existing);
            Cities[index] = updatedCity;
        }
        else
        {
            Cities.Add(updatedCity);
        }

        LastActivity = DateTime.UtcNow;
    }

    /// <summary>
    /// Get a city by ID
    /// </summary>
    public RegionalCityData? GetCity(string cityId)
    {
        if (string.IsNullOrEmpty(cityId))
            return null;
        
        if (Cities == null)
            return null;
        
        return Cities.FirstOrDefault(c => c.CityId == cityId);
    }

    /// <summary>
    /// Add a connection between two cities
    /// </summary>
    public bool AddConnection(RegionalConnection connection)
    {
        if (connection == null)
            return false;
        
        if (string.IsNullOrEmpty(connection.FromCityId) || string.IsNullOrEmpty(connection.ToCityId))
            return false;
        
        if (Cities == null || Connections == null)
            return false;
        
        // Validate cities exist
        if (!Cities.Any(c => c.CityId == connection.FromCityId) ||
            !Cities.Any(c => c.CityId == connection.ToCityId))
            return false;

        // Check for duplicate
        if (Connections.Any(c =>
            (c.FromCityId == connection.FromCityId && c.ToCityId == connection.ToCityId) ||
            (c.FromCityId == connection.ToCityId && c.ToCityId == connection.FromCityId)))
            return false;

        Connections.Add(connection);

        var fromCity = GetCity(connection.FromCityId);
        var toCity = GetCity(connection.ToCityId);

        AddEvent(new RegionalEvent
        {
            Type = RegionalEventType.ConnectionBuilt,
            Title = $"New {connection.Type} connection!",
            Description = $"A new connection was built between {fromCity?.CityName} and {toCity?.CityName}.",
            SourceCityId = connection.FromCityId
        });

        return true;
    }

    /// <summary>
    /// Get connection between two cities (if exists)
    /// </summary>
    public RegionalConnection? GetConnection(string cityId1, string cityId2)
    {
        if (string.IsNullOrEmpty(cityId1) || string.IsNullOrEmpty(cityId2))
            return null;
        
        if (Connections == null)
            return null;
        
        return Connections.FirstOrDefault(c =>
            (c.FromCityId == cityId1 && c.ToCityId == cityId2) ||
            (c.FromCityId == cityId2 && c.ToCityId == cityId1));
    }

    /// <summary>
    /// Add an event to the region's history
    /// </summary>
    public void AddEvent(RegionalEvent evt)
    {
        evt.Timestamp = DateTime.UtcNow;
        evt.EventId = Guid.NewGuid().ToString();
        RecentEvents.Insert(0, evt);

        // Keep only last 100 events
        if (RecentEvents.Count > 100)
            RecentEvents.RemoveRange(100, RecentEvents.Count - 100);
    }

    /// <summary>
    /// Calculate trade between all cities
    /// </summary>
    /// <summary>
    /// Calculate optimal trade flows between cities, considering connections, capacity, and distance
    /// </summary>
    public List<TradeFlow> CalculateTradeFlows()
    {
        return CalculateTradeFlows(maxTravelTimeMinutes: 60f, capacityUtilizationLimit: 0.85f);
    }

    /// <summary>
    /// Calculate optimal trade flows with configurable constraints
    /// </summary>
    public List<TradeFlow> CalculateTradeFlows(float maxTravelTimeMinutes, float capacityUtilizationLimit = 0.85f)
    {
        var flows = new List<TradeFlow>();

        // Track connection usage for capacity constraints
        var connectionUsage = new Dictionary<string, float>(); // connectionId -> units used

        foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
        {
            // Find exporters and importers
            var exporters = Cities
                .Select(c => new { City = c, Resource = c.Resources.FirstOrDefault(r => r.Type == resourceType) })
                .Where(x => x.Resource != null && x.Resource.ExportAvailable > 0)
                .OrderByDescending(x => x.Resource!.ExportAvailable) // Prioritize larger exporters
                .ToList();

            var importers = Cities
                .Select(c => new { City = c, Resource = c.Resources.FirstOrDefault(r => r.Type == resourceType) })
                .Where(x => x.Resource != null && x.Resource.ImportNeeded > 0)
                .OrderByDescending(x => x.Resource!.ImportNeeded) // Prioritize larger importers
                .ToList();

            if (exporters.Count == 0 || importers.Count == 0)
                continue;

            // Create potential trade matches with connection info
            var potentialMatches = new List<TradeMatch>();

            foreach (var importer in importers)
            {
                foreach (var exporter in exporters)
                {
                    if (exporter.City.CityId == importer.City.CityId) continue;

                    // Check if connection exists
                    var connection = GetConnection(exporter.City.CityId, importer.City.CityId);
                    if (connection == null) continue;

                    // Check travel time limit
                    if (connection.TravelTimeMinutes > maxTravelTimeMinutes) continue;

                    // Calculate potential trade amount
                    var available = exporter.Resource!.ExportAvailable;
                    var needed = importer.Resource!.ImportNeeded;
                    var potentialAmount = Math.Min(available, needed);

                    if (potentialAmount > 0)
                    {
                        // Calculate available capacity on connection
                        var currentUsage = connectionUsage.GetValueOrDefault(connection.ConnectionId, 0f);
                        var availableCapacity = connection.Capacity * capacityUtilizationLimit - currentUsage;

                        // Limit by connection capacity (convert capacity to resource units - rough estimate: 1 capacity = 10 units)
                        var capacityLimit = availableCapacity * 10f; // Adjust multiplier based on game balance
                        var amount = Math.Min(potentialAmount, capacityLimit);

                        if (amount > 0)
                        {
                            potentialMatches.Add(new TradeMatch
                            {
                                Exporter = new ExporterInfo { City = exporter.City, Resource = exporter.Resource },
                                Importer = new ImporterInfo { City = importer.City, Resource = importer.Resource },
                                Connection = connection,
                                Amount = amount,
                                Priority = CalculateTradePriority(
                                    new ExporterInfo { City = exporter.City, Resource = exporter.Resource },
                                    new ImporterInfo { City = importer.City, Resource = importer.Resource },
                                    connection)
                            });
                        }
                    }
                }
            }

            // Sort by priority (higher priority = better match)
            potentialMatches = potentialMatches
                .OrderByDescending(m => m.Priority)
                .ToList();

            // Track remaining needs and available exports
            var remainingNeeds = importers.ToDictionary(
                i => i.City.CityId,
                i => i.Resource!.ImportNeeded
            );
            var remainingExports = exporters.ToDictionary(
                e => e.City.CityId,
                e => e.Resource!.ExportAvailable
            );

            // Process matches in priority order
            foreach (var match in potentialMatches)
            {
                var exporterId = match.Exporter.City.CityId;
                var importerId = match.Importer.City.CityId;
                var connectionId = match.Connection.ConnectionId;

                // Check if still needed and available
                if (remainingNeeds[importerId] <= 0 || remainingExports[exporterId] <= 0)
                    continue;

                // Check connection capacity
                var currentUsage = connectionUsage.GetValueOrDefault(connectionId, 0f);
                var availableCapacity = match.Connection.Capacity * capacityUtilizationLimit - currentUsage;
                var capacityLimit = availableCapacity * 10f;

                // Calculate actual trade amount
                var amount = Math.Min(
                    Math.Min(remainingNeeds[importerId], remainingExports[exporterId]),
                    capacityLimit
                );

                if (amount > 0)
                {
                    flows.Add(new TradeFlow
                    {
                        ResourceType = resourceType,
                        FromCityId = exporterId,
                        ToCityId = importerId,
                        Amount = amount,
                        PricePerUnit = match.Exporter.Resource.Price,
                        ConnectionId = connectionId,
                        TravelTimeMinutes = match.Connection.TravelTimeMinutes
                    });

                    // Update tracking
                    remainingNeeds[importerId] -= amount;
                    remainingExports[exporterId] -= amount;
                    connectionUsage[connectionId] = currentUsage + (amount / 10f); // Track capacity usage
                }
            }
        }

        return flows;
    }

    /// <summary>
    /// Calculate trade priority score (higher = better match)
    /// Factors: distance, connection quality, price, urgency
    /// </summary>
    private float CalculateTradePriority(
        ExporterInfo exporter,
        ImporterInfo importer,
        RegionalConnection connection)
    {
        float priority = 100f; // Base priority

        // Prefer shorter distances (lower travel time = higher priority)
        priority += (60f - connection.TravelTimeMinutes) * 2f;

        // Prefer higher capacity connections
        priority += connection.Capacity / 100f;

        // Prefer less congested connections
        if (connection.Capacity > 0)
        {
            var congestion = connection.UsagePercent / 100f;
            priority += (1f - congestion) * 20f;
        }

        // Prefer better prices (higher price = more revenue for exporter)
        priority += exporter.Resource.Price / 10f;

        // Prefer larger trade amounts (economies of scale)
        var tradeAmount = Math.Min(exporter.Resource.ExportAvailable, importer.Resource.ImportNeeded);
        priority += tradeAmount / 100f;

        return priority;
    }

    /// <summary>
    /// Helper class for trade matching
    /// </summary>
    private class TradeMatch
    {
        public ExporterInfo Exporter { get; set; } = null!;
        public ImporterInfo Importer { get; set; } = null!;
        public RegionalConnection Connection { get; set; } = null!;
        public float Amount { get; set; }
        public float Priority { get; set; }
    }

    /// <summary>
    /// Exporter information for trade matching
    /// </summary>
    private class ExporterInfo
    {
        public RegionalCityData City { get; set; } = null!;
        public ResourceData Resource { get; set; } = null!;
    }

    /// <summary>
    /// Importer information for trade matching
    /// </summary>
    private class ImporterInfo
    {
        public RegionalCityData City { get; set; } = null!;
        public ResourceData Resource { get; set; } = null!;
    }

    /// <summary>
    /// Generate a random region code
    /// </summary>
    private static string GenerateRegionCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var part1 = new string(Enumerable.Range(0, 4).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        var part2 = new string(Enumerable.Range(0, 4).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        return $"{part1}-{part2}";
    }

    #endregion
}

/// <summary>
/// A connection between two cities
/// </summary>
[Serializable]
public class RegionalConnection
{
    [JsonProperty("connectionId")]
    public string ConnectionId { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("fromCityId")]
    public string FromCityId { get; set; } = "";

    [JsonProperty("toCityId")]
    public string ToCityId { get; set; } = "";

    [JsonProperty("type")]
    public ConnectionType Type { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = "";

    /// <summary>Vehicles or passengers per hour</summary>
    [JsonProperty("capacity")]
    public int Capacity { get; set; }

    /// <summary>Current usage</summary>
    [JsonProperty("currentUsage")]
    public int CurrentUsage { get; set; }

    /// <summary>Travel time in minutes</summary>
    [JsonProperty("travelTimeMinutes")]
    public float TravelTimeMinutes { get; set; }

    /// <summary>Cost to upgrade</summary>
    [JsonProperty("upgradeCost")]
    public float UpgradeCost { get; set; }

    /// <summary>Get usage percentage</summary>
    [JsonIgnore]
    public float UsagePercent => Capacity > 0 ? (float)CurrentUsage / Capacity * 100 : 0;

    /// <summary>Is this connection congested?</summary>
    [JsonIgnore]
    public bool IsCongested => UsagePercent > 85;
}

/// <summary>
/// A trade agreement between cities
/// </summary>
[Serializable]
public class TradeAgreement
{
    [JsonProperty("agreementId")]
    public string AgreementId { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("fromCityId")]
    public string FromCityId { get; set; } = "";

    [JsonProperty("toCityId")]
    public string ToCityId { get; set; } = "";

    [JsonProperty("resourceType")]
    public ResourceType ResourceType { get; set; }

    [JsonProperty("amount")]
    public float Amount { get; set; }

    [JsonProperty("pricePerUnit")]
    public float PricePerUnit { get; set; }

    [JsonProperty("isActive")]
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Calculated trade flow between cities
/// </summary>
[Serializable]
public class TradeFlow
{
    [JsonProperty("resourceType")]
    public ResourceType ResourceType { get; set; }

    [JsonProperty("fromCityId")]
    public string FromCityId { get; set; } = "";

    [JsonProperty("toCityId")]
    public string ToCityId { get; set; } = "";

    [JsonProperty("amount")]
    public float Amount { get; set; }

    [JsonProperty("pricePerUnit")]
    public float PricePerUnit { get; set; }

    [JsonProperty("totalValue")]
    public float TotalValue => Amount * PricePerUnit;

    /// <summary>Connection used for this trade (if known)</summary>
    [JsonProperty("connectionId")]
    public string? ConnectionId { get; set; }

    /// <summary>Travel time in minutes</summary>
    [JsonProperty("travelTimeMinutes")]
    public float TravelTimeMinutes { get; set; }

    /// <summary>Timestamp when trade was calculated</summary>
    [JsonProperty("calculatedAt")]
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// An event that occurred in the region
/// </summary>
[Serializable]
public class RegionalEvent
{
    [JsonProperty("eventId")]
    public string EventId { get; set; } = "";

    [JsonProperty("type")]
    public RegionalEventType Type { get; set; }

    [JsonProperty("title")]
    public string Title { get; set; } = "";

    [JsonProperty("description")]
    public string Description { get; set; } = "";

    [JsonProperty("sourceCityId")]
    public string? SourceCityId { get; set; }

    [JsonProperty("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonProperty("data")]
    public Dictionary<string, object>? Data { get; set; }
}

/// <summary>
/// Types of regional events
/// </summary>
public enum RegionalEventType
{
    CityJoined,
    CityLeft,
    ConnectionBuilt,
    ConnectionUpgraded,
    MilestoneReached,
    TradeAgreementSigned,
    ServiceShared,
    DisasterOccurred,
    LeaderboardChanged,
    RegionalGoalCompleted
}

