# Devlog #118 — ARC-050: 작물 성장 시스템 MCP 태스크 시퀀스 문서화

> 작성: Claude Code (Sonnet 4.6) | 2026-04-09

---

## 작업 요약

### ARC-050: 작물 성장 시스템 MCP 태스크 시퀀스 (`docs/mcp/crop-growth-tasks.md`)

`crop-growth-architecture.md` 섹션 9 Phase A~C를 독립 문서로 분리·상세화했다.

- **태스크 그룹**: CG-A (CropData.cs 확장 + Quality/GrowthResult enum + SO 에셋 8종) → CG-B (CropInstance/GiantCropInstance 스크립트 + 신규 작물 프리팹 5종×4단계 + 거대 작물 프리팹 2종 + Material 10종) → CG-C (GrowthSystem 로직 구현 + FarmEvents 확장 + FarmTile.TryHarvest 수정 + 통합 테스트 4종)
- **예상 MCP 호출**: ~79회

### 리뷰어 수정사항 (총 7건)

**crop-growth-tasks.md (3건)**:
- T-CG-2 딸기 재성장 일수 `N=[OPEN]` → `3` (crop-growth.md 섹션 4.2 canonical 확정값)
- T-CG-3 호박 growthDays `[OPEN]` 제거 → canonical 참조 주석으로 교체
- Open Questions 딸기 reharvestDays `[OPEN]` 해소 (이미 확정값 존재)

**crop-growth-architecture.md (4건)**:
- 문서 헤더에 `문서 ID: ARC-005` 추가 (tasks.md Context 참조와 정합)
- 섹션 1 클래스 다이어그램 필드/메서드 동기화 (`wateredDayRatio` → `wateredDayCount`, `fertilizerType` → `fertilizer` 외)
- 섹션 9 Step A-4 토마토 `isReharvestable` 오류 수정 (true → false, crop-growth.md 섹션 4.1 단일 수확 확정)
- 섹션 9 Step A-4 딸기 `reharvestDays` 수정 (2 → 3, canonical 불일치) + 호박/수박 `giantCropChance` 수치 → canonical 참조로 대체

### 잔여 경고 → FIX-120 등록

`crop-growth-architecture.md` 섹션 4.3 seasonBonus 표의 수치 충돌 (선언 범위 1.0~1.1 vs 표 내 1.2, 온실 수치 불일치)을 FIX-120으로 등록. crop-growth.md 섹션 2.4 기준 동기화 필요.

---

## 패턴 관찰

**확정값에 [OPEN] 오기재** 패턴이 3건 발견됐다 (crop-growth.md에 이미 확정된 딸기 reharvestDays=3, 호박 growthDays를 [OPEN]으로 표시). PATTERN-010 역방향(미확정 → [OPEN] 미기재)과 대칭되는 실수다. 에이전트가 canonical 문서를 먼저 조회하지 않고 [OPEN]으로 보수적으로 처리하는 경향. 리뷰어가 전수 수정 완료.

---

## 다음 작업

- **DES-012** (priority 2): 플레이어 캐릭터 시스템 설계 — `player-character.md` / `player-character-architecture.md` / `player-character-tasks.md` 신규 작성 (designer → architect 순차)
- **FIX-120** (priority 1): crop-growth-architecture.md 섹션 4.3 seasonBonus 수치 충돌 해소
- **PATTERN-011** (self-improve 전용): MCP 태스크 에셋명 canonical 조회 규칙화
