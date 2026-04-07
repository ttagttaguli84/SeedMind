# NPC/상점 시스템 기술 아키텍처

> NPC 데이터 구조, 상점 시스템 확장, 대화 시스템, 대장간 연계, 여행 상인 스케줄러, 추가 NPC(마을 상인/농업 전문가/여행 상인 확장) 아키텍처  
> 작성: Claude Code (Opus) | 2026-04-06 | 확장: 2026-04-07 (CON-008)  
> 문서 ID: ARC-008

---

## Context

이 문서는 SeedMind의 NPC 및 상점 시스템에 대한 기술 아키텍처를 정의한다. 기존 `economy-architecture.md`에서 설계한 ShopSystem/ShopData를 기반으로, NPC 개체 관리, 대화 시스템, 대장간 연계, 여행 상인 스케줄러를 통합하는 구조를 기술한다. CON-003 NPC/상점 콘텐츠 문서(`docs/content/npcs.md`)와 연계하여, NPC 이름/성격/대사 등 콘텐츠 값은 해당 문서를 canonical로 참조한다.

**설계 목표**:
- 기존 ShopSystem(`docs/systems/economy-architecture.md` 섹션 4.3)을 NPC별 상점으로 확장하되, 기존 인터페이스를 깨뜨리지 않는다
- NPC 데이터(외형, 스케줄, 대사)를 ScriptableObject로 분리하여 콘텐츠 확장이 코드 변경 없이 가능하도록 한다
- 대화 시스템은 단순 분기(선형 + 선택지) 구조로, 과도한 복잡성을 피한다
- ToolUpgradeSystem(`docs/systems/tool-upgrade-architecture.md`)과 대장간 NPC의 연동을 이벤트 기반으로 설계한다

---

# Part I -- 디자인 구조

---

## 1. NPC 시스템 전체 흐름

```
[플레이어] ── E키(Interact) ──> [NPC 감지]
    │                              │
    │                     ┌────────┴────────┐
    │                     │  NPCController  │
    │                     │  (NPC별 행동)    │
    │                     └────────┬────────┘
    │                              │
    │              ┌───────────────┼───────────────┐
    │              ▼               ▼               ▼
    │     ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
    │     │ DialogueSystem│ │  ShopSystem  │ │ToolUpgrade   │
    │     │ (대화 트리거)  │ │  (상점 열기)  │ │System (대장간)│
    │     └──────┬───────┘ └──────┬───────┘ └──────┬───────┘
    │            │                │                │
    │            ▼                ▼                ▼
    │     ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
    │     │ DialogueUI   │ │   ShopUI     │ │ UpgradeUI    │
    │     │ (대화창 표시)  │ │  (상점 UI)   │ │ (업그레이드UI)│
    │     └──────────────┘ └──────────────┘ └──────────────┘
```

### 1.1 상호작용 흐름 상세

1. 플레이어가 NPC 인터랙션 범위 내에서 E키를 누른다
2. NPCController가 해당 NPC의 NPCData SO를 참조하여 기본 인사 대화를 트리거한다
3. 대화 종료 후 NPC 유형에 따라 서비스 메뉴가 표시된다:
   - 잡화 상인: 구매/판매 선택
   - 대장간: 업그레이드/재료 구매/도구 수령 선택
   - 목수: 시설 건설/농장 확장 선택
   - 여행 상인: 특수 물품 구매
4. 서비스 선택 시 해당 시스템(ShopSystem, ToolUpgradeSystem, BuildingManager)에 처리를 위임한다

### 1.2 NPC 종류별 역할 정의

| NPC 유형 | 영문 키 | 역할 | 연동 시스템 | 위치 |
|----------|---------|------|-------------|------|
| 잡화 상인 | `GeneralMerchant` | 씨앗/비료/소모품 판매, 작물/가공품 매입 | ShopSystem, EconomyManager | 마을 중앙 |
| 대장간 주인 | `Blacksmith` | 도구 업그레이드, 업그레이드 재료 판매 | ToolUpgradeSystem, ShopSystem | 마을 외곽 |
| 목수 | `Carpenter` | 시설 건설 의뢰, 농장 확장 | BuildingManager, ShopSystem | 마을 북쪽 |
| 여행 상인 | `TravelingMerchant` | 희귀 씨앗/아이템 판매 (주기적 방문) | ShopSystem | 마을 광장 (방문 시, -> see `docs/content/npcs.md` 섹션 2.2) |
| 마을 상인 | `VillageMerchant` | 가공품/생활용품 판매, 농산물 고가 매입 | ShopSystem, EconomyManager | (-> see `docs/content/npcs.md`, CON-008) |
| 농업 전문가 | `AgricultureExpert` | 농업 힌트/조언 제공, 특수 씨앗 보상 | HintSystem, ProgressionManager | (-> see `docs/content/npcs.md`, CON-008) |

NPC 이름, 성격, 외형 등 콘텐츠 상세는 (-> see `docs/content/npcs.md`, CON-003/CON-008)에서 정의한다.

---

# Part II -- 기술 아키텍처

---

## 2. ScriptableObject 설계

### 2.1 NPCData (ScriptableObject)

NPC 개체의 정적 데이터를 정의하는 SO. 각 NPC마다 하나의 에셋을 생성한다.

```csharp
// illustrative
namespace SeedMind.NPC.Data
{
    [CreateAssetMenu(fileName = "NewNPCData", menuName = "SeedMind/NPCData")]
    public class NPCData : ScriptableObject
    {
        [Header("기본 정보")]
        public string npcId;                      // "general_merchant", "blacksmith"
        public string displayName;                // UI 표시 이름 (→ see docs/content/npcs.md, CON-003)
        public NPCType npcType;                   // enum: GeneralMerchant, Blacksmith, Carpenter, TravelingMerchant, VillageMerchant, AgricultureExpert (→ see NPCType enum 정의)
        public Sprite portrait;                   // 대화창 초상화

        [Header("위치/스케줄")]
        public Vector3 defaultPosition;           // 기본 위치 (→ see docs/content/npcs.md, CON-003)
        public int openHour;                      // 활동 시작 시간 (→ see docs/systems/time-season.md 섹션 1.7)
        public int closeHour;                     // 활동 종료 시간 (→ see docs/systems/time-season.md 섹션 1.7)
        public DayFlag closedDays;                // 휴무 요일 비트마스크 (→ see docs/systems/economy-system.md 섹션 3.2)

        [Header("대화")]
        public DialogueData greetingDialogue;     // 기본 인사 대화 데이터
        public DialogueData closedDialogue;       // 영업 외/휴무 시 대화
        // weatherClosedDialogue 필드는 CON-008 섹션 11.5에서 추가됨 (→ see 섹션 11.5)

        [Header("상점 연결")]
        public ShopData shopData;                 // 연결된 상점 데이터 (null이면 상점 없음, → see docs/systems/economy-architecture.md 섹션 4.3)

        [Header("시각")]
        public GameObject prefab;                 // NPC 프리팹 참조
        public float interactionRadius;           // 인터랙션 감지 범위 (기본 2.0f) // → see docs/content/npcs.md
    }
}
```

**NPCType enum**:

```csharp
namespace SeedMind.NPC
{
    public enum NPCType
    {
        GeneralMerchant    = 0,
        Blacksmith         = 1,
        Carpenter          = 2,
        TravelingMerchant  = 3,
        VillageMerchant    = 4,   // 마을 상인 (CON-008)
        AgricultureExpert  = 5    // 농업 전문가 (CON-008)
    }
}
```

**DayFlag enum** (비트마스크):

```csharp
namespace SeedMind.NPC
{
    [System.Flags]
    public enum DayFlag
    {
        None      = 0,
        Monday    = 1 << 0,   // Day 1, 8, 15, 22
        Tuesday   = 1 << 1,   // Day 2, 9, 16, 23
        Wednesday = 1 << 2,   // Day 3, 10, 17, 24
        Thursday  = 1 << 3,   // Day 4, 11, 18, 25
        Friday    = 1 << 4,   // Day 5, 12, 19, 26
        Saturday  = 1 << 5,   // Day 6, 13, 20, 27
        Sunday    = 1 << 6    // Day 7, 14, 21, 28
    }
}
```

에셋 이름: `SO_NPC_GeneralMerchant.asset`, `SO_NPC_Blacksmith.asset` 등  
저장 경로: `Assets/_Project/Data/NPCs/`

### 2.2 DialogueData (ScriptableObject)

NPC 대화 흐름을 정의하는 SO. 단순 선형 + 선택지 구조를 지원한다.

```csharp
// illustrative
namespace SeedMind.NPC.Data
{
    [CreateAssetMenu(fileName = "NewDialogueData", menuName = "SeedMind/DialogueData")]
    public class DialogueData : ScriptableObject
    {
        public string dialogueId;                 // "greeting_merchant_spring"
        public DialogueNode[] nodes;              // 대화 노드 배열 (순서대로 진행)
    }

    [System.Serializable]
    public class DialogueNode
    {
        public string speakerName;                // 화자 이름 (NPC 이름 또는 "")
        [TextArea(2, 5)]
        public string text;                       // 대사 텍스트 (→ see docs/content/npcs.md, CON-003)
        public DialogueChoice[] choices;          // 선택지 배열 (비어있으면 자동 다음 노드 진행)
    }

    [System.Serializable]
    public class DialogueChoice
    {
        public string choiceText;                 // 선택지 UI 텍스트
        public DialogueChoiceAction action;       // 선택 시 동작
        public int jumpToNode;                    // 점프할 노드 인덱스 (-1 = 대화 종료)
    }

    public enum DialogueChoiceAction
    {
        Continue     = 0,   // 다음 노드로 진행 (jumpToNode 사용)
        OpenShop     = 1,   // 상점 UI 열기
        OpenUpgrade  = 2,   // 도구 업그레이드 UI 열기
        OpenBuild    = 3,   // 시설 건설 UI 열기
        CloseDialogue = 4,  // 대화 종료
        OpenHint     = 5,   // 농업 전문가 힌트 UI 열기 (CON-008)
        RequestAdvice = 6   // 농업 조언 요청 (CON-008)
    }
}
```

에셋 이름: `SO_Dlg_Greeting_Merchant.asset`, `SO_Dlg_Closed_Blacksmith.asset` 등  
저장 경로: `Assets/_Project/Data/Dialogues/`

### 2.3 ShopInventoryData (ScriptableObject) -- 여행 상인 전용 확장

기존 ShopData(-> see `docs/systems/economy-architecture.md` 섹션 4.3)는 고정 상점(잡화/대장간/목공소)에 사용한다. 여행 상인은 방문마다 재고가 달라지므로, 후보 아이템 풀을 정의하는 별도 SO가 필요하다.

```csharp
// illustrative
namespace SeedMind.NPC.Data
{
    [CreateAssetMenu(fileName = "NewTravelingShopPool", menuName = "SeedMind/TravelingShopPool")]
    public class TravelingShopPoolData : ScriptableObject
    {
        public string poolId;                         // "traveling_pool_default"

        [Header("후보 아이템")]
        public TravelingShopCandidate[] candidates;   // 전체 후보 목록

        [Header("출현 규칙")]
        public int minItemCount;                      // 방문 시 최소 아이템 수 // → see docs/content/npcs.md
        public int maxItemCount;                      // 방문 시 최대 아이템 수 // → see docs/content/npcs.md
    }

    [System.Serializable]
    public class TravelingShopCandidate
    {
        public ScriptableObject itemReference;        // CropData, FertilizerData 등
        public PriceData priceData;                   // 가격 데이터 참조
        public float selectionWeight;                 // 선택 가중치 (높을수록 등장 확률 높음)
        public int minPlayerLevel;                    // 최소 플레이어 레벨 요건
        public SeasonFlag availableSeasons;           // 등장 가능 계절 (비트마스크)
        public int stockMin;                          // 재고 최소
        public int stockMax;                          // 재고 최대
    }
}
```

에셋 이름: `SO_TravelingPool_Default.asset`  
저장 경로: `Assets/_Project/Data/NPCs/`

---

## 3. 핵심 클래스 설계

### 3.1 클래스 책임 요약

| 클래스 | 유형 | 네임스페이스 | 책임 |
|--------|------|-------------|------|
| **NPCManager** | MonoBehaviour (Singleton) | `SeedMind.NPC` | NPC 레지스트리 관리, 활동 상태 갱신, NPC 검색 |
| **NPCController** | MonoBehaviour | `SeedMind.NPC` | 개별 NPC 행동 제어, 인터랙션 처리, 대화/서비스 위임 |
| **DialogueSystem** | MonoBehaviour (Singleton) | `SeedMind.NPC` | 대화 흐름 관리, 노드 진행, 선택지 처리 |
| **TravelingMerchantScheduler** | MonoBehaviour | `SeedMind.NPC` | 여행 상인 방문 일정 관리, 재고 생성 |
| **NPCEvents** | static class | `SeedMind.NPC` | NPC 관련 정적 이벤트 허브 |
| **NPCData** | ScriptableObject | `SeedMind.NPC.Data` | NPC 정적 데이터 |
| **DialogueData** | ScriptableObject | `SeedMind.NPC.Data` | 대화 흐름 데이터 |
| **TravelingShopPoolData** | ScriptableObject | `SeedMind.NPC.Data` | 여행 상인 후보 아이템 풀 |
| **TravelingMerchantData** | ScriptableObject | `SeedMind.NPC.Data` | 여행 상인 방문 스케줄/확률 설정 (CON-008) |
| **NPCHintData** | ScriptableObject | `SeedMind.NPC.Data` | 농업 전문가 힌트 조건/내용 (CON-008) |
| **NPCHintSystem** | MonoBehaviour | `SeedMind.NPC` | 농업 전문가 힌트 판정, 조건 평가, 표시 관리 (CON-008) |
| **OperatingScheduleEvaluator** | static class | `SeedMind.NPC` | NPC 영업 시간 판정 유틸리티 (CON-008) |

### 3.2 NPCManager

```
┌──────────────────────────────────────────────────────────────┐
│                 NPCManager (MonoBehaviour, Singleton)          │
│──────────────────────────────────────────────────────────────│
│  [설정 참조]                                                   │
│  - _npcRegistry: NPCData[]         (전체 NPC 데이터 SO 목록)  │
│                                                              │
│  [상태]                                                       │
│  - _activeNPCs: Dictionary<string, NPCController>            │
│  - _npcStates: Dictionary<string, NPCActivityState>          │
│                                                              │
│  [읽기 전용 프로퍼티]                                           │
│  + ActiveNPCs: IReadOnlyDictionary<string, NPCController>    │
│                                                              │
│  [메서드]                                                     │
│  + Initialize(): void                                        │
│  + GetNPC(string npcId): NPCController                       │
│  + IsNPCAvailable(string npcId): bool                        │
│  + RefreshNPCStates(int currentHour, int currentDay): void   │
│  + GetSaveData(): NPCSaveData                                │
│  + LoadSaveData(NPCSaveData data): void                      │
│                                                              │
│  [이벤트 구독]                                                 │
│  - TimeManager.OnHourChanged → RefreshNPCStates()            │
│  - WeatherSystem.OnWeatherChanged → HandleWeatherChange()    │
└──────────────────────────────────────────────────────────────┘
```

**NPCActivityState enum**:

```csharp
namespace SeedMind.NPC
{
    public enum NPCActivityState
    {
        Active,           // 영업 중, 인터랙션 가능
        Closed,           // 영업 외 시간 또는 휴무일
        WeatherClosed,    // 악천후로 임시 마감 (→ see docs/systems/time-season.md 섹션 3.4)
        Away              // 부재 (여행 상인 미방문 상태)
    }
}
```

### 3.3 NPCController

```
┌──────────────────────────────────────────────────────────────┐
│                 NPCController (MonoBehaviour)                  │
│──────────────────────────────────────────────────────────────│
│  [설정 참조]                                                   │
│  - _npcData: NPCData (ScriptableObject)                      │
│                                                              │
│  [외부 참조 (자동 탐색)]                                        │
│  - _dialogueSystem: DialogueSystem                           │
│  - _shopSystem: ShopSystem (상점 보유 NPC만)                   │
│  - _upgradeSystem: ToolUpgradeSystem (대장간만)               │
│  - _buildingManager: BuildingManager (목수만)                 │
│                                                              │
│  [상태]                                                       │
│  - _currentState: NPCActivityState                           │
│  - _isInteracting: bool                                      │
│                                                              │
│  [메서드]                                                     │
│  + Interact(PlayerController player): void                   │
│  + SetState(NPCActivityState state): void                    │
│  + IsAvailable(): bool                                       │
│  - StartDialogue(): void                                     │
│  - HandleDialogueChoice(DialogueChoiceAction action): void   │
│  - OpenNPCService(): void                                    │
└──────────────────────────────────────────────────────────────┘
```

### 3.4 DialogueSystem

```
┌──────────────────────────────────────────────────────────────┐
│               DialogueSystem (MonoBehaviour, Singleton)        │
│──────────────────────────────────────────────────────────────│
│  [상태]                                                       │
│  - _currentDialogue: DialogueData                            │
│  - _currentNodeIndex: int                                    │
│  - _isActive: bool                                           │
│  - _currentNPC: NPCController (대화 중인 NPC 참조)            │
│                                                              │
│  [읽기 전용 프로퍼티]                                           │
│  + IsActive: bool                                            │
│  + CurrentNode: DialogueNode                                 │
│                                                              │
│  [이벤트]                                                     │
│  + OnDialogueStarted: Action<DialogueData>                   │
│  + OnDialogueNodeChanged: Action<DialogueNode>               │
│  + OnDialogueChoiceMade: Action<DialogueChoiceAction>        │
│  + OnDialogueEnded: Action                                   │
│                                                              │
│  [메서드]                                                     │
│  + StartDialogue(DialogueData data, NPCController npc): void │
│  + AdvanceNode(): void                                       │
│  + SelectChoice(int choiceIndex): void                       │
│  + EndDialogue(): void                                       │
│  - ProcessChoiceAction(DialogueChoiceAction action): void    │
└──────────────────────────────────────────────────────────────┘
```

### 3.5 TravelingMerchantScheduler

```
┌──────────────────────────────────────────────────────────────┐
│         TravelingMerchantScheduler (MonoBehaviour)             │
│──────────────────────────────────────────────────────────────│
│  [설정 참조]                                                   │
│  - _merchantNPCData: NPCData                                 │
│  - _shopPool: TravelingShopPoolData                          │
│  - _spawnPosition: Transform                                 │
│  - _visitDays: DayFlag (토/일 고정 → see docs/content/npcs.md │
│                         섹션 6.2)                             │
│                                                              │
│  [상태]                                                       │
│  - _isPresent: bool                                          │
│  - _currentStock: List<ShopItemEntry>                        │
│  - _randomSeed: int                                          │
│  - _affinityPoints: int                                      │
│                                                              │
│  [메서드]                                                     │
│  + CheckVisitSchedule(int currentDay, int currentDayOfWeek): │
│    void                                                      │
│  + GenerateStock(int playerLevel, Season season): void       │
│  + SpawnMerchant(): void                                     │
│  + DespawnMerchant(): void                                   │
│  + GetAffinityLevel(int points, int[] thresholds):           │
│    MerchantAffinityLevel                                     │
│  + ApplyAffinityBonus(ref StockItem item,                    │
│    MerchantAffinityLevel level): void                        │
│  + GetSaveData(): TravelingMerchantSaveData                  │
│  + LoadSaveData(TravelingMerchantSaveData data): void        │
│                                                              │
│  [이벤트 구독]                                                 │
│  - TimeManager.OnDayChanged → CheckVisitSchedule()           │
│  - NPCEvents.OnAffinityChanged → UpdateAffinityPoints()      │
└──────────────────────────────────────────────────────────────┘
```

**여행 상인 등장 조건 로직** (DayFlag 기반 고정 스케줄):

```
CheckVisitSchedule(currentDay, currentDayOfWeek):
    // currentDayOfWeek: 0=Monday ~ 6=Sunday
    // _visitDays: DayFlag 비트마스크 (토/일 → see docs/content/npcs.md 섹션 6.2)
    isVisitDay = (_visitDays & (1 << currentDayOfWeek)) != 0

    if _isPresent && !isVisitDay:
        DespawnMerchant()
    elif !_isPresent && isVisitDay:
        GenerateStock(playerLevel, currentSeason)
        SpawnMerchant()
```

등장 요일, 등장 시간, 플레이어 레벨 요건 등의 수치는 (-> see `docs/content/npcs.md` 섹션 6.2, CON-003)에서 정의한다. 난수 주기(`visitIntervalMin/Max`) 방식을 채택하지 않고, canonical 문서의 "매주 토/일 고정 등장" 방식을 따른다.

---

## 4. 이벤트 시스템 -- NPCEvents

기존 패턴(FarmEvents, BuildingEvents, ToolUpgradeEvents)을 계승하는 정적 이벤트 허브.

```csharp
// illustrative
namespace SeedMind.NPC
{
    public static class NPCEvents
    {
        // --- 대화 ---
        public static event Action<string, DialogueData> OnDialogueStarted;   // npcId, dialogueData
        public static event Action<string> OnDialogueEnded;                   // npcId

        // --- 상점 ---
        public static event Action<string> OnShopOpened;                      // npcId
        public static event Action<string> OnShopClosed;                      // npcId

        // --- NPC 상태 ---
        public static event Action<string, NPCActivityState> OnNPCStateChanged;  // npcId, newState

        // --- 여행 상인 ---
        public static event Action OnTravelingMerchantArrived;
        public static event Action OnTravelingMerchantDeparted;

        // --- 발행 메서드 (internal) ---
        internal static void RaiseDialogueStarted(string npcId, DialogueData data)
            => OnDialogueStarted?.Invoke(npcId, data);
        internal static void RaiseDialogueEnded(string npcId)
            => OnDialogueEnded?.Invoke(npcId);
        internal static void RaiseShopOpened(string npcId)
            => OnShopOpened?.Invoke(npcId);
        internal static void RaiseShopClosed(string npcId)
            => OnShopClosed?.Invoke(npcId);
        internal static void RaiseNPCStateChanged(string npcId, NPCActivityState state)
            => OnNPCStateChanged?.Invoke(npcId, state);
        internal static void RaiseTravelingMerchantArrived()
            => OnTravelingMerchantArrived?.Invoke();
        internal static void RaiseTravelingMerchantDeparted()
            => OnTravelingMerchantDeparted?.Invoke();
    }
}
```

### 4.1 이벤트 소비 매핑

| 이벤트 | 발행자 | 소비자 | 용도 |
|--------|--------|--------|------|
| `OnDialogueStarted` | DialogueSystem | DialogueUI, HUDController | 대화창 표시, HUD 숨김 |
| `OnDialogueEnded` | DialogueSystem | DialogueUI, PlayerController | 대화창 닫기, 플레이어 입력 복원 |
| `OnShopOpened` | NPCController | ShopUI, PlayerController | 상점 UI 표시, 플레이어 이동 잠금 |
| `OnShopClosed` | NPCController | ShopUI, PlayerController | 상점 UI 닫기, 플레이어 이동 복원 |
| `OnNPCStateChanged` | NPCManager | NPCController, UI | NPC 외형/대화 전환 |
| `OnTravelingMerchantArrived` | TravelingMerchantScheduler | HUDController, NPCManager | 알림 표시, NPC 활성화 |
| `OnTravelingMerchantDeparted` | TravelingMerchantScheduler | HUDController, NPCManager | NPC 비활성화 |

---

## 5. 데이터 흐름 상세

### 5.1 플레이어 -> NPC -> 상점 구매 흐름

```
1. PlayerController.OnInteract()
   │
   2. 범위 내 NPCController 감지 (Physics.OverlapSphere, interactionRadius)
   │
   3. NPCController.Interact(player)
   │   ├── IsAvailable() 확인
   │   │   ├── false → closedDialogue 재생 → 종료
   │   │   └── true → 계속
   │   │
   │   4. DialogueSystem.StartDialogue(greetingDialogue, this)
   │       │
   │       5. DialogueUI 표시 → 대사 진행
   │       │
   │       6. 선택지 표시 (예: "물건 구매" / "물건 판매" / "대화 종료")
   │           │
   │           7a. [OpenShop 선택]
   │           │   → NPCController.HandleDialogueChoice(OpenShop)
   │           │   → ShopSystem.Open() (해당 NPC의 ShopData 참조)
   │           │   → ShopUI 표시
   │           │       │
   │           │       8. ShopUI에서 아이템 선택 → 수량 입력
   │           │       │
   │           │       9. ShopSystem.TryBuyItem(item, qty)
   │           │       │   ├── 골드 확인 → EconomyManager.CanSpend(cost)
   │           │       │   ├── 레벨 확인 → ProgressionManager.CurrentLevel >= requiredLevel
   │           │       │   ├── 재고 확인 → stockLimit
   │           │       │   ├── 인벤토리 공간 확인 → InventoryManager.HasSpace()
   │           │       │   │
   │           │       │   ├── [성공]
   │           │       │   │   ├── EconomyManager.SpendGold(cost)
   │           │       │   │   ├── InventoryManager.AddItem(item, qty)
   │           │       │   │   ├── 재고 차감
   │           │       │   │   └── NPCEvents.RaiseShopOpened() / ShopSystem.OnItemPurchased
   │           │       │   │
   │           │       │   └── [실패] → ShopUI 피드백 표시
   │           │       │
   │           │       10. ShopUI 닫기 → NPCEvents.RaiseShopClosed()
   │           │
   │           7b. [CloseDialogue 선택]
   │               → DialogueSystem.EndDialogue()
   │               → NPCEvents.RaiseDialogueEnded()
```

### 5.2 대장간 도구 업그레이드 흐름

```
1. NPCController(Blacksmith).Interact(player)
   │
   2. DialogueSystem → 인사 대화
   │
   3. 선택지: "도구 업그레이드" / "재료 구매" / "도구 수령" / "나가기"
   │
   ├── 3a. [OpenUpgrade 선택]
   │       │
   │       4. ToolUpgradeSystem.GetUpgradeableTools()
   │       │   → 현재 소유 도구 중 nextTier가 있는 도구 목록
   │       │
   │       5. UpgradeUI 표시 (도구 목록 + 각 조건/비용)
   │       │   → ToolUpgradeSystem.CanUpgrade(toolData) 결과로 UI 색상 결정
   │       │
   │       6. 도구 선택 → ToolUpgradeSystem.StartUpgrade(toolData)
   │       │   (→ see docs/systems/tool-upgrade-architecture.md 섹션 2 for 상세 흐름)
   │       │   ├── 골드 차감, 재료 차감, 도구 잠금
   │       │   └── ToolUpgradeEvents.OnUpgradeStarted 발행
   │       │
   │       7. 소요 일수 경과 후 재방문 시:
   │          → "도구 수령" 선택
   │          → ToolUpgradeSystem.CompleteUpgrade(toolType)
   │          → 인벤토리에 업그레이드된 도구 교체
   │          → ToolUpgradeEvents.OnUpgradeCompleted 발행
   │
   └── 3b. [OpenShop 선택 - 재료 구매]
           │
           → ShopSystem.Open() (대장간 ShopData 참조)
           → 철 조각, 정제 강철 등 구매
           (→ see docs/systems/tool-upgrade.md 섹션 6.3 for 대장간 판매 품목)
```

---

## 6. 프로젝트 구조 확장

### 6.1 폴더 구조

기존 프로젝트 구조(-> see `docs/systems/project-structure.md`)에 NPC 모듈을 추가한다.

```
Assets/_Project/Scripts/
    └── NPC/                           # NPC 시스템
        ├── NPCManager.cs
        ├── NPCController.cs
        ├── DialogueSystem.cs
        ├── TravelingMerchantScheduler.cs
        ├── NPCEvents.cs               # 정적 이벤트 허브
        └── Data/
            ├── NPCData.cs             # ScriptableObject 정의
            ├── NPCType.cs             # enum
            ├── NPCActivityState.cs    # enum
            ├── DayFlag.cs             # enum (비트마스크)
            ├── DialogueData.cs        # ScriptableObject 정의
            ├── DialogueNode.cs        # 직렬화 데이터 클래스
            ├── DialogueChoice.cs      # 직렬화 데이터 클래스
            ├── DialogueChoiceAction.cs # enum
            ├── TravelingShopPoolData.cs # ScriptableObject 정의
            └── NPCSaveData.cs         # NPCSaveData, TravelingMerchantSaveData (직렬화 클래스)

Assets/_Project/Data/
    ├── NPCs/                          # NPC SO 에셋
    │   ├── SO_NPC_GeneralMerchant.asset
    │   ├── SO_NPC_Blacksmith.asset
    │   ├── SO_NPC_Carpenter.asset
    │   ├── SO_NPC_TravelingMerchant.asset
    │   └── SO_TravelingPool_Default.asset
    └── Dialogues/                     # 대화 데이터 SO 에셋
        ├── SO_Dlg_Greeting_Merchant.asset
        ├── SO_Dlg_Greeting_Blacksmith.asset
        ├── SO_Dlg_Greeting_Carpenter.asset
        ├── SO_Dlg_Greeting_TravelingMerchant.asset
        ├── SO_Dlg_Closed_Merchant.asset
        └── ...
```

### 6.2 네임스페이스

```
SeedMind.NPC                          # NPCManager, NPCController, DialogueSystem, NPCEvents 등
SeedMind.NPC.Data                     # NPCData, DialogueData, TravelingShopPoolData 등 SO 정의
```

### 6.3 Assembly Definition

| asmdef 파일 | 위치 | 참조하는 asmdef |
|-------------|------|----------------|
| `SeedMind.NPC.asmdef` | `Scripts/NPC/` | Core, Farm, Player, Economy, Building |

NPC 모듈은 다른 모듈의 시스템(ShopSystem, ToolUpgradeSystem, BuildingManager)과 직접 연동하므로 참조 범위가 넓다. 다만 다른 모듈에서 NPC를 직접 참조하지 않고, NPCEvents를 통해 이벤트 기반으로 통신하는 것이 원칙이다.

[RISK] NPC 모듈이 Player, Economy, Building을 모두 참조하면 의존성 범위가 넓어진다. 인터페이스 기반 추상화(IServiceProvider 등)로 직접 참조를 줄이는 것을 고려할 수 있으나, 현재 규모에서는 과도한 추상화이므로 직접 참조를 허용한다.

### 6.4 의존성 다이어그램 업데이트

기존 의존성 구조(-> see `docs/systems/project-structure.md` 섹션 3.1)에 NPC 모듈을 추가한다.

```
                ┌─────────┐
                │  Core   │  (의존성 없음)
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
                ▼           ▼    ▼
           ┌─────────────────────┐
           │        NPC          │  (Economy, Building, Player, Farm, Core 참조)
           └──────────┬──────────┘
                      │
                      ▼
                ┌──────────┐
                │    UI    │  (모든 것 참조 가능)
                └──────────┘
```

### 6.5 씬 계층 구조 확장

기존 SCN_Farm 계층(-> see `docs/systems/project-structure.md` 섹션 5.4)에 NPC 섹션을 추가한다.

```
SCN_Farm (Scene Root)
├── --- MANAGERS ---
│   ├── ...
│   └── NPCManager            (NPC 레지스트리 관리)
│
├── --- NPCs ---
│   ├── NPC_GeneralMerchant    (NPCController + NPCData 참조)
│   ├── NPC_Blacksmith         (NPCController + NPCData 참조)
│   ├── NPC_Carpenter          (NPCController + NPCData 참조)
│   └── NPC_TravelingMerchant  (동적 생성/파괴, TravelingMerchantScheduler)
│
├── --- ECONOMY ---
│   ├── EconomyManager
│   ├── Shop                   (기존 ShopSystem — NPC별 ShopData로 전환)
│   └── ShippingBin
│
└── --- UI ---
    ├── Canvas_Overlay
    │   ├── ...
    │   ├── DialoguePanel       (대화창 UI)
    │   └── UpgradePanel        (도구 업그레이드 UI)
    └── ...
```

---

## 7. 세이브/로드 확장

### 7.1 NPC 세이브 데이터

> **[DEPRECATED]** 이 섹션의 TravelingMerchantSaveData 정의(4필드)는 CON-008c에서 7필드로 확장되었다.  
> 최신 버전: **섹션 9.4** (C# 클래스 + JSON 스키마 7필드 PATTERN-005 준수).  
> 이 섹션은 히스토리 보존 목적으로 유지한다.

```csharp
// illustrative — 초기 버전 (→ see 섹션 9.4, 13.2 for CON-008 확장 버전)
namespace SeedMind.NPC.Data
{
    [System.Serializable]
    public class NPCSaveData
    {
        public TravelingMerchantSaveData travelingMerchant;
        // 향후 확장: NPC 호감도, 대화 진행 상태 등
    }

    [System.Serializable]
    public class TravelingMerchantSaveData
    {
        public bool isPresent;              // 현재 방문 중 여부
        public int randomSeed;              // 재고 생성 시드
        public string[] currentStockItemIds; // 현재 재고 아이템 ID 목록
        public int[] currentStockQuantities; // 현재 재고 수량
        // DayFlag 기반 고정 스케줄이므로 nextVisitDay, departureDayOffset 불필요
        // 등장 요일: -> see docs/content/npcs.md 섹션 6.2
    }
}
```

기존 세이브 루트 구조(-> see `docs/pipeline/data-pipeline.md` Part I 섹션 3)에 `npc` 필드를 추가한다.

### 7.2 JSON 스키마 확장

> **[DEPRECATED]** 이 섹션의 JSON 스키마(4필드)는 CON-008c에서 7필드로 확장되었다.  
> 최신 버전: **섹션 9.4** (C# 클래스 + JSON 스키마 7필드 PATTERN-005 준수).  
> 이 섹션은 히스토리 보존 목적으로 유지한다.

```json
{
    "npc": {
        "travelingMerchant": {
            "isPresent": false,
            "randomSeed": 42,
            "currentStockItemIds": [],
            "currentStockQuantities": []
        }
    }
}
```

PATTERN-005 준수: JSON 스키마와 C# 클래스의 필드명/필드 수가 동일하다 (isPresent, randomSeed, currentStockItemIds, currentStockQuantities -- 4개씩). DayFlag 기반 고정 스케줄 전환으로 nextVisitDay, departureDayOffset 필드를 양쪽 모두에서 제거하였다.

---

## 8. MCP 구현 태스크 요약

상세 MCP 태스크는 별도 ARC-009로 분리 예정. 여기서는 주요 구현 단계를 개략한다.

### Phase A: NPC 기반 구조 생성

**Step A-1**: NPC 스크립트 생성
- `Scripts/NPC/` 폴더 생성
- NPCData.cs, NPCType.cs, NPCActivityState.cs, DayFlag.cs SO/enum 클래스 작성
- NPCEvents.cs 정적 이벤트 허브 작성

**Step A-2**: NPCManager + NPCController 생성
- NPCManager.cs (Singleton) 작성
- NPCController.cs 작성
- SCN_Farm에 NPCManager GameObject 추가

### Phase B: 대화 시스템 생성

**Step B-1**: DialogueData SO + DialogueSystem 생성
- DialogueData.cs, DialogueNode.cs, DialogueChoice.cs, DialogueChoiceAction.cs 작성
- DialogueSystem.cs (Singleton) 작성

**Step B-2**: DialogueUI 연결
- Canvas_Overlay에 DialoguePanel 추가
- DialogueUI.cs 작성 (DialogueSystem 이벤트 구독)

### Phase C: NPC SO 에셋 생성

**Step C-1**: NPC 데이터 에셋 생성
- `Data/NPCs/` 폴더 생성
- SO_NPC_GeneralMerchant, SO_NPC_Blacksmith, SO_NPC_Carpenter, SO_NPC_TravelingMerchant 에셋 생성
- 각 에셋에 NPCData 필드 설정 (위치, 시간, 상점 참조 등)
- NPC 이름/대사 등 콘텐츠 값은 (-> see `docs/content/npcs.md`, CON-003)에서 확정 후 반영

**Step C-2**: 대화 데이터 에셋 생성
- `Data/Dialogues/` 폴더 생성
- NPC별 인사/휴무 대화 SO 에셋 생성

### Phase D: NPC GameObject 배치

**Step D-1**: SCN_Farm에 NPC 오브젝트 배치
- --- NPCs --- 구분 오브젝트 생성
- NPC_GeneralMerchant, NPC_Blacksmith, NPC_Carpenter 배치
- 각 오브젝트에 NPCController 부착, NPCData SO 할당

### Phase E: 여행 상인 시스템

**Step E-1**: TravelingMerchantScheduler 생성
- TravelingShopPoolData.cs SO 작성
- TravelingMerchantScheduler.cs 작성
- SO_TravelingPool_Default 에셋 생성 (후보 아이템 설정)

### Phase F: 기존 시스템 연동

**Step F-1**: ShopSystem 확장
- 기존 ShopSystem이 NPCController에서 NPC별 ShopData를 받아 열리도록 연결
- 대장간 ShopData에 재료 판매 품목 추가

**Step F-2**: ToolUpgradeSystem 연동
- NPCController(Blacksmith)에서 OpenUpgrade 선택 시 ToolUpgradeSystem 호출 경로 연결
- BlacksmithPanel(UpgradePanel)은 ARC-015(tool-upgrade-tasks.md)에서 이미 생성됨 -- 참조 연결만 수행

**Step F-3**: 통합 테스트
- NPC 접근 -> 대화 -> 상점/업그레이드 전체 흐름 Play Mode 테스트
- 여행 상인 방문/출발 사이클 테스트
- 세이브/로드 후 NPC 상태 복원 확인

---

## Cross-references

| 문서 | 참조 내용 |
|------|-----------|
| `docs/architecture.md` | 마스터 기술 아키텍처 (프로젝트 구조, 시스템 계층) |
| `docs/systems/economy-system.md` 섹션 3 | 상점 시스템 디자인 (상점 종류, 영업시간, 인벤토리 규칙) |
| `docs/systems/economy-architecture.md` 섹션 4.3 | ShopData SO 필드 정의, ShopSystem 클래스 상세 |
| `docs/systems/tool-upgrade.md` 섹션 6 | 대장간 NPC 디자인 (상호작용 흐름, 판매 품목) |
| `docs/systems/tool-upgrade-architecture.md` | ToolUpgradeSystem 클래스 설계 |
| `docs/systems/inventory-system.md` | 인벤토리 시스템 (구매 아이템 수령 흐름) |
| `docs/systems/project-structure.md` | 프로젝트 구조 (폴더, 네임스페이스, 의존성, 씬 계층) |
| `docs/systems/time-season.md` 섹션 1.7 | 상점 영업시간, 요일 매핑 canonical |
| `docs/pipeline/data-pipeline.md` | SO 데이터 구조, 세이브/로드 구조 |
| `docs/content/npcs.md` (CON-003) | NPC 이름, 성격, 대사, 여행 상인 수치 canonical |
| `docs/content/traveler-npc.md` (CON-008c) | 여행 상인 "바람이" 아이템 목록, 계절별 재고, 친밀도 임계값, 대화 canonical |
| `docs/balance/traveler-economy.md` (BAL-005) | 여행 상인 희귀 아이템 ROI 분석, 권장 가격 canonical |

---

## Open Questions

1. [RESOLVED] **NPC 호감도 시스템**: 여행 상인(바람이)에 대해 친밀도 시스템이 CON-008c에서 설계 완료됨. TravelingMerchantData SO에 affinityThresholds, friendDiscountRate 등 필드 추가 (섹션 9.1). TravelingMerchantSaveData에 affinityPoints 필드 추가 (섹션 9.4). 다른 NPC(하나/철수/목이)에 대한 호감도 시스템은 별도 설계 필요.

2. [OPEN] **대화 시스템 확장 범위**: 현재 선형 + 선택지 구조로 설계했으나, 조건 분기(계절별, 호감도별 대사 변경)가 필요하면 DialogueNode에 조건 필드를 추가해야 한다. 복잡도와 구현 비용 간 트레이드오프 검토 필요.

3. [OPEN] **NPC 이동 AI**: 현재 NPC는 고정 위치에 서 있는 것으로 설계했다. NPC가 시간대에 따라 이동하는 패턴(아침에 가게 열기, 점심에 마을 광장 이동 등)을 넣을지. 구현 복잡도 대비 몰입감 향상 효과 분석 필요.

4. [OPEN] **목수 NPC의 BuildingManager 연동 상세**: 현재 목수 NPC는 시설 건설 의뢰 역할로 정의했으나, BuildingManager와의 구체적인 연동 흐름(건설 의뢰 -> 소요 일수 -> 완성 알림)은 별도 설계 필요.

5. [RESOLVED] **여행 상인 고유 아이템**: CON-003/CON-008c에서 8종의 독점 아이템 확정 (→ see docs/content/npcs.md 섹션 6.3). 단, ItemType/PriceCategory enum에 Consumable/Decoration 카테고리 추가 필요 (섹션 9.6 참조).

---

## Risks

1. [RISK] **NPC 모듈의 넓은 의존성**: NPC 모듈이 Core, Farm, Player, Economy, Building을 모두 참조한다. asmdef 순환 참조는 없으나(NPC를 참조하는 모듈은 UI뿐), 변경 영향 범위가 넓어 NPC 모듈 수정 시 다른 모듈 테스트가 필요할 수 있다.

2. [RISK] **MCP에서 DialogueData SO 에셋 생성 시 배열 필드 설정의 난이도**: DialogueData는 DialogueNode 배열 내에 DialogueChoice 배열을 포함하는 중첩 구조이다. MCP로 이 구조를 자동 생성할 때 직렬화된 배열의 중첩 참조가 제대로 설정되지 않을 수 있다. (-> see `docs/architecture.md` Risks 참조, 기존 SO 배열 필드 MCP 지원 범위 불확실 이슈)

3. [RISK] **기존 ShopSystem과의 통합 충돌**: 기존 economy-architecture.md의 ShopSystem은 단일 ShopData를 참조하는 구조이다. NPC별로 다른 ShopData를 동적으로 교체하려면 ShopSystem.Open()에 ShopData 파라미터를 추가해야 하며, 기존 인터페이스 수정이 필요하다.

4. [RISK] **여행 상인 재고 재현성**: 세이브/로드 시 여행 상인의 재고를 정확히 재현하려면 randomSeed 기반으로 동일한 재고를 재생성해야 한다. Random.seed 관리가 다른 시스템의 난수 사용과 충돌하지 않도록 독립 인스턴스(System.Random)를 사용해야 한다.

---

*이 문서는 NPC/상점 시스템의 기술 아키텍처 문서이다. NPC 클래스 구조, 대화 시스템, 이벤트 패턴, 의존성 구조는 이 문서를 정본으로 한다. NPC 콘텐츠(이름, 대사, 수치)는 `docs/content/npcs.md` (CON-003/CON-008)가 canonical이다.*

---

# Part III -- 추가 NPC 확장 아키텍처 (CON-008)

> 마을 상인, 농업 전문가, 여행 상인 확장 로직의 기술 아키텍처  
> 작성: Claude Code (Opus) | 2026-04-07

---

## 9. 여행 상인 (Traveling Merchant) 확장 로직

### 9.1 TravelingMerchantData (ScriptableObject)

기존 TravelingShopPoolData(섹션 2.3)가 아이템 풀을 정의하는 반면, TravelingMerchantData는 방문 스케줄, 방문 확률, 가격 정책 파라미터를 통합 관리하는 SO이다.

```csharp
// illustrative
namespace SeedMind.NPC.Data
{
    [CreateAssetMenu(fileName = "NewTravelingMerchantData", menuName = "SeedMind/TravelingMerchantData")]
    public class TravelingMerchantData : ScriptableObject
    {
        [Header("방문 스케줄")]
        public DayFlag visitDays;                     // 방문 요일 비트마스크 (→ see docs/content/npcs.md, CON-008)
        public int visitStartHour;                    // 방문 시작 시간 (→ see docs/content/npcs.md 섹션 6.2)
        public int visitEndHour;                      // 방문 종료 시간 (→ see docs/content/npcs.md 섹션 6.2)
        public int minPlayerLevel;                    // 등장 최소 플레이어 레벨 (→ see docs/content/npcs.md 섹션 6.2)

        [Header("계절별 방문 확률 보정")]
        // [OPEN] 현재 canonical(docs/content/npcs.md 섹션 6.2)은 등장 확률 100% 고정이다.
        // 이 필드들은 미래 확장용으로만 선언하며, 기본값은 1.0f(100%)로 설정해야 한다.
        // canonical을 변경하기 전에는 이 보정값으로 방문을 건너뛰는 로직을 활성화하면 안 된다.
        // → see docs/content/npcs.md 섹션 6.2 for 등장 확률 canonical
        public float springVisitChance;               // 봄 방문 확률 보정 (기본값 1.0, → see docs/content/npcs.md 섹션 6.2)
        public float summerVisitChance;               // 여름 (기본값 1.0, → see docs/content/npcs.md 섹션 6.2)
        public float autumnVisitChance;               // 가을 (기본값 1.0, → see docs/content/npcs.md 섹션 6.2)
        public float winterVisitChance;               // 겨울 (기본값 1.0, → see docs/content/npcs.md 섹션 6.2)

        [Header("아이템 풀 참조")]
        public TravelingShopPoolData defaultPool;     // 기본 아이템 풀 (섹션 2.3)
        public TravelingShopPoolData[] seasonalPools;  // 계절별 추가 풀 (4개: 봄/여름/가을/겨울)

        [Header("가격 파라미터 -- BAL-005")]
        public float rarePriceMin;                    // 희귀 아이템 가격 하한 배율 (→ see docs/systems/economy-system.md, BAL-005)
        public float rarePriceMax;                    // 희귀 아이템 가격 상한 배율 (→ see docs/systems/economy-system.md, BAL-005)
        public float priceVariancePerVisit;           // 방문마다 가격 변동폭 (→ see docs/systems/economy-system.md, BAL-005)

        [Header("친밀도 보상 파라미터 -- CON-008c")]
        // → see docs/content/traveler-npc.md 섹션 2.5 for 친밀도 단계별 보상 canonical
        public int[] affinityThresholds;              // 친밀도 단계 임계값 배열 (→ see docs/content/traveler-npc.md 섹션 2.5)
        public int regularBonusItemCount;             // Regular 단계 보상: 아이템 풀 추가 선택 수 (→ see docs/content/traveler-npc.md 섹션 2.5)
        public float friendDiscountRate;              // Friend 단계 보상: 전 아이템 할인율 (→ see docs/content/traveler-npc.md 섹션 2.5)
        public int friendBonusStockPerItem;           // Friend 단계 보상: 아이템당 재고 추가 수 (→ see docs/content/traveler-npc.md 섹션 2.5)

        [Header("재고 생성")]
        public int stockSeedBase;                     // 난수 시드 기반값 (게임 시작 시 고정)
    }
}
```

에셋 이름: `SO_TravelingMerchantConfig.asset`  
저장 경로: `Assets/_Project/Data/NPCs/`

### 9.2 계절별/주기별 방문 스케줄 (TimeManager 연동)

기존 TravelingMerchantScheduler(섹션 3.5)의 `CheckVisitSchedule` 로직을 확장하여, 계절별 방문 확률 보정과 특수 이벤트 방문을 지원한다.

```
확장된 CheckVisitSchedule(currentDay, currentDayOfWeek, currentSeason):

    1. DayFlag 기반 요일 판정 (기존 로직 유지)
       isVisitDay = (_merchantData.visitDays & (1 << currentDayOfWeek)) != 0

    2. 계절별 방문 확률 보정 (신규)
       seasonChance = GetSeasonVisitChance(currentSeason)
       // canonical(docs/content/npcs.md 섹션 6.2)은 등장 확률 100% 고정이다.
       // 현재 모든 seasonChance 기본값은 1.0f이어야 한다.
       // → see docs/content/npcs.md 섹션 6.2 for 등장 확률 canonical
       
       if isVisitDay:
           // 계절 확률 적용 (canonical이 1.0f이면 항상 true — 실질적으로 건너뛰지 않음)
           rng = new System.Random(_merchantData.stockSeedBase + currentDay)
           actualVisit = rng.NextDouble() < seasonChance

    3. 레벨 요건 확인
       if ProgressionManager.CurrentLevel < _merchantData.minPlayerLevel:
           actualVisit = false

    4. 시간대 체크 (visitStartHour ~ visitEndHour)
       // TimeManager.OnHourChanged 이벤트에서 호출
       if currentHour == visitStartHour && actualVisit:
           GenerateStock(playerLevel, currentSeason)
           SpawnMerchant()
       elif currentHour == visitEndHour && _isPresent:
           DespawnMerchant()

이벤트 구독:
    TimeManager.OnDayChanged  → CheckVisitSchedule() (요일/계절 판정)
    TimeManager.OnHourChanged → CheckVisitTimeWindow() (등장/퇴장 시간 판정)
```

### 9.3 희귀 아이템 풀 관리 (시드 기반 재현성)

여행 상인의 재고 생성은 결정론적(deterministic)이어야 한다. 동일한 게임 상태에서 동일한 재고가 생성되도록 시드 기반 난수를 사용한다.

```
GenerateStock(playerLevel, currentSeason) 확장:

    1. 결정론적 시드 생성
       seed = _merchantData.stockSeedBase
              ^ (currentYear * 1000)
              ^ (currentSeason * 100)
              ^ currentDay
       rng = new System.Random(seed)
       // System.Random 사용 (UnityEngine.Random과 독립)

    2. 풀 병합 (기본 + 계절)
       mergedPool = Merge(_merchantData.defaultPool, GetSeasonalPool(currentSeason))

    3. 후보 필터링
       validCandidates = mergedPool.candidates
           .Where(c => playerLevel >= c.minPlayerLevel)
           .Where(c => (c.availableSeasons & CurrentSeasonFlag) != 0)
           .ToList()

    4. 가중 랜덤 선택 (minItemCount ~ maxItemCount + 친밀도 보너스)
       // → see docs/content/npcs.md for minItemCount, maxItemCount 수치
       itemCount = rng.Next(minItemCount, maxItemCount + 1)
       
       // 친밀도 보상 반영 (→ see docs/content/traveler-npc.md 섹션 2.5)
       affinityLevel = GetAffinityLevel(playerAffinityPoints, _merchantData.affinityThresholds)
       if affinityLevel >= Regular:   // Regular 단계 이상
           itemCount += _merchantData.regularBonusItemCount  // 아이템 풀 추가 선택
       
       selectedItems = WeightedRandomSelect(validCandidates, itemCount, rng)

    5. 2주 쿨다운 적용
       // _recentItems: HashSet<string> — 직전 방문에서 판매한 아이템 ID
       selectedItems = selectedItems.Where(i => !_recentItems.Contains(i.itemId))

    6. 가격 산정 (BAL-005)
       foreach item in selectedItems:
           basePrice = item.priceData.basePrice
           variance = rng.NextDouble() * _merchantData.priceVariancePerVisit * 2
                      - _merchantData.priceVariancePerVisit
           finalPrice = Clamp(
               basePrice * (1.0 + variance),
               basePrice * _merchantData.rarePriceMin,
               basePrice * _merchantData.rarePriceMax
           )
           // → see docs/systems/economy-system.md, BAL-005 for 상하한 배율

    7. 재고 수량 결정 (친밀도 보너스 포함)
       foreach item in selectedItems:
           stock = rng.Next(item.stockMin, item.stockMax + 1)
           if affinityLevel >= Friend:   // Friend 단계 이상 (→ see docs/content/traveler-npc.md 섹션 2.5)
               stock += _merchantData.friendBonusStockPerItem

    8. 가격에 친밀도 할인 적용
       foreach item in selectedItems:
           if affinityLevel >= Friend:   // Friend 단계 이상 (→ see docs/content/traveler-npc.md 섹션 2.5)
               item.finalPrice = (int)(item.finalPrice * (1.0 - _merchantData.friendDiscountRate))

    9. _currentStock에 저장, _recentItems 갱신
```

[RISK] **시드 기반 난수와 세이브/로드 동기화**: 재고 생성 시드는 게임 일수/계절/연도의 조합이므로, 세이브 파일에 시드 자체를 저장하지 않고도 재현 가능하다. 다만 `_recentItems`(2주 쿨다운 목록)는 세이브에 포함해야 한다. TravelingMerchantSaveData에 `recentItemIds` 필드 추가 필요.

### 9.4 TravelingMerchantSaveData 확장

```csharp
// illustrative
namespace SeedMind.NPC.Data
{
    [System.Serializable]
    public class TravelingMerchantSaveData
    {
        public bool isPresent;                    // 현재 방문 중 여부
        public int randomSeed;                    // 재고 생성 시드 (세이브 시점의 시드)
        public string[] currentStockItemIds;      // 현재 재고 아이템 ID 목록
        public int[] currentStockQuantities;      // 현재 재고 수량
        public int[] currentStockPrices;          // 현재 재고 가격 (BAL-005 가격 변동 반영)
        public string[] recentItemIds;            // 2주 쿨다운 아이템 ID 목록
        public int affinityPoints;                // 바람이 친밀도 포인트 (→ see docs/content/traveler-npc.md 섹션 2.5)
    }
}
```

JSON 스키마 확장:

```json
{
    "npc": {
        "travelingMerchant": {
            "isPresent": false,
            "randomSeed": 42,
            "currentStockItemIds": [],
            "currentStockQuantities": [],
            "currentStockPrices": [],
            "recentItemIds": [],
            "affinityPoints": 0
        }
    }
}
```

PATTERN-005 준수: JSON 스키마와 C# 클래스의 필드 7개 (isPresent, randomSeed, currentStockItemIds, currentStockQuantities, currentStockPrices, recentItemIds, affinityPoints)가 동일하다.

> **주의**: 섹션 7.2의 기존 TravelingMerchantSaveData(4필드)를 이 확장 버전(7필드)으로 교체한다. 기존 필드에 `currentStockPrices`, `recentItemIds`, `affinityPoints` 3개가 추가되었다.

### 9.5 가격 반영 경로 (BAL-005 → 게임 내 반영)

BAL-005에서 확정된 가격 파라미터가 게임에 반영되는 데이터 흐름:

```
[데이터 설정 단계]
1. TravelingShopPoolData SO (섹션 2.3)
   - 각 TravelingShopCandidate.priceData → PriceData SO 참조
   - PriceData.basePrice에 아이템별 기본 가격 설정
     (→ see docs/content/npcs.md 섹션 6.3 for 아이템별 가격 canonical)

2. TravelingMerchantData SO (섹션 9.1)
   - rarePriceMin, rarePriceMax → 가격 변동 상하한 배율
   - priceVariancePerVisit → 방문별 변동폭
   - friendDiscountRate → 친밀도 할인율
     (→ see docs/systems/economy-system.md, BAL-005 for 배율 수치)

[런타임 단계]
3. TravelingMerchantScheduler.GenerateStock() (섹션 9.3)
   - 시드 기반 난수로 가격 변동 적용
   - 친밀도 단계별 할인/재고 보너스 적용
   - 최종 가격을 _currentStock에 저장

4. ShopUI
   - _currentStock에서 아이템 목록/가격/재고 표시
   - 구매 시 EconomyManager.ProcessPurchase() 호출

[가격 조정 시 수정 대상]
- 개별 아이템 기본가: PriceData SO (Assets/_Project/Data/Prices/)
- 가격 변동 범위: TravelingMerchantData SO (Assets/_Project/Data/NPCs/)
- 친밀도 할인율: TravelingMerchantData SO의 friendDiscountRate
- 아이템 풀 구성: TravelingShopPoolData SO (Assets/_Project/Data/NPCs/)
```

### 9.6 여행 상인 아이템의 enum 분류 검토

[RISK] **ItemType/PriceCategory enum 누락**: 여행 상인 독점 아이템 중 에너지 토닉(`item_energy_tonic`), 성장 촉진제(`item_growth_accel`), 행운의 부적(`item_lucky_charm`)은 소비 아이템(Consumable)이다. 현재 enum 분류 상태:

| 아이템 | ItemType (data-pipeline.md) | PriceCategory (economy-architecture.md) | 문제 |
|--------|---------------------------|----------------------------------------|------|
| 만능 비료 | `Fertilizer` | `Fertilizer` | 적합 |
| 비계절 씨앗 | `Seed` | `Seed` | 적합 |
| 정원 등불, 풍향계 | `Special` | 해당 없음 | PriceCategory에 `Decoration` 누락 |
| 에너지 토닉 | `Special` | 해당 없음 | `Consumable` 카테고리 필요 |
| 성장 촉진제 | `Special` | 해당 없음 | `Consumable` 카테고리 필요 |
| 행운의 부적 | `Special` | 해당 없음 | `Consumable` 카테고리 필요 |
| 온실 전용 씨앗 | `Seed` | `Seed` | 적합 |

**권장 조치**: 
1. `ItemType` enum에 `Consumable` 값 추가 (data-pipeline.md)
2. `PriceCategory` enum에 `Consumable`, `Decoration` 값 추가 (economy-architecture.md 섹션 4.2)
3. 여행 상인 아이템은 구매 전용이므로 `SupplyCategory` 확장은 불필요 (수급 보정 대상이 아님)

### 9.7 SupplyCategory 연동 분석

여행 상인의 희귀 아이템(만능 비료, 특수 씨앗, 소비 아이템 등)은 모두 **구매 전용**이다. 플레이어가 NPC에게 되팔 수 없으므로 수급 보정 시스템(`PriceFluctuationSystem.GetSupplyMultiplier()`)의 영향을 받지 않는다.

따라서 FIX-044에서 추가된 `SupplyCategory` enum(Crop/AnimalProduct/Fish/ProcessedGoods)에 `RareItem` 등 별도 카테고리를 추가할 필요가 없다. 여행 상인 아이템의 `PriceData.supplyCategory`는 설정하지 않거나, ProcessedGoods(수급 보정 면제)로 설정하면 된다.

> **단, 향후 여행 상인 아이템을 NPC에게 되팔 수 있도록 변경할 경우**, SupplyCategory 확장이 필요하다. 이 시점에서 `Consumable` 카테고리(수급 보정 면제)를 SupplyCategory에도 추가하는 것을 검토해야 한다.

---

## 10. 농업 전문가 NPC 힌트 시스템

### 10.1 설계 목표

농업 전문가 NPC는 상점 기능이 없는 대신, 플레이어의 현재 진행 상태에 맞는 농업 힌트를 제공하는 특수 NPC이다. 기존 ContextHintSystem(-> see `docs/systems/tutorial-architecture.md` 섹션 8)이 자동 팝업 힌트를 담당하는 반면, 농업 전문가는 **플레이어가 능동적으로 찾아가서 조언을 받는** 메카닉이다.

**기존 시스템과의 차별점**:

| 항목 | ContextHintSystem (튜토리얼) | NPCHintSystem (농업 전문가) |
|------|----------------------------|---------------------------|
| 트리거 | 자동 (게임 상태 감지) | 수동 (플레이어가 NPC에게 말 걸기) |
| 표시 방식 | 토스트/오버레이 팝업 | 대화 UI 내 힌트 텍스트 |
| 반복 빈도 | 쿨다운 기반 자동 반복 | 방문할 때마다 1회 제공 |
| 해금 조건 | 튜토리얼 단계 기반 | 플레이어 레벨/진행도 기반 |
| 콘텐츠 깊이 | 간단한 팁 (1~2줄) | 상세한 조언 (3~5줄, 전략적 힌트 포함) |

### 10.2 NPCHintData (ScriptableObject)

```csharp
// illustrative
namespace SeedMind.NPC.Data
{
    [CreateAssetMenu(fileName = "NewNPCHint", menuName = "SeedMind/NPCHintData")]
    public class NPCHintData : ScriptableObject
    {
        [Header("식별")]
        public string hintId;                       // "AGRI_HINT_FirstCrop"
        public int priority;                        // 높을수록 우선 표시 (→ see docs/content/npcs.md, CON-008)

        [Header("발동 조건")]
        public HintConditionType conditionType;     // enum: 조건 유형
        public string conditionParam;               // 조건 파라미터 (예: "crop_potato")
        public int requiredPlayerLevel;             // 최소 레벨 요건 (→ see docs/balance/progression-curve.md)
        public bool requiresOnce;                   // true이면 한 번만 표시

        [Header("힌트 내용")]
        [TextArea(3, 8)]
        public string hintText;                     // 힌트 텍스트 (→ see docs/content/npcs.md, CON-008)
        public string hintCategory;                 // "작물", "시설", "경제", "계절" 등

        [Header("보상 (선택적)")]
        public ScriptableObject rewardItem;         // 힌트 보상 아이템 (null이면 보상 없음)
        public int rewardQuantity;                  // 보상 수량 // → see docs/content/npcs.md, CON-008
    }
}
```

### 10.3 HintConditionType enum

```csharp
// illustrative
namespace SeedMind.NPC.Data
{
    public enum HintConditionType
    {
        // --- 작물 관련 ---
        FirstPlanting        = 0,   // 처음 작물 심기 (한 번도 심은 적 없음)
        FirstHarvest         = 1,   // 첫 수확 완료
        CropTypeUnlocked     = 2,   // 특정 작물 해금 (conditionParam = cropId)
        SeasonalCropMissing  = 3,   // 해당 계절 작물을 하나도 안 심었을 때

        // --- 시설 관련 ---
        FacilityBuilt        = 10,  // 특정 시설 건설 완료 (conditionParam = facilityId)
        FacilityNotBuilt     = 11,  // 특정 시설 미건설 (conditionParam = facilityId)
        ProcessingUnlocked   = 12,  // 가공 시스템 해금됨

        // --- 경제 관련 ---
        GoldThreshold        = 20,  // 골드가 특정 임계값 이상 (conditionParam = threshold string)
        FirstSale            = 21,  // 첫 판매 완료
        PriceSurge           = 22,  // 특정 작물의 가격이 급등 중

        // --- 계절/시간 관련 ---
        SeasonStart          = 30,  // 특정 계절 시작 (conditionParam = "Spring"/"Summer"/...)
        WinterApproaching    = 31,  // 가을 후반 (겨울 준비 힌트)

        // --- 진행도 관련 ---
        LevelReached         = 40,  // 특정 레벨 도달 (conditionParam = level string)
        QuestCompleted       = 41,  // 특정 퀘스트 완료 (conditionParam = questId)
        ToolUpgraded         = 42   // 특정 도구 업그레이드 완료 (conditionParam = toolType)
    }
}
```

### 10.4 NPCHintSystem 클래스

```
┌──────────────────────────────────────────────────────────────┐
│                 NPCHintSystem (MonoBehaviour)                  │
│──────────────────────────────────────────────────────────────│
│  [설정 참조]                                                   │
│  - _allHints: NPCHintData[]           (전체 힌트 데이터 SO)   │
│  - _expertNPCData: NPCData            (농업 전문가 NPC 참조)  │
│                                                              │
│  [외부 참조]                                                   │
│  - _progressionManager: ProgressionManager                   │
│  - _farmGrid: FarmGrid                                       │
│  - _buildingManager: BuildingManager                         │
│  - _economyManager: EconomyManager                           │
│  - _timeManager: TimeManager                                 │
│                                                              │
│  [상태]                                                       │
│  - _displayedHintIds: HashSet<string>  (이미 표시된 일회성 힌트)│
│  - _lastHintId: string                 (마지막 표시 힌트)     │
│                                                              │
│  [메서드]                                                     │
│  + RequestHint(): NPCHintData          (현재 상태에서 최적 힌트 반환)│
│  + EvaluateCondition(NPCHintData): bool (조건 평가)          │
│  + MarkAsDisplayed(string hintId): void (일회성 힌트 기록)    │
│  + ClaimReward(NPCHintData): bool      (보상 수령 처리)      │
│  + GetSaveData(): NPCHintSaveData                            │
│  + LoadSaveData(NPCHintSaveData): void                       │
│                                                              │
│  [힌트 선택 알고리즘]                                           │
│  RequestHint():                                              │
│    1. _allHints에서 조건 충족 힌트 필터링                      │
│    2. requiresOnce && 이미 표시된 힌트 제외                    │
│    3. requiredPlayerLevel 미달 힌트 제외                       │
│    4. 남은 힌트 중 priority 최고값 선택                        │
│    5. 동일 priority 시 hintId 알파벳 순서                     │
└──────────────────────────────────────────────────────────────┘
```

### 10.5 힌트 조건 평가 흐름

```
EvaluateCondition(hintData):

    switch hintData.conditionType:

        case FirstPlanting:
            return FarmGrid.TotalCropsEverPlanted == 0

        case FirstHarvest:
            return FarmGrid.TotalHarvests == 0

        case CropTypeUnlocked:
            return ProgressionManager.IsCropUnlocked(hintData.conditionParam)
                   && !FarmGrid.HasEverPlanted(hintData.conditionParam)

        case SeasonalCropMissing:
            return TimeManager.CurrentSeason == ParseSeason(hintData.conditionParam)
                   && FarmGrid.GetActiveCropCount() == 0

        case FacilityBuilt:
            return BuildingManager.HasBuilding(hintData.conditionParam)

        case FacilityNotBuilt:
            return !BuildingManager.HasBuilding(hintData.conditionParam)

        case GoldThreshold:
            return EconomyManager.CurrentGold >= int.Parse(hintData.conditionParam)

        case SeasonStart:
            return TimeManager.CurrentSeason.ToString() == hintData.conditionParam
                   && TimeManager.CurrentDay <= 3  // 계절 시작 3일 이내

        case WinterApproaching:
            return TimeManager.CurrentSeason == Season.Autumn
                   && TimeManager.CurrentDay >= 21  // 가을 21일 이후
                   // → see docs/systems/time-season.md for 계절별 일수

        case LevelReached:
            return ProgressionManager.CurrentLevel >= int.Parse(hintData.conditionParam)

        ... (기타 조건 유형)
```

### 10.6 progression-curve.md 연동 (레벨 기반 힌트 해금)

농업 전문가의 힌트는 플레이어 레벨에 따라 해금된다. 해금 레벨은 NPCHintData.requiredPlayerLevel 필드로 지정하며, 구체적인 레벨 값은 progression-curve.md가 canonical이다.

```
힌트 해금 구조:

    레벨 1~2: 기초 농업 힌트 (물 주기, 씨앗 심기, 수확 방법)
    레벨 3~4: 중급 힌트 (비료 활용, 품질 향상, 계절 전략)
    레벨 5~6: 고급 힌트 (가공소 활용, 가격 변동 활용, 효율적 농장 배치)
    레벨 7+:  전문가 힌트 (희귀 작물 전략, 수익 극대화, 겨울 온실 전략)

    (→ see docs/balance/progression-curve.md for 정확한 레벨별 해금 목록)
```

### 10.7 NPCHintSaveData

```csharp
// illustrative
namespace SeedMind.NPC.Data
{
    [System.Serializable]
    public class NPCHintSaveData
    {
        public string[] displayedHintIds;     // 이미 표시된 일회성 힌트 ID 목록
        public string lastHintId;             // 마지막으로 표시된 힌트 ID
    }
}
```

JSON 스키마:

```json
{
    "npc": {
        "hints": {
            "displayedHintIds": ["AGRI_HINT_FirstCrop", "AGRI_HINT_FirstHarvest"],
            "lastHintId": "AGRI_HINT_FertilizerTip"
        }
    }
}
```

PATTERN-005 준수: JSON 2필드 (displayedHintIds, lastHintId) = C# 2필드 동일.

---

## 11. NPC 운영 시간 스케줄 시스템

### 11.1 설계 목표

기존 NPCData에 `openHour`/`closeHour`/`closedDays` 필드가 있으나(섹션 2.1), 이를 통합적으로 평가하는 로직이 분산되어 있다. 운영 시간 스케줄 시스템은 NPC별 영업 상태를 TimeManager 이벤트와 연동하여 일관되게 관리한다.

### 11.2 OperatingSchedule 구조

NPCData에 포함된 기존 필드를 구조체로 그룹화하여 가독성과 확장성을 높인다.

```csharp
// illustrative
namespace SeedMind.NPC.Data
{
    [System.Serializable]
    public struct OperatingSchedule
    {
        public int openHour;                          // 영업 시작 시간 (→ see docs/systems/economy-system.md 섹션 3.2)
        public int closeHour;                         // 영업 종료 시간 (→ see docs/systems/economy-system.md 섹션 3.2)
        public DayFlag closedDays;                    // 휴무 요일 비트마스크 (→ see docs/systems/economy-system.md 섹션 3.2)
        public bool immuneToWeather;                  // 악천후에도 영업 여부 (예: 대장간=true)
        public DayFlag specialOpenDays;               // 특별 영업일 (평소 휴무일이지만 영업하는 날)
    }
}
```

NPCData 필드 리팩터링 (기존 openHour/closeHour/closedDays를 OperatingSchedule 구조체로 교체):

```csharp
// illustrative — NPCData 필드 변경 부분만
namespace SeedMind.NPC.Data
{
    public class NPCData : ScriptableObject
    {
        // ... 기존 필드 유지 ...

        [Header("위치/스케줄")]
        public Vector3 defaultPosition;               // (기존)
        public OperatingSchedule schedule;             // 기존 openHour/closeHour/closedDays를 대체
        // openHour, closeHour, closedDays 개별 필드는 제거

        // ... 나머지 필드 유지 ...
    }
}
```

### 11.3 OperatingScheduleEvaluator (유틸리티 클래스)

```csharp
// illustrative
namespace SeedMind.NPC
{
    public static class OperatingScheduleEvaluator
    {
        /// <summary>
        /// NPC의 현재 영업 상태를 판정한다.
        /// </summary>
        public static NPCActivityState Evaluate(
            OperatingSchedule schedule,
            int currentHour,
            int currentDayOfWeek,       // 0=Monday ~ 6=Sunday
            bool isStormOrBlizzard)     // 폭풍/폭설 여부 (→ see docs/systems/time-season.md 섹션 3.4)
        {
            // 1. 악천후 체크
            if (isStormOrBlizzard && !schedule.immuneToWeather)
                return NPCActivityState.WeatherClosed;

            // 2. 휴무일 체크
            DayFlag todayFlag = (DayFlag)(1 << currentDayOfWeek);
            bool isClosedDay = (schedule.closedDays & todayFlag) != 0;
            bool isSpecialOpen = (schedule.specialOpenDays & todayFlag) != 0;

            if (isClosedDay && !isSpecialOpen)
                return NPCActivityState.Closed;

            // 3. 영업 시간 체크
            if (currentHour < schedule.openHour || currentHour >= schedule.closeHour)
                return NPCActivityState.Closed;

            return NPCActivityState.Active;
        }

        /// <summary>
        /// 다음 영업 시작까지 남은 시간(게임 시간 기준)을 반환한다.
        /// UI에서 "X시간 후 영업 시작" 표시에 사용.
        /// </summary>
        public static int GetHoursUntilOpen(
            OperatingSchedule schedule,
            int currentHour,
            int currentDayOfWeek)
        {
            // 현재 시간이 closeHour 이후이면 다음 영업일의 openHour까지 계산
            // 현재 시간이 openHour 이전이면 오늘 openHour까지 계산
            // 휴무일이면 다음 영업일까지 계산
            // ... (구현 로직은 Phase 2에서 상세화)
            return 0; // → placeholder
        }
    }
}
```

### 11.4 TimeManager 이벤트 구독 방식

NPCManager가 TimeManager 이벤트를 구독하여 모든 NPC의 영업 상태를 일괄 갱신한다.

```
NPCManager 이벤트 구독 흐름:

    [초기화]
    NPCManager.Initialize():
        TimeManager.OnHourChanged += OnHourChanged
        WeatherSystem.OnWeatherChanged += OnWeatherChanged

    [매 시간 변경]
    OnHourChanged(int newHour):
        int dayOfWeek = TimeManager.GetDayOfWeek()  // 0~6
        bool isStorm = WeatherSystem.IsStormOrBlizzard()

        foreach npc in _activeNPCs.Values:
            NPCActivityState newState = OperatingScheduleEvaluator.Evaluate(
                npc.NPCData.schedule, newHour, dayOfWeek, isStorm
            )

            if newState != npc.CurrentState:
                npc.SetState(newState)
                NPCEvents.RaiseNPCStateChanged(npc.NPCData.npcId, newState)

    [날씨 변경]
    OnWeatherChanged(WeatherType newWeather):
        // 폭풍/폭설 시작/종료 시 즉시 재평가
        int currentHour = TimeManager.CurrentHour
        int dayOfWeek = TimeManager.GetDayOfWeek()
        bool isStorm = (newWeather == WeatherType.Storm || newWeather == WeatherType.Blizzard)

        foreach npc in _activeNPCs.Values:
            // ... 동일 Evaluate 로직
```

### 11.5 비영업 시간 UI 처리

비영업 시간에 NPC에게 접근하면 다음과 같이 처리한다.

```
NPCController.Interact(player) 확장:

    if _currentState == NPCActivityState.Active:
        // 기존 흐름: 인사 대화 → 서비스 메뉴
        StartDialogue(_npcData.greetingDialogue)

    elif _currentState == NPCActivityState.Closed:
        // 비영업 시간 대화
        StartDialogue(_npcData.closedDialogue)
        // closedDialogue 내에서 다음 영업 시간 표시
        // → "내일 {openHour}시에 다시 오세요"

    elif _currentState == NPCActivityState.WeatherClosed:
        // 악천후 임시 마감 대화
        StartDialogue(_npcData.weatherClosedDialogue)  // 신규 필드
        // → "폭풍이 지나면 다시 열게요"

    elif _currentState == NPCActivityState.Away:
        // 부재 (여행 상인 미방문)
        // NPC GameObject가 비활성이므로 인터랙션 불가
```

**NPCData 신규 필드**:

```csharp
// NPCData에 추가되는 대화 참조 필드 (섹션 2.1 확장)
[Header("대화 -- 확장 (CON-008)")]
public DialogueData weatherClosedDialogue;    // 악천후 임시 마감 시 대화
```

### 11.6 NPC 시각적 상태 표시

| NPCActivityState | 시각적 표시 | UI 힌트 |
|------------------|------------|---------|
| Active | NPC 기본 외형, 인터랙션 아이콘(말풍선) 표시 | 없음 |
| Closed | NPC 외형 변경 없음, 인터랙션 아이콘 회색 처리 | "영업 종료" 라벨 |
| WeatherClosed | NPC가 실내로 이동 (또는 비활성), 문 닫힘 표시 | "폭풍으로 임시 마감" 라벨 |
| Away | NPC GameObject 비활성 | 빈 좌판/빈 가판대만 표시 |

NPCController에서 상태 변경 시 시각 업데이트:

```
NPCController.SetState(NPCActivityState state):
    _currentState = state

    switch state:
        case Active:
            _interactionIcon.SetActive(true)
            _interactionIcon.color = Color.white
            _closedLabel.SetActive(false)

        case Closed:
            _interactionIcon.SetActive(true)
            _interactionIcon.color = Color.gray
            _closedLabel.SetActive(true)
            _closedLabel.text = "영업 종료"    // → see docs/content/npcs.md for 정확한 문구

        case WeatherClosed:
            _interactionIcon.SetActive(false)
            _closedLabel.SetActive(true)
            _closedLabel.text = "임시 마감"    // → see docs/content/npcs.md for 정확한 문구

        case Away:
            gameObject.SetActive(false)
```

---

## 12. NPCEvents 확장 (CON-008)

기존 NPCEvents(섹션 4)에 추가 NPC 관련 이벤트를 추가한다.

```csharp
// illustrative — 기존 NPCEvents에 추가
namespace SeedMind.NPC
{
    public static partial class NPCEvents
    {
        // --- 농업 전문가 힌트 (CON-008) ---
        public static event Action<string, NPCHintData> OnHintProvided;       // npcId, hintData
        public static event Action<string, NPCHintData> OnHintRewardClaimed;  // npcId, hintData

        // --- NPC 스케줄 (CON-008) ---
        public static event Action<string, int> OnNPCOpeningSoon;             // npcId, minutesUntilOpen (UI 알림용)

        // --- 발행 메서드 ---
        internal static void RaiseHintProvided(string npcId, NPCHintData data)
            => OnHintProvided?.Invoke(npcId, data);
        internal static void RaiseHintRewardClaimed(string npcId, NPCHintData data)
            => OnHintRewardClaimed?.Invoke(npcId, data);
        internal static void RaiseNPCOpeningSoon(string npcId, int minutes)
            => OnNPCOpeningSoon?.Invoke(npcId, minutes);
    }
}
```

### 12.1 확장 이벤트 소비 매핑

| 이벤트 | 발행자 | 소비자 | 용도 |
|--------|--------|--------|------|
| `OnHintProvided` | NPCHintSystem | DialogueUI, AchievementSystem | 힌트 대화 표시, 업적 추적 |
| `OnHintRewardClaimed` | NPCHintSystem | InventoryManager, NotificationManager | 보상 아이템 지급, 알림 표시 |
| `OnNPCOpeningSoon` | NPCManager | NotificationManager | "하나의 가게가 곧 열립니다" 알림 |

---

## 13. 프로젝트 구조 확장 (CON-008)

### 13.1 추가 파일

기존 폴더 구조(섹션 6.1)에 다음 파일이 추가된다.

```
Assets/_Project/Scripts/NPC/
    ├── ... (기존 파일)
    ├── NPCHintSystem.cs               # 농업 전문가 힌트 판정 (CON-008)
    ├── OperatingScheduleEvaluator.cs   # 영업 시간 판정 유틸리티 (CON-008)
    └── Data/
        ├── ... (기존 파일)
        ├── TravelingMerchantData.cs    # 여행 상인 스케줄/가격 설정 SO (CON-008)
        ├── NPCHintData.cs             # 농업 전문가 힌트 SO (CON-008)
        ├── HintConditionType.cs       # 힌트 조건 타입 enum (CON-008)
        ├── OperatingSchedule.cs       # 영업 시간 구조체 (CON-008)
        └── NPCHintSaveData.cs         # 힌트 세이브 데이터 (CON-008)

Assets/_Project/Data/
    ├── NPCs/
    │   ├── ... (기존 에셋)
    │   ├── SO_NPC_VillageMerchant.asset       # 마을 상인 NPC 데이터 (CON-008)
    │   ├── SO_NPC_AgricultureExpert.asset      # 농업 전문가 NPC 데이터 (CON-008)
    │   └── SO_TravelingMerchantConfig.asset   # 여행 상인 방문 설정 (CON-008)
    ├── Hints/                                  # 힌트 데이터 SO 에셋 (CON-008 신규 폴더)
    │   ├── SO_Hint_FirstCrop.asset
    │   ├── SO_Hint_FirstHarvest.asset
    │   ├── SO_Hint_FertilizerTip.asset
    │   └── ...
    └── Dialogues/
        ├── ... (기존 에셋)
        ├── SO_Dlg_Greeting_VillageMerchant.asset
        ├── SO_Dlg_Greeting_AgricultureExpert.asset
        └── SO_Dlg_WeatherClosed_*.asset        # 악천후 마감 대화 (CON-008)
```

### 13.2 NPCSaveData 통합 확장

기존 NPCSaveData(섹션 7.1)에 힌트 세이브 데이터를 포함한다.

```csharp
// illustrative — 섹션 7.1 교체
namespace SeedMind.NPC.Data
{
    [System.Serializable]
    public class NPCSaveData
    {
        public TravelingMerchantSaveData travelingMerchant;  // 섹션 9.4 확장 버전
        public NPCHintSaveData hints;                         // 섹션 10.7 (CON-008)
        // 향후 확장: NPC 호감도, 마을 상인 재고 상태 등
    }
}
```

---

## 14. MCP 구현 태스크 요약 (CON-008 확장분)

기존 Phase A~F(섹션 8)에 추가.

### Phase G: 여행 상인 확장

**Step G-1**: TravelingMerchantData SO 작성
- `Scripts/NPC/Data/TravelingMerchantData.cs` 생성
- SO 에셋 `SO_TravelingMerchantConfig.asset` 생성, 필드 설정 (수치는 -> see docs/content/npcs.md, CON-008)

**Step G-2**: TravelingMerchantScheduler 확장
- CheckVisitSchedule에 계절별 확률 보정 로직 추가
- GenerateStock에 시드 기반 재현성 / 2주 쿨다운 / BAL-005 가격 범위 로직 추가
- TravelingMerchantSaveData를 7필드 확장 버전으로 교체 (섹션 9.4 참조: isPresent/randomSeed/currentStockItemIds/currentStockQuantities/currentStockPrices/recentItemIds/affinityPoints)

### Phase H: 농업 전문가 힌트 시스템

**Step H-1**: 힌트 데이터 구조 생성
- `Scripts/NPC/Data/NPCHintData.cs`, `HintConditionType.cs`, `NPCHintSaveData.cs` 작성

**Step H-2**: NPCHintSystem 생성
- `Scripts/NPC/NPCHintSystem.cs` 작성
- 힌트 조건 평가 로직 구현
- NPCEvents에 OnHintProvided/OnHintRewardClaimed 이벤트 추가

**Step H-3**: 힌트 SO 에셋 생성
- `Data/Hints/` 폴더 생성
- 레벨별 힌트 SO 에셋 생성 (수치는 -> see docs/content/npcs.md, CON-008)

**Step H-4**: 농업 전문가 NPC 배치
- SO_NPC_AgricultureExpert.asset 생성
- SCN_Farm에 NPC_AgricultureExpert 오브젝트 배치

### Phase I: 운영 시간 스케줄 시스템

**Step I-1**: OperatingSchedule 구조 생성
- `Scripts/NPC/Data/OperatingSchedule.cs` 구조체 작성
- `Scripts/NPC/OperatingScheduleEvaluator.cs` 유틸리티 클래스 작성

**Step I-2**: NPCData 리팩터링
- 기존 openHour/closeHour/closedDays 필드를 OperatingSchedule 구조체로 교체
- 기존 NPC SO 에셋의 데이터 마이그레이션

**Step I-3**: NPCManager 영업 상태 갱신 로직 통합
- NPCManager.OnHourChanged에서 OperatingScheduleEvaluator.Evaluate 호출
- NPCController.SetState에 시각적 상태 표시 로직 추가
- weatherClosedDialogue 필드 추가 및 대화 데이터 에셋 생성

### Phase J: 마을 상인 NPC 배치

**Step J-1**: 마을 상인 NPC 생성
- SO_NPC_VillageMerchant.asset 생성
- SCN_Farm에 NPC_VillageMerchant 오브젝트 배치
- ShopData 에셋 연결

### Phase K: 통합 테스트

**Step K-1**: 여행 상인 확장 테스트
- 계절별 방문 확률 보정 → Play Mode 테스트
- 시드 기반 재고 재현성 → 세이브/로드 후 동일 재고 확인
- BAL-005 가격 상하한 범위 확인

**Step K-2**: 농업 전문가 테스트
- 레벨별 힌트 해금 → 레벨 변경 후 힌트 변화 확인
- 일회성 힌트 표시 후 재방문 시 미표시 확인
- 보상 아이템 지급 확인

**Step K-3**: 운영 시간 테스트
- 영업 시작/종료 시간에 NPC 상태 전환 확인
- 휴무일 접근 시 closedDialogue 재생 확인
- 폭풍 중 weatherClosedDialogue 재생 확인 (immuneToWeather NPC 제외)

---

## Cross-references (CON-008 추가분)

| 문서 | 참조 내용 |
|------|-----------|
| `docs/content/npcs.md` (CON-003/CON-008) | 추가 NPC 콘텐츠 canonical (마을 상인/농업 전문가/여행 상인 확장 수치) |
| `docs/systems/economy-system.md` 섹션 3.2 | 영업시간, 휴무일 canonical |
| `docs/systems/economy-system.md` (BAL-005) | 희귀 아이템 가격 상하한 정책 |
| `docs/balance/progression-curve.md` | 레벨별 해금 목록 (힌트 해금 레벨 canonical) |
| `docs/systems/time-season.md` 섹션 3.4 | 폭풍/폭설 조건 (WeatherClosed 판정 기준) |
| `docs/systems/tutorial-architecture.md` 섹션 8 | ContextHintSystem (기존 자동 힌트 — NPCHintSystem과 역할 분리) |
| `docs/systems/save-load-architecture.md` | GameSaveData 통합 루트에 NPCSaveData 확장 반영 |

---

## Open Questions (CON-008 추가분)

6. [OPEN] **농업 전문가 힌트 보상의 범위**: 현재 힌트 보상으로 아이템(특수 씨앗 등)을 지급하는 구조이나, 보상 종류(골드, XP, 아이템)를 다양화할지. 다양화 시 NPCHintData에 rewardType enum 추가 필요.

7. [OPEN] **마을 상인의 고가 매입 메카닉**: 마을 상인이 잡화 상인보다 특정 가공품을 더 비싸게 매입하는 구조의 구체적인 가격 배율은 CON-008 콘텐츠 확정 후 economy-system.md에 반영 필요.

8. [OPEN] **OperatingSchedule의 계절별 영업시간 변동**: 현재는 연중 고정 영업시간이나, 계절별로 영업시간이 달라지는 NPC(예: 여름에 더 오래 영업)를 지원할지. 지원 시 OperatingSchedule에 seasonalOverrides 필드 추가 필요.

9. [OPEN] **여행 상인 아이템 재판매 가능 여부**: 여행 상인에서 구매한 소비 아이템(에너지 토닉 등)을 다른 NPC에게 되팔 수 있는지 결정 필요. 되팔기 허용 시 SupplyCategory 확장 및 되팔기 가격 정책 수립 필요.

---

## Risks (CON-008 추가분)

5. [RISK] **NPCData 리팩터링 영향 범위**: openHour/closeHour/closedDays 개별 필드를 OperatingSchedule 구조체로 교체하면, 기존 MCP 태스크(Phase C)에서 생성한 NPC SO 에셋의 직렬화 데이터가 깨질 수 있다. Phase I 실행 시 기존 에셋 데이터 마이그레이션 스크립트가 필요할 수 있다.

6. [RISK] **NPCHintSystem의 넓은 의존성**: 힌트 조건 평가를 위해 FarmGrid, BuildingManager, EconomyManager, TimeManager, ProgressionManager 등 거의 모든 주요 시스템을 참조한다. 인터페이스 추상화 없이 직접 참조 시, 이들 시스템의 API 변경이 NPCHintSystem에 연쇄 영향을 미친다.

7. [RISK] **여행 상인 시드 기반 재현성과 게임 밸런스 패치 간의 충돌**: 시드 기반으로 재고를 결정하면, 후속 밸런스 패치(아이템 풀 변경, 확률 조정)가 기존 세이브 파일의 재고 재현성을 깨뜨릴 수 있다. 세이브 파일에 현재 재고를 직접 저장하는 방식(currentStockItemIds/Quantities/Prices)으로 이를 완화하지만, 아이템 정의 자체가 변경되면 세이브 호환성 문제가 남는다.
