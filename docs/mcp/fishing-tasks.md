# 낚시 시스템 MCP 태스크 시퀀스 (ARC-028)

> FishingManager, FishData SO, FishingMinigame, FishingConfig SO, FishingPoint 씬 배치, 기존 시스템 확장, 이벤트 연동, 세이브/로드, 통합 테스트를 MCP for Unity 태스크로 상세 정의
> 작성: Claude Code (Opus 4.6) | 2026-04-07
> Phase 1 | 문서 ID: ARC-028

---

## 1. 개요

### 1.1 목적

이 문서는 `docs/systems/fishing-architecture.md`(ARC-026) Part VI에서 요약된 MCP 구현 계획(Phase A~F)을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 낚시 시스템의 데이터 레이어(FishData SO, FishingConfig SO), 시스템 레이어(FishingManager, FishingMinigame, FishingEvents), 씬 배치(FishingPoint), 기존 시스템 확장(XPSource, HarvestOrigin, GameSaveData -- 이미 RESOLVED), 이벤트 연동, 세이브/로드 통합을 완성한다.

### 1.2 의존성

```
낚시 시스템 MCP 태스크 의존 관계:
├── SeedMind.Core     (TimeManager, SaveManager, EventBus, ISaveable, HarvestOrigin)
├── SeedMind.Farm     (FarmZoneManager, ZoneType, FarmGrid)
├── SeedMind.Level    (ProgressionManager, XPSource)
├── SeedMind.Economy  (EconomyManager -- 물고기 판매 가격 계산)
├── SeedMind.Player   (InventoryManager -- 물고기 인벤토리 추가)
└── SeedMind.UI       (Canvas_HUD, Canvas_Overlay -- 미니게임 UI는 별도 태스크)
```

(-> see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 1.3 전제 조건 (완료된 태스크 의존성)

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조, SCN_Farm 기본 계층 (MANAGERS, FARM, PLAYER, UI), Canvas_HUD, Canvas_Overlay |
| ARC-003 | `docs/mcp/farming-tasks.md` | Phase A~C 전체 | FarmGrid, CropData SO, ToolData SO, GrowthSystem |
| ARC-013 | `docs/mcp/inventory-tasks.md` | Phase A~C 전체 | InventoryManager, PlayerInventory, ItemSlot, ItemType.Fish |
| ARC-023 | `docs/mcp/farm-expansion-tasks.md` | 전체 | FarmZoneManager, ZoneType.Pond, Zone F 타일 구조 |
| DES-013 | `docs/systems/fishing-system.md` | 설계 완료 | 어종 15종, 미니게임 규칙, 희귀도 체계 |
| ARC-026 | `docs/systems/fishing-architecture.md` | 설계 완료 | 클래스 다이어그램, 연동 흐름, 세이브/로드 구조 |

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
| `InventoryManager` (인벤토리) | ARC-013 |
| `EconomyManager` (경제) | economy-architecture.md 기반 |
| `ProgressionManager` (레벨/XP) | BAL-002 기반 |
| `TimeManager` (시간/시즌) | ARC-004 기반 |
| `FarmZoneManager` (구역 관리) | ARC-023 기반 |
| `SaveManager` (세이브/로드) | ARC-011 기반 |

### 1.5 총 MCP 호출 예상 수

| 태스크 | 설명 | MCP 호출 수 |
|--------|------|------------|
| F-1 | 데이터 레이어 스크립트 생성 (enum, class, SO, MonoBehaviour) | ~15회 |
| F-2 | FishData SO 에셋 생성 (어종 15종) | 196회 |
| F-3 | FishingConfig SO 에셋 생성 | 11회 |
| F-4 | 씬 배치 (FishingManager, FishingPoint x3) | 14회 |
| F-5 | 기존 시스템 확장 (FIX-049~053 RESOLVED 확인 + 실제 C# 반영) | 5회 |
| F-6 | 연동 설정 (이벤트 구독, 세이브/로드 등록) | 5회 |
| F-7 | 통합 테스트 시퀀스 | ~32회 |
| **합계** | | **~278회** |

[RISK] 총 ~272회 MCP 호출은 상당하다. F-2(FishData SO 생성)의 196회를 Editor 스크립트(CreateFishAssets.cs)로 일괄 생성하면 ~6회로 감소 가능. Editor 스크립트 우회를 강력 권장.

---

## 2. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋/스크립트 폴더 생성 | F-1 |
| `create_script` | C# 스크립트 파일 생성 | F-1 |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | F-2, F-3 |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | F-2~F-6 전체 |
| `create_object` | 빈 GameObject 생성 | F-4 |
| `add_component` | MonoBehaviour 컴포넌트 부착 | F-4 |
| `set_parent` | 오브젝트 부모 설정 | F-4 |
| `save_scene` | 씬 저장 | F-4, F-7 |
| `modify_script` | 기존 스크립트 수정 (enum 확장, switch 문 추가) | F-5, F-6 |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | F-7 |
| `execute_method` | 런타임 메서드 호출 (테스트) | F-7 |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | F-7 |

[RISK] `set_property`로 `float[]` (timeWeights) 및 `FishData[]` (배열 참조)를 설정할 수 있는지 사전 검증 필요. 미지원 시 DataRegistry 패턴으로 런타임 로드 전환.

---

## 3. 필요 C# 스크립트 목록

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 태스크 순서대로 작성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/Fishing/FishingState.cs` | `FishingState` (enum) | `SeedMind.Fishing` | F-1 Phase 1 |
| S-02 | `Scripts/Fishing/FishRarity.cs` | `FishRarity` (enum) | `SeedMind.Fishing` | F-1 Phase 1 |
| S-03 | `Scripts/Fishing/MinigameResult.cs` | `MinigameResult` (enum) | `SeedMind.Fishing` | F-1 Phase 1 |
| S-04 | `Scripts/Fishing/FishingStats.cs` | `FishingStats` (Plain C#) | `SeedMind.Fishing` | F-1 Phase 1 |
| S-05 | `Scripts/Fishing/FishingSaveData.cs` | `FishingSaveData` (Serializable) | `SeedMind.Fishing` | F-1 Phase 1 |
| S-06 | `Scripts/Fishing/FishingEvents.cs` | `FishingEvents` (static) | `SeedMind.Fishing` | F-1 Phase 2 |
| S-07 | `Scripts/Fishing/Data/FishData.cs` | `FishData` (SO, GameDataSO 상속) | `SeedMind.Fishing.Data` | F-1 Phase 2 |
| S-08 | `Scripts/Fishing/Data/FishingConfig.cs` | `FishingConfig` (SO) | `SeedMind.Fishing.Data` | F-1 Phase 2 |
| S-09 | `Scripts/Fishing/FishingPoint.cs` | `FishingPoint` (MonoBehaviour) | `SeedMind.Fishing` | F-1 Phase 3 |
| S-10 | `Scripts/Fishing/FishingMinigame.cs` | `FishingMinigame` (Plain C#) | `SeedMind.Fishing` | F-1 Phase 3 |
| S-11 | `Scripts/Fishing/FishingManager.cs` | `FishingManager` (MonoBehaviour, Singleton, ISaveable) | `SeedMind.Fishing` | F-1 Phase 4 |

(모든 경로 접두어: `Assets/_Project/`)

**컴파일 순서**: S-01~S-05 -> S-06~S-08 -> S-09~S-10 -> S-11. 각 Phase 사이에 Unity 컴파일 대기가 필요하다.

[RISK] S-11(FishingManager)은 SeedMind.Core, SeedMind.Farm, SeedMind.Level, SeedMind.Player에 대한 참조가 필요하므로, asmdef 의존성 누락 시 컴파일 실패한다.

---

## 4. 태스크 목록

---

### F-1: 데이터 레이어 스크립트 생성

**목적**: 낚시 시스템의 모든 C# 스크립트를 생성한다.

**전제**: ARC-002(scene-setup-tasks.md)의 폴더 구조가 완성된 상태.

#### F-1-01: Fishing 폴더 구조 생성

```
create_folder
  path: "Assets/_Project/Scripts/Fishing"

create_folder
  path: "Assets/_Project/Scripts/Fishing/Data"

create_folder
  path: "Assets/_Project/Data/Fish"
```

- **MCP 호출**: 3회

#### F-1-02: Phase 1 -- enum 및 데이터 클래스 스크립트

```
create_script
  path: "Assets/_Project/Scripts/Fishing/FishingState.cs"
  content: FishingState enum (Idle, Casting, Waiting, Biting, Reeling, Success, Fail)
  // -> see docs/systems/fishing-architecture.md 섹션 11

create_script
  path: "Assets/_Project/Scripts/Fishing/FishRarity.cs"
  content: FishRarity enum (Common, Uncommon, Rare, Legendary)
  // -> see docs/systems/fishing-architecture.md 섹션 3

create_script
  path: "Assets/_Project/Scripts/Fishing/MinigameResult.cs"
  content: MinigameResult enum (InProgress, Success, Fail)
  // -> see docs/systems/fishing-architecture.md 섹션 2

create_script
  path: "Assets/_Project/Scripts/Fishing/FishingStats.cs"
  content: FishingStats (Plain C# class)
    필드: totalCasts, totalCaught, totalFailed, caughtByFishId (Dictionary<string,int>),
          rareFishCaught, maxStreak, currentStreak
  // -> see docs/systems/fishing-architecture.md 섹션 1

create_script
  path: "Assets/_Project/Scripts/Fishing/FishingSaveData.cs"
  content: FishingSaveData (Serializable class)
    필드: totalCasts, totalCaught, totalFailed, caughtByFishId,
          rareFishCaught, maxStreak, currentStreak
  // -> see docs/systems/fishing-architecture.md 섹션 10
```

- **MCP 호출**: 5회
- **검증**: Unity 컴파일 대기 -> 에러 없음 확인

#### F-1-03: Phase 2 -- 이벤트 허브 및 SO 스크립트

```
create_script
  path: "Assets/_Project/Scripts/Fishing/FishingEvents.cs"
  content: FishingEvents (static class)
    이벤트: OnFishCast (Action<FishingPoint>),
            OnFishBite (Action<FishData>),
            OnFishCaught (Action<FishData, CropQuality>),
            OnFishingFailed (Action),
            OnInventoryFull (Action<FishData>)
  // -> see docs/systems/fishing-architecture.md 섹션 1

create_script
  path: "Assets/_Project/Scripts/Fishing/Data/FishData.cs"
  content: FishData : GameDataSO, IInventoryItem
    상속 필드: dataId, displayName, description, icon
    고유 필드: fishId, rarity (FishRarity), basePrice, seasonAvailability (SeasonFlag),
              timeWeights (float[5]), weatherBonus (WeatherFlag),
              minigameDifficulty, targetZoneWidthMul, moveSpeed,
              maxStackSize, expReward
    IInventoryItem: ItemType => ItemType.Fish, MaxStackSize, Sellable => true
  // -> see docs/systems/fishing-architecture.md 섹션 3

create_script
  path: "Assets/_Project/Scripts/Fishing/Data/FishingConfig.cs"
  content: FishingConfig : ScriptableObject
    필드: castDurationRange (Vector2), biteDelayRange (Vector2),
          biteWindowDuration (float), reelingDuration (float),
          excitementDecayRate (float), excitementGainPerInput (float),
          successThreshold (float), failThreshold (float),
          qualityThresholds (float[4])
  // -> see docs/systems/fishing-architecture.md 섹션 1
```

- **MCP 호출**: 3회
- **검증**: Unity 컴파일 대기 -> 에러 없음 확인

#### F-1-04: Phase 3 -- FishingPoint 및 FishingMinigame

```
create_script
  path: "Assets/_Project/Scripts/Fishing/FishingPoint.cs"
  content: FishingPoint (MonoBehaviour)
    필드: pointId (string), tilePosition (Vector2Int),
          availableFish (FishData[]), rarityWeights (float[4]),
          _isOccupied (bool), _dailyUseCount (int)
    프로퍼티: IsAvailable
    메서드: CanFish(), SetOccupied(), ResetDailyCount()
  // -> see docs/systems/fishing-architecture.md 섹션 4

create_script
  path: "Assets/_Project/Scripts/Fishing/FishingMinigame.cs"
  content: FishingMinigame (Plain C# class)
    상태: _excitementGauge, _targetZoneCenter, _targetZoneWidth,
          _elapsedTime, _isActive
    설정: _config (FishingConfig), _currentFish (FishData)
    메서드: Start(), Update(float deltaTime), ProcessInput(),
            Reset(), MoveTargetZone(), ApplyDecay(),
            CheckCompletion(), CalculateQuality()
  // -> see docs/systems/fishing-architecture.md 섹션 2
```

- **MCP 호출**: 2회
- **검증**: Unity 컴파일 대기 -> 에러 없음 확인

#### F-1-05: Phase 4 -- FishingManager MonoBehaviour

```
create_script
  path: "Assets/_Project/Scripts/Fishing/FishingManager.cs"
  content: FishingManager (MonoBehaviour, Singleton, ISaveable)
    상태: _fishDataRegistry (FishData[]), _fishingPoints (FishingPoint[]),
          _currentState (FishingState), _activeFish (FishData),
          _fishingStats (FishingStats)
    설정: _fishingConfig (FishingConfig), _minigame (FishingMinigame)
    프로퍼티: CurrentState, ActiveFish, FishingStats, IsPlayerFishing
    이벤트: OnStateChanged, OnFishCast, OnFishBite, OnFishCaught, OnFishingFailed
    메서드: Initialize(), TryStartFishing(FishingPoint), CancelFishing(),
            OnMinigameInput(), SelectRandomFish(), CalculateBiteDelay(),
            CompleteFishing(), GrantRewards(),
            GetSaveData(), LoadSaveData()
    ISaveable: SaveLoadOrder = 52
    구독: TimeManager.RegisterOnDayChanged(priority: 55),
          TimeManager.RegisterOnSeasonChanged(priority: 55)
  // -> see docs/systems/fishing-architecture.md 섹션 1, 11, 12
```

- **MCP 호출**: 1회
- **검증**: Unity 컴파일 대기 -> 에러 없음 확인. 특히 asmdef 의존성(SeedMind.Core, SeedMind.Farm, SeedMind.Level, SeedMind.Player) 확인

#### F-1 asmdef 생성

```
create_script
  path: "Assets/_Project/Scripts/Fishing/SeedMind.Fishing.asmdef"
  content: asmdef JSON
    name: "SeedMind.Fishing"
    references: ["SeedMind.Core", "SeedMind.Farm", "SeedMind.Level", "SeedMind.Player"]
  // -> see docs/systems/fishing-architecture.md 섹션 1.1
  // -> see docs/systems/project-structure.md for asmdef 규칙
```

- **MCP 호출**: 1회 (F-1-02 이전에 실행해야 함)

**F-1 합계**: 3 + 5 + 3 + 2 + 1 + (asmdef)1 = **~15회**

[RISK] F-1-05의 FishingManager는 SeedMind.Core의 ISaveable, TimeManager 및 SeedMind.Player의 InventoryManager에 의존한다. asmdef에 모든 참조가 포함되어야 하며, 누락 시 컴파일 실패로 이후 모든 태스크가 블록된다.

---

### F-2: FishData SO 에셋 생성 (어종 15종)

**목적**: 어종 15종의 FishData SO 에셋을 생성하고 필드값을 설정한다.

**전제**: F-1 Phase 2 완료 (FishData.cs 컴파일 완료)

**의존 태스크**: F-1

> **주의**: 모든 수치(basePrice, expReward, timeWeights, rarityWeights 등)는 MCP 실행 시점에 canonical 문서에서 읽어 입력한다. 본 문서에서 수치를 직접 기재하지 않는다 (PATTERN-006).

#### F-2-01: SO_Fish_CrucianCarp (붕어)

```
create_scriptable_object
  type: "SeedMind.Fishing.Data.FishData"
  asset_path: "Assets/_Project/Data/Fish/SO_Fish_CrucianCarp.asset"

set_property  target: "SO_Fish_CrucianCarp"
  dataId = "fish_crucian_carp"
  displayName = "붕어"
  description = "연못에서 가장 흔히 잡히는 민물고기. 사계절 잡을 수 있다."
  fishId = "fish_crucian_carp"
  rarity = 0                                   // Common (-> see docs/systems/fishing-architecture.md 섹션 3)
  basePrice = 0                                // (-> see docs/systems/fishing-system.md 섹션 4.2)
  seasonAvailability = 0                       // (-> see docs/systems/fishing-system.md 섹션 4.2)
  timeWeights = [0,0,0,0,0]                    // (-> see docs/systems/fishing-system.md 섹션 4.2)
  weatherBonus = 0                             // (-> see docs/systems/fishing-system.md 섹션 4.2)
  minigameDifficulty = 0                       // (-> see docs/systems/fishing-system.md 섹션 3.3)
  targetZoneWidthMul = 0                       // (-> see docs/systems/fishing-system.md 섹션 3.3)
  moveSpeed = 0                                // (-> see docs/systems/fishing-system.md 섹션 3.3)
  maxStackSize = 0                             // (-> see docs/systems/inventory-architecture.md 섹션 1.1)
  expReward = 0                                // (-> see docs/balance/progression-curve.md)
```

- **MCP 호출**: 1(생성) + 12(필드 설정) = 13회

#### F-2-02 ~ F-2-15: 나머지 14종

동일 패턴으로 14종 추가 생성. 각 어종의 ID, 이름, 설명, 희귀도는 아래 표에 따르며, 수치 파라미터는 모두 플레이스홀더 0으로 기재 후 canonical 참조 표기한다.

| 스텝 | 에셋명 | dataId | displayName | rarity |
|------|--------|--------|-------------|--------|
| F-2-02 | SO_Fish_Loach | `fish_loach` | 미꾸라지 | Common (0) |
| F-2-03 | SO_Fish_Carp | `fish_carp` | 잉어 | Common (0) |
| F-2-04 | SO_Fish_Catfish | `fish_catfish` | 메기 | Uncommon (1) |
| F-2-05 | SO_Fish_Trout | `fish_trout` | 송어 | Uncommon (1) |
| F-2-06 | SO_Fish_Crawfish | `fish_crawfish` | 가재 | Uncommon (1) |
| F-2-07 | SO_Fish_Bluegill | `fish_bluegill` | 블루길 | Common (0) |
| F-2-08 | SO_Fish_Eel | `fish_eel` | 뱀장어 | Uncommon (1) |
| F-2-09 | SO_Fish_Smelt | `fish_smelt` | 빙어 | Common (0) |
| F-2-10 | SO_Fish_CherrySalmon | `fish_cherry_salmon` | 산천어 | Rare (2) |
| F-2-11 | SO_Fish_GoldenCarp | `fish_golden_carp` | 황금 잉어 | Rare (2) |
| F-2-12 | SO_Fish_LegendCatfish | `fish_legend_catfish` | 전설의 메기왕 | Legendary (3) |
| F-2-13 | SO_Fish_RainbowTrout | `fish_rainbow_trout` | 무지개송어 | Rare (2) |
| F-2-14 | SO_Fish_LotusKoi | `fish_lotus_koi` | 연꽃 잉어 | Legendary (3) |
| F-2-15 | SO_Fish_IceKingSmelt | `fish_ice_king_smelt` | 얼음 빙어왕 | Rare (2) |

(-> see `docs/systems/fishing-system.md` 섹션 4.2 for 전체 어종 목록 canonical)

각 에셋의 basePrice, expReward, seasonAvailability, timeWeights, weatherBonus, minigameDifficulty, targetZoneWidthMul, moveSpeed, maxStackSize는 모두 플레이스홀더 0으로 설정. MCP 실행 시점에 canonical 값 대입.

- **MCP 호출 (F-2-02 ~ F-2-15)**: 14종 x 13회 = 182회
- **F-2-01 포함 합계**: 13 + 182 = **195회**

[RISK] 195회는 과다하다. **권장 대안**: Editor 스크립트 `CreateFishAssets.cs`를 생성하여 `fishing-system.md` 섹션 4.2 테이블에서 데이터를 읽어 15종을 일괄 생성하면 ~6회(스크립트 생성 1 + 실행 1 + 검증 4)로 감소 가능.

**F-2 합계**: **~196회** (Editor 스크립트 사용 시 ~6회)

---

### F-3: FishingConfig SO 에셋 생성

**목적**: 미니게임 밸런스 파라미터를 담는 FishingConfig SO 에셋을 생성한다.

**전제**: F-1 Phase 2 완료 (FishingConfig.cs 컴파일 완료)

**의존 태스크**: F-1

#### F-3-01: SO_FishingConfig

```
create_scriptable_object
  type: "SeedMind.Fishing.Data.FishingConfig"
  asset_path: "Assets/_Project/Data/Fish/SO_FishingConfig.asset"

set_property  target: "SO_FishingConfig"
  castDurationRange = (0, 0)                   // Vector2 (min, max) (-> see docs/systems/fishing-system.md 섹션 2.4)
  biteDelayRange = (0, 0)                      // Vector2 (min, max) (-> see docs/systems/fishing-system.md 섹션 2.2)
  biteWindowDuration = 0                       // (-> see docs/systems/fishing-system.md 섹션 2.2)
  reelingDuration = 0                          // (-> see docs/systems/fishing-system.md 섹션 3.3)
  excitementDecayRate = 0                      // (-> see docs/systems/fishing-system.md 섹션 3.3)
  excitementGainPerInput = 0                   // (-> see docs/systems/fishing-system.md 섹션 3.3)
  successThreshold = 0                         // (-> see docs/systems/fishing-system.md 섹션 3.3)
  failThreshold = 0                            // (-> see docs/systems/fishing-system.md 섹션 3.3)
  qualityThresholds = [0, 0, 0, 0]             // [Normal, Silver, Gold, Iridium] (-> see docs/systems/fishing-system.md 섹션 3.2)
```

> **주의**: 모든 수치는 플레이스홀더 0. MCP 실행 시점에 canonical 문서에서 확정값을 대입한다.

- **MCP 호출**: 1(생성) + 9(필드 설정) = 10회

[RISK] `qualityThresholds` (float[])의 MCP `set_property` 배열 지원 여부 사전 검증 필요. 미지원 시 FishingManager.Initialize()에서 코드 fallback 초기화.

#### F-3-02: 컴파일 확인

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

**F-3 합계**: 10 + 1 = **11회**

---

### F-4: 씬 배치

**목적**: SCN_Farm 씬에 FishingManager GameObject와 FishingPoint 3개를 배치한다.

**전제**: F-1 Phase 4 완료 (FishingManager.cs, FishingPoint.cs 컴파일), F-2, F-3 완료 (SO 에셋 존재)

**의존 태스크**: F-1, F-2, F-3

#### F-4-01: FishingManager GameObject 생성

```
create_object
  name: "FishingManager"
  scene: "SCN_Farm"

set_parent
  child: "FishingManager"
  parent: "--- MANAGERS ---"

add_component
  target: "FishingManager"
  component: "SeedMind.Fishing.FishingManager"
```

- **MCP 호출**: 3회

#### F-4-02: FishingManager SO 참조 연결

```
set_property  target: "FishingManager" (FishingManager 컴포넌트)
  _fishingConfig = "Assets/_Project/Data/Fish/SO_FishingConfig.asset"

set_property  target: "FishingManager" (FishingManager 컴포넌트)
  _fishDataRegistry = [
    "Assets/_Project/Data/Fish/SO_Fish_CrucianCarp.asset",
    "Assets/_Project/Data/Fish/SO_Fish_Loach.asset",
    "Assets/_Project/Data/Fish/SO_Fish_Carp.asset",
    "Assets/_Project/Data/Fish/SO_Fish_Catfish.asset",
    "Assets/_Project/Data/Fish/SO_Fish_Trout.asset",
    "Assets/_Project/Data/Fish/SO_Fish_Crawfish.asset",
    "Assets/_Project/Data/Fish/SO_Fish_Bluegill.asset",
    "Assets/_Project/Data/Fish/SO_Fish_Eel.asset",
    "Assets/_Project/Data/Fish/SO_Fish_Smelt.asset",
    "Assets/_Project/Data/Fish/SO_Fish_CherrySalmon.asset",
    "Assets/_Project/Data/Fish/SO_Fish_GoldenCarp.asset",
    "Assets/_Project/Data/Fish/SO_Fish_LegendCatfish.asset",
    "Assets/_Project/Data/Fish/SO_Fish_RainbowTrout.asset",
    "Assets/_Project/Data/Fish/SO_Fish_LotusKoi.asset",
    "Assets/_Project/Data/Fish/SO_Fish_IceKingSmelt.asset"
  ]
```

> **주의**: `_fishDataRegistry`는 FishData[] 배열이다. MCP의 `set_property`로 SO 배열 참조를 설정할 수 있는지 검증 필요. 미지원 시 DataRegistry 패턴을 사용하여 코드에서 동적 로드.

- **MCP 호출**: 2회

[RISK] MCP에서 SerializedProperty를 통한 SO 배열 참조 설정 가능 여부 불확실. (-> see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련). 대안: DataRegistry.GetAllFishData()로 런타임 로드.

#### F-4-03: FishingPoint x3 배치 (Zone F 내)

Zone F의 연못 가장자리 3개 지점에 FishingPoint를 배치한다. 타일 좌표는 Zone F 레이아웃에 따른다 `(-> see docs/systems/farm-expansion.md 섹션 4.3)`.

```
// FishingPoint 1
create_object
  name: "FishingPoint_01"
  scene: "SCN_Farm"

set_parent
  child: "FishingPoint_01"
  parent: "--- FARM ---"

add_component
  target: "FishingPoint_01"
  component: "SeedMind.Fishing.FishingPoint"

set_property  target: "FishingPoint_01" (FishingPoint 컴포넌트)
  pointId = "fp_01"
  tilePosition = (0, 0)                       // (-> see docs/systems/farm-expansion.md 섹션 4.3 for Zone F 배치 좌표)
  // availableFish, rarityWeights는 F-4-02 이후 별도 설정 필요

// FishingPoint 2 (동일 패턴)
create_object  name: "FishingPoint_02"  scene: "SCN_Farm"
set_parent     child: "FishingPoint_02"  parent: "--- FARM ---"
add_component  target: "FishingPoint_02"  component: "SeedMind.Fishing.FishingPoint"
set_property   target: "FishingPoint_02"  pointId = "fp_02"  tilePosition = (0, 0)

// FishingPoint 3 (동일 패턴)
create_object  name: "FishingPoint_03"  scene: "SCN_Farm"
set_parent     child: "FishingPoint_03"  parent: "--- FARM ---"
add_component  target: "FishingPoint_03"  component: "SeedMind.Fishing.FishingPoint"
set_property   target: "FishingPoint_03"  pointId = "fp_03"  tilePosition = (0, 0)
```

> **주의**: tilePosition의 (0,0)은 플레이스홀더. 실제 좌표는 Zone F 레이아웃 확정 후 대입 `(-> see docs/systems/farm-expansion.md 섹션 4.3)`.

- **MCP 호출**: 3포인트 x (create 1 + parent 1 + component 1 + property 1) = 12회 중, 간소화하여 약 **8회** (일부 set_property 병합)

#### F-4-04: FishingManager에 FishingPoint 참조 연결

```
set_property  target: "FishingManager" (FishingManager 컴포넌트)
  _fishingPoints = [FishingPoint_01, FishingPoint_02, FishingPoint_03]
```

- **MCP 호출**: 1회

[RISK] FishingPoint 배열 참조 설정이 씬 내 GameObject 참조이므로, SO 배열과는 다른 방식으로 처리된다. MCP `set_property`에서 씬 내 오브젝트 배열 참조 지원 여부 확인 필요.

#### F-4-05: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

**F-4 합계**: 3 + 2 + 8 + 1 + 1 = **~14회** (최소 추정치, 실제 배열 설정 방식에 따라 변동)

---

### F-5: 기존 시스템 확장

**목적**: 낚시 시스템 연동을 위한 기존 스크립트 수정. FIX-049~053이 이미 문서 레벨에서 RESOLVED 처리되었으므로, 실제 C# 스크립트에 변경 사항을 반영한다.

**전제**: F-1 완료. 기존 시스템 스크립트가 컴파일 완료 상태.

**의존 태스크**: F-1

> **참고**: FIX-049~053은 아키텍처 문서 레벨에서 이미 RESOLVED 처리되었다 `(-> see docs/systems/fishing-architecture.md FIX 태스크 제안)`. 여기서는 실제 C# 파일에 해당 변경 사항을 적용한다.

#### F-5-01: HarvestOrigin enum에 Fishing 추가 (FIX-049 반영)

```
modify_script
  path: "Assets/_Project/Scripts/SeedMind/HarvestOrigin.cs"
  action: HarvestOrigin enum에 Fishing = 3 값 추가
  // -> see docs/systems/economy-architecture.md 섹션 3.10.2 [RESOLVED-FIX-049]
```

- **MCP 호출**: 1회

#### F-5-02: XPSource enum에 FishingCatch 추가 및 GetExpForSource() switch 문 확장 (FIX-050 반영)

```
modify_script
  path: "Assets/_Project/Scripts/Level/XPSource.cs"
  action: XPSource enum에 FishingCatch 값 추가
  // -> see docs/systems/progression-architecture.md 섹션 2.2 [RESOLVED-FIX-050]

modify_script
  path: "Assets/_Project/Scripts/Level/ProgressionManager.cs"
  action: GetExpForSource() switch 문에 FishingCatch case 추가
    case XPSource.FishingCatch:
      var (fishData, fishQuality) = ((FishData, CropQuality))context;
      return CalculateFishingExp(fishData, fishQuality);
    // CalculateFishingExp() 메서드 정의도 추가
  // -> see docs/systems/fishing-architecture.md 섹션 6.2
```

- **MCP 호출**: 2회

#### F-5-03: GameSaveData에 fishing 필드 추가 (FIX-051 반영)

```
modify_script
  path: "Assets/_Project/Scripts/Core/SaveLoad/GameSaveData.cs"
  action: public FishingSaveData fishing; 필드 추가 (null 허용)
  // -> see docs/pipeline/data-pipeline.md Part I 섹션 2.1 [RESOLVED-FIX-051]
```

- **MCP 호출**: 1회

#### F-5-04: 컴파일 확인

```
execute_menu_item
  menu: "Assets/Refresh"
  // Unity 컴파일 대기 후 에러 없음 확인
```

- **MCP 호출**: 1회

> **주의**: FIX-052(SaveLoadOrder 할당표 갱신)과 FIX-053(ItemType.Fish 추가)은 이미 문서 레벨에서 RESOLVED. C# 코드 반영은 각각 save-load-architecture.md와 inventory-tasks.md에서 처리 완료.

**F-5 합계**: 1 + 2 + 1 + 1 = **5회**

---

### F-6: 연동 설정 (이벤트 구독, 세이브/로드 등록)

**목적**: FishingManager와 외부 시스템(ProgressionManager, SaveManager, AchievementManager, QuestManager) 간의 이벤트 연동을 설정한다.

**전제**: F-1, F-4, F-5 완료.

**의존 태스크**: F-4 (씬 배치), F-5 (기존 시스템 확장)

#### F-6-01: ProgressionManager에 FishingEvents.OnFishCaught 구독 추가

```
modify_script
  path: "Assets/_Project/Scripts/Level/ProgressionManager.cs"
  action: OnEnable()에 FishingEvents.OnFishCaught += HandleFishCaught 구독 추가
          OnDisable()에 FishingEvents.OnFishCaught -= HandleFishCaught 구독 해제 추가
          HandleFishCaught(FishData fish, CropQuality quality) 메서드 정의:
            int xp = GetExpForSource(XPSource.FishingCatch, (fish, quality));
            AddExp(xp, XPSource.FishingCatch);
  // -> see docs/systems/fishing-architecture.md 섹션 6.3
```

- **MCP 호출**: 1회

#### F-6-02: SaveManager에 FishingManager ISaveable 등록

```
modify_script
  path: "Assets/_Project/Scripts/Core/SaveLoad/SaveManager.cs"
  action: FishingManager ISaveable 등록 확인 (SaveLoadOrder = 52)
  // -> see docs/systems/fishing-architecture.md 섹션 12.2
  // -> see docs/systems/save-load-architecture.md 섹션 7 [RESOLVED-FIX-052]
```

- **MCP 호출**: 1회

#### F-6-03: EconomyManager GetGreenhouseMultiplier() 확장

```
modify_script
  path: "Assets/_Project/Scripts/Economy/EconomyManager.cs"
  action: GetGreenhouseMultiplier() switch 문에 case HarvestOrigin.Fishing: return 1.0; 추가
  // -> see docs/systems/fishing-architecture.md 섹션 5.3
  // -> see docs/systems/economy-architecture.md 섹션 3.10.3
```

- **MCP 호출**: 1회

#### F-6-04: AchievementManager, QuestManager 연동 (선택적)

```
modify_script
  path: "Assets/_Project/Scripts/Core/AchievementManager.cs"
  action: OnEnable()에 FishingEvents.OnFishCaught 구독 추가 (업적 조건 평가)
  // -> see docs/systems/achievement-system.md for 업적 목록

modify_script
  path: "Assets/_Project/Scripts/Core/QuestManager.cs"
  action: OnEnable()에 FishingEvents.OnFishCaught 구독 추가 (퀘스트 진행 조건)
  // -> see docs/systems/quest-system.md for 퀘스트 목록
```

- **MCP 호출**: 2회

[OPEN] AchievementManager와 QuestManager의 낚시 관련 업적/퀘스트 조건이 아직 미정의. 구독 코드는 추가하되, 실제 조건 평가 로직은 업적/퀘스트 콘텐츠 확정 후 구현.

**F-6 합계**: 1 + 1 + 1 + 2 = **5회**

---

### F-7: 통합 테스트 시퀀스

**목적**: 낚시 전체 플로우(캐스팅 -> 대기 -> 입질 -> 미니게임 -> 성공/실패)를 Play Mode에서 검증한다.

**전제**: F-1 ~ F-6 모두 완료.

#### F-7-01: FishingManager 초기화 검증

```
enter_play_mode

get_console_logs
  filter: "FishingManager"
  // "FishingManager: Initialized with 15 fish types, config loaded" 로그 확인
  // "FishingManager: 3 fishing points registered" 로그 확인

exit_play_mode
```

- **MCP 호출**: 3회
- **검증**: FishingManager 싱글턴이 정상 초기화, _fishDataRegistry(15종)와 _fishingConfig가 null이 아닌지, _fishingPoints(3개)가 등록되었는지 확인

#### F-7-02: 낚시 시작 (Idle -> Casting -> Waiting)

```
enter_play_mode

// Zone F 해금 시뮬레이션
execute_method
  target: "FarmZoneManager"
  method: "TryUnlockZone"
  args: ["zone_pond"]

// 낚시 시작
execute_method
  target: "FishingManager"
  method: "TryStartFishing"
  args: [FishingPoint_01 참조]

get_console_logs
  filter: "FishingManager"
  // "FishingManager: State changed Idle -> Casting" 로그 확인
  // "FishingManager: State changed Casting -> Waiting" 로그 확인 (castDuration 후)

exit_play_mode
```

- **MCP 호출**: 5회
- **검증**: 상태 전환 Idle -> Casting -> Waiting이 정상 진행, FishingPoint_01의 IsOccupied가 true인지 확인

#### F-7-03: 입질 및 미니게임 (Waiting -> Biting -> Reeling)

```
enter_play_mode

// 테스트 셋업: Zone F 해금 + 낚시 시작
execute_method  target: "FarmZoneManager"  method: "TryUnlockZone"  args: ["zone_pond"]
execute_method  target: "FishingManager"  method: "TryStartFishing"  args: [FishingPoint_01 참조]

// Biting 상태 강제 전환 (테스트용 -- 대기 시간 스킵)
execute_method
  target: "FishingManager"
  method: "ForceBiteState"
  // [OPEN] 테스트용 ForceBiteState() 메서드 필요, #if UNITY_EDITOR 블록

// 플레이어 입력 시뮬레이션 (Biting -> Reeling)
execute_method
  target: "FishingManager"
  method: "OnMinigameInput"

get_console_logs
  filter: "FishingManager"
  // "FishingManager: State changed Waiting -> Biting" 로그 확인
  // "FishingManager: Fish selected: fish_xxx (Rarity)" 로그 확인
  // "FishingManager: State changed Biting -> Reeling" 로그 확인

exit_play_mode
```

- **MCP 호출**: 7회
- **검증**: Biting 상태에서 입력 시 Reeling 전환, _activeFish가 null이 아닌지, FishingMinigame이 활성화되었는지 확인

[OPEN] ForceBiteState() 테스트용 메서드를 FishingManager에 `#if UNITY_EDITOR` 블록으로 포함할지, 별도 테스트 유틸리티로 분리할지 결정 필요.

#### F-7-04: 미니게임 성공 흐름 (Reeling -> Success)

```
enter_play_mode

// 테스트 셋업 (Zone F 해금 + 낚시 시작 + 강제 Reeling 진입)
execute_method  target: "FarmZoneManager"  method: "TryUnlockZone"  args: ["zone_pond"]
execute_method  target: "FishingManager"  method: "TryStartFishing"  args: [FishingPoint_01 참조]
execute_method  target: "FishingManager"  method: "ForceBiteState"
execute_method  target: "FishingManager"  method: "OnMinigameInput"

// 미니게임 성공 강제 (테스트용)
execute_method
  target: "FishingManager"
  method: "ForceMinigameResult"
  args: [1]   // MinigameResult.Success

get_console_logs
  filter: "FishingManager"
  // "FishingManager: State changed Reeling -> Success" 로그 확인
  // "FishingManager: Fish caught! fish_xxx, Quality=Normal" 로그 확인
  // "FishingManager: XP granted: N (FishingCatch)" 로그 확인
  // "FishingManager: Inventory AddItem: fish_xxx x1" 로그 확인

exit_play_mode
```

- **MCP 호출**: 8회
- **검증**: 인벤토리에 물고기 추가, XP 부여(XPSource.FishingCatch), FishingEvents.OnFishCaught 이벤트 발행, 통계(totalCaught++) 갱신 확인

#### F-7-05: 세이브/로드 무결성

```
enter_play_mode

// 셋업: 낚시 2회 성공 시뮬레이션
execute_method  target: "FarmZoneManager"  method: "TryUnlockZone"  args: ["zone_pond"]

// 1회차 성공
execute_method  target: "FishingManager"  method: "SimulateFishingSuccess"  args: ["fish_carp"]
// 2회차 성공
execute_method  target: "FishingManager"  method: "SimulateFishingSuccess"  args: ["fish_trout"]

// 저장
execute_method  target: "SaveManager"  method: "Save"

// 통계 초기화 (시뮬레이션)
execute_method  target: "FishingManager"  method: "ResetStats"

// 로드
execute_method  target: "SaveManager"  method: "Load"

get_console_logs
  filter: "FishingManager"
  // "FishingManager: Loaded fishing stats -- totalCaught=2, totalCasts=2" 로그 확인

exit_play_mode
```

- **MCP 호출**: 9회
- **검증**: 로드 후 FishingStats의 totalCaught=2, caughtByFishId에 fish_carp:1, fish_trout:1이 존재하는지 확인

[OPEN] SimulateFishingSuccess()와 ResetStats()는 테스트용 메서드. `#if UNITY_EDITOR` 블록으로 빌드 제외 권장.

**F-7 합계**: 3 + 5 + 7 + 8 + 9 = **~32회** (최소 추정치)

---

## 5. 태스크 의존 관계 다이어그램

```
F-1 (스크립트 생성)
  |
  ├── F-2 (FishData SO 에셋 15종) ──┐
  |                                  |
  ├── F-3 (FishingConfig SO 에셋) ──┤
  |                                  |
  └── F-5 (기존 시스템 확장) ────────┤
                                     |
                       F-4 (씬 배치) ←┘
                         |
                         ├── F-6 (연동 설정)
                         |
                         └── F-7 (통합 테스트) ← F-6 완료 후
```

**병렬 실행 가능 그룹**:
- F-2, F-3, F-5는 F-1 완료 후 병렬 실행 가능
- F-4는 F-2, F-3 완료 후 실행 (SO 에셋 참조 연결 필요)
- F-6은 F-4, F-5 완료 후 실행
- F-7은 모든 태스크 완료 후에만 실행

---

## Cross-references

| 문서 | 관련 내용 |
|------|----------|
| `docs/systems/fishing-architecture.md` (ARC-026) | 전체 아키텍처 설계, 클래스 다이어그램, 코드 예시, MCP Phase 요약 |
| `docs/systems/fishing-system.md` (DES-013) | 어종 15종, 미니게임 규칙, 희귀도 체계, 판매가 canonical |
| `docs/systems/farm-expansion.md` (DES-012) | Zone F(연못) 구조, 해금 비용, FishingPoint 배치 좌표 |
| `docs/systems/farm-expansion-architecture.md` (ARC-023) | FarmZoneManager, ZoneType.Pond |
| `docs/systems/progression-architecture.md` | XPSource.FishingCatch, GetExpForSource() 확장 |
| `docs/systems/economy-architecture.md` | HarvestOrigin.Fishing, GetGreenhouseMultiplier() 확장 |
| `docs/systems/save-load-architecture.md` (ARC-011) | GameSaveData 확장, ISaveable, SaveLoadOrder 52 |
| `docs/pipeline/data-pipeline.md` | DataRegistry, SO 에셋 로드 전략 |
| `docs/systems/inventory-architecture.md` | InventoryManager.TryAddItem(), ItemType.Fish |
| `docs/mcp/inventory-tasks.md` (ARC-013) | InventoryManager 의존, ItemType.Fish 추가 (FIX-053) |
| `docs/systems/project-structure.md` | 네임스페이스(SeedMind.Fishing), 폴더 구조, asmdef 규칙 |
| `docs/balance/progression-curve.md` | 낚시 XP 값 canonical |
| `docs/systems/time-season.md` | SeasonFlag, 시간대 정의, 날씨 종류 |
| `docs/mcp/livestock-tasks.md` (ARC-024) | 동일 형식 참고 (태스크 구조, 검증 패턴) |

---

## Open Questions

1. [OPEN] **미니게임 방식 불일치**: fishing-system.md 섹션 3이 ExcitementGauge 방식을 확정했으나, 이전 버전의 Oscillating Bar 방식 잔여 기술이 남아 있을 수 있다. MCP 구현 시 ExcitementGauge 방식 기준으로 통일한다. (-> see `docs/systems/fishing-architecture.md` [OPEN] FIX-055)

2. [OPEN] **SO 배열 참조 설정**: MCP `set_property`로 ScriptableObject 배열(FishData[])을 직접 할당할 수 있는지 불확실. 미지원 시 DataRegistry 패턴으로 런타임 동적 로드 전환 필요.

3. [OPEN] **float[] 필드 MCP 설정**: FishData.timeWeights(float[5])와 FishingConfig.qualityThresholds(float[4])를 MCP `set_property`로 설정 가능한지 검증 필요. 미지원 시 코드 fallback 초기화.

4. [OPEN] **테스트용 메서드**: ForceBiteState(), ForceMinigameResult(), SimulateFishingSuccess(), ResetStats() 등 테스트 전용 메서드를 FishingManager에 `#if UNITY_EDITOR` 블록으로 포함할지, 별도 테스트 유틸리티 클래스로 분리할지 결정 필요.

5. [OPEN] **겨울 낚시 허용 여부**: time-season.md에서 겨울 "낚시/채집 불가"로 명시되어 있으나, fishing-system.md에서 겨울 어종 2종(빙어, 얼음 빙어왕)을 정의하며 얼음 낚시를 제안. 해결 전까지 겨울 어종 SO는 생성하되 seasonAvailability에서 Winter 비트를 포함/제외 여부는 보류.

6. [OPEN] **FishingPoint 일일 사용 제한**: 무제한 vs 포인트당 일일 제한 미정. FishingPoint._dailyUseCount 필드는 생성하되, 제한 로직은 확정 후 활성화.

---

## Risks

1. [RISK] **총 MCP 호출 수**: F-1~F-7 합산 약 278회 (실행 중 변동 가능). F-2(SO 에셋 생성)의 ~196회가 대부분을 차지. Editor 스크립트 일괄 생성으로 약 200회 절감 가능. **순수 MCP 실행 시 시간 비용이 크므로, F-2는 Editor 스크립트 우회를 강력 권장.**

2. [RISK] **asmdef 의존성 누락**: SeedMind.Fishing asmdef가 SeedMind.Core, SeedMind.Farm, SeedMind.Level, SeedMind.Player를 참조해야 한다. 누락 시 FishingManager.cs 컴파일 실패로 이후 모든 태스크가 블록된다. F-1 asmdef 생성 단계에서 반드시 검증.

3. [RISK] **미니게임 프레임 의존성**: FishingMinigame.Update()가 매 프레임 호출되므로, deltaTime 기반 보정이 없으면 프레임레이트에 따라 난이도가 달라질 수 있다. 모든 시간 계산에 deltaTime을 반드시 적용해야 한다. (-> see `docs/systems/fishing-architecture.md` Risks 항목 5)

4. [RISK] **SaveLoadOrder 52 충돌 가능성**: PlayerController(50)와 InventoryManager(55) 사이의 좁은 간격에 배치. 향후 다른 시스템이 51~54 범위를 사용하면 충돌 가능. (-> see `docs/systems/fishing-architecture.md` Risks 항목 3)

5. [RISK] **FIX-049~053 C# 반영 시점**: 문서 레벨에서 RESOLVED이나, 실제 C# 파일 수정은 F-5에서 수행. 다른 태스크(ARC-024 등)와의 enum 값 번호 충돌 가능. HarvestOrigin.Fishing=3, XPSource.FishingCatch 등의 번호가 겹치지 않도록 주의.

6. [RISK] **DES-013 basePrice 미확정**: expReward는 FIX-064에서 progression-curve.md 섹션 1.2.7에 canonical 등록 완료. basePrice는 fishing-system.md에서 확정값 기재 여부 확인 필요. MCP 실행 시 fishing-system.md 섹션 4.2의 값을 사용하되, 추후 밸런스 조정 시 SO 필드 재설정 필요. (-> see `docs/balance/progression-curve.md` 섹션 1.2.7.1 for expReward canonical)

---

*이 문서는 Claude Code가 기존 MCP 태스크 문서의 패턴과 규칙을 준수하여 자율적으로 작성했습니다.*
