# 목축/낙농 시스템 MCP 태스크 시퀀스 (ARC-024)

> AnimalManager, AnimalData SO, LivestockConfig SO, 행복도 시스템, Zone E 연동, 세이브/로드, UI, 이벤트 통합을 MCP for Unity 태스크로 상세 정의
> 작성: Claude Code (Opus 4.6) | 2026-04-07
> Phase 1 | 문서 ID: ARC-024

---

## 1. 개요

### 1.1 목적

이 문서는 `docs/systems/livestock-architecture.md`(ARC-019) Part II에서 요약된 MCP 구현 계획(Phase A~E)을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 목축/낙농 시스템의 데이터 레이어(AnimalData SO, LivestockConfig SO), 시스템 레이어(AnimalManager, HappinessCalculator, LivestockEvents), 씬 배치, 기존 시스템 확장(XPSource, HarvestOrigin, GameSaveData), UI, 이벤트 연동을 완성한다.

### 1.2 의존성

```
목축/낙농 시스템 MCP 태스크 의존 관계:
├── SeedMind.Core     (TimeManager, SaveManager, EventBus, ISaveable, HarvestOrigin)
├── SeedMind.Farm     (FarmZoneManager, ZoneType, FarmGrid)
├── SeedMind.Level    (ProgressionManager, XPSource)
├── SeedMind.Economy  (EconomyManager -- 동물 구매 골드 차감, 생산물 판매)
├── SeedMind.Player   (InventoryManager -- 사료 차감, 생산물 추가)
├── SeedMind.Building (BuildingManager -- 외양간/닭장 건설 이벤트)
└── SeedMind.UI       (AnimalShopUI, AnimalCareUI -- 본 태스크 생성 대상)
```

(-> see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 1.3 전제 조건 (완료된 태스크 의존성)

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조, SCN_Farm 기본 계층 (MANAGERS, FARM, PLAYER, UI), Canvas_HUD, Canvas_Overlay |
| ARC-003 | `docs/mcp/farming-tasks.md` | Phase A~C 전체 | FarmGrid, CropData SO, ToolData SO, GrowthSystem |
| ARC-007 | `docs/mcp/facilities-tasks.md` | Phase A~D 전체 | BuildingManager, BuildingEvents |
| ARC-013 | `docs/mcp/inventory-tasks.md` | Phase A~C 전체 | InventoryManager, PlayerInventory, ItemSlot |
| DES-012 | `docs/systems/farm-expansion.md` | 설계 완료 | Zone E(목장) 구조 정의 |

### 1.4 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `Canvas_HUD` (HUD UI 루트) | ARC-002 Phase B |
| `Canvas_Overlay` (오버레이 UI 루트) | ARC-002 Phase B |
| `DataRegistry` (SO 로드 시스템) | `docs/pipeline/data-pipeline.md` Part II |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A |
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `--- FARM ---` (씬 계층 부모) | ARC-002 Phase B |
| `FarmGrid` (경작 그리드) | ARC-003 |
| `BuildingManager` (시설 시스템) | ARC-007 |
| `InventoryManager` (인벤토리) | ARC-013 |
| `EconomyManager` (경제) | economy-architecture.md 기반 |
| `ProgressionManager` (레벨/XP) | BAL-002 기반 |
| `TimeManager` (시간/시즌) | ARC-004 기반 |
| `FarmZoneManager` (구역 관리) | ARC-023 기반 |

### 1.5 총 MCP 호출 예상 수

| 태스크 | 설명 | MCP 호출 수 |
|--------|------|------------|
| L-1 | 스크립트 생성 (enum, struct, class, MonoBehaviour) | 18회 |
| L-2 | AnimalData SO 에셋 생성 (4종 + LivestockConfig 1종) | 65회 |
| L-3 | FeedData 사료 아이템 등록 (기존 ItemData SO 확장) | 28회 |
| L-4 | 씬 배치 (AnimalManager 배치, SO 참조 연결) | 6회 |
| L-5 | 기존 시스템 확장 (XPSource, HarvestOrigin, GameSaveData) | 6회 |
| L-6 | Zone E 연동 (외양간/닭장 건설 핸들러) | 8회 |
| L-7 | UI 생성 (동물 구매 팝업, 돌봄 패널) | 53회 |
| L-8 | 이벤트 연동 (LivestockEvents 구독 설정) | 4회 |
| L-9 | 통합 테스트 시퀀스 | 33회 |
| **합계** | | **~221회** |

[RISK] 총 ~221회 MCP 호출은 상당하다. L-2(SO 에셋 생성)의 65회를 Editor 스크립트(CreateAnimalAssets.cs)로 일괄 생성하면 ~8회로 감소 가능. L-7(UI 생성)의 53회도 Editor 스크립트로 ~6회로 감소 가능.

---

## 2. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋/스크립트 폴더 생성 | L-1, L-2 |
| `create_script` | C# 스크립트 파일 생성 | L-1 |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | L-2, L-3 |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | L-2~L-8 전체 |
| `create_object` | 빈 GameObject 생성 | L-4, L-7 |
| `add_component` | MonoBehaviour 컴포넌트 부착 | L-4, L-7 |
| `set_parent` | 오브젝트 부모 설정 | L-4, L-7 |
| `save_scene` | 씬 저장 | L-4, L-7, L-9 |
| `modify_script` | 기존 스크립트 수정 (enum 확장 등) | L-5 |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | L-9 |
| `execute_method` | 런타임 메서드 호출 (테스트) | L-9 |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | L-9 |

[RISK] `create_scriptable_object`로 AnimationCurve 필드(LivestockConfig.productionMultiplierCurve)를 설정할 수 있는지 불확실. MCP for Unity에서 AnimationCurve 키프레임 설정을 지원하지 않을 경우, 코드 내 fallback 커브 초기화가 필요하다. (-> see `docs/systems/livestock-architecture.md` Risks 섹션 항목 3)

---

## 3. 필요 C# 스크립트 목록

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 태스크 순서대로 작성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/Livestock/AnimalType.cs` | `AnimalType` (enum) | `SeedMind.Livestock` | L-1 Phase 1 |
| S-02 | `Scripts/Livestock/CollectResult.cs` | `CollectResult` (enum) | `SeedMind.Livestock` | L-1 Phase 1 |
| S-03 | `Scripts/Livestock/AnimalProductInfo.cs` | `AnimalProductInfo` (struct) | `SeedMind.Livestock` | L-1 Phase 1 |
| S-04 | `Scripts/Livestock/Data/AnimalData.cs` | `AnimalData` (SO) | `SeedMind.Livestock.Data` | L-1 Phase 2 |
| S-05 | `Scripts/Livestock/Data/LivestockConfig.cs` | `LivestockConfig` (SO) | `SeedMind.Livestock.Data` | L-1 Phase 2 |
| S-06 | `Scripts/Livestock/AnimalInstance.cs` | `AnimalInstance` (Plain C#) | `SeedMind.Livestock` | L-1 Phase 2 |
| S-07 | `Scripts/Livestock/Data/AnimalSaveData.cs` | `AnimalSaveData`, `AnimalInstanceSaveData` | `SeedMind.Livestock` | L-1 Phase 2 |
| S-08 | `Scripts/Livestock/HappinessCalculator.cs` | `HappinessCalculator` (static) | `SeedMind.Livestock` | L-1 Phase 3 |
| S-09 | `Scripts/Livestock/LivestockEvents.cs` | `LivestockEvents` (static) | `SeedMind.Livestock` | L-1 Phase 3 |
| S-10 | `Scripts/Livestock/AnimalManager.cs` | `AnimalManager` (MonoBehaviour Singleton) | `SeedMind.Livestock` | L-1 Phase 4 |
| S-11 | `Scripts/UI/AnimalShopUI.cs` | `AnimalShopUI` (MonoBehaviour) | `SeedMind.UI` | L-1 Phase 5 |
| S-12 | `Scripts/UI/AnimalCareUI.cs` | `AnimalCareUI` (MonoBehaviour) | `SeedMind.UI` | L-1 Phase 5 |
| S-13 | `Scripts/UI/AnimalSlotUI.cs` | `AnimalSlotUI` (MonoBehaviour) | `SeedMind.UI` | L-1 Phase 5 |

(모든 경로 접두어: `Assets/_Project/`)

**컴파일 순서**: S-01~S-03 -> S-04~S-07 -> S-08~S-09 -> S-10 -> S-11~S-13. 각 Phase 사이에 Unity 컴파일 대기가 필요하다.

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 특히 S-10(AnimalManager)은 SeedMind.Core, SeedMind.Farm, SeedMind.Level, SeedMind.Economy, SeedMind.Player에 대한 참조가 필요하므로, asmdef 의존성 누락 시 컴파일 실패한다.

---

## 4. 태스크 목록

---

### L-1: 스크립트 생성

**목적**: 목축/낙농 시스템의 모든 C# 스크립트를 생성한다.

**전제**: ARC-002(scene-setup-tasks.md)의 폴더 구조가 완성된 상태.

#### L-1-01: Livestock 폴더 구조 생성

```
create_folder
  path: "Assets/_Project/Scripts/Livestock"

create_folder
  path: "Assets/_Project/Scripts/Livestock/Data"

create_folder
  path: "Assets/_Project/Data/Animals"

create_folder
  path: "Assets/_Project/Prefabs/Animals"
```

- **MCP 호출**: 4회

#### L-1-02: Phase 1 -- enum/struct 스크립트

```
create_script
  path: "Assets/_Project/Scripts/Livestock/AnimalType.cs"
  content: AnimalType enum (Poultry, Cattle, SmallAnimal)
  // → see docs/systems/livestock-architecture.md 섹션 2.1

create_script
  path: "Assets/_Project/Scripts/Livestock/CollectResult.cs"
  content: CollectResult enum (Success, NotReady, InventoryFull)
  // → see docs/systems/livestock-architecture.md 섹션 4.6

create_script
  path: "Assets/_Project/Scripts/Livestock/AnimalProductInfo.cs"
  content: AnimalProductInfo struct (productItemId, quality, estimatedAmount)
  // → see docs/systems/livestock-architecture.md 섹션 10
```

- **MCP 호출**: 3회
- **검증**: Unity 컴파일 대기 -> 에러 없음 확인

#### L-1-03: Phase 2 -- SO 및 데이터 클래스 스크립트

```
create_script
  path: "Assets/_Project/Scripts/Livestock/Data/AnimalData.cs"
  content: AnimalData ScriptableObject
    필드: animalId, animalName, animalType, purchasePrice, requiredFeedId,
          dailyFeedAmount, productItemId, productionIntervalDays, baseProductAmount,
          baseHappinessDecay, feedHappinessGain, petHappinessGain, unlockLevel,
          animalPrefab
  // → see docs/systems/livestock-architecture.md 섹션 2

create_script
  path: "Assets/_Project/Scripts/Livestock/Data/LivestockConfig.cs"
  content: LivestockConfig ScriptableObject
    필드: initialBarnCapacity, barnUpgradeCapacity[], barnUpgradeCost[],
          initialCoopCapacity, coopUpgradeCapacity[], coopUpgradeCost[],
          goldQualityThreshold, silverQualityThreshold,
          neglectThresholdDays, neglectPenaltyPerDay, initialHappiness,
          productionMultiplierCurve (AnimationCurve)
  // → see docs/systems/livestock-architecture.md 섹션 5.2

create_script
  path: "Assets/_Project/Scripts/Livestock/AnimalInstance.cs"
  content: AnimalInstance (Plain C# Serializable class)
    필드: instanceId, animalDataId, happiness, daysSinceLastFed,
          daysSinceLastPetted, isFedToday, isPettedToday,
          daysSinceLastProduct, isProductReady, productQuality,
          purchaseDay, displayName, [NonSerialized] data
  // → see docs/systems/livestock-architecture.md 섹션 3

create_script
  path: "Assets/_Project/Scripts/Livestock/Data/AnimalSaveData.cs"
  content: AnimalSaveData + AnimalInstanceSaveData (Serializable)
    AnimalSaveData: isUnlocked, barnLevel, coopLevel, animals[]
    AnimalInstanceSaveData: 모든 AnimalInstance 직렬화 필드 매핑
  // → see docs/systems/livestock-architecture.md 섹션 8.2
```

- **MCP 호출**: 4회
- **검증**: Unity 컴파일 대기 -> 에러 없음 확인

#### L-1-04: Phase 3 -- 유틸리티/이벤트 스크립트

```
create_script
  path: "Assets/_Project/Scripts/Livestock/HappinessCalculator.cs"
  content: HappinessCalculator (static class)
    메서드: CalculateDailyDelta(), GetProductionMultiplier(), GetProductQuality()
    모든 수치 임계값은 LivestockConfig SO에서 로드
  // → see docs/systems/livestock-architecture.md 섹션 5.1

create_script
  path: "Assets/_Project/Scripts/Livestock/LivestockEvents.cs"
  content: LivestockEvents (static class)
    이벤트: OnAnimalPurchased, OnAnimalFed, OnAnimalPetted, OnAnimalSad,
            OnProductReady, OnProductCollected, OnBarnUpgraded, OnCoopUpgraded
    Raise 메서드: 각 이벤트에 대응하는 RaiseXxx() 메서드
  // → see docs/systems/livestock-architecture.md 섹션 10
```

- **MCP 호출**: 2회
- **검증**: Unity 컴파일 대기 -> 에러 없음 확인

#### L-1-05: Phase 4 -- AnimalManager MonoBehaviour

```
create_script
  path: "Assets/_Project/Scripts/Livestock/AnimalManager.cs"
  content: AnimalManager (MonoBehaviour, Singleton, ISaveable)
    상태: _animals, _barnLevel, _barnCapacity, _coopLevel, _coopCapacity, _isUnlocked
    설정: _animalDataRegistry, _livestockConfig
    이벤트: OnAnimalAdded, OnAnimalFed, OnAnimalPetted, OnProductReady,
            OnProductCollected, OnBarnUpgraded, OnCoopUpgraded
    메서드: Initialize(), UnlockBarn(), UpgradeBarn(), UpgradeCoop(),
            HandleCoopBuilt(), TryBuyAnimal(), FeedAnimal(), PetAnimal(),
            CollectProduct(), DailyUpdate(), GetAnimalById(),
            GetSaveData(), LoadSaveData()
    구독: TimeManager.OnDayChanged(priority: 55),
          FarmZoneManager.OnZoneUnlocked
    ISaveable: SaveLoadOrder = 48
  // → see docs/systems/livestock-architecture.md 섹션 1, 4
```

- **MCP 호출**: 1회
- **검증**: Unity 컴파일 대기 -> 에러 없음 확인. 특히 asmdef 의존성(SeedMind.Core, SeedMind.Farm, SeedMind.Level) 확인

#### L-1-06: Phase 5 -- UI 스크립트

```
create_script
  path: "Assets/_Project/Scripts/UI/AnimalShopUI.cs"
  content: AnimalShopUI (MonoBehaviour)
    필드: _shopPanel, _animalSlotPrefab, _buyButton, _closeButton
    메서드: Open(), Close(), PopulateSlots(), OnBuyClicked()
    참조: AnimalManager.TryBuyAnimal()

create_script
  path: "Assets/_Project/Scripts/UI/AnimalCareUI.cs"
  content: AnimalCareUI (MonoBehaviour)
    필드: _carePanel, _animalNameText, _happinessBar, _productIcon,
          _feedButton, _petButton, _collectButton
    메서드: Show(AnimalInstance), Hide(), UpdateDisplay(),
            OnFeedClicked(), OnPetClicked(), OnCollectClicked()
    참조: AnimalManager.FeedAnimal(), PetAnimal(), CollectProduct()

create_script
  path: "Assets/_Project/Scripts/UI/AnimalSlotUI.cs"
  content: AnimalSlotUI (MonoBehaviour)
    필드: _animalIcon, _nameText, _priceText, _selectButton
    메서드: Setup(AnimalData), OnSelected()
```

- **MCP 호출**: 3회
- **검증**: Unity 컴파일 대기 -> 에러 없음 확인

#### L-1 asmdef 생성

```
create_script
  path: "Assets/_Project/Scripts/Livestock/SeedMind.Livestock.asmdef"
  content: asmdef JSON
    name: "SeedMind.Livestock"
    references: ["SeedMind.Core", "SeedMind.Farm", "SeedMind.Level"]
  // → see docs/systems/livestock-architecture.md 섹션 11.1
```

- **MCP 호출**: 1회 (L-1-02 이전에 실행해야 함)

**L-1 합계**: 4 + 3 + 4 + 2 + 1 + 3 + 1 = **18회** (예상 대비 +2: asmdef 포함)

---

### L-2: AnimalData SO 에셋 생성 (4종 + LivestockConfig 1종)

**목적**: 동물 4종(닭, 염소, 소, 양)의 AnimalData SO 에셋과 LivestockConfig SO 에셋을 생성하고 필드값을 설정한다.

**전제**: L-1 Phase 1~3 완료 (AnimalData.cs, LivestockConfig.cs 컴파일 완료)

**의존 태스크**: L-1

#### L-2-01: SO_Animal_Chicken (닭)

```
create_scriptable_object
  type: "SeedMind.Livestock.Data.AnimalData"
  asset_path: "Assets/_Project/Data/Animals/SO_Animal_Chicken.asset"

set_property  target: "SO_Animal_Chicken"
  animalId = "animal_chicken"
  animalName = "닭"
  animalType = 0                               // Poultry (→ see docs/systems/livestock-architecture.md 섹션 2.1)
  purchasePrice = 0                            // (→ see docs/content/livestock-system.md 섹션 1.1)
  requiredFeedId = "item_poultry_feed"         // (→ see docs/content/livestock-system.md 섹션 2.2)
  dailyFeedAmount = 0                          // (→ see docs/content/livestock-system.md 섹션 2.2)
  productItemId = "item_egg"                   // (→ see docs/content/livestock-system.md 섹션 4.1)
  productionIntervalDays = 0                   // (→ see docs/content/livestock-system.md 섹션 1.1)
  baseProductAmount = 0                        // (→ see docs/content/livestock-system.md 섹션 4.1)
  baseHappinessDecay = 0                       // (→ see docs/content/livestock-system.md 섹션 5.2)
  feedHappinessGain = 0                        // (→ see docs/content/livestock-system.md 섹션 5.2)
  petHappinessGain = 0                         // (→ see docs/content/livestock-system.md 섹션 5.2)
  unlockLevel = 0                              // (→ see docs/content/livestock-system.md 섹션 1.2)
```

> **주의**: 모든 수치(purchasePrice, dailyFeedAmount, productionIntervalDays 등)는 MCP 실행 시점에 canonical 문서(`docs/content/livestock-system.md`)에서 읽어 입력한다. 본 문서에서 수치를 직접 기재하지 않는다 (PATTERN-006).

- **MCP 호출**: 1(생성) + 12(필드 설정) = 13회

#### L-2-02: SO_Animal_Goat (염소)

```
create_scriptable_object
  type: "SeedMind.Livestock.Data.AnimalData"
  asset_path: "Assets/_Project/Data/Animals/SO_Animal_Goat.asset"

set_property  target: "SO_Animal_Goat"
  animalId = "animal_goat"
  animalName = "염소"
  animalType = 2                               // SmallAnimal (→ see docs/systems/livestock-architecture.md 섹션 2.1)
  purchasePrice = 0                            // (→ see docs/content/livestock-system.md 섹션 1.1)
  requiredFeedId = "item_hay"                  // (→ see docs/content/livestock-system.md 섹션 2.2)
  dailyFeedAmount = 0                          // (→ see docs/content/livestock-system.md 섹션 2.2)
  productItemId = "item_goat_milk"             // (→ see docs/content/livestock-system.md 섹션 4.1)
  productionIntervalDays = 0                   // (→ see docs/content/livestock-system.md 섹션 1.1)
  baseProductAmount = 0                        // (→ see docs/content/livestock-system.md 섹션 4.1)
  baseHappinessDecay = 0                       // (→ see docs/content/livestock-system.md 섹션 5.2)
  feedHappinessGain = 0                        // (→ see docs/content/livestock-system.md 섹션 5.2)
  petHappinessGain = 0                         // (→ see docs/content/livestock-system.md 섹션 5.2)
  unlockLevel = 0                              // (→ see docs/content/livestock-system.md 섹션 1.2)
```

- **MCP 호출**: 1(생성) + 12(필드 설정) = 13회

#### L-2-03: SO_Animal_Cow (소)

```
create_scriptable_object
  type: "SeedMind.Livestock.Data.AnimalData"
  asset_path: "Assets/_Project/Data/Animals/SO_Animal_Cow.asset"

set_property  target: "SO_Animal_Cow"
  animalId = "animal_cow"
  animalName = "소"
  animalType = 1                               // Cattle (→ see docs/systems/livestock-architecture.md 섹션 2.1)
  purchasePrice = 0                            // (→ see docs/content/livestock-system.md 섹션 1.1)
  requiredFeedId = "item_premium_hay"          // (→ see docs/content/livestock-system.md 섹션 2.2)
  dailyFeedAmount = 0                          // (→ see docs/content/livestock-system.md 섹션 2.2)
  productItemId = "item_milk"                  // (→ see docs/content/livestock-system.md 섹션 4.1)
  productionIntervalDays = 0                   // (→ see docs/content/livestock-system.md 섹션 1.1)
  baseProductAmount = 0                        // (→ see docs/content/livestock-system.md 섹션 4.1)
  baseHappinessDecay = 0                       // (→ see docs/content/livestock-system.md 섹션 5.2)
  feedHappinessGain = 0                        // (→ see docs/content/livestock-system.md 섹션 5.2)
  petHappinessGain = 0                         // (→ see docs/content/livestock-system.md 섹션 5.2)
  unlockLevel = 0                              // (→ see docs/content/livestock-system.md 섹션 1.2)
```

- **MCP 호출**: 1(생성) + 12(필드 설정) = 13회

#### L-2-04: SO_Animal_Sheep (양)

```
create_scriptable_object
  type: "SeedMind.Livestock.Data.AnimalData"
  asset_path: "Assets/_Project/Data/Animals/SO_Animal_Sheep.asset"

set_property  target: "SO_Animal_Sheep"
  animalId = "animal_sheep"
  animalName = "양"
  animalType = 2                               // SmallAnimal (→ see docs/systems/livestock-architecture.md 섹션 2.1)
  purchasePrice = 0                            // (→ see docs/content/livestock-system.md 섹션 1.1)
  requiredFeedId = "item_pasture_grass"        // (→ see docs/content/livestock-system.md 섹션 2.2)
  dailyFeedAmount = 0                          // (→ see docs/content/livestock-system.md 섹션 2.2)
  productItemId = "item_wool"                  // (→ see docs/content/livestock-system.md 섹션 4.1)
  productionIntervalDays = 0                   // (→ see docs/content/livestock-system.md 섹션 1.1)
  baseProductAmount = 0                        // (→ see docs/content/livestock-system.md 섹션 4.1)
  baseHappinessDecay = 0                       // (→ see docs/content/livestock-system.md 섹션 5.2)
  feedHappinessGain = 0                        // (→ see docs/content/livestock-system.md 섹션 5.2)
  petHappinessGain = 0                         // (→ see docs/content/livestock-system.md 섹션 5.2)
  unlockLevel = 0                              // (→ see docs/content/livestock-system.md 섹션 1.2)
```

- **MCP 호출**: 1(생성) + 12(필드 설정) = 13회

#### L-2-05: SO_LivestockConfig

```
create_scriptable_object
  type: "SeedMind.Livestock.Data.LivestockConfig"
  asset_path: "Assets/_Project/Data/Animals/SO_LivestockConfig.asset"

set_property  target: "SO_LivestockConfig"
  initialBarnCapacity = 0                      // (→ see docs/content/livestock-system.md 섹션 3.2)
  barnUpgradeCapacity = []                     // (→ see docs/content/livestock-system.md 섹션 3.2)
  barnUpgradeCost = []                         // (→ see docs/content/livestock-system.md 섹션 3.2)
  initialCoopCapacity = 0                      // (→ see docs/content/livestock-system.md 섹션 3.1)
  coopUpgradeCapacity = []                     // (→ see docs/content/livestock-system.md 섹션 3.1)
  coopUpgradeCost = []                         // (→ see docs/content/livestock-system.md 섹션 3.1)
  goldQualityThreshold = 0                     // (→ see docs/content/livestock-system.md 섹션 5.3)
  silverQualityThreshold = 0                   // (→ see docs/content/livestock-system.md 섹션 5.3)
  neglectThresholdDays = 0                     // (→ see docs/content/livestock-system.md 섹션 5.4)
  neglectPenaltyPerDay = 0                     // (→ see docs/content/livestock-system.md 섹션 5.4)
  initialHappiness = 0                         // (→ see docs/content/livestock-system.md 섹션 5.1)
  productionMultiplierCurve = AnimationCurve   // (→ see docs/content/livestock-system.md 섹션 5.3)
```

> **주의**: `productionMultiplierCurve`(AnimationCurve)의 MCP 설정 가능 여부가 불확실하다. 키프레임 직접 설정이 불가능할 경우, AnimalManager.Initialize()에서 코드로 기본 커브를 생성하는 fallback을 구현한다.

- **MCP 호출**: 1(생성) + 12(필드 설정) = 13회

[RISK] 배열 필드(barnUpgradeCapacity[], barnUpgradeCost[], coopUpgradeCapacity[], coopUpgradeCost[])의 MCP `set_property` 지원 여부 검증 필요. 미지원 시 Editor 스크립트 우회.

**L-2 합계**: 13 x 4 + 13 = **65회** (예상 대비 +13: 필드 수가 많아 상향 조정)

---

### L-3: FeedData 사료 아이템 등록

**목적**: 사료 4종(모이, 건초, 프리미엄 건초, 목초)을 기존 ItemData SO 체계에 등록한다.

**전제**: L-1 완료, 인벤토리 시스템(ARC-013)의 ItemData SO 구조가 존재하는 상태.

**의존 태스크**: ARC-013 (inventory-tasks.md)

#### L-3-01 ~ L-3-04: 사료 아이템 SO 4종

각 사료 아이템에 대해:

```
create_scriptable_object
  type: "[기존 ItemData SO 타입]"
  asset_path: "Assets/_Project/Data/Items/SO_Item_PoultryFeed.asset"

set_property  target: "SO_Item_PoultryFeed"
  itemId = "item_poultry_feed"                 // (→ see docs/content/livestock-system.md 섹션 2.2)
  itemName = "모이"
  itemType = [Feed 카테고리]
  basePrice = 0                                // (→ see docs/content/livestock-system.md 섹션 2.2)
  stackable = true
  maxStack = 99
```

동일 패턴으로 4종 생성:
- `SO_Item_PoultryFeed.asset` (모이, `item_poultry_feed`)
- `SO_Item_Hay.asset` (건초, `item_hay`)
- `SO_Item_PremiumHay.asset` (프리미엄 건초, `item_premium_hay`)
- `SO_Item_PastureGrass.asset` (목초, `item_pasture_grass`)

사료 ID, 이름, 가격은 모두 (-> see `docs/content/livestock-system.md` 섹션 2.2).

- **MCP 호출**: 4(생성) + 4 x 6(필드 설정) = 28회

[OPEN] 사료 아이템이 기존 ItemData SO를 재사용할지, 별도 FeedData SO가 필요한지는 인벤토리 시스템 설계에 따라 결정. 현재는 기존 ItemData SO 재사용 가정.

**L-3 합계**: **28회** (예상 대비 +16: 아이템별 필드 설정 포함)

---

### L-4: 씬 배치 -- AnimalManager 배치 및 SO 참조 연결

**목적**: SCN_Farm 씬에 AnimalManager GameObject를 생성하고, SO 에셋 참조를 연결한다.

**전제**: L-1 Phase 4 완료 (AnimalManager.cs 컴파일), L-2 완료 (SO 에셋 존재)

**의존 태스크**: L-1, L-2

#### L-4-01: AnimalManager GameObject 생성

```
create_object
  name: "AnimalManager"
  scene: "SCN_Farm"

set_parent
  child: "AnimalManager"
  parent: "--- MANAGERS ---"

add_component
  target: "AnimalManager"
  component: "SeedMind.Livestock.AnimalManager"
```

- **MCP 호출**: 3회

#### L-4-02: SO 참조 연결

```
set_property  target: "AnimalManager" (AnimalManager 컴포넌트)
  _livestockConfig = "Assets/_Project/Data/Animals/SO_LivestockConfig.asset"

set_property  target: "AnimalManager" (AnimalManager 컴포넌트)
  _animalDataRegistry = [
    "Assets/_Project/Data/Animals/SO_Animal_Chicken.asset",
    "Assets/_Project/Data/Animals/SO_Animal_Goat.asset",
    "Assets/_Project/Data/Animals/SO_Animal_Cow.asset",
    "Assets/_Project/Data/Animals/SO_Animal_Sheep.asset"
  ]
```

> **주의**: `_animalDataRegistry`는 AnimalData[] 배열이다. MCP의 `set_property`로 SO 배열 참조를 설정할 수 있는지 검증 필요. 미지원 시 DataRegistry 패턴을 사용하여 코드에서 동적 로드.

- **MCP 호출**: 2회

[RISK] MCP에서 SerializedProperty를 통한 SO 배열 참조 설정 가능 여부 불확실. (-> see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련). 대안: DataRegistry.GetAllAnimalData()로 런타임 로드.

#### L-4-03: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

**L-4 합계**: 3 + 2 + 1 = **6회** (예상 대비 -4: 최소화)

---

### L-5: 기존 시스템 확장

**목적**: 목축 시스템 연동을 위해 기존 스크립트(XPSource, HarvestOrigin, GameSaveData)를 수정한다.

**전제**: L-1 완료. 기존 시스템 스크립트가 컴파일 완료 상태.

**의존 태스크**: L-1

#### L-5-01: XPSource enum 확장

```
modify_script
  path: "Assets/_Project/Scripts/Level/XPSource.cs"
  action: 기존 enum에 AnimalCare, AnimalHarvest 값 추가
  // → see docs/systems/livestock-architecture.md 섹션 7.1
```

- **MCP 호출**: 1회

#### L-5-02: ProgressionManager.GetExpForSource() switch 문 확장

```
modify_script
  path: "Assets/_Project/Scripts/Level/ProgressionManager.cs"
  action: GetExpForSource() switch 문에 AnimalCare, AnimalHarvest case 추가
  // → see docs/systems/livestock-architecture.md 섹션 7.2
```

- **MCP 호출**: 1회

#### L-5-03: HarvestOrigin enum 확장

```
modify_script
  path: "Assets/_Project/Scripts/SeedMind/HarvestOrigin.cs"
  action: 기존 enum에 Barn = 2 값 추가
  // → see docs/systems/livestock-architecture.md 섹션 9
```

- **MCP 호출**: 1회

#### L-5-04: GameSaveData 확장

```
modify_script
  path: "Assets/_Project/Scripts/Core/SaveLoad/GameSaveData.cs"
  action: AnimalSaveData animals 필드 추가
  // → see docs/systems/livestock-architecture.md 섹션 8.3
```

- **MCP 호출**: 1회

#### L-5-05: SaveManager ISaveable 등록

```
modify_script
  path: "Assets/_Project/Scripts/Core/SaveLoad/SaveManager.cs"
  action: AnimalManager의 ISaveable 등록 확인 (SaveLoadOrder = 48)
  // → see docs/systems/livestock-architecture.md 섹션 8.1
```

- **MCP 호출**: 1회

#### L-5-06: 컴파일 확인

```
execute_menu_item
  menu: "Assets/Refresh"
  // Unity 컴파일 대기 후 에러 없음 확인
```

- **MCP 호출**: 1회

**L-5 합계**: **6회** (예상 대비 -2)

---

### L-6: Zone E 연동 -- 외양간/닭장 건설 핸들러

**목적**: Zone E 해금 시 AnimalManager를 활성화하고, 외양간/닭장 건설 이벤트를 처리하는 연동을 설정한다.

**전제**: L-4 완료. FarmZoneManager 및 BuildingManager가 씬에 존재.

**의존 태스크**: L-4, ARC-023 (farm-expansion-architecture), ARC-007 (facilities-tasks)

#### L-6-01: FarmZoneManager.OnZoneUnlocked 이벤트 구독 확인

AnimalManager.OnEnable()에서 `FarmZoneManager.OnZoneUnlocked += HandleZoneUnlocked` 구독이 정상 작동하는지 확인한다.

```
enter_play_mode

execute_method
  target: "FarmZoneManager"
  method: "TryUnlockZone"
  args: ["zone_south_meadow"]

get_console_logs
  filter: "AnimalManager"
  // "AnimalManager: Zone E unlocked, livestock system activated" 로그 확인

exit_play_mode
```

- **MCP 호출**: 4회

#### L-6-02: BuildingManager.OnBuildingConstructed 이벤트 구독 확인

AnimalManager가 외양간/닭장 건설 이벤트를 수신하는지 확인한다.

```
enter_play_mode

execute_method
  target: "BuildingManager"
  method: "SimulateBuildComplete"
  args: ["building_barn"]

get_console_logs
  filter: "AnimalManager"
  // "AnimalManager: Barn built, barnLevel=1, capacity=X" 로그 확인

exit_play_mode
```

> **주의**: SimulateBuildComplete는 테스트용 메서드이다. 실제 BuildingManager에 해당 메서드가 없을 경우, BuildingEvents.RaiseBuildingConstructed()를 직접 호출한다.

- **MCP 호출**: 4회

**L-6 합계**: **8회** (예상 대비 +2: 테스트 포함)

---

### L-7: UI 생성 -- 동물 구매 팝업 및 돌봄 패널

**목적**: 동물 구매 UI(AnimalShopUI)와 돌봄 UI(AnimalCareUI)의 GameObject 계층 구조를 생성한다.

**전제**: L-1 Phase 5 완료 (UI 스크립트 컴파일), Canvas_Overlay 존재.

**의존 태스크**: L-1, ARC-002

#### L-7-01: 동물 구매 팝업 (AnimalShopUI)

```
create_object
  name: "Panel_AnimalShop"
  scene: "SCN_Farm"

set_parent
  child: "Panel_AnimalShop"
  parent: "Canvas_Overlay"

add_component
  target: "Panel_AnimalShop"
  component: "UnityEngine.UI.Image"
  // 배경 패널

add_component
  target: "Panel_AnimalShop"
  component: "SeedMind.UI.AnimalShopUI"

// --- 내부 요소 ---

create_object  name: "Title_Text"
set_parent     child: "Title_Text"  parent: "Panel_AnimalShop"
add_component  target: "Title_Text"  component: "TMPro.TextMeshProUGUI"
set_property   target: "Title_Text"  text: "동물 구매"

create_object  name: "AnimalSlotContainer"
set_parent     child: "AnimalSlotContainer"  parent: "Panel_AnimalShop"
add_component  target: "AnimalSlotContainer"  component: "UnityEngine.UI.VerticalLayoutGroup"

create_object  name: "Btn_Buy"
set_parent     child: "Btn_Buy"  parent: "Panel_AnimalShop"
add_component  target: "Btn_Buy"  component: "UnityEngine.UI.Button"
add_component  target: "Btn_Buy"  component: "TMPro.TextMeshProUGUI"
set_property   target: "Btn_Buy"  text: "구매"

create_object  name: "Btn_Close"
set_parent     child: "Btn_Close"  parent: "Panel_AnimalShop"
add_component  target: "Btn_Close"  component: "UnityEngine.UI.Button"

// AnimalShopUI 참조 연결
set_property  target: "Panel_AnimalShop" (AnimalShopUI 컴포넌트)
  _shopPanel = Panel_AnimalShop
  _buyButton = Btn_Buy
  _closeButton = Btn_Close

// 비활성화 (기본 상태)
set_property  target: "Panel_AnimalShop"  active: false
```

- **MCP 호출**: ~18회

#### L-7-02: AnimalSlotUI 프리팹

```
create_object  name: "PFB_AnimalSlot"

create_object  name: "Icon"
set_parent     child: "Icon"  parent: "PFB_AnimalSlot"
add_component  target: "Icon"  component: "UnityEngine.UI.Image"

create_object  name: "NameText"
set_parent     child: "NameText"  parent: "PFB_AnimalSlot"
add_component  target: "NameText"  component: "TMPro.TextMeshProUGUI"

create_object  name: "PriceText"
set_parent     child: "PriceText"  parent: "PFB_AnimalSlot"
add_component  target: "PriceText"  component: "TMPro.TextMeshProUGUI"

add_component  target: "PFB_AnimalSlot"  component: "UnityEngine.UI.Button"
add_component  target: "PFB_AnimalSlot"  component: "SeedMind.UI.AnimalSlotUI"

set_property  target: "PFB_AnimalSlot" (AnimalSlotUI 컴포넌트)
  _animalIcon = Icon (Image)
  _nameText = NameText (TMP)
  _priceText = PriceText (TMP)
  _selectButton = PFB_AnimalSlot (Button)

save_as_prefab
  source: "PFB_AnimalSlot"
  path: "Assets/_Project/Prefabs/UI/PFB_AnimalSlot.prefab"
```

- **MCP 호출**: ~12회

#### L-7-03: 돌봄 패널 (AnimalCareUI)

```
create_object  name: "Panel_AnimalCare"
set_parent     child: "Panel_AnimalCare"  parent: "Canvas_HUD"

add_component  target: "Panel_AnimalCare"  component: "UnityEngine.UI.Image"
add_component  target: "Panel_AnimalCare"  component: "SeedMind.UI.AnimalCareUI"

// --- 내부 요소 ---

create_object  name: "AnimalNameText"
set_parent     child: "AnimalNameText"  parent: "Panel_AnimalCare"
add_component  target: "AnimalNameText"  component: "TMPro.TextMeshProUGUI"

create_object  name: "HappinessBar"
set_parent     child: "HappinessBar"  parent: "Panel_AnimalCare"
add_component  target: "HappinessBar"  component: "UnityEngine.UI.Slider"

create_object  name: "ProductIcon"
set_parent     child: "ProductIcon"  parent: "Panel_AnimalCare"
add_component  target: "ProductIcon"  component: "UnityEngine.UI.Image"

create_object  name: "Btn_Feed"
set_parent     child: "Btn_Feed"  parent: "Panel_AnimalCare"
add_component  target: "Btn_Feed"  component: "UnityEngine.UI.Button"
add_component  target: "Btn_Feed"  component: "TMPro.TextMeshProUGUI"
set_property   target: "Btn_Feed"  text: "먹이 주기"

create_object  name: "Btn_Pet"
set_parent     child: "Btn_Pet"  parent: "Panel_AnimalCare"
add_component  target: "Btn_Pet"  component: "UnityEngine.UI.Button"
add_component  target: "Btn_Pet"  component: "TMPro.TextMeshProUGUI"
set_property   target: "Btn_Pet"  text: "쓰다듬기"

create_object  name: "Btn_Collect"
set_parent     child: "Btn_Collect"  parent: "Panel_AnimalCare"
add_component  target: "Btn_Collect"  component: "UnityEngine.UI.Button"
add_component  target: "Btn_Collect"  component: "TMPro.TextMeshProUGUI"
set_property   target: "Btn_Collect"  text: "수집"

// AnimalCareUI 참조 연결
set_property  target: "Panel_AnimalCare" (AnimalCareUI 컴포넌트)
  _carePanel = Panel_AnimalCare
  _animalNameText = AnimalNameText (TMP)
  _happinessBar = HappinessBar (Slider)
  _productIcon = ProductIcon (Image)
  _feedButton = Btn_Feed (Button)
  _petButton = Btn_Pet (Button)
  _collectButton = Btn_Collect (Button)

// 비활성화 (기본 상태)
set_property  target: "Panel_AnimalCare"  active: false
```

- **MCP 호출**: ~22회

#### L-7-04: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

**L-7 합계**: 18 + 12 + 22 + 1 = **~53회** (예상 대비 +15: UI 구조가 깊어 상향)

[RISK] UI 프리팹 생성 호출 수가 크다. Editor 스크립트(CreateLivestockUI.cs)로 일괄 생성하면 ~8회로 감소 가능.

---

### L-8: 이벤트 연동

**목적**: LivestockEvents 정적 이벤트와 외부 시스템(ProgressionManager, UI 등) 간의 구독 관계를 설정하고 검증한다.

**전제**: L-1, L-4, L-5, L-7 완료.

**의존 태스크**: L-5 (XPSource 확장), L-7 (UI 생성)

#### L-8-01: ProgressionManager 이벤트 구독 확인

ProgressionManager.OnEnable()에 다음 구독이 추가되었는지 확인:

```
modify_script
  path: "Assets/_Project/Scripts/Level/ProgressionManager.cs"
  action: OnEnable()에 LivestockEvents 구독 추가
    LivestockEvents.OnAnimalPetted += HandleAnimalCare
    LivestockEvents.OnProductCollected += HandleAnimalHarvest
  // → see docs/systems/livestock-architecture.md 섹션 7.3
```

- **MCP 호출**: 1회

#### L-8-02: UI 이벤트 바인딩

AnimalCareUI가 AnimalManager의 인스턴스 이벤트를 구독하는지 확인:

```
modify_script
  path: "Assets/_Project/Scripts/UI/AnimalCareUI.cs"
  action: OnEnable()에 AnimalManager 이벤트 구독 추가
    AnimalManager.Instance.OnAnimalFed += RefreshDisplay
    AnimalManager.Instance.OnAnimalPetted += RefreshDisplay
    AnimalManager.Instance.OnProductCollected += RefreshDisplay
```

- **MCP 호출**: 1회

#### L-8-03: AnimalShopUI NPC 연동

목축상(Rancher) NPC 상호작용 시 AnimalShopUI.Open()을 호출하는 연결:

```
modify_script
  path: "Assets/_Project/Scripts/NPC/NPCInteraction.cs"
  action: NPC 타입이 Rancher일 때 AnimalShopUI.Open() 호출 분기 추가
```

- **MCP 호출**: 1회

#### L-8-04: 컴파일 확인

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

**L-8 합계**: **4회** (예상 대비 -2)

---

### L-9: 통합 테스트 시퀀스

**목적**: 전체 목축/낙농 시스템의 핵심 흐름을 Play Mode에서 검증한다.

**전제**: L-1 ~ L-8 모두 완료.

#### L-9-01: AnimalManager 초기화 검증

```
enter_play_mode

get_console_logs
  filter: "AnimalManager"
  // "AnimalManager: Initialized with X animal types, config loaded" 로그 확인

exit_play_mode
```

- **MCP 호출**: 3회
- **검증**: AnimalManager 싱글턴이 정상 초기화되고, _animalDataRegistry와 _livestockConfig가 null이 아닌지 확인

#### L-9-02: 동물 구매 흐름

```
enter_play_mode

// Zone E 해금 시뮬레이션
execute_method
  target: "AnimalManager"
  method: "UnlockBarn"

// 닭 구매
execute_method
  target: "AnimalManager"
  method: "TryBuyAnimal"
  args: ["animal_chicken"]

get_console_logs
  filter: "AnimalManager"
  // "AnimalManager: Animal purchased - chicken, instanceId=xxx, happiness=Y" 로그 확인
  // Y = 초기 행복도 (→ see docs/content/livestock-system.md 섹션 5.1)

exit_play_mode
```

- **MCP 호출**: 5회
- **검증**: 동물 목록에 닭 1마리 추가, 골드 차감 확인, OnAnimalAdded 이벤트 발행 확인

#### L-9-03: 일일 돌봄 사이클

```
enter_play_mode

// 동물 구매 (테스트 셋업)
execute_method  target: "AnimalManager"  method: "UnlockBarn"
execute_method  target: "AnimalManager"  method: "TryBuyAnimal"  args: ["animal_chicken"]

// 먹이 주기
execute_method
  target: "AnimalManager"
  method: "FeedAnimal"
  args: [인스턴스 참조]

get_console_logs
  filter: "AnimalFed"
  // "AnimalFed: chicken, happiness delta=+Z" 로그 확인

// 쓰다듬기
execute_method
  target: "AnimalManager"
  method: "PetAnimal"
  args: [인스턴스 참조]

get_console_logs
  filter: "AnimalPetted"

// DailyUpdate 수동 트리거
execute_method
  target: "AnimalManager"
  method: "DailyUpdate"

get_console_logs
  filter: "DailyUpdate"
  // "DailyUpdate: chicken happiness=N, productReady=bool" 로그 확인

exit_play_mode
```

- **MCP 호출**: 10회
- **검증**: 행복도 증가, isFedToday/isPettedToday 플래그, 생산 주기 카운트 진행 확인

#### L-9-04: 생산물 수집 흐름

```
enter_play_mode

// 셋업: 동물 구매 + 생산물 준비 상태 강제 설정
execute_method  target: "AnimalManager"  method: "UnlockBarn"
execute_method  target: "AnimalManager"  method: "TryBuyAnimal"  args: ["animal_chicken"]
// 생산 준비 상태 강제 설정 (테스트용)
execute_method  target: "AnimalManager"  method: "ForceProductReady"  args: [인스턴스 참조]

// 수집
execute_method
  target: "AnimalManager"
  method: "CollectProduct"
  args: [인스턴스 참조]

get_console_logs
  filter: "ProductCollected"
  // "ProductCollected: item_egg x1, quality=Normal" 로그 확인

exit_play_mode
```

- **MCP 호출**: 7회
- **검증**: 인벤토리에 달걀 추가, XP 부여(AnimalHarvest), OnProductCollected 이벤트 발행 확인

[OPEN] ForceProductReady() 테스트용 메서드를 AnimalManager에 포함할지, 별도 테스트 유틸리티로 분리할지 결정 필요. `#if UNITY_EDITOR` 또는 `[Conditional("UNITY_EDITOR")]`로 빌드에서 제외하는 것을 권장.

#### L-9-05: 세이브/로드 무결성

```
enter_play_mode

// 셋업: 동물 2마리 구매
execute_method  target: "AnimalManager"  method: "UnlockBarn"
execute_method  target: "AnimalManager"  method: "TryBuyAnimal"  args: ["animal_chicken"]
execute_method  target: "AnimalManager"  method: "TryBuyAnimal"  args: ["animal_cow"]

// 저장
execute_method  target: "SaveManager"  method: "Save"

// 상태 클리어 (시뮬레이션)
execute_method  target: "AnimalManager"  method: "ClearAllAnimals"

// 로드
execute_method  target: "SaveManager"  method: "Load"

get_console_logs
  filter: "AnimalManager"
  // "AnimalManager: Loaded 2 animals, barnLevel=1, coopLevel=0/1" 로그 확인

exit_play_mode
```

- **MCP 호출**: 8회
- **검증**: 로드 후 동물 수 2마리, 각 동물의 instanceId/happiness/daysSinceLastFed가 저장값과 일치

**L-9 합계**: 3 + 5 + 10 + 7 + 8 = **33회** (예상 대비 +17: 테스트 케이스 세분화)

---

## 5. 태스크 의존 관계 다이어그램

```
L-1 (스크립트 생성)
  │
  ├── L-2 (AnimalData SO 에셋) ──┐
  │                               │
  ├── L-3 (FeedData 사료 등록) ──┤
  │                               │
  └── L-5 (기존 시스템 확장) ────┤
                                  │
                    L-4 (씬 배치) ←┘
                      │
                      ├── L-6 (Zone E 연동)
                      │
                      ├── L-7 (UI 생성)
                      │     │
                      │     └── L-8 (이벤트 연동)
                      │
                      └── L-9 (통합 테스트) ← L-6, L-7, L-8 모두 완료 후
```

**병렬 실행 가능 그룹**:
- L-2, L-3, L-5는 L-1 완료 후 병렬 실행 가능
- L-6, L-7은 L-4 완료 후 병렬 실행 가능
- L-9는 모든 태스크 완료 후에만 실행

---

## Cross-references

| 문서 | 관련 내용 |
|------|----------|
| `docs/systems/livestock-architecture.md` (ARC-019) | 전체 아키텍처 설계, 클래스 다이어그램, 코드 예시 |
| `docs/content/livestock-system.md` (CON-006) | 동물 종류, 가격, 사료, 생산물, 행복도 수치 canonical |
| `docs/systems/farm-expansion.md` (DES-012) | Zone E(목장) 구조, 해금 비용, 타일 배치 |
| `docs/systems/farm-expansion-architecture.md` (ARC-023) | FarmZoneManager, ZoneType.Pasture |
| `docs/systems/progression-architecture.md` | XPSource enum, GetExpForSource() 확장 |
| `docs/systems/economy-architecture.md` | EconomyManager, HarvestOrigin.Barn 확장 |
| `docs/systems/save-load-architecture.md` (ARC-011) | GameSaveData 확장, ISaveable, SaveLoadOrder 48 |
| `docs/mcp/facilities-tasks.md` (ARC-007) | BuildingManager 의존, 문서 형식 참조 |
| `docs/mcp/inventory-tasks.md` (ARC-013) | InventoryManager 의존, ItemData SO 체계 참조 |
| `docs/pipeline/data-pipeline.md` | DataRegistry, SO 에셋 로드 전략 |
| `docs/systems/project-structure.md` | 네임스페이스, 폴더 구조, asmdef 규칙 |

---

## Open Questions

1. [OPEN] **AnimationCurve MCP 설정**: LivestockConfig의 `productionMultiplierCurve`를 MCP `set_property`로 키프레임 단위 설정할 수 있는지 불확실. MCP for Unity의 AnimationCurve 지원 범위 사전 검증 필요. (-> see `docs/systems/livestock-architecture.md` Risks 항목 3)

2. [OPEN] **SO 배열 참조 설정**: MCP `set_property`로 ScriptableObject 배열(AnimalData[])을 직접 할당할 수 있는지 불확실. 미지원 시 DataRegistry 패턴으로 런타임 동적 로드 전환 필요.

3. [OPEN] **사료 아이템 SO 타입**: 기존 ItemData SO를 사료에도 재사용할지, 별도 FeedData SO를 정의할지는 인벤토리 시스템 설계 확정 후 결정. (-> see `docs/mcp/inventory-tasks.md` 참조)

4. [OPEN] **ForceProductReady() 테스트 메서드**: 통합 테스트(L-9-04)에서 사용하는 생산 강제 준비 메서드를 AnimalManager에 `#if UNITY_EDITOR` 블록으로 포함할지, 별도 테스트 유틸리티 클래스로 분리할지 결정 필요.

5. [OPEN] **동물 프리팹 비주얼**: 이 태스크 문서에서는 동물 프리팹의 외형(메시, 머티리얼, 애니메이션)을 다루지 않는다. 동물 프리팹은 별도 아트 파이프라인 태스크에서 처리하며, 현재는 placeholder cube/sphere를 사용한다.

6. [OPEN] **닭장/외양간 BuildingData SO**: 닭장과 외양간의 BuildingData SO가 기존 facilities-tasks.md(ARC-007)에서 이미 생성되었는지, 아니면 본 태스크에서 추가 생성해야 하는지 확인 필요. ARC-007에서 7종 시설(물탱크, 창고, 온실, 가공소, 제분소, 발효실, 베이커리)만 다루었으므로, 외양간/닭장은 본 태스크 또는 별도 태스크에서 추가해야 할 가능성이 높다.

---

## Risks

1. [RISK] **총 MCP 호출 수**: L-1~L-9 합산 약 223회 (실행 중 변동 가능). Editor 스크립트 일괄 생성(L-2, L-7)으로 약 100회 절감 가능. 순수 MCP 실행 시 시간 비용이 크므로, SO 에셋 생성(L-2)과 UI 생성(L-7)은 Editor 스크립트 우회를 강력 권장.

2. [RISK] **asmdef 의존성 누락**: SeedMind.Livestock asmdef가 SeedMind.Core, SeedMind.Farm, SeedMind.Level을 참조해야 한다. 누락 시 AnimalManager.cs 컴파일 실패로 이후 모든 태스크가 블록된다. L-1 asmdef 생성 단계에서 반드시 검증.

3. [RISK] **AnimationCurve SO 필드**: LivestockConfig의 productionMultiplierCurve를 MCP로 설정할 수 없을 경우, 코드 내 fallback 커브 초기화 필요. AnimalManager.Initialize()에서 `if (config.productionMultiplierCurve == null || config.productionMultiplierCurve.length == 0)` 체크 후 기본 커브 생성.

4. [RISK] **기존 스크립트 수정 충돌**: L-5에서 XPSource, HarvestOrigin, GameSaveData를 수정할 때, 다른 태스크(ARC-007, ARC-013 등)와의 수정 충돌 가능. enum 값 번호가 겹치지 않도록 주의.

5. [RISK] **CON-006 수치 미확정**: 모든 SO 필드값이 `docs/content/livestock-system.md`(CON-006)를 참조한다. CON-006의 수치가 변경되면 SO 에셋을 재설정해야 한다. 변경 빈도가 높은 초기에는 Editor 스크립트를 통한 일괄 재설정 메커니즘을 마련하는 것이 좋다.

6. [RISK] **외양간/닭장 BuildingData 부재**: ARC-007(facilities-tasks.md)에서 외양간/닭장의 BuildingData SO가 생성되지 않았다. L-6 Zone E 연동 전에 BuildingData SO 2종(building_barn, building_chicken_coop)의 추가 생성이 필요하다. 이 누락은 별도 보충 태스크로 처리해야 한다.

---

*이 문서는 Claude Code가 기존 MCP 태스크 문서의 패턴과 규칙을 준수하여 자율적으로 작성했습니다.*