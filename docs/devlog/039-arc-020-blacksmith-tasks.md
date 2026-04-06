# Devlog #039 — ARC-020: 대장간 NPC MCP 태스크 시퀀스 독립 문서화

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

ARC-020 태스크를 완료했다. 지난 세션(#038)에서 CON-004 대장간 NPC 상세 설계를 마친 데 이어, `blacksmith-architecture.md`의 Part II MCP 구현 계획을 독립 문서 `docs/mcp/blacksmith-tasks.md`로 분리·상세화했다. 디자이너·아키텍트 에이전트를 병렬로 구동하여 문서 작성 및 아키텍처 검증을 동시에 수행한 뒤, 리뷰어 에이전트로 CRITICAL 3건·WARNING 2건을 검출하고 전량 수정했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/mcp/blacksmith-tasks.md` | **신규** — ARC-020 MCP 태스크 시퀀스 (~156회 MCP 호출, T-1~T-6 6단계) |
| `docs/mcp/tool-upgrade-design-analysis.md` | 기존 미추적 파일 — 이번 커밋에 포함 |
| `docs/systems/save-load-architecture.md` | `AffinitySaveData` 필드 추가 (트리/JSON/C# 3곳, 필드 수 17→18) |
| `docs/systems/blacksmith-architecture.md` | JSON 수치 4개 canonical 참조 교체, C# 참조 수정, Phase C Step C-1 오브젝트 생성 방식 수정, Cross-references 추가 |
| `TODO.md` | ARC-020 완료, FIX-027 신규 추가 |

---

## blacksmith-tasks.md 구조 요약

| Phase | 내용 | MCP 호출 수 |
|-------|------|------------|
| T-1 | 스크립트 생성 10종 (enum, SO 클래스, FSM, UI 클래스) | 14회 |
| T-2 | SO 에셋 생성 (BlacksmithNPCData 1종 + DialogueData 10종) | 58회 |
| T-3 | NPC_Blacksmith 프리팹 확장 + InteractionZone 배치 | 12회 |
| T-4 | ToolUpgradeScreen UI 계층 구성 | 38회 |
| T-5 | 씬 배치 및 참조 연결 (NPCAffinityTracker, UIManager 등록) | 14회 |
| T-6 | 통합 테스트 시퀀스 13단계 | 20회 |
| **합계** | | **~156회** |

---

## 주요 결정 사항

### 아키텍트 에이전트 검증 결과 (4개 [OPEN] 항목 해소)

| [OPEN] | 결론 |
|--------|------|
| ToolData SO 구조 | `tool-upgrade-architecture.md`에서 이미 **방식 B (등급별 별도 SO + nextTier 체인)** 채택 확정. farming-tasks.md T1 기본 SO에서 시작하여 ARC-015가 등급별 체인 추가 |
| NPC 공통 인터랙션 시스템 | ARC-009(npc-shop-tasks.md)에서 NPCController/DialogueSystem/영업시간 체크 전부 구현됨. ARC-020은 대장간 **고유** 로직만 담당 |
| 마을 씬 구조 | project-structure.md 확인 — 단일 SCN_Farm, 별도 VillageScene 없음. NPC는 `--- NPCs ---` 계층에 배치 |
| ToolUpgradeSaveData | save-load-architecture.md에 이미 포함됨 (PlayerSaveData.toolUpgradeState) |

### 리뷰 수정 사항

| 항목 | 수정 내용 |
|------|----------|
| CRITICAL-1 | save-load-architecture.md GameSaveData에 `affinity: AffinitySaveData` 필드 추가 (3곳 동기화) |
| CRITICAL-2 | blacksmith-architecture.md JSON 예시에서 수치 4개(친밀도 증가량·임계값·할인율) → canonical 참조로 교체 |
| CRITICAL-3 | blacksmith-architecture.md C# 주석·섹션 5.2/5.3의 참조가 `npcs.md`(수치 없음)를 가리킴 → `blacksmith-npc.md 섹션 2.5`로 수정 |
| WARNING-1 | blacksmith-architecture.md Part II Phase C Step C-1: `create_object → "BlacksmithNPC"` → `add_component → NPC_Blacksmith` (ARC-009 이미 생성됨) |
| WARNING-2 | blacksmith-architecture.md Cross-references에 `blacksmith-npc.md(CON-004)` 추가 |

---

## 신규 TODO 항목

| ID | 내용 |
|----|------|
| FIX-027 | blacksmith-architecture.md NPCAffinityTracker 클래스 다이어그램에 메서드 4개 추가 (INFO-1 후속) |

---

*이 문서는 Claude Code가 ARC-020 태스크에 따라 자율적으로 작성했습니다.*
