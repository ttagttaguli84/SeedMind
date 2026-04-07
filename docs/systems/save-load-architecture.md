# 세이브/로드 시스템 기술 아키텍처

> SaveManager 싱글턴, GameSaveData 통합 루트 구조, 자동저장 트리거, 비동기 저장/로드 API, 이벤트 시스템, 오류 처리/복구, MCP 구현 태스크 요약  
> 작성: Claude Code (Opus) | 2026-04-06  
> 문서 ID: ARC-011

---

## Context

이 문서는 SeedMind의 세이브/로드 시스템에 대한 **전용 기술 아키텍처 문서**이다. 기존 `docs/pipeline/data-pipeline.md`에 세이브 데이터 구조와 SaveManager의 기본 설계가 산재되어 있으나, 세이브/로드 시스템을 독립적인 아키텍처 단위로 상세화할 필요가 생겼다. 이 문서는 다음을 목표로 한다:

1. 기존 문서들에 흩어진 SaveData 구조를 **통합 루트 클래스(GameSaveData)**로 정리
2. SaveManager의 비동기 API 설계를 상세화
3. 자동저장 트리거 시스템의 컴포넌트 구조를 정의
4. 저장 실패, 파일 손상, 버전 불일치에 대한 오류 처리 전략을 설계
5. Unity MCP를 통한 구현 태스크 요약을 제공

**기존 data-pipeline.md와의 관계**: data-pipeline.md는 데이터 분류/필드 정의/직렬화 전략 등 데이터 파이프라인 전반을 다루며, 이 문서는 그 중 세이브/로드의 **런타임 아키텍처**만을 깊이 있게 확장한다. 개별 SaveData 필드 정의의 canonical 출처는 data-pipeline.md이다.

---

## Part I -- 시스템 아키텍처

---

### 1. 개요

#### 1.1 SaveManager의 역할과 책임

SaveManager는 `DontDestroyOnLoad` 싱글턴으로, 게임 상태의 직렬화/역직렬화 및 파일 I/O를 중앙에서 관리한다.

| 책임 | 설명 |
|------|------|
| 직렬화/역직렬화 | GameSaveData <-> JSON 변환 |
| 파일 I/O | 비동기 읽기/쓰기, 원자적 쓰기 보장 |
| 슬롯 관리 | 멀티슬롯(3개) 메타 정보 관리 |
| ISaveable 레지스트리 | 저장 대상 시스템 등록/해제, 순서 정렬 |
| 자동저장 조율 | AutoSaveTrigger로부터의 요청 처리, 쿨다운 관리 |
| 마이그레이션 | 세이브 버전 불일치 시 단계적 변환 |
| 오류 처리 | 저장 실패 시 백업, 손상 감지, 폴백 |

#### 1.2 저장 경로

```
{Application.persistentDataPath}/Saves/
├── save_0.json         # 슬롯 0
├── save_1.json         # 슬롯 1
├── save_2.json         # 슬롯 2
├── save_0.json.bak     # 슬롯 0 백업 (이전 성공 저장)
├── save_1.json.bak     # 슬롯 1 백업
├── save_2.json.bak     # 슬롯 2 백업
└── save_meta.json      # 슬롯 메타 정보
```

Windows 경로 예시: `C:\Users\{username}\AppData\LocalLow\{CompanyName}\SeedMind\Saves\`

#### 1.3 직렬화 방식 선택

| 방식 | 장점 | 단점 | 채택 |
|------|------|------|:----:|
| Unity JsonUtility | 별도 의존성 없음, 빠름 | Dictionary 미지원, 다형성 미지원, null 처리 제한적 | X |
| **Newtonsoft.Json** | Dictionary/다형성/null 완전 지원, 풍부한 옵션 | 외부 패키지 의존성 | **O** |
| BinaryFormatter | 빠른 직렬화 | 보안 취약, .NET 6+ deprecated, 사람이 읽을 수 없음 | X |

**채택 근거**: Unity 6에 `com.unity.nuget.newtonsoft-json` 패키지가 기본 제공되므로 외부 의존성 부담이 없다. Dictionary 직렬화(PriceFluctuationSaveData), null 처리(구버전 세이브 호환), 인덴트 포맷(디버깅 편의) 등의 이점이 크다. BinaryFormatter는 보안 취약점과 deprecation으로 인해 배제한다.

(-> see `docs/pipeline/data-pipeline.md` Part II 섹션 3.2 for 직렬화 전략 canonical 정의)

#### 1.4 멀티슬롯 구조

| 파라미터 | 값 |
|----------|-----|
| 최대 세이브 슬롯 | 3 |
| 파일명 패턴 | `save_{N}.json` (N = 0, 1, 2) |
| 메타 파일 | `save_meta.json` |
| 백업 파일 패턴 | `save_{N}.json.bak` |

(-> see `docs/pipeline/data-pipeline.md` Part I 섹션 3.1 for 세이브 슬롯 구조 canonical 정의)

---

### 2. GameSaveData 통합 구조

#### 2.1 통합 루트 클래스 계층

기존 문서들에 정의된 세이브 데이터를 하나의 통합 루트 구조로 조합한다.

```
GameSaveData (루트)
├── SaveMetadata
│   ├── saveVersion: string           # 세이브 포맷 버전 (예: "1.0.0")
│   ├── savedAt: string               # 저장 시각 (ISO 8601)
│   └── playTimeSeconds: int          # 총 플레이 시간 (초)
│
├── PlayerSaveData                    # (→ see data-pipeline.md Part II 섹션 2.2)
│   ├── posX, posY, posZ, rotY
│   ├── currentEnergy, maxEnergy
│   ├── inventorySlots[]              # InventorySlotSaveData[]
│   ├── equippedToolIndex
│   ├── level, currentExp
│   └── toolUpgradeState              # ToolUpgradeSaveData (→ see tool-upgrade-architecture.md 섹션 7.2)
│
├── FarmSaveData                      # (→ see data-pipeline.md Part II 섹션 2.3)
│   ├── gridWidth, gridHeight
│   └── tiles[]                       # FarmTileSaveData[]
│       └── crop                      # CropInstanceSaveData (nullable)
│
├── ZoneSaveData                      # (→ see farm-expansion-architecture.md 섹션 9, ARC-023)
│   └── zones[]                       # ZoneEntrySaveData[] (zoneId, isUnlocked, obstacles[])
│
├── InventorySaveData                 # (→ see inventory-architecture.md 섹션 6.1)
│   ├── slots[]                       # 배낭 슬롯 배열
│   ├── maxSlots
│   ├── toolbarSlots[]                # 8슬롯 범용 툴바
│   ├── toolbarSelectedIndex
│   └── wateringCanCharges
│
├── TimeSaveData                      # (→ see time-season-architecture.md 섹션 7.1)
│   ├── currentYear, currentSeason
│   ├── currentDay, currentHour
│
├── WeatherSaveData                   # (→ see time-season-architecture.md 섹션 7.2)
│   ├── currentWeather, tomorrowWeather
│   ├── weatherSeed
│   ├── consecutiveSameWeatherDays
│   └── consecutiveExtremeWeatherDays
│
├── EconomySaveData                   # (→ see economy-architecture.md 섹션 4.6)
│   ├── currentGold
│   ├── transactionLog                # TransactionLogSaveData
│   └── priceFluctuation              # PriceFluctuationSaveData
│
├── BuildingSaveData[]                # (→ see data-pipeline.md Part II 섹션 2.6)
│   └── storageSlots[]                # ItemSlotSaveData[] (창고에만)
│                                     # ItemSlotSaveData: itemId, itemType, qty, quality, origin [FIX-034]
│                                     # (→ see economy-architecture.md 섹션 3.10 for HarvestOrigin)
│
├── ProcessingSaveData[]              # (→ see data-pipeline.md Part II 섹션 2.6)
│
├── UnlockSaveData                    # (→ see data-pipeline.md Part II 섹션 2.6)
│
├── ShopStockSaveData[]               # (→ see data-pipeline.md Part II 섹션 2.6)
│
├── MilestoneSaveData                 # (→ see progression-architecture.md 섹션 4.4)
│   ├── completedMilestoneIds[]
│   ├── progressEntries[]
│   └── totalExpEarned
│
├── NPCSaveData                       # (→ see npc-shop-architecture.md 섹션 7.1)
│   └── travelingMerchant             # TravelingMerchantSaveData
│
├── TutorialSaveData                  # (→ see tutorial-architecture.md 섹션 7)
│   ├── completedSequenceIds[]
│   ├── completedStepIds[]
│   ├── activeSequenceId
│   ├── activeStepIndex
│   └── contextHintCooldowns          # Dictionary<string, int>
│
├── AchievementSaveData               # (→ see achievement-architecture.md 섹션 7)
│
├── AnimalSaveData                    # (→ see livestock-architecture.md 섹션 8, ARC-019)
│   ├── isUnlocked: bool
│   ├── barnLevel: int
│   └── animals[]                     # AnimalInstanceSaveData[]
│
├── AffinitySaveData                  # (→ see blacksmith-architecture.md 섹션 5.5)
│   └── entries[]                     # AffinityEntry[] (npcId, affinityValue, lastVisitDay, triggeredDialogueIds[])
│
└── FishCatalogSaveData               # (→ see fishing-architecture.md 섹션 20, ARC-030)
    ├── entries[]                      # FishCatalogEntry[] (fishId, isDiscovered, maxSizeCm 등)
    └── discoveredCount                # int
```

#### 2.2 JSON 스키마 (PATTERN-005 준수)

```json
{
  "saveVersion": "1.0.0",
  "savedAt": "2026-04-06T15:30:00Z",
  "playTimeSeconds": 3600,

  "player": {
    "posX": 4.0, "posY": 0.0, "posZ": 3.0, "rotY": 0.0,
    "currentEnergy": 85, "maxEnergy": 100,
    "inventorySlots": [],
    "equippedToolIndex": 0,
    "level": 3, "currentExp": 120,
    "toolUpgradeState": null
  },

  "farm": {
    "gridWidth": 8, "gridHeight": 8,
    "tiles": []
  },

  "farmZones": {
    "zones": []
  },

  "inventory": {
    "slots": [],
    "maxSlots": 15,
    "toolbarSlots": [],
    "toolbarSelectedIndex": 0,
    "wateringCanCharges": 20
  },

  "time": {
    "currentYear": 1, "currentSeason": 0,
    "currentDay": 15, "currentHour": 14.5
  },

  "weather": {
    "currentWeather": "Rain",
    "tomorrowWeather": "Clear",
    "weatherSeed": 42,
    "consecutiveSameWeatherDays": 1,
    "consecutiveExtremeWeatherDays": 0
  },

  "economy": {
    "currentGold": 1250,
    "transactionLog": { "entries": [], "totalEarned": 0, "totalSpent": 0 },
    "priceFluctuation": { "seasonSalesEntries": [] }
  },

  "buildings": [],
  "processing": [],

  "unlocks": {
    "unlockedCrops": [], "unlockedBuildings": [], "unlockedRecipes": [],
    "unlockedFertilizers": null, "unlockedTools": null, "unlockedFarmExpansions": null
  },

  "shops": [],

  "milestones": {
    "completedMilestoneIds": [],
    "progressEntries": [],
    "totalExpEarned": 0
  },

  "npc": {
    "travelingMerchant": {
      "isPresent": false,
      "nextVisitDay": 7,
      "currentStockSeed": 0,
      "stockItems": []
    }
  },

  "tutorial": {
    "completedSequenceIds": [],
    "completedStepIds": [],
    "activeSequenceId": "",
    "activeStepIndex": -1,
    "contextHintCooldowns": {}
  },

  "achievements": {
    "records": [],
    "totalUnlocked": 0
  },

  "animals": {
    "isUnlocked": false,
    "barnLevel": 0,
    "animals": []
  },

  "affinity": {
    "entries": []
  },

  "fishCatalog": {
    "entries": [],
    "discoveredCount": 0
  }
}
```

#### 2.3 C# 통합 루트 클래스 (PATTERN-005 준수)

```csharp
// illustrative
namespace SeedMind.Save.Data
{
    using SeedMind.Player;
    using SeedMind.Farm;
    using SeedMind.Economy;
    using SeedMind.Building;
    using SeedMind.Core;
    using SeedMind.Level;
    using SeedMind.NPC.Data;
    using SeedMind.Tutorial;
    using SeedMind.Achievement;
    using SeedMind.Fishing.Catalog;

    /// <summary>
    /// 하나의 세이브 슬롯에 저장되는 전체 게임 상태.
    /// 개별 SaveData 클래스의 canonical 정의는 각 시스템 문서를 참조.
    /// </summary>
    [System.Serializable]
    public class GameSaveData
    {
        // --- 메타데이터 ---
        public string saveVersion;              // 세이브 포맷 버전 (예: "1.0.0")
        public string savedAt;                  // 저장 시각 (ISO 8601)
        public int playTimeSeconds;             // 총 플레이 시간 (초)

        // --- 시스템별 세이브 데이터 ---
        public PlayerSaveData player;                    // → see data-pipeline.md Part II 섹션 2.2
        public FarmSaveData farm;                        // → see data-pipeline.md Part II 섹션 2.3
        public ZoneSaveData farmZones;                   // → see farm-expansion-architecture.md 섹션 9 (ARC-023, null 허용)
        public InventorySaveData inventory;              // → see inventory-architecture.md 섹션 6.1
        public TimeSaveData time;                        // → see time-season-architecture.md 섹션 7.1
        public WeatherSaveData weather;                  // → see time-season-architecture.md 섹션 7.2
        public EconomySaveData economy;                  // → see economy-architecture.md 섹션 4.6
        public BuildingSaveData[] buildings;              // → see data-pipeline.md Part II 섹션 2.6
        public ProcessingSaveData[] processing;          // → see data-pipeline.md Part II 섹션 2.6
        public UnlockSaveData unlocks;                   // → see data-pipeline.md Part II 섹션 2.6
        public ShopStockSaveData[] shops;                // → see data-pipeline.md Part II 섹션 2.6
        public MilestoneSaveData milestones;             // → see progression-architecture.md 섹션 4.4 (null 허용)
        public NPCSaveData npc;                          // → see npc-shop-architecture.md 섹션 7.1 (null 허용)
        public TutorialSaveData tutorial;                // → see tutorial-architecture.md 섹션 7 (null 허용)
        public AchievementSaveData achievements;         // → see achievement-architecture.md 섹션 7 (null 허용)
        public AnimalSaveData animals;                   // → see livestock-architecture.md 섹션 8 (ARC-019, null 허용)
        public AffinitySaveData affinity;                // → see blacksmith-architecture.md 섹션 5.5 (null 허용)
        public FishCatalogSaveData fishCatalog;           // → see fishing-architecture.md 섹션 20 (ARC-030, null 허용)
    }
}
```

**PATTERN-005 검증**: JSON 스키마(섹션 2.2)와 C# 클래스(섹션 2.3)의 필드 수 동일:
- 메타데이터 3개: saveVersion, savedAt, playTimeSeconds
- 시스템 데이터 18개: player, farm, farmZones, inventory, time, weather, economy, buildings, processing, unlocks, shops, milestones, npc, tutorial, achievements, animals, affinity, fishCatalog
- 총 21개 필드 -- 양쪽 일치 (ARC-030 fishCatalog 필드 추가)

**기존 data-pipeline.md와의 차이점**: data-pipeline.md의 GameSaveData에는 `inventory`, `npc`, `tutorial` 필드가 명시적으로 분리되지 않았다. `inventory`는 PlayerSaveData 내부에 포함되어 있었고, `npc`와 `tutorial`은 각 아키텍처 문서에서 개별적으로 확장을 기술했다. 이 문서에서는 향후 구현 시 모든 세이브 데이터가 루트에서 명확히 접근 가능하도록 통합한다.

[OPEN] `inventory`를 PlayerSaveData 내부에 유지할지, 루트 레벨로 분리할지. data-pipeline.md에서는 PlayerSaveData.inventorySlots로 관리하고 inventory-architecture.md에서는 별도 InventorySaveData로 관리한다. 구현 시 한쪽으로 통일 필요. 현재 이 문서에서는 양쪽 모두 유지하되, 실제 구현 시에는 InventorySaveData를 루트 레벨에 배치하고 PlayerSaveData에서는 제거하는 방향을 권장한다.

---

### 3. 자동저장 트리거 시스템

#### 3.1 AutoSaveTrigger 컴포넌트

```csharp
// illustrative
namespace SeedMind.Save
{
    /// <summary>
    /// 게임 이벤트를 감지하여 자동저장을 요청하는 컴포넌트.
    /// SaveManager와 분리하여 트리거 로직을 독립 관리한다.
    /// </summary>
    public class AutoSaveTrigger : MonoBehaviour
    {
        // --- 설정 ---
        [SerializeField] private float saveCooldownSeconds = 60f;  // → see 섹션 3.2

        // --- 상태 ---
        private float _lastSaveTime;
        private bool _pendingSave;

        // --- 이벤트 구독 ---
        private void OnEnable()
        {
            TimeManager.OnDayChanged += OnDayChanged;
            TimeManager.OnSeasonChanged += OnSeasonChanged;
            // BuildingEvents.OnConstructionCompleted += OnFacilityBuilt;  // → see facilities-architecture.md
        }

        private void OnDisable()
        {
            TimeManager.OnDayChanged -= OnDayChanged;
            TimeManager.OnSeasonChanged -= OnSeasonChanged;
            // BuildingEvents.OnConstructionCompleted -= OnFacilityBuilt;
        }

        // --- 트리거 핸들러 ---
        private void OnDayChanged(int newDay)
        {
            RequestAutoSave("DayChanged");
        }

        private void OnSeasonChanged(/* Season newSeason */)
        {
            RequestAutoSave("SeasonChanged");
        }

        private void OnFacilityBuilt(/* string buildingId */)
        {
            RequestAutoSave("FacilityBuilt");
        }

        // --- 쿨다운 제어 ---
        private void RequestAutoSave(string reason)
        {
            if (Time.realtimeSinceStartup - _lastSaveTime < saveCooldownSeconds)
            {
                _pendingSave = true;  // 쿨다운 중이면 대기
                return;
            }

            ExecuteAutoSave(reason);
        }

        private void ExecuteAutoSave(string reason)
        {
            _lastSaveTime = Time.realtimeSinceStartup;
            _pendingSave = false;
            SaveEvents.RaiseAutoSaveTriggered(reason);
            SaveManager.Instance.AutoSaveAsync();
        }

        private void Update()
        {
            // 대기 중인 세이브가 있고 쿨다운이 끝났으면 실행
            if (_pendingSave && Time.realtimeSinceStartup - _lastSaveTime >= saveCooldownSeconds)
            {
                ExecuteAutoSave("Deferred");
            }
        }
    }
}
```

#### 3.2 트리거 이벤트 목록

| 트리거 | 이벤트 소스 | 우선도 | 설명 |
|--------|-----------|:------:|------|
| 하루 종료 | `TimeManager.OnDayChanged` | 높음 | 매일 06:00에 새 날 시작 시 자동저장 |
| 계절 전환 | `TimeManager.OnSeasonChanged` | 높음 | 계절 변경은 중대한 게임 상태 변경 시점 |
| 시설 건설 완료 | `BuildingEvents.OnConstructionCompleted` | 중간 | 장시간 투자 결과 보호 |
| 수동 저장 | 플레이어 UI 조작 | 즉시 | 일시정지 메뉴 "저장" 버튼 (쿨다운 무시) |

(-> see `docs/pipeline/data-pipeline.md` Part II 섹션 3.7 for 자동 저장 트리거 canonical 정의)

#### 3.3 쿨다운 로직

| 파라미터 | 값 | 설명 |
|----------|-----|------|
| saveCooldownSeconds | 60 | 자동저장 간 최소 간격 (실시간 초) |
| 쿨다운 예외 | 수동 저장 | 플레이어 명시 요청은 쿨다운 무시 |
| 대기 처리 | _pendingSave 플래그 | 쿨다운 중 트리거 발생 시 완료 후 1회 실행 |

쿨다운 로직은 짧은 시간 내 복수 트리거(예: 계절 전환과 하루 종료가 동시 발생)가 중복 저장을 유발하는 것을 방지한다.

---

### 4. SaveManager API

#### 4.1 비동기 저장/로드 API

```csharp
// illustrative
namespace SeedMind.Save
{
    using SeedMind.Save.Data;
    using Newtonsoft.Json;
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// 게임 상태의 저장/로드를 중앙 관리.
    /// DontDestroyOnLoad Singleton.
    /// </summary>
    public class SaveManager : Singleton<SaveManager>
    {
        // --- 상수 ---
        private const int MAX_SLOTS = 3;                     // → see data-pipeline.md Part I 섹션 3.1
        private const string SAVE_DIR = "Saves";
        private const string FILE_PREFIX = "save_";
        private const string FILE_EXT = ".json";
        private const string BACKUP_EXT = ".json.bak";
        private const string META_FILE = "save_meta.json";
        public const string CURRENT_VERSION = "1.0.0";

        // --- 직렬화 설정 ---
        private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,                // → see data-pipeline.md Part II 섹션 3.2
            NullValueHandling = NullValueHandling.Include,
            DefaultValueHandling = DefaultValueHandling.Include,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        // --- 상태 ---
        private int _activeSlot = -1;           // 현재 활성 슬롯 (-1 = 없음)
        private bool _isSaving;
        private bool _isLoading;
        private float _playTimeAccumulator;     // 세션 내 누적 플레이 시간

        // --- ISaveable 레지스트리 ---
        private readonly List<ISaveable> _saveables = new List<ISaveable>();

        // === 공개 API ===

        /// <summary>비동기 저장. slotIndex: 0~2.</summary>
        public async Task<bool> SaveAsync(int slotIndex)
        {
            // → see 섹션 4.2 저장 흐름
            return false; // placeholder
        }

        /// <summary>비동기 로드. slotIndex: 0~2.</summary>
        public async Task<bool> LoadAsync(int slotIndex)
        {
            // → see 섹션 4.3 로드 흐름
            return false; // placeholder
        }

        /// <summary>자동저장. 현재 _activeSlot에 저장.</summary>
        public async void AutoSaveAsync()
        {
            if (_activeSlot >= 0)
            {
                await SaveAsync(_activeSlot);
            }
        }

        /// <summary>슬롯 메타 정보 조회.</summary>
        public SaveSlotInfo GetSlotInfo(int slotIndex)
        {
            // save_meta.json에서 slotIndex에 해당하는 정보 반환
            return null; // placeholder
        }

        /// <summary>슬롯 삭제.</summary>
        public void DeleteSlot(int slotIndex)
        {
            // save_{N}.json + save_{N}.json.bak 삭제
            // save_meta.json 갱신
        }

        /// <summary>저장 파일 존재 여부 확인.</summary>
        public bool HasSave(int slotIndex)
        {
            string path = GetSavePath(slotIndex);
            return File.Exists(path);
        }

        // === ISaveable 관리 ===

        public void Register(ISaveable saveable)
        {
            if (!_saveables.Contains(saveable))
                _saveables.Add(saveable);
        }

        public void Unregister(ISaveable saveable)
        {
            _saveables.Remove(saveable);
        }

        // === 유틸리티 ===

        private string GetSaveDir()
        {
            return Path.Combine(UnityEngine.Application.persistentDataPath, SAVE_DIR);
        }

        private string GetSavePath(int slotIndex)
        {
            return Path.Combine(GetSaveDir(), $"{FILE_PREFIX}{slotIndex}{FILE_EXT}");
        }

        private string GetBackupPath(int slotIndex)
        {
            return Path.Combine(GetSaveDir(), $"{FILE_PREFIX}{slotIndex}{BACKUP_EXT}");
        }
    }
}
```

#### 4.2 저장 흐름 (SaveAsync)

```
SaveAsync(slotIndex)
    │
    ├── 1) 가드: _isSaving이면 return false (중복 방지)
    │        slotIndex 범위 검증 (0 ~ MAX_SLOTS-1)
    │
    ├── 2) _isSaving = true
    │        SaveEvents.RaiseSaveStarted(slotIndex)
    │
    ├── 3) GameSaveData 인스턴스 생성
    │       ├── saveVersion = CURRENT_VERSION
    │       ├── savedAt = DateTime.UtcNow.ToString("o")
    │       └── playTimeSeconds = 기존 누적 + _playTimeAccumulator
    │
    ├── 4) ISaveable 목록을 SaveLoadOrder 순으로 정렬
    │        foreach saveable in _saveables.OrderBy(s => s.SaveLoadOrder):
    │            gameSaveData에 saveable.GetSaveData() 결과 할당
    │
    ├── 5) JSON 직렬화
    │       json = JsonConvert.SerializeObject(gameSaveData, _jsonSettings)
    │
    ├── 6) 원자적 파일 쓰기
    │       ├── 기존 save_{N}.json → save_{N}.json.bak으로 복사 (백업)
    │       ├── 임시 파일 save_{N}.json.tmp에 쓰기
    │       └── save_{N}.json.tmp → save_{N}.json으로 rename (원자적)
    │
    ├── 7) 메타 정보 갱신
    │       SaveMetaFile 로드 → slots[slotIndex] 갱신 → 재저장
    │
    ├── 8) _isSaving = false, _activeSlot = slotIndex
    │        SaveEvents.RaiseSaveCompleted(slotIndex)
    │        return true
    │
    └── 9) 예외 발생 시:
            _isSaving = false
            SaveEvents.RaiseSaveFailed(slotIndex, exception.Message)
            Debug.LogError(...)
            return false
```

#### 4.3 로드 흐름 (LoadAsync)

```
LoadAsync(slotIndex)
    │
    ├── 1) 가드: _isLoading이면 return false
    │        HasSave(slotIndex) == false이면 → TryLoadBackup (섹션 6.2)
    │
    ├── 2) _isLoading = true
    │        SaveEvents.RaiseLoadStarted(slotIndex)
    │
    ├── 3) 파일 읽기
    │       json = await File.ReadAllTextAsync(GetSavePath(slotIndex))
    │
    ├── 4) JSON 역직렬화
    │       gameSaveData = JsonConvert.DeserializeObject<GameSaveData>(json, _jsonSettings)
    │       → 파싱 실패 시 → TryLoadBackup (섹션 6.2)
    │
    ├── 5) 버전 검증 + 마이그레이션
    │       if gameSaveData.saveVersion != CURRENT_VERSION:
    │           gameSaveData = SaveMigrator.Migrate(gameSaveData)
    │       → see data-pipeline.md Part II 섹션 3.8 for 마이그레이션 전략
    │
    ├── 6) 데이터 무결성 검증
    │       errors = SaveDataValidator.Validate(gameSaveData)
    │       if errors.Count > 0: 경고 로그 출력 (치명적이지 않으면 계속 진행)
    │       → see data-pipeline.md Part II 섹션 5.2 for 검증 규칙
    │
    ├── 7) 시스템별 데이터 복원 (SaveLoadOrder 순)
    │       foreach saveable in _saveables.OrderBy(s => s.SaveLoadOrder):
    │           saveable.LoadSaveData(해당 데이터)
    │
    │       복원 순서 (→ see data-pipeline.md Part II 섹션 3.6):
    │       ├── [10] TimeManager        → 시간 기준 (가장 먼저)
    │       ├── [20] WeatherSystem      → 시간 의존
    │       ├── [30] EconomyManager     → 시간/날씨 의존
    │       ├── [40] FarmGrid           → SO 참조 복원 필요
    │       ├── [50] PlayerController   → 인벤토리 SO 참조
    │       ├── [55] InventoryManager   → 인벤토리 상태 복원
    │       ├── [60] BuildingManager    → 시설 상태 복원
    │       ├── [70] ProgressionManager → 해금/마일스톤
    │       ├── [75] NPCManager         → NPC 상태
    │       ├── [80] TutorialManager    → 튜토리얼 진행
    │       ├── [85] QuestManager       → 튜토리얼 완료 상태 참조
    │       └── [90] AchievementManager → QuestCompleted 조건 정합성 보장 (마지막)
    │
    ├── 8) _isLoading = false, _activeSlot = slotIndex
    │        SaveEvents.RaiseLoadCompleted(slotIndex)
    │        return true
    │
    └── 9) 예외 발생 시:
            _isLoading = false
            SaveEvents.RaiseLoadFailed(slotIndex, exception.Message)
            return false
```

#### 4.4 SaveSlotInfo 메타 구조

```csharp
// illustrative
namespace SeedMind.Save.Data
{
    [System.Serializable]
    public class SaveMetaFile
    {
        public SaveSlotInfo[] slots;  // MAX_SLOTS 크기
    }

    [System.Serializable]
    public class SaveSlotInfo
    {
        public int slotIndex;
        public string slotName;             // 플레이어 지정 이름 (기본: "슬롯 1")
        public string savedAt;              // ISO 8601
        public int year;                    // 표시용: 게임 내 연도
        public int seasonIndex;             // 표시용: 게임 내 계절
        public int day;                     // 표시용: 게임 내 일
        public int playTimeSeconds;         // 총 플레이 시간
        public int gold;                    // 표시용: 보유 골드
        public int level;                   // 표시용: 플레이어 레벨
        public bool exists;                 // 세이브 데이터 존재 여부
    }
}
```

(-> see `docs/pipeline/data-pipeline.md` Part II 섹션 3.3 for SaveSlotInfo canonical 정의)

---

### 5. 이벤트 시스템

#### 5.1 SaveEvents 정적 클래스

```csharp
// illustrative
namespace SeedMind.Save
{
    using System;

    /// <summary>
    /// 세이브/로드 시스템의 정적 이벤트 허브.
    /// UI, 자동저장 트리거 등 외부 시스템이 구독한다.
    /// </summary>
    public static class SaveEvents
    {
        // --- 저장 이벤트 ---
        public static event Action<int> OnSaveStarted;           // slotIndex
        public static event Action<int> OnSaveCompleted;         // slotIndex
        public static event Action<int, string> OnSaveFailed;    // slotIndex, errorMessage

        // --- 로드 이벤트 ---
        public static event Action<int> OnLoadStarted;           // slotIndex
        public static event Action<int> OnLoadCompleted;         // slotIndex
        public static event Action<int, string> OnLoadFailed;    // slotIndex, errorMessage

        // --- 자동저장 이벤트 ---
        public static event Action<string> OnAutoSaveTriggered;  // reason

        // --- 발행 메서드 ---
        internal static void RaiseSaveStarted(int slot) => OnSaveStarted?.Invoke(slot);
        internal static void RaiseSaveCompleted(int slot) => OnSaveCompleted?.Invoke(slot);
        internal static void RaiseSaveFailed(int slot, string msg) => OnSaveFailed?.Invoke(slot, msg);
        internal static void RaiseLoadStarted(int slot) => OnLoadStarted?.Invoke(slot);
        internal static void RaiseLoadCompleted(int slot) => OnLoadCompleted?.Invoke(slot);
        internal static void RaiseLoadFailed(int slot, string msg) => OnLoadFailed?.Invoke(slot, msg);
        internal static void RaiseAutoSaveTriggered(string reason) => OnAutoSaveTriggered?.Invoke(reason);
    }
}
```

#### 5.2 이벤트 구독 예시

| 구독자 | 이벤트 | 반응 |
|--------|--------|------|
| HUDController | OnSaveStarted | "저장 중..." 아이콘 표시 |
| HUDController | OnSaveCompleted | "저장 완료" 토스트 표시 후 페이드 |
| HUDController | OnSaveFailed | "저장 실패" 경고 표시 |
| SaveSlotUI | OnSaveCompleted | 슬롯 정보 새로고침 |
| SaveSlotUI | OnLoadCompleted | 슬롯 선택 UI 닫기 |
| AutoSaveTrigger | OnAutoSaveTriggered | (자체 발행, 디버그 로그) |

---

### 6. 오류 처리

#### 6.1 원자적 쓰기 (저장 실패 방지)

파일 쓰기 도중 크래시가 발생하면 JSON이 불완전해질 수 있다. 이를 방지하기 위해 **임시 파일 + rename** 패턴을 적용한다.

```
저장 프로세스:
1. save_{N}.json → save_{N}.json.bak 복사 (이전 성공본 백업)
2. save_{N}.json.tmp에 새 데이터 쓰기
3. save_{N}.json.tmp → save_{N}.json으로 rename (원자적 교체)
4. save_{N}.json.tmp 잔존 시 삭제

실패 시나리오:
- 2단계에서 크래시: .tmp만 불완전, 원본 .json은 무사
- 3단계에서 실패: .bak에서 복구 가능
```

[RISK] Windows에서 `File.Move`는 대상 파일이 존재하면 예외를 발생시킨다. `File.Move(src, dst, overwrite: true)` (.NET 6+) 또는 삭제 후 이동이 필요하다. Unity 6의 .NET 버전을 확인해야 한다.

#### 6.2 손상된 세이브 파일 감지 및 복구

```csharp
// illustrative
namespace SeedMind.Save
{
    public partial class SaveManager
    {
        /// <summary>
        /// 주 세이브 파일 로드 실패 시 백업에서 복구 시도.
        /// </summary>
        private async Task<GameSaveData> TryLoadWithFallback(int slotIndex)
        {
            string mainPath = GetSavePath(slotIndex);
            string backupPath = GetBackupPath(slotIndex);

            // 1차: 주 파일 로드 시도
            GameSaveData data = TryDeserialize(mainPath);
            if (data != null) return data;

            // 2차: 백업 파일 로드 시도
            Debug.LogWarning($"[SaveManager] 주 세이브 파일 손상, 백업에서 복구 시도: slot {slotIndex}");
            data = TryDeserialize(backupPath);
            if (data != null)
            {
                // 백업 성공 → 주 파일 복원
                File.Copy(backupPath, mainPath, overwrite: true);
                return data;
            }

            // 둘 다 실패
            Debug.LogError($"[SaveManager] 세이브 및 백업 모두 손상: slot {slotIndex}");
            return null;
        }

        private GameSaveData TryDeserialize(string path)
        {
            if (!File.Exists(path)) return null;
            try
            {
                string json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<GameSaveData>(json, _jsonSettings);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[SaveManager] 역직렬화 실패: {path} — {ex.Message}");
                return null;
            }
        }
    }
}
```

**복구 우선순위**:

1. 주 파일 (`save_{N}.json`) 로드 시도
2. 백업 파일 (`save_{N}.json.bak`) 로드 시도
3. 둘 다 실패 시 → 사용자에게 "세이브 파일 손상" 경고 UI 표시, 새 게임 안내

#### 6.3 버전 불일치 처리 (세이브 마이그레이션)

(-> see `docs/pipeline/data-pipeline.md` Part II 섹션 3.8 for SaveMigrator canonical 정의)

마이그레이션 원칙:

| 버전 변경 | 처리 | 사용자 경험 |
|-----------|------|-----------|
| Patch (1.0.0 -> 1.0.1) | 자동 (포맷 변경 없음) | 투명 |
| Minor (1.0.x -> 1.1.0) | SaveMigrator 체인 자동 변환 | "세이브 파일 업데이트됨" 토스트 |
| Major (1.x -> 2.0) | 로드 거부 | "호환되지 않는 세이브 파일" 경고 팝업 |

```csharp
// illustrative — 버전 판정 로직
namespace SeedMind.Save
{
    public partial class SaveManager
    {
        private GameSaveData HandleVersionMismatch(GameSaveData data)
        {
            var savedVer = new Version(data.saveVersion);
            var currentVer = new Version(CURRENT_VERSION);

            if (savedVer.Major != currentVer.Major)
            {
                // Major 불일치 → 로드 거부
                throw new SaveVersionException(
                    $"호환되지 않는 세이브 버전: {data.saveVersion} (현재: {CURRENT_VERSION})");
            }

            if (savedVer < currentVer)
            {
                // Minor/Patch 마이그레이션
                data = SaveMigrator.Migrate(data);
            }

            return data;
        }
    }

    public class SaveVersionException : Exception
    {
        public SaveVersionException(string message) : base(message) { }
    }
}
```

#### 6.4 오류 복구 매트릭스

| 오류 상황 | 감지 방법 | 복구 전략 |
|----------|----------|----------|
| JSON 파싱 실패 | JsonException catch | 백업 파일에서 복구 (섹션 6.2) |
| 파일 미존재 | File.Exists 체크 | 슬롯 비어있음으로 표시 |
| 쓰기 중 크래시 | .tmp 파일 잔존 감지 | 다음 로드 시 .tmp 삭제, .bak 사용 |
| 디스크 공간 부족 | IOException catch | 사용자 경고, 저장 취소 |
| Major 버전 불일치 | saveVersion 비교 | 로드 거부 + 경고 UI |
| null 필드 (구버전) | 역직렬화 후 null 체크 | 기본값으로 초기화 (마이그레이션) |
| dataId 참조 실패 | DataRegistry.Get 실패 | 해당 항목 스킵 + 경고 로그 |

---

### 7. ISaveable 인터페이스

(-> see `docs/pipeline/data-pipeline.md` Part II 섹션 3.6 for canonical 정의)

```csharp
// illustrative
namespace SeedMind.Save
{
    /// <summary>
    /// 저장/로드 대상 시스템이 구현하는 인터페이스.
    /// SaveManager가 이 인터페이스를 통해 각 시스템과 통신한다.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>복원 순서 (낮을수록 먼저 로드).</summary>
        int SaveLoadOrder { get; }

        /// <summary>현재 상태를 직렬화 가능한 객체로 반환.</summary>
        object GetSaveData();

        /// <summary>직렬화된 데이터에서 상태를 복원.</summary>
        void LoadSaveData(object data);
    }
}
```

**SaveLoadOrder 전체 할당표**:

| 시스템 | SaveLoadOrder | 근거 |
|--------|:------------:|------|
| TimeManager | 10 | 모든 시스템의 시간 기준 |
| WeatherSystem | 20 | 시간 의존 |
| EconomyManager | 30 | 시간/날씨 의존 |
| FarmGrid | 40 | SO 참조 복원(DataRegistry) 필요 |
| FarmZoneManager | 45 | FarmGrid(40) 복원 후 구역 해금 상태 적용 (ARC-023) |
| AnimalManager | 48 | FarmZoneManager(45) 이후 — Zone E 해금 상태 복원 후 동물 상태 로드 (ARC-019) |
| PlayerController | 50 | 인벤토리 SO 참조 필요 |
| FishingManager | 52 | PlayerController(50) 이후 — 낚시 상태 복원 (→ see docs/systems/fishing-architecture.md, FIX-052) |
| FishCatalogManager | 53 | FishingManager(52) 이후 — 도감 상태 복원, 구버전 세이브 마이그레이션 시 FishingStats 참조 (→ see docs/systems/fishing-architecture.md 섹션 20.4, ARC-030) |
| InventoryManager | 55 | 별도 인벤토리 상태 복원 |
| BuildingManager | 60 | 시설 SO 참조 복원 |
| ProgressionManager | 70 | 해금/마일스톤 복원 |
| NPCManager | 75 | NPC 상태, 여행 상인 일정 |
| TutorialManager | 80 | 튜토리얼 진행 (다른 시스템 상태 참조 가능) |
| QuestManager | 85 | 튜토리얼 완료 상태 참조 필요, 퀘스트 해금 조건 복원 |
| AchievementManager | 90 | QuestManager(85) 이후. QuestCompleted 조건 업적의 진행도 복원 정합성 보장 |

---

### 8. Unity 폴더 구조

#### 8.1 파일 배치

```
Assets/_Project/Scripts/Save/              # SeedMind.Save 네임스페이스
├── SaveManager.cs                          # 싱글턴, 비동기 저장/로드 API
├── AutoSaveTrigger.cs                      # 자동저장 트리거 컴포넌트
├── SaveEvents.cs                           # 정적 이벤트 허브
├── ISaveable.cs                            # 저장/로드 인터페이스
├── SaveMigrator.cs                         # 세이브 버전 마이그레이션
├── SaveDataValidator.cs                    # 세이브 데이터 무결성 검증
├── SaveVersionException.cs                 # 버전 불일치 예외
└── Data/                                   # SeedMind.Save.Data 네임스페이스
    ├── GameSaveData.cs                     # 통합 루트 세이브 클래스
    ├── SaveMetaFile.cs                     # 메타 파일 구조
    └── SaveSlotInfo.cs                     # 슬롯 정보 구조
```

개별 시스템의 SaveData 클래스는 각 시스템 폴더에 배치한다:

| 파일 | 위치 | 네임스페이스 |
|------|------|-------------|
| PlayerSaveData.cs | Scripts/Player/ | SeedMind.Player |
| FarmSaveData.cs, FarmTileSaveData.cs, CropInstanceSaveData.cs | Scripts/Farm/ | SeedMind.Farm |
| InventorySaveData.cs | Scripts/Player/ | SeedMind.Player |
| TimeSaveData.cs, WeatherSaveData.cs | Scripts/Core/ | SeedMind.Core |
| EconomySaveData.cs, PriceFluctuationSaveData.cs | Scripts/Economy/ | SeedMind.Economy |
| BuildingSaveData.cs, ProcessingSaveData.cs | Scripts/Building/ | SeedMind.Building |
| UnlockSaveData.cs | Scripts/Core/ | SeedMind.Core |
| ShopStockSaveData.cs | Scripts/Economy/ | SeedMind.Economy |
| MilestoneSaveData.cs | Scripts/Level/ | SeedMind.Level |
| NPCSaveData.cs, TravelingMerchantSaveData.cs | Scripts/NPC/Data/ | SeedMind.NPC.Data |
| TutorialSaveData.cs | Scripts/Tutorial/ | SeedMind.Tutorial |
| ToolUpgradeSaveData.cs | Scripts/Player/Data/ | SeedMind.Player |

#### 8.2 네임스페이스

```
SeedMind.Save                    # SaveManager, AutoSaveTrigger, SaveEvents, ISaveable
SeedMind.Save.Data               # GameSaveData, SaveMetaFile, SaveSlotInfo
```

(-> see `docs/systems/project-structure.md` 섹션 2 for 전체 네임스페이스 설계)

#### 8.3 asmdef 의존성

`SeedMind.Save.asmdef`는 다음을 참조한다:

| 참조 대상 | 이유 |
|----------|------|
| SeedMind.Core | Singleton, ISaveable 기본 인프라 |
| Newtonsoft.Json | JSON 직렬화 |

SaveManager가 각 시스템의 SaveData를 직접 참조하지 않고 ISaveable 인터페이스를 통해 통신하므로, Farm/Player/Economy 등에 대한 직접 의존성은 없다. 단, GameSaveData 루트 클래스가 모든 SaveData 타입을 참조하므로 GameSaveData는 SeedMind.Save.Data가 아닌 SeedMind.Core에 배치하는 방안도 검토 필요.

[OPEN] GameSaveData 클래스의 배치 위치. SeedMind.Save.Data에 두면 의존성이 모든 시스템으로 퍼진다. SeedMind.Core에 두면 Core의 책임이 과도해진다. 인터페이스 기반 역직렬화로 해결하거나, GameSaveData를 JObject(Newtonsoft)로 처리하고 각 시스템이 자신의 섹션만 파싱하는 방안이 있다.

---

## Part II -- MCP 구현 태스크 요약

---

### Phase A: SaveManager 싱글턴 생성

```
Step A-1: Scripts/Save/ 폴더 구조 생성
          → MCP: CreateFolder("Assets/_Project/Scripts/Save")
          → MCP: CreateFolder("Assets/_Project/Scripts/Save/Data")

Step A-2: ISaveable.cs 작성
          → MCP: CreateScript("ISaveable.cs", Scripts/Save/)
          → namespace SeedMind.Save, interface ISaveable 정의

Step A-3: SaveEvents.cs 작성
          → MCP: CreateScript("SaveEvents.cs", Scripts/Save/)
          → 정적 이벤트 7개 + Raise 메서드 정의

Step A-4: SaveManager.cs 작성
          → MCP: CreateScript("SaveManager.cs", Scripts/Save/)
          → Singleton<SaveManager> 상속, 비동기 API 구현
          → 원자적 쓰기, 백업 로직 포함

Step A-5: AutoSaveTrigger.cs 작성
          → MCP: CreateScript("AutoSaveTrigger.cs", Scripts/Save/)
          → 이벤트 구독, 쿨다운 로직

Step A-6: SCN_Farm 씬에 "SaveManager" GameObject 생성
          → MCP: CreateGameObject("SaveManager", parent="--- MANAGERS ---")
          → MCP: AddComponent(SaveManager)
          → MCP: AddComponent(AutoSaveTrigger)
```

### Phase B: GameSaveData 직렬화 테스트

```
Step B-1: Data/ 폴더에 GameSaveData.cs, SaveMetaFile.cs, SaveSlotInfo.cs 작성
          → MCP: CreateScript 3개

Step B-2: SaveMigrator.cs, SaveDataValidator.cs 작성
          → MCP: CreateScript 2개

Step B-3: 직렬화 테스트
          → MCP: EnterPlayMode
          → SaveManager.SaveAsync(0) 호출
          → persistentDataPath/Saves/ 에 JSON 파일 생성 확인
          → JSON 내용 로그 출력으로 검증

Step B-4: 역직렬화 테스트
          → SaveManager.LoadAsync(0) 호출
          → 각 시스템의 LoadSaveData 호출 확인 (콘솔 로그)
```

### Phase C: 자동저장 트리거 연결

```
Step C-1: AutoSaveTrigger가 TimeManager.OnDayChanged를 구독하는지 확인
          → MCP: EnterPlayMode → 하루 경과 → 자동저장 로그 확인

Step C-2: 쿨다운 동작 확인
          → 빠른 시간 진행으로 연속 트리거 발생 → 60초 간격 준수 확인

Step C-3: 수동 저장 테스트
          → SaveManager.SaveAsync(slotIndex) 직접 호출 → 쿨다운 무시 확인
```

### Phase D: SaveSlotUI 연결

```
Step D-1: Canvas_Overlay 하위에 SaveSlotPanel 생성
          → MCP: CreateGameObject("SaveSlotPanel", parent="Canvas_Overlay")
          → 3개 슬롯 UI 요소 배치

Step D-2: SaveSlotUI.cs 작성
          → SaveEvents 구독하여 슬롯 정보 갱신
          → GetSlotInfo() 호출로 메타 표시 (날짜, 골드, 플레이 시간)

Step D-3: 메인 메뉴 연동
          → SCN_MainMenu에서 "불러오기" 선택 시 SaveSlotPanel 표시
          → 슬롯 선택 → LoadAsync → SCN_Farm 전환
```

---

## Cross-references

- `docs/architecture.md` -- 마스터 기술 아키텍처 (SaveManager 위치: Core/SaveManager.cs)
- `docs/systems/project-structure.md` -- 네임스페이스, asmdef, 씬 계층 구조
- `docs/pipeline/data-pipeline.md` -- **핵심 참조**: SaveData 필드 정의 canonical (Part I 섹션 3, Part II 섹션 2~3)
- `docs/systems/farming-architecture.md` -- FarmSaveData, FarmTileSaveData, CropInstanceSaveData 사용처
- `docs/systems/inventory-architecture.md` -- InventorySaveData 구조, 저장/로드 흐름 (섹션 6)
- `docs/systems/tutorial-architecture.md` -- TutorialSaveData 구조 (섹션 7)
- `docs/systems/progression-architecture.md` -- ProgressionSaveData, MilestoneSaveData (섹션 4~5)
- `docs/systems/economy-architecture.md` -- EconomySaveData, PriceFluctuationSaveData (섹션 4.6)
- `docs/systems/npc-shop-architecture.md` -- NPCSaveData, TravelingMerchantSaveData (섹션 7)
- `docs/systems/tool-upgrade-architecture.md` -- ToolUpgradeSaveData (섹션 7.2)
- `docs/systems/time-season-architecture.md` -- TimeSaveData, WeatherSaveData (섹션 7)
- `docs/systems/achievement-architecture.md` (ARC-017) -- AchievementSaveData 구조, SaveLoadOrder 90 할당 (섹션 7)

---

## Open Questions

1. [OPEN] **InventorySaveData 위치**: PlayerSaveData 내부(data-pipeline.md 방식) vs 루트 레벨 분리(inventory-architecture.md 방식). 구현 시 한쪽으로 통일 필요. 루트 분리를 권장하되, 기존 data-pipeline.md와의 정합성 검토 필요.

2. [OPEN] **GameSaveData 클래스의 asmdef 배치**: SeedMind.Save.Data에 배치하면 모든 시스템 SaveData에 의존. JObject 파싱으로 각 시스템이 자기 섹션만 처리하는 방안 검토.

3. [OPEN] **자동저장 전용 슬롯**: 현재 3개 슬롯에 자동저장이 덮어쓰는 구조. 별도 `save_auto.json` 슬롯을 두어 수동 저장과 분리할지. (-> see data-pipeline.md Part II 섹션 3.7 동일 이슈)

4. [OPEN] **비동기 저장의 메인 스레드 블로킹**: `JsonConvert.SerializeObject`는 메인 스레드에서 실행된다. 데이터 수집(GetSaveData)도 메인 스레드 필수. 파일 I/O만 비동기로 충분한지, 직렬화까지 별도 스레드로 옮길지.

5. [OPEN] **Development/Release 빌드 분기**: Development에서는 `Formatting.Indented`, Release에서는 `Formatting.None`으로 전환할지. (-> see data-pipeline.md Part II 섹션 3.2 동일 이슈)

---

## Risks

1. [RISK] **원자적 쓰기 플랫폼 호환성**: Windows에서 `File.Move`는 대상 파일 존재 시 예외 발생. Unity 6의 .NET 버전에 따라 `File.Move(src, dst, overwrite: true)` 사용 가능 여부 확인 필요. 불가 시 삭제 후 이동 패턴 적용.

2. [RISK] **비동기 저장 중 게임 상태 변경**: SaveAsync 실행 중 게임이 계속 진행되면, 수집한 데이터와 실제 상태가 불일치할 수 있다. GetSaveData() 호출 시점에 스냅샷을 찍으므로 파일 I/O 중의 변경은 무관하나, 수집 자체가 여러 프레임에 걸치면 문제가 된다. 현재 설계에서는 단일 프레임 내 수집을 전제한다.

3. [RISK] **세이브 파일 크기 증가**: 현재 예상은 약 45KB(JSON minify 시 30KB 이하)이나, 농장 확장(16x16), 거래 로그 누적, NPC 시스템 확장 시 크기가 증가할 수 있다. 100KB 이상이면 직렬화 시간이 체감될 수 있으며, 파일 압축 또는 바이너리 포맷 검토가 필요하다. (-> see data-pipeline.md Part I 섹션 3.4 for 크기 예상)

4. [RISK] **ISaveable 등록 순서**: 각 시스템이 자발적으로 Register/Unregister를 호출하므로, 등록 누락이나 씬 재로드 시 중복 등록이 발생할 수 있다. OnEnable/OnDisable에서의 일관된 관리가 필수.

5. [RISK] **Dictionary 직렬화 일관성**: PriceFluctuationSaveData와 TutorialSaveData의 contextHintCooldowns에 Dictionary를 사용한다. Newtonsoft.Json은 Dictionary를 지원하나, data-pipeline.md에서는 StringIntPair[] 변환을 제안했다. 두 접근 방식이 혼재하면 혼란이 생긴다. 하나로 통일 필요.

---

*이 문서는 Claude Code가 기존 데이터 파이프라인/시스템 아키텍처 문서들의 세이브/로드 설계를 통합하고, 비동기 API, 자동저장 트리거, 오류 복구 전략을 독립적인 아키텍처 단위로 상세화하여 자율적으로 작성했습니다.*
