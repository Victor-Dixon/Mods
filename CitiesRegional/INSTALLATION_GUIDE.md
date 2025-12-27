# Installation Guide - Cities Regional Mod

**Version:** 0.1.0  
**Last Updated:** 2025-12-27  
**Status:** Development Build

---

## 📋 Prerequisites

Before installing Cities Regional, ensure you have:

- **Cities: Skylines 2** installed and updated
- **BepInEx 5.x** installed in your CS2 game directory
- **.NET 6.0 SDK or later** (for building from source)
- **Gooee UI Framework** (optional, for UI features - install via Thunderstore)

---

## 🚀 Installation Methods

### Method 1: Build from Source (Development)

**Step 1: Clone or Download the Repository**
```bash
git clone <repository-url>
cd CitiesRegional
```

**Step 2: Set Environment Variables**

**Windows PowerShell:**
```powershell
$env:CS2_INSTALL = "C:\Program Files (x86)\Steam\steamapps\common\Cities Skylines II"
$env:BEPINEX_PLUGINS = "$env:CS2_INSTALL\BepInEx\plugins"
```

**Windows CMD:**
```cmd
set CS2_INSTALL=C:\Program Files (x86)\Steam\steamapps\common\Cities Skylines II
set BEPINEX_PLUGINS=%CS2_INSTALL%\BepInEx\plugins
```

**Linux/Mac:**
```bash
export CS2_INSTALL="/path/to/Cities Skylines II"
export BEPINEX_PLUGINS="$CS2_INSTALL/BepInEx/plugins"
```

**Step 3: Build the Mod**
```bash
dotnet build --configuration Debug
```

The mod will automatically deploy to `BepInEx/plugins/CitiesRegional/` if `BEPINEX_PLUGINS` is set.

**Step 4: Verify Installation**
- Check that `CitiesRegional.dll` exists in `BepInEx/plugins/CitiesRegional/`
- Launch CS2 and check BepInEx logs for `[CitiesRegional]` entries

---

### Method 2: Manual Installation (Release Build)

**Step 1: Download Release**
- Download the latest release from GitHub/Thunderstore
- Extract the ZIP file

**Step 2: Copy Files**
- Copy `CitiesRegional.dll` to: `BepInEx/plugins/CitiesRegional/`
- Ensure the directory structure is:
  ```
  BepInEx/
  └── plugins/
      └── CitiesRegional/
          └── CitiesRegional.dll
  ```

**Step 3: Verify**
- Launch CS2
- Check `BepInEx/LogOutput.log` for successful loading

---

## 🔧 Installing Dependencies

### BepInEx 5 Installation

1. **Download BepInEx 5:**
   - Visit: https://github.com/BepInEx/BepInEx/releases
   - Download `BepInEx_x64_5.x.x.x.zip`

2. **Extract to Game Directory:**
   - Extract to: `Cities Skylines II/game/`
   - Structure should be:
     ```
     game/
     ├── BepInEx/
     │   ├── core/
     │   ├── plugins/
     │   └── config/
     └── Cities2_Data/
     ```

3. **First Launch:**
   - Launch CS2 once to initialize BepInEx
   - Check for `BepInEx/LogOutput.log` file

### Gooee UI Framework (Optional)

**For UI Features (Recommended):**

1. **Via Thunderstore Mod Manager:**
   - Install Thunderstore Mod Manager or r2modman
   - Search for "Gooee" by Cities2Modding
   - Install the mod

2. **Manual Installation:**
   - Download from: https://thunderstore.io/c/cities-skylines-ii/p/Cities2Modding/Gooee/
   - Extract to: `BepInEx/plugins/Gooee/`
   - Ensure `Gooee.dll` is present

**Note:** Gooee is deprecated but still functional. UI features require Gooee for now.

---

## ✅ Verification

### Check Installation

**1. File Check:**
```powershell
# Windows PowerShell
Test-Path "$env:CS2_INSTALL\BepInEx\plugins\CitiesRegional\CitiesRegional.dll"
```

**2. Log Check:**
After launching CS2, check `BepInEx/LogOutput.log` for:
```
[CitiesRegional] Cities Regional v0.1.0 loading...
[CitiesRegional] Regional Manager initialized.
[CitiesRegional] Cities Regional loaded successfully!
```

**3. In-Game Check:**
- Launch CS2
- Load a city
- Check for Cities Regional UI (if Gooee installed)
- Verify no errors in BepInEx logs

---

## 🐛 Troubleshooting

### Mod Not Loading

**Problem:** Cities Regional doesn't appear in logs

**Solutions:**
1. Verify `CitiesRegional.dll` is in correct location
2. Check BepInEx version (must be 5.x)
3. Verify game version compatibility
4. Check `BepInEx/LogOutput.log` for errors

### Build Errors

**Problem:** `dotnet build` fails

**Solutions:**
1. Verify .NET SDK version: `dotnet --version` (should be 6.0+)
2. Check `CS2_INSTALL` environment variable is set correctly
3. Verify game DLLs exist at expected paths
4. Check `CitiesRegional.csproj` for correct paths

### Gooee Not Found

**Problem:** UI features don't work, Gooee errors

**Solutions:**
1. Install Gooee via Thunderstore
2. Verify `Gooee.dll` exists in `BepInEx/plugins/Gooee/`
3. Check Gooee version compatibility
4. See `GOOEE_API_TESTING_GUIDE.md` for troubleshooting

### Runtime Errors

**Problem:** Game crashes or mod errors

**Solutions:**
1. Check `BepInEx/LogOutput.log` for error details
2. Verify all dependencies are installed
3. Check game version compatibility
4. Disable other mods to test for conflicts
5. Report issue with log file attached

---

## 📁 File Locations

### Important Paths

| Item | Default Location |
|------|-----------------|
| **Game Install** | `C:\Program Files (x86)\Steam\steamapps\common\Cities Skylines II` |
| **BepInEx** | `{Game Install}\game\BepInEx\` |
| **Mod DLL** | `{BepInEx}\plugins\CitiesRegional\CitiesRegional.dll` |
| **Config File** | `{BepInEx}\config\com.citiesregional.mod.cfg` |
| **Log File** | `{BepInEx}\LogOutput.log` |
| **Gooee DLL** | `{BepInEx}\plugins\Gooee\Gooee.dll` |

### Custom Install Locations

If CS2 is installed in a custom location:
- Update `CS2_INSTALL` environment variable
- Update paths in `CitiesRegional.csproj` if building from source

---

## 🔄 Updating the Mod

### From Source

```bash
git pull
dotnet build --configuration Debug
```

The mod will auto-deploy if `BEPINEX_PLUGINS` is set.

### From Release

1. Download new release
2. Replace `CitiesRegional.dll` in `BepInEx/plugins/CitiesRegional/`
3. Launch game to verify

---

## 🗑️ Uninstallation

**To Remove Cities Regional:**

1. Delete folder: `BepInEx/plugins/CitiesRegional/`
2. Delete config: `BepInEx/config/com.citiesregional.mod.cfg` (optional)
3. Launch game to verify removal

**Note:** Removing Cities Regional does not affect your save games or other mods.

---

## 📚 Additional Resources

- **README.md** - Project overview and features
- **GOOEE_API_TESTING_GUIDE.md** - UI testing guide
- **MASTER_TASK_LOG.md** - Development status
- **MISSION_BRIEFING.md** - Project architecture

---

## 🆘 Getting Help

**If you encounter issues:**

1. Check `BepInEx/LogOutput.log` for errors
2. Review troubleshooting section above
3. Check GitHub Issues for known problems
4. Report new issues with:
   - Game version
   - BepInEx version
   - Cities Regional version
   - Log file excerpt
   - Steps to reproduce

---

**Status:** Ready for installation  
**Last Updated:** 2025-12-27

