# 시설 시스템 기술 아키텍처 (Facilities Architecture)

> 시설(Building) 시스템의 클래스 구조, 데이터 흐름, 서브시스템 설계, 저장/로드, MCP 구현 계획  
> 작성: Claude Code (Opus) | 2026-04-06  
> Phase 1 | 문서 ID: ARC-007

---

## 1. 개요

### Context

시설 시스템은 SeedMind의 농장 확장을 담당하는 핵심 시스템이다. 플레이어는 골드를 투자하여 물탱크, 온실, 창고, 가공소를 건설하고, 각 시설의 고유 효과를 통해 경작 효율을 높이거나 수익을 극대화한다. 이 문서는 `docs/architecture.md`의 Building 섹션(Scripts/Building/)을 기술적으로 상세화하며, `docs/pipeline/data-pipeline.md` 섹션 2.4~2.5의 BuildingData/ProcessingRecipeData SO를 기반으로 런타임 시스템을 설계한다.

### 설계 목표

- 시설 유형별 로직을 **서브시스템**으로 분리하여 확장성 확보
- BuildingManager가 시설 생명주기(건설/가동/업그레이드/철거)를 통합 관리
- 모든 시설 효과는 ScriptableObject 데이터 드리븐 -- 코드 변경 없이 밸런스 조정 가능
- 경작(Farm), 인벤토리(Player), 경제(Economy), 시간(Core) 시스템과의 명확한 인터페이스

### 의존성

```
SeedMind.Building asmdef 참조:
├── SeedMind.Core     (TimeManager, SaveManager, EventBus, GameDataSO)
├── SeedMind.Farm     (FarmGrid, FarmTile, CropInstance, GrowthSystem)
└── SeedMind.Economy  (EconomyManager -- 건설 비용 지불)

외부에서 SeedMind.Building을 참조하는 모듈:
├── SeedMind.Player   (InventoryManager -- 창고 연동은 이벤트로 통신)
└── SeedMind.UI       (BuildingUI -- 건설/가공 UI)
```

(-> see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

---

## 2. BuildingData ScriptableObject

### 2.1 JSON 스키마

BuildingData의 필드 정의는 `docs/pipeline/data-pipeline.md` 섹션 2.4가 canonical이다. 아래는 시설 유형별 JSON 예시로, 필드 구조를 시각적으로 보여준다. 수치는 canonical 문서를 참조한다.

**물탱크 (SO_Bldg_WaterTank)**:

```json
{
  "dataId": "building_water_tank",
  "displayName": "물탱크",
  "description": "인접 경작지에 매일 아침 자동으로 물을 준다.",
  "icon": null,
  "buildCost": 0,
  "requiredLevel": 0,
  "buildTimeDays": 0,
  "tileSize": { "x": 2, "y": 2 },
  "placementRules": "FarmOnly",
  "prefab": null,
  "constructionPrefab": null,
  "effectType": "AutoWater",
  "effectRadius": 0,
  "effectValue": 0.0,
  "maxUpgradeLevel": 0,
  "upgradeCosts": []
}
```

> buildCost, requiredLevel, buildTimeDays, effectRadius, effectValue, maxUpgradeLevel, upgradeCosts:  
> (-> see `docs/pipeline/data-pipeline.md` 섹션 2.4 및 `docs/design.md` 섹션 4.6 for canonical 수치)

**온실 (SO_Bldg_Greenhouse)**:

```json
{
  "dataId": "building_greenhouse",
  "displayName": "온실",
  "description": "계절에 관계없이 모든 작물을 재배할 수 있다.",
  "icon": null,
  "buildCost": 0,
  "requiredLevel": 0,
  "buildTimeDays": 0,
  "tileSize": { "x": 6, "y": 6 },
  "placementRules": "FarmOnly",
  "prefab": null,
  "constructionPrefab": null,
  "effectType": "SeasonBypass",
  "effectRadius": 0,
  "effectValue": 0.0,
  "maxUpgradeLevel": 0,
  "upgradeCosts": []
}
```

> (-> see `docs/pipeline/data-pipeline.md` 섹션 2.4 for canonical 수치)

**창고 (SO_Bldg_Storage)**:

```json
{
  "dataId": "building_storage",
  "displayName": "창고",
  "description": "작물을 저장하여 가격 변동에 대응할 수 있다.",
  "icon": null,
  "buildCost": 0,
  "requiredLevel": 0,
  "buildTimeDays": 0,
  "tileSize": { "x": 3, "y": 2 },
  "placementRules": "FarmOnly",
  "prefab": null,
  "constructionPrefab": null,
  "effectType": "Storage",
  "effectRadius": 0,
  "effectValue": 0.0,
  "maxUpgradeLevel": 0,
  "upgradeCosts": []
}
```

> effectValue (최대 저장 슬롯 수): (-> see `docs/pipeline/data-pipeline.md` 섹션 2.4 for canonical 수치)

**가공소 (SO_Bldg_Processor)**:

```json
{
  "dataId": "building_processing",
  "displayName": "가공소",
  "description": "작물을 가공하여 더 높은 가격에 판매할 수 있다.",
  "icon": null,
  "buildCost": 0,
  "requiredLevel": 0,
  "buildTimeDays": 0,
  "tileSize": { "x": 4, "y": 3 },
  "placementRules": "FarmOnly",
  "prefab": null,
  "constructionPrefab": null,
  "effectType": "Processing",
  "effectRadius": 0,
  "effectValue": 0.0,
  "maxUpgradeLevel": 0,
  "upgradeCosts": []
}
```

> effectValue (초기 가공 슬롯 수): (-> see `docs/pipeline/data-pipeline.md` 섹션 2.4 for canonical 수치)

### 2.2 C# 클래스 (PATTERN-005: JSON과 필드 일치 필수)

BuildingData SO의 C# 클래스 정의. JSON 스키마(섹션 2.1)와 모든 필드명/필드 수가 완전히 일치한다.

```csharp
// illustrative
namespace SeedMind.Building.Data
{
    /// <summary>
    /// 시설(건물) 데이터를 정의하는 ScriptableObject.
    /// GameDataSO를 상속하여 dataId, displayName, icon은 부모에서 제공.
    /// (-> see docs/pipeline/data-pipeline.md Part II 섹션 1 for GameDataSO)
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Bldg_New", menuName = "SeedMind/Building/BuildingData")]
    public class BuildingData : GameDataSO
    {
        // --- GameDataSO 상속 필드 ---
        // public string dataId;        (부모에서 제공, JSON의 "dataId")
        // public string displayName;   (부모에서 제공, JSON의 "displayName")
        // public Sprite icon;          (부모에서 제공, JSON의 "icon")

        [Header("설명")]
        public string description;                  // UI 표시용 설명 텍스트

        [Header("건설")]
        public int buildCost;                       // 건설 비용 (-> see docs/design.md 섹션 4.6)
        public int requiredLevel;                   // 해금 레벨 (-> see docs/design.md 섹션 4.6)
        public int buildTimeDays;                   // 건설 소요 일수 (-> see docs/pipeline/data-pipeline.md 섹션 2.4)

        [Header("배치")]
        public Vector2Int tileSize;                 // 점유 타일 크기 (가로x세로)
        public PlacementRule placementRules;        // 배치 가능 위치 제한

        [Header("비주얼")]
        public GameObject prefab;                   // 시설 3D 모델 프리팹
        public GameObject constructionPrefab;       // 건설 중 모델 프리팹

        [Header("효과")]
        public BuildingEffectType effectType;       // 시설 효과 종류
        public int effectRadius;                    // 효과 적용 반경 (-> see docs/pipeline/data-pipeline.md 섹션 2.4)
        public float effectValue;                   // 효과 수치 (-> see docs/pipeline/data-pipeline.md 섹션 2.4)

        [Header("업그레이드")]
        public int maxUpgradeLevel;                 // 업그레이드 가능 횟수 (-> see docs/pipeline/data-pipeline.md 섹션 2.4)
        public int[] upgradeCosts;                  // 단계별 업그레이드 비용 (-> see docs/pipeline/data-pipeline.md 섹션 2.4)
    }

    /// <summary>
    /// 시설 효과 유형.
    /// (-> see docs/pipeline/data-pipeline.md 섹션 2.4 for canonical enum 정의)
    /// </summary>
    public enum BuildingEffectType
    {
        None,           // 효과 없음
        AutoWater,      // 인접 타일 자동 물주기 (물탱크)
        SeasonBypass,   // 계절 무관 재배 (온실)
        Storage,        // 작물 저장 (창고) -- effectValue = 최대 슬롯 수
        Processing      // 작물 가공 (가공소) -- effectValue = 초기 슬롯 수
    }

    /// <summary>
    /// 시설 배치 규칙.
    /// (-> see docs/pipeline/data-pipeline.md 섹션 2.4 for canonical enum 정의)
    /// </summary>
    public enum PlacementRule
    {
        FarmOnly,       // 농장 그리드 내부에만 배치 가능
        FarmEdge,       // 농장 그리드 가장자리에만 배치 가능
        Anywhere        // 농장 영역 어디든 배치 가능
    }
}
```

**PATTERN-005 검증 (JSON 필드 <-> C# 필드 매핑)**:

| JSON 필드 | C# 필드 | 출처 |
|-----------|---------|------|
| dataId | GameDataSO.dataId | 부모 클래스 |
| displayName | GameDataSO.displayName | 부모 클래스 |
| icon | GameDataSO.icon | 부모 클래스 |
| description | description | BuildingData |
| buildCost | buildCost | BuildingData |
| requiredLevel | requiredLevel | BuildingData |
| buildTimeDays | buildTimeDays | BuildingData |
| tileSize | tileSize | BuildingData |
| placementRules | placementRules | BuildingData |
| prefab | prefab | BuildingData |
| constructionPrefab | constructionPrefab | BuildingData |
| effectType | effectType | BuildingData |
| effectRadius | effectRadius | BuildingData |
| effectValue | effectValue | BuildingData |
| maxUpgradeLevel | maxUpgradeLevel | BuildingData |
| upgradeCosts | upgradeCosts | BuildingData |

총 16 필드: JSON 16개 = C# 16개 (부모 3 + 자체 13). 일치 확인 완료.

---

## 3. BuildingManager

### 3.1 클래스 다이어그램

```
┌─────────────────────────────────────────────────────────────────────┐
│                        SeedMind.Building                            │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│           BuildingManager (MonoBehaviour, Singleton)           │
│──────────────────────────────────────────────────────────────│
│  [상태]                                                       │
│  - _buildings: List<BuildingInstance>   // 건설된 시설 목록     │
│  - _buildingParent: Transform          // 시설 GO 부모 노드    │
│                                                              │
│  [설정 참조]                                                   │
│  - _allBuildingData: BuildingData[]    // 등록된 모든 시설 SO   │
│                                                              │
│  [서브시스템 참조]                                              │
│  - _waterTankSystem: WaterTankSystem                         │
│  - _greenhouseSystem: GreenhouseSystem                       │
│  - _storageSystem: StorageSystem                             │
│  - _processingSystem: ProcessingSystem                       │
│                                                              │
│  [읽기 전용 프로퍼티]                                           │
│  + Buildings: IReadOnlyList<BuildingInstance>                 │
│  + BuildingCount: int                                        │
│                                                              │
│  [이벤트]                                                     │
│  + OnBuildingPlaced: Action<BuildingInstance>                 │
│  + OnBuildingCompleted: Action<BuildingInstance>              │
│  + OnBuildingUpgraded: Action<BuildingInstance, int>          │
│  + OnBuildingRemoved: Action<string>  // buildingId           │
│                                                              │
│  [메서드]                                                     │
│  + CanPlace(BuildingData data, int gridX, int gridY): bool   │
│  + PlaceBuilding(BuildingData data, int gridX, int gridY):   │
│                                            BuildingInstance   │
│  + RemoveBuilding(BuildingInstance inst): bool                │
│  + UpgradeBuilding(BuildingInstance inst): bool               │
│  + GetBuildingAt(int gridX, int gridY): BuildingInstance      │
│  + GetBuildingsByType(BuildingEffectType type):               │
│                                   List<BuildingInstance>      │
│  - OnDayChangedHandler(int newDay): void                     │
│  - AdvanceConstruction(): void                               │
│  + GetSaveData(): BuildingSaveData[]                         │
│  + LoadSaveData(BuildingSaveData[] data): void               │
│                                                              │
│  [구독]                                                       │
│  + OnEnable():                                               │
│      TimeManager.RegisterOnDayChanged(priority: 50)          │
│  + OnDisable(): 구독 해제                                      │
└──────────────────────────────────────────────────────────────┘
         │ owns N                    │ delegates
         ▼                          ▼
┌────────────────────┐    ┌───────────────────┐
│ BuildingInstance   │    │ WaterTankSystem   │
│ (Plain C# class)   │    │ GreenhouseSystem  │
│                    │    │ StorageSystem     │
│ data: BuildingData │    │ ProcessingSystem  │
│ gridX, gridY: int  │    └───────────────────┘
│ isOperational: bool│
│ upgradeLevel: int  │
│ buildProgress: float│
│ gameObject: GO ref │
└────────────────────┘
```

### 3.2 BuildingInstance 런타임 클래스

```csharp
// illustrative
namespace SeedMind.Building
{
    /// <summary>
    /// 건설된 시설의 런타임 인스턴스. MonoBehaviour가 아닌 순수 C# 클래스.
    /// BuildingSaveData와 1:1 대응한다.
    /// </summary>
    public class BuildingInstance
    {
        public BuildingData Data { get; private set; }        // SO 참조
        public int GridX { get; private set; }                // 배치 위치 X (좌하단 기준)
        public int GridY { get; private set; }                // 배치 위치 Y
        public bool IsOperational { get; set; }               // 건설 완료 여부
        public int UpgradeLevel { get; set; }                 // 현재 업그레이드 단계
        public float BuildProgress { get; set; }              // 건설 진행률 (0.0~1.0)
        public GameObject SceneObject { get; set; }           // 씬 내 GameObject 참조

        public BuildingInstance(BuildingData data, int gridX, int gridY)
        {
            Data = data;
            GridX = gridX;
            GridY = gridY;
            IsOperational = false;
            UpgradeLevel = 0;
            BuildProgress = 0f; // → see docs/pipeline/data-pipeline.md 섹션 2.4
        }

        /// <summary>
        /// 이 시설이 점유하는 모든 타일 좌표를 반환.
        /// </summary>
        public IEnumerable<Vector2Int> GetOccupiedTiles()
        {
            for (int x = GridX; x < GridX + Data.tileSize.x; x++)
            {
                for (int y = GridY; y < GridY + Data.tileSize.y; y++)
                {
                    yield return new Vector2Int(x, y);
                }
            }
        }
    }
}
```

### 3.3 건설 흐름

```
플레이어 건설 요청
    │
    ▼
[1] CanPlace() 검증
    ├── 해금 레벨 확인 (ProgressionManager.CurrentLevel >= data.requiredLevel)
    ├── 골드 확인 (EconomyManager.CanAfford(data.buildCost))
    ├── 타일 검증 (FarmGrid에서 tileSize 범위 내 모든 타일이 Empty인지)
    └── 배치 규칙 검증 (PlacementRule 충족 여부)
    │
    ▼ 검증 통과
[2] PlaceBuilding()
    ├── EconomyManager.SpendGold(data.buildCost, "building_construction")
    ├── BuildingInstance 생성 (BuildProgress = 0, IsOperational = false)
    ├── FarmGrid의 해당 타일들을 TileState.Building으로 변경
    ├── constructionPrefab 인스턴스화 → 씬 배치
    ├── _buildings에 추가
    └── OnBuildingPlaced 이벤트 발행
    │
    ▼ 매일 아침 (OnDayChanged)
[3] AdvanceConstruction()
    ├── 건설 중인 시설의 BuildProgress += 1.0 / data.buildTimeDays
    ├── BuildProgress >= 1.0이면:
    │   ├── IsOperational = true
    │   ├── constructionPrefab 제거 → prefab으로 교체
    │   ├── 해당 서브시스템에 등록 (RegisterBuilding)
    │   └── OnBuildingCompleted 이벤트 발행
    └── 건설 중이면 진행률 UI 업데이트
```

### 3.4 업그레이드 흐름

```
UpgradeBuilding(inst)
    │
    ▼
[1] 검증
    ├── inst.UpgradeLevel < inst.Data.maxUpgradeLevel
    └── EconomyManager.CanAfford(inst.Data.upgradeCosts[inst.UpgradeLevel])
    │
    ▼ 통과
[2] 실행
    ├── EconomyManager.SpendGold(upgradeCosts[upgradeLevel])
    ├── inst.UpgradeLevel++
    ├── 서브시스템에 업그레이드 반영 (OnBuildingUpgraded 통해)
    └── OnBuildingUpgraded 이벤트 발행
```

### 3.5 철거 흐름

```
RemoveBuilding(inst)
    │
    ▼
[1] 서브시스템에서 해제 (UnregisterBuilding)
[2] inst.SceneObject 파괴
[3] FarmGrid의 점유 타일을 TileState.Empty로 복원
[4] _buildings에서 제거
[5] EconomyManager.RefundGold(refundAmount) — 건설 비용 + 업그레이드 비용 합산의 50% 환급
[6] OnBuildingRemoved 이벤트 발행
```

철거 환급 규칙: (-> see `docs/content/facilities.md` 섹션 2.5 for canonical. 환급률 50%, 즉시 처리)

---

## 4. 물탱크 시스템 (WaterTankSystem)

### 4.1 클래스 설계

```csharp
// illustrative
namespace SeedMind.Building
{
    /// <summary>
    /// 물탱크의 자동 물주기 로직을 처리하는 서브시스템.
    /// BuildingManager가 소유하며, 물탱크 BuildingInstance를 관리한다.
    /// </summary>
    public class WaterTankSystem
    {
        private List<BuildingInstance> _waterTanks
            = new List<BuildingInstance>();
        private FarmGrid _farmGrid;  // 참조 주입

        /// <summary>
        /// 물탱크를 등록한다. 건설 완료(OnBuildingCompleted) 시 호출.
        /// </summary>
        public void RegisterTank(BuildingInstance tank) { /* ... */ }

        /// <summary>
        /// 물탱크를 해제한다. 철거 시 호출.
        /// </summary>
        public void UnregisterTank(BuildingInstance tank) { /* ... */ }

        /// <summary>
        /// 매일 아침 호출. 모든 물탱크의 범위 내 경작 타일에 자동 물주기.
        /// BuildingManager.OnDayChanged에서 건설 진행 후 호출.
        /// </summary>
        public void ProcessDailyWatering()
        {
            // foreach 물탱크:
            //   effectRadius = tank.Data.effectRadius (-> see docs/pipeline/data-pipeline.md 섹션 2.4)
            //   업그레이드에 따른 반경 보정: actualRadius = effectRadius + tank.UpgradeLevel
            //   중심 좌표 = (tank.GridX + tileSize.x/2, tank.GridY + tileSize.y/2)
            //   범위 내 FarmTile 순회 → Planted/Dry 상태인 타일에 Watered 적용
        }

        /// <summary>
        /// 특정 타일이 물탱크 범위 내에 있는지 확인.
        /// </summary>
        public bool IsTileCoveredByTank(int tileX, int tileY) { /* ... */ return false; }

        /// <summary>
        /// 물탱크 범위가 겹치는 타일 좌표 목록 반환 (디버그/UI용).
        /// </summary>
        public List<Vector2Int> GetCoveredTiles(BuildingInstance tank) { /* ... */ return null; }
    }
}
```

### 4.2 범위 계산 로직

```
물탱크 중심점 계산:
  centerX = tank.GridX + tank.Data.tileSize.x / 2
  centerY = tank.GridY + tank.Data.tileSize.y / 2

실제 적용 반경:
  actualRadius = tank.Data.effectRadius + tank.UpgradeLevel
  // → see docs/pipeline/data-pipeline.md 섹션 2.4 for effectRadius canonical 값

범위 내 타일 (맨해튼 거리 기반):
  for each tile in FarmGrid:
    if |tile.x - centerX| + |tile.y - centerY| <= actualRadius:
      if tile.state == Planted or tile.state == Dry:
        tile.SetWatered(true)
```

[OPEN] 범위 계산 방식을 맨해튼 거리(다이아몬드)로 할지 체비셰프 거리(정사각형)로 할지. 맨해튼이 시각적으로 더 직관적이나, 정사각형이 그리드 기반에 더 맞을 수 있다.

### 4.3 TimeSystem 연동

```
TimeManager.OnDayChanged (priority 순서)
    │
    ├── priority 10: WeatherSystem -- 날씨 갱신
    ├── priority 20: GrowthSystem -- 작물 성장 처리
    ├── priority 30: EconomyManager -- 가격 변동
    ├── priority 40: ShopSystem -- 재고 갱신
    ├── priority 50: BuildingManager -- 건설 진행 + 자동 물주기
    │   └── BuildingManager.OnDayChangedHandler()
    │       ├── AdvanceConstruction()  -- 건설 중인 시설 진행
    │       └── _waterTankSystem.ProcessDailyWatering()  -- 자동 물주기
    └── priority 60: (향후 확장)
```

**중요**: GrowthSystem(priority 20)이 먼저 실행되므로, 물탱크의 자동 물주기(priority 50)는 **당일이 아닌 다음 날 성장에 영향**을 준다. 즉 물탱크가 건설된 첫 날에는 자동 물주기의 성장 효과가 없으며, 다음 날부터 효과가 적용된다. 이는 수동 물주기와 동일한 타이밍이다.

[RISK] priority 순서가 변경되면 물탱크 효과 타이밍이 달라진다. GrowthSystem보다 물탱크가 먼저 실행되면 건설 당일부터 효과가 적용되어 밸런스가 깨질 수 있다.

---

## 5. 온실 시스템 (GreenhouseSystem)

### 5.1 클래스 설계

```csharp
// illustrative
namespace SeedMind.Building
{
    /// <summary>
    /// 온실의 내부 경작 타일 관리 및 계절 보정 Override를 처리하는 서브시스템.
    /// 온실 내부 타일은 FarmGrid의 일부이되, 계절 제약이 해제된다.
    /// </summary>
    public class GreenhouseSystem
    {
        private List<BuildingInstance> _greenhouses
            = new List<BuildingInstance>();
        private FarmGrid _farmGrid;              // 참조 주입
        private HashSet<Vector2Int> _greenhouseTiles
            = new HashSet<Vector2Int>();          // 온실 내부 경작 가능 타일 캐시

        /// <summary>
        /// 온실을 등록한다. 건설 완료 시 호출.
        /// 내부 경작 가능 타일을 계산하여 캐시에 추가.
        /// </summary>
        public void RegisterGreenhouse(BuildingInstance greenhouse) { /* ... */ }

        /// <summary>
        /// 온실을 해제한다. 철거 시 내부 타일 캐시도 제거.
        /// </summary>
        public void UnregisterGreenhouse(BuildingInstance greenhouse) { /* ... */ }

        /// <summary>
        /// 특정 타일이 온실 내부인지 확인.
        /// GrowthSystem과 FarmTile에서 계절 제약 검사 시 사용.
        /// </summary>
        public bool IsInsideGreenhouse(int tileX, int tileY)
        {
            return _greenhouseTiles.Contains(new Vector2Int(tileX, tileY));
        }

        /// <summary>
        /// 온실 내부의 경작 가능 타일 좌표 목록 반환.
        /// 테두리(1타일 벽)를 제외한 내부 영역.
        /// </summary>
        public List<Vector2Int> GetInteriorTiles(BuildingInstance greenhouse)
        {
            // tileSize (6,6) → 내부 Lv.1 = 4x4 = 16타일 (Lv.2: 4x6, Lv.3: 6x6)
            // 테두리 1타일 벽을 제외: x=[gridX+1, gridX+tileSize.x-2], y 동일
            // 업그레이드 레벨에 따라 내부 경작 영역이 달라짐
            // (-> see docs/content/facilities.md 섹션 4.2 for 레벨별 내부 타일 수 canonical)
            return null;
        }
    }
}
```

### 5.2 계절 보정 Override 로직

온실 내부 타일에서는 계절 제약이 해제된다. 이를 위해 GrowthSystem과의 인터페이스가 필요하다.

```
GrowthSystem.ProcessGrowth(FarmTile tile)
    │
    ├── [기존] 계절 확인: CropData.seasonMask에 현재 계절이 포함되는지
    │
    ├── [온실 추가] GreenhouseSystem.IsInsideGreenhouse(tile.x, tile.y)
    │   ├── true → 계절 검사 건너뜀, 모든 작물 성장 가능
    │   └── false → 기존 계절 검사 수행
    │
    └── 성장 처리 계속...
```

### 5.3 CropGrowthSystem과의 인터페이스

GrowthSystem이 GreenhouseSystem을 직접 참조하지 않고, **인터페이스를 통해 계절 제약 해제를 질의**한다.

```csharp
// illustrative
namespace SeedMind.Farm
{
    /// <summary>
    /// 타일의 계절 제약 해제 여부를 질의하는 인터페이스.
    /// GrowthSystem은 이 인터페이스에만 의존하며,
    /// GreenhouseSystem이 구현을 제공한다.
    /// </summary>
    public interface ISeasonOverrideProvider
    {
        bool IsSeasonOverridden(int tileX, int tileY);
    }
}
```

**의존성 방향**: `SeedMind.Farm`에 인터페이스 정의 → `SeedMind.Building`에서 구현. 이렇게 하면 Farm -> Building 의존성이 생기지 않고, Building -> Farm 방향만 유지된다 (-> see `docs/systems/project-structure.md` 섹션 3.2 의존성 매트릭스).

[RISK] ISeasonOverrideProvider를 GrowthSystem에 주입하는 타이밍. GameManager 초기화 시 BuildingManager가 GrowthSystem에 자신의 GreenhouseSystem을 ISeasonOverrideProvider로 등록해야 한다. 초기화 순서가 잘못되면 NullReferenceException 발생.

---

## 6. 창고 시스템 (StorageSystem)

### 6.1 클래스 설계

```csharp
// illustrative
namespace SeedMind.Building
{
    /// <summary>
    /// 창고의 슬롯 관리 및 아이템 입출 로직을 처리하는 서브시스템.
    /// 인벤토리 시스템(InventoryManager)과 이벤트 기반으로 통신한다.
    /// </summary>
    public class StorageSystem
    {
        private Dictionary<BuildingInstance, StorageSlotContainer> _storages
            = new Dictionary<BuildingInstance, StorageSlotContainer>();

        /// <summary>
        /// 창고를 등록한다. 건설 완료 시 호출.
        /// effectValue에서 최대 슬롯 수를 읽어 초기화.
        /// </summary>
        public void RegisterStorage(BuildingInstance storage)
        {
            int maxSlots = (int)storage.Data.effectValue; // → see docs/pipeline/data-pipeline.md 섹션 2.4
            _storages[storage] = new StorageSlotContainer(maxSlots);
        }

        /// <summary>
        /// 창고를 해제한다. 철거 시 내부 아이템은 드롭 또는 인벤토리로 반환.
        /// </summary>
        public void UnregisterStorage(BuildingInstance storage) { /* ... */ }

        /// <summary>
        /// 특정 창고에 아이템을 저장한다.
        /// InventoryManager에서 호출. 이벤트 통신 대신 직접 호출 가능
        /// (Building -> Economy 의존은 허용, -> see project-structure.md 섹션 3.2).
        /// </summary>
        public bool StoreItem(BuildingInstance storage, string itemId,
                              int quantity, string quality)
        {
            // StorageSlotContainer에 빈 슬롯 또는 스택 가능 슬롯 확인
            // 성공 시 true, 슬롯 부족 시 false
            return false;
        }

        /// <summary>
        /// 특정 창고에서 아이템을 꺼낸다.
        /// </summary>
        public bool RetrieveItem(BuildingInstance storage, string itemId,
                                 int quantity, out string quality)
        {
            quality = null;
            return false;
        }

        /// <summary>
        /// 특정 창고의 모든 슬롯 정보를 반환 (UI용).
        /// </summary>
        public IReadOnlyList<StorageSlot> GetSlots(BuildingInstance storage)
        {
            return _storages.TryGetValue(storage, out var container)
                ? container.Slots
                : null;
        }

        /// <summary>
        /// 특정 창고의 빈 슬롯 수를 반환.
        /// </summary>
        public int GetEmptySlotCount(BuildingInstance storage)
        {
            return _storages.TryGetValue(storage, out var container)
                ? container.EmptySlotCount
                : 0;
        }
    }
}
```

### 6.2 StorageSlotContainer / StorageSlot

```csharp
// illustrative
namespace SeedMind.Building
{
    /// <summary>
    /// 창고의 슬롯 배열을 관리하는 컨테이너.
    /// InventoryManager의 ItemSlot과 유사하지만 도구는 저장 불가.
    /// </summary>
    public class StorageSlotContainer
    {
        private StorageSlot[] _slots;

        public IReadOnlyList<StorageSlot> Slots => _slots;
        public int MaxSlots => _slots.Length;
        public int EmptySlotCount { get; private set; }

        public StorageSlotContainer(int maxSlots)
        {
            _slots = new StorageSlot[maxSlots];
            for (int i = 0; i < maxSlots; i++)
                _slots[i] = new StorageSlot();
            EmptySlotCount = maxSlots;
        }

        // AddItem, RemoveItem, FindSlot 등 내부 메서드
    }

    /// <summary>
    /// 창고의 개별 슬롯. 아이템 ID, 수량, 품질을 저장.
    /// </summary>
    public class StorageSlot
    {
        public string ItemId { get; set; }        // null = 빈 슬롯
        public int Quantity { get; set; }
        public string Quality { get; set; }       // CropQuality enum 문자열
        public bool IsEmpty => string.IsNullOrEmpty(ItemId);
    }
}
```

### 6.3 InventoryManager와의 아이템 이동 인터페이스

```
플레이어가 창고에 아이템 저장:
    InventoryManager → BuildingManager.GetBuildingAt(x, y) → StorageSystem.StoreItem()
    성공 시 → InventoryManager.RemoveItem() → UI 갱신

플레이어가 창고에서 아이템 꺼냄:
    StorageSystem.RetrieveItem() → InventoryManager.AddItem()
    성공 시 → UI 갱신

통신 방식:
    - InventoryManager(SeedMind.Player)는 BuildingManager(SeedMind.Building)를 직접 참조 불가
      (-> see project-structure.md 섹션 3.2: Player는 Building 참조 금지)
    - 대신 BuildingEvents를 통해 통신하거나,
      UI 계층에서 양쪽을 중재한다 (StorageUI가 양쪽 매니저에 접근)
```

[OPEN] Player -> Building 의존이 금지되므로, 창고 아이템 이동의 중재자 역할을 UI가 할지 별도 중재 서비스를 만들지. UI 중재 방식이 가장 단순하며 project-structure.md의 의존성 규칙을 위반하지 않는다.

### 6.4 SaveData 구조

기존 `BuildingSaveData.storageSlots[]`를 활용한다 (-> see `docs/pipeline/data-pipeline.md` Part I 섹션 3.3 및 Part II 섹션 2.6).

```
BuildingSaveData (storage 시설인 경우)
├── buildingId: "storage"
├── gridX, gridY: 배치 위치
├── isOperational: true
├── upgradeLevel: 0
├── buildProgress: 1.0
└── storageSlots: ItemSlotSaveData[]
    ├── { itemId: "crop_potato", quantity: 20, quality: "Normal" }
    └── { itemId: "seed_carrot", quantity: 5, quality: null }
```

업그레이드로 슬롯 수가 증가한 경우, 저장 시 실제 사용 중인 슬롯만 기록하고, 로드 시 `effectValue + upgradeLevel * slotIncrement`로 최대 슬롯을 재계산한다.

---

## 7. 가공소 시스템 (ProcessingSystem)

### 7.1 ProcessingRecipeData ScriptableObject

레시피 데이터의 필드 정의는 `docs/pipeline/data-pipeline.md` 섹션 2.5가 canonical이다.

**JSON 예시 (잼 레시피)**:

```json
{
  "dataId": "jam_potato",
  "displayName": "감자 잼",
  "description": "감자를 가공하여 만든 잼.",
  "icon": null,
  "processingType": "Jam",
  "inputCategory": "Vegetable",
  "priceMultiplier": 0.0,
  "priceBonus": 0,
  "processingTimeHours": 0.0,
  "outputItemId": "jam_potato"
}
```

> priceMultiplier, priceBonus, processingTimeHours:  
> (-> see `docs/pipeline/data-pipeline.md` 섹션 2.5 및 `docs/systems/economy-system.md` 섹션 2.5 for canonical 수치)

**C# 클래스 (PATTERN-005)**:

```csharp
// illustrative
namespace SeedMind.Building.Data
{
    /// <summary>
    /// 가공 레시피를 정의하는 ScriptableObject.
    /// GameDataSO를 상속하여 dataId, displayName, icon은 부모에서 제공.
    /// (-> see docs/pipeline/data-pipeline.md 섹션 2.5 for canonical 필드 정의)
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Recipe_New", menuName = "SeedMind/Building/ProcessingRecipeData")]
    public class ProcessingRecipeData : GameDataSO
    {
        // --- GameDataSO 상속 필드 ---
        // public string dataId;        (부모, JSON의 "dataId")
        // public string displayName;   (부모, JSON의 "displayName")
        // public Sprite icon;          (부모, JSON의 "icon")

        [Header("가공")]
        public ProcessingType processingType;         // 가공 유형 (Jam, Juice, Pickle)
        public CropCategory inputCategory;            // 입력 가능 작물 카테고리
        public float priceMultiplier;                 // 원재료 기본가 대비 배수 (-> see docs/systems/economy-system.md 섹션 2.5)
        public int priceBonus;                        // 고정 가격 보너스 (-> see docs/systems/economy-system.md 섹션 2.5)
        public float processingTimeHours;             // 가공 소요 시간 (게임 내 시간, -> see docs/pipeline/data-pipeline.md 섹션 2.5)
        public string outputItemId;                   // 출력 아이템 식별자
    }

    /// <summary>
    /// 가공 유형. (-> see docs/pipeline/data-pipeline.md 섹션 2.5 for canonical 정의)
    /// </summary>
    public enum ProcessingType
    {
        Jam,            // 잼 (-> see docs/pipeline/data-pipeline.md 섹션 2.5)
        Juice,          // 주스
        Pickle,         // 절임
        Mill,           // 제분 (제분소)
        Fermentation,   // 발효 (발효실)
        Bake            // 베이킹 (베이커리)
    }
}
```

**PATTERN-005 검증 (JSON 필드 <-> C# 필드 매핑)**:

| JSON 필드 | C# 필드 | 출처 |
|-----------|---------|------|
| dataId | GameDataSO.dataId | 부모 클래스 |
| displayName | GameDataSO.displayName | 부모 클래스 |
| icon | GameDataSO.icon | 부모 클래스 |
| processingType | processingType | ProcessingRecipeData |
| inputCategory | inputCategory | ProcessingRecipeData |
| priceMultiplier | priceMultiplier | ProcessingRecipeData |
| priceBonus | priceBonus | ProcessingRecipeData |
| processingTimeHours | processingTimeHours | ProcessingRecipeData |
| outputItemId | outputItemId | ProcessingRecipeData |

총 9 필드: JSON 9개 = C# 9개 (부모 3 + 자체 6). 일치 확인 완료.

> **참고**: data-pipeline.md 섹션 2.5의 canonical 필드 중 `description`이 JSON 예시에는 있으나 data-pipeline.md의 테이블에는 별도 행으로 존재한다. 본 문서에서는 `description`을 JSON에 포함하되, C# 클래스에서는 `GameDataSO`에 description 필드가 없으므로 별도 추가하지 않는다. description은 BuildingData에만 존재하는 필드이며, ProcessingRecipeData의 description은 displayName으로 대체한다.

[OPEN] ProcessingRecipeData에도 description 필드를 추가할지. 현재 canonical(data-pipeline.md 섹션 2.5)에 description이 테이블에 명시되어 있으므로, 추가가 필요할 수 있다. 추가 시 JSON과 C# 양쪽에 동시 반영 필요.

### 7.2 클래스 설계

```csharp
// illustrative
namespace SeedMind.Building
{
    /// <summary>
    /// 가공소의 레시피 처리, 큐 관리, 시간 경과 처리를 담당하는 서브시스템.
    /// </summary>
    public class ProcessingSystem
    {
        private Dictionary<BuildingInstance, ProcessingSlot[]> _processors
            = new Dictionary<BuildingInstance, ProcessingSlot[]>();

        /// <summary>
        /// 가공소를 등록한다. 건설 완료 시 호출.
        /// effectValue에서 초기 슬롯 수를 읽어 슬롯 배열 초기화.
        /// </summary>
        public void RegisterProcessor(BuildingInstance processor)
        {
            int slotCount = (int)processor.Data.effectValue; // → see docs/pipeline/data-pipeline.md 섹션 2.4
            _processors[processor] = new ProcessingSlot[slotCount];
            for (int i = 0; i < slotCount; i++)
                _processors[processor][i] = new ProcessingSlot();
        }

        /// <summary>
        /// 가공소를 해제한다. 진행 중 가공은 아이템 반환.
        /// </summary>
        public void UnregisterProcessor(BuildingInstance processor) { /* ... */ }

        /// <summary>
        /// 가공 작업을 시작한다.
        /// </summary>
        public bool StartProcessing(BuildingInstance processor,
                                     ProcessingRecipeData recipe,
                                     string inputCropId,
                                     int inputQuantity)
        {
            // 빈 슬롯 찾기
            // 입력 작물의 카테고리가 recipe.inputCategory에 맞는지 검증
            // 인벤토리에서 입력 아이템 차감 (이벤트를 통해 InventoryManager에 요청)
            // ProcessingSlot에 가공 정보 기록
            return false;
        }

        /// <summary>
        /// 시간 경과에 따른 가공 진행 처리. 매 시간 변경 시 호출.
        /// TimeManager.OnHourChanged에 구독.
        /// </summary>
        public void ProcessTimeAdvance(float elapsedHours)
        {
            // 모든 가공소의 모든 활성 슬롯에 대해:
            //   remainingHours -= elapsedHours
            //   if remainingHours <= 0:
            //     가공 완료 → 출력 아이템을 수거 대기 상태로 전환
            //     OnProcessingComplete 이벤트 발행
        }

        /// <summary>
        /// 완료된 가공품을 수거한다. 플레이어 인터랙션 시 호출.
        /// </summary>
        public bool CollectOutput(BuildingInstance processor, int slotIndex,
                                   out string outputItemId, out int quantity)
        {
            outputItemId = null;
            quantity = 0;
            return false;
        }

        /// <summary>
        /// 특정 가공소의 슬롯 상태를 반환 (UI용).
        /// </summary>
        public IReadOnlyList<ProcessingSlot> GetSlots(BuildingInstance processor)
        {
            return _processors.TryGetValue(processor, out var slots)
                ? slots
                : null;
        }
    }
}
```

### 7.3 ProcessingSlot

```csharp
// illustrative
namespace SeedMind.Building
{
    /// <summary>
    /// 가공소의 개별 가공 슬롯. ProcessingSaveData와 1:1 대응.
    /// </summary>
    public class ProcessingSlot
    {
        public enum SlotState { Empty, Processing, Completed }

        public SlotState State { get; set; }                // 슬롯 상태
        public ProcessingRecipeData Recipe { get; set; }    // 현재 레시피 (null = 빈 슬롯)
        public string InputCropId { get; set; }             // 입력 작물 ID
        public int InputQuantity { get; set; }              // 입력 수량
        public float RemainingHours { get; set; }           // 남은 시간
        public float TotalHours { get; set; }               // 총 소요 시간

        public bool IsEmpty => State == SlotState.Empty;
        public bool IsCompleted => State == SlotState.Completed;
        public float ProgressRatio => TotalHours > 0
            ? 1f - (RemainingHours / TotalHours)
            : 0f;
    }
}
```

### 7.4 처리 큐 관리

```
가공 시작 흐름:
    플레이어가 가공소에 인터랙트
    → ProcessingUI 표시 (사용 가능 레시피 목록)
    → 레시피 선택 + 입력 작물 선택
    → ProcessingSystem.StartProcessing() 호출
        ├── 빈 슬롯 검색
        ├── 입력 작물 카테고리 검증
        ├── 인벤토리에서 입력 아이템 차감 (이벤트 통신)
        └── 슬롯에 가공 정보 기록, State = Processing

시간 경과 흐름:
    TimeManager.OnHourChanged
    → ProcessingSystem.ProcessTimeAdvance(1.0f)
    → 각 활성 슬롯: remainingHours -= 1.0
    → remainingHours <= 0인 슬롯: State = Completed

수거 흐름:
    플레이어가 가공소에 인터랙트
    → 완료된 슬롯 표시
    → CollectOutput() → 인벤토리에 출력 아이템 추가 (이벤트 통신)
    → 슬롯 State = Empty
```

### 7.5 동시 슬롯 및 업그레이드

- 가공소의 초기 슬롯 수: (-> see `docs/pipeline/data-pipeline.md` 섹션 2.4, effectValue)
- 업그레이드 시 슬롯 추가: 기존 배열을 확장하여 새 슬롯 추가
- 최대 업그레이드 레벨: (-> see `docs/pipeline/data-pipeline.md` 섹션 2.4, maxUpgradeLevel)

---

## 8. 저장/로드 구조

### 8.1 시설 저장 데이터 흐름

```
저장 시:
    BuildingManager.GetSaveData()
    ├── foreach BuildingInstance:
    │   ├── BuildingSaveData 생성 (기본 필드)
    │   ├── Storage 시설이면: StorageSystem.GetSlots() → storageSlots[] 직렬화
    │   └── Processor 시설이면: (별도 ProcessingSaveData로 저장)
    └── ProcessingSystem.GetSaveData()
        └── foreach 활성 ProcessingSlot: ProcessingSaveData 생성

로드 시:
    BuildingManager.LoadSaveData(BuildingSaveData[], ProcessingSaveData[])
    ├── foreach BuildingSaveData:
    │   ├── DataRegistry에서 buildingId → BuildingData SO 복원
    │   ├── BuildingInstance 재생성
    │   ├── isOperational == true이면: 서브시스템에 등록
    │   ├── isOperational == false이면: constructionPrefab 배치, buildProgress 복원
    │   └── Storage 시설이면: storageSlots[] → StorageSlotContainer 복원
    └── foreach ProcessingSaveData:
        └── ProcessingSlot 복원 (Recipe SO는 DataRegistry로 복원)
```

### 8.2 SaveData C# 클래스

기존 data-pipeline.md Part II 섹션 2.6에 정의된 BuildingSaveData, ProcessingSaveData를 그대로 사용한다.

(-> see `docs/pipeline/data-pipeline.md` Part II 섹션 2.6 for BuildingSaveData, ProcessingSaveData C# 클래스 정의)

### 8.3 BuildingEvents (이벤트 허브)

```csharp
// illustrative
namespace SeedMind.Building
{
    /// <summary>
    /// 시설 시스템의 이벤트 허브. 다른 시스템이 구독하여 시설 변경에 반응.
    /// (FarmEvents 패턴과 동일, -> see docs/systems/farming-architecture.md)
    /// </summary>
    public static class BuildingEvents
    {
        // 건설 관련
        public static event Action<BuildingInstance> OnBuildingPlaced;
        public static event Action<BuildingInstance> OnBuildingCompleted;
        public static event Action<BuildingInstance, int> OnBuildingUpgraded; // inst, newLevel
        public static event Action<string> OnBuildingRemoved;                // buildingId

        // 가공 관련
        public static event Action<BuildingInstance, int> OnProcessingStarted;  // processor, slotIndex
        public static event Action<BuildingInstance, int> OnProcessingComplete; // processor, slotIndex
        public static event Action<BuildingInstance, int, string> OnProcessingCollected; // processor, slotIndex, outputItemId

        // 창고 관련
        public static event Action<BuildingInstance> OnStorageChanged;   // 슬롯 내용 변경 시

        // 내부 발행 메서드
        internal static void RaiseBuildingPlaced(BuildingInstance inst) => OnBuildingPlaced?.Invoke(inst);
        internal static void RaiseBuildingCompleted(BuildingInstance inst) => OnBuildingCompleted?.Invoke(inst);
        // ... 나머지 동일 패턴
    }
}
```

---

# Part II -- MCP 구현 계획

---

## Phase A: 시설 데이터 파이프라인 (ScriptableObject 생성)

**목표**: BuildingData SO **7개** + ProcessingRecipeData SO 32개를 MCP로 생성 (-> see `docs/content/processing-system.md` for 레시피 목록).

**MCP 명령 패턴**:

```
Step A-1: Create Script "BuildingData.cs" in Scripts/Building/Data/
          → GameDataSO 상속, 필드 정의 (섹션 2.2 참조)
Step A-2: Create Script "BuildingEffectType.cs" (enum) in Scripts/Building/Data/
Step A-3: Create Script "PlacementRule.cs" (enum) in Scripts/Building/Data/
Step A-4: Create Script "ProcessingRecipeData.cs" in Scripts/Building/Data/
          → GameDataSO 상속, 필드 정의 (섹션 7.1 참조)
Step A-5: Create Script "ProcessingType.cs" (enum) in Scripts/Building/Data/
Step A-6: Unity 컴파일 대기 → 에러 확인
Step A-7: Create SO asset "SO_Bldg_WaterTank" in Data/Buildings/
          → dataId: "building_water_tank", 필드값: (→ see docs/design.md 섹션 4.6 + docs/content/facilities.md)
Step A-8: Create SO asset "SO_Bldg_Greenhouse" in Data/Buildings/
          → dataId: "building_greenhouse", 필드값: (→ see docs/design.md 섹션 4.6 + docs/content/facilities.md)
Step A-9: Create SO asset "SO_Bldg_Storage" in Data/Buildings/
          → dataId: "building_storage", 필드값: (→ see docs/design.md 섹션 4.6 + docs/content/facilities.md)
Step A-10: Create SO asset "SO_Bldg_Processor" in Data/Buildings/
           → dataId: "building_processing", 필드값: (→ see docs/design.md 섹션 4.6 + docs/content/facilities.md)
Step A-11: Create SO asset "SO_Bldg_Mill" in Data/Buildings/
           → dataId: "building_mill", 필드값: (→ see docs/design.md 섹션 4.6 + docs/content/facilities.md 섹션 7)
Step A-12: Create SO asset "SO_Bldg_Fermentation" in Data/Buildings/
           → dataId: "building_fermentation", 필드값: (→ see docs/design.md 섹션 4.6 + docs/content/facilities.md 섹션 8)
Step A-13: Create SO asset "SO_Bldg_Bakery" in Data/Buildings/
           → dataId: "building_bakery", 필드값: (→ see docs/design.md 섹션 4.6 + docs/content/facilities.md 섹션 9)
Step A-14~A-45: Create SO asset "SO_Recipe_*" (32개) in Data/Buildings/Recipes/
           → 레시피 목록: (→ see docs/content/processing-system.md)
           → 필드값 공식: (→ see docs/pipeline/data-pipeline.md 섹션 2.5)
           → Editor 스크립트(CreateAllRecipes.cs) 일괄 생성 강력 권장 (32종 x ~16회 = ~512회 절감)
```

**검증 기준**:
- 모든 SO 에셋이 DataRegistry에 등록 가능한지 dataId 고유성 확인
- GameDataSO.Validate() 통과 확인 (MCP 콘솔 로그)

## Phase B: BuildingManager + BuildingInstance 기본 구조

**목표**: 시설 배치/철거/업그레이드의 핵심 로직 구현.

**MCP 명령 패턴**:

```
Step B-1: Create Script "BuildingInstance.cs" in Scripts/Building/
          → 섹션 3.2 참조
Step B-2: Create Script "BuildingEvents.cs" in Scripts/Building/
          → 섹션 8.3 참조
Step B-3: Create Script "BuildingManager.cs" in Scripts/Building/
          → MonoBehaviour, Singleton 상속
          → CanPlace, PlaceBuilding, RemoveBuilding, UpgradeBuilding 메서드
Step B-4: Create GameObject "BuildingManager" in SCN_Farm
          → Add BuildingManager.cs component
          → _allBuildingData에 SO 에셋 4개 할당
Step B-5: Create empty child "Buildings" under "--- FARM ---"
          → BuildingManager._buildingParent에 할당
Step B-6: Unity 컴파일 대기 → Play Mode 진입
Step B-7: MCP 콘솔에서 BuildingManager.CanPlace() 테스트
          → 유효 위치/무효 위치 모두 검증
Step B-8: MCP 콘솔에서 BuildingManager.PlaceBuilding() 테스트
          → 건설 시작 → OnBuildingPlaced 이벤트 발행 확인
```

**검증 기준**:
- CanPlace가 그리드 범위, 타일 상태, 골드, 레벨을 올바르게 검증하는지
- PlaceBuilding이 타일 상태를 Building으로 변경하는지
- RemoveBuilding이 타일 상태를 Empty로 복원하는지

## Phase C: 물탱크 + 온실 서브시스템

**목표**: WaterTankSystem, GreenhouseSystem 구현 및 FarmGrid/GrowthSystem 연동.

**MCP 명령 패턴**:

```
Step C-1: Create Script "WaterTankSystem.cs" in Scripts/Building/Buildings/
          → 섹션 4.1 참조
Step C-2: Create Script "ISeasonOverrideProvider.cs" in Scripts/Farm/
          → 인터페이스 정의 (섹션 5.3 참조)
Step C-3: Create Script "GreenhouseSystem.cs" in Scripts/Building/Buildings/
          → ISeasonOverrideProvider 구현, 섹션 5.1 참조
Step C-4: GrowthSystem.cs 수정 → ISeasonOverrideProvider 주입 받아 계절 검사에 활용
Step C-5: BuildingManager.cs 수정 → WaterTankSystem, GreenhouseSystem 인스턴스 생성
          → OnBuildingCompleted에서 effectType에 따라 서브시스템에 등록
Step C-6: Unity 컴파일 대기 → Play Mode 진입
Step C-7: MCP 콘솔에서 물탱크 건설 → 다음 날 → 범위 내 타일 자동 물주기 확인
Step C-8: MCP 콘솔에서 온실 건설 → 겨울 계절 변경 → 온실 내부에서 작물 성장 확인
```

**검증 기준**:
- 물탱크 범위 내 타일이 매일 아침 Watered 상태로 변경되는지
- 온실 내부 타일에서 겨울에도 작물이 성장하는지
- GrowthSystem priority(20)보다 BuildingManager priority(50)가 후순위인지

## Phase D: 창고 + 가공소 서브시스템

**목표**: StorageSystem, ProcessingSystem 구현 및 인벤토리/경제 시스템 연동.

**MCP 명령 패턴**:

```
Step D-1: Create Script "StorageSlot.cs" in Scripts/Building/Buildings/
Step D-2: Create Script "StorageSlotContainer.cs" in Scripts/Building/Buildings/
Step D-3: Create Script "StorageSystem.cs" in Scripts/Building/Buildings/
          → 섹션 6.1 참조
Step D-4: Create Script "ProcessingSlot.cs" in Scripts/Building/Buildings/
          → 섹션 7.3 참조
Step D-5: Create Script "ProcessingSystem.cs" in Scripts/Building/Buildings/
          → 섹션 7.2 참조
Step D-6: BuildingManager.cs 수정 → StorageSystem, ProcessingSystem 인스턴스 생성
          → OnBuildingCompleted에서 effectType에 따라 서브시스템에 등록
Step D-7: Unity 컴파일 대기 → Play Mode 진입
Step D-8: MCP 콘솔에서 창고 건설 → StoreItem/RetrieveItem 테스트
Step D-9: MCP 콘솔에서 가공소 건설 → StartProcessing → 시간 경과 → CollectOutput 테스트
```

**검증 기준**:
- 창고에 아이템 저장/꺼내기가 정상 작동하는지
- 가공소에서 레시피 선택 → 가공 시작 → 시간 경과 → 완료 → 수거 전체 흐름 확인
- 가공 슬롯이 동시에 여러 레시피를 처리할 수 있는지

## Phase E: 저장/로드 및 통합 테스트

**목표**: 시설 상태의 저장/로드 검증, 전체 시스템 통합 테스트.

**MCP 명령 패턴**:

```
Step E-1: BuildingManager에 GetSaveData/LoadSaveData 구현
Step E-2: SaveManager에 BuildingSaveData[], ProcessingSaveData[] 통합
Step E-3: Unity Play Mode → 시설 여러 개 건설 → 저장 → 재시작 → 로드 → 상태 복원 확인
Step E-4: 경계 케이스 테스트:
          - 건설 중 저장/로드 → buildProgress 복원 확인
          - 가공 중 저장/로드 → remainingHours 복원 확인
          - 창고에 아이템이 있는 상태에서 저장/로드 → storageSlots 복원 확인
Step E-5: DataRegistry 무결성 테스트
          - 존재하지 않는 buildingId로 로드 시 → 오류 처리 확인
Step E-6: 전체 통합 시나리오:
          - 물탱크 건설 → 자동 물주기 → 작물 성장 → 수확 → 창고 저장
          - 가공소 건설 → 작물 가공 → 가공품 수거 → 판매
```

**검증 기준**:
- 저장 후 로드 시 모든 시설 상태가 동일하게 복원되는지
- 건설 중/가공 중 저장-로드가 안전한지
- 전체 게임 루프에서 시설 시스템이 자연스럽게 동작하는지

---

## Cross-references

- `docs/design.md` 섹션 4.6 -- 시설 목록 및 비용/해금 canonical
- `docs/architecture.md` 섹션 3 -- Building 폴더 구조, 프로젝트 개요
- `docs/pipeline/data-pipeline.md` 섹션 2.4 -- BuildingData SO 필드 canonical 정의
- `docs/pipeline/data-pipeline.md` 섹션 2.5 -- ProcessingRecipeData SO 필드 canonical 정의
- `docs/pipeline/data-pipeline.md` Part II 섹션 2.6 -- BuildingSaveData, ProcessingSaveData C# 클래스
- `docs/systems/farming-architecture.md` -- FarmGrid, FarmTile, GrowthSystem (물탱크/온실 연동)
- `docs/systems/inventory-architecture.md` -- InventoryManager (창고 연동)
- `docs/systems/economy-architecture.md` -- EconomyManager (건설 비용, 가공품 판매)
- `docs/systems/economy-system.md` 섹션 2.5 -- 가공 공식 및 가격 canonical
- `docs/systems/time-season-architecture.md` -- TimeManager (OnDayChanged priority, OnHourChanged)
- `docs/systems/progression-architecture.md` -- 해금 레벨 연동
- `docs/systems/project-structure.md` 섹션 2~4 -- 네임스페이스, 의존성, asmdef

---

## Open Questions ([OPEN])

- 철거 환급은 `docs/content/facilities.md` 섹션 2.5에서 확정 (50%, 즉시 처리). OPEN 해소.
- 온실 내부 경작 가능 영역: Lv.1 = 4x4 (16타일), Lv.2 = 4x6 (24타일), Lv.3 = 6x6 (36타일). (-> see `docs/content/facilities.md` 섹션 4.2) OPEN 해소.
- [OPEN] 물탱크 범위 계산 방식: 맨해튼 거리(다이아몬드) vs 체비셰프 거리(정사각형). 현재 맨해튼으로 설계.
- [OPEN] Player -> Building 의존 금지로 인한 창고 아이템 이동 중재자 설계. UI 중재 방식이 가장 단순.
- [OPEN] ProcessingRecipeData에 description 필드 추가 여부. canonical(data-pipeline.md 섹션 2.5)과의 정합성 확인 필요.
- [OPEN] 시설 업그레이드 시스템(upgradeCosts)의 상세 설계 미정 (-> see `docs/pipeline/data-pipeline.md` 섹션 2.4).
- [OPEN] 가공소 업그레이드 시 슬롯 증가량 (1슬롯/레벨? 설정 가능하게 할지).

---

## Risks ([RISK])

- [RISK] ISeasonOverrideProvider 주입 타이밍: GameManager 초기화 시 BuildingManager가 GrowthSystem에 GreenhouseSystem을 등록해야 한다. 초기화 순서 잘못되면 NullReferenceException.
- [RISK] OnDayChanged priority 순서 변경 시 물탱크 효과 타이밍이 달라진다. GrowthSystem(20)보다 BuildingManager(50)가 먼저 실행되면 밸런스 파괴.
- [RISK] MCP for Unity에서 ScriptableObject의 배열/참조 필드(upgradeCosts[], prefab) 설정 지원 범위가 불확실 (-> see `docs/architecture.md` Risks).
- [RISK] 창고 철거 시 내부 아이템 처리. 인벤토리가 가득 차면 아이템 손실 가능. 철거 전 인벤토리 여유 확인 또는 아이템 드롭 필요.
- [RISK] 가공 중 저장-로드 시 ProcessingRecipeData SO 참조 복원 실패 가능. DataRegistry를 통한 recipeId -> SO 매핑이 반드시 성공해야 한다.

---

*이 문서는 Claude Code가 기술적 제약과 설계 목표를 고려하여 자율적으로 작성했습니다.*
