# Test Scenarios - CitiesRegional Mod

**Date:** 2025-12-26  
**Status:** ✅ Complete - Ready for Use

---

## Overview

This document describes test scenarios for validating the CitiesRegional mod behavior in-game. These scenarios can be used for manual testing or automated validation via log analysis.

---

## Scenario 1: OnUpdate Execution Verification

### Objective
Verify that `CityDataEcsBridgeSystem.OnUpdate()` is being called correctly.

### Test Steps
1. Load a city in Cities: Skylines 2
2. Wait for city to fully load (1-2 seconds)
3. Check logs for first call confirmation

### Expected Log Patterns
```
*** [ECS] OnUpdate called for the first time! ***
[CitiesRegional] *** ECS Bridge OnUpdate FIRST CALL ***
```

### Validation Criteria
- ✅ First call log appears within 1-2 seconds of city load
- ✅ No errors in logs
- ✅ System is enabled (`Enabled = true`)

### Automated Test
- `LogBasedValidationTests.FirstCall_ShouldBeDetected()`

---

## Scenario 2: Heartbeat Verification

### Objective
Verify that heartbeat logging is working correctly.

### Test Steps
1. Load a city
2. Wait for at least 10 seconds
3. Check logs for heartbeat entries

### Expected Log Patterns
```
[ECS] Heartbeat: Frame=512, Enabled=True, Pop=52340
[CitiesRegional] ECS Heartbeat: Frame=512, Enabled=True, Pop=52340
```

### Validation Criteria
- ✅ Heartbeat logs appear every ~8.5 seconds (512 frames)
- ✅ Population values are non-zero (if city has population)
- ✅ Enabled status is `True`
- ✅ Frame numbers increment correctly

### Automated Test
- `LogBasedValidationTests.Heartbeats_ShouldBePresent()`
- `LogBasedValidationTests.HeartbeatFrequency_ShouldBeApproximately512Frames()`

---

## Scenario 3: Data Collection Accuracy

### Objective
Verify that data collection is working correctly and updating regularly.

### Test Steps
1. Load a city with population
2. Wait for at least 5 seconds
3. Check logs for data update entries

### Expected Log Patterns
```
[ECS] Data Update: Pop=52340, Treasury=$1,500,000, Happiness=78.5, Frame=256
[CitiesRegional] ECS Update: Pop=52340, Treasury=$1,500,000, Frame=256
```

### Validation Criteria
- ✅ Data update logs appear every ~4 seconds (256 frames)
- ✅ Population values are non-negative
- ✅ Treasury values are reasonable (not negative, not astronomical)
- ✅ Data updates occur after first call

### Automated Test
- `LogBasedValidationTests.DataUpdates_ShouldBePresent()`
- `DataCollectionIntegrationTests.DataCollection_ShouldStartAfterFirstCall()`
- `DataCollectionIntegrationTests.DataCollection_ShouldUpdateRegularly()`

---

## Scenario 4: Trade System Integration

### Objective
Verify that trade system integration is working correctly.

### Test Steps
1. Load a city with active trade
2. Wait for data collection
3. Check logs for trade data entries

### Expected Log Patterns
```
[CitiesRegional] Trade data: Export=$50,000, Import=$30,000
[CitiesRegional] Per-resource trade: 2 exports, 1 imports
[CitiesRegional] Trade system export for industrial goods: 500
```

### Validation Criteria
- ✅ Trade data appears in logs if trade is active
- ✅ Export/import values are non-negative
- ✅ Trade data enhances resource calculations
- ✅ Fallback to production/consumption if trade data unavailable

### Automated Test
- `DataCollectionIntegrationTests.TradeData_ShouldBePresentIfAvailable()`
- `TradeDataValidationTests.*`

---

## Scenario 5: Data Range Validation

### Objective
Verify that all data values are within expected ranges.

### Test Steps
1. Collect data from logs or in-game
2. Validate data ranges

### Validation Criteria
- ✅ Metrics (happiness, health, education, etc.): 0-100
- ✅ Population counts: >= 0
- ✅ Treasury: Reasonable range (not negative, not astronomical)
- ✅ Export/import values: >= 0

### Automated Test
- `DataRangeValidationTests.*`

---

## Scenario 6: Data Consistency Validation

### Objective
Verify that data relationships are consistent.

### Test Steps
1. Collect data from logs or in-game
2. Validate data consistency

### Validation Criteria
- ✅ Export available <= Production
- ✅ Import needed <= Consumption
- ✅ Workers <= Population
- ✅ Workers + Unemployed <= Population
- ✅ Average household size: 1.5-4 people

### Automated Test
- `DataConsistencyTests.*`

---

## Scenario 7: System Discovery

### Objective
Verify that game systems are discovered correctly.

### Test Steps
1. Load a city
2. Check discovery logs

### Expected Log Patterns
```
[ECS] Starting data collection
[ECS] Systems discovered: CitySystem, EconomySystem, TradeSystem
```

### Validation Criteria
- ✅ Systems are discovered
- ✅ No discovery errors
- ✅ Data collection can proceed

### Automated Test
- Manual validation (check logs)

---

## Scenario 8: Effect Application

### Objective
Verify that regional effects are applied correctly.

### Test Steps
1. Load multiple cities in region
2. Verify effects are calculated
3. Check effect application logs

### Validation Criteria
- ✅ Effects are calculated based on regional data
- ✅ Effects are applied to cities
- ✅ No errors during effect application

### Automated Test
- Manual validation (requires multiple cities)

---

## Edge Cases

### Empty City
- **Test:** Load a new city with 0 population
- **Expected:** Data collection should still work, population = 0
- **Validation:** No errors, data values are 0 or reasonable defaults

### Negative Treasury
- **Test:** Load a city with negative treasury (debt)
- **Expected:** Treasury value should be negative but within reasonable bounds
- **Validation:** Treasury >= -1,000,000,000

### Extreme Values
- **Test:** Load a city with very high population (>1M)
- **Expected:** All calculations should handle large values
- **Validation:** No overflow errors, values are reasonable

### Missing Systems
- **Test:** Test in environment where some systems are unavailable
- **Expected:** Graceful fallback, no crashes
- **Validation:** Errors are logged but mod continues to work

---

## Log Locations

### Player.log (Unity)
```
%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Player.log
```

### BepInEx Log
```
%USERPROFILE%\AppData\LocalLow\BepInEx\LogOutput.log
```

### Discovery Log
```
%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Logs\CitiesRegional_Discovery.txt
```

---

## Running Automated Tests

### Run All Integration Tests
```bash
cd CitiesRegional.Tests
dotnet test --filter "FullyQualifiedName~IntegrationTests"
```

### Run Log-Based Validation Tests
```bash
dotnet test --filter "FullyQualifiedName~LogBasedValidationTests"
```

### Run Data Collection Tests
```bash
dotnet test --filter "FullyQualifiedName~DataCollectionIntegrationTests"
```

### Run Validation Tests
```bash
dotnet test --filter "FullyQualifiedName~ValidationTests"
```

---

## Troubleshooting

### Tests Fail Because Logs Don't Exist
- **Solution:** Run the game and load a city first to generate logs
- **Alternative:** Tests will skip gracefully if logs don't exist

### Tests Fail Because No Data in Logs
- **Solution:** Ensure mod is installed and enabled
- **Check:** Verify mod DLL is in BepInEx plugins folder
- **Verify:** Check that systems are being discovered

### Tests Pass But Game Doesn't Work
- **Note:** Tests validate logic and logs, not actual game integration
- **Action:** Always test in-game for final verification
- **Check:** Review game logs for runtime errors

---

## Related Documentation

- `TESTING.md` - General testing guide
- `ONUPDATE_VERIFICATION.md` - OnUpdate verification details
- `TRADE_SYSTEM_INTEGRATION.md` - Trade system integration details
- `AGENT1_COORDINATION_PACKAGE.md` - Coordination package

---

**Ready for testing!** 🚀

