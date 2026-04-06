# 시간/계절 시스템 MCP 태스크 시퀀스

> TimeManager/WeatherSystem/FestivalManager Unity 구현을 위한 MCP for Unity 태스크 실행 순서. `time-season-architecture.md` 섹션 8 기반으로 상세화.
> 작성: Claude Code (Sonnet 4.6) | 2026-04-07
> 문서 ID: ARC-021 | Phase 1

---

## 1. 개요

이 문서는 `docs/systems/time-season-architecture.md`(섹션 8)에서 요약된 MCP 구현 계획(Phase A~E)을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 시간/계절 시스템의 데이터 레이어(TimeConfig SO 1종, SeasonData SO 4종, WeatherData SO 4종, FestivalData SO 4종), 시스템 레이어(스크립트 8종), 씬 배치, HUD 연결, 통합 테스트를 완성한다.

---

## 2. 전제 조건

### 2.1 완료된 태스크 의존성

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조, SCN_Farm 기본 계층 (MANAGERS, UI), Canvas_Overlay |
| ARC-003 | `docs/mcp/farming-tasks.md` | Phase A~C 전체 | FarmGrid, FarmTile, GrowthSystem, FarmEvents |

### 2.2 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `Canvas_Overlay` (UI 루트) | ARC-002 Phase B |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A |
| `FarmGrid` (GameObject + FarmGrid.cs) | ARC-003 |
| `GrowthSystem` (MonoBehaviour) | ARC-003 |
| `HUDController` (UI 컴포넌트) | ARC-002 Phase B |
| `SaveManager` | `docs/mcp/save-load-tasks.md` |

### 2.3 필요 C# 스크립트 목록

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 Phase 순서대로 작성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 Phase |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/Core/Data/TimeConfig.cs` | `TimeConfig` (ScriptableObject) | `SeedMind.Core` | Phase A |
| S-02 | `Scripts/Core/Data/TimeSaveData.cs` | `TimeSaveData` (Serializable) | `SeedMind.Core` | Phase A |
| S-03 | `Scripts/Core/TimeManager.cs` | `TimeManager` (MonoBehaviour, Singleton) | `SeedMind.Core` | Phase A |
| S-04 | `Scripts/Core/Data/SeasonData.cs` | `SeasonData` (ScriptableObject) | `SeedMind.Core` | Phase B |
| S-05 | `Scripts/Core/Data/DayPhaseVisual.cs` | `DayPhaseVisual` (Serializable class) | `SeedMind.Core` | Phase B |
| S-06 | `Scripts/Core/EnvironmentController.cs` | `EnvironmentController` (MonoBehaviour) | `SeedMind.Core` | Phase B |
| S-07 | `Scripts/Core/Data/WeatherData.cs` | `WeatherData` (ScriptableObject) | `SeedMind.Core` | Phase C |
| S-08 | `Scripts/Core/Data/WeatherSaveData.cs` | `WeatherSaveData` (Serializable) | `SeedMind.Core` | Phase C |
| S-09 | `Scripts/Core/WeatherSystem.cs` | `WeatherSystem` (MonoBehaviour) | `SeedMind.Core` | Phase C |
| S-10 | `Scripts/Core/Data/FestivalData.cs` | `FestivalData` (ScriptableObject) | `SeedMind.Core` | Phase D |
| S-11 | `Scripts/Core/FestivalManager.cs` | `FestivalManager` (MonoBehaviour) | `SeedMind.Core` | Phase D |

(모든 경로 접두어: `Assets/_Project/`)

**컴파일 순서**: S-01 ~ S-02 -> S-03 -> S-04 ~ S-05 -> S-06 -> S-07 ~ S-08 -> S-09 -> S-10 -> S-11. 각 Phase 사이에 Unity 컴파일 대기(`execute_menu_item`)가 필요하다.

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. TimeManager가 Singleton 베이스 클래스에 의존하므로, `Scripts/Core/Singleton.cs`가 선행 컴파일되어 있어야 한다 (-> see `docs/systems/project-structure.md` 섹션 1).

---

## 3. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 Phase |
|----------|------|-----------|
| `create_script` | C# 스크립트 생성 | A, B, C, D |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | A, B, C, D |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | A~E 전체 |
| `create_object` | 빈 GameObject 생성 | A |
| `add_component` | MonoBehaviour 컴포넌트 부착 | A, B, C, D |
| `set_parent` | 오브젝트 부모 설정 | A |
| `create_folder` | 에셋 폴더 생성 | A, B, C, D |
| `save_scene` | 씬 저장 | A, C, D, E |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | A, B, C, D, E |
| `execute_menu_item` | Unity 컴파일 대기, 에디터 명령 | A~D |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | A, B, C, D, E |

[RISK] `create_scriptable_object` 도구의 가용 여부 및 파라미터 형식 사전 검증 필요. SO 인스턴스 생성이 MCP에서 미지원인 경우 Editor 스크립트를 통한 우회 필요 (-> see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련).

---

## 4. Phase A: TimeManager 기본 (~20 MCP 호출)

### 전제 조건

- ARC-002 Phase A, B 전체 완료 (폴더 구조, SCN_Farm 기본 계층)
- `Scripts/Core/Singleton.cs` 컴파일 완료

### Step A-1: TimeManager.cs 스크립트 작성

**목표**: TimeManager 핵심 스크립트와 의존 데이터 클래스(TimeConfig, TimeSaveData) 생성.

#### A-1-01: TimeConfig SO 클래스

```
create_script
  path: "Assets/_Project/Scripts/Core/Data/TimeConfig.cs"
  // 클래스 설계: -> see docs/systems/time-season-architecture.md 섹션 2.2
  // 필드: secondsPerGameHour, dayStartHour, dayEndHour, daysPerSeason,
  //        seasonsPerYear, defaultTimeScale, maxTimeScale
  // 기본값: -> see docs/systems/time-season-architecture.md 섹션 2.2
```

- **MCP 호출**: 1회

#### A-1-02: TimeSaveData 직렬화 클래스

```
create_script
  path: "Assets/_Project/Scripts/Core/Data/TimeSaveData.cs"
  // 클래스 설계: -> see docs/systems/time-season-architecture.md 섹션 7.1
  // 필드: year, seasonIndex, day, hour, dayPhaseIndex, timeScale
```

- **MCP 호출**: 1회

#### A-1-03: TimeManager 본체

```
create_script
  path: "Assets/_Project/Scripts/Core/TimeManager.cs"
  // 클래스 설계: -> see docs/systems/time-season-architecture.md 섹션 2.1
  // 포함 항목:
  //   - 직렬화 필드 (_currentYear, _currentSeason, _currentDay, _currentHour, _currentDayPhase, _timeScale, _isPaused)
  //   - 설정 참조 (_timeConfig: TimeConfig, _seasonDataSet: SeasonData[])
  //   - 읽기 전용 프로퍼티
  //   - 우선순위 기반 이벤트 디스패처 (-> see 섹션 4.2)
  //   - Update 루프 (-> see 섹션 3.1)
  //   - AdvanceDay/AdvanceSeason/AdvanceYear (-> see 섹션 3.3~3.4)
  //   - UpdateDayPhase (-> see 섹션 3.2)
  //   - SetTimeScale, Pause/Resume, SkipToNextDay
  //   - GetSaveData/LoadSaveData
```

- **MCP 호출**: 1회

#### A-1-04: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

#### A-1 검증 체크리스트

- [ ] `Assets/_Project/Scripts/Core/Data/TimeConfig.cs` 존재, 컴파일 에러 없음
- [ ] `Assets/_Project/Scripts/Core/Data/TimeSaveData.cs` 존재, 컴파일 에러 없음
- [ ] `Assets/_Project/Scripts/Core/TimeManager.cs` 존재, 컴파일 에러 없음

**A-1 MCP 호출 합계**: 4회

### Step A-2: SO_TimeConfig 에셋 생성

**목표**: TimeConfig ScriptableObject 인스턴스를 생성하고 기본값을 설정한다.

**전제**: S-01(TimeConfig.cs) 컴파일 완료.

#### A-2-01: 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Core"
```

- **MCP 호출**: 1회

#### A-2-02: SO_TimeConfig 생성 및 필드 설정

```
create_scriptable_object
  type: "SeedMind.Core.TimeConfig"
  asset_path: "Assets/_Project/Data/Core/SO_TimeConfig.asset"

set_property  target: "SO_TimeConfig"
  secondsPerGameHour = 0.0     // (-> see docs/systems/time-season-architecture.md 섹션 2.2 for canonical)
  dayStartHour = 0              // (-> see docs/systems/time-season-architecture.md 섹션 2.2)
  dayEndHour = 0                // (-> see docs/systems/time-season-architecture.md 섹션 2.2)
  daysPerSeason = 0             // (-> see docs/systems/time-season-architecture.md 섹션 2.2)
  seasonsPerYear = 0            // (-> see docs/systems/time-season-architecture.md 섹션 2.2)
  defaultTimeScale = 0.0        // (-> see docs/systems/time-season-architecture.md 섹션 2.2)
  maxTimeScale = 0.0            // (-> see docs/systems/time-season-architecture.md 섹션 2.2)
```

> **주의**: 구체적 수치는 MCP 실행 시점에 `time-season-architecture.md` 섹션 2.2에서 읽어 입력한다. 본 문서에서 수치를 직접 기재하지 않는다 (PATTERN-006).

- **MCP 호출**: 1(생성) + 7(필드 설정) = 8회

#### A-2 검증 체크리스트

- [ ] `Assets/_Project/Data/Core/SO_TimeConfig.asset` 존재
- [ ] 모든 필드에 값이 설정됨
- [ ] 콘솔 에러 없음

**A-2 MCP 호출 합계**: 9회

### Step A-3: TimeSystem GameObject + 컴포넌트 배치

**목표**: SCN_Farm 씬에 TimeSystem GameObject를 생성하고 TimeManager 컴포넌트를 부착한다.

**전제**: S-03(TimeManager.cs) 컴파일 완료, SO_TimeConfig 에셋 존재.

#### A-3-01: TimeSystem GameObject 생성

```
create_object
  name: "TimeSystem"

set_parent
  target: "TimeSystem"
  parent: "--- MANAGERS ---"
```

- **MCP 호출**: 2회

#### A-3-02: TimeManager 컴포넌트 부착 및 참조 연결

```
add_component
  target: "TimeSystem"
  component: "SeedMind.Core.TimeManager"

set_property  target: "TimeSystem" component: "TimeManager"
  _timeConfig = "Assets/_Project/Data/Core/SO_TimeConfig.asset"    // SO 참조 연결
```

- **MCP 호출**: 2회

#### A-3-03: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

#### A-3 검증 체크리스트

- [ ] `--- MANAGERS ---` 하위에 `TimeSystem` GameObject 존재
- [ ] `TimeManager` 컴포넌트 부착 확인
- [ ] `_timeConfig` 필드에 SO_TimeConfig 참조 연결됨
- [ ] 콘솔 에러 없음

**A-3 MCP 호출 합계**: 5회

### Step A-4: Play Mode 검증 (로그 확인)

**목표**: TimeManager의 시간 진행이 정상 동작하는지 Console 로그로 검증한다.

#### A-4-01: Play Mode 진입 및 로그 확인

```
enter_play_mode

// 약 35초 대기 (실시간 33초 = 게임 내 1시간)

get_console_logs
  // 확인 항목:
  // - "Hour: 7" 로그 출력 (06:00 → 07:00 전환)
  // - DayPhase 전환 로그 ("DayPhase: Dawn → Morning")

exit_play_mode
```

- **MCP 호출**: 3회 (enter + get_logs + exit)

#### A-4 검증 체크리스트

- [ ] 시간 진행 로그 정상 출력
- [ ] DayPhase 전환 이벤트 발행 확인
- [ ] 콘솔 에러/경고 없음

**A-4 MCP 호출 합계**: 3회

### Step A-5: HUDController 연결

**목표**: 기존 HUDController에 TimeManager 이벤트를 구독하여 시간/날짜/계절 텍스트를 표시한다.

#### A-5-01: HUDController에 시간 표시 필드 참조 설정

```
set_property  target: "HUDController"
  // TimeManager의 OnHourChanged, OnDayChanged, OnSeasonChanged 구독
  // HUD 텍스트 필드에 현재 시간/날짜/계절 표시
  // 구체적 UI 구성: -> see docs/systems/time-season-architecture.md 섹션 8 Phase A Step A-5
```

> HUDController의 시간 표시 UI 요소(Text_Time, Text_Day, Text_Season)가 ARC-002에서 생성되어 있어야 한다. 미존재 시 `create_object` + `add_component<TextMeshProUGUI>`로 추가 필요.

- **MCP 호출**: ~3회 (참조 연결)

#### A-5 검증 체크리스트

- [ ] HUD에 현재 시간 표시
- [ ] HUD에 현재 날짜 표시
- [ ] HUD에 현재 계절 표시

**A-5 MCP 호출 합계**: ~3회

### Phase A MCP 호출 총계: ~24회

---

## 5. Phase B: SeasonData 환경 연출 (~35 MCP 호출)

### 전제 조건

- Phase A 전체 완료 (TimeManager 정상 동작)
- S-03(TimeManager.cs) 컴파일 완료

### Step B-1: SeasonData SO 4개 생성

**목표**: 4계절의 환경 데이터를 담는 SeasonData ScriptableObject 에셋 4개를 생성한다.

**전제**: S-04(SeasonData.cs), S-05(DayPhaseVisual.cs) 컴파일 완료.

#### B-1-01: 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/Core/Data/SeasonData.cs"
  // 클래스 설계: -> see docs/systems/time-season-architecture.md 섹션 2.1 (SeasonData 박스)
  // 필드: season, displayName, sunColor, sunIntensity, ambientColor, fogColor, fogDensity,
  //        phaseOverrides (DayPhaseVisual[]), growthSpeedMultiplier, shopPriceMultiplier,
  //        terrainTintColor, treePrefabOverride, particleEffect

create_script
  path: "Assets/_Project/Scripts/Core/Data/DayPhaseVisual.cs"
  // 클래스 설계: -> see docs/systems/time-season-architecture.md 섹션 2.1 (DayPhaseVisual 박스)
  // 필드: phase, lightColor, lightIntensity, lightRotation, ambientColor, transitionDuration
```

- **MCP 호출**: 2회

#### B-1-02: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

#### B-1-03: 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Core/Seasons"
```

- **MCP 호출**: 1회

#### B-1-04: SeasonData SO 4개 생성

반복 패턴 (Spring, Summer, Autumn, Winter 각 1개):

```
create_scriptable_object
  type: "SeedMind.Core.SeasonData"
  asset_path: "Assets/_Project/Data/Core/Seasons/SO_Season_Spring.asset"

set_property  target: "SO_Season_Spring"
  season = 0                           // Season.Spring (int cast)
  displayName = "봄"
  // 환경 값(sunColor, sunIntensity, ambientColor, fogColor, fogDensity, terrainTintColor):
  //   (-> see docs/systems/time-season.md 섹션 2.3 조명/색감 테이블)
  // growthSpeedMultiplier:
  //   (-> see docs/systems/time-season.md 섹션 2.2 성장 보너스)
  // shopPriceMultiplier:
  //   (-> see docs/systems/economy-system.md 계절별 가격 배수)
```

> 동일 패턴을 SO_Season_Summer (season=1, displayName="여름"), SO_Season_Autumn (season=2, displayName="가을"), SO_Season_Winter (season=3, displayName="겨울")에 반복 적용한다.

- **MCP 호출**: 4(생성) + 4 x 7(필드 설정) = 32회

> **주의**: sunColor, ambientColor, fogColor, terrainTintColor 등의 구체적 색상 코드는 MCP 실행 시점에 canonical 문서에서 읽어 입력한다 (PATTERN-006).

#### B-1 검증 체크리스트

- [ ] `Assets/_Project/Data/Core/Seasons/` 폴더 존재
- [ ] SO 에셋 4개: SO_Season_Spring, SO_Season_Summer, SO_Season_Autumn, SO_Season_Winter
- [ ] 각 에셋의 `season` 필드가 올바른 enum 값
- [ ] 콘솔 에러 없음

**B-1 MCP 호출 합계**: ~36회

[RISK] SeasonData 필드 수가 많아(12+ 필드) set_property 호출이 많다. SO 에셋 생성 시 필드를 일괄 설정하는 MCP 도구가 없다면, Editor 스크립트(CreateSeasonAssets.cs)를 통해 호출 수를 4회로 줄일 수 있다.

### Step B-2: DayPhaseVisual 데이터 설정 (20 세트)

**목표**: 각 SeasonData의 `phaseOverrides` 배열에 5개 DayPhaseVisual 데이터를 설정한다. (5 시간대 x 4 계절 = 20 세트)

#### B-2-01: 각 SeasonData SO에 DayPhaseVisual 배열 설정

```
// SO_Season_Spring.phaseOverrides[0] = Dawn 시간대 설정
set_property  target: "SO_Season_Spring"
  phaseOverrides[0].phase = 0          // DayPhase.Dawn
  phaseOverrides[0].lightColor = ...   // (-> see docs/systems/time-season-architecture.md 섹션 1.2 DayPhase 조명 테이블)
  phaseOverrides[0].lightIntensity = ...  // (-> see 동일)
  phaseOverrides[0].lightRotation = ...   // (-> see 동일)
  phaseOverrides[0].ambientColor = ...    // (-> see 동일)
  phaseOverrides[0].transitionDuration = ...  // (-> see 동일)

// phaseOverrides[1] = Morning, [2] = Afternoon, [3] = Evening, [4] = Night
// 동일 패턴 반복
```

> 4계절 x 5시간대 = 20 세트. 각 세트당 6개 필드 설정.

- **MCP 호출**: 20 세트 x ~6(필드) = ~120회

[RISK] DayPhaseVisual이 Serializable class이므로 `set_property`로 중첩 배열 내부 필드를 설정할 수 있는지 사전 검증 필요. 불가능한 경우 Editor 스크립트로 우회하여 ~120회를 ~4회(계절당 1회)로 감축할 수 있다. **이 Phase에서 가장 큰 리스크**이다.

> **대안 전략**: Editor 스크립트 `CreateSeasonPhaseData.cs`를 작성하여 `execute_menu_item`으로 실행. 스크립트 내부에서 20 세트의 DayPhaseVisual 데이터를 하드코딩하여 일괄 설정한다. 이 경우 MCP 호출은 1(스크립트 생성) + 1(실행) = 2회.

#### B-2 검증 체크리스트

- [ ] 각 SeasonData SO에 phaseOverrides 배열 크기 = 5
- [ ] 20 세트 모두 lightColor, lightIntensity, lightRotation, ambientColor, transitionDuration 설정 완료
- [ ] 콘솔 에러 없음

**B-2 MCP 호출 합계**: ~120회 (또는 대안 전략 시 ~2회)

### Step B-3: EnvironmentController.cs 작성 + 배치

**목표**: 조명 보간 및 계절 환경 전환을 담당하는 EnvironmentController를 생성하고 씬에 배치한다.

**전제**: S-04(SeasonData.cs), S-05(DayPhaseVisual.cs) 컴파일 완료.

#### B-3-01: EnvironmentController 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/Core/EnvironmentController.cs"
  // 클래스 설계: -> see docs/systems/time-season-architecture.md 섹션 6
  // 기능:
  //   - OnDayPhaseChanged 구독 → Directional Light 색상/강도/회전 보간
  //   - OnSeasonChanged 구독 → 지형 tint, 나무 프리팹, 파티클 전환
  //   - Directional Light 참조 (Inspector 연결)
  //   - 보간 방식: 코루틴 또는 DOTween (transitionDuration 초)
```

- **MCP 호출**: 1회

#### B-3-02: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

#### B-3-03: TimeSystem GameObject에 EnvironmentController 부착

```
add_component
  target: "TimeSystem"
  component: "SeedMind.Core.EnvironmentController"

set_property  target: "TimeSystem" component: "EnvironmentController"
  // _directionalLight = Directional Light 참조
  // _timeManager = TimeSystem의 TimeManager 참조
```

- **MCP 호출**: 3회 (부착 + 참조 2개)

#### B-3-04: TimeManager에 SeasonData 배열 연결

```
set_property  target: "TimeSystem" component: "TimeManager"
  _seasonDataSet[0] = "Assets/_Project/Data/Core/Seasons/SO_Season_Spring.asset"
  _seasonDataSet[1] = "Assets/_Project/Data/Core/Seasons/SO_Season_Summer.asset"
  _seasonDataSet[2] = "Assets/_Project/Data/Core/Seasons/SO_Season_Autumn.asset"
  _seasonDataSet[3] = "Assets/_Project/Data/Core/Seasons/SO_Season_Winter.asset"
```

- **MCP 호출**: 4회

#### B-3-05: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

#### B-3 검증 체크리스트

- [ ] `TimeSystem` GameObject에 `EnvironmentController` 컴포넌트 부착됨
- [ ] Directional Light 참조 연결됨
- [ ] TimeManager._seasonDataSet에 SO 4개 연결됨
- [ ] 콘솔 에러 없음

**B-3 MCP 호출 합계**: 10회

### Step B-4: Play Mode 테스트

**목표**: 시간 진행에 따른 조명 변화와 계절 전환 환경 연출을 확인한다.

#### B-4-01: 조명 변화 테스트

```
enter_play_mode

// 시간대 전환 관찰 (Dawn → Morning 전환 시 조명 보간)
get_console_logs
  // 확인: "DayPhase changed: Dawn → Morning" + 조명 보간 시작 로그

exit_play_mode
```

- **MCP 호출**: 3회

#### B-4-02: 계절 전환 테스트

```
enter_play_mode

// SkipToNextDay() 반복 호출 (28회) → Season 전환 확인
// execute_menu_item 또는 Debug Console 명령으로 SkipToNextDay 실행

get_console_logs
  // 확인: "Season Changed: Spring → Summer" 로그
  // 확인: 조명/환경 색상 변경 로그

exit_play_mode
```

- **MCP 호출**: 3회

#### B-4 검증 체크리스트

- [ ] 시간대 전환 시 조명 보간 동작
- [ ] 계절 전환 시 환경 색상/파티클 전환 동작
- [ ] 콘솔 에러/경고 없음

**B-4 MCP 호출 합계**: 6회

### Phase B MCP 호출 총계: ~172회 (대안 전략 적용 시 ~54회)

[RISK] DayPhaseVisual 20 세트 설정(B-2)이 Phase B 호출 수의 대부분을 차지한다. **대안 전략(Editor 스크립트 일괄 설정)을 강력 권장**한다.

---

## 6. Phase C: WeatherSystem (~30 MCP 호출)

### 전제 조건

- Phase A 전체 완료 (TimeManager 정상 동작)
- Phase B의 SeasonData SO 4개 생성 완료
- ARC-003 FarmGrid/GrowthSystem 존재 (비 효과 연동용)

### Step C-1: WeatherSystem.cs 작성

**목표**: 날씨 결정 알고리즘, 이벤트 발행, 게임 시스템 연동 로직을 담은 WeatherSystem 스크립트를 생성한다.

#### C-1-01: WeatherData SO 클래스

```
create_script
  path: "Assets/_Project/Scripts/Core/Data/WeatherData.cs"
  // 클래스 설계: -> see docs/systems/time-season-architecture.md 섹션 2.4
  // 필드: season, clearChance, cloudyChance, rainChance, heavyRainChance,
  //        stormChance, snowChance, blizzardChance
  // 연속 보정 필드: maxConsecutiveSameWeatherDays, maxConsecutiveExtremeWeatherDays, consecutivePenalty
  // 날씨 효과 필드: rainGrowthBonus, stormCropDamageChance, blizzardWitherChance
  // 기본값: -> see docs/systems/time-season-architecture.md 섹션 2.4
```

- **MCP 호출**: 1회

#### C-1-02: WeatherSaveData 클래스

```
create_script
  path: "Assets/_Project/Scripts/Core/Data/WeatherSaveData.cs"
  // 클래스 설계: -> see docs/systems/time-season-architecture.md 섹션 7.2
  // 필드: weatherSeed, currentWeatherIndex, tomorrowWeatherIndex,
  //        consecutiveSameWeatherDays, totalElapsedDays
```

- **MCP 호출**: 1회

#### C-1-03: WeatherSystem 본체

```
create_script
  path: "Assets/_Project/Scripts/Core/WeatherSystem.cs"
  // 클래스 설계: -> see docs/systems/time-season-architecture.md 섹션 2.3
  // 핵심 로직:
  //   - Weighted Random with Correction (-> see 섹션 5.1)
  //   - OnDayChanged 구독 (priority: 0) → ProcessDayWeather
  //   - ApplyWeatherEffects → FarmGrid.WaterAllPlantedTiles (Rain/HeavyRain/Storm)
  //   - 시드 기반 결정론적 난수 (System.Random)
  //   - GetSaveData/LoadSaveData (-> see 섹션 7.2~7.3)
```

- **MCP 호출**: 1회

#### C-1-04: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

#### C-1 검증 체크리스트

- [ ] WeatherData.cs, WeatherSaveData.cs, WeatherSystem.cs 모두 컴파일 에러 없음
- [ ] WeatherType enum 7종이 time-season-architecture.md 섹션 1.3과 일치

**C-1 MCP 호출 합계**: 4회

### Step C-2: WeatherData SO 4개 생성

**목표**: 4계절의 날씨 확률 데이터를 담는 WeatherData SO 에셋을 생성한다.

**전제**: S-07(WeatherData.cs) 컴파일 완료.

#### C-2-01: 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Core/Weather"
```

- **MCP 호출**: 1회

#### C-2-02: WeatherData SO 4개 생성

반복 패턴 (Spring, Summer, Autumn, Winter 각 1개):

```
create_scriptable_object
  type: "SeedMind.Core.WeatherData"
  asset_path: "Assets/_Project/Data/Core/Weather/SO_Weather_Spring.asset"

set_property  target: "SO_Weather_Spring"
  season = 0                           // Season.Spring
  // 확률 필드 (clearChance, cloudyChance, rainChance, heavyRainChance,
  //            stormChance, snowChance, blizzardChance):
  //   (-> see docs/systems/time-season.md 섹션 3.2 계절별 날씨 확률 테이블)
  // 연속 보정 필드 (maxConsecutiveSameWeatherDays, maxConsecutiveExtremeWeatherDays, consecutivePenalty):
  //   (-> see docs/systems/time-season-architecture.md 섹션 2.4)
  // 날씨 효과 필드 (rainGrowthBonus, stormCropDamageChance, blizzardWitherChance):
  //   (-> see docs/systems/time-season.md 섹션 3.4 농작 영향 테이블)
```

> 동일 패턴을 SO_Weather_Summer (season=1), SO_Weather_Autumn (season=2), SO_Weather_Winter (season=3)에 반복 적용한다.

> **주의**: 날씨 확률 배열의 구체적 수치(0.40, 0.20 등)는 MCP 실행 시점에 `docs/systems/time-season.md` 섹션 3.2에서 읽어 입력한다 (PATTERN-006).

- **MCP 호출**: 4(생성) + 4 x 10(필드 설정: 확률 7 + 보정 3) = 44회

[RISK] 필드 수가 많아 set_property 호출이 다수 발생한다. Editor 스크립트로 일괄 생성 시 ~4회로 감축 가능.

#### C-2 검증 체크리스트

- [ ] `Assets/_Project/Data/Core/Weather/` 폴더 존재
- [ ] SO 에셋 4개: SO_Weather_Spring, SO_Weather_Summer, SO_Weather_Autumn, SO_Weather_Winter
- [ ] 각 에셋의 `season` 필드가 올바른 enum 값
- [ ] 각 계절의 확률 합계 = 1.0
- [ ] 겨울의 Rain/HeavyRain/Storm = 0.0, Snow/Blizzard > 0
- [ ] 콘솔 에러 없음

**C-2 MCP 호출 합계**: ~45회

### Step C-3: TimeSystem GameObject에 WeatherSystem 컴포넌트 추가

**목표**: 기존 TimeSystem GameObject에 WeatherSystem을 추가하고 WeatherData SO 배열을 연결한다.

#### C-3-01: WeatherSystem 부착 및 참조 연결

```
add_component
  target: "TimeSystem"
  component: "SeedMind.Core.WeatherSystem"

set_property  target: "TimeSystem" component: "WeatherSystem"
  _weatherDataSet[0] = "Assets/_Project/Data/Core/Weather/SO_Weather_Spring.asset"
  _weatherDataSet[1] = "Assets/_Project/Data/Core/Weather/SO_Weather_Summer.asset"
  _weatherDataSet[2] = "Assets/_Project/Data/Core/Weather/SO_Weather_Autumn.asset"
  _weatherDataSet[3] = "Assets/_Project/Data/Core/Weather/SO_Weather_Winter.asset"
```

- **MCP 호출**: 1(부착) + 4(배열 연결) = 5회

#### C-3-02: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

#### C-3 검증 체크리스트

- [ ] `TimeSystem` GameObject에 `WeatherSystem` 컴포넌트 부착됨
- [ ] _weatherDataSet에 SO 4개 정상 연결
- [ ] 콘솔 에러 없음

**C-3 MCP 호출 합계**: 6회

### Step C-4: 비/폭풍 효과 연동

**목표**: WeatherSystem의 ApplyWeatherEffects가 FarmGrid, GrowthSystem, HUD와 정상 연동되도록 참조를 설정한다.

#### C-4-01: WeatherSystem 참조 연결

```
set_property  target: "TimeSystem" component: "WeatherSystem"
  // FarmGrid 참조 (Rain/HeavyRain/Storm → WaterAllPlantedTiles)
  // GrowthSystem 참조 (Storm → stormCropDamageChance, Blizzard → blizzardWitherChance)
  // HUDController 참조 (날씨 아이콘 표시용)
```

> WeatherSystem의 FarmGrid 연동: Rain/HeavyRain/Storm 시 `FarmGrid.WaterAllPlantedTiles()` 호출 (-> see `docs/systems/time-season-architecture.md` 섹션 5.2).
> 날씨 아이콘 7종 (-> see `docs/systems/time-season.md` 섹션 3.1) UI 연동은 HUDController 확장 필요.

- **MCP 호출**: ~3회

#### C-4 검증 체크리스트

- [ ] WeatherSystem → FarmGrid 참조 연결됨
- [ ] WeatherSystem → HUDController 참조 연결됨
- [ ] 콘솔 에러 없음

**C-4 MCP 호출 합계**: ~3회

### Step C-5: Play Mode 테스트

**목표**: 날씨 결정, 비 시 자동 물주기, 시드 기반 결정론 동작을 검증한다.

#### C-5-01: 날씨 결정 테스트

```
enter_play_mode

// SkipToNextDay() 호출 → 날씨 변화 확인
get_console_logs
  // 확인: "Weather Changed: Clear → Rain" 등 로그
  // 확인: "Tomorrow Weather: Cloudy" (예보) 로그

exit_play_mode
```

- **MCP 호출**: 3회

#### C-5-02: 시드 재현 테스트

```
enter_play_mode

// 고정 시드로 날씨 결정 후 기록
// 재시작 후 동일 시드로 동일 날씨 순서 확인
get_console_logs

exit_play_mode
```

- **MCP 호출**: 3회

#### C-5 검증 체크리스트

- [ ] 날씨 변화 로그 정상 출력
- [ ] 비 오는 날 자동 물주기 확인 (FarmEvents.OnTileWatered 로그)
- [ ] 동일 시드 → 동일 날씨 순서 확인 (결정론적)
- [ ] 연속 날씨 제한 동작 확인 (동일 날씨 최대 3일, Storm/Blizzard 최대 2일)
- [ ] 콘솔 에러/경고 없음

**C-5 MCP 호출 합계**: 6회

### Phase C MCP 호출 총계: ~64회 (대안 전략 적용 시 ~22회)

---

## 7. Phase D: FestivalManager (~15 MCP 호출)

### 전제 조건

- Phase A 전체 완료 (TimeManager 정상 동작, OnDayChanged 발행)
- Phase C의 WeatherSystem 부착 완료 (우선순위 기반 이벤트 디스패처 동작 확인)

### Step D-1: FestivalManager.cs 작성

**목표**: 축제 판정 및 이벤트 발행을 담당하는 FestivalManager, FestivalData 스크립트를 생성한다.

#### D-1-01: FestivalData SO 클래스

```
create_script
  path: "Assets/_Project/Scripts/Core/Data/FestivalData.cs"
  // 클래스 설계: -> see docs/systems/time-season-architecture.md 섹션 2.5 (FestivalData 박스)
  // 필드: festivalName, festivalId, season, day, description,
  //        shopDiscountRate, specialCropBonus, bonusMultiplier, dialogueKey
```

- **MCP 호출**: 1회

#### D-1-02: FestivalManager 본체

```
create_script
  path: "Assets/_Project/Scripts/Core/FestivalManager.cs"
  // 클래스 설계: -> see docs/systems/time-season-architecture.md 섹션 2.5 (FestivalManager 박스)
  // 기능:
  //   - _festivals: FestivalData[] 배열 참조
  //   - OnDayChanged 구독 (priority: 30) → CheckFestival(season, day)
  //   - _activeFestival 상태 관리
  //   - OnFestivalStarted / OnFestivalEnded 이벤트 발행
  //   - IsFestivalDay(), GetActiveFestival() 쿼리
```

- **MCP 호출**: 1회

#### D-1-03: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

#### D-1 검증 체크리스트

- [ ] FestivalData.cs, FestivalManager.cs 컴파일 에러 없음

**D-1 MCP 호출 합계**: 3회

### Step D-2: FestivalData SO 4개 생성

**목표**: 4개 축제의 FestivalData SO 에셋을 생성한다.

**전제**: S-10(FestivalData.cs) 컴파일 완료.

#### D-2-01: 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Core/Festivals"
```

- **MCP 호출**: 1회

#### D-2-02: FestivalData SO 4개 생성

축제 4종: (-> see `docs/systems/time-season.md` 섹션 4.2 for canonical 축제 상세)

```
create_scriptable_object
  type: "SeedMind.Core.FestivalData"
  asset_path: "Assets/_Project/Data/Core/Festivals/SO_Festival_SpringSeed.asset"

set_property  target: "SO_Festival_SpringSeed"
  festivalId = "festival_spring_seed"
  // festivalName, season, day, description, shopDiscountRate,
  // bonusMultiplier, dialogueKey:
  //   (-> see docs/systems/time-season.md 섹션 4.2 봄 씨앗 축제)
```

> 동일 패턴을 다음 3개 에셋에 반복:
> - `SO_Festival_SummerFireworks.asset` (-> see docs/systems/time-season.md 섹션 4.2 여름 불꽃 축제)
> - `SO_Festival_AutumnHarvest.asset` (-> see docs/systems/time-season.md 섹션 4.2 가을 수확 축제)
> - `SO_Festival_WinterStarlight.asset` (-> see docs/systems/time-season.md 섹션 4.2 겨울 별빛 축제)

> **주의**: 축제 이름, 날짜, 보상 수치 등 구체적 값은 MCP 실행 시점에 `docs/systems/time-season.md` 섹션 4.2에서 읽어 입력한다 (PATTERN-006).

- **MCP 호출**: 4(생성) + 4 x 7(필드 설정) = 32회

[RISK] specialCropBonus 필드는 CropData SO 참조이다. 해당 SO가 ARC-003(farming-tasks)에서 생성 완료되어 있어야 한다. 미존재 시 null로 남기고 나중에 연결.

#### D-2-03: FestivalManager 씬 배치 및 참조 연결

```
add_component
  target: "TimeSystem"
  component: "SeedMind.Core.FestivalManager"

set_property  target: "TimeSystem" component: "FestivalManager"
  _festivals[0] = "Assets/_Project/Data/Core/Festivals/SO_Festival_SpringSeed.asset"
  _festivals[1] = "Assets/_Project/Data/Core/Festivals/SO_Festival_SummerFireworks.asset"
  _festivals[2] = "Assets/_Project/Data/Core/Festivals/SO_Festival_AutumnHarvest.asset"
  _festivals[3] = "Assets/_Project/Data/Core/Festivals/SO_Festival_WinterStarlight.asset"

save_scene
```

- **MCP 호출**: 1(부착) + 4(배열 연결) + 1(저장) = 6회

#### D-2 검증 체크리스트

- [ ] `Assets/_Project/Data/Core/Festivals/` 폴더 존재
- [ ] SO 에셋 4개 존재
- [ ] 각 축제의 season/day 값이 canonical 문서와 일치
- [ ] TimeSystem에 FestivalManager 컴포넌트 부착, _festivals 배열 연결
- [ ] 콘솔 에러 없음

**D-2 MCP 호출 합계**: ~39회

### Step D-3: Play Mode 테스트

**목표**: 축제 날짜 도달 시 이벤트 발행과 축제 상태를 검증한다.

#### D-3-01: 축제 이벤트 테스트

```
enter_play_mode

// SkipToNextDay() 반복 호출 → Spring Day 13 도달 시 축제 이벤트 확인
get_console_logs
  // 확인: "Festival Started: 봄 씨앗 축제" 로그
  // 확인: IsFestivalDay() == true

// Day 14로 넘어가면 축제 종료
get_console_logs
  // 확인: "Festival Ended: 봄 씨앗 축제" 로그

exit_play_mode
```

- **MCP 호출**: 4회 (enter + get_logs x2 + exit)

#### D-3 검증 체크리스트

- [ ] Spring Day 13 → OnFestivalStarted 발행
- [ ] Spring Day 14 → OnFestivalEnded 발행
- [ ] IsFestivalDay() 정상 동작
- [ ] GetActiveFestival() 정상 반환
- [ ] 콘솔 에러/경고 없음

**D-3 MCP 호출 합계**: 4회

### Phase D MCP 호출 총계: ~46회

---

## 8. Phase E: 통합 테스트 (~5 MCP 호출)

### 전제 조건

- Phase A~D 전체 완료
- FarmGrid, GrowthSystem, SaveManager 존재 (-> see ARC-003, save-load-tasks.md)

### Step E-1: 전체 시스템 연동 테스트

**목표**: 시간 → 날씨 → 물주기 → 성장 → 계절 전환 → 축제의 전체 흐름을 검증한다.

#### E-1-01: 연동 흐름 테스트

```
enter_play_mode

// 1) 시간 진행 확인 (06:00 → 07:00 → ... → 24:00)
// 2) 날씨 결정 확인 (OnDayChanged → WeatherSystem priority 0)
// 3) 비 오는 날 → FarmGrid 자동 물주기 (priority 0에서 처리)
// 4) GrowthSystem 성장 처리 (priority 10)
// 5) SkipToNextDay 28회 → 계절 전환 (OnSeasonChanged)
// 6) 축제 날짜 → FestivalManager 이벤트 발행 (priority 30)

get_console_logs
  // OnDayChanged 구독자 실행 순서 확인:
  //   priority 0: WeatherSystem
  //   priority 10: GrowthSystem
  //   priority 20: FarmGrid
  //   priority 30: FestivalManager
  //   priority 40: EconomyManager
  //   priority 50: SaveManager
  //   priority 90: HUDController
  // (-> see docs/systems/time-season-architecture.md 섹션 4.3)

exit_play_mode
```

- **MCP 호출**: 3회 (enter + get_logs + exit)

#### E-1 검증 체크리스트

- [ ] OnDayChanged 구독자 실행 순서가 우선순위대로 진행
- [ ] 비 날씨 → 자동 물주기 → 성장 처리 순서 보장
- [ ] 계절 전환 시 OnSeasonChanged → OnDayChanged 순서 보장
- [ ] 축제 이벤트 정상 발행
- [ ] HUD 갱신 정상 (시간, 날짜, 계절, 날씨 아이콘)
- [ ] 콘솔 에러/경고 없음

**E-1 MCP 호출 합계**: 3회

### Step E-2: 저장/로드 테스트

**목표**: 시간/날씨 상태가 저장/로드 후 정확히 복원되는지 검증한다.

#### E-2-01: 저장/로드 테스트

```
enter_play_mode

// 1) 시간을 Summer Day 15, Hour 14:30까지 진행
// 2) SaveManager.Save() 호출
// 3) 플레이 중단 → 재시작 → SaveManager.Load() 호출
// 4) 복원된 상태 확인:
//    - TimeManager: year=1, season=Summer, day=15, hour=14.5
//    - WeatherSystem: 동일 시드로 동일 날씨 재현
//    - DayPhase: Afternoon

get_console_logs
  // 확인: 로드 후 상태 일치 로그
  // 확인: 날씨 시드 재현 → 동일 날씨 시퀀스
  // (-> see docs/systems/time-season-architecture.md 섹션 7.3 로드 흐름)

exit_play_mode
```

- **MCP 호출**: 3회 (enter + get_logs + exit)

#### E-2 검증 체크리스트

- [ ] TimeManager 상태 (year, season, day, hour, dayPhase) 정확히 복원
- [ ] WeatherSystem 상태 (currentWeather, tomorrowWeather, seed) 정확히 복원
- [ ] 날씨 시드 재현: 로드 후 이후 날씨가 저장 전과 동일
- [ ] 콘솔 에러/경고 없음

**E-2 MCP 호출 합계**: 3회

### Phase E MCP 호출 총계: ~6회

---

## 9. 태스크 요약

### Phase별 MCP 호출 수

| Phase | 설명 | MCP 호출 (직접 입력) | MCP 호출 (Editor 스크립트 대안) |
|-------|------|---------------------|-------------------------------|
| **A** | TimeManager 기본 | ~24회 | ~24회 |
| **B** | SeasonData 환경 연출 | ~172회 | ~54회 |
| **C** | WeatherSystem | ~64회 | ~22회 |
| **D** | FestivalManager | ~46회 | ~20회 |
| **E** | 통합 테스트 | ~6회 | ~6회 |
| **합계** | | **~312회** | **~126회** |

[RISK] 직접 입력 방식의 ~312회 호출은 과다하다. **Editor 스크립트 일괄 생성 전략을 적극 활용**하여 ~126회로 감축할 것을 강력 권장한다. 특히 Phase B의 DayPhaseVisual 20 세트(~120회 → ~2회), Phase C의 WeatherData 필드 설정(~44회 → ~4회)이 핵심 감축 대상이다.

### 생성 에셋 목록

| 카테고리 | 에셋 | 수량 |
|----------|------|------|
| **스크립트** | TimeConfig, TimeSaveData, TimeManager, SeasonData, DayPhaseVisual, EnvironmentController, WeatherData, WeatherSaveData, WeatherSystem, FestivalData, FestivalManager | 11종 |
| **SO 에셋** | SO_TimeConfig (1), SO_Season_* (4), SO_Weather_* (4), SO_Festival_* (4) | 13종 |
| **씬 오브젝트** | TimeSystem (TimeManager + EnvironmentController + WeatherSystem + FestivalManager) | 1개 |
| **에셋 폴더** | Data/Core, Data/Core/Seasons, Data/Core/Weather, Data/Core/Festivals | 4개 |

### 씬 계층 최종 구조

```
SCN_Farm
├── --- MANAGERS ---
│   ├── ... (기존 매니저들)
│   └── TimeSystem                  ← 신규
│       ├── [TimeManager]           ← 컴포넌트
│       ├── [EnvironmentController] ← 컴포넌트
│       ├── [WeatherSystem]         ← 컴포넌트
│       └── [FestivalManager]       ← 컴포넌트
└── ...
```

---

## 10. Cross-references

- `docs/systems/time-season-architecture.md` -- 본 문서의 설계 원본, 섹션 8이 MCP 계획 초안
- `docs/systems/time-season.md` -- canonical 날씨 확률(섹션 3.2), 축제 상세(섹션 4.2), 시간대 정의(섹션 1.2)
- `docs/architecture.md` 4.3절 -- 시간 시스템 개요
- `docs/design.md` 4.3절 -- 시간/계절 시스템 게임 설계
- `docs/systems/project-structure.md` -- 네임스페이스 SeedMind.Core, 폴더 구조
- `docs/mcp/scene-setup-tasks.md` (ARC-002) -- 폴더 구조, 씬 기본 계층 (전제 조건)
- `docs/mcp/farming-tasks.md` (ARC-003) -- FarmGrid, GrowthSystem (전제 조건)
- `docs/mcp/save-load-tasks.md` -- SaveManager 연동 (Phase E)
- `docs/systems/farming-architecture.md` -- GrowthSystem OnDayChanged 구독
- `docs/systems/crop-growth-architecture.md` -- 계절 전환 시 SeasonalWither 처리

---

## Open Questions

- [OPEN] DayPhaseVisual 20 세트의 MCP 직접 입력이 현실적인지, Editor 스크립트 우회가 필수인지는 MCP `set_property`의 중첩 배열 지원 여부에 따라 결정된다.
- [OPEN] HUDController의 시간/날씨 표시 UI 요소(Text_Time, Text_Day, Text_Season, Icon_Weather)가 ARC-002에서 생성되어 있는지 확인 필요. 미존재 시 Phase A Step A-5에서 추가 생성이 필요하다.
- [OPEN] EnvironmentController의 조명 보간에 DOTween을 사용할지, Unity 내장 코루틴을 사용할지. DOTween 패키지가 프로젝트에 포함되어 있어야 한다.

## Risks

- [RISK] `set_property`로 Serializable class 배열(DayPhaseVisual[])의 중첩 필드 설정이 불가능할 수 있다 (Phase B-2). Editor 스크립트 우회 전략 필수 대비.
- [RISK] SO 참조 연결(`_timeConfig`, `_seasonDataSet`, `_weatherDataSet`, `_festivals`)이 MCP `set_property`로 가능한지 사전 검증 필요. 에셋 경로 기반 참조가 지원되지 않는 경우 `AssetDatabase.LoadAssetAtPath` 방식의 Editor 스크립트 필요.
- [RISK] 총 MCP 호출 수 ~312회(직접 입력)는 과다하며, 네트워크 지연/에러 누적으로 실패 확률이 높아진다. Editor 스크립트 병행으로 ~126회까지 감축 권장.
- [RISK] TimeManager가 Singleton 베이스 클래스에 의존하므로, Singleton.cs가 선행 컴파일 안 되어 있으면 Phase A 전체가 블록된다.
- [RISK] FestivalData.specialCropBonus 필드는 CropData SO 참조이며, 해당 SO가 ARC-003에서 생성 완료되어 있어야 한다. 순서 의존성 주의.
