# 작물 성장 시스템 MCP 태스크 시퀀스 (ARC-050)

> 작성: Claude Code (Sonnet 4.6) | 2026-04-09 | 문서 ID: ARC-050 | Phase 1

---

## Context

이 문서는 작물 성장 시스템의 Unity MCP 구현 태스크 시퀀스를 정의한다. `docs/systems/crop-growth-architecture.md`(ARC-005) 섹션 9 (Phase A~C)에서 요약된 구현 계획을 독립 문서로 분리하여, CropData SO 에셋 생성 → CropInstance 프리팹 및 비주얼 → GrowthSystem 컴포넌트 연동 및 통합 테스트까지의 전체 MCP 호출 시퀀스를 상세히 기술한다.

**상위 설계 문서**: `docs/systems/crop-growth-architecture.md` (ARC-005)  
**작물 수치 canonical 출처**: `docs/systems/crop-growth.md` (DES-003), `docs/design.md` 섹션 4.2  
**패턴 참조**: `docs/mcp/energy-tasks.md` (ARC-047), `docs/mcp/farming-tasks.md` (ARC-003)

---

## 1. 개요

### 목적

이 문서는 `docs/systems/crop-growth-architecture.md`(ARC-005) 섹션 9의 Phase A~C를 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: MCP 명령만으로 CropData ScriptableObject 에셋(8종), CropInstance/GiantCropInstance 스크립트, 신규 작물 단계별 프리팹(5종 × 4단계), 거대 작물 프리팹(2종), GrowthSystem 완성, 통합 테스트를 완료한다.

### 의존성

```
작물 성장 시스템 MCP 태스크 의존 관계:
├── SeedMind.Farm         (FarmGrid, FarmTile, FarmingSystem — ARC-003 결과물)
├── SeedMind.Farm.Data    (CropData SO 클래스 — ARC-003 Phase A에서 초기 정의)
├── SeedMind.Core         (TimeManager, SeasonFlag, FarmEvents)
├── SeedMind.Player.Data  (FertilizerData — 비료 시스템 SO)
└── SeedMind.UI           (HUD — OnCropStageChanged, OnGiantCropFormed 소비)
```

(→ see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 완료된 태스크 의존성

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조(`Assets/_Project/`), SCN_Farm 기본 계층, SCN_Test_FarmGrid |
| ARC-003 | `docs/mcp/farming-tasks.md` | Phase A~C 전체 | FarmGrid, FarmTile, FarmingSystem, GrowthSystem(빈 껍데기), CropData.cs 초기 정의, 감자/당근/토마토 프리팹 |

### 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 문서 | 비고 |
|--------------|----------|------|
| `Assets/_Project/Scripts/Farm/` 폴더 | ARC-002 Phase A | CropInstance.cs는 이 폴더에 신규 생성 |
| `Assets/_Project/Scripts/Farm/Data/` 폴더 | ARC-002 Phase A | CropData.cs, Quality.cs, GrowthResult.cs 위치 |
| `Assets/_Project/Data/Crops/` 폴더 | ARC-003 Phase A | SO 에셋 저장 위치 |
| `CropData.cs` (초기 버전) | ARC-003 Phase A | 이번 CG-A에서 필드 확장만 수행 — 재생성 금지 |
| `FarmingSystem` (씬 오브젝트) | ARC-003 Phase C | GrowthSystem과 동일 GO에 배치 또는 별도 GO — 씬 확인 후 결정 |
| `GrowthSystem` (씬 오브젝트) | ARC-002 Phase B | 빈 껍데기 GO로 존재. 이번 CG-C에서 스크립트 부착 |
| `SO_Crop_Potato.asset` | ARC-003 Phase A | 이번 CG-A에서 신규 필드값만 설정 — 재생성 금지 |
| `SO_Crop_Carrot.asset` | ARC-003 Phase A | 동일 |
| `SO_Crop_Tomato.asset` | ARC-003 Phase A | 동일 |
| `PFB_Crop_Potato_Stage0~3` | ARC-003 Phase B | 감자 4단계 프리팹 — 재생성 금지 |
| `PFB_Crop_Carrot_Stage0~3` | ARC-003 Phase B | 당근 4단계 프리팹 — 재생성 금지 |
| `PFB_Crop_Tomato_Stage0~3` | ARC-003 Phase B | 토마토 4단계 프리팹 — 재생성 금지 |
| `FarmEvents.cs` | ARC-003 Phase C | 이번 CG-C에서 이벤트 필드 추가만 수행 |

### 총 MCP 호출 예상 수

| 태스크 그룹 | 설명 | 예상 호출 수 | 복잡도 |
|-------------|------|:------------:|:------:|
| CG-A | CropData.cs 확장 + enum 2종 + SO 에셋 8종 (기존 3종 업데이트 + 신규 5종) | ~22회 | 중 |
| CG-B | CropInstance.cs + GiantCropInstance.cs + 신규 작물 프리팹(5종×4단계=20개) + Material 5종 + 거대 작물 프리팹 2종 + SO 배열 연결 | ~45회 | 고 |
| CG-C | GrowthSystem.cs 확장 + FarmEvents.cs 추가 + FarmTile.TryHarvest 수정 + 통합 테스트 | ~12회 | 중 |
| **합계** | | **~79회** | |

[RISK] CropData SO의 `growthStagePrefabs[]` 배열과 `giantCropPrefab` 참조 설정이 MCP `set_property`로 지원되는지 사전 검증 필요. 미지원 시 Editor 스크립트(`InitCropData.cs`) 일괄 설정으로 우회한다 (→ see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정).

---

## 2. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋 폴더 생성 | CG-A, CG-B |
| `create_script` | C# 스크립트 파일 생성/수정 | CG-A, CG-B, CG-C |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | CG-A |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | CG-A, CG-B, CG-C |
| `create_object` | 빈 GameObject 또는 Primitive 생성 | CG-B |
| `add_component` | MonoBehaviour 컴포넌트 부착 | CG-B, CG-C |
| `create_prefab` | GameObject → Prefab 변환 | CG-B |
| `create_material` | Material 에셋 생성 | CG-B |
| `set_parent` | 오브젝트 부모 설정 | CG-B |
| `save_scene` | 씬 저장 | CG-C |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | CG-C |
| `execute_method` | 런타임 메서드 호출 (ProcessNewDay, TryHarvest 테스트) | CG-C |
| `get_console_logs` | 콘솔 로그 확인 | CG-A, CG-C |
| `execute_menu_item` | 컴파일 대기 | CG-A, CG-B, CG-C |

[RISK] `create_scriptable_object` 도구의 가용 여부 및 파라미터 형식 사전 검증 필요. SO 인스턴스 생성이 MCP에서 미지원인 경우, Editor 스크립트를 통한 우회 필요. (→ see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)

---

## 3. 필요 C# 스크립트 목록

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 태스크 순서대로 작성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 | 비고 |
|---|----------|--------|-------------|-----------|------|
| S-01 | `Scripts/Farm/Data/Quality.cs` | `Quality` (enum) | `SeedMind.Farm.Data` | CG-A | Normal / Silver / Gold / Iridium |
| S-02 | `Scripts/Farm/Data/GrowthResult.cs` | `GrowthResult` (enum) | `SeedMind.Farm.Data` | CG-A | None / StageAdvanced / Completed / Withered |
| S-03 | `Scripts/Farm/Data/CropData.cs` | `CropData` (SO) | `SeedMind.Farm.Data` | CG-A | 기존 파일에 필드 추가 (재생성 금지) |
| S-04 | `Scripts/Farm/CropInstance.cs` | `CropInstance` (Plain C# Serializable) | `SeedMind.Farm` | CG-B | |
| S-05 | `Scripts/Farm/GiantCropInstance.cs` | `GiantCropInstance` (Plain C# class) | `SeedMind.Farm` | CG-B | |
| S-06 | `Scripts/Farm/GrowthSystem.cs` | `GrowthSystem` (MonoBehaviour) | `SeedMind.Farm` | CG-C | 기존 빈 껍데기에 로직 추가 |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] S-03(CropData.cs) 필드 추가 시 ARC-003에서 이미 사용 중인 기존 필드(cropName, cropId, seedPrice 등)와 충돌하지 않도록 추가 필드만 append. 기존 인스펙터 직렬화 데이터 손상 주의.

---

## 4. 태스크 목록

---

### CG-A: CropData SO 에셋 생성

**목적**: Quality/GrowthResult enum 정의, CropData.cs에 다중 수확/거대 작물 필드 추가, SO 에셋 8종(기존 3종 업데이트 + 신규 5종) 생성 및 기본 필드 설정.

**전제 조건**:
- ARC-002 Phase A 완료 (`Assets/_Project/Data/Crops/` 폴더 존재)
- ARC-003 Phase A 완료 (CropData.cs 초기 버전 컴파일 완료, SO_Crop_Potato/Carrot/Tomato.asset 존재)

**예상 MCP 호출**: ~22회

#### CG-A-01: Quality enum 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/Farm/Data/Quality.cs"
  namespace: "SeedMind.Farm.Data"
  // enum 값: Normal(0), Silver(1), Gold(2), Iridium(3)
  // 가격 배수: Normal=x1.0, Silver=x1.25, Gold=x1.5, Iridium=x2.0
  // (→ see docs/systems/crop-growth-architecture.md 섹션 1 Quality enum)
```

- **MCP 호출**: 1회

#### CG-A-02: GrowthResult enum 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/Farm/Data/GrowthResult.cs"
  namespace: "SeedMind.Farm.Data"
  // enum 값: None(0), StageAdvanced(1), Completed(2), Withered(3)
  // (→ see docs/systems/crop-growth-architecture.md 섹션 1 GrowthResult enum)
```

- **MCP 호출**: 1회

#### CG-A-03: CropData.cs 필드 확장

기존 `CropData.cs`(ARC-003 정의)에 다중 수확 및 거대 작물 필드를 추가한다. 재생성이 아닌 필드 append.

```
create_script (update mode)
  path: "Assets/_Project/Scripts/Farm/Data/CropData.cs"
  // 추가 Header: "수확"
  //   isReharvestable: bool (false)
  //   reharvestDays: int (0)
  // 추가 Header: "거대 작물"
  //   giantCropPrefab: GameObject (null)
  //   giantCropMinSize: int (3)
  //   giantCropChance: float
  //     (→ see docs/systems/crop-growth.md 섹션 5.1 for giantCropChance 기본값)
  // 추가 Header: "비주얼"
  //   soilMaterial: Material (null)
  // (→ see docs/systems/crop-growth-architecture.md 섹션 3 CropData 확장 필드)

execute_menu_item
  menuPath: "Assets/Refresh"
  // 컴파일 완료 대기
```

- **MCP 호출**: 2회

#### CG-A-04: 기존 SO 에셋 신규 필드 설정 (기존 3종)

ARC-003에서 생성된 감자/당근/토마토 SO 에셋에 새 필드값을 설정한다.

```
// SO_Crop_Potato.asset
set_property
  target: "Assets/_Project/Data/Crops/SO_Crop_Potato.asset"
  property: "isReharvestable"
  value: false
set_property
  target: "Assets/_Project/Data/Crops/SO_Crop_Potato.asset"
  property: "reharvestDays"
  value: 0
// giantCropPrefab = null (기본값 유지), giantCropChance 값
// (→ see docs/systems/crop-growth.md 섹션 5.1)

// SO_Crop_Carrot.asset — 동일 패턴
set_property
  target: "Assets/_Project/Data/Crops/SO_Crop_Carrot.asset"
  property: "isReharvestable"
  value: false

// SO_Crop_Tomato.asset — 단일 수확 (crop-growth.md 섹션 4.1 확인)
set_property
  target: "Assets/_Project/Data/Crops/SO_Crop_Tomato.asset"
  property: "isReharvestable"
  value: false
  // [OPEN - to be filled after DES-003 토마토 다중 수확 여부 재확인 후 설정]
  // (→ see docs/systems/crop-growth-architecture.md 섹션 7.1 [OPEN] 항목)
```

- **MCP 호출**: ~5회

#### CG-A-05: 신규 SO 에셋 생성 (5종)

```
// 옥수수
create_scriptable_object
  className: "SeedMind.Farm.Data.CropData"
  assetPath: "Assets/_Project/Data/Crops/SO_Crop_Corn.asset"

// 딸기
create_scriptable_object
  className: "SeedMind.Farm.Data.CropData"
  assetPath: "Assets/_Project/Data/Crops/SO_Crop_Strawberry.asset"

// 호박
create_scriptable_object
  className: "SeedMind.Farm.Data.CropData"
  assetPath: "Assets/_Project/Data/Crops/SO_Crop_Pumpkin.asset"

// 해바라기
create_scriptable_object
  className: "SeedMind.Farm.Data.CropData"
  assetPath: "Assets/_Project/Data/Crops/SO_Crop_Sunflower.asset"

// 수박
create_scriptable_object
  className: "SeedMind.Farm.Data.CropData"
  assetPath: "Assets/_Project/Data/Crops/SO_Crop_Watermelon.asset"
```

- **MCP 호출**: 5회

#### CG-A-06: 신규 SO 에셋 기본 필드 설정

각 SO 에셋의 cropName, cropId, allowedSeasons, isReharvestable 등 기본 식별 필드를 설정한다. 수치(growthDays, sellPrice, seedPrice, reharvestDays, giantCropChance)는 canonical 문서 참조 — 직접 기재 금지.

```
// SO_Crop_Corn
set_property  target: ".../SO_Crop_Corn.asset"  property: "cropName"  value: "옥수수"
set_property  target: ".../SO_Crop_Corn.asset"  property: "cropId"    value: "corn"
set_property  target: ".../SO_Crop_Corn.asset"  property: "isReharvestable"  value: false
// growthDays, sellPrice, seedPrice, allowedSeasons
// (→ see docs/design.md 섹션 4.2 and docs/systems/crop-growth.md 섹션 3.1)

// SO_Crop_Strawberry
set_property  target: ".../SO_Crop_Strawberry.asset"  property: "cropName"  value: "딸기"
set_property  target: ".../SO_Crop_Strawberry.asset"  property: "cropId"    value: "strawberry"
set_property  target: ".../SO_Crop_Strawberry.asset"  property: "isReharvestable"  value: true
// reharvestDays (→ see docs/systems/crop-growth.md 섹션 4.2)
// growthDays, sellPrice, seedPrice, allowedSeasons (→ see docs/design.md 섹션 4.2)

// SO_Crop_Pumpkin
set_property  target: ".../SO_Crop_Pumpkin.asset"  property: "cropName"  value: "호박"
set_property  target: ".../SO_Crop_Pumpkin.asset"  property: "cropId"    value: "pumpkin"
set_property  target: ".../SO_Crop_Pumpkin.asset"  property: "isReharvestable"  value: false
set_property  target: ".../SO_Crop_Pumpkin.asset"  property: "giantCropMinSize"  value: 3
// giantCropChance (→ see docs/systems/crop-growth.md 섹션 5.1)
// growthDays, sellPrice, seedPrice, allowedSeasons (→ see docs/design.md 섹션 4.2)

// SO_Crop_Sunflower
set_property  target: ".../SO_Crop_Sunflower.asset"  property: "cropName"  value: "해바라기"
set_property  target: ".../SO_Crop_Sunflower.asset"  property: "cropId"    value: "sunflower"
set_property  target: ".../SO_Crop_Sunflower.asset"  property: "isReharvestable"  value: false
// growthDays, sellPrice, seedPrice, allowedSeasons (→ see docs/design.md 섹션 4.2)

// SO_Crop_Watermelon
set_property  target: ".../SO_Crop_Watermelon.asset"  property: "cropName"  value: "수박"
set_property  target: ".../SO_Crop_Watermelon.asset"  property: "cropId"    value: "watermelon"
set_property  target: ".../SO_Crop_Watermelon.asset"  property: "isReharvestable"  value: false
set_property  target: ".../SO_Crop_Watermelon.asset"  property: "giantCropMinSize"  value: 3
// giantCropChance (→ see docs/systems/crop-growth.md 섹션 5.1)
// growthDays, sellPrice, seedPrice, allowedSeasons (→ see docs/design.md 섹션 4.2)
```

- **MCP 호출**: ~8회 (각 SO당 비수치 필드 우선 설정, 수치는 canonical 조회 후 입력)

**검증**:

```
get_console_logs
  // CropData 필드 출력 확인
  // 8종 SO 에셋 Inspector 값 검토
```

---

### CG-B: CropInstance 프리팹 + 단계별 비주얼

**목적**: CropInstance.cs 및 GiantCropInstance.cs 스크립트 생성. 신규 5종 작물의 4단계 Placeholder 프리팹(20개), Material(5종), 거대 작물 프리팹(2종) 생성. 각 SO의 `growthStagePrefabs[]` 배열 및 `giantCropPrefab` 연결.

**전제 조건**:
- CG-A 전체 완료 (CropData.cs 확장 버전 컴파일 완료, Quality/GrowthResult enum 존재)
- ARC-003 Phase B 완료 (감자/당근/토마토 Stage0~3 프리팹 존재)

**예상 MCP 호출**: ~45회

#### CG-B-01: CropInstance.cs 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/Farm/CropInstance.cs"
  namespace: "SeedMind.Farm"
  // [System.Serializable] Plain C# class
  // 필드: _cropData(CropData), _currentGrowthDays(float), _totalGrowthDays(int),
  //       _currentStage(int), _isWatered(bool), _dryDayCount(int),
  //       _fertilizer(FertilizerData nullable), _totalElapsedDays(int),
  //       _wateredDayCount(int), _plantedSeason(SeasonFlag),
  //       _isGiantPart(bool), _giantCropRef(GiantCropInstance)
  // 메서드: Grow(), MarkWatered(), ResetWatered(), IncrementDryDay(),
  //        TrackDay(), ResetForReharvest(), DetermineQuality(), CalculateYield()
  // (→ see docs/systems/crop-growth-architecture.md 섹션 2)

execute_menu_item
  menuPath: "Assets/Refresh"
  // 컴파일 완료 대기
```

- **MCP 호출**: 2회

#### CG-B-02: GiantCropInstance.cs 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/Farm/GiantCropInstance.cs"
  namespace: "SeedMind.Farm"
  // Plain C# class (비직렬화)
  // 필드: cropData(CropData), originTile(Vector2Int), size(int),
  //       memberTiles(List<FarmTile>), giantPrefabInstance(GameObject)
  // 메서드: Harvest(), Destroy(), AddMemberTile(), SpawnVisual()
  // (→ see docs/systems/crop-growth-architecture.md 섹션 1 GiantCropInstance)

execute_menu_item
  menuPath: "Assets/Refresh"
```

- **MCP 호출**: 2회

#### CG-B-03: 신규 작물 Material 생성 (5종)

```
create_material
  path: "Assets/_Project/Materials/Crops/M_Crop_Corn_Early.mat"
  shader: "Universal Render Pipeline/Lit"
  baseColor: "#4CAF50"  // 초기 녹색

create_material
  path: "Assets/_Project/Materials/Crops/M_Crop_Corn_Mature.mat"
  shader: "Universal Render Pipeline/Lit"
  baseColor: "#FFD54F"  // 성숙: 금색 계열
  // (→ see docs/systems/crop-growth-architecture.md 섹션 9 Phase B Step B-4)

// 딸기 (Early: 녹색, Mature: 빨간색)
create_material  path: ".../M_Crop_Strawberry_Early.mat"  baseColor: "#4CAF50"
create_material  path: ".../M_Crop_Strawberry_Mature.mat"  baseColor: "#E53935"

// 호박 (Early: 녹색, Mature: 주황색)
create_material  path: ".../M_Crop_Pumpkin_Early.mat"  baseColor: "#4CAF50"
create_material  path: ".../M_Crop_Pumpkin_Mature.mat"  baseColor: "#FF6F00"

// 해바라기 (Early: 녹색, Mature: 노란색)
create_material  path: ".../M_Crop_Sunflower_Early.mat"  baseColor: "#4CAF50"
create_material  path: ".../M_Crop_Sunflower_Mature.mat"  baseColor: "#FDD835"

// 수박 (Early: 녹색, Mature: 진한 초록)
create_material  path: ".../M_Crop_Watermelon_Early.mat"  baseColor: "#4CAF50"
create_material  path: ".../M_Crop_Watermelon_Mature.mat"  baseColor: "#2E7D32"
```

- **MCP 호출**: 10회

#### CG-B-04: 신규 작물 단계별 Placeholder 프리팹 생성 (5종 × 4단계 = 20개)

감자/당근/토마토 프리팹은 ARC-003에서 이미 생성됨 — 중복 생성 금지.

각 작물은 4단계(Stage0~Stage3) 프리팹을 생성한다. 공통 스케일 및 프리미티브 규칙은 ARC-003 섹션 2.2 표준을 따른다.

```
// === 옥수수 (Corn) ===
// Stage0 — Sphere, Scale(0.1, 0.1, 0.1), M_Crop_Corn_Early
create_object  primitive: "Sphere"  name: "PFB_Crop_Corn_Stage0"
set_property   target: "PFB_Crop_Corn_Stage0"  property: "localScale"  value: (0.1, 0.1, 0.1)
add_component  target: "PFB_Crop_Corn_Stage0"  component: "MeshRenderer"
// material: M_Crop_Corn_Early
create_prefab  source: "PFB_Crop_Corn_Stage0"
  dest: "Assets/_Project/Prefabs/Crops/PFB_Crop_Corn_Stage0.prefab"

// Stage1 — Capsule, Scale(0.2, 0.3, 0.2)
// Stage2 — Capsule, Scale(0.3, 0.5, 0.3)
// Stage3 — Capsule, Scale(0.4, 0.7, 0.4), M_Crop_Corn_Mature
// (동일 패턴 반복)

// === 딸기 (Strawberry) ===
// PFB_Crop_Strawberry_Stage0~3 — 동일 패턴
// Stage3: Capsule + Sphere(열매), M_Crop_Strawberry_Mature

// === 호박 (Pumpkin) ===
// PFB_Crop_Pumpkin_Stage0~3 — 동일 패턴
// Stage3: M_Crop_Pumpkin_Mature

// === 해바라기 (Sunflower) ===
// PFB_Crop_Sunflower_Stage0~3 — 동일 패턴

// === 수박 (Watermelon) ===
// PFB_Crop_Watermelon_Stage0~3 — 동일 패턴
// (→ 스케일 기준: docs/mcp/farming-tasks.md 섹션 2.2)
// (→ 프리팹 네이밍 규칙: docs/systems/project-structure.md 섹션 6)
```

- **MCP 호출**: ~20회 (작물당 4회: create_object + scale설정 + material설정 + create_prefab)

#### CG-B-05: 거대 작물 프리팹 생성 (2종)

```
// PFB_GiantCrop_Pumpkin
create_object  primitive: "Sphere"  name: "PFB_GiantCrop_Pumpkin"
set_property   target: "PFB_GiantCrop_Pumpkin"  property: "localScale"  value: (3, 2.5, 3)
// material: M_Crop_Pumpkin_Mature (#FF6F00)
create_prefab  source: "PFB_GiantCrop_Pumpkin"
  dest: "Assets/_Project/Prefabs/Crops/PFB_GiantCrop_Pumpkin.prefab"

// PFB_GiantCrop_Watermelon
create_object  primitive: "Capsule"  name: "PFB_GiantCrop_Watermelon"
set_property   target: "PFB_GiantCrop_Watermelon"  property: "localScale"  value: (3, 2, 3)
// material: M_Crop_Watermelon_Mature (#2E7D32)
create_prefab  source: "PFB_GiantCrop_Watermelon"
  dest: "Assets/_Project/Prefabs/Crops/PFB_GiantCrop_Watermelon.prefab"
```

- **MCP 호출**: ~6회

#### CG-B-06: CropData SO에 프리팹 배열 연결

각 SO의 `growthStagePrefabs[]` 배열에 Stage0~3 프리팹을 연결하고, 거대 작물 SO에 `giantCropPrefab`을 연결한다.

```
// SO_Crop_Corn — growthStagePrefabs[0~3]
set_property
  target: "Assets/_Project/Data/Crops/SO_Crop_Corn.asset"
  property: "growthStagePrefabs"
  value: [
    "Assets/_Project/Prefabs/Crops/PFB_Crop_Corn_Stage0.prefab",
    "Assets/_Project/Prefabs/Crops/PFB_Crop_Corn_Stage1.prefab",
    "Assets/_Project/Prefabs/Crops/PFB_Crop_Corn_Stage2.prefab",
    "Assets/_Project/Prefabs/Crops/PFB_Crop_Corn_Stage3.prefab"
  ]
  // (→ see docs/systems/crop-growth-architecture.md 섹션 3 CropData.growthStagePrefabs)

// SO_Crop_Strawberry, Pumpkin, Sunflower, Watermelon — 동일 패턴

// SO_Crop_Pumpkin — giantCropPrefab 연결
set_property
  target: "Assets/_Project/Data/Crops/SO_Crop_Pumpkin.asset"
  property: "giantCropPrefab"
  value: "Assets/_Project/Prefabs/Crops/PFB_GiantCrop_Pumpkin.prefab"

// SO_Crop_Watermelon — giantCropPrefab 연결
set_property
  target: "Assets/_Project/Data/Crops/SO_Crop_Watermelon.asset"
  property: "giantCropPrefab"
  value: "Assets/_Project/Prefabs/Crops/PFB_GiantCrop_Watermelon.prefab"

// 기존 SO 3종(Potato/Carrot/Tomato)에도 growthStagePrefabs 배열 연결
// (ARC-003 Phase B에서 프리팹은 생성되었지만 배열 연결이 미완성인 경우 여기서 완성)
```

- **MCP 호출**: ~5회

[RISK] `set_property`로 GameObject 참조 배열(`growthStagePrefabs[]`) 설정이 지원되지 않는 경우, Editor 스크립트(`InitCropPrefabs.cs`)를 작성하여 `AssetDatabase.LoadAssetAtPath`로 각 프리팹을 로드하고 SO에 할당하는 방식으로 우회한다.

**검증**:

```
enter_play_mode
get_console_logs
  // 각 CropData.growthStagePrefabs 배열 길이 = 4 확인
  // giantCropPrefab != null 확인 (Pumpkin, Watermelon)
exit_play_mode
```

---

### CG-C: GrowthSystem 컴포넌트 연동 및 통합 테스트

**목적**: GrowthSystem.cs에 ProcessNewDay/ProcessSeasonTransition/TryGiantCropMerge 로직 구현, FarmEvents.cs에 신규 이벤트 추가, FarmTile.TryHarvest 품질 결정 및 다중 수확 분기 처리, 통합 테스트 4종 실행.

**전제 조건**:
- CG-A, CG-B 전체 완료
- ARC-003 Phase C 완료 (FarmTile, FarmGrid, FarmingSystem, FarmEvents.cs 컴파일 완료)
- SCN_Test_FarmGrid 씬 존재 (ARC-002 Phase B 결과)

**예상 MCP 호출**: ~12회

#### CG-C-01: GrowthSystem.cs 로직 구현

```
create_script (update mode)
  path: "Assets/_Project/Scripts/Farm/GrowthSystem.cs"
  namespace: "SeedMind.Farm"
  // MonoBehaviour
  // OnEnable: TimeManager.OnDayChanged 구독
  // OnDisable: 구독 해제
  // ProcessNewDay(): 계절 전환 체크 → 타일별 성장 → 거대 작물 병합 → 이벤트 발행
  // ProcessSeasonTransition(SeasonFlag): 재배 불가 계절 작물 고사 처리
  // TryGiantCropMerge(): NxN 영역 탐색 → MergeToGiantCrop
  // SpreadGrowthOverFrames(List<...>): IEnumerator, BATCH_SIZE=16
  // (→ see docs/systems/crop-growth-architecture.md 섹션 4, 8)

execute_menu_item
  menuPath: "Assets/Refresh"
```

- **MCP 호출**: 2회

#### CG-C-02: FarmEvents.cs 신규 이벤트 추가

```
create_script (update mode)
  path: "Assets/_Project/Scripts/Farm/FarmEvents.cs"
  namespace: "SeedMind.Farm"
  // 기존 이벤트 유지 (재생성 금지)
  // 추가:
  //   OnCropStageChanged: Action<Vector2Int, int>
  //   OnCropHarvestedWithQuality: Action<Vector2Int, CropData, Quality>
  //   OnCropReharvestReady: Action<Vector2Int, CropData>
  //   OnGiantCropFormed: Action<Vector2Int, int>
  //   OnGiantCropHarvested: Action<Vector2Int, CropData, int>
  //   OnSeasonCropCheck: Action<SeasonFlag>
  // (→ see docs/systems/crop-growth-architecture.md 섹션 6.1)

execute_menu_item
  menuPath: "Assets/Refresh"
```

- **MCP 호출**: 2회

#### CG-C-03: FarmTile.TryHarvest 수정

```
create_script (update mode)
  path: "Assets/_Project/Scripts/Farm/FarmTile.cs"
  namespace: "SeedMind.Farm"
  // TryHarvest 메서드 수정:
  //   품질 결정 로직 추가: _crop.DetermineQuality(_soilQuality)
  //   다중 수확 분기: isReharvestable → ResetForReharvest() + SetState(Dry)
  //   단일 수확 분기: _crop = null + SetState(Tilled)
  //   이벤트 발행: FarmEvents.OnCropHarvestedWithQuality
  // (→ see docs/systems/crop-growth-architecture.md 섹션 7.3)

execute_menu_item
  menuPath: "Assets/Refresh"
```

- **MCP 호출**: 2회

#### CG-C-04: GrowthSystem 씬 배치

GrowthSystem GO는 ARC-002에서 이미 생성됨. 스크립트만 부착하고 FarmGrid 참조를 연결한다.

```
add_component
  target: "GrowthSystem"  // SCN_Test_FarmGrid 씬의 기존 GO
  component: "SeedMind.Farm.GrowthSystem"

set_property
  target: "GrowthSystem"
  component: "GrowthSystem"
  property: "_farmGrid"
  value: {GameObject reference: "FarmGrid"}

save_scene
```

- **MCP 호출**: 3회

#### CG-C-05: 통합 테스트

SCN_Test_FarmGrid 씬에서 4종 테스트 시나리오를 실행한다.

**테스트 T-CG-1: 감자 단일 수확**
```
enter_play_mode
execute_method
  target: "FarmingSystem"
  method: "DebugPlantCrop"
  args: ["potato", 0, 0]  // 타일(0,0)에 감자 심기

// 물주기 3일 시뮬레이션
execute_method  target: "GrowthSystem"  method: "DebugSimulateDays"  args: [3, true]
  // true = 매일 물 주기

execute_method  target: "FarmTile_0_0"  method: "TryHarvest"  args: []

get_console_logs
  // 기대 로그:
  // [GrowthSystem] Day1: potato Stage0→Stage1 (Corn: StageAdvanced)
  // [GrowthSystem] Day3: potato Completed → Harvestable
  // [FarmTile] Harvested: potato, qty=1, quality=Normal|Silver
exit_play_mode
```

**테스트 T-CG-2: 딸기 다중 수확**
```
enter_play_mode
execute_method  method: "DebugPlantCrop"  args: ["strawberry", 1, 0]
execute_method  method: "DebugSimulateDays"  args: [5, true]  // 첫 수확까지

// 1차 수확
execute_method  target: "FarmTile_1_0"  method: "TryHarvest"

// 재성장 3일 후 2차 수확
// reharvestDays = 3 (→ see docs/systems/crop-growth.md 섹션 4.2)
execute_method  method: "DebugSimulateDays"  args: [3, true]
execute_method  target: "FarmTile_1_0"  method: "TryHarvest"

get_console_logs
  // 기대 로그:
  // [FarmTile] Reharvest ready: strawberry
  // [FarmTile] Harvested(2nd): strawberry, qty=1, quality=...
exit_play_mode
```

**테스트 T-CG-3: 호박 거대 작물 병합**
```
enter_play_mode
// 3x3 영역(0~2, 0~2)에 호박 심기
execute_method  method: "DebugPlantArea"  args: ["pumpkin", 0, 0, 3, 3]

// giantCropChance를 임시로 1.0으로 설정 (확률 100%)
set_property
  target: "Assets/_Project/Data/Crops/SO_Crop_Pumpkin.asset"
  property: "giantCropChance"
  value: 1.0

// growthDays만큼 시뮬레이션 (→ see docs/design.md 섹션 4.2)
execute_method  method: "DebugSimulateDays"  args: [10, true]  // 10 = 호박 growthDays (→ see docs/design.md 섹션 4.2)

get_console_logs
  // 기대 로그:
  // [GrowthSystem] GiantCrop formed: pumpkin at (0,0) size=3
  // FarmEvents.OnGiantCropFormed 발행 확인

// 거대 작물 수확
execute_method  target: "FarmTile_0_0"  method: "TryHarvest"
get_console_logs
  // [GiantCrop] Harvested: pumpkin, totalYield = 9종 * 2배 = 18

exit_play_mode
```

**테스트 T-CG-4: 계절 고사 처리**
```
enter_play_mode
// 봄 작물(감자)을 심고 여름으로 전환
execute_method  method: "DebugPlantCrop"  args: ["potato", 2, 0]
execute_method  target: "TimeManager"  method: "DebugChangeSeason"  args: ["Summer"]

get_console_logs
  // 기대 로그:
  // [GrowthSystem] Season changed to Summer: potato at (2,0) withered (allowedSeasons mismatch)
  // FarmEvents.OnCropWithered 발행 확인
exit_play_mode
```

- **MCP 호출**: ~3회 (enter/exit play_mode + get_console_logs 각 테스트 공유)

**최종 검증 체크리스트**:

- [ ] CropData SO 8종 모두 Inspector에서 필드값 확인 가능
- [ ] growthStagePrefabs[] 배열 길이 = 4 (8종 모두)
- [ ] T-CG-1: 감자 단일 수확 정상 동작, Harvestable → Tilled 전환
- [ ] T-CG-2: 딸기 다중 수확 정상 동작, Harvestable → Dry 전환, 2차 수확 가능
- [ ] T-CG-3: 호박 3x3 거대 작물 병합 및 수확량 18개 확인
- [ ] T-CG-4: 계절 전환 시 호환 불가 작물 고사 확인
- [ ] Console에 [RISK] 항목(프레임 드롭, 레이스 컨디션) 관련 에러 없음

---

## Cross-references

- `docs/systems/crop-growth-architecture.md` (ARC-005) — 이 문서의 상위 아키텍처 설계. 섹션 9가 이 문서의 소스.
- `docs/systems/crop-growth.md` (DES-003) — 작물 수치(성장일, 재수확 주기, 거대 작물 확률) canonical
- `docs/design.md` 섹션 4.2 — 작물 종류, 씨앗 가격, 판매가 canonical
- `docs/mcp/farming-tasks.md` (ARC-003) — 선행 태스크. FarmGrid/FarmTile/CropData 초기 정의 및 감자/당근/토마토 프리팹 생성
- `docs/mcp/scene-setup-tasks.md` (ARC-002) — 폴더 구조, 씬 계층 정의
- `docs/systems/farming-architecture.md` — FarmTile 상태 머신, FarmEvents 기반 구조, 비료 배수 canonical
- `docs/systems/project-structure.md` — 네임스페이스 규칙(`SeedMind.Farm`), 프리팹 네이밍(`PFB_Crop_*`) canonical

---

## Open Questions

- [OPEN] ARC-003에서 생성된 SO_Crop_Tomato.asset의 `isReharvestable` 값 확정 필요. 토마토를 다중 수확으로 전환할지 여부는 `docs/systems/crop-growth-architecture.md` 섹션 7.1 [OPEN] 항목 결정 후 설정.
- 딸기 `reharvestDays` = 3 (확정) — `docs/systems/crop-growth.md` 섹션 4.2 canonical.
- [OPEN] `DebugSimulateDays` / `DebugPlantArea` 등 테스트 헬퍼 메서드가 GrowthSystem/FarmingSystem에 이미 구현되어 있는지 ARC-003 완료 시 확인 필요. 없으면 CG-C 테스트 단계에서 별도 생성.
- [OPEN] 거대 작물의 품질 결정 방식 미확정 — 멤버 타일 평균 품질 점수 vs 고정 Gold 이상 보장. (→ see `docs/systems/crop-growth-architecture.md` Open Questions 1번)

## Risks

- [RISK] CropData.cs 필드 확장 시 기존 SO 에셋(Potato/Carrot/Tomato)의 직렬화 데이터 손상 가능성. 새 필드는 기본값(false/0/null)으로 초기화되어야 하며, 확장 후 Unity 재임포트 시 에러 없음을 확인해야 한다.
- [RISK] `growthStagePrefabs[]` 배열 MCP 설정 미지원 시 ~35회 추가 호출 발생. Editor 스크립트(`InitCropPrefabs.cs`) 일괄 처리로 ~5회로 압축 가능. 수치는 반드시 canonical 문서에서 조회해야 한다 (PATTERN-007).
- [RISK] 코루틴 분산 처리(SpreadGrowthOverFrames)가 동작하지 않는 경우 256타일 동시 프리팹 교체로 프레임 드롭 발생. Play Mode 테스트 시 Profiler 확인 권장. (→ see `docs/systems/crop-growth-architecture.md` 섹션 10.2)
- [RISK] `TryGiantCropMerge`에서 이미 플레이어가 일부 타일을 수확하고 있을 때의 동시성 문제는 ProcessNewDay 내 단일 스레드 처리로 안전하나, 테스트 헬퍼 메서드(`DebugSimulateDays`)가 Update와 별도 타이밍에 실행되는 경우 레이스 컨디션 가능성 있음.

---

*이 문서는 `docs/systems/crop-growth-architecture.md`(ARC-005) 섹션 9를 독립 MCP 태스크 시퀀스 문서로 분리·상세화한 것입니다. Claude Code (Sonnet 4.6) 작성.*
