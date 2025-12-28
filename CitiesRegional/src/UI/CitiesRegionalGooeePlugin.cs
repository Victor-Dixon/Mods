using System;
using BepInEx;
using CitiesRegional.Services;
using CitiesRegional.Models;
using CitiesRegional.UI.Panels;
using CitiesRegional.UI.Components;
using static CitiesRegional.PluginInfo;

namespace CitiesRegional.UI;

/// <summary>
/// Gooee plugin for CitiesRegional mod
/// NOTE: Gooee is deprecated and API documentation is incomplete.
/// This implementation is based on expected BepInEx plugin patterns.
/// Will need in-game testing to verify API correctness.
/// 
/// ACTIVATION INSTRUCTIONS (when API verified):
/// 1. Uncomment the GooeePlugin code below (lines with //ACTIVATE:)
/// 2. Comment out the placeholder code (lines with //PLACEHOLDER:)
/// 3. Rebuild and test in-game
/// 4. See GOOEE_API_TESTING_GUIDE.md for detailed steps
/// </summary>

// ============================================
// ACTIVATED: Gooee API verified - type is Gooee.Plugin
// ============================================
using Gooee;

[BepInPlugin(PluginInfo.PLUGIN_GUID + ".UI", PluginInfo.PLUGIN_NAME + " UI", PluginInfo.PLUGIN_VERSION)]
[BepInDependency("Gooee", BepInDependency.DependencyFlags.HardDependency)]
public class CitiesRegionalGooeePlugin : Gooee.Plugin
{
    private RegionalManager? _regionalManager;
    private CitiesRegionalUI? _ui;
    private TradeDashboardPanel? _tradeDashboard;
    private RegionPanel? _regionPanel;
    private TradeDashboardComponent? _tradeDashboardComponent;
    private RegionPanelComponent? _regionPanelComponent;
    
    private void Awake()
    {
        CitiesRegional.Logging.LogInfo("CitiesRegional GooeePlugin Awake() called");
        
        // Get RegionalManager from main plugin
        var mainPlugin = CitiesRegional.CitiesRegionalPlugin.Instance;
        if (mainPlugin != null)
        {
            _regionalManager = mainPlugin.GetRegionalManager();
            _ui = new CitiesRegionalUI();
            _ui.Initialize(_regionalManager);
            
            // Initialize panel structures
            _tradeDashboard = new TradeDashboardPanel();
            _tradeDashboard.Initialize(_regionalManager, _ui);
            
            _regionPanel = new RegionPanel();
            _regionPanel.Initialize(_regionalManager, _ui);
            
            // Initialize React components (ready for Gooee integration)
            _tradeDashboardComponent = new TradeDashboardComponent();
            _tradeDashboardComponent.Initialize(_tradeDashboard);
            
            _regionPanelComponent = new RegionPanelComponent();
            _regionPanelComponent.Initialize(_regionPanel);
            
            CitiesRegional.Logging.LogInfo("RegionalManager connected to GooeePlugin");
            CitiesRegional.Logging.LogInfo("Panel structures initialized (TradeDashboard, RegionPanel)");
            CitiesRegional.Logging.LogInfo("React components initialized (TradeDashboardComponent, RegionPanelComponent)");
            
            // Panel registration with Gooee will be implemented once panel registration API is confirmed
            // TODO: Register panels when Gooee panel registration API is verified
            // RegisterPanel<TradeDashboardPanel>();
            // RegisterPanel<RegionPanel>();
        }
        else
        {
            CitiesRegional.Logging.LogWarn("Main plugin instance not found - GooeePlugin may not function correctly");
        }
    }
    
    // Panel creation methods will be implemented once Gooee panel registration API is verified
    // These will use Gooee's React component system
}

// ============================================
// PLACEHOLDER: Commented out - Gooee API verified
// ============================================
/*
// Placeholder implementation - replaced with Gooee.Plugin inheritance
[BepInPlugin(PluginInfo.PLUGIN_GUID + ".UI", PluginInfo.PLUGIN_NAME + " UI", PluginInfo.PLUGIN_VERSION)]
public class CitiesRegionalGooeePlugin : BepInEx.BaseUnityPlugin
{
    // ... placeholder code removed ...
}
*/
