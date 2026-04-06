# Self-Improve 보고서 v2 — 2026-04-06

- **에이전트**: self-improve
- **트리거**: PATTERN-001~004 처리 요청 (이전 세션 미완료 항목 포함)
- **분석 범위**: git log (최근 8커밋), TODO.md, `.claude/rules/*.md`, 이전 보고서(`self_improve_20260406.md`), `docs/systems/economy-architecture.md`, `docs/mcp/scene-setup-tasks.md`

---

## 1. 이전 세션 상태 확인

이전 보고서(`self_improve_20260406.md`)에서 PATTERN-001/002에 대한 규칙 파일 변경안이 제시되었으나, 해당 보고서는 변경을 "사용자 직접 적용 필요"로 표기하고 실제 파일은 수정하지 않았다. TODO.md에도 PENDING 상태로 남아 있었다. 이번 세션에서 PATTERN-001~004 전체를 처리한다.

---

## 2. 발견된 패턴

### PATTERN-001 — Canonical 수치 독립 복사 기재
- **유형**: 반복 문서 일관성 오류
- **발생 횟수**: 3세션 이상 × 복수 건
- **신규 사례 (이번 분석)**: `docs/systems/economy-architecture.md` 클래스 다이어그램 내 `startingGold: 500` 직접 기재 — canonical 출처인 `docs/systems/economy-system.md` 섹션 1.2에 수치가 있으므로 참조 표기로 대체해야 한다
- **근본 원인**: 아키텍처 문서 작성 시 디자인/시스템 문서의 수치를 참조 없이 복사 기재

### PATTERN-002 — 동일 문서 내 섹션 간 수치 불일치
- **유형**: 단일 문서 내부 일관성 오류
- **발생 횟수**: 세션4 1건 확인 (crop-growth-architecture.md giantCropChance)
- **근본 원인**: 동일 수치가 여러 섹션에 중복 기재되어 한쪽만 수정 시 불일치 발생

### PATTERN-003 — enum 확장 시 코드 예시 미갱신
- **유형**: 단일 문서 내부 일관성 오류 (enum 특화)
- **발생 건수**: 1건 확인 (WeatherType 5종→7종, GetWeatherMultiplier 예시 미갱신)
- **근본 원인**: enum 값 목록과 해당 enum을 처리하는 switch/if 코드 예시가 별도 섹션에 기재되어 있어, 목록 업데이트 시 코드 예시를 함께 갱신하지 않음

### PATTERN-004 — 디자인(Part I) vs MCP 구현(Part II) 불일치
- **유형**: 동일 문서 내 부분 간 불일치
- **발생 건수**: 4건 (`docs/mcp/scene-setup-tasks.md` 기준 — Buildings 부모 오브젝트, 시작 시각, EconomyManager 배치, TestPlayer 누락)
- **근본 원인**: Part I에서 확정된 계층 구조/초기값이 Part II MCP 태스크 기술 시 반영되지 않음. 두 섹션을 작성하는 간격이 길어지면서 발생

---

## 3. 적용된 변경사항

### 3-1. `.claude/rules/doc-standards.md` 업데이트
- **Consistency Rules 섹션**: 한국어로 재작성 + 아키텍처 문서의 수치 직접 기재 금지 명시 + enum 전수 업데이트 규칙 추가
- **Canonical 데이터 매핑 섹션 신규 추가**: 데이터 종류별 canonical 문서 매핑 테이블 (7개 항목)
- **해결하는 패턴**: PATTERN-001, PATTERN-003

### 3-2. `.claude/rules/workflow.md` 업데이트
- **Agent Collaboration 섹션**: Architect 에이전트의 canonical 매핑 확인 의무 추가
- **Reviewer Checklist 섹션 신규 추가**: 8개 항목의 리뷰 체크리스트
  - 항목 1~3: canonical 참조 검증 (PATTERN-001)
  - 항목 4: 동일 문서 내 중복 수치 검증 (PATTERN-002)
  - 항목 5: enum 코드 예시 전수 갱신 검증 (PATTERN-003)
  - 항목 6: Part I vs Part II 대조 검증 (PATTERN-004)
  - 항목 7~8: 기존 구조 검증 (cross-references, 태그)
- **해결하는 패턴**: PATTERN-002, PATTERN-003, PATTERN-004

### 3-3. `TODO.md` 업데이트
- PATTERN-001, PATTERN-002, PATTERN-003, PATTERN-004 모두 DONE 처리

---

## 4. 신규 패턴 후보 (다음 self-improve 시 추적)

| 후보 ID | 설명 | 감지 근거 | 임계값 |
|---------|------|-----------|--------|
| PATTERN-005 (후보) | 아키텍처 다이어그램 내 ScriptableObject 기본값 직접 기재 | economy-architecture.md `startingGold: 500` (이번 분석에서 1건 확인) | 3건 누적 시 패턴 공식 등록 |

소급 수정 필요 항목: `docs/systems/economy-architecture.md` 클래스 다이어그램의 `startingGold: 500` → `startingGold: (→ see docs/systems/economy-system.md 섹션 1.2)` 로 교체. 다음 해당 문서 수정 시 함께 처리.

---

## 5. CLAUDE.md 변경 제안

없음. 현재 CLAUDE.md는 self-improve 트리거 조건(3+ 동일 이슈, 또는 10+ TODO 완료)을 이미 명시하고 있으며 추가 변경 불필요.

---

## 6. 다음 권장 액션

1. **즉시**: ARC-003 (농장 그리드 생성 MCP 태스크 시퀀스) 작성 — 새 Reviewer Checklist를 처음으로 적용하여 PATTERN-004 재발 방지 효과 검증
2. **단기**: 기존 아키텍처 문서들 소급 점검
   - `docs/systems/economy-architecture.md` — `startingGold: 500` 참조 표기 교체
   - `docs/systems/crop-growth-architecture.md` — `giantCropChance` 중복 값 통일
   - `docs/systems/time-season-architecture.md` — 날씨 확률 참조 표기 교체
3. **중기**: BAL-001 (작물 경제 밸런스 시트) 작성 시 Canonical 데이터 매핑 테이블 활용 첫 사례로 삼기
