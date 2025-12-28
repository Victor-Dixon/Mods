// DISABLED: Gooee is deprecated and incompatible with CS2 v1.5.3+
// This file is kept for reference but the class is commented out.
// 
// The core CitiesRegional mod works without this UI plugin.
// UI will be implemented using CS2's native binding system or IMGUI.

/*
using System;
using BepInEx;
using CitiesRegional.Services;
using CitiesRegional.Models;
using CitiesRegional.UI.Panels;
using CitiesRegional.UI.Components;
using static CitiesRegional.PluginInfo;
using Gooee;

namespace CitiesRegional.UI;

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
        
        var mainPlugin = CitiesRegional.CitiesRegionalPlugin.Instance;
        if (mainPlugin != null)
        {
            _regionalManager = mainPlugin.GetRegionalManager();
            _ui = new CitiesRegionalUI();
            _ui.Initialize(_regionalManager);
            
            _tradeDashboard = new TradeDashboardPanel();
            _tradeDashboard.Initialize(_regionalManager, _ui);
            
            _regionPanel = new RegionPanel();
            _regionPanel.Initialize(_regionalManager, _ui);
            
            _tradeDashboardComponent = new TradeDashboardComponent();
            _tradeDashboardComponent.Initialize(_tradeDashboard);
            
            _regionPanelComponent = new RegionPanelComponent();
            _regionPanelComponent.Initialize(_regionPanel);
            
            CitiesRegional.Logging.LogInfo("RegionalManager connected to GooeePlugin");
        }
    }
}
*/
