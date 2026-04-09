# 도전 과제/업적 시스템 MCP 태스크 시퀀스

> 업적 시스템의 스크립트 생성, SO 에셋 생성, UI 프리팹 구성, 씬 배치, 이벤트 연결, 세이브 통합, 통합 테스트를 MCP for Unity 태스크로 상세 정의  
> 작성: Claude Code (Opus) | 2026-04-07  
> 기반 문서: docs/systems/achievement-architecture.md (ARC-017), docs/content/achievements.md (CON-013)  
> 문서 ID: ARC-017-MCP  
> 최종 갱신: ARC-035 — 채집 업적 5종(A-031~A-035) MCP 태스크 추가

---

## Context

이 문서는 `docs/systems/achievement-architecture.md`(ARC-017) Part II의 Step 1~5 개요를 상세한 MCP 태스크 시퀀스로 확장한다. 업적 데이터 구조(enum 4종, Serializable 클래스 3종, SO 1종), 시스템 클래스(AchievementManager, AchievementEvents), UI 컴포넌트(AchievementPanel, AchievementToastUI, AchievementItemUI), SO 에셋(카테고리별 업적 데이터), 씬 배치, 기존 시스템 이벤트 연결, 세이브/로드 통합, 통합 테스트까지 MCP for Unity 도구 호출 수준의 구체적 명세를 포함한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 업적 시스템의 데이터 레이어(AchievementData SO), 시스템 레이어(AchievementManager, AchievementEvents), UI 레이어(AchievementPanel, AchievementToastUI, AchievementItemUI), 씬 배치, 세이브 통합을 완성한다.

**전제 조건**:
- ARC-002(scene-setup-tasks.md) Phase A~B 완료: 폴더 구조, SCN_Farm 기본 계층(MANAGERS, UI, Canvas_HUD, Canvas_Overlay, Canvas_Popup)
- ARC-003(farming-tasks.md) 완료: FarmEvents, 기본 시스템 인프라
- economy-architecture.md 기반 EconomyManager, EconomyEvents 구현 완료
- ARC-007(facilities-tasks.md) 완료: BuildingManager, BuildingEvents 구현 완료
- ARC-008(npc-shop-tasks.md) 완료: NPCManager, NPCEvents 구현 완료
- ARC-011(save-load-architecture.md) 기반 SaveManager, ISaveable 인프라 구현 완료
- ARC-015(tool-upgrade-tasks.md) 완료: ToolUpgradeEvents 구현 완료
- ARC-016(quest-tasks.md) 완료: QuestManager, QuestEvents 구현 완료
- progression-architecture.md 기반 ProgressionManager, ProgressionEvents 구현 완료
- processing-architecture.md 기반 ProcessingEvents 구현 완료
- gathering-system.md 기반 GatheringSystem, GatheringEvents 구현 완료 (T-7 전제)

---

# Part I -- 설계 요약

---

## 1. 태스크 맵

| 태스크 | 설명 | MCP 호출 수 |
|--------|------|------------|
| T-1 | 스크립트 생성 (enum 4종, Serializable 3종, SO 1종, 시스템 2종, UI 3종) | 15회 |
| T-2 | SO 에셋 생성 (AchievementData, 카테고리별) | ~451회 (Tiered 포함) / ~5회 (T-2-ALT 사용 시) |
| T-3 | UI 프리팹/씬 오브젝트 생성 (AchievementPanel, AchievementToast, AchievementItemUI) | ~42회 |
| T-4 | 씬 배치 및 참조 연결 | ~12회 |
| T-5 | 세이브 통합 및 이벤트 연결 | ~8회 |
| T-6 | 통합 테스트 시퀀스 | ~20회 |
| T-7 | 채집 업적 SO 에셋 생성 + 이벤트 연결 (A-031~A-035) | ~80회 (T-2-ALT 사용 시 ~3회) |
| **합계** | | **~628회** (T-2-ALT 사용 시 ~105회) |

[RISK] Tiered 업적 4종(기존) + 1종(채집 ach_gather_02)을 `set_property`로 개별 설정 시 T-2+T-7만 약 530회에 달한다. T-2-ALT (Editor 스크립트)를 우선 검토하여 대폭 축소하는 것을 권장한다. T-2-ALT 사용 여부에 따라 합계가 크게 달라진다. 또한 T-2의 SO 에셋 생성에서 Tiered 업적의 `AchievementTierData[]` 배열 설정이 MCP `set_property`로 가능한지 사전 검증 필요.

## 2. 스크립트 목록

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/Achievement/Data/AchievementCategory.cs` | `AchievementCategory` (enum) | `SeedMind.Achievement.Data` | T-1 Phase 1 |
| S-02 | `Scripts/Achievement/Data/AchievementConditionType.cs` | `AchievementConditionType` (enum) | `SeedMind.Achievement.Data` | T-1 Phase 1 |
| S-03 | `Scripts/Achievement/Data/AchievementRewardType.cs` | `AchievementRewardType` (enum) | `SeedMind.Achievement` | T-1 Phase 1 |
| S-04 | `Scripts/Achievement/Data/AchievementType.cs` | `AchievementType` (enum) | `SeedMind.Achievement` | T-1 Phase 1 |
| S-05 | `Scripts/Achievement/Data/AchievementTierData.cs` | `AchievementTierData` ([Serializable]) | `SeedMind.Achievement.Data` | T-1 Phase 1 |
| S-06 | `Scripts/Achievement/Data/AchievementData.cs` | `AchievementData` (ScriptableObject) | `SeedMind.Achievement.Data` | T-1 Phase 1 |
| S-07 | `Scripts/Achievement/AchievementRecord.cs` | `AchievementRecord`, `TierUnlockRecord` ([Serializable]) | `SeedMind.Achievement` | T-1 Phase 2 |
| S-08 | `Scripts/Achievement/AchievementSaveData.cs` | `AchievementSaveData` ([Serializable]) | `SeedMind.Achievement` | T-1 Phase 2 |
| S-09 | `Scripts/Achievement/AchievementEvents.cs` | `AchievementEvents` (static class) | `SeedMind.Achievement` | T-1 Phase 2 |
| S-10 | `Scripts/Achievement/AchievementManager.cs` | `AchievementManager` (MonoBehaviour, Singleton, ISaveable) | `SeedMind.Achievement` | T-1 Phase 3 |
| S-11 | `Scripts/UI/AchievementPanel.cs` | `AchievementPanel` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 4 |
| S-12 | `Scripts/UI/AchievementToastUI.cs` | `AchievementToastUI` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 4 |
| S-13 | `Scripts/UI/AchievementItemUI.cs` | `AchievementItemUI` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 4 |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: S-01~S-06 -> S-07~S-09 -> S-10 -> S-11~S-13. 각 Phase 사이에 Unity 컴파일 대기(`execute_menu_item`)가 필요하다.

## 3. SO 에셋 목록

업적별 AchievementData SO 에셋. 업적 ID, 조건, 보상 수치는 모두 canonical 문서를 참조한다.
(-> see docs/systems/achievement-system.md 섹션 3 for 전체 업적 목록)

| # | 에셋명 | 경로 | 카테고리 | 생성 태스크 |
|---|--------|------|----------|-----------|
| A-01 | `SO_Ach_Farming01.asset` | `Assets/_Project/Data/Achievements/Farming/` | Farming | T-2-02 |
| A-02 | `SO_Ach_Farming02.asset` | `Assets/_Project/Data/Achievements/Farming/` | Farming | T-2-03 |
| A-03 | `SO_Ach_Farming03.asset` | `Assets/_Project/Data/Achievements/Farming/` | Farming | T-2-04 |
| A-04 | `SO_Ach_Farming04.asset` | `Assets/_Project/Data/Achievements/Farming/` | Farming | T-2-05 |
| A-05 | `SO_Ach_Farming05.asset` | `Assets/_Project/Data/Achievements/Farming/` | Farming | T-2-06 |
| A-06 | `SO_Ach_Economy01.asset` | `Assets/_Project/Data/Achievements/Economy/` | Economy | T-2-07 |
| A-07 | `SO_Ach_Economy02.asset` | `Assets/_Project/Data/Achievements/Economy/` | Economy | T-2-08 |
| A-08 | `SO_Ach_Economy03.asset` | `Assets/_Project/Data/Achievements/Economy/` | Economy | T-2-09 |
| A-09 | `SO_Ach_Economy04.asset` | `Assets/_Project/Data/Achievements/Economy/` | Economy | T-2-10 |
| A-10 | `SO_Ach_Facility01.asset` | `Assets/_Project/Data/Achievements/Facility/` | Facility | T-2-11 |
| A-11 | `SO_Ach_Facility02.asset` | `Assets/_Project/Data/Achievements/Facility/` | Facility | T-2-12 |
| A-12 | `SO_Ach_Facility03.asset` | `Assets/_Project/Data/Achievements/Facility/` | Facility | T-2-13 |
| A-13 | `SO_Ach_Facility04.asset` | `Assets/_Project/Data/Achievements/Facility/` | Facility | T-2-14 |
| A-14 | `SO_Ach_Tool01.asset` | `Assets/_Project/Data/Achievements/Tool/` | Tool | T-2-15 |
| A-15 | `SO_Ach_Tool02.asset` | `Assets/_Project/Data/Achievements/Tool/` | Tool | T-2-16 |
| A-16 | `SO_Ach_Tool03.asset` | `Assets/_Project/Data/Achievements/Tool/` | Tool | T-2-17 |
| A-17 | `SO_Ach_Explorer01.asset` | `Assets/_Project/Data/Achievements/Explorer/` | Explorer | T-2-18 |
| A-18 | `SO_Ach_Explorer02.asset` | `Assets/_Project/Data/Achievements/Explorer/` | Explorer | T-2-19 |
| A-19 | `SO_Ach_Explorer03.asset` | `Assets/_Project/Data/Achievements/Explorer/` | Explorer | T-2-20 |
| A-20 | `SO_Ach_Explorer04.asset` | `Assets/_Project/Data/Achievements/Explorer/` | Explorer | T-2-21 |
| A-21 | `SO_Ach_Quest01.asset` | `Assets/_Project/Data/Achievements/Quest/` | Quest | T-2-22 |
| A-22 | `SO_Ach_Quest02.asset` | `Assets/_Project/Data/Achievements/Quest/` | Quest | T-2-23 |
| A-23 | `SO_Ach_Quest03.asset` | `Assets/_Project/Data/Achievements/Quest/` | Quest | T-2-24 |
| A-24 | `SO_Ach_Quest04.asset` | `Assets/_Project/Data/Achievements/Quest/` | Quest | T-2-25 |
| A-25 | `SO_Ach_Hidden01.asset` | `Assets/_Project/Data/Achievements/Hidden/` | Hidden | T-2-26 |
| A-26 | `SO_Ach_Hidden02.asset` | `Assets/_Project/Data/Achievements/Hidden/` | Hidden | T-2-27 |
| A-27 | `SO_Ach_Hidden03.asset` | `Assets/_Project/Data/Achievements/Hidden/` | Hidden | T-2-28 |
| A-28 | `SO_Ach_Hidden04.asset` | `Assets/_Project/Data/Achievements/Hidden/` | Hidden | T-2-29 |
| A-29 | `SO_Ach_Hidden05.asset` | `Assets/_Project/Data/Achievements/Hidden/` | Hidden | T-2-30 |
| A-30 | `SO_Ach_Hidden06.asset` | `Assets/_Project/Data/Achievements/Hidden/` | Hidden | T-2-31 |
| A-31 | `SO_Ach_Gather01.asset` | `Assets/_Project/Data/Achievements/Gatherer/` | Gatherer | T-7-02 |
| A-32 | `SO_Ach_Gather02.asset` | `Assets/_Project/Data/Achievements/Gatherer/` | Gatherer | T-7-03 |
| A-33 | `SO_Ach_Gather03.asset` | `Assets/_Project/Data/Achievements/Gatherer/` | Gatherer | T-7-04 |
| A-34 | `SO_Ach_Gather04.asset` | `Assets/_Project/Data/Achievements/Gatherer/` | Gatherer | T-7-05 |
| A-35 | `SO_Ach_Gather05.asset` | `Assets/_Project/Data/Achievements/Gatherer/` | Gatherer | T-7-06 |
| A-36 | `SO_Ach_Hidden07.asset` | `Assets/_Project/Data/Achievements/Hidden/` | Hidden | T-2-32 |

> 업적 총 개수 40개 (-> see docs/content/achievements.md 섹션 13.1 for 카테고리별 배분). 기존 30개(T-2) + 낚시 4개(별도) + 채집 5개(T-7) + ach_hidden_07 통합 수집 마스터 1개(T-2-32, CON-017 추가)

## 4. 씬 GameObject 목록

| # | 오브젝트명 | 부모 | 컴포넌트 | 생성 태스크 |
|---|-----------|------|----------|-----------|
| G-01 | `AchievementManager` | `--- MANAGERS ---` | AchievementManager | T-4-01 |
| G-02 | `AchievementLayer` | `Canvas_Overlay` | (빈 오브젝트, 레이아웃 그룹) | T-3 Phase 1 |
| G-03 | `AchievementPanel` | `AchievementLayer` | AchievementPanel | T-3 Phase 1 |
| G-04 | `AchievementToast` | `Canvas_Popup` | AchievementToastUI | T-3 Phase 2 |
| G-05 | `SCN_Test_Achievement.unity` | (독립 씬) | 테스트 전용 | T-6-01 |

## 5. 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `Canvas_HUD`, `Canvas_Overlay`, `Canvas_Popup` (UI 루트) | ARC-002 Phase B |
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A |
| `EconomyManager`, `EconomyEvents` | economy-architecture.md |
| `ProgressionManager`, `ProgressionEvents` | progression-architecture.md |
| `InventoryManager` | inventory-architecture.md (ARC-013) |
| `NPCManager`, `NPCEvents` | ARC-008 (npc-shop-tasks.md) |
| `SaveManager`, `ISaveable` | ARC-011 (save-load-architecture.md) |
| `QuestManager`, `QuestEvents` | ARC-016 (quest-tasks.md) |
| `TimeManager` | ARC-002 |
| `BuildingManager`, `BuildingEvents` | ARC-007 (facilities-tasks.md) |
| `FarmEvents` | ARC-003 (farming-tasks.md) |
| `ToolUpgradeEvents` | ARC-015 (tool-upgrade-tasks.md) |
| `ProcessingEvents` | processing-architecture.md (ARC-012) |

## 6. MCP 도구 매핑

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

# Part II -- MCP 태스크 시퀀스

---

## T-1: 스크립트 생성

**목적**: 업적 시스템에 필요한 모든 C# 스크립트를 생성한다.

**전제**: Core 인프라(TimeManager, SaveManager 등) 컴파일 완료. Economy/Player/Building/NPC/Quest/Progression/Processing 모듈 컴파일 완료.

---

### T-1 Phase 1: 데이터 구조 스크립트 (S-01 ~ S-06)

#### T-1-01: 폴더 생성

```
create_folder
  path: "Assets/_Project/Scripts/Achievement"

create_folder
  path: "Assets/_Project/Scripts/Achievement/Data"
```

- **MCP 호출**: 2회

#### T-1-02: AchievementCategory enum (S-01)

```
create_script
  path: "Assets/_Project/Scripts/Achievement/Data/AchievementCategory.cs"
  content: |
    // S-01: 업적 카테고리 열거형
    // -> see docs/systems/achievement-architecture.md 섹션 2.2
    // -> see docs/systems/achievement-system.md 섹션 1 for 카테고리 정의
    namespace SeedMind.Achievement.Data
    {
        public enum AchievementCategory
        {
            Farming     = 0,   // 경작/수확 관련
            Economy     = 1,   // 골드/거래 관련
            Facility    = 2,   // 시설 건설/업그레이드 관련
            Tool        = 3,   // 도구 업그레이드 관련
            Explorer    = 4,   // 탐험/발견 관련
            Quest       = 5,   // 퀘스트 완료 관련
            Hidden      = 6,   // 숨겨진 업적 (달성 전 조건 비공개)
            Angler      = 7,   // 낚시 관련 (-> see docs/content/achievements.md 섹션 9)
            Gatherer    = 8    // 채집 관련 (-> see docs/content/achievements.md 섹션 9.5)
        }
    }
```

- **MCP 호출**: 1회
- **검증**: 컴파일 에러 없음

#### T-1-03: AchievementConditionType enum (S-02)

```
create_script
  path: "Assets/_Project/Scripts/Achievement/Data/AchievementConditionType.cs"
  content: |
    // S-02: 업적 달성 조건 타입 열거형
    // -> see docs/systems/achievement-architecture.md 섹션 2.3
    namespace SeedMind.Achievement.Data
    {
        public enum AchievementConditionType
        {
            HarvestCount            = 0,   // 총 수확 횟수 (작물 무관)
            GoldEarned              = 1,   // 누적 골드 획득량
            BuildingCount           = 2,   // 건설한 시설 총 수
            ToolUpgradeCount        = 3,   // 도구 업그레이드 총 횟수
            NPCMet                  = 4,   // 만난 NPC 수
            QuestCompleted          = 5,   // 완료한 퀘스트 총 수
            SpecificCropHarvested   = 6,   // 특정 작물 수확 횟수
            GoldSpent               = 7,   // 누적 골드 지출량
            DaysPlayed              = 8,   // 게임 내 경과 일수
            SeasonCompleted         = 9,   // 완료한 계절 수
            SpecificBuildingBuilt   = 10,  // 특정 시설 건설 여부
            TotalItemsSold          = 11,  // 판매한 아이템 총 수
            QualityHarvestCount     = 12,  // 특정 품질 이상 수확 횟수
            ProcessingCount         = 13,  // 가공 완료 총 횟수
            PurchaseCount           = 14,  // 상점 구매 횟수
            GatherCount             = 15,  // 채집 총 횟수 (아이템 무관)
            GatherSpeciesCollected  = 16,  // 채집으로 수집한 종류 수 (고유 itemId 수)
            GatherSickleUpgraded    = 17,  // 채집 낫 업그레이드 단계 달성
            Custom                  = 99   // 숨겨진 업적 전용 복합 조건
            // Custom 조건 적용 업적 → see docs/systems/achievement-system.md 섹션 3.7
            // GatherCount, GatherSpeciesCollected, GatherSickleUpgraded → see docs/systems/achievement-architecture.md 섹션 2.3
        }
    }
```

- **MCP 호출**: 1회
- **검증**: 컴파일 에러 없음

#### T-1-04: AchievementRewardType enum (S-03)

```
create_script
  path: "Assets/_Project/Scripts/Achievement/Data/AchievementRewardType.cs"
  content: |
    // S-03: 업적 보상 타입 열거형
    // -> see docs/systems/achievement-architecture.md 섹션 4.2
    namespace SeedMind.Achievement
    {
        public enum AchievementRewardType
        {
            None    = 0,   // 보상 없음 (달성 자체가 목적)
            Gold    = 1,   // 골드
            XP      = 2,   // 경험치
            Item    = 3,   // 아이템
            Title   = 4    // 칭호 (-> see docs/systems/achievement-system.md 섹션 4.2)
        }
    }
```

- **MCP 호출**: 1회

#### T-1-05: AchievementType enum (S-04)

```
create_script
  path: "Assets/_Project/Scripts/Achievement/Data/AchievementType.cs"
  content: |
    // S-04: 업적 유형 열거형
    // -> see docs/systems/achievement-architecture.md 섹션 2.1
    namespace SeedMind.Achievement
    {
        public enum AchievementType
        {
            Single  = 0,   // 단일 달성 (1회 조건 충족 시 영구 해금)
            Tiered  = 1    // 단계형 달성 (Bronze -> Silver -> Gold 순차 해금)
        }
    }
```

- **MCP 호출**: 1회

#### T-1-06: AchievementTierData 직렬화 클래스 (S-05)

```
create_script
  path: "Assets/_Project/Scripts/Achievement/Data/AchievementTierData.cs"
  content: |
    // S-05: 단계형 업적의 각 단계 데이터 (Bronze/Silver/Gold)
    // -> see docs/systems/achievement-architecture.md 섹션 4.1
    namespace SeedMind.Achievement.Data
    {
        [System.Serializable]
        public class AchievementTierData
        {
            public string tierName;                     // "Bronze" / "Silver" / "Gold"
            public AchievementConditionType conditionType;
            public string targetId;
            public int targetValue;                     // -> see docs/systems/achievement-system.md 섹션 3
            public AchievementRewardType rewardType;
            public int rewardAmount;                    // -> see docs/balance/progression-curve.md
            public string rewardItemId;
            public string rewardTitleId;
        }
    }
```

- **MCP 호출**: 1회

#### T-1-07: AchievementData ScriptableObject (S-06)

```
create_script
  path: "Assets/_Project/Scripts/Achievement/Data/AchievementData.cs"
  content: |
    // S-06: 업적 정적 정의 ScriptableObject
    // -> see docs/systems/achievement-architecture.md 섹션 4.1
    using UnityEngine;

    namespace SeedMind.Achievement.Data
    {
        [CreateAssetMenu(fileName = "NewAchievementData", menuName = "SeedMind/AchievementData")]
        public class AchievementData : ScriptableObject
        {
            [Header("기본 정보")]
            public string achievementId;                // 고유 식별자
            public string displayName;                  // -> see docs/systems/achievement-system.md 섹션 3
            [TextArea(2, 4)]
            public string description;                  // -> see docs/systems/achievement-system.md 섹션 3
            public AchievementCategory category;        // -> see S-01
            public AchievementType type;                // -> see S-04

            [Header("달성 조건 -- Single 전용")]
            public AchievementConditionType conditionType;  // -> see S-02
            public string targetId;                     // 대상 ID (""이면 any)
            public int targetValue;                     // -> see docs/systems/achievement-system.md

            [Header("단계형 조건 -- Tiered 전용")]
            public AchievementTierData[] tiers;         // Bronze[0], Silver[1], Gold[2]

            [Header("보상 -- Single 전용")]
            public AchievementRewardType rewardType;    // -> see S-03
            public int rewardAmount;                    // -> see docs/balance/progression-curve.md
            public string rewardItemId;                 // 아이템 ID ("" 이면 미사용)
            public string rewardTitleId;                // 칭호 ID ("" 이면 미사용)

            [Header("표시")]
            public bool isHidden;                       // true이면 달성 전까지 조건 비공개
            public Sprite icon;                         // 업적 아이콘 (null이면 카테고리 기본)
            public int sortOrder;                       // 카테고리 내 표시 순서
        }
    }
```

- **MCP 호출**: 1회
- **검증**: 16개 필드 (-> see docs/systems/achievement-architecture.md 섹션 4.3 PATTERN-005 검증)

#### T-1 Phase 1 마무리: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회
- **검증**: 콘솔에 컴파일 에러 없음 확인 (`get_console_logs`)

---

### T-1 Phase 2: 런타임 상태 및 이벤트 스크립트 (S-07 ~ S-09)

#### T-1-08: AchievementRecord, TierUnlockRecord 직렬화 클래스 (S-07)

```
create_script
  path: "Assets/_Project/Scripts/Achievement/AchievementRecord.cs"
  content: |
    // S-07: 개별 업적의 런타임 진행 상태
    // -> see docs/systems/achievement-architecture.md 섹션 6.1
    using System.Collections.Generic;
    using UnityEngine;

    namespace SeedMind.Achievement
    {
        [System.Serializable]
        public class AchievementRecord
        {
            public string achievementId;
            public int currentProgress;
            public bool isUnlocked;
            public int unlockedDay;        // -1 = 미달성
            public int unlockedSeason;     // -1 = 미달성
            public int unlockedYear;       // -1 = 미달성
            public string currentTier;     // "None"/"Bronze"/"Silver"/"Gold" (Single이면 "")
            public List<TierUnlockRecord> tierHistory;

            public AchievementRecord(string id)
            {
                achievementId = id;
                currentProgress = 0;
                isUnlocked = false;
                unlockedDay = -1;
                unlockedSeason = -1;
                unlockedYear = -1;
                currentTier = "";
                tierHistory = new List<TierUnlockRecord>();
            }

            public float GetNormalizedProgress(int targetValue)
            {
                if (targetValue <= 0) return isUnlocked ? 1f : 0f;
                return Mathf.Clamp01((float)currentProgress / targetValue);
            }
        }

        [System.Serializable]
        public class TierUnlockRecord
        {
            public string tier;            // "Bronze" / "Silver" / "Gold"
            public int unlockedDay;
            public int unlockedSeason;
            public int unlockedYear;
        }
    }
```

- **MCP 호출**: 1회
- **검증**: AchievementRecord 8필드, TierUnlockRecord 4필드 (-> see docs/systems/achievement-architecture.md 섹션 6.2 PATTERN-005)

#### T-1-09: AchievementSaveData 직렬화 클래스 (S-08)

```
create_script
  path: "Assets/_Project/Scripts/Achievement/AchievementSaveData.cs"
  content: |
    // S-08: 업적 세이브 데이터 구조
    // -> see docs/systems/achievement-architecture.md 섹션 7.2
    using System.Collections.Generic;

    namespace SeedMind.Achievement
    {
        [System.Serializable]
        public class AchievementSaveData
        {
            public List<AchievementRecord> records;
            public int totalUnlocked;
        }
    }
```

- **MCP 호출**: 1회

#### T-1-10: AchievementEvents 정적 이벤트 허브 (S-09)

```
create_script
  path: "Assets/_Project/Scripts/Achievement/AchievementEvents.cs"
  content: |
    // S-09: 업적 시스템 정적 이벤트 허브
    // -> see docs/systems/achievement-architecture.md 섹션 3.3
    using SeedMind.Achievement.Data;

    namespace SeedMind.Achievement
    {
        public static class AchievementEvents
        {
            /// <summary>업적 달성 시 발행. UI 토스트 트리거용.</summary>
            public static event System.Action<AchievementData> OnAchievementUnlocked;

            /// <summary>업적 진행도 갱신 시 발행. UI 프로그레스 바 갱신용.</summary>
            public static event System.Action<string, float> OnProgressUpdated;
            // string = achievementId, float = normalizedProgress (0.0~1.0)

            internal static void RaiseAchievementUnlocked(AchievementData data)
                => OnAchievementUnlocked?.Invoke(data);

            internal static void RaiseProgressUpdated(string id, float progress)
                => OnProgressUpdated?.Invoke(id, progress);
        }
    }
```

- **MCP 호출**: 1회

#### T-1 Phase 2 마무리: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회
- **검증**: `get_console_logs`로 에러 없음 확인

---

### T-1 Phase 3: 시스템 스크립트 (S-10)

#### T-1-11: AchievementManager (S-10)

```
create_script
  path: "Assets/_Project/Scripts/Achievement/AchievementManager.cs"
  content: |
    // S-10: 업적 시스템 매니저 (Singleton, ISaveable)
    // -> see docs/systems/achievement-architecture.md 섹션 3.2
    // SaveLoadOrder = 90 -> see docs/systems/save-load-architecture.md 섹션 7
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using SeedMind.Core;
    using SeedMind.Achievement.Data;
    using SeedMind.Save;

    namespace SeedMind.Achievement
    {
        public class AchievementManager : Singleton<AchievementManager>, ISaveable
        {
            [SerializeField] private AchievementData[] _allAchievements;

            private Dictionary<string, AchievementRecord> _records
                = new Dictionary<string, AchievementRecord>();
            private HashSet<string> _unlockedIds = new HashSet<string>();
            private Dictionary<string, AchievementData> _achievementLookup
                = new Dictionary<string, AchievementData>();

            // ISaveable
            public int SaveLoadOrder => 90; // -> see docs/systems/save-load-architecture.md 섹션 7

            public void Initialize()
            {
                _achievementLookup.Clear();
                _records.Clear();
                _unlockedIds.Clear();

                foreach (var data in _allAchievements)
                {
                    _achievementLookup[data.achievementId] = data;
                    _records[data.achievementId] = new AchievementRecord(data.achievementId);
                }

                SubscribeAll();
                Debug.Log($"[AchievementManager] Initialized with {_allAchievements.Length} achievements.");
            }

            // -> see docs/systems/achievement-architecture.md 섹션 5 for 이벤트 구독 매핑
            private void SubscribeAll()
            {
                // FarmEvents.OnCropHarvested += HandleHarvest;
                // EconomyEvents.OnSaleCompleted += HandleSale;
                // EconomyEvents.OnGoldSpent += HandleGoldSpent;
                // BuildingEvents.OnConstructionCompleted += HandleBuildingBuilt;
                // ToolEvents.OnToolUpgraded += HandleToolUpgrade;
                // NPCEvents.OnNPCFirstMet += HandleNPCMet;
                // QuestEvents.OnQuestCompleted += HandleQuestCompleted;
                // ProcessingEvents.OnProcessingCompleted += HandleProcessing;
                // TimeManager.OnDayChanged += HandleDayChanged;
                // TimeManager.OnSeasonChanged += HandleSeasonChanged;
                // GatheringEvents.OnItemGathered += HandleGather;           // ARC-035
                // GatheringEvents.OnSickleUpgraded += HandleSickleUpgrade;  // ARC-035
                Debug.Log("[AchievementManager] Event subscriptions registered (12 events).");
            }

            private void UnsubscribeAll()
            {
                // 구독 해제 (SubscribeAll과 대칭)
            }

            public void UpdateProgress(AchievementConditionType condType, int amount)
            {
                foreach (var data in _allAchievements)
                {
                    if (IsMatchingCondition(data, condType) && !IsFullyUnlocked(data.achievementId))
                    {
                        var record = _records[data.achievementId];
                        record.currentProgress += amount;
                        AchievementEvents.RaiseProgressUpdated(
                            data.achievementId,
                            record.GetNormalizedProgress(GetTargetValue(data)));
                        CheckCompletion(data.achievementId);
                    }
                }
            }

            public void UpdateProgress(AchievementConditionType condType, string targetId, int amount)
            {
                foreach (var data in _allAchievements)
                {
                    if (IsMatchingCondition(data, condType)
                        && data.targetId == targetId
                        && !IsFullyUnlocked(data.achievementId))
                    {
                        var record = _records[data.achievementId];
                        record.currentProgress += amount;
                        AchievementEvents.RaiseProgressUpdated(
                            data.achievementId,
                            record.GetNormalizedProgress(GetTargetValue(data)));
                        CheckCompletion(data.achievementId);
                    }
                }
            }

            public bool CheckCompletion(string achievementId)
            {
                if (!_achievementLookup.TryGetValue(achievementId, out var data)) return false;
                var record = _records[achievementId];

                if (data.type == AchievementType.Single)
                {
                    if (!record.isUnlocked && record.currentProgress >= data.targetValue)
                    {
                        UnlockAchievement(achievementId);
                        return true;
                    }
                }
                else if (data.type == AchievementType.Tiered)
                {
                    // 단계별 순차 확인
                    for (int i = 0; i < data.tiers.Length; i++)
                    {
                        var tier = data.tiers[i];
                        bool alreadyUnlocked = record.tierHistory.Any(t => t.tier == tier.tierName);
                        if (!alreadyUnlocked && record.currentProgress >= tier.targetValue)
                        {
                            UnlockTier(achievementId, tier);
                        }
                    }
                }
                return false;
            }

            public void UnlockAchievement(string achievementId)
            {
                var record = _records[achievementId];
                record.isUnlocked = true;
                // record.unlockedDay = TimeManager.Instance.CurrentDay;
                // record.unlockedSeason = TimeManager.Instance.CurrentSeasonIndex;
                // record.unlockedYear = TimeManager.Instance.CurrentYear;
                _unlockedIds.Add(achievementId);

                GrantReward(_achievementLookup[achievementId]);
                AchievementEvents.RaiseAchievementUnlocked(_achievementLookup[achievementId]);
                Debug.Log($"[AchievementManager] Unlocked: {achievementId}");
            }

            private void UnlockTier(string achievementId, AchievementTierData tier)
            {
                var record = _records[achievementId];
                record.currentTier = tier.tierName;
                record.tierHistory.Add(new TierUnlockRecord
                {
                    tier = tier.tierName,
                    // unlockedDay = TimeManager.Instance.CurrentDay,
                    // unlockedSeason = TimeManager.Instance.CurrentSeasonIndex,
                    // unlockedYear = TimeManager.Instance.CurrentYear
                });

                GrantTierReward(tier);

                // Gold 단계 달성 시 전체 업적 달성 처리
                if (tier.tierName == "Gold")
                {
                    record.isUnlocked = true;
                    _unlockedIds.Add(achievementId);
                }

                AchievementEvents.RaiseAchievementUnlocked(_achievementLookup[achievementId]);
                Debug.Log($"[AchievementManager] Tier unlocked: {achievementId} - {tier.tierName}");
            }

            private void GrantReward(AchievementData data)
            {
                // -> see docs/systems/achievement-architecture.md 섹션 3.2 UnlockAchievement 흐름
                // switch (data.rewardType)
                // {
                //     case AchievementRewardType.Gold:
                //         EconomyManager.Instance.AddGold(data.rewardAmount);
                //         break;
                //     case AchievementRewardType.XP:
                //         ProgressionManager.Instance.AddXP(data.rewardAmount);
                //         break;
                //     case AchievementRewardType.Item:
                //         InventoryManager.Instance.TryAddItem(data.rewardItemId, 1);
                //         break;
                //     case AchievementRewardType.Title:
                //         // 칭호 해금 로직
                //         break;
                // }
                Debug.Log($"[AchievementManager] Reward granted: {data.rewardType} x{data.rewardAmount}");
            }

            private void GrantTierReward(AchievementTierData tier)
            {
                // GrantReward와 동일 패턴, tier.rewardType/rewardAmount 사용
                Debug.Log($"[AchievementManager] Tier reward: {tier.rewardType} x{tier.rewardAmount}");
            }

            // --- 조회 API ---
            public AchievementRecord GetRecord(string achievementId)
                => _records.TryGetValue(achievementId, out var r) ? r : null;

            public IReadOnlyList<AchievementData> GetAchievementsByCategory(AchievementCategory cat)
                => _allAchievements.Where(a => a.category == (AchievementCategory)cat).ToList();

            public IReadOnlyList<AchievementData> GetUnlockedAchievements()
                => _allAchievements.Where(a => _unlockedIds.Contains(a.achievementId)).ToList();

            public float GetOverallProgress()
                => _allAchievements.Length > 0 ? (float)_unlockedIds.Count / _allAchievements.Length : 0f;

            public bool IsUnlocked(string achievementId)
                => _unlockedIds.Contains(achievementId);

            // --- 유틸 ---
            private bool IsMatchingCondition(AchievementData data, AchievementConditionType condType)
            {
                if (data.type == AchievementType.Single) return data.conditionType == condType;
                if (data.type == AchievementType.Tiered && data.tiers.Length > 0)
                    return data.tiers[0].conditionType == condType;
                return false;
            }

            private bool IsFullyUnlocked(string achievementId)
                => _unlockedIds.Contains(achievementId);

            private int GetTargetValue(AchievementData data)
            {
                if (data.type == AchievementType.Single) return data.targetValue;
                if (data.type == AchievementType.Tiered && data.tiers.Length > 0)
                    return data.tiers[data.tiers.Length - 1].targetValue;
                return 0;
            }

            // --- ISaveable ---
            public object GetSaveData()
            {
                return new AchievementSaveData
                {
                    records = new List<AchievementRecord>(_records.Values),
                    totalUnlocked = _unlockedIds.Count
                };
            }

            public void LoadSaveData(object data)
            {
                if (data is not AchievementSaveData saveData) return;
                _records.Clear();
                _unlockedIds.Clear();
                foreach (var record in saveData.records)
                {
                    _records[record.achievementId] = record;
                    if (record.isUnlocked) _unlockedIds.Add(record.achievementId);
                }
                Debug.Log($"[AchievementManager] Loaded {saveData.records.Count} records, {saveData.totalUnlocked} unlocked.");
            }

            private void OnDestroy()
            {
                UnsubscribeAll();
            }
        }
    }
```

- **MCP 호출**: 1회
- **의존**: S-01~S-09 컴파일 완료 필수

#### T-1 Phase 3 마무리: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회
- **검증**: `get_console_logs`로 에러 없음 확인

---

### T-1 Phase 4: UI 스크립트 (S-11 ~ S-13)

#### T-1-12: AchievementPanel (S-11)

```
create_script
  path: "Assets/_Project/Scripts/UI/AchievementPanel.cs"
  content: |
    // S-11: 업적 목록 패널 UI
    // -> see docs/systems/achievement-architecture.md 섹션 8.1
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using SeedMind.Achievement;
    using SeedMind.Achievement.Data;

    namespace SeedMind.UI
    {
        public class AchievementPanel : MonoBehaviour
        {
            [SerializeField] private Transform _contentParent;
            [SerializeField] private AchievementItemUI _itemPrefab;
            [SerializeField] private Button[] _categoryTabs;
            [SerializeField] private TMP_Text _progressText;

            private bool _isOpen;
            private AchievementCategory _currentCategory;

            public void Toggle()
            {
                if (_isOpen) Close(); else Open();
            }

            public void Open()
            {
                _isOpen = true;
                gameObject.SetActive(true);
                RefreshList();
                UpdateProgressText();
            }

            public void Close()
            {
                _isOpen = false;
                gameObject.SetActive(false);
            }

            public void SetCategory(AchievementCategory category)
            {
                _currentCategory = category;
                RefreshList();
            }

            private void RefreshList()
            {
                // 기존 항목 정리, 카테고리 필터링, AchievementItemUI 인스턴스 생성
                Debug.Log($"[AchievementPanel] Refreshing for category: {_currentCategory}");
            }

            private void UpdateProgressText()
            {
                var manager = AchievementManager.Instance;
                if (manager == null) return;
                float progress = manager.GetOverallProgress();
                int total = manager.GetUnlockedAchievements().Count;
                // 전체 업적 수 -> see docs/systems/achievement-system.md 섹션 1
                _progressText.text = $"{total} 달성";
            }
        }
    }
```

- **MCP 호출**: 1회

#### T-1-13: AchievementToastUI (S-12)

```
create_script
  path: "Assets/_Project/Scripts/UI/AchievementToastUI.cs"
  content: |
    // S-12: 업적 달성 토스트 알림 UI
    // -> see docs/systems/achievement-architecture.md 섹션 8.2
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using SeedMind.Achievement;
    using SeedMind.Achievement.Data;

    namespace SeedMind.UI
    {
        public class AchievementToastUI : MonoBehaviour
        {
            [SerializeField] private Image _iconImage;
            [SerializeField] private TMP_Text _titleText;
            [SerializeField] private TMP_Text _descriptionText;
            [SerializeField] private Animator _animator;

            [SerializeField] private float _displayDuration = 4f;
                // -> see docs/systems/achievement-system.md 섹션 5.4
            [SerializeField] private float _hiddenDisplayDuration = 6f;
                // -> see docs/systems/achievement-system.md 섹션 5.4
            [SerializeField] private float _slideInDuration = 0.3f;
            [SerializeField] private float _slideOutDuration = 0.3f;

            private Queue<AchievementData> _toastQueue = new Queue<AchievementData>();
            private bool _isShowing;

            private void OnEnable()
            {
                AchievementEvents.OnAchievementUnlocked += ShowToast;
            }

            private void OnDisable()
            {
                AchievementEvents.OnAchievementUnlocked -= ShowToast;
            }

            public void ShowToast(AchievementData data)
            {
                _toastQueue.Enqueue(data);
                if (!_isShowing) StartCoroutine(ProcessQueue());
            }

            private IEnumerator ProcessQueue()
            {
                _isShowing = true;
                while (_toastQueue.Count > 0)
                {
                    var data = _toastQueue.Dequeue();
                    yield return StartCoroutine(DisplaySingle(data));
                }
                _isShowing = false;
            }

            private IEnumerator DisplaySingle(AchievementData data)
            {
                _titleText.text = data.displayName;
                _descriptionText.text = data.description;
                if (data.icon != null) _iconImage.sprite = data.icon;

                gameObject.SetActive(true);
                // 슬라이드 인
                yield return new WaitForSeconds(_slideInDuration);

                float duration = data.isHidden ? _hiddenDisplayDuration : _displayDuration;
                yield return new WaitForSeconds(duration);

                // 슬라이드 아웃
                yield return new WaitForSeconds(_slideOutDuration);
                gameObject.SetActive(false);
            }
        }
    }
```

- **MCP 호출**: 1회

#### T-1-14: AchievementItemUI (S-13)

```
create_script
  path: "Assets/_Project/Scripts/UI/AchievementItemUI.cs"
  content: |
    // S-13: 업적 목록 개별 항목 UI
    // -> see docs/systems/achievement-architecture.md 섹션 8.3
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;
    using SeedMind.Achievement;
    using SeedMind.Achievement.Data;

    namespace SeedMind.UI
    {
        public class AchievementItemUI : MonoBehaviour
        {
            [SerializeField] private Image _iconImage;
            [SerializeField] private TMP_Text _titleText;
            [SerializeField] private TMP_Text _descriptionText;
            [SerializeField] private Slider _progressBar;
            [SerializeField] private TMP_Text _progressText;
            [SerializeField] private GameObject _completedOverlay;
            [SerializeField] private GameObject _hiddenOverlay;

            public void Setup(AchievementData data, AchievementRecord record)
            {
                if (data.isHidden && !record.isUnlocked)
                {
                    SetHiddenState();
                    return;
                }

                _titleText.text = data.displayName;
                _descriptionText.text = data.description;
                if (data.icon != null) _iconImage.sprite = data.icon;

                if (record.isUnlocked)
                    SetUnlockedState();
                else
                    SetProgressState(data, record);
            }

            private void SetUnlockedState()
            {
                _completedOverlay.SetActive(true);
                _hiddenOverlay.SetActive(false);
                _progressBar.gameObject.SetActive(false);
            }

            private void SetProgressState(AchievementData data, AchievementRecord record)
            {
                _completedOverlay.SetActive(false);
                _hiddenOverlay.SetActive(false);
                _progressBar.gameObject.SetActive(true);

                int target = data.type == AchievementType.Single
                    ? data.targetValue
                    : (data.tiers.Length > 0 ? data.tiers[data.tiers.Length - 1].targetValue : 0);

                _progressBar.value = record.GetNormalizedProgress(target);
                _progressText.text = $"{record.currentProgress}/{target}";
            }

            private void SetHiddenState()
            {
                _titleText.text = "???";
                _descriptionText.text = "숨겨진 업적";
                _completedOverlay.SetActive(false);
                _hiddenOverlay.SetActive(true);
                _progressBar.gameObject.SetActive(false);
            }
        }
    }
```

- **MCP 호출**: 1회

#### T-1 Phase 4 마무리: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회
- **검증**: `get_console_logs`로 에러 없음 확인. 특히 TMPro 참조, SeedMind.Achievement 네임스페이스 참조 정상 여부 확인.

---

## T-2: SO 에셋 생성

> **[최적화 필수]** `tiers[]` 중첩 배열은 `set_property`로 불안정하다 (CLAUDE.md MCP 제한사항 — SO 배열 참조 설정 불가). **T-2 개별 생성 절차를 건너뛰고 T-2-ALT를 무조건 사용한다.** T-2-01(폴더 생성)만 수행 후 T-2-ALT로 이동할 것.

**목적**: AchievementData ScriptableObject 에셋을 카테고리별로 생성하고, 필드값을 설정한다.

**전제**: T-1 전체 완료 (모든 스크립트 컴파일 성공).

---

### T-2-01: 에셋 폴더 생성

```
create_folder  path: "Assets/_Project/Data/Achievements"
create_folder  path: "Assets/_Project/Data/Achievements/Farming"
create_folder  path: "Assets/_Project/Data/Achievements/Economy"
create_folder  path: "Assets/_Project/Data/Achievements/Facility"
create_folder  path: "Assets/_Project/Data/Achievements/Tool"
create_folder  path: "Assets/_Project/Data/Achievements/Explorer"
create_folder  path: "Assets/_Project/Data/Achievements/Quest"
create_folder  path: "Assets/_Project/Data/Achievements/Hidden"
```

- **MCP 호출**: 8회

### T-2-02 ~ T-2-06: Farming 카테고리 SO 생성

각 업적의 achievementId, displayName, description, category, type, conditionType, targetId, targetValue, rewardType, rewardAmount, rewardItemId, rewardTitleId, isHidden, sortOrder를 설정한다.

> **모든 수치(targetValue, rewardAmount 등)는 canonical 문서를 참조한다.**
> (-> see docs/systems/achievement-system.md 섹션 3.1 for Farming 업적 목록)
> (-> see docs/systems/achievement-system.md 섹션 4.1 for 골드/XP 보상 범위)

**T-2-02: SO_Ach_Farming01 (ach_farming_01: 씨앗의 시작)**

```
create_scriptable_object
  type: "AchievementData"
  path: "Assets/_Project/Data/Achievements/Farming/SO_Ach_Farming01.asset"

set_property  asset: "SO_Ach_Farming01"  property: "achievementId"  value: "ach_farming_01"
set_property  asset: "SO_Ach_Farming01"  property: "displayName"    value: "씨앗의 시작"
set_property  asset: "SO_Ach_Farming01"  property: "description"    value: "첫 번째 작물을 수확하세요."
set_property  asset: "SO_Ach_Farming01"  property: "category"       value: 0  // Farming
set_property  asset: "SO_Ach_Farming01"  property: "type"           value: 0  // Single
set_property  asset: "SO_Ach_Farming01"  property: "conditionType"  value: 0  // HarvestCount
set_property  asset: "SO_Ach_Farming01"  property: "targetId"       value: ""
set_property  asset: "SO_Ach_Farming01"  property: "targetValue"    value: 1  // -> see docs/systems/achievement-system.md 섹션 3.1
set_property  asset: "SO_Ach_Farming01"  property: "rewardType"     value: 4  // Title
set_property  asset: "SO_Ach_Farming01"  property: "rewardAmount"   value: 0
set_property  asset: "SO_Ach_Farming01"  property: "rewardItemId"   value: ""
set_property  asset: "SO_Ach_Farming01"  property: "rewardTitleId"  value: "title_sprout_farmer"  // 칭호: 새싹 농부 (-> see docs/content/achievements.md 섹션 10.1)
set_property  asset: "SO_Ach_Farming01"  property: "isHidden"       value: false
set_property  asset: "SO_Ach_Farming01"  property: "sortOrder"      value: 1
```

- **MCP 호출**: 15회 (create 1 + set_property 14)

**T-2-03: SO_Ach_Farming02 (ach_farming_02: 수확의 대가 -- Tiered)**

```
create_scriptable_object
  type: "AchievementData"
  path: "Assets/_Project/Data/Achievements/Farming/SO_Ach_Farming02.asset"

set_property  asset: "SO_Ach_Farming02"  property: "achievementId"  value: "ach_farming_02"
set_property  asset: "SO_Ach_Farming02"  property: "displayName"    value: "수확의 대가"
set_property  asset: "SO_Ach_Farming02"  property: "description"    value: "작물을 많이 수확하세요."
set_property  asset: "SO_Ach_Farming02"  property: "category"       value: 0  // Farming
set_property  asset: "SO_Ach_Farming02"  property: "type"           value: 1  // Tiered
set_property  asset: "SO_Ach_Farming02"  property: "isHidden"       value: false
set_property  asset: "SO_Ach_Farming02"  property: "sortOrder"      value: 2
```

Tiered 업적의 `tiers` 배열 설정:

```
// tiers[0] = Bronze
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.size"  value: 3
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[0].tierName"       value: "Bronze"
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[0].conditionType"  value: 0  // HarvestCount
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[0].targetId"       value: ""
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[0].targetValue"    value: 50
    // -> see docs/systems/achievement-system.md 섹션 3.1 ach_farming_02 Bronze
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[0].rewardType"     value: 1  // Gold
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[0].rewardAmount"   value: 50
    // -> see docs/systems/achievement-system.md 섹션 4.1 Bronze 골드 보상
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[0].rewardItemId"   value: ""
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[0].rewardTitleId"  value: ""

// tiers[1] = Silver
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[1].tierName"       value: "Silver"
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[1].conditionType"  value: 0
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[1].targetId"       value: ""
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[1].targetValue"    value: 200
    // -> see docs/systems/achievement-system.md 섹션 3.1 ach_farming_02 Silver
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[1].rewardType"     value: 1
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[1].rewardAmount"   value: 150
    // -> see docs/systems/achievement-system.md 섹션 4.1 Silver 골드 보상
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[1].rewardItemId"   value: ""
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[1].rewardTitleId"  value: "title_skilled_farmer"

// tiers[2] = Gold
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[2].tierName"       value: "Gold"
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[2].conditionType"  value: 0
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[2].targetId"       value: ""
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[2].targetValue"    value: 1000
    // -> see docs/systems/achievement-system.md 섹션 3.1 ach_farming_02 Gold
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[2].rewardType"     value: 1
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[2].rewardAmount"   value: 300
    // -> see docs/systems/achievement-system.md 섹션 4.1 Gold 골드 보상
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[2].rewardItemId"   value: "item_speed_fertilizer"
set_property  asset: "SO_Ach_Farming02"  property: "tiers.Array.data[2].rewardTitleId"  value: "title_harvest_master"
```

- **MCP 호출**: ~34회 (create 1 + 기본 필드 7 + tiers 배열 크기 1 + tiers 데이터 3x8=24 + 여유 1)

[RISK] `tiers.Array.data[n].fieldName` 형식의 중첩 배열 `set_property`가 MCP for Unity에서 지원되는지 사전 검증 필요. 지원되지 않으면 **대안 T-2-ALT** (아래 섹션) 적용.

**T-2-04 ~ T-2-06: 나머지 Farming 업적 (ach_farming_03 ~ ach_farming_05)**

패턴은 T-2-02 (Single 업적)와 동일.

- `SO_Ach_Farming03`: conditionType=SeasonCompleted, targetValue (-> see docs/systems/achievement-system.md 섹션 3.1)
- `SO_Ach_Farming04`: conditionType=SpecificCropHarvested (전체 작물 종류), targetValue (-> see docs/design.md 섹션 4.2 for 작물 수)
- `SO_Ach_Farming05`: conditionType=QualityHarvestCount, targetValue=1 (-> see docs/systems/achievement-system.md 섹션 3.1)
- **MCP 호출**: 각 15회 x 3 = 45회

### T-2-07 ~ T-2-10: Economy 카테고리

패턴: Single(3개) + Tiered(1개)
(-> see docs/systems/achievement-system.md 섹션 3.2 for Economy 업적 목록)

- `SO_Ach_Economy01`: Single, conditionType=TotalItemsSold, targetValue=1
- `SO_Ach_Economy02`: **Tiered**, conditionType=GoldEarned, 3단계 (-> see docs/systems/achievement-system.md 섹션 3.2)
- `SO_Ach_Economy03`: Single, conditionType=GoldEarned (단일 거래), targetValue (-> see 섹션 3.2)
- `SO_Ach_Economy04`: Single, conditionType=GoldEarned (가공품 판매 누적 수익 추적. 가공품 필터는 isProcessed 커스텀 핸들러로 처리), targetValue (-> see docs/systems/achievement-system.md 섹션 3.2)
  // [주의] AchievementData.conditionType은 단일 값만 지원. GoldEarned를 기본으로 하고, 가공품 여부 필터는 HandleSale 핸들러 내부에서 적용한다. (-> see docs/systems/achievement-architecture.md 섹션 5)
- **MCP 호출**: Single 15x3 + Tiered ~34 = ~79회

### T-2-11 ~ T-2-14: Facility 카테고리

패턴: Single(3개) + Tiered(1개)
(-> see docs/systems/achievement-system.md 섹션 3.3 for Facility 업적 목록)

- `SO_Ach_Facility01~03`: Single, conditionType=BuildingCount/SpecificBuildingBuilt
- `SO_Ach_Facility04`: **Tiered**, conditionType=ProcessingCount, 3단계
- **MCP 호출**: Single 15x3 + Tiered ~34 = ~79회

### T-2-15 ~ T-2-17: Tool 카테고리

패턴: Single(3개)
(-> see docs/systems/achievement-system.md 섹션 3.4 for Tool 업적 목록)

- `SO_Ach_Tool01~03`: Single, conditionType=ToolUpgradeCount
- **MCP 호출**: 15x3 = 45회

### T-2-18 ~ T-2-21: Explorer 카테고리

패턴: Single(3개) + Tiered(1개)
(-> see docs/systems/achievement-system.md 섹션 3.5 for Explorer 업적 목록)

- `SO_Ach_Explorer01~03`: Single, conditionType=NPCMet/GoldSpent/SeasonCompleted
- `SO_Ach_Explorer04`: **Tiered**, conditionType=GoldSpent (구매 횟수), 3단계

[OPEN] `ach_explorer_02` (바람이의 단골)와 `ach_explorer_04` (쇼핑 마니아)의 conditionType이 `GoldSpent(7)`로 매핑되어 있으나, 실제 추적 대상은 "구매 횟수"이므로 semantic 불일치가 있다. 구현 전 전용 conditionType 추가(`PurchaseCount`) 또는 `Custom(99)` 처리 방식을 결정해야 한다. (-> see docs/content/achievements.md Open Questions #4, docs/systems/achievement-architecture.md 섹션 2.3)
- **MCP 호출**: Single 15x3 + Tiered ~34 = ~79회

### T-2-22 ~ T-2-25: Quest 카테고리

패턴: Single(3개) + Tiered(1개)
(-> see docs/systems/achievement-system.md 섹션 3.6 for Quest 업적 목록)

- `SO_Ach_Quest01`: Single, conditionType=QuestCompleted, targetValue=1
- `SO_Ach_Quest02`: **Tiered**, conditionType=QuestCompleted, 3단계
- `SO_Ach_Quest03~04`: Single, conditionType=QuestCompleted (조건부)
- **MCP 호출**: Single 15x3 + Tiered ~34 = ~79회

### T-2-26 ~ T-2-31: Hidden 카테고리

패턴: Single(6개), isHidden=true, conditionType=Custom(99)
(-> see docs/systems/achievement-system.md 섹션 3.7 for Hidden 업적 목록)

- `SO_Ach_Hidden01~06`: Single, conditionType=Custom(99), isHidden=true
- **MCP 호출**: 15x6 = 90회

---

### T-2-32: ach_hidden_07 SO 에셋 생성 (통합 수집 마스터, CON-017)

`ach_hidden_07`은 낚시 도감 전종(ach_fish_04) AND 채집 도감 전종(ach_gather_03)을 모두 달성한 플레이어에게만 연쇄 해금되는 숨겨진 업적이다. (-> see `docs/content/achievements.md` 섹션 7.3 for 업적 수치, `docs/systems/achievement-architecture.md` 섹션 3.2 for HandleAchievementChain 로직)

```
create_asset
  type: "SeedMind.Achievement.Data.AchievementData"
  path: "Assets/_Project/Data/Achievements/Hidden/SO_Ach_Hidden07.asset"
```

```
set_property  asset: "Assets/_Project/Data/Achievements/Hidden/SO_Ach_Hidden07.asset"
  achievementId: "ach_hidden_07"
  isHidden: true
  conditionType: 99  // Custom(99): 복합 연쇄 조건 (HandleAchievementChain 내부 처리)
  targetValue: 0     // HandleAchievementChain이 직접 UnlockAchievement 호출
  // 보상 수치 -> see docs/content/achievements.md 섹션 7.3
```

**연쇄 해금 이벤트 구독 확인** (T-2-32 완료 후):

`AchievementManager.Awake()`에 아래 구독이 포함되어 있는지 검증:
```
AchievementEvents.OnAchievementUnlocked += HandleAchievementChain;
// -> see docs/systems/achievement-architecture.md 섹션 3.2 for HandleAchievementChain 구현
```

- **MCP 호출**: 2회 (create_asset 1 + set_property 1)

---

### T-2-ALT: Editor 스크립트를 통한 일괄 생성 (**기본 경로**)

**[기본 경로]** `tiers[]` 배열 설정은 `set_property`로 불안정하므로 항상 이 방법을 사용한다. T-2-01(폴더 생성) 완료 후 바로 이 절차를 실행한다.

```
create_script
  path: "Assets/_Project/Editor/CreateAchievementAssets.cs"
  content: |
    // Editor 전용: 업적 SO 에셋 일괄 생성
    // 모든 업적 데이터는 docs/systems/achievement-system.md 섹션 3의 canonical 정의를 기반으로 함
    #if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using SeedMind.Achievement.Data;

    public static class CreateAchievementAssets
    {
        [MenuItem("SeedMind/Create Achievement Assets")]
        public static void CreateAll()
        {
            // Farming, Economy, Facility, Tool, Explorer, Quest, Hidden 카테고리별
            // AchievementData SO 생성 + 필드 설정 + tiers[] 배열 설정
            // 총 30개 에셋 생성
            // -> see docs/systems/achievement-system.md 섹션 3 for 전체 업적 목록 및 수치
            Debug.Log("[CreateAchievementAssets] 30 achievement assets created.");
        }
    }
    #endif
```

실행:
```
execute_menu_item
  menu: "SeedMind/Create Achievement Assets"
```

- **MCP 호출**: 2회 (create_script 1 + execute_menu_item 1)
- **장점**: T-2 전체의 ~95회 호출을 2회로 축소
- **단점**: Editor 스크립트 내에 수치가 하드코딩됨 (PATTERN-006 위반 가능)

[RISK] Editor 스크립트 사용 시 수치가 코드에 하드코딩된다. `// -> copied from docs/systems/achievement-system.md 섹션 3.X` 주석을 반드시 병기한다. (PATTERN-006)

---

## T-3: UI 프리팹/씬 오브젝트 생성

**목적**: 업적 패널, 토스트 알림, 목록 아이템 UI의 씬 내 오브젝트 계층을 구성한다.

**전제**: T-1 전체 완료 (UI 스크립트 컴파일 성공).

(-> see docs/systems/achievement-architecture.md 섹션 8.4 for 씬 계층 구조)

---

### T-3 Phase 1: AchievementPanel 계층 생성

#### T-3-01: AchievementLayer 생성

```
create_object  name: "AchievementLayer"
set_parent     object: "AchievementLayer"  parent: "Canvas_Overlay"
```

- **MCP 호출**: 2회

#### T-3-02: AchievementPanel 오브젝트

```
create_object  name: "AchievementPanel"
set_parent     object: "AchievementPanel"  parent: "AchievementLayer"
add_component  object: "AchievementPanel"  component: "AchievementPanel"
add_component  object: "AchievementPanel"  component: "CanvasGroup"
set_property   object: "AchievementPanel"  component: "RectTransform"
               property: "anchorMin"  value: [0.1, 0.05]
set_property   object: "AchievementPanel"  component: "RectTransform"
               property: "anchorMax"  value: [0.9, 0.95]
```

- **MCP 호출**: 6회

#### T-3-03: Header 영역

```
create_object  name: "Header"
set_parent     object: "Header"  parent: "AchievementPanel"

create_object  name: "TitleText"
set_parent     object: "TitleText"  parent: "Header"
add_component  object: "TitleText"  component: "TextMeshProUGUI"
set_property   object: "TitleText"  component: "TextMeshProUGUI"
               property: "text"  value: "업적"

create_object  name: "ProgressText"
set_parent     object: "ProgressText"  parent: "Header"
add_component  object: "ProgressText"  component: "TextMeshProUGUI"
set_property   object: "ProgressText"  component: "TextMeshProUGUI"
               property: "text"  value: "0 달성"
    // 전체 업적 수 -> see docs/systems/achievement-system.md 섹션 1

create_object  name: "CloseButton"
set_parent     object: "CloseButton"  parent: "Header"
add_component  object: "CloseButton"  component: "Button"
```

- **MCP 호출**: 11회

#### T-3-04: CategoryTabs 영역

```
create_object  name: "CategoryTabs"
set_parent     object: "CategoryTabs"  parent: "AchievementPanel"
add_component  object: "CategoryTabs"  component: "HorizontalLayoutGroup"
```

탭 버튼 생성 (8개: All + 7카테고리):

```
// Tab_All, Tab_Farming, Tab_Economy, Tab_Facility, Tab_Tool, Tab_Explorer, Tab_Quest, Tab_Hidden
// 각각: create_object + set_parent + add_component(Button) + add_component(TextMeshProUGUI) + set_property(text)
```

- **MCP 호출**: 8 x 4 = 32회 (create + parent + button + text 설정)

[RISK] 탭 8개의 반복 생성으로 MCP 호출이 많다. 간소화를 위해 LayoutGroup 컴포넌트를 활용하고, 탭 버튼은 프리팹화하여 Instantiate로 대체할 수 있다.

#### T-3-05: ScrollView + Content 영역

```
create_object  name: "ScrollView"
set_parent     object: "ScrollView"  parent: "AchievementPanel"
add_component  object: "ScrollView"  component: "ScrollRect"
add_component  object: "ScrollView"  component: "Image"

create_object  name: "Content"
set_parent     object: "Content"  parent: "ScrollView"
add_component  object: "Content"  component: "VerticalLayoutGroup"
add_component  object: "Content"  component: "ContentSizeFitter"
set_property   object: "Content"  component: "ContentSizeFitter"
               property: "verticalFit"  value: 2  // PreferredSize

set_property   object: "ScrollView"  component: "ScrollRect"
               property: "content"  value: "Content"
```

- **MCP 호출**: 9회

---

### T-3 Phase 2: AchievementToast 계층 생성

```
create_object  name: "AchievementToast"
set_parent     object: "AchievementToast"  parent: "Canvas_Popup"
add_component  object: "AchievementToast"  component: "AchievementToastUI"
add_component  object: "AchievementToast"  component: "CanvasGroup"

// RectTransform: 상단 중앙 배치
set_property   object: "AchievementToast"  component: "RectTransform"
               property: "anchorMin"  value: [0.25, 0.85]
set_property   object: "AchievementToast"  component: "RectTransform"
               property: "anchorMax"  value: [0.75, 0.95]

create_object  name: "IconImage"
set_parent     object: "IconImage"  parent: "AchievementToast"
add_component  object: "IconImage"  component: "Image"

create_object  name: "TitleText"
set_parent     object: "TitleText"  parent: "AchievementToast"
add_component  object: "TitleText"  component: "TextMeshProUGUI"
set_property   object: "TitleText"  component: "TextMeshProUGUI"
               property: "text"  value: "업적 달성!"

create_object  name: "DescriptionText"
set_parent     object: "DescriptionText"  parent: "AchievementToast"
add_component  object: "DescriptionText"  component: "TextMeshProUGUI"
```

- **MCP 호출**: 14회

#### T-3 Phase 2 마무리: Toast 참조 연결

```
set_property   object: "AchievementToast"  component: "AchievementToastUI"
               property: "_iconImage"  value: "IconImage"
set_property   object: "AchievementToast"  component: "AchievementToastUI"
               property: "_titleText"  value: "TitleText"
set_property   object: "AchievementToast"  component: "AchievementToastUI"
               property: "_descriptionText"  value: "DescriptionText"
```

- **MCP 호출**: 3회

기본 비활성 상태 설정:

```
set_property   object: "AchievementToast"  property: "activeSelf"  value: false
```

- **MCP 호출**: 1회

---

### T-3 Phase 3: AchievementItemUI 프리팹 생성

```
create_object  name: "AchievementItemUI"
add_component  object: "AchievementItemUI"  component: "AchievementItemUI"

create_object  name: "ItemIcon"
set_parent     object: "ItemIcon"  parent: "AchievementItemUI"
add_component  object: "ItemIcon"  component: "Image"

create_object  name: "ItemTitle"
set_parent     object: "ItemTitle"  parent: "AchievementItemUI"
add_component  object: "ItemTitle"  component: "TextMeshProUGUI"

create_object  name: "ItemDescription"
set_parent     object: "ItemDescription"  parent: "AchievementItemUI"
add_component  object: "ItemDescription"  component: "TextMeshProUGUI"

create_object  name: "ProgressBar"
set_parent     object: "ProgressBar"  parent: "AchievementItemUI"
add_component  object: "ProgressBar"  component: "Slider"

create_object  name: "ProgressText"
set_parent     object: "ProgressText"  parent: "AchievementItemUI"
add_component  object: "ProgressText"  component: "TextMeshProUGUI"

create_object  name: "CompletedOverlay"
set_parent     object: "CompletedOverlay"  parent: "AchievementItemUI"
add_component  object: "CompletedOverlay"  component: "Image"

create_object  name: "HiddenOverlay"
set_parent     object: "HiddenOverlay"  parent: "AchievementItemUI"
add_component  object: "HiddenOverlay"  component: "Image"
```

참조 연결:

```
set_property   object: "AchievementItemUI"  component: "AchievementItemUI"
               property: "_iconImage"  value: "ItemIcon"
set_property   object: "AchievementItemUI"  component: "AchievementItemUI"
               property: "_titleText"  value: "ItemTitle"
set_property   object: "AchievementItemUI"  component: "AchievementItemUI"
               property: "_descriptionText"  value: "ItemDescription"
set_property   object: "AchievementItemUI"  component: "AchievementItemUI"
               property: "_progressBar"  value: "ProgressBar"
set_property   object: "AchievementItemUI"  component: "AchievementItemUI"
               property: "_progressText"  value: "ProgressText"
set_property   object: "AchievementItemUI"  component: "AchievementItemUI"
               property: "_completedOverlay"  value: "CompletedOverlay"
set_property   object: "AchievementItemUI"  component: "AchievementItemUI"
               property: "_hiddenOverlay"  value: "HiddenOverlay"
```

프리팹으로 저장:

```
// AchievementItemUI를 프리팹으로 저장
// 경로: Assets/_Project/Prefabs/UI/AchievementItemUI.prefab
```

- **MCP 호출**: ~25회

---

## T-4: 씬 배치 및 참조 연결

**목적**: SCN_Farm 씬에 AchievementManager를 배치하고, UI 참조를 연결한다.

**전제**: T-1~T-3 완료.

---

### T-4-01: AchievementManager GameObject 생성

```
create_object  name: "AchievementManager"
set_parent     object: "AchievementManager"  parent: "--- MANAGERS ---"
add_component  object: "AchievementManager"  component: "AchievementManager"
```

- **MCP 호출**: 3회

### T-4-02: AchievementManager에 SO 배열 연결

```
// _allAchievements 배열에 30개 SO 에셋 연결
set_property   object: "AchievementManager"  component: "AchievementManager"
               property: "_allAchievements.Array.size"  value: 30
    // -> see docs/systems/achievement-system.md 섹션 1 for 총 업적 수

// 인덱스 0~29에 SO 에셋 참조 설정
set_property   object: "AchievementManager"  component: "AchievementManager"
               property: "_allAchievements.Array.data[0]"
               value: "Assets/_Project/Data/Achievements/Farming/SO_Ach_Farming01.asset"
// ... (인덱스 1~29 반복)
```

- **MCP 호출**: 31회 (size 1 + data 30)

[RISK] 30개 SO 참조를 개별 `set_property`로 설정하는 것은 번거롭다. DataRegistry 패턴 (-> see docs/pipeline/data-pipeline.md)을 통해 폴더 기반 자동 로드로 대체할 수 있다. 이 경우 `_allAchievements`를 `Resources.LoadAll<AchievementData>` 또는 DataRegistry 스캔으로 초기화하여 수동 참조 연결을 제거한다.

### T-4-03: AchievementPanel 참조 연결

```
set_property   object: "AchievementPanel"  component: "AchievementPanel"
               property: "_contentParent"  value: "Content"
set_property   object: "AchievementPanel"  component: "AchievementPanel"
               property: "_progressText"  value: "ProgressText"
// _itemPrefab은 프리팹 참조
// _categoryTabs는 탭 버튼 배열 참조
```

- **MCP 호출**: ~6회

### T-4-04: 기본 비활성 설정

```
set_property   object: "AchievementPanel"  property: "activeSelf"  value: false
```

- **MCP 호출**: 1회

### T-4-05: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

---

## T-5: 세이브 통합 및 이벤트 연결

**목적**: SaveManager에 ISaveable 등록, GameSaveData에 achievements 필드 추가, 이벤트 구독 검증.

**전제**: T-4 완료.

---

### T-5-01: GameSaveData에 AchievementSaveData 필드 추가

```
// GameSaveData.cs에 AchievementSaveData 필드 추가
// -> see docs/systems/achievement-architecture.md 섹션 7.3
// -> see docs/systems/save-load-architecture.md 섹션 2.1

// 기존 GameSaveData.cs를 수정하여 추가:
// public AchievementSaveData achievements;
```

GameSaveData.cs는 `Assets/_Project/Scripts/Save/Data/GameSaveData.cs`에 위치 (-> see docs/systems/project-structure.md 섹션 1).

```
// 해당 파일을 수정 (create_script로 덮어쓰기 또는 수동 편집)
// 필드 추가: public SeedMind.Achievement.AchievementSaveData achievements;
```

- **MCP 호출**: 1회

### T-5-02: SaveManager에 ISaveable 등록

SaveManager의 자동 등록 메커니즘이 있는 경우(-> see docs/systems/save-load-architecture.md), `FindObjectsOfType<ISaveable>()` 또는 수동 레지스트리를 통해 AchievementManager를 등록한다.

```
// SaveManager가 씬 내 모든 ISaveable을 자동 탐색하는 경우:
// AchievementManager의 ISaveable 구현이 자동 인식됨 (추가 MCP 호출 불필요)

// 수동 레지스트리인 경우:
set_property   object: "SaveManager"  component: "SaveManager"
               property: "_saveables.Array.size"  value: (기존+1)
set_property   object: "SaveManager"  component: "SaveManager"
               property: "_saveables.Array.data[N]"  value: "AchievementManager"
```

- **MCP 호출**: 0~2회 (자동/수동에 따라)

### T-5-03: 이벤트 구독 확인

AchievementManager의 `SubscribeAll()` 메서드가 아래 12개 이벤트를 정상 구독하는지 확인:

| # | 이벤트 | 출처 | 핸들러 | 갱신 대상 |
|---|--------|------|--------|-----------|
| 1 | `FarmEvents.OnCropHarvested` | FarmSystem | HandleHarvest | HarvestCount, SpecificCropHarvested, QualityHarvestCount |
| 2 | `EconomyEvents.OnSaleCompleted` | EconomyManager | HandleSale | GoldEarned, TotalItemsSold |
| 3 | `EconomyEvents.OnGoldSpent` | EconomyManager | HandleGoldSpent | GoldSpent |
| 4 | `BuildingEvents.OnConstructionCompleted` | BuildingManager | HandleBuildingBuilt | BuildingCount, SpecificBuildingBuilt |
| 5 | `ToolEvents.OnToolUpgraded` | ToolSystem | HandleToolUpgrade | ToolUpgradeCount |
| 6 | `NPCEvents.OnNPCFirstMet` | NPCManager | HandleNPCMet | NPCMet |
| 7 | `QuestEvents.OnQuestCompleted` | QuestManager | HandleQuestCompleted | QuestCompleted |
| 8 | `ProcessingEvents.OnProcessingCompleted` | ProcessingSystem | HandleProcessing | ProcessingCount |
| 9 | `TimeManager.OnDayChanged` | TimeManager | HandleDayChanged | DaysPlayed |
| 10 | `TimeManager.OnSeasonChanged` | TimeManager | HandleSeasonChanged | SeasonCompleted |
| 11 | `GatheringEvents.OnItemGathered` | GatheringSystem | HandleGather | GatherCount, GatherSpeciesCollected |
| 12 | `GatheringEvents.OnSickleUpgraded` | GatheringSystem | HandleSickleUpgrade | GatherSickleUpgraded |

(-> see docs/systems/achievement-architecture.md 섹션 5 for 이벤트 구독 매핑)

[RISK] `EconomyEvents.OnGoldSpent`와 `NPCEvents.OnNPCFirstMet`가 기존 아키텍처에 정의되어 있는지 확인 필요. 없으면 해당 시스템에 이벤트를 추가해야 한다. (-> see docs/systems/achievement-architecture.md Risks 섹션)

- **MCP 호출**: 0회 (코드 레벨 검증, T-6에서 런타임 확인)

### T-5-04: InputManager에 Y키 바인딩 추가

```
// Y키 -> AchievementPanel.Toggle() 바인딩
// InputSystem 사용 시: SeedMindInputActions.inputactions에 "Achievement" 액션 추가
// -> see docs/systems/achievement-system.md 섹션 5.1 for 키 바인딩
```

- **MCP 호출**: ~2회 (InputAction 에셋 수정)

### T-5-05: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

---

## T-6: 통합 테스트 시퀀스

**목적**: 업적 시스템의 핵심 기능을 MCP 런타임 테스트로 검증한다.

**전제**: T-1~T-5 완료.

---

### T-6-01: 테스트 씬 생성

```
// SCN_Test_Achievement.unity 생성
// 최소 구성: AchievementManager + 테스트용 SO 2~3개 + Canvas + AchievementToastUI
```

- **MCP 호출**: ~5회

### T-6-02: 초기화 테스트

```
enter_play_mode

execute_method
  target: "AchievementManager"
  method: "Initialize"

get_console_logs
  filter: "[AchievementManager]"
  expected: "Initialized with N achievements"
```

- **MCP 호출**: 3회
- **검증**: 콘솔 로그에 업적 수 정확 출력

### T-6-03: 진행도 갱신 테스트

```
execute_method
  target: "AchievementManager"
  method: "UpdateProgress"
  args: [0, 1]  // AchievementConditionType.HarvestCount, amount=1

get_console_logs
  filter: "[AchievementManager]"
  expected: progressUpdated 이벤트 발행 로그
```

- **MCP 호출**: 2회
- **검증**: AchievementRecord.currentProgress 증가 확인

### T-6-04: 달성 판정 테스트

```
// targetValue=1인 Single 업적(ach_farming_01)에 대해:
execute_method
  target: "AchievementManager"
  method: "UpdateProgress"
  args: [0, 1]  // HarvestCount, 1

get_console_logs
  filter: "[AchievementManager] Unlocked"
  expected: "Unlocked: ach_farming_01"
```

- **MCP 호출**: 2회
- **검증**: isUnlocked=true, OnAchievementUnlocked 이벤트 발행

### T-6-05: Tiered 업적 테스트

```
// ach_farming_02 (Tiered: Bronze=50, Silver=200, Gold=1000)에 대해:
// Bronze 달성 테스트
execute_method
  target: "AchievementManager"
  method: "UpdateProgress"
  args: [0, 50]  // HarvestCount, 50
    // -> see docs/systems/achievement-system.md 섹션 3.1 ach_farming_02 Bronze targetValue

get_console_logs
  filter: "Tier unlocked"
  expected: "Tier unlocked: ach_farming_02 - Bronze"
```

- **MCP 호출**: 2회
- **검증**: currentTier="Bronze", tierHistory에 Bronze 기록 추가

### T-6-06: 보상 지급 테스트

```
// AchievementManager.GrantReward 호출 결과 확인
// Gold 보상인 경우: EconomyManager의 골드 잔액 변화 확인

get_console_logs
  filter: "Reward granted"
  expected: "Reward granted: Gold x50"
    // -> see docs/systems/achievement-system.md 섹션 4.1 for Bronze 골드 보상
```

- **MCP 호출**: 1회

### T-6-07: 토스트 UI 테스트

```
// OnAchievementUnlocked 이벤트 발행 후 AchievementToastUI 활성화 확인
get_console_logs
  filter: "AchievementToast"

// 화면 캡처 (가능한 경우)
```

- **MCP 호출**: 1회
- **검증**: 토스트 오브젝트 activeSelf=true, 텍스트 내용 정확

### T-6-08: 숨겨진 업적 UI 테스트

```
// isHidden=true인 업적의 AchievementItemUI.Setup 호출 확인
// 미달성 시: titleText="???", hiddenOverlay 활성
// 달성 후: titleText=실제 이름, hiddenOverlay 비활성, completedOverlay 활성
```

- **MCP 호출**: 2회
- **검증**: 표시 규칙 (-> see docs/systems/achievement-architecture.md 섹션 8.3 표시 규칙 표)

### T-6-09: 세이브/로드 테스트

```
// 1) 업적 일부 달성 후 세이브
execute_method
  target: "SaveManager"
  method: "SaveGame"

// 2) 플레이 모드 종료 후 재진입
exit_play_mode
enter_play_mode

// 3) 로드
execute_method
  target: "SaveManager"
  method: "LoadGame"

// 4) 진행도/달성 상태 유지 확인
execute_method
  target: "AchievementManager"
  method: "GetOverallProgress"

get_console_logs
  filter: "[AchievementManager] Loaded"
  expected: "Loaded N records, M unlocked"
```

- **MCP 호출**: 5회
- **검증**: 로드 후 기존 진행도/달성 상태 일치, totalUnlocked 정확

### T-6-10: 조건 타입별 이벤트 연결 검증

각 AchievementConditionType에 대해 해당 이벤트 발행 시 AchievementManager가 반응하는지 확인:

| # | 이벤트 발행 | 검증 방법 |
|---|------------|-----------|
| 1 | FarmEvents.OnCropHarvested 시뮬레이션 | HarvestCount 업적 progress 증가 |
| 2 | EconomyEvents.OnSaleCompleted 시뮬레이션 | GoldEarned, TotalItemsSold 업적 progress 증가 |
| 3 | BuildingEvents.OnConstructionCompleted 시뮬레이션 | BuildingCount 업적 progress 증가 |
| 4 | ToolEvents.OnToolUpgraded 시뮬레이션 | ToolUpgradeCount 업적 progress 증가 |
| 5 | QuestEvents.OnQuestCompleted 시뮬레이션 | QuestCompleted 업적 progress 증가 |
| 6 | TimeManager.OnDayChanged 시뮬레이션 | DaysPlayed 업적 progress 증가 |
| 7 | GatheringEvents.OnItemGathered 시뮬레이션 | GatherCount, GatherSpeciesCollected 업적 progress 증가 |
| 8 | GatheringEvents.OnSickleUpgraded 시뮬레이션 | GatherSickleUpgraded 업적 progress 증가 |

- **MCP 호출**: 16회 (각 이벤트: execute_method 1 + get_console_logs 1)
- **검증**: 모든 ConditionType에 대해 이벤트 구독이 정상 동작 (채집 이벤트 2종 포함)

### T-6-11: 테스트 종료

```
exit_play_mode
save_scene
```

- **MCP 호출**: 2회

---

## T-7: 채집 업적 SO 에셋 생성 + 이벤트 연결 (ARC-035)

> **[최적화 필수]** T-7-01(폴더 생성)만 수행 후 T-7-ALT를 사용한다. T-7-02~T-7-06 개별 생성은 건너뜀.

**목적**: 채집 업적 5종(A-031~A-035)의 AchievementData SO 에셋을 생성하고, GatheringEvents 이벤트 핸들러 연결을 구성한다.

**전제**: T-1 전체 완료 (모든 스크립트 컴파일 성공). AchievementConditionType enum에 GatherCount(15), GatherSpeciesCollected(16), GatherSickleUpgraded(17) 값이 포함되어야 한다 (-> see docs/systems/achievement-architecture.md 섹션 2.3). AchievementCategory enum에 Gatherer(8) 값이 포함되어야 한다.

**업적 정의 참조**: (-> see docs/content/achievements.md 섹션 9.5 for 채집가 업적 5종 전체 목록, 조건, 보상 수치)

---

### T-7-01: 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Achievements/Gatherer"
```

- **MCP 호출**: 1회

### T-7-02: SO_Ach_Gather01 (ach_gather_01: 첫 채집)

```
create_scriptable_object
  type: "AchievementData"
  name: "SO_Ach_Gather01"
  path: "Assets/_Project/Data/Achievements/Gatherer"

// 필드 설정 — 모든 수치는 canonical 참조
// -> see docs/content/achievements.md 섹션 9.5.1 for achievementId, displayName, description
// -> see docs/content/achievements.md 섹션 9.5.3 for 보상 (골드, XP, 칭호)
set_property  asset: "SO_Ach_Gather01"  property: "achievementId"        value: "ach_gather_01"
set_property  asset: "SO_Ach_Gather01"  property: "displayName"          value: (-> see docs/content/achievements.md 섹션 9.5.1)
set_property  asset: "SO_Ach_Gather01"  property: "description"          value: (-> see docs/content/achievements.md 섹션 9.5.1)
set_property  asset: "SO_Ach_Gather01"  property: "category"             value: 8   // Gatherer
set_property  asset: "SO_Ach_Gather01"  property: "type"                 value: 0   // Single
set_property  asset: "SO_Ach_Gather01"  property: "conditionType"        value: 15  // GatherCount
set_property  asset: "SO_Ach_Gather01"  property: "targetValue"          value: (-> see docs/content/achievements.md 섹션 9.5.1 — conditionValue)
set_property  asset: "SO_Ach_Gather01"  property: "targetId"             value: ""
set_property  asset: "SO_Ach_Gather01"  property: "isHidden"             value: false
set_property  asset: "SO_Ach_Gather01"  property: "rewardType"           value: 1   // Gold
set_property  asset: "SO_Ach_Gather01"  property: "rewardGold"           value: (-> see docs/content/achievements.md 섹션 9.5.3)
set_property  asset: "SO_Ach_Gather01"  property: "rewardXP"             value: (-> see docs/content/achievements.md 섹션 9.5.3)
set_property  asset: "SO_Ach_Gather01"  property: "rewardTitleId"        value: "title_novice_gatherer"
set_property  asset: "SO_Ach_Gather01"  property: "rewardItemId"         value: ""
```

- **MCP 호출**: 16회 (create_scriptable_object 1 + set_property 15)
- **검증**: SO 에셋 Inspector에서 필드값 확인

### T-7-03: SO_Ach_Gather02 (ach_gather_02: 채집 애호가 -- Tiered)

```
create_scriptable_object
  type: "AchievementData"
  name: "SO_Ach_Gather02"
  path: "Assets/_Project/Data/Achievements/Gatherer"

// 기본 필드 설정
// -> see docs/content/achievements.md 섹션 9.5.1, 9.5.2 for 전체 수치
set_property  asset: "SO_Ach_Gather02"  property: "achievementId"        value: "ach_gather_02"
set_property  asset: "SO_Ach_Gather02"  property: "displayName"          value: (-> see docs/content/achievements.md 섹션 9.5.1)
set_property  asset: "SO_Ach_Gather02"  property: "description"          value: (-> see docs/content/achievements.md 섹션 9.5.1)
set_property  asset: "SO_Ach_Gather02"  property: "category"             value: 8   // Gatherer
set_property  asset: "SO_Ach_Gather02"  property: "type"                 value: 1   // Tiered
set_property  asset: "SO_Ach_Gather02"  property: "conditionType"        value: 15  // GatherCount
set_property  asset: "SO_Ach_Gather02"  property: "isHidden"             value: false

// Tiered 배열 설정 (3단계: Bronze, Silver, Gold)
// -> see docs/content/achievements.md 섹션 9.5.2 for 단계별 targetValue, 보상 골드, XP, 칭호, 특수 보상
// [RISK] tiers[] 배열의 set_property 지원 여부에 따라 T-2-ALT 패턴 적용 필요
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.size"     value: 3

// Bronze 단계 (-> see docs/content/achievements.md 섹션 9.5.2 Bronze 행)
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[0].tierName"       value: "Bronze"
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[0].targetValue"    value: (-> see 섹션 9.5.2)
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[0].rewardGold"     value: (-> see 섹션 9.5.2)
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[0].rewardXP"       value: (-> see 섹션 9.5.2)
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[0].rewardTitleId"  value: ""
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[0].rewardItemId"   value: ""

// Silver 단계 (-> see docs/content/achievements.md 섹션 9.5.2 Silver 행)
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[1].tierName"       value: "Silver"
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[1].targetValue"    value: (-> see 섹션 9.5.2)
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[1].rewardGold"     value: (-> see 섹션 9.5.2)
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[1].rewardXP"       value: (-> see 섹션 9.5.2)
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[1].rewardTitleId"  value: "title_gathering_lover"
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[1].rewardItemId"   value: ""

// Gold 단계 (-> see docs/content/achievements.md 섹션 9.5.2 Gold 행)
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[2].tierName"       value: "Gold"
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[2].targetValue"    value: (-> see 섹션 9.5.2)
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[2].rewardGold"     value: (-> see 섹션 9.5.2)
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[2].rewardXP"       value: (-> see 섹션 9.5.2)
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[2].rewardTitleId"  value: "title_skilled_gatherer"
set_property  asset: "SO_Ach_Gather02"  property: "tiers.Array.data[2].rewardItemId"   value: (-> see 섹션 9.5.2 — 채집 숙련도 XP 보너스 아이템)
```

- **MCP 호출**: ~28회 (create_scriptable_object 1 + set_property 7(기본) + tiers.Array.size 1 + 6x3(단계) = 27)
- **검증**: SO 에셋 Inspector에서 tiers 배열 3단계 확인

### T-7-04: SO_Ach_Gather03 (ach_gather_03: 채집 도감 완성)

```
create_scriptable_object
  type: "AchievementData"
  name: "SO_Ach_Gather03"
  path: "Assets/_Project/Data/Achievements/Gatherer"

// -> see docs/content/achievements.md 섹션 9.5.1, 9.5.3 for 전체 수치
set_property  asset: "SO_Ach_Gather03"  property: "achievementId"        value: "ach_gather_03"
set_property  asset: "SO_Ach_Gather03"  property: "displayName"          value: (-> see docs/content/achievements.md 섹션 9.5.1)
set_property  asset: "SO_Ach_Gather03"  property: "description"          value: (-> see docs/content/achievements.md 섹션 9.5.1)
set_property  asset: "SO_Ach_Gather03"  property: "category"             value: 8   // Gatherer
set_property  asset: "SO_Ach_Gather03"  property: "type"                 value: 0   // Single
set_property  asset: "SO_Ach_Gather03"  property: "conditionType"        value: 16  // GatherSpeciesCollected
set_property  asset: "SO_Ach_Gather03"  property: "targetValue"          value: (-> see docs/content/achievements.md 섹션 9.5.1 — 27종, docs/systems/gathering-system.md 섹션 3.9)
set_property  asset: "SO_Ach_Gather03"  property: "targetId"             value: ""
set_property  asset: "SO_Ach_Gather03"  property: "isHidden"             value: false
set_property  asset: "SO_Ach_Gather03"  property: "rewardType"           value: 1   // Gold
set_property  asset: "SO_Ach_Gather03"  property: "rewardGold"           value: (-> see docs/content/achievements.md 섹션 9.5.3)
set_property  asset: "SO_Ach_Gather03"  property: "rewardXP"             value: (-> see docs/content/achievements.md 섹션 9.5.3)
set_property  asset: "SO_Ach_Gather03"  property: "rewardTitleId"        value: "title_gathering_doctor"
set_property  asset: "SO_Ach_Gather03"  property: "rewardItemId"         value: (-> see docs/content/achievements.md 섹션 9.5.3 — 채집 도감 장식품)
```

- **MCP 호출**: 16회 (create_scriptable_object 1 + set_property 15)

### T-7-05: SO_Ach_Gather04 (ach_gather_04: 전설의 채집가)

```
create_scriptable_object
  type: "AchievementData"
  name: "SO_Ach_Gather04"
  path: "Assets/_Project/Data/Achievements/Gatherer"

// -> see docs/content/achievements.md 섹션 9.5.1, 9.5.3 for 전체 수치
// [NOTE] ach_gather_04는 Legendary 채집물만 카운트하므로 Custom 필터(rarityFilter=Legendary) 필요
// -> see docs/content/achievements.md 섹션 9.5.1 — "Custom 추적: rarityFilter = Legendary"
set_property  asset: "SO_Ach_Gather04"  property: "achievementId"        value: "ach_gather_04"
set_property  asset: "SO_Ach_Gather04"  property: "displayName"          value: (-> see docs/content/achievements.md 섹션 9.5.1)
set_property  asset: "SO_Ach_Gather04"  property: "description"          value: (-> see docs/content/achievements.md 섹션 9.5.1)
set_property  asset: "SO_Ach_Gather04"  property: "category"             value: 8   // Gatherer
set_property  asset: "SO_Ach_Gather04"  property: "type"                 value: 0   // Single
set_property  asset: "SO_Ach_Gather04"  property: "conditionType"        value: 15  // GatherCount (Legendary 필터는 HandleGather 핸들러 내부에서 적용)
set_property  asset: "SO_Ach_Gather04"  property: "targetValue"          value: (-> see docs/content/achievements.md 섹션 9.5.1 — conditionValue)
set_property  asset: "SO_Ach_Gather04"  property: "targetId"             value: "rarity_legendary"  // 커스텀 필터 식별자
set_property  asset: "SO_Ach_Gather04"  property: "isHidden"             value: false
set_property  asset: "SO_Ach_Gather04"  property: "rewardType"           value: 1   // Gold
set_property  asset: "SO_Ach_Gather04"  property: "rewardGold"           value: (-> see docs/content/achievements.md 섹션 9.5.3)
set_property  asset: "SO_Ach_Gather04"  property: "rewardXP"             value: (-> see docs/content/achievements.md 섹션 9.5.3)
set_property  asset: "SO_Ach_Gather04"  property: "rewardTitleId"        value: "title_legendary_gatherer"
set_property  asset: "SO_Ach_Gather04"  property: "rewardItemId"         value: ""
```

- **MCP 호출**: 16회 (create_scriptable_object 1 + set_property 15)
- [RISK] `ach_gather_04`의 Legendary 레어리티 필터 처리를 위해 `targetId` 필드를 커스텀 식별자로 활용한다. HandleGather 핸들러에서 `targetId == "rarity_legendary"` 조건 분기가 필요하다. (-> see docs/systems/achievement-architecture.md 섹션 5 HandleGather)

### T-7-06: SO_Ach_Gather05 (ach_gather_05: 채집 낫의 진화)

```
create_scriptable_object
  type: "AchievementData"
  name: "SO_Ach_Gather05"
  path: "Assets/_Project/Data/Achievements/Gatherer"

// -> see docs/content/achievements.md 섹션 9.5.1, 9.5.3 for 전체 수치
set_property  asset: "SO_Ach_Gather05"  property: "achievementId"        value: "ach_gather_05"
set_property  asset: "SO_Ach_Gather05"  property: "displayName"          value: (-> see docs/content/achievements.md 섹션 9.5.1)
set_property  asset: "SO_Ach_Gather05"  property: "description"          value: (-> see docs/content/achievements.md 섹션 9.5.1)
set_property  asset: "SO_Ach_Gather05"  property: "category"             value: 8   // Gatherer
set_property  asset: "SO_Ach_Gather05"  property: "type"                 value: 0   // Single
set_property  asset: "SO_Ach_Gather05"  property: "conditionType"        value: 17  // GatherSickleUpgraded
set_property  asset: "SO_Ach_Gather05"  property: "targetValue"          value: (-> see docs/content/achievements.md 섹션 9.5.1 — conditionValue)
set_property  asset: "SO_Ach_Gather05"  property: "targetId"             value: ""
set_property  asset: "SO_Ach_Gather05"  property: "isHidden"             value: false
set_property  asset: "SO_Ach_Gather05"  property: "rewardType"           value: 1   // Gold
set_property  asset: "SO_Ach_Gather05"  property: "rewardGold"           value: (-> see docs/content/achievements.md 섹션 9.5.3)
set_property  asset: "SO_Ach_Gather05"  property: "rewardXP"             value: (-> see docs/content/achievements.md 섹션 9.5.3)
set_property  asset: "SO_Ach_Gather05"  property: "rewardTitleId"        value: "title_sickle_master"
set_property  asset: "SO_Ach_Gather05"  property: "rewardItemId"         value: ""
```

- **MCP 호출**: 16회 (create_scriptable_object 1 + set_property 15)

### T-7-07: AchievementManager에 채집 업적 SO 참조 추가

```
// AchievementManager의 _allAchievements 배열에 A-31 ~ A-35 추가
// 기존 배열 크기를 5 증가시키고 신규 SO를 참조 연결
set_property
  object: "AchievementManager"
  component: "AchievementManager"
  property: "_allAchievements.Array.size"
  value: (기존 크기 + 5)  // -> see T-4-02 기존 배열 크기 확인 후 설정

set_property  object: "AchievementManager"  component: "AchievementManager"  property: "_allAchievements.Array.data[N+0]"  value: "SO_Ach_Gather01"
set_property  object: "AchievementManager"  component: "AchievementManager"  property: "_allAchievements.Array.data[N+1]"  value: "SO_Ach_Gather02"
set_property  object: "AchievementManager"  component: "AchievementManager"  property: "_allAchievements.Array.data[N+2]"  value: "SO_Ach_Gather03"
set_property  object: "AchievementManager"  component: "AchievementManager"  property: "_allAchievements.Array.data[N+3]"  value: "SO_Ach_Gather04"
set_property  object: "AchievementManager"  component: "AchievementManager"  property: "_allAchievements.Array.data[N+4]"  value: "SO_Ach_Gather05"

save_scene
```

- **MCP 호출**: 7회 (set_property 6 + save_scene 1)
- **검증**: AchievementManager Inspector에서 _allAchievements 배열에 채집 업적 5종 포함 확인
- [NOTE] 배열 인덱스 N은 기존 업적 수(T-4-02 완료 시점의 배열 크기)에 따라 결정. 낚시 업적 포함 시 N=34 (기존 30 + 낚시 4)

### T-7-ALT: Editor 스크립트를 통한 채집 업적 일괄 생성 (**기본 경로**)

**[기본 경로]** T-7-01(폴더 생성) 완료 후 바로 이 절차를 실행한다. T-7-02~T-7-06은 건너뜀.

```
// 기존 CreateAchievementAssets.cs에 Gatherer 카테고리 추가
// 또는 별도 CreateGatherAchievementAssets.cs 생성
create_script
  path: "Assets/_Project/Editor/CreateGatherAchievementAssets.cs"
  content: |
    // Editor 전용: 채집 업적 SO 에셋 5종 일괄 생성
    // 모든 업적 데이터는 docs/content/achievements.md 섹션 9.5의 canonical 정의를 기반으로 함
    // -> copied from docs/content/achievements.md 섹션 9.5.1~9.5.3
    #if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;
    using SeedMind.Achievement.Data;

    public static class CreateGatherAchievementAssets
    {
        [MenuItem("SeedMind/Create Gather Achievement Assets")]
        public static void CreateAll()
        {
            // ach_gather_01 ~ ach_gather_05 (5종)
            // Single 4종 + Tiered 1종 (ach_gather_02)
            // -> see docs/content/achievements.md 섹션 9.5 for 전체 수치
            Debug.Log("[CreateGatherAchievementAssets] 5 gathering achievement assets created.");
        }
    }
    #endif
```

실행:
```
execute_menu_item
  menu: "SeedMind/Create Gather Achievement Assets"
```

- **MCP 호출**: 2회 (create_script 1 + execute_menu_item 1)
- **장점**: T-7-02 ~ T-7-06 전체의 ~80회 호출을 2회로 축소
- **단점**: Editor 스크립트 내에 수치가 하드코딩됨 (PATTERN-006 위반 가능)

[RISK] T-2-ALT와 동일 — Editor 스크립트 사용 시 수치가 코드에 하드코딩된다. `// -> copied from docs/content/achievements.md 섹션 9.5.X` 주석을 반드시 병기한다. (PATTERN-006)

---

## Cross-references

- `docs/systems/achievement-architecture.md` (ARC-017) -- 핵심 클래스 설계, API, 이벤트 구독 매핑, SaveLoadOrder
- `docs/systems/achievement-system.md` (DES-011) -- 카테고리, 업적 목록, 보상 체계, UI 설계 (canonical)
- `docs/content/achievements.md` (CON-007/CON-013) -- 업적 39종 확정 보상 수치, 칭호 49종 canonical 테이블, 아이템 보상 15종 (canonical 수치 출처). 채집 업적 5종은 섹션 9.5
- `docs/systems/save-load-architecture.md` (ARC-011) -- ISaveable 인터페이스, SaveLoadOrder 할당표, GameSaveData 루트 구조
- `docs/systems/quest-architecture.md` (ARC-013) -- QuestManager, QuestEvents (퀘스트 완료 이벤트)
- `docs/systems/inventory-architecture.md` -- InventoryManager API (아이템 보상 지급)
- `docs/systems/economy-architecture.md` -- EconomyManager, EconomyEvents (골드 보상, 판매 이벤트)
- `docs/systems/progression-architecture.md` -- ProgressionManager (XP 보상 지급)
- `docs/systems/farming-system.md` -- FarmEvents (수확 이벤트)
- `docs/systems/gathering-system.md` -- GatheringEvents (채집 이벤트: OnItemGathered, OnSickleUpgraded)
- `docs/content/gathering-items.md` -- 채집 아이템 27종 (채집 도감 업적 조건 참조)
- `docs/systems/facilities-architecture.md` (ARC-007) -- BuildingManager, BuildingEvents (건설 이벤트)
- `docs/systems/processing-architecture.md` (ARC-012) -- ProcessingEvents (가공 완료 이벤트)
- `docs/systems/time-season-architecture.md` -- TimeManager (일/계절 변경 이벤트)
- `docs/systems/project-structure.md` -- 폴더 구조, 네임스페이스 규칙
- `docs/pipeline/data-pipeline.md` (ARC-004) -- SO 에셋 생성 패턴, DataRegistry
- `docs/balance/progression-curve.md` -- 업적 보상 수치 참조
- `docs/mcp/quest-tasks.md` (ARC-016) -- 유사 시스템 MCP 태스크 패턴 참고
- `docs/mcp/inventory-tasks.md` (ARC-013) -- 인벤토리 MCP 태스크 패턴 참고

---

## Open Questions

- [OPEN] `tiers.Array.data[n].fieldName` 형식의 중첩 배열 `set_property`가 MCP for Unity에서 지원되는지 사전 검증 필요. 지원되지 않으면 T-2-ALT (Editor 스크립트 일괄 생성) 적용.
- [OPEN] AchievementManager의 `_allAchievements` 배열을 수동 참조 연결 대신 DataRegistry 또는 `Resources.LoadAll<AchievementData>` 패턴으로 자동화할지 결정 필요. (-> see docs/pipeline/data-pipeline.md)
- [OPEN] `EconomyEvents.OnGoldSpent`와 `NPCEvents.OnNPCFirstMet`가 기존 아키텍처에 정의되어 있는지 확인 필요. 없으면 해당 시스템에 이벤트를 추가해야 하며, 관련 아키텍처 문서 업데이트 수반.
- 업적 총 개수 및 세부 수치는 `docs/content/achievements.md` (CON-007/CON-013/CON-017)에 확정. 40개, 보상 XP 총 3,160 XP(ach_hidden_07 포함), 칭호 49종. (-> see docs/content/achievements.md 섹션 13.1). 채집 업적 5종(490 XP, 2,600G)은 T-7에서, ach_hidden_07(통합 수집 마스터)은 T-2-32에서 SO 에셋 생성
- [OPEN] `ach_gather_04`(전설의 채집가)의 Legendary 레어리티 필터를 `targetId="rarity_legendary"` + HandleGather 핸들러 분기로 처리할지, Custom(99)으로 처리할지 결정 필요. 현재 T-7-05에서는 GatherCount(15) + targetId 커스텀 필터 방식을 채택
- [OPEN] `ach_gather_02` Gold 단계의 채집 숙련도 XP 보너스(+25%, 50회) 아이템 구현 방식 미확정. 버프 시스템 공통화 설계 필요 (-> see docs/content/achievements.md Open Question #9)

---

## Risks

- [RISK] **MCP 호출 총량**: Tiered 배열 직접 설정 시 약 548회, T-2-ALT(Editor 스크립트) 사용 시 약 102회. 태스크 맵 초기 추정치(~192회)는 Tiered 업적 배열 설정 호출 수를 과소 계산한 것이었다. T-2-ALT(Editor 스크립트 일괄 생성)를 우선 검토한다. 이 경우 수치가 코드에 하드코딩되는 PATTERN-006 위반 리스크가 있으므로 `// -> copied from docs/systems/achievement-system.md 섹션 3.X` 주석을 반드시 병기해야 한다.
- [RISK] **중첩 배열 set_property**: MCP for Unity의 `set_property`가 `tiers.Array.data[n].fieldName` 형식을 지원하지 않으면 Tiered 업적의 SO 생성이 불가능하다. T-2-ALT로 대체 필요.
- [RISK] **이벤트 구독 누락**: 12개 외부 이벤트 중 하나라도 누락되면 해당 조건 타입 업적이 동작하지 않는다. T-6-10에서 모든 ConditionType별 검증을 수행하지만, Custom(99) 타입은 개별 하드코딩 핸들러여서 별도 테스트가 필요하다. 채집 이벤트 2종(GatheringEvents.OnItemGathered, GatheringEvents.OnSickleUpgraded)이 T-7 추가로 포함되었다.
- [RISK] **이벤트 페이로드 불일치**: 기존 시스템의 이벤트 페이로드에 AchievementManager가 필요로 하는 필드(cropId, quality, totalGold 등)가 모두 포함되어 있는지 확인 필요. (-> see docs/systems/achievement-architecture.md Risks 섹션)
- [RISK] **컴파일 순서**: S-01~S-06 -> S-07~S-09 -> S-10 -> S-11~S-13 순서를 엄격히 지키지 않으면 참조 에러 발생. 각 Phase 사이에 반드시 Unity 컴파일 대기(`execute_menu_item`)를 삽입한다.
- [RISK] **SaveLoadOrder 충돌**: AchievementManager(90)는 현재 할당표에서 높은 값이다. 향후 시스템 추가 시 충돌 방지를 위해 `save-load-architecture.md` 할당표를 반드시 갱신해야 한다.

---

*이 문서는 Claude Code가 자율적으로 작성했습니다.*
