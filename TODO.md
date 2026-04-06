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
| VIS-001 | 2 | 비주얼 가이드 (로우폴리 스타일 참고자료, 색상 팔레트) |
| AUD-001 | 1 | 사운드 디자인 문서 (필요한 효과음/BGM 목록) |
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
| BAL-003 | 2 | 겨울 작물 3종 ROI/밸런스 분석 (겨울무/표고버섯/시금치 → crop-economy.md 추가) |
| ~~ARC-007~~ | ~~2~~ | ~~시설 MCP 태스크 시퀀스 (facilities-architecture.md Part II를 독립 문서로 분리 → docs/mcp/facilities-tasks.md)~~ — DONE → `docs/mcp/facilities-tasks.md` |
| ~~DES-007~~ | ~~2~~ | ~~도구 업그레이드 시스템 설계 (호미/물뿌리개/낫 각 3단계 업그레이드, 비용, 효과)~~ — DONE -> `docs/systems/tool-upgrade.md` |
| ~~DES-008~~ | ~~1~~ | ~~세이브/로드 UX 설계 (자동저장 트리거, 수동 저장, 멀티슬롯 여부)~~ — DONE → `docs/systems/save-load-system.md` |
| ~~FIX-004~~ | ~~3~~ | ~~data-pipeline.md 섹션 2.4 시설 에셋 테이블의 tileSize/buildTimeDays 직접 수치를 `(→ see docs/content/facilities.md)` 참조로 교체 (PATTERN-007 후속)~~ — DONE |
| ~~ARC-008~~ | ~~2~~ | ~~도구 업그레이드 MCP 태스크 시퀀스 (tool-upgrade-architecture.md Part II를 독립 문서로 분리 → docs/mcp/tool-upgrade-tasks.md)~~ — DONE → `docs/mcp/tool-upgrade-tasks.md` (문서 ID ARC-015로 할당, ARC-008은 npc-shop-architecture.md에 이미 사용됨) |
| CON-004 | 2 | 대장간 NPC 상세 (캐릭터, 대화, 업그레이드 인터페이스 UX) |
| ~~ARC-009~~ | ~~2~~ | ~~NPC/상점 MCP 태스크 시퀀스 독립 문서화 (npc-shop-architecture.md Part II 요약 → docs/mcp/npc-shop-tasks.md 상세 분리)~~ — DONE → `docs/mcp/npc-shop-tasks.md` |
| BAL-005 | 1 | 여행 상인 희귀 아이템 가격 밸런스 분석 (npcs.md [OPEN] 후속 — 만능 비료/겨울 씨앗 등 8종 ROI 검증) |
| ARC-010 | 2 | 튜토리얼 MCP 태스크 시퀀스 독립 문서화 (tutorial-architecture.md Part II → docs/mcp/tutorial-tasks.md) |
| ~~ARC-011~~ | ~~2~~ | ~~세이브/로드 시스템 기술 아키텍처 (SaveManager, GameSaveData 통합 구조, 자동저장 트리거 설계)~~ — DONE → `docs/systems/save-load-architecture.md` |
| ARC-012 | 2 | 세이브/로드 MCP 태스크 시퀀스 독립 문서화 (save-load-architecture.md Part II → docs/mcp/save-load-tasks.md) |
| ~~CON-005~~ | ~~2~~ | ~~가공/요리 시스템 콘텐츠 상세 (가공소별 레시피 전체 목록, 처리 시간, 연료 소모 규칙)~~ — DONE -> `docs/content/processing-system.md` |
| ~~DES-009~~ | ~~2~~ | ~~퀘스트/미션 시스템 설계 (계절별 목표, NPC 의뢰, 달성 보상 구조)~~ — DONE → `docs/systems/quest-system.md`, `docs/systems/quest-architecture.md` |
| ~~ARC-014~~ | ~~2~~ | ~~가공 시스템 MCP 태스크 시퀀스 독립 문서화 (processing-architecture.md Part II → docs/mcp/processing-tasks.md)~~ — DONE → `docs/mcp/processing-tasks.md` (ID: ARC-014) |
| ~~BAL-004~~ | ~~2~~ | ~~가공품 ROI/밸런스 분석 (32종 레시피 수익성, 가공 체인 ROI, 가공 vs 생작물 비교 → docs/balance/processing-economy.md)~~ — DONE → `docs/balance/processing-economy.md` |
| CON-006 | 1 | 목축/낙농 시스템 콘텐츠 상세 (치즈 공방 활성화 선행 조건 — 젖소/염소 등 동물 관리 메카닉) |
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
| VIS-001 | 2 | 비주얼 가이드 (로우폴리 스타일 참고자료, 색상 팔레트) |
| BAL-003 | 2 | 겨울 작물 3종 ROI/밸런스 분석 (겨울무/표고버섯/시금치 → crop-economy.md 추가) |
| CON-004 | 2 | 대장간 NPC 상세 (캐릭터, 대화, 업그레이드 인터페이스 UX) |
| ARC-019 | 1 | 목축/낙농 시스템 기술 아키텍처 (AnimalManager, AnimalData SO, 돌봄 사이클 설계 — CON-006 후속) |
| BAL-008 | 1 | 목축/낙농 경제 밸런스 분석 (동물 구매/사료 비용 vs 젖/양모/알 판매 ROI — CON-006 + ARC-019 후속) |
| FIX-026 | 2 | time-season-tasks.md 작성 예정 항목 우선순위 확인 (time-season-architecture.md Cross-references에 '작성 예정'으로 등재됨) |
| ~~FIX-025~~ | ~~2~~ | ~~tutorial-tasks.md Step 07/11 이벤트명 확정 (TimeEvents.OnSleepExecuted, 구매 완료 이벤트 — ARC-010 리뷰 WARNING 후속)~~ — DONE: S07→TimeManager.OnSleepCompleted, S11→EconomyEvents.OnShopPurchased 확정; economy-architecture.md 이벤트 표 보완 |
