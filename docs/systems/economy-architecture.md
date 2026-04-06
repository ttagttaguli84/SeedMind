# 경제 시스템 기술 아키텍처

> EconomyManager, ShopSystem, PriceData, PriceFluctuationSystem, TransactionLog의 클래스 설계, 가격 변동 알고리즘, 이벤트 연동, MCP 구현 계획  
> 작성: Claude Code (Opus) | 2026-04-06

---

## Context

이 문서는 `docs/architecture.md` Economy 섹션(Scripts/Economy/)을 기술적으로 상세화한다. SeedMind의 경제 시스템은 핵심 게임 루프의 동기 부여를 담당한다 -- 플레이어가 작물을 수확하고 판매하여 골드를 벌고, 그 골드로 씨앗/도구/시설을 구매하여 농장을 확장한다. 동적 가격 변동, 계절/날씨 보정, 수급 기반 보정을 통해 플레이어에게 판매 타이밍 전략을 제공한다.

**설계 목표**:
- 가격이 예측 가능하되 단조롭지 않아야 한다 -- 플레이어가 "언제 팔지"를 고민하게 만든다
- 모든 경제 수치는 ScriptableObject로 외부화하여 코드 변경 없이 밸런스 조정 가능
- TransactionLog를 통해 밸런싱 데이터를 수집하고 MCP 콘솔로 검증 가능

---

## 1. 클래스 다이어그램

```
┌─────────────────────────────────────────────────────────────────────┐
│                         SeedMind.Economy                            │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│              EconomyManager (MonoBehaviour, Singleton)         │
│──────────────────────────────────────────────────────────────│
│  [상태]                                                       │
│  - _currentGold: int                                         │
│  - _transactionLog: TransactionLog                           │
│                                                              │
│  [설정 참조]                                                   │
│  - _economyConfig: EconomyConfig (ScriptableObject)          │
│  - _priceFluctuation: PriceFluctuationSystem (내부 인스턴스)   │
│                                                              │
│  [읽기 전용 프로퍼티]                                           │
│  + CurrentGold: int                                          │
│  + TransactionLog: TransactionLog (읽기 전용)                 │
│                                                              │
│  [이벤트]                                                     │
│  + OnGoldChanged: Action<int, int>        // oldGold, newGold│
│  + OnTransactionComplete: Action<Transaction>                │
│  + OnPriceChanged: Action                 // 가격표 갱신됨     │
│                                                              │
│  [메서드]                                                     │
│  + Initialize(): void                                        │
│  + AddGold(int amount, string reason): bool                  │
│  + SpendGold(int amount, string reason): bool                │
│  + CanAfford(int amount): bool                               │
│  + GetSellPrice(CropData crop, CropQuality quality,          │
│       HarvestOrigin origin = HarvestOrigin.Outdoor): int     │
│  + GetBuyPrice(string itemId): int                           │
│  + SellCrop(CropData crop, int quantity, CropQuality q): int │
│  + BuyItem(string itemId, int quantity): bool                │
│  - OnDayChangedHandler(int newDay): void                     │
│  - OnSeasonChangedHandler(Season newSeason): void            │
│  + GetSaveData(): EconomySaveData                            │
│  + LoadSaveData(EconomySaveData data): void                  │
│                                                              │
│  [구독]                                                       │
│  + OnEnable():                                               │
│      TimeManager.RegisterOnDayChanged(priority: 40)          │
│      TimeManager.RegisterOnSeasonChanged(priority: 30)       │
│  + OnDisable(): 구독 해제                                      │
└──────────────────────────────────────────────────────────────┘
         │ owns                          │ ref
         ▼                               ▼
┌────────────────────────┐     ┌──────────────────────────────┐
│ PriceFluctuationSystem │     │  EconomyConfig (SO)          │
│ (Plain C# class)       │     │──────────────────────────────│
│                        │     │  startingGold: 500           │
│ (아래 1.2 참조)        │     │  maxGold: 999999             │
│                        │     │  sellPriceFloor: 0.5f        │
│                        │     │  sellPriceCeiling: 2.0f      │
│                        │     │  categorySupplyParams[4]     │
│                        │     │  transactionLogCapacity: 200 │
└────────────────────────┘     └──────────────────────────────┘
         │ uses
         ▼
┌────────────────────────┐
│ PriceData (SO)         │
│ (아래 1.4 참조)        │
└────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                ShopSystem (MonoBehaviour)                      │
│──────────────────────────────────────────────────────────────│
│  [설정 참조]                                                   │
│  - _shopData: ShopData (ScriptableObject)                    │
│  - _economyManager: EconomyManager (참조)                     │
│                                                              │
│  [상태]                                                       │
│  - _availableItems: List<ShopItemEntry>  (현재 판매 가능 목록) │
│  - _isOpen: bool                                             │
│                                                              │
│  [읽기 전용 프로퍼티]                                           │
│  + IsOpen: bool                                              │
│  + AvailableItems: IReadOnlyList<ShopItemEntry>              │
│                                                              │
│  [이벤트]                                                     │
│  + OnShopOpened: Action                                      │
│  + OnShopClosed: Action                                      │
│  + OnShopPurchased: Action<ShopItemEntry, int>               │  // ← EconomyEvents 정적 허브를 통해 외부 구독 (→ 섹션 2.2)
│  + OnItemSold: Action<CropData, int, int>  // crop, qty, gold│
│                                                              │
│  [메서드]                                                     │
│  + Open(): void                                              │
│  + Close(): void                                             │
│  + RefreshStock(): void                                      │
│  + TryBuyItem(ShopItemEntry item, int qty): bool             │
│  + TrySellCrop(CropData crop, int qty, CropQuality q): int  │
│  + GetDisplayPrice(ShopItemEntry item): int                  │
│  - FilterByPlayerLevel(int level): void                      │
│  - FilterBySeason(Season season): void                       │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│               TransactionLog (Plain C# class)                 │
│──────────────────────────────────────────────────────────────│
│  [상태]                                                       │
│  - _entries: CircularBuffer<Transaction>                      │
│  - _capacity: int                                            │
│  - _totalEarned: long     (누적 수입)                         │
│  - _totalSpent: long      (누적 지출)                         │
│                                                              │
│  [읽기 전용 프로퍼티]                                           │
│  + Entries: IReadOnlyList<Transaction>                        │
│  + Count: int                                                │
│  + TotalEarned: long                                         │
│  + TotalSpent: long                                          │
│  + NetProfit: long (= TotalEarned - TotalSpent)              │
│                                                              │
│  [메서드]                                                     │
│  + AddEntry(Transaction tx): void                            │
│  + GetEntriesByDay(int day): List<Transaction>               │
│  + GetEntriesBySeason(Season season): List<Transaction>      │
│  + GetItemSalesCount(string itemId): int   (수급 보정 입력 — 품목 무관 범용) │
│  + Clear(): void                                             │
│  + GetSaveData(): TransactionLogSaveData                     │
│  + LoadSaveData(TransactionLogSaveData data): void           │
└──────────────────────────────────────────────────────────────┘
```

### 1.1 클래스 책임 요약

| 클래스 | 유형 | 네임스페이스 | 책임 |
|--------|------|-------------|------|
| **EconomyManager** | MonoBehaviour (Singleton) | `SeedMind.Economy` | 골드 관리, 거래 처리, 가격 조회 인터페이스 |
| **ShopSystem** | MonoBehaviour | `SeedMind.Economy` | 상점 UI 연동, 재고 관리, 구매/판매 처리 |
| **PriceFluctuationSystem** | Plain C# class | `SeedMind.Economy` | 가격 변동 계산 (계절/수급/날씨 보정) |
| **TransactionLog** | Plain C# class | `SeedMind.Economy` | 거래 기록 관리, 통계 집계, 수급 데이터 제공 |
| **EconomyConfig** | ScriptableObject | `SeedMind.Economy.Data` | 경제 시스템 글로벌 설정값 |
| **ShopData** | ScriptableObject | `SeedMind.Economy.Data` | 상점 정의 (판매 품목, 운영 시간) |
| **PriceData** | ScriptableObject | `SeedMind.Economy.Data` | 품목별 가격 및 보정 계수 정의 |

### 1.2 PriceFluctuationSystem 상세

```
┌──────────────────────────────────────────────────────────────┐
│         PriceFluctuationSystem (Plain C# class)               │
│──────────────────────────────────────────────────────────────│
│  [참조]                                                       │
│  - _economyConfig: EconomyConfig                             │
│  - _priceDataMap: Dictionary<string, PriceData>              │
│  - _transactionLog: TransactionLog                           │
│                                                              │
│  [캐시]                                                       │
│  - _cachedPrices: Dictionary<string, int>  (품목ID → 최종가)  │
│  - _isDirty: bool (재계산 필요 여부)                           │
│                                                              │
│  [현재 보정 상태]                                               │
│  - _currentSeason: Season                                    │
│  - _currentWeather: WeatherType                              │
│                                                              │
│  [메서드]                                                     │
│  + Initialize(EconomyConfig config, TransactionLog log): void│
│  + SetSeason(Season season): void   // OnSeasonChanged에서   │
│  + SetWeather(WeatherType weather): void                     │
│  + CalculateSellPrice(PriceData data, CropQuality q,         │
│       CropData cropData,                                     │
│       HarvestOrigin origin = HarvestOrigin.Outdoor): int     │
│  + CalculateBuyPrice(PriceData data): int                    │
│  + MarkDirty(): void                                         │
│  + RecalculateAllPrices(): void                              │
│  - GetSeasonMultiplier(PriceData data, Season season): float │
│  - GetSupplyMultiplier(string itemId, SupplyCategory cat): float │
│  - GetWeatherMultiplier(PriceData data, WeatherType w): float│
│  - GetQualityMultiplier(CropQuality quality): float          │
│  - GetGreenhouseMultiplier(HarvestOrigin origin,             │
│       CropData cropData, Season currentSeason): float        │
│  - ClampPrice(float rawPrice, PriceData data): int           │
│  + GetSaveData(): PriceFluctuationSaveData                   │
│  + LoadSaveData(PriceFluctuationSaveData data): void         │
└──────────────────────────────────────────────────────────────┘
```

### 1.3 의존성 방향

```
TimeManager (Core)
    │ events: OnDayChanged, OnSeasonChanged
    ▼
EconomyManager (Economy)
    │ owns
    ├── PriceFluctuationSystem
    │       │ reads
    │       ├── PriceData[] (SO)
    │       ├── EconomyConfig (SO)
    │       └── TransactionLog (수급 데이터)
    │
    ├── TransactionLog
    │
    └── references → ShopSystem

WeatherSystem (Core)
    │ event: OnWeatherChanged
    ▼
EconomyManager
    → PriceFluctuationSystem.SetWeather()

FarmEvents (Farm)
    │ event: OnCropHarvested
    ▼
ShopSystem (판매 제안 UI 트리거, 선택적)

PlayerInventory (Player)
    ↔ ShopSystem (구매/판매 시 아이템 교환)
```

이 의존성은 `docs/systems/project-structure.md` 3.2절의 의존성 매트릭스에 부합한다: Economy는 Core와 Farm을 참조할 수 있으나, Player와 UI는 참조하지 않는다.

---

## 2. 이벤트 연동

### 2.1 TimeManager 이벤트 구독

EconomyManager는 `docs/systems/time-season-architecture.md` 4절의 우선순위 기반 이벤트 디스패처에 등록한다.

**OnDayChanged 구독 (priority: 40)**:

```
EconomyManager.OnDayChangedHandler(int newDay):
    1. PriceFluctuationSystem.SetWeather(WeatherSystem.CurrentWeather)
    2. PriceFluctuationSystem.RecalculateAllPrices()
    3. OnPriceChanged?.Invoke()
```

(-> see `docs/systems/time-season-architecture.md` 섹션 4.3 for canonical 실행 순서)

priority 40은 WeatherSystem(0), GrowthSystem(10), FarmGrid(20), FestivalManager(30) 이후에 실행되므로, 날씨와 축제 정보가 확정된 상태에서 가격을 재계산한다.

**OnSeasonChanged 구독 (priority: 30)**:

```
EconomyManager.OnSeasonChangedHandler(Season newSeason):
    1. PriceFluctuationSystem.SetSeason(newSeason)
    2. ShopSystem.RefreshStock()  // 계절별 재고 갱신
    3. PriceFluctuationSystem.RecalculateAllPrices()
    4. OnPriceChanged?.Invoke()
```

(-> see `docs/systems/time-season-architecture.md` 섹션 4.4 for canonical 실행 순서)

### 2.2 EconomyManager가 발행하는 이벤트

| 이벤트 | 페이로드 | 발행 시점 | 예상 소비자 |
|--------|----------|-----------|-------------|
| `OnGoldChanged` | `(int oldGold, int newGold)` | AddGold, SpendGold 호출 시 | HUDController (GoldDisplay 갱신), SaveManager |
| `OnTransactionComplete` | `Transaction` | SellCrop, BuyItem 완료 시 | ShopUI (거래 피드백), TransactionLog |
| `OnPriceChanged` | (없음) | 일일 가격 재계산 완료 후 | ShopUI (가격표 갱신), HUDController |
| `EconomyEvents.OnShopPurchased` | `(ShopItemEntry, int qty)` | BuyItem 완료 시 (ShopSystem.OnShopPurchased 래핑) | AchievementManager(PurchaseCount), TutorialTriggerSystem(Step 11), ShopUI |
| `EconomyEvents.OnSaleCompleted` | `(string itemId, int qty, int goldEarned)` | SellCrop 완료 시 | AchievementManager(GoldEarned), NotificationManager, UIManager |
| `EconomyEvents.OnGoldSpent` | `int amount` | SpendGold 호출 시 | AchievementManager(GoldSpent) |

### 2.3 외부 이벤트 소비

| 소스 이벤트 | 소스 시스템 | EconomyManager 처리 |
|-------------|------------|---------------------|
| `TimeManager.OnDayChanged` | Core | 일일 가격 재계산 (priority 40) |
| `TimeManager.OnSeasonChanged` | Core | 계절 가격 테이블 교체 (priority 30) |
| `WeatherSystem.OnWeatherChanged` | Core | 날씨 보정 계수 갱신 → MarkDirty |
| `FestivalManager.OnFestivalStarted` | Core | 축제 할인율 적용 → MarkDirty |
| `FestivalManager.OnFestivalEnded` | Core | 축제 할인율 해제 → MarkDirty |

### 2.4 이벤트 구독/해제 패턴

```csharp
// illustrative
namespace SeedMind.Economy
{
    public class EconomyManager : Singleton<EconomyManager>
    {
        private void OnEnable()
        {
            TimeManager.Instance.RegisterOnDayChanged(40, OnDayChangedHandler);
            TimeManager.Instance.RegisterOnSeasonChanged(30, OnSeasonChangedHandler);
            WeatherSystem.Instance.OnWeatherChanged += OnWeatherChangedHandler;
            FestivalManager.Instance.OnFestivalStarted += OnFestivalStartedHandler;
            FestivalManager.Instance.OnFestivalEnded += OnFestivalEndedHandler;
        }

        private void OnDisable()
        {
            TimeManager.Instance?.UnregisterOnDayChanged(OnDayChangedHandler);
            TimeManager.Instance?.UnregisterOnSeasonChanged(OnSeasonChangedHandler);
            if (WeatherSystem.Instance != null)
                WeatherSystem.Instance.OnWeatherChanged -= OnWeatherChangedHandler;
            if (FestivalManager.Instance != null)
            {
                FestivalManager.Instance.OnFestivalStarted -= OnFestivalStartedHandler;
                FestivalManager.Instance.OnFestivalEnded -= OnFestivalEndedHandler;
            }
        }
    }
}
```

---

## 3. 가격 변동 알고리즘

### 3.1 최종 가격 공식

```
최종_판매가 = 기본_판매가 × 계절_보정 × 수급_보정 × 날씨_보정 × 품질_보정 × 축제_보정 × 온실_보정

최종_구매가 = 기본_구매가 × 계절_보정 × 축제_보정
```

- 온실_보정 상세: -> see 섹션 3.7 (BAL-010 확정, x0.8 페널티 또는 x1.2 시너지)

최종 가격은 정수로 반올림(Mathf.RoundToInt)하며, 상한/하한으로 클램핑한다.

### 3.2 계절 보정 (Season Multiplier)

각 PriceData SO에 계절별 보정 배열을 정의한다. 계절에 따른 수요 변동을 반영한다.

```
Pseudocode: GetSeasonMultiplier(PriceData data, Season season)

    return data.seasonMultipliers[(int)season]
    // 예: 감자 = [1.0 (봄), 0.8 (여름), 1.2 (가을), 1.5 (겨울)]
    // 봄에는 감자가 풍부하여 기본가, 겨울에는 희소하여 1.5배
```

**설계 근거**: 작물이 해당 계절에 재배 가능하면 공급이 많아 가격이 낮고, 재배 불가능한 계절에는 희소성으로 가격이 오른다. 이는 플레이어에게 창고 시설(-> see `docs/design.md` 섹션 4.6)을 활용한 매매 타이밍 전략을 제공한다.

### 3.3 수급 보정 (Supply Multiplier)

최근 판매량에 따라 가격이 하락하는 수급 시뮬레이션이다.

```
Pseudocode: GetSupplyMultiplier(string itemId, SupplyCategory category)

    // 카테고리별 수급 파라미터 조회 (→ see docs/systems/economy-system.md 섹션 7.3)
    supplyParams = EconomyConfig.GetSupplyParams(category)
    // supplyParams: { demandThreshold, supplyDropRate, minSupplyMultiplier }

    if supplyParams.demandThreshold < 0:
        return 1.0  // 수급 보정 면제 (ProcessedGoods 등)

    recentSales = TransactionLog.GetItemSalesCount(itemId)
    // 현재 계절(28일) 내 해당 품목 총 판매 수량

    if recentSales <= supplyParams.demandThreshold:
        return 1.0  // 수요 충분, 보정 없음

    excessRatio = (recentSales - supplyParams.demandThreshold) / supplyParams.demandThreshold
    // 초과 판매 비율 (0.0 ~ ...)

    supplyPenalty = excessRatio * supplyParams.supplyDropRate
    // Crop: 0.02, AnimalProduct: 0.008, Fish: 0.01 (→ see economy-system.md 섹션 7.3)

    multiplier = max(supplyParams.minSupplyMultiplier, 1.0 - supplyPenalty)
    // Crop: 최저 0.70, AnimalProduct: 최저 0.85, Fish: 최저 0.80 (→ see economy-system.md 섹션 7.3)

    return multiplier
```

**supplyParams**: EconomyConfig SO의 `categorySupplyParams` 배열에서 `SupplyCategory` 인덱스로 조회. 카테고리별로 demandThreshold, supplyDropRate, minSupplyMultiplier가 개별 설정된다 (-> see 섹션 3.11).

**리셋 타이밍**: 계절이 바뀌면 recentSales 카운트가 리셋된다 (새 시장 사이클 시작).

### 3.4 날씨 보정 (Weather Multiplier)

```
Pseudocode: GetWeatherMultiplier(PriceData data, WeatherType weather)

    switch (weather):
        case Clear:     return 1.0    // 기본
        case Cloudy:    return 1.0    // 영향 없음
        case Rain:      return 1.0    // 영향 없음
        case HeavyRain: return 1.02   // 폭우 다음 날 +2% (-> see economy-system.md 섹션 2.6.3)
        case Storm:     return 1.05   // 폭풍 다음 날 +5%
        case Snow:      return 1.03   // 눈 다음 날 +3%
        case Blizzard:  return 1.08   // 폭설 다음 날 +8%
```

> WeatherType 값은 `SeedMind.Core.WeatherType` enum을 따른다 (-> see `docs/systems/time-season-architecture.md` 섹션 1.3). 날씨 보정 수치의 canonical 정의는 `docs/systems/economy-system.md` 섹션 2.6.3이다.

날씨 보정은 판매가에만 적용된다. 구매가(씨앗, 도구)는 날씨와 무관하다.

### 3.5 품질 보정 (Quality Multiplier)

작물 품질에 따른 판매가 보정이다 (-> see `docs/systems/crop-growth.md` for 품질 등급 정의).

```
Pseudocode: GetQualityMultiplier(CropQuality quality)

    switch (quality):
        case Normal:    return 1.0
        case Silver:    return 1.25
        case Gold:      return 1.5
        case Iridium:   return 2.0
```

품질 보정은 판매가에만 적용된다.

### 3.6 축제 보정 (Festival Multiplier)

축제가 활성화된 날에는 FestivalData의 shopDiscountRate가 적용된다 (-> see `docs/systems/time-season-architecture.md` 섹션 2.5).

```
Pseudocode:

    if FestivalManager.IsFestivalDay():
        festival = FestivalManager.GetActiveFestival()

        // 구매가 할인
        buyMultiplier = 1.0 - festival.shopDiscountRate
        // 예: shopDiscountRate = 0.2 → 구매가 20% 할인

        // 특정 작물 판매가 보너스
        if crop == festival.specialCropBonus:
            sellMultiplier = festival.bonusMultiplier
            // 예: 수확 축제에서 호박 판매 시 1.5배
        else:
            sellMultiplier = 1.0
    else:
        buyMultiplier = 1.0
        sellMultiplier = 1.0
```

### 3.7 온실 판매가 보정 (Greenhouse Price Modifier) [BAL-010, RESOLVED]

온실에서 수확한 비계절 작물에 대한 판매가 추가 보정이다 (-> see `docs/systems/economy-system.md` 섹션 2.6.5 for canonical 규칙).

**[RESOLVED-BAL-010]** 2026-04-07 확정: 시나리오 A(비계절 페널티 x0.8)와 시나리오 B(겨울 전용 시너지 x1.2)를 **동시 적용**. 상세 근거는 `docs/balance/crop-economy.md` 섹션 4.3.10 참조. 아래 섹션 3.7.1~3.7.2는 각 구현 방향의 참고 기록이며, 3.7.3에서 확정 구현 방향을 참조할 것.

디자이너 확정 전 두 시나리오를 병기한 원본 기록:

#### 3.7.1 시나리오 A: 비계절 온실 작물 판매가 페널티

```
Pseudocode: GetGreenhouseMultiplier(bool isGreenhouse, SeasonFlag cropSeasons, Season currentSeason)

    if !isGreenhouse:
        return 1.0  // 야외 재배 → 보정 없음

    if (cropSeasons & (1 << currentSeason)) != 0:
        return 1.0  // 해당 계절 작물 → 보정 없음

    if cropSeasons == SeasonFlag.Winter:
        return 1.0  // 겨울 전용 작물 → 페널티 면제 (온실이 본래 환경)

    return economyConfig.greenhouseOffSeasonPenalty
    // 잠정 0.8 → 비계절 작물 판매가 20% 감소
    // → see docs/systems/economy-system.md 섹션 2.6.5
```

**필요 변경 사항**:

| 대상 | 변경 내용 |
|------|----------|
| `EconomyConfig` SO | `greenhouseOffSeasonPenalty: float` 필드 추가 (기본값 잠정 0.8) |
| `PriceFluctuationSystem.CalculateSellPrice()` | 시그니처에 `bool isGreenhouse` 파라미터 추가 |
| `EconomyManager.GetSellPrice()` | 호출 측에서 `FarmTile.IsInGreenhouse` 값 전달 |
| `CropData` SO | 변경 불필요 (기존 `allowedSeasons`로 계절 매칭 판별 가능) |

**데이터 흐름 변경**:

```
ShopSystem.TrySellCrop(crop, qty, quality, isGreenhouse)
    │
    └── EconomyManager.GetSellPrice(crop, quality, isGreenhouse)
            → PriceFluctuationSystem.CalculateSellPrice(priceData, quality, isGreenhouse)
                → greenhouseMul = GetGreenhouseMultiplier(isGreenhouse, cropData.allowedSeasons, currentSeason)
                → finalPrice = base × season × supply × weather × quality × festival × greenhouseMul
                → ClampPrice()
```

[RISK] `ShopSystem.TrySellCrop()` 시그니처 변경은 출하함 판매 흐름에도 영향을 줌. 출하함에서도 `isGreenhouse` 정보를 전달해야 하므로, 인벤토리 아이템에 수확 출처(origin) 태그가 필요할 수 있다.

#### 3.7.2 시나리오 B: 겨울 전용 작물 온실 시너지 보너스

```
Pseudocode: GetGreenhouseMultiplier(bool isGreenhouse, CropData cropData, Season currentSeason)

    if !isGreenhouse:
        return 1.0  // 야외 재배 → 보정 없음

    if !cropData.isWinterExclusive:
        return 1.0  // 겨울 전용이 아닌 작물 → 보정 없음

    return cropData.greenhouseSynergyBonus
    // 잠정 1.2 → 겨울 전용 작물 판매가 20% 증가
    // → see docs/systems/economy-system.md 섹션 2.6.5
```

**필요 변경 사항**:

| 대상 | 변경 내용 |
|------|----------|
| `CropData` SO | `isWinterExclusive: bool` 필드 추가 (겨울 전용 3종에만 true) |
| `CropData` SO | `greenhouseSynergyBonus: float` 필드 추가 (기본값 잠정 1.2) |
| `PriceFluctuationSystem.CalculateSellPrice()` | 시그니처에 `bool isGreenhouse`, `CropData cropData` 파라미터 추가 |
| `EconomyConfig` SO | 변경 불필요 (보너스 값이 CropData에 개별 저장) |

[RISK] `isWinterExclusive` 필드를 CropData에 추가하면, `allowedSeasons == SeasonFlag.Winter`와 의미적으로 중복될 수 있다. 대안: `isWinterExclusive` 대신 `allowedSeasons == SeasonFlag.Winter` 조건으로 판별하여 필드 추가를 회피.

#### 3.7.3 시나리오 비교 및 권장사항

| 기준 | 시나리오 A (페널티) | 시나리오 B (보너스) |
|------|-------------------|-------------------|
| SO 스키마 변경 | EconomyConfig에 필드 1개 | CropData에 필드 1~2개 |
| 메서드 시그니처 변경 | CalculateSellPrice에 bool 1개 추가 | CalculateSellPrice에 bool + CropData 추가 |
| 영향 범위 | 비계절 온실 작물 전체 (광범위) | 겨울 전용 작물 3종만 (국소적) |
| 밸런스 조정 용이성 | EconomyConfig 하나로 글로벌 조절 | 작물별 개별 조절 가능 |
| 인벤토리 출처 태그 필요 | 예 (출하함 판매 시) | 예 (출하함 판매 시) |
| 구현 복잡도 | 낮음 | 중간 |

**[RESOLVED-BAL-010] 확정 구현 방향** (2026-04-07): 시나리오 A와 B를 **동시 적용**. 필요 변경 사항:

| 대상 | 변경 내용 |
|------|----------|
| `EconomyConfig` SO | `greenhouseOffSeasonPenalty = 0.8f` (확정값, FIX-030 적용 후) |
| `CropData` SO | `greenhouseSynergyBonus: float` 필드 추가 (겨울 전용 3종: 1.2, 나머지: 1.0) |
| `PriceFluctuationSystem.CalculateSellPrice()` | `CropData cropData, HarvestOrigin origin` 파라미터로 확정 [FIX-034] (→ see 섹션 1.2, 3.10) |
| `GetGreenhouseMultiplier()` | `HarvestOrigin origin, CropData cropData, Season currentSeason` 시그니처로 확정 [FIX-034] (→ see 섹션 3.10.3) |

수치 canonical: `docs/balance/crop-economy.md` 섹션 4.3.10 (-> see canonical)

**공통 필요 사항 (어느 시나리오든)**:

[RESOLVED-FIX-034] 수확물의 온실 출처 추적 문제를 `HarvestOrigin` enum(Outdoor, Greenhouse)을 `ItemSlot`에 추가하는 **방식 A**로 해소했다. 판매 시점에 출처를 조회하여 온실 보정 계수를 적용한다. 상세 설계는 **섹션 3.10** 참조.

대안이었던 방식 B(수확 시점 가격 확정)는 수급/날씨/축제 보정이 수확 시점에 고정되어 "언제 팔지"라는 핵심 경제 루프를 무력화하므로 기각했다(섹션 3.10.1 비교표 참조).

### 3.8 가격 클램핑

```
Pseudocode: ClampPrice(float rawPrice, PriceData data)

    basePrice = data.basePrice
    floor = basePrice * economyConfig.sellPriceFloor   // 기본 × 0.5
    ceiling = basePrice * economyConfig.sellPriceCeiling // 기본 × 2.0

    clampedPrice = Clamp(rawPrice, floor, ceiling)
    return RoundToInt(clampedPrice)
```

- `sellPriceFloor` (기본 0.5): 아무리 폭락해도 기본가의 50% 이하로 내려가지 않는다
- `sellPriceCeiling` (기본 2.0): 아무리 폭등해도 기본가의 200%를 넘지 않는다
- 이 값은 EconomyConfig SO에서 조정 가능

### 3.9 전체 계산 예시

**예시**: 토마토 판매 (Gold 품질, 가을, 폭풍, 수급 정상)
```
기본_판매가 = 60G  (-> see docs/design.md 4.2)
계절_보정  = 1.2   (가을에 토마토는 여름 작물이라 희소)
수급_보정  = 1.0   (수요 이내)
날씨_보정  = 1.15  (폭풍)
품질_보정  = 1.5   (Gold)
축제_보정  = 1.0   (축제 아님)

최종_판매가 = 60 × 1.2 × 1.0 × 1.15 × 1.5 × 1.0 × 1.0 = 124.2 → 124G
                                                         ^^^ 온실_보정 (야외 재배 = 1.0)

클램핑 확인: floor = 30, ceiling = 120
클램핑 적용: min(124, 120) = 120G
```

**예시 2 [BAL-010]**: 딸기 온실 비계절 판매 (시나리오 A 적용 시, Normal 품질, 겨울, 수급 정상)
```
기본_판매가 = 80G  (-> see docs/design.md 4.2)
계절_보정  = 1.5   (겨울에 봄 작물이라 희소)
수급_보정  = 1.0   (수요 이내)
날씨_보정  = 1.0   (맑음)
품질_보정  = 1.0   (Normal)
축제_보정  = 1.0   (축제 아님)
온실_보정  = 0.8   (비계절 온실 작물 페널티, -> see economy-system.md 2.6.5, 잠정값)

최종_판매가 = 80 × 1.5 × 1.0 × 1.0 × 1.0 × 1.0 × 0.8 = 96G

클램핑 확인: floor = 40, ceiling = 160
클램핑 적용: 96G (범위 내)

비교: 페널티 없을 때 = 80 × 1.5 = 120G → 페널티 적용으로 24G 감소
```

### 3.10 HarvestOrigin 추적 설계 [FIX-034, RESOLVED]

수확물의 온실/야외 출처를 판매 시점까지 추적하여, 온실 판매가 보정(-> see 섹션 3.7)을 정확히 적용하기 위한 설계이다. 섹션 3.7.3의 [RISK]를 해소한다.

#### 3.10.1 접근법 비교 및 결정

| 항목 | 방식 A: HarvestOrigin 태그 (채택) | 방식 B: 수확 시점 가격 확정 (기각) |
|------|----------------------------------|----------------------------------|
| 핵심 아이디어 | `ItemSlot`에 `origin: HarvestOrigin` 필드 추가 | 수확 시 `adjustedBasePrice: int` 저장 |
| 가격 변동 호환 | **완전 호환** — 수급/날씨/축제 보정 판매 시점 적용 | **비호환** — 온실 보정이 수확 시점에 고정 |
| 플레이어 전략 | "언제 팔지" 전략 유지 | 수확 타이밍 메타 강제 (의도하지 않음) |
| 스택 정책 | origin 다르면 별도 슬롯 (itemId+quality+origin 3중 매칭) | 스택 분리 구조적으로 동일 발생 |
| 파급 범위 | ItemSlot, 세이브/로드, 인벤토리 API | 동등하거나 더 복잡 |

**결정: 방식 A 채택.** economy-system.md의 핵심 설계 목표("판매 타이밍의 전략적 선택")와 충돌하지 않으며, 기존 품질별 슬롯 분리 패턴의 자연스러운 확장이다.

#### 3.10.2 HarvestOrigin enum 정의

```csharp
namespace SeedMind   // 최상위 네임스페이스 — ItemType, CropQuality와 동일 레벨
{
    /// <summary>
    /// 수확물의 재배 출처를 추적한다.
    /// 온실 판매가 보정(→ see economy-architecture.md 섹션 3.7) 적용 시 사용.
    /// </summary>
    public enum HarvestOrigin
    {
        Outdoor    = 0,   // 야외 농장 타일에서 수확
        Greenhouse = 1,   // 온실 내부 타일에서 수확
        Barn       = 2    // 외양간/목장 동물 생산물 (→ see docs/systems/livestock-architecture.md 섹션 9, ARC-019)
        // [OPEN] Cave = 3, Planter = 4 등 향후 확장
    }
}
```

**네임스페이스 근거**: `HarvestOrigin`은 Economy, Player(Inventory), Farming 세 시스템에서 참조하므로 특정 하위 네임스페이스에 넣으면 순환 참조 위험이 있다. `ItemType`, `CropQuality`와 동일한 최상위 패턴 적용(-> see `docs/systems/project-structure.md`).

**파일 위치**: `Assets/Scripts/Core/HarvestOrigin.cs`

#### 3.10.3 GetGreenhouseMultiplier() 확정 Pseudocode (A+B 동시 적용)

```
Pseudocode: GetGreenhouseMultiplier(HarvestOrigin origin, CropData cropData, Season currentSeason)

    // 1) 야외 수확 또는 동물 생산물 → 보정 없음
    if origin == HarvestOrigin.Outdoor:
        return 1.0

    if origin == HarvestOrigin.Barn:
        return 1.0  // 동물 생산물은 온실 보정 적용 대상 외 (→ see ARC-019 섹션 9)

    // 2) 온실 수확 + 해당 계절 작물 → 보정 없음
    if (cropData.allowedSeasons & (1 << currentSeason)) != 0:
        return 1.0

    // 3) 온실 수확 + 겨울 전용 작물 → 시너지 보너스 (시나리오 B)
    if cropData.allowedSeasons == SeasonFlag.Winter:
        return cropData.greenhouseSynergyBonus
        // → see docs/balance/crop-economy.md 섹션 4.3.10 for canonical 값 (1.2)

    // 4) 온실 수확 + 비계절 일반 작물 → 페널티 (시나리오 A)
    return economyConfig.greenhouseOffSeasonPenalty
    // → see docs/balance/crop-economy.md 섹션 4.3.10 for canonical 값 (0.8)
```

| 조건 | 반환값 | 설명 |
|------|--------|------|
| `origin == Outdoor` | 1.0 | 야외 재배 — 보정 없음 |
| `origin == Barn` | 1.0 | 동물 생산물 — 온실 보정 적용 대상 외 (ARC-019) |
| `origin == Greenhouse` + 해당 계절 작물 | 1.0 | 온실이지만 제철 — 보정 없음 |
| `origin == Greenhouse` + 겨울 전용 작물 | `cropData.greenhouseSynergyBonus` (→ see canonical) | 겨울 시너지 보너스 |
| `origin == Greenhouse` + 비계절 일반 작물 | `economyConfig.greenhouseOffSeasonPenalty` (→ see canonical) | 비계절 페널티 |

#### 3.10.4 캐시 전략 변경

기존 `_cachedPrices`는 `itemId → finalPrice` 단일 캐시였다. 온실 보정은 출처별로 다른 값이 나오므로, **온실 보정(`greenhouseMul`)은 캐시 밖에서 사후 적용**한다.

```
캐시 저장: seasonMul × supplyMul × weatherMul × festivalMul (origin 독립)
조회 시 적용: 캐시된 값 × qualityMul × GetGreenhouseMultiplier(origin, ...)
```

이렇게 하면 캐시 키 변경 없이 origin별 다른 최종가를 반환할 수 있다.

#### 3.10.5 스택 분리 정책

동일 `itemId` + 동일 `quality`이더라도 `origin`이 다르면 **별도 슬롯**에 저장한다.

**근거**: 판매 시점에 origin별로 최대 20% 이상 가격이 달라지므로, 한 스택에서 origin을 추적하는 것은 불가능하다. 기존 품질별 슬롯 분리 패턴의 자연스러운 확장이다.

**스택 키**: `itemId + quality + origin` 3중 매칭 (-> see `docs/systems/inventory-architecture.md` 섹션 3.3)

[RISK] 배낭 슬롯(→ see `docs/systems/inventory-system.md` 섹션 1.1 for canonical 슬롯 수)과 origin 분리로 후반부 슬롯 부족 가능. 온실은 레벨 5+ 콘텐츠이므로 창고 확장(→ see `docs/content/facilities.md`)으로 완화 가능하나 플레이테스트 검증 필요.

#### 3.10.6 파급 문서 목록

| 영향 문서 | 변경 위치 | 내용 |
|-----------|----------|------|
| `docs/systems/inventory-architecture.md` | 섹션 3.3 ItemSlot, 5.1 AddItem, 5.2 RemoveItem | origin 필드, CanStackWith 조건, API 파라미터 추가 |
| `docs/pipeline/data-pipeline.md` | Part I 섹션 3.2 JSON, Part II ItemSlotSaveData | origin 필드 추가 |
| `docs/systems/save-load-architecture.md` | 세이브 트리 내 ItemSlotSaveData 설명 | origin 필드 언급 |

[OPEN] 가공품의 origin 전파: 온실산 딸기로 만든 딸기잼에 온실 페널티 적용 여부. 현재 가공품은 품질 속성이 없으므로 origin 추적도 불필요할 가능성이 높다 — 별도 밸런스 검토 필요.

[OPEN] 야생 채집(forageable) 시스템 추가 시 `HarvestOrigin.Wild` 값 필요 여부 — 현재 scope 외이므로 보류.

### 3.11 동물 생산물 및 낚시 생산물 수급 정책 (FIX-044)

[RESOLVED-FIX-044] 동물 생산물 수급 변동 정책: 카테고리별 별도 파라미터(옵션 B) 채택.

| SupplyCategory | demandThreshold | supplyDropRate | minSupplyMultiplier | 비고 |
|----------------|----------------|----------------|---------------------|------|
| Crop | 20 | 0.02 | 0.70 | (-> see economy-system.md 섹션 7.3) |
| AnimalProduct | 35 | 0.008 | 0.85 | 안정 수입 보전, 대량 생산 시 완만한 하락 (-> see economy-system.md 섹션 7.3) |
| Fish | 30 | 0.01 | 0.80 | 중간 수급 민감도, 보조 수입 보전 (-> see economy-system.md 섹션 7.3) |
| ProcessedGoods | -1 | -- | -- | 수급 보정 면제 (가공 투자 동기 보전) |

**설계 근거**: 목축은 "안정적 수입원"으로 설계(BAL-008)되어 작물의 가격 변동성과 대조된다. 동물 생산물에 작물과 동일한 수급 보정을 적용하면 후기 다두 사육 시 수익이 급격히 감소하여 목축 진입 동기가 무너진다. AnimalProduct 카테고리는 민감도를 절반(supplyDropRate 0.01)으로 낮추고 최저 배수를 0.85로 제한하여 대량 생산 시에도 최소 수익성이 보장된다. ProcessedGoods(가공품)는 가공 투자 ROI 보전을 위해 수급 보정에서 완전 면제한다.

**파급 영향**:
- `EconomyConfig.categorySupplyParams` 배열에 4개 SupplyParams 원소를 MCP로 설정 (-> see 섹션 4.1)
- `PriceData.supplyCategory` 필드를 품목별 SO에 설정 (-> see 섹션 4.2)
- `PriceFluctuationSystem.GetSupplyMultiplier()` 시그니처에 SupplyCategory 파라미터 추가 (-> see 섹션 1.2)
- `TransactionLog.GetItemSalesCount()` 메서드명 변경 (cropId 한정 -> itemId 범용) (-> see 섹션 1 클래스 다이어그램)

---

## 4. 데이터 구조

### 4.1 EconomyConfig (ScriptableObject)

```csharp
namespace SeedMind.Economy.Data
{
    [CreateAssetMenu(fileName = "EconomyConfig", menuName = "SeedMind/EconomyConfig")]
    public class EconomyConfig : ScriptableObject
    {
        [Header("기본 설정")]
        public int startingGold = 500;           // 게임 시작 골드
        public int maxGold = 999999;             // 골드 상한

        [Header("가격 변동")]
        public float sellPriceFloor = 0.5f;      // 판매가 하한 배수
        public float sellPriceCeiling = 2.0f;    // 판매가 상한 배수
        // [DEPRECATED by FIX-044] supplyDecayRate → categorySupplyParams로 대체
        // public float supplyDecayRate = 0.1f;

        [Header("온실 보정 [BAL-010, RESOLVED]")]
        // 비계절 온실 작물 판매가 페널티 (전 계절 적용). → see docs/systems/economy-system.md 섹션 2.6.5
        public float greenhouseOffSeasonPenalty = 0.8f; // BAL-010 확정값 (→ see docs/balance/crop-economy.md 섹션 4.3.10)

        [Header("카테고리별 수급 파라미터")]
        // → see docs/systems/economy-system.md 섹션 7.3
        public SupplyParams[] categorySupplyParams = new SupplyParams[4];
        // indexed by SupplyCategory (0=Crop, 1=AnimalProduct, 2=Fish, 3=ProcessedGoods)

        [Header("로그")]
        public int transactionLogCapacity = 200; // 거래 로그 최대 보관 수

        /// <summary>
        /// SupplyCategory 인덱스로 수급 파라미터를 반환한다.
        /// 배열 범위 초과 시 Crop 기본값을 반환한다.
        /// </summary>
        public SupplyParams GetSupplyParams(SupplyCategory category)
        {
            int idx = (int)category;
            if (idx >= 0 && idx < categorySupplyParams.Length)
                return categorySupplyParams[idx];
            return categorySupplyParams[0]; // fallback to Crop
        }
    }

    /// <summary>
    /// 카테고리별 수급 보정 파라미터.
    /// EconomyConfig.categorySupplyParams 배열의 원소로 사용된다.
    /// → see docs/systems/economy-system.md 섹션 7.3 (canonical 수치)
    /// </summary>
    [System.Serializable]
    public class SupplyParams
    {
        public int demandThreshold;        // 계절당 수요 기준치. -1 = 수급 보정 면제
        // → see docs/systems/economy-system.md 섹션 7.3
        public float supplyDropRate;       // 초과 판매 시 가격 하락률
        // → see docs/systems/economy-system.md 섹션 7.3
        public float minSupplyMultiplier;  // 수급 보정 최저 배수 (하한)
        // → see docs/systems/economy-system.md 섹션 7.3
    }
}
```

에셋 이름: `SO_EconomyConfig.asset` (-> see `docs/systems/project-structure.md` 6.1절 네이밍 규칙)  
저장 경로: `Assets/_Project/Data/Config/`

### 4.2 PriceData (ScriptableObject)

```csharp
namespace SeedMind.Economy.Data
{
    [CreateAssetMenu(fileName = "NewPriceData", menuName = "SeedMind/PriceData")]
    public class PriceData : ScriptableObject
    {
        [Header("기본 정보")]
        public string itemId;                    // "potato", "seed_potato" 등
        public string displayName;               // "감자", "감자 씨앗" 등
        public PriceCategory category;           // Crop, Seed, Tool, Building, Processed

        [Header("기본 가격")]
        public int basePrice;                    // 기본 가격 (→ see docs/design.md 4.2)

        [Header("계절 보정")]
        public float[] seasonMultipliers = new float[4] { 1.0f, 1.0f, 1.0f, 1.0f };
        // [0]=Spring, [1]=Summer, [2]=Autumn, [3]=Winter

        [Header("수급 설정")]
        public SupplyCategory supplyCategory = SupplyCategory.Crop;
        // → see docs/systems/economy-system.md 섹션 7.3
        public int demandThreshold = 20;         // 계절당 수요 기준치 — 실제값은 EconomyConfig.categorySupplyParams에서 카테고리별 관리 (→ see economy-system.md 섹션 7.3)
        // demandThreshold = -1 이면 수급 보정 면제

        [Header("품목별 오버라이드")]
        public bool isAffectedByWeather = true;  // 날씨 보정 적용 여부
    }

    public enum PriceCategory
    {
        Crop,       // 수확물 (판매 전용)
        Seed,       // 씨앗 (구매 전용)
        Tool,       // 도구 (구매 전용)
        Building,   // 시설 (구매 전용)
        Fertilizer, // 비료 (구매 전용)
        Processed   // 가공품 (판매/구매)
    }

    /// <summary>
    /// 수급 보정 카테고리. PriceData.supplyCategory에서 사용.
    /// 카테고리별 수급 파라미터(demandThreshold, supplyDropRate, minSupplyMultiplier)는
    /// EconomyConfig.categorySupplyParams 배열로 관리된다.
    /// → see docs/systems/economy-system.md 섹션 7.3
    /// </summary>
    public enum SupplyCategory
    {
        Crop            = 0,  // 작물: 높은 수급 민감도
        AnimalProduct   = 1,  // 동물 생산물: 낮은 수급 민감도 (안정 수입 보전)
        Fish            = 2,  // 낚시 생산물: 중간 수급 민감도 (보조 수입 보전, → see economy-system.md 섹션 2.6.2.1)
        ProcessedGoods  = 3   // 가공품: 수급 보정 면제
    }
}
```

에셋 이름: `SO_Price_Potato.asset`, `SO_Price_Seed_Potato.asset` 등  
저장 경로: `Assets/_Project/Data/Prices/`

### 4.3 ShopData (ScriptableObject)

```csharp
namespace SeedMind.Economy.Data
{
    [CreateAssetMenu(fileName = "NewShopData", menuName = "SeedMind/ShopData")]
    public class ShopData : ScriptableObject
    {
        [Header("상점 정보")]
        public string shopId;                    // "general_store"
        public string shopName;                  // "잡화점"

        [Header("운영")]
        public int openHour = 8;                 // 오전 8시 오픈
        public int closeHour = 18;               // 오후 6시 마감 (잡화 상점 기준 → see docs/systems/time-season.md 섹션 1.7)

        [Header("판매 품목")]
        public ShopItemEntry[] items;            // 판매 품목 목록
    }

    [System.Serializable]
    public class ShopItemEntry
    {
        public PriceData priceData;              // 가격 데이터 참조
        public int stockLimit = -1;              // -1 = 무제한, 양수 = 일일 재고 한도
        public int requiredLevel = 0;            // 해금 레벨 (→ see docs/design.md 4.2, 4.6)
        public SeasonFlag availableSeasons;      // 판매 가능 계절 (비트마스크)

        // 참조용: 실제 아이템 데이터
        public ScriptableObject itemReference;   // CropData, ToolData, FertilizerData 등
    }
}
```

에셋 이름: `SO_Shop_GeneralStore.asset`  
저장 경로: `Assets/_Project/Data/Config/`

### 4.4 CropQuality Enum

```csharp
namespace SeedMind.Economy
{
    public enum CropQuality
    {
        Normal   = 0,   // 일반 — 기본 판매가
        Silver   = 1,   // 은별 — x1.25
        Gold     = 2,   // 금별 — x1.5
        Iridium  = 3    // 이리듐별 — x2.0
    }
}
```

(-> see `docs/systems/crop-growth.md` for 품질 결정 로직)

### 4.5 Transaction (직렬화 가능 데이터 클래스)

```csharp
namespace SeedMind.Economy
{
    [System.Serializable]
    public class Transaction
    {
        public TransactionType type;      // Buy, Sell
        public string itemId;             // "potato", "seed_potato"
        public int quantity;
        public int unitPrice;             // 단가
        public int totalPrice;            // 총액 (= unitPrice * quantity)
        public int day;                   // 거래 시점 (TimeManager.TotalElapsedDays)
        public Season season;             // 거래 시점 계절
        public HarvestOrigin origin;      // [FIX-034] 수확 출처 (Buy 거래는 Outdoor 고정)
                                          // → see docs/systems/economy-architecture.md 섹션 3.10
    }

    public enum TransactionType
    {
        Buy,
        Sell
    }
}
```

### 4.6 저장 데이터 구조

```csharp
namespace SeedMind.Economy
{
    [System.Serializable]
    public class EconomySaveData
    {
        public int currentGold;
        public TransactionLogSaveData transactionLog;
        public PriceFluctuationSaveData priceFluctuation;
    }

    [System.Serializable]
    public class TransactionLogSaveData
    {
        public Transaction[] entries;     // 최근 N개 거래
        public long totalEarned;
        public long totalSpent;
    }

    [System.Serializable]
    public class PriceFluctuationSaveData
    {
        // 계절별 누적 판매량 (수급 보정 복원용)
        public Dictionary<string, int> seasonSalesCount;
    }
}
```

### 4.7 SaveManager 연동

```
저장:
    SaveManager.Save()
        ├── ... (TimeManager, WeatherSystem 등)
        └── EconomyManager.GetSaveData() → EconomySaveData
            ├── currentGold
            ├── TransactionLog.GetSaveData() → TransactionLogSaveData
            └── PriceFluctuationSystem.GetSaveData() → PriceFluctuationSaveData

로드:
    SaveManager.Load()
        ├── ... (TimeManager, WeatherSystem 등)
        └── EconomyManager.LoadSaveData(economySaveData)
            ├── _currentGold 복원
            ├── TransactionLog.LoadSaveData(logData)
            └── PriceFluctuationSystem.LoadSaveData(priceData)
                → 수급 보정 복원, 가격 재계산
```

(-> see `docs/systems/time-season-architecture.md` 섹션 7.3 for SaveManager 저장/로드 흐름 패턴)

---

## 5. 데이터 흐름

### 5.1 작물 판매 흐름

[FIX-034] `isGreenhouse` 파라미터를 `HarvestOrigin origin`으로 교체. 상세 설계는 섹션 3.10 참조.

```
PlayerInventory → ShopSystem.TrySellCrop(crop, qty, quality, origin)  // [FIX-034] origin
    │
    ├── 1) 상점 오픈 확인 (ShopSystem.IsOpen)
    │
    ├── 2) 가격 계산
    │       EconomyManager.GetSellPrice(crop, quality, origin)         // [FIX-034]
    │           → PriceFluctuationSystem.CalculateSellPrice(priceData, quality, cropData, origin)
    │               → baseMul = seasonMul × supplyMul × weatherMul × qualityMul × festivalMul
    │               → greenhouseMul = GetGreenhouseMultiplier(origin, cropData, currentSeason)
    │               → finalPrice = basePrice × baseMul × greenhouseMul
    │               → ClampPrice()
    │           → return finalPrice
    │
    ├── 3) 거래 실행
    │       totalGold = finalPrice × qty
    │       EconomyManager.AddGold(totalGold, "sell_crop")
    │       PlayerInventory.RemoveItem(crop.itemId, qty, quality, origin)  // [FIX-034]
    │
    ├── 4) 거래 기록
    │       Transaction tx = new Transaction {
    │           type=Sell, itemId, qty, unitPrice, totalPrice, day, season,
    │           origin  // [FIX-034]
    │       }
    │       TransactionLog.AddEntry(tx)
    │       EconomyManager.OnTransactionComplete?.Invoke(tx)
    │
    └── 5) 이벤트 발행
            ShopSystem.OnItemSold?.Invoke(crop, qty, totalGold)
```

### 5.2 씨앗 구매 흐름

```
ShopUI → ShopSystem.TryBuyItem(item, qty)
    │
    ├── 1) 상점 오픈 확인, 재고 확인, 레벨 확인
    │
    ├── 2) 가격 계산
    │       EconomyManager.GetBuyPrice(itemId)
    │           → PriceFluctuationSystem.CalculateBuyPrice(priceData)
    │               → basePrice × seasonMul × festivalMul
    │           → return finalPrice
    │
    ├── 3) 골드 확인
    │       totalCost = finalPrice × qty
    │       if !EconomyManager.CanAfford(totalCost): return false
    │
    ├── 4) 거래 실행
    │       EconomyManager.SpendGold(totalCost, "buy_seed")
    │       PlayerInventory.Add(item.itemReference, qty)
    │
    ├── 5) 재고 갱신 (일일 재고 한도가 있는 경우)
    │       item.stockLimit -= qty
    │
    ├── 6) 거래 기록
    │       Transaction tx = new Transaction { type=Buy, ... }
    │       TransactionLog.AddEntry(tx)
    │       EconomyManager.OnTransactionComplete?.Invoke(tx)
    │
    └── 7) 이벤트 발행
            ShopSystem.OnShopPurchased?.Invoke(item, qty)
            EconomyEvents.OnShopPurchased?.Invoke(item, qty)  // 정적 이벤트 허브 경유 — AchievementManager·TutorialTriggerSystem 구독
```

### 5.3 일일 가격 갱신 흐름

```
TimeManager.OnDayChanged (priority 40)
    │
    ▼
EconomyManager.OnDayChangedHandler(int newDay)
    │
    ├── 1) PriceFluctuationSystem.SetWeather(WeatherSystem.CurrentWeather)
    │       → _isDirty = true
    │
    ├── 2) PriceFluctuationSystem.RecalculateAllPrices()
    │       → foreach PriceData in _priceDataMap:
    │           finalPrice = CalculateSellPrice(data, Normal, null, Outdoor)
    │           // 캐시는 origin 독립 기본가 저장 (qualityMul × greenhouseMul은 조회 시 사후 적용)
    │           // → see 섹션 3.10.4 캐시 전략
    │           _cachedPrices[data.itemId] = finalPrice
    │       → _isDirty = false
    │
    ├── 3) ShopSystem.RefreshStock()  (일일 재고 리셋)
    │
    └── 4) EconomyManager.OnPriceChanged?.Invoke()
            → ShopUI가 구독하여 가격표 갱신
```

---

## 6. 성능 고려사항

### 6.1 가격 재계산 빈도 및 캐싱

**재계산 타이밍**: 하루에 1회(OnDayChanged), 계절 변경 시 1회(OnSeasonChanged), 축제 시작/종료 시 각 1회.

**캐싱 전략**: `PriceFluctuationSystem._cachedPrices`에 품목별 최종 가격을 Dictionary로 캐시한다. `_isDirty` 플래그로 재계산 필요 여부를 추적하며, `GetSellPrice()`/`GetBuyPrice()` 호출 시 캐시가 유효하면 Dictionary lookup만 수행한다 (O(1)).

```
현재 품목 수: 작물 8종 + 씨앗 8종 + 도구 5종 + 비료 4종 + 시설 4종 = ~29종
재계산 비용: 29 × (곱셈 5회 + Clamp 1회) = ~170 연산 → 무시 가능
```

따라서 현재 스코프에서는 캐싱이 선택적이나, 확장성을 위해 캐싱 구조를 미리 설계한다.

### 6.2 TransactionLog 메모리 관리

TransactionLog는 **CircularBuffer** 패턴을 사용하여 최대 `transactionLogCapacity`(기본 200) 건까지만 보관한다.

```
CircularBuffer 동작:
    - 내부 배열 크기 = capacity
    - 새 거래 추가 시 writeIndex를 순환 이동
    - capacity 초과 시 가장 오래된 항목을 자동 덮어씀
    - 누적 통계(totalEarned, totalSpent)는 별도 필드로 관리하여 삭제된 항목의 통계 손실 방지
```

**메모리 추정**: Transaction 1건 = ~64 bytes, 200건 = ~12.8 KB → 무시 가능

**수급 보정용 카운트**: `GetItemSalesCount()`는 현재 계절 내 entries만 순회한다. 최악 200건 전체 순회해도 O(200)으로 성능 문제 없음. 빈도가 높아지면(매 프레임 호출 등) Dictionary 캐시 추가를 고려한다.

### 6.3 가격 조회 최적화

ShopUI가 열려 있는 동안 매 프레임 가격을 조회하지 않도록 설계한다:
- ShopUI는 **열릴 때 1회** + **OnPriceChanged 이벤트 수신 시** 가격표를 갱신
- 인벤토리에서 판매 미리보기 가격은 마우스 호버 시 1회 계산, 커서 이동 전까지 캐시

---

## 7. MCP 구현 계획

### Phase A: 기본 경제 프레임워크 (MCP 5단계)

```
Step A-1: Scripts/Economy/ 폴더 구조 생성
          → Economy/EconomyManager.cs
          → Economy/ShopSystem.cs
          → Economy/PriceFluctuationSystem.cs
          → Economy/TransactionLog.cs
          → Economy/Data/EconomyConfig.cs
          → Economy/Data/PriceData.cs
          → Economy/Data/ShopData.cs

Step A-2: SeedMind.Economy.asmdef 생성
          → 참조: SeedMind.Core, SeedMind.Farm
          → Scripts/Economy/ 폴더에 배치

Step A-3: EconomyConfig SO 인스턴스 생성
          → Assets/_Project/Data/Config/SO_EconomyConfig.asset
          → startingGold=500, maxGold=999999
          → sellPriceFloor=0.5, sellPriceCeiling=2.0
          → categorySupplyParams[4] (-> see 섹션 3.11 수급 정책 테이블)
          → transactionLogCapacity=200

Step A-4: SCN_Farm 씬의 "--- ECONOMY ---" 하위에 컴포넌트 부착
          → "EconomyManager" GameObject에 EconomyManager.cs 부착
            → _economyConfig 필드에 SO_EconomyConfig 연결
          → "Shop" GameObject에 ShopSystem.cs 부착

Step A-5: Play Mode 테스트
          → EconomyManager 초기화 확인 (Console: "EconomyManager initialized, gold=500")
          → AddGold(100, "test") 호출 → OnGoldChanged 이벤트 확인
          → SpendGold(50, "test") 호출 → 골드 차감 확인
```

### Phase B: 가격 데이터 (MCP 4단계)

```
Step B-1: 기본 작물 PriceData SO 생성 (Assets/_Project/Data/Prices/)
          → SO_Price_Potato:   basePrice=30, seasonMul=[1.0, 0.8, 1.2, 1.5], demandThreshold=20
          → SO_Price_Carrot:   basePrice=35, seasonMul=[1.0, 0.9, 1.0, 1.3], demandThreshold=20
          → SO_Price_Tomato:   basePrice=60, seasonMul=[0.9, 1.0, 1.2, 1.5], demandThreshold=15
          → SO_Price_Corn:     basePrice=100, seasonMul=[0.9, 1.0, 1.1, 1.4], demandThreshold=12
          (가격 기본값 → see docs/design.md 4.2)

Step B-2: 씨앗 PriceData SO 생성
          (씨앗 가격 = floor(작물 판매가 × 0.5), canonical → see docs/systems/economy-system.md 섹션 2.2)
          → SO_Price_Seed_Potato:  basePrice=15, category=Seed, demandThreshold=-1 (수급 보정 없음)
          → SO_Price_Seed_Carrot:  basePrice=17, category=Seed
          → SO_Price_Seed_Tomato:  basePrice=30, category=Seed
          → SO_Price_Seed_Corn:    basePrice=50, category=Seed

Step B-3: ShopData SO 생성
          → SO_Shop_GeneralStore:
            shopName="잡화점", openHour=8, closeHour=18
            items: 모든 씨앗 + 기본 비료 + 속성장 비료
            각 항목에 requiredLevel, availableSeasons 설정

Step B-4: PriceFluctuationSystem에 PriceData 매핑 등록
          → EconomyManager._priceDataMap에 모든 SO 등록
          → Play Mode에서 GetSellPrice("potato", Normal) 호출 → 기본가 30G 반환 확인
```

### Phase C: 가격 변동 연동 (MCP 3단계)

```
Step C-1: TimeManager 이벤트 연동
          → EconomyManager.OnEnable에서 RegisterOnDayChanged(40, handler)
          → OnDayChanged 시 RecalculateAllPrices() 호출 확인
          → Console: "Prices recalculated: potato=30G, tomato=60G, ..."

Step C-2: 계절/날씨 보정 테스트
          → TimeManager.SkipToNextDay() 반복 → 계절 변경 시 가격 변동 확인
          → WeatherSystem.SetWeather(Stormy) 수동 호출 → 폭풍 보정 확인
          → Console 로그로 보정 계수 출력

Step C-3: 수급 보정 테스트
          → SellCrop(potato, 25, Normal) 반복 호출 (demandThreshold=20 초과)
          → 다음 날 GetSellPrice("potato") 확인 → 가격 하락 확인
          → Console: "Supply penalty for potato: sales=25, threshold=20, multiplier=0.975"
```

### Phase D: 상점 UI 연동 (MCP 3단계)

```
Step D-1: ShopUI와 ShopSystem 연결
          → Canvas_Overlay/ShopPanel 활성화 시 ShopSystem.Open() 호출
          → 가격표 표시: 각 항목의 GetDisplayPrice() 결과 바인딩

Step D-2: 구매/판매 버튼 연결
          → 구매 버튼 → ShopSystem.TryBuyItem()
          → 판매 버튼 → ShopSystem.TrySellCrop()
          → OnGoldChanged → HUD GoldDisplay 즉시 갱신

Step D-3: 통합 테스트
          → 상점 열기 → 씨앗 구매 → 골드 차감 확인
          → 인벤토리에서 작물 판매 → 골드 증가 확인
          → 거래 후 TransactionLog 확인 (디버그 콘솔)
```

---

## Open Questions

- [OPEN] 가공품(잼, 주스 등) 판매 시스템은 가공소(Processor) 해금 후 구현. PriceData에 PriceCategory.Processed는 미리 정의했으나, 가공품 목록 및 레시피는 미정
- [OPEN] 상점이 복수 개(잡화점, 대장간 등)인지, 단일 통합 상점인지 미확정. 현재 설계는 ShopData SO를 교체하면 복수 상점 지원 가능
- [OPEN] 날씨 보정 수치(0.95, 1.15, 1.1)가 적절한지 플레이테스트 전까지 검증 불가
- [RESOLVED-FIX-044] demandThreshold의 카테고리별 기본값 확정: Crop=20, AnimalProduct=40, Fish=40, ProcessedGoods=-1(면제). 품목별 미세 조정은 플레이테스트 후 결정 (-> see 섹션 3.11)

---

## Risks

- [RISK] PriceFluctuationSaveData에 `Dictionary<string, int>`를 사용하나, Unity의 JsonUtility는 Dictionary를 직접 직렬화하지 못한다. Newtonsoft.Json 사용 또는 List<KeyValuePair> 변환 래퍼가 필요하다
- [RISK] CircularBuffer는 Unity 표준 라이브러리에 없다. 자체 구현 필요. 단순 배열 + writeIndex 래핑으로 구현 가능하나 에지 케이스(빈 버퍼, 한 바퀴 순환 시점) 테스트 필요
- [RISK] 수급 보정의 recentSales 계절 리셋 시, 저장/로드 시점이 계절 전환 직전이면 리셋이 누락될 수 있다. LoadSaveData 시 현재 계절과 저장된 계절이 다르면 수급 카운트를 강제 리셋하는 로직 필요
- [RISK] MCP를 통한 ScriptableObject 배열/참조 필드 설정이 제한적일 수 있다 (-> see `docs/architecture.md` Risks). PriceData의 seasonMultipliers float[4] 배열을 MCP가 정확히 설정할 수 있는지 사전 검증 필요
- [RISK] 축제 보정과 수급 보정이 동시에 적용되면 가격이 예상 범위를 크게 벗어날 수 있다. 클램핑이 최종 방어선이나, 축제 기간에는 수급 보정을 약화(×0.5)하는 등의 추가 로직이 필요할 수 있다

---

## Cross-references

- `docs/architecture.md` 3절 (Economy 폴더 구조), 6절 (데이터 관리)
- `docs/design.md` 4.2절 (작물 가격 canonical), 4.4절 (경제 시스템 개요), 4.6절 (시설 비용)
- `docs/systems/project-structure.md` 2절 (SeedMind.Economy 네임스페이스), 3.2절 (의존성 매트릭스), 4절 (SeedMind.Economy.asmdef)
- `docs/systems/time-season-architecture.md` 2.5절 (FestivalData), 4.3절 (OnDayChanged 우선순위 40), 4.4절 (OnSeasonChanged 우선순위 30), 7절 (저장/로드 패턴)
- `docs/systems/farming-architecture.md` 6절 (FarmEvents.OnCropHarvested)
- `docs/systems/crop-growth.md` (품질 등급, 성장 공식)
- `docs/balance/crop-economy.md` 섹션 4.3.10 (BAL-010 온실 보정 확정 수치 canonical — x0.8 페널티, x1.2 시너지)
- `docs/systems/inventory-architecture.md` -- ItemSlot.origin 필드, AddItem/RemoveItem API (FIX-034, 섹션 3.10)
- `docs/pipeline/data-pipeline.md` -- ItemSlotSaveData JSON/C# 스키마 (섹션 3.2~3.3, FIX-034)
- `docs/systems/economy-system.md` 섹션 7.3 -- 카테고리별 수급 파라미터 canonical 수치 (FIX-044)
- `docs/systems/livestock-system.md` -- 동물 생산물 수익 설계, BAL-008 안정 수입원 원칙
- `docs/systems/fishing-system.md` -- 낚시 생산물 수급 정책 (Fish 카테고리, FIX-044)

---

*이 문서는 Claude Code가 기존 아키텍처 문서와의 일관성을 검증하며 자율적으로 작성했습니다.*
