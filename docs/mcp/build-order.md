# SeedMind — MCP 태스크 빌드 순서 로드맵

> 작성: Claude Code (Sonnet 4.6) | 2026-04-08
> 문서 ID: ARC-044
> 이 문서는 Phase 2 Unity 구현 시 MCP 태스크 시퀀스의 실행 순서와 의존성 그래프를 정의한다.

---

## 1. 개요

### 1.1 목적

SeedMind의 23개 MCP 태스크 파일을 어떤 순서로 실행해야 하는지 정의한다. 각 태스크는 선행 의존성이 있으며, 이를 무시하고 실행하면 스크립트 컴파일 실패, 컴포넌트 부착 오류, SO 참조 누락 등이 발생한다. 이 문서는 Phase 2 착수 시 실행 순서 로드맵 역할을 한다.

### 1.2 Phase 그룹 구성

전체 태스크를 7개 Phase로 분류한다.

| Phase | 그룹명 | 설명 |
|-------|--------|------|
| A | Foundation (기반) | 모든 시스템의 공통 전제 조건 — 씬 골격, 프로젝트 구조 |
| B | Core Systems (핵심 시스템) | 게임 루프의 기반이 되는 시스템 — 농장, 시간, 세이브, 인벤토리, 진행 |
| C | Content (콘텐츠) | Core 위에 올라가는 데이터 레이어 — 작물 데이터, 시설, NPC 상점 |
| D | Feature Systems (기능 시스템) | 도구 업그레이드, 가공, 튜토리얼, 퀘스트, 업적 |
| E | UI & UX | UI 시스템 통합, 사운드 |
| F | Advanced Features (고급 기능) | 농장 확장, 낚시, 채집, 목축 |
| G | Polish (완성도) | 수집 도감, 장식 |

### 1.3 MCP 호출 총 예상 횟수

| 파일 | 예상 MCP 호출 수 |
|------|----------------|
| scene-setup-tasks.md | ~175회 |
| farming-tasks.md | ~255회 |
| crop-content-tasks.md | [OPEN - 총 호출 수 미집계] |
| time-season-tasks.md | ~312회 (단축 시 ~126회) |
| progression-tasks.md | [OPEN - 총 호출 수 미집계] |
| inventory-tasks.md | ~118회 |
| facilities-tasks.md | ~232회 |
| npc-shop-tasks.md | ~166회 |
| tool-upgrade-tasks.md | ~172회 |
| blacksmith-tasks.md | ~156회 |
| processing-tasks.md | ~651회 (Editor 스크립트 사용 시 ~139회) |
| save-load-tasks.md | ~94회 |
| quest-tasks.md | ~181회 |
| achievement-tasks.md | ~628회 (T-2-ALT 사용 시 ~105회) |
| ui-tasks.md | ~130회 |
| tutorial-tasks.md | ~352회 |
| sound-tasks.md | ~148회 |
| farm-expansion-tasks.md | ~99회 |
| livestock-tasks.md | ~221회 |
| fishing-tasks.md | ~278회 |
| gathering-tasks.md | ~220회 |
| collection-tasks.md | ~126회 |
| decoration-tasks.md | ~105회 |

> 합계 ([OPEN] 항목 제외): **~5,099회** (최적화 시 ~2,503회)
> Editor 스크립트 우회를 적극 활용할 것을 권장한다 (→ see 각 태스크 파일의 [RISK] 섹션).

---

## 2. 의존성 그래프

각 태스크 파일과 선행 의존 파일 관계를 정리한다.

| Phase | 태스크 파일 | 문서 ID | 선행 파일 | 예상 MCP 호출 |
|-------|------------|---------|-----------|--------------|
| A | scene-setup-tasks.md | ARC-002 | 없음 (최초) | ~175회 |
| B | farming-tasks.md | ARC-003 | scene-setup-tasks.md | ~255회 |
| B | time-season-tasks.md | ARC-021 | scene-setup-tasks.md, farming-tasks.md | ~312회 |
| B | save-load-tasks.md | ARC-012 | scene-setup-tasks.md, farming-tasks.md | ~94회 |
| B | progression-tasks.md | BAL-002-MCP | scene-setup-tasks.md | [OPEN - 미집계] |
| C | crop-content-tasks.md | CON-001-ARC | scene-setup-tasks.md, farming-tasks.md | [OPEN - 미집계] |
| C | facilities-tasks.md | ARC-007 | scene-setup-tasks.md, farming-tasks.md | ~232회 |
| C | inventory-tasks.md | ARC-013 | scene-setup-tasks.md, farming-tasks.md, facilities-tasks.md | ~118회 |
| D | npc-shop-tasks.md | ARC-009 | scene-setup-tasks.md, farming-tasks.md, tool-upgrade-tasks.md | ~166회 |
| D | tool-upgrade-tasks.md | ARC-015 | scene-setup-tasks.md, farming-tasks.md, facilities-tasks.md | ~172회 |
| D | blacksmith-tasks.md | ARC-020 | scene-setup-tasks.md, farming-tasks.md, npc-shop-tasks.md, tool-upgrade-tasks.md | ~156회 |
| D | processing-tasks.md | ARC-014 | scene-setup-tasks.md, farming-tasks.md, facilities-tasks.md | ~651회 |
| D | quest-tasks.md | ARC-016 | scene-setup-tasks.md, farming-tasks.md, npc-shop-tasks.md, save-load-tasks.md, progression-tasks.md, tutorial-tasks.md | ~181회 |
| D | achievement-tasks.md | ARC-017-MCP | scene-setup-tasks.md, farming-tasks.md, facilities-tasks.md, npc-shop-tasks.md, save-load-tasks.md, tool-upgrade-tasks.md, quest-tasks.md, progression-tasks.md, processing-tasks.md, gathering-tasks.md | ~628회 |
| D | tutorial-tasks.md | ARC-010 | scene-setup-tasks.md, farming-tasks.md, tool-upgrade-tasks.md, npc-shop-tasks.md | ~352회 |
| E | ui-tasks.md | ARC-022 | scene-setup-tasks.md, farming-tasks.md, inventory-tasks.md, npc-shop-tasks.md, save-load-tasks.md, processing-tasks.md, quest-tasks.md, achievement-tasks.md, progression-tasks.md, tutorial-tasks.md, time-season-tasks.md | ~130회 |
| E | sound-tasks.md | ARC-027 | scene-setup-tasks.md, farming-tasks.md, time-season-tasks.md | ~148회 |
| F | farm-expansion-tasks.md | ARC-025 | scene-setup-tasks.md, farming-tasks.md, inventory-tasks.md | ~99회 |
| F | livestock-tasks.md | ARC-024 | scene-setup-tasks.md, farming-tasks.md, facilities-tasks.md, inventory-tasks.md, farm-expansion-tasks.md | ~221회 |
| F | fishing-tasks.md | ARC-028 | scene-setup-tasks.md, farming-tasks.md, inventory-tasks.md, farm-expansion-tasks.md | ~278회 |
| F | gathering-tasks.md | ARC-032 | scene-setup-tasks.md, farming-tasks.md, fishing-tasks.md, progression-tasks.md, inventory-tasks.md, save-load-tasks.md | ~220회 |
| G | collection-tasks.md | ARC-041 | scene-setup-tasks.md, fishing-tasks.md, gathering-tasks.md, progression-tasks.md, save-load-tasks.md | ~126회 |
| G | decoration-tasks.md | ARC-046 | scene-setup-tasks.md, farming-tasks.md, inventory-tasks.md, save-load-tasks.md, farm-expansion-tasks.md | ~105회 |

---

## 3. 단계별 빌드 순서

### Phase A — Foundation (기반)

모든 시스템의 전제 조건. 이 단계 없이는 어떤 태스크도 실행할 수 없다.

| 순서 | 태스크 파일 | 핵심 결과물 |
|------|------------|------------|
| A-1 | scene-setup-tasks.md | Unity 프로젝트 폴더 구조, SCN_Farm 씬 골격 (MANAGERS, FARM, PLAYER, UI 계층), Canvas 계층 4종, GameManager, SaveManager, DataRegistry |

**Phase A 완료 조건**: SCN_Farm 씬에 기본 계층 구조가 존재하고, `Assets/_Project/` 폴더 트리가 완성된 상태.

---

### Phase B — Core Systems (핵심 시스템)

게임 루프의 기반. 농장 그리드, 시간 시스템, 세이브/로드, 인벤토리 기반 인프라, 진행 시스템을 구성한다.

Phase B 내 태스크들은 일부 병렬 실행이 가능하나, `farming-tasks.md`가 나머지 대부분의 B 태스크보다 선행되어야 한다.

| 순서 | 태스크 파일 | 선행 | 핵심 결과물 |
|------|------------|------|------------|
| B-1 | farming-tasks.md | A-1 완료 | FarmGrid (8x8), FarmTile 64개, GrowthSystem, ToolSystem, FarmEvents |
| B-2 | save-load-tasks.md | A-1, B-1 완료 | SaveManager, ISaveable, AutoSaveTrigger, GameSaveData 루트 |
| B-3 | time-season-tasks.md | A-1, B-1 완료 | TimeManager, WeatherSystem, FestivalManager, SeasonData SO 4종 |
| B-4 | progression-tasks.md | A-1 완료 | ProgressionManager, ProgressionData SO, LevelBarUI |

**병렬 가능**: B-2, B-3, B-4는 B-1 완료 후 서로 독립적으로 진행 가능.

---

### Phase C — Content (콘텐츠)

Core Systems 위에 올라가는 데이터 레이어. 작물 ScriptableObject, 시설, 인벤토리 시스템을 구성한다.

| 순서 | 태스크 파일 | 선행 | 핵심 결과물 |
|------|------------|------|------------|
| C-1 | crop-content-tasks.md | A-1, B-1 완료 | CropData SO 8종 + 겨울 작물 3종, 성장 단계 프리팹, DataRegistry 등록 |
| C-2 | facilities-tasks.md | A-1, B-1 완료 | BuildingData SO 7종, BuildingManager, 시설 프리팹 7종 |
| C-3 | inventory-tasks.md | A-1, B-1, C-2 완료 | InventoryManager, PlayerInventory, InventoryPanel, ToolbarPanel |

**병렬 가능**: C-1과 C-2는 서로 독립적으로 진행 가능. C-3는 C-2 완료 후 시작.

---

### Phase D — Feature Systems (기능 시스템)

콘텐츠 위에 올라가는 기능 시스템. 도구 업그레이드, 대장간, 가공, NPC, 튜토리얼, 퀘스트, 업적을 포함한다.

| 순서 | 태스크 파일 | 선행 | 핵심 결과물 |
|------|------------|------|------------|
| D-1 | tool-upgrade-tasks.md | A-1, B-1, C-2 완료 | ToolData SO 9종 (3등급), ToolUpgradeSystem, ToolUpgradeEvents |
| D-2 | npc-shop-tasks.md | A-1, B-1, D-1 완료 | NPCData SO 4종, DialogueSystem, NPCManager, DialoguePanel |
| D-3 | blacksmith-tasks.md | A-1, B-1, D-1, D-2 완료 | BlacksmithNPCData SO, ToolUpgradeScreen UI |
| D-4 | processing-tasks.md | A-1, B-1, C-2 완료 | ProcessingRecipeData SO 32종, ProcessingSystem, ProcessingUI |
| D-5 | tutorial-tasks.md | A-1, B-1, D-1, D-2 완료 | TutorialManager, TutorialStepData SO, Canvas_Tutorial |
| D-6 | quest-tasks.md | A-1, B-1, D-2, B-2, B-4, D-5 완료 | QuestData SO 20종, QuestManager, QuestLogPanel |
| D-7 | achievement-tasks.md | A-1, B-1, C-2, D-2, B-2, D-1, D-6, B-4, D-4, gathering-tasks.md 완료 | AchievementData SO, AchievementManager, AchievementPanel |

**주의**: D-7(업적)은 채집 시스템(Phase F의 gathering-tasks.md)에도 의존하므로 실제로는 Phase F 이후로 이동하거나, 채집 업적(T-7)만 별도로 후속 단계에 배치하는 것이 현실적이다. 채집 업적 5종(A-031~A-035)에 한해 Phase G로 이동 가능.

---

### Phase E — UI & UX

모든 시스템 이벤트를 집결하는 UI 통합 레이어와 사운드 시스템.

| 순서 | 태스크 파일 | 선행 | 핵심 결과물 |
|------|------------|------|------------|
| E-1 | ui-tasks.md | A-1, B-1, C-3, D-2, B-2, D-4, D-6, D-7, B-4, D-5, B-3 완료 | UIManager, ScreenBase 파생 8종, PopupQueue, NotificationManager |
| E-2 | sound-tasks.md | A-1, B-1, B-3 완료 | SoundManager, AudioMixer, SoundData SO, BGMScheduler |

**병렬 가능**: E-1과 E-2는 선행 조건이 겹치지 않으므로 병렬 진행 가능.

---

### Phase F — Advanced Features (고급 기능)

농장 확장 후 열리는 추가 콘텐츠 시스템.

| 순서 | 태스크 파일 | 선행 | 핵심 결과물 |
|------|------------|------|------------|
| F-1 | farm-expansion-tasks.md | A-1, B-1, C-3 완료 | FarmZoneManager, ZoneData SO 7종, 장애물 개간 시스템 |
| F-2 | livestock-tasks.md | A-1, B-1, C-2, C-3, F-1 완료 | AnimalData SO, AnimalManager, HappinessCalculator |
| F-3 | fishing-tasks.md | A-1, B-1, C-3, F-1 완료 | FishData SO, FishingManager, FishingMinigame, FishCatalogManager |
| F-4 | gathering-tasks.md | A-1, B-1, F-3, B-4, C-3, B-2 완료 | GatheringItemData SO, GatheringManager, GatheringProficiency |

**병렬 가능**: F-2와 F-3는 F-1 완료 후 서로 독립적으로 진행 가능. F-4는 F-3 완료 후 시작.

---

### Phase G — Polish (완성도)

도감 통합과 장식 시스템.

| 순서 | 태스크 파일 | 선행 | 핵심 결과물 |
|------|------------|------|------------|
| G-1 | collection-tasks.md | A-1, F-3, F-4, B-4, B-2 완료 | GatheringCatalogData SO, CollectionUIController, FishCatalog 통합 UI |
| G-2 | decoration-tasks.md | A-1, B-1, C-3, B-2, F-1 완료 | DecorationItemData SO 29종, DecorationManager, DecorationConfig SO |

**병렬 가능**: G-1과 G-2는 서로 독립적으로 진행 가능.

---

## 4. 병렬 구현 가능 그룹

같은 Phase 내에서 병렬로 진행 가능한 태스크 쌍 목록.

| Phase | 병렬 가능 태스크 A | 병렬 가능 태스크 B | 조건 |
|-------|-----------------|-----------------|------|
| B | save-load-tasks.md | time-season-tasks.md | farming-tasks.md 완료 후 |
| B | save-load-tasks.md | progression-tasks.md | scene-setup-tasks.md 완료 후 |
| B | time-season-tasks.md | progression-tasks.md | scene-setup-tasks.md 완료 후 |
| C | crop-content-tasks.md | facilities-tasks.md | farming-tasks.md 완료 후 |
| D | tool-upgrade-tasks.md | processing-tasks.md | facilities-tasks.md 완료 후 |
| D | npc-shop-tasks.md | processing-tasks.md | tool-upgrade-tasks.md 완료 후 (npc), facilities-tasks.md 완료 후 (processing) |
| E | ui-tasks.md | sound-tasks.md | 각각의 선행 조건 충족 후 |
| F | livestock-tasks.md | fishing-tasks.md | farm-expansion-tasks.md 완료 후 |
| G | collection-tasks.md | decoration-tasks.md | 각각의 선행 조건 충족 후 |

---

## 5. 크리티컬 패스

전체 구현에서 가장 긴 의존성 체인. MVP 완성까지의 최단 경로.

### MVP 정의

농장 그리드에서 작물을 심고, 자라고, 수확하고, 판매할 수 있는 상태. 최소한의 저장/로드 지원.

**MVP 크리티컬 패스**:

```
scene-setup-tasks.md (A)
  → farming-tasks.md (B-1)
    → crop-content-tasks.md (C-1)
    → save-load-tasks.md (B-2)
```

예상 MCP 호출: ~175 + ~255 + [OPEN] + ~94 = **~524회 + crop-content [OPEN - 미집계]**

### 전체 기능 크리티컬 패스 (가장 긴 의존성 체인)

```
scene-setup-tasks.md (A-1)
  → farming-tasks.md (B-1)
    → facilities-tasks.md (C-2)
      → tool-upgrade-tasks.md (D-1)
        → npc-shop-tasks.md (D-2)
          → tutorial-tasks.md (D-5)
            → quest-tasks.md (D-6)
              → achievement-tasks.md (D-7, 채집 업적 제외)
                → ui-tasks.md (E-1)
```

이 체인을 따라 직렬 실행 시, 크리티컬 패스 길이는 8단계이다.

병렬 실행 (save-load, time-season, progression, crop-content, processing을 크리티컬 패스와 동시 진행) 시, 전체 구현 완료 기간을 단축할 수 있다.

---

## 6. 구현 시 주의사항

### 6.1 Editor 스크립트 우회 권장 대상

MCP 단독 실행 시 호출 수가 과다한 태스크는 Editor 스크립트로 대체할 것을 권장한다.

| 태스크 파일 | MCP 단독 | Editor 스크립트 사용 시 | 절감 |
|------------|---------|----------------------|------|
| processing-tasks.md (P-2) | ~517회 | ~5회 | ~512회 |
| achievement-tasks.md (T-2) | ~451회 | ~5회 | ~446회 |
| farm-expansion-tasks.md (Z-8) | ~56회 | ~3회 | ~53회 |
| decoration-tasks.md (D-C) | ~65회 | ~5회 | ~60회 |

### 6.2 컴파일 순서 준수

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있다. 각 태스크 파일의 스크립트 목록 순서를 반드시 준수하고, Phase 사이에 `execute_menu_item`(Unity 컴파일 대기)을 삽입한다.

### 6.3 중복 생성 방지

각 태스크 파일의 "이미 존재하는 오브젝트" 섹션을 반드시 확인한다. 특히 `--- MANAGERS ---`, `Canvas_Overlay`, `DataRegistry` 등은 ARC-002에서 생성되며, 이후 모든 태스크에서 재생성하면 안 된다.

---

## 7. Cross-references

- `docs/mcp/scene-setup-tasks.md` — Phase A 기반 씬 구성 (ARC-002)
- `docs/mcp/farming-tasks.md` — Phase B 농장 그리드 구성 (ARC-003)
- `docs/mcp/crop-content-tasks.md` — Phase C 작물 SO 에셋 생성 (CON-001-ARC)
- `docs/mcp/time-season-tasks.md` — Phase B 시간/계절 시스템 (ARC-021)
- `docs/mcp/progression-tasks.md` — Phase B 진행 시스템 (BAL-002-MCP)
- `docs/mcp/inventory-tasks.md` — Phase C 인벤토리 시스템 (ARC-013)
- `docs/mcp/facilities-tasks.md` — Phase C 시설 시스템 (ARC-007)
- `docs/mcp/npc-shop-tasks.md` — Phase D NPC/상점 (ARC-009)
- `docs/mcp/tutorial-tasks.md` — Phase D 튜토리얼 (ARC-010)
- `docs/mcp/save-load-tasks.md` — Phase B 세이브/로드 (ARC-012)
- `docs/mcp/tool-upgrade-tasks.md` — Phase D 도구 업그레이드 (ARC-015)
- `docs/mcp/blacksmith-tasks.md` — Phase D 대장간 NPC (ARC-020)
- `docs/mcp/processing-tasks.md` — Phase D 가공 시스템 (ARC-014)
- `docs/mcp/quest-tasks.md` — Phase D 퀘스트 시스템 (ARC-016)
- `docs/mcp/achievement-tasks.md` — Phase D 업적 시스템 (ARC-017-MCP)
- `docs/mcp/ui-tasks.md` — Phase E UI 시스템 (ARC-022)
- `docs/mcp/farm-expansion-tasks.md` — Phase F 농장 확장 (ARC-025)
- `docs/mcp/livestock-tasks.md` — Phase F 목축/낙농 (ARC-024)
- `docs/mcp/fishing-tasks.md` — Phase F 낚시 (ARC-028)
- `docs/mcp/gathering-tasks.md` — Phase F 채집 (ARC-032)
- `docs/mcp/sound-tasks.md` — Phase E 사운드 (ARC-027)
- `docs/mcp/collection-tasks.md` — Phase G 수집 도감 (ARC-041)
- `docs/mcp/decoration-tasks.md` — Phase G 장식 (ARC-046)
- `docs/architecture.md` — 마스터 기술 아키텍처
- `docs/systems/project-structure.md` — Unity 폴더 구조 및 네임스페이스 canonical
- `docs/pipeline/data-pipeline.md` — SO 에셋 스키마 canonical
