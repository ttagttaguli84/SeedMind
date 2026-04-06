# 튜토리얼 시스템 MCP 태스크 시퀀스 (ARC-010)

> 튜토리얼 매니저 배치, UI 프리팹 생성, TutorialStepData/SequenceData/ContextHintData SO 에셋 생성, 통합 테스트를 MCP for Unity 태스크로 상세 정의  
> 작성: Claude Code (Opus 4.6) | 2026-04-07  
> Phase 1 | 문서 ID: ARC-010

---

## 1. 개요

### 목적

이 문서는 `docs/systems/tutorial-architecture.md`(DES-006) Part II(MCP-1~MCP-5)를 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 튜토리얼 시스템의 데이터 레이어(SO 에셋), 시스템 레이어(스크립트), UI 레이어(Canvas_Tutorial 프리팹), 씬 배치를 완성한다.

### 의존성

```
튜토리얼 시스템 MCP 태스크 의존 관계:
├── SeedMind.Core       (GameManager, TimeManager, SaveManager)
├── SeedMind.Farm       (FarmGrid, FarmTile, FarmEvents, CropData)
├── SeedMind.Player     (InventoryManager, PlayerController)
├── SeedMind.Economy    (EconomyManager -- 골드 조회)
├── SeedMind.Building   (BuildingEvents -- 시설 트리거)
├── SeedMind.NPC        (NPCEvents -- 상점/대화 트리거)
├── SeedMind.ToolUpgrade(ToolUpgradeEvents -- 업그레이드 트리거)
└── SeedMind.UI         (UIManager, 기존 Canvas 계층)
```

(-> see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 완료된 태스크 의존성

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | 전체 | 폴더 구조, SCN_Farm 기본 계층 (MANAGERS, UI) |
| ARC-003 | `docs/mcp/farming-tasks.md` | 전체 | FarmGrid, CropData SO, ToolData SO, FarmEvents |
| ARC-008/ARC-015 | `docs/mcp/tool-upgrade-tasks.md` | 전체 | ToolUpgradeSystem, ToolUpgradeEvents |
| ARC-009 | `docs/mcp/npc-shop-tasks.md` | 전체 | NPCManager, NPCEvents, DialogueSystem |

### 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `Canvas_HUD`, `Canvas_Screen`, `Canvas_Popup`, `Canvas_Notification` | ARC-002 Phase B / ui-architecture.md |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A |
| `Assets/_Project/Scripts/UI/` 폴더 | ARC-002 Phase A |
| `FarmGrid`, `FarmEvents` | ARC-003 |
| `EconomyManager` | economy-architecture.md |
| `InventoryManager` | inventory-architecture.md |
| `BuildingManager`, `BuildingEvents` | ARC-007 |
| `NPCManager`, `NPCEvents`, `DialogueSystem` | ARC-009 |
| `ToolUpgradeSystem`, `ToolUpgradeEvents` | ARC-015 |

### 총 MCP 호출 예상 수

| 태스크 | 설명 | 호출 수 |
|--------|------|--------|
| T-1 | TutorialManager GameObject 배치 | ~30회 |
| T-2 | TutorialUI 프리팹 생성 | ~64회 |
| T-3 | TutorialStepData SO 에셋 생성 (메인 12단계) | ~122회 |
| T-4 | 시스템 튜토리얼 SO 에셋 생성 (4종) | ~45회 |
| T-5 | ContextHintData SO 에셋 생성 (7종) | ~79회 |
| T-6 | 통합 테스트 | 12회 |
| **합계** | | **~352회** |

[RISK] SO 배열 참조 설정(TutorialSequenceData.steps[], ContextHintSystem._allHints[])이 MCP `set_property`로 가능한지 사전 검증 필요. 불가능한 경우 Editor 스크립트(CreateTutorialAssets.cs)를 통한 일괄 생성으로 T-3~T-5의 호출 수를 대폭 감소시킬 수 있다.

---

## 2. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋/스크립트 폴더 생성 | T-1, T-3 |
| `create_script` | C# 스크립트 파일 생성 | T-1 |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | T-3, T-4, T-5 |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | T-1~T-5 전체 |
| `create_object` | 빈 GameObject 생성 | T-1, T-2 |
| `add_component` | MonoBehaviour 컴포넌트 부착 | T-1, T-2 |
| `set_parent` | 오브젝트 부모 설정 | T-1, T-2 |
| `save_as_prefab` | GameObject를 프리팹으로 저장 | T-2 |
| `save_scene` | 씬 저장 | T-1, T-2, T-6 |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | T-6 |
| `execute_menu_item` | 편집기 명령 실행 (컴파일 대기 등) | T-1 |
| `execute_method` | 런타임 메서드 호출 (테스트) | T-6 |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | T-6 |

[RISK] `create_scriptable_object`, `save_as_prefab` 도구의 가용 여부 및 파라미터 형식 사전 검증 필요. (-> see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)

---

## 3. 필요 C# 스크립트 목록

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 Phase 순서대로 작성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 Phase |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/Tutorial/Data/TutorialType.cs` | `TutorialType` (enum) | `SeedMind.Tutorial.Data` | T-1 Phase 1 |
| S-02 | `Scripts/Tutorial/Data/TutorialUIType.cs` | `TutorialUIType` (enum) | `SeedMind.Tutorial.Data` | T-1 Phase 1 |
| S-03 | `Scripts/Tutorial/Data/TutorialAnchorType.cs` | `TutorialAnchorType` (enum) | `SeedMind.Tutorial.Data` | T-1 Phase 1 |
| S-04 | `Scripts/Tutorial/Data/StepCompletionType.cs` | `StepCompletionType` (enum) | `SeedMind.Tutorial.Data` | T-1 Phase 1 |
| S-05 | `Scripts/Tutorial/Data/TutorialTriggerType.cs` | `TutorialTriggerType` (enum) | `SeedMind.Tutorial.Data` | T-1 Phase 1 |
| S-06 | `Scripts/Tutorial/Data/HintConditionType.cs` | `HintConditionType` (enum) | `SeedMind.Tutorial.Data` | T-1 Phase 1 |
| S-07 | `Scripts/Tutorial/Data/TutorialSequenceData.cs` | `TutorialSequenceData` (SO) | `SeedMind.Tutorial.Data` | T-1 Phase 1 |
| S-08 | `Scripts/Tutorial/Data/TutorialStepData.cs` | `TutorialStepData` (SO) | `SeedMind.Tutorial.Data` | T-1 Phase 1 |
| S-09 | `Scripts/Tutorial/Data/ContextHintData.cs` | `ContextHintData` (SO) | `SeedMind.Tutorial.Data` | T-1 Phase 1 |
| S-10 | `Scripts/Tutorial/TutorialEvents.cs` | `TutorialEvents` (static) | `SeedMind.Tutorial` | T-1 Phase 2 |
| S-11 | `Scripts/Tutorial/TutorialSaveData.cs` | `TutorialSaveData` (직렬화 클래스) | `SeedMind.Tutorial` | T-1 Phase 2 |
| S-12 | `Scripts/Tutorial/TutorialManager.cs` | `TutorialManager` (MonoBehaviour Singleton) | `SeedMind.Tutorial` | T-1 Phase 3 |
| S-13 | `Scripts/Tutorial/TutorialTriggerSystem.cs` | `TutorialTriggerSystem` (MonoBehaviour) | `SeedMind.Tutorial` | T-1 Phase 3 |
| S-14 | `Scripts/Tutorial/ContextHintSystem.cs` | `ContextHintSystem` (MonoBehaviour) | `SeedMind.Tutorial` | T-1 Phase 3 |
| S-15 | `Scripts/UI/TutorialUI.cs` | `TutorialUI` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 4 |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: S-01~S-09 -> S-10/S-11 -> S-12~S-14 -> S-15. 각 Phase 사이에 Unity 컴파일 대기(`execute_menu_item`)가 필요하다.

---

## 4. SO 에셋 목록

| # | 에셋명 | 경로 | SO 타입 | 생성 태스크 |
|---|--------|------|---------|-----------|
| A-01 | `SO_TutSeq_MainTutorial.asset` | `Assets/_Project/Data/Tutorial/Sequences/` | TutorialSequenceData | T-3-01 |
| A-02 | `SO_TutStep_Main_01_Arrival.asset` | `Assets/_Project/Data/Tutorial/Steps/` | TutorialStepData | T-3-02 |
| A-03 | `SO_TutStep_Main_02_Movement.asset` | `Assets/_Project/Data/Tutorial/Steps/` | TutorialStepData | T-3-02 |
| A-04 | `SO_TutStep_Main_03_MeetHana.asset` | `Assets/_Project/Data/Tutorial/Steps/` | TutorialStepData | T-3-02 |
| A-05 | `SO_TutStep_Main_04_TillSoil.asset` | `Assets/_Project/Data/Tutorial/Steps/` | TutorialStepData | T-3-02 |
| A-06 | `SO_TutStep_Main_05_PlantSeeds.asset` | `Assets/_Project/Data/Tutorial/Steps/` | TutorialStepData | T-3-02 |
| A-07 | `SO_TutStep_Main_06_WaterCrops.asset` | `Assets/_Project/Data/Tutorial/Steps/` | TutorialStepData | T-3-02 |
| A-08 | `SO_TutStep_Main_07_Sleep.asset` | `Assets/_Project/Data/Tutorial/Steps/` | TutorialStepData | T-3-02 |
| A-09 | `SO_TutStep_Main_08_GrowthCheck.asset` | `Assets/_Project/Data/Tutorial/Steps/` | TutorialStepData | T-3-02 |
| A-10 | `SO_TutStep_Main_09_Harvest.asset` | `Assets/_Project/Data/Tutorial/Steps/` | TutorialStepData | T-3-02 |
| A-11 | `SO_TutStep_Main_10_FirstSale.asset` | `Assets/_Project/Data/Tutorial/Steps/` | TutorialStepData | T-3-02 |
| A-12 | `SO_TutStep_Main_11_Reinvest.asset` | `Assets/_Project/Data/Tutorial/Steps/` | TutorialStepData | T-3-02 |
| A-13 | `SO_TutStep_Main_12_Complete.asset` | `Assets/_Project/Data/Tutorial/Steps/` | TutorialStepData | T-3-02 |
| A-14 | `SO_TutSeq_BuildingIntro.asset` | `Assets/_Project/Data/Tutorial/Sequences/` | TutorialSequenceData | T-4-01 |
| A-15 | `SO_TutSeq_ToolUpgradeIntro.asset` | `Assets/_Project/Data/Tutorial/Sequences/` | TutorialSequenceData | T-4-01 |
| A-16 | `SO_TutSeq_SeasonChange.asset` | `Assets/_Project/Data/Tutorial/Sequences/` | TutorialSequenceData | T-4-01 |
| A-17 | `SO_TutSeq_ProcessingIntro.asset` | `Assets/_Project/Data/Tutorial/Sequences/` | TutorialSequenceData | T-4-01 |
| A-18 | `SO_CtxHint_WaterReminder.asset` | `Assets/_Project/Data/Tutorial/Hints/` | ContextHintData | T-5-01 |
| A-19 | `SO_CtxHint_LowGold.asset` | `Assets/_Project/Data/Tutorial/Hints/` | ContextHintData | T-5-01 |
| A-20 | `SO_CtxHint_InventoryFull.asset` | `Assets/_Project/Data/Tutorial/Hints/` | ContextHintData | T-5-01 |
| A-21 | `SO_CtxHint_SeasonCrop.asset` | `Assets/_Project/Data/Tutorial/Hints/` | ContextHintData | T-5-01 |
| A-22 | `SO_CtxHint_HarvestReady.asset` | `Assets/_Project/Data/Tutorial/Hints/` | ContextHintData | T-5-01 |
| A-23 | `SO_CtxHint_NightWarning.asset` | `Assets/_Project/Data/Tutorial/Hints/` | ContextHintData | T-5-01 |
| A-24 | `SO_CtxHint_ProcessingReady.asset` | `Assets/_Project/Data/Tutorial/Hints/` | ContextHintData | T-5-01 |

---

## 5. 씬 GameObject 목록

| # | 오브젝트명 | 부모 | 컴포넌트 | 생성 태스크 |
|---|-----------|------|----------|-----------|
| G-01 | `TutorialSystem` | SCN_Farm (root) | TutorialManager (루트에 직접 부착) | T-1-01 |
| G-02 | `TutorialTriggerSystem` | `TutorialSystem` | TutorialTriggerSystem | T-1-03 |
| G-03 | `ContextHintSystem` | `TutorialSystem` | ContextHintSystem | T-1-04 |
| G-04 | `Canvas_Tutorial` | `--- UI ---` | Canvas, CanvasScaler, GraphicRaycaster, TutorialUI | T-2 |
| G-05 | `Panel_Dimmer` | `Canvas_Tutorial` | Image | T-2-02 |
| G-06 | `Mask_Highlight` | `Panel_Dimmer` | RectTransform (마스크) | T-2-02 |
| G-07 | `Panel_Bubble` | `Canvas_Tutorial` | Image, TMP_Text, Button | T-2-03 |
| G-08 | `Panel_Arrow` | `Canvas_Tutorial` | Image | T-2-04 |
| G-09 | `Panel_Popup` | `Canvas_Tutorial` | Image, TMP_Text x2, Button | T-2-05 |
| G-10 | `Panel_Progress` | `Canvas_Tutorial` | TMP_Text, Slider | T-2-06 |

---

## 6. 태스크 T-1: TutorialManager GameObject 배치 및 스크립트 생성

**목적**: 튜토리얼 시스템의 핵심 스크립트를 생성하고, SCN_Farm 씬에 TutorialSystem 컨테이너와 하위 매니저를 배치한다.

**전제**: Core 인프라(GameManager, TimeManager, SaveManager) 컴파일 완료. Farm/Economy/Building/NPC/ToolUpgrade 모듈 컴파일 완료.

**의존 태스크**: ARC-002(씬 기본 계층), ARC-003(FarmEvents), ARC-007(BuildingEvents), ARC-009(NPCEvents), ARC-015(ToolUpgradeEvents)

---

### T-1 Phase 1: 데이터 구조 스크립트 (S-01 ~ S-09)

#### T-1-01: 폴더 생성

```
create_folder
  path: "Assets/_Project/Scripts/Tutorial"

create_folder
  path: "Assets/_Project/Scripts/Tutorial/Data"
```

- **MCP 호출**: 2회

#### T-1-02: TutorialType enum (S-01)

```
create_script
  path: "Assets/_Project/Scripts/Tutorial/Data/TutorialType.cs"
  content: |
    // S-01: 튜토리얼 유형 열거형
    // -> see docs/systems/tutorial-architecture.md 섹션 4.1
    namespace SeedMind.Tutorial.Data
    {
        public enum TutorialType
        {
            MainTutorial   = 0,
            SystemTutorial = 1
        }
    }
```

- **MCP 호출**: 1회

#### T-1-03: TutorialUIType enum (S-02)

```
create_script
  path: "Assets/_Project/Scripts/Tutorial/Data/TutorialUIType.cs"
  content: |
    // S-02: 튜토리얼 UI 표시 유형
    // -> see docs/systems/tutorial-architecture.md 섹션 4.2
    namespace SeedMind.Tutorial.Data
    {
        public enum TutorialUIType
        {
            Bubble     = 0,
            Popup      = 1,
            Arrow      = 2,
            Highlight  = 3,
            Combined   = 4
        }
    }
```

- **MCP 호출**: 1회

#### T-1-04: TutorialAnchorType enum (S-03)

```
create_script
  path: "Assets/_Project/Scripts/Tutorial/Data/TutorialAnchorType.cs"
  content: |
    // S-03: UI 앵커 유형
    // -> see docs/systems/tutorial-architecture.md 섹션 4.2
    namespace SeedMind.Tutorial.Data
    {
        public enum TutorialAnchorType
        {
            WorldTarget    = 0,
            ScreenPosition = 1,
            UIElement      = 2
        }
    }
```

- **MCP 호출**: 1회

#### T-1-05: StepCompletionType enum (S-04)

```
create_script
  path: "Assets/_Project/Scripts/Tutorial/Data/StepCompletionType.cs"
  content: |
    // S-04: 단계 완료 판정 유형
    // -> see docs/systems/tutorial-architecture.md 섹션 4.2
    namespace SeedMind.Tutorial.Data
    {
        public enum StepCompletionType
        {
            EventBased      = 0,
            TimeBased       = 1,
            ClickToContinue = 2,
            Composite       = 3
        }
    }
```

- **MCP 호출**: 1회

#### T-1-06: TutorialTriggerType enum (S-05)

```
create_script
  path: "Assets/_Project/Scripts/Tutorial/Data/TutorialTriggerType.cs"
  content: |
    // S-05: 시퀀스 시작 트리거 유형
    // -> see docs/systems/tutorial-architecture.md 섹션 4.1
    namespace SeedMind.Tutorial.Data
    {
        public enum TutorialTriggerType
        {
            NewGame        = 0,
            UnlockAchieved = 1,
            FirstVisit     = 2,
            EventFired     = 3,
            LevelReached   = 4
        }
    }
```

- **MCP 호출**: 1회

#### T-1-07: HintConditionType enum (S-06)

```
create_script
  path: "Assets/_Project/Scripts/Tutorial/Data/HintConditionType.cs"
  content: |
    // S-06: 상황별 힌트 발동 조건 유형
    // -> see docs/systems/tutorial-architecture.md 섹션 8.2
    namespace SeedMind.Tutorial.Data
    {
        public enum HintConditionType
        {
            DryTilesExist      = 0,
            LowGold            = 1,
            InventoryFull      = 2,
            SeasonMismatchCrop = 3,
            UnusedFertilizer   = 4,
            ReadyToHarvest     = 5,
            NightTime          = 6,
            ProcessingReady    = 7
        }
    }
```

- **MCP 호출**: 1회

#### T-1-08: TutorialSequenceData SO 클래스 (S-07)

```
create_script
  path: "Assets/_Project/Scripts/Tutorial/Data/TutorialSequenceData.cs"
  namespace: "SeedMind.Tutorial.Data"
  base_class: "ScriptableObject"
```

필드 정의: (-> see `docs/systems/tutorial-architecture.md` 섹션 4.1)

- **MCP 호출**: 1회

#### T-1-09: TutorialStepData SO 클래스 (S-08)

```
create_script
  path: "Assets/_Project/Scripts/Tutorial/Data/TutorialStepData.cs"
  namespace: "SeedMind.Tutorial.Data"
  base_class: "ScriptableObject"
```

필드 정의: (-> see `docs/systems/tutorial-architecture.md` 섹션 4.2)

- **MCP 호출**: 1회

#### T-1-10: ContextHintData SO 클래스 (S-09)

```
create_script
  path: "Assets/_Project/Scripts/Tutorial/Data/ContextHintData.cs"
  namespace: "SeedMind.Tutorial.Data"
  base_class: "ScriptableObject"
```

필드 정의: (-> see `docs/systems/tutorial-architecture.md` 섹션 8.2)

- **MCP 호출**: 1회

#### T-1-11: Unity 컴파일 대기 (Phase 1 -> Phase 2)

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

---

### T-1 Phase 2: 이벤트/세이브 클래스 (S-10 ~ S-11)

#### T-1-12: TutorialEvents 정적 클래스 (S-10)

```
create_script
  path: "Assets/_Project/Scripts/Tutorial/TutorialEvents.cs"
  namespace: "SeedMind.Tutorial"
  type: static class
```

이벤트 정의: (-> see `docs/systems/tutorial-architecture.md` 섹션 9)

- **MCP 호출**: 1회

#### T-1-13: TutorialSaveData 직렬화 클래스 (S-11)

```
create_script
  path: "Assets/_Project/Scripts/Tutorial/TutorialSaveData.cs"
  namespace: "SeedMind.Tutorial"
  type: class (Plain C#, [System.Serializable])
```

필드 정의: (-> see `docs/systems/tutorial-architecture.md` 섹션 7.2, PATTERN-005 준수)

- **MCP 호출**: 1회

#### T-1-14: Unity 컴파일 대기 (Phase 2 -> Phase 3)

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

---

### T-1 Phase 3: 시스템 클래스 (S-12 ~ S-14)

#### T-1-15: TutorialManager MonoBehaviour (S-12)

```
create_script
  path: "Assets/_Project/Scripts/Tutorial/TutorialManager.cs"
  namespace: "SeedMind.Tutorial"
  base_class: "MonoBehaviour"  // Singleton 패턴 적용
```

메서드: TryStartSequence, AdvanceToStep, OnStepCompleted, SkipSequence, CompleteSequence, ExportSaveData, ImportSaveData, TryStartSequenceByTrigger, GetActiveStep, IsSequenceCompleted
(-> see `docs/systems/tutorial-architecture.md` 섹션 3.2)

- **MCP 호출**: 1회

#### T-1-16: TutorialTriggerSystem MonoBehaviour (S-13)

```
create_script
  path: "Assets/_Project/Scripts/Tutorial/TutorialTriggerSystem.cs"
  namespace: "SeedMind.Tutorial"
  base_class: "MonoBehaviour"
```

이벤트 구독 대상: FarmEvents, BuildingEvents, NPCEvents, ToolUpgradeEvents, ProgressionEvents
(-> see `docs/systems/tutorial-architecture.md` 섹션 5.1)

- **MCP 호출**: 1회

#### T-1-17: ContextHintSystem MonoBehaviour (S-14)

```
create_script
  path: "Assets/_Project/Scripts/Tutorial/ContextHintSystem.cs"
  namespace: "SeedMind.Tutorial"
  base_class: "MonoBehaviour"
```

메서드: EvaluateHints, IsHintAvailable, EvaluateCondition, ShowHint, DecrementCooldowns, ExportCooldowns, ImportCooldowns
(-> see `docs/systems/tutorial-architecture.md` 섹션 8.3)

- **MCP 호출**: 1회

#### T-1-18: Unity 컴파일 대기 (Phase 3 -> Phase 4)

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

---

### T-1 Phase 4: UI 클래스 (S-15)

#### T-1-19: TutorialUI MonoBehaviour (S-15)

```
create_script
  path: "Assets/_Project/Scripts/UI/TutorialUI.cs"
  namespace: "SeedMind.UI"
  base_class: "MonoBehaviour"
```

메서드: ShowStep, HideStep, ShowBubble, ShowArrow, ShowHighlight, ShowDimmer, ShowPopup, HideAll, PositionToAnchor, UpdateProgress
(-> see `docs/systems/tutorial-architecture.md` 섹션 6.2)

- **MCP 호출**: 1회

#### T-1-20: Unity 컴파일 대기 (Phase 4 -> 씬 배치)

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회

---

### T-1 Phase 5: 씬 배치

#### T-1-21: TutorialSystem 컨테이너 생성

```
create_object
  name: "TutorialSystem"
  scene: "SCN_Farm"

set_property  target: "TutorialSystem"
  transform.position = (0, 0, 0)
```

- **MCP 호출**: 2회

#### T-1-22: TutorialManager 컴포넌트 부착 (G-01에 직접 부착)

```
add_component
  target: "TutorialSystem"
  component: "SeedMind.Tutorial.TutorialManager"
```

- **MCP 호출**: 1회

#### T-1-23: TutorialTriggerSystem 오브젝트 (G-03)

```
create_object
  name: "TutorialTriggerSystem"
  parent: "TutorialSystem"

add_component
  target: "TutorialTriggerSystem"
  component: "SeedMind.Tutorial.TutorialTriggerSystem"

set_property  target: "TutorialTriggerSystem" -> TutorialTriggerSystem
  _manager = [TutorialSystem의 TutorialManager 참조]
```

- **MCP 호출**: 3회

#### T-1-24: ContextHintSystem 오브젝트 (G-04)

```
create_object
  name: "ContextHintSystem"
  parent: "TutorialSystem"

add_component
  target: "ContextHintSystem"
  component: "SeedMind.Tutorial.ContextHintSystem"
```

- **MCP 호출**: 2회

#### T-1-25: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

---

### T-1 검증 체크리스트

- [ ] 스크립트 15개 존재 (S-01 ~ S-15)
- [ ] Unity 컴파일 에러 없음 (콘솔 확인)
- [ ] `TutorialType` enum: 2개 값 (MainTutorial, SystemTutorial)
- [ ] `TutorialUIType` enum: 5개 값 (Bubble, Popup, Arrow, Highlight, Combined)
- [ ] `StepCompletionType` enum: 4개 값 (EventBased, TimeBased, ClickToContinue, Composite)
- [ ] `HintConditionType` enum: 8개 값 (DryTilesExist ~ ProcessingReady)
- [ ] `TutorialManager`가 MonoBehaviour(Singleton) 상속
- [ ] SCN_Farm에 "TutorialSystem" 오브젝트가 존재하고 TutorialManager 컴포넌트 부착됨
- [ ] "TutorialTriggerSystem"이 TutorialSystem 하위에 존재하고 _manager 참조가 연결됨
- [ ] "ContextHintSystem"이 TutorialSystem 하위에 존재

**T-1 MCP 호출 합계**: 2(폴더) + 9(enum/SO 스크립트) + 4(컴파일 대기) + 5(시스템 스크립트) + 1(UI 스크립트) + 2(컨테이너) + 1(컴포넌트 부착) + 3(TriggerSystem) + 2(ContextHintSystem) + 1(씬 저장) = ~30회

> 위 호출 수에서 스크립트 내용 작성은 `create_script` 1회로 카운트. 실제 스크립트의 필드/메서드 구현량이 크므로 MCP `create_script` 호출 시 content 파라미터가 길어질 수 있다.

---

## 7. 태스크 T-2: TutorialUI 프리팹 생성

**목적**: 튜토리얼 UI 전용 Canvas(Canvas_Tutorial)와 하위 패널들을 생성하고 프리팹으로 저장한다.

**전제**: T-1 완료 (TutorialUI.cs 컴파일 완료)

**의존 태스크**: T-1

---

### T-2-01: Canvas_Tutorial 생성 (G-04)

```
create_object
  name: "Canvas_Tutorial"
  parent: "--- UI ---"
  scene: "SCN_Farm"

add_component
  target: "Canvas_Tutorial"
  component: "Canvas"

set_property  target: "Canvas_Tutorial" -> Canvas
  renderMode = 0                            // Screen Space - Overlay
  sortingOrder = 40                         // (-> see docs/systems/ui-architecture.md 섹션 2.2)

add_component
  target: "Canvas_Tutorial"
  component: "CanvasScaler"

set_property  target: "Canvas_Tutorial" -> CanvasScaler
  uiScaleMode = 1                           // Scale With Screen Size
  referenceResolution = { x: 1920, y: 1080 }
  matchWidthOrHeight = 0.5

add_component
  target: "Canvas_Tutorial"
  component: "GraphicRaycaster"
```

> Sort Order 40은 canonical 정의를 따른다. (→ see `docs/systems/ui-architecture.md` 섹션 2.2)

- **MCP 호출**: 6회

### T-2-02: Panel_Dimmer + Mask_Highlight (G-06, G-07)

```
create_object
  name: "Panel_Dimmer"
  parent: "Canvas_Tutorial"

add_component
  target: "Panel_Dimmer"
  component: "Image"

set_property  target: "Panel_Dimmer" -> Image
  color = { r: 0, g: 0, b: 0, a: 0.5 }

set_property  target: "Panel_Dimmer" -> RectTransform
  anchorMin = { x: 0, y: 0 }
  anchorMax = { x: 1, y: 1 }
  offsetMin = { x: 0, y: 0 }
  offsetMax = { x: 0, y: 0 }

set_property  target: "Panel_Dimmer"
  activeSelf = false

create_object
  name: "Mask_Highlight"
  parent: "Panel_Dimmer"

set_property  target: "Mask_Highlight" -> RectTransform
  anchorMin = { x: 0.5, y: 0.5 }
  anchorMax = { x: 0.5, y: 0.5 }
  sizeDelta = { x: 200, y: 200 }
```

- **MCP 호출**: 7회

### T-2-03: Panel_Bubble (G-08)

```
create_object
  name: "Panel_Bubble"
  parent: "Canvas_Tutorial"

add_component
  target: "Panel_Bubble"
  component: "Image"

set_property  target: "Panel_Bubble" -> RectTransform
  pivot = { x: 0.5, y: 0 }                 // 하단 중앙 (말풍선 꼬리 위치)
  sizeDelta = { x: 400, y: 150 }

// 메시지 텍스트
create_object
  name: "Text_Message"
  parent: "Panel_Bubble"

add_component
  target: "Text_Message"
  component: "TMPro.TextMeshProUGUI"

set_property  target: "Text_Message" -> RectTransform
  anchorMin = { x: 0.1, y: 0.2 }
  anchorMax = { x: 0.9, y: 0.8 }
  offsetMin = { x: 0, y: 0 }
  offsetMax = { x: 0, y: 0 }

// 아이콘
create_object
  name: "Image_Icon"
  parent: "Panel_Bubble"

add_component
  target: "Image_Icon"
  component: "Image"

set_property  target: "Image_Icon"
  activeSelf = false

// 계속 버튼
create_object
  name: "Button_Continue"
  parent: "Panel_Bubble"

add_component
  target: "Button_Continue"
  component: "Button"

add_component
  target: "Button_Continue"
  component: "Image"

set_property  target: "Panel_Bubble"
  activeSelf = false
```

- **MCP 호출**: 12회

### T-2-04: Panel_Arrow (G-09)

```
create_object
  name: "Panel_Arrow"
  parent: "Canvas_Tutorial"

add_component
  target: "Panel_Arrow"
  component: "Image"

set_property  target: "Panel_Arrow" -> RectTransform
  sizeDelta = { x: 64, y: 64 }

set_property  target: "Panel_Arrow"
  activeSelf = false
```

- **MCP 호출**: 4회

### T-2-05: Panel_Popup (G-10)

```
create_object
  name: "Panel_Popup"
  parent: "Canvas_Tutorial"

add_component
  target: "Panel_Popup"
  component: "Image"

set_property  target: "Panel_Popup" -> RectTransform
  anchorMin = { x: 0.5, y: 0.5 }
  anchorMax = { x: 0.5, y: 0.5 }
  sizeDelta = { x: 600, y: 400 }

// 제목 텍스트
create_object
  name: "Text_Title"
  parent: "Panel_Popup"

add_component
  target: "Text_Title"
  component: "TMPro.TextMeshProUGUI"

// 본문 텍스트
create_object
  name: "Text_Body"
  parent: "Panel_Popup"

add_component
  target: "Text_Body"
  component: "TMPro.TextMeshProUGUI"

// 닫기 버튼
create_object
  name: "Button_Close"
  parent: "Panel_Popup"

add_component
  target: "Button_Close"
  component: "Button"

add_component
  target: "Button_Close"
  component: "Image"

set_property  target: "Panel_Popup"
  activeSelf = false
```

- **MCP 호출**: 10회

### T-2-06: Panel_Progress (G-11)

```
create_object
  name: "Panel_Progress"
  parent: "Canvas_Tutorial"

// 단계 카운터 텍스트
create_object
  name: "Text_StepCounter"
  parent: "Panel_Progress"

add_component
  target: "Text_StepCounter"
  component: "TMPro.TextMeshProUGUI"

set_property  target: "Text_StepCounter" -> TMPro.TextMeshProUGUI
  text = "1/12"

// 진행 슬라이더
create_object
  name: "Slider_Progress"
  parent: "Panel_Progress"

add_component
  target: "Slider_Progress"
  component: "Slider"

set_property  target: "Panel_Progress" -> RectTransform
  anchorMin = { x: 0.3, y: 0.9 }
  anchorMax = { x: 0.7, y: 0.95 }

set_property  target: "Panel_Progress"
  activeSelf = false
```

- **MCP 호출**: 8회

### T-2-07: TutorialUI 컴포넌트 부착 및 참조 연결

```
add_component
  target: "Canvas_Tutorial"
  component: "SeedMind.UI.TutorialUI"

set_property  target: "Canvas_Tutorial" -> TutorialUI
  _dimmerPanel = [Panel_Dimmer 참조]
  _highlightMask = [Mask_Highlight RectTransform 참조]
  _bubblePanel = [Panel_Bubble 참조]
  _messageText = [Text_Message TMP_Text 참조]
  _iconImage = [Image_Icon Image 참조]
  _continueButton = [Button_Continue Button 참조]
  _arrowPanel = [Panel_Arrow RectTransform 참조]
  _arrowImage = [Panel_Arrow Image 참조]
  _popupPanel = [Panel_Popup 참조]
  _popupTitle = [Text_Title TMP_Text 참조]
  _popupBody = [Text_Body TMP_Text 참조]
  _stepCounterText = [Text_StepCounter TMP_Text 참조]
  _progressSlider = [Slider_Progress Slider 참조]
```

[RISK] 개별 SerializeField 참조를 MCP `set_property`로 설정하는 것이 13개에 달한다. 단일 호출로 다중 프로퍼티를 설정할 수 있는지 MCP 도구 사양에 따라 호출 수가 달라진다.

- **MCP 호출**: 1(컴포넌트) + 13(참조) = 14회

### T-2-08: TutorialManager에 TutorialUI 참조 연결

```
set_property  target: "TutorialSystem" -> TutorialManager
  _tutorialUI = [Canvas_Tutorial의 TutorialUI 참조]
```

- **MCP 호출**: 1회

### T-2-09: 프리팹 저장

```
save_as_prefab
  source: "Canvas_Tutorial"
  path: "Assets/_Project/Prefabs/UI/PFB_UI_Tutorial.prefab"
```

- **MCP 호출**: 1회

### T-2-10: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

---

### T-2 검증 체크리스트

- [ ] `Canvas_Tutorial` 존재, Sort Order = 40 (-> see `docs/systems/ui-architecture.md` 섹션 2.2)
- [ ] `Canvas_Tutorial` Render Mode = Screen Space - Overlay
- [ ] Canvas Scaler: Scale With Screen Size, Reference 1920x1080
- [ ] `Panel_Dimmer` 하위에 `Mask_Highlight` 존재, 기본 비활성
- [ ] `Panel_Bubble` 하위에 Text_Message, Image_Icon, Button_Continue 존재, 기본 비활성
- [ ] `Panel_Arrow` 존재, 기본 비활성
- [ ] `Panel_Popup` 하위에 Text_Title, Text_Body, Button_Close 존재, sizeDelta 600x400, 기본 비활성
- [ ] `Panel_Progress` 하위에 Text_StepCounter, Slider_Progress 존재, 기본 비활성
- [ ] TutorialUI 컴포넌트의 SerializeField 참조 13개가 모두 연결됨
- [ ] TutorialManager._tutorialUI가 Canvas_Tutorial의 TutorialUI를 참조
- [ ] `PFB_UI_Tutorial.prefab` 프리팹 저장 완료
- [ ] 콘솔 에러 없음

**T-2 MCP 호출 합계**: 6(Canvas) + 7(Dimmer) + 12(Bubble) + 4(Arrow) + 10(Popup) + 8(Progress) + 14(TutorialUI 참조) + 1(Manager 참조) + 1(프리팹) + 1(씬 저장) = ~64회

> **주의**: 실제 호출 수는 T-2 개요(42회)보다 많아졌다. 이는 하위 UI 요소의 개별 생성/설정이 필요하기 때문이다. 일부 `set_property`가 다중 필드를 한 번에 설정할 수 있으면 호출 수를 줄일 수 있다.

---

## 8. 태스크 T-3: TutorialStepData SO 에셋 생성 (메인 12단계)

**목적**: 메인 튜토리얼 시퀀스(SEQ_MainTutorial)와 12개 단계(Step)의 SO 에셋을 생성한다.

**전제**: T-1 완료 (TutorialSequenceData, TutorialStepData 클래스 컴파일 완료)

**의존 태스크**: T-1

---

### T-3-01: 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Tutorial"

create_folder
  path: "Assets/_Project/Data/Tutorial/Sequences"

create_folder
  path: "Assets/_Project/Data/Tutorial/Steps"

create_folder
  path: "Assets/_Project/Data/Tutorial/Hints"
```

- **MCP 호출**: 4회

### T-3-02: TutorialSequenceData SO (메인 튜토리얼)

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialSequenceData"
  asset_path: "Assets/_Project/Data/Tutorial/Sequences/SO_TutSeq_MainTutorial.asset"

set_property  target: "SO_TutSeq_MainTutorial"
  sequenceId = "SEQ_MainTutorial"
  displayName = "메인 튜토리얼"
  tutorialType = 0                          // MainTutorial
  autoStartOnNewGame = true
  prerequisiteSequenceId = ""
  startTriggerType = 0                      // NewGame
  startTriggerParam = ""
  skippable = true
  pauseGameTime = false
```

- **MCP 호출**: 1(생성) + 9(필드) = 10회

### T-3-03: TutorialStepData SO 에셋 12개 생성

메인 튜토리얼 12단계 각각의 SO를 생성한다. 각 단계의 stepId, uiType, completionType, completionEventType 등의 **구체적 값**은 canonical 문서를 참조한다.

> (-> see `docs/systems/tutorial-system.md` 섹션 2.1 단계 상세)  
> (-> see `docs/systems/tutorial-architecture.md` 섹션 5.2 이벤트-단계 매핑 테이블)

각 SO 에셋의 공통 생성 패턴:

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialStepData"
  asset_path: "Assets/_Project/Data/Tutorial/Steps/<에셋명>.asset"

set_property  target: "<에셋명>"
  stepId = "<단계 ID>"
  uiType = <enum 값>                       // (-> see tutorial-architecture.md 섹션 4.2)
  completionType = <enum 값>               // (-> see tutorial-architecture.md 섹션 4.2)
  completionEventType = "<이벤트명>"        // (-> see tutorial-architecture.md 섹션 5.2)
  messageText = "[placeholder]"             // 콘텐츠 확정 후 업데이트
  blockOtherInput = <true/false>
  autoAdvanceDelay = <초>
```

#### Step 01: 농장 도착

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialStepData"
  asset_path: "Assets/_Project/Data/Tutorial/Steps/SO_TutStep_Main_01_Arrival.asset"

set_property  target: "SO_TutStep_Main_01_Arrival"
  stepId = "STEP_MainTutorial_01_Arrival"
  uiType = 1                                // Popup (시네마틱 연출)
  completionType = 1                        // TimeBased (자동 진행 또는 아무 키 스킵)
  completionEventType = ""                  // 시간 기반이므로 이벤트 불필요
  messageText = "[placeholder: 농장 도착 시네마틱]"
  autoAdvanceDelay = 5.0                    // 약 5초 후 자동 진행
  blockOtherInput = true
```

- **MCP 호출**: 1(생성) + 7(필드) = 8회

#### Step 02: 첫걸음 — 이동

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialStepData"
  asset_path: "Assets/_Project/Data/Tutorial/Steps/SO_TutStep_Main_02_Movement.asset"

set_property  target: "SO_TutStep_Main_02_Movement"
  stepId = "STEP_MainTutorial_02_Movement"
  uiType = 4                                // Combined (키 프롬프트 + 화살표)
  completionType = 0                        // EventBased (이동 감지)
  completionEventType = "PlayerEvents.OnMoveCompleted"
  messageText = "[placeholder: WASD 이동 안내]"
  showArrow = false
  showHighlight = false
  blockOtherInput = false
```

- **MCP 호출**: 8회

#### Step 03: 하나와의 만남

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialStepData"
  asset_path: "Assets/_Project/Data/Tutorial/Steps/SO_TutStep_Main_03_MeetHana.asset"

set_property  target: "SO_TutStep_Main_03_MeetHana"
  stepId = "STEP_MainTutorial_03_MeetHana"
  uiType = 4                                // Combined (NPC 마커 + 방향 화살표 + 퀘스트 텍스트)
  completionType = 0                        // EventBased
  completionEventType = "NPCEvents.OnDialogueStarted"
  completionParam = "npc_hana"
  messageText = "[placeholder: 하나를 찾아가세요]"
  showArrow = true
  anchorType = 0                            // WorldTarget
  anchorTargetId = "NPC_GeneralMerchant"
  blockOtherInput = false
```

- **MCP 호출**: 8회

#### Step 04: 땅 갈기

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialStepData"
  asset_path: "Assets/_Project/Data/Tutorial/Steps/SO_TutStep_Main_04_TillSoil.asset"

set_property  target: "SO_TutStep_Main_04_TillSoil"
  stepId = "STEP_MainTutorial_04_TillSoil"
  uiType = 4                                // Combined (도구바 하이라이트 + 타일 마커)
  completionType = 0                        // EventBased
  completionEventType = "FarmEvents.OnTileTilled"
  messageText = "[placeholder: 호미로 땅을 갈아 보세요]"
  showArrow = false
  showHighlight = true
  anchorType = 2                            // UIElement (도구바 호미 슬롯)
  anchorTargetId = "ToolBar_Slot_1"
  blockOtherInput = false
```

> **참고**: completionParam으로 목표 타일 수(3개)를 지정할 수 있다. 구체적 수치는 (-> see `docs/systems/tutorial-system.md` 섹션 2.1 S04, `tutorial_tillTarget`)

- **MCP 호출**: 8회

#### Step 05: 씨앗 심기

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialStepData"
  asset_path: "Assets/_Project/Data/Tutorial/Steps/SO_TutStep_Main_05_PlantSeeds.asset"

set_property  target: "SO_TutStep_Main_05_PlantSeeds"
  stepId = "STEP_MainTutorial_05_PlantSeeds"
  uiType = 4                                // Combined
  completionType = 0                        // EventBased
  completionEventType = "FarmEvents.OnCropPlanted"
  messageText = "[placeholder: 씨앗을 심어 보세요]"
  showHighlight = true
  anchorType = 2                            // UIElement (도구바 씨앗봉투 슬롯)
  anchorTargetId = "ToolBar_Slot_3"
  blockOtherInput = false
```

- **MCP 호출**: 8회

#### Step 06: 물주기

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialStepData"
  asset_path: "Assets/_Project/Data/Tutorial/Steps/SO_TutStep_Main_06_WaterCrops.asset"

set_property  target: "SO_TutStep_Main_06_WaterCrops"
  stepId = "STEP_MainTutorial_06_WaterCrops"
  uiType = 4                                // Combined
  completionType = 0                        // EventBased
  completionEventType = "FarmEvents.OnTileWatered"
  messageText = "[placeholder: 물을 주세요]"
  showHighlight = true
  anchorType = 2                            // UIElement (도구바 물뿌리개 슬롯)
  anchorTargetId = "ToolBar_Slot_2"
  blockOtherInput = false
```

- **MCP 호출**: 8회

#### Step 07: 수면

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialStepData"
  asset_path: "Assets/_Project/Data/Tutorial/Steps/SO_TutStep_Main_07_Sleep.asset"

set_property  target: "SO_TutStep_Main_07_Sleep"
  stepId = "STEP_MainTutorial_07_Sleep"
  uiType = 4                                // Combined (시계 하이라이트 + 집 방향 화살표)
  completionType = 0                        // EventBased
  completionEventType = "TimeEvents.OnSleepExecuted"
  messageText = "[placeholder: 집으로 돌아가 잠을 자세요]"
  showArrow = true
  anchorType = 0                            // WorldTarget (집/침대 위치)
  anchorTargetId = "PlayerHouse"
  blockOtherInput = false
```

- **MCP 호출**: 8회

#### Step 08: 성장 확인

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialStepData"
  asset_path: "Assets/_Project/Data/Tutorial/Steps/SO_TutStep_Main_08_GrowthCheck.asset"

set_property  target: "SO_TutStep_Main_08_GrowthCheck"
  stepId = "STEP_MainTutorial_08_GrowthCheck"
  uiType = 0                                // Bubble (정보 안내)
  completionType = 2                        // ClickToContinue (FarmEvents.OnCropInfoViewed 미정의 — [OPEN])
  completionEventType = ""
  messageText = "[placeholder: 작물 위에 마우스를 올려 보세요]"
  showArrow = true
  anchorType = 0                            // WorldTarget (농장 타일)
  anchorTargetId = "FarmGrid"
  blockOtherInput = false
```

- **MCP 호출**: 8회

#### Step 09: 첫 수확

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialStepData"
  asset_path: "Assets/_Project/Data/Tutorial/Steps/SO_TutStep_Main_09_Harvest.asset"

set_property  target: "SO_TutStep_Main_09_Harvest"
  stepId = "STEP_MainTutorial_09_Harvest"
  uiType = 4                                // Combined (타일 하이라이트 + 낫 도구바)
  completionType = 0                        // EventBased
  completionEventType = "FarmEvents.OnCropHarvested"
  messageText = "[placeholder: 작물을 수확하세요!]"
  showHighlight = true
  anchorType = 2                            // UIElement (도구바 낫 슬롯)
  anchorTargetId = "ToolBar_Slot_4"
  blockOtherInput = false
```

- **MCP 호출**: 8회

#### Step 10: 판매

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialStepData"
  asset_path: "Assets/_Project/Data/Tutorial/Steps/SO_TutStep_Main_10_FirstSale.asset"

set_property  target: "SO_TutStep_Main_10_FirstSale"
  stepId = "STEP_MainTutorial_10_FirstSale"
  uiType = 4                                // Combined (출하함/상점 방향 화살표)
  completionType = 0                        // EventBased
  completionEventType = "NPCEvents.OnShopOpened"
  messageText = "[placeholder: 수확물을 팔아 보세요]"
  showArrow = true
  anchorType = 0                            // WorldTarget
  anchorTargetId = "NPC_GeneralMerchant"
  blockOtherInput = false
```

- **MCP 호출**: 8회

#### Step 11: 재투자

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialStepData"
  asset_path: "Assets/_Project/Data/Tutorial/Steps/SO_TutStep_Main_11_Reinvest.asset"

set_property  target: "SO_TutStep_Main_11_Reinvest"
  stepId = "STEP_MainTutorial_11_Reinvest"
  uiType = 4                                // Combined
  completionType = 0                        // EventBased
  completionEventType = "EconomyEvents.OnItemPurchased"
  messageText = "[placeholder: 씨앗을 구매하세요]"
  showArrow = true
  anchorType = 0                            // WorldTarget
  anchorTargetId = "NPC_GeneralMerchant"
  blockOtherInput = false
```

- **MCP 호출**: 8회

#### Step 12: 완료

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialStepData"
  asset_path: "Assets/_Project/Data/Tutorial/Steps/SO_TutStep_Main_12_Complete.asset"

set_property  target: "SO_TutStep_Main_12_Complete"
  stepId = "STEP_MainTutorial_12_Complete"
  uiType = 1                                // Popup (완료 배너)
  completionType = 1                        // TimeBased (자동 종료)
  completionEventType = ""
  messageText = "[placeholder: 튜토리얼 완료!]"
  autoAdvanceDelay = 3.0                    // 완료 배너 표시 시간 (-> see tutorial-system.md 섹션 8)
  blockOtherInput = true
```

- **MCP 호출**: 8회

### T-3-04: 시퀀스 steps[] 배열 연결

SO_TutSeq_MainTutorial의 `steps` 배열에 12개 TutorialStepData SO를 순서대로 연결한다.

```
set_property  target: "SO_TutSeq_MainTutorial"
  steps[0]  = [SO_TutStep_Main_01_Arrival 참조]
  steps[1]  = [SO_TutStep_Main_02_Movement 참조]
  steps[2]  = [SO_TutStep_Main_03_MeetHana 참조]
  steps[3]  = [SO_TutStep_Main_04_TillSoil 참조]
  steps[4]  = [SO_TutStep_Main_05_PlantSeeds 참조]
  steps[5]  = [SO_TutStep_Main_06_WaterCrops 참조]
  steps[6]  = [SO_TutStep_Main_07_Sleep 참조]
  steps[7]  = [SO_TutStep_Main_08_GrowthCheck 참조]
  steps[8]  = [SO_TutStep_Main_09_Harvest 참조]
  steps[9]  = [SO_TutStep_Main_10_FirstSale 참조]
  steps[10] = [SO_TutStep_Main_11_Reinvest 참조]
  steps[11] = [SO_TutStep_Main_12_Complete 참조]
```

[RISK] SO 배열에 다른 SO 참조를 MCP `set_property`로 순서대로 설정하는 것이 가능한지 사전 검증 필요. 불가능한 경우 Editor 스크립트를 통한 우회가 필요하다. (farming-architecture.md의 동일 리스크와 연동)

- **MCP 호출**: 12회 (개별 인덱스 설정) 또는 1회 (배열 일괄 설정 가능한 경우)

---

### T-3 검증 체크리스트

- [ ] `Assets/_Project/Data/Tutorial/Sequences/` 폴더 존재
- [ ] `Assets/_Project/Data/Tutorial/Steps/` 폴더 존재
- [ ] `Assets/_Project/Data/Tutorial/Hints/` 폴더 존재
- [ ] `SO_TutSeq_MainTutorial.asset` 존재, sequenceId = "SEQ_MainTutorial", tutorialType = MainTutorial, autoStartOnNewGame = true
- [ ] TutorialStepData SO 12개 존재 (SO_TutStep_Main_01_Arrival ~ SO_TutStep_Main_12_Complete)
- [ ] 각 SO의 stepId가 "STEP_MainTutorial_XX_Name" 패턴을 따름
- [ ] SO_TutSeq_MainTutorial.steps[] 배열에 12개 SO가 순서대로 연결됨
- [ ] Step 01, 12의 completionType = TimeBased (자동 진행)
- [ ] Step 02~11의 completionType = EventBased
- [ ] 콘솔 에러 없음

**T-3 MCP 호출 합계**: 4(폴더) + 10(시퀀스) + 96(Step 12개 x 8) + 12(배열 연결) = ~122회

> **주의**: 호출 수가 상당히 많다. Editor 스크립트(CreateTutorialStepAssets.cs)를 통한 일괄 생성으로 Step SO 12개를 ~3회(스크립트 생성 + 실행 + 검증)로 줄이는 것을 강력히 권장한다.

---

## 9. 태스크 T-4: 시스템 튜토리얼 SO 에셋 생성 (4종)

**목적**: 시설 건설, 도구 업그레이드, 계절 전환, 가공소의 시스템 튜토리얼 시퀀스 SO를 생성한다.

**전제**: T-1 완료 (TutorialSequenceData 클래스 컴파일 완료), T-3-01(폴더 생성) 완료

**의존 태스크**: T-1, T-3

---

### T-4-01: 시스템 튜토리얼 시퀀스 SO 4종 생성

각 시퀀스의 트리거 조건과 내용은 canonical 문서를 참조한다. (-> see `docs/systems/tutorial-architecture.md` 섹션 1.3)

> **참고**: 시스템 튜토리얼의 개별 Step SO는 본 태스크에서 생성하지 않는다. 각 시스템 튜토리얼의 세부 단계는 해당 시스템 콘텐츠가 확정된 후 별도 태스크로 생성한다.

#### SEQ_BuildingIntro

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialSequenceData"
  asset_path: "Assets/_Project/Data/Tutorial/Sequences/SO_TutSeq_BuildingIntro.asset"

set_property  target: "SO_TutSeq_BuildingIntro"
  sequenceId = "SEQ_BuildingIntro"
  displayName = "시설 건설 소개"
  tutorialType = 1                          // SystemTutorial
  autoStartOnNewGame = false
  prerequisiteSequenceId = "SEQ_MainTutorial"
  startTriggerType = 3                      // EventFired
  startTriggerParam = "ProgressionEvents.OnBuildingUnlocked"
  skippable = true
  pauseGameTime = false
  steps = []                                // Step SO는 추후 연결
```

- **MCP 호출**: 1(생성) + 9(필드) = 10회

#### SEQ_ToolUpgradeIntro

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialSequenceData"
  asset_path: "Assets/_Project/Data/Tutorial/Sequences/SO_TutSeq_ToolUpgradeIntro.asset"

set_property  target: "SO_TutSeq_ToolUpgradeIntro"
  sequenceId = "SEQ_ToolUpgradeIntro"
  displayName = "도구 업그레이드 소개"
  tutorialType = 1                          // SystemTutorial
  autoStartOnNewGame = false
  prerequisiteSequenceId = "SEQ_MainTutorial"
  startTriggerType = 2                      // FirstVisit (대장간 첫 방문)
  startTriggerParam = "Blacksmith"
  skippable = true
  pauseGameTime = false
  steps = []
```

- **MCP 호출**: 10회

#### SEQ_SeasonChange

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialSequenceData"
  asset_path: "Assets/_Project/Data/Tutorial/Sequences/SO_TutSeq_SeasonChange.asset"

set_property  target: "SO_TutSeq_SeasonChange"
  sequenceId = "SEQ_SeasonChange"
  displayName = "계절 전환 안내"
  tutorialType = 1                          // SystemTutorial
  autoStartOnNewGame = false
  prerequisiteSequenceId = "SEQ_MainTutorial"
  startTriggerType = 3                      // EventFired
  startTriggerParam = "TimeEvents.OnSeasonChanged"
  skippable = true
  pauseGameTime = false
  steps = []
```

- **MCP 호출**: 10회

#### SEQ_ProcessingIntro

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.TutorialSequenceData"
  asset_path: "Assets/_Project/Data/Tutorial/Sequences/SO_TutSeq_ProcessingIntro.asset"

set_property  target: "SO_TutSeq_ProcessingIntro"
  sequenceId = "SEQ_ProcessingIntro"
  displayName = "가공 시스템 소개"
  tutorialType = 1                          // SystemTutorial
  autoStartOnNewGame = false
  prerequisiteSequenceId = "SEQ_MainTutorial"
  startTriggerType = 2                      // FirstVisit (가공소 첫 사용)
  startTriggerParam = "Processor"
  skippable = true
  pauseGameTime = false
  steps = []
```

- **MCP 호출**: 10회

---

### T-4-02: TutorialManager._allSequences 배열 연결

모든 시퀀스 SO(메인 1 + 시스템 4 = 5개)를 TutorialManager에 등록한다.

```
set_property  target: "TutorialSystem" -> TutorialManager
  _allSequences[0] = [SO_TutSeq_MainTutorial 참조]
  _allSequences[1] = [SO_TutSeq_BuildingIntro 참조]
  _allSequences[2] = [SO_TutSeq_ToolUpgradeIntro 참조]
  _allSequences[3] = [SO_TutSeq_SeasonChange 참조]
  _allSequences[4] = [SO_TutSeq_ProcessingIntro 참조]
```

[RISK] T-3-04와 동일한 SO 배열 참조 설정 리스크.

- **MCP 호출**: 5회 (개별 인덱스) 또는 1회 (배열 일괄)

---

### T-4 검증 체크리스트

- [ ] 시스템 튜토리얼 SO 4개 존재: BuildingIntro, ToolUpgradeIntro, SeasonChange, ProcessingIntro
- [ ] 모든 시스템 튜토리얼의 tutorialType = 1 (SystemTutorial)
- [ ] 모든 시스템 튜토리얼의 prerequisiteSequenceId = "SEQ_MainTutorial"
- [ ] TutorialManager._allSequences에 5개 시퀀스가 등록됨
- [ ] 콘솔 에러 없음

**T-4 MCP 호출 합계**: 40(시퀀스 4 x 10) + 5(배열 연결) = ~45회

---

## 10. 태스크 T-5: ContextHintData SO 에셋 생성 (7종)

**목적**: 상황별 힌트 7종의 SO 에셋을 생성하고 ContextHintSystem에 등록한다.

**전제**: T-1 완료 (ContextHintData, ContextHintSystem 클래스 컴파일 완료), T-3-01(폴더 생성) 완료

**의존 태스크**: T-1, T-3

---

### T-5-01: ContextHintData SO 7종 생성

각 힌트의 메시지 텍스트, 쿨다운, 우선순위 등은 canonical 문서를 참조한다. (-> see `docs/systems/tutorial-system.md` 섹션 7.2, 7.3, 7.4)

> **참고**: tutorial-system.md에 정의된 힌트는 총 17종 이상이나, 본 태스크에서는 tutorial-architecture.md MCP-4에서 명시한 **핵심 7종**만 생성한다. 나머지는 콘텐츠 확장 태스크에서 추가한다.

#### HINT_WaterReminder (물 안 준 타일 경고)

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.ContextHintData"
  asset_path: "Assets/_Project/Data/Tutorial/Hints/SO_CtxHint_WaterReminder.asset"

set_property  target: "SO_CtxHint_WaterReminder"
  hintId = "HINT_WaterReminder"
  conditionType = 0                         // DryTilesExist
  conditionParam = ""                       // 임계값은 런타임에서 설정 (-> see tutorial-system.md 섹션 7.4)
  messageText = "[placeholder: 작물이 목말라하고 있어요!]"
  displayDuration = 0.0                     // (-> see tutorial-system.md 섹션 7.4 hintDisplayDuration)
  cooldownDays = 0                          // (-> see tutorial-system.md 섹션 7.4)
  maxShowCount = 1
  requireTutorialComplete = false
  priority = 5
```

> **주의**: `displayDuration`, `cooldownDays`의 구체적 수치는 MCP 실행 시점에 canonical 문서(`docs/systems/tutorial-system.md` 섹션 7.4)에서 읽어 입력한다. 본 문서에서는 placeholder(0)로 표기한다 (PATTERN-006).

- **MCP 호출**: 1(생성) + 9(필드) = 10회

#### HINT_LowGold (골드 부족)

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.ContextHintData"
  asset_path: "Assets/_Project/Data/Tutorial/Hints/SO_CtxHint_LowGold.asset"

set_property  target: "SO_CtxHint_LowGold"
  hintId = "HINT_LowGold"
  conditionType = 1                         // LowGold
  conditionParam = ""
  messageText = "[placeholder: 돈이 부족하면 작물을 팔아 보세요]"
  displayDuration = 0.0                     // (-> see tutorial-system.md 섹션 7.4)
  cooldownDays = 0                          // (-> see tutorial-system.md 섹션 7.4)
  maxShowCount = 1
  requireTutorialComplete = true
  priority = 3
```

- **MCP 호출**: 10회

#### HINT_InventoryFull (인벤토리 가득)

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.ContextHintData"
  asset_path: "Assets/_Project/Data/Tutorial/Hints/SO_CtxHint_InventoryFull.asset"

set_property  target: "SO_CtxHint_InventoryFull"
  hintId = "HINT_InventoryFull"
  conditionType = 2                         // InventoryFull
  conditionParam = ""
  messageText = "[placeholder: 배낭이 가득 찼어요!]"
  displayDuration = 0.0                     // (-> see tutorial-system.md 섹션 7.4)
  cooldownDays = 0                          // (-> see tutorial-system.md 섹션 7.4)
  maxShowCount = 1
  requireTutorialComplete = false
  priority = 7
```

- **MCP 호출**: 10회

#### HINT_SeasonCrop (계절 불일치 작물)

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.ContextHintData"
  asset_path: "Assets/_Project/Data/Tutorial/Hints/SO_CtxHint_SeasonCrop.asset"

set_property  target: "SO_CtxHint_SeasonCrop"
  hintId = "HINT_SeasonCrop"
  conditionType = 3                         // SeasonMismatchCrop
  conditionParam = ""
  messageText = "[placeholder: 이 씨앗은 지금 계절에 맞지 않아요]"
  displayDuration = 0.0                     // (-> see tutorial-system.md 섹션 7.4)
  cooldownDays = 0                          // (-> see tutorial-system.md 섹션 7.4)
  maxShowCount = 1
  requireTutorialComplete = false
  priority = 6
```

- **MCP 호출**: 10회

#### HINT_HarvestReady (수확 가능 방치)

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.ContextHintData"
  asset_path: "Assets/_Project/Data/Tutorial/Hints/SO_CtxHint_HarvestReady.asset"

set_property  target: "SO_CtxHint_HarvestReady"
  hintId = "HINT_HarvestReady"
  conditionType = 5                         // ReadyToHarvest
  conditionParam = ""
  messageText = "[placeholder: 수확 가능한 작물이 있어요!]"
  displayDuration = 0.0                     // (-> see tutorial-system.md 섹션 7.4)
  cooldownDays = 0                          // (-> see tutorial-system.md 섹션 7.4)
  maxShowCount = 0                          // 무제한 (쿨다운으로 빈도 제한)
  requireTutorialComplete = true
  priority = 4
```

- **MCP 호출**: 10회

#### HINT_NightWarning (야간 경고)

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.ContextHintData"
  asset_path: "Assets/_Project/Data/Tutorial/Hints/SO_CtxHint_NightWarning.asset"

set_property  target: "SO_CtxHint_NightWarning"
  hintId = "HINT_NightWarning"
  conditionType = 6                         // NightTime
  conditionParam = ""
  messageText = "[placeholder: 밤이 늦었어요. 집으로 돌아가세요]"
  displayDuration = 0.0                     // (-> see tutorial-system.md 섹션 7.4)
  cooldownDays = 0                          // (-> see tutorial-system.md 섹션 7.4)
  maxShowCount = 0                          // 무제한 (매일 야간에 표시 가능)
  requireTutorialComplete = false
  priority = 8
```

- **MCP 호출**: 10회

#### HINT_ProcessingReady (가공 완료 대기)

```
create_scriptable_object
  type: "SeedMind.Tutorial.Data.ContextHintData"
  asset_path: "Assets/_Project/Data/Tutorial/Hints/SO_CtxHint_ProcessingReady.asset"

set_property  target: "SO_CtxHint_ProcessingReady"
  hintId = "HINT_ProcessingReady"
  conditionType = 7                         // ProcessingReady
  conditionParam = ""
  messageText = "[placeholder: 가공이 완료되었어요!]"
  displayDuration = 0.0                     // (-> see tutorial-system.md 섹션 7.4)
  cooldownDays = 0                          // (-> see tutorial-system.md 섹션 7.4)
  maxShowCount = 0                          // 무제한
  requireTutorialComplete = true
  priority = 4
```

- **MCP 호출**: 10회

---

### T-5-02: ContextHintSystem._allHints 배열 연결

```
set_property  target: "ContextHintSystem" -> ContextHintSystem
  _allHints[0] = [SO_CtxHint_WaterReminder 참조]
  _allHints[1] = [SO_CtxHint_LowGold 참조]
  _allHints[2] = [SO_CtxHint_InventoryFull 참조]
  _allHints[3] = [SO_CtxHint_SeasonCrop 참조]
  _allHints[4] = [SO_CtxHint_HarvestReady 참조]
  _allHints[5] = [SO_CtxHint_NightWarning 참조]
  _allHints[6] = [SO_CtxHint_ProcessingReady 참조]
```

- **MCP 호출**: 7회 (개별) 또는 1회 (배열 일괄)

### T-5-03: ContextHintSystem._tutorialUI 참조 연결

```
set_property  target: "ContextHintSystem" -> ContextHintSystem
  _tutorialUI = [Canvas_Tutorial의 TutorialUI 참조]
```

- **MCP 호출**: 1회

### T-5-04: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

---

### T-5 검증 체크리스트

- [ ] ContextHintData SO 7개 존재: WaterReminder, LowGold, InventoryFull, SeasonCrop, HarvestReady, NightWarning, ProcessingReady
- [ ] 각 SO의 hintId가 "HINT_" 접두사를 따름
- [ ] conditionType 값이 HintConditionType enum과 일치:
  - WaterReminder = 0 (DryTilesExist)
  - LowGold = 1
  - InventoryFull = 2
  - SeasonCrop = 3 (SeasonMismatchCrop)
  - HarvestReady = 5 (ReadyToHarvest)
  - NightWarning = 6 (NightTime)
  - ProcessingReady = 7
- [ ] ContextHintSystem._allHints에 7개 SO가 등록됨
- [ ] ContextHintSystem._tutorialUI 참조가 연결됨
- [ ] 콘솔 에러 없음

**T-5 MCP 호출 합계**: 70(힌트 7 x 10) + 7(배열 연결) + 1(UI 참조) + 1(씬 저장) = ~79회

---

## 11. 태스크 T-6: 통합 테스트

**목적**: 튜토리얼 시스템의 초기화, 이벤트 구독, 시퀀스 진행, UI 표시, 세이브/로드를 검증한다.

**전제**: T-1~T-5 완료

**의존 태스크**: T-1, T-2, T-3, T-4, T-5

---

### T-6-01: Play Mode 진입

```
enter_play_mode
```

- **MCP 호출**: 1회

### T-6-02: 초기화 확인

```
get_console_logs
  filter: "TutorialManager"
```

**예상 로그**:

- `"TutorialManager initialized, 5 sequences loaded"` -- 시퀀스 5개 (메인 1 + 시스템 4)
- `"ContextHintSystem initialized, 7 hints loaded"` -- 힌트 7개
- `"TutorialTriggerSystem: event subscriptions complete"` -- 이벤트 구독 완료

- **MCP 호출**: 1회

### T-6-03: 메인 튜토리얼 자동 시작 확인

새 게임 시작 시 SEQ_MainTutorial이 자동으로 시작되는지 확인한다.

```
get_console_logs
  filter: "OnSequenceStarted"
```

**예상 로그**:

- `"TutorialEvents.OnSequenceStarted: SEQ_MainTutorial"`
- `"TutorialEvents.OnStepStarted: SEQ_MainTutorial, STEP_MainTutorial_01_Arrival"`

- **MCP 호출**: 1회

### T-6-04: 이벤트 트리거 테스트 (경작)

호미로 타일 경작 시 TutorialTriggerSystem이 FarmEvents.OnTileTilled를 수신하는지 확인한다.

```
execute_method
  type: "SeedMind.Farm.FarmGrid"
  method: "DebugTillTile"
  params: { "x": 3, "y": 3 }

get_console_logs
  filter: "TileTilled"
```

**예상 로그**:

- `"FarmEvents.OnTileTilled: (3,3)"`
- `"TutorialTriggerSystem: HandleTileTilled matched active step"`

- **MCP 호출**: 2회

### T-6-05: 시퀀스 스킵 테스트

```
execute_method
  type: "SeedMind.Tutorial.TutorialManager"
  method: "SkipSequence"

get_console_logs
  filter: "OnSequenceSkipped"
```

**예상 로그**:

- `"TutorialEvents.OnSequenceSkipped: SEQ_MainTutorial"`
- TutorialManager 상태가 Idle로 복귀

- **MCP 호출**: 2회

### T-6-06: UI 표시 확인

TutorialUI의 패널 활성화/비활성화 동작을 확인한다.

```
get_console_logs
  filter: "TutorialUI"
```

**예상 로그**:

- `"TutorialUI.ShowStep: STEP_MainTutorial_01_Arrival"` (Play Mode 진입 시 자동 시작된 경우)
- 패널 활성화/비활성화 로그

- **MCP 호출**: 1회

### T-6-07: 세이브/로드 테스트

```
execute_method
  type: "SeedMind.Tutorial.TutorialManager"
  method: "ExportSaveData"

get_console_logs
  filter: "SaveData"
```

**예상 로그**:

- `"TutorialSaveData exported: completedSequences=1, activeSequence=none"`

- **MCP 호출**: 2회

### T-6-08: Play Mode 종료

```
exit_play_mode
```

- **MCP 호출**: 1회

### T-6-09: 씬 저장

```
save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 1회

---

### T-6 검증 체크리스트

- [ ] TutorialManager 초기화 성공 (5개 시퀀스 로드)
- [ ] ContextHintSystem 초기화 성공 (7개 힌트 로드)
- [ ] TutorialTriggerSystem 이벤트 구독 완료 (FarmEvents, BuildingEvents, NPCEvents, ToolUpgradeEvents)
- [ ] 새 게임 시 SEQ_MainTutorial 자동 시작
- [ ] FarmEvents.OnTileTilled 이벤트가 TutorialTriggerSystem에 수신됨
- [ ] SkipSequence 호출 시 상태가 Idle로 복귀하고 OnSequenceSkipped 이벤트 발행됨
- [ ] TutorialUI 패널 표시/숨기기 동작 확인
- [ ] ExportSaveData 호출 시 직렬화 데이터 정상 생성
- [ ] 콘솔에 에러/경고 없음

**T-6 MCP 호출 합계**: 12회

---

## Cross-references

| 관련 문서 | 연관 내용 |
|-----------|-----------|
| `docs/systems/tutorial-architecture.md` (DES-006) | Part I 클래스 설계, Part II MCP 요약 -- 본 문서의 원본 |
| `docs/systems/tutorial-system.md` (DES-006) | 메인 12단계 상세, 컨텍스트 힌트 목록, 튜닝 파라미터 canonical |
| `docs/systems/ui-architecture.md` | Canvas 정렬 우선순위 (섹션 2.2), Canvas_Tutorial Sort Order = 40 canonical |
| `docs/mcp/scene-setup-tasks.md` (ARC-002) | 폴더 구조, SCN_Farm 기본 계층 |
| `docs/mcp/farming-tasks.md` (ARC-003) | FarmGrid, FarmEvents -- 튜토리얼 트리거 대상 |
| `docs/mcp/facilities-tasks.md` (ARC-007) | BuildingEvents -- 시설 튜토리얼 트리거 |
| `docs/mcp/npc-shop-tasks.md` (ARC-009) | NPCEvents, DialogueSystem -- NPC 대화 트리거 |
| `docs/mcp/tool-upgrade-tasks.md` (ARC-015) | ToolUpgradeEvents -- 업그레이드 튜토리얼 트리거 |
| `docs/systems/project-structure.md` | 네임스페이스 규칙, 폴더 구조 (Tutorial 모듈) |
| `docs/pipeline/data-pipeline.md` (섹션 3.2) | 마스터 세이브 스키마 -- tutorial 필드 |
| `docs/balance/progression-curve.md` (섹션 1.2.4) | 첫 수확 보너스 XP -- S09 보상 참조 |

---

## Open Questions

1. [OPEN] **Step 08 완료 조건**: `FarmEvents.OnCropInfoViewed` 이벤트가 farming-architecture.md에 정의되어 있지 않아 현재 ClickToContinue로 임시 처리. 호버 팝업 표시를 감지하는 이벤트를 FarmEvents에 추가해야 한다.

2. [OPEN] **시스템 튜토리얼 Step SO 미생성**: T-4에서 4개 시스템 튜토리얼의 시퀀스 SO만 생성하고 개별 Step SO는 생성하지 않았다. 각 시스템 튜토리얼의 세부 단계 콘텐츠가 확정된 후 별도 태스크가 필요하다.

3. [OPEN] **ContextHint 확장**: tutorial-system.md에는 17종 이상의 힌트가 정의되어 있으나 본 태스크에서는 핵심 7종만 생성한다. 나머지 힌트(hint_dry_crop, hint_withered, hint_rain_water, hint_season_warning, hint_wrong_season, hint_crop_rotation, hint_first_levelup, hint_quality_crop, hint_water_tank, hint_greenhouse 등)의 생성 시점 결정 필요.

4. [OPEN] **Editor 스크립트 우회**: T-3의 Step SO 12개 생성이 ~96회 MCP 호출을 요구한다. Editor 스크립트(CreateTutorialStepAssets.cs)를 통한 일괄 생성 전략의 구체적 설계가 필요하다.

5. [OPEN] **completionEventType 문자열 매핑**: Step SO의 completionEventType 필드에 문자열로 이벤트를 지정하는 방식은 런타임에서 리플렉션 또는 switch 분기가 필요하다. enum 기반으로 변경할지, 문자열 매핑 테이블을 유지할지 결정 필요.

---

## Risks

1. [RISK] **SO 배열 참조 설정 불가**: MCP `set_property`로 SO 배열(steps[], _allHints[], _allSequences[])에 다른 SO 참조를 순서대로 설정하는 것이 미지원일 수 있다. 이 경우 Editor 스크립트를 통한 우회가 필수이다. farming-tasks.md, facilities-tasks.md의 동일 리스크와 연동.

2. [RISK] **총 MCP 호출 수 과다**: 전체 태스크 합계가 ~350회에 달한다 (T-3, T-5의 SO 개별 필드 설정이 주요 원인). Editor 스크립트를 통한 일괄 생성으로 대폭 감소 가능하나, Editor 스크립트 자체의 작성/컴파일 비용이 추가된다.

3. [RISK] **static event 구독 누수**: TutorialTriggerSystem이 FarmEvents, BuildingEvents, NPCEvents, ToolUpgradeEvents 등 다수의 이벤트를 구독하므로, OnDisable 해제 누락 시 메모리 누수 및 null reference 발생 가능. 씬 전환 시 전체 이벤트 초기화 루틴 필요. (-> see `docs/systems/tutorial-architecture.md` [RISK])

4. [RISK] **입력 차단 오버레이 충돌**: blockOtherInput 활성화 시 다른 UI(인벤토리, 상점)의 Raycast가 차단되는 것은 의도적이나, 비정상 상태에서의 복구 로직이 필요하다. (-> see `docs/systems/tutorial-architecture.md` [RISK])

5. [RISK] **이벤트 미정의 (3건)**: S07 수면(`TimeEvents.OnSleepExecuted`), S08 성장 확인(ClickToContinue 임시 처리), S11 구매 완료(`EconomyEvents.OnItemPurchased` 정확한 이름 미확정) — 해당 이벤트를 canonical 아키텍처 문서에 먼저 추가해야 MCP SO 값 확정 가능.

---

*이 문서는 Claude Code가 ARC-010 태스크에 따라, facilities-tasks.md 및 npc-shop-tasks.md의 패턴을 계승하여 자율적으로 작성했습니다.*
