# 게임 진행 시스템 기술 아키텍처

> ProgressionManager, XP/레벨 데이터 구조, 해금(Unlock) 시스템, 마일스톤 시스템의 클래스 설계, 이벤트 연동, MCP 구현 계획  
> 작성: Claude Code (Opus) | 2026-04-06  
> 문서 ID: BAL-002

---

## Context

이 문서는 SeedMind의 게임 진행(Progression) 시스템의 기술 아키텍처를 정의한다. 진행 시스템은 플레이어의 경험치 획득, 레벨업, 콘텐츠 해금, 마일스톤 달성을 관리하며, 핵심 게임 루프에서 "확장/해금"의 동기를 제공하는 시스템이다.

기존 아키텍처의 `SeedMind.Level` 네임스페이스(`Scripts/Level/`)에 배치되며, `docs/pipeline/data-pipeline.md` 섹션 2.6에 정의된 LevelConfig SO와 호환된다.

**설계 목표**:
- XP 획득과 레벨업이 이벤트 기반으로 다른 시스템에 전파되어야 한다
- 모든 진행 수치(XP 테이블, 해금 목록)는 ScriptableObject로 외부화
- 해금 상태는 세이브/로드에 안전하게 포함되어야 한다
- 마일스톤 시스템은 레벨 외 달성 목표를 데이터 드리븐으로 관리

---

# Part I -- 시스템 설계

---

## 1. ProgressionManager 클래스 설계

### 1.1 클래스 다이어그램

```
┌─────────────────────────────────────────────────────────────────────┐
│                         SeedMind.Level                              │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────┐
│          ProgressionManager (MonoBehaviour, Singleton)         │
│──────────────────────────────────────────────────────────────│
│  [상태]                                                       │
│  - _currentLevel: int                                        │
│  - _currentExp: int                                          │
│  - _totalExpEarned: int          // 누적 총 경험치            │
│                                                              │
│  [설정 참조]                                                   │
│  - _progressionData: ProgressionData (ScriptableObject)      │
│  - _unlockRegistry: UnlockRegistry (내부 인스턴스)             │
│  - _milestoneTracker: MilestoneTracker (내부 인스턴스)         │
│                                                              │
│  [읽기 전용 프로퍼티]                                           │
│  + CurrentLevel: int                                         │
│  + CurrentExp: int                                           │
│  + ExpToNextLevel: int           // 다음 레벨까지 남은 EXP    │
│  + ExpProgress: float            // 0.0~1.0 진행률            │
│  + IsMaxLevel: bool                                          │
│  + TotalExpEarned: int                                       │
│                                                              │
│  [이벤트]                                                     │
│  + OnExpGained: Action<ExpGainInfo>      // XP 획득 정보      │
│  + OnLevelUp: Action<LevelUpInfo>        // 레벨업 정보       │
│  + OnUnlockAcquired: Action<UnlockInfo>  // 해금 발생         │
│  + OnMilestoneComplete: Action<MilestoneData> // 마일스톤 달성│
│                                                              │
│  [메서드]                                                     │
│  + Initialize(ProgressionData data): void                    │
│  + AddExp(int amount, XPSource source): void                 │
│  + GetExpForSource(XPSource source, object context): int     │
│  + IsUnlocked(UnlockType type, string itemId): bool          │
│  + GetUnlockedItems(UnlockType type): string[]               │
│  + CheckMilestones(): void                                   │
│  + GetSaveData(): ProgressionSaveData                        │
│  + LoadSaveData(ProgressionSaveData data): void              │
│  - ProcessLevelUp(): void                                    │
│  - GrantUnlocks(int newLevel): void                          │
│  - CalculateHarvestExp(CropData crop, CropQuality q): int   │
│                                                              │
│  [구독]                                                       │
│  + OnEnable():                                               │
│      FarmEvents.OnCropHarvested += HandleCropHarvested       │
│      BuildingManager.OnBuildingConstructed += HandleBuilding  │
│      ToolSystem.OnToolUsed += HandleToolUse                  │
│      QuestEvents.OnQuestRewarded += HandleQuestXP            │
│      AchievementEvents.OnAchievementUnlocked += HandleAchievementXP │
│  + OnDisable(): 구독 해제                                      │
└──────────────────────────────────────────────────────────────┘
         │ owns                        │ ref
         ▼                             ▼
┌────────────────────────┐   ┌──────────────────────────────┐
│   UnlockRegistry       │   │  ProgressionData (SO)        │
│   (Plain C# class)     │   │──────────────────────────────│
│                        │   │  (→ see 섹션 2 for 필드)      │
│  (아래 섹션 3 참조)     │   │                              │
└────────────────────────┘   └──────────────────────────────┘
         │
         ▼
┌────────────────────────┐
│  MilestoneTracker      │
│  (Plain C# class)      │
│                        │
│  (아래 섹션 4 참조)     │
└────────────────────────┘
```

### 1.2 매니저 간 인터페이스

ProgressionManager는 다른 시스템과 이벤트 기반으로 통신한다. 의존성 방향은 `docs/systems/project-structure.md` 섹션 3의 매트릭스를 따른다.

```
[이벤트 수신 — ProgressionManager가 구독하는 이벤트]

FarmEvents.OnCropHarvested ──▶ ProgressionManager.HandleCropHarvested()
    → 작물 수확 시 XP 계산 및 부여
    → 수확 경험치 공식 (→ see docs/pipeline/data-pipeline.md 섹션 2.6)

BuildingManager.OnBuildingConstructed ──▶ ProgressionManager.HandleBuilding()
    → 시설 건설 시 XP 부여

ToolSystem.OnToolUsed ──▶ ProgressionManager.HandleToolUse()
    → 도구 사용 시 소량 XP 부여

TimeManager.OnDayChanged ──▶ ProgressionManager.OnDayChanged()
    → 일일 마일스톤 체크 트리거

QuestEvents.OnQuestRewarded ──▶ ProgressionManager.HandleQuestXP()
    → 퀘스트 완료 보상 XP 부여 (AddExp(amount, XPSource.QuestComplete))
    → XP 수치 → see docs/systems/quest-system.md 섹션 3~6

AchievementEvents.OnAchievementUnlocked ──▶ ProgressionManager.HandleAchievementXP()
    → 업적 달성 보상 XP 부여 (AddExp(amount, XPSource.AchievementReward))
    → XP 수치 → see docs/content/achievements.md


[이벤트 발행 — ProgressionManager가 발행하는 이벤트]

ProgressionManager.OnExpGained ──▶ UI (경험치 바 갱신, 획득 팝업)
ProgressionManager.OnLevelUp ──▶ UI (레벨업 연출)
                              ──▶ ShopSystem (상점 품목 갱신)
                              ──▶ FarmGrid (확장 가능 여부 갱신)
ProgressionManager.OnUnlockAcquired ──▶ UI (해금 알림)
                                    ──▶ ShopSystem (해금 아이템 판매 가능)
ProgressionManager.OnMilestoneComplete ──▶ UI (마일스톤 달성 연출)
                                       ──▶ EconomyManager (보상 골드 지급)
```

### 1.3 이벤트 데이터 구조

```csharp
// illustrative
namespace SeedMind.Level
{
    // XP 획득 정보
    public struct ExpGainInfo
    {
        public int amount;           // 획득 XP
        public XPSource source;      // XP 출처
        public int totalExp;         // 획득 후 총 EXP
        public int currentLevel;     // 현재 레벨
    }

    // 레벨업 정보
    public struct LevelUpInfo
    {
        public int previousLevel;
        public int newLevel;
        public UnlockInfo[] newUnlocks;  // 이번 레벨에서 해금된 항목들
    }

    // 해금 정보
    public struct UnlockInfo
    {
        public UnlockType type;
        public string itemId;        // 해금된 항목의 dataId
        public string displayName;   // UI 표시용 이름
    }
}
```

### 1.4 이벤트 처리 우선순위

TimeManager의 우선순위 기반 이벤트 시스템(→ see `docs/systems/time-season-architecture.md`)과 호환:

| 이벤트 | 우선순위 | 근거 |
|--------|---------|------|
| OnDayChanged 구독 | 60 | 성장 처리(50) 후, 경제(40) 전에 마일스톤 체크 |

---

## 2. XP/레벨 데이터 구조

### 2.1 ProgressionData ScriptableObject

기존 `docs/pipeline/data-pipeline.md` 섹션 2.6의 LevelConfig를 확장하여 ProgressionData로 재명명한다. LevelConfig의 기존 필드를 포함하면서 해금 목록과 마일스톤 정의를 추가한다.

```csharp
// illustrative
namespace SeedMind.Level.Data
{
    [CreateAssetMenu(menuName = "SeedMind/ProgressionData")]
    public class ProgressionData : ScriptableObject
    {
        [Header("레벨 설정")]
        public int maxLevel;                    // → see docs/pipeline/data-pipeline.md 섹션 2.6
        public int[] expPerLevel;               // → see docs/balance/progression-curve.md

        [Header("경험치 획득 설정")]
        public int harvestExpBase;              // → see docs/pipeline/data-pipeline.md 섹션 2.6
        public float harvestExpPerGrowthDay;    // → see docs/pipeline/data-pipeline.md 섹션 2.6
        public float[] qualityExpBonus;         // → see docs/pipeline/data-pipeline.md 섹션 2.6

        [Header("비수확 XP 소스")]
        public int buildingConstructExp;        // 시설 건설 시 XP (→ see docs/balance/progression-curve.md)
        public int toolUseExp;                  // 도구 사용 시 XP (→ see docs/balance/progression-curve.md)
        public int facilityProcessExp;          // 가공 완료 시 XP (→ see docs/balance/progression-curve.md)

        [Header("해금 테이블")]
        public LevelUnlockEntry[] unlockTable;  // 레벨별 해금 목록

        [Header("마일스톤")]
        public MilestoneData[] milestones;      // 전체 마일스톤 목록
    }
}
```

**LevelConfig와의 관계**: ProgressionData는 LevelConfig의 상위 호환이다. `data-pipeline.md`에서 정의한 LevelConfig 필드(maxLevel, expPerLevel, harvestExpBase, harvestExpPerGrowthDay, qualityExpBonus)를 모두 포함하며, 해금/마일스톤 데이터를 추가한다. SO 에셋 파일명은 `SO_ProgressionData.asset`으로, 기존 `SO_LevelConfig.asset`을 대체한다.

[RISK] data-pipeline.md의 LevelConfig → ProgressionData 이름 변경 시, 해당 문서와 architecture.md의 참조도 업데이트 필요. 다음 `/review` 시 반영.

### 2.2 XPSource enum

```csharp
// illustrative
namespace SeedMind.Level
{
    public enum XPSource
    {
        CropHarvest,        // 작물 수확
        ToolUse,            // 도구 사용 (경작, 물주기 등)
        FacilityBuild,      // 시설 건설
        FacilityProcess,    // 가공 완료
        MilestoneReward,    // 마일스톤 달성 보너스
        QuestComplete,      // 퀘스트 완료 보상 (→ see docs/systems/quest-system.md 섹션 7)
        AchievementReward,  // 업적 달성 보상 (→ see docs/content/achievements.md 섹션 2.4)
        ToolUpgrade,        // 도구 업그레이드 완료 (→ see docs/balance/progression-curve.md)
    }
}
```

### 2.3 XP 계산 로직

ProgressionManager 내부에서 XPSource별 경험치를 계산하는 메서드:

```csharp
// illustrative
namespace SeedMind.Level
{
    public partial class ProgressionManager
    {
        public int GetExpForSource(XPSource source, object context)
        {
            switch (source)
            {
                case XPSource.CropHarvest:
                    var (crop, quality) = ((CropData, CropQuality))context;
                    return CalculateHarvestExp(crop, quality);

                case XPSource.ToolUse:
                    return _progressionData.toolUseExp;
                    // → see docs/balance/progression-curve.md for value

                case XPSource.FacilityBuild:
                    return _progressionData.buildingConstructExp;
                    // → see docs/balance/progression-curve.md for value

                case XPSource.FacilityProcess:
                    return _progressionData.facilityProcessExp;
                    // → see docs/balance/progression-curve.md for value

                case XPSource.MilestoneReward:
                    var milestone = (MilestoneData)context;
                    return milestone.expReward;
                    // → see docs/balance/progression-curve.md for value

                case XPSource.QuestComplete:
                    // 퀘스트 완료 XP는 QuestRewarder가 직접 amount를 계산하여 전달
                    // → see docs/systems/quest-system.md 섹션 3~6 (canonical XP 수치)
                    return (int)context;  // context = 퀘스트 보상 XP (int)

                case XPSource.AchievementReward:
                    // 업적 보상 XP는 AchievementManager가 직접 amount를 계산하여 전달
                    // → see docs/content/achievements.md (canonical XP 수치)
                    return (int)context;  // context = 업적 보상 XP (int)

                case XPSource.ToolUpgrade:
                    // 도구 업그레이드 완료 시 BlacksmithEvents.OnUpgradeCompleted에서 전달
                    // → see docs/balance/progression-curve.md (canonical XP 수치)
                    return _progressionData.toolUpgradeExp;

                default:
                    return 0;
            }
        }

        private int CalculateHarvestExp(CropData crop, CropQuality quality)
        {
            // 공식: (→ see docs/pipeline/data-pipeline.md 섹션 2.6)
            // 수확_경험치 = (harvestExpBase + floor(growthDays * harvestExpPerGrowthDay)) * qualityExpBonus[quality]
            // [주의] 이 공식은 기준선이며, 최종 작물별 XP는 CropData.harvestExp 필드로 오버라이드 가능하다.
            // 실제 확정 XP 수치는 → see docs/balance/progression-curve.md 섹션 1.2.1 (수동 조정값 우선)
            int baseExp = crop.harvestExp > 0
                        ? crop.harvestExp  // CropData에 명시적 XP가 있으면 우선 사용
                        : _progressionData.harvestExpBase
                          + Mathf.FloorToInt(crop.growthDays * _progressionData.harvestExpPerGrowthDay);
            float qualityMult = _progressionData.qualityExpBonus[(int)quality];
            return Mathf.RoundToInt(baseExp * qualityMult);
        }
    }
}
```

### 2.4 AddExp / ProcessLevelUp 흐름

```
AddExp(amount, source)
│
├── _currentExp += amount
├── _totalExpEarned += amount
├── OnExpGained.Invoke(ExpGainInfo)
│
├── while (_currentExp >= expPerLevel[_currentLevel] && !IsMaxLevel)
│   │
│   ├── _currentExp -= expPerLevel[_currentLevel]
│   ├── _currentLevel++
│   ├── GrantUnlocks(_currentLevel)
│   └── OnLevelUp.Invoke(LevelUpInfo)
│
└── CheckMilestones()
```

**다중 레벨업 처리**: 한 번의 XP 획득으로 여러 레벨이 오를 수 있다. while 루프로 처리하며, 각 레벨업마다 개별 이벤트를 발행한다.

---

## 3. 해금(Unlock) 시스템

### 3.1 UnlockType enum

```csharp
// illustrative
namespace SeedMind.Level
{
    public enum UnlockType
    {
        Crop,           // 작물 (CropData.unlockLevel)
        Facility,       // 시설 (BuildingData.requiredLevel)
        Fertilizer,     // 비료 (FertilizerData.unlockLevel)
        Tool,           // 도구 등급 (ToolData.tier)
        Recipe,         // 가공 레시피
        FarmExpansion,  // 농장 확장 단계
    }
}
```

### 3.2 LevelUnlockEntry 데이터 구조

```csharp
// illustrative
namespace SeedMind.Level.Data
{
    [System.Serializable]
    public class LevelUnlockEntry
    {
        public int level;                  // 해금 레벨
        public UnlockItemEntry[] items;    // 이 레벨에서 해금되는 항목들
    }

    [System.Serializable]
    public class UnlockItemEntry
    {
        public UnlockType type;            // 해금 유형
        public string itemId;              // 해금 대상의 dataId (SO 참조)
        public string displayName;         // UI 표시용 이름
    }
}
```

**해금 테이블 데이터**: 각 레벨에서 해금되는 항목 목록은 ProgressionData SO의 unlockTable 필드에 데이터 드리븐으로 설정된다. 구체적인 해금 내용(어떤 레벨에서 어떤 작물/시설이 해금되는지)은 (→ see `docs/design.md` 섹션 4.2, 4.5, 4.6)에서 정의된다.

### 3.3 UnlockRegistry 클래스

```csharp
// illustrative
namespace SeedMind.Level
{
    /// <summary>
    /// 런타임에 해금 상태를 관리하는 내부 클래스.
    /// ProgressionManager가 소유하며, 외부에서 직접 접근하지 않는다.
    /// </summary>
    public class UnlockRegistry
    {
        // 해금된 항목을 UnlockType별로 관리
        private Dictionary<UnlockType, HashSet<string>> _unlockedItems;

        // 초기화 — 레벨 1 기본 해금 적용
        public void Initialize(LevelUnlockEntry[] unlockTable, int currentLevel)
        {
            _unlockedItems = new Dictionary<UnlockType, HashSet<string>>();
            foreach (UnlockType type in Enum.GetValues(typeof(UnlockType)))
                _unlockedItems[type] = new HashSet<string>();

            // 현재 레벨까지의 모든 해금 적용
            for (int lvl = 1; lvl <= currentLevel; lvl++)
            {
                var entry = FindEntry(unlockTable, lvl);
                if (entry != null)
                    foreach (var item in entry.items)
                        _unlockedItems[item.type].Add(item.itemId);
            }
        }

        public bool IsUnlocked(UnlockType type, string itemId)
            => _unlockedItems.ContainsKey(type) && _unlockedItems[type].Contains(itemId);

        public string[] GetUnlockedItems(UnlockType type)
            => _unlockedItems.ContainsKey(type)
               ? _unlockedItems[type].ToArray()
               : Array.Empty<string>();

        // 레벨업 시 새 해금 적용, 새로 해금된 항목 반환
        public UnlockInfo[] ApplyLevelUnlocks(LevelUnlockEntry[] unlockTable, int newLevel)
        {
            var entry = FindEntry(unlockTable, newLevel);
            if (entry == null) return Array.Empty<UnlockInfo>();

            var newUnlocks = new List<UnlockInfo>();
            foreach (var item in entry.items)
            {
                if (_unlockedItems[item.type].Add(item.itemId))
                {
                    newUnlocks.Add(new UnlockInfo
                    {
                        type = item.type,
                        itemId = item.itemId,
                        displayName = item.displayName
                    });
                }
            }
            return newUnlocks.ToArray();
        }

        // 세이브/로드 지원
        public UnlockSaveData GetSaveData() { /* ... */ }
        public void LoadSaveData(UnlockSaveData data) { /* ... */ }
    }
}
```

### 3.4 상점/UI와의 연동

해금 시스템은 다른 시스템에 해금 여부 판정을 제공한다:

```
[상점 연동]
ShopSystem.RefreshInventory()
    → foreach item in shopData.availableItems:
        if ProgressionManager.IsUnlocked(item.unlockType, item.itemId):
            → 구매 가능으로 표시
        else:
            → 회색 처리 + "레벨 N에서 해금" 표시

[건설 연동]
BuildingManager.CanBuild(BuildingData building)
    → ProgressionManager.IsUnlocked(UnlockType.Facility, building.buildingId)
    → && EconomyManager.CanAfford(building.buildCost)

[씨앗 심기 연동]
FarmTile.CanPlant(CropData crop)
    → ProgressionManager.IsUnlocked(UnlockType.Crop, crop.cropId)
```

### 3.5 세이브/로드 호환

해금 데이터는 `docs/pipeline/data-pipeline.md` 섹션 Part II 2.6에 정의된 UnlockSaveData 형식을 그대로 사용한다:

```csharp
// illustrative — (→ see docs/pipeline/data-pipeline.md Part II 섹션 2.6 for canonical 정의)
namespace SeedMind.Core
{
    [System.Serializable]
    public class UnlockSaveData
    {
        public string[] unlockedCrops;
        public string[] unlockedBuildings;
        public string[] unlockedRecipes;
        // 확장 필드 (ProgressionData 도입 시 추가)
        public string[] unlockedFertilizers;
        public string[] unlockedTools;
        public string[] unlockedFarmExpansions;
    }
}
```

[RISK] UnlockSaveData에 unlockedFertilizers, unlockedTools, unlockedFarmExpansions 필드 추가가 필요하다. 기존 data-pipeline.md의 UnlockSaveData 정의와 세이브 호환성을 위해, 추가 필드는 null 허용으로 설계하여 이전 세이브 파일 로드 시 빈 배열로 초기화한다.

[OPEN] 해금 데이터를 별도 저장 vs 레벨 역산 -- `docs/pipeline/data-pipeline.md`에서도 동일한 OPEN 이슈가 제기되어 있다. 현재 설계는 별도 저장 방식을 채택한다. 이유: 마일스톤 보상이나 이벤트 등 레벨 외 해금 경로가 추가될 가능성이 있으며, 별도 저장이 확장에 안전하다.

---

## 4. 마일스톤 시스템

### 4.1 MilestoneData ScriptableObject

마일스톤은 레벨과 독립적인 달성 목표를 정의한다. ProgressionData SO의 milestones 배열에 포함된다.

```csharp
// illustrative
namespace SeedMind.Level.Data
{
    [System.Serializable]
    public class MilestoneData
    {
        public string milestoneId;             // 고유 식별자
        public string displayName;             // UI 표시 이름
        public string description;             // 달성 조건 설명 텍스트
        public MilestoneConditionType conditionType;  // 조건 유형
        public string conditionParam;          // 조건 파라미터 (아이템 ID 등)
        public int conditionValue;             // 조건 수치 (달성 목표값)
        public MilestoneReward reward;         // 보상 정의
        public Sprite icon;                    // UI 아이콘
        public bool isHidden;                  // 달성 전 숨김 여부
    }

    [System.Serializable]
    public class MilestoneReward
    {
        public int goldReward;                 // 골드 보상 (→ see docs/balance/progression-curve.md)
        public int expReward;                  // EXP 보상 (→ see docs/balance/progression-curve.md)
        public UnlockItemEntry[] unlockRewards; // 해금 보상 (레벨 외 해금 경로)
    }
}
```

### 4.2 MilestoneConditionType enum

```csharp
// illustrative
namespace SeedMind.Level
{
    public enum MilestoneConditionType
    {
        LevelReached,        // 특정 레벨 도달
        FirstHarvest,        // 특정 작물 첫 수확 (conditionParam = cropId)
        TotalHarvest,        // 총 수확 횟수 (conditionValue = 목표 횟수)
        CropHarvestCount,    // 특정 작물 수확 횟수 (conditionParam = cropId)
        GoldOwned,           // 골드 보유량 도달 (conditionValue = 목표 골드)
        GoldEarned,          // 누적 골드 수입 (conditionValue = 목표 골드)
        FacilityBuilt,       // 특정 시설 건설 (conditionParam = buildingId)
        TotalFacilities,     // 총 시설 수 (conditionValue = 목표 수)
        QualityHarvest,      // 특정 품질 이상 수확 (conditionParam = 품질, conditionValue = 횟수)
        DaysPlayed,          // 경과 일수 (conditionValue = 목표 일수)
    }
}
```

### 4.3 MilestoneTracker 클래스

```csharp
// illustrative
namespace SeedMind.Level
{
    /// <summary>
    /// 마일스톤 진행 상황을 추적하고 달성 여부를 판정하는 내부 클래스.
    /// ProgressionManager가 소유한다.
    /// </summary>
    public class MilestoneTracker
    {
        // 마일스톤별 현재 진행 수치
        private Dictionary<string, int> _milestoneProgress;
        // 완료된 마일스톤 ID 집합
        private HashSet<string> _completedMilestones;
        // 전체 마일스톤 정의 참조
        private MilestoneData[] _allMilestones;

        public void Initialize(MilestoneData[] milestones)
        {
            _allMilestones = milestones;
            _milestoneProgress = new Dictionary<string, int>();
            _completedMilestones = new HashSet<string>();
        }

        /// <summary>
        /// 특정 조건 유형의 진행값을 갱신한다.
        /// 예: OnCropHarvested 시 TotalHarvest, CropHarvestCount 등을 업데이트.
        /// </summary>
        public void UpdateProgress(MilestoneConditionType type, string param, int value)
        {
            foreach (var ms in _allMilestones)
            {
                if (_completedMilestones.Contains(ms.milestoneId)) continue;
                if (ms.conditionType != type) continue;
                if (!string.IsNullOrEmpty(ms.conditionParam) && ms.conditionParam != param) continue;

                // 누적형 진행
                if (!_milestoneProgress.ContainsKey(ms.milestoneId))
                    _milestoneProgress[ms.milestoneId] = 0;
                _milestoneProgress[ms.milestoneId] += value;
            }
        }

        /// <summary>
        /// 스냅샷형 진행값을 설정한다 (GoldOwned처럼 현재 값 기준 판정).
        /// </summary>
        public void SetProgress(MilestoneConditionType type, string param, int currentValue)
        {
            foreach (var ms in _allMilestones)
            {
                if (_completedMilestones.Contains(ms.milestoneId)) continue;
                if (ms.conditionType != type) continue;
                _milestoneProgress[ms.milestoneId] = currentValue;
            }
        }

        /// <summary>
        /// 완료 가능한 마일스톤을 검사하여 완료 목록을 반환한다.
        /// </summary>
        public MilestoneData[] CheckCompletions()
        {
            var completions = new List<MilestoneData>();
            foreach (var ms in _allMilestones)
            {
                if (_completedMilestones.Contains(ms.milestoneId)) continue;
                if (_milestoneProgress.TryGetValue(ms.milestoneId, out int progress)
                    && progress >= ms.conditionValue)
                {
                    _completedMilestones.Add(ms.milestoneId);
                    completions.Add(ms);
                }
            }
            return completions.ToArray();
        }

        // 세이브/로드
        public MilestoneSaveData GetSaveData() { /* ... */ }
        public void LoadSaveData(MilestoneSaveData data) { /* ... */ }
    }
}
```

### 4.4 마일스톤 세이브 데이터

```csharp
// illustrative
namespace SeedMind.Level
{
    [System.Serializable]
    public class MilestoneSaveData
    {
        public string[] completedMilestoneIds;            // 완료된 마일스톤 ID
        public MilestoneProgressEntry[] progressEntries;  // 진행 중인 마일스톤 수치
    }

    [System.Serializable]
    public class MilestoneProgressEntry
    {
        public string milestoneId;
        public int currentValue;
    }
}
```

**GameSaveData 통합**: MilestoneSaveData는 기존 `docs/pipeline/data-pipeline.md` Part II 섹션 2.1의 GameSaveData에 새 필드로 추가한다:

```csharp
// illustrative — GameSaveData 확장 (→ see docs/pipeline/data-pipeline.md Part II 섹션 2.1)
public class GameSaveData
{
    // ... 기존 필드 ...
    public UnlockSaveData unlocks;           // 기존
    public MilestoneSaveData milestones;     // 신규 추가
}
```

[RISK] GameSaveData에 milestones 필드 추가 시 이전 세이브 파일과의 호환성 처리 필요. null 체크로 대응 가능하지만, data-pipeline.md의 세이브 버전 마이그레이션 전략(현재 OPEN)이 확정되면 해당 전략을 따라야 한다.

### 4.5 마일스톤 예시 (데이터만, 수치는 참조)

| 마일스톤 ID | 조건 | 보상 | 비고 |
|-------------|------|------|------|
| `ms_first_harvest` | FirstHarvest, any | 골드 + EXP (→ see docs/balance/progression-curve.md) | 첫 수확 축하 |
| `ms_crop_variety_5` | TotalHarvest, 5종 | 골드 (→ see docs/balance/progression-curve.md) | 다양한 작물 도전 |
| `ms_gold_1000` | GoldOwned, 1000 | EXP (→ see docs/balance/progression-curve.md) | 부농의 길 |
| `ms_first_building` | FacilityBuilt, any | EXP (→ see docs/balance/progression-curve.md) | 첫 시설 건설 |
| `ms_quality_gold_10` | QualityHarvest, Gold, 10 | 해금 보상 (→ see docs/balance/progression-curve.md) | 품질 장인 |

---

## 5. 세이브/로드 통합 설계

### 5.1 ProgressionSaveData

ProgressionManager의 전체 상태를 저장하는 통합 세이브 데이터. 기존 PlayerSaveData의 level/currentExp 필드와의 관계를 정리한다.

```csharp
// illustrative
namespace SeedMind.Level
{
    [System.Serializable]
    public class ProgressionSaveData
    {
        // 레벨/경험치 (PlayerSaveData.level, PlayerSaveData.currentExp와 동기화)
        public int level;
        public int currentExp;
        public int totalExpEarned;

        // 마일스톤 진행
        public MilestoneSaveData milestones;
    }
}
```

**PlayerSaveData와의 관계**: 기존 `docs/pipeline/data-pipeline.md`의 PlayerSaveData에는 이미 level과 currentExp 필드가 있다. 중복 저장을 방지하기 위해 두 가지 전략 중 하나를 선택해야 한다:

**채택 방안 -- PlayerSaveData 유지, ProgressionSaveData는 추가 데이터만 저장**:
- PlayerSaveData.level, PlayerSaveData.currentExp는 그대로 유지 (기존 세이브 호환)
- ProgressionSaveData에는 totalExpEarned + MilestoneSaveData만 저장
- 로드 시 ProgressionManager는 PlayerSaveData에서 level/exp를 읽고, GameSaveData.milestones에서 마일스톤을 읽는다

```
[저장 흐름]
SaveManager.Save()
    → PlayerSaveData.level = ProgressionManager.CurrentLevel
    → PlayerSaveData.currentExp = ProgressionManager.CurrentExp
    → GameSaveData.unlocks = ProgressionManager.UnlockRegistry.GetSaveData()
    → GameSaveData.milestones = ProgressionManager.MilestoneTracker.GetSaveData()
    → GameSaveData.milestones.totalExpEarned = ProgressionManager.TotalExpEarned

[로드 흐름]
SaveManager.Load()
    → ProgressionManager.LoadSaveData(
          level: playerSave.level,
          currentExp: playerSave.currentExp,
          totalExpEarned: milestoneSave?.totalExpEarned ?? 0,
          unlocks: unlockSave,
          milestones: milestoneSave
      )
    → UnlockRegistry.Initialize(unlockTable, level)
    → UnlockRegistry.LoadSaveData(unlockSave)  // 레벨 외 해금 오버라이드
    → MilestoneTracker.LoadSaveData(milestoneSave)
```

---

## 6. 프로젝트 구조 내 배치

### 6.1 폴더/파일 배치

(→ see `docs/systems/project-structure.md` 섹션 1, 2 for 기존 구조)

```
Assets/_Project/Scripts/Level/
├── ProgressionManager.cs          // SeedMind.Level
├── UnlockRegistry.cs              // SeedMind.Level
├── MilestoneTracker.cs            // SeedMind.Level
├── XPSource.cs                    // SeedMind.Level (enum)
├── UnlockType.cs                  // SeedMind.Level (enum)
├── MilestoneConditionType.cs      // SeedMind.Level (enum)
├── ProgressionEvents.cs           // SeedMind.Level (ExpGainInfo, LevelUpInfo, UnlockInfo)
└── Data/
    ├── ProgressionData.cs         // SeedMind.Level.Data (ScriptableObject)
    ├── LevelUnlockEntry.cs        // SeedMind.Level.Data
    ├── MilestoneData.cs           // SeedMind.Level.Data
    └── MilestoneReward.cs         // SeedMind.Level.Data
```

### 6.2 Assembly Definition 의존성

기존 `SeedMind.Level.asmdef`의 참조를 확인한다 (→ see `docs/systems/project-structure.md` 섹션 4):

| asmdef | 참조 | 비고 |
|--------|------|------|
| `SeedMind.Level.asmdef` | Core, Farm | 기존 설계 그대로 |

ProgressionManager가 EconomyManager(골드 보상 지급)를 호출해야 하는 경우가 있으나, 직접 참조 대신 이벤트로 통신한다. OnMilestoneComplete 이벤트를 EconomyManager가 구독하여 골드를 지급하는 방식으로, Level → Economy 직접 의존을 피한다.

[RISK] ProgressionManager가 BuildingManager.OnBuildingConstructed를 구독하려면 Building 모듈 참조가 필요해 보이지만, 이벤트를 Core의 EventBus로 중계하면 직접 참조를 피할 수 있다. 의존성 매트릭스에서 Level → Building 참조는 금지되어 있으므로(→ see `docs/systems/project-structure.md` 섹션 3.2), 반드시 EventBus 경유 방식을 사용해야 한다.

### 6.3 씬 계층 구조 내 배치

SCN_Farm 씬의 MANAGERS 그룹에 ProgressionManager를 추가한다:

```
--- MANAGERS ---
├── GameManager          (DontDestroyOnLoad)
├── TimeManager          (DontDestroyOnLoad)
├── SaveManager          (DontDestroyOnLoad)
└── ProgressionManager   (DontDestroyOnLoad)    ← 신규
```

ProgressionManager는 DontDestroyOnLoad로 설정하여 씬 전환 시에도 레벨/해금 상태를 유지한다.

---

# Part II -- MCP 구현 계획

---

## 7. Unity 씬 배치/초기화 MCP 태스크 시퀀스

### 7.1 ProgressionManager 오브젝트 생성

```
Step 1: SCN_Farm 씬 열기
        MCP: open_scene("Assets/_Project/Scenes/Main/SCN_Farm.unity")

Step 2: ProgressionManager GameObject 생성
        MCP: create_gameobject("ProgressionManager")
        → parent: "--- MANAGERS ---" 하위
        → position: (0, 0, 0)

Step 3: ProgressionManager.cs 컴포넌트 추가
        MCP: add_component("ProgressionManager", "SeedMind.Level.ProgressionManager")

Step 4: DontDestroyOnLoad 확인
        → Singleton<T> 베이스 클래스 사용으로 자동 처리
        (→ see docs/systems/project-structure.md 섹션 1, Core/Singleton.cs)

Step 5: ProgressionData SO 참조 연결
        MCP: set_component_property(
            "ProgressionManager",
            "SeedMind.Level.ProgressionManager",
            "_progressionData",
            "Assets/_Project/Data/Config/SO_ProgressionData.asset"
        )
```

[RISK] MCP의 ScriptableObject 참조 필드 설정 지원 범위가 불확실하다 (→ see `docs/architecture.md` Risks). Step 5에서 SO 참조 할당이 MCP로 불가능할 경우, 런타임에 Resources.Load 또는 SORegistry를 통한 참조 해소로 우회해야 한다.

### 7.2 UI 요소 갱신

기존 씬 계층의 Canvas_HUD에 이미 LevelBar가 배치되어 있다 (→ see `docs/systems/project-structure.md` 섹션 5.4):

```
Canvas_HUD
├── TimeDisplay
├── GoldDisplay
├── ToolBar
└── LevelBar         ← 이미 계획됨, ProgressionManager 이벤트에 연결
```

```
Step 6: LevelBar UI 오브젝트 확인 (기존)
        MCP: find_gameobject("Canvas_HUD/LevelBar")

Step 7: LevelBarUI.cs 컴포넌트 추가
        MCP: add_component("Canvas_HUD/LevelBar", "SeedMind.UI.LevelBarUI")

Step 8: LevelBarUI 프로퍼티 설정
        → ProgressionManager 참조는 Singleton 패턴으로 런타임 접근
        → UI 요소(Slider, Text)는 Inspector에서 수동 연결 또는 GetComponent 자동 바인딩
```

---

## 8. SO 에셋 생성 및 JSON 스키마

### 8.1 ProgressionData SO 에셋 생성 MCP 절차

```
Step 9: ProgressionData SO 에셋 생성
        MCP: create_scriptableobject_asset(
            "SeedMind.Level.Data.ProgressionData",
            "Assets/_Project/Data/Config/SO_ProgressionData.asset"
        )

Step 10: 기본 필드 설정 (모든 수치는 → see docs/balance/progression-curve.md)
         MCP: set_so_field("SO_ProgressionData", "maxLevel", 10)
              // → see docs/pipeline/data-pipeline.md 섹션 2.6
         MCP: set_so_field("SO_ProgressionData", "harvestExpBase", 5)
              // → see docs/pipeline/data-pipeline.md 섹션 2.6
         MCP: set_so_field("SO_ProgressionData", "harvestExpPerGrowthDay", 1.0)
              // → see docs/pipeline/data-pipeline.md 섹션 2.6

Step 11: expPerLevel 배열 설정
         MCP: set_so_field("SO_ProgressionData", "expPerLevel",
              [80, 128, 205, 328, 524, 839, 1342, 2147, 3436])
              // → see docs/balance/progression-curve.md 섹션 2.4.1 for canonical values
              // 9개 요소: 레벨 1→2 부터 레벨 9→10 까지 구간 경험치 (baseXP=80, growthFactor=1.60)

Step 12: qualityExpBonus 배열 설정
         MCP: set_so_field("SO_ProgressionData", "qualityExpBonus",
              [1.0, 1.2, 1.5, 2.0])
              // → see docs/pipeline/data-pipeline.md 섹션 2.6
              // 인덱스: Normal=0, Silver=1, Gold=2, Iridium=3
```

[RISK] MCP에서 배열 필드(expPerLevel, qualityExpBonus) 설정이 지원되는지 사전 검증 필요. 지원되지 않을 경우, JSON Import 파이프라인으로 우회한다 (→ see 섹션 8.2).

### 8.2 JSON 스키마 (data-pipeline.md 형식과 일관)

ProgressionData SO를 JSON Import 파이프라인으로 생성/갱신할 경우를 위한 스키마 정의:

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "ProgressionData",
  "description": "플레이어 진행 시스템 설정 (SO_ProgressionData.asset)",
  "type": "object",
  "required": ["maxLevel", "expPerLevel", "harvestExpBase"],
  "properties": {
    "maxLevel": {
      "type": "integer",
      "minimum": 1,
      "maximum": 100,
      "description": "최대 레벨 (→ see docs/pipeline/data-pipeline.md 섹션 2.6)"
    },
    "expPerLevel": {
      "type": "array",
      "items": { "type": "integer", "minimum": 1 },
      "minItems": 1,
      "description": "레벨별 구간 경험치 배열. 길이 = maxLevel - 1 (→ see docs/balance/progression-curve.md)"
    },
    "harvestExpBase": {
      "type": "integer",
      "minimum": 0,
      "description": "작물 수확 시 기본 경험치 (→ see docs/pipeline/data-pipeline.md 섹션 2.6)"
    },
    "harvestExpPerGrowthDay": {
      "type": "number",
      "minimum": 0.0,
      "description": "성장일수당 추가 경험치 배수 (→ see docs/pipeline/data-pipeline.md 섹션 2.6)"
    },
    "qualityExpBonus": {
      "type": "array",
      "items": { "type": "number", "minimum": 0.0 },
      "minItems": 4,
      "maxItems": 4,
      "description": "품질별 경험치 보너스 배수 [Normal, Silver, Gold, Iridium] (→ see docs/pipeline/data-pipeline.md 섹션 2.6)"
    },
    "buildingConstructExp": {
      "type": "integer",
      "minimum": 0,
      "description": "시설 건설 시 XP (→ see docs/balance/progression-curve.md)"
    },
    "toolUseExp": {
      "type": "integer",
      "minimum": 0,
      "description": "도구 사용 시 XP (→ see docs/balance/progression-curve.md)"
    },
    "facilityProcessExp": {
      "type": "integer",
      "minimum": 0,
      "description": "가공 완료 시 XP (→ see docs/balance/progression-curve.md)"
    },
    "unlockTable": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["level", "items"],
        "properties": {
          "level": { "type": "integer", "minimum": 1 },
          "items": {
            "type": "array",
            "items": {
              "type": "object",
              "required": ["type", "itemId", "displayName"],
              "properties": {
                "type": { "type": "string", "enum": ["Crop", "Facility", "Fertilizer", "Tool", "Recipe", "FarmExpansion"] },
                "itemId": { "type": "string" },
                "displayName": { "type": "string" }
              }
            }
          }
        }
      },
      "description": "레벨별 해금 테이블 (→ see docs/design.md 섹션 4.2, 4.5, 4.6)"
    },
    "milestones": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["milestoneId", "conditionType", "conditionValue"],
        "properties": {
          "milestoneId": { "type": "string" },
          "displayName": { "type": "string" },
          "description": { "type": "string" },
          "conditionType": {
            "type": "string",
            "enum": ["LevelReached", "FirstHarvest", "TotalHarvest", "CropHarvestCount", "GoldOwned", "GoldEarned", "FacilityBuilt", "TotalFacilities", "QualityHarvest", "DaysPlayed"]
          },
          "conditionParam": { "type": "string" },
          "conditionValue": { "type": "integer", "minimum": 1 },
          "reward": {
            "type": "object",
            "properties": {
              "goldReward": { "type": "integer", "minimum": 0 },
              "expReward": { "type": "integer", "minimum": 0 },
              "unlockRewards": { "type": "array" }
            }
          },
          "isHidden": { "type": "boolean", "default": false }
        }
      },
      "description": "마일스톤 목록 (→ see docs/balance/progression-curve.md)"
    }
  }
}
```

### 8.3 SO 에셋 파일 경로

(→ see `docs/systems/project-structure.md` 섹션 6.1 for 네이밍 규칙)

```
Assets/_Project/Data/Config/
└── SO_ProgressionData.asset       ← SO_LevelConfig.asset 대체
```

### 8.4 해금 테이블 MCP 설정 태스크

```
Step 13: unlockTable 설정 (데이터 → see docs/design.md 섹션 4.2, 4.5, 4.6)
         → 레벨 1: 감자(potato), 당근(carrot) — 시작 해금
         → 레벨 2: 토마토(tomato)
         → 레벨 3: 옥수수(corn), 물탱크(water_tank), 고급 비료
         → 레벨 4: 딸기(strawberry), 창고(storage), 속성장 비료
         → 레벨 5: 호박(pumpkin), 해바라기(sunflower), 온실(greenhouse)
         → 레벨 6: 유기 비료
         → 레벨 7: 수박(watermelon), 가공소(processor)
         → 레벨 8~10: (→ see docs/pipeline/data-pipeline.md 섹션 2.6 [RISK])

         MCP: JSON Import 방식 권장 (배열/중첩 객체이므로)
```

---

## 9. 전체 MCP 태스크 요약

| Step | 작업 | 의존성 |
|------|------|--------|
| 1 | SCN_Farm 씬 열기 | - |
| 2 | ProgressionManager GameObject 생성 | Step 1 |
| 3 | ProgressionManager.cs 컴포넌트 추가 | Step 2 |
| 4 | DontDestroyOnLoad 확인 (Singleton 자동) | Step 3 |
| 5 | ProgressionData SO 참조 연결 | Step 3, Step 9 |
| 6 | LevelBar UI 오브젝트 확인 | Step 1 |
| 7 | LevelBarUI.cs 컴포넌트 추가 | Step 6 |
| 8 | LevelBarUI 프로퍼티 설정 | Step 7 |
| 9 | ProgressionData SO 에셋 생성 | - |
| 10 | 기본 필드 설정 | Step 9 |
| 11 | expPerLevel 배열 설정 | Step 9 |
| 12 | qualityExpBonus 배열 설정 | Step 9 |
| 13 | unlockTable 설정 (JSON Import) | Step 9 |

**병렬 실행 가능 그룹**:
- Group A: Step 1~8 (씬 작업)
- Group B: Step 9~13 (SO 에셋 작업)
- Group A와 B는 병렬 실행 가능. Step 5만 양쪽 의존.

---

## Cross-references

- `docs/architecture.md` — 마스터 기술 아키텍처 (프로젝트 구조 3절, 매니저 목록)
- `docs/design.md` 섹션 4.2 — 작물 종류/해금 조건 (canonical)
- `docs/design.md` 섹션 4.5 — 레벨/해금 시스템 게임 디자인 (canonical)
- `docs/design.md` 섹션 4.6 — 시설 해금 조건 (canonical)
- `docs/systems/project-structure.md` — 네임스페이스(SeedMind.Level), asmdef, 씬 계층 구조
- `docs/pipeline/data-pipeline.md` 섹션 2.6 — LevelConfig SO 필드 정의 (canonical XP 공식/수치)
- `docs/pipeline/data-pipeline.md` Part II 섹션 2.1~2.6 — 세이브 데이터 구조 (PlayerSaveData, UnlockSaveData)
- `docs/systems/economy-architecture.md` — EconomyManager 이벤트 인터페이스
- `docs/systems/farming-architecture.md` — FarmEvents.OnCropHarvested 이벤트
- `docs/systems/time-season-architecture.md` — TimeManager 이벤트 우선순위 시스템
- `docs/balance/progression-curve.md` — 진행 밸런스 시트: XP 테이블, 해금 타이밍, 마일스톤 보상 수치 (canonical, Designer 동시 작성 중)
- `docs/mcp/scene-setup-tasks.md` — 기본 씬 구성 MCP 태스크 (MANAGERS 계층 참조)

## Open Questions

- [OPEN] ProgressionData로 이름 변경 시 기존 LevelConfig 참조를 일괄 업데이트할지, 호환 래퍼를 유지할지. 현재 data-pipeline.md에 LevelConfig로 정의되어 있으므로 다음 `/review`에서 통일 필요.
- [OPEN] 해금 데이터를 별도 저장 vs 레벨 역산 -- 본 문서에서는 별도 저장으로 채택했으나, data-pipeline.md에서도 동일 OPEN 이슈가 열려 있음. 양쪽 문서의 결론을 통일해야 한다.
- [OPEN] 마일스톤 보상으로 해금을 제공하는 경우, 해당 해금이 레벨 기반 해금 테이블과 중복되면 어떻게 처리할지. 현재 설계에서는 HashSet으로 관리하므로 중복 해금이 무시되지만, UI 표시 측면에서 정리 필요.
- [OPEN] 도구 사용 XP(toolUseExp)가 너무 빈번하게 발생할 수 있음. 일일 상한(daily cap) 도입 여부를 밸런스 테스트 후 결정.

## Risks

- [RISK] **LevelConfig → ProgressionData 마이그레이션**: data-pipeline.md, architecture.md 등 기존 문서에서 LevelConfig로 참조하는 곳을 모두 업데이트해야 한다. 다음 `/review`에서 일괄 처리 필요.
- [RISK] **UnlockSaveData 필드 확장**: 기존 3개 필드(unlockedCrops, unlockedBuildings, unlockedRecipes)에 3개 필드(unlockedFertilizers, unlockedTools, unlockedFarmExpansions)를 추가해야 한다. 이전 세이브 파일 호환성 확인 필요.
- [RISK] **GameSaveData에 milestones 필드 추가**: 세이브 파일 버전 관리 전략이 미확정(data-pipeline.md OPEN) 상태에서 필드 추가. null-safe 처리 필수.
- [RISK] **Level → Building 의존성 위반 방지**: ProgressionManager가 BuildingManager의 이벤트를 구독하려면 EventBus 중계가 필수. 직접 참조 시 asmdef 순환 참조 발생.
- [RISK] **MCP SO 배열/참조 설정**: unlockTable과 같은 중첩 배열 객체의 MCP 설정 가능 여부가 불확실. JSON Import 우회 방안을 기본으로 계획.

---

*이 문서는 Claude Code가 기술적 제약과 설계 목표를 고려하여 자율적으로 작성했습니다.*
