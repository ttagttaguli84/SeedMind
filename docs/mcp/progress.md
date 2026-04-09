# SeedMind — MCP 태스크 실행 진행 상황

> 최종 업데이트: 2026-04-10
> **갱신 규칙**: 각 MCP 태스크 파일 실행 완료 직후 해당 항목을 ✅로 바꾸고 커밋. 세션 종료와 무관하게 완료 즉시 갱신한다.
> 새 세션 시작 시 `/start`가 이 파일을 읽어 진행 위치를 복원한다.

---

## 현재 위치

**Phase B — Core Systems (핵심 시스템)**
- 다음 실행 파일: `save-load-tasks.md` (B-2), `time-season-tasks.md` (B-3), `progression-tasks.md` (B-4) 병렬 진행 가능

---

## Phase A — Foundation ✅

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `scene-setup-tasks.md` (ARC-002) | ✅ 완료 | SCN_Farm, SCN_MainMenu, SCN_Loading, Build Settings 등록 완료 |

---

## Phase B — Core Systems

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `farming-tasks.md` (ARC-003) | ✅ 완료 | Phase A~C 완료 (그리드 64타일, 작물SO, 도구SO, 스크립트 컴포넌트, 레이어). Phase D(Play Mode 검증) 미실행 |
| `save-load-tasks.md` (ARC-012) | ⬜ 미시작 | |
| `time-season-tasks.md` (ARC-021) | ⬜ 미시작 | |
| `progression-tasks.md` (BAL-002-MCP) | ⬜ 미시작 | |

---

## Phase C — Content

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `crop-content-tasks.md` (CON-001-ARC) | ⬜ 미시작 | farming-tasks.md 완료 후 |
| `facilities-tasks.md` (ARC-007) | ⬜ 미시작 | farming-tasks.md 완료 후 |
| `inventory-tasks.md` (ARC-013) | ⬜ 미시작 | facilities-tasks.md 완료 후 |

---

## Phase D — Feature Systems

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `tool-upgrade-tasks.md` (ARC-015) | ⬜ 미시작 | |
| `npc-shop-tasks.md` (ARC-009) | ⬜ 미시작 | |
| `blacksmith-tasks.md` (ARC-020) | ⬜ 미시작 | |
| `processing-tasks.md` (ARC-014) | ⬜ 미시작 | |
| `quest-tasks.md` (ARC-016) | ⬜ 미시작 | |
| `achievement-tasks.md` (ARC-017-MCP) | ⬜ 미시작 | |
| `tutorial-tasks.md` (ARC-010) | ⬜ 미시작 | |

---

## Phase E — UI & UX

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `ui-tasks.md` (ARC-022) | ⬜ 미시작 | |
| `sound-tasks.md` (ARC-027) | ⬜ 미시작 | |

---

## Phase F — Advanced Features

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `farm-expansion-tasks.md` (ARC-025) | ⬜ 미시작 | |
| `livestock-tasks.md` (ARC-024) | ⬜ 미시작 | |
| `fishing-tasks.md` (ARC-028) | ⬜ 미시작 | |
| `gathering-tasks.md` (ARC-032) | ⬜ 미시작 | |

---

## Phase G — Polish

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `collection-tasks.md` (ARC-041) | ⬜ 미시작 | |
| `decoration-tasks.md` (ARC-046) | ⬜ 미시작 | |

---

## 실전 메모 (세션 중 발견사항)

- farming-tasks.md: 타일 레이어(FarmTile, index 8), 작물 SO, 도구 SO, 프리팹 12개 등이 이전 세션에 이미 완성된 상태였음. GrowthSystem.farmGrid 참조(null→FarmGrid)만 2026-04-10 세션에서 보완.
- scene-setup-tasks.md: Canvas_Overlay는 비활성(SetActive=false) 상태이므로 find_gameobjects에서 검색 안 됨 — include_inactive=true 필요.
- Build Settings: SCN_Loading(0), SCN_MainMenu(1), SCN_Farm(2) 이미 등록 완료.

---

## 실행 순서 참조

`docs/mcp/build-order.md` 섹션 3 참조.
