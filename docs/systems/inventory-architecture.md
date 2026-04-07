# 인벤토리/아이템 시스템 기술 아키텍처

> InventoryManager, ItemData, ItemStack, PlayerInventory, InventoryUI의 클래스 설계, 슬롯 관리 로직, 세이브/로드 연동, 경제 시스템 인터페이스, MCP 구현 계획  
> 작성: Claude Code (Opus) | 2026-04-06

---

## Context

이 문서는 SeedMind의 인벤토리/아이템 시스템에 대한 기술 아키텍처를 정의한다. 인벤토리 시스템은 플레이어가 소지하는 모든 아이템(씨앗, 작물, 도구, 비료, 가공품)을 관리하며, 경작/경제/진행 시스템과의 교차점 역할을 한다. 플레이어 조작의 핵심인 도구 선택/사용과, 상점 거래 시의 아이템 추가/제거가 이 시스템을 통해 처리된다.

**설계 목표**:
- 배낭 슬롯과 툴바 슬롯을 명확히 분리하여 UI와 로직의 복잡도를 낮춘다
- 아이템 식별에 문자열 ID를 사용하여 직렬화 안전성을 확보한다
- 기존 SO(CropData, ToolData, FertilizerData)와 IInventoryItem 인터페이스로 자연스럽게 통합한다
- 모든 슬롯 변경이 이벤트로 전파되어 UI와 다른 시스템이 반응할 수 있어야 한다

---

# Part I -- 시스템 설계

---

## 1. 핵심 설계 결정

### 1.1 아이템 메타데이터 통합 방식: IInventoryItem 인터페이스 (방식 B) 채택

`docs/pipeline/data-pipeline.md` 섹션 2.7에서 검토된 두 방식 중 **방식 B(IInventoryItem 인터페이스)**를 채택한다.

| 대안 | 장점 | 단점 | 결정 |
|------|------|------|------|
| A: 별도 ItemData SO | 아이템 전용 필드 자유 추가 | 에셋 수 2배 증가, SO 간 참조 복잡 | 기각 |
| **B: IInventoryItem 인터페이스** | 기존 SO 재사용, 에셋 수 최소화, 참조 자연스러움 | 공통 인터페이스 구현 강제 | **채택** |

**근거**: CropData, ToolData, FertilizerData 등이 이미 `GameDataSO`를 상속하며(`docs/pipeline/data-pipeline.md` Part II 섹션 1), `dataId`와 `displayName`을 공통으로 가진다. IInventoryItem은 이 위에 인벤토리 관련 속성(MaxStackSize, Sellable 등)만 추가하면 되므로 에셋 이중화 없이 통합 가능하다.

### 1.2 툴바와 배낭 슬롯: 독립 슬롯 방식 채택

| 대안 | 장점 | 단점 | 결정 |
|------|------|------|------|
| 배낭 앞 N슬롯 공유 | 구현 단순, 슬롯 총 수 일치 | 배낭 정렬 시 툴바 깨짐, 도구/소모품 혼재 | 기각 |
| **독립 슬롯** | 툴바는 도구 전용, 배낭 정렬 독립적 | 관리 슬롯 배열 2개 | **채택** |

**근거**: data-pipeline.md의 InventorySaveData가 이미 `slots`(배낭)와 `toolSlots`(도구)를 분리 정의하고 있다. 도구는 스택되지 않고 고유한 tier를 가지므로, 씨앗/작물과 같은 슬롯 배열에 넣으면 정렬/스택 로직이 복잡해진다. 독립 분리가 기존 세이브 스키마와도 일치한다.

### 1.3 아이템 ID 체계: 문자열 ID (string)

| 대안 | 장점 | 단점 | 결정 |
|------|------|------|------|
| SO 직접 참조 | 타입 안전, 에디터에서 드래그 가능 | JSON 직렬화 불가, 에셋 리네임 시 깨짐 | 기각 |
| int ID | 비교 빠름 | 가독성 낮음, 충돌 관리 필요 | 기각 |
| **string ID** | 가독성 높음, JSON 친화, DataRegistry 매핑 | 비교 약간 느림 (무시 가능 수준) | **채택** |

**근거**: 기존 `GameDataSO.dataId` 체계와 일치하며, 세이브 JSON에서 `"itemId": "seed_potato"` 형태로 사람이 읽을 수 있다. DataRegistry를 통해 dataId -> SO 참조를 O(1)로 조회한다 (-> see `docs/pipeline/data-pipeline.md` Part II 섹션 1).

---

## 2. 클래스 다이어그램

```
+---------------------------------------------------------------------------+
|                      SeedMind.Player (네임스페이스)                         |
+---------------------------------------------------------------------------+

+--------------------------------------------------------------+
|        InventoryManager (MonoBehaviour, Singleton)             |
|--------------------------------------------------------------|
|  [슬롯 데이터]                                                 |
|  - _backpackSlots: ItemSlot[]                                |
|  - _toolbarSlots: ItemSlot[]                                 |
|  - _maxBackpackSlots: int        // → see data-pipeline.md   |
|  - _toolbarSize: int             // → see data-pipeline.md   |
|                                                              |
|  [읽기 전용 프로퍼티]                                           |
|  + BackpackSlots: IReadOnlyList<ItemSlot>                    |
|  + ToolbarSlots: IReadOnlyList<ItemSlot>                     |
|  + ToolbarSelectedIndex: int                                 |
|  + SelectedToolbarItem: IInventoryItem                       |
|  + BackpackEmptySlotCount: int                               |
|                                                              |
|  [이벤트]                                                     |
|  + OnBackpackChanged: Action<InventoryChangeInfo>            |
|  + OnToolbarChanged: Action<InventoryChangeInfo>             |
|  + OnToolbarSelectionChanged: Action<int, int>   // old, new |
|  + OnSlotClicked: Action<SlotLocation, int>                  |
|                                                              |
|  [배낭 연산]                                                   |
|  + AddItem(string itemId, int qty, CropQuality q,            |
|       HarvestOrigin origin = Outdoor): AddResult             |  // [FIX-034]
|  + RemoveItem(string itemId, int qty, CropQuality q,         |
|       HarvestOrigin origin = Outdoor): bool                  |  // [FIX-034]
|  + HasItem(string itemId, int qty): bool                     |
|  + GetItemCount(string itemId,                               |
|       CropQuality q = Normal,                                |
|       HarvestOrigin origin = Outdoor): int                   |  // [FIX-034]
|  + MoveSlot(SlotLocation src, int srcIdx,                    |
|             SlotLocation dst, int dstIdx): bool              |
|  + SplitSlot(SlotLocation loc, int idx, int splitQty): bool  |
|  + MergeSlots(SlotLocation loc, int srcIdx, int dstIdx): bool|
|  + SortBackpack(): void                                      |
|  + ExpandBackpack(int additionalSlots): void                 |
|                                                              |
|  [툴바 연산]                                                   |
|  + SelectToolbarSlot(int index): void                        |
|  + SetToolbarItem(int index, string toolId): bool            |
|                                                              |
|  [세이브/로드]                                                 |
|  + GetSaveData(): InventorySaveData                          |
|  + LoadSaveData(InventorySaveData data): void                |
|                                                              |
|  [구독]                                                       |
|  + OnEnable():                                               |
|      BuildingManager.OnBuildingConstructed += HandleStorage   |
+--------------------------------------------------------------+
         |                              |
         | manages                      | uses
         v                              v
+------------------------+    +------------------------------+
| ItemSlot (struct)      |    | IInventoryItem (interface)   |
|------------------------|    |------------------------------|
| + ItemId: string       |    | + ItemId: string             |
| + Quantity: int        |    | + ItemName: string           |
| + Quality: CropQuality |    | + ItemType: ItemType enum    |
| + Origin: HarvestOrigin|    | + Icon: Sprite               |  // [FIX-034]
| + IsEmpty: bool        |    | + MaxStackSize: int          |
|                        |    | + Sellable: bool             |
| + CanStackWith(other)  |    +------------------------------+
| + RemainingCapacity()  |
+------------------------+         ^  ^  ^  ^
                                   |  |  |  |
                     +-------------+  |  |  +------------+
                     |                |  |               |
              CropData         ToolData  FertilizerData  ProcessingRecipeData
              (GameDataSO)     (GameDataSO) (GameDataSO) (GameDataSO)
              implements       implements  implements    implements
              IInventoryItem   IInventory  IInventory    IInventoryItem
                               Item        Item


+--------------------------------------------------------------+
|           PlayerInventory (MonoBehaviour)                      |
|--------------------------------------------------------------|
|  [참조]                                                       |
|  - _inventoryManager: InventoryManager                       |
|  - _toolSystem: ToolSystem                                   |
|                                                              |
|  [읽기 전용 프로퍼티]                                           |
|  + CurrentTool: ToolData                                     |
|  + CurrentSeed: CropData                                     |
|  + CurrentItem: IInventoryItem                               |
|                                                              |
|  [메서드]                                                     |
|  + UseCurrentItem(FarmTile target): UseResult                |
|  + ConsumeCurrentItem(int qty): bool                         |
|  + CycleToolbar(int direction): void                         |
|                                                              |
|  [구독]                                                       |
|  + OnEnable():                                               |
|      InputActions.Player.ToolSelect += HandleToolSelect      |
|      InputActions.Player.UseTool += HandleUseTool            |
+--------------------------------------------------------------+


+--------------------------------------------------------------+
|              InventoryUI (MonoBehaviour)                       |
|--------------------------------------------------------------|
|  [참조]                                                       |
|  - _inventoryManager: InventoryManager                       |
|  - _slotUIPrefab: GameObject                                 |
|  - _backpackGrid: Transform                                  |
|  - _toolbarContainer: Transform                              |
|  - _tooltipPanel: TooltipUI                                  |
|                                                              |
|  [상태]                                                       |
|  - _backpackSlotUIs: SlotUI[]                                |
|  - _toolbarSlotUIs: SlotUI[]                                 |
|  - _draggedSlot: DragData                                    |
|  - _isOpen: bool                                             |
|                                                              |
|  [메서드]                                                     |
|  + Open(): void                                              |
|  + Close(): void                                             |
|  + Toggle(): void                                            |
|  + RefreshAll(): void                                        |
|  - OnSlotDragStart(SlotUI slot): void                        |
|  - OnSlotDrop(SlotUI source, SlotUI target): void            |
|  - OnSlotHover(SlotUI slot): void                            |
|  - OnSlotRightClick(SlotUI slot): void                       |
|                                                              |
|  [구독]                                                       |
|  + OnEnable():                                               |
|      InventoryManager.OnBackpackChanged += RefreshBackpack   |
|      InventoryManager.OnToolbarChanged += RefreshToolbar     |
|      InputActions.Player.Inventory += HandleToggle           |
+--------------------------------------------------------------+
         |
         | renders
         v
+------------------------+
| SlotUI (MonoBehaviour) |
|------------------------|
| - _icon: Image         |
| - _quantityText: TMP   |
| - _qualityBorder: Image|
| - _selectedHighlight   |
|                        |
| + SetSlot(ItemSlot)    |
| + SetEmpty()           |
| + SetSelected(bool)    |
| + ShowTooltip()        |
+------------------------+
```

---

## 3. 핵심 데이터 구조 상세

### 3.1 IInventoryItem 인터페이스

```csharp
namespace SeedMind
{
    /// <summary>
    /// 인벤토리에 저장 가능한 아이템의 공통 계약.
    /// CropData, ToolData, FertilizerData, FishData 등이 구현한다.
    /// </summary>
    public interface IInventoryItem
    {
        string ItemId { get; }          // GameDataSO.dataId와 동일
        string ItemName { get; }        // GameDataSO.displayName과 동일
        ItemType ItemType { get; }      // 아이템 분류
        Sprite Icon { get; }            // UI 아이콘
        int MaxStackSize { get; }       // 슬롯당 최대 수량 (→ see data-pipeline.md 섹션 2.7)
        bool Sellable { get; }          // 판매 가능 여부
    }
}
```

### 3.2 ItemType enum

```csharp
namespace SeedMind
{
    public enum ItemType
    {
        Crop,           // 수확 작물 -- 스택 가능 (→ see data-pipeline.md 섹션 2.7 for maxStack)
        Seed,           // 씨앗 -- 스택 가능
        Tool,           // 도구 -- 스택 불가, 도구 슬롯 전용
        Fertilizer,     // 비료 -- 스택 가능
        Consumable,     // 소모품(음식 등) -- 스택 가능 (→ see inventory-system.md 섹션 1.1 for Consumable 정의)
        Processed,      // 가공품 -- 스택 가능
        Material,       // 건축 재료 -- 스택 가능 (향후 확장)
        Fish,           // 물고기 -- 스택 가능, 품질별 별도 슬롯 (→ see docs/systems/fishing-system.md, FIX-053)
        Gathered,       // 채집물 -- 스택 가능, 품질별 별도 슬롯 (→ see docs/systems/gathering-system.md, ARC-031)
        Special         // 특수 아이템 -- 스택 불가 (이벤트 보상 등)
    }
}
```

### 3.3 ItemSlot 구조체

```csharp
namespace SeedMind.Player
{
    /// <summary>
    /// 인벤토리 슬롯 하나를 표현하는 값 타입.
    /// </summary>
    [System.Serializable]
    public struct ItemSlot
    {
        public string itemId;           // 빈 슬롯이면 null 또는 ""
        public int quantity;            // 0이면 빈 슬롯
        public CropQuality quality;     // 작물일 때만 의미 있음 (Tool/Seed 등은 Normal 고정)
        public HarvestOrigin origin;    // [FIX-034] 수확 출처 (Crop 타입만 의미 있음, 나머지는 Outdoor 고정)
                                        // → see docs/systems/economy-architecture.md 섹션 3.10

        public bool IsEmpty => string.IsNullOrEmpty(itemId) || quantity <= 0;

        /// <summary>
        /// 같은 아이템으로 스택 가능한지 판정.
        /// [FIX-034] 조건: 같은 itemId, 같은 quality, 같은 origin, 대상 아이템이 stackable.
        /// </summary>
        public bool CanStackWith(ItemSlot other)
        {
            if (IsEmpty || other.IsEmpty) return false;
            return itemId == other.itemId
                && quality == other.quality
                && origin == other.origin;   // [FIX-034]
        }

        /// <summary>
        /// 현재 슬롯의 잔여 스택 용량.
        /// DataRegistry에서 IInventoryItem을 조회하여 MaxStackSize를 확인한다.
        /// </summary>
        public int RemainingCapacity(int maxStackSize)
        {
            return maxStackSize - quantity; // → maxStackSize는 IInventoryItem.MaxStackSize
        }

        public static ItemSlot Empty => new ItemSlot
        {
            itemId = null, quantity = 0,
            quality = CropQuality.Normal,
            origin = HarvestOrigin.Outdoor   // [FIX-034] 기본값
        };
    }
}
```

### 3.4 SlotLocation enum

```csharp
namespace SeedMind.Player
{
    /// <summary>
    /// 슬롯 이동/조작 시 출발지/도착지를 구분하는 열거형.
    /// </summary>
    public enum SlotLocation
    {
        Backpack,       // 배낭 슬롯
        Toolbar,        // 툴바 슬롯
        Storage         // 창고 슬롯 (창고 건설 후 활성화, → see inventory-system.md 섹션 2.3)
    }
}
```

### 3.5 보조 데이터 구조

```csharp
namespace SeedMind.Player
{
    /// <summary>
    /// AddItem 결과. 남은 수량 > 0이면 배낭이 가득 찬 것.
    /// </summary>
    public struct AddResult
    {
        public bool success;            // 1개라도 추가되었으면 true
        public int addedQuantity;       // 실제 추가된 수량
        public int remainingQuantity;   // 추가하지 못한 잔여 수량
    }

    /// <summary>
    /// 인벤토리 변경 이벤트 정보.
    /// </summary>
    public struct InventoryChangeInfo
    {
        public SlotLocation location;   // 변경된 슬롯 위치
        public int slotIndex;           // 변경된 슬롯 인덱스 (-1이면 전체 갱신)
        public string itemId;           // 관련 아이템 ID (빈 슬롯이면 null)
        public InventoryAction action;  // 변경 유형
    }

    public enum InventoryAction
    {
        Add,
        Remove,
        Move,
        Split,
        Merge,
        Sort,
        Expand
    }

    /// <summary>
    /// 아이템 사용 결과.
    /// </summary>
    public struct UseResult
    {
        public bool success;
        public string message;          // 실패 사유 또는 성공 메시지 (디버그용)
    }
}
```

---

## 4. IInventoryItem 구현: 기존 SO 확장

기존 GameDataSO 하위 클래스들이 IInventoryItem을 구현하는 방법을 정의한다. 필드값(MaxStackSize 등)은 canonical 문서를 참조한다.

### 4.1 CropData의 IInventoryItem 구현

```csharp
// CropData는 수확 작물과 씨앗 양쪽의 인벤토리 항목 역할을 한다.
// 씨앗은 별도 SO 없이 CropData에서 파생: itemId = "seed_" + dataId
// → see docs/systems/farming-architecture.md 섹션 4.1 for CropData canonical 필드

public class CropData : GameDataSO, IInventoryItem
{
    // --- IInventoryItem 구현 ---
    public string ItemId => dataId;                         // "potato", "tomato" 등
    public string ItemName => displayName;
    public ItemType ItemType => SeedMind.ItemType.Crop;     // 수확물로서의 타입
    public Sprite Icon => icon;
    public int MaxStackSize => 99;                          // → see docs/pipeline/data-pipeline.md 섹션 2.7
    public bool Sellable => true;

    // 씨앗으로서의 파생 속성 (InventoryManager가 "seed_" prefix로 구분)
    public string SeedItemId => "seed_" + dataId;
    public ItemType SeedItemType => SeedMind.ItemType.Seed;
    // 씨앗 MaxStackSize도 99 (→ see data-pipeline.md 섹션 2.7)
}
```

### 4.2 ToolData의 IInventoryItem 구현

```csharp
// → see docs/systems/farming-architecture.md 섹션 4.3, docs/pipeline/data-pipeline.md 섹션 2.3 for ToolData canonical 필드

public class ToolData : GameDataSO, IInventoryItem
{
    public string ItemId => dataId;                         // "hoe_basic", "watering_can_copper" 등
    public string ItemName => displayName;
    public ItemType ItemType => SeedMind.ItemType.Tool;
    public Sprite Icon => icon;
    public int MaxStackSize => 1;                           // 도구는 스택 불가
    public bool Sellable => false;                          // 도구는 판매 불가
}
```

### 4.3 FertilizerData의 IInventoryItem 구현

```csharp
// → see docs/systems/farming-architecture.md 섹션 4.2 for FertilizerData canonical 필드

public class FertilizerData : GameDataSO, IInventoryItem
{
    public string ItemId => dataId;
    public string ItemName => displayName;
    public ItemType ItemType => SeedMind.ItemType.Fertilizer;
    public Sprite Icon => icon;
    public int MaxStackSize => 30;                          // → see docs/systems/inventory-system.md 섹션 1.1
    public bool Sellable => true;
}
```

### 4.4 FishData의 IInventoryItem 구현

```csharp
// FishData는 낚시로 획득한 물고기의 인벤토리 항목 역할을 한다.
// → see docs/systems/fishing-architecture.md 섹션 3 for FishData canonical 필드
// → see docs/pipeline/data-pipeline.md Part I 섹션 2.4 for ItemType.Fish

public class FishData : GameDataSO, IInventoryItem
{
    // --- IInventoryItem 구현 ---
    public string ItemId => dataId;                         // "fish_carp", "fish_goldfish" 등
    public string ItemName => displayName;
    public ItemType ItemType => SeedMind.ItemType.Fish;     // 물고기 타입
    public Sprite Icon => icon;
    public int MaxStackSize => maxStackSize;                // 어종별 개별 설정 (→ see docs/pipeline/data-pipeline.md 섹션 2.7)
    public bool Sellable => true;

    // --- 어종 고유 필드 (→ see docs/systems/fishing-architecture.md 섹션 3) ---
    // rarity, basePrice, seasonAvailability, timeWeights,
    // weatherBonus, minigameDifficulty, targetZoneWidthMul,
    // moveSpeed, expReward 등은 FishData SO에 정의
}
```

### 4.5 ProcessingRecipeData의 IInventoryItem 구현

```csharp
// 가공품은 ProcessingRecipeData를 통해 인벤토리에 등록된다.
// → see docs/pipeline/data-pipeline.md 섹션 2.5 for ProcessingRecipeData canonical 필드

public class ProcessingRecipeData : GameDataSO, IInventoryItem
{
    public string ItemId => dataId;                         // "jam_potato", "juice_tomato" 등
    public string ItemName => displayName;
    public ItemType ItemType => SeedMind.ItemType.Processed;
    public Sprite Icon => icon;
    public int MaxStackSize => 30;                          // → see docs/systems/inventory-system.md 섹션 1.1
    public bool Sellable => true;
}
```

---

## 5. InventoryManager 핵심 로직

### 5.1 AddItem 알고리즘

```
AddItem(itemId, qty, quality, origin = HarvestOrigin.Outdoor):  // [FIX-034] origin 파라미터 추가
    1) DataRegistry에서 IInventoryItem 조회 → item
    2) item.ItemType == Tool 이면 → 거부 (도구는 SetToolbarItem으로만 추가)
    3) maxStack = item.MaxStackSize
    4) 기존 슬롯 스택 시도:
       for each slot in _backpackSlots:
           if slot.itemId == itemId AND slot.quality == quality
              AND slot.origin == origin:    // [FIX-034] origin 매칭 추가
               canAdd = min(qty, slot.RemainingCapacity(maxStack))
               slot.quantity += canAdd
               qty -= canAdd
               fire OnBackpackChanged(slot index, Add)
               if qty == 0: return success
    5) 빈 슬롯에 새 스택 생성:
       for each slot in _backpackSlots:
           if slot.IsEmpty:
               canAdd = min(qty, maxStack)
               slot = new ItemSlot(itemId, canAdd, quality, origin)  // [FIX-034]
               qty -= canAdd
               fire OnBackpackChanged(slot index, Add)
               if qty == 0: return success
    6) return AddResult { success = (addedQty > 0), remaining = qty }
```

### 5.2 RemoveItem 알고리즘

```
RemoveItem(itemId, qty, quality, origin = HarvestOrigin.Outdoor):  // [FIX-034] origin 파라미터 추가
    1) 먼저 총 보유량 확인: if GetItemCount(itemId, quality, origin) < qty → return false
    2) 뒤에서부터 제거 (마지막 슬롯부터):
       for i = _backpackSlots.Length - 1 downto 0:
           if slot.itemId == itemId AND slot.quality == quality
              AND slot.origin == origin:    // [FIX-034]
               canRemove = min(qty, slot.quantity)
               slot.quantity -= canRemove
               qty -= canRemove
               if slot.quantity == 0: slot = ItemSlot.Empty
               fire OnBackpackChanged(i, Remove)
               if qty == 0: return true
    3) return true (이미 1단계에서 보유량 검증 완료)
```

### 5.3 MoveSlot 알고리즘

```
MoveSlot(srcLoc, srcIdx, dstLoc, dstIdx):
    1) src = GetSlot(srcLoc, srcIdx)
    2) dst = GetSlot(dstLoc, dstIdx)

    // 도구 슬롯 제약 검증
    3) if dstLoc == Toolbar:
           srcItem = DataRegistry.Get(src.itemId)
           if srcItem.ItemType != Tool → return false
       // 도구는 툴바에서 배낭으로 이동 가능 (분실은 불가, 배낭 보관 허용)
       // → see inventory-system.md 섹션 2.2 "도구를 툴바에서 제거해도 배낭에 보관됨"
       // if srcLoc == Toolbar AND dstLoc == Backpack AND src.ItemType == Tool → 허용

    4) if dst.IsEmpty:
           SetSlot(dstLoc, dstIdx, src)
           SetSlot(srcLoc, srcIdx, ItemSlot.Empty)
       elif src.CanStackWith(dst):
           // 스택 병합 시도
           maxStack = DataRegistry.Get(src.itemId).MaxStackSize
           canMerge = min(src.quantity, dst.RemainingCapacity(maxStack))
           dst.quantity += canMerge
           src.quantity -= canMerge
           if src.quantity == 0: src = ItemSlot.Empty
       else:
           // 교환 (swap)
           SetSlot(srcLoc, srcIdx, dst)
           SetSlot(dstLoc, dstIdx, src)

    5) fire OnBackpackChanged / OnToolbarChanged
    6) return true
```

### 5.4 SortBackpack 알고리즘

```
SortBackpack():
    1) 모든 비어있지 않은 슬롯을 리스트로 추출
    2) 같은 itemId + quality + origin인 슬롯끼리 수량 합산 → 최소 슬롯 수로 재배치
       // [FIX-034] origin이 다른 슬롯은 합산하지 않는다 (스택 키: itemId+quality+origin)
    3) 정렬 기준: ItemType (enum 순서) → itemId (알파벳) → origin (Outdoor < Greenhouse)
                  → quality (역순, Gold→Normal)
       // [FIX-034] origin 정렬 추가
    4) 정렬된 리스트를 _backpackSlots 앞쪽부터 채움, 나머지는 Empty
    5) fire OnBackpackChanged(-1, Sort)  // -1 = 전체 갱신
```

---

## 6. 세이브/로드 연동

### 6.1 InventorySaveData 구조

세이브 데이터 구조는 `docs/pipeline/data-pipeline.md` 섹션 3.2의 InventorySaveData를 그대로 사용한다. 아래는 해당 정의의 아키텍처 관점 매핑이다.

```
InventorySaveData (→ see docs/pipeline/data-pipeline.md 섹션 3.2 for canonical JSON 스키마)
├── slots[]: 배낭 슬롯 배열
│   ├── itemId: string       → ItemSlot.itemId
│   ├── itemType: string     → ItemType enum의 문자열 표현
│   ├── quantity: int        → ItemSlot.quantity
│   ├── quality: string      → CropQuality enum의 문자열 표현
│   └── origin: string       → ItemSlot.origin (HarvestOrigin enum) [FIX-034]
│                               null/미지정 시 Outdoor 기본값 (하위 호환)
├── maxSlots: int            → _maxBackpackSlots
├── toolbarSlots[]: 툴바 슬롯 배열 (8칸 범용)
│   ├── itemId: string       → ItemSlot.itemId (씨앗/비료/도구 등 혼재 가능)
│   └── quantity: int        → ItemSlot.quantity
├── toolbarSelectedIndex: int → _toolbarSelectedIndex (추가 필드)
└── wateringCanCharges: int  → 물뿌리개 잔여 충전량 (→ see farming-system.md)
```

[결정] `toolbarSelectedIndex` 필드는 data-pipeline.md 섹션 3.2 InventorySaveData JSON 스키마에 추가 완료.

### 6.2 저장 흐름

```
SaveManager.Save()
    │
    ├── InventoryManager.GetSaveData()
    │       ├── _backpackSlots → slots[] 변환 (빈 슬롯 제외)
    │       ├── _toolbarSlots → toolSlots[] 변환
    │       ├── _maxBackpackSlots → maxSlots
    │       ├── _toolbarSelectedIndex → toolbarSelectedIndex
    │       └── ToolSystem.WateringCanCharges → wateringCanCharges
    │
    └── GameSaveData에 InventorySaveData 포함 (→ see data-pipeline.md Part II 섹션 3)
```

### 6.3 로드 흐름

```
SaveManager.Load()
    │
    ├── GameSaveData 역직렬화 (→ see data-pipeline.md Part II 섹션 4)
    │
    └── InventoryManager.LoadSaveData(data)
            │
            ├── 1) _maxBackpackSlots = data.maxSlots
            ├── 2) _backpackSlots 초기화 (maxSlots 크기)
            ├── 3) data.slots 순회:
            │       │ DataRegistry.Get(slot.itemId) 존재 확인
            │       │ 존재하면 → ItemSlot 생성, 배낭에 배치
            │       └ 존재하지 않으면 → 경고 로그, 해당 슬롯 스킵
            ├── 4) data.toolSlots 순회:
            │       │ DataRegistry.Get(tool.toolId) 존재 확인
            │       └ ToolData 매핑 후 도구 슬롯 배치
            ├── 5) _toolbarSelectedIndex = data.toolbarSelectedIndex
            └── 6) fire OnBackpackChanged(-1, Sort) + OnToolbarChanged(-1, Sort)
```

[RISK] 세이브 파일에 존재하지만 현재 빌드에서 제거된 아이템 ID가 있을 수 있다. 로드 시 DataRegistry 조회 실패를 graceful하게 처리해야 한다. data-pipeline.md 섹션 3.5의 검증 규칙과 동일 패턴 적용.

---

## 7. 경제 시스템 연동

### 7.1 구매 흐름

```
ShopUI → ShopSystem.TryBuyItem(item, qty)
    │
    ├── (기존 경제 로직: 가격 계산, 골드 차감)
    │   → see docs/systems/economy-architecture.md 섹션 5.2 for 상세 흐름
    │
    ├── 아이템 타입 판별:
    │   ├── Seed → InventoryManager.AddItem("seed_" + cropId, qty, Normal)
    │   ├── Fertilizer → InventoryManager.AddItem(fertilizerId, qty, Normal)
    │   └── Tool → InventoryManager.SetToolbarItem(slotIdx, toolId)
    │
    └── AddResult 확인:
        ├── success → 거래 완료, OnTransactionComplete 발행
        └── remaining > 0 → "배낭이 가득 찼습니다" UI 피드백
            → 추가되지 못한 수량만큼 골드 환불: EconomyManager.AddGold()
```

### 7.2 판매 흐름

```
ShopUI → ShopSystem.TrySellCrop(crop, qty, quality, origin)   // [FIX-034] origin
    │
    ├── 1) InventoryManager.HasItem(cropId, qty) 확인
    │      → false면 거래 거부
    │
    ├── 2) 가격 계산
    │      → EconomyManager.GetSellPrice(crop, quality, origin)  // [FIX-034]
    │      → see docs/systems/economy-architecture.md 섹션 3, 3.10 for 가격 공식
    │
    ├── 3) InventoryManager.RemoveItem(cropId, qty, quality, origin)  // [FIX-034]
    │
    ├── 4) EconomyManager.AddGold(totalPrice, "sell_crop")
    │
    └── 5) OnTransactionComplete 발행
```

### 7.3 인터페이스 계약

InventoryManager는 경제 시스템에 직접 의존하지 않는다. 연동은 ShopSystem이 중재한다.

```
[InventoryManager]  <----(참조)---- [ShopSystem] ----(참조)----> [EconomyManager]
      (SeedMind.Player)                (SeedMind.Economy)           (SeedMind.Economy)
```

Assembly Definition 의존성 (-> see `docs/systems/project-structure.md` 섹션 5):
- `SeedMind.Player.asmdef` → Core, Farm (Economy에 의존하지 않음)
- `SeedMind.Economy.asmdef` → Core, Farm, **Player** (ShopSystem이 InventoryManager 참조)

[RISK] 현재 project-structure.md의 asmdef 정의에서 Economy → Player 의존이 명시되어 있지 않다. ShopSystem이 InventoryManager를 참조하려면 Economy.asmdef에 Player 참조를 추가해야 한다. 순환 의존이 발생하지 않는지 확인 필요.

[OPEN] Economy → Player 의존 대신 이벤트 기반 디커플링을 도입할 수 있다. ShopSystem이 `Action<string, int>` 형태의 콜백을 받아 InventoryManager를 직접 참조하지 않는 방식. 성능/복잡도 트레이드오프 검토 필요.

---

## 8. 진행 시스템 연동

### 8.1 창고 슬롯 확장

창고(Storage) 건설 시 창고 슬롯이 추가된다. 배낭 슬롯은 상점에서 별도 업그레이드로 확장한다 (-> see `docs/systems/inventory-system.md` 섹션 2.1, 2.3 및 `docs/design.md` 섹션 4.6 for 시설 정보).

[RISK] 기존 설계(배낭 슬롯 확장)에서 변경됨. 창고 건설은 외부 저장 공간을 추가하고, 배낭 업그레이드(1,000G/3,000G/8,000G)는 상점에서 별도 구매하는 구조이다. BuildingEffectType.StorageExpansion은 창고 슬롯 배열 추가로 처리해야 한다.

```
BuildingManager.OnBuildingConstructed(BuildingData building)
    │
    └── InventoryManager.HandleStorage(building)
            │
            ├── if building.effectType == BuildingEffectType.StorageExpansion:
            │       // 창고 슬롯 배열 추가 (배낭 슬롯 확장이 아님)
            │       AddStorageSlots(building.buildingId, building.effectValue)
            │       // effectValue = 창고 슬롯 수 (30칸, → see inventory-system.md 섹션 2.3)
            │       // effectValue는 docs/pipeline/data-pipeline.md 섹션 2.4 BuildingData에서 조회
            │
            └── fire OnBackpackChanged(-1, Expand)  // Storage 슬롯 갱신 이벤트로 대체 예정
```

### 8.2 아이템 해금

새 아이템의 구매/획득 가능 여부는 ProgressionManager의 해금 상태에 따른다.

```
ShopSystem.FilterByPlayerLevel(level):
    │
    ├── ProgressionManager.IsUnlocked(UnlockType.Crop, cropId)
    │   → see docs/systems/progression-architecture.md for UnlockRegistry 로직
    │
    └── 해금되지 않은 아이템은 ShopUI에서 잠금 표시 또는 비노출
```

---

# Part II -- MCP 구현 계획

---

## 9. MCP 태스크 시퀀스

### Phase A: InventoryManager 코어

```
Step A-1: Scripts/Player/ 에 IInventoryItem.cs 작성
          → namespace SeedMind
          → IInventoryItem 인터페이스 정의 (ItemId, ItemName, ItemType, Icon, MaxStackSize, Sellable)

Step A-2: Scripts/Player/ 에 ItemSlot.cs 작성
          → namespace SeedMind.Player
          → struct ItemSlot 정의
          → enum SlotLocation, InventoryAction 정의
          → struct AddResult, InventoryChangeInfo, UseResult 정의

Step A-3: Scripts/Player/ 에 InventoryManager.cs 작성
          → namespace SeedMind.Player
          → MonoBehaviour, Singleton 패턴 (GameManager와 동일 방식)
          → ItemSlot[] _backpackSlots, _toolbarSlots 초기화
          → AddItem, RemoveItem, HasItem, GetItemCount 구현
          → MoveSlot, SplitSlot, MergeSlots, SortBackpack 구현
          → 이벤트 OnBackpackChanged, OnToolbarChanged, OnToolbarSelectionChanged 선언
          → GetSaveData(), LoadSaveData() 구현

Step A-4: SCN_Farm 씬에 "InventoryManager" 빈 GameObject 생성
          → MCP: CreateGameObject("InventoryManager")
          → MCP: AddComponent<InventoryManager>()
          → MCP: SetPosition(0, 0, 0) (매니저이므로 위치 무관)

Step A-5: 기존 CropData, ToolData, FertilizerData, ProcessingRecipeData, FishData에 IInventoryItem 구현 추가
          → 각 클래스에 인터페이스 멤버 구현
          → MaxStackSize, Sellable 값은 ItemType에 따라 결정
          → FishData: ItemType.Fish, MaxStackSize=maxStackSize(어종별), Sellable=true
```

### Phase B: PlayerInventory 연결

```
Step B-1: Scripts/Player/ 에 PlayerInventory.cs 작성
          → namespace SeedMind.Player
          → InventoryManager, ToolSystem 참조
          → CurrentTool, CurrentSeed, CurrentItem 프로퍼티
          → UseCurrentItem(FarmTile) 구현 -- ToolSystem.UseTool() 또는 FarmTile.Plant() 위임
          → CycleToolbar(direction) 구현

Step B-2: Player 프리팹에 PlayerInventory 컴포넌트 추가
          → MCP: AddComponent<PlayerInventory>(playerGameObject)
          → MCP: SetReference(PlayerInventory._inventoryManager, inventoryManagerObj)

Step B-3: 콘솔 테스트
          → InventoryManager.AddItem("seed_potato", 10, Normal) 호출
          → InventoryManager.GetItemCount("seed_potato") == 10 확인
          → InventoryManager.RemoveItem("seed_potato", 3, Normal) 호출
          → GetItemCount == 7 확인
          → Console: "Backpack slot 0: seed_potato x7"
```

### Phase C: ItemData SO 에셋 생성

```
Step C-1: 씨앗 아이템 (CropData가 이미 씨앗 역할을 겸함 -- 별도 에셋 불필요)
          → 기존 CropData 에셋 8종에 IInventoryItem 속성이 자동 활성화됨
          → "seed_" + dataId로 씨앗 식별

Step C-2: 도구 에셋 확인 (기존 ToolData 에셋)
          → 이미 data-pipeline.md에 정의된 17~22개 ToolData 에셋 활용
          → 각 ToolData가 IInventoryItem을 구현하므로 추가 에셋 불필요

Step C-3: 비료 에셋 확인 (기존 FertilizerData 에셋)
          → 이미 data-pipeline.md에 정의된 4개 FertilizerData 에셋 활용

Step C-4: DataRegistry에 IInventoryItem 조회 메서드 추가
          → GetInventoryItem(string itemId): IInventoryItem
          → "seed_" prefix 처리: "seed_potato" → CropData("potato") 반환 (SeedItemType으로)
```

### Phase D: InventoryUI 구성

```
Step D-1: Canvas_HUD 하위에 "ToolbarPanel" 생성
          → MCP: CreateGameObject("ToolbarPanel", parent=Canvas_HUD)
          → HorizontalLayoutGroup 컴포넌트 추가
          → 툴바 슬롯 수만큼 SlotUI 프리팹 인스턴스 생성
             (→ see inventory-system.md 섹션 2.2: toolbarSlotCount = 8칸)
             [결정] 8칸 범용 툴바 채택. data-pipeline.md toolbarSlots를 8개 범용 슬롯으로 업데이트 완료 (FIX-001 해소)

Step D-2: "InventoryPanel" 팝업 UI 생성
          → MCP: CreateGameObject("InventoryPanel", parent=Canvas_HUD)
          → 기본 상태: SetActive(false)
          → GridLayoutGroup 컴포넌트 추가 (열 수: 5, → see inventory-system.md 섹션 2.1 "5열 x N행")
          → 배낭 슬롯 수만큼 SlotUI 프리팹 인스턴스 생성
             (→ see inventory-system.md 섹션 2.1: 초기 15슬롯, 최대 30슬롯)

Step D-3: SlotUI 프리팹 생성
          → MCP: CreateGameObject("SlotUI")
          → 자식: Icon (Image), QuantityText (TextMeshPro), QualityBorder (Image), SelectedHighlight (Image)
          → SlotUI.cs 컴포넌트 추가
          → 프리팹으로 저장: Prefabs/UI/SlotUI.prefab

Step D-4: InventoryUI.cs 작성 및 연결
          → namespace SeedMind.UI
          → _inventoryManager 참조 설정
          → Open/Close/Toggle 메서드
          → OnBackpackChanged, OnToolbarChanged 구독 → RefreshAll()
          → Tab 키 입력 → Toggle()

Step D-5: 드래그 앤 드롭 구현
          → SlotUI에 IBeginDragHandler, IDragHandler, IEndDragHandler 구현
          → 드래그 시작: 아이콘 복사 → 마우스 따라다님
          → 드롭: InventoryManager.MoveSlot(src, dst) 호출
          → 슬롯 외 영역에 드롭: 아이템 버리기 확인 팝업 (향후)

Step D-6: 툴팁 구현
          → SlotUI에 IPointerEnterHandler, IPointerExitHandler 구현
          → Hover 시: TooltipUI에 아이템 정보 표시 (이름, 설명, 가격)
          → 가격 정보: EconomyManager.GetSellPrice() 조회 (→ see economy-architecture.md)

Step D-7: 통합 테스트
          → Play Mode 진입
          → AddItem("seed_potato", 10) → 배낭 UI에 감자 씨앗 x10 표시 확인
          → Tab 키 → 인벤토리 패널 열림/닫힘 확인
          → 슬롯 드래그 → MoveSlot 동작 확인
          → Console: "InventoryUI refreshed: 1 slots occupied"
```

---

## 10. 이벤트 연동 요약

### 10.1 InventoryManager가 발행하는 이벤트

| 이벤트 | 파라미터 | 발행 시점 | 구독자 |
|--------|----------|----------|--------|
| `OnBackpackChanged` | `InventoryChangeInfo` | Add/Remove/Move/Split/Merge/Sort/Expand | InventoryUI, ShopUI (판매 가능 목록 갱신) |
| `OnToolbarChanged` | `InventoryChangeInfo` | 도구 추가/변경 | InventoryUI (툴바 갱신), PlayerInventory |
| `OnToolbarSelectionChanged` | `(int oldIndex, int newIndex)` | 숫자키/스크롤로 선택 변경 | PlayerInventory (CurrentTool 갱신), HUDController (선택 하이라이트) |

### 10.2 InventoryManager가 구독하는 이벤트

| 이벤트 소스 | 이벤트 | 처리 |
|------------|--------|------|
| `BuildingManager` | `OnBuildingConstructed` | 창고 건설 시 HandleStorage() → AddStorageSlots() (배낭 슬롯 확장 아님, 창고 슬롯 배열 추가) |

### 10.3 타 시스템의 InventoryManager 호출

| 호출자 | 메서드 | 시점 |
|--------|--------|------|
| ShopSystem | `AddItem()` | 상점 구매 완료 후 |
| ShopSystem | `RemoveItem()` | 상점 판매 처리 중 |
| ShopSystem | `HasItem()` | 판매 전 보유량 확인 |
| FarmTile | `RemoveItem("seed_X", 1)` | 씨앗 심기 소비 |
| FarmTile / GrowthSystem | `AddItem(cropId, yield, quality)` | 작물 수확 |
| PlayerInventory | `SelectToolbarSlot()` | 숫자키 입력 |

---

## 11. 네임스페이스 및 파일 배치

```
Assets/_Project/Scripts/
├── SeedMind/                        # 최상위 네임스페이스 공용 정의
│   ├── IInventoryItem.cs            # interface IInventoryItem
│   └── ItemType.cs                  # enum ItemType (기존 data-pipeline.md 정의)
│
├── Player/                          # SeedMind.Player 네임스페이스
│   ├── InventoryManager.cs          # 싱글톤, 슬롯 관리 핵심
│   ├── PlayerInventory.cs           # 플레이어 부착, 도구 사용
│   ├── ItemSlot.cs                  # struct ItemSlot + 보조 구조체
│   └── Data/                        # SeedMind.Player.Data
│       └── (플레이어 관련 SO, 현재 없음)
│
└── UI/                              # SeedMind.UI 네임스페이스
    ├── InventoryUI.cs               # 배낭 UI
    ├── SlotUI.cs                    # 개별 슬롯 렌더링
    └── TooltipUI.cs                 # 툴팁 패널
```

(-> see `docs/systems/project-structure.md` for 전체 프로젝트 구조)

---

## Risks

- [RISK] **Economy asmdef 순환 의존**: ShopSystem(Economy)이 InventoryManager(Player)를 참조하려면 Economy.asmdef에 Player 의존을 추가해야 한다. 현재 Player.asmdef가 Economy에 의존하지 않으므로 단방향은 유지되지만, 향후 Player가 Economy 기능을 필요로 하면 순환이 발생한다. 이벤트 기반 디커플링 또는 Core에 공통 인터페이스 배치로 대비해야 한다.

- [RISK] **"seed_" prefix 파생 ID 충돌**: CropData의 dataId가 "seed_"로 시작하는 경우(예: "seed_special") 씨앗 ID와 충돌한다. CropData.dataId 네이밍 규칙에서 "seed_" prefix를 금지하는 규칙을 data-pipeline.md의 ID 유일성 섹션에 추가해야 한다.

- [RISK] **MCP로 드래그 앤 드롭 테스트 불가**: 드래그 앤 드롭은 마우스 입력 시뮬레이션이 필요하여 MCP 콘솔 테스트로는 검증할 수 없다. Phase D-5 이후 수동 Play Mode 테스트가 필요하다.

- [RISK] **ItemSlot struct 복사 의미론**: ItemSlot이 struct이므로 배열에서 꺼내 수정하면 원본이 바뀌지 않는다. 반드시 수정 후 배열에 다시 할당하는 패턴을 사용해야 한다 (`_backpackSlots[i] = modifiedSlot`).

---

## Open Questions

- [OPEN] **Economy → Player asmdef 의존 vs 이벤트 디커플링**: ShopSystem이 InventoryManager를 직접 참조할 것인지, 이벤트/콜백으로 간접 연동할 것인지 최종 결정 필요 (-> see 섹션 7.3).

- [OPEN] **toolbarSelectedIndex 세이브 필드**: data-pipeline.md 섹션 3.2 InventorySaveData JSON 스키마에 필드 추가 완료(리뷰어 수정). 섹션 6.1의 [OPEN] 태그는 해소됨.

- [결정] **툴바 슬롯 수**: 8칸 범용 툴바 채택. data-pipeline.md `toolbarSlots`를 8개 범용 슬롯으로 수정 완료 (FIX-001 해소).

- [결정] **창고 슬롯 세이브 구조**: BuildingSaveData 하위 `storageSlots[]` 필드에 각 창고 건물이 독립적으로 저장. data-pipeline.md BuildingSaveData에 `storageSlots` 필드 및 `ItemSlotSaveData` 클래스 추가 완료 (FIX-003 해소).

- [OPEN] **아이템 버리기(Drop) 기능**: 인벤토리에서 아이템을 월드에 버리는 기능의 구현 범위. MVP에서 제외할지 여부.

- [OPEN] **품질별 슬롯 분리 vs 통합**: 같은 작물이라도 품질(Normal/Silver/Gold)이 다르면 별도 슬롯에 저장된다. 이 방식이 슬롯을 과다 소비하는지 밸런스 검증 필요.

---

## Cross-references

- `docs/design.md` -- 작물/시설 canonical 데이터 (섹션 4.2, 4.6)
- `docs/architecture.md` -- 프로젝트 구조, Scripts/Player/, Scripts/UI/ (섹션 3)
- `docs/systems/project-structure.md` -- 네임스페이스, asmdef 정의
- `docs/systems/farming-architecture.md` -- CropData, FertilizerData, ToolData SO 정의 (섹션 4.1~4.3)
- `docs/systems/economy-architecture.md` -- ShopSystem, EconomyManager 연동 (섹션 5), HarvestOrigin enum 정의 (섹션 3.10) [FIX-034]
- `docs/systems/economy-system.md` -- 경제 시스템 디자인, 가공 분류 (DES-004)
- `docs/systems/progression-architecture.md` -- 해금 시스템, UnlockRegistry (BAL-002)
- `docs/pipeline/data-pipeline.md` -- IInventoryItem 인터페이스, InventorySaveData JSON 스키마, GameDataSO 베이스 (ARC-004)
- `docs/systems/crop-growth.md` -- CropQuality enum, 품질 시스템 (DES-002)
- `docs/systems/fishing-architecture.md` -- FishData SO 정의 (섹션 3), IInventoryItem 구현 (FIX-063)

---

*이 문서는 Claude Code가 기존 아키텍처 문서 및 데이터 파이프라인 설계와의 일관성을 고려하여 자율적으로 작성했습니다.*
