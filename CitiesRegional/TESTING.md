# Testing Guide - CitiesRegional Mod

## Overview

The CitiesRegional mod includes a comprehensive test suite that allows rapid development and validation without requiring the game to run.

**Last Updated:** 2025-12-26  
**Total Tests:** 32+ tests (19 original + 13 Phase 3 enhancements)  
**Status:** ✅ Performance benchmarks and edge case coverage complete

---

## Test Suite Structure

### Test Project: `CitiesRegional.Tests`

**Framework:** xUnit  
**Target:** .NET 8.0  
**Status:** ✅ 32+ tests (19 original + 13 Phase 3)

### Test Categories

#### 1. RegionalCityDataTests (6 tests)
Tests for the core data model without game dependencies.

**Tests:**
- `RegionalCityData_CanBeCreated` - Validates data model creation
- `RegionalCityData_GetNetTradeBalance_CalculatesCorrectly` - Trade balance math
- `RegionalCityData_GetNetTradeBalance_ReturnsZeroForMissingResource` - Edge cases
- `RegionalCityData_Clone_CreatesIndependentCopy` - Data integrity
- `ResourceData_ExportAvailable_CalculatedFromProductionConsumption` - Export logic
- `ResourceData_ImportNeeded_CalculatedFromConsumptionProduction` - Import logic

**Purpose:** Ensures data models work correctly and calculations are accurate.

#### 2. DataCollectionLogicTests (11 tests)
Tests for data collection calculation logic.

**Tests:**
- Resource export/import calculations
- GDP estimation logic
- Population-based estimates (workers, students, tourists)
- Resource pricing validation
- Edge cases (zero values, negative calculations)

**Purpose:** Validates that data collection logic produces correct results.

#### 3. PerformanceBenchmarkTests (4 tests) ✅ **NEW - Phase 3**
Performance benchmarks for critical operations.

**Tests:**
- `RegionalCityData_Clone_ShouldBeFast()` - Clone performance (<100ms)
- `RegionalCityData_GetNetTradeBalance_ShouldBeFast()` - Trade balance lookups (<1ms avg)
- `ResourceData_ExportImport_Calculations_ShouldBeFast()` - Calculation performance (<0.1ms avg)
- `RegionalCityData_WithManyResources_ShouldHandleEfficiently()` - Multi-resource performance (<5ms per resource)

**Purpose:** Ensures operations meet performance targets for real-time gameplay.

#### 4. EdgeCaseTests (9 tests) ✅ **NEW - Phase 3**
Tests for edge cases and boundary conditions.

**Tests:**
- Empty city scenarios (0 population)
- Negative treasury (debt handling)
- Extreme population values (2M+)
- Zero production/consumption
- Boundary metric values (0-100)
- Missing resource handling
- Data independence (clone)
- Export vs import logic

**Purpose:** Validates robustness and handles edge cases gracefully.

---

## Running Tests

### Quick Test Run
```bash
cd CitiesRegional.Tests
dotnet test
```

### Verbose Output
```bash
dotnet test --verbosity normal
```

### Run Specific Test Category
```bash
# Run only model tests
dotnet test --filter "FullyQualifiedName~RegionalCityData"

# Run only logic tests
dotnet test --filter "FullyQualifiedName~DataCollectionLogic"

# Run performance tests
dotnet test --filter "FullyQualifiedName~PerformanceTests"

# Run edge case tests
dotnet test --filter "FullyQualifiedName~EdgeCaseTests"
```

### Continuous Testing (Watch Mode)
```bash
# Install dotnet watch if needed
dotnet tool install -g dotnet-watch

# Run tests in watch mode
dotnet watch test
```

---

## Test Coverage

### ✅ What's Tested

1. **Data Models**
   - Creation and validation
   - Property assignments
   - Clone functionality
   - Trade balance calculations

2. **Calculation Logic**
   - Resource export/import math
   - GDP estimation
   - Population-based estimates
   - Edge cases and boundary conditions

3. **Data Integrity**
   - Independent copies (clone)
   - Value ranges (0-100 for metrics)
   - Positive values where required

### ⚠️ What's Not Tested (Yet)

1. **Game System Integration**
   - Tests that require Game.dll are skipped
   - CityDataCollector direct tests need game DLLs
   - Future: Mock game systems for full coverage

2. **Effect Application**
   - Modifying game state not yet tested
   - Requires game to be running

3. **Network/Server Integration**
   - Sync functionality not tested
   - Requires server to be running

---

## Adding New Tests

### Example: Testing a New Calculation

```csharp
[Fact]
public void MyNewCalculation_WorksCorrectly()
{
    // Arrange
    var input = 100f;
    
    // Act
    var result = MyCalculation(input);
    
    // Assert
    Assert.Equal(expectedValue, result);
}
```

### Best Practices

1. **Test One Thing:** Each test should verify one specific behavior
2. **Clear Names:** Test names should describe what they test
3. **Arrange-Act-Assert:** Follow AAA pattern
4. **No Game Dependencies:** Keep tests fast and independent
5. **Edge Cases:** Test zero, negative, and boundary values

---

## Test Results Interpretation

### ✅ Passing Tests
- All logic is working correctly
- Data models are valid
- Calculations are accurate

### ❌ Failing Tests
- Check error message for specific issue
- Verify test data is correct
- Check if logic changed and test needs update

### ⚠️ Skipped Tests
- Tests that require Game.dll are skipped
- These need game installation to run
- Consider mocking for full coverage

---

## Integration with Development Workflow

### Before Committing
```bash
# Run tests to ensure nothing broke
dotnet test
```

### During Development
```bash
# Run tests in watch mode for instant feedback
dotnet watch test
```

### CI/CD Integration
Tests can run in automated pipelines:
- GitHub Actions
- Azure DevOps
- GitLab CI
- Any .NET-compatible CI system

---

## Troubleshooting

### Tests Fail to Load Game.dll
**Solution:** Set `CS2_INSTALL` environment variable:
```bash
$env:CS2_INSTALL = "D:\mods\CS2\Cities.Skylines.II.v1.5.3f1\game"
```

### Tests Pass But Game Doesn't Work
- Tests validate logic, not game integration
- Always test in-game for final verification
- Check game logs for runtime errors

### Test Project Won't Build
- Ensure main project builds first
- Check project references are correct
- Verify .NET SDK version matches

---

## Future Enhancements

1. **Mock Game Systems**
   - Create mock implementations
   - Test CityDataCollector without game
   - Full integration test coverage

2. **Performance Tests**
   - Benchmark data collection
   - Test with large datasets
   - Verify no performance regressions

3. **Integration Tests**
   - Test with actual game DLLs
   - Verify system discovery works
   - Test data collection in real scenarios

---

## Related Documentation

- **Main Project:** See `MISSION_BRIEFING.md`
- **Phase 2 Progress:** See `PHASE2_PROGRESS.md`
- **System Discovery:** See `DISCOVERY_IMPLEMENTATION.md`

