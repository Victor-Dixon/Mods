# GooeePlugin API Testing Guide

**Task:** UI-001 - GooeePlugin API Verification  
**Priority:** P0 - Critical  
**Estimated Time:** 30 minutes  
**Status:** Ready for Testing

---

## 🎯 Objective

Verify GooeePlugin API availability and structure in-game to determine if we can proceed with GooeePlugin implementation or need an alternative approach.

---

## 📋 Pre-Testing Checklist

### Prerequisites
- [ ] Cities: Skylines 2 installed and updated
- [ ] BepInEx 5 installed and working
- [ ] Gooee mod installed in `BepInEx/plugins/Gooee.dll`
- [ ] CitiesRegional mod built and deployed to `BepInEx/plugins/CitiesRegional/`
- [ ] Game can launch successfully

### Files to Verify
- [ ] `BepInEx/plugins/Gooee.dll` exists
- [ ] `BepInEx/plugins/CitiesRegional/CitiesRegional.dll` exists (main plugin)
- [ ] `BepInEx/plugins/CitiesRegional/CitiesRegional.dll` exists (UI plugin - same DLL, different class)

---

## 🧪 Testing Steps

### Step 1: Launch Game
1. Launch Cities: Skylines 2
2. Wait for game to fully load
3. Load a city or create a new one
4. Wait for city to fully initialize (~10-30 seconds)

### Step 2: Check BepInEx Logs
**Location:** `D:\mods\CS2\Cities.Skylines.II.v1.5.3f1\game\BepInEx\LogOutput.log`

**Look for:**
```
[CitiesRegional] Cities Regional v0.1.0 loading...
[CitiesRegional] CitiesRegional GooeePlugin placeholder initialized
[CitiesRegional] GooeePlugin API not yet implemented - waiting for API verification
[CitiesRegional] [GooeeProbe] start (utc=...)
[CitiesRegional] [GooeeProbe] Gooee assembly loaded: ...
[CitiesRegional] [GooeeProbe] GooeePlugin type FOUND: Gooee.Plugins.GooeePlugin
[CitiesRegional] RegionalManager connected (placeholder mode)
```

**Expected Results:**
- ✅ Main plugin loads successfully
- ✅ GooeePlugin placeholder initializes
- ✅ GooeeProbe logs appear (confirms the placeholder ran)
- ✅ No errors related to Gooee.dll loading
- ⚠️ If errors about Gooee.dll missing dependencies, note them

**How to interpret GooeeProbe output:**
- **Gooee present + API found**:
  - `Gooee.dll expected path: ... (exists=True)`
  - `Gooee assembly loaded: ...`
  - `GooeePlugin type FOUND: Gooee.Plugins.GooeePlugin`
- **Gooee missing / not loaded**:
  - `Gooee.dll expected path: ... (exists=False)` (or path not probed if env vars not set)
  - `Gooee assembly NOT detected among loaded assemblies.`
  - `GooeePlugin type NOT found ...`

### Step 3: Check for Gooee Menu
1. Look for Gooee menu/button in-game UI
2. Check if "CitiesRegional" appears in Gooee menu
3. Note: Placeholder won't show in menu yet (that's expected)

**Expected Results:**
- ✅ Gooee framework loads (if Gooee menu appears)
- ⚠️ CitiesRegional won't appear yet (placeholder mode)

### Step 4: Verify GooeePlugin Base Class
**This requires runtime reflection (now built-in) or activating the GooeePlugin code:**

**Option A: Check Logs for Errors**
- Look for TypeLoadException or MissingMethodException
- If no errors AND GooeeProbe reports `GooeePlugin type FOUND`, the base class is available

**Option B: Try Activating GooeePlugin Code**
1. Uncomment GooeePlugin code in `CitiesRegionalGooeePlugin.cs`
2. Comment out placeholder code
3. Rebuild and redeploy
4. Launch game again
5. Check logs for:
   - ✅ Success: "CitiesRegional GooeePlugin OnSetup() called"
   - ❌ Failure: TypeLoadException or compilation errors

---

## 📊 Test Results Template

### Test Date: _______________
### Tester: _______________

#### Prerequisites
- [ ] All prerequisites met

#### Step 1: Game Launch
- [ ] Game launched successfully
- [ ] City loaded successfully
- [ ] No crashes

#### Step 2: BepInEx Logs
- [ ] Main plugin loaded: `[CitiesRegional] Cities Regional v0.1.0 loading...`
- [ ] GooeePlugin placeholder initialized
- [ ] RegionalManager connected
- [ ] No errors about Gooee.dll

**Errors Found:**
```
[Paste any errors here]
```

#### Step 3: Gooee Menu
- [ ] Gooee menu/button visible in-game
- [ ] Gooee framework appears to be working
- [ ] CitiesRegional not in menu (expected for placeholder)

#### Step 4: GooeePlugin API Verification
- [ ] Attempted to activate GooeePlugin code
- [ ] Result: [ ] Success [ ] Failure

**If Success:**
- [ ] OnSetup() method called
- [ ] Plugin appears in Gooee menu
- [ ] No TypeLoadException

**If Failure:**
- [ ] Error type: _______________
- [ ] Error message: _______________
- [ ] Alternative approach needed: [ ] Yes [ ] No

---

## 🎯 Decision Tree

### Scenario A: GooeePlugin API Available
**If GooeePlugin base class is available and OnSetup() works:**
- ✅ Proceed with UI-002: Implement GooeePlugin Inheritance
- ✅ Uncomment GooeePlugin code
- ✅ Start creating UI panels

### Scenario B: GooeePlugin API Not Available
**If GooeePlugin base class is not available or causes errors:**
- ⚠️ Document findings
- ⚠️ Consider alternative UI approaches:
  - HookUI (if available)
  - Unity UI Toolkit
  - Custom BepInEx UI
- ⚠️ Update MASTER_TASK_LOG.md with blocker

### Scenario C: Gooee Deprecated/Unworkable
**If Gooee is completely broken or incompatible:**
- ❌ Mark Gooee as unworkable
- ❌ Research alternative UI frameworks
- ❌ Update UI_DEFINITION_OF_DONE.md
- ❌ Create new tasks for alternative UI approach

---

## 📝 Post-Testing Actions

### If API Verified (Scenario A)
1. Update `GOOEE_API_RESEARCH.md` with findings
2. Uncomment GooeePlugin code in `CitiesRegionalGooeePlugin.cs`
3. Comment out placeholder code
4. Rebuild and test
5. Update MASTER_TASK_LOG.md: UI-001 → ✅ Complete, UI-002 → 🚧 In Progress

### If API Not Available (Scenario B or C)
1. Document findings in `GOOEE_API_RESEARCH.md`
2. Update MASTER_TASK_LOG.md with blocker
3. Research alternative UI frameworks
4. Create new tasks for alternative approach
5. Update UI_DEFINITION_OF_DONE.md if needed

---

## 🔧 Quick Activation Guide

### To Activate GooeePlugin Code (if API verified):

1. **Edit `src/UI/CitiesRegionalGooeePlugin.cs`:**
   - Uncomment lines 15-55 (GooeePlugin code)
   - Comment out lines 58-81 (placeholder code)

2. **Rebuild:**
   ```bash
   cd D:\mods\CitiesRegional
   dotnet build
   ```

3. **Redeploy:**
   - Auto-deploy should copy to `BepInEx/plugins/CitiesRegional/`
   - Or manually copy `bin/Debug/netstandard2.1/CitiesRegional.dll`

4. **Test:**
   - Launch game
   - Check logs for "OnSetup() called"
   - Check Gooee menu for "CitiesRegional"

---

## 📚 Related Files

- `src/UI/CitiesRegionalGooeePlugin.cs` - GooeePlugin implementation
- `GOOEE_API_RESEARCH.md` - API research findings
- `MASTER_TASK_LOG.md` - Task tracking (UI-001)
- `UI_DEFINITION_OF_DONE.md` - UI completion criteria

---

**Status:** Ready for Testing  
**Next:** Execute testing steps and document results

