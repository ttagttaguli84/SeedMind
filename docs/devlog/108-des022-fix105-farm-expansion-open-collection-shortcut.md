# Devlog #108 — DES-022 + FIX-105: farm-expansion.md [OPEN] 항목 확정 및 수집 도감 단축키 확정

> 작성: Claude Code (Sonnet 4.6) | 2026-04-08

---

## 작업 요약

**DES-022**: `docs/systems/farm-expansion.md` 잔존 [OPEN] 항목 3건 일괄 처리 — Phase 2 착수 전 선행 필요 작업 완료

**FIX-105**: 수집 도감 단축키 바인딩 `C` 키로 확정 (collection-system.md / fish-catalog.md / ui-system.md 3개 문서 동기화)

---

## DES-022: farm-expansion.md [OPEN] 항목 3건 처리

### OPEN #1 (섹션 1.3 / Open Questions #6) — 576타일 vs 1,024타일 정의 불일치 [RESOLVED]

**결정**: 두 수치는 서로 다른 범위를 정의한다.

| 수치 | 정의 |
|------|------|
| **1,024타일(32x32)** | 전체 월드 맵 크기 (경작 Zone + 비경작 영역 전체 포함) |
| **576타일** | 경작 가능 Zone A~G의 합계 |

448타일 차이는 마을, 광산 입구, 이동 통로, 강/산 등 비경작 월드 영역으로, Phase 3~4 콘텐츠 확장 공간으로 예약.

### OPEN #2 (섹션 3.3 / Open Questions #3) — 개간 보상 사용처 [RESOLVED]

**결정**: Phase 1 MVP에서 목재/돌/섬유를 **판매 전용**으로 확정.

- 근거: `docs/content/facilities.md` 섹션 2.4의 골드 전용 건설 시스템과 일관성 유지
- Phase 3~4에서 크래프팅 시스템 도입 검토 예정

### OPEN #3 (섹션 4.5 / Open Questions #7) — 온실 Zone G 허용 여부 [RESOLVED]

**결정**: 온실 Zone G 건설 **금지**.

- 근거: Zone G Rich 토양 효과와 BAL-010 온실 보정(비주 계절 페널티 x0.8 / 겨울 시너지 x1.2)이 중첩될 경우 의도치 않은 수익 증폭 발생
- 온실 건설 불가 구역 최종 확정: Zone E(목장), Zone F(연못 타일 위), Zone G(과수원 전용)

---

## FIX-105: 수집 도감 단축키 `C` 키 확정

**결정**: 수집 도감 패널 토글 단축키로 `C` 키 채택.

**근거**:
- 기존 할당 키: E(상호작용), I/Tab(인벤토리), J(퀘스트 로그), Y(업적)
- `C` = Catalog/Collection 이니셜, 직관적이며 충돌 없음

**수정 파일**:

| 파일 | 수정 내용 |
|------|----------|
| `docs/systems/collection-system.md` | 섹션 6.1 테이블 `[OPEN]` → `C 키` 확정, Open Questions #2 RESOLVED |
| `docs/content/fish-catalog.md` | 섹션 5.1 `[OPEN]` → `C 키` 확정, Open Questions #3 RESOLVED |
| `docs/systems/ui-system.md` | 섹션 11 키 바인딩 맵에 `C \| 수집 도감 패널 토글` 행 추가 |

---

## 완료 상태

- DES-022 DONE — farm-expansion.md Phase 2 착수 전 미결 항목 전수 해소
- FIX-105 DONE — 수집 도감 단축키 `C` 키 3개 문서 일관 등록
