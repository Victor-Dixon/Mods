# PRD: Cities Regional - Connected Cities Network

## 📋 Overview

| Field | Value |
|-------|-------|
| **Mod Name** | Cities Regional |
| **Codename** | `CitiesRegional` |
| **Category** | Multiplayer / Social |
| **Complexity** | ⭐⭐⭐ Medium |
| **Time to MVP** | 2-3 months |
| **Team Size** | 1-2 developers (solo-friendly!) |
| **Target Users** | Friends who want connected but independent cities |

---

## 🎯 Executive Summary

**Cities Regional** enables players to connect their independently-running cities into a shared regional economy. Unlike full multiplayer (which requires real-time sync of millions of entities), Regional only syncs **aggregated city data** every few minutes - making it technically achievable as a solo project.

### The Key Insight

```
Full Multiplayer:     Sync 1,000,000+ entities in real-time = NIGHTMARE
Regional Play:        Sync ~50 aggregated values every 5 min = ACHIEVABLE
```

---

## 🎯 Problem Statement

Players want to share their Cities: Skylines 2 experience with friends, but:

1. **Full multiplayer is extremely complex** - years of development
2. **Async save sharing is tedious** - not a real shared experience  
3. **No competition/cooperation mechanics** - playing alone together

**Regional Play solves this by:**
- Each player runs their own city (no sync complexity)
- Cities affect each other through trade, commuters, shared services
- Creates meaningful cooperation without technical nightmares

---

## 🎯 Goals & Success Metrics

### Primary Goals
1. Enable friends to have interconnected cities
2. Create meaningful inter-city economic relationships
3. Support 2-8 players in a region
4. Achieve this with reasonable development effort

### Success Metrics
| Metric | Target |
|--------|--------|
| Connection stability | 99%+ uptime |
| Sync latency tolerance | Up to 30 seconds OK |
| Players per region | 2-8 supported |
| Impact on single-player | Zero when not connected |
| Time to working MVP | 8 weeks |

---

## 👥 User Personas

### 1. The Cooperative Friends (Primary)
- 2-4 friends who play CS2
- Want to build a "region" together
- Different playstyles (one likes transit, one likes industry)
- Online at different times (async-friendly needed)

### 2. The Competitors
- Want same starting conditions
- Race to milestones
- Compare cities on leaderboard
- Bragging rights

### 3. The Specialists
- "I'll be the industrial city, you be residential"
- Intentional interdependence
- Maximize regional efficiency together

---

## ✨ Core Concept: The Region

### What Is A Region?

A **Region** is a group of 2-8 player cities that share:
- Economic connections (trade, workers)
- Physical connections (highways, rail)
- Optional shared services (airport, university)
- A persistent cloud/P2P state

```
┌─────────────────────────────────────────────────────────────────┐
│                        THE REGION                                │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│    ┌──────────┐         ┌──────────┐         ┌──────────┐      │
│    │  City A  │◄───────►│  City B  │◄───────►│  City C  │      │
│    │  (You)   │  trade  │ (Friend) │  trade  │ (Friend) │      │
│    │          │ workers │          │ workers │          │      │
│    └────┬─────┘         └────┬─────┘         └────┬─────┘      │
│         │                    │                    │             │
│         │     ┌──────────────┴──────────────┐    │             │
│         └────►│      Regional Services       │◄───┘             │
│               │  - Shared Airport            │                  │
│               │  - Regional University       │                  │
│               │  - Inter-city Transit        │                  │
│               └──────────────────────────────┘                  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### What Gets Synced?

| Data Category | Specific Values | Sync Frequency |
|---------------|-----------------|----------------|
| **Population** | Total, workers, unemployed | Every 5 min |
| **Economy** | GDP, trade capacity, prices | Every 5 min |
| **Resources** | Exports available, imports needed | Every 2 min |
| **Connections** | Highway/rail capacity, travel time | On change |
| **Services** | Shared service utilization | Every 5 min |
| **Events** | Milestones, disasters, achievements | Immediate |
| **City Stats** | Land value avg, happiness, pollution | Every 10 min |

### What Does NOT Sync?

- Individual citizens (millions of entities)
- Individual vehicles
- Building-level data
- Real-time simulation state
- Camera/UI state

---

## ✨ Features

### MVP (Phase 1) - 6 weeks

#### F1: Region Creation & Joining
**Description:** Create or join a regional network

**Create Region Flow:**
```
┌─────────────────────────────────────────────┐
│ 🌐 CREATE REGION                      [X]   │
├─────────────────────────────────────────────┤
│                                             │
│ Region Name: [Metro Partnership    ]        │
│                                             │
│ Connection Type:                            │
│ ○ Peer-to-Peer (no server needed)          │
│ ● Cloud Hosted (always available)          │
│ ○ Self-Hosted Server (advanced)            │
│                                             │
│ Max Cities: [4] ▼                           │
│                                             │
│ Region Code: METRO-7X4K-9PLM               │
│ (Share this with friends to join)          │
│                                             │
│ [Create Region]                             │
└─────────────────────────────────────────────┘
```

**Join Region Flow:**
```
┌─────────────────────────────────────────────┐
│ 🔗 JOIN REGION                        [X]   │
├─────────────────────────────────────────────┤
│                                             │
│ Region Code: [METRO-7X4K-9PLM     ]        │
│                                             │
│ Your City: [Riverdale            ]         │
│                                             │
│ ┌─────────────────────────────────────┐    │
│ │ Region Found: Metro Partnership     │    │
│ │ Cities: 2/4                         │    │
│ │ - Springfield (Alice) ✓ Online     │    │
│ │ - Shelbyville (Bob) ○ Offline      │    │
│ └─────────────────────────────────────┘    │
│                                             │
│ [Join Region]                              │
└─────────────────────────────────────────────┘
```

#### F2: Regional Dashboard
**Description:** Overview of all cities in your region

```
┌─────────────────────────────────────────────────────────────────┐
│ 🌐 REGIONAL OVERVIEW: Metro Partnership                   [X]   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  YOUR CITY          PARTNER CITIES                              │
│  ┌────────────┐     ┌────────────┐     ┌────────────┐          │
│  │ Riverdale  │     │Springfield │     │Shelbyville │          │
│  │ Pop: 45K   │     │ Pop: 62K   │     │ Pop: 28K   │          │
│  │ 🟢 Online  │     │ 🟢 Online  │     │ ⚫ Offline │          │
│  │            │     │            │     │            │          │
│  │ Industrial │     │ Commercial │     │ Residential│          │
│  │ Specialist │     │ Hub        │     │ Suburb     │          │
│  └────────────┘     └────────────┘     └────────────┘          │
│                                                                 │
│  REGIONAL STATS                                                │
│  ════════════════════════════════════════════════════          │
│  Total Population: 135,000                                     │
│  Regional GDP: $4.2M/week                                      │
│  Trade Volume: 12,400 units/week                               │
│  Commuters: 8,200 daily                                        │
│                                                                 │
│  [Trade] [Connections] [Services] [Leaderboard] [Settings]     │
└─────────────────────────────────────────────────────────────────┘
```

#### F3: Trade System
**Description:** Cities exchange resources automatically

**How Trade Works:**
1. Each city reports what it produces and needs
2. System matches exports to imports
3. Resources "flow" between cities
4. Revenue/costs calculated and applied

**Trade Configuration:**
```
┌─────────────────────────────────────────────────────────────────┐
│ 📦 TRADE CONFIGURATION                                    [X]   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  YOUR EXPORTS (Available to region)                            │
│  ─────────────────────────────────────────────────────────     │
│  Resource          Production    Export     Price               │
│  🏭 Industrial     1,200/wk     [800] ▼    $45/unit            │
│  🌾 Agriculture    400/wk       [400] ▼    $20/unit            │
│  ⚡ Electricity    2,000 MW     [500] ▼    $12/MW              │
│                                                                 │
│  YOUR IMPORTS (Needed from region)                             │
│  ─────────────────────────────────────────────────────────     │
│  Resource          Demand       Import     Max Price            │
│  👷 Workers        500/day      [Auto]     $--                  │
│  📦 Commercial     600/wk       [400] ▼    $60/unit            │
│  🎓 Education      200 slots    [150] ▼    $100/slot           │
│                                                                 │
│  TRADE BALANCE: +$28,400/week                                  │
│                                                                 │
│  [Auto-Balance] [Apply] [Trade History]                        │
└─────────────────────────────────────────────────────────────────┘
```

**Trade Flow Visualization:**
```
┌─────────────────────────────────────────────────────────────────┐
│ 📊 TRADE FLOWS                                            [X]   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│            Springfield                                          │
│                 │                                               │
│        Commercial Goods (400)                                   │
│                 │                                               │
│                 ▼                                               │
│  Riverdale ◄────────────► Shelbyville                          │
│      │     Workers (200)        │                               │
│      │                          │                               │
│      └──── Industrial (300) ────┘                               │
│                                                                 │
│  Line thickness = trade volume                                 │
│  Arrow direction = flow direction                              │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

#### F4: Worker Commuting
**Description:** Citizens can work in neighboring cities

**How It Works:**
1. Your city has job openings
2. Neighbor cities have unemployed workers
3. System calculates commute feasibility (time, connections)
4. Workers "commute" - appear as employed in your city
5. Their home city gets residential tax, your city gets productivity

**Commute Panel:**
```
┌─────────────────────────────────────────────────────────────────┐
│ 👷 REGIONAL WORKFORCE                                     [X]   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  WORKERS COMMUTING TO YOUR CITY                                │
│  ─────────────────────────────────────────────────────────     │
│  From Springfield:  1,240 workers (25 min commute)             │
│  From Shelbyville:    890 workers (18 min commute)             │
│  Total Inbound:     2,130 workers                              │
│                                                                 │
│  YOUR WORKERS COMMUTING OUT                                    │
│  ─────────────────────────────────────────────────────────     │
│  To Springfield:      450 workers (25 min commute)             │
│  To Shelbyville:      320 workers (18 min commute)             │
│  Total Outbound:      770 workers                              │
│                                                                 │
│  NET WORKFORCE GAIN: +1,360 workers                            │
│                                                                 │
│  [Adjust Commute Limits] [View by Industry] [Improve Transit]  │
└─────────────────────────────────────────────────────────────────┘
```

#### F5: Regional Connections
**Description:** Physical links between cities

**Connection Types:**
| Type | Capacity | Cost | Build Time |
|------|----------|------|------------|
| Highway (2-lane) | 1,000 vehicles/hr | $50K | Instant |
| Highway (4-lane) | 2,500 vehicles/hr | $150K | Instant |
| Highway (6-lane) | 5,000 vehicles/hr | $300K | Instant |
| Regional Rail | 2,000 passengers/hr | $200K | Instant |
| High-Speed Rail | 5,000 passengers/hr | $500K | Instant |
| Cargo Rail | 500 containers/hr | $250K | Instant |

**Connection Editor:**
```
┌─────────────────────────────────────────────────────────────────┐
│ 🛣️ REGIONAL CONNECTIONS                                   [X]   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  RIVERDALE ←→ SPRINGFIELD                                      │
│  ─────────────────────────────────────────────────────────     │
│  🛣️ Highway 101 (4-lane)                                       │
│     Capacity: 2,500 veh/hr  |  Usage: 1,840 (74%)              │
│     Travel Time: 22 minutes                                     │
│     [Upgrade to 6-lane: $150K]                                 │
│                                                                 │
│  🚂 Metro Regional Rail                                        │
│     Capacity: 2,000 pass/hr |  Usage: 1,200 (60%)              │
│     Travel Time: 15 minutes                                     │
│     [Add Express Service: $100K]                               │
│                                                                 │
│  [+ Add New Connection]                                        │
│                                                                 │
│  RIVERDALE ←→ SHELBYVILLE                                      │
│  ─────────────────────────────────────────────────────────     │
│  🛣️ County Road (2-lane)                                       │
│     Capacity: 1,000 veh/hr  |  Usage: 950 (95%) ⚠️             │
│     Travel Time: 35 minutes                                     │
│     [Upgrade to Highway: $100K]                                │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Phase 2 - 4 weeks

#### F6: Shared Regional Services
**Description:** Build services that benefit the whole region

**Shareable Services:**
| Service | Host Benefit | User Benefit | Cost Share |
|---------|--------------|--------------|------------|
| Regional Airport | Tourism +30% | Tourism +15% | 40/30/30 |
| University | Educated workers | Student slots | Per student |
| Stadium | Entertainment | Events | Per event |
| Hospital | Healthcare | Overflow care | Per patient |
| Power Plant | Revenue | Cheap power | Per MW |
| Landfill | Revenue | Waste disposal | Per ton |

**Shared Service Panel:**
```
┌─────────────────────────────────────────────────────────────────┐
│ 🏛️ REGIONAL SERVICES                                     [X]   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  SERVICES YOU HOST                                             │
│  ─────────────────────────────────────────────────────────     │
│  🎓 Regional University                                        │
│     Capacity: 5,000 students                                   │
│     Your students: 2,100 | Regional: 2,400                     │
│     Revenue from region: $48,000/week                          │
│                                                                 │
│  SERVICES YOU USE                                              │
│  ─────────────────────────────────────────────────────────     │
│  ✈️ Regional Airport (hosted by: Springfield)                  │
│     Your tourism boost: +18%                                   │
│     Your cost share: $12,000/week                              │
│                                                                 │
│  ♻️ Regional Recycling (hosted by: Shelbyville)                │
│     Your waste processed: 450 tons/week                        │
│     Your cost: $4,500/week                                     │
│                                                                 │
│  [Propose New Service] [Renegotiate Costs]                     │
└─────────────────────────────────────────────────────────────────┘
```

#### F7: Regional Events & Milestones
**Description:** Shared achievements and events

**Event Types:**
- 🎉 Milestone reached (city hits 50K pop)
- 🏆 Competition won (highest happiness this month)
- ⚠️ Disaster in region (affects trade routes)
- 📢 Regional project completed

**Events Feed:**
```
┌─────────────────────────────────────────────────────────────────┐
│ 📰 REGIONAL NEWS                                          [X]   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  🎉 TODAY                                                       │
│  ────────────────────────────────────────────────────────      │
│  • Springfield reached 75,000 population!                      │
│  • New highway connection opened: Riverdale ↔ Shelbyville      │
│                                                                 │
│  📅 THIS WEEK                                                   │
│  ────────────────────────────────────────────────────────      │
│  • Regional trade volume hit 10,000 units milestone           │
│  • Shelbyville's power plant now serving 3 cities             │
│  • Traffic between cities up 25% - consider rail?             │
│                                                                 │
│  🏆 MONTHLY LEADERS                                            │
│  ────────────────────────────────────────────────────────      │
│  • Highest Population: Springfield (75K) 👑                    │
│  • Best Happiness: Riverdale (82%) 👑                          │
│  • Most Profitable: Shelbyville (+$125K) 👑                    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

#### F8: Regional Leaderboard
**Description:** Friendly competition between cities

```
┌─────────────────────────────────────────────────────────────────┐
│ 🏆 REGIONAL LEADERBOARD                                   [X]   │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  Category          1st           2nd           3rd              │
│  ════════════════════════════════════════════════════════      │
│  Population        Springfield   Riverdale     Shelbyville      │
│                    75,000        45,000        28,000           │
│                                                                 │
│  Happiness         Riverdale     Shelbyville   Springfield      │
│                    82%           78%           71%              │
│                                                                 │
│  Economy           Shelbyville   Springfield   Riverdale        │
│                    +$125K        +$89K         +$45K            │
│                                                                 │
│  Traffic Flow      Riverdale     Shelbyville   Springfield      │
│                    94%           87%           72%              │
│                                                                 │
│  Education         Riverdale     Springfield   Shelbyville      │
│                    91%           85%           79%              │
│                                                                 │
│  [This Week] [This Month] [All Time] [Custom Metric]           │
└─────────────────────────────────────────────────────────────────┘
```

### Phase 3 - Future

#### F9: Regional Challenges
- Timed goals for the whole region
- "Collectively reach 500K population"
- "Reduce regional pollution by 50%"
- Rewards for completion

#### F10: Disaster Cooperation
- Disasters can spread between cities
- Share emergency services
- Evacuation to neighbor cities

#### F11: Regional Planning Map
- Overview map showing all cities
- Connection visualization
- Future expansion planning

---

## 🏗️ Technical Architecture

### Architecture Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    Cities Regional Mod                           │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                    Client Layer                           │  │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐       │  │
│  │  │ Data        │  │ Effect      │  │ UI          │       │  │
│  │  │ Collector   │  │ Applicator  │  │ Panels      │       │  │
│  │  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘       │  │
│  └─────────┼────────────────┼────────────────┼──────────────┘  │
│            │                │                │                  │
│            ▼                ▼                ▼                  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                   Regional Manager                        │  │
│  │  - Orchestrates sync                                      │  │
│  │  - Manages region state                                   │  │
│  │  - Handles offline/online transitions                     │  │
│  └───────────────────────────┬──────────────────────────────┘  │
│                              │                                  │
│  ┌───────────────────────────┼──────────────────────────────┐  │
│  │                    Network Layer                          │  │
│  │  ┌─────────────┐         │         ┌─────────────┐       │  │
│  │  │ P2P Mode    │◄────────┴────────►│ Cloud Mode  │       │  │
│  │  │ (LiteNetLib)│                   │ (REST API)  │       │  │
│  │  └─────────────┘                   └─────────────┘       │  │
│  └──────────────────────────────────────────────────────────┘  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘

                              │
                              │ HTTPS / UDP
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Regional Server (Optional)                    │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │
│  │ Region      │  │ State       │  │ Event       │              │
│  │ Registry    │  │ Store       │  │ Broadcaster │              │
│  └─────────────┘  └─────────────┘  └─────────────┘              │
└─────────────────────────────────────────────────────────────────┘
```

### Data Models

```csharp
// Core data that gets synced
[Serializable]
public class RegionalCityData
{
    // Identity
    public string CityId;          // Unique identifier
    public string CityName;        // Display name
    public string PlayerName;      // Owner
    public string PlayerId;        // Owner ID
    
    // Status
    public bool IsOnline;
    public DateTime LastSeen;
    public DateTime LastSync;
    
    // Population
    public int Population;
    public int Workers;
    public int UnemployedWorkers;
    public int AvailableJobs;
    public int Students;
    
    // Economy
    public long Treasury;
    public float WeeklyIncome;
    public float WeeklyExpenses;
    public float GDPEstimate;
    
    // Resources (production/consumption per week)
    public ResourceData[] Resources;
    
    // City metrics
    public float Happiness;
    public float Health;
    public float Education;
    public float TrafficFlow;
    public float LandValueAvg;
    public float Pollution;
    
    // Capabilities
    public SharedService[] HostedServices;
    public SharedService[] UsedServices;
}

[Serializable]
public class ResourceData
{
    public ResourceType Type;
    public float Production;      // Units per week
    public float Consumption;     // Units per week
    public float ExportAvailable; // Willing to export
    public float ImportNeeded;    // Needs to import
    public float Price;           // Per unit
}

public enum ResourceType
{
    IndustrialGoods,
    CommercialGoods,
    Agriculture,
    Electricity,
    Water,
    Workers,            // People, not goods
    Students,           // Education capacity
    Tourists,           // Tourism exchange
    Waste,              // For shared disposal
    RawMaterials
}

[Serializable]
public class RegionalConnection
{
    public string FromCityId;
    public string ToCityId;
    public ConnectionType Type;
    public int Capacity;          // Vehicles or passengers per hour
    public int CurrentUsage;
    public float TravelTimeMinutes;
    public float CostToUpgrade;
}

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

[Serializable]
public class SharedService
{
    public string ServiceId;
    public SharedServiceType Type;
    public string HostCityId;
    public int Capacity;
    public int UsedCapacity;
    public Dictionary<string, int> UsageByCity;  // CityId -> usage
    public Dictionary<string, float> CostShareByCity;
}

public enum SharedServiceType
{
    Airport,
    University,
    Stadium,
    Hospital,
    PowerPlant,
    WaterTreatment,
    Landfill,
    RecyclingCenter,
    ConventionCenter
}

[Serializable]
public class Region
{
    public string RegionId;
    public string RegionName;
    public string RegionCode;      // For joining
    public string HostPlayerId;    // Creator
    public int MaxCities;
    
    public List<RegionalCityData> Cities;
    public List<RegionalConnection> Connections;
    public List<TradeAgreement> TradeAgreements;
    public List<RegionalEvent> RecentEvents;
    
    public DateTime CreatedAt;
    public DateTime LastActivity;
}
```

### Sync Protocol

```csharp
public interface IRegionalSync
{
    // Connection
    Task<bool> ConnectToRegion(string regionCode);
    Task<Region> CreateRegion(string name, int maxCities);
    Task LeaveRegion();
    
    // Data sync
    Task PushCityData(RegionalCityData myCity);
    Task<List<RegionalCityData>> PullRegionData();
    
    // Connections
    Task<bool> ProposeConnection(RegionalConnection connection);
    Task<bool> AcceptConnection(string connectionId);
    
    // Services
    Task<bool> OfferService(SharedService service);
    Task<bool> RequestServiceAccess(string serviceId);
    
    // Events
    Task BroadcastEvent(RegionalEvent evt);
    event Action<RegionalEvent> OnEventReceived;
}

// REST API Implementation
public class CloudRegionalSync : IRegionalSync
{
    private readonly HttpClient _client;
    private readonly string _baseUrl = "https://api.citiesregional.com";
    
    public async Task PushCityData(RegionalCityData myCity)
    {
        var json = JsonConvert.SerializeObject(myCity);
        await _client.PostAsync(
            $"{_baseUrl}/regions/{_regionId}/cities/{myCity.CityId}",
            new StringContent(json, Encoding.UTF8, "application/json")
        );
    }
    
    public async Task<List<RegionalCityData>> PullRegionData()
    {
        var response = await _client.GetAsync(
            $"{_baseUrl}/regions/{_regionId}/cities"
        );
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<RegionalCityData>>(json);
    }
}

// P2P Implementation (no server needed)
public class P2PRegionalSync : IRegionalSync
{
    private readonly NetManager _netManager;
    private readonly Dictionary<string, NetPeer> _peers;
    
    // Uses LiteNetLib for direct peer connections
    // Region state is shared among all peers
    // Conflict resolution: latest timestamp wins
}
```

### Game Integration (Harmony Patches)

```csharp
// Collect city data periodically
public class CityDataCollector : GameSystemBase
{
    private RegionalCityData _currentData;
    private float _lastCollectTime;
    private const float COLLECT_INTERVAL = 60f; // Every minute
    
    protected override void OnUpdate()
    {
        if (Time.time - _lastCollectTime < COLLECT_INTERVAL)
            return;
            
        _currentData = CollectCityData();
        RegionalManager.Instance.UpdateLocalData(_currentData);
        _lastCollectTime = Time.time;
    }
    
    private RegionalCityData CollectCityData()
    {
        var data = new RegionalCityData();
        
        // Read from game systems
        data.Population = GetPopulationSystem().TotalPopulation;
        data.Workers = GetPopulationSystem().WorkingPopulation;
        data.Treasury = GetEconomySystem().Treasury;
        // ... etc
        
        return data;
    }
}

// Apply effects from regional connections
public class RegionalEffectsApplicator : GameSystemBase
{
    protected override void OnUpdate()
    {
        var regionData = RegionalManager.Instance.GetRegionData();
        if (regionData == null) return;
        
        ApplyTradeEffects(regionData);
        ApplyCommuterEffects(regionData);
        ApplyServiceEffects(regionData);
    }
    
    private void ApplyTradeEffects(Region region)
    {
        // Calculate net imports/exports
        // Adjust local resource levels
        // Apply revenue/costs
    }
    
    private void ApplyCommuterEffects(Region region)
    {
        // Calculate commuters in/out
        // Adjust worker availability
        // Affect traffic on connections
    }
}
```

### Server Component (Simple)

```csharp
// Minimal ASP.NET Core Web API
[ApiController]
[Route("api/regions")]
public class RegionsController : ControllerBase
{
    private readonly IRegionStore _store;
    
    [HttpPost]
    public async Task<Region> CreateRegion(CreateRegionRequest request)
    {
        var region = new Region
        {
            RegionId = Guid.NewGuid().ToString(),
            RegionCode = GenerateCode(),
            RegionName = request.Name,
            MaxCities = request.MaxCities,
            CreatedAt = DateTime.UtcNow
        };
        
        await _store.SaveRegion(region);
        return region;
    }
    
    [HttpGet("{regionId}/cities")]
    public async Task<List<RegionalCityData>> GetCities(string regionId)
    {
        var region = await _store.GetRegion(regionId);
        return region.Cities;
    }
    
    [HttpPost("{regionId}/cities/{cityId}")]
    public async Task UpdateCity(string regionId, string cityId, 
                                  RegionalCityData cityData)
    {
        var region = await _store.GetRegion(regionId);
        var existing = region.Cities.FirstOrDefault(c => c.CityId == cityId);
        
        if (existing != null)
            region.Cities.Remove(existing);
            
        cityData.LastSync = DateTime.UtcNow;
        region.Cities.Add(cityData);
        
        await _store.SaveRegion(region);
        await _eventHub.BroadcastCityUpdate(regionId, cityData);
    }
}
```

---

## 📅 Development Roadmap

### Week 1-2: Foundation
- [ ] Project setup (BepInEx, networking libs)
- [ ] Data models defined
- [ ] Basic region creation/join
- [ ] Simple sync proof of concept

### Week 3-4: Core Sync
- [ ] City data collection from game
- [ ] Push/pull implementation
- [ ] Basic UI (region dashboard)
- [ ] Connection management

### Week 5-6: Trade System
- [ ] Resource tracking
- [ ] Trade matching algorithm
- [ ] Trade effects on economy
- [ ] Trade UI

### Week 7-8: Polish & Testing
- [ ] Commuter system
- [ ] Connection upgrades
- [ ] Error handling
- [ ] Multi-player testing
- [ ] Performance optimization

### Week 9-10: Phase 2 Start
- [ ] Shared services
- [ ] Events system
- [ ] Leaderboard

---

## ⚠️ Risks & Mitigations

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Server costs | Medium | Medium | Start with P2P, add cloud later |
| Cheating (fake data) | Medium | Low | Validation, trust model |
| Game updates break collectors | High | Medium | Abstract game API access |
| Players never online together | Medium | Low | Async-first design |
| Network issues | Medium | Medium | Graceful degradation |

---

## 🔗 Dependencies

| Dependency | Purpose | Required |
|------------|---------|----------|
| BepInEx | Mod framework | Yes |
| Harmony | Game patching | Yes |
| Newtonsoft.Json | Serialization | Yes |
| LiteNetLib | P2P networking | For P2P mode |
| HttpClient | Cloud API | For cloud mode |

---

## ✅ Definition of Done

### MVP Complete When:
- [ ] Create/join region works
- [ ] 2-4 cities sync data correctly
- [ ] Trade system functional
- [ ] Basic commuter effects
- [ ] Connections can be built/upgraded
- [ ] Works with both online/offline players
- [ ] Published on Thunderstore

### Quality Bar
- Sync is reliable (>99%)
- No data loss on disconnect
- Intuitive UI
- <5% performance impact
- Well documented

---

## 🎯 Why This Will Succeed

1. **Technically achievable** - No real-time sync nightmare
2. **Clear value prop** - Friends can play "together"
3. **Async-friendly** - Works even if friends play at different times
4. **Expandable** - Can add features over time
5. **Novel** - Nothing like this exists for CS2 yet

This is the multiplayer mod that can actually ship. 🚀

