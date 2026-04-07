---
description: 세션 시작 시 이것만 실행. 컨텍스트 복원 → 최우선 작업 선택 → 설계 확장 → 검증 ��� 커밋 푸시까지 자동 수행
---

You are running a **planning session** for SeedMind.

## Phase 0 — Context Restore (새 세션마다 필수)

1. Read `CLAUDE.md` — project rules
2. Read `TODO.md` — current backlog
3. Read `README.md` — current phase/status
4. Read the latest `docs/devlog/*.md` — what happened last session
5. `docs/design.md` and `docs/architecture.md`는 **최우선 태스크가 신규 시스템 설계(DES-*)인 경우에만** 읽는다. BAL/FIX/CON/ARC 태스크인 경우 해당 태스크의 관련 문서만 읽는다.
6. Print a 3-line status summary to the user:
   ```
   [SeedMind] Phase N | TODO: N items | 최근 작업: <last devlog title>
   이번 세션 목표: <highest-priority TODO item>
   진행합니다.
   ```

## Phase 1 — Task Selection

- Pick the highest-priority item from `TODO.md`
- If multiple items share the same priority, pick the one that unblocks the most other items

## Phase 2 — Design Expansion

태스크 유형에 따라 에이전트 전략을 선택한다:

| 태스크 유형 | 전략 | 설명 |
|------------|------|------|
| `DES-*` (신규 시스템 설계) | designer + architect 병렬 | 디자인과 아키텍처를 동시에 |
| `ARC-*` (아키텍처 단독) | architect만 | 디자인 문서 이미 존재 |
| `BAL-*` (밸런스 분석) | designer만 | 수치 계산 집약, 구조 변경 없음 |
| `FIX-*` (문서 수정) | 에이전트 없이 직접 편집 | 단순 필드 추가/동기화/참조 수정 |
| `CON-*` (콘텐츠 추가) | designer만 | 기존 구조에 콘텐츠 채우기 |

**병렬 에이전트 사용 조건**: DES-* 태스크이면서 해당 시스템 아키텍처 문서가 아직 없는 경우에만 designer+architect를 동시에 스폰한다.

**architect 에이전트 단독 실행 시**: 반드시 designer 결과물 또는 관련 디자인 문서를 먼저 읽은 후 작성한다. `doc-standards.md`의 Canonical 데이터 매핑을 확인하고 수치 직접 기재를 금지한다.

## Phase 3 — Review

태스크 유형에 따라 리뷰어 실행 여부를 결정한다:

| 태스크 유형 | 리뷰어 실행 여부 | 리유 |
|------------|----------------|------|
| `DES-*` 신규 시스템 | 필수 | 신규 문서 전체 검증 필요 |
| `ARC-*` 신규 아키텍처 | 필수 | JSON/C# 불일치, enum 동기화 등 |
| `BAL-*` 밸런스 분석 | 필수 | 연료비 계산 오류, 수치 불일치 빈발 |
| `CON-*` 콘텐츠 추가 | 필수 | cross-reference 및 canonical 참조 누락 빈발 |
| `FIX-*` 단순 수정 (1~2 필드 동기화) | 생략 가능 | 변경 범위가 단일 필드/참조에 한정될 때 |

**FIX-* 리뷰어 생략 조건**: 변경이 명확히 한 문서의 한 섹션에 국한되고, canonical 출처가 이미 확정된 수치를 그대로 옮기는 경우에만 생략한다. 신규 수치 도입 또는 여러 문서 동시 수정 시 리뷰어 필수.

**리뷰어 실행 시**: Reviewer Checklist 12개 항목을 전수 확인한다. 체크리스트 항목을 생략하거나 "해당 없음"으로 처리할 경우 그 이유를 명시해야 한다.

## Phase 4 — Wrap Up

1. Update `TODO.md` — remove completed items, add new ones discovered
2. If TODO.md drops below 10 rows: analyze docs for gaps, add new items
3. Update `README.md` current status if a phase milestone was reached
4. Write a devlog entry in `docs/devlog/NNN-<title>.md`
5. Commit and push

## Session Task Budget

Phase 0에서 선택한 최우선 태스크의 Priority로 이번 세션 처리 개수를 결정한다:

| 최우선 태스크 Priority | 세션 내 처리 개수 |
|----------------------|----------------|
| 3 이상 | 1개 |
| 2 | 2개 |
| 1 | 3~4개 |

Phase 4 완료 후 **남은 budget이 있으면**:
1. `TODO.md`만 재읽기 (Phase 0의 다른 파일은 재읽지 않는다)
2. 다음 최우선 항목으로 Phase 1→4 반복
3. Budget 소진 또는 Priority-2+ 항목만 남으면 종료

## Rules
- Designer and architect must reference each other's output
- Reviewer has final say on consistency issues
- All output in Korean. Document content in Korean (technical terms in English where natural).
- Budget 내에서는 루프 가능. Budget 소진 시 반드시 종료.
