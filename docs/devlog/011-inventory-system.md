# Devlog #011 — 인벤토리/아이템 시스템 (DES-005 + ARC-006)

> 2026-04-06 | Phase 1 | 작성: Claude Code (Opus)

---

## 오늘 한 일

### DES-005 + ARC-006: 인벤토리/아이템 시스템

Designer + Architect 병렬 실행으로 2개 문서 신규 작성. Reviewer가 CRITICAL 5건·WARNING 7건 발견 후 전부 수정. 추가로 FIX-001~003 설계 결정 3건을 즉시 처리.

**신규 문서**:
1. `docs/systems/inventory-system.md` — 인벤토리 게임 설계 (Designer)
2. `docs/systems/inventory-architecture.md` — 인벤토리 기술 아키텍처 (Architect)

**수정된 문서**: `docs/pipeline/data-pipeline.md`, `docs/design.md`

### 핵심 설계 내용

**아이템 분류 체계** (6개 카테고리):
- Seed(씨앗), Crop(수확물), Tool(도구), Consumable(비료/소모품), Material(건설 재료), Processed(가공품)
- 스택 규칙: 씨앗/수확물 99개, 비료/가공품 30개, 도구 1개(스택 불가)

**인벤토리 구조**:
- 배낭: 15칸 시작 → 최대 30칸 (레벨 3/5/8 업그레이드, 비용 1,000G/3,000G/8,000G)
- 툴바: 8칸 범용 (씨앗, 비료도 단축 배치 가능)
- 창고: 30칸/동, 최대 3동 (각 BuildingSaveData에 슬롯 독립 저장)

**아키텍처 핵심 결정**:
1. IInventoryItem 인터페이스 방식 채택: 기존 CropData/ToolData/FertilizerData SO가 인터페이스를 구현 → 에셋 이중화 없이 통합
2. 툴바와 배낭 독립 슬롯: 기존 data-pipeline.md 세이브 스키마와 일치, 정렬 로직 단순화
3. 문자열 ID 체계(`seed_potato`, `tool_hoe_basic` 등): JSON 직렬화 안전, DataRegistry O(1) 조회

### 리뷰 결과

**CRITICAL 5건 (수정 완료)**:
1. data-pipeline.md 배낭 슬롯 수 24→15 불일치 → data-pipeline.md 수정 및 canonical 참조 추가
2. 툴바 설계 불일치 (5칸 도구 전용 vs 8칸 범용) → 8칸 범용 채택, data-pipeline.md toolbarSlots 갱신
3. ItemCategory enum 불일치 (Consumable vs Fertilizer) → inventory-architecture.md에 Consumable 추가
4. toolbarSelectedIndex 필드 data-pipeline.md 누락 → 추가
5. 창고 슬롯 세이브 구조 미정 → BuildingSaveData.storageSlots[] 채택 및 ItemSlotSaveData 클래스 정의

**WARNING 7건 (수정 완료)**:
1. SlotLocation enum에 Storage 누락 → 추가
2. 창고 건설이 배낭을 확장한다는 오류 → AddStorageSlots 로직으로 수정
3. 도구를 배낭으로 이동 불가 제약 (디자인과 불일치) → 제약 제거
4. FertilizerData/ProcessingRecipeData maxStack 50 → 30 (canonical 통일)
5. 카테고리 필터 탭 이름 불일치 → Consumable 추가로 해소
6. [OPEN] 도끼 포함 여부 design.md와 불일치 → [OPEN] 태그 유지, 추후 결정
7. [OPEN] toolbarSelectedIndex 세이브 필드 → 추가 완료

---

## 의사결정 기록

1. **8칸 범용 툴바**: Stardew Valley 스타일. 씨앗과 비료를 툴바에 올려두면 작업 흐름이 자연스럽다. 도구 전용 슬롯으로 제한하면 씨앗 선택 시 항상 인벤토리를 열어야 해 UX가 나빠짐.
2. **IInventoryItem 인터페이스**: 별도 ItemData SO를 만들면 에셋 이중화(CropData + CropItemData)가 발생한다. 기존 SO에 인터페이스만 구현시키는 방식이 data-pipeline.md 체계와 자연스럽게 통합됨.
3. **창고 슬롯 → BuildingSaveData**: 창고가 여러 동일 수 있으므로 각 건물이 자신의 슬롯을 독립 보유하는 것이 직관적이고 모듈형 설계에 맞음.
4. **maxStack 30 (Consumable canonical)**: inventory-system.md가 디자인 문서이므로 최종 수치의 canonical 출처. 아키텍처 문서의 50은 오류.

---

## 미결 사항 ([OPEN])

- **도끼 포함 여부**: design.md 5절에는 도구 5종(호미, 물뿌리개, 씨앗, 낫, 손). inventory-system.md에서 도끼를 4번 슬롯 권장으로 제안했으나 확정 미완료. CON-002(시설 콘텐츠) 작업 시 함께 결정 예정.

---

## 다음 단계

- PATTERN-005·006: self-improve 처리 (현재 세션 또는 다음 세션)
- CON-001: 작물 콘텐츠 상세 (Priority 2)
- CON-002: 시설 콘텐츠 상세 (Priority 2)

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
