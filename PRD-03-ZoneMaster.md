# PRD: Zone Master - District Specialization & Mixed-Use Zoning

## 📋 Overview

| Field | Value |
|-------|-------|
| **Mod Name** | Zone Master |
| **Codename** | `ZoneMaster` |
| **Category** | Gameplay / Zoning |
| **Complexity** | ⭐⭐⭐ Medium-High |
| **Time to MVP** | 5-7 weeks |
| **Target Users** | Urban planners, realistic city builders, customization enthusiasts |

---

## 🎯 Problem Statement

Cities: Skylines 2's zoning system is rigid and doesn't reflect real-world urban development:

**Current Limitations:**
1. No mixed-use zones (residential + commercial together)
2. Districts have limited policy options
3. Can't restrict specific building types within zones
4. No control over density transitions
5. Zoning doesn't consider walkability/transit access

**User Pain Points:**
- "I want apartments above shops like in real cities"
- "My downtown should have different rules than suburbs"
- "I can't create a historic district with building restrictions"
- "No way to make a university district or entertainment zone"

---

## 🎯 Goals & Success Metrics

### Primary Goals
1. Enable mixed-use zoning (residential + commercial in same zone)
2. Provide granular district specialization options
3. Allow building type restrictions/requirements per zone
4. Create smooth density transitions

### Success Metrics
| Metric | Target |
|--------|--------|
| Downloads (first month) | 20,000+ |
| User rating | 4.5+ stars |
| Performance impact | <2% FPS loss |
| Cities using custom zones | 90% of users |

---

## 👥 User Personas

### 1. The Urban Planner (Primary)
- Wants realistic city development
- Studies real zoning codes
- Cares about density gradients
- 200+ hour player

### 2. The Theme Builder
- Wants European old town feel
- Asian commercial districts
- Specific aesthetic goals
- Visual focus

### 3. The Min-Maxer
- Optimize land value
- Maximum efficiency
- Data-driven zoning
- Less about realism

---

## ✨ Features

### MVP (Phase 1) - 4 weeks

#### F1: Mixed-Use Zone Types
**Description:** New zone types combining residential and commercial

| Zone Type | Ground Floor | Upper Floors | Density |
|-----------|--------------|--------------|---------|
| Mixed Low | Small retail | 2-3 floor apartments | Low |
| Mixed Medium | Retail/Office | 4-8 floor apartments | Medium |
| Mixed High | Large retail | 10+ floor apartments | High |
| Commercial Residential | Flexible | Flexible | Variable |

**UI - Zone Palette Extension:**
```
┌─────────────────────────────────────────────┐
│ 🏗️ ZONING                                   │
├─────────────────────────────────────────────┤
│ Standard Zones:                             │
│ [Res Low] [Res Med] [Res High]              │
│ [Com Low] [Com High] [Office]               │
│ [Ind] [Industrial]                          │
│                                             │
│ ━━━━━━━ Zone Master ━━━━━━━                 │
│ Mixed-Use Zones:                            │
│ [Mixed Low] [Mixed Med] [Mixed High]        │
│                                             │
│ [+ Create Custom Zone]                      │
└─────────────────────────────────────────────┘
```

#### F2: Custom Zone Creator
**Description:** Define your own zone types with specific rules

**Configurable Properties:**
```
┌─────────────────────────────────────────────┐
│ 📝 Create Custom Zone                 [X]   │
├─────────────────────────────────────────────┤
│ Zone Name: [University District    ]        │
│ Zone Color: [🎨 Purple        ] ▼           │
│                                             │
│ ─── Building Types Allowed ───              │
│ [✓] Low-rise Residential                    │
│ [✓] Dormitories                             │
│ [✓] Small Commercial (cafes, bookstores)    │
│ [ ] Large Commercial                        │
│ [✓] Office (research, startups)             │
│ [ ] Heavy Industry                          │
│ [✓] Education Buildings                     │
│                                             │
│ ─── Density Settings ───                    │
│ Min Floors: [2]  Max Floors: [6]            │
│ Lot Coverage: [60]%                         │
│ Setback Required: [✓]                       │
│                                             │
│ ─── Special Rules ───                       │
│ [✓] Pedestrian priority                     │
│ [ ] No parking requirements                 │
│ [✓] Green space requirement (15%)           │
│                                             │
│ [Preview Buildings] [Save Zone] [Cancel]    │
└─────────────────────────────────────────────┘
```

#### F3: District Policy Extensions
**Description:** Additional policies beyond base game

| Policy | Effect |
|--------|--------|
| Historic Preservation | Limits building height, requires old style |
| Entertainment District | Allows bars/clubs, extended hours |
| Tech Campus | Office + light industrial, requires transit |
| Eco-Friendly | Requires green roofs, solar, limits parking |
| Waterfront Development | Special building types, public access required |
| Transit-Oriented | Higher density near transit, reduced parking |

**UI Integration:**
- New tab in District panel
- Toggleable policies with descriptions
- Visual indicators on map

#### F4: Density Gradient Tool
**Description:** Automatically transition density based on distance

**How It Works:**
1. Define a center point (downtown, transit station)
2. Set density rings (High → Medium → Low)
3. System creates gradient zones automatically

**UI:**
```
┌─────────────────────────────────────────────┐
│ 📐 Density Gradient                   [X]   │
├─────────────────────────────────────────────┤
│ Center Point: [Click map to set]            │
│                                             │
│ Ring 1 (0-500m):   [High Density    ] ▼     │
│ Ring 2 (500m-1km): [Medium Density  ] ▼     │
│ Ring 3 (1km-2km):  [Low Density     ] ▼     │
│ Beyond:            [Suburban        ] ▼     │
│                                             │
│ [✓] Smooth transitions                      │
│ [✓] Follow roads                            │
│ [ ] Circular (ignore terrain)               │
│                                             │
│ [Preview] [Apply] [Cancel]                  │
└─────────────────────────────────────────────┘
```

### Phase 2 - 3 weeks

#### F5: Building Style Restrictions
**Description:** Control which building styles appear in zones

- Filter by era (Modern, Historic, Industrial)
- Filter by style (European, American, Asian)
- Blacklist specific buildings
- Whitelist only specific buildings

#### F6: Land Use Visualization
**Description:** Overlay showing actual land use vs zoning

- Color-coded by type
- Show zone violations
- Identify rezoning opportunities
- Export for analysis

#### F7: Form-Based Zoning
**Description:** Regulate building form rather than use

**Form Controls:**
- Building height
- Facade requirements
- Setbacks
- Lot coverage
- Parking location

### Phase 3 - Future

#### F8: Zoning History
- Track zone changes over time
- Undo zone changes
- "Before and after" comparison

#### F9: Zoning Templates
- Save zone configurations
- Share with community
- Import from other cities

#### F10: Real-World Zoning Presets
- NYC-style mixed use
- Japanese commercial-residential
- European historic center
- American suburban

---

## 🏗️ Technical Architecture

### Approach: Zone Type Extension
Rather than replacing the game's zoning system, we'll:
1. Create custom zone types that map to combinations of base zones
2. Use Harmony patches to control building spawning
3. Add post-spawn validation for building types

### System Components

```
┌─────────────────────────────────────────────────────────┐
│                    Zone Master Mod                       │
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │ Zone        │  │ Building    │  │ Policy      │     │
│  │ Registry    │  │ Filter      │  │ Manager     │     │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘     │
│         │                │                │             │
│         ▼                ▼                ▼             │
│  ┌─────────────────────────────────────────────────┐   │
│  │              Zone Master Data                    │   │
│  │   (Custom zones, policies, building rules)       │   │
│  └─────────────────────────────────────────────────┘   │
│         │                                               │
│  ┌──────┴──────┬──────────────┐                        │
│  ▼              ▼              ▼                        │
│  ┌─────────────┐ ┌────────────┐ ┌─────────────┐        │
│  │ Zone        │ │ Building   │ │ UI          │        │
│  │ Patches     │ │ Spawn      │ │ Extensions  │        │
│  │             │ │ Patches    │ │             │        │
│  └─────────────┘ └────────────┘ └─────────────┘        │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

### Key Data Structures

```csharp
public class CustomZoneType
{
    public string Id;
    public string Name;
    public Color ZoneColor;
    
    // Building type rules
    public List<BuildingCategory> AllowedCategories;
    public List<string> BlacklistedBuildings;
    public List<string> WhitelistedBuildings;
    
    // Density rules
    public int MinFloors;
    public int MaxFloors;
    public float MaxLotCoverage;
    public float MinSetback;
    
    // Special flags
    public bool IsMixedUse;
    public bool RequiresTransitAccess;
    public bool HistoricPreservation;
}

public class DistrictPolicy
{
    public string Id;
    public string Name;
    public string Description;
    public Sprite Icon;
    
    public Action<District> OnApply;
    public Action<District> OnRemove;
    public Func<Building, bool> BuildingFilter;
}

public enum BuildingCategory
{
    ResidentialLow,
    ResidentialMedium,
    ResidentialHigh,
    CommercialSmall,
    CommercialLarge,
    OfficeSmall,
    OfficeLarge,
    IndustrialLight,
    IndustrialHeavy,
    Civic,
    Education,
    Entertainment
}
```

### Key Harmony Patches

```csharp
// Intercept zone placement
[HarmonyPatch(typeof(ZoneSystem), "ApplyZone")]
class ZonePlacementPatch
{
    // Translate custom zone to base zone + metadata
}

// Filter building spawning
[HarmonyPatch(typeof(BuildingSpawnSystem), "SpawnBuilding")]
class BuildingSpawnPatch
{
    // Check if building type is allowed in this zone
}

// Modify building selection pool
[HarmonyPatch(typeof(ZonedBuildingSystem), "GetBuildingPrefabs")]
class BuildingPoolPatch
{
    // Return filtered list based on zone rules
}

// Add custom zone rendering
[HarmonyPatch(typeof(ZoneRenderSystem), "RenderZones")]
class ZoneRenderPatch
{
    // Render custom zone colors
}
```

### Game Systems to Hook

| System | Purpose |
|--------|---------|
| `ZoneSystem` | Zone placement and management |
| `ZonedBuildingSystem` | Building prefab selection |
| `BuildingSpawnSystem` | Building instantiation |
| `DistrictSystem` | District policies |
| `LandValueSystem` | Custom land value rules |

---

## 🎨 UI/UX Design

### Zone Palette Integration
- Add new section to existing zone toolbar
- Custom zones appear with distinct icons
- Tooltip shows zone rules

### Zone Brush Behavior
- Same brush mechanics as base game
- Custom zones paint with their color
- Mixed-use shows striped pattern

### Color System
| Zone Type | Color | Pattern |
|-----------|-------|---------|
| Mixed Low | Purple | Solid |
| Mixed Medium | Magenta | Solid |
| Mixed High | Deep Purple | Solid |
| Custom | User-defined | User-defined |

### Building Preview
When hovering over zone:
- Show sample buildings that can spawn
- Show restricted buildings (grayed out)
- Density range indicator

---

## 📅 Development Roadmap

### Week 1-2: Foundation
- [ ] Project setup
- [ ] Zone system research and hooks
- [ ] Custom zone data structures
- [ ] Basic zone registration

### Week 3-4: Core Zoning
- [ ] Mixed-use zone implementation
- [ ] Building filter system
- [ ] Zone palette UI extension
- [ ] Zone color rendering

### Week 5-6: Customization
- [ ] Custom zone creator UI
- [ ] Policy system framework
- [ ] 5 starter policies
- [ ] Settings persistence

### Week 7: Polish & Testing
- [ ] Density gradient tool
- [ ] Performance optimization
- [ ] Compatibility testing
- [ ] Documentation

---

## ⚠️ Risks & Mitigations

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| Breaking vanilla zoning | High | High | Extensive testing, don't modify core data |
| Building spawn issues | Medium | High | Fallback to default spawning |
| Save game compatibility | Medium | High | Version migration, careful serialization |
| Performance with many zones | Low | Medium | Cache zone lookups, lazy updates |

---

## 🔗 Dependencies

| Dependency | Version | Required |
|------------|---------|----------|
| BepInEx | 5.4.x | Yes |
| Harmony | 2.x | Yes |
| Gooee | Latest | Recommended |

---

## 🔄 Compatibility

### Must Work With
- Anarchy (place buildings in custom zones)
- Find It (find buildings for zones)
- Better Bulldozer

### Potential Conflicts
- Other zoning mods (rare currently)
- Building unlock mods

---

## 📚 Reference Materials

### Real-World Zoning Codes
- NYC Zoning Resolution
- Japanese Land Use Zones
- German Baunutzungsverordnung

### CS1 Mods to Study
- District Policies (Realistic Population)
- Building Themes

### Game Code to Study
- ZoneSystem implementation
- Building spawning logic

---

## ✅ Definition of Done

### MVP Complete When:
- [ ] 3 mixed-use zone types functional
- [ ] Custom zone creator works
- [ ] 5 district policies available
- [ ] Density gradient tool works
- [ ] Zone colors render correctly
- [ ] Buildings spawn according to rules
- [ ] Saves/loads properly
- [ ] Published on Thunderstore

### Quality Criteria
- No zone system breakage
- Buildings match zone rules
- Intuitive zone creation
- Performance acceptable
- Well-documented

