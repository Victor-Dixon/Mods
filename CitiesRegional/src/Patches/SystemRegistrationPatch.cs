using System;
using System.Reflection;
using CitiesRegional.Systems;
using Colossal.Serialization.Entities;
using Game;
using Game.Audio;
using Game.SceneFlow;
using HarmonyLib;
using UnityEngine;

namespace CitiesRegional.Patches
{
    /// <summary>
    /// Manually apply Harmony patches to register our systems.
    /// This approach is more reliable with Unity's domain reload.
    /// </summary>
    public static class SystemRegistrationPatch
    {
        private static bool _systemsRegistered = false;
        private static Harmony _harmony;

        /// <summary>
        /// Apply patches manually. Call from plugin Awake().
        /// </summary>
        public static void ApplyPatches(string harmonyId)
        {
            try
            {
                _harmony = new Harmony(harmonyId);
                
                // Patch AudioManager.OnGameLoadingComplete
                var originalMethod = typeof(AudioManager).GetMethod(
                    "OnGameLoadingComplete",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                if (originalMethod != null)
                {
                    var postfix = typeof(SystemRegistrationPatch).GetMethod(
                        nameof(AudioManager_Postfix),
                        BindingFlags.Static | BindingFlags.Public);
                    
                    _harmony.Patch(originalMethod, postfix: new HarmonyMethod(postfix));
                    Debug.Log("[CitiesRegional] Patched AudioManager.OnGameLoadingComplete");
                }
                else
                {
                    Debug.LogWarning("[CitiesRegional] Could not find AudioManager.OnGameLoadingComplete");
                }

                // Patch UpdateSystem.OnCreate
                var updateMethod = typeof(UpdateSystem).GetMethod(
                    "OnCreate",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                
                if (updateMethod != null)
                {
                    var postfix2 = typeof(SystemRegistrationPatch).GetMethod(
                        nameof(UpdateSystem_Postfix),
                        BindingFlags.Static | BindingFlags.Public);
                    
                    _harmony.Patch(updateMethod, postfix: new HarmonyMethod(postfix2));
                    Debug.Log("[CitiesRegional] Patched UpdateSystem.OnCreate");
                }
                else
                {
                    Debug.LogWarning("[CitiesRegional] Could not find UpdateSystem.OnCreate");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CitiesRegional] Patching failed: {ex}");
            }
        }

        public static void AudioManager_Postfix(AudioManager __instance, Purpose purpose, GameMode mode)
        {
            Debug.Log($"[CitiesRegional] AudioManager.OnGameLoadingComplete fired! mode={mode}");
            if (!mode.IsGameOrEditor()) return;
            RegisterSystems(__instance.World, "AudioManager");
            
            // Trigger discovery now that game is fully loaded
            try
            {
                var discovery = __instance.World.GetExistingSystemManaged<SystemDiscoverySystem>();
                if (discovery != null)
                {
                    Debug.Log("[CitiesRegional] Triggering discovery from AudioManager...");
                    discovery.ForceRunDiscovery();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CitiesRegional] Failed to trigger discovery: {ex.Message}");
            }
        }

        public static void UpdateSystem_Postfix(UpdateSystem __instance)
        {
            Debug.Log("[CitiesRegional] UpdateSystem.OnCreate fired!");
            RegisterSystems(__instance.World, "UpdateSystem");
        }

        public static void RegisterSystems(Unity.Entities.World world, string source)
        {
            if (_systemsRegistered)
            {
                Debug.Log($"[CitiesRegional] Systems already registered, skipping ({source})");
                return;
            }
            
            if (world == null)
            {
                Debug.LogError("[CitiesRegional] World is null!");
                return;
            }

            try
            {
                Debug.Log($"[CitiesRegional] Creating systems in World: {world.Name}");
                
                // Create and enable SystemDiscoverySystem
                var discovery = world.GetOrCreateSystemManaged<SystemDiscoverySystem>();
                discovery.Enabled = true;
                Debug.Log("[CitiesRegional] SystemDiscoverySystem created and enabled");
                
                // Create and enable CityDataEcsBridgeSystem
                var bridge = world.GetOrCreateSystemManaged<CityDataEcsBridgeSystem>();
                bridge.Enabled = true;
                Debug.Log("[CitiesRegional] CityDataEcsBridgeSystem created and enabled");
                
                _systemsRegistered = true;
                Debug.Log("[CitiesRegional] *** SUCCESS: Systems registered and enabled! ***");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CitiesRegional] System creation failed: {ex}");
            }
        }
    }
}

