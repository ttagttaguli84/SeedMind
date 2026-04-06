# 경작 시스템 기술 아키텍처

> 경작(Farming) 시스템의 클래스 구조, 데이터 흐름, 이벤트 설계, MCP 구현 계획  
> 작성: Claude Code (Opus) | 2026-04-06

---

## Context

경작 시스템은 SeedMind의 핵심 게임 루프를 구성하는 가장 중요한 시스템이다. 플레이어가 땅을 일구고, 씨앗을 심고, 물을 주고, 수확하는 전체 과정을 담당한다. 이 문서는 `docs/architecture.md`의 Farm Grid 시스템(4.1, 4.2절)을 기술적으로 상세화하며, `docs/design.md`의 경작 시스템(4.1절) 및 작물 데이터(4.2절)를 구현 수준으로 번역한다.

---

## 1. 클래스 다이어그램

```
                    ┌──────────────────┐
                    │   GameManager    │
                    │   (singleton)    │
                    └───────┬──────────┘
                            │ references
            ┌───────────────┼───────────────┐
            ▼               ▼               ▼
    ┌──────────────┐ ┌─────────────┐ ┌──────────────┐
    │  TimeManager │ │  FarmGrid   │ │ PlayerController│
    │              │ │             │ │              │
    │ OnDayChanged─┼─▶ ProcessDay()│ │ OnToolUsed───┼──┐
    └──────────────┘ └──────┬──────┘ └──────────────┘  │
                            │ owns N×M                  │
                            ▼                           │
                    ┌──────────────┐                    │
                    │   FarmTile   │◄───────────────────┘
                    │              │  HandleToolAction()
                    │ state: enum  │
                    │ crop: ref    │
                    └──────┬──────┘
                           │ owns 0..1
                           ▼
                    ┌──────────────┐     ┌──────────────────┐
                    │ CropInstance │────▶│ CropData (SO)    │
                    │              │ ref │                  │
                    │ currentDay   │     │ cropName         │
                    │ growthStage  │     │ growthDays       │
                    │ isWatered    │     │ sellPrice        │
                    │ fertilizerMul│     │ seedPrice        │
                    └──────────────┘     │ unlockLevel      │
                                         │ seasonMask       │
                           ┌─────────────│ growthStagePrefabs│
                           │             └──────────────────┘
                           ▼
                    ┌──────────────┐
                    │ GrowthSystem │  (매일 아침 배치 처리)
                    │              │
                    │ ProcessAll() │
                    └──────────────┘

    ┌──────────────────┐  ┌──────────────────┐
    │ FertilizerData   │  │ ToolData (SO)    │
    │ (SO)             │  │                  │
    │ growthMultiplier │  │ toolName         │
    │ qualityBonus     │  │ toolType (enum)  │
    │ duration         │  │ range            │
    └──────────────────┘  │ tier             │
                          └──────────────────┘
```

### 클래스 책임 요약

| 클래스 | 유형 | 책임 |
|--------|------|------|
| **FarmGrid** | MonoBehaviour | 타일 배열 소유, 좌표 ↔ 타일 변환, 그리드 확장 |
| **FarmTile** | MonoBehaviour | 단일 타일 상태 관리, 시각적 표현, 도구 액션 수신 |
| **CropInstance** | Plain C# class | 심어진 작물의 런타임 상태 (성장 일수, 물 여부, 비료) |
| **GrowthSystem** | MonoBehaviour | 매일 아침 전체 타일 순회, 성장 처리 |
| **CropData** | ScriptableObject | 작물 정적 데이터 (성장 일수, 가격, 프리팹) |
| **FertilizerData** | ScriptableObject | 비료 정적 데이터 (성장 배수, 품질 보너스) |
| **ToolData** | ScriptableObject | 도구 정적 데이터 (종류, 등급, 범위) |

---

## 2. 타일 상태 머신

### 2.1 상태 정의

```csharp
public enum TileState
{
    Empty,        // 빈 땅 — 잡초 가능
    Tilled,       // 경작됨 — 씨앗 심기 가능
    Planted,      // 씨앗 심어짐 — 물주기 필요
    Watered,      // 물 줌 — 다음 날 성장 처리
    Dry,          // 물 증발 후 건조 — 물주기 필요
    Harvestable,  // 수확 가능 — 낫 사용
    Withered,     // 작물 고사 — 낫으로 제거
    Building      // 시설 설치됨 — 경작 불가
}
```

### 2.2 상태 전환 규칙

```
Empty ──[호미 사용]──▶ Tilled
Tilled ──[씨앗 사용]──▶ Planted
Tilled ──[3일 방치]──▶ Empty  (경작지 퇴화)
Planted ──[물뿌리개]──▶ Watered
Watered ──[새 날 아침, 성장 완료 X]──▶ Dry  (물 증발)
Watered ──[새 날 아침, 성장 완료 O]──▶ Harvestable
Dry ──[물뿌리개]──▶ Watered
Dry ──[3일 연속 건조]──▶ Withered  (작물 고사)
Withered ──[낫 사용]──▶ Tilled  (고사체 제거)
Harvestable ──[낫 사용]──▶ Tilled  (작물 수확, 인벤토리에 추가)
Any ──[건설 시스템]──▶ Building
Building ──[철거]──▶ Empty
```

### 2.3 상태 전환 시 발행되는 이벤트

| 전환 | 이벤트 | 페이로드 |
|------|--------|----------|
| Any -> Tilled | `OnTileTilled` | `(Vector2Int position)` |
| Tilled -> Planted | `OnCropPlanted` | `(Vector2Int position, CropData crop)` |
| Planted/Dry -> Watered | `OnTileWatered` | `(Vector2Int position)` |
| Watered -> Dry/Harvestable | `OnCropGrew` | `(Vector2Int position, int newStage)` |
| Harvestable -> Tilled | `OnCropHarvested` | `(Vector2Int position, CropData crop, int quantity)` |
| Dry -> Withered | `OnCropWithered` | `(Vector2Int position, CropData crop)` |
| Withered -> Tilled | `OnCropCleared` | `(Vector2Int position)` |

### 2.4 FarmTile 상태 전환 메서드 (illustrative)

```csharp
public class FarmTile : MonoBehaviour
{
    [SerializeField] private TileState _state = TileState.Empty;
    private CropInstance _crop;
    private int _neglectDays;  // 물 안 준 일수 / 방치 일수

    // 상태 읽기 전용 프로퍼티
    public TileState State => _state;
    public CropInstance Crop => _crop;
    public Vector2Int GridPosition { get; set; }

    public bool TryTill()
    {
        if (_state != TileState.Empty) return false;
        SetState(TileState.Tilled);
        FarmEvents.OnTileTilled?.Invoke(GridPosition);
        return true;
    }

    public bool TryPlant(CropData cropData)
    {
        if (_state != TileState.Tilled) return false;
        _crop = new CropInstance(cropData);
        _neglectDays = 0;
        SetState(TileState.Planted);
        FarmEvents.OnCropPlanted?.Invoke(GridPosition, cropData);
        return true;
    }

    public bool TryWater()
    {
        if (_state != TileState.Planted && _state != TileState.Dry) return false;
        _neglectDays = 0;
        SetState(TileState.Watered);
        FarmEvents.OnTileWatered?.Invoke(GridPosition);
        return true;
    }

    public bool TryHarvest(out CropData harvested, out int quantity)
    {
        harvested = null; quantity = 0;
        if (_state != TileState.Harvestable) return false;
        harvested = _crop.Data;
        quantity = _crop.CalculateYield();
        FarmEvents.OnCropHarvested?.Invoke(GridPosition, harvested, quantity);
        _crop = null;
        SetState(TileState.Tilled);
        return true;
    }

    private void SetState(TileState newState)
    {
        _state = newState;
        UpdateVisual();  // 머티리얼/메시 변경
    }
}
```

---

## 3. 데이터 흐름

### 3.1 플레이어 도구 사용 흐름

```
PlayerController.OnToolUsed (Input System 콜백)
    │
    ▼
ToolSystem.UseCurrentTool(Vector3 worldPos)
    │
    ├── 1) worldPos → FarmGrid.WorldToGrid(worldPos) → Vector2Int gridPos
    │
    ├── 2) FarmGrid.GetTile(gridPos) → FarmTile tile
    │
    ├── 3) switch (currentTool.toolType)
    │       case Hoe:    tile.TryTill()
    │       case Water:  tile.TryWater()
    │       case Seed:   tile.TryPlant(selectedSeed)
    │       case Sickle: tile.TryHarvest(out crop, out qty)
    │                    → PlayerInventory.Add(crop, qty)
    │
    └── 4) 결과에 따라 도구 애니메이션/효과음 재생
```

### 3.2 일일 성장 처리 흐름

```
TimeManager.OnDayChanged
    │
    ▼
GrowthSystem.ProcessNewDay()
    │
    ├── foreach (FarmTile tile in FarmGrid.AllTiles)
    │       │
    │       ├── if tile.State == Watered:
    │       │       crop.currentGrowthDays += 1 * crop.fertilizerMultiplier
    │       │       if crop.currentGrowthDays >= cropData.growthDays:
    │       │           tile.SetState(Harvestable)
    │       │       else:
    │       │           crop.UpdateGrowthStage()
    │       │           tile.SetState(Dry)  // 다음 날 아침 물 증발
    │       │
    │       ├── if tile.State == Dry (물 안 줌):
    │       │       tile.neglectDays += 1
    │       │       if neglectDays >= 3: tile.Wither() → Withered
    │       │
    │       ├── if tile.State == Tilled (씨앗 없음):
    │       │       tile.neglectDays += 1
    │       │       if neglectDays >= 3: tile.Revert() → Empty
    │       │
    │       └── 모든 Watered 타일의 isWatered 리셋 (물은 매일 줘야 함)
    │
    └── 완료 후 FarmEvents.OnDailyGrowthComplete?.Invoke()
```

---

## 4. ScriptableObject 스키마 상세

### 4.1 CropData

```csharp
[CreateAssetMenu(fileName = "NewCrop", menuName = "SeedMind/CropData")]
public class CropData : ScriptableObject
{
    [Header("기본 정보")]
    public string cropName;           // "감자", "토마토" 등
    public string cropId;             // "potato", "tomato" (코드용 고유 식별자)
    public Sprite icon;               // UI 아이콘

    [Header("경제")]
    public int seedPrice;             // 씨앗 구매가 (→ see docs/design.md 4.2)
    public int sellPrice;             // 수확물 판매가 (→ see docs/design.md 4.2)

    [Header("성장")]
    public int growthDays;            // 총 성장 일수 (→ see docs/design.md 4.2)
    public int growthStageCount = 4;  // 시각적 단계 수 (기본 4)
    public SeasonFlag allowedSeasons; // 재배 가능 계절 (비트마스크)

    [Header("수확")]
    public int baseYield = 1;         // 기본 수확량
    public float qualityChance = 0.1f;// 고품질 확률

    [Header("해금")]
    public int unlockLevel;           // 해금 레벨 (→ see docs/design.md 4.2)

    [Header("비주얼")]
    public GameObject[] growthStagePrefabs;  // 단계별 3D 모델 (길이 = growthStageCount)
    public Material soilMaterial;            // 심어졌을 때 토양 머티리얼 오버라이드
}

[System.Flags]
public enum SeasonFlag
{
    None   = 0,
    Spring = 1 << 0,
    Summer = 1 << 1,
    Autumn = 1 << 2,
    Winter = 1 << 3  // 온실 전용
}
```

### 4.2 FertilizerData

```csharp
[CreateAssetMenu(fileName = "NewFertilizer", menuName = "SeedMind/FertilizerData")]
public class FertilizerData : ScriptableObject
{
    public string fertilizerName;
    public string fertilizerId;
    public int buyPrice;

    [Header("효과")]
    public float growthMultiplier = 1.25f; // 성장 속도 배수 (기본 비료: 1.25 = +25%, → see farming-system.md 5.1)
    public float qualityBonus = 0.15f;     // 고품질 확률 추가
    public int effectDuration = 1;         // 효과 지속 일수 (1=당일만)

    [Header("비주얼")]
    public Color soilTintColor;            // 비료 적용 시 토양 색상 힌트
}
```

### 4.3 ToolData

```csharp
[CreateAssetMenu(fileName = "NewTool", menuName = "SeedMind/ToolData")]
public class ToolData : ScriptableObject
{
    public string toolName;
    public ToolType toolType;
    public int tier = 1;               // 1~5 (기본/구리/철/금/이리듐)

    [Header("성능")]
    public int range = 1;              // 영향 범위 (1=단일 타일, 3=3x3 등)
    public float useSpeed = 1.0f;      // 사용 속도 배수

    [Header("업그레이드")]
    public int upgradeCost;            // 다음 등급 업그레이드 비용
    public ToolData nextTier;          // 다음 등급 ToolData 참조 (null이면 최종)

    [Header("비주얼")]
    public Sprite icon;
    public GameObject modelPrefab;
}

public enum ToolType
{
    Hoe,          // 호미: Empty → Tilled
    WateringCan,  // 물뿌리개: Planted/Dry → Watered
    SeedBag,      // 씨앗 봉투: Tilled → Planted (선택된 CropData에 의존)
    Sickle,       // 낫: Harvestable → Tilled + 수확
    Hand          // 빈손: 상호작용용
}
```

---

## 5. 성장 업데이트 전략

### 선택: 매일 아침 배치 처리

| 방식 | 장점 | 단점 |
|------|------|------|
| **매일 아침 배치 (채택)** | 예측 가능, 디버그 쉬움, 성능 부담 없음 | 하루 중간에 성장 불가 |
| 실시간 틱 | 부드러운 성장 연출 | 불필요한 복잡도, 저장 어려움 |

### 배치 처리 구현 전략

```
GrowthSystem는 TimeManager.OnDayChanged 이벤트에 구독.
새 날이 시작되면(6:00) 단일 프레임 내에서 전체 타일을 순회한다.

처리 순서:
1. Watered 타일의 작물 성장 일수 증가
2. 성장 단계 전환 판정 (growthDays / growthStageCount로 단계 경계 계산)
3. 성장 완료 판정 → Harvestable 전환
4. Growing 타일의 neglectDays 증가, 고사 판정
5. Tilled 빈 타일의 neglectDays 증가, 퇴화 판정
6. 모든 타일의 isWatered 플래그 리셋
```

### 성능 고려

- 최대 그리드 16x16 = 256 타일: 배치 처리 부담 무시 가능
- 타일 순회는 단순 배열 인덱싱 (O(N), N <= 256)
- 프리팹 교체(성장 단계 변경)는 Object Pooling 불필요 (빈도 낮음)
- [RISK] 프리팹 Instantiate가 동시에 다수 발생할 수 있음 -- 성장 단계 전환이 많은 날 아침에 프레임 드롭 가능. 대안: 코루틴으로 프리팹 교체를 수 프레임에 분산.

---

## 6. 이벤트 시스템 설계

### 6.1 정적 이벤트 허브 패턴

```csharp
// 경작 관련 이벤트를 중앙 집중 관리
public static class FarmEvents
{
    // 타일 상태
    public static Action<Vector2Int> OnTileTilled;
    public static Action<Vector2Int> OnTileWatered;

    // 작물 생명주기
    public static Action<Vector2Int, CropData> OnCropPlanted;
    public static Action<Vector2Int, int> OnCropGrew;           // int = newStage
    public static Action<Vector2Int, CropData, int> OnCropHarvested; // int = quantity
    public static Action<Vector2Int, CropData> OnCropWithered;

    // 일일 처리
    public static Action OnDailyGrowthComplete;
}
```

### 6.2 이벤트 소비자 예시

| 이벤트 | 소비자 | 용도 |
|--------|--------|------|
| `OnCropPlanted` | UI (HUD) | "감자를 심었습니다!" 메시지 |
| `OnCropPlanted` | EconomyManager | 씨앗 비용 차감 확인 |
| `OnCropHarvested` | PlayerInventory | 수확물 추가 |
| `OnCropHarvested` | LevelSystem | 경험치 부여 |
| `OnCropHarvested` | UI (HUD) | "+1 감자" 팝업 |
| `OnCropWithered` | UI (HUD) | 경고 메시지 |
| `OnDailyGrowthComplete` | SaveManager | 자동 저장 트리거 |

### 6.3 설계 원칙

- **Fire and forget**: 이벤트 발행자는 소비자를 모른다
- **단방향 데이터 흐름**: FarmTile이 이벤트 발행 → 외부 시스템이 구독
- **구독 해제 필수**: MonoBehaviour의 OnDisable에서 구독 해제 (메모리 누수 방지)
- [RISK] static event는 씬 전환 시 구독이 남을 수 있음. 씬 로드 시 이벤트 초기화 루틴 필요.

---

## 7. MCP 구현 계획

경작 시스템을 MCP for Unity를 통해 단계적으로 구축하는 태스크 시퀀스. 상세 MCP 태스크는 `docs/mcp/farming-tasks.md`에 별도 작성 예정(ARC-003).

### Phase A: 기본 그리드 (MCP 6단계)

```
Step A-1: 빈 씬에 GameObject "FarmSystem" 생성
          → GrowthSystem.cs 컴포넌트 부착

Step A-2: "FarmSystem" 하위에 GameObject "FarmGrid" 생성
          → FarmGrid.cs 컴포넌트 부착
          → gridWidth=8, gridHeight=8 설정

Step A-3: FarmGrid 하위에 Quad 기반 타일 8x8 = 64개 생성
          → 각 타일 이름: "Tile_0_0" ~ "Tile_7_7"
          → 각 타일에 FarmTile.cs 컴포넌트 부착
          → GridPosition 프로퍼티 설정 (x, y)
          → 위치: (x * 1.0, 0, y * 1.0)

Step A-4: Material "M_Soil_Empty" 생성 (URP/Lit, Base Color: #8B7355)
          Material "M_Soil_Tilled" 생성 (URP/Lit, Base Color: #5C4033)
          Material "M_Soil_Watered" 생성 (URP/Lit, Base Color: #3B2A1A)
          → 모든 타일에 M_Soil_Empty 할당

Step A-5: Main Camera를 쿼터뷰로 설정
          → Position: (4, 10, -4)
          → Rotation: (45, 45, 0)
          → Projection: Orthographic, Size: 6

Step A-6: Directional Light "Sun" 설정
          → Rotation: (50, -30, 0)
          → Color: warm white (#FFF4E0)
          → Play Mode 진입 → 그리드 시각 확인
```

### Phase B: 작물 데이터 (MCP 4단계)

```
Step B-1: Assets/Data/Crops/ 폴더에 CropData SO 생성
          → SO_Crop_Potato (감자: growthDays=3, sellPrice=30, seedPrice=15)
          → SO_Crop_Carrot (당근: growthDays=3, sellPrice=35, seedPrice=15)
          → SO_Crop_Tomato (토마토: growthDays=5, sellPrice=60, seedPrice=25)

Step B-2: 작물 단계별 Placeholder 프리팹 생성 (Capsule/Cube 기반)
          → 각 작물 4단계: Seed(작은 점), Sprout(작은 캡슐), Growth(중간 캡슐), Mature(큰 캡슐)
          → 단계별 스케일: (0.1, 0.1, 0.1) → (0.2, 0.3, 0.2) → (0.3, 0.5, 0.3) → (0.4, 0.7, 0.4)

Step B-3: 작물별 Material 생성
          → M_Crop_Potato: green (#4CAF50) → brown (#8D6E63)
          → M_Crop_Carrot: green (#66BB6A) → orange (#FF9800)
          → M_Crop_Tomato: green (#81C784) → red (#F44336)

Step B-4: CropData SO의 growthStagePrefabs 배열에 프리팹 연결
          → Play Mode에서 CropData 로드 및 필드 확인
```

### Phase C: 상호작용 연결 (MCP 3단계)

```
Step C-1: PlayerController에 레이캐스트 기반 타일 선택 로직 연결
          → 마우스 클릭 → Physics.Raycast → FarmTile 참조 획득

Step C-2: ToolSystem 컴포넌트를 Player에 부착
          → 기본 도구 세트: Hoe, WateringCan, SeedBag, Sickle, Hand
          → 숫자키 1~5로 도구 전환

Step C-3: Play Mode 통합 테스트
          → 호미로 타일 경작 → 씨앗 심기 → 물주기
          → Console 로그로 상태 전환 확인
          → FarmEvents 이벤트 발행 확인
```

---

## Open Questions

- [OPEN] 작물 품질 시스템(일반/고급/최상급)을 Phase 1에 포함할지, 후속 확장으로 미룰지
- [OPEN] 다중 수확 작물(딸기 등 반복 수확) 지원 여부 -- CropData에 `isRepeating`, `regrowDays` 필드 추가 필요
- [OPEN] 비료 적용을 물주기와 별도 액션으로 할지, 물에 혼합하는 방식으로 할지

## Risks

- [RISK] 프리팹 동시 교체로 인한 프레임 드롭 (섹션 5 참조) -- 코루틴 분산 처리로 대응
- [RISK] static event 구독 누수 (섹션 6.3 참조) -- 씬 전환 시 이벤트 초기화 루틴 필요
- [RISK] MCP를 통한 ScriptableObject 필드 설정의 정확성 -- MCP가 SO의 배열/참조 필드를 정확히 설정할 수 있는지 사전 검증 필요

---

## Cross-references

- `docs/architecture.md` 4.1절 (Farm Grid 시스템), 4.2절 (작물 성장)
- `docs/design.md` 4.1절 (경작 시스템), 4.2절 (작물 종류)
- `docs/systems/project-structure.md` (네임스페이스, 의존성)
- `docs/mcp/farming-tasks.md` (상세 MCP 태스크 시퀀스, 작성 예정 -- ARC-003)
- `docs/balance/crop-economy.md` (작물 경제 밸런스, 작성 예정 -- BAL-001)
