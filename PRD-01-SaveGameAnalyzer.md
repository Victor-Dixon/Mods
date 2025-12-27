# PRD: City Analytics - Save Game Analyzer

## 📋 Overview

| Field | Value |
|-------|-------|
| **Mod Name** | City Analytics |
| **Codename** | `CityAnalytics` |
| **Category** | Debug / Visualization Tools |
| **Complexity** | ⭐⭐ Medium-Low |
| **Time to MVP** | 2-4 weeks |
| **Target Users** | City optimizers, data enthusiasts, mod developers |

---

## 🎯 Problem Statement

Cities: Skylines 2 provides limited visibility into the underlying simulation data. Players who want to optimize their cities lack:
- Historical data tracking (how metrics change over time)
- Exportable data for external analysis
- Visual heatmaps for traffic, land value, pollution
- Debugging tools for understanding simulation behavior

**User Pain Points:**
1. "Why is my traffic suddenly terrible?" - No way to see what changed
2. "Is my economy improving?" - Only current snapshot available
3. "Where should I build next?" - No data-driven recommendations
4. Mod developers need to understand game state for debugging

---

## 🎯 Goals & Success Metrics

### Primary Goals
1. Provide historical tracking of key city metrics
2. Visualize spatial data through heatmaps
3. Enable data export for external analysis
4. Create a foundation for future analytics mods

### Success Metrics
| Metric | Target |
|--------|--------|
| Downloads (first month) | 5,000+ |
| User rating | 4.5+ stars |
| Performance impact | <2% FPS loss |
| Data accuracy | 100% match with game values |

---

## 👥 User Personas

### 1. The Optimizer (Primary)
- Plays 50+ hours
- Wants to min-max their city
- Exports data to spreadsheets
- Asks "why" constantly

### 2. The Casual Analyst
- Enjoys pretty graphs
- Wants quick insights
- Doesn't export data
- 10-minute session with the tool

### 3. The Mod Developer
- Needs debugging info
- Wants raw data access
- API consumer
- Technical user

---

## ✨ Features

### MVP (Phase 1) - 2 weeks

#### F1: City Dashboard
**Description:** Main UI panel showing current city statistics

| Stat Category | Metrics |
|---------------|---------|
| Population | Total, births, deaths, immigration, emigration |
| Economy | Income, expenses, balance, tax rates |
| Traffic | Average flow, congestion %, vehicles |
| Services | Coverage %, response times |
| Happiness | Overall, by category |

**UI Mockup:**
```
┌─────────────────────────────────────────────┐
│ 📊 CITY ANALYTICS                     [X]  │
├─────────────────────────────────────────────┤
│ Population: 52,340 (+124 today)             │
│ ████████████████████░░░░ 78% Happiness      │
│                                             │
│ 💰 Economy          🚗 Traffic              │
│ Income:  $2.4M      Flow: 76%               │
│ Expense: $1.8M      Congestion: 12%         │
│ Balance: +$600K     Vehicles: 8,432         │
│                                             │
│ [History] [Heatmaps] [Export] [Settings]    │
└─────────────────────────────────────────────┘
```

#### F2: Historical Graphs
**Description:** Track metrics over in-game time

- Line charts for each metric category
- Configurable time range (day, week, month, year)
- Multiple metrics on same graph for correlation
- Hover for exact values

**Data Points Tracked:**
- Every in-game day: snapshot of all metrics
- Stored in mod save data
- Max 365 in-game days (configurable)

#### F3: Data Export
**Description:** Export city data to files

| Format | Use Case |
|--------|----------|
| CSV | Spreadsheet analysis |
| JSON | Programmatic access |

**Export Options:**
- Current snapshot
- Full history
- Filtered by date range
- Filtered by metric category

### Phase 2 - 2 weeks

#### F4: Traffic Heatmap
**Description:** Visual overlay showing traffic density

- Color gradient: Green (free flow) → Red (congested)
- Toggle on/off from dashboard
- Updates every few seconds
- Click road segment for details

#### F5: Land Value Heatmap
**Description:** Visualize property values across the city

- Color gradient: Blue (low) → Gold (high)
- Shows value trends (up/down arrows)
- Useful for zoning decisions

#### F6: Pollution Heatmap
**Description:** Air, ground, and noise pollution visualization

- Separate toggles for each type
- Intensity-based coloring
- Source identification

### Phase 3 - Future

#### F7: Alerts & Notifications
- "Traffic congestion increased 20% this week"
- "Budget deficit detected"
- Configurable thresholds

#### F8: City Comparison
- Compare current city to previous saves
- Benchmark against "ideal" metrics

#### F9: API for Other Mods
- Expose data via mod API
- Allow other mods to add custom metrics

---

## 🏗️ Technical Architecture

### Tech Stack
| Component | Technology |
|-----------|------------|
| Framework | BepInEx 5.x |
| Patching | Harmony 2.x |
| UI | Gooee (React-based) or Unity IMGUI |
| Data Storage | JSON in mod folder |
| Visualization | Custom shaders for heatmaps |

### System Components

```
┌─────────────────────────────────────────────────────────┐
│                    City Analytics Mod                    │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │ Data        │  │ History     │  │ Export      │     │
│  │ Collector   │  │ Manager     │  │ Service     │     │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘     │
│         │                │                │             │
│         ▼                ▼                ▼             │
│  ┌─────────────────────────────────────────────────┐   │
│  │              Data Store (JSON)                   │   │
│  └─────────────────────────────────────────────────┘   │
│         │                                               │
│         ▼                                               │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │ Dashboard   │  │ Graphs      │  │ Heatmap     │     │
│  │ UI          │  │ Renderer    │  │ Renderer    │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### Key Classes

```csharp
// Main plugin entry
[BepInPlugin("com.yourname.cityanalytics", "City Analytics", "1.0.0")]
public class CityAnalyticsPlugin : BaseUnityPlugin

// Data collection
public class MetricsCollector : GameSystemBase
public class PopulationMetrics : IMetricProvider
public class EconomyMetrics : IMetricProvider
public class TrafficMetrics : IMetricProvider

// Storage
public class HistoryManager
public class MetricSnapshot
public class ExportService

// UI
public class DashboardPanel
public class GraphRenderer
public class HeatmapOverlay
```

### Game Systems to Hook

| System | Data Available |
|--------|----------------|
| `PopulationSystem` | Birth/death rates, age distribution |
| `EconomySystem` | Income, expenses, taxes |
| `TrafficSystem` | Vehicle counts, congestion |
| `ServiceCoverageSystem` | Service effectiveness |
| `PollutionSystem` | Pollution levels by type |
| `LandValueSystem` | Property values |

### Data Schema

```json
{
  "version": "1.0",
  "cityName": "My City",
  "snapshots": [
    {
      "gameDate": "Year 2, Month 3, Day 15",
      "realTimestamp": "2024-01-15T10:30:00Z",
      "population": {
        "total": 52340,
        "births": 45,
        "deaths": 12,
        "immigrants": 89,
        "emigrants": 23
      },
      "economy": {
        "income": 2400000,
        "expenses": 1800000,
        "balance": 15000000,
        "taxResidential": 12,
        "taxCommercial": 10,
        "taxIndustrial": 10
      },
      "traffic": {
        "averageFlow": 0.76,
        "congestion": 0.12,
        "totalVehicles": 8432
      },
      "happiness": {
        "overall": 78,
        "health": 82,
        "education": 75,
        "safety": 80
      }
    }
  ]
}
```

---

## 🎨 UI/UX Design

### Design Principles
1. **Non-intrusive** - Small button to toggle, doesn't block gameplay
2. **Scannable** - Key metrics visible at a glance
3. **Deep on demand** - Drill down for details
4. **Consistent with game** - Match CS2's visual style

### Color Palette
| Use | Color |
|-----|-------|
| Background | `#1a1a2e` (dark blue) |
| Panels | `#16213e` |
| Accent | `#0f3460` |
| Positive | `#4ecca3` (green) |
| Negative | `#e94560` (red) |
| Neutral | `#a0a0a0` |

### Hotkeys
| Key | Action |
|-----|--------|
| `Ctrl+Shift+A` | Toggle dashboard |
| `H` (in dashboard) | Toggle heatmap |
| `E` (in dashboard) | Quick export |

---

## 📅 Development Roadmap

### Week 1: Foundation
- [ ] Project setup (BepInEx, Harmony)
- [ ] Basic plugin structure
- [ ] Hook into game systems for data
- [ ] Simple IMGUI debug panel

### Week 2: Core Features
- [ ] Data collection for all metrics
- [ ] JSON storage system
- [ ] History tracking
- [ ] Basic dashboard UI

### Week 3: Visualization
- [ ] Graph rendering
- [ ] CSV/JSON export
- [ ] Settings panel
- [ ] Polish and testing

### Week 4: Heatmaps (Optional for MVP)
- [ ] Traffic heatmap shader
- [ ] Land value overlay
- [ ] Performance optimization

---

## ⚠️ Risks & Mitigations

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Game update breaks hooks | High | High | Abstract game API access, quick update response |
| Performance impact | Medium | High | Async data collection, throttle updates |
| Save data corruption | Low | High | Separate save file, validation, backups |
| UI conflicts with other mods | Medium | Low | Configurable position, z-ordering |

---

## 🔗 Dependencies

| Dependency | Version | Required |
|------------|---------|----------|
| BepInEx | 5.4.x | Yes |
| Harmony | 2.x | Yes |
| Gooee | Latest | Optional (for advanced UI) |
| Unified Icon Library | Latest | Optional (for icons) |

---

## 📚 Reference Materials

### Existing Mods to Study
- **Info Loom** - Similar concept, study their approach
- **CSL Stats** (CS1) - Inspiration for features

### Documentation
- CS2 Modding Wiki: https://wiki.ciim.dev/
- BepInEx Docs: https://docs.bepinex.dev/
- Harmony Docs: https://harmony.pardeike.net/

---

## ✅ Definition of Done

### MVP Complete When:
- [ ] Dashboard shows live city metrics
- [ ] History tracks last 30 in-game days
- [ ] Export to CSV works
- [ ] <2% performance impact
- [ ] Works with latest game version
- [ ] Published on Thunderstore

### Quality Criteria
- No crashes on normal gameplay
- Accurate data (validated against game UI)
- Responsive UI (<100ms updates)
- Clean code, documented

