using System;
using System.Collections.Generic;
using CitiesRegional.Config;
using CitiesRegional.Models;

namespace CitiesRegional.Services;

/// <summary>
/// Collects city data from game systems.
/// This is a mock implementation - real version would hook into CS2's game systems.
/// </summary>
public class CityDataCollector
{
    private string _cityId;
    private string _cityName;
    private string _playerName;
    private string _playerId;
    
    public CityDataCollector()
    {
        // Generate stable IDs for this city/player
        _cityId = GetOrCreateCityId();
        _playerId = GetOrCreatePlayerId();
        _cityName = "My City"; // Would read from game
        _playerName = Environment.UserName;
    }
    
    /// <summary>
    /// Collect current city data from game systems.
    /// In the real implementation, this would read from CS2's ECS systems.
    /// </summary>
    public RegionalCityData CollectCurrentData()
    {
        // Ensure CityId is valid (regenerate if somehow null/empty)
        if (string.IsNullOrEmpty(_cityId))
        {
            CitiesRegional.Logging.LogWarn("[Collector] CityId was null/empty, regenerating...");
            _cityId = GetOrCreateCityId();
        }
        
        // Ensure PlayerId is valid
        if (string.IsNullOrEmpty(_playerId))
        {
            CitiesRegional.Logging.LogWarn("[Collector] PlayerId was null/empty, regenerating...");
            _playerId = GetOrCreatePlayerId();
        }
        
        var data = new RegionalCityData
        {
            CityId = _cityId,
            CityName = _cityName,
            PlayerName = _playerName,
            PlayerId = _playerId,
            IsOnline = true,
            LastSeen = DateTime.UtcNow,
            LastSync = DateTime.UtcNow
        };
        
        // In real implementation, these would read from game systems:
        // data.Population = PopulationSystem.TotalPopulation;
        // data.Treasury = EconomySystem.Treasury;
        // etc.
        
        // For now, collect from mock/placeholder methods
        CollectPopulationData(data);
        CollectEconomyData(data);
        CollectResourceData(data);
        CollectMetricsData(data);
        CollectServicesData(data);
        
        return data;
    }
    
    #region Data Collection Methods (Stubs - Replace with real game hooks)
    
    private void CollectPopulationData(RegionalCityData data)
    {
        // Use ECS bridge system to get real population
        var bridge = CitiesRegional.Systems.CityDataEcsBridgeSystem.Instance;
        if (bridge == null)
        {
            CitiesRegional.Logging.LogWarn("[Collector] ECS bridge not ready; using placeholder values.");
            data.Population = GetGameValue("Population", 0);
            data.Workers = GetGameValue("Workers", 0);
            data.UnemployedWorkers = GetGameValue("Unemployed", 0);
            data.AvailableJobs = GetGameValue("JobOpenings", 0);
            data.Students = GetGameValue("Students", 0);
            data.Tourists = GetGameValue("Tourists", 0);
            return;
        }

        // Real population data from ECS queries
        data.Population = bridge.GetPopulationSnapshot();
        
        // Try to get real employment data, fall back to estimates
        var workers = bridge.GetWorkersSnapshot();
        var unemployed = bridge.GetUnemployedSnapshot();
        
        if (workers > 0)
        {
            data.Workers = workers;
            data.UnemployedWorkers = unemployed;
            data.AvailableJobs = Math.Max(0, (int)(data.Population * 0.03f)); // Still estimate jobs
        }
        else
        {
            // Estimate based on population if real data not available
            data.Workers = (int)(data.Population * 0.5f);
            data.UnemployedWorkers = (int)(data.Population * 0.05f);
            data.AvailableJobs = (int)(data.Population * 0.03f);
        }
        
        // Estimates for students and tourists (enhance later with real queries)
        data.Students = (int)(data.Population * 0.15f);
        data.Tourists = (int)(data.Population * 0.01f);
        
        // Update city name if available
        var cityName = bridge.GetCityNameSnapshot();
        if (!string.IsNullOrEmpty(cityName) && cityName != "Unknown")
        {
            _cityName = cityName;
        }
    }
    
    private void CollectEconomyData(RegionalCityData data)
    {
        // Get real economy data from ECS bridge
        var bridge = CitiesRegional.Systems.CityDataEcsBridgeSystem.Instance;
        if (bridge != null)
        {
            data.Treasury = bridge.GetTreasurySnapshot();
            data.WeeklyIncome = bridge.GetWeeklyIncomeSnapshot();
            data.WeeklyExpenses = bridge.GetWeeklyExpensesSnapshot();
            
            // Calculate GDP estimate from income
            if (data.WeeklyIncome > 0)
            {
                data.GDPEstimate = data.WeeklyIncome * 52f; // Annual estimate
            }
            else if (data.Population > 0)
            {
                data.GDPEstimate = data.Population * 500f; // Fallback estimate
            }
        }
        else
        {
            // Fallback estimates
            var population = data.Population > 0 ? data.Population : 10000;
            data.Treasury = GetGameValue("Treasury", Math.Max(100000L, population * 150L));
            data.WeeklyIncome = GetGameValue("WeeklyIncome", population * 25f);
            data.WeeklyExpenses = GetGameValue("WeeklyExpenses", population * 20f);
            data.GDPEstimate = GetGameValue("GDP", population * 500f);
        }
    }
    
    private void CollectResourceData(RegionalCityData data)
    {
        var bridge = CitiesRegional.Systems.CityDataEcsBridgeSystem.Instance;
        var resources = new List<ResourceData>();
        
        // Try to read resource data from game systems
        float electricityProduction = 0;
        float electricityConsumption = 0;
        float waterProduction = 0;
        float waterConsumption = 0;
        float industrialGoodsProduction = 0;
        float industrialGoodsConsumption = 0;
        float industrialGoodsStockpile = 0;
        float commercialGoodsProduction = 0;
        float commercialGoodsConsumption = 0;
        float commercialGoodsStockpile = 0;
        
        // Try to read trade data from TradeSystem
        float totalExportValue = 0;
        float totalImportValue = 0;
        Dictionary<string, float> exportByResource = new();
        Dictionary<string, float> importByResource = new();
        bool hasTradeData = false;
        
        if (bridge != null)
        {
            bridge.TryReadResourceData(out electricityProduction, out electricityConsumption, 
                out waterProduction, out waterConsumption,
                out industrialGoodsProduction, out industrialGoodsConsumption, out industrialGoodsStockpile,
                out commercialGoodsProduction, out commercialGoodsConsumption, out commercialGoodsStockpile);
            
            // Read trade data
            hasTradeData = bridge.TryReadTradeData(out totalExportValue, out totalImportValue, 
                out exportByResource, out importByResource);
            
            if (hasTradeData)
            {
                CitiesRegional.Logging.LogInfo($"Trade data: Export=${totalExportValue:C}, Import=${totalImportValue:C}");
                if (exportByResource.Count > 0 || importByResource.Count > 0)
                {
                    CitiesRegional.Logging.LogInfo($"Per-resource trade: {exportByResource.Count} exports, {importByResource.Count} imports");
                }
            }
        }
        
        // Electricity resource
        if (electricityProduction > 0 || electricityConsumption > 0)
        {
            var exportAvailable = Math.Max(0, electricityProduction - electricityConsumption);
            resources.Add(new ResourceData
            {
                Type = ResourceType.Electricity,
                Production = electricityProduction,
                Consumption = electricityConsumption,
                ExportAvailable = exportAvailable,
                ImportNeeded = Math.Max(0, electricityConsumption - electricityProduction),
                Price = 12f, // Default price, could be read from game
                Stockpile = 0 // Electricity isn't stockpiled
            });
        }
        else
        {
            // Fallback estimate
            resources.Add(new ResourceData
            {
                Type = ResourceType.Electricity,
                Production = GetGameValue("ElectricityProduction", 2000f),
                Consumption = GetGameValue("ElectricityConsumption", 1500f),
                ExportAvailable = GetGameValue("ElectricityExport", 500f),
                ImportNeeded = 0,
                Price = 12f,
                Stockpile = 0
            });
        }
        
        // Water resource
        if (waterProduction > 0 || waterConsumption > 0)
        {
            var exportAvailable = Math.Max(0, waterProduction - waterConsumption);
            resources.Add(new ResourceData
            {
                Type = ResourceType.Water,
                Production = waterProduction,
                Consumption = waterConsumption,
                ExportAvailable = exportAvailable,
                ImportNeeded = Math.Max(0, waterConsumption - waterProduction),
                Price = 8f,
                Stockpile = 0
            });
        }
        
        // Industrial goods - use real data if available, enhanced with trade data
        if (industrialGoodsProduction > 0 || industrialGoodsConsumption > 0 || hasTradeData)
        {
            var exportAvailable = Math.Max(0, industrialGoodsProduction - industrialGoodsConsumption);
            var importNeeded = Math.Max(0, industrialGoodsConsumption - industrialGoodsProduction);
            
            // Enhance with trade data if available
            if (hasTradeData)
            {
                foreach (var kvp in exportByResource)
                {
                    var key = kvp.Key.ToLowerInvariant();
                    if (key.Contains("industrial") || key.Contains("manufactured"))
                    {
                        exportAvailable = Math.Max(exportAvailable, kvp.Value);
                        CitiesRegional.Logging.LogDebug($"Trade system export for industrial goods: {kvp.Value}");
                    }
                }
                foreach (var kvp in importByResource)
                {
                    var key = kvp.Key.ToLowerInvariant();
                    if (key.Contains("industrial") || key.Contains("manufactured"))
                    {
                        importNeeded = Math.Max(importNeeded, kvp.Value);
                        CitiesRegional.Logging.LogDebug($"Trade system import for industrial goods: {kvp.Value}");
                    }
                }
            }
            
            resources.Add(new ResourceData
            {
                Type = ResourceType.IndustrialGoods,
                Production = industrialGoodsProduction,
                Consumption = industrialGoodsConsumption,
                ExportAvailable = exportAvailable,
                ImportNeeded = importNeeded,
                Price = 45f,
                Stockpile = industrialGoodsStockpile > 0 ? industrialGoodsStockpile : GetGameValue("IndustrialStock", 2000f)
            });
        }
        else
        {
            // Fallback estimates
            resources.Add(new ResourceData
            {
                Type = ResourceType.IndustrialGoods,
                Production = GetGameValue("IndustrialProduction", 1000f),
                Consumption = GetGameValue("IndustrialConsumption", 600f),
                ExportAvailable = GetGameValue("IndustrialExport", 400f),
                ImportNeeded = 0,
                Price = 45f,
                Stockpile = GetGameValue("IndustrialStock", 2000f)
            });
        }
        
        // Commercial goods - use real data if available, enhanced with trade data
        if (commercialGoodsProduction > 0 || commercialGoodsConsumption > 0 || hasTradeData)
        {
            var exportAvailable = Math.Max(0, commercialGoodsProduction - commercialGoodsConsumption);
            var importNeeded = Math.Max(0, commercialGoodsConsumption - commercialGoodsProduction);
            
            // Enhance with trade data if available
            if (hasTradeData)
            {
                foreach (var kvp in exportByResource)
                {
                    var key = kvp.Key.ToLowerInvariant();
                    if (key.Contains("commercial") || key.Contains("retail"))
                    {
                        exportAvailable = Math.Max(exportAvailable, kvp.Value);
                        CitiesRegional.Logging.LogDebug($"Trade system export for commercial goods: {kvp.Value}");
                    }
                }
                foreach (var kvp in importByResource)
                {
                    var key = kvp.Key.ToLowerInvariant();
                    if (key.Contains("commercial") || key.Contains("retail"))
                    {
                        importNeeded = Math.Max(importNeeded, kvp.Value);
                        CitiesRegional.Logging.LogDebug($"Trade system import for commercial goods: {kvp.Value}");
                    }
                }
            }
            
            resources.Add(new ResourceData
            {
                Type = ResourceType.CommercialGoods,
                Production = commercialGoodsProduction,
                Consumption = commercialGoodsConsumption,
                ExportAvailable = exportAvailable,
                ImportNeeded = importNeeded,
                Price = 60f,
                Stockpile = commercialGoodsStockpile > 0 ? commercialGoodsStockpile : GetGameValue("CommercialStock", 1000f)
            });
        }
        else
        {
            // Fallback estimates
            resources.Add(new ResourceData
            {
                Type = ResourceType.CommercialGoods,
                Production = GetGameValue("CommercialProduction", 500f),
                Consumption = GetGameValue("CommercialConsumption", 800f),
                ExportAvailable = 0,
                ImportNeeded = GetGameValue("CommercialImport", 300f),
                Price = 60f,
                Stockpile = GetGameValue("CommercialStock", 1000f)
            });
        }
        
        // Workers resource
        resources.Add(new ResourceData
        {
            Type = ResourceType.Workers,
            Production = data.UnemployedWorkers,
            Consumption = 0,
            ExportAvailable = data.UnemployedWorkers / 2, // Export up to 50%
            ImportNeeded = Math.Max(0, data.AvailableJobs - data.UnemployedWorkers),
            Price = 0, // Workers don't have a price, but commute costs apply
            Stockpile = 0
        });

        ApplyMaxExportCap(resources);
        data.Resources = resources;
    }

    private void ApplyMaxExportCap(List<ResourceData> resources)
    {
        if (resources.Count == 0)
        {
            return;
        }

        var settings = RegionalSettings.Instance;
        var maxExportPercentage = Math.Max(0, Math.Min(100, settings.MaxExportPercentage.Value));
        if (maxExportPercentage >= 100)
        {
            return;
        }

        var maxExportRatio = maxExportPercentage / 100f;
        foreach (var resource in resources)
        {
            var maxExport = resource.Production * maxExportRatio;
            if (resource.ExportAvailable > maxExport)
            {
                resource.ExportAvailable = maxExport;
            }
        }
    }
    
    private void CollectMetricsData(RegionalCityData data)
    {
        // Get real metrics from ECS bridge
        var bridge = CitiesRegional.Systems.CityDataEcsBridgeSystem.Instance;
        if (bridge != null)
        {
            data.Happiness = bridge.GetHappinessSnapshot();
            data.Health = bridge.GetHealthSnapshot();
            data.Education = bridge.GetEducationSnapshot();
            data.TrafficFlow = bridge.GetTrafficFlowSnapshot();
            data.Pollution = bridge.GetPollutionSnapshot();
            data.CrimeRate = bridge.GetCrimeRateSnapshot();
            
            // Land value not yet implemented, use fallback
            if (data.LandValueAvg == 0)
            {
                data.LandValueAvg = GetGameValue("LandValue", 5000f);
            }
        }
        else
        {
            // Fallback estimates
            data.Happiness = GetGameValue("Happiness", 75f);
            data.Health = GetGameValue("Health", 80f);
            data.Education = GetGameValue("Education", 70f);
            data.TrafficFlow = GetGameValue("TrafficFlow", 85f);
            data.LandValueAvg = GetGameValue("LandValue", 5000f);
            data.Pollution = GetGameValue("Pollution", 25f);
            data.CrimeRate = GetGameValue("Crime", 15f);
        }
    }
    
    private void CollectServicesData(RegionalCityData data)
    {
        // TODO: Detect regional-capable services in the city (airports, universities, hospitals)
        // This requires scanning game systems for service buildings
        // Services can be shared across the region for mutual benefit
        data.HostedServices = new List<SharedServiceInfo>();
        data.UsedServices = new List<SharedServiceUsage>();
        data.ConnectionPoints = new List<ConnectionPoint>();
        
        // Example: Check if city has an airport
        // if (HasBuilding(BuildingType.Airport))
        // {
        //     data.HostedServices.Add(new SharedServiceInfo
        //     {
        //         Type = SharedServiceType.Airport,
        //         Capacity = GetAirportCapacity(),
        //         AvailableCapacity = GetAirportAvailableCapacity()
        //     });
        // }
    }
    
    #endregion
    
    #region Helper Methods
    
    /// <summary>
    /// Get or create a persistent city ID
    /// </summary>
    private string GetOrCreateCityId()
    {
        // In real implementation, would store in save file or config
        // For now, generate based on machine + path
        var key = $"{Environment.MachineName}_{Environment.CurrentDirectory}";
        return $"city_{Math.Abs(key.GetHashCode()):X8}";
    }
    
    /// <summary>
    /// Get or create a persistent player ID
    /// </summary>
    private string GetOrCreatePlayerId()
    {
        // In real implementation, could use Steam ID or similar
        return $"player_{Math.Abs(Environment.UserName.GetHashCode()):X8}";
    }
    
    /// <summary>
    /// Placeholder for reading game values.
    /// In real implementation, this would query the game's ECS systems.
    /// </summary>
    private T GetGameValue<T>(string valueName, T defaultValue)
    {
        // TODO: Implement actual game value reading from CS2 game systems
        // Currently uses placeholder values - replace with real ECS system queries
        // See CityDataEcsBridgeSystem for examples of real data collection
        // This is where you'd hook into CS2's systems:
        //
        // Example for population:
        // var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        // var query = entityManager.CreateEntityQuery(typeof(Citizen));
        // return query.CalculateEntityCount();
        
        CitiesRegional.Logging.LogDebug($"GetGameValue({valueName}) - using default: {defaultValue}");
        return defaultValue;
    }
    
    /// <summary>
    /// Update city name from game
    /// </summary>
    public void SetCityName(string name)
    {
        _cityName = name;
    }
    
    #endregion
}

