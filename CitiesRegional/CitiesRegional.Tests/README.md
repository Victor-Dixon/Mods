# CitiesRegional.Tests

Unit tests for CitiesRegional mod that can run without launching the game.

## Running Tests

```bash
cd CitiesRegional.Tests
dotnet test
```

## Test Coverage

### ✅ Working Tests (No Game DLLs Required)
- **RegionalCityDataTests**: Tests for the data model
  - Data creation and validation
  - Trade balance calculations
  - Clone functionality
  
- **DataCollectionLogicTests**: Tests for data collection logic
  - Resource export/import calculations
  - GDP estimation
  - Population-based estimates
  - Resource pricing

### ⚠️ Tests Requiring Game DLLs
Tests that use `CityDataCollector` directly require Game.dll to be available.
These are skipped for now. To run them, ensure:
1. `CS2_INSTALL` environment variable is set
2. Game DLLs are accessible at the expected path

## Future Enhancements

1. **Mock Game Systems**: Create mock implementations of game systems for testing
2. **Integration Tests**: Test with actual game DLLs (requires game installation)
3. **Performance Tests**: Benchmark data collection performance
4. **Edge Case Tests**: Test with extreme values (0 population, negative treasury, etc.)

