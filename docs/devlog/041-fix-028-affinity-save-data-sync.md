# Devlog #041 — FIX-028: AffinitySaveData 스키마 동기화

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

FIX-028 작업을 완료했다. FIX-027에서 `blacksmith-architecture.md`의 `AffinityEntry` 타입을 수정했으나 `blacksmith-tasks.md`의 코드 예시가 구버전으로 남아있던 문제를 해소했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/mcp/blacksmith-tasks.md` | T-1-03 AffinityEntry: `bool[] triggeredDialogues` → `string[] triggeredDialogueIds`; T-1-04 NPCAffinityTracker: `Dictionary<string, bool[]>` → `Dictionary<string, HashSet<string>>`, 메서드 시그니처 4개 canonical 동기화 |
| `docs/systems/blacksmith-architecture.md` | 섹션 1 NPCAffinityTracker 클래스 다이어그램에 `_lastVisitDayMap`·`_triggeredDialogueMap` 2개 상태 필드 추가; 섹션 5.5에 `dailyVisitDates` 불필요 결정 주석 추가 |
| `TODO.md` | FIX-028 완료 표시, ARC-010/ARC-012/BAL-003 중복 항목 정리, 신규 항목 3개 추가 |

---

## FIX-028 수정 내용 상세

### 문제

FIX-027에서 `blacksmith-architecture.md` 섹션 5.5 `AffinityEntry`를 수정했지만, `blacksmith-tasks.md` T-1-03·T-1-04 코드 예시는 업데이트되지 않았다.

| 위치 | 구버전 | 신버전 |
|------|--------|--------|
| T-1-03 AffinityEntry 필드 | `bool[] triggeredDialogues` | `string[] triggeredDialogueIds` |
| T-1-04 `_triggeredDialogueMap` 타입 | `Dictionary<string, bool[]>` | `Dictionary<string, HashSet<string>>` |
| T-1-04 메서드 시그니처 | `(npcId, level)` 매개변수 | `(npcId, dialogueId)` 로 수정 |

### dailyVisitDates 결정

FIX-028에서 검토한 `dailyVisitDates` 별도 필드는 불필요하다고 결론지었다.  
`AffinityEntry.lastVisitDay: int` 가 이미 `CanGiveDailyAffinity` / `MarkDailyVisit` 메서드의 일일 중복 방지 역할을 수행한다. 이 결정을 `blacksmith-architecture.md` 섹션 5.5에 주석으로 명시했다.

### 리뷰 후속 수정 (WARNING-1, WARNING-2)

리뷰어가 발견한 추가 불일치 2건을 즉시 수정했다:

| 항목 | 파일 | 수정 내용 |
|------|------|-----------|
| WARNING-1 | blacksmith-architecture.md 섹션 1 | NPCAffinityTracker `[상태]` 블록에 `_lastVisitDayMap`, `_triggeredDialogueMap` 누락 → 추가 |
| WARNING-2 | blacksmith-tasks.md T-1-03 | `lastVisitDay` 주석 "일일 대화 중복 방지" → "CanGiveDailyAffinity/MarkDailyVisit 중복 방지" canonical 표현으로 통일 |

---

## 세 문서 최종 동기화 상태

| AffinityEntry 필드 | blacksmith-architecture.md 5.5 | blacksmith-tasks.md T-1-03 | save-load-architecture.md |
|--------------------|-------------------------------|---------------------------|--------------------------|
| npcId: string | ✓ | ✓ | ✓ (트리) |
| affinityValue: int | ✓ | ✓ | ✓ (트리) |
| lastVisitDay: int | ✓ | ✓ | ✓ (트리) |
| triggeredDialogueIds: string[] | ✓ | ✓ | ✓ (트리) |

---

## 신규 TODO 항목

| ID | Priority | 내용 |
|----|----------|------|
| ARC-022 | 2 | UI 시스템 MCP 태스크 시퀀스 독립 문서화 (ui-architecture.md → docs/mcp/ui-tasks.md) |
| DES-012 | 1 | 농장 확장 시스템 설계 (구역 해금, 타일 구매) |
| CON-008 | 1 | 추가 NPC 상세 설계 (마을 상인/농업 전문가 등) |

---

*이 문서는 Claude Code가 FIX-028 태스크에 따라 자율적으로 작성했습니다.*
