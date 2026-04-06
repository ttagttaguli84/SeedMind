# 대장간 NPC MCP 태스크 시퀀스 (ARC-020)

> 대장간 NPC(BlacksmithNPC)의 스크립트 생성, SO 에셋 생성, 씬 배치, UI 구성, 친밀도 연동, 세이브/로드 연동, 통합 테스트를 MCP for Unity 태스크로 상세 정의  
> 작성: Claude Code (Opus) | 2026-04-07  
> Phase 1 | 문서 ID: ARC-020

---

## 1. 개요

### 1.1 목적

이 문서는 `docs/systems/blacksmith-architecture.md`(ARC-020) Part II에서 요약된 MCP 구현 계획(Phase A~D)을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 대장간 NPC 시스템의 데이터 레이어(BlacksmithNPCData SO 1종, DialogueData SO 10종), 시스템 레이어(스크립트 10종), UI 레이어(ToolUpgradeScreen), 씬 배치, 친밀도/세이브 연동을 완성한다.

### 1.2 의존성

```
대장간 NPC 시스템 MCP 태스크 의존 관계:
├── SeedMind.Core     (TimeManager, SaveManager, EventBus, DataRegistry)
├── SeedMind.NPC      (NPCController, DialogueSystem, NPCManager, NPCEvents)
├── SeedMind.NPC.Data (NPCData, DialogueData, DialogueNode, DialogueChoice)
├── SeedMind.Tool     (ToolUpgradeSystem, ToolUpgradeEvents, ToolData SO)
├── SeedMind.Player   (InventoryManager -- 도구 잠금/해제, 재료 차감)
├── SeedMind.Economy  (EconomyManager -- 골드 차감, ShopSystem -- 재료 상점)
├── SeedMind.Level    (ProgressionManager -- XP 부여)
└── SeedMind.UI       (UIManager, ScreenBase, PopupQueue)
```

(-> see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 1.3 완료된 태스크 의존성

| 문서 ID | 문서 | 완료 필수 | 핵심 결과물 |
|---------|------|----------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | 전체 | 폴더 구조, SCN_Farm 기본 계층 (MANAGERS, NPCs, UI) |
| ARC-003 | `docs/mcp/farming-tasks.md` | 전체 | FarmGrid, ToolData SO 기본 구조, ToolSystem |
| ARC-009 | `docs/mcp/npc-shop-tasks.md` | T-1, T-2, T-3 | NPCData SO, NPCController, DialogueSystem, DialogueUI, NPCManager, NPCEvents |
| ARC-015 | `docs/mcp/tool-upgrade-tasks.md` | 전체 | ToolUpgradeSystem, ToolUpgradeEvents, ToolData SO 등급별 TierStats 확장 |
| ARC-018 | (ui-architecture.md 기반) | UIManager, ScreenBase | UIManager Screen FSM 인프라 |

### 1.4 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `Canvas_Overlay` (UI 루트) | ARC-002 Phase B |
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `--- NPCs ---` (씬 계층 부모) | ARC-009 T-4-01 |
| `NPC_Blacksmith` (NPCController 부착 GameObject) | ARC-009 T-4-03 |
| `NPCManager` | ARC-009 T-4-05 |
| `DialogueSystem` | ARC-009 T-4-06 |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A |
| `SO_NPC_Blacksmith.asset` (NPCData SO) | ARC-009 T-2-03 |
| `SO_Dlg_Greeting_Blacksmith.asset` | ARC-009 T-2-08 |
| `SO_Dlg_Closed_Blacksmith.asset` | ARC-009 T-2-11 |
| `ToolUpgradeSystem` (씬 매니저) | ARC-015 T-4 |
| `ToolUpgradeEvents` | ARC-015 T-2 |
| `InventoryManager` | inventory-architecture.md |
| `EconomyManager`, `ShopSystem` | economy-architecture.md |
| `ProgressionManager` | progression-architecture.md |

### 1.5 태스크 맵

| 태스크 | 설명 | MCP 호출 수 |
|--------|------|------------|
| T-1 | 스크립트 생성 (enum, SO 클래스, 시스템 클래스, UI 클래스) | 14회 |
| T-2 | SO 에셋 생성 (BlacksmithNPCData 1종 + DialogueData 10종) | 58회 |
| T-3 | NPC_Blacksmith 프리팹 확장 + InteractionZone 배치 | 12회 |
| T-4 | ToolUpgradeScreen UI 계층 구성 | 38회 |
| T-5 | 씬 배치 및 참조 연결 (NPCAffinityTracker, UIManager 등록) | 14회 |
| T-6 | 통합 테스트 시퀀스 | 20회 |
| **합계** | | **~156회** |

[RISK] 총 ~156회 MCP 호출은 상당하다. 특히 T-2의 DialogueData SO에서 DialogueNode[] 내 DialogueChoice[] 중첩 배열 설정이 MCP `set_property`로 가능한지 사전 검증 필요. 불가능한 경우 Editor 스크립트(CreateBlacksmithAssets.cs)를 통한 일괄 생성으로 T-2의 58회를 ~6회로 감소시킬 수 있다.

### 1.6 스크립트 목록

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/NPC/Data/BlacksmithInteractionState.cs` | `BlacksmithInteractionState` (enum) | `SeedMind.NPC` | T-1 Phase 1 |
| S-02 | `Scripts/NPC/Data/BlacksmithNPCData.cs` | `BlacksmithNPCData` (ScriptableObject) | `SeedMind.NPC.Data` | T-1 Phase 1 |
| S-03 | `Scripts/NPC/Data/AffinitySaveData.cs` | `AffinitySaveData`, `AffinityEntry` | `SeedMind.NPC` | T-1 Phase 1 |
| S-04 | `Scripts/NPC/NPCAffinityTracker.cs` | `NPCAffinityTracker` (MonoBehaviour) | `SeedMind.NPC` | T-1 Phase 2 |
| S-05 | `Scripts/NPC/BlacksmithEvents.cs` | `BlacksmithEvents` (static class) | `SeedMind.NPC` | T-1 Phase 2 |
| S-06 | `Scripts/NPC/BlacksmithNPC.cs` | `BlacksmithNPC` (MonoBehaviour) | `SeedMind.NPC` | T-1 Phase 3 |
| S-07 | `Scripts/UI/ToolComparisonPanel.cs` | `ToolComparisonPanel` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 4 |
| S-08 | `Scripts/UI/MaterialSlotUI.cs` | `MaterialSlotUI` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 4 |
| S-09 | `Scripts/UI/ToolUpgradeUI.cs` | `ToolUpgradeUI` (ScreenBase 파생) | `SeedMind.UI` | T-1 Phase 4 |
| S-10 | `Scripts/UI/ToolUpgradeSlotUI.cs` | `ToolUpgradeSlotUI` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 4 |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: S-01~S-03 -> S-04/S-05 -> S-06 -> S-07~S-10. 각 Phase 사이에 Unity 컴파일 대기(`execute_menu_item`)가 필요하다.

### 1.7 SO 에셋 목록

| # | 에셋명 | 경로 | SO 타입 | 생성 태스크 |
|---|--------|------|---------|-----------|
| A-01 | `SO_BlacksmithNPC_Cheolsu.asset` | `Assets/_Project/Data/NPCs/` | BlacksmithNPCData | T-2-02 |
| A-02 | `SO_Dlg_Blacksmith_Greet_Lv0.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-03 |
| A-03 | `SO_Dlg_Blacksmith_Greet_Lv1.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-04 |
| A-04 | `SO_Dlg_Blacksmith_Greet_Lv2.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-05 |
| A-05 | `SO_Dlg_Blacksmith_Greet_Lv3.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-06 |
| A-06 | `SO_Dlg_Blacksmith_Closed.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-07 |
| A-07 | `SO_Dlg_Blacksmith_Pickup.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-08 |
| A-08 | `SO_Dlg_Blacksmith_Affinity_Lv1.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-09 |
| A-09 | `SO_Dlg_Blacksmith_Affinity_Lv2.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-10 |
| A-10 | `SO_Dlg_Blacksmith_Affinity_Lv3.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-11 |
| A-11 | `SO_Dlg_Blacksmith_FirstMeet.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-12 |

> **참고**: ARC-009에서 이미 생성된 `SO_Dlg_Greeting_Blacksmith.asset`와 `SO_Dlg_Closed_Blacksmith.asset`는 기본 NPCData용이다. ARC-020에서 생성하는 대화 SO는 BlacksmithNPCData 전용으로, 친밀도 단계별 분화된 대화를 담는다. 기존 SO는 NPCData 범용 인사말로 유지하고, BlacksmithNPCData의 `greetingDialogues[]`는 A-02~A-05를 참조한다.

### 1.8 씬 GameObject 목록

| # | 오브젝트명 | 부모 | 컴포넌트 | 생성 태스크 |
|---|-----------|------|----------|-----------|
| G-01 | `Blacksmith_InteractionZone` | `NPC_Blacksmith` | BoxCollider2D (Trigger) | T-3-02 |
| G-02 | `NPCAffinityTracker` | `--- MANAGERS ---` | NPCAffinityTracker | T-5-01 |
| G-03 | `ToolUpgradeScreen` | `Canvas_Overlay` | ToolUpgradeUI (ScreenBase) | T-4-01 |
| G-04 | (ToolUpgradeScreen 내부 UI 계층) | `ToolUpgradeScreen` | 다수 (섹션 4 참조) | T-4-02~T-4-18 |

> **중복 생성 금지**: `NPC_Blacksmith`(G-03 in ARC-009)는 이미 존재한다. T-3에서는 기존 오브젝트에 `BlacksmithNPC` 컴포넌트를 추가 부착하고, 하위에 InteractionZone을 생성한다.

---

## MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋 폴더 생성 | T-1 |
| `create_script` | C# 스크립트 파일 생성 | T-1 |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | T-2 |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | T-2~T-5 전체 |
| `create_object` | 빈 GameObject 생성 | T-3, T-4, T-5 |
| `add_component` | MonoBehaviour 컴포넌트 부착 | T-3, T-4, T-5 |
| `set_parent` | 오브젝트 부모 설정 | T-3, T-4 |
| `save_scene` | 씬 저장 | T-3, T-5, T-6 |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | T-6 |
| `execute_menu_item` | 편집기 명령 실행 (컴파일 대기 등) | T-1 |
| `execute_method` | 런타임 메서드 호출 (테스트) | T-6 |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | T-6 |

---

## 2. T-1: 스크립트 생성

**목적**: 대장간 NPC 시스템에 필요한 모든 C# 스크립트를 생성한다.

**전제**: ARC-009(NPC/상점 시스템) 스크립트 생성 완료. ARC-015(도구 업그레이드) 스크립트 생성 완료. SeedMind.NPC, SeedMind.UI 네임스페이스 사용 가능. ScreenBase 클래스 사용 가능.

---

### T-1 Phase 1: 데이터 구조 스크립트 (S-01 ~ S-03)

#### T-1-01: BlacksmithInteractionState enum (S-01)

- **입력**: blacksmith-architecture.md 섹션 2.1 상태 정의
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/NPC/Data/BlacksmithInteractionState.cs"
  content: |
    // S-01: 대장간 NPC 상호작용 상태 열거형
    // -> see docs/systems/blacksmith-architecture.md 섹션 2.1
    namespace SeedMind.NPC
    {
        public enum BlacksmithInteractionState
        {
            Idle              = 0,  // 상호작용 없음
            Greeting          = 1,  // 인사 대화 재생 중
            ServiceMenu       = 2,  // 서비스 선택지 표시 중
            Chatting          = 3,  // 일상 대화 재생
            UpgradeSelect     = 4,  // ToolUpgradeUI에서 도구 선택 중
            UpgradeConfirm    = 5,  // 업그레이드 확인 팝업
            UpgradeResult     = 6,  // 업그레이드 시작/실패 결과 표시
            PickupResult      = 7,  // 완성 도구 수령 결과 표시
            MaterialShop      = 8,  // 재료 구매 상점 (ShopUI 위임)
        }
    }
```

- **검증**: 컴파일 에러 없음
- **에러 처리**: 네임스페이스 충돌 시 `SeedMind.NPC.Data`로 이동
- **MCP 호출**: 1회

#### T-1-02: BlacksmithNPCData ScriptableObject (S-02)

- **입력**: blacksmith-architecture.md 섹션 4.2 C# 클래스 정의
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/NPC/Data/BlacksmithNPCData.cs"
  content: |
    // S-02: 대장간 NPC 고유 데이터 ScriptableObject
    // -> see docs/systems/blacksmith-architecture.md 섹션 4.2
    using UnityEngine;

    namespace SeedMind.NPC.Data
    {
        [CreateAssetMenu(fileName = "NewBlacksmithNPCData", menuName = "SeedMind/BlacksmithNPCData")]
        public class BlacksmithNPCData : ScriptableObject
        {
            [Header("기본 식별")]
            public string npcId;                          // "npc_cheolsu"
            public string displayName;                    // -> see docs/content/npcs.md 섹션 4.1

            [Header("대화 -- 친밀도 단계별 인사")]
            public DialogueData[] greetingDialogues;      // 인덱스 = 친밀도 단계 (0~3)
            public DialogueData closedDialogue;            // 영업 외/휴무 시 대화
            public DialogueData pendingPickupDialogue;     // 완성 도구 수령 안내 대화

            [Header("친밀도")]
            public int[] affinityThresholds;              // 단계별 임계값
                                                          // -> see docs/content/blacksmith-npc.md 섹션 2.5
            public DialogueData[] affinityDialogues;      // 단계 상승 시 일회성 특수 대화

            [Header("친밀도 보상")]
            public int upgradeCompleteAffinity;            // 업그레이드 완료 시 친밀도 증가량
                                                          // -> see docs/content/blacksmith-npc.md 섹션 2.5
            public int materialPurchaseAffinity;           // 재료 1회 구매 시 친밀도 증가량
                                                          // -> see docs/content/blacksmith-npc.md 섹션 2.5

            [Header("친밀도 혜택")]
            public int specialDiscountAffinityLevel;      // 할인 혜택 해금 친밀도 단계
                                                          // -> see docs/content/blacksmith-npc.md 섹션 2.5
            public float discountRate;                    // 할인율 (0.1 = 10%)
                                                          // -> see docs/content/blacksmith-npc.md 섹션 2.5
        }
    }
```

- **검증**: `DialogueData` 타입 참조가 컴파일 가능한지 확인 (ARC-009에서 생성된 S-07 의존)
- **에러 처리**: `DialogueData` 미발견 시 ARC-009 T-1 Phase 1 완료 확인
- **MCP 호출**: 1회

#### T-1-03: AffinitySaveData 직렬화 클래스 (S-03)

- **입력**: blacksmith-architecture.md 섹션 5.5 세이브 데이터 구조
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/NPC/Data/AffinitySaveData.cs"
  content: |
    // S-03: NPC 친밀도 세이브 데이터
    // -> see docs/systems/blacksmith-architecture.md 섹션 5.5
    namespace SeedMind.NPC
    {
        [System.Serializable]
        public class AffinitySaveData
        {
            public AffinityEntry[] entries;
        }

        [System.Serializable]
        public class AffinityEntry
        {
            public string npcId;                  // "npc_cheolsu"
            public int affinityValue;             // 현재 친밀도 수치
            public int lastVisitDay;              // 마지막 방문 일차 (일일 대화 중복 방지)
            public bool[] triggeredDialogues;     // 단계별 특수 대화 재생 여부
        }
    }
```

- **검증**: 컴파일 에러 없음
- **MCP 호출**: 1회
- **Phase 1 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

### T-1 Phase 2: 시스템 스크립트 (S-04 ~ S-05)

#### T-1-04: NPCAffinityTracker (S-04)

- **입력**: blacksmith-architecture.md 섹션 1 클래스 다이어그램, 섹션 5 친밀도 시스템
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/NPC/NPCAffinityTracker.cs"
  content: |
    // S-04: NPC 친밀도 추적 매니저 (범용, 대장간 전용 아님)
    // -> see docs/systems/blacksmith-architecture.md 섹션 1, 5
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    namespace SeedMind.NPC
    {
        /// <summary>
        /// 전체 NPC의 친밀도를 추적, 관리한다.
        /// 세이브/로드 시 AffinitySaveData를 사용한다.
        /// </summary>
        public class NPCAffinityTracker : MonoBehaviour
        {
            private Dictionary<string, int> _affinityMap
                = new Dictionary<string, int>();
            private Dictionary<string, int> _lastVisitDayMap
                = new Dictionary<string, int>();
            private Dictionary<string, bool[]> _triggeredDialogueMap
                = new Dictionary<string, bool[]>();

            // --- 이벤트 ---
            public event Action<string, int, int> OnAffinityChanged;
                // npcId, oldValue, newValue
            public event Action<string, int> OnAffinityLevelUp;
                // npcId, newLevel

            // --- 공개 메서드 ---
            // GetAffinity(npcId): int
            // AddAffinity(npcId, amount): void
            //   -> 단계 상승 감지 시 OnAffinityLevelUp 발행
            // GetAffinityLevel(npcId, thresholds): int
            // HasTriggeredDialogue(npcId, level): bool
            // MarkDialogueTriggered(npcId, level): void
            // CanGiveDailyAffinity(npcId, currentDay): bool
            // MarkDailyVisit(npcId, currentDay): void
            // GetSaveData(): AffinitySaveData
            // LoadSaveData(data): void
        }
    }
```

- **검증**: `AffinitySaveData` 참조 가능 (S-03 의존)
- **MCP 호출**: 1회

#### T-1-05: BlacksmithEvents (S-05)

- **입력**: blacksmith-architecture.md 섹션 6.1 이벤트 허브
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/NPC/BlacksmithEvents.cs"
  content: |
    // S-05: 대장간 NPC 고유 이벤트 허브
    // -> see docs/systems/blacksmith-architecture.md 섹션 6.1
    using System;

    namespace SeedMind.NPC
    {
        /// <summary>
        /// 대장간 NPC 고유 이벤트.
        /// NPCEvents(-> see npc-shop-architecture.md 섹션 4)를 보완한다.
        /// </summary>
        public static class BlacksmithEvents
        {
            /// <summary>업그레이드 UI에서 업그레이드 시작 요청 완료</summary>
            public static event Action<string, int> OnUpgradeRequested;
            // toolId, targetTier

            /// <summary>도구 수령(pickup) 완료</summary>
            public static event Action<string, int> OnToolPickedUp;
            // toolId, newTier

            /// <summary>친밀도 단계 상승</summary>
            public static event Action<string, int> OnAffinityLevelUp;
            // npcId, newLevel

            // --- 발행 메서드 (internal) ---
            internal static void RaiseUpgradeRequested(string toolId, int targetTier)
                => OnUpgradeRequested?.Invoke(toolId, targetTier);
            internal static void RaiseToolPickedUp(string toolId, int newTier)
                => OnToolPickedUp?.Invoke(toolId, newTier);
            internal static void RaiseAffinityLevelUp(string npcId, int newLevel)
                => OnAffinityLevelUp?.Invoke(npcId, newLevel);
        }
    }
```

- **검증**: 컴파일 에러 없음
- **MCP 호출**: 1회
- **Phase 2 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

### T-1 Phase 3: BlacksmithNPC 스크립트 (S-06)

#### T-1-06: BlacksmithNPC MonoBehaviour (S-06)

- **입력**: blacksmith-architecture.md 섹션 1 클래스 다이어그램, 섹션 2 State Machine
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/NPC/BlacksmithNPC.cs"
  content: |
    // S-06: 대장간 NPC 상호작용 진입점
    // -> see docs/systems/blacksmith-architecture.md 섹션 1, 2
    using UnityEngine;
    using SeedMind.NPC.Data;

    namespace SeedMind.NPC
    {
        /// <summary>
        /// 대장간 NPC(철수) 상호작용 진입점.
        /// 기존 NPCController에 부착하여 대장간 고유의 FSM 기반 대화/서비스 흐름을 관리한다.
        /// </summary>
        public class BlacksmithNPC : MonoBehaviour
        {
            [Header("설정 참조")]
            [SerializeField] private NPCData _npcData;
                // -> see npc-shop-architecture.md 섹션 2.1
            [SerializeField] private BlacksmithNPCData _blacksmithData;

            // --- 외부 참조 (Awake에서 캐싱) ---
            // _dialogueSystem: DialogueSystem (싱글턴)
            // _upgradeSystem: ToolUpgradeSystem (싱글턴)
            // _affinityTracker: NPCAffinityTracker (싱글턴)

            // --- 상태 ---
            // _interactionState: BlacksmithInteractionState
            // _isInteracting: bool

            // --- 공개 메서드 ---
            // Interact(PlayerController player): void
            //   -> 영업시간 체크, 인사말 선택, FSM 진입
            // HandleDialogueChoice(DialogueChoiceAction action): void
            //   -> 선택지에 따른 상태 전이
            // OnUpgradeUIResult(UpgradeUIResult result): void
            //   -> ToolUpgradeUI 결과 수신

            // --- 내부 메서드 ---
            // SelectGreetingDialogue(): DialogueData
            //   -> 친밀도 단계, 수령 대기, 최초 만남 판별
            //   -> (-> see blacksmith-architecture.md 섹션 5.4)
            // CheckPendingPickup(): bool
            // BuildChoiceList(): DialogueChoice[]
            //   -> "도구 업그레이드", "도구 수령"(조건부), "재료 구매",
            //      "이야기하기", "나가기"

            // --- 이벤트 구독 ---
            // OnEnable(): ToolUpgradeEvents.OnUpgradeCompleted += ...
            // OnDisable(): 구독 해제
        }
    }
```

- **검증**: `NPCData`, `BlacksmithNPCData`, `BlacksmithInteractionState`, `DialogueData` 타입 모두 참조 가능
- **에러 처리**: 참조 타입 미발견 시 이전 Phase 컴파일 완료 확인
- **MCP 호출**: 1회
- **Phase 3 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

### T-1 Phase 4: UI 스크립트 (S-07 ~ S-10)

#### T-1-07: ToolComparisonPanel (S-07)

- **입력**: blacksmith-architecture.md 섹션 3.1 UI 계층 ComparisonView
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/ToolComparisonPanel.cs"
  content: |
    // S-07: 도구 현재/업그레이드 후 스탯 비교 패널
    // -> see docs/systems/blacksmith-architecture.md 섹션 3.1
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    namespace SeedMind.UI
    {
        /// <summary>
        /// 도구의 현재 또는 업그레이드 후 스탯을 표시하는 패널.
        /// ToolUpgradeUI의 ComparisonView에서 좌(현재)/우(업그레이드 후) 한 쌍으로 사용.
        /// </summary>
        public class ToolComparisonPanel : MonoBehaviour
        {
            [SerializeField] private TMP_Text _label;         // "현재" / "업그레이드 후"
            [SerializeField] private Image _toolIcon;
            [SerializeField] private TMP_Text _toolName;
            [SerializeField] private TMP_Text _tierText;
            [SerializeField] private TMP_Text _rangeStat;
            [SerializeField] private TMP_Text _speedStat;
            [SerializeField] private TMP_Text _specialStat;

            // + Setup(ToolData tool, int tier, bool isUpgraded): void
            //   -> 스탯 수치는 ToolData SO에서 TierStats[tier] 조회
            //      (-> see docs/systems/tool-upgrade.md 섹션 3)
            // + HighlightChanges(ToolComparisonPanel other): void
            //   -> 개선 수치 초록색, 악화 수치 빨간색
        }
    }
```

- **MCP 호출**: 1회

#### T-1-08: MaterialSlotUI (S-08)

- **입력**: blacksmith-architecture.md 섹션 3.1 CostPanel > MaterialSlotList
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/MaterialSlotUI.cs"
  content: |
    // S-08: 재료 슬롯 UI (아이콘 + 이름 + 수량)
    // -> see docs/systems/blacksmith-architecture.md 섹션 3.1
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    namespace SeedMind.UI
    {
        /// <summary>
        /// 업그레이드 비용 패널에서 재료 1종의 정보를 표시하는 슬롯.
        /// </summary>
        public class MaterialSlotUI : MonoBehaviour
        {
            [SerializeField] private Image _materialIcon;
            [SerializeField] private TMP_Text _materialName;
            [SerializeField] private TMP_Text _quantityText;  // "보유/필요" 형식

            // + Setup(string materialId, int required, int owned): void
            //   -> 부족 시 빨간색, 충족 시 흰색
            //   -> (-> see blacksmith-architecture.md 섹션 3.2 충족/부족 규칙)
        }
    }
```

- **MCP 호출**: 1회

#### T-1-09: ToolUpgradeUI (S-09)

- **입력**: blacksmith-architecture.md 섹션 3.3 ToolUpgradeUI 클래스 설계
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/ToolUpgradeUI.cs"
  content: |
    // S-09: 대장간 도구 업그레이드 화면 (ScreenBase 파생)
    // -> see docs/systems/blacksmith-architecture.md 섹션 3.3
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    namespace SeedMind.UI
    {
        /// <summary>
        /// 대장간 도구 업그레이드 화면.
        /// ScreenBase를 상속하여 UIManager Screen FSM에 통합된다.
        /// ScreenType.ToolUpgrade = 11 (-> see ui-architecture.md 섹션 1.2)
        /// </summary>
        public class ToolUpgradeUI : ScreenBase
        {
            [Header("도구 목록")]
            [SerializeField] private Transform _toolSlotContainer;
            [SerializeField] private ToolUpgradeSlotUI _toolSlotPrefab;

            [Header("비교 뷰")]
            [SerializeField] private ToolComparisonPanel _currentToolPanel;
            [SerializeField] private ToolComparisonPanel _upgradedToolPanel;

            [Header("비용 패널")]
            [SerializeField] private TMP_Text _goldCostText;
            [SerializeField] private Image _goldIcon;
            [SerializeField] private Transform _materialSlotContainer;
            [SerializeField] private MaterialSlotUI _materialSlotPrefab;
            [SerializeField] private TMP_Text _timeCostText;
            [SerializeField] private TMP_Text _levelRequirementText;

            [Header("버튼")]
            [SerializeField] private Button _upgradeButton;
            [SerializeField] private Button _cancelButton;
            [SerializeField] private Button _closeButton;

            [Header("확인 팝업")]
            [SerializeField] private GameObject _confirmPopup;
            [SerializeField] private TMP_Text _confirmText;
            [SerializeField] private Button _confirmYesButton;
            [SerializeField] private Button _confirmNoButton;

            // --- 콜백 ---
            public event Action<UpgradeUIResult> OnResultCallback;

            // --- ScreenBase 오버라이드 ---
            // OnBeforeOpen(): ToolUpgradeSystem에서 업그레이드 가능 도구 목록 로드
            // OnAfterOpen(): 첫 번째 도구 슬롯에 포커스
            // OnBeforeClose(): 선택 초기화

            // --- 메서드 ---
            // + RefreshToolList(): void
            // + SelectTool(ToolData tool): void
            // + RefreshComparisonView(): void
            // + RefreshCostPanel(): void
            //   -> 비용 수치: (-> see docs/systems/tool-upgrade.md 섹션 2.1)
            //   -> 충족/부족 색상: (-> see blacksmith-architecture.md 섹션 3.2)
            // + OnUpgradeButtonClicked(): void -> 확인 팝업 표시
            // + OnConfirmUpgrade(): void -> ToolUpgradeSystem.StartUpgrade()
            // + OnCancelUpgrade(): void -> 확인 팝업 닫기

            // --- 조건 검증 순서 ---
            // 등급 체크 -> 진행 중 체크 -> 레벨 체크 -> 골드/재료 체크
            // (-> see docs/mcp/tool-upgrade-design-analysis.md 섹션 5.3)
        }
    }
```

- **검증**: `ScreenBase` 클래스 접근 가능 (ARC-018 의존), `ToolUpgradeSlotUI`/`ToolComparisonPanel`/`MaterialSlotUI` 참조 가능
- **MCP 호출**: 1회

#### T-1-10: ToolUpgradeSlotUI (S-10)

- **입력**: blacksmith-architecture.md 섹션 3.4 ToolUpgradeSlotUI
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/ToolUpgradeSlotUI.cs"
  content: |
    // S-10: 업그레이드 UI 개별 도구 슬롯
    // -> see docs/systems/blacksmith-architecture.md 섹션 3.4
    // 기존 tool-upgrade-architecture.md(DES-007)의 구조를 구체화
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    namespace SeedMind.UI
    {
        /// <summary>
        /// 도구 업그레이드 UI에서 개별 도구의 상태를 표시하는 슬롯.
        /// </summary>
        public class ToolUpgradeSlotUI : MonoBehaviour
        {
            [SerializeField] private Image _toolIcon;
            [SerializeField] private TMP_Text _toolName;
            [SerializeField] private Image _tierBadge;
            [SerializeField] private GameObject _upgradeArrow;
            [SerializeField] private GameObject _pendingOverlay;   // "업그레이드 중"
            [SerializeField] private TMP_Text _pendingDaysText;    // "잔여 N일"
            [SerializeField] private Button _selectButton;

            // + Setup(ToolData tool, UpgradeCheckResult result, PendingUpgrade pending): void
            //   -> 등급 뱃지 색상: (-> see docs/systems/tool-upgrade.md 섹션 7.3)
            // + SetSelected(bool selected): void
        }
    }
```

- **MCP 호출**: 1회
- **Phase 4 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

### T-1 완료 기준

- [ ] 10개 스크립트 파일 모두 `Assets/_Project/Scripts/` 하위에 생성됨
- [ ] Unity 콘솔에 컴파일 에러 없음
- [ ] `BlacksmithNPCData`, `NPCAffinityTracker`, `BlacksmithNPC`, `ToolUpgradeUI`, `ToolUpgradeSlotUI` 클래스가 Inspector에서 확인 가능
- [ ] `BlacksmithNPCData`가 Create Asset 메뉴(`SeedMind/BlacksmithNPCData`)에 표시됨

---

## 3. T-2: SO 에셋 생성

**목적**: 대장간 NPC 고유 데이터(BlacksmithNPCData SO 1종)와 친밀도 단계별 대화(DialogueData SO 10종)를 생성한다.

**전제**: T-1 완료. `BlacksmithNPCData`, `DialogueData` SO 타입 컴파일 완료.

---

### T-2-01: 폴더 확인

```
# 이미 존재하는 폴더 확인 (생성 불필요 시 건너뜀)
# Assets/_Project/Data/NPCs/ -- ARC-009에서 생성 완료
# Assets/_Project/Data/Dialogues/ -- ARC-009에서 생성 완료
```

- **MCP 호출**: 0회 (이미 존재)

### T-2-02: BlacksmithNPCData SO 생성 (A-01)

- **입력**: blacksmith-architecture.md 섹션 4.1 JSON 예시
- **액션**:

```
create_scriptable_object
  type: "BlacksmithNPCData"
  path: "Assets/_Project/Data/NPCs/SO_BlacksmithNPC_Cheolsu.asset"

set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "npcId" value: "npc_cheolsu"
set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "displayName" value: "철수"
set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "affinityThresholds" value: [0, 10, 25, 50]
    // -> copied from docs/content/blacksmith-npc.md 섹션 2.5 (canonical)
set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "upgradeCompleteAffinity" value: 5
    // -> copied from docs/content/blacksmith-npc.md 섹션 2.5
set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "materialPurchaseAffinity" value: 1
    // -> copied from docs/content/blacksmith-npc.md 섹션 2.5
set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "specialDiscountAffinityLevel" value: 3
    // -> copied from docs/content/blacksmith-npc.md 섹션 2.5
set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "discountRate" value: 0.1
    // -> copied from docs/content/blacksmith-npc.md 섹션 2.5
```

- **검증**: SO 에셋이 Inspector에서 열리고 모든 필드값이 설정됨
- **에러 처리**: `create_scriptable_object` 미지원 시 `execute_method`로 Editor 스크립트 우회
- **MCP 호출**: 8회

### T-2-03 ~ T-2-06: 친밀도 단계별 인사 DialogueData SO (A-02 ~ A-05)

- **입력**: blacksmith-npc.md 섹션 3.2 일반 방문 대화, 섹션 2.5 관계 발전 단계
- **액션** (4개 SO 반복, Lv0~Lv3):

```
# Lv0: 낯선 사이 인사 (A-02)
create_scriptable_object
  type: "DialogueData"
  path: "Assets/_Project/Data/Dialogues/SO_Dlg_Blacksmith_Greet_Lv0.asset"

set_property  property: "dialogueId" value: "dlg_blacksmith_greet_lv0"
set_property  property: "nodes[0].speakerName" value: "철수"
set_property  property: "nodes[0].text" value: "왔나. 볼일이 있으면 말해."
    // -> see docs/content/blacksmith-npc.md 섹션 3.2 general_01
set_property  property: "nodes[0].choices" value: [서비스 선택지 배열]
    // choices: 업그레이드/수령/재료구매/이야기하기/나가기
    // -> see blacksmith-architecture.md 섹션 2.3 ServiceMenu

# Lv1: 알고 지내는 사이 (A-03) -- dialogueId: "dlg_blacksmith_greet_lv1"
# Lv2: 단골 (A-04) -- dialogueId: "dlg_blacksmith_greet_lv2"
# Lv3: 친구 (A-05) -- dialogueId: "dlg_blacksmith_greet_lv3"
# 대사 텍스트: (-> see docs/content/blacksmith-npc.md 섹션 3.2, 3.8)
```

- **검증**: 각 SO의 `dialogueId`가 고유하고, `nodes[0].text`에 대사가 정확히 입력됨
- **에러 처리**: `nodes` 배열 내 중첩 객체 설정 불가 시 Editor 스크립트 우회
- **MCP 호출**: 각 SO당 4회 x 4종 = 16회

[RISK] DialogueData의 `nodes[]` 배열 내 `DialogueNode.choices[]` 중첩 배열을 MCP `set_property`로 설정하는 것이 기술적으로 가능한지 사전 검증 필요. 불가능한 경우 Editor 스크립트를 통해 일괄 생성한다.

### T-2-07: 영업 외 대화 DialogueData SO (A-06)

- **입력**: blacksmith-npc.md 섹션 3.7 영업 외 대화
- **액션**:

```
create_scriptable_object
  type: "DialogueData"
  path: "Assets/_Project/Data/Dialogues/SO_Dlg_Blacksmith_Closed.asset"

set_property  property: "dialogueId" value: "dlg_blacksmith_closed"
set_property  property: "nodes[0].speakerName" value: "철수"
set_property  property: "nodes[0].text" value: "(대사 텍스트 -> see docs/content/blacksmith-npc.md 섹션 3.7)"
```

- **MCP 호출**: 3회

### T-2-08: 도구 수령 안내 DialogueData SO (A-07)

- **입력**: blacksmith-npc.md 섹션 3.4 업그레이드 완료 대화
- **액션**:

```
create_scriptable_object
  type: "DialogueData"
  path: "Assets/_Project/Data/Dialogues/SO_Dlg_Blacksmith_Pickup.asset"

set_property  property: "dialogueId" value: "dlg_blacksmith_pickup"
set_property  property: "nodes[0].speakerName" value: "철수"
set_property  property: "nodes[0].text" value: "(대사 텍스트 -> see docs/content/blacksmith-npc.md 섹션 3.4)"
set_property  property: "nodes[0].choices" value: [{choiceText: "도구 수령", action: "Continue", jumpToNode: -1}]
```

- **MCP 호출**: 4회

### T-2-09 ~ T-2-11: 친밀도 단계 상승 특수 대화 (A-08 ~ A-10)

- **입력**: blacksmith-npc.md 섹션 3.8 친밀도 단계별 특수 대화
- **액션** (3개 SO 반복, Lv1~Lv3):

```
# Lv1 단계 상승 대화 (A-08)
create_scriptable_object
  type: "DialogueData"
  path: "Assets/_Project/Data/Dialogues/SO_Dlg_Blacksmith_Affinity_Lv1.asset"

set_property  property: "dialogueId" value: "dlg_blacksmith_affinity_lv1"
set_property  property: "nodes[0].speakerName" value: "철수"
set_property  property: "nodes[0].text" value: "(대사 텍스트 -> see docs/content/blacksmith-npc.md 섹션 3.8)"

# Lv2 (A-09), Lv3 (A-10) 동일 패턴
```

- **MCP 호출**: 각 SO당 3회 x 3종 = 9회

### T-2-12: 최초 만남 대화 DialogueData SO (A-11)

- **입력**: blacksmith-npc.md 섹션 3.1 최초 만남 대화
- **액션**:

```
create_scriptable_object
  type: "DialogueData"
  path: "Assets/_Project/Data/Dialogues/SO_Dlg_Blacksmith_FirstMeet.asset"

set_property  property: "dialogueId" value: "dlg_blacksmith_firstmeet"
set_property  property: "nodes[0].speakerName" value: "철수"
set_property  property: "nodes[0].text" value: "...응. 새 농부구나.\n도구가 보이는데, 기본 도구로군.\n나중에 좀 더 쓸 만한 게 필요하면 재료를 가져와. 만들어 줄 테니."
    // -> see docs/content/blacksmith-npc.md 섹션 3.1
```

- **MCP 호출**: 3회

### T-2-13: BlacksmithNPCData에 DialogueData 참조 연결

- **입력**: T-2-02에서 생성한 SO에 T-2-03~T-2-12의 DialogueData 참조를 설정
- **액션**:

```
set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "greetingDialogues[0]"
              value: ref("Assets/_Project/Data/Dialogues/SO_Dlg_Blacksmith_Greet_Lv0.asset")
set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "greetingDialogues[1]"
              value: ref("Assets/_Project/Data/Dialogues/SO_Dlg_Blacksmith_Greet_Lv1.asset")
set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "greetingDialogues[2]"
              value: ref("Assets/_Project/Data/Dialogues/SO_Dlg_Blacksmith_Greet_Lv2.asset")
set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "greetingDialogues[3]"
              value: ref("Assets/_Project/Data/Dialogues/SO_Dlg_Blacksmith_Greet_Lv3.asset")
set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "closedDialogue"
              value: ref("Assets/_Project/Data/Dialogues/SO_Dlg_Blacksmith_Closed.asset")
set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "pendingPickupDialogue"
              value: ref("Assets/_Project/Data/Dialogues/SO_Dlg_Blacksmith_Pickup.asset")
set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "affinityDialogues[0]"
              value: ref("Assets/_Project/Data/Dialogues/SO_Dlg_Blacksmith_Affinity_Lv1.asset")
set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "affinityDialogues[1]"
              value: ref("Assets/_Project/Data/Dialogues/SO_Dlg_Blacksmith_Affinity_Lv2.asset")
set_property  path: "SO_BlacksmithNPC_Cheolsu" property: "affinityDialogues[2]"
              value: ref("Assets/_Project/Data/Dialogues/SO_Dlg_Blacksmith_Affinity_Lv3.asset")
```

- **검증**: Inspector에서 SO_BlacksmithNPC_Cheolsu의 모든 DialogueData 참조가 None이 아님
- **에러 처리**: SO 배열 참조 설정 불가 시 string ID 기반 런타임 조회로 대체 (DataRegistry 활용)
- **MCP 호출**: 9회

[RISK] MCP의 SO 배열 참조(`ref()`) 설정 지원 여부. ARC-009, ARC-015와 동일 리스크. 대안으로 string ID 기반 런타임 조회 가능. BlacksmithNPCData에 `string[] greetingDialogueIds` 필드를 추가하고, 런타임에서 DataRegistry를 통해 ID -> SO 변환.

---

### T-2 완료 기준

- [ ] SO 에셋 11종이 `Assets/_Project/Data/` 하위에 생성됨
- [ ] `SO_BlacksmithNPC_Cheolsu.asset`의 모든 필드가 올바르게 설정됨
- [ ] DialogueData SO 10종의 `dialogueId`가 모두 고유값
- [ ] BlacksmithNPCData의 greetingDialogues[0~3], closedDialogue, pendingPickupDialogue, affinityDialogues[0~2] 참조가 연결됨

---

## 4. T-3: NPC_Blacksmith 프리팹 확장 + InteractionZone 배치

**목적**: ARC-009에서 생성된 NPC_Blacksmith GameObject에 BlacksmithNPC 컴포넌트를 추가 부착하고, 인터랙션 감지용 BoxCollider2D 트리거 자식 오브젝트를 배치한다.

**전제**: ARC-009 T-4-03 완료 (NPC_Blacksmith, NPCController 부착). T-1 완료 (BlacksmithNPC 스크립트).

---

### T-3-01: BlacksmithNPC 컴포넌트 추가

- **입력**: NPC_Blacksmith 기존 오브젝트
- **액션**:

```
add_component
  object: "NPC_Blacksmith"
  component: "SeedMind.NPC.BlacksmithNPC"

set_property  path: "NPC_Blacksmith" component: "BlacksmithNPC" property: "_npcData"
              value: ref("Assets/_Project/Data/NPCs/SO_NPC_Blacksmith.asset")
set_property  path: "NPC_Blacksmith" component: "BlacksmithNPC" property: "_blacksmithData"
              value: ref("Assets/_Project/Data/NPCs/SO_BlacksmithNPC_Cheolsu.asset")
```

- **검증**: Inspector에서 NPC_Blacksmith에 NPCController + BlacksmithNPC 두 컴포넌트가 공존
- **MCP 호출**: 3회

### T-3-02: InteractionZone 생성

- **입력**: tool-upgrade-design-analysis.md 섹션 1.2 씬 오브젝트 표
- **액션**:

```
create_object
  name: "Blacksmith_InteractionZone"
  parent: "NPC_Blacksmith"

add_component
  object: "Blacksmith_InteractionZone"
  component: "BoxCollider2D"

set_property  path: "Blacksmith_InteractionZone" component: "BoxCollider2D"
              property: "isTrigger" value: true
set_property  path: "Blacksmith_InteractionZone" component: "BoxCollider2D"
              property: "size" value: [3.0, 3.0]
    // 인터랙션 범위 3x3 타일
set_property  path: "Blacksmith_InteractionZone" component: "BoxCollider2D"
              property: "offset" value: [0.0, 0.0]
```

- **검증**: NPC_Blacksmith 하위에 Blacksmith_InteractionZone이 존재하고, BoxCollider2D가 Trigger 모드
- **MCP 호출**: 5회

### T-3-03: 대장간 건물 시각적 오브젝트 확인

- **입력**: tool-upgrade-design-analysis.md 섹션 1.2 Blacksmith_Building
- **액션**:

```
# Blacksmith_Building은 마을 씬 구성 태스크에서 별도로 생성되어야 한다.
# 현재는 NPC_Blacksmith 위치 근처에 배치될 것이라는 가정만 기록.
# [OPEN] 마을 씬 레이아웃 태스크 미정
```

- **MCP 호출**: 0회

### T-3-04: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

---

### T-3 완료 기준

- [ ] NPC_Blacksmith에 BlacksmithNPC 컴포넌트 부착됨
- [ ] BlacksmithNPC._npcData, _blacksmithData 참조가 올바른 SO를 가리킴
- [ ] Blacksmith_InteractionZone이 NPC_Blacksmith 하위에 존재
- [ ] BoxCollider2D isTrigger=true, size=(3,3)

---

## 5. T-4: ToolUpgradeScreen UI 계층 구성

**목적**: Canvas_Overlay 하위에 대장간 업그레이드 UI(ToolUpgradeScreen)를 blacksmith-architecture.md 섹션 3.1의 계층 구조대로 구성한다.

**전제**: T-1 Phase 4 완료 (UI 스크립트). UIManager(ARC-018) 및 Canvas_Overlay(ARC-002) 존재.

---

### T-4-01: ToolUpgradeScreen 루트 생성

```
create_object
  name: "ToolUpgradeScreen"
  parent: "Canvas_Overlay"

add_component
  object: "ToolUpgradeScreen"
  component: "RectTransform"
  // stretch to fill parent

add_component
  object: "ToolUpgradeScreen"
  component: "CanvasGroup"

set_property  component: "CanvasGroup" property: "alpha" value: 0
set_property  component: "CanvasGroup" property: "blocksRaycasts" value: false
set_property  component: "CanvasGroup" property: "interactable" value: false

add_component
  object: "ToolUpgradeScreen"
  component: "SeedMind.UI.ToolUpgradeUI"
```

- **MCP 호출**: 6회

### T-4-02: Background (반투명 검정 오버레이)

```
create_object  name: "Background"  parent: "ToolUpgradeScreen"
add_component  object: "Background"  component: "Image"
set_property   component: "Image" property: "color" value: [0, 0, 0, 0.6]
    // RectTransform: stretch to fill parent
```

- **MCP 호출**: 3회

### T-4-03: MainPanel (중앙 정렬)

```
create_object  name: "MainPanel"  parent: "ToolUpgradeScreen"
    // RectTransform: 중앙 정렬, sizeDelta: (800, 600)
add_component  object: "MainPanel"  component: "Image"
    // 패널 배경 이미지
```

- **MCP 호출**: 2회

### T-4-04: Header (타이틀 + 닫기 버튼)

```
create_object  name: "Header"  parent: "MainPanel"
add_component  component: "HorizontalLayoutGroup"

create_object  name: "TitleText"  parent: "Header"
add_component  component: "TextMeshProUGUI"
set_property   property: "text" value: "도구 업그레이드"

create_object  name: "CloseButton"  parent: "Header"
add_component  component: "Button"
```

- **MCP 호출**: 6회

### T-4-05 ~ T-4-09: ContentArea 좌우 분할

```
# T-4-05: ContentArea (HorizontalLayoutGroup)
create_object  name: "ContentArea"  parent: "MainPanel"
add_component  component: "HorizontalLayoutGroup"

# T-4-06: LeftPanel (VerticalLayoutGroup, 비중 40%)
create_object  name: "LeftPanel"  parent: "ContentArea"
add_component  component: "VerticalLayoutGroup"
# + ToolListLabel (TMP, "보유 도구")
# + ToolSlotContainer (VerticalLayoutGroup, ToolSlot_0~2)

# T-4-07: RightPanel (VerticalLayoutGroup, 비중 60%)
create_object  name: "RightPanel"  parent: "ContentArea"
add_component  component: "VerticalLayoutGroup"
```

- **MCP 호출**: 6회

### T-4-10: LeftPanel 내부 -- ToolSlotContainer

```
create_object  name: "ToolListLabel"  parent: "LeftPanel"
add_component  component: "TextMeshProUGUI"
set_property   property: "text" value: "보유 도구"

create_object  name: "ToolSlotContainer"  parent: "LeftPanel"
add_component  component: "VerticalLayoutGroup"
```

- **MCP 호출**: 4회

### T-4-11: RightPanel 내부 -- ComparisonView

```
create_object  name: "ComparisonView"  parent: "RightPanel"
add_component  component: "HorizontalLayoutGroup"

create_object  name: "CurrentToolPanel"  parent: "ComparisonView"
add_component  component: "SeedMind.UI.ToolComparisonPanel"

create_object  name: "ArrowIcon"  parent: "ComparisonView"
add_component  component: "Image"

create_object  name: "UpgradedToolPanel"  parent: "ComparisonView"
add_component  component: "SeedMind.UI.ToolComparisonPanel"
```

- **MCP 호출**: 8회
- **ToolComparisonPanel 내부 요소** (Label, ToolIcon, ToolName, TierText, StatsGroup): 각 패널 내에서 create_object + add_component로 구성. 세부 레이아웃은 (-> see blacksmith-architecture.md 섹션 3.1 ComparisonView)

### T-4-12: RightPanel 내부 -- CostPanel

```
create_object  name: "CostPanel"  parent: "RightPanel"
add_component  component: "VerticalLayoutGroup"

# CostLabel, GoldCost, MaterialSlotList, TimeCost, LevelRequirement
# 세부 구조: (-> see blacksmith-architecture.md 섹션 3.1 CostPanel)
```

- **MCP 호출**: 약 6회

### T-4-13: Footer (취소 + 업그레이드 시작 버튼)

```
create_object  name: "Footer"  parent: "MainPanel"
add_component  component: "HorizontalLayoutGroup"

create_object  name: "CancelButton"  parent: "Footer"
add_component  component: "Button"

create_object  name: "UpgradeButton"  parent: "Footer"
add_component  component: "Button"
```

- **MCP 호출**: 6회

### T-4-14: ConfirmPopup (확인 팝업, 기본 비활성)

```
create_object  name: "ConfirmPopup"  parent: "ToolUpgradeScreen"
set_property   property: "activeSelf" value: false

# ConfirmText, ConfirmButton, CancelButton 하위 생성
```

- **MCP 호출**: 4회

### T-4-15: ToolUpgradeUI SerializeField 참조 연결

- **액션**: ToolUpgradeUI 컴포넌트의 SerializeField에 생성된 UI 오브젝트를 참조 연결

```
set_property  path: "ToolUpgradeScreen" component: "ToolUpgradeUI"
              property: "_toolSlotContainer" value: ref("ToolSlotContainer")
set_property  property: "_currentToolPanel" value: ref("CurrentToolPanel")
set_property  property: "_upgradedToolPanel" value: ref("UpgradedToolPanel")
set_property  property: "_goldCostText" value: ref("GoldCostText")
set_property  property: "_timeCostText" value: ref("TimeCostText")
set_property  property: "_levelRequirementText" value: ref("LevelRequirementText")
set_property  property: "_upgradeButton" value: ref("UpgradeButton")
set_property  property: "_cancelButton" value: ref("CancelButton")
set_property  property: "_closeButton" value: ref("CloseButton")
set_property  property: "_confirmPopup" value: ref("ConfirmPopup")
set_property  property: "_confirmText" value: ref("ConfirmText")
set_property  property: "_confirmYesButton" value: ref("ConfirmYesButton")
set_property  property: "_confirmNoButton" value: ref("ConfirmNoButton")
```

- **MCP 호출**: 13회

---

### T-4 완료 기준

- [ ] Canvas_Overlay 하위에 ToolUpgradeScreen이 존재하고 기본 비활성(alpha=0)
- [ ] ToolUpgradeUI 컴포넌트의 모든 SerializeField가 None이 아님
- [ ] MainPanel 내부에 Header/ContentArea/Footer 계층이 올바르게 구성됨
- [ ] ComparisonView에 CurrentToolPanel + ArrowIcon + UpgradedToolPanel이 존재
- [ ] ConfirmPopup이 비활성 상태로 존재

---

## 6. T-5: 씬 배치 및 참조 연결

**목적**: NPCAffinityTracker를 씬에 배치하고, UIManager에 ToolUpgradeScreen을 등록하며, 시스템 간 참조를 연결한다.

**전제**: T-1~T-4 완료. --- MANAGERS --- 계층 존재.

---

### T-5-01: NPCAffinityTracker 씬 배치

```
create_object
  name: "NPCAffinityTracker"
  parent: "--- MANAGERS ---"

add_component
  object: "NPCAffinityTracker"
  component: "SeedMind.NPC.NPCAffinityTracker"
```

- **검증**: --- MANAGERS --- 하위에 NPCAffinityTracker 존재
- **MCP 호출**: 2회

### T-5-02: UIManager에 ToolUpgradeScreen 등록

```
set_property
  path: "UIManager"
  component: "UIManager"
  property: "_screens[ScreenType.ToolUpgrade]"
  value: ref("ToolUpgradeScreen")
    // ScreenType.ToolUpgrade = 11
    // -> see docs/systems/ui-architecture.md 섹션 1.2
```

- **검증**: UIManager에서 `ShowScreen(ScreenType.ToolUpgrade)` 호출 시 ToolUpgradeScreen이 활성화됨
- **에러 처리**: Dictionary 직접 설정 불가 시, UIManager의 `[SerializeField] ScreenEntry[] _screenEntries` 배열에 항목 추가
- **MCP 호출**: 1회

[RISK] UIManager의 Screen 등록 방식이 Dictionary인 경우 MCP에서 직접 설정이 불가능할 수 있다. 대안: UIManager에 `RegisterScreen(ScreenType, ScreenBase)` 메서드를 추가하고, ToolUpgradeUI의 Awake에서 자동 등록하는 패턴을 사용.

### T-5-03: BlacksmithNPC에 외부 참조 연결

```
# NPCAffinityTracker는 싱글턴이므로 런타임에서 FindObjectOfType로 캐싱
# ToolUpgradeSystem, DialogueSystem도 동일
# SerializeField가 아닌 경우 MCP에서 설정 불필요

# 명시적 SerializeField 참조가 필요한 경우:
set_property  path: "NPC_Blacksmith" component: "BlacksmithNPC"
              property: "_affinityTracker" value: ref("NPCAffinityTracker")
```

- **MCP 호출**: 1회 (필요 시)

### T-5-04: 기존 시스템 수정 -- ProgressionManager XPSource 연동

```
# ProgressionManager가 ToolUpgradeEvents.OnUpgradeCompleted를 구독하여
# XPSource.ToolUpgrade로 XP를 부여하는 핸들러 추가
# -> see blacksmith-architecture.md 섹션 6.2

# 이는 스크립트 수정 사항이므로 T-1 또는 별도 패치에서 처리
# MCP에서는 런타임 연결만 확인
```

- **MCP 호출**: 0회 (스크립트 수정 범위)

### T-5-05: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

---

### T-5 완료 기준

- [ ] NPCAffinityTracker가 --- MANAGERS --- 하위에 배치됨
- [ ] UIManager에 ScreenType.ToolUpgrade가 등록됨
- [ ] BlacksmithNPC의 외부 참조(affinityTracker 등)가 연결됨
- [ ] 씬 저장 완료

---

## 7. T-6: 통합 테스트 시퀀스

**목적**: 대장간 NPC 상호작용의 전체 흐름을 Play Mode에서 검증한다.

**전제**: T-1~T-5 전체 완료. ARC-015(도구 업그레이드 시스템) 전체 완료.

---

### T-6-01: Play Mode 진입 및 초기화 확인

- **액션**:

```
enter_play_mode

get_console_logs
    # 확인 항목:
    # - "BlacksmithNPC initialized" 로그
    # - "NPCAffinityTracker initialized" 로그
    # - "ToolUpgradeUI registered as ScreenType.ToolUpgrade" 로그
    # - 콘솔 에러 없음
```

- **검증**: 세 시스템 모두 에러 없이 초기화됨
- **에러 처리**: NullReferenceException 발생 시 참조 연결(T-3, T-5) 재확인
- **MCP 호출**: 2회

### T-6-02: NPC 인터랙션 진입 테스트

- **액션**:

```
execute_method
    # 플레이어를 BlacksmithNPC 인터랙션 범위 내로 이동
    # PlayerController.SetPosition(NPC_Blacksmith.position + offset)

execute_method
    # NPC_Blacksmith의 BlacksmithNPC.Interact() 호출

get_console_logs
    # 확인 항목:
    # - 인사 대화 재생 시작 로그
    # - BlacksmithInteractionState가 Greeting으로 전이
    # - DialogueUI 활성화
```

- **검증**: Greeting 상태에서 철수 인사 대사가 표시됨
- **MCP 호출**: 3회

### T-6-03: 서비스 메뉴 표시 테스트

- **액션**:

```
execute_method
    # DialogueSystem.CompleteCurrentNode() -- 대화 진행

get_console_logs
    # 확인 항목:
    # - BlacksmithInteractionState가 ServiceMenu로 전이
    # - 선택지 5종 표시: 업그레이드, 수령(비활성), 재료구매, 이야기하기, 나가기
```

- **검증**: ServiceMenu에 올바른 선택지가 표시됨. "도구 수령"은 완성 도구 없으므로 비활성
- **MCP 호출**: 2회

### T-6-04: 업그레이드 UI 열림 테스트

- **액션**:

```
execute_method
    # BlacksmithNPC.HandleDialogueChoice(DialogueChoiceAction.OpenUpgrade)

get_console_logs
    # 확인 항목:
    # - UIManager가 ScreenType.ToolUpgrade로 전환
    # - ToolUpgradeUI.OnBeforeOpen() 호출됨
    # - 도구 3종(호미, 물뿌리개, 낫) 슬롯 표시
```

- **검증**: ToolUpgradeScreen이 활성화되고 도구 목록이 표시됨
- **MCP 호출**: 2회

### T-6-05: 조건 검증 테스트

- **액션**:

```
execute_method
    # ToolUpgradeUI.SelectTool(SO_Tool_Hoe)

get_console_logs
    # 확인 항목:
    # - ComparisonView에 현재 등급 vs 다음 등급 스탯 표시
    # - CostPanel에 골드/재료/시간 비용 표시
    # - 조건 부족 시 UpgradeButton 비활성
    # - 조건 충족 시 UpgradeButton 활성
```

- **검증**: 업그레이드 조건이 올바르게 검증되고 UI에 반영됨
- **에러 처리**: 비용 수치가 올바른지 확인 (-> see docs/systems/tool-upgrade.md 섹션 2.1)
- **MCP 호출**: 2회

### T-6-06: 업그레이드 의뢰 테스트

- **액션**:

```
# 테스트용 골드/재료를 충분히 지급
execute_method
    # EconomyManager.AddGold(9999)
    # InventoryManager.AddItem("iron_scrap", 99)

execute_method
    # ToolUpgradeUI.OnConfirmUpgrade()

get_console_logs
    # 확인 항목:
    # - ToolUpgradeSystem.StartUpgrade() 호출됨
    # - 골드/재료 차감 확인
    # - 도구 잠금(isLocked=true) 확인
    # - BlacksmithEvents.OnUpgradeRequested 이벤트 발행
    # - ToolUpgradeManager의 activeUpgrades에 등록
    # - BlacksmithInteractionState -> UpgradeResult 전이
```

- **검증**: 업그레이드가 정상적으로 시작되고 도구가 잠금 상태로 전환됨
- **MCP 호출**: 3회

### T-6-07: 도구 잠금 상태 확인

- **액션**:

```
execute_method
    # InventoryManager.IsToolLocked(ToolType.Hoe) 확인

get_console_logs
    # 확인 항목:
    # - 잠긴 도구 슬롯에 잠금 아이콘 오버레이 표시
    # - 잠긴 도구 선택 시도 시 "업그레이드 중입니다" 메시지
```

- **검증**: 도구 잠금이 인벤토리와 툴바 UI에 모두 반영됨
- **MCP 호출**: 2회

### T-6-08: 일수 경과 시뮬레이션 + 업그레이드 완료

- **액션**:

```
execute_method
    # TimeManager.AdvanceDay() 반복 호출 (업그레이드 소요 일수만큼)
    # -> see docs/systems/tool-upgrade.md 섹션 2.1 for 소요 일수

get_console_logs
    # 확인 항목:
    # - 매일 새벽에 remainingDays-- 처리
    # - remainingDays == 0 도달 시 ToolUpgradeEvents.OnUpgradeCompleted 발행
    # - NPC 위에 알림 아이콘 표시
```

- **검증**: 업그레이드가 지정 일수 후 완료 상태로 전환됨
- **MCP 호출**: 2회

### T-6-09: 도구 수령 테스트

- **액션**:

```
execute_method
    # BlacksmithNPC.Interact() -- 재방문
    # 인사 대화에서 pendingPickupDialogue 재생 확인
    # ServiceMenu에서 "도구 수령" 활성화 확인

execute_method
    # "도구 수령" 선택 -> PickupResult 상태

get_console_logs
    # 확인 항목:
    # - 도구 등급 변경 (Basic -> Reinforced)
    # - 도구 잠금 해제 (isLocked=false)
    # - 스프라이트 교체
    # - BlacksmithEvents.OnToolPickedUp 발행
    # - ProgressionManager XP 부여
    # - 친밀도 증가 (upgradeCompleteAffinity)
```

- **검증**: 수령 후 도구 등급이 올바르게 변경되고, 잠금 해제, XP 부여, 친밀도 증가가 모두 처리됨
- **MCP 호출**: 3회

### T-6-10: 친밀도 시스템 테스트

- **액션**:

```
execute_method
    # NPCAffinityTracker에 친밀도를 수동으로 설정하여 단계 전환 테스트
    # AddAffinity("npc_cheolsu", 10) -> Lv1 전환

get_console_logs
    # 확인 항목:
    # - OnAffinityLevelUp 이벤트 발행
    # - 다음 방문 시 greetingDialogues[1] 재생
    # - 단계 상승 특수 대화(affinityDialogues[0]) 1회 재생

execute_method
    # AddAffinity("npc_cheolsu", 40) -> Lv3 전환
    # 재료 구매 시 할인 적용 확인 (discountRate = 0.1)
```

- **검증**: 친밀도 단계 전환이 올바르게 작동하고, 대화/혜택이 단계에 맞게 변경됨
- **MCP 호출**: 3회

### T-6-11: 세이브/로드 테스트

- **액션**:

```
execute_method
    # SaveManager.Save() 호출

execute_method
    # 업그레이드 진행 중 상태 + 친밀도 값이 포함된 세이브 데이터 생성 확인

exit_play_mode
enter_play_mode

execute_method
    # SaveManager.Load() 호출

get_console_logs
    # 확인 항목:
    # - 친밀도 값 복원 (AffinitySaveData)
    # - 업그레이드 진행 중이었다면 잔여 일수 복원
    # - 도구 잠금 상태 복원
    # - 단계별 특수 대화 재생 여부 복원 (triggeredDialogues)
```

- **검증**: 세이브/로드 후 모든 상태가 올바르게 복원됨
- **에러 처리**: SaveManager ISaveable 인터페이스 미구현 시 해당 스크립트에 구현 추가 필요
- **MCP 호출**: 5회

### T-6-12: 엣지 케이스 테스트

- **액션**:

```
# 동시 복수 업그레이드: 3종 도구를 모두 업그레이드 의뢰
execute_method
    # 3종 동시 업그레이드 후 도구 전부 잠금 확인
    # "사용 가능한 도구 없음" 경고 UI 표시 확인

# 영업시간 외 방문:
execute_method
    # TimeManager.SetHour(22) -- 영업시간 외
    # BlacksmithNPC.Interact() -> closedDialogue 재생 확인

get_console_logs
```

- **검증**: 엣지 케이스가 올바르게 처리됨 (-> see tool-upgrade-design-analysis.md 섹션 5.7)
- **MCP 호출**: 3회

### T-6-13: Play Mode 종료

```
exit_play_mode
save_scene
```

- **MCP 호출**: 2회

---

### T-6 완료 기준

- [ ] NPC 인터랙션 진입 -> 인사 대화 -> 서비스 메뉴 흐름 정상
- [ ] 업그레이드 UI에서 도구 선택, 조건 검증, 비용 표시 정상
- [ ] 업그레이드 의뢰 -> 골드/재료 차감, 도구 잠금 정상
- [ ] 일수 경과 -> 업그레이드 완료 -> 알림 아이콘 정상
- [ ] 도구 수령 -> 등급 변경, 잠금 해제, XP 부여, 친밀도 증가 정상
- [ ] 친밀도 단계 전환 -> 인사말 변경, 특수 대화, 할인 혜택 정상
- [ ] 세이브/로드 -> 친밀도, 업그레이드 진행, 잠금 상태 복원 정상
- [ ] 엣지 케이스(동시 업그레이드, 영업시간 외) 처리 정상

---

## 8. 의존성 그래프

```
ARC-002 (scene-setup)
    │
    ├──▶ ARC-003 (farming) ──▶ ARC-015 (tool-upgrade)
    │                                    │
    │                                    ├──▶ ARC-020 T-1 (스크립트)
    │                                    │
    ├──▶ ARC-009 (npc-shop) ─────────────┤
    │         │                          │
    │         ├── NPCData SO ────────────┤
    │         ├── NPCController ─────────┤
    │         ├── DialogueSystem ────────┤
    │         └── NPC_Blacksmith ────────┤
    │                                    │
    ├──▶ ARC-018 (ui-architecture) ──────┤
    │         │                          │
    │         ├── UIManager ─────────────┤
    │         └── ScreenBase ────────────┤
    │                                    │
    │                              ARC-020 T-1 (스크립트)
    │                                    │
    │                              ARC-020 T-2 (SO 에셋)
    │                                    │
    │                              ARC-020 T-3 (NPC 프리팹 확장)
    │                                    │
    │                              ARC-020 T-4 (UI 계층 구성)
    │                                    │
    │                              ARC-020 T-5 (씬 배치/연결)
    │                                    │
    │                              ARC-020 T-6 (통합 테스트)
    │
    └──▶ economy-architecture (EconomyManager, ShopSystem)
         progression-architecture (ProgressionManager, XPSource)
         save-load-architecture (SaveManager, ISaveable)
```

### 선후 관계 요약

| 선행 태스크 | 후행 태스크 | 의존 내용 |
|------------|------------|-----------|
| ARC-009 T-1 | ARC-020 T-1 | DialogueData, NPCData 타입 의존 |
| ARC-015 전체 | ARC-020 T-1 | ToolUpgradeSystem, ToolUpgradeEvents, ToolData SO |
| ARC-018 | ARC-020 T-1 Phase 4 | ScreenBase 클래스 의존 |
| ARC-020 T-1 | ARC-020 T-2 | BlacksmithNPCData SO 타입 컴파일 필요 |
| ARC-020 T-1 | ARC-020 T-3 | BlacksmithNPC 스크립트 컴파일 필요 |
| ARC-020 T-1 | ARC-020 T-4 | ToolUpgradeUI 스크립트 컴파일 필요 |
| ARC-020 T-2 | ARC-020 T-3 | SO_BlacksmithNPC_Cheolsu 에셋 참조 |
| ARC-020 T-3, T-4 | ARC-020 T-5 | 씬 오브젝트 존재 필요 |
| ARC-020 T-1~T-5 | ARC-020 T-6 | 전체 시스템 완성 후 통합 테스트 |

---

## 9. Cross-references

| 문서 | 참조 내용 |
|------|-----------|
| `docs/systems/blacksmith-architecture.md` (ARC-020) | 클래스 다이어그램, State Machine, UI 레이아웃, SO 스키마, 이벤트 설계 -- 본 태스크의 설계 원본 |
| `docs/content/blacksmith-npc.md` (CON-004) | 철수 캐릭터, 대화 스크립트, 친밀도 4단계, 영업 조건 (canonical) |
| `docs/mcp/tool-upgrade-design-analysis.md` | MCP 태스크 설계를 위한 디자인 분석 (Phase 분할, 엣지 케이스) |
| `docs/mcp/npc-shop-tasks.md` (ARC-009) | NPC/상점 MCP 태스크 포맷 레퍼런스, NPCData/DialogueData 스크립트 원본 |
| `docs/mcp/facilities-tasks.md` (ARC-007) | MCP 태스크 포맷 레퍼런스 |
| `docs/mcp/tool-upgrade-tasks.md` (ARC-015) | ToolUpgradeSystem, ToolData SO 생성 태스크 |
| `docs/systems/npc-shop-architecture.md` (ARC-008) | NPCController, DialogueSystem, NPCEvents 기반 구조 |
| `docs/systems/tool-upgrade-architecture.md` (DES-007) | ToolUpgradeSystem, ToolData SO, ToolEffectResolver |
| `docs/systems/tool-upgrade.md` | 업그레이드 비용, 재료, 등급, 소요 일수, 도구별 성능 수치 (canonical) |
| `docs/systems/ui-architecture.md` (ARC-018) | UIManager, ScreenBase, ScreenType enum, PopupQueue |
| `docs/systems/progression-architecture.md` | ProgressionManager, XPSource enum |
| `docs/systems/achievement-architecture.md` | AchievementManager, AchievementConditionType |
| `docs/systems/economy-architecture.md` | ShopSystem, EconomyManager |
| `docs/systems/save-load-architecture.md` | SaveManager, ISaveable 인터페이스 |
| `docs/content/npcs.md` (CON-003) | NPC 기본 캐릭터 설정, 대화 시스템 구조 |
| `docs/systems/project-structure.md` | 프로젝트 폴더 구조, 네임스페이스 규칙 |

---

## 10. Open Questions

1. [OPEN] **NPC 친밀도 업적 추가 여부**: `AchievementConditionType.NPCAffinityLevel`을 추가하여 "대장장이와 오랜 친구가 되다" 업적을 도입할지. 도입 시 `achievement-architecture.md`에 조건 타입 추가 + `AchievementManager`에 `BlacksmithEvents.OnAffinityLevelUp` 구독 핸들러 추가 필요.

2. [OPEN] **마을 씬 구조**: NPC와 마을 건물이 FarmScene에 포함되는지, 별도 VillageScene으로 분리되는지. T-3의 NPC 배치 위치와 InteractionZone 크기에 영향. 현재 설계에서는 FarmScene 단일 씬을 가정.

3. [OPEN] **NPCAffinityTracker 배치 위치**: 독립 싱글턴으로 둘지(현재 설계), NPCManager의 서브 시스템으로 통합할지. 독립 배치 시 --- MANAGERS --- 하위에 별도 GameObject, 통합 시 NPCManager 컴포넌트 내부로 이동.

4. [OPEN] **DialogueData 중첩 배열 MCP 설정 가능 여부**: `nodes[].choices[]` 중첩 배열을 MCP `set_property`로 설정할 수 있는지 사전 검증 필요. 불가능 시 Editor 스크립트 우회 경로 확보.

5. [OPEN] **재료 드롭 경로 MCP 범위**: 상점 구매 외 몬스터/채굴 드롭을 통한 재료 획득 시스템이 ARC-020 범위에 포함되는지. 현재는 대장간 상점 구매만 범위 내.

6. [OPEN] **친밀도 단계 3(Friend) 추가 혜택**: 현재 재료 구매 10% 할인만 정의됨. 업그레이드 소요 시간 단축 등 추가 혜택 도입 여부.

---

## Risks

- [RISK] **MCP SO 배열 참조 설정**: T-2-13에서 BlacksmithNPCData의 `greetingDialogues[]`, `affinityDialogues[]` 배열에 DialogueData SO 참조를 설정할 때 MCP 지원 여부 사전 검증 필요. 대안: string ID 기반 런타임 조회 (DataRegistry).

- [RISK] **DialogueData 중첩 배열**: `DialogueNode.choices[]` 배열 설정이 `set_property`로 불가능할 수 있음. Editor 스크립트(CreateBlacksmithDialogues.cs) 우회 필요.

- [RISK] **ScreenType enum 동기화**: `ToolUpgrade = 11` 추가 시 `ui-architecture.md` 섹션 1.2의 enum 정의, 상태 전이 규칙, Screen별 구현 요약 표를 동시에 업데이트해야 한다.

- [RISK] **XPSource enum 동기화**: `XPSource.ToolUpgrade` 추가 시 `progression-architecture.md` 섹션 2.2 enum 및 섹션 2.3 `GetExpForSource()` switch 문을 동시에 업데이트해야 한다.

- [RISK] **총 156회 MCP 호출**: 호출 수가 많아 실행 시간이 상당할 수 있다. Editor 스크립트를 통한 SO 일괄 생성(T-2), UI 일괄 구성(T-4)으로 호출 수를 ~60회로 절감 가능.

- [RISK] **친밀도 시스템 범용성**: NPCAffinityTracker를 범용으로 설계했으나, NPC별 친밀도 증가 방식/보상이 크게 다를 경우 전략 패턴 등 추가 추상화가 필요할 수 있다.

---

*이 문서는 `docs/systems/blacksmith-architecture.md`(ARC-020) Part II를 독립 확장한 MCP 태스크 시퀀스이다. 모든 게임플레이 수치는 canonical 문서를 참조하며, 직접 기재하지 않는다.*
