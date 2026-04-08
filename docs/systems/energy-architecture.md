# 에너지 시스템 기술 아키텍처

> EnergyManager MonoBehaviour, EnergyConfig ScriptableObject, IEnergyConsumer 통합 패턴, 회복 파이프라인, EnergyBarUI 연동, 세이브/로드 통합  
> 작성: Claude Code (Sonnet 4.6) | 2026-04-08  
> 문서 ID: ARC-044

---

## Context

이 문서는 SeedMind 에너지 시스템의 **기술 아키텍처 문서**이다. 에너지 시스템 설계의 canonical 출처는 `docs/systems/energy-system.md` (DES-024)이며, 이 문서는 그 설계를 Unity 컴포넌트 구조로 어떻게 구현할지를 정의한다.

**설계 목표**:
- EnergyManager 싱글턴이 에너지 상태를 중앙에서 관리하며 다른 시스템은 API만 호출
- IEnergyConsumer 인터페이스로 각 시스템(농업/낚시/채집)의 소모 로직을 EnergyManager에 위임
- EnergyConfig ScriptableObject로 모든 튜닝 파라미터를 외부화하여 코드 수정 없이 밸런스 조정 가능
- 이벤트 기반 UI 연동으로 EnergyBarUI와 EnergyManager 사이의 결합을 최소화
- PlayerSaveData 내 에너지 필드를 통해 세이브/로드 시스템에 통합

**본 문서가 canonical인 데이터**:
- EnergyManager 클래스 다이어그램, API 설계
- EnergyConfig ScriptableObject 필드 정의 (스키마만, 실제 수치는 energy-system.md 참조)
- IEnergyConsumer 인터페이스 설계
- EnergyEvents 정적 이벤트 허브 설계
- EnergyBarUI 컴포넌트 구조
- EnergySaveData 필드 정의 및 SaveLoadOrder 할당
- Unity 폴더 배치 및 네임스페이스

**본 문서가 canonical이 아닌 데이터 (참조만)**:

| 데이터 종류 | 참조처 |
|------------|--------|
| 행동별 에너지 소모량, 날씨/시간 배수, 튜닝 파라미터 수치 | `docs/systems/energy-system.md` |
| 에너지 바 HUD 시각적 표현 상세 | `docs/systems/ui-system.md` 섹션 [E] |
| Screen FSM, Canvas 계층 구조 | `docs/systems/ui-architecture.md` (ARC-018) |
| ISaveable 인터페이스, SaveLoadOrder 전체 할당표 | `docs/systems/save-load-architecture.md` (ARC-011) 섹션 7 |
| PlayerSaveData 필드 정의 | `docs/pipeline/data-pipeline.md` Part II 섹션 2.2 |
| ProgressionManager.OnLevelUp 이벤트 구조 | `docs/systems/progression-architecture.md` (BAL-002) 섹션 1.3 |
| 프로젝트 폴더 구조, 네임스페이스 규칙 | `docs/systems/project-structure.md` |

---

# Part I — 아키텍처 설계

---

## 1. 클래스 개요

### 1.1 클래스 책임 요약

| 클래스 | 유형 | 네임스페이스 | 책임 |
|--------|------|-------------|------|
| **EnergyManager** | MonoBehaviour (Singleton) | `SeedMind.Player` | 에너지 상태 관리, 소모/회복 API, 이벤트 발행 |
| **EnergyConfig** | ScriptableObject | `SeedMind.Player.Data` | 튜닝 파라미터 외부화 |
| **EnergyEvents** | static class | `SeedMind.Player` | 에너지 관련 정적 이벤트 허브 |
| **IEnergyConsumer** | interface | `SeedMind.Player` | 에너지 소모 시스템이 구현하는 계약 |
| **EnergyBarUI** | MonoBehaviour | `SeedMind.UI` | HUD 에너지 바 표시, 이벤트 구독 |
| **EnergySaveData** | plain C# class | `SeedMind.Player` | 에너지 상태 직렬화 데이터 |

### 1.2 시스템 관계도

```
┌───────────────────────────────────────────────────────────────────┐
│                   SeedMind.Player                                  │
└───────────────────────────────────────────────────────────────────┘

[소모 시스템]                  [EnergyManager]              [UI]
FarmingSystem  ─────────────▶ ┌─────────────┐ ─OnChanged──▶ EnergyBarUI
FishingManager ─TryConsume─▶ │EnergyManager│
GatheringMgr   ─────────────▶ │  (Singleton)│ ─OnDepleted─▶ PlayerController
                               │             │                (이동 속도 적용)
                               │             │ ─OnPassOut──▶ EconomyManager
                               └──────┬──────┘               (골드 손실)
                                      │ ref
                                      ▼
                               ┌─────────────┐
                               │EnergyConfig │
                               │    (SO)     │
                               └─────────────┘

[회복 파이프라인]
TimeManager.OnDayChanged ──▶ EnergyManager.OnDayChanged()
    └─ 수면 트리거: 회복 계산 후 다음 날 에너지 초기화

FoodItemConsumption ──▶ EnergyManager.RecoverEnergy(amount, source)
    └─ 즉시 에너지 회복, 임시 maxEnergy 증가 처리

ProgressionManager.OnLevelUp ──▶ EnergyManager.OnLevelUp(level)
    └─ 해당 레벨 도달 시 영구 maxEnergy 증가

[세이브/로드]
EnergyManager ─implements─▶ ISaveable (SaveLoadOrder = 51)
    └─ PlayerSaveData.currentEnergy / .maxEnergy 필드에 통합
```

---

## 2. EnergyManager 클래스 설계

### 2.1 클래스 다이어그램

```
┌──────────────────────────────────────────────────────────────────────┐
│             EnergyManager (MonoBehaviour, Singleton)                   │
│──────────────────────────────────────────────────────────────────────│
│  [설정 참조]                                                           │
│  - _config: EnergyConfig (ScriptableObject)                           │
│                                                                       │
│  [상태]                                                               │
│  - _currentEnergy: int                                                │
│  - _maxEnergy: int              // 기본 maxEnergy + 레벨업 보너스     │
│  - _tempMaxBonusToday: int       // 당일 음식 임시 최대치 보너스       │
│  - _isWellRested: bool           // 숙면 상태 (Morning 할인 활성 여부) │
│  - _currentWeatherMult: float    // 현재 날씨 에너지 배수             │
│  - _currentTimeMult: float       // 현재 시간대 에너지 배수           │
│                                                                       │
│  [읽기 전용 프로퍼티]                                                   │
│  + CurrentEnergy: int                                                 │
│  + MaxEnergy: int                // _maxEnergy + _tempMaxBonusToday  │
│  + TempMaxBonus: int             // 당일 임시 최대치 보너스            │
│  + IsWellRested: bool                                                 │
│  + IsDepleted: bool              // CurrentEnergy <= 0               │
│  + EnergyRatio: float            // CurrentEnergy / MaxEnergy (0~1)  │
│  + IsWarning: bool               // CurrentEnergy <= warningThreshold │
│                                                                       │
│  [소모 API]                                                           │
│  + CanConsume(int amount): bool                                       │
│  + TryConsume(int amount, EnergySource source): bool                 │
│      // 성공 시 OnEnergyChanged, OnEnergyConsumed 발행                │
│      // 실패(부족) 시 OnEnergyInsufficient 발행                       │
│  + ConsumeRaw(int amount): void  // 검사 없이 강제 소모 (기절 처리용) │
│                                                                       │
│  [회복 API]                                                           │
│  + RecoverEnergy(int amount, EnergySource source): void              │
│      // MaxEnergy 상한 준수, 임시 최대치 포함 처리                    │
│      // OnEnergyChanged 발행                                          │
│  + AddTempMaxBonus(int bonus): void                                  │
│      // 당일 임시 maxEnergy 증가 (→ see energy-system.md 섹션 5.2)   │
│      // 상한 초과 시 상한으로 고정                                     │
│  + SetWellRested(bool value): void                                   │
│      // 숙면 상태 설정 (수면 처리 시 TimeManager가 호출)              │
│                                                                       │
│  [배수 갱신 API]                                                      │
│  + SetWeatherMultiplier(float mult): void   // WeatherSystem이 호출  │
│  + SetTimeMultiplier(float mult): void      // TimeManager가 호출    │
│                                                                       │
│  [레벨업 연동]                                                        │
│  + ApplyLevelUpBonus(int newLevel): void                             │
│      // ProgressionManager.OnLevelUp 수신 시 호출                    │
│      // 해당 레벨 bonusLevel에 해당하면 영구 maxEnergy 증가          │
│                                                                       │
│  [일 전환 처리]                                                       │
│  - ProcessDayEnd(SleepType sleepType, int energyAtSleep): void      │
│      // 수면 방식에 따라 회복량 계산 (→ see energy-system.md 섹션 5.1)│
│      // 골드 페널티 발행 (기절 시)                                    │
│      // _isWellRested 설정                                            │
│  - ResetDayState(): void                                             │
│      // _tempMaxBonusToday = 0, _isWellRested = false (Morning 12:00)│
│                                                                       │
│  [초기화/세이브]                                                       │
│  + Initialize(EnergyConfig config): void                             │
│  + GetSaveData(): EnergySaveData                                     │
│  + LoadSaveData(EnergySaveData data): void                           │
│                                                                       │
│  [이벤트 구독 — OnEnable/OnDisable]                                   │
│  + TimeManager.OnDayChanged ─▶ HandleDayChanged                      │
│  + TimeManager.OnTimeSlotChanged ─▶ HandleTimeSlotChanged            │
│  + WeatherSystem.OnWeatherChanged ─▶ HandleWeatherChanged            │
│  + ProgressionManager.OnLevelUp ─▶ HandleLevelUp                     │
└──────────────────────────────────────────────────────────────────────┘
```

### 2.2 에너지 소모 계산 흐름

```
TryConsume(baseAmount, source) 호출
    │
    ├─ 1) 배수 적용
    │      effectiveAmount = Mathf.CeilToInt(
    │          baseAmount * _currentWeatherMult * _currentTimeMult
    │      )
    │      // 숙면 Morning 할인 적용 (→ see energy-system.md 섹션 5.1)
    │      if (_isWellRested && IsMorningSlot())
    │          effectiveAmount = Mathf.FloorToInt(effectiveAmount * (1f - config.sleepEarlyMorningDiscount))
    │          // sleepEarlyMorningDiscount → see docs/systems/energy-system.md 섹션 5.1
    │
    ├─ 2) 소모 가능 여부 확인
    │      if (_currentEnergy < effectiveAmount) → OnEnergyInsufficient 발행, return false
    │
    ├─ 3) 소모 실행
    │      _currentEnergy -= effectiveAmount
    │
    ├─ 4) 이벤트 발행
    │      EnergyEvents.RaiseEnergyConsumed(source, effectiveAmount)
    │      EnergyEvents.RaiseEnergyChanged(_currentEnergy, MaxEnergy)
    │      if (_currentEnergy == 0) EnergyEvents.RaiseEnergyDepleted()
    │      if (_currentEnergy <= config.energyWarningThreshold && 직전은 초과)
    │          EnergyEvents.RaiseEnergyWarning()
    │
    └─ return true
```

### 2.3 SleepType 열거형

```csharp
// illustrative
namespace SeedMind.Player
{
    /// <summary>
    /// 수면 방식. 수면 회복량과 숙면 보너스 적용 여부를 결정한다.
    /// 회복량 수치 → see docs/systems/energy-system.md 섹션 5.1
    /// </summary>
    public enum SleepType
    {
        EarlySleep  = 0,   // 20:00 이전 조기 수면 (숙면 보너스 발생)
        NormalSleep = 1,   // 20:00~24:00 일반·늦은 수면 (회복 100%, 숙면 보너스 없음)
        PassOut     = 2    // 기절 (50% 회복, 골드 페널티)
    }
}
```

### 2.4 EnergySource 열거형

```csharp
// illustrative
namespace SeedMind.Player
{
    /// <summary>
    /// 에너지 소모/회복의 발생 원인. 이벤트 페이로드 및 분석 로그에 사용.
    /// </summary>
    public enum EnergySource
    {
        // 소모
        FarmingTool    = 0,   // 경작 도구 (호미, 물뿌리개, 낫)
        FishingCast    = 1,   // 낚시 캐스팅
        FishingFail    = 2,   // 낚시 미니게임 실패 추가 소모
        GatheringTool  = 3,   // 채집 도구 사용
        PassOut        = 4,   // 기절 에너지 차감

        // 회복
        SleepRecovery  = 10,  // 수면 회복
        FoodBasic      = 11,  // 기본 음식 섭취
        FoodNormal     = 12,  // 일반 요리 섭취
        FoodPremium    = 13,  // 고급 요리 섭취
        FoodLuxury     = 14,  // 최고급 요리 섭취
        RestArea       = 15,  // 집 내부 휴식 [OPEN] 기능 미확정
        HotSpring      = 16,  // 온천 시설 [OPEN] 기능 미확정
        EventReward    = 17   // 이벤트/퀘스트 보상
    }
}
```

---

## 3. EnergyConfig ScriptableObject 설계

### 3.1 필드 정의

```csharp
// illustrative
namespace SeedMind.Player.Data
{
    /// <summary>
    /// 에너지 시스템 튜닝 파라미터 ScriptableObject.
    /// 모든 수치의 canonical 출처 → see docs/systems/energy-system.md 섹션 9
    /// </summary>
    [CreateAssetMenu(fileName = "EnergyConfig", menuName = "SeedMind/Energy/EnergyConfig")]
    public class EnergyConfig : ScriptableObject
    {
        [Header("기본 최대 에너지")]
        public int baseMaxEnergy;              // → see docs/systems/energy-system.md 섹션 1.1
        public int startOfDayEnergy;           // → see docs/systems/energy-system.md 섹션 1.1

        [Header("레벨업 보너스")]
        public int[] levelUpBonusLevels;       // → see docs/systems/energy-system.md 섹션 1.2
        public int energyBonusPerThreshold;    // → see docs/systems/energy-system.md 섹션 1.2

        [Header("경고/고갈 임계값")]
        public int energyWarningThreshold;     // → see docs/systems/energy-system.md 섹션 6.1
        public float energyDepletionSpeedPenalty; // → see docs/systems/energy-system.md 섹션 6.1

        [Header("수면 회복")]
        public float sleepFullRecovery;        // → see docs/systems/energy-system.md 섹션 5.1
        public int sleepEarlyBonusEnergy;      // → see docs/systems/energy-system.md 섹션 5.1
        public float sleepEarlyBonusMultiplier; // → see docs/systems/energy-system.md 섹션 5.1
        public float sleepEarlyMorningDiscount; // → see docs/systems/energy-system.md 섹션 5.1
        public float passOutEnergyRecovery;    // → see docs/systems/energy-system.md 섹션 5.1

        [Header("기절 골드 페널티")]
        public float passOutGoldPenaltyRate;   // → see docs/systems/energy-system.md 섹션 6.2
        public int passOutGoldPenaltyCap;      // → see docs/systems/energy-system.md 섹션 6.2

        [Header("날씨 배수")]
        public float heavyRainEnergyMult;      // → see docs/systems/energy-system.md 섹션 3
        public float stormEnergyMult;          // → see docs/systems/energy-system.md 섹션 3
        public float snowEnergyMult;           // → see docs/systems/energy-system.md 섹션 3
        public float blizzardEnergyMult;       // → see docs/systems/energy-system.md 섹션 3

        [Header("시간대 배수")]
        public float nightEnergyMultiplier;    // → see docs/systems/energy-system.md 섹션 4

        [Header("경작 도구 에너지 소모")]
        public int hoeEnergyBasic;             // → see docs/systems/energy-system.md 섹션 2.1
        public int hoeEnergyReinforced;        // → see docs/systems/energy-system.md 섹션 2.1
        public int hoeEnergyLegendary;         // → see docs/systems/energy-system.md 섹션 2.1
        public int waterEnergyBasic;           // → see docs/systems/energy-system.md 섹션 2.1
        public int waterEnergyReinforced;      // → see docs/systems/energy-system.md 섹션 2.1
        public int waterEnergyLegendary;       // → see docs/systems/energy-system.md 섹션 2.1
        public int sickleEnergyBasic;          // → see docs/systems/energy-system.md 섹션 2.1
        public int sickleEnergyReinforced;     // → see docs/systems/energy-system.md 섹션 2.1
        public int sickleEnergyLegendary;      // → see docs/systems/energy-system.md 섹션 2.1

        [Header("낚시 에너지 소모")]
        public int castEnergy;                 // → see docs/systems/energy-system.md 섹션 2.2
        public int castEnergyHighLevel;        // → see docs/systems/energy-system.md 섹션 2.2
        public int fishingHighLevelThreshold;  // → see docs/systems/energy-system.md 섹션 2.2
        public int failExtraEnergy;            // → see docs/systems/energy-system.md 섹션 2.2

        [Header("채집 에너지 소모")]
        public int toolGatherEnergy;           // → see docs/systems/energy-system.md 섹션 2.3

        [Header("음식 임시 최대치")]
        public int tempMaxEnergyBonusCap;      // → see docs/systems/energy-system.md 섹션 5.2

        /// <summary>
        /// 도구 등급(ToolGrade)에 해당하는 호미 에너지 소모량 반환.
        /// 등급 enum → see docs/systems/tool-upgrade.md
        /// </summary>
        public int GetHoeEnergy(ToolGrade grade)
        {
            return grade switch
            {
                ToolGrade.Basic       => hoeEnergyBasic,       // → see docs/systems/energy-system.md 섹션 2.1
                ToolGrade.Reinforced  => hoeEnergyReinforced,  // → see docs/systems/energy-system.md 섹션 2.1
                ToolGrade.Legendary   => hoeEnergyLegendary,   // → see docs/systems/energy-system.md 섹션 2.1
                _                     => hoeEnergyBasic
            };
        }

        /// <summary>
        /// WeatherType에 해당하는 에너지 배수 반환.
        /// WeatherType 정의 → see docs/systems/time-season-architecture.md
        /// 배수 수치 → see docs/systems/energy-system.md 섹션 3
        /// </summary>
        public float GetWeatherMultiplier(WeatherType weather)
        {
            return weather switch
            {
                WeatherType.HeavyRain => heavyRainEnergyMult, // → see docs/systems/energy-system.md 섹션 3
                WeatherType.Storm     => stormEnergyMult,      // → see docs/systems/energy-system.md 섹션 3
                WeatherType.Snow      => snowEnergyMult,       // → see docs/systems/energy-system.md 섹션 3
                WeatherType.Blizzard  => blizzardEnergyMult,   // → see docs/systems/energy-system.md 섹션 3
                _                     => 1.0f
            };
        }
    }
}
```

### 3.2 SO 에셋 데이터 테이블

| 에셋 이름 | 에셋 경로 | 수치 참조 |
|----------|----------|----------|
| `EnergyConfig` | `Assets/_Project/Data/Energy/EnergyConfig.asset` | (→ see docs/systems/energy-system.md 섹션 9) |

---

## 4. IEnergyConsumer 인터페이스 및 소모 통합

### 4.1 IEnergyConsumer 설계

에너지 소모가 필요한 시스템(FarmingSystem, FishingManager, GatheringManager)은 직접 EnergyManager를 참조하는 대신, 공통 API인 `EnergyManager.TryConsume()`을 호출한다. 별도의 IEnergyConsumer 인터페이스를 각 시스템에 구현하는 방식도 가능하나, SeedMind에서는 **직접 API 호출 방식**을 채택한다.

**채택 근거**:
- 에너지 소모 로직이 각 시스템 내부에 분산되지 않고 EnergyManager에 집중
- 소모 시스템이 EnergyManager에 결합되지만, 단방향 의존성이므로 허용 범위
- 인터페이스 방식은 소모량 계산 책임이 불명확해지는 위험이 있음

```csharp
// illustrative — FarmingSystem에서의 사용 패턴
namespace SeedMind.Farm
{
    public class FarmingSystem : MonoBehaviour
    {
        [SerializeField] private EnergyManager _energyManager; // Inspector 연결

        private bool TryUseTool(ToolGrade grade, ToolType toolType)
        {
            int cost = _energyManager.Config.GetHoeEnergy(grade);
            // 실제 소모량 산출 → see docs/systems/energy-system.md 섹션 2.1
            if (!_energyManager.TryConsume(cost, EnergySource.FarmingTool))
            {
                // UI 피드백: "에너지가 부족합니다"
                return false;
            }
            // 도구 실행
            return true;
        }
    }
}
```

### 4.2 시스템별 소모 패턴 요약

| 시스템 | 소모 호출 시점 | EnergySource | 비고 |
|--------|-------------|-------------|------|
| FarmingSystem | 도구 사용 확정 직전 | `FarmingTool` | 도구 등급별 소모 (→ see energy-system.md 섹션 2.1) |
| FishingManager | 캐스팅 확정 시 | `FishingCast` | 숙련도 레벨 기준 소모 (→ see energy-system.md 섹션 2.2) |
| FishingManager | 미니게임 실패 판정 시 | `FishingFail` | 실패 추가 소모 (→ see energy-system.md 섹션 2.2) |
| GatheringManager | 채집 도구 사용 시 | `GatheringTool` | 맨손은 소모 0이므로 TryConsume 호출 불필요 (→ see energy-system.md 섹션 2.3) |

---

## 5. 회복 파이프라인

### 5.1 수면 회복 흐름

수면 처리는 TimeManager의 일 전환 이벤트 또는 플레이어의 침대 상호작용으로 시작된다.

```
[수면 트리거]
  PlayerController.InteractWithBed()
      │
      ├─ TimeManager.TriggerSleep(currentHour) 호출
      │      └─ SleepType 결정:
      │           currentHour < 20h → EarlySleep
      │           20h <= currentHour < 24h → NormalSleep
      │
      └─ EnergyManager.ProcessDayEnd(sleepType, _currentEnergy) 호출
             │
             ├─ EarlySleep:
             │    bonus = Min(_currentEnergy × sleepEarlyBonusMultiplier, sleepEarlyBonusEnergy)
             │    nextDayEnergy = Min(_maxEnergy + bonus, _maxEnergy + sleepEarlyBonusEnergy)
             │    _isWellRested = true
             │    // 수치 → see docs/systems/energy-system.md 섹션 5.1
             │
             ├─ NormalSleep:
             │    nextDayEnergy = _maxEnergy (100%)
             │    _isWellRested = false
             │
             └─ PassOut (TimeManager.OnPassOut 이벤트 수신):
                  nextDayEnergy = Mathf.RoundToInt(_maxEnergy × passOutEnergyRecovery)
                  _isWellRested = false
                  EnergyEvents.RaisePassOut(currentGold, goldLost)
                  // 골드 페널티 계산 → see energy-system.md 섹션 6.2
```

### 5.2 음식 회복 흐름

```
[음식 아이템 사용 — InventorySystem 또는 FoodSystem이 호출]
  EnergyManager.RecoverEnergy(amount, source)
      │
      ├─ source가 FoodPremium 또는 FoodLuxury인 경우:
      │    AddTempMaxBonus(tempBonusAmount)
      │    // 상한: tempMaxEnergyBonusCap → see energy-system.md 섹션 5.2
      │
      ├─ _currentEnergy += amount
      │    // MaxEnergy 상한 준수 (임시 보너스 포함 상한까지)
      │
      └─ EnergyEvents.RaiseEnergyChanged(_currentEnergy, MaxEnergy)
```

### 5.3 Morning 시간대 숙면 해제

```
TimeManager.OnTimeSlotChanged 수신
    │
    ├─ 새 시간대가 Morning(08:00) 진입 → _isWellRested 유지 (Morning 할인 활성)
    └─ 새 시간대가 Afternoon(12:00) 진입 → _isWellRested = false, ResetWellRested 이벤트 발행
```

---

## 6. EnergyEvents 이벤트 허브

### 6.1 이벤트 목록

```csharp
// illustrative
namespace SeedMind.Player
{
    /// <summary>
    /// 에너지 시스템 정적 이벤트 허브.
    /// UI, PlayerController, EconomyManager 등이 구독한다.
    /// </summary>
    public static class EnergyEvents
    {
        /// <summary>에너지 값이 변경될 때마다 발행 (소모/회복 모두).</summary>
        public static event Action<int, int> OnEnergyChanged;     // (current, max)

        /// <summary>에너지 소모 발생 시 발행.</summary>
        public static event Action<EnergySource, int> OnEnergyConsumed; // (source, amount)

        /// <summary>에너지 부족으로 소모 실패 시 발행 — UI 피드백 트리거.</summary>
        public static event Action OnEnergyInsufficient;

        /// <summary>에너지가 경고 임계값 이하로 떨어질 때 발행.</summary>
        public static event Action<int> OnEnergyWarning;          // (currentEnergy)

        /// <summary>에너지가 0에 도달했을 때 발행.</summary>
        public static event Action OnEnergyDepleted;

        /// <summary>기절 발생 시 발행 — 골드 페널티 처리용.</summary>
        public static event Action<int, int> OnPassOut;           // (currentGold, goldLost)

        /// <summary>숙면 보너스 활성화 시 발행.</summary>
        public static event Action OnWellRestedActivated;

        /// <summary>숙면 보너스 해제 시 발행 (Morning 종료).</summary>
        public static event Action OnWellRestedDeactivated;

        /// <summary>레벨업으로 영구 maxEnergy 증가 시 발행.</summary>
        public static event Action<int, int> OnMaxEnergyIncreased; // (oldMax, newMax)

        // --- Raise 메서드 ---
        public static void RaiseEnergyChanged(int current, int max)
            => OnEnergyChanged?.Invoke(current, max);
        public static void RaiseEnergyConsumed(EnergySource src, int amount)
            => OnEnergyConsumed?.Invoke(src, amount);
        public static void RaiseEnergyInsufficient()
            => OnEnergyInsufficient?.Invoke();
        public static void RaiseEnergyWarning(int current)
            => OnEnergyWarning?.Invoke(current);
        public static void RaiseEnergyDepleted()
            => OnEnergyDepleted?.Invoke();
        public static void RaisePassOut(int currentGold, int goldLost)
            => OnPassOut?.Invoke(currentGold, goldLost);
        public static void RaiseWellRestedActivated()
            => OnWellRestedActivated?.Invoke();
        public static void RaiseWellRestedDeactivated()
            => OnWellRestedDeactivated?.Invoke();
        public static void RaiseMaxEnergyIncreased(int oldMax, int newMax)
            => OnMaxEnergyIncreased?.Invoke(oldMax, newMax);
    }
}
```

### 6.2 이벤트 구독자 요약

| 이벤트 | 주요 구독자 | 처리 내용 |
|--------|----------|---------|
| `OnEnergyChanged` | EnergyBarUI | 게이지 바 갱신, 숫자 텍스트 갱신 |
| `OnEnergyInsufficient` | EnergyBarUI / NotificationManager | "에너지 부족" 피드백 표시 |
| `OnEnergyWarning` | EnergyBarUI | 바 색상 빨간색 전환 + 펄스 애니메이션 시작 |
| `OnEnergyDepleted` | EnergyBarUI / PlayerController | "완전히 지쳤습니다!" 메시지, 이동 속도 50% 감소 적용 |
| `OnPassOut` | EconomyManager | 골드 페널티 차감, 화면 페이드아웃 |
| `OnWellRestedActivated` | EnergyBarUI | 별빛 아이콘 표시 |
| `OnWellRestedDeactivated` | EnergyBarUI | 별빛 아이콘 숨김 |
| `OnMaxEnergyIncreased` | EnergyBarUI | 게이지 바 최대치 갱신 |

---

## 7. EnergyBarUI 컴포넌트 설계

### 7.1 클래스 다이어그램

```
┌──────────────────────────────────────────────────────────────────────┐
│             EnergyBarUI (MonoBehaviour)                                │
│──────────────────────────────────────────────────────────────────────│
│  [Inspector 참조]                                                      │
│  - _fillImage: Image              // 에너지 바 Fill 이미지             │
│  - _labelText: TextMeshProUGUI    // "현재치 / 최대치" 텍스트          │
│  - _wellRestedIcon: GameObject    // 숙면 상태 별빛 아이콘             │
│  - _tempMaxExtension: Image       // 임시 최대치 초과 연장 바 (황금색) │
│  - _pulseAnimation: Animator      // 경고 상태 펄스 애니메이터         │
│  - _floatingTextPrefab: GameObject // 소모 수치 플로팅 텍스트 프리팹   │
│                                                                       │
│  [상태]                                                               │
│  - _isWarning: bool               // 경고 상태 여부                   │
│                                                                       │
│  [생명주기]                                                           │
│  + OnEnable()                                                         │
│      EnergyEvents.OnEnergyChanged += HandleEnergyChanged             │
│      EnergyEvents.OnEnergyWarning += HandleEnergyWarning             │
│      EnergyEvents.OnEnergyDepleted += HandleEnergyDepleted           │
│      EnergyEvents.OnWellRestedActivated += HandleWellRestedOn        │
│      EnergyEvents.OnWellRestedDeactivated += HandleWellRestedOff     │
│      EnergyEvents.OnEnergyConsumed += HandleEnergyConsumed           │
│      EnergyEvents.OnMaxEnergyIncreased += HandleMaxEnergyIncreased   │
│  + OnDisable() → 전체 구독 해제                                        │
│                                                                       │
│  [핸들러]                                                             │
│  - HandleEnergyChanged(int current, int max): void                   │
│      → _fillImage.fillAmount 갱신 (baseMax 기준)                      │
│      → _labelText 갱신                                                │
│      → _tempMaxExtension 표시/비표시 및 크기 갱신                      │
│  - HandleEnergyWarning(int current): void                            │
│      → _isWarning = true, 바 색상 빨간색 전환, 펄스 애니메이션 시작   │
│  - HandleEnergyDepleted(): void                                      │
│      → "완전히 지쳤습니다!" 토스트 발행 (NotificationManager 경유)    │
│  - HandleWellRestedOn(): void → _wellRestedIcon 활성화               │
│  - HandleWellRestedOff(): void → _wellRestedIcon 비활성화            │
│  - HandleEnergyConsumed(EnergySource src, int amount): void          │
│      → 플로팅 텍스트 생성: "-{amount}" (에너지 바 상단)               │
│  - HandleMaxEnergyIncreased(int oldMax, int newMax): void            │
│      → 바 최대치 레이아웃 갱신                                         │
└──────────────────────────────────────────────────────────────────────┘
```

### 7.2 임시 최대치 시각 표현

에너지가 기본 maxEnergy를 초과할 때(음식 임시 최대치 보너스 활성) `_tempMaxExtension` 이미지가 황금색으로 연장 표시된다.

```
[기본 maxEnergy 100 기준]
┌────────────────────────────────────────────────────┐
│  녹색 게이지 (현재/기본최대치)  │ 황금 연장 (임시) │
└────────────────────────────────────────────────────┘
 ◀──────── baseMax 기준 너비 ────────▶ ◀── 보너스 ──▶
```

UI 시각 표현 상세 규격 → see `docs/systems/ui-system.md` 섹션 [E]

---

## 8. 세이브/로드 통합

### 8.1 EnergySaveData 구조

에너지 상태는 PlayerSaveData 내 필드로 통합 저장한다. 별도의 최상위 세이브 데이터 클래스를 추가하지 않는다.

```csharp
// illustrative — PlayerSaveData 내 에너지 관련 필드 (기존)
// canonical 정의 → see docs/pipeline/data-pipeline.md Part II 섹션 2.2
namespace SeedMind.Player
{
    [System.Serializable]
    public class PlayerSaveData
    {
        // 위치, 레벨, 경험치 등 기타 필드 생략 (canonical: data-pipeline.md)

        public int currentEnergy;          // 현재 에너지
        public int maxEnergy;              // 영구 최대 에너지 (레벨업 보너스 누적 포함)
        // [OPEN] 당일 임시 maxEnergy 보너스(_tempMaxBonusToday) 및
        //        숙면 상태(_isWellRested)의 세이브 포함 여부 미확정.
        //        세이브 후 재시작 시 당일 보너스가 유지되어야 하는지,
        //        또는 다음 날 시작으로 처리해야 하는지 결정 필요.
        //        → [OPEN - to be filled after DES-024 follow-up 확정]
    }
}
```

### 8.2 EnergyManager ISaveable 구현

```csharp
// illustrative
namespace SeedMind.Player
{
    public partial class EnergyManager : MonoBehaviour, ISaveable
    {
        // SaveLoadOrder 51: PlayerController(50) 직후,
        // FishingManager(52) 이전에 복원되어야 함
        // → see docs/systems/save-load-architecture.md 섹션 7 SaveLoadOrder 할당표
        public int SaveLoadOrder => 51;

        public object GetSaveData()
        {
            // PlayerSaveData의 에너지 필드만 갱신
            // 전체 PlayerSaveData 구성 → see data-pipeline.md Part II 섹션 2.2
            return new { currentEnergy = _currentEnergy, maxEnergy = _maxEnergy };
        }

        public void LoadSaveData(object data)
        {
            // PlayerSaveData에서 에너지 필드 복원
            // 수치 기본값 → see docs/systems/energy-system.md 섹션 1.1
        }
    }
}
```

### 8.3 SaveLoadOrder 할당

| 시스템 | SaveLoadOrder | 근거 |
|--------|:------------:|------|
| PlayerController | 50 | 기존 할당 (→ see save-load-architecture.md 섹션 7) |
| **EnergyManager** | **51** | PlayerController(50) 직후 복원. 에너지 상태가 플레이어 상태에 종속 |
| FishingManager | 52 | 기존 할당 (에너지 상태 복원 후 낚시 상태 복원 가능) |

[RISK] EnergyManager를 PlayerController와 분리된 ISaveable로 등록하는 경우, PlayerSaveData의 currentEnergy/maxEnergy 필드가 PlayerController 복원과 EnergyManager 복원 양쪽에서 사용되어 이중 처리 위험이 있다. 구현 시 PlayerController.LoadSaveData()가 에너지 필드를 EnergyManager에 위임하도록 처리하거나, EnergyManager만 에너지 필드를 처리하도록 역할을 명확히 분리해야 한다.

---

## 9. Unity 폴더 구조 및 네임스페이스

### 9.1 파일 배치

```
Assets/_Project/Scripts/Player/            # SeedMind.Player 네임스페이스
├── EnergyManager.cs                       # 싱글턴, 상태 관리, ISaveable
├── EnergyEvents.cs                        # 정적 이벤트 허브
├── EnergySource.cs                        # enum
├── SleepType.cs                           # enum
└── Data/                                  # SeedMind.Player.Data 네임스페이스
    └── EnergyConfig.cs                    # ScriptableObject 정의

Assets/_Project/Scripts/UI/                # SeedMind.UI 네임스페이스
└── EnergyBarUI.cs                         # HUD 에너지 바 컴포넌트

Assets/_Project/Data/Energy/               # SO 에셋 폴더
└── EnergyConfig.asset
```

(-> see `docs/systems/project-structure.md` 섹션 1 for 전체 폴더 구조)

### 9.2 네임스페이스

| 네임스페이스 | 포함 클래스 |
|------------|-----------|
| `SeedMind.Player` | EnergyManager, EnergyEvents, EnergySource, SleepType |
| `SeedMind.Player.Data` | EnergyConfig |
| `SeedMind.UI` | EnergyBarUI |

### 9.3 asmdef 의존성

`EnergyManager`는 `SeedMind.Player.asmdef` 내에 위치하며 다음을 참조한다:

| 참조 대상 | 이유 |
|----------|------|
| `SeedMind.Core.asmdef` | ISaveable, Singleton 기반 클래스 |
| `SeedMind.Save.asmdef` | ISaveable 인터페이스 |

`EnergyBarUI`는 `SeedMind.UI.asmdef`에 위치하며 `SeedMind.Player.asmdef`를 참조한다 (EnergyEvents 구독을 위해).

---

# Part II — MCP 태스크 시퀀스 요약

*상세 MCP 작업 계획은 별도 문서 `docs/mcp/energy-tasks.md`에서 관리한다. 본 섹션은 구현 단계 개요만 제공한다.*

### Step 1: EnergyConfig SO 생성
- MCP: `CreateScript("EnergyConfig.cs", "Scripts/Player/Data/")`
- MCP: `CreateAsset("EnergyConfig.asset", "Data/Energy/")`
- 모든 수치 필드는 `docs/systems/energy-system.md` 섹션 9 기준으로 Inspector에서 입력

### Step 2: EnergyManager 스크립트 생성
- MCP: `CreateScript("EnergyManager.cs", "Scripts/Player/")`
- MCP: `CreateScript("EnergyEvents.cs", "Scripts/Player/")`
- MCP: `CreateScript("EnergySource.cs", "Scripts/Player/")`
- MCP: `CreateScript("SleepType.cs", "Scripts/Player/")`
- GameManager GameObject에 EnergyManager 컴포넌트 추가, EnergyConfig 에셋 연결

### Step 3: PlayerController 연동
- PlayerController의 `OnEnable()`에 EnergyEvents.OnEnergyDepleted 구독 추가
- 에너지 0 시 이동 속도 패널티 적용 (→ see energy-system.md 섹션 6.1)

### Step 4: EnergyBarUI 생성
- MCP: `CreateScript("EnergyBarUI.cs", "Scripts/UI/")`
- HUD Canvas의 좌측 하단에 Energy Bar UI 프리팹 배치
- EnergyBarUI 컴포넌트에 Fill Image, Label Text, WellRested Icon 참조 연결

### Step 5: FarmingSystem / FishingManager / GatheringManager 연동
- 각 시스템의 도구 사용/캐스팅 메서드에 `EnergyManager.TryConsume()` 호출 추가

### Step 6: 수면 회복 연동
- TimeManager의 날 전환 처리에서 `EnergyManager.ProcessDayEnd()` 호출 추가
- 기절(PassOut) 이벤트 발행 구조 확인 및 EconomyManager 연동 테스트

### Step 7: ISaveable 등록 및 세이브/로드 검증
- EnergyManager를 SaveManager에 Register() 호출 (OnEnable에서)
- 세이브 → 로드 후 에너지 수치 복원 콘솔 로그 확인

---

## Cross-references

| 관련 문서 | 연관 내용 |
|-----------|-----------|
| `docs/systems/energy-system.md` (DES-024) | 에너지 수치, 튜닝 파라미터 canonical 출처 |
| `docs/systems/farming-system.md` 섹션 3.2 | 경작 도구 에너지 연동 (FIX 처리 예정) |
| `docs/systems/fishing-system.md` 섹션 2.3 | 낚시 에너지 연동 |
| `docs/systems/gathering-system.md` 섹션 1.3 | 채집 에너지 연동 |
| `docs/systems/ui-system.md` 섹션 [E] | 에너지 바 HUD 시각적 표현 canonical |
| `docs/systems/ui-architecture.md` (ARC-018) | HUDController, UIEvents 구조 |
| `docs/systems/save-load-architecture.md` (ARC-011) 섹션 7 | ISaveable, SaveLoadOrder 전체 할당표 |
| `docs/pipeline/data-pipeline.md` Part II 섹션 2.2 | PlayerSaveData 필드 정의 |
| `docs/systems/progression-architecture.md` (BAL-002) 섹션 1.3 | OnLevelUp 이벤트 구조 |
| `docs/systems/project-structure.md` 섹션 1~2 | 폴더 구조, 네임스페이스 규칙 |
| `docs/systems/tool-upgrade.md` | ToolGrade enum 정의 |
| `docs/systems/time-season-architecture.md` | WeatherType enum, TimeSlot 정의 |

---

## Open Questions

1. [OPEN - to be filled after DES-024 follow-up 확정] 세이브 시점에 당일 임시 maxEnergy 보너스(`_tempMaxBonusToday`)와 숙면 상태(`_isWellRested`)를 저장해야 하는지 여부. 저장한다면 PlayerSaveData 필드 추가 필요 — data-pipeline.md와 save-load-architecture.md 동기 업데이트 필요.
2. [OPEN] EnergyManager를 PlayerController와 별개의 ISaveable로 분리할지, PlayerController 내부에서 에너지 필드를 함께 처리할지 결정 필요. 현재 SaveLoadOrder 51 할당은 분리 방식을 전제로 한다.
3. [OPEN] 에너지 고갈 상태(에너지 0)에서 이동 속도 50% 감소를 PlayerController가 직접 EnergyEvents를 구독하여 처리할지, EnergyManager가 PlayerController의 메서드를 직접 호출할지 결정 필요. 이벤트 기반 방식이 결합도 측면에서 우선 권장.
4. [OPEN] 달리기 기능 추가 시 에너지 소모 여부 — DES 미확정 (→ see energy-system.md Open Questions 2번)
5. [OPEN] 온천/집 휴식 회복 기능 구현 시 RestArea/HotSpring EnergySource 사용 및 회복 트리거 설계 필요 — 해당 시설 설계 문서 확정 후 연동

---

## Risks

1. [RISK] **배수 중첩 정밀도**: 날씨 배수와 시간 배수를 곱한 후 ceil 처리하는 과정에서, 실제 소모량이 체감되지 않는 경우(예: 1 × 1.1 × 1.2 = 1.32 → ceil = 2) 플레이어 입장에서 예측 불가한 소모가 발생할 수 있다. 소모량 플로팅 텍스트 표시가 투명성 확보에 필수.
2. [RISK] **PlayerSaveData 에너지 필드 이중 처리**: PlayerController와 EnergyManager 양쪽이 currentEnergy/maxEnergy를 처리할 경우 복원 순서에 따라 값이 덮어쓰여지는 버그 발생 위험. 구현 시 에너지 필드의 소유권을 단일 시스템(EnergyManager 권장)으로 명확히 지정해야 함.
3. [RISK] **수면 트리거 타이밍**: TimeManager의 24:00 기절 처리와 플레이어 침대 상호작용 수면 처리가 동시에 발생할 가능성이 있다. ProcessDayEnd()에 중복 호출 방지 플래그(`_dayEndProcessed`) 추가 필요.
4. [RISK] **EnergyWarning 이벤트 발행 조건**: 에너지가 경고 임계값 이하 → 이상 → 이하로 반복될 때 경고 이벤트가 중복 발행될 수 있다. `_isWarning` 상태 플래그로 이미 경고 중인 경우 재발행을 방지해야 함.

---

*이 문서는 Claude Code가 ARC-044 태스크에 따라 작성했습니다. `docs/systems/energy-system.md` (DES-024) 설계를 기반으로 Unity 컴포넌트 아키텍처를 정의합니다.*
