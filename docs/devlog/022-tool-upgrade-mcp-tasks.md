# Devlog #022 — 도구 업그레이드 MCP 태스크 시퀀스 (ARC-008 → ARC-015)

> 2026-04-06 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### ARC-008 (→ ARC-015): 도구 업그레이드 MCP 태스크 시퀀스 독립 문서화

tool-upgrade-architecture.md의 Part II를 독립 상세 문서로 분리·확장. 문서 ID 충돌(ARC-008은 npc-shop-architecture.md에 이미 배정됨)로 ARC-015로 재배정.

**신규 문서**:
- `docs/mcp/tool-upgrade-tasks.md` — 도구 업그레이드 MCP 태스크 시퀀스 (ARC-015)

**수정된 문서**:
- `docs/systems/tool-upgrade-architecture.md` — SmithyUI/SmithyPanel → BlacksmithPanelUI/BlacksmithPanel 전체 통일 (섹션 9.1, 9.2, Phase D, 이벤트 표)
- `TODO.md` — ARC-008 DONE 처리, 신규 항목 5개 추가
- `docs/devlog/014-tool-upgrade-system.md` — 후속 작업 ARC-008 → ARC-015 수정
- `docs/devlog/021-facilities-mcp-tasks.md` — 후속 작업 ARC-008 → ARC-015 수정

---

## 핵심 설계 내용

### tool-upgrade-tasks.md 구성

**총 MCP 호출 예상**: ~172회

| 태스크 | 내용 | 호출 수 |
|--------|------|--------|
| T-1 | ToolData SO 에셋 생성 (9종 + 재료 SO 2종) | 98회 |
| T-2 | 스크립트 8종 생성 (데이터/유틸/시스템/UI) | 14회 |
| T-3 | 대장간 UI 프리팹 생성 (BlacksmithPanel 3탭) | 32회 |
| T-4 | 씬 배치 (ToolUpgradeSystem + 참조 연결) | 12회 |
| T-5 | 통합 테스트 시퀀스 | 16회 |

**스크립트 목록 (T-01~T-08)**:
- T-01: ToolSpecialEffect (enum)
- T-02: PendingUpgrade (데이터 구조체)
- T-03: ToolUpgradeInfo / UpgradeCheckResult / ToolUpgradeFailReason (결과 타입)
- T-04: ToolUpgradeSaveData (직렬화)
- T-05: ToolEffectResolver (static 유틸)
- T-06: ToolUpgradeEvents (static 이벤트 버스)
- T-07: ToolUpgradeSystem (MonoBehaviour 핵심 로직)
- T-08: BlacksmithPanelUI (UI 컨트롤러)

**ToolData SO 9종**: 호미/물뿌리개/낫 × 기본/강화/전설 3등급, nextTier 체인 연결

---

## 리뷰 결과

**CRITICAL 2건 (수정 완료)**:
1. [C-1] 문서 ID 중복: tool-upgrade-tasks.md가 ARC-008을 사용했으나 npc-shop-architecture.md에 이미 배정됨 → ARC-015로 재배정, 관련 devlog 2개 및 TODO.md 일괄 수정
2. [C-2] UI 스크립트 이름 불일치: tool-upgrade-architecture.md에 `SmithyUI`/`SmithyPanel` 잔재 → `BlacksmithPanelUI`/`BlacksmithPanel`로 전체 통일

**WARNING 2건 (수정 완료)**:
1. [W-1] 전설 낫 `ToolSpecialEffect` 단일 enum 값으로 다중 효과 표현 불가 → T-1-10에 `[OPEN]` 태그 추가, Open Questions 항목 5번 등록 (해결 방안 3가지 제시)
2. [W-2] `ToolUpgradeSlotUI` 스크립트 T-08 목록 누락 → 섹션 1.2에 `[OPEN]` 주석으로 불일치 명시

**INFO 항목**: Reviewer Checklist 항목 1~12 전원 통과 (해당 없는 항목 N/A 처리)

---

## 의사결정 기록

1. **문서 ID ARC-015 배정**: ARC-008~ARC-014 중 ARC-013이 미사용이어서 ARC-013을 인벤토리 MCP 태스크로 배정하고, 도구 업그레이드는 ARC-015로 이동. 향후 MCP 태스크 시퀀스 문서에는 생성 전 ID 충돌 여부를 먼저 확인하는 선행 절차 필요.

2. **UI 명칭 BlacksmithPanelUI로 통일**: NPC 캐릭터명(Blacksmith)과 일관된 네이밍 채택. facilities-tasks.md의 `BuildingShopPanel` 패턴과 동일하게 Panel 접미어 사용.

3. **ToolSpecialEffect Flags enum 방향 제안**: 전설 낫의 3가지 효과(보너스 수확 + 품질 상승 + 씨앗 회수)를 단일 필드로 표현하려면 `[Flags]` 어트리뷰트를 활용하는 것이 가장 깔끔. 단 비트마스크 값 배정이 필요하며 FIX-007에서 확정.

---

## 미결 사항 ([OPEN])

- ToolData SO 에셋 수 확정 (현재 9종 — 혼합 등급 도구 추가 여부)
- MaterialItemData SO 별도 클래스 필요 여부 (vs ItemData 공용 클래스 재사용)
- ToolType enum 값 확인 (inventory-system.md와 일치 여부)
- 대장간 ShopData SO 필요 여부 (재료 구매 탭 구현 방식)
- **ToolSpecialEffect enum 확장 방안** (FIX-007 → tool-upgrade-architecture.md 반영)

---

## 후속 작업

- `FIX-007`: ToolSpecialEffect enum 다중 효과 처리 방안 확정
- `ARC-009`: NPC/상점 MCP 태스크 시퀀스 (npc-shop-tasks.md)
- `ARC-010`: 튜토리얼 MCP 태스크 시퀀스 (tutorial-tasks.md)
- `ARC-012`: 세이브/로드 MCP 태스크 시퀀스 (save-load-tasks.md)
- `ARC-013`: 인벤토리 MCP 태스크 시퀀스 (inventory-tasks.md)
- `ARC-016`: 퀘스트 MCP 태스크 시퀀스 (quest-tasks.md)

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
