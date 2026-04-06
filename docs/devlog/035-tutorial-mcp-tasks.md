# Devlog #035 — FIX-024 + ARC-010: 튜토리얼 MCP 태스크 문서 완성

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

업적 토스트 위치 수정(FIX-024)과 튜토리얼 시스템 MCP 태스크 시퀀스 독립 문서화(ARC-010)를 완료했다.

### 생성/수정된 파일

| 파일 | 내용 |
|------|------|
| `docs/systems/achievement-system.md` | 섹션 5.4 토스트 위치 "좌측 하단" → "상단 중앙" 4행 수정 (FIX-024) |
| `docs/systems/ui-system.md` | 섹션 7.2 [OPEN] 태그 제거, canonical 참조로 정리 (FIX-024 후속) |
| `docs/systems/tutorial-architecture.md` | Part I 섹션 6.1 + Part II MCP-2 Sort Order 100 → 40 수정 |
| `docs/mcp/tutorial-tasks.md` | 튜토리얼 MCP 태스크 시퀀스 독립 문서 — 신규 (ARC-010) |
| `TODO.md` | FIX-024·ARC-010 완료 처리, FIX-025 등록 |

---

## 주요 결정 사항

### FIX-024: 토스트 위치 통일

`achievement-system.md` 섹션 5.4의 "화면 좌측 하단" → "화면 상단 중앙" 수정. canonical 정의는 `ui-system.md` 섹션 7.2 + `achievement-architecture.md` 섹션 3.1 기준.

### ARC-010: tutorial-tasks.md 신규 작성

`tutorial-architecture.md` Part II(MCP-1~5 요약)를 독립 태스크 문서로 분리·확장했다. 주요 구조:

| 태스크 | 내용 | MCP 호출 |
|--------|------|---------|
| T-1 | TutorialManager/TriggerSystem/ContextHintSystem 배치 | ~30회 |
| T-2 | Canvas_Tutorial 프리팹 생성 (6개 패널) | ~64회 |
| T-3 | 메인 튜토리얼 12단계 SO 에셋 생성 | ~122회 |
| T-4 | 시스템 튜토리얼 4종 SO 에셋 생성 | ~45회 |
| T-5 | ContextHintData SO 7종 생성 | ~79회 |
| T-6 | 통합 테스트 | 12회 |
| **합계** | | **~352회** |

[RISK] 352회는 많다. T-3의 12개 Step SO 개별 생성이 주요 원인 — Editor 스크립트(CreateTutorialAssets.cs) 일괄 생성으로 ~50회 감소 가능.

---

## 리뷰 결과 및 수정

| 심각도 | 건수 | 처리 |
|--------|------|------|
| CRITICAL | 4건 | 4건 즉시 수정 완료 |
| WARNING | 6건 | 2건 수정, 4건 FIX-025/[OPEN] 등록 |
| INFO | 6건 | 확인 |

### 수정 내역

| 문제 | 수정 내용 |
|------|----------|
| MCP 호출 수 개요 테이블 부정확 | 실제 합계 ~352회로 업데이트 |
| G-02 TutorialManager 별도 GO — T-1-22과 불일치 | TutorialManager를 G-01(TutorialSystem)에 직접 부착으로 통일 |
| `FarmEvents.OnCropInfoViewed` 미정의 이벤트 | Step 08 completionType → ClickToContinue(2)로 임시 처리 + [OPEN] 등록 |
| Sort Order 100 언급 (이미 수정 완료 상태) | 관련 [RISK] 주석 제거/업데이트 |
| Canvas_Tutorial 부모 SCN_Farm root → `--- UI ---` | WARNING-3 수정 |

### FIX-025 등록

tutorial-tasks.md Step 07(`TimeEvents.OnSleepExecuted`)과 Step 11(구매 완료 이벤트)의 정확한 이벤트명이 canonical 아키텍처 문서에 미정의 상태. 해당 이벤트를 먼저 아키텍처에 추가한 후 SO 값을 확정해야 한다.

---

## 다음 작업 후보

| ID | Priority | 내용 |
|----|----------|------|
| ARC-012 | 2 | 세이브/로드 MCP 태스크 시퀀스 |
| VIS-001 | 2 | 비주얼 가이드 |
| BAL-003 | 2 | 겨울 작물 ROI 분석 |
| CON-004 | 2 | 대장간 NPC 상세 |
| FIX-025 | 2 | tutorial-tasks.md 이벤트명 확정 |

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
