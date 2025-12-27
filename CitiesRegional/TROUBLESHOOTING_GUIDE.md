# Troubleshooting Guide - Cities Regional Mod

**Version:** 0.1.0  
**Last Updated:** 2025-12-27

---

## 📋 Quick Reference

| Issue | Quick Fix | Full Details |
|-------|-----------|--------------|
| Mod not loading | Check BepInEx installation | [Mod Not Loading](#mod-not-loading) |
| Game crashes | Check log files | [Game Crashes](#game-crashes) |
| Sync not working | Verify server connection | [Sync Issues](#sync-issues) |
| UI not appearing | Check Gooee installation | [UI Issues](#ui-issues) |
| Data collection errors | Check ECS bridge logs | [Data Collection](#data-collection-issues) |

---

## 🔍 Common Issues

### Mod Not Loading

**Symptoms:**
- Mod doesn't appear in BepInEx log
- No `[CitiesRegional]` entries in logs
- Game loads but mod features unavailable

**Diagnosis Steps:**

1. **Check BepInEx Installation**
   ```powershell
   # Verify BepInEx is installed
   Test-Path "$env:CS2_INSTALL\BepInEx\core\BepInEx.dll"
   ```

2. **Check Plugin Location**
   ```powershell
   # Verify plugin DLL exists
   Test-Path "$env:CS2_INSTALL\BepInEx\plugins\CitiesRegional.dll"
   ```

3. **Check Log Files**
   - **BepInEx Log:** `%USERPROFILE%\AppData\LocalLow\BepInEx\LogOutput.log`
   - Look for: `[CitiesRegional] Plugin Awake() called`
   - Look for: `Cities Regional v0.1.0 loading...`

**Solutions:**

- **BepInEx not installed:**
  - Install BepInEx 5.x from Thunderstore or GitHub
  - Ensure `winhttp.dll` is in CS2 game directory

- **Plugin DLL missing:**
  - Rebuild the mod: `dotnet build`
  - Check auto-deploy path in `CitiesRegional.csproj`
  - Manually copy `CitiesRegional.dll` to BepInEx plugins folder

- **Dependency missing:**
  - Ensure all dependencies are in `BepInEx\plugins` folder
  - Check for missing `.dll` files in error logs

- **Version mismatch:**
  - Ensure BepInEx version matches mod requirements
  - Check game version compatibility

---

### Game Crashes

**Symptoms:**
- Game crashes on startup
- Game crashes when loading city
- Game crashes during gameplay

**Diagnosis Steps:**

1. **Check Crash Logs**
   - **Unity Player Log:** `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Player.log`
   - **BepInEx Log:** `%USERPROFILE%\AppData\LocalLow\BepInEx\LogOutput.log`
   - Look for stack traces mentioning `CitiesRegional`

2. **Check Error Messages**
   - Look for `[CitiesRegional] ERROR` entries
   - Look for `Exception` or `NullReferenceException`
   - Check Harmony patch errors

**Common Causes & Solutions:**

- **Harmony Patch Failure:**
  ```
  [CitiesRegional] Harmony patching failed: ...
  ```
  - **Solution:** Check Harmony version compatibility
  - **Solution:** Disable other mods to check for conflicts
  - **Solution:** Check `SystemRegistrationPatch.cs` for errors

- **Missing Game Systems:**
  ```
  [CitiesRegional] ERROR: System not found: ...
  ```
  - **Solution:** Game update may have changed system names
  - **Solution:** Check `SystemDiscoveryBootstrap.cs` logs
  - **Solution:** Update mod for latest game version

- **Null Reference Exception:**
  ```
  NullReferenceException: Object reference not set to an instance
  ```
  - **Solution:** Check `CityDataEcsBridgeSystem.cs` for null checks
  - **Solution:** Ensure World is initialized before accessing systems
  - **Solution:** Check `RegionalManager` initialization order

- **Out of Memory:**
  ```
  OutOfMemoryException: ...
  ```
  - **Solution:** Reduce sync frequency in `RegionalManager`
  - **Solution:** Check for memory leaks in data collection
  - **Solution:** Reduce city data snapshot frequency

---

### Sync Issues

**Symptoms:**
- City data not syncing
- Cannot create or join regions
- Sync errors in logs
- Other cities not appearing

**Diagnosis Steps:**

1. **Check Connection Status**
   ```csharp
   // In-game, check RegionalManager status
   var manager = CitiesRegionalPlugin.Instance.GetRegionalManager();
   Debug.Log($"Connected: {manager.IsConnected}");
   Debug.Log($"Last Sync: {manager.LastSyncTime}");
   ```

2. **Check Sync Logs**
   - Look for: `[CitiesRegional] Performing regional sync...`
   - Look for: `[CitiesRegional] Sync error: ...`
   - Check network connectivity

3. **Check Server Status**
   - Verify regional server is running (if using local server)
   - Check server URL in configuration
   - Test server endpoint: `GET /api/regions/health`

**Common Causes & Solutions:**

- **Server Not Running:**
  ```
  [CitiesRegional] Sync error: Unable to connect to server
  ```
  - **Solution:** Start regional server: `dotnet run --project RegionalServer`
  - **Solution:** Check server URL in `CloudRegionalSync.cs`
  - **Solution:** Verify firewall allows connections

- **Network Timeout:**
  ```
  [CitiesRegional] Sync error: The operation timed out
  ```
  - **Solution:** Increase timeout in `CloudRegionalSync.cs`
  - **Solution:** Check network connection
  - **Solution:** Verify server is accessible

- **Invalid Region Code:**
  ```
  [CitiesRegional] ERROR: Invalid region code
  ```
  - **Solution:** Verify region code format (6 characters)
  - **Solution:** Check region exists on server
  - **Solution:** Ensure region hasn't expired

- **Authentication Failed:**
  ```
  [CitiesRegional] Sync error: Unauthorized
  ```
  - **Solution:** Check API key configuration
  - **Solution:** Verify server authentication settings
  - **Solution:** Check city ID format

---

### UI Issues

**Symptoms:**
- UI panel not appearing
- UI buttons not working
- UI shows incorrect data
- UI crashes game

**Diagnosis Steps:**

1. **Check Gooee Installation**
   - Verify Gooee mod is installed via Thunderstore
   - Check Gooee version compatibility
   - Look for Gooee initialization in logs

2. **Check UI Logs**
   - Look for: `[GooeeProbe]` entries in BepInEx log
   - Check: `[CitiesRegional] UI initialized`
   - Look for React/JavaScript errors

3. **Check Plugin Status**
   - Verify `CitiesRegionalGooeePlugin.cs` is loading
   - Check for placeholder vs. real implementation
   - Ensure UI components are registered

**Common Causes & Solutions:**

- **Gooee Not Installed:**
  ```
  [GooeeProbe] Gooee.dll not found
  ```
  - **Solution:** Install Gooee from Thunderstore
  - **Solution:** Verify Gooee is in BepInEx plugins folder
  - **Solution:** Check Gooee version compatibility

- **UI Plugin Not Loading:**
  ```
  [CitiesRegional] ERROR: UI initialization failed
  ```
  - **Solution:** Check `CitiesRegionalGooeePlugin.cs` for errors
  - **Solution:** Verify React components are built
  - **Solution:** Check UI asset paths

- **UI Not Responding:**
  - **Solution:** Check UI event handlers in `CitiesRegionalUI.cs`
  - **Solution:** Verify RegionalManager is initialized
  - **Solution:** Check UI update frequency

- **UI Shows Wrong Data:**
  - **Solution:** Verify data binding in React components
  - **Solution:** Check `RegionalManager.LocalCityData` updates
  - **Solution:** Ensure sync is working correctly

---

### Data Collection Issues

**Symptoms:**
- Population shows as 0
- Economy data incorrect
- Resources not tracked
- Metrics not updating

**Diagnosis Steps:**

1. **Check ECS Bridge Logs**
   - Look for: `[ECS] CityDataEcsBridgeSystem.OnUpdate()`
   - Check: `[ECS] Population=X` entries
   - Verify update frequency (every ~4 seconds)

2. **Check System Discovery**
   - Look for: `[ECS] Systems discovered: ...`
   - Verify required systems are found
   - Check for system name changes

3. **Check Data Collection Logs**
   - Look for: `[CitiesRegional] Collecting city data...`
   - Check: `[CitiesRegional] Data collection error: ...`
   - Verify data values are reasonable

**Common Causes & Solutions:**

- **Systems Not Discovered:**
  ```
  [ECS] ERROR: Required system not found
  ```
  - **Solution:** Check `SystemDiscoveryBootstrap.cs` logs
  - **Solution:** Verify Harmony patches are applied
  - **Solution:** Check game version compatibility

- **Population Always Zero:**
  ```
  [ECS] Population: 0
  ```
  - **Solution:** Check `CityDataEcsBridgeSystem.cs` population reading
  - **Solution:** Verify PopulationSystem is accessible
  - **Solution:** Check city has citizens

- **Treasury Not Updating:**
  ```
  [ECS] Treasury: 0
  ```
  - **Solution:** Check EconomySystem access
  - **Solution:** Verify treasury reading logic
  - **Solution:** Check game economy is active

- **Resources Not Collected:**
  ```
  [ECS] Resources: Empty
  ```
  - **Solution:** Check ResourceSystem discovery
  - **Solution:** Verify resource reading methods
  - **Solution:** Ensure city produces/consumes resources

---

### Trade System Issues

**Symptoms:**
- Trade flows not calculating
- Trade matching errors
- Trade effects not applying
- Trade statistics incorrect

**Diagnosis Steps:**

1. **Check Trade Calculation Logs**
   - Look for: `[CitiesRegional] Calculating trade flows...`
   - Check: `[CitiesRegional] Trade flow validation found X errors`
   - Verify trade flow results

2. **Check Region Connections**
   - Verify cities are in same region
   - Check connections exist between cities
   - Verify connection capacity

3. **Check Trade Validation**
   - Look for validation errors in logs
   - Check trade flow amounts
   - Verify resource data accuracy

**Common Causes & Solutions:**

- **No Trade Flows:**
  ```
  [CitiesRegional] Trade flows: 0
  ```
  - **Solution:** Verify cities have export/import needs
  - **Solution:** Check connections exist between cities
  - **Solution:** Ensure resource data is accurate

- **Trade Validation Errors:**
  ```
  [CitiesRegional] Trade flow validation found errors
  ```
  - **Solution:** Check `TradeFlowCalculator.ValidateTradeFlows()` output
  - **Solution:** Verify city IDs are valid
  - **Solution:** Check connection IDs match

- **Trade Effects Not Applied:**
  ```
  [CitiesRegional] ERROR: Effect application failed
  ```
  - **Solution:** Check `RegionalEffectsApplicator.cs`
  - **Solution:** Verify game systems allow modification
  - **Solution:** Check effect application logs

---

## 🔧 Advanced Troubleshooting

### Enable Debug Logging

**BepInEx Configuration:**
```ini
# BepInEx.cfg
[Logging.Console]
Enabled = true
LogLevel = Debug
```

**CitiesRegional Debug Mode:**
```csharp
// In CitiesRegionalPlugin.cs, enable debug logging
Logging.LogDebug("Debug mode enabled");
```

### Check System Discovery

**View Discovered Systems:**
```csharp
// Check SystemDiscoveryBootstrap logs
// Look for: [ECS] Systems discovered: System1, System2, ...
```

**Manual System Check:**
```csharp
var world = World.DefaultGameObjectInjectionWorld;
var systems = world.Systems;
foreach (var system in systems)
{
    Debug.Log($"System: {system.GetType().Name}");
}
```

### Verify Harmony Patches

**Check Patch Status:**
```csharp
// In CitiesRegionalPlugin.cs
var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
var patches = harmony.GetPatchedMethods();
foreach (var method in patches)
{
    Debug.Log($"Patched: {method.Name}");
}
```

### Test Data Collection

**Manual Data Collection Test:**
```csharp
var collector = new CityDataCollector();
var data = collector.CollectCurrentData();
Debug.Log($"Population: {data.Population}");
Debug.Log($"Treasury: {data.Treasury}");
Debug.Log($"Resources: {data.Resources.Count}");
```

---

## 📝 Log File Locations

| Log Type | Location |
|----------|----------|
| **BepInEx Log** | `%USERPROFILE%\AppData\LocalLow\BepInEx\LogOutput.log` |
| **Unity Player Log** | `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Player.log` |
| **Discovery Log** | `%USERPROFILE%\AppData\LocalLow\Colossal Order\Cities Skylines II\Logs\CitiesRegional_Discovery.txt` |
| **Config File** | `%USERPROFILE%\AppData\LocalLow\BepInEx\config\BepInEx.cfg` |

---

## 🆘 Getting Help

### Before Asking for Help

1. **Collect Information:**
   - BepInEx log file
   - Unity Player log file
   - Game version
   - Mod version
   - List of other mods installed

2. **Describe the Issue:**
   - What were you doing when it happened?
   - What did you expect to happen?
   - What actually happened?
   - Can you reproduce it?

3. **Check Known Issues:**
   - Review this troubleshooting guide
   - Check GitHub issues
   - Search Discord for similar problems

### Where to Get Help

- **GitHub Issues:** Report bugs and request features
- **Discord:** Ask questions and get community support
- **Documentation:** Check `README.md`, `USER_GUIDE.md`, `API_DOCUMENTATION.md`

---

## 🔄 Reset & Clean Install

If all else fails, try a clean install:

1. **Backup Your Saves**
   - Copy city save files to safe location

2. **Remove Mod Files**
   ```powershell
   Remove-Item "$env:CS2_INSTALL\BepInEx\plugins\CitiesRegional.dll"
   Remove-Item "$env:CS2_INSTALL\BepInEx\plugins\CitiesRegional.pdb"
   ```

3. **Clear Logs**
   ```powershell
   Remove-Item "$env:USERPROFILE\AppData\LocalLow\BepInEx\LogOutput.log"
   ```

4. **Rebuild and Reinstall**
   ```bash
   dotnet clean
   dotnet build
   # Auto-deploy should copy files
   ```

5. **Test in Clean City**
   - Load a new city
   - Check logs for initialization
   - Verify basic functionality

---

**Last Updated:** 2025-12-27  
**For more help:** See `INSTALLATION_GUIDE.md` and `USER_GUIDE.md`

