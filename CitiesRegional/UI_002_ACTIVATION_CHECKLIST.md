# UI-002: GooeePlugin Inheritance Activation Checklist

**Task:** UI-002 - Implement GooeePlugin Inheritance  
**Priority:** P1 - High  
**Status:** ⏳ Pending (Blocked by UI-001)  
**Estimated Time:** 1-2 hours  
**Points:** 5

---

## 🎯 Prerequisites

- [ ] **UI-001 Complete**: GooeePlugin API verified and documented
- [ ] **GooeePlugin Base Class Available**: Confirmed via `[GooeeProbe]` logs
- [ ] **Gooee.dll Present**: Located at `BepInEx/plugins/Gooee.dll`
- [ ] **Build Environment**: Project builds successfully

---

## 📋 Activation Steps

### Step 1: Verify UI-001 Results

**Check BepInEx Logs for:**
```
[CitiesRegional] [GooeeProbe] Gooee.dll expected path: ... (exists=True)
[CitiesRegional] [GooeeProbe] Gooee assembly loaded: ...
[CitiesRegional] [GooeeProbe] GooeePlugin type FOUND: Gooee.Plugins.GooeePlugin
```

**If GooeePlugin type NOT found:**
- ⚠️ Document findings in `GOOEE_API_RESEARCH.md`
- ⚠️ Consider alternative UI approach (HookUI / UIToolkit)
- ⚠️ Update MASTER_TASK_LOG.md with blocker

**If GooeePlugin type FOUND:**
- ✅ Proceed to Step 2

---

### Step 2: Activate GooeePlugin Code

**File:** `src/UI/CitiesRegionalGooeePlugin.cs`

1. **Uncomment GooeePlugin Implementation** (lines 27-67):
   - Remove `/*` at line 27
   - Remove `*/` at line 68
   - This activates the `GooeePlugin` inheritance code

2. **Comment Out Placeholder Code** (lines 74-120):
   - Add `/*` before line 74 (`// Placeholder implementation...`)
   - Add `*/` after line 120 (end of placeholder class)

3. **Verify Using Statements**:
   - Ensure `using Gooee;` and `using Gooee.Plugins;` are present
   - Ensure `using static CitiesRegional.PluginInfo;` is removed (PluginInfo is now in main plugin file)

---

### Step 3: Update Plugin Attributes

**Verify BepInPlugin Attribute:**
```csharp
[BepInPlugin(PluginInfo.PLUGIN_GUID + ".UI", PluginInfo.PLUGIN_NAME + " UI", PluginInfo.PLUGIN_VERSION)]
[BepInDependency("Cities2Modding-Gooee-1.1.5")]
public class CitiesRegionalGooeePlugin : GooeePlugin
```

**Note:** `PluginInfo` is now defined in `CitiesRegionalPlugin.cs`, so use `PluginInfo.PLUGIN_GUID` syntax.

---

### Step 4: Implement Panel Registration

**In `OnSetup()` method, add panel registration:**

```csharp
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
        
        // Register panels with Gooee
        // TODO: Uncomment when Gooee panel registration API is confirmed
        // RegisterPanel<TradeDashboardPanel>();
        // RegisterPanel<RegionPanel>();
    }
    else
    {
        CitiesRegional.Logging.LogWarn("Main plugin instance not found - GooeePlugin may not function correctly");
    }
}
```

**Panel Registration Notes:**
- Panel registration API may need verification via Gooee documentation or runtime testing
- TradeDashboardPanel and RegionPanel are already initialized in placeholder code
- React components (TradeDashboardComponent, RegionPanelComponent) are ready for integration

---

### Step 5: Build and Test

1. **Build Project:**
   ```bash
   cd D:\mods\CitiesRegional
   dotnet build
   ```
   - ✅ Should build without errors
   - ⚠️ If compilation errors about Gooee types, verify Gooee.dll reference in .csproj

2. **Deploy to BepInEx:**
   - Auto-deploy should copy to `BepInEx/plugins/CitiesRegional/`
   - Or manually copy `bin/Debug/netstandard2.1/CitiesRegional.dll`

3. **Launch CS2:**
   - Start game with Gooee mod enabled
   - Load a city or create new one

4. **Check BepInEx Logs:**
   **Expected Success Logs:**
   ```
   [CitiesRegional] Cities Regional v0.1.0 loading...
   [CitiesRegional] CitiesRegional GooeePlugin OnSetup() called
   [CitiesRegional] RegionalManager connected to GooeePlugin
   ```

   **Check Gooee Menu:**
   - Look for "CitiesRegional" entry in Gooee menu
   - Menu should be accessible in-game

---

## ✅ Acceptance Criteria

- [ ] GooeePlugin code uncommented and active
- [ ] Placeholder code commented out
- [ ] Inherits from `Gooee.Plugins.GooeePlugin`
- [ ] `OnSetup()` method implemented
- [ ] Plugin appears in Gooee menu in-game
- [ ] No compilation errors
- [ ] Plugin initializes correctly (OnSetup() called)
- [ ] RegionalManager connected successfully
- [ ] No runtime errors in BepInEx logs

---

## 🐛 Troubleshooting

### Compilation Error: "GooeePlugin type not found"
- **Cause:** Gooee.dll not referenced or not found
- **Fix:** Verify `Gooee.dll` exists at `BepInEx/plugins/Gooee.dll`
- **Fix:** Check `.csproj` has Gooee reference (should be present)

### Runtime Error: "TypeLoadException"
- **Cause:** GooeePlugin base class not available at runtime
- **Fix:** Verify Gooee mod is installed and enabled
- **Fix:** Check BepInEx logs for Gooee loading errors

### Plugin Not Appearing in Gooee Menu
- **Cause:** OnSetup() not called or failed silently
- **Fix:** Check BepInEx logs for errors
- **Fix:** Verify `[BepInDependency]` attribute matches Gooee mod GUID

### RegionalManager Not Found
- **Cause:** Main plugin not initialized before UI plugin
- **Fix:** Verify main plugin loads first (check logs)
- **Fix:** Add null check and retry logic if needed

---

## 📝 Post-Activation Actions

1. **Update MASTER_TASK_LOG.md:**
   - Mark UI-001 as ✅ Complete
   - Mark UI-002 as ✅ Complete
   - Update UI-003 status to 🚧 In Progress

2. **Document Findings:**
   - Update `GOOEE_API_RESEARCH.md` with activation results
   - Note any API differences from expected behavior

3. **Next Steps:**
   - Proceed to UI-003: Create Basic Visible Panel
   - Test panel registration and visibility
   - Implement React component rendering

---

## 📚 Related Files

- `src/UI/CitiesRegionalGooeePlugin.cs` - GooeePlugin implementation
- `src/UI/CitiesRegionalUI.cs` - UI helper class
- `src/UI/Panels/TradeDashboardPanel.cs` - Trade dashboard panel
- `src/UI/Panels/RegionPanel.cs` - Region panel
- `GOOEE_API_TESTING_GUIDE.md` - UI-001 testing guide
- `GOOEE_API_RESEARCH.md` - API research findings
- `MASTER_TASK_LOG.md` - Task tracking

---

**Status:** Ready for activation after UI-001 completion  
**Last Updated:** 2025-12-27  
**Next:** Wait for UI-001 verification results

