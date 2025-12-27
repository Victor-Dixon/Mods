using System;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using CitiesRegional.Services;
using CitiesRegional.Bootstrap;
using CitiesRegional.Patches;
using CitiesRegional.UI;
using UnityEngine;

namespace CitiesRegional;

/// <summary>
/// Cities Regional - Connect your city to friends' cities in a shared regional economy.
/// 
/// Features:
/// - Trade resources between cities
/// - Workers can commute to neighbor cities
/// - Share regional services (airport, university)
/// - Build highway/rail connections
/// - Compete on leaderboards
/// </summary>
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class CitiesRegionalPlugin : BaseUnityPlugin
{
    public static CitiesRegionalPlugin? Instance { get; private set; }

    private RegionalManager? _regionalManager;
    private CitiesRegionalUI? _ui;
    private Harmony? _harmony;
    private SystemDiscoveryBootstrap? _discovery;
    private bool _isInitialized;

    // Static logging helpers for systems/patches
    internal static void LogInfo(string msg) => Logging.LogInfo(msg);
    internal static void LogWarn(string msg) => Logging.LogWarning(msg);
    internal static void LogDebug(string msg) => Logging.LogDebug(msg);
    internal static void LogError(string msg) => Logging.LogError(msg);

    private void Awake()
    {
        Instance = this;

        // Initialize unified logging
        Logging.Init(Logger);

        Logging.LogInfo($"Cities Regional v{PluginInfo.PLUGIN_VERSION} loading...");
        Debug.Log("[CitiesRegional] Plugin Awake() called");

        // Apply Harmony patches manually for better control
        try
        {
            var harmonyId = PluginInfo.PLUGIN_GUID + "_Harmony";
            SystemRegistrationPatch.ApplyPatches(harmonyId);
            Logging.LogInfo("Applied Harmony patches manually.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CitiesRegional] Harmony patching failed: {ex}");
            Logging.LogError($"Harmony patching failed: {ex.Message}");
        }

        // Start system discovery bootstrap (fallback if Harmony patch doesn't work)
        // This waits for the World to be ready, then registers SystemDiscoverySystem
        _discovery = new SystemDiscoveryBootstrap(this);
        _discovery.Start();

        // Initialize the Regional Manager
        try
        {
            _regionalManager = new RegionalManager();
            Logging.LogInfo("Regional Manager initialized.");

            // Initialize UI
            _ui = new CitiesRegional.UI.CitiesRegionalUI();
            _ui.Initialize(_regionalManager);
            Logging.LogInfo("UI initialized (Gooee reference added).");
        }
        catch (Exception ex)
        {
            Logging.LogError($"Failed to initialize Regional Manager: {ex}");
        }

        _isInitialized = true;
        Logging.LogInfo("Cities Regional loaded successfully!");
        Debug.Log("[CitiesRegional] Plugin loaded successfully!");
    }

    private void OnDestroy()
    {
        Logging.LogInfo("Cities Regional unloading...");

        _regionalManager?.Dispose();
        _regionalManager = null;
        _discovery = null;
        _isInitialized = false;

        _harmony?.UnpatchSelf();

        Logging.LogInfo("Cities Regional unloaded.");
    }

    /// <summary>
    /// Get the Regional Manager instance
    /// </summary>
    public RegionalManager GetRegionalManager()
    {
        return _regionalManager ?? throw new InvalidOperationException("Regional Manager not initialized");
    }
}

/// <summary>
/// Plugin metadata
/// </summary>
public static class PluginInfo
{
    public const string PLUGIN_GUID = "com.citiesregional.mod";
    public const string PLUGIN_NAME = "Cities Regional";
    public const string PLUGIN_VERSION = "0.1.0";
}
