# Self-Improve 보고서 — 2026-04-06

- **에이전트**: self-improve
- **트리거**: 3세션 연속 동일 패턴 발생 (PATTERN-001, PATTERN-002)
- **분석 범위**: git log (최근 5커밋), TODO.md, `.claude/rules/*.md`

---

## 1. 발견된 패턴

### PATTERN-001 — Canonical 수치 독립 복사 기재
- **유형**: 반복 문서 일관성 오류
- **발생 횟수**: 3세션 × 복수 건 (세션2: 경작 시스템 1건 이상, 세션3: 작물 성장 3건, 세션4: 시간/계절 3건)
- **근본 원인**: 아키텍처 문서 작성 시 디자인 문서의 수치를 참조 없이 직접 복사해 기재. 이후 디자인 문서 수치가 변경되어도 아키텍처 문서는 갱신되지 않아 불일치 발생
- **대표 예시**:
  - `docs/systems/crop-growth.md` 성장일수가 `docs/design.md` 섹션 4.2 값과 불일치
  - `docs/systems/time-season-architecture.md` 날씨 확률이 `docs/systems/time-season.md` 값과 불일치

### PATTERN-002 — 동일 문서 내 섹션 간 수치 불일치
- **유형**: 단일 문서 내부 일관성 오류
- **발생 횟수**: 1건 이상 확인 (세션4)
- **근본 원인**: 튜닝 파라미터 섹션과 로직 설명 섹션에 동일 수치가 중복 기재되어 한쪽만 수정 시 불일치 발생
- **대표 예시**:
  - `docs/systems/crop-growth-architecture.md` — `giantCropChance` 값이 섹션 A에서 1%, 섹션 B에서 15%로 불일치

---

## 2. 적용된 변경사항

### TODO.md (적용 완료)
- `PATTERN-001` — 완료 처리
- `PATTERN-002` — 완료 처리

---

## 3. CLAUDE.md 및 `.claude/rules/` 변경 제안 (사용자 직접 적용 필요)

에이전트는 `.claude/rules/` 하위 파일을 직접 수정할 수 없습니다 (Claude Code 보안 정책). 아래 변경사항을 사용자가 직접 적용해주세요.

### 3-1. `.claude/rules/doc-standards.md` — Consistency Rules 섹션 전체 교체

**현재:**
```
## Consistency Rules
- Crop names, building names, system names: use the EXACT same string everywhere
- Numbers (prices, growth days, thresholds): single source of truth in the most specific doc
- If a value appears in multiple docs, one must be marked as canonical with `(→ see docs/X.md)`
```

**교체 내용:**
```markdown
## Consistency Rules
- 작물 이름, 건물 이름, 시스템 이름: 모든 문서에서 완전히 동일한 문자열 사용
- 수치(가격, 성장일수, 임계값): 가장 구체적인 canonical 문서에 단일 출처 기재
- 수치가 여러 문서에 나타날 경우, canonical 문서 외 모든 곳에는 실제 값 대신 `(→ see docs/X.md)` 참조만 기재
- **아키텍처 문서에서 디자인 수치를 기재할 때는 반드시 `(→ see canonical)` 참조를 붙여야 하며, 독립적으로 수치를 기재하는 것을 금지한다**
- **코드 예시 내 기본값(defaultValue, tuningParam 등)도 canonical 참조 주석 필요**: `// → see docs/systems/crop-growth.md`
- 동일 문서 내에서도 섹션 간 수치가 중복 기재되지 않도록 한다. 한 섹션에 기재하고 다른 섹션에서는 해당 섹션을 참조한다

## Canonical 데이터 매핑

신규 문서 작성 시, 아래 매핑에 따라 canonical 출처를 확인하고 수치를 직접 기재하지 않는다.

| 데이터 종류 | Canonical 문서 | 비고 |
|------------|---------------|------|
| 작물 이름, 씨앗 가격, 판매가, 성장일수 | `docs/design.md` 섹션 4.2 | 작물 목록 전체 |
| 작물 성장 단계, 품질 공식, 특수 성장(giant 등) | `docs/systems/crop-growth.md` | 성장 메카닉 전반 |
| 타일 상태, 도구 인터랙션, 물/비료 효과 | `docs/systems/farming-system.md` | 경작 메카닉 전반 |
| 시간대 정의, 날씨 종류, 날씨 확률, 계절 전환 | `docs/systems/time-season.md` | 시간/날씨 데이터 |
| 시설 이름, 건설 요건, 업그레이드 경로 | `docs/design.md` 섹션 4.6 | 시설 목록 전체 |
| Unity 프로젝트 폴더 구조, 네임스페이스 | `docs/systems/project-structure.md` | 프로젝트 구조 |
```

### 3-2. `.claude/rules/workflow.md` — Agent Collaboration 섹션 교체

**현재:**
```
## Agent Collaboration
- **Designer** and **Architect** work in parallel on the same system
- **Reviewer** always runs after Designer+Architect complete
- **Self-Improve** runs when triggered (3+ same-type issues, or user request)
```

**교체 내용:**
```markdown
## Agent Collaboration
- **Designer** and **Architect** work in parallel on the same system
- **Architect** 에이전트는 문서 작성 전 반드시 `doc-standards.md`의 Canonical 데이터 매핑을 확인한다. 수치 직접 기재 금지, 참조 표기 필수
- **Reviewer** always runs after Designer+Architect complete
- **Self-Improve** runs when triggered (3+ same-type issues, or user request)

## Reviewer Checklist

리뷰어는 문서 검토 시 아래 항목을 반드시 확인한다:

1. [ ] 모든 수치(가격, 성장일수, 확률, 임계값)에 canonical 출처 참조 표기가 있는가?
2. [ ] 아키텍처 문서가 디자인 문서의 수치를 독립 복사하여 기재하고 있지 않은가?
3. [ ] 코드 예시 내 기본값에 `// → see canonical` 주석이 있는가?
4. [ ] 동일 문서 내 같은 수치가 섹션 간 중복 기재되어 있지 않은가?
5. [ ] Cross-references 섹션이 존재하고 관련 문서를 모두 나열하고 있는가?
6. [ ] [OPEN] 및 [RISK] 태그가 적절히 사용되었는가?
```

---

## 4. 다음 권장 액션

1. **즉시**: 사용자가 위 3-1, 3-2 변경사항을 `.claude/rules/` 파일에 직접 적용
2. **다음 세션**: DES-004 (경제 시스템) 작성 시 새 규칙을 처음으로 적용하여 효과 검증
3. **중기**: 기존 아키텍처 문서 (`farming-architecture.md`, `crop-growth-architecture.md`, `time-season-architecture.md`) 소급 점검 — 독립 기재 수치를 참조 표기로 교체
4. **향후**: 3세션 후 동일 패턴 재발 여부 확인. 재발 시 리뷰어 자동화 체크리스트 명령어 (`/review`) 내 별도 단계 추가 검토
