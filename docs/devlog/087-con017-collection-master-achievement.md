# Devlog #087 — CON-017: "통합 수집 마스터" 업적 도입 결정

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

CON-017 태스크: DES-018에서 통합 수집 도감을 채택한 이후 남은 [OPEN] 항목 — "통합 수집 마스터" 업적 도입 여부를 결정하고 관련 문서에 반영했다.

---

## 설계 결정: 업적 도입 확정

| 방안 | 결과 |
|------|------|
| A. Hidden 업적으로 도입 | ✓ 채택 |
| B. 개별 업적(어종/채집)으로 충분 | — |

**채택 근거**: 어종 15종 + 채집 27종 두 도감을 모두 완성하는 최종 도전 목표가 필요하며, "숨겨진 업적"으로 분류해 발견의 즐거움을 제공한다. XP 영향(+30 XP)은 무시 가능한 수준(+0.3%p).

---

## 업적 상세: ach_hidden_07

| 항목 | 값 |
|------|-----|
| achievementId | `ach_hidden_07` |
| 이름 | 통합 수집 마스터 |
| 유형 | Single (Hidden) |
| 조건 | `ach_fish_04` + `ach_gather_03` 모두 달성 |
| conditionType | Custom(99) |
| 보상 | 100G / 30 XP |
| 칭호 | 수집의 대가 (`title_collection_master`) |
| 아이템 | 도감 배경: 전설의 자연 x1 |

---

## 수정된 파일

| 파일 | 변경 내용 |
|------|----------|
| `docs/content/achievements.md` | 업적 39→40종, Hidden 7개, XP 3,130→3,160, 골드 10,950→11,050G, 칭호 43→50종, `title_collection_master` 추가 |
| `docs/systems/collection-system.md` | OPEN#4 → RESOLVED 처리, Cross-references에 achievement-architecture.md 추가 |
| `docs/systems/achievement-architecture.md` | Custom(99) 주석에 ach_hidden_07 추가, OnAchievementUnlocked → HandleAchievementChain 구독 선언 추가 |
| `docs/balance/xp-integration.md` | 업적 XP 3,130→3,160 전수 갱신 (섹션 1.2, 3.1, 5.2, 5.3 등) |
| `docs/balance/progression-curve.md` | 섹션 1.2 업적 XP ~2,640→~3,160 갱신 |

---

## 리뷰 수정 사항

| 이슈 | 수정 내용 |
|------|----------|
| CRITICAL-5: Custom enum 주석 | ach_hidden_05/06/07 주석 추가 |
| WARNING-1: xp-integration.md 구버전 수치 | 3,160 XP 전수 갱신 |
| WARNING-4: Cross-references 누락 | achievement-architecture.md 참조 추가 |
| WARNING-칭호 수: 43→50 표기 오류 | achievements.md 섹션 1.1 수정 |

---

## 후속 태스크 (ARC-040 추가)

ARC-040: `HandleAchievementChain` 핸들러 구체적 구현 방식 확정 — Custom(99) 핸들러 목록 achievement-system.md 섹션 7.1과 동기화

---

*이 문서는 Claude Code가 CON-017 세션에서 자율적으로 작성했습니다.*
