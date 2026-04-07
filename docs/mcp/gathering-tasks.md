# 채집 시스템 MCP 태스크 시퀀스 (ARC-032)

> 작성: Claude Code (Sonnet 4.6) | 2026-04-07 | 문서 ID: ARC-032 | Phase 1

---

## Context

이 문서는 채집(Gathering) 시스템의 Unity MCP 구현 태스크 시퀀스를 정의한다. `docs/systems/gathering-architecture.md`(ARC-031) Part III에서 요약된 Phase A~G 계획을 독립 문서로 분리하여, 스크립트 생성 → SO 에셋 → 씬 배치 → 기존 시스템 확장 → UI 연동 → 통합 검증까지의 전체 MCP 호출 시퀀스를 상세히 기술한다.

**상위 설계 문서**: `docs/systems/gathering-architecture.md` (ARC-031)
**패턴 참조**: `docs/mcp/facilities-tasks.md` (ARC-007), `docs/mcp/fishing-tasks.md` (ARC-028)

---

## 1. 개요

### 목적

이 문서는 `docs/systems/gathering-architecture.md`(ARC-031) Part III에서 요약된 MCP 구현 계획(Phase A~G)을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 채집 시스템의 데이터 레이어(SO 에셋), 시스템 레이어(스크립트), 씬 배치, 기존 시스템 확장, UI 연동, 통합 검증을 완성한다.

### 의존성

```
채집 시스템 MCP 태스크 의존 관계:
├── SeedMind.Core        (TimeManager, SaveManager, EventBus, GameDataSO)
├── SeedMind.Farm        (FarmZoneManager -- Zone 해금 상태 참조)
├── SeedMind.Player      (InventoryManager -- TryAddItem() 채집물 추가)
├── SeedMind.Economy     (EconomyManager -- HarvestOrigin.Gathering 가격 보정)
├── SeedMind.Progression (ProgressionManager -- XPSource.GatheringComplete)
├── SeedMind.Fishing     (FishingProficiency -- 동일 패턴 참조)
└── SeedMind.UI          (채집 프롬프트, 결과 팝업, 숙련도 토스트)
```

(-> see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 완료된 태스크 의존성

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조, SCN_Farm 기본 계층 (Managers, Farm, Player, UI) |
| ARC-003 | `docs/mcp/farming-tasks.md` | Phase A~C 전체 | FarmGrid 타일, CropData SO, GrowthSystem |
| ARC-010 | `docs/mcp/fishing-tasks.md` | Phase A~E 전체 | FishingProficiency 패턴 (동일 구조 참조), HarvestOrigin enum |
| ARC-009 | `docs/mcp/progression-tasks.md` | Phase A 이상 | ProgressionManager, XPSource enum |
| ARC-011 | `docs/mcp/inventory-tasks.md` | Phase A 이상 | InventoryManager, ItemType enum |
| ARC-008 | `docs/mcp/save-load-tasks.md` | Phase A 이상 | SaveManager, ISaveable, GameSaveData |

### 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `--- FARM ---` (씬 계층 부모) | ARC-002 Phase B |
| `Canvas_Overlay` (UI 루트) | ARC-002 Phase B |
| `DataRegistry` (SO 로드 시스템) | `docs/pipeline/data-pipeline.md` Part II |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A |
| `ProgressionManager` (씬 오브젝트) | ARC-009 |
| `InventoryManager` (씬 오브젝트) | ARC-011 |
| `SaveManager` (씬 오브젝트) | ARC-008 |
| `HarvestOrigin` enum | ARC-010 (낚시에서 추가) |
| `XPSource` enum | ARC-009 |
| `ItemType` enum | ARC-011 |

### 총 MCP 호출 예상 수

| 태스크 | 호출 수 | 복잡도 |
|--------|--------|--------|
| G-A: 데이터 레이어 스크립트 생성 (14종) | ~16회 | 중 |
| G-B: 시스템 레이어 스크립트 생성 (2종) | ~4회 | 중 |
| G-C: SO 에셋 인스턴스 생성 | ~40회 | 고 (아이템/포인트 수에 비례) |
| G-D: 씬 배치 (GatheringManager, GatheringPoint) | ~20회 | 중 |
| G-E: 기존 시스템 확장 (enum/switch 수정) | ~14회 | 중 |
| G-F: UI 연동 (프롬프트, 팝업, 토스트) | ~18회 | 중 |
| G-G: 통합 검증 (12개 체크) | ~24회 | 저 |
| **합계** | **~136회** | |

[RISK] SO 에셋 인스턴스 생성(G-C)의 호출 수는 아이템/포인트 종류에 비례한다. Editor 스크립트(`CreateGatheringAssets.cs`)를 통한 일괄 생성으로 G-C의 ~40회를 ~5회로 압축 가능. MCP 단독 실행 시 시간 비용이 크므로 Editor 스크립트 우회를 권장한다.

---

## 2. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋 폴더 생성 | G-A, G-C |
| `create_script` | C# 스크립트 파일 생성 | G-A, G-B |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | G-C |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | G-C, G-D, G-F |
| `create_object` | 빈 GameObject 생성 | G-D, G-F |
| `add_component` | MonoBehaviour 컴포넌트 부착 | G-D, G-F |
| `set_parent` | 오브젝트 부모 설정 | G-D, G-F |
| `edit_script` | 기존 스크립트 수정 (enum/switch 확장) | G-E |
| `save_scene` | 씬 저장 | G-D, G-F, G-G |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | G-G |
| `execute_method` | 런타임 메서드 호출 (테스트) | G-G |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | G-G |
| `execute_menu_item` | 편집기 명령 실행 (컴파일 대기 등) | G-A, G-B |

[RISK] `create_scriptable_object` 도구의 가용 여부 및 파라미터 형식 사전 검증 필요. SO 인스턴스 생성이 MCP에서 미지원인 경우, Editor 스크립트를 통한 우회 필요. (-> see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)

---

## 3. 필요 C# 스크립트 목록

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 태스크 순서대로 작성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/Gathering/Data/GatheringCategory.cs` | `GatheringCategory` (enum) | `SeedMind.Gathering` | G-A |
| S-02 | `Scripts/Gathering/Data/GatheringRarity.cs` | `GatheringRarity` (enum) | `SeedMind.Gathering` | G-A |
| S-03 | `Scripts/Gathering/Data/GatheringItemData.cs` | `GatheringItemData` (SO) | `SeedMind.Gathering` | G-A |
| S-04 | `Scripts/Gathering/Data/GatheringPointData.cs` | `GatheringPointData` (SO) | `SeedMind.Gathering` | G-A |
| S-05 | `Scripts/Gathering/Data/GatheringConfig.cs` | `GatheringConfig` (SO) | `SeedMind.Gathering` | G-A |
| S-06 | `Scripts/Gathering/Data/GatheringItemEntry.cs` | `GatheringItemEntry` (struct) | `SeedMind.Gathering` | G-A |
| S-07 | `Scripts/Gathering/Data/SeasonalItemOverride.cs` | `SeasonalItemOverride` (class) | `SeedMind.Gathering` | G-A |
| S-08 | `Scripts/Gathering/GatheringStats.cs` | `GatheringStats` (Plain C#) | `SeedMind.Gathering` | G-A |
| S-09 | `Scripts/Gathering/GatheringPointState.cs` | `GatheringPointState` (Plain C#) | `SeedMind.Gathering` | G-A |
| S-10 | `Scripts/Gathering/GatherResult.cs` | `GatherResult` (struct) | `SeedMind.Gathering` | G-A |
| S-11 | `Scripts/Gathering/GatheringSaveData.cs` | `GatheringSaveData` (class) | `SeedMind.Gathering` | G-A |
| S-12 | `Scripts/Gathering/GatheringPointStateSaveData.cs` | `GatheringPointStateSaveData` (class) | `SeedMind.Gathering` | G-A |
| S-13 | `Scripts/Gathering/GatheringEvents.cs` | `GatheringEvents` (static) | `SeedMind.Gathering` | G-A |
| S-14 | `Scripts/Gathering/GatheringProficiency.cs` | `GatheringProficiency` (Plain C#) | `SeedMind.Gathering` | G-A |
| S-15 | `Scripts/Gathering/GatheringPoint.cs` | `GatheringPoint` (MonoBehaviour) | `SeedMind.Gathering` | G-B |
| S-16 | `Scripts/Gathering/GatheringManager.cs` | `GatheringManager` (MonoBehaviour) | `SeedMind.Gathering` | G-B |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: S-01~S-07 -> S-08~S-14 -> S-15 -> S-16. 각 그룹 사이에 Unity 컴파일 대기(`execute_menu_item`)가 필요하다.

---

## 4. 태스크 목록

---

### G-A: 데이터 레이어 스크립트 생성 (Phase A)

**목적**: 채집 시스템의 데이터 정의 클래스를 생성한다. enum, ScriptableObject, 직렬화 가능 struct/class, Plain C# 클래스 총 14개 파일.

**전제 조건**:
- ARC-002 완료 (폴더 구조 존재)
- `SeedMind.Core` 네임스페이스의 `GameDataSO`, `ISaveable`, `Season` enum 컴파일 완료

**예상 MCP 호출**: ~16회 (14 create_script + 1 create_folder + 1 execute_menu_item)

#### G-A-01: 스크립트 폴더 생성

```
create_folder
  path: "Assets/_Project/Scripts/Gathering"

create_folder
  path: "Assets/_Project/Scripts/Gathering/Data"
```

- **MCP 호출**: 2회

#### G-A-02: enum 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/Gathering/Data/GatheringCategory.cs"
  namespace: "SeedMind.Gathering"
  // enum 값: -> see docs/systems/gathering-system.md 섹션 2

create_script
  path: "Assets/_Project/Scripts/Gathering/Data/GatheringRarity.cs"
  namespace: "SeedMind.Gathering"
  // enum 값: Common, Uncommon, Rare, Legendary
  // -> see docs/systems/gathering-system.md 섹션 2
```

- **MCP 호출**: 2회

#### G-A-03: SO 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/Gathering/Data/GatheringItemData.cs"
  namespace: "SeedMind.Gathering"
  // GameDataSO 상속, IInventoryItem 구현
  // 필드: -> see docs/systems/gathering-architecture.md 섹션 2.2

create_script
  path: "Assets/_Project/Scripts/Gathering/Data/GatheringPointData.cs"
  namespace: "SeedMind.Gathering"
  // ScriptableObject 상속
  // 필드: -> see docs/systems/gathering-architecture.md 섹션 2.1

create_script
  path: "Assets/_Project/Scripts/Gathering/Data/GatheringConfig.cs"
  namespace: "SeedMind.Gathering"
  // ScriptableObject 상속
  // 필드: -> see docs/systems/gathering-architecture.md 섹션 2.3
```

- **MCP 호출**: 3회

#### G-A-04: 직렬화 가능 struct/class 생성

```
create_script
  path: "Assets/_Project/Scripts/Gathering/Data/GatheringItemEntry.cs"
  namespace: "SeedMind.Gathering"
  // [System.Serializable] struct: item (GatheringItemData) + weight (float)

create_script
  path: "Assets/_Project/Scripts/Gathering/Data/SeasonalItemOverride.cs"
  namespace: "SeedMind.Gathering"
  // [System.Serializable] class: season (Season) + overrideItems (GatheringItemEntry[])
```

- **MCP 호출**: 2회

#### G-A-05: Plain C# 클래스 생성

```
create_script
  path: "Assets/_Project/Scripts/Gathering/GatheringStats.cs"
  namespace: "SeedMind.Gathering"
  // 필드: -> see docs/systems/gathering-architecture.md 섹션 1 (시스템 다이어그램)

create_script
  path: "Assets/_Project/Scripts/Gathering/GatheringPointState.cs"
  namespace: "SeedMind.Gathering"
  // 필드: -> see docs/systems/gathering-architecture.md 섹션 1 (시스템 다이어그램)

create_script
  path: "Assets/_Project/Scripts/Gathering/GatherResult.cs"
  namespace: "SeedMind.Gathering"
  // struct: success, item, quality, quantity, bonusTriggered
  // -> see docs/systems/gathering-architecture.md 섹션 1

create_script
  path: "Assets/_Project/Scripts/Gathering/GatheringSaveData.cs"
  namespace: "SeedMind.Gathering"
  // 필드: -> see docs/systems/gathering-architecture.md 섹션 7

create_script
  path: "Assets/_Project/Scripts/Gathering/GatheringPointStateSaveData.cs"
  namespace: "SeedMind.Gathering"
  // 필드: -> see docs/systems/gathering-architecture.md 섹션 7

create_script
  path: "Assets/_Project/Scripts/Gathering/GatheringEvents.cs"
  namespace: "SeedMind.Gathering"
  // static class: OnItemGathered, OnPointDepleted, OnPointRespawned, OnProficiencyLevelUp

create_script
  path: "Assets/_Project/Scripts/Gathering/GatheringProficiency.cs"
  namespace: "SeedMind.Gathering"
  // Plain C# 클래스, FishingProficiency와 동일 패턴
  // XP 테이블: -> see docs/systems/gathering-system.md 섹션 4
```

- **MCP 호출**: 7회

#### G-A-06: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- 모든 G-A 스크립트 컴파일 완료 확인 후 G-B 진행
- **MCP 호출**: 1회 (+ get_console_logs로 에러 확인)

---

### G-B: 시스템 레이어 스크립트 생성 (Phase B)

**목적**: GatheringPoint MonoBehaviour와 GatheringManager 싱글턴 MonoBehaviour를 생성한다.

**전제 조건**:
- **G-A 완료** (모든 데이터 클래스 컴파일 완료)
- TimeManager, InventoryManager, ProgressionManager가 컴파일 완료 상태

**예상 MCP 호출**: ~4회 (2 create_script + 1 execute_menu_item + 1 get_console_logs)

#### G-B-01: GatheringPoint 컴포넌트

```
create_script
  path: "Assets/_Project/Scripts/Gathering/GatheringPoint.cs"
  namespace: "SeedMind.Gathering"
  // MonoBehaviour
  // SerializeField: _pointData (GatheringPointData)
  // 프로퍼티: PointData, PointId
  // 인터랙션 트리거 영역 (Collider2D)
  // -> see docs/systems/gathering-architecture.md 섹션 3
```

- **MCP 호출**: 1회

#### G-B-02: GatheringManager 싱글턴

```
create_script
  path: "Assets/_Project/Scripts/Gathering/GatheringManager.cs"
  namespace: "SeedMind.Gathering"
  // MonoBehaviour, Singleton, ISaveable
  // 필드/메서드: -> see docs/systems/gathering-architecture.md 섹션 1 (시스템 다이어그램)
  // SaveLoadOrder: 54
  // TimeManager 이벤트 구독 priority: 56
  // -> see docs/systems/gathering-architecture.md 섹션 5, 6, 7
```

- **MCP 호출**: 1회

#### G-B-03: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"

get_console_logs
```

- GatheringPoint, GatheringManager 컴파일 완료 확인
- **MCP 호출**: 2회

---

### G-C: SO 에셋 인스턴스 생성 (Phase C)

**목적**: GatheringConfig, GatheringItemData, GatheringPointData, PriceData SO 에셋 인스턴스를 생성하고 필드값을 설정한다.

**전제 조건**:
- **G-A, G-B 완료** (SO 클래스 및 Manager 컴파일 완료)

**예상 MCP 호출**: ~40회 (아이템/포인트 수에 비례)

#### G-C-01: 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Gathering"

create_folder
  path: "Assets/_Project/Data/Gathering/Items"

create_folder
  path: "Assets/_Project/Data/Gathering/Points"

create_folder
  path: "Assets/_Project/Data/Gathering/Prices"
```

- **MCP 호출**: 4회

#### G-C-02: GatheringConfig SO 에셋

```
create_scriptable_object
  type: "SeedMind.Gathering.GatheringConfig"
  asset_path: "Assets/_Project/Data/Gathering/SO_GatheringConfig_Default.asset"

set_property  target: "SO_GatheringConfig_Default"
  respawnDaysRange = { x: 0, y: 0 }   // (-> see docs/systems/gathering-system.md 섹션 2)
  baseGatherEnergy = 0                 // (-> see docs/systems/gathering-system.md 섹션 2)
  maxActivePoints = 0                  // (-> see docs/systems/gathering-system.md 섹션 2)
  seasonalRefreshOnChange = true       // (-> see docs/systems/gathering-system.md 섹션 3)
  qualityThresholds = []               // (-> see docs/systems/gathering-system.md 섹션 4.5)
  // proficiency 관련 필드: (-> see docs/systems/gathering-system.md 섹션 4)
```

> **주의**: 모든 구체적 수치는 MCP 실행 시점에 canonical 문서에서 읽어 입력한다. 본 문서에서 수치를 직접 기재하지 않는다 (PATTERN-006).

- **MCP 호출**: 1(생성) + 6(필드 설정) = ~7회

#### G-C-03: GatheringItemData SO 에셋 (아이템별)

아이템 목록 및 각 필드값은 canonical 문서를 참조한다.
(-> see `docs/systems/gathering-system.md` 섹션 2 for 전체 아이템 목록)
(-> see `docs/content/gathering-items.md` for 아이템별 상세 파라미터)

각 아이템에 대해:
```
create_scriptable_object
  type: "SeedMind.Gathering.GatheringItemData"
  asset_path: "Assets/_Project/Data/Gathering/Items/SO_GItem_<ItemId>.asset"

set_property  target: "SO_GItem_<ItemId>"
  dataId = "<item_id>"                  // (-> see docs/systems/gathering-system.md)
  displayName = "<이름>"                 // (-> see docs/systems/gathering-system.md)
  category = <int>                      // (-> see docs/systems/gathering-system.md 섹션 2)
  rarity = <int>                        // (-> see docs/systems/gathering-system.md 섹션 2)
  basePrice = 0                         // (-> see docs/balance/economy-balance.md)
  gatherEnergy = 0                      // (-> see docs/systems/gathering-system.md 섹션 2)
  requiredTool = <int>                  // (-> see docs/systems/gathering-system.md 섹션 2)
  availableSeasons = []                 // (-> see docs/systems/gathering-system.md 섹션 3)
  weatherBonus = 0.0                    // (-> see docs/systems/gathering-system.md 섹션 3)
```

- **MCP 호출**: 아이템 수 x (1 생성 + ~8 필드설정) = 아이템 수 x ~9회
- 예상 아이템 수: (-> see `docs/systems/gathering-system.md` 섹션 2)

#### G-C-04: GatheringPointData SO 에셋 (포인트별)

포인트 목록 및 각 필드값은 canonical 문서를 참조한다.
(-> see `docs/systems/gathering-system.md` 섹션 3 for 포인트 배치)

각 포인트에 대해:
```
create_scriptable_object
  type: "SeedMind.Gathering.GatheringPointData"
  asset_path: "Assets/_Project/Data/Gathering/Points/SO_GPoint_<PointId>.asset"

set_property  target: "SO_GPoint_<PointId>"
  pointId = "<point_id>"               // (-> see docs/systems/gathering-system.md 섹션 3)
  displayName = "<포인트명>"             // (-> see docs/systems/gathering-system.md 섹션 3)
  zoneId = "<zone_id>"                 // (-> see docs/systems/gathering-system.md 섹션 3)
  requiredZoneUnlocked = true
  respawnDays = 0                      // (-> see docs/systems/gathering-system.md 섹션 2)
  respawnVariance = 0                  // (-> see docs/systems/gathering-system.md 섹션 2)
  availableItems = []                  // SO 배열 참조 -- 아래 주의 참조
```

> **주의**: `availableItems` (GatheringItemEntry[])와 `seasonOverrides` (SeasonalItemOverride[])는 SO 배열 참조 필드이다. MCP로 SO 배열/참조 설정이 불가능할 경우, `GatheringPoint.Awake()`에서 `Resources.LoadAll<GatheringItemData>()` 패턴으로 우회한다.

[RISK] MCP로 SO 배열에 다른 SO 참조를 설정하는 것이 가능한지 미확인. 대안: Editor 스크립트로 일괄 할당 또는 런타임 자동 로드.

- **MCP 호출**: 포인트 수 x (1 생성 + ~6 필드설정) = 포인트 수 x ~7회

#### G-C-05: PriceData SO 에셋 (채집물별)

```
create_scriptable_object
  type: "SeedMind.Economy.PriceData"
  asset_path: "Assets/_Project/Data/Gathering/Prices/SO_Price_Gather_<ItemId>.asset"

set_property  target: "SO_Price_Gather_<ItemId>"
  // 가격 필드: (-> see docs/balance/economy-balance.md, docs/systems/economy-system.md)
```

- **MCP 호출**: 아이템 수 x ~3회

---

### G-D: 씬 배치 (Phase D)

**목적**: GatheringManager 싱글턴 오브젝트와 각 Zone별 GatheringPoint 오브젝트를 씬에 배치한다.

**전제 조건**:
- **G-A, G-B, G-C 완료** (스크립트 컴파일 + SO 에셋 생성 완료)
- SCN_Farm 씬이 열려 있는 상태

**예상 MCP 호출**: ~20회

#### G-D-01: GatheringManager 씬 오브젝트 생성

```
create_object
  name: "GatheringManager"

set_parent
  child: "GatheringManager"
  parent: "--- MANAGERS ---"

add_component
  target: "GatheringManager"
  type: "SeedMind.Gathering.GatheringManager"

set_property  target: "GatheringManager (GatheringManager)"
  _gatheringConfig = SO_GatheringConfig_Default   // SO 참조
```

- **MCP 호출**: 4회

[RISK] MCP로 SerializeField의 SO 참조 설정이 가능한지 미확인. 대안: GatheringManager.Awake()에서 `Resources.Load<GatheringConfig>("Data/Gathering/SO_GatheringConfig_Default")`로 자동 로드.

#### G-D-02: Zone D(숲) GatheringPoint 배치

Zone D 해금 후 활성화되는 채집 포인트 N개를 배치한다.
(-> see `docs/systems/gathering-system.md` 섹션 3 for 포인트 배치 위치/개수)

각 포인트에 대해:
```
create_object
  name: "GatheringPoint_<PointId>"

set_parent
  child: "GatheringPoint_<PointId>"
  parent: "--- FARM ---/Zone_D"

add_component
  target: "GatheringPoint_<PointId>"
  type: "SeedMind.Gathering.GatheringPoint"

set_property  target: "GatheringPoint_<PointId> (Transform)"
  localPosition = { x: 0, y: 0, z: 0 }   // (-> see docs/systems/gathering-system.md 섹션 3)

set_property  target: "GatheringPoint_<PointId> (GatheringPoint)"
  _pointData = SO_GPoint_<PointId>         // SO 참조
```

- **MCP 호출**: 포인트 수 x 5회

#### G-D-03: Zone B/C(평야) GatheringPoint 배치

평야 Zone에 소수의 채집 포인트를 배치한다.
(-> see `docs/systems/gathering-system.md` 섹션 3 for 배치 세부)

- 절차는 G-D-02와 동일, `parent` Zone만 변경

#### G-D-04: GatheringManager에 GatheringPoint 배열 할당

```
set_property  target: "GatheringManager (GatheringManager)"
  _gatheringPoints = [GatheringPoint_01, GatheringPoint_02, ...]  // 씬 참조 배열
```

[RISK] MCP로 씬 오브젝트 배열 참조 설정이 불가능할 수 있다. 대안: `GatheringManager.Awake()`에서 `FindObjectsOfType<GatheringPoint>()`로 자동 수집.

- **MCP 호출**: 1회

#### G-D-05: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

---

### G-E: 기존 시스템 확장 (Phase E)

**목적**: 채집 시스템이 기존 시스템과 연동되도록 enum 값 추가, switch 문 확장, 이벤트 구독 추가 등을 수행한다.

**전제 조건**:
- **G-A, G-B 완료** (채집 시스템 스크립트 존재)
- HarvestOrigin, XPSource, ItemType enum이 이미 정의되어 있는 상태

**예상 MCP 호출**: ~14회 (7 edit_script + 7 set_property/검증)

#### G-E-01: HarvestOrigin enum 확장

```
edit_script
  path: "Assets/_Project/Scripts/Economy/HarvestOrigin.cs"
  // `Gathering = 4` 값 추가
  // -> see docs/systems/gathering-architecture.md 섹션 5.3
```

- **MCP 호출**: 1회

#### G-E-02: XPSource enum 확장

```
edit_script
  path: "Assets/_Project/Scripts/Progression/XPSource.cs"
  // `GatheringComplete` 값 추가
  // -> see docs/systems/gathering-architecture.md 섹션 5.2
```

- **MCP 호출**: 1회

#### G-E-03: ProgressionManager switch 확장

```
edit_script
  path: "Assets/_Project/Scripts/Progression/ProgressionManager.cs"
  // GetExpForSource() switch에 GatheringComplete case 추가
  // XP 양: (-> see docs/systems/gathering-system.md 섹션 4)
  // GatheringEvents.OnItemGathered 구독 추가
  // -> see docs/systems/gathering-architecture.md 섹션 5.2
```

- **MCP 호출**: 1회

#### G-E-04: ItemType enum 확장

```
edit_script
  path: "Assets/_Project/Scripts/Player/ItemType.cs"
  // `Gathered` 값 추가
  // -> see docs/systems/gathering-architecture.md 섹션 5.4
```

- **MCP 호출**: 1회

#### G-E-05: GameSaveData 확장

```
edit_script
  path: "Assets/_Project/Scripts/Core/GameSaveData.cs"
  // `public GatheringSaveData gathering;` 필드 추가
  // -> see docs/systems/gathering-architecture.md 섹션 7
```

- **MCP 호출**: 1회

#### G-E-06: SaveManager ISaveable 등록 확인

```
edit_script
  path: "Assets/_Project/Scripts/Core/SaveManager.cs"
  // GatheringManager ISaveable 자동 등록 확인
  // SaveLoadOrder = 54가 올바르게 인식되는지 확인
  // -> see docs/systems/gathering-architecture.md 섹션 7
```

- **MCP 호출**: 1회

#### G-E-07: EconomyManager GetGreenhouseMultiplier 확장

```
edit_script
  path: "Assets/_Project/Scripts/Economy/EconomyManager.cs"
  // GetGreenhouseMultiplier() switch에 `HarvestOrigin.Gathering` case 추가 (return 1.0)
  // -> see docs/systems/gathering-architecture.md Phase E
```

- **MCP 호출**: 1회

#### G-E-08: 컴파일 확인

```
execute_menu_item
  menu: "Assets/Refresh"

get_console_logs
  // 컴파일 에러 없음 확인
```

- **MCP 호출**: 2회

---

### G-F: UI 연동 (Phase F)

**목적**: 채집 인터랙션 프롬프트, 결과 팝업, 숙련도 레벨업 토스트를 구현한다.

**전제 조건**:
- **G-A~G-E 완료**
- `Canvas_Overlay` UI 루트가 씬에 존재

**예상 MCP 호출**: ~18회

#### G-F-01: 채집 인터랙션 프롬프트 UI

```
create_object
  name: "Panel_GatherPrompt"

set_parent
  child: "Panel_GatherPrompt"
  parent: "Canvas_Overlay"

// TextMeshPro 텍스트 ("E키로 채집" 등)
create_object  name: "Text_GatherPrompt"
set_parent     child: "Text_GatherPrompt"  parent: "Panel_GatherPrompt"
add_component  target: "Text_GatherPrompt"  type: "TMPro.TextMeshProUGUI"

set_property   target: "Text_GatherPrompt (TextMeshProUGUI)"
  text = "E키로 채집"
  fontSize = 0      // (-> see docs/systems/ui-system.md for UI 스타일 가이드)

set_property   target: "Panel_GatherPrompt"
  active = false    // 기본 비활성
```

- **MCP 호출**: ~6회

#### G-F-02: 채집 결과 팝업

```
create_object
  name: "Panel_GatherResult"

set_parent
  child: "Panel_GatherResult"
  parent: "Canvas_Overlay"

// 아이콘, 아이템명, 수량, 품질 표시 영역
create_object  name: "Image_ItemIcon"
set_parent     child: "Image_ItemIcon"  parent: "Panel_GatherResult"
add_component  target: "Image_ItemIcon"  type: "UnityEngine.UI.Image"

create_object  name: "Text_ItemName"
set_parent     child: "Text_ItemName"  parent: "Panel_GatherResult"
add_component  target: "Text_ItemName"  type: "TMPro.TextMeshProUGUI"

create_object  name: "Text_Quantity"
set_parent     child: "Text_Quantity"  parent: "Panel_GatherResult"
add_component  target: "Text_Quantity"  type: "TMPro.TextMeshProUGUI"

set_property   target: "Panel_GatherResult"
  active = false
```

- **MCP 호출**: ~8회

#### G-F-03: 숙련도 레벨업 토스트

```
// FishingProficiency 토스트와 동일 패턴
// -> see docs/mcp/fishing-tasks.md의 UI 토스트 섹션

create_object  name: "Panel_GatherLevelUp"
set_parent     child: "Panel_GatherLevelUp"  parent: "Canvas_Overlay"

create_object  name: "Text_LevelUpMsg"
set_parent     child: "Text_LevelUpMsg"  parent: "Panel_GatherLevelUp"
add_component  target: "Text_LevelUpMsg"  type: "TMPro.TextMeshProUGUI"

set_property   target: "Panel_GatherLevelUp"
  active = false
```

- **MCP 호출**: ~4회

---

### G-G: 통합 검증 (Phase G)

**목적**: 채집 시스템의 전체 흐름을 PlayMode에서 검증한다.

**전제 조건**:
- **G-A~G-F 모두 완료**
- SCN_Farm 씬 저장 완료

**예상 MCP 호출**: ~24회 (enter_play_mode, execute_method, get_console_logs 반복)

#### G-G-01 ~ G-G-12: 검증 항목

| Step | 검증 내용 | MCP 방법 |
|------|----------|----------|
| G-G-01 | GatheringManager 싱글턴 정상 생성 확인 | `enter_play_mode` -> `get_console_logs` (초기화 로그 확인) |
| G-G-02 | Zone D 해금 -> GatheringPoint 활성화 확인 | `execute_method` (Zone 해금 트리거) -> `get_console_logs` |
| G-G-03 | 채집 인터랙션 -> 아이템 획득 -> 인벤토리 추가 흐름 확인 | `execute_method` (TryGather 호출) -> `get_console_logs` |
| G-G-04 | 포인트 비활성화 -> 재생성 타이머 -> 재활성화 흐름 확인 | `execute_method` (DayChanged 시뮬레이션) -> `get_console_logs` |
| G-G-05 | 계절 전환 시 아이템 풀 교체 확인 | `execute_method` (SeasonChanged 시뮬레이션) -> `get_console_logs` |
| G-G-06 | 계절 전환 시 전체 포인트 리프레시 확인 (seasonalRefreshOnChange) | `execute_method` -> `get_console_logs` |
| G-G-07 | XP 부여 확인 (XPSource.GatheringComplete) | `execute_method` (TryGather) -> `get_console_logs` (XP 로그) |
| G-G-08 | 채집 숙련도 XP 부여 및 레벨업 확인 | `execute_method` (반복 채집) -> `get_console_logs` (레벨업 이벤트) |
| G-G-09 | 세이브 -> 로드 후 포인트 상태/통계/숙련도 복원 확인 | `execute_method` (Save -> Load) -> `get_console_logs` |
| G-G-10 | 에너지 부족 시 채집 거부 확인 | `execute_method` (에너지 0으로 설정 후 TryGather) -> `get_console_logs` |
| G-G-11 | 필요 도구 미보유 시 채집 거부 확인 | `execute_method` (도구 미보유 상태에서 TryGather) -> `get_console_logs` |
| G-G-12 | HarvestOrigin.Gathering이 판매 시 올바르게 인식되는지 확인 | `execute_method` (Sell 호출) -> `get_console_logs` |

각 검증 항목당 약 2회 MCP 호출 (execute_method + get_console_logs).

#### G-G-13: 최종 씬 저장 및 PlayMode 종료

```
exit_play_mode
save_scene
```

- **MCP 호출**: 2회

---

## 5. 태스크 실행 순서 요약

```
G-A (데이터 레이어 스크립트) ──┐
                              │
                              └── G-B (시스템 레이어 스크립트) ──┐
                                                               │
                                  G-C (SO 에셋 생성) ◄──────────┤
                                                               │
                                  G-E (기존 시스템 확장) ◄───────┘
                                       │
                                       └── G-D (씬 배치) ──┐
                                                           │
                                           G-F (UI 연동) ◄─┤
                                                           │
                                                           └── G-G (통합 검증)
```

**병렬 실행 가능**:
- G-C와 G-E는 G-B 완료 후 병렬 실행 가능 (G-C는 SO 에셋, G-E는 기존 스크립트 수정)
- G-D와 G-F는 G-C + G-E 완료 후 병렬 실행 가능
- G-G는 모든 태스크 완료 후 실행

---

## 6. Cross-references

- `docs/systems/gathering-architecture.md` (ARC-031) -- 채집 시스템 기술 아키텍처 (클래스 설계, 세이브/로드, MCP Phase 요약)
- `docs/systems/gathering-system.md` (DES-016) -- 채집 시스템 디자인 canonical (아이템 목록, 가격, 숙련도 테이블, 포인트 배치)
- `docs/content/gathering-items.md` (CON-012) -- 채집 아이템 콘텐츠 canonical
- `docs/balance/economy-balance.md` -- 경제 밸런스 (채집물 가격 canonical)
- `docs/pipeline/data-pipeline.md` -- ScriptableObject 구조, GameSaveData 루트 클래스
- `docs/systems/fishing-architecture.md` -- 낚시 아키텍처 (유사 패턴 참조: Proficiency, SaveData, MCP Phase)
- `docs/mcp/fishing-tasks.md` -- 낚시 MCP 태스크 (UI 토스트 패턴 참조)
- `docs/systems/progression-architecture.md` -- XPSource enum, AddExp 패턴
- `docs/systems/economy-architecture.md` -- HarvestOrigin enum, GetGreenhouseMultiplier
- `docs/systems/inventory-architecture.md` -- ItemType enum, TryAddItem API
- `docs/systems/save-load-architecture.md` -- ISaveable, SaveLoadOrder, GameSaveData
- `docs/systems/farm-expansion-architecture.md` -- FarmZoneManager, Zone 해금 상태
- `docs/systems/time-season-architecture.md` -- TimeManager, OnDayChanged/OnSeasonChanged
- `docs/systems/project-structure.md` 섹션 2~4 -- 네임스페이스, 의존성, asmdef
- `docs/mcp/scene-setup-tasks.md` (ARC-002) -- 씬 기본 계층 구조
- `docs/mcp/farming-tasks.md` (ARC-003) -- 경작 시스템 MCP 태스크
- `docs/mcp/facilities-tasks.md` (ARC-007) -- 시설 시스템 MCP 태스크 (채집물 가공소 연동 참조)

---

### Phase -> 태스크 매핑 (gathering-architecture.md Part III 대응)

| Phase (architecture.md) | 대응 태스크 (this doc) |
|--------------------------|----------------------|
| Phase A: SO 에셋 생성 (데이터 레이어) | G-A (데이터 레이어 스크립트 14종) |
| Phase B: GatheringManager 구현 (시스템 레이어) | G-B (GatheringPoint + GatheringManager) |
| Phase C: SO 에셋 인스턴스 생성 | G-C (Config/Item/Point/Price SO 에셋) |
| Phase D: 포인트 배치 (씬 구성) | G-D (씬 배치) |
| Phase E: 기존 시스템 확장 | G-E (enum/switch 확장 7건) |
| Phase F: UI 연동 | G-F (프롬프트, 결과 팝업, 레벨업 토스트) |
| Phase G: 검증 | G-G (12개 검증 항목 + 씬 저장) |

---

## 7. Open Questions ([OPEN])

- [OPEN] **간단 미니게임 도입 여부**: 현재 설계는 단순 인터랙션(클릭 -> 수집)이지만, 숙련도 레벨이 높아질 때 또는 Rare/Legendary 아이템 채집 시 간단한 미니게임(타이밍 클릭 등)을 도입할지 여부. Designer와 합의 필요. (-> see `docs/systems/gathering-architecture.md` 섹션 9)

- [OPEN] **GatheringRarity와 FishRarity 통합**: 두 enum이 동일한 구조(Common/Uncommon/Rare/Legendary)를 가진다. `SeedMind.ItemRarity`로 통합하면 코드 중복이 줄지만, 시스템 간 결합도가 높아진다. (-> see `docs/systems/gathering-architecture.md` 섹션 9)

- [OPEN] **채집 도구 시스템**: 일부 채집물이 특정 도구를 요구하는 설계이나, 도구별 채집 효과(범위, 속도)의 세부 사양은 확정 필요. (-> see `docs/systems/gathering-system.md`)

- [OPEN] **날씨 영향 세부 설계**: `weatherBonus` 필드가 존재하지만, 구체적인 보정 계수는 확정 필요. (-> see `docs/systems/gathering-system.md`)

---

## 8. Risks ([RISK])

- [RISK] `create_scriptable_object` MCP 도구 가용 여부 미확인. SO 인스턴스 생성이 MCP에서 미지원인 경우, Editor 스크립트를 통한 우회 필요. (-> see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)
- [RISK] MCP로 SO 배열/참조 필드(`availableItems[]`, `seasonOverrides[]`, `_gatheringPoints[]`, `_gatheringConfig`) 설정이 불가능할 수 있다. 대안: `Resources.LoadAll` 또는 `FindObjectsOfType`으로 런타임 자동 로드/수집.
- [RISK] SerializeField 오브젝트 참조 설정(UI 컴포넌트의 TextMeshPro 참조 등)이 MCP로 불가능할 수 있다. 대안: Awake()에서 `transform.Find()` 또는 `GetComponentInChildren<>()`으로 자동 탐색.
- [RISK] 스크립트 컴파일 에러 시 후속 `add_component` 실패. 컴파일 순서(G-A -> G-B)와 각 Phase 사이 컴파일 대기가 필수.
- [RISK] 총 ~136회 MCP 호출은 실행 시간이 길다. Editor 스크립트(`CreateGatheringAssets.cs`)로 SO 에셋(G-C)을 일괄 생성하면 약 40회를 ~5회로 압축 가능. MCP 단독 실행 대비 Editor 스크립트 우회를 권장.
- [RISK] **포인트 상태 Dictionary 세이브 크기**: 채집 포인트 수가 수십 개에 달할 경우 세이브 파일 크기 증가. 비활성 포인트만 저장하는 최적화 고려. (-> see `docs/systems/gathering-architecture.md` 섹션 9)
- [RISK] **GatheringSaveData 구버전 호환**: `gathering` 필드가 null인 구버전 세이브 파일 로드 시 방어 로직 필수. (-> see `docs/systems/gathering-architecture.md` 섹션 9)
- [RISK] **Zone 미해금 상태에서 포인트 재생성 타이머**: 해금 전 타이머 작동 정책 확정 필요. (-> see `docs/systems/gathering-architecture.md` 섹션 9)

---
