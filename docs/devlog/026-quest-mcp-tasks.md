# Devlog #026 — 퀘스트 MCP 태스크 시퀀스 + FIX-008/009

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### FIX-008: inventory-system.md 도구 ID 표기 통일

`inventory-system.md` 섹션 5.3과 섹션 7의 도구 아이템 ID를 canonical 형식(`{type}_{tier}`)으로 통일.

| 변경 전 | 변경 후 |
|---------|---------|
| `tool_hoe` | `hoe_basic` |
| `tool_wateringcan` | `wateringcan_basic` |
| `tool_sickle` | `sickle_basic` |
| `tool_axe` | `axe_basic` |

섹션 7 명명 규칙 테이블도 `tool_` 접두사 없는 `{type}_{tier}` 형식으로 업데이트. "도구 등급은 ID에 포함하지 않는다"는 구 variant 규칙을 삭제하고 `_basic`/`_reinforced`/`_legendary` 3단계 tier suffix 포함 규칙으로 교체.

### FIX-009: data-pipeline.md ShippingBinSaveData 스키마 추가

`data-pipeline.md`에 출하함 세이브 데이터 구조를 추가:
- 섹션 1.2 동적 데이터 테이블에 `ShippingBinSaveData` 항목 추가
- 섹션 3.2 최상위 세이브 스키마에 `"shippingBin": {}` 필드 추가
- 섹션 3.3에 `ShippingBinSaveData` JSON 스키마 + 필드 설명 추가

`pendingItems[]` 배열로 정산 전 아이템을 저장하고, 매일 06:00 정산 후 빈 배열로 초기화하는 구조.

### ARC-016: 퀘스트 MCP 태스크 시퀀스 독립 문서화

`docs/mcp/quest-tasks.md` 신규 작성. `quest-architecture.md` Part II의 Step 1~5 요약을 상세한 MCP 호출 명세로 확장.

**총 MCP 호출 예상**: ~181회

| 태스크 | 내용 | 호출 수 |
|--------|------|--------|
| T-1 | 스크립트 20종 생성 (enum 6종, Serializable 4종, SO 1종, 시스템 7종, UI 2종) | 24회 |
| T-2 | SO 에셋 20종 (메인 퀘스트 4 + 일일 목표 풀 12 + 농장 도전 4) | ~72회 |
| T-3 | UI 프리팹 (QuestLogPanel, QuestTrackingWidget, QuestCompletePopup) | ~38회 |
| T-4 | 씬 배치 및 참조 연결 (QuestManager GO, SO 배열 설정) | ~18회 |
| T-5 | 기존 시스템 연동 (asmdef, GameSaveData 확장, J키 입력 바인딩) | 5회 |
| T-6 | 통합 테스트 5시나리오 (해금, 진행도, 일일 목표, 보상, 세이브/로드) | ~24회 |

**스크립트 컴파일 순서**: S-01~S-12 → S-13 (QuestEvents) → S-14~S-18 (시스템 클래스) → S-19~S-20 (UI)

---

## 리뷰 결과

**CRITICAL 2건 (수정 완료)**:
- [C-1] `data-pipeline.md` 섹션 3.3 InventorySaveData toolbarSlots의 도구 ID가 구 형식(`tool_hoe_basic`, `tool_watering_can_copper`, `tool_sickle_basic`) → FIX-008 형식(`hoe_basic`, `wateringcan_basic`, `sickle_basic`)으로 교체
- [C-2] PATTERN-005 위반 — `data-pipeline.md` Part II에 ShippingBinSaveData C# 클래스 정의 누락 → Part II 섹션 2.6에 추가

**WARNING 3건 (수정 완료)**:
- [W-1] `data-pipeline.md` 섹션 3.4 세이브 파일 크기 표에 ShippingBinSaveData 항목 누락 → ~500 bytes 추가, 총계 ~46 KB 업데이트
- [W-2] `data-pipeline.md` 섹션 2.5에 동일 [OPEN] 항목 중복 기재 → 중복 삭제
- [W-3] `quest-tasks.md` 스크립트 경로와 `quest-architecture.md` Step 1-6 경로 불일치 → quest-architecture.md 경로를 `Quest/Data/`로 통일

---

## 의사결정 기록

1. **ShippingBinSaveData 단일 구조체**: 출하함이 2개(레벨 6 해금)여도 판매 내용물을 하나의 `ShippingBinSaveData`로 통합 관리. 2번째 출하함의 월드 배치 위치만 `BuildingSaveData`에 저장. 게임 내 출하 내용물은 어느 함에 넣든 같은 날 함께 정산되므로 분리 불필요.

2. **ARC-016 NPC 의뢰 SO 미포함**: 봄 메인 퀘스트 4종 + 일일 목표 풀 12종 + 농장 도전 초반 4종만 이번 태스크에 포함. NPC 의뢰 SO 에셋(수락 인터페이스 미정, `NPCEvents.OnItemDelivered` 미정의)은 NPC 시스템 구현 완료 후 별도 태스크로 추가.

---

## 미결 사항 ([OPEN])

- `NPCEvents.OnItemDelivered` 이벤트 미정의 (quest-architecture.md OPEN 반영)
- `save-load-architecture.md` GameSaveData 루트 클래스에 `quest` 필드 추가 필요 (PATTERN-005)
- NPC 의뢰 QuestData SO 에셋 생성 (별도 태스크)

---

## 후속 작업

- `ARC-012`: 세이브/로드 MCP 태스크 시퀀스
- `ARC-010`: 튜토리얼 MCP 태스크 시퀀스
- `BAL-006`: 퀘스트/미션 보상 밸런스 분석
- `DES-010`: 도전 과제/업적 시스템 설계

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
