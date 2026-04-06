# TODO

| ID | Priority | Description |
|----|----------|-------------|
| ~~DES-001~~ | ~~5~~ | ~~경작 시스템 상세 설계 (타일 상태, 도구 인터랙션, 물/비료 효과)~~ — DONE → `docs/systems/farming-system.md` |
| ~~DES-002~~ | ~~5~~ | ~~작물 성장 시스템 상세 (성장 단계, 시각적 변화, 계절 제한)~~ — DONE → `docs/systems/crop-growth.md` |
| ~~DES-003~~ | ~~4~~ | ~~시간/계절 시스템 상세 (하루 흐름, 계절 전환, 날씨)~~ — DONE → `docs/systems/time-season.md`, `docs/systems/time-season-architecture.md` |
| ~~DES-004~~ | ~~4~~ | ~~경제 시스템 상세 (가격 변동, 상점 구조, 수입/지출 밸런스)~~ — DONE → `docs/systems/economy-system.md`, `docs/systems/economy-architecture.md` |
| ~~ARC-001~~ | ~~5~~ | ~~Unity 프로젝트 구조 상세 설계~~ — DONE → `docs/systems/project-structure.md` |
| ~~ARC-005~~ | ~~5~~ | ~~경작 시스템 기술 아키텍처~~ — DONE → `docs/systems/farming-architecture.md` |
| ~~ARC-002~~ | ~~4~~ | ~~MCP 작업 계획 — 기본 씬 구성 태스크 시퀀스~~ — DONE → `docs/mcp/scene-setup-tasks.md` |
| ~~ARC-003~~ | ~~4~~ | ~~MCP 작업 계획 — 농장 그리드 생성 태스크 시퀀스~~ — DONE → `docs/mcp/farming-tasks.md` |
| ~~ARC-004~~ | ~~3~~ | ~~데이터 파이프라인 설계 (ScriptableObject 구조, JSON 스키마)~~ — DONE → `docs/pipeline/data-pipeline.md` |
| ~~BAL-001~~ | ~~3~~ | ~~작물 경제 밸런스 시트 (씨앗 비용 vs 판매가, ROI 분석)~~ — DONE → `docs/balance/crop-economy.md` |
| ~~BAL-002~~ | ~~3~~ | ~~게임 진행 곡선 (레벨별 해금, 예상 플레이타임)~~ — DONE → `docs/balance/progression-curve.md`, `docs/systems/progression-architecture.md`, `docs/mcp/progression-tasks.md` |
| ~~CON-001~~ | ~~2~~ | ~~작물 콘텐츠 상세 (전체 작물 목록, 계절별 분류, 특수 작물)~~ — DONE → `docs/content/crops.md`, `docs/mcp/crop-content-tasks.md` |
| BAL-003 | 2 | 겨울 작물 3종 ROI/밸런스 분석 (겨울무/표고버섯/시금치 → crop-economy.md 추가) |
| ~~CON-002~~ | ~~2~~ | ~~시설 콘텐츠 상세 (건설 요건, 업그레이드 경로)~~ — DONE → `docs/content/facilities.md`, `docs/systems/facilities-architecture.md` |
| CON-003 | 2 | NPC/상점 콘텐츠 (상인 캐릭터, 대화, 상점 인벤토리) |
| VIS-001 | 2 | 비주얼 가이드 (로우폴리 스타일 참고자료, 색상 팔레트) |
| AUD-001 | 1 | 사운드 디자인 문서 (필요한 효과음/BGM 목록) |
| ~~DES-005~~ | ~~3~~ | ~~인벤토리/아이템 시스템 상세 설계 (아이템 분류, 스택, 인벤토리 UI 규칙)~~ — DONE → `docs/systems/inventory-system.md` |
| DES-006 | 2 | 튜토리얼/온보딩 시스템 설계 (첫 플레이 가이드, 단계별 안내) |
| ~~ARC-006~~ | ~~3~~ | ~~인벤토리 시스템 기술 아키텍처 (InventoryManager, ItemData SO)~~ — DONE → `docs/systems/inventory-architecture.md` |
| ~~FIX-001~~ | ~~4~~ | ~~[C-2] 툴바 슬롯 수 설계 결정~~ — DONE: 8칸 범용 툴바 채택, data-pipeline.md toolbarSlots 8개로 수정 |
| ~~FIX-002~~ | ~~3~~ | ~~[W-2] FertilizerData.MaxStackSize 값 통일~~ — DONE: maxStack=30으로 통일 (inventory-system.md canonical) |
| ~~FIX-003~~ | ~~3~~ | ~~[W-7] 창고 슬롯 세이브 구조~~ — DONE: BuildingSaveData.storageSlots[] 추가, ItemSlotSaveData 클래스 정의 |
| ~~PATTERN-001~~ | ~~-~~ | ~~[self-improve 전용] 신규 문서가 canonical 문서의 수치/이름을 복사해 독립 기재 → 불일치 반복 발생~~ — DONE → `.claude/rules/doc-standards.md` Canonical 데이터 매핑 섹션 추가 (`logs/reports/self_improve_20260406_v2.md`) |
| ~~PATTERN-002~~ | ~~-~~ | ~~[self-improve 전용] 동일 문서 내에서도 섹션 간 수치 불일치 발생~~ — DONE → `.claude/rules/workflow.md` Reviewer Checklist 항목 1~4 추가 (`logs/reports/self_improve_20260406_v2.md`) |
| ~~PATTERN-003~~ | ~~-~~ | ~~[self-improve 전용] enum/타입 확장 시 같은 문서 내 다른 섹션(pseudo-code, 예시 코드 등)이 업데이트되지 않음~~ — DONE → `.claude/rules/doc-standards.md` Consistency Rules에 enum 전수 업데이트 규칙 추가, `.claude/rules/workflow.md` Reviewer Checklist 항목 5 추가 (`logs/reports/self_improve_20260406_v2.md`) |
| ~~PATTERN-004~~ | ~~-~~ | ~~[self-improve 전용] 같은 문서 내 디자인 섹션(Part I)과 MCP 구현 섹션(Part II) 간 불일치 반복 발생 (Buildings 부모, 시작 시각, EconomyManager 배치, TestPlayer 누락 등 4건)~~ — DONE → `.claude/rules/workflow.md` Reviewer Checklist 항목 6 추가 (`logs/reports/self_improve_20260406_v2.md`) |
| ~~PATTERN-005~~ | ~~-~~ | ~~[self-improve 전용] 동일 문서 내 Part I JSON 예시와 Part II C# 클래스 간 필드 불일치 반복 발생 (ARC-004에서 5건: PlayerSaveData 에너지/도구인덱스, FarmTileSaveData soil/consecutive 필드, CropInstanceSaveData fertilizer 필드명, GameSaveData 누락 필드) → JSON 예시 작성 후 C# 클래스를 동기화하지 않거나 그 반대인 경우~~ — DONE → `.claude/rules/doc-standards.md` Consistency Rules 및 `.claude/rules/workflow.md` Reviewer Checklist 항목 9 추가 |
| ~~PATTERN-006~~ | ~~-~~ | ~~[self-improve 전용] MCP 태스크 문서(Part II 포함)가 canonical 수치를 직접 배열로 기재하고, canonical이 변경(BAL-002 조정)되어도 MCP 문서가 자동 갱신되지 않는 패턴 (BAL-002에서 3건: data-pipeline.md 2.6 XP 테이블, progression-architecture.md Part II Step 11, progression-tasks.md Step A-3) → MCP 태스크 내 배열 수치도 `→ see canonical` 참조로만 표기하거나, 값 기재 시 반드시 canonical과 동시 수정 체크리스트 추가 필요~~ — DONE → `.claude/rules/doc-standards.md` Consistency Rules 및 `.claude/rules/workflow.md` Reviewer Checklist 항목 10 추가 |
| PATTERN-007 | - | [self-improve 전용] SO 에셋 데이터 테이블(data-pipeline.md 섹션 2.4 등)의 tileSize/buildTimeDays 등 구조적 파라미터가 콘텐츠 문서(facilities.md 등)와 별도로 기재되어 3건 이상 불일치 발생 (온실 tileSize 4x4 vs 6x6, 가공소 2x2 vs 4x3, 창고 2x3 vs 3x2, 건설 기간 불일치) → SO 에셋 테이블의 tileSize/buildTimeDays 등 콘텐츠 정의 파라미터는 canonical 콘텐츠 문서를 참조만 하고 직접 기재 금지 규칙 추가 필요 |
| BAL-003 | 2 | 겨울 작물 3종 ROI/밸런스 분석 (겨울무/표고버섯/시금치 → crop-economy.md 추가) |
| BAL-004 | 2 | 가공품 ROI/밸런스 분석 (18종 레시피 수익성, 가공 vs 생작물 비교 → crop-economy.md 또는 신규 balance 문서) |
| ARC-007 | 2 | 시설 MCP 태스크 시퀀스 (facilities-architecture.md Part II를 독립 문서로 분리 → docs/mcp/facilities-tasks.md) |
| DES-007 | 2 | 도구 업그레이드 시스템 설계 (호미/물뿌리개/낫 각 3단계 업그레이드, 비용, 효과) |
| DES-008 | 1 | 세이브/로드 UX 설계 (자동저장 트리거, 수동 저장, 멀티슬롯 여부) |
