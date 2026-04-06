# UI 시스템 기술 아키텍처

> UIManager Screen FSM, ScreenBase 추상 클래스, PopupQueue 시스템, Canvas 계층 구조, NotificationManager 알림 큐, 이벤트-UI 연동 설계, MCP 구현 태스크 요약  
> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: ARC-018

---

## Context

이 문서는 SeedMind의 UI 시스템에 대한 **기술 아키텍처 문서**이다. UI 시스템은 게임 내 모든 화면(Screen), 팝업(Popup), 알림(Notification), HUD를 관리하는 최상위 프레젠테이션 레이어로, 의존성 매트릭스에서 최상층에 위치한다 (-> see `docs/systems/project-structure.md` 섹션 3.2).

**설계 목표**:
- Screen FSM으로 화면 전환을 중앙 관리하여 동시에 여러 화면이 열리는 버그를 방지
- ScreenBase 추상 클래스로 모든 화면의 Open/Close 생명주기를 통일
- PopupQueue로 팝업의 우선순위 기반 표시를 보장 (동시 팝업 충돌 방지)
- NotificationManager로 토스트 알림의 우선순위 큐와 동시 표시 수를 제어
- 이벤트 기반 연동으로 각 Manager와 UI 사이의 결합을 최소화
- World Space HUD와 Screen Space UI를 Canvas 계층으로 명확히 분리

**본 문서가 canonical인 데이터**:
- UIManager 클래스 설계, Screen FSM 상태 전이 규칙
- ScreenBase 추상 클래스 구조, 생명주기 API
- PopupQueue 시스템 설계 (우선순위, 큐 처리 로직)
- Canvas 계층 구조 및 정렬 우선순위 (Order in Layer)
- NotificationManager 클래스 설계, 알림 우선순위 enum
- UIEvents 정적 이벤트 허브 설계
- UI 관련 파일 배치 및 네임스페이스 확장

**본 문서가 canonical이 아닌 데이터 (참조만)**:

| 데이터 종류 | 참조처 |
|------------|--------|
| InventoryUI 슬롯 구조, 드래그/드롭 로직 | `docs/systems/inventory-architecture.md` 섹션 6 |
| QuestUI 패널/목표 표시 구조 | `docs/systems/quest-architecture.md` 섹션 8 |
| AchievementToastUI, AchievementPanel 구조 | `docs/systems/achievement-architecture.md` 섹션 8 |
| 세이브/로드 슬롯 UI | `docs/systems/save-load-architecture.md` |
| LevelBarUI XP 바 구조 | `docs/systems/progression-architecture.md` 섹션 7.2 |
| TutorialUI 오버레이 구조 | `docs/systems/tutorial-architecture.md` 섹션 6 |
| ProcessingUI 가공소 패널 구조 | `docs/systems/processing-architecture.md` 섹션 7 |
| DialogueUI 대화 패널 구조 | `docs/systems/npc-shop-architecture.md` |
| 프로젝트 폴더 구조, 네임스페이스 규칙 | `docs/systems/project-structure.md` |
| Input System 액션 매핑 | `docs/architecture.md` 섹션 4.4 |
| SaveManager API, ISaveable 인터페이스 | `docs/systems/save-load-architecture.md` (ARC-011) |

---

# Part I -- 아키텍처 설계

---

## 1. UIManager 클래스 설계

### 1.1 클래스 책임 요약

| 클래스 | 유형 | 네임스페이스 | 책임 |
|--------|------|-------------|------|
| **UIManager** | MonoBehaviour (Singleton) | `SeedMind.UI` | Screen FSM 관리, 화면 전환 중앙 제어, PopupQueue 조율 |
| **ScreenBase** | 추상 MonoBehaviour | `SeedMind.UI` | 모든 Screen의 Open/Close 생명주기 추상화 |
| **PopupBase** | 추상 MonoBehaviour | `SeedMind.UI` | 모든 Popup의 Show/Hide 생명주기 추상화 |
| **NotificationManager** | MonoBehaviour (Singleton) | `SeedMind.UI` | 토스트 알림 큐 관리, 우선순위 기반 표시 |
| **UIEvents** | static class | `SeedMind.UI` | UI 관련 정적 이벤트 허브 |
| **HUDController** | MonoBehaviour | `SeedMind.UI` | 상시 표시 HUD (시간, 골드, 도구, 미니맵) |
| **TooltipManager** | MonoBehaviour (Singleton) | `SeedMind.UI` | 마우스 오버 툴팁 표시 |

### 1.2 Screen FSM (유한 상태 기계)

UIManager는 게임 내 주요 화면을 FSM으로 관리한다. 동시에 하나의 Screen만 활성화되며, 화면 전환 시 이전 Screen을 Close하고 새 Screen을 Open한다.

```csharp
// illustrative
namespace SeedMind.UI
{
    public enum ScreenType
    {
        None        = 0,   // 화면 없음 (HUD만 표시)
        Farming     = 1,   // 농장 기본 화면 (= None과 동일, HUD 모드)
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

**상태 전이 규칙**:

```
[Farming/None] ──(Tab)──▶ [Inventory]
      │                        │
      │                    (Tab/Esc)
      │                        │
      ◀────────────────────────┘
      │
      ├──(상점 NPC 상호작용)──▶ [Shop] ──(Esc/닫기)──▶ [Farming]
      │
      ├──(J 키)──▶ [Quest] ──(Esc/J)──▶ [Farming]
      │
      ├──(Y 키)──▶ [Achievement] ──(Esc/Y)──▶ [Farming]
      │
      ├──(Esc)──▶ [Menu] ──(Esc)──▶ [Farming]
      │
      ├──(NPC 상호작용)──▶ [Dialogue] ──(대화 종료)──▶ [Farming]
      │                       │
      │                       └──(업그레이드 선택)──▶ [ToolUpgrade] ──(Esc/닫기)──▶ [Farming]
      │
      └──(가공소 상호작용)──▶ [Processing] ──(Esc/닫기)──▶ [Farming]

[Menu] ──(세이브/로드 버튼)──▶ [SaveLoad] ──(Esc/뒤로)──▶ [Menu]
```

**전이 제약 조건**:
- Farming 이외의 Screen에서 다른 Screen으로 직접 전환 불가 (반드시 Farming으로 복귀 후 전환)
- 단, Menu -> SaveLoad는 예외적으로 Menu 위에 중첩 허용
- Dialogue Screen 활성 시 다른 모든 입력(이동, 도구 사용) 차단

### 1.3 UIManager 클래스 다이어그램

```
┌──────────────────────────────────────────────────────────────────┐
│           UIManager (MonoBehaviour, Singleton)                     │
│──────────────────────────────────────────────────────────────────│
│  [상태]                                                           │
│  - _currentScreen: ScreenType                                    │
│  - _previousScreen: ScreenType                                   │
│  - _isTransitioning: bool                                        │
│                                                                  │
│  [Screen 레지스트리]                                               │
│  - _screens: Dictionary<ScreenType, ScreenBase>                  │
│                                                                  │
│  [Popup 큐]                                                       │
│  - _popupQueue: PopupQueue            (아래 섹션 1.5 참조)        │
│  - _activePopup: PopupBase                                       │
│                                                                  │
│  [참조]                                                           │
│  - _hudController: HUDController                                 │
│  - _notificationManager: NotificationManager                     │
│                                                                  │
│  [읽기 전용 프로퍼티]                                               │
│  + CurrentScreen: ScreenType                                     │
│  + IsScreenOpen: bool                  // _currentScreen != None │
│  + IsTransitioning: bool                                         │
│  + IsPopupActive: bool                                           │
│                                                                  │
│  [화면 전환 API]                                                   │
│  + OpenScreen(ScreenType type): void                             │
│  + CloseCurrentScreen(): void                                    │
│  + ToggleScreen(ScreenType type): void  // 열려 있으면 닫고,     │
│  │                                       // 닫혀 있으면 열기      │
│  + ReturnToPreviousScreen(): void                                │
│                                                                  │
│  [팝업 API]                                                       │
│  + ShowPopup(PopupBase popup, PopupPriority priority): void      │
│  + ClosePopup(): void                                            │
│  + CloseAllPopups(): void                                        │
│                                                                  │
│  [Screen 등록]                                                    │
│  + RegisterScreen(ScreenType type, ScreenBase screen): void      │
│  + UnregisterScreen(ScreenType type): void                       │
│                                                                  │
│  [유틸리티]                                                       │
│  + SetInputMode(UIInputMode mode): void   // 게임/UI/대화 모드   │
│  + IsInputBlocked(): bool                                        │
│                                                                  │
│  [구독]                                                           │
│  + OnEnable():                                                   │
│      InputActions.Player.Inventory += HandleInventoryToggle      │
│      InputActions.Player.Menu += HandleMenuToggle                │
│      InputActions.UI.Cancel += HandleCancel                      │
│  + OnDisable(): 구독 해제                                         │
│                                                                  │
│  [내부 메서드]                                                     │
│  - TransitionScreen(ScreenType from, ScreenType to): IEnumerator │
│  - ValidateTransition(ScreenType to): bool                       │
│  - UpdateInputMode(): void                                       │
└──────────────────────────────────────────────────────────────────┘
```

### 1.4 ScreenBase 추상 클래스

모든 Screen(Inventory, Shop, Quest, Achievement, Menu, SaveLoad, Dialogue, Processing)이 상속하는 기반 클래스.

```
┌──────────────────────────────────────────────────────────────────┐
│           ScreenBase (abstract MonoBehaviour)                      │
│──────────────────────────────────────────────────────────────────│
│  [참조]                                                           │
│  - _canvasGroup: CanvasGroup       (페이드 인/아웃용)             │
│  - _firstSelected: Selectable      (키보드/컨트롤러 포커스 시작점)│
│                                                                  │
│  [설정]                                                           │
│  - _screenType: ScreenType         (어떤 화면인지 식별)           │
│  - _pauseGameTime: bool            (이 화면 열릴 때 게임 시간 정지)│
│  - _fadeInDuration: float = 0.15f                                │
│  - _fadeOutDuration: float = 0.1f                                │
│                                                                  │
│  [읽기 전용 프로퍼티]                                               │
│  + ScreenType: ScreenType                                        │
│  + IsOpen: bool                                                  │
│  + PausesGameTime: bool                                          │
│                                                                  │
│  [생명주기 (Template Method 패턴)]                                  │
│  + Open(): IEnumerator       (sealed)                            │
│  │   ├── OnBeforeOpen()      (virtual, 파생 클래스 오버라이드)     │
│  │   ├── 페이드 인 (CanvasGroup.alpha 0→1)                       │
│  │   ├── OnAfterOpen()       (virtual)                           │
│  │   └── UIEvents.RaiseScreenOpened(_screenType)                 │
│  │                                                               │
│  + Close(): IEnumerator      (sealed)                            │
│  │   ├── OnBeforeClose()     (virtual)                           │
│  │   ├── 페이드 아웃 (CanvasGroup.alpha 1→0)                     │
│  │   ├── OnAfterClose()      (virtual)                           │
│  │   └── UIEvents.RaiseScreenClosed(_screenType)                 │
│  │                                                               │
│  # OnBeforeOpen(): virtual void     // 데이터 로드, 리프레시      │
│  # OnAfterOpen(): virtual void      // 포커스 설정, 애니메이션    │
│  # OnBeforeClose(): virtual void    // 정리, 데이터 반영          │
│  # OnAfterClose(): virtual void     // 최종 정리                  │
│                                                                  │
│  [유틸리티]                                                       │
│  # SetInteractable(bool): void      (CanvasGroup.interactable)  │
│  # SetBlocksRaycasts(bool): void                                │
└──────────────────────────────────────────────────────────────────┘
```

**생명주기 흐름**:
```
UIManager.OpenScreen(ScreenType.Inventory)
├── 1) ValidateTransition() — 전환 가능 여부 검증
├── 2) _isTransitioning = true
├── 3) 현재 Screen이 있으면: yield return _currentScreen.Close()
├── 4) yield return _screens[type].Open()
│      ├── OnBeforeOpen() — InventoryUI가 InventoryManager에서 슬롯 데이터 로드
│      ├── CanvasGroup alpha 0→1 (0.15초)
│      ├── OnAfterOpen() — 첫 번째 슬롯에 포커스
│      └── UIEvents.RaiseScreenOpened(ScreenType.Inventory)
├── 5) _previousScreen = _currentScreen
├── 6) _currentScreen = type
├── 7) UpdateInputMode() — UI 입력 모드로 전환
└── 8) _isTransitioning = false
```

### 1.5 PopupQueue 시스템

팝업은 Screen 위에 중첩되어 표시되는 모달 UI이다. 동시에 여러 팝업이 요청될 수 있으므로 우선순위 기반 큐로 관리한다.

```csharp
// illustrative
namespace SeedMind.UI
{
    public enum PopupPriority
    {
        Low      = 0,   // 일반 안내 (예: 힌트)
        Normal   = 1,   // 일반 팝업 (예: 확인 대화상자)
        High     = 2,   // 중요 팝업 (예: 레벨업, 퀘스트 완료)
        Critical = 3    // 최우선 (예: 자동저장 실패 경고, 튜토리얼 강제 팝업)
    }
}
```

```
┌──────────────────────────────────────────────────────────────────┐
│              PopupQueue (일반 C# 클래스)                           │
│──────────────────────────────────────────────────────────────────│
│  [상태]                                                           │
│  - _queue: SortedList<PopupPriority, Queue<PopupRequest>>        │
│  - _isProcessing: bool                                           │
│                                                                  │
│  [메서드]                                                         │
│  + Enqueue(PopupBase popup, PopupPriority priority): void        │
│  + Dequeue(): PopupRequest?           // 가장 높은 우선순위부터   │
│  + Peek(): PopupRequest?                                         │
│  + Clear(): void                                                 │
│  + Count: int                                                    │
│  + IsEmpty: bool                                                 │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│              PopupRequest (struct)                                 │
│──────────────────────────────────────────────────────────────────│
│  + Popup: PopupBase                                              │
│  + Priority: PopupPriority                                       │
│  + Timestamp: float                    // Time.unscaledTime       │
└──────────────────────────────────────────────────────────────────┘
```

**PopupBase 추상 클래스**:

```
┌──────────────────────────────────────────────────────────────────┐
│            PopupBase (abstract MonoBehaviour)                      │
│──────────────────────────────────────────────────────────────────│
│  [참조]                                                           │
│  - _canvasGroup: CanvasGroup                                     │
│  - _backgroundDimmer: Image          (반투명 배경)                │
│                                                                  │
│  [설정]                                                           │
│  - _closeOnBackgroundClick: bool = true                          │
│  - _fadeInDuration: float = 0.2f                                 │
│  - _fadeOutDuration: float = 0.15f                               │
│                                                                  │
│  [생명주기]                                                       │
│  + Show(): IEnumerator               (sealed)                    │
│  │   ├── OnBeforeShow()              (virtual)                   │
│  │   ├── 배경 딤 + 팝업 페이드 인                                 │
│  │   ├── OnAfterShow()               (virtual)                   │
│  │   └── UIEvents.RaisePopupShown(this)                          │
│  │                                                               │
│  + Hide(): IEnumerator               (sealed)                    │
│  │   ├── OnBeforeHide()              (virtual)                   │
│  │   ├── 팝업 페이드 아웃 + 배경 딤 해제                           │
│  │   ├── OnAfterHide()               (virtual)                   │
│  │   └── UIEvents.RaisePopupHidden(this)                         │
│  │                                                               │
│  [콜백]                                                           │
│  + OnConfirm: Action                  // 확인 버튼 콜백           │
│  + OnCancel: Action                   // 취소/닫기 콜백           │
└──────────────────────────────────────────────────────────────────┘
```

**팝업 처리 흐름**:
```
UIManager.ShowPopup(popup, PopupPriority.High)
├── 1) _popupQueue.Enqueue(popup, High)
├── 2) _activePopup이 없으면:
│      ├── request = _popupQueue.Dequeue()
│      ├── _activePopup = request.Popup
│      └── yield return _activePopup.Show()
├── 3) _activePopup이 있고 신규 우선순위가 더 높으면:
│      ├── 현재 팝업 일시 중단 (Hide 없이 비활성화)
│      ├── 신규 팝업 Show
│      └── 신규 팝업 종료 시 이전 팝업 재활성화
└── 4) 그 외: 큐에서 대기
```

### 1.6 UIInputMode

화면 상태에 따라 입력 처리 모드를 전환한다.

```csharp
// illustrative
namespace SeedMind.UI
{
    public enum UIInputMode
    {
        Gameplay    = 0,   // 이동/도구 사용/상호작용 활성, UI 커서 비활성
        UIScreen    = 1,   // 이동 차단, UI 커서 활성, Esc로 닫기
        Dialogue    = 2,   // 모든 입력 차단, 대화 진행 키만 허용
        Popup       = 3    // UI Screen 위 팝업, Screen 조작 차단
    }
}
```

---

## 2. CanvasGroup 계층 구조

### 2.1 Canvas 분리 전략

게임에서 사용하는 Canvas를 역할별로 3개로 분리한다. 각 Canvas는 독립된 정렬 순서를 가진다.

```
SCN_Farm.unity
│
├── Canvas_WorldSpace          # World Space — 작물/시설 위 표시
│   ├── Camera: Main Camera
│   ├── Render Mode: World Space
│   └── Sort Order: N/A (3D 공간 깊이 기준)
│
├── Canvas_HUD                 # Screen Space — 상시 HUD
│   ├── Render Mode: Screen Space - Overlay
│   ├── Sort Order: 0
│   └── Reference Resolution: 1920 x 1080
│
├── Canvas_Screen              # Screen Space — 전체 화면 UI
│   ├── Render Mode: Screen Space - Overlay
│   ├── Sort Order: 10
│   └── Reference Resolution: 1920 x 1080
│
├── Canvas_Popup               # Screen Space — 팝업 레이어
│   ├── Render Mode: Screen Space - Overlay
│   ├── Sort Order: 20
│   └── Reference Resolution: 1920 x 1080
│
├── Canvas_Notification        # Screen Space — 토스트 알림 레이어
│   ├── Render Mode: Screen Space - Overlay
│   ├── Sort Order: 30
│   └── Reference Resolution: 1920 x 1080
│
└── Canvas_Tutorial            # Screen Space — 튜토리얼 오버레이 (최상위)
    ├── Render Mode: Screen Space - Overlay
    ├── Sort Order: 40
    └── Reference Resolution: 1920 x 1080
```

### 2.2 Canvas 정렬 우선순위 (Order in Layer)

| Canvas | Sort Order | 역할 | Raycaster |
|--------|:----------:|------|:---------:|
| `Canvas_WorldSpace` | N/A | 작물 이름, 성장 게이지, 시설 상태 | Physics Raycaster (World) |
| `Canvas_HUD` | 0 | 시간, 골드, 미니맵, 툴바 | Graphic Raycaster (HUD 영역만) |
| `Canvas_Screen` | 10 | 인벤토리, 상점, 퀘스트, 업적, 메뉴 | Graphic Raycaster (전체) |
| `Canvas_Popup` | 20 | 확인 대화상자, 레벨업 팝업 | Graphic Raycaster (전체) |
| `Canvas_Notification` | 30 | 토스트 알림 | Graphic Raycaster 없음 (클릭 무시) |
| `Canvas_Tutorial` | 40 | 튜토리얼 하이라이트, 강제 안내 | Graphic Raycaster (하이라이트 영역만) |

### 2.3 Raycaster 충돌 처리 규칙

| 규칙 | 설명 |
|------|------|
| **팝업 딤 배경 차단** | Canvas_Popup 활성 시, 딤 배경 Image(Raycast Target=true)가 하위 Canvas 클릭을 차단 |
| **Screen 배경 차단** | Canvas_Screen 활성 시, Screen 배경 패널이 HUD/World 클릭 차단 |
| **알림 패스스루** | Canvas_Notification의 토스트는 Raycast Target=false, 아래 UI 클릭에 영향 없음 |
| **튜토리얼 마스크** | Canvas_Tutorial은 SpotlightMask로 하이라이트 대상만 클릭 허용, 나머지 차단 |
| **HUD 최소 차단** | Canvas_HUD의 버튼/아이콘만 Raycast Target=true, 나머지 배경은 false |

### 2.4 World Space HUD 상세

World Space Canvas는 게임 월드 내 오브젝트 위에 표시되는 정보 UI이다.

```
Canvas_WorldSpace
├── CropInfoPrefab (풀링)      # 작물 상태 표시
│   ├── GrowthBar             # 성장 진행도 바
│   ├── CropNameText          # 작물 이름
│   └── QualityIcon           # 예상 품질 아이콘
│
├── BuildingInfoPrefab (풀링)  # 시설 상태 표시
│   ├── StatusIcon            # 가동 중/대기/연료 부족
│   ├── ProgressBar           # 가공 진행도
│   └── BuildingNameText      # 시설 이름
│
└── InteractionPromptPrefab   # 상호작용 프롬프트 ("E: 수확", "E: 대화")
    ├── KeyIcon               # 키 아이콘
    └── ActionText            # 행동 설명
```

**World Space UI 표시 규칙**:
- 카메라와의 거리에 따라 자동 스케일링 (Billboard 방식, 항상 카메라 정면)
- 플레이어가 일정 거리 이내일 때만 활성화 (오브젝트 풀링으로 관리)
- Screen Space UI가 활성화되면 World Space UI 비활성화 (시각적 혼잡 방지)

---

## 3. 알림 시스템 아키텍처 (NotificationManager)

### 3.1 NotificationPriority enum

```csharp
// illustrative
namespace SeedMind.UI
{
    /// <summary>
    /// 토스트 알림 우선순위.
    /// 높은 우선순위일수록 먼저 표시되며, 동일 우선순위 내에서는 FIFO.
    /// </summary>
    public enum NotificationPriority
    {
        Low      = 0,   // 일반 정보 (예: "씨앗을 심었습니다")
        Normal   = 1,   // 일반 성과 (예: "감자 수확 x3")
        High     = 2,   // 중요 성과 (예: 퀘스트 완료, 레벨업)
        Critical = 3    // 긴급 알림 (예: 작물 고사 경고, 저장 실패)
    }
}
```

### 3.2 NotificationManager 클래스 다이어그램

```
┌──────────────────────────────────────────────────────────────────┐
│        NotificationManager (MonoBehaviour, Singleton)              │
│──────────────────────────────────────────────────────────────────│
│  [설정]                                                           │
│  - _maxVisibleToasts: int = 3         // 동시 최대 표시 수        │
│  - _defaultDuration: float = 3.0f     // 기본 표시 시간 (초)      │
│  - _criticalDuration: float = 5.0f    // Critical 표시 시간       │
│  - _slideInDuration: float = 0.25f                               │
│  - _slideOutDuration: float = 0.2f                               │
│  - _verticalSpacing: float = 8f       // 토스트 간 간격 (px)      │
│                                                                  │
│  [참조]                                                           │
│  - _toastPrefab: GameObject           // ToastUI 프리팹           │
│  - _toastContainer: Transform         // Canvas_Notification 자식 │
│                                                                  │
│  [상태]                                                           │
│  - _pendingQueue: PriorityQueue<NotificationRequest>             │
│  - _activeToasts: List<ToastUI>       // 현재 표시 중인 토스트    │
│  - _toastPool: ObjectPool<ToastUI>    // 오브젝트 풀              │
│                                                                  │
│  [읽기 전용 프로퍼티]                                               │
│  + ActiveToastCount: int                                         │
│  + PendingCount: int                                             │
│                                                                  │
│  [API]                                                            │
│  + ShowNotification(NotificationData data): void                 │
│  + ShowNotification(string msg, NotificationPriority p): void    │
│  + ClearAll(): void                                              │
│                                                                  │
│  [내부 메서드]                                                     │
│  - ProcessQueue(): void               // Update에서 호출          │
│  - SpawnToast(NotificationRequest): ToastUI                      │
│  - RetireToast(ToastUI): void         // 풀 반환 + 재배치         │
│  - RepositionActiveToasts(): void     // 토스트 위치 재계산       │
│                                                                  │
│  [이벤트 구독]                                                    │
│  + OnEnable():                                                   │
│      QuestEvents.OnQuestCompleted += HandleQuestComplete         │
│      QuestEvents.OnQuestFailed += HandleQuestFailed              │
│      AchievementEvents.OnAchievementUnlocked += HandleAchievement│
│      ProgressionEvents.OnLevelUp += HandleLevelUp                │
│      ProgressionEvents.OnUnlockAcquired += HandleUnlock          │
│      FarmEvents.OnCropHarvested += HandleHarvest                 │
│      FarmEvents.OnCropWithered += HandleWithered                 │
│      EconomyEvents.OnSaleCompleted += HandleSale                 │
│      SaveEvents.OnSaveFailed += HandleSaveFailed                 │
│      BuildingEvents.OnConstructionCompleted += HandleBuildingDone│
│      ProcessingEvents.OnProcessingCompleted += HandleProcessDone │
│      TutorialEvents.OnTutorialStepCompleted += HandleTutorialStep│
│  + OnDisable(): 구독 해제                                         │
└──────────────────────────────────────────────────────────────────┘
```

### 3.3 NotificationData / NotificationRequest

```
┌──────────────────────────────────────────────────────────────────┐
│              NotificationData (struct)                             │
│──────────────────────────────────────────────────────────────────│
│  + Message: string                    // 표시할 텍스트            │
│  + Priority: NotificationPriority                                │
│  + Icon: Sprite                       // null이면 기본 아이콘     │
│  + Duration: float                    // 0이면 우선순위별 기본값   │
│  + Color: Color                       // 배경색 힌트 (선택)       │
└──────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│              NotificationRequest (struct)                          │
│──────────────────────────────────────────────────────────────────│
│  + Data: NotificationData                                        │
│  + Timestamp: float                   // Time.unscaledTime        │
│  + CompareKey: int                    // Priority * 10000 - Timestamp (PQ 정렬키) │
└──────────────────────────────────────────────────────────────────┘
```

### 3.4 ToastUI 프리팹 구조

```
┌──────────────────────────────────────────────────────────────────┐
│              ToastUI (MonoBehaviour)                               │
│──────────────────────────────────────────────────────────────────│
│  [참조]                                                           │
│  - _backgroundImage: Image            // 배경 (Rounded Rect)     │
│  - _iconImage: Image                  // 좌측 아이콘              │
│  - _messageText: TMP_Text             // 메시지 텍스트            │
│  - _progressBar: Image                // 표시 시간 잔여 바 (필링) │
│  - _canvasGroup: CanvasGroup                                     │
│                                                                  │
│  [상태]                                                           │
│  - _remainingTime: float                                         │
│  - _isActive: bool                                               │
│                                                                  │
│  [메서드]                                                         │
│  + Setup(NotificationData data): void                            │
│  + SlideIn(): IEnumerator                                        │
│  + SlideOut(): IEnumerator                                       │
│  + Tick(float deltaTime): bool        // false 반환 시 만료       │
│  + ForceHide(): void                  // 즉시 숨김 (ClearAll용)  │
└──────────────────────────────────────────────────────────────────┘
```

### 3.5 큐 처리 로직

```
매 프레임 (NotificationManager.Update):
│
├── 1) 활성 토스트 Tick (만료 시 RetireToast)
│
├── 2) _activeToasts.Count < _maxVisibleToasts && _pendingQueue.Count > 0:
│      ├── request = _pendingQueue.Dequeue()
│      ├── toast = SpawnToast(request)
│      ├── _activeToasts.Add(toast)
│      └── toast.SlideIn() 시작
│
└── 3) 토스트 추가/제거 발생 시: RepositionActiveToasts()
       ├── 화면 우상단에서 아래로 _verticalSpacing 간격 배치
       └── 기존 토스트는 DOTween/Lerp으로 부드럽게 이동
```

### 3.6 이벤트-알림 매핑 테이블

| 이벤트 | 발행 시스템 | 알림 우선순위 | 메시지 예시 |
|--------|------------|:------------:|------------|
| `QuestEvents.OnQuestCompleted` | QuestManager | High | "퀘스트 완료: {questName}" |
| `QuestEvents.OnQuestFailed` | QuestManager | High | "퀘스트 실패: {questName}" |
| `AchievementEvents.OnAchievementUnlocked` | AchievementManager | High | (-> see `docs/systems/achievement-architecture.md` 섹션 8.2 for 전용 토스트) |
| `ProgressionEvents.OnLevelUp` | ProgressionManager | High | "레벨 업! Lv.{level}" |
| `ProgressionEvents.OnUnlockAcquired` | ProgressionManager | Normal | "{itemName} 해금!" |
| `FarmEvents.OnCropHarvested` | FarmGrid | Low | "{cropName} 수확 x{qty}" |
| `FarmEvents.OnCropWithered` | GrowthSystem | Critical | "{cropName} 고사!" |
| `EconomyEvents.OnSaleCompleted` | EconomyManager | Normal | "{goldAmount}G 획득" |
| `SaveEvents.OnSaveFailed` | SaveManager | Critical | "저장 실패 - 재시도 중..." |
| `BuildingEvents.OnConstructionCompleted` | BuildingManager | Normal | "{buildingName} 건설 완료" |
| `ProcessingEvents.OnProcessingCompleted` | ProcessingSystem | Normal | "{itemName} 가공 완료" |
| `TutorialEvents.OnTutorialStepCompleted` | TutorialManager | Low | (튜토리얼 내부 처리, 토스트 선택적) |

**업적 알림 위임**: 업적 해금 알림은 `AchievementToastUI`가 별도 전용 UI로 처리한다 (-> see `docs/systems/achievement-architecture.md` 섹션 8.2). NotificationManager는 업적 해금 이벤트를 수신하지만, AchievementToastUI가 이미 등록되어 있으면 중복 표시를 방지한다.

---

## 4. 이벤트-UI 연동 설계

### 4.1 UIEvents 정적 클래스

```csharp
// illustrative
namespace SeedMind.UI
{
    /// <summary>
    /// UI 시스템의 외부 발행 이벤트.
    /// 다른 시스템이 UI 상태 변화를 감지하기 위해 구독한다.
    /// </summary>
    public static class UIEvents
    {
        // --- Screen 상태 ---
        public static event System.Action<ScreenType> OnScreenOpened;
        public static event System.Action<ScreenType> OnScreenClosed;

        // --- Popup 상태 ---
        public static event System.Action<PopupBase> OnPopupShown;
        public static event System.Action<PopupBase> OnPopupHidden;

        // --- 입력 모드 ---
        public static event System.Action<UIInputMode> OnInputModeChanged;

        // --- 알림 ---
        public static event System.Action<NotificationData> OnNotificationShown;
        public static event System.Action OnAllNotificationsCleared;

        // --- HUD 갱신 요청 ---
        public static event System.Action OnHUDRefreshRequested;

        // --- Raise 메서드 ---
        public static void RaiseScreenOpened(ScreenType t) => OnScreenOpened?.Invoke(t);
        public static void RaiseScreenClosed(ScreenType t) => OnScreenClosed?.Invoke(t);
        public static void RaisePopupShown(PopupBase p) => OnPopupShown?.Invoke(p);
        public static void RaisePopupHidden(PopupBase p) => OnPopupHidden?.Invoke(p);
        public static void RaiseInputModeChanged(UIInputMode m) => OnInputModeChanged?.Invoke(m);
        public static void RaiseNotificationShown(NotificationData d) => OnNotificationShown?.Invoke(d);
        public static void RaiseAllNotificationsCleared() => OnAllNotificationsCleared?.Invoke();
        public static void RaiseHUDRefreshRequested() => OnHUDRefreshRequested?.Invoke();
    }
}
```

### 4.2 각 Manager -> UIManager 데이터 흐름

UI 시스템은 의존성 최상층에 위치하므로, 하위 시스템의 이벤트를 구독하여 표시를 갱신한다. 하위 시스템은 UI를 직접 참조하지 않는다.

```
[하위 시스템 이벤트 발행]                        [UI 시스템 구독/처리]

TimeManager
├── OnHourChanged ─────────────────────────▶ HUDController.RefreshTimeDisplay()
├── OnDayChanged ──────────────────────────▶ HUDController.RefreshDateDisplay()
└── OnSeasonChanged ───────────────────────▶ HUDController.RefreshSeasonIcon()

EconomyManager
├── OnGoldChanged ─────────────────────────▶ HUDController.RefreshGoldDisplay()
├── OnSaleCompleted ───────────────────────▶ NotificationManager.HandleSale()
└── OnShopPurchased ───────────────────────▶ ShopUI.RefreshAfterPurchase()
                                               (ShopUI가 직접 구독, Screen 활성 시만)

InventoryManager
├── OnBackpackChanged ─────────────────────▶ InventoryUI.RefreshBackpack()
├── OnToolbarChanged ──────────────────────▶ HUDController.RefreshToolbar()
└── OnToolbarSelectionChanged ─────────────▶ HUDController.RefreshToolSelection()

ProgressionManager
├── OnExpGained ───────────────────────────▶ HUDController.RefreshExpBar()
│                                              (→ see docs/systems/progression-architecture.md 섹션 7.2)
├── OnLevelUp ─────────────────────────────▶ NotificationManager.HandleLevelUp()
│                                          ▶ UIManager.ShowPopup(LevelUpPopup, High)
└── OnUnlockAcquired ──────────────────────▶ NotificationManager.HandleUnlock()

QuestManager (via QuestEvents)
├── OnQuestCompleted ──────────────────────▶ NotificationManager.HandleQuestComplete()
├── OnObjectiveProgress ───────────────────▶ QuestUI.RefreshObjective()
│                                              (QuestUI가 직접 구독, Screen 활성 시만)
└── OnQuestUnlocked ───────────────────────▶ NotificationManager (Low 우선순위)

AchievementManager (via AchievementEvents)
├── OnAchievementUnlocked ─────────────────▶ AchievementToastUI.ShowToast()
│                                              (→ see docs/systems/achievement-architecture.md 섹션 8.2)
└── OnProgressUpdated ─────────────────────▶ AchievementPanel.RefreshItem()
                                               (AchievementPanel 활성 시만)

SaveManager (via SaveEvents)
├── OnSaveStarted ─────────────────────────▶ HUDController.ShowSaveIndicator()
├── OnSaveCompleted ───────────────────────▶ HUDController.HideSaveIndicator()
└── OnSaveFailed ──────────────────────────▶ NotificationManager.HandleSaveFailed()
                                           ▶ UIManager.ShowPopup(SaveErrorPopup, Critical)

BuildingManager (via BuildingEvents)
├── OnConstructionCompleted ───────────────▶ NotificationManager.HandleBuildingDone()
└── OnConstructionStarted ─────────────────▶ (World Space UI 진행바 활성화)

ProcessingSystem (via ProcessingEvents)
└── OnProcessingCompleted ─────────────────▶ NotificationManager.HandleProcessDone()

TutorialManager (via TutorialEvents)
├── OnTutorialStepStarted ─────────────────▶ TutorialUI.ShowStep()
│                                              (→ see docs/systems/tutorial-architecture.md 섹션 6)
└── OnTutorialCompleted ───────────────────▶ NotificationManager (Low)
```

### 4.3 HUDController 상세

HUDController는 게임 플레이 중 항상 표시되는 상시 UI를 관리한다.

```
┌──────────────────────────────────────────────────────────────────┐
│           HUDController (MonoBehaviour)                            │
│──────────────────────────────────────────────────────────────────│
│  [참조 — 시간/날짜]                                                │
│  - _timeText: TMP_Text               // "14:30"                  │
│  - _dayText: TMP_Text                // "Day 15"                 │
│  - _seasonIcon: Image                // 계절 아이콘               │
│  - _weatherIcon: Image               // 날씨 아이콘               │
│                                                                  │
│  [참조 — 경제]                                                     │
│  - _goldText: TMP_Text               // "1,250 G"                │
│                                                                  │
│  [참조 — 진행]                                                     │
│  - _levelText: TMP_Text              // "Lv.5"                   │
│  - _expBar: Slider                   // XP 진행률 바              │
│                                                                  │
│  [참조 — 툴바]                                                     │
│  - _toolbarSlotUIs: SlotUI[]         // 툴바 슬롯 UI 배열        │
│  - _selectedHighlight: Image         // 선택 하이라이트           │
│                                                                  │
│  [참조 — 시스템 상태]                                               │
│  - _saveIndicator: GameObject        // 저장 중 아이콘            │
│  - _autoSaveSpinner: Animator                                    │
│                                                                  │
│  [구독 — OnEnable]                                                │
│  TimeManager.OnHourChanged += RefreshTimeDisplay                 │
│  TimeManager.OnDayChanged += RefreshDateDisplay                  │
│  TimeManager.OnSeasonChanged += RefreshSeasonIcon                │
│  EconomyManager.OnGoldChanged += RefreshGoldDisplay              │
│  ProgressionManager.OnExpGained += RefreshExpBar                 │
│  ProgressionManager.OnLevelUp += RefreshLevelDisplay             │
│  InventoryManager.OnToolbarChanged += RefreshToolbar             │
│  InventoryManager.OnToolbarSelectionChanged += RefreshToolSelect │
│  SaveEvents.OnSaveStarted += ShowSaveIndicator                   │
│  SaveEvents.OnSaveCompleted += HideSaveIndicator                 │
│  UIEvents.OnHUDRefreshRequested += RefreshAll                    │
│  UIEvents.OnScreenOpened += OnScreenOpened    // Screen 열리면 HUD 축소/숨김 │
│  UIEvents.OnScreenClosed += OnScreenClosed    // Screen 닫히면 HUD 복원      │
└──────────────────────────────────────────────────────────────────┘
```

### 4.4 Screen별 구현 요약

각 ScreenBase 파생 클래스는 이미 다른 아키텍처 문서에서 정의되어 있다. 이 문서는 UIManager와의 연동 인터페이스만 기술한다.

| Screen 클래스 | ScreenType | 정의 문서 | UIManager 연동 비고 |
|---------------|-----------|-----------|---------------------|
| `InventoryUI` | Inventory | `docs/systems/inventory-architecture.md` 섹션 6 | Tab 키 토글, OnBeforeOpen에서 슬롯 리프레시 |
| `ShopUI` | Shop | (-> see economy-architecture.md) | NPC 상호작용으로 열림, 거래 후 골드/인벤토리 갱신 |
| `QuestUI` | Quest | `docs/systems/quest-architecture.md` 섹션 8 | J 키 토글, 목표 진행도 실시간 갱신 |
| `AchievementPanel` | Achievement | `docs/systems/achievement-architecture.md` 섹션 8.1 | Y 키 토글, 카테고리 탭 분류 (-> see `docs/systems/achievement-system.md` 섹션 5.1) |
| `MenuUI` | Menu | (본 문서 고유) | Esc 키 토글, 게임 시간 일시 정지 |
| `SaveLoadUI` | SaveLoad | (-> see save-load-architecture.md) | Menu에서 진입, 슬롯 3개 표시 |
| `DialogueUI` | Dialogue | (-> see npc-shop-architecture.md) | NPC 상호작용, 모든 입력 차단 |
| `ProcessingUI` | Processing | `docs/systems/processing-architecture.md` 섹션 7 | 가공소 상호작용으로 열림 |

---

## 5. 기존 시스템 UI와의 통합

### 5.1 AchievementToastUI와 NotificationManager 공존

`achievement-architecture.md` 섹션 8.2에서 정의한 `AchievementToastUI`는 업적 전용 토스트로, NotificationManager의 일반 토스트와 별도 위치에 표시된다.

| 항목 | AchievementToastUI | NotificationManager |
|------|-------------------|---------------------|
| Canvas 위치 | Canvas_Notification 하위 | Canvas_Notification 하위 |
| 표시 위치 | 화면 상단 중앙 | 화면 우상단 |
| 큐 방식 | 독립 Queue<AchievementData> | PriorityQueue<NotificationRequest> |
| 동시 표시 | 1개 | 최대 3개 |
| 이벤트 | AchievementEvents.OnAchievementUnlocked | 다수 이벤트 (섹션 3.6 참조) |

**중복 방지**: NotificationManager는 `AchievementEvents.OnAchievementUnlocked` 구독 시, AchievementToastUI가 씬에 존재하면 해당 이벤트의 알림 생성을 건너뛴다 (중복 토스트 방지).

### 5.2 TutorialUI와 UIManager 협력

튜토리얼 시스템은 Canvas_Tutorial(Sort Order 40)을 사용하여 모든 UI 위에 오버레이된다 (-> see `docs/systems/tutorial-architecture.md` 섹션 6). UIManager는 튜토리얼 활성 중 다음을 보장한다:

- 튜토리얼 단계가 특정 Screen을 요구하면 UIManager.OpenScreen()을 호출
- 튜토리얼 활성 중 Esc 키 동작 차단 (메뉴 열기 금지)
- 튜토리얼 완료 후 원래 UIInputMode 복원

---

## 6. 파일 배치 및 네임스페이스

기존 `docs/systems/project-structure.md`의 `Scripts/UI/` 폴더를 확장한다.

```
Assets/_Project/Scripts/UI/              # SeedMind.UI 네임스페이스
├── UIManager.cs                         # 싱글턴, Screen FSM, PopupQueue 관리
├── ScreenBase.cs                        # Screen 추상 기반 클래스
├── PopupBase.cs                         # Popup 추상 기반 클래스
├── PopupQueue.cs                        # 우선순위 큐 유틸리티
├── NotificationManager.cs              # 싱글턴, 토스트 알림 큐
├── ToastUI.cs                           # 토스트 프리팹 컴포넌트
├── TooltipManager.cs                    # 마우스 오버 툴팁
├── UIEvents.cs                          # 정적 이벤트 허브
├── HUDController.cs                     # 상시 HUD (기존)
├── LevelBarUI.cs                        # XP 바 (기존, → see progression-architecture.md)
├── InventoryUI.cs                       # 인벤토리 화면 (기존, → see inventory-architecture.md)
├── ShopUI.cs                            # 상점 화면 (기존)
├── QuestUI.cs                           # 퀘스트 화면 (기존, → see quest-architecture.md)
├── AchievementPanel.cs                  # 업적 화면 (기존, → see achievement-architecture.md)
├── AchievementToastUI.cs                # 업적 전용 토스트 (기존, → see achievement-architecture.md)
├── MenuUI.cs                            # 메뉴/설정 화면
├── SaveLoadUI.cs                        # 세이브/로드 슬롯 화면
├── DialogueUI.cs                        # 대화 화면 (기존)
├── ProcessingUI.cs                      # 가공소 화면 (기존, → see processing-architecture.md)
├── TutorialUI.cs                        # 튜토리얼 UI (기존, → see tutorial-architecture.md)
├── SlotUI.cs                            # 아이템 슬롯 공통 컴포넌트 (기존, → see inventory-architecture.md)
├── TooltipUI.cs                         # 툴팁 패널 컴포넌트
└── Data/                                # SeedMind.UI.Data 네임스페이스
    ├── ScreenType.cs                    # enum
    ├── PopupPriority.cs                 # enum
    ├── UIInputMode.cs                   # enum
    ├── NotificationPriority.cs          # enum
    ├── NotificationData.cs              # struct
    └── NotificationRequest.cs           # struct
```

---

# Part II -- MCP 구현 태스크 요약

---

## MCP-Step 1: UIManager 코어 GameObject 생성

```
1.1 SCN_Farm.unity 열기
1.2 빈 GameObject "UIRoot" 생성 → DontDestroyOnLoad 마커
1.3 UIRoot에 UIManager.cs 컴포넌트 추가
1.4 UIRoot 하위에 빈 GameObject "NotificationManager" 생성
    → NotificationManager.cs 컴포넌트 추가
1.5 UIRoot 하위에 빈 GameObject "TooltipManager" 생성
    → TooltipManager.cs 컴포넌트 추가
```

## MCP-Step 2: Canvas 계층 설정

```
2.1 GameObject "Canvas_WorldSpace" 생성
    → Canvas 컴포넌트: Render Mode = World Space
    → GraphicRaycaster 추가
    → CanvasScaler: Scale With Screen Size, 1920x1080

2.2 GameObject "Canvas_HUD" 생성
    → Canvas 컴포넌트: Render Mode = Screen Space - Overlay, Sort Order = 0
    → CanvasScaler: Scale With Screen Size, 1920x1080
    → GraphicRaycaster 추가
    → HUDController.cs 컴포넌트 추가

2.3 GameObject "Canvas_Screen" 생성
    → Canvas: Screen Space - Overlay, Sort Order = 10
    → CanvasScaler: Scale With Screen Size, 1920x1080
    → GraphicRaycaster 추가
    → 기본 상태: CanvasGroup.alpha = 0, interactable = false

2.4 GameObject "Canvas_Popup" 생성
    → Canvas: Screen Space - Overlay, Sort Order = 20
    → CanvasScaler: Scale With Screen Size, 1920x1080
    → GraphicRaycaster 추가

2.5 GameObject "Canvas_Notification" 생성
    → Canvas: Screen Space - Overlay, Sort Order = 30
    → CanvasScaler: Scale With Screen Size, 1920x1080
    → GraphicRaycaster 제거 (클릭 패스스루)

2.6 GameObject "Canvas_Tutorial" 생성
    → Canvas: Screen Space - Overlay, Sort Order = 40
    → CanvasScaler: Scale With Screen Size, 1920x1080
    → GraphicRaycaster 추가
```

## MCP-Step 3: HUD 구조 배치

```
3.1 Canvas_HUD 하위에 "TopBar" 패널 생성
    → Anchor: Top Stretch
    → 자식: TimeText(TMP), DayText(TMP), SeasonIcon(Image), WeatherIcon(Image), GoldText(TMP)

3.2 Canvas_HUD 하위에 "LevelBar" 패널 생성
    → Anchor: Top-Left
    → 자식: LevelText(TMP), ExpBarSlider(Slider)
    → LevelBarUI.cs 추가

3.3 Canvas_HUD 하위에 "ToolbarContainer" 생성
    → Anchor: Bottom-Center
    → HorizontalLayoutGroup 추가
    → SlotUI 프리팹 인스턴스 배치 (슬롯 수: → see docs/systems/inventory-architecture.md 섹션 1.2)
    → SelectedHighlight Image 추가

3.4 Canvas_HUD 하위에 "SaveIndicator" 생성
    → Anchor: Top-Right
    → 기본 비활성 (SetActive false)
    → 저장 아이콘 Image + 회전 Animator
```

## MCP-Step 4: 핵심 Screen 프리팹 생성

```
4.1 Canvas_Screen 하위에 "InventoryScreen" 생성
    → ScreenBase(InventoryUI) 컴포넌트
    → CanvasGroup 추가 (alpha=0, interactable=false, blocksRaycasts=false)
    → 내부 구조: → see docs/systems/inventory-architecture.md 섹션 6
    → 프리팹화: PFB_Screen_Inventory.prefab → Assets/_Project/Prefabs/UI/

4.2 Canvas_Screen 하위에 "ShopScreen" 생성
    → ScreenBase(ShopUI) 컴포넌트
    → CanvasGroup 추가
    → 프리팹화: PFB_Screen_Shop.prefab

4.3 Canvas_Screen 하위에 "QuestScreen" 생성
    → ScreenBase(QuestUI) 컴포넌트
    → 내부 구조: → see docs/systems/quest-architecture.md 섹션 8
    → 프리팹화: PFB_Screen_Quest.prefab

4.4 Canvas_Screen 하위에 "AchievementScreen" 생성
    → ScreenBase(AchievementPanel) 컴포넌트
    → 내부 구조: → see docs/systems/achievement-architecture.md 섹션 8.1
    → 프리팹화: PFB_Screen_Achievement.prefab

4.5 Canvas_Screen 하위에 "MenuScreen" 생성
    → ScreenBase(MenuUI) 컴포넌트
    → CanvasGroup 추가
    → 자식: ResumeButton, SaveButton, LoadButton, SettingsButton, QuitButton
    → 프리팹화: PFB_Screen_Menu.prefab

4.6 Canvas_Screen 하위에 "SaveLoadScreen" 생성
    → ScreenBase(SaveLoadUI) 컴포넌트
    → 자식: SlotContainer(3슬롯), BackButton
    → 프리팹화: PFB_Screen_SaveLoad.prefab

4.7 Canvas_Screen 하위에 "DialogueScreen" 생성
    → ScreenBase(DialogueUI) 컴포넌트
    → 프리팹화: PFB_Screen_Dialogue.prefab

4.8 Canvas_Screen 하위에 "ProcessingScreen" 생성
    → ScreenBase(ProcessingUI) 컴포넌트
    → 내부 구조: → see docs/systems/processing-architecture.md 섹션 7
    → 프리팹화: PFB_Screen_Processing.prefab
```

## MCP-Step 5: 알림 프리팹 생성

```
5.1 Canvas_Notification 하위에 "ToastContainer" 생성 (우상단 Anchor)
    → VerticalLayoutGroup (Spacing: 8, Child Alignment: Upper Right)

5.2 ToastUI 프리팹 생성: PFB_Toast.prefab
    → RoundedRect 배경 Image
    → 좌측 Icon Image (48x48)
    → MessageText (TMP, 최대 2줄)
    → ProgressBar Image (Fill)
    → CanvasGroup
    → ToastUI.cs 추가

5.3 NotificationManager에 _toastPrefab, _toastContainer 참조 설정

5.4 AchievementToastUI 전용 토스트 배치 (화면 상단 중앙)
    → see docs/systems/achievement-architecture.md 섹션 8.2
```

## MCP-Step 6: UIManager Screen 등록 및 검증

```
6.1 각 ScreenBase 파생 클래스의 Awake()에서 UIManager.RegisterScreen() 호출 설정

6.2 Play Mode 진입 → 다음 검증:
    - Tab 키: InventoryScreen Open/Close 토글 확인
    - Esc 키: MenuScreen Open 확인
    - J 키: QuestScreen Open 확인
    - Y 키: AchievementScreen Open 확인
    - 화면 전환 시 이전 Screen 자동 Close 확인

6.3 NotificationManager 검증:
    - Console에서 테스트 알림 발송 → 토스트 표시/자동 만료 확인
    - 4개 이상 연속 발송 → 동시 최대 3개 표시, 나머지 큐 대기 확인

6.4 PopupQueue 검증:
    - 동시 팝업 2개 요청 → 우선순위 높은 것 먼저 표시 확인
    - 팝업 닫기 후 큐 다음 항목 자동 표시 확인
```

---

## Cross-references

- `docs/systems/ui-system.md` (DES-011) -- UI/UX 디자인 canonical (HUD 배치, 키 바인딩, 알림 우선순위, Canvas 논리 계층)
- `docs/architecture.md` -- 마스터 기술 아키텍처, 프로젝트 구조 개요, 입력 시스템 (섹션 4.4)
- `docs/systems/project-structure.md` -- 폴더 구조, 네임스페이스 규칙, 의존성 매트릭스 (UI는 최상층)
- `docs/systems/inventory-architecture.md` -- InventoryUI, SlotUI, 드래그/드롭 구조 (섹션 6)
- `docs/systems/quest-architecture.md` (ARC-013) -- QuestUI, QuestEvents 이벤트 허브
- `docs/systems/achievement-architecture.md` (ARC-017) -- AchievementPanel, AchievementToastUI, AchievementEvents
- `docs/systems/save-load-architecture.md` (ARC-011) -- SaveManager, SaveEvents, SaveLoadUI 슬롯 구조
- `docs/systems/progression-architecture.md` (BAL-002) -- ProgressionManager 이벤트, LevelBarUI (섹션 7.2)
- `docs/systems/tutorial-architecture.md` -- TutorialUI, Canvas_Tutorial 오버레이 (섹션 6)
- `docs/systems/processing-architecture.md` (ARC-012) -- ProcessingUI, ProcessingEvents
- `docs/systems/npc-shop-architecture.md` (ARC-008) -- DialogueUI, NPCEvents
- `docs/systems/economy-architecture.md` -- EconomyManager.OnGoldChanged, EconomyEvents
- `docs/systems/farming-architecture.md` -- FarmEvents (OnCropHarvested, OnCropWithered 등)
- `docs/systems/crop-growth-architecture.md` -- FarmEvents 확장 (OnCropStageChanged, OnGiantCropFormed 등)
- `docs/pipeline/data-pipeline.md` -- ScriptableObject 패턴, DataRegistry
- `docs/systems/facilities-architecture.md` (ARC-007) -- BuildingEvents

---

## Open Questions

- [OPEN] **DOTween vs Unity Animation**: 토스트 슬라이드 인/아웃, 팝업 페이드 등의 UI 트위닝에 DOTween 패키지를 사용할지, Unity 내장 Animator/코루틴만으로 처리할지. DOTween은 코드 기반 애니메이션에 편리하나 외부 의존성이 추가된다.

- [OPEN] **UI Toolkit vs uGUI**: Unity 6에서 UI Toolkit이 런타임 UI를 지원하기 시작했으나, 현재 설계는 uGUI(Canvas/CanvasGroup) 기반이다. 프로토타입 후 UI Toolkit 전환 필요성을 재검토한다.

- [OPEN] **Screen 전환 애니메이션**: 단순 페이드 인/아웃 외에 슬라이드, 스케일 등의 전환 효과가 필요한지. 로우폴리 스타일에 맞는 간결한 전환이 적합할 것으로 예상되나 디자인 확정 후 결정.

- [OPEN] **접근성 (Accessibility)**: 키보드/컨트롤러 전용 UI 내비게이션이 필요한지. 현재 _firstSelected 필드로 기본 지원은 설계했으나, 전체 내비게이션 흐름은 추후 확정.

---

## Risks

- [RISK] **static event 구독 누수**: UIEvents, QuestEvents 등 정적 이벤트를 UI 컴포넌트가 구독한 후 씬 전환 시 해제하지 않으면 메모리 누수 및 NullReferenceException 발생. 모든 ScreenBase/HUDController는 OnDisable에서 반드시 구독 해제해야 한다. 이 패턴은 `docs/systems/farming-architecture.md` 섹션 6.3에서 이미 [RISK]로 식별됨.

- [RISK] **Canvas 5개 분리 성능**: Screen Space Overlay Canvas가 5개(HUD, Screen, Popup, Notification, Tutorial)일 경우 Canvas.BuildBatch 호출 빈도가 증가한다. 실제 프로파일링 후 Canvas 병합이 필요할 수 있다.

- [RISK] **NotificationManager와 AchievementToastUI 중복 표시**: 두 시스템이 동일 이벤트(OnAchievementUnlocked)를 구독하므로, 중복 방지 로직이 정확히 동작하지 않으면 업적 알림이 2번 표시된다. 런타임 검증 필수.

- [RISK] **PopupQueue 우선순위 역전**: 낮은 우선순위 팝업이 먼저 표시된 상태에서 높은 우선순위 팝업이 요청되었을 때, 현재 팝업을 중단하고 신규 팝업을 표시하는 로직의 코루틴 동시성 처리에 주의 필요.

- [RISK] **World Space UI 성능**: 작물 타일 수가 최대 16x16 = 256개일 때, World Space CropInfoPrefab 풀링 크기와 활성/비활성 전환 빈도가 프레임 레이트에 영향을 줄 수 있다. 가시 범위 내 타일만 활성화하는 LOD 전략이 필요.

- [RISK] **MCP ScriptableObject 배열 참조 설정 한계**: UIManager._screens Dictionary의 초기화가 MCP로 자동화 가능한지 불확실. 런타임 RegisterScreen() 패턴으로 우회하되, 에디터 Inspector에서 수동 설정이 필요할 수 있다. (기존 [RISK] in `docs/architecture.md`)

---

*이 문서는 Claude Code가 기존 아키텍처 문서들과의 일관성을 검증하여 자율적으로 작성했습니다.*
