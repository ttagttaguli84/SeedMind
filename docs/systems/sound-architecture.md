# 사운드 시스템 기술 아키텍처

> SoundManager, BGMScheduler, SFX 풀, AudioMixer 구조, SoundData SO, 이벤트 연동, MCP 구현 계획  
> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: AUD-001

---

## Context

이 문서는 SeedMind의 사운드 시스템에 대한 기술 아키텍처를 정의한다. 사운드 시스템은 BGM, SFX(효과음), Ambient(환경음), UI 사운드를 관리하며, 기존 이벤트 시스템(FarmEvents, TimeManager, EconomyEvents, ProgressionManager, QuestEvents, AchievementEvents)과 구독 관계를 통해 게임 상태에 반응하는 오디오를 재생한다.

**설계 목표**:
- 모든 사운드 재생이 이벤트 기반으로 트리거되어야 한다 -- SoundManager가 직접 게임 로직에 의존하지 않는다
- BGM 전환은 계절/날씨/시간대 변화에 자동 반응하며, crossfade로 부드럽게 전환된다
- SFX는 AudioSource 풀로 관리하여 동시 재생 수를 제어한다
- 모든 볼륨/거리 수치는 ScriptableObject로 외부화하여 코드 변경 없이 튜닝 가능
- AudioMixer를 통해 채널별 독립 볼륨 제어와 설정 저장이 가능하다

---

# Part I -- 시스템 설계

---

## 1. Enum 및 타입 정의

### 1.1 AudioChannel

```csharp
namespace SeedMind.Audio
{
    /// <summary>
    /// AudioMixer의 채널 분류. 각 채널은 독립적인 볼륨/뮤트 제어를 가진다.
    /// </summary>
    public enum AudioChannel
    {
        Master,     // 전체 마스터 볼륨
        BGM,        // 배경 음악
        SFX,        // 효과음 (게임플레이)
        Ambient,    // 환경음 (새소리, 바람, 비 등)
        UI          // UI 효과음 (버튼 클릭, 알림 등)
    }
}
```

### 1.2 BGMTrack

```csharp
namespace SeedMind.Audio
{
    /// <summary>
    /// 배경 음악 트랙 식별자. 계절별 기본 BGM + 특수 상황 BGM.
    /// 트랙 목록은 sound-design.md가 canonical이다.
    /// </summary>
    public enum BGMTrack
    {
        // 계절별 기본 BGM (→ see docs/systems/sound-design.md)
        Spring,
        Summer,
        Autumn,
        Winter,

        // 시간대별 BGM (→ see docs/systems/sound-design.md)
        NightTime,

        // 특수 상황 BGM (→ see docs/systems/sound-design.md)
        Festival,
        IndoorHome,     // bgm_indoor_home: 자택 실내
        Shop,           // bgm_indoor_shop: 상점
        Rain,           // bgm_rain: Rain/HeavyRain 날씨
        Storm,          // bgm_storm: Storm 날씨 (Blizzard도 이 트랙 재사용 — [OPEN] bgm_blizzard 별도 트랙 검토 필요)

        // 시스템 BGM
        TitleScreen,
        GameOver,

        None        // BGM 없음 (정지 상태)
    }
}
```

### 1.3 SFXId

```csharp
namespace SeedMind.Audio
{
    /// <summary>
    /// 효과음 식별자. 전체 SFX 목록은 sound-design.md가 canonical이다.
    /// SoundData SO의 id 필드와 1:1 매핑된다.
    /// </summary>
    public enum SFXId
    {
        // === 경작 — 등급별 (→ see docs/systems/sound-design.md 섹션 2.1) ===
        HoeTillBasic,           // sfx_hoe_basic: 기본 호미
        HoeTillReinforced,      // sfx_hoe_reinforced: 강화 호미
        HoeTillLegendary,       // sfx_hoe_legendary: 전설 호미
        WaterBasic,             // sfx_water_basic: 기본 물뿌리개
        WaterReinforced,        // sfx_water_reinforced: 강화 물뿌리개
        WaterLegendary,         // sfx_water_legendary: 전설 물뿌리개
        ScytheBasic,            // sfx_scythe_basic: 기본 낫
        ScytheReinforced,       // sfx_scythe_reinforced: 강화 낫
        ScytheLegendary,        // sfx_scythe_legendary: 전설 낫
        SeedPlant,              // sfx_seed_plant: 씨앗 심기
        Harvest,                // sfx_harvest: 작물 수확
        Fertilize,              // sfx_fertilize: 비료 뿌리기

        // === 작물 (→ see docs/systems/sound-design.md 섹션 2.2) ===
        CropGrow,               // sfx_crop_grow: 성장 단계 전환
        CropWither,             // sfx_crop_wither: 작물 시들음
        CropGolden,             // sfx_crop_golden: 황금 품질 수확
        CropGiant,              // sfx_crop_giant: 거대 작물 발견

        // === 도구 (→ see docs/systems/sound-design.md 섹션 2.3) ===
        ToolEquip,              // sfx_tool_equip: 도구 장착
        ToolUpgradeStart,       // sfx_tool_upgrade_start: 업그레이드 의뢰
        ToolUpgradeComplete,    // sfx_tool_upgrade_complete: 업그레이드 수령
        ToolBreak,              // sfx_tool_break: 에너지 부족 사용 시도

        // === 시설/건설 (→ see docs/systems/sound-design.md 섹션 2.4) ===
        ConstructStart,         // sfx_construct_start: 건설 시작
        ConstructComplete,      // sfx_construct_complete: 건설 완료
        ConstructUpgrade,       // sfx_construct_upgrade: 시설 업그레이드
        FacilityActivate,       // sfx_facility_activate: 시설 가동
        FacilityIdle,           // sfx_facility_idle: 시설 대기 루프 (3D)

        // === 가공 (→ see docs/systems/sound-design.md 섹션 2.5) ===
        ProcessStart,           // sfx_process_start: 가공 시작
        ProcessComplete,        // sfx_process_complete: 가공 완료
        MillRunning,            // sfx_mill_running: 제분소 가동음 루프 (3D)
        FermentBubble,          // sfx_ferment_bubble: 발효실 가동음 루프 (3D)
        BakeryOven,             // sfx_bakery_oven: 베이커리 가동음 루프 (3D)
        CheeseChurn,            // sfx_cheese_churn: 치즈 공방 가동음 루프 (3D)

        // === 목축 (→ see docs/systems/sound-design.md 섹션 2.6) ===
        AnimalFeed,             // sfx_animal_feed: 동물 먹이주기
        AnimalMilk,             // sfx_milk: 우유 짜기
        AnimalShear,            // sfx_shear: 털 깎기
        EggCollect,             // sfx_egg_collect: 알 줍기
        ChickenCluck,           // sfx_chicken_cluck: 닭 울음 (3D, variation)
        CowMoo,                 // sfx_cow_moo: 소 울음 (3D, variation)
        SheepBaa,               // sfx_sheep_baa: 양 울음 (3D, variation)
        GoatBleat,              // sfx_goat_bleat: 염소 울음 (3D, variation)
        AnimalHappy,            // sfx_animal_happy: 동물 행복 반응
        AnimalSick,             // sfx_animal_sick: 동물 아픔

        // === 낚시 (→ see docs/systems/sound-design.md 섹션 2.7) ===
        FishCastLine,           // sfx_cast_line: 낚싯줄 던지기 → FishingEvents.OnFishCast
        FishNibble,             // sfx_fish_nibble: 물고기 탐색 (찌 흔들림 반복)
        FishBite,               // sfx_fish_bite: 물고기 입질 → FishingEvents.OnFishBite
        FishReelIn,             // sfx_reel_in: 줄 감기 루프
        FishStruggle,           // sfx_fish_struggle: 물고기 저항
        FishCatchNormal,        // sfx_fish_catch_normal: 일반 낚시 성공
        FishCatchRare,          // sfx_fish_catch_rare: 희귀 낚시 성공
        FishEscape,             // sfx_fish_escape: 낚시 실패 → FishingEvents.OnFishingFailed
        FishSplash,             // sfx_fish_splash: 낚시터 물 튀김 환경 연출 (3D)

        // === NPC/상점 (→ see docs/systems/sound-design.md 섹션 2.8) ===
        ShopOpen,               // sfx_shop_open: 상점 열기
        ShopClose,              // sfx_shop_close: 상점 닫기
        Purchase,               // sfx_purchase: 구매 완료
        Sale,                   // sfx_sell: 판매 완료
        DialogueStart,          // sfx_dialog_start: 대화 시작
        DialogueAdvance,        // sfx_dialog_advance: 대화 넘기기
        DialogueChoice,         // sfx_dialog_choice: 선택지 선택
        AffinityUp,             // sfx_affinity_up: NPC 친밀도 상승
        ShippingBin,            // sfx_shipping_bin: 출하함 투입

        // === 퀘스트/업적 (→ see docs/systems/sound-design.md 섹션 2.9) ===
        QuestAccept,            // sfx_quest_accept: 퀘스트 수락
        QuestProgress,          // sfx_quest_progress: 퀘스트 목표 부분 달성
        QuestComplete,          // sfx_quest_complete: 퀘스트 완료
        QuestReward,            // sfx_quest_reward: 퀘스트 보상 수령
        AchievementToast,       // sfx_achievement_toast: 업적 달성 토스트

        // === UI (→ see docs/systems/sound-design.md 섹션 2.10) ===
        UIClick,                // sfx_ui_click: 버튼 클릭
        UIHover,                // sfx_ui_hover: 버튼 호버
        UITab,                  // sfx_ui_tab: 탭 전환
        InventoryOpen,          // sfx_inventory_open: 인벤토리 열기
        InventoryClose,         // sfx_inventory_close: 인벤토리 닫기
        ItemMove,               // sfx_item_move: 아이템 드래그
        Notification,           // sfx_notification: 시스템 알림
        UIError,                // sfx_error: 에러/불가 동작
        UIConfirm,              // sfx_confirm: 확인
        UICancel,               // sfx_cancel: 취소

        // === 환경/날씨 Ambient (→ see docs/systems/sound-design.md 섹션 2.11) ===
        AmbRainLight,           // amb_rain_light: 가벼운 비
        AmbRainHeavy,           // amb_rain_heavy: 강한 비
        AmbSnow,                // amb_snow: 눈 (발자국 + 바람)
        AmbWindLight,           // amb_wind_light: 바람
        AmbBirdsDay,            // amb_birds_day: 낮 새소리
        AmbStorm,               // amb_storm: 폭풍
        Thunder,                // sfx_thunder: 천둥
        LightningFlash,         // sfx_lightning_flash: 번개
        AmbBlizzard,            // amb_blizzard: 눈보라
        AmbCicada,              // amb_cicada: 여름 매미
        AmbCricket,             // amb_cricket: 귀뚜라미
        AmbInsectsNight,        // amb_insects_night: 밤 벌레 소리
        AmbWaves,               // amb_waves: 낚시터 파도 루프 (3D, Linear)
        FootstepDirt,           // sfx_footstep_dirt: 흙 위 발걸음
        FootstepGrass,          // sfx_footstep_grass: 잔디 위 발걸음
        FootstepWood,           // sfx_footstep_wood: 나무 바닥 발걸음
        FootstepSnow,           // sfx_footstep_snow: 눈 위 발걸음
        FootstepStone,          // sfx_footstep_stone: 돌 위 발걸음

        // === 진행/레벨 (→ see docs/systems/sound-design.md 섹션 2.12) ===
        LevelUp,                // sfx_level_up: 레벨업
        XPGain,                 // sfx_xp_gain: 경험치 획득
        GoldGain,               // sfx_gold_gain: 골드 획득
        GoldSpend,              // sfx_gold_spend: 골드 지출
        EnergyWarning,          // sfx_energy_warning: 에너지 낮음 경고
        EnergyDepleted,         // sfx_energy_depleted: 에너지 소진

        // === 시간/계절 (→ see docs/systems/sound-design.md 섹션 2.13) ===
        MorningChime,           // sfx_morning_chime: 하루 시작 알림
        EveningBell,            // sfx_evening_bell: 저녁 시작
        MidnightWarning,        // sfx_midnight_warning: 자정 경고
        PassOut,                // sfx_pass_out: 기절
        Sleep,                  // sfx_sleep: 수면
        SeasonTransition,       // sfx_season_transition: 계절 전환
        DaySummary              // sfx_day_summary: 하루 정산
    }
}
```

### 1.4 SoundEvent 구조체

```csharp
namespace SeedMind.Audio
{
    /// <summary>
    /// SFX 재생 요청을 캡슐화하는 구조체.
    /// PlaySFX 내부에서 생성되어 AudioSource 풀에 전달된다.
    /// </summary>
    public struct SoundEvent
    {
        public SFXId Id;
        public Vector3? Position;       // null이면 2D 재생
        public float VolumeScale;       // 1.0 = SoundData.baseVolume 그대로
        public float PitchOverride;     // 0이면 SoundData 기본 + variation 적용
    }
}
```

---

## 2. SoundManager 클래스 설계

### 2.1 클래스 다이어그램

```
┌─────────────────────────────────────────────────────────────────────┐
│                         SeedMind.Audio                               │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│              SoundManager (MonoBehaviour, Singleton)           │
│──────────────────────────────────────────────────────────────│
│  [AudioSource 참조]                                           │
│  - _bgmSourceA: AudioSource        // crossfade용 A 채널     │
│  - _bgmSourceB: AudioSource        // crossfade용 B 채널     │
│  - _ambientSource: AudioSource     // 환경음 전용             │
│  - _sfxPool: SFXPool (내부 인스턴스) // SFX AudioSource 풀    │
│  - _uiSource: AudioSource          // UI 전용 (2D only)      │
│                                                              │
│  [설정 참조]                                                   │
│  - _mixer: AudioMixer              // Unity AudioMixer 에셋  │
│  - _soundRegistry: SoundRegistry   // SFXId → SoundData 매핑 │
│  - _bgmRegistry: BGMRegistry       // BGMTrack → AudioClip   │
│  - _bgmScheduler: BGMScheduler (내부 인스턴스)                 │
│                                                              │
│  [상태]                                                       │
│  - _currentBGM: BGMTrack                                     │
│  - _activeBGMSource: int  (0=A, 1=B) // 현재 활성 BGM 소스   │
│  - _isCrossfading: bool                                      │
│                                                              │
│  [읽기 전용 프로퍼티]                                           │
│  + CurrentBGM: BGMTrack                                      │
│  + IsMuted(AudioChannel channel): bool                       │
│                                                              │
│  [메서드 -- BGM]                                              │
│  + PlayBGM(BGMTrack track, float fadeTime): void             │
│  + StopBGM(float fadeTime): void                             │
│  + CrossfadeBGM(BGMTrack to, float duration): void           │
│  - CrossfadeCoroutine(AudioClip clip, float duration):       │
│       IEnumerator                                            │
│                                                              │
│  [메서드 -- SFX]                                              │
│  + PlaySFX(SFXId id, Vector3? position = null): void         │
│  + PlaySFX(SoundEvent evt): void                             │
│  + PlaySFXWithDelay(SFXId id, float delay): void             │
│                                                              │
│  [메서드 -- Ambient]                                          │
│  + PlayAmbient(AudioClip clip, float fadeTime): void         │
│  + StopAmbient(float fadeTime): void                         │
│                                                              │
│  [메서드 -- 볼륨/설정]                                         │
│  + SetVolume(AudioChannel channel, float volume): void       │
│  + GetVolume(AudioChannel channel): float                    │
│  + Mute(AudioChannel channel, bool muted): void              │
│  + SaveAudioSettings(): void                                 │
│  + LoadAudioSettings(): void                                 │
│                                                              │
│  [구독]                                                       │
│  + OnEnable():                                               │
│      SoundEventBridge.OnSFXRequested += HandleSFXRequest     │
│      SoundEventBridge.OnBGMRequested += HandleBGMRequest     │
│  + OnDisable(): 구독 해제                                      │
└──────────────────────────────────────────────────────────────┘
         │ owns                          │ ref
         ▼                               ▼
┌────────────────────────┐     ┌──────────────────────────────┐
│ SFXPool                │     │  SoundRegistry               │
│ (Plain C# class)       │     │  (ScriptableObject)          │
│                        │     │──────────────────────────────│
│ (아래 3절 참조)        │     │  entries: SoundData[]        │
│                        │     │  _lookup: Dict<SFXId,        │
│                        │     │    SoundData>                │
│                        │     │  + Get(SFXId): SoundData     │
└────────────────────────┘     └──────────────────────────────┘
         │                               │
         ▼                               ▼
┌────────────────────────┐     ┌──────────────────────────────┐
│ BGMScheduler           │     │  BGMRegistry                 │
│ (Plain C# class)       │     │  (ScriptableObject)          │
│                        │     │──────────────────────────────│
│ (아래 6절 참조)        │     │  entries: BGMEntry[]         │
│                        │     │  + GetClip(BGMTrack):        │
│                        │     │    AudioClip                 │
└────────────────────────┘     └──────────────────────────────┘
```

### 2.2 네임스페이스 배치

```
Assets/_Project/Scripts/Audio/          (→ see docs/systems/project-structure.md)
├── SoundManager.cs                     # 싱글턴 매니저
├── SFXPool.cs                          # AudioSource 풀
├── BGMScheduler.cs                     # 계절/날씨 BGM 자동 전환
├── SoundEventBridge.cs                 # 정적 이벤트 허브 (다른 시스템 → 사운드)
├── Data/
│   ├── SoundData.cs                    # SFX SO
│   ├── SoundRegistry.cs               # SFXId → SoundData 매핑 SO
│   ├── BGMRegistry.cs                  # BGMTrack → AudioClip 매핑 SO
│   └── AudioSettingsData.cs            # 유저 볼륨 설정 직렬화 클래스
```

### 2.3 Singleton 패턴

기존 `GameManager`, `TimeManager`, `EconomyManager`와 동일한 싱글턴 패턴을 따른다:

```csharp
namespace SeedMind.Audio
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePools();
        }
    }
}
```

> `DontDestroyOnLoad` 적용 -- 씬 전환 시에도 BGM이 끊기지 않아야 한다.

---

## 3. SFX AudioSource 풀 설계

### 3.1 SFXPool 클래스

```csharp
namespace SeedMind.Audio
{
    /// <summary>
    /// SFX 재생용 AudioSource 오브젝트 풀.
    /// 풀 크기와 교체 정책은 sound-design.md를 참조한다.
    /// </summary>
    public class SFXPool
    {
        private AudioSource[] _sources;
        private int _poolSize;  // → see docs/systems/sound-design.md
        private int _nextIndex;

        public SFXPool(Transform parent, int poolSize, AudioMixerGroup sfxGroup)
        {
            _poolSize = poolSize;  // → see docs/systems/sound-design.md
            _sources = new AudioSource[_poolSize];
            _nextIndex = 0;

            for (int i = 0; i < _poolSize; i++)
            {
                var go = new GameObject($"SFX_Source_{i}");
                go.transform.SetParent(parent);
                _sources[i] = go.AddComponent<AudioSource>();
                _sources[i].outputAudioMixerGroup = sfxGroup;
                _sources[i].playOnAwake = false;
            }
        }

        public AudioSource GetAvailable()
        {
            // Round-robin 방식으로 순환
            // 모든 소스가 사용 중이면 가장 오래된 것을 교체 (oldest-replace 정책)
            var source = _sources[_nextIndex];
            _nextIndex = (_nextIndex + 1) % _poolSize;
            return source;
        }

        public void Play(SoundEvent evt, SoundData data)
        {
            var source = GetAvailable();

            // variation 배열에서 랜덤 선택
            var clip = data.clips[Random.Range(0, data.clips.Length)];

            source.clip = clip;
            source.volume = data.baseVolume * evt.VolumeScale;
                // baseVolume → see docs/systems/sound-design.md

            // pitch variation 적용
            source.pitch = (evt.PitchOverride > 0f)
                ? evt.PitchOverride
                : 1f + Random.Range(-data.pitchVariation, data.pitchVariation);
                // pitchVariation → see docs/systems/sound-design.md

            // 3D vs 2D 설정
            if (data.is3D && evt.Position.HasValue)
            {
                source.spatialBlend = 1f;   // 완전 3D
                source.transform.position = evt.Position.Value;
                source.maxDistance = data.maxDistance;
                    // maxDistance → see docs/systems/sound-design.md
                source.rolloffMode = AudioRolloffMode.Logarithmic;
                    // [OPEN] amb_waves(파도)는 Linear 모드 사용 — SoundData에 rolloffMode 필드 추가 검토 필요
                    // (→ see docs/systems/sound-design.md 섹션 3.4)
            }
            else
            {
                source.spatialBlend = 0f;   // 완전 2D
            }

            source.Play();
        }
    }
}
```

### 3.2 풀 크기 결정 근거

| 항목 | 설명 |
|------|------|
| 풀 크기 | (-> see docs/systems/sound-design.md) |
| 동시 재생 상한 | 풀 크기와 동일 -- 풀 고갈 시 oldest-replace 정책 적용 |
| 교체 정책 | **Oldest-replace**: 모든 슬롯이 사용 중이면 가장 먼저 시작된 SFX를 중단하고 새 SFX에 할당 |
| 교체 정책 근거 | 무시(skip) 정책은 중요 SFX가 누락될 수 있어 부적합. oldest는 이미 재생 완료에 가까워 중단 시 위화감이 최소 |

### 3.3 3D SFX vs 2D SFX 구분

| 분류 | spatialBlend | 대상 SFX 예시 | 설명 |
|------|-------------|---------------|------|
| 3D SFX | 1.0 | HoeTill, WateringCan, SeedPlant, HarvestNormal, BuildPlace, AnimalFeed | 월드 위치 기반 거리 감쇠 적용. 카메라와의 거리에 따라 볼륨 변화 |
| 2D SFX | 0.0 | UIClick, UIOpen, Purchase, LevelUp, AchievementToast, MorningChime | 위치 무관, 항상 동일 볼륨. UI/시스템 피드백 |

> `SoundData.is3D` 필드로 SO별 개별 설정. 기본값 결정은 (-> see docs/systems/sound-design.md).

---

## 4. Unity AudioMixer 채널 구조

### 4.1 Mixer 계층

```
AudioMixer "MainMixer"
├── Master
│   ├── BGM          (exposed: "BGMVolume")
│   ├── SFX          (exposed: "SFXVolume")
│   ├── Ambient      (exposed: "AmbientVolume")
│   └── UI           (exposed: "UIVolume")
```

### 4.2 Exposed Parameter 규칙

| AudioChannel | Exposed Parameter Name | 범위 | 설명 |
|-------------|----------------------|------|------|
| Master | `MasterVolume` | -80dB ~ 0dB | 전체 마스터 |
| BGM | `BGMVolume` | -80dB ~ 0dB | 배경 음악 |
| SFX | `SFXVolume` | -80dB ~ 0dB | 효과음 |
| Ambient | `AmbientVolume` | -80dB ~ 0dB | 환경음 |
| UI | `UIVolume` | -80dB ~ 0dB | UI 효과음 |

> 네이밍 규칙: `{AudioChannel}Volume` -- 채널 enum 이름 + "Volume" 접미사.

### 4.3 볼륨 변환 공식

```csharp
// 유저가 설정하는 0.0~1.0 슬라이더 값을 dB로 변환
private float LinearToDecibel(float linear)
{
    // 0이면 완전 무음 (-80dB), 1이면 0dB
    return (linear > 0.0001f)
        ? Mathf.Log10(linear) * 20f
        : -80f;
}
```

### 4.4 SetVolume / GetVolume 구현

```csharp
public void SetVolume(AudioChannel channel, float volume)
{
    // volume: 0.0 ~ 1.0 (유저 슬라이더 값)
    string paramName = GetExposedParamName(channel);
    _mixer.SetFloat(paramName, LinearToDecibel(Mathf.Clamp01(volume)));
}

public float GetVolume(AudioChannel channel)
{
    string paramName = GetExposedParamName(channel);
    _mixer.GetFloat(paramName, out float dbValue);
    return Mathf.Pow(10f, dbValue / 20f); // dB → linear
}

private string GetExposedParamName(AudioChannel channel)
{
    return channel switch
    {
        AudioChannel.Master  => "MasterVolume",
        AudioChannel.BGM     => "BGMVolume",
        AudioChannel.SFX     => "SFXVolume",
        AudioChannel.Ambient => "AmbientVolume",
        AudioChannel.UI      => "UIVolume",
        _ => throw new ArgumentOutOfRangeException(nameof(channel))
    };
}
```

> switch 문은 AudioChannel enum의 모든 값을 빠짐없이 처리한다. AudioChannel에 값이 추가되면 이 switch도 동시 업데이트 필수.

---

## 5. ScriptableObject 설계

### 5.1 SoundData -- SFX 데이터

```csharp
namespace SeedMind.Audio.Data
{
    [CreateAssetMenu(menuName = "SeedMind/Audio/SoundData")]
    public class SoundData : ScriptableObject
    {
        [Header("식별")]
        public SFXId id;

        [Header("클립")]
        public AudioClip[] clips;       // variation 배열 (랜덤 선택)

        [Header("재생 설정")]
        public float baseVolume;        // → see docs/systems/sound-design.md
        public float pitchVariation;    // → see docs/systems/sound-design.md
                                        // 예: 0.1이면 pitch 0.9~1.1 범위

        [Header("공간 설정")]
        public bool is3D;               // true: 3D (위치 기반), false: 2D
        public float maxDistance;       // → see docs/systems/sound-design.md
                                        // 3D SFX의 최대 청취 거리 (Unity 단위)
    }
}
```

### 5.2 SoundRegistry -- SFXId-SoundData 매핑

```csharp
namespace SeedMind.Audio.Data
{
    [CreateAssetMenu(menuName = "SeedMind/Audio/SoundRegistry")]
    public class SoundRegistry : ScriptableObject
    {
        [SerializeField]
        private SoundData[] entries;

        // 런타임 조회용 딕셔너리 (Initialize에서 구축)
        private Dictionary<SFXId, SoundData> _lookup;

        public void Initialize()
        {
            _lookup = new Dictionary<SFXId, SoundData>(entries.Length);
            foreach (var entry in entries)
            {
                if (!_lookup.TryAdd(entry.id, entry))
                {
                    Debug.LogWarning($"[SoundRegistry] 중복 SFXId: {entry.id}");
                }
            }
        }

        public SoundData Get(SFXId id)
        {
            if (_lookup.TryGetValue(id, out var data))
                return data;

            Debug.LogError($"[SoundRegistry] 미등록 SFXId: {id}");
            return null;
        }
    }
}
```

### 5.3 BGMRegistry -- BGMTrack-AudioClip 매핑

```csharp
namespace SeedMind.Audio.Data
{
    [CreateAssetMenu(menuName = "SeedMind/Audio/BGMRegistry")]
    public class BGMRegistry : ScriptableObject
    {
        [System.Serializable]
        public struct BGMEntry
        {
            public BGMTrack track;
            public AudioClip clip;
            public float loopStartTime;   // → see docs/systems/sound-design.md
            public float loopEndTime;     // → see docs/systems/sound-design.md
                                          // 0이면 클립 전체 루프
        }

        [SerializeField]
        private BGMEntry[] entries;

        private Dictionary<BGMTrack, BGMEntry> _lookup;

        public void Initialize()
        {
            _lookup = new Dictionary<BGMTrack, BGMEntry>(entries.Length);
            foreach (var entry in entries)
            {
                _lookup.TryAdd(entry.track, entry);
            }
        }

        public AudioClip GetClip(BGMTrack track)
        {
            return _lookup.TryGetValue(track, out var entry) ? entry.clip : null;
        }

        public BGMEntry? GetEntry(BGMTrack track)
        {
            return _lookup.TryGetValue(track, out var entry)
                ? entry
                : (BGMEntry?)null;
        }
    }
}
```

---

## 6. BGMScheduler -- 계절/날씨/시간대 자동 전환

### 6.1 설계 의도

BGMScheduler는 TimeManager와 WeatherSystem의 이벤트를 구독하여, 현재 게임 상태에 맞는 BGM을 자동으로 결정하고 SoundManager에 crossfade 요청을 보낸다. SoundManager 자체는 "어떤 BGM을 재생할지" 판단하지 않고, BGMScheduler가 그 책임을 전담한다.

### 6.2 BGM 우선순위 체계

여러 조건이 동시에 충족될 때 어떤 BGM을 재생할지 결정하는 우선순위:

| 우선순위 | 조건 | BGMTrack 예시 | 설명 |
|----------|------|--------------|------|
| 1 (최고) | 특수 이벤트 | Festival, GameOver | 강제 재생, 다른 조건 무시 |
| 2 | 실내/상점 | IndoorHome, Shop | 특정 장소 진입 (-> see docs/systems/sound-design.md 섹션 1.4) |
| 3 | 날씨 | Storm, Rain | 악천후 시 분위기 전환 |
| 4 | 시간대 | NightTime | 밤 시간대 |
| 5 (최저) | 계절 기본 | Spring, Summer, Autumn, Winter | 기본 BGM |

> 각 조건별 적용 기준 (날씨 종류, 시간대 경계 등)은 (-> see docs/systems/sound-design.md).

### 6.3 클래스 설계

```csharp
namespace SeedMind.Audio
{
    /// <summary>
    /// 게임 상태를 모니터링하여 적절한 BGM을 자동으로 결정한다.
    /// TimeManager, WeatherSystem 이벤트를 구독한다.
    /// </summary>
    public class BGMScheduler
    {
        private SoundManager _soundManager;
        private BGMTrack _forcedTrack;       // 우선순위 1: 특수 이벤트
        private BGMTrack _weatherTrack;      // 우선순위 2: 날씨
        private BGMTrack _locationTrack;     // 우선순위 3: 장소
        private BGMTrack _timeTrack;         // 우선순위 4: 시간대
        private BGMTrack _seasonTrack;       // 우선순위 5: 계절

        public void Initialize(SoundManager soundManager)
        {
            _soundManager = soundManager;
            _forcedTrack = BGMTrack.None;
            _weatherTrack = BGMTrack.None;
            _locationTrack = BGMTrack.None;
            _timeTrack = BGMTrack.None;
            _seasonTrack = BGMTrack.None;
        }

        // --- 이벤트 핸들러 ---

        public void OnSeasonChanged(Season season)
        {
            _seasonTrack = season switch
            {
                Season.Spring => BGMTrack.Spring,
                Season.Summer => BGMTrack.Summer,
                Season.Autumn => BGMTrack.Autumn,
                Season.Winter => BGMTrack.Winter,
                _ => BGMTrack.Spring
            };
            // → Season enum은 docs/systems/time-season-architecture.md 섹션 1.1
            EvaluateAndApply();
        }

        public void OnWeatherChanged(WeatherType weather)
        {
            _weatherTrack = weather switch
            {
                WeatherType.Storm   => BGMTrack.Storm,
                WeatherType.Blizzard => BGMTrack.Storm,
                WeatherType.Rain     => BGMTrack.Rain,
                WeatherType.HeavyRain => BGMTrack.Rain,
                _ => BGMTrack.None    // Clear/Cloudy/Snow: 날씨 BGM 없음
            };
            // → WeatherType enum은 docs/systems/time-season-architecture.md 섹션 1.3
            EvaluateAndApply();
        }

        public void OnDayPhaseChanged(DayPhase phase)
        {
            _timeTrack = phase switch
            {
                DayPhase.Night => BGMTrack.NightTime,
                _ => BGMTrack.None
            };
            // → DayPhase enum은 docs/systems/time-season-architecture.md 섹션 1.2
            EvaluateAndApply();
        }

        /// <summary>
        /// 특수 이벤트용 강제 BGM 설정/해제.
        /// ForceTrack(BGMTrack.None)으로 해제.
        /// </summary>
        public void ForceTrack(BGMTrack track)
        {
            _forcedTrack = track;
            EvaluateAndApply();
        }

        public void SetLocationTrack(BGMTrack track)
        {
            _locationTrack = track;
            EvaluateAndApply();
        }

        // --- 핵심 결정 로직 ---

        private void EvaluateAndApply()
        {
            var resolved = ResolveTrack();
            if (resolved != _soundManager.CurrentBGM)
            {
                float fadeDuration = 0f;
                    // → see docs/systems/sound-design.md for default fade duration
                _soundManager.CrossfadeBGM(resolved, fadeDuration);
            }
        }

        private BGMTrack ResolveTrack()
        {
            // 우선순위 순서대로 평가 (→ see docs/systems/sound-design.md 섹션 1.4 BGM 우선순위 스택)
            // 1. 특수 이벤트 (Festival, GameOver)
            if (_forcedTrack   != BGMTrack.None) return _forcedTrack;
            // 2. 실내/상점 (실내 BGM이 날씨 BGM보다 우선)
            if (_locationTrack != BGMTrack.None) return _locationTrack;
            // 3. 날씨
            if (_weatherTrack  != BGMTrack.None) return _weatherTrack;
            // 4. 시간대 (밤)
            if (_timeTrack     != BGMTrack.None) return _timeTrack;
            // 5. 계절 기본
            if (_seasonTrack   != BGMTrack.None) return _seasonTrack;
            return BGMTrack.Spring; // 궁극적 기본값 (게임 시작 시 초기 계절이 설정되기 전 안전값)
        }
    }
}
```

---

## 7. BGM Crossfade 구현

### 7.1 Dual-Source Crossfade 패턴

SoundManager는 두 개의 BGM AudioSource(_bgmSourceA, _bgmSourceB)를 번갈아 사용하여 끊김 없는 crossfade를 구현한다.

```csharp
private IEnumerator CrossfadeCoroutine(AudioClip newClip, float duration)
{
    if (_isCrossfading) yield break;  // 중복 crossfade 방지
    _isCrossfading = true;

    var fadeOut = (_activeBGMSource == 0) ? _bgmSourceA : _bgmSourceB;
    var fadeIn  = (_activeBGMSource == 0) ? _bgmSourceB : _bgmSourceA;

    fadeIn.clip = newClip;
    fadeIn.volume = 0f;
    fadeIn.Play();

    float elapsed = 0f;
    float startVolume = fadeOut.volume;

    while (elapsed < duration)
    {
        elapsed += Time.unscaledDeltaTime;  // 타임스케일 영향 안받음
        float t = elapsed / duration;
        fadeOut.volume = Mathf.Lerp(startVolume, 0f, t);
        fadeIn.volume = Mathf.Lerp(0f, startVolume, t);
        yield return null;
    }

    fadeOut.Stop();
    fadeOut.volume = 0f;
    fadeIn.volume = startVolume;

    _activeBGMSource = (_activeBGMSource == 0) ? 1 : 0;
    _isCrossfading = false;
}
```

> `Time.unscaledDeltaTime` 사용 -- 게임 일시정지(TimeScale=0) 시에도 BGM 전환이 정상 작동해야 한다.

---

## 8. 이벤트 연동 설계

### 8.1 SoundEventBridge -- 이벤트 브릿지 패턴

다른 시스템의 이벤트를 사운드 시스템으로 변환하는 중간 계층. SoundManager가 FarmEvents 등을 직접 구독하지 않고, SoundEventBridge를 경유한다. 이는 사운드 시스템과 게임 로직 시스템 간의 결합도를 낮춘다.

```csharp
namespace SeedMind.Audio
{
    /// <summary>
    /// 게임 이벤트 → 사운드 이벤트 변환 브릿지.
    /// 각 게임 시스템의 이벤트를 구독하고, SoundManager에 SFX/BGM 요청을 전달한다.
    /// </summary>
    public class SoundEventBridge : MonoBehaviour
    {
        // 사운드 시스템 내부 이벤트
        public static Action<SoundEvent> OnSFXRequested;
        public static Action<BGMTrack, float> OnBGMRequested;

        private void OnEnable()
        {
            // 경작 이벤트 (→ see docs/systems/farming-architecture.md 섹션 6.1)
            FarmEvents.OnTileTilled     += HandleTileTilled;
            FarmEvents.OnTileWatered    += HandleTileWatered;
            FarmEvents.OnCropPlanted    += HandleCropPlanted;
            FarmEvents.OnCropHarvested  += HandleCropHarvested;
            FarmEvents.OnCropWithered   += HandleCropWithered;

            // 시간 이벤트 (→ see docs/systems/time-season-architecture.md)
            // BGMScheduler가 직접 구독: OnSeasonChanged, OnDayPhaseChanged
            // SoundEventBridge는 SFX 트리거만 담당:
            TimeManager.Instance.OnDayChanged += HandleDayStarted;

            // 경제 이벤트 (→ see docs/systems/economy-architecture.md)
            EconomyEvents.OnShopPurchased += HandleShopPurchased;
            EconomyEvents.OnSaleCompleted += HandleSaleCompleted;

            // 진행 이벤트 (→ see docs/systems/progression-architecture.md)
            ProgressionManager.Instance.OnLevelUp += HandleLevelUp;

            // 퀘스트 이벤트 (→ see docs/systems/quest-architecture.md)
            QuestEvents.OnQuestCompleted += HandleQuestCompleted;

            // 업적 이벤트 (→ see docs/systems/achievement-architecture.md)
            AchievementEvents.OnAchievementUnlocked += HandleAchievementUnlocked;

            // 낚시 이벤트 (→ see docs/systems/fishing-architecture.md)
            FishingEvents.OnFishCast   += HandleFishCast;
            FishingEvents.OnFishBite   += HandleFishBite;
            FishingEvents.OnFishCaught += HandleFishCaught;
        }

        private void OnDisable()
        {
            FarmEvents.OnTileTilled     -= HandleTileTilled;
            FarmEvents.OnTileWatered    -= HandleTileWatered;
            FarmEvents.OnCropPlanted    -= HandleCropPlanted;
            FarmEvents.OnCropHarvested  -= HandleCropHarvested;
            FarmEvents.OnCropWithered   -= HandleCropWithered;
            TimeManager.Instance.OnDayChanged -= HandleDayStarted;
            EconomyEvents.OnShopPurchased -= HandleShopPurchased;
            EconomyEvents.OnSaleCompleted -= HandleSaleCompleted;
            ProgressionManager.Instance.OnLevelUp -= HandleLevelUp;
            QuestEvents.OnQuestCompleted -= HandleQuestCompleted;
            AchievementEvents.OnAchievementUnlocked -= HandleAchievementUnlocked;
            FishingEvents.OnFishCast   -= HandleFishCast;
            FishingEvents.OnFishBite   -= HandleFishBite;
            FishingEvents.OnFishCaught -= HandleFishCaught;
        }
    }
}
```

### 8.2 이벤트 → 사운드 매핑 테이블

#### 8.2.1 SFX 매핑

| 트리거 이벤트 | SFXId | 3D/2D | 비고 |
|--------------|-------|-------|------|
| `FarmEvents.OnTileTilled` | `HoeTill` | 3D | 타일 위치 사용 |
| `FarmEvents.OnTileWatered` | `WateringCan` | 3D | 타일 위치 사용 |
| `FarmEvents.OnCropPlanted` | `SeedPlant` | 3D | 타일 위치 사용 |
| `FarmEvents.OnCropHarvested` | `HarvestNormal` / `HarvestGold` / `HarvestIridium` | 3D | 품질별 variation (-> see docs/systems/sound-design.md) |
| `FarmEvents.OnCropWithered` | `CropWither` | 3D | 타일 위치 사용 |
| `TimeManager.OnDayChanged` | `MorningChime` | 2D | 하루 시작 알림 |
| `EconomyEvents.OnShopPurchased` | `Purchase` | 2D | 상점 구매 |
| `EconomyEvents.OnSaleCompleted` | `Sale` | 2D | 상점 판매 |
| `ProgressionManager.OnLevelUp` | `LevelUp` | 2D | 레벨업 팡파레 |
| `QuestEvents.OnQuestCompleted` | `QuestComplete` | 2D | 퀘스트 완료 |
| `AchievementEvents.OnAchievementUnlocked` | `AchievementToast` | 2D | 업적 달성 |
| `FishingEvents.OnFishCast` | `FishCastLine` | 3D | 플레이어 위치 |
| `FishingEvents.OnFishBite` | `FishBite` | 3D | 낚시 위치 |
| `FishingEvents.OnFishCaught` | `FishCatchNormal` 또는 `FishCatchRare` | 3D | 희귀도에 따라 분기 (→ see sound-design.md 섹션 2.7) |

#### 8.2.2 BGM 매핑 (BGMScheduler 담당)

| 트리거 이벤트 | BGM 결정 로직 | 우선순위 |
|--------------|-------------|----------|
| `TimeManager.OnSeasonChanged` | Season -> 계절별 BGMTrack | 5 |
| `TimeManager.OnDayPhaseChanged` | Night -> NightTime, 그 외 None | 4 |
| `WeatherSystem.OnWeatherChanged` | Storm/Blizzard -> Storm, Rain/HeavyRain -> Rain | 2 |
| 장소 진입 (수동 호출) | Shop 등 | 3 |
| 특수 이벤트 (수동 호출) | Festival, GameOver 등 | 1 |

### 8.3 설계 원칙

- **단방향 데이터 흐름**: 게임 이벤트 -> SoundEventBridge -> SoundManager. 사운드 시스템이 게임 상태를 변경하지 않는다.
- **Fire and forget**: FarmEvents 발행자는 사운드 시스템의 존재를 모른다.
- **브릿지 분리**: SoundManager는 FarmEvents/EconomyEvents 등을 직접 참조하지 않는다. SoundEventBridge만 양쪽을 안다.
- **구독 해제 안전**: OnDisable에서 모든 구독을 해제하여 메모리 누수를 방지한다.

---

## 9. 오디오 설정 저장/로드

### 9.1 AudioSettingsData

```csharp
namespace SeedMind.Audio.Data
{
    /// <summary>
    /// 유저 볼륨 설정을 직렬화하는 데이터 클래스.
    /// PlayerPrefs에 JSON으로 저장한다.
    /// </summary>
    [System.Serializable]
    public class AudioSettingsData
    {
        public float masterVolume;    // 0.0 ~ 1.0
        public float bgmVolume;       // 0.0 ~ 1.0
        public float sfxVolume;       // 0.0 ~ 1.0
        public float ambientVolume;   // 0.0 ~ 1.0
        public float uiVolume;        // 0.0 ~ 1.0
        public bool masterMuted;
        public bool bgmMuted;
        public bool sfxMuted;
        public bool ambientMuted;
        public bool uiMuted;
    }
}
```

> 볼륨 기본값은 (-> see docs/systems/sound-design.md).

### 9.2 저장 위치

`PlayerPrefs`를 사용한다. 게임 세이브 데이터와 분리 -- 오디오 설정은 세이브 슬롯에 독립적이며 전역으로 적용된다.

```csharp
private const string AUDIO_SETTINGS_KEY = "SeedMind_AudioSettings";

public void SaveAudioSettings()
{
    var data = new AudioSettingsData
    {
        masterVolume  = GetVolume(AudioChannel.Master),
        bgmVolume     = GetVolume(AudioChannel.BGM),
        sfxVolume     = GetVolume(AudioChannel.SFX),
        ambientVolume = GetVolume(AudioChannel.Ambient),
        uiVolume      = GetVolume(AudioChannel.UI),
        // muted 상태도 저장
    };
    PlayerPrefs.SetString(AUDIO_SETTINGS_KEY, JsonUtility.ToJson(data));
    PlayerPrefs.Save();
}
```

---

# Part II -- MCP 구현 태스크

---

## 10. MCP 태스크 시퀀스

### Step 1: AudioMixer 에셋 생성 및 채널 구조 설정

```
1-1: Unity에서 AudioMixer "MainMixer" 에셋 생성
     → Assets/_Project/Audio/MainMixer.mixer
1-2: Master Group 하위에 4개 자식 Group 생성:
     BGM, SFX, Ambient, UI
1-3: 각 Group에 Exposed Parameter 등록:
     MasterVolume, BGMVolume, SFXVolume, AmbientVolume, UIVolume
```

### Step 2: SoundManager GameObject 구성

```
2-1: 빈 GameObject "SoundManager" 생성 (씬 루트)
2-2: SoundManager.cs 컴포넌트 추가
2-3: 자식 AudioSource 구성:
     ├── "BGM_Source_A" → AudioSource (loop=true, playOnAwake=false, output=BGM Group)
     ├── "BGM_Source_B" → AudioSource (loop=true, playOnAwake=false, output=BGM Group)
     ├── "Ambient_Source" → AudioSource (loop=true, playOnAwake=false, output=Ambient Group)
     └── "UI_Source" → AudioSource (loop=false, playOnAwake=false, spatialBlend=0, output=UI Group)
2-4: SFX 풀 초기화 — N개 자식 AudioSource 생성 (→ see docs/systems/sound-design.md for pool size)
     └── "SFX_Source_0" ~ "SFX_Source_N" → output=SFX Group
2-5: _mixer 필드에 MainMixer 참조 할당
```

### Step 3: ScriptableObject 에셋 생성

```
3-1: SoundRegistry SO 에셋 생성
     → Assets/_Project/Data/Audio/SoundRegistry.asset
3-2: BGMRegistry SO 에셋 생성
     → Assets/_Project/Data/Audio/BGMRegistry.asset
3-3: SFXId enum의 각 값에 대응하는 SoundData SO 에셋 생성
     → Assets/_Project/Data/Audio/SFX/SD_HoeTillBasic.asset, SD_WaterBasic.asset, ...
     → 각 SO의 id(SFXId enum 값), clips[], baseVolume, pitchVariation, is3D, maxDistance 설정 (→ see docs/systems/sound-design.md)
3-4: BGMTrack enum의 각 값에 대응하는 BGMEntry를 BGMRegistry에 등록
     → AudioClip 참조는 에셋 임포트 후 연결
```

### Step 4: SoundEventBridge 연결 및 이벤트 구독 검증

```
4-1: SoundEventBridge.cs 컴포넌트를 SoundManager GameObject에 추가
4-2: Play Mode 진입
4-3: 검증 시나리오 실행:
     → 농장 타일 경작 → 콘솔에 "SFX: HoeTill at (x,y)" 로그 확인
     → 계절 변경 → 콘솔에 "BGM: CrossfadeTo Spring/Summer/..." 로그 확인
     → 상점 구매 → 콘솔에 "SFX: Purchase (2D)" 로그 확인
4-4: 볼륨 설정 변경 → PlayerPrefs 저장/로드 확인
```

### Step 5: BGMScheduler 통합 테스트

```
5-1: BGMScheduler 초기화 검증 — 게임 시작 시 현재 계절 BGM 자동 재생
5-2: 계절 전환 crossfade 확인 — 콘솔에 "BGMScheduler: Season=Summer, Resolved=Summer" 로그
5-3: 날씨 우선순위 검증 — Storm 시 계절 BGM 대신 Storm BGM 재생 확인
5-4: 날씨 종료 후 계절 BGM 복귀 확인
```

---

## Cross-references

| 참조 문서 | 관계 |
|----------|------|
| `docs/systems/sound-design.md` | **canonical** -- 모든 볼륨/거리/페이드 수치, BGM/SFX 목록의 단일 출처 |
| `docs/systems/farming-architecture.md` 섹션 6.1 | FarmEvents 정적 이벤트 허브 정의 |
| `docs/systems/economy-architecture.md` | EconomyEvents 정의 |
| `docs/systems/progression-architecture.md` | ProgressionManager.OnLevelUp 이벤트 |
| `docs/systems/time-season-architecture.md` | TimeManager 이벤트, Season/DayPhase/WeatherType enum |
| `docs/systems/quest-architecture.md` | QuestEvents 정의 |
| `docs/systems/achievement-architecture.md` | AchievementEvents 정의 |
| `docs/systems/fishing-architecture.md` | FishingEvents 정의 |
| `docs/pipeline/data-pipeline.md` | SO 패턴, GameDataSO 기반 클래스 |
| `docs/systems/project-structure.md` | 네임스페이스/폴더 구조 |

---

## Open Questions

1. **[OPEN]** Ambient 사운드(환경음)의 레이어링 필요 여부 -- 새소리 + 바람소리를 동시에 재생해야 하는 경우 Ambient AudioSource를 복수로 확장해야 할 수 있다. 현재는 단일 Ambient 소스로 설계했으나, sound-design.md에서 환경음 레이어가 정의되면 AmbientPool로 확장 검토.

2. **[OPEN]** 실내/실외 전환 시 Ambient 처리 방식 -- 건물 진입 시 환경음을 페이드아웃하고 실내 환경음으로 전환할지, 저역 필터(Low-pass)만 적용할지. AudioMixer Snapshot 활용 가능성 검토 필요.

3. **[OPEN]** BGM 루프 포인트 구현 방식 -- Unity AudioSource의 기본 loop은 클립 전체를 반복한다. intro + loop body 구조를 위해 `AudioSource.timeSamples` 기반 수동 루프를 구현해야 할 수 있다. BGMRegistry의 `loopStartTime`/`loopEndTime` 필드가 이를 대비하지만, 구현 복잡도 평가 필요.

---

## Risks

1. **[RISK]** **static event 구독 누수**: SoundEventBridge가 FarmEvents 등의 static Action에 구독한다. 씬 전환 시 OnDisable이 호출되지 않으면 구독이 남아 NullReference가 발생할 수 있다. DontDestroyOnLoad 적용으로 완화되지만, 씬 재로드 시나리오에서 검증 필요. (FarmEvents와 동일 위험 -> see docs/systems/farming-architecture.md 섹션 6.3)

2. **[RISK]** **SFX 풀 고갈 시 중요 SFX 누락**: oldest-replace 정책은 대부분 적절하지만, 레벨업 SFX가 다수의 수확 SFX에 의해 교체될 수 있다. 우선순위 기반 교체(priority-based eviction)를 향후 고려할 수 있으나, 초기 구현에서는 풀 크기를 충분히 확보하여 대응한다.

3. **[RISK]** **AudioClip 메모리 사용량**: 모든 BGM 클립을 BGMRegistry에 참조로 유지하면 메모리에 상주한다. BGM 클립 크기가 커질 경우 Addressables 기반 비동기 로딩으로 전환해야 할 수 있다. 초기에는 직접 참조로 시작하되, 메모리 프로파일링 후 결정.

4. **[RISK]** **Time.unscaledDeltaTime 의존**: crossfade 코루틴이 unscaledDeltaTime을 사용하므로 프레임 레이트 변동에 따라 페이드 곡선이 미세하게 달라질 수 있다. 실질적 영향은 미미하나, 매우 낮은 FPS에서는 페이드가 거칠게 느껴질 수 있다.

5. **[RISK]** **DontDestroyOnLoad 중복 인스턴스**: 메인 메뉴 -> 게임 씬 전환 시 SoundManager가 중복 생성될 수 있다. Awake에서 중복 검사(Instance != null)로 방어하지만, 전환 순간 짧은 시간 동안 두 인스턴스가 공존할 수 있다.
