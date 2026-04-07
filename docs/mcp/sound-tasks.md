# 사운드 시스템 MCP 태스크 시퀀스 (ARC-027)

> SoundManager, BGMScheduler, SFXPool, AudioMixer, SoundData SO, SoundEventBridge의 MCP for Unity 태스크를 상세 정의  
> 작성: Claude Code (Opus 4.6) | 2026-04-07  
> Phase 1 | 문서 ID: ARC-027

---

## Context

이 문서는 `docs/systems/sound-architecture.md`(AUD-001) Part II에서 요약된 MCP 구현 계획(Step 1~5)을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 사운드 시스템의 AudioMixer 에셋, SoundManager 씬 구성, SoundData/BGMRegistry SO 에셋 생성, 이벤트 연결, 통합 테스트, MVP 사운드 에셋 임포트를 완성한다.

---

## 1. 의존성

```
사운드 시스템 MCP 태스크 의존 관계:
├── SeedMind.Core     (TimeManager, SaveManager, EventBus)
├── SeedMind.Farm     (FarmEvents -- 경작 이벤트)
├── SeedMind.Economy  (EconomyEvents -- 구매/판매 이벤트)
├── SeedMind.Player   (ProgressionManager -- 레벨업 이벤트)
├── SeedMind.Quest    (QuestEvents -- 퀘스트 완료 이벤트)
├── SeedMind.Achievement (AchievementEvents -- 업적 달성 이벤트)
└── SeedMind.Fishing  (FishingEvents -- 낚시 이벤트)
```

(-> see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 완료된 태스크 의존성

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조, SCN_Farm 기본 계층 (Managers, Farm, Player, UI) |
| ARC-003 | `docs/mcp/farming-tasks.md` | Phase A~C 전체 | FarmGrid 타일, FarmEvents 정적 이벤트 허브 |
| ARC-005 | `docs/mcp/time-season-tasks.md` | 전체 | TimeManager, WeatherSystem, Season/DayPhase/WeatherType enum |

### 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A |
| `TimeManager` (싱글턴) | ARC-005 |
| `FarmEvents` (정적 이벤트 허브) | ARC-003 |

### 총 MCP 호출 예상 수

| 태스크 | 호출 수 |
|--------|--------|
| S-1: AudioMixer 에셋 생성 | ~18회 |
| S-2: SoundManager GameObject 구성 | ~28회 |
| S-3: ScriptableObject 에셋 생성 | ~45회 |
| S-4: SoundEventBridge 연결 및 검증 | ~12회 |
| S-5: BGMScheduler 통합 테스트 | ~10회 |
| S-6: MVP 사운드 에셋 임포트 및 연결 | ~35회 |
| **합계** | **~148회** |

[RISK] SO 에셋 대량 생성(S-3)에서 MCP 호출 수가 많다. Editor 스크립트(CreateSoundAssets.cs)를 통한 SoundData SO 일괄 생성으로 S-3의 호출 수를 대폭 줄일 수 있다. MCP 단독 실행 시 시간 비용이 크므로 Editor 스크립트 우회를 권장한다.

---

## 2. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋 폴더 생성 | S-1, S-3, S-6 |
| `create_asset` | AudioMixer 에셋 생성 | S-1 |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | S-3 |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | S-1~S-6 전체 |
| `create_script` | C# 스크립트 파일 생성 | S-2 |
| `create_object` | 빈 GameObject 생성 | S-2 |
| `add_component` | MonoBehaviour/AudioSource 컴포넌트 부착 | S-2, S-4 |
| `set_parent` | 오브젝트 부모 설정 | S-2 |
| `save_scene` | 씬 저장 | S-2, S-4, S-5 |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | S-4, S-5 |
| `execute_menu_item` | 편집기 명령 실행 (컴파일 대기 등) | S-2, S-3 |
| `execute_method` | 런타임 메서드 호출 (테스트) | S-4, S-5 |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | S-4, S-5 |
| `import_asset` | 외부 오디오 파일 임포트 | S-6 |

[RISK] `create_asset`으로 AudioMixer를 생성할 수 있는지 사전 검증 필요. AudioMixer는 Unity 내장 에셋 타입으로, MCP에서 직접 생성 불가할 경우 Unity Editor에서 수동 생성하거나 Editor 스크립트를 통한 우회가 필요하다.

---

## 3. 필요 C# 스크립트 목록

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 태스크 순서대로 작성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| A-01 | `Scripts/Audio/Data/SoundData.cs` | `SoundData` (SO) | `SeedMind.Audio.Data` | S-2 Phase 1 |
| A-02 | `Scripts/Audio/Data/SoundRegistry.cs` | `SoundRegistry` (SO) | `SeedMind.Audio.Data` | S-2 Phase 1 |
| A-03 | `Scripts/Audio/Data/BGMRegistry.cs` | `BGMRegistry` (SO) | `SeedMind.Audio.Data` | S-2 Phase 1 |
| A-04 | `Scripts/Audio/Data/AudioSettingsData.cs` | `AudioSettingsData` | `SeedMind.Audio.Data` | S-2 Phase 1 |
| A-05 | `Scripts/Audio/SFXPool.cs` | `SFXPool` | `SeedMind.Audio` | S-2 Phase 2 |
| A-06 | `Scripts/Audio/BGMScheduler.cs` | `BGMScheduler` | `SeedMind.Audio` | S-2 Phase 2 |
| A-07 | `Scripts/Audio/SoundManager.cs` | `SoundManager` (MonoBehaviour) | `SeedMind.Audio` | S-2 Phase 3 |
| A-08 | `Scripts/Audio/SoundEventBridge.cs` | `SoundEventBridge` (MonoBehaviour) | `SeedMind.Audio` | S-4 |

(모든 경로 접두어: `Assets/_Project/`)

> enum 파일(AudioChannel, BGMTrack, SFXId)은 sound-architecture.md 섹션 1.1~1.3에 정의되어 있으며, S-2 Phase 1 이전에 생성되어야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| E-01 | `Scripts/Audio/AudioChannel.cs` | `AudioChannel` (enum) | `SeedMind.Audio` | S-2 Phase 0 |
| E-02 | `Scripts/Audio/BGMTrack.cs` | `BGMTrack` (enum) | `SeedMind.Audio` | S-2 Phase 0 |
| E-03 | `Scripts/Audio/SFXId.cs` | `SFXId` (enum) | `SeedMind.Audio` | S-2 Phase 0 |
| E-04 | `Scripts/Audio/SoundEvent.cs` | `SoundEvent` (struct) | `SeedMind.Audio` | S-2 Phase 0 |

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: E-01~E-04 -> A-01~A-04 -> A-05~A-06 -> A-07 -> A-08. 각 Phase 사이에 Unity 컴파일 대기가 필요하다.

---

## 4. 태스크 목록

---

### S-1: AudioMixer 에셋 생성 및 채널 구조 설정

**목적**: MainMixer 에셋을 생성하고, 5개 AudioMixerGroup(Master/BGM/SFX/Ambient/UI) 및 하위 서브그룹을 구성하며, Exposed Parameter를 등록한다.

**전제**: `Assets/_Project/Audio/` 폴더가 존재해야 한다.

**의존 태스크**: ARC-002 (폴더 구조 생성)

#### S-1-01: 오디오 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Audio"

create_folder
  path: "Assets/_Project/Audio/BGM"

create_folder
  path: "Assets/_Project/Audio/SFX"

create_folder
  path: "Assets/_Project/Audio/Ambient"

create_folder
  path: "Assets/_Project/Audio/UI"
```

- **MCP 호출**: 5회

#### S-1-02: MainMixer 에셋 생성

```
create_asset
  type: "AudioMixer"
  asset_path: "Assets/_Project/Audio/MainMixer.mixer"
```

> [RISK] AudioMixer 에셋 생성이 MCP에서 미지원일 경우, Editor 스크립트 또는 수동 생성 필요.

- **MCP 호출**: 1회

#### S-1-03: AudioMixerGroup 생성

Master Group 하위에 4개 자식 Group을 생성한다. 채널 구조는 sound-design.md의 상세 서브그룹 구조를 따른다 (-> see docs/systems/sound-design.md 섹션 3.1).

```
// Master Group은 AudioMixer 생성 시 자동 존재

// 1단계 자식 Group 생성
add_mixer_group  mixer: "MainMixer"  parent: "Master"  name: "BGM"
add_mixer_group  mixer: "MainMixer"  parent: "Master"  name: "SFX"
add_mixer_group  mixer: "MainMixer"  parent: "Master"  name: "Ambient"
add_mixer_group  mixer: "MainMixer"  parent: "Master"  name: "UI"

// BGM 서브그룹
add_mixer_group  mixer: "MainMixer"  parent: "BGM"  name: "BGM_Main"
add_mixer_group  mixer: "MainMixer"  parent: "BGM"  name: "BGM_Weather"

// SFX 서브그룹
add_mixer_group  mixer: "MainMixer"  parent: "SFX"  name: "SFX_Player"
add_mixer_group  mixer: "MainMixer"  parent: "SFX"  name: "SFX_World"
add_mixer_group  mixer: "MainMixer"  parent: "SFX"  name: "SFX_Jingle"

// Ambient 서브그룹
add_mixer_group  mixer: "MainMixer"  parent: "Ambient"  name: "AMB_Weather"
add_mixer_group  mixer: "MainMixer"  parent: "Ambient"  name: "AMB_Nature"
add_mixer_group  mixer: "MainMixer"  parent: "Ambient"  name: "AMB_Water"

// UI 서브그룹
add_mixer_group  mixer: "MainMixer"  parent: "UI"  name: "UI_Click"
add_mixer_group  mixer: "MainMixer"  parent: "UI"  name: "UI_Notify"
```

> [RISK] `add_mixer_group` MCP 도구 존재 여부 미확인. 미지원 시 Editor 스크립트 우회 필요.

- **MCP 호출**: ~14회 (서브그룹 포함)

#### S-1-04: Exposed Parameter 등록

```
expose_parameter  mixer: "MainMixer"  group: "Master"   param: "MasterVolume"
expose_parameter  mixer: "MainMixer"  group: "BGM"      param: "BGMVolume"
expose_parameter  mixer: "MainMixer"  group: "SFX"      param: "SFXVolume"
expose_parameter  mixer: "MainMixer"  group: "Ambient"  param: "AmbientVolume"
expose_parameter  mixer: "MainMixer"  group: "UI"       param: "UIVolume"
```

> Exposed Parameter 네이밍 규칙: `{AudioChannel}Volume` (-> see docs/systems/sound-architecture.md 섹션 4.2)

- **MCP 호출**: 5회

#### S-1-05: 기본 볼륨 설정

각 채널의 기본 볼륨은 sound-design.md canonical 값을 참조하여 설정한다.

```
set_property  mixer: "MainMixer"  param: "MasterVolume"  value: 0
    // (-> see docs/systems/sound-design.md 섹션 3.2 for dB 값)
set_property  mixer: "MainMixer"  param: "BGMVolume"  value: 0
    // (-> see docs/systems/sound-design.md 섹션 3.2 for dB 값)
set_property  mixer: "MainMixer"  param: "SFXVolume"  value: 0
    // (-> see docs/systems/sound-design.md 섹션 3.2 for dB 값)
set_property  mixer: "MainMixer"  param: "AmbientVolume"  value: 0
    // (-> see docs/systems/sound-design.md 섹션 3.2 for dB 값)
set_property  mixer: "MainMixer"  param: "UIVolume"  value: 0
    // (-> see docs/systems/sound-design.md 섹션 3.2 for dB 값)
```

> **주의**: 구체적 dB 수치는 MCP 실행 시점에 canonical 문서에서 읽어 입력한다 (PATTERN-006).

- **MCP 호출**: 5회

#### S-1 검증 체크리스트

- [ ] `Assets/_Project/Audio/MainMixer.mixer` 파일 존재
- [ ] Master > BGM > BGM_Main, BGM_Weather 계층 확인
- [ ] Master > SFX > SFX_Player, SFX_World, SFX_Jingle 계층 확인
- [ ] Master > Ambient > AMB_Weather, AMB_Nature, AMB_Water 계층 확인
- [ ] Master > UI > UI_Click, UI_Notify 계층 확인
- [ ] 5개 Exposed Parameter (MasterVolume, BGMVolume, SFXVolume, AmbientVolume, UIVolume) 등록 완료
- [ ] 각 채널 기본 볼륨이 canonical 값과 일치

---

### S-2: SoundManager GameObject 구성

**목적**: SoundManager 싱글턴 GameObject를 씬에 배치하고, BGM Dual-Source, Ambient Source, UI Source, SFX 풀을 자식 AudioSource로 구성한다.

**전제**: S-1 완료 (MainMixer 에셋 존재). 스크립트 E-01~E-04, A-01~A-07 컴파일 완료.

**의존 태스크**: S-1

#### S-2-01: 스크립트 생성 -- Phase 0 (Enum/Struct)

```
create_script
  path: "Assets/_Project/Scripts/Audio/AudioChannel.cs"
  // AudioChannel enum (-> see docs/systems/sound-architecture.md 섹션 1.1)

create_script
  path: "Assets/_Project/Scripts/Audio/BGMTrack.cs"
  // BGMTrack enum (-> see docs/systems/sound-architecture.md 섹션 1.2)

create_script
  path: "Assets/_Project/Scripts/Audio/SFXId.cs"
  // SFXId enum (-> see docs/systems/sound-architecture.md 섹션 1.3)

create_script
  path: "Assets/_Project/Scripts/Audio/SoundEvent.cs"
  // SoundEvent struct (-> see docs/systems/sound-architecture.md 섹션 1.4)

execute_menu_item  menu: "Assets/Refresh"  // 컴파일 대기
```

- **MCP 호출**: 5회

#### S-2-02: 스크립트 생성 -- Phase 1 (데이터 레이어 SO)

```
create_script
  path: "Assets/_Project/Scripts/Audio/Data/SoundData.cs"
  // SoundData SO (-> see docs/systems/sound-architecture.md 섹션 5.1)

create_script
  path: "Assets/_Project/Scripts/Audio/Data/SoundRegistry.cs"
  // SoundRegistry SO (-> see docs/systems/sound-architecture.md 섹션 5.2)

create_script
  path: "Assets/_Project/Scripts/Audio/Data/BGMRegistry.cs"
  // BGMRegistry SO (-> see docs/systems/sound-architecture.md 섹션 5.3)

create_script
  path: "Assets/_Project/Scripts/Audio/Data/AudioSettingsData.cs"
  // AudioSettingsData 직렬화 클래스 (-> see docs/systems/sound-architecture.md 섹션 9.1)

execute_menu_item  menu: "Assets/Refresh"  // 컴파일 대기
```

- **MCP 호출**: 5회

#### S-2-03: 스크립트 생성 -- Phase 2 (시스템 클래스)

```
create_script
  path: "Assets/_Project/Scripts/Audio/SFXPool.cs"
  // SFXPool 클래스 (-> see docs/systems/sound-architecture.md 섹션 3.1)

create_script
  path: "Assets/_Project/Scripts/Audio/BGMScheduler.cs"
  // BGMScheduler 클래스 (-> see docs/systems/sound-architecture.md 섹션 6.3)

execute_menu_item  menu: "Assets/Refresh"  // 컴파일 대기
```

- **MCP 호출**: 3회

#### S-2-04: 스크립트 생성 -- Phase 3 (매니저)

```
create_script
  path: "Assets/_Project/Scripts/Audio/SoundManager.cs"
  // SoundManager MonoBehaviour 싱글턴 (-> see docs/systems/sound-architecture.md 섹션 2)

execute_menu_item  menu: "Assets/Refresh"  // 컴파일 대기
```

- **MCP 호출**: 2회

#### S-2-05: SoundManager GameObject 생성

```
create_object
  name: "SoundManager"
  // 씬 루트에 배치 (DontDestroyOnLoad 대상)

add_component
  target: "SoundManager"
  type: "SeedMind.Audio.SoundManager"
```

- **MCP 호출**: 2회

#### S-2-06: BGM Dual-Source 구성

```
// BGM Source A
create_object
  name: "BGM_Source_A"

set_parent
  target: "BGM_Source_A"
  parent: "SoundManager"

add_component
  target: "BGM_Source_A"
  type: "AudioSource"

set_property  target: "BGM_Source_A/AudioSource"
  loop = true
  playOnAwake = false
  outputAudioMixerGroup = "MainMixer/BGM/BGM_Main"

// BGM Source B
create_object
  name: "BGM_Source_B"

set_parent
  target: "BGM_Source_B"
  parent: "SoundManager"

add_component
  target: "BGM_Source_B"
  type: "AudioSource"

set_property  target: "BGM_Source_B/AudioSource"
  loop = true
  playOnAwake = false
  outputAudioMixerGroup = "MainMixer/BGM/BGM_Main"
```

- **MCP 호출**: 8회 (각 Source 4회 x 2)

#### S-2-07: Ambient Source 구성

```
create_object
  name: "Ambient_Source"

set_parent
  target: "Ambient_Source"
  parent: "SoundManager"

add_component
  target: "Ambient_Source"
  type: "AudioSource"

set_property  target: "Ambient_Source/AudioSource"
  loop = true
  playOnAwake = false
  spatialBlend = 0    // 2D
  outputAudioMixerGroup = "MainMixer/Ambient/AMB_Weather"
```

- **MCP 호출**: 4회

#### S-2-08: UI Source 구성

```
create_object
  name: "UI_Source"

set_parent
  target: "UI_Source"
  parent: "SoundManager"

add_component
  target: "UI_Source"
  type: "AudioSource"

set_property  target: "UI_Source/AudioSource"
  loop = false
  playOnAwake = false
  spatialBlend = 0    // 2D (UI 사운드는 항상 2D)
  outputAudioMixerGroup = "MainMixer/UI/UI_Click"
```

- **MCP 호출**: 4회

#### S-2-09: SFX 풀 AudioSource 생성

SFX 풀 크기는 sound-design.md를 참조한다 (-> see docs/systems/sound-design.md 섹션 3.5 for poolSize).

```
// poolSize개의 SFX AudioSource를 SoundManager 자식으로 생성
// 반복: i = 0 ~ (poolSize - 1)  // poolSize → see docs/systems/sound-design.md 섹션 3.5

create_object
  name: "SFX_Source_{i}"

set_parent
  target: "SFX_Source_{i}"
  parent: "SoundManager"

add_component
  target: "SFX_Source_{i}"
  type: "AudioSource"

set_property  target: "SFX_Source_{i}/AudioSource"
  playOnAwake = false
  outputAudioMixerGroup = "MainMixer/SFX/SFX_Player"
    // 실제 서브그룹(SFX_Player/SFX_World/SFX_Jingle)은 런타임에 SFXPool이 동적 할당
```

> **주의**: 풀 크기(poolSize)의 구체적 수치는 MCP 실행 시점에 canonical 문서에서 읽어 반복 횟수를 결정한다. 본 문서에서 수치를 직접 기재하지 않는다 (PATTERN-006).

- **MCP 호출**: poolSize x 3회 (create_object + set_parent + add_component) + poolSize x 1회 (set_property) = poolSize x 4회
  - 예상: (-> see docs/systems/sound-design.md 섹션 3.5 for poolSize) x 4회

#### S-2-10: SoundManager 필드 참조 할당

```
set_property  target: "SoundManager/SoundManager"
  _mixer = "Assets/_Project/Audio/MainMixer.mixer"    // AudioMixer 참조
  _soundRegistry = null                                // S-3에서 생성 후 할당
  _bgmRegistry = null                                  // S-3에서 생성 후 할당
  _bgmSourceA = "SoundManager/BGM_Source_A/AudioSource"
  _bgmSourceB = "SoundManager/BGM_Source_B/AudioSource"
  _ambientSource = "SoundManager/Ambient_Source/AudioSource"
  _uiSource = "SoundManager/UI_Source/AudioSource"

save_scene
```

- **MCP 호출**: 2회

#### S-2 검증 체크리스트

- [ ] SoundManager GameObject가 씬 루트에 존재
- [ ] SoundManager.cs 컴포넌트 부착 확인
- [ ] 자식 오브젝트: BGM_Source_A, BGM_Source_B, Ambient_Source, UI_Source 존재
- [ ] SFX_Source_0 ~ SFX_Source_{poolSize-1} 자식 AudioSource 존재 (-> see docs/systems/sound-design.md 섹션 3.5 for poolSize)
- [ ] 모든 AudioSource의 outputAudioMixerGroup이 MainMixer 하위 적절한 그룹을 참조
- [ ] BGM Source A/B: loop=true, playOnAwake=false
- [ ] UI Source: spatialBlend=0, loop=false

---

### S-3: ScriptableObject 에셋 생성

**목적**: SoundRegistry, BGMRegistry SO 에셋을 생성하고, SFXId enum의 각 값에 대응하는 SoundData SO 에셋을 생성한다. BGMTrack enum의 각 값에 대응하는 BGMEntry를 BGMRegistry에 등록한다.

**전제**: S-2 Phase 1 완료 (SoundData.cs, SoundRegistry.cs, BGMRegistry.cs 컴파일 완료)

**의존 태스크**: S-2

#### S-3-01: 오디오 데이터 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Audio"

create_folder
  path: "Assets/_Project/Data/Audio/SFX"

create_folder
  path: "Assets/_Project/Data/Audio/BGM"
```

- **MCP 호출**: 3회

#### S-3-02: SoundRegistry SO 에셋 생성

```
create_scriptable_object
  type: "SeedMind.Audio.Data.SoundRegistry"
  asset_path: "Assets/_Project/Data/Audio/SoundRegistry.asset"
```

- **MCP 호출**: 1회

#### S-3-03: BGMRegistry SO 에셋 생성

```
create_scriptable_object
  type: "SeedMind.Audio.Data.BGMRegistry"
  asset_path: "Assets/_Project/Data/Audio/BGMRegistry.asset"
```

- **MCP 호출**: 1회

#### S-3-04: SoundData SO 에셋 생성 (MVP 대상)

MVP 사운드 목록에 해당하는 SoundData SO를 생성한다. SFX 전체 목록 및 MVP 범위는 canonical 문서를 참조한다 (-> see docs/systems/sound-design.md 섹션 4.1 for MVP 사운드 목록).

각 SoundData SO 에셋의 생성 패턴:

```
// 패턴 -- SFXId별 반복
create_scriptable_object
  type: "SeedMind.Audio.Data.SoundData"
  asset_path: "Assets/_Project/Data/Audio/SFX/SD_{SFXId}.asset"

set_property  target: "SD_{SFXId}"
  id = {SFXId enum 값}
  clips = []                // S-6에서 AudioClip 할당
  baseVolume = 0            // (-> see docs/systems/sound-design.md 해당 SFX 섹션)
  pitchVariation = 0        // (-> see docs/systems/sound-design.md 섹션 3.3)
  is3D = false              // (-> see docs/systems/sound-design.md 해당 SFX 섹션 -- 3D/2D 구분)
  maxDistance = 0            // (-> see docs/systems/sound-design.md 섹션 3.4)
```

> **주의**: 각 SFX의 `baseVolume`, `pitchVariation`, `is3D`, `maxDistance` 구체적 수치는 MCP 실행 시점에 canonical 문서에서 읽어 입력한다 (PATTERN-006).

MVP SoundData SO 목록 (-> see docs/systems/sound-design.md 섹션 4.1 for 상세 ID 목록):

| 카테고리 | 에셋명 패턴 | 수량 |
|---------|------------|------|
| 경작 (기본 등급) | `SD_HoeTillBasic`, `SD_SeedPlant`, `SD_WaterBasic`, `SD_ScytheBasic`, `SD_Harvest`, `SD_Fertilize` | (-> see docs/systems/sound-design.md 섹션 4.1) |
| 작물 | `SD_CropGrow`, `SD_CropWither` | (-> see docs/systems/sound-design.md 섹션 4.1) |
| 도구 | `SD_ToolEquip` | (-> see docs/systems/sound-design.md 섹션 4.1) |
| UI | `SD_UIClick`, `SD_InventoryOpen`, `SD_InventoryClose`, `SD_Notification`, `SD_UIError`, `SD_UIConfirm` | (-> see docs/systems/sound-design.md 섹션 4.1) |
| 상점/NPC | `SD_ShopOpen`, `SD_ShopClose`, `SD_Purchase`, `SD_Sale` | (-> see docs/systems/sound-design.md 섹션 4.1) |
| 진행 | `SD_LevelUp`, `SD_XPGain`, `SD_GoldGain`, `SD_GoldSpend` | (-> see docs/systems/sound-design.md 섹션 4.1) |
| 시간 | `SD_MorningChime`, `SD_Sleep` | (-> see docs/systems/sound-design.md 섹션 4.1) |
| 환경 | `SD_FootstepDirt`, `SD_FootstepGrass` | (-> see docs/systems/sound-design.md 섹션 4.1) |

- **MCP 호출**: MVP 사운드 수 x 2회 (create_scriptable_object + set_property) = (-> see docs/systems/sound-design.md 섹션 4.1 for MVP 합계) x 2회

[RISK] SoundData SO를 하나씩 MCP로 생성하면 호출 수가 많다. `CreateSoundAssets.cs` Editor 스크립트를 작성하여 일괄 생성하는 우회를 권장한다. Editor 스크립트는 enum 값을 순회하면서 SO를 자동 생성하므로 MCP 호출을 1~2회로 줄일 수 있다.

#### S-3-05: SoundRegistry에 SoundData 등록

생성된 SoundData SO들을 SoundRegistry의 `entries` 배열에 할당한다.

```
set_property  target: "SoundRegistry"
  entries = [
    "Assets/_Project/Data/Audio/SFX/SD_HoeTillBasic.asset",
    "Assets/_Project/Data/Audio/SFX/SD_SeedPlant.asset",
    ...
    // MVP SoundData SO 전체 (-> see docs/systems/sound-design.md 섹션 4.1 for 전체 목록)
  ]
```

> [RISK] MCP `set_property`로 SO 배열 참조를 설정할 수 있는지 사전 검증 필요. 미지원 시 Editor 스크립트 우회.

- **MCP 호출**: 1회

#### S-3-06: BGMRegistry에 BGMEntry 등록

BGMTrack enum의 각 값에 대응하는 BGMEntry를 등록한다. 전체 BGM 트랙 수는 canonical 문서를 참조 (-> see docs/systems/sound-design.md 섹션 1.2, 1.3 for 계절별/특수/시스템 BGM 목록).

```
set_property  target: "BGMRegistry"
  entries = [
    { track: BGMTrack.Spring,      clip: null, loopStartTime: 0, loopEndTime: 0 },
    { track: BGMTrack.Summer,      clip: null, loopStartTime: 0, loopEndTime: 0 },
    { track: BGMTrack.Autumn,      clip: null, loopStartTime: 0, loopEndTime: 0 },
    { track: BGMTrack.Winter,      clip: null, loopStartTime: 0, loopEndTime: 0 },
    { track: BGMTrack.NightTime,   clip: null, loopStartTime: 0, loopEndTime: 0 },
    { track: BGMTrack.Festival,    clip: null, loopStartTime: 0, loopEndTime: 0 },
    { track: BGMTrack.IndoorHome,  clip: null, loopStartTime: 0, loopEndTime: 0 },
    { track: BGMTrack.Shop,        clip: null, loopStartTime: 0, loopEndTime: 0 },
    { track: BGMTrack.Rain,        clip: null, loopStartTime: 0, loopEndTime: 0 },
    { track: BGMTrack.Storm,       clip: null, loopStartTime: 0, loopEndTime: 0 },
    { track: BGMTrack.TitleScreen, clip: null, loopStartTime: 0, loopEndTime: 0 },
    { track: BGMTrack.GameOver,    clip: null, loopStartTime: 0, loopEndTime: 0 },
  ]
  // clip 참조는 S-6에서 오디오 파일 임포트 후 할당
  // loopStartTime/loopEndTime은 (-> see docs/systems/sound-design.md for 루프 사양)
```

> BGMTrack.None은 "BGM 없음" 상태이므로 entries에 포함하지 않는다.

- **MCP 호출**: 1회

#### S-3-07: SoundManager SO 참조 할당

S-3-02, S-3-03에서 생성된 SO를 SoundManager 컴포넌트에 연결한다.

```
set_property  target: "SoundManager/SoundManager"
  _soundRegistry = "Assets/_Project/Data/Audio/SoundRegistry.asset"
  _bgmRegistry = "Assets/_Project/Data/Audio/BGMRegistry.asset"

save_scene
```

- **MCP 호출**: 2회

#### S-3 검증 체크리스트

- [ ] `Assets/_Project/Data/Audio/SoundRegistry.asset` 존재
- [ ] `Assets/_Project/Data/Audio/BGMRegistry.asset` 존재
- [ ] MVP SoundData SO 에셋 전체 존재 (-> see docs/systems/sound-design.md 섹션 4.1 for 목록)
- [ ] SoundRegistry.entries 배열에 모든 MVP SoundData SO가 할당됨
- [ ] BGMRegistry.entries에 BGMTrack enum 값별 BGMEntry 등록 (TitleScreen, GameOver 포함)
- [ ] SoundManager 컴포넌트의 _soundRegistry, _bgmRegistry 필드가 SO를 참조

---

### S-4: SoundEventBridge 연결 및 이벤트 구독 검증

**목적**: SoundEventBridge 컴포넌트를 SoundManager에 추가하고, 게임 이벤트 -> 사운드 이벤트 연결이 정상 동작하는지 검증한다.

**전제**: S-3 완료, 의존 시스템(FarmEvents, EconomyEvents, TimeManager 등) 스크립트 컴파일 완료.

**의존 태스크**: S-3, ARC-003 (FarmEvents), ARC-005 (TimeManager)

#### S-4-01: SoundEventBridge 스크립트 생성

```
create_script
  path: "Assets/_Project/Scripts/Audio/SoundEventBridge.cs"
  // SoundEventBridge MonoBehaviour (-> see docs/systems/sound-architecture.md 섹션 8.1)

execute_menu_item  menu: "Assets/Refresh"  // 컴파일 대기
```

- **MCP 호출**: 2회

#### S-4-02: 컴포넌트 부착

```
add_component
  target: "SoundManager"
  type: "SeedMind.Audio.SoundEventBridge"

save_scene
```

- **MCP 호출**: 2회

#### S-4-03: 이벤트 구독 검증 -- Play Mode 테스트

```
enter_play_mode

// 검증 시나리오 1: 경작 SFX
execute_method
  // 경작 타일 경작 시뮬레이션 -> FarmEvents.OnTileTilled 발행
  // 콘솔에 "[SoundManager] PlaySFX: HoeTillBasic at (x,y)" 로그 확인

get_console_logs
  filter: "SoundManager"

// 검증 시나리오 2: 계절 변경 BGM
execute_method
  // 계절 변경 시뮬레이션 -> TimeManager.OnSeasonChanged 발행
  // 콘솔에 "[BGMScheduler] Season=Summer, Resolved=Summer" 로그 확인

get_console_logs
  filter: "BGMScheduler"

// 검증 시나리오 3: 상점 구매 SFX
execute_method
  // 상점 구매 시뮬레이션 -> EconomyEvents.OnShopPurchased 발행
  // 콘솔에 "[SoundManager] PlaySFX: Purchase (2D)" 로그 확인

get_console_logs
  filter: "SoundManager"

exit_play_mode
```

- **MCP 호출**: 8회

#### S-4-04: 볼륨 설정 저장/로드 검증

```
enter_play_mode

execute_method
  // SoundManager.Instance.SetVolume(AudioChannel.BGM, 0.5f)
  // SoundManager.Instance.SaveAudioSettings()
  // 콘솔에 "[SoundManager] AudioSettings saved" 로그 확인

execute_method
  // SoundManager.Instance.LoadAudioSettings()
  // SoundManager.Instance.GetVolume(AudioChannel.BGM) == 0.5f 확인

get_console_logs
  filter: "SoundManager"

exit_play_mode
```

- **MCP 호출**: 4회 (enter + 2 execute + get_logs) -- exit 포함하면 5회

#### S-4 검증 체크리스트

- [ ] SoundEventBridge 컴포넌트가 SoundManager에 부착됨
- [ ] 경작 이벤트 -> HoeTillBasic SFX 재생 로그 확인
- [ ] 계절 변경 -> BGMScheduler가 적절한 BGMTrack 결정 로그 확인
- [ ] 상점 구매 -> Purchase SFX 재생 로그 확인
- [ ] 볼륨 설정 SaveAudioSettings -> LoadAudioSettings 라운드트립 확인
- [ ] OnDisable 시 모든 이벤트 구독 해제 확인 (메모리 누수 없음)

---

### S-5: BGMScheduler 통합 테스트

**목적**: BGMScheduler의 계절 전환 crossfade, 날씨 우선순위, 실내/실외 전환, 타이틀/게임오버 BGM 전환을 검증한다.

**전제**: S-4 완료 (SoundEventBridge 연결 완료)

**의존 태스크**: S-4

#### S-5-01: 초기화 검증

```
enter_play_mode

// 게임 시작 시 현재 계절 BGM 자동 재생 확인
get_console_logs
  filter: "BGMScheduler"
  // "[BGMScheduler] Initialize: Season=Spring, Resolved=Spring" 로그 예상
```

- **MCP 호출**: 2회

#### S-5-02: 계절 전환 crossfade 검증

```
execute_method
  // TimeManager를 통해 계절을 Summer로 강제 변경
  // BGMScheduler.OnSeasonChanged(Season.Summer) 트리거

get_console_logs
  filter: "BGMScheduler"
  // "[BGMScheduler] Season=Summer, Resolved=Summer" 로그 확인
  // "[SoundManager] CrossfadeBGM: Spring -> Summer, duration=..." 로그 확인
```

- **MCP 호출**: 2회

#### S-5-03: 날씨 우선순위 검증

```
execute_method
  // WeatherSystem을 통해 날씨를 Storm으로 강제 변경
  // BGMScheduler.OnWeatherChanged(WeatherType.Storm) 트리거

get_console_logs
  filter: "BGMScheduler"
  // "[BGMScheduler] Weather=Storm, Resolved=Storm" 로그 확인
  // Storm BGM이 계절 BGM보다 우선 재생됨

execute_method
  // 날씨를 Clear로 복귀
  // BGMScheduler.OnWeatherChanged(WeatherType.Clear) 트리거

get_console_logs
  filter: "BGMScheduler"
  // "[BGMScheduler] Weather=Clear, Resolved=Summer" 로그 확인
  // 계절 BGM으로 복귀
```

- **MCP 호출**: 4회

#### S-5-04: 타이틀/게임오버 BGM 전환 검증

```
execute_method
  // BGMScheduler.ForceTrack(BGMTrack.TitleScreen) 호출

get_console_logs
  filter: "BGMScheduler"
  // "[BGMScheduler] Forced=TitleScreen, Resolved=TitleScreen" 로그 확인

execute_method
  // BGMScheduler.ForceTrack(BGMTrack.GameOver) 호출

get_console_logs
  filter: "BGMScheduler"
  // "[BGMScheduler] Forced=GameOver, Resolved=GameOver" 로그 확인

execute_method
  // BGMScheduler.ForceTrack(BGMTrack.None) 호출 -- 강제 BGM 해제

get_console_logs
  filter: "BGMScheduler"
  // "[BGMScheduler] Forced=None, Resolved={현재 계절}" 로그 확인
  // 계절 BGM으로 복귀

exit_play_mode
save_scene
```

- **MCP 호출**: 8회

#### S-5-05: 실내/실외 전환 검증

```
enter_play_mode

execute_method
  // BGMScheduler.SetLocationTrack(BGMTrack.IndoorHome) 호출

get_console_logs
  filter: "BGMScheduler"
  // "[BGMScheduler] Location=IndoorHome, Resolved=IndoorHome" 로그 확인
  // 실내 BGM이 날씨/계절보다 우선

execute_method
  // BGMScheduler.SetLocationTrack(BGMTrack.None) 호출 -- 실외 복귀

get_console_logs
  filter: "BGMScheduler"
  // 이전 상태(계절/날씨)로 복귀 확인

exit_play_mode
```

- **MCP 호출**: 6회

#### S-5 검증 체크리스트

- [ ] 게임 시작 시 현재 계절 BGM 자동 재생
- [ ] 계절 전환 -> crossfade 로그 확인 (전환 시간은 -> see docs/systems/sound-design.md 섹션 1.4)
- [ ] Storm 날씨 -> Storm BGM 우선 재생, Clear 복귀 -> 계절 BGM 복귀
- [ ] TitleScreen BGM 강제 재생 -> 해제 시 계절 BGM 복귀
- [ ] GameOver BGM 강제 재생 확인
- [ ] 실내(IndoorHome) BGM -> 실외 복귀 시 이전 BGM 복귀
- [ ] BGM 우선순위 스택 동작 확인 (-> see docs/systems/sound-design.md 섹션 1.4 BGM 우선순위 스택)

---

### S-6: MVP 사운드 에셋 임포트 및 연결

**목적**: sound-design.md 섹션 4.1에 정의된 MVP 사운드 에셋을 Unity 프로젝트에 임포트하고, 각 SoundData SO의 clips 배열과 BGMRegistry의 clip 참조에 연결한다.

**전제**: S-3 완료 (SoundData SO 에셋 생성 완료), 오디오 파일이 외부 경로에 준비되어 있어야 한다.

**의존 태스크**: S-3

#### S-6-01: SFX 오디오 파일 임포트

MVP SFX 목록에 해당하는 오디오 파일을 임포트한다 (-> see docs/systems/sound-design.md 섹션 4.1 for MVP SFX 목록).

```
// 패턴 -- 각 SFX 파일 반복
import_asset
  source: "{외부 오디오 파일 경로}"
  destination: "Assets/_Project/Audio/SFX/{sfx_id}.wav"
  // 파일 사양: (-> see docs/systems/sound-design.md 섹션 3.6)
  //   포맷: wav, 샘플레이트: 44100Hz, 비트뎁스: 16bit, 채널: Mono
```

> **오디오 파일 소스**: 외부 도구(SFXR, Freesound, AI 생성 등)로 사전 제작된 파일이 필요. MCP는 오디오 파일을 생성하지 않으며, 이미 준비된 파일을 임포트만 한다.

- **MCP 호출**: MVP SFX 수 x 1회 (-> see docs/systems/sound-design.md 섹션 4.1 for MVP 합계)

#### S-6-02: BGM 오디오 파일 임포트

MVP BGM은 1트랙(bgm_spring)만 포함한다 (-> see docs/systems/sound-design.md 섹션 4.1).

```
import_asset
  source: "{외부 BGM 파일 경로}"
  destination: "Assets/_Project/Audio/BGM/bgm_spring.ogg"
  // 파일 사양: (-> see docs/systems/sound-design.md 섹션 3.6)
  //   포맷: ogg (Vorbis), Stereo
```

- **MCP 호출**: 1회

#### S-6-03: AudioClip Import 설정

임포트된 오디오 파일의 Unity 임포트 설정을 조정한다.

```
// SFX 파일: forceToMono=true (3D 대상), loadInBackground=false
// BGM 파일: forceToMono=false (Stereo 유지), loadInBackground=true

// 패턴 -- 각 SFX 파일
set_property  target: "Assets/_Project/Audio/SFX/{sfx_id}.wav"
  loadType = "DecompressOnLoad"
  compressionFormat = "PCM"
  // (-> see docs/systems/sound-design.md 섹션 3.6 for 사양)

// BGM 파일
set_property  target: "Assets/_Project/Audio/BGM/bgm_spring.ogg"
  loadType = "Streaming"
  compressionFormat = "Vorbis"
  quality = 0
  // (-> see docs/systems/sound-design.md 섹션 3.6 for quality 값)
```

- **MCP 호출**: MVP SFX 수 + 1회 (BGM)

#### S-6-04: SoundData SO에 AudioClip 할당

각 SoundData SO의 `clips` 배열에 임포트된 AudioClip을 할당한다.

```
// 패턴 -- 각 SoundData SO
set_property  target: "SD_HoeTillBasic"
  clips = ["Assets/_Project/Audio/SFX/sfx_hoe_basic.wav"]

set_property  target: "SD_SeedPlant"
  clips = ["Assets/_Project/Audio/SFX/sfx_seed_plant.wav"]

// ... MVP SFX 전체 반복
// (-> see docs/systems/sound-design.md 섹션 4.1 for 전체 목록)
```

- **MCP 호출**: MVP SFX 수 x 1회

#### S-6-05: BGMRegistry에 AudioClip 할당

```
set_property  target: "BGMRegistry"
  entries[0].clip = "Assets/_Project/Audio/BGM/bgm_spring.ogg"
  // Spring BGM만 MVP에서 연결
  // 나머지 트랙은 Phase 3 이후 에셋 준비 시 할당
```

- **MCP 호출**: 1회

#### S-6-06: 임포트 결과 검증

```
enter_play_mode

// Spring BGM 재생 확인
get_console_logs
  filter: "SoundManager"
  // "[SoundManager] PlayBGM: Spring" 로그 + 실제 오디오 출력 확인

// 경작 SFX 재생 확인
execute_method
  // 경작 시뮬레이션 -> HoeTillBasic SFX 재생
  // 콘솔에 "[SoundManager] PlaySFX: HoeTillBasic" + AudioClip 재생 확인

get_console_logs
  filter: "SoundManager"

exit_play_mode
save_scene
```

- **MCP 호출**: 5회

#### S-6 검증 체크리스트

- [ ] MVP SFX 오디오 파일 전체가 `Assets/_Project/Audio/SFX/` 에 임포트됨 (-> see docs/systems/sound-design.md 섹션 4.1)
- [ ] MVP BGM 오디오 파일(bgm_spring.ogg)이 `Assets/_Project/Audio/BGM/`에 임포트됨
- [ ] SFX 파일: DecompressOnLoad, PCM, 3D 대상은 forceToMono
- [ ] BGM 파일: Streaming, Vorbis, Stereo 유지
- [ ] 모든 MVP SoundData SO의 clips 배열에 AudioClip 할당 완료
- [ ] BGMRegistry의 Spring entry에 bgm_spring.ogg clip 할당 완료
- [ ] Play Mode에서 Spring BGM 재생 확인
- [ ] Play Mode에서 경작 SFX 재생 확인

---

## 5. 태스크 실행 순서 요약

```
S-1: AudioMixer 에셋 생성 (~18회)
 │
 ▼
S-2: SoundManager GameObject 구성 (~28회)
 │   ├── Phase 0: Enum/Struct 스크립트 생성
 │   ├── Phase 1: SO 데이터 레이어 스크립트 생성
 │   ├── Phase 2: SFXPool, BGMScheduler 스크립트 생성
 │   ├── Phase 3: SoundManager 스크립트 생성
 │   └── GameObject + AudioSource 배치
 │
 ▼
S-3: ScriptableObject 에셋 생성 (~45회)
 │   ├── SoundRegistry, BGMRegistry SO 생성
 │   ├── MVP SoundData SO 생성
 │   └── SoundManager SO 참조 할당
 │
 ├───────────────────────┐
 ▼                       ▼
S-4: SoundEventBridge    S-6: MVP 사운드 에셋
     연결 및 검증             임포트 및 연결
     (~12회)                  (~35회)
 │                       │
 ▼                       │
S-5: BGMScheduler        │
     통합 테스트          │
     (~10회)              │
 │                       │
 └───────┬───────────────┘
         ▼
     전체 완료
```

> S-4와 S-6은 병렬 실행 가능 (S-3 완료 후). 단, S-5는 S-4 완료 후에만 실행.
> S-6은 오디오 파일이 사전 준비되어 있어야 실행 가능.

---

## Cross-references

| 참조 문서 | 관계 |
|----------|------|
| `docs/systems/sound-architecture.md` (AUD-001) | 본 문서의 원본 아키텍처. Part I 시스템 설계, Part II MCP 태스크 개요 |
| `docs/systems/sound-design.md` (DES-014) | **canonical** -- BGM 트랙 목록, SFX 전체 목록, poolSize, 볼륨/거리 수치, MVP 사운드 목록 |
| `docs/mcp/scene-setup-tasks.md` (ARC-002) | 의존 -- 폴더 구조, 씬 계층 |
| `docs/mcp/farming-tasks.md` (ARC-003) | 의존 -- FarmEvents 이벤트 허브 |
| `docs/mcp/time-season-tasks.md` (ARC-005) | 의존 -- TimeManager, WeatherSystem |
| `docs/systems/project-structure.md` | 네임스페이스/폴더 구조 |
| `docs/pipeline/data-pipeline.md` | SO 패턴 참조 |

---

## Open Questions

1. **[OPEN]** AudioMixer 에셋과 AudioMixerGroup을 MCP로 생성할 수 있는지 검증되지 않았다. `create_asset`, `add_mixer_group`, `expose_parameter` MCP 도구가 AudioMixer를 지원하지 않을 경우, S-1 전체를 Editor 스크립트 또는 수동 조작으로 대체해야 한다.

2. **[OPEN]** SoundData SO의 `clips` 배열에 AudioClip 참조를 MCP `set_property`로 설정할 수 있는지 검증 필요. 배열 타입 참조 설정이 미지원일 경우 Editor 스크립트 우회가 필요하다.

3. **[OPEN]** Ambient 사운드 레이어링 -- 현재 단일 Ambient AudioSource로 설계했으나, 새소리 + 바람소리 동시 재생이 필요한 경우 Ambient 풀 확장이 필요할 수 있다 (-> see docs/systems/sound-architecture.md Open Questions 1번).

---

## Risks

1. **[RISK]** **AudioMixer MCP 생성 미지원 가능성**: Unity AudioMixer는 특수 에셋 타입으로, MCP for Unity에서 프로그래밍 방식 생성을 지원하지 않을 수 있다. 대안: Editor 스크립트 `AssetDatabase.CreateAsset` + `AudioMixerController` API 사용, 또는 사전 제작된 MainMixer.mixer 파일을 프로젝트에 수동 배치.

2. **[RISK]** **SO 배열 참조 설정**: MCP `set_property`가 ScriptableObject 배열 필드(SoundRegistry.entries, SoundData.clips)의 참조 설정을 지원하지 않을 수 있다. 대안: Editor 스크립트를 통한 일괄 할당.

3. **[RISK]** **오디오 파일 사전 준비 필요**: S-6은 외부에서 제작된 오디오 파일이 존재해야 실행 가능하다. 오디오 파일이 없으면 S-6 전체가 블로킹된다. MVP 단계에서는 placeholder 사운드(SFXR 생성 등)로 대체할 수 있다.

4. **[RISK]** **MCP 호출 수 최적화**: 총 ~148회 MCP 호출 중 S-3(SO 생성)이 가장 많다. Editor 스크립트 일괄 생성으로 대폭 감소 가능. MCP 단독 실행 시 시간 비용을 고려하여 Editor 스크립트 우회를 우선 검토해야 한다.

5. **[RISK]** **DontDestroyOnLoad 중복**: 메인 메뉴 -> 게임 씬 전환 시 SoundManager 중복 생성 가능성. Awake 중복 검사로 방어하지만 전환 순간 짧은 시간 두 인스턴스 공존 가능 (-> see docs/systems/sound-architecture.md Risks 5번).
