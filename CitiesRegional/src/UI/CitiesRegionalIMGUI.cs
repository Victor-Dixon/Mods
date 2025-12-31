using System;
using UnityEngine;
using CitiesRegional.Config;
using CitiesRegional.Services;

namespace CitiesRegional.UI;

/// <summary>
/// Simple IMGUI-based debug panel for CitiesRegional
/// Press F9 to toggle the panel visibility
/// 
/// This is a temporary solution while Gooee is deprecated.
/// Can be replaced with CS2's native UI binding system later.
/// </summary>
public class CitiesRegionalIMGUI : MonoBehaviour
{
    private bool _showPanel = false;
    private Rect _windowRect = new Rect(20, 20, 350, 400);
    private RegionalManager? _regionalManager;
    private string _regionCode = "";
    private string _regionName = "";
    private Vector2 _scrollPosition;
    private bool _settingsLoaded;
    private string _cloudServerUrl = "";
    private string _syncIntervalSeconds = "";
    private string _maxExportPercentage = "";
    private string _maxCommuteMinutes = "";
    
    public void Initialize(RegionalManager regionalManager)
    {
        _regionalManager = regionalManager;
        CitiesRegional.Logging.LogInfo("CitiesRegionalIMGUI initialized - Press F9 to toggle panel");
    }
    
    private void Update()
    {
        // Toggle panel with F9
        if (Input.GetKeyDown(KeyCode.F9))
        {
            _showPanel = !_showPanel;
            CitiesRegional.Logging.LogInfo($"CitiesRegional panel: {(_showPanel ? "shown" : "hidden")}");
        }
    }
    
    private void OnGUI()
    {
        if (!_showPanel) return;
        
        // Apply a simple skin
        GUI.skin.window.normal.background = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.15f, 0.95f));
        GUI.skin.button.normal.background = MakeTex(2, 2, new Color(0.2f, 0.3f, 0.5f, 1f));
        GUI.skin.button.hover.background = MakeTex(2, 2, new Color(0.3f, 0.4f, 0.6f, 1f));
        GUI.skin.label.normal.textColor = Color.white;
        GUI.skin.textField.normal.background = MakeTex(2, 2, new Color(0.15f, 0.15f, 0.2f, 1f));
        
        _windowRect = GUI.Window(12345, _windowRect, DrawWindow, "🌐 Cities Regional");
    }
    
    private void DrawWindow(int windowId)
    {
        GUILayout.BeginVertical();

        EnsureSettingsFields();
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

        // Status section
        GUILayout.Label("━━━ Status ━━━", GUILayout.ExpandWidth(true));
        
        if (_regionalManager == null)
        {
            GUILayout.Label("⚠️ Regional Manager not initialized");
        }
        else
        {
            var region = _regionalManager.GetCurrentRegion();
            if (region != null)
            {
                GUILayout.Label($"📍 Region: {region.RegionName}");
                GUILayout.Label($"🔑 Code: {region.RegionCode}");
                GUILayout.Label($"🏙️ Cities: {region.Cities?.Count ?? 0} / {region.MaxCities}");
                GUILayout.Label($"🔗 Connections: {region.Connections?.Count ?? 0}");
                
                GUILayout.Space(10);
                if (GUILayout.Button("Leave Region"))
                {
                    _ = _regionalManager.LeaveRegion();
                }
            }
            else
            {
                GUILayout.Label("Not in a region");
                
                GUILayout.Space(10);
                GUILayout.Label("━━━ Join Region ━━━");
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Code:", GUILayout.Width(50));
                _regionCode = GUILayout.TextField(_regionCode, GUILayout.Width(150));
                GUILayout.EndHorizontal();
                
                if (GUILayout.Button("Join Region") && !string.IsNullOrEmpty(_regionCode))
                {
                    _ = _regionalManager.JoinRegion(_regionCode);
                }
                
                GUILayout.Space(10);
                GUILayout.Label("━━━ Create Region ━━━");
                
                GUILayout.BeginHorizontal();
                GUILayout.Label("Name:", GUILayout.Width(50));
                _regionName = GUILayout.TextField(_regionName, GUILayout.Width(150));
                GUILayout.EndHorizontal();
                
                if (GUILayout.Button("Create Region") && !string.IsNullOrEmpty(_regionName))
                {
                    _ = _regionalManager.CreateRegion(_regionName);
                }
            }
            
            // Sync status
            GUILayout.Space(10);
            GUILayout.Label("━━━ Sync ━━━");
            GUILayout.Label($"🔄 Status: {(_regionalManager.IsSyncing ? "Syncing..." : "Idle")}");
            
            if (GUILayout.Button("Force Sync"))
            {
                _ = _regionalManager.ForceSync();
            }
        }

        DrawSettingsPanel();

        GUILayout.Space(10);
        GUILayout.Label("Press F9 to close", GUI.skin.box);

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        
        // Make window draggable
        GUI.DragWindow();
    }
    
    private static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void EnsureSettingsFields()
    {
        if (_settingsLoaded)
        {
            return;
        }

        var settings = RegionalSettings.Instance;
        _cloudServerUrl = settings.CloudServerUrl.Value;
        _syncIntervalSeconds = settings.SyncIntervalSeconds.Value.ToString();
        _maxExportPercentage = settings.MaxExportPercentage.Value.ToString();
        _maxCommuteMinutes = settings.MaxCommuteMinutes.Value.ToString();
        _settingsLoaded = true;
    }

    private void DrawSettingsPanel()
    {
        GUILayout.Space(10);
        GUILayout.Label("━━━ Settings (Debug) ━━━");

        var settings = RegionalSettings.Instance;

        var enableP2P = GUILayout.Toggle(settings.EnableP2PMode.Value, "Enable P2P Mode");
        if (enableP2P != settings.EnableP2PMode.Value)
        {
            settings.EnableP2PMode.Value = enableP2P;
        }

        var autoTrade = GUILayout.Toggle(settings.AutoTradeEnabled.Value, "Auto Trade Enabled");
        if (autoTrade != settings.AutoTradeEnabled.Value)
        {
            settings.AutoTradeEnabled.Value = autoTrade;
        }

        var enableCommuters = GUILayout.Toggle(settings.EnableCommuters.Value, "Enable Commuters");
        if (enableCommuters != settings.EnableCommuters.Value)
        {
            settings.EnableCommuters.Value = enableCommuters;
        }

        GUILayout.Space(6);
        GUILayout.Label("Cloud Server URL");
        _cloudServerUrl = GUILayout.TextField(_cloudServerUrl);
        if (GUILayout.Button("Apply Server URL"))
        {
            settings.CloudServerUrl.Value = _cloudServerUrl.Trim();
        }

        GUILayout.Space(6);
        GUILayout.Label("Sync Interval (seconds)");
        _syncIntervalSeconds = GUILayout.TextField(_syncIntervalSeconds);
        if (GUILayout.Button("Apply Sync Interval"))
        {
            if (int.TryParse(_syncIntervalSeconds, out var value))
            {
                settings.SyncIntervalSeconds.Value = Math.Max(10, value);
                _syncIntervalSeconds = settings.SyncIntervalSeconds.Value.ToString();
            }
        }

        GUILayout.Space(6);
        GUILayout.Label("Max Export Percentage");
        _maxExportPercentage = GUILayout.TextField(_maxExportPercentage);
        if (GUILayout.Button("Apply Max Export %"))
        {
            if (int.TryParse(_maxExportPercentage, out var value))
            {
                settings.MaxExportPercentage.Value = Math.Max(0, Math.Min(100, value));
                _maxExportPercentage = settings.MaxExportPercentage.Value.ToString();
            }
        }

        GUILayout.Space(6);
        GUILayout.Label("Max Commute Minutes");
        _maxCommuteMinutes = GUILayout.TextField(_maxCommuteMinutes);
        if (GUILayout.Button("Apply Max Commute Minutes"))
        {
            if (int.TryParse(_maxCommuteMinutes, out var value))
            {
                settings.MaxCommuteMinutes.Value = Math.Max(1, value);
                _maxCommuteMinutes = settings.MaxCommuteMinutes.Value.ToString();
            }
        }
    }
}

