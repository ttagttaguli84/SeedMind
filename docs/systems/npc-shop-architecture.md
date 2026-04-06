# NPC/상점 시스템 기술 아키텍처

> NPC 데이터 구조, 상점 시스템 확장, 대화 시스템, 대장간 연계, 여행 상인 스케줄러의 클래스 설계 및 MCP 구현 계획  
> 작성: Claude Code (Opus) | 2026-04-06  
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

NPC 이름, 성격, 외형 등 콘텐츠 상세는 (-> see `docs/content/npcs.md`, CON-003)에서 정의한다.

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
        public NPCType npcType;                   // enum: GeneralMerchant, Blacksmith, Carpenter, TravelingMerchant
        public Sprite portrait;                   // 대화창 초상화

        [Header("위치/스케줄")]
        public Vector3 defaultPosition;           // 기본 위치 (→ see docs/content/npcs.md, CON-003)
        public int openHour;                      // 활동 시작 시간 (→ see docs/systems/time-season.md 섹션 1.7)
        public int closeHour;                     // 활동 종료 시간 (→ see docs/systems/time-season.md 섹션 1.7)
        public DayFlag closedDays;                // 휴무 요일 비트마스크 (→ see docs/systems/economy-system.md 섹션 3.2)

        [Header("대화")]
        public DialogueData greetingDialogue;     // 기본 인사 대화 데이터
        public DialogueData closedDialogue;       // 영업 외/휴무 시 대화

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
        GeneralMerchant  = 0,
        Blacksmith       = 1,
        Carpenter        = 2,
        TravelingMerchant = 3
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
        CloseDialogue = 4   // 대화 종료
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
│                                                              │
│  [메서드]                                                     │
│  + CheckVisitSchedule(int currentDay, int currentDayOfWeek): │
│    void                                                      │
│  + GenerateStock(int playerLevel, Season season): void       │
│  + SpawnMerchant(): void                                     │
│  + DespawnMerchant(): void                                   │
│  + GetSaveData(): TravelingMerchantSaveData                  │
│  + LoadSaveData(TravelingMerchantSaveData data): void        │
│                                                              │
│  [이벤트 구독]                                                 │
│  - TimeManager.OnDayChanged → CheckVisitSchedule()           │
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

```csharp
// illustrative
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

---

## Open Questions

1. [OPEN] **NPC 호감도 시스템**: 현재 NPC는 상점/서비스 제공자로만 기능하나, 호감도 시스템(선물, 대화 빈도에 따른 호감 상승 -> 할인/특수 아이템 해금)을 도입할지. 도입 시 NPCSaveData에 호감도 필드 추가 필요. 현재 설계에서 `NPCSaveData`에 확장 여지를 남겨두었다.

2. [OPEN] **대화 시스템 확장 범위**: 현재 선형 + 선택지 구조로 설계했으나, 조건 분기(계절별, 호감도별 대사 변경)가 필요하면 DialogueNode에 조건 필드를 추가해야 한다. 복잡도와 구현 비용 간 트레이드오프 검토 필요.

3. [OPEN] **NPC 이동 AI**: 현재 NPC는 고정 위치에 서 있는 것으로 설계했다. NPC가 시간대에 따라 이동하는 패턴(아침에 가게 열기, 점심에 마을 광장 이동 등)을 넣을지. 구현 복잡도 대비 몰입감 향상 효과 분석 필요.

4. [OPEN] **목수 NPC의 BuildingManager 연동 상세**: 현재 목수 NPC는 시설 건설 의뢰 역할로 정의했으나, BuildingManager와의 구체적인 연동 흐름(건설 의뢰 -> 소요 일수 -> 완성 알림)은 별도 설계 필요.

5. [OPEN] **여행 상인 고유 아이템**: 여행 상인만 판매하는 독점 아이템(희귀 씨앗, 장식 아이템 등)의 종류와 수는 CON-003에서 확정 필요.

---

## Risks

1. [RISK] **NPC 모듈의 넓은 의존성**: NPC 모듈이 Core, Farm, Player, Economy, Building을 모두 참조한다. asmdef 순환 참조는 없으나(NPC를 참조하는 모듈은 UI뿐), 변경 영향 범위가 넓어 NPC 모듈 수정 시 다른 모듈 테스트가 필요할 수 있다.

2. [RISK] **MCP에서 DialogueData SO 에셋 생성 시 배열 필드 설정의 난이도**: DialogueData는 DialogueNode 배열 내에 DialogueChoice 배열을 포함하는 중첩 구조이다. MCP로 이 구조를 자동 생성할 때 직렬화된 배열의 중첩 참조가 제대로 설정되지 않을 수 있다. (-> see `docs/architecture.md` Risks 참조, 기존 SO 배열 필드 MCP 지원 범위 불확실 이슈)

3. [RISK] **기존 ShopSystem과의 통합 충돌**: 기존 economy-architecture.md의 ShopSystem은 단일 ShopData를 참조하는 구조이다. NPC별로 다른 ShopData를 동적으로 교체하려면 ShopSystem.Open()에 ShopData 파라미터를 추가해야 하며, 기존 인터페이스 수정이 필요하다.

4. [RISK] **여행 상인 재고 재현성**: 세이브/로드 시 여행 상인의 재고를 정확히 재현하려면 randomSeed 기반으로 동일한 재고를 재생성해야 한다. Random.seed 관리가 다른 시스템의 난수 사용과 충돌하지 않도록 독립 인스턴스(System.Random)를 사용해야 한다.

---

*이 문서는 NPC/상점 시스템의 기술 아키텍처 문서이다. NPC 클래스 구조, 대화 시스템, 이벤트 패턴, 의존성 구조는 이 문서를 정본으로 한다. NPC 콘텐츠(이름, 대사, 수치)는 `docs/content/npcs.md` (CON-003)가 canonical이다.*
