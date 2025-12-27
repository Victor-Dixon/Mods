# PRD: Transit+ - Public Transportation Enhancement

## 📋 Overview

| Field | Value |
|-------|-------|
| **Mod Name** | Transit+ |
| **Codename** | `TransitPlus` |
| **Category** | Gameplay / Transportation |
| **Complexity** | ⭐⭐⭐ Medium |
| **Time to MVP** | 4-6 weeks |
| **Target Users** | Transit enthusiasts, efficiency players, realistic city builders |

---

## 🎯 Problem Statement

Cities: Skylines 2's public transit system lacks the depth and control that dedicated players need:

**Current Limitations:**
1. No control over service frequency/headways
2. Can't adjust vehicle capacity
3. No express/local service patterns
4. Limited visibility into line performance
5. No passenger flow visualization
6. Can't coordinate transfers between lines

**User Pain Points:**
- "My buses are always bunching up"
- "I can't tell which lines are overcrowded"
- "I want a subway express that skips stations"
- "No way to see where passengers are going"

---

## 🎯 Goals & Success Metrics

### Primary Goals
1. Give players granular control over transit operations
2. Provide visibility into transit system performance
3. Enable realistic transit patterns (express/local, timed transfers)
4. Improve transit efficiency without increasing vehicle counts

### Success Metrics
| Metric | Target |
|--------|--------|
| Downloads (first month) | 15,000+ |
| User rating | 4.5+ stars |
| Performance impact | <3% FPS loss |
| Lines improved per user | 80% report better performance |

---

## 👥 User Personas

### 1. The Transit Nerd (Primary)
- Loves real-world transit systems
- Wants realism (headways, timed transfers)
- Studies passenger flows
- 100+ hour player

### 2. The Efficiency Player
- Wants to fix bunching
- Minimum vehicles, maximum throughput
- Data-driven decisions
- Doesn't care about realism, just results

### 3. The Casual Builder
- Just wants buses to work better
- Simple toggles, not complex configs
- Auto-optimize feature

---

## ✨ Features

### MVP (Phase 1) - 4 weeks

#### F1: Line Frequency Control
**Description:** Set custom service frequencies per line

**UI:**
```
┌─────────────────────────────────────────────┐
│ 🚌 Line 5 - Downtown Express         [X]   │
├─────────────────────────────────────────────┤
│ Frequency Control                           │
│ ┌─────────────────────────────────────────┐ │
│ │ Current: 1 vehicle every 8 min         │ │
│ │                                         │ │
│ │ Target Headway: [5] minutes  ▼         │ │
│ │                                         │ │
│ │ Peak Hours (7-9, 17-19):               │ │
│ │ Target Headway: [3] minutes  ▼         │ │
│ │                                         │ │
│ │ [■] Enable schedule-based dispatch     │ │
│ └─────────────────────────────────────────┘ │
│                                             │
│ Vehicles Required: 8 (currently 6)         │
│ [Auto-adjust fleet] [Apply] [Cancel]       │
└─────────────────────────────────────────────┘
```

**Functionality:**
- Set target headway (time between vehicles)
- Different headways for peak/off-peak
- Auto-calculate required fleet size
- Override game's default dispatching

#### F2: Line Performance Dashboard
**Description:** Analytics for each transit line

**Metrics Shown:**
| Metric | Description |
|--------|-------------|
| Ridership | Passengers per day |
| Load Factor | % of capacity used |
| Headway Variance | Consistency of spacing |
| Average Wait Time | At busiest stops |
| Revenue/Cost | Financial performance |

**UI:**
```
┌─────────────────────────────────────────────┐
│ 📊 Line Performance                   [X]   │
├─────────────────────────────────────────────┤
│ Line 5 - Downtown Express                   │
│ ════════════════════════════════════════    │
│                                             │
│ Daily Ridership: 4,523 passengers           │
│ ████████████████░░░░ 82% Load Factor        │
│                                             │
│ Headway Target: 5 min | Actual: 5.2 min     │
│ Variance: ±1.2 min (Good)                   │
│                                             │
│ Busiest Stops:                              │
│ 1. Central Station - 1,245 boardings        │
│ 2. Market Square - 892 boardings            │
│ 3. University - 756 boardings               │
│                                             │
│ [View Route] [Edit Settings] [Suggestions]  │
└─────────────────────────────────────────────┘
```

#### F3: Vehicle Capacity Modifier
**Description:** Adjust passenger capacity per vehicle type

| Vehicle Type | Default | Range |
|--------------|---------|-------|
| Small Bus | 30 | 20-45 |
| Articulated Bus | 90 | 60-120 |
| Tram | 150 | 100-200 |
| Metro | 600 | 400-900 |

**Use Cases:**
- Simulate bendy buses with higher capacity
- Balance realism vs. efficiency
- Reduce vehicle count for performance

#### F4: Bunching Prevention
**Description:** AI-assisted spacing maintenance

**How It Works:**
1. Monitor vehicle positions on route
2. Detect when vehicles get too close
3. Slow down trailing vehicle or speed up leader
4. Option: Hold at station to restore spacing

**Settings:**
- Enable/disable per line
- Aggressiveness (gentle/moderate/strict)
- Minimum headway threshold

### Phase 2 - 3 weeks

#### F5: Express/Local Service
**Description:** Create express lines that skip stops

**UI:**
```
┌─────────────────────────────────────────────┐
│ 🚇 Express Service Configuration            │
├─────────────────────────────────────────────┤
│ Base Line: Metro Line 1                     │
│                                             │
│ Stop           | Express | Local |          │
│ ───────────────|─────────|───────|          │
│ Terminal A     |   ●     |   ●   |          │
│ Oak Street     |   ○     |   ●   |          │
│ Central        |   ●     |   ●   |          │
│ Park Ave       |   ○     |   ●   |          │
│ University     |   ●     |   ●   |          │
│ Industrial     |   ○     |   ●   |          │
│ Terminal B     |   ●     |   ●   |          │
│                                             │
│ ● = Stops   ○ = Skips                       │
│                                             │
│ Express saves: 4 min 23 sec per trip        │
│ [Create Express Line] [Cancel]              │
└─────────────────────────────────────────────┘
```

#### F6: Timed Transfers
**Description:** Coordinate arrivals between connecting lines

**How It Works:**
- Designate transfer hubs
- Select connecting lines
- Set coordination window (e.g., arrive within 2 min of each other)
- System adjusts departure times

#### F7: Passenger Flow Visualization
**Description:** See where passengers are traveling

**Visualization Modes:**
1. **Desire Lines:** Show origin-destination pairs
2. **Load by Segment:** Color-code route sections by crowding
3. **Station Flows:** Animated dots showing passenger movement

### Phase 3 - Future

#### F8: Line Optimization Suggestions
- AI analyzes routes and suggests improvements
- "Consider adding stop at X (5,000 potential riders)"
- "This section is underutilized, consider removing stop"

#### F9: Fare Zones
- Define zones
- Set zone-based pricing
- Track revenue by zone

#### F10: Transit Priority
- Give transit vehicles signal priority
- Dedicated lanes integration
- Queue jumping at intersections

---

## 🏗️ Technical Architecture

### System Components

```
┌─────────────────────────────────────────────────────────┐
│                    Transit+ Mod                          │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │ Line        │  │ Dispatch    │  │ Analytics   │     │
│  │ Manager     │  │ Controller  │  │ Collector   │     │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘     │
│         │                │                │             │
│         ▼                ▼                ▼             │
│  ┌─────────────────────────────────────────────────┐   │
│  │           Transit Data Store                     │   │
│  │   (Line configs, schedules, statistics)          │   │
│  └─────────────────────────────────────────────────┘   │
│         │                                               │
│  ┌──────┴──────┐                                       │
│  ▼              ▼                                       │
│  ┌─────────────┐  ┌─────────────┐                      │
│  │ Harmony     │  │ UI          │                      │
│  │ Patches     │  │ Panels      │                      │
│  └─────────────┘  └─────────────┘                      │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### Key Harmony Patches

```csharp
// Override vehicle dispatching
[HarmonyPatch(typeof(TransportLineSystem), "DispatchVehicle")]
class DispatchVehiclePatch

// Modify vehicle capacity
[HarmonyPatch(typeof(PublicTransportVehicleData), "get_Capacity")]
class VehicleCapacityPatch

// Intercept stop decisions for express service
[HarmonyPatch(typeof(TransportVehicleAI), "ShouldStopAtStop")]
class ExpressServicePatch

// Track passenger boarding for analytics
[HarmonyPatch(typeof(PassengerSystem), "BoardVehicle")]
class PassengerTrackingPatch
```

### Data Structures

```csharp
public class TransitLineConfig
{
    public Entity LineEntity;
    public string LineName;
    public TransitMode Mode; // Bus, Tram, Metro, etc.
    
    // Frequency settings
    public int TargetHeadwayMinutes;
    public int PeakHeadwayMinutes;
    public TimeRange PeakHours;
    
    // Express settings
    public bool IsExpress;
    public List<Entity> SkippedStops;
    
    // Bunching prevention
    public bool BunchingPreventionEnabled;
    public float MinHeadwayThreshold;
}

public class LineStatistics
{
    public int DailyRidership;
    public float AverageLoadFactor;
    public float AverageHeadway;
    public float HeadwayVariance;
    public Dictionary<Entity, int> StopBoardings;
    public float RevenuePerDay;
    public float CostPerDay;
}
```

### Game Systems to Hook

| System | Purpose |
|--------|---------|
| `TransportLineSystem` | Line management, vehicle dispatch |
| `PublicTransportVehicleSystem` | Vehicle movement, capacity |
| `PassengerSystem` | Boarding, alighting, routing |
| `WaitingPassengersSystem` | Queue management |
| `TransportStopSystem` | Stop data, waiting counts |

---

## 🎨 UI/UX Design

### Access Points
1. **Transit Info Panel Extension** - Add tabs to existing game UI
2. **Dedicated Window** - `Ctrl+T` to open Transit+ dashboard
3. **Line Context Menu** - Right-click line for quick settings

### Design Language
- Match CS2's transit line colors
- Use transit iconography (bus, metro symbols)
- Clear data visualization (bars, sparklines)

### Mobile-Inspired Touch Targets
- Large buttons for frequent actions
- Swipe-style interactions where applicable
- Collapsible sections

---

## 📅 Development Roadmap

### Week 1-2: Foundation
- [ ] Project setup
- [ ] Hook into TransportLineSystem
- [ ] Read existing line data
- [ ] Basic debug UI showing line info

### Week 3-4: Core Controls
- [ ] Frequency control implementation
- [ ] Capacity modifier
- [ ] Settings persistence
- [ ] Basic UI panels

### Week 5-6: Analytics & Polish
- [ ] Performance metrics collection
- [ ] Dashboard UI
- [ ] Bunching prevention algorithm
- [ ] Testing and optimization

### Week 7-8: Express Service (Phase 2 start)
- [ ] Skip-stop logic
- [ ] Express line creation UI
- [ ] Passenger flow visualization

---

## ⚠️ Risks & Mitigations

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Breaking game's transit AI | High | High | Extensive testing, fallback to default |
| Performance with many lines | Medium | Medium | Throttle updates, cache calculations |
| Conflicts with Traffic mod | Medium | Medium | Coordinate with Traffic mod team |
| Passenger pathfinding breaks | Low | High | Don't modify pathfinding, only dispatch |

---

## 🔗 Dependencies

| Dependency | Version | Required |
|------------|---------|----------|
| BepInEx | 5.4.x | Yes |
| Harmony | 2.x | Yes |
| Gooee | Latest | Recommended |
| Unified Icon Library | Latest | Recommended |

---

## 🔄 Compatibility

### Must Work With
- Traffic mod (lane management)
- Anarchy (placing stops anywhere)
- Better Bulldozer

### Potential Conflicts
- Any mod that overrides TransportLineSystem dispatch
- Vehicle capacity mods (settings would overlap)

---

## 📚 Reference Materials

### Real-World Transit Inspiration
- Transport for London's iBus system
- NYC MTA's countdown clocks
- Japanese rail's clockwork precision

### CS1 Mods to Study
- IPT (Improved Public Transport)
- Transport Lines Manager

### CS2 Code to Study
- Traffic mod (similar system hooks)
- Game's TransportLineSystem (decompile)

---

## ✅ Definition of Done

### MVP Complete When:
- [ ] Can set custom frequency per line
- [ ] Dashboard shows line performance
- [ ] Capacity modifier works
- [ ] Bunching prevention functional
- [ ] <3% FPS impact with 20 lines
- [ ] Save/load persists settings
- [ ] Published on Thunderstore

### Quality Criteria
- No transit system breakage
- Accurate statistics
- Intuitive UI for power users
- Documented features

