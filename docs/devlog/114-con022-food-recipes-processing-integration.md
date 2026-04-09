# Devlog #114 — CON-022: 음식 레시피 19종 processing-system.md 통합

> 작성: Claude Code (Sonnet 4.6) | 2026-04-09

---

## 작업 요약

**CON-022**: `docs/content/processing-system.md`에 음식 레시피 19종 통합 — 전체 레시피 수 56종 → 75종

food-items.md(DES-025/CON-021)에서 확정된 음식 레시피를 processing-system.md canonical 레시피 문서에 정식 통합. 가공소 일반 요리 8종(섹션 3.1.5)과 베이커리 고급/최고급 요리 11종(섹션 3.4.2)으로 구분 추가.

---

## 수정된 파일

| 파일 | 수정 내용 |
|------|----------|
| `docs/content/processing-system.md` | 섹션 3.1.5 (일반 요리 8종), 섹션 3.4 구조 재편(3.4.1/3.4.2), 섹션 3.8 요약 75종, 2.2 총계 갱신 |
| `docs/content/food-items.md` | Open Questions 5·6번 완료 표시 (CON-022 완료로 [OPEN] 해소) |

---

## 주요 추가 내용

### 섹션 3.1.5 — 가공소 일반 요리 8종 (신규)

| 레시피 ID | 이름 | 재료 | 가공 시간 | 판매가 |
|----------|------|------|:---:|:---:|
| recipe_food_common_roasted_corn | 구운 옥수수 | 옥수수 x1 | 1시간 | 80G |
| recipe_food_common_potato_soup | 감자 수프 | 감자 x2 | 1시간 | 50G |
| recipe_food_common_tomato_salad | 토마토 샐러드 | 토마토 x1 | 30분 | 55G |
| recipe_food_common_grilled_fish | 구운 생선 정식 | 생선(Common) x1 | 1시간 | 60G |
| recipe_food_common_carrot_stew | 당근 스튜 | 당근 x2 | 1시간 | 55G |
| recipe_food_common_mushroom_soup | 버섯 수프 | 채집 버섯 x1~2 | 1시간 | 40G |
| recipe_food_common_corn_porridge | 옥수수 죽 | 옥수수 x1 + 감자 x1 | 1시간 | 75G |
| recipe_food_common_herb_tea | 약초 차 | 채집 약초 x2 | 30분 | 20G |

### 섹션 3.4.2 — 베이커리 고급/최고급 요리 11종 (신규)

**고급 요리 7종 (장작 x1)**: 호박 스튜(350G), 딸기 잼 토스트(320G), 특제 생선 스튜(380G), 달걀 봄나물 비빔밥(300G), 가을 버섯 요리(340G), 수박 셔벗(480G), 치즈 그라탱(420G)

**최고급 요리 4종 (장작 x2)**: 황금 연꽃 만찬(900G), 천년 영지 보양식(850G), 왕실 수확 연회(1,200G), 산삼 강정(1,100G)

---

## 설계 결정 사항

### 天년 영지 보양식 재료 처리
food-items.md에서 `item_chicken_soup` 중간 가공재 경유를 기술했으나, 레시피 수를 19종으로 유지하기 위해 베이커리 레시피에서 달걀 x2 + 당근 x1로 직투입 처리. 중간 가공재 분류 여부는 food-items.md Open Questions 2번으로 추적.

### 레시피 수 최종 확인
- 가공소(일반): 18(작물) + 3(생선) + 9(채집) + 1(광석) + **8(일반 요리)** = 39종
- 베이커리: 5(기존 요리) + 2(생선) + 2(채집 요리) + **7(고급)** + **4(최고급)** = 20종
- 발효실: 5 + 2 = 7종 | 제분소: 4종 | 치즈 공방: 5종
- **총계: 75종**

---

## 리뷰 결과

**CRITICAL**: 없음
**WARNING 1건 (수정 완료)**: 천년 영지 보양식 재료 item_chicken_soup 처리 방식 주석 추가 (처리)
**INFO 1건 (수정 완료)**: food-items.md Open Questions 5·6번 완료 표시

---

## 잔존 [OPEN] 항목

| 항목 | 내용 | 후속 작업 |
|------|------|----------|
| 황금 연꽃/천년 영지 판매가 | 잠정 100G/120G | FIX-119 |
| item_sugar 상점 정의 | 50G 잠정 | food-items.md Open Questions 1번 |
| item_chicken_soup 분류 | 중간 가공재 vs 독립 아이템 | food-items.md Open Questions 2번 |
| 이동 속도 +20% 최고급 효과 | 잠정값 | 이동 속도 시스템 설계 후 검증 |
