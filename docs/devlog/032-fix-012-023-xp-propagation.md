# Devlog #032 — FIX-012~023: BAL-007 A' XP 전체 문서 반영

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

BAL-007에서 확정한 제안 A'(퀘스트 900 XP, 업적 2,250 XP 유지, 1년차 레벨 8 목표)를 7개 문서에 일괄 반영했다.

### 변경 파일 목록

| 파일 | FIX ID | 변경 내용 |
|------|--------|----------|
| `docs/systems/quest-system.md` | FIX-012, 018 | 섹션 3~7 퀘스트 XP 전면 재확정 |
| `docs/balance/progression-curve.md` | FIX-013, 014, 015 | 소스 배분 요약·DEPRECATED·통합 시뮬레이션 추가 |
| `docs/balance/quest-rewards.md` | FIX-016 | 기준 XP 4,609→9,029 정정 |
| `docs/content/achievements.md` | FIX-017 | XP 비율 49%→24.9%, [RISK]→[NOTE] |
| `docs/systems/progression-architecture.md` | FIX-019, 020, 021 | XPSource enum + switch + 클래스 다이어그램 |
| `docs/systems/quest-architecture.md` | FIX-022 | GrantXP 구현체 추가 |
| `docs/systems/achievement-architecture.md` | FIX-023 | GrantReward XP 호출 명시 |

---

## 주요 변경 내용

### 퀘스트 XP 삭감 (FIX-012)

총 52개 퀘스트 XP 수치 변경 (9,147 XP → 900 XP):

| 카테고리 | 구 XP | 확정 XP | 변경률 |
|----------|-------|---------|--------|
| 메인 퀘스트 (14개) | 1,300 XP | 280 XP | -78.5% |
| NPC 의뢰 (11개) | 590 XP | 140 XP | -76.3% |
| 일일 목표 (1년차) | 2,857 XP | 280 XP | -90.2% |
| 농장 도전 (1년차) | ~1,500 XP | ~200 XP | -86.7% |

### XPSource enum 확장 (FIX-019~021)

`XPSource.QuestComplete`, `XPSource.AchievementReward` 추가:
- `progression-architecture.md`: enum 2값 추가, switch 2 case 추가, 클래스 다이어그램 이벤트 구독 2개 추가
- `quest-architecture.md`: `GrantXP` 구현 코드 (`AddExp(scaledXP, XPSource.QuestComplete)`)
- `achievement-architecture.md`: `GrantReward` XP 경로 (`AddExp(xp, XPSource.AchievementReward)`)

### progression-curve.md 3개 수정 (FIX-013~015)

1. **섹션 1.2 요약 테이블 추가**: 수확 55%, 경작 15%, 시설 12%, 가공 3%, 퀘스트 10%, 업적 5%
2. **섹션 1.3.1 [DEPRECATED]**: baseXP=50, gF=1.55 구 파라미터 deprecated 명시, 섹션 2.4.1 참조
3. **섹션 2.4.4 통합 시뮬레이션**: 퀘스트/업적 포함 1년차 XP (~4,972 XP, 레벨 8 중반) 확정

---

## 리뷰 결과

CRITICAL 4건 즉시 수정:
1. `progression-curve.md` 섹션 1.2.3 — 물주기 XP 1→0, 호미질 XP 1→2 (섹션 2.4.1 조정 미반영)
2. `quest-rewards.md` 섹션 2.1 — 메인 퀘스트 XP 집계 1,300→280 정정
3. `quest-rewards.md` 섹션 2.2 — NPC 의뢰 XP 집계 590→140 정정
4. `quest-rewards.md` 섹션 7.2 — 기준 XP 4,609→9,029 정정

WARNING → TODO 등록: REV-001~003 (quest-rewards.md 섹션 2.4/2.5/6.2 추가 정정)

---

## 후속 작업

- **REV-001** (Priority 3): quest-rewards.md 섹션 2.4 농장 도전 XP 재계산
- **REV-002** (Priority 2): quest-rewards.md 섹션 2.5 전체 총계 재산정
- **REV-003** (Priority 2): quest-rewards.md 섹션 6.2 제안 A→A' 업데이트

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
