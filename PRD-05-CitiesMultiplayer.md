# PRD: Cities Together - Multiplayer Co-op for Cities: Skylines 2

## 📋 Overview

| Field | Value |
|-------|-------|
| **Mod Name** | Cities Together |
| **Codename** | `CitiesTogether` |
| **Category** | Multiplayer / Networking |
| **Complexity** | ⭐⭐⭐⭐⭐ Extreme |
| **Time to MVP** | 6-12+ months |
| **Team Size** | 3-5 developers recommended |
| **Target Users** | Friends who want to build together |

---

## ⚠️ IMPORTANT DISCLAIMER

This is a **moonshot project** requiring:
- Deep game engine knowledge
- Networking expertise
- Significant time investment
- A dedicated team
- High tolerance for complexity

**Recommended only if:**
- You have prior multiplayer modding experience
- You can commit 6+ months
- You have teammates or can recruit them
- You're prepared for potential failure

---

## 🎯 Problem Statement

City builders are inherently collaborative, but CS2 is single-player only:

**User Desires:**
- "I want to build a city with my friend"
- "My partner and I want different districts"
- "We want to compete on who builds better"
- "Cooperative mayor simulation"

**Why This Is Hard:**
- CS2 runs complex simulation locally
- Simulation state is massive (millions of entities)
- Unity ECS/DOTS architecture is different from CS1
- Determinism is required for sync
- Performance is already a concern

---

## 🎯 Vision & Modes

### Mode 1: Shared City (Primary Goal)
Multiple players work on the same city simultaneously.

| Aspect | Approach |
|--------|----------|
| Authority | Host runs simulation |
| Building | All players can build |
| Conflicts | Lock-based (can't both edit same area) |
| Sync | Stream state updates |

### Mode 2: Competitive (Stretch Goal)
Players build separate cities, compare metrics.

| Aspect | Approach |
|--------|----------|
| Simulation | Each player runs own city |
| Comparison | Share stats periodically |
| Challenges | Same starting conditions, timed |

### Mode 3: Regional ⭐ RECOMMENDED STARTING POINT
Separate cities that trade and connect.

| Aspect | Approach |
|--------|----------|
| Simulation | Each player runs own |
| Connection | Trade goods, share highways |
| Impact | Your city affects neighbors |

> **📄 See [PRD-05b-RegionalPlay.md](PRD-05b-RegionalPlay.md) for the full Regional Mode specification.**
> 
> Regional Mode is **significantly more achievable** (2-3 months vs 6-12 months) because it only syncs ~50 aggregated values every few minutes instead of millions of entities in real-time.

#### Why Regional Mode First?

| Challenge | Shared City | Regional Mode |
|-----------|-------------|---------------|
| Entities to sync | 1,000,000+ | ~50 values |
| Sync frequency | Real-time (<100ms) | Every 2-5 minutes |
| Determinism required | Yes (critical) | No |
| Desync risk | Very high | Very low |
| Solo developer feasible | No | **Yes** |
| Time to MVP | 6-12 months | **2-3 months** |

#### Regional Mode Key Features
1. **Trade System** - Cities exchange resources automatically
2. **Worker Commuting** - Citizens can work in neighbor cities
3. **Shared Services** - Build airport/university that serves whole region
4. **Regional Connections** - Build highways/rail between cities
5. **Leaderboards** - Friendly competition

#### Recommended Strategy
```
Phase 1: Build Regional Mode (2-3 months)
    ↓
Phase 2: Learn from the experience
    ↓
Phase 3: If successful, consider Shared City mode
    ↓
Phase 4: Shared City would reuse Regional infrastructure
```

---

## 🎯 Success Metrics

| Metric | Target |
|--------|--------|
| Simultaneous players | 2-4 stable |
| Latency tolerance | <200ms playable |
| Desync rate | <1% per hour |
| City size supported | Medium (50k pop) |
| Community interest | 50k+ downloads if achieved |

---

## ✨ Features

### MVP (Phase 1) - 6 months

#### F1: Host/Join System
**Description:** Basic lobby and connection

```
┌─────────────────────────────────────────────┐
│ 🌐 CITIES TOGETHER                    [X]   │
├─────────────────────────────────────────────┤
│                                             │
│  ┌─────────────────────────────────────┐    │
│  │ 🏠 HOST GAME                        │    │
│  │                                     │    │
│  │ Your City: [New Metropolis]        │    │
│  │ Max Players: [4] ▼                  │    │
│  │ Password: [********]               │    │
│  │                                     │    │
│  │ [Start Hosting]                    │    │
│  └─────────────────────────────────────┘    │
│                                             │
│  ┌─────────────────────────────────────┐    │
│  │ 🔗 JOIN GAME                        │    │
│  │                                     │    │
│  │ Host IP: [192.168.1.100]           │    │
│  │ Port: [7777]                        │    │
│  │ Password: [********]               │    │
│  │                                     │    │
│  │ [Connect]                          │    │
│  └─────────────────────────────────────┘    │
│                                             │
└─────────────────────────────────────────────┘
```

#### F2: State Synchronization
**Description:** Keep all players seeing the same city

**Sync Strategy:**
```
Host (Authority)
    │
    ├── Runs full simulation
    ├── Validates all actions
    ├── Broadcasts state updates
    │
    ▼
Clients
    │
    ├── Receive state updates
    ├── Send action requests
    ├── Predict local changes
    └── Rollback on mismatch
```

**What Gets Synced:**
| Data Type | Frequency | Method |
|-----------|-----------|--------|
| Building placement | Immediate | Event |
| Road changes | Immediate | Event |
| Simulation state | 1/second | Snapshot delta |
| Camera position | 10/second | For "follow player" |
| Chat messages | Immediate | Event |

#### F3: Action Validation
**Description:** Ensure players can't cheat or break things

**Validation Flow:**
```
Player Action → Client Mod → Network → Host Mod → Validate → Execute → Broadcast
                   │                                            │
                   └── Optimistic local preview ←───────────────┘
                       (rollback if rejected)
```

**Validated Actions:**
- Building placement (can afford? valid location?)
- Bulldozing (player has permission?)
- Budget changes (player is mayor?)
- Zone painting (within player's district?)

#### F4: Permission System
**Description:** Control who can do what

| Role | Permissions |
|------|-------------|
| Mayor (Host) | Full control, budget, policies |
| Planner | Build, zone, roads |
| Viewer | Camera only, chat |
| District Manager | Build only in assigned district |

```
┌─────────────────────────────────────────────┐
│ 👥 PLAYERS                            [X]   │
├─────────────────────────────────────────────┤
│ Player           Role           Status      │
│ ────────────────────────────────────────    │
│ 👑 HostPlayer    Mayor          Connected   │
│ 👷 BuilderBob    Planner        Connected   │
│ 👀 Spectator1    Viewer         Connected   │
│                                             │
│ [Manage Roles] [Kick] [District Assignment] │
└─────────────────────────────────────────────┘
```

#### F5: Chat System
**Description:** Player communication

- Text chat
- Ping locations on map
- Quick voice lines (optional)
- Player markers on map

### Phase 2 - 3 months

#### F6: District Ownership
**Description:** Assign city districts to specific players

- Each player "owns" certain districts
- Can only build in your districts
- Revenue sharing configuration
- Competition mode (separate scores)

#### F7: Conflict Resolution
**Description:** Handle simultaneous edits

- Visual lock indicators
- Automatic conflict detection
- Merge or reject strategies
- Undo for conflicts

#### F8: Reconnection Handling
**Description:** Don't lose players to disconnects

- Save player state
- Queue actions during disconnect
- Resync on reconnect
- Graceful degradation

### Phase 3 - 3 months

#### F9: Dedicated Server Option
**Description:** Headless server for persistent cities

- Run without game client
- Players can join/leave
- City persists
- Better performance

#### F10: Save/Load Multiplayer Games
**Description:** Continue sessions later

- Host saves full state
- Players save their settings
- Resume exactly where left off
- Migrate host capability

#### F11: Spectator Mode
**Description:** Watch without participating

- Zero impact on simulation
- Free camera
- Follow players
- Stream-friendly

---

## 🏗️ Technical Architecture

### High-Level Architecture

```
┌───────────────────────────────────────────────────────────────┐
│                     Cities Together                            │
├───────────────────────────────────────────────────────────────┤
│                                                               │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │                    Network Layer                        │  │
│  │  ┌───────────┐  ┌───────────┐  ┌───────────┐          │  │
│  │  │ Transport │  │ Protocol  │  │ Reliable  │          │  │
│  │  │ (LiteNet) │  │ Messages  │  │ Ordered   │          │  │
│  │  └───────────┘  └───────────┘  └───────────┘          │  │
│  └─────────────────────────────────────────────────────────┘  │
│                           │                                    │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │                  Synchronization Layer                  │  │
│  │  ┌───────────┐  ┌───────────┐  ┌───────────┐          │  │
│  │  │ State     │  │ Action    │  │ Delta     │          │  │
│  │  │ Manager   │  │ Queue     │  │ Compressor│          │  │
│  │  └───────────┘  └───────────┘  └───────────┘          │  │
│  └─────────────────────────────────────────────────────────┘  │
│                           │                                    │
│  ┌─────────────────────────────────────────────────────────┐  │
│  │                    Game Integration                     │  │
│  │  ┌───────────┐  ┌───────────┐  ┌───────────┐          │  │
│  │  │ Harmony   │  │ Action    │  │ Simulation│          │  │
│  │  │ Patches   │  │ Handlers  │  │ Hooks     │          │  │
│  │  └───────────┘  └───────────┘  └───────────┘          │  │
│  └─────────────────────────────────────────────────────────┘  │
│                                                               │
└───────────────────────────────────────────────────────────────┘
```

### Networking Stack

**Transport Layer:**
- **LiteNetLib** - Reliable UDP, NAT punchthrough
- **Alternative:** Mirror, Steamworks (if allowed)

**Protocol:**
```csharp
// Message types
public enum MessageType : byte
{
    // Connection
    Handshake,
    Disconnect,
    Heartbeat,
    
    // State sync
    FullState,
    DeltaState,
    
    // Actions
    ActionRequest,
    ActionResult,
    ActionRejected,
    
    // Misc
    Chat,
    Ping,
    PlayerUpdate
}

// Example message
[Serializable]
public struct BuildingPlaceMessage
{
    public MessageType Type => MessageType.ActionRequest;
    public uint RequestId;
    public int PlayerId;
    public int BuildingPrefabId;
    public float3 Position;
    public quaternion Rotation;
    public long Timestamp;
}
```

### State Synchronization Strategy

**Full State Sync:**
- On player join
- On major desync detection
- Every N minutes as backup

**Delta Sync:**
- Building added/removed
- Road segment changes
- Zone changes
- Budget/policy changes

**Not Synced (Local Only):**
- Camera position
- UI state
- Graphics settings
- Individual citizen paths (too granular)

### Entity Handling (ECS Challenge)

CS2 uses Unity ECS with millions of entities. Strategy:

```csharp
// Track entity ownership
public struct NetworkedEntity : IComponentData
{
    public int OwningPlayerId;
    public uint NetworkId;
    public EntitySyncType SyncType;
}

public enum EntitySyncType
{
    HostAuthoritative,  // Host controls completely
    PlayerOwned,        // Player who placed controls
    ReadOnly,          // Clients can't modify
    Predicted          // Client predicts, host validates
}
```

### Determinism Strategy

**Problem:** Game simulation must produce identical results on all machines.

**Solutions:**
1. **Lockstep:** All machines run same inputs → same results
   - Pro: Perfect sync
   - Con: Slowest player limits everyone

2. **Host Authority (CHOSEN):** Host runs simulation, broadcasts results
   - Pro: One source of truth
   - Con: Latency for clients

3. **Prediction + Rollback:** Clients predict, rollback on mismatch
   - Pro: Responsive
   - Con: Complex, visual artifacts

### Key Harmony Patches

```csharp
// Intercept building placement
[HarmonyPatch(typeof(BuildingPlacementSystem), "PlaceBuilding")]
class BuildingPlacementPatch
{
    static bool Prefix(BuildingPrefab prefab, float3 position)
    {
        if (!NetworkManager.IsHost)
        {
            // Client: Send request to host
            NetworkManager.SendBuildRequest(prefab, position);
            return false; // Don't execute locally
        }
        return true; // Host: Execute normally
    }
    
    static void Postfix(Entity building)
    {
        if (NetworkManager.IsHost)
        {
            // Host: Broadcast to clients
            NetworkManager.BroadcastBuildingPlaced(building);
        }
    }
}

// Intercept bulldozing
[HarmonyPatch(typeof(BulldozeSystem), "Bulldoze")]
class BulldozePatch
{
    // Similar pattern...
}

// Intercept money spending
[HarmonyPatch(typeof(BudgetSystem), "SpendMoney")]
class BudgetPatch
{
    // Validate with host before spending
}
```

---

## 🛡️ Security Considerations

### Threat Model
| Threat | Mitigation |
|--------|------------|
| Cheating (infinite money) | Host validates all budget changes |
| Griefing (mass bulldoze) | Permission system, undo capability |
| Denial of Service | Rate limiting, kick capability |
| Save corruption | Validation, backups |
| Impersonation | Authentication tokens |

### Anti-Cheat (Lightweight)
- Host validates all actions
- Sanity checks on values
- Action rate limiting
- No client-side money calculations

---

## 📅 Development Roadmap

### Month 1-2: Research & Foundation
- [ ] Deep dive into CS2's simulation systems
- [ ] Prototype state extraction
- [ ] Set up networking library
- [ ] Basic host/join connection

### Month 3-4: Core Sync
- [ ] Implement building sync
- [ ] Implement road sync
- [ ] Basic delta compression
- [ ] Handle disconnections

### Month 5-6: Playable Alpha
- [ ] Zone sync
- [ ] Budget sync
- [ ] Permission system
- [ ] Chat system
- [ ] Alpha testing with testers

### Month 7-8: Stability
- [ ] Desync detection and recovery
- [ ] Performance optimization
- [ ] Edge case handling
- [ ] Beta release

### Month 9-10: Polish
- [ ] District ownership
- [ ] Spectator mode
- [ ] Save/load multiplayer
- [ ] UI polish

### Month 11-12: Release
- [ ] Extensive testing
- [ ] Documentation
- [ ] Community beta
- [ ] Public release

---

## 👥 Team Requirements

### Ideal Team Composition

| Role | Skills Needed | Time Commitment |
|------|---------------|-----------------|
| **Lead Developer** | Architecture, ECS, networking | 20+ hrs/week |
| **Network Developer** | Netcode, sync, protocols | 15+ hrs/week |
| **Game Integration** | Harmony, CS2 systems | 15+ hrs/week |
| **UI Developer** | Unity UI, UX design | 10+ hrs/week |
| **QA/Testing** | Systematic testing | 10+ hrs/week |

### Solo Developer?
If you must go solo:
- Start with Mode 2 (Competitive) - much simpler
- Or contribute to CSM's CS2 port if they start one
- Or focus on a smaller feature and build expertise

---

## ⚠️ Risks & Challenges

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| CS2 simulation too complex | High | Critical | Start with limited sync scope |
| Performance issues | High | High | Heavy optimization, limit city size |
| Game updates break mod | High | High | Abstract game APIs, quick updates |
| Desync issues | High | High | Robust recovery, checksums |
| Player base too small | Medium | Medium | Quality over quantity |
| Legal issues with networking | Low | Critical | Review TOS, no bypass DRM |

---

## 📚 Reference Materials

### CSM (CS1 Multiplayer)
- GitHub: https://github.com/CitiesSkylinesMultiplayer/CSM
- Study their architecture
- Learn from their challenges
- Potential collaboration?

### Networking Resources
- LiteNetLib documentation
- Gabriel Gambetta's game networking articles
- GDC talks on multiplayer

### ECS/DOTS
- Unity ECS documentation
- CS2-specific ECS patterns

---

## ✅ Definition of Done

### Minimum Viable Multiplayer
- [ ] 2 players can connect
- [ ] See each other's cursors/cameras
- [ ] Buildings sync correctly
- [ ] Roads sync correctly
- [ ] Zones sync correctly
- [ ] Basic budget sync
- [ ] Chat works
- [ ] Can play for 30 min without desync
- [ ] Reconnection works

### Quality Bar for Release
- [ ] 4 players stable
- [ ] Medium cities work (50k pop)
- [ ] <5% desync rate
- [ ] Graceful degradation on lag
- [ ] Clear error messages
- [ ] Documentation
- [ ] Tutorial

---

## 🎯 Fallback Options

If full multiplayer proves too difficult:

### Fallback A: Async Multiplayer
- Take turns, not simultaneous
- Pass save file back and forth
- Much simpler technically

### Fallback B: Shared Challenges
- Same starting city
- Timed building challenge
- Compare screenshots/stats
- Leaderboard

### Fallback C: Spectator Only
- Stream game state to viewers
- No interaction, just watching
- Good for content creators

---

## 💬 Final Thoughts

This is the most ambitious mod on this list. It's a legitimate engineering challenge that has broken many modding efforts.

**Proceed if:**
- You're in it for the journey, not just the destination
- You have or can build a team
- You can accept partial success as a win
- You want to push your limits as a developer

**Don't proceed if:**
- You need a quick win
- You're working alone without networking experience
- You need this to succeed for external reasons

If you do attempt this, the CS2 modding community will remember you either way. 🚀

