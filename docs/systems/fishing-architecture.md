# 낚시 시스템 기술 아키텍처

> FishingManager, FishData SO, FishingMinigame 상태 머신, 인벤토리/경제/진행 시스템 연동, 세이브/로드 통합, MCP 구현 계획
> 작성: Claude Code (Opus) | 2026-04-07
> 문서 ID: ARC-026

---

## Context

이 문서는 SeedMind의 낚시 시스템에 대한 기술 아키텍처를 정의한다. 낚시 시스템은 Zone F(연못 구역)를 중심으로 작동하며, 플레이어에게 경작 외의 수입원과 미니게임 인터랙션을 제공한다.

Zone F는 12x8(96타일) 크기로 연못 타일 30개를 포함하며, 해금 조건은 `(-> see docs/systems/farm-expansion.md 섹션 2.1)`. Zone F의 ZoneType은 `Pond`이다 `(-> see docs/systems/farm-expansion-architecture.md 섹션 2.2)`.

**설계 목표**:
- 낚시 포인트 인터랙션 -> 미니게임(타이밍 게이지) -> 물고기 획득이라는 명확한 흐름
- FishData SO로 어종 데이터를 외부화하여 코드 변경 없이 밸런스 조정 가능
- 기존 InventoryManager, EconomyManager, ProgressionManager와 자연스럽게 연동
- 미니게임 상태 머신을 명확히 분리하여 독립 테스트 가능

**기존 시스템과의 관계**:
- FarmZoneManager(farm-expansion-architecture.md)가 Zone F 해금/활성화를 관리하며, FishingManager는 Zone F 내 FishingPoint 위치를 참조한다
- InventoryManager(inventory-architecture.md)는 `AddItem()` API를 통해 낚은 물고기를 배낭에 추가한다
- EconomyManager(economy-architecture.md)는 `GetSellPrice()` 호출 시 `HarvestOrigin.Fishing`을 인식하여 가격 보정을 적용한다
- ProgressionManager(progression-architecture.md)는 `XPSource.FishingCatch`를 통해 낚시 경험치를 부여한다

---

# Part I -- 클래스 설계

---

## 1. 클래스 다이어그램

```
+---------------------------------------------------------------------+
|                       SeedMind.Fishing (네임스페이스)                  |
+---------------------------------------------------------------------+

+--------------------------------------------------------------+
|          FishingManager (MonoBehaviour, Singleton)             |
|--------------------------------------------------------------|
|  [상태]                                                       |
|  - _fishDataRegistry: FishData[]     // Inspector 할당, 전체 어종 SO |
|  - _fishingPoints: FishingPoint[]    // 씬 내 낚시 포인트 참조  |
|  - _currentState: FishingState       // 현재 상태 머신 상태     |
|  - _activeFish: FishData             // 현재 걸린 물고기 (null 가능) |
|  - _fishingStats: FishingStats       // 런타임 통계 (Plain C#)  |
|                                                              |
|  [설정 참조]                                                   |
|  - _fishingConfig: FishingConfig (ScriptableObject)           |
|  - _minigame: FishingMinigame        // 미니게임 로직 (내부 인스턴스) |
|                                                              |
|  [읽기 전용 프로퍼티]                                           |
|  + CurrentState: FishingState                                |
|  + ActiveFish: FishData                                      |
|  + FishingStats: FishingStats (읽기 전용)                     |
|  + IsPlayerFishing: bool             // Idle이 아닌 모든 상태  |
|                                                              |
|  [이벤트]                                                     |
|  + OnStateChanged: Action<FishingState, FishingState>        |
|         // oldState, newState                                |
|  + OnFishCaught: Action<FishData, CropQuality>               |
|         // 잡은 물고기, 품질                                   |
|  + OnFishingFailed: Action                                   |
|                                                              |
|  [메서드]                                                     |
|  + Initialize(): void                                        |
|  + TryStartFishing(FishingPoint point): bool                 |
|  + CancelFishing(): void                                     |
|  + OnMinigameInput(): void           // 플레이어 입력 전달     |
|  - SelectRandomFish(FishingPoint point): FishData            |
|  - CalculateBiteDelay(): float                               |
|  - CompleteFishing(bool success): void                       |
|  - GrantRewards(FishData fish, CropQuality quality): void    |
|  + GetSaveData(): FishingSaveData                            |
|  + LoadSaveData(FishingSaveData data): void                  |
|                                                              |
|  [ISaveable 구현]                                             |
|  + SaveLoadOrder => 52                                       |
|  + GetSaveData(): object                                     |
|  + LoadSaveData(object data): void                           |
|                                                              |
|  [구독]                                                       |
|  + OnEnable():                                               |
|      TimeManager.RegisterOnDayChanged(priority: 55)          |
|      TimeManager.RegisterOnSeasonChanged(priority: 55)       |
|  + OnDisable(): 구독 해제                                      |
+--------------------------------------------------------------+
         | owns                          | references
         v                              v
+----------------------------+  +---------------------------+
| FishingMinigame            |  | FishingConfig (SO)        |
| (Plain C# class)           |  |---------------------------|
|                            |  | castDurationRange:        |
| (아래 섹션 2 참조)          |  |   Vector2 (min, max)      |
|                            |  | biteDelayRange:           |
+----------------------------+  |   Vector2 (min, max)      |
                                | biteWindowDuration: float |
+----------------------------+  | reelingDuration: float    |
| FishData (SO)              |  | excitementDecayRate: float|
| (아래 섹션 3 참조)          |  | excitementGainPerInput:   |
|                            |  |   float                   |
+----------------------------+  | successThreshold: float   |
                                | failThreshold: float      |
+----------------------------+  | qualityThresholds:        |
| FishingPoint               |  |   float[] (Normal/Silver/ |
| (MonoBehaviour)            |  |   Gold/Iridium)           |
| (아래 섹션 4 참조)          |  +---------------------------+
+----------------------------+

+----------------------------+
| FishingStats (Plain C#)    |
|----------------------------|
| totalCasts: int            |
| totalCaught: int           |
| totalFailed: int           |
| caughtByFishId:            |
|   Dictionary<string, int>  |
| rareFishCaught: int        |
| maxStreak: int             |
| currentStreak: int         |
+----------------------------+
```

### 1.1 네임스페이스 배치

```
Assets/_Project/Scripts/Fishing/     # SeedMind.Fishing 네임스페이스
├── FishingManager.cs                 # 싱글턴, 상태 관리, 인터랙션 진입점
├── FishingMinigame.cs                # 미니게임 로직 (ExcitementGauge 방식)
├── FishingPoint.cs                   # 낚시 포인트 컴포넌트 (씬 배치)
├── FishingEvents.cs                  # 정적 이벤트 허브
├── FishingState.cs                   # enum 정의
├── FishingStats.cs                   # 통계 데이터 클래스
└── FishingSaveData.cs                # 세이브 데이터 클래스
```

(-> see `docs/systems/project-structure.md` for 네임스페이스 규칙)

### 1.2 의존성 방향

```
TimeManager (Core)
    | events: OnDayChanged, OnSeasonChanged
    v
FishingManager (Fishing)
    | owns
    +-- FishingMinigame (로직 위임)
    | reads
    +-- FishData[] (SO, 어종 데이터)
    +-- FishingConfig (SO, 밸런스 설정)
    | references (씬)
    +-- FishingPoint[] (Zone F 내 낚시 지점)
    | calls (외부 시스템)
    +-- InventoryManager.TryAddItem()      // 물고기 배낭 추가
    +-- ProgressionManager.AddExp()         // XP 부여
    +-- EconomyManager (간접 -- 판매는 ShopSystem 경유)
    | implements
    +-- ISaveable (SaveManager 등록)
```

**순환 참조 방지**: FishingManager는 InventoryManager, ProgressionManager를 단방향으로 참조한다. 역방향 참조(Inventory -> Fishing)는 이벤트(`FishingEvents.OnFishCaught`)를 통해 느슨하게 연결한다.

---

## 2. FishingMinigame (Plain C# class)

```
+--------------------------------------------------------------+
|               FishingMinigame (Plain C# class)                |
|--------------------------------------------------------------|
|  [상태]                                                       |
|  - _excitementGauge: float           // 0.0 ~ 1.0 현재 게이지 |
|  - _targetZoneCenter: float          // 0.0 ~ 1.0 타깃 존 중심 |
|  - _targetZoneWidth: float           // 타깃 존 폭 (난이도 의존) |
|  - _elapsedTime: float               // 릴링 경과 시간         |
|  - _isActive: bool                                           |
|                                                              |
|  [설정 참조]                                                   |
|  - _config: FishingConfig (SO)                               |
|  - _currentFish: FishData            // 난이도 파라미터 참조    |
|                                                              |
|  [읽기 전용 프로퍼티]                                           |
|  + ExcitementGauge: float                                    |
|  + TargetZoneCenter: float                                   |
|  + TargetZoneWidth: float                                    |
|  + ElapsedTime: float                                        |
|  + IsActive: bool                                            |
|                                                              |
|  [메서드]                                                     |
|  + Start(FishData fish, FishingConfig config): void          |
|  + Update(float deltaTime): MinigameResult                   |
|  + ProcessInput(): void              // 플레이어 입력 처리      |
|  + Reset(): void                                             |
|                                                              |
|  [내부 메서드]                                                 |
|  - MoveTargetZone(float deltaTime): void                     |
|  - ApplyDecay(float deltaTime): void                         |
|  - CheckCompletion(): MinigameResult                         |
|  - CalculateQuality(): CropQuality                           |
+--------------------------------------------------------------+
```

**ExcitementGauge 방식 설명**:
- 릴링(Reeling) 상태에서 세로 게이지가 표시된다
- 물고기 아이콘이 게이지 내에서 상하로 불규칙하게 이동한다 (타깃 존)
- 플레이어가 입력(클릭/탭)하면 게이지 커서가 상승하고, 입력하지 않으면 하강한다
- 커서가 타깃 존 안에 있으면 excitementGauge가 증가, 밖에 있으면 감소
- excitementGauge가 successThreshold 이상이면 성공, failThreshold 이하면 실패
- 물고기 희귀도에 따라 타깃 존 폭과 이동 속도가 달라진다

> [OPEN] **미니게임 방식 불일치**: `docs/systems/fishing-system.md` 섹션 3(Timing Gauge System)은 **Oscillating Bar** 방식(가로 게이지, 좌우 왕복 커서, 클릭으로 진행도 축적)을 채택한다고 명시하고 있다. 본 아키텍처 문서(ARC-026)는 **ExcitementGauge** 방식(세로 게이지, 상하 이동 타깃 존, 입력 시 상승/미입력 시 하강)을 구현 대상으로 기술한다. 두 방식은 구현 방식이 근본적으로 다르므로, Designer와 Architect 간 합의 후 하나로 통일해야 한다. FIX-055 권고.

**MinigameResult enum**:
```csharp
// illustrative
namespace SeedMind.Fishing
{
    public enum MinigameResult
    {
        InProgress,     // 아직 진행 중
        Success,        // 게이지가 successThreshold 도달
        Fail            // 게이지가 failThreshold 이하로 하락
    }
}
```

---

## 3. FishData (ScriptableObject)

```
+--------------------------------------------------------------+
|                  FishData : GameDataSO                         |
|--------------------------------------------------------------|
|  [GameDataSO 상속 필드]                                        |
|  + dataId: string                    // 예: "fish_carp"       |
|  + displayName: string               // 예: "잉어"            |
|  + description: string               // 아이템 설명           |
|  + icon: Sprite                      // 인벤토리 아이콘       |
|                                                              |
|  [어종 고유 필드]                                              |
|  + fishId: string                    // dataId 별칭           |
|  + rarity: FishRarity                // Common/Uncommon/Rare/Legendary |
|  + basePrice: int                    // 기본 판매가            |
|       // -> see docs/systems/fishing-system.md (canonical)    |
|  + seasonAvailability: SeasonFlag    // 출현 계절 비트 플래그  |
|  + timeWeights: float[]             // 시간대별 가중치 (5개 슬롯) |
|       // [Dawn, Morning, Afternoon, Evening, Night]          |
|       // -> see docs/systems/time-season.md 섹션 1.2 for 시간대 정의 |
|  + weatherBonus: WeatherFlag         // 보너스 날씨 (비 올 때 출현율 증가 등) |
|  + minigameDifficulty: float         // 0.0~1.0, 미니게임 난이도 보정 |
|  + targetZoneWidthMul: float         // 타깃 존 폭 배율 (1.0 = 기본) |
|  + moveSpeed: float                  // 타깃 존 이동 속도      |
|  + maxStackSize: int                 // 인벤토리 스택 한도     |
|       // -> see docs/systems/inventory-architecture.md 섹션 1.1 |
|  + expReward: int                    // 잡았을 때 XP          |
|       // -> see docs/balance/progression-curve.md (canonical)  |
|                                                              |
|  [IInventoryItem 구현]                                        |
|  + ItemType => ItemType.Fish                                 |
|  + MaxStackSize => maxStackSize                              |
|  + Sellable => true                                          |
+--------------------------------------------------------------+
```

**FishRarity enum**:
```csharp
// illustrative
namespace SeedMind.Fishing
{
    public enum FishRarity
    {
        Common,         // 흔함 -- 높은 출현율, 낮은 난이도
        Uncommon,       // 보통 -- 중간 출현율
        Rare,           // 희귀 -- 낮은 출현율, 높은 난이도
        Legendary       // 전설 -- 매우 희귀, 특수 조건 필요
    }
}
```

[RESOLVED-FIX-053] `ItemType` enum에 `Fish` 값이 추가되었다. `docs/pipeline/data-pipeline.md` Part I 섹션 2.4 및 `docs/systems/inventory-architecture.md` 섹션 3.2, `docs/mcp/inventory-tasks.md` Step 1-01에 동시 반영. (→ see docs/systems/inventory-architecture.md 섹션 3.2)

---

## 4. FishingPoint (MonoBehaviour)

```
+--------------------------------------------------------------+
|              FishingPoint (MonoBehaviour)                      |
|--------------------------------------------------------------|
|  [Inspector 설정]                                              |
|  + pointId: string                   // 고유 ID (예: "fp_01") |
|  + tilePosition: Vector2Int          // Zone F 내 타일 좌표    |
|  + availableFish: FishData[]         // 이 포인트에서 잡히는 어종 |
|  + rarityWeights: float[]            // [Common, Uncommon, Rare, Legendary] 가중치 |
|       // -> see docs/systems/fishing-system.md (canonical)    |
|                                                              |
|  [상태]                                                       |
|  - _isOccupied: bool                 // 누군가 낚시 중인지     |
|  - _dailyUseCount: int               // 오늘 사용 횟수         |
|                                                              |
|  [읽기 전용 프로퍼티]                                           |
|  + IsAvailable: bool                 // !_isOccupied && 일일 제한 미초과 |
|                                                              |
|  [메서드]                                                     |
|  + CanFish(): bool                                           |
|  + SetOccupied(bool occupied): void                          |
|  + ResetDailyCount(): void           // OnDayChanged에서 호출 |
+--------------------------------------------------------------+
```

**Zone F 내 FishingPoint 배치**: Zone F의 연못 타일(30타일) 중 3개 지점에 FishingPoint를 배치한다 `(-> see docs/systems/farm-expansion.md 섹션 4.3)`. 각 포인트는 연못 가장자리 타일에 위치하며, 플레이어가 인접 타일에서 인터랙션한다.

---

# Part II -- 핵심 시스템 연동

---

## 5. 인벤토리 연동 (InventoryManager)

### 5.1 낚시 성공 시 아이템 추가 흐름

```
FishingManager.CompleteFishing(success: true)
    |
    +-- 1) 품질 결정
    |       quality = _minigame.CalculateQuality()
    |       // 품질 기준: excitementGauge 최종값 기반
    |       // qualityThresholds -> see docs/systems/fishing-system.md (canonical)
    |
    +-- 2) 인벤토리 추가
    |       bool added = InventoryManager.Instance.TryAddItem(
    |           itemId: fish.dataId,        // 예: "fish_carp"
    |           quantity: 1,
    |           quality: quality,            // Normal/Silver/Gold/Iridium
    |           origin: HarvestOrigin.Fishing  // [신규] 낚시 출처
    |       )
    |       if (!added):
    |           // 배낭 가득 -> 바닥에 드랍 또는 경고 UI
    |           FishingEvents.OnInventoryFull?.Invoke(fish)
    |
    +-- 3) XP 부여
    |       int xp = ProgressionManager.Instance.GetExpForSource(
    |           XPSource.FishingCatch,
    |           context: (fish, quality)     // Tuple<FishData, CropQuality>
    |       )
    |       ProgressionManager.Instance.AddExp(xp, XPSource.FishingCatch)
    |
    +-- 4) 통계 갱신
    |       _fishingStats.totalCaught++
    |       _fishingStats.caughtByFishId[fish.dataId]++
    |       if (fish.rarity >= FishRarity.Rare) _fishingStats.rareFishCaught++
    |       _fishingStats.currentStreak++
    |       _fishingStats.maxStreak = Max(maxStreak, currentStreak)
    |
    +-- 5) 이벤트 발행
            FishingEvents.OnFishCaught?.Invoke(fish, quality)
            // 구독자: AchievementManager, QuestManager 등
```

### 5.2 HarvestOrigin.Fishing 추가

**canonical 정의 위치**: `docs/systems/economy-architecture.md` 섹션 3.10.2

현재 HarvestOrigin enum:
```csharp
// illustrative — 현재 상태 (→ see docs/systems/economy-architecture.md 섹션 3.10.2)
public enum HarvestOrigin
{
    Outdoor    = 0,
    Greenhouse = 1,
    Barn       = 2
    // [OPEN] Cave = 3, Planter = 4 등 향후 확장
}
```

**필요 변경**:
```csharp
// illustrative — 낚시 시스템 추가 후
public enum HarvestOrigin
{
    Outdoor    = 0,
    Greenhouse = 1,
    Barn       = 2,
    Fishing    = 3    // [신규] 낚시로 획득한 물고기 (DES-013)
    // [OPEN] Cave = 4, Planter = 5 등 향후 확장
}
```

[RISK] `HarvestOrigin.Fishing` 추가 시 `economy-architecture.md` 섹션 3.10.3의 `GetGreenhouseMultiplier()` switch 문에 `Fishing` case를 추가해야 한다. 물고기는 온실 보정 대상이 아니므로 `return 1.0`을 반환한다.

**FIX 태스크**: `economy-architecture.md` 섹션 3.10.2 HarvestOrigin enum에 `Fishing = 3` 추가, 섹션 3.10.3 switch 문에 `case HarvestOrigin.Fishing: return 1.0;` 추가 (FIX-049 권고).

### 5.3 GetGreenhouseMultiplier() 확장 (필요 변경 사항)

```
// economy-architecture.md 섹션 3.10.3에 추가할 case:
Pseudocode: GetGreenhouseMultiplier(HarvestOrigin origin, ...)

    if origin == HarvestOrigin.Fishing:
        return 1.0  // 물고기는 온실 보정 적용 대상 외
    // ... 기존 로직 유지
```

---

## 6. 진행 시스템 연동 (ProgressionManager)

### 6.1 XPSource.FishingCatch 추가

**canonical 정의 위치**: `docs/systems/progression-architecture.md` 섹션 2.2

현재 XPSource enum의 마지막 값은 `AnimalHarvest`이다. 낚시 시스템 추가로 다음 값이 필요하다:

```csharp
// illustrative — 추가할 값 (→ see docs/systems/progression-architecture.md 섹션 2.2)
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
    FishingCatch        // [신규] 물고기 잡기 (DES-013)
}
```

### 6.2 GetExpForSource() 확장

`progression-architecture.md` 섹션 2.3의 switch 문에 다음 case를 추가해야 한다:

```csharp
// illustrative — progression-architecture.md 섹션 2.3에 추가
case XPSource.FishingCatch:
    // 낚시 XP는 FishData.expReward 기반, 품질 보정 적용
    // -> see docs/balance/progression-curve.md for canonical XP 값
    var (fishData, fishQuality) = ((FishData, CropQuality))context;
    return CalculateFishingExp(fishData, fishQuality);
```

**FIX 태스크**: `progression-architecture.md` 섹션 2.2 XPSource enum에 `FishingCatch` 추가, 섹션 2.3 `GetExpForSource()` switch 문에 `FishingCatch` case 추가, `CalculateFishingExp()` 메서드 정의 추가 (FIX-050 권고).

[OPEN] 낚시 XP 계산 공식이 미정. 기본 공식 후보: `fishData.expReward * qualityExpBonus[quality]`. 품질 보정 배율은 작물 수확과 동일한 테이블 사용 가능 `(-> see docs/balance/progression-curve.md)`.

### 6.3 이벤트 구독 확장

ProgressionManager의 `OnEnable()`에 낚시 이벤트 구독을 추가해야 한다:

```csharp
// illustrative — progression-architecture.md OnEnable()에 추가
FishingEvents.OnFishCaught += HandleFishCaught;
```

---

## 7. 경제 시스템 연동 (EconomyManager)

### 7.1 물고기 판매 흐름

물고기 판매는 기존 `ShopSystem.TrySellCrop()` 또는 출하함을 통해 이루어진다. `HarvestOrigin.Fishing`을 전달하면 `GetGreenhouseMultiplier()`에서 1.0을 반환하므로 온실 보정 없이 기본 가격 로직이 적용된다.

```
플레이어 -> 상점/출하함에 물고기 넣기
    |
    +-- ShopSystem.TrySellCrop(fishData, qty, quality, origin: Fishing)
    |       |
    |       +-- EconomyManager.GetSellPrice(fishData, quality, HarvestOrigin.Fishing)
    |       |       |
    |       |       +-- base = fishData.basePrice
    |       |       |       // -> see docs/systems/fishing-system.md (canonical)
    |       |       +-- seasonMul = PriceFluctuationSystem 계절 보정
    |       |       +-- supplyMul = 수급 보정
    |       |       +-- weatherMul = 날씨 보정
    |       |       +-- qualityMul = 품질 보정
    |       |       +-- originMul = GetGreenhouseMultiplier(Fishing, ...) = 1.0
    |       |       +-- finalPrice = base * seasonMul * supplyMul * weatherMul * qualityMul * originMul
    |       |       +-- ClampPrice()
    |       |
    |       +-- InventoryManager.RemoveItem(fishData.dataId, qty, quality, origin: Fishing)
    |       +-- EconomyManager.AddGold(totalGold, "sell_fish")
    |       +-- TransactionLog.AddEntry(tx)
```

[OPEN] 물고기 전용 가격 변동 시스템이 필요한지 검토. 현재 설계에서는 작물과 동일한 PriceFluctuationSystem을 공유하나, 물고기만의 수급/계절 보정이 필요할 수 있다. 초기 버전에서는 기존 시스템 공유로 시작하고, 밸런스 테스트 후 분리 여부를 결정한다.

### 7.2 물고기 PriceData SO

물고기 어종별로 PriceData SO가 필요하다. 기존 작물과 동일한 구조를 사용하되, `HarvestOrigin.Fishing` 카테고리로 구분한다.

```
PriceData_Fish_Carp (SO)
    |-- basePrice: (-> see docs/systems/fishing-system.md for canonical)
    |-- category: "fish"
    |-- seasonalMultipliers: (-> see docs/systems/fishing-system.md for canonical)
```

---

## 8. Zone F 타일과 FishingPoint 등록

### 8.1 FishingPoint 배치 방식

Zone F의 연못 타일(30타일) 중 가장자리 3개 타일에 FishingPoint를 배치한다. FishingManager는 FarmZoneManager에서 Zone F의 활성화 여부를 확인한 후, FishingPoint 목록을 초기화한다.

```
FarmZoneManager.TryUnlockZone("zone_pond")
    |
    +-- Zone F 타일 활성화 (FarmGrid.ActivateZoneTiles)
    |
    +-- FishingManager.OnZoneUnlocked("zone_pond") 이벤트 수신
            |
            +-- FishingPoint[] points = FindFishingPointsInZone("zone_pond")
            +-- foreach point: point.Initialize()
```

### 8.2 ZoneType.Pond 활용

`FarmZoneManager.GetZoneForTile(pos)` 호출 시 반환되는 ZoneData의 `zoneType`이 `Pond`이면, 해당 타일에서 낚싯대 사용이 가능함을 판별한다 `(-> see docs/systems/farm-expansion-architecture.md 섹션 2.2)`.

---

# Part III -- 데이터 구조

---

## 9. FishData SO JSON 예시

```json
{
    "dataId": "fish_carp",
    "displayName": "잉어",
    "description": "연못에서 흔히 잡히는 민물고기. 사계절 잡을 수 있다.",
    "fishId": "fish_carp",
    "rarity": "Common",
    "basePrice": 0,
    "seasonAvailability": 15,
    "timeWeights": [0.8, 1.0, 1.2, 0.8, 0.5],
    "weatherBonus": 2,
    "minigameDifficulty": 0.2,
    "targetZoneWidthMul": 1.2,
    "moveSpeed": 1.0,
    "maxStackSize": 10,
    "expReward": 0,
    "itemType": "Fish",
    "sellable": true
}
```

**필드 설명**:
- `basePrice`: `0`으로 표기 -- canonical 값은 `(-> see docs/systems/fishing-system.md)`. 아직 미확정이므로 플레이스홀더.
- `seasonAvailability`: `15` = `0b1111` (Spring|Summer|Fall|Winter) -- 사계절 출현 `(-> see docs/systems/time-season.md for SeasonFlag)`
- `timeWeights`: 5개 시간대 가중치 [Dawn, Morning, Afternoon, Evening, Night] `(-> see docs/systems/time-season.md 섹션 1.2 for 시간대 정의)`
- `weatherBonus`: `2` = `WeatherFlag.Rain` -- 비 올 때 출현율 증가
- `expReward`: `0`으로 표기 -- canonical 값은 `(-> see docs/balance/progression-curve.md)`. 아직 미확정이므로 플레이스홀더.

[OPEN] basePrice, expReward 등 밸런스 수치는 fishing-system.md (디자인 문서) 작성 시 확정 필요.

## 10. FishingSaveData C# 클래스

```csharp
// illustrative
namespace SeedMind.Fishing
{
    [System.Serializable]
    public class FishingSaveData
    {
        // 통계
        public int totalCasts;                              // 총 캐스팅 횟수
        public int totalCaught;                             // 총 잡은 횟수
        public int totalFailed;                             // 총 실패 횟수
        public Dictionary<string, int> caughtByFishId;      // 어종별 잡은 횟수
        public int rareFishCaught;                          // 희귀 이상 물고기 잡은 횟수
        public int maxStreak;                               // 최대 연속 성공
        public int currentStreak;                           // 현재 연속 성공
    }
}
```

### 10.1 JSON <-> C# 필드 동기화 검증 (PATTERN-005)

| JSON 필드 | C# 필드 | 타입 | 비고 |
|-----------|---------|------|------|
| `totalCasts` | `totalCasts` | `int` | 일치 |
| `totalCaught` | `totalCaught` | `int` | 일치 |
| `totalFailed` | `totalFailed` | `int` | 일치 |
| `caughtByFishId` | `caughtByFishId` | `Dictionary<string, int>` | 일치 |
| `rareFishCaught` | `rareFishCaught` | `int` | 일치 |
| `maxStreak` | `maxStreak` | `int` | 일치 |
| `currentStreak` | `currentStreak` | `int` | 일치 |

**FishingSaveData JSON 예시**:

```json
{
    "totalCasts": 47,
    "totalCaught": 32,
    "totalFailed": 15,
    "caughtByFishId": {
        "fish_carp": 12,
        "fish_trout": 8,
        "fish_catfish": 7,
        "fish_golden_carp": 5
    },
    "rareFishCaught": 3,
    "maxStreak": 8,
    "currentStreak": 2
}
```

**필드 수**: JSON 7개, C# 7개 -- 동기화 완료.

---

# Part IV -- 미니게임 상태 머신

---

## 11. FishingState enum

```csharp
// illustrative
namespace SeedMind.Fishing
{
    public enum FishingState
    {
        Idle,       // 대기 -- 낚시를 하고 있지 않음
        Casting,    // 캐스팅 -- 낚싯대를 던지는 애니메이션 재생 중
        Waiting,    // 대기 -- 찌가 물에 떠 있고, 물고기가 물기를 기다리는 중
        Biting,     // 입질 -- 물고기가 미끼를 물었다! 플레이어 입력 대기 (제한 시간)
        Reeling,    // 감기 -- 미니게임 진행 중 (ExcitementGauge)
        Success,    // 성공 -- 물고기를 잡았다!
        Fail        // 실패 -- 물고기를 놓쳤다
    }
}
```

### 11.1 상태 전환 다이어그램

```
                  TryStartFishing(point)
                  [point.CanFish() == true]
    [Idle] ─────────────────────────────> [Casting]
      ^                                       |
      |                                       | 캐스팅 애니메이션 완료
      |                                       | (castDuration 경과)
      |                                       v
      |                                  [Waiting]
      |                                       |
      |        CancelFishing()                | CalculateBiteDelay() 경과
      |  <────────────────────────────────    | (랜덤 대기 시간)
      |                                       v
      |                                   [Biting]
      |                                    /     \
      |              biteWindowDuration   /       \ 플레이어 입력
      |              경과 (시간 초과)     /         \ (클릭/탭)
      |                                v           v
      |                            [Fail]      [Reeling]
      |                              |             |
      |                              |             | FishingMinigame.Update()
      |                              |             | 매 프레임 호출
      |                              |             |
      |                              |        MinigameResult?
      |                              |         /         \
      |                              |   Success         Fail
      |                              |     v               v
      |                              | [Success]        [Fail]
      |                              |     |               |
      |    자동 전환 (1~2초 후)        |     |               |
      +<─────────────────────────────+─────+───────────────+
```

### 11.2 상태별 전환 조건 상세

| 현재 상태 | 전환 조건 | 다음 상태 | 부수 효과 |
|-----------|----------|----------|----------|
| **Idle** | `TryStartFishing(point)` 호출, `point.CanFish() == true`, 낚싯대 소지 | Casting | `point.SetOccupied(true)`, 에너지 소모 `(-> see docs/systems/farming-system.md 섹션 3.2)` |
| **Casting** | `castDuration` 경과 (FishingConfig) | Waiting | 캐스팅 VFX 재생 |
| **Waiting** | `CalculateBiteDelay()` 반환 시간 경과 (랜덤) | Biting | 찌 흔들림 VFX, SFX "딸깍" |
| **Waiting** | `CancelFishing()` 호출 | Idle | `point.SetOccupied(false)`, 에너지 소모 없음 (이미 캐스팅에서 차감) |
| **Biting** | 플레이어 입력 (클릭/탭) within `biteWindowDuration` | Reeling | `SelectRandomFish(point)` 호출, `_minigame.Start(fish, config)` |
| **Biting** | `biteWindowDuration` 초과 (입력 없음) | Fail | "물고기가 도망갔다!" 메시지, `_fishingStats.totalFailed++` |
| **Reeling** | `_minigame.Update()` -> `MinigameResult.Success` | Success | `CompleteFishing(true)` -> `GrantRewards()` |
| **Reeling** | `_minigame.Update()` -> `MinigameResult.Fail` | Fail | `CompleteFishing(false)`, `_fishingStats.currentStreak = 0` |
| **Success** | 자동 전환 (보상 연출 후, 약 1~2초) | Idle | `point.SetOccupied(false)`, 통계/이벤트 처리 완료 |
| **Fail** | 자동 전환 (실패 연출 후, 약 1초) | Idle | `point.SetOccupied(false)`, `_fishingStats.totalFailed++` |

### 11.3 물고기 선택 알고리즘

```
SelectRandomFish(FishingPoint point):
    1) point.availableFish 중 현재 계절에 출현 가능한 어종 필터링
       -> fish.seasonAvailability & currentSeasonFlag != 0
       // -> see docs/systems/time-season.md for SeasonFlag

    2) 시간대 가중치 적용
       -> fish.timeWeights[currentTimeSlot] 값을 기본 가중치로 사용
       // -> see docs/systems/time-season.md 섹션 1.2 for 시간대 정의

    3) 날씨 보너스 적용
       -> if (currentWeather & fish.weatherBonus != 0): weight *= 1.5
       // 보너스 배율 -> see docs/systems/fishing-system.md (canonical)

    4) 희귀도 가중치 곱산
       -> weight *= point.rarityWeights[(int)fish.rarity]

    5) 가중치 합산 후 랜덤 선택 (WeightedRandom)
       -> return selectedFish
```

[OPEN] 날씨 보너스 배율(1.5)이 잠정값이다. fishing-system.md에서 canonical 값을 확정해야 한다.

---

# Part V -- 세이브/로드 통합

---

## 12. GameSaveData 확장

### 12.1 fishing 필드 추가 위치

**canonical 정의 위치**: `docs/pipeline/data-pipeline.md` Part I 섹션 2.1 (GameSaveData)

현재 `GameSaveData`의 마지막 필드는 `tutorial: TutorialSaveData`이다. 낚시 시스템 추가로 다음 필드가 필요하다:

```csharp
// illustrative — data-pipeline.md 섹션 2.1 GameSaveData에 추가
public class GameSaveData
{
    // ... 기존 필드 유지 ...
    public TutorialSaveData tutorial;         // 기존 마지막 필드

    // DES-013: 낚시 시스템 추가
    public FishingSaveData fishing;            // 낚시 통계 (null 허용 -- 구버전 세이브 호환)
        // -> see docs/systems/fishing-architecture.md 섹션 10
}
```

**FIX 태스크**: `data-pipeline.md` Part I 섹션 2.1 GameSaveData 클래스에 `public FishingSaveData fishing;` 필드 추가, null 허용 주석 추가 (FIX-051 권고).

### 12.2 SaveLoadOrder 할당

| 시스템 | SaveLoadOrder | 근거 |
|--------|:------------:|------|
| FishingManager | **52** | PlayerController(50) 이후, InventoryManager(55) 이전. 낚시 상태 복원은 플레이어 위치 복원 후, 인벤토리 복원 전에 이루어져야 한다. FishingManager는 인벤토리에 의존하지 않으며(통계만 복원), 인벤토리가 물고기 아이템을 복원할 때 FishData SO가 이미 레지스트리에 있어야 하므로 52가 적절하다. |

(-> see `docs/systems/save-load-architecture.md` 섹션 7 for 전체 할당표)

**FIX 태스크**: `save-load-architecture.md` 섹션 7 SaveLoadOrder 할당표에 `FishingManager | 52 | 낚시 통계 복원` 행 추가 (FIX-052 권고).

### 12.3 세이브/로드 흐름

**저장**:
```
SaveManager.SaveAsync()
    |
    +-- ... [10] TimeManager, [20] WeatherSystem, [30] EconomyManager, [40] FarmGrid, [45] FarmZoneManager, [48] AnimalManager, [50] PlayerController ...
    +-- [52] FishingManager.GetSaveData()
    |       -> return new FishingSaveData {
    |           totalCasts = _fishingStats.totalCasts,
    |           totalCaught = _fishingStats.totalCaught,
    |           totalFailed = _fishingStats.totalFailed,
    |           caughtByFishId = new Dictionary<string, int>(_fishingStats.caughtByFishId),
    |           rareFishCaught = _fishingStats.rareFishCaught,
    |           maxStreak = _fishingStats.maxStreak,
    |           currentStreak = _fishingStats.currentStreak
    |       }
    +-- [55] InventoryManager ...
    +-- ... 나머지 시스템 ...
```

**로드**:
```
SaveManager.LoadAsync()
    |
    +-- ... [10]~[50] 이전 시스템 복원 ...
    +-- [52] FishingManager.LoadSaveData(data.fishing)
    |       if (data.fishing == null):
    |           // 구버전 세이브 -- 초기값으로 세팅
    |           _fishingStats = new FishingStats()
    |       else:
    |           _fishingStats.totalCasts = data.fishing.totalCasts
    |           _fishingStats.totalCaught = data.fishing.totalCaught
    |           ... (나머지 필드 복원)
    +-- [55] InventoryManager ...
```

---

# Part VI -- MCP 태스크 요약

---

## 13. MCP 구현 태스크 시퀀스

> 상세 태스크는 `docs/mcp/fishing-tasks.md`에서 별도 작성 예정

### Phase A: 데이터 레이어 (SO 생성)

| Step | MCP 명령 | 설명 |
|------|---------|------|
| A-1 | CreateScript `FishRarity.cs` | enum 정의 (SeedMind.Fishing) |
| A-2 | CreateScript `FishData.cs` : ScriptableObject | GameDataSO 상속, IInventoryItem 구현 |
| A-3 | CreateScript `FishingConfig.cs` : ScriptableObject | 미니게임 밸런스 파라미터 |
| A-4 | CreateScript `FishingState.cs` | enum 정의 |
| A-5 | CreateScript `MinigameResult.cs` | enum 정의 |
| A-6 | CreateScript `FishingStats.cs` | Plain C# 통계 클래스 |
| A-7 | CreateScript `FishingSaveData.cs` | 직렬화 가능 클래스 |
| A-8 | CreateScript `FishingEvents.cs` | 정적 이벤트 허브 |

### Phase B: 시스템 레이어 (Manager + Minigame)

| Step | MCP 명령 | 설명 |
|------|---------|------|
| B-1 | CreateScript `FishingMinigame.cs` | ExcitementGauge 로직 |
| B-2 | CreateScript `FishingPoint.cs` : MonoBehaviour | 낚시 포인트 컴포넌트 |
| B-3 | CreateScript `FishingManager.cs` : MonoBehaviour | 싱글턴, ISaveable 구현 |

### Phase C: SO 에셋 생성

| Step | MCP 명령 | 설명 |
|------|---------|------|
| C-1 | CreateAsset `FishingConfig_Default` | FishingConfig SO 에셋 |
| C-2 | CreateAsset `FishData_Carp` 등 | 어종별 FishData SO 에셋 `(-> see docs/systems/fishing-system.md for 어종 목록)` |
| C-3 | CreateAsset `PriceData_Fish_*` | 어종별 가격 SO |

### Phase D: 씬 배치

| Step | MCP 명령 | 설명 |
|------|---------|------|
| D-1 | CreateGameObject `FishingManager` | 싱글턴 오브젝트, DontDestroyOnLoad |
| D-2 | Zone F 연못 가장자리에 FishingPoint x3 배치 | `tilePosition` 설정 |
| D-3 | FishingManager Inspector에 FishData[], FishingConfig, FishingPoint[] 할당 | |

### Phase E: 기존 시스템 확장

| Step | MCP 명령 | 설명 |
|------|---------|------|
| E-1 | EditScript `HarvestOrigin.cs` | `Fishing = 3` 추가 |
| E-2 | EditScript `XPSource.cs` | `FishingCatch` 추가 |
| E-3 | EditScript `ProgressionManager.cs` | `GetExpForSource()` switch 문에 FishingCatch case 추가, `FishingEvents.OnFishCaught` 구독 추가 |
| E-4 | EditScript `ItemType.cs` | `Fish` 값 추가 |
| E-5 | EditScript `GameSaveData.cs` | `public FishingSaveData fishing;` 추가 |
| E-6 | EditScript `SaveManager.cs` | FishingManager ISaveable 등록 확인 |
| E-7 | EditScript `GetGreenhouseMultiplier()` | `HarvestOrigin.Fishing` case 추가 (`return 1.0`) |

### Phase F: 검증

| Step | 검증 내용 |
|------|----------|
| F-1 | FishingManager 싱글턴 정상 생성 확인 (MCP Console.Log) |
| F-2 | Zone F 해금 -> FishingPoint 활성화 확인 |
| F-3 | 낚시 상태 머신 Idle -> Casting -> Waiting -> Biting -> Reeling -> Success/Fail 전환 확인 |
| F-4 | 물고기 인벤토리 추가 확인 (AddItem 호출, HarvestOrigin.Fishing) |
| F-5 | XP 부여 확인 (XPSource.FishingCatch) |
| F-6 | 세이브 -> 로드 후 FishingStats 복원 확인 |

---

## Cross-references

| 참조 문서 | 참조 위치 | 관계 |
|-----------|----------|------|
| `docs/systems/farm-expansion.md` (DES-012) | 섹션 4.3 Zone F | Zone F 연못 구역 구조, 낚시 포인트 배치 |
| `docs/systems/farm-expansion-architecture.md` (ARC-023) | 섹션 2.2 ZoneType | ZoneType.Pond 정의 |
| `docs/systems/inventory-architecture.md` | 섹션 5.1 AddItem | 물고기 아이템 추가 흐름 |
| `docs/systems/economy-architecture.md` | 섹션 3.10.2 HarvestOrigin | HarvestOrigin.Fishing 추가 필요 |
| `docs/systems/economy-architecture.md` | 섹션 3.10.3 GetGreenhouseMultiplier | Fishing case 추가 필요 |
| `docs/systems/progression-architecture.md` | 섹션 2.2 XPSource | XPSource.FishingCatch 추가 필요 |
| `docs/systems/progression-architecture.md` | 섹션 2.3 GetExpForSource | FishingCatch case 추가 필요 |
| `docs/systems/save-load-architecture.md` (ARC-011) | 섹션 7 SaveLoadOrder | FishingManager SaveLoadOrder 52 추가 필요 |
| `docs/pipeline/data-pipeline.md` | Part I 섹션 2.1 GameSaveData | FishingSaveData 필드 추가 필요 |
| `docs/systems/time-season.md` | 섹션 1.2 시간대, SeasonFlag | 어종 출현 조건 |
| `docs/systems/project-structure.md` | 네임스페이스 규칙 | SeedMind.Fishing 배치 |
| `docs/systems/fishing-system.md` (DES-013) | 어종 목록, 가격, 밸런스 | FishData canonical 수치 |
| `docs/balance/progression-curve.md` | 낚시 XP 값 | FishingCatch XP canonical — FIX-050 완료 후 등록 예정 |

---

## Open Questions

1. [RESOLVED] `docs/systems/fishing-system.md` (DES-013) 작성 완료. 어종 목록 15종, 기본 판매가, 희귀도별 출현 가중치, 미니게임 난이도 파라미터 확정. basePrice 및 expReward 등 일부 밸런스 수치는 `docs/balance/progression-curve.md` canonical 등록 후 FishData SO에 반영 필요.
2. [OPEN] 낚시 XP 계산 공식 미확정. 작물 수확 XP 공식(`harvestExpBase + floor(growthDays * harvestExpPerGrowthDay) * qualityBonus`)을 물고기에 어떻게 적용할지 (rarity 기반? basePrice 기반?) 결정 필요.
3. [RESOLVED-FIX-053] `ItemType` enum에 `Fish` 값 추가 완료. data-pipeline.md, inventory-architecture.md 섹션 3.2, inventory-tasks.md Step 1-01에 반영.
4. [OPEN] 물고기 전용 가격 변동 시스템 분리 여부. 초기 버전은 기존 PriceFluctuationSystem 공유, 추후 분리 검토.
5. [OPEN] 낚시 에너지 소모량 미확정. 캐스팅 1회당 에너지 소모를 `(-> see docs/systems/farming-system.md 섹션 3.2)` 기존 도구 사용 에너지와 동일 레벨로 할지 별도 설정할지 결정 필요.
6. [OPEN] 날씨 보너스 배율(잠정 1.5)이 미확정. fishing-system.md에서 canonical 값을 설정해야 한다.
7. [OPEN] FishingPoint 일일 사용 제한 횟수 미확정. 무제한으로 할지, 포인트당 일일 제한을 둘지 결정 필요.
8. [OPEN] 낚싯대 도구 요건 미확정. 기존 ToolType enum에 FishingRod를 추가해야 하며, 도구 업그레이드 경로도 설계 필요.

---

## Risks

1. [RESOLVED-FIX-049] **HarvestOrigin enum 확장 파급**: `Fishing = 3`이 economy-architecture.md 섹션 3.10.2에 추가되었고, `GetGreenhouseMultiplier()` pseudocode(섹션 3.10.3)에도 Fishing case가 반영되었다.
2. [RESOLVED-FIX-050/부분] **XPSource enum 확장 파급**: `FishingCatch`가 progression-architecture.md 섹션 2.2 XPSource enum 및 섹션 2.3 `GetExpForSource()` switch 문에 추가되었다. 단, ProgressionData SO의 `fishingCatchExp` 필드는 FishData.expReward 기반 계산으로 대체되므로 별도 필드 불필요 — progression-architecture.md 섹션 2.1 클래스 정의에 주석으로 명시됨.
3. [RISK] **SaveLoadOrder 52 충돌 가능성**: PlayerController(50)와 InventoryManager(55) 사이의 좁은 간격에 배치. 향후 다른 시스템이 51~54 범위를 사용하면 충돌 가능. save-load-architecture.md 할당표를 반드시 갱신해야 한다.
4. [RISK] **FishData SO와 IInventoryItem 통합**: FishData가 GameDataSO를 상속하고 IInventoryItem을 구현해야 하므로, 기존 IInventoryItem 인터페이스에 Fish 타입이 누락되면 런타임 오류 발생.
5. [RISK] **미니게임 프레임 의존성**: FishingMinigame.Update()가 매 프레임 호출되므로, deltaTime 기반 보정이 없으면 프레임레이트에 따라 난이도가 달라질 수 있다. 모든 시간 계산에 deltaTime을 반드시 적용해야 한다.

---

## FIX 태스크 제안 (TODO.md 추가 권고)

> **주의**: FIX-044는 TODO.md에 economy-architecture.md 동물 수급 변동 정책 건으로 이미 배정됨. 낚시 시스템 관련 FIX는 FIX-049부터 시작.

| FIX ID | 대상 문서 | 변경 내용 | 상태 |
|--------|----------|----------|:----:|
| FIX-049 | `economy-architecture.md` | 섹션 3.10.2 HarvestOrigin에 `Fishing = 3` 추가, 섹션 3.10.3 switch 문에 Fishing case 추가 | RESOLVED |
| FIX-050 | `progression-architecture.md` | 섹션 2.2 XPSource에 `FishingCatch` 추가, 섹션 2.3 `GetExpForSource()` switch 문에 FishingCatch case 추가 | RESOLVED |
| FIX-051 | `data-pipeline.md` | Part I 섹션 3.2 JSON 스키마 및 Part II GameSaveData C# 클래스에 `fishing` 필드 추가 | RESOLVED |
| FIX-052 | `save-load-architecture.md` | 섹션 7 SaveLoadOrder 할당표에 `FishingManager \| 52` 추가 | RESOLVED |
| FIX-053 | `pipeline/data-pipeline.md`, `inventory-architecture.md`, `inventory-tasks.md` | ItemType enum에 `Fish` 값 추가 | RESOLVED |
