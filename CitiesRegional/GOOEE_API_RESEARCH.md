# Gooee API Research

**Date:** 2025-12-26  
**Status:** ⚠️ API Documentation Incomplete

---

## 🔍 Research Findings

### Gooee Status
- **Deprecated:** Marked as deprecated on Thunderstore
- **Documentation:** "Work in progress" per README
- **Dependencies:** BepInEx, HookUI (optional)

### API Discovery Challenges
1. **DLL Inspection:** Cannot load Gooee.dll without game dependencies (Game.dll, Colossal.UI.Binding.dll)
2. **GitHub:** No clear examples found in repository
3. **Documentation:** Incomplete per README

### Added: In-Game Runtime Reflection Probe (Recommended)
To avoid offline DLL inspection issues, the mod now includes a reflection-only probe that runs at plugin startup and logs what it can detect **from already-loaded assemblies**.

**Log marker:** `[GooeeProbe]`

**What it reports:**
- Whether a `Gooee` assembly is present in the current AppDomain
- Whether `Gooee.Plugins.GooeePlugin` exists
- Whether expected members exist (`Name`, `OnSetup`, `OnUpdate`)

**Where it runs:**
- `src/UI/CitiesRegionalGooeePlugin.cs` (placeholder) during `Awake()`

**Source:**
- `src/UI/GooeeApiProbe.cs`

### Expected Pattern (Based on BepInEx Standards)
```csharp
public class MyGooeePlugin : GooeePlugin
{
    public override string Name => "MyPlugin";
    
    public override void OnSetup()
    {
        // Setup UI components
    }
}
```

---

## ⚠️ Blocker

**Cannot verify GooeePlugin API without:**
- Running game (to load dependencies)
- Complete documentation
- Working examples

---

## 🎯 Next Steps

1. **Option A:** Implement based on expected pattern, test in-game
2. **Option B:** Check if HookUI is required and install it
3. **Option C:** Consider alternative UI framework (if Gooee proves unworkable)

---

**Status:** Research complete, implementation pending API verification


