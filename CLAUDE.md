# SeedMind — AI Autonomous Farm Simulation

AI(Claude Code)가 사람의 개입 없이 농장 시뮬레이션 게임을 설계·구축·배포하는 실험적 프로젝트.

## 현재 상태
- **Phase 1**: 게임 설계 (문서 기반 확장 중)
- 코드 작업 없음 — 모든 작업은 문서로 진행
- 문서가 충분히 성숙하면 Phase 2(Unity 구현)로 전환

## 기술 스택
- **Engine**: Unity 6 (MCP for Unity 연동)
- **Language**: C#
- **AI ↔ Unity**: MCP로 씬/오브젝트/스크립트 직접 제어

## 핵심 규칙
1. Language: 모든 출력 한국어. 코드/변수명은 영어. `.claude/` 하위 파일(agents, rules, commands)은 **영문으로 작성** — AI 지시문이므로 영문이 일관성 및 해석 정확도에 유리.
2. Document-only phase: 코드 파일 생성 금지. `docs/` 하위 마크다운만 작성.
3. 매 작업 후: 문서 작성 → `/review` → 수정 → commit → push. (FIX-* 단순 수정은 /review 생략 가능 — 변경이 단일 문서·단일 섹션에 한정되고 확정 수치를 그대로 옮기는 경우)
4. 주기적: `/self-improve` (3+ 동일 이슈 발견 시, 또는 10+ TODO 완료 시).
5. 상세 규칙: `.claude/rules/doc-standards.md` (문서 표준·캐노니컬 매핑), `.claude/rules/workflow.md` (워크플로·에이전트·체크리스트) 참조.

## 에이전트 체계
> 상세 정의: `.claude/agents/*.md`

| Agent | 적용 태스크 | Role |
|-------|------------|------|
| designer | DES / BAL / CON | 게임 디자인 확장/심화 |
| architect | DES(순차) / ARC | 기술 아키텍처 설계 |
| reviewer | DES / ARC / BAL / CON 필수, FIX 조건부 | 문서 일관성/완전성 검증 |
| self-improve | PATTERN-* 또는 사용자 요청 | 반복 패턴 감지 → 규칙/커맨드 개선 |

## 커맨드
> 상세 정의: `.claude/commands/*.md`

| Command | Description |
|---------|-------------|
| `/start` | 세션 시작 — 컨텍스트 복원 → 최우선 작업 → 설계 → 검증 → 커밋 |
| `/expand <system>` | 특정 시스템 문서 확장 |
| `/review` | 전체 문서 일관성 검증 |
| `/todo` | TODO에서 최우선 항목 실행 |
| `/self-improve` | 메타 개선 분석 |
| `/devlog` | 개발 일지 작성 |
| `/cost-monitor` | 세션 비용 분석 및 최적화 |

## 프로젝트 구조
```
docs/
├── design.md          # 마스터 게임 디자인
├── architecture.md    # 마스터 기술 아키텍처
├── systems/           # 시스템별 상세 문서
├── content/           # 콘텐츠 스펙 (작물, 건물, NPC)
├── balance/           # 밸런스 시트
├── mcp/               # MCP 작업 계획
├── pipeline/          # 빌드/에셋 파이프라인
└── devlog/            # 개발 일지
logs/
└── reports/           # /review, /self-improve 출력 보고서
```

## Git
- Commit message: 한국어
- 매 논리적 작업 단위마다 commit + push
- Co-Authored-By 태그 포함

## Session Defaults
- 모든 /command는 이 규칙을 상속
- 문서 변경 후 항상 cross-reference 검증
- TODO 자동 보충: DES/ARC 항목이 0개일 때만 신규 추가 (FIX/BAL/CON 수는 무시)
- FIX 즉시 처리 원칙: 작업 완료 시 발견되는 후속 수정(참조 추가, 수치 반영, enum 동기화 등)은 같은 작업 내에서 즉시 처리. 별도 FIX 항목으로 TODO에 등록하지 않음. TODO에 FIX를 올리는 경우는 즉시 처리 불가능한 경우만 (다른 시스템 설계 미확정, 사용자 결정 필요 등)
