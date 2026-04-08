# 농장 장식 시스템 — 기술 아키텍처 (ARC-043)

> 작성: Claude Code (Sonnet 4.6) | 2026-04-08
> 문서 ID: ARC-043
> 설계 기준: DES-023 (`docs/systems/decoration-system.md`)

---

## 1. Context

이 문서는 DES-023(`docs/systems/decoration-system.md`)에서 설계된 농장 장식 시스템의 **기술 아키텍처**를 정의한다. 장식품 배치(Place) / 철거(Remove) 런타임 흐름, ScriptableObject 에셋 스키마, 세이브 데이터 구조, Unity Tilemap 연동 방식, MCP 구현 태스크 개요를 포함한다.

**참조 설계 문서**: `docs/systems/decoration-system.md` (DES-023)
- 카테고리 분류(Fence/Path/Light/Ornament/WaterDecor), 배치 제약, 경제 연동, 해금 조건 등 모든 설계 수치의 canonical 출처

**설계 원칙**:
- 장식 시스템은 핵심 루프와 독립된 표현 레이어 — `FarmGrid`/`BuildingManager` 등 기존 시스템에 읽기 전용 의존만 허용
- 데이터(DecorationItemData SO)와 동작(DecorationManager MonoBehaviour)을 명확히 분리
- ISaveable 패턴으로 기존 세이브/로드 파이프라인에 통합 (→ see `docs/systems/save-load-architecture.md`)

---

## 2. Part I — 클래스 설계

---

### 2.1 DecorationManager

**역할**: 장식품 배치·철거 흐름 중앙 제어, 배치 가능 여부 검사, 인스턴스 상태 관리, ISaveable 구현

**클래스 다이어그램**:

```
DecorationManager (MonoBehaviour, Singleton, ISaveable)
│
├── [Fields]
│   ├── _items: Dictionary<int, DecorationInstance>   // key = instanceId
│   ├── _nextInstanceId: int
│   ├── _decoConfig: DecorationConfig                 // → see 섹션 2.3
│   ├── _farmGrid: FarmGrid                           // 읽기 전용 참조
│   ├── _fenceLayer: Tilemap                          // 울타리 edge 레이어
│   ├── _pathLayer: Tilemap                           // 경로 오버레이 레이어
│   ├── _objectLayer: Transform                       // 조명/장식물 오브젝트 부모
│
├── [ISaveable 구현]
│   + SaveLoadOrder => 57
│   + GetSaveData(): object  → DecorationSaveData
│   + LoadSaveData(object data)
│
├── [배치 흐름]
│   + CanPlace(DecorationItemData item, Vector3Int cell, EdgeDirection? edge): bool
│   + Place(DecorationItemData item, Vector3Int cell, EdgeDirection? edge): int   // instanceId 반환
│   + Remove(int instanceId): void
│
├── [배치 가능 여부 내부 검사]
│   - IsOccupied(Vector3Int cell, int width, int height): bool
│   - IsFarmland(Vector3Int cell): bool
│   - IsWaterSource(Vector3Int cell): bool
│   - IsBuildingTile(Vector3Int cell): bool
│   - IsZoneUnlocked(Vector3Int cell): bool
│   - IsEdgeOccupied(Vector3Int cell, EdgeDirection edge): bool    // 울타리 전용
│
└── [이벤트]
    + static OnDecorationPlaced: Action<DecorationPlacedInfo>
    + static OnDecorationRemoved: Action<int>   // instanceId
```

**배치(Place) 흐름 상세**:

```
[플레이어] 장식 배치 요청
    │
    ▼
DecorationManager.CanPlace(item, cell, edge)
    │  ├─ 에셋 isEdgePlaced 분기
    │  │     true  → IsEdgeOccupied(cell, edge)
    │  │     false → IsOccupied(cell, item.tileWidthX, item.tileHeightZ)
    │  ├─ IsFarmland / IsWaterSource / IsBuildingTile / IsZoneUnlocked 검사
    │  └─ 해금 레벨 / 계절 제약 검사
    │
    ├─ 불가 → false 반환 → UI 빨간 하이라이트
    └─ 가능 → DecorationManager.Place() 진행
                  │
                  ├─ DecorationInstance 생성 (_items에 등록)
                  ├─ category별 렌더링 처리
                  │     Fence     → _fenceLayer.SetTile (edge sprite)
                  │     Path      → _pathLayer.SetTile (floor tile)
                  │     Light/Ornament/WaterDecor
                  │               → Instantiate(item.prefab) 아래 _objectLayer
                  ├─ OnDecorationPlaced 이벤트 발행
                  └─ instanceId 반환
```

**철거(Remove) 흐름**:

```
[플레이어] 장식물 클릭 → 철거 선택 (E키)
    │
    ▼
DecorationManager.Remove(instanceId)
    │
    ├─ _items에서 DecorationInstance 조회
    ├─ category별 렌더링 제거
    │     Fence / Path → Tilemap.SetTile(null)
    │     그 외         → Destroy(instance.gameObject)
    ├─ _items에서 제거
    └─ OnDecorationRemoved 이벤트 발행
         (골드 환불 없음 — → see docs/systems/decoration-system.md 섹션 3.2)
```

---

### 2.2 DecorationItemData SO 스키마

**클래스 정의** (ScriptableObject):

```csharp
// illustrative — namespace SeedMind.Decoration.Data
namespace SeedMind.Decoration.Data
{
    [CreateAssetMenu(menuName = "SeedMind/Decoration/DecorationItemData")]
    public class DecorationItemData : ScriptableObject
    {
        // --- 식별
        public string   itemId;           // 예: "FenceStone"
        public string   displayName;      // 예: "돌 울타리"
        public Sprite   icon;

        // --- 카테고리
        public DecoCategoryType category; // → see docs/systems/decoration-system.md 섹션 1.2

        // --- 가격
        public int      buyPrice;         // → see docs/systems/decoration-system.md 섹션 2.1~2.5

        // --- 배치 방식
        public bool     isEdgePlaced;     // true = 울타리(edge), false = tile 점유
        public int      tileWidthX;       // 점유 타일 X (isEdgePlaced=true 이면 0)
        public int      tileHeightZ;      // 점유 타일 Z (isEdgePlaced=true 이면 0)

        // --- 해금
        public int      unlockLevel;      // 0 = 시작부터 가능
                                          // → see docs/systems/decoration-system.md 섹션 4.2
        public string   unlockZoneId;     // 해금 구역 ID, 없으면 ""
                                          // → see docs/systems/farm-expansion.md 섹션 1.3

        // --- 계절 제약
        public Season   limitedSeason;    // None = 항상 판매
                                          // → see docs/systems/time-season.md 계절 정의

        // --- 카테고리별 파라미터 (해당 카테고리만 사용)
        public float    lightRadius;      // Light 전용; 나머지 0
                                          // → see docs/systems/decoration-system.md 섹션 2.3
        public float    moveSpeedBonus;   // Path 전용 (0.1 = +10%); 나머지 0
                                          // → see docs/systems/decoration-system.md 섹션 2.2
        public int      durabilityMax;    // Fence 전용; 0 = 영구
                                          // → see docs/systems/decoration-system.md 섹션 2.1

        // --- 렌더링
        public GameObject prefab;         // Light/Ornament/WaterDecor 오브젝트 프리팹
        public TileBase   floorTile;      // Path 전용 Tilemap 타일
        public TileBase   edgeTileH;      // Fence 수평 스프라이트
        public TileBase   edgeTileV;      // Fence 수직 스프라이트
        public TileBase   edgeTileCorner; // Fence 코너 스프라이트 (auto-tiling)

        // --- UI
        public string   description;
    }
}
```

**DecoCategoryType enum**:

```csharp
// illustrative — namespace SeedMind.Decoration.Data
public enum DecoCategoryType
{
    Fence,        // 울타리 — edge 배치
    Path,         // 경로 — 타일 오버레이
    Light,        // 조명 — 1x1 타일 점유
    Ornament,     // 장식물 — 1x1 또는 2x2
    WaterDecor    // 수경 장식 — 2x2~3x3, Zone F 전용
}
```

**SO 에셋 데이터 테이블** (에셋 수 예상):

| 카테고리 | 에셋 수 | 비고 |
|----------|:-------:|------|
| Fence | 4 | → see docs/systems/decoration-system.md 섹션 2.1 |
| Path | 5 | → see docs/systems/decoration-system.md 섹션 2.2 |
| Light | 4 | → see docs/systems/decoration-system.md 섹션 2.3 |
| Ornament | 11 | → see docs/systems/decoration-system.md 섹션 2.4 |
| WaterDecor | 5 | → see docs/systems/decoration-system.md 섹션 2.5 |
| **합계** | **29** | |

buyPrice, unlockLevel, lightRadius, moveSpeedBonus, durabilityMax 등 **콘텐츠 수치는 모두 `docs/systems/decoration-system.md` 섹션 2.1~2.5에서만 정의**한다. 이 문서에 직접 기재하지 않는다 (PATTERN-007).

**에셋 네이밍 패턴** (→ see `docs/systems/project-structure.md` 섹션 6.2):

```
SO_Deco_FenceWood.asset
SO_Deco_PathGravel.asset
SO_Deco_LightTorch.asset
SO_Deco_OrnaWindmillS.asset
SO_Deco_WaterLotus.asset
```

---

### 2.3 DecorationConfig SO 스키마

전역 장식 시스템 설정 ScriptableObject.

```csharp
// illustrative — namespace SeedMind.Decoration.Data
namespace SeedMind.Decoration.Data
{
    [CreateAssetMenu(menuName = "SeedMind/Decoration/DecorationConfig")]
    public class DecorationConfig : ScriptableObject
    {
        // 배치 UI 색상
        public Color  validHighlightColor;    // 배치 가능 하이라이트 색
        public Color  invalidHighlightColor;  // 배치 불가 하이라이트 색

        // 울타리 내구도 처리
        public float  fenceDurabilityDecayPerSeason; // 계절당 감소율
                                                      // → see docs/systems/decoration-system.md 섹션 2.1
        public float  fenceRepairCostRatio;           // 수리 비용 = buyPrice × repairRatio
                                                      // → see docs/systems/decoration-system.md 섹션 2.1

        // 최대 배치 수 제한 (성능 안전망)
        [OPEN - to be filled after DES-023 확정 후]
        // public int maxDecorationsPerZone;

        // Path 이동속도 보너스 적용 여부 (접근성 옵션 연동용)
        public bool   pathSpeedBonusEnabled;  // → see docs/systems/decoration-system.md 섹션 2.2
    }
}
```

에셋 경로: `Assets/_Project/Data/Config/SO_DecorationConfig.asset`

---

### 2.4 세이브 데이터 구조

#### DecorationSaveData (JSON 스키마)

PATTERN-005 준수: JSON 스키마와 C# 클래스의 필드명 및 수가 동일해야 한다.

```json
{
  "decorations": [
    {
      "instanceId": 1,
      "itemId": "FenceStone",
      "cellX": 3,
      "cellZ": 5,
      "edge": 0,
      "durability": 100,
      "colorVariantIndex": 0
    },
    {
      "instanceId": 2,
      "itemId": "PathGravel",
      "cellX": 4,
      "cellZ": 5,
      "edge": -1,
      "durability": -1,
      "colorVariantIndex": 0
    }
  ],
  "nextInstanceId": 3
}
```

**필드 설명**:

| 필드 | 타입 | 설명 |
|------|------|------|
| `instanceId` | int | 인스턴스 고유 ID (제거 시 참조용) |
| `itemId` | string | SO 에셋 식별자 (→ see DecorationItemData.itemId) |
| `cellX` | int | 배치 타일 X 좌표 (Tilemap 셀 좌표) |
| `cellZ` | int | 배치 타일 Z 좌표 (Tilemap 셀 좌표) |
| `edge` | int | 울타리 edge 방향 (-1 = N/A, 0=North, 1=East, 2=South, 3=West) |
| `durability` | int | 현재 내구도 (-1 = 영구 오브젝트) |
| `colorVariantIndex` | int | 마법 수정 조명 등 색상 변형 인덱스 ([OPEN - 색상 팔레트 UI 설계 후 확정]) |
| `nextInstanceId` | int | 다음 인스턴스에 부여할 ID |

#### DecorationSaveData C# 클래스 (PATTERN-005)

```csharp
// illustrative — namespace SeedMind.Decoration
namespace SeedMind.Decoration
{
    [Serializable]
    public class DecorationSaveData
    {
        public List<DecorationInstanceSave> decorations = new();
        public int                          nextInstanceId;
    }

    [Serializable]
    public class DecorationInstanceSave
    {
        public int    instanceId;
        public string itemId;
        public int    cellX;
        public int    cellZ;
        public int    edge;          // -1 = N/A
        public int    durability;    // -1 = 영구
        public int    colorVariantIndex;
    }
}
```

JSON의 `decorations[*]` 7개 필드와 `DecorationInstanceSave` 7개 필드가 일치한다. `DecorationSaveData` 최상위 2개 필드도 동일하다.

#### DecorationInstance (런타임 상태 클래스)

세이브 데이터와 별도로, 런타임에 `_items` 딕셔너리에서 관리하는 상태 클래스:

```csharp
// illustrative — namespace SeedMind.Decoration
public class DecorationInstance
{
    public int              instanceId;
    public DecorationItemData data;        // SO 참조
    public Vector3Int       cell;
    public EdgeDirection    edge;          // Fence 전용
    public int              durability;   // Fence 전용
    public int              colorVariantIndex;
    public GameObject       runtimeObject; // Path/Fence 외 오브젝트 참조
}
```

#### GameSaveData 통합

`save-load-architecture.md`의 `GameSaveData` 루트 클래스에 `decoration` 필드 추가가 필요하다.

```json
// GameSaveData JSON 추가 필드 (save-load-architecture.md 섹션 2.2 갱신 필요)
{
  "decoration": {
    "decorations": [],
    "nextInstanceId": 1
  }
}
```

[OPEN - FIX 태스크로 save-load-architecture.md GameSaveData에 decoration 필드 추가 필요 — DES-023 확정 완료, 즉시 반영 가능]

---

### 2.5 Unity Tilemap 연동

#### Tilemap 레이어 구조

장식 시스템은 SCN_Farm 씬의 `--- ENVIRONMENT ---` 아래에 별도 자식 그룹으로 배치한다.

```
--- ENVIRONMENT ---
│
├── Terrain                         # 기존 지형 Tilemap
│   └── (FarmGrid 타일맵들)
│
├── Decorations                     # ARC-043 신규
│   ├── PathLayer    (Tilemap)       # Sorting Layer: Decoration, Order: 1
│   │                                # 경로 바닥 오버레이
│   ├── FenceLayer   (Tilemap)       # Sorting Layer: Decoration, Order: 2
│   │                                # edge 배치 울타리
│   └── DecoObjects  (Transform)     # 조명·장식물·수경 오브젝트 부모
│
└── Lighting
    └── ...
```

**Tilemap 설계 선택 근거**:

| 레이어 | 방식 | 이유 |
|--------|------|------|
| Path | Tilemap 오버레이 | 바닥 텍스처 타일 배치에 최적, 기존 Terrain 위에 별도 레이어로 분리하여 충돌 없음 |
| Fence | Tilemap edge (Rule Tile) | auto-tiling을 Rule Tile로 구현. 수평/수직/코너 스프라이트 자동 선택 |
| Light/Ornament/WaterDecor | GameObject Instantiate | 애니메이션, 파티클, 광원 컴포넌트가 필요하므로 Tilemap 부적합 |

#### 배치 가능 여부 검사 — TileState 연동

`CanPlace()` 내부에서 `FarmGrid.GetTile(cell)` 을 통해 TileState를 조회한다.

```csharp
// illustrative — CanPlace() 내 TileState 검사 예시
bool IsFarmland(Vector3Int cell)
{
    var tile = _farmGrid.GetTile(cell);
    // TileState.Farmland = 경작지 상태
    return tile != null && tile.state == TileState.Farmland;
    // → see docs/systems/farming-system.md TileState 정의
}

bool IsBuildingTile(Vector3Int cell)
{
    return _buildingManager.IsTileOccupied(cell);
    // → see docs/systems/facilities-architecture.md BuildingManager
}
```

**배치 검사 우선순위** (CanPlace 내부 순서):

1. 구역 해금 여부 (`IsZoneUnlocked`) — FarmZoneManager 조회
2. 시설 타일 점유 여부 (`IsBuildingTile`)
3. 경작지 타일 여부 (`IsFarmland`)
4. 수원 타일 여부 (`IsWaterSource`)
5. 기존 장식 점유 여부 (`IsOccupied` / `IsEdgeOccupied`)
6. 해금 레벨 / 계절 제약 (ProgressionManager.CurrentLevel 조회)

**다중 타일(2x2 이상) 검사**:

```csharp
// illustrative
bool IsOccupied(Vector3Int origin, int w, int h)
{
    for (int dx = 0; dx < w; dx++)
    for (int dz = 0; dz < h; dz++)
    {
        var cell = origin + new Vector3Int(dx, 0, dz);
        if (IsFarmland(cell) || IsBuildingTile(cell)) return true;
        if (_items.Values.Any(i => OccupiesCell(i, cell))) return true;
    }
    return false;
}
```

#### 울타리 Auto-Tiling

울타리는 Unity Rule Tile을 사용하여 인접 울타리와 자동으로 연결 스프라이트를 선택한다.

```
Rule Tile 규칙:
  ─ (수평) : 좌/우 neighbor 있을 때
  │ (수직) : 상/하 neighbor 있을 때
  └ (코너) : 복합 방향 neighbor
  · (단독) : neighbor 없을 때
```

[RISK] Unity Rule Tile은 Tilemap 좌표 기반으로 동작하므로, Fence edge 배치와 Rule Tile 체계를 연동하려면 edge 위치를 Tilemap 셀로 변환하는 유틸리티 메서드가 필요하다. EdgeToCell(cell, edge) 변환 설계는 Phase 2 구현 시 확정한다.

---

## 3. Part II — MCP 태스크 개요

전체 MCP 태스크 시퀀스는 별도 문서 `docs/mcp/decoration-tasks.md`에서 상세화한다. 아래는 태스크 그룹 요약이다.

### D-A: ScriptableObject 클래스 생성

| 태스크 | 내용 |
|--------|------|
| D-A-01 | `Scripts/Decoration/Data/` 폴더 생성 |
| D-A-02 | `DecoCategoryType.cs` enum 스크립트 생성 |
| D-A-03 | `DecorationItemData.cs` SO 클래스 생성 (섹션 2.2 스키마) |
| D-A-04 | `DecorationConfig.cs` SO 클래스 생성 (섹션 2.3 스키마) |
| D-A-05 | `DecorationSaveData.cs` / `DecorationInstanceSave.cs` 생성 (섹션 2.4) |
| D-A-06 | `DecorationInstance.cs` 런타임 클래스 생성 |
| D-A-07 | `DecorationEvents.cs` 정적 이벤트 허브 생성 |

### D-B: DecorationManager MonoBehaviour 생성

| 태스크 | 내용 |
|--------|------|
| D-B-01 | `DecorationManager.cs` 생성 (Singleton, ISaveable) |
| D-B-02 | CanPlace() / Place() / Remove() 메서드 구현 |
| D-B-03 | SaveManager에 ISaveable 등록 (SaveLoadOrder=57) |
| D-B-04 | GameSaveData에 DecorationSaveData 필드 추가 |

### D-C: SO 에셋 생성 (29종)

| 태스크 | 내용 |
|--------|------|
| D-C-01 | `Data/Decorations/` 폴더 생성 |
| D-C-02 | Fence 에셋 4종 생성 (buyPrice 등 → see docs/systems/decoration-system.md 섹션 2.1) |
| D-C-03 | Path 에셋 5종 생성 (→ see docs/systems/decoration-system.md 섹션 2.2) |
| D-C-04 | Light 에셋 4종 생성 (→ see docs/systems/decoration-system.md 섹션 2.3) |
| D-C-05 | Ornament 에셋 11종 생성 (→ see docs/systems/decoration-system.md 섹션 2.4) |
| D-C-06 | WaterDecor 에셋 5종 생성 (→ see docs/systems/decoration-system.md 섹션 2.5) |
| D-C-07 | SO_DecorationConfig.asset 생성 |

### D-D: SCN_Farm 씬 계층 설정

| 태스크 | 내용 |
|--------|------|
| D-D-01 | `Decorations` GameObject 생성 (`--- ENVIRONMENT ---` 하위) |
| D-D-02 | `PathLayer` Tilemap 생성 (Sorting Layer: Decoration, Order: 1) |
| D-D-03 | `FenceLayer` Tilemap 생성 (Sorting Layer: Decoration, Order: 2, Rule Tile) |
| D-D-04 | `DecoObjects` Transform 생성 |
| D-D-05 | DecorationManager 컴포넌트 부착, 필드 연결 |

### D-E: 검증

| 태스크 | 내용 |
|--------|------|
| D-E-01 | 콘솔 로그로 Place/Remove 이벤트 발행 확인 |
| D-E-02 | SaveManager를 통한 세이브/로드 왕복 테스트 |
| D-E-03 | CanPlace 경작지/건물/Zone 제약 검사 단위 테스트 |

---

## 4. Cross-references

| 문서 | 관련 섹션 | 연관 내용 |
|------|----------|----------|
| `docs/systems/decoration-system.md` (DES-023) | 전체 | 이 아키텍처의 설계 기준, 모든 콘텐츠 수치의 canonical 출처 |
| `docs/systems/farming-system.md` | 섹션 1, TileState | 타일 상태, 경작지 제약 |
| `docs/systems/farm-expansion.md` | 섹션 1.3 | Zone 해금 조건, 수경 장식 접근 가능 시점 |
| `docs/systems/farm-expansion-architecture.md` | 섹션 1, 5 | FarmZoneManager.IsZoneUnlocked() 참조 |
| `docs/systems/save-load-architecture.md` (ARC-011) | 섹션 7 | ISaveable 인터페이스, SaveLoadOrder 할당표 (57 추가 필요) |
| `docs/pipeline/data-pipeline.md` (ARC-004) | 섹션 1.1, 2.4 | SO 에셋 체계, BuildingData 패턴 |
| `docs/systems/project-structure.md` | 섹션 2, 6 | SeedMind.Decoration 네임스페이스, 에셋 네이밍 규칙 |
| `docs/systems/time-season.md` | 계절 정의 | limitedSeason 필드의 Season enum |
| `docs/systems/economy-system.md` | 섹션 1.2~1.4 | 장식 시스템의 골드 소모처 역할 |
| `docs/systems/inventory-system.md` | 전체 | 장식품 인벤토리 슬롯 처리 |
| `docs/systems/progression-architecture.md` | 섹션 2.1 | 해금 레벨 판정 (ProgressionManager.CurrentLevel) |
| `docs/content/livestock-system.md` | 전체 | 울타리 동물 이동 차단 연동 ([OPEN#1] DES-023) |
| `docs/systems/time-season.md` | 야간 가시성 | 조명 기능 효과 전제 조건 ([OPEN#2] DES-023) |

---

## 5. Open Questions / Risks

### Open Questions

- [OPEN] **save-load-architecture.md GameSaveData 갱신**: `decoration: DecorationSaveData` 필드를 GameSaveData JSON 스키마 및 C# 클래스에 추가해야 한다. FIX 태스크로 등록 필요.

- [OPEN] **SaveLoadOrder 할당표 갱신**: `save-load-architecture.md` 섹션 7에 `DecorationManager | 57` 행 추가 필요. 현재 GatheringCatalogManager=56, BuildingManager=60 사이 여유 공간(57~59) 중 57 배정 예정이나, FIX 태스크로 공식 등록 후 확정.

- [OPEN] **project-structure.md 갱신**: `SeedMind.Decoration` 네임스페이스와 `Scripts/Decoration/` 폴더를 네임스페이스 목록 및 폴더 구조에 추가 필요.

- [OPEN] **DecorationConfig.maxDecorationsPerZone**: 구역당 최대 장식 수 제한은 성능 영향 측정 후 Phase 2에서 결정. DES-023 미확정.

- [OPEN] **색상 변형(colorVariantIndex)**: 마법 수정 조명(`LightCrystal`) 색상 팔레트 UI 설계는 ui-system.md에서 결정 후 반영. 현재 `colorVariantIndex=0` 고정.

- [OPEN] **울타리 EdgeDirection enum 범위**: North/East/South/West 4방향 외 대각선 허용 여부는 Tilemap Rule Tile 구현 시 결정 (→ see DES-023 [OPEN#1]).

- [OPEN] **야간 조명 효과 구현**: `LightRadius` 파라미터를 실제 Unity URP 2D Light로 구현할지 여부는 `docs/systems/time-season.md` 야간 가시성 메카닉 확정 후 결정 (→ see DES-023 [OPEN#2]).

### Risks

- [RISK] **다중 타일 충돌 검사 복잡도**: 2x2 이상 장식물(풍차/우물/분수 등)과 경로·울타리·시설이 혼재하는 경우 충돌 검사 순회가 O(n×m) 규모로 증가한다. 장식 수가 100개 이하 수준이면 허용 범위이나, 전 구역 완전 장식 시(~200+ 인스턴스) 배치 빈도를 초당 1회 미만으로 제한하는 throttle 추가를 고려한다.

- [RISK] **Tilemap Rule Tile 자동 연결**: 울타리 Rule Tile은 FenceLayer 내 인접 셀만 기준으로 연결 규칙을 적용한다. 울타리 제거 시 인접 세그먼트의 Rule Tile이 자동 갱신되어야 하는데, `Tilemap.RefreshTile(cell)` 호출이 누락되면 잘못된 스프라이트가 남는다. Remove() 흐름에서 인접 셀 Refresh 명시 필요.

- [RISK] **저장 데이터 크기**: 전 구역 돌판 경로 전체 배치 시 인스턴스 수가 수백 개에 달한다. `DecorationInstanceSave` 하나가 약 60바이트 JSON이므로 500개 기준 ~30KB이며, 전체 세이브 파일 허용 상한 기준(→ see `docs/pipeline/data-pipeline.md`) 내에 있을 것으로 추정되나 Phase 2에서 실측 필요.

- [RISK] **울타리 내구도 미결 처리**: FarmGrid.OnSeasonChanged 이벤트에 구독하여 내구도를 감소시켜야 하나, 나무 울타리 "부서진 상태" 스프라이트 처리(FenceLayer Rule Tile 갱신)는 Rule Tile의 조건 분기로 처리할지 별도 Tilemap 레이어로 처리할지가 미결이다. Phase 2 시각 구현 시 결정.
