# 시설 시스템 MCP 태스크 시퀀스 (ARC-007)

> 시설(Building) 시스템의 ScriptableObject 에셋 생성, 스크립트 생성, 프리팹 배치, 씬 연결, 통합 테스트를 MCP for Unity 태스크로 상세 정의  
> 작성: Claude Code (Opus) | 2026-04-06  
> Phase 1 | 문서 ID: ARC-007

---

## 1. 개요

### 목적

이 문서는 `docs/systems/facilities-architecture.md`(ARC-007) Part II에서 요약된 MCP 구현 계획(Phase A~E)을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 시설 시스템의 데이터 레이어(SO 에셋), 시스템 레이어(스크립트), 프리팹, 씬 배치, UI 연결을 완성한다.

### 의존성

```
시설 시스템 MCP 태스크 의존 관계:
├── SeedMind.Core     (TimeManager, SaveManager, EventBus, GameDataSO)
├── SeedMind.Farm     (FarmGrid, FarmTile, CropInstance, GrowthSystem)
├── SeedMind.Economy  (EconomyManager -- 건설 비용 지불)
├── SeedMind.Player   (InventoryManager -- 창고 연동, 이벤트 통신)
└── SeedMind.UI       (BuildingUI -- 건설/가공/창고 UI)
```

(-> see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 완료된 태스크 의존성

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조, SCN_Farm 기본 계층 (Managers, Farm, Player, UI) |
| ARC-003 | `docs/mcp/farming-tasks.md` | Phase A~C 전체 | FarmGrid 타일, CropData SO, ToolData SO, GrowthSystem |

### 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `Canvas_Overlay` (UI 루트) | ARC-002 Phase B |
| `DataRegistry` (SO 로드 시스템) | `docs/pipeline/data-pipeline.md` Part II |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A |
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `--- FARM ---` (씬 계층 부모) | ARC-002 Phase B |
| `FarmGrid` (경작 그리드) | ARC-003 |

### 총 MCP 호출 예상 수

| 태스크 | 호출 수 |
|--------|--------|
| F-1: BuildingData SO 에셋 생성 (7종) | 79회 |
| F-2: BuildingManager 스크립트 생성 | 12회 |
| F-3: 시설 프리팹 생성 (7종) | 72회 |
| F-4: 씬 배치 -- FarmScene에 BuildingManager 배치 | 8회 |
| F-5: 건설 UI 연결 (BuildingShopUI) | 24회 |
| F-6: 업그레이드 UI 연결 | 14회 |
| F-7: 시설 인터랙션 (레시피 실행 트리거) | 10회 |
| F-8: 통합 테스트 시퀀스 | 18회 |
| **합계** | **~232회** |

[RISK] 총 232회 MCP 호출은 상당하다. Editor 스크립트(CreateBuildingAssets.cs)를 통한 SO 에셋 일괄 생성으로 F-1의 78회를 ~5회로 감소시킬 수 있다. MCP 단독 실행 시 시간 비용이 크므로 Editor 스크립트 우회를 권장한다.

---

## 2. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋 폴더 생성 | F-1 |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | F-1 |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | F-1~F-7 전체 |
| `create_script` | C# 스크립트 파일 생성 | F-2 |
| `create_object` | 빈 GameObject 생성 | F-3, F-4, F-5, F-6 |
| `add_component` | MonoBehaviour 컴포넌트 부착 | F-3, F-4, F-5, F-6 |
| `set_parent` | 오브젝트 부모 설정 | F-3, F-4, F-5 |
| `save_as_prefab` | GameObject를 프리팹으로 저장 | F-3 |
| `save_scene` | 씬 저장 | F-4, F-5, F-6, F-8 |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | F-8 |
| `execute_menu_item` | 편집기 명령 실행 (컴파일 대기 등) | F-2, F-3 |
| `execute_method` | 런타임 메서드 호출 (테스트) | F-8 |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | F-8 |

[RISK] `create_scriptable_object`, `save_as_prefab` 도구의 가용 여부 및 파라미터 형식 사전 검증 필요. SO 인스턴스 생성이나 프리팹 저장이 MCP에서 미지원인 경우, Editor 스크립트를 통한 우회 필요. (-> see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)

---

## 3. 필요 C# 스크립트 목록

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 태스크 순서대로 작성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/Building/Data/BuildingEffectType.cs` | `BuildingEffectType` (enum) | `SeedMind.Building.Data` | F-2 |
| S-02 | `Scripts/Building/Data/PlacementRule.cs` | `PlacementRule` (enum) | `SeedMind.Building.Data` | F-2 |
| S-03 | `Scripts/Building/Data/BuildingData.cs` | `BuildingData` (SO) | `SeedMind.Building.Data` | F-2 |
| S-04 | `Scripts/Building/BuildingInstance.cs` | `BuildingInstance` | `SeedMind.Building` | F-2 |
| S-05 | `Scripts/Building/BuildingEvents.cs` | `BuildingEvents` (static) | `SeedMind.Building` | F-2 |
| S-06 | `Scripts/Building/BuildingManager.cs` | `BuildingManager` (MonoBehaviour) | `SeedMind.Building` | F-2 |
| S-07 | `Scripts/Building/Buildings/WaterTankSystem.cs` | `WaterTankSystem` | `SeedMind.Building` | F-2 |
| S-08 | `Scripts/Farm/ISeasonOverrideProvider.cs` | `ISeasonOverrideProvider` (interface) | `SeedMind.Farm` | F-2 |
| S-09 | `Scripts/Building/Buildings/GreenhouseSystem.cs` | `GreenhouseSystem` | `SeedMind.Building` | F-2 |
| S-10 | `Scripts/Building/Buildings/StorageSlot.cs` | `StorageSlot` | `SeedMind.Building` | F-2 |
| S-11 | `Scripts/Building/Buildings/StorageSlotContainer.cs` | `StorageSlotContainer` | `SeedMind.Building` | F-2 |
| S-12 | `Scripts/Building/Buildings/StorageSystem.cs` | `StorageSystem` | `SeedMind.Building` | F-2 |
| S-13 | `Scripts/UI/BuildingShopUI.cs` | `BuildingShopUI` (MonoBehaviour) | `SeedMind.UI` | F-5 |
| S-14 | `Scripts/UI/BuildingInfoUI.cs` | `BuildingInfoUI` (MonoBehaviour) | `SeedMind.UI` | F-6 |
| S-15 | `Scripts/Building/Buildings/ProcessingSlot.cs` | `ProcessingSlot` | `SeedMind.Building` | F-2 |
| S-16 | `Scripts/Building/Buildings/ProcessingSystem.cs` | `ProcessingSystem` | `SeedMind.Building` | F-2 |
| S-17 | `Scripts/Building/BuildingInteraction.cs` | `BuildingInteraction` (MonoBehaviour) | `SeedMind.Building` | F-7 |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: S-01/S-02 -> S-03 -> S-04/S-05 -> S-06 -> S-07~S-12 -> S-15/S-16 -> S-13/S-14 -> S-17. 각 Phase 사이에 Unity 컴파일 대기가 필요하다.

---

## 4. 태스크 목록

---

### F-1: BuildingData SO 에셋 생성 (시설 7종)

**목적**: 시설 7종(물탱크, 창고, 온실, 가공소, 제분소, 발효실, 베이커리)의 BuildingData SO 에셋을 생성한다.

**전제**: BuildingData.cs, BuildingEffectType.cs, PlacementRule.cs가 컴파일 완료된 상태 (F-2 Phase 1 완료).

**의존 태스크**: F-2 (Phase 1: 데이터 레이어 스크립트)

#### F-1-01: 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Buildings"

create_folder
  path: "Assets/_Project/Data/Buildings/Recipes"
```

- **MCP 호출**: 2회

#### F-1-02: 물탱크 BuildingData SO

```
create_scriptable_object
  type: "SeedMind.Building.Data.BuildingData"
  asset_path: "Assets/_Project/Data/Buildings/SO_Bldg_WaterTank.asset"

set_property  target: "SO_Bldg_WaterTank"
  dataId = "building_water_tank"
  displayName = "물탱크"
  description = "인접 경작지에 매일 아침 자동으로 물을 준다."
  effectType = 1                              // AutoWater (-> see docs/systems/facilities-architecture.md 섹션 2.2 enum)
  buildCost = 0                               // (-> see docs/design.md 섹션 4.6)
  requiredLevel = 0                           // (-> see docs/design.md 섹션 4.6)
  buildTimeDays = 0                           // (-> see docs/content/facilities.md 섹션 2.2)
  tileSize = { x: 0, y: 0 }                  // (-> see docs/content/facilities.md 섹션 2.1)
  placementRules = 0                          // FarmOnly
  effectRadius = 0                            // (-> see docs/content/facilities.md 섹션 3.2)
  effectValue = 0.0                           // (-> see docs/content/facilities.md 섹션 3.2)
  maxUpgradeLevel = 0                         // (-> see docs/content/facilities.md 섹션 3.4)
  upgradeCosts = []                           // (-> see docs/content/facilities.md 섹션 3.4)
```

> **주의**: `buildCost`, `requiredLevel`, `buildTimeDays`, `tileSize`, `effectRadius`, `effectValue`, `maxUpgradeLevel`, `upgradeCosts`의 구체적 수치는 MCP 실행 시점에 canonical 문서에서 읽어 입력한다. 본 문서에서 수치를 직접 기재하지 않는다 (PATTERN-006, PATTERN-007).

- **MCP 호출**: 1(생성) + 10(필드 설정) = 11회

#### F-1-03: 창고 BuildingData SO

```
create_scriptable_object
  type: "SeedMind.Building.Data.BuildingData"
  asset_path: "Assets/_Project/Data/Buildings/SO_Bldg_Storage.asset"

set_property  target: "SO_Bldg_Storage"
  dataId = "building_storage"
  displayName = "창고"
  description = "작물을 저장하여 가격 변동에 대응할 수 있다."
  effectType = 3                              // Storage
  buildCost = 0                               // (-> see docs/design.md 섹션 4.6)
  requiredLevel = 0                           // (-> see docs/design.md 섹션 4.6)
  buildTimeDays = 0                           // (-> see docs/content/facilities.md 섹션 2.2)
  tileSize = { x: 0, y: 0 }                  // (-> see docs/content/facilities.md 섹션 2.1)
  placementRules = 0                          // FarmOnly
  effectRadius = 0                            // 창고는 범위 효과 없음
  effectValue = 0.0                           // 최대 슬롯 수 (-> see docs/systems/inventory-system.md 섹션 2.3)
  maxUpgradeLevel = 0                         // (-> see docs/content/facilities.md 섹션 5.5)
  upgradeCosts = []                           // (-> see docs/content/facilities.md 섹션 5.5)
```

- **MCP 호출**: 11회

#### F-1-04: 온실 BuildingData SO

```
create_scriptable_object
  type: "SeedMind.Building.Data.BuildingData"
  asset_path: "Assets/_Project/Data/Buildings/SO_Bldg_Greenhouse.asset"

set_property  target: "SO_Bldg_Greenhouse"
  dataId = "building_greenhouse"
  displayName = "온실"
  description = "계절에 관계없이 모든 작물을 재배할 수 있다."
  effectType = 2                              // SeasonBypass
  buildCost = 0                               // (-> see docs/design.md 섹션 4.6)
  requiredLevel = 0                           // (-> see docs/design.md 섹션 4.6)
  buildTimeDays = 0                           // (-> see docs/content/facilities.md 섹션 2.2)
  tileSize = { x: 0, y: 0 }                  // (-> see docs/content/facilities.md 섹션 2.1)
  placementRules = 0                          // FarmOnly
  effectRadius = 0                            // 온실은 내부 영역만 사용
  effectValue = 0.0                           // 내부 경작 타일 수 (-> see docs/content/facilities.md 섹션 4.2)
  maxUpgradeLevel = 0                         // (-> see docs/content/facilities.md 섹션 4.6)
  upgradeCosts = []                           // (-> see docs/content/facilities.md 섹션 4.6)
```

- **MCP 호출**: 11회

#### F-1-05: 가공소 BuildingData SO

```
create_scriptable_object
  type: "SeedMind.Building.Data.BuildingData"
  asset_path: "Assets/_Project/Data/Buildings/SO_Bldg_Processor.asset"

set_property  target: "SO_Bldg_Processor"
  dataId = "building_processing"
  displayName = "가공소"
  description = "작물을 가공하여 더 높은 가격에 판매할 수 있다."
  effectType = 4                              // Processing
  buildCost = 0                               // (-> see docs/design.md 섹션 4.6)
  requiredLevel = 0                           // (-> see docs/design.md 섹션 4.6)
  buildTimeDays = 0                           // (-> see docs/content/facilities.md 섹션 2.2)
  tileSize = { x: 0, y: 0 }                  // (-> see docs/content/facilities.md 섹션 2.1)
  placementRules = 0                          // FarmOnly
  effectRadius = 0                            // 가공소는 범위 효과 없음
  effectValue = 0.0                           // 초기 슬롯 수 (-> see docs/content/processing-system.md 섹션 5.1)
  maxUpgradeLevel = 0                         // (-> see docs/content/facilities.md 섹션 6.5)
  upgradeCosts = []                           // (-> see docs/content/facilities.md 섹션 6.5)
```

- **MCP 호출**: 11회

#### F-1-06: 제분소 BuildingData SO

```
create_scriptable_object
  type: "SeedMind.Building.Data.BuildingData"
  asset_path: "Assets/_Project/Data/Buildings/SO_Bldg_Mill.asset"

set_property  target: "SO_Bldg_Mill"
  dataId = "building_mill"
  displayName = "제분소"
  description = "곡물과 작물을 분쇄하여 가루와 분말을 만든다."
  effectType = 4                              // Processing
  buildCost = 0                               // (-> see docs/design.md 섹션 4.6)
  requiredLevel = 0                           // (-> see docs/design.md 섹션 4.6)
  buildTimeDays = 0                           // (-> see docs/content/facilities.md 섹션 2.2)
  tileSize = { x: 0, y: 0 }                  // (-> see docs/content/facilities.md 섹션 2.1)
  placementRules = 0                          // FarmOnly
  effectRadius = 0
  effectValue = 0.0                           // 슬롯 수 (-> see docs/content/processing-system.md 섹션 2.3.1)
  maxUpgradeLevel = 0                         // (-> see docs/content/facilities.md 섹션 7.5)
  upgradeCosts = []                           // (-> see docs/content/facilities.md 섹션 7.5)
```

- **MCP 호출**: 11회

#### F-1-07: 발효실 BuildingData SO

```
create_scriptable_object
  type: "SeedMind.Building.Data.BuildingData"
  asset_path: "Assets/_Project/Data/Buildings/SO_Bldg_Fermentation.asset"

set_property  target: "SO_Bldg_Fermentation"
  dataId = "building_fermentation"
  displayName = "발효실"
  description = "작물을 발효시켜 와인, 식초, 발효식품을 만든다."
  effectType = 4                              // Processing
  buildCost = 0                               // (-> see docs/design.md 섹션 4.6)
  requiredLevel = 0                           // (-> see docs/design.md 섹션 4.6)
  buildTimeDays = 0                           // (-> see docs/content/facilities.md 섹션 2.2)
  tileSize = { x: 0, y: 0 }                  // (-> see docs/content/facilities.md 섹션 2.1)
  placementRules = 0                          // FarmOnly
  effectRadius = 0
  effectValue = 0.0                           // 슬롯 수 (-> see docs/content/processing-system.md 섹션 2.3.2)
  maxUpgradeLevel = 0                         // (-> see docs/content/facilities.md 섹션 8.5)
  upgradeCosts = []                           // (-> see docs/content/facilities.md 섹션 8.5)
```

- **MCP 호출**: 11회

#### F-1-08: 베이커리 BuildingData SO

```
create_scriptable_object
  type: "SeedMind.Building.Data.BuildingData"
  asset_path: "Assets/_Project/Data/Buildings/SO_Bldg_Bakery.asset"

set_property  target: "SO_Bldg_Bakery"
  dataId = "building_bakery"
  displayName = "베이커리"
  description = "가공 중간재를 사용하여 고급 요리를 만든다. 연료(장작) 소모."
  effectType = 4                              // Processing
  buildCost = 0                               // (-> see docs/design.md 섹션 4.6)
  requiredLevel = 0                           // (-> see docs/design.md 섹션 4.6)
  buildTimeDays = 0                           // (-> see docs/content/facilities.md 섹션 2.2)
  tileSize = { x: 0, y: 0 }                  // (-> see docs/content/facilities.md 섹션 2.1)
  placementRules = 0                          // FarmOnly
  effectRadius = 0
  effectValue = 0.0                           // 슬롯 수 (-> see docs/content/processing-system.md 섹션 2.3.3)
  maxUpgradeLevel = 0                         // (-> see docs/content/facilities.md 섹션 9.5)
  upgradeCosts = []                           // (-> see docs/content/facilities.md 섹션 9.5)
```

- **MCP 호출**: 11회

#### F-1 검증 체크리스트

- [ ] `Assets/_Project/Data/Buildings/` 폴더 존재
- [ ] SO 에셋 7개 존재: `SO_Bldg_WaterTank`, `SO_Bldg_Storage`, `SO_Bldg_Greenhouse`, `SO_Bldg_Processor`, `SO_Bldg_Mill`, `SO_Bldg_Fermentation`, `SO_Bldg_Bakery`
- [ ] 물탱크: `effectType = 1` (AutoWater)
- [ ] 온실: `effectType = 2` (SeasonBypass)
- [ ] 창고: `effectType = 3` (Storage)
- [ ] 가공소/제분소/발효실/베이커리: `effectType = 4` (Processing)
- [ ] 모든 `dataId`가 `docs/content/facilities.md` 섹션 2.1의 영문 ID와 일치
- [ ] 콘솔 에러 없음

**F-1 MCP 호출 합계**: 2(폴더) + 11 x 7(에셋) = 79회

---

### F-2: BuildingManager 스크립트 생성

**목적**: 시설 시스템의 핵심 스크립트(enum, SO 클래스, 런타임 클래스, 매니저, 서브시스템)를 생성한다.

**전제**: GameDataSO 기반 클래스가 존재 (-> see `docs/pipeline/data-pipeline.md` Part II 섹션 1). FarmGrid, GrowthSystem 스크립트가 존재 (ARC-003).

**의존 태스크**: 없음 (F-1보다 먼저 실행해야 함. SO 에셋 생성 전에 클래스 정의가 필요)

#### F-2-01: BuildingEffectType enum 생성

```
create_script
  path: "Assets/_Project/Scripts/Building/Data/BuildingEffectType.cs"
  namespace: "SeedMind.Building.Data"
  type: enum
```

enum 값: (-> see `docs/systems/facilities-architecture.md` 섹션 2.2 BuildingEffectType enum)

#### F-2-02: PlacementRule enum 생성

```
create_script
  path: "Assets/_Project/Scripts/Building/Data/PlacementRule.cs"
  namespace: "SeedMind.Building.Data"
  type: enum
```

enum 값: (-> see `docs/systems/facilities-architecture.md` 섹션 2.2 PlacementRule enum)

#### F-2-03: BuildingData SO 클래스 생성

```
create_script
  path: "Assets/_Project/Scripts/Building/Data/BuildingData.cs"
  namespace: "SeedMind.Building.Data"
  base_class: "GameDataSO"
```

필드 정의: (-> see `docs/systems/facilities-architecture.md` 섹션 2.2 및 `docs/pipeline/data-pipeline.md` 섹션 2.4)

#### F-2-04: BuildingInstance 런타임 클래스 생성

```
create_script
  path: "Assets/_Project/Scripts/Building/BuildingInstance.cs"
  namespace: "SeedMind.Building"
  type: class (Plain C#)
```

클래스 구조: (-> see `docs/systems/facilities-architecture.md` 섹션 3.2)

#### F-2-05: BuildingEvents 정적 클래스 생성

```
create_script
  path: "Assets/_Project/Scripts/Building/BuildingEvents.cs"
  namespace: "SeedMind.Building"
  type: static class
```

이벤트 정의: (-> see `docs/systems/facilities-architecture.md` 섹션 8.3)

#### F-2-06: BuildingManager MonoBehaviour 생성

```
create_script
  path: "Assets/_Project/Scripts/Building/BuildingManager.cs"
  namespace: "SeedMind.Building"
  base_class: "MonoBehaviour"
```

메서드: CanPlace, PlaceBuilding, RemoveBuilding, UpgradeBuilding, GetBuildingAt, GetBuildingsByType, OnDayChangedHandler, AdvanceConstruction, GetSaveData, LoadSaveData
(-> see `docs/systems/facilities-architecture.md` 섹션 3.1~3.5)

테스트 전용 메서드 (에디터/디버그용):
- `DebugBuildInstant(string dataId, int x, int y)` — 건설 시간 생략, 즉시 완료
- `DebugUpgrade(string dataId)` — 골드 확인 없이 즉시 업그레이드
- `DebugDemolish(string dataId, int x, int y)` — 즉시 철거
- `DebugStoreItem(string buildingId, string itemId, int count, string quality)` — StorageSystem 프록시 (MonoBehaviour 아닌 StorageSystem 직접 호출 대신 사용)
- `DebugRetrieveItem(string buildingId, string itemId, int count)` — StorageSystem 프록시

#### F-2-07: WaterTankSystem 생성

```
create_script
  path: "Assets/_Project/Scripts/Building/Buildings/WaterTankSystem.cs"
  namespace: "SeedMind.Building"
  type: class (Plain C#)
```

클래스 구조: (-> see `docs/systems/facilities-architecture.md` 섹션 4.1)

#### F-2-08: ISeasonOverrideProvider 인터페이스 생성

```
create_script
  path: "Assets/_Project/Scripts/Farm/ISeasonOverrideProvider.cs"
  namespace: "SeedMind.Farm"
  type: interface
```

인터페이스 정의: (-> see `docs/systems/facilities-architecture.md` 섹션 5.3)

#### F-2-09: GreenhouseSystem 생성

```
create_script
  path: "Assets/_Project/Scripts/Building/Buildings/GreenhouseSystem.cs"
  namespace: "SeedMind.Building"
  type: class (Plain C#)
```

ISeasonOverrideProvider 구현. 클래스 구조: (-> see `docs/systems/facilities-architecture.md` 섹션 5.1)

#### F-2-10: StorageSlot / StorageSlotContainer / StorageSystem 생성

```
create_script
  path: "Assets/_Project/Scripts/Building/Buildings/StorageSlot.cs"
  namespace: "SeedMind.Building"
  type: class (Plain C#)

create_script
  path: "Assets/_Project/Scripts/Building/Buildings/StorageSlotContainer.cs"
  namespace: "SeedMind.Building"
  type: class (Plain C#)

create_script
  path: "Assets/_Project/Scripts/Building/Buildings/StorageSystem.cs"
  namespace: "SeedMind.Building"
  type: class (Plain C#)
```

클래스 구조: (-> see `docs/systems/facilities-architecture.md` 섹션 6.1~6.2)

#### F-2-11: Unity 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

컴파일 성공 확인 후 F-1으로 진행.

#### F-2 검증 체크리스트

- [ ] 16개 스크립트 파일 존재 (S-01 ~ S-16; S-17은 F-7에서 생성)
- [ ] Unity 컴파일 에러 없음 (콘솔 확인)
- [ ] `BuildingEffectType` enum에 5개 값 (None, AutoWater, SeasonBypass, Storage, Processing)
- [ ] `PlacementRule` enum에 3개 값 (FarmOnly, FarmEdge, Anywhere)
- [ ] `BuildingData` 필드 수가 16개 (부모 3 + 자체 13, PATTERN-005)
- [ ] `BuildingManager`가 MonoBehaviour 상속
- [ ] `GreenhouseSystem`이 ISeasonOverrideProvider 구현

**F-2 MCP 호출 합계**: 12(스크립트 생성) + 1(컴파일) - 일부 병합 가능 = ~12회

---

### F-3: 시설 프리팹 생성 (7종 + 건설 중 프리팹)

**목적**: 각 시설의 완성 프리팹과 건설 중 프리팹을 생성한다. placeholder 3D 오브젝트(Cube 기반) + BoxCollider + 인터랙션 트리거를 포함한다.

**전제**: F-2 완료 (스크립트 컴파일 완료)

**의존 태스크**: F-2

#### F-3-01: 프리팹 폴더 생성

```
create_folder
  path: "Assets/_Project/Prefabs/Buildings"
create_folder
  path: "Assets/_Project/Prefabs/Buildings/Construction"
```

- **MCP 호출**: 2회

#### F-3-02: 물탱크 프리팹 생성

```
create_object
  name: "PFB_Bldg_WaterTank"

// 본체 Cube (placeholder)
create_object
  name: "Model"
  parent: "PFB_Bldg_WaterTank"
  // Cube 프리미티브 사용

set_property  target: "Model"
  Transform.localScale = (0, 0, 0)           // (-> see docs/content/facilities.md 섹션 2.1 tileSize 기반 스케일)
  MeshRenderer.material = null               // 머티리얼은 후속 에셋 파이프라인에서 설정

// BoxCollider (인터랙션 영역)
add_component  target: "PFB_Bldg_WaterTank"
  type: "UnityEngine.BoxCollider"

set_property  target: "PFB_Bldg_WaterTank.BoxCollider"
  isTrigger = false
  size = (0, 0, 0)                           // (-> see docs/content/facilities.md 섹션 2.1 tileSize)

// 인터랙션 트리거 (별도 자식 오브젝트)
create_object
  name: "InteractionTrigger"
  parent: "PFB_Bldg_WaterTank"

add_component  target: "InteractionTrigger"
  type: "UnityEngine.BoxCollider"

set_property  target: "InteractionTrigger.BoxCollider"
  isTrigger = true
  size = (0, 0, 0)                           // tileSize보다 1타일 확장 (인터랙션 영역)

save_as_prefab
  source: "PFB_Bldg_WaterTank"
  path: "Assets/_Project/Prefabs/Buildings/PFB_Bldg_WaterTank.prefab"
```

> 모든 크기/스케일 수치는 MCP 실행 시점에 `docs/content/facilities.md` 섹션 2.1의 tileSize를 참조하여 입력 (PATTERN-007).

- **MCP 호출**: 3(오브젝트) + 2(컴포넌트) + 4(프로퍼티) + 1(프리팹 저장) = 10회

#### F-3-03 ~ F-3-08: 나머지 시설 프리팹 (6종)

창고, 온실, 가공소, 제분소, 발효실, 베이커리 각각 F-3-02와 동일 패턴으로 생성.

프리팹 네이밍:
- `PFB_Bldg_Storage`
- `PFB_Bldg_Greenhouse`
- `PFB_Bldg_Processor`
- `PFB_Bldg_Mill`
- `PFB_Bldg_Fermentation`
- `PFB_Bldg_Bakery`

각 시설의 스케일 및 크기: (-> see `docs/content/facilities.md` 섹션 2.1 시설 일람)

- **MCP 호출**: 10회 x 6 = 60회

#### F-3-09: 건설 중 공통 프리팹 생성

모든 시설의 건설 중 상태에 공통으로 사용하는 placeholder 프리팹. 반투명 박스 + 프로그레스 UI.

```
create_object
  name: "PFB_Bldg_Construction"

create_object
  name: "FrameModel"
  parent: "PFB_Bldg_Construction"

set_property  target: "FrameModel"
  Transform.localScale = (1, 1, 1)           // 실제 스케일은 런타임에 BuildingData.tileSize로 조정

// 반투명 머티리얼 할당 (URP/Lit, Alpha=0.3)

save_as_prefab
  source: "PFB_Bldg_Construction"
  path: "Assets/_Project/Prefabs/Buildings/Construction/PFB_Bldg_Construction.prefab"
```

- **MCP 호출**: 2(오브젝트) + 2(프로퍼티) + 1(프리팹 저장) = 5회

[RISK] `save_as_prefab` MCP 도구 가용 여부 미확인. 대안: PrefabUtility Editor 스크립트로 일괄 프리팹화.

#### F-3 검증 체크리스트

- [ ] `Assets/_Project/Prefabs/Buildings/` 폴더 존재
- [ ] 프리팹 7개 존재: `PFB_Bldg_WaterTank`, `PFB_Bldg_Storage`, `PFB_Bldg_Greenhouse`, `PFB_Bldg_Processor`, `PFB_Bldg_Mill`, `PFB_Bldg_Fermentation`, `PFB_Bldg_Bakery`
- [ ] 건설 중 프리팹 1개 존재: `PFB_Bldg_Construction`
- [ ] 모든 프리팹에 BoxCollider 존재
- [ ] 모든 프리팹에 InteractionTrigger 자식(isTrigger=true) 존재
- [ ] 콘솔 에러 없음

**F-3 MCP 호출 합계**: 2(폴더) + 9(물탱크) + 54(나머지 6종) + 5(건설 중) - 일부 병합 가능 = ~63회 (실제: 약 70회, 병합으로 63회)

---

### F-4: 씬 배치 -- FarmScene에 BuildingManager 배치

**목적**: SCN_Farm 씬에 BuildingManager GameObject를 생성하고, 컴포넌트를 부착하며, SO 에셋 참조를 연결한다.

**전제**: F-1 완료 (SO 에셋 존재), F-2 완료 (스크립트 컴파일), F-3 완료 (프리팹 존재)

**의존 태스크**: F-1, F-2, F-3

#### F-4-01: BuildingManager GameObject 생성

```
create_object
  name: "BuildingManager"

set_parent
  target: "BuildingManager"
  parent: "--- MANAGERS ---"
```

- **MCP 호출**: 2회

#### F-4-02: BuildingManager 컴포넌트 부착

```
add_component  target: "BuildingManager"
  type: "SeedMind.Building.BuildingManager"
```

- **MCP 호출**: 1회

#### F-4-03: Buildings 부모 노드 생성

```
create_object
  name: "--- BUILDINGS ---"

set_parent
  target: "--- BUILDINGS ---"
  parent: "--- FARM ---"

set_property  target: "BuildingManager"
  _buildingParent = [--- BUILDINGS --- Transform 참조]
```

- **MCP 호출**: 3회

#### F-4-04: BuildingData SO 배열 연결

```
set_property  target: "BuildingManager"
  _allBuildingData = [
    SO_Bldg_WaterTank,
    SO_Bldg_Storage,
    SO_Bldg_Greenhouse,
    SO_Bldg_Processor,
    SO_Bldg_Mill,
    SO_Bldg_Fermentation,
    SO_Bldg_Bakery
  ]
```

[RISK] MCP로 SO 배열에 참조를 설정하는 것이 가능한지 미확인. 대안: BuildingManager.Awake()에서 `Resources.LoadAll<BuildingData>("Data/Buildings")`로 자동 로드.

- **MCP 호출**: 1회

#### F-4-05: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

#### F-4 검증 체크리스트

- [ ] `--- MANAGERS ---/BuildingManager` GameObject 존재
- [ ] BuildingManager 컴포넌트 부착 확인
- [ ] `--- FARM ---/--- BUILDINGS ---` 빈 Transform 존재
- [ ] `_buildingParent` 참조 연결 확인
- [ ] `_allBuildingData` 배열에 7개 SO 연결 확인
- [ ] 콘솔 에러 없음

**F-4 MCP 호출 합계**: 8회

---

### F-5: 건설 UI 연결 (BuildingShopUI)

**목적**: 목공소 NPC에게 건설을 의뢰하는 UI 패널을 생성하고 BuildingShopUI 컴포넌트를 연결한다.

**전제**: F-2 완료 (스크립트 컴파일), Canvas_Overlay 존재 (ARC-002)

**의존 태스크**: F-2

#### F-5-01: BuildingShopUI.cs 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/UI/BuildingShopUI.cs"
  namespace: "SeedMind.UI"
  base_class: "MonoBehaviour"
```

SerializeField 목록:
- `_buildingListParent: Transform` -- 시설 목록 영역
- `_buildingSlotPrefab: GameObject` -- 개별 시설 슬롯 프리팹
- `_goldText: Text/TMP_Text` -- 소지 골드 표시
- `_closeButton: Button`

공개 메서드:
- `Open()` -- 패널 열기, 시설 목록 표시
- `OnBuildingSelected(BuildingData data)` -- 시설 선택 시 배치 모드 진입
- `Close()` -- 패널 닫기

(-> see `docs/systems/facilities-architecture.md` 섹션 3.3 건설 흐름)

- **MCP 호출**: 1회

#### F-5-02: Unity 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

#### F-5-03: BuildingShopPanel GameObject 생성

```
create_object
  name: "BuildingShopPanel"
  parent: "Canvas_Overlay"

set_property  target: "BuildingShopPanel"
  RectTransform.anchorMin = (0.1, 0.1)
  RectTransform.anchorMax = (0.9, 0.9)
  RectTransform.offsetMin = (0, 0)
  RectTransform.offsetMax = (0, 0)
  activeInHierarchy = false                  // 기본 비활성
```

- **MCP 호출**: 1(생성) + 5(프로퍼티) = 6회

#### F-5-04: 건설 목록 영역 생성

```
create_object
  name: "BuildingListArea"
  parent: "BuildingShopPanel"

set_property  target: "BuildingListArea"
  RectTransform.anchorMin = (0, 0)
  RectTransform.anchorMax = (0.7, 1)
  RectTransform.offsetMin = (10, 50)
  RectTransform.offsetMax = (-5, -10)
```

- **MCP 호출**: 1(생성) + 4(프로퍼티) = 5회

#### F-5-05: 시설 정보 영역 생성

```
create_object
  name: "InfoArea"
  parent: "BuildingShopPanel"

set_property  target: "InfoArea"
  RectTransform.anchorMin = (0.7, 0)
  RectTransform.anchorMax = (1, 1)
  RectTransform.offsetMin = (5, 50)
  RectTransform.offsetMax = (-10, -10)
```

- **MCP 호출**: 1(생성) + 4(프로퍼티) = 5회

#### F-5-06: CloseButton 생성

```
create_object
  name: "CloseButton"
  parent: "BuildingShopPanel"

add_component  target: "CloseButton"
  type: "UnityEngine.UI.Button"

set_property  target: "CloseButton"
  RectTransform.anchorMin = (0.9, 0.9)
  RectTransform.anchorMax = (1, 1)
```

- **MCP 호출**: 1(생성) + 1(컴포넌트) + 2(프로퍼티) = 4회

#### F-5-07: BuildingShopUI 컴포넌트 부착 및 참조 연결

```
add_component  target: "BuildingShopPanel"
  type: "SeedMind.UI.BuildingShopUI"

set_property  target: "BuildingShopPanel.BuildingShopUI"
  _buildingListParent = [BuildingListArea Transform 참조]
  _closeButton = [CloseButton Button 참조]
  _goldText = [GoldText TMP_Text 참조]
  _buildingSlotPrefab = [PFB_BuildingSlot 프리팹 참조]
```

[RISK] SerializeField 오브젝트 참조 설정이 MCP로 가능한지 미확인. 대안: BuildingShopUI.Awake()에서 `transform.Find()`로 자동 탐색, _buildingSlotPrefab은 Resources.Load()로 지연 로드.

- **MCP 호출**: 1(컴포넌트) + 4(프로퍼티) = 5회

#### F-5-08: 시설 슬롯 프리팹 생성

```
create_object
  name: "PFB_BuildingSlot"

// 하위 UI 요소: 아이콘, 이름, 비용, 해금 레벨 표시
create_object  name: "Icon", parent: "PFB_BuildingSlot"
create_object  name: "NameText", parent: "PFB_BuildingSlot"
create_object  name: "CostText", parent: "PFB_BuildingSlot"

save_as_prefab
  source: "PFB_BuildingSlot"
  path: "Assets/_Project/Prefabs/UI/PFB_BuildingSlot.prefab"
```

- **MCP 호출**: 4(오브젝트) + 1(프리팹 저장) - 일부 프로퍼티 설정 포함 = ~5회 (프로퍼티 별도 시 더 많음, 최소 기준)

#### F-5-09: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

#### F-5 검증 체크리스트

- [ ] `Canvas_Overlay/BuildingShopPanel` 존재, 기본 비활성
- [ ] 하위에 BuildingListArea, InfoArea, CloseButton 존재
- [ ] BuildingShopUI 컴포넌트 부착 확인
- [ ] CloseButton에 Button 컴포넌트 존재
- [ ] `PFB_BuildingSlot.prefab` 존재
- [ ] 콘솔 에러 없음

**F-5 MCP 호출 합계**: ~26회 (일부 프로퍼티 병합 시 축소 가능)

---

### F-6: 업그레이드 UI 연결

**목적**: 시설 인터랙션 시 표시되는 건물 정보/업그레이드 패널을 생성하고 BuildingInfoUI 컴포넌트를 연결한다.

**전제**: F-2 완료, Canvas_Overlay 존재

**의존 태스크**: F-2

#### F-6-01: BuildingInfoUI.cs 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/UI/BuildingInfoUI.cs"
  namespace: "SeedMind.UI"
  base_class: "MonoBehaviour"
```

SerializeField 목록:
- `_buildingNameText: TMP_Text`
- `_buildingLevelText: TMP_Text`
- `_effectDescriptionText: TMP_Text`
- `_upgradeButton: Button`
- `_upgradeCostText: TMP_Text`
- `_demolishButton: Button`
- `_closeButton: Button`

공개 메서드:
- `Open(BuildingInstance inst)` -- 시설 정보 표시
- `OnUpgradeClicked()` -- 업그레이드 실행
- `OnDemolishClicked()` -- 철거 실행
- `Close()`

(-> see `docs/systems/facilities-architecture.md` 섹션 3.4 업그레이드 흐름, 섹션 3.5 철거 흐름)

- **MCP 호출**: 1회

#### F-6-02: Unity 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

#### F-6-03: BuildingInfoPanel GameObject 생성

```
create_object
  name: "BuildingInfoPanel"
  parent: "Canvas_Overlay"

set_property  target: "BuildingInfoPanel"
  RectTransform.anchorMin = (0.25, 0.2)
  RectTransform.anchorMax = (0.75, 0.8)
  RectTransform.offsetMin = (0, 0)
  RectTransform.offsetMax = (0, 0)
  activeInHierarchy = false                  // 기본 비활성
```

- **MCP 호출**: 1(생성) + 5(프로퍼티) = 6회

#### F-6-04: 정보 표시 영역 및 버튼 생성

```
create_object  name: "NameText", parent: "BuildingInfoPanel"
create_object  name: "LevelText", parent: "BuildingInfoPanel"
create_object  name: "EffectText", parent: "BuildingInfoPanel"
create_object  name: "UpgradeButton", parent: "BuildingInfoPanel"
create_object  name: "DemolishButton", parent: "BuildingInfoPanel"
create_object  name: "CloseButton", parent: "BuildingInfoPanel"

add_component  target: "UpgradeButton"  type: "UnityEngine.UI.Button"
add_component  target: "DemolishButton"  type: "UnityEngine.UI.Button"
add_component  target: "CloseButton"  type: "UnityEngine.UI.Button"
```

- **MCP 호출**: 6(오브젝트) + 3(컴포넌트) = 9회 (프로퍼티 설정 포함 시 더 많으나, 최소 기준)

#### F-6-05: BuildingInfoUI 컴포넌트 부착

```
add_component  target: "BuildingInfoPanel"
  type: "SeedMind.UI.BuildingInfoUI"
```

[RISK] SerializeField 참조 연결은 MCP 제한 가능성 있음. 대안: Awake()에서 자동 탐색.

- **MCP 호출**: 1회

#### F-6-06: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

#### F-6 검증 체크리스트

- [ ] `Canvas_Overlay/BuildingInfoPanel` 존재, 기본 비활성
- [ ] 하위에 NameText, LevelText, EffectText, UpgradeButton, DemolishButton, CloseButton 존재
- [ ] BuildingInfoUI 컴포넌트 부착 확인
- [ ] 3개 Button 컴포넌트 존재
- [ ] 콘솔 에러 없음

**F-6 MCP 호출 합계**: ~14회 (프로퍼티 설정 일부 생략, 최소 기준. RectTransform 세부 설정 포함 시 증가)

---

### F-7: 시설 인터랙션 (레시피 실행 트리거)

**목적**: 플레이어가 시설에 접근하여 E키를 누르면 해당 시설의 UI가 열리도록 인터랙션 트리거를 연결한다.

**전제**: F-3 완료 (프리팹에 InteractionTrigger 존재), F-5/F-6 완료 (UI 존재)

**의존 태스크**: F-3, F-5, F-6

#### F-7-01: BuildingInteraction.cs 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/Building/BuildingInteraction.cs"
  namespace: "SeedMind.Building"
  base_class: "MonoBehaviour"
```

이 컴포넌트는 각 시설 프리팹에 부착되며, InteractionTrigger의 OnTriggerEnter/Exit를 감지한다.

주요 로직:
- 플레이어가 트리거 안에 있을 때 E키 입력 감지
- effectType에 따라 적절한 UI 호출:
  - AutoWater -> BuildingInfoUI.Open (물탱크 정보)
  - SeasonBypass -> 온실 내부 진입 (씬 전환 또는 카메라 전환)
  - Storage -> 창고 UI 열기
  - Processing -> ProcessingUI.Open (가공소는 ARC-014에서 별도 처리)

(-> see `docs/systems/facilities-architecture.md` 섹션 3.3, `docs/content/facilities.md` 섹션 3.5/4.7/5.3)

- **MCP 호출**: 1회

#### F-7-02: Unity 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

#### F-7-03: 프리팹에 BuildingInteraction 컴포넌트 부착

각 시설 프리팹(7종)을 열어 BuildingInteraction 컴포넌트를 부착한다.

```
// 7종 프리팹 각각에 대해:
add_component  target: "PFB_Bldg_{Type}"
  type: "SeedMind.Building.BuildingInteraction"
```

- **MCP 호출**: 7회

#### F-7-04: GrowthSystem에 ISeasonOverrideProvider 연결

GrowthSystem.cs를 수정하여 ISeasonOverrideProvider를 주입받도록 한다.

```
// GrowthSystem.cs 수정 사항:
private ISeasonOverrideProvider _seasonOverride;

public void SetSeasonOverrideProvider(ISeasonOverrideProvider provider)
{
    _seasonOverride = provider;
}
// ProcessGrowth에서 계절 검사 시 _seasonOverride.IsSeasonOverridden() 사용
```

(-> see `docs/systems/facilities-architecture.md` 섹션 5.2~5.3)

- **MCP 호출**: 1회 (스크립트 수정)

#### F-7 검증 체크리스트

- [ ] BuildingInteraction.cs 존재, 컴파일 에러 없음
- [ ] 7종 프리팹 모두에 BuildingInteraction 컴포넌트 부착
- [ ] GrowthSystem에 ISeasonOverrideProvider 주입 인터페이스 존재
- [ ] 트리거 영역 진입 시 적절한 UI 호출 로직 존재

**F-7 MCP 호출 합계**: 10회

---

### F-8: 통합 테스트 시퀀스

**목적**: 시설 시스템의 전체 동작을 Play Mode에서 검증한다. 건설, 업그레이드, 철거, 서브시스템 효과, 저장/로드를 포함한다.

**전제**: F-1 ~ F-7 모두 완료

**의존 태스크**: F-1 ~ F-7 전체

#### F-8-01: Play Mode 진입

```
enter_play_mode
```

- **MCP 호출**: 1회

#### F-8-02: 물탱크 건설 테스트

```
execute_method
  target: "BuildingManager"
  method: "DebugBuildInstant"
  args: ["building_water_tank", 3, 3]        // dataId, gridX, gridY

get_console_logs
  // 확인: "BuildingPlaced: building_water_tank at (3,3)"
  // 확인: "BuildingCompleted: building_water_tank"
```

> `DebugBuildInstant`는 건설 시간을 건너뛰고 즉시 완료하는 테스트용 메서드.  
> BuildingManager에 별도 추가 필요 (F-2-06에서 포함).

- **MCP 호출**: 2회

#### F-8-03: 물탱크 자동 물주기 테스트

```
execute_method
  target: "TimeManager"
  method: "DebugAdvanceDay"

get_console_logs
  // 확인: WaterTankSystem이 범위 내 타일에 물주기 수행 로그
  // 확인: 범위 내 Planted 타일이 Watered로 변경
```

- **MCP 호출**: 2회

#### F-8-04: 온실 건설 및 계절 우회 테스트

```
execute_method
  target: "BuildingManager"
  method: "DebugBuildInstant"
  args: ["building_greenhouse", 0, 0]

execute_method
  target: "TimeManager"
  method: "DebugSetSeason"
  args: [3]                                   // Winter = 3

get_console_logs
  // 확인: 온실 내부 타일에서 계절 제약 해제
  // 확인: GrowthSystem이 온실 내부 작물에 대해 성장 처리 수행
```

- **MCP 호출**: 3회

#### F-8-05: 창고 건설 및 아이템 저장/꺼내기 테스트

```
execute_method
  target: "BuildingManager"
  method: "DebugBuildInstant"
  args: ["building_storage", 6, 0]

execute_method
  target: "BuildingManager"
  method: "DebugStoreItem"
  args: ["building_storage", "crop_potato", 10, "Normal"]
  // StorageSystem은 MonoBehaviour 아님 → BuildingManager 프록시 사용 (F-2-06 참조)

execute_method
  target: "BuildingManager"
  method: "DebugRetrieveItem"
  args: ["building_storage", "crop_potato", 5]

get_console_logs
  // 확인: StoreItem 성공 (10개 저장)
  // 확인: RetrieveItem 성공 (5개 꺼냄, 잔여 5개)
```

- **MCP 호출**: 4회

#### F-8-06: 업그레이드 테스트

```
execute_method
  target: "BuildingManager"
  method: "DebugUpgrade"
  args: ["building_water_tank"]

get_console_logs
  // 확인: "BuildingUpgraded: building_water_tank, newLevel: 1"
  // 확인: 물탱크 범위 증가
```

- **MCP 호출**: 2회

#### F-8-07: 철거 테스트

```
execute_method
  target: "BuildingManager"
  method: "DebugDemolish"
  args: ["building_water_tank", 3, 3]

get_console_logs
  // 확인: "BuildingRemoved: building_water_tank"
  // 확인: 점유 타일이 Empty로 복원
  // 확인: 환급 골드 지급 (건설비 + 업그레이드비 합산의 50%)
```

- **MCP 호출**: 2회

#### F-8-08: 저장/로드 테스트

```
// 시설 여러 개 건설된 상태에서:
execute_method
  target: "SaveManager"
  method: "DebugSave"

exit_play_mode
enter_play_mode

execute_method
  target: "SaveManager"
  method: "DebugLoad"

get_console_logs
  // 확인: 모든 시설 상태 복원 (위치, 레벨, 가동 상태)
  // 확인: 창고 내 아이템 복원
  // 확인: 서브시스템 재등록 완료
```

- **MCP 호출**: 5회 (save + exit + enter + load + logs)

#### F-8-09: 경계 케이스 테스트

```
// 1) 겹침 배치 시도 -> CanPlace = false 확인
execute_method
  target: "BuildingManager"
  method: "CanPlace"
  args: ["building_water_tank", 3, 3]        // 이미 시설이 있는 위치

// 2) 골드 부족 시 건설 시도 -> 실패 확인
// 3) 해금 레벨 미달 시 건설 시도 -> 실패 확인
// 4) 최대 업그레이드 초과 시도 -> 실패 확인

get_console_logs
```

- **MCP 호출**: ~3회 (주요 케이스만)

#### F-8-10: Play Mode 종료 및 씬 저장

```
exit_play_mode
save_scene
```

- **MCP 호출**: 2회

#### F-8 검증 체크리스트

- [ ] 물탱크 건설 -> 자동 물주기 -> 범위 내 타일 Watered 확인
- [ ] 온실 건설 -> 겨울 계절 -> 온실 내부 작물 성장 확인
- [ ] 창고 건설 -> 아이템 저장/꺼내기 정상 동작
- [ ] 업그레이드 -> 레벨 증가, 효과 강화 확인
- [ ] 철거 -> 타일 복원, 환급 골드 확인 (-> see `docs/content/facilities.md` 섹션 2.5 for 환급률)
- [ ] 저장/로드 -> 모든 시설 상태 복원
- [ ] 겹침/골드부족/레벨미달 -> 적절한 실패 처리
- [ ] GrowthSystem priority(20)보다 BuildingManager priority(50)가 후순위 확인
- [ ] 콘솔 에러 없음

**F-8 MCP 호출 합계**: ~18회 (일부 경계 케이스는 선택적)

---

## 5. 태스크 실행 순서 요약

```
F-2 (스크립트 생성) ─┐
                     ├── F-1 (SO 에셋 생성) ─┐
                     │                       │
                     ├── F-3 (프리팹 생성) ───┤
                     │                       │
                     ├── F-5 (건설 UI) ───────┤
                     │                       │
                     └── F-6 (업그레이드 UI) ──┤
                                             │
                                             ├── F-4 (씬 배치) ─┐
                                             │                  │
                                             └── F-7 (인터랙션) ─┤
                                                                │
                                                                └── F-8 (통합 테스트)
```

**병렬 실행 가능**:
- F-1, F-3, F-5, F-6은 F-2 완료 후 병렬 실행 가능
- F-4, F-7은 F-1+F-3+F-5+F-6 완료 후 병렬 실행 가능
- F-8은 모든 태스크 완료 후 실행

---

## 6. Cross-references

- `docs/systems/facilities-architecture.md` (ARC-007) -- 시설 시스템 기술 아키텍처 (클래스 설계, Part II MCP 요약)
- `docs/content/facilities.md` (CON-002) -- 시설 콘텐츠 canonical (건설 요건, 업그레이드, tileSize, effectRadius)
- `docs/design.md` 섹션 4.6 -- 시설 목록 및 비용/해금 canonical
- `docs/pipeline/data-pipeline.md` 섹션 2.4 -- BuildingData SO 필드 canonical 정의
- `docs/pipeline/data-pipeline.md` 섹션 2.5 -- ProcessingRecipeData SO 필드 canonical 정의
- `docs/pipeline/data-pipeline.md` Part II 섹션 2.6 -- BuildingSaveData C# 클래스
- `docs/systems/farming-architecture.md` -- FarmGrid, FarmTile, GrowthSystem (물탱크/온실 연동)
- `docs/systems/economy-architecture.md` -- EconomyManager (건설 비용)
- `docs/systems/inventory-system.md` 섹션 2.3 -- 창고 슬롯 수 canonical
- `docs/systems/time-season-architecture.md` -- TimeManager (OnDayChanged priority, OnHourChanged)
- `docs/systems/project-structure.md` 섹션 2~4 -- 네임스페이스, 의존성, asmdef
- `docs/mcp/scene-setup-tasks.md` (ARC-002) -- 씬 기본 계층 구조
- `docs/mcp/farming-tasks.md` (ARC-003) -- 경작 시스템 MCP 태스크
- `docs/mcp/processing-tasks.md` (ARC-014) -- 가공 시스템 MCP 태스크 (가공소 레시피 SO/UI는 해당 문서에서 처리)
- `docs/content/processing-system.md` -- 가공 레시피 목록 canonical (레시피 직접 기재 금지, PATTERN-008)
- `docs/systems/economy-system.md` 섹션 2.5 -- 가공 공식 및 가격 산정 canonical

---

### Phase → 태스크 매핑 (facilities-architecture.md Part II 대응)

| Phase (architecture.md) | 대응 태스크 (this doc) |
|--------------------------|----------------------|
| Phase A: SO 데이터 파이프라인 | F-1 (SO 에셋), F-2 (스크립트) |
| Phase B: BuildingManager 기본 구조 | F-2 (BuildingManager, BuildingInstance, Events), F-4 (씬 배치) |
| Phase C: 물탱크 + 온실 서브시스템 | F-2 (S-07~S-09), F-7 (인터랙션 연동) |
| Phase D: 창고 + 가공소 서브시스템 | F-2 (S-10~S-12, S-15, S-16), F-7 일부 |
| Phase E: 저장/로드 + 통합 테스트 | F-8 (F-8-08 저장/로드 포함) |

---

## 7. Open Questions ([OPEN])

- [OPEN] 물탱크 범위 계산 방식: 맨해튼 거리(다이아몬드) vs 체비셰프 거리(정사각형). 현재 맨해튼으로 설계되어 있으나 확정 필요. (-> see `docs/systems/facilities-architecture.md` 섹션 4.2)
- [OPEN] Player -> Building 의존 금지로 인한 창고 아이템 이동 중재자 설계. UI 중재 방식이 가장 단순하나 확정 필요. (-> see `docs/systems/facilities-architecture.md` 섹션 6.3)
- [OPEN] 온실 내부 진입 방식: 별도 씬 전환 vs 카메라 전환. F-7에서 인터랙션 로직 구현 시 결정 필요.
- [OPEN] 건설 중 프리팹을 시설별로 분리할지, 공통 프리팹 1개에 런타임 스케일 조정으로 대응할지. 현재 공통 프리팹 방식으로 설계.

---

## 8. Risks ([RISK])

- [RISK] `create_scriptable_object`, `save_as_prefab` MCP 도구 가용 여부 미확인. SO 인스턴스 생성이나 프리팹 저장이 MCP에서 미지원인 경우, Editor 스크립트를 통한 우회 필요.
- [RISK] MCP로 SO 배열/참조 필드(`_allBuildingData[]`, `prefab`, `constructionPrefab`) 설정이 불가능할 수 있다. 대안: `Resources.LoadAll` 또는 Addressables로 런타임 자동 로드.
- [RISK] SerializeField 오브젝트 참조 설정(UI 컴포넌트의 Button, Transform 참조 등)이 MCP로 불가능할 수 있다. 대안: Awake()에서 `transform.Find()` 또는 `GetComponentInChildren<>()`으로 자동 탐색.
- [RISK] ISeasonOverrideProvider 주입 타이밍: GameManager 초기화 시 BuildingManager가 GrowthSystem에 GreenhouseSystem을 등록해야 한다. 초기화 순서가 잘못되면 NullReferenceException. (-> see `docs/systems/facilities-architecture.md` 섹션 5.3)
- [RISK] OnDayChanged priority 순서 변경 시 물탱크 효과 타이밍이 달라진다. GrowthSystem(20)보다 BuildingManager(50)가 먼저 실행되면 밸런스 파괴. (-> see `docs/systems/facilities-architecture.md` 섹션 4.3)
- [RISK] 총 227회 MCP 호출은 실행 시간이 길다. Editor 스크립트로 SO 에셋(F-1)과 프리팹(F-3)을 일괄 생성하면 약 141회를 ~10회로 압축 가능. MCP 단독 실행 대비 Editor 스크립트 우회를 강력 권장.
- [RISK] 창고 철거 시 내부 아이템 처리. 인벤토리가 가득 차면 아이템 손실 가능. 철거 전 인벤토리 여유 확인 또는 아이템 드롭 구현 필요. (-> see `docs/content/facilities.md` 섹션 2.5)

---

*이 문서는 Claude Code가 기술적 제약과 설계 목표를 고려하여 자율적으로 작성했습니다.*
