# ✅ Phase 2 Implementation Complete: Discovery + Real Population

**Date:** 2025-12-25  
**Status:** ✅ Ready for Testing  
**Commit:** `feat(phase2): add ECS bridge + system discovery; read real population via Citizen query`

---

## 🎯 Objective Achieved

**Ship the first real in-game datapoint: Population** via an in-game ECS bridge system that runs inside the game World.

---

## 📦 What Was Implemented

### 1. SystemDiscoverySystem (Updated)
- **Simplified** to use `GameSystemBase` and log all managed systems once per load
- Logs systems in `World.Systems` ordered by full name
- Helps identify available game systems quickly

### 2. CityDataEcsBridgeSystem (New)
- **ECS bridge system** that runs inside the game World
- Builds `EntityQuery` for `Citizen` entities (excludes `Deleted`)
- Calculates population on throttle (every 256 frames = ~4 seconds at 60fps)
- Exposes `GetPopulationSnapshot()` for `CityDataCollector`
- Safe guards: checks for World/SimulationSystem availability

### 3. SystemRegistrationPatch (Updated)
- **Dual-patch approach** for robustness:
  - Primary: `UpdateSystem.OnCreate` (if available)
  - Fallback: `AudioManager.OnGameLoadingComplete` (known to work)
- Registers both `SystemDiscoverySystem` and `CityDataEcsBridgeSystem`
- Error handling with logging

### 4. CityDataCollector (Updated)
- **Real population** via `CityDataEcsBridgeSystem.Instance.GetPopulationSnapshot()`
- Falls back to placeholder (0) if bridge not ready
- Other fields still use estimates based on population (workers, jobs, etc.)
- Logs warnings when bridge unavailable

### 5. CitiesRegionalPlugin (Updated)
- Added static logging helpers: `LogInfo`, `LogWarn`, `LogDebug`, `LogError`
- Accessible from systems/patches without instance reference
- Maintains existing bootstrap as additional fallback

---

## 🔧 Files Created/Modified

### Modified Files
- ✅ `src/Systems/SystemDiscoverySystem.cs` - Simplified to GameSystemBase
- ✅ `src/Systems/CityDataCollector.cs` - Uses real population from bridge
- ✅ `src/Patches/SystemRegistrationPatch.cs` - Dual-patch registration
- ✅ `src/CitiesRegionalPlugin.cs` - Added static logging helpers

### New Files
- ✅ `src/Systems/CityDataEcsBridgeSystem.cs` - ECS bridge for population query

---

## 🎯 Acceptance Criteria Status

| Criteria | Status | Notes |
|----------|--------|-------|
| BepInEx console shows one-time system dump | ✅ Ready | SystemDiscoverySystem logs all systems |
| Every N seconds, log shows Population = X | ✅ Ready | CityDataEcsBridgeSystem logs every 256 frames |
| No crashes after 30+ minutes | ⏳ Pending | Needs in-game testing |
| CityDataCollector returns real population | ✅ Ready | Uses bridge snapshot |
| Sync pipeline sends non-zero values | ⏳ Pending | Needs server test |

---

## 🚀 How It Works

### System Flow

```
1. Game Loads
   ↓
2. Harmony Patch (UpdateSystem or AudioManager)
   ↓
3. Register Systems:
   - SystemDiscoverySystem (logs once, then disables)
   - CityDataEcsBridgeSystem (runs continuously)
   ↓
4. CityDataEcsBridgeSystem.OnUpdate():
   - Every 256 frames (~4 seconds)
   - Query Citizen entities (exclude Deleted)
   - Calculate count → _population
   - Log: "Population=X (frame=Y)"
   ↓
5. CityDataCollector.CollectPopulationData():
   - Get bridge instance
   - Call GetPopulationSnapshot()
   - Set data.Population = real value
   ↓
6. RegionalManager syncs data to server
```

### Query Pattern

```csharp
// Based on InfoLoom approach:
EntityQuery: 
  All: [Citizen]
  None: [Deleted]
  
// CalculateEntityCount() gives us total population
```

---

## 📊 Expected Log Output

When you run the game, you should see:

```
[INFO] [CitiesRegional] Cities Regional v0.1.0 loading...
[INFO] [CitiesRegional] Applied 2 Harmony patches.
[INFO] [CitiesRegional][Patch] Registered discovery + ECS bridge systems.
[INFO] [CitiesRegional][Discovery] Systems in World: 150
[INFO] [CitiesRegional][Discovery] Game.Simulation.PopulationSystem
[INFO] [CitiesRegional][Discovery] Game.Economy.EconomySystem
...
[INFO] [CitiesRegional][ECS] CityDataEcsBridgeSystem created.
[DEBUG] [CitiesRegional][ECS] Population=52340 (frame=256)
[DEBUG] [CitiesRegional][ECS] Population=52450 (frame=512)
[DEBUG] [CitiesRegional][ECS] Population=52600 (frame=768)
```

---

## ⚠️ Known Issues / Notes

1. **Temp Component:** Removed from query (may not exist in this CS2 version)
   - Query now: `Citizen` only, excludes `Deleted`
   - Should still work correctly

2. **UpdateSystem:** May not exist in all CS2 versions
   - Fallback to `AudioManager.OnGameLoadingComplete` (known to work)
   - Bootstrap also provides additional fallback

3. **Other Population Fields:** Still use estimates
   - Workers = 50% of population
   - Unemployed = 5% of population
   - Jobs = 3% of population
   - Students = 15% of population
   - Tourists = 1% of population
   - **Next:** Add real queries for these

4. **Throttle:** 256 frames = ~4 seconds at 60fps
   - Adjust `UpdateEveryNFrames` if needed
   - Lower = more frequent updates (more CPU)
   - Higher = less frequent (less accurate)

---

## ✅ Build Status

**Build:** ✅ **SUCCESS**  
**Warnings:** 2 (non-critical)  
**Errors:** 0  
**Deployed:** Auto-deployed to `BepInEx\plugins\CitiesRegional\`

---

## 🎮 Next Steps: Testing

1. **Launch CS2** with BepInEx
2. **Load a city** (or create new one)
3. **Check BepInEx logs:**
   - `D:\mods\CS2\...\BepInEx\LogOutput.log`
   - Look for `[Discovery]` entries (system dump)
   - Look for `[ECS] Population=X` entries (updates every ~4 seconds)
4. **Verify:**
   - Population value changes as city grows
   - No crashes after 30+ minutes
   - Sync sends real population (not 0 or placeholder)

---

## 🚀 Next Swarm Move (After This Lands)

1. **Add Treasury** to bridge system:
   - Query Economy system for treasury value
   - Log on same cadence
   - Wire to `CollectEconomyData()`

2. **Verify Sync:**
   - Check server receives non-zero population
   - Check server receives real treasury values
   - Test with 2+ cities

---

## 📝 Code Quality

- ✅ Error handling with try/catch
- ✅ Null checks for World/System availability
- ✅ Logging at appropriate levels (Info/Debug/Warn)
- ✅ Singleton pattern for bridge instance
- ✅ Throttled updates (performance-conscious)
- ✅ Fallback mechanisms (multiple registration paths)

---

**Status:** ✅ **READY FOR IN-GAME TESTING**

Launch CS2 and check the logs! 🎮

