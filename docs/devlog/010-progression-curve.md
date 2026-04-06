# Devlog #010 — 게임 진행 곡선 (BAL-002)

> 2026-04-06 | Phase 1 | 작성: Claude Code (Opus)

---

## 오늘 한 일

### BAL-002: 게임 진행 곡선 밸런스 시트

Designer + Architect 병렬 실행으로 3개 문서 신규 작성, Reviewer가 CRITICAL 5건·WARNING 4건 발견 후 전부 수정.

**신규 문서**:
1. `docs/balance/progression-curve.md` — 진행 곡선 밸런스 시트 (Designer)
2. `docs/systems/progression-architecture.md` — 진행 시스템 기술 아키텍처 (Architect)
3. `docs/mcp/progression-tasks.md` — MCP 태스크 시퀀스 (Architect)

**수정된 문서**: `docs/design.md`, `docs/architecture.md`, `docs/pipeline/data-pipeline.md`, `docs/systems/project-structure.md`

### 핵심 설계 내용

**경험치(XP) 시스템**:
- 작물별 수확 XP: 감자 5 ~ 수박 25 (성장일수·해금 레벨 기반)
- 보조 XP: 호미질 2, 시설 건설 30, 가공 완성 5 등
- 물주기 XP 제거 결정 (어뷰징 방지)

**레벨 테이블** (baseXP=80, growthFactor=1.60):
- 레벨 7 누적 2,104 XP (~12시간), 레벨 10 누적 9,029 XP (~50시간)

**시뮬레이션 발견 문제 및 해결**:
- P-01: 원안에서 봄 1시즌에 레벨 6 도달 → baseXP/growthFactor 상향으로 해결
- P-02: 물주기 XP가 전체의 35~40% → 물주기 XP 제거로 해결

### 리뷰 결과

**CRITICAL 5건 (수정 완료)**:
1. data-pipeline.md에 구버전 XP 테이블 독립 기재 → 참조로 교체
2. MCP 문서 2곳에 구버전 XP 배열 하드코딩 → 최종값으로 교체
3. project-structure.md의 Level/ 클래스명 불일치 → 전면 교체
4. UnlockSaveData 필드 누락 → 3개 필드 추가
5. GameSaveData milestones 필드 누락 → 추가

**WARNING 4건 (수정 완료)**:
1. LevelConfig → ProgressionData 명칭 전환 (7개 위치)
2. LevelBarUI.cs 누락 → project-structure.md에 추가
3. MCP 태스크 플레이스홀더 → 실제 수치로 교체
4. CalculateHarvestExp 공식-데이터 불일치 → 우선순위 로직 추가

**PATTERN-006 신규 등록**: MCP 태스크 문서가 canonical 수치를 배열로 직접 기재하는 반복 패턴

---

## 의사결정 기록

1. **물주기 XP 제거**: 시뮬레이션에서 물주기 XP가 전체의 35~40%를 차지하여 "의미 있는 선택"이 아닌 반복 작업에 과도한 보상이 됨. 제거 후 레벨 곡선이 자연스러워짐.
2. **baseXP=80, growthFactor=1.60**: 초안(50, 1.55)에서는 봄 1시즌에 레벨 6 도달하는 문제 발생. 상향 조정으로 레벨 3~4 사이에 자연스러운 정체 구간 형성.
3. **LevelConfig → ProgressionData**: 기존 SO 이름을 확장된 기능에 맞게 변경. 해금 테이블과 마일스톤을 하나의 SO에 통합.

---

## 다음 단계

- DES-005 / ARC-006: 인벤토리/아이템 시스템 (Priority 3)
- CON-001~003: 콘텐츠 상세 (Priority 2)

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
