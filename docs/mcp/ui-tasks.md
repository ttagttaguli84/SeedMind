# UI 시스템 MCP 태스크 시퀀스 (ARC-022)

> UIManager 코어, Canvas 계층, HUD, ScreenBase 파생 Screen 프리팹, PopupQueue, NotificationManager, ToastUI 프리팹, UIEvents 이벤트 허브, Screen 등록, 통합 테스트를 MCP for Unity 태스크로 상세 정의  
> 작성: Claude Code (Opus) | 2026-04-07  
> Phase 1 | 문서 ID: ARC-022  
> 기반 문서: docs/systems/ui-architecture.md (ARC-018)

---

## 1. 개요

### 1.1 목적

이 문서는 `docs/systems/ui-architecture.md`(ARC-018) Part II에서 요약된 MCP 구현 계획(MCP-Step 1~6)을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 UI 시스템의 데이터 레이어(enum 4종, struct 2종), 시스템 레이어(스크립트 10종), Canvas 계층(6개 Canvas), HUD 배치, Screen 프리팹(8종), 알림 프리팹(ToastUI), Screen 등록 및 통합 테스트를 완성한다.

### 1.2 의존성

```
UI 시스템 MCP 태스크 의존 관계:
├── SeedMind.Core     (TimeManager, SaveManager, EventBus)
├── SeedMind.Farm     (FarmEvents -- OnCropHarvested, OnCropWithered)
├── SeedMind.Economy  (EconomyManager -- OnGoldChanged, OnSaleCompleted)
├── SeedMind.Player   (InventoryManager -- OnBackpackChanged, OnToolbarChanged)
├── SeedMind.Level    (ProgressionManager -- OnExpGained, OnLevelUp)
├── SeedMind.Quest    (QuestEvents -- OnQuestCompleted, OnObjectiveProgress)
├── SeedMind.Achievement (AchievementEvents -- OnAchievementUnlocked)
├── SeedMind.Building (BuildingEvents -- OnConstructionCompleted)
├── SeedMind.Processing (ProcessingEvents -- OnProcessingCompleted)
├── SeedMind.Tutorial (TutorialEvents -- OnTutorialStepStarted)
└── SeedMind.NPC      (NPCEvents, DialogueSystem)
```

(-> see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 1.3 완료된 태스크 의존성

| 문서 ID | 문서 | 완료 필수 | 핵심 결과물 |
|---------|------|----------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | 전체 | 폴더 구조, SCN_Farm 기본 계층 (MANAGERS, UI) |
| ARC-003 | `docs/mcp/farming-tasks.md` | 전체 | FarmGrid, FarmEvents |
| ARC-006 | `docs/mcp/inventory-tasks.md` | 전체 | InventoryManager, SlotUI |
| ARC-009 | `docs/mcp/npc-shop-tasks.md` | T-1, T-3 | DialogueUI, NPCEvents |
| ARC-011 | `docs/mcp/save-load-tasks.md` | 전체 | SaveManager, SaveEvents |
| ARC-012 | `docs/mcp/processing-tasks.md` | 전체 | ProcessingEvents |
| ARC-013 | `docs/mcp/quest-tasks.md` | 전체 | QuestEvents, QuestUI |
| ARC-017 | `docs/mcp/achievement-tasks.md` | 전체 | AchievementEvents, AchievementToastUI, AchievementPanel |
| BAL-002-MCP | `docs/mcp/progression-tasks.md` | 전체 | ProgressionManager, ProgressionEvents, LevelBarUI |
| - | `docs/mcp/tutorial-tasks.md` | 전체 | TutorialEvents, TutorialUI |
| - | `docs/mcp/time-season-tasks.md` | 전체 | TimeManager 이벤트 |

### 1.4 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `--- UI ---` (씬 계층 부모) | ARC-002 Phase B |
| `Canvas_Overlay` (기존 UI 루트) | ARC-002 Phase B |
| `InventoryManager` | ARC-006 |
| `SaveManager` | ARC-011 |
| `QuestManager` | ARC-013 |
| `AchievementManager` | ARC-017 |
| `ProgressionManager` | BAL-002 |
| `TimeManager` | time-season-tasks.md |
| `EconomyManager` | economy-architecture.md |
| `DialogueUI` (DialoguePanel) | ARC-009 T-3 |
| `AchievementToastUI` | ARC-017 |
| `AchievementPanel` | ARC-017 |
| `QuestUI` | ARC-013 |
| `LevelBarUI` | BAL-002 |
| `TutorialUI` | tutorial-tasks.md |
| `ProcessingUI` | ARC-012 |
| `InventoryUI` | ARC-006 |
| `SlotUI` 프리팹 | ARC-006 |

> **주의**: 기존 시스템에서 이미 생성된 UI 컴포넌트(DialogueUI, QuestUI, InventoryUI 등)는 이 태스크에서 재생성하지 않는다. 이 태스크는 UIManager 코어 인프라와 Canvas 계층 재구성, Screen 등록 연결, HUD/알림 시스템에 집중한다. 기존 UI 컴포넌트는 ScreenBase를 상속하도록 리팩터링하고 Canvas_Screen 하위로 이동시킨다.

### 1.5 태스크 맵

| 태스크 | 설명 | MCP 호출 수 |
|--------|------|------------|
| T-1 | 스크립트 생성 (enum, struct, 시스템 클래스, 추상 클래스) | ~22회 |
| T-2 | Canvas 계층 생성 및 설정 | 18회 |
| T-3 | UIManager 코어 GameObject 및 참조 설정 | ~17회 |
| T-4 | HUD 구조 배치 | 16회 |
| T-5 | Screen 프리팹 생성 및 Canvas_Screen 이동 | 20회 |
| T-6 | 알림 시스템 (NotificationManager + ToastUI 프리팹) | 14회 |
| T-7 | Screen 등록 및 참조 연결 | 12회 |
| T-8 | 통합 테스트 시퀀스 | 16회 |
| **합계** | | **~130회** |

[RISK] 기존 Canvas_Overlay에 배치된 UI 오브젝트들을 새 Canvas 계층으로 이동해야 한다. MCP `set_parent`로 기존 오브젝트의 부모를 변경할 때 RectTransform 앵커/오프셋이 초기화될 수 있으므로 이동 후 앵커 재설정이 필요하다.

### 1.6 스크립트 목록

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/UI/Data/ScreenType.cs` | `ScreenType` (enum) | `SeedMind.UI` | T-1 Phase 1 |
| S-02 | `Scripts/UI/Data/PopupPriority.cs` | `PopupPriority` (enum) | `SeedMind.UI` | T-1 Phase 1 |
| S-03 | `Scripts/UI/Data/UIInputMode.cs` | `UIInputMode` (enum) | `SeedMind.UI` | T-1 Phase 1 |
| S-04 | `Scripts/UI/Data/NotificationPriority.cs` | `NotificationPriority` (enum) | `SeedMind.UI` | T-1 Phase 1 |
| S-05 | `Scripts/UI/Data/NotificationData.cs` | `NotificationData` (struct) | `SeedMind.UI` | T-1 Phase 1 |
| S-06 | `Scripts/UI/Data/NotificationRequest.cs` | `NotificationRequest` (struct) | `SeedMind.UI` | T-1 Phase 1 |
| S-07 | `Scripts/UI/ScreenBase.cs` | `ScreenBase` (abstract MonoBehaviour) | `SeedMind.UI` | T-1 Phase 2 |
| S-08 | `Scripts/UI/PopupBase.cs` | `PopupBase` (abstract MonoBehaviour) | `SeedMind.UI` | T-1 Phase 2 |
| S-09 | `Scripts/UI/PopupQueue.cs` | `PopupQueue` (C# class) | `SeedMind.UI` | T-1 Phase 2 |
| S-10 | `Scripts/UI/UIEvents.cs` | `UIEvents` (static class) | `SeedMind.UI` | T-1 Phase 2 |
| S-11 | `Scripts/UI/UIManager.cs` | `UIManager` (MonoBehaviour Singleton) | `SeedMind.UI` | T-1 Phase 3 |
| S-12 | `Scripts/UI/NotificationManager.cs` | `NotificationManager` (MonoBehaviour Singleton) | `SeedMind.UI` | T-1 Phase 3 |
| S-13 | `Scripts/UI/ToastUI.cs` | `ToastUI` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 3 |
| S-14 | `Scripts/UI/HUDController.cs` | `HUDController` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 4 |
| S-15 | `Scripts/UI/TooltipManager.cs` | `TooltipManager` (MonoBehaviour Singleton) | `SeedMind.UI` | T-1 Phase 4 |
| S-16 | `Scripts/UI/TooltipUI.cs` | `TooltipUI` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 4 |
| S-17 | `Scripts/UI/MenuUI.cs` | `MenuUI` (ScreenBase 파생) | `SeedMind.UI` | T-1 Phase 4 |
| S-18 | `Scripts/UI/SaveLoadUI.cs` | `SaveLoadUI` (ScreenBase 파생) | `SeedMind.UI` | T-1 Phase 4 |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: S-01~S-06 -> S-07~S-10 -> S-11~S-13 -> S-14~S-18. 각 Phase 사이에 Unity 컴파일 대기(`execute_menu_item`)가 필요하다.

### 1.7 씬 GameObject 목록

| # | 오브젝트명 | 부모 | 컴포넌트 | 생성 태스크 |
|---|-----------|------|----------|-----------|
| G-01 | `UIRoot` | SCN_Farm (root) | UIManager | T-3-01 |
| G-02 | `NotificationMgr` | `UIRoot` | NotificationManager | T-3-03 |
| G-03 | `TooltipMgr` | `UIRoot` | TooltipManager | T-3-05 |
| G-04 | `Canvas_WorldSpace` | SCN_Farm (root) | Canvas (World Space) | T-2-01 |
| G-05 | `Canvas_HUD` | SCN_Farm (root) | Canvas (Overlay, Sort 0), HUDController | T-2-02 |
| G-06 | `Canvas_Screen` | SCN_Farm (root) | Canvas (Overlay, Sort 10) | T-2-03 |
| G-07 | `Canvas_Popup` | SCN_Farm (root) | Canvas (Overlay, Sort 20) | T-2-04 |
| G-08 | `Canvas_Notification` | SCN_Farm (root) | Canvas (Overlay, Sort 30) | T-2-05 |
| G-09 | `Canvas_Tutorial` | SCN_Farm (root) | Canvas (Overlay, Sort 40) | T-2-06 |
| G-10 | `TopBar` | `Canvas_HUD` | Image (배경) | T-4-01 |
| G-11 | `LevelBar` | `Canvas_HUD` | LevelBarUI | T-4-05 |
| G-12 | `ToolbarContainer` | `Canvas_HUD` | HorizontalLayoutGroup | T-4-07 |
| G-13 | `SaveIndicator` | `Canvas_HUD` | Image | T-4-09 |
| G-14 | `MenuScreen` | `Canvas_Screen` | MenuUI (ScreenBase) | T-5-05 |
| G-15 | `SaveLoadScreen` | `Canvas_Screen` | SaveLoadUI (ScreenBase) | T-5-07 |
| G-16 | `ToastContainer` | `Canvas_Notification` | VerticalLayoutGroup | T-6-01 |
| G-17 | `TooltipPanel` | `Canvas_Popup` | TooltipUI | T-3-07 |

> **중복 생성 금지**: 기존에 ARC-002에서 생성된 `Canvas_Overlay` 및 그 하위 UI 오브젝트는 삭제하지 않고, 새 Canvas 계층으로 `set_parent`를 통해 이동한다. 기존 UI 컴포넌트(InventoryUI, QuestUI, DialogueUI 등)는 Canvas_Screen 하위로 재배치한다.

---

## MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋 폴더 생성 | T-1 |
| `create_script` | C# 스크립트 파일 생성 | T-1 |
| `create_object` | 빈 GameObject 생성 | T-2, T-3, T-4, T-5, T-6 |
| `add_component` | MonoBehaviour/Canvas/CanvasGroup 등 부착 | T-2~T-7 전체 |
| `set_property` | 컴포넌트 프로퍼티 설정 (Sort Order, Anchor 등) | T-2~T-7 전체 |
| `set_parent` | 오브젝트 부모 설정, 기존 UI 이동 | T-2, T-4, T-5 |
| `save_scene` | 씬 저장 | T-2, T-3, T-5, T-7 |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | T-8 |
| `execute_menu_item` | 편집기 명령 실행 (컴파일 대기 등) | T-1 |
| `execute_method` | 런타임 메서드 호출 (테스트) | T-8 |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | T-8 |

---

## 2. T-1: 스크립트 생성

**목적**: UI 시스템에 필요한 모든 C# 스크립트를 생성한다.

**전제**: ARC-002(scene-setup-tasks.md) 완료. `Assets/_Project/Scripts/UI/` 폴더 존재. SeedMind.UI 네임스페이스 사용 가능.

---

### T-1 Phase 1: 데이터 구조 스크립트 (S-01 ~ S-06)

#### T-1-01: ScreenType enum (S-01)

- **입력**: ui-architecture.md 섹션 1.2 ScreenType 정의
- **액션**:

```
create_folder
  path: "Assets/_Project/Scripts/UI/Data"

create_script
  path: "Assets/_Project/Scripts/UI/Data/ScreenType.cs"
  content: |
    // S-01: 화면 유형 열거형
    // -> see docs/systems/ui-architecture.md 섹션 1.2
    namespace SeedMind.UI
    {
        public enum ScreenType
        {
            None        = 0,   // 화면 없음 (HUD만 표시)
            Farming     = 1,   // 농장 기본 화면 (= None과 동일)
            Inventory   = 2,   // 인벤토리 화면
            Shop        = 3,   // 상점 화면
            Quest       = 4,   // 퀘스트 목록 화면
            Achievement = 5,   // 업적 화면
            Menu        = 6,   // 메뉴/설정 화면
            SaveLoad    = 7,   // 세이브/로드 슬롯 화면
            Dialogue    = 8,   // NPC 대화 화면
            Processing  = 9,   // 가공소 화면
            Crafting    = 10,  // 크래프팅 화면 (향후 확장)
            ToolUpgrade = 11   // 대장간 도구 업그레이드 화면
        }
    }
```

- **검증**: 컴파일 에러 없음
- **MCP 호출**: 2회 (폴더 + 스크립트)

#### T-1-02: PopupPriority enum (S-02)

- **입력**: ui-architecture.md 섹션 1.5 PopupPriority 정의
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/Data/PopupPriority.cs"
  content: |
    // S-02: 팝업 우선순위 열거형
    // -> see docs/systems/ui-architecture.md 섹션 1.5
    namespace SeedMind.UI
    {
        public enum PopupPriority
        {
            Low      = 0,   // 일반 안내 (예: 힌트)
            Normal   = 1,   // 일반 팝업 (예: 확인 대화상자)
            High     = 2,   // 중요 팝업 (예: 레벨업, 퀘스트 완료)
            Critical = 3    // 최우선 (예: 자동저장 실패, 튜토리얼 강제)
        }
    }
```

- **검증**: 컴파일 에러 없음
- **MCP 호출**: 1회

#### T-1-03: UIInputMode enum (S-03)

- **입력**: ui-architecture.md 섹션 1.6 UIInputMode 정의
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/Data/UIInputMode.cs"
  content: |
    // S-03: UI 입력 모드 열거형
    // -> see docs/systems/ui-architecture.md 섹션 1.6
    namespace SeedMind.UI
    {
        public enum UIInputMode
        {
            Gameplay    = 0,   // 이동/도구 사용/상호작용 활성
            UIScreen    = 1,   // 이동 차단, UI 커서 활성, Esc로 닫기
            Dialogue    = 2,   // 모든 입력 차단, 대화 진행 키만 허용
            Popup       = 3    // UI Screen 위 팝업, Screen 조작 차단
        }
    }
```

- **검증**: 컴파일 에러 없음
- **MCP 호출**: 1회

#### T-1-04: NotificationPriority enum (S-04)

- **입력**: ui-architecture.md 섹션 3.1 NotificationPriority 정의
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/Data/NotificationPriority.cs"
  content: |
    // S-04: 토스트 알림 우선순위 열거형
    // -> see docs/systems/ui-architecture.md 섹션 3.1
    namespace SeedMind.UI
    {
        public enum NotificationPriority
        {
            Low      = 0,   // 일반 정보 (예: "씨앗을 심었습니다")
            Normal   = 1,   // 일반 성과 (예: "감자 수확 x3")
            High     = 2,   // 중요 성과 (예: 퀘스트 완료, 레벨업)
            Critical = 3    // 긴급 알림 (예: 작물 고사 경고, 저장 실패)
        }
    }
```

- **검증**: 컴파일 에러 없음
- **MCP 호출**: 1회

#### T-1-05: NotificationData struct (S-05)

- **입력**: ui-architecture.md 섹션 3.3 NotificationData 정의
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/Data/NotificationData.cs"
  content: |
    // S-05: 알림 데이터 구조체
    // -> see docs/systems/ui-architecture.md 섹션 3.3
    using UnityEngine;

    namespace SeedMind.UI
    {
        public struct NotificationData
        {
            public string Message;                  // 표시할 텍스트
            public NotificationPriority Priority;   // 우선순위
            public Sprite Icon;                     // null이면 기본 아이콘
            public float Duration;                  // 0이면 우선순위별 기본값
            public Color Color;                     // 배경색 힌트 (선택)
        }
    }
```

- **검증**: `NotificationPriority` 참조 가능 (S-04 의존)
- **MCP 호출**: 1회

#### T-1-06: NotificationRequest struct (S-06)

- **입력**: ui-architecture.md 섹션 3.3 NotificationRequest 정의
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/Data/NotificationRequest.cs"
  content: |
    // S-06: 알림 요청 구조체 (큐 내부 사용)
    // -> see docs/systems/ui-architecture.md 섹션 3.3
    namespace SeedMind.UI
    {
        public struct NotificationRequest
        {
            public NotificationData Data;           // 알림 데이터
            public float Timestamp;                 // Time.unscaledTime
            public int CompareKey;                  // Priority * 10000 - Timestamp (PQ 정렬키)
        }
    }
```

- **검증**: `NotificationData` 참조 가능 (S-05 의존)
- **MCP 호출**: 1회
- **Phase 1 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

### T-1 Phase 2: 추상 클래스 및 유틸리티 (S-07 ~ S-10)

#### T-1-07: ScreenBase 추상 클래스 (S-07)

- **입력**: ui-architecture.md 섹션 1.4 ScreenBase 클래스 다이어그램
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/ScreenBase.cs"
  content: |
    // S-07: Screen 추상 기반 클래스 (Template Method 패턴)
    // -> see docs/systems/ui-architecture.md 섹션 1.4
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    namespace SeedMind.UI
    {
        /// <summary>
        /// 모든 Screen의 Open/Close 생명주기를 통일하는 추상 기반 클래스.
        /// 파생 클래스: InventoryUI, ShopUI, QuestUI, AchievementPanel,
        ///              MenuUI, SaveLoadUI, DialogueUI, ProcessingUI
        /// </summary>
        public abstract class ScreenBase : MonoBehaviour
        {
            [SerializeField] protected CanvasGroup _canvasGroup;
            [SerializeField] protected Selectable _firstSelected;
            [SerializeField] protected ScreenType _screenType;
            [SerializeField] protected bool _pauseGameTime;
            [SerializeField] protected float _fadeInDuration = 0.15f;
                // -> see docs/systems/ui-architecture.md 섹션 1.4
            [SerializeField] protected float _fadeOutDuration = 0.1f;
                // -> see docs/systems/ui-architecture.md 섹션 1.4

            public ScreenType ScreenType => _screenType;
            public bool IsOpen { get; protected set; }
            public bool PausesGameTime => _pauseGameTime;

            // --- 생명주기 (sealed) ---
            // Open(): IEnumerator
            //   OnBeforeOpen() -> Fade In -> OnAfterOpen() -> UIEvents.RaiseScreenOpened
            // Close(): IEnumerator
            //   OnBeforeClose() -> Fade Out -> OnAfterClose() -> UIEvents.RaiseScreenClosed

            // --- 파생 클래스 오버라이드 ---
            // OnBeforeOpen(): virtual void
            // OnAfterOpen(): virtual void
            // OnBeforeClose(): virtual void
            // OnAfterClose(): virtual void

            // --- 유틸리티 ---
            // SetInteractable(bool): void
            // SetBlocksRaycasts(bool): void
        }
    }
```

- **검증**: `ScreenType`, `UIEvents` 참조 가능 (S-01, S-10 의존 -- S-10과 동시 생성)
- **MCP 호출**: 1회

#### T-1-08: PopupBase 추상 클래스 (S-08)

- **입력**: ui-architecture.md 섹션 1.5 PopupBase 클래스 다이어그램
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/PopupBase.cs"
  content: |
    // S-08: Popup 추상 기반 클래스
    // -> see docs/systems/ui-architecture.md 섹션 1.5
    using System;
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    namespace SeedMind.UI
    {
        /// <summary>
        /// 모든 Popup의 Show/Hide 생명주기를 통일하는 추상 기반 클래스.
        /// Screen 위에 중첩 표시되며, PopupQueue에 의해 우선순위 관리.
        /// </summary>
        public abstract class PopupBase : MonoBehaviour
        {
            [SerializeField] protected CanvasGroup _canvasGroup;
            [SerializeField] protected Image _backgroundDimmer;
            [SerializeField] protected bool _closeOnBackgroundClick = true;
            [SerializeField] protected float _fadeInDuration = 0.2f;
                // -> see docs/systems/ui-architecture.md 섹션 1.5
            [SerializeField] protected float _fadeOutDuration = 0.15f;
                // -> see docs/systems/ui-architecture.md 섹션 1.5

            public Action OnConfirm;
            public Action OnCancel;

            // --- 생명주기 (sealed) ---
            // Show(): IEnumerator
            //   OnBeforeShow() -> Dim + Fade In -> OnAfterShow() -> UIEvents.RaisePopupShown
            // Hide(): IEnumerator
            //   OnBeforeHide() -> Fade Out + Dim Off -> OnAfterHide() -> UIEvents.RaisePopupHidden

            // --- 파생 클래스 오버라이드 ---
            // OnBeforeShow(): virtual void
            // OnAfterShow(): virtual void
            // OnBeforeHide(): virtual void
            // OnAfterHide(): virtual void
        }
    }
```

- **검증**: 컴파일 에러 없음
- **MCP 호출**: 1회

#### T-1-09: PopupQueue 유틸리티 클래스 (S-09)

- **입력**: ui-architecture.md 섹션 1.5 PopupQueue 클래스 다이어그램
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/PopupQueue.cs"
  content: |
    // S-09: 팝업 우선순위 큐
    // -> see docs/systems/ui-architecture.md 섹션 1.5
    using System.Collections.Generic;

    namespace SeedMind.UI
    {
        /// <summary>
        /// PopupPriority 기반 정렬 큐. UIManager 내부에서 사용.
        /// </summary>
        public class PopupQueue
        {
            private SortedList<PopupPriority, Queue<PopupRequest>> _queue
                = new SortedList<PopupPriority, Queue<PopupRequest>>();
            private bool _isProcessing;

            // Enqueue(PopupBase popup, PopupPriority priority): void
            // Dequeue(): PopupRequest?
            // Peek(): PopupRequest?
            // Clear(): void
            // Count: int
            // IsEmpty: bool
        }

        public struct PopupRequest
        {
            public PopupBase Popup;
            public PopupPriority Priority;
            public float Timestamp;    // Time.unscaledTime
        }
    }
```

- **검증**: `PopupBase`, `PopupPriority` 참조 가능 (S-02, S-08 의존)
- **MCP 호출**: 1회

#### T-1-10: UIEvents 정적 이벤트 허브 (S-10)

- **입력**: ui-architecture.md 섹션 4.1 UIEvents 클래스
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/UIEvents.cs"
  content: |
    // S-10: UI 정적 이벤트 허브
    // -> see docs/systems/ui-architecture.md 섹션 4.1
    using System;

    namespace SeedMind.UI
    {
        /// <summary>
        /// UI 시스템의 외부 발행 이벤트.
        /// 다른 시스템이 UI 상태 변화를 감지하기 위해 구독한다.
        /// </summary>
        public static class UIEvents
        {
            // --- Screen 상태 ---
            public static event Action<ScreenType> OnScreenOpened;
            public static event Action<ScreenType> OnScreenClosed;

            // --- Popup 상태 ---
            public static event Action<PopupBase> OnPopupShown;
            public static event Action<PopupBase> OnPopupHidden;

            // --- 입력 모드 ---
            public static event Action<UIInputMode> OnInputModeChanged;

            // --- 알림 ---
            public static event Action<NotificationData> OnNotificationShown;
            public static event Action OnAllNotificationsCleared;

            // --- HUD 갱신 ---
            public static event Action OnHUDRefreshRequested;

            // --- Raise 메서드 ---
            public static void RaiseScreenOpened(ScreenType t)
                => OnScreenOpened?.Invoke(t);
            public static void RaiseScreenClosed(ScreenType t)
                => OnScreenClosed?.Invoke(t);
            public static void RaisePopupShown(PopupBase p)
                => OnPopupShown?.Invoke(p);
            public static void RaisePopupHidden(PopupBase p)
                => OnPopupHidden?.Invoke(p);
            public static void RaiseInputModeChanged(UIInputMode m)
                => OnInputModeChanged?.Invoke(m);
            public static void RaiseNotificationShown(NotificationData d)
                => OnNotificationShown?.Invoke(d);
            public static void RaiseAllNotificationsCleared()
                => OnAllNotificationsCleared?.Invoke();
            public static void RaiseHUDRefreshRequested()
                => OnHUDRefreshRequested?.Invoke();
        }
    }
```

- **검증**: `ScreenType`, `PopupBase`, `UIInputMode`, `NotificationData` 참조 가능
- **MCP 호출**: 1회
- **Phase 2 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

### T-1 Phase 3: 시스템 스크립트 (S-11 ~ S-13)

#### T-1-11: UIManager (S-11)

- **입력**: ui-architecture.md 섹션 1.3 UIManager 클래스 다이어그램
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/UIManager.cs"
  content: |
    // S-11: UI 시스템 중앙 관리자 (Singleton)
    // -> see docs/systems/ui-architecture.md 섹션 1.3
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    namespace SeedMind.UI
    {
        /// <summary>
        /// Screen FSM 관리, 화면 전환 중앙 제어, PopupQueue 조율.
        /// </summary>
        public class UIManager : MonoBehaviour
        {
            public static UIManager Instance { get; private set; }

            // --- 상태 ---
            private ScreenType _currentScreen = ScreenType.None;
            private ScreenType _previousScreen = ScreenType.None;
            private bool _isTransitioning;

            // --- Screen 레지스트리 ---
            private Dictionary<ScreenType, ScreenBase> _screens
                = new Dictionary<ScreenType, ScreenBase>();

            // --- Popup 큐 ---
            private PopupQueue _popupQueue = new PopupQueue();
            private PopupBase _activePopup;

            // --- 참조 ---
            [SerializeField] private HUDController _hudController;
            [SerializeField] private NotificationManager _notificationManager;

            // --- 읽기 전용 프로퍼티 ---
            public ScreenType CurrentScreen => _currentScreen;
            public bool IsScreenOpen => _currentScreen != ScreenType.None
                                     && _currentScreen != ScreenType.Farming;
            public bool IsTransitioning => _isTransitioning;
            public bool IsPopupActive => _activePopup != null;

            // --- 화면 전환 API ---
            // OpenScreen(ScreenType type): void
            // CloseCurrentScreen(): void
            // ToggleScreen(ScreenType type): void
            // ReturnToPreviousScreen(): void

            // --- 팝업 API ---
            // ShowPopup(PopupBase popup, PopupPriority priority): void
            // ClosePopup(): void
            // CloseAllPopups(): void

            // --- Screen 등록 ---
            // RegisterScreen(ScreenType type, ScreenBase screen): void
            // UnregisterScreen(ScreenType type): void

            // --- 유틸리티 ---
            // SetInputMode(UIInputMode mode): void
            // IsInputBlocked(): bool

            // --- 내부 메서드 ---
            // TransitionScreen(ScreenType from, ScreenType to): IEnumerator
            // ValidateTransition(ScreenType to): bool
            // UpdateInputMode(): void

            // --- 구독 (OnEnable/OnDisable) ---
            // InputActions.Player.Inventory -> HandleInventoryToggle
            // InputActions.Player.Menu -> HandleMenuToggle
            // InputActions.UI.Cancel -> HandleCancel
        }
    }
```

- **검증**: `ScreenBase`, `PopupBase`, `PopupQueue`, `HUDController`, `NotificationManager` 참조 가능
- **에러 처리**: `HUDController`/`NotificationManager` 미발견 시 Phase 4 스크립트 참조 대기
- **MCP 호출**: 1회

#### T-1-12: NotificationManager (S-12)

- **입력**: ui-architecture.md 섹션 3.2 NotificationManager 클래스 다이어그램
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/NotificationManager.cs"
  content: |
    // S-12: 토스트 알림 큐 관리자 (Singleton)
    // -> see docs/systems/ui-architecture.md 섹션 3.2
    using System.Collections.Generic;
    using UnityEngine;

    namespace SeedMind.UI
    {
        /// <summary>
        /// 토스트 알림의 우선순위 큐와 동시 표시 수를 제어한다.
        /// </summary>
        public class NotificationManager : MonoBehaviour
        {
            public static NotificationManager Instance { get; private set; }

            // --- 설정 ---
            [SerializeField] private int _maxVisibleToasts = 3;
                // -> see docs/systems/ui-architecture.md 섹션 3.2
            [SerializeField] private float _defaultDuration = 3.0f;
                // -> see docs/systems/ui-architecture.md 섹션 3.2
            [SerializeField] private float _criticalDuration = 5.0f;
                // -> see docs/systems/ui-architecture.md 섹션 3.2
            [SerializeField] private float _slideInDuration = 0.25f;
                // -> see docs/systems/ui-architecture.md 섹션 3.2
            [SerializeField] private float _slideOutDuration = 0.2f;
                // -> see docs/systems/ui-architecture.md 섹션 3.2
            [SerializeField] private float _verticalSpacing = 8f;
                // -> see docs/systems/ui-architecture.md 섹션 3.2

            // --- 참조 ---
            [SerializeField] private GameObject _toastPrefab;
            [SerializeField] private Transform _toastContainer;

            // --- 상태 ---
            // _pendingQueue: PriorityQueue<NotificationRequest>
            // _activeToasts: List<ToastUI>
            // _toastPool: ObjectPool<ToastUI>

            // --- API ---
            // ShowNotification(NotificationData data): void
            // ShowNotification(string msg, NotificationPriority p): void
            // ClearAll(): void

            // --- 내부 메서드 ---
            // ProcessQueue(): void    (Update에서 호출)
            // SpawnToast(NotificationRequest): ToastUI
            // RetireToast(ToastUI): void
            // RepositionActiveToasts(): void

            // --- 이벤트 구독 (OnEnable/OnDisable) ---
            // -> see docs/systems/ui-architecture.md 섹션 3.2 이벤트 구독 목록
            // QuestEvents, AchievementEvents, ProgressionEvents,
            // FarmEvents, EconomyEvents, SaveEvents,
            // BuildingEvents, ProcessingEvents, TutorialEvents
        }
    }
```

- **검증**: `ToastUI` 참조 가능 (S-13 의존 -- 동시 생성)
- **MCP 호출**: 1회

#### T-1-13: ToastUI (S-13)

- **입력**: ui-architecture.md 섹션 3.4 ToastUI 프리팹 구조
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/ToastUI.cs"
  content: |
    // S-13: 토스트 UI 프리팹 컴포넌트
    // -> see docs/systems/ui-architecture.md 섹션 3.4
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    namespace SeedMind.UI
    {
        /// <summary>
        /// 개별 토스트 알림 UI. NotificationManager가 풀링하여 관리.
        /// </summary>
        public class ToastUI : MonoBehaviour
        {
            [SerializeField] private Image _backgroundImage;
            [SerializeField] private Image _iconImage;
            [SerializeField] private TMP_Text _messageText;
            [SerializeField] private Image _progressBar;
            [SerializeField] private CanvasGroup _canvasGroup;

            private float _remainingTime;
            private bool _isActive;

            // Setup(NotificationData data): void
            // SlideIn(): IEnumerator
            // SlideOut(): IEnumerator
            // Tick(float deltaTime): bool   // false 반환 시 만료
            // ForceHide(): void             // 즉시 숨김 (ClearAll용)
        }
    }
```

- **검증**: `NotificationData` 참조 가능 (S-05 의존)
- **MCP 호출**: 1회
- **Phase 3 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

### T-1 Phase 4: HUD, 툴팁, 고유 Screen 스크립트 (S-14 ~ S-18)

#### T-1-14: HUDController (S-14)

- **입력**: ui-architecture.md 섹션 4.3 HUDController 클래스 다이어그램
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/HUDController.cs"
  content: |
    // S-14: 상시 HUD 관리자
    // -> see docs/systems/ui-architecture.md 섹션 4.3
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    namespace SeedMind.UI
    {
        /// <summary>
        /// 게임 플레이 중 항상 표시되는 상시 UI(시간, 골드, 툴바, 세이브 표시)를 관리.
        /// </summary>
        public class HUDController : MonoBehaviour
        {
            // --- 참조: 시간/날짜 ---
            [Header("시간/날짜")]
            [SerializeField] private TMP_Text _timeText;
            [SerializeField] private TMP_Text _dayText;
            [SerializeField] private Image _seasonIcon;
            [SerializeField] private Image _weatherIcon;

            // --- 참조: 경제 ---
            [Header("경제")]
            [SerializeField] private TMP_Text _goldText;

            // --- 참조: 진행 ---
            [Header("진행")]
            [SerializeField] private TMP_Text _levelText;
            [SerializeField] private Slider _expBar;

            // --- 참조: 툴바 ---
            [Header("툴바")]
            [SerializeField] private Transform _toolbarContainer;
                // SlotUI[] 참조, 슬롯 수: -> see docs/systems/inventory-architecture.md 섹션 1.2
            [SerializeField] private Image _selectedHighlight;

            // --- 참조: 시스템 상태 ---
            [Header("시스템")]
            [SerializeField] private GameObject _saveIndicator;

            // --- 이벤트 구독 (OnEnable/OnDisable) ---
            // -> see docs/systems/ui-architecture.md 섹션 4.3
            // TimeManager.OnHourChanged -> RefreshTimeDisplay
            // TimeManager.OnDayChanged -> RefreshDateDisplay
            // TimeManager.OnSeasonChanged -> RefreshSeasonIcon
            // EconomyManager.OnGoldChanged -> RefreshGoldDisplay
            // ProgressionManager.OnExpGained -> RefreshExpBar
            // ProgressionManager.OnLevelUp -> RefreshLevelDisplay
            // InventoryManager.OnToolbarChanged -> RefreshToolbar
            // InventoryManager.OnToolbarSelectionChanged -> RefreshToolSelect
            // SaveEvents.OnSaveStarted -> ShowSaveIndicator
            // SaveEvents.OnSaveCompleted -> HideSaveIndicator
            // UIEvents.OnHUDRefreshRequested -> RefreshAll
            // UIEvents.OnScreenOpened -> OnScreenOpened (HUD 축소)
            // UIEvents.OnScreenClosed -> OnScreenClosed (HUD 복원)
        }
    }
```

- **검증**: 컴파일 에러 없음
- **MCP 호출**: 1회

#### T-1-15: TooltipManager (S-15)

- **입력**: ui-architecture.md 섹션 1.1 TooltipManager
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/TooltipManager.cs"
  content: |
    // S-15: 마우스 오버 툴팁 관리자 (Singleton)
    // -> see docs/systems/ui-architecture.md 섹션 1.1
    using UnityEngine;

    namespace SeedMind.UI
    {
        /// <summary>
        /// 마우스 오버 시 아이템/버튼 등의 툴팁을 표시한다.
        /// </summary>
        public class TooltipManager : MonoBehaviour
        {
            public static TooltipManager Instance { get; private set; }

            [SerializeField] private TooltipUI _tooltipUI;

            // Show(string title, string description, Vector2 position): void
            // Hide(): void
        }
    }
```

- **검증**: `TooltipUI` 참조 가능 (S-16 의존 -- 동시 생성)
- **MCP 호출**: 1회

#### T-1-16: TooltipUI (S-16)

- **입력**: ui-architecture.md 섹션 6 파일 배치
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/TooltipUI.cs"
  content: |
    // S-16: 툴팁 패널 컴포넌트
    // -> see docs/systems/ui-architecture.md 섹션 1.1
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    namespace SeedMind.UI
    {
        /// <summary>
        /// 마우스 근처에 표시되는 툴팁 패널.
        /// </summary>
        public class TooltipUI : MonoBehaviour
        {
            [SerializeField] private Image _backgroundImage;
            [SerializeField] private TMP_Text _titleText;
            [SerializeField] private TMP_Text _descriptionText;
            [SerializeField] private CanvasGroup _canvasGroup;
            [SerializeField] private RectTransform _rectTransform;

            // Setup(string title, string description): void
            // SetPosition(Vector2 screenPos): void
            // Show(): void
            // Hide(): void
        }
    }
```

- **검증**: 컴파일 에러 없음
- **MCP 호출**: 1회

#### T-1-17: MenuUI (S-17)

- **입력**: ui-architecture.md 섹션 4.4 Screen별 구현 요약
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/MenuUI.cs"
  content: |
    // S-17: 메뉴/설정 화면 (ScreenBase 파생)
    // -> see docs/systems/ui-architecture.md 섹션 4.4
    using UnityEngine;
    using UnityEngine.UI;

    namespace SeedMind.UI
    {
        /// <summary>
        /// Esc 키로 열리는 메뉴 화면. 게임 시간 일시 정지.
        /// 하위: ResumeButton, SaveButton, LoadButton, SettingsButton, QuitButton
        /// </summary>
        public class MenuUI : ScreenBase
        {
            [Header("메뉴 버튼")]
            [SerializeField] private Button _resumeButton;
            [SerializeField] private Button _saveButton;
            [SerializeField] private Button _loadButton;
            [SerializeField] private Button _settingsButton;
            [SerializeField] private Button _quitButton;

            // OnBeforeOpen(): 게임 시간 정지
            // OnAfterClose(): 게임 시간 재개
            // HandleResume(): UIManager.CloseCurrentScreen()
            // HandleSave(): SaveManager.Save()
            // HandleLoad(): UIManager.OpenScreen(SaveLoad)
            // HandleSettings(): (향후 확장)
            // HandleQuit(): 확인 팝업 표시
        }
    }
```

- **검증**: `ScreenBase` 참조 가능 (S-07 의존)
- **MCP 호출**: 1회

#### T-1-18: SaveLoadUI (S-18)

- **입력**: ui-architecture.md 섹션 4.4, save-load-architecture.md
- **액션**:

```
create_script
  path: "Assets/_Project/Scripts/UI/SaveLoadUI.cs"
  content: |
    // S-18: 세이브/로드 슬롯 화면 (ScreenBase 파생)
    // -> see docs/systems/ui-architecture.md 섹션 4.4
    // -> see docs/systems/save-load-architecture.md for 슬롯 구조
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    namespace SeedMind.UI
    {
        /// <summary>
        /// Menu에서 진입하는 세이브/로드 슬롯 화면.
        /// 슬롯 수: -> see docs/systems/save-load-architecture.md
        /// </summary>
        public class SaveLoadUI : ScreenBase
        {
            [Header("슬롯")]
            [SerializeField] private Transform _slotContainer;
            [SerializeField] private Button _backButton;

            // OnBeforeOpen(): SaveManager에서 슬롯 데이터 로드, 슬롯 UI 갱신
            // HandleSlotClick(int slotIndex): 세이브 또는 로드 실행
            // HandleBack(): UIManager.OpenScreen(Menu)
        }
    }
```

- **검증**: `ScreenBase` 참조 가능 (S-07 의존)
- **MCP 호출**: 1회
- **Phase 4 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

## 3. T-2: Canvas 계층 생성 및 설정

**목적**: 6개 Canvas를 생성하고 Sort Order, Render Mode, CanvasScaler, GraphicRaycaster를 설정한다.

**전제**: ARC-002 완료. SCN_Farm.unity 존재. T-1 Phase 4 컴파일 완료.

---

### T-2-01: Canvas_WorldSpace 생성

- **목적**: 작물/시설 위 World Space UI 컨테이너
- **액션**:

```
create_object
  name: "Canvas_WorldSpace"
  scene: "SCN_Farm"

add_component
  target: "Canvas_WorldSpace"
  component: "Canvas"

set_property
  target: "Canvas_WorldSpace"
  component: "Canvas"
  property: "renderMode"
  value: 2    // WorldSpace

add_component
  target: "Canvas_WorldSpace"
  component: "GraphicRaycaster"
```

- **검증**: Canvas_WorldSpace가 씬 루트에 존재, Render Mode = World Space
- **MCP 호출**: 4회

### T-2-02: Canvas_HUD 생성

- **목적**: 상시 HUD (시간, 골드, 미니맵, 툴바)
- **액션**:

```
create_object
  name: "Canvas_HUD"
  scene: "SCN_Farm"

add_component
  target: "Canvas_HUD"
  component: "Canvas"

set_property
  target: "Canvas_HUD"
  component: "Canvas"
  property: "renderMode"
  value: 0    // Screen Space - Overlay

set_property
  target: "Canvas_HUD"
  component: "Canvas"
  property: "sortingOrder"
  value: 0    // -> see docs/systems/ui-architecture.md 섹션 2.2

add_component
  target: "Canvas_HUD"
  component: "CanvasScaler"

set_property
  target: "Canvas_HUD"
  component: "CanvasScaler"
  property: "uiScaleMode"
  value: 1    // Scale With Screen Size

set_property
  target: "Canvas_HUD"
  component: "CanvasScaler"
  property: "referenceResolution"
  value: { x: 1920, y: 1080 }

add_component
  target: "Canvas_HUD"
  component: "GraphicRaycaster"
```

- **검증**: Sort Order = 0, 해상도 1920x1080
- **MCP 호출**: 8회 (이하 Canvas 동일 패턴)

### T-2-03: Canvas_Screen 생성

- **목적**: 전체 화면 UI (인벤토리, 상점, 퀘스트 등)
- **액션**: Canvas_HUD와 동일 패턴, Sort Order = 10
  - `set_property` sortingOrder = 10
  - CanvasGroup 추가 (alpha=0, interactable=false, blocksRaycasts=false)

```
add_component
  target: "Canvas_Screen"
  component: "CanvasGroup"

set_property
  target: "Canvas_Screen"
  component: "CanvasGroup"
  property: "alpha"
  value: 0

set_property
  target: "Canvas_Screen"
  component: "CanvasGroup"
  property: "interactable"
  value: false

set_property
  target: "Canvas_Screen"
  component: "CanvasGroup"
  property: "blocksRaycasts"
  value: false
```

- **검증**: Sort Order = 10, 초기 상태 비활성
- **MCP 호출**: (Canvas 기본 ~6회) + (CanvasGroup 4회) = ~10회

### T-2-04: Canvas_Popup 생성

- **목적**: 팝업 레이어 (확인 대화상자, 레벨업)
- **액션**: Canvas 기본 패턴, Sort Order = 20
- **MCP 호출**: ~6회

### T-2-05: Canvas_Notification 생성

- **목적**: 토스트 알림 레이어 (클릭 패스스루)
- **액션**: Canvas 기본 패턴, Sort Order = 30, **GraphicRaycaster 미추가** (클릭 패스스루)
- **검증**: GraphicRaycaster가 없어야 한다 (알림은 클릭 차단하지 않음)
- **MCP 호출**: ~5회

### T-2-06: Canvas_Tutorial 생성

- **목적**: 튜토리얼 오버레이 (최상위)
- **액션**: Canvas 기본 패턴, Sort Order = 40
- **MCP 호출**: ~6회

### T-2-07: 기존 Canvas_Overlay 내 UI 이동

- **목적**: ARC-002에서 생성된 Canvas_Overlay 하위의 기존 UI 오브젝트를 새 Canvas 계층으로 이동
- **전제**: T-2-01~T-2-06 완료
- **액션**:

```
# 기존 Screen 계열 UI를 Canvas_Screen으로 이동
set_parent
  target: "InventoryScreen"    // (기존 InventoryUI가 배치된 오브젝트명)
  parent: "Canvas_Screen"

set_parent
  target: "QuestScreen"
  parent: "Canvas_Screen"

set_parent
  target: "DialoguePanel"
  parent: "Canvas_Screen"

set_parent
  target: "ProcessingScreen"
  parent: "Canvas_Screen"

set_parent
  target: "AchievementScreen"
  parent: "Canvas_Screen"

# AchievementToastUI를 Canvas_Notification으로 이동
set_parent
  target: "AchievementToastUI"
  parent: "Canvas_Notification"

# TutorialUI를 Canvas_Tutorial로 이동
set_parent
  target: "TutorialUI"
  parent: "Canvas_Tutorial"
```

- **검증**: 각 UI 오브젝트가 올바른 Canvas 하위에 위치하는지 확인
- **에러 처리**: 기존 오브젝트명이 다를 경우 실제 오브젝트명으로 조정
- **MCP 호출**: ~7회

[RISK] 기존 오브젝트를 `set_parent`로 이동할 때 RectTransform 앵커/오프셋이 초기화될 수 있다. 이동 후 각 오브젝트의 앵커를 재설정해야 할 수 있다.

### T-2-08: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

---

## 4. T-3: UIManager 코어 GameObject 생성

**목적**: UIRoot, NotificationManager, TooltipManager GameObject를 생성하고 컴포넌트를 부착한다.

**전제**: T-1 전체 컴파일 완료. T-2 Canvas 계층 생성 완료.

---

### T-3-01: UIRoot 생성

```
create_object
  name: "UIRoot"
  scene: "SCN_Farm"

add_component
  target: "UIRoot"
  component: "SeedMind.UI.UIManager"
```

- **검증**: UIRoot에 UIManager 컴포넌트 부착 확인
- **MCP 호출**: 2회

### T-3-02: UIRoot를 MANAGERS 하위로 이동

```
set_parent
  target: "UIRoot"
  parent: "--- MANAGERS ---"
```

- **MCP 호출**: 1회

### T-3-03: NotificationMgr 생성

```
create_object
  name: "NotificationMgr"

add_component
  target: "NotificationMgr"
  component: "SeedMind.UI.NotificationManager"

set_parent
  target: "NotificationMgr"
  parent: "UIRoot"
```

- **검증**: NotificationMgr이 UIRoot 하위에 위치
- **MCP 호출**: 3회

### T-3-04, T-3-06: (예비 번호 — 추후 확장 시 사용)

### T-3-05: TooltipMgr 생성

```
create_object
  name: "TooltipMgr"

add_component
  target: "TooltipMgr"
  component: "SeedMind.UI.TooltipManager"

set_parent
  target: "TooltipMgr"
  parent: "UIRoot"
```

- **MCP 호출**: 3회

### T-3-07: TooltipPanel 생성 (Canvas_Popup 하위)

```
create_object
  name: "TooltipPanel"

add_component
  target: "TooltipPanel"
  component: "SeedMind.UI.TooltipUI"

add_component
  target: "TooltipPanel"
  component: "CanvasGroup"

set_property
  target: "TooltipPanel"
  component: "CanvasGroup"
  property: "alpha"
  value: 0

set_parent
  target: "TooltipPanel"
  parent: "Canvas_Popup"
```

- **검증**: TooltipPanel이 Canvas_Popup 하위에 초기 비활성 상태
- **MCP 호출**: 4회

### T-3-08: UIManager 참조 연결

```
set_property
  target: "UIRoot"
  component: "SeedMind.UI.UIManager"
  property: "_hudController"
  value: "Canvas_HUD"    // HUDController가 부착된 오브젝트

set_property
  target: "UIRoot"
  component: "SeedMind.UI.UIManager"
  property: "_notificationManager"
  value: "NotificationMgr"

set_property
  target: "TooltipMgr"
  component: "SeedMind.UI.TooltipManager"
  property: "_tooltipUI"
  value: "TooltipPanel"
```

- **검증**: UIManager Inspector에서 _hudController, _notificationManager 참조가 설정되었는지 확인
- **MCP 호출**: 3회

### T-3-09: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

---

## 5. T-4: HUD 구조 배치

**목적**: Canvas_HUD 하위에 TopBar, LevelBar, ToolbarContainer, SaveIndicator를 배치한다.

**전제**: T-2-02(Canvas_HUD) 완료. T-1 Phase 4(HUDController) 컴파일 완료.

---

### T-4-01: TopBar 패널 생성

```
create_object
  name: "TopBar"

set_parent
  target: "TopBar"
  parent: "Canvas_HUD"

add_component
  target: "TopBar"
  component: "Image"

# Anchor: Top Stretch
set_property
  target: "TopBar"
  component: "RectTransform"
  property: "anchorMin"
  value: { x: 0, y: 1 }

set_property
  target: "TopBar"
  component: "RectTransform"
  property: "anchorMax"
  value: { x: 1, y: 1 }

set_property
  target: "TopBar"
  component: "RectTransform"
  property: "sizeDelta"
  value: { x: 0, y: 48 }
```

- **MCP 호출**: 5회

### T-4-02: TopBar 자식 요소 생성

```
# TimeText
create_object name: "TimeText"
set_parent target: "TimeText" parent: "TopBar"
add_component target: "TimeText" component: "TMPro.TextMeshProUGUI"

# DayText
create_object name: "DayText"
set_parent target: "DayText" parent: "TopBar"
add_component target: "DayText" component: "TMPro.TextMeshProUGUI"

# SeasonIcon
create_object name: "SeasonIcon"
set_parent target: "SeasonIcon" parent: "TopBar"
add_component target: "SeasonIcon" component: "Image"

# WeatherIcon
create_object name: "WeatherIcon"
set_parent target: "WeatherIcon" parent: "TopBar"
add_component target: "WeatherIcon" component: "Image"

# GoldText
create_object name: "GoldText"
set_parent target: "GoldText" parent: "TopBar"
add_component target: "GoldText" component: "TMPro.TextMeshProUGUI"
```

- **MCP 호출**: 15회 (5개 요소 x 3회)

### T-4-03, T-4-04: (예비 번호 — 추후 TopBar 추가 요소 확장 시 사용)

### T-4-05: LevelBar 패널 생성

```
create_object name: "LevelBar"
set_parent target: "LevelBar" parent: "Canvas_HUD"

# Anchor: Top-Left
set_property target: "LevelBar" component: "RectTransform"
  property: "anchorMin" value: { x: 0, y: 1 }
set_property target: "LevelBar" component: "RectTransform"
  property: "anchorMax" value: { x: 0, y: 1 }

# 자식: LevelText, ExpBarSlider
create_object name: "LevelText"
set_parent target: "LevelText" parent: "LevelBar"
add_component target: "LevelText" component: "TMPro.TextMeshProUGUI"

create_object name: "ExpBarSlider"
set_parent target: "ExpBarSlider" parent: "LevelBar"
add_component target: "ExpBarSlider" component: "Slider"
```

- **검증**: LevelBarUI 컴포넌트는 기존 progression-tasks.md에서 생성됨. 여기서는 GameObject 구조만 배치
- **MCP 호출**: ~8회

### T-4-06: (예비 번호 — 추후 LevelBar 추가 요소 확장 시 사용)

### T-4-07: ToolbarContainer 생성

```
create_object name: "ToolbarContainer"
set_parent target: "ToolbarContainer" parent: "Canvas_HUD"

# Anchor: Bottom-Center
set_property target: "ToolbarContainer" component: "RectTransform"
  property: "anchorMin" value: { x: 0.5, y: 0 }
set_property target: "ToolbarContainer" component: "RectTransform"
  property: "anchorMax" value: { x: 0.5, y: 0 }

add_component target: "ToolbarContainer" component: "HorizontalLayoutGroup"

set_property target: "ToolbarContainer" component: "HorizontalLayoutGroup"
  property: "spacing" value: 4

# SelectedHighlight
create_object name: "SelectedHighlight"
set_parent target: "SelectedHighlight" parent: "ToolbarContainer"
add_component target: "SelectedHighlight" component: "Image"
```

- **검증**: HorizontalLayoutGroup이 부착되고 spacing 설정됨
- **참고**: SlotUI 프리팹 인스턴스 수는 -> see `docs/systems/inventory-architecture.md` 섹션 1.2
- **MCP 호출**: ~8회

### T-4-08: (예비 번호 — 추후 ToolbarContainer 추가 요소 확장 시 사용)

### T-4-09: SaveIndicator 생성

```
create_object name: "SaveIndicator"
set_parent target: "SaveIndicator" parent: "Canvas_HUD"

# Anchor: Top-Right
set_property target: "SaveIndicator" component: "RectTransform"
  property: "anchorMin" value: { x: 1, y: 1 }
set_property target: "SaveIndicator" component: "RectTransform"
  property: "anchorMax" value: { x: 1, y: 1 }

add_component target: "SaveIndicator" component: "Image"

# 초기 비활성
set_property target: "SaveIndicator" property: "activeSelf" value: false
```

- **검증**: 초기 상태 비활성(SetActive false)
- **MCP 호출**: ~5회

### T-4-10: HUDController 참조 연결

```
set_property target: "Canvas_HUD" component: "SeedMind.UI.HUDController"
  property: "_timeText" value: "TimeText"
set_property target: "Canvas_HUD" component: "SeedMind.UI.HUDController"
  property: "_dayText" value: "DayText"
set_property target: "Canvas_HUD" component: "SeedMind.UI.HUDController"
  property: "_seasonIcon" value: "SeasonIcon"
set_property target: "Canvas_HUD" component: "SeedMind.UI.HUDController"
  property: "_weatherIcon" value: "WeatherIcon"
set_property target: "Canvas_HUD" component: "SeedMind.UI.HUDController"
  property: "_goldText" value: "GoldText"
set_property target: "Canvas_HUD" component: "SeedMind.UI.HUDController"
  property: "_levelText" value: "LevelText"
set_property target: "Canvas_HUD" component: "SeedMind.UI.HUDController"
  property: "_expBar" value: "ExpBarSlider"
set_property target: "Canvas_HUD" component: "SeedMind.UI.HUDController"
  property: "_toolbarContainer" value: "ToolbarContainer"
set_property target: "Canvas_HUD" component: "SeedMind.UI.HUDController"
  property: "_selectedHighlight" value: "SelectedHighlight"
set_property target: "Canvas_HUD" component: "SeedMind.UI.HUDController"
  property: "_saveIndicator" value: "SaveIndicator"
```

- **MCP 호출**: 10회

---

## 6. T-5: Screen 프리팹 생성 및 Canvas_Screen 이동

**목적**: UIManager에 등록할 Screen 프리팹을 생성한다. 기존 시스템에서 이미 생성된 Screen(InventoryUI, QuestUI 등)은 T-2-07에서 Canvas_Screen으로 이동 완료. 여기서는 신규 Screen(MenuScreen, SaveLoadScreen)과 아직 프리팹화되지 않은 Screen을 처리한다.

**전제**: T-2 Canvas 계층 완료. T-1 Phase 4 컴파일 완료.

---

### T-5-01 ~ T-5-04: 기존 Screen ScreenBase 확장

기존 시스템에서 생성된 UI(InventoryUI, QuestUI, DialogueUI, ProcessingUI, AchievementPanel, ShopUI)는 ScreenBase를 상속하도록 각 아키텍처 문서의 MCP 태스크에서 리팩터링한다. 이 문서에서는 해당 Screen이 Canvas_Screen 하위에 올바르게 배치되었는지만 검증한다.

- **검증**: Canvas_Screen 하위에 다음 오브젝트가 존재하는지 확인
  - InventoryScreen (InventoryUI)
  - ShopScreen (ShopUI)
  - QuestScreen (QuestUI)
  - AchievementScreen (AchievementPanel)
  - DialogueScreen (DialogueUI)
  - ProcessingScreen (ProcessingUI)
- **MCP 호출**: 0회 (검증만)

### T-5-05: MenuScreen 생성

```
create_object name: "MenuScreen"

set_parent target: "MenuScreen" parent: "Canvas_Screen"

add_component target: "MenuScreen" component: "SeedMind.UI.MenuUI"

add_component target: "MenuScreen" component: "CanvasGroup"

set_property target: "MenuScreen" component: "CanvasGroup"
  property: "alpha" value: 0
set_property target: "MenuScreen" component: "CanvasGroup"
  property: "interactable" value: false
set_property target: "MenuScreen" component: "CanvasGroup"
  property: "blocksRaycasts" value: false

# Anchor: Stretch (전체 화면)
set_property target: "MenuScreen" component: "RectTransform"
  property: "anchorMin" value: { x: 0, y: 0 }
set_property target: "MenuScreen" component: "RectTransform"
  property: "anchorMax" value: { x: 1, y: 1 }

# MenuUI._screenType = Menu
set_property target: "MenuScreen" component: "SeedMind.UI.MenuUI"
  property: "_screenType" value: 6    // ScreenType.Menu
set_property target: "MenuScreen" component: "SeedMind.UI.MenuUI"
  property: "_pauseGameTime" value: true
```

- **MCP 호출**: ~10회

### T-5-06: MenuScreen 버튼 배치

```
# 자식 버튼 생성
create_object name: "ResumeButton"
set_parent target: "ResumeButton" parent: "MenuScreen"
add_component target: "ResumeButton" component: "Button"
add_component target: "ResumeButton" component: "Image"

create_object name: "SaveButton"
set_parent target: "SaveButton" parent: "MenuScreen"
add_component target: "SaveButton" component: "Button"
add_component target: "SaveButton" component: "Image"

create_object name: "LoadButton"
set_parent target: "LoadButton" parent: "MenuScreen"
add_component target: "LoadButton" component: "Button"
add_component target: "LoadButton" component: "Image"

create_object name: "SettingsButton"
set_parent target: "SettingsButton" parent: "MenuScreen"
add_component target: "SettingsButton" component: "Button"
add_component target: "SettingsButton" component: "Image"

create_object name: "QuitButton"
set_parent target: "QuitButton" parent: "MenuScreen"
add_component target: "QuitButton" component: "Button"
add_component target: "QuitButton" component: "Image"
```

- **MCP 호출**: ~20회 (5버튼 x 4회)

### T-5-07: SaveLoadScreen 생성

```
create_object name: "SaveLoadScreen"

set_parent target: "SaveLoadScreen" parent: "Canvas_Screen"

add_component target: "SaveLoadScreen" component: "SeedMind.UI.SaveLoadUI"

add_component target: "SaveLoadScreen" component: "CanvasGroup"

set_property target: "SaveLoadScreen" component: "CanvasGroup"
  property: "alpha" value: 0
set_property target: "SaveLoadScreen" component: "CanvasGroup"
  property: "interactable" value: false
set_property target: "SaveLoadScreen" component: "CanvasGroup"
  property: "blocksRaycasts" value: false

# SaveLoadUI._screenType = SaveLoad
set_property target: "SaveLoadScreen" component: "SeedMind.UI.SaveLoadUI"
  property: "_screenType" value: 7    // ScreenType.SaveLoad
```

- **MCP 호출**: ~8회

### T-5-08: SaveLoadScreen 슬롯 배치

```
create_object name: "SlotContainer"
set_parent target: "SlotContainer" parent: "SaveLoadScreen"
add_component target: "SlotContainer" component: "VerticalLayoutGroup"

# 슬롯 3개 (-> see docs/systems/save-load-architecture.md for 슬롯 수)
create_object name: "SaveSlot_1"
set_parent target: "SaveSlot_1" parent: "SlotContainer"
add_component target: "SaveSlot_1" component: "Button"

create_object name: "SaveSlot_2"
set_parent target: "SaveSlot_2" parent: "SlotContainer"
add_component target: "SaveSlot_2" component: "Button"

create_object name: "SaveSlot_3"
set_parent target: "SaveSlot_3" parent: "SlotContainer"
add_component target: "SaveSlot_3" component: "Button"

create_object name: "BackButton"
set_parent target: "BackButton" parent: "SaveLoadScreen"
add_component target: "BackButton" component: "Button"
```

- **MCP 호출**: ~13회

### T-5-09: SaveLoadUI 참조 연결

```
set_property target: "SaveLoadScreen" component: "SeedMind.UI.SaveLoadUI"
  property: "_slotContainer" value: "SlotContainer"
set_property target: "SaveLoadScreen" component: "SeedMind.UI.SaveLoadUI"
  property: "_backButton" value: "BackButton"
```

- **MCP 호출**: 2회

### T-5-10: MenuUI 참조 연결

```
set_property target: "MenuScreen" component: "SeedMind.UI.MenuUI"
  property: "_resumeButton" value: "ResumeButton"
set_property target: "MenuScreen" component: "SeedMind.UI.MenuUI"
  property: "_saveButton" value: "SaveButton"
set_property target: "MenuScreen" component: "SeedMind.UI.MenuUI"
  property: "_loadButton" value: "LoadButton"
set_property target: "MenuScreen" component: "SeedMind.UI.MenuUI"
  property: "_settingsButton" value: "SettingsButton"
set_property target: "MenuScreen" component: "SeedMind.UI.MenuUI"
  property: "_quitButton" value: "QuitButton"
```

- **MCP 호출**: 5회

### T-5-11: Screen 프리팹화

```
# 프리팹 폴더 생성 (이미 존재할 수 있음)
create_folder
  path: "Assets/_Project/Prefabs/UI"

# MenuScreen 프리팹화
# (MCP 도구의 프리팹 생성 API에 따라 실행)
# -> PFB_Screen_Menu.prefab

# SaveLoadScreen 프리팹화
# -> PFB_Screen_SaveLoad.prefab
```

- **MCP 호출**: ~3회

[RISK] MCP for Unity의 프리팹 생성/등록 API가 `create_prefab` 또는 유사 명령을 지원하는지 사전 확인 필요. 미지원 시 수동 프리팹화 또는 Editor 스크립트 우회 필요.

### T-5-12: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

---

## 7. T-6: 알림 시스템 (NotificationManager + ToastUI 프리팹)

**목적**: Canvas_Notification 하위에 ToastContainer를 배치하고, ToastUI 프리팹을 생성하며, NotificationManager에 참조를 연결한다.

**전제**: T-2-05(Canvas_Notification) 완료. T-1 Phase 3(NotificationManager, ToastUI) 컴파일 완료.

---

### T-6-01: ToastContainer 생성

```
create_object name: "ToastContainer"

set_parent target: "ToastContainer" parent: "Canvas_Notification"

# Anchor: 우상단
set_property target: "ToastContainer" component: "RectTransform"
  property: "anchorMin" value: { x: 1, y: 1 }
set_property target: "ToastContainer" component: "RectTransform"
  property: "anchorMax" value: { x: 1, y: 1 }
set_property target: "ToastContainer" component: "RectTransform"
  property: "pivot" value: { x: 1, y: 1 }

add_component target: "ToastContainer" component: "VerticalLayoutGroup"

set_property target: "ToastContainer" component: "VerticalLayoutGroup"
  property: "spacing"
  value: 8    // -> see docs/systems/ui-architecture.md 섹션 3.2 _verticalSpacing

set_property target: "ToastContainer" component: "VerticalLayoutGroup"
  property: "childAlignment"
  value: 1    // UpperRight
```

- **MCP 호출**: 8회

### T-6-02: ToastUI 프리팹 구성

```
create_object name: "PFB_Toast"

# 배경 Image (RoundedRect)
add_component target: "PFB_Toast" component: "Image"

# CanvasGroup
add_component target: "PFB_Toast" component: "CanvasGroup"

# ToastUI 스크립트
add_component target: "PFB_Toast" component: "SeedMind.UI.ToastUI"

# 자식: IconImage
create_object name: "IconImage"
set_parent target: "IconImage" parent: "PFB_Toast"
add_component target: "IconImage" component: "Image"
set_property target: "IconImage" component: "RectTransform"
  property: "sizeDelta" value: { x: 48, y: 48 }

# 자식: MessageText
create_object name: "MessageText"
set_parent target: "MessageText" parent: "PFB_Toast"
add_component target: "MessageText" component: "TMPro.TextMeshProUGUI"

# 자식: ProgressBar
create_object name: "ProgressBar"
set_parent target: "ProgressBar" parent: "PFB_Toast"
add_component target: "ProgressBar" component: "Image"
set_property target: "ProgressBar" component: "Image"
  property: "type" value: 3    // Filled
```

- **MCP 호출**: ~14회

### T-6-03: ToastUI 참조 연결

```
set_property target: "PFB_Toast" component: "SeedMind.UI.ToastUI"
  property: "_backgroundImage" value: "PFB_Toast"
set_property target: "PFB_Toast" component: "SeedMind.UI.ToastUI"
  property: "_iconImage" value: "IconImage"
set_property target: "PFB_Toast" component: "SeedMind.UI.ToastUI"
  property: "_messageText" value: "MessageText"
set_property target: "PFB_Toast" component: "SeedMind.UI.ToastUI"
  property: "_progressBar" value: "ProgressBar"
set_property target: "PFB_Toast" component: "SeedMind.UI.ToastUI"
  property: "_canvasGroup" value: "PFB_Toast"
```

- **MCP 호출**: 5회

### T-6-04: ToastUI 프리팹화 및 NotificationManager 연결

```
# PFB_Toast를 프리팹으로 저장
# -> Assets/_Project/Prefabs/UI/PFB_Toast.prefab

# NotificationManager 참조 설정
set_property target: "NotificationMgr" component: "SeedMind.UI.NotificationManager"
  property: "_toastPrefab" value: "PFB_Toast"
set_property target: "NotificationMgr" component: "SeedMind.UI.NotificationManager"
  property: "_toastContainer" value: "ToastContainer"
```

- **MCP 호출**: ~3회

### T-6-05: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

---

## 8. T-7: Screen 등록 및 참조 연결

**목적**: 모든 ScreenBase 파생 Screen이 UIManager에 등록되도록 설정한다.

**전제**: T-3(UIManager), T-5(Screen 프리팹) 완료.

---

### T-7-01: 등록 방식 설명

UIManager의 Screen 등록은 **런타임 RegisterScreen() 패턴**을 사용한다. 각 ScreenBase 파생 클래스가 Awake()에서 `UIManager.Instance.RegisterScreen(_screenType, this)`를 호출하므로, Inspector 참조 설정이 불필요하다.

**검증 대상**: 각 ScreenBase 파생 클래스의 `_screenType` 필드가 올바르게 설정되어 있는지 확인한다.

### T-7-02: ScreenType 필드 설정 확인/수정

기존 시스템에서 생성된 Screen의 `_screenType` 필드를 확인하고, 미설정된 경우 설정한다.

```
# InventoryUI._screenType = Inventory (2)
set_property target: "InventoryScreen"
  component: "SeedMind.UI.InventoryUI"    // 또는 해당 ScreenBase 파생 클래스명
  property: "_screenType" value: 2

# ShopUI._screenType = Shop (3)
set_property target: "ShopScreen"
  component: "ShopUI" property: "_screenType" value: 3

# QuestUI._screenType = Quest (4)
set_property target: "QuestScreen"
  component: "QuestUI" property: "_screenType" value: 4

# AchievementPanel._screenType = Achievement (5)
set_property target: "AchievementScreen"
  component: "AchievementPanel" property: "_screenType" value: 5

# MenuUI._screenType = Menu (6) -- T-5-05에서 설정 완료

# SaveLoadUI._screenType = SaveLoad (7) -- T-5-07에서 설정 완료

# DialogueUI._screenType = Dialogue (8)
set_property target: "DialogueScreen"
  component: "DialogueUI" property: "_screenType" value: 8

# ProcessingUI._screenType = Processing (9)
set_property target: "ProcessingScreen"
  component: "ProcessingUI" property: "_screenType" value: 9
```

- **에러 처리**: 기존 Screen 클래스가 아직 ScreenBase를 상속하지 않는 경우, 해당 시스템의 MCP 태스크에서 리팩터링이 필요하다. 여기서는 set_property를 시도하고 실패 시 로그로 기록한다.
- **MCP 호출**: ~6회

### T-7-03: CanvasGroup 확인

모든 Screen에 CanvasGroup이 부착되어 있는지 확인한다. 누락된 경우 추가한다.

```
# 기존 Screen에 CanvasGroup 누락 시 추가
add_component target: "InventoryScreen" component: "CanvasGroup"
add_component target: "ShopScreen" component: "CanvasGroup"
add_component target: "QuestScreen" component: "CanvasGroup"
add_component target: "AchievementScreen" component: "CanvasGroup"
add_component target: "DialogueScreen" component: "CanvasGroup"
add_component target: "ProcessingScreen" component: "CanvasGroup"
```

- **에러 처리**: 이미 CanvasGroup이 있으면 MCP가 경고를 반환할 수 있음. 무시해도 무방.
- **MCP 호출**: ~6회

### T-7-04: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

---

## 9. T-8: 통합 테스트 시퀀스

**목적**: UI 시스템의 핵심 기능(Screen 전환, 팝업 큐, 알림 큐)이 정상 동작하는지 Play Mode에서 검증한다.

**전제**: T-1~T-7 전체 완료.

---

### T-8-01: Play Mode 진입

```
enter_play_mode
```

- **MCP 호출**: 1회

### T-8-02: Screen FSM 테스트

```
# 테스트 1: Tab 키 -> InventoryScreen 열기
execute_method
  target: "UIRoot"
  component: "SeedMind.UI.UIManager"
  method: "ToggleScreen"
  args: [2]    // ScreenType.Inventory

get_console_logs
  filter: "ScreenOpened"
# 기대: "[UIEvents] ScreenOpened: Inventory"

# 테스트 2: Tab 키 -> InventoryScreen 닫기
execute_method
  target: "UIRoot"
  component: "SeedMind.UI.UIManager"
  method: "ToggleScreen"
  args: [2]

get_console_logs
  filter: "ScreenClosed"
# 기대: "[UIEvents] ScreenClosed: Inventory"

# 테스트 3: Esc -> MenuScreen 열기
execute_method
  target: "UIRoot"
  component: "SeedMind.UI.UIManager"
  method: "OpenScreen"
  args: [6]    // ScreenType.Menu

get_console_logs
  filter: "ScreenOpened"
# 기대: "[UIEvents] ScreenOpened: Menu"

# 테스트 4: 이전 Screen 자동 Close 확인
execute_method
  target: "UIRoot"
  component: "SeedMind.UI.UIManager"
  method: "OpenScreen"
  args: [4]    // ScreenType.Quest (Menu에서 직접 전환 불가 -- ValidateTransition 실패 기대)

get_console_logs
  filter: "Validate"
# 기대: "[UIManager] ValidateTransition failed: Quest (from Menu)"
```

- **MCP 호출**: 8회

### T-8-03: NotificationManager 테스트

```
# 테스트 5: 알림 1개 발송
execute_method
  target: "NotificationMgr"
  component: "SeedMind.UI.NotificationManager"
  method: "ShowNotification"
  args: ["테스트 알림 1", 1]    // Normal priority

get_console_logs
  filter: "Notification"
# 기대: "[NotificationManager] Toast shown: 테스트 알림 1"

# 테스트 6: 4개 연속 발송 -> 동시 최대 3개 표시
execute_method method: "ShowNotification" args: ["알림 A", 0]
execute_method method: "ShowNotification" args: ["알림 B", 1]
execute_method method: "ShowNotification" args: ["알림 C", 2]
execute_method method: "ShowNotification" args: ["알림 D", 3]

get_console_logs
  filter: "active"
# 기대: "[NotificationManager] ActiveToastCount: 3, PendingCount: 1"
```

- **MCP 호출**: 6회

### T-8-04: PopupQueue 테스트

```
# 테스트 7: 팝업 표시 요청 (Low, High 순서)
# PopupBase 테스트용 인스턴스가 필요하므로, 
# 런타임에서 간이 팝업 생성 후 테스트

execute_method
  target: "UIRoot"
  component: "SeedMind.UI.UIManager"
  method: "ShowPopup"
  args: ["TestPopup_Low", 0]    // Low priority

execute_method
  target: "UIRoot"
  component: "SeedMind.UI.UIManager"
  method: "ShowPopup"
  args: ["TestPopup_High", 2]   // High priority

get_console_logs
  filter: "Popup"
# 기대: High 팝업이 먼저 표시됨
```

- **MCP 호출**: 3회

[RISK] 팝업 테스트는 PopupBase 파생 인스턴스가 씬에 존재해야 한다. 테스트용 간이 팝업 프리팹이 없으면 이 테스트는 건너뛰고 추후 개별 시스템 통합 시 검증한다.

### T-8-05: HUD 갱신 테스트

```
# 테스트 8: HUD 전체 갱신 요청
execute_method
  target: "UIRoot"  // or UIEvents를 직접 호출
  component: "SeedMind.UI.UIEvents"
  method: "RaiseHUDRefreshRequested"

get_console_logs
  filter: "HUD"
# 기대: "[HUDController] RefreshAll called"
```

- **MCP 호출**: 2회

### T-8-06: Play Mode 종료

```
exit_play_mode
```

- **MCP 호출**: 1회

### T-8-07: 최종 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

---

## Cross-references

- `docs/systems/ui-architecture.md` (ARC-018) -- 본 태스크의 기반 아키텍처 문서
- `docs/systems/ui-system.md` (DES-011) -- UI/UX 디자인 canonical (HUD 배치, 키 바인딩, 알림 우선순위)
- `docs/architecture.md` -- 마스터 기술 아키텍처, 입력 시스템 (섹션 4.4)
- `docs/systems/project-structure.md` -- 폴더 구조, 네임스페이스 규칙, 의존성 매트릭스
- `docs/mcp/scene-setup-tasks.md` (ARC-002) -- 폴더 구조, Canvas_Overlay 원본 생성
- `docs/mcp/inventory-tasks.md` (ARC-006) -- InventoryUI, SlotUI 생성
- `docs/mcp/npc-shop-tasks.md` (ARC-009) -- DialogueUI 생성
- `docs/mcp/save-load-tasks.md` (ARC-011) -- SaveManager, SaveEvents
- `docs/mcp/processing-tasks.md` (ARC-012) -- ProcessingUI 생성
- `docs/mcp/quest-tasks.md` (ARC-013) -- QuestUI 생성
- `docs/mcp/achievement-tasks.md` (ARC-017) -- AchievementPanel, AchievementToastUI 생성
- `docs/mcp/blacksmith-tasks.md` (ARC-020) -- ToolUpgradeScreen, BlacksmithNPC UI
- `docs/mcp/progression-tasks.md` (BAL-002) -- LevelBarUI 생성
- `docs/mcp/tutorial-tasks.md` -- TutorialUI 생성
- `docs/systems/inventory-architecture.md` -- InventoryUI 구조 (섹션 6)
- `docs/systems/quest-architecture.md` (ARC-013) -- QuestUI 구조 (섹션 8)
- `docs/systems/achievement-architecture.md` (ARC-017) -- AchievementPanel, AchievementToastUI (섹션 8)
- `docs/systems/save-load-architecture.md` (ARC-011) -- SaveLoadUI 슬롯 구조
- `docs/systems/progression-architecture.md` (BAL-002) -- LevelBarUI (섹션 7.2)
- `docs/systems/tutorial-architecture.md` -- TutorialUI (섹션 6)

---

## Open Questions

- [OPEN] **Canvas_Overlay 폐기 시점**: 기존 Canvas_Overlay에서 새 Canvas 계층으로 모든 UI를 이동한 후, Canvas_Overlay를 삭제할지 빈 상태로 유지할지. 다른 시스템이 Canvas_Overlay를 참조하고 있을 수 있으므로, 전체 시스템 이전 완료 후 삭제하는 것이 안전하다.

- [OPEN] **프리팹 생성 MCP API**: MCP for Unity가 `create_prefab` 또는 동등한 프리팹 생성 API를 제공하는지 확인 필요. 미제공 시 Editor 스크립트를 통한 프리팹화 우회가 필요하며, T-5-11과 T-6-04의 프리팹화 단계가 영향을 받는다.

- [OPEN] **기존 Screen 클래스 ScreenBase 상속 리팩터링**: InventoryUI, QuestUI, DialogueUI 등 기존 Screen 클래스가 아직 ScreenBase를 상속하지 않는다면, 각 시스템의 스크립트를 수정해야 한다. 이 리팩터링을 각 시스템의 MCP 태스크에서 수행할지, 별도 리팩터링 태스크(ARC-023)를 만들지 결정 필요.

- [OPEN] **DOTween 의존성**: 토스트 슬라이드 인/아웃, 팝업 페이드 등에 DOTween을 사용할지 코루틴으로 처리할지. (-> see ui-architecture.md Open Questions)

---

## Risks

- [RISK] **기존 UI 이동 시 RectTransform 초기화**: `set_parent`로 기존 UI 오브젝트를 새 Canvas로 이동할 때 RectTransform의 앵커/오프셋/피봇이 초기화될 수 있다. T-2-07 실행 후 각 오브젝트의 앵커를 재설정해야 할 수 있으며, 이로 인해 추가 MCP 호출이 ~20회 증가할 수 있다.

- [RISK] **static event 구독 누수**: UIEvents, QuestEvents 등 정적 이벤트를 UI 컴포넌트가 구독한 후 씬 전환 시 해제하지 않으면 메모리 누수 및 NullReferenceException 발생. 모든 ScreenBase/HUDController는 OnDisable에서 반드시 구독 해제해야 한다.

- [RISK] **Canvas 5개 분리 성능**: Screen Space Overlay Canvas가 5개(HUD, Screen, Popup, Notification, Tutorial)일 경우 Canvas.BuildBatch 호출 빈도 증가. 실제 프로파일링 후 Canvas 병합이 필요할 수 있다.

- [RISK] **MCP set_property의 enum 값 설정**: ScreenType enum 값을 정수로 설정할 때 MCP가 올바르게 해석하는지 사전 검증 필요. 문자열("Menu")로 설정해야 할 수 있다.

- [RISK] **ScreenBase 상속 리팩터링 범위**: 기존 시스템(inventory, quest, dialogue, processing, achievement)의 UI 클래스가 ScreenBase를 상속하도록 리팩터링하면 기존 MCP 태스크와의 호환성이 깨질 수 있다. 각 시스템의 스크립트를 `create_script`로 덮어쓸 때 기존 로직이 소실되지 않도록 주의.

- [RISK] **PopupQueue 테스트 인프라 부재**: T-8-04의 팝업 테스트는 PopupBase 파생 인스턴스가 필요하나, 이 태스크 시퀀스에서 테스트용 팝업 프리팹을 별도 생성하지 않는다. 추후 개별 시스템(레벨업 팝업, 확인 대화상자 등)이 구현된 후 통합 검증해야 한다.

---

*이 문서는 Claude Code가 docs/systems/ui-architecture.md(ARC-018)의 Part II를 기반으로 독립 MCP 태스크 시퀀스로 확장하여 자율적으로 작성했습니다.*
