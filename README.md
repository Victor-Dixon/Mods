# 🎮 Cities: Skylines 2 Mod Development

Welcome to your CS2 modding workspace! This folder contains Product Requirements Documents (PRDs) for 5 potential mod projects.

---

## ✅ Official Repo Notes

This repository is intended to be the **official source repo** for the workspace and mod projects.

**We do not commit**:
- Game installs / deploy targets (`CS2/`, `data/`, any `BepInEx/`)
- Large archives (`*.rar`, `*.zip`, `*.7z`)
- Build outputs (`bin/`, `obj/`)

See `.gitignore` for the full list.

---

## 📑 PRD Index

| # | Mod Name | PRD File | Complexity | Time to MVP |
|---|----------|----------|------------|-------------|
| 1 | **City Analytics** | [PRD-01-SaveGameAnalyzer.md](PRD-01-SaveGameAnalyzer.md) | ⭐⭐ Medium-Low | 2-4 weeks |
| 2 | **Transit+** | [PRD-02-TransitPlus.md](PRD-02-TransitPlus.md) | ⭐⭐⭐ Medium | 4-6 weeks |
| 3 | **Zone Master** | [PRD-03-ZoneMaster.md](PRD-03-ZoneMaster.md) | ⭐⭐⭐ Medium-High | 5-7 weeks |
| 4 | **Traffic Contributions** | [PRD-04-TrafficContributions.md](PRD-04-TrafficContributions.md) | ⭐⭐⭐ Medium | 1-2 weeks to first PR |
| 5 | **Cities Together** | [PRD-05-CitiesMultiplayer.md](PRD-05-CitiesMultiplayer.md) | ⭐⭐⭐⭐⭐ Extreme | 6-12+ months |

---

## 🎯 Quick Comparison

### By Learning Value
| Best For | Recommended PRD |
|----------|-----------------|
| **First-time modder** | PRD-01 (City Analytics) |
| **Learning game systems** | PRD-02 (Transit+) |
| **Open-source contribution** | PRD-04 (Traffic Contributions) |
| **Ambitious long-term** | PRD-05 (Multiplayer) |

### By Impact
| Highest Downloads Potential | Why |
|----------------------------|-----|
| PRD-03 (Zone Master) | Mixed-use zoning is universally wanted |
| PRD-04 (Traffic) | Traffic is #1 player concern |
| PRD-05 (Multiplayer) | Game-changing if achieved |

### By Solo-Friendliness
| PRD | Solo Feasible? |
|-----|----------------|
| PRD-01 | ✅ Yes - Perfect for solo |
| PRD-02 | ✅ Yes - Manageable solo |
| PRD-03 | ⚠️ Maybe - Complex but doable |
| PRD-04 | ✅ Yes - You're joining a team |
| PRD-05 | ❌ No - Team required |

---

## 📚 Each PRD Contains

- **Problem Statement** - What user pain points does this solve?
- **Goals & Metrics** - How do we measure success?
- **User Personas** - Who are we building for?
- **Features** - Detailed feature breakdown with mockups
- **Technical Architecture** - System design, key classes, Harmony patches
- **UI/UX Design** - Visual design and interaction patterns
- **Development Roadmap** - Week-by-week plan
- **Risks & Mitigations** - What could go wrong?
- **Dependencies** - Required libraries and mods
- **Definition of Done** - When is it shippable?

---

## 🚀 Getting Started

### 1. Choose Your Path
Read through the PRDs and pick based on:
- Your experience level
- Time available
- What excites you most

### 2. Set Up Development Environment
```bash
# You'll need:
# - Visual Studio 2022 or JetBrains Rider
# - .NET 6.0 SDK
# - Cities: Skylines 2 (obviously)
# - BepInEx installed in game

# Clone the mod template:
git clone https://github.com/Captain-Of-Coit/cities-skylines-2-mod-template.git
cd cities-skylines-2-mod-template
```

### 3. Start Building!
Once you pick a PRD, I can help you:
- Set up the project structure
- Write the initial code
- Debug issues
- Test features

---

## 🔗 Resources

| Resource | Link |
|----------|------|
| CS2 Modding Wiki | https://wiki.ciim.dev/ |
| Cities2Modding Discord | https://discord.gg/cities2modding |
| BepInEx Docs | https://docs.bepinex.dev/ |
| Harmony Docs | https://harmony.pardeike.net/ |
| Thunderstore (Mods) | https://thunderstore.io/c/cities-skylines-ii/ |

---

## 📁 Project Structure

```
D:\mods\
├── README.md                    # This file
│
├── PRD-01-SaveGameAnalyzer.md   # PRDs
├── PRD-02-TransitPlus.md
├── PRD-03-ZoneMaster.md
├── PRD-04-TrafficContributions.md
├── PRD-05-CitiesMultiplayer.md
├── PRD-05b-RegionalPlay.md      # ⭐ Detailed Regional Mode spec
│
└── CitiesRegional/              # 🚀 WORKING PROTOTYPE
    ├── CitiesRegional.csproj    # Main mod project
    ├── README.md                # Build & usage instructions
    ├── src/
    │   ├── CitiesRegionalPlugin.cs
    │   ├── Models/
    │   │   ├── RegionalCityData.cs
    │   │   └── Region.cs
    │   ├── Services/
    │   │   ├── RegionalManager.cs
    │   │   ├── IRegionalSync.cs
    │   │   └── CloudRegionalSync.cs
    │   └── Systems/
    │       ├── CityDataCollector.cs
    │       └── RegionalEffectsApplicator.cs
    └── Server/
        ├── RegionalServer.cs    # ASP.NET Core API
        ├── RegionalServer.csproj
        └── Dockerfile
```

## 🚀 Ready-to-Build Prototype

The **CitiesRegional** folder contains a working prototype with:

✅ Complete data models for regional sync  
✅ Cloud sync service implementation  
✅ Regional manager orchestration  
✅ Placeholder game hooks (ready to fill in)  
✅ Simple REST API server  
✅ Docker support for server deployment  

To build:
```powershell
cd CitiesRegional
$env:CS2_INSTALL = "C:\Path\To\Cities Skylines II"
dotnet build
```

---

## ❓ What's Next?

Tell me which PRD you want to pursue and I'll help you:

1. **Create the project structure**
2. **Write the initial Plugin.cs entry point**
3. **Set up the first Harmony patches**
4. **Build the first feature**
5. **Test it in-game**

Let's build something awesome! 🏙️

