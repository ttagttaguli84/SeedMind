# Devlog #019 — 퀘스트/미션 시스템 (DES-009) + 시설 콘텐츠 보완 (FIX-005)

> 2026-04-06 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### FIX-005: 특화 가공소 3종 시설 상세 추가

이전 세션(CON-005)에서 design.md에 등록된 제분소·발효실·베이커리의 상세 건설 요건/업그레이드 경로가 `docs/content/facilities.md`에 누락된 상태였다. 이를 보완했다.

**수정된 문서**:
- `docs/content/facilities.md`
  - 섹션 2.1 일람표에 특화 가공소 3종 행 추가
  - 섹션 2.2 건설 시간 표에 3종 항목 추가
  - 섹션 7~9 신규 추가 (제분소/발효실/베이커리 상세)
  - 기존 섹션 7(향후 확장 후보) → 섹션 10으로 리넘버링
  - Cross-references에 processing-system.md, processing-architecture.md, npcs.md 추가

### DES-009: 퀘스트/미션 시스템 설계

Designer + Architect 병렬 실행 → Reviewer CRITICAL 4건·WARNING 2건 발견 후 전부 수정.

**신규 문서**:
1. `docs/systems/quest-system.md` — 퀘스트/미션 시스템 설계 canonical (DES-009)
2. `docs/systems/quest-architecture.md` — 퀘스트 기술 아키텍처

**수정된 문서**:
- `docs/content/facilities.md` — 레시피 직접 기재 3건 → canonical 참조로 교체 (CRITICAL-1~3)
- `docs/systems/quest-system.md` — 존재하지 않는 파일 참조 2건 수정 (CRITICAL-4)
- `docs/systems/save-load-architecture.md` — SaveLoadOrder 할당표에 QuestManager=85 추가 (WARNING-1)
- `docs/content/facilities.md` — 섹션 10 하위 번호 오기 수정 (WARNING-2)
- `TODO.md` — DES-009/FIX-005 DONE 처리, PATTERN-008 등록

---

## 핵심 설계 내용

### 퀘스트 4종 체계

| 카테고리 | 범위 | 예시 |
|----------|------|------|
| 메인 퀘스트 | 계절별 (봄/여름/가을/겨울 각 3~4개) | "봄 첫 수확: 작물 20개 수확" |
| NPC 의뢰 | 중기 (NPC 4인 각 2~3종) | "철수 의뢰: 감자 10개 납품" |
| 일일 목표 | 단기 (12종 로테이션, 매일 2개) | "오늘의 목표: 당근 5개 수확" |
| 농장 도전 | 장기 달성 (총 23종) | "경제왕: 단일 계절 5,000G 수익" |

**총 퀘스트 수**: 메인 14 + NPC 11 + 일일 12 + 도전 23 = 60종

### 보상 체계

| 보상 타입 | 범위 |
|-----------|------|
| 골드 보상 | 일일: 50~200G / 메인: 300~1,500G / 도전: 500~3,000G |
| XP 보상 | 전체 레벨업 XP의 10~15% 기여 |
| 아이템 보상 | 희귀 씨앗, 레시피 해금, 업그레이드 재료 |
| 특수 해금 | 레시피/시설/NPC 대화 언락 |

### 기술 아키텍처 핵심

**클래스 구조 (12개)**:
- `QuestManager` (MonoBehaviour, ISaveable, SaveLoadOrder=85)
- `QuestTracker` — 9개 외부 이벤트 → 12종 ObjectiveType 추적
- `QuestRewarder` — EconomyManager/ProgressionManager/InventoryManager 호출
- `DailyQuestSelector` — 매일 2개 랜덤 선택, 중복 방지
- `NPCRequestScheduler` — NPC 의뢰 등장/쿨다운 관리

**이벤트 버스 패턴**: 기존 정적 이벤트 허브에 QuestEvents 8개 추가  
**SaveLoadOrder**: QuestManager=85 (TutorialManager=80 이후, 할당표 등록 완료)

---

## 리뷰 결과

**CRITICAL 4건 (수정 완료)**:

1. [C-1] facilities.md 섹션 7.2 — 제분소 레시피 직접 기재, canonical(processing-system.md)과 불일치 → canonical 참조로 교체
2. [C-2] facilities.md 섹션 8.2 — 발효실 레시피 직접 기재, 레시피 ID/판매가 불일치 → canonical 참조로 교체
3. [C-3] facilities.md 섹션 9.2 — 베이커리 레시피 직접 기재, 재료 구성/판매가 불일치 → canonical 참조로 교체
4. [C-4] quest-system.md — `save-load-system.md` 존재하지 않는 파일 참조 2건 → `save-load-architecture.md`로 수정

**WARNING 2건 (수정 완료)**:

1. [W-1] save-load-architecture.md — SaveLoadOrder 할당표에 QuestManager=85 누락 → 추가
2. [W-2] facilities.md 섹션 10 — 하위 번호 7.x로 잘못 기재 → 10.x로 수정

**PATTERN 등록**:
- PATTERN-008: 비-canonical 문서(facilities.md)에 레시피 목록 직접 기재 시 canonical 불일치 발생 패턴 → self-improve 처리 예정

---

## 의사결정 기록

1. **퀘스트 시스템 구조 4분류**: 단일 "퀘스트"보다 메인/NPC의뢰/일일/도전의 4층 구조가 단기·중기·장기 목표를 고르게 제공. 일일 목표가 매일 "오늘 할 일"을 주어 일상적 플레이 루프를 지탱.

2. **SaveLoadOrder=85**: TutorialManager(80)보다 나중에 복원하여 튜토리얼 완료 상태를 참조한 뒤 퀘스트 해금 조건을 판단. NPCShopManager(70)보다는 나중에 복원하여 NPC 의뢰 연동이 안정적으로 작동.

3. **DailyQuestSelector 별도 분리**: QuestManager 비대화 방지. 일일 목표 선택 로직(랜덤, 중복 방지, 세이브/로드)은 독립 클래스로 캡슐화.

4. **FIX-005 레시피 direct-listing 방지**: facilities.md 신규 섹션에 레시피 테이블을 직접 넣었다가 CRITICAL 3건 발생. processing-system.md가 canonical이므로 시설 문서는 개요+슬롯+업그레이드만 기재하고 레시피는 `(→ see processing-system.md 섹션 X.X)` 참조로 통일.

---

## 미결 사항 ([OPEN])

- 퀘스트 XP가 기존 레벨업 속도에 미치는 영향 정밀 분석 (progression-curve.md 재시뮬레이션)
- 2년차 이후 메인 퀘스트 갱신 정책
- 퀘스트 전용 레시피 도입 여부
- 밀 작물 미구현으로 제분소 밀가루 레시피 보류 (DES-010 등 작물 확장 시 추가)

---

## 후속 작업

- `BAL-004`: 가공품 ROI/밸런스 분석 (processing-economy.md)
- `ARC-013`: 가공 시스템 MCP 태스크 시퀀스 (processing-tasks.md)
- `ARC-012`: 세이브/로드 MCP 태스크 시퀀스 (save-load-tasks.md)
- `PATTERN-008`: self-improve — 비-canonical 레시피 직접 기재 방지 규칙 추가

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
