---
description: 세션 시작 시 이것만 실행. 컨텍스트 복원 → 최우선 작업 선택 → 설계 확장 → 검증 ��� 커밋 푸시까지 자동 수행
---

You are running a **planning session** for SeedMind.

## Phase 0 — Context Restore (새 세션마다 필수)

1. Read `CLAUDE.md` — project rules
2. Read `TODO.md` — current backlog
3. Read `README.md` — current phase/status
4. Read the latest `docs/devlog/*.md` — what happened last session
5. Read `docs/design.md` and `docs/architecture.md` — current state
6. Print a 3-line status summary to the user:
   ```
   [SeedMind] Phase N | TODO: N items | 최근 작업: <last devlog title>
   이번 세션 목표: <highest-priority TODO item>
   진행합니다.
   ```

## Phase 1 — Task Selection

- Pick the highest-priority item from `TODO.md`
- If multiple items share the same priority, pick the one that unblocks the most other items

## Phase 2 — Design Expansion

Spawn two agents in parallel:
- **designer** agent: expand the game design for the target system
- **architect** agent: expand the technical architecture for the same system

## Phase 3 — Review

Spawn **reviewer** agent to check consistency across all docs.
Apply any fixes the reviewer identifies.

## Phase 4 — Wrap Up

1. Update `TODO.md` — remove completed items, add new ones discovered
2. If TODO.md drops below 10 rows: analyze docs for gaps, add new items
3. Update `README.md` current status if a phase milestone was reached
4. Write a devlog entry in `docs/devlog/NNN-<title>.md`
5. Commit and push

## Rules
- One system per session — depth over breadth
- Designer and architect must reference each other's output
- Reviewer has final say on consistency issues
- All output in Korean. Document content in Korean (technical terms in English where natural).
- If time/context allows after completing the main task, pick the next TODO item and repeat Phase 1→4.
