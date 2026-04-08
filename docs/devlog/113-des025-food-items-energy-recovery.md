# Devlog #113 — DES-025 / CON-021: 음식 아이템 에너지 회복 설계

> 작성: Claude Code (Sonnet 4.6) | 2026-04-09

---

## 작업 요약

**DES-025 / CON-021**: `docs/content/food-items.md` 신규 생성 — 음식 아이템 canonical 명세
**수정**: `docs/systems/energy-system.md` 섹션 5.2 [OPEN] 해소 + 섹션 1.2 참조 오류 수정

`energy-system.md` 섹션 5.2의 [OPEN] 항목(음식 아이템 구체 목록/회복량 미확정)을 해소하는 후속 설계.

---

## 신규 파일

| 파일 | 내용 |
|------|------|
| `docs/content/food-items.md` | 음식 아이템 canonical 명세 (CON-021/DES-025) |

---

## 주요 설계 결정

### 요리 시스템 통합 방식
**기존 가공 시스템(가공소 + 베이커리) 확장 채택**. 별도 요리 시설 불필요.
- design.md 섹션 4.6 시설 목록 수정 불필요
- 일반 요리 8종 → 가공소(일반), 고급/최고급 요리 11종 → 베이커리

### 공급 경로
| 등급 | 획득 경로 |
|------|-----------|
| 기본 음식 (6종) | 채집 원물 직접 섭취 |
| 일반 요리 (8종) | 가공소 제조 OR 잡화 상점 구매 (50% 마크업) |
| 고급 요리 (7종) | 베이커리 전용 (상점 미판매) |
| 최고급 요리 (4종) | 베이커리 전용, 희귀 재료 (상점 미판매) |

### 음식 등급 회복량 확정 (energy-system.md 섹션 5.2 canonical)

| 등급 | 즉시 회복량 | 특수 효과 |
|------|:----------:|-----------|
| 기본 음식 | +10 | 없음 |
| 일반 요리 | +25 | 없음 |
| 고급 요리 | +45 | 임시 maxEnergy +20 (해당 날) |
| 최고급 요리 | +60 | 임시 maxEnergy +20 + 이동 속도 +20%\* |

\* [OPEN] 이동 속도 수치 잠정값, 이동 속도 시스템 설계 후 검증 필요.

---

## 리뷰 수정 사항 (CRITICAL 2건 + WARNING 5건 처리)

| 항목 | 파일 | 수정 내용 |
|------|------|----------|
| CRITICAL-1 | energy-system.md 섹션 1.2 | "(섹션 3.2 참조)" → "(섹션 5.2 참조)" 오타 수정 |
| CRITICAL-2 | food-items.md 섹션 3.3 | "봄나물 비빔밥 (고급)" → "달걀 봄나물 비빔밥" (processing-system.md 기존 아이템명 중복 해소) |
| WARNING-2 | energy-system.md 섹션 5.2 | canonical 방향 명확화: "회복량 수치의 canonical은 energy-system.md" 표기 추가 |
| WARNING-3 | food-items.md 섹션 3.1 | 기본 음식 판매가 직접 수치 → `(-> see gathering-system.md)` 참조로 교체 |
| WARNING-4 | food-items.md 섹션 3.4 | 황금 연꽃/천년 영지 가격에 [OPEN] 잠정값 표기 + 이동 속도 +20%에 \* 주석 추가 |
| WARNING-5 | food-items.md Cross-references | livestock-system.md 섹션 4(달걀 판매가), gathering-items.md 섹션 번호 추가 |
| WARNING-1 | doc-standards.md | food-items.md canonical 매핑 추가 → 파일 보호로 PATTERN-011 self-improve에서 처리 예정 |

---

## 후속 작업 (TODO 등록)

| ID | Priority | 내용 |
|----|----------|------|
| CON-022 | 2 | processing-system.md에 음식 레시피 19종 실제 통합 (56→75종) |
| FIX-119 | 1 | gathering-items.md 황금 연꽃/천년 영지 판매가 canonical 확정 |
| FIX-118 | 2 | 기존 — farming/fishing/gathering/time-season 에너지 수치 canonical 참조 교체 (미완료) |
| ARC-047 | 1 | 기존 — energy-tasks.md MCP 시퀀스 |
| PATTERN-011 | - | self-improve 전용 — doc-standards.md food-items.md 매핑 행 추가 포함 |

---

## Open Questions 신규 (food-items.md)

1. [OPEN] `item_sugar` 잡화 상점 정의 확정 (50G 잠정)
2. [OPEN] `item_chicken_soup` 일반 요리 목록 정식 포함 여부
3. [OPEN] 이동 속도 +20% — 이동 속도 시스템 설계 후 검증
4. [OPEN] 음식 섭취 UI/UX 방식
5. [OPEN] processing-system.md 음식 레시피 통합 (CON-022)
