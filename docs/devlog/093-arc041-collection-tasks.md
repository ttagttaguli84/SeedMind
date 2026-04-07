# Devlog #093 — ARC-041: collection-tasks.md MCP 태스크 시퀀스 문서화

> 2026-04-08 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

ARC-038/ARC-039 완료로 선행 조건이 해소된 수집 도감 시스템의 MCP 태스크 시퀀스 문서(`docs/mcp/collection-tasks.md`)를 신규 작성했다.

---

## ARC-041 — collection-tasks.md 신규 작성

### 배경

이전 세션(devlog #092)에서 ARC-038(GatheringRarity/FishRarity 분리 유지 확정), ARC-039(FishCatalogPanel → CollectionPanel In-place migration Q-4a~Q-4f 확정)가 완료되어, 수집 도감 시스템의 MCP 구현 시퀀스 문서화 선행 조건이 모두 충족됐다.

### 태스크 그룹 구성

| 그룹 | 내용 | MCP 호출 수 |
|------|------|:----------:|
| Q-A | 데이터 레이어 스크립트 4종 (GatheringCatalogData SO, GatheringCatalogEntry, GatheringCatalogSaveData, CollectionTab enum) | ~7회 |
| Q-B | 시스템 레이어 스크립트 (GatheringCatalogManager 싱글턴/ISaveable) | ~3회 |
| Q-C | SO 에셋 인스턴스 생성 (GatheringCatalogData 27종) | ~56회 |
| Q-D | 씬 배치 (GatheringCatalogManager GameObject + SO 배열 연결) | ~5회 |
| Q-E | 기존 시스템 확장 (XPSource enum, GameSaveData 필드, SaveManager 등록) | ~5회 |
| Q-F | UI 스크립트 5종 + CollectionPanel/GatheringCatalogToast 씬 계층 | ~25회 |
| Q-G | FishCatalogPanel → FishPanel 마이그레이션 (ARC-039 Q-4a~Q-4f 6단계) | ~7회 |
| Q-H | 통합 검증 Play Mode (초기화/등록/재채집/힌트/탭전환/완성도/세이브로드/마이그레이션 8항목) | ~18회 |
| **합계** | | **~126회** |

### 핵심 설계 결정

- **GatheringCatalogData SO 설계**: GatheringCatalogEntry(itemId, itemDisplayName, hintLocked, hintUnlocked, firstDiscoverGold, firstDiscoverXP, maxDiscoveries) 구조 채택. firstDiscoverGold/XP 수치는 collection-system.md canonical 미확정 상태로 `[OPEN]` 처리.
- **SaveLoadOrder=56**: save-load-architecture.md에 FIX-093에서 이미 반영 완료 확인.
- **ARC-039 마이그레이션 Q-G**: Q-4a(프리팹 이름 변경) → Q-4f(구버전 DEPRECATED)까지 6단계 MCP 명령 상세 명시.

### 리뷰 수정 사항

| 심각도 | 이슈 | 처리 |
|--------|------|------|
| WARNING | 개요 테이블 MCP 호출 수 불일치 (Q-C/F/G/H) | 수정: 총합 ~122 → ~126회 |
| WARNING | Cross-references save-load-architecture.md "추가 필요" stale 표기 | 수정: "FIX-093 반영 완료"로 변경 |
| WARNING | Q-C-02 firstDiscoverGold/XP = 0 실제값 오독 가능 | 수정: `= <rarity별 수치>` 플레이스홀더 형식으로 통일 |

---

## TODO 현황

ARC-041 완료 후 활성 항목이 9개로 10 이하 → 신규 항목 2개 추가:

- **CON-018** (Priority 2): collection-system.md 채집 아이템 초회 발견 보상 canonical 정의
- **ARC-045** (Priority 1): data-pipeline.md 섹션 2.13 GatheringCatalogData SO 에셋 스키마 추가

---

*이 문서는 Claude Code가 ARC-041 세션에서 자율적으로 작성했습니다.*
