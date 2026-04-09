# 가공 시스템 MCP 태스크 시퀀스

> 가공 시스템(ProcessingSystem) Unity 구현을 위한 MCP for Unity 태스크 실행 순서. `processing-architecture.md` Part II 기반으로 상세화.  
> 작성: Claude Code (Sonnet 4.6) | 2026-04-06  
> 문서 ID: ARC-014 | Phase 1

---

## 개요

이 문서는 `docs/systems/processing-architecture.md`(ARC-012) Part II에서 요약된 MCP 구현 계획을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 가공 시스템의 데이터 레이어, 시스템 레이어, SO 에셋, UI, 씬 배치를 완성한다.

---

## 전제 조건

### 완료된 태스크 의존성

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조, SCN_Farm 기본 계층 (Managers, Farm, Player, UI) |
| ARC-003 | `docs/mcp/farming-tasks.md` | Phase A~C 전체 | FarmGrid 타일, CropData SO, ToolData SO |
| ARC-007 | `docs/systems/facilities-architecture.md` | (문서 참조) | BuildingManager, BuildingInstance, BuildingData, BuildingEvents 클래스 |

### 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `BuildingManager` (씬 내 GameObject 또는 Singleton) | ARC-007 시설 시스템 구현 |
| `Canvas_Overlay` (UI 루트) | ARC-002 Phase B |
| `DataRegistry` (SO 로드 시스템) | `docs/pipeline/data-pipeline.md` Part II |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A |

### 필요 C# 스크립트 (MCP 실행 전 작성 필요)

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 Phase 순서대로 작성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 |
|---|----------|--------|-------------|
| S-01 | `Scripts/Building/Data/ProcessingType.cs` | `ProcessingType` (enum) | `SeedMind.Building.Data` |
| S-02 | `Scripts/Building/Data/ProcessingRecipeData.cs` | `ProcessingRecipeData` | `SeedMind.Building.Data` |
| S-03 | `Scripts/Building/Buildings/ProcessingSlot.cs` | `ProcessingSlot` | `SeedMind.Building` |
| S-04 | `Scripts/Building/ProcessingSaveData.cs` | `ProcessingSaveData` | `SeedMind.Building` |
| S-05 | `Scripts/Building/Buildings/ProcessingSystem.cs` | `ProcessingSystem` | `SeedMind.Building` |
| S-06 | `Scripts/UI/ProcessingUI.cs` | `ProcessingUI` | `SeedMind.UI` |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: S-01 -> S-02 -> S-03 -> S-04 -> S-05 -> S-06. 각 Phase 사이에 Unity 컴파일 대기가 필요하다.

---

## MCP 도구 매핑

| MCP 도구 | 용도 | 사용 Phase |
|----------|------|-----------|
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | P-1, P-2, P-3 |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | P-1~P-10 전체 |
| `create_object` | 빈 GameObject 생성 | P-9, P-10 |
| `add_component` | MonoBehaviour 컴포넌트 부착 | P-8, P-9, P-10 |
| `set_parent` | 오브젝트 부모 설정 | P-9, P-10 |
| `create_folder` | 에셋 폴더 생성 | P-1 |
| `save_scene` | 씬 저장 | P-9, P-10, P-13 |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | P-11, P-12, P-13 |
| `execute_menu_item` | 편집기 명령 실행 | 컴파일 대기 등 |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | P-11~P-13 |

[RISK] `create_scriptable_object` 도구의 가용 여부 및 파라미터 형식 사전 검증 필요. SO 인스턴스 생성이 MCP에서 미지원인 경우, Editor 스크립트를 통한 우회 필요. (-> see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)

---

## Part I -- ScriptableObject 에셋 생성

### Task P-1: ProcessingBuildingData SO 에셋 생성

**목표**: 가공소 4종(가공소, 제분소, 발효실, 베이커리)의 BuildingData SO 에셋을 생성한다. 이들은 기존 BuildingData 스키마를 따르며 `effectType = Processing`으로 설정된다.

**전제**: BuildingData.cs가 컴파일 완료된 상태 (ARC-007 구현 완료).

#### P-1-01: 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Buildings/Processing"
```

#### P-1-02: 가공소(일반) BuildingData SO

```
create_scriptable_object
  type: "SeedMind.Building.Data.BuildingData"
  asset_path: "Assets/_Project/Data/Buildings/Processing/SO_Building_Processing.asset"

set_property  target: "SO_Building_Processing"
  dataId = "building_processing"
  displayName = "가공소"
  effectType = 4                              // Processing (int cast, -> see docs/systems/facilities-architecture.md 섹션 2.1)
  effectValue = 1                             // 초기 슬롯 수 (-> see docs/content/processing-system.md 섹션 5.1)
  buildCost = 0                               // (-> see docs/design.md 섹션 4.6 for canonical 값)
  unlockLevel = 0                             // (-> see docs/design.md 섹션 4.6)
  tileSize = 0                                // (-> see docs/content/facilities.md 섹션 6.1)
  buildTimeDays = 0                           // (-> see docs/content/facilities.md 섹션 6.1)
```

> **주의**: `buildCost`, `unlockLevel`, `tileSize`, `buildTimeDays`의 구체적 수치는 MCP 실행 시점에 canonical 문서에서 읽어 입력한다. 본 문서에서 수치를 직접 기재하지 않는다 (PATTERN-006, PATTERN-007).

- **MCP 호출**: 1(생성) + 8(필드 설정) = 9회

#### P-1-03: 제분소 BuildingData SO

```
create_scriptable_object
  type: "SeedMind.Building.Data.BuildingData"
  asset_path: "Assets/_Project/Data/Buildings/Processing/SO_Building_Mill.asset"

set_property  target: "SO_Building_Mill"
  dataId = "building_mill"
  displayName = "제분소"
  effectType = 4                              // Processing
  effectValue = 1                             // 슬롯 수 (-> see docs/content/processing-system.md 섹션 2.3.1)
  buildCost = 0                               // (-> see docs/content/processing-system.md 섹션 2.3.1)
  unlockLevel = 0                             // (-> see docs/content/processing-system.md 섹션 2.3.1)
  tileSize = 0                                // (-> see docs/content/processing-system.md 섹션 2.3.1)
  buildTimeDays = 0                           // (-> see docs/content/processing-system.md 섹션 2.3.1)
```

- **MCP 호출**: 9회

#### P-1-04: 발효실 BuildingData SO

```
create_scriptable_object
  type: "SeedMind.Building.Data.BuildingData"
  asset_path: "Assets/_Project/Data/Buildings/Processing/SO_Building_Fermentation.asset"

set_property  target: "SO_Building_Fermentation"
  dataId = "building_fermentation"
  displayName = "발효실"
  effectType = 4                              // Processing
  effectValue = 2                             // 슬롯 수 (-> see docs/content/processing-system.md 섹션 2.3.2)
  buildCost = 0                               // (-> see docs/content/processing-system.md 섹션 2.3.2)
  unlockLevel = 0                             // (-> see docs/content/processing-system.md 섹션 2.3.2)
  tileSize = 0                                // (-> see docs/content/processing-system.md 섹션 2.3.2)
  buildTimeDays = 0                           // (-> see docs/content/processing-system.md 섹션 2.3.2)
```

- **MCP 호출**: 9회

#### P-1-05: 베이커리 BuildingData SO

```
create_scriptable_object
  type: "SeedMind.Building.Data.BuildingData"
  asset_path: "Assets/_Project/Data/Buildings/Processing/SO_Building_Bakery.asset"

set_property  target: "SO_Building_Bakery"
  dataId = "building_bakery"
  displayName = "베이커리"
  effectType = 4                              // Processing
  effectValue = 2                             // 슬롯 수 (-> see docs/content/processing-system.md 섹션 2.3.3)
  buildCost = 0                               // (-> see docs/content/processing-system.md 섹션 2.3.3)
  unlockLevel = 0                             // (-> see docs/content/processing-system.md 섹션 2.3.3)
  tileSize = 0                                // (-> see docs/content/processing-system.md 섹션 2.3.3)
  buildTimeDays = 0                           // (-> see docs/content/processing-system.md 섹션 2.3.3)
```

- **MCP 호출**: 9회

#### P-1 검증 체크리스트

- [ ] `Assets/_Project/Data/Buildings/Processing/` 폴더 존재
- [ ] SO 에셋 4개 존재: `SO_Building_Processing`, `SO_Building_Mill`, `SO_Building_Fermentation`, `SO_Building_Bakery`
- [ ] 모든 에셋의 `effectType = 4` (Processing)
- [ ] `effectValue`가 각 가공소의 초기 슬롯 수와 일치
- [ ] 콘솔 에러 없음

**P-1 MCP 호출 합계**: 1(폴더) + 9 x 4(에셋) = 37회

---

### Task P-2: RecipeData SO 에셋 생성

> **[최적화 필수]** P-2-01(폴더 생성)만 수행 후 P-2-ALT로 이동한다. P-2-02~P-2-07 개별 생성(517회)은 건너뜀.

**목표**: 전체 32종 레시피의 ProcessingRecipeData SO 에셋을 생성한다.

**전제**: ProcessingRecipeData.cs, ProcessingType.cs가 컴파일 완료된 상태 (스크립트 S-01, S-02).

#### P-2-01: 레시피 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Recipes"
create_folder
  path: "Assets/_Project/Data/Recipes/Processing"
create_folder
  path: "Assets/_Project/Data/Recipes/Mill"
create_folder
  path: "Assets/_Project/Data/Recipes/Fermentation"
create_folder
  path: "Assets/_Project/Data/Recipes/Bakery"
```

- **MCP 호출**: 5회

#### P-2-02: 가공소(일반) 레시피 SO -- 잼 7종

레시피 ID 목록 및 수치: (-> see `docs/content/processing-system.md` 섹션 3.1.1)

반복 패턴 (잼 레시피 7종, 각 에셋 동일 구조):

```
create_scriptable_object
  type: "SeedMind.Building.Data.ProcessingRecipeData"
  asset_path: "Assets/_Project/Data/Recipes/Processing/SO_Recipe_Jam_{Crop}.asset"

set_property  target: "SO_Recipe_Jam_{Crop}"
  dataId = "recipe_jam_{crop}"                      // (-> see docs/content/processing-system.md 섹션 3.1.1)
  displayName = "{작물명} 잼"
  description = "{작물명}을(를) 가공하여 만든 잼."
  processingType = 0                                 // Jam (int cast)
  inputCategory = {category_int}                     // (-> see docs/systems/economy-system.md 섹션 2.5 작물 분류)
  inputItemId = "{crop_id}"
  inputQuantity = 1                                  // (-> see docs/content/processing-system.md 섹션 3.1.1)
  priceMultiplier = 0.0                              // (-> see docs/systems/economy-system.md 섹션 2.5)
  priceBonus = 0                                     // (-> see docs/systems/economy-system.md 섹션 2.5)
  processingTimeHours = 0.0                          // (-> see docs/content/processing-system.md 섹션 3.1)
  fuelCost = 0                                       // 가공소(일반)는 연료 불필요
  requiredFacilityTier = 0                           // Tier 1 (기본)
  outputItemId = "jam_{crop}"
  outputQuantity = 1
```

> 수치 0.0 / 0은 placeholder이다. MCP 실행 시점에 canonical 문서에서 값을 읽어 설정한다.

적용 대상 8종: potato, carrot, tomato, corn, strawberry, pumpkin, watermelon, shiitake (겨울 작물 포함하여 섹션 3.1.1 + 3.1.4에서 잼 해당 분).

- **MCP 호출**: 8 x (1 생성 + 15 필드) = 128회

#### P-2-03: 가공소(일반) 레시피 SO -- 주스 3종

레시피 목록: (-> see `docs/content/processing-system.md` 섹션 3.1.2)

P-2-02와 동일 패턴, `processingType = 1` (Juice).

- **MCP 호출**: 3 x 16 = 48회

#### P-2-04: 가공소(일반) 레시피 SO -- 절임 7종

레시피 목록: (-> see `docs/content/processing-system.md` 섹션 3.1.3 + 3.1.4)

P-2-02와 동일 패턴, `processingType = 2` (Pickle).

절임 대상 7종: potato, carrot, tomato, corn, pumpkin, winter_radish, spinach.

- **MCP 호출**: 7 x 16 = 112회

#### P-2-05: 제분소 레시피 SO -- 4종

레시피 목록 및 수치: (-> see `docs/content/processing-system.md` 섹션 3.2)

```
create_scriptable_object
  type: "SeedMind.Building.Data.ProcessingRecipeData"
  asset_path: "Assets/_Project/Data/Recipes/Mill/SO_Recipe_Mill_{Crop}.asset"

set_property  target: "SO_Recipe_Mill_{Crop}"
  dataId = "recipe_mill_{crop}_{output}"
  displayName = "{결과물명}"
  description = "{작물명}을(를) 분쇄하여 만든 가루."
  processingType = 3                                 // Mill (int cast)
  inputCategory = {category_int}
  inputItemId = "{crop_id}"
  inputQuantity = 0                                  // (-> see docs/content/processing-system.md 섹션 3.2, 작물별 상이)
  priceMultiplier = 0.0                              // (-> see docs/content/processing-system.md 섹션 3.2 제분 공식)
  priceBonus = 0                                     // (-> see docs/content/processing-system.md 섹션 3.2)
  processingTimeHours = 0.0                          // (-> see docs/content/processing-system.md 섹션 3.2)
  fuelCost = 0                                       // 제분소는 연료 불필요
  requiredFacilityTier = 0
  outputItemId = "{output_id}"
  outputQuantity = 1
```

- **MCP 호출**: 4 x 16 = 64회

#### P-2-06: 발효실 레시피 SO -- 5종

레시피 목록 및 수치: (-> see `docs/content/processing-system.md` 섹션 3.3)

P-2-05와 동일 패턴, `processingType = 4` (Fermentation).

- **MCP 호출**: 5 x 16 = 80회

#### P-2-07: 베이커리 레시피 SO -- 5종

레시피 목록 및 수치: (-> see `docs/content/processing-system.md` 섹션 3.4)

베이커리는 **복수 재료**를 요구하는 유일한 가공소이며, `fuelCost > 0`이다.

```
create_scriptable_object
  type: "SeedMind.Building.Data.ProcessingRecipeData"
  asset_path: "Assets/_Project/Data/Recipes/Bakery/SO_Recipe_Bake_{Output}.asset"

set_property  target: "SO_Recipe_Bake_{Output}"
  dataId = "recipe_bake_{output}"
  displayName = "{결과물명}"
  description = "{결과물 설명}"
  processingType = 5                                 // Bake (int cast)
  inputCategory = 0                                  // 베이커리는 카테고리 무관 (복수 재료)
  inputItemId = "{primary_input_id}"                 // 주재료 ID
  inputQuantity = 0                                  // (-> see docs/content/processing-system.md 섹션 3.4)
  priceMultiplier = 0.0                              // 베이커리는 고정 가격, 배수 미사용
  priceBonus = 0                                     // (-> see docs/content/processing-system.md 섹션 3.4)
  processingTimeHours = 0.0                          // (-> see docs/content/processing-system.md 섹션 3.4)
  fuelCost = 0                                       // (-> see docs/content/processing-system.md 섹션 3.4, 1~2개)
  requiredFacilityTier = 0
  outputItemId = "{output_id}"
  outputQuantity = 0                                 // (-> see docs/content/processing-system.md 섹션 3.4, 1~3개)
```

[OPEN] 베이커리 레시피는 복수 재료(예: 호박 분말 x1 + 딸기 잼 x1)를 요구하지만, 현재 ProcessingRecipeData 스키마는 `inputItemId` 1개만 지원한다. 복수 재료를 지원하려면 `inputItems: InputRequirement[]` 배열 필드가 필요하다. 이 스키마 확장은 processing-architecture.md에서 별도 결정이 필요하다.

- **MCP 호출**: 5 x 16 = 80회

#### P-2 검증 체크리스트

- [ ] 레시피 에셋 폴더 4개 존재 (Processing, Mill, Fermentation, Bakery)
- [ ] SO 에셋 총 32개 존재
  - Processing: 18개 (잼 8 + 주스 3 + 절임 7)
  - Mill: 4개
  - Fermentation: 5개
  - Bakery: 5개
- [ ] 모든 에셋의 `dataId`가 `docs/content/processing-system.md` 섹션 3.5와 일치
- [ ] `processingType` int 값이 ProcessingType enum과 일치
- [ ] 가공소/제분소/발효실의 `fuelCost = 0`, 베이커리만 `fuelCost > 0`
- [ ] 콘솔 에러 없음

**P-2 MCP 호출 합계**: 5(폴더) + 128(잼 8) + 48(주스 3) + 112(절임 7) + 64(제분 4) + 80(발효 5) + 80(베이커리 5) = 517회

> **[최적화 필수]** P-2 개별 생성(517회)을 건너뛰고 P-2-ALT를 사용한다. P-2-01(폴더 생성)만 수행 후 P-2-ALT로 이동할 것.

---

### Task P-2-ALT: Editor 스크립트를 통한 레시피 SO 일괄 생성 (**기본 경로**)

**[기본 경로]** P-2-01(폴더 생성) 완료 후 바로 이 절차를 실행한다. P-2-02~P-2-07은 건너뜀.

**목표**: `CreateAllRecipeSOs.cs` Editor 스크립트를 생성하고 실행하여 레시피 SO 32종을 일괄 생성한다.

**전제**: ProcessingRecipeData.cs, ProcessingType.cs 컴파일 완료. P-2-01 폴더 4개 생성 완료.

#### P-2-ALT-01: Editor 스크립트 생성

```
create_script
  path: "Assets/_Project/Editor/CreateAllRecipeSOs.cs"
  content: |
    // Editor 전용: 가공 레시피 SO 32종 일괄 생성
    // 모든 레시피 수치는 docs/content/processing-system.md 섹션 3의 canonical 정의를 기반으로 함
    // -> copied from docs/content/processing-system.md 섹션 3.1~3.4, 3.7
    #if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using SeedMind.Building.Data;

    public static class CreateAllRecipeSOs
    {
        [MenuItem("SeedMind/Create All Recipe SOs")]
        public static void CreateAll()
        {
            // 가공소(일반): 잼 8종(섹션 3.1.1+3.1.4) + 주스 3종(3.1.2) + 절임 7종(3.1.3+3.1.4) = 18종
            // 제분소: 4종 (섹션 3.2)
            // 발효실: 5종 (섹션 3.3)
            // 베이커리: 5종 (섹션 3.4)
            // 제련로: 1종 — 철 광석 → 철 조각 (섹션 3.7, DES-020)
            // -> see docs/content/processing-system.md 섹션 3 전체 for 각 레시피 dataId/수치
            // -> see docs/content/processing-system.md 섹션 4.2 for 연료 비용 canonical

            // 각 레시피를 ProcessingRecipeData SO로 생성 후 AssetDatabase.CreateAsset()
            // 필드: dataId, displayName, processingType, inputItemId, inputQuantity,
            //        priceMultiplier, priceBonus, processingTimeHours, fuelCost,
            //        requiredFacilityTier, outputItemId, outputQuantity

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateAllRecipeSOs] 32 recipe SO assets created.");
        }
    }
    #endif
```

- **MCP 호출**: 1회 (create_script)

[RISK] Editor 스크립트 내 수치는 canonical 문서에서 읽어 구현한다. 베이커리 레시피 복수 재료 문제(P-2 OPEN#2): `inputItemId`를 주재료 1종으로 설정하고, 부재료는 description에 명시하는 임시 처리를 적용한다.

#### P-2-ALT-02: 컴파일 대기

```
execute_menu_item
  menu: "File/Save Project"
```

- **MCP 호출**: 1회

#### P-2-ALT-03: 레시피 SO 일괄 생성 실행

```
execute_menu_item
  menu: "SeedMind/Create All Recipe SOs"
```

- **MCP 호출**: 1회

#### P-2-ALT 검증 체크리스트

- [ ] `Assets/_Project/Data/Recipes/` 하위 SO 에셋 총 32개 (+ 제련 1종 포함 시 33개)
- [ ] 콘솔에 `[CreateAllRecipeSOs] 32 recipe SO assets created.` 출력
- [ ] 콘솔 에러 없음

**P-2-ALT MCP 호출 합계**: 3회 (create_script 1 + execute_menu_item 2)  
**P-2 대비 절감**: 514회 (517 → 3)

---

### Task P-3: FuelData SO 에셋 생성

**목표**: 연료 아이템(장작) 데이터 에셋을 생성한다.

**전제**: 아이템 데이터 스키마가 정의된 상태. 장작은 일반 아이템이므로 기존 ItemData SO 스키마를 사용한다.

연료 종류 및 수치: (-> see `docs/content/processing-system.md` 섹션 4.2)

#### P-3-01: 장작 아이템 SO

```
create_scriptable_object
  type: "SeedMind.Core.Data.ItemData"                // 일반 아이템 SO (가칭)
  asset_path: "Assets/_Project/Data/Items/SO_Item_Firewood.asset"

set_property  target: "SO_Item_Firewood"
  dataId = "item_firewood"
  displayName = "장작"
  description = "베이커리의 연료로 사용되는 장작."
  itemType = 0                                       // Consumable / Material (-> see 아이템 스키마)
  buyPrice = 0                                       // (-> see docs/content/processing-system.md 섹션 4.2)
  sellPrice = 0                                      // (-> see docs/content/processing-system.md 섹션 4.2)
  stackSize = 99
```

> `buyPrice`, `sellPrice` 수치는 MCP 실행 시 canonical 문서에서 읽어 입력 (PATTERN-006).

- **MCP 호출**: 1(생성) + 7(필드) = 8회

[OPEN] ItemData SO 스키마가 아직 canonical로 정의되지 않았다. `data-pipeline.md`에 ItemData 섹션 추가가 선행되어야 한다. 장작 외에도 일반 아이템(가공품 결과물 등)이 동일 스키마를 사용할 것이므로, 스키마 확정이 시급하다.

#### P-3 검증 체크리스트

- [ ] `SO_Item_Firewood.asset` 존재
- [ ] `dataId = "item_firewood"` 확인
- [ ] 콘솔 에러 없음

**P-3 MCP 호출 합계**: 8회

---

## Part II -- 스크립트 생성 및 연결

> 본 Phase의 모든 스크립트는 illustrative 코드이며, 실제 작성은 Phase 2(Unity 구현)에서 수행한다. 여기서는 MCP `add_component` 및 `set_property`에 필요한 **스크립트 존재 확인**과 **컴파일 순서**만 정의한다.

### Task P-4: ProcessingSystem 데이터 레이어 스크립트 생성

**목표**: ProcessingType enum, ProcessingRecipeData SO 클래스, ProcessingSlot, ProcessingSaveData를 생성한다.

#### P-4-01: ProcessingType.cs 생성

```
create_script
  path: "Assets/_Project/Scripts/Building/Data/ProcessingType.cs"
  namespace: "SeedMind.Building.Data"
  type: enum
```

enum 값: (-> see `docs/pipeline/data-pipeline.md` 섹션 2.5 ProcessingType enum canonical)

#### P-4-02: ProcessingRecipeData.cs 생성

```
create_script
  path: "Assets/_Project/Scripts/Building/Data/ProcessingRecipeData.cs"
  namespace: "SeedMind.Building.Data"
  base_class: "GameDataSO"
```

필드 정의: (-> see `docs/pipeline/data-pipeline.md` 섹션 2.5 및 `docs/systems/processing-architecture.md` 섹션 2.1)

#### P-4-03: ProcessingSlot.cs 생성

```
create_script
  path: "Assets/_Project/Scripts/Building/Buildings/ProcessingSlot.cs"
  namespace: "SeedMind.Building"
  type: class (Plain C#)
```

클래스 구조: (-> see `docs/systems/processing-architecture.md` 섹션 2.2)

#### P-4-04: ProcessingSaveData.cs 생성

```
create_script
  path: "Assets/_Project/Scripts/Building/ProcessingSaveData.cs"
  namespace: "SeedMind.Building"
  type: class (Serializable)
```

필드 정의: (-> see `docs/systems/processing-architecture.md` 섹션 5.1 및 `docs/pipeline/data-pipeline.md` Part II 섹션 2.6)

#### P-4-05: Unity 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

컴파일 성공 후 다음 Phase로 진행.

#### P-4 검증 체크리스트

- [ ] 4개 스크립트 파일 존재
- [ ] Unity 컴파일 에러 없음 (콘솔 확인)
- [ ] `ProcessingType` enum에 6개 값(Jam, Juice, Pickle, Mill, Fermentation, Bake)
- [ ] `ProcessingRecipeData` 필드 수가 15개 (부모 3 + 자체 12, PATTERN-005)

**P-4 MCP 호출 합계**: 4(생성) + 1(컴파일) = 5회

---

### Task P-5: ProcessingSystem 시스템 레이어 스크립트 생성

**목표**: ProcessingSystem (Plain C# 클래스)을 생성하고, BuildingManager에 인스턴스 필드를 추가한다.

#### P-5-01: ProcessingSystem.cs 생성

```
create_script
  path: "Assets/_Project/Scripts/Building/Buildings/ProcessingSystem.cs"
  namespace: "SeedMind.Building"
  type: class (Plain C#)
```

클래스 구조: (-> see `docs/systems/processing-architecture.md` 섹션 2.3)

주요 메서드:
- `RegisterProcessor(BuildingInstance)` -- 가공소 등록
- `UnregisterProcessor(BuildingInstance)` -- 가공소 해제
- `StartProcessing(BuildingInstance, ProcessingRecipeData, string, int)` -- 가공 시작
- `ProcessTimeAdvance(float)` -- 시간 경과 처리
- `CollectOutput(BuildingInstance, int, out string, out int)` -- 결과물 수거
- `GetSlots(BuildingInstance)` / `GetAvailableRecipes(BuildingInstance)` -- 조회
- `GetSaveData()` / `LoadSaveData(ProcessingSaveData[])` -- 저장/복원

#### P-5-02: BuildingManager.cs 수정 -- ProcessingSystem 인스턴스 추가

기존 BuildingManager에 아래 필드/로직 추가:

```
// BuildingManager.cs 수정 사항:
private ProcessingSystem _processingSystem = new ProcessingSystem();

// OnBuildingCompleted 핸들러에 추가:
if (inst.Data.effectType == BuildingEffectType.Processing)
    _processingSystem.RegisterProcessor(inst);

// TimeManager.OnHourChanged 구독에 추가:
_processingSystem.ProcessTimeAdvance(1.0f);
```

(-> see `docs/systems/processing-architecture.md` 섹션 2.3, 2.4 클래스 관계 다이어그램)

#### P-5-03: BuildingEvents.cs 수정 -- 가공 이벤트 추가

기존 BuildingEvents 정적 클래스에 가공 관련 이벤트 4개 추가:

- `OnProcessingStarted(BuildingInstance, int)`
- `OnProcessingComplete(BuildingInstance, int)`
- `OnProcessingCollected(BuildingInstance, int, string)`
- `OnProcessingCancelled(BuildingInstance, int, string, int)`

(-> see `docs/systems/processing-architecture.md` 섹션 6.1)

#### P-5-04: Unity 컴파일 대기

#### P-5 검증 체크리스트

- [ ] ProcessingSystem.cs 존재, 컴파일 에러 없음
- [ ] BuildingManager에 `_processingSystem` 필드 존재
- [ ] BuildingEvents에 가공 이벤트 4개 추가 확인
- [ ] TimeManager.OnHourChanged에 ProcessTimeAdvance 연결 확인

**P-5 MCP 호출 합계**: 3(생성/수정) + 1(컴파일) = 4회

---

### Task P-6: DataRegistry에 ProcessingRecipeData 등록

**목표**: DataRegistry가 런타임에 모든 ProcessingRecipeData를 로드하도록 등록한다.

(-> see `docs/pipeline/data-pipeline.md` Part II 섹션 1 for DataRegistry 구조)

#### P-6-01: DataRegistry.cs 수정

```
// DataRegistry에 추가:
private ProcessingRecipeData[] _recipes;

public ProcessingRecipeData[] AllRecipes => _recipes;

// Load 메서드에 추가:
_recipes = Resources.LoadAll<ProcessingRecipeData>("Data/Recipes");
// 또는 Addressables 사용 시 해당 방식으로 로드
```

#### P-6-02: ProcessingSystem에 레시피 레지스트리 연결

```
// ProcessingSystem 초기화 시:
_allRecipes = DataRegistry.Instance.AllRecipes;
```

#### P-6 검증 체크리스트

- [ ] DataRegistry에 ProcessingRecipeData 로드 로직 존재
- [ ] ProcessingSystem이 전체 레시피 목록에 접근 가능
- [ ] 콘솔 에러 없음

**P-6 MCP 호출 합계**: 2(수정) + 1(컴파일) = 3회

---

### Task P-7: ProcessingUI 생성

**목표**: 가공소 인터랙션 UI 스크립트와 프리팹을 생성한다.

#### P-7-01: ProcessingUI.cs 생성

```
create_script
  path: "Assets/_Project/Scripts/UI/ProcessingUI.cs"
  namespace: "SeedMind.UI"
  base_class: "MonoBehaviour"
```

클래스 구조: (-> see `docs/systems/processing-architecture.md` 섹션 7.2)

SerializeField 목록:
- `_recipeListParent: Transform`
- `_slotStatusParent: Transform`
- `_recipeSlotPrefab: GameObject`
- `_processingSlotPrefab: GameObject`

공개 메서드:
- `Open(BuildingInstance processor)`
- `OnRecipeSelected(ProcessingRecipeData recipe)`
- `OnCollectClicked(int slotIndex)`
- `Close()`

#### P-7-02: RecipeSlotUI.cs 생성

```
create_script
  path: "Assets/_Project/Scripts/UI/RecipeSlotUI.cs"
  namespace: "SeedMind.UI"
  base_class: "MonoBehaviour"
```

개별 레시피 표시 UI 컴포넌트. 아이콘, 이름, 재료 요구량, 가공 시간 표시.

#### P-7-03: ProcessingSlotUI.cs 생성

```
create_script
  path: "Assets/_Project/Scripts/UI/ProcessingSlotUI.cs"
  namespace: "SeedMind.UI"
  base_class: "MonoBehaviour"
```

개별 가공 슬롯 상태 UI 컴포넌트. 프로그레스 바, 수거 버튼 포함.

#### P-7-04: Unity 컴파일 대기

#### P-7 검증 체크리스트

- [ ] 3개 UI 스크립트 파일 존재
- [ ] Unity 컴파일 에러 없음
- [ ] ProcessingUI가 MonoBehaviour 상속

**P-7 MCP 호출 합계**: 3(생성) + 1(컴파일) = 4회

---

## Part III -- 씬 배치 및 연결

### Task P-8: 시설 Prefab에 가공 관련 설정 연결

**목표**: 가공소 4종의 BuildingData SO를 BuildingManager의 레지스트리에 등록하여, 건설 시스템에서 접근 가능하게 한다.

#### P-8-01: BuildingManager에 가공소 BuildingData 등록

```
set_property  target: "BuildingManager" (씬 내 GameObject)
  _buildingDatabase = [
    ...(기존 BuildingData 목록),
    SO_Building_Processing,
    SO_Building_Mill,
    SO_Building_Fermentation,
    SO_Building_Bakery
  ]
```

[RISK] MCP로 SO 배열에 참조를 추가하는 것이 가능한지 미확인. 대안: BuildingManager.Awake()에서 Resources.LoadAll로 자동 로드.

- **MCP 호출**: 1~4회 (배열 추가 방식에 따라)

#### P-8 검증 체크리스트

- [ ] BuildingManager의 빌딩 데이터베이스에 4종 가공소 등록 확인
- [ ] 각 SO의 dataId가 canonical과 일치

**P-8 MCP 호출 합계**: ~4회

---

### Task P-9: ProcessingPanel UI 씬 배치

**목표**: Canvas_Overlay 하위에 ProcessingPanel을 생성하고, ProcessingUI 컴포넌트를 연결한다.

UI 계층 구조: (-> see `docs/systems/processing-architecture.md` 섹션 7.1)

#### P-9-01: ProcessingPanel GameObject 생성

```
create_object
  name: "ProcessingPanel"
  parent: "Canvas_Overlay"

set_property  target: "ProcessingPanel"
  RectTransform.anchorMin = (0, 0)
  RectTransform.anchorMax = (1, 1)
  RectTransform.offsetMin = (50, 50)
  RectTransform.offsetMax = (-50, -50)
  activeInHierarchy = false                          // 기본 비활성 (가공소 인터랙션 시 활성화)
```

#### P-9-02: RecipeListArea 생성

```
create_object
  name: "RecipeListArea"
  parent: "ProcessingPanel"

set_property  target: "RecipeListArea"
  RectTransform.anchorMin = (0, 0)
  RectTransform.anchorMax = (0.5, 1)
  RectTransform.offsetMin = (10, 50)
  RectTransform.offsetMax = (-5, -10)
```

#### P-9-03: SlotStatusArea 생성

```
create_object
  name: "SlotStatusArea"
  parent: "ProcessingPanel"

set_property  target: "SlotStatusArea"
  RectTransform.anchorMin = (0.5, 0)
  RectTransform.anchorMax = (1, 1)
  RectTransform.offsetMin = (5, 50)
  RectTransform.offsetMax = (-10, -10)
```

#### P-9-04: CloseButton 생성

```
create_object
  name: "CloseButton"
  parent: "ProcessingPanel"

add_component  target: "CloseButton"
  type: "UnityEngine.UI.Button"

set_property  target: "CloseButton"
  RectTransform.anchorMin = (0.9, 0.9)
  RectTransform.anchorMax = (1, 1)
  RectTransform.offsetMin = (0, 0)
  RectTransform.offsetMax = (0, 0)
```

#### P-9-05: ProcessingUI 컴포넌트 부착

```
add_component  target: "ProcessingPanel"
  type: "SeedMind.UI.ProcessingUI"

set_property  target: "ProcessingPanel.ProcessingUI"
  _recipeListParent = [RecipeListArea Transform 참조]
  _slotStatusParent = [SlotStatusArea Transform 참조]
```

[RISK] SerializeField 오브젝트 참조 설정이 MCP로 가능한지 미확인. 대안: ProcessingUI.Awake()에서 `transform.Find()`로 자동 탐색.

#### P-9-06: 씬 저장

```
save_scene
```

#### P-9 검증 체크리스트

- [ ] `Canvas_Overlay/ProcessingPanel` 존재, 기본 비활성
- [ ] `ProcessingPanel` 하위에 RecipeListArea, SlotStatusArea, CloseButton 존재
- [ ] ProcessingUI 컴포넌트 부착 확인
- [ ] CloseButton에 Button 컴포넌트 존재
- [ ] 콘솔 에러 없음

**P-9 MCP 호출 합계**: 4(오브젝트 생성) + ~12(프로퍼티 설정) + 2(컴포넌트) + 1(저장) = ~19회

---

### Task P-10: RecipeSlotUI / ProcessingSlotUI 프리팹 생성

**목표**: 레시피 슬롯과 가공 슬롯의 UI 프리팹을 생성한다.

#### P-10-01: RecipeSlotUI 프리팹

```
create_object
  name: "PFB_RecipeSlot"

add_component  target: "PFB_RecipeSlot"
  type: "SeedMind.UI.RecipeSlotUI"

// 하위 UI 요소 생성 (아이콘, 이름, 재료 텍스트, 시간 텍스트)
create_object  name: "Icon", parent: "PFB_RecipeSlot"
create_object  name: "RecipeName", parent: "PFB_RecipeSlot"
create_object  name: "MaterialText", parent: "PFB_RecipeSlot"
create_object  name: "TimeText", parent: "PFB_RecipeSlot"

save_as_prefab
  source: "PFB_RecipeSlot"
  path: "Assets/_Project/Prefabs/UI/PFB_RecipeSlot.prefab"
```

[RISK] `save_as_prefab` MCP 도구 가용 여부 미확인. 대안: PrefabUtility Editor 스크립트.

#### P-10-02: ProcessingSlotUI 프리팹

```
create_object
  name: "PFB_ProcessingSlot"

add_component  target: "PFB_ProcessingSlot"
  type: "SeedMind.UI.ProcessingSlotUI"

// 하위 UI 요소 생성 (상태 텍스트, 프로그레스 바, 수거 버튼)
create_object  name: "StatusText", parent: "PFB_ProcessingSlot"
create_object  name: "ProgressBar", parent: "PFB_ProcessingSlot"
create_object  name: "CollectButton", parent: "PFB_ProcessingSlot"

save_as_prefab
  source: "PFB_ProcessingSlot"
  path: "Assets/_Project/Prefabs/UI/PFB_ProcessingSlot.prefab"
```

#### P-10-03: ProcessingUI에 프리팹 참조 연결

```
set_property  target: "ProcessingPanel.ProcessingUI"
  _recipeSlotPrefab = [PFB_RecipeSlot 프리팹 참조]
  _processingSlotPrefab = [PFB_ProcessingSlot 프리팹 참조]
```

#### P-10 검증 체크리스트

- [ ] `Assets/_Project/Prefabs/UI/PFB_RecipeSlot.prefab` 존재
- [ ] `Assets/_Project/Prefabs/UI/PFB_ProcessingSlot.prefab` 존재
- [ ] 각 프리팹에 해당 UI 컴포넌트 부착 확인
- [ ] ProcessingUI에 프리팹 참조 연결 확인

**P-10 MCP 호출 합계**: ~20회

---

## Part IV -- 통합 테스트 시퀀스

### Task P-11: 기본 가공 플로우 테스트

**목표**: 가공소 건설 -> 레시피 선택 -> 가공 시작 -> 시간 경과 -> 가공 완료 -> 결과물 수거의 전체 플로우를 검증한다.

#### P-11-01: Play Mode 진입

```
enter_play_mode
```

#### P-11-02: 가공소 건설 트리거

MCP 콘솔을 통해 가공소를 즉시 건설한다.

```
execute_method
  target: "BuildingManager"
  method: "DebugBuildInstant"
  args: ["building_processing", 10, 10]              // dataId, gridX, gridY
```

> 테스트용 Debug 메서드가 BuildingManager에 존재해야 한다.

#### P-11-03: 가공 시작 테스트

```
execute_method
  target: "BuildingManager"
  method: "DebugStartProcessing"
  args: ["building_processing", "recipe_jam_potato", "potato", 1]
```

#### P-11-04: 시간 경과 시뮬레이션

```
execute_method
  target: "TimeManager"
  method: "DebugAdvanceHours"
  args: [4]                                          // 잼 기본 가공 시간 (-> see docs/content/processing-system.md 섹션 3.1)
```

#### P-11-05: 가공 완료 확인

```
get_console_logs
  filter: "ProcessingComplete"
```

기대 로그: `[ProcessingSystem] Processing complete: building_processing, slot 0, recipe_jam_potato`

#### P-11-06: 결과물 수거 테스트

```
execute_method
  target: "BuildingManager"
  method: "DebugCollectOutput"
  args: ["building_processing", 0]                   // buildingId, slotIndex
```

#### P-11-07: 인벤토리 확인

```
get_console_logs
  filter: "InventoryAdd"
```

기대 로그: `[InventoryManager] Added: jam_potato x1`

#### P-11-08: Play Mode 종료

```
exit_play_mode
```

#### P-11 검증 체크리스트

- [ ] 가공소 건설 성공 (OnBuildingCompleted 이벤트 발행)
- [ ] ProcessingSystem.RegisterProcessor 호출 확인
- [ ] StartProcessing 성공 (OnProcessingStarted 이벤트 발행)
- [ ] 4시간 경과 후 OnProcessingComplete 이벤트 발행
- [ ] CollectOutput 성공 (OnProcessingCollected 이벤트 발행)
- [ ] 인벤토리에 jam_potato 추가 확인
- [ ] 슬롯 상태가 Empty로 복귀
- [ ] 콘솔 에러 없음

**P-11 MCP 호출 합계**: 8회

---

### Task P-12: 연료 소모 테스트

**목표**: 베이커리에서 장작을 소모하는 가공 플로우를 검증한다.

#### P-12-01: Play Mode 진입 + 베이커리 건설

```
enter_play_mode

execute_method
  target: "BuildingManager"
  method: "DebugBuildInstant"
  args: ["building_bakery", 15, 10]
```

#### P-12-02: 인벤토리에 재료 + 연료 추가

```
execute_method
  target: "InventoryManager"
  method: "DebugAddItem"
  args: ["corn_flour", 1]                            // 제분소 산출물

execute_method
  target: "InventoryManager"
  method: "DebugAddItem"
  args: ["item_firewood", 5]                         // 장작
```

#### P-12-03: 연료 소모 가공 시작

```
execute_method
  target: "BuildingManager"
  method: "DebugStartProcessing"
  args: ["building_bakery", "recipe_bake_corn_bread", "corn_flour", 1]
```

#### P-12-04: 연료 차감 확인

```
get_console_logs
  filter: "RemoveItem.*firewood"
```

기대 로그: `[InventoryManager] Removed: item_firewood x1` (-> see `docs/content/processing-system.md` 섹션 3.4 fuelCost)

#### P-12-05: 연료 부족 시 실패 테스트

```
// 인벤토리에서 장작을 모두 제거
execute_method
  target: "InventoryManager"
  method: "DebugRemoveItem"
  args: ["item_firewood", 99]

// 가공 시도 -> 실패 기대
execute_method
  target: "BuildingManager"
  method: "DebugStartProcessing"
  args: ["building_bakery", "recipe_bake_corn_bread", "corn_flour", 1]

get_console_logs
  filter: "FuelInsufficient"
```

기대 로그: `[ProcessingSystem] Fuel insufficient: item_firewood required 1, has 0`

#### P-12-06: Play Mode 종료

```
exit_play_mode
```

#### P-12 검증 체크리스트

- [ ] 베이커리 가공 시작 시 장작이 인벤토리에서 즉시 차감
- [ ] 장작 부족 시 가공 시작 실패
- [ ] 가공소(일반)/제분소/발효실은 연료 차감 없이 가공 시작 성공
- [ ] 콘솔 에러 없음

**P-12 MCP 호출 합계**: ~10회

---

### Task P-13: 세이브/로드 통합 테스트

**목표**: 가공 중인 상태에서 저장 후 로드 시 가공 상태가 정확히 복원되는지 검증한다.

저장/복원 구조: (-> see `docs/systems/processing-architecture.md` 섹션 5)

#### P-13-01: Play Mode 진입 + 가공 시작

```
enter_play_mode

execute_method  target: "BuildingManager"
  method: "DebugBuildInstant"  args: ["building_processing", 10, 10]

execute_method  target: "BuildingManager"
  method: "DebugStartProcessing"
  args: ["building_processing", "recipe_jam_potato", "potato", 1]
```

#### P-13-02: 시간 일부 경과 (가공 진행 중 상태)

```
execute_method  target: "TimeManager"
  method: "DebugAdvanceHours"  args: [2]             // 4시간 중 2시간 경과 -> 50% 진행
```

#### P-13-03: 저장 실행

```
execute_method  target: "SaveManager"
  method: "Save"  args: [0]                          // 슬롯 0
```

#### P-13-04: 가공 상태 초기화 후 로드

```
execute_method  target: "SaveManager"
  method: "Load"  args: [0]                          // 슬롯 0
```

#### P-13-05: 복원 상태 확인

```
get_console_logs
  filter: "ProcessingSystem.*Load"
```

기대 확인 사항:
- 슬롯 상태 = Processing
- 남은 시간 = 약 2시간 (저장 시점 값)
- 레시피 ID = recipe_jam_potato
- 입력 작물 = potato

#### P-13-06: 복원 후 가공 완료까지 진행

```
execute_method  target: "TimeManager"
  method: "DebugAdvanceHours"  args: [2]             // 잔여 2시간 경과

get_console_logs
  filter: "ProcessingComplete"
```

기대 로그: 가공 완료 이벤트 정상 발행

#### P-13-07: Play Mode 종료 + 씬 저장

```
exit_play_mode
save_scene
```

#### P-13 검증 체크리스트

- [ ] 저장 데이터에 ProcessingSaveData 포함 확인
- [ ] ProcessingSaveData.remainingHours가 저장 시점의 값과 일치
- [ ] ProcessingSaveData.recipeId가 정확
- [ ] 로드 후 슬롯 상태(State, RemainingHours, TotalHours) 정확히 복원
- [ ] 복원 후 시간 경과 -> 가공 완료 정상 동작
- [ ] 콘솔 에러 없음

**P-13 MCP 호출 합계**: ~12회

---

## 실행 순서 다이어그램

```
Phase A: 데이터 레이어                    Phase B: 시스템 레이어
┌──────────────────────────┐          ┌──────────────────────────┐
│ P-4: 데이터 스크립트 생성  │          │ P-5: ProcessingSystem    │
│  - ProcessingType.cs     │──────────│  - ProcessingSystem.cs   │
│  - ProcessingRecipeData  │  의존    │  - BuildingManager 수정  │
│  - ProcessingSlot.cs     │          │  - BuildingEvents 수정   │
│  - ProcessingSaveData.cs │          │                          │
│  [컴파일 대기]            │          │ P-6: DataRegistry 등록   │
└──────────────────────────┘          │  [컴파일 대기]            │
                                      └──────────┬───────────────┘
                                                  │
Phase C: SO 에셋 생성                              │ 의존
┌──────────────────────────┐                      │
│ P-1: BuildingData SO x4  │◄─────────────────────┘
│ P-2: RecipeData SO x32   │
│ P-3: FuelData SO x1      │
│  [37 + 3(ALT) + 8 MCP 호출] │
└──────────┬───────────────┘
           │
Phase D: UI 레이어                     Phase E: 씬 배치
┌──────────────────────────┐          ┌──────────────────────────┐
│ P-7: UI 스크립트 생성     │          │ P-8: BuildingManager     │
│  - ProcessingUI.cs       │──────────│      데이터 연결         │
│  - RecipeSlotUI.cs       │  의존    │ P-9: ProcessingPanel     │
│  - ProcessingSlotUI.cs   │          │      씬 배치             │
│  [컴파일 대기]            │          │ P-10: UI 프리팹 생성     │
└──────────────────────────┘          │  [씬 저장]               │
                                      └──────────┬───────────────┘
                                                  │
Phase F: 통합 테스트                               │ 의존
┌──────────────────────────┐                      │
│ P-11: 기본 가공 플로우    │◄─────────────────────┘
│ P-12: 연료 소모 테스트    │
│ P-13: 세이브/로드 테스트  │
└──────────────────────────┘
```

### 총 MCP 호출 추정

| Task | 호출 수 | 비고 |
|------|--------|------|
| P-1 | 37 | BuildingData SO x4 |
| P-2-ALT | **3** | **기본 경로** — Editor 스크립트 일괄 생성 (P-2 개별 517회 대신 사용) |
| P-3 | 8 | FuelData SO x1 |
| P-4 | 5 | 데이터 스크립트 4개 + 컴파일 |
| P-5 | 4 | 시스템 스크립트 + 수정 |
| P-6 | 3 | DataRegistry 수정 |
| P-7 | 4 | UI 스크립트 3개 + 컴파일 |
| P-8 | ~4 | BuildingManager 데이터 연결 |
| P-9 | ~19 | ProcessingPanel 씬 배치 |
| P-10 | ~20 | UI 프리팹 2종 |
| P-11 | 8 | 기본 플로우 테스트 |
| P-12 | ~10 | 연료 테스트 |
| P-13 | ~12 | 세이브/로드 테스트 |
| **총합** | **~139** | P-2-ALT 적용 기준 (P-2 단독 실행 시 ~651) |

---

## Cross-references

| 참조 문서 | 관련 내용 |
|----------|----------|
| `docs/systems/processing-architecture.md` (ARC-012) | 가공 시스템 전체 기술 아키텍처 (Part II가 본 문서의 원본) |
| `docs/content/processing-system.md` (CON-005) | 레시피 32종 canonical, 연료 시스템, 특화 가공소 3종 |
| `docs/pipeline/data-pipeline.md` (ARC-004) | ProcessingRecipeData 필드 정의 섹션 2.5, ProcessingSaveData 섹션 2.6 |
| `docs/systems/facilities-architecture.md` (ARC-007) | BuildingManager, BuildingData, BuildingEvents |
| `docs/systems/save-load-architecture.md` (DES-008) | GameSaveData.processing[], SaveLoadOrder |
| `docs/mcp/scene-setup-tasks.md` (ARC-002) | 기본 씬 구성 (Canvas_Overlay 등 전제 조건) |
| `docs/mcp/farming-tasks.md` (ARC-003) | 농장 태스크 형식 참조, CropData SO |
| `docs/systems/project-structure.md` | 네임스페이스, 폴더 구조, 의존성 규칙 |
| `docs/systems/economy-system.md` 섹션 2.5 | 가공 공식, 배수, 분류 canonical |
| `docs/systems/progression-architecture.md` | XPSource.Processing 경험치 연동 |

---

## Open Questions

1. **[OPEN]** `create_scriptable_object` MCP 도구의 정확한 파라미터 형식이 미확인이다. SO 생성 시 타입 문자열의 네임스페이스 포함 여부, 필드 설정의 배치(batch) 지원 여부를 사전 테스트해야 한다.

2. **[OPEN]** 베이커리 레시피의 복수 재료 지원 문제. 현재 ProcessingRecipeData 스키마는 `inputItemId` 1개만 지원하므로, 호박 파이(호박 분말 + 딸기 잼) 등 복수 재료 레시피를 정확히 표현할 수 없다. `InputRequirement[]` 배열 필드 추가 또는 별도 `BakeryRecipeData` SO 생성이 필요하다.

3. ~~**[OPEN]** P-2의 517회 MCP 호출을 Editor 스크립트로 대체할 경우, 해당 스크립트의 위치와 실행 방법을 별도로 문서화해야 한다.~~ **[RESOLVED]** P-2-ALT 섹션에 `Assets/_Project/Editor/CreateAllRecipeSOs.cs` MenuItem 방식으로 정의 완료.

4. **[OPEN]** SO 에셋의 오브젝트 참조 필드(Sprite icon, GameObject prefab 등) 설정이 MCP로 가능한지 미확인. 불가능한 경우 해당 필드는 Unity Editor에서 수동 연결이 필요하며, 이 수동 작업 목록을 별도 정리해야 한다.

5. **[OPEN]** ItemData SO 스키마(P-3에서 사용)가 아직 canonical로 정의되지 않았다. 장작뿐 아니라 가공품 결과물(jam_potato 등) 32종도 ItemData SO가 필요하다. 이 에셋 생성 태스크가 본 문서 범위에 포함되어야 하는지, 별도 태스크로 분리할지 결정 필요.

---

*이 문서는 Claude Code가 processing-architecture.md Part II를 기반으로, MCP for Unity 도구 호출 수준의 상세 태스크 시퀀스로 확장하여 자율적으로 작성했습니다.*
