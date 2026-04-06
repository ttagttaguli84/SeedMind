# NPC/상점 시스템 MCP 태스크 시퀀스

> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: ARC-009  
> 기반 문서: docs/systems/npc-shop-architecture.md (ARC-008)

---

## Context

이 문서는 `docs/systems/npc-shop-architecture.md`(ARC-008) 섹션 8의 Phase A~F 개요를 상세한 MCP 태스크 시퀀스로 확장한다. NPC 데이터 구조(SO/enum), 대화 시스템, NPC 매니저, 씬 배치, 기존 시스템 연동(ShopSystem 확장, ToolUpgradeSystem 대장간 연계), 통합 테스트까지 MCP for Unity 도구 호출 수준의 구체적 명세를 포함한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 NPC/상점 시스템의 데이터 레이어(NPCData SO 4종 + DialogueData SO 6종 + TravelingShopPoolData SO 1종), 시스템 레이어(스크립트 14종), UI 레이어(DialoguePanel 프리팹), 씬 배치를 완성한다.

**전제 조건**:
- ARC-002(scene-setup-tasks.md) Phase A~B 완료: 폴더 구조, SCN_Farm 기본 계층(MANAGERS, UI)
- ARC-003(farming-tasks.md) 완료: 기본 시스템 인프라
- economy-architecture.md 기반 ShopSystem, ShopData, EconomyManager 구현 완료
- ARC-015(tool-upgrade-tasks.md) 완료: ToolUpgradeSystem, ToolUpgradeEvents 구현 완료

---

## 1. 개요

### 1.1 태스크 맵

| 태스크 | 설명 | MCP 호출 수 |
|--------|------|------------|
| T-1 | 스크립트 생성 (enum, SO 클래스, 시스템 클래스, UI 클래스) | 17회 |
| T-2 | SO 에셋 생성 (NPCData 4종 + DialogueData 6종 + TravelingShopPool 1종) | 67회 |
| T-3 | UI 프리팹/씬 오브젝트 생성 (DialoguePanel) | 24회 |
| T-4 | 씬 배치 및 참조 연결 (G-09 프리팹 등록 포함) | 34회 |
| T-5 | 기존 시스템 연동 설정 | 6회 |
| T-6 | 통합 테스트 시퀀스 | 18회 |
| **합계** | | **~166회** |

[RISK] 총 ~166회 MCP 호출은 상당하다. 특히 T-2의 SO 에셋 생성에서 DialogueData의 중첩 배열(DialogueNode[] 내 DialogueChoice[]) 설정이 MCP set_property로 가능한지 사전 검증 필요. 불가능한 경우 Editor 스크립트(CreateNPCAssets.cs)를 통한 일괄 생성으로 T-2의 67회를 ~8회로 감소시킬 수 있다.

### 1.2 스크립트 목록

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/NPC/Data/NPCType.cs` | `NPCType` (enum) | `SeedMind.NPC` | T-1 Phase 1 |
| S-02 | `Scripts/NPC/Data/NPCActivityState.cs` | `NPCActivityState` (enum) | `SeedMind.NPC` | T-1 Phase 1 |
| S-03 | `Scripts/NPC/Data/DayFlag.cs` | `DayFlag` ([Flags] enum) | `SeedMind.NPC` | T-1 Phase 1 |
| S-04 | `Scripts/NPC/Data/DialogueChoiceAction.cs` | `DialogueChoiceAction` (enum) | `SeedMind.NPC.Data` | T-1 Phase 1 |
| S-05 | `Scripts/NPC/Data/DialogueChoice.cs` | `DialogueChoice` (직렬화 클래스) | `SeedMind.NPC.Data` | T-1 Phase 1 |
| S-06 | `Scripts/NPC/Data/DialogueNode.cs` | `DialogueNode` (직렬화 클래스) | `SeedMind.NPC.Data` | T-1 Phase 1 |
| S-07 | `Scripts/NPC/Data/DialogueData.cs` | `DialogueData` (ScriptableObject) | `SeedMind.NPC.Data` | T-1 Phase 1 |
| S-08 | `Scripts/NPC/Data/NPCData.cs` | `NPCData` (ScriptableObject) | `SeedMind.NPC.Data` | T-1 Phase 1 |
| S-09 | `Scripts/NPC/Data/TravelingShopPoolData.cs` | `TravelingShopPoolData` (ScriptableObject) | `SeedMind.NPC.Data` | T-1 Phase 1 |
| S-10 | `Scripts/NPC/Data/NPCSaveData.cs` | `NPCSaveData`, `TravelingMerchantSaveData` (직렬화 클래스) | `SeedMind.NPC.Data` | T-1 Phase 1 |
| S-11 | `Scripts/NPC/NPCEvents.cs` | `NPCEvents` (static class) | `SeedMind.NPC` | T-1 Phase 2 |
| S-12 | `Scripts/NPC/NPCManager.cs` | `NPCManager` (MonoBehaviour Singleton) | `SeedMind.NPC` | T-1 Phase 3 |
| S-13 | `Scripts/NPC/NPCController.cs` | `NPCController` (MonoBehaviour) | `SeedMind.NPC` | T-1 Phase 3 |
| S-14 | `Scripts/NPC/DialogueSystem.cs` | `DialogueSystem` (MonoBehaviour Singleton) | `SeedMind.NPC` | T-1 Phase 3 |
| S-15 | `Scripts/NPC/TravelingMerchantScheduler.cs` | `TravelingMerchantScheduler` (MonoBehaviour) | `SeedMind.NPC` | T-1 Phase 3 |
| S-16 | `Scripts/UI/DialogueUI.cs` | `DialogueUI` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 4 |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: S-01~S-10 -> S-11 -> S-12~S-15 -> S-16. 각 Phase 사이에 Unity 컴파일 대기(`execute_menu_item`)가 필요하다.

### 1.3 SO 에셋 목록

| # | 에셋명 | 경로 | SO 타입 | 생성 태스크 |
|---|--------|------|---------|-----------|
| A-01 | `SO_NPC_GeneralMerchant.asset` | `Assets/_Project/Data/NPCs/` | NPCData | T-2-02 |
| A-02 | `SO_NPC_Blacksmith.asset` | `Assets/_Project/Data/NPCs/` | NPCData | T-2-03 |
| A-03 | `SO_NPC_Carpenter.asset` | `Assets/_Project/Data/NPCs/` | NPCData | T-2-04 |
| A-04 | `SO_NPC_TravelingMerchant.asset` | `Assets/_Project/Data/NPCs/` | NPCData | T-2-05 |
| A-05 | `SO_TravelingPool_Default.asset` | `Assets/_Project/Data/NPCs/` | TravelingShopPoolData | T-2-06 |
| A-06 | `SO_Dlg_Greeting_Merchant.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-07 |
| A-07 | `SO_Dlg_Greeting_Blacksmith.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-08 |
| A-08 | `SO_Dlg_Greeting_Carpenter.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-09 |
| A-09 | `SO_Dlg_Closed_Merchant.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-10 |
| A-10 | `SO_Dlg_Closed_Blacksmith.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-11 |
| A-11 | `SO_Dlg_Closed_Carpenter.asset` | `Assets/_Project/Data/Dialogues/` | DialogueData | T-2-12 |

### 1.4 씬 GameObject 목록

| # | 오브젝트명 | 부모 | 컴포넌트 | 생성 태스크 |
|---|-----------|------|----------|-----------|
| G-01 | `--- NPCs ---` | SCN_Farm (root) | 없음 (구분용) | T-4-01 |
| G-02 | `NPC_GeneralMerchant` | `--- NPCs ---` | NPCController | T-4-02 |
| G-03 | `NPC_Blacksmith` | `--- NPCs ---` | NPCController | T-4-03 |
| G-04 | `NPC_Carpenter` | `--- NPCs ---` | NPCController | T-4-04 |
| G-05 | `NPCManager` | `--- MANAGERS ---` | NPCManager | T-4-05 |
| G-06 | `DialogueSystem` | `--- MANAGERS ---` | DialogueSystem | T-4-06 |
| G-07 | `TravelingMerchantScheduler` | `--- MANAGERS ---` | TravelingMerchantScheduler | T-4-07 |
| G-08 | `DialoguePanel` | `Canvas_Overlay` | DialogueUI | T-3-01 |
| G-09 | `NPC_TravelingMerchant` | `--- NPCs ---` | NPCController | 동적 (T-4-08) |

> **G-09 참고**: `NPC_TravelingMerchant`는 씬에 상시 배치되지 않는다. `TravelingMerchantScheduler`가 런타임에 프리팹을 인스턴스화/파괴하며, T-4-08에서 프리팹 등록만 수행한다.

### 1.5 선행 태스크에서 생성되는 오브젝트

이 태스크 시퀀스에서 직접 생성하지 않지만, 참조 연결이 필요한 오브젝트 목록이다. 중복 생성하지 않고 참조만 설정한다.

| 오브젝트 | 생성 태스크 | 비고 |
|---------|------------|------|
| BlacksmithPanel (UpgradePanel) | ARC-015 (tool-upgrade-tasks.md) | 대장간 NPC 대화에서 OpenUpgrade 시 참조 |

### 1.6 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `Canvas_Overlay` (UI 루트) | ARC-002 Phase B |
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A |
| `EconomyManager` | economy-architecture.md |
| `ShopSystem`, `ShopData` | economy-architecture.md 섹션 4.3 |
| `ToolUpgradeSystem`, `ToolUpgradeEvents` | ARC-015 |
| `InventoryManager` | inventory-architecture.md |
| `BuildingManager` | facilities-tasks.md (ARC-007) |

---

## MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋/스크립트 폴더 생성 | T-1, T-2 |
| `create_script` | C# 스크립트 파일 생성 | T-1 |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | T-2 |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | T-2~T-5 전체 |
| `create_object` | 빈 GameObject 생성 | T-3, T-4 |
| `add_component` | MonoBehaviour 컴포넌트 부착 | T-3, T-4 |
| `set_parent` | 오브젝트 부모 설정 | T-3, T-4 |
| `save_as_prefab` | GameObject를 프리팹으로 저장 | T-3 |
| `save_scene` | 씬 저장 | T-4, T-6 |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | T-6 |
| `execute_menu_item` | 편집기 명령 실행 (컴파일 대기 등) | T-1 |
| `execute_method` | 런타임 메서드 호출 (테스트) | T-6 |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | T-6 |

---

## 2. T-1: 스크립트 생성

**목적**: NPC/상점 시스템에 필요한 모든 C# 스크립트를 생성한다.

**전제**: Core 인프라(TimeManager, SaveManager 등) 컴파일 완료. Economy/Player/Building 모듈 컴파일 완료.

---

### T-1 Phase 1: 데이터 구조 스크립트 (S-01 ~ S-10)

#### T-1-01: 폴더 생성

```
create_folder
  path: "Assets/_Project/Scripts/NPC"

create_folder
  path: "Assets/_Project/Scripts/NPC/Data"
```

- **MCP 호출**: 2회

#### T-1-02: NPCType enum (S-01)

```
create_script
  path: "Assets/_Project/Scripts/NPC/Data/NPCType.cs"
  content: |
    // S-01: NPC 유형 열거형
    // -> see docs/systems/npc-shop-architecture.md 섹션 2.1
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

- **MCP 호출**: 1회

#### T-1-03: NPCActivityState enum (S-02)

```
create_script
  path: "Assets/_Project/Scripts/NPC/Data/NPCActivityState.cs"
  content: |
    // S-02: NPC 활동 상태 열거형
    // -> see docs/systems/npc-shop-architecture.md 섹션 3.2
    namespace SeedMind.NPC
    {
        public enum NPCActivityState
        {
            Active,           // 영업 중, 인터랙션 가능
            Closed,           // 영업 외 시간 또는 휴무일
            WeatherClosed,    // 악천후로 임시 마감 (-> see docs/systems/time-season.md 섹션 3.4)
            Away              // 부재 (여행 상인 미방문 상태)
        }
    }
```

- **MCP 호출**: 1회

#### T-1-04: DayFlag [Flags] enum (S-03)

```
create_script
  path: "Assets/_Project/Scripts/NPC/Data/DayFlag.cs"
  content: |
    // S-03: 요일 비트마스크 열거형
    // -> see docs/systems/npc-shop-architecture.md 섹션 2.1
    namespace SeedMind.NPC
    {
        [System.Flags]
        public enum DayFlag
        {
            None      = 0,
            Monday    = 1 << 0,
            Tuesday   = 1 << 1,
            Wednesday = 1 << 2,
            Thursday  = 1 << 3,
            Friday    = 1 << 4,
            Saturday  = 1 << 5,
            Sunday    = 1 << 6
        }
    }
```

- **MCP 호출**: 1회

#### T-1-05: DialogueChoiceAction enum (S-04)

```
create_script
  path: "Assets/_Project/Scripts/NPC/Data/DialogueChoiceAction.cs"
  content: |
    // S-04: 대화 선택지 액션 열거형
    // -> see docs/systems/npc-shop-architecture.md 섹션 2.2
    namespace SeedMind.NPC.Data
    {
        public enum DialogueChoiceAction
        {
            Continue      = 0,   // 다음 노드로 진행 (jumpToNode 사용)
            OpenShop      = 1,   // 상점 UI 열기
            OpenUpgrade   = 2,   // 도구 업그레이드 UI 열기
            OpenBuild     = 3,   // 시설 건설 UI 열기
            CloseDialogue = 4    // 대화 종료
        }
    }
```

- **MCP 호출**: 1회

#### T-1-06: DialogueChoice 직렬화 클래스 (S-05)

```
create_script
  path: "Assets/_Project/Scripts/NPC/Data/DialogueChoice.cs"
  content: |
    // S-05: 대화 선택지 데이터
    // -> see docs/systems/npc-shop-architecture.md 섹션 2.2
    namespace SeedMind.NPC.Data
    {
        [System.Serializable]
        public class DialogueChoice
        {
            public string choiceText;                 // 선택지 UI 텍스트
            public DialogueChoiceAction action;       // 선택 시 동작
            public int jumpToNode;                    // 점프할 노드 인덱스 (-1 = 대화 종료)
        }
    }
```

- **MCP 호출**: 1회

#### T-1-07: DialogueNode 직렬화 클래스 (S-06)

```
create_script
  path: "Assets/_Project/Scripts/NPC/Data/DialogueNode.cs"
  content: |
    // S-06: 대화 노드 데이터
    // -> see docs/systems/npc-shop-architecture.md 섹션 2.2
    using UnityEngine;

    namespace SeedMind.NPC.Data
    {
        [System.Serializable]
        public class DialogueNode
        {
            public string speakerName;                // 화자 이름
            [TextArea(2, 5)]
            public string text;                       // 대사 텍스트 (-> see docs/content/npcs.md)
            public DialogueChoice[] choices;          // 선택지 배열 (비어있으면 자동 다음 노드)
        }
    }
```

- **MCP 호출**: 1회

#### T-1-08: DialogueData ScriptableObject (S-07)

```
create_script
  path: "Assets/_Project/Scripts/NPC/Data/DialogueData.cs"
  content: |
    // S-07: 대화 흐름 ScriptableObject
    // -> see docs/systems/npc-shop-architecture.md 섹션 2.2
    using UnityEngine;

    namespace SeedMind.NPC.Data
    {
        [CreateAssetMenu(fileName = "NewDialogueData", menuName = "SeedMind/DialogueData")]
        public class DialogueData : ScriptableObject
        {
            public string dialogueId;                 // "greeting_merchant_spring"
            public DialogueNode[] nodes;              // 대화 노드 배열 (순서대로 진행)
        }
    }
```

- **MCP 호출**: 1회

#### T-1-09: NPCData ScriptableObject (S-08)

```
create_script
  path: "Assets/_Project/Scripts/NPC/Data/NPCData.cs"
  content: |
    // S-08: NPC 정적 데이터 ScriptableObject
    // -> see docs/systems/npc-shop-architecture.md 섹션 2.1
    using UnityEngine;

    namespace SeedMind.NPC.Data
    {
        [CreateAssetMenu(fileName = "NewNPCData", menuName = "SeedMind/NPCData")]
        public class NPCData : ScriptableObject
        {
            [Header("기본 정보")]
            public string npcId;                      // -> see docs/content/npcs.md
            public string displayName;                // -> see docs/content/npcs.md
            public NPCType npcType;
            public Sprite portrait;

            [Header("위치/스케줄")]
            public Vector3 defaultPosition;           // -> see docs/content/npcs.md
            public int openHour;                      // -> see docs/systems/time-season.md 섹션 1.7
            public int closeHour;                     // -> see docs/systems/time-season.md 섹션 1.7
            public DayFlag closedDays;                // -> see docs/systems/economy-system.md 섹션 3.2

            [Header("대화")]
            public DialogueData greetingDialogue;
            public DialogueData closedDialogue;

            [Header("상점 연결")]
            public ScriptableObject shopData;         // ShopData SO 참조 (null이면 상점 없음)
                                                      // -> see docs/systems/economy-architecture.md 섹션 4.3

            [Header("시각")]
            public GameObject prefab;
            public float interactionRadius;           // -> see docs/content/npcs.md
        }
    }
```

- **MCP 호출**: 1회

#### T-1-10: TravelingShopPoolData ScriptableObject (S-09)

```
create_script
  path: "Assets/_Project/Scripts/NPC/Data/TravelingShopPoolData.cs"
  content: |
    // S-09: 여행 상인 후보 아이템 풀 ScriptableObject
    // -> see docs/systems/npc-shop-architecture.md 섹션 2.3
    using UnityEngine;

    namespace SeedMind.NPC.Data
    {
        [CreateAssetMenu(fileName = "NewTravelingShopPool", menuName = "SeedMind/TravelingShopPool")]
        public class TravelingShopPoolData : ScriptableObject
        {
            public string poolId;

            [Header("후보 아이템")]
            public TravelingShopCandidate[] candidates;

            [Header("출현 규칙")]
            public int minItemCount;                  // -> see docs/content/npcs.md
            public int maxItemCount;                  // -> see docs/content/npcs.md
        }

        [System.Serializable]
        public class TravelingShopCandidate
        {
            public ScriptableObject itemReference;    // CropData, FertilizerData 등
            public float selectionWeight;
            public int minPlayerLevel;
            public int stockMin;
            public int stockMax;
        }
    }
```

- **MCP 호출**: 1회

#### T-1-11: NPCSaveData 직렬화 클래스 (S-10)

```
create_script
  path: "Assets/_Project/Scripts/NPC/Data/NPCSaveData.cs"
  content: |
    // S-10: NPC 세이브 데이터
    // -> see docs/systems/npc-shop-architecture.md 섹션 7.1~7.2
    namespace SeedMind.NPC.Data
    {
        [System.Serializable]
        public class NPCSaveData
        {
            public TravelingMerchantSaveData travelingMerchant;
        }

        [System.Serializable]
        public class TravelingMerchantSaveData
        {
            public bool isPresent;
            public int randomSeed;
            public string[] currentStockItemIds;
            public int[] currentStockQuantities;
            // DayFlag 기반 고정 스케줄이므로 nextVisitDay, departureDayOffset 불필요
            // -> see docs/content/npcs.md 섹션 6.2
        }
    }
```

- **MCP 호출**: 1회
- **Phase 1 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

### T-1 Phase 2: 이벤트 허브 스크립트 (S-11)

#### T-1-12: NPCEvents static class (S-11)

```
create_script
  path: "Assets/_Project/Scripts/NPC/NPCEvents.cs"
  content: |
    // S-11: NPC 정적 이벤트 허브
    // -> see docs/systems/npc-shop-architecture.md 섹션 4
    using System;
    using SeedMind.NPC.Data;

    namespace SeedMind.NPC
    {
        public static class NPCEvents
        {
            // --- 대화 ---
            public static event Action<string, DialogueData> OnDialogueStarted;
            public static event Action<string> OnDialogueEnded;

            // --- 상점 ---
            public static event Action<string> OnShopOpened;
            public static event Action<string> OnShopClosed;

            // --- NPC 상태 ---
            public static event Action<string, NPCActivityState> OnNPCStateChanged;

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

- **MCP 호출**: 1회
- **Phase 2 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

### T-1 Phase 3: 시스템 클래스 스크립트 (S-12 ~ S-15)

#### T-1-13: NPCManager (S-12)

```
create_script
  path: "Assets/_Project/Scripts/NPC/NPCManager.cs"
  content: |
    // S-12: NPC 레지스트리 관리 (MonoBehaviour Singleton)
    // -> see docs/systems/npc-shop-architecture.md 섹션 3.2
    using UnityEngine;
    using System.Collections.Generic;
    using SeedMind.NPC.Data;

    namespace SeedMind.NPC
    {
        public class NPCManager : MonoBehaviour
        {
            [SerializeField] private NPCData[] _npcRegistry;

            private Dictionary<string, NPCController> _activeNPCs
                = new Dictionary<string, NPCController>();
            private Dictionary<string, NPCActivityState> _npcStates
                = new Dictionary<string, NPCActivityState>();

            public IReadOnlyDictionary<string, NPCController> ActiveNPCs => _activeNPCs;

            public void Initialize() { /* NPC 등록, 초기 상태 설정 */ }
            public NPCController GetNPC(string npcId) => _activeNPCs.GetValueOrDefault(npcId);
            public bool IsNPCAvailable(string npcId)
                => _npcStates.TryGetValue(npcId, out var s) && s == NPCActivityState.Active;
            public void RefreshNPCStates(int currentHour, int currentDay) { /* 시간 기반 상태 갱신 */ }
            public NPCSaveData GetSaveData() { return new NPCSaveData(); }
            public void LoadSaveData(NPCSaveData data) { /* 세이브 복원 */ }
            // [구독] TimeManager.OnHourChanged -> RefreshNPCStates()
            // [구독] WeatherSystem.OnWeatherChanged -> HandleWeatherChange()
            // 전체 구현: -> see docs/systems/npc-shop-architecture.md 섹션 3.2
        }
    }
```

- **MCP 호출**: 1회

#### T-1-14: NPCController (S-13)

```
create_script
  path: "Assets/_Project/Scripts/NPC/NPCController.cs"
  content: |
    // S-13: 개별 NPC 행동 제어 (MonoBehaviour)
    // -> see docs/systems/npc-shop-architecture.md 섹션 3.3
    using UnityEngine;
    using SeedMind.NPC.Data;

    namespace SeedMind.NPC
    {
        public class NPCController : MonoBehaviour
        {
            [SerializeField] private NPCData _npcData;

            private NPCActivityState _currentState;
            private bool _isInteracting;

            public NPCData Data => _npcData;
            public NPCActivityState CurrentState => _currentState;

            public void Interact(/* PlayerController player */) { /* 대화 트리거 */ }
            public void SetState(NPCActivityState state)
            {
                _currentState = state;
                NPCEvents.RaiseNPCStateChanged(_npcData.npcId, state);
            }
            public bool IsAvailable() => _currentState == NPCActivityState.Active;
            private void StartDialogue() { /* DialogueSystem 호출 */ }
            private void HandleDialogueChoice(DialogueChoiceAction action) { /* 서비스 위임 */ }
            private void OpenNPCService() { /* NPC 유형별 서비스 분기 */ }
            // 전체 구현: -> see docs/systems/npc-shop-architecture.md 섹션 3.3, 5.1~5.2
        }
    }
```

- **MCP 호출**: 1회

#### T-1-15: DialogueSystem (S-14)

```
create_script
  path: "Assets/_Project/Scripts/NPC/DialogueSystem.cs"
  content: |
    // S-14: 대화 흐름 관리 (MonoBehaviour Singleton)
    // -> see docs/systems/npc-shop-architecture.md 섹션 3.4
    using System;
    using UnityEngine;
    using SeedMind.NPC.Data;

    namespace SeedMind.NPC
    {
        public class DialogueSystem : MonoBehaviour
        {
            private DialogueData _currentDialogue;
            private int _currentNodeIndex;
            private bool _isActive;
            private NPCController _currentNPC;

            public bool IsActive => _isActive;
            public DialogueNode CurrentNode =>
                (_currentDialogue != null && _currentNodeIndex < _currentDialogue.nodes.Length)
                ? _currentDialogue.nodes[_currentNodeIndex] : null;

            // 이벤트
            public event Action<DialogueData> OnDialogueStarted;
            public event Action<DialogueNode> OnDialogueNodeChanged;
            public event Action<DialogueChoiceAction> OnDialogueChoiceMade;
            public event Action OnDialogueEnded;

            public void StartDialogue(DialogueData data, NPCController npc)
            {
                _currentDialogue = data;
                _currentNPC = npc;
                _currentNodeIndex = 0;
                _isActive = true;
                OnDialogueStarted?.Invoke(data);
                NPCEvents.RaiseDialogueStarted(npc.Data.npcId, data);
                OnDialogueNodeChanged?.Invoke(CurrentNode);
            }
            public void AdvanceNode() { /* _currentNodeIndex++, 범위 체크 */ }
            public void SelectChoice(int choiceIndex) { /* 선택지 처리, 점프/액션 */ }
            public void EndDialogue()
            {
                _isActive = false;
                OnDialogueEnded?.Invoke();
                NPCEvents.RaiseDialogueEnded(_currentNPC.Data.npcId);
                _currentDialogue = null;
                _currentNPC = null;
            }
            private void ProcessChoiceAction(DialogueChoiceAction action)
            {
                OnDialogueChoiceMade?.Invoke(action);
            }
            // 전체 구현: -> see docs/systems/npc-shop-architecture.md 섹션 3.4
        }
    }
```

- **MCP 호출**: 1회

#### T-1-16: TravelingMerchantScheduler (S-15)

```
create_script
  path: "Assets/_Project/Scripts/NPC/TravelingMerchantScheduler.cs"
  content: |
    // S-15: 여행 상인 방문 일정 관리 (MonoBehaviour)
    // -> see docs/systems/npc-shop-architecture.md 섹션 3.5
    using UnityEngine;
    using System.Collections.Generic;
    using SeedMind.NPC.Data;

    namespace SeedMind.NPC
    {
        public class TravelingMerchantScheduler : MonoBehaviour
        {
            [SerializeField] private NPCData _merchantNPCData;
            [SerializeField] private TravelingShopPoolData _shopPool;
            [SerializeField] private Transform _spawnPosition;
            [SerializeField] private DayFlag _visitDays;  // -> see docs/content/npcs.md 섹션 6.2 (토/일 고정)

            private bool _isPresent;
            // private List<ShopItemEntry> _currentStock;   // ShopItemEntry -> see economy-architecture.md
            private int _randomSeed;

            /// <summary>
            /// 고정 요일 스케줄 기반 방문 판단.
            /// 난수 주기 방식이 아닌 DayFlag 비트마스크로 등장 요일을 결정한다.
            /// </summary>
            public void CheckVisitSchedule(int currentDay, int currentDayOfWeek)
            {
                // currentDayOfWeek: 0=Mon ~ 6=Sun
                // DayFlag 비트마스크로 해당 요일인지 확인
                // -> see docs/content/npcs.md 섹션 6.2
                /* 등장일이면 SpawnMerchant(), 비등장일이면 DespawnMerchant() */
            }
            public void GenerateStock(int playerLevel /*, Season season */) { /* 재고 생성 */ }
            public void SpawnMerchant()
            {
                _isPresent = true;
                NPCEvents.RaiseTravelingMerchantArrived();
            }
            public void DespawnMerchant()
            {
                _isPresent = false;
                NPCEvents.RaiseTravelingMerchantDeparted();
            }
            public TravelingMerchantSaveData GetSaveData() { return new TravelingMerchantSaveData(); }
            public void LoadSaveData(TravelingMerchantSaveData data) { /* 복원 */ }
            // [구독] TimeManager.OnDayChanged -> CheckVisitSchedule()
            // 등장 요일: -> see docs/content/npcs.md 섹션 6.2 (매주 토/일 고정)
            // 전체 구현: -> see docs/systems/npc-shop-architecture.md 섹션 3.5
        }
    }
```

- **MCP 호출**: 1회
- **Phase 3 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

### T-1 Phase 4: UI 스크립트 (S-16)

#### T-1-17: DialogueUI (S-16)

```
create_script
  path: "Assets/_Project/Scripts/UI/DialogueUI.cs"
  content: |
    // S-16: 대화창 UI 컨트롤러
    // -> see docs/systems/npc-shop-architecture.md 섹션 3.4, 6.5
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using SeedMind.NPC;
    using SeedMind.NPC.Data;

    namespace SeedMind.UI
    {
        public class DialogueUI : MonoBehaviour
        {
            [Header("UI 참조")]
            [SerializeField] private GameObject _dialoguePanel;
            [SerializeField] private Image _portraitImage;
            [SerializeField] private TextMeshProUGUI _speakerNameText;
            [SerializeField] private TextMeshProUGUI _dialogueText;
            [SerializeField] private Transform _choiceContainer;
            [SerializeField] private GameObject _choicePrefab;
            [SerializeField] private Button _advanceButton;

            private DialogueSystem _dialogueSystem;

            private void OnEnable()
            {
                // DialogueSystem 이벤트 구독
                // _dialogueSystem.OnDialogueStarted += HandleDialogueStarted;
                // _dialogueSystem.OnDialogueNodeChanged += HandleNodeChanged;
                // _dialogueSystem.OnDialogueEnded += HandleDialogueEnded;
            }
            private void OnDisable()
            {
                // 이벤트 구독 해제
            }
            private void HandleDialogueStarted(DialogueData data) { _dialoguePanel.SetActive(true); }
            private void HandleNodeChanged(DialogueNode node) { /* 텍스트/선택지 갱신 */ }
            private void HandleDialogueEnded() { _dialoguePanel.SetActive(false); }
            // 전체 구현: -> see docs/systems/npc-shop-architecture.md 섹션 3.4
        }
    }
```

- **MCP 호출**: 1회
- **Phase 4 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

**T-1 합계**: 스크립트 16회 + 폴더 2회 + 컴파일 대기 4회 = **17회** (컴파일 대기 포함 시 21회)

---

## 3. T-2: SO 에셋 생성

**목적**: NPC/상점 시스템에 필요한 모든 ScriptableObject 에셋을 생성하고 필드를 설정한다.

**전제**: T-1 모든 Phase 컴파일 완료. NPCData, DialogueData, TravelingShopPoolData 클래스가 Unity에서 인식 가능한 상태.

---

### T-2-01: 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/NPCs"

create_folder
  path: "Assets/_Project/Data/Dialogues"
```

- **MCP 호출**: 2회

---

### T-2-02: SO_NPC_GeneralMerchant (잡화 상인 하나)

```
create_scriptable_object
  type: "SeedMind.NPC.Data.NPCData"
  asset_path: "Assets/_Project/Data/NPCs/SO_NPC_GeneralMerchant.asset"

set_property  target: "SO_NPC_GeneralMerchant"
  npcId = "npc_hana"                              // -> see docs/content/npcs.md 섹션 2.1
  displayName = "하나"                             // -> see docs/content/npcs.md 섹션 3.1
  npcType = 0                                     // GeneralMerchant (int cast)
  defaultPosition = (0, 0, 0)                     // -> see docs/content/npcs.md 섹션 2.2
  openHour = 0                                    // -> see docs/systems/economy-system.md 섹션 3.2
  closeHour = 0                                   // -> see docs/systems/economy-system.md 섹션 3.2
  closedDays = 0                                  // -> see docs/systems/economy-system.md 섹션 3.2
  interactionRadius = 0                           // -> see docs/content/npcs.md

set_property  target: "SO_NPC_GeneralMerchant"
  greetingDialogue = ref:SO_Dlg_Greeting_Merchant // T-2-07에서 생성 후 연결
  closedDialogue = ref:SO_Dlg_Closed_Merchant     // T-2-10에서 생성 후 연결
  shopData = ref:SO_Shop_GeneralMerchant          // 기존 ShopData 에셋 참조 (economy-architecture)
```

> **주의**: `openHour`, `closeHour`, `closedDays`, `defaultPosition`, `interactionRadius`의 구체적 수치는 MCP 실행 시점에 canonical 문서에서 읽어 입력한다. 본 문서에서 수치를 직접 기재하지 않는다 (PATTERN-006).

- **MCP 호출**: 1(생성) + 8(필드 설정) + 3(참조 연결) = **12회**

[RISK] `greetingDialogue`, `closedDialogue` 참조는 T-2-07/T-2-10 에셋 생성 후에만 설정 가능. 순서 의존성 있음. 대안: 모든 SO 에셋을 먼저 생성(필드 비워둠)한 뒤 참조 연결을 별도 패스로 실행.

---

### T-2-03: SO_NPC_Blacksmith (대장간 철수)

```
create_scriptable_object
  type: "SeedMind.NPC.Data.NPCData"
  asset_path: "Assets/_Project/Data/NPCs/SO_NPC_Blacksmith.asset"

set_property  target: "SO_NPC_Blacksmith"
  npcId = "npc_cheolsu"                           // -> see docs/content/npcs.md 섹션 2.1
  displayName = "철수"                             // -> see docs/content/npcs.md 섹션 4.1
  npcType = 1                                     // Blacksmith
  defaultPosition = (0, 0, 0)                     // -> see docs/content/npcs.md 섹션 2.2
  openHour = 0                                    // -> see docs/systems/economy-system.md 섹션 3.2
  closeHour = 0                                   // -> see docs/systems/economy-system.md 섹션 3.2
  closedDays = 0                                  // -> see docs/systems/economy-system.md 섹션 3.2
  interactionRadius = 0                           // -> see docs/content/npcs.md

set_property  target: "SO_NPC_Blacksmith"
  greetingDialogue = ref:SO_Dlg_Greeting_Blacksmith
  closedDialogue = ref:SO_Dlg_Closed_Blacksmith
  shopData = ref:SO_Shop_Blacksmith               // 대장간 ShopData (재료 판매)
```

- **MCP 호출**: **12회**

---

### T-2-04: SO_NPC_Carpenter (목수 목이)

```
create_scriptable_object
  type: "SeedMind.NPC.Data.NPCData"
  asset_path: "Assets/_Project/Data/NPCs/SO_NPC_Carpenter.asset"

set_property  target: "SO_NPC_Carpenter"
  npcId = "npc_moki"                              // -> see docs/content/npcs.md 섹션 2.1
  displayName = "목이"                             // -> see docs/content/npcs.md 섹션 6.1
  npcType = 2                                     // Carpenter
  defaultPosition = (0, 0, 0)                     // -> see docs/content/npcs.md 섹션 2.2
  openHour = 0                                    // -> see docs/systems/economy-system.md 섹션 3.2
  closeHour = 0                                   // -> see docs/systems/economy-system.md 섹션 3.2
  closedDays = 0                                  // -> see docs/systems/economy-system.md 섹션 3.2
  interactionRadius = 0                           // -> see docs/content/npcs.md

set_property  target: "SO_NPC_Carpenter"
  greetingDialogue = ref:SO_Dlg_Greeting_Carpenter
  closedDialogue = ref:SO_Dlg_Closed_Carpenter
  shopData = null                                 // 목수는 상점 없음, BuildingManager로 위임
```

- **MCP 호출**: **12회**

---

### T-2-05: SO_NPC_TravelingMerchant (여행 상인 바람이)

```
create_scriptable_object
  type: "SeedMind.NPC.Data.NPCData"
  asset_path: "Assets/_Project/Data/NPCs/SO_NPC_TravelingMerchant.asset"

set_property  target: "SO_NPC_TravelingMerchant"
  npcId = "npc_barami"                            // -> see docs/content/npcs.md 섹션 2.1
  displayName = "바람이"                           // -> see docs/content/npcs.md 섹션 5.1
  npcType = 3                                     // TravelingMerchant
  defaultPosition = (0, 0, 0)                     // -> see docs/content/npcs.md 섹션 2.2
  openHour = 0                                    // 방문 시 항시 영업 (-> see docs/content/npcs.md)
  closeHour = 0                                   // -> see docs/content/npcs.md
  closedDays = 0                                  // DayFlag.None (방문 시 무휴)
  interactionRadius = 0                           // -> see docs/content/npcs.md

set_property  target: "SO_NPC_TravelingMerchant"
  greetingDialogue = null                         // 여행 상인 대화 SO는 방문 시 동적 설정
  closedDialogue = null
  shopData = null                                 // TravelingMerchantScheduler가 동적 재고 관리
```

- **MCP 호출**: **12회**

---

### T-2-06: SO_TravelingPool_Default (여행 상인 아이템 풀)

```
create_scriptable_object
  type: "SeedMind.NPC.Data.TravelingShopPoolData"
  asset_path: "Assets/_Project/Data/NPCs/SO_TravelingPool_Default.asset"

set_property  target: "SO_TravelingPool_Default"
  poolId = "traveling_pool_default"
  minItemCount = 0                                // -> see docs/content/npcs.md 섹션 5.3
  maxItemCount = 0                                // -> see docs/content/npcs.md 섹션 5.3
  candidates = []                                 // 후보 아이템 목록은 MCP 실행 시점에
                                                  // canonical(docs/content/npcs.md 섹션 5.4)에서 설정
```

> **주의**: `candidates` 배열은 중첩 직렬화 데이터이며 MCP set_property로 설정이 어려울 수 있다 (PATTERN-006). Editor 스크립트 우회를 권장한다.

- **MCP 호출**: 1(생성) + 3(필드 설정) = **4회**

[RISK] TravelingShopCandidate 배열은 SO 참조(itemReference)를 포함하는 중첩 구조. MCP로 배열 요소별 SO 참조를 설정하는 것이 불가능할 수 있다. Editor 스크립트 우회 필요.

---

### T-2-07 ~ T-2-09: 인사 대화 DialogueData SO (3종)

각 NPC의 인사 대화 SO를 생성한다. 대사 텍스트는 canonical 문서에서 읽어 설정한다.

#### T-2-07: SO_Dlg_Greeting_Merchant

```
create_scriptable_object
  type: "SeedMind.NPC.Data.DialogueData"
  asset_path: "Assets/_Project/Data/Dialogues/SO_Dlg_Greeting_Merchant.asset"

set_property  target: "SO_Dlg_Greeting_Merchant"
  dialogueId = "greeting_merchant"
  // nodes 배열 설정:
  // nodes[0].speakerName = "하나"                  // -> see docs/content/npcs.md 섹션 3.3
  // nodes[0].text = (인사 대사)                    // -> see docs/content/npcs.md 섹션 3.3
  // nodes[0].choices[0] = { "물건 구매", OpenShop, -1 }
  // nodes[0].choices[1] = { "물건 판매", OpenShop, -1 }
  // nodes[0].choices[2] = { "대화 종료", CloseDialogue, -1 }
```

> **주의**: DialogueNode[] 내 DialogueChoice[] 중첩 배열 설정은 MCP set_property의 제약을 받을 수 있다. 아래 [RISK] 참조.

- **MCP 호출**: 1(생성) + 1(dialogueId) + ~3(nodes 배열) = **~5회**

#### T-2-08: SO_Dlg_Greeting_Blacksmith

```
create_scriptable_object
  type: "SeedMind.NPC.Data.DialogueData"
  asset_path: "Assets/_Project/Data/Dialogues/SO_Dlg_Greeting_Blacksmith.asset"

set_property  target: "SO_Dlg_Greeting_Blacksmith"
  dialogueId = "greeting_blacksmith"
  // nodes[0].speakerName = "철수"                  // -> see docs/content/npcs.md 섹션 4.3
  // nodes[0].text = (인사 대사)                    // -> see docs/content/npcs.md 섹션 4.3
  // nodes[0].choices[0] = { "도구 업그레이드", OpenUpgrade, -1 }
  // nodes[0].choices[1] = { "재료 구매", OpenShop, -1 }
  // nodes[0].choices[2] = { "대화 종료", CloseDialogue, -1 }
```

- **MCP 호출**: **~5회**

#### T-2-09: SO_Dlg_Greeting_Carpenter

```
create_scriptable_object
  type: "SeedMind.NPC.Data.DialogueData"
  asset_path: "Assets/_Project/Data/Dialogues/SO_Dlg_Greeting_Carpenter.asset"

set_property  target: "SO_Dlg_Greeting_Carpenter"
  dialogueId = "greeting_carpenter"
  // nodes[0].speakerName = "목이"                  // -> see docs/content/npcs.md 섹션 6.3
  // nodes[0].text = (인사 대사)                    // -> see docs/content/npcs.md 섹션 6.3
  // nodes[0].choices[0] = { "시설 건설", OpenBuild, -1 }
  // nodes[0].choices[1] = { "대화 종료", CloseDialogue, -1 }
```

- **MCP 호출**: **~5회**

---

### T-2-10 ~ T-2-12: 휴무/영업외 대화 DialogueData SO (3종)

#### T-2-10: SO_Dlg_Closed_Merchant

```
create_scriptable_object
  type: "SeedMind.NPC.Data.DialogueData"
  asset_path: "Assets/_Project/Data/Dialogues/SO_Dlg_Closed_Merchant.asset"

set_property  target: "SO_Dlg_Closed_Merchant"
  dialogueId = "closed_merchant"
  // nodes[0].speakerName = "하나"
  // nodes[0].text = (휴무 대사)                    // -> see docs/content/npcs.md 섹션 3.3
  // nodes[0].choices = []                         // 선택지 없음, 자동 종료
```

- **MCP 호출**: **~3회**

#### T-2-11: SO_Dlg_Closed_Blacksmith

```
create_scriptable_object
  type: "SeedMind.NPC.Data.DialogueData"
  asset_path: "Assets/_Project/Data/Dialogues/SO_Dlg_Closed_Blacksmith.asset"

set_property  target: "SO_Dlg_Closed_Blacksmith"
  dialogueId = "closed_blacksmith"
  // nodes[0].speakerName = "철수"
  // nodes[0].text = (휴무 대사)                    // -> see docs/content/npcs.md 섹션 4.3
  // nodes[0].choices = []
```

- **MCP 호출**: **~3회**

#### T-2-12: SO_Dlg_Closed_Carpenter

```
create_scriptable_object
  type: "SeedMind.NPC.Data.DialogueData"
  asset_path: "Assets/_Project/Data/Dialogues/SO_Dlg_Closed_Carpenter.asset"

set_property  target: "SO_Dlg_Closed_Carpenter"
  dialogueId = "closed_carpenter"
  // nodes[0].speakerName = "목이"
  // nodes[0].text = (휴무 대사)                    // -> see docs/content/npcs.md 섹션 6.3
  // nodes[0].choices = []
```

- **MCP 호출**: **~3회**

---

### T-2 참조 연결 패스

모든 SO 에셋 생성 완료 후, NPCData의 `greetingDialogue` / `closedDialogue` 필드에 DialogueData SO 참조를 연결한다.

```
set_property  target: "SO_NPC_GeneralMerchant"
  greetingDialogue = ref:"Assets/_Project/Data/Dialogues/SO_Dlg_Greeting_Merchant.asset"
  closedDialogue = ref:"Assets/_Project/Data/Dialogues/SO_Dlg_Closed_Merchant.asset"

set_property  target: "SO_NPC_Blacksmith"
  greetingDialogue = ref:"Assets/_Project/Data/Dialogues/SO_Dlg_Greeting_Blacksmith.asset"
  closedDialogue = ref:"Assets/_Project/Data/Dialogues/SO_Dlg_Closed_Blacksmith.asset"

set_property  target: "SO_NPC_Carpenter"
  greetingDialogue = ref:"Assets/_Project/Data/Dialogues/SO_Dlg_Greeting_Carpenter.asset"
  closedDialogue = ref:"Assets/_Project/Data/Dialogues/SO_Dlg_Closed_Carpenter.asset"
```

- **MCP 호출**: 6회

---

**T-2 합계**: 폴더 2회 + NPCData 4종 x 12회 + TravelingPool 4회 + DialogueData(인사) 3종 x 5회 + DialogueData(휴무) 3종 x 3회 + 참조 연결 6회 = 2 + 48 + 4 + 15 + 9 + 6 = **~67회** (배열 설정 난이도에 따라 변동)

[RISK] DialogueData의 nodes 배열(DialogueNode[] 내 DialogueChoice[])은 중첩 직렬화 구조이다. MCP `set_property`가 중첩 배열의 개별 요소를 지원하지 않을 수 있다. 이 경우 Editor 스크립트(`CreateDialogueAssets.cs`)로 일괄 생성하여 T-2-07~T-2-12의 ~24회를 ~3회로 감소시킬 수 있다. (-> see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)

---

## 4. T-3: UI 프리팹/씬 오브젝트 생성

**목적**: DialoguePanel UI 프리팹을 생성한다.

**전제**: T-1 Phase 4 (DialogueUI.cs) 컴파일 완료. Canvas_Overlay 존재 (ARC-002).

---

### T-3-01: DialoguePanel 루트 생성

```
create_object
  name: "DialoguePanel"
  parent: "Canvas_Overlay"

add_component
  target: "DialoguePanel"
  component: "RectTransform"
  // 이미 UI 오브젝트이므로 자동 부착

set_property  target: "DialoguePanel/RectTransform"
  anchorMin = (0, 0)
  anchorMax = (1, 0.35)                           // 화면 하단 35%
  offsetMin = (20, 20)
  offsetMax = (-20, 0)
```

- **MCP 호출**: 3회

### T-3-02: DialoguePanel 배경 이미지

```
create_object
  name: "BG_Dialogue"
  parent: "DialoguePanel"

add_component
  target: "BG_Dialogue"
  component: "UnityEngine.UI.Image"

set_property  target: "BG_Dialogue/Image"
  color = (0.1, 0.1, 0.15, 0.9)                  // 어두운 반투명 배경
  raycastTarget = true

set_property  target: "BG_Dialogue/RectTransform"
  anchorMin = (0, 0)
  anchorMax = (1, 1)
```

- **MCP 호출**: 4회

### T-3-03: 초상화 영역

```
create_object
  name: "PortraitImage"
  parent: "DialoguePanel"

add_component
  target: "PortraitImage"
  component: "UnityEngine.UI.Image"

set_property  target: "PortraitImage/RectTransform"
  anchorMin = (0, 0.1)
  anchorMax = (0.2, 0.9)
  offsetMin = (10, 0)
  offsetMax = (0, 0)
```

- **MCP 호출**: 3회

### T-3-04: 화자 이름 텍스트

```
create_object
  name: "SpeakerNameText"
  parent: "DialoguePanel"

add_component
  target: "SpeakerNameText"
  component: "TMPro.TextMeshProUGUI"

set_property  target: "SpeakerNameText/TextMeshProUGUI"
  fontSize = 20
  fontStyle = "Bold"
  color = (1, 0.9, 0.6, 1)                       // 골드색 이름

set_property  target: "SpeakerNameText/RectTransform"
  anchorMin = (0.22, 0.75)
  anchorMax = (0.7, 0.95)
```

- **MCP 호출**: 4회

### T-3-05: 대사 텍스트

```
create_object
  name: "DialogueText"
  parent: "DialoguePanel"

add_component
  target: "DialogueText"
  component: "TMPro.TextMeshProUGUI"

set_property  target: "DialogueText/TextMeshProUGUI"
  fontSize = 16
  color = (1, 1, 1, 1)

set_property  target: "DialogueText/RectTransform"
  anchorMin = (0.22, 0.1)
  anchorMax = (0.98, 0.72)
```

- **MCP 호출**: 4회

### T-3-06: 선택지 컨테이너

```
create_object
  name: "ChoiceContainer"
  parent: "DialoguePanel"

add_component
  target: "ChoiceContainer"
  component: "UnityEngine.UI.VerticalLayoutGroup"

set_property  target: "ChoiceContainer/VerticalLayoutGroup"
  spacing = 8
  childAlignment = "UpperCenter"
  childForceExpandWidth = true
  childForceExpandHeight = false

set_property  target: "ChoiceContainer/RectTransform"
  anchorMin = (0.22, 0.1)
  anchorMax = (0.98, 0.72)
```

> 선택지 컨테이너는 DialogueText와 동일 영역에 배치하며, 대사 표시와 선택지 표시가 상호 배타적으로 활성화된다 (DialogueUI에서 제어).

- **MCP 호출**: 4회

### T-3-07: DialogueUI 컴포넌트 부착 및 참조 연결

```
add_component
  target: "DialoguePanel"
  component: "SeedMind.UI.DialogueUI"

set_property  target: "DialoguePanel/DialogueUI"
  _dialoguePanel = ref:DialoguePanel
  _portraitImage = ref:PortraitImage/Image
  _speakerNameText = ref:SpeakerNameText/TextMeshProUGUI
  _dialogueText = ref:DialogueText/TextMeshProUGUI
  _choiceContainer = ref:ChoiceContainer/Transform
```

- **MCP 호출**: 2회

### T-3-08: 프리팹 저장 (선택사항)

```
save_as_prefab
  target: "DialoguePanel"
  path: "Assets/_Project/Prefabs/UI/PFB_UI_DialoguePanel.prefab"
```

- **MCP 호출**: 1회

[RISK] `save_as_prefab` MCP 도구 가용 여부 사전 검증 필요. 미지원 시 씬에 직접 배치된 상태로 유지한다.

---

**T-3 합계**: 3 + 4 + 3 + 4 + 4 + 4 + 2 + 1 = **~24회** (UI 배치 미세조정 추가 시 변동)

---

## 5. T-4: 씬 배치 및 참조 연결

**목적**: SCN_Farm 씬에 NPC 오브젝트와 매니저를 배치하고, SO 참조를 연결한다.

**전제**: T-1~T-3 완료. SCN_Farm 씬에 `--- MANAGERS ---`, `Canvas_Overlay` 존재.

---

### T-4-01: NPC 구분 오브젝트 생성

```
create_object
  name: "--- NPCs ---"

set_parent
  target: "--- NPCs ---"
  parent: (SCN_Farm root level)
```

- **MCP 호출**: 2회

### T-4-02: NPC_GeneralMerchant 배치

```
create_object
  name: "NPC_GeneralMerchant"

set_parent
  target: "NPC_GeneralMerchant"
  parent: "--- NPCs ---"

add_component
  target: "NPC_GeneralMerchant"
  component: "SeedMind.NPC.NPCController"

set_property  target: "NPC_GeneralMerchant/NPCController"
  _npcData = ref:"Assets/_Project/Data/NPCs/SO_NPC_GeneralMerchant.asset"

set_property  target: "NPC_GeneralMerchant/Transform"
  position = (0, 0, 0)                           // -> see docs/content/npcs.md 섹션 2.2
```

- **MCP 호출**: 5회

### T-4-03: NPC_Blacksmith 배치

```
create_object
  name: "NPC_Blacksmith"

set_parent
  target: "NPC_Blacksmith"
  parent: "--- NPCs ---"

add_component
  target: "NPC_Blacksmith"
  component: "SeedMind.NPC.NPCController"

set_property  target: "NPC_Blacksmith/NPCController"
  _npcData = ref:"Assets/_Project/Data/NPCs/SO_NPC_Blacksmith.asset"

set_property  target: "NPC_Blacksmith/Transform"
  position = (0, 0, 0)                           // -> see docs/content/npcs.md 섹션 2.2
```

- **MCP 호출**: 5회

### T-4-04: NPC_Carpenter 배치

```
create_object
  name: "NPC_Carpenter"

set_parent
  target: "NPC_Carpenter"
  parent: "--- NPCs ---"

add_component
  target: "NPC_Carpenter"
  component: "SeedMind.NPC.NPCController"

set_property  target: "NPC_Carpenter/NPCController"
  _npcData = ref:"Assets/_Project/Data/NPCs/SO_NPC_Carpenter.asset"

set_property  target: "NPC_Carpenter/Transform"
  position = (0, 0, 0)                           // -> see docs/content/npcs.md 섹션 2.2
```

- **MCP 호출**: 5회

### T-4-05: NPCManager 매니저 배치

```
create_object
  name: "NPCManager"

set_parent
  target: "NPCManager"
  parent: "--- MANAGERS ---"

add_component
  target: "NPCManager"
  component: "SeedMind.NPC.NPCManager"

set_property  target: "NPCManager/NPCManager"
  _npcRegistry = [
    ref:"Assets/_Project/Data/NPCs/SO_NPC_GeneralMerchant.asset",
    ref:"Assets/_Project/Data/NPCs/SO_NPC_Blacksmith.asset",
    ref:"Assets/_Project/Data/NPCs/SO_NPC_Carpenter.asset",
    ref:"Assets/_Project/Data/NPCs/SO_NPC_TravelingMerchant.asset"
  ]
```

- **MCP 호출**: 4회

### T-4-06: DialogueSystem 매니저 배치

```
create_object
  name: "DialogueSystem"

set_parent
  target: "DialogueSystem"
  parent: "--- MANAGERS ---"

add_component
  target: "DialogueSystem"
  component: "SeedMind.NPC.DialogueSystem"
```

- **MCP 호출**: 3회

### T-4-07: TravelingMerchantScheduler 매니저 배치

```
create_object
  name: "TravelingMerchantScheduler"

set_parent
  target: "TravelingMerchantScheduler"
  parent: "--- MANAGERS ---"

add_component
  target: "TravelingMerchantScheduler"
  component: "SeedMind.NPC.TravelingMerchantScheduler"

set_property  target: "TravelingMerchantScheduler/TravelingMerchantScheduler"
  _merchantNPCData = ref:"Assets/_Project/Data/NPCs/SO_NPC_TravelingMerchant.asset"
  _shopPool = ref:"Assets/_Project/Data/NPCs/SO_TravelingPool_Default.asset"
```

- **MCP 호출**: 4회

### T-4-08: NPC_TravelingMerchant 프리팹 등록 (G-09)

> `NPC_TravelingMerchant`는 씬에 직접 배치하지 않는다. 프리팹으로만 등록하고, `TravelingMerchantScheduler`가 런타임에 인스턴스화/파괴한다.

```
create_object
  name: "NPC_TravelingMerchant"

add_component
  target: "NPC_TravelingMerchant"
  component: "SeedMind.NPC.NPCController"

set_property  target: "NPC_TravelingMerchant/NPCController"
  _npcData = ref:"Assets/_Project/Data/NPCs/SO_NPC_TravelingMerchant.asset"

save_as_prefab
  target: "NPC_TravelingMerchant"
  path: "Assets/_Project/Prefabs/NPCs/NPC_TravelingMerchant.prefab"

// 프리팹 저장 후 씬에서 인스턴스 삭제 (런타임 동적 생성이므로)
// delete_object  target: "NPC_TravelingMerchant"
// [RISK] MCP에 delete_object가 없을 수 있음. 수동 삭제 또는 SetActive(false) 대체.

set_property  target: "TravelingMerchantScheduler/TravelingMerchantScheduler"
  _merchantPrefab = ref:"Assets/_Project/Prefabs/NPCs/NPC_TravelingMerchant.prefab"
```

- **MCP 호출**: 5회

### T-4-09: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

---

**T-4 합계**: 2 + 5 + 5 + 5 + 4 + 3 + 4 + 5 + 1 = **~34회** (NPC 위치는 canonical에서 읽어 설정)

---

## 6. T-5: 기존 시스템 연동 설정

**목적**: 기존 ShopSystem의 인터페이스를 확장하고, NPCController(Blacksmith)에서 ToolUpgradeSystem 연동 경로를 설정한다.

**전제**: T-4 완료. ShopSystem, ToolUpgradeSystem 이미 존재.

---

### T-5-01: ShopSystem.Open(ShopData) 파라미터 확장

기존 ShopSystem은 단일 ShopData를 참조하는 구조이다 (-> see `docs/systems/economy-architecture.md` 섹션 4.3). NPC별로 다른 ShopData를 동적으로 교체하려면 `Open(ShopData data)` 메서드를 추가해야 한다.

```
// ShopSystem.cs 수정 (기존 스크립트에 메서드 추가)
// MCP에서는 create_script로 전체 재생성하거나, 수동 편집 필요

// 추가 메서드:
// public void Open(ShopData data)
// {
//     _currentShopData = data;
//     // 기존 Open() 로직 실행
//     NPCEvents.RaiseShopOpened(data.shopId);
// }
```

[RISK] MCP는 기존 스크립트의 부분 편집을 지원하지 않는다. `create_script`로 ShopSystem.cs 전체를 재생성해야 한다. 기존 ShopSystem 로직을 유지하면서 `Open(ShopData data)` 오버로드를 추가하는 방식으로 처리한다.

```
create_script
  path: "Assets/_Project/Scripts/Economy/ShopSystem.cs"
  content: |
    // ShopSystem 확장 (NPC별 ShopData 동적 교체 지원)
    // 기존 구현: -> see docs/systems/economy-architecture.md 섹션 4.3
    // NPC 연동 확장: -> see docs/systems/npc-shop-architecture.md 섹션 5.1
    // ... (기존 코드 + Open(ShopData data) 오버로드 추가)
```

- **MCP 호출**: 1회

### T-5-02: NPCController -> ToolUpgradeSystem 연동 확인

NPCController(Blacksmith)의 `HandleDialogueChoice(OpenUpgrade)` 경로에서 ToolUpgradeSystem을 호출하도록 확인한다. 이는 T-1-14(NPCController.cs) 작성 시 이미 구조적으로 포함되어 있다.

추가 작업: ToolUpgradeSystem 참조를 NPCController의 Inspector에서 설정한다.

> **BlacksmithPanel(UpgradePanel)은 ARC-015(tool-upgrade-tasks.md)에서 이미 생성됨.** NPCController.Blacksmith에서 이를 참조 연결하는 작업만 이 태스크에서 수행한다. BlacksmithPanel을 중복 생성하지 않는다.

```
set_property  target: "NPC_Blacksmith/NPCController"
  // _upgradeSystem 참조는 런타임 FindObjectOfType으로 자동 탐색
  // 또는 Inspector에서 직접 연결
  // BlacksmithPanel 참조: ARC-015에서 생성된 오브젝트를 연결
```

- **MCP 호출**: 1회

### T-5-03: NPCController -> BuildingManager 연동 확인

목수 NPC의 `HandleDialogueChoice(OpenBuild)` 경로에서 BuildingManager를 호출하도록 확인한다.

```
set_property  target: "NPC_Carpenter/NPCController"
  // _buildingManager 참조는 런타임 FindObjectOfType으로 자동 탐색
```

- **MCP 호출**: 1회

### T-5-04: DialogueUI -> DialogueSystem 참조 연결

```
set_property  target: "DialoguePanel/DialogueUI"
  _dialogueSystem = ref:"DialogueSystem"
```

- **MCP 호출**: 1회

### T-5-05: Assembly Definition 생성

```
create_script
  path: "Assets/_Project/Scripts/NPC/SeedMind.NPC.asmdef"
  content: |
    {
        "name": "SeedMind.NPC",
        "rootNamespace": "SeedMind.NPC",
        "references": [
            "SeedMind.Core",
            "SeedMind.Farm",
            "SeedMind.Player",
            "SeedMind.Economy",
            "SeedMind.Building"
        ],
        "includePlatforms": [],
        "excludePlatforms": [],
        "autoReferenced": true
    }
```

- **MCP 호출**: 1회

### T-5-06: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

---

**T-5 합계**: 1 + 1 + 1 + 1 + 1 + 1 = **6회**

---

## 7. T-6: 통합 테스트 시퀀스

**목적**: NPC 대화, 상점 구매, 도구 업그레이드, 여행 상인 방문 전체 흐름을 Play Mode에서 검증한다.

**전제**: T-1~T-5 모든 태스크 완료. 컴파일 에러 없음.

---

### T-6-01: 테스트 A -- NPC 상태 갱신 확인

```
enter_play_mode

execute_method
  class: "SeedMind.NPC.NPCManager"
  method: "RefreshNPCStates"
  args: [8, 1]                                    // 오전 8시, 1일차

get_console_logs
  // 기대: "NPC npc_hana state changed to Active"
  // 기대: "NPC npc_cheolsu state changed to Active"
  // 기대: "NPC npc_moki state changed to Active"

exit_play_mode
```

- **MCP 호출**: 4회

### T-6-02: 테스트 B -- NPC 대화 흐름

```
enter_play_mode

execute_method
  class: "SeedMind.NPC.NPCController"
  target: "NPC_GeneralMerchant"
  method: "Interact"

get_console_logs
  // 기대: "Dialogue started: greeting_merchant with npc_hana"
  // 기대: DialogueUI 활성화

execute_method
  class: "SeedMind.NPC.DialogueSystem"
  method: "SelectChoice"
  args: [2]                                       // "대화 종료" 선택

get_console_logs
  // 기대: "Dialogue ended: npc_hana"

exit_play_mode
```

- **MCP 호출**: 5회

### T-6-03: 테스트 C -- 대장간 업그레이드 경로

```
enter_play_mode

execute_method
  class: "SeedMind.NPC.NPCController"
  target: "NPC_Blacksmith"
  method: "Interact"

get_console_logs
  // 기대: "Dialogue started: greeting_blacksmith with npc_cheolsu"

execute_method
  class: "SeedMind.NPC.DialogueSystem"
  method: "SelectChoice"
  args: [0]                                       // "도구 업그레이드" 선택

get_console_logs
  // 기대: "DialogueChoiceAction: OpenUpgrade"
  // 기대: ToolUpgradeSystem 호출 로그

exit_play_mode
```

- **MCP 호출**: 5회

### T-6-04: 테스트 D -- 여행 상인 방문 사이클

```
enter_play_mode

execute_method
  class: "SeedMind.NPC.TravelingMerchantScheduler"
  method: "SpawnMerchant"

get_console_logs
  // 기대: "TravelingMerchant arrived"
  // 기대: NPCEvents.OnTravelingMerchantArrived 발행

execute_method
  class: "SeedMind.NPC.TravelingMerchantScheduler"
  method: "DespawnMerchant"

get_console_logs
  // 기대: "TravelingMerchant departed"
  // 기대: NPCEvents.OnTravelingMerchantDeparted 발행

exit_play_mode
```

- **MCP 호출**: 5회

### T-6-05: 최종 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

---

**T-6 합계**: 4 + 5 + 5 + 5 + 1 = **~18회** (추가 엣지 케이스 테스트 시 확장)

[RISK] `execute_method`로 MonoBehaviour 인스턴스 메서드를 호출할 때, 대상 오브젝트를 이름으로 특정하는 방식이 MCP에서 지원되는지 사전 검증 필요. 미지원 시 테스트용 Editor 스크립트를 통한 우회 필요.

---

## Cross-references

| 문서 | 참조 내용 |
|------|-----------|
| `docs/systems/npc-shop-architecture.md` (ARC-008) | 본 태스크의 기반 아키텍처 (클래스 설계, 데이터 구조, 이벤트, 씬 계층) |
| `docs/content/npcs.md` (CON-003) | NPC 이름, 성격, 대사, 영업시간, 여행 상인 수치 canonical |
| `docs/systems/economy-architecture.md` 섹션 4.3 | ShopData SO, ShopSystem 클래스 (확장 대상) |
| `docs/systems/economy-system.md` 섹션 3 | 상점 종류, 영업시간, 휴무일, 인벤토리 규칙 |
| `docs/systems/tool-upgrade-architecture.md` | ToolUpgradeSystem (대장간 연동 대상) |
| `docs/mcp/tool-upgrade-tasks.md` (ARC-015) | ToolUpgradeSystem MCP 태스크 (선행 의존) |
| `docs/systems/time-season.md` 섹션 1.7 | 시간대 정의, 요일 매핑 |
| `docs/systems/project-structure.md` | 프로젝트 구조, 네임스페이스, asmdef, 씬 계층 |
| `docs/pipeline/data-pipeline.md` | SO 데이터 구조, 세이브/로드 구조 |
| `docs/mcp/scene-setup-tasks.md` (ARC-002) | 기본 씬 계층, Canvas_Overlay (선행 의존) |
| `docs/mcp/farming-tasks.md` (ARC-003) | 기본 시스템 인프라 (선행 의존) |
| `docs/mcp/facilities-tasks.md` (ARC-007) | BuildingManager (목수 연동 대상) |

---

## Open Questions

1. [OPEN] **NPC 호감도 시스템**: 현재 NPC는 상점/서비스 제공자로만 기능한다. 호감도 시스템(선물, 대화 빈도에 따른 호감 상승 -> 할인/특수 아이템 해금) 도입 시 NPCSaveData에 호감도 필드 추가 필요. 현재 설계에서 확장 여지를 남겨두었다. (-> see `npc-shop-architecture.md` Open Questions #1)

2. [OPEN] **대화 조건 분기**: 현재 선형 + 선택지 구조로 설계했으나, 계절별/호감도별 대사 변경이 필요하면 DialogueNode에 조건 필드를 추가해야 한다. T-1-07(DialogueNode.cs)에 `conditionField`를 추가하는 것은 후속 확장으로 남긴다. (-> see `npc-shop-architecture.md` Open Questions #2)

3. [OPEN] **목수 NPC의 BuildingManager 연동 상세**: `HandleDialogueChoice(OpenBuild)` 경로에서 BuildingManager의 어떤 메서드를 호출할지(건설 UI 열기? 건설 가능 목록 조회?) 구체적 흐름이 미정이다. (-> see `npc-shop-architecture.md` Open Questions #4)

4. [OPEN] **DialogueUI 필드 연결 누락**: `_choicePrefab` 필드에 할당할 선택지 버튼 프리팹이 T-3에서 정의되지 않았다. 선택지 버튼 프리팹(ChoiceButton)을 별도 T-3 서브태스크로 추가하거나, DialogueUI 내부에서 코드로 동적 생성할지 결정 필요. 또한 `_advanceButton` 참조도 T-3-07 set_property에서 누락됨. AdvanceButton 오브젝트 생성 후 DialogueUI._advanceButton 필드에 연결하는 태스크가 T-3에 추가 필요.

---

## Risks

1. [RISK] **MCP에서 DialogueData SO 중첩 배열 설정 난이도**: DialogueData는 `DialogueNode[]` 내에 `DialogueChoice[]`를 포함하는 2단 중첩 배열이다. MCP `set_property`로 이 구조를 자동 생성할 때 직렬화된 배열의 중첩 참조가 제대로 설정되지 않을 수 있다. **완화**: Editor 스크립트(`CreateDialogueAssets.cs`)를 통한 일괄 생성으로 우회. (-> see `npc-shop-architecture.md` Risks #2)

2. [RISK] **NPC 모듈의 넓은 의존성**: NPC 모듈이 Core, Farm, Player, Economy, Building을 모두 참조한다. asmdef 순환 참조는 없으나(NPC를 참조하는 모듈은 UI뿐), 변경 영향 범위가 넓어 NPC 모듈 수정 시 다른 모듈 테스트가 필요할 수 있다. (-> see `npc-shop-architecture.md` Risks #1)

3. [RISK] **기존 ShopSystem 인터페이스 수정 충돌**: T-5-01에서 ShopSystem.cs를 `create_script`로 전체 재생성해야 한다. 기존 ShopSystem 로직을 정확히 유지하면서 `Open(ShopData)` 오버로드를 추가해야 하므로, 기존 economy-architecture.md의 구현과 완전히 동기화된 상태에서 작업해야 한다. (-> see `npc-shop-architecture.md` Risks #3)

4. [RISK] **여행 상인 재고 재현성**: 세이브/로드 시 randomSeed 기반으로 동일 재고를 재생성해야 한다. `System.Random` 독립 인스턴스를 사용하여 다른 시스템의 난수와 충돌하지 않도록 한다. (-> see `npc-shop-architecture.md` Risks #4)

5. [RISK] **총 ~166회 MCP 호출**: 호출 수가 많아 실행 시간이 길어질 수 있다. Editor 스크립트 우회를 적용하면 T-2의 SO 에셋 생성을 대폭 줄일 수 있어 총 ~100회 이하로 감소 가능. 우선 MCP 단독 실행을 시도하고, SO 배열 설정 실패 시 Editor 스크립트로 전환한다.

---

*이 문서는 NPC/상점 시스템의 MCP 태스크 시퀀스 문서이다. 기반 아키텍처는 `docs/systems/npc-shop-architecture.md`(ARC-008)가 정본이며, NPC 콘텐츠 수치는 `docs/content/npcs.md`(CON-003)가 canonical이다.*
