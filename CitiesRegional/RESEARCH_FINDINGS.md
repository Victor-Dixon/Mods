# 🔍 CS2 Modding Research Findings

**Date:** 2025-12-25  
**Purpose:** Understand how to access CS2 game systems for data collection and modification

---

## 📚 Key Resources Found

### 1. Official Modding Resources
- **Cities 2 Modding Wiki:** https://wiki.ciim.dev/ (redirects, but referenced in many places)
- **Modding Template:** https://github.com/Captain-Of-Coit/cities-skylines-2-mod-template
- **Cities 2 Modding Discord:** https://discord.gg/vd7HXnpPJf
- **Thunderstore:** https://thunderstore.io/c/cities-skylines-ii/

### 2. UI Frameworks
- **Gooee:** Bootstrap-like CSS framework with React components
- **HookUI:** Library for integrating custom UI components

---

## 🏗️ Architecture Patterns

### System Registration Pattern

CS2 mods use **ECS (Entity Component System)** architecture. Here's how to create and register systems:

```csharp
using Game;
using Unity.Entities;
using UnityEngine.Scripting;

namespace MyMod.Systems
{
    // 1. Create a system that inherits from GameSystemBase
    public class MyDataCollectorSystem : GameSystemBase
    {
        // Reference to other game systems
        private SimulationSystem simulation;
        // private PopulationSystem population; // Example
        
        protected override void OnCreate()
        {
            base.OnCreate();
            
            // 2. Get existing game systems using World.GetExistingSystemManaged<T>()
            // this.simulation = World.GetExistingSystemManaged<SimulationSystem>();
            // this.population = World.GetExistingSystemManaged<PopulationSystem>();
        }
        
        protected override void OnUpdate()
        {
            // This runs every frame
            // Use sparingly - only for real-time updates
        }
    }
}
```

### Harmony Patch Pattern for System Registration

Systems must be registered via Harmony patches that hook into game lifecycle events:

```csharp
using HarmonyLib;
using Game.Audio;
using Game.SceneFlow;
using MyMod.Systems;

namespace MyMod.Patches
{
    // Hook into AudioManager.OnGameLoadingComplete to register our system
    [HarmonyPatch(typeof(AudioManager), "OnGameLoadingComplete")]
    class AudioManager_OnGameLoadingComplete
    {
        static void Postfix(AudioManager __instance, 
                           Colossal.Serialization.Entities.Purpose purpose, 
                           GameMode mode)
        {
            // Only register in game/editor mode, not in menu
            if (!mode.IsGameOrEditor())
                return;

            // Register our custom system with the game's ECS World
            __instance.World.GetOrCreateSystem<MyDataCollectorSystem>();
        }
    }
}
```

### Plugin Entry Point Pattern

```csharp
using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace MyMod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            // Apply all Harmony patches in this assembly
            var harmony = Harmony.CreateAndPatchAll(
                Assembly.GetExecutingAssembly(), 
                MyPluginInfo.PLUGIN_GUID + "_Cities2Harmony"
            );
        }
    }
}
```

---

## 🎯 Key Game Systems to Access

Based on the template and PRD research, these are the systems we need to access:

| System Type | Purpose | How to Access |
|-------------|---------|---------------|
| `SimulationSystem` | Core simulation | `World.GetExistingSystemManaged<SimulationSystem>()` |
| `PopulationSystem` | Population data | `World.GetExistingSystemManaged<PopulationSystem>()` |
| `EconomySystem` | Treasury, income, expenses | `World.GetExistingSystemManaged<EconomySystem>()` |
| `ResourceSystem` | Resource production/consumption | `World.GetExistingSystemManaged<ResourceSystem>()` |
| `TrafficSystem` | Traffic flow, congestion | `World.GetExistingSystemManaged<TrafficSystem>()` |
| `ServiceCoverageSystem` | Service effectiveness | `World.GetExistingSystemManaged<ServiceCoverageSystem>()` |
| `PollutionSystem` | Pollution levels | `World.GetExistingSystemManaged<PollutionSystem>()` |
| `LandValueSystem` | Property values | `World.GetExistingSystemManaged<LandValueSystem>()` |

**⚠️ Note:** These system names are **educated guesses** based on CS1 patterns and PRD research. We need to:
1. Use reflection to discover actual system names
2. Check existing mods for examples
3. Ask the Discord community

---

## 🔧 Project Structure Pattern

From the template:

```
MyMod/
├── Plugin.cs                    # BepInEx entry point
├── MyMod.csproj                 # Project file with game DLL references
├── Patches/
│   └── MyModPatches.cs         # Harmony patches for system registration
└── Systems/
    └── MyModSystem.cs          # Custom ECS systems
```

---

## 📦 DLL References Pattern

From the template's `.csproj`:

```xml
<PropertyGroup>
    <Cities2_Location>D:\mods\CS2\Cities.Skylines.II.v1.5.3f1\game</Cities2_Location>
</PropertyGroup>

<ItemGroup>
    <!-- Reference all Colossal DLLs -->
    <Reference Include="$(Cities2_Location)\Cities2_Data\Managed\Colossal.*.dll" Private="False"/>
    
    <!-- Reference Game.dll (main game logic) -->
    <Reference Include="$(Cities2_Location)\Cities2_Data\Managed\Game.dll" Private="False"/>
    
    <!-- Reference Unity DLLs -->
    <Reference Include="$(Cities2_Location)\Cities2_Data\Managed\Unity.*.dll" Private="False"/>
</ItemGroup>
```

**Key Point:** `Private="False"` means DLLs are not copied to output - they're loaded from the game directory at runtime.

---

## 🔍 Discovery Strategy

Since we don't know the exact system names yet, here's how to discover them:

### Method 1: Reflection-Based Discovery

```csharp
// In your system's OnCreate():
var allSystems = World.Systems;
foreach (var system in allSystems)
{
    Logger.LogInfo($"Found system: {system.GetType().FullName}");
}
```

### Method 2: Search Game.dll with ILSpy/DnSpy

1. Open `Game.dll` in ILSpy or DnSpy
2. Search for classes containing "System" and "Population" or "Economy"
3. Look for classes inheriting from `SystemBase` or `GameSystemBase`

### Method 3: Check Existing Mods

Search GitHub for:
- `"Cities Skylines 2" mod population`
- `"Cities Skylines 2" mod economy`
- `"Cities Skylines 2" mod "GameSystemBase"`

### Method 4: Ask Discord

Post on Cities 2 Modding Discord:
- "How do I read population from CS2?"
- "What system handles economy data?"
- "How do I access city treasury?"

---

## 🎨 UI Integration Patterns

### Gooee Framework

```csharp
// Example from research
using Gooee;

public class MyModUI : GooeePlugin
{
    public override string Name => "My Mod";
    
    public override void OnSetup()
    {
        // Setup UI components
    }
}
```

### HookUI Framework

```csharp
// Example pattern (needs verification)
using HookUI;

public class MyModPanel : HookUIPanel
{
    // Create React-based UI
}
```

---

## ⚠️ Important Notes

1. **System Names Are Unknown:** We don't know the exact names of `PopulationSystem`, `EconomySystem`, etc. They might be:
   - `Game.Simulation.PopulationSystem`
   - `Game.Economy.EconomySystem`
   - Or completely different names

2. **ECS Architecture:** CS2 uses Unity ECS/DOTS, which is different from CS1's object-oriented approach

3. **Thread Safety:** ECS systems run on different threads - be careful with shared state

4. **Game Updates:** System names/APIs may change with game updates - use abstractions

---

## 🚀 Next Steps

1. **Create Test System:** Build a simple system that logs all available systems
2. **Discover System Names:** Use reflection to find population/economy systems
3. **Read One Value:** Start with reading population (one value)
4. **Verify Approach:** Test in-game to ensure it works
5. **Scale Up:** Once one value works, expand to all needed data

---

## 📝 Code Snippets to Try

### Test System to Discover Available Systems

```csharp
using Game;
using Unity.Entities;
using UnityEngine.Scripting;

namespace CitiesRegional.Systems
{
    public class SystemDiscoverySystem : GameSystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            
            // Log all systems
            var allSystems = World.Systems;
            foreach (var system in allSystems)
            {
                var systemType = system.GetType();
                if (systemType.Name.Contains("Population") || 
                    systemType.Name.Contains("Economy") ||
                    systemType.Name.Contains("Resource"))
                {
                    UnityEngine.Debug.Log($"[CitiesRegional] Found relevant system: {systemType.FullName}");
                }
            }
        }
        
        protected override void OnUpdate() { }
    }
}
```

### Harmony Patch to Register Discovery System

```csharp
using HarmonyLib;
using Game.Audio;
using Game.SceneFlow;
using CitiesRegional.Systems;

namespace CitiesRegional.Patches
{
    [HarmonyPatch(typeof(AudioManager), "OnGameLoadingComplete")]
    class RegisterSystemsPatch
    {
        static void Postfix(AudioManager __instance, 
                           Colossal.Serialization.Entities.Purpose purpose, 
                           GameMode mode)
        {
            if (!mode.IsGameOrEditor())
                return;

            __instance.World.GetOrCreateSystem<SystemDiscoverySystem>();
        }
    }
}
```

---

## 🔗 References

- [CS2 Mod Template](https://github.com/Captain-Of-Coit/cities-skylines-2-mod-template)
- [Cities 2 Modding Wiki](https://wiki.ciim.dev/)
- [Cities 2 Modding Discord](https://discord.gg/vd7HXnpPJf)
- [BepInEx Docs](https://docs.bepinex.dev/)
- [Harmony Docs](https://harmony.pardeike.net/)
- [Unity ECS Documentation](https://docs.unity3d.com/Packages/com.unity.entities@latest)

---

**Status:** Research complete. Ready to implement discovery system and start accessing game data.

