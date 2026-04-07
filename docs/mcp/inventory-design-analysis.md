# 인벤토리 MCP 태스크 시퀀스(ARC-013) 설계 분석

> 작성: Claude Code (Opus) | 2026-04-07  
> 역할: Game Designer  
> 대상 문서: `docs/systems/inventory-architecture.md` Part II (섹션 9)  
> 소비자: Architect 에이전트 (inventory-tasks.md 작성 시 활용)

---

## Context

이 문서는 `docs/systems/inventory-architecture.md`의 Part II MCP 구현 계획(Phase A~D)을 게임 설계 관점에서 분석하고, 독립 MCP 태스크 문서(`docs/mcp/inventory-tasks.md`)로 분리할 때 보완해야 할 사항을 정리한다.

---

## 1. MCP 태스크의 게임 설계 반영도 검증

### 1.1 아이템 분류 체계 반영 상태

`inventory-system.md` 섹션 1.1에 정의된 6개 카테고리와 MCP 태스크의 SO/enum 반영을 대조한다.

| 카테고리 | 영문 키 | ItemType enum (섹션 3.2) | SO 타입 | MCP Phase C 반영 | 상태 |
|----------|---------|--------------------------|---------|-----------------|------|
| 씨앗 | `Seed` | `Seed` | CropData (파생 ID) | C-1: "seed_" + dataId 방식 | OK |
| 수확물 | `Crop` | `Crop` | CropData | C-1: 기존 에셋 8종 활용 | OK |
| 도구 | `Tool` | `Tool` | ToolData | C-2: 기존 에셋 17~22종 | OK |
| 비료/소모품 | `Consumable` | `Fertilizer` + `Consumable` | FertilizerData | C-3: 기존 에셋 4종 | **주의** |
| 가공품 | `Processed` | `Processed` | ProcessingRecipeData | 명시적 언급 없음 | **누락** |
| 건설 재료 | `Material` | `Material` | 미정 | 언급 없음 | Phase 2 예정, OK |
| 물고기 | `Fish` | `Fish` | FishData | (→ see fishing-system.md, FIX-053) | OK |

**발견 사항**:

1. **[GAP-01] 가공품 SO 에셋 참조 누락**: Phase C에서 ProcessingRecipeData 에셋의 IInventoryItem 구현 확인 태스크가 없다. 아키텍처 섹션 4.4에서 ProcessingRecipeData의 IInventoryItem 구현을 정의하고 있으므로, Phase C에 "C-5: ProcessingRecipeData 에셋 IInventoryItem 확인" 태스크를 추가해야 한다. 가공품 에셋 수는 processing-system.md의 레시피 목록에 의존한다.

2. **[GAP-02] Consumable vs Fertilizer 구분 불명확**: 게임 설계(inventory-system.md 섹션 1.1)에서 "비료/소모품"을 하나의 카테고리(`Consumable`)로 묶었으나, 아키텍처 ItemType enum은 `Fertilizer`와 `Consumable`을 분리했다. 이는 의도된 분리(비료와 음식의 기능이 다르므로)이지만, 게임 설계 문서의 카테고리 정의와 아키텍처의 enum이 1:1이 아닌 점을 MCP 태스크 문서에 명시적으로 기록해야 한다.

3. **[GAP-03] Special 아이템 타입**: ItemType enum에 `Special`이 존재하나, inventory-system.md에 대응하는 카테고리가 없다. 이벤트 보상 등 향후 확장용이므로 Phase 1 MCP 태스크에서는 제외하되, enum 정의에는 포함한다.

### 1.2 툴바 슬롯 8개 반영 상태

| 설계 요건 (inventory-system.md 섹션 2.2) | MCP 태스크 반영 | 상태 |
|-------------------------------------------|----------------|------|
| toolbarSlotCount = 8 고정 | Phase D-1: 8칸 슬롯 UI 생성 명시 | OK |
| 숫자키 1~8 선택 | Phase B-1: CycleToolbar, SelectToolbarSlot | OK |
| 기본 배치 (호미/물뿌리개/낫/도끼 + 빈 슬롯 4개) | **명시적 태스크 없음** | **누락** |
| 모든 카테고리 배치 가능 | MoveSlot에서 도구 제약만 검증 | OK |

**발견 사항**:

4. **[GAP-04] 툴바 초기 배치 태스크 누락**: 게임 시작 시 슬롯 1~4에 기본 도구를 자동 배치하는 로직이 MCP 태스크에 없다. 이는 InventoryManager.LoadSaveData()의 "새 게임" 분기 또는 별도 InitializeNewGame() 메서드로 구현해야 한다. Phase A-3 또는 Phase B에 "새 게임 초기화: 기본 도구 4종 툴바 배치" 태스크를 추가할 것을 권장한다.

### 1.3 아이템 스택 규칙 반영 상태

| 스택 규칙 (inventory-system.md 섹션 1.1) | 아키텍처 반영 (섹션 4.1~4.4) | MCP 태스크 반영 | 상태 |
|-------------------------------------------|-------------------------------|----------------|------|
| 도구: 스택 불가 (maxStack=1) | ToolData.MaxStackSize = 1 | A-5에서 구현 | OK |
| 씨앗/수확물: maxStack=99 | CropData.MaxStackSize = 99 | A-5에서 구현 | OK |
| 비료/소모품: maxStack=30 | FertilizerData.MaxStackSize = 30 | A-5에서 구현 | OK |
| 가공품: maxStack=30 | ProcessingRecipeData.MaxStackSize = 30 | A-5에서 구현 | OK |
| 품질별 별도 슬롯 | ItemSlot.quality 필드로 분리 | A-2 struct에 포함 | OK |

스택 규칙은 전반적으로 잘 반영되어 있다.

---

## 2. MCP 태스크 시퀀스 확장 제안

### 2.1 SO 에셋 수량 산정

MCP 태스크에서 생성하거나 확인해야 할 SO 에셋의 전체 수량을 산정한다.

#### 신규 생성 불필요 (기존 SO 재활용)

| SO 타입 | 에셋 수 | 출처 | 비고 |
|---------|---------|------|------|
| CropData | 8종 | farming-tasks.md / crop-content-tasks.md | 씨앗 + 수확물 역할 겸용 (16 아이템 ID) |
| ToolData | 17~22종 | tool-upgrade-tasks.md | 4종 기본 도구 x 3등급 + 알파 |
| FertilizerData | 4종 | farming-tasks.md | fert_basic ~ fert_organic |
| ProcessingRecipeData | (→ see processing-system.md) | processing-tasks.md | 가공 레시피 수에 의존 |

**총 기존 에셋 활용: 약 33~38종** (인벤토리 시스템이 IInventoryItem 인터페이스 구현만 추가)

#### 신규 생성 필요

| 에셋 | 타입 | 수량 | 설명 |
|------|------|------|------|
| 없음 (현재 설계) | - | 0 | IInventoryItem 인터페이스 방식 B 채택으로 별도 ItemData SO 불필요 |

**핵심 판단**: 방식 B(IInventoryItem 인터페이스) 채택 덕분에 신규 SO 에셋 생성이 0이다. 대신 기존 SO 클래스에 인터페이스 구현을 추가하는 코드 수정이 핵심 태스크가 된다.

### 2.2 추가 권장 태스크

기존 Phase A~D에 추가해야 할 태스크를 제안한다.

#### Phase A 보강: 초기화 및 테스트 인프라

| 태스크 ID | 설명 | 근거 |
|-----------|------|------|
| A-6 | 새 게임 시 인벤토리 초기화 로직 (기본 도구 4종 툴바 배치, 초기 씨앗 지급) | GAP-04. inventory-system.md 섹션 2.2 기본 배치 + design.md의 "감자 씨앗 5개 지급" |
| A-7 | DataRegistry.GetInventoryItem() 확장 -- "seed_" prefix 처리 로직 | 아키텍처 섹션 Phase C-4에 해당하나, DataRegistry 수정은 Phase A 코어에 포함이 적절 |

#### Phase C 보강: 가공품 연동

| 태스크 ID | 설명 | 근거 |
|-----------|------|------|
| C-5 | ProcessingRecipeData에 IInventoryItem 구현 확인 및 누락 시 추가 | GAP-01 |
| C-6 | 가공품 아이템 ID 패턴("proc_{crop}_{type}") DataRegistry 등록 검증 | inventory-system.md 섹션 5.5의 ID 패턴 |

#### Phase E 신규: 창고 시스템

현재 Phase A~D에 **창고(Storage)** 관련 태스크가 완전히 누락되어 있다. inventory-system.md 섹션 2.3에 정의된 창고 시스템은 별도 Phase로 분리한다.

| 태스크 ID | 설명 | 근거 |
|-----------|------|------|
| E-1 | StorageManager 클래스 작성 (또는 InventoryManager에 Storage 메서드 추가) | 창고별 ItemSlot[] 배열 관리 |
| E-2 | StorageUI 클래스 작성 (배낭+창고 나란히 표시) | inventory-system.md 섹션 4.5 UI 레이아웃 |
| E-3 | BuildingManager.OnBuildingConstructed 구독 연결 | 아키텍처 섹션 8.1 HandleStorage 흐름 |
| E-4 | 창고 GameObject 상호작용 (E키 트리거 -> StorageUI 오픈) | inventory-system.md 섹션 2.3 접근 방식 |
| E-5 | 창고 세이브/로드 (BuildingSaveData.storageSlots 연동) | 아키텍처 섹션 6.1, data-pipeline.md |

#### Phase F 신규: 출하함 시스템

inventory-system.md 섹션 2.4의 출하함(Shipping Bin)도 별도 Phase로 분리한다.

| 태스크 ID | 설명 | 근거 |
|-----------|------|------|
| F-1 | ShippingBin 클래스 작성 (당일 아이템 보관, 익일 06:00 정산) | inventory-system.md 섹션 2.4 |
| F-2 | ShippingBinUI 작성 (배낭에서 드래그 -> 출하함) | inventory-system.md 섹션 2.4 동작 1~5 |
| F-3 | TimeManager 06:00 이벤트 구독 -> 자동 판매 트리거 | economy-system.md 수급 변동 적용 |
| F-4 | 출하함 GameObject 씬 배치 (농장 기본 제공) | scene-setup-tasks.md에 추가 또는 여기서 처리 |
| F-5 | 판매 결과 요약 UI (다음 날 아침 화면) | 새로운 UI 프리팹 필요 |

### 2.3 UI 프리팹 전체 목록

MCP 태스크에서 생성해야 할 UI 프리팹을 종합한다.

| # | 프리팹명 | 경로 | 구성 요소 | MCP Phase |
|---|---------|------|-----------|-----------|
| P-01 | `SlotUI.prefab` | `Prefabs/UI/` | Icon(Image), QuantityText(TMP), QualityBorder(Image), SelectedHighlight(Image), SlotUI.cs | D-3 |
| P-02 | `ToolbarPanel` | 씬 내 Canvas_HUD 자식 | HorizontalLayoutGroup, SlotUI x 8 | D-1 |
| P-03 | `InventoryPanel` | 씬 내 Canvas_HUD 자식 | GridLayoutGroup, SlotUI x 15~30, 정렬 버튼, 필터 탭 | D-2 |
| P-04 | `TooltipPanel` | 씬 내 Canvas_HUD 자식 | 이름(TMP Bold), 카테고리(TMP), 설명(TMP), 속성(TMP), 가격(TMP) | D-6 |
| P-05 | `StoragePanel` | 씬 내 Canvas_HUD 자식 | 배낭 그리드 + 창고 그리드 나란히, 창고 탭(#1/#2/#3), 정렬 버튼 x 2 | E-2 |
| P-06 | `ShippingBinPanel` | 씬 내 Canvas_HUD 자식 | 배낭 그리드 + 출하함 리스트, 회수 버튼 | F-2 |
| P-07 | `SellSummaryPanel` | 씬 내 Canvas_HUD 자식 | 판매 항목 리스트(TMP), 총 수입(TMP), 확인 버튼 | F-5 |

**기존 Phase D에 포함된 것**: P-01 ~ P-04 (4종)  
**추가 필요**: P-05 ~ P-07 (3종)

### 2.4 InventoryPanel 세부 구성

inventory-system.md 섹션 4.1~4.4의 UI 규칙을 MCP 태스크에 반영할 세부 사항:

| UI 요소 | 설계 요건 | MCP 구현 태스크 |
|---------|-----------|----------------|
| 배낭 그리드 | 5열 x N행 (inventory-system.md 섹션 2.1) | D-2에서 GridLayoutGroup.constraintCount = 5 |
| 정렬 버튼 | 하단 좌측 (섹션 4.4) | D-2에 Button + 정렬 로직 바인딩 |
| 카테고리 필터 탭 | 전체/씨앗/수확물/도구/소모품/가공품 (섹션 4.4) | D-2에 6개 탭 버튼 추가 |
| 품질 별 표시 | 좌측 하단, Silver=1개/Gold=2개/Iridium=3개 (섹션 4.2) | SlotUI.cs에 품질별 스프라이트 배열 |
| 도구 등급 표시 | 좌측 상단 (섹션 4.2) | SlotUI.cs에 등급 아이콘 |

[OPEN] Phase D-2의 GridLayoutGroup 열 수가 아키텍처(6~8열)와 게임 설계(5열)에서 불일치한다. 게임 설계의 5열을 canonical로 채택하고 아키텍처를 수정할 것을 권장한다.

---

## 3. 타 시스템 연동 포인트

### 3.1 연동 매트릭스

| 연동 시스템 | 연동 방향 | 연동 내용 | 관련 MCP 태스크 | 전제 조건 |
|------------|-----------|-----------|----------------|-----------|
| **FarmingSystem** | Farming -> Inventory | 수확 시 AddItem(cropId, yield, quality) | Phase B 통합 | farming-tasks.md 완료 |
| **FarmingSystem** | Inventory -> Farming | 씨앗 심기 시 RemoveItem("seed_X", 1) | Phase B 통합 | farming-tasks.md 완료 |
| **FarmingSystem** | Inventory -> Farming | 비료 사용 시 RemoveItem(fertilizerId, 1) | Phase B 통합 | farming-tasks.md 완료 |
| **EconomySystem** | Economy -> Inventory | 상점 구매 시 AddItem() | 별도 연동 태스크 | economy-architecture.md 기반 ShopSystem 완료 |
| **EconomySystem** | Inventory -> Economy | 상점 판매 시 RemoveItem() + GetSellPrice() | 별도 연동 태스크 | economy-architecture.md 완료 |
| **ProcessingSystem** | Processing -> Inventory | 가공 완료 시 AddItem(procId, 1) | Phase C-5, E 이후 | processing-tasks.md 완료 |
| **ProcessingSystem** | Inventory -> Processing | 가공 시작 시 RemoveItem(materialId, qty) | Phase C-5, E 이후 | processing-tasks.md 완료 |
| **SaveSystem** | 양방향 | GetSaveData() / LoadSaveData() | Phase A-3 내 포함 | data-pipeline.md 세이브 인프라 |
| **ProgressionSystem** | Progression -> Inventory | 배낭 업그레이드 해금 조건 확인 | 별도 연동 태스크 | progression-tasks.md 완료 |
| **BuildingSystem** | Building -> Inventory | 창고 건설 시 Storage 슬롯 추가 | Phase E-3 | facilities-tasks.md 완료 |
| **ToolUpgradeSystem** | ToolUpgrade -> Inventory | 도구 업그레이드 시 SetToolbarItem() | 별도 연동 태스크 | tool-upgrade-tasks.md 완료 |
| **NPC/ShopSystem** | NPC -> Inventory | NPC 상점 거래 시 AddItem/RemoveItem | 별도 연동 태스크 | npc-shop-tasks.md 완료 |
| **TimeManager** | Time -> Inventory(ShippingBin) | 06:00 이벤트 -> 출하함 자동 판매 | Phase F-3 | time-season 시스템 완료 |

### 3.2 연동 태스크 의존성 그래프

```
farming-tasks.md (ARC-003) ──┐
                              ├──> inventory-tasks.md Phase A~D (코어 + UI)
data-pipeline.md (ARC-004) ──┘
                                        │
                                        v
                              ┌── Phase E (창고) ←── facilities-tasks.md (ARC-007)
                              │
                              ├── Phase F (출하함) ←── economy + time-season 시스템
                              │
                              └── Phase G (타 시스템 연동)
                                    ├── FarmingSystem 연동 (수확/심기/비료)
                                    ├── EconomySystem 연동 (구매/판매)
                                    ├── ProcessingSystem 연동 (가공 투입/회수)
                                    ├── ToolUpgradeSystem 연동 (도구 교체)
                                    └── ProgressionSystem 연동 (배낭 업그레이드 해금)
```

### 3.3 FarmingSystem 연동 상세

가장 빈번한 연동으로, 핵심 게임 루프의 일부이다.

**씨앗 심기 흐름**:
```
PlayerInventory.UseCurrentItem(FarmTile target)
    │
    ├── currentItem.ItemType == Seed 확인
    ├── target.State == Tilled 확인
    ├── 계절 호환성 확인 (→ see time-season.md)
    │
    ├── InventoryManager.RemoveItem("seed_" + cropId, 1, Normal)
    │   └── 실패 시 → "씨앗이 부족합니다" 메시지
    │
    └── FarmTile.Plant(cropId)
        └── 성공 → 타일 상태 Tilled -> Planted
```

**수확 흐름**:
```
FarmTile.Harvest() 또는 PlayerInventory.UseCurrentItem(Sickle, FarmTile)
    │
    ├── GrowthSystem에서 yield, quality 계산
    │
    ├── InventoryManager.AddItem(cropId, yield, quality)
    │   ├── success → 수확 완료, 타일 상태 초기화
    │   └── remaining > 0 → "배낭이 가득 찼습니다", 수확 취소
    │
    └── ProgressionManager.AddExperience(cropXP)
```

### 3.4 EconomySystem 연동 상세

**구매 흐름** (아키텍처 섹션 7.1과 일치 확인):
- ShopSystem이 InventoryManager.AddItem() 호출
- AddResult.remaining > 0 시 부분 환불 처리 필요
- 배낭 업그레이드 구매 시: InventoryManager.ExpandBackpack() 호출 (아이템 추가가 아님)

**판매 흐름** (아키텍처 섹션 7.2와 일치 확인):
- 상점 직접 판매: 수급 변동 미적용 (inventory-system.md 섹션 6.2 방법 A)
- 출하함 판매: 수급 변동 적용 (inventory-system.md 섹션 6.2 방법 B)
- 이 두 경로의 차이가 MCP 태스크에서 명확히 분리되어야 한다

### 3.5 SaveSystem 연동 상세

**확인 사항**:
- InventorySaveData가 data-pipeline.md 섹션 3.2에 canonical 정의됨
- toolbarSelectedIndex 필드 추가 완료 (아키텍처 [결정] 태그)
- 창고 슬롯은 BuildingSaveData.storageSlots에 저장 (아키텍처 [결정] 태그)

**[GAP-05] 출하함 세이브 데이터 누락**: inventory-system.md 섹션 8에서 "출하함 목록: 당일 넣은 아이템 리스트"를 세이브 대상으로 명시하고 있으나, data-pipeline.md의 InventorySaveData나 별도 세이브 구조에 출하함 필드가 정의되어 있지 않다. MCP 태스크 작성 전에 data-pipeline.md에 ShippingBinSaveData 구조를 추가해야 한다.

---

## 4. 최종 MCP 태스크 Phase 요약 (확장 제안 포함)

| Phase | 설명 | 스크립트 | SO 에셋 | UI 프리팹 | 예상 MCP 호출 |
|-------|------|---------|---------|-----------|-------------|
| A | InventoryManager 코어 + 초기화 | 4~5종 | 0 | 0 | ~25회 |
| B | PlayerInventory 연결 + 콘솔 테스트 | 1종 | 0 | 0 | ~10회 |
| C | 기존 SO에 IInventoryItem 구현 추가 + DataRegistry 확장 | 0 (기존 수정) | 0 | 0 | ~15회 |
| D | InventoryUI + ToolbarUI + 드래그 앤 드롭 + 툴팁 | 3종 | 0 | 4종 | ~45회 |
| E (신규) | 창고 시스템 | 1~2종 | 0 | 1종 | ~20회 |
| F (신규) | 출하함 시스템 | 2종 | 0 | 2종 | ~25회 |
| G (신규) | 타 시스템 연동 + 통합 테스트 | 0 (기존 수정) | 0 | 0 | ~20회 |
| **합계** | | **~12종** | **0** | **7종** | **~160회** |

[RISK] 총 ~160회 MCP 호출은 npc-shop-tasks.md(~166회)와 유사한 규모이다. Phase D(UI 생성)가 가장 호출 수가 많으며, 특히 SlotUI 프리팹의 자식 오브젝트 생성과 컴포넌트 설정이 반복적이다. 에디터 스크립트를 통한 일괄 생성(CreateInventoryUIAssets.cs)으로 D의 ~45회를 ~15회로 줄일 수 있다.

---

## 5. GAP 요약 및 권장 조치

| GAP ID | 내용 | 심각도 | 권장 조치 |
|--------|------|--------|-----------|
| GAP-01 | Phase C에 ProcessingRecipeData IInventoryItem 확인 태스크 누락 | 중 | Phase C-5 태스크 추가 |
| GAP-02 | Consumable vs Fertilizer enum 분리가 설계 카테고리와 불일치 | 낮 | 문서에 매핑 관계 명시, 설계 변경 불필요 |
| GAP-03 | Special ItemType의 설계 대응 카테고리 없음 | 낮 | Phase 1에서 무시, enum만 유지 |
| GAP-04 | 툴바 초기 배치(기본 도구 4종) 태스크 누락 | 높 | Phase A에 A-6 태스크 추가 |
| GAP-05 | 출하함 세이브 데이터(ShippingBinSaveData) 미정의 | 중 | data-pipeline.md에 추가 후 Phase F 작성 |

---

## 6. 아키텍처 문서 수정 권장 사항

MCP 태스크 문서 작성 전에 `inventory-architecture.md`에서 수정이 권장되는 항목:

1. **Phase D-2 GridLayoutGroup 열 수**: "6~8열" -> "5열" (inventory-system.md 섹션 2.1의 "5열 x N행" 기준)
2. **Phase C에 C-5, C-6 태스크 추가**: ProcessingRecipeData + 가공품 ID 패턴
3. **Phase A에 A-6 태스크 추가**: 새 게임 초기화 로직
4. **Phase E, F, G 개요 추가**: 창고/출하함/연동 Phase

---

## Cross-references

| 관련 문서 | 참조 내용 |
|-----------|-----------|
| `docs/systems/inventory-system.md` | 인벤토리 게임 설계 canonical (DES-005) |
| `docs/systems/inventory-architecture.md` | 인벤토리 기술 아키텍처 + MCP 개요 |
| `docs/pipeline/data-pipeline.md` 섹션 3.2 | InventorySaveData JSON 스키마 |
| `docs/mcp/npc-shop-tasks.md` | MCP 태스크 문서 형식 참조 (ARC-009) |
| `docs/mcp/farming-tasks.md` | FarmingSystem 연동 전제 조건 (ARC-003) |
| `docs/mcp/tool-upgrade-tasks.md` | ToolUpgradeSystem 연동 전제 조건 (ARC-015) |
| `docs/mcp/facilities-tasks.md` | 창고 건설 연동 전제 조건 (ARC-007) |
| `docs/mcp/processing-tasks.md` | 가공 시스템 연동 전제 조건 |
| `docs/systems/economy-system.md` | 가격 구조, 수급 변동, 가공품 분류 |
| `docs/systems/time-season.md` | 06:00 정산 이벤트, 계절별 씨앗 필터링 |

---

*이 문서는 Game Designer 에이전트가 inventory-architecture.md Part II의 MCP 태스크를 게임 설계 관점에서 분석한 결과물이다. Architect 에이전트는 이 분석을 바탕으로 docs/mcp/inventory-tasks.md를 작성한다.*
