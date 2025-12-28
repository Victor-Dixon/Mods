using System;
using BepInEx;
using CitiesRegional.Services;
using CitiesRegional.Models;
using CitiesRegional.UI.Panels;
using CitiesRegional.UI.Components;
using static CitiesRegional.PluginInfo;
using Gooee;

namespace CitiesRegional.UI;

/// <summary>
/// Gooee plugin for CitiesRegional mod
/// </summary>
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
        }
        else
        {
            CitiesRegional.Logging.LogWarn("Main plugin instance not found - GooeePlugin may not function correctly");
        }
    }
}
