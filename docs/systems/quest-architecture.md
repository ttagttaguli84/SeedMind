# 퀘스트/미션 시스템 기술 아키텍처

> QuestManager 싱글턴, QuestData/QuestObjectiveData/QuestRewardData ScriptableObject, QuestInstance 런타임 상태, 이벤트 기반 목표 추적, NPC 의뢰 연동, 세이브/로드 통합, MCP 구현 태스크 요약  
> 작성: Claude Code (Opus) | 2026-04-06  
> 문서 ID: ARC-013

---

## Context

이 문서는 SeedMind의 퀘스트/미션 시스템에 대한 **기술 아키텍처 문서**이다. 게임 디자인 문서(`docs/systems/quest-system.md`, DES-009)에서 정의한 4개 카테고리(메인 퀘스트, NPC 의뢰, 일일 목표, 농장 도전), 12종 목표 타입, 5종 보상 타입을 Unity 6 + MCP 환경에서 구현하기 위한 클래스 설계, 이벤트 아키텍처, 세이브/로드 통합을 기술한다.

**설계 목표**:
- 퀘스트 데이터를 ScriptableObject로 완전 분리하여, 코드 변경 없이 퀘스트 추가/수정 가능
- 이벤트 기반 목표 추적으로, 기존 시스템(Farm, Economy, Building 등)과 느슨한 결합 유지
- ISaveable 패턴을 통해 기존 세이브/로드 파이프라인에 자연스럽게 통합
- NPC 의뢰 시스템은 NPCManager/DialogueSystem과 이벤트로 연동

**본 문서가 canonical인 데이터**:
- QuestManager 클래스 설계, API 시그니처
- QuestData/QuestObjectiveData/QuestRewardData SO 필드 구조
- QuestInstance 런타임 클래스 구조
- QuestSaveData 직렬화 구조
- QuestEvents 이벤트 허브 설계
- 이벤트 구독 매핑 (어떤 이벤트가 어떤 목표 타입을 추적하는지)
- SaveLoadOrder 할당 값

**본 문서가 canonical이 아닌 데이터 (참조만)**:

| 데이터 종류 | 참조처 |
|------------|--------|
| 퀘스트 분류 체계, 목표 타입, 보상 타입, 상태 전이 | `docs/systems/quest-system.md` (DES-009) |
| 계절별 메인 퀘스트 목록, NPC 의뢰 목록, 일일 목표 풀 | `docs/systems/quest-system.md` 섹션 3~6 |
| 보상 수치 (골드, XP, 아이템) | `docs/systems/quest-system.md` 섹션 7 |
| 퀘스트 UI/UX 구조 | `docs/systems/quest-system.md` 섹션 8 |
| SaveManager API, ISaveable 인터페이스 | `docs/systems/save-load-architecture.md` (ARC-011) |
| SaveLoadOrder 전체 할당표 | `docs/systems/save-load-architecture.md` 섹션 7 |
| NPC 데이터 구조, NPCManager, DialogueSystem | `docs/systems/npc-shop-architecture.md` (ARC-008) |
| 인벤토리 시스템 API | `docs/systems/inventory-architecture.md` |
| 진행/레벨 시스템 | `docs/systems/progression-architecture.md` |
| 프로젝트 폴더 구조, 네임스페이스 규칙 | `docs/systems/project-structure.md` |

---

# Part I -- 기술 설계

---

## 1. 개요

### 1.1 시스템 역할

퀘스트 시스템은 게임 내 목표를 관리하고 추적하는 중앙 시스템이다. 다른 시스템들이 발행하는 이벤트를 구독하여 퀘스트 진행도를 자동 갱신하고, 목표 달성 시 보상을 지급한다.

### 1.2 아키텍처 전체 흐름

```
[게임 시스템들]                 [퀘스트 시스템]                  [UI]
     │                              │                           │
     ├── FarmEvents                 │                           │
     │   .OnCropHarvested ──────▶ QuestTracker ──▶ 진행도 갱신 ──▶ QuestUI
     │                              │                           │
     ├── EconomyEvents              │                           │
     │   .OnItemSold ───────────▶ QuestTracker ──▶ 진행도 갱신 ──▶ QuestUI
     │                              │                           │
     ├── BuildingEvents             │                           │
     │   .OnConstructionCompleted ▶ QuestTracker ──▶ 퀘스트 완료 ──▶ 완료 연출
     │                              │                           │
     └── ...                   QuestManager                     │
                                    │                           │
                               ┌────┴────┐                      │
                               ▼         ▼                      │
                          보상 지급    상태 전이                  │
                          (EconomyManager,   (Locked→Active     │
                           ProgressionManager, →Completed)      │
                           InventoryManager)                    │
```

### 1.3 설계 원칙

| 원칙 | 구현 |
|------|------|
| 데이터/로직 분리 | ScriptableObject(QuestData)는 정의만, MonoBehaviour(QuestManager)는 행동만 |
| 이벤트 기반 추적 | 직접 참조 대신 정적 이벤트 허브(FarmEvents, EconomyEvents 등) 구독 |
| 단일 책임 | QuestManager = 생명주기, QuestTracker = 진행도, QuestRewarder = 보상 |
| 확장성 | 새 목표 타입 추가 시 ObjectiveType enum + 이벤트 구독 핸들러 1개만 추가 (현재 14종) |

---

## 2. 열거형 정의

### 2.1 QuestCategory

(-> see `docs/systems/quest-system.md` 섹션 1 for 카테고리 정의)

```csharp
// illustrative
namespace SeedMind.Quest
{
    public enum QuestCategory
    {
        MainQuest        = 0,   // 계절별 메인 퀘스트
        NPCRequest       = 1,   // NPC 의뢰
        DailyChallenge   = 2,   // 일일 목표
        FarmChallenge    = 3    // 농장 도전
    }
}
```

### 2.2 QuestStatus

(-> see `docs/systems/quest-system.md` 섹션 2.5 for 상태 전이 정의)

```csharp
// illustrative
namespace SeedMind.Quest
{
    public enum QuestStatus
    {
        Locked      = 0,   // 해금 조건 미충족
        Available   = 1,   // 해금됨, 수락 대기
        Active      = 2,   // 진행 중
        Completed   = 3,   // 목표 달성, 보상 수령 대기
        Rewarded    = 4,   // 보상 수령 완료
        Failed      = 5,   // 시간 초과 실패
        Expired     = 6    // 만료 (일일 목표 자동 소멸)
    }
}
```

### 2.3 ObjectiveType

(-> see `docs/systems/quest-system.md` 섹션 2.2 for 목표 타입 정의)

```csharp
// illustrative
namespace SeedMind.Quest
{
    public enum ObjectiveType
    {
        Harvest         = 0,   // 작물 수확
        Sell            = 1,   // 아이템 판매
        Deliver         = 2,   // NPC에게 납품
        Process         = 3,   // 가공품 제작
        Build           = 4,   // 시설 건설
        EarnGold        = 5,   // 골드 획득
        Till            = 6,   // 경작지 생성
        Water           = 7,   // 물주기
        QualityHarvest  = 8,   // 특정 품질 이상 수확
        UpgradeTool     = 9,   // 도구 업그레이드
        ReachLevel      = 10,  // 레벨 도달
        Composite       = 11,  // 복합 (AND/OR)
        Fish            = 12,  // 낚시 (어종 포획)
        Gather          = 13   // 채집 (채집물 수집)
    }
}
```

### 2.4 RewardType

(-> see `docs/systems/quest-system.md` 섹션 2.3 for 보상 타입 정의)

```csharp
// illustrative
namespace SeedMind.Quest
{
    public enum RewardType
    {
        Gold            = 0,
        XP              = 1,
        Item            = 2,
        Recipe          = 3,   // 가공 레시피 해금
        Unlock          = 4    // 시설/기능 해금
    }
}
```

### 2.5 UnlockConditionType

(-> see `docs/systems/quest-system.md` 섹션 2.4 for 해금 조건 정의)

```csharp
// illustrative
namespace SeedMind.Quest
{
    public enum UnlockConditionType
    {
        Level              = 0,
        Season             = 1,
        QuestComplete      = 2,
        FacilityBuilt      = 3,
        DayOfSeason        = 4,
        TutorialComplete   = 5
    }
}
```

---

## 3. 클래스 구조

### 3.1 클래스 책임 요약

| 클래스 | 유형 | 네임스페이스 | 책임 |
|--------|------|-------------|------|
| **QuestManager** | MonoBehaviour (Singleton, ISaveable) | `SeedMind.Quest` | 퀘스트 생명주기 관리, 해금 판정, 상태 전이, 세이브/로드 |
| **QuestTracker** | 일반 C# 클래스 | `SeedMind.Quest` | 이벤트 구독, 목표 진행도 갱신, 완료 판정 |
| **QuestRewarder** | 일반 C# 클래스 | `SeedMind.Quest` | 보상 지급 처리 (EconomyManager, ProgressionManager, InventoryManager 호출) |
| **DailyQuestSelector** | 일반 C# 클래스 | `SeedMind.Quest` | 일일 목표 2개 랜덤 선택, 중복 방지 |
| **NPCRequestScheduler** | 일반 C# 클래스 | `SeedMind.Quest` | NPC 의뢰 등장/쿨다운 관리 |
| **QuestEvents** | static class | `SeedMind.Quest` | 퀘스트 관련 정적 이벤트 허브 |
| **QuestData** | ScriptableObject | `SeedMind.Quest.Data` | 퀘스트 정적 정의 |
| **QuestObjectiveData** | Serializable class (QuestData 내부) | `SeedMind.Quest.Data` | 개별 목표 정의 |
| **QuestRewardData** | Serializable class (QuestData 내부) | `SeedMind.Quest.Data` | 개별 보상 정의 |
| **QuestUnlockCondition** | Serializable class (QuestData 내부) | `SeedMind.Quest.Data` | 해금 조건 정의 |
| **QuestInstance** | 일반 C# 클래스 | `SeedMind.Quest` | 런타임 퀘스트 상태 (SO + 진행도) |
| **QuestSaveData** | Serializable class | `SeedMind.Quest` | 세이브 데이터 구조 |

### 3.2 QuestManager

```
┌──────────────────────────────────────────────────────────────────┐
│          QuestManager (MonoBehaviour, Singleton, ISaveable)       │
│──────────────────────────────────────────────────────────────────│
│  [설정 참조]                                                      │
│  - _allQuests: QuestData[]        (전체 퀘스트 SO 배열)           │
│  - _dailyQuestPool: QuestData[]   (일일 목표 풀 SO 배열)         │
│                                                                  │
│  [내부 컴포넌트]                                                   │
│  - _tracker: QuestTracker         (이벤트 구독, 진행도 갱신)      │
│  - _rewarder: QuestRewarder       (보상 지급)                    │
│  - _dailySelector: DailyQuestSelector   (일일 목표 선택)         │
│  - _npcScheduler: NPCRequestScheduler   (NPC 의뢰 관리)         │
│                                                                  │
│  [런타임 상태]                                                    │
│  - _activeQuests: Dictionary<string, QuestInstance>               │
│  - _completedQuestIds: HashSet<string>                           │
│  - _questHistory: Dictionary<string, QuestStatus>                │
│                                                                  │
│  [ISaveable]                                                     │
│  + SaveLoadOrder => 85                                           │
│  + GetSaveData(): object                                         │
│  + LoadSaveData(object data): void                               │
│                                                                  │
│  [공개 API]                                                       │
│  + Initialize(): void                                            │
│  + AcceptQuest(string questId): bool                             │
│  + AbandonQuest(string questId): bool                            │
│  + ClaimReward(string questId): bool                             │
│  + GetQuestsByCategory(QuestCategory): IReadOnlyList<QuestInstance>│
│  + GetActiveQuests(): IReadOnlyList<QuestInstance>                │
│  + IsQuestCompleted(string questId): bool                        │
│  + GetTrackedQuest(): QuestInstance                              │
│  + SetTrackedQuest(string questId): void                         │
│                                                                  │
│  [이벤트 구독]                                                    │
│  - TimeManager.OnDayChanged → CheckDailyReset, CheckTimeLimits  │
│  - TimeManager.OnSeasonChanged → UnlockSeasonalQuests            │
│  - ProgressionEvents.OnLevelUp → CheckLevelUnlocks              │
│  - TutorialEvents.OnTutorialCompleted → ActivateQuestSystem      │
│  - NPCEvents.OnRequestAccepted → AcceptNPCQuest                 │
└──────────────────────────────────────────────────────────────────┘
```

**QuestManager 주요 로직 흐름**:

```
Initialize()
├── 1) _allQuests 배열에서 모든 QuestData 로드
├── 2) 각 QuestData에 대해 QuestInstance 생성 (초기 상태: Locked)
├── 3) 해금 조건 검사 → 조건 충족 시 Available로 전환
├── 4) MainQuest/DailyChallenge/FarmChallenge는 Available → Active 자동 전환
├── 5) _tracker 초기화 (이벤트 구독 시작)
└── 6) _dailySelector.SelectDailyQuests() 호출 (첫 날)

OnDayChanged(newDay)
├── 1) 만료된 일일 목표 → Expired 처리
├── 2) _dailySelector.SelectDailyQuests() → 새 일일 목표 2개 Active
├── 3) NPC 의뢰 시간 제한 검사 → 초과 시 Failed 처리
├── 4) _npcScheduler.UpdateCooldowns() → 쿨다운 감소
└── 5) 해금 조건 재검사 (DayOfSeason 등)

OnSeasonChanged(newSeason)
├── 1) 해당 계절 메인 퀘스트 해금 (Season 조건)
└── 2) NPC 의뢰 풀 갱신 (계절 필터 적용)
```

### 3.3 QuestTracker

QuestTracker는 QuestManager의 내부 컴포넌트로, 게임 이벤트를 구독하여 활성 퀘스트의 진행도를 갱신한다.

```
┌──────────────────────────────────────────────────────────────────┐
│                  QuestTracker (일반 C# 클래스)                     │
│──────────────────────────────────────────────────────────────────│
│  [참조]                                                           │
│  - _manager: QuestManager                                        │
│                                                                  │
│  [메서드]                                                         │
│  + SubscribeAll(): void       (모든 관련 이벤트 구독)             │
│  + UnsubscribeAll(): void     (구독 해제)                        │
│  - OnCropHarvested(CropHarvestInfo): void                        │
│  - OnItemSold(SellInfo): void                                    │
│  - OnItemDelivered(DeliverInfo): void                            │
│  - OnItemProcessed(ProcessInfo): void                            │
│  - OnBuildingCompleted(string buildingId): void                  │
│  - OnGoldChanged(int delta): void                                │
│  - OnTileTilled(Vector2Int pos): void                            │
│  - OnCropWatered(Vector2Int pos): void                           │
│  - OnToolUpgraded(ToolUpgradeInfo): void                         │
│  - OnLevelReached(int newLevel): void                            │
│  - OnFishCaught(FishData, CropQuality): void                     │
│  - OnItemGathered(GatheringItemData, CropQuality, int): void     │
│  + UpdateObjective(ObjectiveType, string targetId, int delta): void│
│                                                                  │
│  [내부 로직]                                                      │
│  - CheckCompletion(QuestInstance): bool                           │
│  - HandleCompletion(QuestInstance): void                          │
└──────────────────────────────────────────────────────────────────┘
```

### 3.4 QuestRewarder

```
┌──────────────────────────────────────────────────────────────────┐
│                  QuestRewarder (일반 C# 클래스)                    │
│──────────────────────────────────────────────────────────────────│
│  [외부 참조]                                                      │
│  - _economyManager: EconomyManager                               │
│  - _progressionManager: ProgressionManager                       │
│  - _inventoryManager: InventoryManager                           │
│  - _unlockRegistry: UnlockRegistry                               │
│                                                                  │
│  [메서드]                                                         │
│  + GrantRewards(QuestData questData, int playerLevel): void      │
│  - GrantGold(int baseAmount, int playerLevel): void              │
│  - GrantXP(int baseAmount, int playerLevel): void                │
│  - GrantItem(string itemId, int count): void                     │
│  - GrantRecipe(string recipeId): void                            │
│  - GrantUnlock(string unlockId): void                            │
│  - ApplyLevelScale(int baseValue, int playerLevel): int          │
└──────────────────────────────────────────────────────────────────┘
```

**보상 레벨 스케일 공식**: (-> see `docs/systems/quest-system.md` 섹션 5.2 for 공식 정의)

```csharp
// illustrative
private int ApplyLevelScale(int baseValue, int playerLevel)
{
    // → see docs/systems/quest-system.md 섹션 5.2 for 스케일 공식
    float scale = 1f + (playerLevel - 1) * 0.1f; // → see canonical
    scale = Mathf.Min(scale, 1.9f);               // → see canonical (레벨 10 상한)
    return Mathf.RoundToInt(baseValue * scale);
}

private void GrantXP(int baseAmount, int playerLevel)
{
    // XP 보상은 ProgressionManager에 XPSource.QuestComplete 출처로 전달
    // XP 수치 → see docs/systems/quest-system.md 섹션 3~6 (canonical)
    int scaledXP = ApplyLevelScale(baseAmount, playerLevel);
    _progressionManager.AddExp(scaledXP, XPSource.QuestComplete);
}
```

---

## 4. ScriptableObject 설계

### 4.1 QuestData (ScriptableObject)

퀘스트의 정적 정의를 담는 SO. 각 퀘스트마다 하나의 에셋을 생성한다.

```csharp
// illustrative
namespace SeedMind.Quest.Data
{
    [CreateAssetMenu(fileName = "NewQuestData", menuName = "SeedMind/QuestData")]
    public class QuestData : ScriptableObject
    {
        [Header("기본 정보")]
        public string questId;                        // 고유 식별자 (예: "main_spring_01")
        public QuestCategory category;                // → see 섹션 2.1
        public string titleKR;                        // 한국어 제목 (→ see docs/systems/quest-system.md)
        [TextArea(2, 4)]
        public string descriptionKR;                  // 한국어 설명 (→ see docs/systems/quest-system.md)
        public string giverId;                        // 퀘스트 부여자 NPC ID ("system"이면 시스템 자동)

        [Header("목표")]
        public QuestObjectiveData[] objectives;       // 목표 배열 (1개 이상)

        [Header("보상")]
        public QuestRewardData[] rewards;             // 보상 배열

        [Header("해금 조건")]
        public QuestUnlockCondition[] unlockConditions; // 해금 조건 배열 (AND 로직)

        [Header("제한")]
        public int timeLimitDays;                     // 제한 시간 (0이면 무기한) (→ see docs/systems/quest-system.md 섹션 2.1)
        public Season season;                         // 연관 계절 (None이면 전 계절)
        public bool isRepeatable;                     // 반복 가능 여부

        [Header("UI")]
        public Sprite icon;                           // 퀘스트 아이콘 (null이면 카테고리 기본 아이콘)
    }
}
```

### 4.2 QuestObjectiveData (Serializable)

```csharp
// illustrative
namespace SeedMind.Quest.Data
{
    [System.Serializable]
    public class QuestObjectiveData
    {
        public ObjectiveType type;                    // → see 섹션 2.3
        public string targetId;                       // 대상 ID (작물ID, 시설ID, NPCID 등, ""이면 any)
        public int requiredAmount;                    // 목표 수량 (→ see docs/systems/quest-system.md)
        public int minQuality;                        // 최소 품질 등급 (QualityHarvest용, 0이면 무관)
        [TextArea(1, 2)]
        public string descriptionKR;                  // 목표 설명 텍스트

        // Composite 전용
        public CompositeMode compositeMode;           // AND / OR
        public QuestObjectiveData[] subObjectives;    // 하위 목표 (Composite일 때만 사용)
    }

    public enum CompositeMode
    {
        And = 0,
        Or  = 1
    }
}
```

### 4.3 QuestRewardData (Serializable)

```csharp
// illustrative
namespace SeedMind.Quest.Data
{
    [System.Serializable]
    public class QuestRewardData
    {
        public RewardType type;                       // → see 섹션 2.4
        public int amount;                            // 수량 (골드, XP, 아이템 개수) (→ see docs/systems/quest-system.md 섹션 7)
        public string targetId;                       // 대상 ID (아이템ID, 레시피ID, 해금ID; Gold/XP는 "")
        public bool scaledByLevel;                    // 레벨 스케일 적용 여부 (→ see docs/systems/quest-system.md 섹션 5.2)
    }
}
```

### 4.4 QuestUnlockCondition (Serializable)

```csharp
// illustrative
namespace SeedMind.Quest.Data
{
    [System.Serializable]
    public class QuestUnlockCondition
    {
        public UnlockConditionType type;              // → see 섹션 2.5
        public string stringParam;                    // 문자열 파라미터 (퀘스트ID, 시설ID 등)
        public int intParam;                          // 정수 파라미터 (레벨, 일차 등) (→ see docs/systems/quest-system.md 섹션 2.4)
        public Season seasonParam;                    // 계절 파라미터 (Season 조건용)
    }
}
```

### 4.5 PATTERN-005 검증: SO 필드 동기화

QuestData의 필드 구조와 아래 섹션 6.2의 JSON 스키마 간 필드 일치를 검증한다:

| C# 필드 | JSON 키 | 일치 |
|---------|---------|:----:|
| questId | questId | O |
| category | category | O |
| titleKR | titleKR | O |
| descriptionKR | descriptionKR | O |
| giverId | giverId | O |
| objectives[] | objectives[] | O |
| rewards[] | rewards[] | O |
| unlockConditions[] | unlockConditions[] | O |
| timeLimitDays | timeLimitDays | O |
| season | season | O |
| isRepeatable | isRepeatable | O |
| icon | (에디터 전용, 직렬화 제외) | - |

QuestObjectiveData 필드 수: 6개 (type, targetId, requiredAmount, minQuality, descriptionKR, compositeMode + subObjectives는 Composite 전용)  
QuestRewardData 필드 수: 4개 (type, amount, targetId, scaledByLevel)  
QuestUnlockCondition 필드 수: 4개 (type, stringParam, intParam, seasonParam)

---

## 5. 런타임 상태 클래스

### 5.1 QuestInstance

SO(정적 정의)와 런타임 진행 상태를 결합하는 래퍼 클래스.

```csharp
// illustrative
namespace SeedMind.Quest
{
    /// <summary>
    /// 런타임에서 활성 퀘스트의 상태를 관리한다.
    /// QuestData SO를 참조하면서 진행도를 추적한다.
    /// </summary>
    public class QuestInstance
    {
        // --- 정적 참조 ---
        public QuestData Data { get; private set; }

        // --- 런타임 상태 ---
        public QuestStatus Status { get; set; }
        public int[] ObjectiveProgress { get; private set; }   // 목표별 현재 진행도
        public int AcceptedDay { get; set; }                   // 수락한 게임 내 일차 (-1 = 미수락)
        public int CompletedDay { get; set; }                  // 완료한 게임 내 일차 (-1 = 미완료)
        public bool IsTracked { get; set; }                    // HUD 추적 여부

        // --- 생성자 ---
        public QuestInstance(QuestData data)
        {
            Data = data;
            Status = QuestStatus.Locked;
            ObjectiveProgress = new int[data.objectives.Length];
            AcceptedDay = -1;
            CompletedDay = -1;
            IsTracked = false;
        }

        // --- 진행도 API ---
        public void UpdateProgress(int objectiveIndex, int delta)
        {
            if (Status != QuestStatus.Active) return;
            ObjectiveProgress[objectiveIndex] += delta;
            int required = Data.objectives[objectiveIndex].requiredAmount;
            ObjectiveProgress[objectiveIndex] = Mathf.Min(ObjectiveProgress[objectiveIndex], required);
        }

        public bool IsObjectiveComplete(int objectiveIndex)
        {
            return ObjectiveProgress[objectiveIndex] >= Data.objectives[objectiveIndex].requiredAmount;
        }

        public bool AreAllObjectivesComplete()
        {
            for (int i = 0; i < ObjectiveProgress.Length; i++)
            {
                if (!IsObjectiveComplete(i)) return false;
            }
            return true;
        }

        public float GetOverallProgress()
        {
            if (Data.objectives.Length == 0) return 1f;
            float total = 0f;
            for (int i = 0; i < Data.objectives.Length; i++)
            {
                total += (float)ObjectiveProgress[i] / Data.objectives[i].requiredAmount;
            }
            return total / Data.objectives.Length;
        }

        // --- 시간 제한 ---
        public int GetRemainingDays(int currentDay)
        {
            if (Data.timeLimitDays <= 0) return -1;  // 무기한
            return Data.timeLimitDays - (currentDay - AcceptedDay);
        }

        public bool IsExpired(int currentDay)
        {
            if (Data.timeLimitDays <= 0) return false;
            return GetRemainingDays(currentDay) <= 0;
        }
    }
}
```

---

## 6. 이벤트 시스템 설계

### 6.1 이벤트 설계 방침: 정적 이벤트 허브 패턴

기존 SeedMind 아키텍처는 각 시스템별 정적 이벤트 허브(FarmEvents, ProgressionEvents, SaveEvents 등)를 사용한다 (-> see `docs/systems/project-structure.md` 섹션 3.4). 퀘스트 시스템도 동일한 패턴을 따른다.

**범용 EventBus 대신 정적 이벤트 허브를 선택한 근거**:
- 기존 시스템과의 일관성 유지
- 타입 안전성 (제네릭 EventBus는 런타임 타입 매칭 필요)
- 코드 추적 용이성 (IDE에서 이벤트 소스/구독자를 직접 검색 가능)

### 6.2 QuestEvents (정적 이벤트 허브)

```csharp
// illustrative
namespace SeedMind.Quest
{
    /// <summary>
    /// 퀘스트 시스템의 외부 발행 이벤트.
    /// UI 및 다른 시스템이 구독한다.
    /// </summary>
    public static class QuestEvents
    {
        // --- 상태 변경 ---
        public static event System.Action<QuestInstance> OnQuestUnlocked;        // Locked → Available
        public static event System.Action<QuestInstance> OnQuestActivated;       // Available → Active
        public static event System.Action<QuestInstance> OnQuestCompleted;       // Active → Completed
        public static event System.Action<QuestInstance> OnQuestRewarded;        // Completed → Rewarded
        public static event System.Action<QuestInstance> OnQuestFailed;          // Active → Failed/Expired

        // --- 진행도 ---
        public static event System.Action<QuestInstance, int> OnObjectiveProgress; // (quest, objectiveIndex)

        // --- 일일 목표 ---
        public static event System.Action<QuestInstance[]> OnDailyQuestsSelected; // 매일 아침 새 일일 목표

        // --- NPC 의뢰 ---
        public static event System.Action<QuestInstance> OnNPCRequestAvailable;   // 새 NPC 의뢰 등장

        // --- Raise 메서드 ---
        public static void RaiseQuestUnlocked(QuestInstance q) => OnQuestUnlocked?.Invoke(q);
        public static void RaiseQuestActivated(QuestInstance q) => OnQuestActivated?.Invoke(q);
        public static void RaiseQuestCompleted(QuestInstance q) => OnQuestCompleted?.Invoke(q);
        public static void RaiseQuestRewarded(QuestInstance q) => OnQuestRewarded?.Invoke(q);
        public static void RaiseQuestFailed(QuestInstance q) => OnQuestFailed?.Invoke(q);
        public static void RaiseObjectiveProgress(QuestInstance q, int idx) => OnObjectiveProgress?.Invoke(q, idx);
        public static void RaiseDailyQuestsSelected(QuestInstance[] quests) => OnDailyQuestsSelected?.Invoke(quests);
        public static void RaiseNPCRequestAvailable(QuestInstance q) => OnNPCRequestAvailable?.Invoke(q);
    }
}
```

### 6.3 이벤트 구독 매핑

QuestTracker가 구독하는 외부 이벤트와, 해당 이벤트가 추적하는 ObjectiveType의 매핑:

| 외부 이벤트 | 이벤트 소스 | 추적 ObjectiveType | 비고 |
|------------|-----------|-------------------|------|
| `FarmEvents.OnCropHarvested` | `SeedMind.Farm` | Harvest, QualityHarvest | CropHarvestInfo에 cropId, quality 포함 |
| `FarmEvents.OnTileTilled` | `SeedMind.Farm` | Till | 타일 좌표 전달 |
| `FarmEvents.OnCropWatered` | `SeedMind.Farm` | Water | 타일 좌표 전달 |
| `EconomyEvents.OnItemSold` | `SeedMind.Economy` | Sell, EarnGold | itemId, amount, goldEarned 포함 |
| `BuildingEvents.OnConstructionCompleted` | `SeedMind.Building` | Build | buildingId 전달 |
| `ProgressionEvents.OnLevelUp` | `SeedMind.Level` | ReachLevel | newLevel 전달 |
| `ToolUpgradeEvents.OnUpgradeCompleted` | `SeedMind.Player` | UpgradeTool | toolId, newTier 전달 |
| `ProcessingEvents.OnProcessingCompleted` | `SeedMind.Building` | Process | recipeId, outputId 전달 |
| `NPCEvents.OnItemDelivered` | `SeedMind.NPC` | Deliver | npcId, itemId, amount 전달 |
| `FishingEvents.OnFishCaught` | `SeedMind.Fishing` | Fish | FishData, CropQuality 전달 (-> see `docs/systems/fishing-architecture.md`) |
| `GatheringEvents.OnItemGathered` | `SeedMind.Gathering` | Gather | GatheringItemData, CropQuality, quantity 전달 (-> see `docs/systems/gathering-architecture.md`) |

[OPEN] `NPCEvents.OnItemDelivered` 이벤트가 아직 NPC 아키텍처에 정의되지 않았다. 납품 전용 이벤트를 NPC 시스템에 추가해야 한다.

### 6.4 QuestTracker 이벤트 핸들러 예시

```csharp
// illustrative
private void OnCropHarvested(CropHarvestInfo info)
{
    // Harvest 목표 갱신 (any 또는 특정 작물)
    UpdateObjective(ObjectiveType.Harvest, info.cropId, 1);

    // QualityHarvest 목표 갱신 (품질 필터 적용)
    // → see docs/systems/crop-growth.md 섹션 4.3 for 품질 등급 정의
    UpdateObjective(ObjectiveType.QualityHarvest, info.cropId, 1, info.quality);
}

private void OnItemSold(SellInfo info)
{
    UpdateObjective(ObjectiveType.Sell, info.itemId, info.amount);
    UpdateObjective(ObjectiveType.EarnGold, "", info.goldEarned);
}

private void OnFishCaught(FishData fish, CropQuality quality)
{
    // Fish 목표 갱신 (any 또는 특정 어종)
    // → see docs/systems/fishing-architecture.md for FishData 구조
    UpdateObjective(ObjectiveType.Fish, fish.fishId, 1);
}

private void OnItemGathered(GatheringItemData item, CropQuality quality, int quantity)
{
    // Gather 목표 갱신 (any 또는 특정 채집물)
    // → see docs/systems/gathering-architecture.md for GatheringItemData 구조
    UpdateObjective(ObjectiveType.Gather, item.itemId, quantity);
}

/// <summary>
/// 활성 퀘스트들의 목표 진행도를 갱신한다.
/// </summary>
public void UpdateObjective(ObjectiveType type, string targetId, int delta, int quality = 0)
{
    foreach (var quest in _manager.GetActiveQuests())
    {
        for (int i = 0; i < quest.Data.objectives.Length; i++)
        {
            var obj = quest.Data.objectives[i];
            if (obj.type != type) continue;
            if (!string.IsNullOrEmpty(obj.targetId) && obj.targetId != targetId) continue;
            if (obj.minQuality > 0 && quality < obj.minQuality) continue;

            quest.UpdateProgress(i, delta);
            QuestEvents.RaiseObjectiveProgress(quest, i);
        }

        if (quest.AreAllObjectivesComplete())
        {
            HandleCompletion(quest);
        }
    }
}
```

---

## 7. NPC 의뢰 연동 방식

### 7.1 NPCRequestScheduler

NPC 의뢰의 등장/쿨다운/만료를 관리하는 클래스.

```
┌──────────────────────────────────────────────────────────────────┐
│              NPCRequestScheduler (일반 C# 클래스)                 │
│──────────────────────────────────────────────────────────────────│
│  [참조]                                                           │
│  - _npcRequestPool: QuestData[]    (NPCRequest 카테고리 전체)    │
│  - _manager: QuestManager                                        │
│                                                                  │
│  [상태]                                                           │
│  - _npcCooldowns: Dictionary<string, int>   (NPC ID → 남은 쿨다운)│
│  - _activeRequestCount: int                                      │
│                                                                  │
│  [메서드]                                                         │
│  + TryOfferNewRequests(int currentDay, Season season, int level): void │
│  + UpdateCooldowns(): void                                       │
│  + GetAvailableRequests(string npcId): QuestData[]               │
│  + OnRequestCompleted(string questId): void                      │
│  + OnRequestFailed(string questId): void                         │
└──────────────────────────────────────────────────────────────────┘
```

### 7.2 NPC 대화 연동 흐름

```
[플레이어] ── E키 ──▶ [NPCController.Interact()]
    │                         │
    │                         ▼
    │                 [DialogueSystem.StartDialogue()]
    │                         │
    │                         ├── 인사 대화 진행
    │                         │
    │                         ▼
    │                 [서비스 메뉴 표시]
    │                    │
    │                    ├── "의뢰 확인" 선택
    │                    │       │
    │                    │       ▼
    │                    │  [QuestManager.GetAvailableNPCRequests(npcId)]
    │                    │       │
    │                    │       ├── 의뢰 있음 → 의뢰 대화 표시
    │                    │       │       │
    │                    │       │       ├── "수락" → QuestManager.AcceptQuest(questId)
    │                    │       │       └── "거절" → 대화 종료
    │                    │       │
    │                    │       └── 의뢰 없음 → "현재 의뢰가 없습니다" 대화
    │                    │
    │                    ├── "보상 수령" 선택 (완료된 의뢰 있을 때)
    │                    │       │
    │                    │       └── QuestManager.ClaimReward(questId) → 보상 대화
    │                    │
    │                    └── "상점" / 기타 서비스
```

### 7.3 의뢰 관련 규칙 (아키텍처 반영)

(-> see `docs/systems/quest-system.md` 섹션 4.1 for 의뢰 규칙 상세)

| 규칙 | 아키텍처 반영 |
|------|-------------|
| 동시 활성 최대 3개 | `NPCRequestScheduler._activeRequestCount` 로 제한 |
| 실패 시 쿨다운 후 재등장 | `_npcCooldowns` Dictionary로 NPC별 관리 |
| 계절 필터 | `TryOfferNewRequests()`에서 Season 파라미터로 필터링 |
| 바람이 체류 기간 연동 | NPCManager의 TravelingMerchantScheduler 상태 참조 |

---

## 8. SaveLoadOrder 및 ISaveable 구현

### 8.1 SaveLoadOrder 할당

(-> see `docs/systems/save-load-architecture.md` 섹션 7 for 전체 할당표)

| 시스템 | SaveLoadOrder | 근거 |
|--------|:------------:|------|
| QuestManager | **85** | TutorialManager(80) 이후, 퀘스트 해금 조건에 튜토리얼 완료 상태가 필요 |

**기존 할당표와의 관계**:
- TutorialManager = 80 (직전)
- QuestManager = 85 (신규)
- 퀘스트 로드 시 TimeManager(10), ProgressionManager(70), NPCManager(75), TutorialManager(80)의 데이터가 이미 복원된 상태를 전제

### 8.2 QuestSaveData 구조

#### JSON 스키마 (PATTERN-005 준수)

```json
{
  "questProgress": [
    {
      "questId": "main_spring_01",
      "status": 4,
      "objectiveProgress": [10, 3],
      "acceptedDay": 3,
      "completedDay": 8,
      "isTracked": false
    },
    {
      "questId": "daily_harvest_5",
      "status": 6,
      "objectiveProgress": [2],
      "acceptedDay": 15,
      "completedDay": -1,
      "isTracked": false
    }
  ],
  "completedQuestIds": ["main_spring_01", "fc_first_harvest"],
  "dailyState": {
    "lastSelectedDay": 15,
    "previousDailyIds": ["daily_water", "daily_harvest_5"],
    "todayDailyIds": ["daily_sell", "daily_earn"]
  },
  "npcRequestState": {
    "cooldowns": {
      "general_merchant": 3,
      "blacksmith": 0
    },
    "activeRequestCount": 1
  },
  "cumulativeStats": {
    "totalHarvested": 47,
    "totalSold": 1250,
    "totalProcessed": 5,
    "totalBuilt": 2
  }
}
```

#### C# 클래스 (PATTERN-005 준수)

```csharp
// illustrative
namespace SeedMind.Quest
{
    [System.Serializable]
    public class QuestSaveData
    {
        public QuestProgressEntry[] questProgress;          // 개별 퀘스트 진행도 배열
        public string[] completedQuestIds;                  // 완료된 퀘스트 ID 목록 (영구)
        public DailyQuestSaveState dailyState;              // 일일 목표 상태
        public NPCRequestSaveState npcRequestState;         // NPC 의뢰 상태
        public CumulativeStatsSaveData cumulativeStats;     // 누적 통계 (농장 도전 추적용)
    }

    [System.Serializable]
    public class QuestProgressEntry
    {
        public string questId;                              // SO의 questId와 매칭
        public int status;                                  // QuestStatus enum (int)
        public int[] objectiveProgress;                     // 목표별 현재 진행도
        public int acceptedDay;                             // 수락 일차 (-1 = 미수락)
        public int completedDay;                            // 완료 일차 (-1 = 미완료)
        public bool isTracked;                              // HUD 추적 여부
    }

    [System.Serializable]
    public class DailyQuestSaveState
    {
        public int lastSelectedDay;                         // 마지막 일일 목표 선택 일차
        public string[] previousDailyIds;                   // 전날 선택된 일일 목표 (중복 방지용)
        public string[] todayDailyIds;                      // 오늘 선택된 일일 목표
    }

    [System.Serializable]
    public class NPCRequestSaveState
    {
        public Dictionary<string, int> cooldowns;           // NPC ID → 남은 쿨다운 일수
        public int activeRequestCount;                      // 현재 활성 의뢰 수
    }

    [System.Serializable]
    public class CumulativeStatsSaveData
    {
        public int totalHarvested;                          // 누적 수확 수
        public int totalSold;                               // 누적 판매액
        public int totalProcessed;                          // 누적 가공 수
        public int totalBuilt;                              // 누적 건설 수
    }
}
```

**PATTERN-005 검증**: JSON 스키마와 C# 클래스의 필드 일치:
- QuestSaveData: 5개 필드 (questProgress, completedQuestIds, dailyState, npcRequestState, cumulativeStats) -- 양쪽 일치
- QuestProgressEntry: 6개 필드 (questId, status, objectiveProgress, acceptedDay, completedDay, isTracked) -- 양쪽 일치
- DailyQuestSaveState: 3개 필드 (lastSelectedDay, previousDailyIds, todayDailyIds) -- 양쪽 일치
- NPCRequestSaveState: 2개 필드 (cooldowns, activeRequestCount) -- 양쪽 일치
- CumulativeStatsSaveData: 4개 필드 (totalHarvested, totalSold, totalProcessed, totalBuilt) -- 양쪽 일치

### 8.3 ISaveable 구현

```csharp
// illustrative
// QuestManager.cs 내부
public int SaveLoadOrder => 85; // → see 섹션 8.1

public object GetSaveData()
{
    var data = new QuestSaveData();

    // questProgress: 활성/완료/실패 상태인 퀘스트만 저장
    var entries = new List<QuestProgressEntry>();
    foreach (var kvp in _activeQuests)
    {
        var inst = kvp.Value;
        if (inst.Status == QuestStatus.Locked) continue;
        entries.Add(new QuestProgressEntry
        {
            questId = inst.Data.questId,
            status = (int)inst.Status,
            objectiveProgress = (int[])inst.ObjectiveProgress.Clone(),
            acceptedDay = inst.AcceptedDay,
            completedDay = inst.CompletedDay,
            isTracked = inst.IsTracked
        });
    }
    data.questProgress = entries.ToArray();

    data.completedQuestIds = _completedQuestIds.ToArray();

    data.dailyState = _dailySelector.GetSaveState();
    data.npcRequestState = _npcScheduler.GetSaveState();
    data.cumulativeStats = _tracker.GetCumulativeStats();

    return data;
}

public void LoadSaveData(object rawData)
{
    if (rawData is not QuestSaveData data) return;

    // 1) 완료 기록 복원
    _completedQuestIds = new HashSet<string>(data.completedQuestIds ?? System.Array.Empty<string>());

    // 2) 퀘스트 인스턴스 상태 복원
    foreach (var entry in data.questProgress ?? System.Array.Empty<QuestProgressEntry>())
    {
        if (_activeQuests.TryGetValue(entry.questId, out var inst))
        {
            inst.Status = (QuestStatus)entry.status;
            for (int i = 0; i < entry.objectiveProgress.Length && i < inst.ObjectiveProgress.Length; i++)
            {
                inst.ObjectiveProgress[i] = entry.objectiveProgress[i];
            }
            inst.AcceptedDay = entry.acceptedDay;
            inst.CompletedDay = entry.completedDay;
            inst.IsTracked = entry.isTracked;
        }
    }

    // 3) 일일 목표 / NPC 의뢰 상태 복원
    _dailySelector.LoadSaveState(data.dailyState);
    _npcScheduler.LoadSaveState(data.npcRequestState);
    _tracker.LoadCumulativeStats(data.cumulativeStats);

    // 4) 해금 조건 재검사
    RecheckAllUnlockConditions();
}
```

### 8.4 GameSaveData 확장

기존 GameSaveData 루트 클래스에 QuestSaveData 필드를 추가해야 한다.

(-> see `docs/systems/save-load-architecture.md` 섹션 2.1 for GameSaveData 구조)

```
GameSaveData (루트)
├── ... (기존 필드들)
├── TutorialSaveData
└── QuestSaveData              # ← 신규 추가 (null 허용)
```

[OPEN] save-load-architecture.md의 GameSaveData에 `QuestSaveData quest` 필드를 추가하고, JSON 스키마와 C# 클래스를 동시에 업데이트해야 한다. 다음 리뷰 시 반영 필요.

---

## 9. ScriptableObject 에셋 구조

### 9.1 파일 배치

```
Assets/_Project/
├── Scripts/Quest/                        # SeedMind.Quest 네임스페이스
│   ├── QuestManager.cs                   # 싱글턴, ISaveable
│   ├── QuestTracker.cs                   # 이벤트 구독, 진행도 갱신
│   ├── QuestRewarder.cs                  # 보상 지급
│   ├── DailyQuestSelector.cs             # 일일 목표 선택
│   ├── NPCRequestScheduler.cs            # NPC 의뢰 관리
│   ├── QuestEvents.cs                    # 정적 이벤트 허브
│   ├── QuestInstance.cs                  # 런타임 상태
│   ├── QuestSaveData.cs                  # 세이브 데이터 (전체 Serializable 클래스들)
│   └── Data/                             # SeedMind.Quest.Data 네임스페이스
│       ├── QuestData.cs                  # SO 정의
│       ├── QuestObjectiveData.cs         # 목표 Serializable
│       ├── QuestRewardData.cs            # 보상 Serializable
│       └── QuestUnlockCondition.cs       # 해금 조건 Serializable
│
├── Data/Quests/                          # SO 에셋 인스턴스
│   ├── Main/                             # 메인 퀘스트 SO
│   │   ├── SO_Quest_MainSpring01.asset
│   │   ├── SO_Quest_MainSpring02.asset
│   │   ├── SO_Quest_MainSpring03.asset
│   │   ├── SO_Quest_MainSpring04.asset
│   │   ├── SO_Quest_MainSummer01.asset
│   │   └── ... (계절별)
│   ├── NPC/                              # NPC 의뢰 SO
│   │   ├── SO_Quest_NPCHana01.asset
│   │   ├── SO_Quest_NPCHana02.asset
│   │   ├── SO_Quest_NPCCheolsu01.asset
│   │   └── ...
│   ├── Daily/                            # 일일 목표 SO
│   │   ├── SO_Quest_DailyWater.asset
│   │   ├── SO_Quest_DailyHarvest5.asset
│   │   ├── SO_Quest_DailyHarvest10.asset
│   │   └── ...
│   └── Challenge/                        # 농장 도전 SO
│       ├── SO_Quest_FCFirstHarvest.asset
│       ├── SO_Quest_FCHarvest100.asset
│       └── ...
```

### 9.2 네임스페이스

```
SeedMind.Quest                    # QuestManager, QuestTracker, QuestRewarder, QuestEvents, QuestInstance
SeedMind.Quest.Data               # QuestData, QuestObjectiveData, QuestRewardData, QuestUnlockCondition
```

### 9.3 Assembly Definition

| asmdef 파일 | 위치 | 참조하는 asmdef |
|-------------|------|----------------|
| `SeedMind.Quest.asmdef` | `Scripts/Quest/` | Core, Farm, Economy, Building, Level, NPC |

**의존성 근거**:
- Core: Singleton, ISaveable 기반
- Farm: FarmEvents 구독 (Harvest, Till, Water)
- Economy: EconomyEvents 구독 (Sell, EarnGold), EconomyManager 보상 지급
- Building: BuildingEvents 구독 (Build)
- Level: ProgressionEvents 구독 (ReachLevel), ProgressionManager XP 지급
- NPC: NPCEvents 구독 (Deliver), NPCManager 의뢰 연동

### 9.4 의존성 다이어그램 확장

기존 의존성 다이어그램(-> see `docs/systems/project-structure.md` 섹션 3.1)에 Quest 모듈을 추가:

```
                ┌─────────┐
                │  Core   │
                └────┬────┘
                     │
        ┌────────────┼────────────┐
        ▼            ▼            ▼
   ┌────────┐  ┌──────────┐  ┌────────┐
   │  Farm  │  │  Player  │  │ Level  │
   └────┬───┘  └─────┬────┘  └────┬───┘
        │            │            │
        │      ┌─────┴─────┐     │
        │      ▼           ▼     │
        │  ┌────────┐  ┌────────┐│
        └─▶│Economy │  │Building││
           └────┬───┘  └────┬───┘│
                │           │    │
                └─────┬─────┘    │
                      ▼          ▼
               ┌──────────────┐
               │     NPC      │
               └──────┬───────┘
                      ▼
               ┌──────────────┐
               │   Tutorial   │
               └──────┬───────┘
                      ▼
               ┌──────────────┐
               │    Quest     │  ← 신규 (Tutorial 이후, UI 이전)
               └──────┬───────┘
                      ▼
               ┌──────────────┐
               │      UI      │
               └──────────────┘
```

Quest 모듈은 Tutorial과 같은 계층에서 기존 시스템 이벤트를 구독만 하며, 역방향 의존은 없다.

---

## 10. 씬 계층 구조 확장

기존 SCN_Farm 씬 계층(-> see `docs/systems/project-structure.md` 섹션 5.4)에 퀘스트 오브젝트를 추가:

```
SCN_Farm (Scene Root)
├── --- MANAGERS ---
│   ├── GameManager          (DontDestroyOnLoad)
│   ├── TimeManager          (DontDestroyOnLoad)
│   ├── SaveManager          (DontDestroyOnLoad)
│   └── QuestManager         (DontDestroyOnLoad)    ← 신규
│
├── ... (기존 구조)
│
└── --- UI ---
    ├── Canvas_HUD
    │   ├── ... (기존 요소)
    │   └── QuestTrackingWidget                     ← 신규 (우측 추적 퀘스트)
    ├── Canvas_Overlay
    │   ├── ... (기존 패널)
    │   └── QuestLogPanel                           ← 신규 (퀘스트 로그 전체)
    └── Canvas_Popup
        ├── PopupMessage
        └── QuestCompletePopup                      ← 신규 (완료 배너)
```

---

# Part II -- MCP 구현 요약

---

## Step 1: QuestManager 게임오브젝트 및 핵심 스크립트 생성

```
1-1: CreateScript("QuestManager.cs", "Assets/_Project/Scripts/Quest/")
     → namespace SeedMind.Quest
     → MonoBehaviour, Singleton<QuestManager>, ISaveable
     → SaveLoadOrder = 85

1-2: CreateScript("QuestEvents.cs", "Assets/_Project/Scripts/Quest/")
     → static class, 이벤트 8개 선언 (→ see 섹션 6.2)

1-3: CreateScript("QuestInstance.cs", "Assets/_Project/Scripts/Quest/")
     → 런타임 상태 래퍼 (→ see 섹션 5.1)

1-4: CreateScript("QuestSaveData.cs", "Assets/_Project/Scripts/Quest/")
     → QuestSaveData, QuestProgressEntry, DailyQuestSaveState,
       NPCRequestSaveState, CumulativeStatsSaveData

1-5: SCN_Farm 씬에서
     → CreateGameObject("QuestManager") under "--- MANAGERS ---"
     → AddComponent<QuestManager>
     → Set DontDestroyOnLoad

1-6: 열거형 스크립트 생성
     → QuestCategory.cs, QuestStatus.cs, ObjectiveType.cs,
       RewardType.cs, UnlockConditionType.cs, CompositeMode.cs
     → 모두 Assets/_Project/Scripts/Quest/Data/ 배치
       (→ see docs/mcp/quest-tasks.md ARC-016 섹션 1.2 스크립트 목록 S-01~S-06)
```

## Step 2: QuestData ScriptableObject 정의 및 에셋 생성

```
2-1: CreateScript("QuestData.cs", "Assets/_Project/Scripts/Quest/Data/")
     → ScriptableObject, [CreateAssetMenu] (→ see 섹션 4.1)

2-2: CreateScript("QuestObjectiveData.cs", "Assets/_Project/Scripts/Quest/Data/")
     → [Serializable] class (→ see 섹션 4.2)

2-3: CreateScript("QuestRewardData.cs", "Assets/_Project/Scripts/Quest/Data/")
     → [Serializable] class (→ see 섹션 4.3)

2-4: CreateScript("QuestUnlockCondition.cs", "Assets/_Project/Scripts/Quest/Data/")
     → [Serializable] class (→ see 섹션 4.4)

2-5: SO 에셋 폴더 생성
     → CreateFolder("Assets/_Project/Data/Quests/Main")
     → CreateFolder("Assets/_Project/Data/Quests/NPC")
     → CreateFolder("Assets/_Project/Data/Quests/Daily")
     → CreateFolder("Assets/_Project/Data/Quests/Challenge")

2-6: 봄 메인 퀘스트 SO 에셋 생성 (4개)
     → 퀘스트 데이터 값은 (→ see docs/systems/quest-system.md 섹션 3.1)
     → SO_Quest_MainSpring01.asset ~ SO_Quest_MainSpring04.asset

2-7: 일일 목표 SO 에셋 생성 (12개)
     → 퀘스트 데이터 값은 (→ see docs/systems/quest-system.md 섹션 5.1)
     → SO_Quest_DailyWater.asset 등
```

## Step 3: 이벤트 연동 (QuestTracker, QuestRewarder)

```
3-1: CreateScript("QuestTracker.cs", "Assets/_Project/Scripts/Quest/")
     → 이벤트 구독 매핑 구현 (→ see 섹션 6.3)
     → SubscribeAll() / UnsubscribeAll()

3-2: CreateScript("QuestRewarder.cs", "Assets/_Project/Scripts/Quest/")
     → GrantRewards() 구현 (→ see 섹션 3.4)
     → EconomyManager, ProgressionManager, InventoryManager 참조

3-3: CreateScript("DailyQuestSelector.cs", "Assets/_Project/Scripts/Quest/")
     → 일일 목표 2개 선택 로직 (→ see docs/systems/quest-system.md 섹션 5.1)

3-4: CreateScript("NPCRequestScheduler.cs", "Assets/_Project/Scripts/Quest/")
     → NPC 의뢰 등장/쿨다운 관리 (→ see 섹션 7.1)

3-5: QuestManager에서 내부 컴포넌트 초기화
     → _tracker, _rewarder, _dailySelector, _npcScheduler 인스턴스 생성
     → OnEnable에서 이벤트 구독, OnDisable에서 해제

3-6: Assembly Definition 생성
     → CreateAsmdef("SeedMind.Quest", "Assets/_Project/Scripts/Quest/")
     → 참조: Core, Farm, Economy, Building, Level, NPC
```

## Step 4: UI 연결

```
4-1: SCN_Farm → Canvas_HUD 하위에 QuestTrackingWidget 생성
     → 활성 추적 퀘스트 1~2개 표시 (→ see docs/systems/quest-system.md 섹션 8.3)

4-2: SCN_Farm → Canvas_Overlay 하위에 QuestLogPanel 생성
     → 탭 구조: 메인 퀘스트 / NPC 의뢰 / 일일 목표 / 농장 도전
     → (→ see docs/systems/quest-system.md 섹션 8.1)

4-3: SCN_Farm → Canvas_Popup 하위에 QuestCompletePopup 생성
     → 퀘스트 완료 배너, 보상 표시

4-4: CreateScript("QuestLogUI.cs", "Assets/_Project/Scripts/UI/")
     → QuestEvents 구독, 퀘스트 목록/진행도 표시
     → namespace SeedMind.UI

4-5: CreateScript("QuestTrackingUI.cs", "Assets/_Project/Scripts/UI/")
     → HUD 추적 위젯 갱신

4-6: 입력 바인딩 추가
     → SeedMindInputActions에 "QuestLog" 액션 추가 (J키)
```

## Step 5: 테스트 및 검증

```
5-1: SCN_Test_Quest.unity 테스트 씬 생성
     → QuestManager, TimeManager, FarmGrid 최소 구성

5-2: 테스트 시나리오 A: 메인 퀘스트 흐름
     → 튜토리얼 완료 시뮬레이션 → main_spring_01 자동 활성화
     → 작물 수확 10회 → 퀘스트 완료 → 보상 지급 확인
     → Console.Log로 상태 전이 확인

5-3: 테스트 시나리오 B: 일일 목표
     → TimeManager.AdvanceDay() 호출 → 일일 목표 2개 생성 확인
     → 목표 달성 → 완료 확인
     → 다음 날 → 이전 목표 만료, 새 목표 생성 확인

5-4: 테스트 시나리오 C: NPC 의뢰
     → NPCController.Interact() → 의뢰 목록 표시
     → 수락 → 진행 → 완료 → NPC 재방문 → 보상 수령

5-5: 테스트 시나리오 D: 세이브/로드
     → 퀘스트 진행 중 저장 → 로드 → 진행도 유지 확인
     → 완료된 퀘스트 로드 후 재달성 불가 확인

5-6: 테스트 시나리오 E: 농장 도전
     → fc_first_harvest (첫 수확) 자동 완료 확인
     → 누적 통계 증가 확인
```

---

## Cross-references

- `docs/systems/quest-system.md` (DES-009) -- 퀘스트 게임 디자인 (카테고리, 목표 타입, 보상 수치 canonical)
- `docs/systems/save-load-architecture.md` (ARC-011) -- ISaveable 인터페이스, SaveLoadOrder 할당표, GameSaveData 루트 구조
- `docs/systems/npc-shop-architecture.md` (ARC-008) -- NPCManager, NPCController, DialogueSystem 클래스 설계
- `docs/systems/inventory-architecture.md` -- InventoryManager API (아이템 보상 지급)
- `docs/systems/progression-architecture.md` -- ProgressionManager API (XP 보상 지급), UnlockRegistry
- `docs/systems/economy-architecture.md` -- EconomyManager API (골드 보상 지급), EconomyEvents
- `docs/systems/farming-architecture.md` -- FarmEvents (OnCropHarvested, OnTileTilled 등)
- `docs/systems/processing-architecture.md` -- ProcessingEvents (OnProcessingCompleted)
- `docs/systems/tutorial-architecture.md` -- TutorialEvents (OnTutorialCompleted, 퀘스트 시스템 활성화 트리거)
- `docs/systems/tool-upgrade-architecture.md` -- ToolUpgradeEvents (OnUpgradeCompleted)
- `docs/systems/fishing-architecture.md` -- FishingEvents (OnFishCaught), FishData 구조 (ARC-034)
- `docs/systems/gathering-architecture.md` -- GatheringEvents (OnItemGathered), GatheringItemData 구조 (ARC-034)
- `docs/systems/project-structure.md` -- 폴더 구조, 네임스페이스, 의존성 매트릭스, 씬 계층
- `docs/pipeline/data-pipeline.md` -- SO 에셋 데이터 파이프라인

---

## Open Questions

- [OPEN] `NPCEvents.OnItemDelivered` 이벤트가 NPC 아키텍처(`docs/systems/npc-shop-architecture.md`)에 아직 정의되지 않았다. 납품(Deliver) 목표 타입을 추적하려면 NPC 시스템에 해당 이벤트를 추가해야 한다.
- [OPEN] `save-load-architecture.md`의 GameSaveData 루트 클래스에 `QuestSaveData quest` 필드를 추가해야 한다. JSON 스키마와 C# 클래스 양쪽을 동시에 업데이트해야 한다 (PATTERN-005).
- [OPEN] `project-structure.md`의 의존성 매트릭스와 asmdef 목록에 Quest 모듈을 추가해야 한다.
- [OPEN] CumulativeStatsSaveData의 통계 항목(totalHarvested, totalSold 등)이 기존 시스템에서 이미 추적되고 있는지 확인 필요. 중복 추적을 피하려면 기존 시스템의 통계를 직접 참조하는 방안도 검토해야 한다.
- [OPEN] Composite 목표 타입의 재귀 구조(QuestObjectiveData 내부에 subObjectives 배열)가 Unity Inspector에서 편집하기 어려울 수 있다. Custom Editor 또는 SO 분리가 필요할 수 있다.

---

## Risks

- [RISK] **이벤트 구독 누락**: QuestTracker가 14종 ObjectiveType을 추적하기 위해 11개 이상의 외부 이벤트를 구독해야 한다. 하나라도 누락되면 해당 목표 타입이 동작하지 않는다. SubscribeAll()/UnsubscribeAll()에서 모든 이벤트를 명시적으로 나열하고, 테스트 시나리오에서 각 ObjectiveType별 검증이 필요하다.
- [RISK] **이벤트 페이로드 불일치**: 기존 시스템의 이벤트 페이로드(CropHarvestInfo, SellInfo 등)에 QuestTracker가 필요로 하는 필드(cropId, quality, goldEarned 등)가 모두 포함되어 있는지 확인 필요. 누락 시 기존 이벤트 구조를 확장해야 하며, 이는 해당 시스템 문서의 업데이트를 수반한다.
- [RISK] **SaveLoadOrder 충돌**: QuestManager(85)는 TutorialManager(80) 이후에 로드되어야 한다. 향후 다른 시스템이 80~90 범위에 할당되면 충돌 가능. 새 시스템 추가 시 save-load-architecture.md의 할당표를 반드시 갱신해야 한다.
- [RISK] **NPCRequestSaveState의 Dictionary 직렬화**: `cooldowns` 필드가 `Dictionary<string, int>` 타입이다. Newtonsoft.Json은 Dictionary를 지원하나, save-load-architecture.md에서 StringIntPair[] 변환 접근도 제안했다. 직렬화 방식을 통일해야 한다 (-> see `docs/systems/save-load-architecture.md` RISK 5).
- [RISK] **일일 목표 중복 방지 로직**: DailyQuestSelector가 전날 선택 결과(previousDailyIds)를 기억해야 하는데, 세이브/로드 시 이 상태가 유실되면 동일 목표가 연속 출현할 수 있다. DailyQuestSaveState에 포함되어 있으나, 정확한 복원 검증이 필요하다.

---

*이 문서는 Claude Code가 docs/systems/quest-system.md(DES-009)의 게임 디자인을 기반으로 기술 아키텍처를 자율 설계했습니다.*
