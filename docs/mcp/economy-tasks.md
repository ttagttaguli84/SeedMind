# 경제 시스템 MCP 태스크 시퀀스 (ARC-048)

> 작성: Claude Code (Sonnet 4.6) | 2026-04-09 | 문서 ID: ARC-048 | Phase 1

---

## Context

이 문서는 경제 시스템의 Unity MCP 구현 태스크 시퀀스를 정의한다. `docs/systems/economy-architecture.md`(ARC-004) 섹션 7 "MCP 구현 계획"(Phase A~D)을 독립 문서로 분리하여, 스크립트 생성 → SO 에셋 생성 → 씬 배치 → 가격 변동 연동 → 상점 UI 연동까지의 전체 MCP 호출 시퀀스를 상세히 기술한다.

**상위 설계 문서**: `docs/systems/economy-architecture.md` (ARC-004)  
**경제 수치 canonical 출처**: `docs/systems/economy-system.md` (DES 계열)  
**작물 가격 canonical 출처**: `docs/design.md` 섹션 4.2  
**패턴 참조**: `docs/mcp/energy-tasks.md` (ARC-047), `docs/mcp/gathering-tasks.md` (ARC-032)

---

## 1. 개요

### 목적

이 문서는 `docs/systems/economy-architecture.md`(ARC-004) 섹션 7에서 요약된 MCP 구현 계획(Phase A~D)을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 경제 시스템의 데이터 레이어(SO 에셋), 시스템 레이어(스크립트), 씬 배치, 가격 변동 연동, 상점 UI 통합 검증을 완성한다.

### 의존성

```
경제 시스템 MCP 태스크 의존 관계:
├── SeedMind.Core        (ISaveable, Singleton 기반 클래스)
├── SeedMind.Save        (SaveManager, ISaveable 인터페이스)
├── SeedMind.Farm        (FarmingSystem — OnCropHarvested 이벤트 연동)
├── SeedMind.Player      (InventoryManager — 판매 대상 아이템 제공)
├── SeedMind.Time        (TimeManager — OnDayChanged 이벤트 구독)
├── SeedMind.Weather     (WeatherSystem — OnWeatherChanged 이벤트 구독)
└── SeedMind.UI          (Canvas_Overlay — ShopPanel, HUD GoldDisplay 배치)
```

(→ see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 완료된 태스크 의존성

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조, SCN_Farm 기본 계층 (Managers, Environment, UI) |
| ARC-003 | `docs/mcp/farming-tasks.md` | Phase A~C 전체 | FarmingSystem MonoBehaviour 컴파일 완료, FarmEvents.OnCropHarvested |
| ARC-008 | `docs/mcp/save-load-tasks.md` | Phase A 이상 | SaveManager, ISaveable, GameSaveData |
| ARC-013 | `docs/mcp/inventory-tasks.md` | Phase A 이상 | InventoryManager, 아이템 데이터 구조 |
| ARC-018 | `docs/mcp/ui-tasks.md` | HUD Phase 이상 | Canvas_Overlay, HUDController, ShopPanel 슬롯 |
| ARC-019 | `docs/mcp/time-season-tasks.md` | Phase A 이상 | TimeManager, OnDayChanged 이벤트, WeatherSystem |

### 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 | 비고 |
|--------------|------|------|
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B | `GameManager` GO가 이미 배치됨 |
| `--- ECONOMY ---` (씬 계층 부모) | ARC-002 Phase B 또는 ARC-004 | EconomyManager, Shop GO의 상위 |
| `Canvas_Overlay` (UI 루트) | ARC-002 Phase B | ShopPanel을 그 하위에 배치 |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A | |
| `Assets/_Project/Scripts/` 폴더 구조 | ARC-002 Phase A | |
| `SaveManager` (씬 오브젝트) | ARC-008 | ISaveable.Register() 대상 |
| `ISaveable` 인터페이스 | ARC-008 | EconomyManager가 구현 |
| `TimeManager` (씬 오브젝트) | ARC-019 | EC-A-4 단계에서 이벤트 연결만 추가 |
| `WeatherSystem` (씬 오브젝트) | ARC-019 | EC-C-2 단계에서 이벤트 연결만 추가 |
| `FarmingSystem` (씬 오브젝트) | ARC-003 | FarmEvents.OnCropHarvested 구독 참조 |
| `InventoryManager` (씬 오브젝트) | ARC-013 | TrySellCrop 호출 시 인벤토리 데이터 참조 |
| — Phase A 생성 오브젝트 (B~D에서 재사용) — | | |
| `EconomyManager` (씬 GO + 컴포넌트) | EC-A (이 문서 Phase A) | EC-B, EC-C, EC-D에서 참조 연결 대상 |
| `Shop` (씬 GO + 컴포넌트) | EC-A (이 문서 Phase A) | EC-B-3, EC-D에서 ShopData 연결 대상 |
| `SO_EconomyConfig.asset` | EC-A (이 문서 Phase A) | EC-B 이후 모든 단계의 설정 기반 |

### 총 MCP 호출 예상 수

| 태스크 그룹 | 설명 | 예상 호출 수 | 복잡도 |
|-------------|------|:------------:|:------:|
| EC-A | 스크립트 생성 + SO 에셋 생성 + 씬 배치 (Phase A 통합) | ~18회 | 중 |
| EC-B | 가격 데이터 SO 생성 (PriceData, ShopData) | ~12회 | 중 |
| EC-C | 가격 변동 연동 (TimeManager, WeatherSystem 이벤트) | ~8회 | 중 |
| EC-D | 상점 UI 연동 + 통합 테스트 | ~10회 | 중 |
| **합계** | | **~48회** | |

[RISK] `EconomyConfig.asset`의 수치 필드(startingGold, maxGold, sellPriceFloor, sellPriceCeiling, transactionLogCapacity 등)는 반드시 `docs/systems/economy-system.md` canonical 참조를 통해 설정해야 한다. 직접 기재 금지 (PATTERN-007).

[RISK] `PriceData` SO는 작물 종류 × 계절 보정 배열 등 다수 배열 필드를 포함한다. MCP `set_property`로 개별 설정 시 호출 수가 급증한다. Editor 스크립트(`InitPriceData.cs`) 방식으로 일괄 생성·설정하는 것을 강력히 권장한다.

---

## 2. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋 폴더 생성 | EC-A, EC-B |
| `create_script` | C# 스크립트 파일 생성 | EC-A |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | EC-A, EC-B |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | EC-A, EC-B, EC-C, EC-D |
| `create_object` | 빈 GameObject 생성 | EC-A |
| `add_component` | MonoBehaviour 컴포넌트 부착 | EC-A |
| `set_parent` | 오브젝트 부모 설정 | EC-A |
| `save_scene` | 씬 저장 | EC-A, EC-B, EC-C, EC-D |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | EC-A, EC-C, EC-D |
| `execute_method` | 런타임 메서드 호출 (AddGold, SpendGold, GetSellPrice 테스트) | EC-A, EC-C |
| `get_console_logs` | 콘솔 로그 확인 | EC-A, EC-C, EC-D |
| `execute_menu_item` | 컴파일 대기, Editor 스크립트 실행 | EC-A, EC-B |

[RISK] `create_scriptable_object` 도구의 가용 여부 및 파라미터 형식 사전 검증 필요. SO 인스턴스 생성이 MCP에서 미지원인 경우, Editor 스크립트를 통한 우회 필요. (→ see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)

---

## 3. 필요 C# 스크립트 목록

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 태스크 순서대로 작성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/Economy/Data/EconomyConfig.cs` | `EconomyConfig` (SO) | `SeedMind.Economy.Data` | EC-A |
| S-02 | `Scripts/Economy/Data/PriceData.cs` | `PriceData` (SO) | `SeedMind.Economy.Data` | EC-A |
| S-03 | `Scripts/Economy/Data/ShopData.cs` | `ShopData` (SO) | `SeedMind.Economy.Data` | EC-A |
| S-04 | `Scripts/Economy/TransactionLog.cs` | `TransactionLog` (Plain C#) | `SeedMind.Economy` | EC-A |
| S-05 | `Scripts/Economy/PriceFluctuationSystem.cs` | `PriceFluctuationSystem` (Plain C#) | `SeedMind.Economy` | EC-A |
| S-06 | `Scripts/Economy/ShopSystem.cs` | `ShopSystem` (MonoBehaviour) | `SeedMind.Economy` | EC-A |
| S-07 | `Scripts/Economy/EconomyManager.cs` | `EconomyManager` (MonoBehaviour, Singleton, ISaveable) | `SeedMind.Economy` | EC-A |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: S-01~S-04 → S-05 → S-06 → S-07. 각 그룹 사이에 Unity 컴파일 대기(`execute_menu_item`)가 필요하다.

---

## 4. 태스크 그룹 A: 기본 경제 프레임워크

**목적**: 경제 시스템의 데이터 정의 클래스(SO 3종), 헬퍼 클래스 2종, MonoBehaviour 2종을 생성하고, `EconomyConfig` SO를 씬에 배치하여 초기화까지 검증한다.

**전제 조건**:
- ARC-002 완료 (`Assets/_Project/Scripts/`, `Assets/_Project/Data/` 폴더 존재, SCN_Farm 기본 계층)
- ARC-008 완료 (`ISaveable` 인터페이스, `SaveManager` 컴파일 완료)
- `SeedMind.Core` 네임스페이스의 Singleton 기반 클래스 컴파일 완료

**예상 MCP 호출**: ~18회

#### EC-A-01: Scripts/Economy/ 폴더 구조 생성

```
create_folder
  path: "Assets/_Project/Scripts/Economy"

create_folder
  path: "Assets/_Project/Scripts/Economy/Data"
```

- **MCP 호출**: 2회

#### EC-A-02: Data 레이어 스크립트 생성 (SO 3종)

```
create_script
  path: "Assets/_Project/Scripts/Economy/Data/EconomyConfig.cs"
  namespace: "SeedMind.Economy.Data"
  // ScriptableObject 상속
  // [CreateAssetMenu(fileName = "SO_EconomyConfig", menuName = "SeedMind/Economy/EconomyConfig")]
  // 필드:
  //   startingGold: int          // → see docs/systems/economy-system.md
  //   maxGold: int               // → see docs/systems/economy-system.md
  //   sellPriceFloor: float      // → see docs/systems/economy-system.md
  //   sellPriceCeiling: float    // → see docs/systems/economy-system.md
  //   categorySupplyParams: SupplyCategoryParam[]  // → see economy-architecture.md 섹션 3.11
  //   transactionLogCapacity: int  // → see docs/systems/economy-system.md
  // → see docs/systems/economy-architecture.md 섹션 3 (EconomyConfig)

create_script
  path: "Assets/_Project/Scripts/Economy/Data/PriceData.cs"
  namespace: "SeedMind.Economy.Data"
  // ScriptableObject 상속
  // [CreateAssetMenu(fileName = "SO_Price", menuName = "SeedMind/Economy/PriceData")]
  // 필드:
  //   itemId: string
  //   category: ItemCategory (enum)
  //   basePrice: int             // → see docs/design.md 섹션 4.2
  //   seasonMultipliers: float[] // 길이 4 (봄/여름/가을/겨울)
  //   demandThreshold: int       // → see docs/systems/economy-system.md
  // → see docs/systems/economy-architecture.md 섹션 3 (PriceData)

create_script
  path: "Assets/_Project/Scripts/Economy/Data/ShopData.cs"
  namespace: "SeedMind.Economy.Data"
  // ScriptableObject 상속
  // [CreateAssetMenu(fileName = "SO_Shop", menuName = "SeedMind/Economy/ShopData")]
  // 필드:
  //   shopName: string
  //   openHour: int
  //   closeHour: int
  //   items: ShopItem[]  (itemId, priceDataRef, requiredLevel, availableSeasons)
  // → see docs/systems/economy-architecture.md 섹션 3 (ShopData)
```

- **MCP 호출**: 3회

#### EC-A-03: Logic 레이어 스크립트 생성 (Plain C# 2종)

```
create_script
  path: "Assets/_Project/Scripts/Economy/TransactionLog.cs"
  namespace: "SeedMind.Economy"
  // Plain C# class (MonoBehaviour 아님)
  // 필드: _entries(List<TransactionEntry>), _capacity(int), TotalEarned(int), TotalSpent(int)
  // 메서드: Record(TransactionEntry), GetRecentSales(string itemId, int days), GetSaveData(), LoadSaveData()
  // → see docs/systems/economy-architecture.md 섹션 2 (TransactionLog)

create_script
  path: "Assets/_Project/Scripts/Economy/PriceFluctuationSystem.cs"
  namespace: "SeedMind.Economy"
  // Plain C# class
  // 필드: _config(EconomyConfig), _transactionLog(TransactionLog)
  // 메서드:
  //   CalculatePrice(PriceData data, Season season, WeatherType weather, int recentSales): int
  //   GetSeasonMultiplier(PriceData data, Season season): float   // → see docs/systems/time-season.md
  //   GetWeatherMultiplier(WeatherType weather): float            // → see docs/systems/economy-system.md
  //   GetSupplyMultiplier(PriceData data, int recentSales): float // → see docs/systems/economy-system.md
  //   RecalculateAllPrices(Season season, WeatherType weather): void
  // → see docs/systems/economy-architecture.md 섹션 2 (PriceFluctuationSystem)
```

- **MCP 호출**: 2회

#### EC-A-04: MonoBehaviour 스크립트 생성 (2종)

```
create_script
  path: "Assets/_Project/Scripts/Economy/ShopSystem.cs"
  namespace: "SeedMind.Economy"
  // MonoBehaviour
  // [SerializeField] _shopData: ShopData
  // [SerializeField] _economyManager: EconomyManager
  // 메서드: Open(), Close(), TryBuyItem(string itemId): bool, TrySellCrop(string itemId, int qty, CropQuality quality): bool
  // GetDisplayPrice(string itemId): int  (PriceFluctuationSystem 경유)
  // → see docs/systems/economy-architecture.md 섹션 2 (ShopSystem)

create_script
  path: "Assets/_Project/Scripts/Economy/EconomyManager.cs"
  namespace: "SeedMind.Economy"
  // MonoBehaviour, Singleton, ISaveable 구현
  // SaveLoadOrder = 50   // → see docs/systems/save-load-architecture.md SaveLoadOrder 할당표
  // [SerializeField] _economyConfig: EconomyConfig
  // private 상태: _currentGold(int), _priceDataMap(Dictionary<string,PriceData>)
  // 프로퍼티: CurrentGold(int), GoldRatio(float)
  // 이벤트: OnGoldChanged(int newGold)
  // 골드 API: AddGold(int amount, string reason), SpendGold(int amount, string reason): bool, CanSpend(int amount): bool
  // 가격 API: GetSellPrice(string itemId, CropQuality quality): int, GetBuyPrice(string itemId): int
  // 일 전환: ProcessDayEnd()
  // ISaveable: GetSaveData(), LoadSaveData(object)
  // OnEnable 구독: TimeManager.OnDayChanged(priority 40), WeatherSystem.OnWeatherChanged
  // → see docs/systems/economy-architecture.md 섹션 2 (EconomyManager)
```

- **MCP 호출**: 2회

#### EC-A-05: asmdef 생성

```
create_script
  path: "Assets/_Project/Scripts/Economy/SeedMind.Economy.asmdef"
  // 참조: SeedMind.Core, SeedMind.Farm
  // → see docs/systems/project-structure.md 섹션 4 (asmdef 의존성 규칙)
```

- **MCP 호출**: 1회

#### EC-A-06: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- 모든 EC-A 스크립트 컴파일 완료 확인 후 SO 에셋 생성 및 씬 배치 진행
- **MCP 호출**: 1회

#### EC-A-07: EconomyConfig SO 에셋 생성

```
create_folder
  path: "Assets/_Project/Data/Config"

create_scriptable_object
  type: "SeedMind.Economy.Data.EconomyConfig"
  path: "Assets/_Project/Data/Config/SO_EconomyConfig.asset"
```

- **MCP 호출**: 2회 (폴더가 이미 존재하면 1회)

#### EC-A-08: EconomyConfig SO 필드 설정

에셋의 모든 수치 필드는 **Inspector에서 직접 입력**하거나, 아래 Editor 스크립트 방식으로 일괄 설정한다. 수치를 이 문서에 직접 기재하는 것은 PATTERN-007 위반이다.

```
// [권장] Editor 스크립트 방식
create_script
  path: "Assets/_Project/Editor/InitEconomyConfig.cs"
  // [MenuItem("SeedMind/Init/EconomyConfig")] 메서드 포함
  // 모든 수치: → see docs/systems/economy-system.md

execute_menu_item
  menu: "SeedMind/Init/EconomyConfig"
```

- **MCP 호출**: 2회 (스크립트 생성 + 메뉴 실행)

**수치 입력 참조 테이블**:

| SO 필드 | 수치 참조 |
|--------|----------|
| startingGold | (→ see docs/systems/economy-system.md 경제 초기값 섹션) |
| maxGold | (→ see docs/systems/economy-system.md) |
| sellPriceFloor / sellPriceCeiling | (→ see docs/systems/economy-system.md 가격 변동 범위 섹션) |
| categorySupplyParams (배열) | (→ see docs/systems/economy-architecture.md 섹션 3.11 수급 정책 테이블) |
| transactionLogCapacity | (→ see docs/systems/economy-system.md) |

#### EC-A-09: EconomyManager, Shop GameObject 씬 배치

```
create_object
  name: "EconomyManager"
  scene: "SCN_Farm"

set_parent
  object: "EconomyManager"
  parent: "--- ECONOMY ---"

add_component
  object: "EconomyManager"
  component: "SeedMind.Economy.EconomyManager"

set_property
  object: "EconomyManager"
  component: "SeedMind.Economy.EconomyManager"
  property: "_economyConfig"
  value: "Assets/_Project/Data/Config/SO_EconomyConfig.asset"

create_object
  name: "Shop"
  scene: "SCN_Farm"

set_parent
  object: "Shop"
  parent: "--- ECONOMY ---"

add_component
  object: "Shop"
  component: "SeedMind.Economy.ShopSystem"
```

- **MCP 호출**: 7회

#### EC-A-10: Play Mode 초기화 검증

```
enter_play_mode

execute_method
  target: "EconomyManager"
  method: "AddGold"
  params: [100, "test"]
  // Console 기대: "EconomyManager initialized, gold=<startingGold → see economy-system.md>"

execute_method
  target: "EconomyManager"
  method: "SpendGold"
  params: [50, "test"]
  // Console 기대: 골드 차감 및 OnGoldChanged 이벤트 발행 확인

get_console_logs

exit_play_mode
```

- **MCP 호출**: 5회 (enter + execute×2 + get_logs + exit)

**검증 체크리스트**:
- [ ] 콘솔에 `EconomyManager initialized` 로그 출력
- [ ] `AddGold` 호출 후 `OnGoldChanged` 이벤트 발행 확인
- [ ] `SpendGold` 호출 후 골드 차감 및 이벤트 발행 확인
- [ ] 컴파일 에러 없음

```
save_scene
  scene: "SCN_Farm"
```

---

## 5. 태스크 그룹 B: 가격 데이터 SO 생성

**목적**: 기본 작물 및 씨앗 `PriceData` SO를 생성하고, `ShopData` SO를 생성하여 잡화점 재고를 정의한다. 모든 가격 수치는 canonical 문서 참조만 허용한다.

**전제 조건**:
- EC-A 완료 (`PriceData.cs`, `ShopData.cs` 컴파일 완료, `EconomyManager` 씬 배치 완료)
- `docs/design.md` 섹션 4.2 (작물 기본 가격) 확인

**예상 MCP 호출**: ~12회

#### EC-B-01: 가격 데이터 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Prices"
```

- **MCP 호출**: 1회

#### EC-B-02: 기본 작물 PriceData SO 생성 (Editor 스크립트 방식 권장)

작물별 `basePrice`, `seasonMultipliers[]`, `demandThreshold` 수치는 이 문서에 직접 기재하지 않는다 (PATTERN-006, PATTERN-007).

```
// [권장] Editor 스크립트 방식
create_script
  path: "Assets/_Project/Editor/InitCropPriceData.cs"
  // [MenuItem("SeedMind/Init/CropPriceData")] 메서드 포함
  // 생성 대상: SO_Price_Potato, SO_Price_Carrot, SO_Price_Tomato, SO_Price_Corn
  // 작물 basePrice → see docs/design.md 섹션 4.2
  // seasonMultipliers → see docs/systems/economy-system.md (계절 보정 계수 섹션)
  // demandThreshold → see docs/systems/economy-system.md (수급 임계값 섹션)
  // 경로: "Assets/_Project/Data/Prices/SO_Price_<Name>.asset"

execute_menu_item
  menu: "SeedMind/Init/CropPriceData"
```

- **MCP 호출**: 2회 (스크립트 생성 + 메뉴 실행)

#### EC-B-03: 씨앗 PriceData SO 생성

씨앗 가격 산출 공식 (→ see `docs/systems/economy-system.md` 섹션 2.2). 수치 직접 기재 금지.

```
create_script
  path: "Assets/_Project/Editor/InitSeedPriceData.cs"
  // [MenuItem("SeedMind/Init/SeedPriceData")] 메서드 포함
  // 생성 대상: SO_Price_Seed_Potato, SO_Price_Seed_Carrot, SO_Price_Seed_Tomato, SO_Price_Seed_Corn
  // 씨앗 basePrice 공식 → see docs/systems/economy-system.md 섹션 2.2
  // category = Seed, demandThreshold = -1 (수급 영향 없음)
  // 경로: "Assets/_Project/Data/Prices/SO_Price_Seed_<Name>.asset"

execute_menu_item
  menu: "SeedMind/Init/SeedPriceData"
```

- **MCP 호출**: 2회

#### EC-B-04: ShopData SO 생성

```
create_folder
  path: "Assets/_Project/Data/Shops"

create_scriptable_object
  type: "SeedMind.Economy.Data.ShopData"
  path: "Assets/_Project/Data/Shops/SO_Shop_GeneralStore.asset"
```

- **MCP 호출**: 2회

#### EC-B-05: ShopData 필드 설정

shopName, openHour, closeHour, items 배열 수치는 Inspector 입력 또는 Editor 스크립트 방식을 사용한다.

```
// [권장] Editor 스크립트 방식
create_script
  path: "Assets/_Project/Editor/InitShopData.cs"
  // [MenuItem("SeedMind/Init/ShopData")] 메서드 포함
  // shopName="잡화점"
  // 운영 시간 → see docs/systems/economy-architecture.md 섹션 Phase B Step B-3
  // items: 씨앗 전종 + 기본 비료 + 속성장 비료
  // requiredLevel, availableSeasons → see docs/systems/economy-system.md

execute_menu_item
  menu: "SeedMind/Init/ShopData"
```

- **MCP 호출**: 2회

#### EC-B-06: ShopSystem에 ShopData 연결 + PriceData 매핑 등록

```
set_property
  object: "Shop"
  component: "SeedMind.Economy.ShopSystem"
  property: "_shopData"
  value: "Assets/_Project/Data/Shops/SO_Shop_GeneralStore.asset"
```

PriceData 매핑 등록(`_priceDataMap`)은 EconomyManager Awake 또는 Inspector 직렬화로 처리한다.

- **MCP 호출**: 1회

Play Mode에서 `GetSellPrice("potato", Normal)` 호출 → 기본가 반환 확인.  
기본가 수치 (→ see `docs/design.md` 섹션 4.2).

**검증 체크리스트**:
- [ ] `SO_Price_Potato`, `SO_Price_Carrot`, `SO_Price_Tomato`, `SO_Price_Corn` 에셋 생성 완료
- [ ] 씨앗 PriceData SO 4종 생성 완료
- [ ] `SO_Shop_GeneralStore` 에셋 생성 완료, items 배열 설정 완료
- [ ] `GetSellPrice` 호출 시 canonical 기본가 반환 확인

```
save_scene
  scene: "SCN_Farm"
```

---

## 6. 태스크 그룹 C: 가격 변동 연동

**목적**: `TimeManager.OnDayChanged` 및 `WeatherSystem.OnWeatherChanged` 이벤트를 `EconomyManager`에 연결하여 일 전환 시 가격 재계산 및 계절/날씨 보정을 검증한다. 수급 보정(demandThreshold 초과 시 가격 하락)도 확인한다.

**전제 조건**:
- EC-A, EC-B 완료
- ARC-019 완료 (`TimeManager`, `WeatherSystem` 씬 배치 및 이벤트 구조 확정)

**예상 MCP 호출**: ~8회

#### EC-C-01: TimeManager 이벤트 연동 확인

`EconomyManager.OnEnable()`에서 `TimeManager.RegisterOnDayChanged(40, handler)`를 호출하여 `RecalculateAllPrices()`를 등록했는지 코드 레벨에서 확인한다. MCP `get_console_logs`로 콘솔 출력을 검증한다.

```
enter_play_mode

get_console_logs
  // 기대: "Prices recalculated: ..." (일 전환 시 출력)
  // 가격 수치는 직접 기재하지 않음 — canonical 기준으로 확인
  // → see docs/design.md 섹션 4.2, docs/systems/economy-system.md

exit_play_mode
```

- **MCP 호출**: 3회

#### EC-C-02: 계절/날씨 보정 테스트

```
enter_play_mode

execute_method
  target: "TimeManager"
  method: "SkipToNextDay"
  // 계절 변경 시 가격 변동 확인 (계절 보정 계수 → see docs/systems/economy-system.md)

execute_method
  target: "WeatherSystem"
  method: "SetWeather"
  params: ["Stormy"]
  // 폭풍 보정 확인 (날씨 보정 계수 → see docs/systems/economy-system.md)

get_console_logs

exit_play_mode
```

- **MCP 호출**: 5회

#### EC-C-03: 수급 보정 테스트

Play Mode에서 `SellCrop(potato, N, Normal)` 반복 호출로 demandThreshold 초과 시 가격 하락을 확인한다.  
- demandThreshold 수치 (→ see `docs/systems/economy-system.md` 수급 임계값 섹션)
- 수급 보정 배수 (→ see `docs/systems/economy-system.md` 수급 보정 공식 섹션)

```
// 콘솔 기대: "Supply penalty for potato: sales=N, threshold=T, multiplier=M"
// N, T, M 수치는 canonical 참조 — 직접 기재 금지
```

**검증 체크리스트**:
- [ ] 일 전환 시 `RecalculateAllPrices()` 호출 콘솔 로그 확인
- [ ] 계절 변경 시 가격 변동 확인 (계절 보정 적용)
- [ ] 폭풍 날씨 설정 후 가격 보정 적용 확인
- [ ] demandThreshold 초과 판매 후 다음 날 가격 하락 확인

```
save_scene
  scene: "SCN_Farm"
```

---

## 7. 태스크 그룹 D: 상점 UI 연동

**목적**: `Canvas_Overlay/ShopPanel`과 `ShopSystem`을 연결하여 구매/판매 버튼 동작, HUD GoldDisplay 갱신, 거래 후 TransactionLog 기록을 통합 검증한다.

**전제 조건**:
- EC-A, EC-B, EC-C 완료
- ARC-018 완료 (`Canvas_Overlay`, `ShopPanel`, HUD 슬롯 존재)

**예상 MCP 호출**: ~10회

#### EC-D-01: ShopUI와 ShopSystem 연결

```
set_property
  object: "Canvas_Overlay/ShopPanel"
  component: "ShopPanelUI"  // ARC-018에서 생성된 컴포넌트
  property: "_shopSystem"
  value: "Shop"  // 씬 내 ShopSystem GO 참조

// ShopPanel 활성화 시 ShopSystem.Open() 호출 연결
// 가격표 표시: GetDisplayPrice() 결과 바인딩
// → see docs/systems/economy-architecture.md Phase D Step D-1
```

- **MCP 호출**: 1회

#### EC-D-02: 구매/판매 버튼 연결

```
// 구매 버튼 → ShopSystem.TryBuyItem(itemId)
// 판매 버튼 → ShopSystem.TrySellCrop(itemId, qty, quality)
// OnGoldChanged → HUD GoldDisplay 즉시 갱신
// → see docs/systems/economy-architecture.md Phase D Step D-2
```

버튼 연결은 Inspector의 UnityEvent 직렬화를 통해 설정하거나 Editor 스크립트 방식을 사용한다.

- **MCP 호출**: 2회 (set_property × 2)

#### EC-D-03: 통합 테스트

```
enter_play_mode

execute_method
  target: "Canvas_Overlay/ShopPanel"
  method: "Open"
  // 상점 열기 → 가격표 표시 확인

execute_method
  target: "Shop"
  method: "TryBuyItem"
  params: ["seed_potato"]
  // 씨앗 구매 → 골드 차감 확인

execute_method
  target: "Shop"
  method: "TrySellCrop"
  params: ["potato", 1, "Normal"]
  // 작물 판매 → 골드 증가 확인

get_console_logs
  // 거래 후 TransactionLog 확인 (디버그 콘솔)
  // → see docs/systems/economy-architecture.md Phase D Step D-3

exit_play_mode
```

- **MCP 호출**: 7회 (enter + execute×3 + get_logs + exit + save_scene)

**검증 체크리스트**:
- [ ] 상점 열기 시 가격표 정상 표시 (GetDisplayPrice() 반환값 기반)
- [ ] 씨앗 구매 후 골드 차감 및 HUD GoldDisplay 즉시 갱신
- [ ] 작물 판매 후 골드 증가 및 HUD GoldDisplay 즉시 갱신
- [ ] 거래 후 TransactionLog에 기록 확인 (콘솔 출력)
- [ ] 골드 부족 시 구매 거부 동작 확인 (CanSpend → false)

```
save_scene
  scene: "SCN_Farm"
```

---

## 8. 전체 완료 체크리스트

- [ ] **EC-A** Scripts/Economy/ 폴더 구조 및 스크립트 7종 생성 완료
- [ ] **EC-A** SeedMind.Economy.asmdef 생성, 의존성 설정 완료
- [ ] **EC-A** SO_EconomyConfig.asset 생성 및 수치 설정 완료 (canonical 참조 확인)
- [ ] **EC-A** EconomyManager, Shop GO 씬 배치 및 초기화 검증 완료
- [ ] **EC-B** SO_Price_Potato/Carrot/Tomato/Corn 에셋 4종 생성 완료
- [ ] **EC-B** SO_Price_Seed_* 에셋 4종 생성 완료
- [ ] **EC-B** SO_Shop_GeneralStore 에셋 생성 및 아이템 목록 설정 완료
- [ ] **EC-C** 일 전환 시 가격 재계산 동작 확인
- [ ] **EC-C** 계절/날씨 보정 적용 확인
- [ ] **EC-C** 수급 보정 (demandThreshold 초과 → 가격 하락) 확인
- [ ] **EC-D** ShopUI ↔ ShopSystem 연결 완료
- [ ] **EC-D** 구매/판매 버튼 및 HUD GoldDisplay 갱신 확인
- [ ] **EC-D** TransactionLog 기록 확인
- [ ] **세이브/로드** EconomySaveData (currentGold, priceFluctuation, transactionLog) 직렬화/역직렬화 확인
  - SaveLoadOrder (→ see `docs/systems/save-load-architecture.md` SaveLoadOrder 할당표)

---

## Open Questions

- [OPEN] `--- ECONOMY ---` 씬 계층 부모 GO가 ARC-002에서 생성되는지, 또는 이 태스크 문서에서 최초 생성해야 하는지 확인 필요 (→ ARC-002 완료 후 확인)
- [OPEN] `ShopPanelUI` 컴포넌트명 및 UnityEvent 직렬화 구조는 ARC-018 확정 후 반영 필요
- [OPEN] EconomySaveData SaveLoadOrder 값 확인 — economy-architecture.md 섹션 4.6 기재값이 save-load-architecture.md 할당표와 일치하는지 검증 필요 (→ ARC-008/ARC-004 교차 확인)
- [OPEN] `categorySupplyParams` 배열 4종의 ItemCategory enum 값 — economy-architecture.md 섹션 3.11 확정 후 반영 필요

---

## Risks

- [RISK] `PriceData` SO의 `seasonMultipliers(float[])` 배열을 MCP `set_property`로 개별 설정할 경우 호출 수가 급증한다. Editor 스크립트 일괄 처리 방식(InitCropPriceData.cs)을 사용해야 한다.
- [RISK] `create_scriptable_object` MCP 도구의 가용 여부 및 배열 필드 설정 지원 사전 검증 필요. 미지원 시 Editor 스크립트 방식 100% 전환 필요.
- [RISK] `ShopSystem.TryBuyItem` / `TrySellCrop` 호출 시 `InventoryManager` 참조가 누락되면 NullReference 발생. EC-D 진입 전 ARC-013 완료 상태 재확인 필요.
- [RISK] `EconomyManager.OnEnable`의 `TimeManager.RegisterOnDayChanged` 등록 우선순위(40)가 다른 시스템과 충돌할 경우 가격 재계산 순서 오류 발생 가능. (→ see `docs/systems/time-season-architecture.md` 이벤트 우선순위 섹션)

---

## Cross-references

- `docs/systems/economy-architecture.md` (ARC-004) — 상위 설계 문서, 클래스 구조, MCP 구현 계획 원본
- `docs/systems/economy-system.md` — 경제 수치 canonical 출처 (startingGold, 가격 변동 범위, 수급 보정 계수)
- `docs/design.md` 섹션 4.2 — 작물 기본 가격 canonical 출처
- `docs/systems/project-structure.md` 섹션 2, 3, 4 — 폴더 구조 및 asmdef 의존성 규칙
- `docs/systems/time-season-architecture.md` 섹션 2.5, 4.3, 4.4 — OnDayChanged 이벤트 구조, 이벤트 우선순위
- `docs/systems/farming-architecture.md` 섹션 6 — FarmEvents.OnCropHarvested 이벤트 구조
- `docs/systems/save-load-architecture.md` — SaveLoadOrder 할당표, ISaveable 인터페이스
- `docs/pipeline/data-pipeline.md` Part II 섹션 2.4 — BuildingData SO 스키마 참조
- `docs/mcp/energy-tasks.md` (ARC-047) — 참조 패턴 문서
- `docs/mcp/scene-setup-tasks.md` (ARC-002) — 씬 기본 계층 생성 문서
- `docs/mcp/save-load-tasks.md` (ARC-008) — SaveManager, ISaveable 생성 문서
