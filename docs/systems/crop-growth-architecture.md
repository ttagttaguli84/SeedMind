# 작물 성장 시스템 기술 아키텍처

> 작물의 런타임 생명주기, 성장 공식, 품질 결정, 다중 수확, 거대 작물을 포함하는 기술 설계  
> 작성: Claude Code (Opus) | 2026-04-06

---

## Context

이 문서는 `docs/systems/farming-architecture.md`의 작물 성장 관련 부분을 독립적으로 심화한다. farming-architecture.md가 경작 시스템 전반(그리드, 타일 상태, 도구, 이벤트)을 다루는 반면, 이 문서는 **작물이 심어진 후부터 수확(또는 고사)까지의 전체 생명주기**에 집중한다. CropInstance의 런타임 상태 관리, GrowthSystem의 배치 처리 로직, 품질 결정 알고리즘, 다중 수확 작물 메카닉, 거대 작물 병합 로직, 그리고 MCP를 통한 단계별 구현 계획을 포함한다.

**설계 목표**: 데이터 드리븐 구조로 새로운 작물을 CropData ScriptableObject 하나만 추가하여 확장할 수 있도록 한다.

---

## 1. 클래스 다이어그램

```
    ┌──────────────────────────────────────────────────────────────────┐
    │                        SeedMind.Farm                            │
    └──────────────────────────────────────────────────────────────────┘

    ┌─────────────┐         ┌──────────────┐
    │ TimeManager │         │  FarmGrid    │
    │ (Core)      │         │              │
    │             │         │ tiles[N,M]   │
    │ OnDayChanged├────────▶│              │
    │ OnSeasonChg │         └──────┬───────┘
    └─────────────┘                │ owns N×M
                                   ▼
                           ┌──────────────┐
                           │  FarmTile    │
                           │              │
                           │ state: enum  │
                           │ soilQuality  │
                           │ crop: ref ───┼──────┐
                           └──────────────┘      │
                                                 │ owns 0..1
                                                 ▼
    ┌──────────────────────────────────────────────────────┐
    │                   CropInstance                        │
    │  (Plain C# class — Serializable)                     │
    │──────────────────────────────────────────────────────│
    │  - cropData: CropData (SO ref)                       │
    │  - currentGrowthDays: float                          │
    │  - totalGrowthDays: int  (= cropData.growthDays)     │
    │  - currentStage: int                                 │
    │  - quality: Quality (수확 시 결정)                     │
    │  - isWatered: bool                                   │
    │  - dryDayCount: int                                  │
    │  - fertilizerType: FertilizerData (nullable)         │
    │  - plantedSeason: SeasonFlag                         │
    │  - wateredDayRatio: float (물 준 날 / 총 경과 일)     │
    │  - totalElapsedDays: int                             │
    │  - isGiantPart: bool                                 │
    │  - giantCropRef: GiantCropInstance (nullable)        │
    │──────────────────────────────────────────────────────│
    │  + Grow(float amount): GrowthResult                  │
    │  + GetCurrentStage(): int                            │
    │  + CheckHarvestable(): bool                          │
    │  + DetermineQuality(float soilQuality): Quality      │
    │  + MarkWatered() / ResetWatered()                    │
    │  + IncrementDryDay(): bool (true = withered)         │
    │  + ResetForReharvest()                               │
    └──────────────┬───────────────────────────────────────┘
                   │ ref
                   ▼
    ┌──────────────────────────────────────────────────────┐
    │                  CropData (ScriptableObject)          │
    │──────────────────────────────────────────────────────│
    │  [기본 정보]                                          │
    │  - cropName: string                                  │
    │  - cropId: string                                    │
    │  - icon: Sprite                                      │
    │                                                      │
    │  [경제]                                               │
    │  - seedPrice: int                                    │
    │  - sellPrice: int                                    │
    │                                                      │
    │  [성장]                                               │
    │  - growthDays: int                                   │
    │  - growthStageCount: int (= 4)                       │
    │  - allowedSeasons: SeasonFlag (비트마스크)             │
    │                                                      │
    │  [수확]                                               │
    │  - baseYield: int (= 1)                              │
    │  - qualityChance: float (= 0.1)                      │
    │  - isReharvestable: bool                             │
    │  - reharvestDays: int                                │
    │                                                      │
    │  [거대 작물]                                           │
    │  - giantCropPrefab: GameObject (nullable)            │
    │  - giantCropMinSize: int (= 3, 3x3)                 │
    │  - giantCropChance: float (= 0.01)                   │
    │                                                      │
    │  [해금]                                               │
    │  - unlockLevel: int                                  │
    │                                                      │
    │  [비주얼]                                              │
    │  - growthStagePrefabs: GameObject[]                   │
    │  - soilMaterial: Material                            │
    └──────────────────────────────────────────────────────┘

    ┌──────────────────────────────────────────────────────┐
    │                  GrowthSystem (MonoBehaviour)         │
    │──────────────────────────────────────────────────────│
    │  - farmGrid: FarmGrid (ref)                          │
    │──────────────────────────────────────────────────────│
    │  + OnEnable(): TimeManager.OnDayChanged 구독          │
    │  + OnDisable(): 구독 해제                              │
    │  + ProcessNewDay(): 전체 타일 배치 처리                 │
    │  - ProcessTileGrowth(FarmTile): void                  │
    │  - ProcessSeasonCheck(FarmTile): void                 │
    │  - ProcessDryCheck(FarmTile): void                    │
    │  - TryGiantCropMerge(): void                         │
    │  - SpreadGrowthOverFrames(): IEnumerator             │
    └──────────────────────────────────────────────────────┘

    ┌──────────────────────────────────────────────────────┐
    │            GiantCropInstance (Plain C# class)         │
    │──────────────────────────────────────────────────────│
    │  - cropData: CropData                                │
    │  - originTile: Vector2Int (좌하단 기준)                │
    │  - size: int (3 = 3x3)                               │
    │  - memberTiles: List<FarmTile>                        │
    │  - giantPrefabInstance: GameObject                    │
    │──────────────────────────────────────────────────────│
    │  + Harvest(): (CropData, int quantity)                │
    │  + Destroy(): void                                   │
    └──────────────────────────────────────────────────────┘

    ┌──────────────────────────────────────────────────────┐
    │              GrowthResult (enum)                      │
    │──────────────────────────────────────────────────────│
    │  None,           // 변화 없음                         │
    │  StageAdvanced,  // 시각적 단계 전환                   │
    │  Completed,      // 성장 완료 → Harvestable            │
    │  Withered        // 고사                               │
    └──────────────────────────────────────────────────────┘

    ┌──────────────────────────────────────────────────────┐
    │              Quality (enum)                           │
    │──────────────────────────────────────────────────────│
    │  Normal,    // x1.0 — 별 없음                         │
    │  Silver,    // x1.25 — 은색 별                         │
    │  Gold,      // x1.5 — 금색 별                          │
    │  Iridium    // x2.0 — 보라색 별                        │
    └──────────────────────────────────────────────────────┘
```

### 클래스 관계 요약

| 관계 | 설명 |
|------|------|
| FarmGrid → FarmTile | 1:N 소유. 그리드가 타일 배열 관리 |
| FarmTile → CropInstance | 1:0..1 소유. 타일에 작물이 있거나 없음 |
| CropInstance → CropData | N:1 참조. 여러 인스턴스가 같은 SO 참조 |
| CropInstance → FertilizerData | N:0..1 참조. 비료가 적용되었거나 없음 |
| GrowthSystem → FarmGrid | 1:1 참조. 매일 아침 전체 타일 순회 |
| GiantCropInstance → FarmTile | 1:N 참조. 병합된 타일들을 추적 |
| GiantCropInstance → CropData | 1:1 참조. 거대 작물의 원본 데이터 |

---

## 2. CropInstance 상세

### 2.1 필드 정의

```csharp
namespace SeedMind.Farm
{
    [System.Serializable]
    public class CropInstance
    {
        // --- 정적 데이터 참조 ---
        [SerializeField] private CropData _cropData;

        // --- 성장 상태 ---
        [SerializeField] private float _currentGrowthDays;   // 누적 성장 일수 (소수점: 비료 보너스 반영)
        [SerializeField] private int _totalGrowthDays;       // 목표 성장 일수 (= cropData.growthDays)
        [SerializeField] private int _currentStage;          // 현재 시각적 단계 (0 ~ growthStageCount-1)

        // --- 물 관리 ---
        [SerializeField] private bool _isWatered;            // 오늘 물을 줬는지
        [SerializeField] private int _dryDayCount;           // 연속 건조 일수 (3 도달 시 고사)

        // --- 비료 ---
        [SerializeField] private FertilizerData _fertilizer; // 적용된 비료 (null = 없음)

        // --- 품질 추적 ---
        [SerializeField] private int _totalElapsedDays;      // 총 경과 일수
        [SerializeField] private int _wateredDayCount;       // 물 준 총 일수

        // --- 계절 ---
        [SerializeField] private SeasonFlag _plantedSeason;  // 심어진 계절

        // --- 거대 작물 ---
        [SerializeField] private bool _isGiantPart;          // 거대 작물에 병합되었는지
        private GiantCropInstance _giantCropRef;             // 비직렬화 — 런타임 참조

        // --- 품질 (수확 시 결정) ---
        private Quality _quality;

        // --- 읽기 전용 프로퍼티 ---
        public CropData Data => _cropData;
        public int CurrentStage => _currentStage;
        public float GrowthProgress => _currentGrowthDays / _totalGrowthDays;
        public bool IsWatered => _isWatered;
        public bool IsGiantPart => _isGiantPart;
        public float WateredRatio => _totalElapsedDays > 0
            ? (float)_wateredDayCount / _totalElapsedDays
            : 0f;
    }
}
```

### 2.2 생성자

```csharp
public CropInstance(CropData data, SeasonFlag currentSeason)
{
    _cropData = data;
    _totalGrowthDays = data.growthDays;
    _currentGrowthDays = 0f;
    _currentStage = 0;
    _isWatered = false;
    _dryDayCount = 0;
    _fertilizer = null;
    _totalElapsedDays = 0;
    _wateredDayCount = 0;
    _plantedSeason = currentSeason;
    _isGiantPart = false;
    _giantCropRef = null;
    _quality = Quality.Normal;
}
```

### 2.3 핵심 메서드

```csharp
/// 하루 성장 처리. 반환값으로 시각적 갱신 필요 여부 전달.
public GrowthResult Grow(float fertilizerMultiplier, float soilMultiplier, float seasonBonus)
{
    if (_isGiantPart) return GrowthResult.None;  // 거대 작물 소속이면 개별 성장 안 함

    float effectiveGrowth = 1.0f * fertilizerMultiplier * soilMultiplier * seasonBonus;
    _currentGrowthDays += effectiveGrowth;

    // 성장 완료 체크
    if (_currentGrowthDays >= _totalGrowthDays)
    {
        _currentGrowthDays = _totalGrowthDays;
        _currentStage = _cropData.growthStageCount - 1;
        return GrowthResult.Completed;
    }

    // 단계 전환 체크
    int newStage = CalculateStage();
    if (newStage != _currentStage)
    {
        _currentStage = newStage;
        return GrowthResult.StageAdvanced;
    }

    return GrowthResult.None;
}

/// 현재 성장 일수 기반 시각적 단계 계산
private int CalculateStage()
{
    float progress = _currentGrowthDays / _totalGrowthDays;
    int stage = Mathf.FloorToInt(progress * _cropData.growthStageCount);
    return Mathf.Clamp(stage, 0, _cropData.growthStageCount - 1);
}

/// 물 관리
public void MarkWatered()
{
    _isWatered = true;
    _dryDayCount = 0;
}

public void ResetWatered()
{
    _isWatered = false;
}

/// 건조 일수 증가. true 반환 시 고사.
public bool IncrementDryDay()
{
    _dryDayCount++;
    return _dryDayCount >= 3;
}

/// 하루 경과 추적 (물 관리 비율 계산용)
public void TrackDay()
{
    _totalElapsedDays++;
    if (_isWatered) _wateredDayCount++;
}

/// 다중 수확 작물: 수확 후 재성장 준비
public void ResetForReharvest()
{
    _currentGrowthDays = 0f;
    _totalGrowthDays = _cropData.reharvestDays;
    _currentStage = _cropData.growthStageCount - 2;  // 마지막 직전 단계(성장 중)로 복귀
    _isWatered = false;
    _dryDayCount = 0;
    // _fertilizer 유지 — 비료 효과는 뿌리에 남음
    // _wateredDayCount, _totalElapsedDays 유지 — 누적 관리 비율
}

/// 수확 시 품질 결정
public Quality DetermineQuality(float soilQuality)
{
    float fertilizerBonus = _fertilizer != null ? _fertilizer.qualityBonus : 0f;
    float waterBonus = WateredRatio * 0.2f;  // 물 관리 비율 최대 +0.2

    float qualityScore = soilQuality + fertilizerBonus + waterBonus
                       + UnityEngine.Random.Range(0f, 0.3f);

    if (qualityScore >= 0.9f) return Quality.Iridium;
    if (qualityScore >= 0.6f) return Quality.Gold;
    if (qualityScore >= 0.3f) return Quality.Silver;
    return Quality.Normal;
}

/// 수확량 계산
public int CalculateYield()
{
    int yield = _cropData.baseYield;
    // 거대 작물은 별도 처리 (GiantCropInstance.Harvest)
    return yield;
}
```

---

## 3. CropData ScriptableObject 확장

기존 `farming-architecture.md`에서 정의한 CropData에 다중 수확 및 거대 작물 필드를 추가한다.

```csharp
[CreateAssetMenu(fileName = "NewCrop", menuName = "SeedMind/CropData")]
public class CropData : ScriptableObject
{
    [Header("기본 정보")]
    public string cropName;           // "감자", "토마토" 등
    public string cropId;             // "potato", "tomato" (코드용 고유 식별자)
    public Sprite icon;               // UI 아이콘

    [Header("경제")]
    public int seedPrice;             // 씨앗 구매가 (-> see docs/design.md 4.2)
    public int sellPrice;             // 수확물 판매가 (-> see docs/design.md 4.2)

    [Header("성장")]
    public int growthDays;            // 총 성장 일수 (-> see docs/design.md 4.2)
    public int growthStageCount = 4;  // 시각적 단계 수 (기본 4)
    public SeasonFlag allowedSeasons; // 재배 가능 계절 (비트마스크)

    [Header("수확")]
    public int baseYield = 1;         // 기본 수확량
    public float qualityChance = 0.1f;// 고품질 확률 보정
    public bool isReharvestable;      // 다중 수확 여부 (딸기, 토마토 등)
    public int reharvestDays;         // 재수확까지 성장 일수 (isReharvestable=true일 때만 유효)

    [Header("거대 작물")]
    public GameObject giantCropPrefab; // 거대 작물 프리팹 (null이면 거대 작물 불가)
    public int giantCropMinSize = 3;   // 최소 영역 크기 (3 = 3x3)
    public float giantCropChance = 0.01f; // 일일 거대 작물 전환 확률

    [Header("해금")]
    public int unlockLevel;           // 해금 레벨 (-> see docs/design.md 4.2)

    [Header("비주얼")]
    public GameObject[] growthStagePrefabs;  // 단계별 3D 모델 (길이 = growthStageCount)
    public Material soilMaterial;            // 심어졌을 때 토양 머티리얼 오버라이드
}
```

### 기존 대비 추가된 필드

| 필드 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| `isReharvestable` | bool | false | true이면 수확 후 재성장 |
| `reharvestDays` | int | 0 | 재수확까지 필요한 일수 |
| `giantCropPrefab` | GameObject | null | null이면 거대 작물 불가 |
| `giantCropMinSize` | int | 3 | 거대 작물 최소 NxN 크기 |
| `giantCropChance` | float | 0.15 | 일일 거대 작물 병합 확률 (15%) (-> see crop-growth.md 섹션 5.1, canonical) |

### 작물별 데이터 예시

| cropId | growthDays | allowedSeasons | isReharvestable | reharvestDays | giantCropPrefab |
|--------|-----------|----------------|-----------------|---------------|-----------------|
| potato | 3 | Spring | false | - | null |
| carrot | 3 | Spring,Autumn | false | - | null |
| tomato | 5 | Summer | false | - | null |
| corn | 7 | Summer | false | - | null |
| strawberry | 5 | Spring | true | 3 | null |
| pumpkin | 10 | Autumn | false | - | PFB_GiantCrop_Pumpkin |
| sunflower | 8 | Summer | false | - | null |
| watermelon | 12 | Summer | false | - | PFB_GiantCrop_Watermelon |

(-> see `docs/systems/crop-growth.md` 섹션 3.1 및 4.1, canonical)

---

## 4. GrowthSystem 설계

### 4.1 역할

GrowthSystem은 `TimeManager.OnDayChanged` 이벤트에 구독하여 매일 아침 06:00에 전체 경작 타일을 순회하며 성장을 처리하는 중앙 제어 컴포넌트이다.

### 4.2 일일 처리 흐름 (ProcessNewDay)

```
TimeManager.OnDayChanged
    │
    ▼
GrowthSystem.ProcessNewDay()
    │
    ├── 1) 계절 전환 체크 (ProcessSeasonTransition)
    │       새 계절이 시작되었는가?
    │       → 예: 현재 계절에 allowedSeasons가 아닌 작물 전부 고사 처리
    │
    ├── 2) 타일별 성장 처리 (foreach tile in FarmGrid.AllTiles)
    │       │
    │       ├── tile.State == Watered:
    │       │     a) crop.TrackDay()
    │       │     b) 성장 공식 적용:
    │       │        effectiveGrowth = 1.0 * fertilizerMul * soilMul * seasonBonus
    │       │     c) crop.Grow(fertilizerMul, soilMul, seasonBonus)
    │       │     d) result == Completed → tile.SetState(Harvestable)
    │       │        result == StageAdvanced → 프리팹 교체 예약
    │       │     e) crop.ResetWatered()
    │       │     f) tile.SetState(Dry)  [Completed가 아닌 경우]
    │       │
    │       ├── tile.State == Planted (물 안 줌, 첫 날):
    │       │     a) crop.TrackDay()
    │       │     b) crop.IncrementDryDay()
    │       │        → 3일 도달: tile.Wither()
    │       │
    │       ├── tile.State == Dry (물 안 줌):
    │       │     a) crop.TrackDay()
    │       │     b) crop.IncrementDryDay()
    │       │        → 3일 도달: tile.Wither()
    │       │
    │       └── tile.State == Tilled (빈 경작지):
    │             neglectDays++
    │             → 3일 도달: tile.Revert() → Empty
    │
    ├── 3) 거대 작물 병합 체크 (TryGiantCropMerge)
    │       Harvestable 상태의 동일 작물 3x3 영역 탐색
    │
    └── 4) 이벤트 발행
          FarmEvents.OnDailyGrowthComplete?.Invoke()
```

### 4.3 성장 공식

```
effectiveGrowth = baseGrowth * fertilizerMultiplier * soilMultiplier * seasonBonus
```

| 파라미터 | 값 범위 | 설명 |
|----------|---------|------|
| baseGrowth | 1.0 (고정) | 물을 준 날 기본 1일 성장 |
| fertilizerMultiplier | 1.0 ~ 1.5 | 비료 종류에 따른 배수 (-> farming-system.md 5.1) |
| soilMultiplier | 0.9 ~ 1.2 | 토양 품질 단계에 따른 배수 (Poor=0.9, Normal=1.0, Fertile=1.1, Rich=1.2) (-> farming-system.md 6절, canonical) |
| seasonBonus | 1.0 ~ 1.1 | 계절 적합도. 재배 가능 계절=1.0, 주 계절=1.1 |

**비료 배수 상세** (-> see farming-system.md 5.1):

| 비료 | fertilizerMultiplier |
|------|---------------------|
| 없음 | 1.0 |
| 기본 비료 (Basic) | 1.25 |
| 속성장 비료 (Speed-Gro) | 1.5 |
| 고급 비료 (Quality) | 1.0 (속도 영향 없음, 품질 보너스) |
| 유기 비료 (Organic) | 1.0 (토양 품질 자체를 영구 상승) |

**계절 보너스 상세**:

| 조건 | seasonBonus |
|------|-------------|
| 재배 가능 계절 (기본) | 1.0 |
| 여름 + 과일류 작물 | 1.2 (-> design.md 4.3 여름 성장 속도 보너스) |
| 온실 내 겨울 재배 | 0.8 (겨울 페널티) |
| 재배 불가 계절 (야외) | 성장 안 함 → 고사 처리 |

**참고 [BAL-010]**: 온실 비계절 작물의 성장 속도 페널티 외에 판매가 보정도 적용될 수 있다 (-> see `docs/systems/economy-architecture.md` 섹션 3.7, `docs/systems/economy-system.md` 섹션 2.6.5).

### 4.4 계절 전환 처리 (ProcessSeasonTransition)

```csharp
/// 새 계절이 시작되었을 때 호출
private void ProcessSeasonTransition(SeasonFlag newSeason)
{
    foreach (FarmTile tile in _farmGrid.AllTiles)
    {
        if (tile.Crop == null) continue;
        if (tile.State == TileState.Building) continue;

        CropData data = tile.Crop.Data;

        // 새 계절에서 재배 불가한 작물 → 고사
        if ((data.allowedSeasons & newSeason) == SeasonFlag.None)
        {
            // 온실 타일이면 면제
            if (!tile.IsInGreenhouse)
            {
                tile.Wither();
                FarmEvents.OnCropWithered?.Invoke(tile.GridPosition, data);
            }
        }
    }
}
```

### 4.5 물 안 준 날 처리

```
dryDayCount 추적:
  - 물을 주면 (MarkWatered) → dryDayCount = 0 리셋
  - 새 날 아침, 물 안 준 상태(Planted 또는 Dry)이면 → dryDayCount++
  - dryDayCount >= 3 → Withered 전환

시각적 힌트:
  dryDayCount == 1: 타일 색상 약간 밝아짐 (경고 없음)
  dryDayCount == 2: 작물 오브젝트 약간 처짐 + UI 경고 아이콘
  dryDayCount == 3: 고사 → 갈색 시듦 비주얼 + FarmEvents.OnCropWithered
```

---

## 5. 품질 결정 알고리즘

### 5.1 계산 시점

품질은 **수확 시점**에 1회 결정된다. 성장 중에는 품질이 정해지지 않는다.

### 5.2 입력 파라미터

| 파라미터 | 범위 | 출처 |
|----------|------|------|
| soilQuality | 0.0 ~ 0.4 | FarmTile.soilQuality (토양 4등급: Poor=0.0, Normal=0.1, Fertile=0.25, Rich=0.4) (-> farming-system.md 섹션 6.1, canonical) |
| fertilizerBonus | 0.0 ~ 0.3 | FertilizerData.qualityBonus (고급 비료 시 0.3) |
| waterBonus | 0.0 ~ 0.2 | CropInstance.WateredRatio * 0.2 (물 관리 비율) |
| randomFactor | 0.0 ~ 0.3 | UnityEngine.Random.Range(0f, 0.3f) |

### 5.3 공식

```
qualityScore = soilQuality + fertilizerBonus + waterBonus + randomFactor
```

최소값: 0.0 + 0.0 + 0.0 + 0.0 = 0.0
최대값: 0.4 + 0.3 + 0.2 + 0.3 = 1.2

### 5.4 품질 등급 판정 (-> see farming-system.md 5.4)

| 등급 | qualityScore 범위 | 가격 배수 | 비주얼 |
|------|-------------------|-----------|--------|
| Normal | < 0.3 | x1.0 | 별 없음 |
| Silver | 0.3 ~ 0.6 미만 | x1.25 | 은색 별 1개 |
| Gold | 0.6 ~ 0.9 미만 | x1.5 | 금색 별 1개 |
| Iridium | >= 0.9 | x2.0 | 보라색 별 1개 |

### 5.5 시나리오별 확률 분석

**최악의 조건** (비료 없음, 나쁜 토양, 물 관리 50%):
- qualityScore = 0.0 + 0.0 + 0.1 + random(0~0.3)
- Normal 확률 ~67%, Silver ~33%, Gold/Iridium 0%

**최적 조건** (고급 비료, 최상 토양, 물 100%):
- qualityScore = 0.3 + 0.3 + 0.2 + random(0~0.3)
- 최소 0.8 → Silver 이상 보장
- Gold ~33%, Iridium ~67%

**표준 조건** (기본 비료, 보통 토양, 물 80%):
- qualityScore = 0.15 + 0.0 + 0.16 + random(0~0.3)
- Normal ~0%, Silver ~67%, Gold ~33%

---

## 6. 이벤트 시스템 확장

기존 `farming-architecture.md`의 FarmEvents에 작물 성장 관련 이벤트를 추가한다.

### 6.1 추가 이벤트

```csharp
public static class FarmEvents
{
    // --- 기존 (farming-architecture.md 6.1절) ---
    public static Action<Vector2Int> OnTileTilled;
    public static Action<Vector2Int> OnTileWatered;
    public static Action<Vector2Int, CropData> OnCropPlanted;
    public static Action<Vector2Int, int> OnCropGrew;                    // int = newStage
    public static Action<Vector2Int, CropData, int> OnCropHarvested;     // int = quantity
    public static Action<Vector2Int, CropData> OnCropWithered;
    public static Action OnDailyGrowthComplete;

    // --- 추가: 작물 성장 시스템 ---
    public static Action<Vector2Int, int> OnCropStageChanged;            // int = newStage (프리팹 교체 트리거)
    public static Action<Vector2Int, CropData, Quality> OnCropHarvestedWithQuality;  // 품질 포함 수확
    public static Action<Vector2Int, CropData> OnCropReharvestReady;     // 다중 수확 작물 재수확 준비 완료
    public static Action<Vector2Int, int> OnGiantCropFormed;             // int = size (3x3 등)
    public static Action<Vector2Int, CropData, int> OnGiantCropHarvested; // int = totalYield
    public static Action<SeasonFlag> OnSeasonCropCheck;                   // 계절 전환 시 작물 체크 시작
}
```

### 6.2 이벤트 파라미터 상세

| 이벤트 | 파라미터 | 발행 시점 |
|--------|----------|-----------|
| `OnCropStageChanged` | (위치, 새 단계 번호) | GrowthSystem이 단계 전환 감지 시 |
| `OnCropHarvestedWithQuality` | (위치, 작물 데이터, 품질) | 수확 시 품질 결정 후 |
| `OnCropReharvestReady` | (위치, 작물 데이터) | 다중 수확 작물이 재수확 가능 상태 도달 시 |
| `OnGiantCropFormed` | (좌하단 위치, 크기) | 거대 작물 병합 성공 시 |
| `OnGiantCropHarvested` | (좌하단 위치, 작물 데이터, 총 수확량) | 거대 작물 수확 시 |
| `OnSeasonCropCheck` | (새 계절) | 계절 전환 시 고사 체크 시작 전 |

### 6.3 이벤트 소비자 매핑

| 이벤트 | 소비자 | 용도 |
|--------|--------|------|
| `OnCropStageChanged` | FarmTile | 프리팹 교체 실행 |
| `OnCropStageChanged` | UI (HUD) | 성장 진행 이펙트 |
| `OnCropHarvestedWithQuality` | PlayerInventory | 품질별 아이템 추가 |
| `OnCropHarvestedWithQuality` | EconomyManager | 품질별 판매가 계산 |
| `OnCropReharvestReady` | UI (HUD) | "수확 가능" 아이콘 재표시 |
| `OnGiantCropFormed` | UI (HUD) | "거대 작물 출현!" 연출 |
| `OnGiantCropHarvested` | LevelSystem | 보너스 경험치 |

---

## 7. 다중 수확 작물 처리

### 7.1 대상 작물

(-> see `docs/systems/crop-growth.md` 섹션 4.2, canonical)

| 작물 | 첫 수확까지 | 재수확 주기 | 재배 계절 |
|------|------------|------------|-----------|
| 딸기 | 5일 | 3일 | Spring |

**참고**: 토마토는 단일 수확 작물이다 (crop-growth.md 섹션 4.1 참조). 이전 버전에서 토마토를 다중 수확으로 잘못 기재한 내용을 수정했다.

[OPEN] 토마토를 다중 수확 작물로 전환할지 검토 필요. 전환 시 crop-growth.md 섹션 4.1 및 4.2를 먼저 수정해야 한다.

### 7.2 생명주기

```
[심기] → [성장 5일] → [수확 가능]
                           │
                     [수확] ← 아이템 획득
                           │
                     [재성장 상태로 전환]
                     CropInstance.ResetForReharvest()
                           │
                     stage = growthStageCount - 2 (성장 단계)
                     totalGrowthDays = reharvestDays
                     currentGrowthDays = 0
                           │
                     [재성장 N일] → [수확 가능]
                           │
                     [수확] ← 아이템 획득
                           │
                     ... (계절 끝날 때까지 반복)
```

### 7.3 타일 상태 전환

일반 작물: `Harvestable → [수확] → Tilled` (작물 인스턴스 제거)

다중 수확: `Harvestable → [수확] → Dry` (작물 인스턴스 유지, 재성장 시작)

```csharp
// FarmTile.TryHarvest 수정 (illustrative)
public bool TryHarvest(out CropData harvested, out int quantity, out Quality quality)
{
    harvested = null; quantity = 0; quality = Quality.Normal;
    if (_state != TileState.Harvestable) return false;

    harvested = _crop.Data;
    quantity = _crop.CalculateYield();
    quality = _crop.DetermineQuality(_soilQuality);

    FarmEvents.OnCropHarvestedWithQuality?.Invoke(GridPosition, harvested, quality);

    if (_crop.Data.isReharvestable)
    {
        // 다중 수확: 재성장 준비
        _crop.ResetForReharvest();
        SetState(TileState.Dry);  // 물 다시 줘야 재성장
        // 프리팹을 성장 중 단계로 교체
        UpdateVisualToStage(_crop.CurrentStage);
    }
    else
    {
        // 단일 수확: 작물 제거
        _crop = null;
        SetState(TileState.Tilled);
    }

    return true;
}
```

### 7.4 계절 종료 시 다중 수확 작물

계절이 끝나면 모든 야외 작물이 고사하므로, 다중 수확 작물도 계절 전환 시 제거된다. 온실 내 다중 수확 작물은 계절 제한 없이 계속 수확 가능하다.

---

## 8. 거대 작물 체크

### 8.1 발생 조건

거대 작물은 다음 조건이 **모두** 충족될 때 병합 판정에 진입한다:

1. **같은 작물**: NxN 영역의 모든 타일에 동일한 cropId의 CropInstance가 있어야 함
2. **같은 성장 완료**: 모든 타일이 `Harvestable` 상태
3. **거대 작물 지원**: 해당 CropData의 `giantCropPrefab`이 null이 아님
4. **확률**: `giantCropChance` (기본 15%)로 일일 판정 (-> see crop-growth.md 섹션 5.1, canonical)

### 8.2 탐색 알고리즘

```
TryGiantCropMerge() — 매일 아침 성장 처리 후 호출

1) FarmGrid에서 giantCropPrefab이 존재하는 CropData를 가진
   Harvestable 타일 목록 수집

2) 각 타일을 좌하단 기준으로 NxN 영역 체크:
   for each candidate tile (x, y):
       size = cropData.giantCropMinSize  // 보통 3
       valid = true
       for dx in [0, size):
           for dy in [0, size):
               neighborTile = FarmGrid.GetTile(x+dx, y+dy)
               if neighborTile == null: valid = false; break
               if neighborTile.State != Harvestable: valid = false; break
               if neighborTile.Crop.Data.cropId != cropId: valid = false; break
               if neighborTile.Crop.IsGiantPart: valid = false; break

       if valid:
           roll = Random.Range(0f, 1f)
           if roll < cropData.giantCropChance:
               MergeToGiantCrop(x, y, size, cropData)

3) 중복 방지: 한 번 체크된 타일은 skip 목록에 추가
```

### 8.3 병합 로직 (MergeToGiantCrop)

```csharp
private void MergeToGiantCrop(int originX, int originY, int size, CropData data)
{
    // 1) GiantCropInstance 생성
    var giant = new GiantCropInstance(data, new Vector2Int(originX, originY), size);

    // 2) 영역 내 모든 타일의 CropInstance를 거대 작물에 귀속
    for (int dx = 0; dx < size; dx++)
    {
        for (int dy = 0; dy < size; dy++)
        {
            FarmTile tile = _farmGrid.GetTile(originX + dx, originY + dy);
            tile.Crop.SetGiantPart(giant);
            giant.AddMemberTile(tile);

            // 개별 작물 프리팹 비활성화
            tile.HideCropVisual();
        }
    }

    // 3) 거대 작물 프리팹 생성
    //    위치: 영역 중앙, 스케일: size x size
    Vector3 centerPos = _farmGrid.GridToWorld(originX + size/2, originY + size/2);
    giant.SpawnVisual(centerPos);

    // 4) 이벤트 발행
    FarmEvents.OnGiantCropFormed?.Invoke(new Vector2Int(originX, originY), size);
}
```

### 8.4 거대 작물 수확

```
수확 조건: 거대 작물의 아무 타일이나 낫으로 상호작용
수확량: 기본 수확량 * 타일 수 * 2 (거대 작물 보너스 x2)
  예: 호박 거대 작물 (3x3) = 1 * 9 * 2 = 18개 호박

수확 처리:
  1) GiantCropInstance.Harvest() 호출
  2) 거대 프리팹 파괴 (파티클 이펙트 동반)
  3) 모든 멤버 타일의 CropInstance 제거
  4) 모든 멤버 타일을 Tilled 상태로 전환
  5) FarmEvents.OnGiantCropHarvested 발행
```

---

## 9. MCP 구현 계획

### Phase A: CropData ScriptableObject 에셋 생성

```
Step A-1: CropData.cs 클래스에 새 필드 추가
          → isReharvestable, reharvestDays
          → giantCropPrefab, giantCropMinSize, giantCropChance
          → Assets/_Project/Scripts/Farm/Data/CropData.cs

Step A-2: Quality enum 파일 생성
          → Assets/_Project/Scripts/Farm/Data/Quality.cs
          → Normal, Silver, Gold, Iridium 정의

Step A-3: GrowthResult enum 파일 생성
          → Assets/_Project/Scripts/Farm/Data/GrowthResult.cs
          → None, StageAdvanced, Completed, Withered 정의

Step A-4: CropData SO 에셋 생성 (기존 3종 업데이트 + 신규 5종)
          → Assets/_Project/Data/Crops/ 폴더
          → SO_Crop_Potato: growthDays=3, sellPrice=30, seedPrice=15,
            allowedSeasons=Spring, isReharvestable=false
          → SO_Crop_Carrot: growthDays=3, sellPrice=35, seedPrice=15,
            allowedSeasons=Spring, isReharvestable=false
          → SO_Crop_Tomato: growthDays=5, sellPrice=60, seedPrice=25,
            allowedSeasons=Summer, isReharvestable=true, reharvestDays=3
          → SO_Crop_Corn: growthDays=7, sellPrice=100, seedPrice=30,
            allowedSeasons=Summer, isReharvestable=false
          → SO_Crop_Strawberry: growthDays=5, sellPrice=80, seedPrice=40,
            allowedSeasons=Spring, isReharvestable=true, reharvestDays=2
          → SO_Crop_Pumpkin: growthDays=10, sellPrice=200, seedPrice=80,
            allowedSeasons=Autumn, isReharvestable=false,
            giantCropChance=0.01
          → SO_Crop_Sunflower: growthDays=8, sellPrice=150, seedPrice=60,
            allowedSeasons=Summer, isReharvestable=false
          → SO_Crop_Watermelon: growthDays=12, sellPrice=350, seedPrice=120,
            allowedSeasons=Summer, isReharvestable=false,
            giantCropChance=0.01

Step A-5: 각 SO 에셋의 필드값 검증
          → Play Mode 진입
          → Console 로그로 각 CropData 필드 출력
```

### Phase B: CropInstance 프리팹 + 단계별 비주얼

```
Step B-1: CropInstance.cs 작성
          → Assets/_Project/Scripts/Farm/CropInstance.cs
          → 섹션 2의 필드 및 메서드 구현

Step B-2: GiantCropInstance.cs 작성
          → Assets/_Project/Scripts/Farm/GiantCropInstance.cs

Step B-3: 신규 작물 4단계 Placeholder 프리팹 생성
          (기존 감자/당근/토마토는 farming-architecture.md Phase B에서 생성)
          → PFB_Crop_Corn_Stage0~3
          → PFB_Crop_Strawberry_Stage0~3
          → PFB_Crop_Pumpkin_Stage0~3
          → PFB_Crop_Sunflower_Stage0~3
          → PFB_Crop_Watermelon_Stage0~3

          단계별 스케일 (-> farming-architecture.md B-2):
          Stage0: (0.1, 0.1, 0.1)
          Stage1: (0.2, 0.3, 0.2)
          Stage2: (0.3, 0.5, 0.3)
          Stage3: (0.4, 0.7, 0.4)

Step B-4: 작물별 Material 생성
          → M_Crop_Corn: green → gold (#FFD54F)
          → M_Crop_Strawberry: green → red (#E53935)
          → M_Crop_Pumpkin: green → orange (#FF6F00)
          → M_Crop_Sunflower: green → yellow (#FDD835)
          → M_Crop_Watermelon: green → dark green (#2E7D32) + 줄무늬

Step B-5: 거대 작물 프리팹 생성
          → PFB_GiantCrop_Pumpkin: 큰 Sphere 기반, 스케일 (3, 2.5, 3)
            Material: orange (#FF6F00)
          → PFB_GiantCrop_Watermelon: 큰 Capsule 기반, 스케일 (3, 2, 3)
            Material: dark green (#2E7D32)

Step B-6: CropData SO의 growthStagePrefabs 배열에 프리팹 연결
          → 거대 작물 SO에 giantCropPrefab 연결
          → Play Mode에서 프리팹 로드 확인
```

### Phase C: GrowthSystem 컴포넌트 연결

```
Step C-1: GrowthSystem.cs 확장
          → ProcessNewDay() 메서드에 성장 공식 구현
          → ProcessSeasonTransition() 구현
          → TryGiantCropMerge() 구현

Step C-2: FarmEvents.cs에 신규 이벤트 추가
          → OnCropStageChanged, OnCropHarvestedWithQuality
          → OnCropReharvestReady, OnGiantCropFormed, OnGiantCropHarvested
          → OnSeasonCropCheck

Step C-3: FarmTile.TryHarvest() 수정
          → 품질 결정 로직 통합
          → 다중 수확 분기 처리

Step C-4: 통합 테스트
          → SCN_Test_FarmGrid 씬에서 테스트
          → 감자 심기 → 물주기 3일 → 수확 (단일 수확 확인)
          → 딸기 심기 → 물주기 5일 → 수확 → 재성장 3일 → 재수확 (다중 수확 확인)
          → 호박 3x3 심기 → 성장 완료 → Console에서 giantCropChance=1.0 임시 설정 → 거대 작물 병합 확인
          → Console 로그로 품질 결정 결과 확인
```

---

## 10. 성능 고려사항

### 10.1 배치 처리 비용

| 항목 | 최대치 (16x16) | 예상 비용 |
|------|---------------|-----------|
| 타일 순회 | 256회 | O(N), 단순 배열 인덱싱. 무시 가능 |
| 성장 계산 | 256회 곱셈/비교 | CPU 부담 무시 가능 |
| 거대 작물 탐색 | 256 * 9 = 2304 타일 검사 (최악) | O(N * K^2), K=3. 여전히 무시 가능 |
| **프리팹 교체** | **최대 256개 동시** | **[RISK] 프레임 드롭 가능** |

### 10.2 프리팹 교체 전략

**문제**: 모든 작물이 같은 날 단계 전환하면 한 프레임에 최대 256번 Instantiate/Destroy 발생.

**해결: 코루틴 분산 처리**

```csharp
private IEnumerator SpreadGrowthOverFrames(List<(FarmTile tile, int newStage)> pendingChanges)
{
    const int BATCH_SIZE = 16;  // 프레임당 최대 프리팹 교체 수
    for (int i = 0; i < pendingChanges.Count; i += BATCH_SIZE)
    {
        int end = Mathf.Min(i + BATCH_SIZE, pendingChanges.Count);
        for (int j = i; j < end; j++)
        {
            var (tile, stage) = pendingChanges[j];
            tile.UpdateVisualToStage(stage);
        }
        yield return null;  // 다음 프레임으로 양보
    }
}
```

이 방식으로 256개 프리팹 교체 시 최대 16프레임(~0.27초 at 60fps)에 걸쳐 분산된다.

### 10.3 프리팹 교체 vs 메시 스왑

| 방식 | 장점 | 단점 |
|------|------|------|
| **프리팹 교체 (채택)** | 단계별 완전한 비주얼 자유도, 구현 단순 | Instantiate 비용, GC 발생 |
| 메시 스왑 | GC 없음, 즉각적 | 컴포넌트 구성 동일해야 함, 유연성 낮음 |
| Object Pool | GC 제거, 재사용 | 초기 메모리 할당, 관리 복잡도 |

**판단**: 256타일 규모에서 프리팹 교체의 GC 영향은 미미하다. 코루틴 분산으로 프레임 드롭을 방지하면 충분하다. 프로파일링 후 문제 발생 시 Object Pool로 전환한다.

### 10.4 메모리 예측

| 항목 | 단위 크기 | 최대 수량 | 총 메모리 |
|------|-----------|-----------|-----------|
| CropInstance (필드) | ~80 bytes | 256 | ~20 KB |
| CropData SO (참조) | ~4 bytes (포인터) | 256 | ~1 KB |
| 작물 프리팹 (씬 내) | ~1 KB (로우폴리) | 256 | ~256 KB |
| 거대 작물 프리팹 | ~5 KB | 최대 4~5 | ~25 KB |
| **총계** | | | **~300 KB** |

메모리 측면에서 어떤 제약도 없다.

---

## Cross-references

- `docs/architecture.md` 4.1절 (Farm Grid 시스템), 4.2절 (작물 성장 기본 구조)
- `docs/systems/farming-architecture.md` (경작 시스템 전반: 클래스 다이어그램, 타일 상태 머신, 이벤트 허브, MCP Phase A~C)
- `docs/systems/farming-system.md` 5절 (비료 메카닉), 5.4절 (수확 품질 시스템 canonical)
- `docs/design.md` 4.2절 (작물 종류 및 가격 canonical), 4.3절 (계절 시스템)
- `docs/systems/project-structure.md` (네임스페이스 SeedMind.Farm, 에셋 네이밍 SO_Crop_ 규칙)
- `docs/content/crops.md` (전체 작물 상세 스펙 canonical — CON-001)
- `docs/balance/crop-economy.md` (작물 경제 밸런스 시트 — BAL-001, BAL-003, BAL-010 결정 포함)
- `docs/systems/economy-architecture.md` 섹션 3.7 (온실 판매가 보정 구현, BAL-010)
- `docs/systems/economy-system.md` 섹션 2.6.5 (온실 보정 canonical 규칙, BAL-010)

---

## Open Questions

- [OPEN] 거대 작물의 품질은 어떻게 결정할 것인가? 멤버 타일의 평균 품질 점수 사용 vs 고정 Gold 이상 보장
- [OPEN] 다중 수확 작물의 재수확 횟수에 상한을 둘 것인가? 현재 설계는 계절 끝까지 무제한
- [OPEN] 비료 효과가 다중 수확 작물의 재성장 주기에도 적용되는가? 현재 설계는 비료 유지 (ResetForReharvest에서 _fertilizer 보존)
- [OPEN] 거대 작물 확률(giantCropChance)을 일일 판정으로 할지, 수확 가능 상태 도달 시 1회 판정으로 할지
- [OPEN] 온실 내 거대 작물 허용 여부 — 3x3 공간 확보가 현실적인지

## Risks

- [RISK] MCP를 통한 ScriptableObject 배열/참조 필드 설정이 정확히 동작하는지 사전 검증 필요 (-> architecture.md 동일 리스크). 특히 growthStagePrefabs[] 배열과 giantCropPrefab 참조 설정.
- [RISK] 프리팹 동시 교체로 인한 프레임 드롭 — 코루틴 분산 처리(섹션 10.2)로 대응하되, 프로파일링 필수
- [RISK] 거대 작물 병합 시 이미 플레이어가 일부 타일을 수확한 경우의 레이스 컨디션 — TryGiantCropMerge는 ProcessNewDay 내에서만 호출되므로 플레이어 입력과 동시 실행되지 않음. 안전.
- [RISK] 다중 수확 작물의 dryDayCount가 재성장 시 리셋되므로, 재성장 중 물을 안 줘도 고사까지 3일 유예가 다시 부여됨. 밸런스 관점에서 적절한지 검토 필요.
- [RISK] Quality.DetermineQuality의 random 요소(0~0.3)가 결과를 지나치게 좌우할 수 있음. 플레이테스트 후 범위 조정 가능성 있음.

---

*이 문서는 Claude Code가 기존 경작 아키텍처와 게임 디자인을 기반으로 작물 성장 시스템을 기술적으로 상세화한 문서입니다.*
