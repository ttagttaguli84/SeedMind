# 에너지 시스템 MCP 태스크 시퀀스 (ARC-047)

> 작성: Claude Code (Sonnet 4.6) | 2026-04-09 | 문서 ID: ARC-047 | Phase 1

---

## Context

이 문서는 에너지 시스템의 Unity MCP 구현 태스크 시퀀스를 정의한다. `docs/systems/energy-architecture.md`(ARC-044) Part II에서 요약된 Step 1~7 개요를 독립 문서로 분리하여, 스크립트 생성 → SO 에셋 생성 → 씬 배치 → UI 연동 → 시스템 통합 → 수면/기절 연동 → 세이브/로드 검증까지의 전체 MCP 호출 시퀀스를 상세히 기술한다.

**상위 설계 문서**: `docs/systems/energy-architecture.md` (ARC-044)  
**에너지 수치 canonical 출처**: `docs/systems/energy-system.md` (DES-024)  
**패턴 참조**: `docs/mcp/decoration-tasks.md` (ARC-046), `docs/mcp/gathering-tasks.md` (ARC-032)

---

## 1. 개요

### 목적

이 문서는 `docs/systems/energy-architecture.md`(ARC-044) Part II에서 요약된 MCP 구현 계획(Step 1~7)을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 에너지 시스템의 데이터 레이어(SO 에셋), 시스템 레이어(스크립트), 씬 배치, UI 연동, 통합 검증을 완성한다.

### 의존성

```
에너지 시스템 MCP 태스크 의존 관계:
├── SeedMind.Core        (ISaveable, Singleton 기반 클래스)
├── SeedMind.Save        (SaveManager, ISaveable 인터페이스)
├── SeedMind.Player      (PlayerController — EnergyDepleted 구독)
├── SeedMind.Farm        (FarmingSystem — TryConsume 연동)
├── SeedMind.Fishing     (FishingManager — TryConsume 연동)
├── SeedMind.Gathering   (GatheringManager — TryConsume 연동)
├── SeedMind.Economy     (EconomyManager — PassOut 골드 페널티)
└── SeedMind.UI          (HUD Canvas — EnergyBarUI 배치)
```

(→ see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 완료된 태스크 의존성

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조, SCN_Farm 기본 계층 (Managers, Environment, UI) |
| ARC-003 | `docs/mcp/farming-tasks.md` | Phase A~C 전체 | FarmingSystem MonoBehaviour 컴파일 완료 |
| ARC-012 | `docs/mcp/save-load-tasks.md` | Phase A 이상 | SaveManager, ISaveable, GameSaveData, PlayerSaveData |
| ARC-013 | `docs/mcp/inventory-tasks.md` | Phase A 이상 | InventoryManager, 아이템 사용 이벤트 구조 |
| ARC-028 | `docs/mcp/fishing-tasks.md` | Phase A 이상 | FishingManager MonoBehaviour 컴파일 완료 |
| ARC-032 | `docs/mcp/gathering-tasks.md` | Phase A 이상 | GatheringManager MonoBehaviour 컴파일 완료 |
| ARC-018 | `docs/mcp/ui-tasks.md` | HUD Phase 이상 | Canvas_Overlay, HUDController, 좌측 하단 HUD 슬롯 |

### 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 | 비고 |
|--------------|------|------|
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B | `GameManager` GO가 이미 배치됨 |
| `Canvas_Overlay` (UI 루트) | ARC-002 Phase B | EnergyBarUI 프리팹을 그 하위에 배치 |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A | |
| `Assets/_Project/Scripts/Player/` 폴더 | ARC-002 Phase A | |
| `SaveManager` (씬 오브젝트) | ARC-008 | ISaveable.Register() 대상 |
| `ISaveable` 인터페이스 | ARC-008 | EnergyManager가 구현 |
| `PlayerSaveData` 클래스 | ARC-008 | currentEnergy / maxEnergy 필드는 이미 정의됨 (→ see docs/pipeline/data-pipeline.md Part II 섹션 2.2) |
| `FarmingSystem` (씬 오브젝트) | ARC-003 | E-E에서 _energyManager 참조 연결만 추가 |
| `FishingManager` (씬 오브젝트) | ARC-028 | E-E에서 _energyManager 참조 연결만 추가 |
| `GatheringManager` (씬 오브젝트) | ARC-032 | E-E에서 _energyManager 참조 연결만 추가 |
| `PlayerController` (씬 오브젝트) | ARC-003 이상 | E-C에서 EnergyEvents 구독 설정 |
| `EconomyManager` (씬 오브젝트) | 이전 Economy ARC | E-F에서 OnPassOut 구독 확인 |

### 총 MCP 호출 예상 수

| 태스크 그룹 | 설명 | 예상 호출 수 | 복잡도 |
|-------------|------|:------------:|:------:|
| E-A | 스크립트 생성 (5개 파일) | ~9회 | 저 |
| E-B | SO 에셋 생성 (EnergyConfig.asset 1종 + 필드 설정) | ~6회 | 저 |
| E-C | EnergyManager 씬 배치 + EnergyConfig 연결 + PlayerController 연동 | ~8회 | 중 |
| E-D | EnergyBarUI 생성 및 HUD Canvas 배치 + 참조 연결 | ~15회 | 중 |
| E-E | FarmingSystem / FishingManager / GatheringManager 연동 | ~6회 | 중 |
| E-F | 수면 회복 / 기절 연동 (TimeManager, EconomyManager) | ~5회 | 중 |
| E-G | ISaveable 등록 + 세이브/로드 검증 | ~6회 | 저 |
| **합계** | | **~55회** | |

[RISK] `EnergyConfig.asset`의 수치 필드 수는 약 25개에 달한다. 각 필드를 `set_property`로 개별 설정하면 ~25회의 추가 호출이 필요하다. Editor 스크립트(`InitEnergyConfig.cs`)를 통한 일괄 설정으로 ~25회를 ~2회로 압축 가능. 수치는 반드시 `docs/systems/energy-system.md` 섹션 9를 참조하여 입력해야 한다 (PATTERN-007).

---

## 2. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋 폴더 생성 | E-A, E-B |
| `create_script` | C# 스크립트 파일 생성 | E-A, E-D |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | E-B |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | E-B, E-C, E-D, E-E |
| `create_object` | 빈 GameObject 생성 | E-C, E-D |
| `add_component` | MonoBehaviour 컴포넌트 부착 | E-C, E-D |
| `set_parent` | 오브젝트 부모 설정 | E-C, E-D |
| `save_scene` | 씬 저장 | E-C, E-D, E-F, E-G |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | E-G |
| `execute_method` | 런타임 메서드 호출 (TryConsume, ProcessDayEnd 테스트) | E-G |
| `get_console_logs` | 콘솔 로그 확인 | E-C, E-G |
| `execute_menu_item` | 컴파일 대기 | E-A, E-D |

[RISK] `create_scriptable_object` 도구의 가용 여부 및 파라미터 형식 사전 검증 필요. SO 인스턴스 생성이 MCP에서 미지원인 경우, Editor 스크립트를 통한 우회 필요. (→ see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)

---

## 3. 필요 C# 스크립트 목록

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 태스크 순서대로 작성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/Player/Data/EnergyConfig.cs` | `EnergyConfig` (SO) | `SeedMind.Player.Data` | E-A |
| S-02 | `Scripts/Player/EnergySource.cs` | `EnergySource` (enum) | `SeedMind.Player` | E-A |
| S-03 | `Scripts/Player/SleepType.cs` | `SleepType` (enum) | `SeedMind.Player` | E-A |
| S-04 | `Scripts/Player/EnergyEvents.cs` | `EnergyEvents` (static class) | `SeedMind.Player` | E-A |
| S-05 | `Scripts/Player/EnergyManager.cs` | `EnergyManager` (MonoBehaviour, Singleton, ISaveable) | `SeedMind.Player` | E-A |
| S-06 | `Scripts/UI/EnergyBarUI.cs` | `EnergyBarUI` (MonoBehaviour) | `SeedMind.UI` | E-D |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: S-01~S-04 → S-05 → (E-C 완료 후) S-06. 각 그룹 사이에 Unity 컴파일 대기(`execute_menu_item`)가 필요하다.

---

## 4. 태스크 목록

---

### E-A: 스크립트 생성

**목적**: 에너지 시스템의 데이터 정의 클래스와 핵심 MonoBehaviour를 생성한다. enum 2종, SO 1종, static class 1종, MonoBehaviour 1종 총 5개 파일.

**전제 조건**:
- ARC-002 완료 (`Assets/_Project/Scripts/Player/` 폴더 존재)
- `SeedMind.Core` 네임스페이스의 `ISaveable` 컴파일 완료
- `SeedMind.Save` 네임스페이스의 `SaveManager` 컴파일 완료

**예상 MCP 호출**: ~9회 (1 create_folder + 7 create_script + 1 execute_menu_item)

#### E-A-01: 스크립트 폴더 생성

```
create_folder
  path: "Assets/_Project/Scripts/Player/Data"
```

- **MCP 호출**: 1회
- `Assets/_Project/Scripts/Player/` 폴더는 ARC-002에서 생성됨 — 중복 생성 금지

#### E-A-02: enum 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/Player/EnergySource.cs"
  namespace: "SeedMind.Player"
  // enum 값: FarmingTool(0), FishingCast(1), FishingFail(2),
  //          GatheringTool(3), PassOut(4),
  //          SleepRecovery(10), FoodBasic(11), FoodNormal(12),
  //          FoodPremium(13), FoodLuxury(14), RestArea(15), HotSpring(16), EventReward(17)
  // -> see docs/systems/energy-architecture.md 섹션 2.4

create_script
  path: "Assets/_Project/Scripts/Player/SleepType.cs"
  namespace: "SeedMind.Player"
  // enum 값: EarlySleep(0), NormalSleep(1), PassOut(2)
  // -> see docs/systems/energy-architecture.md 섹션 2.3
```

- **MCP 호출**: 2회

#### E-A-03: EnergyConfig ScriptableObject 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/Player/Data/EnergyConfig.cs"
  namespace: "SeedMind.Player.Data"
  // ScriptableObject 상속
  // [CreateAssetMenu(fileName = "EnergyConfig", menuName = "SeedMind/Energy/EnergyConfig")]
  // 필드 그룹 (수치는 Inspector에서 입력 — 직접 기재 금지):
  //   [Header("기본 최대 에너지")] baseMaxEnergy, startOfDayEnergy
  //   [Header("레벨업 보너스")] levelUpBonusLevels(int[]), energyBonusPerThreshold
  //   [Header("경고/고갈 임계값")] energyWarningThreshold, energyDepletionSpeedPenalty
  //   [Header("수면 회복")] sleepFullRecovery, sleepEarlyBonusEnergy,
  //     sleepEarlyBonusMultiplier, sleepEarlyMorningDiscount, passOutEnergyRecovery
  //   [Header("기절 골드 페널티")] passOutGoldPenaltyRate, passOutGoldPenaltyCap
  //   [Header("날씨 배수")] heavyRainEnergyMult, stormEnergyMult, snowEnergyMult, blizzardEnergyMult
  //   [Header("시간대 배수")] nightEnergyMultiplier
  //   [Header("경작 도구 에너지 소모")] hoeEnergyBasic, hoeEnergyReinforced, hoeEnergyLegendary,
  //     waterEnergyBasic, waterEnergyReinforced, waterEnergyLegendary,
  //     sickleEnergyBasic, sickleEnergyReinforced, sickleEnergyLegendary
  //   [Header("낚시 에너지 소모")] castEnergy, castEnergyHighLevel, fishingHighLevelThreshold, failExtraEnergy
  //   [Header("채집 에너지 소모")] toolGatherEnergy
  //   [Header("음식 임시 최대치")] tempMaxEnergyBonusCap
  //   헬퍼 메서드: GetHoeEnergy(ToolGrade), GetWeatherMultiplier(WeatherType)
  // 모든 수치 → see docs/systems/energy-system.md 섹션 9
  // -> see docs/systems/energy-architecture.md 섹션 3.1
```

- **MCP 호출**: 1회

#### E-A-04: EnergyEvents 정적 이벤트 허브 생성

```
create_script
  path: "Assets/_Project/Scripts/Player/EnergyEvents.cs"
  namespace: "SeedMind.Player"
  // static class
  // 이벤트 (Action<>):
  //   OnEnergyChanged(int current, int max)
  //   OnEnergyConsumed(EnergySource source, int amount)
  //   OnEnergyInsufficient()
  //   OnEnergyWarning(int currentEnergy)
  //   OnEnergyDepleted()
  //   OnPassOut(int currentGold, int goldLost)
  //   OnWellRestedActivated()
  //   OnWellRestedDeactivated()
  //   OnMaxEnergyIncreased(int oldMax, int newMax)
  // Raise 메서드: RaiseEnergyChanged, RaiseEnergyConsumed,
  //              RaiseEnergyInsufficient, RaiseEnergyWarning, RaiseEnergyDepleted,
  //              RaisePassOut, RaiseWellRestedActivated, RaiseWellRestedDeactivated,
  //              RaiseMaxEnergyIncreased
  // -> see docs/systems/energy-architecture.md 섹션 6.1
```

- **MCP 호출**: 1회

#### E-A-05: EnergyManager MonoBehaviour 생성

```
create_script
  path: "Assets/_Project/Scripts/Player/EnergyManager.cs"
  namespace: "SeedMind.Player"
  // MonoBehaviour, Singleton, ISaveable 구현
  // SaveLoadOrder = 51
  // [SerializeField] _config: EnergyConfig
  // private 상태: _currentEnergy(int), _maxEnergy(int), _tempMaxBonusToday(int),
  //   _isWellRested(bool), _currentWeatherMult(float), _currentTimeMult(float)
  // 읽기 전용 프로퍼티: CurrentEnergy, MaxEnergy, TempMaxBonus, IsWellRested, IsDepleted,
  //   EnergyRatio, IsWarning
  // 소모 API: CanConsume(int), TryConsume(int, EnergySource), ConsumeRaw(int)
  // 회복 API: RecoverEnergy(int, EnergySource), AddTempMaxBonus(int), SetWellRested(bool)
  // 배수 갱신 API: SetWeatherMultiplier(float), SetTimeMultiplier(float)
  // 레벨업: ApplyLevelUpBonus(int newLevel)
  // 일 전환: ProcessDayEnd(SleepType, int energyAtSleep), ResetDayState()
  // 초기화/세이브: Initialize(EnergyConfig), GetSaveData(), LoadSaveData(object)
  // OnEnable 구독: TimeManager.OnDayChanged, TimeManager.OnTimeSlotChanged,
  //               WeatherSystem.OnWeatherChanged, ProgressionManager.OnLevelUp
  // -> see docs/systems/energy-architecture.md 섹션 2.1
```

- **MCP 호출**: 1회

#### E-A-06: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- 모든 E-A 스크립트 컴파일 완료 확인 후 E-B 진행
- **MCP 호출**: 1회

---

### E-B: SO 에셋 생성 (EnergyConfig.asset)

**목적**: `EnergyConfig` ScriptableObject 에셋 인스턴스를 생성하고, 에너지 수치 필드를 canonical 문서 기준으로 설정한다.

**전제 조건**:
- E-A 완료 (`EnergyConfig.cs` 컴파일 완료)
- `Assets/_Project/Data/` 폴더 존재 (ARC-002)

**예상 MCP 호출**: ~6회 (1 create_folder + 1 create_scriptable_object + 최대 4회 set_property 또는 Editor 스크립트 방식)

[RISK] `EnergyConfig.asset`의 수치 필드는 약 25개이다. MCP `set_property` 개별 호출 방식은 ~25회 추가 호출을 발생시키므로, Editor 스크립트(`InitEnergyConfig.cs`) 일괄 설정 방식을 강력히 권장한다. 수치는 반드시 `docs/systems/energy-system.md` 섹션 9 참조 — 임의 기재 금지 (PATTERN-007).

#### E-B-01: 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Energy"
```

- **MCP 호출**: 1회

#### E-B-02: EnergyConfig SO 에셋 생성

```
create_scriptable_object
  type: "SeedMind.Player.Data.EnergyConfig"
  path: "Assets/_Project/Data/Energy/EnergyConfig.asset"
```

- **MCP 호출**: 1회

#### E-B-03: SO 필드 설정 (참조 방식)

에셋의 모든 수치 필드는 **Inspector에서 직접 입력**하거나, 아래 Editor 스크립트 방식으로 일괄 설정한다. 수치를 이 문서에 직접 기재하는 것은 PATTERN-007 위반이다.

```
// [권장] Editor 스크립트 방식
create_script
  path: "Assets/_Project/Editor/InitEnergyConfig.cs"
  // [MenuItem("SeedMind/Init/EnergyConfig")] 메서드 포함
  // 모든 수치: -> see docs/systems/energy-system.md 섹션 9

execute_menu_item
  menu: "SeedMind/Init/EnergyConfig"
```

- **MCP 호출**: 2회 (스크립트 생성 + 메뉴 실행)

**수치 입력 참조 테이블**:

| SO 필드 그룹 | 수치 참조 |
|------------|----------|
| 기본 최대 에너지 / 시작 에너지 | (→ see docs/systems/energy-system.md 섹션 1.1) |
| 레벨업 보너스 레벨 배열, 보너스량 | (→ see docs/systems/energy-system.md 섹션 1.2) |
| 경고/고갈 임계값, 이동 속도 패널티 | (→ see docs/systems/energy-system.md 섹션 6.1) |
| 수면 회복 계수 (조기/일반/기절) | (→ see docs/systems/energy-system.md 섹션 5.1) |
| 기절 골드 페널티 비율 및 상한 | (→ see docs/systems/energy-system.md 섹션 6.2) |
| 날씨 배수 (폭우/폭풍/눈/눈보라) | (→ see docs/systems/energy-system.md 섹션 3) |
| 시간대 배수 (야간) | (→ see docs/systems/energy-system.md 섹션 4) |
| 경작 도구 에너지 소모 (등급별) | (→ see docs/systems/energy-system.md 섹션 2.1) |
| 낚시 에너지 소모 / 실패 추가 소모 | (→ see docs/systems/energy-system.md 섹션 2.2) |
| 채집 도구 에너지 소모 | (→ see docs/systems/energy-system.md 섹션 2.3) |
| 음식 임시 최대치 상한 | (→ see docs/systems/energy-system.md 섹션 5.2) |

#### E-B-04: 씬 저장 (EnergyConfig 에셋 확인 후)

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회 (에셋 저장은 create_scriptable_object 시 자동)

---

### E-C: EnergyManager 씬 배치 + EnergyConfig 연결

**목적**: `EnergyManager` MonoBehaviour를 씬에 배치하고, `EnergyConfig.asset`을 Inspector 참조로 연결한다. 또한 PlayerController가 `EnergyEvents.OnEnergyDepleted`를 구독하여 이동 속도 패널티를 적용하도록 설정을 확인한다.

**전제 조건**:
- E-A 완료 (EnergyManager.cs 컴파일 완료)
- E-B 완료 (EnergyConfig.asset 생성 완료)
- ARC-002 완료 (`--- MANAGERS ---` 씬 계층 존재)

**예상 MCP 호출**: ~8회

#### E-C-01: EnergyManager GameObject 생성

```
create_object
  name: "EnergyManager"
  scene: "SCN_Farm"

set_parent
  object: "EnergyManager"
  parent: "--- MANAGERS ---"

add_component
  object: "EnergyManager"
  component: "SeedMind.Player.EnergyManager"
```

- **MCP 호출**: 3회

#### E-C-02: EnergyConfig 에셋 연결

```
set_property
  object: "EnergyManager"
  component: "SeedMind.Player.EnergyManager"
  property: "_config"
  value: "Assets/_Project/Data/Energy/EnergyConfig.asset"
```

- **MCP 호출**: 1회

#### E-C-03: SaveLoadOrder 설정

```
set_property
  object: "EnergyManager"
  component: "SeedMind.Player.EnergyManager"
  property: "saveLoadOrder"
  value: 51
  // SaveLoadOrder = 51 -> see docs/systems/save-load-architecture.md SaveLoadOrder 할당표
```

- **MCP 호출**: 1회

#### E-C-04: PlayerController 연동 확인

PlayerController의 `OnEnable()`에서 `EnergyEvents.OnEnergyDepleted`를 구독하고, 이동 속도 패널티를 적용하는 로직이 포함되어 있는지 확인한다.

```
get_console_logs
  // Play Mode 진입 후 에너지 고갈 시 PlayerController 패널티 적용 로그 확인
  // 이동 속도 패널티 수치 → see docs/systems/energy-system.md 섹션 6.1
```

- **MCP 호출**: 1회 (Play Mode 테스트 시)

[RISK] PlayerController가 기존 코드에 EnergyEvents 구독이 없는 경우, PlayerController.cs의 `OnEnable()` 메서드에 구독 코드 추가가 필요하다. 이는 farming-tasks.md(ARC-003) 범위 밖의 수정이므로 FIX 작업으로 처리해야 할 수 있다.

#### E-C-05: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

**E-C 완료 체크리스트**:
- [ ] `--- MANAGERS ---` 하위에 `EnergyManager` GO가 존재한다
- [ ] `EnergyManager` 컴포넌트의 `_config` 필드에 `EnergyConfig.asset`이 연결되어 있다
- [ ] `saveLoadOrder`가 51로 설정되어 있다
- [ ] Play Mode 진입 시 컴파일 에러가 없다

---

### E-D: EnergyBarUI 생성 및 HUD Canvas 배치

**목적**: `EnergyBarUI` MonoBehaviour 스크립트를 생성하고, HUD Canvas 좌측 하단에 EnergyBar UI 프리팹을 배치한 후 Fill Image, Label Text, WellRested Icon 참조를 연결한다.

**전제 조건**:
- E-C 완료 (EnergyManager 씬 배치 완료, EnergyEvents 컴파일 완료)
- ARC-018 완료 (`Canvas_Overlay` 및 HUD 좌측 하단 슬롯 존재)

**예상 MCP 호출**: ~15회

#### E-D-01: EnergyBarUI 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/UI/EnergyBarUI.cs"
  namespace: "SeedMind.UI"
  // MonoBehaviour
  // [SerializeField] 참조:
  //   _fillImage(Image), _labelText(TextMeshProUGUI), _wellRestedIcon(GameObject),
  //   _tempMaxExtension(Image), _pulseAnimation(Animator), _floatingTextPrefab(GameObject)
  // private 상태: _isWarning(bool)
  // OnEnable: EnergyEvents 이벤트 구독 (OnEnergyChanged, OnEnergyWarning,
  //   OnEnergyDepleted, OnWellRestedActivated, OnWellRestedDeactivated,
  //   OnEnergyConsumed, OnMaxEnergyIncreased)
  // OnDisable: 전체 구독 해제
  // 핸들러: HandleEnergyChanged, HandleEnergyWarning, HandleEnergyDepleted,
  //         HandleWellRestedOn, HandleWellRestedOff,
  //         HandleEnergyConsumed, HandleMaxEnergyIncreased
  // -> see docs/systems/energy-architecture.md 섹션 7.1
```

- **MCP 호출**: 1회

#### E-D-02: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

#### E-D-03: EnergyBar UI GameObject 생성

```
create_object
  name: "HUD_EnergyBar"
  scene: "SCN_Farm"

set_parent
  object: "HUD_EnergyBar"
  parent: "Canvas_Overlay/HUD_BottomLeft"
  // HUD_BottomLeft 슬롯이 없는 경우 Canvas_Overlay 직접 하위에 배치

add_component
  object: "HUD_EnergyBar"
  component: "SeedMind.UI.EnergyBarUI"
```

- **MCP 호출**: 3회

#### E-D-04: UI 자식 오브젝트 생성

```
create_object
  name: "FillImage"
  scene: "SCN_Farm"
  // Image 컴포넌트 추가, fill 방식 설정
  // 시각 규격 → see docs/systems/ui-system.md 섹션 [E]

set_parent
  object: "FillImage"
  parent: "HUD_EnergyBar"

create_object
  name: "LabelText"
  scene: "SCN_Farm"
  // TextMeshProUGUI 컴포넌트 추가

set_parent
  object: "LabelText"
  parent: "HUD_EnergyBar"

create_object
  name: "WellRestedIcon"
  scene: "SCN_Farm"
  // Image 또는 Sprite 컴포넌트
  // 기본 비활성 상태(SetActive false)

set_parent
  object: "WellRestedIcon"
  parent: "HUD_EnergyBar"

create_object
  name: "TempMaxExtension"
  scene: "SCN_Farm"
  // Image 컴포넌트 추가 — 임시 maxEnergy 초과 시 황금색 연장 바
  // 기본 비활성 상태(SetActive false)
  // 시각 규격 → see docs/systems/energy-architecture.md 섹션 7.2

set_parent
  object: "TempMaxExtension"
  parent: "HUD_EnergyBar"

create_object
  name: "PulseAnimation"
  scene: "SCN_Farm"
  // Animator 컴포넌트 추가 — 경고 상태 펄스 애니메이터
  // 기본 비활성 상태(SetActive false)

set_parent
  object: "PulseAnimation"
  parent: "HUD_EnergyBar"

create_object
  name: "FloatingTextPrefab"
  scene: "SCN_Farm"
  // TextMeshProUGUI 컴포넌트 추가 — 소모 수치 플로팅 텍스트 프리팹
  // [RISK] 프리팹 에셋으로 관리하는 경우 씬 내 오브젝트 대신 prefab 참조 방식 필요

set_parent
  object: "FloatingTextPrefab"
  parent: "HUD_EnergyBar"
```

- **MCP 호출**: 12회 (create_object + set_parent × 6쌍)

[RISK] UI 세부 레이아웃 설정(RectTransform anchoredPosition, sizeDelta 등)은 MCP `set_property`로 설정하기 복잡하다. UI 정밀 배치는 Unity Editor에서 Inspector를 통해 수동 조정하거나, UI 설정 Editor 스크립트로 자동화하는 것을 권장한다. 시각 규격 → see `docs/systems/ui-system.md` 섹션 [E].

#### E-D-05: EnergyBarUI 참조 연결

```
set_property
  object: "HUD_EnergyBar"
  component: "SeedMind.UI.EnergyBarUI"
  property: "_fillImage"
  value: [FillImage 오브젝트 참조]

set_property
  object: "HUD_EnergyBar"
  component: "SeedMind.UI.EnergyBarUI"
  property: "_labelText"
  value: [LabelText 오브젝트 참조]

set_property
  object: "HUD_EnergyBar"
  component: "SeedMind.UI.EnergyBarUI"
  property: "_wellRestedIcon"
  value: [WellRestedIcon 오브젝트 참조]

set_property
  object: "HUD_EnergyBar"
  component: "SeedMind.UI.EnergyBarUI"
  property: "_tempMaxExtension"
  value: [TempMaxExtension 오브젝트 참조]

set_property
  object: "HUD_EnergyBar"
  component: "SeedMind.UI.EnergyBarUI"
  property: "_pulseAnimation"
  value: [PulseAnimation 오브젝트 참조]

set_property
  object: "HUD_EnergyBar"
  component: "SeedMind.UI.EnergyBarUI"
  property: "_floatingTextPrefab"
  value: [FloatingTextPrefab 오브젝트 참조]
```

- **MCP 호출**: 6회

#### E-D-06: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

**E-D 완료 체크리스트**:
- [ ] `Canvas_Overlay` 하위에 `HUD_EnergyBar` GO가 존재한다
- [ ] `EnergyBarUI` 컴포넌트의 6개 참조 필드(`_fillImage`, `_labelText`, `_wellRestedIcon`, `_tempMaxExtension`, `_pulseAnimation`, `_floatingTextPrefab`)가 모두 연결되어 있다
- [ ] Play Mode 진입 후 에너지 변경 시 게이지 바가 갱신된다

---

### E-E: FarmingSystem / FishingManager / GatheringManager 연동

**목적**: 각 시스템의 도구 사용/캐스팅 메서드에 `EnergyManager.TryConsume()` 호출이 포함되도록 Inspector 참조를 연결한다. 스크립트 수정 없이 Inspector `_energyManager` 필드 참조만 연결하는 것이 이상적이나, 기존 스크립트에 `_energyManager` 필드가 없는 경우 스크립트 수정이 필요하다.

**전제 조건**:
- E-C 완료 (EnergyManager 씬 배치 완료)
- ARC-003 완료 (FarmingSystem 씬 배치)
- ARC-028 완료 (FishingManager 씬 배치)
- ARC-032 완료 (GatheringManager 씬 배치)

**예상 MCP 호출**: ~6회

#### E-E-01: FarmingSystem에 EnergyManager 참조 연결

```
set_property
  object: "FarmingSystem"
  component: "SeedMind.Farm.FarmingSystem"
  property: "_energyManager"
  value: [EnergyManager 씬 오브젝트 참조]
  // 소모 시점: 도구 사용 확정 직전 TryConsume 호출
  // 소모량: EnergyConfig.GetHoeEnergy(grade) / waterEnergy / sickleEnergy
  // → see docs/systems/energy-system.md 섹션 2.1
```

- **MCP 호출**: 1회

[RISK] `FarmingSystem.cs`에 `[SerializeField] private EnergyManager _energyManager;` 필드가 없는 경우, farming-tasks.md(ARC-003) 범위 밖의 스크립트 수정이 필요하다. 이 경우 별도 FIX 태스크로 처리하고, 수정 후 Unity 재컴파일 대기(`execute_menu_item`) 필요.

#### E-E-02: FishingManager에 EnergyManager 참조 연결

```
set_property
  object: "FishingManager"
  component: "SeedMind.Fishing.FishingManager"
  property: "_energyManager"
  value: [EnergyManager 씬 오브젝트 참조]
  // 소모 시점 1: 캐스팅 확정 시 → EnergySource.FishingCast
  // 소모 시점 2: 미니게임 실패 판정 시 → EnergySource.FishingFail
  // 소모량 → see docs/systems/energy-system.md 섹션 2.2
```

- **MCP 호출**: 1회

#### E-E-03: GatheringManager에 EnergyManager 참조 연결

```
set_property
  object: "GatheringManager"
  component: "SeedMind.Gathering.GatheringManager"
  property: "_energyManager"
  value: [EnergyManager 씬 오브젝트 참조]
  // 소모 시점: 채집 도구 사용 시 → EnergySource.GatheringTool
  // 맨손 채집은 소모 0이므로 TryConsume 호출 불필요
  // 소모량 → see docs/systems/energy-system.md 섹션 2.3
```

- **MCP 호출**: 1회

#### E-E-04: 씬 저장 + Play Mode 통합 검증

```
save_scene
  scene: "SCN_Farm"

enter_play_mode

execute_method
  // FarmingSystem.TryUseTool() 또는 동등한 테스트 메서드 호출
  // 에너지 소모 로그 확인

get_console_logs
  // "[EnergyManager] TryConsume: amount=X, source=FarmingTool, remaining=Y" 형태 로그 확인

exit_play_mode
```

- **MCP 호출**: ~3회

**E-E 완료 체크리스트**:
- [ ] FarmingSystem 도구 사용 시 에너지가 감소한다
- [ ] FishingManager 캐스팅 시 에너지가 감소한다
- [ ] 에너지 부족 시 TryConsume이 false를 반환하고 OnEnergyInsufficient 이벤트가 발행된다
- [ ] GatheringManager 도구 사용 시 에너지가 감소한다
- [ ] EnergyBarUI 게이지 바가 소모에 반응하여 갱신된다

---

### E-F: 수면 회복 / 기절 연동

**목적**: TimeManager의 날 전환 이벤트에서 `EnergyManager.ProcessDayEnd()`가 호출되는지 확인하고, PassOut 이벤트 발행 시 EconomyManager가 골드 페널티를 처리하는지 검증한다.

**전제 조건**:
- E-C 완료 (EnergyManager 씬 배치 완료)
- TimeManager 씬 배치 및 OnDayChanged 이벤트 발행 구조 존재 (ARC-002 또는 time-season-tasks.md)
- EconomyManager 씬 배치 완료

**예상 MCP 호출**: ~5회

#### E-F-01: TimeManager 연동 확인

TimeManager의 `OnDayChanged` 이벤트 발행 시 EnergyManager의 `HandleDayChanged()` 핸들러가 호출되는지 확인한다. EnergyManager는 `OnEnable()`에서 `TimeManager.OnDayChanged`를 구독하도록 설계되어 있다.

```
enter_play_mode

execute_method
  // 시간 가속 또는 TimeManager.SimulateDayEnd() 테스트 메서드 호출
  // 날 전환 후 에너지 회복 확인

get_console_logs
  // "[EnergyManager] ProcessDayEnd: sleepType=NormalSleep, recovered=X" 형태 로그 확인
  // 회복 수치 → see docs/systems/energy-system.md 섹션 5.1

exit_play_mode
```

- **MCP 호출**: 3회

[RISK] TimeManager가 단순 `UnityEvent` 방식이 아닌 `static event Action` 방식으로 `OnDayChanged`를 발행하는 경우, EnergyManager의 `OnEnable()` 구독 코드와 방식이 일치해야 한다. 이벤트 방식 불일치 시 구독이 무시되어 수면 회복이 동작하지 않는다.

#### E-F-02: 기절(PassOut) EconomyManager 연동 확인

```
execute_method
  // 에너지 강제 소모 후 기절 트리거 (ConsumeRaw 또는 TimeManager 24:00 시뮬레이션)

get_console_logs
  // "[EnergyManager] PassOut: goldLost=X" 로그 확인
  // "[EconomyManager] ApplyPassOutPenalty: goldLost=X" 로그 확인
  // 골드 페널티 수치 → see docs/systems/energy-system.md 섹션 6.2
```

- **MCP 호출**: 2회 (enter/exit_play_mode는 E-F-01에서 이미 완료됨)

#### E-F-03: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회 (E-F-01~02와 합산)

**E-F 완료 체크리스트**:
- [ ] 날 전환 후 에너지가 수면 방식에 따라 회복된다 (→ see energy-system.md 섹션 5.1)
- [ ] 조기 수면 시 숙면(WellRested) 보너스가 활성화되고 EnergyBarUI에 아이콘이 표시된다
- [ ] 기절 발생 시 OnPassOut 이벤트가 발행되고 EconomyManager가 골드 페널티를 차감한다
- [ ] ResetDayState()가 Morning 12:00 진입 시 _isWellRested를 해제한다

---

### E-G: ISaveable 등록 + 세이브/로드 검증

**목적**: EnergyManager를 SaveManager에 등록하고, 세이브 → 종료 → 로드 후 에너지 수치가 정상 복원되는지 검증한다.

**전제 조건**:
- E-C 완료 (EnergyManager 씬 배치, SaveLoadOrder = 51 설정)
- ARC-008 완료 (SaveManager, ISaveable, PlayerSaveData)

**예상 MCP 호출**: ~6회

#### E-G-01: SaveManager 등록 확인

EnergyManager는 `OnEnable()`에서 `SaveManager.Instance.Register(this)`를 호출하도록 설계되어 있다. Play Mode 진입 후 SaveManager가 EnergyManager를 ISaveable로 인식하는지 확인한다.

```
enter_play_mode

get_console_logs
  // "[SaveManager] Registered: EnergyManager (order=51)" 형태 로그 확인
```

- **MCP 호출**: 2회

#### E-G-02: 세이브 실행

```
execute_method
  // SaveManager.Save() 호출
  // PlayerSaveData의 currentEnergy, maxEnergy 필드 직렬화 확인

get_console_logs
  // "[EnergyManager] GetSaveData: currentEnergy=X, maxEnergy=Y" 로그 확인
```

- **MCP 호출**: 1회

#### E-G-03: 로드 후 복원 검증

```
execute_method
  // 에너지를 일부 소모 후 재세이브, 이후 LoadSaveData() 호출하여 복원 확인

get_console_logs
  // "[EnergyManager] LoadSaveData: restored currentEnergy=X, maxEnergy=Y" 로그 확인
  // 복원된 수치가 세이브 시점 수치와 일치해야 함

exit_play_mode
```

- **MCP 호출**: 2회

#### E-G-04: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

**E-G 완료 체크리스트**:
- [ ] Play Mode 진입 시 SaveManager에 EnergyManager가 등록된다 (SaveLoadOrder = 51)
- [ ] 세이브 후 로드 시 currentEnergy, maxEnergy가 정확히 복원된다
- [ ] 레벨업 보너스로 증가된 maxEnergy도 복원된다
- [ ] 복원 후 EnergyBarUI 게이지 바가 복원된 수치를 정상 표시한다

[RISK] 현재 PlayerSaveData에 `_tempMaxBonusToday`와 `_isWellRested` 필드가 없는 상태이다. 세이브 시점에 임시 보너스가 활성화되어 있으면 로드 후 소실된다. 이 동작이 의도된 것인지 설계 레벨에서 확인 필요 (→ see energy-architecture.md Open Questions 1번).

---

## Cross-references

| 관련 문서 | 연관 내용 |
|-----------|-----------|
| `docs/systems/energy-architecture.md` (ARC-044) | 에너지 시스템 기술 아키텍처 전체 (클래스, API, 이벤트, 폴더) |
| `docs/systems/energy-system.md` (DES-024) | 에너지 수치 canonical 출처 (소모량, 회복량, 임계값, 배수) |
| `docs/systems/ui-system.md` 섹션 [E] | EnergyBarUI 시각 표현 규격 |
| `docs/systems/save-load-architecture.md` (ARC-011) 섹션 7 | ISaveable, SaveLoadOrder 전체 할당표 |
| `docs/pipeline/data-pipeline.md` Part II 섹션 2.2 | PlayerSaveData 필드 정의 (currentEnergy, maxEnergy) |
| `docs/systems/project-structure.md` | 폴더 구조, 네임스페이스 규칙, asmdef 의존성 |
| `docs/mcp/scene-setup-tasks.md` (ARC-002) | 씬 기본 계층 구조 (`--- MANAGERS ---`, `Canvas_Overlay`) |
| `docs/mcp/farming-tasks.md` (ARC-003) | FarmingSystem 씬 배치 |
| `docs/mcp/fishing-tasks.md` (ARC-028) | FishingManager 씬 배치 |
| `docs/mcp/gathering-tasks.md` (ARC-032) | GatheringManager 씬 배치 |
| `docs/mcp/save-load-tasks.md` (ARC-012) | SaveManager 씬 배치, ISaveable 구조 |
| `docs/mcp/ui-tasks.md` (ARC-018) | Canvas_Overlay, HUDController, HUD 좌측 하단 슬롯 배치 |

---

## Open Questions

1. [OPEN - to be filled after DES-024 follow-up 확정] 세이브 시점에 당일 임시 maxEnergy 보너스(`_tempMaxBonusToday`)와 숙면 상태(`_isWellRested`)를 저장해야 하는지 여부. 결정 전까지 두 필드는 세이브 대상에서 제외. (→ see energy-architecture.md Open Questions 1번)
2. [OPEN] `FarmingSystem`, `FishingManager`, `GatheringManager` 기존 코드에 `[SerializeField] private EnergyManager _energyManager;` 필드가 이미 포함되어 있는지 확인 필요. 미포함 시 각 시스템의 MCP 태스크 문서(ARC-003, ARC-028, ARC-032)에 FIX를 등록해야 한다.
3. [OPEN] EnergyBarUI 프리팹을 별도 프리팹 에셋으로 생성할지, 씬 내 오브젝트로만 관리할지 미확정. 프리팹 방식은 다른 씬에서 재사용 시 필요. (→ see ui-architecture.md ARC-018)

---

## Risks

1. [RISK] **EnergyConfig 필드 수 (~25개)**: MCP `set_property` 개별 호출 방식으로 모든 필드를 설정하면 ~25회 추가 호출이 발생한다. Editor 스크립트(`InitEnergyConfig.cs`) 방식을 통해 1~2회 호출로 압축하고, 수치는 반드시 `docs/systems/energy-system.md` 섹션 9를 참조하여 입력해야 한다 (PATTERN-007).
2. [RISK] **컴파일 순서 의존성**: S-01~S-04(enum/SO/static class)가 컴파일 완료되지 않은 상태에서 S-05(EnergyManager)를 생성하면 컴파일 에러가 발생한다. E-A-06 `execute_menu_item` 대기 후 다음 태스크 진행 필수.
3. [RISK] **PlayerController EnergyEvents 구독 누락**: PlayerController 기존 코드에 `OnEnergyDepleted` 구독이 없으면 에너지 고갈 시 이동 속도 패널티가 적용되지 않는다. E-C-04 검증 단계에서 콘솔 로그로 반드시 확인할 것.
4. [RISK] **TimeManager 이벤트 방식 불일치**: TimeManager가 `static event Action` 대신 다른 방식(UnityEvent, 직접 메서드 호출 등)으로 날 전환을 처리하는 경우, EnergyManager의 `OnEnable()` 구독 코드가 작동하지 않는다. E-F-01 검증에서 사전 확인 필수.
5. [RISK] **이중 세이브 처리**: PlayerController와 EnergyManager가 모두 `PlayerSaveData.currentEnergy`를 처리하는 경우, SaveLoadOrder 순서에 따라 값이 덮어쓰여지는 버그가 발생할 수 있다. EnergyManager(SaveLoadOrder=51)만 에너지 필드를 소유하도록 PlayerController 측 처리를 확인해야 한다. (→ see energy-architecture.md Risks 2번)

---

*이 문서는 Claude Code가 ARC-047 태스크에 따라 작성했습니다. `docs/systems/energy-architecture.md`(ARC-044) Part II 요약을 독립 MCP 태스크 시퀀스 문서로 분리·상세화합니다.*
