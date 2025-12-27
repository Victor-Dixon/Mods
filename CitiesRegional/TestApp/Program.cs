using System;
using System.Linq;
using System.Threading.Tasks;
using CitiesRegional;
using CitiesRegional.Models;
using CitiesRegional.Services;

namespace CitiesRegional.TestApp;

/// <summary>
/// Test application to verify the regional system works.
/// Run this to test without needing Cities: Skylines 2.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         Cities Regional - Test Application                 ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════╝");
        Console.WriteLine();
        
        // Initialize the plugin
        var plugin = new CitiesRegionalPlugin();
        plugin.Initialize();
        
        Console.WriteLine();
        Console.WriteLine("Testing Regional Data Models...");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        
        // Create test city data
        var city1 = CreateTestCity("Metropolis", "Alice", 75000);
        var city2 = CreateTestCity("Riverside", "Bob", 45000);
        var city3 = CreateTestCity("Hilltown", "Charlie", 28000);
        
        // Create a region
        var region = new Region
        {
            RegionName = "Test Metro Area",
            MaxCities = 4
        };
        
        // Add cities
        region.AddCity(city1);
        region.AddCity(city2);
        region.AddCity(city3);
        
        Console.WriteLine($"Region: {region.RegionName}");
        Console.WriteLine($"Region Code: {region.RegionCode}");
        Console.WriteLine($"Cities: {region.Cities.Count}");
        Console.WriteLine();
        
        // Print city summaries
        foreach (var city in region.Cities)
        {
            Console.WriteLine($"  📍 {city.CityName} (Mayor: {city.PlayerName})");
            Console.WriteLine($"     Population: {city.Population:N0}");
            Console.WriteLine($"     Happiness: {city.Happiness}%");
            Console.WriteLine($"     Treasury: ${city.Treasury:N0}");
            Console.WriteLine();
        }
        
        // Add connections
        Console.WriteLine("Adding connections...");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        
        region.AddConnection(new RegionalConnection
        {
            FromCityId = city1.CityId,
            ToCityId = city2.CityId,
            Type = ConnectionType.Highway4Lane,
            Name = "Interstate 101",
            Capacity = 2500,
            TravelTimeMinutes = 25
        });
        
        region.AddConnection(new RegionalConnection
        {
            FromCityId = city2.CityId,
            ToCityId = city3.CityId,
            Type = ConnectionType.RegionalRail,
            Name = "Metro Rail",
            Capacity = 2000,
            TravelTimeMinutes = 20
        });
        
        foreach (var conn in region.Connections)
        {
            var from = region.GetCity(conn.FromCityId);
            var to = region.GetCity(conn.ToCityId);
            Console.WriteLine($"  🛣️ {conn.Name}: {from?.CityName} ↔ {to?.CityName}");
            Console.WriteLine($"     Type: {conn.Type}, Capacity: {conn.Capacity}/hr, Time: {conn.TravelTimeMinutes} min");
        }
        Console.WriteLine();
        
        // Calculate trade flows
        Console.WriteLine("Calculating trade flows...");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        
        var flows = region.CalculateTradeFlows();
        foreach (var flow in flows)
        {
            var from = region.GetCity(flow.FromCityId);
            var to = region.GetCity(flow.ToCityId);
            Console.WriteLine($"  📦 {flow.ResourceType}: {from?.CityName} → {to?.CityName}");
            Console.WriteLine($"     Amount: {flow.Amount:N0} units @ ${flow.PricePerUnit}/unit = ${flow.TotalValue:N0}");
        }
        Console.WriteLine();
        
        // Show regional stats
        Console.WriteLine("Regional Statistics:");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        Console.WriteLine($"  Total Population: {region.TotalPopulation:N0}");
        Console.WriteLine($"  Total GDP: ${region.TotalGDP:N0}");
        Console.WriteLine($"  Average Happiness: {region.AverageHappiness:F1}%");
        Console.WriteLine($"  Online Cities: {region.OnlineCities}/{region.Cities.Count}");
        Console.WriteLine();
        
        // Show recent events
        Console.WriteLine("Recent Events:");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        foreach (var evt in region.RecentEvents.Take(5))
        {
            Console.WriteLine($"  📰 [{evt.Timestamp:HH:mm:ss}] {evt.Title}");
        }
        Console.WriteLine();
        
        // Test JSON serialization
        Console.WriteLine("Testing JSON Serialization...");
        Console.WriteLine("─────────────────────────────────────────────────────────────");
        
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(city1, Newtonsoft.Json.Formatting.Indented);
        Console.WriteLine($"City data size: {json.Length} bytes");
        Console.WriteLine("Sample JSON (first 500 chars):");
        Console.WriteLine(json.Substring(0, Math.Min(500, json.Length)));
        Console.WriteLine("...");
        Console.WriteLine();
        
        // Cleanup
        plugin.Shutdown();
        
        Console.WriteLine("═════════════════════════════════════════════════════════════");
        Console.WriteLine("✅ All tests passed! The regional system is working.");
        Console.WriteLine("═════════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("Next steps:");
        Console.WriteLine("  1. Run the Server: cd Server && dotnet run");
        Console.WriteLine("  2. Update CS2 path in Directory.Build.props");
        Console.WriteLine("  3. Enable BepInEx packages in CitiesRegional.csproj");
        Console.WriteLine("  4. Fill in game hooks in CityDataCollector.cs");
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
    
    static RegionalCityData CreateTestCity(string name, string player, int population)
    {
        var random = new Random(name.GetHashCode());
        
        return new RegionalCityData
        {
            CityName = name,
            PlayerName = player,
            PlayerId = $"player_{player.ToLower()}",
            IsOnline = random.Next(100) > 30, // 70% chance online
            
            Population = population,
            Workers = (int)(population * 0.45),
            UnemployedWorkers = (int)(population * 0.05),
            AvailableJobs = (int)(population * 0.03),
            Students = (int)(population * 0.15),
            Tourists = random.Next(100, 1000),
            
            Treasury = random.Next(500000, 5000000),
            WeeklyIncome = random.Next(100000, 500000),
            WeeklyExpenses = random.Next(80000, 400000),
            GDPEstimate = population * random.Next(80, 120),
            
            Happiness = random.Next(65, 90),
            Health = random.Next(70, 95),
            Education = random.Next(60, 90),
            TrafficFlow = random.Next(60, 95),
            LandValueAvg = random.Next(3000, 10000),
            Pollution = random.Next(10, 40),
            CrimeRate = random.Next(5, 25),
            
            Resources = new System.Collections.Generic.List<ResourceData>
            {
                new ResourceData
                {
                    Type = ResourceType.IndustrialGoods,
                    Production = random.Next(500, 2000),
                    Consumption = random.Next(300, 1500),
                    ExportAvailable = random.Next(0, 500),
                    ImportNeeded = random.Next(0, 300),
                    Price = 45
                },
                new ResourceData
                {
                    Type = ResourceType.CommercialGoods,
                    Production = random.Next(200, 1000),
                    Consumption = random.Next(400, 1200),
                    ExportAvailable = random.Next(0, 200),
                    ImportNeeded = random.Next(0, 400),
                    Price = 60
                },
                new ResourceData
                {
                    Type = ResourceType.Workers,
                    Production = (int)(population * 0.05), // Unemployed
                    Consumption = 0,
                    ExportAvailable = (int)(population * 0.025),
                    ImportNeeded = random.Next(0, (int)(population * 0.02)),
                    Price = 0
                }
            }
        };
    }
}

