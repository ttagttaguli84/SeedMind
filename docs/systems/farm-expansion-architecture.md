# 농장 확장 시스템 기술 아키텍처

> FarmZoneManager, ZoneData SO, 타일 구매/해금 흐름, 개간(Clearing) 시스템, 세이브/로드 통합, MCP 구현 태스크  
> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: ARC-023

---

## Context

농장 확장 시스템은 플레이어가 초기 8x8 타일 농장을 6단계에 걸쳐 구역 단위로 확장하는 핵심 진행 메카닉이다. 구역 수, 크기, 해금 비용은 `(→ see docs/systems/farm-expansion.md 섹션 1, DES-012)`. 기존 `farming-architecture.md`의 FarmGrid/FarmTile 구조 위에 **구역(Zone)** 레이어를 추가하여, 구역별 해금/구매/개간 흐름을 관리한다.

**기존 시스템과의 관계**:
- FarmGrid(farming-architecture.md)는 타일 배열의 소유자이며, FarmZoneManager는 구역 단위의 해금/활성화를 FarmGrid에 위임한다
- EconomyManager(economy-architecture.md)는 골드 차감 API(`SpendGold`)를 제공하며, 구역 구매 시 호출된다
- ProgressionManager(progression-architecture.md)는 레벨 체크 API(`IsUnlocked(UnlockType.FarmExpansion, zoneId)`)를 제공한다

---

## Part I -- 아키텍처 설계

### 1. 클래스 다이어그램

```
┌──────────────────────────────────────────────────────────────────────┐
│              FarmZoneManager (MonoBehaviour, Singleton)               │
│──────────────────────────────────────────────────────────────────────│
│  [상태]                                                              │
│  - _zones: ZoneData[]                // Inspector에서 할당할 전체 구역 SO 목록 │
│  - _zoneStates: Dictionary<string, ZoneRuntimeState>  // 런타임 상태  │
│  - _obstacleInstances: Dictionary<Vector2Int, ObstacleInstance>      │
│                                                                      │
│  [참조]                                                              │
│  - _farmGrid: FarmGrid              // 타일 활성화 위임               │
│  - _economyManager: EconomyManager  // 골드 차감 (→ Singleton 접근)  │
│                                                                      │
│  [메서드]                                                            │
│  + Initialize(): void                                                │
│  + TryUnlockZone(string zoneId): bool                               │
│  + IsZoneUnlocked(string zoneId): bool                              │
│  + GetZoneState(string zoneId): ZoneState                           │
│  + ClearObstacle(Vector2Int tilePos, ToolType tool): ClearResult    │
│  + GetZoneForTile(Vector2Int tilePos): ZoneData                     │
│  + GetObstacleAt(Vector2Int tilePos): ObstacleInstance              │
│  + GetSaveData(): ZoneSaveData                                      │
│  + LoadSaveData(ZoneSaveData data): void                            │
│                                                                      │
│  [ISaveable 구현]                                                    │
│  + SaveLoadOrder => 45                                               │
│  + GetSaveData(): object                                             │
│  + LoadSaveData(object data): void                                  │
└──────────────────────────────────────────────────────────────────────┘
          │ references                    │ references
          ▼                               ▼
┌───────────────────┐          ┌──────────────────────────┐
│  ZoneData (SO)    │          │  FarmGrid                │
│                   │          │  (farming-architecture)  │
│  zoneId: string   │          │                          │
│  zoneName: string │          │  + ActivateZoneTiles(    │
│  requiredLevel:int│          │      Vector2Int[] pos):  │
│  unlockCost: int  │          │      void                │
│  tilePositions:   │          │  + DeactivateZoneTiles(  │
│    Vector2Int[]   │          │      Vector2Int[] pos):  │
│  zoneType: ZoneType│         │      void                │
│  obstacleMap:     │          │  + GetTile(pos): FarmTile│
│    ObstacleEntry[]│          └──────────────────────────┘
│  sortOrder: int   │
└───────────────────┘

┌──────────────────────────────┐
│  ObstacleInstance (Plain C#) │
│                              │
│  entry: ObstacleEntry        │
│  position: Vector2Int        │
│  currentHP: int              │
│  isCleared: bool             │
│  droppedLoot: bool           │
└──────────────────────────────┘

┌──────────────────────────────┐
│  ZoneRuntimeState (Plain C#) │
│                              │
│  zoneId: string              │
│  state: ZoneState            │
│  clearedObstacleCount: int   │
│  totalObstacleCount: int     │
└──────────────────────────────┘
```

### 1.1 클래스 책임 요약

| 클래스 | 유형 | 책임 |
|--------|------|------|
| **FarmZoneManager** | MonoBehaviour (Singleton) | 구역 해금/상태 관리, 개간 처리, 이벤트 발행, 세이브/로드 |
| **ZoneData** | ScriptableObject | 구역 정적 데이터 (위치, 비용, 장애물 배치) |
| **ObstacleEntry** | Serializable struct (SO 내부) | 장애물 정적 데이터 (위치, 종류, HP, 드랍) |
| **ObstacleInstance** | Plain C# class | 장애물 런타임 상태 (남은 HP, 제거 여부) |
| **ZoneRuntimeState** | Plain C# class | 구역 런타임 상태 집계 |

---

### 2. Enum 정의

#### 2.1 ZoneState

```csharp
// illustrative
namespace SeedMind.Farm
{
    /// <summary>
    /// 농장 구역의 현재 상태.
    /// </summary>
    public enum ZoneState
    {
        Locked,         // 레벨 미달 -- 구매 불가
        Unlockable,     // 레벨 충족 -- 구매 가능 (골드 필요)
        Unlocked,       // 구매 완료 -- 장애물 존재 가능
        FullyCleared    // 모든 장애물 제거 완료
    }
}
```

#### 2.2 ZoneType

```csharp
// illustrative
namespace SeedMind.Farm
{
    /// <summary>
    /// 구역의 용도/특성을 정의.
    /// 확장 구역은 기본적으로 Farmland이지만, 향후 특수 구역을 지원한다.
    /// </summary>
    public enum ZoneType
    {
        Farmland,       // 일반 농경지 -- 모든 작물 재배 가능
        Orchard,        // 과수원 -- 과일 나무 전용 (향후 확장)
        Pasture,        // 목초지 -- 동물 사육 전용 (향후 확장)
        Greenhouse,     // 온실 부지 -- 온실 건설 전용
        Pond            // 양어장 -- 낚시/양식 전용 (향후 확장)
    }
}
```

[OPEN] Phase 1에서 사용할 ZoneType은 `Farmland`만이다. 나머지는 콘텐츠 확장을 위한 예약 슬롯이며, 구현 시점은 미정. Orchard/Pasture/Pond는 별도 콘텐츠 문서(CON-006, CON-007 등)에서 요구사항이 확정된 후 활성화한다.

#### 2.3 ObstacleType

```csharp
// illustrative
namespace SeedMind.Farm
{
    /// <summary>
    /// 농장 구역에 존재하는 장애물 종류.
    /// 도구별 처리 가능 여부와 필요 도구 등급이 다르다.
    /// </summary>
    // 장애물 종류 및 도구 요건 → see docs/systems/farm-expansion.md 섹션 3.1
    public enum ObstacleType
    {
        Weed,           // debris_weed    — 낫 Basic+, HP 1
        SmallRock,      // debris_small_rock  — 곡괭이*, HP 2/1/1
        LargeRock,      // debris_large_rock  — 곡괭이 Reinforced+, HP -/5/2
        Stump,          // debris_stump   — 도끼*, HP 3/2/1
        SmallTree,      // debris_small_tree  — 도끼*, HP 2/1/1
        LargeTree,      // debris_large_tree  — 도끼 Reinforced+, HP -/6/3
        Bush            // debris_bush    — 낫 Basic+, HP 2/1/1
    }
    // *곡괭이(Pickaxe)/도끼(Axe): ToolType 미확장 상태. 현재는 호미 등급별 대체. → see [RISK] 섹션 2.3
}
```

[RISK] 도끼(Axe)와 곡괭이(Pickaxe)가 ToolType enum에 없다. 현재 ToolType은 Hoe, WateringCan, SeedBag, Sickle, Hand만 정의되어 있다(farming-architecture.md 섹션 4.3). 개간 전용 도구 추가 여부를 결정해야 한다. 현재 설계에서는 호미(Hoe)의 등급별 개간 능력으로 대체하되, ToolType 확장은 [OPEN]으로 남긴다.

---

### 3. ZoneData ScriptableObject 상세

```csharp
// illustrative
namespace SeedMind.Farm
{
    [CreateAssetMenu(fileName = "NewZone", menuName = "SeedMind/ZoneData")]
    public class ZoneData : ScriptableObject
    {
        [Header("기본 정보")]
        public string zoneId;             // "zone_east_1" 등 고유 식별자
        public string zoneName;           // "동쪽 확장지 1단계" (UI 표시용)
        public int sortOrder;             // 해금 순서 (1~4), UI 정렬용

        [Header("해금 조건")]
        public int requiredLevel;         // 해금 필요 레벨 (→ see docs/systems/farm-expansion.md, DES-012)
        public int unlockCost;            // 해금 비용 골드 (→ see docs/systems/farm-expansion.md, DES-012)

        [Header("구역 구성")]
        public ZoneType zoneType;         // 구역 용도
        public Vector2Int[] tilePositions;// 이 구역에 속하는 타일 좌표 목록
        public ObstacleEntry[] obstacleMap;// 초기 장애물 배치

        [Header("비주얼")]
        public Material lockedOverlayMaterial;  // 잠금 상태 오버레이
        public GameObject unlockVFXPrefab;      // 해금 시 이펙트
    }

    [System.Serializable]
    public struct ObstacleEntry
    {
        public Vector2Int localPosition;  // 구역 내 상대 좌표
        public ObstacleType type;         // 장애물 종류
        public int maxHP;                 // 제거 필요 타격 수
        public string[] lootDropIds;      // 제거 시 드랍 아이템 ID 목록 (→ see docs/systems/farm-expansion.md 섹션 3.1~3.4)
        public GameObject obstaclePrefab; // 장애물 3D 모델
    }
}
```

**ZoneData와 FarmGrid의 좌표 관계**:
- ZoneData.tilePositions는 FarmGrid의 절대 좌표를 사용한다
- 초기 8x8 구역(zone_home)은 (0,0)~(7,7) 좌표를 차지한다
- 확장 구역은 8x8 바깥의 좌표를 할당받는다 (구역별 절대 좌표 → see docs/systems/farm-expansion.md 섹션 1.2~1.3, DES-012)
- FarmGrid는 초기화 시 최대 농장 크기(→ see docs/systems/farm-expansion.md 섹션 1.1)에 해당하는 모든 좌표를 미리 할당하되, 해금되지 않은 구역의 타일은 비활성(inactive) 상태로 둔다

[RISK] FarmGrid가 초기부터 최대 크기(→ see DES-012 섹션 1.1)를 사전 할당하면 메모리 사용이 증가한다. 대안: 구역 해금 시 동적으로 타일을 생성. 그러나 전체 타일은 ~576개이므로(타일당 ~200bytes, 총 ~115KB) 사전 할당 방식을 채택한다.

---

### 4. 타일 구매/해금 흐름

#### 4.1 시퀀스 다이어그램

```
Player                FarmZoneManager     ProgressionManager     EconomyManager      FarmGrid
  │                        │                     │                    │                  │
  │ [구역 구매 버튼 클릭]    │                     │                    │                  │
  ├───TryUnlockZone(zoneId)▶│                     │                    │                  │
  │                        │                     │                    │                  │
  │                        │──IsUnlocked(        │                    │                  │
  │                        │  FarmExpansion,      │                    │                  │
  │                        │  zoneId)────────────▶│                    │                  │
  │                        │                     │                    │                  │
  │                        │◀──false (미해금) ────│                    │                  │
  │                        │   또는 true          │                    │                  │
  │                        │                     │                    │                  │
  │                        │  [미해금 시 → 레벨 체크]                   │                  │
  │                        │──GetZoneState()─────▶│                    │                  │
  │                        │  requiredLevel 비교   │                    │                  │
  │                        │                     │                    │                  │
  │                        │  [레벨 충족 시]       │                    │                  │
  │                        │──────────────────────────SpendGold(cost)─▶│                  │
  │                        │                     │                    │                  │
  │                        │◀─────────────────────────bool success ──│                  │
  │                        │                     │                    │                  │
  │                        │  [골드 차감 성공 시]   │                    │                  │
  │                        │──────────────────────────────────────────── ActivateZone   │
  │                        │                     │                    │  Tiles(pos[])───▶│
  │                        │                     │                    │                  │
  │                        │──ZoneEvents.OnZoneUnlocked 발행 ────────────────────────────▶
  │                        │                     │                    │                  │
  │◀──true (성공) ─────────│                     │                    │                  │
```

#### 4.2 TryUnlockZone 의사 코드

```csharp
// illustrative
public bool TryUnlockZone(string zoneId)
{
    // 1) 구역 데이터 검증
    ZoneData zone = GetZoneData(zoneId);
    if (zone == null) return false;

    // 2) 이미 해금된 구역인지 확인
    if (IsZoneUnlocked(zoneId)) return false;

    // 3) 레벨 요건 확인
    // → see docs/systems/progression-architecture.md 섹션 3.4
    if (!ProgressionManager.Instance.IsUnlocked(UnlockType.FarmExpansion, zoneId))
    {
        ZoneEvents.OnZoneUnlockFailed?.Invoke(zoneId, ZoneUnlockFailReason.LevelInsufficient);
        return false;
    }

    // 4) 골드 차감 시도
    // unlockCost → see docs/systems/farm-expansion.md (DES-012)
    // SpendGold API → see docs/systems/economy-architecture.md 섹션 1
    if (!EconomyManager.Instance.SpendGold(zone.unlockCost, $"zone_unlock_{zoneId}"))
    {
        ZoneEvents.OnZoneUnlockFailed?.Invoke(zoneId, ZoneUnlockFailReason.InsufficientGold);
        return false;
    }

    // 5) 구역 활성화
    _zoneStates[zoneId].state = ZoneState.Unlocked;
    FarmGrid.Instance.ActivateZoneTiles(zone.tilePositions);

    // 6) 장애물 인스턴스 생성
    SpawnObstacles(zone);

    // 7) 이벤트 발행
    ZoneEvents.OnZoneUnlocked?.Invoke(zoneId, zone);

    // 8) 해금 상태 등록 (ProgressionManager)
    ProgressionManager.Instance.RegisterUnlock(UnlockType.FarmExpansion, zoneId);

    return true;
}
```

#### 4.3 해금 실패 사유 enum

```csharp
// illustrative
namespace SeedMind.Farm
{
    public enum ZoneUnlockFailReason
    {
        LevelInsufficient,    // 레벨 미달
        InsufficientGold,     // 골드 부족
        AlreadyUnlocked,      // 이미 해금됨
        PrerequisiteZone      // 선행 구역 미해금 (향후 확장용)
    }
}
```

---

### 5. 개간(Clearing) 시스템

#### 5.1 설계 결정: FarmZoneManager에 통합

개간 로직을 별도 ClearingManager로 분리하지 않고 FarmZoneManager에 통합한다.

**근거**:
- 장애물은 구역 해금과 직접 연결되며, 구역 외부에는 장애물이 존재하지 않는다
- 장애물 상태는 구역 상태의 부분 집합이다 (모든 장애물 제거 = FullyCleared)
- 별도 매니저를 두면 구역-장애물 동기화 복잡도가 불필요하게 증가한다

#### 5.2 도구별 처리 가능 장애물 매핑

| 장애물 | 필요 도구 | 최소 도구 등급 | HP | 드랍 |
|--------|-----------|---------------|-----|------|
| Weed (잡초) | Hoe / Hand | Basic | 1 | 없음 또는 섬유 |
| Rock (돌) | Hoe | Reinforced | 2 | 돌 조각 |
| Stump (그루터기) | Hoe | Legendary | 3 | 목재 |
| Tree (나무) | -- | -- | -- | 제거 불가 (향후 도끼 도입 시 변경) |
| Boulder (큰 바위) | -- | -- | -- | 제거 불가 (향후 폭탄/특수 도구) |

(HP, 드랍 아이템의 구체적 수치 → see docs/systems/farm-expansion.md, DES-012 작성 시 확정)

[OPEN] Tree와 Boulder를 제거 불가로 유지할지, 구역 해금 시 자동 제거할지. 현재 설계: 제거 불가 장애물은 해당 타일을 영구적으로 사용 불가 상태로 만든다. 대안: 구역 내 Tree/Boulder가 차지하는 타일 수를 제한(최대 2개)하여 손해를 최소화.

#### 5.3 ClearObstacle 흐름

```csharp
// illustrative
public ClearResult ClearObstacle(Vector2Int tilePos, ToolType tool, int toolTier)
{
    // 1) 해당 타일에 장애물이 있는지 확인
    if (!_obstacleInstances.TryGetValue(tilePos, out var obstacle))
        return ClearResult.NoObstacle;

    if (obstacle.isCleared)
        return ClearResult.AlreadyCleared;

    // 2) 도구 적합성 확인
    if (!CanToolClear(obstacle.entry.type, tool, toolTier))
        return ClearResult.WrongTool;

    // 3) HP 차감
    obstacle.currentHP -= 1;

    if (obstacle.currentHP > 0)
    {
        ZoneEvents.OnObstacleHit?.Invoke(tilePos, obstacle.currentHP);
        return ClearResult.Hit;
    }

    // 4) 장애물 제거 완료
    obstacle.isCleared = true;
    _obstacleInstances.Remove(tilePos);

    // 5) 드랍 처리
    // lootDropIds → see docs/systems/farm-expansion.md (DES-012)
    if (obstacle.entry.lootDropIds != null && obstacle.entry.lootDropIds.Length > 0)
    {
        foreach (string lootId in obstacle.entry.lootDropIds)
            DropManager.SpawnDrop(tilePos, lootId);  // → see inventory-architecture.md
    }

    // 6) 이벤트 발행
    ZoneEvents.OnObstacleCleared?.Invoke(tilePos, obstacle.entry.type);

    // 7) 구역 완전 개간 체크
    string zoneId = GetZoneIdForTile(tilePos);
    CheckZoneFullyCleared(zoneId);

    return ClearResult.Cleared;
}

private bool CanToolClear(ObstacleType obstacle, ToolType tool, int toolTier)
{
    // → see 섹션 5.2 도구별 처리 가능 장애물 매핑
    switch (obstacle)
    {
        case ObstacleType.Weed:
            return tool == ToolType.Hoe || tool == ToolType.Hand;
        case ObstacleType.Rock:
            return tool == ToolType.Hoe && toolTier >= 2; // Reinforced+
        case ObstacleType.Stump:
            return tool == ToolType.Hoe && toolTier >= 3; // Legendary
        case ObstacleType.Tree:
        case ObstacleType.Boulder:
            return false; // 현재 제거 불가
        default:
            return false;
    }
}
```

#### 5.4 ClearResult enum

```csharp
// illustrative
namespace SeedMind.Farm
{
    public enum ClearResult
    {
        NoObstacle,      // 해당 타일에 장애물 없음
        AlreadyCleared,  // 이미 제거됨
        WrongTool,       // 부적합한 도구/등급
        Hit,             // 타격 성공 (HP 잔여)
        Cleared          // 제거 완료
    }
}
```

---

### 6. FarmGrid 확장 메서드

기존 farming-architecture.md의 FarmGrid에 구역 시스템 지원을 위한 메서드를 추가한다. 이는 FarmGrid의 **확장**이며 기존 API를 변경하지 않는다.

```csharp
// illustrative -- FarmGrid에 추가되는 메서드
public partial class FarmGrid : MonoBehaviour
{
    // 기존 API: WorldToGrid(), GetTile(), AllTiles 등은 유지

    /// <summary>
    /// 지정된 타일 좌표 목록을 활성화한다.
    /// 타일 GameObject를 SetActive(true)하고 상태를 Empty로 초기화한다.
    /// </summary>
    public void ActivateZoneTiles(Vector2Int[] positions)
    {
        foreach (var pos in positions)
        {
            FarmTile tile = GetTile(pos);
            if (tile == null) continue;
            tile.gameObject.SetActive(true);
            tile.Initialize(TileState.Empty);
        }
    }

    /// <summary>
    /// 지정된 타일 좌표 목록을 비활성화한다 (구역 잠금 시).
    /// </summary>
    public void DeactivateZoneTiles(Vector2Int[] positions)
    {
        foreach (var pos in positions)
        {
            FarmTile tile = GetTile(pos);
            if (tile == null) continue;
            tile.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 전체 가능한 그리드 크기(16x16)를 사전 할당하되,
    /// 초기 구역 외 타일은 비활성 상태로 생성한다.
    /// </summary>
    public void InitializeFullGrid(int maxWidth, int maxHeight)
    {
        // maxWidth, maxHeight → see docs/systems/farming-system.md 섹션 1
        // 초기에 FarmZoneManager가 호출하여 전체 그리드를 생성
        // 이후 구역 해금 시 ActivateZoneTiles()로 활성화
    }
}
```

---

### 7. 이벤트 시스템

#### 7.1 ZoneEvents 정적 이벤트 허브

```csharp
// illustrative
namespace SeedMind.Farm
{
    /// <summary>
    /// 농장 확장/개간 관련 이벤트를 중앙 집중 관리.
    /// 기존 FarmEvents(farming-architecture.md 섹션 6.1)와 분리하여
    /// 구역 시스템 전용 이벤트를 정의한다.
    /// </summary>
    public static class ZoneEvents
    {
        // 구역 해금
        public static Action<string, ZoneData> OnZoneUnlocked;        // (zoneId, zoneData)
        public static Action<string, ZoneUnlockFailReason> OnZoneUnlockFailed; // (zoneId, reason)

        // 장애물
        public static Action<Vector2Int, int> OnObstacleHit;          // (position, remainingHP)
        public static Action<Vector2Int, ObstacleType> OnObstacleCleared; // (position, type)

        // 구역 완전 개간
        public static Action<string> OnZoneFullyCleared;              // (zoneId)
    }
}
```

#### 7.2 이벤트 소비자 매핑

| 이벤트 | 소비자 | 용도 |
|--------|--------|------|
| `OnZoneUnlocked` | UI (ZoneUnlockPopup) | "동쪽 확장지 해금!" 연출 |
| `OnZoneUnlocked` | ProgressionManager | 농장 확장 XP 부여 (→ see docs/balance/progression-curve.md) |
| `OnZoneUnlocked` | SaveManager | 자동 저장 트리거 |
| `OnZoneUnlockFailed` | UI (HUD) | "레벨이 부족합니다" / "골드가 부족합니다" 피드백 |
| `OnObstacleHit` | VFX/SFX | 타격 파티클, 효과음 |
| `OnObstacleCleared` | PlayerInventory | 드랍 아이템 수령 |
| `OnObstacleCleared` | UI (HUD) | "+목재" 팝업 |
| `OnZoneFullyCleared` | UI (Achievement) | "구역 개간 완료" 알림 |
| `OnZoneFullyCleared` | QuestManager | 관련 퀘스트 진행도 갱신 |

#### 7.3 기존 이벤트와의 관계

- ZoneEvents는 FarmEvents(farming-architecture.md)와 **별도 클래스**이다
- 구역 해금 후 타일이 활성화되면 해당 타일은 기존 FarmEvents의 상태 전환 이벤트를 정상적으로 발행한다
- 장애물이 있는 타일에서 FarmTile의 도구 액션은 FarmZoneManager.ClearObstacle()로 우선 라우팅된다

---

### 8. FarmTile 도구 액션 라우팅

장애물이 있는 타일에서 도구를 사용하면, 기존 FarmTile.TryTill() 등이 아닌 FarmZoneManager.ClearObstacle()이 호출되어야 한다. ToolSystem에서 이를 분기한다.

```
ToolSystem.UseCurrentTool(Vector3 worldPos)
    │
    ├── 1) worldPos → FarmGrid.WorldToGrid(worldPos) → Vector2Int gridPos
    │
    ├── 2) FarmGrid.GetTile(gridPos) → FarmTile tile
    │
    ├── 3) [NEW] 장애물 체크
    │       ObstacleInstance obs = FarmZoneManager.Instance.GetObstacleAt(gridPos)
    │       if (obs != null && !obs.isCleared):
    │           → FarmZoneManager.ClearObstacle(gridPos, currentTool.toolType, currentTool.tier)
    │           → return (기존 타일 액션 실행하지 않음)
    │
    ├── 4) [기존] switch (currentTool.toolType)
    │       → 기존 로직 그대로 (farming-architecture.md 섹션 3.1)
    │
    └── 5) 결과에 따라 도구 애니메이션/효과음 재생
```

---

### 9. 세이브/로드 통합

#### 9.1 ZoneSaveData 스키마 (PATTERN-005 준수)

**Part A: JSON 스키마**

```json
{
  "zones": [
    {
      "zoneId": "zone_east_1",
      "isUnlocked": true,
      "obstacles": [
        {
          "posX": 8,
          "posY": 2,
          "isCleared": true
        },
        {
          "posX": 9,
          "posY": 3,
          "isCleared": false,
          "currentHP": 1
        }
      ]
    },
    {
      "zoneId": "zone_east_2",
      "isUnlocked": false,
      "obstacles": []
    }
  ]
}
```

**Part B: C# 클래스** (PATTERN-005: JSON과 필드명/필드 수 일치)

```csharp
// illustrative
namespace SeedMind.Save
{
    [System.Serializable]
    public class ZoneSaveData
    {
        public ZoneEntrySaveData[] zones;   // 구역별 세이브 데이터 배열
    }

    [System.Serializable]
    public class ZoneEntrySaveData
    {
        public string zoneId;               // ZoneData.zoneId 참조
        public bool isUnlocked;             // 해금 여부
        public ObstacleSaveData[] obstacles; // 장애물 상태 배열
    }

    [System.Serializable]
    public class ObstacleSaveData
    {
        public int posX;                    // 타일 X 좌표
        public int posY;                    // 타일 Y 좌표
        public bool isCleared;              // 제거 여부
        public int currentHP;              // 남은 HP (isCleared=true이면 0)
    }
}
```

**PATTERN-005 검증**:
- ZoneSaveData: JSON 필드 1개(zones) = C# 필드 1개(zones) -- 일치
- ZoneEntrySaveData: JSON 필드 3개(zoneId, isUnlocked, obstacles) = C# 필드 3개 -- 일치
- ObstacleSaveData: JSON 필드 4개(posX, posY, isCleared, currentHP) = C# 필드 4개 -- 일치

#### 9.2 GameSaveData 필드 추가

기존 GameSaveData(save-load-architecture.md 섹션 2)에 다음 필드를 추가한다:

```csharp
// illustrative -- GameSaveData에 추가
public class GameSaveData
{
    // ... 기존 필드 (→ see save-load-architecture.md 섹션 2.2) ...

    public ZoneSaveData farmZones;    // → see 본 문서 섹션 9.1 (null 허용, 이전 세이브 호환)
}
```

**null-safe 처리**: farmZones가 null인 경우(이전 버전 세이브 파일 로드 시), FarmZoneManager는 초기 구역만 해금된 기본 상태로 초기화한다.

#### 9.3 SaveLoadOrder 배정

| 시스템 | SaveLoadOrder | 근거 |
|--------|:------------:|------|
| FarmGrid | 40 | 기존 (→ see save-load-architecture.md) |
| **FarmZoneManager** | **45** | FarmGrid(40) 이후 -- 타일 활성화를 위해 FarmGrid가 먼저 복원되어야 함 |
| PlayerController | 50 | 기존 |

FarmZoneManager의 SaveLoadOrder는 45로 배정한다. FarmGrid(40)가 타일 배열을 복원한 후, FarmZoneManager(45)가 구역 해금 상태에 따라 타일을 활성화/비활성화한다.

#### 9.4 저장/로드 흐름

```
[저장 흐름]
SaveManager.Save()
    → FarmGrid.GetSaveData()           // SaveLoadOrder 40: 전체 타일 상태
    → FarmZoneManager.GetSaveData()    // SaveLoadOrder 45: 구역 해금 + 장애물 상태
        → foreach zone in _zones:
            ZoneEntrySaveData {
                zoneId, isUnlocked,
                obstacles[] (미제거 장애물만 저장)
            }

[로드 흐름]
SaveManager.Load()
    → FarmGrid.LoadSaveData()          // SaveLoadOrder 40: 16x16 타일 배열 복원 (전부 inactive)
    → FarmZoneManager.LoadSaveData()   // SaveLoadOrder 45:
        → foreach zoneEntry in savedZones:
            if zoneEntry.isUnlocked:
                FarmGrid.ActivateZoneTiles(zone.tilePositions)
                장애물 인스턴스 복원 (savedObstacles 기반)
```

---

### 10. 의존성 및 모듈 구조

#### 10.1 asmdef 의존성

FarmZoneManager는 기존 `SeedMind.Farm.asmdef`에 포함된다.

| 모듈 | 의존 대상 | 근거 |
|------|-----------|------|
| `SeedMind.Farm` | `SeedMind.Core` | 이벤트, 싱글턴 패턴 |
| `SeedMind.Farm` | `SeedMind.Economy` (이벤트 경유) | SpendGold 호출 -- Singleton 접근 |
| `SeedMind.Farm` | `SeedMind.Level` (이벤트 경유) | IsUnlocked 체크 -- Singleton 접근 |

[RISK] FarmZoneManager가 EconomyManager와 ProgressionManager를 직접 참조하면 asmdef 순환 참조가 발생할 수 있다. 현재 설계에서는 Singleton 패턴으로 런타임 접근하므로 컴파일 의존성은 발생하지 않지만, 타입 참조가 필요한 경우 인터페이스(IEconomyService, IProgressionService)를 Core에 정의하고 각 매니저가 구현하는 방식으로 분리해야 한다.

#### 10.2 파일 배치

```
Assets/_Project/Scripts/Farm/
├── FarmGrid.cs                    // 기존 (partial class 확장)
├── FarmTile.cs                    // 기존
├── FarmEvents.cs                  // 기존
├── FarmZoneManager.cs             // 신규 -- SeedMind.Farm
├── ZoneData.cs                    // 신규 -- SeedMind.Farm (SO)
├── ZoneEvents.cs                  // 신규 -- SeedMind.Farm
├── ZoneState.cs                   // 신규 -- SeedMind.Farm (enum)
├── ZoneType.cs                    // 신규 -- SeedMind.Farm (enum)
├── ObstacleType.cs                // 신규 -- SeedMind.Farm (enum)
├── ObstacleEntry.cs               // 신규 -- SeedMind.Farm (struct, SO 내부)
├── ObstacleInstance.cs            // 신규 -- SeedMind.Farm (런타임 클래스)
├── ZoneRuntimeState.cs            // 신규 -- SeedMind.Farm (런타임 클래스)
├── ClearResult.cs                 // 신규 -- SeedMind.Farm (enum)
└── ZoneUnlockFailReason.cs        // 신규 -- SeedMind.Farm (enum)

Assets/_Project/Scripts/Save/
├── ZoneSaveData.cs                // 신규 -- SeedMind.Save
├── ZoneEntrySaveData.cs           // 신규 -- SeedMind.Save
└── ObstacleSaveData.cs            // 신규 -- SeedMind.Save
```

#### 10.3 씬 계층 구조 내 배치

```
--- MANAGERS ---
├── GameManager          (DontDestroyOnLoad)
├── TimeManager          (DontDestroyOnLoad)
├── SaveManager          (DontDestroyOnLoad)
├── ProgressionManager   (DontDestroyOnLoad)
└── FarmZoneManager      (DontDestroyOnLoad)    ← 신규

--- FARM ---
├── FarmGrid
│   ├── Tile_0_0 ~ ...           (최대 크기 사전 할당 → see DES-012 섹션 1.1, 초기 대부분 inactive)
│   └── ...
└── ObstacleContainer            ← 신규: 장애물 프리팹 부모
    ├── Obstacle_8_2 (Rock)
    ├── Obstacle_9_3 (Stump)
    └── ...
```

---

## Part II -- MCP 태스크 시퀀스

FarmZoneManager 구현을 위한 Unity MCP 태스크. 상세 MCP 태스크는 `docs/mcp/farm-expansion-tasks.md`에 독립 문서화한다.

### Phase A: ZoneData ScriptableObject 생성 (MCP 5단계)

```
Step A-1: Assets/_Project/Data/Zones/ 폴더 생성
          MCP: create_folder("Assets/_Project/Data/Zones")

Step A-2: ZoneData SO 스크립트 생성 (코드 작성 선행)
          → ZoneData.cs, ObstacleEntry.cs 작성 후 컴파일 확인

Step A-3: ZoneData SO 인스턴스 생성 (구역 ID → see docs/systems/farm-expansion.md 섹션 1.3)
          → SO_Zone_Home       (초기 농장,   zoneId="zone_home",         sortOrder=0)
          → SO_Zone_SouthPlain (1단계 확장,  zoneId="zone_south_plain",  sortOrder=1)
          → SO_Zone_NorthPlain (2단계 확장,  zoneId="zone_north_plain",  sortOrder=2)
          → SO_Zone_EastForest (3단계 확장,  zoneId="zone_east_forest",  sortOrder=3)
          → SO_Zone_SouthMeadow(4단계 확장,  zoneId="zone_south_meadow", sortOrder=4)
          → SO_Zone_Pond       (5단계 확장,  zoneId="zone_pond",         sortOrder=5)
          → SO_Zone_Orchard    (6단계 확장,  zoneId="zone_orchard",      sortOrder=6)
          → 각 SO의 requiredLevel, unlockCost 설정
            (→ see docs/systems/farm-expansion.md 섹션 2.1, DES-012)

Step A-4: 각 ZoneData SO의 tilePositions 배열 설정
          → zone_home: (0,0)~(7,7) = 64타일
          → 확장 구역 좌표 → see docs/systems/farm-expansion.md 섹션 1.2 구역 배치도, DES-012

Step A-5: 장애물 프리팹 Placeholder 생성
          → PFB_Obstacle_Weed (녹색 평면 Quad, scale 0.3)
          → PFB_Obstacle_Rock (회색 Sphere, scale 0.4)
          → PFB_Obstacle_Stump (갈색 Cylinder, scale 0.5)
          → 각 ZoneData.obstacleMap에 장애물 배치 설정
            (→ see docs/systems/farm-expansion.md, DES-012 for 배치 수량)
```

### Phase B: FarmZoneManager MonoBehaviour 생성 (MCP 4단계)

```
Step B-1: FarmZoneManager.cs 스크립트 작성 (코드 작성 선행)
          → 섹션 1, 4.2의 설계에 따른 구현

Step B-2: SCN_Farm 씬에서 FarmZoneManager GameObject 생성
          MCP: create_gameobject("FarmZoneManager")
          → parent: "--- MANAGERS ---" 하위
          → FarmZoneManager.cs 컴포넌트 부착

Step B-3: FarmZoneManager Inspector 설정
          → _zones 배열에 SO_Zone_Home ~ SO_Zone_Orchard 할당 (7개)
          → _farmGrid 참조: FarmGrid 오브젝트

Step B-4: Play Mode 테스트
          → Console: "FarmZoneManager initialized, zones=7"
          → IsZoneUnlocked("zone_home") == true 확인
          → IsZoneUnlocked("zone_south_plain") == false 확인
          → GetZoneState("zone_south_plain") == Unlockable 확인 (레벨 요건 없음)
```

### Phase C: FarmGrid 연동 -- ActivateZoneTiles (MCP 3단계)

```
Step C-1: FarmGrid.cs에 ActivateZoneTiles / DeactivateZoneTiles 메서드 추가
          → 섹션 6의 설계에 따른 구현

Step C-2: FarmGrid.InitializeFullGrid(16, 16) 호출 확인
          → 기존 8x8 → 16x16 사전 할당으로 변경
          → 초기 구역(zone_initial) 타일만 active, 나머지 inactive

Step C-3: Play Mode 통합 테스트
          → TryUnlockZone("zone_east_1") 호출 (충분한 골드/레벨 설정 후)
          → 새 타일 활성화 확인 (SetActive true)
          → ZoneEvents.OnZoneUnlocked 이벤트 발행 확인
          → Console 로그: "Zone zone_east_1 unlocked, activated N tiles"
```

### Phase D: UI 연동 -- Zone 해금 팝업 (MCP 4단계)

```
Step D-1: Canvas_HUD에 ZoneUnlockPanel UI 오브젝트 생성
          → 구역 이름, 비용, 레벨 요건 텍스트
          → 해금 버튼 (Buy / Locked)
          → 장애물 현황 표시 (N/M 제거됨)

Step D-2: ZoneMapUI 컴포넌트 생성 (구역 선택 미니맵)
          → 각 구역을 색상으로 표시 (Locked=빨강, Unlockable=노랑, Unlocked=초록, FullyCleared=파랑)
          → 클릭 시 ZoneUnlockPanel 표시

Step D-3: ZoneEvents 구독 연결
          → OnZoneUnlocked → 해금 연출 (VFX + 사운드)
          → OnZoneUnlockFailed → 실패 피드백 메시지
          → OnZoneFullyCleared → 완전 개간 축하 메시지

Step D-4: Play Mode UI 테스트
          → 구역 선택 → 해금 버튼 → 골드 차감 → 타일 활성화 확인
          → 레벨 미달 시 "레벨이 부족합니다" 메시지 확인
```

### Phase E: 세이브/로드 연동 (MCP 3단계)

```
Step E-1: ZoneSaveData 관련 클래스 작성 (코드 작성 선행)
          → 섹션 9.1의 JSON/C# 스키마에 따른 구현

Step E-2: FarmZoneManager에 ISaveable 인터페이스 구현
          → SaveLoadOrder = 45
          → GetSaveData(): 현재 구역 상태 + 장애물 상태 직렬화
          → LoadSaveData(): 구역 해금 복원 + 장애물 인스턴스 복원

Step E-3: GameSaveData에 farmZones 필드 추가
          → null-safe 처리 (이전 세이브 호환)
          → Play Mode 저장 → 로드 → 구역 상태 유지 확인
          → Console 로그: "FarmZoneManager loaded, unlocked zones: N/5"
```

---

## Open Questions

1. [OPEN] ZoneType의 Orchard/Pasture/Greenhouse/Pond는 Phase 1에서 미구현. 콘텐츠 확장 시점 미정 (섹션 2.2)
2. [OPEN] Tree/Boulder 장애물의 처리 방식 -- 영구 사용 불가 vs 자동 제거 vs 향후 도구 도입 (섹션 5.2)
3. [OPEN] 도끼(Axe)/곡괭이(Pickaxe) ToolType 추가 여부. 현재는 호미 등급별 개간으로 대체 (섹션 2.3)
4. [OPEN] 구역 확장 방향/레이아웃은 DES-012(게임 디자인)에서 확정 필요. 현재 East/South로 가정 (Phase A Step A-3)
5. [OPEN] 초기 구역(zone_initial)에도 장애물을 배치할지 여부. 배치 시 튜토리얼에서 개간을 학습시킬 수 있으나, 게임 시작이 느려질 수 있음

---

## Risks

1. [RISK] **ToolType enum 미확장**: 도끼/곡괭이 없이 호미로 모든 장애물을 처리하면 게임플레이가 단조로워질 수 있다. ToolType 확장 시 farming-architecture.md의 ToolType enum과 모든 switch 문 업데이트 필요 (섹션 2.3)
2. [RISK] **FarmGrid 16x16 사전 할당**: 비활성 타일 192개(256-64)의 메모리 부담은 미미하나, 각 타일에 부착된 MonoBehaviour가 많으면 초기 로드 시간 증가 가능. 프로파일링 필요 (섹션 3)
3. [RISK] **asmdef 순환 참조**: FarmZoneManager → EconomyManager/ProgressionManager 직접 참조 시 발생. 인터페이스 분리 또는 Singleton 런타임 접근으로 회피하되, 코드 리뷰에서 감시 필요 (섹션 10.1)
4. [RISK] **static event 구독 누수**: ZoneEvents도 FarmEvents와 동일한 위험. 씬 전환 시 이벤트 초기화 루틴에 ZoneEvents 포함 필요 (→ see farming-architecture.md 섹션 6.3)
5. [RISK] **DES-012 미완성**: 이 아키텍처 문서는 DES-012(농장 확장 게임 디자인)와 병렬 작성 중이다. DES-012의 수치/레이아웃 확정 후 본 문서의 ZoneData SO 인스턴스와 MCP 태스크를 업데이트해야 한다

---

## Cross-references

| 관련 문서 | 연관 내용 |
|-----------|-----------|
| `docs/systems/farming-architecture.md` | FarmGrid, FarmTile, TileState, ToolType 기존 정의 |
| `docs/systems/farming-system.md` 섹션 1 | 농장 그리드 크기, 확장 방식/비용 (canonical) |
| `docs/systems/economy-architecture.md` 섹션 1 | EconomyManager.SpendGold() API |
| `docs/systems/economy-system.md` 섹션 3.3 | 농장 확장 비용 테이블 (canonical) |
| `docs/systems/save-load-architecture.md` 섹션 2~8 | GameSaveData 구조, ISaveable, SaveLoadOrder |
| `docs/systems/progression-architecture.md` 섹션 3 | UnlockType.FarmExpansion, IsUnlocked API |
| `docs/pipeline/data-pipeline.md` 섹션 2.4 | BuildingData SO 구조 (ZoneData 패턴 참조) |
| `docs/systems/farm-expansion.md` (DES-012, 병렬 작성 중) | 구역별 수치, 레이아웃, 장애물 배치 (canonical) |
| `docs/content/npcs.md` 섹션 3.2 | 목수 NPC -- 농장 확장 서비스 제공자 |
| `docs/systems/quest-system.md` 섹션 3.1 | "농장 확장의 시작" 퀘스트 (경작지 20타일 이상) |
| `docs/balance/progression-curve.md` | 농장 확장 XP (각 단계 25 XP) |

---

*이 문서는 Claude Code가 ARC-023 태스크에 따라 작성했습니다.*
