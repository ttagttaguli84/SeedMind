# 튜토리얼/온보딩 시스템 기술 아키텍처

> 튜토리얼 매니저, 단계 데이터 구조, 트리거 시스템, UI 컴포넌트, 세이브 데이터, 상황별 힌트, 이벤트 허브의 클래스 설계 및 MCP 구현 계획  
> 작성: Claude Code (Opus) | 2026-04-06  
> 문서 ID: DES-006

---

## Context

이 문서는 SeedMind의 튜토리얼/온보딩 시스템에 대한 기술 아키텍처를 정의한다. 새로운 플레이어가 게임의 핵심 루프(경작, 도구 사용, 상점 거래, 시설 건설)를 자연스럽게 학습할 수 있도록 안내하는 시스템이다. 기존 이벤트 버스 패턴(FarmEvents, BuildingEvents, NPCEvents, ToolUpgradeEvents)을 활용하여 게임 내 행동을 감지하고, ScriptableObject 기반으로 튜토리얼 단계를 데이터 드리븐으로 구성한다.

**설계 목표**:
- 기존 게임플레이 시스템(Farm, Player, Economy, Building)의 코드를 수정하지 않고, 이벤트 구독만으로 튜토리얼을 구동한다
- 튜토리얼 단계를 ScriptableObject로 정의하여, 코드 변경 없이 순서/조건/메시지를 조정할 수 있다
- 튜토리얼 진행 상태를 세이브/로드하여 중간 이탈 후 복귀해도 이어서 진행 가능하다
- 강제 튜토리얼(첫 플레이)과 상황별 힌트(반복 노출 가능)를 구분한다

---

# Part I -- 시스템 설계

---

## 1. 전체 흐름

```
[게임 시작 / 세이브 로드]
    │
    ▼
┌──────────────────────┐
│   TutorialManager    │  (싱글턴, 튜토리얼 상태 관리)
│   ─ 현재 활성 시퀀스  │
│   ─ 현재 단계 인덱스  │
│   ─ 완료된 시퀀스 목록 │
└──────────┬───────────┘
           │
    ┌──────┴──────┐
    ▼             ▼
┌────────────┐  ┌─────────────────┐
│ 메인 튜토   │  │ ContextHint     │
│ 리얼 시퀀스 │  │ System          │
│ (강제/순차) │  │ (상황별 자동 팁) │
└─────┬──────┘  └────────┬────────┘
      │                  │
      ▼                  ▼
┌──────────────────────────────┐
│       TutorialUI             │
│  ─ 말풍선, 화살표, 하이라이트 │
│  ─ 팝업 패널, 진행 표시기     │
└──────────────────────────────┘
```

### 1.1 튜토리얼 유형 분류

| 유형 | 영문 키 | 특성 | 트리거 조건 |
|------|---------|------|-------------|
| 메인 튜토리얼 | `MainTutorial` | 순차적, 첫 플레이 시 강제 진행 | 새 게임 시작 시 자동 |
| 시스템 튜토리얼 | `SystemTutorial` | 특정 시스템 첫 접근 시 1회 | 해당 시스템 해금/첫 사용 시 |
| 상황별 힌트 | `ContextHint` | 반복 가능, 닫기 가능, 빈도 제한 | 특정 게임 상태 감지 시 |

### 1.2 메인 튜토리얼 시퀀스 (개요)

게임 첫 시작 시 아래 순서로 진행된다. 각 단계는 TutorialStepData SO로 정의한다. 단계별 상세 내용은 canonical 문서를 참조한다. (→ see `docs/systems/tutorial-system.md` 섹션 2.1)

```
시퀀스: SEQ_MainTutorial
├── Step 01 (S01): 농장 도착 — 시네마틱/스킵 분기
├── Step 02 (S02): 첫걸음 — 이동 배우기 (WASD)
├── Step 03 (S03): 하나와의 만남 — 상점 방문 (NPC 인터랙션, 씨앗 수령)
├── Step 04 (S04): 땅 갈기 — 호미 사용 (타일 Tilled 전환)
├── Step 05 (S05): 씨앗 심기 — 첫 파종 (Tilled → Planted)
├── Step 06 (S06): 물주기 — 성장의 시작 (Planted → Watered)
├── Step 07 (S07): 하루의 끝 — 수면과 시간 (수면 실행)
├── Step 08 (S08): 성장 확인 — 새싹 발견 (타일 호버 팝업 확인)
├── Step 09 (S09): 첫 수확 — 수확의 기쁨 (Harvestable → 수확)
├── Step 10 (S10): 출하와 판매 — 첫 수입 (작물 판매)
├── Step 11 (S11): 재투자 — 씨앗 구매 (상점 구매)
└── Step 12 (S12): 튜토리얼 완료 — 자유의 시작
```

시퀀스 내 구체적 메시지 텍스트와 NPC 대사는 콘텐츠 문서에서 정의한다. 본 문서는 구조와 흐름만 기술한다.

### 1.3 시스템 튜토리얼 목록

| 시퀀스 ID | 트리거 | 내용 |
|-----------|--------|------|
| `SEQ_BuildingIntro` | 첫 시설 해금 시 | 건설 모드 진입, 배치, 확인 |
| `SEQ_ToolUpgradeIntro` | 대장간 첫 방문 시 | 업그레이드 의뢰 흐름 |
| `SEQ_SeasonChange` | 첫 계절 전환 시 | 계절별 재배 가능 작물 안내 |
| `SEQ_ProcessingIntro` | 가공소 첫 사용 시 | 가공 레시피 선택, 결과물 수령 |

---

## 2. 클래스 다이어그램

```
                    ┌──────────────────────┐
                    │    GameManager       │
                    │    (singleton)       │
                    └──────────┬───────────┘
                               │ references
               ┌───────────────┼───────────────┐
               ▼               ▼               ▼
    ┌────────────────┐ ┌──────────────┐ ┌──────────────┐
    │ TutorialManager│ │  TimeManager │ │  SaveManager │
    │ (singleton)    │ │              │ │              │
    └───────┬────────┘ └──────────────┘ └──────────────┘
            │
    ┌───────┼────────────────────┐
    │       │                    │
    ▼       ▼                    ▼
┌──────┐ ┌───────────────┐ ┌───────────────────┐
│Tuto  │ │ContextHint    │ │ TutorialUI        │
│rial  │ │System         │ │ (MonoBehaviour)   │
│Trigger│ │(MonoBehaviour)│ │                   │
│System│ │               │ │ ─ ArrowIndicator  │
│      │ │ ─ hintRules[] │ │ ─ HighlightOverlay│
│      │ │ ─ cooldowns   │ │ ─ DialogueBubble  │
└──┬───┘ └───────────────┘ │ ─ ProgressBar     │
   │                       └───────────────────┘
   │ 이벤트 구독
   ▼
┌────────────────────────────────────────────┐
│  기존 이벤트 허브들 (수정 없음)               │
│  FarmEvents / BuildingEvents / NPCEvents   │
│  ToolUpgradeEvents / ProgressionEvents     │
└────────────────────────────────────────────┘

데이터 (ScriptableObject):
┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│TutorialSequence  │  │TutorialStepData  │  │ContextHintData   │
│Data (SO)         │──│(SO)              │  │(SO)              │
│                  │  │                  │  │                  │
│ sequenceId       │  │ stepId           │  │ hintId           │
│ steps[]          │  │ triggerEventType │  │ conditionType    │
│ autoStart        │  │ completionEvent  │  │ message          │
│ prerequisite     │  │ uiConfig         │  │ cooldownDays     │
└──────────────────┘  └──────────────────┘  └──────────────────┘
```

### 클래스 책임 요약

| 클래스 | 유형 | 네임스페이스 | 책임 |
|--------|------|-------------|------|
| **TutorialManager** | MonoBehaviour (Singleton) | SeedMind.Tutorial | 튜토리얼 상태 관리, 시퀀스 진행 제어, 세이브/로드 연동 |
| **TutorialTriggerSystem** | MonoBehaviour | SeedMind.Tutorial | 이벤트 버스 구독, 트리거 조건 판정, 시퀀스/단계 활성화 요청 |
| **ContextHintSystem** | MonoBehaviour | SeedMind.Tutorial | 게임 상태 주기적 체크, 상황별 힌트 표시 판정, 쿨다운 관리 |
| **TutorialUI** | MonoBehaviour | SeedMind.UI | 화살표/하이라이트/말풍선/팝업 표시, 입력 차단 오버레이 |
| **TutorialSequenceData** | ScriptableObject | SeedMind.Tutorial.Data | 튜토리얼 시퀀스 정의 (단계 배열, 시작 조건) |
| **TutorialStepData** | ScriptableObject | SeedMind.Tutorial.Data | 개별 단계 정의 (트리거, 완료 조건, UI 설정) |
| **ContextHintData** | ScriptableObject | SeedMind.Tutorial.Data | 상황별 힌트 정의 (조건, 메시지, 쿨다운) |
| **TutorialEvents** | Static class | SeedMind.Tutorial | 튜토리얼 전용 이벤트 허브 |
| **TutorialSaveData** | Plain C# class | SeedMind.Tutorial | 진행 상태 직렬화용 데이터 클래스 |

---

## 3. TutorialManager 상세

### 3.1 상태 머신

```
┌────────────┐    시퀀스 시작    ┌────────────────┐
│   Idle     │ ──────────────▶ │ RunningSequence │
│ (대기 중)   │                 │ (시퀀스 진행 중)  │
└────────────┘                 └───────┬─────────┘
      ▲                               │
      │          시퀀스 완료/스킵        │
      └────────────────────────────────┘

RunningSequence 내부:
┌──────────────┐  완료 이벤트  ┌──────────────┐  마지막 단계  ┌──────────────┐
│ WaitTrigger  │────────────▶│ StepActive   │────────────▶│ StepComplete │
│ (트리거 대기) │             │ (UI 표시 중)  │             │ (전환 대기)   │
└──────────────┘             └──────────────┘             └──────┬───────┘
                                                                │
                                                        다음 단계 있음
                                                                │
                                                        ┌───────▼───────┐
                                                        │ WaitTrigger   │
                                                        │ (다음 단계)    │
                                                        └───────────────┘
```

### 3.2 TutorialManager 클래스 (illustrative)

```csharp
// illustrative
namespace SeedMind.Tutorial
{
    public class TutorialManager : MonoBehaviour // Singleton<TutorialManager> 상속
    {
        [Header("설정")]
        [SerializeField] private TutorialSequenceData[] _allSequences;  // 에디터에서 등록
        [SerializeField] private TutorialUI _tutorialUI;

        // 런타임 상태
        private TutorialState _state = TutorialState.Idle;
        private TutorialSequenceData _activeSequence;
        private int _currentStepIndex;
        private HashSet<string> _completedSequences = new HashSet<string>();
        private HashSet<string> _completedSteps = new HashSet<string>();

        // --- 시퀀스 제어 ---

        public bool TryStartSequence(string sequenceId)
        {
            if (_state != TutorialState.Idle) return false;
            if (_completedSequences.Contains(sequenceId)) return false;

            var seq = FindSequence(sequenceId);
            if (seq == null) return false;

            _activeSequence = seq;
            _currentStepIndex = 0;
            _state = TutorialState.RunningSequence;

            TutorialEvents.OnSequenceStarted?.Invoke(sequenceId);
            AdvanceToStep(0);
            return true;
        }

        public void AdvanceToStep(int stepIndex)
        {
            if (_activeSequence == null) return;
            if (stepIndex >= _activeSequence.steps.Length)
            {
                CompleteSequence();
                return;
            }

            _currentStepIndex = stepIndex;
            var step = _activeSequence.steps[stepIndex];

            _tutorialUI.ShowStep(step);
            TutorialEvents.OnStepStarted?.Invoke(_activeSequence.sequenceId, step.stepId);
        }

        public void OnStepCompleted()
        {
            var step = _activeSequence.steps[_currentStepIndex];
            _completedSteps.Add(step.stepId);
            TutorialEvents.OnStepCompleted?.Invoke(_activeSequence.sequenceId, step.stepId);

            _tutorialUI.HideStep();
            AdvanceToStep(_currentStepIndex + 1);
        }

        public void SkipSequence()
        {
            if (_activeSequence == null) return;
            string seqId = _activeSequence.sequenceId; // null 할당 전에 미리 보관
            _completedSequences.Add(seqId);
            _tutorialUI.HideStep();
            _activeSequence = null;
            _state = TutorialState.Idle;
            TutorialEvents.OnSequenceSkipped?.Invoke(seqId);
        }

        private void CompleteSequence()
        {
            string seqId = _activeSequence.sequenceId;
            _completedSequences.Add(seqId);
            _activeSequence = null;
            _state = TutorialState.Idle;
            TutorialEvents.OnSequenceCompleted?.Invoke(seqId);
        }

        private TutorialSequenceData FindSequence(string id)
        {
            foreach (var seq in _allSequences)
                if (seq.sequenceId == id) return seq;
            return null;
        }

        // --- 세이브/로드 ---

        public TutorialSaveData ExportSaveData()
        {
            return new TutorialSaveData
            {
                completedSequenceIds = new List<string>(_completedSequences),
                completedStepIds = new List<string>(_completedSteps),
                activeSequenceId = _activeSequence?.sequenceId ?? "",
                activeStepIndex = _currentStepIndex,
                contextHintCooldowns = GetComponent<ContextHintSystem>()?.ExportCooldowns()
                    ?? new Dictionary<string, int>()
            };
        }

        public void ImportSaveData(TutorialSaveData data)
        {
            _completedSequences = new HashSet<string>(data.completedSequenceIds);
            _completedSteps = new HashSet<string>(data.completedStepIds);

            if (!string.IsNullOrEmpty(data.activeSequenceId))
            {
                var seq = FindSequence(data.activeSequenceId);
                if (seq != null)
                {
                    _activeSequence = seq;
                    _currentStepIndex = data.activeStepIndex;
                    _state = TutorialState.RunningSequence;
                    AdvanceToStep(_currentStepIndex);
                }
            }

            GetComponent<ContextHintSystem>()?.ImportCooldowns(data.contextHintCooldowns);
        }
    }

    public enum TutorialState
    {
        Idle,              // 대기 중
        RunningSequence    // 시퀀스 진행 중
    }
}
```

---

## 4. TutorialStepData / TutorialSequenceData (ScriptableObject)

### 4.1 TutorialSequenceData

시퀀스 단위의 튜토리얼을 정의하는 SO. 하나의 시퀀스는 여러 단계(Step)를 순서대로 포함한다.

```csharp
// illustrative
namespace SeedMind.Tutorial.Data
{
    [CreateAssetMenu(fileName = "NewTutorialSequence", menuName = "SeedMind/TutorialSequenceData")]
    public class TutorialSequenceData : ScriptableObject
    {
        [Header("기본 정보")]
        public string sequenceId;             // "SEQ_MainTutorial", "SEQ_BuildingIntro"
        public string displayName;            // UI 표시용 이름
        public TutorialType tutorialType;     // MainTutorial, SystemTutorial

        [Header("시퀀스 구성")]
        public TutorialStepData[] steps;      // 단계 배열 (순서대로 진행)

        [Header("시작 조건")]
        public bool autoStartOnNewGame;       // true이면 새 게임 시작 시 자동 시작
        public string prerequisiteSequenceId; // 선행 시퀀스 ID (빈 문자열이면 없음)
        public TutorialTriggerType startTriggerType;  // 시작 트리거 유형
        public string startTriggerParam;      // 트리거 파라미터 (이벤트명, 해금 ID 등)

        [Header("옵션")]
        public bool skippable;                // 스킵 가능 여부
        public bool pauseGameTime;            // 진행 중 게임 시간 정지 여부
    }

    public enum TutorialType
    {
        MainTutorial   = 0,  // 메인 온보딩 (첫 플레이)
        SystemTutorial = 1   // 시스템별 소개 (해금 시 1회)
    }
}
```

### 4.2 TutorialStepData

개별 튜토리얼 단계를 정의하는 SO. 각 단계는 플레이어에게 보여줄 UI와 완료 조건을 포함한다.

```csharp
// illustrative
namespace SeedMind.Tutorial.Data
{
    [CreateAssetMenu(fileName = "NewTutorialStep", menuName = "SeedMind/TutorialStepData")]
    public class TutorialStepData : ScriptableObject
    {
        [Header("식별")]
        public string stepId;                  // "STEP_MainTutorial_01_Move"

        [Header("UI 표시")]
        public TutorialUIType uiType;          // Bubble, Popup, Arrow, Highlight, Combined
        [TextArea(2, 5)]
        public string messageText;             // 표시할 안내 메시지
        public Sprite iconOverride;            // 아이콘 오버라이드 (null이면 기본)
        public TutorialAnchorType anchorType;  // 화면 위치 기준 (WorldTarget, ScreenPosition, UIElement)
        public string anchorTargetId;          // 대상 오브젝트/UI 요소 식별자

        [Header("화살표/하이라이트")]
        public bool showArrow;                 // 화살표 표시 여부
        public Vector2 arrowOffset;            // 화살표 오프셋
        public bool showHighlight;             // 대상 하이라이트 여부
        public float highlightRadius;          // 하이라이트 영역 반경 // → see 튜토리얼 UI 설정

        [Header("완료 조건")]
        public StepCompletionType completionType;  // 완료 판정 방식
        public string completionEventType;     // 감지할 이벤트 유형 (FarmEvents.OnTileTilled 등)
        public string completionParam;         // 이벤트 파라미터 조건 (선택)
        public float autoAdvanceDelay;         // 자동 진행 딜레이 (초, 0이면 이벤트 기반)

        [Header("입력 제어")]
        public bool blockOtherInput;           // 튜토리얼 대상 외 입력 차단
        public string[] allowedInputActions;   // 차단 시 허용할 Input Action 이름 목록
    }

    public enum TutorialUIType
    {
        Bubble     = 0,  // 말풍선 (대상 근처)
        Popup      = 1,  // 화면 중앙 팝업
        Arrow      = 2,  // 화살표만 (텍스트 없음)
        Highlight  = 3,  // 하이라이트만 (텍스트 없음)
        Combined   = 4   // 말풍선 + 화살표 + 하이라이트
    }

    public enum TutorialAnchorType
    {
        WorldTarget    = 0,  // 월드 오브젝트 위치 추적
        ScreenPosition = 1,  // 화면 고정 좌표
        UIElement      = 2   // UI 요소에 부착
    }

    public enum StepCompletionType
    {
        EventBased  = 0,  // 특정 이벤트 발생 시 완료
        TimeBased   = 1,  // 일정 시간 경과 후 자동 완료
        ClickToContinue = 2,  // 클릭/키 입력으로 다음
        Composite   = 3   // 여러 조건 AND 조합
    }

    public enum TutorialTriggerType
    {
        NewGame        = 0,  // 새 게임 시작
        UnlockAchieved = 1,  // 특정 해금 달성
        FirstVisit     = 2,  // 특정 영역 첫 방문
        EventFired     = 3,  // 특정 이벤트 발생
        LevelReached   = 4   // 특정 레벨 도달
    }
}
```

에셋 이름 규칙: `SO_TutSeq_MainTutorial.asset`, `SO_TutStep_Main_01_Move.asset`  
저장 경로: `Assets/_Project/Data/Tutorial/`

---

## 5. TutorialTrigger 시스템

### 5.1 이벤트 기반 트리거 설계

TutorialTriggerSystem은 기존 이벤트 버스(FarmEvents, BuildingEvents, NPCEvents 등)에 구독하여, 튜토리얼 단계의 시작/완료 조건을 판정한다. 기존 시스템 코드를 수정하지 않는다.

```csharp
// illustrative
namespace SeedMind.Tutorial
{
    public class TutorialTriggerSystem : MonoBehaviour
    {
        [SerializeField] private TutorialManager _manager;

        private void OnEnable()
        {
            // 경작 이벤트 구독
            FarmEvents.OnTileTilled += HandleTileTilled;
            FarmEvents.OnCropPlanted += HandleCropPlanted;
            FarmEvents.OnTileWatered += HandleTileWatered;
            FarmEvents.OnCropHarvested += HandleCropHarvested;

            // 시설 이벤트 구독
            BuildingEvents.OnBuildingPlaced += HandleBuildingPlaced;
            BuildingEvents.OnBuildingCompleted += HandleBuildingCompleted;
            BuildingEvents.OnProcessingComplete += HandleProcessingComplete;

            // NPC 이벤트 구독
            NPCEvents.OnShopOpened += HandleShopOpened;
            NPCEvents.OnDialogueStarted += HandleDialogueStarted;

            // 도구 이벤트 구독
            ToolUpgradeEvents.OnUpgradeStarted += HandleUpgradeStarted;
            ToolUpgradeEvents.OnUpgradeCompleted += HandleUpgradeCompleted;

            // 진행 이벤트 구독
            // ProgressionEvents는 해금/레벨업 감지용
        }

        private void OnDisable()
        {
            // 모든 구독 해제 (메모리 누수 방지)
            FarmEvents.OnTileTilled -= HandleTileTilled;
            FarmEvents.OnCropPlanted -= HandleCropPlanted;
            FarmEvents.OnTileWatered -= HandleTileWatered;
            FarmEvents.OnCropHarvested -= HandleCropHarvested;

            BuildingEvents.OnBuildingPlaced -= HandleBuildingPlaced;
            BuildingEvents.OnBuildingCompleted -= HandleBuildingCompleted;
            BuildingEvents.OnProcessingComplete -= HandleProcessingComplete;

            NPCEvents.OnShopOpened -= HandleShopOpened;
            NPCEvents.OnDialogueStarted -= HandleDialogueStarted;

            ToolUpgradeEvents.OnUpgradeStarted -= HandleUpgradeStarted;
            ToolUpgradeEvents.OnUpgradeCompleted -= HandleUpgradeCompleted;
        }

        // --- 이벤트 핸들러 ---

        private void HandleTileTilled(Vector2Int pos)
        {
            TryCompleteActiveStep("FarmEvents.OnTileTilled");
            TryTriggerSequence(TutorialTriggerType.EventFired, "FarmEvents.OnTileTilled");
        }

        private void HandleCropPlanted(Vector2Int pos, CropData crop)
        {
            TryCompleteActiveStep("FarmEvents.OnCropPlanted");
        }

        private void HandleTileWatered(Vector2Int pos)
        {
            TryCompleteActiveStep("FarmEvents.OnTileWatered");
        }

        private void HandleCropHarvested(Vector2Int pos, CropData crop, int qty)
        {
            TryCompleteActiveStep("FarmEvents.OnCropHarvested");
        }

        private void HandleBuildingPlaced(BuildingInstance inst)
        {
            TryCompleteActiveStep("BuildingEvents.OnBuildingPlaced");
            TryTriggerSequence(TutorialTriggerType.EventFired, "BuildingEvents.OnBuildingPlaced");
        }

        private void HandleBuildingCompleted(BuildingInstance inst)
        {
            TryCompleteActiveStep("BuildingEvents.OnBuildingCompleted");
        }

        private void HandleProcessingComplete(BuildingInstance inst, int slotIndex)
        {
            TryCompleteActiveStep("BuildingEvents.OnProcessingComplete");
        }

        private void HandleShopOpened(string npcId)
        {
            TryCompleteActiveStep("NPCEvents.OnShopOpened");
        }

        private void HandleDialogueStarted(string npcId, DialogueData data)
        {
            TryCompleteActiveStep("NPCEvents.OnDialogueStarted");
        }

        private void HandleUpgradeStarted(ToolUpgradeInfo info)
        {
            TryCompleteActiveStep("ToolUpgradeEvents.OnUpgradeStarted");
            TryTriggerSequence(TutorialTriggerType.EventFired, "ToolUpgradeEvents.OnUpgradeStarted");
        }

        private void HandleUpgradeCompleted(ToolUpgradeInfo info)
        {
            TryCompleteActiveStep("ToolUpgradeEvents.OnUpgradeCompleted");
        }

        // --- 판정 로직 ---

        private void TryCompleteActiveStep(string eventType)
        {
            var activeStep = _manager.GetActiveStep();
            if (activeStep == null) return;
            if (activeStep.completionType != StepCompletionType.EventBased) return;
            if (activeStep.completionEventType != eventType) return;

            _manager.OnStepCompleted();
        }

        private void TryTriggerSequence(TutorialTriggerType triggerType, string param)
        {
            // TutorialManager에 등록된 미완료 시퀀스 중
            // startTriggerType과 startTriggerParam이 일치하는 것을 찾아 시작
            _manager.TryStartSequenceByTrigger(triggerType, param);
        }
    }
}
```

### 5.2 이벤트-단계 매핑 테이블

| 튜토리얼 단계 | 감지 이벤트 | 완료 조건 |
|---------------|-------------|-----------|
| Step 01: 이동 | (Input System 직접 감지) | 플레이어 위치 변경 감지 |
| Step 03: 땅 경작 | `FarmEvents.OnTileTilled` | 1회 발생 |
| Step 04: 씨앗 심기 | `FarmEvents.OnCropPlanted` | 1회 발생 |
| Step 05: 물주기 | `FarmEvents.OnTileWatered` | 1회 발생 |
| Step 07: 수확 | `FarmEvents.OnCropHarvested` | 1회 발생 |
| Step 09: 상점 판매 | `NPCEvents.OnShopOpened` | 상점 UI 열림 |
| SEQ_BuildingIntro | `BuildingEvents.OnBuildingPlaced` | 시설 1개 배치 |
| SEQ_ToolUpgradeIntro | `ToolUpgradeEvents.OnUpgradeStarted` | 업그레이드 1회 시작 |

---

## 6. TutorialUI 컴포넌트

### 6.1 UI 계층 구조

```
Canvas_Tutorial (Overlay, Sort Order: 40)  // → see docs/systems/ui-architecture.md 섹션 2.2
├── Panel_Dimmer                 # 반투명 배경 (입력 차단 시)
│   └── Mask_Highlight           # 하이라이트 영역 마스크 (원형/사각)
├── Panel_Bubble                 # 말풍선 컨테이너
│   ├── Image_BubbleBackground
│   ├── Text_Message (TMP)
│   ├── Image_Icon
│   └── Button_Continue          # "다음" 또는 "확인" 버튼
├── Panel_Arrow                  # 화살표 인디케이터
│   └── Image_Arrow (회전 가능)
├── Panel_Popup                  # 중앙 팝업 패널
│   ├── Text_Title (TMP)
│   ├── Text_Body (TMP)
│   ├── Image_Illustration
│   └── Button_Close
└── Panel_Progress               # 진행 표시기
    ├── Text_StepCounter (TMP)   # "3/12"
    └── Slider_Progress
```

### 6.2 TutorialUI 클래스 (illustrative)

```csharp
// illustrative
namespace SeedMind.UI
{
    public class TutorialUI : MonoBehaviour
    {
        [Header("UI 참조")]
        [SerializeField] private GameObject _dimmerPanel;
        [SerializeField] private RectTransform _highlightMask;
        [SerializeField] private GameObject _bubblePanel;
        [SerializeField] private TMP_Text _messageText;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _continueButton;
        [SerializeField] private RectTransform _arrowPanel;
        [SerializeField] private Image _arrowImage;
        [SerializeField] private GameObject _popupPanel;
        [SerializeField] private TMP_Text _popupTitle;
        [SerializeField] private TMP_Text _popupBody;
        [SerializeField] private TMP_Text _stepCounterText;
        [SerializeField] private Slider _progressSlider;

        [Header("설정")]
        [SerializeField] private float _fadeInDuration;     // → see 튜토리얼 UI 연출 설정
        [SerializeField] private float _arrowBobSpeed;      // → see 튜토리얼 UI 연출 설정
        [SerializeField] private float _arrowBobAmplitude;  // → see 튜토리얼 UI 연출 설정

        private Camera _mainCamera;
        private TutorialStepData _activeStep;

        public void ShowStep(TutorialStepData step)
        {
            _activeStep = step;
            HideAll();

            switch (step.uiType)
            {
                case TutorialUIType.Bubble:
                    ShowBubble(step);
                    break;
                case TutorialUIType.Popup:
                    ShowPopup(step);
                    break;
                case TutorialUIType.Arrow:
                    ShowArrow(step);
                    break;
                case TutorialUIType.Highlight:
                    ShowHighlight(step);
                    break;
                case TutorialUIType.Combined:
                    ShowBubble(step);
                    ShowArrow(step);
                    ShowHighlight(step);
                    break;
            }

            if (step.blockOtherInput)
                ShowDimmer();

            UpdateProgress();
        }

        public void HideStep()
        {
            HideAll();
            _activeStep = null;
        }

        private void ShowBubble(TutorialStepData step)
        {
            _bubblePanel.SetActive(true);
            _messageText.text = step.messageText;
            _iconImage.sprite = step.iconOverride;
            _iconImage.gameObject.SetActive(step.iconOverride != null);

            // 앵커 위치 계산
            PositionToAnchor(_bubblePanel.GetComponent<RectTransform>(), step);

            // 클릭 진행 버튼 표시 여부
            _continueButton.gameObject.SetActive(
                step.completionType == StepCompletionType.ClickToContinue);
        }

        private void ShowArrow(TutorialStepData step)
        {
            if (!step.showArrow) return;
            _arrowPanel.gameObject.SetActive(true);
            PositionToAnchor(_arrowPanel, step);
            _arrowPanel.anchoredPosition += step.arrowOffset;
            // 화살표 상하 흔들림 애니메이션은 Update에서 처리
        }

        private void ShowHighlight(TutorialStepData step)
        {
            if (!step.showHighlight) return;
            _highlightMask.gameObject.SetActive(true);
            // 하이라이트 대상 위치/크기에 맞춰 마스크 설정
            PositionToAnchor(_highlightMask, step);
            _highlightMask.sizeDelta = Vector2.one * step.highlightRadius * 2f;
        }

        private void ShowDimmer()
        {
            _dimmerPanel.SetActive(true);
        }

        private void ShowPopup(TutorialStepData step)
        {
            _popupPanel.SetActive(true);
            _popupBody.text = step.messageText;
        }

        private void HideAll()
        {
            _dimmerPanel.SetActive(false);
            _highlightMask.gameObject.SetActive(false);
            _bubblePanel.SetActive(false);
            _arrowPanel.gameObject.SetActive(false);
            _popupPanel.SetActive(false);
        }

        private void PositionToAnchor(RectTransform target, TutorialStepData step)
        {
            switch (step.anchorType)
            {
                case TutorialAnchorType.WorldTarget:
                    // anchorTargetId로 GameObject.Find → WorldToScreenPoint
                    var go = GameObject.Find(step.anchorTargetId);
                    if (go != null && _mainCamera != null)
                    {
                        Vector2 screenPos = _mainCamera.WorldToScreenPoint(go.transform.position);
                        target.position = screenPos;
                    }
                    break;

                case TutorialAnchorType.ScreenPosition:
                    // anchorTargetId를 "x,y" 파싱 (정규화 좌표)
                    break;

                case TutorialAnchorType.UIElement:
                    // anchorTargetId로 UI 요소 탐색 → 위치 복사
                    break;
            }
        }

        private void UpdateProgress()
        {
            // TutorialManager에서 현재 시퀀스/총 단계 수 조회
            var manager = TutorialManager.Instance;  // 싱글턴 참조
            if (manager == null) return;
            // stepCounterText, progressSlider 업데이트
        }

        private void Update()
        {
            // 화살표 상하 흔들림 애니메이션
            if (_arrowPanel.gameObject.activeSelf)
            {
                float bob = Mathf.Sin(Time.time * _arrowBobSpeed) * _arrowBobAmplitude;
                // 흔들림 적용
            }

            // 월드 타겟 추적 (매 프레임 위치 갱신)
            if (_activeStep != null && _activeStep.anchorType == TutorialAnchorType.WorldTarget)
            {
                // 위치 재계산
            }
        }
    }
}
```

---

## 7. TutorialSaveData (JSON + C# 동기화, PATTERN-005)

### 7.1 JSON 스키마

```json
{
  "completedSequenceIds": ["SEQ_MainTutorial", "SEQ_BuildingIntro"],
  "completedStepIds": ["STEP_MainTutorial_01_Move", "STEP_MainTutorial_02_ToolSelect"],
  "activeSequenceId": "",
  "activeStepIndex": 0,
  "contextHintCooldowns": {
    "HINT_WaterReminder": 5,
    "HINT_SeasonCropWarning": 12
  }
}
```

### 7.2 C# 클래스

```csharp
// illustrative
namespace SeedMind.Tutorial
{
    [System.Serializable]
    public class TutorialSaveData
    {
        public List<string> completedSequenceIds;             // 완료된 시퀀스 ID 목록
        public List<string> completedStepIds;                 // 완료된 단계 ID 목록
        public string activeSequenceId;                       // 현재 진행 중인 시퀀스 (빈 문자열이면 없음)
        public int activeStepIndex;                           // 현재 진행 중인 단계 인덱스
        public Dictionary<string, int> contextHintCooldowns;  // 힌트 ID → 남은 쿨다운 일수
    }
}
```

**PATTERN-005 검증**: JSON 스키마(7.1)와 C# 클래스(7.2)의 필드가 정확히 일치한다:
- `completedSequenceIds` -- List\<string\> / string[]
- `completedStepIds` -- List\<string\> / string[]
- `activeSequenceId` -- string
- `activeStepIndex` -- int
- `contextHintCooldowns` -- Dictionary\<string, int\> / object

### 7.3 마스터 세이브 스키마 확장

기존 최상위 세이브 스키마(`docs/pipeline/data-pipeline.md` 섹션 3.2)에 `tutorial` 필드를 추가한다:

```json
{
  "version": "1.0",
  "saveDate": "...",
  "player": { },
  "inventory": { },
  "farm": { },
  "time": { },
  "weather": { },
  "economy": { },
  "buildings": [ ],
  "processing": [ ],
  "unlocks": { },
  "shops": [ ],
  "tutorial": { }
}
```

---

## 8. ContextHintSystem (상황별 자동 팁)

### 8.1 설계 개요

메인 튜토리얼과 별도로, 게임 진행 중 특정 상황에서 자동으로 노출되는 힌트 시스템이다. 쿨다운을 두어 같은 힌트가 반복 노출되지 않도록 한다.

### 8.2 ContextHintData (ScriptableObject)

```csharp
// illustrative
namespace SeedMind.Tutorial.Data
{
    [CreateAssetMenu(fileName = "NewContextHint", menuName = "SeedMind/ContextHintData")]
    public class ContextHintData : ScriptableObject
    {
        [Header("식별")]
        public string hintId;                  // "HINT_WaterReminder"

        [Header("발동 조건")]
        public HintConditionType conditionType;  // 조건 유형
        public string conditionParam;            // 조건 파라미터

        [Header("표시")]
        [TextArea(2, 4)]
        public string messageText;             // 힌트 메시지
        public Sprite icon;                    // 아이콘 (선택)
        public float displayDuration;          // 표시 시간(초) // → see 힌트 UI 설정

        [Header("빈도 제어")]
        public int cooldownDays;               // 재표시까지 최소 경과 일수 // → see 밸런스 설정
        public int maxShowCount;               // 최대 표시 횟수 (0 = 무제한)
        public bool requireTutorialComplete;   // 메인 튜토리얼 완료 후에만 표시

        [Header("우선순위")]
        public int priority;                   // 높을수록 우선 표시 (여러 힌트 동시 발동 시)
    }

    public enum HintConditionType
    {
        DryTilesExist     = 0,  // 물 안 준 타일이 N개 이상
        LowGold           = 1,  // 소지 골드가 임계값 이하
        InventoryFull      = 2,  // 인벤토리 가득 참
        SeasonMismatchCrop = 3,  // 현재 계절에 맞지 않는 작물 보유
        UnusedFertilizer   = 4,  // 비료 보유 중 미사용
        ReadyToHarvest     = 5,  // 수확 가능 작물 방치
        NightTime          = 6,  // 야간인데 아직 활동 중
        ProcessingReady    = 7   // 가공 완료 수령 대기
    }
}
```

### 8.3 ContextHintSystem 클래스 (illustrative)

```csharp
// illustrative
namespace SeedMind.Tutorial
{
    public class ContextHintSystem : MonoBehaviour
    {
        [SerializeField] private ContextHintData[] _allHints;
        [SerializeField] private TutorialUI _tutorialUI;

        private Dictionary<string, int> _cooldowns = new Dictionary<string, int>();  // hintId → 남은 쿨다운 일수
        private Dictionary<string, int> _showCounts = new Dictionary<string, int>(); // hintId → 표시 횟수
        private float _checkInterval = 10f;  // 상태 체크 주기 (초) // → see 성능 튜닝 설정
        private float _timer;

        private void OnEnable()
        {
            // 매일 아침 쿨다운 감소
            FarmEvents.OnDailyGrowthComplete += DecrementCooldowns;
        }

        private void OnDisable()
        {
            FarmEvents.OnDailyGrowthComplete -= DecrementCooldowns;
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < _checkInterval) return;
            _timer = 0f;

            // 튜토리얼 진행 중이면 힌트 표시 안 함
            if (TutorialManager.Instance.State != TutorialState.Idle) return;

            EvaluateHints();
        }

        private void EvaluateHints()
        {
            ContextHintData bestHint = null;
            int bestPriority = int.MinValue;

            foreach (var hint in _allHints)
            {
                if (!IsHintAvailable(hint)) continue;
                if (!EvaluateCondition(hint)) continue;
                if (hint.priority > bestPriority)
                {
                    bestHint = hint;
                    bestPriority = hint.priority;
                }
            }

            if (bestHint != null)
                ShowHint(bestHint);
        }

        private bool IsHintAvailable(ContextHintData hint)
        {
            // 쿨다운 체크
            if (_cooldowns.TryGetValue(hint.hintId, out int remaining) && remaining > 0)
                return false;

            // 최대 표시 횟수 체크
            if (hint.maxShowCount > 0)
            {
                _showCounts.TryGetValue(hint.hintId, out int count);
                if (count >= hint.maxShowCount) return false;
            }

            // 메인 튜토리얼 완료 필수 체크
            if (hint.requireTutorialComplete &&
                !TutorialManager.Instance.IsSequenceCompleted("SEQ_MainTutorial"))
                return false;

            return true;
        }

        private bool EvaluateCondition(ContextHintData hint)
        {
            switch (hint.conditionType)
            {
                case HintConditionType.DryTilesExist:
                    // FarmGrid에서 Dry 상태 타일 수 조회
                    return false; // placeholder
                case HintConditionType.LowGold:
                    // EconomyManager.CurrentGold < threshold
                    return false;
                case HintConditionType.InventoryFull:
                    // InventoryManager.BackpackEmptySlotCount == 0
                    return false;
                // ... 기타 조건
                default:
                    return false;
            }
        }

        private void ShowHint(ContextHintData hint)
        {
            _cooldowns[hint.hintId] = hint.cooldownDays;
            _showCounts.TryGetValue(hint.hintId, out int count);
            _showCounts[hint.hintId] = count + 1;

            TutorialEvents.OnContextHintShown?.Invoke(hint.hintId);
            // TutorialUI에 힌트 표시 위임 (토스트 스타일)
        }

        private void DecrementCooldowns()
        {
            var keys = new List<string>(_cooldowns.Keys);
            foreach (var key in keys)
            {
                _cooldowns[key] = Mathf.Max(0, _cooldowns[key] - 1);
            }
        }

        // --- 세이브/로드 ---

        public Dictionary<string, int> ExportCooldowns()
        {
            return new Dictionary<string, int>(_cooldowns);
        }

        public void ImportCooldowns(Dictionary<string, int> data)
        {
            _cooldowns = new Dictionary<string, int>(data);
        }
    }
}
```

---

## 9. TutorialEvents (이벤트 허브)

기존 프로젝트의 정적 이벤트 허브 패턴(FarmEvents, NPCEvents, BuildingEvents, ToolUpgradeEvents)을 계승한다.

```csharp
// illustrative
namespace SeedMind.Tutorial
{
    /// <summary>
    /// 튜토리얼 시스템 전용 이벤트 허브.
    /// 기존 XXXEvents 패턴과 동일한 정적 이벤트 구조.
    /// </summary>
    public static class TutorialEvents
    {
        // --- 시퀀스 ---
        public static event Action<string> OnSequenceStarted;      // sequenceId
        public static event Action<string> OnSequenceCompleted;    // sequenceId
        public static event Action<string> OnSequenceSkipped;      // sequenceId

        // --- 단계 ---
        public static event Action<string, string> OnStepStarted;  // sequenceId, stepId
        public static event Action<string, string> OnStepCompleted; // sequenceId, stepId

        // --- 상황별 힌트 ---
        public static event Action<string> OnContextHintShown;     // hintId
        public static event Action<string> OnContextHintDismissed; // hintId

        // --- 전체 ---
        public static event Action OnAllTutorialsCompleted;        // 모든 시퀀스 완료 시
    }
}
```

### 9.1 이벤트 소비자 예시

| 이벤트 | 소비자 | 용도 |
|--------|--------|------|
| `OnSequenceStarted` | SaveManager | 자동 저장 트리거 |
| `OnSequenceCompleted` | ProgressionManager | 튜토리얼 완료 XP 보상 |
| `OnSequenceCompleted` | UI (HUD) | 축하 메시지 표시 |
| `OnStepStarted` | TutorialUI | UI 표시 업데이트 |
| `OnStepCompleted` | TutorialUI | UI 숨기기, 진행률 갱신 |
| `OnContextHintShown` | Analytics (향후) | 힌트 노출 추적 |
| `OnAllTutorialsCompleted` | GameManager | 튜토리얼 모드 해제 |

### 9.2 설계 원칙

- **Fire and forget**: 이벤트 발행자는 소비자를 모른다 (기존 패턴 동일)
- **구독 해제 필수**: MonoBehaviour.OnDisable에서 구독 해제
- [RISK] static event 구독 누수 -- 씬 전환 시 이벤트 초기화 루틴 필요 (farming-architecture.md 6.3절과 동일 패턴)

---

## 10. 네임스페이스 및 폴더 구조

### 10.1 네임스페이스

```
SeedMind.Tutorial              # TutorialManager, TutorialTriggerSystem,
                               # ContextHintSystem, TutorialState,
                               # TutorialEvents, TutorialSaveData
SeedMind.Tutorial.Data         # TutorialSequenceData, TutorialStepData,
                               # ContextHintData, 각종 enum 정의
SeedMind.UI                    # TutorialUI (기존 UI 네임스페이스에 합류)
```

### 10.2 폴더 구조

```
Assets/_Project/
├── Scripts/
│   ├── Tutorial/                        # SeedMind.Tutorial
│   │   ├── TutorialManager.cs
│   │   ├── TutorialTriggerSystem.cs
│   │   ├── ContextHintSystem.cs
│   │   ├── TutorialEvents.cs
│   │   ├── TutorialSaveData.cs
│   │   └── Data/                        # SeedMind.Tutorial.Data
│   │       ├── TutorialSequenceData.cs
│   │       ├── TutorialStepData.cs
│   │       ├── ContextHintData.cs
│   │       ├── TutorialType.cs          # enum
│   │       ├── TutorialUIType.cs        # enum
│   │       ├── TutorialAnchorType.cs    # enum
│   │       ├── StepCompletionType.cs    # enum
│   │       ├── TutorialTriggerType.cs   # enum
│   │       └── HintConditionType.cs     # enum
│   │
│   └── UI/
│       └── TutorialUI.cs               # SeedMind.UI (기존 UI 폴더에 추가)
│
├── Data/
│   └── Tutorial/                        # SO 에셋 인스턴스
│       ├── Sequences/                   # SO_TutSeq_MainTutorial.asset 등
│       ├── Steps/                       # SO_TutStep_Main_01_Move.asset 등
│       └── Hints/                       # SO_CtxHint_WaterReminder.asset 등
│
└── Prefabs/
    └── UI/
        └── PFB_UI_Tutorial.prefab       # 튜토리얼 UI 프리팹
```

### 10.3 의존성 방향

```
                ┌─────────┐
                │  Core   │  (의존성 없음)
                └────┬────┘
                     │
        ┌────────────┼────────────┐
        ▼            ▼            ▼
   ┌────────┐  ┌──────────┐  ┌────────┐
   │  Farm  │  │  Player  │  │ Level  │
   └────────┘  └──────────┘  └────────┘
        │            │            │
        └──────┬─────┴─────┬──────┘
               ▼           ▼
          ┌────────┐  ┌────────┐
          │Economy │  │Building│
          └────────┘  └────────┘
               │           │
               └─────┬─────┘
                     ▼
              ┌────────────┐
              │  Tutorial  │  ← 기존 시스템 이벤트를 구독만 함 (단방향)
              └──────┬─────┘
                     ▼
              ┌────────────┐
              │     UI     │
              └────────────┘
```

Tutorial 모듈은 기존 시스템(Farm, Player, Economy, Building, Level)의 이벤트를 **구독만** 하고, 역방향 의존은 없다. 기존 시스템은 Tutorial의 존재를 모른다.

---

# Part II -- MCP 구현 태스크 요약

---

## MCP-1: TutorialManager GameObject 배치

```
Step 1: SCN_Farm 씬에 빈 GameObject "TutorialSystem" 생성
        → Position: (0, 0, 0)
        → Parent: 씬 루트

Step 2: "TutorialSystem"에 TutorialManager.cs 컴포넌트 부착
        → Singleton 패턴 적용 확인

Step 3: "TutorialSystem" 하위에 빈 GameObject "TutorialTriggerSystem" 생성
        → TutorialTriggerSystem.cs 컴포넌트 부착
        → _manager 필드에 부모의 TutorialManager 참조 연결

Step 4: "TutorialSystem" 하위에 빈 GameObject "ContextHintSystem" 생성
        → ContextHintSystem.cs 컴포넌트 부착
```

## MCP-2: TutorialUI 프리팹 생성 및 배치

```
Step 1: Canvas "Canvas_Tutorial" 생성
        → Render Mode: Screen Space - Overlay
        → Sort Order: 40 (→ see docs/systems/ui-architecture.md 섹션 2.2 Canvas 계층)
        → Canvas Scaler: Scale With Screen Size, Reference 1920x1080

Step 2: Canvas_Tutorial 하위에 Panel_Dimmer 생성
        → Image 컴포넌트, Color: (0, 0, 0, 0.5)
        → RectTransform: Stretch All, 전체 화면
        → 기본 비활성

Step 3: Canvas_Tutorial 하위에 Panel_Bubble 생성
        → Image (배경), TMP_Text (메시지), Button (계속)
        → Pivot: 하단 중앙 (말풍선 꼬리 위치)
        → 기본 비활성

Step 4: Canvas_Tutorial 하위에 Panel_Arrow 생성
        → Image (화살표 스프라이트)
        → 기본 비활성

Step 5: Canvas_Tutorial 하위에 Panel_Popup 생성
        → Image (배경), TMP_Text (제목), TMP_Text (본문), Button (닫기)
        → RectTransform: 화면 중앙, 600x400
        → 기본 비활성

Step 6: Canvas_Tutorial 하위에 Panel_Progress 생성
        → TMP_Text (단계 카운터), Slider (진행 바)
        → RectTransform: 상단 중앙
        → 기본 비활성

Step 7: Canvas_Tutorial에 TutorialUI.cs 컴포넌트 부착
        → 각 Panel 참조 연결
        → TutorialManager._tutorialUI 필드에 연결

Step 8: Canvas_Tutorial을 Prefabs/UI/PFB_UI_Tutorial.prefab으로 저장
```

## MCP-3: TutorialStepData SO 에셋 생성

```
Step 1: Assets/_Project/Data/Tutorial/Sequences/ 폴더 생성
        Assets/_Project/Data/Tutorial/Steps/ 폴더 생성
        Assets/_Project/Data/Tutorial/Hints/ 폴더 생성

Step 2: TutorialSequenceData SO 생성
        → SO_TutSeq_MainTutorial.asset
        → sequenceId = "SEQ_MainTutorial"
        → tutorialType = MainTutorial
        → autoStartOnNewGame = true
        → skippable = true
        → pauseGameTime = false

Step 3: TutorialStepData SO 에셋 12개 생성 (메인 튜토리얼 단계별, → see `docs/systems/tutorial-system.md` 섹션 2.1)
        → SO_TutStep_Main_01_Arrival.asset ~ SO_TutStep_Main_12_Complete.asset
        → 각 SO에 stepId, uiType, completionType, completionEventType 설정
        → 메시지 텍스트는 placeholder로 설정 (콘텐츠 문서에서 확정 후 업데이트)

Step 4: SO_TutSeq_MainTutorial의 steps[] 배열에 Step 3의 SO 12개를 순서대로 연결

Step 5: 시스템 튜토리얼 시퀀스 SO 생성 (→ see 섹션 1.3 목록)
        → SO_TutSeq_BuildingIntro.asset
        → SO_TutSeq_ToolUpgradeIntro.asset
        → SO_TutSeq_SeasonChange.asset
        → SO_TutSeq_ProcessingIntro.asset
```

## MCP-4: ContextHintData SO 에셋 생성

```
Step 1: ContextHintData SO 에셋 생성 (→ see 섹션 8.2 HintConditionType 목록)
        → SO_CtxHint_WaterReminder.asset (conditionType = DryTilesExist)
        → SO_CtxHint_LowGold.asset (conditionType = LowGold)
        → SO_CtxHint_InventoryFull.asset (conditionType = InventoryFull)
        → SO_CtxHint_SeasonCrop.asset (conditionType = SeasonMismatchCrop)
        → SO_CtxHint_HarvestReady.asset (conditionType = ReadyToHarvest)
        → SO_CtxHint_NightWarning.asset (conditionType = NightTime)
        → SO_CtxHint_ProcessingReady.asset (conditionType = ProcessingReady)

Step 2: ContextHintSystem의 _allHints 배열에 Step 1의 SO들을 연결

Step 3: TutorialManager의 _allSequences 배열에 MCP-3의 시퀀스 SO들을 연결
```

## MCP-5: 이벤트 버스 연결 테스트

```
Step 1: Play Mode 진입

Step 2: 콘솔 로그로 TutorialManager 초기화 확인
        → "TutorialManager initialized, X sequences loaded" 메시지 확인

Step 3: 호미로 타일 경작 시 TutorialTriggerSystem 이벤트 수신 확인
        → FarmEvents.OnTileTilled 구독 동작 로그 확인

Step 4: 튜토리얼 시퀀스 시작/단계 진행/완료 사이클 확인
        → TutorialEvents.OnSequenceStarted, OnStepStarted, OnStepCompleted 발행 확인

Step 5: 세이브/로드 후 튜토리얼 진행 상태 복원 확인
        → 완료된 시퀀스가 재시작되지 않는 것 확인
        → 진행 중인 시퀀스가 올바른 단계에서 재개되는 것 확인

Step 6: Play Mode 종료
```

---

## Cross-references

- `docs/architecture.md` -- 마스터 기술 아키텍처, 프로젝트 구조 (3절)
- `docs/systems/project-structure.md` -- 폴더 구조, 네임스페이스, 의존성 규칙 (Tutorial 모듈 반영 완료)
- `docs/systems/farming-architecture.md` -- FarmEvents 이벤트 허브 (6.1절), 이벤트 소비자 패턴
- `docs/systems/npc-shop-architecture.md` -- NPCEvents 이벤트 허브, NPC 인터랙션 흐름 (ARC-008)
- `docs/systems/facilities-architecture.md` -- BuildingEvents 이벤트 허브
- `docs/systems/tool-upgrade-architecture.md` -- ToolUpgradeEvents 이벤트 허브 (DES-007)
- `docs/systems/inventory-architecture.md` -- InventoryManager, 인벤토리 이벤트
- `docs/pipeline/data-pipeline.md` -- 세이브 데이터 구조 (1.2절 TutorialSaveData), SO 에셋 분류 (1.1절, tutorial SO 반영 완료)
- `docs/systems/progression-architecture.md` -- ProgressionEvents, 해금/레벨업 이벤트 (튜토리얼 트리거용)

---

## Open Questions

- [OPEN] 메인 튜토리얼의 시간 경과 대기 단계(Step 06)에서 시간을 빠르게 진행시킬지, 아니면 자연 경과를 기다리게 할지 -- 게임플레이 흐름에 큰 영향
- [OPEN] 튜토리얼 스킵 시 보상(초기 자원 지급) 여부 -- 숙련 플레이어 경험과 밸런스 간의 트레이드오프
- [OPEN] ContextHintSystem의 상태 체크 주기(`_checkInterval`)를 고정값으로 할지, SO에서 설정 가능하게 할지
- [OPEN] 튜토리얼 UI의 월드 타겟 추적 시, 카메라 뷰 밖 대상에 대한 화살표 표시 전략 (화면 가장자리 클램핑 등)
- [OPEN] 다국어 지원 시 튜토리얼 메시지의 Localization 전략 -- TMP의 LocalizationTable 연동 또는 별도 문자열 테이블

---

## Risks

- [RISK] **static event 구독 누수**: TutorialTriggerSystem이 다수의 이벤트를 구독하므로, OnDisable에서의 해제 누락 시 메모리 누수 및 null reference 발생 가능. 씬 전환 시 전체 이벤트 초기화 루틴과 병행 필요.
- [RISK] **MCP를 통한 SO 배열 참조 설정**: TutorialSequenceData.steps[] 배열에 TutorialStepData SO 참조를 순서대로 연결하는 작업이 MCP로 정확히 가능한지 사전 검증 필요 (farming-architecture.md의 동일 리스크와 연동).
- [RISK] **입력 차단 오버레이와 기존 UI 충돌**: blockOtherInput 활성화 시 다른 UI(인벤토리, 상점)의 Raycast가 차단되는 것은 의도적이나, 비정상 상태(튜토리얼 중 상점 UI가 열려있는 경우 등)에서의 복구 로직 필요.
- [RISK] **튜토리얼 진행 중 세이브/로드 타이밍**: 단계 전환 도중(UI 애니메이션 재생 중)에 세이브가 발생하면 activeStepIndex가 불일치할 수 있음. 단계 전환이 완료된 안정 상태에서만 세이브하도록 가드 필요.
- [RISK] **ContextHintSystem의 게임 상태 직접 조회**: EvaluateCondition에서 FarmGrid, EconomyManager, InventoryManager 등을 직접 참조해야 하므로, 의존성이 넓어진다. 인터페이스 또는 이벤트 기반 간접 조회로 결합도를 낮추는 방안 검토 필요.

---

*이 문서는 Claude Code가 기존 아키텍처 패턴(이벤트 버스, SO 기반 데이터, 싱글턴 매니저)을 계승하여 자율적으로 작성했습니다.*
