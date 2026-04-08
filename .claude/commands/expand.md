---
description: Expand a specific system's documentation. Args: <system-name>
---

Expand documentation for a specific game system.

Arguments: $ARGUMENTS

## Process

1. Parse system name from arguments (e.g., "farming", "economy", "time", "building")
2. **Main session reads** the following files (do NOT delegate file reading to agents):
   - `docs/design.md` — system's current design
   - `docs/architecture.md` — system's current architecture
   - Any task-relevant system docs under `docs/systems/` or `docs/content/`
3. Select the agent based on task type (following `start.md` Phase 2 strategy):
   - **New system design (DES-*)**: designer → architect (sequential)
   - **Supplementing existing architecture (ARC-*)**: spawn architect only
   - **Content/balance (CON-*, BAL-*)**: spawn designer only
4. **Inject pre-read content into each agent prompt** using the block below. Agents must NOT re-read files already provided.
   ```
   ## Pre-loaded Documents (do NOT re-read these files)
   ### docs/design.md
   <paste content>
   ### docs/architecture.md
   <paste content>
   ### [other files read above]
   <paste content>
   ```
5. Spawn **reviewer** — inject the newly written document content + canonical docs into its prompt
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
