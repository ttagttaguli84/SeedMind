# Devlog #018 — 가공/요리 시스템 (CON-005)

> 2026-04-06 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### CON-005: 가공/요리 시스템 콘텐츠 설계

Designer + Architect 병렬 실행 → Reviewer CRITICAL 3건·WARNING 5건 발견 후 전부 수정.

**신규 문서**:
1. `docs/content/processing-system.md` — 가공/요리 시스템 콘텐츠 canonical 문서 (32종 레시피)
2. `docs/systems/processing-architecture.md` — 가공 시스템 기술 아키텍처 (Part I + Part II 요약)

**수정된 문서**:
- `docs/pipeline/data-pipeline.md` — ProcessingSaveData 4필드 추가, ProcessingRecipeData 필드명 통일+5필드 추가, ProcessingType enum 3종 추가
- `docs/systems/processing-architecture.md` — 섹션 4 연료 시스템 교정, SO 에셋 목록 canonical 참조 전환
- `docs/architecture.md` — CON-005 cross-reference 추가
- `docs/design.md` — 섹션 4.6에 특화 가공소 3종(제분소·발효실·베이커리) 추가
- `docs/content/npcs.md` — 하나 상점 판매 목록에 장작(item_firewood, 30G) 추가

---

## 핵심 설계 내용

### 가공소 4종 체계

기존 단일 가공소(`building_processing`)에 특화 가공소 3종 신규 추가:

| 가공소 | 해금 레벨 | 건설 비용 | 슬롯 수 | 주요 역할 |
|--------|-----------|-----------|---------|-----------|
| 가공소 (일반) | Lv.7 | 3,000G | 1~3 (확장) | 잼/주스/절임/건과일 |
| 제분소 | Lv.5 | 1,500G | 1 (고정) | 곡물 → 가루 (중간재) |
| 발효실 | Lv.8 | 4,000G | 2 (고정) | 장기 발효 (와인, 식초, 된장) |
| 베이커리 | Lv.9 | 5,000G | 2 (고정) | 가공 체인 최종 산물 (빵, 케이크) |

**치즈 공방 제외 사유**: 목축 시스템 미구현으로 유제품 원재료 확보 불가 → [OPEN] CON-006으로 등록

### 레시피 체계 (32종)

| 가공소 | 레시피 수 |
|--------|-----------|
| 가공소 (일반) | 18종 |
| 제분소 | 4종 |
| 발효실 | 5종 |
| 베이커리 | 5종 |

**가공 체인 구조**: 제분소(밀가루) → 베이커리(빵/케이크) → 최종 판매  
**최고가 레시피**: 로열 타르트(2,100G) — 제분소+가공소+베이커리 3단계 체인의 최종 산물

### 연료 시스템
- **베이커리만 연료(장작) 소모**: 빵 1~2개, 케이크 2~3개
- 장작은 잡화 상점(하나)에서 30G/개 구매 (npcs.md 추가 완료)
- 다른 가공소: 연료 불필요

### 기술 아키텍처 핵심

**책임 분리**:
- `BuildingManager`: 가공소 건설·배치
- `ProcessingSystem`: 가공 로직 (BuildingManager의 서브시스템, Plain C# 클래스)

**ProcessingRecipeData SO 스키마** (PATTERN-005 준수):
- JSON 15필드 ↔ C# 15필드 완전 동기화
- 주요 필드: `recipeId`, `facilityType`, `inputItemId`, `inputQuantity`, `outputItemId`, `outputQuantity`, `processingTimeHours`, `fuelCost`

**ISaveable 복원 순서**: BuildingManager가 ProcessingSystem 대행 (SaveLoadOrder=60)

---

## 리뷰 결과

**CRITICAL 3건 (수정 완료)**:

1. [C-1] data-pipeline.md ProcessingSaveData에 4개 필드 누락(processorBuildingId, slotState, outputItemId, outputQuantity) → 추가
2. [C-2] data-pipeline.md ProcessingRecipeData 필드명 불일치(recipeId/recipeName vs dataId/displayName) + 5개 필드 누락 → 통일 및 추가
3. [C-3] 연료 시스템 설계 불일치 (processing-architecture.md "예약 필드" vs processing-system.md "베이커리 전용 연료") → architecture 교정

**WARNING 5건 (수정 완료)**:

1. [W-1] processing-architecture.md SO 에셋 목록 14종 미갱신 → canonical 참조 전환 (PATTERN-006)
2. [W-2] architecture.md CON-005 cross-reference 누락 → 추가
3. [W-3] npcs.md 하나 상점 장작 판매 미등록 → 추가
4. [W-4] design.md 섹션 4.6 특화 가공소 3종 미반영 → 추가
5. [W-5] data-pipeline.md ProcessingType enum 3종 미확장 → Mill/Fermentation/Bake 추가

---

## 의사결정 기록

1. **가공소 4종 분리**: 단일 가공소보다 특화 가공소 체계가 레벨 진행과 연계하여 더 자연스러운 해금 경험을 제공. 제분소(Lv.5)가 베이커리(Lv.9)의 재료를 공급하는 구조로 중간 단계 목표 강화.

2. **치즈 공방 [OPEN] 처리**: 목축 시스템이 설계되지 않은 상태에서 유제품 레시피를 정의하면 orphan 레시피가 생성됨. CON-006으로 후속 과제 등록하고 현재 단계에서는 제외.

3. **32종 레시피 (기존 "18종" 언급 초과)**: 초기 스펙의 "18종"은 가공소 일반만의 추정치. 특화 가공소 3종 추가로 14종 확대. BAL-004가 수정된 수치로 분석 예정.

4. **ProcessingSystem을 독립 Manager 아닌 BuildingManager 서브시스템으로**: 가공소는 본질적으로 시설(Building)의 한 종류. 별도 ISaveable로 등록하면 복원 순서 관리가 복잡해짐. BuildingManager가 ProcessingSystem 생명주기를 통제하는 것이 적합.

---

## 미결 사항 ([OPEN])

- 치즈 공방 활성화를 위한 목축/낙농 시스템 설계 (CON-006)
- 가공 중 시설 업그레이드 처리 방식 (중단 vs 계속)
- 오프라인 시간 경과 시 가공 진행 여부
- MCP SO 에셋 생성 시 enum 필드 설정 지원 여부
- 가공품 품질 시스템 도입 여부 (재료 품질 → 결과물 품질 승계)

---

## 후속 작업

- `BAL-004`: 가공품 ROI/밸런스 분석 (32종 레시피 → `docs/balance/processing-economy.md`)
- `ARC-013`: 가공 시스템 MCP 태스크 시퀀스 (`docs/mcp/processing-tasks.md`)
- `FIX-005`: `facilities.md`에 특화 가공소 3종 건설 요건 추가
- `CON-006`: 목축/낙농 시스템 콘텐츠 상세

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
