using System;
using BepInEx.Configuration;

namespace CitiesRegional.Config;

/// <summary>
/// Single source of truth for configurable regional settings.
/// </summary>
public sealed class RegionalSettings
{
    public const int DefaultSyncIntervalSeconds = 120;
    public const bool DefaultEnableP2PMode = true;
    public const string DefaultCloudServerUrl = "https://api.citiesregional.com";
    public const bool DefaultAutoTradeEnabled = true;
    public const int DefaultMaxExportPercentage = 50;
    public const bool DefaultEnableCommuters = true;
    public const int DefaultMaxCommuteMinutes = 45;

    private static RegionalSettings? _instance;

    public ConfigEntry<int> SyncIntervalSeconds { get; }
    public ConfigEntry<bool> EnableP2PMode { get; }
    public ConfigEntry<string> CloudServerUrl { get; }
    public ConfigEntry<bool> AutoTradeEnabled { get; }
    public ConfigEntry<int> MaxExportPercentage { get; }
    public ConfigEntry<bool> EnableCommuters { get; }
    public ConfigEntry<int> MaxCommuteMinutes { get; }

    private RegionalSettings(ConfigFile config)
    {
        SyncIntervalSeconds = config.Bind(
            "Sync",
            "SyncIntervalSeconds",
            DefaultSyncIntervalSeconds,
            "How often to sync with region (seconds)."
        );
        EnableP2PMode = config.Bind(
            "Sync",
            "EnableP2PMode",
            DefaultEnableP2PMode,
            "Use peer-to-peer mode instead of cloud sync."
        );
        CloudServerUrl = config.Bind(
            "Sync",
            "CloudServerUrl",
            DefaultCloudServerUrl,
            "Cloud server URL (used when P2P is disabled)."
        );
        AutoTradeEnabled = config.Bind(
            "Gameplay",
            "AutoTradeEnabled",
            DefaultAutoTradeEnabled,
            "Automatically apply regional trade effects."
        );
        MaxExportPercentage = config.Bind(
            "Gameplay",
            "MaxExportPercentage",
            DefaultMaxExportPercentage,
            "Max % of production to export (0-100)."
        );
        EnableCommuters = config.Bind(
            "Gameplay",
            "EnableCommuters",
            DefaultEnableCommuters,
            "Allow worker commuting between cities."
        );
        MaxCommuteMinutes = config.Bind(
            "Gameplay",
            "MaxCommuteMinutes",
            DefaultMaxCommuteMinutes,
            "Max commute time workers accept (minutes)."
        );
    }

    public static RegionalSettings Initialize(ConfigFile config)
    {
        _instance ??= new RegionalSettings(config);
        return _instance;
    }

    public static RegionalSettings Instance =>
        _instance ?? throw new InvalidOperationException("RegionalSettings not initialized.");

    public TimeSpan GetSyncInterval()
    {
        var seconds = Math.Max(10, SyncIntervalSeconds.Value);
        return TimeSpan.FromSeconds(seconds);
    }

    public int GetMaxCommuteMinutes()
    {
        return Math.Max(1, MaxCommuteMinutes.Value);
    }
}
