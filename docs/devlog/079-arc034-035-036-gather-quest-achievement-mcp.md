# Devlog #079 — ARC-034/035/036: 채집 퀘스트·업적·SO 태스크 동기화

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

채집 시스템(CON-013, ARC-033) 완결 후 남아 있던 downstream ARC 3건을 병렬 처리했다. 퀘스트 아키텍처 enum 동기화, 업적 MCP 태스크 섹션 신규 추가, 채집 SO 에셋 생성 태스크 상세화가 목표였다. 리뷰 2건의 CRITICAL 이슈를 발견·수정하여 완결했다.

### 수정된 파일

| 파일 | 변경 내용 | 작업 |
|------|-----------|------|
| `docs/systems/quest-architecture.md` | QuestObjectiveType Fish=12/Gather=13 추가, 이벤트 핸들러 2종, Cross-references 갱신 | ARC-034 |
| `docs/mcp/quest-tasks.md` | T-1-04 ObjectiveType 갱신, T-1-15 SubscribeAll 갱신, Risks 갱신 | ARC-034 |
| `docs/mcp/achievement-tasks.md` | T-7 섹션 신규(A-031~A-035 채집 업적 5종 MCP), AchievementCategory Angler/Gatherer 추가, SubscribeAll 10→12이벤트, T-5-03 테이블 12행 | ARC-035 + 리뷰 수정 |
| `docs/systems/achievement-architecture.md` | AchievementCategory Hidden=6 이후 Angler=7/Gatherer=8 추가, 스크립트 목록 7→9개, 에셋 폴더 구조 Angler/Gatherer 추가 | 리뷰 CRITICAL 수정 |
| `docs/mcp/gathering-tasks.md` | G-C 섹션 상세화: SO 에셋 60개(Config+Item 27+Point 5+Price 27), ~136→~220회 MCP 호출 | ARC-036 |

---

## ARC-034: QuestObjectiveType Gather/Fish 추가

CON-013에서 채집 퀘스트 5종이 추가되었으나 quest-architecture.md의 `QuestObjectiveType` enum에 `Gather`가 누락된 상태였다. 점검 과정에서 `Fish = 12`도 같이 누락되어 있음을 확인, 두 값을 함께 추가했다.

- `quest-architecture.md` 섹션 2.3: `Fish = 12`, `Gather = 13` 추가
- 섹션 3.3: `OnFishCaught`, `OnItemGathered` 핸들러 메서드 2개 추가
- 섹션 6.3: 이벤트-핸들러 매핑 테이블 2행 추가
- `quest-tasks.md` T-1-04: enum 동기화; T-1-15: SubscribeAll 구독 추가

---

## ARC-035: 채집 업적 MCP 태스크 T-7 섹션 신규 추가

`achievement-tasks.md`에 채집 업적 A-031~A-035 5종에 대한 SO 에셋 생성·이벤트 연결 태스크 섹션 T-7을 신규 추가했다. 낚시 업적(T-6-계열) 패턴을 그대로 적용했다.

- **태스크 구성**: T-7-01 폴더 생성(1회) + T-7-02~06 SO 에셋 각 16~28회 + T-7-07 참조 연결(7회) = ~80회 MCP 호출
- `AchievementCategory` enum에 `Angler=7`, `Gatherer=8` 추가 (T-1-02)
- `AchievementConditionType` enum에 GatherCount=15~GatherSickleUpgraded=17 추가 반영 (T-1-03)
- 총 MCP 호출 수 ~548 → ~628회로 갱신

---

## ARC-036: gathering-tasks.md G-C 상세화

`data-pipeline.md` 섹션 2.10~2.12(ARC-033)에 등록된 3종 SO 스키마(GatheringPointData 12필드, GatheringItemData 16필드, GatheringConfig 14필드)를 MCP 태스크로 반영했다. 기존 G-C는 필드 수와 에셋 수가 미명시 상태였다.

- **에셋 구성**: GatheringConfig 1개 + GatheringItemData 27종 + GatheringPointData 5종 + PriceData 27종 = 60개
- **MCP 호출**: ~40 → ~124회 (G-C 단독), 문서 전체 ~136 → ~220회
- [RISK] G-C 단독 56%의 호출 비중 — Editor 스크립트 우회 시 ~5회로 압축 가능

---

## 리뷰 CRITICAL 2건 수정

리뷰어가 ARC-035 이후 `achievement-architecture.md`와 `achievement-tasks.md`의 불일치를 지적했다.

| ID | 파일 | 이슈 | 수정 |
|----|------|------|------|
| CRITICAL-1 | achievement-tasks.md T-1-11 | SubscribeAll() 주석 10이벤트, Debug.Log "10 events" — GatheringEvents 2종 누락 | 주석 2줄 추가, "12 events"로 갱신, T-5-03 테이블 11~12행 추가 |
| CRITICAL-2 | achievement-architecture.md 섹션 2.2 | AchievementCategory enum이 Hidden=6으로 끝남 — Angler/Gatherer 미추가, 스크립트 목록 7개 값 그대로 | Angler=7/Gatherer=8 추가, 9개 값, 에셋 폴더 구조 동기화 |

---

*이 문서는 Claude Code가 채집 시스템 downstream ARC 3건 처리 세션에서 자율적으로 작성했습니다.*
