# Gooee API Discovery Results

**Date:** 2025-12-28  
**Status:** ✅ API Verified via Runtime Reflection

---

## 🎯 Key Discovery

**Gooee Plugin Type:** `Gooee.Plugin` (NOT `Gooee.Plugins.GooeePlugin`)

---

## ✅ Verified API Details

### Type Information
- **Full Name:** `Gooee.Plugin`
- **Base Class:** `BepInEx.BaseUnityPlugin`
- **Assembly:** `Gooee, Version=0.1.1.5, Culture=neutral, PublicKeyToken=null`
- **Location:** `BepInEx/plugins/Gooee/Gooee.dll`

### Properties
- `Info` (PluginInfo) - Plugin metadata
- `Config` (ConfigFile) - Configuration file access
- Standard Unity/BepInEx properties (name, enabled, gameObject, transform, etc.)

### Methods
- **No `OnSetup()` method** - Uses standard Unity `Awake()` instead
- **No `OnUpdate()` method** - Uses standard Unity `Update()` if needed
- **No `Name` property override** - Uses standard BepInEx plugin name
- Standard Unity MonoBehaviour methods (coroutines, GetComponent, etc.)

---

## 📋 Implementation Pattern

```csharp
using Gooee;

[BepInPlugin(PluginInfo.PLUGIN_GUID + ".UI", PluginInfo.PLUGIN_NAME + " UI", PluginInfo.PLUGIN_VERSION)]
[BepInDependency("Cities2Modding-Gooee-1.1.5")]
public class CitiesRegionalGooeePlugin : Gooee.Plugin
{
    private void Awake()
    {
        // Initialize UI components here
        // Get RegionalManager from main plugin
        // Set up panels/components
    }
}
```

---

## 🔍 Discovery Method

Used runtime reflection probe (`GooeeApiProbe`) to:
1. Detect Gooee assembly loading
2. Search for plugin base class types
3. Enumerate public members of `Gooee.Plugin`
4. Verify inheritance hierarchy

**Probe Log Marker:** `[GooeeProbe]`

---

## ⚠️ Important Notes

1. **No Custom Lifecycle Methods:** Gooee.Plugin uses standard Unity/BepInEx lifecycle (`Awake`, `Start`, `Update`, `OnDestroy`)

2. **Standard BepInEx Plugin:** Gooee.Plugin is essentially a BepInEx plugin with Gooee-specific properties (`Info`, `Config`)

3. **Panel Registration:** Panel registration methods not yet discovered - may be static methods or require further investigation

---

## ✅ UI-001 Status

**COMPLETE** - Gooee API verified:
- ✅ Gooee assembly detected
- ✅ Plugin base class found: `Gooee.Plugin`
- ✅ API structure understood
- ✅ Implementation pattern identified

**Next:** UI-002 - Implement GooeePlugin Inheritance

---

## 📚 References

- Gooee on Thunderstore: https://thunderstore.io/c/cities-skylines-ii/p/Cities2Modding/Gooee/
- Probe Source: `src/UI/GooeeApiProbe.cs`
- Implementation: `src/UI/CitiesRegionalGooeePlugin.cs`

