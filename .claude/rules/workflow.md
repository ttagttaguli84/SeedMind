# Workflow Rules

## Document-Only Policy
- This project is in DESIGN PHASE — no code files (.cs, .json, .unity) are written
- All work produces markdown documents in `docs/`
- Code snippets in docs are illustrative, not executable

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
5. [ ] enum/타입이 확장된 경우, 같은 문서 내 모든 switch 문과 코드 예시가 업데이트되었는가?
6. [ ] Part I(디자인)과 Part II(MCP 구현) 간 오브젝트 계층, 초기값, 배치 정보가 일치하는가?
7. [ ] Cross-references 섹션이 존재하고 관련 문서를 모두 나열하고 있는가?
8. [ ] [OPEN] 및 [RISK] 태그가 적절히 사용되었는가?
9. [ ] (PATTERN-005) 동일 문서 내 Part I JSON 스키마와 Part II C# 클래스의 필드명·필드 수가 일치하는가? — 한쪽에 필드를 추가/삭제했을 때 반대쪽도 즉시 업데이트되었는가?
10. [ ] (PATTERN-006) MCP 태스크 문서(Part II 포함)에 배열·테이블 형태의 수치를 직접 기재한 경우, `(→ see canonical)` 참조 주석이 함께 기재되어 있는가? — 직접 기재 시 canonical 문서와 동시 수정 여부를 확인한다.
11. [ ] (PATTERN-007) SO 에셋 데이터 테이블(data-pipeline.md 섹션 2.4 등)에서 tileSize/buildTimeDays/effectRadius/recipeCount 등 콘텐츠 정의 파라미터를 직접 수치로 기재했을 경우, canonical 콘텐츠 문서(docs/content/facilities.md 등) 참조로 교체되었는가? — 직접 기재된 구체적 수치는 반드시 참조 표기로 대체한다.

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
