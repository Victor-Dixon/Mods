# How to Get Gooee for CS2

**Date:** 2025-12-26  
**Status:** Gooee is a Thunderstore mod, not a NuGet package

---

## ✅ How to Get Gooee

### Method 1: Thunderstore (Recommended)

**Gooee on Thunderstore:**
- URL: https://thunderstore.io/c/cities-skylines-ii/p/Cities2Modding/Gooee/
- Publisher: Cities2Modding
- Install via: Thunderstore Mod Manager or r2modman

**Steps:**
1. Install Thunderstore Mod Manager or r2modman
2. Search for "Gooee" in Cities: Skylines 2
3. Install the mod
4. Gooee DLL will be in: `BepInEx/plugins/Gooee/Gooee.dll`

### Method 2: Manual Download

1. Go to: https://thunderstore.io/c/cities-skylines-ii/p/Cities2Modding/Gooee/
2. Download the mod
3. Extract to: `BepInEx/plugins/Gooee/`
4. Reference the DLL in project

---

## 📦 Adding Gooee to Project

Once Gooee is installed, add DLL reference to `CitiesRegional.csproj`:

```xml
<ItemGroup>
  <Reference Include="Gooee">
    <HintPath>$(CS2_INSTALL)\BepInEx\plugins\Gooee\Gooee.dll</HintPath>
    <Private>false</Private>
  </Reference>
</ItemGroup>
```

Or if using relative path:
```xml
<Reference Include="Gooee">
  <HintPath>..\..\CS2\Cities.Skylines.II.v1.5.3f1\game\BepInEx\plugins\Gooee\Gooee.dll</HintPath>
  <Private>false</Private>
</Reference>
```

---

## ✅ After Adding Gooee

1. **Update CitiesRegionalUI.cs**
   - Remove conditional compilation
   - Make class inherit from `GooeePlugin`
   - Implement `OnSetup()` method

2. **Build and Test**
   - Build project
   - Launch game
   - Gooee menu should appear
   - CitiesRegional should be in menu

---

## 🔍 Verify Gooee Installation

Check if Gooee is installed:
```powershell
Test-Path "D:\mods\CS2\Cities.Skylines.II.v1.5.3f1\game\BepInEx\plugins\Gooee\Gooee.dll"
```

If `True`, Gooee is installed and ready to use.

---

**Next Step:** Install Gooee via Thunderstore, then add DLL reference to project


