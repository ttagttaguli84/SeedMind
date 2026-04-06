# 시간/계절 시스템 기술 아키텍처

> TimeManager, WeatherSystem, SeasonData, FestivalManager의 클래스 설계, 이벤트 처리 순서, 날씨 알고리즘, MCP 구현 계획  
> 작성: Claude Code (Opus) | 2026-04-06

---

## Context

이 문서는 `docs/architecture.md` 4.3절(시간 시스템)을 기술적으로 상세화한다. SeedMind의 시간/계절 시스템은 게임 전체의 리듬을 결정하는 핵심 인프라이며, 경작 시스템(GrowthSystem), 경제 시스템(EconomyManager), UI(HUD) 등 거의 모든 시스템이 이 시스템의 이벤트에 의존한다. 따라서 이벤트 발행 순서와 구독자 실행 순서의 명확한 정의가 특히 중요하다.

**설계 목표**:
- 시간 진행이 결정론적(Deterministic)이어야 한다 -- 동일한 입력(저장 데이터)으로 동일한 결과 보장
- 날씨 시스템은 시드(seed) 기반 난수로 재현 가능해야 한다
- 모든 시간 관련 데이터는 ScriptableObject로 분리하여 밸런스 조정이 코드 변경 없이 가능하도록 한다

---

## 1. Enum 정의

### 1.1 Season

```csharp
namespace SeedMind.Core
{
    public enum Season
    {
        Spring = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 3
    }
}
```

> SeasonFlag(비트마스크)는 `farming-architecture.md`에 정의되어 있으며 작물 재배 가능 계절 판별에 사용된다. Season enum은 현재 계절을 나타내는 단일 값으로 별도 목적이다.

### 1.2 DayPhase (시간대)

```csharp
namespace SeedMind.Core
{
    public enum DayPhase
    {
        Dawn,       // 06:00 ~ 08:00 미만  — 새벽, 하루 시작
        Morning,    // 08:00 ~ 11:59  — 오전
        Afternoon,  // 12:00 ~ 16:59  — 오후
        Evening,    // 17:00 ~ 19:59  — 저녁, 석양
        Night       // 20:00 ~ 23:59  — 밤, 하루 종료 접근
    }
}
```

**시간대별 환경 효과**:

| DayPhase | 조명 색온도 | Light Intensity | 앰비언트 | 플레이어 영향 |
|----------|------------|-----------------|----------|---------------|
| Dawn | #FFE4B5 (황금빛) | 0.6 | Warm | 하루 시작, 일일 처리 실행 |
| Morning | #FFFAED (밝은 백색) | 1.0 | Neutral | 기본 활동 시간 |
| Afternoon | #FFFFFF (백색) | 1.0 | Neutral | 기본 활동 시간 |
| Evening | #FFB347 (주황) | 0.7 | Warm | 상점 마감 경고 |
| Night | #4A6FA5 (푸른빛) | 0.3 | Cool | 이동 속도 감소, 자동 귀가 임박 |

### 1.3 WeatherType (날씨)

```csharp
namespace SeedMind.Core
{
    public enum WeatherType
    {
        Clear,       // 맑음 — 기본 날씨
        Cloudy,      // 흐림 — 시각적 변화만, 게임플레이 영향 없음
        Rain,        // 비 — 야외 경작 타일 자동 물주기
        HeavyRain,   // 폭우 — 자동 물주기 + 이동속도 감소
        Storm,       // 폭풍 — 자동 물주기 + 작물 피해 + 이동속도 감소
        Snow,        // 눈 — 야외 작물 성장 정지, 이동속도 감소
        Blizzard     // 폭설 — 성장 정지 + 이동속도 크게 감소 + 시야 축소
    }
}
```

> canonical 날씨 종류 정의는 `docs/systems/time-season.md` 섹션 3.1이다. 7종 날씨(Clear/Cloudy/Rain/HeavyRain/Storm/Snow/Blizzard) 모두 여기서 관리된다.

---

## 2. 클래스 설계

### 2.1 클래스 다이어그램

```
┌─────────────────────────────────────────────────────────────────────┐
│                         SeedMind.Core                               │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│                    TimeManager (MonoBehaviour, Singleton)      │
│──────────────────────────────────────────────────────────────│
│  [직렬화 필드]                                                │
│  - _currentYear: int (1~)                                    │
│  - _currentSeason: Season                                    │
│  - _currentDay: int (1~28)                                   │
│  - _currentHour: float (6.0~24.0)                            │
│  - _currentDayPhase: DayPhase                                │
│  - _timeScale: float (default 1.0)                           │
│  - _isPaused: bool                                           │
│                                                              │
│  [설정 참조]                                                   │
│  - _timeConfig: TimeConfig (ScriptableObject)                │
│  - _seasonDataSet: SeasonData[] (4개, 계절별)                  │
│                                                              │
│  [읽기 전용 프로퍼티]                                           │
│  + CurrentYear: int                                          │
│  + CurrentSeason: Season                                     │
│  + CurrentDay: int                                           │
│  + CurrentHour: float                                        │
│  + CurrentDayPhase: DayPhase                                 │
│  + DaysInSeason: int (= 28, TimeConfig에서)                   │
│  + IsPaused: bool                                            │
│  + CurrentSeasonData: SeasonData                             │
│  + TotalElapsedDays: int (= (year-1)*112 + season*28 + day) │
│                                                              │
│  [이벤트]                                                     │
│  + OnHourChanged: Action<int>           // oldHour → newHour │
│  + OnDayPhaseChanged: Action<DayPhase>                       │
│  + OnDayChanged: Action<int>            // newDay            │
│  + OnSeasonChanged: Action<Season>      // newSeason         │
│  + OnYearChanged: Action<int>           // newYear           │
│  + OnSleepCompleted: Action             // 수면 완료 (SkipToNextDay 내부 발행, OnDayChanged 이전) │
│                                                              │
│  [메서드]                                                     │
│  + Initialize(): void                                        │
│  - Update(): void        // 매 프레임 시간 진행               │
│  - AdvanceTime(float deltaTime): void                        │
│  - AdvanceHour(): void                                       │
│  - AdvanceDay(): void                                        │
│  - AdvanceSeason(): void                                     │
│  - AdvanceYear(): void                                       │
│  - UpdateDayPhase(): void                                    │
│  + SetTimeScale(float scale): void                           │
│  + Pause() / Resume(): void                                  │
│  + SkipToNextDay(): void  // 디버그/수면용 → OnSleepCompleted 발행 후 OnDayChanged 발행 │
│  + GetSaveData(): TimeSaveData                               │
│  + LoadSaveData(TimeSaveData data): void                     │
└──────────────────────────────────────────────────────────────┘
         │ events                          │ ref
         ▼                                 ▼
┌────────────────────┐          ┌──────────────────────────┐
│  WeatherSystem     │          │  TimeConfig (SO)         │
│  (MonoBehaviour)   │          │──────────────────────────│
│                    │          │  (필드 및 수치:           │
│ (아래 2.3 참조)    │          │   → see 섹션 2.2)        │
│                    │          │                          │
└────────────────────┘          └──────────────────────────┘
         │ events
         ▼
┌────────────────────┐
│  FestivalManager   │
│  (아래 2.5 참조)   │
└────────────────────┘

         ┌──────────────────────────────────────────┐
         │         SeasonData (ScriptableObject)      │
         │──────────────────────────────────────────│
         │  season: Season                           │
         │  displayName: string ("봄", "여름"...)     │
         │                                           │
         │  [환경]                                    │
         │  sunColor: Color                          │
         │  sunIntensity: float                      │
         │  ambientColor: Color                      │
         │  fogColor: Color                          │
         │  fogDensity: float                        │
         │                                           │
         │  [시간대별 오버라이드]                       │
         │  phaseOverrides: DayPhaseVisual[] (5개)    │
         │                                           │
         │  [게임플레이]                               │
         │  growthSpeedMultiplier: float (1.0)        │
         │  shopPriceMultiplier: float (1.0)          │
         │  availableCropSeasons: SeasonFlag           │
         │                                           │
         │  [비주얼]                                   │
         │  terrainTintColor: Color                   │
         │  treePrefabOverride: GameObject             │
         │  particleEffect: GameObject (낙엽, 눈 등)  │
         └──────────────────────────────────────────┘

         ┌──────────────────────────────────────────┐
         │      DayPhaseVisual (Serializable class)   │
         │──────────────────────────────────────────│
         │  phase: DayPhase                          │
         │  lightColor: Color                        │
         │  lightIntensity: float                    │
         │  lightRotation: Vector3 (태양 각도)        │
         │  ambientColor: Color                      │
         │  transitionDuration: float (초, 보간 시간) │
         └──────────────────────────────────────────┘
```

### 2.2 TimeConfig ScriptableObject

시간 시스템의 모든 수치를 외부화한다. 밸런스 조정 시 코드 변경 없이 SO만 수정하면 된다.

```csharp
namespace SeedMind.Core
{
    [CreateAssetMenu(fileName = "TimeConfig", menuName = "SeedMind/TimeConfig")]
    public class TimeConfig : ScriptableObject
    {
        // canonical 수치 출처: 이 섹션(time-season-architecture.md 섹션 2.2)
        [Header("시간 진행")]
        public float secondsPerGameHour = 33.33f;  // 실시간 33초 = 게임 내 1시간 // → canonical (this section)
        public int dayStartHour = 6;               // 하루 시작 시각               // → canonical (this section)
        public int dayEndHour = 24;                 // 하루 종료 시각 (자동 다음 날) // → canonical (this section)

        [Header("달력")]
        public int daysPerSeason = 28;              // 1계절 = 28일                 // → canonical (this section)
        public int seasonsPerYear = 4;              // 1년 = 4계절                  // → canonical (this section)

        [Header("배속")]
        public float defaultTimeScale = 1.0f;       // 기본 배속                    // → canonical (this section)
        public float maxTimeScale = 3.0f;            // 최대 배속                   // → canonical (this section)

        // 파생 값 (읽기 전용)
        public int DaysPerYear => daysPerSeason * seasonsPerYear; // 112
        public float RealSecondsPerDay => secondsPerGameHour * (dayEndHour - dayStartHour); // ~600초 = 10분
    }
}
```

### 2.3 WeatherSystem

```
┌──────────────────────────────────────────────────────────────┐
│              WeatherSystem (MonoBehaviour)                     │
│──────────────────────────────────────────────────────────────│
│  [상태]                                                       │
│  - _currentWeather: WeatherType                              │
│  - _tomorrowWeather: WeatherType  (내일 예보, 선결정)          │
│  - _weatherSeed: int              (결정론적 난수 시드)         │
│  - _rng: System.Random            (시드 기반 난수 생성기)      │
│                                                              │
│  [설정 참조]                                                   │
│  - _weatherDataSet: WeatherData[] (4개, 계절별)               │
│                                                              │
│  [읽기 전용 프로퍼티]                                           │
│  + CurrentWeather: WeatherType                               │
│  + TomorrowWeather: WeatherType (내일 예보)                    │
│  + IsRaining: bool (Rain || HeavyRain || Storm)               │
│                                                              │
│  [이벤트]                                                     │
│  + OnWeatherChanged: Action<WeatherType>                     │
│                                                              │
│  [메서드]                                                     │
│  + OnEnable(): TimeManager.OnDayChanged 구독                  │
│  + OnDisable(): 구독 해제                                      │
│  - DetermineWeather(Season season): WeatherType               │
│  - ProcessDayWeather(int newDay): void                        │
│  + ApplyWeatherEffects(): void                                │
│  + GetSaveData(): WeatherSaveData                             │
│  + LoadSaveData(WeatherSaveData data): void                   │
│  + SetWeatherSeed(int seed): void                             │
└──────────────────────────────────────────────────────────────┘
```

### 2.4 WeatherData ScriptableObject

날씨 종류는 7종(`Clear/Cloudy/Rain/HeavyRain/Storm/Snow/Blizzard`)이며, canonical 정의는 `docs/systems/time-season.md` 섹션 3.1이다.

```csharp
namespace SeedMind.Core
{
    [CreateAssetMenu(fileName = "NewWeatherData", menuName = "SeedMind/WeatherData")]
    public class WeatherData : ScriptableObject
    {
        public Season season;

        // 확률 수치 canonical: docs/systems/time-season.md 섹션 3.2
        [Header("확률 (합계 = 1.0) — canonical: docs/systems/time-season.md 섹션 3.2")]
        public float clearChance;     // Clear (맑음)    // → see docs/systems/time-season.md 섹션 3.2
        public float cloudyChance;    // Cloudy (흐림)   // → see docs/systems/time-season.md 섹션 3.2
        public float rainChance;      // Rain (비)       // → see docs/systems/time-season.md 섹션 3.2
        public float heavyRainChance; // HeavyRain (폭우) // → see docs/systems/time-season.md 섹션 3.2
        public float stormChance;     // Storm (폭풍)    // → see docs/systems/time-season.md 섹션 3.2
        public float snowChance;      // Snow (눈) — 겨울 전용    // → see docs/systems/time-season.md 섹션 3.2
        public float blizzardChance;  // Blizzard (폭설) — 겨울 전용 // → see docs/systems/time-season.md 섹션 3.2

        // 연속 보정 수치 canonical: 이 섹션 (time-season-architecture.md 섹션 2.4)
        [Header("연속 날씨 보정")]
        public int maxConsecutiveSameWeatherDays = 3;  // 동일 날씨 최대 연속일    // → canonical (this section)
        public int maxConsecutiveExtremeWeatherDays = 2; // Storm/Blizzard 최대 연속일 // → canonical (this section)
        public float consecutivePenalty = 0.5f;         // 연속 시 확률 감소 배수  // → canonical (this section)

        // 날씨 효과 수치 canonical: 이 섹션 (time-season-architecture.md 섹션 2.4)
        [Header("날씨 효과")]
        public float rainGrowthBonus = 0.0f;        // 비 올 때 성장 보너스 (기본 0) // → canonical (this section)
        public float stormCropDamageChance = 0.1f;  // 폭풍 시 작물 피해 확률       // → canonical (this section)
        public float blizzardWitherChance = 0.05f;  // 폭설 시 작물 동사 확률       // → canonical (this section)
    }
}
```

**계절별 날씨 확률 기본값**: (-> see `docs/systems/time-season.md` 섹션 3.2 for canonical — 이 문서에 직접 기재 금지, PATTERN-006)

### 2.5 FestivalManager

```
┌──────────────────────────────────────────────────────────────┐
│           FestivalManager (MonoBehaviour)                      │
│──────────────────────────────────────────────────────────────│
│  [설정 참조]                                                   │
│  - _festivals: FestivalData[]                                 │
│                                                              │
│  [상태]                                                       │
│  - _activeFestival: FestivalData (nullable)                   │
│                                                              │
│  [이벤트]                                                     │
│  + OnFestivalStarted: Action<FestivalData>                    │
│  + OnFestivalEnded: Action<FestivalData>                      │
│                                                              │
│  [메서드]                                                     │
│  + OnEnable(): TimeManager.OnDayChanged 구독                  │
│  - CheckFestival(Season season, int day): void                │
│  + GetActiveFestival(): FestivalData                          │
│  + IsFestivalDay(): bool                                      │
└──────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│           FestivalData (ScriptableObject)                      │
│──────────────────────────────────────────────────────────────│
│  - festivalName: string ("수확 축제", "꽃 축제" 등)            │
│  - festivalId: string                                        │
│  - season: Season                                            │
│  - day: int (해당 계절의 몇 일차)                               │
│  - description: string                                       │
│  - shopDiscountRate: float (0 ~ 0.5)                         │
│  - specialCropBonus: CropData (해당 축제의 보너스 작물)         │
│  - bonusMultiplier: float (1.5 등)                            │
│  - dialogueKey: string (축제 대사 키)                          │
└──────────────────────────────────────────────────────────────┘
```

**기본 축제 목록**: (-> see `docs/systems/time-season.md` 섹션 4.2 for canonical — 이 문서에 직접 기재 금지, PATTERN-006)

---

## 3. 시간 진행 로직

### 3.1 Update 루프

TimeManager의 Update에서 매 프레임 시간을 진행한다. `Time.deltaTime` 기반으로 실시간과 게임 시간을 변환한다.

```csharp
// illustrative pseudocode
private void Update()
{
    if (_isPaused) return;

    float scaledDelta = Time.deltaTime * _timeScale;
    float hoursToAdd = scaledDelta / _timeConfig.secondsPerGameHour;

    float previousHour = _currentHour;
    _currentHour += hoursToAdd;

    // 정수 시간 경계 통과 체크
    int prevHourInt = Mathf.FloorToInt(previousHour);
    int currHourInt = Mathf.FloorToInt(_currentHour);

    if (currHourInt > prevHourInt)
    {
        for (int h = prevHourInt + 1; h <= currHourInt; h++)
        {
            OnHourChanged?.Invoke(h);
        }
        UpdateDayPhase();
    }

    // 하루 종료 체크
    if (_currentHour >= _timeConfig.dayEndHour)
    {
        _currentHour = _timeConfig.dayEndHour;  // 초과 방지
        AdvanceDay();
    }
}
```

**핵심 설계 결정**:
- 시간은 `float`으로 연속 진행하되, 이벤트는 정수 시간 경계에서만 발행한다
- 하루 종료(24:00)에 도달하면 즉시 AdvanceDay를 호출하고, 다음 프레임에서 6:00부터 재개한다
- 배속(timeScale)은 `Time.deltaTime`에 곱하는 방식으로 구현하여 Unity의 `Time.timeScale`과 독립적이다

### 3.2 시간대 전환

```csharp
private void UpdateDayPhase()
{
    DayPhase newPhase;
    int hour = Mathf.FloorToInt(_currentHour);

    if (hour < 8)       newPhase = DayPhase.Dawn;
    else if (hour < 12) newPhase = DayPhase.Morning;
    else if (hour < 17) newPhase = DayPhase.Afternoon;
    else if (hour < 20) newPhase = DayPhase.Evening;
    else                newPhase = DayPhase.Night;

    if (newPhase != _currentDayPhase)
    {
        _currentDayPhase = newPhase;
        OnDayPhaseChanged?.Invoke(newPhase);
    }
}
```

### 3.3 하루 종료 -> 다음 날 전환 순서

하루 종료 시 여러 시스템이 순차적으로 처리되어야 한다. 이 순서가 **게임 로직의 정확성에 매우 중요**하다.

```
_currentHour >= 24
    │
    ▼
TimeManager.AdvanceDay()
    │
    ├── 1) _currentDay++
    │      _currentHour = dayStartHour (6.0)
    │
    ├── 2) 계절 전환 체크
    │      if (_currentDay > daysPerSeason):
    │          _currentDay = 1
    │          AdvanceSeason()  ──▶  OnSeasonChanged 발행
    │                               (연도 전환 포함 시 OnYearChanged도 발행)
    │
    └── 3) OnDayChanged 발행  ──▶  구독자 실행 (섹션 4 참조)
```

**중요**: `OnSeasonChanged`는 `OnDayChanged`보다 **먼저** 발행된다. 이는 OnDayChanged 구독자들이 이미 새 계절 정보를 참조할 수 있도록 하기 위함이다.

### 3.4 계절 전환 처리 순서

```
TimeManager.AdvanceSeason()
    │
    ├── 1) _currentSeason = (Season)(((int)_currentSeason + 1) % 4)
    │
    ├── 2) 연도 전환 체크
    │      if (_currentSeason == Season.Spring):
    │          _currentYear++
    │          OnYearChanged?.Invoke(_currentYear)
    │
    └── 3) OnSeasonChanged?.Invoke(_currentSeason)
```

---

## 4. 이벤트 처리 순서 (Event Execution Order)

### 4.1 문제 정의

Unity의 C# 이벤트(`Action`)는 구독 순서에 따라 호출된다. 그러나 구독 순서는 `OnEnable` 호출 순서에 의존하며, 이는 씬 로드 순서와 Script Execution Order에 따라 달라진다. 여러 시스템이 `OnDayChanged`에 구독하면 실행 순서가 비결정론적이 될 수 있다.

### 4.2 해결: 우선순위 기반 이벤트 디스패처

직접 `Action` 이벤트를 구독하는 대신, TimeManager가 **우선순위 기반 콜백 리스트**를 관리한다.

```csharp
// illustrative
public class TimeManager : MonoBehaviour
{
    private SortedList<int, Action<int>> _dayChangedCallbacks = new();

    /// priority가 낮을수록 먼저 실행 (0 = 최우선)
    public void RegisterOnDayChanged(int priority, Action<int> callback)
    {
        _dayChangedCallbacks.Add(priority, callback);
    }

    public void UnregisterOnDayChanged(Action<int> callback)
    {
        // SortedList에서 해당 callback 제거
    }

    private void FireOnDayChanged(int newDay)
    {
        foreach (var kvp in _dayChangedCallbacks)
        {
            kvp.Value.Invoke(newDay);
        }
    }
}
```

### 4.3 OnDayChanged 구독자 실행 순서

| 우선순위 | 시스템 | 처리 내용 | 근거 |
|----------|--------|-----------|------|
| **0** | WeatherSystem | 오늘 날씨 확정, 내일 예보 결정 | 날씨가 다른 시스템(물주기, 성장)에 영향 |
| **10** | GrowthSystem | 작물 성장 배치 처리, 고사/수확 판정 | 날씨 결과(비 = 자동 물주기) 반영 필요 |
| **20** | FarmGrid | 타일 시각 갱신, 경작지 퇴화 처리 | 성장 결과 반영 후 시각 업데이트 |
| **30** | FestivalManager | 축제 시작/종료 체크 | 날짜 확정 후 판정 |
| **40** | EconomyManager | 계절/축제 가격 변동 반영 | 축제 정보 필요 |
| **50** | SaveManager | 자동 저장 | 모든 시스템 처리 완료 후 |
| **90** | HUDController | UI 갱신 (날짜, 계절, 날씨 표시) | 최종 상태 반영 |

### 4.4 OnSeasonChanged 구독자 실행 순서

| 우선순위 | 시스템 | 처리 내용 |
|----------|--------|-----------|
| **0** | TimeManager(자체) | SeasonData 교체, 환경 설정 적용 |
| **10** | GrowthSystem | 계절 부적합 작물 고사 처리 (-> crop-growth-architecture.md 4.4) |
| **20** | WeatherSystem | 새 계절의 WeatherData로 교체 |
| **30** | EconomyManager | 계절별 가격 테이블 교체 |
| **90** | HUDController | 계절 UI 갱신 |

### 4.5 레이스 컨디션 방지 전략

1. **우선순위 숫자 간격 10**: 새 시스템 삽입 시 사이에 끼울 수 있도록 여유 확보
2. **동일 우선순위 금지**: RegisterOnDayChanged에서 중복 priority가 들어오면 `Debug.LogError`로 경고
3. **이벤트 발행 순서 문서화**: 이 섹션(4.3, 4.4)이 canonical 참조점
4. **단일 프레임 보장**: 모든 OnDayChanged 콜백은 동일 프레임 내에서 동기적으로 실행. 코루틴이나 비동기 처리가 필요한 경우 콜백 내에서 시작하되, 논리적 상태 변경은 콜백 내에서 즉시 완료해야 한다.

[RISK] 우선순위 기반 시스템이 SortedList 키 충돌을 일으킬 수 있다. SortedList 대신 `List<(int priority, Action callback)>`을 사용하고 수동 정렬하는 방식이 더 안전할 수 있다. 구현 시 재검토 필요.

---

## 5. 날씨 시스템 아키텍처

### 5.1 날씨 결정 알고리즘 (Weighted Random with Correction)

```
ProcessDayWeather(int newDay):
    │
    ├── 1) 현재 날씨 = _tomorrowWeather (전날 예보한 값)
    │      OnWeatherChanged?.Invoke(_currentWeather)
    │
    ├── 2) 내일 날씨 결정:
    │      a) WeatherData에서 계절별 확률 로드
    │      b) 연속 보정 적용:
    │         if (동일 날씨 연속 >= maxConsecutiveRainyDays):
    │             해당 날씨 확률 *= consecutivePenalty
    │             나머지 확률 재정규화
    │      c) Weighted Random 선택 (_rng 사용, 결정론적)
    │
    └── 3) _tomorrowWeather = 결정된 날씨
```

**Weighted Random 구현** (illustrative):

```csharp
private WeatherType SelectWeather(WeatherData data)
{
    // WeatherType enum 순서(0~6): Clear, Cloudy, Rain, HeavyRain, Storm, Snow, Blizzard
    float[] weights = {
        data.clearChance,
        data.cloudyChance,
        data.rainChance,
        data.heavyRainChance,
        data.stormChance,
        data.snowChance,
        data.blizzardChance
    };

    // 연속 보정 적용
    ApplyConsecutiveCorrection(weights);

    // 정규화
    float total = weights.Sum();
    float roll = (float)_rng.NextDouble() * total;

    float cumulative = 0f;
    for (int i = 0; i < weights.Length; i++)
    {
        cumulative += weights[i];
        if (roll <= cumulative)
            return (WeatherType)i;
    }
    return WeatherType.Clear;  // fallback
}
```

### 5.2 날씨 -> 게임 시스템 연동

```
WeatherSystem.ApplyWeatherEffects()
    │
    ├── [Rain / HeavyRain / Storm]
    │   ├── FarmGrid: 모든 Planted/Dry 타일 자동 Watered 처리
    │   │   (물뿌리개 사용 효과와 동일)
    │   ├── GrowthSystem: rainGrowthBonus 적용 (현재 0, 향후 확장)
    │   └── UI: 비 파티클 이펙트 활성화
    │
    ├── [Storm 추가]
    │   └── GrowthSystem: 각 작물에 stormCropDamageChance 확률로 피해
    │       피해 = dryDayCount +1 (고사 가속)
    │
    ├── [Snow / Blizzard]
    │   ├── 야외 경작 불가 강화 (이미 겨울이므로 시각적 강조)
    │   └── UI: 눈 파티클 이펙트 활성화
    │
    ├── [Blizzard 추가]
    │   └── GrowthSystem: 각 작물에 blizzardWitherChance 확률로 즉시 Withered
    │
    └── [Clear / Cloudy]
        └── 특별한 게임 효과 없음 (시각적 차이만)
```

**비 오는 날 자동 물주기 흐름**:

```
WeatherSystem (priority 0, OnDayChanged)
    → _currentWeather == Rain or HeavyRain or Storm
    → FarmGrid.WaterAllPlantedTiles() 호출
        → 각 FarmTile: TryWater() 호출
        → FarmEvents.OnTileWatered 발행

GrowthSystem (priority 10, OnDayChanged)
    → 이미 물이 뿌려진 상태에서 성장 처리 진행
    → 정상적으로 Watered 타일의 성장 처리
```

이 순서가 보장되는 이유: WeatherSystem의 우선순위(0)가 GrowthSystem(10)보다 높기 때문이다.

### 5.3 결정론적 날씨 시드

날씨는 `System.Random`을 시드 기반으로 사용하여 재현 가능하다.

```
초기 시드 = 세이브 슬롯 생성 시 랜덤 배정
    또는 플레이어가 "시드" 입력 가능 (옵션)

_rng = new System.Random(weatherSeed)

매일 _rng.NextDouble()을 호출하므로,
동일 시드 + 동일 날짜 = 동일 날씨 보장
```

[RISK] 저장/로드 시 `System.Random`의 내부 상태를 직렬화할 수 없다. 대안: 로드 시 시드로 새 Random을 생성하고, 현재 날짜까지 `NextDouble()`를 반복 호출하여 상태를 재현한다. 총 플레이 일수가 수천 일에 이르면 로드 시간에 미미한 영향이 있을 수 있으나, 실측 무시 가능 수준이다.

---

## 6. 환경 시각 전환

### 6.1 조명 전환

TimeManager가 OnDayPhaseChanged를 발행하면, 환경 관리 시스템이 현재 SeasonData의 DayPhaseVisual을 참조하여 Directional Light와 앰비언트를 보간한다.

```
OnDayPhaseChanged(DayPhase newPhase)
    │
    ├── SeasonData.phaseOverrides[newPhase] 참조
    │
    ├── DOTween 또는 코루틴으로 보간 (transitionDuration 초)
    │   ├── Directional Light color → phaseVisual.lightColor
    │   ├── Directional Light intensity → phaseVisual.lightIntensity
    │   ├── Directional Light rotation → phaseVisual.lightRotation
    │   └── RenderSettings.ambientLight → phaseVisual.ambientColor
    │
    └── 날씨 오버레이 (비/눈 시 조명 추가 감쇄)
```

### 6.2 계절 전환

```
OnSeasonChanged(Season newSeason)
    │
    ├── SeasonData 교체
    │
    ├── 지형 색상 변경 (terrainTintColor)
    │   → 모든 지형 머티리얼의 _BaseColor에 tint 적용
    │
    ├── 나무 프리팹 교체 (treePrefabOverride)
    │   → 봄(꽃), 여름(녹색), 가을(단풍), 겨울(빈 가지)
    │
    └── 파티클 이펙트 전환 (particleEffect)
        → 가을: 낙엽 파티클
        → 겨울: 눈 파티클
```

---

## 7. 저장/로드

### 7.1 TimeSaveData

```csharp
namespace SeedMind.Core
{
    [System.Serializable]
    public class TimeSaveData
    {
        public int year;
        public int seasonIndex;   // (int)Season
        public int day;
        public float hour;
        public int dayPhaseIndex; // (int)DayPhase
        public float timeScale;
    }
}
```

### 7.2 WeatherSaveData

```csharp
namespace SeedMind.Core
{
    [System.Serializable]
    public class WeatherSaveData
    {
        public int weatherSeed;           // 초기 시드 (재현용)
        public int currentWeatherIndex;   // (int)WeatherType
        public int tomorrowWeatherIndex;  // (int)WeatherType
        public int consecutiveSameWeatherDays;  // 연속 동일 날씨 카운트
        public int totalElapsedDays;      // 난수 상태 재현을 위한 총 경과 일수
    }
}
```

### 7.3 저장/로드 흐름

```
저장:
    SaveManager.Save()
        ├── TimeManager.GetSaveData() → TimeSaveData
        ├── WeatherSystem.GetSaveData() → WeatherSaveData
        └── JSON 직렬화 → 파일 저장

로드:
    SaveManager.Load()
        ├── JSON 파싱
        ├── TimeManager.LoadSaveData(timeSaveData)
        │   → 모든 필드 복원, DayPhase 재계산
        └── WeatherSystem.LoadSaveData(weatherSaveData)
            → 시드로 new System.Random 생성
            → totalElapsedDays만큼 NextDouble() 호출 (상태 재현)
            → currentWeather, tomorrowWeather 복원
```

---

## 8. MCP 구현 계획

TimeManager/WeatherSystem을 MCP for Unity를 통해 단계적으로 구축하는 태스크 시퀀스. 상세 태스크는 `docs/mcp/time-season-tasks.md` (ARC-021)에 분리 문서화 완료.

### Phase A: TimeManager 기본 (MCP 5단계)

```
Step A-1: Scripts/Core/TimeManager.cs 작성
          → 필드, 프로퍼티, 이벤트 선언
          → Update 루프 시간 진행 로직

Step A-2: Data/Core/ 폴더에 TimeConfig SO 생성
          → SO_TimeConfig 생성 및 필드 값 설정
          → 수치: (-> see 섹션 2.2 TimeConfig 코드 블록)
          // PATTERN-006: 수치 직접 기재 금지 — 섹션 2.2가 canonical 출처

Step A-3: SCN_Farm 씬에 GameObject "TimeSystem" 생성
          → TimeManager.cs 컴포넌트 부착
          → _timeConfig 필드에 SO_TimeConfig 참조 연결
          → DontDestroyOnLoad 설정

Step A-4: Play Mode 진입 → Console 로그 확인
          → 매 정수 시간마다 "Hour: N" 로그 출력
          → 24:00 도달 시 "Day Changed: N" 로그 출력
          → 28일 경과 시 "Season Changed: Season" 로그 출력

Step A-5: HUDController에 시간/날짜/계절 텍스트 연결
          → OnHourChanged → HUD 시간 갱신
          → OnDayChanged → HUD 날짜 갱신
          → OnSeasonChanged → HUD 계절 갱신
```

### Phase B: SeasonData 환경 연출 (MCP 4단계)

```
Step B-1: SeasonData SO 4개 생성 (-> docs/mcp/time-season-tasks.md Phase B Step B-1 상세)
          → SO_Season_Spring / SO_Season_Summer / SO_Season_Autumn / SO_Season_Winter 생성
          → sunColor, growthSpeedMultiplier 등 수치: (-> see docs/systems/time-season.md 섹션 2.2~2.3)
          // PATTERN-006: 수치 직접 기재 금지 — canonical 문서에서 MCP 실행 시점에 읽어 입력

Step B-2: DayPhaseVisual 데이터 각 SeasonData에 설정 (5개 시간대 x 4계절 = 20 세트)
          → 조명 색상, 강도, 태양 각도 값 입력

Step B-3: 환경 전환 로직 스크립트 작성 (EnvironmentController.cs)
          → OnDayPhaseChanged 구독 → 조명 보간
          → OnSeasonChanged 구독 → 지형 색상/파티클 전환

Step B-4: Play Mode 테스트
          → 시간 진행에 따른 조명 변화 확인
          → SkipToNextDay() 반복 호출로 계절 전환 확인
```

### Phase C: WeatherSystem (MCP 5단계)

```
Step C-1: Scripts/Core/WeatherSystem.cs 작성
          → 날씨 결정 알고리즘 (Weighted Random)
          → 이벤트 발행 로직

Step C-2: WeatherData SO 4개 생성 (계절별)
          → SO_Weather_Spring ~ SO_Weather_Winter
          → 확률 값 설정 (섹션 2.4 테이블 참조)

Step C-3: WeatherSystem을 "TimeSystem" GameObject에 부착
          → _weatherDataSet 배열에 SO 4개 연결
          → TimeManager.RegisterOnDayChanged(priority: 0) 호출

Step C-4: 비 효과 연동
          → Rain/HeavyRain/Storm 시 FarmGrid.WaterAllPlantedTiles() 호출
          → Storm 시 작물 피해 로직 추가 (stormCropDamageChance)
          → Blizzard 시 작물 동사 로직 추가 (blizzardWitherChance)
          → HUD에 날씨 아이콘 표시 (7종 아이콘)

Step C-5: Play Mode 테스트
          → 날씨 변화 Console 로그 확인
          → 비 오는 날 자동 물주기 확인
          → 시드 고정 후 재시작 시 동일 날씨 순서 확인
```

### Phase D: FestivalManager (MCP 3단계)

```
Step D-1: Scripts/Core/FestivalManager.cs 작성
          → OnDayChanged 구독, 축제 판정

Step D-2: FestivalData SO 4개 생성 (-> docs/systems/time-season.md 섹션 4.2)
          → SO_Festival_SpringSeed, SO_Festival_SummerFireworks,
            SO_Festival_AutumnHarvest, SO_Festival_WinterStarlight
          // 날짜·이름·효과 수치: (-> see docs/systems/time-season.md 섹션 4.2) — 직접 기재 금지 (PATTERN-006)

Step D-3: Play Mode 테스트
          → 축제 날짜에 이벤트 발행 확인
          → 할인/보너스 로직 확인
```

### Phase E: 통합 테스트 (MCP 2단계)

```
Step E-1: 전체 시스템 연동 테스트
          → 시간 진행 → 날씨 결정 → 비 시 물주기 → 성장 처리
          → 계절 전환 → 부적합 작물 고사
          → OnDayChanged 실행 순서 Console 로그 확인

Step E-2: 저장/로드 테스트
          → 중간 상태 저장 → 재로드 후 동일 시간/날씨 확인
          → 날씨 시드 재현 확인
```

---

## 9. 성능 고려사항

### 9.1 시간 업데이트 비용

- Update()에서의 시간 진행: float 연산 1~2회. **성능 영향 무시 가능**.
- OnHourChanged 이벤트: 게임 내 1시간마다 발행 (실시간 ~33초 간격). 구독자 수가 적으므로 부담 없음.
- OnDayChanged 이벤트: ~10분마다 1회. 모든 구독자가 동기적으로 실행되므로 **이 시점에 프레임 스파이크 가능**.

### 9.2 OnDayChanged 프레임 스파이크 대응

OnDayChanged 콜백 체인 (날씨 결정 + 전체 타일 성장 처리 + 시각 갱신)이 단일 프레임에서 실행된다. 최대 그리드 16x16 = 256 타일이므로 현재 규모에서는 문제없으나, 확장 시 대비책:

- **프리팹 교체 분산**: GrowthSystem의 시각적 프리팹 교체를 코루틴으로 수 프레임에 분산 (-> crop-growth-architecture.md 섹션 5 참조)
- **논리적 처리는 즉시, 시각적 갱신은 분산**: 상태 변경은 OnDayChanged 프레임에서 완료하되, 메시/머티리얼 변경은 다음 수 프레임에 걸쳐 처리

### 9.3 날씨 전환 시 환경 변경 비용

- 조명 보간: DOTween/코루틴으로 수십 프레임에 걸쳐 진행. 프레임당 비용 극소.
- 계절 전환 시 머티리얼 변경: 모든 지형 타일의 머티리얼 프로퍼티 변경. 256 타일 x MaterialPropertyBlock 방식 사용 시 DrawCall 증가 없음.
- 파티클 이펙트: 계절/날씨별 파티클은 오브젝트 풀링 또는 사전 생성 후 활성화/비활성화 방식.

[RISK] 계절 전환 시 나무 프리팹 일괄 교체가 GC 스파이크를 일으킬 수 있다. 오브젝트 풀링 또는 머티리얼 swap 방식(프리팹 교체 대신 머티리얼만 변경)으로 대응 검토 필요.

---

## 10. 프로젝트 구조 확장

이 시스템으로 인해 추가되는 파일:

```
Assets/_Project/
├── Scripts/Core/
│   ├── TimeManager.cs            # 기존 계획, 이 문서에서 상세화
│   ├── WeatherSystem.cs          # 신규
│   ├── FestivalManager.cs        # 신규
│   └── EnvironmentController.cs  # 신규 (조명/환경 시각 관리)
│
├── Data/Core/
│   ├── SO_TimeConfig.asset        # TimeConfig SO
│   ├── Seasons/
│   │   ├── SO_Season_Spring.asset
│   │   ├── SO_Season_Summer.asset
│   │   ├── SO_Season_Autumn.asset
│   │   └── SO_Season_Winter.asset
│   ├── Weather/
│   │   ├── SO_Weather_Spring.asset
│   │   ├── SO_Weather_Summer.asset
│   │   ├── SO_Weather_Autumn.asset
│   │   └── SO_Weather_Winter.asset
│   └── Festivals/
│       ├── SO_Festival_SpringSeed.asset
│       ├── SO_Festival_SummerFireworks.asset
│       ├── SO_Festival_AutumnHarvest.asset
│       └── SO_Festival_WinterStarlight.asset
```

---

## Open Questions

- [OPEN] 수면(Sleep) 메카닉: 밤에 침대에서 잠자면 즉시 다음 날 6:00으로 건너뛰는 기능. `SkipToNextDay()` 내부에서 `OnSleepCompleted` → `OnDayChanged` 순서로 발행. 수면 중 체력/스태미나 회복 로직 필요 여부는 미결.
- [OPEN] 시간 일시정지 상황: 대화, 상점 UI, 인벤토리 열람 시 시간을 멈출지, 계속 진행할지. 현재 설계는 Pause()/Resume()으로 대응 가능.
- [OPEN] 날씨 예보 UI: 내일 날씨(_tomorrowWeather)를 TV/라디오 같은 인게임 오브젝트로 보여줄지, HUD에 직접 표시할지.
- [OPEN] 시간 배속 UI: 플레이어에게 1x/2x/3x 배속 전환을 허용할지. SetTimeScale()로 구현 가능하나 UX 설계 필요.

## Risks

- [RISK] 우선순위 기반 이벤트 디스패처의 SortedList 키 충돌 (섹션 4.5 참조) -- List + 수동 정렬로 대안 검토
- [RISK] System.Random 상태 직렬화 불가 (섹션 5.3 참조) -- 로드 시 시드 + 반복 호출로 재현
- [RISK] 계절 전환 시 프리팹 일괄 교체 GC 스파이크 (섹션 9.3 참조) -- 오브젝트 풀링 또는 머티리얼 swap 방식 검토
- [RISK] OnDayChanged 콜백 체인이 길어지면 단일 프레임 스파이크 발생 가능 -- 시각적 갱신 분산으로 대응
- [RISK] DayPhaseVisual 데이터가 20세트(5시간대 x 4계절)로 많아 MCP로 수동 입력이 번거로울 수 있음 -- 템플릿 SO를 만들고 계절별로 복사/수정하는 전략 필요

---

## Cross-references

- `docs/architecture.md` 4.3절 (시간 시스템 개요)
- `docs/design.md` 4.3절 (시간/계절 시스템 게임 설계)
- `docs/systems/time-season.md` (시간/계절/날씨 게임 디자인 상세, canonical 날씨 종류/확률/축제 목록)
- `docs/systems/farming-architecture.md` (GrowthSystem의 OnDayChanged 구독, 일일 성장 흐름)
- `docs/systems/crop-growth-architecture.md` (계절 전환 시 SeasonalWither 처리, 성장 공식의 seasonBonus)
- `docs/systems/project-structure.md` (네임스페이스 SeedMind.Core, 의존성 방향)
- `docs/mcp/time-season-tasks.md` (상세 MCP 태스크 시퀀스, ARC-021)
- `docs/balance/weather.md` (계절별 날씨 확률 밸런스, 작성 예정)
- `docs/mcp/tutorial-tasks.md` (Step 07: `TimeManager.OnSleepCompleted` 구독 — FIX-025)
- `docs/mcp/save-load-tasks.md` (T-6: `TimeManager.OnDayChanged`/`OnSeasonChanged` 자동저장 트리거 — ARC-012)

---

*이 문서는 Claude Code가 기존 아키텍처 문서와의 일관성을 검증하며 자율적으로 작성했습니다.*
