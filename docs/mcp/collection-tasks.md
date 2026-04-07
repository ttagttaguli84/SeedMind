# 수집 도감 시스템 MCP 태스크 시퀀스 (ARC-041)

> 작성: Claude Code (Sonnet 4.6) | 2026-04-08 | 문서 ID: ARC-041 | Phase 1

---

## Context

이 문서는 수집 도감(Collection) 시스템의 Unity MCP 구현 태스크 시퀀스를 정의한다. `docs/systems/collection-architecture.md`(ARC-037) 섹션 9(Phase M~R)에서 요약된 구현 계획을 독립 문서로 분리하여, 스크립트 생성 → SO 에셋 → 씬 배치 → 기존 시스템 확장 → UI 연동 → 씬 마이그레이션 → 세이브/로드 연동 → 통합 검증까지의 전체 MCP 호출 시퀀스를 상세히 기술한다.

수집 도감 시스템은 두 서브시스템으로 구성된다:
- **GatheringCatalog** (신규): `docs/systems/collection-architecture.md`(ARC-037)에서 설계된 27종 채집 아이템 도감
- **CollectionUI** (신규): CollectionUIController, FishPanel, GatheringPanel을 묶는 통합 도감 UI
- **FishCatalog 통합** (ARC-039): 기존 FishCatalogPanel을 CollectionPanel 하위로 마이그레이션

**상위 설계 문서**: `docs/systems/collection-architecture.md` (ARC-037)
**패턴 참조**: `docs/mcp/gathering-tasks.md` (ARC-032), `docs/mcp/fishing-tasks.md` (ARC-028)

---

## 1. 개요

### 목적

이 문서는 `docs/systems/collection-architecture.md`(ARC-037) 섹션 9(Phase M~R)에서 요약된 MCP 구현 계획을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 수집 도감 시스템의 데이터 레이어(SO 에셋), 시스템 레이어(스크립트), UI 연동, 씬 마이그레이션, 세이브/로드 통합, 통합 검증을 완성한다.

### 의존성

```
수집 도감 MCP 태스크 의존 관계:
├── SeedMind.Core        (TimeManager, SaveManager, ISaveable, GameSaveData)
├── SeedMind.Progression (ProgressionManager, XPSource.GatheringCatalog 신규 추가)
├── SeedMind.Economy     (EconomyManager -- 초회 보상 골드 지급)
├── SeedMind.Gathering   (GatheringManager, GatheringEvents.OnItemGathered, GatheringItemData, GatheringRarity)
├── SeedMind.Fishing.Catalog (FishCatalogManager -- 읽기 전용 참조)
└── SeedMind.UI          (채집 발견 토스트, CollectionPanel)
```

(-> see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 완료된 태스크 의존성

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조, SCN_Farm 기본 계층 (Managers, Farm, Player, UI, Canvas) |
| ARC-028 | `docs/mcp/fishing-tasks.md` | F-1~F-6 전체 | FishCatalogManager, FishCatalogUI, FishCatalogPanel 프리팹 |
| ARC-032 | `docs/mcp/gathering-tasks.md` | G-A~G-F 전체 | GatheringManager, GatheringItemData, GatheringRarity, GatheringEvents.OnItemGathered |
| ARC-009 | `docs/mcp/progression-tasks.md` | Phase A 이상 | ProgressionManager, XPSource enum |
| ARC-008 | `docs/mcp/save-load-tasks.md` | Phase A 이상 | SaveManager, ISaveable, GameSaveData |

### 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `Canvas_Overlay` (UI 루트) | ARC-002 Phase B |
| `DataRegistry` (SO 로드 시스템) | `docs/pipeline/data-pipeline.md` Part II |
| `FishCatalogManager` (씬 오브젝트) | ARC-028 |
| `FishCatalogPanel` (UI 프리팹 — Q-4a에서 리네이밍 대상) | ARC-028 |
| `GatheringManager` (씬 오브젝트) | ARC-032 |
| `GatheringItemData` SO 에셋 (27종) | ARC-032 G-C |
| `GatheringRarity` enum | ARC-032 G-A |
| `GatheringEvents.OnItemGathered` | ARC-032 G-A |
| `ProgressionManager` (씬 오브젝트) | ARC-009 |
| `SaveManager` (씬 오브젝트) | ARC-008 |
| `XPSource` enum | ARC-009 |

### 총 MCP 호출 예상 수

| 태스크 | 호출 수 | 복잡도 |
|--------|--------|--------|
| Q-A: 데이터 레이어 스크립트 생성 (4종) | ~7회 | 저 |
| Q-B: 시스템 레이어 스크립트 생성 (1종) | ~3회 | 저 |
| Q-C: SO 에셋 인스턴스 생성 (GatheringCatalogData 27종) | ~56회 | 고 |
| Q-D: 씬 배치 (GatheringCatalogManager) | ~5회 | 저 |
| Q-E: 기존 시스템 확장 (XPSource, GameSaveData) | ~5회 | 저 |
| Q-F: UI 스크립트 생성 (5종) 및 씬 계층 구성 | ~25회 | 중 |
| Q-G: FishCatalogPanel 씬 마이그레이션 (Q-4a~Q-4f) | ~7회 | 중 |
| Q-H: 통합 검증 (8개 체크) | ~18회 | 저 |
| **합계** | **~126회** | |

[RISK] SO 에셋 인스턴스 생성(Q-C)의 호출 수는 27종 x 2회 = 54회로 전체의 44%를 차지한다. Editor 스크립트(`CreateGatheringCatalogAssets.cs`)를 통한 일괄 생성으로 Q-C의 ~54회를 ~2회로 압축 가능. MCP 단독 실행 시 시간 비용이 크므로 Editor 스크립트 우회를 강력히 권장한다.

---

## 2. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋 폴더 생성 | Q-A, Q-C |
| `create_script` | C# 스크립트 파일 생성 | Q-A, Q-B, Q-F |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | Q-C |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | Q-C, Q-D, Q-F |
| `create_object` | 빈 GameObject 생성 | Q-D, Q-F |
| `add_component` | MonoBehaviour 컴포넌트 부착 | Q-D, Q-F |
| `set_parent` | 오브젝트 부모 설정 | Q-D, Q-F, Q-G |
| `edit_script` | 기존 스크립트 수정 (enum/field 추가) | Q-E |
| `rename_object` | 씬 오브젝트/프리팹 리네이밍 | Q-G |
| `save_scene` | 씬 저장 | Q-D, Q-F, Q-G, Q-H |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | Q-H |
| `execute_method` | 런타임 메서드 호출 (테스트) | Q-H |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | Q-H |
| `execute_menu_item` | 편집기 명령 실행 (컴파일 대기 등) | Q-A, Q-B, Q-E, Q-F |

[RISK] `create_scriptable_object` 도구의 가용 여부 및 파라미터 형식 사전 검증 필요. SO 인스턴스 생성이 MCP에서 미지원인 경우, Editor 스크립트를 통한 우회 필요. (-> see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)

---

## 3. 필요 C# 스크립트 목록

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 태스크 순서대로 작성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| C-01 | `Scripts/Collection/GatheringCatalogData.cs` | `GatheringCatalogData` (SO) | `SeedMind.Collection` | Q-A |
| C-02 | `Scripts/Collection/GatheringCatalogEntry.cs` | `GatheringCatalogEntry` (class) | `SeedMind.Collection` | Q-A |
| C-03 | `Scripts/Collection/GatheringCatalogSaveData.cs` | `GatheringCatalogSaveData` (class) | `SeedMind.Collection` | Q-A |
| C-04 | `Scripts/Collection/UI/CollectionTab.cs` | `CollectionTab` (enum) | `SeedMind.Collection.UI` | Q-A |
| C-05 | `Scripts/Collection/GatheringCatalogManager.cs` | `GatheringCatalogManager` (MonoBehaviour) | `SeedMind.Collection` | Q-B |
| C-06 | `Scripts/Collection/UI/CollectionUIController.cs` | `CollectionUIController` (MonoBehaviour) | `SeedMind.Collection.UI` | Q-F |
| C-07 | `Scripts/Collection/UI/GatheringCatalogUI.cs` | `GatheringCatalogUI` (MonoBehaviour) | `SeedMind.Collection.UI` | Q-F |
| C-08 | `Scripts/Collection/UI/GatheringCatalogItemUI.cs` | `GatheringCatalogItemUI` (MonoBehaviour) | `SeedMind.Collection.UI` | Q-F |
| C-09 | `Scripts/Collection/UI/GatheringCatalogDetailPanel.cs` | `GatheringCatalogDetailPanel` (MonoBehaviour) | `SeedMind.Collection.UI` | Q-F |
| C-10 | `Scripts/Collection/UI/GatheringCatalogToastUI.cs` | `GatheringCatalogToastUI` (MonoBehaviour) | `SeedMind.Collection.UI` | Q-F |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: C-01~C-04 -> C-05 -> C-06~C-10. 각 그룹 사이에 Unity 컴파일 대기(`execute_menu_item`)가 필요하다.

---

## 4. 태스크 목록

---

### Q-A: 데이터 레이어 스크립트 생성

**목적**: GatheringCatalogData SO, GatheringCatalogEntry, GatheringCatalogSaveData, CollectionTab enum을 생성한다. 총 4개 파일.

**전제 조건**:
- ARC-002 완료 (폴더 구조 존재)
- ARC-032 G-A 완료 (`SeedMind.Gathering` 네임스페이스의 `GatheringRarity`, `GatheringItemData` 컴파일 완료)
- `SeedMind.Core` 네임스페이스의 `ISaveable` 컴파일 완료

**예상 MCP 호출**: ~7회 (2 create_folder + 4 create_script + 1 execute_menu_item)

#### Q-A-01: 스크립트 폴더 생성

```
create_folder
  path: "Assets/_Project/Scripts/Collection"

create_folder
  path: "Assets/_Project/Scripts/Collection/UI"
```

- **MCP 호출**: 2회

#### Q-A-02: GatheringCatalogData SO 스크립트

```
create_script
  path: "Assets/_Project/Scripts/Collection/GatheringCatalogData.cs"
  namespace: "SeedMind.Collection"
  // ScriptableObject 상속
  // [CreateAssetMenu] 어트리뷰트: menuName = "SeedMind/GatheringCatalogData"
  // 필드 스키마: -> see docs/systems/collection-architecture.md 섹션 3
  // 희귀도 필드: GatheringRarity rarity (-> see docs/systems/gathering-architecture.md 섹션 2.2)
  // 보상 필드: firstDiscoverGold, firstDiscoverXP
  //   -> see docs/systems/collection-system.md 섹션 3.3 (canonical 희귀도별 수치)
```

- **MCP 호출**: 1회

#### Q-A-03: GatheringCatalogEntry 런타임 상태 클래스

```
create_script
  path: "Assets/_Project/Scripts/Collection/GatheringCatalogEntry.cs"
  namespace: "SeedMind.Collection"
  // [System.Serializable] class
  // 필드: itemId, isDiscovered, totalGathered, bestQuality, firstGatheredDay,
  //       firstGatheredSeason, firstGatheredYear, [NonSerialized] isNewBestQuality
  // -> see docs/systems/collection-architecture.md 섹션 4
```

- **MCP 호출**: 1회

#### Q-A-04: GatheringCatalogSaveData 세이브 데이터

```
create_script
  path: "Assets/_Project/Scripts/Collection/GatheringCatalogSaveData.cs"
  namespace: "SeedMind.Collection"
  // [System.Serializable] class
  // 필드: List<GatheringCatalogEntry> entries, int discoveredCount
  // -> see docs/systems/collection-architecture.md 섹션 5.2
```

- **MCP 호출**: 1회

#### Q-A-05: CollectionTab enum

```
create_script
  path: "Assets/_Project/Scripts/Collection/UI/CollectionTab.cs"
  namespace: "SeedMind.Collection.UI"
  // enum 값: Fish = 0, Gathering = 1
  // -> see docs/systems/collection-architecture.md 섹션 6.1
```

- **MCP 호출**: 1회

#### Q-A-06: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- C-01~C-04 컴파일 완료 확인 후 Q-B 진행
- **MCP 호출**: 1회 (+ get_console_logs로 에러 확인)

---

### Q-B: 시스템 레이어 스크립트 생성

**목적**: GatheringCatalogManager 싱글턴 MonoBehaviour를 생성한다.

**전제 조건**:
- **Q-A 완료** (GatheringCatalogData, GatheringCatalogEntry, GatheringCatalogSaveData 컴파일 완료)
- ARC-032 G-A 완료 (GatheringRarity, GatheringItemData, GatheringEvents 컴파일 완료)
- EconomyManager, ProgressionManager, TimeManager 컴파일 완료

**예상 MCP 호출**: ~3회 (1 create_script + 1 execute_menu_item + 1 get_console_logs)

#### Q-B-01: GatheringCatalogManager 싱글턴

```
create_script
  path: "Assets/_Project/Scripts/Collection/GatheringCatalogManager.cs"
  namespace: "SeedMind.Collection"
  // MonoBehaviour, Singleton, ISaveable
  // SaveLoadOrder: 56
  //   -> see docs/systems/collection-architecture.md 섹션 5.2 (SaveLoadOrder 할당 근거)
  //   -> see docs/systems/save-load-architecture.md 섹션 7 (전체 할당표)
  // 핵심 메서드: Initialize(), RegisterGather(), GetEntry(), GetCatalogData(),
  //             GetAllEntries(), IsDiscovered(), CheckMilestone()
  //   -> see docs/systems/collection-architecture.md 섹션 5.1
  // 이벤트 구독: GatheringEvents.OnItemGathered (OnEnable/OnDisable)
  //   -> see docs/systems/collection-architecture.md 섹션 7.1
  // 세이브/로드: GetSaveData(), LoadSaveData()
  //   MigrateFromGatheringStats() 포함 (구버전 세이브 호환)
  //   -> see docs/systems/collection-architecture.md 섹션 5.2
  // 마일스톤 목록: -> see docs/systems/collection-system.md 섹션 5.3.2 (canonical)
```

- **MCP 호출**: 1회

#### Q-B-02: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"

get_console_logs
```

- GatheringCatalogManager 컴파일 완료 확인
- **MCP 호출**: 2회

---

### Q-C: SO 에셋 인스턴스 생성 (GatheringCatalogData 27종)

**목적**: 채집 도감 정적 데이터 SO 에셋(GatheringCatalogData) 27종을 생성하고 필드값을 설정한다.

**전제 조건**:
- **Q-A, Q-B 완료** (GatheringCatalogData 클래스 컴파일 완료)
- ARC-032 G-C 완료 (GatheringItemData SO 27종 에셋 생성 완료 — itemId 대응 참조 필요)

**예상 MCP 호출**: ~58회 (2 create_folder + 27종 x 2회 + 2 검증)

#### Q-C-01: 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/GatheringCatalog"
```

- **MCP 호출**: 1회

#### Q-C-02: GatheringCatalogData SO 에셋 (27종)

아이템 목록 canonical: (-> see `docs/content/gathering-items.md`)
희귀도별 보상 수치 canonical: (-> see `docs/systems/collection-system.md 섹션 3.3`)
에셋명 패턴: `GatheringCatalog_<ItemId>` (예: `GatheringCatalog_Dandelion`, `GatheringCatalog_Mushroom`)

각 아이템에 대해:

```
create_scriptable_object
  type: "SeedMind.Collection.GatheringCatalogData"
  asset_path: "Assets/_Project/Data/GatheringCatalog/GatheringCatalog_<ItemId>.asset"

set_property  target: "GatheringCatalog_<ItemId>"
  itemId = "<item_id>"              // (-> see docs/content/gathering-items.md)
  displayName = "<표시명>"          // (-> see docs/content/gathering-items.md)
  hintLocked = "<미발견 힌트>"      // (-> see docs/content/gathering-items.md 아이템별 힌트 텍스트)
  descriptionUnlocked = "<설명>"    // (-> see docs/content/gathering-items.md 아이템별 설명)
  rarity = <GatheringRarity값>      // (-> see docs/content/gathering-items.md)
  firstDiscoverGold = <rarity별 수치>  // (-> see docs/systems/collection-system.md 섹션 3.3 — 희귀도별 수치)
  firstDiscoverXP = <rarity별 수치>    // (-> see docs/systems/collection-system.md 섹션 3.3 — 희귀도별 수치)
  catalogIcon = null                // Sprite 에셋 생성 후 Inspector에서 수동 할당
  sortOrder = 0                     // (-> see docs/content/gathering-items.md 정렬 순서)
```

> **주의**: 모든 구체적 수치는 MCP 실행 시점에 canonical 문서에서 읽어 입력한다. 본 문서에서 수치를 직접 기재하지 않는다 (PATTERN-006). `firstDiscoverGold`, `firstDiscoverXP`는 희귀도(rarity)에 따른 계층형 수치이므로, `collection-system.md 섹션 3.3`의 테이블을 참조하여 해당 아이템의 rarity 값에 맞는 수치를 입력한다.

- **MCP 호출**: 27 x (1 생성 + 1 set_property) = **~54회**
- 에셋 수: 27종 (-> see `docs/systems/collection-system.md` 섹션 5.2)

[RISK] 27종 x 2회 = 54회 MCP 호출은 실행 시간이 길다. Editor 스크립트(`CreateGatheringCatalogAssets.cs`)로 일괄 생성하면 ~2회로 압축 가능.

#### Q-C-03: SO 에셋 생성 검증

```
get_console_logs
  // 27종 에셋 생성 완료 확인
  // GatheringCatalogData.itemId 누락 항목 없음 확인
```

- **MCP 호출**: 1회

**Q-C 합계**: 1 + 54 + 1 = **~56회**

---

### Q-D: 씬 배치 (GatheringCatalogManager)

**목적**: GatheringCatalogManager 싱글턴 오브젝트를 SCN_Farm 씬에 배치하고, GatheringCatalogData SO 배열을 연결한다.

**전제 조건**:
- **Q-A, Q-B, Q-C 완료** (스크립트 컴파일 + SO 에셋 생성 완료)
- SCN_Farm 씬이 열려 있는 상태

**예상 MCP 호출**: ~5회

#### Q-D-01: GatheringCatalogManager 씬 오브젝트 생성

```
create_object
  name: "GatheringCatalogManager"

set_parent
  child: "GatheringCatalogManager"
  parent: "--- MANAGERS ---"

add_component
  target: "GatheringCatalogManager"
  type: "SeedMind.Collection.GatheringCatalogManager"
```

- **MCP 호출**: 3회

#### Q-D-02: GatheringCatalogData 배열 할당

```
set_property  target: "GatheringCatalogManager (GatheringCatalogManager)"
  _catalogDataRegistry = [GatheringCatalog_Dandelion, GatheringCatalog_Mushroom, ...]
  // 27종 GatheringCatalogData SO 배열 할당
  // -> see docs/systems/collection-architecture.md 섹션 5 (_catalogDataRegistry 필드)
```

[RISK] MCP로 SO 배열에 다른 SO 참조를 설정하는 것이 가능한지 미확인. 대안: `GatheringCatalogManager.Initialize()`에서 `Resources.LoadAll<GatheringCatalogData>("Data/GatheringCatalog")`로 자동 로드하거나, Editor 스크립트로 일괄 할당.

- **MCP 호출**: 1회

#### Q-D-03: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

---

### Q-E: 기존 시스템 확장

**목적**: GatheringCatalogManager의 ISaveable 통합을 위해 XPSource enum 확장, GameSaveData 필드 추가, SaveManager 등록 확인을 수행한다.

**전제 조건**:
- **Q-A, Q-B 완료** (GatheringCatalogSaveData 컴파일 완료)
- XPSource enum, GameSaveData 파일이 이미 존재하는 상태

**예상 MCP 호출**: ~5회 (3 edit_script + 1 execute_menu_item + 1 get_console_logs)

#### Q-E-01: XPSource enum 확장

```
edit_script
  path: "Assets/_Project/Scripts/Progression/XPSource.cs"
  // `GatheringCatalog` 값 추가
  //   (최초 채집물 등록 시 XP 보상 소스)
  // -> see docs/systems/collection-architecture.md 섹션 5.1 (RegisterGather 알고리즘)
```

- **MCP 호출**: 1회

#### Q-E-02: GameSaveData 확장

```
edit_script
  path: "Assets/_Project/Scripts/Core/GameSaveData.cs"
  // `public GatheringCatalogSaveData gatheringCatalog;` 필드 추가
  //   (null이면 구버전 세이브로 판단하여 마이그레이션 실행)
  // -> see docs/systems/collection-architecture.md 섹션 5.2 (세이브/로드 흐름)
  // -> see docs/pipeline/data-pipeline.md Part I (GameSaveData 스키마)
```

- **MCP 호출**: 1회

#### Q-E-03: SaveManager ISaveable 등록 확인

```
edit_script
  path: "Assets/_Project/Scripts/Core/SaveManager.cs"
  // GatheringCatalogManager ISaveable 자동 등록 확인
  // SaveLoadOrder = 56이 올바르게 인식되는지 확인
  //   순서: FishCatalogManager(53) -> GatheringManager(54) -> InventoryManager(55) -> GatheringCatalogManager(56)
  // -> see docs/systems/collection-architecture.md 섹션 5.2 (SaveLoadOrder 할당)
  // -> see docs/systems/save-load-architecture.md 섹션 7 (전체 할당표)
```

- **MCP 호출**: 1회

#### Q-E-04: 컴파일 확인

```
execute_menu_item
  menu: "Assets/Refresh"

get_console_logs
  // 컴파일 에러 없음 확인
```

- **MCP 호출**: 2회

---

### Q-F: UI 스크립트 생성 및 씬 계층 구성

**목적**: CollectionUIController, GatheringCatalogUI 관련 스크립트 5종을 생성하고, CollectionPanel 씬 계층을 구성한다.

**전제 조건**:
- **Q-A~Q-E 완료** (모든 데이터/시스템 레이어 스크립트 컴파일 완료)
- ARC-028 완료 (FishCatalogUI, FishCatalogManager 컴파일 완료)
- `Canvas_Overlay` UI 루트가 씬에 존재

**예상 MCP 호출**: ~20회 (5 create_script + 1 execute_menu_item + 씬 계층 ~13회 + 1 save_scene)

#### Q-F-01: UI 스크립트 생성 (5종)

```
create_script
  path: "Assets/_Project/Scripts/Collection/UI/CollectionUIController.cs"
  namespace: "SeedMind.Collection.UI"
  // MonoBehaviour
  // 참조: _fishCatalogManager (FishCatalogManager), _gatheringCatalogManager (GatheringCatalogManager)
  // UI 참조: _tabButtons (Button[2]), _completionHeaderText (TMP_Text),
  //          _fishPanel (FishCatalogUI), _gatheringPanel (GatheringCatalogUI)
  // 프로퍼티: TotalDiscoveredCount, TotalItemCount, OverallCompletionRate
  // 메서드: Open(), Close(), SwitchTab(), RefreshCurrentTab(), UpdateCompletionHeader()
  // 이벤트 구독: FishCatalogManager.OnCatalogUpdated, GatheringCatalogManager.OnCatalogUpdated 등
  // -> see docs/systems/collection-architecture.md 섹션 6.1, 6.2

create_script
  path: "Assets/_Project/Scripts/Collection/UI/GatheringCatalogUI.cs"
  namespace: "SeedMind.Collection.UI"
  // MonoBehaviour
  // 참조: _catalogManager, _scrollRect, _itemPrefab, _contentParent, _detailPanel, _itemPool
  // _categoryFilter: GatheringCategory? (null = 전체)
  // 메서드: Refresh(), SetCategoryFilter(), SelectItem()
  // -> see docs/systems/collection-architecture.md 섹션 6.3

create_script
  path: "Assets/_Project/Scripts/Collection/UI/GatheringCatalogItemUI.cs"
  namespace: "SeedMind.Collection.UI"
  // MonoBehaviour
  // 참조: _icon (Image), _nameText (TMP_Text), _rarityBadge (Image),
  //       _gatheredCountText (TMP_Text), _lockOverlay (GameObject)
  // 메서드: SetData(GatheringCatalogData, GatheringCatalogEntry), OnClick()
  // -> see docs/systems/collection-architecture.md 섹션 6.3

create_script
  path: "Assets/_Project/Scripts/Collection/UI/GatheringCatalogDetailPanel.cs"
  namespace: "SeedMind.Collection.UI"
  // MonoBehaviour
  // 참조: _icon, _nameText, _descriptionText, _rarityText,
  //       _totalGatheredText, _bestQualityText, _firstGatheredText
  // 메서드: ShowItem(GatheringCatalogData, GatheringCatalogEntry), Hide()
  // -> see docs/systems/collection-architecture.md 섹션 6.3

create_script
  path: "Assets/_Project/Scripts/Collection/UI/GatheringCatalogToastUI.cs"
  namespace: "SeedMind.Collection.UI"
  // MonoBehaviour
  // GatheringCatalogManager.OnItemDiscovered 구독
  // 미발견 아이템 최초 채집 시 토스트 표시
  // -> see docs/systems/collection-architecture.md 섹션 6.4 (GatheringCatalogToast 계층)
```

- **MCP 호출**: 5회

#### Q-F-02: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"

get_console_logs
  // C-06~C-10 컴파일 완료 확인
```

- **MCP 호출**: 2회

#### Q-F-03: CollectionPanel 씬 계층 구성

씬 계층 목표 구조: (-> see `docs/systems/collection-architecture.md` 섹션 6.4)

```
// CollectionPanel 루트 생성
create_object
  name: "CollectionPanel"

set_parent
  child: "CollectionPanel"
  parent: "Canvas_Overlay"

add_component
  target: "CollectionPanel"
  type: "SeedMind.Collection.UI.CollectionUIController"

set_property  target: "CollectionPanel"
  // 기본 비활성화 (패널은 Open() 호출 시 활성화)
  activeSelf = false

// Header 구성
create_object name: "Header"
set_parent child: "Header" parent: "CollectionPanel"

create_object name: "TitleText"
set_parent child: "TitleText" parent: "Header"

create_object name: "CompletionText"
set_parent child: "CompletionText" parent: "Header"

create_object name: "CloseButton"
set_parent child: "CloseButton" parent: "Header"

// TabBar 구성
create_object name: "TabBar"
set_parent child: "TabBar" parent: "CollectionPanel"

create_object name: "FishTabButton"
set_parent child: "FishTabButton" parent: "TabBar"

create_object name: "GatheringTabButton"
set_parent child: "GatheringTabButton" parent: "TabBar"
```

- **MCP 호출**: ~13회

#### Q-F-04: GatheringCatalogToast 생성

```
create_object
  name: "GatheringCatalogToast"

set_parent
  child: "GatheringCatalogToast"
  parent: "Canvas_Overlay"

add_component
  target: "GatheringCatalogToast"
  type: "SeedMind.Collection.UI.GatheringCatalogToastUI"

set_property  target: "GatheringCatalogToast"
  activeSelf = false  // 기본 비활성화
```

- **MCP 호출**: 4회

#### Q-F-05: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

**Q-F 합계**: 5 + 2 + 13 + 4 + 1 = **~25회** (단계 상세화 후 실제 수 조정 가능)

---

### Q-G: FishCatalogPanel 씬 마이그레이션 (ARC-039)

**목적**: 기존 독립 FishCatalogPanel을 CollectionPanel/FishPanel로 통합한다. Q-4a~Q-4f 6단계 순서 엄수.

**전제 조건**:
- **Q-F 완료** (CollectionPanel 씬 계층 존재)
- ARC-028 완료 (FishCatalogPanel 프리팹 존재)

**예상 MCP 호출**: ~8회

> **주의**: 이 단계는 기존 FishCatalogUI.cs 코드를 수정하지 않는다. 씬 계층 및 Inspector 참조 재연결만 수행한다. (-> see `docs/systems/collection-architecture.md` 섹션 9 Q-4a~Q-4f)

#### Q-G-01 (Q-4a): FishCatalogPanel → FishPanel 리네이밍

```
rename_object
  target: "FishCatalogPanel"
  new_name: "FishPanel"
```

- **MCP 호출**: 1회

#### Q-G-02 (Q-4b): FishPanel을 CollectionPanel 하위로 이동

```
set_parent
  child: "FishPanel"
  parent: "CollectionPanel"
```

- **MCP 호출**: 1회

#### Q-G-03 (Q-4c): FishPanel 내 CloseButton 비활성화

```
set_property  target: "FishPanel/Header/CloseButton"
  activeSelf = false
  // CollectionPanel/Header/CloseButton이 공통 Close 역할 담당
```

- **MCP 호출**: 1회

#### Q-G-04 (Q-4d): FishPanel 내 완성도 표시 비활성화

```
set_property  target: "FishPanel/Header/CompletionText"
  activeSelf = false
  // CollectionUIController.UpdateCompletionHeader()가 상단 CollectionPanel/Header/CompletionText에 위임
```

- **MCP 호출**: 1회

#### Q-G-05 (Q-4e): FishCatalogUI Inspector 참조 재연결

```
set_property  target: "FishPanel (FishCatalogUI)"
  // FishPanel 기준으로 Inspector 참조 재연결
  // _scrollRect, _itemPrefab, _contentParent, _detailPanel 등 모두 FishPanel 내부 경로로 갱신
  // -> see docs/systems/fishing-architecture.md 섹션 21.1 (FishCatalogUI 참조 필드 목록)
```

- **MCP 호출**: 1회

#### Q-G-06 (Q-4f): 구버전 FishCatalogPanel 처리

```
rename_object
  target: "DEPRECATED_FishCatalogPanel"  // 이미 씬에서 제거되었으면 생략
  // 참조 깨짐 방지: 프리팹 에셋을 Legacy/ 폴더로 이동
  // -> see docs/systems/collection-architecture.md 섹션 9 Q-4f
```

- **MCP 호출**: 1회 (프리팹 이동은 `move_asset` 도구 또는 파일 시스템 이동 후 reimport)

#### Q-G-07: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

**Q-G 합계**: ~7회

---

### Q-H: 통합 검증

**목적**: 수집 도감 시스템의 핵심 기능 8개 항목을 Play Mode에서 검증한다.

**전제 조건**:
- **Q-A~Q-G 전체 완료**
- SCN_Farm 씬이 저장된 상태

**예상 MCP 호출**: ~16회 (1 enter + 8 execute_method/get_console + 1 exit + 검증 간 대기 포함)

#### Q-H-01: GatheringCatalogManager 싱글턴 초기화 확인

```
enter_play_mode

get_console_logs
  // "[GatheringCatalogManager] Initialized: 27 items" 로그 확인
  // _catalogDataRegistry 길이 == 27 확인
```

- **MCP 호출**: 2회

#### Q-H-02: 채집 아이템 최초 등록 (isDiscovered = true)

```
execute_method
  target: "GatheringCatalogManager"
  method: "RegisterGather"
  params: ["gather_dandelion", 0, 1]  // CropQuality.Normal=0, quantity=1
  // gather_dandelion: -> see docs/content/gathering-items.md (테스트용 아이템 ID)

get_console_logs
  // "[GatheringCatalogManager] Discovered: gather_dandelion" 로그 확인
  // OnItemDiscovered 이벤트 발행 확인
```

- **MCP 호출**: 2회

#### Q-H-03: 재채집 시 totalGathered 증가 및 bestQuality 갱신

```
execute_method
  target: "GatheringCatalogManager"
  method: "RegisterGather"
  params: ["gather_dandelion", 2, 3]  // CropQuality.Gold=2, quantity=3

get_console_logs
  // totalGathered = 4 (1+3) 확인
  // bestQuality = 2 (Gold) 확인
  // isNewBestQuality = true 확인
```

- **MCP 호출**: 2회

#### Q-H-04: 미발견 아이템 힌트 텍스트 표시

```
execute_method
  target: "CollectionPanel (CollectionUIController)"
  method: "Open"

execute_method
  target: "CollectionPanel (CollectionUIController)"
  method: "SwitchTab"
  params: [1]  // CollectionTab.Gathering = 1

get_console_logs
  // 미발견 아이템 hintLocked 텍스트 표시 로그 확인
```

- **MCP 호출**: 2회 (Open + SwitchTab 합산)

#### Q-H-05: CollectionUIController 탭 전환

```
execute_method
  target: "CollectionPanel (CollectionUIController)"
  method: "SwitchTab"
  params: [0]  // CollectionTab.Fish = 0

get_console_logs
  // FishPanel 활성화, GatheringPanel 비활성화 로그 확인
```

- **MCP 호출**: 2회

#### Q-H-06: 전체 완성도 집계 정확성

```
execute_method
  target: "CollectionPanel (CollectionUIController)"
  method: "UpdateCompletionHeader"

get_console_logs
  // TotalDiscoveredCount = fish발견수 + gathering발견수 확인
  // TotalItemCount 값: -> see docs/systems/collection-system.md 섹션 5.2 (canonical)
  // OverallCompletionRate % 계산 정확성 확인
```

- **MCP 호출**: 2회

#### Q-H-07: 세이브 -> 로드 후 GatheringCatalogSaveData 복원

```
execute_method
  target: "SaveManager"
  method: "SaveAsync"

execute_method
  target: "SaveManager"
  method: "LoadAsync"

get_console_logs
  // discoveredCount, entries 복원 확인
  // gather_dandelion entry: isDiscovered=true, totalGathered=4, bestQuality=2
```

- **MCP 호출**: 3회

#### Q-H-08: 구버전 세이브 마이그레이션

```
execute_method
  target: "GatheringCatalogManager"
  method: "LoadSaveData"
  params: [null]  // gatheringCatalog=null → MigrateFromGatheringStats 실행

get_console_logs
  // "[GatheringCatalogManager] Migrating from GatheringStats..." 로그 확인
  // 기존 gatheredByItemId 데이터에서 GatheringCatalogEntry 생성 확인
```

- **MCP 호출**: 2회 (+ exit_play_mode 1회)

```
exit_play_mode
```

**Q-H 합계**: 2 + 2 + 2 + 2 + 2 + 2 + 3 + 2 + 1 = **~18회**

---

## 5. 의존 관계 다이어그램

```
Q-A (데이터 레이어 스크립트)
  |
  +── Q-B (GatheringCatalogManager)
  |     |
  |     +── Q-C (SO 에셋 27종) ──── Q-D (씬 배치)
  |     |                               |
  |     +── Q-E (기존 시스템 확장)      |
  |                                    |
  +── Q-F (UI 스크립트 + 씬 계층) ────+── Q-G (씬 마이그레이션)
                                            |
                                            v
                                       Q-H (통합 검증)
```

**병렬 실행 가능**:
- Q-C와 Q-E는 Q-B 완료 후 병렬 실행 가능
- Q-F는 Q-A 완료 직후 시작 가능 (Q-B와 병렬), 단 씬 계층 배치는 Q-B 완료 필요

---

## Cross-references

| 문서 | 관련 섹션 | 연관 |
|------|----------|------|
| `docs/systems/collection-architecture.md` (ARC-037) | 섹션 2~9 전체 | 본 문서의 상위 설계 문서. 클래스 다이어그램, 메서드 명세, 씬 계층, 세이브/로드, Phase M~R 원본 |
| `docs/systems/collection-system.md` (DES-018) | 섹션 3.3, 5.2, 5.3.2 | 희귀도별 보상 수치, 채집 도감 아이템 수, 마일스톤 보상 상세 — canonical |
| `docs/systems/fishing-architecture.md` (ARC-026/ARC-030) | 섹션 21.5, ARC-039 마이그레이션 노트 | FishCatalogPanel → FishPanel 마이그레이션 원본 기록 |
| `docs/systems/gathering-architecture.md` (ARC-031) | 섹션 2.2, 5.1, 5.2 | GatheringItemData, GatheringRarity, GatheringEvents.OnItemGathered — 이벤트 소스 |
| `docs/content/gathering-items.md` (CON-012) | 섹션 3~7 | 채집 아이템 힌트 텍스트, 희귀도, itemId — canonical |
| `docs/systems/save-load-architecture.md` (ARC-011) | 섹션 7 | SaveLoadOrder 할당표 (GatheringCatalogManager=56 반영 완료 — FIX-093 적용됨) |
| `docs/pipeline/data-pipeline.md` | Part I (GameSaveData 스키마) | GameSaveData에 `gatheringCatalog` 필드 추가 대상 (Q-E-02) |
| `docs/mcp/gathering-tasks.md` (ARC-032) | G-A~G-G 전체 | 채집 시스템 MCP 태스크 — 본 문서의 전제 조건, 패턴 참조 |
| `docs/mcp/fishing-tasks.md` (ARC-028) | F-1~F-8 전체 | 낚시 시스템 MCP 태스크 — FishCatalogPanel 생성 출처 |
| `docs/systems/project-structure.md` | 섹션 2, 3, 4 | 네임스페이스 규칙, asmdef 구조 |

---

## Open Questions

1. [OPEN] (ARC-041) **GatheringCatalogData와 GatheringItemData SO 참조 방식**: 두 SO가 itemId 문자열로 연결되어 있다. GatheringCatalogData에 `public GatheringItemData itemData` 직접 참조 필드를 추가하면 런타임 조회 비용을 줄일 수 있으나, SO 간 순환 참조 위험이 있다. 구현 시 결정. (ARC-042 추적)

2. [OPEN] (ARC-041) **Q-F-03 CollectionPanel 씬 계층의 GatheringPanel 배치**: Q-G에서 FishPanel 마이그레이션 후 CollectionPanel에 GatheringPanel을 추가해야 한다. GatheringCatalogUI 프리팹을 별도로 만들거나 씬 계층에 직접 구성할지 결정 필요. 현재 문서는 씬 직접 구성 방식을 가정한다.

3. [OPEN] (ARC-041) **Q-G-06 Legacy 폴더 이동**: `move_asset` MCP 도구 가용 여부 미확인. 파일 시스템 이동 후 Unity reimport가 필요할 수 있다.

---

## Risks

1. [RISK] **(ARC-041) MCP SO 배열 참조 설정 미확인**: Q-D-02에서 GatheringCatalogManager의 `_catalogDataRegistry` 배열에 27종 GatheringCatalogData SO를 할당하는 것이 MCP로 가능한지 미확인. 대안: `GatheringCatalogManager.Initialize()`에서 `Resources.LoadAll<GatheringCatalogData>("Data/GatheringCatalog")`로 자동 로드. (-> see `docs/systems/collection-architecture.md` Risks 2)

2. [RISK] **(ARC-041) Q-G FishPanel Inspector 참조 깨짐**: FishPanel을 CollectionPanel 하위로 이동하면 기존 FishCatalogUI.cs의 Inspector 참조(ScrollRect, ItemPrefab 등)가 깨질 수 있다. Q-G-05에서 반드시 재연결 확인 필요. (-> see `docs/systems/collection-architecture.md` Open Question 4 해소 내역)

3. [RISK] **(ARC-041) SaveLoadOrder 56 밀집**: FishCatalogManager(53), GatheringManager(54), InventoryManager(55), GatheringCatalogManager(56) 연속 배치. BuildingManager(60)까지 3칸 여유. 향후 도감 시스템 확장 시 간격 부족 가능. (-> see `docs/systems/collection-architecture.md` Risks 1)

4. [RISK] **(ARC-041) Q-C Editor 스크립트 우회 시 itemId 오타**: 27종 SO 에셋 일괄 생성 시 itemId 문자열이 GatheringItemData.dataId와 불일치하면 런타임에 도감 항목이 누락된다. 생성 후 Q-H-01에서 27종 전체 itemId 일치 여부 검증 필수. (-> see `docs/systems/collection-architecture.md` Risks 2)
