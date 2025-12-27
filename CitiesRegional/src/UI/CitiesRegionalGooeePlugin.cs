using System;
using BepInEx;
using CitiesRegional.Services;
using CitiesRegional.Models;
using CitiesRegional.UI.Panels;
using CitiesRegional.UI.Components;

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
// ACTIVATE: Uncomment when Gooee API verified
// ============================================
/*
using Gooee;
using Gooee.Plugins;

[BepInPlugin(PluginInfo.PLUGIN_GUID + ".UI", PluginInfo.PLUGIN_NAME + " UI", PluginInfo.PLUGIN_VERSION)]
[BepInDependency("Cities2Modding-Gooee-1.1.5")]
public class CitiesRegionalGooeePlugin : GooeePlugin
{
    private RegionalManager? _regionalManager;
    private CitiesRegionalUI? _ui;
    
    public override string Name => "CitiesRegional";
    
    public override void OnSetup()
    {
        CitiesRegional.Logging.LogInfo("CitiesRegional GooeePlugin OnSetup() called");
        
        // Get RegionalManager from main plugin
        var mainPlugin = CitiesRegional.CitiesRegionalPlugin.Instance;
        if (mainPlugin != null)
        {
            _regionalManager = mainPlugin.GetRegionalManager();
            _ui = new CitiesRegionalUI();
            _ui.Initialize(_regionalManager);
            
            CitiesRegional.Logging.LogInfo("RegionalManager connected to GooeePlugin");
        }
        else
        {
            CitiesRegional.Logging.LogWarn("Main plugin instance not found - GooeePlugin may not function correctly");
        }
        
        // Panel structures already initialized in placeholder
        // When Gooee API is active, register panels here:
        // RegisterPanel<TradeDashboardPanel>();
        // RegisterPanel<RegionPanel>();
    }
    
    // Panel creation methods will be implemented once Gooee API is verified
    // These will use Gooee's React component system
}
*/

// ============================================
// PLACEHOLDER: Comment out when Gooee API verified
// ============================================

// Placeholder implementation until Gooee API is verified in-game
[BepInPlugin(PluginInfo.PLUGIN_GUID + ".UI", PluginInfo.PLUGIN_NAME + " UI", PluginInfo.PLUGIN_VERSION)]
public class CitiesRegionalGooeePlugin : BepInEx.BaseUnityPlugin
{
    private RegionalManager? _regionalManager;
    private CitiesRegionalUI? _ui;
    private TradeDashboardPanel? _tradeDashboard;
    private RegionPanel? _regionPanel;
    private TradeDashboardComponent? _tradeDashboardComponent;
    private RegionPanelComponent? _regionPanelComponent;
    
    private void Awake()
    {
        CitiesRegional.Logging.LogInfo("CitiesRegional GooeePlugin placeholder initialized");
        CitiesRegional.Logging.LogWarn("GooeePlugin API not yet implemented - waiting for API verification");

        // Runtime Gooee API probe (reflection only). This makes UI-001 verification a single log grep.
        GooeeApiProbe.Log(GooeeApiProbe.Run());
        
        // Get RegionalManager from main plugin
        var mainPlugin = CitiesRegional.CitiesRegionalPlugin.Instance;
        if (mainPlugin != null)
        {
            _regionalManager = mainPlugin.GetRegionalManager();
            _ui = new CitiesRegionalUI();
            _ui.Initialize(_regionalManager);
            
            // Initialize panel structures (ready for Gooee integration)
            _tradeDashboard = new TradeDashboardPanel();
            _tradeDashboard.Initialize(_regionalManager, _ui);
            
            _regionPanel = new RegionPanel();
            _regionPanel.Initialize(_regionalManager, _ui);
            
            // Initialize React components (ready for Gooee integration)
            _tradeDashboardComponent = new TradeDashboardComponent();
            _tradeDashboardComponent.Initialize(_tradeDashboard);
            
            _regionPanelComponent = new RegionPanelComponent();
            _regionPanelComponent.Initialize(_regionPanel);
            
            CitiesRegional.Logging.LogInfo("RegionalManager connected (placeholder mode)");
            CitiesRegional.Logging.LogInfo("Panel structures initialized (TradeDashboard, RegionPanel)");
            CitiesRegional.Logging.LogInfo("React components initialized (TradeDashboardComponent, RegionPanelComponent)");
        }
    }
}
