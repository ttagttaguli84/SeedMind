# 인벤토리 시스템 MCP 태스크 시퀀스 (ARC-013)

> 인벤토리/아이템 시스템의 스크립트 생성, SO 에셋 생성, UI 프리팹 구성, 씬 배치, 타 시스템 연동, 통합 테스트를 MCP for Unity 태스크로 상세 정의  
> 작성: Claude Code (Opus) | 2026-04-07  
> Phase 1 | 문서 ID: ARC-013

---

## 1. 개요

### 1.1 목적

이 문서는 `docs/systems/inventory-architecture.md` Part II에서 요약된 MCP 구현 계획(Phase A~D)을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 인벤토리 시스템의 데이터 레이어(IInventoryItem 인터페이스 구현, ItemType enum, ItemSlot 구조체), 시스템 레이어(InventoryManager, PlayerInventory), UI 레이어(InventoryPanel, ToolbarPanel, SlotUI 프리팹), 씬 배치, 타 시스템 연동을 완성한다.

### 1.2 의존성

```
인벤토리 시스템 MCP 태스크 의존 관계:
├── SeedMind.Core     (TimeManager, SaveManager, DataRegistry, GameDataSO)
├── SeedMind.Farm     (FarmGrid, FarmTile, CropData, FertilizerData, ToolData, ToolSystem)
├── SeedMind.Economy  (EconomyManager, ShopSystem -- 구매/판매 시 아이템 추가/제거)
├── SeedMind.Building (BuildingManager -- 창고 건설 시 슬롯 확장)
├── SeedMind.Player   (InventoryManager, PlayerInventory -- 본 태스크 생성 대상)
└── SeedMind.UI       (InventoryUI, SlotUI, TooltipUI -- 본 태스크 생성 대상)
```

(-> see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 1.3 전제 조건 (완료된 태스크 의존성)

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조, SCN_Farm 기본 계층 (MANAGERS, PLAYER, UI), Canvas_HUD, Canvas_Overlay |
| ARC-003 | `docs/mcp/farming-tasks.md` | Phase A~C 전체 | FarmGrid, CropData SO 8종, ToolData SO, FertilizerData SO 4종, ToolSystem |
| ARC-007 | `docs/mcp/facilities-tasks.md` | (문서 참조) | BuildingManager, BuildingEvents, StorageSystem |

### 1.4 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `Canvas_HUD` (HUD UI 루트) | ARC-002 Phase B |
| `Canvas_Overlay` (오버레이 UI 루트) | ARC-002 Phase B |
| `DataRegistry` (SO 로드 시스템) | `docs/pipeline/data-pipeline.md` Part II |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A |
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `--- PLAYER ---` (씬 계층 부모) | ARC-002 Phase B |
| `CropData SO 8종` (+ 겨울 전용 3종) | ARC-003 |
| `ToolData SO 9종` (3종 x 3등급) | ARC-015 (tool-upgrade-tasks.md) |
| `FertilizerData SO 4종` | ARC-003 |
| `ToolSystem` (도구 사용 처리) | ARC-003 |
| `EconomyManager` (경제 시스템) | economy-architecture.md 기반 |
| `BuildingManager` (시설 시스템) | ARC-007 |

### 1.5 총 MCP 호출 예상 수

| 태스크 | 설명 | MCP 호출 수 |
|--------|------|------------|
| T-1 | 스크립트 생성 (enum, struct, interface, MonoBehaviour) | 14회 |
| T-2 | SO 에셋 생성/검증 (기존 SO IInventoryItem 확장, DataRegistry 메서드 추가) | 18회 |
| T-3 | UI 프리팹 생성 (SlotUI, ToolbarPanel, InventoryPanel, TooltipUI) | 46회 |
| T-4 | 씬 배치 및 참조 연결 (InventoryManager, PlayerInventory 배치) | 14회 |
| T-5 | 타 시스템 연동 설정 (FarmingSystem, EconomySystem, BuildingManager) | 8회 |
| T-6 | 통합 테스트 시퀀스 | 18회 |
| **합계** | | **~118회** |

[RISK] 총 ~118회 MCP 호출 중 T-3(UI 프리팹 생성)이 46회로 가장 크다. UI 계층 구조가 깊고 컴포넌트 설정이 많기 때문이다. Editor 스크립트(CreateInventoryUI.cs)를 통한 일괄 생성으로 T-3의 46회를 ~6회로 감소시킬 수 있다.

### 1.6 스크립트 목록

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/SeedMind/ItemType.cs` | `ItemType` (enum) | `SeedMind` | T-1 Phase 1 |
| S-02 | `Scripts/SeedMind/IInventoryItem.cs` | `IInventoryItem` (interface) | `SeedMind` | T-1 Phase 1 |
| S-03 | `Scripts/Player/ItemSlot.cs` | `ItemSlot` (struct), `SlotLocation` (enum), `InventoryAction` (enum), `AddResult` (struct), `InventoryChangeInfo` (struct), `UseResult` (struct) | `SeedMind.Player` | T-1 Phase 1 |
| S-04 | `Scripts/Player/InventoryManager.cs` | `InventoryManager` (MonoBehaviour Singleton) | `SeedMind.Player` | T-1 Phase 2 |
| S-05 | `Scripts/Player/PlayerInventory.cs` | `PlayerInventory` (MonoBehaviour) | `SeedMind.Player` | T-1 Phase 2 |
| S-06 | `Scripts/UI/InventoryUI.cs` | `InventoryUI` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 3 |
| S-07 | `Scripts/UI/SlotUI.cs` | `SlotUI` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 3 |
| S-08 | `Scripts/UI/TooltipUI.cs` | `TooltipUI` (MonoBehaviour) | `SeedMind.UI` | T-1 Phase 3 |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: S-01/S-02 -> S-03 -> S-04/S-05 -> S-06~S-08. 각 Phase 사이에 Unity 컴파일 대기(`execute_menu_item`)가 필요하다.

### 1.7 SO 에셋 변경 목록

인벤토리 시스템은 별도 SO를 새로 생성하지 않는다. 기존 SO 클래스(CropData, ToolData, FertilizerData, ProcessingRecipeData)에 IInventoryItem 인터페이스 구현을 추가하는 방식이다 (-> see `docs/systems/inventory-architecture.md` 섹션 1.1, 4.1~4.4).

| SO 타입 | 기존 에셋 수 | 변경 내용 |
|---------|-------------|----------|
| CropData | 8종 (+ 겨울 3종 = 11종) | IInventoryItem 구현 추가 (수확물 + 씨앗 파생) |
| ToolData | 9종 (3종 x 3등급) | IInventoryItem 구현 추가 |
| FertilizerData | 4종 | IInventoryItem 구현 추가 |
| ProcessingRecipeData | (가공 시스템 구현 시 결정) | IInventoryItem 구현 추가 |

### 1.8 씬 GameObject 목록

| # | 오브젝트명 | 부모 | 컴포넌트 | 생성 태스크 |
|---|-----------|------|----------|-----------|
| G-01 | `InventoryManager` | `--- MANAGERS ---` | InventoryManager | T-4-01 |
| G-02 | `PlayerInventory` | `--- PLAYER ---` / Player 프리팹 | PlayerInventory | T-4-02 |
| G-03 | `ToolbarPanel` | `Canvas_HUD` | HorizontalLayoutGroup | T-3-01 |
| G-04 | `InventoryPanel` | `Canvas_HUD` | InventoryUI, GridLayoutGroup | T-3-05 |
| G-05 | `TooltipPanel` | `Canvas_Overlay` | TooltipUI | T-3-09 |

### 1.9 프리팹 목록

| 프리팹명 | 경로 | 설명 |
|---------|------|------|
| `PFB_UI_SlotUI.prefab` | `Assets/_Project/Prefabs/UI/` | 인벤토리/툴바 공용 슬롯 프리팹 |

---

## 2. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋 폴더 생성 | T-2 |
| `create_script` | C# 스크립트 파일 생성 | T-1 |
| `create_object` | 빈 GameObject 생성 | T-3, T-4 |
| `add_component` | MonoBehaviour/UI 컴포넌트 부착 | T-3, T-4, T-5 |
| `set_property` | 컴포넌트 프로퍼티 설정, SO 필드값 설정 | T-2~T-5 전체 |
| `set_parent` | 오브젝트 부모 설정 | T-3, T-4 |
| `save_as_prefab` | GameObject를 프리팹으로 저장 | T-3 |
| `save_scene` | 씬 저장 | T-4, T-5, T-6 |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | T-6 |
| `execute_menu_item` | 편집기 명령 실행 (컴파일 대기 등) | T-1, T-2 |
| `execute_method` | 런타임 메서드 호출 (테스트) | T-6 |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | T-6 |

[RISK] `save_as_prefab` 도구의 가용 여부 및 파라미터 형식 사전 검증 필요. SlotUI 프리팹 저장이 MCP에서 미지원인 경우, Editor 스크립트를 통한 우회 필요 (-> see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련).

---

## 3. 스크립트 생성 (T-1)

**목적**: 인벤토리 시스템에 필요한 모든 C# 스크립트를 생성한다. 컴파일 의존 순서를 지켜 Phase별로 나누어 작성한다.

**전제**: `Assets/_Project/Scripts/` 폴더 구조가 ARC-002에 의해 생성 완료된 상태.

---

### Phase 1: 공통 정의 (enum, interface, struct)

#### Step 1-01: ItemType enum 생성

```
create_script
  path: "Assets/_Project/Scripts/SeedMind/ItemType.cs"
  content:
    namespace SeedMind
    {
        public enum ItemType
        {
            Crop,           // 수확 작물
            Seed,           // 씨앗
            Tool,           // 도구
            Fertilizer,     // 비료
            Consumable,     // 소모품(음식 등)
            Processed,      // 가공품
            Material,       // 건축 재료 (향후 확장)
            Special         // 특수 아이템
        }
    }
```

- **MCP 호출**: 1회
- **검증**: 컴파일 에러 없음 확인

#### Step 1-02: IInventoryItem 인터페이스 생성

```
create_script
  path: "Assets/_Project/Scripts/SeedMind/IInventoryItem.cs"
  content:
    namespace SeedMind
    {
        public interface IInventoryItem
        {
            string ItemId { get; }
            string ItemName { get; }
            ItemType ItemType { get; }
            UnityEngine.Sprite Icon { get; }
            int MaxStackSize { get; }       // → see docs/pipeline/data-pipeline.md 섹션 2.7
            bool Sellable { get; }
        }
    }
```

- **MCP 호출**: 1회
- **의존**: S-01 (ItemType enum) 컴파일 완료

#### Step 1-03: ItemSlot 구조체 및 보조 데이터 구조 생성

```
create_script
  path: "Assets/_Project/Scripts/Player/ItemSlot.cs"
  content:
    // → see docs/systems/inventory-architecture.md 섹션 3.3~3.5 for 전체 정의
    namespace SeedMind.Player
    {
        [System.Serializable]
        public struct ItemSlot { ... }       // itemId, quantity, quality

        public enum SlotLocation { Backpack, Toolbar, Storage }
        public enum InventoryAction { Add, Remove, Move, Split, Merge, Sort, Expand }

        public struct AddResult { ... }            // success, addedQuantity, remainingQuantity
        public struct InventoryChangeInfo { ... }  // location, slotIndex, itemId, action
        public struct UseResult { ... }            // success, message
    }
```

> **구현 상세**: 각 struct/enum의 필드 목록은 `docs/systems/inventory-architecture.md` 섹션 3.3~3.5에 정의된 코드 예시를 그대로 사용한다.

- **MCP 호출**: 1회
- **의존**: S-01, S-02 컴파일 완료

#### Step 1-04: Unity 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회
- **검증**: 콘솔에 컴파일 에러 없음 확인 (`get_console_logs`)

---

### Phase 2: 시스템 클래스 (InventoryManager, PlayerInventory)

#### Step 1-05: InventoryManager 생성

```
create_script
  path: "Assets/_Project/Scripts/Player/InventoryManager.cs"
  content:
    // → see docs/systems/inventory-architecture.md 섹션 2, 5 for 전체 정의
    namespace SeedMind.Player
    {
        public class InventoryManager : MonoBehaviour  // Singleton 패턴
        {
            // 슬롯 데이터
            private ItemSlot[] _backpackSlots;
            private ItemSlot[] _toolbarSlots;
            private int _maxBackpackSlots;        // → see docs/systems/inventory-system.md 섹션 2.1
            private int _toolbarSize;             // → see docs/systems/inventory-system.md 섹션 2.2

            // 이벤트
            public event Action<InventoryChangeInfo> OnBackpackChanged;
            public event Action<InventoryChangeInfo> OnToolbarChanged;
            public event Action<int, int> OnToolbarSelectionChanged;

            // 배낭 연산: AddItem, RemoveItem, HasItem, GetItemCount
            // 슬롯 조작: MoveSlot, SplitSlot, MergeSlots, SortBackpack, ExpandBackpack
            // 툴바 연산: SelectToolbarSlot, SetToolbarItem
            // 세이브/로드: GetSaveData, LoadSaveData
        }
    }
```

> **AddItem/RemoveItem/MoveSlot/SortBackpack 알고리즘**: `docs/systems/inventory-architecture.md` 섹션 5.1~5.4의 의사코드를 구현한다.

- **MCP 호출**: 1회

#### Step 1-06: PlayerInventory 생성

```
create_script
  path: "Assets/_Project/Scripts/Player/PlayerInventory.cs"
  content:
    // → see docs/systems/inventory-architecture.md 섹션 2 (PlayerInventory 클래스 다이어그램) for 전체 정의
    namespace SeedMind.Player
    {
        public class PlayerInventory : MonoBehaviour
        {
            [SerializeField] private InventoryManager _inventoryManager;
            // _toolSystem 참조 (ToolSystem, → see farming-architecture.md)

            // 읽기 전용 프로퍼티: CurrentTool, CurrentSeed, CurrentItem
            // 메서드: UseCurrentItem(FarmTile), ConsumeCurrentItem(int), CycleToolbar(int)
            // 구독: InputActions.Player.ToolSelect, InputActions.Player.UseTool
        }
    }
```

- **MCP 호출**: 1회

#### Step 1-07: Unity 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 1회
- **검증**: 콘솔에 컴파일 에러 없음 확인

---

### Phase 3: UI 클래스 (InventoryUI, SlotUI, TooltipUI)

#### Step 1-08: SlotUI 생성

```
create_script
  path: "Assets/_Project/Scripts/UI/SlotUI.cs"
  content:
    // → see docs/systems/inventory-architecture.md 섹션 2 (SlotUI 클래스 다이어그램) for 전체 정의
    namespace SeedMind.UI
    {
        public class SlotUI : MonoBehaviour,
            IBeginDragHandler, IDragHandler, IEndDragHandler,
            IPointerEnterHandler, IPointerExitHandler
        {
            [SerializeField] private Image _icon;
            [SerializeField] private TextMeshProUGUI _quantityText;
            [SerializeField] private Image _qualityBorder;
            [SerializeField] private Image _selectedHighlight;

            // SetSlot(ItemSlot), SetEmpty(), SetSelected(bool), ShowTooltip()
            // 드래그 앤 드롭: OnBeginDrag, OnDrag, OnEndDrag
            // 호버: OnPointerEnter, OnPointerExit
        }
    }
```

- **MCP 호출**: 1회

#### Step 1-09: InventoryUI 생성

```
create_script
  path: "Assets/_Project/Scripts/UI/InventoryUI.cs"
  content:
    // → see docs/systems/inventory-architecture.md 섹션 2 (InventoryUI 클래스 다이어그램) for 전체 정의
    namespace SeedMind.UI
    {
        public class InventoryUI : MonoBehaviour
        {
            [SerializeField] private InventoryManager _inventoryManager;
            [SerializeField] private GameObject _slotUIPrefab;
            [SerializeField] private Transform _backpackGrid;
            [SerializeField] private Transform _toolbarContainer;
            [SerializeField] private TooltipUI _tooltipPanel;

            private SlotUI[] _backpackSlotUIs;
            private SlotUI[] _toolbarSlotUIs;
            private bool _isOpen;

            // Open(), Close(), Toggle(), RefreshAll()
            // OnSlotDragStart, OnSlotDrop, OnSlotHover, OnSlotRightClick
            // 구독: OnBackpackChanged, OnToolbarChanged, InputActions.Player.Inventory
        }
    }
```

- **MCP 호출**: 1회

#### Step 1-10: TooltipUI 생성

```
create_script
  path: "Assets/_Project/Scripts/UI/TooltipUI.cs"
  content:
    namespace SeedMind.UI
    {
        public class TooltipUI : MonoBehaviour
        {
            [SerializeField] private TextMeshProUGUI _itemNameText;
            [SerializeField] private TextMeshProUGUI _categoryText;
            [SerializeField] private TextMeshProUGUI _descriptionText;
            [SerializeField] private TextMeshProUGUI _priceText;
            [SerializeField] private Image _qualityIcon;
            [SerializeField] private CanvasGroup _canvasGroup;

            // Show(IInventoryItem item, CropQuality quality, Vector2 position)
            // Hide()
            // 표시 지연: 0.3초 (→ see docs/systems/inventory-system.md 섹션 4.3)
        }
    }
```

- **MCP 호출**: 1회

#### Step 1-11: Unity 컴파일 대기 및 최종 검증

```
execute_menu_item
  menu: "Assets/Refresh"

get_console_logs
```

- **MCP 호출**: 2회
- **검증**: 전체 스크립트 S-01~S-08 컴파일 에러 없음 확인

---

**T-1 합계**: 14회 MCP 호출

---

## 4. SO 에셋 생성 및 IInventoryItem 확장 (T-2)

**목적**: 기존 SO 클래스(CropData, ToolData, FertilizerData, ProcessingRecipeData)에 IInventoryItem 인터페이스 구현을 추가하고, DataRegistry에 인벤토리 아이템 조회 메서드를 추가한다.

**전제**: T-1 완료 (IInventoryItem, ItemType 컴파일 완료). ARC-003에서 CropData/ToolData/FertilizerData SO 클래스 및 에셋이 이미 생성된 상태.

---

### Phase 1: 기존 SO 클래스에 IInventoryItem 구현 추가

#### Step 2-01: CropData에 IInventoryItem 구현 추가

기존 `CropData.cs`를 수정하여 IInventoryItem 인터페이스를 구현한다.

```
// CropData.cs 수정 (기존 파일에 인터페이스 구현 추가)
// → see docs/systems/inventory-architecture.md 섹션 4.1 for 구현 상세

public class CropData : GameDataSO, IInventoryItem
{
    public string ItemId => dataId;
    public string ItemName => displayName;
    public ItemType ItemType => SeedMind.ItemType.Crop;
    public Sprite Icon => icon;
    public int MaxStackSize => 99;              // → see docs/pipeline/data-pipeline.md 섹션 2.7
    public bool Sellable => true;

    // 씨앗 파생 속성
    public string SeedItemId => "seed_" + dataId;
    public ItemType SeedItemType => SeedMind.ItemType.Seed;
}
```

> **주의**: CropData는 수확물(Crop)과 씨앗(Seed) 양쪽의 인벤토리 항목 역할을 겸한다. 씨앗 ID는 `"seed_" + dataId` prefix로 구분한다 (-> see `docs/systems/inventory-architecture.md` 섹션 4.1).

- **MCP 호출**: 1회 (`create_script` -- 기존 파일 덮어쓰기)

#### Step 2-02: ToolData에 IInventoryItem 구현 추가

```
// ToolData.cs 수정
// → see docs/systems/inventory-architecture.md 섹션 4.2 for 구현 상세

public class ToolData : GameDataSO, IInventoryItem
{
    public string ItemId => dataId;
    public string ItemName => displayName;
    public ItemType ItemType => SeedMind.ItemType.Tool;
    public Sprite Icon => icon;
    public int MaxStackSize => 1;               // 도구는 스택 불가 (→ see docs/systems/inventory-system.md 섹션 1.1)
    public bool Sellable => false;              // 도구는 판매 불가
}
```

- **MCP 호출**: 1회

#### Step 2-03: FertilizerData에 IInventoryItem 구현 추가

```
// FertilizerData.cs 수정
// → see docs/systems/inventory-architecture.md 섹션 4.3 for 구현 상세

public class FertilizerData : GameDataSO, IInventoryItem
{
    public string ItemId => dataId;
    public string ItemName => displayName;
    public ItemType ItemType => SeedMind.ItemType.Fertilizer;
    public Sprite Icon => icon;
    public int MaxStackSize => 30;              // → see docs/systems/inventory-system.md 섹션 1.1
    public bool Sellable => true;
}
```

- **MCP 호출**: 1회

#### Step 2-04: ProcessingRecipeData에 IInventoryItem 구현 추가

```
// ProcessingRecipeData.cs 수정
// → see docs/systems/inventory-architecture.md 섹션 4.4 for 구현 상세

public class ProcessingRecipeData : GameDataSO, IInventoryItem
{
    public string ItemId => dataId;
    public string ItemName => displayName;
    public ItemType ItemType => SeedMind.ItemType.Processed;
    public Sprite Icon => icon;
    public int MaxStackSize => 30;              // → see docs/systems/inventory-system.md 섹션 1.1
    public bool Sellable => true;
}
```

- **MCP 호출**: 1회

#### Step 2-05: Unity 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"

get_console_logs
```

- **MCP 호출**: 2회
- **검증**: CropData, ToolData, FertilizerData, ProcessingRecipeData가 IInventoryItem을 올바르게 구현하는지 컴파일 에러 없음 확인

---

### Phase 2: DataRegistry에 인벤토리 아이템 조회 메서드 추가

#### Step 2-06: DataRegistry 확장

```
// DataRegistry.cs에 메서드 추가
// → see docs/systems/inventory-architecture.md 섹션 9 Phase C Step C-4

public IInventoryItem GetInventoryItem(string itemId)
{
    // "seed_" prefix 처리: "seed_potato" → CropData("potato") 반환
    if (itemId.StartsWith("seed_"))
    {
        string cropId = itemId.Substring(5);   // "seed_" 제거
        var cropData = Get<CropData>(cropId);
        // CropData를 Seed 타입의 IInventoryItem으로 반환
        return cropData;                        // SeedItemType 사용
    }
    // 일반 아이템: dataId로 직접 조회
    return Get<GameDataSO>(itemId) as IInventoryItem;
}
```

- **MCP 호출**: 1회
- **의존**: Step 2-05 컴파일 완료

---

### Phase 3: 기존 SO 에셋의 인벤토리 필드 검증

기존 CropData SO 에셋(8종 + 겨울 3종), FertilizerData SO 에셋(4종)은 IInventoryItem 구현이 코드 레벨에서 자동 활성화되므로, 에셋별 추가 set_property 호출은 불필요하다. MaxStackSize, Sellable 등은 코드에서 하드코딩된 값을 반환한다.

#### Step 2-07: CropData 에셋 검증 (11종)

```
// 기존 에셋이 IInventoryItem 프로퍼티를 정상 반환하는지 확인
// 수확물 8종: crop_potato, crop_carrot, crop_tomato, crop_corn,
//            crop_strawberry, crop_pumpkin, crop_sunflower, crop_watermelon
// 겨울 3종:  crop_winter_radish, crop_shiitake, crop_spinach
// → see docs/content/crops.md 섹션 3 for 전체 목록

execute_method
  type: "SeedMind.Core.DataRegistry"
  method: "ValidateInventoryItems"
  // 콘솔에 각 에셋의 ItemId, ItemType, MaxStackSize 출력
```

- **MCP 호출**: 1회

#### Step 2-08: ToolData 에셋 검증 (9종)

```
// 도구 9종: hoe_basic~legendary, wateringcan_basic~legendary, sickle_basic~legendary
// → see docs/mcp/tool-upgrade-tasks.md 섹션 1.3.1 for 전체 목록

execute_method
  type: "SeedMind.Core.DataRegistry"
  method: "ValidateToolInventoryItems"
```

- **MCP 호출**: 1회

#### Step 2-09: FertilizerData 에셋 검증 (4종)

```
// 비료 4종: fert_basic, fert_quality, fert_speedgro, fert_organic
// → see docs/systems/inventory-system.md 섹션 5.4 for 전체 목록

execute_method
  type: "SeedMind.Core.DataRegistry"
  method: "ValidateFertilizerInventoryItems"
```

- **MCP 호출**: 1회

#### Step 2-10: 씨앗 파생 ID 검증

```
// "seed_" prefix 파생 ID가 올바르게 매핑되는지 확인
// seed_potato → CropData("potato") 반환 확인

execute_method
  type: "SeedMind.Core.DataRegistry"
  method: "GetInventoryItem"
  args: ["seed_potato"]
  // 콘솔: "seed_potato → CropData(potato), ItemType=Seed, MaxStack=99"
```

- **MCP 호출**: 1회

---

### Phase 4: ItemType별 SO 수량 요약

| ItemType | SO 타입 | 에셋 수 | 비고 |
|----------|---------|---------|------|
| Crop | CropData | 11종 | 8종 기본 + 3종 겨울 전용 (-> see `docs/content/crops.md` 섹션 3) |
| Seed | CropData (파생) | 11종 | CropData에서 `"seed_" + dataId`로 파생, 별도 에셋 없음 |
| Tool | ToolData | 9종 | 3종 x 3등급 (-> see `docs/mcp/tool-upgrade-tasks.md` 섹션 1.3.1) |
| Fertilizer | FertilizerData | 4종 | (-> see `docs/systems/inventory-system.md` 섹션 5.4) |
| Processed | ProcessingRecipeData | (미정) | 가공 시스템 구현 시 결정 (-> see `docs/content/processing-system.md`) |
| Consumable | (미정) | 0종 | [OPEN] 음식 아이템 상세 목록 미정 (-> see `docs/systems/inventory-system.md` 섹션 5.4) |
| Material | (미정) | 2종 | 철 조각, 정제 강철 (-> see `docs/mcp/tool-upgrade-tasks.md` 섹션 1.3.2) |
| Special | (미정) | 0종 | 이벤트 보상 등 (향후 확장) |

#### Step 2-11: 컴파일 최종 확인

```
execute_menu_item
  menu: "Assets/Refresh"

get_console_logs
```

- **MCP 호출**: 2회

---

**T-2 합계**: 18회 MCP 호출 (검증 포함)

[RISK] `create_script`로 기존 SO 클래스 파일을 덮어쓸 때, 기존 코드가 손실될 수 있다. MCP 실행 전에 기존 파일 백업 또는 Git 커밋을 권장한다.

---

## 5. UI 프리팹 생성 (T-3)

**목적**: ToolbarPanel(화면 하단 항상 표시), InventoryPanel(I/Tab 키로 토글), SlotUI 프리팹, TooltipUI를 생성한다.

**전제**: T-1 Phase 3 완료 (SlotUI, InventoryUI, TooltipUI 스크립트 컴파일 완료). Canvas_HUD, Canvas_Overlay가 ARC-002에서 생성 완료된 상태.

---

### Phase 1: SlotUI 프리팹 생성

#### Step 3-01: SlotUI 루트 오브젝트 생성

```
create_object
  name: "SlotUI"
  // 임시 씬 오브젝트로 생성, 프리팹화 후 삭제
```

- **MCP 호출**: 1회

#### Step 3-02: SlotUI 자식 오브젝트 생성

```
create_object  name: "Background"     parent: "SlotUI"
add_component  target: "Background"   type: "UnityEngine.UI.Image"
set_property   target: "Background/Image"  color: "(0.2, 0.2, 0.2, 0.8)"  // 어두운 슬롯 배경

create_object  name: "Icon"           parent: "SlotUI"
add_component  target: "Icon"         type: "UnityEngine.UI.Image"
set_property   target: "Icon/Image"   raycastTarget: false

create_object  name: "QuantityText"   parent: "SlotUI"
add_component  target: "QuantityText" type: "TMPro.TextMeshProUGUI"
set_property   target: "QuantityText/TextMeshProUGUI"
  alignment: "BottomRight"
  fontSize: 14                          // → see docs/systems/inventory-system.md 섹션 4.2

create_object  name: "QualityBorder"  parent: "SlotUI"
add_component  target: "QualityBorder" type: "UnityEngine.UI.Image"
set_property   target: "QualityBorder/Image"  raycastTarget: false

create_object  name: "SelectedHighlight" parent: "SlotUI"
add_component  target: "SelectedHighlight" type: "UnityEngine.UI.Image"
set_property   target: "SelectedHighlight/Image"
  color: "(1, 0.9, 0.3, 0.5)"          // 선택 하이라이트 색상 (노란색 반투명)
  enabled: false                         // 기본 비활성화
```

- **MCP 호출**: 15회 (5 오브젝트 x 3 호출 평균)

#### Step 3-03: SlotUI 컴포넌트 부착 및 참조 연결

```
add_component  target: "SlotUI"  type: "SeedMind.UI.SlotUI"

set_property   target: "SlotUI/SlotUI"
  _icon: ref("SlotUI/Icon/Image")
  _quantityText: ref("SlotUI/QuantityText/TextMeshProUGUI")
  _qualityBorder: ref("SlotUI/QualityBorder/Image")
  _selectedHighlight: ref("SlotUI/SelectedHighlight/Image")
```

- **MCP 호출**: 2회

#### Step 3-04: 프리팹으로 저장

```
save_as_prefab
  source: "SlotUI"
  path: "Assets/_Project/Prefabs/UI/PFB_UI_SlotUI.prefab"
```

- **MCP 호출**: 1회

---

### Phase 2: ToolbarPanel 생성 (Canvas_HUD 하위)

#### Step 3-05: ToolbarPanel 오브젝트 생성

```
create_object  name: "ToolbarPanel"   parent: "Canvas_HUD"

add_component  target: "ToolbarPanel"  type: "UnityEngine.UI.HorizontalLayoutGroup"
set_property   target: "ToolbarPanel/HorizontalLayoutGroup"
  spacing: 4
  childAlignment: "MiddleCenter"
  childForceExpandWidth: false
  childForceExpandHeight: false

add_component  target: "ToolbarPanel"  type: "UnityEngine.UI.ContentSizeFitter"
set_property   target: "ToolbarPanel/ContentSizeFitter"
  horizontalFit: "PreferredSize"
  verticalFit: "PreferredSize"
```

- **MCP 호출**: 5회

#### Step 3-06: 툴바 슬롯 인스턴스 생성 (8칸)

```
// 툴바 슬롯 8개 인스턴스화
// 슬롯 수: 8칸 고정 (→ see docs/systems/inventory-system.md 섹션 2.2)

// 반복 8회:
instantiate_prefab
  prefab: "Assets/_Project/Prefabs/UI/PFB_UI_SlotUI.prefab"
  parent: "ToolbarPanel"
  name: "ToolSlot_0" ~ "ToolSlot_7"
```

> **주의**: MCP에 `instantiate_prefab` 도구가 없을 경우, `create_object` + `add_component` + 참조 설정으로 각 슬롯을 수동 생성해야 한다. 이 경우 호출 수가 8 x 4 = 32회로 증가한다.

- **MCP 호출**: 8회 (또는 수동 생성 시 32회)

[RISK] MCP에서 프리팹 인스턴스화 도구(`instantiate_prefab`)의 가용 여부 미확인. 미지원 시 Editor 스크립트로 일괄 인스턴스화 필요.

#### Step 3-07: ToolbarPanel RectTransform 앵커 설정

```
set_property   target: "ToolbarPanel/RectTransform"
  anchorMin: "(0.5, 0)"         // 하단 중앙
  anchorMax: "(0.5, 0)"
  pivot: "(0.5, 0)"
  anchoredPosition: "(0, 20)"   // 하단에서 20px 위
```

- **MCP 호출**: 1회

---

### Phase 3: InventoryPanel 생성 (Canvas_HUD 하위, 기본 비활성)

#### Step 3-08: InventoryPanel 오브젝트 생성

```
create_object  name: "InventoryPanel"  parent: "Canvas_HUD"

set_property   target: "InventoryPanel"  activeSelf: false   // 기본 비활성

add_component  target: "InventoryPanel"  type: "UnityEngine.UI.Image"
set_property   target: "InventoryPanel/Image"
  color: "(0.1, 0.1, 0.1, 0.9)"       // 어두운 반투명 배경
```

- **MCP 호출**: 4회

#### Step 3-09: 배낭 그리드 컨테이너 생성

```
create_object  name: "BackpackGrid"  parent: "InventoryPanel"

add_component  target: "BackpackGrid"  type: "UnityEngine.UI.GridLayoutGroup"
set_property   target: "BackpackGrid/GridLayoutGroup"
  constraintCount: 5                    // 5열 (→ see docs/systems/inventory-system.md 섹션 2.1)
  constraint: "FixedColumnCount"
  cellSize: "(64, 64)"
  spacing: "(4, 4)"
```

- **MCP 호출**: 4회

#### Step 3-10: 정렬 버튼 생성

```
create_object  name: "SortButton"  parent: "InventoryPanel"
add_component  target: "SortButton"  type: "UnityEngine.UI.Button"

create_object  name: "SortButtonText"  parent: "SortButton"
add_component  target: "SortButtonText"  type: "TMPro.TextMeshProUGUI"
set_property   target: "SortButtonText/TextMeshProUGUI"
  text: "정렬"
  fontSize: 14
```

- **MCP 호출**: 4회

#### Step 3-11: InventoryUI 컴포넌트 부착 및 참조 연결

```
add_component  target: "InventoryPanel"  type: "SeedMind.UI.InventoryUI"

set_property   target: "InventoryPanel/InventoryUI"
  _slotUIPrefab: ref("Assets/_Project/Prefabs/UI/PFB_UI_SlotUI.prefab")
  _backpackGrid: ref("InventoryPanel/BackpackGrid/Transform")
  _toolbarContainer: ref("ToolbarPanel/Transform")
  // _inventoryManager: T-4에서 씬 배치 후 연결
  // _tooltipPanel: Step 3-12에서 연결
```

- **MCP 호출**: 2회

#### Step 3-12: InventoryPanel RectTransform 앵커 설정

```
set_property   target: "InventoryPanel/RectTransform"
  anchorMin: "(0.5, 0.5)"      // 화면 중앙
  anchorMax: "(0.5, 0.5)"
  pivot: "(0.5, 0.5)"
  sizeDelta: "(380, 400)"       // 패널 크기
```

- **MCP 호출**: 1회

---

### Phase 4: TooltipPanel 생성 (Canvas_Overlay 하위)

#### Step 3-13: TooltipPanel 오브젝트 생성

```
create_object  name: "TooltipPanel"  parent: "Canvas_Overlay"

set_property   target: "TooltipPanel"  activeSelf: false     // 기본 비활성

add_component  target: "TooltipPanel"  type: "UnityEngine.CanvasGroup"
set_property   target: "TooltipPanel/CanvasGroup"
  alpha: 0                               // 투명 시작
  blocksRaycasts: false
```

- **MCP 호출**: 4회

#### Step 3-14: 툴팁 내부 텍스트 요소 생성

```
// → see docs/systems/inventory-system.md 섹션 4.3 for 툴팁 레이아웃

create_object  name: "ItemNameText"    parent: "TooltipPanel"
add_component  target: "ItemNameText"  type: "TMPro.TextMeshProUGUI"
set_property   target: "ItemNameText/TextMeshProUGUI"
  fontStyle: "Bold"
  fontSize: 16

create_object  name: "CategoryText"    parent: "TooltipPanel"
add_component  target: "CategoryText"  type: "TMPro.TextMeshProUGUI"
set_property   target: "CategoryText/TextMeshProUGUI"
  fontSize: 12
  color: "(0.7, 0.7, 0.7, 1)"

create_object  name: "DescriptionText" parent: "TooltipPanel"
add_component  target: "DescriptionText" type: "TMPro.TextMeshProUGUI"
set_property   target: "DescriptionText/TextMeshProUGUI"
  fontSize: 13

create_object  name: "PriceText"       parent: "TooltipPanel"
add_component  target: "PriceText"     type: "TMPro.TextMeshProUGUI"
set_property   target: "PriceText/TextMeshProUGUI"
  fontSize: 13
  color: "(1, 0.84, 0, 1)"             // 골드 색상

create_object  name: "QualityIcon"     parent: "TooltipPanel"
add_component  target: "QualityIcon"   type: "UnityEngine.UI.Image"
set_property   target: "QualityIcon/Image"  raycastTarget: false
```

- **MCP 호출**: 15회 (5 요소 x 3 호출 평균)

#### Step 3-15: TooltipUI 컴포넌트 부착 및 참조 연결

```
add_component  target: "TooltipPanel"  type: "SeedMind.UI.TooltipUI"

set_property   target: "TooltipPanel/TooltipUI"
  _itemNameText: ref("TooltipPanel/ItemNameText/TextMeshProUGUI")
  _categoryText: ref("TooltipPanel/CategoryText/TextMeshProUGUI")
  _descriptionText: ref("TooltipPanel/DescriptionText/TextMeshProUGUI")
  _priceText: ref("TooltipPanel/PriceText/TextMeshProUGUI")
  _qualityIcon: ref("TooltipPanel/QualityIcon/Image")
  _canvasGroup: ref("TooltipPanel/CanvasGroup")
```

- **MCP 호출**: 2회

#### Step 3-16: InventoryUI에 TooltipPanel 참조 연결

```
set_property   target: "InventoryPanel/InventoryUI"
  _tooltipPanel: ref("TooltipPanel/TooltipUI")
```

- **MCP 호출**: 1회

---

**T-3 합계**: ~46회 MCP 호출

---

## 6. 씬 배치 및 참조 연결 (T-4)

**목적**: InventoryManager, PlayerInventory를 SCN_Farm 씬에 배치하고, 모든 컴포넌트 간 참조를 연결한다.

**전제**: T-1~T-3 완료. SCN_Farm에 `--- MANAGERS ---`, `--- PLAYER ---` 계층이 존재.

---

#### Step 4-01: InventoryManager GameObject 생성 및 배치

```
create_object  name: "InventoryManager"
set_parent     target: "InventoryManager"  parent: "--- MANAGERS ---"
add_component  target: "InventoryManager"  type: "SeedMind.Player.InventoryManager"

// 초기 배낭 슬롯 수 설정
set_property   target: "InventoryManager/InventoryManager"
  _maxBackpackSlots: 0                    // → 런타임 초기화 시 설정 (→ see docs/systems/inventory-system.md 섹션 2.1)
  _toolbarSize: 0                         // → 런타임 초기화 시 설정 (→ see docs/systems/inventory-system.md 섹션 2.2)
```

> **주의**: `_maxBackpackSlots`와 `_toolbarSize`의 구체적 수치는 MCP 실행 시점에 canonical 문서에서 읽어 입력한다 (-> see `docs/systems/inventory-system.md` 섹션 2.1, 2.2). 본 문서에서 수치를 직접 기재하지 않는다 (PATTERN-006).

- **MCP 호출**: 4회

#### Step 4-02: PlayerInventory 배치 (Player 오브젝트에 부착)

```
// Player 프리팹 또는 씬 내 Player 오브젝트에 PlayerInventory 부착
add_component  target: "Player"  type: "SeedMind.Player.PlayerInventory"

set_property   target: "Player/PlayerInventory"
  _inventoryManager: ref("InventoryManager/InventoryManager")
  // _toolSystem: ref("ToolSystem") -- ARC-003에서 생성된 오브젝트
```

- **MCP 호출**: 2회

#### Step 4-03: InventoryUI에 InventoryManager 참조 연결

```
set_property   target: "InventoryPanel/InventoryUI"
  _inventoryManager: ref("InventoryManager/InventoryManager")
```

- **MCP 호출**: 1회

#### Step 4-04: DontDestroyOnLoad 설정

```
// InventoryManager는 씬 전환 시에도 유지되어야 한다.
// Singleton 패턴에서 Awake()에서 DontDestroyOnLoad(gameObject) 호출.
// 스크립트 코드에 포함되므로 MCP 추가 설정 불필요.
// 단, GameManager가 이미 DontDestroyOnLoad를 관리하는 경우,
// InventoryManager를 GameManager 자식으로 배치하여 함께 유지할 수 있다.
```

> **결정**: InventoryManager는 독립 Singleton으로, Awake()에서 `DontDestroyOnLoad(gameObject)`를 호출한다. GameManager와 독립적으로 씬 전환을 생존한다.

- **MCP 호출**: 0회 (코드 레벨에서 처리)

#### Step 4-05: 초기 툴바 배치 설정

게임 시작 시 툴바에 기본 도구 4종을 배치한다.

```
// 런타임 초기화 로직 (InventoryManager.InitializeDefaults())
// 기본 도구 배치:
//   슬롯 0: "hoe_basic"        (→ see docs/systems/inventory-system.md 섹션 2.2)
//   슬롯 1: "wateringcan_basic"
//   슬롯 2: "sickle_basic"
//   슬롯 3: "axe_basic"        (→ see docs/systems/inventory-system.md 섹션 5.3 [OPEN])
//   슬롯 4~7: 비어 있음
// 구체적 도구 ID는 canonical 문서 참조
```

> **주의**: 이 배치는 코드 레벨에서 `InventoryManager.InitializeDefaults()` 메서드로 처리한다. SO나 씬 설정이 아닌 로직 기반이므로 MCP 호출 불필요. 다만 테스트(T-6)에서 검증한다.

- **MCP 호출**: 0회

#### Step 4-06: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

---

**T-4 합계**: ~14회 MCP 호출 (일부 단계는 코드 레벨 처리)

> 실제 MCP 실행 호출: 8회. 나머지 6회는 set_property 상세 분기.

---

## 7. 타 시스템 연동 (T-5)

**목적**: 인벤토리 시스템과 FarmingSystem, EconomySystem/ShopSystem, BuildingManager, SaveManager 간의 연동 포인트를 설정한다.

**전제**: T-4 완료. 각 연동 대상 시스템이 구현 완료된 상태.

---

### 7.1 FarmingSystem 연동

#### Step 5-01: 씨앗 심기 시 인벤토리 차감 연결

```
// FarmTile.Plant() 또는 FarmingSystem.PlantSeed() 메서드에서
// InventoryManager.RemoveItem("seed_" + cropId, 1, Normal) 호출 추가
// → see docs/systems/inventory-architecture.md 섹션 10.3

// 코드 수정: FarmTile.cs 또는 FarmingSystem.cs
// MCP: create_script (기존 파일 업데이트)
```

- **MCP 호출**: 1회

#### Step 5-02: 수확 시 인벤토리 추가 연결

```
// GrowthSystem.Harvest() 또는 FarmTile.Harvest() 메서드에서
// InventoryManager.AddItem(cropId, yieldAmount, quality) 호출 추가
// → see docs/systems/inventory-architecture.md 섹션 10.3

// 코드 수정: GrowthSystem.cs 또는 FarmTile.cs
// MCP: create_script (기존 파일 업데이트)
```

- **MCP 호출**: 1회

---

### 7.2 EconomySystem / ShopSystem 연동

#### Step 5-03: 구매 시 아이템 추가 연결

```
// ShopSystem.TryBuyItem() 메서드에서
// InventoryManager.AddItem(itemId, qty, Normal) 호출
// → see docs/systems/inventory-architecture.md 섹션 7.1 for 구매 흐름

// 코드 수정: ShopSystem.cs
// MCP: create_script (기존 파일 업데이트)
```

- **MCP 호출**: 1회

#### Step 5-04: 판매 시 아이템 제거 연결

```
// ShopSystem.TrySellCrop() 메서드에서
// InventoryManager.HasItem() 확인 → RemoveItem() 호출
// → see docs/systems/inventory-architecture.md 섹션 7.2 for 판매 흐름

// 코드 수정: ShopSystem.cs
// MCP: create_script (기존 파일 업데이트)
```

- **MCP 호출**: 1회

---

### 7.3 BuildingManager 연동 (창고 슬롯 확장)

#### Step 5-05: 창고 건설 이벤트 구독

```
// InventoryManager.OnEnable()에서
// BuildingManager.OnBuildingConstructed += HandleStorage 구독
// → see docs/systems/inventory-architecture.md 섹션 8.1

// HandleStorage 로직:
//   if building.effectType == StorageExpansion:
//       AddStorageSlots(building.buildingId, building.effectValue)
//       // effectValue = 창고 슬롯 수 (→ see docs/systems/inventory-system.md 섹션 2.3)

// 코드: InventoryManager.cs에 이미 포함 (T-1 Step 1-05에서 작성)
// 추가 MCP 호출 불필요 — 단, BuildingManager 참조 연결 필요
```

```
set_property   target: "InventoryManager/InventoryManager"
  // BuildingManager 이벤트 구독은 코드에서 FindObjectOfType으로 처리
  // 또는 [SerializeField] 참조 직접 연결:
```

- **MCP 호출**: 1회

---

### 7.4 SaveManager 연동

#### Step 5-06: SaveManager에 인벤토리 세이브/로드 등록

```
// SaveManager.Save()에서 InventoryManager.GetSaveData() 호출
// SaveManager.Load()에서 InventoryManager.LoadSaveData() 호출
// → see docs/systems/inventory-architecture.md 섹션 6.2, 6.3 for 저장/로드 흐름
// → see docs/pipeline/data-pipeline.md 섹션 3.2 for InventorySaveData canonical JSON 스키마

// 코드 수정: SaveManager.cs
// MCP: create_script (기존 파일 업데이트)
```

- **MCP 호출**: 1회

#### Step 5-07: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

---

**T-5 합계**: 8회 MCP 호출

---

## 8. 통합 테스트 시퀀스 (T-6)

**목적**: 인벤토리 시스템의 핵심 기능을 MCP 콘솔 명령으로 검증한다.

**전제**: T-1~T-5 완료. Play Mode에서 테스트 실행.

---

### 8.1 테스트 준비

#### Step 6-01: Play Mode 진입

```
enter_play_mode
```

- **MCP 호출**: 1회

#### Step 6-02: 초기 상태 검증

```
execute_method
  type: "SeedMind.Player.InventoryManager"
  method: "Instance.BackpackEmptySlotCount"
  // 예상 결과: 초기 배낭 슬롯 수 (→ see docs/systems/inventory-system.md 섹션 2.1)

get_console_logs
  // "InventoryManager initialized: backpack=N slots, toolbar=8 slots"
```

- **MCP 호출**: 2회

---

### 8.2 아이템 추가/제거 테스트

#### Step 6-03: 씨앗 추가 테스트

```
execute_method
  type: "SeedMind.Player.InventoryManager"
  method: "Instance.AddItem"
  args: ["seed_potato", 10, "Normal"]

get_console_logs
  // "AddItem: seed_potato x10 added to backpack slot 0"
```

- **MCP 호출**: 2회

#### Step 6-04: 아이템 수량 확인

```
execute_method
  type: "SeedMind.Player.InventoryManager"
  method: "Instance.GetItemCount"
  args: ["seed_potato"]
  // 예상 결과: 10

get_console_logs
```

- **MCP 호출**: 2회

#### Step 6-05: 아이템 제거 테스트

```
execute_method
  type: "SeedMind.Player.InventoryManager"
  method: "Instance.RemoveItem"
  args: ["seed_potato", 3, "Normal"]

execute_method
  type: "SeedMind.Player.InventoryManager"
  method: "Instance.GetItemCount"
  args: ["seed_potato"]
  // 예상 결과: 7

get_console_logs
  // "RemoveItem: seed_potato x3 removed. Remaining: 7"
```

- **MCP 호출**: 3회

---

### 8.3 스택 오버플로우 테스트

#### Step 6-06: 최대 스택 초과 추가

```
execute_method
  type: "SeedMind.Player.InventoryManager"
  method: "Instance.AddItem"
  args: ["seed_potato", 200, "Normal"]
  // MaxStackSize=99이므로 (→ see docs/pipeline/data-pipeline.md 섹션 2.7), 7+200=207 → 슬롯 0(99) + 슬롯 1(99) + 슬롯 2(9)

get_console_logs
  // "AddItem: seed_potato x200 added (multi-slot). Slots used: 3"
```

- **MCP 호출**: 2회

---

### 8.4 슬롯 이동/정렬 테스트

#### Step 6-07: MoveSlot 테스트

```
execute_method
  type: "SeedMind.Player.InventoryManager"
  method: "Instance.MoveSlot"
  args: ["Backpack", 0, "Backpack", 5]     // 슬롯 0 → 슬롯 5 이동

get_console_logs
  // "MoveSlot: Backpack[0] → Backpack[5] success"
```

- **MCP 호출**: 2회

#### Step 6-08: SortBackpack 테스트

```
execute_method
  type: "SeedMind.Player.InventoryManager"
  method: "Instance.SortBackpack"

get_console_logs
  // "SortBackpack: X items sorted, Y stacks merged"
```

- **MCP 호출**: 2회

---

### 8.5 Play Mode 종료

#### Step 6-09: Play Mode 종료

```
exit_play_mode
```

- **MCP 호출**: 1회

---

### 8.6 테스트 체크리스트

| # | 테스트 항목 | 검증 방법 | 예상 결과 |
|---|------------|----------|----------|
| TC-01 | 초기 배낭 슬롯 수 | `BackpackEmptySlotCount` 조회 | 초기값과 일치 (-> see inventory-system.md 섹션 2.1) |
| TC-02 | 초기 툴바 도구 배치 | `ToolbarSlots` 조회 | 슬롯 0~3에 기본 도구 4종 배치 |
| TC-03 | AddItem 기본 동작 | `AddItem` + `GetItemCount` | 추가 수량 정확히 반영 |
| TC-04 | RemoveItem 기본 동작 | `RemoveItem` + `GetItemCount` | 제거 수량 정확히 반영 |
| TC-05 | 스택 오버플로우 | 99 초과 추가 | 다중 슬롯으로 분배 |
| TC-06 | 배낭 가득 참 | 빈 슬롯 없이 추가 시도 | `AddResult.remaining > 0` |
| TC-07 | MoveSlot | 슬롯 간 이동 | 출발/도착 슬롯 정확히 변경 |
| TC-08 | SortBackpack | 정렬 실행 | ItemType → itemId 순서 정렬, 스택 병합 |
| TC-09 | 도구 → 배낭 이동 | 도구를 배낭으로 MoveSlot | 이동 성공 (도구 분실 불가) |
| TC-10 | 비도구 → 툴바 이동 | 씨앗을 툴바로 MoveSlot | 이동 성공 (8칸 범용 툴바) |
| TC-11 | 품질별 슬롯 분리 | 같은 작물 Normal/Silver 추가 | 별도 슬롯에 저장 |
| TC-12 | UI 연동 | AddItem 후 UI 갱신 | OnBackpackChanged 이벤트 발행, UI 반영 확인 (콘솔 로그) |

[RISK] TC-12(UI 연동)는 MCP 콘솔 테스트로는 시각적 검증이 불가능하다. 이벤트 발행 여부는 콘솔 로그로 확인하되, 실제 UI 렌더링은 수동 Play Mode 테스트가 필요하다.

[RISK] 드래그 앤 드롭(Step D-5 in inventory-architecture.md)은 마우스 입력 시뮬레이션이 필요하여 MCP 콘솔 테스트로 검증할 수 없다 (-> see `docs/systems/inventory-architecture.md` Risks 섹션).

---

**T-6 합계**: 18회 MCP 호출

---

## Cross-references

| 관련 문서 | 참조 내용 |
|-----------|-----------|
| `docs/systems/inventory-architecture.md` | C# 클래스 설계, 알고리즘, MCP Phase A~D 개요 (ARC-013 기반) |
| `docs/systems/inventory-system.md` | 인벤토리 게임 설계 -- 슬롯 수, 배낭 업그레이드 경로, 아이템 분류 (DES-005 canonical) |
| `docs/content/crops.md` | 작물 카탈로그 -- CropData SO 11종(8 기본 + 3 겨울)의 ItemID 매핑 (CON-001) |
| `docs/pipeline/data-pipeline.md` | IInventoryItem 인터페이스, InventorySaveData JSON 스키마, GameDataSO 베이스 (ARC-004) |
| `docs/systems/project-structure.md` | 네임스페이스, asmdef 정의, 폴더 구조 |
| `docs/systems/farming-architecture.md` | CropData, FertilizerData, ToolData SO 정의 (섹션 4.1~4.3) |
| `docs/systems/economy-architecture.md` | ShopSystem, EconomyManager 연동 (섹션 5) |
| `docs/mcp/scene-setup-tasks.md` | ARC-002 -- 폴더 구조, 씬 계층, Canvas 생성 |
| `docs/mcp/farming-tasks.md` | ARC-003 -- CropData/ToolData/FertilizerData SO 생성, FarmGrid |
| `docs/mcp/tool-upgrade-tasks.md` | ARC-015 -- ToolData SO 9종 (3등급 체계) |
| `docs/mcp/facilities-tasks.md` | ARC-007 -- BuildingManager, StorageSystem |
| `docs/mcp/npc-shop-tasks.md` | ARC-009 -- NPC/상점 시스템, ShopSystem 연동 |
| `docs/mcp/inventory-design-analysis.md` | 게임 설계 관점 GAP 분석 (GAP-01~05) 및 추가 Phase E~G 권장 태스크 |
| `docs/mcp/processing-tasks.md` | 가공 시스템 MCP 태스크 -- ProcessingRecipeData SO 에셋 수 확정 시 참조 |

---

## Open Questions [OPEN]

| 태그 | 섹션 | 내용 |
|------|------|------|
| [OPEN] | 1.5 | `instantiate_prefab` MCP 도구의 가용 여부 미확인. Step 3-06에서 프리팹 인스턴스화에 사용하는데, 미지원 시 수동 생성으로 호출 수가 크게 증가한다. |
| [OPEN] | 4 섹션 Phase 4 | 음식 아이템(Consumable) SO 에셋이 미정. 가공품 중 일부를 음식으로 사용할지, 별도 SO를 만들지 미결정 (-> see `docs/systems/inventory-system.md` 섹션 5.4). |
| [OPEN] | 4 섹션 Phase 4 | ProcessingRecipeData의 에셋 수가 가공 시스템 구현에 의존하여 미정. 가공 시스템 MCP 태스크에서 결정 필요. |
| [OPEN] | 6 Step 4-05 | 도끼(Axe)의 기본 도구 포함 여부 미결정. design.md 도구 목록과의 정합성 검토 필요 (-> see `docs/systems/inventory-system.md` 섹션 5.3). |
| [OPEN] | 전체 (GAP-05) | 출하함(Shipping Bin) MCP 태스크가 이 문서에 포함되지 않음. inventory-system.md 섹션 2.4에 출하함 세이브 데이터(ShippingBinSaveData) 정의가 필요하며, 구현 태스크(Phase F: ShippingBin 클래스, ShippingBinUI, 06:00 자동 판매 연동)는 별도 문서 또는 후속 태스크로 분리 예정 (-> see `docs/mcp/inventory-design-analysis.md` 섹션 2.2 Phase F). |

---

## Risks [RISK]

| 태그 | 섹션 | 내용 | 완화 방안 |
|------|------|------|----------|
| [RISK] | 1.5 | 총 ~118회 MCP 호출 중 T-3(UI)이 46회. UI 계층이 복잡하여 호출 수가 크다. | Editor 스크립트(CreateInventoryUI.cs) 일괄 생성으로 T-3을 ~6회로 감소 가능. |
| [RISK] | 1.6 | 스크립트 컴파일 에러 시 MCP `add_component` 실패. | Phase별 컴파일 대기 및 콘솔 로그 확인. |
| [RISK] | 2 | `save_as_prefab` MCP 도구의 가용 여부 미확인. | Editor 스크립트 우회 또는 수동 프리팹 생성. |
| [RISK] | 4 T-2 | `create_script`로 기존 SO 클래스 덮어쓰기 시 코드 손실. | MCP 실행 전 Git 커밋으로 백업. |
| [RISK] | 5 T-3 Phase 2 | `instantiate_prefab` MCP 도구 미지원 시 SlotUI 프리팹 인스턴스화 불가. 수동 생성 시 호출 수 32회로 증가. | Editor 스크립트로 일괄 인스턴스화. |
| [RISK] | 8.6 TC-12 | UI 렌더링 시각적 검증은 MCP 콘솔 테스트로 불가. | 이벤트 발행은 콘솔 로그로 확인, 렌더링은 수동 Play Mode 테스트. |
| [RISK] | 8.6 | 드래그 앤 드롭 기능은 마우스 입력 시뮬레이션이 필요하여 MCP 테스트 불가. | 수동 Play Mode 테스트 필수 (-> see inventory-architecture.md Risks). |
| [RISK] | 7.2 | Economy asmdef에서 Player 참조 추가 필요 (ShopSystem → InventoryManager). 순환 의존 발생 가능성. | 이벤트 기반 디커플링 또는 Core에 공통 인터페이스 배치 (-> see inventory-architecture.md 섹션 7.3). |

---

*이 문서는 Claude Code가 기존 아키텍처 문서, 디자인 문서, MCP 태스크 문서와의 일관성을 고려하여 자율적으로 작성했습니다.*
