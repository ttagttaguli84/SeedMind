# 낚시 시스템 기술 아키텍처

> FishingManager, FishData SO, FishingMinigame 상태 머신, FishingProficiency 숙련도, FishCatalogManager 도감, 인벤토리/경제/진행 시스템 연동, 세이브/로드 통합, MCP 구현 계획
> 작성: Claude Code (Opus) | 2026-04-07
> 문서 ID: ARC-026 (숙련도 확장: ARC-029, FIX-069 | 도감 확장: ARC-030)

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
|  - _proficiency: FishingProficiency  // 숙련도 추적 (Plain C#)  |
|                                                              |
|  [설정 참조]                                                   |
|  - _fishingConfig: FishingConfig (ScriptableObject)           |
|  - _minigame: FishingMinigame        // 미니게임 로직 (내부 인스턴스) |
|                                                              |
|  [읽기 전용 프로퍼티]                                           |
|  + CurrentState: FishingState                                |
|  + ActiveFish: FishData                                      |
|  + FishingStats: FishingStats (읽기 전용)                     |
|  + Proficiency: FishingProficiency (읽기 전용)                |
|  + ProficiencyLevel: int             // _proficiency.Level    |
|  + IsPlayerFishing: bool             // Idle이 아닌 모든 상태  |
|                                                              |
|  [이벤트]                                                     |
|  + OnStateChanged: Action<FishingState, FishingState>        |
|         // oldState, newState                                |
|  + OnFishCast: Action<FishingPoint>                          |
|         // 낚싯줄 던짐 — SoundEventBridge 구독 (sfx_cast_line) |
|  + OnFishBite: Action<FishData>                              |
|         // 물고기 입질 — SoundEventBridge 구독 (sfx_fish_bite) |
|  + OnFishCaught: Action<FishData, CropQuality>               |
|         // 잡은 물고기, 품질                                   |
|  + OnFishingFailed: Action                                   |
|  + OnProficiencyLevelUp: Action<int>  // newLevel (ARC-029)  |
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
|  [얼음 낚시 — FIX-071]                                        |
|  - _activeIceHoles: List<IceHoleData>  // 현재 활성 얼음 구멍  |
|                                                              |
|  + CreateIceHole(Vector2Int tilePos): bool                   |
|       // 곡괭이 클릭 시 호출. 겨울 아닌 계절이면 false 반환     |
|       // _activeIceHoles.Count >= iceHoleMax이면 false        |
|       // 에너지 소모 (-> see docs/systems/fishing-system.md 섹션 10) |
|  + GetActiveHoleCount(): int                                 |
|  + RemoveExpiredHoles(): void                                |
|       // OnDayChanged에서 호출. createdDay + iceHoleDuration  |
|       // 초과 시 제거. 눈(Snow) 날씨면 즉시 재결빙             |
|  + RemoveAllIceHoles(): void                                 |
|       // OnSeasonChanged에서 호출. 겨울->봄 전환 시 전체 제거   |
|  + IsIceHole(Vector2Int tilePos): bool                       |
|       // 해당 타일에 활성 얼음 구멍이 있는지 확인               |
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
| (아래 섹션 4 참조)          |  | iceHoleDuration: int      |
+----------------------------+  |   // (-> see fishing-     |
                                |   //  system.md 섹션 10)  |
                                | iceHoleMax: int            |
                                |   // (-> see fishing-     |
                                |   //  system.md 섹션 10)  |
                                | iceHoleEnergy: int         |
                                |   // (-> see fishing-     |
                                |   //  system.md 섹션 10)  |
                                +---------------------------+

+----------------------------+
| FishingProficiency         |
| (Plain C# class)           |
| (아래 섹션 4A 참조)         |
+----------------------------+

+----------------------------+
| FishCatalogManager         |
| (MonoBehaviour, Singleton) |
| (Part VII 섹션 16 참조)    |
| — ARC-030                  |
+----------------------------+
         | reads
         v
+----------------------------+
| FishCatalogData (SO)       |
| (Part VII 섹션 15 참조)    |
| — ARC-030                  |
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

+----------------------------+
| IceHoleData (Plain C#)     |
| — FIX-071                  |
|----------------------------|
| tilePosition: Vector2Int   |
| createdDay: int            |
|   // 생성 시점의 dayOfSeason|
| createdSeason: Season      |
|   // 항상 Winter여야 함     |
+----------------------------+
```

### 1.1 네임스페이스 배치

```
Assets/_Project/Scripts/Fishing/     # SeedMind.Fishing 네임스페이스
├── FishingManager.cs                 # 싱글턴, 상태 관리, 인터랙션 진입점
├── FishingMinigame.cs                # 미니게임 로직 (ExcitementGauge 방식)
├── FishingPoint.cs                   # 낚시 포인트 컴포넌트 (씬 배치)
├── FishingProficiency.cs             # 숙련도 추적 (Plain C#, ARC-029)
├── FishingEvents.cs                  # 정적 이벤트 허브
├── FishingState.cs                   # enum 정의
├── FishingStats.cs                   # 통계 데이터 클래스
├── FishingSaveData.cs                # 세이브 데이터 클래스
├── IceHoleData.cs                    # 얼음 구멍 데이터 (Plain C#, FIX-071)
└── Catalog/                          # SeedMind.Fishing.Catalog 네임스페이스 (ARC-030)
    ├── FishCatalogData.cs            # 도감 SO (어종 도감 정적 데이터)
    ├── FishCatalogEntry.cs           # 런타임 상태 클래스
    ├── FishCatalogManager.cs         # 도감 매니저 (Singleton, ISaveable)
    ├── FishCatalogSaveData.cs        # 세이브 데이터 클래스
    └── UI/
        ├── FishCatalogUI.cs          # 도감 메인 패널
        ├── FishCatalogItemUI.cs      # 도감 항목 프리팹
        ├── FishCatalogDetailPanel.cs # 상세 정보 패널
        └── FishCatalogToastUI.cs     # 발견 토스트 알림
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
|       // -> see docs/pipeline/data-pipeline.md 섹션 2.7        |
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

**Zone F 내 FishingPoint 배치**: Zone F의 연못 타일(30타일) 중 3개 지점에 FishingPoint MonoBehaviour 오브젝트를 배치한다 `(-> see docs/systems/farm-expansion.md 섹션 4.3)`. 각 FishingPoint는 연못 가장자리의 특정 구역을 관할하며, 해당 구역 내 인접 육지 타일(약 20개소 중 일부)에서 플레이어가 인터랙션하면 가장 가까운 FishingPoint에 위임된다. 즉 물리적 낚시 가능 위치(약 20개소)와 FishingPoint 오브젝트 수(3개)는 서로 다른 개념이다: FishingPoint는 어종 풀과 희귀도 가중치를 구역 단위로 관리하는 논리적 컨트롤러이다.

---

## 4A. FishingProficiency (Plain C# class) — ARC-029

### 4A.1 설계 개요

FishingProficiency는 낚시 전용 숙련도 XP/레벨을 추적하는 Plain C# 클래스이다. FishingManager가 소유(`_proficiency` 필드)하며, 레벨업 시 해금 처리와 미니게임 파라미터 보정을 담당한다.

**설계 결정**: FishingProficiency를 MonoBehaviour가 아닌 Plain C# class로 설계한 이유는 FishingManager 내부에서만 사용되는 데이터 클래스이며, 독립적인 씬 존재가 불필요하기 때문이다. FishingStats와 동일한 패턴을 따른다.

### 4A.2 클래스 다이어그램

```
+--------------------------------------------------------------+
|            FishingProficiency (Plain C# class)                 |
|--------------------------------------------------------------|
|  [상태]                                                       |
|  - _currentXP: int                   // 누적 낚시 숙련도 XP   |
|  - _currentLevel: int                // 현재 숙련도 레벨 (1~10)|
|  - _config: FishingConfig            // SO 참조 (XP 테이블 등) |
|                                                              |
|  [읽기 전용 프로퍼티]                                           |
|  + CurrentXP: int                                            |
|  + CurrentLevel: int                                         |
|  + XPToNextLevel: int                // 다음 레벨까지 남은 XP  |
|  + IsMaxLevel: bool                  // Level == 10           |
|                                                              |
|  [이벤트]                                                     |
|  + OnLevelUp: Action<int>            // newLevel              |
|       // FishingManager.OnProficiencyLevelUp에 위임           |
|                                                              |
|  [메서드]                                                     |
|  + FishingProficiency(FishingConfig config): constructor      |
|  + AddXP(int amount): void           // XP 추가, 레벨업 판정  |
|  + GetXPForCatch(FishRarity rarity, bool isGiant): int       |
|       // -> see docs/systems/fishing-system.md 섹션 7.3      |
|  + GetBiteDelayMultiplier(): float                           |
|       // -> see docs/systems/fishing-system.md 섹션 7.4      |
|  + GetRarityBonus(): float                                   |
|       // -> see docs/systems/fishing-system.md 섹션 7.4      |
|  + GetTreasureChestBonus(): float                            |
|       // -> see docs/systems/fishing-system.md 섹션 7.4      |
|  + GetMaxFishQuality(): CropQuality                          |
|       // -> see docs/systems/fishing-system.md 섹션 7.2      |
|  + GetDoubleCatchChance(): float                             |
|       // -> see docs/systems/fishing-system.md 섹션 7.4      |
|  + GetEnergyCostReduction(): int                             |
|       // -> see docs/systems/fishing-system.md 섹션 7.2      |
|  + GetMiniGameSuccessRate(): float                           |
|       // -> see docs/systems/fishing-system.md 섹션 7.4      |
|                                                              |
|  [세이브/로드]                                                 |
|  + GetSaveData(): (int xp, int level)                        |
|  + LoadSaveData(int xp, int level): void                     |
+--------------------------------------------------------------+
```

### 4A.3 FishingConfig 확장 (숙련도 설정)

기존 FishingConfig SO에 숙련도 관련 필드를 추가한다.

```
FishingConfig (SO) -- 숙련도 섹션 추가 (ARC-029)
    |
    |-- [기존 필드 유지]
    |-- castDurationRange, biteDelayRange, ...
    |
    |-- [숙련도 설정 -- 신규]
    |-- proficiencyXPThresholds: int[]     // 레벨별 필요 누적 XP
    |       // 길이 10, [0]=0, [1]=50, ..., [9]=2250
    |       // -> see docs/systems/fishing-system.md 섹션 7.2 (canonical)
    |-- proficiencyMaxLevel: int           // 10
    |       // -> see docs/systems/fishing-system.md 섹션 7.2 (canonical)
    |-- catchXPByRarity: int[]             // [Common, Uncommon, Rare, Legendary]
    |       // -> see docs/systems/fishing-system.md 섹션 7.3 (canonical)
    |-- giantXPMultiplier: float           // 2.0
    |       // -> see docs/systems/fishing-system.md 섹션 7.3 (canonical)
    |-- treasureChestXP: int               // 8
    |       // -> see docs/systems/fishing-system.md 섹션 7.3 (canonical)
    |-- failXP: int                        // 1
    |       // -> see docs/systems/fishing-system.md 섹션 7.3 (canonical)
    |-- biteDelayMultiplierByLevel: float[] // 레벨별 바이트 대기 시간 보정
    |       // -> see docs/systems/fishing-system.md 섹션 7.4 (canonical)
    |-- rarityBonusByLevel: float[]         // 레벨별 희귀종 확률 보정
    |       // -> see docs/systems/fishing-system.md 섹션 7.4 (canonical)
    |-- treasureBonusByLevel: float[]       // 레벨별 보물 상자 확률 보너스
    |       // -> see docs/systems/fishing-system.md 섹션 7.4 (canonical)
    |-- doubleCatchChanceByLevel: float[]   // 레벨별 더블 캐치 확률
    |       // -> see docs/systems/fishing-system.md 섹션 7.4 (canonical)
    |-- maxQualityByLevel: CropQuality[]   // 레벨별 최대 품질
    |       // -> see docs/systems/fishing-system.md 섹션 7.2 (canonical)
    |-- energyCostReductionByLevel: int[]  // 레벨별 에너지 소모 감소량
    |       // -> see docs/systems/fishing-system.md 섹션 7.2 (canonical)
    |-- successRateByLevel: float[]        // 레벨별 미니게임 성공률
    |       // -> see docs/systems/fishing-system.md 섹션 7.4 (canonical)
```

### 4A.4 AddXP 알고리즘

```
FishingProficiency.AddXP(int amount):
    if (_currentLevel >= _config.proficiencyMaxLevel): return
    // -> see docs/systems/fishing-system.md 섹션 7.2 for maxLevel

    _currentXP += amount

    while (_currentLevel < _config.proficiencyMaxLevel):
        int nextLevelXP = _config.proficiencyXPThresholds[_currentLevel]
        // -> see docs/systems/fishing-system.md 섹션 7.2 for XP 테이블
        if (_currentXP >= nextLevelXP):
            _currentLevel++
            OnLevelUp?.Invoke(_currentLevel)
            // FishingManager가 이 이벤트를 FishingEvents.OnProficiencyLevelUp에 위임
            Debug.Log($"[FishingProficiency] 레벨 업! Lv.{_currentLevel}")
        else:
            break
```

### 4A.5 GetXPForCatch 알고리즘

```
FishingProficiency.GetXPForCatch(FishRarity rarity, bool isGiant): int
    int baseXP = _config.catchXPByRarity[(int)rarity]
    // -> see docs/systems/fishing-system.md 섹션 7.3 for XP 값
    if (isGiant):
        baseXP = (int)(baseXP * _config.giantXPMultiplier)
        // -> see docs/systems/fishing-system.md 섹션 7.3 for giantXPMultiplier
    return baseXP
```

### 4A.6 보정 메서드 pseudocode

모든 보정값은 FishingConfig SO의 레벨별 배열에서 조회한다. 배열의 canonical 값은 `(-> see docs/systems/fishing-system.md 섹션 7.2~7.4)`이다.

```
GetBiteDelayMultiplier(): float
    return _config.biteDelayMultiplierByLevel[_currentLevel - 1]
    // Lv.1=1.0, Lv.5=0.85, Lv.10=0.70
    // -> see docs/systems/fishing-system.md 섹션 7.4

GetRarityBonus(): float
    return _config.rarityBonusByLevel[_currentLevel - 1]
    // Lv.1=1.0, Lv.4=1.1, Lv.7+=1.2
    // -> see docs/systems/fishing-system.md 섹션 7.4

GetTreasureChestBonus(): float
    return _config.treasureBonusByLevel[_currentLevel - 1]
    // Lv.1=0.0, Lv.5+=0.02
    // -> see docs/systems/fishing-system.md 섹션 7.4

GetMaxFishQuality(): CropQuality
    return _config.maxQualityByLevel[_currentLevel - 1]
    // Lv.1~2=Normal, Lv.3~5=Silver, Lv.6~8=Gold, Lv.9+=Iridium
    // -> see docs/systems/fishing-system.md 섹션 7.2

GetDoubleCatchChance(): float
    return _config.doubleCatchChanceByLevel[_currentLevel - 1]
    // Lv.1~8=0.0, Lv.9+=0.05
    // -> see docs/systems/fishing-system.md 섹션 7.4

GetEnergyCostReduction(): int
    return _config.energyCostReductionByLevel[_currentLevel - 1]
    // Lv.1~7=0, Lv.8+=1
    // -> see docs/systems/fishing-system.md 섹션 7.2

GetMiniGameSuccessRate(): float
    return _config.successRateByLevel[_currentLevel - 1]
    // Lv.1=0.50, Lv.5=0.65, Lv.10=0.80
    // -> see docs/systems/fishing-system.md 섹션 7.4
```

### 4A.7 FishingManager 통합

FishingManager에서 숙련도 보정이 적용되는 지점:

```
1) CalculateBiteDelay():
    float baseDelay = Random.Range(config.biteDelayRange.x, config.biteDelayRange.y)
    float proficiencyMul = _proficiency.GetBiteDelayMultiplier()
    // -> see docs/systems/fishing-system.md 섹션 7.4
    return baseDelay * proficiencyMul

2) SelectRandomFish(FishingPoint point):
    // 기존 알고리즘(섹션 11.3)의 4단계에 숙련도 보정 추가
    4-a) 숙련도 희귀종 보정
        -> if (fish.rarity >= FishRarity.Rare):
               weight *= _proficiency.GetRarityBonus()
               // -> see docs/systems/fishing-system.md 섹션 7.4

3) CompleteFishing(success: true):
    // Giant 판정 (FishData에 isGiant 필드 없음 — 런타임 판정)
    bool canBeGiant = (fish.rarity == FishRarity.Common || fish.rarity == FishRarity.Rare)
                      // Giant 가능 어종: 잉어(C)/산천어(R)/황금 잉어(R)/무지개송어(R)
                      // -> see docs/systems/fishing-system.md 섹션 4.4 (canonical)
    bool isGiant = canBeGiant && (Random.value < _fishingConfig.giantChance)
                      // giantChance = 0.05 -> see docs/systems/fishing-system.md 섹션 4.4

    // 품질 판정 시 최대 품질 제한
    CropQuality maxQuality = _proficiency.GetMaxFishQuality()
    // -> see docs/systems/fishing-system.md 섹션 7.2
    quality = Min(_minigame.CalculateQuality(), maxQuality)

    // 더블 캐치 판정
    float doubleCatchChance = _proficiency.GetDoubleCatchChance()
    // -> see docs/systems/fishing-system.md 섹션 7.4
    int catchCount = (Random.value < doubleCatchChance) ? 2 : 1

4) TryStartFishing(point):
    // 에너지 소모 보정
    int baseCost = (-> see docs/systems/farming-system.md 섹션 3.2)
    int reduction = _proficiency.GetEnergyCostReduction()
    // -> see docs/systems/fishing-system.md 섹션 7.2
    int finalCost = Max(1, baseCost - reduction)

5) GrantRewards():
    // 보물 상자 확률 보정 (기존 보물 상자 드랍 로직에)
    float baseTreasureChance = (-> see docs/systems/fishing-system.md 섹션 5.3)
    float bonus = _proficiency.GetTreasureChestBonus()
    // -> see docs/systems/fishing-system.md 섹션 7.4
    float finalTreasureChance = baseTreasureChance + bonus

6) FishingMinigame.EvaluateResult():
    // 미니게임 종료 시 성공/실패 판정
    float successRate = _proficiency.GetMiniGameSuccessRate()
    // -> see docs/systems/fishing-system.md 섹션 7.4
    // 현재 ExcitementGauge 채우기 100% 도달 여부를 기본 판정으로 하되,
    // 낮은 숙련도에서 RNG 판정 추가로 "실력 있어도 실패 가능" 구현
    // 설계 의도: Lv.1 50%는 타고난 어려움 (낮은 숙련도 = 불안정한 낚시)
    bool minigameSuccess = minigame.IsTargetZoneReached() &&
                           (Random.value < successRate)
    // FishingManager.OnMinigameComplete(bool success)에 전달
```

### 4A.8 실패 시 XP 부여

미니게임 실패 시에도 소량 XP를 부여한다:

```
FishingManager.CompleteFishing(success: false):
    // 기존 실패 처리 후
    int failXP = _config.failXP
    // -> see docs/systems/fishing-system.md 섹션 7.3
    _proficiency.AddXP(failXP)
    _fishingStats.currentStreak = 0
```

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
    +-- 5) 낚시 숙련도 XP 부여 (ARC-029)
    |       // isGiant: Giant 판정은 GrantRewards() 내부에서 별도 수행하며,
    |       //          CompleteFishing에는 bool isGiant 파라미터로 전달된다
    |       int fishingXP = _proficiency.GetXPForCatch(fish.rarity, isGiant)
    |       // XP 값 -> see docs/systems/fishing-system.md 섹션 7.3
    |       _proficiency.AddXP(fishingXP)
    |       // 레벨업 시 FishingEvents.OnProficiencyLevelUp 발행
    |
    +-- 6) 이벤트 발행
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

### 6.2 GetExpForSource() 확장 및 낚시 XP 계산 공식

`progression-architecture.md` 섹션 2.3의 switch 문에 다음 case를 추가해야 한다:

```csharp
// illustrative — progression-architecture.md 섹션 2.3에 추가
case XPSource.FishingCatch:
    // 낚시 XP는 FishData.expReward 기반, 품질 보정 적용
    // -> see docs/balance/progression-curve.md 섹션 1.2.7 for canonical XP 값
    var (fishData, fishQuality) = ((FishData, CropQuality))context;
    return CalculateFishingExp(fishData, fishQuality);
```

**CalculateFishingExp() 메서드 정의**:

```csharp
// illustrative — progression-architecture.md에 추가
private int CalculateFishingExp(FishData fishData, CropQuality quality)
{
    // 공식: floor(fishData.expReward * qualityExpBonus[quality])
    // → see docs/balance/progression-curve.md 섹션 1.2.7.2 for canonical 공식
    // → qualityExpBonus 테이블은 작물 수확과 공유 (섹션 1.2.2)
    float qualityBonus = GetQualityExpBonus(quality);
    // → see docs/balance/progression-curve.md 섹션 1.2.2 for qualityExpBonus 배율
    return Mathf.FloorToInt(fishData.expReward * qualityBonus);
}
```

**희귀도별 expReward canonical 값** (-> see `docs/balance/progression-curve.md` 섹션 1.2.7.1):

| FishRarity | expReward |
|------------|-----------|
| Common | 10 |
| Uncommon | 20 |
| Rare | 40 |
| Legendary | 80 |

**품질 보정 배율**: 작물 수확과 동일한 테이블 공유 (-> see `docs/balance/progression-curve.md` 섹션 1.2.2)

**FIX 태스크**: `progression-architecture.md` 섹션 2.2 XPSource enum에 `FishingCatch` 추가, 섹션 2.3 `GetExpForSource()` switch 문에 `FishingCatch` case 추가, `CalculateFishingExp()` 메서드 정의 추가 (FIX-050 권고).

[RESOLVED-FIX-064] 낚시 XP 계산 공식 확정: `floor(fishData.expReward * qualityExpBonus[quality])`. 희귀도별 기본 XP(Common=10, Uncommon=20, Rare=40, Legendary=80)와 품질 보정 배율(작물 수확과 동일 테이블)이 `docs/balance/progression-curve.md` 섹션 1.2.7에 canonical 등록되었다.

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

Zone F의 연못 타일(30타일) 중 가장자리 3개 타일에 FishingPoint MonoBehaviour 오브젝트를 배치한다. 각 FishingPoint는 인접 구역의 육지 타일(물리적 낚시 가능 위치 약 20개소)을 논리적으로 관할하며, 구역별 어종 풀과 희귀도 가중치를 관리한다. 플레이어가 유효 육지 타일에서 낚싯대를 사용하면 가장 가까운 FishingPoint로 요청이 위임된다. FishingManager는 FarmZoneManager에서 Zone F의 활성화 여부를 확인한 후, FishingPoint 목록을 초기화한다.

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

## 8A. 겨울 얼음 낚시 아키텍처 (FIX-071)

### 8A.1 설계 개요

겨울 계절에 Zone F 연못이 결빙되면, 플레이어는 곡괭이로 물 타일을 클릭하여 얼음 구멍을 뚫고 낚시할 수 있다. 이 섹션은 얼음 낚시의 기술 아키텍처를 정의한다.

**디자인 canonical**: 얼음 낚시 메카닉의 모든 수치(에너지 소모, 구멍 유지 일수, 최대 구멍 수, 겨울 어종 등)는 `(-> see docs/systems/fishing-system.md 섹션 10)`.

### 8A.2 IceHoleData 클래스

```csharp
// illustrative
namespace SeedMind.Fishing
{
    [System.Serializable]
    public class IceHoleData
    {
        public Vector2Int tilePosition;   // 얼음 구멍 타일 좌표
        public int createdDay;            // 생성 시점의 dayOfSeason
        public Season createdSeason;      // 항상 Season.Winter
    }
}
```

### 8A.3 FishingManager 내 얼음 낚시 로직

FishingManager가 얼음 낚시를 직접 관리한다. 별도 IceHoleManager를 두지 않고, FishingManager 내부에 메서드를 추가하여 복잡도를 억제한다.

```
CreateIceHole(Vector2Int tilePos): bool
    |
    +-- 1) 계절 체크: TimeManager.CurrentSeason != Winter → return false
    +-- 2) 타일 체크: FarmZoneManager.GetZoneForTile(tilePos).zoneType != Pond → return false
    +-- 3) 수량 체크: _activeIceHoles.Count >= _fishingConfig.iceHoleMax → return false
    |       // iceHoleMax (-> see docs/systems/fishing-system.md 섹션 10)
    +-- 4) 중복 체크: _activeIceHoles.Any(h => h.tilePosition == tilePos) → return false
    +-- 5) 에너지 소모: EnergyManager.TryConsumeEnergy(_fishingConfig.iceHoleEnergy)
    |       // iceHoleEnergy (-> see docs/systems/fishing-system.md 섹션 10)
    +-- 6) 도구 체크: 플레이어가 곡괭이(Pickaxe) 장착 중인지 확인
    +-- 7) 생성: _activeIceHoles.Add(new IceHoleData {
    |           tilePosition = tilePos,
    |           createdDay = TimeManager.CurrentDayOfSeason,
    |           createdSeason = Season.Winter
    |       })
    +-- 8) VFX: 얼음 깨짐 이펙트 재생
    +-- return true
```

```
RemoveExpiredHoles():  // OnDayChanged(priority: 55)에서 호출
    |
    +-- foreach hole in _activeIceHoles:
    |       int elapsed = TimeManager.CurrentDayOfSeason - hole.createdDay
    |       bool expired = elapsed >= _fishingConfig.iceHoleDuration
    |           // iceHoleDuration (-> see docs/systems/fishing-system.md 섹션 10)
    |       bool snowRefreeze = WeatherSystem.CurrentWeather == Weather.Snow
    |           && elapsed >= 1  // 눈 내리는 날은 다음 날 재결빙
    |       if expired || snowRefreeze:
    |           markForRemoval(hole)
    +-- 제거 대상 일괄 삭제
    +-- VFX: 재결빙 이펙트 재생 (제거된 구멍 위치)
```

```
RemoveAllIceHoles():  // OnSeasonChanged(priority: 55)에서 호출
    |
    +-- if newSeason != Winter:
    |       _activeIceHoles.Clear()
    |       // 모든 얼음 구멍 VFX 제거
    +-- [참고] 겨울->겨울 전환은 불가능하므로 항상 전체 삭제
```

### 8A.4 계절 전환 연동 (OnSeasonChanged)

FishingManager는 `TimeManager.RegisterOnSeasonChanged(priority: 55)`로 이벤트를 구독한다(섹션 1 참조). OnSeasonChanged 콜백에서 다음을 수행한다:

1. **겨울 종료 시 (Winter -> Spring)**: `RemoveAllIceHoles()` 호출. 모든 활성 얼음 구멍을 제거하고 결빙 VFX를 해제한다.
2. **겨울 시작 시 (Autumn -> Winter)**: Zone F 연못 타일에 결빙 VFX를 적용한다. 이 시점에서 얼음 구멍은 아직 없으며, 플레이어의 곡괭이 인터랙션으로만 생성된다.

**중요**: `time-season-architecture.md` 섹션 4.4 OnSeasonChanged 구독자 실행 순서에 FishingManager(priority: 55)가 등록되어야 한다. GrowthSystem(10)과 HUDController(90) 사이에 배치된다.

### 8A.5 TryStartFishing 분기 확장

기존 `TryStartFishing(FishingPoint point)` 로직에 겨울 계절 분기를 추가한다:

```
TryStartFishing(FishingPoint point): bool
    |
    +-- [기존] point.CanFish() 체크
    +-- [기존] 낚싯대 소지 체크
    +-- [추가 — FIX-071] 겨울 계절 체크:
    |       if TimeManager.CurrentSeason == Winter:
    |           // 얼음 구멍이 있는 타일에서만 낚시 가능
    |           if !IsIceHole(point.tilePosition):
    |               return false  // "얼음 구멍을 먼저 뚫어야 합니다" 메시지
    |           // Blizzard 날씨면 낚시 불가
    |           if WeatherSystem.CurrentWeather == Weather.Blizzard:
    |               return false  // "눈보라로 낚시할 수 없습니다" 메시지
    +-- [기존] 상태 전환: Idle -> Casting
```

### 8A.6 어종 필터링 확장

기존 `SelectRandomFish()` 알고리즘(섹션 11.3)의 1단계 계절 필터링에서, 겨울 계절에는 `seasonAvailability`에 `Winter` 플래그가 설정된 어종만 반환된다. 겨울 전용 어종은 `(-> see docs/systems/fishing-system.md 섹션 10)`.

### 8A.7 세이브/로드

FishingSaveData에 얼음 구멍 목록을 추가한다:

```csharp
// illustrative -- FishingSaveData 확장 (FIX-071)
// 기존 필드는 섹션 10 참조
public List<IceHoleData> activeIceHoles;  // 활성 얼음 구멍 목록
```

GetSaveData()/LoadSaveData()에서 `_activeIceHoles`를 직렬화/역직렬화한다.

**필드 수 동기화**: 기존 JSON 9개 + C# 9개 -> JSON 10개 + C# 10개 (activeIceHoles 추가).

---

# Part III -- 데이터 구조

---

## 9. FishData SO JSON 예시

```json
{
    "dataId": "fish_carp",
    "displayName": "잉어",
    "description": "연못에서 흔히 잡히는 민물고기. 사계절 잡을 수 있다.",
    "icon": "(Sprite 에셋 참조 -- JSON 직렬화 대상 아님, Unity Inspector에서 할당)",
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
- `icon`: Sprite 에셋 참조. Unity SO에서 Inspector로 할당하며, JSON 직렬화 대상이 아님. C# 클래스(섹션 3) `GameDataSO.icon` 필드에 대응. (PATTERN-005 동기화 항목)
- `basePrice`: `0`으로 표기 -- canonical 값은 `(-> see docs/systems/fishing-system.md 섹션 4.2)`. BAL-012에서 15종 전수 확정 완료 (-> see `docs/balance/fishing-economy.md` 섹션 1.3).
- `seasonAvailability`: `15` = `0b1111` (Spring|Summer|Fall|Winter) -- 사계절 출현 `(-> see docs/systems/time-season.md for SeasonFlag)`
- `timeWeights`: 5개 시간대 가중치 [Dawn, Morning, Afternoon, Evening, Night] `(-> see docs/systems/time-season.md 섹션 1.2 for 시간대 정의)`
- `weatherBonus`: `2` = `WeatherFlag.Rain` -- 비 올 때 출현율 증가
- `expReward`: `0`으로 표기 -- canonical 값은 `(-> see docs/balance/progression-curve.md 섹션 1.2.7.1)`. Common 어종의 실제 값은 10. JSON 예시에서는 canonical 문서 참조용 플레이스홀더로 유지한다.

[RESOLVED-BAL-012] basePrice 15종 전수 확정 완료 (-> see `docs/balance/fishing-economy.md` 섹션 1.3). expReward는 FIX-064에서 확정 완료 (-> see `docs/balance/progression-curve.md` 섹션 1.2.7). JSON 예시의 플레이스홀더 값(0)은 MCP 실행 시 canonical 값으로 대체한다.

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

        // 숙련도 (ARC-029)
        public int fishingProficiencyXP;                    // 누적 숙련도 XP
        public int fishingProficiencyLevel;                 // 현재 숙련도 레벨 (1~10)
            // -> see docs/systems/fishing-system.md 섹션 7.2 for 레벨 테이블

        // 얼음 낚시 (FIX-071)
        public List<IceHoleData> activeIceHoles;            // 활성 얼음 구멍 목록 (null 허용 -- 구버전 세이브 호환)
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
| `fishingProficiencyXP` | `fishingProficiencyXP` | `int` | 일치 (ARC-029) |
| `fishingProficiencyLevel` | `fishingProficiencyLevel` | `int` | 일치 (ARC-029) |
| `activeIceHoles` | `activeIceHoles` | `List<IceHoleData>` | 일치 (FIX-071), null 허용 |

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
    "currentStreak": 2,
    "fishingProficiencyXP": 320,
    "fishingProficiencyLevel": 4,
    "activeIceHoles": [
        { "tilePosition": { "x": 5, "y": 3 }, "createdDay": 12, "createdSeason": 3 }
    ]
}
```

**필드 수**: JSON 10개, C# 10개 -- 동기화 완료 (FIX-071: activeIceHoles 추가).

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
    |           currentStreak = _fishingStats.currentStreak,
    |           fishingProficiencyXP = _proficiency.CurrentXP,       // ARC-029
    |           fishingProficiencyLevel = _proficiency.CurrentLevel,  // ARC-029
    |           activeIceHoles = new List<IceHoleData>(_activeIceHoles)  // FIX-071
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
    |           _proficiency.LoadSaveData(0, 1)    // ARC-029: Lv.1, 0 XP
    |           _activeIceHoles = new List<IceHoleData>()  // FIX-071
    |       else:
    |           _fishingStats.totalCasts = data.fishing.totalCasts
    |           _fishingStats.totalCaught = data.fishing.totalCaught
    |           ... (나머지 통계 필드 복원)
    |           _proficiency.LoadSaveData(            // ARC-029
    |               data.fishing.fishingProficiencyXP,
    |               data.fishing.fishingProficiencyLevel
    |           )
    |           _activeIceHoles = data.fishing.activeIceHoles   // FIX-071
    |               ?? new List<IceHoleData>()  // null 방어 (구버전 세이브 호환)
    +-- [55] InventoryManager ...
```

---

# Part VI -- MCP 태스크 요약

---

## 13. MCP 구현 태스크 시퀀스

> 상세 태스크는 `docs/mcp/fishing-tasks.md` (ARC-028)에서 별도 관리된다. 이 섹션은 Phase 요약 개요를 제공한다.

### Phase A: 데이터 레이어 (SO 생성)

| Step | MCP 명령 | 설명 |
|------|---------|------|
| A-1 | CreateScript `FishRarity.cs` | enum 정의 (SeedMind.Fishing) |
| A-2 | CreateScript `FishData.cs` : ScriptableObject | GameDataSO 상속, IInventoryItem 구현 |
| A-3 | CreateScript `FishingConfig.cs` : ScriptableObject | 미니게임 밸런스 파라미터 |
| A-4 | CreateScript `FishingState.cs` | enum 정의 |
| A-5 | CreateScript `MinigameResult.cs` | enum 정의 |
| A-6 | CreateScript `FishingStats.cs` | Plain C# 통계 클래스 |
| A-7 | CreateScript `FishingSaveData.cs` | 직렬화 가능 클래스 (fishingProficiencyXP/Level 포함) |
| A-8 | CreateScript `FishingEvents.cs` | 정적 이벤트 허브 (OnProficiencyLevelUp 포함) |
| A-9 | CreateScript `FishingProficiency.cs` | Plain C# 숙련도 클래스 (ARC-029) |

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

### Phase D2: 얼음 낚시 (FIX-071)

| Step | MCP 명령 | 설명 |
|------|---------|------|
| D2-1 | CreateScript `IceHoleData.cs` | Plain C# 직렬화 가능 데이터 클래스 |
| D2-2 | EditScript `FishingConfig.cs` | `iceHoleDuration`, `iceHoleMax`, `iceHoleEnergy` 필드 추가. 값은 `(-> see docs/systems/fishing-system.md 섹션 10)` |
| D2-3 | EditScript `FishingManager.cs` | `_activeIceHoles`, `CreateIceHole()`, `RemoveExpiredHoles()`, `RemoveAllIceHoles()`, `IsIceHole()` 추가 |
| D2-4 | EditScript `FishingManager.cs` | `TryStartFishing()` 내 겨울 계절 분기 추가 |
| D2-5 | EditScript `FishingSaveData.cs` | `activeIceHoles` 필드 추가 |
| D2-6 | EditScript `FishingManager.cs` | `GetSaveData()`/`LoadSaveData()`에 얼음 구멍 직렬화 추가 |
| D2-7 | Zone F 연못 타일에 결빙 머티리얼/VFX 프리팹 배치 | 겨울 계절 시각 효과 |

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
| F-7 | 겨울 계절에 얼음 구멍 생성 -> 낚시 -> 성공/실패 흐름 확인 (FIX-071) |
| F-8 | 얼음 구멍 만료(iceHoleDuration 경과) 시 자동 제거 확인 (FIX-071) |
| F-9 | 계절 전환(겨울->봄) 시 모든 얼음 구멍 제거 확인 (FIX-071) |
| F-10 | Blizzard 날씨에서 낚시 시도 차단 확인 (FIX-071) |

---

---

# Part VII -- 낚시 도감 아키텍처 (ARC-030)

---

## 14. 개요

낚시 도감(Fish Catalog)은 플레이어가 포획한 어종을 자동으로 기록하고, 미발견 어종에 대한 힌트를 제공하며, 완성도 마일스톤에 따른 보상을 트리거하는 수집 시스템이다. 업적 시스템(-> see `docs/systems/achievement-architecture.md`)의 AchievementManager 패턴을 참고하되, 도감 고유의 크기 판정(SizeRoll)과 최대 크기 기록 추적 기능을 포함한다.

**설계 목표**:
- 어종 도감 데이터를 FishCatalogData SO로 외부화하여 힌트 텍스트 등을 코드 변경 없이 수정 가능
- FishCatalogManager가 도감 상태(발견/미발견, 최대 크기, 포획 횟수)를 관리
- 낚시 성공 시 크기 판정(SizeRoll)을 수행하여 크기 기록 갱신 및 판매가 보정
- 도감 완성도 마일스톤 달성 시 이벤트를 통해 보상 지급
- FishCatalogSaveData로 세이브/로드 연동

---

## 15. FishCatalogData (ScriptableObject)

어종 도감의 정적 표시 정보를 정의하는 SO. FishData SO와 1:1 대응하며, 도감 UI에서 표시할 힌트 및 크기 범위 정보를 담는다.

```csharp
// illustrative
namespace SeedMind.Fishing.Catalog
{
    [CreateAssetMenu(fileName = "NewFishCatalogData", menuName = "SeedMind/FishCatalogData")]
    public class FishCatalogData : ScriptableObject
    {
        [Header("식별")]
        public string fishId;                       // FishData.fishId와 동일 키
        public string displayName;                  // 도감 표시명 (FishData.displayName과 동일)
            // -> see docs/systems/fishing-system.md 섹션 4.2 for 어종명

        [Header("도감 힌트")]
        [TextArea(2, 4)]
        public string hintLocked;                   // 미발견 시 표시 힌트 (예: "봄에 연못에서 자주 보인다")
        [TextArea(2, 4)]
        public string hintUnlocked;                 // 발견 후 표시 설명 (예: "사계절 잡히는 민물고기")

        [Header("희귀도")]
        public FishRarity rarityTier;               // -> see 섹션 3 FishRarity enum

        [Header("크기 범위")]
        public float sizeMinCm;                     // 최소 크기 (cm, 절대값)
            // -> see docs/content/fish-catalog.md 섹션 3.1 (canonical)
        public float sizeMaxCm;                     // 최대 크기 (cm, 절대값)
            // -> see docs/content/fish-catalog.md 섹션 3.1 (canonical)
        public float giantSizeMultiplier;           // Giant 변이 시 추가 크기 배율
            // -> see docs/systems/fishing-system.md 섹션 4.4
        // 크기 등급(Small/Medium/Large) 분류 기준 및 판매가 보정률:
        //   -> see docs/content/fish-catalog.md 섹션 2.3 (3등급 이산 보정 canonical)

        [Header("초회 포획 보상")]
        public int firstCatchGold;                  // 초회 등록 골드 보상
            // -> see docs/content/fish-catalog.md 섹션 3.1 (canonical)
        public int firstCatchXP;                    // 초회 등록 XP 보상
            // -> see docs/content/fish-catalog.md 섹션 3.1 (canonical)

        [Header("도감 표시")]
        public Sprite catalogIcon;                  // 도감 전용 아이콘 (null이면 FishData.icon 사용)
        public int sortOrder;                       // 도감 내 표시 순서
    }
}
```

### 15.1 FishCatalogData JSON 스키마

```json
{
    "fishId": "fish_carp",
    "displayName": "잉어",
    "hintLocked": "연못에서 흔히 볼 수 있는 민물고기. 사계절 잡을 수 있다고 한다.",
    "hintUnlocked": "연못의 대표 어종. 사계절 안정적으로 잡히며, 가끔 대물이 걸리기도 한다.",
    "rarityTier": "Common",
    "sizeMinCm": 0,
    "sizeMaxCm": 0,
    "giantSizeMultiplier": 0,
    "firstCatchGold": 0,
    "firstCatchXP": 0,
    "catalogIcon": "(Sprite 에셋 참조 -- JSON 직렬화 대상 아님)",
    "sortOrder": 3
}
```

**필드 설명**:
- `sizeMinCm`, `sizeMaxCm`: `0`으로 표기 -- canonical 값은 `(-> see docs/content/fish-catalog.md 섹션 3.1)`.
- `giantSizeMultiplier`: `0`으로 표기 -- canonical 값은 `(-> see docs/systems/fishing-system.md 섹션 4.4)`.
- `firstCatchGold`, `firstCatchXP`: `0`으로 표기 -- canonical 값은 `(-> see docs/content/fish-catalog.md 섹션 3.1)`.
- `catalogIcon`: Sprite 에셋 참조. Unity SO에서 Inspector로 할당. JSON 직렬화 대상 아님 (PATTERN-005).
- 크기 등급 보정률(x0.9/x1.0/x1.15): FishCatalogData SO에 저장되지 않음 -- 코드에서 fish-catalog.md 섹션 2.3 기준으로 직접 판정.

### 15.2 PATTERN-005 검증: FishCatalogData C# ↔ JSON 동기화

| C# 필드 | JSON 키 | 타입 | 일치 |
|---------|---------|------|:----:|
| fishId | fishId | string | O |
| displayName | displayName | string | O |
| hintLocked | hintLocked | string | O |
| hintUnlocked | hintUnlocked | string | O |
| rarityTier | rarityTier | FishRarity(string) | O |
| sizeMinCm | sizeMinCm | float | O |
| sizeMaxCm | sizeMaxCm | float | O |
| giantSizeMultiplier | giantSizeMultiplier | float | O |
| firstCatchGold | firstCatchGold | int | O |
| firstCatchXP | firstCatchXP | int | O |
| catalogIcon | (에디터 전용, 직렬화 제외) | Sprite | - |
| sortOrder | sortOrder | int | O |

필드 수: C# 12개 (catalogIcon 포함) / JSON 11개 (catalogIcon 제외, 에디터 전용) -- 일치 [FIX-075 적용: baseSizeCm/variance → sizeMinCm/sizeMaxCm, firstCatchGold/XP 추가]

---

## 16. FishCatalogManager (MonoBehaviour)

도감 상태를 관리하는 매니저. FishingManager와 분리하여 단일 책임을 유지한다.

```
+--------------------------------------------------------------+
|       FishCatalogManager (MonoBehaviour, Singleton)            |
|--------------------------------------------------------------|
|  [데이터]                                                      |
|  - _catalogDataRegistry: FishCatalogData[]                    |
|       // Inspector 할당, 전체 어종 도감 SO (15종)              |
|  - _catalogDataMap: Dictionary<string, FishCatalogData>       |
|       // fishId -> SO 빠른 조회용 (Initialize에서 구축)        |
|                                                              |
|  [런타임 상태]                                                 |
|  - _entries: Dictionary<string, FishCatalogEntry>             |
|       // fishId -> 런타임 도감 항목 (발견 여부, 최대 크기 등)  |
|  - _discoveredCount: int                                      |
|       // 발견한 어종 수 (빠른 조회용 캐시)                     |
|  - _totalFishCount: int                                       |
|       // 전체 어종 수 (15) -- _catalogDataRegistry.Length      |
|                                                              |
|  [읽기 전용 프로퍼티]                                          |
|  + DiscoveredCount: int                                       |
|  + TotalFishCount: int                                        |
|  + CompletionRate: float  // DiscoveredCount / TotalFishCount |
|  + IsComplete: bool       // DiscoveredCount == TotalFishCount|
|                                                              |
|  [이벤트]                                                     |
|  + OnFishDiscovered: Action<string>                           |
|       // fishId -- 최초 발견 시에만 발행                       |
|  + OnCatalogUpdated: Action<string, FishCatalogEntry>         |
|       // fishId, 갱신된 항목 -- 매 포획 시 발행                |
|  + OnMilestoneReached: Action<int>                            |
|       // discoveredCount -- 마일스톤 도달 시 발행              |
|  + OnCatalogCompleted: Action                                 |
|       // 도감 100% 완성 시 1회 발행                            |
|                                                              |
|  [메서드]                                                     |
|  + Initialize(): void                                         |
|       // _catalogDataMap 구축, 빈 _entries 생성               |
|  + RegisterCatch(string fishId, float sizeCm,                 |
|       bool isGiant): FishCatalogEntry                         |
|       // 포획 기록 갱신, 최초 발견/최대 크기 갱신 판정         |
|  + GetEntry(string fishId): FishCatalogEntry                  |
|       // null이면 미발견                                       |
|  + GetCatalogData(string fishId): FishCatalogData             |
|  + GetAllEntries(): IReadOnlyDictionary<string,               |
|       FishCatalogEntry>                                       |
|  + IsDiscovered(string fishId): bool                          |
|  - CheckMilestone(int newCount): void                         |
|       // 마일스톤 도달 여부 판정                               |
|                                                              |
|  [ISaveable 구현]                                             |
|  + SaveLoadOrder => 53                                        |
|       // FishingManager(52) 이후, InventoryManager(55) 이전   |
|  + GetSaveData(): object                                      |
|  + LoadSaveData(object data): void                            |
|                                                              |
|  [구독]                                                       |
|  + OnEnable():                                                |
|      FishingEvents.OnFishCaughtWithSize += HandleFishCaught   |
|  + OnDisable(): 구독 해제                                     |
+--------------------------------------------------------------+
```

### 16.1 RegisterCatch 알고리즘

```
RegisterCatch(fishId, sizeCm, isGiant):
    entry = _entries[fishId]
    if entry == null:
        // 최초 발견
        entry = new FishCatalogEntry(fishId)
        _entries[fishId] = entry
        _discoveredCount++
        entry.isDiscovered = true
        entry.firstCaughtDay = TimeManager.CurrentDay
        entry.firstCaughtSeason = TimeManager.CurrentSeason
        entry.firstCaughtYear = TimeManager.CurrentYear
        OnFishDiscovered?.Invoke(fishId)
        CheckMilestone(_discoveredCount)

    entry.totalCaught++
    if isGiant: entry.giantCaught++

    if sizeCm > entry.maxSizeCm:
        entry.maxSizeCm = sizeCm
        entry.isNewRecord = true   // UI에서 "NEW!" 표시용
    else:
        entry.isNewRecord = false

    OnCatalogUpdated?.Invoke(fishId, entry)
    return entry
```

### 16.2 CheckMilestone 알고리즘

```
CheckMilestone(newCount):
    milestones = [5, 10, 15]
        // -> see docs/content/fish-catalog.md 섹션 4.1 for 마일스톤 목록 및 보상 (확정: 5/10/15종 3단계)
    if newCount in milestones:
        OnMilestoneReached?.Invoke(newCount)
    if newCount == _totalFishCount:
        OnCatalogCompleted?.Invoke()
```

[RESOLVED-CON-011] 마일스톤 보상 종류 확정: 5종(100G+30XP+미끼x10), 10종(300G+80XP+고급미끼x5+도감배경), 15종(500G+150XP+도감배경+프레임) (-> see `docs/content/fish-catalog.md` 섹션 4.1).

---

## 17. FishCatalogEntry (런타임 상태 클래스)

```csharp
// illustrative
namespace SeedMind.Fishing.Catalog
{
    /// <summary>
    /// 개별 어종의 도감 런타임 상태.
    /// FishCatalogManager가 Dictionary로 관리하며, 세이브/로드 시 직렬화된다.
    /// </summary>
    [System.Serializable]
    public class FishCatalogEntry
    {
        public string fishId;              // FishCatalogData.fishId 참조
        public bool isDiscovered;          // 발견 여부
        public int totalCaught;            // 이 어종의 누적 포획 횟수
        public int giantCaught;            // Giant 변이 포획 횟수
        public float maxSizeCm;            // 최대 크기 기록 (cm)
        public int firstCaughtDay;         // 최초 포획 일차 (-1 = 미발견)
        public int firstCaughtSeason;      // 최초 포획 계절 인덱스 (-1 = 미발견)
        public int firstCaughtYear;        // 최초 포획 연도 (-1 = 미발견)

        [System.NonSerialized]
        public bool isNewRecord;           // 이번 포획이 최대 크기 갱신인지 (직렬화 제외, UI 전용)

        public FishCatalogEntry(string id)
        {
            fishId = id;
            isDiscovered = false;
            totalCaught = 0;
            giantCaught = 0;
            maxSizeCm = 0f;
            firstCaughtDay = -1;
            firstCaughtSeason = -1;
            firstCaughtYear = -1;
            isNewRecord = false;
        }
    }
}
```

### 17.1 PATTERN-005 검증: FishCatalogEntry C# ↔ JSON

| C# 필드 | JSON 키 | 타입 | 일치 |
|---------|---------|------|:----:|
| fishId | fishId | string | O |
| isDiscovered | isDiscovered | bool | O |
| totalCaught | totalCaught | int | O |
| giantCaught | giantCaught | int | O |
| maxSizeCm | maxSizeCm | float | O |
| firstCaughtDay | firstCaughtDay | int | O |
| firstCaughtSeason | firstCaughtSeason | int | O |
| firstCaughtYear | firstCaughtYear | int | O |
| isNewRecord | (NonSerialized, 직렬화 제외) | bool | - |

필드 수: C# 9개 (isNewRecord 포함) / JSON 8개 (isNewRecord 제외, NonSerialized) -- 일치

---

## 18. 크기 판정 아키텍처 (SizeRoll)

낚시 성공 시 물고기의 실제 크기를 결정하는 로직. FishingManager의 `CompleteFishing(true)` 흐름에서 호출된다.

### 18.1 SizeRoll 알고리즘

```
RollFishSize(FishCatalogData catalogData, bool isGiant): float
    // 1. sizeMinCm ~ sizeMaxCm 범위에서 가중 랜덤 크기 결정
    //    -> see docs/content/fish-catalog.md 섹션 2.2 for 가중 분포 공식 (weightedRandom = random^1.3)
    t = Mathf.Pow(Random.value, 1.3f)  // 소형 편향 분포
    rawSizeCm = Mathf.Lerp(catalogData.sizeMinCm, catalogData.sizeMaxCm, t)
        // -> see docs/content/fish-catalog.md 섹션 3.1 for sizeMinCm/sizeMaxCm canonical 값

    // 2. Giant 보정 (Giant일 경우 sizeMaxCm * giantSizeMultiplier로 고정)
    if isGiant:
        return catalogData.sizeMaxCm * catalogData.giantSizeMultiplier
            // -> see docs/systems/fishing-system.md 섹션 4.4

    return rawSizeCm
```

### 18.2 크기 기반 판매가 보정 (3등급 이산 보정)

크기 등급(Small/Medium/Large)을 판정하여 이산 배율을 반환한다.
등급 분류 기준 및 배율은 `(-> see docs/content/fish-catalog.md 섹션 2.3)` canonical.

```
GetSizePriceMultiplier(FishCatalogData catalogData, float sizeCm, bool isGiant): float
    // Giant는 별도 처리 (크기 보정률 영향 없음, Giant 배수만 적용)
    if isGiant: return 1.0f

    range = catalogData.sizeMaxCm - catalogData.sizeMinCm
    // 3등급 분류: -> see docs/content/fish-catalog.md 섹션 2.3
    //   Small  : sizeMinCm ~ sizeMinCm + range*0.5  → x0.9
    //   Medium : sizeMinCm + range*0.5 ~ sizeMinCm + range*0.8 → x1.0
    //   Large  : sizeMinCm + range*0.8 ~ sizeMaxCm  → x1.15
    if sizeCm < catalogData.sizeMinCm + range * 0.5f:
        return 0.9f   // Small
    elif sizeCm < catalogData.sizeMinCm + range * 0.8f:
        return 1.0f   // Medium
    else:
        return 1.15f  // Large
```

### 18.3 FishingManager.CompleteFishing 확장

기존 `CompleteFishing(true)` 흐름에 크기 판정과 도감 업데이트를 추가한다:

```
CompleteFishing(success = true):
    fish = _activeFish
    quality = DetermineQuality(...)    // 기존 품질 판정

    // --- ARC-030: 크기 판정 및 도감 연동 ---
    catalogData = FishCatalogManager.Instance.GetCatalogData(fish.fishId)
    isGiant = RollGiantChance(fish)     // 기존 Giant 판정 (-> see 섹션 4.4)
    sizeCm = RollFishSize(catalogData, isGiant)
    sizePriceMul = GetSizePriceMultiplier(catalogData, sizeCm)

    // 도감 등록
    catalogEntry = FishCatalogManager.Instance.RegisterCatch(
        fish.fishId, sizeCm, isGiant
    )

    // 이벤트 발행 (기존 OnFishCaught 확장)
    FishingEvents.OnFishCaughtWithSize?.Invoke(fish, quality, sizeCm, isGiant)

    // 인벤토리 추가 (기존)
    InventoryManager.Instance.TryAddItem(fish, 1, quality, HarvestOrigin.Fishing)

    // 판매가에 크기 보정 반영 (EconomyManager가 sizePriceMul 참조)
    // -> 물고기 아이템에 sizePriceMul을 메타데이터로 부착 (InventorySlot.metadata)

    // XP, 숙련도 등 기존 보상 로직 유지
    GrantRewards(fish, quality)
```

---

## 19. 이벤트 시스템 확장

### 19.1 FishingEvents 확장 필드

기존 `FishingEvents` 정적 클래스에 도감 관련 이벤트를 추가한다:

```csharp
// illustrative
namespace SeedMind.Fishing
{
    public static partial class FishingEvents
    {
        // --- 기존 이벤트 (변경 없음) ---
        // public static Action<FishData, CropQuality> OnFishCaught;

        // --- ARC-030: 크기 정보 포함 이벤트 추가 ---
        public static Action<FishData, CropQuality, float, bool> OnFishCaughtWithSize;
            // (fishData, quality, sizeCm, isGiant)
            // FishCatalogManager가 이 이벤트를 구독하여 도감 업데이트

        // --- ARC-030: 도감 이벤트 (FishCatalogManager가 발행) ---
        public static Action<string> OnFishDiscovered;
            // fishId -- 최초 발견 시 발행
        public static Action<int> OnCatalogMilestoneReached;
            // discoveredCount -- 마일스톤 도달 시 발행
        public static Action OnCatalogCompleted;
            // 도감 100% 완성 시 1회 발행
    }
}
```

### 19.2 이벤트 흐름 다이어그램

```
[플레이어: 낚시 성공]
    |
    v
FishingManager.CompleteFishing(true)
    |
    +-- RollFishSize() --> sizeCm
    +-- GetSizePriceMultiplier() --> sizePriceMul
    |
    +-- FishingEvents.OnFishCaughtWithSize(fish, quality, sizeCm, isGiant)
    |       |
    |       +-- [구독자 1] FishCatalogManager.HandleFishCaught()
    |       |       |
    |       |       +-- RegisterCatch(fishId, sizeCm, isGiant)
    |       |       |       |
    |       |       |       +-- (최초 발견?) --> OnFishDiscovered
    |       |       |       |       |
    |       |       |       |       +-- [구독자] AchievementManager
    |       |       |       |       +-- [구독자] FishCatalogUI (도감 토스트)
    |       |       |       |
    |       |       |       +-- (마일스톤?) --> OnCatalogMilestoneReached
    |       |       |       |       |
    |       |       |       |       +-- [구독자] ProgressionManager (보상)
    |       |       |       |       +-- [구독자] FishCatalogUI (마일스톤 팝업)
    |       |       |       |
    |       |       |       +-- (100%?) --> OnCatalogCompleted
    |       |       |               |
    |       |       |               +-- [구독자] AchievementManager (ach_fish_04)
    |       |       |
    |       |       +-- OnCatalogUpdated(fishId, entry)
    |       |               |
    |       |               +-- [구독자] FishCatalogUI (항목 갱신)
    |       |
    |       +-- [구독자 2] ProgressionManager (기존 XP 부여)
    |       +-- [구독자 3] FishingStats 통계 업데이트
    |
    +-- InventoryManager.TryAddItem(fish, 1, quality, HarvestOrigin.Fishing)
    +-- GrantRewards(fish, quality)
```

### 19.3 기존 OnFishCaught와의 호환성

기존 `OnFishCaught(FishData, CropQuality)` 이벤트는 하위 호환을 위해 유지한다. `OnFishCaughtWithSize`를 신규 구독자에게 사용하고, `OnFishCaught`는 기존 구독자(ProgressionManager 등)가 계속 사용한다. FishingManager에서 양쪽 이벤트를 모두 발행한다:

```
CompleteFishing(true):
    ...
    FishingEvents.OnFishCaught?.Invoke(fish, quality)          // 기존 유지
    FishingEvents.OnFishCaughtWithSize?.Invoke(fish, quality, sizeCm, isGiant)  // ARC-030 추가
```

---

## 20. FishCatalogSaveData

### 20.1 C# 클래스

```csharp
// illustrative
namespace SeedMind.Fishing.Catalog
{
    [System.Serializable]
    public class FishCatalogSaveData
    {
        public List<FishCatalogEntry> entries;     // 발견된 어종 목록
        public int discoveredCount;                 // 발견 어종 수 (빠른 조회용 캐시)
    }
}
```

### 20.2 JSON 예시

```json
{
    "entries": [
        {
            "fishId": "fish_carp",
            "isDiscovered": true,
            "totalCaught": 12,
            "giantCaught": 1,
            "maxSizeCm": 48.7,
            "firstCaughtDay": 5,
            "firstCaughtSeason": 0,
            "firstCaughtYear": 1
        },
        {
            "fishId": "fish_golden_carp",
            "isDiscovered": true,
            "totalCaught": 2,
            "giantCaught": 0,
            "maxSizeCm": 35.2,
            "firstCaughtDay": 18,
            "firstCaughtSeason": 1,
            "firstCaughtYear": 1
        }
    ],
    "discoveredCount": 2
}
```

### 20.3 PATTERN-005 검증: FishCatalogSaveData C# ↔ JSON

| C# 필드 | JSON 키 | 타입 | 일치 |
|---------|---------|------|:----:|
| entries | entries | List\<FishCatalogEntry\> | O |
| discoveredCount | discoveredCount | int | O |

필드 수: 2개 -- 양쪽 일치

### 20.4 FishCatalogManager 세이브/로드 흐름

**SaveLoadOrder = 53**: FishingManager(52) 이후, InventoryManager(55) 이전. 도감 상태는 FishingStats 복원 후에 로드되어야 하며, 인벤토리와는 독립적이다.

(-> see `docs/systems/save-load-architecture.md` 섹션 7 for 전체 할당표)

**저장**:
```
SaveManager.SaveAsync()
    +-- [52] FishingManager.GetSaveData()      // 기존
    +-- [53] FishCatalogManager.GetSaveData()
    |       -> return new FishCatalogSaveData {
    |           entries = new List<FishCatalogEntry>(_entries.Values),
    |           discoveredCount = _discoveredCount
    |       }
    +-- [55] InventoryManager ...
```

**로드**:
```
SaveManager.LoadAsync()
    +-- [52] FishingManager.LoadSaveData(data.fishing)      // 기존
    +-- [53] FishCatalogManager.LoadSaveData(data.fishCatalog)
    |       if (data.fishCatalog == null):
    |           // 구버전 세이브 -- 기존 FishingStats.caughtByFishId에서 마이그레이션
    |           MigrateFromFishingStats(data.fishing)
    |       else:
    |           _entries.Clear()
    |           foreach (var entry in data.fishCatalog.entries):
    |               _entries[entry.fishId] = entry
    |               if entry.isDiscovered: _discoveredCount++
    +-- [55] InventoryManager ...
```

**구버전 세이브 마이그레이션**:
```
MigrateFromFishingStats(FishingSaveData fishingData):
    if fishingData == null: return
    foreach (kvp in fishingData.caughtByFishId):
        entry = new FishCatalogEntry(kvp.Key)
        entry.isDiscovered = true
        entry.totalCaught = kvp.Value
        entry.maxSizeCm = 0          // 구버전에 크기 기록 없음
        entry.firstCaughtDay = -1    // 구버전에 시점 기록 없음
        entry.firstCaughtSeason = -1
        entry.firstCaughtYear = -1
        _entries[kvp.Key] = entry
        _discoveredCount++
```

---

## 21. 도감 UI 아키텍처

### 21.1 FishCatalogUI (MonoBehaviour)

```
+--------------------------------------------------------------+
|              FishCatalogUI (MonoBehaviour)                     |
|--------------------------------------------------------------|
|  [참조]                                                       |
|  - _catalogManager: FishCatalogManager                        |
|  - _contentParent: Transform        (목록 아이템 부모)         |
|  - _itemPrefab: FishCatalogItemUI   (목록 항목 프리팹)         |
|  - _progressText: TMP_Text          (완성도 "8/15")           |
|  - _progressBar: Image              (완성도 게이지 fill)       |
|  - _detailPanel: FishCatalogDetailPanel  (선택 시 상세 표시)   |
|                                                              |
|  [상태]                                                       |
|  - _isOpen: bool                                              |
|  - _itemPool: List<FishCatalogItemUI>  (오브젝트 풀)          |
|  - _selectedFishId: string                                    |
|                                                              |
|  [메서드]                                                     |
|  + Open(): void                                               |
|       // 도감 UI 열기, RefreshAll() 호출                       |
|  + Close(): void                                              |
|  + RefreshAll(): void                                         |
|       // 전체 어종 목록 갱신 (15종)                            |
|  + RefreshEntry(string fishId): void                          |
|       // 개별 항목만 갱신 (OnCatalogUpdated 구독)              |
|  + SelectFish(string fishId): void                            |
|       // 상세 패널에 어종 정보 표시                             |
|                                                              |
|  [구독]                                                       |
|  + OnEnable():                                                |
|      FishCatalogManager.OnCatalogUpdated += RefreshEntry      |
|      FishCatalogManager.OnFishDiscovered += OnNewDiscovery    |
|  + OnDisable(): 구독 해제                                     |
+--------------------------------------------------------------+
```

### 21.2 FishCatalogItemUI (항목 프리팹)

```
+--------------------------------------------------------------+
|              FishCatalogItemUI (MonoBehaviour)                 |
|--------------------------------------------------------------|
|  [UI 요소]                                                    |
|  - _icon: Image                     // 어종 아이콘 (미발견: 실루엣) |
|  - _nameText: TMP_Text              // 어종 이름 (미발견: "???") |
|  - _rarityIcon: Image               // 희귀도 표시              |
|  - _caughtCountText: TMP_Text       // 포획 횟수               |
|  - _newBadge: GameObject             // "NEW!" 배지 (최초 발견) |
|                                                              |
|  [메서드]                                                     |
|  + SetData(FishCatalogData data,                              |
|       FishCatalogEntry entry): void                           |
|       // data != null, entry == null -> 미발견 표시             |
|       // data != null, entry != null -> 발견 표시              |
|  + OnClick(): void                                            |
|       // FishCatalogUI.SelectFish(fishId) 호출                |
+--------------------------------------------------------------+
```

### 21.3 FishCatalogDetailPanel (상세 패널)

```
+--------------------------------------------------------------+
|           FishCatalogDetailPanel (MonoBehaviour)               |
|--------------------------------------------------------------|
|  [UI 요소]                                                    |
|  - _fishIcon: Image                  // 큰 어종 이미지         |
|  - _nameText: TMP_Text               // 어종 이름              |
|  - _rarityText: TMP_Text             // 희귀도                 |
|  - _descriptionText: TMP_Text        // hintUnlocked 또는 hintLocked |
|  - _maxSizeText: TMP_Text            // "최대 크기: 48.7cm"    |
|  - _totalCaughtText: TMP_Text        // "포획 횟수: 12"        |
|  - _giantCaughtText: TMP_Text        // "Giant 포획: 1"        |
|  - _firstCaughtText: TMP_Text        // "최초 포획: 1년 봄 5일" |
|  - _seasonHintText: TMP_Text         // 출현 계절 힌트          |
|                                                              |
|  [메서드]                                                     |
|  + ShowFish(FishCatalogData data,                             |
|       FishCatalogEntry entry): void                           |
|       // 미발견: 실루엣 + hintLocked + "???" 표시              |
|       // 발견: 아이콘 + hintUnlocked + 상세 통계               |
|  + Hide(): void                                               |
+--------------------------------------------------------------+
```

### 21.4 FishCatalogToastUI (발견 알림)

새로운 어종 발견 시 화면 상단에 토스트 알림을 표시한다.

```
+--------------------------------------------------------------+
|            FishCatalogToastUI (MonoBehaviour)                  |
|--------------------------------------------------------------|
|  [UI 요소]                                                    |
|  - _toastPanel: CanvasGroup          // 페이드 인/아웃         |
|  - _fishIcon: Image                  // 발견된 어종 아이콘      |
|  - _messageText: TMP_Text            // "새 어종 발견! - 잉어" |
|  - _progressText: TMP_Text           // "도감 완성도: 8/15"    |
|                                                              |
|  [설정]                                                       |
|  - _displayDuration: float           // 토스트 표시 시간       |
|       // -> see docs/content/fish-catalog.md for 기본값       |
|  - _fadeSpeed: float                 // 페이드 속도            |
|                                                              |
|  [메서드]                                                     |
|  + ShowToast(FishCatalogData data, int discoveredCount,       |
|       int totalCount): void                                   |
|  - FadeCoroutine(): IEnumerator                               |
|                                                              |
|  [구독]                                                       |
|  + OnEnable():                                                |
|      FishingEvents.OnFishDiscovered += HandleDiscovery        |
|  + OnDisable(): 구독 해제                                     |
+--------------------------------------------------------------+
```

### 21.5 씬 계층 구조

```
Canvas (기존)
├── ... (기존 UI)
├── FishCatalogPanel (기본 비활성)
│   ├── Header
│   │   ├── TitleText ("어종 도감")
│   │   ├── ProgressText ("8/15")
│   │   ├── ProgressBar (Image, fill)
│   │   └── CloseButton
│   ├── CatalogGrid (GridLayoutGroup)
│   │   └── FishCatalogItemUI (프리팹, 15개 인스턴스)
│   └── DetailPanel (FishCatalogDetailPanel)
│       ├── FishIcon
│       ├── NameText
│       ├── RarityText
│       ├── DescriptionText
│       ├── MaxSizeText
│       ├── TotalCaughtText
│       ├── GiantCaughtText
│       ├── FirstCaughtText
│       └── SeasonHintText
└── FishCatalogToast (기본 비활성, 화면 상단)
    ├── FishIcon
    ├── MessageText
    └── ProgressText
```

---

## 22. 프로젝트 폴더 구조 확장

```
Assets/_Project/Scripts/Fishing/
├── ... (기존 파일)
├── Catalog/                              # SeedMind.Fishing.Catalog 네임스페이스
│   ├── FishCatalogData.cs                # ScriptableObject (어종 도감 정적 데이터)
│   ├── FishCatalogEntry.cs               # 런타임 상태 클래스
│   ├── FishCatalogManager.cs             # 도감 매니저 (Singleton, ISaveable)
│   ├── FishCatalogSaveData.cs            # 세이브 데이터 클래스
│   └── UI/
│       ├── FishCatalogUI.cs              # 도감 메인 패널
│       ├── FishCatalogItemUI.cs          # 도감 항목 프리팹
│       ├── FishCatalogDetailPanel.cs     # 상세 정보 패널
│       └── FishCatalogToastUI.cs         # 발견 토스트 알림

Assets/_Project/Data/Fishing/
├── ... (기존 FishData SO)
├── Catalog/                              # FishCatalogData SO 에셋
│   ├── FishCatalog_CrucianCarp.asset
│   ├── FishCatalog_Loach.asset
│   ├── FishCatalog_Carp.asset
│   └── ... (15종)
```

(-> see `docs/systems/project-structure.md` for 네임스페이스 규칙)

---

## 23. MCP 태스크 시퀀스 (ARC-030)

### Phase G: 도감 데이터 레이어

| Step | MCP 명령 | 설명 |
|------|---------|------|
| G-1 | CreateScript `FishCatalogData.cs` : ScriptableObject | SeedMind.Fishing.Catalog 네임스페이스, 섹션 15 스키마 |
| G-2 | CreateScript `FishCatalogEntry.cs` | 런타임 상태 클래스, 섹션 17 |
| G-3 | CreateScript `FishCatalogSaveData.cs` | 세이브 데이터 클래스, 섹션 20.1 |

### Phase H: 도감 시스템 레이어

| Step | MCP 명령 | 설명 |
|------|---------|------|
| H-1 | CreateScript `FishCatalogManager.cs` : MonoBehaviour | Singleton, ISaveable(order=53), 섹션 16 |
| H-2 | EditScript `FishingEvents.cs` | OnFishCaughtWithSize, OnFishDiscovered, OnCatalogMilestoneReached, OnCatalogCompleted 추가 |
| H-3 | EditScript `FishingManager.cs` | CompleteFishing에 SizeRoll + FishCatalogManager.RegisterCatch 호출 추가, 섹션 18.3 |

### Phase I: 도감 SO 에셋 생성

| Step | MCP 명령 | 설명 |
|------|---------|------|
| I-1 | CreateAsset `FishCatalog_CrucianCarp` 등 15종 | FishCatalogData SO 에셋, 수치는 `(-> see docs/content/fish-catalog.md)` |

### Phase J: 도감 UI 프리팹

| Step | MCP 명령 | 설명 |
|------|---------|------|
| J-1 | CreateScript `FishCatalogUI.cs` | 메인 패널, 섹션 21.1 |
| J-2 | CreateScript `FishCatalogItemUI.cs` | 항목 프리팹, 섹션 21.2 |
| J-3 | CreateScript `FishCatalogDetailPanel.cs` | 상세 패널, 섹션 21.3 |
| J-4 | CreateScript `FishCatalogToastUI.cs` | 발견 토스트, 섹션 21.4 |
| J-5 | CreatePrefab `FishCatalogPanel` | 씬 계층 구성, 섹션 21.5 |
| J-6 | CreatePrefab `FishCatalogToast` | 토스트 알림 프리팹 |

### Phase K: 기존 시스템 확장

| Step | MCP 명령 | 설명 |
|------|---------|------|
| K-1 | EditScript `GameSaveData.cs` | `public FishCatalogSaveData fishCatalog;` 추가 |
| K-2 | EditScript `SaveManager.cs` | FishCatalogManager ISaveable 등록 확인, SaveLoadOrder=53 |
| K-3 | CreateGameObject `FishCatalogManager` | 싱글턴 오브젝트, FishCatalogData[] 할당 |

### Phase L: 도감 검증

| Step | 검증 내용 |
|------|----------|
| L-1 | FishCatalogManager 싱글턴 정상 생성 확인 (MCP Console.Log) |
| L-2 | 물고기 포획 시 FishCatalogEntry 생성 및 isDiscovered=true 확인 |
| L-3 | 동일 어종 재포획 시 totalCaught 증가 + maxSizeCm 갱신 확인 |
| L-4 | Giant 변이 포획 시 giantCaught 증가 + 크기 보정 적용 확인 |
| L-5 | 마일스톤 도달 시 OnMilestoneReached 이벤트 발행 확인 |
| L-6 | 도감 100% 완성 시 OnCatalogCompleted 발행 확인 |
| L-7 | 세이브 -> 로드 후 FishCatalogSaveData 복원 확인 (discoveredCount, entries) |
| L-8 | 구버전 세이브(fishCatalog=null) 로드 시 FishingStats에서 마이그레이션 확인 |
| L-9 | FishCatalogUI 열기 -> 15종 표시 (발견: 아이콘+이름, 미발견: 실루엣+"???") 확인 |
| L-10 | 발견 토스트 UI 표시 + 자동 소멸 확인 |

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
| `docs/systems/time-season-architecture.md` (ARC-007) | 섹션 4.4 OnSeasonChanged 구독자 순서 | FishingManager priority 55 등록 (FIX-071) |
| `docs/systems/project-structure.md` | 네임스페이스 규칙 | SeedMind.Fishing 배치 |
| `docs/systems/fishing-system.md` (DES-013) | 어종 목록, 가격, 밸런스 | FishData canonical 수치 |
| `docs/systems/fishing-system.md` 섹션 7 | 숙련도 레벨 테이블, XP 획득, 보정 요약 | FishingProficiency canonical (ARC-029) |
| `docs/balance/progression-curve.md` | 낚시 XP 값 | FishingCatch XP canonical — 섹션 1.2.7에 등록 완료 (FIX-064) |
| `docs/balance/xp-integration.md` | XP 배분 구조 | 낚시 독립 숙련도 분리 근거 (ARC-029) |
| `docs/content/fish-catalog.md` (CON-011) | 어종별 sizeMinCm/sizeMaxCm, 크기 등급 규칙, 마일스톤 보상, 힌트 텍스트 — FishCatalogData canonical 수치 (ARC-030). [FIX-075 적용 완료: sizeMinCm/sizeMaxCm 통일, hintLocked/hintUnlocked 통일, firstCatchGold/XP 추가] |
| `docs/systems/achievement-architecture.md` (ARC-024) | 섹션 3 AchievementManager 패턴 | FishCatalogManager 수집 패턴 참조 (ARC-030) |
| `docs/systems/save-load-architecture.md` (ARC-011) | 섹션 2.1~2.3 GameSaveData | FishCatalogSaveData 필드 추가 (ARC-030) |

---

## Open Questions

1. [RESOLVED] `docs/systems/fishing-system.md` (DES-013) 작성 완료. 어종 목록 15종, 기본 판매가, 희귀도별 출현 가중치, 미니게임 난이도 파라미터 확정. basePrice 및 expReward 등 일부 밸런스 수치는 `docs/balance/progression-curve.md` canonical 등록 후 FishData SO에 반영 필요.
2. [RESOLVED-FIX-064] 낚시 XP 계산 공식 확정: `floor(fishData.expReward * qualityExpBonus[quality])`. rarity 기반(Common=10, Uncommon=20, Rare=40, Legendary=80). canonical 등록 완료 (-> see `docs/balance/progression-curve.md` 섹션 1.2.7).
3. [RESOLVED-FIX-053] `ItemType` enum에 `Fish` 값 추가 완료. data-pipeline.md, inventory-architecture.md 섹션 3.2, inventory-tasks.md Step 1-01에 반영.
4. [OPEN] 물고기 전용 가격 변동 시스템 분리 여부. 초기 버전은 기존 PriceFluctuationSystem 공유, 추후 분리 검토.
5. [OPEN] 낚시 에너지 소모량 미확정. 캐스팅 1회당 에너지 소모를 `(-> see docs/systems/farming-system.md 섹션 3.2)` 기존 도구 사용 에너지와 동일 레벨로 할지 별도 설정할지 결정 필요.
6. [OPEN] 날씨 보너스 배율(잠정 1.5)이 미확정. fishing-system.md에서 canonical 값을 설정해야 한다.
7. [OPEN] FishingPoint 일일 사용 제한 횟수 미확정. 무제한으로 할지, 포인트당 일일 제한을 둘지 결정 필요.
8. [OPEN] 낚싯대 도구 요건 미확정. 기존 ToolType enum에 FishingRod를 추가해야 하며, 도구 업그레이드 경로도 설계 필요.
9. [RESOLVED-CON-011] (ARC-030) 도감 마일스톤 보상 확정: 5종(100G+30XP+미끼x10), 10종(300G+80XP+고급미끼x5+도감배경"연못의사계절"), 15종(500G+150XP+도감배경"전설의연못"+프레임"황금물결") (-> see `docs/content/fish-catalog.md` 섹션 4.1).
10. [OPEN] (ARC-030) FishCatalogData SO와 FishData SO의 1:1 관계 유지 방안. 현재 fishId로 연결하지만, SO 참조(직접 FishData 필드)로 변경하면 Inspector에서 관리가 편해진다. 구현 시 결정 필요.
11. [OPEN] (ARC-030) 크기 판매가 보정(sizePriceMul)을 InventorySlot.metadata에 저장할지, 판매 시점에 재계산할지 결정 필요. metadata 방식이면 인벤토리 아키텍처 확장 필요.

---

## Risks

1. [RESOLVED-FIX-049] **HarvestOrigin enum 확장 파급**: `Fishing = 3`이 economy-architecture.md 섹션 3.10.2에 추가되었고, `GetGreenhouseMultiplier()` pseudocode(섹션 3.10.3)에도 Fishing case가 반영되었다.
2. [RESOLVED-FIX-050/부분] **XPSource enum 확장 파급**: `FishingCatch`가 progression-architecture.md 섹션 2.2 XPSource enum 및 섹션 2.3 `GetExpForSource()` switch 문에 추가되었다. 단, ProgressionData SO의 `fishingCatchExp` 필드는 FishData.expReward 기반 계산으로 대체되므로 별도 필드 불필요 — progression-architecture.md 섹션 2.1 클래스 정의에 주석으로 명시됨.
3. [RISK] **SaveLoadOrder 52 충돌 가능성**: PlayerController(50)와 InventoryManager(55) 사이의 좁은 간격에 배치. 향후 다른 시스템이 51~54 범위를 사용하면 충돌 가능. save-load-architecture.md 할당표를 반드시 갱신해야 한다.
4. [RISK] **FishData SO와 IInventoryItem 통합**: FishData가 GameDataSO를 상속하고 IInventoryItem을 구현해야 하므로, 기존 IInventoryItem 인터페이스에 Fish 타입이 누락되면 런타임 오류 발생.
5. [RISK] **미니게임 프레임 의존성**: FishingMinigame.Update()가 매 프레임 호출되므로, deltaTime 기반 보정이 없으면 프레임레이트에 따라 난이도가 달라질 수 있다. 모든 시간 계산에 deltaTime을 반드시 적용해야 한다.
6. [RISK] **숙련도 보정 누적 효과 (ARC-029)**: biteDelayMultiplier(-30%), rarityBonus(x1.2), doubleCatch(5%), energyReduction(-1)이 Lv.10에서 동시 적용되면 시간당 수익이 과도해질 수 있다. fishing-system.md 섹션 7의 밸런스 시트(BAL)와 연계하여 정기 검증 필요.
7. [RISK] **숙련도 XP 리셋 불가 (ARC-029)**: 현재 설계에는 숙련도 XP/레벨 리셋 메커니즘이 없다. 밸런스 조정 시 기존 세이브에 소급 적용할 방법을 사전에 고려해야 한다.
8. [RISK] **얼음 구멍 dayOfSeason 기반 만료 계산 (FIX-071)**: `createdDay + iceHoleDuration`이 계절 일수를 초과하는 경우(예: 겨울 마지막 날에 생성) 처리가 필요하다. 단, 겨울 종료 시 `RemoveAllIceHoles()`가 호출되므로 실질적 문제는 없으나, 엣지 케이스 테스트는 필요하다.
9. [RISK] **FishingSaveData 필드 수 증가 (FIX-071)**: `activeIceHoles` 추가로 JSON 10개 + C# 10개. 기존 세이브 파일과의 하위 호환성을 위해 `activeIceHoles`가 null일 때 빈 리스트로 초기화하는 방어 로직이 LoadSaveData()에 필요하다.
10. [RISK] **(ARC-030) SaveLoadOrder 52~53 밀집**: FishingManager(52)와 FishCatalogManager(53)이 연속 배치되어 InventoryManager(55) 이전의 간격이 1로 줄어든다. 향후 낚시 관련 시스템이 추가되면 54 하나만 남는다.
11. [RISK] **(ARC-030) FishCatalogData와 FishData 동기화**: 두 SO가 동일한 fishId를 공유하므로, 어종 추가/삭제 시 양쪽 SO를 반드시 동시에 관리해야 한다. 한쪽만 갱신하면 도감에 빈 항목이 표시되거나 누락될 수 있다.
12. [RISK] **(ARC-030) 구버전 세이브 마이그레이션 한계**: `MigrateFromFishingStats`는 caughtByFishId에서 발견 여부만 복원하며, maxSizeCm과 firstCaught 시점 정보는 복구 불가. 구버전 세이브 사용자에게 도감의 크기 기록과 최초 발견 시점이 -1로 표시되는 것이 수용 가능한지 확인 필요.
13. [RISK] **(ARC-030) FishCatalogEntry 내 totalCaught와 FishingStats.caughtByFishId 중복**: 동일한 정보가 두 곳에 저장된다. 불일치 시 도감과 통계가 다른 값을 보여줄 수 있다. FishCatalogManager.RegisterCatch에서만 양쪽 값을 갱신하도록 단일 진입점을 강제해야 한다.

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
| FIX-063 | `inventory-architecture.md` | 섹션 4에 FishData IInventoryItem 구현 예시 추가 (섹션 4.4) | RESOLVED |
| FIX-064 | `fishing-architecture.md`, `balance/progression-curve.md` | 낚시 XP 계산 공식 확정, 섹션 6.2 업데이트 + progression-curve.md 섹션 1.2.7 canonical 등록 | RESOLVED |
| FIX-069 | `fishing-system.md`, `fishing-architecture.md` | 낚시 포인트 수(20개소 vs 3개) 개념 차이 명확화 — 물리적 위치 vs FishingPoint 오브젝트 구분 설명 추가 | RESOLVED |
| FIX-071 | `fishing-architecture.md`, `time-season-architecture.md` | 겨울 얼음 낚시 아키텍처 반영: IceHoleData 클래스, FishingManager 얼음 구멍 관리 메서드, OnSeasonChanged 구독자 등록, MCP 태스크 D2, 세이브/로드 확장 | RESOLVED |
| FIX-072(b) | `save-load-architecture.md` | 섹션 2.1~2.3 GameSaveData에 `fishCatalog: FishCatalogSaveData` 필드 추가, 섹션 7 SaveLoadOrder에 `FishCatalogManager \| 53` 행 추가 — ARC-030 작업 중 완료됨. ID 충돌: TODO.md FIX-072는 별도 항목(economy-system.md). | RESOLVED (이번 세션) |
| FIX-073 | `data-pipeline.md` | Part I GameSaveData JSON 스키마 및 C# 클래스에 `fishCatalog` 필드 추가 | OPEN |
| FIX-074 | `docs/content/fish-catalog.md` (신규) | CON-011에서 fish-catalog.md 작성 완료. OPEN/CONFLICT 해소됨. | RESOLVED (CON-011) |
| FIX-075 | `docs/systems/fishing-architecture.md` 섹션 15, 18 + `docs/content/fish-catalog.md` 섹션 7 | 크기 데이터 모델 통일 완료: sizeMinCm/sizeMaxCm 절대값 채택, hintLocked/hintUnlocked 통일, firstCatchGold/XP 추가, GetSizePriceMultiplier 3등급 이산 로직 교체. | RESOLVED |
