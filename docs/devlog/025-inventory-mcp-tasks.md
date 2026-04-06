# Devlog #025 — 인벤토리 MCP 태스크 시퀀스 (ARC-013)

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### ARC-013: 인벤토리 MCP 태스크 시퀀스 독립 문서화

`inventory-architecture.md`(ARC-006)의 Part II MCP 구현 계획을 상세한 독립 태스크 시퀀스 문서로 확장. 총 ~118회 MCP 호출 예상.

**신규 문서**:
- `docs/mcp/inventory-tasks.md` — 인벤토리 시스템 MCP 태스크 시퀀스 (ARC-013)
- `docs/mcp/inventory-design-analysis.md` — 디자이너 에이전트 GAP 분석 (중간 산출물)

**수정된 문서**:
- `docs/systems/inventory-architecture.md` — 섹션 10.2 메서드명 수정(C-1), Phase D-2 GridLayout 열 수 수정(W-1)
- `docs/mcp/inventory-tasks.md` — 리뷰 반영: canonical 참조 주석 추가, Cross-references 보완, GAP-05 [OPEN] 추가
- `TODO.md` — ARC-013 DONE 처리, FIX-008/FIX-009 신규 추가

---

## 핵심 설계 내용

### inventory-tasks.md 구성

**총 MCP 호출 예상**: ~118회

| 태스크 | 내용 | 호출 수 |
|--------|------|--------|
| T-1 | 스크립트 8종 생성 (S-01~S-08) | ~14회 |
| T-2 | SO 에셋 — 기존 ItemData SO에 IInventoryItem 구현 추가, DataRegistry 확장 | ~18회 |
| T-3 | UI 프리팹 생성 (SlotUI, ToolbarPanel 8칸, InventoryPanel 5열, TooltipPanel) | ~46회 |
| T-4 | 씬 배치 및 참조 연결 (InventoryManager, PlayerInventory) | ~14회 |
| T-5 | 타 시스템 연동 (Farming/Economy/Building/Save) | ~8회 |
| T-6 | 통합 테스트 시퀀스 (TC-01~TC-12) | ~18회 |

**스크립트 목록 (S-01~S-08)**:
- S-01: ItemType enum
- S-02: IInventoryItem interface
- S-03: ItemData (ScriptableObject)
- S-04: InventorySlot (직렬화 클래스)
- S-05: InventorySaveData (직렬화 클래스)
- S-06: InventoryEvents (static 이벤트 허브)
- S-07: InventoryManager (MonoBehaviour Singleton)
- S-08: PlayerInventory (MonoBehaviour)

---

## 리뷰 결과

**CRITICAL 1건 (수정 완료)**:
- [C-1] inventory-architecture.md 섹션 10.2 이벤트 테이블에서 창고 건설 시 `ExpandBackpack()` 호출로 잘못 기재 → `HandleStorage() → AddStorageSlots()`로 수정

**WARNING 5건 (수정 완료 4건 / FIX 태스크 2건)**:
- [W-1] inventory-architecture.md Phase D-2 GridLayout 열 수 `6~8` → `5` (canonical 참조 추가) — 수정 완료
- [W-2] 출하함 ShippingBinSaveData 스키마 미정의(GAP-05) → [OPEN] 태그 추가, FIX-009 등록
- [W-3] Cross-references에 `inventory-design-analysis.md`, `processing-tasks.md` 누락 → 추가 완료
- [W-4] Step 2-02 MaxStackSize => 1 canonical 참조 주석 누락, Step 6-06 테스트 주석 참조 누락 → 추가 완료
- [W-5] inventory-system.md 도구 ID 표기 불일치(tool_hoe vs hoe_basic) → FIX-008 등록

---

## 의사결정 기록

1. **기존 SO 에셋 재활용**: 인벤토리 시스템에서 ItemData SO를 별도로 새로 생성하는 대신, 이미 작성된 CropData/ToolData/FertilizerData SO에 `IInventoryItem` 인터페이스를 구현하는 방식을 채택. MCP 호출 수를 대폭 절감하고 데이터 중복을 방지.

2. **UI 구조**: ToolbarPanel(8칸)과 InventoryPanel(5열 x N행)을 분리된 프리팹으로 구성. 툴바는 항상 표시, 인벤토리 패널은 Toggle로 열고 닫음.

3. **CreativeMode 초기 배치**: 게임 시작 시 툴바 슬롯 0~3에 호미/물뿌리개/낫/도끼를 자동 배치하는 로직을 T-4에 포함(GAP-04 해결).

---

## 미결 사항 ([OPEN])

- 출하함 ShippingBinSaveData 스키마 정의 (data-pipeline.md 3.2 — FIX-009)
- inventory-system.md 도구 ID 표기 통일 (tool_hoe → hoe_basic — FIX-008)
- InventoryUI 접근 단축키 (기본값 [I] 키 — 설계 문서 미확인)
- instantiate_prefab MCP 미지원 시 수동 생성 절차 상세화

---

## 후속 작업

- `FIX-008`: inventory-system.md 도구 ID 표기 통일
- `FIX-009`: data-pipeline.md ShippingBinSaveData 스키마 추가
- `ARC-012`: 세이브/로드 MCP 태스크 시퀀스
- `ARC-016`: 퀘스트 MCP 태스크 시퀀스
- `ARC-010`: 튜토리얼 MCP 태스크 시퀀스

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
