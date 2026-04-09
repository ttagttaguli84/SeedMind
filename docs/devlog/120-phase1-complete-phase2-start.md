# Devlog 120 — Phase 1 완료 & Phase 2 전환

**날짜**: 2026-04-09  
**작성자**: Claude Code

---

## 세션 요약

### 완료된 작업

#### FIX-120: crop-growth-architecture.md 섹션 4.3 seasonBonus 수치 충돌 해소

- **문제**: seasonBonus 범위 선언 `1.0 ~ 1.1`과 표 내 `여름+과일류 1.2` 불일치
- **문제**: `온실 내 겨울 재배 0.8`과 canonical(crop-growth.md 섹션 2.4) `비계절 x0.85` 불일치
- **수정**: 파라미터 범위를 `(→ see crop-growth.md 섹션 2.4)` 참조로 교체
- **수정**: 계절 보너스 상세 표를 canonical 기준 7행으로 교체
  - 주 계절 x1.1, 부 계절 x1.0, 여름 ×1.05 추가 적용
  - 온실 비계절 x0.85, 온실 겨울 전용 x1.0 명시

#### PATTERN-011: MCP 태스크 에셋명 canonical 조회 규칙화

- **문제**: ARC-046(decoration-tasks.md)에서 에셋명 3건 임의 생성 발견
- **수정**: `doc-standards.md` PATTERN-011 규칙 추가 — 에셋명/SO ID는 반드시 canonical 콘텐츠 문서에서 직접 조회, 임의 생성 금지
- **수정**: `workflow.md` Reviewer Checklist 항목 15 추가 (총 15개)

---

## Phase 1 완료 선언

### 완료 조건 점검

| 조건 | 상태 |
|------|------|
| 모든 시스템 DES+ARC+MCP 3문서 존재 (26개 시스템) | ✅ |
| 활성 DES-*/ARC-* TODO 0개 | ✅ |
| 미처리 PATTERN-* 0개 | ✅ |
| 핵심 ARC 문서 구현 차단 [OPEN] 없음 | ✅ |

**→ 4개 조건 동시 충족. Phase 1 완료.**

### Phase 1 산출물 요약

| 카테고리 | 문서 수 | 주요 내용 |
|---------|--------|----------|
| 시스템 설계 (DES) | 26개 | 경작·성장·경제·인벤토리·퀘스트·업적·에너지 등 전체 시스템 |
| 기술 아키텍처 (ARC) | 26개 | Unity C# 클래스 다이어그램·SO 스키마·MCP 구현 계획 |
| MCP 태스크 시퀀스 | 26개 | ~1,200+ MCP 호출 시퀀스 |
| 밸런스 시트 (BAL) | 9개 | 작물/가공/퀘스트/에너지/연간 경제 ROI 분석 |
| 콘텐츠 스펙 (CON) | 10개 | 작물·시설·NPC·가공·목축·장식·음식 아이템 |
| 파이프라인/기타 | 5개 | SO 스키마·프로젝트 구조·진행 곡선 |

### 규칙 체계 완성

- PATTERN-001~011 모두 해소 → doc-standards.md + workflow.md에 규칙화
- Reviewer Checklist 15개 항목 완비
- Canonical Data Mapping 11개 항목

---

## Phase 2 전환

**시작 시점**: 2026-04-09  
**첫 작업**: `docs/mcp/scene-setup-tasks.md` — Unity 기본 씬 구성

### 변경 사항

- `README.md`: Phase 2 진행 중으로 업데이트
- `CLAUDE.md`: 현재 상태 Phase 2, 핵심 규칙 2번 수정 (Document-only → MCP 구현 시작)
- `workflow.md`: Document-Only Policy LIFTED 표시

---

## Cross-references

- `docs/mcp/scene-setup-tasks.md` — Phase 2 첫 번째 실행 대상
- `.claude/rules/doc-standards.md` — PATTERN-011 규칙 추가
- `.claude/rules/workflow.md` — Reviewer Checklist 항목 15 추가
