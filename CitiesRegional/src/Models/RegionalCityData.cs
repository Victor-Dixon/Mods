using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CitiesRegional.Models;

/// <summary>
/// Represents the aggregated data of a city in the region.
/// This is the core data structure that gets synced between cities.
/// Only contains high-level metrics, NOT individual entities.
/// </summary>
[Serializable]
public class RegionalCityData
{
    #region Identity
    
    /// <summary>Unique identifier for this city</summary>
    [JsonProperty("cityId")]
    public string CityId { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>Display name of the city</summary>
    [JsonProperty("cityName")]
    public string CityName { get; set; } = "Unnamed City";
    
    /// <summary>Player who owns this city</summary>
    [JsonProperty("playerName")]
    public string PlayerName { get; set; } = "Unknown";
    
    /// <summary>Unique player identifier</summary>
    [JsonProperty("playerId")]
    public string PlayerId { get; set; } = "";
    
    #endregion
    
    #region Status
    
    /// <summary>Is the player currently online?</summary>
    [JsonProperty("isOnline")]
    public bool IsOnline { get; set; }
    
    /// <summary>Last time player was seen online</summary>
    [JsonProperty("lastSeen")]
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    
    /// <summary>Last time this data was synced</summary>
    [JsonProperty("lastSync")]
    public DateTime LastSync { get; set; } = DateTime.UtcNow;
    
    /// <summary>Current in-game date</summary>
    [JsonProperty("gameDate")]
    public string GameDate { get; set; } = "";
    
    #endregion
    
    #region Population
    
    /// <summary>Total city population</summary>
    [JsonProperty("population")]
    public int Population { get; set; }
    
    /// <summary>Number of employed workers</summary>
    [JsonProperty("workers")]
    public int Workers { get; set; }
    
    /// <summary>Number of unemployed workers available</summary>
    [JsonProperty("unemployedWorkers")]
    public int UnemployedWorkers { get; set; }
    
    /// <summary>Number of unfilled job positions</summary>
    [JsonProperty("availableJobs")]
    public int AvailableJobs { get; set; }
    
    /// <summary>Number of students</summary>
    [JsonProperty("students")]
    public int Students { get; set; }
    
    /// <summary>Number of tourists currently visiting</summary>
    [JsonProperty("tourists")]
    public int Tourists { get; set; }
    
    #endregion
    
    #region Economy
    
    /// <summary>City treasury balance</summary>
    [JsonProperty("treasury")]
    public long Treasury { get; set; }
    
    /// <summary>Weekly income</summary>
    [JsonProperty("weeklyIncome")]
    public float WeeklyIncome { get; set; }
    
    /// <summary>Weekly expenses</summary>
    [JsonProperty("weeklyExpenses")]
    public float WeeklyExpenses { get; set; }
    
    /// <summary>Estimated GDP</summary>
    [JsonProperty("gdpEstimate")]
    public float GDPEstimate { get; set; }
    
    #endregion
    
    #region Resources
    
    /// <summary>Resource production and consumption data</summary>
    [JsonProperty("resources")]
    public List<ResourceData> Resources { get; set; } = new();
    
    #endregion
    
    #region City Metrics
    
    /// <summary>Overall citizen happiness (0-100)</summary>
    [JsonProperty("happiness")]
    public float Happiness { get; set; }
    
    /// <summary>Overall health rating (0-100)</summary>
    [JsonProperty("health")]
    public float Health { get; set; }
    
    /// <summary>Education level (0-100)</summary>
    [JsonProperty("education")]
    public float Education { get; set; }
    
    /// <summary>Traffic flow efficiency (0-100)</summary>
    [JsonProperty("trafficFlow")]
    public float TrafficFlow { get; set; }
    
    /// <summary>Average land value</summary>
    [JsonProperty("landValueAvg")]
    public float LandValueAvg { get; set; }
    
    /// <summary>Pollution level (0-100, lower is better)</summary>
    [JsonProperty("pollution")]
    public float Pollution { get; set; }
    
    /// <summary>Crime rate (0-100, lower is better)</summary>
    [JsonProperty("crimeRate")]
    public float CrimeRate { get; set; }
    
    #endregion
    
    #region Regional Capabilities
    
    /// <summary>Services this city hosts for the region</summary>
    [JsonProperty("hostedServices")]
    public List<SharedServiceInfo> HostedServices { get; set; } = new();
    
    /// <summary>Regional services this city uses</summary>
    [JsonProperty("usedServices")]
    public List<SharedServiceUsage> UsedServices { get; set; } = new();
    
    /// <summary>Connection points available (highway exits, rail stations)</summary>
    [JsonProperty("connectionPoints")]
    public List<ConnectionPoint> ConnectionPoints { get; set; } = new();
    
    #endregion
    
    #region Methods
    
    /// <summary>
    /// Calculate the net trade balance for a specific resource
    /// </summary>
    public float GetNetTradeBalance(ResourceType resourceType)
    {
        if (Resources == null)
            return 0;
        
        var resource = Resources.Find(r => r.Type == resourceType);
        if (resource == null) return 0;
        
        return resource.ExportAvailable - resource.ImportNeeded;
    }
    
    /// <summary>
    /// Check if this city can supply workers to another city
    /// </summary>
    public int GetAvailableWorkersForCommute(int maxCommuteMinutes)
    {
        // In real implementation, would check commute feasibility
        return Math.Min(UnemployedWorkers, UnemployedWorkers / 2); // Export max 50% of unemployed
    }
    
    /// <summary>
    /// Create a snapshot copy of this data
    /// </summary>
    public RegionalCityData Clone()
    {
        var json = JsonConvert.SerializeObject(this);
        return JsonConvert.DeserializeObject<RegionalCityData>(json)!;
    }
    
    #endregion
}

/// <summary>
/// Resource production/consumption data
/// </summary>
[Serializable]
public class ResourceData
{
    [JsonProperty("type")]
    public ResourceType Type { get; set; }
    
    /// <summary>Units produced per week</summary>
    [JsonProperty("production")]
    public float Production { get; set; }
    
    /// <summary>Units consumed per week</summary>
    [JsonProperty("consumption")]
    public float Consumption { get; set; }
    
    /// <summary>Units available for export</summary>
    [JsonProperty("exportAvailable")]
    public float ExportAvailable
    {
        get => _exportAvailable ?? Math.Max(0f, Production - Consumption);
        set => _exportAvailable = Math.Max(0f, value);
    }
    
    /// <summary>Units needed from imports</summary>
    [JsonProperty("importNeeded")]
    public float ImportNeeded
    {
        get => _importNeeded ?? Math.Max(0f, Consumption - Production);
        set => _importNeeded = Math.Max(0f, value);
    }
    
    /// <summary>Price per unit</summary>
    [JsonProperty("price")]
    public float Price { get; set; }
    
    /// <summary>Current stockpile</summary>
    [JsonProperty("stockpile")]
    public float Stockpile { get; set; }

    // Backing fields allow explicit values from game data / sync to override the computed defaults.
    // If unset, Export/Import are derived from Production/Consumption for consistency and testability.
    [JsonIgnore]
    private float? _exportAvailable;

    [JsonIgnore]
    private float? _importNeeded;
}

/// <summary>
/// Types of resources that can be traded between cities
/// </summary>
public enum ResourceType
{
    IndustrialGoods,
    CommercialGoods,
    Agriculture,
    Electricity,
    Water,
    Oil,
    Ore,
    RawMaterials,
    Forestry,
    Tourists,      // Special: tourism flow
    Students,      // Special: education capacity
    Workers,       // Special: labor force
    Waste          // Special: waste processing
}

/// <summary>
/// Information about a shared service hosted by this city
/// </summary>
[Serializable]
public class SharedServiceInfo
{
    [JsonProperty("serviceId")]
    public string ServiceId { get; set; } = "";
    
    [JsonProperty("type")]
    public SharedServiceType Type { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; } = "";
    
    [JsonProperty("capacity")]
    public int Capacity { get; set; }
    
    [JsonProperty("availableCapacity")]
    public int AvailableCapacity { get; set; }
    
    [JsonProperty("costPerUnit")]
    public float CostPerUnit { get; set; }
}

/// <summary>
/// Types of services that can be shared regionally
/// </summary>
public enum SharedServiceType
{
    Airport,
    University,
    Stadium,
    Hospital,
    MedicalCenter,
    PowerPlant,
    WaterTreatment,
    Landfill,
    RecyclingCenter,
    ConventionCenter,
    SpaceCenter,
    Harbor
}

/// <summary>
/// Usage of a shared service by this city
/// </summary>
[Serializable]
public class SharedServiceUsage
{
    [JsonProperty("serviceId")]
    public string ServiceId { get; set; } = "";
    
    [JsonProperty("hostCityId")]
    public string HostCityId { get; set; } = "";
    
    [JsonProperty("type")]
    public SharedServiceType Type { get; set; }
    
    [JsonProperty("unitsUsed")]
    public int UnitsUsed { get; set; }
    
    [JsonProperty("weeklyCost")]
    public float WeeklyCost { get; set; }
}

/// <summary>
/// A connection point where highways/rail can connect to other cities
/// </summary>
[Serializable]
public class ConnectionPoint
{
    [JsonProperty("pointId")]
    public string PointId { get; set; } = "";
    
    [JsonProperty("type")]
    public ConnectionType Type { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; } = "";
    
    [JsonProperty("capacity")]
    public int Capacity { get; set; }
    
    [JsonProperty("isConnected")]
    public bool IsConnected { get; set; }
    
    [JsonProperty("connectedToCityId")]
    public string? ConnectedToCityId { get; set; }
}

/// <summary>
/// Types of connections between cities
/// </summary>
public enum ConnectionType
{
    Highway2Lane,
    Highway4Lane,
    Highway6Lane,
    RegionalRail,
    HighSpeedRail,
    CargoRail,
    Ferry,
    AirRoute
}

