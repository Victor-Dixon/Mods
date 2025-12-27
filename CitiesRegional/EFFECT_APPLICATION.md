# Effect Application - Implementation Guide

**Date:** 2025-12-25  
**Status:** ✅ Implemented - Ready for Testing

---

## Overview

The `RegionalEffectsApplicator` class applies regional effects (trade, commuters, services) to the local city by modifying game state.

---

## ✅ Implemented: Treasury Modification

### Method: `ApplyTreasuryChange(float amount, string reason)`

**Purpose:** Modify the city's treasury balance based on regional effects (trade revenue, service costs, etc.)

**Implementation Approach:**
1. Gets `BudgetSystem` from `CityDataEcsBridgeSystem` (reuses existing system reference)
2. Uses reflection to find modification methods or properties
3. Tries multiple method names: `AddMoney`, `AddTreasury`, `ModifyTreasury`, `SetTreasury`, `ChangeTreasury`
4. Falls back to direct property setter if methods not found
5. Records effect for potential reversal

**Code Location:** `src/Systems/RegionalEffectsApplicator.cs`

**Key Features:**
- ✅ Reflection-based (works without knowing exact API)
- ✅ Multiple fallback strategies
- ✅ Error handling and logging
- ✅ Effect tracking for reversal
- ✅ Zero-amount detection (skips unnecessary operations)

**Usage Example:**
```csharp
var applicator = new RegionalEffectsApplicator();

// Apply trade revenue
applicator.ApplyTradeEffects(tradeFlows, localCityId);

// Apply service revenue
applicator.ApplyServiceEffects(localCity);
```

---

## 🔍 How It Works

### 1. System Access
```csharp
var bridge = CityDataEcsBridgeSystem.Instance;
var budgetSystem = bridge.GetBudgetSystem();
```

### 2. Method Discovery
Uses reflection to find methods that can modify treasury:
- Checks for methods with names like `AddMoney`, `AddTreasury`, etc.
- Validates method signature (single parameter: long, int, or float)
- Invokes method with the amount

### 3. Property Fallback
If no methods found, tries to set `Treasury` property directly:
- Reads current value
- Adds/subtracts the amount
- Sets new value

### 4. Error Handling
- Logs warnings if system not available
- Logs errors if modification fails
- Continues execution even if modification fails (graceful degradation)

---

## ⏳ Pending Implementations

### 1. Resource Modification
**Methods:** `ApplyExportEffect()`, `ApplyImportEffect()`

**Status:** Stub implementation (logs only)

**Needed:**
- Access to `ResourceFlowSystem` or `CountCityStoredResourceSystem`
- Methods to add/remove resources from stockpiles
- Resource type mapping

**Estimated Effort:** 2-3 days

### 2. Worker Modification
**Method:** `ApplyWorkerChange()`

**Status:** Stub implementation (logs only)

**Needed:**
- Understanding of worker/job system
- How to modify available workers
- May require entity manipulation

**Estimated Effort:** 3-5 days (complex)

### 3. Modifier Application
**Method:** `ApplyModifier()`

**Status:** Stub implementation (logs only)

**Needed:**
- Access to modifier system
- Understanding of modifier types
- How to apply temporary modifiers

**Estimated Effort:** 2-3 days

---

## 🧪 Testing

### In-Game Testing Required

1. **Treasury Modification:**
   - Create a test region with trade flows
   - Verify treasury changes appear in-game
   - Check logs for success/failure messages
   - Verify amounts are correct

2. **Error Handling:**
   - Test with BudgetSystem unavailable
   - Test with invalid amounts
   - Verify graceful degradation

3. **Effect Tracking:**
   - Verify effects are recorded
   - Test effect reversal (when leaving region)

### Test Scenarios

| Scenario | Expected Result |
|----------|----------------|
| Trade revenue +$10,000 | Treasury increases by $10,000 |
| Service cost -$5,000 | Treasury decreases by $5,000 |
| Zero amount | Operation skipped (logged) |
| System unavailable | Warning logged, operation skipped |
| Method not found | Property setter attempted, or warning logged |

---

## 📝 Logging

All operations are logged:
- **Info:** Successful modifications
- **Debug:** Detailed operation info
- **Warn:** System unavailable or method not found
- **Error:** Exceptions during modification

**Log Format:**
```
[CitiesRegional] Treasury change applied: +10,000 (Regional Trade) via AddMoney
[CitiesRegional] Treasury change applied: -5,000 (Service Cost: Airport) via property setter (new total: 1,495,000)
```

---

## 🔧 Future Enhancements

1. **Batch Operations:** Apply multiple changes in one transaction
2. **Validation:** Check treasury limits before applying
3. **History:** Track all treasury changes for debugging
4. **Notifications:** Show in-game notifications for large changes
5. **Reversal System:** Implement proper effect reversal

---

## 📚 Related Files

- `src/Systems/RegionalEffectsApplicator.cs` - Main implementation
- `src/Systems/CityDataEcsBridgeSystem.cs` - System access
- `src/Models/RegionalCityData.cs` - Data models
- `MISSION_BRIEFING.md` - Project overview

---

## 🎯 Next Steps

1. ✅ Implement treasury modification (DONE)
2. ⏳ Test in-game to verify it works
3. ⏳ Implement resource modification
4. ⏳ Implement worker modification
5. ⏳ Implement modifier application

