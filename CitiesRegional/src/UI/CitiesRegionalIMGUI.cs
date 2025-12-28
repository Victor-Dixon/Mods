using System;
using UnityEngine;
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
        
        GUILayout.Space(10);
        GUILayout.Label("Press F9 to close", GUI.skin.box);
        
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
}

