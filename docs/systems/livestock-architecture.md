# 목축/낙농 시스템 기술 아키텍처

> AnimalManager, AnimalData SO, AnimalInstance 런타임, 행복도(Happiness) 시스템, Zone E 연동, XP 통합, 세이브/로드 확장, MCP 구현 태스크 개요  
> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: ARC-019

---

## Context

목축/낙농 시스템은 플레이어가 동물을 구매하고, 매일 먹이와 애정을 주며, 생산물(우유, 달걀, 양털 등)을 수확하는 시스템이다. 농장 확장 Zone E(`zone_south_meadow`, ZoneType.Pasture)가 해금된 후 활성화되며, 외양간(Barn) 건설을 통해 동물 수용 능력을 확보한다.

동물 종류, 구매 가격, 먹이 소모량, 생산물 종류, 행복도 수치 등 모든 콘텐츠 데이터의 canonical 출처는 `docs/content/livestock-system.md` (CON-006)이다. **이 아키텍처 문서에서는 수치를 직접 기재하지 않으며, 모든 데이터 참조는 canonical 문서를 가리킨다.**

**기존 시스템과의 관계**:
- **FarmZoneManager** (farm-expansion-architecture.md, ARC-023): Zone E 해금 시 AnimalManager 초기화 트리거
- **EconomyManager** (economy-architecture.md): 동물 구매 골드 차감(`SpendGold`), 생산물 판매(`SellItem`)
- **ProgressionManager** (progression-architecture.md, BAL-002): XPSource 확장, AddExp() 호출
- **TimeManager** (time-season-architecture.md): OnDayChanged 이벤트 구독, 일일 돌봄 사이클
- **InventoryManager**: 생산물 아이템 추가, 사료 아이템 차감
- **HarvestOrigin** (economy-architecture.md 섹션 3.10): 동물 생산물에 대한 origin 추적 확장

---

## Part I -- 아키텍처 설계

---

### 1. 클래스 다이어그램

```
┌─────────────────────────────────────────────────────────────────────┐
│                       SeedMind.Livestock                            │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│          AnimalManager (MonoBehaviour, Singleton)              │
│──────────────────────────────────────────────────────────────│
│  [상태]                                                       │
│  - _animals: List<AnimalInstance>                             │
│  - _barnLevel: int             // 외양간 레벨 (0 = 미건설)   │
│  - _barnCapacity: int          // 외양간 수용 한계 (중/대형) │
│  - _coopLevel: int             // 닭장 레벨 (0 = 미건설)     │
│  - _coopCapacity: int          // 닭장 수용 한계 (닭 전용)   │
│  - _isUnlocked: bool           // Zone E 해금 여부           │
│                                                              │
│  [설정 참조]                                                   │
│  - _animalDataRegistry: AnimalData[]   // 전체 동물 SO 목록  │
│  - _livestockConfig: LivestockConfig (ScriptableObject)      │
│                                                              │
│  [읽기 전용 프로퍼티]                                           │
│  + Animals: IReadOnlyList<AnimalInstance>                     │
│  + BarnLevel: int                                            │
│  + BarnCapacity: int                                         │
│  + CoopLevel: int                                            │
│  + CoopCapacity: int                                         │
│  + CurrentCount: int                                         │
│  + IsUnlocked: bool                                          │
│                                                              │
│  [이벤트]                                                     │
│  + OnAnimalAdded: Action<AnimalInstance>                      │
│  + OnAnimalFed: Action<AnimalInstance>                        │
│  + OnAnimalPetted: Action<AnimalInstance>                     │
│  + OnProductReady: Action<AnimalInstance, AnimalProductInfo>  │
│  + OnProductCollected: Action<AnimalInstance, ItemData, int>  │
│    // UI 구독용 인스턴스 이벤트 — ItemData 포함               │
│    // 경량 시스템(XP/퀘스트) 구독 → LivestockEvents.OnProductCollected 사용│
│  + OnBarnUpgraded: Action<int>           // newLevel         │
│  + OnCoopUpgraded: Action<int>           // newLevel         │
│                                                              │
│  [메서드]                                                     │
│  + Initialize(LivestockConfig config): void                  │
│  + UnlockBarn(): void                    // Zone E 해금 시   │
│  + UpgradeBarn(): bool                   // 외양간 업그레이드│
│  + UpgradeCoop(): bool                   // 닭장 업그레이드  │
│  + HandleCoopBuilt(): void               // 닭장 건설 콜백   │
│  + TryBuyAnimal(AnimalData data): bool                       │
│  + FeedAnimal(AnimalInstance animal): bool                    │
│  + PetAnimal(AnimalInstance animal): void                     │
│  + CollectProduct(AnimalInstance animal): CollectResult       │
│  + DailyUpdate(): void                   // TimeManager 구독 │
│  + GetAnimalById(string instanceId): AnimalInstance           │
│  + GetSaveData(): AnimalSaveData                             │
│  + LoadSaveData(AnimalSaveData data): void                   │
│  - ProcessDailyHappiness(AnimalInstance animal): void         │
│  - ProcessDailyProduction(AnimalInstance animal): void        │
│  - CheckAutoFeed(AnimalInstance animal): void                 │
│                                                              │
│  [구독]                                                       │
│  + OnEnable():                                               │
│      TimeManager.RegisterOnDayChanged(priority: 55)          │
│      FarmZoneManager.OnZoneUnlocked += HandleZoneUnlocked    │
│  + OnDisable(): 구독 해제                                      │
│                                                              │
│  [ISaveable 구현]                                              │
│  + SaveLoadOrder => 48                                       │
│  + GetSaveData(): object                                     │
│  + LoadSaveData(object data): void                           │
└──────────────────────────────────────────────────────────────┘
         │ owns                          │ ref
         ▼                               ▼
┌────────────────────────┐     ┌──────────────────────────────┐
│  AnimalInstance         │     │  AnimalData (SO)             │
│  (Plain C# class)      │     │──────────────────────────────│
│                        │     │  animalId: string            │
│  (아래 섹션 3 참조)    │     │  animalName: string          │
│                        │     │  animalType: AnimalType      │
│                        │     │  (아래 섹션 2 참조)          │
└────────────────────────┘     └──────────────────────────────┘
         │ ref                           │ ref
         ▼                               ▼
┌────────────────────────┐     ┌──────────────────────────────┐
│  HappinessCalculator   │     │  LivestockConfig (SO)        │
│  (Plain C# class)      │     │──────────────────────────────│
│                        │     │  (아래 섹션 5 참조)          │
│  (아래 섹션 5 참조)    │     │                              │
└────────────────────────┘     └──────────────────────────────┘
```

**TimeManager 우선순위 근거**: AnimalManager의 DayChanged 구독 우선순위는 55이다. FarmGrid(40), FarmZoneManager(45) 이후이므로 구역/타일 상태가 확정된 후에 동물 일일 처리를 수행한다. PlayerController(50) 이후이므로 전날의 플레이어 행동(먹이, 애정) 결과를 반영할 수 있다.

---

### 2. AnimalData ScriptableObject

동물 종류별 정적 데이터를 정의하는 ScriptableObject이다. 모든 수치의 canonical 출처는 CON-006이다.

```csharp
// illustrative
namespace SeedMind.Livestock.Data
{
    [CreateAssetMenu(menuName = "SeedMind/Livestock/AnimalData")]
    public class AnimalData : ScriptableObject
    {
        [Header("기본 정보")]
        public string animalId;           // 고유 식별자 (예: "chicken", "cow")
        public string animalName;         // 표시 이름
        public AnimalType animalType;     // enum 분류

        [Header("구매/비용")]
        public int purchasePrice;         // 구매 가격 (→ see docs/content/livestock-system.md 섹션 2)
        public string requiredFeedId;     // 필요한 사료 아이템 ID
        public int dailyFeedAmount;       // 일일 사료 소모량 (→ see docs/content/livestock-system.md 섹션 3)

        [Header("생산")]
        public string productItemId;      // 생산물 아이템 ID
        public int productionIntervalDays;// 생산 주기 (일) (→ see docs/content/livestock-system.md 섹션 4)
        public int baseProductAmount;     // 기본 생산량 (→ see docs/content/livestock-system.md 섹션 4)

        [Header("행복도")]
        public int baseHappinessDecay;    // 일일 기본 감소량 (→ see docs/content/livestock-system.md 섹션 5)
        public int feedHappinessGain;     // 먹이 급여 시 증가량 (→ see docs/content/livestock-system.md 섹션 5)
        public int petHappinessGain;      // 쓰다듬기 시 증가량 (→ see docs/content/livestock-system.md 섹션 5)

        [Header("해금")]
        public int unlockLevel;           // 해금 레벨 (→ see docs/content/livestock-system.md 섹션 2)

        [Header("프리팹")]
        public GameObject animalPrefab;   // 동물 외형 프리팹
    }
}
```

#### 2.1 AnimalType enum

```csharp
// illustrative
namespace SeedMind.Livestock
{
    /// <summary>
    /// 동물 분류. 시설 요구사항 및 생산물 카테고리를 결정한다.
    /// 동물 종류 목록은 (→ see docs/content/livestock-system.md 섹션 1).
    /// </summary>
    public enum AnimalType
    {
        Poultry,      // 가금류 (닭, 오리 등) -- 닭장(Coop) 필요
        Cattle,       // 소류 -- 외양간(Barn) 필요
        SmallAnimal   // 소형 동물 (양, 염소 등) -- 외양간(Barn) 필요
        // [OPEN] 향후 확장: Horse, Pet 등
    }
}
```

**파일 위치**: `Assets/_Project/Scripts/Livestock/Data/AnimalData.cs`

---

### 3. AnimalInstance (런타임 상태)

각 동물 개체의 런타임 상태를 추적하는 Plain C# 클래스이다. AnimalData SO를 참조하되, 변동 가능한 상태만 보유한다.

```csharp
// illustrative
namespace SeedMind.Livestock
{
    [System.Serializable]
    public class AnimalInstance
    {
        // --- 식별 ---
        public string instanceId;          // GUID 기반 고유 ID
        public string animalDataId;        // AnimalData.animalId 참조

        // --- 런타임 상태 ---
        public float happiness;            // 0 ~ 200 (→ see docs/content/livestock-system.md 섹션 5)
        public int daysSinceLastFed;       // 마지막 먹이 급여 이후 경과일
        public int daysSinceLastPetted;    // 마지막 쓰다듬기 이후 경과일
        public bool isFedToday;            // 오늘 먹이 급여 완료 여부
        public bool isPettedToday;         // 오늘 쓰다듬기 완료 여부

        // --- 생산 ---
        public int daysSinceLastProduct;   // 마지막 생산 이후 경과일
        public bool isProductReady;        // 수확 가능 여부
        public int productQuality;         // 현재 생산물 품질 (행복도 기반)

        // --- 메타 ---
        public int purchaseDay;            // 구매 시점 (TimeManager.TotalElapsedDays)
        public string displayName;         // 플레이어가 지정한 이름 (nullable)

        // --- 참조 (런타임에만, 직렬화 제외) ---
        [System.NonSerialized]
        public AnimalData data;            // SO 참조 (로드 시 animalDataId로 복원)
    }
}
```

#### 3.1 AnimalInstance 생성 흐름

```
TryBuyAnimal(AnimalData data)
    │
    ├── 1) 수용 가능 여부 확인 (동물 타입별):
    │       if data.animalType == AnimalType.Poultry:
    │           if CurrentCount(Poultry) >= _coopCapacity: return false
    │       else:
    │           if CurrentCount(NonPoultry) >= _barnCapacity: return false
    │
    ├── 2) 골드 차감: EconomyManager.SpendGold(data.purchasePrice, "BuyAnimal")
    │       → 실패 시 false 반환
    │
    ├── 3) AnimalInstance 생성
    │       instanceId = System.Guid.NewGuid().ToString()
    │       animalDataId = data.animalId
    │       happiness = 초기 행복도  // → see docs/content/livestock-system.md 섹션 5
    │       daysSinceLastFed = 0
    │       daysSinceLastPetted = 0
    │       daysSinceLastProduct = 0
    │       isProductReady = false
    │       purchaseDay = TimeManager.TotalElapsedDays
    │       data = SO 참조
    │
    ├── 4) _animals.Add(instance)
    │
    ├── 5) 이벤트 발행: OnAnimalAdded?.Invoke(instance)
    │       → LivestockEvents.RaiseAnimalPurchased(instance)
    │
    └── 6) return true
```

---

### 4. AnimalManager 핵심 로직

#### 4.1 DailyUpdate() -- 일일 사이클

TimeManager.OnDayChanged 이벤트 구독(priority: 55)으로 매일 호출된다.

```
DailyUpdate()   // TimeManager.OnDayChanged 핸들러
    │
    ├── foreach animal in _animals:
    │
    │   ├── 1) 행복도 처리: ProcessDailyHappiness(animal)
    │   │       → 섹션 5 참조
    │   │
    │   ├── 2) 생산 처리: ProcessDailyProduction(animal)
    │   │       → 아래 4.2 참조
    │   │
    │   ├── 3) 일일 플래그 리셋
    │   │       animal.isFedToday = false
    │   │       animal.isPettedToday = false
    │   │
    │   └── 4) 경과일 카운터 갱신
    │           if !animal.isFedToday: animal.daysSinceLastFed++
    │           if !animal.isPettedToday: animal.daysSinceLastPetted++
    │
    └── end foreach
```

**주의**: DailyUpdate는 날이 바뀌기 **직전**에 호출된다. 즉, 당일의 isFedToday/isPettedToday 상태를 기준으로 행복도를 계산한 후, 다음 날을 위해 플래그를 리셋한다.

#### 4.2 ProcessDailyProduction()

```
ProcessDailyProduction(AnimalInstance animal)
    │
    ├── 1) 이미 productReady이면 → 스킵 (수확 대기 중)
    │
    ├── 2) daysSinceLastProduct++
    │
    ├── 3) if daysSinceLastProduct >= animal.data.productionIntervalDays:
    │       │
    │       ├── isProductReady = true
    │       ├── productQuality = HappinessCalculator.GetProductQuality(animal.happiness)
    │       │       // → see docs/content/livestock-system.md 섹션 5.3 (행복도-생산 연동)
    │       │
    │       └── OnProductReady?.Invoke(animal, productInfo)
    │
    └── end
```

#### 4.3 FeedAnimal()

```csharp
// illustrative
public bool FeedAnimal(AnimalInstance animal)
{
    if (animal.isFedToday) return false;  // 이미 급여

    // 인벤토리에서 사료 차감
    string feedId = animal.data.requiredFeedId;
    int feedAmount = animal.data.dailyFeedAmount;
    // → see docs/content/livestock-system.md 섹션 3

    if (!InventoryManager.Instance.RemoveItem(feedId, feedAmount))
        return false;  // 사료 부족

    animal.isFedToday = true;
    animal.daysSinceLastFed = 0;

    // 행복도 증가
    float gain = animal.data.feedHappinessGain;
    // → see docs/content/livestock-system.md 섹션 5
    animal.happiness = Mathf.Clamp(animal.happiness + gain, 0f, 200f);

    OnAnimalFed?.Invoke(animal);
    LivestockEvents.RaiseAnimalFed(animal);

    return true;
}
```

#### 4.4 PetAnimal()

```csharp
// illustrative
public void PetAnimal(AnimalInstance animal)
{
    if (animal.isPettedToday) return;  // 1일 1회 제한

    animal.isPettedToday = true;
    animal.daysSinceLastPetted = 0;

    float gain = animal.data.petHappinessGain;
    // → see docs/content/livestock-system.md 섹션 5
    animal.happiness = Mathf.Clamp(animal.happiness + gain, 0f, 200f);

    // XP 부여
    ProgressionManager.Instance.AddExp(
        ProgressionManager.Instance.GetExpForSource(XPSource.AnimalCare, animal),
        XPSource.AnimalCare
    );

    OnAnimalPetted?.Invoke(animal);
    LivestockEvents.RaiseAnimalPetted(animal);
}
```

#### 4.5 CollectProduct()

```csharp
// illustrative
public CollectResult CollectProduct(AnimalInstance animal)
{
    if (!animal.isProductReady)
        return CollectResult.NotReady;

    // 생산량 = 기본량 * 행복도 배수
    int amount = Mathf.RoundToInt(
        animal.data.baseProductAmount
        * HappinessCalculator.GetProductionMultiplier(animal.happiness)
    );
    // → see docs/content/livestock-system.md 섹션 4, 6

    // 인벤토리에 생산물 추가
    bool added = InventoryManager.Instance.AddItem(
        animal.data.productItemId,
        amount,
        (CropQuality)animal.productQuality,
        HarvestOrigin.Barn   // [OPEN] HarvestOrigin 확장 필요 -- 섹션 9 참조
    );

    if (!added)
        return CollectResult.InventoryFull;

    // 상태 리셋
    animal.isProductReady = false;
    animal.daysSinceLastProduct = 0;
    animal.productQuality = 0;

    // XP 부여
    ProgressionManager.Instance.AddExp(
        ProgressionManager.Instance.GetExpForSource(XPSource.AnimalHarvest, animal),
        XPSource.AnimalHarvest
    );

    OnProductCollected?.Invoke(animal, itemData, amount);
    LivestockEvents.RaiseProductCollected(animal, amount);

    return CollectResult.Success;
}
```

#### 4.6 CollectResult enum

```csharp
// illustrative
namespace SeedMind.Livestock
{
    public enum CollectResult
    {
        Success,
        NotReady,
        InventoryFull
    }
}
```

---

### 5. 행복도(Happiness) 시스템

행복도는 0 ~ 200 범위의 float 값으로, 동물의 돌봄 상태를 종합적으로 나타낸다. 행복도는 생산물 품질과 생산량에 직접 영향을 준다.

#### 5.1 HappinessCalculator (Pure C# 유틸리티)

```csharp
// illustrative
namespace SeedMind.Livestock
{
    /// <summary>
    /// 행복도 관련 계산을 담당하는 순수 유틸리티 클래스.
    /// 모든 수치 임계값은 LivestockConfig SO에서 로드한다.
    /// </summary>
    public static class HappinessCalculator
    {
        /// <summary>
        /// 일일 행복도 변동을 계산한다.
        /// 행복도 증감 수치는 (→ see docs/content/livestock-system.md 섹션 5).
        /// </summary>
        public static float CalculateDailyDelta(
            AnimalInstance animal,
            LivestockConfig config)
        {
            float delta = 0f;

            // 기본 감소 (매일)
            delta -= animal.data.baseHappinessDecay;
            // → see docs/content/livestock-system.md 섹션 5

            // 먹이 급여 보너스 (당일 급여 완료 시)
            if (animal.isFedToday)
                delta += animal.data.feedHappinessGain;

            // 쓰다듬기 보너스 (당일 완료 시)
            if (animal.isPettedToday)
                delta += animal.data.petHappinessGain;

            // 연속 미급여 페널티
            if (animal.daysSinceLastFed > config.neglectThresholdDays)
                delta -= config.neglectPenaltyPerDay;
            // → see docs/content/livestock-system.md 섹션 5

            return delta;
        }

        /// <summary>
        /// 행복도에 따른 생산량 배수를 반환한다.
        /// 배수 테이블은 (→ see docs/content/livestock-system.md 섹션 5.3).
        /// </summary>
        public static float GetProductionMultiplier(float happiness)
        {
            // happiness 구간별 배수는 LivestockConfig에서 AnimationCurve로 정의
            // → see docs/content/livestock-system.md 섹션 5.3 (행복도 구간별 배수)
            return LivestockConfig.Instance.productionMultiplierCurve.Evaluate(
                happiness / 200f  // 0~1 정규화 (happiness 최대 200, → see docs/content/livestock-system.md 섹션 5)
            );
        }

        /// <summary>
        /// 행복도에 따른 생산물 품질을 결정한다.
        /// 품질 매핑은 (→ see docs/content/livestock-system.md 섹션 5.3).
        /// </summary>
        public static int GetProductQuality(float happiness)
        {
            // 행복도 → CropQuality 매핑
            // → see docs/content/livestock-system.md 섹션 5.3 (행복도-생산 연동 테이블)
            // 임계값은 LivestockConfig SO에서 로드하여 사용 (hardcode 금지)
            if (happiness >= LivestockConfig.Instance.goldQualityThreshold) return (int)CropQuality.Gold;
            if (happiness >= LivestockConfig.Instance.silverQualityThreshold) return (int)CropQuality.Silver;
            return (int)CropQuality.Normal;
        }
    }
}
```

[RISK] HappinessCalculator.GetProductQuality()에서 CropQuality enum을 재사용하고 있다. 동물 생산물 품질이 작물 품질과 완전히 동일한 체계를 사용할지, 별도 ProductQuality enum이 필요할지는 콘텐츠 설계(CON-006) 확정 후 결정한다.

#### 5.2 LivestockConfig ScriptableObject

행복도 계산 및 시스템 전역 설정을 위한 SO이다.

```csharp
// illustrative
namespace SeedMind.Livestock.Data
{
    [CreateAssetMenu(menuName = "SeedMind/Livestock/LivestockConfig")]
    public class LivestockConfig : ScriptableObject
    {
        [Header("외양간")]
        public int initialBarnCapacity;      // 초기 수용 한계 (→ see docs/content/livestock-system.md)
        public int[] barnUpgradeCapacity;    // 레벨별 수용 한계 (→ see docs/content/livestock-system.md)
        public int[] barnUpgradeCost;        // 레벨별 업그레이드 비용 (→ see docs/content/livestock-system.md)

        [Header("닭장")]
        public int initialCoopCapacity;      // 초기 수용 한계 (→ see docs/content/livestock-system.md)
        public int[] coopUpgradeCapacity;    // 레벨별 수용 한계 (→ see docs/content/livestock-system.md)
        public int[] coopUpgradeCost;        // 레벨별 업그레이드 비용 (→ see docs/content/livestock-system.md)

        [Header("생산물 품질 임계값")]
        public float goldQualityThreshold;    // Gold 품질 최소 행복도 (→ see docs/content/livestock-system.md 섹션 5.3)
        public float silverQualityThreshold;  // Silver 품질 최소 행복도 (→ see docs/content/livestock-system.md 섹션 5.3)

        [Header("행복도")]
        public int neglectThresholdDays;     // 연속 미급여 페널티 시작 일수 (→ see docs/content/livestock-system.md 섹션 5)
        public float neglectPenaltyPerDay;   // 미급여 초과 시 일일 추가 감소 (→ see docs/content/livestock-system.md 섹션 5)
        public float initialHappiness;       // 구매 시 초기 행복도 (→ see docs/content/livestock-system.md 섹션 5)

        [Header("생산 배수")]
        public AnimationCurve productionMultiplierCurve;
            // X축: 행복도 (0~1 정규화, 최대값 200으로 나눔), Y축: 배수 (→ see docs/content/livestock-system.md 섹션 5.3)
    }
}
```

**파일 위치**: `Assets/_Project/Scripts/Livestock/Data/LivestockConfig.cs`

#### 5.3 ProcessDailyHappiness() 흐름

```
ProcessDailyHappiness(AnimalInstance animal)
    │
    ├── 1) delta = HappinessCalculator.CalculateDailyDelta(animal, _livestockConfig)
    │
    ├── 2) animal.happiness = Clamp(animal.happiness + delta, 0, 200)
    │
    ├── 3) 임계값 이벤트 체크:
    │       if happiness <= 낮은 임계값:
    │           LivestockEvents.RaiseAnimalSad(animal)
    │           // → see docs/content/livestock-system.md 섹션 5
    │       if happiness == 0:
    │           // [OPEN] 행복도 0일 때의 처리 -- 섹션 10 참조
    │
    └── end
```

---

### 6. Zone E 연동 흐름

Zone E(`zone_south_meadow`, ZoneType.Pasture) 해금과 AnimalManager 활성화의 흐름을 정의한다.

```
[플레이어가 Zone E 구매]
    │
    ├── FarmZoneManager.TryUnlockZone("zone_south_meadow")
    │       → ZoneData.zoneType == ZoneType.Pasture
    │       → 타일 활성화, 장애물 배치
    │       → FarmZoneManager.OnZoneUnlocked?.Invoke("zone_south_meadow")
    │
    ├── AnimalManager.HandleZoneUnlocked(zoneId)
    │       │
    │       ├── if zoneId != "zone_south_meadow": return
    │       │
    │       ├── _isUnlocked = true
    │       │
    │       └── // 외양간 건설 가능 상태 활성화
    │           // 실제 동물 수용은 Barn 건설 후
    │
    ├── [플레이어가 외양간(Barn) 건설]
    │       │
    │       ├── BuildingManager.OnBuildingConstructed("barn")
    │       │
    │       └── AnimalManager.HandleBarnBuilt()
    │               ├── _barnLevel = 1
    │               ├── _barnCapacity = _livestockConfig.initialBarnCapacity
    │               │       // → see docs/content/livestock-system.md
    │               ├── OnBarnUpgraded?.Invoke(1)
    │               └── // 중/대형 동물 구매 가능
    │
    └── [플레이어가 닭장(Chicken Coop) 건설]
            │
            ├── BuildingManager.OnBuildingConstructed("chicken_coop")
            │
            └── AnimalManager.HandleCoopBuilt()
                    ├── _coopLevel = 1
                    ├── _coopCapacity = _livestockConfig.initialCoopCapacity
                    │       // → see docs/content/livestock-system.md
                    ├── OnCoopUpgraded?.Invoke(1)
                    └── // 닭 구매 가능
```

#### 6.1 외양간(Barn) 업그레이드

```
UpgradeBarn(): bool
    │
    ├── 1) 최대 레벨 체크: _barnLevel < _livestockConfig.barnUpgradeCapacity.Length
    │
    ├── 2) 비용 차감: EconomyManager.SpendGold(barnUpgradeCost[nextLevel])
    │       // → see docs/content/livestock-system.md
    │
    ├── 3) _barnLevel++
    │       _barnCapacity = barnUpgradeCapacity[_barnLevel]
    │
    ├── 4) OnBarnUpgraded?.Invoke(_barnLevel)
    │
    └── 5) return true
```

#### 6.2 닭장(Chicken Coop) 업그레이드

```
UpgradeCoop(): bool
    │
    ├── 1) 최대 레벨 체크: _coopLevel < _livestockConfig.coopUpgradeCapacity.Length
    │
    ├── 2) 비용 차감: EconomyManager.SpendGold(coopUpgradeCost[nextLevel])
    │       // → see docs/content/livestock-system.md
    │
    ├── 3) _coopLevel++
    │       _coopCapacity = coopUpgradeCapacity[_coopLevel]
    │
    ├── 4) OnCoopUpgraded?.Invoke(_coopLevel)
    │
    └── 5) return true
```

---

### 7. XP 통합

#### 7.1 XPSource enum 확장

기존 `XPSource` enum(progression-architecture.md 섹션 2.2)에 다음 값을 추가한다:

```csharp
// illustrative -- 기존 XPSource에 추가
namespace SeedMind.Level
{
    public enum XPSource
    {
        CropHarvest,        // 기존
        ToolUse,            // 기존
        FacilityBuild,      // 기존
        FacilityProcess,    // 기존
        MilestoneReward,    // 기존
        QuestComplete,      // 기존
        AchievementReward,  // 기존
        ToolUpgrade,        // 기존
        AnimalCare,         // [신규] 동물 돌봄 (먹이, 쓰다듬기)
        AnimalHarvest       // [신규] 동물 생산물 수확
    }
}
```

#### 7.2 GetExpForSource() 확장

기존 ProgressionManager.GetExpForSource() switch 문(progression-architecture.md 섹션 2.3)에 다음 case를 추가한다:

```csharp
// illustrative -- ProgressionManager.GetExpForSource() 추가분
case XPSource.AnimalCare:
    return _progressionData.animalCareExp;
    // → see docs/balance/progression-curve.md for value

case XPSource.AnimalHarvest:
    var animalInst = (AnimalInstance)context;
    return CalculateAnimalHarvestExp(animalInst);
    // → see docs/balance/progression-curve.md for value
```

#### 7.3 이벤트 구독

```
ProgressionManager.OnEnable() 추가:
    LivestockEvents.OnAnimalPetted += HandleAnimalCare
    LivestockEvents.OnProductCollected += HandleAnimalHarvest
```

**파급 문서**: `docs/systems/progression-architecture.md` 섹션 2.2, 2.3에 AnimalCare/AnimalHarvest case 추가 필요.

---

### 8. 세이브/로드 (AnimalSaveData)

#### 8.1 SaveLoadOrder 할당

| 시스템 | SaveLoadOrder | 근거 |
|--------|:------------:|------|
| FarmZoneManager | 45 | 기존 |
| **AnimalManager** | **48** | FarmZoneManager(45) 이후 -- Zone E 해금 상태 복원 후 동물 상태 로드. FarmGrid(40)와 FarmZoneManager(45) 사이에서 PlayerController(50) 이전에 배치 |

**근거**: AnimalManager는 FarmZoneManager의 구역 해금 상태(`_isUnlocked`)에 의존하므로 45 이후에 로드해야 한다. PlayerController(50)보다 먼저 로드하여 인벤토리 관련 정합성을 확보한다.

#### 8.2 AnimalSaveData 스키마

```csharp
// illustrative
namespace SeedMind.Livestock
{
    [System.Serializable]
    public class AnimalSaveData
    {
        public bool isUnlocked;                    // Zone E 해금 여부
        public int barnLevel;                      // 외양간 레벨
        public int coopLevel;                      // 닭장 레벨
        public AnimalInstanceSaveData[] animals;   // 동물 개체 목록
    }

    [System.Serializable]
    public class AnimalInstanceSaveData
    {
        public string instanceId;
        public string animalDataId;        // AnimalData SO 복원용
        public float happiness;
        public int daysSinceLastFed;
        public int daysSinceLastPetted;
        public bool isFedToday;
        public bool isPettedToday;
        public int daysSinceLastProduct;
        public bool isProductReady;
        public int productQuality;
        public int purchaseDay;
        public string displayName;         // nullable
    }
}
```

#### 8.3 GameSaveData 확장

기존 `GameSaveData` (save-load-architecture.md 섹션 2.3)에 필드를 추가한다:

```csharp
// illustrative -- GameSaveData 추가 필드
public AnimalSaveData animals;   // 목축/낙농 시스템 상태 (ARC-019)
```

**파급 문서**: `docs/systems/save-load-architecture.md` 섹션 2에 animals 필드 추가, 필드 수 카운트 갱신(19 → 20), SaveLoadOrder 할당표에 AnimalManager:48 추가 필요.

#### 8.4 저장/복원 흐름

```
GetSaveData(): AnimalSaveData
    ├── isUnlocked = _isUnlocked
    ├── barnLevel = _barnLevel
    ├── coopLevel = _coopLevel
    └── animals = _animals.Select(a => new AnimalInstanceSaveData {
            instanceId = a.instanceId,
            animalDataId = a.animalDataId,
            happiness = a.happiness,
            ... // 모든 런타임 필드 매핑
        }).ToArray()

LoadSaveData(AnimalSaveData data):
    ├── _isUnlocked = data.isUnlocked
    ├── _barnLevel = data.barnLevel
    ├── _barnCapacity = _livestockConfig.barnUpgradeCapacity[_barnLevel]
    ├── _coopLevel = data.coopLevel
    ├── _coopCapacity = _livestockConfig.coopUpgradeCapacity[_coopLevel]
    └── foreach savedAnimal in data.animals:
            var instance = new AnimalInstance { ... }
            instance.data = DataRegistry.GetAnimalData(savedAnimal.animalDataId)
                // SO 참조 복원
            _animals.Add(instance)
```

---

### 9. HarvestOrigin 확장

동물 생산물의 출처를 추적하기 위해 기존 HarvestOrigin enum(economy-architecture.md 섹션 3.10.2)에 값을 추가한다.

```csharp
// illustrative -- 기존 HarvestOrigin에 추가
namespace SeedMind
{
    public enum HarvestOrigin
    {
        Outdoor    = 0,   // 기존 -- 야외 농장
        Greenhouse = 1,   // 기존 -- 온실
        Barn       = 2    // [신규] 외양간/목장 동물 생산물
        // [OPEN] Cave = 3, Planter = 4 등
    }
}
```

**경제 시스템 연동**: 동물 생산물 판매 시 `HarvestOrigin.Barn`을 전달한다. 가격 보정 계수 적용 여부는 경제 시스템 설계에서 결정한다.

[OPEN] Barn origin에 대한 가격 보정 정책. 야외 작물과 동일한 기본 가격을 적용할지, 동물 생산물 전용 가격 체계를 사용할지는 경제 밸런스(BAL) 문서에서 확정 필요.

**파급 문서**: `docs/systems/economy-architecture.md` 섹션 3.10.2에 `Barn = 2` 추가, `GetGreenhouseMultiplier()` switch 문에 `Barn` case 추가 필요.

---

### 10. LivestockEvents 정적 이벤트 허브

기존 이벤트 패턴(FarmEvents, NPCEvents 등)을 따르는 정적 이벤트 허브이다.

```csharp
// illustrative
namespace SeedMind.Livestock
{
    public static class LivestockEvents
    {
        // --- 동물 생애 ---
        public static event Action<AnimalInstance> OnAnimalPurchased;
        public static event Action<AnimalInstance> OnAnimalFed;
        public static event Action<AnimalInstance> OnAnimalPetted;
        public static event Action<AnimalInstance> OnAnimalSad;  // 행복도 임계값 이하

        // --- 생산 ---
        public static event Action<AnimalInstance, AnimalProductInfo> OnProductReady;
        public static event Action<AnimalInstance, int> OnProductCollected;  // amount
        // ※ 경량 이벤트 허브 — XP/퀘스트 등 시스템 구독용 (ItemData 생략)
        // ※ UI 구독은 AnimalManager 인스턴스 이벤트 OnProductCollected(AnimalInstance, ItemData, int) 사용

        // --- 외양간/닭장 ---
        public static event Action<int> OnBarnUpgraded;  // newLevel
        public static event Action<int> OnCoopUpgraded;  // newLevel

        // --- Raise 메서드 ---
        public static void RaiseAnimalPurchased(AnimalInstance a) => OnAnimalPurchased?.Invoke(a);
        public static void RaiseAnimalFed(AnimalInstance a) => OnAnimalFed?.Invoke(a);
        public static void RaiseAnimalPetted(AnimalInstance a) => OnAnimalPetted?.Invoke(a);
        public static void RaiseAnimalSad(AnimalInstance a) => OnAnimalSad?.Invoke(a);
        public static void RaiseProductReady(AnimalInstance a, AnimalProductInfo i) => OnProductReady?.Invoke(a, i);
        public static void RaiseProductCollected(AnimalInstance a, int amt) => OnProductCollected?.Invoke(a, amt);
        public static void RaiseBarnUpgraded(int lvl) => OnBarnUpgraded?.Invoke(lvl);
        public static void RaiseCoopUpgraded(int lvl) => OnCoopUpgraded?.Invoke(lvl);
    }

    public struct AnimalProductInfo
    {
        public string productItemId;
        public int quality;
        public int estimatedAmount;
    }
}
```

---

### 11. Unity 폴더 구조

```
Assets/_Project/Scripts/Livestock/          # SeedMind.Livestock 네임스페이스
├── AnimalManager.cs                        # 싱글턴, 핵심 관리자
├── AnimalInstance.cs                       # 런타임 동물 상태
├── HappinessCalculator.cs                  # 행복도 계산 유틸리티
├── LivestockEvents.cs                      # 정적 이벤트 허브
├── AnimalType.cs                           # enum
├── CollectResult.cs                        # enum
├── AnimalProductInfo.cs                    # struct
└── Data/                                   # SeedMind.Livestock.Data 네임스페이스
    ├── AnimalData.cs                       # 동물 정의 SO
    ├── LivestockConfig.cs                  # 시스템 설정 SO
    └── AnimalSaveData.cs                   # 세이브 데이터 클래스

Assets/_Project/Data/Animals/               # AnimalData SO 에셋 인스턴스
├── SO_Animal_Chicken.asset
├── SO_Animal_Goat.asset
├── SO_Animal_Cow.asset
└── SO_Animal_Sheep.asset
    // → 동물 종류 목록은 see docs/content/livestock-system.md 섹션 1

Assets/_Project/Prefabs/Animals/            # 동물 프리팹
├── PFB_Chicken.prefab
├── PFB_Goat.prefab
├── PFB_Cow.prefab
└── PFB_Sheep.prefab
```

#### 11.1 asmdef 의존성

| asmdef | 의존 대상 | 이유 |
|--------|----------|------|
| SeedMind.Livestock | SeedMind.Core | Singleton, ISaveable, HarvestOrigin |
| SeedMind.Livestock | SeedMind.Farm | FarmZoneManager, ZoneType |
| SeedMind.Livestock | SeedMind.Level | ProgressionManager, XPSource |

---

## Part II -- MCP 구현 태스크 개요

> 상세 MCP 태스크 시퀀스는 별도 문서(ARC-024: `docs/mcp/livestock-tasks.md`)에서 작성한다.  
> 여기서는 단계별 개요만 기술한다.

### Phase A: SO 및 핵심 스크립트 작성

```
Step A-1: Livestock 폴더 구조 생성
          Scripts/Livestock/, Scripts/Livestock/Data/

Step A-2: enum/struct 작성
          AnimalType.cs, CollectResult.cs, AnimalProductInfo.cs

Step A-3: AnimalData SO, LivestockConfig SO 작성
          → MCP: CreateScript 2개

Step A-4: AnimalInstance.cs, AnimalSaveData.cs 작성
          → MCP: CreateScript 2개

Step A-5: HappinessCalculator.cs, LivestockEvents.cs 작성
          → MCP: CreateScript 2개

Step A-6: AnimalManager.cs 작성 (ISaveable 구현 포함)
          → MCP: CreateScript 1개
```

### Phase B: SO 에셋 인스턴스 생성

```
Step B-1: Data/Animals/ 폴더에 AnimalData SO 에셋 생성
          → MCP: CreateScriptableObject (동물 종류별)
          → 수치는 docs/content/livestock-system.md에서 참조

Step B-2: LivestockConfig SO 에셋 생성
          → MCP: CreateScriptableObject
          → 수치는 docs/content/livestock-system.md에서 참조

Step B-3: AnimationCurve 설정 (productionMultiplierCurve)
          → MCP: SetProperty
```

### Phase C: 씬 통합

```
Step C-1: SCN_Farm에 AnimalManager 오브젝트 생성
          → MCP: CreateGameObject("AnimalManager", parent="--- MANAGERS ---")
          → MCP: AddComponent(AnimalManager)

Step C-2: AnimalData[] 레지스트리 할당
          → MCP: SetProperty(_animalDataRegistry, SO 에셋 배열)

Step C-3: LivestockConfig SO 할당
          → MCP: SetProperty(_livestockConfig, SO 에셋)
```

### Phase D: 기존 시스템 확장

```
Step D-1: XPSource enum에 AnimalCare, AnimalHarvest 추가
          → 기존 파일 수정

Step D-2: ProgressionManager.GetExpForSource() switch 문 확장
          → 기존 파일 수정

Step D-3: HarvestOrigin enum에 Barn 추가
          → 기존 파일 수정

Step D-4: GameSaveData에 animals 필드 추가
          → 기존 파일 수정

Step D-5: SaveLoadOrder 할당표에 AnimalManager:48 등록
          → SaveManager 또는 ISaveable 등록 확인
```

### Phase E: 검증

```
Step E-1: Play Mode 진입 → AnimalManager 초기화 로그 확인
Step E-2: TryBuyAnimal() 호출 → 동물 추가 확인
Step E-3: DailyUpdate() 수동 트리거 → 행복도 변동 로그
Step E-4: CollectProduct() → 인벤토리 추가 확인
Step E-5: 저장/로드 → AnimalSaveData 무결성 확인
```

---

## Cross-references

| 문서 | 관련 내용 |
|------|----------|
| `docs/content/livestock-system.md` (CON-006) | 동물 종류, 가격, 사료, 생산물, 행복도 수치 canonical |
| `docs/systems/farm-expansion-architecture.md` (ARC-023) | Zone E 해금, ZoneType.Pasture, FarmZoneManager |
| `docs/systems/farm-expansion.md` (DES-012) | Zone E 게임 디자인, 구역 구매 흐름 |
| `docs/systems/progression-architecture.md` (BAL-002) | XPSource enum, ProgressionManager, GetExpForSource() |
| `docs/systems/economy-architecture.md` | EconomyManager, HarvestOrigin, 가격 보정 |
| `docs/systems/save-load-architecture.md` (ARC-011) | GameSaveData, ISaveable, SaveLoadOrder 할당표 |
| `docs/pipeline/data-pipeline.md` | SO 구조, JSON 직렬화 전략 |
| `docs/systems/project-structure.md` | 네임스페이스, 폴더 구조, asmdef 규칙 |
| `docs/architecture.md` | 마스터 아키텍처 -- 프로젝트 구조에 Livestock 폴더 추가 필요 |
| `docs/content/processing-system.md` (CON-005) | 치즈 공방 레시피 (동물 생산물 의존) |

---

## Open Questions / Risks

### Open Questions

1. [OPEN] **행복도 0 처리**: 행복도가 0에 도달했을 때 동물이 도망가는지, 생산만 중단하는지, 질병 상태로 전이하는지 결정 필요. 콘텐츠 설계(CON-006)에서 확정.

2. [OPEN] **CropQuality 재사용 vs ProductQuality**: 동물 생산물 품질이 작물 품질 enum(Normal/Silver/Gold/Iridium)을 그대로 사용할지, 별도 품질 체계가 필요한지. Iridium 등급 동물 생산물의 존재 여부에 따라 결정.

3. [OPEN] **HarvestOrigin.Barn 가격 보정**: 동물 생산물에 출처 기반 가격 보정을 적용할지. 작물의 온실 페널티와 대칭되는 구조인지, 독립 가격 체계인지는 경제 밸런스 문서에서 확정.

4. [OPEN] **자동 급여 시스템(Auto-Feed)**: 사료통(Feed Trough) 시설 건설 시 자동 급여를 지원할지. CheckAutoFeed() 메서드를 예약했으나 상세 설계는 CON-006 확정 후.

5. [OPEN] **동물 프리팹 배치**: Zone E 목초지에 동물 프리팹을 어떻게 배치할지. 자유 배회(NavMesh) vs 고정 위치 vs 울타리 영역 내 랜덤. 비주얼 시스템과 연계 필요.

6. [RESOLVED] **닭장(Coop) vs 외양간(Barn) 분리**: AnimalType.Poultry는 Coop, 나머지는 Barn을 요구하는 구조로 확정. 섹션 1 클래스 다이어그램, 섹션 3.1 구매 흐름, 섹션 6 Zone E 연동 흐름에 반영 완료 (FIX-038).

7. [OPEN] **치즈 공방 연계**: processing-system.md(CON-005)에서 보류된 치즈 공방 레시피 활성화. 동물 생산물(우유)이 가공 원재료로 사용되는 흐름의 상세 설계 필요.

### Risks

1. [RISK] **CON-006 미작성**: 이 아키텍처 문서의 모든 수치 참조가 아직 존재하지 않는 `docs/content/livestock-system.md`를 가리킨다. CON-006 작성 전까지 구현 불가. **완화**: CON-006 작성을 ARC-019의 선행 작업으로 TODO에 등록.

2. [RISK] **SaveLoadOrder 48의 간극**: 45(FarmZoneManager)와 50(PlayerController) 사이에 48을 배치했으나, 향후 다른 시스템이 46~49 범위를 필요로 할 경우 충돌 가능. **완화**: 5 단위 간격 규칙에서 벗어나므로, save-load-architecture.md의 할당표에서 명시적 예약 필요.

3. [RISK] **AnimationCurve SO 필드의 MCP 설정**: LivestockConfig의 `productionMultiplierCurve` (AnimationCurve)를 MCP로 설정할 수 있는지 불확실. MCP for Unity의 AnimationCurve 키프레임 설정 지원 범위 검증 필요. **완화**: 직접 Inspector에서 수동 설정하거나, 코드에서 기본 커브를 생성하는 fallback 구현.

4. [RISK] **XPSource enum 확장의 파급 범위**: AnimalCare, AnimalHarvest 추가 시 ProgressionManager의 switch 문뿐 아니라, ProgressionSaveData의 sourceBreakdown Dictionary(존재 시)에도 영향. 기존 세이브 파일과의 하위 호환성 검증 필요.

5. [RISK] **HarvestOrigin enum 확장의 파급 범위**: Barn 추가 시 economy-architecture.md의 GetGreenhouseMultiplier() switch 문, ItemSlot 스택 정책, 인벤토리 UI 등에 연쇄 수정 필요. FIX-034 패턴을 따라 전수 업데이트 필요.

---

*이 문서는 Claude Code가 기존 아키텍처 문서들의 패턴과 규칙을 준수하여 자율적으로 작성했습니다.*
