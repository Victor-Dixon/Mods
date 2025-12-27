# 🚀 Implementation Plan: Connecting to Game Systems

**Based on Research:** See `RESEARCH_FINDINGS.md` for full details

---

## ✅ What We Learned

1. **System Architecture:** CS2 uses Unity ECS/DOTS
2. **System Registration:** Must use Harmony patches to register systems
3. **System Access:** Use `World.GetExistingSystemManaged<T>()` to access game systems
4. **Unknown System Names:** We don't know exact names yet - need discovery

---

## 📋 Step-by-Step Implementation

### Phase 1: Discovery (Day 1)

**Goal:** Find out what systems are available in CS2

**Tasks:**
1. ✅ Create `SystemDiscoverySystem` that logs all systems
2. ✅ Create Harmony patch to register it
3. ✅ Build and test in-game
4. ✅ Review logs to find population/economy systems

**Files to Create:**
- `src/Systems/SystemDiscoverySystem.cs`
- `src/Patches/SystemRegistrationPatch.cs`

**Expected Output:** List of system names like:
- `Game.Simulation.PopulationSystem`
- `Game.Economy.EconomySystem`
- etc.

---

### Phase 2: Read One Value (Day 2)

**Goal:** Successfully read population from the game

**Tasks:**
1. Create `PopulationReaderSystem` 
2. Use discovered system name to access population
3. Log the value to verify it works
4. Test in-game with different city sizes

**Files to Create:**
- `src/Systems/PopulationReaderSystem.cs`

**Success Criteria:** 
- ✅ Population value logged correctly
- ✅ Value updates as city grows
- ✅ No crashes or errors

---

### Phase 3: Read All Data (Day 3-4)

**Goal:** Implement full `CityDataCollector` with real game hooks

**Tasks:**
1. Replace all `GetGameValue()` stubs in `CityDataCollector.cs`
2. Implement:
   - `CollectPopulationData()` - Real population system
   - `CollectEconomyData()` - Real economy system  
   - `CollectResourceData()` - Real resource system
   - `CollectMetricsData()` - Real metrics systems
3. Test each one individually
4. Integrate with `RegionalManager`

**Files to Modify:**
- `src/Services/CityDataCollector.cs`

**Success Criteria:**
- ✅ All data collection methods use real game systems
- ✅ Data updates correctly
- ✅ No performance issues

---

### Phase 4: Apply Effects (Day 5-6)

**Goal:** Implement real effect application

**Tasks:**
1. Discover how to modify game state (treasury, resources)
2. Implement `ApplyTreasuryChange()` - Real implementation
3. Implement `ApplyExportEffect()` / `ApplyImportEffect()` - Real implementation
4. Implement `ApplyWorkerChange()` - Real implementation
5. Test each effect individually

**Files to Modify:**
- `src/Services/RegionalEffectsApplicator.cs`

**Success Criteria:**
- ✅ Treasury changes visible in-game
- ✅ Resources update correctly
- ✅ Effects are reversible

---

## 🔧 Code Templates

### SystemDiscoverySystem Template

```csharp
using Game;
using Unity.Entities;
using UnityEngine.Scripting;
using BepInEx;

namespace CitiesRegional.Systems
{
    public class SystemDiscoverySystem : GameSystemBase
    {
        private bool _hasLogged = false;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            BepInEx.Logging.Log.Info("[CitiesRegional] SystemDiscoverySystem created");
        }
        
        protected override void OnUpdate()
        {
            // Only log once
            if (_hasLogged) return;
            _hasLogged = true;
            
            BepInEx.Logging.Log.Info("[CitiesRegional] === Discovering Game Systems ===");
            
            var allSystems = World.Systems;
            foreach (var system in allSystems)
            {
                var systemType = system.GetType();
                var name = systemType.Name;
                var fullName = systemType.FullName;
                
                // Log all systems, but highlight relevant ones
                if (name.Contains("Population") || 
                    name.Contains("Economy") ||
                    name.Contains("Resource") ||
                    name.Contains("Citizen") ||
                    name.Contains("Money") ||
                    name.Contains("Treasury"))
                {
                    BepInEx.Logging.Log.Info($"[CitiesRegional] ⭐ RELEVANT: {fullName}");
                }
                else
                {
                    BepInEx.Logging.Log.Debug($"[CitiesRegional] System: {fullName}");
                }
            }
            
            BepInEx.Logging.Log.Info("[CitiesRegional] === Discovery Complete ===");
        }
    }
}
```

### SystemRegistrationPatch Template

```csharp
using HarmonyLib;
using Game.Audio;
using Game.SceneFlow;
using CitiesRegional.Systems;

namespace CitiesRegional.Patches
{
    [HarmonyPatch(typeof(AudioManager), "OnGameLoadingComplete")]
    class SystemRegistrationPatch
    {
        static void Postfix(AudioManager __instance, 
                           Colossal.Serialization.Entities.Purpose purpose, 
                           GameMode mode)
        {
            if (!mode.IsGameOrEditor())
                return;

            // Register our systems
            __instance.World.GetOrCreateSystem<SystemDiscoverySystem>();
            // Add more systems here as we create them
        }
    }
}
```

### Update Plugin.cs

```csharp
// In CitiesRegionalPlugin.cs, add Harmony patching:
using HarmonyLib;
using System.Reflection;

private void Awake()
{
    Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

    // Apply all Harmony patches
    var harmony = Harmony.CreateAndPatchAll(
        Assembly.GetExecutingAssembly(), 
        MyPluginInfo.PLUGIN_GUID + "_Harmony"
    );
    
    // ... rest of initialization
}
```

---

## 🎯 Next Immediate Action

**Create the SystemDiscoverySystem now** to start discovering what's available in the game.

This will give us:
1. List of all systems
2. Exact names to use
3. Confidence that our approach works

Then we can proceed with reading actual data.

---

## 📚 Resources

- **Research Document:** `RESEARCH_FINDINGS.md`
- **Template Example:** `D:\mods\CS2-ModTemplate\`
- **Modding Discord:** https://discord.gg/vd7HXnpPJf

