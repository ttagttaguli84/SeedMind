# Devlog #020 — 가공품 경제 밸런스 (BAL-004) + 가공 MCP 태스크 (ARC-014) + PATTERN-008

> 2026-04-06 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### PATTERN-008 Self-Improve: 시설 문서 레시피 직접 기재 방지 규칙

FIX-005 세션에서 facilities.md 섹션 7~9에 레시피 목록을 직접 기재하다가 CRITICAL 3건이 발생한 패턴을 시스템 규칙으로 등록.

**수정된 문서**:
- `.claude/rules/doc-standards.md` — PATTERN-008 규칙 추가, Canonical 데이터 매핑 테이블에 "가공소별 레시피 목록" 행 추가
- `.claude/rules/workflow.md` — Reviewer Checklist 항목 12 추가 (PATTERN-008 검증)
- `logs/reports/self_improve_PATTERN008.md` — 분석 보고서 신규 생성

### BAL-004: 가공품 ROI/밸런스 분석

32종 레시피 전수 분석, 가공 체인 ROI, 가공 vs 직판 비교 완료.

**신규 문서**:
- `docs/balance/processing-economy.md` — 가공품 경제 밸런스 시트

### ARC-014: 가공 시스템 MCP 태스크 시퀀스

processing-architecture.md의 Part II를 독립 상세 문서로 분리. (※ 당초 ARC-013 ID였으나 quest-architecture.md와 충돌 → ARC-014로 재지정)

**신규 문서**:
- `docs/mcp/processing-tasks.md` — 가공 시스템 MCP 태스크 시퀀스 (ARC-014)

**수정된 문서**:
- `docs/systems/processing-architecture.md` — ProcessingType enum 6종 완성 (Mill/Fermentation/Bake 추가), ARC-014 참조 갱신
- `docs/devlog/018-processing-system.md` — ARC-014 ID 참조 갱신
- `docs/devlog/019-quest-system.md` — ARC-014 참조 갱신

### FIX-006 (리뷰 W-3 후속): facilities-architecture.md ProcessingType enum 보완

- `docs/systems/facilities-architecture.md` — ProcessingType enum에 Mill/Fermentation/Bake 3종 추가 (data-pipeline.md canonical 6종과 일치)

---

## 핵심 설계 내용

### BAL-004 주요 분석 결과

**시설별 최고 ROI 레시피**:

| 시설 | 최고 ROI 레시피 | 순이익/일 |
|------|----------------|----------|
| 가공소 | 블루베리 잼 | ~1,200G/일 |
| 발효실 | 호박 장아찌 | ~2,400G/일 |
| 베이커리 | 호박 파이 | ~5,329G/일 |
| 제분소 | 호박 분말 | ~800G/일 (단독 가치 낮음) |

**주요 RISK 식별 (5건)**:
1. [RISK-1] 가공이 모든 품질에서 직판보다 우월 → 가공 배수 하향 조정 검토
2. [RISK-2] 딸기 와인 효율 부족 (잼 대비 1/15 수익)
3. [RISK-3] 호박 파이 과도한 수익 (연중 보관 후 베이커리 직행 루트)
4. [RISK-4] 절임류 존재 이유 부족 (잼과 동시 가능 작물에서 절임 선택 유인 없음)
5. [RISK-5] 제분소 독립 가치 부족 (Lv.5~8 구간에서 호박 분말 외 용도 없음)

**가공 체인 최고 ROI**: 호박 파이 체인 (창고 보관 → 베이커리, 창고 없이도 직접 가공 시 2,980G/일)

### ARC-014 주요 내용

**총 MCP 호출 추정**: ~651회 (Editor 스크립트 우회 시 ~139회)
- P-2(레시피 SO 32종 생성)가 517회(79%)로 압도적 비중 → Editor 스크립트 일괄 생성 강력 권고

**태스크 구성**: P-1~P-13 (SO 에셋 → 스크립트 → 씬 배치 → 통합 테스트)

**핵심 의존성**: FarmTileSystem, InventoryManager, BuildingManager, EconomyManager

---

## 리뷰 결과

**CRITICAL 2건 (수정 완료)**:
1. [C-1] ARC-013 ID 중복 — processing-tasks.md가 quest-architecture.md와 동일 ID 사용 → ARC-014로 재지정, 연관 4개 문서 전부 갱신
2. [C-2] processing-architecture.md의 ProcessingType enum 미완성 (3종) — data-pipeline.md canonical(6종)과 불일치 → Mill/Fermentation/Bake 추가

**WARNING 3건 (수정 완료)**:
1. [W-1] processing-tasks.md P-2-02 잼 레시피 수 오류 (7→8종), MCP 호출 수 재산
2. [W-2] processing-tasks.md P-2-04 절임 레시피 수 오류 (8→7종), MCP 호출 수 재산
3. [W-3] facilities-architecture.md ProcessingType enum 미갱신 (3종) → 6종으로 확장

---

## 의사결정 기록

1. **ARC-013 → ARC-014 재지정**: quest-architecture.md가 먼저 ARC-013으로 발행되었으므로 후발 문서가 ID를 양보. TODO.md, devlog, architecture 참조 전부 업데이트하여 이력 추적 가능성 유지.

2. **PATTERN-008 즉시 규칙화**: 이번 세션에서 처리한 패턴 중 가장 빈번히 재발할 위험성이 높음. 시설 문서는 레시피 "슬롯 수·연료 타입·처리 속도 배율"만 기재하고 레시피 내용은 100% canonical 참조로 통일.

3. **Editor 스크립트 우회 권고**: 레시피 SO 32종을 하나씩 MCP 호출로 생성하면 512회 이상의 호출이 발생. Phase 2 구현 단계에서 Editor 스크립트(CreateAllRecipes 등)를 먼저 생성하여 1~2회 실행으로 모든 에셋을 일괄 생성하는 방식이 효율적.

4. **베이커리/호박 파이 밸런스 경고 등록**: 순이익 5,329G/일은 게임 초반 골드 수급과 비교할 때 과도. 베이커리 해금이 Lv.10+ 후반임을 감안해도 조정 필요 가능성 있음. BAL-004에 RISK 등록하여 향후 플레이테스트 시 검증.

---

## 미결 사항 ([OPEN])

- 가공 배수 조정 파급 영향 분석 (processing-economy.md RISK-1 후속)
- 겨울 작물 3종(겨울무/시금치/표고버섯) 가공 ROI 확정 (BAL-003 선행)
- 절임류 역할 재정립 방안 (별도 효과 부여 또는 NPC 선호 차별화)
- 제분소 밀가루 레시피 추가 (DES-010 밀 작물 확장 이후)

---

## 후속 작업

- `BAL-003`: 겨울 작물 3종 ROI 분석 (crop-economy.md 추가)
- `ARC-007`: 시설 MCP 태스크 시퀀스 (facilities-tasks.md)
- `ARC-012`: 세이브/로드 MCP 태스크 시퀀스 (save-load-tasks.md)
- `VIS-001`: 비주얼 가이드 (로우폴리 스타일, 색상 팔레트)

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
