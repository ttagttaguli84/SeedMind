# 도구 업그레이드 시스템 기술 아키텍처

> 도구 업그레이드의 데이터 구조, 런타임 시스템, 효과 계산, 기존 시스템 연동, MCP 구현 계획  
> 작성: Claude Code (Opus) | 2026-04-06  
> 문서 ID: DES-007 (아키텍처 파트)

---

## Context

이 문서는 SeedMind의 도구 업그레이드 시스템에 대한 기술 아키텍처를 정의한다. 플레이어가 소유한 도구(호미, 물뿌리개, 낫)를 대장간에서 재료와 골드를 지불하여 상위 등급으로 업그레이드하는 전체 흐름을 설계한다. 기존 `farming-architecture.md`의 ToolData/ToolSystem, `inventory-architecture.md`의 InventoryManager, `data-pipeline.md`의 세이브 구조와 일관되게 통합한다.

**설계 목표**:
- 기존 ToolData SO 체인(tier 1~3, nextTier 참조)을 그대로 활용하여 업그레이드 데이터를 표현 (→ see docs/systems/tool-upgrade.md 섹션 1.1)
- 업그레이드 처리 로직을 독립 시스템(ToolUpgradeSystem)으로 분리하여, ToolSystem/InventoryManager와 느슨하게 결합
- 레벨별 도구 효과(범위 확대, 물 저장량 등)를 ToolEffectResolver가 일원화하여 계산
- 세이브/로드 시 도구 등급 정보가 누락 없이 복원되어야 한다

---

# Part I -- 데이터 구조 및 시스템 설계

---

## 1. 핵심 설계 결정

### 1.1 도구 등급 표현 방식: SO 체인 (기존 유지)

| 대안 | 장점 | 단점 | 결정 |
|------|------|------|------|
| **A: 등급별 별도 SO + nextTier 참조 (채택)** | 등급별 독립 에셋, 에디터에서 직관적 | 에셋 수 증가 (15~17개) | **채택** |
| B: 단일 SO + 레벨별 배열 | 에셋 수 최소 (3~5개) | SO 내 배열 복잡, 등급별 프리팹/아이콘 관리 어려움 | 기각 |

**근거**: `docs/pipeline/data-pipeline.md` 섹션 2.3에서 등급별 별도 SO(SO_Tool_Hoe_Basic ~ SO_Tool_Hoe_Legendary)와 nextTier 체인이 정의되어 있다. 3단계 체계(Basic/Reinforced/Legendary)는 `docs/systems/tool-upgrade.md` 섹션 1.1이 canonical이다. 이 구조를 계승한다.

### 1.2 업그레이드 처리 주체: 독립 시스템 (ToolUpgradeSystem)

| 대안 | 장점 | 단점 | 결정 |
|------|------|------|------|
| ToolSystem에 업그레이드 로직 내장 | 한 곳에서 관리 | ToolSystem 비대화, SRP 위반 | 기각 |
| **ToolUpgradeSystem 독립 분리 (채택)** | SRP 준수, 대장간 UI와 직접 연동 | 시스템 간 이벤트 연결 필요 | **채택** |

**근거**: ToolSystem은 도구 사용(UseCurrentTool) 책임에 집중하고, 업그레이드 검증/실행/이벤트는 별도 시스템이 담당하는 것이 확장성과 테스트 용이성에 유리하다.

### 1.3 도구 효과 계산: 전용 Resolver 패턴

도구 등급에 따른 효과(범위, 에너지 비용, 물 저장량 등)는 여러 시스템에서 필요하다(FarmTile, GrowthSystem, PlayerController). 이를 ToolEffectResolver 정적 유틸리티로 일원화하여, 도구 효과를 조회하는 단일 진입점을 제공한다.

---

## 2. 클래스 다이어그램

```
+-----------------------------------------------------------------------+
|                  SeedMind.Player (네임스페이스)                          |
+-----------------------------------------------------------------------+

┌──────────────────────────────────────────────────────────────────┐
│                 ToolUpgradeSystem (MonoBehaviour)                 │
│──────────────────────────────────────────────────────────────────│
│  [참조]                                                           │
│  - _inventoryManager: InventoryManager                           │
│  - _economyManager: EconomyManager                               │
│  - _toolRegistry: ToolData[] (전체 도구 SO 레지스트리)               │
│                                                                  │
│  [이벤트]                                                         │
│  + OnUpgradeStarted: Action<ToolUpgradeInfo>                     │
│  + OnUpgradeCompleted: Action<ToolUpgradeInfo>                   │
│  + OnUpgradeFailed: Action<ToolUpgradeFailReason>                │
│                                                                  │
│  [상태]                                                           │
│  - _pendingUpgrades: Dictionary<ToolType, PendingUpgrade>        │
│                                                                  │
│  [메서드]                                                         │
│  + CanUpgrade(ToolData current): UpgradeCheckResult              │
│  + StartUpgrade(ToolData current): bool                          │
│  + CompleteUpgrade(ToolType toolType): void                      │
│  + CancelUpgrade(ToolType toolType): bool                        │
│  + GetPendingUpgrade(ToolType toolType): PendingUpgrade?         │
│  + GetUpgradeCost(ToolData current): UpgradeCostInfo             │
│                                                                  │
│  [구독]                                                           │
│  + OnEnable():                                                   │
│      TimeManager.OnDayChanged += ProcessUpgradeTimers            │
│  + OnDisable(): 구독 해제                                          │
└──────────────────────────────────────────────────────────────────┘
         │ validates cost via          │ swaps tool SO in
         ▼                            ▼
┌──────────────────┐        ┌──────────────────────────┐
│  EconomyManager  │        │  InventoryManager        │
│  (골드 차감)      │        │  (툴바 슬롯 SO 교체)      │
└──────────────────┘        └──────────────────────────┘

┌──────────────────────────────────────────────────────────────────┐
│           ToolEffectResolver (static utility class)              │
│──────────────────────────────────────────────────────────────────│
│  + GetEffectiveRange(ToolData tool): int                         │
│  + GetEnergyCost(ToolData tool): int                             │
│  + GetUseSpeed(ToolData tool): float                             │
│  + GetWateringCapacity(ToolData tool): int                       │
│  + GetSpecialEffect(ToolData tool): ToolSpecialEffect            │
│  + GetTilePattern(ToolData tool): Vector2Int[]                   │
└──────────────────────────────────────────────────────────────────┘
         ▲ 조회                ▲ 조회
         │                    │
┌──────────────┐    ┌──────────────────┐
│  ToolSystem  │    │  FarmTile        │
│  (도구 사용)  │    │  (타일 효과 적용) │
└──────────────┘    └──────────────────┘


┌──────────────────────────────────────────────────────────────────┐
│                    ToolData (ScriptableObject)                    │
│  (→ see docs/pipeline/data-pipeline.md 섹션 2.3)                 │
│──────────────────────────────────────────────────────────────────│
│  [기본 필드 - farming-architecture.md 4.3 정의]                    │
│  + toolName: string                                              │
│  + toolType: ToolType (enum)                                     │
│  + tier: int (1~3)  // → see docs/systems/tool-upgrade.md 섹션 1.1                                               │
│  + range: int                                                    │
│  + useSpeed: float                                               │
│  + upgradeCost: int                                              │
│  + nextTier: ToolData (SO 참조)                                   │
│  + icon: Sprite                                                  │
│  + modelPrefab: GameObject                                       │
│                                                                  │
│  [data-pipeline.md 확장 필드]                                      │
│  + toolId: string                                                │
│  + description: string                                           │
│  + energyCost: int           // → see docs/systems/tool-upgrade.md│
│  + cooldown: float           // → see docs/systems/tool-upgrade.md│
│  + animationClip: AnimationClip                                  │
│  + useSFX: AudioClip                                             │
│  + upgradeMaterials: UpgradeMaterial[]                            │
│  + upgradeGoldCost: int      // → see docs/systems/tool-upgrade.md│
│  + upgradeTimeDays: int      // → see docs/systems/tool-upgrade.md│
│  + specialEffect: string     // → see docs/systems/tool-upgrade.md│
│                                                                  │
│  [IInventoryItem 구현]                                            │
│  + ItemId => dataId                                              │
│  + ItemName => displayName                                       │
│  + ItemType => ItemType.Tool                                     │
│  + Icon => icon                                                  │
│  + MaxStackSize => 1                                             │
│  + Sellable => false                                             │
└──────────────────────────────────────────────────────────────────┘
         │ nextTier
         ▼
    ┌──────────────────┐
    │  ToolData (next)  │  (체인 형태: Basic → Reinforced → Legendary)
    └──────────────────┘
```

### 클래스 책임 요약

| 클래스 | 유형 | 네임스페이스 | 책임 |
|--------|------|------------|------|
| **ToolUpgradeSystem** | MonoBehaviour | SeedMind.Player | 업그레이드 검증, 실행, 대기 관리, 이벤트 발행 |
| **ToolEffectResolver** | static class | SeedMind.Player | 도구 등급별 효과 수치 계산의 단일 진입점 |
| **ToolData** | ScriptableObject | SeedMind.Player.Data | 도구 정적 데이터 (기존, 변경 없음) |
| **PendingUpgrade** | plain C# class | SeedMind.Player | 진행 중인 업그레이드 상태 (도구 타입, 잔여 일수) |
| **ToolUpgradeInfo** | struct | SeedMind.Player | 업그레이드 이벤트 페이로드 (이전 등급, 새 등급, 도구 타입) |
| **UpgradeCheckResult** | struct | SeedMind.Player | 업그레이드 가능 여부 + 실패 사유 |

---

## 3. 보조 데이터 구조

### 3.1 PendingUpgrade

```csharp
// illustrative
namespace SeedMind.Player
{
    [System.Serializable]
    public class PendingUpgrade
    {
        public ToolType toolType;           // 어떤 도구가 업그레이드 중인지
        public string currentToolId;        // 업그레이드 전 도구 dataId
        public string targetToolId;         // 업그레이드 후 도구 dataId
        public int remainingDays;           // 남은 소요 일수 (→ see docs/systems/tool-upgrade.md)
        public int totalDays;               // 총 소요 일수 (→ see docs/systems/tool-upgrade.md)
    }
}
```

### 3.2 ToolUpgradeInfo (이벤트 페이로드)

```csharp
// illustrative
namespace SeedMind.Player
{
    public struct ToolUpgradeInfo
    {
        public ToolType toolType;
        public ToolData previousTool;       // 업그레이드 전 SO
        public ToolData upgradedTool;       // 업그레이드 후 SO
        public int newTier;                 // 새 등급 (1~5)
    }
}
```

### 3.3 UpgradeCheckResult

```csharp
// illustrative
namespace SeedMind.Player
{
    public struct UpgradeCheckResult
    {
        public bool canUpgrade;
        public ToolUpgradeFailReason failReason;    // None이면 업그레이드 가능
        public UpgradeCostInfo cost;                // 필요 비용 정보
    }

    public enum ToolUpgradeFailReason
    {
        None,                   // 업그레이드 가능
        AlreadyMaxTier,         // 최고 등급 도달
        InsufficientGold,       // 골드 부족
        InsufficientMaterials,  // 재료 부족
        AlreadyUpgrading,       // 해당 도구가 이미 업그레이드 중
        ToolNotOwned,           // 해당 도구를 소유하지 않음
        LevelTooLow             // 플레이어 레벨 부족
    }
}
```

### 3.4 UpgradeCostInfo

```csharp
// illustrative
namespace SeedMind.Player
{
    public struct UpgradeCostInfo
    {
        public int goldCost;                // → see docs/systems/tool-upgrade.md
        public UpgradeMaterial[] materials;  // → see docs/systems/tool-upgrade.md
        public int timeDays;                // → see docs/systems/tool-upgrade.md
        public int requiredLevel;           // → see docs/systems/tool-upgrade.md
    }
}
```

### 3.5 ToolSpecialEffect enum

```csharp
// illustrative
namespace SeedMind.Player
{
    /// <summary>
    /// 고등급 도구의 특수 효과.
    /// 구체적 수치/조건은 → see docs/systems/tool-upgrade.md
    /// </summary>
    public enum ToolSpecialEffect
    {
        None,               // 기본 등급: 특수 효과 없음
        AreaEffect,         // 범위 효과 (3x3 등)
        ChargeAttack,       // 충전 사용 (긴 누르기로 범위 확대)
        AutoWater,          // 자동 물주기 효과
        QualityBoost,       // 수확 품질 보너스
        DoubleHarvest       // 이중 수확 확률
    }
}
```

---

## 4. ToolEffectResolver 상세 설계

ToolEffectResolver는 정적 유틸리티 클래스로, 도구의 등급(tier)에 따른 실효 수치를 반환한다. 모든 시스템이 이 클래스를 통해 도구 효과를 조회함으로써, 효과 계산 로직이 한 곳에 집중된다.

```csharp
// illustrative
namespace SeedMind.Player
{
    /// <summary>
    /// 도구 등급별 효과 계산 유틸리티.
    /// ToolData SO의 필드를 읽어 가공된 효과 수치를 반환한다.
    /// 모든 수치의 canonical 출처: → see docs/systems/tool-upgrade.md
    /// </summary>
    public static class ToolEffectResolver
    {
        /// <summary>
        /// 도구의 유효 범위를 반환한다. range 필드 기반.
        /// 예: 호미 T1=1(단일, Basic), T2=3(1x3, Reinforced), T3=9(3x3, Legendary)
        /// (→ see docs/systems/tool-upgrade.md 섹션 3.1)
        /// </summary>
        public static int GetEffectiveRange(ToolData tool)
        {
            return tool.range; // → see docs/systems/tool-upgrade.md for 등급별 range 값
        }

        /// <summary>
        /// 도구 사용 당 에너지 소모량을 반환한다.
        /// 고등급 도구는 범위가 넓어도 단일 사용당 에너지는 동일하거나 소폭 증가.
        /// </summary>
        public static int GetEnergyCost(ToolData tool)
        {
            return tool.energyCost; // → see docs/systems/tool-upgrade.md for 등급별 energyCost 값
        }

        /// <summary>
        /// 도구 사용 속도 배수를 반환한다.
        /// 고등급일수록 빠름 (useSpeed > 1.0).
        /// </summary>
        public static float GetUseSpeed(ToolData tool)
        {
            return tool.useSpeed; // → see docs/systems/tool-upgrade.md for 등급별 useSpeed 값
        }

        /// <summary>
        /// 물뿌리개 전용: 한 번 충전당 뿌릴 수 있는 타일 수.
        /// 다른 도구는 0을 반환한다.
        /// </summary>
        public static int GetWateringCapacity(ToolData tool)
        {
            if (tool.toolType != ToolType.WateringCan) return 0;
            return tool.range; // → see docs/systems/tool-upgrade.md for 물뿌리개 등급별 용량
        }

        /// <summary>
        /// 도구의 특수 효과를 반환한다.
        /// specialEffect 문자열 → enum 매핑.
        /// </summary>
        public static ToolSpecialEffect GetSpecialEffect(ToolData tool)
        {
            if (string.IsNullOrEmpty(tool.specialEffect)) return ToolSpecialEffect.None;
            // → see docs/systems/tool-upgrade.md for 등급별 특수 효과 목록
            return System.Enum.TryParse<ToolSpecialEffect>(tool.specialEffect, out var effect)
                ? effect
                : ToolSpecialEffect.None;
        }

        /// <summary>
        /// 도구의 영향 타일 패턴을 반환한다 (그리드 오프셋 배열).
        /// 예: range=1 → [(0,0)], range=3(3x1) → [(-1,0),(0,0),(1,0)]
        /// </summary>
        public static Vector2Int[] GetTilePattern(ToolData tool)
        {
            int range = tool.range;
            if (range <= 1)
                return new[] { Vector2Int.zero };

            // → see docs/systems/tool-upgrade.md for 등급별 패턴 정의
            // 기본: 정면 방향 1xN 직선 패턴
            var pattern = new Vector2Int[range];
            for (int i = 0; i < range; i++)
                pattern[i] = new Vector2Int(i, 0);
            return pattern;
        }
    }
}
```

### 4.1 ToolEffectResolver 소비자

| 소비자 | 호출 메서드 | 용도 |
|--------|-----------|------|
| ToolSystem.UseCurrentTool() | GetTilePattern() | 도구 사용 시 영향받는 타일 목록 결정 |
| ToolSystem.UseCurrentTool() | GetEnergyCost() | 에너지 차감량 결정 |
| ToolSystem.UseCurrentTool() | GetUseSpeed() | 도구 애니메이션/쿨다운 속도 결정 |
| FarmTile.TryWater() | GetWateringCapacity() | 물뿌리개 충전 잔량 판정 |
| UI (ToolTooltip) | 전체 메서드 | 도구 정보 표시 |
| UpgradeUI | GetSpecialEffect() | 업그레이드 미리보기 |

---

## 5. ToolUpgradeSystem 상세 설계

### 5.1 업그레이드 흐름

```
플레이어가 대장간(Smithy) NPC/UI에 접근
    │
    ▼
ToolUpgradeSystem.CanUpgrade(currentTool) 호출
    │
    ├── currentTool.nextTier == null? → AlreadyMaxTier
    ├── _pendingUpgrades에 해당 toolType 존재? → AlreadyUpgrading
    ├── 플레이어 레벨 < requiredLevel? → LevelTooLow
    ├── 골드 < upgradeGoldCost? → InsufficientGold
    ├── 재료 부족? → InsufficientMaterials
    └── 모두 통과 → canUpgrade = true, cost 정보 반환
    │
    ▼
UI에서 업그레이드 확인 버튼 클릭
    │
    ▼
ToolUpgradeSystem.StartUpgrade(currentTool) 호출
    │
    ├── 1) EconomyManager.TrySpendGold(upgradeGoldCost) → 골드 차감
    ├── 2) InventoryManager.RemoveItem(각 material.materialId, qty) → 재료 차감
    ├── 3) InventoryManager에서 해당 도구를 "업그레이드 중" 상태로 표시
    │       → 도구 슬롯에서 제거 (대장간에 맡김)
    ├── 4) _pendingUpgrades에 PendingUpgrade 추가
    │       → remainingDays = upgradeTimeDays (→ see docs/systems/tool-upgrade.md)
    └── 5) OnUpgradeStarted 이벤트 발행
    │
    ▼
매일 아침 (TimeManager.OnDayChanged)
    │
    ▼
ToolUpgradeSystem.ProcessUpgradeTimers()
    │
    ├── foreach (pendingUpgrade in _pendingUpgrades)
    │       pendingUpgrade.remainingDays -= 1
    │       if remainingDays <= 0:
    │           CompleteUpgrade(pendingUpgrade.toolType)
    │
    ▼
ToolUpgradeSystem.CompleteUpgrade(toolType)
    │
    ├── 1) DataRegistry에서 targetToolId → ToolData SO 조회
    ├── 2) InventoryManager.SetToolbarItem(index, targetToolId) → 새 도구 장착
    ├── 3) _pendingUpgrades에서 제거
    └── 4) OnUpgradeCompleted 이벤트 발행
```

### 5.2 업그레이드 중 도구 사용 불가 처리

업그레이드 중인 도구는 대장간에 맡겨진 상태이므로 플레이어가 사용할 수 없다.

```
StartUpgrade() 시:
    → InventoryManager의 해당 툴바 슬롯을 비움 (또는 "수리 중" placeholder 아이템으로 교체)
    → ToolSystem은 해당 슬롯 선택 시 "도구가 대장간에 있습니다" 피드백

CompleteUpgrade() 시:
    → 비워진 툴바 슬롯에 업그레이드된 ToolData SO 설정
    → UI 알림: "[도구명]이 [등급명]으로 업그레이드되었습니다!"
```

[RISK] 업그레이드 중 해당 도구 타입이 필수인 행동(예: 호미 업그레이드 중 경작 불가)에 대한 플레이어 안내가 필요하다. UI에서 "내일 완료 예정" 등의 정보를 표시해야 한다.

---

## 6. 기존 ToolSystem 연동 설계

### 6.1 현재 ToolSystem 구조 (farming-architecture.md 섹션 3.1)

```
ToolSystem.UseCurrentTool(Vector3 worldPos)
    │
    ├── worldPos → gridPos 변환
    ├── FarmGrid.GetTile(gridPos) → tile 참조
    ├── switch (currentTool.toolType) → tile.TryTill() / TryWater() / TryPlant() / TryHarvest()
    └── 도구 애니메이션/효과음 재생
```

### 6.2 ToolEffectResolver 통합 후 변경

```
ToolSystem.UseCurrentTool(Vector3 worldPos) -- 변경됨
    │
    ├── 1) currentTool이 null/업그레이드 중? → 조기 반환 + 피드백
    │
    ├── 2) 에너지 체크
    │       int cost = ToolEffectResolver.GetEnergyCost(currentTool);
    │       if (playerEnergy < cost) → "에너지 부족" 피드백, 반환
    │
    ├── 3) 영향 타일 계산 (NEW)
    │       Vector2Int[] pattern = ToolEffectResolver.GetTilePattern(currentTool);
    │       Vector2Int baseGrid = FarmGrid.WorldToGrid(worldPos);
    │       Vector2Int[] targetTiles = ApplyPatternWithDirection(baseGrid, pattern, playerFacing);
    │
    ├── 4) foreach (targetTile in targetTiles)  -- 기존 단일 타일 → 다중 타일
    │       FarmTile tile = FarmGrid.GetTile(targetTile);
    │       if (tile == null) continue;
    │       switch (currentTool.toolType)
    │           case Hoe:    tile.TryTill()
    │           case Water:  tile.TryWater()
    │           case Sickle: tile.TryHarvest(out crop, out qty) → Inventory.Add()
    │           ...
    │
    ├── 5) 에너지 차감: playerEnergy -= cost;
    │
    ├── 6) 쿨다운 적용
    │       float speed = ToolEffectResolver.GetUseSpeed(currentTool);
    │       _cooldownRemaining = currentTool.cooldown / speed;
    │
    └── 7) 도구 애니메이션/효과음 재생 (속도 = useSpeed)
```

### 6.3 변경 요약

| 항목 | 변경 전 | 변경 후 |
|------|--------|--------|
| 영향 타일 | 항상 단일 타일 | ToolEffectResolver.GetTilePattern()으로 다중 타일 |
| 에너지 비용 | 하드코딩 또는 없음 | ToolEffectResolver.GetEnergyCost()로 SO 기반 조회 |
| 사용 속도 | 고정 | ToolEffectResolver.GetUseSpeed()로 등급별 가변 |
| null 도구 처리 | 고려 안 됨 | 업그레이드 중 null 체크 추가 |

---

## 7. SaveData 확장

### 7.1 기존 PlayerSaveData (-> see data-pipeline.md 섹션 2.2)

현재 PlayerSaveData에는 `equippedToolIndex`만 존재하고, 도구 등급 정보는 인벤토리 슬롯의 `itemId`로 간접 표현된다 (예: `"hoe_reinforced"`). 이 방식을 유지하되, **업그레이드 진행 중 상태**를 추가로 저장해야 한다.

### 7.2 ToolUpgradeSaveData (신규)

```csharp
// illustrative
namespace SeedMind.Player
{
    /// <summary>
    /// 도구 업그레이드 진행 상태 세이브 데이터.
    /// PlayerSaveData에 포함된다.
    /// </summary>
    [System.Serializable]
    public class ToolUpgradeSaveData
    {
        public PendingUpgradeSaveEntry[] pendingUpgrades; // 진행 중인 업그레이드 목록
    }

    [System.Serializable]
    public class PendingUpgradeSaveEntry
    {
        public int toolTypeIndex;       // (int)ToolType
        public string currentToolId;    // 업그레이드 전 도구 dataId
        public string targetToolId;     // 업그레이드 후 도구 dataId
        public int remainingDays;       // 남은 소요 일수
        public int totalDays;           // 총 소요 일수
    }
}
```

### 7.3 PlayerSaveData 확장

```csharp
// illustrative — 기존 PlayerSaveData에 필드 추가
namespace SeedMind.Player
{
    [System.Serializable]
    public class PlayerSaveData
    {
        // (기존 필드는 → see docs/pipeline/data-pipeline.md 섹션 2.2)
        public float posX, posY, posZ;
        public float rotY;
        public int currentEnergy;
        public int maxEnergy;
        public InventorySlotSaveData[] inventorySlots;
        public int equippedToolIndex;
        public int level;
        public int currentExp;

        // === 신규: 도구 업그레이드 상태 ===
        public ToolUpgradeSaveData toolUpgradeState; // null이면 진행 중 업그레이드 없음
    }
}
```

### 7.4 세이브/로드 흐름

```
세이브 시:
    SaveManager.Save()
        → ToolUpgradeSystem.GetSaveData() → ToolUpgradeSaveData
        → PlayerSaveData.toolUpgradeState에 할당
        → JSON 직렬화

로드 시:
    SaveManager.Load()
        → JSON 역직렬화 → PlayerSaveData
        → ToolUpgradeSystem.LoadSaveData(data.toolUpgradeState)
            → _pendingUpgrades 복원
            → 해당 도구 슬롯 비움 상태 복원
```

### 7.5 JSON 세이브 예시

```json
{
    "toolUpgradeState": {
        "pendingUpgrades": [
            {
                "toolTypeIndex": 0,
                "currentToolId": "hoe_basic",
                "targetToolId": "hoe_reinforced",
                "remainingDays": 1,
                "totalDays": 2
            }
        ]
    }
}
```

---

## 8. 이벤트 설계

### 8.1 ToolUpgradeEvents (정적 이벤트 허브)

```csharp
// illustrative
namespace SeedMind.Player
{
    /// <summary>
    /// 도구 업그레이드 관련 이벤트 허브.
    /// FarmEvents 패턴을 따른다.
    /// </summary>
    public static class ToolUpgradeEvents
    {
        /// <summary>업그레이드 시작 (도구가 대장간에 맡겨짐)</summary>
        public static Action<ToolUpgradeInfo> OnUpgradeStarted;

        /// <summary>업그레이드 완료 (도구가 새 등급으로 돌아옴)</summary>
        public static Action<ToolUpgradeInfo> OnUpgradeCompleted;

        /// <summary>업그레이드 시도 실패 (비용 부족 등)</summary>
        public static Action<ToolUpgradeFailReason> OnUpgradeFailed;

        /// <summary>매일 아침 진행 상태 갱신 (잔여 일수 변경)</summary>
        public static Action<ToolType, int> OnUpgradeProgressUpdated; // toolType, remainingDays
    }
}
```

### 8.2 이벤트 소비자

| 이벤트 | 소비자 | 용도 |
|--------|--------|------|
| OnUpgradeStarted | UI (HUD) | "호미를 대장간에 맡겼습니다 (2일 소요)" 메시지 |
| OnUpgradeStarted | ProgressionManager | 업그레이드 시작 마일스톤 추적 |
| OnUpgradeCompleted | UI (HUD) | "호미가 철 호미로 업그레이드되었습니다!" 팝업 |
| OnUpgradeCompleted | ProgressionManager | 업그레이드 완료 XP 부여 |
| OnUpgradeFailed | UI | 실패 사유 표시 ("골드가 부족합니다") |
| OnUpgradeProgressUpdated | UI (SmithyPanel) | 진행 바 갱신 |

---

## 9. 프로젝트 구조 반영

### 9.1 신규 파일 목록

| 경로 | 클래스 | 네임스페이스 |
|------|--------|------------|
| `Scripts/Player/ToolUpgradeSystem.cs` | ToolUpgradeSystem | SeedMind.Player |
| `Scripts/Player/ToolEffectResolver.cs` | ToolEffectResolver | SeedMind.Player |
| `Scripts/Player/ToolUpgradeEvents.cs` | ToolUpgradeEvents | SeedMind.Player |
| `Scripts/Player/Data/PendingUpgrade.cs` | PendingUpgrade | SeedMind.Player |
| `Scripts/Player/Data/ToolUpgradeInfo.cs` | ToolUpgradeInfo, UpgradeCheckResult, ToolUpgradeFailReason, UpgradeCostInfo | SeedMind.Player |
| `Scripts/Player/Data/ToolSpecialEffect.cs` | ToolSpecialEffect | SeedMind.Player |
| `Scripts/Player/Data/ToolUpgradeSaveData.cs` | ToolUpgradeSaveData, PendingUpgradeSaveEntry | SeedMind.Player |
| `Scripts/UI/SmithyUI.cs` | SmithyUI | SeedMind.UI |
| `Scripts/UI/ToolUpgradeSlotUI.cs` | ToolUpgradeSlotUI | SeedMind.UI |

모든 경로 접두어: `Assets/_Project/`

### 9.2 Assembly Definition 영향

- `SeedMind.Player.asmdef`에 신규 파일 자동 포함 (기존 참조: Core, Farm)
- `SeedMind.UI.asmdef`에 SmithyUI 포함 (기존 참조에 Player 이미 포함)
- 추가 asmdef 참조 변경 없음

### 9.3 씬 계층 구조 변경

```
--- PLAYER ---
    └── Player
        ├── PlayerModel
        ├── PlayerController
        ├── ToolSystem
        └── ToolUpgradeSystem    ← 신규 추가
```

---

# Part II -- MCP 구현 계획

---

## 10. MCP 태스크 시퀀스

### Phase A: 스크립트 작성 (Claude Code 직접 작성, MCP 불필요)

```
Step A-1: Scripts/Player/Data/ToolSpecialEffect.cs 작성
          → namespace SeedMind.Player
          → enum ToolSpecialEffect 정의

Step A-2: Scripts/Player/Data/ToolUpgradeInfo.cs 작성
          → namespace SeedMind.Player
          → ToolUpgradeInfo struct
          → UpgradeCheckResult struct
          → ToolUpgradeFailReason enum
          → UpgradeCostInfo struct

Step A-3: Scripts/Player/Data/PendingUpgrade.cs 작성
          → namespace SeedMind.Player
          → PendingUpgrade class ([Serializable])

Step A-4: Scripts/Player/Data/ToolUpgradeSaveData.cs 작성
          → namespace SeedMind.Player
          → ToolUpgradeSaveData class
          → PendingUpgradeSaveEntry class

Step A-5: Scripts/Player/ToolEffectResolver.cs 작성
          → namespace SeedMind.Player
          → static class ToolEffectResolver
          → GetEffectiveRange, GetEnergyCost, GetUseSpeed,
            GetWateringCapacity, GetSpecialEffect, GetTilePattern

Step A-6: Scripts/Player/ToolUpgradeEvents.cs 작성
          → namespace SeedMind.Player
          → static class ToolUpgradeEvents
          → OnUpgradeStarted, OnUpgradeCompleted, OnUpgradeFailed, OnUpgradeProgressUpdated

Step A-7: Scripts/Player/ToolUpgradeSystem.cs 작성
          → namespace SeedMind.Player
          → MonoBehaviour
          → CanUpgrade, StartUpgrade, CompleteUpgrade, CancelUpgrade
          → ProcessUpgradeTimers (OnDayChanged 구독)
          → GetSaveData, LoadSaveData

Step A-8: 기존 ToolSystem.cs 수정
          → UseCurrentTool() 메서드에 ToolEffectResolver 연동 추가
          → 다중 타일 패턴 지원
          → 업그레이드 중 도구 null 체크 추가

Step A-9: 기존 PlayerSaveData 확장
          → toolUpgradeState: ToolUpgradeSaveData 필드 추가
```

[RISK] Step A-8에서 기존 ToolSystem 수정 시, farming-tasks.md(ARC-003)에서 정의한 기존 동작과의 호환성 확인 필요. 단일 타일 모드(range=1)에서 기존 동작과 완전 동일해야 한다.

### Phase B: ToolData SO 에셋 생성 (MCP)

```
Step B-1: Assets/_Project/Data/Tools/ 폴더에 ToolData SO 에셋 생성
          (등급별 수치는 → see docs/systems/tool-upgrade.md for canonical values)

          (등급 체계: Basic=1 / Reinforced=2 / Legendary=3 → see docs/systems/tool-upgrade.md 섹션 1.1)

          호미 체인:
          B-1-01: create_asset → SO_Tool_Hoe_Basic       (tier=1)
          B-1-02: create_asset → SO_Tool_Hoe_Reinforced  (tier=2)
          B-1-03: create_asset → SO_Tool_Hoe_Legendary   (tier=3)

          물뿌리개 체인:
          B-1-04: create_asset → SO_Tool_Water_Basic       (tier=1)
          B-1-05: create_asset → SO_Tool_Water_Reinforced  (tier=2)
          B-1-06: create_asset → SO_Tool_Water_Legendary   (tier=3)

          낫 체인:
          B-1-07: create_asset → SO_Tool_Sickle_Basic       (tier=1)
          B-1-08: create_asset → SO_Tool_Sickle_Reinforced  (tier=2)
          B-1-09: create_asset → SO_Tool_Sickle_Legendary   (tier=3)

          단일 등급 도구:
          B-1-10: create_asset → SO_Tool_SeedBag  (tier=1, nextTier=null)
          B-1-11: create_asset → SO_Tool_Hand     (tier=1, nextTier=null)

Step B-2: nextTier 참조 체인 연결
          B-2-01: set_property → SO_Tool_Hoe_Basic.nextTier = SO_Tool_Hoe_Reinforced
          B-2-02: set_property → SO_Tool_Hoe_Reinforced.nextTier = SO_Tool_Hoe_Legendary
          B-2-03: set_property → SO_Tool_Water_Basic.nextTier = SO_Tool_Water_Reinforced
          B-2-04: set_property → SO_Tool_Water_Reinforced.nextTier = SO_Tool_Water_Legendary
          B-2-05: set_property → SO_Tool_Sickle_Basic.nextTier = SO_Tool_Sickle_Reinforced
          B-2-06: set_property → SO_Tool_Sickle_Reinforced.nextTier = SO_Tool_Sickle_Legendary
          (각 체인 2개씩, 총 6개)

          [RISK] MCP의 SO 간 참조 설정 지원 여부 사전 검증 필요.
          대안: nextTier 대신 nextTierId(string)를 사용하고 런타임에 DataRegistry로 조회.
```

### Phase C: 씬 오브젝트 구성 (MCP)

```
Step C-1: Player 오브젝트에 ToolUpgradeSystem 컴포넌트 추가
          C-1-01: add_component → "Player", "SeedMind.Player.ToolUpgradeSystem"

Step C-2: ToolUpgradeSystem에 참조 설정
          C-2-01: set_property → _inventoryManager = InventoryManager (씬 내 참조)
          C-2-02: set_property → _economyManager = EconomyManager (씬 내 참조)
          C-2-03: set_property → _toolRegistry = [SO_Tool_Hoe_Basic, ..., SO_Tool_Hand]
                  [RISK] SO 배열 참조 설정. 기존 farming-tasks.md B-3와 동일 리스크.
```

### Phase D: 대장간 UI (MCP)

```
Step D-1: Canvas_Overlay 하위에 SmithyPanel 생성
          D-1-01: create_object → "SmithyPanel", parent: "Canvas_Overlay"
          D-1-02: add_component → RectTransform, CanvasGroup
          D-1-03: set_property → 기본 비활성 (CanvasGroup.alpha = 0, blocksRaycasts = false)

Step D-2: SmithyPanel 내부 구조
          D-2-01: create_object → "Title" (TextMeshProUGUI, "대장간")
          D-2-02: create_object → "ToolSlotContainer" (HorizontalLayoutGroup)
          D-2-03: create_object → "UpgradeInfoPanel" (선택된 도구 정보 표시)
          D-2-04: create_object → "CostPanel" (비용/재료 표시)
          D-2-05: create_object → "UpgradeButton" (Button, "업그레이드")
          D-2-06: create_object → "ProgressBar" (업그레이드 진행 중 표시)
          D-2-07: create_object → "CloseButton" (Button, "닫기")

Step D-3: SmithyUI 컴포넌트 연결
          D-3-01: add_component → "SmithyPanel", "SeedMind.UI.SmithyUI"
          D-3-02: set_property → 각 UI 요소 참조 연결
```

### Phase E: 통합 테스트 (MCP Play Mode)

```
Step E-1: Play Mode 진입
          → Console 로그로 ToolUpgradeSystem 초기화 확인
          → ToolData SO 로드 및 nextTier 체인 검증

Step E-2: 업그레이드 시뮬레이션
          → 콘솔 커맨드로 골드/재료 지급
          → ToolUpgradeSystem.StartUpgrade() 호출
          → Console 로그: "업그레이드 시작: hoe_basic → hoe_reinforced, 소요 N일"

Step E-3: 시간 경과 테스트
          → TimeManager.AdvanceDay() 반복 호출
          → Console 로그: "업그레이드 진행: 잔여 N일"
          → Console 로그: "업그레이드 완료: hoe_reinforced 획득"

Step E-4: ToolSystem 연동 테스트
          → 업그레이드된 도구로 타일 사용
          → Console 로그: 범위/에너지/속도 값 확인
          → 다중 타일 영향 확인 (range > 1인 경우)

Step E-5: 세이브/로드 테스트
          → 업그레이드 진행 중 세이브
          → 로드 후 _pendingUpgrades 복원 확인
          → 시간 경과 후 정상 완료 확인
```

---

## Open Questions

- [OPEN] 업그레이드 소요 일수 동안 대체 도구(임시 도구)를 제공할지, 아니면 해당 도구 없이 플레이해야 하는지. 게임플레이 경험에 직접적 영향이 크므로 디자이너 결정 필요.
- [OPEN] 대장간이 별도 시설(BuildingData)인지, NPC 기반 서비스인지. 시설이라면 건설/해금 조건이 필요하고, NPC라면 초기부터 이용 가능.
- [OPEN] 업그레이드 취소 시 재료/골드 환불 비율. 전액 환불이면 리스크 없는 시도가 가능해져 긴장감 저하.
- [OPEN] 씨앗봉투(SeedBag)의 업그레이드 가능 여부. "한 번에 여러 타일에 심기" 확장 가능성 있음 (-> see data-pipeline.md 섹션 2.3).
- [OPEN] ToolEffectResolver의 GetTilePattern() 반환 패턴이 방향 의존적인지. 플레이어 facing 방향에 따라 패턴을 회전시킬지.

## Risks

- [RISK] MCP의 SO 간 참조(nextTier 체인) 설정 지원 불확실. 대안: string ID 기반 조회로 전환하면 SO 직접 참조의 에디터 편의성 상실. Phase B에서 사전 검증 후 결정.
- [RISK] 기존 ToolSystem.UseCurrentTool() 수정 시 단일 타일 동작 회귀 가능성. range=1일 때 GetTilePattern()이 정확히 [(0,0)]만 반환하는지 단위 테스트 필수.
- [RISK] 업그레이드 중 세이브/로드 시 도구 슬롯 상태 불일치. 로드 직후 _pendingUpgrades와 인벤토리 슬롯 상태의 정합성 검증 로직 필요.
- [RISK] static event(ToolUpgradeEvents) 구독 누수. FarmEvents와 동일한 씬 전환 시 초기화 루틴 적용 필요 (-> see farming-architecture.md 섹션 6.3).
- [RISK] 다중 도구 동시 업그레이드 허용 시 플레이어가 모든 도구를 잃어 게임 진행 불가 상태에 빠질 수 있음. 동시 업그레이드 수 제한(최대 1~2개) 고려.

---

## Cross-references

- `docs/architecture.md` 3절 (프로젝트 구조), 6절 (데이터 관리)
- `docs/systems/farming-architecture.md` 3.1절 (ToolSystem.UseCurrentTool 흐름), 4.3절 (ToolData SO), 6절 (이벤트 시스템)
- `docs/systems/inventory-architecture.md` 섹션 1.2 (툴바 슬롯 독립 방식), 섹션 2 (InventoryManager 클래스)
- `docs/systems/project-structure.md` 2절 (네임스페이스), 3절 (의존성 규칙), 4절 (asmdef)
- `docs/pipeline/data-pipeline.md` 섹션 2.3 (ToolData 확장 필드), 섹션 2.2 (PlayerSaveData)
- `docs/systems/tool-upgrade.md` (디자인 canonical -- 디자이너 작성 예정, 모든 수치의 단일 출처)
- `docs/systems/progression-architecture.md` (업그레이드 완료 시 XP 부여 연동)
- `docs/mcp/farming-tasks.md` (기존 ToolSystem MCP 태스크 -- Phase C-2)

---

*이 문서는 Claude Code가 기존 아키텍처 문서와의 일관성을 유지하며 자율적으로 작성했습니다.*
