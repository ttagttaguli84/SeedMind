# 작물 콘텐츠 MCP 구현 계획

> Unity에 전체 작물 데이터를 생성하는 MCP 태스크 시퀀스 설계  
> 작성: Claude Code (Opus) | 2026-04-06  
> 문서 ID: CON-001-ARC

---

## Context

이 문서는 `docs/pipeline/data-pipeline.md`에 정의된 CropData ScriptableObject 스키마를 Unity 에디터에 구체적으로 구현하기 위한 MCP 태스크 시퀀스를 정의한다. 작물 8종(봄~가을)과 겨울 작물(`-> see docs/content/crops.md`)에 대해 SO 에셋 생성, 필드 설정, 프리팹 구조 생성, DataRegistry 등록, 검증까지의 전체 파이프라인을 다룬다.

**전제 조건**:
- `docs/mcp/scene-setup-tasks.md`의 기본 씬 구성이 완료된 상태
- `docs/mcp/farming-tasks.md`의 FarmGrid/FarmTile 구조가 존재하는 상태
- `GameDataSO` 베이스 클래스가 `Assets/_Project/Scripts/Core/` 에 생성된 상태 (`-> see docs/pipeline/data-pipeline.md` Part II 섹션 1)

---

# Part I -- CropData ScriptableObject 설계

---

## 1. CropData C# 클래스 정의

CropData는 `GameDataSO`를 상속하고 `IInventoryItem` 인터페이스를 구현한다.

**파일 위치**: `Assets/_Project/Scripts/Farm/Data/CropData.cs`  
**네임스페이스**: `SeedMind.Farm.Data` (`-> see docs/systems/project-structure.md` 섹션 2)

```csharp
// illustrative, not executable
using UnityEngine;
using SeedMind.Core;

namespace SeedMind.Farm.Data
{
    [CreateAssetMenu(fileName = "NewCrop", menuName = "SeedMind/CropData")]
    public class CropData : GameDataSO, IInventoryItem
    {
        // ── 기본 정보 ──────────────────────────────────────
        // dataId, displayName, icon 은 GameDataSO에서 상속
        // (→ see docs/pipeline/data-pipeline.md Part II 섹션 1)

        [Header("작물 식별")]
        public string cropId;                    // 코드용 고유 식별자 (예: "crop_potato")
        public string cropName;                  // 표시 이름 (예: "감자")
        public CropCategory cropCategory;        // 작물 분류 (→ see docs/pipeline/data-pipeline.md 섹션 2.1)

        [Header("계절")]
        public SeasonFlag allowedSeasons;        // 재배 가능 계절 비트마스크
                                                 // (→ see docs/systems/crop-growth.md 섹션 3.1)

        [Header("성장")]
        public int growthDays;                   // 총 성장 일수
                                                 // (→ see docs/design.md 섹션 4.2)
        public int growthStageCount = 4;         // 시각적 단계 수 (기본 4)
                                                 // (→ see docs/systems/crop-growth.md)

        [Header("경제")]
        public int seedPrice;                    // 씨앗 구매가
                                                 // (→ see docs/design.md 섹션 4.2)
        public int sellPrice;                    // 수확물 판매가
                                                 // (→ see docs/design.md 섹션 4.2)

        [Header("수확")]
        public int baseYield = 1;                // 기본 수확량
                                                 // (→ see docs/systems/farming-architecture.md 섹션 4.1)
        public float qualityChance;              // 고품질 확률
                                                 // (→ see docs/systems/farming-architecture.md 섹션 4.1)

        [Header("반복 수확")]
        public bool isRepeating;                 // 반복 수확 가능 여부 (딸기 등)
                                                 // (→ see docs/pipeline/data-pipeline.md 섹션 2.1)
        public int regrowDays;                   // 재수확까지 일수 (isRepeating=true일 때만 유효)
                                                 // (→ see docs/pipeline/data-pipeline.md 섹션 2.1)

        [Header("거대 작물")]
        public float giantCropChance;            // 거대 작물 변이 확률
                                                 // (→ see docs/systems/crop-growth.md 섹션 5.1)
        public GameObject giantCropPrefab;       // 거대 작물 프리팹 참조

        [Header("해금")]
        public int unlockLevel;                  // 해금 레벨
                                                 // (→ see docs/design.md 섹션 4.2)

        [Header("비주얼")]
        public GameObject[] growthStagePrefabs;  // 단계별 3D 모델 (길이 = growthStageCount)
        public Material soilMaterial;            // 심어졌을 때 토양 머티리얼 오버라이드
        public GameObject harvestParticle;       // 수확 시 파티클 프리팹

        [Header("온실")]
        public bool requiresGreenhouse;          // 온실 필수 여부 (겨울 작물)

        [Header("설명")]
        [TextArea(2, 4)]
        public string description;               // UI 표시용 작물 설명

        // ── IInventoryItem 구현 ─────────────────────────────
        // (→ see docs/systems/inventory-architecture.md 섹션 3.1)

        public string ItemId => dataId;          // GameDataSO.dataId 재사용
        public string ItemName => displayName;   // GameDataSO.displayName 재사용
        public ItemType ItemType => ItemType.Crop;
        public Sprite Icon => icon;              // GameDataSO.icon 재사용
        public int MaxStackSize => 99;           // 작물 스택 한계
                                                 // (→ see docs/systems/inventory-architecture.md 섹션 4.1)
        public bool Sellable => true;            // 모든 작물은 판매 가능
    }
}
```

**필드 수 검증 (PATTERN-005)**:

| 출처 | 필드 수 | 비고 |
|------|---------|------|
| farming-architecture.md 섹션 4.1 기본 필드 | 12 | cropName, cropId, seedPrice, sellPrice, growthDays, growthStageCount, allowedSeasons, baseYield, qualityChance, unlockLevel, growthStagePrefabs, soilMaterial |
| data-pipeline.md 섹션 2.1 확장 필드 | 7 | cropCategory, isRepeating, regrowDays, giantCropChance, giantCropPrefab, harvestParticle, description |
| 본 문서 추가 필드 | 1 | requiresGreenhouse (온실 필수 여부) |
| GameDataSO 상속 필드 | 3 | dataId, displayName, icon |
| IInventoryItem 프로퍼티 | 6 | ItemId, ItemName, ItemType, Icon, MaxStackSize, Sellable |

---

## 2. SeasonFlag enum

이미 `docs/systems/farming-architecture.md` 섹션 4.1에 canonical 정의됨. 재기재하지 않는다.

(`-> see docs/systems/farming-architecture.md` 섹션 4.1 SeasonFlag enum)

---

## 3. CropCategory enum

이미 `docs/pipeline/data-pipeline.md` 섹션 2.1에 canonical 정의됨. 재기재하지 않는다.

(`-> see docs/pipeline/data-pipeline.md` 섹션 2.1 CropCategory enum)

---

# Part II -- MCP 태스크 시퀀스

---

## Phase A: 스크립트 생성 (의존성 없음)

### Task A-1: CropData.cs 스크립트 생성

```
도구: create_script
경로: Assets/_Project/Scripts/Farm/Data/CropData.cs
설명: Part I 섹션 1에 정의된 CropData 클래스를 생성한다.
      GameDataSO 상속, IInventoryItem 구현.
의존성: GameDataSO.cs, IInventoryItem.cs 가 존재해야 함
       (→ see docs/pipeline/data-pipeline.md Part II 섹션 1)
       (→ see docs/systems/inventory-architecture.md 섹션 3.1)
```

### Task A-2: CropCategory enum 생성

```
도구: create_script
경로: Assets/_Project/Scripts/Farm/Data/CropCategory.cs
설명: CropCategory enum 정의 (Vegetable, Fruit, FruitVegetable, Special)
      (→ see docs/pipeline/data-pipeline.md 섹션 2.1 for canonical 정의)
의존성: 없음
```

### Task A-3: SeasonFlag enum 확인

```
도구: verify_script (읽기 전용)
경로: Assets/_Project/Scripts/Farm/Data/SeasonFlag.cs (또는 TileState.cs 내 정의)
설명: SeasonFlag enum이 이미 존재하는지 확인.
      없으면 생성 (→ see docs/systems/farming-architecture.md 섹션 4.1)
의존성: 없음
```

---

## Phase B: CropData SO 에셋 생성 (8종 + 겨울 작물)

### Task B-1: 작물 SO 에셋 일괄 생성

대상 작물 8종 (`-> see docs/design.md 섹션 4.2 for 전체 목록`):

| 에셋 파일명 | cropId |
|-------------|--------|
| SO_Crop_Potato.asset | crop_potato |
| SO_Crop_Carrot.asset | crop_carrot |
| SO_Crop_Tomato.asset | crop_tomato |
| SO_Crop_Corn.asset | crop_corn |
| SO_Crop_Strawberry.asset | crop_strawberry |
| SO_Crop_Pumpkin.asset | crop_pumpkin |
| SO_Crop_Sunflower.asset | crop_sunflower |
| SO_Crop_Watermelon.asset | crop_watermelon |

겨울 작물: (`-> see docs/content/crops.md` for 추가 작물 목록 및 수치)

```
도구: create_scriptable_object (반복 x N)
경로: Assets/_Project/Data/Crops/<에셋 파일명>
설명: CropData 타입의 SO 에셋을 생성한다.
      에셋 네이밍 규칙: SO_Crop_<PascalCase이름>.asset
      (→ see docs/systems/project-structure.md 섹션 6.1 네이밍 규칙)
의존성: Task A-1 (CropData.cs 존재)
```

### Task B-2: 기본 작물 SO 필드 설정 (8종)

각 SO 에셋에 대해 아래 필드를 설정한다.

```
도구: set_field (반복 x 8 작물 x N 필드)
대상: Assets/_Project/Data/Crops/SO_Crop_<Name>.asset
설명: 각 작물 SO에 필드값을 설정한다.

필드 설정 목록:
  - dataId         : "crop_<name>"       (예: "crop_potato")
  - displayName    : 한국어 이름          (→ see docs/design.md 섹션 4.2)
  - cropId         : "crop_<name>"
  - cropName       : 한국어 이름          (→ see docs/design.md 섹션 4.2)
  - cropCategory   : CropCategory 값     (→ see docs/pipeline/data-pipeline.md 섹션 2.1)
  - allowedSeasons : SeasonFlag 비트마스크 (→ see docs/systems/crop-growth.md 섹션 3.1)
  - growthDays     : int                  (→ see docs/design.md 섹션 4.2)
  - growthStageCount: 4
  - seedPrice      : int                  (→ see docs/design.md 섹션 4.2)
  - sellPrice      : int                  (→ see docs/design.md 섹션 4.2)
  - baseYield      : 1
  - qualityChance  : float                (→ see docs/systems/farming-architecture.md 섹션 4.1)
  - isRepeating    : bool                 (→ see docs/pipeline/data-pipeline.md 섹션 2.1)
  - regrowDays     : int                  (→ see docs/pipeline/data-pipeline.md 섹션 2.1)
  - giantCropChance: float                (→ see docs/systems/crop-growth.md 섹션 5.1)
  - unlockLevel    : int                  (→ see docs/design.md 섹션 4.2)
  - requiresGreenhouse: false             (봄~가을 작물은 모두 false)
  - description    : 작물 설명 텍스트

의존성: Task B-1
```

### Task B-3: 겨울 작물 SO 생성 및 설정

```
도구: create_scriptable_object + set_field
경로: Assets/_Project/Data/Crops/SO_Crop_<WinterCropName>.asset
설명: 겨울 작물 SO를 생성하고 필드를 설정한다.
      작물 목록, 이름, 수치: (→ see docs/content/crops.md)
      requiresGreenhouse: true
      allowedSeasons: SeasonFlag.Winter
의존성: Task A-1, docs/content/crops.md 확정 후 실행
```

겨울 작물 3종: 겨울무(SO_Crop_WinterRadish.asset), 표고버섯(SO_Crop_Shiitake.asset), 시금치(SO_Crop_Spinach.asset) — 상세 수치는 (→ see docs/content/crops.md 섹션 3.9~3.11)

---

## Phase C: 프리팹 구조 생성

### Task C-1: 작물 프리팹 폴더 생성

```
도구: create_folder (반복 x 작물 수)
경로 패턴:
  Assets/_Project/Prefabs/Crops/<CropId>/
설명: 각 작물별 프리팹 루트 폴더를 생성한다.
      (→ see docs/systems/project-structure.md 섹션 1 for 폴더 구조)

생성할 폴더:
  Assets/_Project/Prefabs/Crops/Potato/
  Assets/_Project/Prefabs/Crops/Carrot/
  Assets/_Project/Prefabs/Crops/Tomato/
  Assets/_Project/Prefabs/Crops/Corn/
  Assets/_Project/Prefabs/Crops/Strawberry/
  Assets/_Project/Prefabs/Crops/Pumpkin/
  Assets/_Project/Prefabs/Crops/Sunflower/
  Assets/_Project/Prefabs/Crops/Watermelon/
  (+ 겨울 작물 → see docs/content/crops.md)

의존성: 없음
```

### Task C-2: 작물 프리팹 생성 (단계별)

각 작물에 대해 4단계 프리팹을 생성한다 (Part III 프리팹 구조 참조).

```
도구: create_prefab (반복 x 작물 수 x 4단계)
경로 패턴:
  Assets/_Project/Prefabs/Crops/<CropName>/PFB_Crop_<CropName>_Stage<N>.prefab

예시 (감자):
  PFB_Crop_Potato_Stage0.prefab   (씨앗)
  PFB_Crop_Potato_Stage1.prefab   (새싹)
  PFB_Crop_Potato_Stage2.prefab   (성장)
  PFB_Crop_Potato_Stage3.prefab   (수확 가능)

설명: Part III에 정의된 계층 구조로 프리팹을 생성한다.
      각 프리팹은 placeholder 메시(Cube/Sphere/Cylinder)를 사용하며,
      이후 아트 에셋으로 교체한다.
의존성: Task C-1
```

### Task C-3: Giant Crop 프리팹 생성

거대 작물 가능 종에 대해 Giant 프리팹을 생성한다.

```
도구: create_prefab (대상 작물만)
경로 패턴:
  Assets/_Project/Prefabs/Crops/<CropName>/PFB_Crop_<CropName>_Giant.prefab

대상: giantCropChance > 0 인 작물
     (→ see docs/systems/crop-growth.md 섹션 5.1 for 대상 작물 목록)

설명: 9타일(3x3) 크기의 Giant Crop 프리팹. (→ see docs/systems/crop-growth.md 섹션 5.1)
      Scale을 3배로 설정하고, 중심점을 (1.0, 0, 1.0) 오프셋.
의존성: Task C-1
```

### Task C-4: 프리팹 참조를 SO에 연결

```
도구: set_field (반복 x 작물 수)
대상: Assets/_Project/Data/Crops/SO_Crop_<Name>.asset
필드: growthStagePrefabs (배열), giantCropPrefab (해당 시)
설명: Task C-2에서 생성한 프리팹 참조를 각 CropData SO의
      growthStagePrefabs 배열에 순서대로 할당한다.
      Giant 프리팹이 있는 작물은 giantCropPrefab도 설정.
의존성: Task B-1, Task C-2, Task C-3
```

---

## Phase D: 머티리얼 생성

### Task D-1: 작물 머티리얼 생성

```
도구: create_material (반복 x 작물 수)
경로 패턴:
  Assets/_Project/Materials/Crops/M_Crop_<CropName>.mat

설명: 각 작물의 기본 색상 머티리얼을 생성한다.
      URP/Lit 셰이더, Base Color만 설정.
      색상값은 placeholder (성장 단계별 색 분화는 후속 태스크에서 처리).
의존성: 없음
```

### Task D-2: 토양 오버라이드 머티리얼 연결

```
도구: set_field (반복 x 작물 수)
대상: Assets/_Project/Data/Crops/SO_Crop_<Name>.asset
필드: soilMaterial
설명: 이미 존재하는 M_Soil_Planted.mat 등을 soilMaterial에 할당.
      (→ see docs/mcp/farming-tasks.md for 토양 머티리얼)
의존성: Task B-1, farming-tasks Phase B 머티리얼
```

---

## Phase E: DataRegistry 등록

### Task E-1: DataRegistry에 CropData 로딩 로직 확인

```
도구: verify_script (읽기 전용)
경로: Assets/_Project/Scripts/Core/DataRegistry.cs
설명: DataRegistry가 CropData SO를 자동 로드하는지 확인.
      Resources.LoadAll<CropData>("") 또는 유사 메커니즘이
      Dictionary<string, CropData> 캐시에 등록하는지 검증.
      (→ see docs/pipeline/data-pipeline.md Part II 섹션 1)
의존성: Task A-1
```

### Task E-2: SO 에셋 경로를 DataRegistry 로드 범위에 포함

```
도구: verify_folder / move_asset (필요시)
설명: Assets/_Project/Data/Crops/ 폴더가 DataRegistry의 로드 경로에
      포함되는지 확인한다.
      - Addressables 미사용 시: Resources 폴더 하위이거나 직접 참조
      - 현재 설계: Resources + 직접 참조 방식
        (→ see docs/systems/project-structure.md Open Questions 섹션)
의존성: Task B-1, Task E-1
```

---

# Part III -- 프리팹 구조 설계

---

## 1. 일반 작물 프리팹 계층

각 성장 단계 프리팹의 내부 구조:

```
PFB_Crop_<Name>_Stage<N> (GameObject, root)
│   - Transform: position (0,0,0), 1타일 크기에 맞춤
│   - Tag: "Crop"
│   - Layer: "Interactable"
│
├── Model (GameObject)
│   - MeshFilter: placeholder 메시 (단계별 상이)
│   - MeshRenderer: M_Crop_<Name>.mat
│   - 단계별 메시 가이드:
│       Stage 0 (씨앗): 작은 Sphere (scale 0.1)
│       Stage 1 (새싹): 작은 Cylinder (scale 0.15, height 0.3)
│       Stage 2 (성장): Cylinder (scale 0.25, height 0.6)
│       Stage 3 (수확): Cylinder + Sphere (scale 0.3, height 0.8)
│
└── Collider (GameObject)
    - BoxCollider: 크기는 1타일 영역 (1x1x1)
    - isTrigger: true (플레이어 상호작용 감지용)
```

**단계 전환 방식**:
- CropInstance 컴포넌트가 현재 성장 단계를 추적한다
- 단계 변경 시: 기존 프리팹을 Destroy하고 새 단계 프리팹을 Instantiate
- 또는 단일 프리팹 내 Stage_0~3 자식 오브젝트의 `SetActive(true/false)` 토글

**통합 프리팹 방식 (대안)**:

```
PFB_Crop_<Name> (GameObject, root)
│   - CropVisual.cs: currentStage에 따라 자식 활성화 전환
│
├── Stage_0_Seed (GameObject, 기본 Active)
│   └── Model + MeshRenderer
│
├── Stage_1_Sprout (GameObject, 기본 Inactive)
│   └── Model + MeshRenderer
│
├── Stage_2_Growing (GameObject, 기본 Inactive)
│   └── Model + MeshRenderer
│
└── Stage_3_Harvestable (GameObject, 기본 Inactive)
    └── Model + MeshRenderer
```

**채택 방식**: 통합 프리팹(단일 루트 + SetActive 전환). 이유:
- Instantiate/Destroy 반복보다 SetActive가 GC 부담 적음
- 프리팹 참조를 SO에 1개만 넣으면 됨 (배열 불필요 가능)
- 단, `growthStagePrefabs` 배열은 호환성을 위해 유지 (4개 또는 통합 1개)

[OPEN] 개별 프리팹 vs 통합 프리팹 최종 결정. 현재는 통합 프리팹을 우선 구현하되, CropData의 growthStagePrefabs 배열도 병행 유지.

---

## 2. Giant Crop 프리팹 구조

```
PFB_Crop_<Name>_Giant (GameObject, root)
│   - Transform: position (0,0,0), scale (3,3,3) -- 9타일(3x3) 차지
│   - Tag: "Crop"
│   - Layer: "Interactable"
│   - CropVisual.cs: isGiant = true
│
├── Model_Giant (GameObject)
│   - MeshFilter: 확대된 작물 메시 (placeholder)
│   - MeshRenderer: M_Crop_<Name>.mat
│
└── Collider_Giant (GameObject)
    - BoxCollider: 크기 (3, 3, 3) -- 3x3 타일 영역
    - isTrigger: true
```

**그리드 점유 처리**:
- Giant Crop은 기준 타일(좌하단) + 인접 8타일 = 총 9타일을 점유 (→ see docs/systems/crop-growth.md 섹션 5.1)
- 생성 시: FarmGrid에 9타일 모두 `TileState.OccupiedByGiant` 마킹
- 수확 시: 9타일 모두 해제, 수확량은 일반 작물의 4배 보너스 (→ see docs/content/crops.md 섹션 4.3)
- [RISK] Giant Crop 배치 시 3x3 영역 내에 이미 작물이 있는 타일이 존재하면 충돌. GrowthSystem에서 Giant 변이 판정 전 인접 8타일 빈 공간 검사 필수.

---

# Part IV -- 검증 태스크

---

## Task V-1: CropData SO 로드 검증

```
도구: execute_in_play_mode / console_log
설명: Play Mode에서 DataRegistry.GetCropData("crop_potato") 호출 결과를 로그로 출력.
검증 항목:
  - 반환값이 null이 아닌가
  - cropId, cropName, seedPrice, sellPrice, growthDays 가 올바른가
    (→ see docs/design.md 섹션 4.2 for 기대값)
의존성: Phase A~E 전체 완료
```

## Task V-2: 전체 작물 SO 일괄 로드 검증

```
도구: execute_in_play_mode / console_log
설명: DataRegistry에서 전체 CropData를 로드하여 카운트 및 요약 출력.
검증 항목:
  - 로드된 CropData 수 == 8 + 겨울 작물 수 (→ see docs/content/crops.md)
  - 모든 SO의 dataId가 고유한가
  - 모든 SO의 cropId와 dataId가 일치하는가
  - allowedSeasons가 None이 아닌가 (최소 1개 계절 설정)
의존성: Task V-1
```

## Task V-3: 프리팹 참조 무결성 검증

```
도구: execute_in_play_mode / console_log
설명: 모든 CropData SO의 프리팹 참조를 검사.
검증 항목:
  - growthStagePrefabs 배열 길이 == growthStageCount (4)
  - 배열 내 null 참조 없음
  - giantCropChance > 0 인 작물은 giantCropPrefab != null
  - requiresGreenhouse == true 인 작물은 allowedSeasons에 Winter 포함
의존성: Task C-4 완료
```

## Task V-4: IInventoryItem 인터페이스 검증

```
도구: execute_in_play_mode / console_log
설명: CropData를 IInventoryItem으로 캐스팅하여 프로퍼티 접근 검증.
검증 항목:
  - (CropData as IInventoryItem).ItemId == dataId
  - (CropData as IInventoryItem).ItemName == displayName
  - (CropData as IInventoryItem).ItemType == ItemType.Crop
  - (CropData as IInventoryItem).MaxStackSize == 99
  - (CropData as IInventoryItem).Sellable == true
의존성: Task V-1
```

---

## 태스크 의존성 요약

```
Phase A (스크립트)
  A-1: CropData.cs ──┐
  A-2: CropCategory  │
  A-3: SeasonFlag     │
                      │
Phase B (SO 에셋)     │
  B-1: SO 생성 ◀──────┘
  B-2: 필드 설정 ◀── B-1
  B-3: 겨울 작물 ◀── A-1 + docs/content/crops.md 확정
                      │
Phase C (프리팹)      │
  C-1: 폴더 생성      │
  C-2: 프리팹 생성 ◀── C-1
  C-3: Giant 프리팹 ◀── C-1
  C-4: SO에 연결 ◀── B-1 + C-2 + C-3
                      │
Phase D (머티리얼)    │
  D-1: 머티리얼 생성  │
  D-2: SO에 연결 ◀── B-1 + D-1
                      │
Phase E (레지스트리)  │
  E-1: 로드 로직 확인 ◀── A-1
  E-2: 경로 확인 ◀── B-1 + E-1
                      │
Phase V (검증)        │
  V-1~V-4 ◀── Phase A~E 전체
```

---

## Open Questions

- 겨울 작물 3종 확정 (겨울무, 표고버섯, 시금치 — `-> see docs/content/crops.md` 섹션 3.9~3.11). Phase B-3, C-1 태스크는 3종 기준으로 설계.
- [OPEN] 통합 프리팹(단일 루트 + SetActive) vs 개별 프리팹(단계별 Instantiate) 최종 결정. Part III에서 통합 방식을 우선 채택했으나, 메모리 프로파일링 후 재검토 가능
- [OPEN] DataRegistry의 CropData 로드 방식 -- Resources.LoadAll vs Addressables. 현재 Resources 방식으로 설계 (`-> see docs/systems/project-structure.md` Open Questions)

## Risks

- [RISK] Giant Crop 프리팹이 9타일(3x3)을 차지할 때 FarmGrid의 타일 점유 시스템과 충돌 가능. GrowthSystem에서 Giant 변이 판정 시 인접 8타일이 비어있는지 사전 검사하는 로직 필수
- [RISK] 겨울 작물의 requiresGreenhouse 플래그가 Greenhouse 건물 시스템과 정합성을 유지해야 함. Greenhouse.cs에서 내부 타일의 SeasonFlag를 오버라이드하는 메커니즘이 필요 (`-> see docs/systems/farming-architecture.md`)
- [RISK] MCP로 SO 에셋을 일괄 생성할 때 필드 설정 순서 오류 발생 가능. 특히 enum(SeasonFlag, CropCategory) 값이 int로 전달되는 경우 매핑 실수 위험. Phase V 검증 태스크로 보완
- [RISK] 통합 프리팹 방식 채택 시, 모든 성장 단계의 메시가 메모리에 상주. 작물 수가 많아지면(100+) 메모리 부담. 현재 스코프(11종 내외)에서는 문제 없음

---

## Cross-references

- `docs/design.md` 섹션 4.2 -- 작물 기본 데이터 canonical (이름, 성장일수, 판매가, 해금 레벨)
- `docs/content/crops.md` -- 작물 콘텐츠 상세 (CON-001 designer), 겨울 작물 정의
- `docs/pipeline/data-pipeline.md` -- CropData SO 스키마 canonical, GameDataSO 베이스, 확장 필드
- `docs/systems/farming-architecture.md` 섹션 4.1 -- CropData 원본 클래스 정의, SeasonFlag enum
- `docs/systems/project-structure.md` -- 폴더 구조, 네임스페이스, 에셋 네이밍 규칙 canonical
- `docs/systems/inventory-architecture.md` 섹션 3.1 -- IInventoryItem 인터페이스 canonical
- `docs/systems/crop-growth.md` -- 성장 단계, 계절 규칙, Giant Crop 메카닉
- `docs/mcp/farming-tasks.md` -- FarmGrid/FarmTile MCP 태스크 (선행 의존)
