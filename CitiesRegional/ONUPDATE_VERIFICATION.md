# OnUpdate Verification - Enhanced Logging

**Date:** 2025-12-26  
**Status:** ✅ Enhanced - Ready for Testing

---

## Overview

Enhanced `CityDataEcsBridgeSystem.OnUpdate()` with comprehensive logging to verify execution and data collection.

---

## ✅ Enhancements Made

### 1. High-Visibility First Call Logging
- Logs to both BepInEx log and Unity Debug.Log
- Uses `***` markers for easy searching
- Confirms OnUpdate is being called

### 2. Heartbeat Logging
- Logs every 512 frames (~8.5 seconds)
- Shows: Frame number, Enabled status, Current population
- Visible in both log files for verification

### 3. Enhanced Update Cycle Logging
- Changed from `LogDebug` to `LogInfo` for visibility
- Also logs to Unity Debug.Log
- Shows key metrics: Population, Treasury, Happiness, Frame

### 4. First Data Collection Confirmation
- Logs when first data collection completes
- Uses high-visibility markers
- Confirms systems are discovered and data is collected

### 5. Error Visibility
- Changed warnings to errors for critical failures
- Logs to both BepInEx and Unity Debug.Log
- Easier to spot issues

---

## 📝 Log Output Examples

### First Call
```
*** [ECS] OnUpdate called for the first time! ***
[CitiesRegional] *** ECS Bridge OnUpdate FIRST CALL ***
```

### Heartbeat (every 512 frames)
```
[ECS] Heartbeat: Frame=512, Enabled=True, Pop=52340
[CitiesRegional] ECS Heartbeat: Frame=512, Enabled=True, Pop=52340
```

### Data Update (every 256 frames)
```
[ECS] Data Update: Pop=52340, Treasury=$1,500,000, Happiness=78.5, Frame=256
[CitiesRegional] ECS Update: Pop=52340, Treasury=$1,500,000, Frame=256
```

### First Collection Complete
```
*** [ECS] First data collection complete! Frame=256 ***
[CitiesRegional] *** First data collection complete! Frame=256 ***
```

---

## 🔍 Where to Check Logs

### BepInEx Log
```
%USERPROFILE%\AppData\LocalLow\BepInEx\LogOutput.log
```

### Unity Player.log
```
%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Player.log
```

### Search Terms
- `*** ECS Bridge OnUpdate FIRST CALL ***` - First execution
- `ECS Heartbeat` - Regular execution confirmation
- `ECS Data Update` - Data collection cycles
- `First data collection complete` - Initial collection done

---

## ✅ Verification Checklist

When testing in-game, verify:

- [ ] First call log appears when city loads
- [ ] Heartbeat logs appear every ~8.5 seconds
- [ ] Data update logs appear every ~4 seconds
- [ ] Population values are non-zero (if city has population)
- [ ] Treasury values are reasonable
- [ ] No error messages in logs

---

## 🐛 Troubleshooting

### No Logs Appearing

1. **Check System Registration:**
   - Look for `*** SUCCESS: Systems registered and enabled! ***` in logs
   - Verify systems are enabled: `Enabled = true`

2. **Check System Update Group:**
   - System may be in wrong update group
   - May need to explicitly set update group

3. **Check RequireForUpdate:**
   - Removed `RequireForUpdate(_citizenQuery)` to ensure OnUpdate runs
   - System should run even if queries are empty

4. **Check SimulationSystem:**
   - If `SimulationSystem not available` appears, game may not be fully loaded
   - Wait for city to fully load before checking

### Logs Appear But No Data

1. **Check System Discovery:**
   - Look for `[ECS] Starting data collection` log
   - Verify systems are discovered (check discovery log)

2. **Check Entity Queries:**
   - Population may be 0 if city is new
   - Queries may need entities to exist first

3. **Check Reflection Access:**
   - Some systems may not have expected properties
   - Check for reflection errors in logs

---

## 📊 Expected Behavior

### Normal Operation
- First call log appears within 1-2 seconds of city load
- Heartbeat every ~8.5 seconds
- Data updates every ~4 seconds
- Population increases as city grows
- Treasury changes as economy runs

### Performance Impact
- Minimal: Logging every 256-512 frames
- No impact on game performance
- Can be reduced if needed

---

## 🔧 Future Enhancements

1. **Configurable Logging:**
   - Add config option to enable/disable verbose logging
   - Allow adjusting heartbeat frequency

2. **Performance Metrics:**
   - Track OnUpdate execution time
   - Log if updates are taking too long

3. **Health Check:**
   - Detect if OnUpdate stops being called
   - Alert if no updates for extended period

---

## 📚 Related Files

- `src/Systems/CityDataEcsBridgeSystem.cs` - Main implementation
- `src/Patches/SystemRegistrationPatch.cs` - System registration
- `MISSION_BRIEFING.md` - Project overview
- `PHASE2_PROGRESS.md` - Phase 2 progress

