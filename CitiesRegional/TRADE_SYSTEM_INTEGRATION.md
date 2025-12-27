# Trade System Integration - Implementation

**Date:** 2025-12-26  
**Status:** ✅ Complete - Ready for Testing

---

## Overview

Enhanced trade system integration to read actual trade data from Cities: Skylines 2's `TradeSystem` and use it to refine resource export/import calculations.

---

## ✅ Implementation Details

### 1. Enhanced `TryReadTradeData()` Method

**Location:** `src/Systems/CityDataEcsBridgeSystem.cs`

**Features:**
- Reads total export/import values from TradeSystem
- Attempts to read per-resource trade data
- Uses reflection to discover properties and methods
- Returns dictionaries of export/import by resource type
- Backward compatible with simplified overload

**Method Signature:**
```csharp
public bool TryReadTradeData(
    out float totalExportValue, 
    out float totalImportValue, 
    out Dictionary<string, float> exportByResource, 
    out Dictionary<string, float> importByResource)
```

**Discovery Strategy:**
1. Searches for properties containing "export"/"import" and "value"/"revenue"/"cost"
2. Attempts to enumerate collections that might contain resource-specific trade data
3. Tries methods that might calculate trade values
4. Falls back gracefully if data not available

### 2. Integration with Resource Collection

**Location:** `src/Systems/CityDataCollector.cs`

**Enhancements:**
- Calls `TryReadTradeData()` during resource collection
- Uses trade data to enhance industrial/commercial goods calculations
- Logs trade data when available
- Falls back to production/consumption calculations if trade data unavailable

**Resource Enhancement:**
- Industrial goods: Checks trade data for "industrial" or "manufactured" keywords
- Commercial goods: Checks trade data for "commercial" or "retail" keywords
- Uses maximum of calculated export/import and trade system values

---

## 📊 Data Flow

```
TradeSystem (Game)
    ↓
TryReadTradeData() (Reflection)
    ↓
exportByResource / importByResource (Dictionary)
    ↓
CollectResourceData() (Enhancement)
    ↓
ResourceData.ExportAvailable / ImportNeeded (Refined)
    ↓
RegionalCityData.Resources (Final)
```

---

## 🔍 Logging

**Info Level:**
- Total export/import values when trade data found
- Per-resource trade counts

**Debug Level:**
- Specific resource trade values (industrial/commercial)
- Trade data discovery details

**Example Logs:**
```
[CitiesRegional] Trade data: Export=$50,000, Import=$30,000
[CitiesRegional] Per-resource trade: 2 exports, 1 imports
[CitiesRegional] Trade system export for industrial goods: 500
```

---

## 🧪 Testing

### In-Game Testing

1. **Verify Trade Data Reading:**
   - Check logs for "Trade data: Export=..." messages
   - Verify values are reasonable
   - Confirm per-resource data appears if available

2. **Verify Resource Enhancement:**
   - Check that industrial/commercial goods export/import values
   - Compare with production/consumption calculations
   - Verify trade data takes precedence when available

3. **Verify Fallback:**
   - Test in city without active trade
   - Verify production/consumption calculations still work
   - Confirm no errors when TradeSystem unavailable

### Expected Behavior

- **With Trade Data:** Export/import values enhanced with actual trade system data
- **Without Trade Data:** Falls back to production/consumption calculations
- **Partial Data:** Uses available trade data, falls back for missing resources

---

## 🔧 Technical Details

### Reflection Strategy

1. **Property Discovery:**
   - Searches all properties on TradeSystem
   - Matches keywords: export, import, value, revenue, cost, total
   - Converts values to float

2. **Collection Enumeration:**
   - Attempts to enumerate collections/dictionaries
   - Extracts resource names and amounts
   - Categorizes as export or import

3. **Method Invocation:**
   - Tries methods with "get"/"calculate" and "export"/"import"/"trade"
   - Invokes parameterless methods
   - Converts results to float

### Error Handling

- All reflection operations wrapped in try-catch
- Logs warnings on failures
- Returns false if no data found
- Continues with fallback calculations

---

## 📝 Files Modified

1. **`src/Systems/CityDataEcsBridgeSystem.cs`**
   - Enhanced `TryReadTradeData()` with per-resource support
   - Added `Dictionary<string, float>` return values
   - Added collection enumeration logic
   - Added method invocation fallback

2. **`src/Systems/CityDataCollector.cs`**
   - Integrated trade data reading
   - Enhanced industrial goods with trade data
   - Enhanced commercial goods with trade data
   - Added trade data logging

---

## 🎯 Benefits

1. **More Accurate Trade Data:**
   - Uses actual game trade system values
   - Reflects real export/import activity
   - More accurate than production/consumption estimates

2. **Per-Resource Granularity:**
   - Can identify specific resource trade
   - Better matching for regional trade
   - More precise trade flow calculations

3. **Robust Fallback:**
   - Works even if TradeSystem unavailable
   - Graceful degradation
   - No breaking changes

---

## ⏳ Future Enhancements

1. **Trade Route Data:**
   - Read specific trade routes
   - Identify trading partners
   - Track trade history

2. **Resource Type Mapping:**
   - Map game resource types to our ResourceType enum
   - Handle all resource types, not just industrial/commercial

3. **Trade Value Calculation:**
   - Use actual trade prices
   - Calculate revenue/cost more accurately
   - Integrate with treasury changes

---

## 📚 Related Files

- `src/Systems/CityDataEcsBridgeSystem.cs` - Trade data reading
- `src/Systems/CityDataCollector.cs` - Resource collection integration
- `src/Models/RegionalCityData.cs` - Data models
- `MISSION_BRIEFING.md` - Project overview
- `PHASE2_PROGRESS.md` - Phase 2 progress

