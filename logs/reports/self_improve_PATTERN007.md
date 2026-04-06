# Self-Improve Report: PATTERN-007

- **날짜**: 2026-04-06
- **트리거**: 사용자 요청 (PATTERN-007 이슈 처리)
- **분석 대상**: docs/pipeline/data-pipeline.md, docs/content/facilities.md

---

## 패턴 분석

### PATTERN-007: SO 에셋 테이블의 콘텐츠 파라미터 불일치

**유형**: 수치 불일치 (cross-document canonical 위반)
**발생 건수**: 4건 이상

**구체적 불일치 사례**:

| 파라미터 | data-pipeline.md 섹션 2.4 기재값 | facilities.md canonical값 |
|---------|--------------------------------|--------------------------|
| 온실 tileSize | 내부 경작지 4x4 언급 | 외벽 포함 6x6 (내부 경작 Lv.1: 4x4) |
| 가공소 tileSize | 직접 기재 없이 혼재 | 4x3 (12타일) |
| 창고 tileSize | (2,2) 기본값으로 혼재 | 3x2 (6타일) |
| 온실/가공소 buildTimeDays | 직접 기재 | facilities.md 섹션 2.1: 온실 2일, 가공소 2일 |

**근본 원인**: 파이프라인 문서(data-pipeline.md)의 "시설별 에셋 데이터" 테이블이 `tileSize`, `buildTimeDays` 등 콘텐츠 파라미터를 직접 수치로 기재하면서 canonical 콘텐츠 문서(facilities.md)와 독립된 출처가 두 개 생성됨. 이후 한쪽 값이 수정될 때 다른 쪽이 갱신되지 않아 불일치 발생.

---

## 적용된 변경

### 1. `.claude/rules/doc-standards.md`

**변경 1 — Consistency Rules에 PATTERN-007 규칙 추가** (라인 29):
```
- (PATTERN-007) SO 에셋 테이블의 콘텐츠 파라미터 직접 기재 금지: data-pipeline.md 등
  파이프라인/아키텍처 문서의 SO 에셋 데이터 테이블에서 tileSize, buildTimeDays,
  recipeCount 등 콘텐츠 정의 파라미터를 구체적 수치로 직접 기재하는 것을 금지한다.
  canonical 콘텐츠 문서를 (-> see docs/content/facilities.md 섹션 X.X) 형식으로만 참조한다.
```

**변경 2 — Canonical 데이터 매핑 테이블에 항목 추가** (라인 44):
```
| 시설 tileSize, buildTimeDays, effectRadius 등 구조적 파라미터 |
  docs/content/facilities.md | 섹션 2.1 건설 프로세스 / 시설별 상세 섹션 |
```

### 2. `.claude/rules/workflow.md`

**변경 — Reviewer Checklist 항목 11 추가** (라인 28):
```
11. [ ] (PATTERN-007) SO 에셋 데이터 테이블(data-pipeline.md 섹션 2.4 등)에서
    tileSize/buildTimeDays/effectRadius/recipeCount 등 콘텐츠 정의 파라미터를
    직접 수치로 기재했을 경우, canonical 콘텐츠 문서(docs/content/facilities.md 등)
    참조로 교체되었는가?
```

### 3. `TODO.md`

- PATTERN-007 항목 DONE 처리

---

## CLAUDE.md 제안 사항

없음. 이번 변경은 `.claude/rules/` 범위 내에서 완결됨.

---

## 후속 권장 작업

1. **data-pipeline.md 섹션 2.4 수정 필요** [승인 필요]: 현재 "시설별 에셋 데이터" 테이블(라인 221~234)에 직접 기재된 tileSize/buildTimeDays 수치를 `(→ see docs/content/facilities.md 섹션 X.X)` 참조로 교체해야 함. 이는 디자인 문서 변경이므로 본 self-improve 에이전트 권한 밖.
2. **다음 /review 실행 시**: Reviewer Checklist 항목 11을 통해 data-pipeline.md 섹션 2.4의 잔존 직접 기재 수치를 검출하고 FIX 태스크로 등록할 것.

---

## 요약

- **패턴 발견**: 1종 (PATTERN-007, 4건 불일치)
- **규칙 파일 적용**: 2건 (doc-standards.md, workflow.md)
- **TODO 처리**: 1건 (PATTERN-007 DONE)
- **CLAUDE.md 제안**: 0건
