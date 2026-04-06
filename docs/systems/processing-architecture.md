# 가공/요리 시스템 기술 아키텍처 (Processing Architecture)

> ProcessingManager의 클래스 구조, 가공 큐/슬롯 관리, 연료 시스템, 이벤트 설계, 저장/복원, MCP 구현 태스크 요약  
> 작성: Claude Code (Opus) | 2026-04-06  
> Phase 1 | 문서 ID: ARC-012

---

## 1. 시스템 개요

### 1.1 Context

가공/요리 시스템은 플레이어가 수확한 작물을 **잼, 주스, 절임** 등의 가공품으로 변환하여 더 높은 가격에 판매할 수 있게 하는 시스템이다. 이 문서는 `docs/systems/facilities-architecture.md` 섹션 7에서 정의된 ProcessingSystem을 **독립 아키텍처 문서**로 확장하여, 가공 프로세스의 전체 런타임 흐름, 클래스 간 책임 분리, 이벤트 연동, 저장/복원 구조를 상세화한다.

**설계 목표**:
- 가공소(Building)와 가공 로직(Processing)의 **책임 경계**를 명확히 정의한다
- 모든 레시피 파라미터는 ScriptableObject 데이터 드리븐 -- 코드 변경 없이 밸런스 조정 가능
- InventoryManager, EconomyManager, TimeManager와의 연동을 이벤트 기반으로 설계한다
- 향후 새로운 가공 유형(요리 등) 추가 시 코드 변경을 최소화하는 확장 가능한 구조

### 1.2 FacilitiesManager와의 관계

가공소는 시설(Building)의 **하위 타입**이다. 즉, 건물로서의 생명주기(건설/배치/업그레이드/철거)는 BuildingManager가 관리하고, 가공 로직은 ProcessingSystem이 담당한다.

```
BuildingManager (시설 생명주기 관리)
├── WaterTankSystem       → effectType == AutoWater
├── GreenhouseSystem      → effectType == SeasonBypass
├── StorageSystem         → effectType == Storage
└── ProcessingSystem      → effectType == Processing    ← 이 문서의 범위
```

**책임 분리 원칙**:

| 영역 | 담당 시스템 | 설명 |
|------|-----------|------|
| 가공소 건설/배치/철거 | BuildingManager | 타일 점유, 비용 지불, 건설 진행 |
| 가공소 업그레이드(슬롯 추가) | BuildingManager + ProcessingSystem | BuildingManager가 업그레이드 트리거 → ProcessingSystem이 슬롯 배열 확장 |
| 가공 작업 시작/진행/완료 | ProcessingSystem | 레시피 검증, 시간 추적, 완료 판정 |
| 재료 투입/결과물 수거 | ProcessingSystem → InventoryManager (이벤트) | 재고 변동은 이벤트 통신 |
| 가공품 가격 계산 | EconomyManager | 가공품도 일반 아이템과 동일한 판매 경로 |

### 1.3 의존성

```
ProcessingSystem (SeedMind.Building 네임스페이스 내)
  참조하는 모듈:
  ├── SeedMind.Core     (TimeManager -- 시간 경과 구독)
  ├── SeedMind.Farm     (CropData, CropCategory -- 입력 작물 검증)
  └── SeedMind.Economy  (간접 참조 없음 -- 가격 계산은 EconomyManager 책임)

  참조 받는 모듈:
  ├── SeedMind.Player   (InventoryManager -- 이벤트 통신으로 재료 차감/결과물 추가)
  └── SeedMind.UI       (ProcessingUI -- 슬롯 상태 표시, 레시피 선택)
```

(-> see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

---

## 2. 핵심 클래스 구조

### 2.1 RecipeData ScriptableObject

레시피 데이터의 canonical 필드 정의는 `docs/pipeline/data-pipeline.md` 섹션 2.5이다. 레시피 목록 및 수치는 `docs/content/facilities.md` 섹션 6.3이 canonical이다.

#### JSON 스키마

```json
{
  "dataId": "recipe_jam_potato",
  "displayName": "감자 잼",
  "description": "감자를 가공하여 만든 잼.",
  "icon": null,
  "processingType": "Jam",
  "inputCategory": "Vegetable",
  "inputItemId": "potato",
  "inputQuantity": 1,
  "priceMultiplier": 0.0,
  "priceBonus": 0,
  "processingTimeHours": 0.0,
  "fuelCost": 0,
  "requiredFacilityTier": 0,
  "outputItemId": "jam_potato",
  "outputQuantity": 1
}
```

> priceMultiplier, priceBonus, processingTimeHours:  
> (-> see `docs/systems/economy-system.md` 섹션 2.5 및 `docs/content/processing-system.md` 섹션 3 for canonical 수치)
>
> fuelCost, requiredFacilityTier:  
> (-> see `docs/content/processing-system.md` 섹션 4 for 연료 시스템 canonical. fuelCost > 0인 레시피는 베이커리 전용이며, 가공소(일반)/제분소/발효실은 0)

#### C# 클래스 (PATTERN-005: JSON과 필드 완전 동기화 필수)

```csharp
// illustrative
namespace SeedMind.Building.Data
{
    /// <summary>
    /// 가공 레시피를 정의하는 ScriptableObject.
    /// GameDataSO를 상속하여 dataId, displayName, icon은 부모에서 제공.
    /// (-> see docs/pipeline/data-pipeline.md 섹션 2.5 for canonical 필드 정의)
    /// </summary>
    [CreateAssetMenu(fileName = "SO_Recipe_New", menuName = "SeedMind/Building/ProcessingRecipeData")]
    public class ProcessingRecipeData : GameDataSO
    {
        // --- GameDataSO 상속 필드 ---
        // public string dataId;        (부모, JSON의 "dataId")
        // public string displayName;   (부모, JSON의 "displayName")
        // public Sprite icon;          (부모, JSON의 "icon")

        [Header("설명")]
        public string description;                        // UI 설명 텍스트

        [Header("가공")]
        public ProcessingType processingType;             // 가공 유형 (Jam, Juice, Pickle)
        public CropCategory inputCategory;                // 입력 가능 작물 카테고리
        public string inputItemId;                        // 입력 아이템 식별자 (예: "potato")
        public int inputQuantity;                         // 입력 수량 (기본 1)
        public float priceMultiplier;                     // 원재료 기본가 대비 배수 (-> see docs/systems/economy-system.md 섹션 2.5)
        public int priceBonus;                            // 고정 가격 보너스 (-> see docs/systems/economy-system.md 섹션 2.5)
        public float processingTimeHours;                 // 가공 소요 시간, 게임 내 시간 (-> see docs/content/facilities.md 섹션 6.2)
        public int fuelCost;                              // 연료 비용 (0 = 불필요, -> see docs/content/processing-system.md 섹션 4)
        public int requiredFacilityTier;                  // 필요 가공소 등급 (0 = Tier 1)
        public string outputItemId;                       // 출력 아이템 식별자 (예: "jam_potato")
        public int outputQuantity;                        // 출력 수량 (기본 1)
    }

    /// <summary>
    /// 가공 유형. (-> see docs/pipeline/data-pipeline.md 섹션 2.5 for canonical 정의)
    /// </summary>
    public enum ProcessingType
    {
        Jam,          // 잼 -- 과일/채소 모두 가능
        Juice,        // 주스 -- 과일류만
        Pickle,       // 절임 -- 채소류만
        Mill,         // 제분 -- 제분소 전용 (-> see docs/pipeline/data-pipeline.md 섹션 2.5)
        Fermentation, // 발효 -- 발효실 전용 (-> see docs/pipeline/data-pipeline.md 섹션 2.5)
        Bake          // 요리 -- 베이커리 전용 (-> see docs/pipeline/data-pipeline.md 섹션 2.5)
    }
}
```

#### PATTERN-005 필드 동기화 검증

| JSON 키 | C# 필드 | 소속 |
|---------|---------|------|
| dataId | GameDataSO.dataId | 부모 클래스 |
| displayName | GameDataSO.displayName | 부모 클래스 |
| icon | GameDataSO.icon | 부모 클래스 |
| description | description | ProcessingRecipeData |
| processingType | processingType | ProcessingRecipeData |
| inputCategory | inputCategory | ProcessingRecipeData |
| inputItemId | inputItemId | ProcessingRecipeData |
| inputQuantity | inputQuantity | ProcessingRecipeData |
| priceMultiplier | priceMultiplier | ProcessingRecipeData |
| priceBonus | priceBonus | ProcessingRecipeData |
| processingTimeHours | processingTimeHours | ProcessingRecipeData |
| fuelCost | fuelCost | ProcessingRecipeData |
| requiredFacilityTier | requiredFacilityTier | ProcessingRecipeData |
| outputItemId | outputItemId | ProcessingRecipeData |
| outputQuantity | outputQuantity | ProcessingRecipeData |

총 15 필드: JSON 15개 = C# 15개 (부모 3 + 자체 12). 일치 확인 완료.

> **참고**: 본 문서는 `facilities-architecture.md` 섹션 7.1의 ProcessingRecipeData에 `inputItemId`, `inputQuantity`, `fuelCost`, `requiredFacilityTier`, `outputQuantity`, `description` 필드를 추가 확장한다. facilities-architecture.md 원본에서는 `inputCategory`로 카테고리 필터만 수행했으나, 실제 레시피는 특정 작물 ID를 지정하므로 `inputItemId`가 필요하다. 두 문서 간 필드 차이는 [OPEN] 항목으로 관리한다.

[OPEN] facilities-architecture.md 섹션 7.1의 ProcessingRecipeData와 본 문서의 확장 필드(inputItemId, inputQuantity, fuelCost, requiredFacilityTier, outputQuantity, description)를 동기화해야 한다. facilities-architecture.md 업데이트 시 본 문서를 canonical로 참조할지, data-pipeline.md를 canonical로 유지할지 결정 필요.

---

### 2.2 ProcessingSlot 클래스

```csharp
// illustrative
namespace SeedMind.Building
{
    /// <summary>
    /// 가공소의 개별 가공 슬롯. ProcessingSaveData와 1:1 대응.
    /// (-> see docs/pipeline/data-pipeline.md Part II 섹션 2.6 for ProcessingSaveData)
    /// </summary>
    public class ProcessingSlot
    {
        public enum SlotState
        {
            Empty,          // 비어있음 -- 새 가공 작업 수용 가능
            Processing,     // 가공 진행 중 -- 시간 경과 대기
            Completed       // 가공 완료 -- 수거 대기
        }

        public SlotState State { get; set; }                // 슬롯 상태
        public ProcessingRecipeData Recipe { get; set; }    // 현재 레시피 (null = 빈 슬롯)
        public string InputCropId { get; set; }             // 입력 작물 ID
        public int InputQuantity { get; set; }              // 입력 수량
        public float RemainingHours { get; set; }           // 남은 시간 (게임 내 시간)
        public float TotalHours { get; set; }               // 총 소요 시간 (진행률 계산용)
        public string OutputItemId { get; set; }            // 출력 아이템 ID
        public int OutputQuantity { get; set; }             // 출력 수량

        /// <summary>진행률 (0.0 ~ 1.0). UI 프로그레스 바용.</summary>
        public float Progress =>
            TotalHours > 0f ? 1f - (RemainingHours / TotalHours) : 0f;

        /// <summary>슬롯을 초기 상태로 리셋.</summary>
        public void Clear()
        {
            State = SlotState.Empty;
            Recipe = null;
            InputCropId = null;
            InputQuantity = 0;
            RemainingHours = 0f;
            TotalHours = 0f;
            OutputItemId = null;
            OutputQuantity = 0;
        }
    }
}
```

---

### 2.3 ProcessingSystem 클래스

ProcessingSystem은 BuildingManager의 **서브시스템**으로, 모든 가공소의 가공 작업을 관리한다. MonoBehaviour가 아닌 **Plain C# 클래스**이며, BuildingManager가 인스턴스를 소유한다.

```csharp
// illustrative
namespace SeedMind.Building
{
    /// <summary>
    /// 가공소의 레시피 처리, 큐 관리, 시간 경과 처리를 담당하는 서브시스템.
    /// BuildingManager가 소유하며, TimeManager.OnHourChanged에 구독하여 시간 경과를 처리한다.
    /// </summary>
    public class ProcessingSystem
    {
        // --- 내부 상태 ---
        private Dictionary<BuildingInstance, ProcessingSlot[]> _processors
            = new Dictionary<BuildingInstance, ProcessingSlot[]>();

        // --- 레시피 레지스트리 ---
        private ProcessingRecipeData[] _allRecipes;   // DataRegistry에서 로드
        // → see docs/pipeline/data-pipeline.md Part II 섹션 1 for DataRegistry

        // =====================================================
        // 가공소 등록/해제
        // =====================================================

        /// <summary>
        /// 가공소를 등록한다. 건설 완료(BuildingEvents.OnBuildingCompleted) 시 호출.
        /// effectValue에서 초기 슬롯 수를 읽어 슬롯 배열 초기화.
        /// </summary>
        public void RegisterProcessor(BuildingInstance processor)
        {
            int slotCount = (int)processor.Data.effectValue;
            // → see docs/pipeline/data-pipeline.md 섹션 2.4 for effectValue canonical
            _processors[processor] = new ProcessingSlot[slotCount];
            for (int i = 0; i < slotCount; i++)
                _processors[processor][i] = new ProcessingSlot();
        }

        /// <summary>
        /// 가공소를 해제한다. 철거 시 호출.
        /// 진행 중인 가공이 있으면 입력 재료를 인벤토리에 반환한다 (이벤트 통신).
        /// </summary>
        public void UnregisterProcessor(BuildingInstance processor)
        {
            // 진행 중 슬롯의 입력 아이템 반환 → BuildingEvents.RaiseProcessingCancelled
            _processors.Remove(processor);
        }

        // =====================================================
        // 가공 작업 시작
        // =====================================================

        /// <summary>
        /// 가공 작업을 시작한다.
        /// </summary>
        /// <returns>성공 여부</returns>
        public bool StartProcessing(BuildingInstance processor,
                                     ProcessingRecipeData recipe,
                                     string inputCropId,
                                     int inputQuantity)
        {
            // 1. 가공소가 등록되어 있는지 확인
            // 2. 빈 슬롯 검색 (State == Empty인 첫 번째 슬롯)
            // 3. 입력 작물의 카테고리가 recipe.inputCategory에 맞는지 검증
            // 4. inputCropId가 recipe.inputItemId와 일치하는지 검증
            // 5. requiredFacilityTier 검증 (processor.UpgradeLevel >= recipe.requiredFacilityTier)
            // 6. 인벤토리에서 입력 아이템 차감 요청 (BuildingEvents를 통해)
            // 7. ProcessingSlot에 가공 정보 기록, State = Processing
            // 8. BuildingEvents.RaiseProcessingStarted(processor, slotIndex) 이벤트 발행
            return false;
        }

        // =====================================================
        // 시간 경과 처리
        // =====================================================

        /// <summary>
        /// 시간 경과에 따른 가공 진행 처리.
        /// TimeManager.OnHourChanged에 구독하여 매 시간 호출된다.
        /// </summary>
        public void ProcessTimeAdvance(float elapsedHours)
        {
            // 모든 가공소의 모든 활성 슬롯에 대해:
            //   if State == Processing:
            //     remainingHours -= elapsedHours
            //     if remainingHours <= 0:
            //       State = Completed
            //       OutputItemId = recipe.outputItemId
            //       OutputQuantity = recipe.outputQuantity
            //       BuildingEvents.RaiseProcessingComplete(processor, slotIndex) 이벤트 발행
        }

        // =====================================================
        // 완료된 가공품 수거
        // =====================================================

        /// <summary>
        /// 완료된 가공품을 수거한다. 플레이어 인터랙션 시 호출.
        /// </summary>
        /// <returns>수거 성공 여부</returns>
        public bool CollectOutput(BuildingInstance processor, int slotIndex,
                                   out string outputItemId, out int quantity)
        {
            // 1. 슬롯 State == Completed 확인
            // 2. outputItemId, quantity 설정
            // 3. 인벤토리에 출력 아이템 추가 요청 (BuildingEvents를 통해)
            // 4. 슬롯 Clear()
            // 5. BuildingEvents.RaiseProcessingCollected(processor, slotIndex, outputItemId)
            outputItemId = null;
            quantity = 0;
            return false;
        }

        // =====================================================
        // 조회 메서드 (UI용)
        // =====================================================

        /// <summary>특정 가공소의 슬롯 상태를 반환.</summary>
        public IReadOnlyList<ProcessingSlot> GetSlots(BuildingInstance processor)
        {
            return _processors.TryGetValue(processor, out var slots)
                ? slots : Array.Empty<ProcessingSlot>();
        }

        /// <summary>특정 가공소에서 사용 가능한 레시피 목록을 반환.</summary>
        public IReadOnlyList<ProcessingRecipeData> GetAvailableRecipes(BuildingInstance processor)
        {
            // processor.UpgradeLevel로 requiredFacilityTier 필터
            // 빈 슬롯이 있는 경우에만 반환
            return Array.Empty<ProcessingRecipeData>();
        }

        /// <summary>특정 가공소에 빈 슬롯이 있는지 확인.</summary>
        public bool HasEmptySlot(BuildingInstance processor)
        {
            // _processors[processor]에서 State == Empty인 슬롯 존재 여부
            return false;
        }

        // =====================================================
        // 업그레이드 처리
        // =====================================================

        /// <summary>
        /// 가공소 업그레이드 시 슬롯 배열을 확장한다.
        /// BuildingManager.UpgradeBuilding() 에서 호출.
        /// </summary>
        public void OnProcessorUpgraded(BuildingInstance processor, int newSlotCount)
        {
            // 기존 슬롯 보존 + 새 슬롯 추가
            // 기존 배열 복사 → 확장된 새 배열 할당
        }

        // =====================================================
        // 저장/복원
        // =====================================================

        /// <summary>전체 가공 상태를 직렬화.</summary>
        public ProcessingSaveData[] GetSaveData()
        {
            // 모든 가공소의 모든 비-Empty 슬롯을 ProcessingSaveData로 변환
            return Array.Empty<ProcessingSaveData>();
        }

        /// <summary>저장 데이터로부터 가공 상태를 복원.</summary>
        public void LoadSaveData(ProcessingSaveData[] data)
        {
            // ProcessingSaveData.recipeId → DataRegistry에서 ProcessingRecipeData 조회
            // ProcessingSaveData.slotIndex → 해당 슬롯에 상태 복원
        }
    }
}
```

---

### 2.4 클래스 관계 다이어그램

```
┌───────────────────────────────────────────────────────────────┐
│                   BuildingManager (Singleton)                   │
│                                                               │
│  owns:                                                        │
│  ├── WaterTankSystem                                          │
│  ├── GreenhouseSystem                                         │
│  ├── StorageSystem                                            │
│  └── ProcessingSystem  ◄─── 이 문서의 범위                      │
│                                                               │
│  OnBuildingCompleted(inst):                                    │
│    if inst.Data.effectType == Processing:                      │
│        _processingSystem.RegisterProcessor(inst)               │
└───────────────────────────────────────────────────────────────┘
         │ uses                        │ raises events
         ▼                             ▼
┌────────────────────┐     ┌──────────────────────────────┐
│ ProcessingRecipeData│     │     BuildingEvents (static)   │
│ (ScriptableObject)  │     │                              │
│                     │     │  OnProcessingStarted         │
│ (섹션 2.1 참조)     │     │  OnProcessingComplete        │
│                     │     │  OnProcessingCollected       │
└────────────────────┘     └──────────────────────────────┘
         │ references                   │ subscribed by
         ▼                             ▼
┌────────────────────┐     ┌──────────────────────────────┐
│  ProcessingSlot     │     │  InventoryManager (Player)    │
│  (섹션 2.2 참조)    │     │  → 이벤트를 통해 재료 차감/   │
│                     │     │    결과물 추가                │
│  State: Empty/      │     │                              │
│         Processing/ │     │  EconomyManager (Economy)     │
│         Completed   │     │  → 가공품 판매 시 가격 계산   │
└────────────────────┘     └──────────────────────────────┘
```

---

## 3. 가공 프로세스 흐름

### 3.1 재료 투입 → 큐 추가

```
플레이어가 가공소에 인터랙트 (E키)
    ↓
ProcessingUI 표시
    ├── GetAvailableRecipes(processor) → 사용 가능 레시피 목록 표시
    ├── 각 레시피에 대해 인벤토리에 재료가 충분한지 표시 (HasItem 조회)
    └── 빈 슬롯 수 표시
    ↓
플레이어가 레시피 선택 + 확인
    ↓
ProcessingSystem.StartProcessing(processor, recipe, inputCropId, inputQuantity)
    ├── 1) 빈 슬롯 검색 → 없으면 실패 반환
    ├── 2) 카테고리/아이템 ID 검증 → 불일치면 실패 반환
    ├── 3) requiredFacilityTier 검증 → 부족하면 실패 반환
    ├── 4) InventoryManager.RemoveItem(inputCropId, inputQuantity) 요청
    │       → BuildingEvents를 통해 UI 계층이 중재
    │       → (-> see docs/systems/facilities-architecture.md 섹션 6.2 for 이벤트 통신 패턴)
    ├── 5) 인벤토리 차감 성공 시:
    │       ProcessingSlot 설정:
    │         State = Processing
    │         Recipe = recipe
    │         InputCropId = inputCropId
    │         InputQuantity = inputQuantity
    │         RemainingHours = recipe.processingTimeHours
    │         TotalHours = recipe.processingTimeHours
    │         OutputItemId = recipe.outputItemId
    │         OutputQuantity = recipe.outputQuantity
    └── 6) BuildingEvents.RaiseProcessingStarted(processor, slotIndex)
```

### 3.2 틱 처리 (시간 경과)

```
TimeManager.OnHourChanged 이벤트 발생
    ↓
BuildingManager._processingSystem.ProcessTimeAdvance(elapsedHours: 1.0)
    ↓
모든 등록된 가공소에 대해:
    모든 슬롯에 대해:
        if State == Processing:
            RemainingHours -= elapsedHours (= 1.0)
            if RemainingHours <= 0:
                State = Completed
                RemainingHours = 0
                BuildingEvents.RaiseProcessingComplete(processor, slotIndex)
                    ↓
                    ProcessingUI 갱신 (완료 표시)
                    HUD 알림 표시 (선택적)
```

**TimeManager 연동 상세**:
- 구독 대상: `TimeManager.OnHourChanged`
- BuildingManager가 ProcessingSystem.ProcessTimeAdvance()를 호출하는 방식
- BuildingManager의 TimeManager 구독 우선순위: (-> see `docs/systems/facilities-architecture.md` 섹션 3.3 for 건설 진행 처리의 OnDayChanged 구독)
- 가공은 **시간 단위** 진행이므로 OnHourChanged를 사용 (건설은 일 단위이므로 OnDayChanged)

### 3.3 완료 → 결과물 수거

```
플레이어가 가공소에 인터랙트 (E키)
    ↓
ProcessingUI 표시
    ├── 완료된 슬롯에 "수거" 버튼 표시 (State == Completed)
    ├── 진행 중 슬롯에 프로그레스 바 표시 (Progress 프로퍼티)
    └── 빈 슬롯에 "가공 시작" 버튼 표시
    ↓
플레이어가 완료된 슬롯의 "수거" 클릭
    ↓
ProcessingSystem.CollectOutput(processor, slotIndex, out outputItemId, out quantity)
    ├── 1) State == Completed 확인
    ├── 2) InventoryManager.AddItem(outputItemId, quantity) 요청
    │       → UI 계층이 중재 (BuildingEvents → InventoryManager)
    ├── 3) 인벤토리 추가 성공 시: 슬롯 Clear()
    ├── 4) 인벤토리 공간 부족 시: 실패 반환 → "배낭이 가득 찼습니다" 메시지
    └── 5) BuildingEvents.RaiseProcessingCollected(processor, slotIndex, outputItemId)
```

### 3.4 전체 데이터 흐름 다이어그램

```
[InventoryManager]                [ProcessingSystem]              [TimeManager]
     │                                  │                              │
     │  ◄── RemoveItem (재료 차감) ──── StartProcessing()              │
     │                                  │                              │
     │                                  │  ◄── OnHourChanged ──────── │
     │                                  │                              │
     │                                  │  ProcessTimeAdvance()        │
     │                                  │  → RemainingHours 감소       │
     │                                  │  → State: Completed          │
     │                                  │                              │
     │  ◄── AddItem (결과물 추가) ──── CollectOutput()                │
     │                                  │                              │
     ▼                                  ▼                              ▼
[EconomyManager]
     │
     │  GetSellPrice(outputItemId) ← 플레이어가 가공품 판매 시
     ▼
```

---

## 4. 연료 시스템 구현

연료 시스템의 설계 규칙(연료 종류, 소모 시점, 적용 가공소)은 `docs/content/processing-system.md` 섹션 4가 canonical이다. 이 섹션은 런타임 구현 상세만 기술한다.

### 4.1 현재 설계: 베이커리 전용 연료

(-> see `docs/content/processing-system.md` 섹션 4 for 연료 시스템 규칙 canonical)

- **가공소 (일반), 제분소, 발효실**: `fuelCost = 0` (연료 불필요)
- **베이커리**: 레시피별 `fuelCost > 0` (장작 1~2개 소모)
- 연료 아이템 ID: `item_firewood`, 구매처: 잡화 상점(하나)

`StartProcessing()` 내 연료 처리 흐름:

```
if recipe.fuelCost > 0:
    if !InventoryManager.HasItem("item_firewood", recipe.fuelCost):
        return false  // "장작 부족" 실패
    InventoryManager.RemoveItem("item_firewood", recipe.fuelCost)  // 시작 시 즉시 소모
```

### 4.2 향후 연료 시스템 확장 방향

[OPEN] 석탄(coal) 등 추가 연료 도입 시 다음 설계 고려:

```
FuelType enum:
    None,       // 연료 불필요
    Wood,       // 장작 (item_firewood)
    Coal        // 석탄 (효율 2배 -- 향후)
```

ProcessingRecipeData의 현재 `fuelCost` 필드는 연료 수량만 지정하며, 연료 유형은 고정(장작)이다. 다중 연료 지원 시 `requiredFuel: FuelType` 필드를 추가해야 한다.

[RISK] 연료 유형 필드 추가 시 ProcessingRecipeData의 JSON 스키마와 C# 클래스를 PATTERN-005에 따라 동시에 업데이트해야 한다. 또한 기존 세이브 데이터와의 호환성을 SaveMigrator로 처리해야 한다.

---

## 5. 저장/복원 구조

### 5.1 ProcessingSaveData 스키마

ProcessingSaveData의 canonical 정의는 `docs/pipeline/data-pipeline.md` Part II 섹션 2.6이다.

```csharp
// illustrative
namespace SeedMind.Building
{
    /// <summary>
    /// 진행 중인 가공 작업의 직렬화 데이터.
    /// GameSaveData.processing[] 배열의 요소.
    /// (-> see docs/pipeline/data-pipeline.md Part II 섹션 2.6 for canonical 정의)
    /// </summary>
    [System.Serializable]
    public class ProcessingSaveData
    {
        public string processorBuildingId;    // 가공소 BuildingInstance의 식별자
        public int slotIndex;                 // 가공 슬롯 인덱스
        public string recipeId;               // ProcessingRecipeData.dataId 참조
        public string inputCropId;            // 입력 작물 ID
        public int inputQuantity;             // 입력 수량
        public float remainingHours;          // 남은 시간 (게임 내 시간)
        public float totalHours;              // 총 소요 시간
        public int slotState;                 // ProcessingSlot.SlotState의 int 캐스팅
        public string outputItemId;           // 출력 아이템 ID (Completed 상태에서만 유효)
        public int outputQuantity;            // 출력 수량
    }
}
```

> **참고**: data-pipeline.md의 기존 ProcessingSaveData에 `processorBuildingId`, `slotState`, `outputItemId`, `outputQuantity` 필드를 추가 확장한다. 기존 필드(slotIndex, recipeId, inputCropId, inputQuantity, remainingHours, totalHours)는 그대로 유지한다.

[OPEN] data-pipeline.md Part II 섹션 2.6의 ProcessingSaveData를 본 문서의 확장 필드로 업데이트해야 한다. processorBuildingId가 없으면 복원 시 어떤 가공소의 어떤 슬롯인지 매핑할 수 없다.

### 5.2 GameSaveData 내 위치

```csharp
// GameSaveData 통합 루트 (-> see docs/systems/save-load-architecture.md 섹션 2)
public class GameSaveData
{
    // ... 기존 필드 ...
    public ProcessingSaveData[] processing;  // 진행 중인 가공 작업 목록
    // ...
}
```

### 5.3 ISaveable 구현

ProcessingSystem의 저장/로드는 **BuildingManager가 대행**한다. BuildingManager가 ISaveable을 구현하며, GetSaveData() 내에서 ProcessingSystem.GetSaveData()를 호출하여 통합 직렬화한다.

```
BuildingManager.GetSaveData():
    ├── BuildingSaveData[] buildings 생성
    ├── ProcessingSaveData[] processing = _processingSystem.GetSaveData()
    └── GameSaveData에 할당

BuildingManager.LoadSaveData(GameSaveData data):
    ├── buildings 복원 (시설 인스턴스 재생성)
    ├── Processing 타입 시설에 대해 RegisterProcessor 호출
    └── _processingSystem.LoadSaveData(data.processing)
        └── recipeId → DataRegistry에서 ProcessingRecipeData 조회
        └── processorBuildingId → 해당 BuildingInstance 매핑
        └── 슬롯 상태 복원
```

**SaveLoadOrder**: BuildingManager = 60 (-> see `docs/systems/save-load-architecture.md` 섹션 7)

복원 순서상 TimeManager(10) → BuildingManager(60) 순이므로, 복원 시점에 시간 정보가 이미 로드된 상태이다. ProcessingSystem의 RemainingHours는 저장 시점의 값을 그대로 복원하며, 오프라인 시간 경과(게임 종료 후 재시작 시 시간 자동 진행)는 적용하지 않는다.

[RISK] 오프라인 시간 경과를 적용하지 않으면, 플레이어가 "가공 시작 → 게임 종료 → 재시작"으로 가공 시간을 우회할 가능성이 있다. 단, 게임 내 시간이 멈추므로 실질적 이점은 없다. 실시간 기반 가공을 도입하게 되면 재검토 필요.

---

## 6. 이벤트/콜백

### 6.1 BuildingEvents 확장

가공 관련 이벤트는 기존 `BuildingEvents` 정적 이벤트 허브에 포함된다 (-> see `docs/systems/facilities-architecture.md` 섹션 8.3).

```csharp
// illustrative -- 기존 BuildingEvents에 포함된 가공 이벤트
namespace SeedMind.Building
{
    public static class BuildingEvents
    {
        // --- 건설 관련 (기존) ---
        public static event Action<BuildingInstance> OnBuildingPlaced;
        public static event Action<BuildingInstance> OnBuildingCompleted;
        public static event Action<BuildingInstance, int> OnBuildingUpgraded;
        public static event Action<string> OnBuildingRemoved;

        // --- 가공 관련 ---

        /// <summary>가공 작업 시작됨. UI 갱신, 진행 시스템 통보.</summary>
        public static event Action<BuildingInstance, int> OnProcessingStarted;
        // → (processor, slotIndex)

        /// <summary>가공 작업 완료됨. UI에 수거 버튼 표시, 알림 트리거.</summary>
        public static event Action<BuildingInstance, int> OnProcessingComplete;
        // → (processor, slotIndex)

        /// <summary>완료된 가공품 수거됨. 경험치 부여, 통계 기록.</summary>
        public static event Action<BuildingInstance, int, string> OnProcessingCollected;
        // → (processor, slotIndex, outputItemId)

        /// <summary>가공 취소됨 (가공소 철거 시). 재료 반환 처리.</summary>
        public static event Action<BuildingInstance, int, string, int> OnProcessingCancelled;
        // → (processor, slotIndex, inputCropId, inputQuantity)

        // --- 창고 관련 (기존) ---
        public static event Action<BuildingInstance> OnStorageChanged;

        // 내부 발행 메서드
        internal static void RaiseProcessingStarted(BuildingInstance p, int slot)
            => OnProcessingStarted?.Invoke(p, slot);
        internal static void RaiseProcessingComplete(BuildingInstance p, int slot)
            => OnProcessingComplete?.Invoke(p, slot);
        internal static void RaiseProcessingCollected(BuildingInstance p, int slot, string itemId)
            => OnProcessingCollected?.Invoke(p, slot, itemId);
        internal static void RaiseProcessingCancelled(BuildingInstance p, int slot, string cropId, int qty)
            => OnProcessingCancelled?.Invoke(p, slot, cropId, qty);
        // ... 기존 Raise 메서드 생략
    }
}
```

### 6.2 이벤트 구독 맵

| 이벤트 | 발행자 | 구독자 | 용도 |
|--------|--------|--------|------|
| OnProcessingStarted | ProcessingSystem | ProcessingUI | 슬롯 UI 갱신 (빈 → 진행 중) |
| OnProcessingComplete | ProcessingSystem | ProcessingUI, HUDController | 슬롯 UI 갱신 (진행 중 → 완료), HUD 알림 |
| OnProcessingCollected | ProcessingSystem | ProgressionManager, TransactionLog | 경험치 부여(XPSource.Processing), 통계 기록 |
| OnProcessingCancelled | ProcessingSystem | InventoryManager (UI 중재) | 입력 재료 인벤토리 반환 |
| OnBuildingCompleted | BuildingManager | ProcessingSystem | effectType == Processing 시 RegisterProcessor |
| OnBuildingRemoved | BuildingManager | ProcessingSystem | UnregisterProcessor |
| OnBuildingUpgraded | BuildingManager | ProcessingSystem | OnProcessorUpgraded (슬롯 확장) |
| TimeManager.OnHourChanged | TimeManager | BuildingManager → ProcessingSystem | ProcessTimeAdvance 호출 |

### 6.3 ProgressionManager 연동

가공품 수거 시 경험치를 부여한다.

```
BuildingEvents.OnProcessingCollected
    → ProgressionManager가 구독
    → XPSource.Processing으로 경험치 추가
    → 경험치 양: (-> see docs/systems/progression-architecture.md for XP 테이블 canonical)
```

(-> see `docs/systems/progression-architecture.md` for XPSource enum 및 경험치 부여 로직)

---

## 7. UI 구조 (ProcessingUI)

### 7.1 UI 계층

```
Canvas_Overlay (기존)
├── InventoryPanel
├── ShopPanel
├── PausePanel
└── ProcessingPanel (신규)
    ├── RecipeListArea           # 사용 가능 레시피 목록
    │   └── RecipeSlotUI[]       # 개별 레시피 (아이콘, 이름, 재료, 시간)
    ├── SlotStatusArea           # 가공소 슬롯 상태
    │   └── ProcessingSlotUI[]   # 개별 슬롯 (프로그레스 바, 수거 버튼)
    └── CloseButton
```

### 7.2 ProcessingUI 클래스

```csharp
// illustrative
namespace SeedMind.UI
{
    /// <summary>
    /// 가공소 인터랙션 UI.
    /// 레시피 선택, 가공 시작, 진행 상태 표시, 결과물 수거를 처리.
    /// SeedMind.UI 네임스페이스 -- SeedMind.Building과 SeedMind.Player 양쪽을 참조 가능.
    /// </summary>
    public class ProcessingUI : MonoBehaviour
    {
        // --- 참조 ---
        private BuildingManager _buildingManager;    // ProcessingSystem 접근
        private InventoryManager _inventoryManager;  // 재료 확인/차감/추가

        // --- UI 요소 ---
        [SerializeField] private Transform _recipeListParent;
        [SerializeField] private Transform _slotStatusParent;
        [SerializeField] private GameObject _recipeSlotPrefab;
        [SerializeField] private GameObject _processingSlotPrefab;

        // --- 현재 선택된 가공소 ---
        private BuildingInstance _currentProcessor;

        /// <summary>가공소 UI를 열기. 플레이어 인터랙션 시 호출.</summary>
        public void Open(BuildingInstance processor) { /* ... */ }

        /// <summary>레시피 선택 시 호출.</summary>
        public void OnRecipeSelected(ProcessingRecipeData recipe) { /* ... */ }

        /// <summary>수거 버튼 클릭 시 호출.</summary>
        public void OnCollectClicked(int slotIndex) { /* ... */ }

        /// <summary>UI 닫기.</summary>
        public void Close() { /* ... */ }
    }
}
```

ProcessingUI는 **SeedMind.UI 네임스페이스**에 배치되어 BuildingManager(SeedMind.Building)와 InventoryManager(SeedMind.Player) 양쪽을 참조할 수 있다. 이는 project-structure.md 섹션 3의 의존성 규칙을 준수한다 (UI는 모든 것을 참조 가능).

---

# Part II -- MCP 구현 태스크 요약

> 이 섹션은 **ARC-014** 독립 MCP 태스크 문서(`docs/mcp/processing-tasks.md`)로 분리 완료되었다.
> 여기서는 핵심 GameObject 계층, ScriptableObject 에셋 목록만 간략히 기술한다.
> 상세 MCP 도구 호출 명세, 실행 순서, 검증 체크리스트는 `docs/mcp/processing-tasks.md`를 참조한다.

---

### 1. GameObject 계층 (SCN_Farm 추가)

```
SCN_Farm (Scene Root)
├── --- MANAGERS ---
│   ├── GameManager          (기존)
│   ├── TimeManager          (기존)
│   ├── SaveManager          (기존)
│   └── (BuildingManager는 기존 -- ProcessingSystem은 내부 인스턴스)
│
├── --- FARM ---              (기존)
├── --- PLAYER ---            (기존)
├── --- ECONOMY ---           (기존)
│
└── --- UI ---
    └── Canvas_Overlay
        └── ProcessingPanel   (신규)
```

ProcessingSystem은 별도 GameObject가 아니라 BuildingManager 내부의 Plain C# 인스턴스이므로, 씬 계층에 추가 오브젝트가 없다.

### 2. ScriptableObject 에셋 목록

에셋 배치 경로: `Assets/_Project/Data/Recipes/`

(-> see `docs/content/processing-system.md` 섹션 3.5 for 전체 레시피 32종 canonical — 에셋명 패턴: `SO_Recipe_<Type>_<Crop>.asset`, dataId 패턴: `recipe_<type>_<crop>`)

총 32개 레시피 에셋 (가공소 18종 + 제분소 4종 + 발효실 5종 + 베이커리 5종).

> **레시피 수치(priceMultiplier, priceBonus, processingTimeHours, fuelCost 등)**는 에셋에 직접 기재하지 않는다. MCP 태스크에서 에셋 생성 시 canonical 문서(`docs/content/processing-system.md` 섹션 3)의 값을 참조하여 설정한다. (PATTERN-006)

### 3. 스크립트 파일 목록

| 파일 | 경로 | 네임스페이스 |
|------|------|-------------|
| ProcessingRecipeData.cs | Scripts/Building/Data/ | SeedMind.Building.Data |
| ProcessingType.cs | Scripts/Building/Data/ | SeedMind.Building.Data |
| ProcessingSlot.cs | Scripts/Building/Buildings/ | SeedMind.Building |
| ProcessingSystem.cs | Scripts/Building/Buildings/ | SeedMind.Building |
| ProcessingSaveData.cs | Scripts/Building/ | SeedMind.Building |
| ProcessingUI.cs | Scripts/UI/ | SeedMind.UI |

### 4. MCP 태스크 개요 (간략)

```
Phase A: 데이터 레이어
  Step A-1: ProcessingRecipeData.cs, ProcessingType.cs 생성 (섹션 2.1)
  Step A-2: ProcessingSlot.cs 생성 (섹션 2.2)
  Step A-3: ProcessingSaveData.cs 생성/업데이트 (섹션 5.1)
  Step A-4: Unity 컴파일 대기

Phase B: 시스템 레이어
  Step B-1: ProcessingSystem.cs 생성 (섹션 2.3)
  Step B-2: BuildingManager.cs 수정 → _processingSystem 인스턴스 추가
  Step B-3: BuildingEvents.cs 수정 → 가공 이벤트 추가 (섹션 6.1)
  Step B-4: Unity 컴파일 대기

Phase C: SO 에셋 생성
  Step C-1: Data/Recipes/ 폴더 생성
  Step C-2: 32개 레시피 SO 에셋 생성 (값은 processing-system.md 섹션 3 참조)
  Step C-3: DataRegistry에 ProcessingRecipeData 등록

Phase D: UI 레이어
  Step D-1: ProcessingUI.cs 생성 (섹션 7.2)
  Step D-2: ProcessingPanel 프리팹 생성
  Step D-3: Canvas_Overlay에 ProcessingPanel 배치

Phase E: 통합 테스트
  Step E-1: Play Mode 진입
  Step E-2: MCP 콘솔에서 가공소 건설 테스트
  Step E-3: StartProcessing → 시간 경과 → CollectOutput 테스트
  Step E-4: 저장 → 로드 → 가공 상태 복원 확인
```

---

## Cross-references

- `docs/architecture.md` -- 마스터 기술 아키텍처 (Building 섹션)
- `docs/systems/facilities-architecture.md` 섹션 7 -- ProcessingSystem 원본 설계 (ARC-007)
- `docs/systems/inventory-architecture.md` -- InventoryManager (재료 투입/결과물 수거)
- `docs/systems/economy-architecture.md` -- EconomyManager (가공품 판매 가격)
- `docs/systems/time-season-architecture.md` -- TimeManager (OnHourChanged 구독)
- `docs/systems/progression-architecture.md` -- ProgressionManager (XPSource.Processing)
- `docs/systems/save-load-architecture.md` -- GameSaveData.processing[], SaveLoadOrder 할당표
- `docs/pipeline/data-pipeline.md` 섹션 2.5 -- ProcessingRecipeData canonical 필드 정의
- `docs/pipeline/data-pipeline.md` Part II 섹션 2.6 -- ProcessingSaveData canonical 정의
- `docs/content/processing-system.md` -- 레시피 32종 canonical, 연료 시스템, 특화 가공소 3종 (CON-005)
- `docs/content/facilities.md` 섹션 6 -- 가공소(일반) 업그레이드, 슬롯 확장 설계
- `docs/systems/project-structure.md` -- 네임스페이스, 폴더 구조, 의존성 규칙

---

## Open Questions

- [OPEN] `facilities-architecture.md` 섹션 7.1의 ProcessingRecipeData와 본 문서의 확장 필드(inputItemId, inputQuantity, fuelCost, requiredFacilityTier, outputQuantity, description) 동기화 필요. data-pipeline.md를 canonical로 유지할지 본 문서를 canonical로 전환할지 결정 필요.
- [OPEN] data-pipeline.md의 ProcessingSaveData에 `processorBuildingId`, `slotState`, `outputItemId`, `outputQuantity` 필드 추가 필요. 현재 canonical과 본 문서 간 필드 차이 존재.
- [OPEN] 연료 시스템 도입 시점 및 FuelType enum 설계 (섹션 4.2 참조).
- [OPEN] 가공 중 가공소 업그레이드 시 기존 진행 중인 슬롯의 처리 방식 -- 일시 정지 후 업그레이드 완료 시 재개? 아니면 업그레이드 중에도 가공 계속?
- [OPEN] ProcessingRecipeData에 description 필드를 공식 추가할지 -- facilities-architecture.md 섹션 7.1에서도 [OPEN]으로 남겨져 있다.
- [OPEN] 가공품 품질 시스템 -- 입력 작물의 CropQuality가 가공품에 영향을 미칠지. 현재 설계에서는 품질과 무관하게 동일 출력.

---

## Risks

- [RISK] **PATTERN-005 동기화 부담**: ProcessingRecipeData의 확장 필드가 3개 문서(본 문서, facilities-architecture.md, data-pipeline.md)에 걸쳐 있어, 필드 변경 시 동시 업데이트 누락 가능성이 높다. canonical 출처를 data-pipeline.md로 단일화하고 나머지는 참조만 하는 것이 안전하다.
- [RISK] **UI 중재 복잡도**: Player → Building 직접 참조가 금지되므로, 재료 차감/결과물 추가를 UI 계층이 중재해야 한다. UI 로직이 비즈니스 로직을 포함하게 되는 문제. 별도 중재 서비스(ProcessingMediator)를 UI와 분리하여 SeedMind 최상위 네임스페이스에 배치하는 대안 검토 필요.
- [RISK] **오프라인 시간 경과 미적용**: 게임 종료 후 재시작 시 가공 시간이 진행되지 않으므로, 플레이어 기대와 다를 수 있음. 게임 내 시간이 멈추므로 기능적 문제는 아니지만 UX 관점에서 재검토 필요.
- [RISK] **MCP SO 배열/참조 설정**: ProcessingRecipeData의 CropCategory enum 필드를 MCP로 설정할 수 있는지 사전 검증 필요. enum은 int로 설정 가능할 것으로 예상되나, 커스텀 enum 지원 여부 확인 필요. (-> see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)

---

*이 문서는 Claude Code가 기존 시설 아키텍처(facilities-architecture.md)를 기반으로, 가공 시스템을 독립 아키텍처 단위로 확장하여 자율적으로 작성했습니다.*
