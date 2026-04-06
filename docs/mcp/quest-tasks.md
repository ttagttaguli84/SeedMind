# 퀘스트/미션 시스템 MCP 태스크 시퀀스

> 작성: Claude Code (Opus 4.6) | 2026-04-07  
> 문서 ID: ARC-016  
> 기반 문서: docs/systems/quest-architecture.md (ARC-013)

---

## Context

이 문서는 `docs/systems/quest-architecture.md`(ARC-013) Part II의 Step 1~5 개요를 상세한 MCP 태스크 시퀀스로 확장한다. 퀘스트 데이터 구조(enum 6종, Serializable 클래스 4종, SO 1종), 시스템 클래스(QuestManager, QuestTracker, QuestRewarder 등), 런타임 상태 클래스, 이벤트 허브, SO 에셋(메인 퀘스트 4종 + 일일 목표 12종 + 농장 도전 4종), UI 오브젝트(QuestLogPanel, QuestTrackingWidget, QuestCompletePopup), 씬 배치, 기존 시스템 연동, 통합 테스트까지 MCP for Unity 도구 호출 수준의 구체적 명세를 포함한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 퀘스트 시스템의 데이터 레이어(QuestData SO 20종), 시스템 레이어(스크립트 20종), UI 레이어(QuestLogPanel, QuestTrackingWidget, QuestCompletePopup), 씬 배치를 완성한다.

**전제 조건**:
- ARC-002(scene-setup-tasks.md) Phase A~B 완료: 폴더 구조, SCN_Farm 기본 계층(MANAGERS, UI, Canvas_HUD, Canvas_Overlay, Canvas_Popup)
- ARC-003(farming-tasks.md) 완료: FarmEvents, 기본 시스템 인프라
- economy-architecture.md 기반 EconomyManager, EconomyEvents 구현 완료
- ARC-008(npc-shop-tasks.md) 완료: NPCManager, NPCEvents, DialogueSystem 구현 완료
- ARC-011(save-load-architecture.md) 기반 SaveManager, ISaveable 인프라 구현 완료
- progression-architecture.md 기반 ProgressionManager, ProgressionEvents 구현 완료
- tutorial-architecture.md 기반 TutorialManager, TutorialEvents 구현 완료

---

## 1. 개요

### 1.1 태스크 맵

| 태스크 | 설명 | MCP 호출 수 |
|--------|------|------------|
| T-1 | 스크립트 생성 (enum, Serializable, SO, 시스템, UI 클래스) | 24회 |
| T-2 | SO 에셋 생성 (QuestData 20종) | ~72회 |
| T-3 | UI 프리팹/씬 오브젝트 생성 (QuestLogPanel, QuestTrackingWidget, QuestCompletePopup) | ~38회 |
| T-4 | 씬 배치 및 참조 연결 | ~18회 |
| T-5 | 기존 시스템 연동 설정 | 5회 |
| T-6 | 통합 테스트 시퀀스 | ~24회 |
| **합계** | | **~181회** |

[RISK] 총 ~181회 MCP 호출은 상당하다. 특히 T-2의 SO 에셋 생성에서 QuestObjectiveData[], QuestRewardData[], QuestUnlockCondition[] 등 중첩 배열 설정이 MCP `set_property`로 가능한지 사전 검증 필요. 불가능한 경우 Editor 스크립트(`CreateQuestAssets.cs`)를 통한 일괄 생성으로 T-2의 ~72회를 ~5회로 감소시킬 수 있다.

### 1.2 스크립트 목록

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/Quest/Data/QuestCategory.cs` | `QuestCategory` (enum) | `SeedMind.Quest` | T-1 Phase 1 |
| S-02 | `Scripts/Quest/Data/QuestStatus.cs` | `QuestStatus` (enum) | `SeedMind.Quest` | T-1 Phase 1 |
| S-03 | `Scripts/Quest/Data/ObjectiveType.cs` | `ObjectiveType` (enum) | `SeedMind.Quest` | T-1 Phase 1 |
| S-04 | `Scripts/Quest/Data/RewardType.cs` | `RewardType` (enum) | `SeedMind.Quest` | T-1 Phase 1 |
| S-05 | `Scripts/Quest/Data/UnlockConditionType.cs` | `UnlockConditionType` (enum) | `SeedMind.Quest` | T-1 Phase 1 |
| S-06 | `Scripts/Quest/Data/CompositeMode.cs` | `CompositeMode` (enum) | `SeedMind.Quest.Data` | T-1 Phase 1 |
| S-07 | `Scripts/Quest/Data/QuestObjectiveData.cs` | `QuestObjectiveData` ([Serializable]) | `SeedMind.Quest.Data` | T-1 Phase 1 |
| S-08 | `Scripts/Quest/Data/QuestRewardData.cs` | `QuestRewardData` ([Serializable]) | `SeedMind.Quest.Data` | T-1 Phase 1 |
| S-09 | `Scripts/Quest/Data/QuestUnlockCondition.cs` | `QuestUnlockCondition` ([Serializable]) | `SeedMind.Quest.Data` | T-1 Phase 1 |
| S-10 | `Scripts/Quest/Data/QuestData.cs` | `QuestData` (ScriptableObject) | `SeedMind.Quest.Data` | T-1 Phase 1 |
| S-11 | `Scripts/Quest/QuestSaveData.cs` | `QuestSaveData`, `QuestProgressEntry`, `DailyQuestSaveState`, `NPCRequestSaveState`, `CumulativeStatsSaveData` ([Serializable]) | `SeedMind.Quest` | T-1 Phase 1 |
| S-12 | `Scripts/Quest/QuestInstance.cs` | `QuestInstance` (일반 C# 클래스) | `SeedMind.Quest` | T-1 Phase 1 |
| S-13 | `Scripts/Quest/QuestEvents.cs` | `QuestEvents` (static class) | `SeedMind.Quest` | T-1 Phase 2 |
| S-14 | `Scripts/Quest/QuestTracker.cs` | `QuestTracker` (일반 C# 클래스) | `SeedMind.Quest` | T-1 Phase 3 |
| S-15 | `Scripts/Quest/QuestRewarder.cs` | `QuestRewarder` (일반 C# 클래스) | `SeedMind.Quest` | T-1 Phase 3 |
| S-16 | `Scripts/Quest/DailyQuestSelector.cs` | `DailyQuestSelector` (일반 C# 클래스) | `SeedMind.Quest` | T-1 Phase 3 |
| S-17 | `Scripts/Quest/NPCRequestScheduler.cs` | `NPCRequestScheduler` (일반 C# 클래스) | `SeedMind.Quest` | T-1 Phase 3 |
| S-18 | `Scripts/Quest/QuestManager.cs` | `QuestManager` (MonoBehaviour Singleton ISaveable) | `SeedMind.Quest` | T-1 Phase 3 |
| S-19 | `Scripts/UI/QuestLogUI.cs` | `QuestLogUI` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 4 |
| S-20 | `Scripts/UI/QuestTrackingUI.cs` | `QuestTrackingUI` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 4 |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: S-01~S-12 -> S-13 -> S-14~S-18 -> S-19~S-20. 각 Phase 사이에 Unity 컴파일 대기(`execute_menu_item`)가 필요하다.

### 1.3 SO 에셋 목록

| # | 에셋명 | 경로 | SO 타입 | 생성 태스크 |
|---|--------|------|---------|-----------|
| A-01 | `SO_Quest_MainSpring01.asset` | `Assets/_Project/Data/Quests/Main/` | QuestData | T-2-02 |
| A-02 | `SO_Quest_MainSpring02.asset` | `Assets/_Project/Data/Quests/Main/` | QuestData | T-2-03 |
| A-03 | `SO_Quest_MainSpring03.asset` | `Assets/_Project/Data/Quests/Main/` | QuestData | T-2-04 |
| A-04 | `SO_Quest_MainSpring04.asset` | `Assets/_Project/Data/Quests/Main/` | QuestData | T-2-05 |
| A-05 | `SO_Quest_DailyWater.asset` | `Assets/_Project/Data/Quests/Daily/` | QuestData | T-2-06 |
| A-06 | `SO_Quest_DailyHarvest5.asset` | `Assets/_Project/Data/Quests/Daily/` | QuestData | T-2-07 |
| A-07 | `SO_Quest_DailyHarvest10.asset` | `Assets/_Project/Data/Quests/Daily/` | QuestData | T-2-08 |
| A-08 | `SO_Quest_DailySell.asset` | `Assets/_Project/Data/Quests/Daily/` | QuestData | T-2-09 |
| A-09 | `SO_Quest_DailyEarn.asset` | `Assets/_Project/Data/Quests/Daily/` | QuestData | T-2-10 |
| A-10 | `SO_Quest_DailyTill.asset` | `Assets/_Project/Data/Quests/Daily/` | QuestData | T-2-11 |
| A-11 | `SO_Quest_DailyQuality.asset` | `Assets/_Project/Data/Quests/Daily/` | QuestData | T-2-12 |
| A-12 | `SO_Quest_DailyProcess.asset` | `Assets/_Project/Data/Quests/Daily/` | QuestData | T-2-13 |
| A-13 | `SO_Quest_DailyFertilize.asset` | `Assets/_Project/Data/Quests/Daily/` | QuestData | T-2-14 |
| A-14 | `SO_Quest_DailyDiverse.asset` | `Assets/_Project/Data/Quests/Daily/` | QuestData | T-2-15 |
| A-15 | `SO_Quest_DailyGoldQuality.asset` | `Assets/_Project/Data/Quests/Daily/` | QuestData | T-2-16 |
| A-16 | `SO_Quest_DailyEarnLarge.asset` | `Assets/_Project/Data/Quests/Daily/` | QuestData | T-2-17 |
| A-17 | `SO_Quest_FCFirstHarvest.asset` | `Assets/_Project/Data/Quests/Challenge/` | QuestData | T-2-18 |
| A-18 | `SO_Quest_FCEarn1000.asset` | `Assets/_Project/Data/Quests/Challenge/` | QuestData | T-2-19 |
| A-19 | `SO_Quest_FCFirstBuilding.asset` | `Assets/_Project/Data/Quests/Challenge/` | QuestData | T-2-20 |
| A-20 | `SO_Quest_FCFirstProcess.asset` | `Assets/_Project/Data/Quests/Challenge/` | QuestData | T-2-21 |

> **초반 콘텐츠 한정**: 봄 메인 퀘스트 4개 + 일일 목표 풀 12개 + 농장 도전 초반 4개 = 총 20종. 여름/가을/겨울 메인 퀘스트, NPC 의뢰, 추가 농장 도전은 후속 태스크에서 확장한다.

### 1.4 씬 GameObject 목록

| # | 오브젝트명 | 부모 | 컴포넌트 | 생성 태스크 |
|---|-----------|------|----------|-----------|
| G-01 | `QuestManager` | `--- MANAGERS ---` | QuestManager | T-4-01 |
| G-02 | `QuestTrackingWidget` | `Canvas_HUD` | QuestTrackingUI | T-3 Phase 2 |
| G-03 | `QuestLogPanel` | `Canvas_Overlay` | QuestLogUI | T-3 Phase 1 |
| G-04 | `QuestCompletePopup` | `Canvas_Popup` | (Animation + CanvasGroup) | T-3 Phase 3 |
| G-05 | `SCN_Test_Quest.unity` | (독립 씬) | 테스트 전용 | T-6-01 |

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
| `save_scene` | 씬 저장 | T-4, T-6 |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | T-6 |
| `execute_menu_item` | 편집기 명령 실행 (컴파일 대기 등) | T-1 |
| `execute_method` | 런타임 메서드 호출 (테스트) | T-6 |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | T-6 |

---

### 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `Canvas_HUD`, `Canvas_Overlay`, `Canvas_Popup` (UI 루트) | ARC-002 Phase B |
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A |
| `EconomyManager`, `EconomyEvents` | economy-architecture.md |
| `ProgressionManager`, `ProgressionEvents` | progression-architecture.md |
| `InventoryManager` | inventory-architecture.md |
| `NPCManager`, `NPCEvents`, `DialogueSystem` | ARC-008 (npc-shop-tasks.md) |
| `SaveManager`, `ISaveable` | ARC-011 (save-load-architecture.md) |
| `TutorialManager`, `TutorialEvents` | tutorial-architecture.md |
| `TimeManager` | ARC-002 |
| `BuildingManager`, `BuildingEvents` | facilities-tasks.md (ARC-007) |
| `FarmEvents` | ARC-003 (farming-tasks.md) |
| `ToolUpgradeEvents` | ARC-015 (tool-upgrade-tasks.md) |

---

## 2. T-1: 스크립트 생성

**목적**: 퀘스트 시스템에 필요한 모든 C# 스크립트를 생성한다.

**전제**: Core 인프라(TimeManager, SaveManager 등) 컴파일 완료. Economy/Player/Building/NPC/Tutorial/Progression 모듈 컴파일 완료.

---

### T-1 Phase 1: 데이터 구조 스크립트 (S-01 ~ S-12)

#### T-1-01: 폴더 생성

```
create_folder
  path: "Assets/_Project/Scripts/Quest"

create_folder
  path: "Assets/_Project/Scripts/Quest/Data"
```

- **MCP 호출**: 2회

#### T-1-02: QuestCategory enum (S-01)

```
create_script
  path: "Assets/_Project/Scripts/Quest/Data/QuestCategory.cs"
  content: |
    // S-01: 퀘스트 카테고리 열거형
    // -> see docs/systems/quest-architecture.md 섹션 2.1
    // -> see docs/systems/quest-system.md 섹션 1 for 카테고리 정의
    namespace SeedMind.Quest
    {
        public enum QuestCategory
        {
            MainQuest        = 0,   // 계절별 메인 퀘스트
            NPCRequest       = 1,   // NPC 의뢰
            DailyChallenge   = 2,   // 일일 목표
            FarmChallenge    = 3    // 농장 도전
        }
    }
```

- **MCP 호출**: 1회

#### T-1-03: QuestStatus enum (S-02)

```
create_script
  path: "Assets/_Project/Scripts/Quest/Data/QuestStatus.cs"
  content: |
    // S-02: 퀘스트 상태 열거형
    // -> see docs/systems/quest-architecture.md 섹션 2.2
    // -> see docs/systems/quest-system.md 섹션 2.5 for 상태 전이 정의
    namespace SeedMind.Quest
    {
        public enum QuestStatus
        {
            Locked      = 0,   // 해금 조건 미충족
            Available   = 1,   // 해금됨, 수락 대기
            Active      = 2,   // 진행 중
            Completed   = 3,   // 목표 달성, 보상 수령 대기
            Rewarded    = 4,   // 보상 수령 완료
            Failed      = 5,   // 시간 초과 실패
            Expired     = 6    // 만료 (일일 목표 자동 소멸)
        }
    }
```

- **MCP 호출**: 1회

#### T-1-04: ObjectiveType enum (S-03)

```
create_script
  path: "Assets/_Project/Scripts/Quest/Data/ObjectiveType.cs"
  content: |
    // S-03: 퀘스트 목표 타입 열거형
    // -> see docs/systems/quest-architecture.md 섹션 2.3
    // -> see docs/systems/quest-system.md 섹션 2.2 for 목표 타입 정의
    namespace SeedMind.Quest
    {
        public enum ObjectiveType
        {
            Harvest         = 0,   // 작물 수확
            Sell            = 1,   // 아이템 판매
            Deliver         = 2,   // NPC에게 납품
            Process         = 3,   // 가공품 제작
            Build           = 4,   // 시설 건설
            EarnGold        = 5,   // 골드 획득
            Till            = 6,   // 경작지 생성
            Water           = 7,   // 물주기
            QualityHarvest  = 8,   // 특정 품질 이상 수확
            UpgradeTool     = 9,   // 도구 업그레이드
            ReachLevel      = 10,  // 레벨 도달
            Composite       = 11   // 복합 (AND/OR)
        }
    }
```

- **MCP 호출**: 1회

#### T-1-05: RewardType enum (S-04)

```
create_script
  path: "Assets/_Project/Scripts/Quest/Data/RewardType.cs"
  content: |
    // S-04: 퀘스트 보상 타입 열거형
    // -> see docs/systems/quest-architecture.md 섹션 2.4
    // -> see docs/systems/quest-system.md 섹션 2.3 for 보상 타입 정의
    namespace SeedMind.Quest
    {
        public enum RewardType
        {
            Gold            = 0,
            XP              = 1,
            Item            = 2,
            Recipe          = 3,   // 가공 레시피 해금
            Unlock          = 4    // 시설/기능 해금
        }
    }
```

- **MCP 호출**: 1회

#### T-1-06: UnlockConditionType enum (S-05)

```
create_script
  path: "Assets/_Project/Scripts/Quest/Data/UnlockConditionType.cs"
  content: |
    // S-05: 퀘스트 해금 조건 타입 열거형
    // -> see docs/systems/quest-architecture.md 섹션 2.5
    // -> see docs/systems/quest-system.md 섹션 2.4 for 해금 조건 정의
    namespace SeedMind.Quest
    {
        public enum UnlockConditionType
        {
            Level              = 0,
            Season             = 1,
            QuestComplete      = 2,
            FacilityBuilt      = 3,
            DayOfSeason        = 4,
            TutorialComplete   = 5
        }
    }
```

- **MCP 호출**: 1회

#### T-1-07: CompositeMode enum (S-06)

```
create_script
  path: "Assets/_Project/Scripts/Quest/Data/CompositeMode.cs"
  content: |
    // S-06: 복합 목표 모드 열거형
    // -> see docs/systems/quest-architecture.md 섹션 4.2
    namespace SeedMind.Quest.Data
    {
        public enum CompositeMode
        {
            And = 0,
            Or  = 1
        }
    }
```

- **MCP 호출**: 1회

#### T-1-08: QuestObjectiveData 직렬화 클래스 (S-07)

```
create_script
  path: "Assets/_Project/Scripts/Quest/Data/QuestObjectiveData.cs"
  content: |
    // S-07: 퀘스트 목표 데이터 (직렬화 클래스)
    // -> see docs/systems/quest-architecture.md 섹션 4.2
    using UnityEngine;

    namespace SeedMind.Quest.Data
    {
        [System.Serializable]
        public class QuestObjectiveData
        {
            public ObjectiveType type;
            public string targetId;                   // 대상 ID (""이면 any)
            public int requiredAmount;                // 목표 수량 (-> see docs/systems/quest-system.md)
            public int minQuality;                    // 최소 품질 (QualityHarvest용, 0이면 무관)
            [TextArea(1, 2)]
            public string descriptionKR;              // 목표 설명 텍스트

            // Composite 전용
            public CompositeMode compositeMode;
            public QuestObjectiveData[] subObjectives;
        }
    }
```

- **MCP 호출**: 1회

#### T-1-09: QuestRewardData 직렬화 클래스 (S-08)

```
create_script
  path: "Assets/_Project/Scripts/Quest/Data/QuestRewardData.cs"
  content: |
    // S-08: 퀘스트 보상 데이터 (직렬화 클래스)
    // -> see docs/systems/quest-architecture.md 섹션 4.3
    namespace SeedMind.Quest.Data
    {
        [System.Serializable]
        public class QuestRewardData
        {
            public RewardType type;
            public int amount;                        // 수량 (-> see docs/systems/quest-system.md 섹션 7)
            public string targetId;                   // 대상 ID (Gold/XP는 "")
            public bool scaledByLevel;                // 레벨 스케일 적용 여부
                                                      // -> see docs/systems/quest-system.md 섹션 5.2
        }
    }
```

- **MCP 호출**: 1회

#### T-1-10: QuestUnlockCondition 직렬화 클래스 (S-09)

```
create_script
  path: "Assets/_Project/Scripts/Quest/Data/QuestUnlockCondition.cs"
  content: |
    // S-09: 퀘스트 해금 조건 (직렬화 클래스)
    // -> see docs/systems/quest-architecture.md 섹션 4.4
    namespace SeedMind.Quest.Data
    {
        [System.Serializable]
        public class QuestUnlockCondition
        {
            public UnlockConditionType type;
            public string stringParam;                // 문자열 파라미터 (퀘스트ID, 시설ID)
            public int intParam;                      // 정수 파라미터 (레벨, 일차)
                                                      // -> see docs/systems/quest-system.md 섹션 2.4
            public Season seasonParam;                // 계절 파라미터 (Season 조건용)
        }
    }
```

- **MCP 호출**: 1회

#### T-1-11: QuestData ScriptableObject (S-10)

```
create_script
  path: "Assets/_Project/Scripts/Quest/Data/QuestData.cs"
  content: |
    // S-10: 퀘스트 정적 정의 ScriptableObject
    // -> see docs/systems/quest-architecture.md 섹션 4.1
    using UnityEngine;

    namespace SeedMind.Quest.Data
    {
        [CreateAssetMenu(fileName = "NewQuestData", menuName = "SeedMind/QuestData")]
        public class QuestData : ScriptableObject
        {
            [Header("기본 정보")]
            public string questId;
            public QuestCategory category;
            public string titleKR;
            [TextArea(2, 4)]
            public string descriptionKR;
            public string giverId;                    // "system"이면 시스템 자동 부여

            [Header("목표")]
            public QuestObjectiveData[] objectives;

            [Header("보상")]
            public QuestRewardData[] rewards;

            [Header("해금 조건")]
            public QuestUnlockCondition[] unlockConditions;

            [Header("제한")]
            public int timeLimitDays;                 // 0이면 무기한
                                                      // -> see docs/systems/quest-system.md 섹션 2.1
            public Season season;                     // None이면 전 계절
            public bool isRepeatable;

            [Header("UI")]
            public Sprite icon;                       // null이면 카테고리 기본 아이콘
        }
    }
```

- **MCP 호출**: 1회

#### T-1-12: QuestSaveData 직렬화 클래스 (S-11)

```
create_script
  path: "Assets/_Project/Scripts/Quest/QuestSaveData.cs"
  content: |
    // S-11: 퀘스트 세이브 데이터 (전체 Serializable 클래스들)
    // -> see docs/systems/quest-architecture.md 섹션 8.2
    using System.Collections.Generic;

    namespace SeedMind.Quest
    {
        [System.Serializable]
        public class QuestSaveData
        {
            public QuestProgressEntry[] questProgress;
            public string[] completedQuestIds;
            public DailyQuestSaveState dailyState;
            public NPCRequestSaveState npcRequestState;
            public CumulativeStatsSaveData cumulativeStats;
        }

        [System.Serializable]
        public class QuestProgressEntry
        {
            public string questId;
            public int status;                        // QuestStatus enum (int)
            public int[] objectiveProgress;
            public int acceptedDay;                   // -1 = 미수락
            public int completedDay;                  // -1 = 미완료
            public bool isTracked;
        }

        [System.Serializable]
        public class DailyQuestSaveState
        {
            public int lastSelectedDay;
            public string[] previousDailyIds;
            public string[] todayDailyIds;
        }

        [System.Serializable]
        public class NPCRequestSaveState
        {
            public Dictionary<string, int> cooldowns; // NPC ID -> 남은 쿨다운 일수
            public int activeRequestCount;
        }

        [System.Serializable]
        public class CumulativeStatsSaveData
        {
            public int totalHarvested;
            public int totalSold;
            public int totalProcessed;
            public int totalBuilt;
        }
    }
```

- **MCP 호출**: 1회

#### T-1-13: QuestInstance 런타임 클래스 (S-12)

```
create_script
  path: "Assets/_Project/Scripts/Quest/QuestInstance.cs"
  content: |
    // S-12: 런타임 퀘스트 상태 래퍼
    // -> see docs/systems/quest-architecture.md 섹션 5.1
    using UnityEngine;
    using SeedMind.Quest.Data;

    namespace SeedMind.Quest
    {
        public class QuestInstance
        {
            public QuestData Data { get; private set; }
            public QuestStatus Status { get; set; }
            public int[] ObjectiveProgress { get; private set; }
            public int AcceptedDay { get; set; }
            public int CompletedDay { get; set; }
            public bool IsTracked { get; set; }

            public QuestInstance(QuestData data)
            {
                Data = data;
                Status = QuestStatus.Locked;
                ObjectiveProgress = new int[data.objectives.Length];
                AcceptedDay = -1;
                CompletedDay = -1;
                IsTracked = false;
            }

            public void UpdateProgress(int objectiveIndex, int delta)
            {
                if (Status != QuestStatus.Active) return;
                ObjectiveProgress[objectiveIndex] += delta;
                int required = Data.objectives[objectiveIndex].requiredAmount;
                ObjectiveProgress[objectiveIndex] = Mathf.Min(
                    ObjectiveProgress[objectiveIndex], required);
            }

            public bool IsObjectiveComplete(int objectiveIndex)
                => ObjectiveProgress[objectiveIndex]
                   >= Data.objectives[objectiveIndex].requiredAmount;

            public bool AreAllObjectivesComplete()
            {
                for (int i = 0; i < ObjectiveProgress.Length; i++)
                    if (!IsObjectiveComplete(i)) return false;
                return true;
            }

            public float GetOverallProgress()
            {
                if (Data.objectives.Length == 0) return 1f;
                float total = 0f;
                for (int i = 0; i < Data.objectives.Length; i++)
                    total += (float)ObjectiveProgress[i]
                             / Data.objectives[i].requiredAmount;
                return total / Data.objectives.Length;
            }

            public int GetRemainingDays(int currentDay)
            {
                if (Data.timeLimitDays <= 0) return -1;
                return Data.timeLimitDays - (currentDay - AcceptedDay);
            }

            public bool IsExpired(int currentDay)
            {
                if (Data.timeLimitDays <= 0) return false;
                return GetRemainingDays(currentDay) <= 0;
            }
        }
    }
```

- **MCP 호출**: 1회
- **Phase 1 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

### T-1 Phase 2: 이벤트 허브 스크립트 (S-13)

#### T-1-14: QuestEvents static class (S-13)

```
create_script
  path: "Assets/_Project/Scripts/Quest/QuestEvents.cs"
  content: |
    // S-13: 퀘스트 정적 이벤트 허브
    // -> see docs/systems/quest-architecture.md 섹션 6.2
    using System;

    namespace SeedMind.Quest
    {
        public static class QuestEvents
        {
            // --- 상태 변경 ---
            public static event Action<QuestInstance> OnQuestUnlocked;
            public static event Action<QuestInstance> OnQuestActivated;
            public static event Action<QuestInstance> OnQuestCompleted;
            public static event Action<QuestInstance> OnQuestRewarded;
            public static event Action<QuestInstance> OnQuestFailed;

            // --- 진행도 ---
            public static event Action<QuestInstance, int> OnObjectiveProgress;

            // --- 일일 목표 ---
            public static event Action<QuestInstance[]> OnDailyQuestsSelected;

            // --- NPC 의뢰 ---
            public static event Action<QuestInstance> OnNPCRequestAvailable;

            // --- Raise 메서드 ---
            public static void RaiseQuestUnlocked(QuestInstance q)
                => OnQuestUnlocked?.Invoke(q);
            public static void RaiseQuestActivated(QuestInstance q)
                => OnQuestActivated?.Invoke(q);
            public static void RaiseQuestCompleted(QuestInstance q)
                => OnQuestCompleted?.Invoke(q);
            public static void RaiseQuestRewarded(QuestInstance q)
                => OnQuestRewarded?.Invoke(q);
            public static void RaiseQuestFailed(QuestInstance q)
                => OnQuestFailed?.Invoke(q);
            public static void RaiseObjectiveProgress(QuestInstance q, int idx)
                => OnObjectiveProgress?.Invoke(q, idx);
            public static void RaiseDailyQuestsSelected(QuestInstance[] quests)
                => OnDailyQuestsSelected?.Invoke(quests);
            public static void RaiseNPCRequestAvailable(QuestInstance q)
                => OnNPCRequestAvailable?.Invoke(q);
        }
    }
```

- **MCP 호출**: 1회
- **Phase 2 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

### T-1 Phase 3: 시스템 클래스 스크립트 (S-14 ~ S-18)

#### T-1-15: QuestTracker (S-14)

```
create_script
  path: "Assets/_Project/Scripts/Quest/QuestTracker.cs"
  content: |
    // S-14: 이벤트 구독 기반 퀘스트 진행도 추적
    // -> see docs/systems/quest-architecture.md 섹션 3.3, 6.3~6.4
    using UnityEngine;
    using SeedMind.Quest.Data;

    namespace SeedMind.Quest
    {
        public class QuestTracker
        {
            private QuestManager _manager;
            private CumulativeStatsSaveData _cumulativeStats;

            public QuestTracker(QuestManager manager)
            {
                _manager = manager;
                _cumulativeStats = new CumulativeStatsSaveData();
            }

            public void SubscribeAll()
            {
                // 이벤트 구독 매핑:
                // -> see docs/systems/quest-architecture.md 섹션 6.3
                // FarmEvents.OnCropHarvested += OnCropHarvested;
                // FarmEvents.OnTileTilled += OnTileTilled;
                // FarmEvents.OnCropWatered += OnCropWatered;
                // EconomyEvents.OnItemSold += OnItemSold;
                // BuildingEvents.OnConstructionCompleted += OnBuildingCompleted;
                // ProgressionEvents.OnLevelUp += OnLevelReached;
                // ToolUpgradeEvents.OnUpgradeCompleted += OnToolUpgraded;
                // ProcessingEvents.OnProcessingCompleted += OnItemProcessed;
                // NPCEvents.OnItemDelivered += OnItemDelivered;
            }

            public void UnsubscribeAll() { /* 전체 구독 해제 */ }

            public void UpdateObjective(ObjectiveType type,
                string targetId, int delta, int quality = 0)
            {
                // -> see docs/systems/quest-architecture.md 섹션 6.4
                // 활성 퀘스트 순회 -> 목표 매칭 -> 진행도 갱신 -> 완료 판정
            }

            public CumulativeStatsSaveData GetCumulativeStats()
                => _cumulativeStats;
            public void LoadCumulativeStats(CumulativeStatsSaveData data)
            {
                if (data != null) _cumulativeStats = data;
            }
        }
    }
```

- **MCP 호출**: 1회

#### T-1-16: QuestRewarder (S-15)

```
create_script
  path: "Assets/_Project/Scripts/Quest/QuestRewarder.cs"
  content: |
    // S-15: 퀘스트 보상 지급 처리
    // -> see docs/systems/quest-architecture.md 섹션 3.4
    using UnityEngine;
    using SeedMind.Quest.Data;

    namespace SeedMind.Quest
    {
        public class QuestRewarder
        {
            // 외부 시스템 참조 (Initialize 시 주입)
            // private EconomyManager _economyManager;
            // private ProgressionManager _progressionManager;
            // private InventoryManager _inventoryManager;

            public void GrantRewards(QuestData questData, int playerLevel)
            {
                foreach (var reward in questData.rewards)
                {
                    int amount = reward.scaledByLevel
                        ? ApplyLevelScale(reward.amount, playerLevel)
                        : reward.amount;
                    // switch (reward.type) 분기 처리
                    // -> see docs/systems/quest-architecture.md 섹션 3.4
                }
            }

            private int ApplyLevelScale(int baseValue, int playerLevel)
            {
                // -> see docs/systems/quest-system.md 섹션 5.2 for 스케일 공식
                float scale = 1f + (playerLevel - 1) * 0.1f; // -> see canonical
                scale = Mathf.Min(scale, 1.9f);               // -> see canonical
                return Mathf.RoundToInt(baseValue * scale);
            }
        }
    }
```

- **MCP 호출**: 1회

#### T-1-17: DailyQuestSelector (S-16)

```
create_script
  path: "Assets/_Project/Scripts/Quest/DailyQuestSelector.cs"
  content: |
    // S-16: 일일 목표 2개 랜덤 선택, 중복 방지
    // -> see docs/systems/quest-architecture.md 섹션 3.1
    // -> see docs/systems/quest-system.md 섹션 5.1 for 선택 규칙
    using SeedMind.Quest.Data;

    namespace SeedMind.Quest
    {
        public class DailyQuestSelector
        {
            private QuestData[] _dailyPool;
            private string[] _previousDailyIds;
            private string[] _todayDailyIds;
            private int _lastSelectedDay;

            public DailyQuestSelector(QuestData[] dailyPool)
            {
                _dailyPool = dailyPool;
                _previousDailyIds = System.Array.Empty<string>();
                _todayDailyIds = System.Array.Empty<string>();
            }

            public QuestData[] SelectDailyQuests(int currentDay, int playerLevel,
                bool hasProcessor)
            {
                // 선택 규칙: -> see docs/systems/quest-system.md 섹션 5.1
                // 1) 풀에서 조건 필터 (레벨, 가공소 보유 여부)
                // 2) 전날과 동일 목표 제외
                // 3) 2개 랜덤 선택
                return null; // 구현 시 교체
            }

            public DailyQuestSaveState GetSaveState()
            {
                return new DailyQuestSaveState
                {
                    lastSelectedDay = _lastSelectedDay,
                    previousDailyIds = _previousDailyIds,
                    todayDailyIds = _todayDailyIds
                };
            }

            public void LoadSaveState(DailyQuestSaveState state)
            {
                if (state == null) return;
                _lastSelectedDay = state.lastSelectedDay;
                _previousDailyIds = state.previousDailyIds
                    ?? System.Array.Empty<string>();
                _todayDailyIds = state.todayDailyIds
                    ?? System.Array.Empty<string>();
            }
        }
    }
```

- **MCP 호출**: 1회

#### T-1-18: NPCRequestScheduler (S-17)

```
create_script
  path: "Assets/_Project/Scripts/Quest/NPCRequestScheduler.cs"
  content: |
    // S-17: NPC 의뢰 등장/쿨다운 관리
    // -> see docs/systems/quest-architecture.md 섹션 7.1
    // -> see docs/systems/quest-system.md 섹션 4.1 for 의뢰 규칙
    using System.Collections.Generic;
    using SeedMind.Quest.Data;

    namespace SeedMind.Quest
    {
        public class NPCRequestScheduler
        {
            private QuestData[] _npcRequestPool;
            private QuestManager _manager;
            private Dictionary<string, int> _npcCooldowns;
            private int _activeRequestCount;

            public NPCRequestScheduler(QuestData[] npcRequestPool,
                QuestManager manager)
            {
                _npcRequestPool = npcRequestPool;
                _manager = manager;
                _npcCooldowns = new Dictionary<string, int>();
            }

            public void TryOfferNewRequests(int currentDay,
                /* Season season, */ int playerLevel) { /* 의뢰 제안 로직 */ }
            public void UpdateCooldowns() { /* 쿨다운 1일 감소 */ }
            public void OnRequestCompleted(string questId) { /* 완료 처리 */ }
            public void OnRequestFailed(string questId) { /* 실패 처리, 쿨다운 시작 */ }

            public NPCRequestSaveState GetSaveState()
            {
                return new NPCRequestSaveState
                {
                    cooldowns = new Dictionary<string, int>(_npcCooldowns),
                    activeRequestCount = _activeRequestCount
                };
            }

            public void LoadSaveState(NPCRequestSaveState state)
            {
                if (state == null) return;
                _npcCooldowns = state.cooldowns
                    ?? new Dictionary<string, int>();
                _activeRequestCount = state.activeRequestCount;
            }
        }
    }
```

- **MCP 호출**: 1회

#### T-1-19: QuestManager (S-18)

```
create_script
  path: "Assets/_Project/Scripts/Quest/QuestManager.cs"
  content: |
    // S-18: 퀘스트 생명주기 관리 (MonoBehaviour Singleton, ISaveable)
    // -> see docs/systems/quest-architecture.md 섹션 3.2
    using UnityEngine;
    using System.Collections.Generic;
    using SeedMind.Quest.Data;

    namespace SeedMind.Quest
    {
        public class QuestManager : MonoBehaviour //, ISaveable
        {
            [SerializeField] private QuestData[] _allQuests;
            [SerializeField] private QuestData[] _dailyQuestPool;

            private QuestTracker _tracker;
            private QuestRewarder _rewarder;
            private DailyQuestSelector _dailySelector;
            private NPCRequestScheduler _npcScheduler;

            private Dictionary<string, QuestInstance> _activeQuests
                = new Dictionary<string, QuestInstance>();
            private HashSet<string> _completedQuestIds = new HashSet<string>();

            // ISaveable
            public int SaveLoadOrder => 85; // -> see docs/systems/quest-architecture.md 섹션 8.1

            public void Initialize()
            {
                // -> see docs/systems/quest-architecture.md 섹션 3.2
                // 1) QuestInstance 생성 2) 해금 판정 3) 자동 Active 전환
                // 4) _tracker 초기화 5) _dailySelector 첫 날 선택
            }

            public bool AcceptQuest(string questId) { return false; }
            public bool AbandonQuest(string questId) { return false; }
            public bool ClaimReward(string questId) { return false; }
            public IReadOnlyList<QuestInstance> GetActiveQuests()
                => new List<QuestInstance>();
            public IReadOnlyList<QuestInstance> GetQuestsByCategory(
                QuestCategory cat) => new List<QuestInstance>();
            public bool IsQuestCompleted(string questId)
                => _completedQuestIds.Contains(questId);
            public QuestInstance GetTrackedQuest() => null;
            public void SetTrackedQuest(string questId) { }

            public object GetSaveData()
            {
                // -> see docs/systems/quest-architecture.md 섹션 8.3
                return new QuestSaveData();
            }
            public void LoadSaveData(object rawData)
            {
                // -> see docs/systems/quest-architecture.md 섹션 8.3
            }

            // [구독] TimeManager.OnDayChanged -> CheckDailyReset, CheckTimeLimits
            // [구독] TimeManager.OnSeasonChanged -> UnlockSeasonalQuests
            // [구독] ProgressionEvents.OnLevelUp -> CheckLevelUnlocks
            // [구독] TutorialEvents.OnTutorialCompleted -> ActivateQuestSystem
            // [구독] NPCEvents.OnRequestAccepted -> AcceptNPCQuest
        }
    }
```

- **MCP 호출**: 1회
- **Phase 3 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

### T-1 Phase 4: UI 스크립트 (S-19 ~ S-20)

#### T-1-20: QuestLogUI (S-19)

```
create_script
  path: "Assets/_Project/Scripts/UI/QuestLogUI.cs"
  content: |
    // S-19: 퀘스트 로그 UI 컨트롤러
    // -> see docs/systems/quest-architecture.md 섹션 10
    // -> see docs/systems/quest-system.md 섹션 8.1 for UI 구조
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using SeedMind.Quest;

    namespace SeedMind.UI
    {
        public class QuestLogUI : MonoBehaviour
        {
            [Header("패널")]
            [SerializeField] private GameObject _logPanel;

            [Header("탭 버튼")]
            [SerializeField] private Button _tabMainQuest;
            [SerializeField] private Button _tabNPCRequest;
            [SerializeField] private Button _tabDailyChallenge;
            [SerializeField] private Button _tabFarmChallenge;

            [Header("콘텐츠")]
            [SerializeField] private Transform _questListContainer;
            [SerializeField] private GameObject _questEntryPrefab;
            [SerializeField] private TextMeshProUGUI _questDetailTitle;
            [SerializeField] private TextMeshProUGUI _questDetailDesc;
            [SerializeField] private Transform _objectiveListContainer;

            private QuestCategory _currentTab = QuestCategory.MainQuest;

            public void Toggle()
            {
                _logPanel.SetActive(!_logPanel.activeSelf);
                if (_logPanel.activeSelf) RefreshList();
            }

            private void OnEnable()
            {
                QuestEvents.OnQuestActivated += OnQuestChanged;
                QuestEvents.OnQuestCompleted += OnQuestChanged;
                QuestEvents.OnObjectiveProgress += OnProgressChanged;
            }
            private void OnDisable()
            {
                QuestEvents.OnQuestActivated -= OnQuestChanged;
                QuestEvents.OnQuestCompleted -= OnQuestChanged;
                QuestEvents.OnObjectiveProgress -= OnProgressChanged;
            }

            private void OnQuestChanged(QuestInstance q) { RefreshList(); }
            private void OnProgressChanged(QuestInstance q, int idx)
                { RefreshList(); }
            private void RefreshList() { /* 현재 탭의 퀘스트 목록 갱신 */ }
            private void ShowQuestDetail(QuestInstance quest) { /* 상세 표시 */ }
        }
    }
```

- **MCP 호출**: 1회

#### T-1-21: QuestTrackingUI (S-20)

```
create_script
  path: "Assets/_Project/Scripts/UI/QuestTrackingUI.cs"
  content: |
    // S-20: HUD 퀘스트 추적 위젯
    // -> see docs/systems/quest-architecture.md 섹션 10
    // -> see docs/systems/quest-system.md 섹션 8.3 for 추적 위젯 구조
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using SeedMind.Quest;

    namespace SeedMind.UI
    {
        public class QuestTrackingUI : MonoBehaviour
        {
            [Header("위젯")]
            [SerializeField] private GameObject _trackingWidget;
            [SerializeField] private TextMeshProUGUI _questTitleText;
            [SerializeField] private TextMeshProUGUI _objectiveText;
            [SerializeField] private Slider _progressBar;

            private void OnEnable()
            {
                QuestEvents.OnQuestActivated += OnQuestChanged;
                QuestEvents.OnQuestCompleted += OnQuestChanged;
                QuestEvents.OnObjectiveProgress += OnProgressChanged;
            }
            private void OnDisable()
            {
                QuestEvents.OnQuestActivated -= OnQuestChanged;
                QuestEvents.OnQuestCompleted -= OnQuestChanged;
                QuestEvents.OnObjectiveProgress -= OnProgressChanged;
            }

            private void OnQuestChanged(QuestInstance q) { Refresh(); }
            private void OnProgressChanged(QuestInstance q, int idx)
                { Refresh(); }
            private void Refresh()
            {
                // QuestManager.GetTrackedQuest() -> 표시 갱신
            }
        }
    }
```

- **MCP 호출**: 1회
- **Phase 4 완료 후**: `execute_menu_item` -> Unity 컴파일 대기 (1회)

---

**T-1 합계**: 폴더 2회 + 스크립트 20회 + 컴파일 대기 4회 = **24회** (컴파일 대기 포함)

---

## 3. T-2: SO 에셋 생성

**목적**: 퀘스트 시스템에 필요한 모든 QuestData ScriptableObject 에셋을 생성하고 필드를 설정한다.

**전제**: T-1 모든 Phase 컴파일 완료. QuestData, QuestObjectiveData, QuestRewardData, QuestUnlockCondition 클래스가 Unity에서 인식 가능한 상태.

---

### T-2-01: 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Quests"

create_folder
  path: "Assets/_Project/Data/Quests/Main"

create_folder
  path: "Assets/_Project/Data/Quests/Daily"

create_folder
  path: "Assets/_Project/Data/Quests/Challenge"

create_folder
  path: "Assets/_Project/Data/Quests/NPC"
```

- **MCP 호출**: 5회

---

### T-2-02: SO_Quest_MainSpring01 (봄의 첫 수확)

```
create_scriptable_object
  type: "SeedMind.Quest.Data.QuestData"
  asset_path: "Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring01.asset"

set_property  target: "SO_Quest_MainSpring01"
  questId = "main_spring_01"
  category = 0                                     // MainQuest
  titleKR = "봄의 첫 수확"                           // -> see docs/systems/quest-system.md 섹션 3.1
  descriptionKR = "감자 또는 당근 10개를 수확하세요"  // -> see docs/systems/quest-system.md 섹션 3.1
  giverId = "system"
  timeLimitDays = 0                                // 무기한
  season = 0                                       // Spring
  isRepeatable = false

  // objectives[0]:
  //   type = 0 (Harvest), targetId = "", requiredAmount = 10
  //   descriptionKR = "감자 또는 당근 10개 수확"
  // -> see docs/systems/quest-system.md 섹션 3.1 for 목표 상세

  // rewards[0]: type = 0 (Gold), amount = (-> see quest-system.md 섹션 3.1)
  // rewards[1]: type = 1 (XP), amount = (-> see quest-system.md 섹션 3.1)

  // unlockConditions[0]:
  //   type = 5 (TutorialComplete), intParam = 1
```

> **주의**: `objectives`, `rewards`, `unlockConditions` 배열은 중첩 직렬화 데이터이며 MCP `set_property`로 개별 요소 설정이 어려울 수 있다 (PATTERN-006). 수치는 MCP 실행 시점에 canonical 문서(`docs/systems/quest-system.md` 섹션 3.1)에서 읽어 입력한다.

- **MCP 호출**: 1(생성) + ~3(기본 필드) + ~3(배열 필드) = **~7회**

---

### T-2-03: SO_Quest_MainSpring02 (농장 확장의 시작)

```
create_scriptable_object
  type: "SeedMind.Quest.Data.QuestData"
  asset_path: "Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring02.asset"

set_property  target: "SO_Quest_MainSpring02"
  questId = "main_spring_02"
  category = 0                                     // MainQuest
  titleKR = "농장 확장의 시작"                       // -> see docs/systems/quest-system.md 섹션 3.1
  descriptionKR = "경작지를 20타일 이상 보유하세요"   // -> see docs/systems/quest-system.md 섹션 3.1
  giverId = "system"
  timeLimitDays = 0
  season = 0                                       // Spring
  isRepeatable = false

  // objectives[0]: type = 6 (Till), requiredAmount = (-> see quest-system.md 섹션 3.1)
  // rewards: (-> see quest-system.md 섹션 3.1)
  // unlockConditions[0]: type = 2 (QuestComplete), stringParam = "main_spring_01"
```

- **MCP 호출**: **~7회**

---

### T-2-04: SO_Quest_MainSpring03 (다양한 작물 재배)

```
create_scriptable_object
  type: "SeedMind.Quest.Data.QuestData"
  asset_path: "Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring03.asset"

set_property  target: "SO_Quest_MainSpring03"
  questId = "main_spring_03"
  category = 0
  titleKR = "다양한 작물 재배"                       // -> see docs/systems/quest-system.md 섹션 3.1
  descriptionKR = "서로 다른 작물 2종 이상 동시 재배" // -> see docs/systems/quest-system.md 섹션 3.1
  giverId = "system"
  timeLimitDays = 0
  season = 0
  isRepeatable = false

  // objectives, rewards, unlockConditions: (-> see quest-system.md 섹션 3.1)
```

- **MCP 호출**: **~7회**

---

### T-2-05: SO_Quest_MainSpring04 (첫 번째 출하 목표)

```
create_scriptable_object
  type: "SeedMind.Quest.Data.QuestData"
  asset_path: "Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring04.asset"

set_property  target: "SO_Quest_MainSpring04"
  questId = "main_spring_04"
  category = 0
  titleKR = "첫 번째 출하 목표"                     // -> see docs/systems/quest-system.md 섹션 3.1
  descriptionKR = "총 판매 수익 500G를 달성하세요"   // -> see docs/systems/quest-system.md 섹션 3.1
  giverId = "system"
  timeLimitDays = 0
  season = 0
  isRepeatable = false

  // objectives[0]: type = 5 (EarnGold), requiredAmount = (-> see quest-system.md 섹션 3.1)
  // rewards, unlockConditions: (-> see quest-system.md 섹션 3.1)
```

- **MCP 호출**: **~7회**

---

### T-2-06 ~ T-2-17: 일일 목표 SO 에셋 생성 (12종)

각 일일 목표 SO를 생성한다. 퀘스트 데이터 값은 canonical 문서에서 읽어 설정한다.

#### T-2-06: SO_Quest_DailyWater

```
create_scriptable_object
  type: "SeedMind.Quest.Data.QuestData"
  asset_path: "Assets/_Project/Data/Quests/Daily/SO_Quest_DailyWater.asset"

set_property  target: "SO_Quest_DailyWater"
  questId = "daily_water"
  category = 2                                     // DailyChallenge
  titleKR = "부지런한 농부"                          // -> see docs/systems/quest-system.md 섹션 5.1
  giverId = "system"
  timeLimitDays = 1
  isRepeatable = true

  // objectives[0]: type = 7 (Water), requiredAmount = (-> see quest-system.md 섹션 5.1)
  // rewards: (-> see quest-system.md 섹션 5.1), scaledByLevel = true
```

- **MCP 호출**: **~4회**

#### T-2-07 ~ T-2-17 (SO_Quest_DailyHarvest5 ~ SO_Quest_DailyEarnLarge)

나머지 11종 일일 목표 SO도 동일 패턴으로 생성한다. 각 에셋의 세부 값은 canonical 문서를 참조한다.

| 태스크 | 에셋명 | questId | Canonical 참조 |
|--------|--------|---------|---------------|
| T-2-07 | SO_Quest_DailyHarvest5 | `daily_harvest_5` | quest-system.md 섹션 5.1 |
| T-2-08 | SO_Quest_DailyHarvest10 | `daily_harvest_10` | quest-system.md 섹션 5.1 |
| T-2-09 | SO_Quest_DailySell | `daily_sell` | quest-system.md 섹션 5.1 |
| T-2-10 | SO_Quest_DailyEarn | `daily_earn` | quest-system.md 섹션 5.1 |
| T-2-11 | SO_Quest_DailyTill | `daily_till` | quest-system.md 섹션 5.1 |
| T-2-12 | SO_Quest_DailyQuality | `daily_quality` | quest-system.md 섹션 5.1 |
| T-2-13 | SO_Quest_DailyProcess | `daily_process` | quest-system.md 섹션 5.1 |
| T-2-14 | SO_Quest_DailyFertilize | `daily_fertilize` | quest-system.md 섹션 5.1 |
| T-2-15 | SO_Quest_DailyDiverse | `daily_diverse` | quest-system.md 섹션 5.1 |
| T-2-16 | SO_Quest_DailyGoldQuality | `daily_gold_quality` | quest-system.md 섹션 5.1 |
| T-2-17 | SO_Quest_DailyEarnLarge | `daily_earn_large` | quest-system.md 섹션 5.1 |

- **MCP 호출**: 각 ~4회 x 11종 = **~44회**

---

### T-2-18 ~ T-2-21: 농장 도전 SO 에셋 생성 (초반 4종)

#### T-2-18: SO_Quest_FCFirstHarvest

```
create_scriptable_object
  type: "SeedMind.Quest.Data.QuestData"
  asset_path: "Assets/_Project/Data/Quests/Challenge/SO_Quest_FCFirstHarvest.asset"

set_property  target: "SO_Quest_FCFirstHarvest"
  questId = "fc_first_harvest"
  category = 3                                     // FarmChallenge
  titleKR = "첫 수확의 기쁨"                         // -> see docs/systems/quest-system.md 섹션 6.1
  giverId = "system"
  timeLimitDays = 0                                // 무기한
  isRepeatable = false

  // objectives[0]: type = 0 (Harvest), requiredAmount = 1
  // rewards: (-> see quest-system.md 섹션 6.1)
  // unlockConditions: 없음 (항상 Active)
```

- **MCP 호출**: **~4회**

#### T-2-19 ~ T-2-21 (나머지 3종)

| 태스크 | 에셋명 | questId | Canonical 참조 |
|--------|--------|---------|---------------|
| T-2-19 | SO_Quest_FCEarn1000 | `fc_earn_1000` | quest-system.md 섹션 6.2 |
| T-2-20 | SO_Quest_FCFirstBuilding | `fc_first_building` | quest-system.md 섹션 6.3 |
| T-2-21 | SO_Quest_FCFirstProcess | `fc_first_process` | quest-system.md 섹션 6.3 |

- **MCP 호출**: 각 ~4회 x 3종 = **~12회**

---

**T-2 합계**: 폴더 5회 + 메인 퀘스트 4종 x 7회 + 일일 목표 12종 x 4회 + 농장 도전 4종 x 4회 = 5 + 28 + 48 + 16 = **~72회** (배열 설정 난이도에 따라 변동)

[RISK] QuestData의 `objectives[]`, `rewards[]`, `unlockConditions[]` 배열은 중첩 Serializable 구조이다. MCP `set_property`가 중첩 배열의 개별 요소를 지원하지 않을 수 있다. 이 경우 Editor 스크립트(`CreateQuestAssets.cs`)로 일괄 생성하여 T-2-02~T-2-21의 ~67회를 ~5회로 감소시킬 수 있다. (-> see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)

---

## 4. T-3: UI 프리팹/씬 오브젝트 생성

**목적**: 퀘스트 시스템 UI 오브젝트(QuestLogPanel, QuestTrackingWidget, QuestCompletePopup)를 생성한다.

**전제**: T-1 Phase 4 (QuestLogUI.cs, QuestTrackingUI.cs) 컴파일 완료. Canvas_HUD, Canvas_Overlay, Canvas_Popup 존재 (ARC-002).

---

### T-3 Phase 1: QuestLogPanel (G-03)

#### T-3-01: QuestLogPanel 루트 생성

```
create_object
  name: "QuestLogPanel"
  parent: "Canvas_Overlay"

set_property  target: "QuestLogPanel/RectTransform"
  anchorMin = (0.15, 0.1)
  anchorMax = (0.85, 0.9)                          // 화면 중앙 70%
```

- **MCP 호출**: 2회

#### T-3-02: QuestLogPanel 배경 이미지

```
create_object
  name: "BG_QuestLog"
  parent: "QuestLogPanel"

add_component
  target: "BG_QuestLog"
  component: "UnityEngine.UI.Image"

set_property  target: "BG_QuestLog/Image"
  color = (0.1, 0.12, 0.15, 0.95)                 // 어두운 반투명 배경
  raycastTarget = true

set_property  target: "BG_QuestLog/RectTransform"
  anchorMin = (0, 0)
  anchorMax = (1, 1)
```

- **MCP 호출**: 4회

#### T-3-03: 탭 버튼 영역

```
create_object
  name: "TabContainer"
  parent: "QuestLogPanel"

add_component
  target: "TabContainer"
  component: "UnityEngine.UI.HorizontalLayoutGroup"

set_property  target: "TabContainer/HorizontalLayoutGroup"
  spacing = 4
  childForceExpandWidth = true
  childForceExpandHeight = true

set_property  target: "TabContainer/RectTransform"
  anchorMin = (0.02, 0.9)
  anchorMax = (0.98, 0.98)
```

- **MCP 호출**: 4회

#### T-3-04: 탭 버튼 4개 (메인/NPC/일일/도전)

각 탭 버튼을 생성한다. 4개 모두 동일 구조.

```
// TabMainQuest
create_object  name: "TabMainQuest"  parent: "TabContainer"
add_component  target: "TabMainQuest"  component: "UnityEngine.UI.Button"
add_component  target: "TabMainQuest"  component: "TMPro.TextMeshProUGUI"
set_property  target: "TabMainQuest/TextMeshProUGUI"
  text = "메인 퀘스트"
  fontSize = 14

// TabNPCRequest, TabDailyChallenge, TabFarmChallenge 동일 패턴
```

- **MCP 호출**: 4개 x 4회 = **16회** (실제 버튼+텍스트 구성에 따라 변동)

[RISK] Unity UI 버튼의 자식 Text 오브젝트 구성이 MCP `create_object`만으로 올바르게 동작하는지 사전 검증 필요. Button + TextMeshProUGUI가 동일 GO에 부착되는 구조를 사용한다.

#### T-3-05: 퀘스트 목록 영역 (좌측)

```
create_object
  name: "QuestListContainer"
  parent: "QuestLogPanel"

add_component
  target: "QuestListContainer"
  component: "UnityEngine.UI.VerticalLayoutGroup"

set_property  target: "QuestListContainer/VerticalLayoutGroup"
  spacing = 4
  childForceExpandWidth = true
  childForceExpandHeight = false

set_property  target: "QuestListContainer/RectTransform"
  anchorMin = (0.02, 0.05)
  anchorMax = (0.4, 0.88)
```

- **MCP 호출**: 4회

#### T-3-06: 퀘스트 상세 영역 (우측)

```
create_object
  name: "QuestDetailPanel"
  parent: "QuestLogPanel"

set_property  target: "QuestDetailPanel/RectTransform"
  anchorMin = (0.42, 0.05)
  anchorMax = (0.98, 0.88)

// 제목 텍스트
create_object  name: "QuestDetailTitle"  parent: "QuestDetailPanel"
add_component  target: "QuestDetailTitle"  component: "TMPro.TextMeshProUGUI"
set_property  target: "QuestDetailTitle/TextMeshProUGUI"
  fontSize = 18
  fontStyle = "Bold"

// 설명 텍스트
create_object  name: "QuestDetailDesc"  parent: "QuestDetailPanel"
add_component  target: "QuestDetailDesc"  component: "TMPro.TextMeshProUGUI"
set_property  target: "QuestDetailDesc/TextMeshProUGUI"
  fontSize = 14

// 목표 리스트
create_object  name: "ObjectiveListContainer"  parent: "QuestDetailPanel"
add_component  target: "ObjectiveListContainer"  component: "UnityEngine.UI.VerticalLayoutGroup"
```

- **MCP 호출**: ~10회

#### T-3-07: QuestLogUI 컴포넌트 부착 및 참조 연결

```
add_component
  target: "QuestLogPanel"
  component: "SeedMind.UI.QuestLogUI"

set_property  target: "QuestLogPanel/QuestLogUI"
  _logPanel = ref:QuestLogPanel
  _tabMainQuest = ref:TabMainQuest/Button
  _tabNPCRequest = ref:TabNPCRequest/Button
  _tabDailyChallenge = ref:TabDailyChallenge/Button
  _tabFarmChallenge = ref:TabFarmChallenge/Button
  _questListContainer = ref:QuestListContainer/Transform
  _questDetailTitle = ref:QuestDetailTitle/TextMeshProUGUI
  _questDetailDesc = ref:QuestDetailDesc/TextMeshProUGUI
  _objectiveListContainer = ref:ObjectiveListContainer/Transform

// 초기 상태: 비활성
set_property  target: "QuestLogPanel"
  activeSelf = false
```

- **MCP 호출**: 3회

---

### T-3 Phase 2: QuestTrackingWidget (G-02)

#### T-3-08: QuestTrackingWidget 루트 생성

```
create_object
  name: "QuestTrackingWidget"
  parent: "Canvas_HUD"

set_property  target: "QuestTrackingWidget/RectTransform"
  anchorMin = (0.7, 0.6)
  anchorMax = (0.98, 0.95)                         // 화면 우측 상단
```

- **MCP 호출**: 2회

#### T-3-09: 추적 퀘스트 제목/목표/진행바

```
// 제목
create_object  name: "TrackQuestTitle"  parent: "QuestTrackingWidget"
add_component  target: "TrackQuestTitle"  component: "TMPro.TextMeshProUGUI"
set_property  target: "TrackQuestTitle/TextMeshProUGUI"
  fontSize = 14
  fontStyle = "Bold"
  color = (1, 0.95, 0.7, 1)

// 목표 텍스트
create_object  name: "TrackObjectiveText"  parent: "QuestTrackingWidget"
add_component  target: "TrackObjectiveText"  component: "TMPro.TextMeshProUGUI"
set_property  target: "TrackObjectiveText/TextMeshProUGUI"
  fontSize = 12

// 진행바
create_object  name: "TrackProgressBar"  parent: "QuestTrackingWidget"
add_component  target: "TrackProgressBar"  component: "UnityEngine.UI.Slider"
```

- **MCP 호출**: ~9회

#### T-3-10: QuestTrackingUI 컴포넌트 부착

```
add_component
  target: "QuestTrackingWidget"
  component: "SeedMind.UI.QuestTrackingUI"

set_property  target: "QuestTrackingWidget/QuestTrackingUI"
  _trackingWidget = ref:QuestTrackingWidget
  _questTitleText = ref:TrackQuestTitle/TextMeshProUGUI
  _objectiveText = ref:TrackObjectiveText/TextMeshProUGUI
  _progressBar = ref:TrackProgressBar/Slider
```

- **MCP 호출**: 2회

---

### T-3 Phase 3: QuestCompletePopup (G-04)

#### T-3-11: QuestCompletePopup 루트 생성

```
create_object
  name: "QuestCompletePopup"
  parent: "Canvas_Popup"

add_component
  target: "QuestCompletePopup"
  component: "UnityEngine.CanvasGroup"

set_property  target: "QuestCompletePopup/RectTransform"
  anchorMin = (0.2, 0.35)
  anchorMax = (0.8, 0.65)                          // 화면 중앙

set_property  target: "QuestCompletePopup/CanvasGroup"
  alpha = 0                                        // 초기 투명 (애니메이션으로 표시)
  blocksRaycasts = false
```

- **MCP 호출**: 4회

#### T-3-12: 완료 배너 내부 요소

```
// 배경
create_object  name: "BG_QuestComplete"  parent: "QuestCompletePopup"
add_component  target: "BG_QuestComplete"  component: "UnityEngine.UI.Image"
set_property  target: "BG_QuestComplete/Image"
  color = (0.15, 0.3, 0.15, 0.95)                 // 녹색 톤 배경

// "퀘스트 완료!" 텍스트
create_object  name: "CompleteLabel"  parent: "QuestCompletePopup"
add_component  target: "CompleteLabel"  component: "TMPro.TextMeshProUGUI"
set_property  target: "CompleteLabel/TextMeshProUGUI"
  text = "퀘스트 완료!"
  fontSize = 24
  fontStyle = "Bold"
  alignment = "Center"
  color = (1, 0.9, 0.3, 1)                        // 골드색

// 퀘스트 제목 텍스트
create_object  name: "CompleteQuestName"  parent: "QuestCompletePopup"
add_component  target: "CompleteQuestName"  component: "TMPro.TextMeshProUGUI"
set_property  target: "CompleteQuestName/TextMeshProUGUI"
  fontSize = 16
  alignment = "Center"

// 보상 텍스트
create_object  name: "RewardText"  parent: "QuestCompletePopup"
add_component  target: "RewardText"  component: "TMPro.TextMeshProUGUI"
set_property  target: "RewardText/TextMeshProUGUI"
  fontSize = 14
  alignment = "Center"

// 초기 상태: 비활성
set_property  target: "QuestCompletePopup"
  activeSelf = false
```

- **MCP 호출**: ~14회

---

**T-3 합계**: Phase 1(~33회) + Phase 2(~13회) + Phase 3(~18회) = **~38회** (UI 배치 미세조정 추가 시 변동)

[RISK] Unity UI 요소의 RectTransform 앵커/오프셋 값은 실제 화면 비율에 맞춰 미세조정이 필요하다. 초기 배치 후 Play Mode에서 시각적 검증이 필수이다.

---

## 5. T-4: 씬 배치 및 참조 연결

**목적**: SCN_Farm 씬에 QuestManager를 배치하고, SO 배열 참조를 연결한다.

**전제**: T-1~T-3 완료. SCN_Farm 씬에 `--- MANAGERS ---`, Canvas 계층 존재.

---

### T-4-01: QuestManager 매니저 배치 (G-01)

```
create_object
  name: "QuestManager"

set_parent
  target: "QuestManager"
  parent: "--- MANAGERS ---"

add_component
  target: "QuestManager"
  component: "SeedMind.Quest.QuestManager"
```

- **MCP 호출**: 3회

### T-4-02: QuestManager SO 배열 참조 연결

```
set_property  target: "QuestManager/QuestManager"
  _allQuests = [
    ref:"Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring01.asset",
    ref:"Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring02.asset",
    ref:"Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring03.asset",
    ref:"Assets/_Project/Data/Quests/Main/SO_Quest_MainSpring04.asset",
    ref:"Assets/_Project/Data/Quests/Challenge/SO_Quest_FCFirstHarvest.asset",
    ref:"Assets/_Project/Data/Quests/Challenge/SO_Quest_FCEarn1000.asset",
    ref:"Assets/_Project/Data/Quests/Challenge/SO_Quest_FCFirstBuilding.asset",
    ref:"Assets/_Project/Data/Quests/Challenge/SO_Quest_FCFirstProcess.asset"
  ]
```

- **MCP 호출**: 1회

[RISK] MCP `set_property`로 SO 배열에 여러 에셋 참조를 한번에 설정하는 것이 가능한지 사전 검증 필요. 미지원 시 개별 인덱스별 설정(`_allQuests[0]`, `_allQuests[1]`, ...)으로 분할해야 하며 호출 수가 크게 증가한다.

### T-4-03: QuestManager 일일 목표 풀 참조 연결

```
set_property  target: "QuestManager/QuestManager"
  _dailyQuestPool = [
    ref:"Assets/_Project/Data/Quests/Daily/SO_Quest_DailyWater.asset",
    ref:"Assets/_Project/Data/Quests/Daily/SO_Quest_DailyHarvest5.asset",
    ref:"Assets/_Project/Data/Quests/Daily/SO_Quest_DailyHarvest10.asset",
    ref:"Assets/_Project/Data/Quests/Daily/SO_Quest_DailySell.asset",
    ref:"Assets/_Project/Data/Quests/Daily/SO_Quest_DailyEarn.asset",
    ref:"Assets/_Project/Data/Quests/Daily/SO_Quest_DailyTill.asset",
    ref:"Assets/_Project/Data/Quests/Daily/SO_Quest_DailyQuality.asset",
    ref:"Assets/_Project/Data/Quests/Daily/SO_Quest_DailyProcess.asset",
    ref:"Assets/_Project/Data/Quests/Daily/SO_Quest_DailyFertilize.asset",
    ref:"Assets/_Project/Data/Quests/Daily/SO_Quest_DailyDiverse.asset",
    ref:"Assets/_Project/Data/Quests/Daily/SO_Quest_DailyGoldQuality.asset",
    ref:"Assets/_Project/Data/Quests/Daily/SO_Quest_DailyEarnLarge.asset"
  ]
```

- **MCP 호출**: 1회

### T-4-04: QuestLogPanel 초기 상태 설정

```
set_property  target: "QuestLogPanel"
  activeSelf = false
```

- **MCP 호출**: 1회

### T-4-05: QuestCompletePopup 초기 상태 설정

```
set_property  target: "QuestCompletePopup"
  activeSelf = false
```

- **MCP 호출**: 1회

### T-4-06: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

---

**T-4 합계**: 3 + 1 + 1 + 1 + 1 + 1 = **~8회** (배열 참조 설정 방식에 따라 변동, 최대 ~18회)

---

## 6. T-5: 기존 시스템 연동 설정

**목적**: Assembly Definition 생성 및 기존 시스템과의 참조를 연결한다.

**전제**: T-4 완료. 기존 시스템 모듈 모두 존재.

---

### T-5-01: Assembly Definition 생성

```
create_script
  path: "Assets/_Project/Scripts/Quest/SeedMind.Quest.asmdef"
  content: |
    {
        "name": "SeedMind.Quest",
        "rootNamespace": "SeedMind.Quest",
        "references": [
            "SeedMind.Core",
            "SeedMind.Farm",
            "SeedMind.Economy",
            "SeedMind.Building",
            "SeedMind.Level",
            "SeedMind.NPC"
        ],
        "includePlatforms": [],
        "excludePlatforms": [],
        "autoReferenced": true
    }
```

- **MCP 호출**: 1회

### T-5-02: GameSaveData 확장

기존 GameSaveData 루트 클래스에 `QuestSaveData quest` 필드를 추가해야 한다.

```
// GameSaveData.cs 수정 (기존 스크립트에 필드 추가)
// -> see docs/systems/quest-architecture.md 섹션 8.4
// -> see docs/systems/save-load-architecture.md 섹션 2.1

// 기존 GameSaveData에 추가:
// public QuestSaveData quest;
```

[RISK] MCP는 기존 스크립트의 부분 편집을 지원하지 않는다. `create_script`로 GameSaveData.cs 전체를 재생성해야 한다. 기존 필드를 유지하면서 `QuestSaveData quest` 필드를 추가하는 방식으로 처리한다.

- **MCP 호출**: 1회

### T-5-03: 입력 바인딩 추가

SeedMindInputActions에 "QuestLog" 액션을 추가한다 (J키).

```
// InputAction 에셋에 QuestLog 액션 추가
// -> see docs/systems/quest-system.md 섹션 8.1
```

[RISK] InputAction 에셋의 MCP 편집 가능 여부 미확인. 미지원 시 수동 설정 필요.

- **MCP 호출**: 1회

### T-5-04: 컴파일 대기

```
execute_menu_item
  // Unity 컴파일 대기
```

- **MCP 호출**: 1회

### T-5-05: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

---

**T-5 합계**: 1 + 1 + 1 + 1 + 1 = **5회**

---

## 7. T-6: 통합 테스트 시퀀스

**목적**: 퀘스트 생명주기(해금 -> 활성화 -> 진행 -> 완료 -> 보상), 일일 목표 교체, 세이브/로드 전체 흐름을 Play Mode에서 검증한다.

**전제**: T-1~T-5 모든 태스크 완료. 컴파일 에러 없음.

---

### T-6-01: 테스트 씬 생성 (G-05)

```
// SCN_Test_Quest.unity 생성
// QuestManager, TimeManager, SaveManager 최소 구성
create_object  name: "TestQuestManager"
add_component  target: "TestQuestManager"  component: "SeedMind.Quest.QuestManager"
// _allQuests, _dailyQuestPool 배열 참조 설정 (T-4-02, T-4-03과 동일)

save_scene
  path: "Assets/_Project/Scenes/Test/SCN_Test_Quest.unity"
```

- **MCP 호출**: ~4회

### T-6-02: 테스트 A -- 메인 퀘스트 해금/활성화

```
enter_play_mode

execute_method
  class: "SeedMind.Quest.QuestManager"
  method: "Initialize"

get_console_logs
  // 기대: "Quest main_spring_01 unlocked (TutorialComplete)"
  // 기대: "Quest main_spring_01 activated (MainQuest auto-accept)"
  // 기대: "Quest fc_first_harvest activated (FarmChallenge auto-accept)"

exit_play_mode
```

- **MCP 호출**: 4회

### T-6-03: 테스트 B -- 목표 진행도 갱신

```
enter_play_mode

execute_method
  class: "SeedMind.Quest.QuestManager"
  method: "Initialize"

// 작물 수확 시뮬레이션 (FarmEvents.OnCropHarvested 발행)
execute_method
  class: "SeedMind.Quest.QuestTracker"
  method: "UpdateObjective"
  args: [0, "", 5, 0]                             // Harvest, any, 5개, quality 무관

get_console_logs
  // 기대: "Quest main_spring_01 objective 0 progress: 5/10"
  // 기대: "Quest fc_first_harvest objective 0 progress: 1/1"
  // 기대: "Quest fc_first_harvest completed!"

exit_play_mode
```

- **MCP 호출**: 5회

### T-6-04: 테스트 C -- 일일 목표 선택/만료

```
enter_play_mode

execute_method
  class: "SeedMind.Quest.QuestManager"
  method: "Initialize"

// 날짜 변경 시뮬레이션 (TimeManager.OnDayChanged 발행)
// 기대: 일일 목표 2개 자동 생성

get_console_logs
  // 기대: "DailyQuestSelector: selected daily_xxx and daily_yyy"
  // 기대: "Quest daily_xxx activated"
  // 기대: "Quest daily_yyy activated"

// 다음 날로 진행
// 기대: 이전 일일 목표 Expired, 새 목표 생성

get_console_logs
  // 기대: "Quest daily_xxx expired"
  // 기대: "DailyQuestSelector: selected daily_zzz and daily_www"

exit_play_mode
```

- **MCP 호출**: 5회

### T-6-05: 테스트 D -- 보상 수령

```
enter_play_mode

execute_method
  class: "SeedMind.Quest.QuestManager"
  method: "Initialize"

// 퀘스트 완료 상태로 강제 설정 후 보상 수령
execute_method
  class: "SeedMind.Quest.QuestManager"
  method: "ClaimReward"
  args: ["fc_first_harvest"]

get_console_logs
  // 기대: "QuestRewarder: granted Gold (-> see quest-system.md 섹션 6.1)"
  // 기대: "QuestRewarder: granted XP (-> see quest-system.md 섹션 6.1)"
  // 기대: "Quest fc_first_harvest rewarded"

exit_play_mode
```

- **MCP 호출**: 5회

### T-6-06: 테스트 E -- 세이브/로드

```
enter_play_mode

execute_method
  class: "SeedMind.Quest.QuestManager"
  method: "Initialize"

// 퀘스트 진행 중 저장
execute_method
  class: "SeedMind.Quest.QuestManager"
  method: "GetSaveData"

get_console_logs
  // 기대: "QuestManager: save data generated (X quests, Y completed)"

// 로드
execute_method
  class: "SeedMind.Quest.QuestManager"
  method: "LoadSaveData"

get_console_logs
  // 기대: "QuestManager: loaded X quest states, Y completed IDs"
  // 기대: 진행도 유지 확인

exit_play_mode
```

- **MCP 호출**: 6회

### T-6-07: 최종 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

---

**T-6 합계**: 4 + 4 + 5 + 5 + 5 + 6 + 1 = **~24회** (추가 엣지 케이스 테스트 시 확장)

[RISK] `execute_method`로 일반 C# 클래스(QuestTracker)의 메서드를 직접 호출하는 것이 MCP에서 지원되는지 사전 검증 필요. MonoBehaviour가 아닌 클래스의 인스턴스 메서드는 QuestManager를 경유하여 호출해야 할 수 있다.

---

## Cross-references

| 문서 | 참조 내용 |
|------|-----------|
| `docs/systems/quest-architecture.md` (ARC-013) | 본 태스크의 기반 아키텍처 (클래스 설계, 데이터 구조, 이벤트, 씬 계층, SaveLoadOrder) |
| `docs/systems/quest-system.md` (DES-009) | 퀘스트 게임 디자인 canonical (카테고리, 목표 타입, 보상 수치, 일일 목표 풀, 농장 도전 목록) |
| `docs/systems/save-load-architecture.md` (ARC-011) | ISaveable 인터페이스, SaveLoadOrder 할당표, GameSaveData 루트 구조 |
| `docs/systems/npc-shop-architecture.md` (ARC-008) | NPCManager, NPCEvents (의뢰 연동 대상) |
| `docs/mcp/npc-shop-tasks.md` (ARC-009) | NPC MCP 태스크 (선행 의존, 형식 참조) |
| `docs/systems/inventory-architecture.md` | InventoryManager API (아이템 보상 지급) |
| `docs/systems/progression-architecture.md` | ProgressionManager API (XP 보상 지급), UnlockRegistry |
| `docs/systems/economy-architecture.md` | EconomyManager API (골드 보상 지급), EconomyEvents |
| `docs/systems/farming-architecture.md` | FarmEvents (OnCropHarvested, OnTileTilled 등) |
| `docs/systems/processing-architecture.md` | ProcessingEvents (OnProcessingCompleted) |
| `docs/systems/tutorial-architecture.md` | TutorialEvents (OnTutorialCompleted, 퀘스트 시스템 활성화 트리거) |
| `docs/systems/tool-upgrade-architecture.md` | ToolUpgradeEvents (OnUpgradeCompleted) |
| `docs/systems/project-structure.md` | 폴더 구조, 네임스페이스, 의존성 매트릭스, 씬 계층 |
| `docs/pipeline/data-pipeline.md` | SO 에셋 데이터 파이프라인 |
| `docs/mcp/scene-setup-tasks.md` (ARC-002) | 기본 씬 계층, Canvas 계층 (선행 의존) |
| `docs/mcp/farming-tasks.md` (ARC-003) | FarmEvents (선행 의존) |
| `docs/mcp/facilities-tasks.md` (ARC-007) | BuildingManager, BuildingEvents (선행 의존) |
| `docs/mcp/tool-upgrade-tasks.md` (ARC-015) | ToolUpgradeEvents (선행 의존) |

---

## Open Questions

1. [OPEN] **NPCEvents.OnItemDelivered 이벤트 미정의**: NPC 아키텍처(`docs/systems/npc-shop-architecture.md`)에 `OnItemDelivered` 이벤트가 아직 정의되지 않았다. 납품(Deliver) 목표 타입을 추적하려면 NPC 시스템에 해당 이벤트를 추가해야 하며, 추가 전까지 T-1-15(QuestTracker)의 Deliver 이벤트 구독은 비활성 상태로 둔다. (-> see `quest-architecture.md` Open Questions)

2. [OPEN] **GameSaveData 확장 시점**: T-5-02에서 GameSaveData에 `QuestSaveData quest` 필드를 추가해야 한다. MCP로 기존 스크립트를 부분 편집할 수 없으므로 전체 재생성이 필요하며, 기존 필드 유실 위험이 있다. save-load-architecture.md의 GameSaveData와 동기화가 필수이다. (-> see `quest-architecture.md` Open Questions)

3. [OPEN] **project-structure.md 의존성 매트릭스 업데이트**: Quest 모듈을 의존성 매트릭스와 asmdef 목록에 추가해야 한다. 이는 본 태스크 시퀀스 실행 후 별도 문서 업데이트로 처리한다. (-> see `quest-architecture.md` Open Questions)

4. [OPEN] **Composite 목표 타입의 Inspector 편집성**: QuestObjectiveData 내부에 `subObjectives` 배열이 재귀 구조이다. Unity Inspector에서 편집하기 어려울 수 있으며, Custom Editor 또는 SO 분리가 필요할 수 있다. MCP 에셋 생성 시에도 재귀 구조 설정이 불가능할 수 있다. (-> see `quest-architecture.md` Open Questions)

5. [OPEN] **NPC 의뢰 SO 에셋 미포함**: 본 태스크에서는 초반 콘텐츠(봄 메인 + 일일 + 도전 초반)만 포함했다. NPC 의뢰(npc_hana_01~03, npc_cheolsu_01~03, npc_moki_01~03, npc_barami_01~02) SO 에셋은 후속 태스크에서 확장한다.

---

## Risks

1. [RISK] **중첩 배열 SO 설정 난이도**: QuestData의 `objectives[]`, `rewards[]`, `unlockConditions[]`는 모두 Serializable 클래스 배열이다. MCP `set_property`로 중첩 구조의 개별 필드를 설정하는 것이 불가능할 수 있다. **대안**: Editor 스크립트(`CreateQuestAssets.cs`)를 T-2 전체의 대체 경로로 준비해야 한다. 이 경우 T-2의 ~67회를 ~5회로 감소시킬 수 있다.

2. [RISK] **이벤트 구독 누락**: QuestTracker가 12종 ObjectiveType을 추적하기 위해 9개 이상의 외부 이벤트를 구독해야 한다. 하나라도 누락되면 해당 목표 타입이 동작하지 않는다. T-6 테스트에서 각 ObjectiveType별 개별 검증 시나리오를 추가해야 하며, 현재 T-6은 Harvest 위주로만 테스트한다. (-> see `quest-architecture.md` Risks)

3. [RISK] **이벤트 페이로드 불일치**: 기존 시스템의 이벤트 페이로드(CropHarvestInfo, SellInfo 등)에 QuestTracker가 필요로 하는 필드가 모두 포함되어 있는지 확인 필요. 누락 시 기존 이벤트 구조를 확장해야 하며, 이는 해당 시스템 아키텍처 문서의 업데이트와 MCP 스크립트 재생성을 수반한다. (-> see `quest-architecture.md` Risks)

4. [RISK] **NPCRequestSaveState Dictionary 직렬화**: `cooldowns` 필드가 `Dictionary<string, int>` 타입이다. Unity의 기본 JsonUtility는 Dictionary를 지원하지 않는다. Newtonsoft.Json 또는 StringIntPair[] 변환 접근이 필요하다. (-> see `quest-architecture.md` Risks, `save-load-architecture.md` RISK 5)

5. [RISK] **SO 배열 참조 일괄 설정**: T-4-02, T-4-03에서 QuestManager의 `_allQuests`(8개), `_dailyQuestPool`(12개) 배열에 SO 참조를 한번에 설정해야 한다. MCP `set_property`가 배열 전체 일괄 설정을 지원하지 않으면 인덱스별 개별 설정(최대 20회 추가)이 필요하다.

6. [RISK] **UI 버튼 구조**: T-3-04에서 탭 버튼의 Button + TextMeshProUGUI 조합이 MCP로 올바르게 생성되는지 사전 검증 필요. Unity UI 버튼은 일반적으로 자식 Text 오브젝트를 갖는 구조이나, MCP에서 이 계층을 자동 생성하지 않을 수 있다.

---

*이 문서는 Claude Code가 docs/systems/quest-architecture.md(ARC-013)의 MCP 구현 요약을 상세한 태스크 시퀀스로 확장했습니다.*
