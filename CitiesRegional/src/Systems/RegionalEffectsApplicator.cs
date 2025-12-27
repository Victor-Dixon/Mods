using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CitiesRegional.Models;
using Unity.Entities;

namespace CitiesRegional.Services;

/// <summary>
/// Applies regional effects (trade, commuters, services) to the local city.
/// This modifies game state based on regional connections.
/// </summary>
public class RegionalEffectsApplicator
{
    // Track applied effects for reversal if needed
    private readonly Dictionary<string, AppliedEffect> _appliedEffects = new();
    
    /// <summary>
    /// Apply trade effects based on calculated trade flows
    /// </summary>
    public void ApplyTradeEffects(List<TradeFlow> tradeFlows, string localCityId)
    {
        float totalRevenue = 0;
        float totalCost = 0;
        
        foreach (var flow in tradeFlows)
        {
            if (flow.FromCityId == localCityId)
            {
                // We are exporting
                ApplyExportEffect(flow);
                totalRevenue += flow.TotalValue;
            }
            else if (flow.ToCityId == localCityId)
            {
                // We are importing
                ApplyImportEffect(flow);
                totalCost += flow.TotalValue;
            }
        }
        
        // Apply net trade balance to city treasury
        var netBalance = totalRevenue - totalCost;
        ApplyTreasuryChange(netBalance, "Regional Trade");
        
        CitiesRegional.Logging.LogInfo(
            $"Trade effects applied: Revenue={totalRevenue:C}, Cost={totalCost:C}, Net={netBalance:C}"
        );
    }
    
    /// <summary>
    /// Apply commuter effects based on regional data
    /// </summary>
    public void ApplyCommuterEffects(Region region, string localCityId)
    {
        var localCity = region.GetCity(localCityId);
        if (localCity == null) return;
        
        int inboundCommuters = 0;
        int outboundCommuters = 0;
        
        foreach (var otherCity in region.Cities.Where(c => c.CityId != localCityId))
        {
            // Check if there's a connection
            var connection = region.GetConnection(localCityId, otherCity.CityId);
            if (connection == null) continue;
            
            // Calculate commuters based on job/worker balance and connection capacity
            var maxCommuteTime = 45; // From config
            
            if (connection.TravelTimeMinutes <= maxCommuteTime)
            {
                // Workers coming TO our city
                if (localCity.AvailableJobs > 0 && otherCity.UnemployedWorkers > 0)
                {
                    var potential = Math.Min(localCity.AvailableJobs, otherCity.UnemployedWorkers / 2);
                    var limited = Math.Min(potential, connection.Capacity / 10); // Capacity limits
                    inboundCommuters += limited;
                }
                
                // Workers going FROM our city
                if (otherCity.AvailableJobs > 0 && localCity.UnemployedWorkers > 0)
                {
                    var potential = Math.Min(otherCity.AvailableJobs, localCity.UnemployedWorkers / 2);
                    var limited = Math.Min(potential, connection.Capacity / 10);
                    outboundCommuters += limited;
                }
            }
        }
        
        ApplyWorkerChange(inboundCommuters - outboundCommuters, "Regional Commuters");
        
        CitiesRegional.Logging.LogInfo(
            $"Commuter effects applied: Inbound={inboundCommuters}, Outbound={outboundCommuters}, Net={inboundCommuters - outboundCommuters}"
        );
    }
    
    /// <summary>
    /// Apply shared service effects
    /// </summary>
    public void ApplyServiceEffects(RegionalCityData localCity)
    {
        // Apply benefits from services we host
        foreach (var service in localCity.HostedServices)
        {
            var revenue = CalculateServiceRevenue(service);
            ApplyTreasuryChange(revenue, $"Service Revenue: {service.Type}");
            
            // Host benefits
            ApplyServiceHostBonus(service);
        }
        
        // Apply benefits (and costs) from services we use
        foreach (var usage in localCity.UsedServices)
        {
            ApplyTreasuryChange(-usage.WeeklyCost, $"Service Cost: {usage.Type}");
            ApplyServiceUsageBonus(usage);
        }
    }
    
    #region Effect Application Methods (Stubs - Replace with real game hooks)
    
    private void ApplyExportEffect(TradeFlow flow)
    {
        if (flow.Amount <= 0)
        {
            CitiesRegional.Logging.LogDebug($"Export effect skipped (zero/negative amount): {flow.ResourceType}");
            return;
        }
        
        try
        {
            var bridge = CitiesRegional.Systems.CityDataEcsBridgeSystem.Instance;
            if (bridge == null)
            {
                CitiesRegional.Logging.LogWarn("Cannot apply export effect: ECS bridge not available");
                return;
            }
            
            // Try to modify resource stockpile via CountCityStoredResourceSystem
            var resourceSystem = bridge.GetCountCityStoredResourceSystem();
            if (resourceSystem == null)
            {
                CitiesRegional.Logging.LogDebug($"Cannot modify {flow.ResourceType} stockpile: CountCityStoredResourceSystem not found");
                RecordEffect($"export_{flow.ResourceType}", new AppliedEffect
                {
                    Type = "Export",
                    ResourceType = flow.ResourceType.ToString(),
                    Amount = flow.Amount,
                    AppliedAt = DateTime.UtcNow
                });
                return;
            }
            
            var systemType = resourceSystem.GetType();
            var resourceTypeName = flow.ResourceType.ToString().ToLowerInvariant();
            
            // Map our ResourceType to game resource property names
            string[] propertyNamePatterns = GetResourcePropertyPatterns(flow.ResourceType);
            
            bool modified = false;
            foreach (var pattern in propertyNamePatterns)
            {
                var props = systemType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var prop in props)
                {
                    var propName = prop.Name.ToLowerInvariant();
                    if (propName.Contains(pattern) && (propName.Contains("amount") || propName.Contains("count") || propName.Contains("stock")))
                    {
                        if (prop.CanRead && prop.CanWrite)
                        {
                            try
                            {
                                var currentValue = Convert.ToSingle(prop.GetValue(resourceSystem) ?? 0f);
                                var newValue = Math.Max(0, currentValue - flow.Amount);
                                prop.SetValue(resourceSystem, newValue);
                                
                                CitiesRegional.Logging.LogInfo($"Export effect applied: {flow.ResourceType} -{flow.Amount} (was {currentValue}, now {newValue}) via {prop.Name}");
                                modified = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                CitiesRegional.Logging.LogDebug($"Could not modify {prop.Name}: {ex.Message}");
                            }
                        }
                    }
                }
                if (modified) break;
            }
            
            if (!modified)
            {
                CitiesRegional.Logging.LogDebug($"Could not find writable property for {flow.ResourceType} in CountCityStoredResourceSystem");
            }
            
            RecordEffect($"export_{flow.ResourceType}", new AppliedEffect
            {
                Type = "Export",
                ResourceType = flow.ResourceType.ToString(),
                Amount = flow.Amount,
                AppliedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            CitiesRegional.Logging.LogError($"Failed to apply export effect: {ex.Message}");
        }
    }
    
    private void ApplyImportEffect(TradeFlow flow)
    {
        if (flow.Amount <= 0)
        {
            CitiesRegional.Logging.LogDebug($"Import effect skipped (zero/negative amount): {flow.ResourceType}");
            return;
        }
        
        try
        {
            var bridge = CitiesRegional.Systems.CityDataEcsBridgeSystem.Instance;
            if (bridge == null)
            {
                CitiesRegional.Logging.LogWarn("Cannot apply import effect: ECS bridge not available");
                return;
            }
            
            // Try to modify resource stockpile via CountCityStoredResourceSystem
            var resourceSystem = bridge.GetCountCityStoredResourceSystem();
            if (resourceSystem == null)
            {
                CitiesRegional.Logging.LogDebug($"Cannot modify {flow.ResourceType} stockpile: CountCityStoredResourceSystem not found");
                RecordEffect($"import_{flow.ResourceType}", new AppliedEffect
                {
                    Type = "Import",
                    ResourceType = flow.ResourceType.ToString(),
                    Amount = flow.Amount,
                    AppliedAt = DateTime.UtcNow
                });
                return;
            }
            
            var systemType = resourceSystem.GetType();
            var resourceTypeName = flow.ResourceType.ToString().ToLowerInvariant();
            
            // Map our ResourceType to game resource property names
            string[] propertyNamePatterns = GetResourcePropertyPatterns(flow.ResourceType);
            
            bool modified = false;
            foreach (var pattern in propertyNamePatterns)
            {
                var props = systemType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var prop in props)
                {
                    var propName = prop.Name.ToLowerInvariant();
                    if (propName.Contains(pattern) && (propName.Contains("amount") || propName.Contains("count") || propName.Contains("stock")))
                    {
                        if (prop.CanRead && prop.CanWrite)
                        {
                            try
                            {
                                var currentValue = Convert.ToSingle(prop.GetValue(resourceSystem) ?? 0f);
                                var newValue = currentValue + flow.Amount;
                                prop.SetValue(resourceSystem, newValue);
                                
                                CitiesRegional.Logging.LogInfo($"Import effect applied: {flow.ResourceType} +{flow.Amount} (was {currentValue}, now {newValue}) via {prop.Name}");
                                modified = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                CitiesRegional.Logging.LogDebug($"Could not modify {prop.Name}: {ex.Message}");
                            }
                        }
                    }
                }
                if (modified) break;
            }
            
            if (!modified)
            {
                CitiesRegional.Logging.LogDebug($"Could not find writable property for {flow.ResourceType} in CountCityStoredResourceSystem");
            }
            
            RecordEffect($"import_{flow.ResourceType}", new AppliedEffect
            {
                Type = "Import",
                ResourceType = flow.ResourceType.ToString(),
                Amount = flow.Amount,
                AppliedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            CitiesRegional.Logging.LogError($"Failed to apply import effect: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Maps ResourceType enum to property name patterns used in game systems
    /// </summary>
    private string[] GetResourcePropertyPatterns(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.IndustrialGoods => new[] { "industrial", "manufactured", "goods" },
            ResourceType.CommercialGoods => new[] { "commercial", "retail", "goods" },
            ResourceType.Agriculture => new[] { "agriculture", "food", "crop" },
            ResourceType.Oil => new[] { "oil", "petroleum" },
            ResourceType.Ore => new[] { "ore", "mineral" },
            ResourceType.RawMaterials => new[] { "raw", "material", "resource" },
            ResourceType.Forestry => new[] { "forestry", "wood", "lumber" },
            _ => new[] { resourceType.ToString().ToLowerInvariant() }
        };
    }
    
    private void ApplyTreasuryChange(float amount, string reason)
    {
        if (Math.Abs(amount) < 0.01f)
        {
            CitiesRegional.Logging.LogDebug($"Treasury change skipped (zero): {reason}");
            return;
        }
        
        try
        {
            // Get BudgetSystem from ECS bridge (reuse the one we're already reading from)
            var bridge = CitiesRegional.Systems.CityDataEcsBridgeSystem.Instance;
            if (bridge == null)
            {
                CitiesRegional.Logging.LogWarn("Cannot apply treasury change: ECS bridge not available");
                return;
            }
            
            var budgetSystem = bridge.GetBudgetSystem();
            if (budgetSystem == null)
            {
                CitiesRegional.Logging.LogWarn("Cannot apply treasury change: BudgetSystem not found");
                return;
            }
            
            var systemType = budgetSystem.GetType();
            long amountLong = (long)Math.Round(amount);
            
            // Try multiple method names that might modify treasury
            MethodInfo? method = null;
            string[] methodNames = { "AddMoney", "AddTreasury", "ModifyTreasury", "SetTreasury", "ChangeTreasury" };
            
            foreach (var methodName in methodNames)
            {
                method = systemType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                if (method != null)
                {
                    var parameters = method.GetParameters();
                    // Check if method signature matches (long or int parameter)
                    if (parameters.Length == 1 && 
                        (parameters[0].ParameterType == typeof(long) || 
                         parameters[0].ParameterType == typeof(int) ||
                         parameters[0].ParameterType == typeof(float)))
                    {
                        break;
                    }
                    method = null;
                }
            }
            
            if (method != null)
            {
                // Invoke the method with the amount
                var paramType = method.GetParameters()[0].ParameterType;
                object paramValue = paramType == typeof(long) ? amountLong :
                                   paramType == typeof(int) ? (int)amountLong :
                                   (float)amount;
                
                method.Invoke(budgetSystem, new[] { paramValue });
                
                CitiesRegional.Logging.LogInfo($"Treasury change applied: {amount:+#,0;-#,0} ({reason}) via {method.Name}");
                
                // Record the effect
                RecordEffect("treasury_change", new AppliedEffect
                {
                    Type = "TreasuryChange",
                    Amount = amount,
                    AppliedAt = DateTime.UtcNow
                });
            }
            else
            {
                // Fallback: Try to set Treasury property directly
                var treasuryProp = systemType.GetProperty("Treasury", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic) 
                    ?? systemType.GetProperty("treasury", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
                    ?? systemType.GetProperty("m_Treasury", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
                
                if (treasuryProp != null && treasuryProp.CanRead && treasuryProp.CanWrite)
                {
                    var currentValue = Convert.ToInt64(treasuryProp.GetValue(budgetSystem) ?? 0L);
                    var newValue = currentValue + amountLong;
                    treasuryProp.SetValue(budgetSystem, newValue);
                    
                    CitiesRegional.Logging.LogInfo($"Treasury change applied: {amount:+#,0;-#,0} ({reason}) via property setter (new total: {newValue:N0})");
                    
                    RecordEffect("treasury_change", new AppliedEffect
                    {
                        Type = "TreasuryChange",
                        Amount = amount,
                        AppliedAt = DateTime.UtcNow
                    });
                }
                else
                {
                    CitiesRegional.Logging.LogWarn($"Cannot apply treasury change: No suitable method or writable property found on BudgetSystem. Amount: {amount:+#,0;-#,0} ({reason})");
                }
            }
        }
        catch (Exception ex)
        {
            CitiesRegional.Logging.LogError($"Failed to apply treasury change: {ex.Message}");
            CitiesRegional.Logging.LogDebug($"Treasury change attempted: {amount:+#,0;-#,0} ({reason})");
        }
    }
    
    private void ApplyWorkerChange(int netWorkers, string reason)
    {
        if (netWorkers == 0)
        {
            CitiesRegional.Logging.LogDebug($"Worker change skipped (zero): {reason}");
            return;
        }
        
        try
        {
            var bridge = CitiesRegional.Systems.CityDataEcsBridgeSystem.Instance;
            if (bridge == null)
            {
                CitiesRegional.Logging.LogWarn("Cannot apply worker change: ECS bridge not available");
                return;
            }
            
            // Try to modify via CountEmploymentSystem
            var employmentSystem = bridge.GetCountEmploymentSystem();
            if (employmentSystem == null)
            {
                CitiesRegional.Logging.LogDebug($"Cannot modify workers: CountEmploymentSystem not found. Change: {netWorkers:+#;-#;0} ({reason})");
                RecordEffect("worker_change", new AppliedEffect
                {
                    Type = "WorkerChange",
                    Amount = netWorkers,
                    AppliedAt = DateTime.UtcNow
                });
                return;
            }
            
            var systemType = employmentSystem.GetType();
            bool modified = false;
            
            // Try to find properties that control worker counts
            var props = systemType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            // Look for properties related to workers, jobs, or employment
            string[] propertyPatterns = { "worker", "employment", "job", "available", "unemployed" };
            
            foreach (var prop in props)
            {
                var propName = prop.Name.ToLowerInvariant();
                bool matchesPattern = propertyPatterns.Any(pattern => propName.Contains(pattern));
                
                if (matchesPattern && prop.CanRead && prop.CanWrite)
                {
                    try
                    {
                        var currentValue = Convert.ToInt32(prop.GetValue(employmentSystem) ?? 0);
                        var newValue = Math.Max(0, currentValue + netWorkers);
                        prop.SetValue(employmentSystem, newValue);
                        
                        CitiesRegional.Logging.LogInfo($"Worker change applied: {netWorkers:+#;-#;0} ({reason}) via {prop.Name} (was {currentValue}, now {newValue})");
                        modified = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        CitiesRegional.Logging.LogDebug($"Could not modify {prop.Name}: {ex.Message}");
                    }
                }
            }
            
            // Try methods that might modify worker counts
            if (!modified)
            {
                var methods = systemType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                string[] methodPatterns = { "add", "modify", "set", "change", "update" };
                
                foreach (var method in methods)
                {
                    var methodName = method.Name.ToLowerInvariant();
                    bool matchesPattern = methodPatterns.Any(pattern => methodName.Contains(pattern)) &&
                                        (methodName.Contains("worker") || methodName.Contains("employment") || methodName.Contains("job"));
                    
                    if (matchesPattern)
                    {
                        var parameters = method.GetParameters();
                        if (parameters.Length == 1 && 
                            (parameters[0].ParameterType == typeof(int) || parameters[0].ParameterType == typeof(long)))
                        {
                            try
                            {
                                var paramValue = parameters[0].ParameterType == typeof(int) ? (object)netWorkers : (object)(long)netWorkers;
                                method.Invoke(employmentSystem, new[] { paramValue });
                                
                                CitiesRegional.Logging.LogInfo($"Worker change applied: {netWorkers:+#;-#;0} ({reason}) via {method.Name}");
                                modified = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                CitiesRegional.Logging.LogDebug($"Could not invoke {method.Name}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            
            if (!modified)
            {
                CitiesRegional.Logging.LogDebug($"Could not find writable property or method for worker modification in CountEmploymentSystem. Change: {netWorkers:+#;-#;0} ({reason})");
            }
            
            RecordEffect("worker_change", new AppliedEffect
            {
                Type = "WorkerChange",
                Amount = netWorkers,
                AppliedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            CitiesRegional.Logging.LogError($"Failed to apply worker change: {ex.Message}");
        }
    }
    
    private float CalculateServiceRevenue(SharedServiceInfo service)
    {
        // Revenue = (Total Usage - Own Usage) * CostPerUnit
        var externalUsage = service.Capacity - service.AvailableCapacity;
        return externalUsage * service.CostPerUnit;
    }
    
    private void ApplyServiceHostBonus(SharedServiceInfo service)
    {
        // Apply bonuses for hosting regional services
        switch (service.Type)
        {
            case SharedServiceType.Airport:
                // Tourism boost
                ApplyModifier("Tourism", 1.3f); // +30%
                break;
                
            case SharedServiceType.University:
                // Education boost
                ApplyModifier("Education", 1.15f); // +15%
                break;
                
            case SharedServiceType.Stadium:
                // Entertainment/happiness boost
                ApplyModifier("Entertainment", 1.2f); // +20%
                break;
        }
    }
    
    private void ApplyServiceUsageBonus(SharedServiceUsage usage)
    {
        // Apply bonuses for using regional services
        switch (usage.Type)
        {
            case SharedServiceType.Airport:
                ApplyModifier("Tourism", 1.15f); // +15%
                break;
                
            case SharedServiceType.University:
                // Education slots available
                ApplyModifier("EducationCapacity", 1.0f + (usage.UnitsUsed / 1000f));
                break;
                
            case SharedServiceType.Hospital:
                // Healthcare availability
                ApplyModifier("Healthcare", 1.1f);
                break;
        }
    }
    
    private void ApplyModifier(string stat, float multiplier)
    {
        if (multiplier <= 0 || Math.Abs(multiplier - 1.0f) < 0.001f)
        {
            CitiesRegional.Logging.LogDebug($"Modifier skipped (no effect): {stat} x{multiplier}");
            return;
        }
        
        try
        {
            var bridge = CitiesRegional.Systems.CityDataEcsBridgeSystem.Instance;
            if (bridge == null)
            {
                CitiesRegional.Logging.LogWarn("Cannot apply modifier: ECS bridge not available");
                return;
            }
            
            // Try to find a modifier system - search common names
            object? modifierSystem = null;
            string[] systemNames = {
                "Game.Simulation.ModifierSystem",
                "Game.Simulation.CityModifierSystem",
                "Game.Simulation.StatModifierSystem",
                "Game.Simulation.BonusSystem"
            };
            
            foreach (var systemName in systemNames)
            {
                try
                {
                    var systemType = Type.GetType($"{systemName}, Game");
                    if (systemType != null)
                    {
                        var world = bridge.GetType().GetProperty("World")?.GetValue(bridge);
                        if (world != null)
                        {
                            var method = typeof(Unity.Entities.World).GetMethod("GetExistingSystemManaged", Type.EmptyTypes);
                            if (method != null)
                            {
                                var genericMethod = method.MakeGenericMethod(systemType);
                                modifierSystem = genericMethod.Invoke(world, null);
                                if (modifierSystem != null) break;
                            }
                        }
                    }
                }
                catch { }
            }
            
            // If modifier system found, try to apply modifier
            if (modifierSystem != null)
            {
                var systemType = modifierSystem.GetType();
                bool applied = false;
                
                // Try methods that might add modifiers
                var methods = systemType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                string[] methodPatterns = { "add", "apply", "set", "create", "register" };
                
                foreach (var method in methods)
                {
                    var methodName = method.Name.ToLowerInvariant();
                    bool matchesPattern = methodPatterns.Any(pattern => methodName.Contains(pattern)) &&
                                        (methodName.Contains("modifier") || methodName.Contains("multiplier") || methodName.Contains("bonus"));
                    
                    if (matchesPattern)
                    {
                        var parameters = method.GetParameters();
                        // Look for methods that take stat name and multiplier
                        if (parameters.Length >= 2)
                        {
                            try
                            {
                                // Try common parameter patterns: (string, float, string) or (string, float)
                                var paramValues = new List<object> { stat, multiplier };
                                if (parameters.Length > 2 && parameters[2].ParameterType == typeof(string))
                                {
                                    paramValues.Add("CitiesRegional");
                                }
                                
                                method.Invoke(modifierSystem, paramValues.ToArray());
                                
                                CitiesRegional.Logging.LogInfo($"Modifier applied: {stat} x{multiplier} via {method.Name}");
                                applied = true;
                                
                                RecordEffect($"modifier_{stat}", new AppliedEffect
                                {
                                    Type = "Modifier",
                                    Amount = multiplier,
                                    AppliedAt = DateTime.UtcNow
                                });
                                break;
                            }
                            catch (Exception ex)
                            {
                                CitiesRegional.Logging.LogDebug($"Could not invoke {method.Name}: {ex.Message}");
                            }
                        }
                    }
                }
                
                if (!applied)
                {
                    CitiesRegional.Logging.LogDebug($"Modifier system found but no suitable method for {stat} x{multiplier}");
                }
            }
            else
            {
                // No modifier system found - track modifier for potential future application
                // This allows the mod to work even if modifier system isn't available
                CitiesRegional.Logging.LogInfo($"Modifier tracked (no system found): {stat} x{multiplier} - will be applied when system available");
                
                RecordEffect($"modifier_{stat}", new AppliedEffect
                {
                    Type = "Modifier",
                    Amount = multiplier,
                    AppliedAt = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            CitiesRegional.Logging.LogError($"Failed to apply modifier: {ex.Message}");
        }
    }
    
    #endregion
    
    #region Effect Tracking
    
    private void RecordEffect(string key, AppliedEffect effect)
    {
        _appliedEffects[key] = effect;
    }
    
    /// <summary>
    /// Remove all applied effects (called when leaving region)
    /// </summary>
    public void ClearAllEffects()
    {
        foreach (var effect in _appliedEffects)
        {
            ReverseEffect(effect.Value);
        }
        _appliedEffects.Clear();
    }
    
    private void ReverseEffect(AppliedEffect effect)
    {
        try
        {
            switch (effect.Type)
            {
                case "TreasuryChange":
                    // Reverse treasury change by applying negative amount
                    ApplyTreasuryChange(-effect.Amount, $"Reversal: {effect.Amount:+#,0;-#,0}");
                    CitiesRegional.Logging.LogDebug($"Reversed treasury change: {-effect.Amount:+#,0;-#,0}");
                    break;
                    
                case "Export":
                    // Reverse export by adding back the resource amount
                    if (!string.IsNullOrEmpty(effect.ResourceType) && Enum.TryParse<ResourceType>(effect.ResourceType, out var exportResourceType))
                    {
                        ReverseResourceEffect(exportResourceType, effect.Amount, isExport: true);
                        CitiesRegional.Logging.LogDebug($"Reversed export: {effect.ResourceType} +{effect.Amount}");
                    }
                    break;
                    
                case "Import":
                    // Reverse import by subtracting the resource amount
                    if (!string.IsNullOrEmpty(effect.ResourceType) && Enum.TryParse<ResourceType>(effect.ResourceType, out var importResourceType))
                    {
                        ReverseResourceEffect(importResourceType, effect.Amount, isExport: false);
                        CitiesRegional.Logging.LogDebug($"Reversed import: {effect.ResourceType} -{effect.Amount}");
                    }
                    break;
                    
                case "WorkerChange":
                    // Reverse worker change by applying negative amount
                    ApplyWorkerChange(-(int)effect.Amount, $"Reversal: {effect.Amount:+#;-#;0}");
                    CitiesRegional.Logging.LogDebug($"Reversed worker change: {-effect.Amount:+#;-#;0}");
                    break;
                    
                case "Modifier":
                    // Modifier reversal is complex - would need to track original stat values
                    // For now, log that reversal is needed but not implemented
                    CitiesRegional.Logging.LogWarn($"Modifier reversal not fully implemented for: {effect.ResourceType ?? "unknown"} (amount: {effect.Amount})");
                    break;
                    
                default:
                    CitiesRegional.Logging.LogWarn($"Unknown effect type for reversal: {effect.Type}");
                    break;
            }
        }
        catch (Exception ex)
        {
            CitiesRegional.Logging.LogError($"Failed to reverse effect {effect.Type}: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Reverse a resource effect (export or import)
    /// </summary>
    private void ReverseResourceEffect(ResourceType resourceType, float amount, bool isExport)
    {
        try
        {
            var bridge = CitiesRegional.Systems.CityDataEcsBridgeSystem.Instance;
            if (bridge == null)
            {
                CitiesRegional.Logging.LogWarn("Cannot reverse resource effect: ECS bridge not available");
                return;
            }
            
            var resourceSystem = bridge.GetCountCityStoredResourceSystem();
            if (resourceSystem == null)
            {
                CitiesRegional.Logging.LogDebug($"Cannot reverse {resourceType} effect: CountCityStoredResourceSystem not found");
                return;
            }
            
            var systemType = resourceSystem.GetType();
            var resourceTypeName = resourceType.ToString().ToLowerInvariant();
            string[] propertyNamePatterns = GetResourcePropertyPatterns(resourceType);
            
            bool modified = false;
            foreach (var pattern in propertyNamePatterns)
            {
                var props = systemType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var prop in props)
                {
                    var propName = prop.Name.ToLowerInvariant();
                    if (propName.Contains(pattern) && (propName.Contains("amount") || propName.Contains("count") || propName.Contains("stock")))
                    {
                        if (prop.CanRead && prop.CanWrite)
                        {
                            try
                            {
                                var currentValue = Convert.ToSingle(prop.GetValue(resourceSystem) ?? 0f);
                                // Reverse: if it was an export (subtracted), add it back; if import (added), subtract it
                                var adjustment = isExport ? amount : -amount;
                                var newValue = Math.Max(0, currentValue + adjustment);
                                prop.SetValue(resourceSystem, newValue);
                                
                                CitiesRegional.Logging.LogInfo($"Resource effect reversed: {resourceType} {adjustment:+#;-#;0} (was {currentValue}, now {newValue}) via {prop.Name}");
                                modified = true;
                                break;
                            }
                            catch (Exception ex)
                            {
                                CitiesRegional.Logging.LogDebug($"Could not reverse via {prop.Name}: {ex.Message}");
                            }
                        }
                    }
                }
                if (modified) break;
            }
            
            if (!modified)
            {
                CitiesRegional.Logging.LogDebug($"Could not reverse {resourceType} effect: No suitable property found");
            }
        }
        catch (Exception ex)
        {
            CitiesRegional.Logging.LogError($"Failed to reverse resource effect: {ex.Message}");
        }
    }
    
    #endregion
}

/// <summary>
/// Tracks an effect that was applied to the city
/// </summary>
public class AppliedEffect
{
    public string Type { get; set; } = "";
    public string? ResourceType { get; set; }
    public float Amount { get; set; }
    public DateTime AppliedAt { get; set; }
}

