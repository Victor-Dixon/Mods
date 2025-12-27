# 🌐 Cities Regional - Connected Cities Network

A Cities: Skylines 2 mod that connects your city to friends' cities in a shared regional economy.

## 🎯 Features

- **Trade Resources** - Export/import goods between cities
- **Worker Commuting** - Citizens can work in neighboring cities
- **Shared Services** - Share airports, universities, hospitals
- **Regional Connections** - Build highways and rail between cities
- **Leaderboards** - Compete with friends on city metrics

## 📁 Project Structure

```
CitiesRegional/
├── CitiesRegional.csproj          # Main mod project
├── src/
│   ├── CitiesRegionalPlugin.cs    # BepInEx plugin entry point
│   ├── Models/
│   │   ├── RegionalCityData.cs    # City data that gets synced
│   │   └── Region.cs              # Region container with connections
│   ├── Services/
│   │   ├── RegionalManager.cs     # Central orchestrator
│   │   ├── IRegionalSync.cs       # Sync interface
│   │   └── CloudRegionalSync.cs   # Cloud API implementation
│   └── Systems/
│       ├── CityDataCollector.cs   # Collects game data
│       └── RegionalEffectsApplicator.cs  # Applies regional effects
├── Server/
│   ├── RegionalServer.cs          # ASP.NET Core server
│   ├── RegionalServer.csproj
│   └── Dockerfile
└── README.md
```

## 🚀 Getting Started

### Quick Install

**For Users:**
1. Download latest release
2. Extract to `BepInEx/plugins/CitiesRegional/`
3. Launch CS2

**For Developers:**
1. Clone repository
2. Set `CS2_INSTALL` environment variable
3. Run `dotnet build`
4. Mod auto-deploys to BepInEx

**📖 Full Installation Guide:** See [INSTALLATION_GUIDE.md](INSTALLATION_GUIDE.md)

### Prerequisites

- **.NET 6.0 SDK or later** (for building from source)
- **Cities: Skylines 2** installed and updated
- **BepInEx 5.x** installed in the game directory
- **Gooee UI Framework** (optional, for UI features - install via Thunderstore)

### Building the Mod

1. **Set the CS2 install path:**
   ```powershell
   # Windows PowerShell
   $env:CS2_INSTALL = "C:\Program Files (x86)\Steam\steamapps\common\Cities Skylines II"
   $env:BEPINEX_PLUGINS = "$env:CS2_INSTALL\BepInEx\plugins"
   ```

2. **Build the project:**
   ```bash
   dotnet build --configuration Debug
   ```

3. **Auto-deployment:**
   - The mod will automatically copy to `BepInEx/plugins/CitiesRegional/` if `BEPINEX_PLUGINS` is set
   - Or manually copy `bin/Debug/netstandard2.1/CitiesRegional.dll`

### Running the Server (Optional)

For cloud-hosted regions, run the server:

```bash
cd Server
dotnet run
```

The server will start on `http://localhost:5000`.

Or use Docker:
```bash
cd Server
docker build -t cities-regional-server .
docker run -p 5000:5000 cities-regional-server
```

## 🔧 Configuration

The mod creates a config file at:
`BepInEx/config/com.citiesregional.mod.cfg`

| Setting | Default | Description |
|---------|---------|-------------|
| SyncIntervalSeconds | 120 | How often to sync with region |
| EnableP2PMode | true | Use peer-to-peer (no server needed) |
| CloudServerUrl | https://api.citiesregional.com | Server URL |
| AutoTradeEnabled | true | Automatically trade resources |
| MaxExportPercentage | 50 | Max % of production to export |
| EnableCommuters | true | Allow worker commuting |
| MaxCommuteMinutes | 45 | Max commute time workers accept |

## 📖 How It Works

### Data Flow

```
┌─────────────┐     Sync every 2 min     ┌─────────────┐
│   Your PC   │◄────────────────────────►│   Server    │
│  (CS2 Mod)  │                          │  (or P2P)   │
└──────┬──────┘                          └──────┬──────┘
       │                                        │
       │ Collect data                    Push/Pull data
       │ Apply effects                          │
       ▼                                        ▼
┌─────────────┐                          ┌─────────────┐
│    Game     │                          │  Friend's   │
│  Systems    │                          │   Cities    │
└─────────────┘                          └─────────────┘
```

### What Gets Synced

Only **aggregated city data** syncs - NOT individual entities:

| Data | Example |
|------|---------|
| Population | 52,340 |
| Treasury | $1.5M |
| Weekly Income | $250K |
| Happiness | 78% |
| Exports | 500 industrial goods/week |
| Imports needed | 300 commercial goods/week |

This keeps sync lightweight (~1KB per city).

### Trade System

1. Each city reports production/consumption
2. System matches exporters with importers
3. Resources flow automatically
4. Revenue/costs applied to treasuries

### Commuting

1. City A has 500 open jobs
2. City B has 600 unemployed workers
3. If connected with reasonable commute time
4. Workers from B work in A
5. B gets residential tax, A gets productivity

## 🛠️ Development

### Key Files to Modify

| File | Purpose |
|------|---------|
| `CityDataCollector.cs` | Hook into game systems to read data |
| `RegionalEffectsApplicator.cs` | Apply trade/commuter effects to game |
| `CloudRegionalSync.cs` | Server communication |

### Adding Game Hooks

The `CityDataCollector` has placeholder methods. Replace with real game access:

```csharp
// Current (placeholder):
data.Population = GetGameValue("Population", 50000);

// Real implementation:
var popSystem = World.GetExistingSystemManaged<PopulationSystem>();
data.Population = popSystem.TotalPopulation;
```

### Testing

1. Build mod in Debug mode
2. Start CS2 with BepInEx
3. Create a region in-game
4. Share region code with friend
5. Friend joins with same mod installed

## 📊 Current Status

**Project Completion:** 83%

**Completed:**
- ✅ Core sync infrastructure
- ✅ Data models and trade system
- ✅ Testing framework (61/61 tests passing)
- ✅ Gooee UI framework preparation

**In Progress:**
- 🚧 UI-001: GooeePlugin API verification (blocked on game launch)

**Pending:**
- ⏳ UI panels implementation
- ⏳ End-to-end integration testing
- ⏳ Performance optimization

**See [MASTER_TASK_LOG.md](MASTER_TASK_LOG.md) for detailed status.**

## 📋 TODO

- [x] Core sync infrastructure
- [x] Trade matching algorithm
- [x] Testing framework
- [ ] UI panels (Gooee or alternative)
- [ ] P2P networking mode (LiteNetLib)
- [ ] Connection visualization on map
- [ ] Shared services implementation
- [ ] Leaderboard UI
- [ ] Save/load region settings

## 📚 Documentation

- **[INSTALLATION_GUIDE.md](INSTALLATION_GUIDE.md)** - Detailed installation instructions
- **[GOOEE_API_TESTING_GUIDE.md](GOOEE_API_TESTING_GUIDE.md)** - UI testing guide
- **[MASTER_TASK_LOG.md](MASTER_TASK_LOG.md)** - Development status and tasks
- **[MISSION_BRIEFING.md](MISSION_BRIEFING.md)** - Project architecture and design
- **[UI_002_ACTIVATION_CHECKLIST.md](UI_002_ACTIVATION_CHECKLIST.md)** - UI activation steps

## 📚 References

- [PRD-05b-RegionalPlay.md](../PRD-05b-RegionalPlay.md) - Full specification
- [CS2 Modding Wiki](https://wiki.ciim.dev/)
- [BepInEx Docs](https://docs.bepinex.dev/)
- [Harmony Docs](https://harmony.pardeike.net/)

## 📄 License

MIT License - See LICENSE file

## 🤝 Contributing

Contributions welcome! Please read the PRD first to understand the architecture.

