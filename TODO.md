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
| ~~CON-002~~ | ~~2~~ | ~~시설 콘텐츠 상세 (건설 요건, 업그레이드 경로)~~ — DONE → `docs/content/facilities.md`, `docs/systems/facilities-architecture.md` |
| ~~CON-003~~ | ~~2~~ | ~~NPC/상점 콘텐츠 (상인 캐릭터, 대화, 상점 인벤토리)~~ — DONE → `docs/content/npcs.md`, `docs/systems/npc-shop-architecture.md` (ARC-008) |
| ~~VIS-001~~ | ~~2~~ | ~~비주얼 가이드 (로우폴리 스타일 참고자료, 색상 팔레트)~~ — DONE → `docs/systems/visual-guide.md`, `docs/systems/visual-architecture.md` |
| ~~AUD-001~~ | ~~1~~ | ~~사운드 디자인 문서 (필요한 효과음/BGM 목록)~~ — DONE → `docs/systems/sound-design.md`, `docs/systems/sound-architecture.md` (BGM 11트랙, SFX 100+종, AudioMixer 구조, SoundManager/BGMScheduler/SoundEventBridge 아키텍처) |
| ~~DES-005~~ | ~~3~~ | ~~인벤토리/아이템 시스템 상세 설계 (아이템 분류, 스택, 인벤토리 UI 규칙)~~ — DONE → `docs/systems/inventory-system.md` |
| ~~DES-006~~ | ~~2~~ | ~~튜토리얼/온보딩 시스템 설계 (첫 플레이 가이드, 단계별 안내)~~ — DONE → `docs/systems/tutorial-system.md`, `docs/systems/tutorial-architecture.md` |
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
| ~~PATTERN-007~~ | ~~-~~ | ~~[self-improve 전용] SO 에셋 데이터 테이블(data-pipeline.md 섹션 2.4 등)의 tileSize/buildTimeDays 등 구조적 파라미터가 콘텐츠 문서(facilities.md 등)와 별도로 기재되어 3건 이상 불일치 발생 (온실 tileSize 4x4 vs 6x6, 가공소 2x2 vs 4x3, 창고 2x3 vs 3x2, 건설 기간 불일치) → SO 에셋 테이블의 tileSize/buildTimeDays 등 콘텐츠 정의 파라미터는 canonical 콘텐츠 문서를 참조만 하고 직접 기재 금지 규칙 추가 필요~~ — DONE → `.claude/rules/doc-standards.md` Consistency Rules 및 Canonical 매핑 + `.claude/rules/workflow.md` Reviewer Checklist 항목 11 추가 (`logs/reports/self_improve_PATTERN007.md`) |
| ~~BAL-003~~ | ~~2~~ | ~~겨울 작물 3종 ROI/밸런스 분석 (겨울무/표고버섯/시금치 → crop-economy.md 추가)~~ — DONE → `docs/balance/crop-economy.md` 섹션 4.3 추가, B-09~B-11 밸런스 이슈 식별 |
| ~~ARC-007~~ | ~~2~~ | ~~시설 MCP 태스크 시퀀스 (facilities-architecture.md Part II를 독립 문서로 분리 → docs/mcp/facilities-tasks.md)~~ — DONE → `docs/mcp/facilities-tasks.md` |
| ~~DES-007~~ | ~~2~~ | ~~도구 업그레이드 시스템 설계 (호미/물뿌리개/낫 각 3단계 업그레이드, 비용, 효과)~~ — DONE -> `docs/systems/tool-upgrade.md` |
| ~~DES-008~~ | ~~1~~ | ~~세이브/로드 UX 설계 (자동저장 트리거, 수동 저장, 멀티슬롯 여부)~~ — DONE → `docs/systems/save-load-system.md` |
| ~~FIX-004~~ | ~~3~~ | ~~data-pipeline.md 섹션 2.4 시설 에셋 테이블의 tileSize/buildTimeDays 직접 수치를 `(→ see docs/content/facilities.md)` 참조로 교체 (PATTERN-007 후속)~~ — DONE |
| ~~ARC-008~~ | ~~2~~ | ~~도구 업그레이드 MCP 태스크 시퀀스 (tool-upgrade-architecture.md Part II를 독립 문서로 분리 → docs/mcp/tool-upgrade-tasks.md)~~ — DONE → `docs/mcp/tool-upgrade-tasks.md` (문서 ID ARC-015로 할당, ARC-008은 npc-shop-architecture.md에 이미 사용됨) |
| ~~CON-004~~ | ~~2~~ | ~~대장간 NPC 상세 (캐릭터, 대화, 업그레이드 인터페이스 UX)~~ — DONE → `docs/content/blacksmith-npc.md`, `docs/systems/blacksmith-architecture.md` (ARC-020) |
| ~~ARC-009~~ | ~~2~~ | ~~NPC/상점 MCP 태스크 시퀀스 독립 문서화 (npc-shop-architecture.md Part II 요약 → docs/mcp/npc-shop-tasks.md 상세 분리)~~ — DONE → `docs/mcp/npc-shop-tasks.md` |
| ~~BAL-005~~ | ~~1~~ | ~~여행 상인 희귀 아이템 가격 밸런스 분석 (npcs.md [OPEN] 후속 — 만능 비료/겨울 씨앗 등 8종 ROI 검증)~~ — DONE → `docs/balance/traveler-economy.md` (8종 ROI 분석, 만능 비료 150→80G, 성장 촉진제 250→150G+2일, 행운의 부적 400→250G+15%) |
| ~~ARC-010~~ | ~~2~~ | ~~튜토리얼 MCP 태스크 시퀀스 독립 문서화 (tutorial-architecture.md Part II → docs/mcp/tutorial-tasks.md)~~ — DONE → `docs/mcp/tutorial-tasks.md` (중복 항목 정리) |
| ~~ARC-011~~ | ~~2~~ | ~~세이브/로드 시스템 기술 아키텍처 (SaveManager, GameSaveData 통합 구조, 자동저장 트리거 설계)~~ — DONE → `docs/systems/save-load-architecture.md` |
| ~~ARC-012~~ | ~~2~~ | ~~세이브/로드 MCP 태스크 시퀀스 독립 문서화 (save-load-architecture.md Part II → docs/mcp/save-load-tasks.md)~~ — DONE → `docs/mcp/save-load-tasks.md` (중복 항목 정리) |
| ~~CON-005~~ | ~~2~~ | ~~가공/요리 시스템 콘텐츠 상세 (가공소별 레시피 전체 목록, 처리 시간, 연료 소모 규칙)~~ — DONE -> `docs/content/processing-system.md` |
| ~~DES-009~~ | ~~2~~ | ~~퀘스트/미션 시스템 설계 (계절별 목표, NPC 의뢰, 달성 보상 구조)~~ — DONE → `docs/systems/quest-system.md`, `docs/systems/quest-architecture.md` |
| ~~ARC-014~~ | ~~2~~ | ~~가공 시스템 MCP 태스크 시퀀스 독립 문서화 (processing-architecture.md Part II → docs/mcp/processing-tasks.md)~~ — DONE → `docs/mcp/processing-tasks.md` (ID: ARC-014) |
| ~~BAL-004~~ | ~~2~~ | ~~가공품 ROI/밸런스 분석 (32종 레시피 수익성, 가공 체인 ROI, 가공 vs 생작물 비교 → docs/balance/processing-economy.md)~~ — DONE → `docs/balance/processing-economy.md` |
| ~~CON-006~~ | ~~1~~ | ~~목축/낙농 시스템 콘텐츠 상세 (치즈 공방 활성화 선행 조건 — 젖소/염소 등 동물 관리 메카닉)~~ — DONE → `docs/content/livestock-system.md` |
| ~~FIX-007~~ | ~~3~~ | ~~tool-upgrade-tasks.md ToolSpecialEffect enum 확장 방안 확정 (전설 낫 다중 효과 처리 — Flags enum vs string[] vs tier 분기 결정, tool-upgrade-architecture.md 반영)~~ — DONE: [System.Flags] 비트마스크 채택, SeedRecovery 추가, ToolData.specialEffect 타입 string→ToolSpecialEffect 변경 |
| ~~ARC-013~~ | ~~2~~ | ~~인벤토리 MCP 태스크 시퀀스 독립 문서화 (inventory-architecture.md Part II → docs/mcp/inventory-tasks.md)~~ — DONE → `docs/mcp/inventory-tasks.md` |
| ~~ARC-016~~ | ~~2~~ | ~~퀘스트 시스템 MCP 태스크 시퀀스 독립 문서화 (quest-architecture.md Part II → docs/mcp/quest-tasks.md)~~ — DONE → `docs/mcp/quest-tasks.md` |
| ~~DES-010~~ | ~~2~~ | ~~도전 과제/업적 시스템 설계 (작물 마스터, 수익 달성, 시설 완성 등 업적 구조 + UI 표시 방식)~~ — DONE → `docs/systems/achievement-system.md`, `docs/systems/achievement-architecture.md` |
| ~~ARC-017~~ | ~~2~~ | ~~업적 시스템 MCP 태스크 시퀀스 독립 문서화 (achievement-architecture.md Part II → docs/mcp/achievement-tasks.md)~~ — DONE → `docs/mcp/achievement-tasks.md` |
| ~~CON-007~~ | ~~2~~ | ~~업적 콘텐츠 상세 (전체 업적 30종 목록, 단계별 목표 수치, 보상 테이블 → docs/content/achievements.md)~~ — DONE → `docs/content/achievements.md` |
| ~~BAL-006~~ | ~~2~~ | ~~퀘스트/미션 보상 밸런스 분석 (계절 목표 골드/경험치 보상 ROI 검증 → docs/balance/quest-rewards.md)~~ — DONE → `docs/balance/quest-rewards.md` (XP 인플레이션 198.5%, 골드 149.4% 초과 확인, 제안 A+C 권장) |
| ~~BAL-007~~ | ~~3~~ | ~~XP 통합 재조정 — 수확/경작 + 퀘스트 + 업적 XP 합산 시뮬레이션, progression-curve.md XP 테이블 재검토 (BAL-006 후속)~~ — DONE → `docs/balance/xp-integration.md` (제안 A': 퀘스트 900 XP, 업적 유지, 1년차 일반 레벨 8 목표 확정) + `docs/balance/bal-007-architecture-analysis.md` |
| ~~FIX-012~~ | ~~3~~ | ~~quest-system.md 섹션 3~6 퀘스트 XP 수치 재확정 (BAL-007 제안 A' 적용 — 카테고리 총량: 메인 280, NPC 140, 일일 280, 도전 200, 합계 900 XP)~~ — DONE: quest-system.md 섹션 3~6 모든 XP 수치 BAL-007 A' 기준으로 확정됨 |
| ~~FIX-013~~ | ~~3~~ | ~~progression-curve.md 섹션 1.2에 퀘스트/업적 XP 배분 카테고리 추가 (수확 55%, 경작 15%, 시설 12%, 가공 3%, 퀘스트 10%, 업적 5%)~~ — DONE: progression-curve.md 섹션 1.2 배분 요약표 추가됨 |
| ~~FIX-014~~ | ~~2~~ | ~~progression-curve.md 섹션 1.3.1에 [DEPRECATED] 태그 추가, 섹션 2.4.1을 canonical XP 테이블로 확정 명시~~ — DONE: [DEPRECATED] 태그 및 참조 추가됨 |
| ~~FIX-015~~ | ~~2~~ | ~~progression-curve.md 섹션 2.4에 퀘스트/업적 XP 포함 통합 시뮬레이션 섹션 추가 (BAL-007 섹션 3.2 시나리오 A' 기준)~~ — DONE: 섹션 2.4.4 통합 시뮬레이션 추가됨 |
| ~~FIX-016~~ | ~~2~~ | ~~quest-rewards.md (BAL-006) 섹션 3.1~3.2 기준 XP 4,609→9,029 정정 및 비율 재계산~~ — DONE: 섹션 3.1 기준 XP 9,029로 정정, [수정-BAL-007] 태그 추가됨 |
| ~~FIX-017~~ | ~~2~~ | ~~achievements.md 섹션 2.4 XP 비율 재계산 (4,609 기준 49% → 9,029 기준 24.9%로 정정) + [RISK] 태그 완화~~ — DONE: 24.9%로 정정, [NOTE]로 변경됨 |
| ~~FIX-018~~ | ~~1~~ | ~~quest-system.md 섹션 7.3 XP 예산 목표 비율을 "10%(~900 XP)"로 확정 업데이트~~ — DONE: 섹션 7.3 확정됨 |
| ~~FIX-019~~ | ~~2~~ | ~~progression-architecture.md 섹션 2.2 XPSource enum에 QuestComplete/AchievementReward 추가~~ — DONE: 추가됨 |
| ~~FIX-020~~ | ~~2~~ | ~~progression-architecture.md 섹션 2.3 GetExpForSource() switch문에 QuestComplete/AchievementReward case 추가~~ — DONE: 추가됨 |
| ~~FIX-021~~ | ~~1~~ | ~~progression-architecture.md 섹션 1 클래스 다이어그램에 QuestEvents/AchievementEvents 구독 명시~~ — DONE: 추가됨 |
| ~~FIX-022~~ | ~~1~~ | ~~quest-architecture.md GrantXP 메서드를 AddExp(amount, XPSource.QuestComplete) 호출로 명시~~ — DONE: 추가됨 |
| ~~FIX-023~~ | ~~1~~ | ~~achievement-architecture.md GrantReward 흐름을 ProgressionManager.AddExp(xp, XPSource.AchievementReward) 호출로 명시~~ — DONE: 추가됨 |
| ~~REV-001~~ | ~~3~~ | ~~quest-rewards.md 섹션 2.4 농장 도전 XP 집계를 BAL-007 A' 적용 후 값으로 재계산 (현행 ~4,400 XP 대폭 삭감 필요)~~ — DONE: 대형 580XP+소형 401XP=981XP, 1년차 실현 200XP 명시 |
| ~~REV-002~~ | ~~2~~ | ~~quest-rewards.md 섹션 2.5 전체 퀘스트 보상 총계 XP 재산정 (현행 9,147 XP → BAL-007 A' 기준 ~900 XP 반영)~~ — DONE: 총계 1,681XP, 1년차 실현 900XP로 업데이트 |
| ~~REV-003~~ | ~~2~~ | ~~quest-rewards.md 섹션 6.2 제안 A 설명을 A'(900 XP) 기준으로 업데이트 (현행 692 XP 기준)~~ — DONE: 제안 A' 확정 표로 업데이트, 6.3 권장 조합도 반영 |
| ~~FIX-010~~ | ~~2~~ | ~~save-load-architecture.md GameSaveData 루트에 `achievements: AchievementSaveData` 필드 추가 (ARC-017 리뷰 후속 — PATTERN-005)~~ — DONE: 트리/JSON/C# 3곳 업데이트, 필드 수 16→17, SaveLoadOrder 90 할당표 추가 |
| ~~FIX-011~~ | ~~2~~ | ~~achievement-architecture.md AchievementConditionType에 PurchaseCount 전용 타입 추가 검토 (Explorer 업적 ach_explorer_02/04 conditionType 불일치 해소 — ARC-017 리뷰 [OPEN])~~ — DONE: PurchaseCount=14 추가, EconomyEvents.OnShopPurchased 이벤트 구독 추가, achievements.md conditionType 수정 |
| ~~FIX-005~~ | ~~2~~ | ~~facilities.md에 특화 가공소 3종(제분소·발효실·베이커리) 건설 요건·업그레이드 경로 추가 (design.md 4.6 반영 후속)~~ — DONE → `docs/content/facilities.md` 섹션 7~9 |
| ~~PATTERN-008~~ | ~~-~~ | ~~[self-improve 전용] 특화 가공소 레시피 목록을 비-canonical 문서(facilities.md 섹션 7~9)에 직접 기재하여 canonical(processing-system.md)과 불일치 3건 발생 (제분소/발효실/베이커리) → 가공소 레시피 목록을 직접 기재하는 패턴 방지 규칙 추가 필요~~ — DONE → `.claude/rules/doc-standards.md` PATTERN-008 규칙 + `.claude/rules/workflow.md` Reviewer Checklist 항목 12 추가 (`logs/reports/self_improve_PATTERN008.md`) |
| ~~FIX-006~~ | ~~3~~ | ~~facilities-architecture.md ProcessingType enum에 Mill/Fermentation/Bake 추가 (W-3 후속)~~ — DONE |
| ~~FIX-008~~ | ~~2~~ | ~~inventory-system.md 섹션 5.3·7 도구 ID 표기 통일 (tool_hoe → hoe_basic 형식, 등급 포함 규칙 수정 — ARC-013 리뷰 W-5 후속)~~ — DONE |
| ~~FIX-009~~ | ~~2~~ | ~~data-pipeline.md 섹션 3.2에 ShippingBinSaveData 스키마 정의 추가 (ARC-013 리뷰 GAP-05 후속)~~ — DONE |
| ~~DES-011~~ | ~~2~~ | ~~UI/UX 시스템 상세 설계 (HUD 구조, 인벤토리 UI 레이아웃, 퀘스트 로그 UI, 알림 시스템)~~ — DONE → `docs/systems/ui-system.md` |
| ~~ARC-018~~ | ~~2~~ | ~~UI 시스템 기술 아키텍처 (UIManager, Screen 전환 FSM, 팝업 큐 시스템, CanvasGroup 구조)~~ — DONE → `docs/systems/ui-architecture.md` |
| ~~FIX-024~~ | ~~3~~ | ~~achievement-system.md 섹션 5.4 업적 토스트 위치 "좌측 하단" → "상단 중앙"으로 수정~~ — DONE |
| ~~ARC-010~~ | ~~2~~ | ~~튜토리얼 MCP 태스크 시퀀스 독립 문서화 (tutorial-architecture.md Part II → docs/mcp/tutorial-tasks.md)~~ — DONE → `docs/mcp/tutorial-tasks.md` |
| ~~ARC-012~~ | ~~2~~ | ~~세이브/로드 MCP 태스크 시퀀스 독립 문서화 (save-load-architecture.md Part II → docs/mcp/save-load-tasks.md)~~ — DONE → `docs/mcp/save-load-tasks.md` |
| ~~BAL-003-dup~~ | ~~2~~ | ~~겨울 작물 3종 ROI/밸런스 분석 중복 항목~~ — REMOVED (line 34에 동일 항목 존재) |
| ~~ARC-019~~ | ~~1~~ | ~~목축/낙농 시스템 기술 아키텍처 (AnimalManager, AnimalData SO, 돌봄 사이클 설계 — CON-006 후속)~~ — DONE → `docs/systems/livestock-architecture.md` |
| ~~BAL-008~~ | ~~1~~ | ~~목축/낙농 경제 밸런스 분석 (동물 구매/사료 비용 vs 젖/양모/알 판매 ROI — CON-006 + ARC-019 후속)~~ — DONE (중복 항목 — 섹션 119 참조) |
| ~~ARC-022~~ | ~~2~~ | ~~UI 시스템 MCP 태스크 시퀀스 독립 문서화 (ui-architecture.md Part II → docs/mcp/ui-tasks.md)~~ — DONE → `docs/mcp/ui-tasks.md` |
| ~~DES-012~~ | ~~2~~ | ~~농장 확장 시스템 설계 (구역 해금, 타일 구매, 신규 땅 개간 메카닉 — ARC-023 선행 요건)~~ — DONE → `docs/systems/farm-expansion.md` (7구역 Zone A~G, 576타일, 총 16,000G 해금 비용) |
| ~~CON-008~~ | ~~1~~ | ~~추가 NPC 상세 설계 (마을 상인/농업 전문가 등 blacksmith 외 NPC 대화 및 서비스 내용 상세화)~~ — DONE → `docs/content/merchant-npc.md`, `docs/content/carpenter-npc.md`, `docs/content/traveler-npc.md`, `docs/systems/npc-shop-architecture.md` 섹션 9~13 (Part III) |
| ~~FIX-026~~ | ~~2~~ | ~~time-season-tasks.md 작성 예정 항목 우선순위 확인 (time-season-architecture.md Cross-references에 '작성 예정'으로 등재됨)~~ — DONE → `docs/mcp/time-season-tasks.md` (ARC-021) 신규 생성, Cross-references 업데이트 |
| ~~ARC-020~~ | ~~2~~ | ~~대장간 NPC MCP 태스크 시퀀스 독립 문서화 (blacksmith-architecture.md Part II → docs/mcp/blacksmith-tasks.md)~~ — DONE → `docs/mcp/blacksmith-tasks.md` |
| ~~FIX-027~~ | ~~2~~ | ~~blacksmith-architecture.md 섹션 1 NPCAffinityTracker 클래스 다이어그램에 메서드 4개 추가 (HasTriggeredDialogue, MarkDialogueTriggered, CanGiveDailyAffinity, MarkDailyVisit — ARC-020 리뷰 INFO-1 후속)~~ — DONE: 메서드 4개 추가, AffinityEntry.triggeredDialogues bool[] → string[] triggeredDialogueIds 타입 수정 |
| ~~FIX-028~~ | ~~2~~ | ~~blacksmith-architecture.md Part II Step A-4에 추가된 4개 메서드(HasTriggeredDialogue 등)의 AffinitySaveData 필드(triggeredDialogueIds, dailyVisitDates) 스키마 확장 검토 (save-load-architecture.md AffinityEntry 동기화 여부 — CRITICAL-5 후속)~~ — DONE: blacksmith-tasks.md AffinityEntry/NPCAffinityTracker 동기화, _lastVisitDayMap/_triggeredDialogueMap 다이어그램 추가, dailyVisitDates 불필요 결정 명시 |
| ~~BAL-009~~ | ~~1~~ | ~~도구 업그레이드 XP 밸런스 분석 (ToolUpgrade XPSource 추가 후 — progression-curve.md toolUpgradeExp 수치 결정)~~ — DONE → `docs/balance/tool-upgrade-xp.md` (90 XP 확정: 15 XP × 6회, progression-curve.md 기존값 일치 확인) |
| ~~BAL-010~~ | ~~2~~ | ~~겨울 전용 작물 온실 경쟁력 조정 (B-09 후속 — 제안 E 비주 계절 온실 페널티 또는 겨울 전용 작물 시너지 보너스 설계 확정, crop-growth.md/economy-system.md 반영)~~ — DONE → `docs/balance/crop-economy.md` 섹션 4.3.10 (제안 E x0.8 + 시너지 x1.2 + 제안 F + B-11 확정) |
| ~~FIX-029~~ | ~~3~~ | ~~crop-growth.md 섹션 3.3 온실 규칙 테이블에 "비주 계절 판매가 x0.8" 및 "겨울 전용 작물 시너지 판매가 x1.2" 행 추가 (BAL-010 후속)~~ — DONE: crop-growth.md 섹션 3.3 두 행 추가 + 트레이드오프 업데이트 + 섹션 4.2 표고버섯 재수확 5일 + 섹션 8 파라미터 추가 |
| ~~FIX-030~~ | ~~3~~ | ~~economy-system.md 판매가 계산 공식에 온실 비주 계절 페널티(x0.8) 및 겨울 전용 시너지(x1.2) 배수 반영 (BAL-010 후속)~~ — DONE: economy-system.md 섹션 2.6.5~2.6.6 확정 반영 (리뷰어 수정) |
| ~~FIX-031~~ | ~~2~~ | ~~crops.md 섹션 3.10 표고버섯 재수확 간격 4일 -> 5일 변경 (BAL-010 제안 F 확정)~~ — DONE: crops.md 섹션 3.10 + 4.1 + 6 모두 5일로 업데이트, ROI 재계산 |
| ~~FIX-032~~ | ~~2~~ | ~~crops.md 섹션 3.9 겨울무 씨앗 가격 20G -> 23G 변경 (BAL-010 B-11 확정)~~ — DONE: crops.md 섹션 3.9 + 6 업데이트, ROI 재계산 |
| ~~FIX-033~~ | ~~2~~ | ~~design.md 섹션 4.2 겨울 작물 canonical 테이블에 표고버섯 재수확 5일, 겨울무 씨앗 23G 반영 (BAL-010 후속)~~ — DONE: design.md는 "→ see crops.md 섹션 3.9~3.11" 참조 방식으로 유지 (canonical 규칙 준수). crops.md 섹션 6 요약표 및 주석 업데이트. crop-economy.md CRITICAL/WARNING 수정 포함. |
| ~~FIX-025~~ | ~~2~~ | ~~tutorial-tasks.md Step 07/11 이벤트명 확정 (TimeEvents.OnSleepExecuted, 구매 완료 이벤트 — ARC-010 리뷰 WARNING 후속)~~ — DONE: S07→TimeManager.OnSleepCompleted, S11→EconomyEvents.OnShopPurchased 확정; economy-architecture.md 이벤트 표 보완 |
| ~~PATTERN-009~~ | ~~-~~ | ~~[self-improve 전용] 밸런스 문서의 히스토리 배너 의무 표기 규칙~~ — DONE → doc-standards.md PATTERN-009 규칙 추가, workflow.md Reviewer Checklist 항목 13 추가 |
| ~~FIX-034~~ | ~~2~~ | ~~economy-architecture.md HarvestOrigin enum 설계 (온실 수확물 출처 추적 — BAL-010 아키텍트 분석 [RISK] 후속, devlog 043 언급)~~ — DONE → economy-architecture.md 섹션 3.10 추가, inventory-architecture.md ItemSlot/AddItem/RemoveItem 업데이트, data-pipeline.md ItemSlotSaveData origin 필드 추가 |
| ~~ARC-023~~ | ~~2~~ | ~~농장 확장 시스템 기술 아키텍처 (FarmZoneManager, ZoneData SO, 타일 구매 흐름 — DES-012 후속)~~ — DONE → `docs/systems/farm-expansion-architecture.md` |
| ~~ARC-024~~ | ~~1~~ | ~~목축/낙농 시스템 MCP 태스크 시퀀스 (AnimalManager/AnimalData 구현 시퀀스 독립 문서화 — ARC-019 후속)~~ — DONE → `docs/mcp/livestock-tasks.md` (9개 태스크 그룹, ~221회 MCP 호출) |
| ~~FIX-035~~ | ~~2~~ | ~~progression-curve.md 섹션 1.2.4 농장 확장 XP를 "4단계 100XP" → "6단계 150XP"로 업데이트 (DES-012에서 Zone B~G 6단계로 확정 — farm-expansion.md [OPEN] 8 후속)~~ — DONE: 섹션 1.2.4 6단계 150XP, 보조 마일스톤 "6단계 Zone G" 수정 |
| ~~FIX-036~~ | ~~2~~ | ~~economy-system.md 섹션 3.3 목공소 인벤토리를 Zone B~G 7구역 방식으로 업데이트 (DES-012 섹션 2.1 해금 조건 테이블 반영 — farming-system.md 섹션 1 확장 방식 동기화 포함)~~ — DONE: economy-system.md 섹션 3.3/5.2, farming-system.md 섹션 1 Zone 참조 교체 |
| ~~FIX-037~~ | ~~1~~ | ~~farm-expansion.md 섹션 4.4 과일나무 데이터(묘목 가격, 수확량, 판매가)를 crops.md로 이전하고 참조 표기로 교체 (WARNING-8: 비-canonical 문서에 작물 가격 직접 기재)~~ — DONE → crops.md 섹션 6 (과일나무 canonical 섹션 신규), farm-expansion.md 섹션 4.4 참조 표기로 교체 |
| ~~PATTERN-010~~ | ~~-~~ | ~~[self-improve 전용] 아키텍처 문서 플레이스홀더 수치/ID 금지 규칙~~ — DONE → doc-standards.md PATTERN-010 규칙 추가, workflow.md Agent Collaboration 및 Reviewer Checklist 항목 14 추가 |
| ~~FIX-038~~ | ~~3~~ | ~~AnimalManager는 _barnLevel 단일 필드로 외양간(Barn)만 추적하고 닭장(Chicken Coop)의 레벨/수용 수를 별도 관리하지 않음 — _coopLevel, _coopCapacity 필드 추가 또는 시설별 레벨 딕셔너리 방식으로 설계 확장 필요 (ARC-019 섹션 1/6 수정)~~ — DONE → livestock-architecture.md 섹션 1/5.2/6/8 전수 업데이트, coopLevel/coopCapacity 분리 |
| ~~FIX-039~~ | ~~2~~ | ~~LivestockConfig SO에 goldQualityThreshold, silverQualityThreshold 필드 추가 필요 (ARC-019 섹션 5.2 — GetProductQuality에서 참조하는 임계값이 SO에 정의되어야 함)~~ — DONE → livestock-architecture.md 섹션 5.2 품질 임계값 필드 추가, livestock-system.md 섹션 5.3 canonical 수치 정의 |
| ~~FIX-040~~ | ~~2~~ | ~~CON-006 섹션 7.3의 XPSource enum 제안이 ARC-019 섹션 7.1과 완전 중복 기재됨 — CON-006 섹션 7.3에서 enum 전체 기재를 제거하고 ARC-019 참조로 교체 (PATTERN-001 위반: 비-canonical 문서에 enum 직접 기재)~~ — DONE → livestock-system.md 섹션 7.3 enum 제거, ARC-019 참조로 교체 |
| ~~FIX-041~~ | ~~1~~ | ~~design.md 섹션 4.6 시설 목록에 외양간(Barn)/닭장(Chicken Coop)/치즈 공방(Cheese Workshop) 추가 필요 (CON-006 섹션 10 [RISK]-2 후속)~~ — DONE → design.md 섹션 4.6 3개 시설 추가 (canonical 참조 방식) |
| ~~FIX-042~~ | ~~2~~ | ~~progression-curve.md 섹션 2.2 상단에 "[DEPRECATED 시나리오]" 배너 추가 (물주기 XP=0 조정 이전 수치 혼재 방지 — ARC-024 리뷰 WARNING-1 후속)~~ — DONE |
| ~~FIX-043~~ | ~~1~~ | ~~livestock-architecture.md AnimalManager 메서드 목록에 `ClearAllAnimals()` 테스트 전용 메서드 추가 (#if UNITY_EDITOR 블록 — ARC-024 리뷰 I-2 후속)~~ — DONE |
| ~~BAL-011~~ | ~~1~~ | ~~목축 XP canonical 등록 (livestock-system.md 섹션 7.3 제안값을 progression-curve.md에 공식 포함 — devlog 048 INFO-1 후속)~~ — DONE → progression-curve.md 섹션 1.2.6 신규 추가 |
| ~~BAL-008~~ | ~~1~~ | ~~목축/낙농 경제 밸런스 분석 (동물 구매/사료 비용 vs 젖/양모/알 판매 ROI — CON-006 + ARC-019 후속)~~ — DONE → `docs/balance/livestock-economy.md` (410줄, 동물별 ROI/가공 체인/초기 투자 분석) |
| ~~ARC-025~~ | ~~1~~ | ~~농장 확장(Zone) 시스템 MCP 태스크 시퀀스 문서화 (FarmZoneManager, ZoneData SO 구현 시퀀스 — ARC-023 후속)~~ — DONE → `docs/mcp/farm-expansion-tasks.md` (9개 태스크 그룹, ~99회 MCP 호출) |
| ~~FIX-056~~ | ~~3~~ | ~~farm-expansion-architecture.md 섹션 5.2 도구-장애물 매핑 표에서 SmallRock/LargeRock 간 등급 기준(Basic vs Reinforced)을 farm-expansion.md 섹션 3.1과 동기화 확인 후 canonical 등록~~ — DONE: HP 수치 canonical 확인(DES-012 섹션 3.1), 아키텍처 참조 동기화, FIX-068과 통합 처리 |
| ~~DES-013~~ | ~~1~~ | ~~낚시 시스템 설계 (Zone F 연못 구역 활용 — 낚시 메카닉, 어종 목록, 계절별 분포)~~ — DONE → `docs/systems/fishing-system.md` |
| ~~CON-009~~ | ~~2~~ | ~~치즈 공방 레시피 정의 (processing-system.md에 치즈 공방 레시피 4~6종 확정 추가 — BAL-008 GAP-1 후속, CON-006 완료로 선행 조건 해소됨)~~ — DONE → `docs/content/processing-system.md` 섹션 3.6 치즈 공방 레시피 5종(치즈/염소치즈/버터/에이지드치즈/크림) 추가, ProcessingType.Cheese enum 추가 |
| ~~FIX-044~~ | ~~1~~ | ~~economy-architecture.md에 동물 생산물 수급 변동 적용 정책 명시 (작물과 동일 풀 vs 별도 카테고리 결정 — BAL-008 GAP-3 후속)~~ — DONE → SupplyCategory enum(Crop/AnimalProduct/Fish/ProcessedGoods), 카테고리별 수급 파라미터, 섹션 3.11 정책 문서 추가 |
| ~~ARC-026~~ | ~~3~~ | ~~fishing-architecture.md 문서 ID 등록 — 낚시 시스템 기술 아키텍처 (DES-013 후속, ARC-026으로 확정)~~ — DONE → `docs/systems/fishing-architecture.md` (devlog 051에서 생성 완료) |
| ~~FIX-049~~ | ~~3~~ | ~~economy-architecture.md 섹션 3.10.2 HarvestOrigin에 `Fishing = 3` 추가, 섹션 3.10.3 switch 문에 Fishing case 추가 (ARC-026 후속)~~ — DONE |
| ~~FIX-050~~ | ~~3~~ | ~~progression-architecture.md 섹션 2.2 XPSource에 `FishingCatch` 추가, 섹션 2.3 GetExpForSource() switch 문에 FishingCatch case 추가 (ARC-026 후속)~~ — DONE |
| ~~FIX-051~~ | ~~3~~ | ~~data-pipeline.md Part I 섹션 3.2 JSON 스키마 및 Part II GameSaveData C# 클래스에 `fishing` 필드 추가 (ARC-026 후속)~~ — DONE |
| ~~FIX-052~~ | ~~2~~ | ~~save-load-architecture.md 섹션 7 SaveLoadOrder 할당표에 `FishingManager \| 52` 행 추가 (ARC-026 후속)~~ — DONE |
| ~~FIX-053~~ | ~~3~~ | ~~data-pipeline.md Part I ItemType enum에 `Fish` 값 추가 (ARC-026 후속)~~ — DONE: data-pipeline.md, inventory-architecture.md 섹션 3.2, inventory-tasks.md Step 1-01에 동시 반영 |
| ~~FIX-054~~ | ~~2~~ | ~~processing-system.md에 생선 가공 레시피 섹션 추가 후 fishing-system.md 섹션 6.2 테이블 제거 및 참조 교체 (PATTERN-008 이전 — DES-013 후속)~~ — DONE: processing-system.md 섹션 3.5 생선 가공 5종 canonical 추가, fishing-system.md 섹션 6.2 참조로 교체 완료 |
| ~~FIX-055~~ | ~~5~~ | ~~낚시 미니게임 방식 결정 및 통일: fishing-system.md 섹션 3(Oscillating Bar 가로 커서)과 fishing-architecture.md 섹션 2(ExcitementGauge 세로 게이지)가 불일치. 한 방식으로 확정 후 두 문서를 동시 수정 (CRITICAL — ARC-026 후속)~~ — DONE: ExcitementGauge 방식으로 통일, fishing-system.md 섹션 3 전면 재작성 (FishData SO targetZoneWidthMul/moveSpeed 필드 정합) |
| ~~FIX-057~~ | ~~3~~ | ~~npcs.md 섹션 9.3 `luckyCharmIridiumBonus` 조정 범위 0.03~0.10 → 0.03~0.20 확장 (BAL-005 섹션 8 Open Question 6 후속 — 권장 값 0.15 수용 위해 상한 확장 필요)~~ — DONE: 섹션 9.3 조정 범위 0.03~0.20으로 확장 |
| ~~FIX-058~~ | ~~2~~ | ~~BAL-005 확정 후 npcs.md 섹션 6.3·9.2·9.3 가격/파라미터 반영: universalFertPrice 150→80G, offSeasonSeedPriceMult 2.0→1.5, growthAccelDays 1→2, luckyCharmIridiumBonus 0.05→0.15, 성장 촉진제 가격 250G→150G, 행운의 부적 400G→250G (BAL-005 권장 조정 — [OPEN] 확정 대기)~~ — DONE: 섹션 6.3/6.4/9.1/9.2/9.3 전면 반영, canonical 참조 추가 |
| ~~FIX-059~~ | ~~3~~ | ~~data-pipeline.md Part I ItemType enum에 `Consumable` 값 추가 — 여행 상인 소비형 아이템(에너지 토닉, 성장 촉진제, 행운의 부적)을 Special과 구분하기 위해 (BAL-005 아키텍처 분석 후속)~~ — DONE: ItemType enum 테이블에 Consumable 추가, ItemSlotSaveData 주석 업데이트 |
| ~~FIX-060~~ | ~~3~~ | ~~economy-architecture.md PriceCategory enum에 `Consumable`, `Decoration` 값 추가 후 관련 switch 문 업데이트 (BAL-005 아키텍처 분석 후속)~~ — DONE: PriceCategory enum에 Consumable/Decoration 추가 (switch 문 없음) |
| ~~FIX-061~~ | ~~2~~ | ~~npc-shop-architecture.md 섹션 7.2의 구버전 TravelingMerchantSaveData(4필드)를 섹션 9.4 확장판(7필드)으로 대체 또는 명시적 deprecated 표기 (BAL-005 아키텍처 분석 후속)~~ — DONE: 섹션 7.1/7.2에 [DEPRECATED] 배너 추가, 섹션 9.4(7필드) 참조 명시 |
| ~~FIX-062~~ | ~~3~~ | ~~npc-shop-architecture.md 섹션 3.5 TravelingMerchantScheduler 클래스 다이어그램에 `_affinityPoints`, `GetAffinityLevel()`, `ApplyAffinityBonus()` 추가 (BAL-005 아키텍처 분석 후속)~~ — DONE: _affinityPoints 필드 + 메서드 2개 + OnAffinityChanged 이벤트 구독 추가 |
| ~~FIX-063~~ | ~~2~~ | ~~inventory-architecture.md 섹션 4에 FishData의 IInventoryItem 구현 예시 추가 (ItemType 프로퍼티 `Fish` 반환 — ARC-026 후속, 리뷰어 INFO-1)~~ — DONE: 섹션 4.4 신규 추가 (MaxStackSize canonical 참조 data-pipeline.md 섹션 2.7), 리뷰어 WARNING 수정 포함 |
| ~~FIX-064~~ | ~~2~~ | ~~fishing-architecture.md 섹션 5.2 또는 balance 문서에 낚시 XP 계산 공식 확정 및 canonical 등록 (rarity 기반 vs basePrice 기반 — ARC-026 후속, 리뷰어 INFO-2)~~ — DONE: progression-curve.md 섹션 1.2.7 (Common=10/Uncommon=20/Rare=40/Legendary=80), fishing-architecture.md 섹션 6.2 CalculateFishingExp() 정의, [OPEN] 해소 |
| ~~FIX-067~~ | ~~3~~ | ~~tool-upgrade.md 섹션 6.1 대장간 영업시간 09:00~18:00 → 10:00~16:00 수정 (economy-system.md 섹션 3.2가 canonical — npcs.md [OPEN] 6 후속)~~ — DONE: tool-upgrade.md는 이미 canonical 참조로 교체됨 확인; npcs.md [OPEN]-6 및 [RISK]-5 RESOLVED 처리 |
| ~~DES-014~~ | ~~2~~ | ~~겨울 온실 전용 씨앗 판매 경로 확정 (여행 상인 독점 vs 잡화 상점 겨울 판매 — npcs.md [OPEN] 1 후속, crops.md 섹션 3.9~3.11 반영)~~ — DONE: 옵션 C(혼합) 확정. 여행 상인(겨울 Day 1~, x1.5), 잡화 상점(겨울 Day 8~, 정가, 온실 보유). npcs.md 섹션 3.3/6.3, crops.md 섹션 4.4, traveler-economy.md 섹션 3.8/6.1, economy-system.md 섹션 3.3, npc-shop-architecture.md 섹션 15, facilities.md 섹션 4.4 반영 완료 |
| ~~ARC-028~~ | ~~2~~ | ~~낚시 시스템 MCP 태스크 시퀀스 문서화 (fishing-architecture.md Part II → docs/mcp/fishing-tasks.md — ARC-026 후속)~~ — DONE → `docs/mcp/fishing-tasks.md` (7개 태스크 그룹, ~278회 MCP 호출) |
| ~~FIX-068~~ | ~~2~~ | ~~farm-expansion-architecture.md [RISK] 해소: ToolType enum에 Axe/Pickaxe 추가 여부 확정~~ — DONE: 접근법 A 확정. Sickle(식물)/Hoe(지형) 분리, ToolType 미확장. farm-expansion.md + architecture 전수 업데이트 |
| ~~FIX-065~~ | ~~2~~ | ~~sound-design.md 섹션 3.5에 SFX 풀 총 크기(poolSize=16) canonical 수치 추가 (AUD-001 리뷰어 WARNING-002 후속)~~ — DONE: 섹션 3.5 풀 크기 테이블 추가 (poolSize=16, 산출 근거 포함), sound-architecture.md 섹션 3.2 참조 `섹션 3.5`로 구체화 |
| ~~FIX-066~~ | ~~2~~ | ~~sound-design.md 섹션 1에 TitleScreen/GameOver BGM 트랙 추가 또는 sound-architecture.md BGMTrack에서 해당 값 제거 결정 (AUD-001 리뷰어 INFO-001 후속)~~ — DONE: sound-design.md 섹션 1.3에 bgm_title_screen/bgm_game_over 추가; 섹션 1.4 우선순위 스택 6단계로 확장(TitleScreen/GameOver 최우선); sound-architecture.md BGMTrack 주석 참조 추가 |
| ~~ARC-027~~ | ~~2~~ | ~~사운드 시스템 MCP 태스크 시퀀스 독립 문서화 (sound-architecture.md Part II → docs/mcp/sound-tasks.md 상세 분리)~~ — DONE → `docs/mcp/sound-tasks.md` (6단계 태스크 그룹, ~148회 MCP 호출) |
| ~~BAL-012~~ | ~~2~~ | ~~낚시 경제 밸런스 분석 (어종별 기본 판매가 ROI 분석 → fishing-system.md basePrice 확정, fishing-architecture.md basePrice 0 플레이스홀더 해소 — ARC-028 후속)~~ — DONE → `docs/balance/fishing-economy.md` (15종 basePrice 확정, ROI/가공 체인/수급 분석, 밸런스 조정 제안 포함) |
| ~~FIX-069~~ | ~~2~~ | ~~fishing-system.md 섹션 2.1 "낚시 포인트 약 20개소" vs fishing-architecture.md 섹션 8.1 "FishingPoint 3개" 불일치 해소 (20개소=물리 위치, 3개=MonoBehaviour 오브젝트 개념 명확화)~~ — DONE: fishing-system.md 섹션 2.1 개념 명확화 완료, fishing-architecture.md 섹션 4/8.1 동기화 완료 |
| ~~CON-010~~ | ~~2~~ | ~~낚시 관련 업적/퀘스트 콘텐츠 추가 (achievements.md에 낚시 업적 3~5종, quest-system.md에 낚시 퀘스트 항목 추가 — ARC-028 완료 후 downstream)~~ — DONE: achievements.md 섹션 9 낚시 업적 4종 추가(ach_fish_01~04), quest-system.md NPC 의뢰 1종+일일 목표 2종+농장 도전 6종 추가, xp-integration.md/progression-curve.md/quest-system.md downstream 동기화 완료 |
| ~~ARC-029~~ | ~~1~~ | ~~낚시 숙련도 시스템 아키텍처 (fishing-system.md 섹션 5 FishingProficiency 설계 → FishingManager 통합, 낚싯대 해금 조건 연동 — DES-013 후속)~~ — DONE: fishing-architecture.md 섹션 4A FishingProficiency 클래스 설계, FishingConfig 확장, FishingManager 통합(섹션 4A.7), F-8 태스크 추가(fishing-tasks.md) |
| ~~BAL-013~~ | ~~3~~ | ~~낚시 성공률 하향 조정 (Lv.1: 80%→50%, Lv.5: 65%, Lv.10: 80% — BAL-012 RISK 후속: 초보 낚시 일일 수익 591G > 수박 최고 수익 350G 초과 해소)~~ — DONE: fishing-system.md 섹션 7.4 canonical 등록, fishing-economy.md 시뮬레이션 전면 재계산(Lv.1: 394G/315G), fishing-architecture.md 4A 업데이트 |
| ~~FIX-071~~ | ~~2~~ | ~~fishing-system.md [OPEN] 겨울 낚시 허용 여부 결정 (time-season.md 섹션 2.3 "낚시/채집 불가" 규칙 변경 여부 — 겨울 빙어/얼음 빙어왕 어종 존재하므로 허용 방향 검토)~~ — DONE: 얼음 낚시 허용 확정, time-season.md 섹션 2.2 규칙 변경 + fishing-architecture.md 섹션 8A 신규 추가 |
| ~~DES-015~~ | ~~1~~ | ~~낚싯대 업그레이드 재료 공급 경로 확정 (구리 광석·금 광석 조달 방안 — 광산 미설계 상태, 여행 상인 구매 vs 별도 채집 활동 결정 필요: fishing-system.md 섹션 1.1 [OPEN] 후속)~~ — DONE: gathering-system.md 섹션 3.8에서 동굴 입구 채집 + 여행 상인 구매 혼합 경로 확정 |
| ~~CON-011~~ | ~~1~~ | ~~낚시 도감 콘텐츠 정의 (15종 어종 도감 항목 상세 — 힌트 텍스트, 최대 크기 범위, 포획 달성 시 특수 보상 목록: fishing-system.md 섹션 8.2 후속)~~ — DONE → `docs/content/fish-catalog.md` (CON-011) |
| ~~FIX-075~~ | ~~3~~ | ~~크기 시스템 데이터 모델 통일: sizeMinCm/sizeMaxCm 절대값 채택, hintLocked/hintUnlocked 통일, firstCatchGold/XP 추가, GetSizePriceMultiplier 3등급 이산 교체~~ — DONE: fishing-architecture.md 섹션 15/18 + fish-catalog.md 섹션 7 동시 수정 완료 |
| ~~FIX-072~~ | ~~1~~ | ~~economy-system.md 섹션 1.3 골드 획득 경로에 "낚시 직판/가공" 항목 추가 (BAL-012 완료 후 downstream — economy-system.md 골드 수입원 목록 미반영)~~ — DONE: devlog #069에서 낚시 직판/가공 행 추가, 채집물 판매 [OPEN] 해소 완료 |
| ~~ARC-030~~ | ~~1~~ | ~~낚시 도감 아키텍처 (FishCatalog 클래스, 세이브 연동 — FishingStats.caughtByFishId 활용, fishing-architecture.md 섹션 신규 추가)~~ — DONE: fishing-architecture.md Part VII (섹션 14~23) 추가, save-load-architecture.md fishCatalog 필드 추가 |
| ~~BAL-014~~ | ~~1~~ | ~~낚시 숙련도 XP 획득 밸런스 검증 (BAL-013 성공률 조정 후 레벨 10 도달 일수 재시뮬레이션 — 기존 추산 45일 유지 여부 확인)~~ — DONE → `docs/balance/bal-014-fishing-xp-balance.md` (총 XP 2,250→4,500 상향, 레벨 10 도달 39일 확정) |
| ~~DES-016~~ | ~~1~~ | ~~채집 시스템 기본 설계 (낚시와 유사한 보조 활동 — 농장 주변 채집 포인트, 계절별 식물/버섯/광물 채집, 독립 숙련도 패턴 적용 여부)~~ — DONE → `docs/systems/gathering-system.md` (DES-016), `docs/systems/gathering-architecture.md` (ARC-031) |
| ~~FIX-076~~ | ~~3~~ | ~~economy-architecture.md 섹션 3.10.2 HarvestOrigin에 `Gathering = 4` 추가, 섹션 3.10.3 switch에 case 추가; 섹션 3.11 SupplyCategory enum에 `Forage = 4` 추가 및 수급 파라미터 정의 (ARC-031 후속)~~ — DONE |
| ~~FIX-077~~ | ~~3~~ | ~~progression-architecture.md 섹션 2.2 XPSource에 `GatheringComplete` 추가, 섹션 2.3 switch에 case 추가 (ARC-031 후속)~~ — DONE: OnEnable() 구독 목록 + 이벤트 흐름 다이어그램 동시 업데이트 |
| ~~FIX-078~~ | ~~2~~ | ~~inventory-architecture.md 섹션 3.2 ItemType에 `Gathered` 추가, data-pipeline.md 관련 섹션 동시 업데이트 (ARC-031 후속)~~ — DONE |
| ~~FIX-079~~ | ~~2~~ | ~~save-load-architecture.md 섹션 7 SaveLoadOrder 할당표에 `GatheringManager \| 54` 행 추가 (ARC-031 후속)~~ — DONE |
| ~~FIX-080~~ | ~~2~~ | ~~data-pipeline.md Part I 섹션 2.1 GameSaveData에 `public GatheringSaveData gathering;` 필드 추가 (ARC-031 후속)~~ — DONE: JSON 스키마 canonical 참조 주석 추가(CRITICAL-2 해소) |
| ~~FIX-081~~ | ~~2~~ | ~~economy-architecture.md 섹션 3.7.1/3.7.2의 구버전 `GetGreenhouseMultiplier(bool isGreenhouse)` pseudocode에 `[SUPERSEDED by 섹션 3.10.3]` 주석 추가 또는 삭제 처리 (Reviewer WARNING — HarvestOrigin 도입 전 초안 잔존)~~ — DONE: `[SUPERSEDED by 섹션 3.10.3]` 배너 + pseudocode 헤더 주석 추가 |
| ~~FIX-082~~ | ~~1~~ | ~~gathering-system.md Cross-references 섹션에 economy-architecture.md(FIX-076), progression-architecture.md(FIX-077), inventory-architecture.md(FIX-078), save-load-architecture.md(FIX-079) 참조 추가~~ — DONE: 섹션 8 Cross-references에 6개 참조 추가 |
| ~~CON-012~~ | ~~2~~ | ~~채집 아이템 콘텐츠 상세 (27종 아이템 상세 스펙 — 판매가, 가공 레시피 연계, 낚싯대 재료 공급 역할 명세 → docs/content/gathering-items.md)~~ — DONE → `docs/content/gathering-items.md` (CON-012): 27종 maxStack/힌트텍스트/NPC선물/SFX 확정, 가공 레시피 연계 정의, 광석 업그레이드 재료 역할 명세 |
| ~~BAL-015~~ | ~~2~~ | ~~채집 경제 밸런스 시트 (22개 포인트 일일 채집 수입 시뮬레이션, 농업·낚시·목축 대비 채집 수입 비중 검증 → docs/balance/gathering-economy.md)~~ — DONE → `docs/balance/gathering-economy.md`: 일일 수입 봄~가을 220~402G(Lv.1), Critical 이슈 발견(비중 45%), 조정안 D(판매가 40% 하향) 추천 |
| ~~ARC-032~~ | ~~2~~ | ~~채집 시스템 MCP 태스크 시퀀스 독립 문서화 (gathering-architecture.md Phase A~G → docs/mcp/gathering-tasks.md 분리, 패턴: facilities-tasks.md 참조)~~ — DONE: docs/mcp/gathering-tasks.md 신규 (Phase G-A~G-G, ~136회 MCP 호출, 16개 스크립트 정의) |
| ~~BAL-016~~ | ~~3~~ | ~~채집 아이템 판매가 하향 조정 확정 (BAL-015 조정안 D — 전체 채집물 판매가 40% 하향, gathering-system.md 섹션 3.3~3.7 수정, gathering-economy.md 섹션 8 미확정 수치 확정)~~ — DONE: 27종 판매가 40% 하향, 섹션 6.1/6.2 시뮬레이션 업데이트, gathering-economy.md 히스토리 배너 추가 및 섹션 4/6.2/Risks 갱신 |
| ~~DES-017~~ | ~~2~~ | ~~채집 낫 업그레이드 경로 상세 설계 (기본/강화/전설 채집 낫 비용·효과·재료 — 도구 업그레이드 시스템과 통합 방안 포함 → gathering-system.md 섹션 업데이트 또는 독립 문서)~~ — DONE: gathering-system.md 섹션 5.4(비용 ROI), 5.5(재료 수급), 5.6(tool-upgrade 통합) + 섹션 7.3 대화 9건 확장 |
| ~~ARC-033~~ | ~~1~~ | ~~채집 시스템 data-pipeline.md 반영 (GatheringPointData/GatheringItemData/GatheringConfig SO 에셋 테이블 섹션 2.4에 추가, PATTERN-007 준수)~~ — DONE: 섹션 2.10~2.12 신규(GatheringPointData 12필드, GatheringItemData 16필드, GatheringConfig 14필드), SO 에셋 경로 추가, Cross-references 갱신 |
| ~~CON-013~~ | ~~1~~ | ~~채집 퀘스트/업적 콘텐츠 (채집 관련 퀘스트 5종 + 업적 5종 → quest-system.md 및 achievements.md 업데이트, con-010 낚시 패턴 참조)~~ — DONE: 퀘스트 5종(NPC 2+일일 2+농장 도전 1), 업적 5종(A-031~A-035), achievement-system.md Gatherer 카테고리 추가 |
| ~~FIX-083~~ | ~~2~~ | ~~gathering-items.md 섹션 9.1 신규 제안 가공 레시피를 processing-system.md에 공식 추가 (섹션 3.7 채집물 가공 신규 생성 — 봄나물 비빔밥/야생 베리잼/건조 버섯/머루 와인/도토리묵 등)~~ — DONE: processing-system.md 섹션 3.7 채집물 레시피 13종 추가 (42→55종), ProcessingRecipeData 스키마 확장 (inputs[], unlockType, 신규 ProcessingType 4종) |
| ~~FIX-084~~ | ~~2~~ | ~~gathering-economy.md 섹션 4.1~4.3 FIX-083 확정 수치 반영~~ — DONE: [OPEN] 제거, 확정 ROI 테이블, 베이커리 연료비 반영(봄 가공 -18G), 4.3.3/4.3.4/Risks 갱신 |
| ~~BAL-017~~ | ~~2~~ | ~~채집물 가공품 ROI 밸런스 분석 신규~~ — DONE: processing-economy.md 섹션 2.8~2.13 신규 (채집물 가공 13종 ROI, 작물 가공 비교, 밸런스 이슈 3건) |
| ~~FIX-085~~ | ~~1~~ | ~~processing-economy.md 레시피 총계 42→55종 갱신~~ — DONE: 32→55종 갱신, 분석 범위 명시(45종 직접 분석, 낚시/치즈공방 추후 추가) |
| ~~DES-018~~ | ~~1~~ | ~~도감 시스템 설계 (채집 도감 + 어종 도감 통합 여부 결정 — gathering-system.md 섹션 9 [OPEN], fish-catalog.md ARC-030 패턴 참조)~~ — DONE: 통합 도감 채택. collection-system.md(설계) + collection-architecture.md(ARC-037) 신규. 채집 도감 27종, 초회 보상/마일스톤 확정. PATTERN-005/006/010 리뷰 CRITICAL 4건 수정 완료 |
| ~~FIX-093~~ | ~~2~~ | ~~save-load-architecture.md에 GatheringCatalogManager SaveLoadOrder=56 추가 (ARC-037 후속)~~ — DONE: JSON/C# gatheringCatalog 필드 추가, PATTERN-005 23개 갱신, SaveLoadOrder 행 추가 |
| ~~FIX-094~~ | ~~2~~ | ~~data-pipeline.md GameSaveData에 gatheringCatalog 필드 추가 (ARC-037 후속)~~ — DONE: JSON 스키마 + C# 클래스 동시 반영 |
| ~~FIX-095~~ | ~~2~~ | ~~project-structure.md에 SeedMind.Collection 네임스페이스/폴더 추가 (ARC-037 후속)~~ — DONE: 네임스페이스 목록 + Scripts/Collection/ 폴더 + asmdef 테이블 추가 |
| ~~FIX-096~~ | ~~2~~ | ~~fish-catalog.md UI 경로 문구 수정: "메뉴 > 어종 도감" → "메뉴 > 수집 도감 > 어종 탭" (DES-018 통합 도감 채택 후속)~~ — DONE |
| ~~FIX-099~~ | ~~1~~ | ~~xp-integration.md에 채집 도감 초회 보상 351 XP 반영 누락 (DES-018 후속 — 섹션 3.1 업적 XP 총합 및 비중 재계산, BAL-019 선행 작업)~~ — DONE: 섹션 3.1/5.1/5.2/5.3/Cross-references 업데이트, 보조 소스 합계 4,275→4,626 XP (51.2%) |
| ~~ARC-038~~ | ~~2~~ | ~~collection-architecture.md GatheringRarity/FishRarity 통합 enum 확정 및 ICatalogProvider 인터페이스 범위 결정 (ARC-037 OPEN 후속)~~ — DONE: 분리 유지 확정. ICatalogProvider 도입 없음. gathering-architecture.md 섹션 9 OPEN#2 동기화. |
| ~~ARC-039~~ | ~~2~~ | ~~collection-architecture.md CollectionPanel/FishCatalogPanel 씬 마이그레이션 전략 확정 및 MCP 태스크 보강 (ARC-037 OPEN 후속)~~ — DONE: In-place migration(Q-4a~Q-4f 6단계) 확정. collection-system.md OPEN#5 동기화. |
| ~~CON-017~~ | ~~3~~ | ~~collection-system.md "통합 수집 마스터" 업적 도입 여부 결정 및 achievements.md 반영 (DES-018 OPEN)~~ — DONE: ach_hidden_07 추가, achievements.md 섹션 10.1/11/12/13 갱신, collection-system.md OPEN#4 RESOLVED |
| ~~BAL-020~~ | ~~3~~ | ~~겨울 채집 포인트 수 재검토 (gathering-system.md 섹션 9 OPEN#5 — 현재 숲 바닥 2개소만 활성, 동굴 입구 추가 여부 결정)~~ — DONE: 옵션 A(현상 유지) 채택. 5개소(숲 바닥 2 + 동굴 입구 3) 유지. OPEN#5 RESOLVED. gathering-economy.md 섹션 7.3 PATTERN-009 히스토리 배너 추가 |
| ~~ARC-040~~ | ~~3~~ | ~~achievement-architecture.md 섹션 3.2 AchievementManager에 AchievementEvents.OnAchievementUnlocked → HandleAchievementChain 구독 상세 구현 명시 (CON-017 리뷰 CRITICAL — ach_hidden_07 연쇄 해금 로직 하드코딩 핸들러 설계, Custom(99) 핸들러 목록 achievement-system.md 섹션 7.1과 동기화)~~ — DONE: HandleAchievementChain pseudocode 추가, achievement-system.md 섹션 7.1/7.2 동기화, 리뷰 CRITICAL 5건+WARNING 3건 추가 수정 |
| ~~FIX-086~~ | ~~2~~ | ~~tool-upgrade.md에 채집 낫 통합 반영 (ToolUpgradeRecipe 스키마에 levelReqType 필드 추가)~~ — DONE: LevelReqType enum(PlayerLevel/GatheringMastery/FishingMastery), UpgradeCostInfo 확장, CanUpgrade 분기 |
| ~~FIX-087~~ | ~~1~~ | ~~npcs.md 대장간(철수) 섹션에 채집 낫 업그레이드 대화 9건 반영 (DES-017 섹션 7.3 확정 대화 → npcs.md 동기화)~~ — DONE: 섹션 4.3 업그레이드 대상 4종 갱신, 섹션 4.4 채집 낫 관련 대화 9건 추가 |
| ~~DES-019~~ | ~~2~~ | ~~베이커리 채집물 레시피 경제 조정 설계~~ — DONE: 조정안 (b)+(a) 채택 — 봄나물 비빔밥 30→60G, 송이 구이 55→70G 상향 확정 |
| ~~BAL-018~~ | ~~1~~ | ~~낚시 가공 + 치즈공방 가공 ROI 분석 (processing-economy.md 섹션 2.14~2.15 신규 추가 — FIX-085에서 "추후 추가 예정"으로 명시)~~ — DONE: 섹션 2.14 생선 가공 5종 + 섹션 2.15 치즈공방 5종 추가. fishing-economy.md 섹션 4.2~4.3 연료비 누락 수정(PATTERN-BAL-COST) |
| ~~CON-014~~ | ~~1~~ | ~~npcs.md 여행 상인 아이템 풀에 수정 원석 추가 (DES-017 섹션 5.5.2 [OPEN] — 전설 낫 재료 대안 공급 경로 확보)~~ — DONE: 160G(직판가 32G x5), 등장 확률 10%, 재고 1개, gathering-system.md [OPEN] 해소 |
| ~~FIX-088~~ | ~~2~~ | ~~achievement-architecture.md AchievementConditionType enum에 GatherCount/GatherSpeciesCollected/GatherSickleUpgraded 3종 추가 (CON-013 [TODO] 후속)~~ — DONE: enum 15→19 값, 이벤트 테이블 GatheringEvents 2행 추가, Step 1-2 16→19개 |
| ~~FIX-089~~ | ~~2~~ | ~~xp-integration.md 채집 퀘스트 XP ~105 + 채집 업적 XP 490 추가분 시뮬레이션 반영 (CON-013 후속)~~ — DONE: 섹션 3.1 Gatherer 행+합계(2,640→3,130/39종), 섹션 4.2.2 시뮬레이션, 5.1~5.4 전체 수치 갱신 |
| ~~BAL-019~~ | ~~2~~ | ~~업적 XP 비중 재검증 및 조정 결정 (업적 39종 3,130 XP, 비중 ~68% — 목표 33~43% 초과 이슈. 업적 XP 하향 또는 레벨 테이블 상향 결정)~~ — DONE: 현상 유지 확정. ~68%는 이전 XP 테이블(4,609) 기준이며 현행(9,029) 기준 35.0%로 목표 범위 내. 목표 범위 33~43% → 30~40% 조정, 1년차 업적 비율 목표 5~10% → 5~15% 조정 |
| ~~FIX-090~~ | ~~2~~ | ~~npcs.md 여행 상인 아이템 풀에 구리 광석/금 광석 공식 추가 (gathering-system.md 섹션 8.1 [OPEN] 후속 — 수정 원석 외 광석 2종 추가 필요)~~ — DONE: npcs.md 섹션 6.3에 구리 광석 x3(100G, 20%) + 금 광석 x1(120G, 10%) 추가. gathering-system.md 섹션 9 OPEN#2 완료 처리 |
| ~~ARC-034~~ | ~~1~~ | ~~quest-architecture.md QuestObjectiveType enum에 Gather 추가 및 quest-tasks.md 반영 (CON-013 후속)~~ — DONE: Fish=12/Gather=13 추가, 이벤트 핸들러 2종, quest-tasks.md T-1-04/T-1-15 동기화 |
| ~~ARC-035~~ | ~~1~~ | ~~achievement-tasks.md에 A-031~A-035 업적 5종 MCP 태스크 추가 (CON-013 후속)~~ — DONE: T-7 섹션 신규(SO 에셋 5종, 이벤트 연결, ~80회 MCP 호출), AchievementCategory Angler/Gatherer 추가, SubscribeAll 12이벤트 |
| ~~CON-016~~ | ~~1~~ | ~~gathering-system.md 강화 채집 낫 ROI 과다 이슈 해소 (섹션 9.1 [OPEN]: ~468일 ROI → 비용 500~700G 하향 또는 Gold 품질 확률 20~25% 상향 결정)~~ — DONE: Gold 품질 확률 15%→20% 상향. 강화 낫 ROI ~468일→~134일. gathering-system.md 4곳 + gathering-economy.md 5곳 수정. BAL-016 히스토리 배너 2건 추가 |
| ~~ARC-036~~ | ~~1~~ | ~~gathering-tasks.md에 GatheringPointData/GatheringItemData/GatheringConfig SO 생성 태스크 추가 (ARC-033 data-pipeline 반영 후속)~~ — DONE: G-C 섹션 상세화(~136→~220회 MCP 호출), SO 에셋 60개, 전체 필드 canonical 참조 |
| ~~FIX-091~~ | ~~2~~ | ~~economy-architecture.md SupplyCategory enum에 Forage=4 추가 + HarvestOrigin enum에 Wild 추가 (gathering-system.md 섹션 9 OPEN#6 — 채집물 경제 시스템 통합)~~ — DONE: FIX-076에서 이미 추가 완료됨 확인. gathering-system.md 섹션 6.3 [OPEN] 및 섹션 9 OPEN#6 완료 처리 |
| ~~DES-020~~ | ~~1~~ | ~~철 광석 도구 업그레이드 대체 재료 여부 결정 (gathering-system.md 섹션 9 OPEN#4 — 채집 철 광석을 철 조각 대체/원료로 쓸지 결정, 밸런스 영향 평가)~~ — DONE: 방안 A(가공소 제련) 채택. 철 광석 x3 -> 철 조각 x1 레시피(`recipe_smelt_iron`) 추가. processing-system.md 56종, gathering-system/gathering-items/tool-upgrade 동기화 완료 |
| ~~FIX-092~~ | ~~2~~ | ~~DES-020 리뷰 WARNING 후속 — processing-system.md 섹션 3.7.4 철 광석 판매가 인라인 참조 주석 추가, gathering-items.md 섹션 10.2 테이블 셀 canonical 참조 추가, tool-upgrade.md/processing-system.md Cross-references 상호 추가~~ — DONE: 인라인 수치 3개 canonical 참조 추가, 테이블 셀 3개 참조 추가, Cross-references 상호 등록 완료 |
| ~~FIX-100~~ | ~~2~~ | ~~achievement-tasks.md에 ach_hidden_07 SO 에셋 생성 및 HandleAchievementChain 구현 태스크 추가 (ARC-040 후속 — CON-017에서 업적 추가됐으나 MCP 태스크 문서 미반영)~~ — DONE: SO 에셋 목록 A-36 추가, T-2-32 태스크 상세 추가, 업적 총개수 39→40 수정 |
| ~~FIX-101~~ | ~~2~~ | ~~achievement-system.md 섹션 7.1 OnItemGathered 행 [TODO] 태그 제거 및 구독 확정 표기 (FIX-088에서 GatherCount/GatherSpeciesCollected/GatherSickleUpgraded enum 추가 완료됨 — [TODO] 잔존 상태)~~ — DONE |
| ~~ARC-041~~ | ~~2~~ | ~~collection-tasks.md MCP 태스크 시퀀스 문서화 (collection-architecture.md Part II → docs/mcp/collection-tasks.md — ARC-038/ARC-039 완료 후 선행 조건)~~ — DONE → `docs/mcp/collection-tasks.md` (8개 태스크 그룹, ~126회 MCP 호출, Q-A~Q-H) |
| ARC-042 | 1 | collection-architecture.md GatheringCatalogData↔GatheringItemData SO 참조 방식 확정 (OPEN#5 — itemId 문자열 연결 vs SO 직접 참조, 구현 시 결정) |
| DES-021 | 1 | 보조 XP 소스 합산 비중 상한 기준 설계 원칙 문서화 (BAL-019 OPEN — 보조 소스 51.2%, 60% 초과 시 재검토 기준 명시) |
| ~~CON-015~~ | ~~2~~ | ~~collection-system.md 채집 도감 통합 UI CollectionPanel 씬 배치 확정 (DES-018 — CollectionUIController가 FishCatalogPanel을 하위로 흡수하는 마이그레이션 경로, collection-architecture.md OPEN#4 후속)~~ — DONE: collection-system.md OPEN#5 [ARC-038/ARC-039 확정]으로 닫음. 마이그레이션 전략 collection-architecture.md Q-4a~Q-4f 참조. |
| ~~FIX-102~~ | ~~2~~ | ~~save-load-architecture.md Cross-references에 collection-architecture.md 참조 추가 (FIX-093 후속 — gatheringCatalog 필드 추가로 collection-architecture.md가 연계됨)~~ — DONE |
| ~~FIX-103~~ | ~~1~~ | ~~data-pipeline.md 시스템 데이터 총계 주석 업데이트 (FIX-094 후속)~~ — N/A: data-pipeline.md에 PATTERN-005 카운트 주석 없음, 수정 불필요 |
| ~~FIX-104~~ | ~~1~~ | ~~fishing-architecture.md 섹션 21.5 씬 계층에 ARC-039 마이그레이션 노트 추가 (FishCatalogPanel → CollectionPanel/FishPanel 통합 예정 명시 — collection-architecture.md Cross-references에 이미 Q-4 마이그레이션 언급됨, 역방향 참조 보완)~~ — DONE: 섹션 21.5 마이그레이션 예정 노트 추가, collection-architecture.md Q-4a~Q-4f 참조 명시 |
| FIX-105 | 1 | collection-system.md 섹션 6.1 단축키 바인딩 OPEN#2 결정 (fish-catalog.md 섹션 5.1과 동일 이슈 — UI 시스템 단축키 정책 통일, ui-system.md 섹션 7 참조) |
| ~~BAL-021~~ | ~~2~~ | ~~연간 수익 흐름 통합 시뮬레이션 — 작물/낚시/채집/가공/목축 5개 수익 소스 합산, 1년차 예상 총수익 범위 확인 (economy-system.md 섹션 6 밸런스 검증 후속)~~ — DONE → `docs/balance/annual-economy.md` (A:~13,500G / B:~131,900G / C:~376,500G, 낚시 우위 구조 [RISK] 확인) |
| ~~DES-023~~ | ~~2~~ | ~~농장 장식/꾸미기 시스템 설계 검토 — 울타리/경로/장식물 배치 메카닉 필요 여부, design.md 4.6 시설 목록에 장식 카테고리 포함 여부 결정~~ — DONE → `docs/systems/decoration-system.md` (5개 카테고리: Fence/Path/Light/Ornament/WaterDecor, 그리드 고정 배치, 0% 환불 소모형, 골드 소모처 역할) |
| ~~ARC-043~~ | ~~2~~ | ~~농장 장식 시스템 기술 아키텍처 (DecorationManager, DecorationItemData SO, DecorationSaveData, Tilemap 연동 — DES-023 후속)~~ — DONE → `docs/systems/decoration-architecture.md` |
| ARC-044 | 1 | 전체 MCP 태스크 시퀀스 의존성 그래프 문서화 — 각 Phase가 선행하는 Phase를 명시한 빌드 순서 개요 작성 (Phase 2 구현 착수 시 실행 순서 로드맵) |
| DES-022 | 1 | farm-expansion.md 잔존 [OPEN] 항목 일괄 처리 — 섹션 3.3 이하 미결 항목 검토 및 확정 (ARC-023 후속, Phase 2 착수 전 선행 필요) |
| ~~FIX-106~~ | ~~2~~ | ~~collection-architecture.md 섹션 2 시스템 다이어그램에 GatheringCatalogManager 박스 추가 (현재 다이어그램이 FishCatalogManager 중심으로 기술되고 GatheringCatalogManager 박스가 누락되어 있는지 확인 후 보완)~~ — N/A: 다이어그램 섹션 2에 GatheringCatalogManager 박스([신규 시스템] 레이블, 필드/메서드/이벤트 전체 포함)가 이미 존재함. 수정 불필요 |
| ~~BAL-024~~ | ~~2~~ | ~~낚시+채집 합산 비율 구조적 이탈 분석 및 조정안 검토 — BAL-023 리뷰에서 새 기준(60%)도 시나리오 B(72%)·C(65%)에서 초과 확인. 낚시 에너지 비용 상향 또는 일일 낚시 시간 제약 도입 중 결정 (economy-system.md 섹션 8 [RISK] 낚시 우위 구조 후속)~~ — DONE → `docs/balance/fishing-gathering-ratio.md` (캐스팅 에너지 2→3, Lv.8+ 3→2, fishGatherVsCropProcessingMax 0.60→0.65 확정) |
| ~~FIX-114~~ | ~~2~~ | ~~fishing-system.md 섹션 2.3 캐스팅 에너지 2→3, Lv.8+ 캐스팅 에너지 2 명시 (BAL-024 확정 파라미터 반영)~~ — DONE: 섹션 2.3 테이블, 플로우 다이어그램, 설계 의도 문구, Lv.8 숙련도 표, 튜닝 파라미터 전수 수정 |
| ~~FIX-115~~ | ~~2~~ | ~~fishing-economy.md 섹션 3.1~3.4, 5.4 새 에너지 기준(캐스팅 3)으로 시뮬레이션 재계산 (BAL-024 후속)~~ — DONE: 섹션 2.1 전제, 2.5 G/E 테이블, 3.1~3.4 계산 블록, 3.5 비교 테이블, 5.4 수급 반영 수익, 섹션 6 요약 모두 갱신 |
| ~~FIX-116~~ | ~~2~~ | ~~economy-system.md 섹션 8.6 `fishGatherVsCropProcessingMax` 0.60→0.65 수정, 섹션 8.3 시나리오별 비율 요약 업데이트 (BAL-024 후속)~~ — DONE: 8.6 목표값 0.65, 8.3 B·C 비율(62.7%/37.2%) 갱신, [RISK]→[RESOLVED: BAL-024] |
| ~~FIX-117~~ | ~~2~~ | ~~annual-economy.md 섹션 4.2 시나리오 B/C 낚시 수익 수치 및 비율 재계산 반영 (BAL-024 후속)~~ — DONE: 1.3 canonical 참조(221/339/752G), 3.2/3.3 합산 낚시 37,968G/62,720G+히스토리 배너, 4.1 비중표, 4.2 달성 여부, 4.3 이상징후 텍스트 갱신 |
| ~~FIX-109~~ | ~~1~~ | ~~economy-architecture.md에 BAL-023 파라미터 키명 변경 반영 확인 — fishGatherVsCropMax → fishGatherVsCropProcessingMax 변경이 economy-architecture.md에 직접 기재된 경우 갱신 (canonical 참조로 교체 or 키명 수정)~~ — DONE: economy-architecture.md에 해당 키명 직접 기재 없음 (canonical 참조 구조로 이미 준수됨), 수정 불필요 |
| ~~CON-018~~ | ~~2~~ | ~~collection-system.md에 채집 아이템 초회 발견 보상 canonical 정의 (rarity별 firstDiscoverGold/XP 테이블 — ARC-041 Q-C-02에서 [OPEN] 처리된 수치, 구현 전 확정 필요)~~ — DONE: 섹션 3.3 희귀도별 테이블(Common 5G/2XP ~ Legendary 200G/50XP) + 섹션 5.2.1 27종 아이템별 테이블 이미 DES-018에서 작성 완료. ARC-041 Q-C-02도 해당 섹션을 참조만 함 |
| ~~ARC-045~~ | ~~1~~ | ~~data-pipeline.md 섹션 2.13에 GatheringCatalogData SO 에셋 스키마 추가 (PATTERN-007 준수 — ARC-041에서 새 SO 타입 도입됨, 기존 섹션 2.10~2.12 패턴 참조)~~ — DONE: 섹션 2.13 신규, 섹션 1.1 행 추가(~190→~217개), Cross-references 3개 추가 |
| ~~BAL-022~~ | ~~2~~ | ~~작물 수익 비중 설계 목표 공식화 (economy-system.md에 "작물 50~70%", "낚시+채집 = 작물의 20~40%" 목표 수치 명시 섹션 추가 — BAL-021 OPEN#1 후속)~~ — DONE → `docs/systems/economy-system.md` 섹션 8 신규 추가 (수익원 비중 목표 canonical 확정, annual-economy.md 섹션 1.3/4.2 수정 완료) |
| ~~FIX-107~~ | ~~2~~ | ~~온실 딸기 재수확 재검토 결정 (겨울 딸기가 전용 작물 압도 — BAL-021 OPEN#3, BAL-001 후속. 재수확 3일→4일 또는 온실 다중 수확 제한 중 확정)~~ — DONE: 재수확 3일 유지 확정. BAL-010 수급 보정 시 표고버섯(5,920G) > 딸기(5,584G) 역전 확인. annual-economy.md OPEN#3/이상징후2 RESOLVED, crop-economy.md 제안A [OPEN] RESOLVED |
| ~~CON-019~~ | ~~1~~ | ~~치즈 공방 연료비 canonical 확정 (livestock-system.md 섹션 7.2 [OPEN] — BAL-021 OPEN#2 후속, 시나리오 C 목축 수익 재계산 전 필요)~~ — DONE: 연료비 0G 확정 (processing-system.md 섹션 4.1 canonical), livestock-system.md 섹션 7.1/7.2/Open Questions, annual-economy.md 시나리오 C [OPEN] 닫힘 |
| ~~FIX-108~~ | ~~2~~ | ~~economy-system.md 섹션 8.2 채집 비중 하한 수정 — 현행 "10~20%"를 "15~20%"로 교정 (gathering-economy.md 섹션 6.2와 불일치, BAL-022 리뷰 WARNING 후속)~~ — DONE: economy-system.md 섹션 8.2 채집 행 "10~20%" → "15~20%" 교정 |
| ~~BAL-023~~ | ~~1~~ | ~~economy-system.md BAL-022 OPEN#1 후속 — 작물 단독 비중 하한(25%) 현실화 결정: 20%로 완화하거나 작물+가공 합산 기준으로 단일화 (시나리오 B·C에서 18~28% 이탈 확인)~~ — DONE: 작물+가공 합산 기준 단일화, 8.2·8.4·8.6 및 annual-economy.md 1.3·4.2·4.3·5 업데이트 |
| ~~FIX-110~~ | ~~1~~ | ~~farm-expansion.md Open Questions 해소된 항목 DONE 처리 — #1(FIX-036 완료), #4(CON-006 완료), #8(FIX-035 완료) 태그 닫기~~ — DONE: Open Questions #1/#4/#8 [RESOLVED] 처리, 인라인 [OPEN] 2건 및 [RISK] 1건 동시 처리 |
| ~~FIX-111~~ | ~~2~~ | ~~save-load-architecture.md GameSaveData JSON/C# 클래스에 `decoration: DecorationSaveData` 필드 추가 (ARC-043 후속 — PATTERN-005 준수)~~ — DONE: 섹션 2.1 트리/2.2 JSON/2.3 C# 3곳 추가, PATTERN-005 필드 수 23→24 갱신, GatheringCatalogSaveData 트리 누락도 동시 수정 |
| ~~FIX-113~~ | ~~2~~ | ~~data-pipeline.md 섹션 2.14~2.15에 DecorationItemData/DecorationConfig SO 에셋 스키마 추가 (PATTERN-007 준수 — ARC-043에서 신규 SO 타입 2종 도입됨, 기존 섹션 2.10~2.13 패턴 참조)~~ — DONE: 섹션 1.1 테이블에 2행 추가(총 247개), 섹션 2.14 DecorationItemData 20필드 + 섹션 2.15 DecorationConfig 5필드 스키마 추가, 모든 콘텐츠 수치는 canonical 참조로만 기재 |
| ~~FIX-112~~ | ~~1~~ | ~~project-structure.md에 SeedMind.Decoration 네임스페이스/폴더 추가 (ARC-043 후속 — Scripts/Decoration/ 폴더, asmdef 등록)~~ — DONE: 섹션 1 폴더 트리에 Scripts/Decoration/ 추가, 섹션 2 네임스페이스에 SeedMind.Decoration/SeedMind.Decoration.Data 추가, 섹션 4 asmdef 테이블에 SeedMind.Decoration.asmdef 추가, Data/Decorations/ 에셋 폴더 추가, Cross-references에 decoration-architecture.md 추가 |
| ~~ARC-046~~ | ~~1~~ | ~~장식 시스템 MCP 태스크 시퀀스 문서화 (decoration-architecture.md Part II → docs/mcp/decoration-tasks.md — ARC-043 후속)~~ — DONE → `docs/mcp/decoration-tasks.md` (5개 태스크 그룹 D-A~D-E, ~105회 MCP 호출) |
| PATTERN-011 | - | [self-improve 전용] MCP 태스크 문서 예시 에셋명이 canonical 콘텐츠 문서 영문 ID와 불일치하는 패턴 반복 발생 (ARC-046에서 Fence/Path/Light 3개 카테고리 3건 — FenceBrick/PathCobble/PathFlower/LightFairy 등 임의 생성). canonical SO ID를 직접 조회하지 않고 임의 작성 → MCP 태스크 작성 시 에셋명은 반드시 canonical 콘텐츠 문서에서 조회하도록 규칙화 필요 |
