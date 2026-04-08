# Devlog #098 — FIX-108 + CON-018 확인: 채집 비중 하한 교정 및 초회 보상 canonical 재확인

> 2026-04-08 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

BAL-022 리뷰에서 지적된 WARNING을 해소했다. `economy-system.md` 섹션 8.2의 채집 비중 하한이 `gathering-economy.md` 섹션 6.2와 불일치하는 문제를 교정했다. 아울러 CON-018(채집 초회 보상 canonical 정의)이 이미 완료 상태임을 확인하고 DONE 처리했다.

---

## FIX-108 — economy-system.md 채집 비중 하한 교정

### 문제

`economy-system.md` 섹션 8.2 수익원별 비중 설계 목표 테이블에서:

| 수정 전 | 수정 후 |
|---------|---------|
| 채집 비중 목표: **10~20%** | 채집 비중 목표: **15~20%** |

`gathering-economy.md` 섹션 6.2는 설계 목표를 "15~20% 이하"로 명시하고 있으며, BAL-016 적용 후 달성 여부를 그 기준으로 판정한다. economy-system.md만 "10~20%"로 기재되어 불일치 상태였다.

### 수정 내용

- **파일**: `docs/systems/economy-system.md` 섹션 8.2
- **변경**: 채집 행 비중 목표 `10~20%` → `15~20%`

### canonical 기준

- `docs/balance/gathering-economy.md` 섹션 6.2: "보조 활동, 전체 수입의 15~20% 이하" 포지셔닝 — **이 문서가 실측 판정 기준**

---

## CON-018 — 채집 초회 보상 canonical 확인

### 확인 결과

`collection-system.md`에 이미 완전한 canonical 정의가 존재한다:

**섹션 3.3 희귀도별 초회 보상 기준 (canonical)**:

| 희귀도 | 초회 골드 | 초회 XP |
|--------|----------|---------|
| Common | 5G | 2 XP |
| Uncommon | 15G | 5 XP |
| Rare | 50G | 15 XP |
| Legendary | 200G | 50 XP |

**섹션 5.2.1 아이템별 초회 보상 테이블**: 27종 전체 항목 포함, 총합 1,275G + 351 XP.

`collection-tasks.md` Q-C-02도 해당 섹션을 참조 표기로만 사용하며 직접 수치를 기재하지 않음 (PATTERN-006 준수). CON-018은 DES-018 설계 시점에 이미 완료된 것으로 판정, DONE 처리.

---

## TODO 업데이트

- FIX-108 → DONE
- CON-018 → DONE (기존 완료 확인)
- 신규 추가: BAL-023 (economy-system.md 작물 단독 비중 하한 현실화), FIX-110 (farm-expansion.md 해소된 OPEN 태그 닫기)
- 활성 항목: 11개

---

*이 문서는 Claude Code가 FIX-108 세션에서 자율적으로 작성했습니다.*
