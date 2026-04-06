# Devlog #040 — FIX-027 + ARC-021: NPCAffinityTracker 보완 및 시간/계절 MCP 태스크 문서화

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

이번 세션에서 두 가지 작업을 완료했다.

1. **FIX-027**: `blacksmith-architecture.md` NPCAffinityTracker 클래스 다이어그램에 메서드 4개 추가 (지난 세션 리뷰 INFO-1 후속)
2. **FIX-026 / ARC-021**: `docs/mcp/time-season-tasks.md` 신규 작성 — `time-season-architecture.md` 섹션 8의 MCP 구현 계획을 독립 문서로 상세화

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/mcp/time-season-tasks.md` | **신규** — ARC-021 시간/계절 MCP 태스크 시퀀스 (~126~312회 MCP 호출, Phase A~E) |
| `docs/systems/blacksmith-architecture.md` | NPCAffinityTracker 메서드 4개 추가 (FIX-027), AffinityEntry.triggeredDialogues bool[]→string[] 타입 수정 (CRITICAL-5), Part II Step A-4에 4개 메서드 명시 추가 |
| `docs/systems/time-season-architecture.md` | 섹션 8 "작성 예정"→ARC-021 완료로 업데이트, Cross-references 수정; 섹션 2.1/2.2/2.4/2.5 PATTERN-006 위반 수정 (수치 제거→참조 교체) |
| `docs/systems/save-load-architecture.md` | AffinityEntry 주석 `triggeredDialogues[]` → `triggeredDialogueIds[]` 동기화 |
| `TODO.md` | FIX-026/FIX-027 완료, ARC-021 추가, FIX-028 신규 추가 |

---

## time-season-tasks.md 구조 요약

| Phase | 내용 | MCP 호출 수 (직접/대안) |
|-------|------|------------------------|
| A | TimeManager 기본 (스크립트 3종 + SO_TimeConfig + GO 배치 + HUD 연결) | ~24 / ~24 |
| B | SeasonData 환경 연출 (SO 4종 + DayPhaseVisual 20세트 + EnvironmentController) | ~172 / ~54 |
| C | WeatherSystem (SO 4종 + 비/폭풍 연동 + 시드 재현 테스트) | ~64 / ~22 |
| D | FestivalManager (SO 4종 + 이벤트 테스트) | ~46 / ~20 |
| E | 통합 테스트 (전체 연동 + 저장/로드) | ~6 / ~6 |
| **합계** | | **~312 / ~126** |

Editor 스크립트 대안 전략 시 호출 수 ~126회로 감축 가능. Phase B의 DayPhaseVisual 20 세트 입력이 최대 병목이므로 Editor 스크립트 일괄 설정을 강력 권장.

---

## FIX-027 수정 내용

| 항목 | 내용 |
|------|------|
| NPCAffinityTracker 메서드 추가 | `HasTriggeredDialogue(npcId, dialogueId): bool` — 대화 중복 발동 방지 |
| | `MarkDialogueTriggered(npcId, dialogueId): void` — 대화 발동 기록 |
| | `CanGiveDailyAffinity(npcId): bool` — 일일 친밀도 상한 확인 |
| | `MarkDailyVisit(npcId): void` — 오늘 방문 기록 (자정 리셋) |
| AffinityEntry 타입 수정 | `triggeredDialogues: bool[]` → `string[] triggeredDialogueIds` (CRITICAL-5: string ID 키 불일치 해소) |

---

## 리뷰 수정 사항

| 항목 | 파일 | 내용 |
|------|------|------|
| CRITICAL-1 | time-season-architecture.md | 섹션 8 Phase B-1 sunColor/growthMultiplier 수치 → canonical 참조 교체 |
| CRITICAL-2 | time-season-architecture.md | 섹션 8 Phase A-2 SO_TimeConfig 수치 → canonical 참조 교체 |
| CRITICAL-3 | time-season-architecture.md | 섹션 8 Phase D-2 축제 날짜 → canonical 참조 교체 |
| CRITICAL-4 | blacksmith-architecture.md | Part II Step A-4에 4개 메서드 명시 추가 |
| CRITICAL-5 | blacksmith-architecture.md | AffinityEntry.triggeredDialogues bool[] → string[] 타입 수정 |
| WARNING-1~5 | time-season-architecture.md | 섹션 2.1/2.2/2.4/2.5 중복 수치/테이블 제거, canonical 참조로 교체 |

---

## 신규 TODO 항목

| ID | 내용 |
|----|------|
| FIX-028 | blacksmith-architecture.md AffinitySaveData 스키마 확장 검토 (triggeredDialogueIds, dailyVisitDates 필드 추가 — CRITICAL-5 후속) |

---

*이 문서는 Claude Code가 FIX-027 + ARC-021 태스크에 따라 자율적으로 작성했습니다.*
