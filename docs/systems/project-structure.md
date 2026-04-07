# Unity 프로젝트 구조 상세 설계

> 폴더 구조, 네임스페이스, 의존성 규칙, Assembly Definition, 씬 구조, 에셋 네이밍 규칙  
> 작성: Claude Code (Opus) | 2026-04-06

---

## Context

이 문서는 `docs/architecture.md` 3절의 프로젝트 구조를 상세화한다. Unity 6 프로젝트의 모든 파일이 일관된 규칙 하에 배치되고, 모듈 간 의존성이 명확히 제어되도록 설계한다. MCP를 통한 자동화 구축 시에도 이 규칙을 따른다.

---

## 1. 폴더 구조 상세

```
Assets/
├── _Project/                          # 프로젝트 전용 최상위 (Unity 패키지와 분리)
│   ├── Scripts/
│   │   ├── Core/                      # 게임 프레임워크 (의존성 없음)
│   │   │   ├── GameManager.cs
│   │   │   ├── TimeManager.cs
│   │   │   ├── EventBus.cs            # 범용 이벤트 시스템
│   │   │   └── Singleton.cs           # 제네릭 싱글턴 베이스
│   │   │   # SaveManager는 Scripts/Save/ 에 배치 (→ see docs/systems/save-load-architecture.md 섹션 8)
│   │   │
│   │   ├── Farm/                      # 경작 시스템
│   │   │   ├── FarmGrid.cs
│   │   │   ├── FarmTile.cs
│   │   │   ├── CropInstance.cs
│   │   │   ├── GrowthSystem.cs
│   │   │   ├── FarmEvents.cs          # 경작 이벤트 허브
│   │   │   └── Data/
│   │   │       ├── CropData.cs        # ScriptableObject 정의
│   │   │       ├── FertilizerData.cs
│   │   │       └── TileState.cs       # enum 정의
│   │   │
│   │   ├── Player/                    # 플레이어 시스템
│   │   │   ├── PlayerController.cs
│   │   │   ├── PlayerInventory.cs
│   │   │   ├── ToolSystem.cs
│   │   │   ├── ToolUpgradeSystem.cs     # 도구 업그레이드 처리 (→ see docs/systems/tool-upgrade-architecture.md)
│   │   │   ├── ToolEffectResolver.cs    # 도구 등급별 효과 계산 (static)
│   │   │   ├── ToolUpgradeEvents.cs     # 업그레이드 이벤트 허브
│   │   │   └── Data/
│   │   │       ├── ToolData.cs
│   │   │       ├── PendingUpgrade.cs          # 업그레이드 진행 상태
│   │   │       ├── ToolUpgradeInfo.cs         # 이벤트 페이로드, enums
│   │   │       ├── ToolSpecialEffect.cs       # 특수 효과 enum
│   │   │       └── ToolUpgradeSaveData.cs     # 세이브 데이터
│   │   │
│   │   ├── Economy/                   # 경제 시스템
│   │   │   ├── EconomyManager.cs
│   │   │   ├── ShopSystem.cs
│   │   │   ├── PriceFluctuationSystem.cs
│   │   │   ├── TransactionLog.cs
│   │   │   └── Data/
│   │   │       ├── EconomyConfig.cs
│   │   │       ├── PriceData.cs
│   │   │       └── ShopData.cs
│   │   │
│   │   ├── Building/                  # 건설 시스템
│   │   │   ├── BuildingManager.cs
│   │   │   ├── Data/
│   │   │   │   └── BuildingData.cs
│   │   │   └── Buildings/
│   │   │       ├── WaterTank.cs
│   │   │       ├── Greenhouse.cs
│   │   │       ├── Storage.cs
│   │   │       └── Processor.cs
│   │   │
│   │   ├── Level/                     # 진행 시스템 (→ see docs/systems/progression-architecture.md)
│   │   │   ├── ProgressionManager.cs  # XP/레벨/해금/마일스톤 관리 (LevelSystem.cs 대체)
│   │   │   ├── UnlockRegistry.cs      # 해금 상태 관리
│   │   │   ├── MilestoneTracker.cs    # 마일스톤 추적
│   │   │   ├── XPSource.cs            # enum
│   │   │   ├── UnlockType.cs          # enum
│   │   │   ├── MilestoneConditionType.cs # enum
│   │   │   ├── ProgressionEvents.cs   # ExpGainInfo, LevelUpInfo, UnlockInfo
│   │   │   └── Data/
│   │   │       ├── ProgressionData.cs # 진행 설정 SO (LevelConfig.cs 대체)
│   │   │       ├── LevelUnlockEntry.cs
│   │   │       ├── MilestoneData.cs
│   │   │       └── MilestoneReward.cs
│   │   │
│   │   ├── Save/                      # 세이브/로드 시스템 (→ see docs/systems/save-load-architecture.md)
│   │   │   ├── SaveManager.cs         # 싱글턴, 비동기 저장/로드 API
│   │   │   ├── AutoSaveTrigger.cs     # 자동저장 트리거 컴포넌트
│   │   │   ├── SaveEvents.cs          # 정적 이벤트 허브
│   │   │   ├── ISaveable.cs           # 저장/로드 인터페이스
│   │   │   ├── SaveMigrator.cs        # 세이브 버전 마이그레이션
│   │   │   ├── SaveDataValidator.cs   # 세이브 데이터 무결성 검증
│   │   │   ├── SaveVersionException.cs # 버전 불일치 예외
│   │   │   └── Data/                  # SeedMind.Save.Data 네임스페이스
│   │   │       ├── GameSaveData.cs    # 통합 루트 세이브 클래스
│   │   │       ├── SaveMetaFile.cs    # 메타 파일 구조
│   │   │       └── SaveSlotInfo.cs    # 슬롯 정보 구조
│   │   │
│   │   ├── Tutorial/                  # 튜토리얼 시스템 (→ see docs/systems/tutorial-architecture.md)
│   │   │   ├── TutorialManager.cs     # 싱글턴, 시퀀스 진행 제어
│   │   │   ├── TutorialTriggerSystem.cs  # 이벤트 버스 구독, 트리거 판정
│   │   │   ├── ContextHintSystem.cs   # 상황별 자동 힌트
│   │   │   ├── TutorialEvents.cs      # 정적 이벤트 허브
│   │   │   ├── TutorialSaveData.cs    # 세이브 데이터
│   │   │   └── Data/
│   │   │       ├── TutorialSequenceData.cs  # 시퀀스 SO 정의
│   │   │       ├── TutorialStepData.cs      # 단계 SO 정의
│   │   │       └── ContextHintData.cs       # 힌트 SO 정의
│   │   │
│   │   ├── Collection/                # 수집 도감 시스템 (→ see docs/systems/collection-architecture.md, ARC-037)
│   │   │   ├── CollectionUIController.cs  # 통합 도감 UI 컨트롤러
│   │   │   ├── GatheringCatalogManager.cs # ISaveable, SaveLoadOrder=56
│   │   │   ├── GatheringCatalogEntry.cs   # 런타임 상태
│   │   │   ├── GatheringCatalogSaveData.cs
│   │   │   └── Data/                  # SeedMind.Collection.Data 네임스페이스
│   │   │       └── GatheringCatalogData.cs  # ScriptableObject 정의
│   │   │
│   │   └── UI/                        # UI 시스템 (→ see docs/systems/ui-architecture.md for full details)
│   │       ├── UIManager.cs           # 싱글턴, Screen FSM, PopupQueue 관리
│   │       ├── ScreenBase.cs          # Screen 추상 기반 클래스
│   │       ├── PopupBase.cs           # Popup 추상 기반 클래스
│   │       ├── PopupQueue.cs          # 우선순위 큐 유틸리티
│   │       ├── NotificationManager.cs # 싱글턴, 토스트 알림 큐
│   │       ├── ToastUI.cs             # 토스트 프리팹 컴포넌트
│   │       ├── TooltipManager.cs      # 마우스 오버 툴팁
│   │       ├── UIEvents.cs            # 정적 이벤트 허브
│   │       ├── HUDController.cs
│   │       ├── LevelBarUI.cs          # 레벨/경험치 바 UI (SeedMind.UI, → see docs/systems/progression-architecture.md 섹션 7.2)
│   │       ├── InventoryUI.cs
│   │       ├── ShopUI.cs
│   │       ├── MenuUI.cs              # 메뉴/설정 화면
│   │       ├── SaveLoadUI.cs          # 세이브/로드 슬롯 화면
│   │       ├── DialogueUI.cs
│   │       ├── TutorialUI.cs          # 튜토리얼 UI (SeedMind.UI, → see docs/systems/tutorial-architecture.md 섹션 6)
│   │       ├── ProcessingUI.cs        # 가공소 UI (SeedMind.UI, → see docs/systems/processing-architecture.md 섹션 7)
│   │       ├── QuestUI.cs             # 퀘스트 화면 (→ see docs/systems/quest-architecture.md 섹션 8)
│   │       ├── AchievementPanel.cs    # 업적 화면 (→ see docs/systems/achievement-architecture.md 섹션 8.1)
│   │       ├── AchievementToastUI.cs  # 업적 전용 토스트 (→ see docs/systems/achievement-architecture.md 섹션 8.2)
│   │       ├── SlotUI.cs              # 아이템 슬롯 공통 (→ see docs/systems/inventory-architecture.md)
│   │       ├── TooltipUI.cs           # 툴팁 패널 컴포넌트
│   │       └── Data/                  # SeedMind.UI.Data 네임스페이스
│   │           ├── ScreenType.cs
│   │           ├── PopupPriority.cs
│   │           ├── UIInputMode.cs
│   │           ├── NotificationPriority.cs
│   │           ├── NotificationData.cs
│   │           └── NotificationRequest.cs
│   │
│   ├── Data/                          # ScriptableObject 인스턴스 (에셋)
│   │   ├── Crops/                     # SO_Crop_Potato.asset 등
│   │   ├── Fertilizers/               # SO_Fert_Basic.asset 등
│   │   ├── Tools/                     # SO_Tool_Hoe_T1.asset 등
│   │   ├── Buildings/                 # SO_Bldg_WaterTank.asset 등
│   │   ├── Recipes/                   # SO_Recipe_Jam_Potato.asset 등 (→ see docs/systems/processing-architecture.md Part II)
│   │   ├── Tutorial/                  # SO_TutSeq_MainTutorial.asset, SO_TutStep_*.asset, SO_CtxHint_*.asset
│   │   └── Config/                    # SO_ProgressionData.asset (SO_LevelConfig.asset 대체), SO_TimeConfig.asset 등
│   │
│   ├── Prefabs/
│   │   ├── Player/                    # PFB_Player.prefab
│   │   ├── Crops/                     # PFB_Crop_Potato_Stage0~3.prefab
│   │   ├── Buildings/                 # PFB_Bldg_WaterTank.prefab
│   │   ├── Farm/                      # PFB_FarmTile.prefab
│   │   ├── Environment/               # PFB_Tree_01.prefab, PFB_Fence_01.prefab
│   │   └── UI/                        # PFB_UI_Popup.prefab
│   │
│   ├── Materials/
│   │   ├── Terrain/                   # M_Soil_Empty.mat, M_Soil_Tilled.mat, M_Grass.mat
│   │   ├── Crops/                     # M_Crop_Potato.mat, M_Crop_Carrot.mat
│   │   ├── Buildings/                 # M_Bldg_Wood.mat, M_Bldg_Stone.mat
│   │   └── Environment/              # M_Water.mat, M_Sky.mat
│   │
│   ├── Textures/                      # 텍스처 (사용 시)
│   │   └── UI/                        # 스프라이트, 아이콘
│   │
│   ├── Scenes/
│   │   ├── Main/
│   │   │   ├── SCN_MainMenu.unity
│   │   │   ├── SCN_Farm.unity         # 메인 플레이 씬
│   │   │   └── SCN_Loading.unity      # 로딩 씬
│   │   └── Test/
│   │       ├── SCN_Test_FarmGrid.unity    # 경작 시스템 단독 테스트
│   │       └── SCN_Test_Player.unity      # 플레이어 이동 테스트
│   │
│   ├── Audio/
│   │   ├── SFX/                       # 효과음
│   │   └── BGM/                       # 배경음악
│   │
│   ├── Animations/
│   │   ├── Player/                    # 플레이어 애니메이션
│   │   └── Crops/                     # 작물 흔들림 등
│   │
│   ├── Input/
│   │   └── SeedMindInputActions.inputactions  # Input System 에셋
│   │
│   └── Resources/
│       └── UI/                        # UI 스프라이트, 폰트
│
├── Plugins/                           # 서드파티 플러그인 (필요 시)
└── Settings/                          # URP, Quality, Input 등 Unity 설정
```

### 폴더 규칙

| 규칙 | 설명 |
|------|------|
| `_Project/` 접두어 | Unity 패키지 폴더(Packages/)와 명확히 분리. 탐색기에서 항상 최상단 정렬 |
| Scripts 내 `Data/` 하위폴더 | ScriptableObject **클래스 정의**(.cs)는 해당 시스템의 Data/ 폴더에 배치 |
| `_Project/Data/` 최상위 | ScriptableObject **인스턴스**(.asset)는 Data/ 폴더에 카테고리별 배치 |
| `Test/` 씬 | 시스템 단독 테스트용 씬. 빌드에 포함하지 않음 |

---

## 2. 네임스페이스 설계

```
SeedMind                          # 최상위 네임스페이스 (공용 인터페이스, 열거형)
├── SeedMind.Core                 # 게임 프레임워크
├── SeedMind.Farm                 # 경작 시스템
├── SeedMind.Farm.Data            # 경작 관련 ScriptableObject 정의
├── SeedMind.Player               # 플레이어 시스템
├── SeedMind.Player.Data          # 도구 데이터
├── SeedMind.Economy              # 경제 시스템
├── SeedMind.Economy.Data         # 가격 데이터
├── SeedMind.Building             # 건설 시스템
├── SeedMind.Building.Data        # 건물 데이터
├── SeedMind.Level                # 레벨/경험치
├── SeedMind.Level.Data           # 레벨 설정 데이터
├── SeedMind.Tutorial             # 튜토리얼 시스템 (→ see docs/systems/tutorial-architecture.md)
├── SeedMind.Tutorial.Data        # 튜토리얼 SO 정의
├── SeedMind.Save                 # 세이브/로드 시스템 (→ see docs/systems/save-load-architecture.md)
├── SeedMind.Save.Data            # GameSaveData, SaveMetaFile, SaveSlotInfo
├── SeedMind.UI                   # UI 시스템 (→ see docs/systems/ui-architecture.md)
├── SeedMind.UI.Data              # UI enum/struct 정의 (ScreenType, PopupPriority, NotificationData 등)
├── SeedMind.Collection           # 수집 도감 시스템 (→ see docs/systems/collection-architecture.md, ARC-037)
└── SeedMind.Collection.Data      # GatheringCatalogData, GatheringCatalogSaveData, FishCatalogData SO 정의
```

### 네임스페이스 규칙

- 모든 클래스는 반드시 네임스페이스에 소속
- 폴더 경로와 네임스페이스 1:1 매핑: `Scripts/Farm/FarmGrid.cs` → `namespace SeedMind.Farm`
- `Data` 하위 네임스페이스는 ScriptableObject 정의 전용
- 글로벌 유틸리티가 필요하면 `SeedMind` 최상위에 배치 (최소화)

---

## 3. 의존성 규칙

### 3.1 의존성 방향 다이어그램

```
                ┌─────────┐
                │  Core   │  (의존성 없음 — 최하층)
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
               ┌────────────┐
               │  Tutorial  │  (기존 시스템 이벤트 구독만, 역방향 의존 없음)
               └──────┬─────┘
                      ▼
               ┌──────────┐
               │    UI    │  (최상층 — 모든 것을 참조 가능)
               └──────────┘
```

### 3.2 의존성 매트릭스

| 모듈 | Core | Farm | Player | Economy | Building | Level | Tutorial | UI |
|------|:----:|:----:|:------:|:-------:|:--------:|:-----:|:--------:|:--:|
| **Core** | - | X | X | X | X | X | X | X |
| **Farm** | O | - | X | X | X | X | X | X |
| **Player** | O | O | - | X | X | X | X | X |
| **Economy** | O | O | X | - | X | X | X | X |
| **Building** | O | O | X | O | - | X | X | X |
| **Level** | O | O | X | X | X | - | X | X |
| **Tutorial** | O | O | O | O | O | O | - | X |
| **UI** | O | O | O | O | O | O | O | - |

O = 참조 허용, X = 참조 금지

### 3.3 핵심 의존성 규칙

1. **Core는 아무것도 참조하지 않는다** — 순수 프레임워크
2. **하위 계층은 상위 계층을 참조하지 않는다** — Farm은 UI를 모른다
3. **동일 계층 간 참조는 이벤트로 통신** — Farm과 Economy는 직접 참조 대신 이벤트 사용
4. **UI는 모든 것을 참조할 수 있다** — 표시 계층이므로
5. **Data 네임스페이스는 동일 모듈 내에서만 참조** — 외부 모듈은 인터페이스/이벤트를 통해 데이터 접근

### 3.4 모듈 간 통신 패턴

```
Farm ──[FarmEvents.OnCropHarvested]──▶ Economy (판매가 계산)
Farm ──[FarmEvents.OnCropHarvested]──▶ Level (경험치 부여)
Player ──[ToolSystem.UseCurrentTool()]──▶ Farm (타일 조작)
TimeManager ──[OnDayChanged]──▶ Farm.GrowthSystem (성장 처리)
TimeManager ──[OnSeasonChanged]──▶ Economy (계절 가격 변동)
```

---

## 4. Assembly Definition 구성

Assembly Definition(asmdef)으로 컴파일 단위를 분리하여 빌드 시간 단축 및 의존성 강제.

| asmdef 파일 | 위치 | 참조하는 asmdef |
|-------------|------|----------------|
| `SeedMind.Core.asmdef` | `Scripts/Core/` | (없음) |
| `SeedMind.Farm.asmdef` | `Scripts/Farm/` | Core |
| `SeedMind.Player.asmdef` | `Scripts/Player/` | Core, Farm |
| `SeedMind.Economy.asmdef` | `Scripts/Economy/` | Core, Farm |
| `SeedMind.Building.asmdef` | `Scripts/Building/` | Core, Farm, Economy |
| `SeedMind.Level.asmdef` | `Scripts/Level/` | Core, Farm |
| `SeedMind.UI.asmdef` | `Scripts/UI/` | Core, Farm, Player, Economy, Building, Level |
| `SeedMind.Collection.asmdef` | `Scripts/Collection/` | Core, Player (→ see docs/systems/collection-architecture.md, ARC-037) |

### asmdef 규칙

- 각 asmdef는 자신의 폴더와 하위 폴더의 스크립트만 포함
- `Auto Referenced` = true (Editor에서 자동 로드)
- `Allow Unsafe Code` = false
- Unity 내장 어셈블리 참조: `Unity.InputSystem` (Player에서만)
- [RISK] asmdef 설정이 잘못되면 순환 참조 컴파일 에러 발생. 의존성 매트릭스를 엄격히 따를 것.

---

## 5. 씬 구조

### 5.1 씬 목록 및 역할

| 씬 | 파일명 | 역할 | 빌드 포함 |
|----|--------|------|:---------:|
| 메인 메뉴 | `SCN_MainMenu.unity` | 타이틀, 새 게임/불러오기, 설정 | O |
| 로딩 | `SCN_Loading.unity` | 씬 전환 중 로딩 UI 표시 | O |
| 농장 (메인) | `SCN_Farm.unity` | 핵심 게임플레이 | O |
| 테스트: 그리드 | `SCN_Test_FarmGrid.unity` | 경작 시스템 단독 검증 | X |
| 테스트: 플레이어 | `SCN_Test_Player.unity` | 이동/입력 검증 | X |

### 5.2 씬 전환 전략

```
SCN_MainMenu
    │
    ├──[새 게임]──▶ SCN_Loading → SCN_Farm (Additive 아님, 단일 씬)
    ├──[불러오기]──▶ SCN_Loading → SCN_Farm + SaveData 적용
    └──[설정]──▶ 메뉴 내 UI 패널 (씬 전환 없음)

SCN_Farm
    │
    ├──[상점]──▶ UI 오버레이 (씬 전환 없음, Canvas 기반)
    ├──[인벤토리]──▶ UI 오버레이
    ├──[메뉴(Esc)]──▶ 일시정지 UI 오버레이
    └──[메인 메뉴로]──▶ SCN_Loading → SCN_MainMenu
```

### 5.3 씬 전환 구현 방식

- `SceneManager.LoadSceneAsync()` 사용 (비동기 로딩)
- 로딩 씬을 중간 단계로 활용: 이전 씬 언로드 → 로딩 UI 표시 → 새 씬 로드
- 상점, 인벤토리 등 서브 화면은 씬 전환 없이 UI Canvas 토글
- GameManager, TimeManager 등 영구 오브젝트는 `DontDestroyOnLoad` 적용

### 5.4 SCN_Farm 씬 계층 구조

```
SCN_Farm (Scene Root)
├── --- MANAGERS ---
│   ├── GameManager          (DontDestroyOnLoad)
│   ├── TimeManager          (DontDestroyOnLoad)
│   └── SaveManager          (DontDestroyOnLoad)
│
├── --- FARM ---
│   ├── FarmSystem
│   │   ├── FarmGrid
│   │   │   ├── Tile_0_0
│   │   │   ├── Tile_0_1
│   │   │   └── ... (8x8)
│   │   └── GrowthSystem
│   │
│   └── Buildings
│       ├── (동적 생성)
│       └── ...
│
├── --- PLAYER ---
│   └── Player
│       ├── PlayerModel
│       ├── PlayerController
│       └── ToolSystem
│
├── --- ENVIRONMENT ---
│   ├── Terrain
│   ├── Lighting
│   │   ├── Sun (Directional Light)
│   │   └── AmbientProbe
│   └── Decorations
│       ├── Fences
│       └── Trees
│
├── --- ECONOMY ---
│   ├── EconomyManager
│   ├── Shop
│   └── ShippingBin          (출하함 — 24시간 이용 가능, → see docs/systems/economy-system.md 섹션 3.2)
│
├── --- CAMERA ---
│   └── Main Camera (Orthographic, 쿼터뷰)
│
└── --- UI ---
    ├── Canvas_HUD          (Screen Space - Overlay)
    │   ├── TimeDisplay
    │   ├── GoldDisplay
    │   ├── ToolBar
    │   └── LevelBar
    ├── Canvas_Overlay      (Screen Space - Overlay, 기본 비활성)
    │   ├── InventoryPanel
    │   ├── ShopPanel
    │   └── PausePanel
    └── Canvas_Popup        (Screen Space - Overlay)
        └── PopupMessage
```

---

## 6. 에셋 네이밍 규칙

### 6.1 접두어 체계

| 에셋 유형 | 접두어 | 예시 |
|-----------|--------|------|
| Scene | `SCN_` | `SCN_Farm.unity` |
| Prefab | `PFB_` | `PFB_Crop_Potato_Stage2.prefab` |
| Material | `M_` | `M_Soil_Tilled.mat` |
| ScriptableObject | `SO_` | `SO_Crop_Potato.asset` |
| Texture | `T_` | `T_UI_Icon_Hoe.png` |
| Animation Clip | `ANIM_` | `ANIM_Player_Walk.anim` |
| Animator Controller | `AC_` | `AC_Player.controller` |
| Audio Clip (SFX) | `SFX_` | `SFX_Hoe_Hit.wav` |
| Audio Clip (BGM) | `BGM_` | `BGM_Spring.ogg` |
| Input Actions | (없음) | `SeedMindInputActions.inputactions` |

### 6.2 네이밍 패턴

```
[접두어]_[카테고리]_[이름]_[변형].확장자

예시:
  PFB_Crop_Potato_Stage0.prefab      작물 프리팹, 감자, 0단계
  PFB_Crop_Potato_Stage3.prefab      작물 프리팹, 감자, 3단계(수확)
  M_Soil_Empty.mat                    머티리얼, 토양, 빈 상태
  M_Soil_Watered.mat                  머티리얼, 토양, 물 준 상태
  SO_Crop_Tomato.asset                ScriptableObject, 토마토 작물 데이터
  SO_Tool_Hoe_T1.asset                ScriptableObject, 호미, 1등급
  SO_Bldg_Greenhouse.asset            ScriptableObject, 온실 건물 데이터
```

### 6.3 스크립트 네이밍

| 유형 | 패턴 | 예시 |
|------|------|------|
| MonoBehaviour | PascalCase, 역할 서술 | `FarmGrid.cs`, `PlayerController.cs` |
| ScriptableObject | PascalCase + `Data` 접미어 | `CropData.cs`, `ToolData.cs` |
| enum | PascalCase | `TileState.cs`, `SeasonFlag.cs` |
| static event hub | PascalCase + `Events` 접미어 | `FarmEvents.cs` |
| interface | `I` 접두어 | `ISaveable.cs`, `IInteractable.cs` |
| 제네릭 베이스 | PascalCase, 추상적 이름 | `Singleton.cs`, `EventBus.cs` |

### 6.4 GameObject 네이밍 (씬 내)

| 유형 | 패턴 | 예시 |
|------|------|------|
| 매니저 | PascalCase | `GameManager`, `TimeManager` |
| 타일 | `Tile_{x}_{y}` | `Tile_0_0`, `Tile_7_7` |
| 구분선 | `--- {CATEGORY} ---` | `--- MANAGERS ---`, `--- UI ---` |
| UI Canvas | `Canvas_{용도}` | `Canvas_HUD`, `Canvas_Overlay` |
| UI 요소 | PascalCase 서술 | `TimeDisplay`, `GoldDisplay` |

---

## Open Questions

- [OPEN] `_Project/` 접두어 대신 `_SeedMind/` 처럼 프로젝트명을 쓸지 -- 패키지가 늘어나면 구분 필요할 수 있음
- [OPEN] 테스트 씬을 `Scenes/Test/` 대신 별도 `_Test/` 최상위 폴더로 분리할지
- [OPEN] Addressables 도입 여부 -- 현재 스코프에서는 Resources + 직접 참조로 충분하나, 확장 시 고려

## Risks

- [RISK] asmdef 순환 참조 -- 의존성 매트릭스(섹션 3.2)를 위반하면 컴파일 실패. 새 모듈 추가 시 반드시 매트릭스 갱신
- [RISK] DontDestroyOnLoad 오브젝트 중복 -- 씬 재로드 시 매니저가 이중 생성될 수 있음. Singleton 패턴의 중복 검사 필수
- [RISK] MCP로 폴더 구조 자동 생성 시 경로 오타 -- MCP 태스크에서 폴더 경로를 상수로 정의하여 재사용

---

## Cross-references

- `docs/architecture.md` 3절 (프로젝트 구조 개요)
- `docs/architecture.md` 4.3절 (TimeManager), 4.4절 (입력 시스템)
- `docs/systems/farming-architecture.md` (경작 시스템 클래스 구조)
- `docs/systems/tool-upgrade-architecture.md` (도구 업그레이드 시스템 — DES-007)
- `docs/mcp/scene-setup-tasks.md` (기본 씬 구성 MCP 태스크 — ARC-002)
- `docs/pipeline/` (빌드 파이프라인, 작성 예정)
