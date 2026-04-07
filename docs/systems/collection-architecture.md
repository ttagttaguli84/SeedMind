---
title: 수집 도감 시스템 기술 아키텍처
date: 2026-04-07
author: Claude Code (Opus 4.6)
---

# 수집 도감 시스템 기술 아키텍처

> 작성: Claude Code (Opus 4.6) | 2026-04-07
> 문서 ID: ARC-037

> FishCatalogManager(기존 ARC-030)와 GatheringCatalogManager(신규)를 통합 UI(CollectionUIController)로 묶는 수집 도감 시스템 아키텍처.

---

## 1. Context

SeedMind에는 두 가지 수집 시스템이 존재한다:

- **어종 도감 (Fish Catalog)**: FishCatalogManager가 15종 어종의 발견/포획 횟수/최대 크기를 추적 (-> see `docs/systems/fishing-architecture.md` Part VII, ARC-030)
- **채집 도감 (Gathering Catalog)**: 27종 채집 아이템의 발견/채집 횟수를 추적 (본 문서에서 신규 설계)

두 도감을 하나의 통합 UI(CollectionUIController)로 묶어 플레이어에게 단일 진입점을 제공하되, 내부적으로는 각 시스템의 독립성을 유지한다.

**설계 원칙**:
1. **FishCatalogManager 변경 없음**: 기존 ARC-030 아키텍처를 그대로 유지. 통합 UI가 FishCatalogManager를 읽기 전용으로 참조
2. **GatheringCatalogManager는 FishCatalogManager 패턴 동일 적용**: Singleton, ISaveable, 이벤트 패턴을 그대로 따라 일관성 확보
3. **CollectionUIController가 집계 전담**: 전체 완성도(%) = 어종 도감 + 채집 도감 합산

---

## 2. 시스템 다이어그램

```
+---------------------------------------------------------------------+
|                    수집 도감 시스템 전체 구조                           |
+---------------------------------------------------------------------+

             [기존 시스템 - 변경 없음]
+--------------------------------------------------------------+
|  FishCatalogManager (SeedMind.Fishing.Catalog)                |
|  (-> see fishing-architecture.md 섹션 16)                     |
|--------------------------------------------------------------|
|  + DiscoveredCount: int                                       |
|  + TotalFishCount: int (15)                                   |
|  + CompletionRate: float                                      |
|  + OnFishDiscovered: Action<string>                           |
|  + OnCatalogUpdated: Action<string, FishCatalogEntry>         |
|  + OnMilestoneReached: Action<int>                            |
|  + OnCatalogCompleted: Action                                 |
|  + GetEntry(fishId): FishCatalogEntry                         |
|  + GetAllEntries(): IReadOnlyDictionary<...>                  |
+--------------------------------------------------------------+
        ^                                        ^
        | 읽기 참조                               | 이벤트 구독
        |                                        |
+--------------------------------------------------------------+
|         CollectionUIController (SeedMind.Collection.UI)       |
|--------------------------------------------------------------|
|  [참조]                                                       |
|  - _fishCatalogManager: FishCatalogManager                   |
|  - _gatheringCatalogManager: GatheringCatalogManager         |
|                                                              |
|  [상태]                                                       |
|  - _currentTab: CollectionTab (Fish / Gathering)             |
|                                                              |
|  [읽기 전용 프로퍼티]                                          |
|  + TotalDiscoveredCount: int                                  |
|       // Fish.DiscoveredCount + Gathering.DiscoveredCount     |
|  + TotalItemCount: int                                        |
|       // Fish.TotalFishCount + Gathering.TotalItemCount       |
|  + OverallCompletionRate: float                               |
|       // TotalDiscoveredCount / TotalItemCount                |
|                                                              |
|  [메서드]                                                     |
|  + Open(): void                                               |
|  + Close(): void                                              |
|  + SwitchTab(CollectionTab tab): void                        |
|  - RefreshCurrentTab(): void                                  |
|  - UpdateCompletionHeader(): void                             |
+--------------------------------------------------------------+
        |                                        |
        | 읽기 참조                               | 이벤트 구독
        v                                        v
             [신규 시스템]
+--------------------------------------------------------------+
|  GatheringCatalogManager (SeedMind.Collection)                |
|  (MonoBehaviour, Singleton, ISaveable)                        |
|--------------------------------------------------------------|
|  [데이터]                                                      |
|  - _catalogDataRegistry: GatheringCatalogData[]              |
|       // Inspector 할당, 전체 채집 도감 SO (27종)              |
|  - _catalogDataMap: Dictionary<string, GatheringCatalogData> |
|       // itemId -> SO 빠른 조회용 (Initialize에서 구축)        |
|                                                              |
|  [런타임 상태]                                                 |
|  - _entries: Dictionary<string, GatheringCatalogEntry>       |
|       // itemId -> 런타임 도감 항목 (발견 여부, 채집 횟수 등)  |
|  - _discoveredCount: int                                      |
|       // 발견한 아이템 수 (빠른 조회용 캐시)                   |
|  - _totalItemCount: int                                       |
|       // 전체 아이템 수 (27) -- _catalogDataRegistry.Length    |
|                                                              |
|  [읽기 전용 프로퍼티]                                          |
|  + DiscoveredCount: int                                       |
|  + TotalItemCount: int                                        |
|  + CompletionRate: float  // DiscoveredCount / TotalItemCount |
|  + IsComplete: bool       // DiscoveredCount == TotalItemCount|
|                                                              |
|  [이벤트]                                                     |
|  + OnItemDiscovered: Action<string>                           |
|       // itemId -- 최초 발견 시에만 발행                       |
|  + OnCatalogUpdated: Action<string, GatheringCatalogEntry>   |
|       // itemId, 갱신된 항목 -- 매 채집 시 발행                |
|  + OnMilestoneReached: Action<int>                            |
|       // discoveredCount -- 마일스톤 도달 시 발행              |
|  + OnCatalogCompleted: Action                                 |
|       // 도감 100% 완성 시 1회 발행                            |
|                                                              |
|  [메서드]                                                     |
|  + Initialize(): void                                         |
|       // _catalogDataMap 구축, 빈 _entries 생성               |
|  + RegisterGather(string itemId, CropQuality quality,         |
|       int quantity): GatheringCatalogEntry                    |
|       // 채집 기록 갱신, 최초 발견/최고 품질 갱신 판정         |
|  + GetEntry(string itemId): GatheringCatalogEntry            |
|       // null이면 미발견                                       |
|  + GetCatalogData(string itemId): GatheringCatalogData       |
|  + GetAllEntries(): IReadOnlyDictionary<string,               |
|       GatheringCatalogEntry>                                  |
|  + IsDiscovered(string itemId): bool                          |
|  - CheckMilestone(int newCount): void                         |
|       // 마일스톤 도달 여부 판정                               |
|                                                              |
|  [ISaveable 구현]                                             |
|  + SaveLoadOrder => 56                                        |
|       // GatheringManager(54) 이후, InventoryManager(55) 이후 |
|       // 채집 도감은 인벤토리와 독립적                         |
|  + GetSaveData(): object                                      |
|  + LoadSaveData(object data): void                            |
|                                                              |
|  [구독]                                                       |
|  + OnEnable():                                                |
|      GatheringEvents.OnItemGathered += HandleItemGathered     |
|  + OnDisable(): 구독 해제                                     |
+--------------------------------------------------------------+
```

---

## 3. GatheringCatalogData (ScriptableObject)

채집 도감의 정적 표시 정보를 정의하는 SO. GatheringItemData SO와 1:1 대응하며, 도감 UI에서 표시할 힌트 및 보상 정보를 담는다. FishCatalogData(-> see `docs/systems/fishing-architecture.md` 섹션 15) 패턴을 따르되, 크기 관련 필드를 제외한다.

```csharp
// illustrative
namespace SeedMind.Collection
{
    [CreateAssetMenu(fileName = "NewGatheringCatalogData", menuName = "SeedMind/GatheringCatalogData")]
    public class GatheringCatalogData : ScriptableObject
    {
        [Header("식별")]
        public string itemId;                       // GatheringItemData.dataId와 동일 키
        public string displayName;                  // 도감 표시명
            // -> see docs/systems/gathering-system.md 섹션 3 for 아이템명

        [Header("도감 힌트")]
        [TextArea(2, 4)]
        public string hintLocked;                   // 미발견 시 표시 힌트
            // -> see docs/content/gathering-items.md (canonical, 아이템별 힌트 텍스트)
        [TextArea(2, 4)]
        public string descriptionUnlocked;          // 발견 후 표시 설명
            // -> see docs/content/gathering-items.md (canonical)
            // -> see docs/systems/collection-system.md 섹션 3.2 for 공통 필드명 정의

        [Header("희귀도")]
        public GatheringRarity rarity;              // -> see docs/systems/gathering-architecture.md 섹션 2.2

        [Header("초회 채집 보상")]
        public int firstDiscoverGold;               // 초회 등록 골드 보상
            // -> see docs/systems/collection-system.md 섹션 3.3 (Common=5G, Uncommon=15G, Rare=50G, Legendary=200G)
        public int firstDiscoverXP;                 // 초회 등록 XP 보상
            // -> see docs/systems/collection-system.md 섹션 3.3 (Common=2XP, Uncommon=5XP, Rare=15XP, Legendary=50XP)

        [Header("도감 표시")]
        public Sprite catalogIcon;                  // 도감 전용 아이콘 (null이면 GatheringItemData.icon 사용)
        public int sortOrder;                       // 도감 내 표시 순서
    }
}
```

### 3.1 GatheringCatalogData JSON 스키마

```json
{
    "itemId": "gather_mushroom",
    "displayName": "숲 버섯",
    "hintLocked": "비 오는 날 숲 속에서 축축한 땅을 살펴보면...",
    "descriptionUnlocked": "습한 환경을 좋아하는 버섯. 비 오는 날 더 자주 발견된다.",
    "rarity": "Common",
    "firstDiscoverGold": 5,
    "firstDiscoverXP": 2,
    "catalogIcon": "(Sprite 에셋 참조 -- JSON 직렬화 대상 아님)",
    "sortOrder": 1
}
```

**필드 설명**:
- `hintLocked`, `descriptionUnlocked`: canonical 값은 `(-> see docs/content/gathering-items.md)` 아이템별 힌트/설명 텍스트. 공통 필드명은 `(-> see docs/systems/collection-system.md 섹션 3.2)`.
- `firstDiscoverGold`, `firstDiscoverXP`: 희귀도별 보상은 `(-> see docs/systems/collection-system.md 섹션 3.3)` — 수치 직접 기재 금지.
- `catalogIcon`: Sprite 에셋 참조. Unity SO에서 Inspector로 할당. JSON 직렬화 대상 아님 (PATTERN-005).

### 3.2 PATTERN-005 검증: GatheringCatalogData C# <-> JSON 동기화

| C# 필드 | JSON 키 | 타입 | 일치 |
|---------|---------|------|:----:|
| itemId | itemId | string | O |
| displayName | displayName | string | O |
| hintLocked | hintLocked | string | O |
| descriptionUnlocked | descriptionUnlocked | string | O |
| rarity | rarity | GatheringRarity(string) | O |
| firstDiscoverGold | firstDiscoverGold | int | O |
| firstDiscoverXP | firstDiscoverXP | int | O |
| catalogIcon | (에디터 전용, 직렬화 제외) | Sprite | - |
| sortOrder | sortOrder | int | O |

필드 수: C# 9개 (catalogIcon 포함) / JSON 8개 (catalogIcon 제외, 에디터 전용) -- 일치

---

## 4. GatheringCatalogEntry (런타임 상태 클래스)

FishCatalogEntry(-> see `docs/systems/fishing-architecture.md` 섹션 17) 패턴을 따르되, 크기/Giant 관련 필드를 제외하고 품질 추적을 추가한다.

```csharp
// illustrative
namespace SeedMind.Collection
{
    /// <summary>
    /// 개별 채집 아이템의 도감 런타임 상태.
    /// GatheringCatalogManager가 Dictionary로 관리하며, 세이브/로드 시 직렬화된다.
    /// </summary>
    [System.Serializable]
    public class GatheringCatalogEntry
    {
        public string itemId;              // GatheringCatalogData.itemId 참조
        public bool isDiscovered;          // 발견 여부
        public int totalGathered;          // 이 아이템의 누적 채집 횟수
        public int bestQuality;            // 최고 품질 (CropQuality int 캐스팅: 0=Normal, 1=Silver, 2=Gold, 3=Iridium)
        public int firstGatheredDay;       // 최초 채집 일차 (-1 = 미발견)
        public int firstGatheredSeason;    // 최초 채집 계절 인덱스 (-1 = 미발견)
        public int firstGatheredYear;      // 최초 채집 연도 (-1 = 미발견)

        [System.NonSerialized]
        public bool isNewBestQuality;      // 이번 채집이 최고 품질 갱신인지 (직렬화 제외, UI 전용)

        public GatheringCatalogEntry(string id)
        {
            itemId = id;
            isDiscovered = false;
            totalGathered = 0;
            bestQuality = 0;
            firstGatheredDay = -1;
            firstGatheredSeason = -1;
            firstGatheredYear = -1;
            isNewBestQuality = false;
        }
    }
}
```

### 4.1 PATTERN-005 검증: GatheringCatalogEntry C# <-> JSON

| C# 필드 | JSON 키 | 타입 | 일치 |
|---------|---------|------|:----:|
| itemId | itemId | string | O |
| isDiscovered | isDiscovered | bool | O |
| totalGathered | totalGathered | int | O |
| bestQuality | bestQuality | int | O |
| firstGatheredDay | firstGatheredDay | int | O |
| firstGatheredSeason | firstGatheredSeason | int | O |
| firstGatheredYear | firstGatheredYear | int | O |
| isNewBestQuality | (NonSerialized, 직렬화 제외) | bool | - |

필드 수: C# 8개 (isNewBestQuality 포함) / JSON 7개 (isNewBestQuality 제외, NonSerialized) -- 일치

---

## 5. GatheringCatalogManager (MonoBehaviour, Singleton, ISaveable)

채집 도감 상태를 관리하는 매니저. FishCatalogManager(-> see `docs/systems/fishing-architecture.md` 섹션 16) 패턴을 동일하게 적용한다.

### 5.1 핵심 메서드

#### RegisterGather 알고리즘

```
RegisterGather(itemId, quality, quantity):
    entry = _entries[itemId]
    if entry == null:
        // 최초 발견
        entry = new GatheringCatalogEntry(itemId)
        _entries[itemId] = entry
        _discoveredCount++
        entry.isDiscovered = true
        entry.firstGatheredDay = TimeManager.CurrentDay
        entry.firstGatheredSeason = TimeManager.CurrentSeason
        entry.firstGatheredYear = TimeManager.CurrentYear

        // 초회 채집 보상 지급
        catalogData = _catalogDataMap[itemId]
        if catalogData.firstDiscoverGold > 0:
            EconomyManager.Instance.AddGold(catalogData.firstDiscoverGold)
        if catalogData.firstDiscoverXP > 0:
            ProgressionManager.Instance.AddExp(XPSource.GatheringCatalog, catalogData.firstDiscoverXP)

        OnItemDiscovered?.Invoke(itemId)
        CheckMilestone(_discoveredCount)

    entry.totalGathered += quantity

    qualityInt = (int)quality
    if qualityInt > entry.bestQuality:
        entry.bestQuality = qualityInt
        entry.isNewBestQuality = true    // UI에서 "NEW!" 표시용
    else:
        entry.isNewBestQuality = false

    OnCatalogUpdated?.Invoke(itemId, entry)
    return entry
```

#### CheckMilestone 알고리즘

```
CheckMilestone(newCount):
    milestones = [10, 20, 27]
        // -> see docs/systems/collection-system.md 섹션 5.3.2
        // 10종: 초보 채집가 (150G + 40XP + 채집 낫 수리 키트 x1)
        // 20종: 숙련 채집가 (400G + 100XP + 도감 배경 "사계절의 숲" + 고급 비료 x5)
        // 27종: 채집 도감 마스터 (600G + 180XP + 도감 배경 "전설의 자연" + 프레임 "숲의 정령")
    if newCount in milestones:
        OnMilestoneReached?.Invoke(newCount)
    if newCount == _totalItemCount:
        OnCatalogCompleted?.Invoke()
```

#### GetEntry / GetCatalogData

```
GetEntry(itemId):
    return _entries.TryGetValue(itemId, out entry) ? entry : null

GetCatalogData(itemId):
    return _catalogDataMap.TryGetValue(itemId, out data) ? data : null

IsDiscovered(itemId):
    entry = GetEntry(itemId)
    return entry != null && entry.isDiscovered
```

### 5.2 세이브/로드 통합 (GatheringCatalogSaveData)

```csharp
// illustrative
namespace SeedMind.Collection
{
    [System.Serializable]
    public class GatheringCatalogSaveData
    {
        public List<GatheringCatalogEntry> entries;     // 발견된 채집 아이템 목록
        public int discoveredCount;                      // 발견 아이템 수 (빠른 조회용 캐시)
    }
}
```

#### JSON 예시

```json
{
    "entries": [
        {
            "itemId": "gather_mushroom",
            "isDiscovered": true,
            "totalGathered": 24,
            "bestQuality": 2,
            "firstGatheredDay": 3,
            "firstGatheredSeason": 0,
            "firstGatheredYear": 1
        },
        {
            "itemId": "gather_dandelion",
            "isDiscovered": true,
            "totalGathered": 15,
            "bestQuality": 0,
            "firstGatheredDay": 1,
            "firstGatheredSeason": 0,
            "firstGatheredYear": 1
        }
    ],
    "discoveredCount": 2
}
```

#### PATTERN-005 검증: GatheringCatalogSaveData C# <-> JSON

| C# 필드 | JSON 키 | 타입 | 일치 |
|---------|---------|------|:----:|
| entries | entries | List\<GatheringCatalogEntry\> | O |
| discoveredCount | discoveredCount | int | O |

필드 수: 2개 -- 양쪽 일치

#### SaveLoadOrder 할당

| 시스템 | SaveLoadOrder | 근거 |
|--------|:------------:|------|
| GatheringCatalogManager | **56** | GatheringManager(54)와 InventoryManager(55) 이후. 채집 도감은 인벤토리에 의존하지 않으며 순수 통계 데이터만 복원하므로 55 이후면 충분하다. FishCatalogManager(53)와의 간격을 유지하여 향후 확장 여지를 남긴다. |

(-> see `docs/systems/save-load-architecture.md` 섹션 7 for 전체 SaveLoadOrder 할당표)

#### 세이브/로드 흐름

**저장**:
```
SaveManager.SaveAsync()
    +-- [54] GatheringManager.GetSaveData()        // 기존
    +-- [55] InventoryManager.GetSaveData()        // 기존
    +-- [56] GatheringCatalogManager.GetSaveData()
    |       -> return new GatheringCatalogSaveData {
    |           entries = new List<GatheringCatalogEntry>(_entries.Values),
    |           discoveredCount = _discoveredCount
    |       }
```

**로드**:
```
SaveManager.LoadAsync()
    +-- [54] GatheringManager.LoadSaveData(data.gathering)       // 기존
    +-- [55] InventoryManager.LoadSaveData(data.inventory)       // 기존
    +-- [56] GatheringCatalogManager.LoadSaveData(data.gatheringCatalog)
    |       if (data.gatheringCatalog == null):
    |           // 구버전 세이브 -- 기존 GatheringStats.gatheredByItemId에서 마이그레이션
    |           MigrateFromGatheringStats(data.gathering)
    |       else:
    |           _entries.Clear()
    |           _discoveredCount = 0
    |           foreach (var entry in data.gatheringCatalog.entries):
    |               _entries[entry.itemId] = entry
    |               if entry.isDiscovered: _discoveredCount++
```

**구버전 세이브 마이그레이션**:
```
MigrateFromGatheringStats(GatheringSaveData gatheringData):
    if gatheringData == null: return
    foreach (kvp in gatheringData.stats.gatheredByItemId):
        entry = new GatheringCatalogEntry(kvp.Key)
        entry.isDiscovered = true
        entry.totalGathered = kvp.Value
        entry.bestQuality = 0           // 구버전에 품질 기록 없음
        entry.firstGatheredDay = -1     // 구버전에 시점 기록 없음
        entry.firstGatheredSeason = -1
        entry.firstGatheredYear = -1
        _entries[kvp.Key] = entry
        _discoveredCount++
```

---

## 6. CollectionUIController

통합 도감 UI를 제어하는 컨트롤러. 어종 도감과 채집 도감을 탭으로 분리하여 표시하되, 상단에 전체 완성도를 집계한다.

### 6.1 탭 구조

```csharp
// illustrative
namespace SeedMind.Collection.UI
{
    public enum CollectionTab
    {
        Fish,       // 어종 도감 (FishCatalogManager 데이터)
        Gathering   // 채집 도감 (GatheringCatalogManager 데이터)
    }
}
```

```
+--------------------------------------------------------------+
|         CollectionUIController (MonoBehaviour)                 |
|--------------------------------------------------------------|
|  [참조]                                                       |
|  - _fishCatalogManager: FishCatalogManager                   |
|  - _gatheringCatalogManager: GatheringCatalogManager         |
|                                                              |
|  [UI 참조]                                                    |
|  - _tabButtons: Button[] (2개: Fish, Gathering)              |
|  - _completionHeaderText: TMP_Text                           |
|       // "전체 수집 도감 12/42 (28.6%)" 형식                   |
|  - _fishPanel: FishCatalogUI                                  |
|       // (-> see fishing-architecture.md 섹션 21.1)           |
|  - _gatheringPanel: GatheringCatalogUI                       |
|       // (아래 섹션 6.3 참조)                                  |
|                                                              |
|  [상태]                                                       |
|  - _currentTab: CollectionTab                                |
|  - _isOpen: bool                                             |
|                                                              |
|  [읽기 전용 프로퍼티]                                          |
|  + TotalDiscoveredCount: int                                  |
|       // _fishCatalogManager.DiscoveredCount                  |
|       //   + _gatheringCatalogManager.DiscoveredCount         |
|  + TotalItemCount: int                                        |
|       // _fishCatalogManager.TotalFishCount (15)              |
|       //   -> see docs/content/fish-catalog.md 섹션 3.1       |
|       //   + _gatheringCatalogManager.TotalItemCount (27)     |
|       //   -> see docs/systems/collection-system.md 섹션 5.2  |
|  + OverallCompletionRate: float                               |
|       // TotalDiscoveredCount / TotalItemCount                |
|                                                              |
|  [메서드]                                                     |
|  + Open(): void                                               |
|       // 패널 활성화, 현재 탭 새로고침, 완성도 헤더 갱신       |
|  + Close(): void                                              |
|  + SwitchTab(CollectionTab tab): void                        |
|       // 탭 전환, 해당 패널 활성화/비활성화                    |
|  - RefreshCurrentTab(): void                                  |
|  - UpdateCompletionHeader(): void                             |
|       // TotalDiscoveredCount, TotalItemCount, % 표시         |
|                                                              |
|  [구독]                                                       |
|  + OnEnable():                                                |
|      _fishCatalogManager.OnCatalogUpdated += OnFishUpdated    |
|      _gatheringCatalogManager.OnCatalogUpdated += OnGatherUpdated |
|  + OnDisable(): 구독 해제                                     |
+--------------------------------------------------------------+
```

### 6.2 완성도 집계

전체 도감 완성도는 어종 도감과 채집 도감의 발견 수를 단순 합산한다.

```
UpdateCompletionHeader():
    fishDiscovered = _fishCatalogManager.DiscoveredCount
    fishTotal = _fishCatalogManager.TotalFishCount           // 15 -> see docs/content/fish-catalog.md 섹션 3.1
    gatherDiscovered = _gatheringCatalogManager.DiscoveredCount
    gatherTotal = _gatheringCatalogManager.TotalItemCount    // 27 -> see docs/systems/collection-system.md 섹션 5.2

    totalDiscovered = fishDiscovered + gatherDiscovered
    totalAll = fishTotal + gatherTotal                       // 42 = 15 + 27
    percentage = (totalDiscovered / (float)totalAll) * 100f

    _completionHeaderText.text = $"전체 수집 도감 {totalDiscovered}/{totalAll} ({percentage:F1}%)"
```

**탭별 완성도**: 각 탭 상단에도 해당 카테고리의 완성도를 별도 표시한다.
- Fish 탭: `"어종 도감 {fishDiscovered}/{fishTotal}"`
- Gathering 탭: `"채집 도감 {gatherDiscovered}/{gatherTotal}"`

### 6.3 GatheringCatalogUI (채집 도감 탭 패널)

FishCatalogUI(-> see `docs/systems/fishing-architecture.md` 섹션 21.1~21.4) 패턴을 동일 적용한다.

```
+--------------------------------------------------------------+
|           GatheringCatalogUI (MonoBehaviour)                   |
|--------------------------------------------------------------|
|  - _catalogManager: GatheringCatalogManager                  |
|  - _scrollRect: ScrollRect                                   |
|  - _itemPrefab: GatheringCatalogItemUI (목록 항목 프리팹)     |
|  - _contentParent: Transform (ScrollRect Content)            |
|  - _detailPanel: GatheringCatalogDetailPanel                 |
|  - _categoryFilter: GatheringCategory?  (null = 전체)        |
|  - _itemPool: List<GatheringCatalogItemUI> (오브젝트 풀)     |
|                                                              |
|  + Refresh(): void                                            |
|  + SetCategoryFilter(GatheringCategory? cat): void           |
|  + SelectItem(string itemId): void                           |
+--------------------------------------------------------------+

+--------------------------------------------------------------+
|           GatheringCatalogItemUI (MonoBehaviour)              |
|--------------------------------------------------------------|
|  - _icon: Image                                              |
|  - _nameText: TMP_Text                                       |
|  - _rarityBadge: Image                                       |
|  - _gatheredCountText: TMP_Text                              |
|  - _lockOverlay: GameObject (미발견 시 활성화)                 |
|                                                              |
|  + SetData(GatheringCatalogData data,                        |
|       GatheringCatalogEntry entry): void                     |
|  + OnClick():                                                |
|       // GatheringCatalogUI.SelectItem(itemId) 호출           |
+--------------------------------------------------------------+

+--------------------------------------------------------------+
|        GatheringCatalogDetailPanel (MonoBehaviour)            |
|--------------------------------------------------------------|
|  - _icon: Image                                              |
|  - _nameText: TMP_Text                                       |
|  - _descriptionText: TMP_Text (hintLocked or descriptionUnlocked) |
|  - _rarityText: TMP_Text                                     |
|  - _totalGatheredText: TMP_Text                              |
|  - _bestQualityText: TMP_Text                                |
|  - _firstGatheredText: TMP_Text                              |
|                                                              |
|  + ShowItem(GatheringCatalogData data,                       |
|       GatheringCatalogEntry entry): void                     |
|  + Hide(): void                                              |
+--------------------------------------------------------------+
```

### 6.4 씬 계층 구조

```
Canvas
├── CollectionPanel (기본 비활성, CollectionUIController)
│   ├── Header
│   │   ├── TitleText ("수집 도감")
│   │   ├── CompletionText ("전체 수집 도감 12/42 (28.6%)")
│   │   └── CloseButton
│   ├── TabBar
│   │   ├── FishTabButton
│   │   └── GatheringTabButton
│   ├── FishPanel (FishCatalogUI -- 기존 fishing-architecture.md 섹션 21.5 구조)
│   │   ├── CategoryCompletionText ("어종 도감 5/15")
│   │   ├── ScrollView
│   │   │   └── Content
│   │   │       └── FishCatalogItemUI (프리팹, 15개 인스턴스)
│   │   └── DetailPanel (FishCatalogDetailPanel)
│   └── GatheringPanel (GatheringCatalogUI)
│       ├── CategoryCompletionText ("채집 도감 7/27")
│       ├── CategoryFilter (Dropdown: 전체/꽃/식물/버섯/열매/수생 식물/광물)
│       │       // -> see docs/systems/collection-system.md 섹션 6.4 (채집 탭 추가 필터 유형별)
│       ├── ScrollView
│       │   └── Content
│       │       └── GatheringCatalogItemUI (프리팹, 27개 인스턴스)
│       └── DetailPanel (GatheringCatalogDetailPanel)
└── GatheringCatalogToast (기본 비활성, 화면 상단)
    ├── IconImage
    ├── MessageText ("새로운 채집물 발견!")
    └── ItemNameText
```

**FishCatalogToast와의 관계**: 기존 FishCatalogToast(-> see `docs/systems/fishing-architecture.md` 섹션 21.4)는 그대로 유지한다. 어종 발견 토스트와 채집물 발견 토스트는 별도 프리팹으로 관리하여 시각적 구분이 가능하도록 한다.

---

## 7. 이벤트 연동

### 7.1 GatheringManager.OnItemGathered -> GatheringCatalogManager

```
이벤트 흐름:

플레이어 채집 인터랙션
    |
    v
GatheringManager.TryGather(point)
    |   (-> see gathering-architecture.md 섹션 5.1)
    |   SelectGatheringItem -> CalculateQuantity -> DetermineQuality -> GrantRewards
    |
    +-- GatheringEvents.OnItemGathered?.Invoke(item, quality, quantity)
        |
        +-- [구독자 1] GatheringCatalogManager.HandleItemGathered()
        |       |
        |       +-- RegisterGather(item.dataId, quality, quantity)
        |       |       |
        |       |       +-- (최초 발견 시) OnItemDiscovered?.Invoke(itemId)
        |       |       |       |
        |       |       |       +-- [구독자] GatheringCatalogUI (토스트 표시)
        |       |       |       +-- [구독자] AchievementManager (채집 도감 업적 체크)
        |       |       |
        |       |       +-- (마일스톤 도달 시) OnMilestoneReached?.Invoke(count)
        |       |       |       |
        |       |       |       +-- [구독자] CollectionUIController (완성도 갱신)
        |       |       |
        |       |       +-- OnCatalogUpdated?.Invoke(itemId, entry)
        |       |               |
        |       |               +-- [구독자] GatheringCatalogUI (항목 갱신)
        |       |               +-- [구독자] CollectionUIController (헤더 갱신)
        |
        +-- [구독자 2] ProgressionManager (기존, 채집 XP)
        +-- [구독자 3] InventoryManager (기존, 아이템 추가)
```

### 7.2 FishCatalogManager 이벤트 -> CollectionUIController

```
FishCatalogManager.OnCatalogUpdated -> CollectionUIController.UpdateCompletionHeader()
FishCatalogManager.OnFishDiscovered -> CollectionUIController.UpdateCompletionHeader()
GatheringCatalogManager.OnCatalogUpdated -> CollectionUIController.UpdateCompletionHeader()
GatheringCatalogManager.OnItemDiscovered -> CollectionUIController.UpdateCompletionHeader()
```

CollectionUIController는 양쪽 매니저의 이벤트를 구독하여, 어느 쪽에서 변경이 발생하든 전체 완성도 헤더를 갱신한다.

---

## 8. 폴더 구조 및 네임스페이스

### 8.1 신규 폴더

```
Assets/_Project/Scripts/Collection/           # SeedMind.Collection 네임스페이스
├── GatheringCatalogData.cs                    # ScriptableObject (채집 도감 정적 데이터)
├── GatheringCatalogEntry.cs                   # 런타임 상태 클래스
├── GatheringCatalogManager.cs                 # 도감 매니저 (Singleton, ISaveable)
├── GatheringCatalogSaveData.cs                # 세이브 데이터 클래스
└── UI/                                        # SeedMind.Collection.UI 네임스페이스
    ├── CollectionUIController.cs              # 통합 도감 진입점 (탭 전환, 완성도 집계)
    ├── CollectionTab.cs                       # enum 정의
    ├── GatheringCatalogUI.cs                  # 채집 도감 패널
    ├── GatheringCatalogItemUI.cs              # 채집 도감 항목 프리팹
    ├── GatheringCatalogDetailPanel.cs         # 채집 도감 상세 패널
    └── GatheringCatalogToastUI.cs             # 채집 발견 토스트 알림
```

### 8.2 기존 폴더 (변경 없음)

```
Assets/_Project/Scripts/Fishing/Catalog/      # SeedMind.Fishing.Catalog (기존 ARC-030, 변경 없음)
├── FishCatalogData.cs
├── FishCatalogEntry.cs
├── FishCatalogManager.cs
├── FishCatalogSaveData.cs
└── UI/
    ├── FishCatalogUI.cs
    ├── FishCatalogItemUI.cs
    ├── FishCatalogDetailPanel.cs
    └── FishCatalogToastUI.cs
```

### 8.3 SO 에셋 폴더

```
Assets/_Project/Data/
├── Catalog/                                   # 기존 FishCatalogData SO (변경 없음)
│   ├── FishCatalog_CrucianCarp.asset
│   └── ... (15종)
└── GatheringCatalog/                          # 신규 GatheringCatalogData SO
    ├── GatheringCatalog_Dandelion.asset
    ├── GatheringCatalog_WildGarlic.asset
    └── ... (27종)
```

### 8.4 네임스페이스 매핑

| 네임스페이스 | 폴더 | 역할 |
|-------------|------|------|
| `SeedMind.Collection` | `Scripts/Collection/` | GatheringCatalogManager, 데이터 클래스 |
| `SeedMind.Collection.UI` | `Scripts/Collection/UI/` | CollectionUIController, 채집 도감 UI |
| `SeedMind.Fishing.Catalog` | `Scripts/Fishing/Catalog/` | FishCatalogManager (기존, 변경 없음) |

### 8.5 Assembly Definition

| asmdef | 경로 | 참조 |
|--------|------|------|
| `SeedMind.Collection.asmdef` | `Scripts/Collection/` | Core, Gathering, Fishing (FishCatalogManager 읽기 참조) |

### 8.6 의존성 방향

```
SeedMind.Collection (신규)
    | reads
    +-- FishCatalogManager (SeedMind.Fishing.Catalog)    // 읽기 전용
    +-- GatheringEvents (SeedMind.Gathering)              // 이벤트 구독
    +-- GatheringItemData (SeedMind.Gathering)            // SO 참조
    +-- GatheringRarity (SeedMind.Gathering)              // enum 참조 -- FishRarity(SeedMind.Fishing.Catalog)와 별도 유지 [ARC-038 확정]
    | calls
    +-- EconomyManager (초회 보상 골드)
    +-- ProgressionManager (초회 보상 XP)
    | implements
    +-- ISaveable (SaveManager 등록)
```

**순환 참조 방지**: SeedMind.Collection은 Gathering과 Fishing.Catalog를 단방향으로 참조한다. 역방향 참조(Gathering -> Collection)는 이벤트를 통해 느슨하게 연결한다. Gathering 시스템은 Collection 네임스페이스를 알지 못한다.

---

## 9. MCP 구현 태스크 요약

### Phase M: 도감 데이터 레이어

| Step | MCP 명령 | 비고 |
|:----:|---------|------|
| M-1 | CreateScript `GatheringCatalogData.cs` : ScriptableObject | SeedMind.Collection 네임스페이스, 섹션 3 스키마 |
| M-2 | CreateScript `GatheringCatalogEntry.cs` | 런타임 상태 클래스, 섹션 4 |
| M-3 | CreateScript `GatheringCatalogSaveData.cs` | 세이브 데이터 클래스, 섹션 5.2 |
| M-4 | CreateScript `CollectionTab.cs` | enum 정의, 섹션 6.1 |

### Phase N: 도감 시스템 레이어

| Step | MCP 명령 | 비고 |
|:----:|---------|------|
| N-1 | CreateScript `GatheringCatalogManager.cs` : MonoBehaviour | Singleton, ISaveable(order=56), 섹션 5 |
| N-2 | EditScript `GatheringEvents.cs` | 기존 OnItemGathered 확인 (GatheringCatalogManager 구독 대상) |

### Phase O: 도감 SO 에셋 생성

| Step | MCP 명령 | 비고 |
|:----:|---------|------|
| O-1 | CreateAsset `GatheringCatalog_Dandelion` 등 27종 | GatheringCatalogData SO 에셋, 수치는 `(-> see docs/content/gathering-items.md)` |

### Phase P: 도감 UI 프리팹

| Step | MCP 명령 | 비고 |
|:----:|---------|------|
| P-1 | CreateScript `CollectionUIController.cs` | 통합 도감 컨트롤러, 섹션 6 |
| P-2 | CreateScript `GatheringCatalogUI.cs` | 채집 도감 패널, 섹션 6.3 |
| P-3 | CreateScript `GatheringCatalogItemUI.cs` | 항목 프리팹, 섹션 6.3 |
| P-4 | CreateScript `GatheringCatalogDetailPanel.cs` | 상세 패널, 섹션 6.3 |
| P-5 | CreateScript `GatheringCatalogToastUI.cs` | 발견 토스트 |
| P-6 | CreatePrefab `CollectionPanel` | 씬 계층 구성, 섹션 6.4 |
| P-7 | CreatePrefab `GatheringCatalogToast` | 토스트 알림 프리팹 |

### Phase Q: 기존 시스템 확장

| Step | MCP 명령 | 비고 |
|:----:|---------|------|
| Q-1 | EditScript `GameSaveData.cs` | `public GatheringCatalogSaveData gatheringCatalog;` 필드 추가 |
| Q-2 | EditScript `SaveManager.cs` | GatheringCatalogManager ISaveable 등록 확인, SaveLoadOrder=56 |
| Q-3 | CreateGameObject `GatheringCatalogManager` | 싱글턴 오브젝트, GatheringCatalogData[] 할당 |
| Q-4a | `FishCatalogPanel` 프리팹을 `FishPanel`로 이름 변경 | In-place migration step 1 [ARC-039] |
| Q-4b | FishPanel을 CollectionPanel 하위 자식으로 이동 | 씬 계층 반영, 섹션 6.4 |
| Q-4c | FishPanel/Header/CloseButton 비활성화 (SetActive false) | CollectionPanel 공통 CloseButton 사용 |
| Q-4d | FishPanel/Header 완성도 표시 비활성화 | CollectionUIController에 위임 |
| Q-4e | FishCatalogUI.cs Inspector 참조 재연결 (FishPanel 기준으로) | 기존 스크립트 코드 변경 없음 |
| Q-4f | 구버전 FishCatalogPanel.prefab → `DEPRECATED_FishCatalogPanel` 리네이밍, Legacy/ 이동 | 참조 깨짐 방지 |

### Phase R: 검증

| Step | 검증 항목 | 비고 |
|:----:|---------|------|
| R-1 | GatheringCatalogManager 싱글턴 정상 생성 확인 (MCP Console.Log) | |
| R-2 | 채집 아이템 수확 시 GatheringCatalogEntry 생성 및 isDiscovered=true 확인 | |
| R-3 | 동일 아이템 재채집 시 totalGathered 증가 및 bestQuality 갱신 확인 | |
| R-4 | 미발견 아이템 힌트 텍스트 표시, 발견 후 전환 확인 | |
| R-5 | CollectionUIController 탭 전환 (Fish <-> Gathering) 정상 동작 확인 | |
| R-6 | 전체 완성도 집계 정확성 확인 (Fish + Gathering 합산) | |
| R-7 | 세이브 -> 로드 후 GatheringCatalogSaveData 복원 확인 (discoveredCount, entries) | |
| R-8 | 구버전 세이브(gatheringCatalog=null) 로드 시 마이그레이션 정상 작동 확인 | |

---

## Cross-references

| 문서 | 관련 섹션 | 연관 |
|------|----------|------|
| `docs/systems/fishing-architecture.md` (ARC-026/ARC-030) | Part VII 섹션 15~21 | FishCatalogData, FishCatalogManager, FishCatalogEntry, FishCatalogSaveData -- 본 문서의 GatheringCatalog* 패턴 원본. 섹션 21.5 씬 계층은 Q-4 마이그레이션 후 CollectionPanel/FishPanel로 통합됨 [ARC-039] |
| `docs/systems/gathering-architecture.md` (ARC-031) | 섹션 1, 2.2 | GatheringManager, GatheringItemData, GatheringRarity, GatheringEvents.OnItemGathered -- 이벤트 소스 |
| `docs/content/fish-catalog.md` (CON-011) | 섹션 3.1, 4.1 | 어종별 도감 데이터, 마일스톤 보상 -- fish catalog canonical |
| `docs/content/gathering-items.md` (CON-012) | 섹션 3~7 | 채집 아이템 힌트 텍스트, 희귀도 -- gathering catalog canonical |
| `docs/systems/save-load-architecture.md` (ARC-011) | 섹션 7 | SaveLoadOrder 할당표 (GatheringCatalogManager=56 추가 필요) |
| `docs/systems/achievement-architecture.md` (ARC-024) | 섹션 3 | AchievementManager 패턴 참조, 채집 도감 업적 연동 가능성 |
| `docs/pipeline/data-pipeline.md` | Part I | GameSaveData에 `gatheringCatalog` 필드 추가 필요 |
| `docs/systems/project-structure.md` | 섹션 2, 3 | 네임스페이스 규칙, asmdef 구조 |
| `docs/systems/collection-system.md` (DES-018) | 섹션 3.2, 3.3, 5.2, 5.3.2, 6.4 | 도감 공통 필드명, 희귀도별 보상 수치, 채집 도감 아이템 수, 마일스톤 보상 상세, 채집 탭 필터 유형 — canonical |

---

## Open Questions

1. ~~[OPEN] (ARC-037) **채집 도감 마일스톤 목록 및 보상**~~ — **[DES-018 완료]** 10종/20종/27종으로 확정. 보상 상세: (-> see docs/systems/collection-system.md 섹션 5.3.2). CheckMilestone 알고리즘 동기화 완료.

2. ~~[OPEN] (ARC-037) **firstDiscoverGold / firstDiscoverXP 희귀도별 스케일**~~ — **[DES-018 완료]** Common=5G/2XP, Uncommon=15G/5XP, Rare=50G/15XP, Legendary=200G/50XP 확정. (-> see docs/systems/collection-system.md 섹션 3.3). 필드명도 `firstDiscoverGold`/`firstDiscoverXP`로 통일.

3. ~~[OPEN] (ARC-037) **GatheringRarity와 FishRarity 통합 여부**: 두 enum이 동일 구조(Common/Uncommon/Rare/Legendary)이다. `SeedMind.ItemRarity`로 통합하면 CollectionUIController에서 희귀도 UI를 일원화할 수 있지만, 시스템 결합도가 높아진다. (-> see `docs/systems/gathering-architecture.md` 섹션 9 OPEN 항목 2)~~ — **[ARC-038 확정]** 분리 유지. 이유: (1) 섹션 1 설계 원칙 "FishCatalogManager 변경 없음" — FishRarity를 공통 enum으로 이동하면 FishCatalogManager 내부 수정 필요, 원칙 위반. (2) CollectionUIController 탭이 FishPanel/GatheringPanel으로 이미 분리되어 있어 공통 enum 없이도 탭별 독립 렌더링 가능. (3) 두 시스템의 향후 독립 진화 여지 보존 (FishRarity: isGiant 연동, GatheringRarity: weatherBonus 연동 등). ICatalogProvider 인터페이스 도입 없음 — CollectionUIController가 FishCatalogManager와 GatheringCatalogManager를 직접 참조하는 현 설계 유지.

4. ~~[OPEN] (ARC-037) **CollectionPanel과 기존 FishCatalogPanel 통합 시 씬 재구성 범위**: 기존 FishCatalogPanel이 독립 프리팹이므로, CollectionPanel 하위로 이동 시 기존 참조가 깨질 수 있다. Phase Q-4에서 신중한 마이그레이션 필요.~~ — **[ARC-039 확정]** 참조 재연결 방식(In-place migration) 채택. 단계: (1) FishCatalogPanel 프리팹을 FishPanel로 이름 변경, (2) FishPanel을 CollectionPanel 자식으로 이동, (3) FishPanel 내 CloseButton 비활성화(CollectionPanel 공통 CloseButton 사용), (4) FishPanel Header 완성도 표시 비활성화(CollectionUIController 위임), (5) FishCatalogUI.cs Inspector 참조 재연결, (6) 구버전 FishCatalogPanel.prefab은 DEPRECATED 접두사 후 Legacy/ 이동. FishCatalogToast는 기존대로 독립 유지(섹션 6.4 현행 유지). 세부 단계는 Phase Q-4a~Q-4f 참조.

5. [OPEN] (ARC-037) **GatheringCatalogData와 GatheringItemData 1:1 관계 유지 방안**: FishCatalogData와 FishData의 동일 이슈(-> see `docs/systems/fishing-architecture.md` Open Question 10). itemId 문자열 연결 vs SO 직접 참조. 구현 시 결정.

---

## Risks

1. [RISK] **(ARC-037) SaveLoadOrder 56 밀집**: FishCatalogManager(53), GatheringManager(54), InventoryManager(55), GatheringCatalogManager(56)으로 53~56이 연속 배치된다. BuildingManager(60)까지 3칸 여유가 있으나, 향후 수집 시스템(예: 요리 도감, 곤충 도감)이 추가되면 간격이 부족할 수 있다.

2. [RISK] **(ARC-037) GatheringCatalogData와 GatheringItemData 동기화**: 두 SO가 동일한 itemId를 공유하므로, 채집 아이템 추가/삭제 시 양쪽 SO를 반드시 동시에 관리해야 한다. 한쪽만 갱신하면 도감에 빈 항목이 표시되거나 누락될 수 있다. (FishCatalogData-FishData 동기화 리스크와 동일, -> see fishing-architecture.md Risk 11)

3. [RISK] **(ARC-037) GatheringCatalogEntry.totalGathered와 GatheringStats.gatheredByItemId 중복**: 동일한 정보가 두 곳에 저장된다. 불일치 시 도감과 통계가 다른 값을 보여줄 수 있다. GatheringCatalogManager.HandleItemGathered에서만 도감 쪽을 갱신하고, GatheringManager.TryGather에서만 Stats 쪽을 갱신하므로 단일 진입점이 보장되지만, 향후 직접 갱신 경로가 추가되면 위험하다.

4. [RISK] **(ARC-037) 통합 UI 메모리**: 27종 채집 + 15종 어종 = 42개 항목의 프리팹 인스턴스를 동시에 풀링하면 메모리 부담이 될 수 있다. 현재 규모(42개)에서는 문제없으나, 향후 도감 카테고리가 확장되면 가상화 스크롤(RecyclerView 패턴) 도입을 고려해야 한다.

---

## FIX 태스크 제안 (TODO.md 추가 권고)

| FIX ID | 대상 문서 | 변경 내용 | 상태 |
|--------|----------|----------|------|
| FIX-093 | `save-load-architecture.md` | 섹션 7 SaveLoadOrder 할당표에 `GatheringCatalogManager \| 56 \| 채집 도감 상태 복원` 행 추가 | PENDING |
| FIX-094 | `data-pipeline.md` | Part I GameSaveData에 `GatheringCatalogSaveData gatheringCatalog` 필드 추가 | PENDING |
| FIX-095 | `project-structure.md` | 섹션 2 폴더 구조에 `Scripts/Collection/` 추가, 섹션 3 네임스페이스에 `SeedMind.Collection` 추가, 섹션 4 asmdef에 `SeedMind.Collection.asmdef` 추가 | PENDING |
