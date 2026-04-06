# Devlog #024 — NPC/상점 MCP 태스크 시퀀스 (ARC-009)

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### ARC-009: NPC/상점 MCP 태스크 시퀀스 독립 문서화

`npc-shop-architecture.md`(ARC-008)의 섹션 8 Phase A~F 개요를 상세한 MCP 태스크 시퀀스로 확장. 총 ~166회 MCP 호출 예상.

**신규 문서**:
- `docs/mcp/npc-shop-tasks.md` — NPC/상점 시스템 MCP 태스크 시퀀스 (ARC-009)

**수정된 문서**:
- `docs/systems/npc-shop-architecture.md` — TravelingMerchantScheduler 스케줄 모델 전면 교체, SaveData 필드 수정, 폴더 구조 통합, Phase F-2 UpgradePanel 참조 수정
- `TODO.md` — ARC-009 DONE 처리

---

## 핵심 설계 내용

### npc-shop-tasks.md 구성

**총 MCP 호출 예상**: ~166회

| 태스크 | 내용 | 호출 수 |
|--------|------|--------|
| T-1 | 스크립트 16종 생성 | ~21회 |
| T-2 | SO 에셋 11종 생성 (NPCData 4 + TravelingPool 1 + DialogueData 6) | ~67회 |
| T-3 | DialoguePanel UI 프리팹 생성 | ~24회 |
| T-4 | 씬 배치 및 참조 연결 (NPC 3종 + 매니저 3종 + 동적 프리팹 1) | ~34회 |
| T-5 | 기존 시스템 연동 (ShopSystem/ToolUpgradeSystem) | 6회 |
| T-6 | 통합 테스트 시퀀스 | ~18회 |

**스크립트 목록 (S-01~S-16)**:
- S-01~S-04: NPCType, NPCActivityState, DayFlag, DialogueChoiceAction (enum)
- S-05~S-07: DialogueNode, DialogueChoice, TravelingShopCandidate (직렬화 클래스)
- S-08~S-09: NPCData, TravelingShopPoolData (SO 클래스)
- S-10: NPCSaveData + TravelingMerchantSaveData (직렬화 클래스)
- S-11: NPCEvents (static 이벤트 허브)
- S-12: DialogueData (SO 클래스)
- S-13: NPCManager (MonoBehaviour Singleton)
- S-14: NPCController (MonoBehaviour)
- S-15: TravelingMerchantScheduler (MonoBehaviour)
- S-16: DialogueUI (UI 컨트롤러)

---

## 리뷰 결과

**CRITICAL 1건 (수정 완료)**:
- [C-1] 씬 계층에서 `NPC_TravelingMerchant`(동적 오브젝트)와 `UpgradePanel`(ARC-015 범위) 누락 → G-09 동적 오브젝트 명시, 섹션 1.5 "선행 태스크 오브젝트" 신설

**WARNING 3건 (수정 완료)**:
- [W-1] `TravelingMerchantSaveData.cs` 독립 파일 vs. `NPCSaveData.cs` 통합 불일치 → architecture.md 섹션 6.1 통합 표기로 수정
- [W-2] 여행 상인 스케줄 모델 불일치 — canonical(`npcs.md`) 고정 토/일요일 방식 vs. 난수 주기 방식 → canonical 채택, architecture.md + tasks.md 스케줄러 로직 전면 교체
- [W-3] `DialogueUI._advanceButton` 필드 연결 누락 → Open Questions에 `_choicePrefab`과 함께 기재

---

## 의사결정 기록

1. **여행 상인 스케줄 모델**: npcs.md(canonical)의 고정 토/일 방식을 채택. `TravelingMerchantScheduler`는 난수 주기 대신 `DayFlag.Saturday | DayFlag.Sunday` 비트마스크 기반 `CheckVisitSchedule(currentDay, currentDayOfWeek)`로 재설계. 세이브 데이터도 `nextVisitDay`, `departureDayOffset` 삭제 → `isPresent`, `randomSeed`, `currentStockItemIds`, `currentStockQuantities` 4필드로 축소.

2. **UpgradePanel 책임 분리**: BlacksmithPanel(UpgradePanel)은 ARC-015 범위. ARC-009는 이를 참조 연결하는 역할만 담당. 두 MCP 태스크 문서 간 중복 작업 방지.

3. **DialogueData 중첩 배열 리스크**: `DialogueNode[] → DialogueChoice[]` 중첩 배열을 MCP `set_property`로 설정 불가능할 경우, Editor 스크립트(`CreateDialogueAssets.cs`) 우회 방안을 T-2에 명시.

---

## 미결 사항 ([OPEN])

- NPC 호감도 시스템 도입 여부 (NPCSaveData 확장 여부)
- 여행 상인 독점 아이템 종류 확정 (npcs.md CON-003 후속)
- DialogueUI `_choicePrefab` / `_advanceButton` 참조 연결 방식
- 목수 NPC BuildingManager 연동 상세 설계

---

## 후속 작업

- `ARC-012`: 세이브/로드 MCP 태스크 시퀀스
- `ARC-013`: 인벤토리 MCP 태스크 시퀀스
- `ARC-016`: 퀘스트 MCP 태스크 시퀀스
- `CON-004`: 대장간 NPC 상세 (캐릭터/대화/인터페이스 UX)

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
