# TODO

| ID | Priority | Description |
|----|----------|-------------|
| ~~DES-001~~ | ~~5~~ | ~~경작 시스템 상세 설계 (타일 상태, 도구 인터랙션, 물/비료 효과)~~ — DONE → `docs/systems/farming-system.md` |
| ~~DES-002~~ | ~~5~~ | ~~작물 성장 시스템 상세 (성장 단계, 시각적 변화, 계절 제한)~~ — DONE → `docs/systems/crop-growth.md` |
| ~~DES-003~~ | ~~4~~ | ~~시간/계절 시스템 상세 (하루 흐름, 계절 전환, 날씨)~~ — DONE → `docs/systems/time-season.md`, `docs/systems/time-season-architecture.md` |
| DES-004 | 4 | 경제 시스템 상세 (가격 변동, 상점 구조, 수입/지출 밸런스) |
| ~~ARC-001~~ | ~~5~~ | ~~Unity 프로젝트 구조 상세 설계~~ — DONE → `docs/systems/project-structure.md` |
| ~~ARC-005~~ | ~~5~~ | ~~경작 시스템 기술 아키텍처~~ — DONE → `docs/systems/farming-architecture.md` |
| ARC-002 | 4 | MCP 작업 계획 — 기본 씬 구성 태스크 시퀀스 |
| ARC-003 | 4 | MCP 작업 계획 — 농장 그리드 생성 태스크 시퀀스 |
| ARC-004 | 3 | 데이터 파이프라인 설계 (ScriptableObject 구조, JSON 스키마) |
| BAL-001 | 3 | 작물 경제 밸런스 시트 (씨앗 비용 vs 판매가, ROI 분석) |
| BAL-002 | 3 | 게임 진행 곡선 (레벨별 해금, 예상 플레이타임) |
| CON-001 | 2 | 작물 콘텐츠 상세 (전체 작물 목록, 계절별 분류, 특수 작물) |
| CON-002 | 2 | 시설 콘텐츠 상세 (건설 요건, 업그레이드 경로) |
| CON-003 | 2 | NPC/상점 콘텐츠 (상인 캐릭터, 대화, 상점 인벤토리) |
| VIS-001 | 2 | 비주얼 가이드 (로우폴리 스타일 참고자료, 색상 팔레트) |
| AUD-001 | 1 | 사운드 디자인 문서 (필요한 효과음/BGM 목록) |
| PATTERN-001 | - | [self-improve 전용] 신규 문서가 canonical 문서의 수치/이름을 복사해 독립 기재 → 불일치 반복 발생. 규칙 추가: 신규 문서는 수치를 직접 기재하지 말고 `(→ see canonical-doc)` 참조만 기재하도록 doc-standards.md 업데이트 |
| PATTERN-002 | - | [self-improve 전용] 동일 문서 내에서도 섹션 간 수치 불일치 발생 (예: crop-growth-architecture.md giantCropChance 1% vs 15%). 규칙 추가: 아키텍처 문서의 튜닝 파라미터 섹션을 항상 디자인 문서와 대조하는 검증 체크리스트 항목 추가 필요 |
