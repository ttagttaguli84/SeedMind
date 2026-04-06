# 대장간 NPC 기술 아키텍처

> BlacksmithNPC 상호작용, 대화 흐름 제어, ToolUpgradeUI 레이아웃, BlacksmithNPCData SO 스키마, 친밀도 연동, 이벤트 설계, MCP 구현 태스크 요약  
> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: ARC-020

---

## Context

이 문서는 SeedMind의 **대장간 NPC(철수, Blacksmith)** 시스템에 대한 기술 아키텍처를 정의한다. 대장간 NPC는 플레이어의 도구 업그레이드 서비스를 제공하는 핵심 NPC로, 기존 NPC/상점 아키텍처(`npc-shop-architecture.md`)의 NPCController/DialogueSystem 구조 위에 대장간 고유의 대화 흐름, 업그레이드 UI, 친밀도 시스템을 결합한다.

**설계 목표**:
- 기존 NPCController/DialogueSystem을 확장하여 대장간 고유의 상호작용 흐름을 구현 (별도 서브클래스 불필요, 데이터 드리븐)
- ToolUpgradeUI를 ScreenBase 파생 클래스로 구현하여 UIManager Screen FSM에 통합
- BlacksmithNPCData SO로 대장간 NPC 고유 데이터를 분리하여 콘텐츠 확장 용이성 확보
- 친밀도(Affinity) 시스템을 NPC 레벨에서 설계하여, 대장간 외 다른 NPC에도 재사용 가능한 구조 제공
- 이벤트 기반 연동으로 ProgressionManager, AchievementManager와 느슨하게 결합

**본 문서가 canonical인 데이터**:
- BlacksmithNPC 클래스 설계, 대장간 상호작용 State Machine
- ToolUpgradeUI (ScreenBase) 계층 구조, 레이아웃
- BlacksmithNPCData SO 스키마 (JSON + C#)
- NPC 친밀도(Affinity) 시스템 아키텍처
- BlacksmithEvents 이벤트 허브 설계

**본 문서가 canonical이 아닌 데이터 (참조만)**:

| 데이터 종류 | 참조처 |
|------------|--------|
| NPC 기본 구조 (NPCData, NPCController, DialogueSystem) | `docs/systems/npc-shop-architecture.md` (ARC-008) |
| ToolUpgradeSystem, ToolData SO, ToolEffectResolver | `docs/systems/tool-upgrade-architecture.md` (DES-007) |
| UIManager, ScreenBase, PopupQueue | `docs/systems/ui-architecture.md` (ARC-018) |
| ScreenType enum 정의 | `docs/systems/ui-architecture.md` 섹션 1.2 |
| 업그레이드 비용, 재료, 등급, 소요 일수 | `docs/systems/tool-upgrade.md` |
| 대장간 NPC 캐릭터 설정, 대화 예시, 영업 정보 | `docs/content/npcs.md` 섹션 4 (CON-003) |
| 친밀도 단계 임계값, 보상 | (-> see `docs/content/npcs.md`) |
| ProgressionManager, XPSource enum | `docs/systems/progression-architecture.md` |
| AchievementManager, AchievementConditionType | `docs/systems/achievement-architecture.md` |
| 프로젝트 폴더 구조, 네임스페이스 | `docs/systems/project-structure.md` |

---

# Part I -- 설계 개요

---

## 1. 클래스 다이어그램

```
+-----------------------------------------------------------------------+
|                  대장간 NPC 시스템 전체 구조                              |
+-----------------------------------------------------------------------+

┌──────────────────────────────────────────────────────────────────┐
│              BlacksmithNPC (MonoBehaviour)                        │
│──────────────────────────────────────────────────────────────────│
│  [설정 참조]                                                       │
│  - _npcData: NPCData                 (→ see npc-shop-architecture│
│                                        .md 섹션 2.1)              │
│  - _blacksmithData: BlacksmithNPCData (SO, 대장간 고유 데이터)     │
│                                                                  │
│  [외부 참조]                                                       │
│  - _dialogueSystem: DialogueSystem   (싱글턴 참조)               │
│  - _upgradeSystem: ToolUpgradeSystem (싱글턴 참조)               │
│  - _affinityTracker: NPCAffinityTracker (친밀도 추적)            │
│                                                                  │
│  [상태]                                                           │
│  - _interactionState: BlacksmithInteractionState (FSM)           │
│  - _isInteracting: bool                                          │
│                                                                  │
│  [메서드]                                                         │
│  + Interact(PlayerController player): void                       │
│  + HandleDialogueChoice(DialogueChoiceAction action): void       │
│  + OnUpgradeUIResult(UpgradeUIResult result): void               │
│  - SelectGreetingDialogue(): DialogueData                        │
│  - CheckPendingPickup(): bool                                    │
│  - BuildChoiceList(): DialogueChoice[]                           │
│                                                                  │
│  [이벤트 구독]                                                     │
│  + OnEnable():                                                   │
│      ToolUpgradeEvents.OnUpgradeCompleted += HandleUpgradeComplete│
│  + OnDisable(): 구독 해제                                          │
└──────────────────────────────────────────────────────────────────┘
         │                        │                       │
         │ delegates to           │ opens                 │ tracks
         ▼                        ▼                       ▼
┌──────────────────┐    ┌──────────────────┐    ┌──────────────────┐
│  DialogueSystem  │    │  ToolUpgradeUI   │    │NPCAffinityTracker│
│  (→ ARC-008)     │    │  (ScreenBase)    │    │  (MonoBehaviour) │
│  대화 흐름 관리   │    │  업그레이드 선택  │    │  친밀도 추적     │
└──────────────────┘    └──────────────────┘    └──────────────────┘
                               │
                               │ validates & executes
                               ▼
                     ┌──────────────────────┐
                     │  ToolUpgradeSystem   │
                     │  (→ DES-007)         │
                     │  업그레이드 처리      │
                     └──────────────────────┘
                        │              │
                        ▼              ▼
               ┌──────────────┐ ┌──────────────────┐
               │EconomyManager│ │InventoryManager  │
               │ (골드 차감)   │ │(재료 차감/도구 교체)│
               └──────────────┘ └──────────────────┘


┌──────────────────────────────────────────────────────────────────┐
│           BlacksmithNPCData (ScriptableObject)                    │
│──────────────────────────────────────────────────────────────────│
│  + npcId: string                                                 │
│  + displayName: string          (→ see docs/content/npcs.md)     │
│  + greetingDialogues: DialogueData[]   (친밀도 단계별 인사말)     │
│  + closedDialogue: DialogueData        (영업 외 대화)             │
│  + pendingPickupDialogue: DialogueData (완성 도구 수령 안내)      │
│  + affinityThresholds: int[]           (친밀도 단계 임계값,       │
│  │                                      → see docs/content/npcs.md)│
│  + affinityDialogues: DialogueData[]   (단계별 특수 대화)         │
│  + upgradeCompleteAffinity: int        (업그레이드 완료 시 친밀도  │
│  │                                      증가량, → see canonical)  │
│  + materialPurchaseAffinity: int       (재료 구매 시 친밀도 증가량)│
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│           NPCAffinityTracker (MonoBehaviour)                      │
│──────────────────────────────────────────────────────────────────│
│  [상태]                                                           │
│  - _affinityMap: Dictionary<string, int>   (npcId → 현재 친밀도)  │
│                                                                  │
│  [메서드]                                                         │
│  + GetAffinity(string npcId): int                                │
│  + AddAffinity(string npcId, int amount): void                   │
│  + GetAffinityLevel(string npcId, int[] thresholds): int         │
│  + GetSaveData(): AffinitySaveData                               │
│  + LoadSaveData(AffinitySaveData data): void                     │
│                                                                  │
│  [이벤트]                                                         │
│  + OnAffinityChanged: Action<string, int, int>                   │
│  │   // npcId, oldValue, newValue                                │
│  + OnAffinityLevelUp: Action<string, int>                        │
│  │   // npcId, newLevel                                          │
└──────────────────────────────────────────────────────────────────┘
```

### 클래스 책임 요약

| 클래스 | 유형 | 네임스페이스 | 책임 |
|--------|------|-------------|------|
| **BlacksmithNPC** | MonoBehaviour | `SeedMind.NPC` | 대장간 NPC 상호작용 진입점, 대화/서비스 위임, 선택지 동적 구성 |
| **BlacksmithNPCData** | ScriptableObject | `SeedMind.NPC.Data` | 대장간 NPC 고유 데이터 (친밀도별 대화, 친밀도 임계값) |
| **ToolUpgradeUI** | MonoBehaviour (ScreenBase) | `SeedMind.UI` | 업그레이드 도구 선택/비용 확인/실행 화면 |
| **NPCAffinityTracker** | MonoBehaviour | `SeedMind.NPC` | 전체 NPC 친밀도 추적, 세이브/로드 (범용, 대장간 전용 아님) |
| **BlacksmithEvents** | static class | `SeedMind.NPC` | 대장간 고유 이벤트 허브 |

---

## 2. 업그레이드 UX 흐름 (State Machine)

### 2.1 BlacksmithInteractionState enum

```csharp
// illustrative
namespace SeedMind.NPC
{
    public enum BlacksmithInteractionState
    {
        Idle,              // 상호작용 없음
        Greeting,          // 인사 대화 재생 중
        ServiceMenu,       // 서비스 선택지 표시 중
        Chatting,          // "이야기하기" 선택 시 랜덤 일상 대화 재생
        UpgradeSelect,     // ToolUpgradeUI에서 도구 선택 중
        UpgradeConfirm,    // 업그레이드 확인 팝업
        UpgradeResult,     // 업그레이드 시작/실패 결과 표시
        PickupResult,      // 완성 도구 수령 결과 표시
        MaterialShop,      // 재료 구매 상점 (ShopUI 위임)
    }
}
```

### 2.2 상태 전이 다이어그램

```
┌──────┐
│ Idle │
└──┬───┘
   │ [E키 인터랙션]
   ▼
┌──────────┐
│ Greeting │◀────────────────────────────────┐
└──┬───────┘                                 │
   │ [대사 완료]                              │
   ▼                                         │
┌─────────────┐                              │
│ ServiceMenu │◀──────────────────────┐      │
│             │◀───────────────┐      │      │
└──┬──┬──┬──┬─┘                │      │      │
   │  │  │  │                  │      │      │
   │  │  │  └─[나가기]──────────┼──────┼──▶ Idle
   │  │  │                     │      │
   │  │  └─[재료 구매]──▶ ┌────────────┐
   │  │                    │MaterialShop│──[닫기]─┘
   │  │                    └────────────┘
   │  │
   │  └─[도구 수령]──▶ ┌──────────────┐
   │    (완성 도구 有)   │ PickupResult │──[확인]──┘
   │                    └──────────────┘
   │
   └─[도구 업그레이드]──▶ ┌───────────────┐
                          │ UpgradeSelect │◀────────────┐
                          └──┬────────────┘              │
                             │                           │
                             ├─[취소/Esc]──▶ ServiceMenu │
                             │                           │
                             └─[도구 선택]                │
                                │                        │
                                ▼                        │
                          ┌───────────────┐              │
                          │UpgradeConfirm │              │
                          └──┬──┬─────────┘              │
                             │  │                        │
                             │  └─[취소]─────────────────┘
                             │
                             └─[확인]
                                │
                                ▼
                          ┌───────────────┐
                          │UpgradeResult  │
                          │ (성공/실패)    │──[확인]──▶ ServiceMenu
                          └───────────────┘
```

### 2.3 각 State 상세

| State | 입력 | 출력 | 취소 경로 |
|-------|------|------|-----------|
| **Idle** | E키 (인터랙션 범위 내) | Greeting으로 전이 | -- |
| **Greeting** | 대사 자동 진행 / 클릭 | ServiceMenu로 전이 | Esc -> Idle |
| **ServiceMenu** | 선택지 클릭 (업그레이드/재료/수령/이야기하기/나가기) | 선택에 따른 State 전이 | "나가기" 선택 -> Idle |
| **Chatting** | 선택지 클릭 (이야기하기) | 랜덤 일상 대사 1종 출력 후 ServiceMenu | Esc -> ServiceMenu |
| **UpgradeSelect** | 도구 슬롯 클릭 | UpgradeConfirm으로 전이 (CanUpgrade 통과 시) | Esc/취소 -> ServiceMenu |
| **UpgradeConfirm** | 확인/취소 버튼 | UpgradeResult 또는 UpgradeSelect로 전이 | 취소 -> UpgradeSelect |
| **UpgradeResult** | 확인 버튼 | ServiceMenu로 전이 | -- (확인만 가능) |
| **PickupResult** | 확인 버튼 | ServiceMenu로 전이 | -- (확인만 가능) |
| **MaterialShop** | ShopUI 인터랙션 | 구매 처리 후 유지 | 닫기/Esc -> ServiceMenu |

### 2.4 UIManager Screen FSM 통합

대장간 상호작용 시 UIManager의 Screen 전환 흐름:

```
[Farming] ──(NPC 상호작용)──▶ [Dialogue] ──(서비스 선택)──▶ [ToolUpgrade]
                                                             또는 [Shop]
```

ScreenType enum 확장이 필요하다:

```csharp
// illustrative -- ui-architecture.md ScreenType enum 확장
// → see docs/systems/ui-architecture.md 섹션 1.2
public enum ScreenType
{
    // ... 기존 값 유지 ...
    ToolUpgrade = 11,   // 대장간 도구 업그레이드 화면 (NEW)
}
```

[RISK] ScreenType에 `ToolUpgrade = 11`을 추가하면 `ui-architecture.md` 섹션 1.2의 ScreenType enum을 동시에 업데이트해야 한다. 전이 규칙도 함께 추가해야 한다: `[Dialogue] --(OpenUpgrade)--> [ToolUpgrade] --(Esc/닫기)--> [Farming]`.

---

## 3. ToolUpgradeUI 레이아웃 구조

### 3.1 Unity UI 계층

```
Canvas_Overlay (Order: 10, → see ui-architecture.md 섹션 2)
  └── ToolUpgradeScreen (ScreenBase 파생, 기본 비활성)
        ├── Background (Image, 반투명 검정 오버레이)
        │
        ├── MainPanel (RectTransform, 중앙 정렬, 800x600)
        │     │
        │     ├── Header (HorizontalLayoutGroup)
        │     │     ├── TitleText (TMP, "도구 업그레이드")
        │     │     └── CloseButton (Button, "X")
        │     │
        │     ├── ContentArea (HorizontalLayoutGroup, 좌우 분할)
        │     │     │
        │     │     ├── LeftPanel (VerticalLayoutGroup, 비중 40%)
        │     │     │     ├── ToolListLabel (TMP, "보유 도구")
        │     │     │     └── ToolSlotContainer (VerticalLayoutGroup)
        │     │     │           ├── ToolSlot_0 (ToolUpgradeSlotUI)
        │     │     │           │     ├── ToolIcon (Image)
        │     │     │           │     ├── ToolName (TMP)
        │     │     │           │     ├── TierBadge (Image, 등급 색상)
        │     │     │           │     └── UpgradeArrow (Image, 업그레이드 가능 시 표시)
        │     │     │           ├── ToolSlot_1
        │     │     │           └── ToolSlot_2
        │     │     │
        │     │     └── RightPanel (VerticalLayoutGroup, 비중 60%)
        │     │           │
        │     │           ├── ComparisonView (HorizontalLayoutGroup)
        │     │           │     ├── CurrentToolPanel (VerticalLayoutGroup)
        │     │           │     │     ├── Label (TMP, "현재")
        │     │           │     │     ├── ToolIcon (Image)
        │     │           │     │     ├── ToolName (TMP)
        │     │           │     │     ├── TierText (TMP, "기본 등급")
        │     │           │     │     └── StatsGroup (VerticalLayoutGroup)
        │     │           │     │           ├── RangeStat (TMP, "범위: 1")
        │     │           │     │           ├── SpeedStat (TMP, "속도: 1.0x")
        │     │           │     │           └── SpecialStat (TMP, "특수: 없음")
        │     │           │     │
        │     │           │     ├── ArrowIcon (Image, ">>>")
        │     │           │     │
        │     │           │     └── UpgradedToolPanel (VerticalLayoutGroup)
        │     │           │           ├── Label (TMP, "업그레이드 후")
        │     │           │           ├── ToolIcon (Image)
        │     │           │           ├── ToolName (TMP)
        │     │           │           ├── TierText (TMP, "강화 등급", 강조 색상)
        │     │           │           └── StatsGroup (VerticalLayoutGroup)
        │     │           │                 ├── RangeStat (TMP, "범위: 3", 변화 강조)
        │     │           │                 ├── SpeedStat (TMP, "속도: 1.2x", 변화 강조)
        │     │           │                 └── SpecialStat (TMP, "특수: 없음")
        │     │           │
        │     │           ├── CostPanel (VerticalLayoutGroup)
        │     │           │     ├── CostLabel (TMP, "필요 비용")
        │     │           │     ├── GoldCost (HorizontalLayoutGroup)
        │     │           │     │     ├── GoldIcon (Image)
        │     │           │     │     └── GoldText (TMP, 충족=흰색/부족=빨강)
        │     │           │     ├── MaterialSlotList (VerticalLayoutGroup)
        │     │           │     │     ├── MaterialSlot_0
        │     │           │     │     │     ├── MaterialIcon (Image)
        │     │           │     │     │     ├── MaterialName (TMP)
        │     │           │     │     │     └── QuantityText (TMP, "2/3", 충족=흰/부족=빨강)
        │     │           │     │     └── MaterialSlot_1 ...
        │     │           │     └── TimeCost (HorizontalLayoutGroup)
        │     │           │           ├── ClockIcon (Image)
        │     │           │           └── TimeText (TMP, "소요: N일")
        │     │           │
        │     │           └── LevelRequirement (HorizontalLayoutGroup)
        │     │                 ├── LevelIcon (Image)
        │     │                 └── LevelText (TMP, "필요 레벨: N", 충족/부족 색상)
        │     │
        │     └── Footer (HorizontalLayoutGroup, 우측 정렬)
        │           ├── CancelButton (Button, "취소")
        │           └── UpgradeButton (Button, "업그레이드 시작")
        │                 └── (비활성 조건: CanUpgrade == false)
        │
        └── ConfirmPopup (PopupBase 파생, 기본 비활성)
              ├── ConfirmText (TMP, "[도구명]을 업그레이드하시겠습니까?")
              ├── ConfirmButton (Button, "확인")
              └── CancelButton (Button, "취소")
```

### 3.2 충족/부족 상태 표시 규칙

| 항목 | 충족 시 | 부족 시 |
|------|---------|---------|
| 골드 | 흰색 텍스트 | 빨간색 텍스트 + 깜빡임 |
| 재료 수량 | 흰색 "보유/필요" | 빨간색 "보유/필요" |
| 플레이어 레벨 | 흰색 + 체크 아이콘 | 빨간색 + 자물쇠 아이콘 |
| 업그레이드 버튼 | 활성 (interactable=true) | 비활성 (interactable=false, 회색) |

### 3.3 ToolUpgradeUI 클래스 설계

```csharp
// illustrative
namespace SeedMind.UI
{
    /// <summary>
    /// 대장간 도구 업그레이드 화면.
    /// ScreenBase를 상속하여 UIManager Screen FSM에 통합된다.
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

        // --- 상태 ---
        private ToolData _selectedTool;
        private UpgradeCheckResult _checkResult;
        private List<ToolUpgradeSlotUI> _toolSlots = new List<ToolUpgradeSlotUI>();

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
        // + OnUpgradeButtonClicked(): void  → 확인 팝업 표시
        // + OnConfirmUpgrade(): void        → ToolUpgradeSystem.StartUpgrade()
        // + OnCancelUpgrade(): void         → 확인 팝업 닫기
    }
}
```

### 3.4 ToolUpgradeSlotUI

```csharp
// illustrative
namespace SeedMind.UI
{
    /// <summary>
    /// 업그레이드 UI의 개별 도구 슬롯.
    /// tool-upgrade-architecture.md의 ToolUpgradeSlotUI를 구체화한다.
    /// </summary>
    public class ToolUpgradeSlotUI : MonoBehaviour
    {
        [SerializeField] private Image _toolIcon;
        [SerializeField] private TMP_Text _toolName;
        [SerializeField] private Image _tierBadge;
        [SerializeField] private GameObject _upgradeArrow;
        [SerializeField] private GameObject _pendingOverlay; // "업그레이드 중" 표시
        [SerializeField] private TMP_Text _pendingDaysText;  // "잔여 N일"
        [SerializeField] private Button _selectButton;

        // + Setup(ToolData tool, UpgradeCheckResult result, PendingUpgrade pending): void
        // + SetSelected(bool selected): void
    }
}
```

---

## 4. BlacksmithNPCData ScriptableObject 스키마

### 4.1 JSON 예시

```json
{
    "npcId": "npc_cheolsu",
    "displayName": "철수",
    "greetingDialogueIds": [
        "dlg_blacksmith_greet_lv0",
        "dlg_blacksmith_greet_lv1",
        "dlg_blacksmith_greet_lv2",
        "dlg_blacksmith_greet_lv3"
    ],
    "closedDialogueId": "dlg_blacksmith_closed",
    "pendingPickupDialogueId": "dlg_blacksmith_pickup",
    "affinityThresholds": "(→ see docs/content/blacksmith-npc.md 섹션 2.5)",
    "affinityDialogueIds": [
        "dlg_blacksmith_affinity_lv1",
        "dlg_blacksmith_affinity_lv2",
        "dlg_blacksmith_affinity_lv3"
    ],
    "upgradeCompleteAffinity": 5,
    "materialPurchaseAffinity": 1,
    "specialDiscountAffinityLevel": 3,
    "discountRate": 0.1
}
```

### 4.2 C# 클래스 정의

```csharp
// illustrative
namespace SeedMind.NPC.Data
{
    /// <summary>
    /// 대장간 NPC 고유 데이터.
    /// 기본 NPCData(→ see npc-shop-architecture.md 섹션 2.1)를 보완하여
    /// 친밀도 기반 대화 전환, 업그레이드 보상 등을 정의한다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBlacksmithNPCData", menuName = "SeedMind/BlacksmithNPCData")]
    public class BlacksmithNPCData : ScriptableObject
    {
        [Header("기본 식별")]
        public string npcId;                              // "npc_cheolsu"
        public string displayName;                        // "철수" (→ see docs/content/npcs.md 섹션 4.1)

        [Header("대화 -- 친밀도 단계별 인사")]
        public DialogueData[] greetingDialogues;          // 인덱스 = 친밀도 단계 (0~3)
        public DialogueData closedDialogue;               // 영업 외/휴무 시 대화
        public DialogueData pendingPickupDialogue;        // 완성 도구 수령 안내 대화

        [Header("친밀도")]
        public int[] affinityThresholds;                  // 단계별 임계값
                                                          // (→ see docs/content/blacksmith-npc.md 섹션 2.5)
        public DialogueData[] affinityDialogues;          // 단계 상승 시 일회성 특수 대화 (인덱스 = 새 단계 - 1)

        [Header("친밀도 보상")]
        public int upgradeCompleteAffinity;               // 업그레이드 완료 시 친밀도 증가량
                                                          // (→ see docs/content/npcs.md)
        public int materialPurchaseAffinity;              // 재료 1회 구매 시 친밀도 증가량
                                                          // (→ see docs/content/npcs.md)

        [Header("친밀도 혜택")]
        public int specialDiscountAffinityLevel;          // 할인 혜택 해금 친밀도 단계
                                                          // (→ see docs/content/npcs.md)
        public float discountRate;                        // 할인율 (0.1 = 10%)
                                                          // (→ see docs/content/npcs.md)
    }
}
```

**PATTERN-005 검증**: JSON 예시와 C# 클래스의 필드 대응표:

| JSON 필드 | C# 필드 | 타입 매칭 |
|-----------|---------|-----------|
| `npcId` | `npcId` | string = string |
| `displayName` | `displayName` | string = string |
| `greetingDialogueIds` | `greetingDialogues` | string[] -> DialogueData[] (SO 참조로 변환) |
| `closedDialogueId` | `closedDialogue` | string -> DialogueData (SO 참조로 변환) |
| `pendingPickupDialogueId` | `pendingPickupDialogue` | string -> DialogueData (SO 참조로 변환) |
| `affinityThresholds` | `affinityThresholds` | int[] = int[] |
| `affinityDialogueIds` | `affinityDialogues` | string[] -> DialogueData[] (SO 참조로 변환) |
| `upgradeCompleteAffinity` | `upgradeCompleteAffinity` | int = int |
| `materialPurchaseAffinity` | `materialPurchaseAffinity` | int = int |
| `specialDiscountAffinityLevel` | `specialDiscountAffinityLevel` | int = int |
| `discountRate` | `discountRate` | float = float |

> JSON에서 대화 데이터는 ID(string)로 참조하고, C# SO에서는 직접 DialogueData 참조를 사용한다. 런타임에서 JSON 임포트 시 DataRegistry를 통해 ID -> SO 참조로 변환한다.

에셋 이름: `SO_BlacksmithNPC_Cheolsu.asset`  
저장 경로: `Assets/_Project/Data/NPCs/`

---

## 5. 친밀도(Affinity) 시스템 연동

### 5.1 설계 원칙

친밀도 시스템은 **NPCAffinityTracker**에서 범용으로 관리하며, 대장간뿐 아니라 모든 NPC에 적용 가능한 구조로 설계한다. NPC별 친밀도 임계값과 보상은 각 NPC의 SO 데이터에서 정의한다.

### 5.2 친밀도 증가 트리거

| 트리거 | 증가량 | 발생 조건 |
|--------|--------|-----------|
| 도구 업그레이드 완료 | `upgradeCompleteAffinity` (-> see `docs/content/npcs.md`) | ToolUpgradeEvents.OnUpgradeCompleted 수신 시 |
| 재료 구매 | `materialPurchaseAffinity` (-> see `docs/content/npcs.md`) | 대장간 상점에서 아이템 구매 완료 시 |
| 일상 대화 | 1 (고정) | 하루 첫 대화 시 1회만 (중복 방지) |

### 5.3 친밀도 단계별 효과

친밀도 단계는 `affinityThresholds` 배열로 정의된다. 구체적 임계값과 보상은 (-> see `docs/content/npcs.md`).

```
단계 0: 초면 (친밀도 0 ~ threshold[1]-1)
  → 기본 인사말 (greetingDialogues[0])
  → 서비스 이용 가능 (업그레이드, 재료 구매)

단계 1: 알고 지내는 사이 (threshold[1] ~ threshold[2]-1)
  → 친근한 인사말 (greetingDialogues[1])
  → 도구 관리 팁 대사 해금

단계 2: 단골 (threshold[2] ~ threshold[3]-1)
  → 특별 인사말 (greetingDialogues[2])
  → 업그레이드 소요 시간 1일 단축 힌트 대사

단계 3: 오랜 친구 (threshold[3] 이상)
  → 개인적 이야기 공유 (greetingDialogues[3])
  → 재료 구매 할인 (discountRate, → see docs/content/npcs.md)
```

### 5.4 친밀도 단계 대화 트리거 조건

```
BlacksmithNPC.SelectGreetingDialogue():
    │
    ├── 1) affinityLevel = _affinityTracker.GetAffinityLevel(npcId, thresholds)
    │
    ├── 2) 단계 상승 직후인가? (이전 방문 시 단계와 비교)
    │       ├── true → affinityDialogues[newLevel - 1] 반환 (일회성 특수 대화)
    │       └── false → 계속
    │
    ├── 3) 완성 도구가 있는가? (_upgradeSystem.GetPendingUpgrade() 확인)
    │       ├── true & remainingDays == 0 → pendingPickupDialogue 반환
    │       └── false → 계속
    │
    └── 4) greetingDialogues[affinityLevel] 반환 (단계별 기본 인사)
```

### 5.5 NPCAffinityTracker 세이브 데이터

```csharp
// illustrative
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
        public string npcId;              // "npc_cheolsu"
        public int affinityValue;         // 현재 친밀도 수치
        public int lastVisitDay;          // 마지막 방문 일차 (일일 대화 친밀도 중복 방지)
        public bool[] triggeredDialogues; // 단계별 특수 대화 재생 여부
    }
}
```

---

## 6. 이벤트 설계

### 6.1 BlacksmithEvents (정적 이벤트 허브)

```csharp
// illustrative
namespace SeedMind.NPC
{
    /// <summary>
    /// 대장간 NPC 고유 이벤트 허브.
    /// NPCEvents(→ see npc-shop-architecture.md 섹션 4)를 보완한다.
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

### 6.2 ProgressionManager XP 부여 연동

업그레이드 완료 시 XP 부여는 기존 `ToolUpgradeEvents.OnUpgradeCompleted`를 통해 ProgressionManager가 수신한다 (-> see `docs/systems/tool-upgrade-architecture.md` 섹션 8.2).

**XPSource 확장 검토**:

| 방안 | 설명 | 결정 |
|------|------|------|
| A: 기존 `XPSource.FacilityBuild` 재활용 | 시설 건설과 동일 카테고리로 처리 | 기각 -- 의미적으로 부적합 |
| **B: `XPSource.ToolUpgrade` 신규 추가 (채택)** | 도구 업그레이드 전용 XP 소스 | **채택** |

```csharp
// illustrative -- progression-architecture.md XPSource enum 확장
// → see docs/systems/progression-architecture.md 섹션 2.2
namespace SeedMind.Level
{
    public enum XPSource
    {
        CropHarvest,
        ToolUse,
        FacilityBuild,
        FacilityProcess,
        MilestoneReward,
        QuestComplete,
        AchievementReward,
        ToolUpgrade,        // NEW: 도구 업그레이드 완료
    }
}
```

[RISK] `XPSource.ToolUpgrade` 추가 시 `progression-architecture.md` 섹션 2.2의 enum 정의와 섹션 2.3의 `GetExpForSource()` switch 문을 동시에 업데이트해야 한다.

**XP 부여 흐름**:

```
ToolUpgradeEvents.OnUpgradeCompleted
    │
    ▼
ProgressionManager.HandleToolUpgradeCompleted(ToolUpgradeInfo info)
    │
    ├── int xp = GetExpForSource(XPSource.ToolUpgrade, info.newTier)
    │   // xp 수치: → see docs/balance/progression-curve.md
    │
    └── AddExp(xp, XPSource.ToolUpgrade)
```

### 6.3 AchievementManager 업적 트리거 연동

기존 `achievement-architecture.md`에서 `ToolEvents.OnToolUpgraded` 이벤트를 구독하여 `AchievementConditionType.ToolUpgradeCount`를 갱신하는 구조가 정의되어 있다 (-> see 섹션 5.1, 이벤트-조건 매핑 표).

대장간 아키텍처에서 추가로 트리거할 수 있는 업적 조건:

| 이벤트 | 트리거되는 AchievementConditionType | 비고 |
|--------|-------------------------------------|------|
| `ToolUpgradeEvents.OnUpgradeCompleted` | `ToolUpgradeCount` | 기존 매핑 유지 (-> see achievement-architecture.md) |
| `BlacksmithEvents.OnAffinityLevelUp` | (신규 검토 필요) | [OPEN] 친밀도 업적 카테고리 추가 여부 |

[OPEN] NPC 친밀도 관련 업적 (예: "대장장이와 오랜 친구가 되다")을 추가할 경우, `AchievementConditionType`에 `NPCAffinityLevel` 값을 추가하고, AchievementManager에 `BlacksmithEvents.OnAffinityLevelUp` 구독 핸들러를 추가해야 한다.

### 6.4 이벤트 소비 매핑 종합

| 이벤트 | 발행자 | 소비자 | 용도 |
|--------|--------|--------|------|
| `BlacksmithEvents.OnUpgradeRequested` | ToolUpgradeUI | HUD (NotificationManager) | "호미를 대장간에 맡겼습니다" 토스트 |
| `BlacksmithEvents.OnToolPickedUp` | BlacksmithNPC | HUD (NotificationManager) | "강화 호미를 수령했습니다!" 토스트 |
| `BlacksmithEvents.OnAffinityLevelUp` | NPCAffinityTracker | DialogueSystem (특수 대화 트리거), AchievementManager (검토 중) | 친밀도 단계 상승 연출 |
| `ToolUpgradeEvents.OnUpgradeCompleted` | ToolUpgradeSystem | ProgressionManager (XP), AchievementManager (업적), BlacksmithNPC (친밀도 증가) | 다중 소비 |
| `NPCEvents.OnDialogueEnded` | DialogueSystem | BlacksmithNPC (FSM 상태 전이) | 대화 종료 후 ServiceMenu 진입 |

---

## 7. 프로젝트 구조 반영

### 7.1 신규/수정 파일 목록

| 경로 | 클래스 | 네임스페이스 | 비고 |
|------|--------|-------------|------|
| `Scripts/NPC/BlacksmithNPC.cs` | BlacksmithNPC | SeedMind.NPC | 신규 |
| `Scripts/NPC/NPCAffinityTracker.cs` | NPCAffinityTracker | SeedMind.NPC | 신규 (범용) |
| `Scripts/NPC/BlacksmithEvents.cs` | BlacksmithEvents | SeedMind.NPC | 신규 |
| `Scripts/NPC/Data/BlacksmithNPCData.cs` | BlacksmithNPCData | SeedMind.NPC.Data | 신규 |
| `Scripts/NPC/Data/BlacksmithInteractionState.cs` | BlacksmithInteractionState | SeedMind.NPC | 신규 |
| `Scripts/NPC/Data/AffinitySaveData.cs` | AffinitySaveData, AffinityEntry | SeedMind.NPC | 신규 |
| `Scripts/UI/ToolUpgradeUI.cs` | ToolUpgradeUI | SeedMind.UI | 신규 (ScreenBase 파생) |
| `Scripts/UI/ToolUpgradeSlotUI.cs` | ToolUpgradeSlotUI | SeedMind.UI | 기존 (DES-007) 구체화 |
| `Scripts/UI/ToolComparisonPanel.cs` | ToolComparisonPanel | SeedMind.UI | 신규 (비교 뷰 패널) |
| `Scripts/UI/MaterialSlotUI.cs` | MaterialSlotUI | SeedMind.UI | 신규 (재료 슬롯) |

모든 경로 접두어: `Assets/_Project/`

### 7.2 SO 에셋 목록

| 에셋 경로 | 에셋 이름 | 비고 |
|-----------|-----------|------|
| `Data/NPCs/` | `SO_BlacksmithNPC_Cheolsu.asset` | BlacksmithNPCData SO |
| `Data/Dialogues/` | `SO_Dlg_Blacksmith_Greet_Lv0.asset` | 친밀도 단계 0 인사 |
| `Data/Dialogues/` | `SO_Dlg_Blacksmith_Greet_Lv1.asset` | 친밀도 단계 1 인사 |
| `Data/Dialogues/` | `SO_Dlg_Blacksmith_Greet_Lv2.asset` | 친밀도 단계 2 인사 |
| `Data/Dialogues/` | `SO_Dlg_Blacksmith_Greet_Lv3.asset` | 친밀도 단계 3 인사 |
| `Data/Dialogues/` | `SO_Dlg_Blacksmith_Closed.asset` | 영업 외 대화 |
| `Data/Dialogues/` | `SO_Dlg_Blacksmith_Pickup.asset` | 도구 수령 안내 |
| `Data/Dialogues/` | `SO_Dlg_Blacksmith_Affinity_Lv1~3.asset` | 단계 상승 특수 대화 |

---

# Part II -- MCP 구현 태스크 요약

---

## 8. MCP 태스크 시퀀스

### Phase A: 스크립트 작성 (Claude Code 직접 작성, MCP 불필요)

```
Step A-1: Scripts/NPC/Data/BlacksmithInteractionState.cs 작성
          → namespace SeedMind.NPC
          → enum BlacksmithInteractionState 정의

Step A-2: Scripts/NPC/Data/BlacksmithNPCData.cs 작성
          → namespace SeedMind.NPC.Data
          → ScriptableObject, 필드: 섹션 4.2 참조

Step A-3: Scripts/NPC/Data/AffinitySaveData.cs 작성
          → namespace SeedMind.NPC
          → AffinitySaveData, AffinityEntry 클래스

Step A-4: Scripts/NPC/NPCAffinityTracker.cs 작성
          → namespace SeedMind.NPC
          → MonoBehaviour, 범용 친밀도 추적
          → GetAffinity, AddAffinity, GetAffinityLevel, GetSaveData, LoadSaveData

Step A-5: Scripts/NPC/BlacksmithEvents.cs 작성
          → namespace SeedMind.NPC
          → static class, 이벤트: 섹션 6.1 참조

Step A-6: Scripts/NPC/BlacksmithNPC.cs 작성
          → namespace SeedMind.NPC
          → MonoBehaviour, FSM 기반 상호작용: 섹션 2 참조

Step A-7: Scripts/UI/ToolComparisonPanel.cs 작성
          → namespace SeedMind.UI
          → 현재/업그레이드 후 스탯 비교 패널

Step A-8: Scripts/UI/MaterialSlotUI.cs 작성
          → namespace SeedMind.UI
          → 재료 슬롯 (아이콘 + 이름 + 수량)

Step A-9: Scripts/UI/ToolUpgradeUI.cs 작성
          → namespace SeedMind.UI
          → ScreenBase 파생, 섹션 3.3 참조

Step A-10: Scripts/UI/ToolUpgradeSlotUI.cs 수정 (기존 DES-007 구체화)
           → 업그레이드 중 오버레이, 잔여 일수 표시 추가

Step A-11: 기존 파일 수정
           → ui-architecture.md의 ScreenType enum에 ToolUpgrade = 11 추가
           → progression-architecture.md의 XPSource enum에 ToolUpgrade 추가
           → GetExpForSource() switch 문에 case 추가
```

### Phase B: SO 에셋 생성 (MCP)

```
Step B-1: BlacksmithNPCData SO 생성
          B-1-01: create_asset → "SO_BlacksmithNPC_Cheolsu", type: BlacksmithNPCData
                  → npcId = "npc_cheolsu"
                  → displayName = "철수"
                  → 친밀도 수치: (→ see docs/content/npcs.md)

Step B-2: 대화 DialogueData SO 생성 (8개)
          B-2-01~04: create_asset → SO_Dlg_Blacksmith_Greet_Lv0~Lv3
          B-2-05: create_asset → SO_Dlg_Blacksmith_Closed
          B-2-06: create_asset → SO_Dlg_Blacksmith_Pickup
          B-2-07~09: create_asset → SO_Dlg_Blacksmith_Affinity_Lv1~Lv3
          → 대사 텍스트: (→ see docs/content/blacksmith-npc.md 섹션 3.1~3.7)

Step B-3: BlacksmithNPCData에 DialogueData 참조 연결
          B-3-01: set_property → greetingDialogues[0~3] = SO_Dlg_Blacksmith_Greet_*
          B-3-02: set_property → closedDialogue = SO_Dlg_Blacksmith_Closed
          B-3-03: set_property → pendingPickupDialogue = SO_Dlg_Blacksmith_Pickup
          B-3-04: set_property → affinityDialogues[0~2] = SO_Dlg_Blacksmith_Affinity_*
```

[RISK] MCP의 SO 배열 참조 설정 지원 여부. tool-upgrade-architecture.md Phase B와 동일 리스크 -- 대안으로 string ID 기반 런타임 조회 가능.

### Phase C: 씬 오브젝트 및 UI 구성 (MCP)

```
Step C-1: 씬에 BlacksmithNPC GameObject 생성
          C-1-01: create_object → "BlacksmithNPC", position: (→ see docs/content/npcs.md 섹션 4)
          C-1-02: add_component → BlacksmithNPC (SeedMind.NPC.BlacksmithNPC)
          C-1-03: set_property → _npcData = SO_NPC_Blacksmith
          C-1-04: set_property → _blacksmithData = SO_BlacksmithNPC_Cheolsu

Step C-2: 씬에 NPCAffinityTracker 추가
          C-2-01: --- MANAGERS --- 하위에 create_object → "NPCAffinityTracker"
          C-2-02: add_component → NPCAffinityTracker (SeedMind.NPC.NPCAffinityTracker)

Step C-3: Canvas_Overlay 하위에 ToolUpgradeScreen 생성
          C-3-01: create_object → "ToolUpgradeScreen", parent: "Canvas_Overlay"
          C-3-02: add_component → RectTransform, CanvasGroup
          C-3-03: set_property → 기본 비활성 (alpha=0, blocksRaycasts=false)
          C-3-04: add_component → ToolUpgradeUI (SeedMind.UI.ToolUpgradeUI)

Step C-4: ToolUpgradeScreen 내부 UI 계층 구성 (섹션 3.1 참조)
          C-4-01~15: create_object → Background, MainPanel, Header, TitleText,
                     CloseButton, ContentArea, LeftPanel, ToolSlotContainer,
                     RightPanel, ComparisonView, CurrentToolPanel, UpgradedToolPanel,
                     CostPanel, Footer, ConfirmPopup
          → 세부 레이아웃 수치는 MCP 실행 시 결정

Step C-5: UIManager에 ToolUpgradeScreen 등록
          C-5-01: UIManager._screens에 ScreenType.ToolUpgrade 키로 ToolUpgradeUI 등록
```

### Phase D: 통합 테스트 (MCP Play Mode)

```
Step D-1: Play Mode 진입
          → Console 로그로 BlacksmithNPC 초기화 확인
          → NPCAffinityTracker 초기 상태 확인

Step D-2: 대장간 상호작용 테스트
          → 플레이어를 BlacksmithNPC 인터랙션 범위 내로 이동
          → E키 입력 → 인사 대화 재생 확인
          → 서비스 메뉴 선택지 표시 확인

Step D-3: 업그레이드 UI 테스트
          → "도구 업그레이드" 선택 → ToolUpgradeUI 열림 확인
          → 도구 목록 표시, 비교 뷰, 비용 패널 확인
          → 부족 조건 시 버튼 비활성 확인

Step D-4: 친밀도 테스트
          → 콘솔 커맨드로 친밀도 수동 증가
          → 단계 상승 시 특수 대화 트리거 확인
          → 인사말 변경 확인

Step D-5: 세이브/로드 테스트
          → 친밀도 값 포함 세이브
          → 로드 후 친밀도 복원 확인
```

---

## Open Questions

- [OPEN] NPC 친밀도 관련 업적 카테고리 추가 여부. "대장장이와 오랜 친구가 되다" 등의 업적을 도입할 경우 `AchievementConditionType.NPCAffinityLevel` 추가 필요.
- [OPEN] 친밀도 단계 3(오랜 친구) 도달 시 재료 할인 외 추가 혜택 (예: 업그레이드 소요 시간 단축) 여부. 현재는 재료 구매 할인만 정의.
- [OPEN] NPCAffinityTracker를 별도 싱글턴으로 둘지, NPCManager의 서브 시스템으로 통합할지. 현재 설계는 독립 MonoBehaviour이나, NPCManager와의 결합도를 고려한 재검토 가능.
- [OPEN] ToolUpgradeUI에서 업그레이드 진행 중인 도구의 잔여 일수를 실시간 표시할지, 대장간 재방문 시에만 표시할지.

---

## Risks

- [RISK] **ScreenType enum 확장 동기화**: ToolUpgrade = 11 추가 시 `ui-architecture.md` 섹션 1.2 및 섹션 1.2의 상태 전이 규칙을 동시에 업데이트해야 한다. Screen별 구현 요약 표(섹션 4.4)에도 행 추가 필요.
- [RISK] **XPSource enum 확장 동기화**: ToolUpgrade 추가 시 `progression-architecture.md` 섹션 2.2 enum 및 섹션 2.3 GetExpForSource() switch 문을 동시에 업데이트해야 한다.
- [RISK] **MCP SO 배열 참조 설정**: BlacksmithNPCData의 greetingDialogues[], affinityDialogues[] 배열에 DialogueData SO 참조를 설정할 때 MCP 지원 여부 사전 검증 필요. 대안: string ID 기반 런타임 조회.
- [RISK] **친밀도 시스템 범용성**: NPCAffinityTracker를 범용으로 설계했으나, NPC별 친밀도 증가 방식/보상이 크게 다를 경우 전략 패턴 등 추가 추상화가 필요할 수 있다.

---

## Cross-references

- `docs/systems/npc-shop-architecture.md` (ARC-008) -- NPCData, NPCController, DialogueSystem, NPCEvents 기반 구조
- `docs/systems/tool-upgrade-architecture.md` (DES-007) -- ToolUpgradeSystem, ToolData SO, ToolEffectResolver, ToolUpgradeEvents
- `docs/systems/ui-architecture.md` (ARC-018) -- UIManager, ScreenBase, ScreenType enum, PopupQueue
- `docs/systems/progression-architecture.md` -- ProgressionManager, XPSource enum
- `docs/systems/achievement-architecture.md` -- AchievementManager, AchievementConditionType, ToolUpgradeCount
- `docs/content/npcs.md` (CON-003) -- 대장간 NPC 캐릭터 설정, 대화 예시, 영업 정보
- `docs/systems/tool-upgrade.md` -- 업그레이드 비용, 재료, 등급, 소요 일수 (canonical)
- `docs/systems/economy-architecture.md` -- ShopSystem, EconomyManager
- `docs/pipeline/data-pipeline.md` -- ToolData SO 에셋 구조
- `docs/systems/project-structure.md` -- 프로젝트 폴더 구조, 네임스페이스 규칙
- `docs/systems/save-load-architecture.md` -- SaveManager, ISaveable 인터페이스
