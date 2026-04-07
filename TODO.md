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
| PATTERN-009 | - | [self-improve 전용] 밸런스 문서에서 결정 이전 분석 섹션 수치가 결정 이후에도 갱신되지 않아 구/신 수치 혼재 발생 (crop-economy.md 섹션 4.3.2~4.3.8 vs 4.3.10에서 5건 식별 — 수확 6회/4일 vs 5회/5일 불일치) → 결정 이전 섹션에 "(히스토리 — BAL-XXX 이전 분석)" 배너 의무 표기 규칙 추가 필요 |
| ~~FIX-034~~ | ~~2~~ | ~~economy-architecture.md HarvestOrigin enum 설계 (온실 수확물 출처 추적 — BAL-010 아키텍트 분석 [RISK] 후속, devlog 043 언급)~~ — DONE → economy-architecture.md 섹션 3.10 추가, inventory-architecture.md ItemSlot/AddItem/RemoveItem 업데이트, data-pipeline.md ItemSlotSaveData origin 필드 추가 |
| ~~ARC-023~~ | ~~2~~ | ~~농장 확장 시스템 기술 아키텍처 (FarmZoneManager, ZoneData SO, 타일 구매 흐름 — DES-012 후속)~~ — DONE → `docs/systems/farm-expansion-architecture.md` |
| ~~ARC-024~~ | ~~1~~ | ~~목축/낙농 시스템 MCP 태스크 시퀀스 (AnimalManager/AnimalData 구현 시퀀스 독립 문서화 — ARC-019 후속)~~ — DONE → `docs/mcp/livestock-tasks.md` (9개 태스크 그룹, ~221회 MCP 호출) |
| ~~FIX-035~~ | ~~2~~ | ~~progression-curve.md 섹션 1.2.4 농장 확장 XP를 "4단계 100XP" → "6단계 150XP"로 업데이트 (DES-012에서 Zone B~G 6단계로 확정 — farm-expansion.md [OPEN] 8 후속)~~ — DONE: 섹션 1.2.4 6단계 150XP, 보조 마일스톤 "6단계 Zone G" 수정 |
| ~~FIX-036~~ | ~~2~~ | ~~economy-system.md 섹션 3.3 목공소 인벤토리를 Zone B~G 7구역 방식으로 업데이트 (DES-012 섹션 2.1 해금 조건 테이블 반영 — farming-system.md 섹션 1 확장 방식 동기화 포함)~~ — DONE: economy-system.md 섹션 3.3/5.2, farming-system.md 섹션 1 Zone 참조 교체 |
| ~~FIX-037~~ | ~~1~~ | ~~farm-expansion.md 섹션 4.4 과일나무 데이터(묘목 가격, 수확량, 판매가)를 crops.md로 이전하고 참조 표기로 교체 (WARNING-8: 비-canonical 문서에 작물 가격 직접 기재)~~ — DONE → crops.md 섹션 6 (과일나무 canonical 섹션 신규), farm-expansion.md 섹션 4.4 참조 표기로 교체 |
| PATTERN-010 | - | [self-improve 전용] 아키텍처 문서를 디자인 문서와 병렬 작성 시 플레이스홀더 수치/ID를 사용하고 디자인 확정 후 동기화하지 않는 패턴 (ARC-023에서 zoneId 5종·최대 크기 불일치 3건 발생) |
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
| FIX-081 | 2 | economy-architecture.md 섹션 3.7.1/3.7.2의 구버전 `GetGreenhouseMultiplier(bool isGreenhouse)` pseudocode에 `[SUPERSEDED by 섹션 3.10.3]` 주석 추가 또는 삭제 처리 (Reviewer WARNING — HarvestOrigin 도입 전 초안 잔존) |
| ARC-032 | 2 | 채집 시스템 MCP 태스크 시퀀스 독립 문서화 (gathering-architecture.md Phase A~G → docs/mcp/gathering-tasks.md 분리, 패턴: facilities-tasks.md 참조) |
| BAL-015 | 2 | 채집 경제 밸런스 시트 (22개 포인트 일일 채집 수입 시뮬레이션, 농업·낚시·목축 대비 채집 수입 비중 검증 → docs/balance/gathering-economy.md) |
| CON-012 | 2 | 채집 아이템 콘텐츠 상세 (27종 아이템 상세 스펙 — 판매가, 가공 레시피 연계, 낚싯대 재료 공급 역할 명세 → docs/content/gathering-items.md) |
| DES-017 | 2 | 채집 낫 업그레이드 경로 상세 설계 (기본/강화/전설 채집 낫 비용·효과·재료 — 도구 업그레이드 시스템과 통합 방안 포함 → gathering-system.md 섹션 업데이트 또는 독립 문서) |
| ARC-033 | 1 | 채집 시스템 data-pipeline.md 반영 (GatheringPointData/GatheringItemData/GatheringConfig SO 에셋 테이블 섹션 2.4에 추가, PATTERN-007 준수) |
| FIX-082 | 1 | gathering-system.md Cross-references 섹션에 economy-architecture.md(FIX-076), progression-architecture.md(FIX-077), inventory-architecture.md(FIX-078), save-load-architecture.md(FIX-079) 참조 추가 |
| CON-013 | 1 | 채집 퀘스트/업적 콘텐츠 (채집 관련 퀘스트 5종 + 업적 5종 → quest-system.md 및 achievements.md 업데이트, con-010 낚시 패턴 참조) |
