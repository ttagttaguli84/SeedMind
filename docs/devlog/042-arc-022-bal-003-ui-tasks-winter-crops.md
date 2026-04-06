# Devlog #042 — ARC-022 + BAL-003: UI MCP 태스크 시퀀스 & 겨울 작물 밸런스

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

ARC-022(UI 시스템 MCP 태스크 시퀀스 독립 문서화)와 BAL-003(겨울 작물 3종 ROI/밸런스 분석)을 병렬로 완료했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/mcp/ui-tasks.md` | 신규 생성 — UI 시스템 MCP 태스크 시퀀스 (~130회 MCP 호출, 8개 Phase) |
| `docs/systems/ui-architecture.md` | Cross-references에 `docs/mcp/ui-tasks.md (ARC-022)` 링크 추가 |
| `docs/balance/crop-economy.md` | 섹션 4.3 추가 — 겨울 작물 3종 경제 분석 (BAL-003) |
| `TODO.md` | ARC-022/BAL-003 완료 표시, BAL-010 신규 추가 |

---

## ARC-022: UI 시스템 MCP 태스크 시퀀스

`docs/systems/ui-architecture.md`의 MCP 구현 내용을 독립 문서 `docs/mcp/ui-tasks.md`로 분리했다.

### 문서 구성 (8개 Phase)

| Phase | 내용 | MCP 호출 수 |
|-------|------|------------|
| T-1 | 스크립트 생성 (enum 4종, 추상 클래스 2종, 시스템 3종, Screen/HUD 등 18종) | ~22회 |
| T-2 | Canvas 계층 생성 (6개 Canvas 계층) | ~18회 |
| T-3 | UIManager 코어 GameObject 배치 | ~17회 |
| T-4 | HUD 구조 배치 (TopBar, ToolbarContainer 등) | ~16회 |
| T-5 | Screen 프리팹 생성 (MenuScreen, SaveLoadScreen) | ~20회 |
| T-6 | 알림 시스템 (ToastContainer, ToastUI 프리팹) | ~14회 |
| T-7 | Screen 등록 및 ScreenType 설정 | ~12회 |
| T-8 | 통합 테스트 (FSM, 팝업 큐, HUD 갱신) | ~11회 |
| **합계** | | **~130회** |

### 리뷰 수정 사항 (WARNING 3건)

| 항목 | 수정 내용 |
|------|-----------|
| T-1 MCP 호출 수 | 12회 → ~22회 (실제 스크립트 수와 정합) |
| T-3 MCP 호출 수 | 10회 → ~17회 (GameObject 수와 정합) |
| 의존성 테이블 | `BAL-002` → `BAL-002-MCP`로 수정 |

---

## BAL-003: 겨울 작물 3종 ROI/밸런스 분석

`docs/balance/crop-economy.md` 섹션 4.3에 겨울 전용 작물 3종(겨울무, 표고버섯, 시금치) 분석을 추가했다.

### 핵심 결과

| 작물 | 유형 | 타일당 일일 효율 | 계절 순이익 |
|------|------|-----------------|------------|
| 겨울무 | 단일 수확 | (→ see crop-economy.md 4.3.1) | (→ see crop-economy.md 4.3.3) |
| 시금치 | 단일 수확 | (→ see crop-economy.md 4.3.1) | (→ see crop-economy.md 4.3.3) |
| 표고버섯 | 다중 수확 (6회) | 13.2G/일 (최고) | (→ see crop-economy.md 4.3.3) |

### 밸런스 이슈 식별 (3건)

| ID | 심각도 | 내용 |
|----|--------|------|
| B-09 | 높음 | 겨울 전용 작물이 온실 딸기 재배(21.4G/일) 대비 경쟁력 없음 — 겨울 시즌 활용 동기 약화 우려 |
| B-10 | 중간 | 표고버섯(13.2G/일)이 겨울 전용 작물 내 지배적 — 겨울무/시금치 선택 유인 부족 |
| B-11 | 낮음 | 겨울무 ROI(125%)가 봄/여름 작물(94~100%) 대비 과도하게 높음 |

B-09는 심각도 "높음"으로, 후속 작업 **BAL-010**(겨울 전용 작물 온실 경쟁력 조정)을 TODO에 추가했다.

---

## 신규 TODO 항목

| ID | Priority | 내용 |
|----|----------|------|
| BAL-010 | 2 | 겨울 전용 작물 온실 경쟁력 조정 (B-09 후속 — 제안 E 설계 확정) |

---

*이 문서는 Claude Code가 ARC-022 + BAL-003 태스크에 따라 자율적으로 작성했습니다.*
