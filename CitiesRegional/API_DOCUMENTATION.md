# API Documentation - Cities Regional Mod

**Version:** 0.1.0  
**Last Updated:** 2025-12-27  
**Target Audience:** Developers extending or integrating with Cities Regional

---

## 📚 Overview

This document describes the public APIs available in the Cities Regional mod for developers who want to:
- Extend the mod functionality
- Integrate with other mods
- Understand the internal architecture
- Build custom UI components

---

## 🏗️ Architecture Overview

```
CitiesRegionalPlugin (BepInEx Entry Point)
    └── RegionalManager (Central Orchestrator)
        ├── IRegionalSync (Sync Interface)
        │   └── CloudRegionalSync (HTTP Implementation)
        ├── CityDataCollector (Data Collection)
        └── RegionalEffectsApplicator (Effect Application)
```

---

## 🔌 Core APIs

### RegionalManager

**Namespace:** `CitiesRegional.Services`  
**Purpose:** Central orchestrator for all regional functionality

#### Properties

```csharp
public bool IsConnected { get; }
```
- Returns `true` if connected to a region
- Read-only property

```csharp
public Region? CurrentRegion { get; }
```
- Returns the current region if connected, `null` otherwise
- Read-only property

```csharp
public RegionalCityData? LocalCityData { get; }
```
- Returns local city data snapshot
- Read-only property

```csharp
public DateTime LastSyncTime { get; }
```
- Returns timestamp of last successful sync
- Read-only property

#### Events

```csharp
public event Action<Region>? OnRegionUpdated;
```
- Fired when region data is updated from sync
- Subscribe to update UI when region changes

```csharp
public event Action<bool>? OnConnectionStatusChanged;
```
- Fired when connection status changes (connected/disconnected)
- Parameter: `true` = connected, `false` = disconnected

```csharp
public event Action<RegionalEvent>? OnRegionalEvent;
```
- Fired when regional events occur (trade, commuter, etc.)
- Subscribe to show notifications

#### Methods

```csharp
public async Task<Region> CreateRegion(string regionName, int maxCities = 4)
```
- Creates a new region and becomes the host
- **Parameters:**
  - `regionName`: Display name for the region
  - `maxCities`: Maximum cities allowed (default: 4)
- **Returns:** Created `Region` object
- **Throws:** `Exception` if creation fails

```csharp
public async Task<bool> JoinRegion(string regionCode)
```
- Joins an existing region using region code
- **Parameters:**
  - `regionCode`: Unique region code (e.g., "ABC123")
- **Returns:** `true` if join successful, `false` otherwise
- **Throws:** `Exception` if join fails

```csharp
public async Task LeaveRegion()
```
- Leaves the current region
- **Returns:** `Task` (async operation)
- **Throws:** `InvalidOperationException` if not in a region

```csharp
public RegionalManager GetRegionalManager()
```
- Gets the RegionalManager instance from the main plugin
- **Returns:** `RegionalManager` instance
- **Throws:** `InvalidOperationException` if not initialized

#### Data Access Methods

```csharp
public RegionalCityData[] GetAllCities()
```
- Gets all cities in the current region
- **Returns:** Array of `RegionalCityData`
- **Returns:** Empty array if not connected

```csharp
public RegionalCityData? GetCity(string cityId)
```
- Gets a specific city by ID
- **Parameters:**
  - `cityId`: Unique city identifier
- **Returns:** `RegionalCityData` or `null` if not found

```csharp
public RegionalConnection[] GetAllConnections()
```
- Gets all connections in the current region
- **Returns:** Array of `RegionalConnection`
- **Returns:** Empty array if not connected

```csharp
public RegionalEvent[] GetRecentEvents(int count = 20)
```
- Gets recent regional events
- **Parameters:**
  - `count`: Number of events to retrieve (default: 20)
- **Returns:** Array of `RegionalEvent`

```csharp
public (RegionalCityData City, float Value)[] GetLeaderboard(
    Func<RegionalCityData, float> metric, 
    bool descending = true)
```
- Gets leaderboard sorted by a metric
- **Parameters:**
  - `metric`: Function to extract metric value from city data
  - `descending`: Sort order (default: true = highest first)
- **Returns:** Array of tuples (city, metric value)
- **Example:**
  ```csharp
  var popLeaderboard = manager.GetLeaderboard(c => c.Population);
  var happinessLeaderboard = manager.GetLeaderboard(c => c.Happiness, false);
  ```

---

### IRegionalSync

**Namespace:** `CitiesRegional.Services`  
**Purpose:** Interface for regional data synchronization

#### Methods

```csharp
Task<Region> CreateRegion(string regionName, int maxCities);
```
- Creates a new region on the sync service
- **Parameters:**
  - `regionName`: Display name
  - `maxCities`: Maximum cities
- **Returns:** Created `Region`

```csharp
Task<Region?> GetRegion(string regionCode);
```
- Retrieves region by code
- **Parameters:**
  - `regionCode`: Unique region code
- **Returns:** `Region` or `null` if not found

```csharp
Task<bool> PushCityData(RegionalCityData data);
```
- Pushes local city data to sync service
- **Parameters:**
  - `data`: City data to sync
- **Returns:** `true` if successful

```csharp
Task<Region> PullRegionData(string regionCode);
```
- Pulls latest region data from sync service
- **Parameters:**
  - `regionCode`: Region code
- **Returns:** Updated `Region`

```csharp
Task<bool> AddConnection(RegionalConnection connection);
```
- Adds a connection to the region
- **Parameters:**
  - `connection`: Connection to add
- **Returns:** `true` if successful

#### Events

```csharp
event Action<RegionalEvent>? OnEventReceived;
```
- Fired when regional events are received from sync service

---

### RegionalCityData

**Namespace:** `CitiesRegional.Models`  
**Purpose:** Represents aggregated city data that gets synced

#### Identity Properties

```csharp
public string CityId { get; set; }
```
- Unique identifier for the city (GUID)

```csharp
public string CityName { get; set; }
```
- Display name of the city

```csharp
public string PlayerName { get; set; }
```
- Name of the player who owns the city

```csharp
public string PlayerId { get; set; }
```
- Unique player identifier

#### Status Properties

```csharp
public bool IsOnline { get; set; }
```
- Whether the player is currently online

```csharp
public DateTime LastSeen { get; set; }
```
- Last time player was seen online

```csharp
public DateTime LastSync { get; set; }
```
- Last time this data was synced

```csharp
public string GameDate { get; set; }
```
- Current in-game date

#### Population Properties

```csharp
public int Population { get; set; }
```
- Total city population

```csharp
public int Workers { get; set; }
```
- Number of employed workers

```csharp
public int UnemployedWorkers { get; set; }
```
- Number of unemployed workers available

```csharp
public int AvailableJobs { get; set; }
```
- Number of available jobs

#### Economy Properties

```csharp
public decimal Treasury { get; set; }
```
- Current city treasury balance

```csharp
public decimal WeeklyIncome { get; set; }
```
- Weekly income

```csharp
public decimal WeeklyExpenses { get; set; }
```
- Weekly expenses

```csharp
public decimal GDPEstimate { get; set; }
```
- Estimated GDP

#### Metrics Properties

```csharp
public float Happiness { get; set; }
```
- City happiness (0-100)

```csharp
public float Health { get; set; }
```
- City health (0-100)

```csharp
public float Education { get; set; }
```
- City education level (0-100)

```csharp
public float TrafficFlow { get; set; }
```
- Traffic flow percentage (0-100)

```csharp
public float Pollution { get; set; }
```
- Pollution level (0-100)

```csharp
public float CrimeRate { get; set; }
```
- Crime rate (0-100)

#### Resources

```csharp
public List<ResourceData> Resources { get; set; }
```
- List of resource production/consumption data

---

### Region

**Namespace:** `CitiesRegional.Models`  
**Purpose:** Container for a regional group of cities

#### Properties

```csharp
public string RegionName { get; set; }
```
- Display name of the region

```csharp
public string RegionCode { get; }
```
- Unique region code (read-only, auto-generated)

```csharp
public int MaxCities { get; set; }
```
- Maximum cities allowed in region

```csharp
public List<RegionalCityData> Cities { get; }
```
- List of cities in the region (read-only)

```csharp
public List<RegionalConnection> Connections { get; }
```
- List of connections between cities (read-only)

#### Methods

```csharp
public void AddCity(RegionalCityData city)
```
- Adds a city to the region

```csharp
public RegionalCityData? GetCity(string cityId)
```
- Gets a city by ID

```csharp
public void AddConnection(RegionalConnection connection)
```
- Adds a connection to the region

```csharp
public List<TradeFlow> CalculateTradeFlows()
```
- Calculates trade flows between cities
- **Returns:** List of `TradeFlow` objects

#### Statistics Properties

```csharp
public int TotalPopulation { get; }
```
- Sum of all city populations

```csharp
public decimal TotalGDP { get; }
```
- Sum of all city GDPs

```csharp
public float AverageHappiness { get; }
```
- Average happiness across all cities

```csharp
public int OnlineCities { get; }
```
- Count of currently online cities

---

## 🎨 UI Integration APIs

### CitiesRegionalUI

**Namespace:** `CitiesRegional.UI`  
**Purpose:** UI helper class providing data access methods

#### Methods

```csharp
public void Initialize(RegionalManager regionalManager)
```
- Initializes the UI helper with RegionalManager
- **Parameters:**
  - `regionalManager`: RegionalManager instance

```csharp
public TradeFlowStatistics? GetTradeStatistics()
```
- Gets current trade statistics for UI display
- **Returns:** `TradeFlowStatistics` or `null` if not available

```csharp
public Region? GetCurrentRegion()
```
- Gets the current region
- **Returns:** `Region` or `null` if not connected

```csharp
public RegionalCityData[] GetCities()
```
- Gets all cities in current region
- **Returns:** Array of `RegionalCityData`

---

## 🔧 Extension Points

### Custom Sync Implementation

Implement `IRegionalSync` to create custom sync backends:

```csharp
public class CustomSyncService : IRegionalSync
{
    public event Action<RegionalEvent>? OnEventReceived;
    
    public Task<Region> CreateRegion(string regionName, int maxCities)
    {
        // Your implementation
    }
    
    // Implement other interface methods...
}
```

### Custom Data Collection

Extend `CityDataCollector` to collect additional data:

```csharp
public class ExtendedDataCollector : CityDataCollector
{
    protected override RegionalCityData CollectData()
    {
        var data = base.CollectData();
        // Add custom data collection
        return data;
    }
}
```

### Event Handlers

Subscribe to RegionalManager events:

```csharp
var manager = CitiesRegionalPlugin.Instance.GetRegionalManager();
manager.OnRegionUpdated += (region) => {
    // Update UI when region changes
};
manager.OnRegionalEvent += (evt) => {
    // Show notification for regional events
};
```

---

## 📝 Code Examples

### Example 1: Get Leaderboard

```csharp
var manager = CitiesRegionalPlugin.Instance.GetRegionalManager();
var leaderboard = manager.GetLeaderboard(c => c.Population);

foreach (var (city, population) in leaderboard)
{
    Console.WriteLine($"{city.CityName}: {population:N0}");
}
```

### Example 2: Monitor Trade Statistics

```csharp
var manager = CitiesRegionalPlugin.Instance.GetRegionalManager();
var ui = new CitiesRegionalUI();
ui.Initialize(manager);

var stats = ui.GetTradeStatistics();
if (stats != null)
{
    Console.WriteLine($"Total Trade Value: {stats.TotalTradeValue:C}");
    Console.WriteLine($"Active Trades: {stats.ActiveTradesCount}");
}
```

### Example 3: Create Region

```csharp
var manager = CitiesRegionalPlugin.Instance.GetRegionalManager();

try
{
    var region = await manager.CreateRegion("My Region", maxCities: 6);
    Console.WriteLine($"Region created: {region.RegionCode}");
}
catch (Exception ex)
{
    Console.WriteLine($"Failed to create region: {ex.Message}");
}
```

---

## ⚠️ Important Notes

### Thread Safety

- Most RegionalManager methods are **not thread-safe**
- Call from Unity main thread or use proper synchronization
- Async methods return `Task` - await them properly

### Null Safety

- Always check for `null` when accessing `CurrentRegion` or `LocalCityData`
- Use null-conditional operators (`?.`) when accessing nested properties

### Error Handling

- Wrap API calls in try-catch blocks
- Check return values for `null` or `false`
- Log errors using `CitiesRegional.Logging.LogError()`

---

## 📚 Related Documentation

- **[USER_GUIDE.md](USER_GUIDE.md)** - End-user documentation
- **[INSTALLATION_GUIDE.md](INSTALLATION_GUIDE.md)** - Installation instructions
- **[MISSION_BRIEFING.md](MISSION_BRIEFING.md)** - Architecture overview
- **[TRADE_SYSTEM_REQUIREMENTS.md](TRADE_SYSTEM_REQUIREMENTS.md)** - Trade system details

---

**Status:** API documentation created  
**Last Updated:** 2025-12-27  
**Note:** This documentation covers the public APIs. Internal implementation details may change.

