# UI-003: Panel Registration Research

**Date:** 2025-12-28  
**Status:** 🔍 Researching Panel Registration API  
**Task:** UI-003 - Create Basic Visible Panel

---

## 🎯 Goal

Create a basic visible panel that:
- Opens from Gooee menu
- Displays placeholder content
- Can be opened/closed
- Works in-game

---

## 🔍 Research Findings

### Gooee.Plugin Methods Discovered

**No panel registration methods found in Gooee.Plugin:**
- Checked all 61 public methods
- No methods containing "Panel", "Register", "Create", "Show", or "Display"
- No static panel-related methods found

**Possible Explanations:**
1. Panel registration may be handled by a separate Gooee API class
2. Panels may be registered via attributes or configuration
3. Gooee may use a different pattern (React component registration)
4. Panel API may be in a different namespace (e.g., `Gooee.UI`, `Gooee.Components`)

---

## 📋 Next Steps

### Option 1: Search for Panel-Related Types
- Enumerate all Gooee types
- Look for types with "Panel", "View", "Component", "UI" in name
- Check for static registration methods

### Option 2: Research Gooee Examples
- Check Thunderstore for mods using Gooee
- Look for source code examples
- Check Gooee GitHub repository (if available)

### Option 3: Implement Basic Test Panel
- Create a simple panel class inheriting from a Gooee base type
- Try different registration patterns
- Test in-game to see what works

### Option 4: Check Gooee Documentation
- Review Thunderstore page for Gooee
- Check for API documentation links
- Look for example mods

---

## 🔧 Implementation Approaches

### Approach A: Direct Panel Class
```csharp
// Try inheriting from a Gooee panel base class
public class BasicPanel : Gooee.Panel  // or Gooee.UI.Panel
{
    // Implement panel content
}
```

### Approach B: Component Registration
```csharp
// Try registering as a component
Gooee.RegisterComponent<BasicPanel>();
```

### Approach C: Menu Item Registration
```csharp
// Try adding menu item that opens panel
Gooee.AddMenuItem("CitiesRegional", () => ShowPanel());
```

---

## 📝 Current Status

- ✅ Gooee.Plugin type verified
- ✅ Panel structures initialized (RegionPanel, TradeDashboardPanel)
- ✅ React components ready (RegionPanelComponent, TradeDashboardComponent)
- ⚠️ Panel registration API not yet discovered
- 🔍 Researching alternative approaches

---

## 🎯 Acceptance Criteria (UI-003)

- [ ] Panel created and registered with Gooee
- [ ] Panel opens from Gooee menu
- [ ] Panel displays content (even placeholder text)
- [ ] Panel can be closed
- [ ] No crashes or errors
- [ ] Works in-game

---

**Next:** Continue research or implement test panel with trial-and-error approach

