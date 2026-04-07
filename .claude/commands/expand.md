---
description: Expand a specific system's documentation. Args: <system-name>
---

Expand documentation for a specific game system.

Arguments: $ARGUMENTS

## Process

1. Parse system name from arguments (e.g., "farming", "economy", "time", "building")
2. Read `docs/design.md` for the system's current design
3. Read `docs/architecture.md` for the system's current architecture
4. Select the agent based on task type (following `start.md` Phase 2 strategy):
   - **New system design (DES-*)**: spawn designer + architect in parallel
   - **Supplementing existing architecture (ARC-*)**: spawn architect only (read design document first)
   - **Content/balance (CON-*, BAL-*)**: spawn designer only
5. Spawn **reviewer** to check the new content against existing docs (all 14 checklist items must be verified)
6. Fix any issues found
7. Update `TODO.md`
8. Commit and push

## Available Systems
- farming (cultivation/tiles/crops)
- economy (gold/shop/prices)
- time (time/seasons/weather)
- building (facilities/expansion)
- player (movement/tools/inventory)
- ui (HUD/menus/interaction)
- progression (levels/unlocks/goals)
- visual (art style/materials/lighting)
- audio (BGM/SFX/ambient)
