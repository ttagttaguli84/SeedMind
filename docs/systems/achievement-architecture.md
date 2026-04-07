# 도전 과제/업적 시스템 기술 아키텍처

> AchievementManager 싱글턴, AchievementData ScriptableObject, AchievementRecord 런타임 진행도, 이벤트 기반 조건 추적, 세이브/로드 통합, UI 구조, MCP 구현 태스크 요약  
> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: ARC-017

---

## Context

이 문서는 SeedMind의 도전 과제/업적(Achievement) 시스템에 대한 **기술 아키텍처 문서**이다. 업적 시스템은 플레이어의 장기적 목표 달성을 추적하고, 달성 시 보상과 알림을 제공하여 게임 진행의 동기를 부여한다. 퀘스트 시스템이 구체적이고 순차적인 미션을 다루는 반면, 업적 시스템은 누적 통계 기반의 비순차적 달성 과제를 관리한다.

**설계 목표**:
- 업적 데이터를 ScriptableObject로 완전 분리하여, 코드 변경 없이 업적 추가/수정 가능
- 이벤트 기반 진행도 추적으로, 기존 시스템(Farm, Economy, Building, Quest 등)과 느슨한 결합 유지
- ISaveable 패턴을 통해 기존 세이브/로드 파이프라인에 자연스럽게 통합
- 숨겨진 업적(Hidden)을 포함한 카테고리 분류로 발견의 재미 제공

**본 문서가 canonical인 데이터**:
- AchievementManager 클래스 설계, API 시그니처
- AchievementData SO 필드 구조
- AchievementRecord 런타임 클래스 구조
- AchievementSaveData 직렬화 구조
- AchievementEvents 이벤트 허브 설계
- 이벤트 구독 매핑 (어떤 이벤트가 어떤 조건 타입을 추적하는지)
- AchievementCategory, AchievementConditionType enum 정의
- SaveLoadOrder 할당 값

**본 문서가 canonical이 아닌 데이터 (참조만)**:

| 데이터 종류 | 참조처 |
|------------|--------|
| 업적별 목표 수치, 보상 수치 (골드, XP, 칭호, 아이템) | `docs/content/achievements.md` (canonical) |
| SaveManager API, ISaveable 인터페이스 | `docs/systems/save-load-architecture.md` (ARC-011) |
| SaveLoadOrder 전체 할당표 | `docs/systems/save-load-architecture.md` 섹션 7 |
| 퀘스트 시스템 이벤트 | `docs/systems/quest-architecture.md` (ARC-013) |
| 인벤토리 시스템 API | `docs/systems/inventory-architecture.md` |
| 경제 시스템 이벤트 | `docs/systems/economy-architecture.md` |
| 진행/레벨 시스템 | `docs/systems/progression-architecture.md` |
| 프로젝트 폴더 구조, 네임스페이스 규칙 | `docs/systems/project-structure.md` |
| 시간/계절 시스템 | `docs/systems/time-season-architecture.md` |

---

# Part I -- 기술 설계

---

## 1. 개요

### 1.1 시스템 역할

업적 시스템은 플레이어의 누적 활동을 추적하고, 사전 정의된 목표에 도달하면 업적을 해금하여 보상을 지급하는 시스템이다. 퀘스트 시스템과 달리 수락/포기 개념이 없으며, 게임 시작 시점부터 모든 업적이 암묵적으로 추적된다.

### 1.2 아키텍처 전체 흐름

```
[게임 시스템들]                   [업적 시스템]                    [UI]
     │                                │                           │
     ├── FarmEvents                   │                           │
     │   .OnCropHarvested ────────▶ AchievementManager            │
     │                                │  UpdateProgress()         │
     ├── EconomyEvents                │       │                   │
     │   .OnSaleCompleted ────────▶   │  CheckCompletion()        │
     │                                │       │                   │
     ├── BuildingEvents               │  ┌────┴────┐              │
     │   .OnConstructionCompleted ─▶  │  │ 달성?   │              │
     │                                │  Yes      No              │
     ├── QuestEvents                  │  │                        │
     │   .OnQuestCompleted ────────▶  │  UnlockAchievement()      │
     │                                │  │                        │
     ├── ToolEvents                   │  ├─▶ 보상 지급             │
     │   .OnToolUpgraded ─────────▶   │  ├─▶ AchievementEvents    │
     │                                │  │   .OnAchievementUnlocked──▶ AchievementToastUI
     └── NPCEvents                    │  │                        │
         .OnNPCMet ────────────────▶  │  └─▶ 세이브 마크          │
                                      │                           │
                                 AchievementManager               │
                                      │                           │
                                 ┌────┴────┐                      │
                                 ▼         ▼                      │
                            보상 지급    상태 저장                  │
                            (EconomyManager,   (ISaveable)        │
                             ProgressionManager)                  │
```

### 1.3 퀘스트 시스템과의 차이점

| 측면 | 퀘스트 | 업적 |
|------|--------|------|
| 수락/포기 | 필요 | 불필요 (자동 추적) |
| 시간 제한 | 있음 (일일 목표, NPC 의뢰) | 없음 |
| 반복 | 일부 반복 가능 | 1회 달성 (영구) |
| 표시 | 활성 퀘스트 목록 | 전체 업적 목록 (미달성 포함) |
| 숨김 | 없음 | 숨겨진 업적 카테고리 |
| 데이터 | 복합 목표, 다중 보상 | 단일 조건, 단일 보상 |

### 1.4 설계 원칙

| 원칙 | 구현 |
|------|------|
| 데이터/로직 분리 | ScriptableObject(AchievementData)는 정의만, MonoBehaviour(AchievementManager)는 행동만 |
| 이벤트 기반 추적 | 직접 참조 대신 정적 이벤트 허브(FarmEvents, EconomyEvents 등) 구독 |
| 단일 책임 | AchievementManager = 진행도 관리 + 달성 판정, UI는 별도 컴포넌트 |
| 확장성 | 새 조건 타입 추가 시 AchievementConditionType enum + 이벤트 구독 핸들러 1개만 추가 |

---

## 2. 열거형 정의

### 2.1 AchievementType

```csharp
// illustrative
namespace SeedMind.Achievement
{
    public enum AchievementType
    {
        Single  = 0,   // 단일 달성 (1회 조건 충족 시 영구 해금)
        Tiered  = 1    // 단계형 달성 (Bronze → Silver → Gold 순차 해금)
    }
}
```

### 2.2 AchievementCategory

```csharp
// illustrative
namespace SeedMind.Achievement
{
    public enum AchievementCategory
    {
        Farming     = 0,   // 경작/수확 관련
        Economy     = 1,   // 골드/거래 관련
        Facility    = 2,   // 시설 건설/업그레이드 관련
        Tool        = 3,   // 도구 업그레이드 관련
        Explorer    = 4,   // 탐험/발견 관련
        Quest       = 5,   // 퀘스트 완료 관련
        Hidden      = 6,   // 숨겨진 업적 (달성 전 조건 비공개)
        Angler      = 7,   // 낚시 관련 (→ see docs/content/achievements.md 섹션 9)
        Gatherer    = 8    // 채집 관련 (→ see docs/content/achievements.md 섹션 9.5)
    }
}
```

### 2.3 AchievementConditionType

```csharp
// illustrative
namespace SeedMind.Achievement
{
    public enum AchievementConditionType
    {
        HarvestCount            = 0,   // 총 수확 횟수 (작물 무관)
        GoldEarned              = 1,   // 누적 골드 획득량
        BuildingCount           = 2,   // 건설한 시설 총 수
        ToolUpgradeCount        = 3,   // 도구 업그레이드 총 횟수
        NPCMet                  = 4,   // 만난 NPC 수
        QuestCompleted          = 5,   // 완료한 퀘스트 총 수
        SpecificCropHarvested   = 6,   // 특정 작물 수확 횟수
        GoldSpent               = 7,   // 누적 골드 지출량
        DaysPlayed              = 8,   // 게임 내 경과 일수
        SeasonCompleted         = 9,   // 완료한 계절 수
        SpecificBuildingBuilt   = 10,  // 특정 시설 건설 여부
        TotalItemsSold          = 11,  // 판매한 아이템 총 수
        QualityHarvestCount     = 12,  // 특정 품질 이상 수확 횟수
        ProcessingCount         = 13,  // 가공 완료 총 횟수
        PurchaseCount           = 14,  // 상점 구매 횟수 (targetId=""이면 전체, targetId=shopId이면 특정 상점)
        GatherCount             = 15,  // 채집 총 횟수 (아이템 무관)
        GatherSpeciesCollected  = 16,  // 채집으로 수집한 종류 수 (고유 itemId 수 — 도감 완성용)
        GatherSickleUpgraded    = 17,  // 채집 낫 업그레이드 단계 달성 (targetValue = 티어, 1=강화/2=전설)
        Custom                  = 99   // 숨겨진 업적 전용 복합 조건 (AchievementManager 내 하드코딩 핸들러)
        // Custom 조건 적용 업적: ach_hidden_01(비 오는 날 수확), ach_hidden_02(밤 시간대 활동),
        //                        ach_hidden_03(특정 도구만 30일), ach_hidden_04(잔액 0 판매),
        //                        ach_hidden_05(거대 작물 수확), ach_hidden_06(전 인벤토리 출하),
        //                        ach_hidden_07(통합 수집 마스터 — ach_fish_04 AND ach_gather_03 달성)
        // → see docs/systems/achievement-system.md 섹션 7.1
    }
}
```

---

## 3. 클래스 구조

### 3.1 클래스 책임 요약

| 클래스 | 유형 | 네임스페이스 | 책임 |
|--------|------|-------------|------|
| **AchievementManager** | MonoBehaviour (Singleton, ISaveable) | `SeedMind.Achievement` | 업적 진행도 관리, 달성 판정, 보상 지급, 세이브/로드 |
| **AchievementEvents** | static class | `SeedMind.Achievement` | 업적 관련 정적 이벤트 허브 |
| **AchievementData** | ScriptableObject | `SeedMind.Achievement.Data` | 업적 정적 정의 (조건, 보상, 카테고리) |
| **AchievementRecord** | Serializable class | `SeedMind.Achievement` | 런타임 진행도 (현재 수치, 달성 여부, 달성 날짜) |
| **AchievementSaveData** | Serializable class | `SeedMind.Achievement` | 세이브 데이터 구조 |
| **AchievementPanel** | MonoBehaviour | `SeedMind.UI` | 업적 목록 UI (Y키 토글) |
| **AchievementToastUI** | MonoBehaviour | `SeedMind.UI` | 달성 알림 토스트 (화면 상단, 4초 표시 / 숨김 업적 6초) |
| **AchievementItemUI** | MonoBehaviour | `SeedMind.UI` | 업적 목록 내 개별 항목 표시 |

### 3.2 AchievementManager

```
┌──────────────────────────────────────────────────────────────────┐
│       AchievementManager (MonoBehaviour, Singleton, ISaveable)    │
│──────────────────────────────────────────────────────────────────│
│  [설정 참조]                                                      │
│  - _allAchievements: AchievementData[]   (전체 업적 SO 배열)     │
│                                                                  │
│  [런타임 상태]                                                    │
│  - _records: Dictionary<string, AchievementRecord>               │
│  - _unlockedIds: HashSet<string>                                 │
│  - _achievementLookup: Dictionary<string, AchievementData>       │
│                                                                  │
│  [외부 참조 — Singleton 직접 접근]                               │
│  - EconomyManager.Instance (보상 골드 지급 시)                   │
│  - ProgressionManager.Instance (보상 XP 지급 시)                 │
│  - InventoryManager.Instance (보상 아이템 지급 시)               │
│  // 필드 직렬화 대신 Instance 직접 호출 방식 사용 (코드 참조 없음) │
│                                                                  │
│  [ISaveable]                                                     │
│  + SaveLoadOrder => 90                                           │
│  + GetSaveData(): object                                         │
│  + LoadSaveData(object data): void                               │
│                                                                  │
│  [공개 API]                                                       │
│  + Initialize(): void                                            │
│  + UpdateProgress(AchievementConditionType, int amount): void    │
│  + UpdateProgress(AchievementConditionType, string targetId,     │
│                   int amount): void                              │
│  + CheckCompletion(string achievementId): bool                   │
│  + UnlockAchievement(string achievementId): void                 │
│  + GetRecord(string achievementId): AchievementRecord            │
│  + GetAchievementsByCategory(AchievementCategory):               │
│       IReadOnlyList<AchievementData>                             │
│  + GetUnlockedAchievements(): IReadOnlyList<AchievementData>     │
│  + GetOverallProgress(): float   (달성률 0.0~1.0)               │
│  + IsUnlocked(string achievementId): bool                        │
│                                                                  │
│  [이벤트 구독] (→ see 섹션 5)                                    │
│  - FarmEvents.OnCropHarvested → HandleHarvest                    │
│  - EconomyEvents.OnSaleCompleted → HandleSale                    │
│  - EconomyEvents.OnGoldSpent → HandleGoldSpent                   │
│  - BuildingEvents.OnConstructionCompleted → HandleBuildingBuilt  │
│  - ToolEvents.OnToolUpgraded → HandleToolUpgrade                 │
│  - NPCEvents.OnNPCFirstMet → HandleNPCMet                       │
│  - QuestEvents.OnQuestCompleted → HandleQuestCompleted           │
│  - ProcessingEvents.OnProcessingCompleted → HandleProcessing     │
│  - TimeManager.OnDayChanged → HandleDayChanged                   │
│  - TimeManager.OnSeasonChanged → HandleSeasonChanged             │
│  - AchievementEvents.OnAchievementUnlocked → HandleAchievementChain │
│    // ach_hidden_07: ach_fish_04 AND ach_gather_03 모두 달성 시  │
│    // Custom(99) 핸들러 내에서 두 업적 달성 여부 확인 후 자동 해금 │
└──────────────────────────────────────────────────────────────────┘
```

**AchievementManager 주요 로직 흐름**:

```
Initialize()
├── 1) _allAchievements 배열에서 모든 AchievementData 로드
├── 2) _achievementLookup 딕셔너리 구성 (achievementId → AchievementData)
├── 3) 각 AchievementData에 대해 AchievementRecord 초기 생성 (progress=0, unlocked=false)
├── 4) 이벤트 구독 등록 (SubscribeAll)
└── 5) 로드된 세이브 데이터가 있으면 _records 덮어쓰기

UpdateProgress(conditionType, amount)
├── 1) _allAchievements 중 conditionType이 일치하는 업적 필터링
├── 2) 이미 달성된 업적은 스킵
├── 3) 해당 AchievementRecord.currentProgress += amount
├── 4) AchievementEvents.OnProgressUpdated 발행
├── 5) CheckCompletion 호출
└── 6) 달성 시 UnlockAchievement 호출

UpdateProgress(conditionType, targetId, amount)
├── 1) conditionType + targetId 모두 일치하는 업적 필터링
│      (SpecificCropHarvested, SpecificBuildingBuilt 등)
├── 2) 이하 동일 흐름
└── ...

UnlockAchievement(achievementId)
├── 1) _records에서 해당 레코드 조회
├── 2) record.isUnlocked = true
├── 3) record.unlockedAt = TimeManager.CurrentGameDate
├── 4) _unlockedIds.Add(achievementId)
├── 5) 보상 지급 (GrantReward)
│      ├── Gold → EconomyManager.AddGold()
│      ├── XP → ProgressionManager.AddExp(xp, XPSource.AchievementReward)
│      │         // XP 수치 → see docs/content/achievements.md (canonical)
│      └── Item → InventoryManager.TryAddItem()
├── 6) AchievementEvents.OnAchievementUnlocked 발행
└── 7) Debug.Log로 달성 기록 (MCP 테스트용)

HandleAchievementChain(AchievementData unlockedAchievement)
├── 1) ach_hidden_07 이미 달성 여부 확인: IsUnlocked("ach_hidden_07") → true이면 즉시 return
├── 2) 트리거 업적 확인: unlockedAchievement.achievementId가
│      "ach_fish_04" 또는 "ach_gather_03"이 아니면 return
├── 3) 양쪽 조건 확인: IsUnlocked("ach_fish_04") && IsUnlocked("ach_gather_03")
└── 4) 조건 충족 시: UnlockAchievement("ach_hidden_07") 호출
       // 주의: UnlockAchievement 내부에서 OnAchievementUnlocked 재발행
       //       → HandleAchievementChain 재진입되지만
       //       1)번 가드(IsUnlocked 확인)로 무한 루프 차단됨
       // 보상 수치 → see docs/content/achievements.md (canonical)
```

### 3.3 AchievementEvents

```csharp
// illustrative
namespace SeedMind.Achievement
{
    /// <summary>
    /// 업적 시스템 정적 이벤트 허브.
    /// 다른 시스템 및 UI가 업적 변동을 구독할 수 있다.
    /// </summary>
    public static class AchievementEvents
    {
        /// <summary>업적 달성 시 발행. UI 토스트 트리거용.</summary>
        public static event System.Action<AchievementData> OnAchievementUnlocked;

        /// <summary>업적 진행도 갱신 시 발행. UI 프로그레스 바 갱신용.</summary>
        public static event System.Action<string, float> OnProgressUpdated;
        // string = achievementId, float = normalizedProgress (0.0~1.0)

        // --- 내부 발행 메서드 ---
        internal static void RaiseAchievementUnlocked(AchievementData data)
            => OnAchievementUnlocked?.Invoke(data);

        internal static void RaiseProgressUpdated(string id, float progress)
            => OnProgressUpdated?.Invoke(id, progress);
    }
}
```

---

## 4. ScriptableObject 설계

### 4.1 AchievementData (ScriptableObject)

업적의 정적 정의를 담는 SO. 각 업적마다 하나의 에셋을 생성한다.

```csharp
// illustrative
namespace SeedMind.Achievement.Data
{
    [CreateAssetMenu(fileName = "NewAchievementData", menuName = "SeedMind/AchievementData")]
    public class AchievementData : ScriptableObject
    {
        [Header("기본 정보")]
        public string achievementId;                // 고유 식별자 (예: "ach_harvest_100")
        public string displayName;                  // 표시 이름 (→ see 향후 docs/content/achievements.md)
        [TextArea(2, 4)]
        public string description;                  // 설명 텍스트 (→ see 향후 docs/content/achievements.md)
        public AchievementCategory category;        // → see 섹션 2.2
        public AchievementType type;                // → see 섹션 2.1 (Single=단일, Tiered=단계형)

        [Header("달성 조건 — Single 전용")]
        public AchievementConditionType conditionType;  // → see 섹션 2.3 (type=Tiered이면 tiers[].conditionType 사용)
        public string targetId;                     // 대상 ID (특정 작물/시설 등, ""이면 any)
        public int targetValue;                     // 목표 수치 (→ see 향후 docs/content/achievements.md)

        [Header("단계형 조건 — Tiered 전용")]
        public AchievementTierData[] tiers;         // Bronze[0], Silver[1], Gold[2] (→ see 섹션 4.5)

        [Header("보상 — Single 전용")]
        public AchievementRewardType rewardType;    // → see 섹션 4.2
        public int rewardAmount;                    // 보상 수량 (→ see docs/balance/progression-curve.md)
        public string rewardItemId;                 // 아이템 보상 시 아이템 ID ("" 이면 미사용)
        public string rewardTitleId;                // 칭호 보상 시 칭호 ID ("" 이면 미사용)

        [Header("표시")]
        public bool isHidden;                       // true이면 달성 전까지 조건 비공개
        public Sprite icon;                         // 업적 아이콘 (null이면 카테고리 기본 아이콘)
        public int sortOrder;                       // 카테고리 내 표시 순서
    }

    /// <summary>단계형 업적(Tiered)의 각 단계 데이터 (Bronze/Silver/Gold)</summary>
    [System.Serializable]
    public class AchievementTierData
    {
        public string tierName;                     // "Bronze" / "Silver" / "Gold"
        public AchievementConditionType conditionType;
        public string targetId;
        public int targetValue;                     // 이 단계의 달성 목표 수치 (→ see 향후 docs/content/achievements.md)
        public AchievementRewardType rewardType;
        public int rewardAmount;                    // (→ see docs/balance/progression-curve.md)
        public string rewardItemId;
        public string rewardTitleId;
    }
}
```

### 4.2 AchievementRewardType

```csharp
// illustrative
namespace SeedMind.Achievement
{
    public enum AchievementRewardType
    {
        None    = 0,   // 보상 없음 (달성 자체가 목적)
        Gold    = 1,   // 골드
        XP      = 2,   // 경험치
        Item    = 3,   // 아이템
        Title   = 4    // 칭호 (→ see docs/systems/achievement-system.md 섹션 4.2)
    }
}
```

### 4.3 PATTERN-005 검증: SO 필드 ↔ JSON 동기화

AchievementData의 C# 필드 구조와 아래 섹션 4.4의 JSON 스키마 간 필드 일치를 검증한다:

| C# 필드 | JSON 키 | 일치 |
|---------|---------|:----:|
| achievementId | achievementId | O |
| displayName | displayName | O |
| description | description | O |
| category | category | O |
| type | type | O |
| conditionType | conditionType | O |
| targetId | targetId | O |
| targetValue | targetValue | O |
| tiers | tiers | O |
| rewardType | rewardType | O |
| rewardAmount | rewardAmount | O |
| rewardItemId | rewardItemId | O |
| rewardTitleId | rewardTitleId | O |
| isHidden | isHidden | O |
| icon | (에디터 전용, 직렬화 제외) | - |
| sortOrder | sortOrder | O |

필드 수: C# 16개 (icon 포함) / JSON 15개 (icon 제외, 에디터 전용) — 일치

### 4.4 AchievementData JSON 스키마

```json
{
  "achievementId": "ach_harvest_100",
  "displayName": "풍요로운 농부",
  "description": "작물을 100번 수확하세요.",
  "category": "Farming",
  "type": "Single",
  "conditionType": "HarvestCount",
  "targetId": "",
  "targetValue": 100,
  "tiers": [],
  "rewardType": "Gold",
  "rewardAmount": 0,
  "rewardItemId": "",
  "rewardTitleId": "",
  "isHidden": false,
  "sortOrder": 1
}
```

> **[주의]** `targetValue`와 `rewardAmount`의 실제 수치는 향후 `docs/content/achievements.md`에서 정의한다. 위 JSON의 수치(100, 0)는 스키마 예시일 뿐이며 canonical이 아니다. (-> see PATTERN-006)

---

## 5. 이벤트 구독 매핑

AchievementManager가 구독하는 외부 이벤트와 그로 인해 갱신되는 AchievementConditionType의 매핑:

| 외부 이벤트 | 출처 시스템 | 갱신 대상 ConditionType | 핸들러 |
|------------|-----------|------------------------|--------|
| `FarmEvents.OnCropHarvested` | CropSystem | HarvestCount, SpecificCropHarvested, QualityHarvestCount | HandleHarvest |
| `EconomyEvents.OnSaleCompleted` | EconomyManager | GoldEarned, TotalItemsSold | HandleSale |
| `EconomyEvents.OnGoldSpent` | EconomyManager | GoldSpent | HandleGoldSpent |
| `BuildingEvents.OnConstructionCompleted` | BuildingManager | BuildingCount, SpecificBuildingBuilt | HandleBuildingBuilt |
| `ToolEvents.OnToolUpgraded` | ToolSystem | ToolUpgradeCount | HandleToolUpgrade |
| `NPCEvents.OnNPCFirstMet` | NPCManager | NPCMet | HandleNPCMet |
| `QuestEvents.OnQuestCompleted` | QuestManager | QuestCompleted | HandleQuestCompleted |
| `ProcessingEvents.OnProcessingCompleted` | ProcessingSystem | ProcessingCount | HandleProcessing |
| `EconomyEvents.OnShopPurchased` | EconomyManager / NPCManager | PurchaseCount | HandleShopPurchase |
| `GatheringEvents.OnItemGathered` | GatheringSystem | GatherCount, GatherSpeciesCollected | HandleGather |
| `GatheringEvents.OnSickleUpgraded` | GatheringSystem | GatherSickleUpgraded | HandleSickleUpgrade |
| `TimeManager.OnDayChanged` | TimeManager | DaysPlayed | HandleDayChanged |
| `TimeManager.OnSeasonChanged` | TimeManager | SeasonCompleted | HandleSeasonChanged |
| `AchievementEvents.OnAchievementUnlocked` | AchievementManager (자기 자신) | Custom(99) — ach_hidden_07 연쇄 해금 | HandleAchievementChain |

**핸들러 구현 패턴** (공통):

```csharp
// illustrative
private void HandleHarvest(CropHarvestInfo info)
{
    // 1. 범용 수확 카운트
    UpdateProgress(AchievementConditionType.HarvestCount, 1);

    // 2. 특정 작물 수확
    UpdateProgress(AchievementConditionType.SpecificCropHarvested, info.cropId, 1);

    // 3. 품질 수확 (minQuality 이상일 때)
    if (info.quality >= 2) // → see docs/systems/crop-growth.md for 품질 등급 정의
    {
        UpdateProgress(AchievementConditionType.QualityHarvestCount, 1);
    }
}

private void HandleSale(SellInfo info)
{
    UpdateProgress(AchievementConditionType.GoldEarned, info.totalGold);
    UpdateProgress(AchievementConditionType.TotalItemsSold, info.quantity);
}
```

---

## 6. 런타임 상태 클래스

### 6.1 AchievementRecord

```csharp
// illustrative
namespace SeedMind.Achievement
{
    /// <summary>
    /// 개별 업적의 런타임 진행 상태.
    /// AchievementManager가 Dictionary로 관리하며, 세이브/로드 시 직렬화된다.
    /// </summary>
    [System.Serializable]
    public class AchievementRecord
    {
        public string achievementId;       // AchievementData.achievementId 참조
        public int currentProgress;        // 현재 누적 진행도
        public bool isUnlocked;            // 달성 여부 (Single: 최종, Tiered: Gold 달성 여부)
        public int unlockedDay;            // 달성 시점의 게임 내 일차 (-1 = 미달성)
        public int unlockedSeason;         // 달성 시점의 계절 인덱스 (-1 = 미달성)
        public int unlockedYear;           // 달성 시점의 연도 (-1 = 미달성)
        public string currentTier;         // 단계형 전용: "None"/"Bronze"/"Silver"/"Gold" (Single이면 "")
        public List<TierUnlockRecord> tierHistory; // 단계형 전용: 각 단계 해금 기록 (Single이면 빈 리스트)

        public AchievementRecord(string id)
        {
            achievementId = id;
            currentProgress = 0;
            isUnlocked = false;
            unlockedDay = -1;
            unlockedSeason = -1;
            unlockedYear = -1;
            currentTier = "";
            tierHistory = new List<TierUnlockRecord>();
        }

        /// <summary>정규화된 진행도 (0.0 ~ 1.0)</summary>
        public float GetNormalizedProgress(int targetValue)
        {
            if (targetValue <= 0) return isUnlocked ? 1f : 0f;
            return Mathf.Clamp01((float)currentProgress / targetValue);
        }
    }

    /// <summary>단계형 업적의 특정 단계 해금 기록</summary>
    [System.Serializable]
    public class TierUnlockRecord
    {
        public string tier;            // "Bronze" / "Silver" / "Gold"
        public int unlockedDay;
        public int unlockedSeason;
        public int unlockedYear;
    }
}
```

### 6.2 PATTERN-005 검증: AchievementRecord 필드 ↔ JSON

| C# 필드 | JSON 키 | 일치 |
|---------|---------|:----:|
| achievementId | achievementId | O |
| currentProgress | currentProgress | O |
| isUnlocked | isUnlocked | O |
| unlockedDay | unlockedDay | O |
| unlockedSeason | unlockedSeason | O |
| unlockedYear | unlockedYear | O |
| currentTier | currentTier | O |
| tierHistory | tierHistory | O |

필드 수: 8개 — 양쪽 일치

---

## 7. 세이브/로드 연동

### 7.1 SaveLoadOrder 할당

(-> see `docs/systems/save-load-architecture.md` 섹션 7 for 전체 할당표)

| 시스템 | SaveLoadOrder | 근거 |
|--------|:------------:|------|
| AchievementManager | **90** | QuestManager(85) 이후. 퀘스트 완료 상태를 참조하여 QuestCompleted 조건 업적의 진행도를 정확히 복원 |

**복원 순서 의존성**: AchievementManager는 다른 시스템의 상태를 직접 참조하지 않으나, QuestManager가 먼저 복원되어야 `QuestCompleted` 조건 타입의 업적 진행도를 검증할 수 있다. 또한 ProgressionManager(70)와 EconomyManager(30)가 먼저 복원되어야 보상 지급 시스템이 정상 작동한다.

### 7.2 AchievementSaveData

```csharp
// illustrative
namespace SeedMind.Achievement
{
    [System.Serializable]
    public class AchievementSaveData
    {
        public List<AchievementRecord> records;   // 전체 업적 진행 기록
        public int totalUnlocked;                  // 달성한 업적 총 수 (빠른 조회용 캐시)
    }
}
```

**Dictionary → List 변환 전략**:

AchievementManager 내부에서는 `Dictionary<string, AchievementRecord>`로 O(1) 조회를 사용하지만, 직렬화 시에는 `List<AchievementRecord>`로 변환한다. Newtonsoft.Json이 Dictionary를 지원하지만, 기존 세이브 시스템의 패턴(-> see `docs/systems/save-load-architecture.md` 섹션 2)을 따라 List 기반 직렬화를 채택한다.

```csharp
// illustrative
// AchievementManager.cs 내부
public int SaveLoadOrder => 90; // → see 섹션 7.1

public object GetSaveData()
{
    return new AchievementSaveData
    {
        records = new List<AchievementRecord>(_records.Values),
        totalUnlocked = _unlockedIds.Count
    };
}

public void LoadSaveData(object data)
{
    if (data is not AchievementSaveData saveData) return;

    _records.Clear();
    _unlockedIds.Clear();

    foreach (var record in saveData.records)
    {
        _records[record.achievementId] = record;
        if (record.isUnlocked)
        {
            _unlockedIds.Add(record.achievementId);
        }
    }
}
```

### 7.3 GameSaveData 통합

GameSaveData 루트 클래스에 `AchievementSaveData` 필드를 추가한다:

```
GameSaveData (루트)
├── ... (기존 필드들) (→ see docs/systems/save-load-architecture.md 섹션 2.1)
│
└── AchievementSaveData               # (NEW)
    ├── records[]                      # AchievementRecord[]
    └── totalUnlocked                  # int
```

### 7.4 AchievementSaveData JSON 스키마 (PATTERN-005)

```json
{
  "achievements": {
    "records": [
      {
        "achievementId": "ach_harvest_100",
        "currentProgress": 47,
        "isUnlocked": false,
        "unlockedDay": -1,
        "unlockedSeason": -1,
        "unlockedYear": -1,
        "currentTier": "",
        "tierHistory": []
      },
      {
        "achievementId": "ach_first_sale",
        "currentProgress": 1,
        "isUnlocked": true,
        "unlockedDay": 3,
        "unlockedSeason": 0,
        "unlockedYear": 1,
        "currentTier": "",
        "tierHistory": []
      }
    ],
    "totalUnlocked": 1
  }
}
```

---

## 8. UI 구조

### 8.1 AchievementPanel

```
┌──────────────────────────────────────────────────────────────────┐
│              AchievementPanel (MonoBehaviour)                      │
│──────────────────────────────────────────────────────────────────│
│  [참조]                                                           │
│  - _achievementManager: AchievementManager                       │
│  - _contentParent: Transform        (목록 아이템 부모)            │
│  - _itemPrefab: AchievementItemUI   (목록 항목 프리팹)            │
│  - _categoryTabs: Button[]          (카테고리 탭 버튼)            │
│  - _progressText: TMP_Text          (전체 달성률 "12/30" — → see docs/systems/achievement-system.md 섹션 1)        │
│                                                                  │
│  [상태]                                                           │
│  - _isOpen: bool                                                 │
│  - _currentCategory: AchievementCategory                         │
│  - _itemPool: List<AchievementItemUI>   (오브젝트 풀)            │
│                                                                  │
│  [메서드]                                                         │
│  + Toggle(): void              (Y키 입력으로 호출)                │
│  + Open(): void                                                  │
│  + Close(): void                                                 │
│  + SetCategory(AchievementCategory): void                        │
│  - RefreshList(): void         (현재 카테고리 업적 표시)          │
│  - UpdateProgressText(): void  (전체 달성률 갱신)                 │
│                                                                  │
│  [입력]                                                           │
│  - Y키 → Toggle()                                                │
│  - ESC키 → Close()                                                │
└──────────────────────────────────────────────────────────────────┘
```

### 8.2 AchievementToastUI

```
┌──────────────────────────────────────────────────────────────────┐
│              AchievementToastUI (MonoBehaviour)                    │
│──────────────────────────────────────────────────────────────────│
│  [참조]                                                           │
│  - _iconImage: Image                                             │
│  - _titleText: TMP_Text                                          │
│  - _descriptionText: TMP_Text                                    │
│  - _animator: Animator           (슬라이드 인/아웃 애니메이션)    │
│                                                                  │
│  [설정]                                                           │
│  - _displayDuration: float = 4f  // → see docs/systems/achievement-system.md 섹션 5.4 (숨김 업적: 6f)      │
│  - _slideInDuration: float = 0.3f                                │
│  - _slideOutDuration: float = 0.3f                               │
│                                                                  │
│  [큐 시스템]                                                      │
│  - _toastQueue: Queue<AchievementData>                           │
│  - _isShowing: bool                                              │
│                                                                  │
│  [메서드]                                                         │
│  + ShowToast(AchievementData): void                              │
│  - ProcessQueue(): IEnumerator                                   │
│  - DisplaySingle(AchievementData): IEnumerator                   │
│                                                                  │
│  [이벤트 구독]                                                    │
│  - AchievementEvents.OnAchievementUnlocked → ShowToast           │
└──────────────────────────────────────────────────────────────────┘
```

**토스트 표시 흐름**:
```
AchievementEvents.OnAchievementUnlocked
├── 1) _toastQueue에 AchievementData 추가
├── 2) _isShowing == false이면 ProcessQueue 코루틴 시작
│      ├── 큐에서 Dequeue
│      ├── 아이콘/제목/설명 세팅
│      ├── 슬라이드 인 (0.3초)
│      ├── 대기 (4초 / 숨김 업적 6초) (→ see docs/systems/achievement-system.md 섹션 5.4)
│      ├── 슬라이드 아웃 (0.3초)
│      └── 큐에 남은 항목이 있으면 반복
└── 3) 큐 비었으면 _isShowing = false
```

### 8.3 AchievementItemUI

```
┌──────────────────────────────────────────────────────────────────┐
│              AchievementItemUI (MonoBehaviour)                     │
│──────────────────────────────────────────────────────────────────│
│  [참조]                                                           │
│  - _iconImage: Image                                             │
│  - _titleText: TMP_Text                                          │
│  - _descriptionText: TMP_Text                                    │
│  - _progressBar: Slider                                          │
│  - _progressText: TMP_Text       ("47/100")                     │
│  - _completedOverlay: GameObject  (달성 시 체크마크 오버레이)     │
│  - _hiddenOverlay: GameObject     (숨겨진 업적 미달성 시 "???")  │
│                                                                  │
│  [메서드]                                                         │
│  + Setup(AchievementData, AchievementRecord): void               │
│  - SetUnlockedState(): void      (달성 완료 표시)                │
│  - SetProgressState(): void      (진행 중 표시)                  │
│  - SetHiddenState(): void        (숨겨진 업적 미달성 표시)       │
│  - SetLockedState(): void        (일반 미달성 표시)              │
└──────────────────────────────────────────────────────────────────┘
```

**표시 규칙**:

| 상태 | isHidden | isUnlocked | 표시 |
|------|:--------:|:----------:|------|
| 달성 완료 | false | true | 아이콘 + 제목 + 설명 + 체크마크 |
| 달성 완료 (숨김) | true | true | 아이콘 + 제목 + 설명 + 체크마크 (달성 후 공개) |
| 진행 중 | false | false | 아이콘 + 제목 + 설명 + 진행 바 |
| 숨김 미달성 | true | false | "???" + "숨겨진 업적" + 진행 바 숨김 |

### 8.4 씬 계층 구조

```
Canvas_Overlay (기존)
└── AchievementLayer
    ├── AchievementPanel          # AchievementPanel.cs
    │   ├── Header
    │   │   ├── TitleText         # "업적"
    │   │   ├── ProgressText      # "12/30 달성" (→ see docs/systems/achievement-system.md 섹션 1)
    │   │   └── CloseButton
    │   ├── CategoryTabs
    │   │   ├── Tab_Farming
    │   │   ├── Tab_Economy
    │   │   ├── Tab_Facility
    │   │   ├── Tab_Tool
    │   │   ├── Tab_Explorer
    │   │   ├── Tab_Quest
    │   │   ├── Tab_Hidden        # 숨겨진 업적 (달성 전 "???" 표시)
    │   │   └── Tab_All           # 전체 보기
    │   └── ScrollView
    │       └── Content           # AchievementItemUI 인스턴스 부모
    │
    └── AchievementToast          # AchievementToastUI.cs
        ├── IconImage
        ├── TitleText             # "업적 달성!"
        └── DescriptionText       # "풍요로운 농부 — 작물을 100번 수확했습니다."
```

---

## 9. 프로젝트 폴더 구조

(-> see `docs/systems/project-structure.md` for 전체 구조)

```
Assets/_Project/
├── Scripts/
│   └── Achievement/                         # 네임스페이스: SeedMind.Achievement
│       ├── AchievementManager.cs            # 싱글턴, ISaveable
│       ├── AchievementEvents.cs             # 정적 이벤트 허브
│       ├── AchievementRecord.cs             # [Serializable] 런타임 진행도
│       ├── AchievementSaveData.cs           # [Serializable] 세이브 구조
│       └── Data/                            # 네임스페이스: SeedMind.Achievement.Data
│           ├── AchievementData.cs           # ScriptableObject
│           ├── AchievementCategory.cs       # enum
│           ├── AchievementConditionType.cs  # enum
│           └── AchievementRewardType.cs     # enum
│
├── Data/
│   └── Achievements/                        # AchievementData SO 에셋
│       ├── Farming/                         # 경작 카테고리
│       ├── Economy/                         # 경제 카테고리
│       ├── Facility/                        # 시설 카테고리
│       ├── Tool/                            # 도구 카테고리
│       ├── Explorer/                        # 탐험 카테고리
│       ├── Quest/                           # 퀘스트 카테고리
│       ├── Hidden/                          # 숨겨진 업적
│       ├── Angler/                          # 낚시 카테고리 (CON-010)
│       └── Gatherer/                        # 채집 카테고리 (CON-013, ARC-035)
│
└── Prefabs/
    └── UI/
        ├── AchievementPanel.prefab
        ├── AchievementToast.prefab
        └── AchievementItemUI.prefab
```

---

## 10. 설계 결정 기록

### 10.1 업적 조건을 단일 conditionType + targetValue로 단순화

| 대안 | 장점 | 단점 | 결정 |
|------|------|------|:----:|
| 퀘스트처럼 다중 목표(ObjectiveData[]) | 복합 조건 가능 | 업적은 단순해야 함, SO 구조 과잉 복잡 | 기각 |
| **단일 conditionType + targetValue** | 단순, SO 생성 용이, 이해 쉬움 | 복합 조건 불가 | **채택** |

**근거**: 업적은 "작물 100번 수확", "골드 10000 달성" 등 단일 조건이 대부분이다. 복합 조건이 필요한 경우 퀘스트 시스템의 Composite 목표를 활용하거나, 별도의 Hidden 업적으로 분리한다.

### 10.2 진행도 추적 방식: 누적 카운터

| 대안 | 장점 | 단점 | 결정 |
|------|------|------|:----:|
| 이벤트 발생 시 조건 재계산 | 정확함 | 매번 전체 데이터 스캔 필요, 성능 부담 | 기각 |
| **누적 카운터 (currentProgress)** | O(1) 갱신, 단순 | 게임 중간 참여 시 과거 누적 미반영 | **채택** |

**근거**: 게임 시작부터 추적하므로 과거 누적 미반영 문제가 없다. 세이브/로드 시에도 카운터만 저장하면 되어 직렬화가 단순하다.

### 10.3 SaveLoadOrder = 90 선택

QuestManager(85) 이후로 배치하여 퀘스트 완료 상태가 먼저 복원된 상태에서 업적 진행도를 복원한다. 업적 시스템은 다른 시스템의 런타임 상태에 직접 의존하지 않으나, 로드 후 초기 정합성 검증(예: "퀘스트 10개 완료" 업적의 진행도와 실제 완료 퀘스트 수 일치 여부)을 위해 모든 시스템 이후에 로드되는 것이 안전하다.

---

# Part II -- MCP 태스크 시퀀스 요약

> 상세 MCP 호출 명세는 `docs/mcp/achievement-tasks.md`에 별도 작성 예정. 본 섹션은 개요만 제공한다.

---

## Step 1: 스크립트 생성

| 순서 | 파일 | 유형 | 네임스페이스 | 비고 |
|:----:|------|------|-------------|------|
| 1-1 | AchievementCategory.cs | enum | SeedMind.Achievement.Data | 9개 값 |
| 1-2 | AchievementConditionType.cs | enum | SeedMind.Achievement.Data | 19개 값 |
| 1-3 | AchievementRewardType.cs | enum | SeedMind.Achievement | 5개 값 |
| 1-4 | AchievementRecord.cs | [Serializable] class | SeedMind.Achievement | 8필드 |
| 1-5 | AchievementSaveData.cs | [Serializable] class | SeedMind.Achievement | 2필드 |
| 1-6 | AchievementData.cs | ScriptableObject | SeedMind.Achievement.Data | 13필드 |
| 1-7 | AchievementEvents.cs | static class | SeedMind.Achievement | 2이벤트 |
| 1-8 | AchievementManager.cs | MonoBehaviour, Singleton, ISaveable | SeedMind.Achievement | 핵심 시스템 |
| 1-9 | AchievementPanel.cs | MonoBehaviour | SeedMind.UI | Y키 토글 |
| 1-10 | AchievementToastUI.cs | MonoBehaviour | SeedMind.UI | 달성 알림 |
| 1-11 | AchievementItemUI.cs | MonoBehaviour | SeedMind.UI | 목록 항목 |

**의존 순서**: 1-1~1-3 (enum) → 1-4~1-6 (데이터) → 1-7 (이벤트) → 1-8 (매니저) → 1-9~1-11 (UI)

## Step 2: SO 에셋 생성

카테고리별 AchievementData SO 에셋을 생성한다. 구체적 업적 목록과 수치는 `docs/content/achievements.md` (향후 작성)에서 정의한다.

- `Data/Achievements/Farming/` — 경작 카테고리 업적 에셋
- `Data/Achievements/Economy/` — 경제 카테고리 업적 에셋
- `Data/Achievements/Facility/` — 시설 카테고리 업적 에셋
- `Data/Achievements/Tool/` — 도구 카테고리 업적 에셋
- `Data/Achievements/Explorer/` — 탐험 카테고리 업적 에셋
- `Data/Achievements/Quest/` — 퀘스트 카테고리 업적 에셋
- `Data/Achievements/Hidden/` — 숨겨진 업적 에셋
- `Data/Achievements/Angler/` — 낚시 카테고리 업적 에셋 (CON-010, → see 섹션 9 폴더 구조)
- `Data/Achievements/Gatherer/` — 채집 카테고리 업적 에셋 (CON-013, ARC-035, → see 섹션 9 폴더 구조)

(-> see `docs/content/achievements.md` for 업적 목록 및 수치)

## Step 3: UI 프리팹 구성

| 프리팹 | 위치 | 구성 요소 |
|--------|------|-----------|
| AchievementPanel.prefab | Prefabs/UI/ | Panel + CategoryTabs + ScrollView + Header |
| AchievementToast.prefab | Prefabs/UI/ | 상단 슬라이드 배너 + Icon + Text |
| AchievementItemUI.prefab | Prefabs/UI/ | Icon + Title + Description + ProgressBar + Overlays |

## Step 4: 씬 배치 및 연결

1. `SCN_Farm` 씬의 `Canvas_Overlay`에 `AchievementLayer` 생성
2. AchievementPanel, AchievementToast 프리팹 인스턴스 배치
3. `_Systems` 루트에 `AchievementManager` 컴포넌트 추가
4. AchievementManager의 `_allAchievements` 배열에 SO 에셋 연결
5. SaveManager의 ISaveable 레지스트리에 AchievementManager 등록
6. InputManager에 Y키 → AchievementPanel.Toggle() 바인딩 추가

## Step 5: 통합 테스트

| 테스트 | 검증 내용 | MCP 방법 |
|--------|-----------|----------|
| 진행도 갱신 | FarmEvents.OnCropHarvested 발행 → HarvestCount 업적 진행도 +1 | Console.Log 확인 |
| 달성 판정 | targetValue 도달 시 isUnlocked = true | Console.Log 확인 |
| 보상 지급 | UnlockAchievement → EconomyManager.AddGold 호출 | 골드 잔액 확인 |
| 토스트 UI | OnAchievementUnlocked 발행 → 토스트 표시 | 화면 캡처 |
| 세이브/로드 | 저장 후 로드 → 진행도/달성 상태 유지 | JSON 파일 내용 확인 |
| 숨겨진 업적 | isHidden=true → "???" 표시, 달성 후 공개 | UI 확인 |

---

## Cross-references

- `docs/systems/save-load-architecture.md` (ARC-011) -- ISaveable 인터페이스, SaveLoadOrder 할당표, GameSaveData 루트 구조
- `docs/systems/quest-architecture.md` (ARC-013) -- QuestManager, QuestEvents (퀘스트 완료 이벤트 구독 대상)
- `docs/systems/inventory-architecture.md` -- InventoryManager API (아이템 보상 지급)
- `docs/systems/economy-architecture.md` -- EconomyManager, EconomyEvents (골드 보상, 판매 이벤트)
- `docs/systems/progression-architecture.md` -- ProgressionManager (XP 보상 지급)
- `docs/systems/farming-system.md` -- FarmEvents (수확 이벤트)
- `docs/systems/facilities-architecture.md` (ARC-007) -- BuildingManager, BuildingEvents (건설 이벤트)
- `docs/systems/processing-architecture.md` (ARC-012) -- ProcessingEvents (가공 완료 이벤트)
- `docs/systems/time-season-architecture.md` -- TimeManager (일/계절 변경 이벤트)
- `docs/pipeline/data-pipeline.md` (ARC-004) -- GameDataSO 기반 클래스, DataRegistry
- `docs/systems/project-structure.md` -- 폴더 구조, 네임스페이스 규칙
- `docs/balance/progression-curve.md` -- 업적 보상 수치 참조
- `docs/systems/achievement-system.md` -- 업적 설계 원본 (이벤트 정의 7.1, 복합 조건 추적 7.2, 보상 규칙)
- `docs/mcp/achievement-tasks.md` -- MCP 상세 태스크 시퀀스 (별도 작성 예정)

---

## Open Questions

- ~~[OPEN] 업적 총 개수~~ — **RESOLVED**: `docs/content/achievements.md` 작성 완료, 30개 업적 확정 (→ see `docs/content/achievements.md` 섹션 2).
- ~~[OPEN] 업적 보상 수치~~ — **RESOLVED**: `docs/content/achievements.md` 섹션 2~9에 카테고리별 골드/XP 보상 canonical 기재 완료 (→ see `docs/content/achievements.md` 섹션 12.1).
- [OPEN] AchievementPanel 열기 단축키는 Y키로 결정 (→ see `docs/systems/achievement-system.md` 섹션 5.1). 전체 키바인딩 문서는 UI 시스템 설계 시 별도 작성 예정.

---

## Risks

- [RISK] **이벤트 구독 누락**: AchievementManager가 10개 이상의 외부 이벤트를 구독해야 한다. 하나라도 누락되면 해당 조건 타입의 업적이 동작하지 않는다. SubscribeAll()/UnsubscribeAll()에서 모든 이벤트를 명시적으로 나열하고, Step 5 통합 테스트에서 각 ConditionType별 검증이 필요하다.
- [RISK] **이벤트 페이로드 불일치**: 기존 시스템의 이벤트 페이로드에 AchievementManager가 필요로 하는 필드가 모두 포함되어 있는지 확인 필요. 특히 `EconomyEvents.OnGoldSpent`는 기존 경제 아키텍처에 정의되지 않았을 수 있으며, 추가 필요 시 해당 문서 업데이트를 수반한다.
- [RISK] **SaveLoadOrder 충돌**: AchievementManager(90)는 현재 할당표에서 마지막이다. 향후 다른 시스템이 85~95 범위에 할당되면 충돌 가능. 새 시스템 추가 시 `save-load-architecture.md`의 할당표를 반드시 갱신해야 한다.
- [RISK] **누적 카운터 정합성**: 세이브 파일이 손상되어 카운터가 실제보다 낮은 값으로 복원될 경우, 이미 달성 조건을 넘긴 업적이 미달성 상태로 남을 수 있다. 로드 후 검증 로직(전체 업적 재검증)을 고려할 수 있으나, 성능 비용과 트레이드오프가 있다.
- [RISK] **숨겨진 업적 스포일러**: AchievementData SO 에셋이 Unity 에디터에서 그대로 노출되므로, 빌드 후 에셋 추출을 통해 숨겨진 업적 내용이 유출될 수 있다. Phase 1에서는 수용하되, 향후 난독화 또는 서버 사이드 정의를 검토할 수 있다.
- [RISK] **NPCEvents.OnNPCFirstMet 이벤트 존재 여부**: 기존 NPC 아키텍처에 "최초 만남" 이벤트가 정의되어 있는지 확인 필요. 없으면 NPCManager에 추가해야 하며, `npc-shop-architecture.md` 업데이트를 수반한다.

---

*이 문서는 Claude Code가 자율적으로 작성했습니다.*
