# Devlog #109 — ARC-044 + ARC-042: MCP 빌드 순서 로드맵 + SO 참조 방식 확정

> 작성: Claude Code (Sonnet 4.6) | 2026-04-08

---

## 작업 요약

**ARC-044**: `docs/mcp/build-order.md` 신규 작성 — 전체 23개 MCP 태스크 시퀀스 의존성 그래프 및 Phase 2 빌드 순서 로드맵

**ARC-042**: collection-architecture.md OPEN#5 + fishing-architecture.md OQ-10 동시 확정 — GatheringCatalogData↔GatheringItemData, FishCatalogData↔FishData SO 참조 방식 결정

---

## ARC-044: MCP 태스크 빌드 순서 로드맵

### 문서 개요

`docs/mcp/build-order.md` 신규 생성. Phase 2 Unity 구현 착수 시 MCP 태스크 실행 순서를 정의하는 로드맵 문서.

### Phase 그룹 구성

23개 MCP 태스크 파일을 의존성 기반으로 7개 Phase로 분류:

| Phase | 그룹 | 태스크 수 | 핵심 내용 |
|-------|------|-----------|----------|
| A | Foundation | 1 | scene-setup (모든 것의 선행) |
| B | Core Systems | 4 | farming, time-season, save-load, progression |
| C | Content | 3 | crop-content, facilities, inventory |
| D | Feature Systems | 7 | tool-upgrade, npc-shop, blacksmith, processing, tutorial, quest, achievement |
| E | UI & UX | 2 | ui-tasks, sound-tasks |
| F | Advanced Features | 4 | farm-expansion, livestock, fishing, gathering |
| G | Polish | 2 | collection, decoration |

### 크리티컬 패스

- **MVP 최단 경로**: scene-setup → farming → crop-content → save-load
- **전체 최장 의존성 체인**: scene-setup → farming → facilities → tool-upgrade → npc-shop → tutorial → quest → achievement → ui (8단계)

### 병렬 구현 가능 그룹

- **Phase B**: time-season / save-load / progression (farming 완료 후 병렬 진행 가능)
- **Phase F**: livestock / fishing (farm-expansion 완료 후 병렬 진행 가능)

### 리뷰 수정 사항

- `tutorial-tasks.md` Phase 분류 E→D 수정 (quest-tasks 선행 조건으로 Phase D에 있어야 함)
- "미기재" 텍스트를 `[OPEN - 미집계]` 태그로 교체 (PATTERN-010 준수)

---

## ARC-042: SO 참조 방식 확정

### 결정 사항

**itemId 문자열 연결 방식 채택** (현상 유지).

두 시스템 모두 동일 방식:
- `GatheringCatalogData.itemId` ↔ `GatheringItemData.dataId` 문자열 매칭
- `FishCatalogData.fishId` ↔ `FishData.fishId` 문자열 매칭

### 결정 근거

1. **세이브/로드 직렬화 호환성**: `GatheringCatalogSaveData.entries[].itemId`, `FishCatalogSaveData.entries[].fishId` — string key 기반 직렬화 구조가 이미 확정
2. **패턴 일관성**: 양 시스템의 `Initialize()` 메서드가 `Dictionary<string, CatalogData>` 구축 패턴 사용
3. **이벤트 인터페이스**: `OnItemGathered(item.dataId)` 등 이벤트가 string itemId 전달
4. **Inspector 편의 대안**: `Initialize()` 시 dataId 불일치 경고 로그로 대체 가능

### 수정 파일

| 파일 | 수정 내용 |
|------|----------|
| `docs/systems/collection-architecture.md` | Open Question 5 RESOLVED — 결정 상세 명시 |
| `docs/systems/fishing-architecture.md` | Open Question 10 RESOLVED — 동일 결정 상세 명시 |

---

## 완료 상태

- ARC-044 DONE — `docs/mcp/build-order.md` 신규 (7 Phase, 의존성 그래프, 크리티컬 패스)
- ARC-042 DONE — OPEN#5(collection) + OQ-10(fishing) 동시 확정

## 잔존 활성 TODO

| ID | Priority | 설명 |
|----|----------|------|
| DES-021 | 1 | 보조 XP 소스 합산 비중 상한 기준 설계 원칙 문서화 |
| PATTERN-011 | - | [self-improve 전용] MCP 태스크 예시 에셋명 canonical 불일치 패턴 |
