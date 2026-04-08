# 장식 시스템 MCP 태스크 시퀀스 (ARC-046)

> 작성: Claude Code (Sonnet 4.6) | 2026-04-08 | 문서 ID: ARC-046 | Phase 1

---

## Context

이 문서는 장식(Decoration) 시스템의 Unity MCP 구현 태스크 시퀀스를 정의한다. `docs/systems/decoration-architecture.md`(ARC-043) 섹션 3에서 요약된 D-A~D-E 태스크 그룹을 독립 문서로 분리하여, 스크립트 생성 → MonoBehaviour 설정 → SO 에셋 생성 → 씬 계층 배치 → 통합 검증까지의 전체 MCP 호출 시퀀스를 상세히 기술한다.

**상위 설계 문서**: `docs/systems/decoration-architecture.md` (ARC-043)  
**패턴 참조**: `docs/mcp/gathering-tasks.md` (ARC-032), `docs/mcp/fishing-tasks.md` (ARC-028)

---

## 1. 개요

### 목적

이 문서는 `docs/systems/decoration-architecture.md`(ARC-043) 섹션 3에서 요약된 MCP 구현 계획(D-A~D-E)을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 장식 시스템의 데이터 레이어(SO 에셋), 시스템 레이어(스크립트), 씬 계층 배치, 통합 검증을 완성한다.

### 의존성

```
장식 시스템 MCP 태스크 의존 관계:
├── SeedMind.Core        (SaveManager, ISaveable, Season enum, GameDataSO)
├── SeedMind.Farm        (FarmGrid -- IsFarmland(), 타일 점유 여부 확인)
├── SeedMind.Farm        (FarmZoneManager -- IsZoneUnlocked() 구역 해금 상태)
├── SeedMind.Player      (InventoryManager -- 구매 인벤토리 연동)
├── SeedMind.Economy     (EconomyManager -- buyPrice 처리)
└── SeedMind.UI          (장식 배치 UI, 인벤토리 팝업)
```

(→ see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 완료된 태스크 의존성

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조, SCN_Farm 기본 계층 (Managers, Environment, UI) |
| ARC-003 | `docs/mcp/farming-tasks.md` | Phase A~C 전체 | FarmGrid 타일맵, FarmTile 컴포넌트 |
| ARC-011 | `docs/mcp/inventory-tasks.md` | Phase A 이상 | InventoryManager, ItemType enum |
| ARC-008 | `docs/mcp/save-load-tasks.md` | Phase A 이상 | SaveManager, ISaveable, GameSaveData |
| ARC-023 | `docs/mcp/farm-expansion-tasks.md` | Phase A 이상 | FarmZoneManager, Zone 해금 시스템 |

### 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 | 비고 |
|--------------|------|------|
| `--- ENVIRONMENT ---` (씬 계층 부모) | ARC-002 Phase B | `Decorations` 노드를 그 하위에 추가 |
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B | `DecorationManager` GO를 그 하위에 추가 |
| `Canvas_Overlay` (UI 루트) | ARC-002 Phase B | |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A | |
| `SaveManager` (씬 오브젝트) | ARC-008 | |
| `ISaveable` 인터페이스 | ARC-008 | |
| `GameSaveData` — `decoration` 필드 | ARC-043 후속 FIX-111 | save-load-architecture.md에 이미 반영됨 |
| `SeedMind.Decoration` 네임스페이스/폴더/asmdef | ARC-043 후속 FIX-112 | project-structure.md에 이미 반영됨 |
| `DecorationItemData` / `DecorationConfig` SO 스키마 | ARC-043 후속 FIX-113 | data-pipeline.md 섹션 2.14~2.15에 이미 반영됨 |
| `FarmZoneManager` (씬 오브젝트) | ARC-023 | `IsZoneUnlocked()` 참조 |

### 총 MCP 호출 예상 수

| 태스크 그룹 | 설명 | 예상 호출 수 | 복잡도 |
|-------------|------|:------------:|:------:|
| D-A | ScriptableObject 클래스 생성 (7개 스크립트) | ~10회 | 중 |
| D-B | DecorationManager MonoBehaviour 생성 (4 태스크) | ~8회 | 중 |
| D-C | SO 에셋 인스턴스 생성 (DecorationItemData 29종 + DecorationConfig 1종) | ~65회 | 고 |
| D-D | SCN_Farm 씬 계층 설정 (Tilemap 레이어 3개 + Manager GO) | ~14회 | 중 |
| D-E | 통합 검증 (3개 체크) | ~8회 | 저 |
| **합계** | | **~105회** | |

[RISK] SO 에셋 인스턴스 생성(D-C)은 29개 DecorationItemData + 1개 DecorationConfig = 30개 에셋, 각 에셋당 평균 2회 호출(create + set_property) 기준 ~60회에 추가 필드 설정이 더해져 ~65회가 예상된다. Editor 스크립트(`CreateDecorationAssets.cs`)를 통한 일괄 생성으로 ~65회를 ~5회로 압축 가능. MCP 단독 실행 시 시간 비용이 크므로 Editor 스크립트 우회를 강력히 권장한다.

---

## 2. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋 폴더 생성 | D-A, D-C |
| `create_script` | C# 스크립트 파일 생성 | D-A, D-B |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | D-C |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | D-C, D-D |
| `create_object` | 빈 GameObject 생성 | D-D |
| `add_component` | MonoBehaviour 컴포넌트 부착 | D-B, D-D |
| `set_parent` | 오브젝트 부모 설정 | D-D |
| `add_tilemap_layer` | Tilemap 레이어(PathLayer, FenceLayer) 생성 | D-D |
| `save_scene` | 씬 저장 | D-D, D-E |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | D-E |
| `execute_method` | 런타임 메서드 호출 (배치 테스트) | D-E |
| `get_console_logs` | 콘솔 로그 확인 | D-E |
| `execute_menu_item` | 편집기 명령 실행 (컴파일 대기 등) | D-A, D-B |

[RISK] `create_scriptable_object` 도구의 가용 여부 및 파라미터 형식 사전 검증 필요. SO 인스턴스 생성이 MCP에서 미지원인 경우, Editor 스크립트를 통한 우회 필요. (→ see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)

[RISK] `add_tilemap_layer` 도구가 Rule Tile을 직접 지원하지 않을 경우, FenceLayer의 Rule Tile 연결은 Editor 스크립트(`SetupDecorationScene.cs`)로 대체 필요.

---

## 3. 필요 C# 스크립트 목록

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 태스크 순서대로 작성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/Decoration/Data/DecoCategoryType.cs` | `DecoCategoryType` (enum) | `SeedMind.Decoration.Data` | D-A |
| S-02 | `Scripts/Decoration/Data/EdgeDirection.cs` | `EdgeDirection` (enum) | `SeedMind.Decoration.Data` | D-A |
| S-03 | `Scripts/Decoration/Data/DecorationItemData.cs` | `DecorationItemData` (SO) | `SeedMind.Decoration.Data` | D-A |
| S-04 | `Scripts/Decoration/Data/DecorationConfig.cs` | `DecorationConfig` (SO) | `SeedMind.Decoration.Data` | D-A |
| S-05 | `Scripts/Decoration/DecorationInstance.cs` | `DecorationInstance` (Plain C#) | `SeedMind.Decoration` | D-A |
| S-06 | `Scripts/Decoration/DecorationSaveData.cs` | `DecorationSaveData`, `DecorationInstanceSave` (classes) | `SeedMind.Decoration` | D-A |
| S-07 | `Scripts/Decoration/DecorationEvents.cs` | `DecorationEvents` (static) | `SeedMind.Decoration` | D-A |
| S-08 | `Scripts/Decoration/DecorationManager.cs` | `DecorationManager` (MonoBehaviour) | `SeedMind.Decoration` | D-B |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: S-01~S-02 → S-03~S-07 → S-08. 각 그룹 사이에 Unity 컴파일 대기(`execute_menu_item`)가 필요하다.

---

## 4. 태스크 목록

---

### D-A: ScriptableObject 클래스 생성

**목적**: 장식 시스템의 데이터 정의 클래스를 생성한다. enum 2종, ScriptableObject 2종, Plain C# 3종 총 7개 파일.

**전제 조건**:
- ARC-002 완료 (폴더 구조 존재)
- `SeedMind.Core` 네임스페이스의 `ISaveable`, `Season` enum 컴파일 완료
- `UnityEngine.Tilemaps.TileBase` 참조 가능

**예상 MCP 호출**: ~10회 (2 create_folder + 7 create_script + 1 execute_menu_item)

#### D-A-01: 스크립트 폴더 생성

```
create_folder
  path: "Assets/_Project/Scripts/Decoration"

create_folder
  path: "Assets/_Project/Scripts/Decoration/Data"
```

- **MCP 호출**: 2회

#### D-A-02: enum 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/Decoration/Data/DecoCategoryType.cs"
  namespace: "SeedMind.Decoration.Data"
  // enum 값: Fence, Path, Light, Ornament, WaterDecor
  // -> see docs/systems/decoration-architecture.md 섹션 2.1

create_script
  path: "Assets/_Project/Scripts/Decoration/Data/EdgeDirection.cs"
  namespace: "SeedMind.Decoration.Data"
  // enum 값: None, North, South, East, West (펜스 배치용 방향)
  // -> see docs/systems/decoration-architecture.md 섹션 2.1
```

- **MCP 호출**: 2회

#### D-A-03: ScriptableObject 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/Decoration/Data/DecorationItemData.cs"
  namespace: "SeedMind.Decoration.Data"
  // ScriptableObject 상속
  // 필드 (20개): itemId, displayName, icon(Sprite), category(DecoCategoryType),
  //   buyPrice, isEdgePlaced, tileWidthX, tileHeightZ, unlockLevel, unlockZoneId,
  //   limitedSeason(Season), lightRadius, moveSpeedBonus, durabilityMax,
  //   prefab(GameObject), floorTile(TileBase), edgeTileH(TileBase),
  //   edgeTileV(TileBase), edgeTileCorner(TileBase), description
  // -> see docs/systems/decoration-architecture.md 섹션 2.2

create_script
  path: "Assets/_Project/Scripts/Decoration/Data/DecorationConfig.cs"
  namespace: "SeedMind.Decoration.Data"
  // ScriptableObject 상속
  // 필드 (5개): validHighlightColor, invalidHighlightColor,
  //   fenceDurabilityDecayPerSeason, fenceRepairCostRatio, pathSpeedBonusEnabled
  // -> see docs/systems/decoration-architecture.md 섹션 2.3
```

- **MCP 호출**: 2회

#### D-A-04: Plain C# 클래스/struct 생성

```
create_script
  path: "Assets/_Project/Scripts/Decoration/DecorationInstance.cs"
  namespace: "SeedMind.Decoration"
  // Plain C# 클래스 (런타임 인스턴스)
  // 필드 (7개): instanceId(int), data(DecorationItemData), cell(Vector3Int),
  //   edge(EdgeDirection), durability(int), colorVariantIndex(int),
  //   runtimeObject(GameObject)
  // -> see docs/systems/decoration-architecture.md 섹션 2.4

create_script
  path: "Assets/_Project/Scripts/Decoration/DecorationSaveData.cs"
  namespace: "SeedMind.Decoration"
  // [System.Serializable] class DecorationSaveData:
  //   decorations(List<DecorationInstanceSave>), nextInstanceId(int)  -- 2필드
  // [System.Serializable] class DecorationInstanceSave:
  //   instanceId(int), itemId(string), cellX(int), cellZ(int),
  //   edge(EdgeDirection), durability(int), colorVariantIndex(int)  -- 7필드
  // PATTERN-005: JSON 스키마와 C# 필드 일치 필수
  // -> see docs/systems/decoration-architecture.md 섹션 2.6

create_script
  path: "Assets/_Project/Scripts/Decoration/DecorationEvents.cs"
  namespace: "SeedMind.Decoration"
  // static class
  // 이벤트: OnDecorationPlaced(Action<DecorationPlacedInfo>),
  //         OnDecorationRemoved(Action<int>)
  // -> see docs/systems/decoration-architecture.md 섹션 2.7
```

- **MCP 호출**: 3회

#### D-A-05: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- 모든 D-A 스크립트 컴파일 완료 확인 후 D-B 진행
- **MCP 호출**: 1회

---

### D-B: DecorationManager MonoBehaviour 생성

**목적**: 장식 배치/제거/저장을 담당하는 핵심 MonoBehaviour `DecorationManager`를 생성하고 GameObject에 부착한다.

**전제 조건**:
- D-A 완료 (S-01~S-07 모두 컴파일 완료)
- `FarmGrid`, `FarmZoneManager` 컴파일 완료 (ARC-003, ARC-023)
- `ISaveable` 인터페이스 컴파일 완료 (ARC-008)

**예상 MCP 호출**: ~8회 (1 create_script + 1 execute_menu_item + 1 create_object + 1 add_component + 3 set_property + 1 save_scene)

#### D-B-01: DecorationManager 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/Decoration/DecorationManager.cs"
  namespace: "SeedMind.Decoration"
  // MonoBehaviour, Singleton, ISaveable 구현
  // SaveLoadOrder = 57
  // SerializedFields: _decoConfig(DecorationConfig), _farmGrid(FarmGrid),
  //   _fenceLayer(Tilemap), _pathLayer(Tilemap), _objectLayer(Transform)
  // Private: _items(Dictionary<int, DecorationInstance>), _nextInstanceId(int)
  // Public methods: CanPlace(), Place(), Remove(), IsOccupied(),
  //   IsFarmland(), IsWaterSource(), IsBuildingTile(),
  //   IsZoneUnlocked(), IsEdgeOccupied()
  // ISaveable: Save() -> DecorationSaveData, Load(DecorationSaveData)
  // -> see docs/systems/decoration-architecture.md 섹션 2.1
```

- **MCP 호출**: 1회

#### D-B-02: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

#### D-B-03: GameObject 생성 및 컴포넌트 부착

```
create_object
  name: "DecorationManager"
  scene: "SCN_Farm"

set_parent
  object: "DecorationManager"
  parent: "--- MANAGERS ---"

add_component
  object: "DecorationManager"
  component: "SeedMind.Decoration.DecorationManager"
```

- **MCP 호출**: 3회

#### D-B-04: SaveLoadOrder 설정

> **[NOTE]** `GameSaveData`에 `decoration: DecorationSaveData` 필드 추가는 FIX-111에서 `save-load-architecture.md`에 이미 반영 완료됨. 이 태스크는 런타임 시 SaveManager가 DecorationManager를 올바른 순서로 호출하도록 `saveLoadOrder` 필드를 설정하는 작업이다.

```
set_property
  object: "DecorationManager"
  component: "SeedMind.Decoration.DecorationManager"
  property: "saveLoadOrder"
  value: 57
  // SaveLoadOrder = 57 -> see docs/systems/save-load-architecture.md SaveLoadOrder 할당표
```

- **MCP 호출**: 1회 (SaveLoadOrder 설정)
- _decoConfig, _farmGrid, _fenceLayer, _pathLayer, _objectLayer 인스펙터 참조 연결은 D-C, D-D 완료 후 수행

#### D-B-05: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

---

### D-C: SO 에셋 인스턴스 생성 (29종 + Config 1종)

**목적**: `DecorationItemData` SO 에셋 29개와 `DecorationConfig` SO 에셋 1개를 생성하고 기본 필드를 설정한다.

**전제 조건**:
- D-A 완료 (DecorationItemData, DecorationConfig 클래스 컴파일 완료)
- `Assets/_Project/Data/Decoration/` 폴더 생성 필요

**예상 MCP 호출**: ~65회

[RISK] 29종 에셋의 `buyPrice`, `unlockLevel`, `unlockZoneId`, `lightRadius`, `moveSpeedBonus`, `durabilityMax` 등 콘텐츠 수치를 MCP로 직접 설정하는 경우, 각 값은 반드시 `docs/content/decoration-items.md` (CON-020) 섹션 1~5를 참조하여 입력해야 한다. 임의 수치 기재 금지 (PATTERN-007).

#### D-C-01: 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Decoration"

create_folder
  path: "Assets/_Project/Data/Decoration/Fence"

create_folder
  path: "Assets/_Project/Data/Decoration/Path"

create_folder
  path: "Assets/_Project/Data/Decoration/Light"

create_folder
  path: "Assets/_Project/Data/Decoration/Ornament"

create_folder
  path: "Assets/_Project/Data/Decoration/WaterDecor"
```

- **MCP 호출**: 6회

#### D-C-02: DecorationConfig SO 생성

```
create_scriptable_object
  type: "SeedMind.Decoration.Data.DecorationConfig"
  path: "Assets/_Project/Data/Config/SO_DecorationConfig.asset"

set_property
  asset: "Assets/_Project/Data/Config/SO_DecorationConfig.asset"
  property: "validHighlightColor"
  value: [색상값 -> see docs/systems/decoration-architecture.md 섹션 2.3]

set_property
  asset: "Assets/_Project/Data/Config/SO_DecorationConfig.asset"
  property: "invalidHighlightColor"
  value: [색상값 -> see docs/systems/decoration-architecture.md 섹션 2.3]

set_property
  asset: "Assets/_Project/Data/Config/SO_DecorationConfig.asset"
  property: "fenceDurabilityDecayPerSeason"
  value: [수치 -> see docs/systems/decoration-architecture.md 섹션 2.3]

set_property
  asset: "Assets/_Project/Data/Config/SO_DecorationConfig.asset"
  property: "fenceRepairCostRatio"
  value: [수치 -> see docs/systems/decoration-architecture.md 섹션 2.3]

set_property
  asset: "Assets/_Project/Data/Config/SO_DecorationConfig.asset"
  property: "pathSpeedBonusEnabled"
  value: [bool -> see docs/systems/decoration-architecture.md 섹션 2.3]
```

- **MCP 호출**: 6회

#### D-C-03: Fence SO 에셋 생성 (4종)

카테고리별 에셋 수: (→ see `docs/systems/decoration-system.md` 섹션 2.1)

```
// 네이밍 패턴: SO_Deco_Fence<이름>.asset
// 예시: SO_Deco_FenceWood.asset, SO_Deco_FenceStone.asset,
//       SO_Deco_FenceIron.asset, SO_Deco_FenceFloral.asset

create_scriptable_object
  type: "SeedMind.Decoration.Data.DecorationItemData"
  path: "Assets/_Project/Data/Decoration/Fence/SO_Deco_FenceWood.asset"

// 이하 4종 동일 패턴
// 각 에셋마다 set_property로 category = Fence, isEdgePlaced = true 설정
// 콘텐츠 수치 (buyPrice, unlockLevel 등):
//   -> see docs/content/decoration-items.md 섹션 1.1
```

- **MCP 호출**: ~12회 (4종 × 3회 평균)

#### D-C-04: Path SO 에셋 생성 (5종)

카테고리별 에셋 수: (→ see `docs/systems/decoration-system.md` 섹션 2.2)

```
// 네이밍 패턴: SO_Deco_Path<이름>.asset
// 예시: SO_Deco_PathDirt.asset, SO_Deco_PathGravel.asset,
//       SO_Deco_PathStone.asset, SO_Deco_PathBrick.asset, SO_Deco_PathWood.asset

create_scriptable_object
  type: "SeedMind.Decoration.Data.DecorationItemData"
  path: "Assets/_Project/Data/Decoration/Path/SO_Deco_PathGravel.asset"

// 이하 5종 동일 패턴
// 각 에셋마다 set_property로 category = Path, isEdgePlaced = false 설정
// 콘텐츠 수치 (moveSpeedBonus, buyPrice 등):
//   -> see docs/content/decoration-items.md 섹션 2.1
```

- **MCP 호출**: ~15회 (5종 × 3회 평균)

#### D-C-05: Light SO 에셋 생성 (4종)

카테고리별 에셋 수: (→ see `docs/systems/decoration-system.md` 섹션 2.3)

```
// 네이밍 패턴: SO_Deco_Light<이름>.asset
// 예시: SO_Deco_LightTorch.asset, SO_Deco_LightLantern.asset,
//       SO_Deco_LightStreet.asset, SO_Deco_LightCrystal.asset

create_scriptable_object
  type: "SeedMind.Decoration.Data.DecorationItemData"
  path: "Assets/_Project/Data/Decoration/Light/SO_Deco_LightLantern.asset"

// 이하 4종 동일 패턴
// 각 에셋마다 set_property로 category = Light, lightRadius 설정
// 콘텐츠 수치 (lightRadius, buyPrice, unlockLevel 등):
//   -> see docs/content/decoration-items.md 섹션 3.1
```

- **MCP 호출**: ~12회 (4종 × 3회 평균)

#### D-C-06: Ornament SO 에셋 생성 (11종)

카테고리별 에셋 수: (→ see `docs/systems/decoration-system.md` 섹션 2.4)

```
// 네이밍 패턴: SO_Deco_<itemId>.asset  (itemId 직접 사용)
// 예시: SO_Deco_OrnaScareRaven.asset, SO_Deco_OrnaWindmillS.asset 등
// itemId 전체 목록: (-> see docs/content/decoration-items.md 섹션 4.1)

create_scriptable_object
  type: "SeedMind.Decoration.Data.DecorationItemData"
  path: "Assets/_Project/Data/Decoration/Ornament/SO_Deco_OrnaScareRaven.asset"

// 이하 11종 동일 패턴 (OrnaFlowerPotS, OrnaFlowerPotL, OrnaBenchWood, OrnaStatueStone,
//   OrnaWindmillS, OrnaWellDecor, OrnaSignBoard, OrnaPumpkinLantern, OrnaSnowman, OrnaStatueGold)
// 각 에셋마다 set_property로 category = Ornament, tileWidthX, tileHeightZ 설정
// 콘텐츠 수치 (buyPrice, unlockLevel, tileWidthX, tileHeightZ 등):
//   -> see docs/content/decoration-items.md 섹션 4.1
```

- **MCP 호출**: ~11회 (11종 × 1회 create + 별도 set_property)

#### D-C-07: WaterDecor SO 에셋 생성 (5종)

카테고리별 에셋 수: (→ see `docs/systems/decoration-system.md` 섹션 2.5)

```
// 네이밍 패턴: SO_Deco_<itemId>.asset  (itemId 직접 사용)
// 예시: SO_Deco_WaterLotus.asset, SO_Deco_WaterFountainS.asset 등
// itemId 전체 목록: (-> see docs/content/decoration-items.md 섹션 5.1)

create_scriptable_object
  type: "SeedMind.Decoration.Data.DecorationItemData"
  path: "Assets/_Project/Data/Decoration/WaterDecor/SO_Deco_WaterLotus.asset"

// 이하 5종 동일 패턴 (WaterBridge, WaterFountainS, WaterFountainL, WaterDuck)
// 각 에셋마다 set_property로 category = WaterDecor 설정
// 콘텐츠 수치 (buyPrice, unlockLevel 등):
//   -> see docs/content/decoration-items.md 섹션 5.1
```

- **MCP 호출**: ~9회 (5종 × ~1.8회 평균)

[RISK] Editor 스크립트 일괄 생성 우회를 사용하는 경우, `CreateDecorationAssets.cs`에서 각 에셋의 콘텐츠 수치를 `docs/content/decoration-items.md` (CON-020) 기준으로 직접 작성해야 한다. MCP 단독으로 30개 에셋을 생성하면 ~65회 호출이 발생하므로, 가능하면 Editor 스크립트를 먼저 실행한 후 D-D로 넘어갈 것을 권장한다.

---

### D-D: SCN_Farm 씬 계층 설정

**목적**: `SCN_Farm` 씬에 장식 시스템 전용 Tilemap 레이어(PathLayer, FenceLayer)와 DecoObjects Transform을 생성하고, `DecorationManager`의 인스펙터 참조를 연결한다.

**전제 조건**:
- ARC-002 완료 (`--- ENVIRONMENT ---` 노드 존재)
- D-A, D-B 완료 (DecorationManager GameObject 존재)
- D-C 완료 (DecorationConfig SO 에셋 존재)

**예상 MCP 호출**: ~14회

**목표 씬 계층**:
```
--- ENVIRONMENT ---
├── Terrain (기존)
├── Decorations (ARC-043 신규)
│   ├── PathLayer (Tilemap, Sorting Layer: Decoration, Order: 1)
│   ├── FenceLayer (Tilemap, Sorting Layer: Decoration, Order: 2, Rule Tile)
│   └── DecoObjects (Transform)
└── Lighting (기존)
```

#### D-D-01: Decorations 부모 노드 생성

```
create_object
  name: "Decorations"
  scene: "SCN_Farm"

set_parent
  object: "Decorations"
  parent: "--- ENVIRONMENT ---"
```

- **MCP 호출**: 2회

#### D-D-02: PathLayer Tilemap 생성

```
create_object
  name: "PathLayer"
  scene: "SCN_Farm"

set_parent
  object: "PathLayer"
  parent: "Decorations"

add_component
  object: "PathLayer"
  component: "UnityEngine.Tilemaps.Tilemap"

add_component
  object: "PathLayer"
  component: "UnityEngine.Tilemaps.TilemapRenderer"

set_property
  object: "PathLayer"
  component: "UnityEngine.Tilemaps.TilemapRenderer"
  property: "sortingLayerName"
  value: "Decoration"

set_property
  object: "PathLayer"
  component: "UnityEngine.Tilemaps.TilemapRenderer"
  property: "sortingOrder"
  value: 1
```

- **MCP 호출**: 6회

#### D-D-03: FenceLayer Tilemap 생성

```
create_object
  name: "FenceLayer"
  scene: "SCN_Farm"

set_parent
  object: "FenceLayer"
  parent: "Decorations"

add_component
  object: "FenceLayer"
  component: "UnityEngine.Tilemaps.Tilemap"

add_component
  object: "FenceLayer"
  component: "UnityEngine.Tilemaps.TilemapRenderer"

set_property
  object: "FenceLayer"
  component: "UnityEngine.Tilemaps.TilemapRenderer"
  property: "sortingLayerName"
  value: "Decoration"

set_property
  object: "FenceLayer"
  component: "UnityEngine.Tilemaps.TilemapRenderer"
  property: "sortingOrder"
  value: 2
```

- [RISK] Rule Tile 연결(`RuleTile` 에셋 → FenceLayer)은 MCP `set_property`로 직접 지원이 불확실. 지원되지 않는 경우 Editor 스크립트로 대체.
- **MCP 호출**: 6회

#### D-D-04: DecoObjects Transform 생성

```
create_object
  name: "DecoObjects"
  scene: "SCN_Farm"

set_parent
  object: "DecoObjects"
  parent: "Decorations"
```

- **MCP 호출**: 2회

#### D-D-05: DecorationManager 인스펙터 참조 연결

```
set_property
  object: "DecorationManager"
  component: "SeedMind.Decoration.DecorationManager"
  property: "_decoConfig"
  value: [ref: "Assets/_Project/Data/Config/SO_DecorationConfig.asset"]

set_property
  object: "DecorationManager"
  component: "SeedMind.Decoration.DecorationManager"
  property: "_farmGrid"
  value: [ref: "FarmGrid" GameObject]

set_property
  object: "DecorationManager"
  component: "SeedMind.Decoration.DecorationManager"
  property: "_fenceLayer"
  value: [ref: "FenceLayer" Tilemap component]

set_property
  object: "DecorationManager"
  component: "SeedMind.Decoration.DecorationManager"
  property: "_pathLayer"
  value: [ref: "PathLayer" Tilemap component]

set_property
  object: "DecorationManager"
  component: "SeedMind.Decoration.DecorationManager"
  property: "_objectLayer"
  value: [ref: "DecoObjects" Transform]
```

- **MCP 호출**: 5회

#### D-D-06: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

---

### D-E: 통합 검증

**목적**: 장식 시스템이 올바르게 초기화되고, 배치/제거/저장-로드 흐름이 정상 동작하는지 확인한다.

**전제 조건**: D-A, D-B, D-C, D-D 모두 완료

**예상 MCP 호출**: ~8회

#### D-E-01: 플레이 모드 진입 및 초기화 확인

```
enter_play_mode

get_console_logs
  // 확인 항목:
  // - "[DecorationManager] Initialized. Items loaded: 0" (첫 실행)
  // - 컴파일/Null Reference 에러 없음
  // -> see docs/systems/decoration-architecture.md 섹션 4 (검증 체크리스트)
```

- **MCP 호출**: 2회

#### D-E-02: 장식 배치 테스트

```
execute_method
  object: "DecorationManager"
  method: "Place"
  args:
    itemId: [SO_Deco_PathGravel의 itemId -> see docs/content/decoration-items.md 섹션 2.1]
    cell: [1, 0, 1]
    edge: "None"
  // 기대 결과: OnDecorationPlaced 이벤트 발생, 콘솔에 배치 로그

get_console_logs
  // 확인: "[DecorationManager] Placed itemId=X at (1,0,1)"
  // OnDecorationPlaced 이벤트 정상 수신 확인
```

- **MCP 호출**: 2회

#### D-E-03: 저장-로드 사이클 테스트

```
execute_method
  object: "SaveManager"
  method: "SaveAll"
  // 기대: DecorationSaveData가 GameSaveData.decoration에 직렬화

execute_method
  object: "DecorationManager"
  method: "ClearAll"
  // 배치 데이터 초기화

execute_method
  object: "SaveManager"
  method: "LoadAll"
  // 기대: 저장된 DecorationSaveData 복원, _items 딕셔너리 재구성

get_console_logs
  // 확인: "[DecorationManager] Loaded X decorations"
  // SaveLoadOrder=57 순서로 로드됨 확인

exit_play_mode
```

- **MCP 호출**: 4회

#### D-E 검증 체크리스트

| # | 항목 | 확인 방법 |
|---|------|---------|
| 1 | DecorationManager Singleton 초기화 | 콘솔 Init 로그 |
| 2 | CanPlace() — 기존 타일 점유 시 false 반환 | execute_method + 콘솔 |
| 3 | Place() — 정상 배치 후 OnDecorationPlaced 이벤트 발생 | get_console_logs |
| 4 | Remove() — 정상 제거 후 OnDecorationRemoved 이벤트 발생 | get_console_logs |
| 5 | Path 배치 시 floorTile이 PathLayer에 정상 적용 | 씬 뷰 확인 |
| 6 | Fence 배치 시 edgeTile이 FenceLayer Rule Tile에 반응 | 씬 뷰 확인 |
| 7 | Save/Load 사이클 후 배치 수 일치 | 콘솔 로그 비교 |
| 8 | FarmGrid 타일 위 배치 거부 (IsFarmland=true인 셀) | execute_method |
| 9 | 미해금 Zone 배치 거부 (IsZoneUnlocked=false인 Zone) | execute_method |

---

## 5. Cross-references

| 문서 | 역할 |
|------|------|
| `docs/systems/decoration-architecture.md` (ARC-043) | 상위 설계 — 클래스 계층, 필드 목록, 이벤트 흐름 |
| `docs/systems/decoration-system.md` | 설계 근거(rationale) — 카테고리 개요, 배치 메카닉, 경제 연동 원칙 |
| `docs/content/decoration-items.md` (CON-020) | 콘텐츠 canonical — 29종 itemId, buyPrice, 파라미터 수치 확정값 |
| `docs/pipeline/data-pipeline.md` 섹션 2.14~2.15 | SO 스키마 정의 (DecorationItemData, DecorationConfig) |
| `docs/systems/save-load-architecture.md` | GameSaveData.decoration 필드 (FIX-111 반영) |
| `docs/systems/project-structure.md` | SeedMind.Decoration 네임스페이스/폴더/asmdef (FIX-112 반영) |
| `docs/mcp/scene-setup-tasks.md` (ARC-002) | 씬 기본 계층 (전제 의존성) |
| `docs/mcp/farming-tasks.md` (ARC-003) | FarmGrid (전제 의존성) |
| `docs/mcp/save-load-tasks.md` (ARC-008) | SaveManager, ISaveable (전제 의존성) |
| `docs/mcp/inventory-tasks.md` (ARC-011) | InventoryManager (전제 의존성) |
| `docs/mcp/farm-expansion-tasks.md` (ARC-023) | FarmZoneManager (전제 의존성) |
| `docs/mcp/gathering-tasks.md` (ARC-032) | 패턴 참조 |
| `docs/mcp/fishing-tasks.md` (ARC-028) | 패턴 참조 |

---

## Open Questions

- [OPEN] `add_tilemap_layer` MCP 도구가 Rule Tile 직접 연결을 지원하는지 확인 필요. 미지원 시 Editor 스크립트 대체 방안 사전 준비.
- [OPEN] `Decoration` Sorting Layer가 ARC-002 씬 설정 단계에서 이미 정의되어 있는지 확인 필요. 없으면 D-D-02 전에 Sorting Layer 생성 태스크 추가.
- [OPEN] `colorVariantIndex` 기능 범위 — 단일 색상 변형만 지원할지 다중 팔레트를 지원할지 DES 확정 전까지 DecorationInstance에 필드만 유지, 실제 변형 로직은 추후 구현.
