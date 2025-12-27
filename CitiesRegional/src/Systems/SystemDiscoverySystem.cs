using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Game;
using Unity.Entities;
using UnityEngine;

namespace CitiesRegional.Systems
{
    /// <summary>
    /// Logs all managed systems once per load to help you find the real targets fast.
    /// Also writes to a file for easy analysis.
    /// </summary>
    public sealed class SystemDiscoverySystem : GameSystemBase
    {
        private bool _dumped;

        // Keywords to highlight - systems containing these are likely useful
        private static readonly string[] Keywords =
        {
            "Population", "Pop",
            "Economy", "Economic", "Money", "Treasury", "Budget", "Tax", "Income", "Expense",
            "Resource", "Industrial", "Commercial", "Demand", "Trade", "Production", "Consumption",
            "Citizen", "Household", "Company", "Work", "Worker", "Employment",
            "Service", "Education", "Health", "Happiness", "Traffic", "Pollution",
            "City", "Zone", "Building", "Statistics", "Counter", "Info"
        };

        protected override void OnCreate()
        {
            base.OnCreate();
            _dumped = false;
            Debug.Log("[CitiesRegional] SystemDiscoverySystem.OnCreate()");
        }

        protected override void OnUpdate()
        {
            // Try from OnUpdate as well
            RunDiscovery();
        }

        /// <summary>
        /// Force run discovery (call when game is fully loaded)
        /// </summary>
        public void ForceRunDiscovery()
        {
            Debug.Log("[CitiesRegional] ForceRunDiscovery called");
            _dumped = false; // Reset so it runs again
            RunDiscovery();
        }

        private void RunDiscovery()
        {
            if (_dumped) return;

            var sb = new StringBuilder();
            var relevantSystems = new List<string>();
            var allSystems = new List<string>();

            try
            {
                // World.Systems exists in Unity.Entities 1.0+ (managed systems list)
                var systems = World.Systems;
                int systemCount = systems.Count;

                // Skip if systems list is too small (game not ready)
                if (systemCount < 50)
                {
                    Debug.Log($"[CitiesRegional] Discovery: Only {systemCount} systems, waiting for more...");
                    return;
                }

                _dumped = true;

                sb.AppendLine("=== CitiesRegional System Discovery ===");
                sb.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"World: {World.Name}");
                sb.AppendLine();
                sb.AppendLine($"Total Systems in World: {systemCount}");
                sb.AppendLine();

                Debug.Log($"[CitiesRegional] Discovery: Found {systemCount} systems!");

                // Categorize systems - use index-based access to avoid boxing
                var byNamespace = new Dictionary<string, List<string>>();

                for (int i = 0; i < systemCount; i++)
                {
                    var s = systems[i];
                    var typeName = s.GetType().FullName ?? "Unknown";
                    var ns = s.GetType().Namespace ?? "NoNamespace";

                    allSystems.Add(typeName);

                    if (!byNamespace.ContainsKey(ns))
                        byNamespace[ns] = new List<string>();
                    byNamespace[ns].Add(s.GetType().Name);

                    // Check if relevant
                    foreach (var kw in Keywords)
                    {
                        if (typeName.IndexOf(kw, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            relevantSystems.Add(typeName);
                            break;
                        }
                    }
                }

                // Write relevant systems first
                sb.AppendLine("=== RELEVANT SYSTEMS (matching keywords) ===");
                sb.AppendLine();
                foreach (var sys in relevantSystems.OrderBy(s => s))
                {
                    sb.AppendLine($"  {sys}");
                }
                sb.AppendLine();

                // Write by namespace
                sb.AppendLine("=== SYSTEMS BY NAMESPACE ===");
                sb.AppendLine();
                foreach (var ns in byNamespace.Keys.OrderBy(k => k))
                {
                    if (ns.StartsWith("Game.") || ns.StartsWith("Colossal."))
                    {
                        sb.AppendLine($"[{ns}] ({byNamespace[ns].Count} systems)");
                        foreach (var sys in byNamespace[ns].OrderBy(s => s))
                        {
                            sb.AppendLine($"    {sys}");
                        }
                        sb.AppendLine();
                    }
                }

                // Write all systems
                sb.AppendLine("=== ALL SYSTEMS ===");
                sb.AppendLine();
                foreach (var sys in allSystems)
                {
                    sb.AppendLine($"  {sys}");
                }

                // Write to file - use LocalLow where game logs are
                var outputDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "AppData", "LocalLow", "Colossal Order", "Cities Skylines II", "Logs");

                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                var outputPath = Path.Combine(outputDir, "CitiesRegional_Discovery.txt");

                File.WriteAllText(outputPath, sb.ToString());
                Debug.Log($"[CitiesRegional] *** Discovery complete! Written to: {outputPath}");
                Debug.Log($"[CitiesRegional] Found {relevantSystems.Count} relevant systems out of {allSystems.Count} total");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CitiesRegional] Discovery failed: {ex}");
            }
        }
    }
}
