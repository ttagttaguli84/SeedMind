# SeedMind — MCP 태스크 실행 진행 상황

> 최종 업데이트: 2026-04-10 (facilities-tasks.md F-1~F-4 완료)
> **갱신 규칙**: 각 MCP 태스크 파일 실행 완료 직후 해당 항목을 ✅로 바꾸고 커밋. 세션 종료와 무관하게 완료 즉시 갱신한다.
> 새 세션 시작 시 `/start`가 이 파일을 읽어 진행 위치를 복원한다.

---

## 현재 위치

**Phase C — Content (콘텐츠)**
- 다음 실행 파일: `inventory-tasks.md` (C-3)

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
| `save-load-tasks.md` (ARC-012) | ✅ 완료 | T-1~T-5 완료 (Singleton/ISaveable/SaveEvents/SaveManager/AutoSaveTrigger/Data 클래스 생성, 씬 배치). T-6(이벤트 연결)은 TimeManager/BuildingEvents 구현 후 처리. T-7(SaveSlotPanel UI), T-8(PauseMenu 연동), T-9(통합 테스트)는 UI 시스템(Phase E) 이후 처리 |
| `time-season-tasks.md` (ARC-021) | ✅ 완료 | Phase A~D 완료 (스크립트 12개, SO 13개, TimeSystem GO 배치). HUDController 시간 표시 연결 및 E-2 저장/로드 테스트는 UI 시스템(Phase E) 이후 처리 |
| `progression-tasks.md` (BAL-002-MCP) | ✅ 완료 | Phase A~B 완료 (스크립트 9개, SO_ProgressionData 생성, ProgressionManager GO 배치, LevelBarUI 연결). Phase C(런타임 검증) execute_code 비활성으로 스킵. unlockTable/milestones 배열은 빈 상태(콘텐츠 확정 후 에디터에서 입력 예정) |

---

## Phase C — Content

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `crop-content-tasks.md` (CON-001-ARC) | ✅ 완료 | Phase A~E 완료. DataRegistry 기본 구조 생성(Resources 배치는 inventory-tasks에서). 검증(V-1~V-4)은 execute_code 비활성으로 스킵. |
| `facilities-tasks.md` (ARC-007) | ✅ 완료 | F-1~F-4 완료 (스크립트 16종, SO 7종, 프리팹 8종, BuildingManager 씬 배치). F-5/F-6(UI 패널), F-8(통합 테스트)는 UI Phase E 이후 처리. |
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

- crop-content-tasks.md: CropData.cs가 이미 단순 버전으로 존재 → GameDataSO+IInventoryItem 상속으로 전면 업데이트. Season [Flags] enum을 SeasonFlag.cs로 분리. Editor 스크립트(CreateCropPrefabs.cs)로 8종×4단계=32 프리팹 + 2 Giant프리팹 + 8 머티리얼 일괄 생성. SO 배열 참조 자동 설정.
- facilities-tasks.md: SO 배열 참조 set_property 불가 → Resources.LoadAll<BuildingData>("Data/Buildings") 자동 로드로 우회. SO 파일 위치를 Data/Buildings → Resources/Data/Buildings로 이동(1회성 에디터 스크립트). GrowthSystem에 SetSeasonOverrideProvider() 주입 패턴으로 온실-계절 연동. BuildingManager에 Singleton 없이 FindObjectOfType 패턴 사용.
- farming-tasks.md: 타일 레이어(FarmTile, index 8), 작물 SO, 도구 SO, 프리팹 12개 등이 이전 세션에 이미 완성된 상태였음. GrowthSystem.farmGrid 참조(null→FarmGrid)만 2026-04-10 세션에서 보완.
- scene-setup-tasks.md: Canvas_Overlay는 비활성(SetActive=false) 상태이므로 find_gameobjects에서 검색 안 됨 — include_inactive=true 필요.
- Build Settings: SCN_Loading(0), SCN_MainMenu(1), SCN_Farm(2) 이미 등록 완료.

---

## 실행 순서 참조

`docs/mcp/build-order.md` 섹션 3 참조.
