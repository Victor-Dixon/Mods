# UI Framework Setup - Gooee/React for CS2

**Date:** 2025-12-26  
**Status:** ⏳ In Progress - Research Phase

---

## Overview

Setting up UI framework for CitiesRegional mod using Gooee (React-based UI framework for CS2 mods).

---

## Research Findings

### Gooee Framework

**What is Gooee?**
- Bootstrap-like CSS framework with React components
- Designed specifically for Cities: Skylines 2 modding
- Provides pre-built UI components (panels, buttons, inputs, etc.)
- Integrates with CS2's UI system

**Key Resources:**
- GitHub: https://github.com/gooee-framework/gooee
- Documentation: https://gooee.dev/
- Discord: Cities 2 Modding Discord

### Integration Pattern

```csharp
using Gooee;
using Gooee.Plugins;

public class CitiesRegionalUI : GooeePlugin
{
    public override string Name => "CitiesRegional";
    
    public override void OnSetup()
    {
        // Setup UI components
    }
}
```

---

## Implementation Plan

### Phase 1: Project Setup

1. **Add Gooee NuGet Package**
   - Add Gooee package reference to `CitiesRegional.csproj`
   - Version: Latest stable (check GitHub for current version)

2. **Create UI Project Structure**
   ```
   src/
     UI/
       Components/
         TradeDashboard.cs
         RegionPanel.cs
         ConnectionPanel.cs
       CitiesRegionalUI.cs (main plugin)
   ```

3. **Base UI Plugin**
   - Create `CitiesRegionalUI` class inheriting from `GooeePlugin`
   - Register with Gooee system
   - Create basic panel structure

### Phase 2: Trade Dashboard Panel

**Components Needed:**
- Trade statistics display
- Active trades list
- Trade history
- Connection status

**UI Structure:**
```
Trade Dashboard
├── Summary Section
│   ├── Total Trade Value
│   ├── Active Trades Count
│   └── Net Trade Balance
├── Active Trades List
│   ├── Trade Item (Resource, From, To, Amount, Value)
│   └── ...
└── Trade History
    ├── Historical trades
    └── Export button
```

### Phase 3: Region Panel

**Components Needed:**
- Region info display
- City list
- Connection map
- Join/Create region buttons

**UI Structure:**
```
Region Panel
├── Region Info
│   ├── Region Name
│   ├── Region Code
│   └── City Count
├── Cities List
│   ├── City Card (Name, Population, Status)
│   └── ...
├── Connections Map
│   └── Visual connection display
└── Actions
    ├── Create Region
    ├── Join Region
    └── Leave Region
```

---

## Next Steps

1. **Research Gooee API**
   - Check GitHub for latest API documentation
   - Review example mods using Gooee
   - Understand component lifecycle

2. **Add Gooee Package**
   - Add NuGet package reference
   - Verify compatibility with .NET Standard 2.1

3. **Create Base UI Plugin**
   - Implement `CitiesRegionalUI` class
   - Create basic panel structure
   - Test UI appears in-game

4. **Implement Trade Dashboard**
   - Create trade statistics components
   - Implement active trades list
   - Add trade history display

5. **Implement Region Panel**
   - Create region info display
   - Implement city list
   - Add connection visualization

---

## Dependencies

**Required:**
- Gooee NuGet package
- React (included with Gooee)
- CS2 game assemblies

**Optional:**
- Custom CSS for styling
- Icons/assets for UI elements

---

## Status

**Current:** Research phase - Need to verify Gooee package availability and API

**Next:** Add Gooee package and create base UI plugin

---

## Notes

- Gooee may require specific CS2 version
- UI components may need to be registered with game's UI system
- Consider performance impact of UI updates
- Test UI in-game to verify integration

---

**UI Framework Setup - Ready for Implementation** 🎨

