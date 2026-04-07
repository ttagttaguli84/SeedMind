# Workflow Rules

## Document-Only Policy
- This project is in DESIGN PHASE — no code files (.cs, .json, .unity) are written
- All work produces markdown documents in `docs/`
- Code snippets in docs are illustrative, not executable

## Agent Collaboration

에이전트 선택은 태스크 유형에 따라 결정한다 (자세한 기준은 `start.md` Phase 2/3 참조):

| 태스크 유형 | Phase 2 에이전트 | Phase 3 리뷰어 |
|------------|----------------|---------------|
| DES-* 신규 시스템 | designer + architect 병렬 | 필수 |
| ARC-* 아키텍처 단독 | architect만 | 필수 |
| BAL-* 밸런스 분석 | designer만 | 필수 |
| CON-* 콘텐츠 추가 | designer만 | 필수 |
| FIX-* 단순 수정 | 에이전트 없이 직접 편집 | 생략 가능 (조건 있음) |

- **Architect** 에이전트는 문서 작성 전 반드시 `doc-standards.md`의 Canonical 데이터 매핑을 확인한다. 수치 직접 기재 금지, 참조 표기 필수
- **Architect** 에이전트가 단독 실행될 때는 관련 디자인 문서(DES, CON)를 먼저 읽은 후 작성을 시작한다. 플레이스홀더 수치/ID 사용 금지 — 디자인이 확정된 후에만 아키텍처를 작성한다 (PATTERN-010)
- **Designer** 에이전트가 BAL-* 또는 CON-* 작업 시 연료비·연산 오류 등 경제 수치 계산은 단계별로 명시하여 리뷰어가 검증 가능하도록 한다 (PATTERN-BAL-COST)
- **Self-Improve** runs when triggered (3+ same-type issues, or user request)

## Reviewer Checklist

리뷰어는 문서 검토 시 아래 항목을 반드시 확인한다:

1. [ ] 모든 수치(가격, 성장일수, 확률, 임계값)에 canonical 출처 참조 표기가 있는가?
2. [ ] 아키텍처 문서가 디자인 문서의 수치를 독립 복사하여 기재하고 있지 않은가?
3. [ ] 코드 예시 내 기본값에 `// → see canonical` 주석이 있는가?
4. [ ] 동일 문서 내 같은 수치가 섹션 간 중복 기재되어 있지 않은가?
5. [ ] enum/타입이 확장된 경우, 같은 문서 내 모든 switch 문과 코드 예시가 업데이트되었는가?
6. [ ] Part I(디자인)과 Part II(MCP 구현) 간 오브젝트 계층, 초기값, 배치 정보가 일치하는가?
7. [ ] Cross-references 섹션이 존재하고 관련 문서를 모두 나열하고 있는가?
8. [ ] [OPEN] 및 [RISK] 태그가 적절히 사용되었는가?
9. [ ] (PATTERN-005) 동일 문서 내 Part I JSON 스키마와 Part II C# 클래스의 필드명·필드 수가 일치하는가? — 한쪽에 필드를 추가/삭제했을 때 반대쪽도 즉시 업데이트되었는가?
10. [ ] (PATTERN-006) MCP 태스크 문서(Part II 포함)에 배열·테이블 형태의 수치를 직접 기재한 경우, `(→ see canonical)` 참조 주석이 함께 기재되어 있는가? — 직접 기재 시 canonical 문서와 동시 수정 여부를 확인한다.
11. [ ] (PATTERN-007) SO 에셋 데이터 테이블(data-pipeline.md 섹션 2.4 등)에서 tileSize/buildTimeDays/effectRadius/recipeCount 등 콘텐츠 정의 파라미터를 직접 수치로 기재했을 경우, canonical 콘텐츠 문서(docs/content/facilities.md 등) 참조로 교체되었는가? — 직접 기재된 구체적 수치는 반드시 참조 표기로 대체한다.
12. [ ] (PATTERN-008) 시설 문서(facilities.md 등)의 가공소 섹션에 레시피 목록(재료/산출물/가격)을 직접 기재했을 경우, canonical 문서(processing-system.md) 참조로 교체되었는가? — 시설 문서에 허용되는 것은 슬롯 수·연료 타입·처리 속도 배율 등 구조적 파라미터뿐이다.
13. [ ] (PATTERN-BAL-COST) 경제/밸런스 문서에서 가공 ROI·수익 계산 시 연료비·재료비가 빠짐없이 차감되었는가? — "가공품 가격 - 재료 직판가"만 계산하고 연료비를 누락한 경우 오계산으로 간주한다. 베이커리/발효실 레시피는 반드시 연료비를 포함하여 순이익을 산출해야 한다.
14. [ ] (PATTERN-010) 아키텍처 문서에서 디자인 문서가 확정하지 않은 수치·ID를 플레이스홀더로 기재한 경우, `[OPEN]` 태그와 함께 명시적으로 표기하고 디자인 확정 전 수치를 canonical 값인 것처럼 기재하지 않았는가?

## Commit Cadence
- Commit after each logical unit of work (one system expansion, one review pass)
- Push after every commit
- Devlog entry at end of each session or major milestone

## TODO Management
- TODO.md is the single backlog
- Format: `| ID | Priority(1~5) | Description |`
- Priority 5 = most urgent
- PATTERN- prefix = systemic issue, handled by self-improve only

## Phase Progression
- Phase advances when all TODO items for that phase are complete
- Update README.md status when phase changes
- Write a devlog entry for each phase transition
