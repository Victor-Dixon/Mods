# 🗺️ CitiesRegional Development Roadmap Status

**Last Updated:** 2025-12-25  
**Current Phase:** Week 3-4 (Core Sync)  
**Overall Progress:** ~40% of MVP

---

## ✅ Completed (Week 1-2: Foundation)

- [x] **Project Setup**
  - ✅ BepInEx 5 installed and configured
  - ✅ Build environment on D: drive
  - ✅ Auto-deploy to BepInEx plugins folder
  - ✅ All project references configured

- [x] **Data Models**
  - ✅ `RegionalCityData` - Complete with all fields
  - ✅ `Region` - Complete with city management
  - ✅ `ResourceData`, `TradeFlow`, `SharedService` - All defined
  - ✅ Connection models defined

- [x] **Basic Sync Infrastructure**
  - ✅ `IRegionalSync` interface defined
  - ✅ `CloudRegionalSync` - REST API client implemented
  - ✅ `RegionalManager` - Orchestration layer complete
  - ✅ RegionalServer - ASP.NET Core API server built

- [x] **Proof of Concept**
  - ✅ TestApp created and builds
  - ✅ Server builds and ready to run
  - ✅ Basic sync flow prototyped (can send/receive data)

---

## 🚧 In Progress (Week 3-4: Core Sync)

### 🔴 Critical - Must Do Next

1. **Connect to Game Systems** ⚠️ **BLOCKER**
   - [ ] Implement real data collection in `CityDataCollector`
   - [ ] Hook into CS2's ECS systems (Population, Economy, Resources)
   - [ ] Replace all `GetGameValue()` stubs with actual game queries
   - **Status:** All methods are placeholders returning dummy data
   - **Impact:** Mod can't collect real city data yet

2. **Apply Effects to Game** ⚠️ **BLOCKER**
   - [ ] Implement real effect application in `RegionalEffectsApplicator`
   - [ ] Hook into game systems to modify treasury, resources, workers
   - [ ] Replace all `Apply*()` stubs with actual game modifications
   - **Status:** All methods just log, don't modify game state
   - **Impact:** Regional effects don't actually affect the city

3. **Basic UI (Region Dashboard)** 🎨
   - [ ] Create Gooee/React UI panel
   - [ ] Show current region status
   - [ ] Display connected cities
   - [ ] Show sync status
   - **Status:** No UI exists yet
   - **Impact:** Players can't see or interact with the mod

4. **Connection Management** 🔌
   - [ ] UI to create/join regions
   - [ ] Region code system
   - [ ] Connection status indicators
   - **Status:** Only programmatic access
   - **Impact:** Players can't easily join regions

---

## 📋 Pending (Week 5-8)

### Week 5-6: Trade System
- [ ] Resource tracking (real game data)
- [ ] Trade matching algorithm
- [ ] Trade effects on economy (real implementation)
- [ ] Trade UI panel

### Week 7-8: Polish & Testing
- [ ] Commuter system (real implementation)
- [ ] Connection upgrades UI
- [ ] Error handling & retry logic
- [ ] Multi-player testing
- [ ] Performance optimization

---

## 🎯 Immediate Next Steps (Priority Order)

### Step 1: Research Game Systems (1-2 days)
**Goal:** Understand how to access CS2's game data

**Tasks:**
- [ ] Find existing CS2 mods that read population/economy data
- [ ] Study CS2's ECS architecture
- [ ] Identify key systems: `PopulationSystem`, `EconomySystem`, `ResourceSystem`
- [ ] Create a test mod that just reads and logs one value (e.g., population)

**Resources:**
- CS2 Modding Wiki: https://wiki.ciim.dev/
- Cities2Modding Discord: https://discord.gg/cities2modding
- Existing mods on Thunderstore/GitHub

### Step 2: Implement Real Data Collection (2-3 days)
**Goal:** Replace `CityDataCollector` stubs with real game hooks

**Tasks:**
- [ ] Implement `CollectPopulationData()` - Read from PopulationSystem
- [ ] Implement `CollectEconomyData()` - Read from EconomySystem
- [ ] Implement `CollectResourceData()` - Read from ResourceSystem
- [ ] Implement `CollectMetricsData()` - Read from various metric systems
- [ ] Test in-game to verify data accuracy

**Files to Modify:**
- `src/Services/CityDataCollector.cs`

### Step 3: Implement Real Effect Application (2-3 days)
**Goal:** Replace `RegionalEffectsApplicator` stubs with real game modifications

**Tasks:**
- [ ] Implement `ApplyTreasuryChange()` - Modify city money
- [ ] Implement `ApplyExportEffect()` / `ApplyImportEffect()` - Modify resources
- [ ] Implement `ApplyWorkerChange()` - Adjust worker/job availability
- [ ] Implement `ApplyModifier()` - Apply stat bonuses
- [ ] Test in-game to verify effects work

**Files to Modify:**
- `src/Services/RegionalEffectsApplicator.cs`

### Step 4: Create Basic UI (3-4 days)
**Goal:** Players can see and interact with the mod

**Tasks:**
- [ ] Set up Gooee/React UI framework
- [ ] Create Region Dashboard panel
- [ ] Show connected cities list
- [ ] Display sync status
- [ ] Add "Create Region" / "Join Region" buttons

**Files to Create:**
- `src/UI/RegionalDashboard.cs` (or React component)

### Step 5: Test End-to-End (1-2 days)
**Goal:** Verify the full flow works

**Tasks:**
- [ ] Start RegionalServer
- [ ] Launch CS2 with mod
- [ ] Create a region
- [ ] Verify data syncs
- [ ] Verify effects apply
- [ ] Test with 2+ cities

---

## 🔍 Current Blockers

| Blocker | Impact | Workaround | Solution |
|---------|--------|------------|----------|
| **No game system hooks** | Can't collect real data | Using dummy data | Research CS2 modding patterns |
| **No effect application** | Regional effects don't work | Effects only logged | Implement game system modifications |
| **No UI** | Players can't use mod | Manual config files | Build Gooee/React UI |
| **Unknown game APIs** | Don't know how to access data | Trial and error | Study existing mods |

---

## 📊 Progress Metrics

| Category | Completion | Notes |
|----------|------------|-------|
| **Infrastructure** | 90% | Server, sync, models all done |
| **Data Collection** | 10% | All stubs, need real implementations |
| **Effect Application** | 10% | All stubs, need real implementations |
| **UI** | 0% | Not started |
| **Testing** | 20% | TestApp works, but not game-tested |

**Overall MVP Progress: ~40%**

---

## 🎓 Learning Resources Needed

1. **CS2 ECS/DOTS Systems:**
   - How to query entities
   - How to access singletons
   - How to modify game state

2. **BepInEx + Harmony:**
   - How to patch game methods
   - How to access game systems
   - Best practices for modding

3. **Gooee/React UI:**
   - How to create panels
   - How to integrate with game UI
   - Styling and layout

---

## 💡 Recommendations

### Option A: Research First (Recommended)
**Spend 1-2 days studying existing CS2 mods** to understand:
- How they access game data
- What Harmony patches they use
- How they modify game state

**Then** implement the real hooks with confidence.

### Option B: Trial and Error
Start implementing hooks now, using:
- Game.dll reflection
- Harmony patches to intercept methods
- Trial and error to find the right systems

**Risk:** May waste time on wrong approaches.

### Option C: Ask Community
Post on Cities2Modding Discord:
- "How do I read population from CS2?"
- "How do I modify city treasury?"
- "What systems handle resources?"

**Benefit:** Get expert guidance quickly.

---

## 🚀 Quick Win: Test Mod

Before tackling the full implementation, create a **simple test mod** that:
1. Reads population (one value)
2. Logs it to BepInEx console
3. Verifies the approach works

This validates your understanding before building the full system.

---

**Next Action:** Choose Option A, B, or C above, then proceed with Step 1.

