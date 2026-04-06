---
description: Expand a specific system's documentation. Args: <system-name>
---

Expand documentation for a specific game system.

Arguments: $ARGUMENTS

## Process

1. Parse system name from arguments (e.g., "farming", "economy", "time", "building")
2. Read `docs/design.md` for the system's current design
3. Read `docs/architecture.md` for the system's current architecture
4. Spawn **designer** and **architect** agents in parallel:
   - Designer: create/expand `docs/systems/<system>.md` (game design perspective)
   - Architect: update the same file with technical specs, or create `docs/mcp/<system>-tasks.md`
5. Spawn **reviewer** to check the new content against existing docs
6. Fix any issues found
7. Update `TODO.md`
8. Commit and push

## Available Systems
- farming (경작/타일/작물)
- economy (골드/상점/가격)
- time (시간/계절/날씨)
- building (시설/확장)
- player (이동/도구/인벤토리)
- ui (HUD/메뉴/인터랙션)
- progression (레벨/해금/목표)
- visual (아트스타일/머티리얼/라이팅)
- audio (BGM/SFX/앰비언트)
