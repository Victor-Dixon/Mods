# User Guide - Cities Regional Mod

**Version:** 0.1.0  
**Last Updated:** 2025-12-27  
**Status:** Development Build

---

## 📖 Introduction

**Cities Regional** is a mod for Cities: Skylines 2 that connects your city to friends' cities in a shared regional economy. Trade resources, share workers, and compete on leaderboards - all without requiring real-time shared city building.

---

## 🎯 What Cities Regional Does

### Core Features

1. **Regional Trade System**
   - Automatically trade resources between cities
   - Export surplus goods (industrial, commercial, agricultural)
   - Import needed resources
   - Earn revenue from exports

2. **Worker Commuting**
   - Workers can commute to neighboring cities
   - Cities with job shortages get workers from cities with unemployment
   - Commuting requires regional connections (highways, rail)

3. **Regional Connections**
   - Build highways, rail, and air connections between cities
   - Connections affect trade capacity and commute times
   - Upgrade connections to increase capacity

4. **Shared Services** (Planned)
   - Share airports, universities, hospitals across the region
   - Cities benefit from regional services
   - Reduces need for every city to build everything

5. **Regional Leaderboards** (Planned)
   - Compete with friends on city metrics
   - Population, happiness, GDP, and more
   - See how your city ranks in the region

---

## 🚀 Getting Started

### Step 1: Install the Mod

See [INSTALLATION_GUIDE.md](INSTALLATION_GUIDE.md) for detailed installation instructions.

**Quick Steps:**
1. Install BepInEx 5.x in your CS2 game directory
2. Install Gooee UI framework (optional, for UI features)
3. Copy Cities Regional mod to `BepInEx/plugins/CitiesRegional/`
4. Launch CS2

### Step 2: Verify Installation

After launching CS2:
1. Check `BepInEx/LogOutput.log` for `[CitiesRegional]` entries
2. Look for "Cities Regional loaded successfully!" message
3. If Gooee is installed, check for Cities Regional in the Gooee menu

### Step 3: Create or Join a Region

**Creating a Region:**
1. Open Cities Regional UI (Gooee menu → Cities Regional)
2. Click "Create Region"
3. Enter region name
4. Share region code with friends

**Joining a Region:**
1. Get region code from friend
2. Open Cities Regional UI
3. Click "Join Region"
4. Enter region code
5. Wait for connection confirmation

---

## 🎮 How to Use

### Understanding the UI

**Region Panel:**
- Shows current region information
- Lists all cities in the region
- Displays regional connections
- Shows region statistics

**Trade Dashboard:**
- Displays active trades
- Shows trade statistics (total value, net balance)
- Lists exports and imports
- Shows trade flow between cities

### Managing Your City

**Automatic Features:**
- Data syncs every 2-5 minutes automatically
- Trade matching happens automatically
- Worker commuting happens automatically
- Effects apply automatically

**Manual Actions:**
- Create/join/leave regions
- Build regional connections
- View trade statistics
- Check regional leaderboards

### Regional Connections

**Building Connections:**
1. Open Region Panel
2. Select a connected city
3. Click "Build Connection"
4. Choose connection type (Highway, Rail, Air)
5. Connection appears on map

**Connection Types:**
- **Highway (4-lane):** High capacity, moderate speed
- **Highway (6-lane):** Very high capacity, moderate speed
- **Regional Rail:** High capacity, fast speed
- **Air Connection:** Low capacity, very fast speed

**Upgrading Connections:**
- Click on existing connection
- Select "Upgrade"
- Choose upgrade type
- Pay upgrade cost

---

## 📊 Understanding Trade

### How Trade Works

1. **Production & Consumption:**
   - Each city produces and consumes resources
   - Surplus production becomes available for export
   - Shortage creates import demand

2. **Automatic Matching:**
   - System matches exporters with importers
   - Considers connection capacity
   - Optimizes for best prices

3. **Trade Flow:**
   - Resources flow along connections
   - Trade value calculated automatically
   - Revenue/costs applied to city treasuries

### Resource Types

- **Industrial Goods:** Produced by industrial zones
- **Commercial Goods:** Produced by commercial zones
- **Agricultural Products:** Produced by agricultural zones
- **Workers:** Available unemployed workers

### Trade Statistics

**Total Trade Value:** Sum of all active trades  
**Active Trades Count:** Number of active trade flows  
**Net Trade Balance:** Exports minus imports (positive = net exporter)

---

## 👥 Worker Commuting

### How Commuting Works

1. **Job Matching:**
   - Cities with job shortages request workers
   - Cities with unemployment offer workers
   - System matches workers to jobs

2. **Commute Requirements:**
   - Workers only commute if connection exists
   - Commute time must be reasonable (< 45 minutes default)
   - Connection capacity limits commuter flow

3. **Benefits:**
   - **Worker City:** Gets residential tax revenue
   - **Job City:** Gets productivity boost
   - **Region:** Better resource utilization

### Managing Commuting

**Enable/Disable:**
- Toggle in mod settings
- Set max commute time
- Adjust connection priorities

**View Commuters:**
- Check Region Panel for commuter statistics
- See worker flow between cities
- Monitor commute times

---

## ⚙️ Configuration

### Mod Settings

**Location:** `BepInEx/config/com.citiesregional.mod.cfg`

**Key Settings:**

| Setting | Default | Description |
|---------|---------|-------------|
| `SyncIntervalSeconds` | 120 | How often to sync with region (2 minutes) |
| `EnableP2PMode` | true | Use peer-to-peer (no server needed) |
| `CloudServerUrl` | https://api.citiesregional.com | Server URL (if using cloud mode) |
| `AutoTradeEnabled` | true | Automatically trade resources |
| `MaxExportPercentage` | 50 | Max % of production to export |
| `EnableCommuters` | true | Allow worker commuting |
| `MaxCommuteMinutes` | 45 | Max commute time workers accept |

### Editing Settings

1. Close CS2
2. Open `BepInEx/config/com.citiesregional.mod.cfg`
3. Edit values as needed
4. Save file
5. Launch CS2

---

## 🐛 Troubleshooting

### Common Issues

**Mod Not Loading:**
- Check `BepInEx/LogOutput.log` for errors
- Verify `CitiesRegional.dll` is in correct location
- Ensure BepInEx 5.x is installed

**UI Not Appearing:**
- Install Gooee UI framework
- Check Gooee menu for Cities Regional entry
- Verify Gooee.dll exists

**Trade Not Working:**
- Check regional connections exist
- Verify cities are in same region
- Check trade settings (AutoTradeEnabled)
- Review logs for trade errors

**Sync Issues:**
- Check internet connection (if using cloud mode)
- Verify server is running (if using cloud mode)
- Check sync interval settings
- Review logs for sync errors

**Performance Issues:**
- Increase sync interval (SyncIntervalSeconds)
- Disable commuters if not needed
- Reduce max export percentage
- Check for other mod conflicts

### Getting Help

1. Check `BepInEx/LogOutput.log` for error messages
2. Review troubleshooting section above
3. Check GitHub Issues for known problems
4. Report new issues with:
   - Game version
   - Mod version
   - Error log excerpt
   - Steps to reproduce

---

## 📈 Tips & Best Practices

### Getting the Most from Regional Play

1. **Plan Your Connections:**
   - Build connections early
   - Upgrade high-traffic connections
   - Consider connection types (rail for workers, highway for goods)

2. **Specialize Your City:**
   - Focus on production or consumption
   - Trade surplus resources
   - Don't try to be self-sufficient

3. **Monitor Trade:**
   - Check trade dashboard regularly
   - Adjust production based on demand
   - Optimize for profitable trades

4. **Coordinate with Friends:**
   - Plan regional specialization
   - Coordinate connection building
   - Share regional services

5. **Manage Commuting:**
   - Build connections for commuter flow
   - Balance worker supply/demand
   - Monitor commute times

---

## 🔮 Future Features

**Planned for Future Releases:**
- Shared services (airports, universities)
- Regional events and notifications
- Advanced leaderboards
- Save/load region settings
- Connection visualization on map
- Regional challenges

---

## 📚 Additional Resources

- **[INSTALLATION_GUIDE.md](INSTALLATION_GUIDE.md)** - Installation instructions
- **[README.md](README.md)** - Project overview
- **[MASTER_TASK_LOG.md](MASTER_TASK_LOG.md)** - Development status
- **[GOOEE_API_TESTING_GUIDE.md](GOOEE_API_TESTING_GUIDE.md)** - UI testing guide

---

## 🆘 Support

**Need Help?**
- Check troubleshooting section above
- Review GitHub Issues
- Check mod documentation
- Report bugs with log files

---

**Status:** User guide created  
**Last Updated:** 2025-12-27  
**Note:** Some features are still in development. This guide will be updated as features are completed.

