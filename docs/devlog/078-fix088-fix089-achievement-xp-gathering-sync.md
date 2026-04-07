# Devlog #078 — FIX-088 + FIX-089: 채집 시스템 아키텍처·XP 동기화

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

지난 세션(#077)에서 완결된 채집 시스템(CON-013)의 후속 동기화 2건을 처리했다. achievement-architecture.md의 enum 갱신과 xp-integration.md의 시뮬레이션 수치 업데이트가 목표였다.

### 수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/systems/achievement-architecture.md` | FIX-088: AchievementConditionType enum 3종 추가, 이벤트 테이블 2행, Step 1-2 값 수 갱신 |
| `docs/balance/xp-integration.md` | FIX-089: 섹션 3.1 Gatherer 행, 4.2.2/5.1/5.2/5.3/5.4 시뮬레이션 전체 갱신 |
| `TODO.md` | FIX-088/089 완료 표시, FIX-091/DES-020 신규 추가 |

---

## FIX-088: achievement-architecture.md AchievementConditionType 3종 추가

CON-013에서 채집 업적 5종(A-031~A-035)이 확정되었으나, 이를 처리하는 ConditionType이 enum에 누락된 상태였다.

추가된 값:

| 값 | ID | 용도 |
|----|-----|------|
| `GatherCount` | 15 | 채집 총 횟수 (아이템 무관) |
| `GatherSpeciesCollected` | 16 | 채집으로 수집한 고유 종류 수 (도감 완성용) |
| `GatherSickleUpgraded` | 17 | 채집 낫 업그레이드 티어 달성 (1=강화, 2=전설) |

이벤트 핸들러 매핑 테이블에도 `GatheringEvents.OnItemGathered` → HandleGather, `GatheringEvents.OnSickleUpgraded` → HandleSickleUpgrade 두 행을 추가했다.

Step 1-2 enum 값 수: 16개 → 19개로 갱신.

---

## FIX-089: xp-integration.md 채집 XP 반영

CON-013 확정 수치:
- 채집 퀘스트 1년차 기여: ~105 XP
- 채집 업적 XP 총량: 490 XP (A-031~A-035 합계)
- 업적 총: 2,640 → 3,130 XP (34종 → 39종)

### 섹션별 변경 내용

**섹션 3.1 (업적 카테고리 추정표)**
- Gatherer(5) 행 추가: 490 XP 총, 1년차 ~70 XP
- 합계: 2,640 → 3,130 XP / ~660~700 → ~730~770 XP
- 일반 업적 XP: ~540 → ~610 / 적극적: ~700 → ~770

**섹션 4.2.2 (수정 시나리오 A' 최종 시뮬레이션)**

| XP 소스 | 캐주얼 | 일반 | 적극적 |
|---------|-------|------|--------|
| 수확/경작 | ~3,332 | ~3,332 | ~4,000 |
| 퀘스트 | 0 | ~600(+100) | ~845(+105) |
| 업적 | ~270(+70) | ~570(+70) | ~730(+70) |
| **합계** | **~3,602** | **~4,502** | **~5,575** |

레벨 도달: 캐주얼 8 갓 진입 → 8 안정, 일반 8중반 → 8중후반, 적극적 레벨 9 직전 유지.

**섹션 5.1~5.4**: 퀘스트 연간 ~540→~640, 업적 ~540→~610, 합계 ~4,412→~4,582. 퀘스트 총 ~1,010→~1,115, 업적 총 2,640→3,130, 보조소스 합계 ~3,650→~4,245(47%). 5.4 안전검증 수치 모두 갱신.

---

## 후속 과제 (TODO 보충)

TODO가 8개로 줄어 2개를 신규 추가했다:

| ID | 내용 |
|----|------|
| FIX-091 | economy-architecture.md SupplyCategory.Forage + HarvestOrigin.Wild 추가 |
| DES-020 | 철 광석 도구 업그레이드 대체 재료 여부 결정 |

---

*이 문서는 Claude Code가 채집 아키텍처·XP 동기화 세션에서 자율적으로 작성했습니다.*
