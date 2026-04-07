# 채집 시스템 기술 아키텍처 (Gathering Architecture)

> GatheringPointData SO, GatheringItemData SO, GatheringManager 싱글턴, GatheringProficiency 숙련도, 포인트 재생성 타이머, 인벤토리/경제/진행 시스템 연동, 세이브/로드 통합, MCP 구현 계획
> 작성: Claude Code (Opus) | 2026-04-07
> 문서 ID: ARC-031

---

## Context

이 문서는 SeedMind의 채집 시스템(Gathering System)에 대한 기술 아키텍처를 정의한다. 채집 시스템은 맵 곳곳(주로 Zone D 숲 지형 및 기타 구역)에 배치된 야생 자원(꽃, 열매, 버섯, 허브 등)을 플레이어가 직접 수집하여 수입원과 가공 재료로 활용하는 시스템이다.

**낚시 시스템과의 핵심 차이**:
- **인터랙션 방식**: 낚시는 미니게임(타이밍/게이지 판단)을 거치지만, 채집은 포인트 도달 후 단순 인터랙션(클릭/탭)으로 즉시 수집한다. 숙련도 레벨이 높아지면 간단한 보너스 판정(추가 드롭 등)이 적용될 수 있다.
- **위치 고정성**: 낚시는 Zone F 연못 가장자리의 자유 접근이지만, 채집 포인트는 맵 전역의 고정 위치에 스폰된다.
- **재생성 주기**: 낚시 포인트는 상시 이용 가능(일일 제한 있음)하지만, 채집 포인트는 수집 후 일정 시간(일 단위) 경과 후 재생성된다.
- **계절/날씨 영향**: 채집 가능 아이템은 계절에 따라 변경되며, 날씨가 특정 아이템의 출현 확률에 영향을 준다.

**설계 목표**:
- GatheringPointData/GatheringItemData SO로 채집 데이터를 외부화하여 코드 변경 없이 콘텐츠 확장 가능
- 포인트 재생성 타이머를 GatheringManager가 중앙 관리하여 세이브/로드 안정성 보장
- 기존 InventoryManager, EconomyManager, ProgressionManager와 자연스럽게 연동
- FishingProficiency와 동일한 Plain C# 패턴으로 GatheringProficiency 구현

**기존 시스템과의 관계**:
- FarmZoneManager(farm-expansion-architecture.md)가 Zone 해금/활성화를 관리하며, GatheringManager는 각 Zone 내 GatheringPoint 위치를 참조한다
- InventoryManager(inventory-architecture.md)는 `TryAddItem()` API를 통해 채집물을 배낭에 추가한다
- EconomyManager(economy-architecture.md)는 `GetSellPrice()` 호출 시 `HarvestOrigin.Gathering`을 인식하여 가격 보정을 적용한다
- ProgressionManager(progression-architecture.md)는 `XPSource.GatheringComplete`를 통해 채집 경험치를 부여한다
- TimeManager(time-season-architecture.md)는 `OnDayChanged` 이벤트를 통해 포인트 재생성을 트리거한다

---

# Part I -- 클래스 설계

---

## 1. 시스템 다이어그램

```
+---------------------------------------------------------------------+
|                    SeedMind.Gathering (네임스페이스)                    |
+---------------------------------------------------------------------+

+--------------------------------------------------------------+
|        GatheringManager (MonoBehaviour, Singleton)             |
|--------------------------------------------------------------|
|  [상태]                                                       |
|  - _gatheringPoints: GatheringPoint[]  // 씬 내 채집 포인트    |
|  - _pointStates: Dictionary<string, GatheringPointState>      |
|       // pointId -> 런타임 상태 (활성/비활성, 재생성 잔여일)     |
|  - _gatheringStats: GatheringStats     // 런타임 통계 (Plain C#)|
|  - _proficiency: GatheringProficiency  // 숙련도 추적 (Plain C#)|
|                                                              |
|  [설정 참조]                                                   |
|  - _gatheringConfig: GatheringConfig (ScriptableObject)       |
|                                                              |
|  [읽기 전용 프로퍼티]                                           |
|  + GatheringStats: GatheringStats (읽기 전용)                  |
|  + Proficiency: GatheringProficiency (읽기 전용)               |
|  + ProficiencyLevel: int              // _proficiency.Level   |
|                                                              |
|  [이벤트]                                                     |
|  + OnItemGathered: Action<GatheringItemData, CropQuality, int>|
|       // 채집 아이템, 품질, 수량                                |
|  + OnPointDepleted: Action<GatheringPoint>                    |
|       // 채집 포인트 소진됨                                     |
|  + OnPointRespawned: Action<GatheringPoint>                   |
|       // 채집 포인트 재생성됨                                   |
|  + OnProficiencyLevelUp: Action<int>   // newLevel            |
|                                                              |
|  [메서드]                                                     |
|  + Initialize(): void                                        |
|  + TryGather(GatheringPoint point): GatherResult             |
|  + IsPointActive(string pointId): bool                       |
|  + GetActivePointCount(): int                                |
|  - SelectGatheringItem(GatheringPoint point): GatheringItemData|
|  - CalculateQuantity(GatheringItemData item): int            |
|  - DetermineQuality(GatheringItemData item): CropQuality     |
|  - GrantRewards(GatheringItemData item, CropQuality q,       |
|       int quantity): void                                    |
|  - ProcessDayChange(int newDay): void                        |
|  - ProcessSeasonChange(Season newSeason): void               |
|  + GetSaveData(): GatheringSaveData                          |
|  + LoadSaveData(GatheringSaveData data): void                |
|                                                              |
|  [ISaveable 구현]                                             |
|  + SaveLoadOrder => 54                                       |
|  + GetSaveData(): object                                     |
|  + LoadSaveData(object data): void                           |
|                                                              |
|  [구독]                                                       |
|  + OnEnable():                                               |
|      TimeManager.RegisterOnDayChanged(priority: 56)          |
|      TimeManager.RegisterOnSeasonChanged(priority: 56)       |
|  + OnDisable(): 구독 해제                                      |
+--------------------------------------------------------------+
        | owns                          | references
        v                              v
+----------------------------+  +---------------------------+
| GatheringProficiency       |  | GatheringConfig (SO)      |
| (Plain C# class)           |  |---------------------------|
| (아래 섹션 4 참조)          |  | respawnDaysRange:         |
|                            |  |   Vector2Int (min, max)   |
+----------------------------+  | baseGatherEnergy: int     |
                                |   // -> see gathering-     |
+----------------------------+  |   //  system.md 섹션 2     |
| GatheringPointData (SO)    |  | maxActivePoints: int      |
| (아래 섹션 2.1 참조)        |  | seasonalRefreshOnChange:  |
+----------------------------+  |   bool                    |
                                | qualityThresholds:        |
+----------------------------+  |   float[] (Normal/Silver/ |
| GatheringItemData (SO)     |  |   Gold/Iridium)           |
| (아래 섹션 2.2 참조)        |  |   // -> see gathering-    |
+----------------------------+  |   //  system.md 섹션 4.5  |
                                | proficiency 관련 필드들    |
+----------------------------+  |   (섹션 4.3 참조)          |
| GatheringPoint             |  +---------------------------+
| (MonoBehaviour)            |
| (아래 섹션 3 참조)          |
+----------------------------+

+----------------------------+
| GatheringStats (Plain C#)  |
|----------------------------|
| totalGathered: int         |
| gatheredByItemId:          |
|   Dictionary<string, int>  |
| gatheredByZone:            |
|   Dictionary<string, int>  |
| rareItemsFound: int        |
+----------------------------+

+----------------------------+
| GatheringPointState        |
| (Plain C#, 직렬화 가능)     |
|----------------------------|
| pointId: string            |
| isActive: bool             |
| respawnDaysRemaining: int  |
| lastGatheredDay: int       |
| lastGatheredSeason: Season |
+----------------------------+

+----------------------------+
| GatherResult               |
| (Plain C# struct)          |
|----------------------------|
| success: bool              |
| item: GatheringItemData    |
| quality: CropQuality       |
| quantity: int              |
| bonusTriggered: bool       |
|   // 숙련도 보너스 드롭 여부 |
+----------------------------+
```

### 1.1 네임스페이스 배치

```
Assets/_Project/Scripts/Gathering/     # SeedMind.Gathering 네임스페이스
├── GatheringManager.cs                 # 싱글턴, 포인트 상태 관리, 인터랙션 진입점
├── GatheringPoint.cs                   # 채집 포인트 컴포넌트 (씬 배치)
├── GatheringProficiency.cs             # 숙련도 추적 (Plain C#)
├── GatheringEvents.cs                  # 정적 이벤트 허브
├── GatheringStats.cs                   # 통계 데이터 클래스
├── GatheringPointState.cs              # 포인트 런타임 상태 (Plain C#)
├── GatherResult.cs                     # 채집 결과 struct
├── GatheringSaveData.cs                # 세이브 데이터 클래스
└── Data/
    ├── GatheringPointData.cs           # SO: 채집 포인트 정의
    ├── GatheringItemData.cs            # SO: 채집 아이템 정의
    └── GatheringConfig.cs              # SO: 밸런스 설정
```

(-> see `docs/systems/project-structure.md` for 네임스페이스 규칙)

### 1.2 의존성 방향

```
TimeManager (Core)
    | events: OnDayChanged, OnSeasonChanged
    v
GatheringManager (Gathering)
    | owns
    +-- GatheringProficiency (숙련도 추적)
    +-- GatheringStats (통계)
    +-- GatheringPointState[] (포인트 상태 관리)
    | reads
    +-- GatheringPointData[] (SO, 포인트 정의)
    +-- GatheringItemData[] (SO, 아이템 데이터)
    +-- GatheringConfig (SO, 밸런스 설정)
    | references (씬)
    +-- GatheringPoint[] (각 Zone 내 채집 지점)
    | calls (외부 시스템)
    +-- InventoryManager.TryAddItem()      // 채집물 배낭 추가
    +-- ProgressionManager.AddExp()         // XP 부여
    +-- EconomyManager (간접 -- 판매는 ShopSystem 경유)
    | implements
    +-- ISaveable (SaveManager 등록)
```

**순환 참조 방지**: GatheringManager는 InventoryManager, ProgressionManager를 단방향으로 참조한다. 역방향 참조(외부 -> Gathering)는 이벤트(`GatheringEvents.OnItemGathered`)를 통해 느슨하게 연결한다. FishingManager와 동일한 패턴이다.

---

## 2. ScriptableObject 정의

### 2.1 GatheringPointData (ScriptableObject)

```
+--------------------------------------------------------------+
|          GatheringPointData : ScriptableObject                |
|--------------------------------------------------------------|
|  [기본 필드]                                                   |
|  + pointId: string                   // 예: "gp_forest_01"   |
|  + displayName: string               // 예: "숲 덤불"         |
|  + description: string               // 포인트 설명           |
|                                                              |
|  [위치/Zone 설정]                                              |
|  + zoneId: string                    // 소속 Zone (예: "zone_east_forest") |
|  + requiredZoneUnlocked: bool        // Zone 해금 필요 여부   |
|                                                              |
|  [아이템 풀]                                                   |
|  + availableItems: GatheringItemEntry[]  // 이 포인트에서 나오는 아이템 |
|       // GatheringItemEntry = { item: GatheringItemData, weight: float } |
|       // -> see docs/systems/gathering-system.md 섹션 3      |
|  + seasonOverrides: SeasonalItemOverride[]                   |
|       // 계절별 아이템 풀 교체 (null이면 기본 풀 사용)          |
|       // -> see docs/systems/gathering-system.md 섹션 3      |
|                                                              |
|  [재생성 설정]                                                 |
|  + respawnDays: int                  // 수집 후 재생성까지 일수 |
|       // -> see docs/systems/gathering-system.md 섹션 2      |
|  + respawnVariance: int              // 재생성 일수 변동폭 (+/-) |
|       // -> see docs/systems/gathering-system.md 섹션 2      |
|                                                              |
|  [비주얼]                                                     |
|  + pointPrefab: GameObject           // 채집 포인트 프리팹 참조 |
|  + depletedPrefab: GameObject        // 소진 상태 프리팹 (빈 덤불 등) |
|  + gatherVFX: GameObject             // 채집 시 이펙트         |
+--------------------------------------------------------------+
```

**GatheringItemEntry** (Inner struct):
```
+--------------------------------------------------------------+
|          GatheringItemEntry (직렬화 가능 struct)               |
|--------------------------------------------------------------|
|  + item: GatheringItemData           // 아이템 SO 참조        |
|  + weight: float                     // 가중치 (0.0~1.0 범위가 아닌 절대 가중치) |
+--------------------------------------------------------------+
```

**SeasonalItemOverride** (Inner class):
```
+--------------------------------------------------------------+
|          SeasonalItemOverride (직렬화 가능 class)              |
|--------------------------------------------------------------|
|  + season: Season                    // 적용 계절             |
|  + overrideItems: GatheringItemEntry[]  // 해당 계절 전용 아이템 풀 |
+--------------------------------------------------------------+
```

### 2.2 GatheringItemData (ScriptableObject)

```
+--------------------------------------------------------------+
|          GatheringItemData : GameDataSO                       |
|--------------------------------------------------------------|
|  [GameDataSO 상속 필드]                                        |
|  + dataId: string                    // 예: "gather_mushroom" |
|  + displayName: string               // 예: "숲 버섯"         |
|  + description: string               // 아이템 설명           |
|  + icon: Sprite                      // 인벤토리 아이콘       |
|                                                              |
|  [채집 아이템 고유 필드]                                        |
|  + gatheringCategory: GatheringCategory                      |
|       // Flower, Berry, Mushroom, Herb, Mineral, Special     |
|  + rarity: GatheringRarity           // Common/Uncommon/Rare/Legendary |
|  + basePrice: int                    // 기본 판매가            |
|       // -> see docs/systems/gathering-system.md (canonical)  |
|  + seasonAvailability: SeasonFlag    // 출현 계절 비트 플래그  |
|       // -> see docs/systems/gathering-system.md 섹션 3      |
|  + weatherBonus: WeatherFlag         // 보너스 날씨 (비 올 때 버섯 확률 증가 등) |
|       // -> see docs/systems/gathering-system.md 섹션 3      |
|  + baseQuantityRange: Vector2Int     // 기본 수량 범위 (min, max) |
|       // -> see docs/systems/gathering-system.md 섹션 3      |
|  + qualityEnabled: bool              // 품질 시스템 적용 여부  |
|       // false이면 항상 Normal 품질                            |
|  + maxStackSize: int                 // 인벤토리 스택 한도     |
|       // -> see docs/pipeline/data-pipeline.md 섹션 2.7       |
|  + expReward: int                    // 채집 시 XP            |
|       // -> see docs/balance/progression-curve.md (canonical)  |
|  + gatherTimeSec: float              // 채집 소요 시간 (초)    |
|       // -> see docs/systems/gathering-system.md 섹션 2      |
|  + requiredTool: ToolType            // 필요 도구 (None이면 맨손 채집) |
|       // -> see docs/systems/gathering-system.md 섹션 2      |
|  + minProficiencyLevel: int          // 최소 채집 숙련도 레벨  |
|       // -> see docs/systems/gathering-system.md 섹션 4      |
|                                                              |
|  [IInventoryItem 구현]                                        |
|  + ItemType => ItemType.Gathered                             |
|  + MaxStackSize => maxStackSize                              |
|  + Sellable => true                                          |
+--------------------------------------------------------------+
```

**GatheringCategory enum**:
```csharp
// illustrative
namespace SeedMind.Gathering
{
    public enum GatheringCategory
    {
        Flower,     // 꽃 -- 선물, 장식, 일부 가공 재료
        Berry,      // 열매 -- 식재료, 소모품 가공
        Mushroom,   // 버섯 -- 요리 재료, 비 오는 날 출현 증가
        Herb,       // 허브 -- 약품/향신료 가공
        Mineral,    // 광물/보석 -- 희귀, 높은 판매가
        Special     // 특수 -- 퀘스트/축제 관련 한정 아이템
    }
}
```

**GatheringRarity enum**:
```csharp
// illustrative
namespace SeedMind.Gathering
{
    public enum GatheringRarity
    {
        Common,     // 흔함 -- 높은 출현율
        Uncommon,   // 보통 -- 중간 출현율
        Rare,       // 희귀 -- 낮은 출현율
        Legendary   // 전설 -- 매우 희귀, 특수 조건 필요
    }
}
```

[OPEN] `GatheringRarity`를 `FishRarity`와 통합하여 `SeedMind.ItemRarity`로 상위 네임스페이스에 둘지 여부. 현재는 시스템 독립성을 위해 별도 enum으로 유지한다. 통합 결정은 Phase 2 리팩토링 시 결정.

### 2.3 GatheringConfig (ScriptableObject)

```
+--------------------------------------------------------------+
|          GatheringConfig : ScriptableObject                   |
|--------------------------------------------------------------|
|  [기본 채집 설정]                                              |
|  + baseGatherEnergy: int             // 채집 1회 에너지 소모   |
|       // -> see docs/systems/gathering-system.md 섹션 2       |
|  + gatherAnimationDuration: float    // 채집 애니메이션 시간 (초) |
|  + maxActivePointsPerZone: int       // Zone당 최대 활성 포인트 |
|       // -> see docs/systems/gathering-system.md 섹션 2       |
|                                                              |
|  [재생성 설정]                                                 |
|  + defaultRespawnDays: int           // 기본 재생성 일수       |
|       // -> see docs/systems/gathering-system.md 섹션 2       |
|  + seasonalRefreshOnChange: bool     // 계절 전환 시 전체 리프레시 |
|                                                              |
|  [품질 판정]                                                   |
|  + qualityThresholds: float[]        // [Normal, Silver, Gold, Iridium] |
|       // -> see docs/systems/gathering-system.md 섹션 4.5     |
|                                                              |
|  [숙련도 설정 -- 섹션 4 참조]                                   |
|  + proficiencyXPThresholds: int[]    // 레벨별 필요 누적 XP    |
|       // -> see docs/systems/gathering-system.md 섹션 4.2     |
|  + proficiencyMaxLevel: int          // 최대 레벨              |
|       // -> see docs/systems/gathering-system.md 섹션 4.2     |
|  + gatherXPByRarity: int[]           // [Common, Uncommon, Rare, Legendary] |
|       // -> see docs/systems/gathering-system.md 섹션 4.3     |
|  + bonusQuantityByLevel: float[]     // 레벨별 추가 수량 확률  |
|       // -> see docs/systems/gathering-system.md 섹션 4.4     |
|  + rarityBonusByLevel: float[]       // 레벨별 희귀 아이템 확률 보정 |
|       // -> see docs/systems/gathering-system.md 섹션 4.4     |
|  + energyCostReductionByLevel: int[] // 레벨별 에너지 소모 감소 |
|       // -> see docs/systems/gathering-system.md 섹션 4.2     |
|  + maxQualityByLevel: CropQuality[]  // 레벨별 최대 품질       |
|       // -> see docs/systems/gathering-system.md 섹션 4.5     |
|  + gatherSpeedMultiplierByLevel: float[] // 레벨별 채집 속도 배율 |
|       // -> see docs/systems/gathering-system.md 섹션 4.4     |
+--------------------------------------------------------------+
```

---

## 3. GatheringPoint (MonoBehaviour)

```
+--------------------------------------------------------------+
|              GatheringPoint (MonoBehaviour)                    |
|--------------------------------------------------------------|
|  [Inspector 설정]                                              |
|  + pointData: GatheringPointData     // SO 참조               |
|  + tilePosition: Vector2Int          // 맵 내 타일 좌표       |
|                                                              |
|  [상태 (GatheringManager가 관리)]                               |
|  - _isActive: bool                   // 현재 채집 가능 여부    |
|  - _visualRoot: GameObject           // 비주얼 루트 (활성/비활성 전환) |
|                                                              |
|  [읽기 전용 프로퍼티]                                           |
|  + PointId: string                   // pointData.pointId    |
|  + IsActive: bool                    // _isActive            |
|  + TilePosition: Vector2Int                                  |
|                                                              |
|  [메서드]                                                     |
|  + SetActive(bool active): void      // GatheringManager가 호출 |
|       // active == true: pointPrefab 표시, depletedPrefab 숨김 |
|       // active == false: pointPrefab 숨김, depletedPrefab 표시 |
|  + PlayGatherEffect(): void          // gatherVFX 재생        |
|  + GetInteractionPrompt(): string    // UI 표시용 프롬프트     |
+--------------------------------------------------------------+
```

**낚시 시스템과의 차이**: FishingPoint는 `_isOccupied`(누군가 낚시 중)와 `_dailyUseCount`(일일 사용 횟수)를 자체 관리하는 반면, GatheringPoint는 활성/비활성 상태만 보유하고 나머지 로직은 GatheringManager의 `_pointStates` Dictionary에서 중앙 관리한다. 이는 채집 포인트가 수십 개에 달할 수 있으므로, 상태를 중앙화하여 세이브/로드와 재생성 타이머 관리를 단순화하기 위한 설계 결정이다.

**Zone별 GatheringPoint 배치**: 각 Zone에 채집 포인트를 배치한다. Zone D(숲)에 가장 많은 포인트가 집중되며, 다른 Zone에도 소수 배치될 수 있다. 포인트 개수와 위치는 `(-> see docs/systems/gathering-system.md 섹션 3)`이 canonical이다.

---

## 4. 숙련도 시스템 (GatheringProficiency)

### 4.1 설계 개요

GatheringProficiency는 채집 전용 숙련도 XP/레벨을 추적하는 Plain C# 클래스이다. GatheringManager가 소유(`_proficiency` 필드)하며, 레벨업 시 보너스 수량, 희귀 아이템 확률 보정, 에너지 소모 감소 등의 해금을 처리한다.

**설계 결정**: FishingProficiency(fishing-architecture.md 섹션 4A)와 동일한 패턴을 따른다. MonoBehaviour가 아닌 Plain C# class로 설계한 이유는 GatheringManager 내부에서만 사용되는 데이터 클래스이며, 독립적인 씬 존재가 불필요하기 때문이다.

### 4.2 클래스 다이어그램

```
+--------------------------------------------------------------+
|           GatheringProficiency (Plain C# class)               |
|--------------------------------------------------------------|
|  [상태]                                                       |
|  - _currentXP: int                   // 누적 채집 숙련도 XP   |
|  - _currentLevel: int                // 현재 숙련도 레벨       |
|       // -> see docs/systems/gathering-system.md 섹션 4.2     |
|  - _config: GatheringConfig          // SO 참조 (XP 테이블 등) |
|                                                              |
|  [읽기 전용 프로퍼티]                                           |
|  + CurrentXP: int                                            |
|  + CurrentLevel: int                                         |
|  + XPToNextLevel: int                // 다음 레벨까지 남은 XP  |
|  + IsMaxLevel: bool                                          |
|                                                              |
|  [이벤트]                                                     |
|  + OnLevelUp: Action<int>            // newLevel              |
|       // GatheringManager.OnProficiencyLevelUp에 위임         |
|                                                              |
|  [메서드]                                                     |
|  + GatheringProficiency(GatheringConfig config): constructor  |
|  + AddXP(int amount): void           // XP 추가, 레벨업 판정  |
|  + GetXPForGather(GatheringRarity rarity): int               |
|       // -> see docs/systems/gathering-system.md 섹션 4.3     |
|  + GetBonusQuantityChance(): float                           |
|       // -> see docs/systems/gathering-system.md 섹션 4.4     |
|  + GetRarityBonus(): float                                   |
|       // -> see docs/systems/gathering-system.md 섹션 4.4     |
|  + GetMaxGatherQuality(): CropQuality                        |
|       // -> see docs/systems/gathering-system.md 섹션 4.5     |
|  + GetEnergyCostReduction(): int                             |
|       // -> see docs/systems/gathering-system.md 섹션 4.2     |
|  + GetGatherSpeedMultiplier(): float                         |
|       // -> see docs/systems/gathering-system.md 섹션 4.4     |
|                                                              |
|  [세이브/로드]                                                 |
|  + GetSaveData(): (int xp, int level)                        |
|  + LoadSaveData(int xp, int level): void                     |
+--------------------------------------------------------------+
```

### 4.3 AddXP 알고리즘

```
GatheringProficiency.AddXP(int amount):
    if (_currentLevel >= _config.proficiencyMaxLevel): return
    // -> see docs/systems/gathering-system.md 섹션 4.2 for maxLevel

    _currentXP += amount

    while (_currentLevel < _config.proficiencyMaxLevel):
        int nextLevelXP = _config.proficiencyXPThresholds[_currentLevel]
        // -> see docs/systems/gathering-system.md 섹션 4.2 for XP 테이블
        if (_currentXP >= nextLevelXP):
            _currentLevel++
            OnLevelUp?.Invoke(_currentLevel)
            // GatheringManager가 이 이벤트를 GatheringEvents.OnProficiencyLevelUp에 위임
            Debug.Log($"[GatheringProficiency] 레벨 업! Lv.{_currentLevel}")
        else:
            break
```

### 4.4 보정 메서드 pseudocode

모든 보정값은 GatheringConfig SO의 레벨별 배열에서 조회한다. 배열의 canonical 값은 `(-> see docs/systems/gathering-system.md 섹션 4)`이다.

```
GetBonusQuantityChance(): float
    return _config.bonusQuantityByLevel[_currentLevel - 1]
    // 레벨이 높을수록 추가 수량 획득 확률 증가
    // -> see docs/systems/gathering-system.md 섹션 4.4

GetRarityBonus(): float
    return _config.rarityBonusByLevel[_currentLevel - 1]
    // 레벨이 높을수록 희귀 아이템 출현 확률 보정
    // -> see docs/systems/gathering-system.md 섹션 4.4

GetMaxGatherQuality(): CropQuality
    return _config.maxQualityByLevel[_currentLevel - 1]
    // 레벨에 따라 획득 가능한 최대 품질 상승
    // -> see docs/systems/gathering-system.md 섹션 4.5

GetEnergyCostReduction(): int
    return _config.energyCostReductionByLevel[_currentLevel - 1]
    // 레벨이 높을수록 채집 에너지 소모 감소
    // -> see docs/systems/gathering-system.md 섹션 4.2

GetGatherSpeedMultiplier(): float
    return _config.gatherSpeedMultiplierByLevel[_currentLevel - 1]
    // 레벨이 높을수록 채집 애니메이션/소요 시간 감소
    // -> see docs/systems/gathering-system.md 섹션 4.4
```

---

## 5. 핵심 흐름

### 5.1 채집 인터랙션 흐름

```
플레이어가 GatheringPoint 인근에서 인터랙션 버튼 클릭
    |
    v
GatheringManager.TryGather(GatheringPoint point)
    |
    +-- 1) 사전 검증
    |       if (!IsPointActive(point.PointId)): return GatherResult.Fail
    |       if (PlayerController.Instance.CurrentEnergy <
    |           _gatheringConfig.baseGatherEnergy - _proficiency.GetEnergyCostReduction()):
    |           return GatherResult.Fail  // 에너지 부족
    |       if (item.requiredTool != ToolType.None &&
    |           !ToolSystem.Instance.HasTool(item.requiredTool)):
    |           return GatherResult.Fail  // 필요 도구 없음
    |
    +-- 2) 에너지 소모
    |       int energyCost = _gatheringConfig.baseGatherEnergy
    |           - _proficiency.GetEnergyCostReduction()
    |       // -> see docs/systems/gathering-system.md 섹션 2
    |       PlayerController.Instance.ConsumeEnergy(energyCost)
    |
    +-- 3) 아이템 선택
    |       GatheringItemData item = SelectGatheringItem(point)
    |       // 계절/날씨에 따른 아이템 풀 결정 후 가중치 랜덤 선택
    |       // 숙련도 희귀도 보정: weight *= _proficiency.GetRarityBonus()
    |
    +-- 4) 수량 결정
    |       int quantity = CalculateQuantity(item)
    |       // baseQuantityRange 범위 내 랜덤
    |       // 숙련도 보너스: Random.value < _proficiency.GetBonusQuantityChance()
    |       //   -> true면 quantity += 1
    |
    +-- 5) 품질 결정
    |       CropQuality quality = DetermineQuality(item)
    |       // qualityThresholds + 숙련도 maxQuality 제한
    |       // -> see docs/systems/gathering-system.md 섹션 4.5
    |
    +-- 6) 인벤토리 추가
    |       bool added = InventoryManager.Instance.TryAddItem(
    |           itemId: item.dataId,
    |           quantity: quantity,
    |           quality: quality,
    |           origin: HarvestOrigin.Gathering  // [신규]
    |       )
    |       if (!added):
    |           GatheringEvents.OnInventoryFull?.Invoke(item)
    |           return GatherResult.Fail  // 배낭 가득
    |
    +-- 7) XP 부여 (게임 진행 XP)
    |       int xp = item.expReward
    |       // -> see docs/balance/progression-curve.md (canonical)
    |       ProgressionManager.Instance.AddExp(xp, XPSource.GatheringComplete)
    |
    +-- 8) 채집 숙련도 XP 부여
    |       int gatherXP = _proficiency.GetXPForGather(item.rarity)
    |       // -> see docs/systems/gathering-system.md 섹션 4.3
    |       _proficiency.AddXP(gatherXP)
    |
    +-- 9) 통계 갱신
    |       _gatheringStats.totalGathered++
    |       _gatheringStats.gatheredByItemId[item.dataId] += quantity
    |       _gatheringStats.gatheredByZone[point.pointData.zoneId]++
    |       if (item.rarity >= GatheringRarity.Rare) _gatheringStats.rareItemsFound++
    |
    +-- 10) 포인트 비활성화 + 재생성 타이머 설정
    |       _pointStates[point.PointId].isActive = false
    |       _pointStates[point.PointId].respawnDaysRemaining =
    |           point.pointData.respawnDays + Random.Range(
    |               -point.pointData.respawnVariance,
    |               point.pointData.respawnVariance + 1)
    |       // -> see docs/systems/gathering-system.md 섹션 2
    |       _pointStates[point.PointId].lastGatheredDay =
    |           TimeManager.Instance.CurrentDay
    |       _pointStates[point.PointId].lastGatheredSeason =
    |           TimeManager.Instance.CurrentSeason
    |       point.SetActive(false)
    |       OnPointDepleted?.Invoke(point)
    |
    +-- 11) 이벤트 발행
    |       GatheringEvents.OnItemGathered?.Invoke(item, quality, quantity)
    |       // 구독자: AchievementManager, QuestManager 등
    |
    +-- 12) 비주얼 피드백
            point.PlayGatherEffect()
            // 채집 완료 파티클 + 사운드
```

### 5.2 포인트 재생성 흐름 (OnDayChanged)

```
GatheringManager.ProcessDayChange(int newDay):
    |
    foreach (pointId, state) in _pointStates:
        if (state.isActive): continue  // 이미 활성 -- 스킵
        |
        state.respawnDaysRemaining--
        if (state.respawnDaysRemaining <= 0):
            state.isActive = true
            state.respawnDaysRemaining = 0
            |
            // 씬 내 GatheringPoint 찾아서 활성화
            GatheringPoint point = FindPointById(pointId)
            if (point != null):
                point.SetActive(true)
                OnPointRespawned?.Invoke(point)
```

### 5.3 계절 전환 처리 (OnSeasonChanged)

```
GatheringManager.ProcessSeasonChange(Season newSeason):
    |
    if (_gatheringConfig.seasonalRefreshOnChange):
        // 모든 포인트를 즉시 재생성 (계절 전환 시 리프레시)
        foreach (pointId, state) in _pointStates:
            state.isActive = true
            state.respawnDaysRemaining = 0
            |
            GatheringPoint point = FindPointById(pointId)
            if (point != null):
                point.SetActive(true)
    |
    // 계절에 따라 아이템 풀이 자동 교체됨
    // (SelectGatheringItem에서 현재 계절 확인하여 seasonOverrides 적용)
```

---

## 6. 기존 시스템 확장

### 6.1 HarvestOrigin.Gathering 추가

**canonical 정의 위치**: `docs/systems/economy-architecture.md` 섹션 3.10.2

현재 HarvestOrigin enum:
```csharp
// illustrative -- 현재 상태 (-> see docs/systems/economy-architecture.md 섹션 3.10.2)
public enum HarvestOrigin
{
    Outdoor    = 0,
    Greenhouse = 1,
    Barn       = 2,
    Fishing    = 3
    // [OPEN] Cave = 4, Planter = 5 등 향후 확장
}
```

**필요 변경**:
```csharp
// illustrative -- 채집 시스템 추가 후
public enum HarvestOrigin
{
    Outdoor    = 0,
    Greenhouse = 1,
    Barn       = 2,
    Fishing    = 3,
    Gathering  = 4    // [신규] 야생 채집물 (ARC-031)
    // [OPEN] Cave = 5, Planter = 6 등 향후 확장
}
```

[RISK] `HarvestOrigin.Gathering` 추가 시 `economy-architecture.md` 섹션 3.10.3의 `GetGreenhouseMultiplier()` switch 문에 `Gathering` case를 추가해야 한다. 채집물은 온실 보정 대상이 아니므로 `return 1.0`을 반환한다.

**FIX 태스크**: `economy-architecture.md` 섹션 3.10.2 HarvestOrigin enum에 `Gathering = 4` 추가, 섹션 3.10.3 switch 문에 `case HarvestOrigin.Gathering: return 1.0;` 추가 (FIX-076 권고).

### 6.2 XPSource.GatheringComplete 추가

**canonical 정의 위치**: `docs/systems/progression-architecture.md` 섹션 2.2

현재 XPSource enum의 마지막 항목은 `FishingCatch`이다. 채집 시스템 추가로 다음 값이 필요하다:

```csharp
// illustrative -- progression-architecture.md 섹션 2.2에 추가
public enum XPSource
{
    CropHarvest,
    ToolUse,
    FacilityBuild,
    FacilityProcess,
    MilestoneReward,
    QuestComplete,
    AchievementReward,
    ToolUpgrade,
    AnimalCare,
    AnimalHarvest,
    FishingCatch,
    GatheringComplete,  // [신규] 채집 완료 (ARC-031)
}
```

**FIX 태스크**: `progression-architecture.md` 섹션 2.2 XPSource enum에 `GatheringComplete` 추가, 섹션 2.3 `GetExpForSource()` switch 문에 GatheringComplete case 추가 (FIX-077 권고).

**GetExpForSource() 확장 pseudocode**:
```
case XPSource.GatheringComplete:
    // 채집 완료 XP는 GatheringItemData.expReward에서 직접 전달
    // -> see docs/balance/progression-curve.md (canonical XP 수치)
    return (int)context;  // context = 채집 보상 XP (int)
```

### 6.3 ItemType.Gathered 추가

**canonical 정의 위치**: `docs/systems/inventory-architecture.md` 섹션 3.2

현재 ItemType enum에 `Gathered` 값을 추가한다:

```csharp
// illustrative -- inventory-architecture.md 섹션 3.2에 추가
public enum ItemType
{
    Crop,
    Seed,
    Tool,
    Fertilizer,
    Consumable,
    Processed,
    Material,
    Fish,
    Gathered,    // [신규] 야생 채집물 (ARC-031)
    Special
}
```

**FIX 태스크**: `inventory-architecture.md` 섹션 3.2 ItemType enum에 `Gathered` 추가, `data-pipeline.md` 관련 섹션 동시 업데이트 (FIX-078 권고).

### 6.4 GetGreenhouseMultiplier() 확장

```
// economy-architecture.md 섹션 3.10.3에 추가할 case:
Pseudocode: GetGreenhouseMultiplier(HarvestOrigin origin, ...)

    if origin == HarvestOrigin.Gathering:
        return 1.0  // 채집물은 온실 보정 적용 대상 외
    // ... 기존 로직 유지
```

---

# Part II -- 세이브/로드 연동

---

## 7. GatheringSaveData C# 클래스

```csharp
// illustrative
namespace SeedMind.Gathering
{
    [System.Serializable]
    public class GatheringSaveData
    {
        // 통계
        public int totalGathered;                               // 총 채집 횟수
        public Dictionary<string, int> gatheredByItemId;         // 아이템별 채집 횟수
        public Dictionary<string, int> gatheredByZone;           // Zone별 채집 횟수
        public int rareItemsFound;                               // 희귀 이상 아이템 발견 횟수

        // 포인트 상태
        public List<GatheringPointStateSaveData> pointStates;    // 모든 포인트의 활성/재생성 상태

        // 숙련도
        public int gatheringProficiencyXP;                       // 누적 숙련도 XP
        public int gatheringProficiencyLevel;                    // 현재 숙련도 레벨
            // -> see docs/systems/gathering-system.md 섹션 4.2 for 레벨 테이블
    }

    [System.Serializable]
    public class GatheringPointStateSaveData
    {
        public string pointId;                                   // 포인트 ID
        public bool isActive;                                    // 활성 여부
        public int respawnDaysRemaining;                          // 재생성 잔여일
        public int lastGatheredDay;                               // 마지막 채집 일
        public int lastGatheredSeason;                            // 마지막 채집 계절 (int로 직렬화)
    }
}
```

### 7.1 JSON <-> C# 필드 동기화 검증 (PATTERN-005)

| JSON 필드 | C# 필드 | 타입 | 비고 |
|-----------|---------|------|------|
| `totalGathered` | `totalGathered` | `int` | 일치 |
| `gatheredByItemId` | `gatheredByItemId` | `Dictionary<string, int>` | 일치 |
| `gatheredByZone` | `gatheredByZone` | `Dictionary<string, int>` | 일치 |
| `rareItemsFound` | `rareItemsFound` | `int` | 일치 |
| `pointStates` | `pointStates` | `List<GatheringPointStateSaveData>` | 일치 |
| `gatheringProficiencyXP` | `gatheringProficiencyXP` | `int` | 일치 |
| `gatheringProficiencyLevel` | `gatheringProficiencyLevel` | `int` | 일치 |

**GatheringPointStateSaveData 필드 동기화**:

| JSON 필드 | C# 필드 | 타입 | 비고 |
|-----------|---------|------|------|
| `pointId` | `pointId` | `string` | 일치 |
| `isActive` | `isActive` | `bool` | 일치 |
| `respawnDaysRemaining` | `respawnDaysRemaining` | `int` | 일치 |
| `lastGatheredDay` | `lastGatheredDay` | `int` | 일치 |
| `lastGatheredSeason` | `lastGatheredSeason` | `int` | 일치 (Season을 int로 직렬화) |

**필드 수**: GatheringSaveData JSON 7개, C# 7개 / GatheringPointStateSaveData JSON 5개, C# 5개 -- 동기화 완료.

**GatheringSaveData JSON 예시**:

```json
{
    "totalGathered": 58,
    "gatheredByItemId": {
        "gather_mushroom": 15,
        "gather_wildflower": 20,
        "gather_herb_mint": 10,
        "gather_berry_blue": 13
    },
    "gatheredByZone": {
        "zone_east_forest": 42,
        "zone_north_plain": 10,
        "zone_south_plain": 6
    },
    "rareItemsFound": 3,
    "pointStates": [
        {
            "pointId": "gp_forest_01",
            "isActive": true,
            "respawnDaysRemaining": 0,
            "lastGatheredDay": 15,
            "lastGatheredSeason": 0
        },
        {
            "pointId": "gp_forest_02",
            "isActive": false,
            "respawnDaysRemaining": 2,
            "lastGatheredDay": 18,
            "lastGatheredSeason": 0
        }
    ],
    "gatheringProficiencyXP": 185,
    "gatheringProficiencyLevel": 3
}
```

### 7.2 SaveLoadOrder 할당

| 시스템 | SaveLoadOrder | 근거 |
|--------|:------------:|------|
| GatheringManager | **54** | FishCatalogManager(53) 이후, InventoryManager(55) 이전. 채집 포인트 상태 복원은 Zone 해금 상태(45)와 플레이어 위치(50) 복원 후에 이루어져야 한다. GatheringManager는 인벤토리에 의존하지 않으며(포인트 상태 + 통계만 복원), 인벤토리가 채집물 아이템을 복원할 때 GatheringItemData SO가 이미 레지스트리에 있어야 하므로 54가 적절하다. |

(-> see `docs/systems/save-load-architecture.md` 섹션 7 for 전체 할당표)

**FIX 태스크**: `save-load-architecture.md` 섹션 7 SaveLoadOrder 할당표에 `GatheringManager | 54 | 채집 포인트 상태 복원` 행 추가 (FIX-079 권고).

### 7.3 GameSaveData 확장

**canonical 정의 위치**: `docs/pipeline/data-pipeline.md` Part I 섹션 2.1 (GameSaveData)

```csharp
// illustrative -- data-pipeline.md 섹션 2.1 GameSaveData에 추가
public class GameSaveData
{
    // ... 기존 필드 유지 ...
    public FishCatalogSaveData fishCatalog;    // 기존 마지막 필드 (ARC-030)

    // ARC-031: 채집 시스템 추가
    public GatheringSaveData gathering;         // 채집 통계/포인트 상태 (null 허용 -- 구버전 세이브 호환)
        // -> see docs/systems/gathering-architecture.md 섹션 7
}
```

**FIX 태스크**: `data-pipeline.md` Part I 섹션 2.1 GameSaveData 클래스에 `public GatheringSaveData gathering;` 필드 추가, null 허용 주석 추가 (FIX-080 권고).

### 7.4 세이브/로드 흐름

**저장**:
```
SaveManager.SaveAsync()
    |
    +-- ... [10]~[53] 이전 시스템 ...
    +-- [54] GatheringManager.GetSaveData()
    |       -> return new GatheringSaveData {
    |           totalGathered = _gatheringStats.totalGathered,
    |           gatheredByItemId = new Dictionary<string, int>(_gatheringStats.gatheredByItemId),
    |           gatheredByZone = new Dictionary<string, int>(_gatheringStats.gatheredByZone),
    |           rareItemsFound = _gatheringStats.rareItemsFound,
    |           pointStates = _pointStates.Values.Select(s => new GatheringPointStateSaveData {
    |               pointId = s.pointId,
    |               isActive = s.isActive,
    |               respawnDaysRemaining = s.respawnDaysRemaining,
    |               lastGatheredDay = s.lastGatheredDay,
    |               lastGatheredSeason = (int)s.lastGatheredSeason
    |           }).ToList(),
    |           gatheringProficiencyXP = _proficiency.CurrentXP,
    |           gatheringProficiencyLevel = _proficiency.CurrentLevel
    |       }
    +-- [55] InventoryManager ...
    +-- ... 나머지 시스템 ...
```

**로드**:
```
SaveManager.LoadAsync()
    |
    +-- ... [10]~[53] 이전 시스템 복원 ...
    +-- [54] GatheringManager.LoadSaveData(data.gathering)
    |       if (data.gathering == null):
    |           // 구버전 세이브 -- 초기값으로 세팅
    |           _gatheringStats = new GatheringStats()
    |           _proficiency.LoadSaveData(0, 1)       // Lv.1, 0 XP
    |           InitializeDefaultPointStates()         // 모든 포인트 활성화
    |       else:
    |           _gatheringStats.totalGathered = data.gathering.totalGathered
    |           _gatheringStats.gatheredByItemId = data.gathering.gatheredByItemId
    |               ?? new Dictionary<string, int>()   // null 방어
    |           _gatheringStats.gatheredByZone = data.gathering.gatheredByZone
    |               ?? new Dictionary<string, int>()   // null 방어
    |           _gatheringStats.rareItemsFound = data.gathering.rareItemsFound
    |           // 포인트 상태 복원
    |           foreach (stateSave in data.gathering.pointStates):
    |               _pointStates[stateSave.pointId] = new GatheringPointState {
    |                   pointId = stateSave.pointId,
    |                   isActive = stateSave.isActive,
    |                   respawnDaysRemaining = stateSave.respawnDaysRemaining,
    |                   lastGatheredDay = stateSave.lastGatheredDay,
    |                   lastGatheredSeason = (Season)stateSave.lastGatheredSeason
    |               }
    |           // 씬 내 GatheringPoint 비주얼 동기화
    |           foreach point in _gatheringPoints:
    |               if (_pointStates.ContainsKey(point.PointId)):
    |                   point.SetActive(_pointStates[point.PointId].isActive)
    |           // 숙련도 복원
    |           _proficiency.LoadSaveData(
    |               data.gathering.gatheringProficiencyXP,
    |               data.gathering.gatheringProficiencyLevel
    |           )
    +-- [55] InventoryManager ...
```

---

# Part III -- MCP 구현 태스크

---

## 8. MCP 구현 태스크 시퀀스

> 상세 태스크 시퀀스는 [별도 문서](docs/mcp/gathering-tasks.md)로 관리된다. 이 섹션은 Phase 요약만 제공한다.

### Phase A: SO 에셋 생성 (데이터 레이어)

| Step | MCP 명령 | 설명 |
|------|---------|------|
| A-1 | CreateScript `GatheringCategory.cs` | enum 정의 (SeedMind.Gathering) |
| A-2 | CreateScript `GatheringRarity.cs` | enum 정의 |
| A-3 | CreateScript `GatheringItemData.cs` : ScriptableObject | GameDataSO 상속, IInventoryItem 구현 |
| A-4 | CreateScript `GatheringPointData.cs` : ScriptableObject | 채집 포인트 정의 |
| A-5 | CreateScript `GatheringConfig.cs` : ScriptableObject | 밸런스 파라미터 |
| A-6 | CreateScript `GatheringItemEntry.cs` | 직렬화 가능 struct (아이템 + 가중치) |
| A-7 | CreateScript `SeasonalItemOverride.cs` | 직렬화 가능 class (계절별 아이템 풀) |
| A-8 | CreateScript `GatheringStats.cs` | Plain C# 통계 클래스 |
| A-9 | CreateScript `GatheringPointState.cs` | Plain C# 런타임 상태 클래스 |
| A-10 | CreateScript `GatherResult.cs` | 채집 결과 struct |
| A-11 | CreateScript `GatheringSaveData.cs` | 직렬화 가능 클래스 (proficiency 포함) |
| A-12 | CreateScript `GatheringPointStateSaveData.cs` | 포인트 상태 세이브 데이터 |
| A-13 | CreateScript `GatheringEvents.cs` | 정적 이벤트 허브 (OnProficiencyLevelUp 포함) |
| A-14 | CreateScript `GatheringProficiency.cs` | Plain C# 숙련도 클래스 |

### Phase B: GatheringManager 구현 (시스템 레이어)

| Step | MCP 명령 | 설명 |
|------|---------|------|
| B-1 | CreateScript `GatheringPoint.cs` : MonoBehaviour | 채집 포인트 컴포넌트 (씬 배치) |
| B-2 | CreateScript `GatheringManager.cs` : MonoBehaviour | 싱글턴, ISaveable 구현, 포인트 상태 관리 |

### Phase C: SO 에셋 인스턴스 생성

| Step | MCP 명령 | 설명 |
|------|---------|------|
| C-1 | CreateAsset `GatheringConfig_Default` | GatheringConfig SO 에셋 |
| C-2 | CreateAsset `GatheringItemData_*` | 아이템별 SO 에셋 `(-> see docs/systems/gathering-system.md for 아이템 목록)` |
| C-3 | CreateAsset `GatheringPointData_*` | 포인트별 SO 에셋 `(-> see docs/systems/gathering-system.md for 포인트 목록)` |
| C-4 | CreateAsset `PriceData_Gather_*` | 채집물별 가격 SO |

### Phase D: 포인트 배치 (씬 구성)

| Step | MCP 명령 | 설명 |
|------|---------|------|
| D-1 | CreateGameObject `GatheringManager` | 싱글턴 오브젝트, DontDestroyOnLoad |
| D-2 | Zone D(숲) 내 GatheringPoint x N 배치 | `tilePosition` 설정, `pointData` SO 할당 |
| D-3 | Zone B/C(평야) 내 GatheringPoint x N 배치 | 소수 배치 |
| D-4 | GatheringManager Inspector에 GatheringPoint[], GatheringConfig 할당 | |

### Phase E: 기존 시스템 확장

| Step | MCP 명령 | 설명 |
|------|---------|------|
| E-1 | EditScript `HarvestOrigin.cs` | `Gathering = 4` 추가 |
| E-2 | EditScript `XPSource.cs` | `GatheringComplete` 추가 |
| E-3 | EditScript `ProgressionManager.cs` | `GetExpForSource()` switch에 GatheringComplete case 추가, `GatheringEvents.OnItemGathered` 구독 추가 |
| E-4 | EditScript `ItemType.cs` | `Gathered` 값 추가 |
| E-5 | EditScript `GameSaveData.cs` | `public GatheringSaveData gathering;` 추가 |
| E-6 | EditScript `SaveManager.cs` | GatheringManager ISaveable 등록 확인 |
| E-7 | EditScript `GetGreenhouseMultiplier()` | `HarvestOrigin.Gathering` case 추가 (`return 1.0`) |

### Phase F: UI 연동

| Step | MCP 명령 | 설명 |
|------|---------|------|
| F-1 | 채집 가능 포인트 인터랙션 프롬프트 UI | "E키로 채집" 등 |
| F-2 | 채집 결과 팝업 | 획득 아이템/수량/품질 표시 |
| F-3 | 채집 숙련도 레벨업 토스트 알림 | FishingProficiency 토스트와 동일 패턴 |

### Phase G: 검증

| Step | 검증 내용 |
|------|----------|
| G-1 | GatheringManager 싱글턴 정상 생성 확인 (MCP Console.Log) |
| G-2 | Zone D 해금 -> GatheringPoint 활성화 확인 |
| G-3 | 채집 인터랙션 -> 아이템 획득 -> 인벤토리 추가 흐름 확인 |
| G-4 | 포인트 비활성화 -> 재생성 타이머 -> 재활성화 흐름 확인 |
| G-5 | 계절 전환 시 아이템 풀 교체 확인 |
| G-6 | 계절 전환 시 전체 포인트 리프레시 확인 (seasonalRefreshOnChange) |
| G-7 | XP 부여 확인 (XPSource.GatheringComplete) |
| G-8 | 채집 숙련도 XP 부여 및 레벨업 확인 |
| G-9 | 세이브 -> 로드 후 포인트 상태/통계/숙련도 복원 확인 |
| G-10 | 에너지 부족 시 채집 거부 확인 |
| G-11 | 필요 도구 미보유 시 채집 거부 확인 |
| G-12 | HarvestOrigin.Gathering이 판매 시 올바르게 인식되는지 확인 |

---

## 9. 기술 리스크 및 오픈 항목

### 리스크

1. [RISK] **포인트 상태 Dictionary 세이브 크기**: 채집 포인트 수가 수십 개에 달할 경우 `pointStates` 배열이 JSON 세이브 파일 크기를 증가시킨다. 최적화로 활성 포인트(기본 상태)는 저장하지 않고 비활성 포인트만 저장하는 방식을 고려할 수 있다. Phase 2에서 프로파일링 후 결정.

2. [RISK] **GatheringSaveData 구버전 호환**: `gathering` 필드가 null인 구버전 세이브 파일 로드 시, 모든 포인트를 활성 상태로 초기화하는 방어 로직이 필수이다.

3. [RISK] **Season enum 직렬화**: `GatheringPointStateSaveData.lastGatheredSeason`을 int로 직렬화한다. Season enum 값이 변경되면 기존 세이브와 호환성이 깨질 수 있다. Season enum은 이미 안정화되어 있으므로 리스크는 낮다.

4. [RISK] **Zone 미해금 상태에서 포인트 재생성 타이머**: Zone이 해금되기 전에도 내부적으로 타이머가 동작하면 최초 해금 시 모든 포인트가 이미 활성화되어 있을 수 있다. 해금 전에는 타이머를 작동시키지 않거나, 해금 시점에 초기화하는 정책이 필요하다.

### 오픈 항목

1. [OPEN] **간단 미니게임 도입 여부**: 현재 설계는 단순 인터랙션(클릭 -> 수집)이지만, 숙련도 레벨이 높아질 때 또는 Rare/Legendary 아이템 채집 시 간단한 미니게임(타이밍 클릭 등)을 도입할지 여부. Designer와 합의 필요.

2. [OPEN] **GatheringRarity와 FishRarity 통합**: 두 enum이 동일한 구조(Common/Uncommon/Rare/Legendary)를 가진다. `SeedMind.ItemRarity`로 통합하면 코드 중복이 줄지만, 시스템 간 결합도가 높아진다.

3. [OPEN] **채집 도구 시스템**: 일부 채집물이 특정 도구(낫으로 풀 베기, 곡괭이로 광물 캐기 등)를 요구하는 설계이나, 도구별 채집 효과(범위, 속도)의 세부 사양은 `gathering-system.md`에서 확정 필요.

4. [OPEN] **날씨 영향 세부 설계**: `weatherBonus` 필드가 존재하지만, 구체적인 보정 계수(비 오는 날 버섯 +50% 등)는 `gathering-system.md`에서 확정 필요.

---

## Cross-references

- `docs/systems/gathering-system.md` -- 채집 시스템 디자인 (canonical: 아이템 목록, 가격, 숙련도 테이블, 포인트 배치)
- `docs/systems/fishing-architecture.md` -- 낚시 아키텍처 (유사 패턴 참조: Proficiency, SaveData, MCP Phase)
- `docs/systems/save-load-architecture.md` -- 세이브/로드 통합 (SaveLoadOrder=54, GameSaveData 확장)
- `docs/pipeline/data-pipeline.md` -- ScriptableObject 구조, GameSaveData 루트 클래스
- `docs/systems/progression-architecture.md` -- XPSource enum, AddExp 패턴 (GatheringComplete 추가 필요)
- `docs/systems/economy-architecture.md` -- HarvestOrigin enum, GetGreenhouseMultiplier (Gathering 추가 필요)
- `docs/systems/inventory-architecture.md` -- ItemType enum, TryAddItem API (Gathered 추가 필요)
- `docs/systems/farm-expansion-architecture.md` -- FarmZoneManager, Zone 해금 상태
- `docs/systems/tool-upgrade-architecture.md` -- ToolType, 도구 보유 확인 API
- `docs/systems/time-season-architecture.md` -- TimeManager, OnDayChanged/OnSeasonChanged 이벤트
- `docs/systems/project-structure.md` -- 네임스페이스 규칙, 폴더 구조

---

## FIX 태스크 요약

| FIX ID | 대상 문서 | 변경 내용 | 상태 |
|--------|----------|----------|------|
| FIX-076 | `economy-architecture.md` | 섹션 3.10.2 HarvestOrigin에 `Gathering = 4` 추가, 섹션 3.10.3 switch에 case 추가; 섹션 3.11 SupplyCategory enum에 `Forage = 4` 추가 및 수급 파라미터 정의 | PENDING |
| FIX-077 | `progression-architecture.md` | 섹션 2.2 XPSource에 `GatheringComplete` 추가, 섹션 2.3 switch에 case 추가 | PENDING |
| FIX-078 | `inventory-architecture.md`, `data-pipeline.md` | ItemType에 `Gathered` 추가 | PENDING |
| FIX-079 | `save-load-architecture.md` | 섹션 7 SaveLoadOrder 할당표에 `GatheringManager | 54` 추가 | PENDING |
| FIX-080 | `data-pipeline.md` | Part I 섹션 2.1 GameSaveData에 `GatheringSaveData gathering` 필드 추가 | PENDING |
