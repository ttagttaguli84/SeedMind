# Devlog #014 — 도구 업그레이드 시스템 (DES-007) + PATTERN-007

> 2026-04-06 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### PATTERN-007: SO 에셋 테이블 수치 기재 금지 규칙 강화

self-improve 에이전트가 처리. `.claude/rules/` 2개 파일 업데이트.

**수정된 파일**:
- `.claude/rules/doc-standards.md` — Consistency Rules에 PATTERN-007 규칙 추가, Canonical 데이터 매핑에 시설 파라미터 항목 추가
- `.claude/rules/workflow.md` — Reviewer Checklist 항목 11 추가
- `TODO.md` — PATTERN-007 DONE 처리

**핵심 규칙**: SO 에셋 테이블(data-pipeline.md 등)에서 `tileSize`, `buildTimeDays`, `recipeCount` 등 콘텐츠 정의 파라미터를 직접 기재 금지. canonical 콘텐츠 문서(`docs/content/facilities.md`)만 참조.

---

### DES-007: 도구 업그레이드 시스템

Designer + Architect 병렬 실행 → Reviewer CRITICAL 5건·WARNING 2건 발견 후 전부 수정. 총 8개 파일 수정됨.

**신규 문서**:
1. `docs/systems/tool-upgrade.md` — 도구 업그레이드 시스템 canonical 설계 문서
2. `docs/systems/tool-upgrade-architecture.md` — 기술 아키텍처 (Part I + Part II)

**수정된 문서**:
- `docs/systems/farming-system.md` — 섹션 7을 tool-upgrade.md 참조로 대체, 물뿌리개 용량 3단계로 업데이트
- `docs/systems/inventory-system.md` — toolTier enum 구 5단계 → 3단계로 수정 (C-1, C-2)
- `docs/balance/progression-curve.md` — 도구 업그레이드 XP 총량 240 → 90으로 수정 (C-5)
- `docs/pipeline/data-pipeline.md` — ToolData SO 체인 5단계 → 3단계로 수정 (C-3)
- `docs/systems/tool-upgrade-architecture.md` — MCP Phase B 에셋명 3단계로 수정 (C-4)
- `docs/design.md`, `docs/architecture.md` — Cross-references 추가

---

## 핵심 설계 내용

### 도구 3종 × 3단계 업그레이드 확정

**단계명**: Basic → Reinforced → Legendary (구 5단계 폐기)

**호미 (Hoe)**:
- Basic: 단일 타일 경작
- Reinforced: 1×3 타일 경작 (수평/수직)
- Legendary: 3×3 영역 경작

**물뿌리개 (Watering Can)**:
- Basic: 저수 20, 단일 타일
- Reinforced: 저수 40, 1×3 범위
- Legendary: 저수 80, 3×3 범위

**낫 (Sickle)**:
- Basic: 단일 수확
- Reinforced: 인접 3타일 동시 수확
- Legendary: 9타일 범위, 품질 보정 +5%

### 업그레이드 시스템 규칙
- 대장간 NPC에서 골드 + 재료(철 조각/정제 강철)로 업그레이드
- 업그레이드 소요 시간: 1~2일
- 업그레이드 중 해당 도구 사용 불가 (전략적 타이밍 결정 유도)
- 레벨 요건: Reinforced → 레벨 5, Legendary → 레벨 10

### 기술 아키텍처 주요 결정
- `ToolData` SO 체인: Basic → Reinforced → Legendary (`nextTier` 참조)
- `ToolUpgradeSystem` + `ToolEffectResolver` 독립 클래스로 분리
- `ToolUpgradeEvents` 정적 허브 (FarmEvents/BuildingEvents 패턴 계승)
- `PlayerSaveData`에 `ToolUpgradeSaveData` 필드 추가 (업그레이드 진행 상태 포함)

---

## 리뷰 결과

**CRITICAL 5건 (수정 완료)**:
1. [C-1] toolTier enum 불일치 — inventory-system.md 구 5단계(Copper/Iron/Gold) → 3단계(Reinforced/Legendary) 수정
2. [C-2] 슬롯 표시 조건 — "Copper 이상" → "Reinforced 이상"으로 수정
3. [C-3] data-pipeline.md SO 체인 5단계 → 3단계 전면 수정
4. [C-4] tool-upgrade-architecture.md MCP Phase B 에셋명 구 5단계 잔존 → 수정
5. [C-5] progression-curve.md XP 총량 "240 XP" → "90 XP" 수정

**WARNING 2건 (수정 완료)**:
1. [W-1] upgradeGoldCost 참조 대상 오류 → tool-upgrade.md 참조로 수정
2. [W-2] tool-upgrade-architecture.md 내 구 tier 언급 잔존 → 수정

---

## 의사결정 기록

1. **5단계 → 3단계 단순화**: 진행감은 충분하면서 밸런스 조정 복잡도를 줄임. Basic / Reinforced / Legendary 3단계가 직관적.

2. **대장간 NPC 업그레이드 방식**: 단순 골드 지불보다 재료 수집 과정이 게임플레이 루프를 풍성하게 만들고 중기 목표를 제공함.

3. **업그레이드 중 도구 사용 불가**: 업그레이드 타이밍을 전략적 결정으로 만들어 단순 클릭이 아닌 계획 요소로 승격.

4. **ToolEffectResolver 분리**: ToolSystem이 직접 조건 분기하지 않고 Resolver에 위임함으로써 새 도구 추가 시 기존 시스템 변경 최소화.

---

## 미결 사항 ([OPEN])

- 대장간 NPC 캐릭터 이름·성격 미정 → CON-004에서 처리
- 업그레이드 재료(철 조각/정제 강철) 드롭 경로 미확정
- 물탱크와 Legendary 물뿌리개의 역할 중복 검토 필요
- Legendary 낫 품질 보정 +5%의 밸런스 영향 → BAL 분석 필요

---

## 후속 작업 필요

- `FIX-004`: data-pipeline.md 섹션 2.4 시설 tileSize 수치 → canonical 참조로 교체 (PATTERN-007 후속)
- `ARC-008`: tool-upgrade MCP 태스크 시퀀스 독립 문서화
- `CON-003` → `CON-004`: 대장간 NPC 상세 설계
- `BAL-003`, `BAL-004`: 겨울 작물·가공품 ROI 분석

---

## 다음 단계

- FIX-004 (Priority 3): data-pipeline.md 시설 tileSize 즉시 수정
- CON-003 (Priority 2): NPC/상점 콘텐츠 (대장간 포함)
- BAL-003 (Priority 2): 겨울 작물 밸런스 분석

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
