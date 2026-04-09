# Self-Improve 보고서 — PATTERN-008 처리

- 날짜: 2026-04-06
- 작성: Claude Code (self-improve 에이전트)
- 트리거: TODO.md PATTERN-008 항목 (사용자 요청)

---

## 1. 발견된 패턴

### 패턴 유형
**비-canonical 문서에 레시피 목록 직접 기재**

### 발생 위치 및 건수
| 시설 | 비-canonical 문서 위치 | canonical 문서 | 불일치 유형 |
|------|----------------------|---------------|------------|
| 제분소 | `docs/content/facilities.md` 섹션 7 | `docs/content/processing-system.md` | 재료/산출물 항목 누락 |
| 발효실 | `docs/content/facilities.md` 섹션 8 | `docs/content/processing-system.md` | 처리 시간 수치 불일치 |
| 베이커리 | `docs/content/facilities.md` 섹션 9 | `docs/content/processing-system.md` | 판매가 수치 불일치 |

총 불일치 건수: **3건** (self-improve 트리거 임계값 3건 충족)

### 근본 원인
시설 문서(`facilities.md`)가 각 가공소 섹션을 작성할 때, 해당 시설의 구조적 파라미터(슬롯 수, 연료 타입 등)와 레시피 콘텐츠(재료, 산출물, 판매가, 처리 시간)를 구분하지 않고 함께 기재하는 관행이 있었다. 레시피 콘텐츠는 `processing-system.md`가 canonical 출처임에도 불구하고, 시설 문서에 독립적으로 복사·기재되어 canonical과의 불일치가 반복적으로 발생하였다.

---

## 2. 적용된 변경사항

### 2-1. `.claude/rules/doc-standards.md` — Consistency Rules 섹션

PATTERN-007 규칙 항목 바로 다음에 PATTERN-008 규칙을 추가하였다.

추가된 내용:
> **(PATTERN-008) 시설 문서의 레시피 목록 직접 기재 금지**: 시설 문서(`facilities.md` 등)의 각 시설 섹션에서 레시피 목록(재료, 산출물, 판매가, 처리 시간 등)을 직접 기재하는 것을 금지한다. 레시피 정보는 canonical 문서(`processing-system.md`)를 `(→ see docs/content/processing-system.md 섹션 X.X)` 형식으로만 참조한다. 시설 문서가 기재할 수 있는 것은 해당 시설의 레시피 슬롯 수, 연료 타입, 처리 속도 배율 등 시설 구조적 파라미터에 한정된다.

### 2-2. `.claude/rules/doc-standards.md` — Canonical 데이터 매핑 테이블

테이블 마지막 행에 신규 매핑 항목을 추가하였다.

추가된 행:
| 가공소별 레시피 목록(재료, 산출물, 판매가, 처리시간) | `docs/content/processing-system.md` | 시설 문서에 직접 기재 금지 |

### 2-3. `.claude/rules/workflow.md` — Reviewer Checklist

항목 11 다음에 항목 12를 추가하였다.

추가된 항목:
> 12. [ ] (PATTERN-008) 시설 문서(facilities.md 등)의 가공소 섹션에 레시피 목록(재료/산출물/가격)을 직접 기재했을 경우, canonical 문서(processing-system.md) 참조로 교체되었는가? — 시설 문서에 허용되는 것은 슬롯 수·연료 타입·처리 속도 배율 등 구조적 파라미터뿐이다.

---

## 3. CLAUDE.md 제안 사항

없음. 이번 변경은 모두 `.claude/rules/` 범위 내에서 완결되었으며 CLAUDE.md 수정이 필요한 사항은 발견되지 않았다.

---

## 4. 다음 권장 행동

1. **기존 문서 소급 수정**: `docs/content/facilities.md` 섹션 7(제분소), 섹션 8(발효실), 섹션 9(베이커리)의 레시피 목록 직접 기재 부분을 `(→ see docs/content/processing-system.md 섹션 X.X)` 참조로 교체한다. — 이 작업은 designer 또는 reviewer 에이전트가 담당한다.
2. **리뷰 재실행**: 소급 수정 완료 후 `/review`를 실행하여 Reviewer Checklist 항목 12를 포함한 전체 검증을 수행한다.
3. **TODO.md 정리**: PATTERN-008 항목을 완료 처리한다.

---

## 5. 요약

| 항목 | 값 |
|------|---|
| 발견된 패턴 수 | 1종 |
| 불일치 사례 수 | 3건 |
| 규칙 파일 변경 수 | 2개 (`doc-standards.md`, `workflow.md`) |
| CLAUDE.md 제안 수 | 0건 |
