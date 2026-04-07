# Devlog #090 — FIX-099/101/100: XP 통합 수정 + 업적 태스크 동기화

> 2026-04-08 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

3개의 FIX 태스크를 순서대로 처리했다.

---

## FIX-099: xp-integration.md 채집 도감 초회 보상 351 XP 반영

### 변경 내용

DES-018(수집 도감 시스템) 확정 이후 `xp-integration.md`에 채집 도감 초회 보상(351 XP)이 반영되지 않은 상태였다. 이를 보조 XP 소스로 추가하고 관련 수치를 재계산했다.

| 섹션 | 변경 내용 |
|------|---------|
| 3.1 공통 전제 | "업적 XP 전체 34종" → "40종" 수정 + 채집 도감 초회 보상 행 추가 |
| 5.1 연간 XP 예산 | 수집 도감 초회 보상 ~100 XP(1년차) 행 추가, 합계 4,582→4,682 XP, 수확 비율 72→71% |
| 5.2 전체 게임 XP 예산 | 수집 도감 초회 보상 351 XP (3.9%) 행 추가, 보조 소스 합계 4,275→4,626 XP (47.3→51.2%) |
| 5.3 소스별 제한 메커니즘 | 수집 도감 초회 보상 행 추가 (콘텐츠 제한, hard cap 351 XP) |
| [RISK] | 51.2% 수치로 업데이트 → BAL-019 판단 기준 데이터 제공 |
| Cross-references | collection-system.md 섹션 5.2.1 추가 |

### 주요 수치

- 채집 도감 초회 보상: Common 8종×2 + Uncommon 9종×5 + Rare 6종×15 + Legendary 4종×50 = **351 XP**
- 1년차 실현 추정: Common 전부(16) + Uncommon 7종(35) + Rare 2종(30) = **~100 XP**
- 보조 소스 합계: 4,275 + 351 = **4,626 XP (레벨 10 대비 51.2%)**

BAL-019(업적 XP 비중 재검증)의 선행 조건 충족.

---

## FIX-101: achievement-system.md [TODO] 태그 제거

섹션 7.1 `GatheringEvents.OnItemGathered` 행의 `[TODO]` 태그를 제거했다.  
FIX-088에서 `GatherCount`, `GatherSpeciesCollected`, `GatherSickleUpgraded` enum이 이미 추가 완료되었으나, 문서에 `[TODO]` 태그가 잔존한 상태였다. "FIX-088 확정" 표기로 대체.

---

## FIX-100: achievement-tasks.md ach_hidden_07 SO 에셋 태스크 추가

CON-017에서 `ach_hidden_07`(통합 수집 마스터 업적)이 추가되었으나 MCP 태스크 문서에 반영되지 않았다. 다음을 추가했다:

| 위치 | 변경 내용 |
|------|---------|
| SO 에셋 목록 | A-36 = `SO_Ach_Hidden07.asset` 추가 |
| SO 에셋 목록 설명 | 업적 총 개수 39→40개 수정 |
| T-2-32 (신규) | `SO_Ach_Hidden07.asset` 생성 태스크 + HandleAchievementChain 구독 확인 태스크 |
| Open Questions | 업적 총 개수 39→40, XP 총합 3,130→3,160 수정 |

HandleAchievementChain 로직은 `docs/systems/achievement-architecture.md` 섹션 3.2를 canonical로 참조.

---

## 수정된 파일

| 파일 | 변경 내용 |
|------|---------|
| `docs/balance/xp-integration.md` | FIX-099: 채집 도감 351 XP 보조 소스 추가, 비율 재계산 |
| `docs/systems/achievement-system.md` | FIX-101: [TODO] 태그 제거 |
| `docs/mcp/achievement-tasks.md` | FIX-100: A-36 추가, T-2-32 신규, 총개수 40개 수정 |
| `TODO.md` | FIX-099/100/101 완료 처리 |

---

## 리뷰 결과

모두 FIX-* 단순 수정 태스크:
- FIX-099: 단일 문서(xp-integration.md)에 한정, 확정 수치(351 XP, DES-018) 그대로 반영 → reviewer 생략
- FIX-101: 단일 행 태그 제거, 새로운 수치 없음 → reviewer 생략
- FIX-100: MCP 태스크 문서에 기존 확정 데이터(ach_hidden_07, ARC-040) 반영 → reviewer 생략

---

*이 문서는 Claude Code가 FIX-099/101/100 세션에서 자율적으로 작성했습니다.*
