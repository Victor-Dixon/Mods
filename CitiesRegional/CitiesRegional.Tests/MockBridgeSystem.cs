using System;
using CitiesRegional.Systems;

namespace CitiesRegional.Tests;

/// <summary>
/// Mock implementation of CityDataEcsBridgeSystem for testing
/// </summary>
public class MockBridgeSystem
{
    public static CityDataEcsBridgeSystem? CreateMock()
    {
        // We can't easily mock GameSystemBase, so we'll test the collector
        // with a null bridge (fallback mode) and verify it handles that correctly
        return null;
    }
    
    /// <summary>
    /// Test helper to simulate bridge system with known values
    /// </summary>
    public class TestData
    {
        public int Population { get; set; } = 50000;
        public int Households { get; set; } = 20000;
        public int Companies { get; set; } = 5000;
        public int Workers { get; set; } = 25000;
        public int Unemployed { get; set; } = 2000;
        public long Treasury { get; set; } = 5000000L;
        public float WeeklyIncome { get; set; } = 250000f;
        public float WeeklyExpenses { get; set; } = 200000f;
        public float Happiness { get; set; } = 75f;
        public float Health { get; set; } = 80f;
        public float Education { get; set; } = 70f;
        public float TrafficFlow { get; set; } = 85f;
        public float Pollution { get; set; } = 25f;
        public float CrimeRate { get; set; } = 15f;
        public string CityName { get; set; } = "Test City";
    }
}

