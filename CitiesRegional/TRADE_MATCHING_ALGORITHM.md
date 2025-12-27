# Trade Matching Algorithm - Implementation

**Date:** 2025-12-26  
**Status:** ✅ Complete - Ready for Testing

---

## Overview

Enhanced trade matching algorithm that considers connections, capacity constraints, distance limits, and optimizes trade flows between cities.

---

## ✅ Implementation Details

### Enhanced `CalculateTradeFlows()` Method

**Location:** `src/Models/Region.cs`

**Features:**
- Connection-based matching (only trades between connected cities)
- Capacity constraint handling
- Distance/travel time limits
- Priority-based optimization
- Multi-resource support
- Configurable constraints

**Method Signatures:**
```csharp
// Default: 60-minute travel time limit, 85% capacity utilization
public List<TradeFlow> CalculateTradeFlows()

// Configurable constraints
public List<TradeFlow> CalculateTradeFlows(float maxTravelTimeMinutes, float capacityUtilizationLimit = 0.85f)
```

---

## 🔍 Algorithm Flow

### 1. Resource Type Iteration
For each resource type (Industrial Goods, Commercial Goods, Electricity, Water, etc.):

### 2. Find Exporters and Importers
- **Exporters:** Cities with `ExportAvailable > 0`
  - Sorted by export availability (largest first)
- **Importers:** Cities with `ImportNeeded > 0`
  - Sorted by import need (largest first)

### 3. Create Potential Matches
For each exporter-importer pair:
- ✅ Check if connection exists
- ✅ Check travel time limit (default: 60 minutes)
- ✅ Calculate potential trade amount
- ✅ Check connection capacity availability
- ✅ Calculate priority score

### 4. Priority-Based Matching
Matches are sorted by priority score (higher = better):
- **Distance:** Shorter travel time = higher priority
- **Capacity:** Higher capacity connections = higher priority
- **Congestion:** Less congested = higher priority
- **Price:** Better prices = higher priority
- **Scale:** Larger trade amounts = higher priority

### 5. Process Matches
Process matches in priority order:
- Check remaining needs and available exports
- Check connection capacity
- Calculate actual trade amount (respecting all constraints)
- Create trade flow
- Update tracking (remaining needs, exports, connection usage)

---

## 📊 Priority Calculation

**Formula:**
```
Priority = 100 (base)
         + (60 - travelTime) * 2 (distance bonus)
         + capacity / 100 (capacity bonus)
         + (1 - congestion) * 20 (congestion penalty)
         + price / 10 (price bonus)
         + tradeAmount / 100 (scale bonus)
```

**Factors:**
- **Distance:** Up to 120 points (60 minutes * 2)
- **Capacity:** Up to ~100 points (1000 capacity / 100)
- **Congestion:** Up to 20 points
- **Price:** Up to ~10 points (100 price / 10)
- **Scale:** Up to ~100 points (10000 units / 100)

**Total Range:** ~100 to ~350 points

---

## 🔧 Capacity Constraints

### Connection Capacity
- **Default Utilization:** 85% of connection capacity
- **Capacity Conversion:** 1 connection capacity = 10 resource units
- **Tracking:** Connection usage tracked across all resources
- **Limits:** Trade amount limited by available capacity

### Example
- Connection capacity: 1000 units/hour
- Utilization limit: 85% = 850 units/hour
- Current usage: 200 units
- Available: 650 units
- Capacity limit: 650 * 10 = 6500 resource units

---

## 📈 Enhanced TradeFlow Model

**New Properties:**
- `ConnectionId` - Connection used for trade
- `TravelTimeMinutes` - Travel time for this trade
- `CalculatedAt` - Timestamp when trade was calculated

**Benefits:**
- Track which connections are used
- Analyze trade efficiency
- Debug trade flow issues
- Historical trade analysis

---

## 🎯 Key Features

### 1. Connection-Based Trading
- Only trades between connected cities
- Respects connection existence
- Uses connection capacity

### 2. Distance Limits
- Configurable travel time limit (default: 60 minutes)
- Prevents unrealistic long-distance trades
- Encourages infrastructure investment

### 3. Capacity Management
- Tracks connection usage across resources
- Prevents over-capacity trading
- Supports multiple resources on same connection

### 4. Priority Optimization
- Matches best exporter-importer pairs first
- Considers multiple factors
- Maximizes trade efficiency

### 5. Multi-Resource Support
- Handles all resource types
- Independent capacity tracking per connection
- Simultaneous multi-resource trading

---

## 🧪 Test Scenarios

### Scenario 1: Basic Trade Flow
- City A exports 500 industrial goods
- City B imports 300 industrial goods
- Connection exists, capacity available
- **Expected:** 300 units traded

### Scenario 2: Capacity Constrained
- City A exports 1000 units
- Connection capacity: 500 units (85% of 588)
- **Expected:** Only 500 units traded

### Scenario 3: No Connection
- City A exports 500 units
- City B imports 300 units
- No connection exists
- **Expected:** No trade

### Scenario 4: Distance Limit
- City A exports 500 units
- City B imports 300 units
- Connection exists but travel time: 90 minutes
- Max travel time: 60 minutes
- **Expected:** No trade

### Scenario 5: Multiple Cities
- City A exports 500 units
- City B needs 200 units
- City C needs 300 units
- Both have connections to City A
- **Expected:** Both cities get goods (priority-based)

### Scenario 6: Priority Optimization
- City A exports 500 units
- City B needs 200 units (closer, better connection)
- City C needs 300 units (farther, worse connection)
- **Expected:** City B gets priority, City C gets remainder

---

## 📝 Usage Example

```csharp
var region = new Region();
// ... add cities and connections ...

// Calculate trade flows with default constraints
var flows = region.CalculateTradeFlows();

// Or with custom constraints
var flows = region.CalculateTradeFlows(
    maxTravelTimeMinutes: 45f,  // 45-minute limit
    capacityUtilizationLimit: 0.90f  // 90% capacity
);

// Apply trade effects
var applicator = new RegionalEffectsApplicator();
applicator.ApplyTradeEffects(flows, localCityId);
```

---

## 🔍 Logging

**Expected Logs:**
```
[CitiesRegional] Trade flow calculated: City A -> City B, 300 units, $13,500
[CitiesRegional] Trade effects applied: Revenue=$13,500, Cost=$0, Net=$13,500
```

**Future Enhancement:**
- Log priority scores
- Log capacity usage
- Log connection utilization
- Log trade matching decisions

---

## ⚙️ Configuration

### Default Values
- **Max Travel Time:** 60 minutes
- **Capacity Utilization:** 85%
- **Capacity Multiplier:** 10 (1 capacity = 10 resource units)

### Tuning
Adjust these values based on:
- Game balance requirements
- Connection types (highway vs rail)
- Resource types (bulk vs high-value)
- Regional size

---

## 🚀 Performance

**Optimization:**
- Priority-based sorting (O(n log n))
- Early termination when needs met
- Efficient connection lookup
- Dictionary-based tracking

**Expected Performance:**
- Small regions (2-4 cities): <1ms
- Medium regions (5-10 cities): <5ms
- Large regions (10+ cities): <10ms

---

## 📚 Related Files

- `src/Models/Region.cs` - Main implementation
- `src/Models/RegionalConnection.cs` - Connection model
- `src/Models/TradeFlow.cs` - Trade flow model
- `src/Systems/RegionalEffectsApplicator.cs` - Effect application
- `TRADE_SYSTEM_REQUIREMENTS.md` - Test requirements

---

## ✅ Status

**Implementation:** ✅ Complete  
**Build Status:** ✅ Successful  
**Ready for:** Testing and integration

---

**Trade Matching Algorithm Complete!** 🚀

