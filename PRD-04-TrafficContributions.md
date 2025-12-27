# PRD: Traffic Mod Contributions - Joining the Existing Effort

## 📋 Overview

| Field | Value |
|-------|-------|
| **Project Type** | Contribution to Existing Open-Source Mod |
| **Target Mod** | Traffic (by Cities2Modding) |
| **Category** | Gameplay / Traffic Management |
| **Complexity** | ⭐⭐⭐ Medium (varies by feature) |
| **Time to First PR** | 1-2 weeks |
| **Target Users** | City builders who want traffic control |

---

## 🎯 Why Contribute vs. Build New?

### Advantages of Contributing
1. **Established codebase** - Learn from working code
2. **Existing users** - Immediate impact (thousands of downloads)
3. **Community support** - Get help from experienced modders
4. **No duplication** - Don't reinvent the wheel
5. **Resume builder** - Public contribution history

### The Traffic Mod Landscape
The "Traffic" mod is the closest thing CS2 has to TMPE from CS1, but it's incomplete:

| Feature | TMPE (CS1) | Traffic (CS2) | Gap |
|---------|------------|---------------|-----|
| Lane Connectors | ✅ Full | ⚠️ Basic | Medium |
| Speed Limits | ✅ Full | ⚠️ Limited | Large |
| Timed Traffic Lights | ✅ Full | ❌ Missing | **Major** |
| Priority Signs | ✅ Full | ⚠️ Basic | Medium |
| Vehicle Restrictions | ✅ Full | ❌ Missing | **Major** |
| Parking AI | ✅ Full | ❌ Missing | Large |
| Junction Rules | ✅ Full | ⚠️ Basic | Medium |

---

## 🎯 Contribution Strategy

### Phase 1: Learn & Small Fixes (Week 1-2)

#### Goal: Get Familiar, Make First Contribution

**Step 1: Setup**
- Fork the Traffic mod repository
- Build and test locally
- Read all documentation
- Join Cities2Modding Discord

**Step 2: Bug Fixes**
Look for issues labeled "good first issue" or:
- UI polish issues
- Tooltip improvements
- Edge case fixes
- Performance optimizations

**Step 3: Documentation**
- Improve README
- Add code comments
- Write user guides
- Create wiki pages

### Phase 2: Small Features (Week 3-4)

#### Potential Small Features to Add

##### Feature A: Speed Limit Presets
**Description:** Quick-apply common speed limit configurations

```
┌─────────────────────────────────────────────┐
│ Speed Limit Presets                         │
├─────────────────────────────────────────────┤
│ [Residential] 30 km/h for all selected      │
│ [Urban] 50 km/h for all selected            │
│ [Highway] 100 km/h for all selected         │
│ [School Zone] 20 km/h for all selected      │
│                                             │
│ [Custom...] Define new preset               │
└─────────────────────────────────────────────┘
```

**Effort:** 2-3 days
**Impact:** Quality of life improvement

##### Feature B: Junction Quick Setup
**Description:** One-click junction configurations

| Config | Description |
|--------|-------------|
| All-Way Stop | Every approach stops |
| Priority Road | Main road has priority |
| Roundabout Mode | Yield on entry |
| Traffic Light | Enable signals |

**Effort:** 3-4 days
**Impact:** Faster workflow

##### Feature C: Copy/Paste Settings
**Description:** Copy lane connections from one intersection to another

**How It Works:**
1. Select source intersection
2. Press Ctrl+C (or button)
3. Select target intersection(s)
4. Press Ctrl+V (or button)
5. Settings applied

**Effort:** 4-5 days
**Impact:** Major time saver

### Phase 3: Major Features (Week 5+)

#### Feature D: Timed Traffic Lights ⭐ HIGH VALUE
**Description:** The most-requested missing feature

**UI Mockup:**
```
┌─────────────────────────────────────────────────────────┐
│ 🚦 Timed Traffic Light - Main St & 5th Ave       [X]   │
├─────────────────────────────────────────────────────────┤
│ Phase 1 (25 sec)     Phase 2 (20 sec)    Phase 3 (10s) │
│ ┌─────────────┐      ┌─────────────┐     ┌───────────┐ │
│ │     🟢      │      │     🔴      │     │    🔴     │ │
│ │  🔴    🔴   │      │  🟢    🟢   │     │ 🔴    🔴  │ │
│ │     🔴      │      │     🔴      │     │    🟢     │ │
│ └─────────────┘      └─────────────┘     └───────────┘ │
│ [Edit Phase]         [Edit Phase]        [Edit Phase]  │
│                                                         │
│ Total Cycle: 55 seconds                                │
│                                                         │
│ [+ Add Phase] [Test Timing] [Apply] [Cancel]           │
└─────────────────────────────────────────────────────────┘
```

**Components:**
1. Phase editor (which lights are green)
2. Timing editor (duration per phase)
3. Cycle preview/simulation
4. Yellow/All-Red intervals
5. Pedestrian phases

**Effort:** 2-3 weeks
**Impact:** MASSIVE - most requested feature

#### Feature E: Vehicle Restrictions
**Description:** Control which vehicles can use roads/lanes

| Restriction Type | Use Case |
|------------------|----------|
| Bus Only | Bus lanes |
| No Trucks | Residential areas |
| Local Access Only | Prevent through traffic |
| Emergency Only | Hospital access |
| No Left Turn | Prevent dangerous turns |

**Effort:** 1-2 weeks
**Impact:** High - common request

---

## 🏗️ Technical Approach

### Understanding the Existing Codebase

#### Repository Structure (Expected)
```
Traffic/
├── src/
│   ├── TrafficPlugin.cs          # Entry point
│   ├── Patches/
│   │   ├── LaneConnectionPatch.cs
│   │   ├── SpeedLimitPatch.cs
│   │   └── PriorityPatch.cs
│   ├── Systems/
│   │   ├── TrafficDataSystem.cs
│   │   └── TrafficUISystem.cs
│   ├── UI/
│   │   ├── TrafficToolPanel.cs
│   │   └── Components/
│   └── Data/
│       └── TrafficSettings.cs
├── Traffic.csproj
└── README.md
```

#### Key Systems to Understand
1. **Lane Connection System** - How lane arrows work
2. **Node Data System** - Intersection configuration
3. **Traffic Light System** - Signal state management
4. **Tool System** - How the mod's tools interact with game

### Coding Standards (Match Existing)

```csharp
// Follow existing patterns
public class MyNewFeature : GameSystemBase
{
    // Use existing naming conventions
    private TrafficDataSystem _trafficData;
    
    protected override void OnCreate()
    {
        base.OnCreate();
        _trafficData = World.GetOrCreateSystemManaged<TrafficDataSystem>();
    }
    
    protected override void OnUpdate()
    {
        // Match existing update patterns
    }
}
```

### Git Workflow

```bash
# 1. Fork the repository on GitHub

# 2. Clone your fork
git clone https://github.com/YOUR_USERNAME/Traffic.git
cd Traffic

# 3. Add upstream remote
git remote add upstream https://github.com/Cities2Modding/Traffic.git

# 4. Create feature branch
git checkout -b feature/timed-traffic-lights

# 5. Make changes, commit often
git add .
git commit -m "feat: add phase editor UI component"

# 6. Keep updated with upstream
git fetch upstream
git rebase upstream/main

# 7. Push and create PR
git push origin feature/timed-traffic-lights
```

---

## 📋 Contribution Checklist

### Before Starting
- [ ] Read CONTRIBUTING.md (if exists)
- [ ] Review open issues and PRs
- [ ] Check Discord for ongoing discussions
- [ ] Understand code style/patterns
- [ ] Set up development environment

### For Each Contribution
- [ ] Create issue first (for larger features)
- [ ] Get maintainer buy-in
- [ ] Write clean, documented code
- [ ] Add/update tests if applicable
- [ ] Update documentation
- [ ] Test thoroughly
- [ ] Create focused PR (one feature per PR)
- [ ] Respond to review feedback

### PR Best Practices
```markdown
## Description
Brief description of what this PR does.

## Changes
- Added X feature
- Fixed Y bug
- Updated Z documentation

## Testing
How was this tested?
- Tested with X scenario
- Verified Y behavior

## Screenshots
(If UI changes)

## Related Issues
Closes #123
```

---

## 🎯 High-Value Contribution Ideas

### Prioritized by Impact & Feasibility

| Priority | Feature | Effort | Impact | Notes |
|----------|---------|--------|--------|-------|
| 1 | Timed Traffic Lights | 2-3 wks | 🔥🔥🔥🔥🔥 | Most requested |
| 2 | Vehicle Restrictions | 1-2 wks | 🔥🔥🔥🔥 | Bus lanes, truck routes |
| 3 | Speed Limit Improvements | 1 wk | 🔥🔥🔥 | Better UI, presets |
| 4 | Copy/Paste Settings | 3-5 days | 🔥🔥🔥 | Workflow improvement |
| 5 | Turn Restrictions | 1 wk | 🔥🔥🔥 | No left turn, etc. |
| 6 | Lane Use Signs | 1 wk | 🔥🔥 | This lane buses only |
| 7 | Traffic Counting | 3-5 days | 🔥🔥 | Analytics feature |

---

## 📅 Suggested Timeline

### Week 1: Onboarding
- [ ] Fork and build project
- [ ] Study codebase architecture
- [ ] Join Discord, introduce yourself
- [ ] Find first issue to work on

### Week 2: First Contribution
- [ ] Submit first PR (bug fix or docs)
- [ ] Get feedback, iterate
- [ ] Merge first contribution! 🎉
- [ ] Pick next small feature

### Week 3-4: Build Reputation
- [ ] Complete 2-3 small contributions
- [ ] Participate in code reviews
- [ ] Help other contributors
- [ ] Propose larger feature

### Week 5+: Major Feature
- [ ] Get buy-in for major feature
- [ ] Design and implement
- [ ] Collaborate with maintainers
- [ ] Ship it!

---

## 🤝 Community Engagement

### Cities2Modding Discord
- **#traffic-mod** - Main discussion channel
- **#help** - Get assistance
- **#showcase** - Share your work

### How to Propose Features
1. Check if already proposed/rejected
2. Create detailed issue with mockups
3. Get community feedback
4. Wait for maintainer approval
5. Then implement

### Building Relationships
- Be respectful and patient
- Help others when you can
- Give credit appropriately
- Accept feedback gracefully
- Celebrate others' contributions

---

## ⚠️ Common Pitfalls

| Pitfall | How to Avoid |
|---------|--------------|
| Huge first PR | Start small, build trust |
| Ignoring existing patterns | Study codebase first |
| Not communicating | Discuss before implementing |
| Breaking changes | Maintain backwards compatibility |
| Abandoned PRs | Follow through on feedback |

---

## 📚 Resources

### Traffic Mod
- GitHub: https://github.com/Cities2Modding/Traffic
- Discord: Cities2Modding server

### Learning Materials
- Harmony patching guide
- CS2 modding wiki
- Unity ECS documentation

### TMPE (for reference)
- GitHub: https://github.com/CitiesSkylinesMods/TMPE
- Study its architecture for inspiration

---

## ✅ Definition of Done

### For This Contribution Path
- [ ] Successfully contributed to Traffic mod
- [ ] At least 1 merged PR
- [ ] Understanding of traffic modding systems
- [ ] Relationships with mod community
- [ ] Ready for larger contributions or own mods

### Signs of Success
- Your PR gets merged
- Positive feedback from maintainers
- Users thank you for features
- You're mentioned in changelog
- You become a regular contributor

