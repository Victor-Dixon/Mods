using System;
using System.Collections.Generic;
using System.Reflection;
using Game;
using Game.Common;
using Game.Citizens;
using Game.Simulation;
using Unity.Entities;

namespace CitiesRegional.Systems
{
    /// <summary>
    /// Runs inside the game world; safe place to do ECS queries and expose snapshots to plugin/services.
    /// Collects city data from ECS components and game systems.
    /// </summary>
    public sealed class CityDataEcsBridgeSystem : GameSystemBase
    {
        public static CityDataEcsBridgeSystem? Instance { get; private set; }

        // Entity queries
        private EntityQuery _citizenQuery;
        private EntityQuery _householdQuery;
        private EntityQuery _companyQuery;
        
        // Cached system references (discovered dynamically)
        private object? _cityConfigurationSystem;
        private object? _countEmploymentSystem;
        private object? _countHouseholdDataSystem;
        private object? _cityStatisticsSystem;
        private object? _budgetSystem;
        private object? _tradeSystem;
        private object? _resourceFlowSystem;
        private object? _citizenHappinessSystem;
        private object? _crimeStatisticsSystem;
        private object? _airPollutionSystem;
        private object? _groundPollutionSystem;
        private object? _countConsumptionSystem;
        private object? _countCityStoredResourceSystem;
        
        private uint _lastUpdateFrame;
        
        // Cached data snapshots - Population
        private int _population;
        private int _households;
        private int _companies;
        private int _workers;
        private int _unemployed;
        private string _cityName = "Unknown";
        
        // Economy data
        private long _treasury;
        private float _weeklyIncome;
        private float _weeklyExpenses;
        
        // Metrics
        private float _happiness;
        private float _health;
        private float _education;
        private float _trafficFlow;
        private float _pollution;
        private float _crimeRate;
        
        private bool _systemsDiscovered;

        // Update every 256 frames (~4 seconds at 60fps) for low impact
        private const uint UpdateEveryNFrames = 256;

        protected override void OnCreate()
        {
            base.OnCreate();
            Instance = this;
            _systemsDiscovered = false;

            // Set up entity queries
            try
            {
                // Population: Count citizens (excluding deleted/temp)
                _citizenQuery = GetEntityQuery(new EntityQueryDesc
                {
                    All = new[] { ComponentType.ReadOnly<Citizen>() },
                    None = new[] { ComponentType.ReadOnly<Deleted>() }
                });
            }
            catch
            {
                _citizenQuery = GetEntityQuery(ComponentType.ReadOnly<Citizen>());
            }

            // Try to set up additional queries
            TryCreateAdditionalQueries();

            // RequireForUpdate ensures OnUpdate only runs when query has entities
            // But we want it to run always to discover systems, so we'll check in OnUpdate
            // RequireForUpdate(_citizenQuery);

            _lastUpdateFrame = 0;
            _population = 0;
            
            // Log that OnCreate completed
            CitiesRegionalPlugin.LogInfo("[ECS] CityDataEcsBridgeSystem.OnCreate() completed");

            CitiesRegionalPlugin.LogInfo("[ECS] CityDataEcsBridgeSystem created.");
        }

        private void TryCreateAdditionalQueries()
        {
            // Try to create Household query
            try
            {
                var householdType = Type.GetType("Game.Citizens.Household, Game");
                if (householdType != null)
                {
                    _householdQuery = GetEntityQuery(new EntityQueryDesc
                    {
                        All = new[] { ComponentType.ReadOnly(householdType) },
                        None = new[] { ComponentType.ReadOnly<Deleted>() }
                    });
                    CitiesRegionalPlugin.LogInfo("[ECS] Created Household query");
                }
            }
            catch (Exception ex)
            {
                CitiesRegionalPlugin.LogDebug($"[ECS] Could not create Household query: {ex.Message}");
            }

            // Try to create Company query
            try
            {
                var companyType = Type.GetType("Game.Companies.CompanyData, Game");
                if (companyType != null)
                {
                    _companyQuery = GetEntityQuery(new EntityQueryDesc
                    {
                        All = new[] { ComponentType.ReadOnly(companyType) },
                        None = new[] { ComponentType.ReadOnly<Deleted>() }
                    });
                    CitiesRegionalPlugin.LogInfo("[ECS] Created Company query");
                }
            }
            catch (Exception ex)
            {
                CitiesRegionalPlugin.LogDebug($"[ECS] Could not create Company query: {ex.Message}");
            }
        }

        private void DiscoverGameSystems()
        {
            if (_systemsDiscovered) return;
            _systemsDiscovered = true;

            CitiesRegionalPlugin.LogInfo("[ECS] Discovering game systems...");

            // Core city systems
            TryGetSystem("Game.Simulation.CityConfigurationSystem", out _cityConfigurationSystem);
            TryGetSystem("Game.Simulation.CityStatisticsSystem", out _cityStatisticsSystem);
            
            // Economy systems
            TryGetSystem("Game.Simulation.BudgetSystem", out _budgetSystem);
            
            // Trade and resources
            TryGetSystem("Game.Simulation.TradeSystem", out _tradeSystem);
            TryGetSystem("Game.Simulation.ResourceFlowSystem", out _resourceFlowSystem);
            
            // Employment and population
            TryGetSystem("Game.Simulation.CountEmploymentSystem", out _countEmploymentSystem);
            TryGetSystem("Game.Simulation.CountHouseholdDataSystem", out _countHouseholdDataSystem);
            
            // Metrics
            TryGetSystem("Game.Simulation.CitizenHappinessSystem", out _citizenHappinessSystem);
            TryGetSystem("Game.Simulation.CrimeStatisticsSystem", out _crimeStatisticsSystem);
            TryGetSystem("Game.Simulation.AirPollutionSystem", out _airPollutionSystem);
            TryGetSystem("Game.Simulation.GroundPollutionSystem", out _groundPollutionSystem);
            
            // Resource counting systems
            TryGetSystem("Game.Simulation.CountConsumptionSystem", out _countConsumptionSystem);
            TryGetSystem("Game.Simulation.CountCityStoredResourceSystem", out _countCityStoredResourceSystem);
            
            // Try to get city name from CityConfigurationSystem
            TryReadCityName();
        }
        
        private void TryReadCityName()
        {
            if (_cityConfigurationSystem == null) return;
            
            try
            {
                var type = _cityConfigurationSystem.GetType();
                var cityNameProp = type.GetProperty("cityName") ?? type.GetProperty("CityName") 
                    ?? type.GetProperty("m_cityName");
                    
                if (cityNameProp != null)
                {
                    _cityName = cityNameProp.GetValue(_cityConfigurationSystem)?.ToString() ?? "Unknown";
                    CitiesRegionalPlugin.LogInfo($"[ECS] City name: {_cityName}");
                }
            }
            catch (Exception ex)
            {
                CitiesRegionalPlugin.LogDebug($"[ECS] Could not read city name: {ex.Message}");
            }
        }

        private bool TryGetSystem(string typeName, out object? system)
        {
            system = null;
            try
            {
                var type = Type.GetType(typeName + ", Game");
                if (type != null)
                {
                    var method = typeof(World).GetMethod("GetExistingSystemManaged", Type.EmptyTypes);
                    if (method != null)
                    {
                        var genericMethod = method.MakeGenericMethod(type);
                        system = genericMethod.Invoke(World, null);
                        if (system != null)
                        {
                            CitiesRegionalPlugin.LogInfo($"[ECS] Found system: {typeName}");
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CitiesRegionalPlugin.LogDebug($"[ECS] Could not get system {typeName}: {ex.Message}");
            }
            return false;
        }

        protected override void OnDestroy()
        {
            if (Instance == this) Instance = null;
            base.OnDestroy();
        }

        protected override void OnUpdate()
        {
            // Always log first call with high visibility
            if (_lastUpdateFrame == 0)
            {
                CitiesRegionalPlugin.LogInfo("*** [ECS] OnUpdate called for the first time! ***");
                UnityEngine.Debug.Log("[CitiesRegional] *** ECS Bridge OnUpdate FIRST CALL ***");
            }
            
            try
            {
                var simSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
                if (simSystem == null)
                {
                    CitiesRegionalPlugin.LogWarn("[ECS] SimulationSystem not available");
                    UnityEngine.Debug.LogWarning("[CitiesRegional] SimulationSystem not available");
                    return;
                }

                var frame = simSystem.frameIndex;

                // Log heartbeat every 512 frames (~8.5 seconds) for visibility
                if ((frame % 512) == 0)
                {
                    CitiesRegionalPlugin.LogInfo($"[ECS] Heartbeat: Frame={frame}, Enabled={Enabled}, Pop={_population}");
                    UnityEngine.Debug.Log($"[CitiesRegional] ECS Heartbeat: Frame={frame}, Enabled={Enabled}, Pop={_population}");
                }

                if (_lastUpdateFrame != 0 && (frame - _lastUpdateFrame) < UpdateEveryNFrames)
                    return;

                _lastUpdateFrame = frame;

                // Discover systems on first update (after world is fully initialized)
                if (_lastUpdateFrame == frame)
                {
                    CitiesRegionalPlugin.LogInfo($"[ECS] Starting data collection on frame {frame}");
                    DiscoverGameSystems();
                }

                // Update population from citizen query
                _population = _citizenQuery.CalculateEntityCount();

                // Update household count if available
                if (_householdQuery.IsEmptyIgnoreFilter == false)
                {
                    try { _households = _householdQuery.CalculateEntityCount(); } catch { }
                }

                // Update company count if available
                if (_companyQuery.IsEmptyIgnoreFilter == false)
                {
                    try { _companies = _companyQuery.CalculateEntityCount(); } catch { }
                }

                // Try to get employment data from system
                TryUpdateEmploymentData();
                
                // Update economy data
                TryUpdateEconomyData();
                
                // Update metrics
                TryUpdateMetricsData();
                
                // Log collected data every 2048 frames (~34 seconds) for debugging
                if ((frame % 2048) == 0 && frame > 0)
                {
                    LogCollectedData();
                }

                // Log info every update cycle (every 256 frames) for visibility
                CitiesRegionalPlugin.LogInfo($"[ECS] Data Update: Pop={_population}, Treasury={_treasury:C0}, Happiness={_happiness:F1}, Frame={frame}");
                UnityEngine.Debug.Log($"[CitiesRegional] ECS Update: Pop={_population}, Treasury={_treasury:C0}, Frame={frame}");
                
                // Also log on first update to confirm it's running
                if (_lastUpdateFrame == frame && frame < UpdateEveryNFrames)
                {
                    CitiesRegionalPlugin.LogInfo($"*** [ECS] First data collection complete! Frame={frame} ***");
                    UnityEngine.Debug.Log($"[CitiesRegional] *** First data collection complete! Frame={frame} ***");
                }
            }
            catch (Exception ex)
            {
                CitiesRegionalPlugin.LogError($"[ECS] Update failed: {ex}");
                UnityEngine.Debug.LogError($"[CitiesRegional] ECS Update failed: {ex}");
            }
        }

        private void TryUpdateEmploymentData()
        {
            if (_countEmploymentSystem == null) return;

            try
            {
                // Try to read total workers and unemployed
                var type = _countEmploymentSystem.GetType();
                
                // Common property names to try
                var workersProp = type.GetProperty("TotalWorkers") ?? type.GetProperty("m_TotalWorkers");
                if (workersProp != null)
                {
                    _workers = Convert.ToInt32(workersProp.GetValue(_countEmploymentSystem));
                }

                var unemployedProp = type.GetProperty("TotalUnemployed") ?? type.GetProperty("m_TotalUnemployed");
                if (unemployedProp != null)
                {
                    _unemployed = Convert.ToInt32(unemployedProp.GetValue(_countEmploymentSystem));
                }
            }
            catch { }
        }

        private void TryUpdateEconomyData()
        {
            // Try to get treasury from BudgetSystem
            if (_budgetSystem != null)
            {
                try
                {
                    var type = _budgetSystem.GetType();
                    var treasuryProp = type.GetProperty("Treasury") ?? type.GetProperty("treasury") 
                        ?? type.GetProperty("m_Treasury") ?? type.GetProperty("m_treasury");
                    
                    if (treasuryProp != null)
                    {
                        var value = treasuryProp.GetValue(_budgetSystem);
                        if (value != null)
                        {
                            _treasury = Convert.ToInt64(value);
                        }
                    }
                    
                    // Try to get income/expenses
                    var incomeProp = type.GetProperty("WeeklyIncome") ?? type.GetProperty("Income") 
                        ?? type.GetProperty("m_WeeklyIncome");
                    if (incomeProp != null)
                    {
                        var value = incomeProp.GetValue(_budgetSystem);
                        if (value != null)
                        {
                            _weeklyIncome = Convert.ToSingle(value);
                        }
                    }
                    
                    var expenseProp = type.GetProperty("WeeklyExpenses") ?? type.GetProperty("Expenses") 
                        ?? type.GetProperty("m_WeeklyExpenses");
                    if (expenseProp != null)
                    {
                        var value = expenseProp.GetValue(_budgetSystem);
                        if (value != null)
                        {
                            _weeklyExpenses = Convert.ToSingle(value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CitiesRegionalPlugin.LogDebug($"[ECS] Could not read budget data: {ex.Message}");
                }
            }
            
            // Fallback: Try CityStatisticsSystem for treasury
            if (_treasury == 0 && _cityStatisticsSystem != null)
            {
                try
                {
                    var type = _cityStatisticsSystem.GetType();
                    var treasuryProp = type.GetProperty("Treasury") ?? type.GetProperty("treasury");
                    if (treasuryProp != null)
                    {
                        var value = treasuryProp.GetValue(_cityStatisticsSystem);
                        if (value != null)
                        {
                            _treasury = Convert.ToInt64(value);
                        }
                    }
                }
                catch { }
            }
        }
        
        private void TryUpdateMetricsData()
        {
            // Try to get happiness from CitizenHappinessSystem
            if (_citizenHappinessSystem != null)
            {
                try
                {
                    var type = _citizenHappinessSystem.GetType();
                    var happinessProp = type.GetProperty("AverageHappiness") ?? type.GetProperty("Happiness") 
                        ?? type.GetProperty("m_AverageHappiness");
                    
                    if (happinessProp != null)
                    {
                        var value = happinessProp.GetValue(_citizenHappinessSystem);
                        if (value != null)
                        {
                            _happiness = Convert.ToSingle(value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CitiesRegionalPlugin.LogDebug($"[ECS] Could not read happiness: {ex.Message}");
                }
            }
            
            // Try CityStatisticsSystem for other metrics
            if (_cityStatisticsSystem != null)
            {
                try
                {
                    var type = _cityStatisticsSystem.GetType();
                    
                    // Health
                    var healthProp = type.GetProperty("Health") ?? type.GetProperty("AverageHealth");
                    if (healthProp != null)
                    {
                        var value = healthProp.GetValue(_cityStatisticsSystem);
                        if (value != null) _health = Convert.ToSingle(value);
                    }
                    
                    // Education
                    var eduProp = type.GetProperty("Education") ?? type.GetProperty("EducationLevel");
                    if (eduProp != null)
                    {
                        var value = eduProp.GetValue(_cityStatisticsSystem);
                        if (value != null) _education = Convert.ToSingle(value);
                    }
                    
                    // Traffic
                    var trafficProp = type.GetProperty("TrafficFlow") ?? type.GetProperty("TrafficEfficiency");
                    if (trafficProp != null)
                    {
                        var value = trafficProp.GetValue(_cityStatisticsSystem);
                        if (value != null) _trafficFlow = Convert.ToSingle(value);
                    }
                }
                catch (Exception ex)
                {
                    CitiesRegionalPlugin.LogDebug($"[ECS] Could not read statistics: {ex.Message}");
                }
            }
            
            // Try to read pollution data
            TryReadPollutionData();
            
            // Try to read crime rate
            TryReadCrimeData();
        }
        
        private void TryReadPollutionData()
        {
            // Try AirPollutionSystem
            if (_airPollutionSystem != null)
            {
                try
                {
                    var type = _airPollutionSystem.GetType();
                    var pollutionProp = type.GetProperty("TotalPollution") ?? type.GetProperty("AveragePollution")
                        ?? type.GetProperty("Pollution") ?? type.GetProperty("m_Pollution");
                    
                    if (pollutionProp != null)
                    {
                        var value = pollutionProp.GetValue(_airPollutionSystem);
                        if (value != null)
                        {
                            _pollution = Math.Max(_pollution, Convert.ToSingle(value));
                        }
                    }
                }
                catch (Exception ex)
                {
                    CitiesRegionalPlugin.LogDebug($"[ECS] Could not read air pollution: {ex.Message}");
                }
            }
            
            // Try GroundPollutionSystem
            if (_groundPollutionSystem != null)
            {
                try
                {
                    var type = _groundPollutionSystem.GetType();
                    var pollutionProp = type.GetProperty("TotalPollution") ?? type.GetProperty("AveragePollution")
                        ?? type.GetProperty("Pollution");
                    
                    if (pollutionProp != null)
                    {
                        var value = pollutionProp.GetValue(_groundPollutionSystem);
                        if (value != null)
                        {
                            // Combine with air pollution (use max or average)
                            var groundPoll = Convert.ToSingle(value);
                            _pollution = Math.Max(_pollution, groundPoll);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CitiesRegionalPlugin.LogDebug($"[ECS] Could not read ground pollution: {ex.Message}");
                }
            }
            
            // Fallback: Try CityStatisticsSystem for pollution
            if (_pollution == 0 && _cityStatisticsSystem != null)
            {
                try
                {
                    var type = _cityStatisticsSystem.GetType();
                    var pollutionProp = type.GetProperty("Pollution") ?? type.GetProperty("AveragePollution");
                    if (pollutionProp != null)
                    {
                        var value = pollutionProp.GetValue(_cityStatisticsSystem);
                        if (value != null) _pollution = Convert.ToSingle(value);
                    }
                }
                catch { }
            }
        }
        
        private void TryReadCrimeData()
        {
            // Try CrimeStatisticsSystem
            if (_crimeStatisticsSystem != null)
            {
                try
                {
                    var type = _crimeStatisticsSystem.GetType();
                    var crimeProp = type.GetProperty("CrimeRate") ?? type.GetProperty("AverageCrimeRate")
                        ?? type.GetProperty("TotalCrime") ?? type.GetProperty("m_CrimeRate");
                    
                    if (crimeProp != null)
                    {
                        var value = crimeProp.GetValue(_crimeStatisticsSystem);
                        if (value != null)
                        {
                            _crimeRate = Convert.ToSingle(value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CitiesRegionalPlugin.LogDebug($"[ECS] Could not read crime rate: {ex.Message}");
                }
            }
            
            // Fallback: Try CityStatisticsSystem for crime
            if (_crimeRate == 0 && _cityStatisticsSystem != null)
            {
                try
                {
                    var type = _cityStatisticsSystem.GetType();
                    var crimeProp = type.GetProperty("CrimeRate") ?? type.GetProperty("Crime");
                    if (crimeProp != null)
                    {
                        var value = crimeProp.GetValue(_cityStatisticsSystem);
                        if (value != null) _crimeRate = Convert.ToSingle(value);
                    }
                }
                catch { }
            }
        }

        // Public snapshot accessors - Population
        public int GetPopulationSnapshot() => _population;
        public int GetHouseholdsSnapshot() => _households;
        public int GetCompaniesSnapshot() => _companies;
        public int GetWorkersSnapshot() => _workers;
        public int GetUnemployedSnapshot() => _unemployed;
        public string GetCityNameSnapshot() => _cityName;
        
        // Economy
        public long GetTreasurySnapshot() => _treasury;
        public float GetWeeklyIncomeSnapshot() => _weeklyIncome;
        public float GetWeeklyExpensesSnapshot() => _weeklyExpenses;
        
        // Metrics
        public float GetHappinessSnapshot() => _happiness;
        public float GetHealthSnapshot() => _health;
        public float GetEducationSnapshot() => _education;
        public float GetTrafficFlowSnapshot() => _trafficFlow;
        public float GetPollutionSnapshot() => _pollution;
        public float GetCrimeRateSnapshot() => _crimeRate;
        
        // System accessors for resource/trade data collection
        public object? GetCityStatisticsSystem() => _cityStatisticsSystem;
        public object? GetBudgetSystem() => _budgetSystem;
        public object? GetTradeSystem() => _tradeSystem;
        public object? GetResourceFlowSystem() => _resourceFlowSystem;
        public object? GetCountCityStoredResourceSystem() => _countCityStoredResourceSystem;
        public object? GetCountEmploymentSystem() => _countEmploymentSystem;
        
        /// <summary>
        /// Attempt to read resource data from game systems using reflection.
        /// This is a best-effort approach since we don't know exact structure yet.
        /// </summary>
        public void TryReadResourceData(out float electricityProduction, out float electricityConsumption, 
            out float waterProduction, out float waterConsumption,
            out float industrialGoodsProduction, out float industrialGoodsConsumption, out float industrialGoodsStockpile,
            out float commercialGoodsProduction, out float commercialGoodsConsumption, out float commercialGoodsStockpile)
        {
            electricityProduction = 0;
            electricityConsumption = 0;
            waterProduction = 0;
            waterConsumption = 0;
            industrialGoodsProduction = 0;
            industrialGoodsConsumption = 0;
            industrialGoodsStockpile = 0;
            commercialGoodsProduction = 0;
            commercialGoodsConsumption = 0;
            commercialGoodsStockpile = 0;
            
            // Try to read from ResourceFlowSystem or related systems
            if (_resourceFlowSystem != null)
            {
                try
                {
                    var type = _resourceFlowSystem.GetType();
                    // Try common property names for resource data
                    var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    foreach (var prop in props)
                    {
                        var name = prop.Name.ToLowerInvariant();
                        if (name.Contains("electricity") && name.Contains("production"))
                        {
                            var value = prop.GetValue(_resourceFlowSystem);
                            if (value != null) electricityProduction = Convert.ToSingle(value);
                        }
                        else if (name.Contains("electricity") && name.Contains("consumption"))
                        {
                            var value = prop.GetValue(_resourceFlowSystem);
                            if (value != null) electricityConsumption = Convert.ToSingle(value);
                        }
                        else if (name.Contains("water") && name.Contains("production"))
                        {
                            var value = prop.GetValue(_resourceFlowSystem);
                            if (value != null) waterProduction = Convert.ToSingle(value);
                        }
                        else if (name.Contains("water") && name.Contains("consumption"))
                        {
                            var value = prop.GetValue(_resourceFlowSystem);
                            if (value != null) waterConsumption = Convert.ToSingle(value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CitiesRegionalPlugin.LogDebug($"[ECS] Could not read resource data: {ex.Message}");
                }
            }
            
            // Try ElectricityStatisticsSystem for electricity data
            try
            {
                var elecStatsType = Type.GetType("Game.Simulation.ElectricityStatisticsSystem, Game");
                if (elecStatsType != null)
                {
                    var method = typeof(World).GetMethod("GetExistingSystemManaged", Type.EmptyTypes);
                    if (method != null)
                    {
                        var genericMethod = method.MakeGenericMethod(elecStatsType);
                        var elecSystem = genericMethod.Invoke(World, null);
                        if (elecSystem != null)
                        {
                            var type = elecSystem.GetType();
                            var productionProp = type.GetProperty("Production") ?? type.GetProperty("TotalProduction");
                            var consumptionProp = type.GetProperty("Consumption") ?? type.GetProperty("TotalConsumption");
                            
                            if (productionProp != null)
                            {
                                var value = productionProp.GetValue(elecSystem);
                                if (value != null) electricityProduction = Convert.ToSingle(value);
                            }
                            if (consumptionProp != null)
                            {
                                var value = consumptionProp.GetValue(elecSystem);
                                if (value != null) electricityConsumption = Convert.ToSingle(value);
                            }
                        }
                    }
                }
            }
            catch { }
            
            // Try WaterStatisticsSystem for water data
            try
            {
                var waterStatsType = Type.GetType("Game.Simulation.WaterStatisticsSystem, Game");
                if (waterStatsType != null)
                {
                    var method = typeof(World).GetMethod("GetExistingSystemManaged", Type.EmptyTypes);
                    if (method != null)
                    {
                        var genericMethod = method.MakeGenericMethod(waterStatsType);
                        var waterSystem = genericMethod.Invoke(World, null);
                        if (waterSystem != null)
                        {
                            var type = waterSystem.GetType();
                            var productionProp = type.GetProperty("Production") ?? type.GetProperty("TotalProduction");
                            var consumptionProp = type.GetProperty("Consumption") ?? type.GetProperty("TotalConsumption");
                            
                            if (productionProp != null)
                            {
                                var value = productionProp.GetValue(waterSystem);
                                if (value != null) waterProduction = Convert.ToSingle(value);
                            }
                            if (consumptionProp != null)
                            {
                                var value = consumptionProp.GetValue(waterSystem);
                                if (value != null) waterConsumption = Convert.ToSingle(value);
                            }
                        }
                    }
                }
            }
            catch { }
            
            // Try CountCityStoredResourceSystem for industrial/commercial goods stockpile
            if (_countCityStoredResourceSystem != null)
            {
                try
                {
                    var type = _countCityStoredResourceSystem.GetType();
                    var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    foreach (var prop in props)
                    {
                        var name = prop.Name.ToLowerInvariant();
                        if (name.Contains("industrial") && (name.Contains("amount") || name.Contains("count") || name.Contains("stock")))
                        {
                            var value = prop.GetValue(_countCityStoredResourceSystem);
                            if (value != null) industrialGoodsStockpile = Convert.ToSingle(value);
                        }
                        else if (name.Contains("commercial") && (name.Contains("amount") || name.Contains("count") || name.Contains("stock")))
                        {
                            var value = prop.GetValue(_countCityStoredResourceSystem);
                            if (value != null) commercialGoodsStockpile = Convert.ToSingle(value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    CitiesRegionalPlugin.LogDebug($"[ECS] Could not read stored resources: {ex.Message}");
                }
            }
            
            // Try CountConsumptionSystem for production/consumption data
            if (_countConsumptionSystem != null)
            {
                try
                {
                    var type = _countConsumptionSystem.GetType();
                    var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    foreach (var prop in props)
                    {
                        var name = prop.Name.ToLowerInvariant();
                        if (name.Contains("industrial"))
                        {
                            var value = prop.GetValue(_countConsumptionSystem);
                            if (value != null)
                            {
                                if (name.Contains("production") || name.Contains("produce"))
                                    industrialGoodsProduction = Convert.ToSingle(value);
                                else if (name.Contains("consumption") || name.Contains("consume"))
                                    industrialGoodsConsumption = Convert.ToSingle(value);
                            }
                        }
                        else if (name.Contains("commercial"))
                        {
                            var value = prop.GetValue(_countConsumptionSystem);
                            if (value != null)
                            {
                                if (name.Contains("production") || name.Contains("produce"))
                                    commercialGoodsProduction = Convert.ToSingle(value);
                                else if (name.Contains("consumption") || name.Contains("consume"))
                                    commercialGoodsConsumption = Convert.ToSingle(value);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    CitiesRegionalPlugin.LogDebug($"[ECS] Could not read consumption data: {ex.Message}");
                }
            }
            
            // Try CompanyStatisticsSystem for production data
            try
            {
                var companyStatsType = Type.GetType("Game.Simulation.CompanyStatisticsSystem, Game");
                if (companyStatsType != null)
                {
                    var method = typeof(World).GetMethod("GetExistingSystemManaged", Type.EmptyTypes);
                    if (method != null)
                    {
                        var genericMethod = method.MakeGenericMethod(companyStatsType);
                        var companyStatsSystem = genericMethod.Invoke(World, null);
                        if (companyStatsSystem != null)
                        {
                            var type = companyStatsSystem.GetType();
                            var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                            
                            foreach (var prop in props)
                            {
                                var name = prop.Name.ToLowerInvariant();
                                if (name.Contains("industrial") && (name.Contains("production") || name.Contains("output")))
                                {
                                    var value = prop.GetValue(companyStatsSystem);
                                    if (value != null) industrialGoodsProduction = Convert.ToSingle(value);
                                }
                                else if (name.Contains("commercial") && (name.Contains("production") || name.Contains("output")))
                                {
                                    var value = prop.GetValue(companyStatsSystem);
                                    if (value != null) commercialGoodsProduction = Convert.ToSingle(value);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }
        
        private void LogCollectedData()
        {
            CitiesRegionalPlugin.LogInfo($"[ECS Data Snapshot] Population: {_population}, Households: {_households}, Companies: {_companies}");
            CitiesRegionalPlugin.LogInfo($"[ECS Data Snapshot] Workers: {_workers}, Unemployed: {_unemployed}");
            CitiesRegionalPlugin.LogInfo($"[ECS Data Snapshot] Treasury: {_treasury}, Income: {_weeklyIncome:F2}, Expenses: {_weeklyExpenses:F2}");
            CitiesRegionalPlugin.LogInfo($"[ECS Data Snapshot] Happiness: {_happiness:F1}, Health: {_health:F1}, Education: {_education:F1}");
            CitiesRegionalPlugin.LogInfo($"[ECS Data Snapshot] Traffic: {_trafficFlow:F1}, Pollution: {_pollution:F1}, Crime: {_crimeRate:F1}");
        }
        
        /// <summary>
        /// Attempt to read trade data from TradeSystem
        /// Returns export/import values and per-resource trade data
        /// </summary>
        public bool TryReadTradeData(out float totalExportValue, out float totalImportValue, 
            out Dictionary<string, float> exportByResource, out Dictionary<string, float> importByResource)
        {
            totalExportValue = 0;
            totalImportValue = 0;
            exportByResource = new Dictionary<string, float>();
            importByResource = new Dictionary<string, float>();
            
            if (_tradeSystem == null)
                return false;
            
            try
            {
                var type = _tradeSystem.GetType();
                var props = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                // Try to find total export/import values
                foreach (var prop in props)
                {
                    var name = prop.Name.ToLowerInvariant();
                    if (name.Contains("export") && (name.Contains("value") || name.Contains("revenue") || name.Contains("total")))
                    {
                        var value = prop.GetValue(_tradeSystem);
                        if (value != null) 
                        {
                            totalExportValue = Convert.ToSingle(value);
                            CitiesRegionalPlugin.LogDebug($"[ECS] Found export value: {totalExportValue:C} from {prop.Name}");
                        }
                    }
                    else if (name.Contains("import") && (name.Contains("value") || name.Contains("cost") || name.Contains("total")))
                    {
                        var value = prop.GetValue(_tradeSystem);
                        if (value != null) 
                        {
                            totalImportValue = Convert.ToSingle(value);
                            CitiesRegionalPlugin.LogDebug($"[ECS] Found import value: {totalImportValue:C} from {prop.Name}");
                        }
                    }
                }
                
                // Try to find per-resource trade data
                // Look for collections, dictionaries, or arrays that might contain resource-specific trade
                foreach (var prop in props)
                {
                    var name = prop.Name.ToLowerInvariant();
                    if ((name.Contains("export") || name.Contains("import") || name.Contains("trade")) 
                        && (name.Contains("resource") || name.Contains("goods") || name.Contains("data")))
                    {
                        try
                        {
                            var value = prop.GetValue(_tradeSystem);
                            if (value != null)
                            {
                                // Try to enumerate if it's a collection
                                if (value is System.Collections.IEnumerable enumerable && !(value is string))
                                {
                                    foreach (var item in enumerable)
                                    {
                                        if (item == null) continue;
                                        
                                        var itemType = item.GetType();
                                        var itemProps = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                                        
                                        string? resourceName = null;
                                        float amount = 0;
                                        bool isExport = false;
                                        
                                        foreach (var itemProp in itemProps)
                                        {
                                            var itemName = itemProp.Name.ToLowerInvariant();
                                            if (itemName.Contains("resource") || itemName.Contains("type") || itemName.Contains("goods"))
                                            {
                                                var resValue = itemProp.GetValue(item);
                                                if (resValue != null) resourceName = resValue.ToString();
                                            }
                                            else if (itemName.Contains("amount") || itemName.Contains("quantity") || itemName.Contains("value"))
                                            {
                                                var amtValue = itemProp.GetValue(item);
                                                if (amtValue != null) amount = Convert.ToSingle(amtValue);
                                            }
                                            else if (itemName.Contains("export"))
                                            {
                                                isExport = true;
                                            }
                                            else if (itemName.Contains("import"))
                                            {
                                                isExport = false;
                                            }
                                        }
                                        
                                        if (resourceName != null && amount > 0)
                                        {
                                            if (isExport)
                                                exportByResource[resourceName] = amount;
                                            else
                                                importByResource[resourceName] = amount;
                                        }
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
                
                // Also try methods that might return trade data
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    var methodName = method.Name.ToLowerInvariant();
                    if ((methodName.Contains("get") || methodName.Contains("calculate")) 
                        && (methodName.Contains("export") || methodName.Contains("import") || methodName.Contains("trade"))
                        && method.GetParameters().Length == 0)
                    {
                        try
                        {
                            var result = method.Invoke(_tradeSystem, null);
                            if (result != null)
                            {
                                var resultType = result.GetType();
                                if (resultType == typeof(float) || resultType == typeof(double) || resultType == typeof(int))
                                {
                                    var val = Convert.ToSingle(result);
                                    if (methodName.Contains("export"))
                                        totalExportValue = val;
                                    else if (methodName.Contains("import"))
                                        totalImportValue = val;
                                }
                            }
                        }
                        catch { }
                    }
                }
                
                return totalExportValue > 0 || totalImportValue > 0 || exportByResource.Count > 0 || importByResource.Count > 0;
            }
            catch (Exception ex)
            {
                CitiesRegionalPlugin.LogWarn($"[ECS] Could not read trade data: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Simplified version for backward compatibility
        /// </summary>
        public void TryReadTradeData(out float exportValue, out float importValue)
        {
            TryReadTradeData(out exportValue, out importValue, out _, out _);
        }
    }
}

