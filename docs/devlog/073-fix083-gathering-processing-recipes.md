# Devlog #073 — FIX-083: 채집물 가공 레시피 공식 추가

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

BAL-016으로 채집물 원재료 판매가가 40% 하향되면서, gathering-items.md의 [OPEN] 가공 레시피 제안들의 경제적 타당성이 생겼다. FIX-083으로 13종 채집물 가공 레시피를 processing-system.md 섹션 3.7에 공식 추가하고, ProcessingRecipeData SO 스키마를 채집물 레시피를 수용하도록 확장했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/content/processing-system.md` | 섹션 3.7 채집물 레시피 13종 신설, 섹션 2.2 레시피 수 테이블 갱신(42→55종), 섹션 3.8 전체 요약 |
| `docs/content/gathering-items.md` | 13종 아이템 가공 연계 [OPEN] → 확정 레시피 ID, 섹션 9 가공 연계 요약 갱신 |
| `docs/systems/gathering-system.md` | 섹션 6.4 가공 연계 예시 테이블 갱신 |
| `docs/systems/processing-architecture.md` | ProcessingRecipeData 스키마 확장 (inputs[], unlockType/Value, ProcessingType 4종 추가) |
| `docs/pipeline/data-pipeline.md` | ProcessingType enum 7→11종, 스키마 필드 갱신, 레시피 에셋 수 18→55 |

---

## 채집물 가공 레시피 13종

### 가공소 (일반) — 9종

| 레시피 ID | 원재료 | 가공품 | 판매가 | 처리 시간 | 해금 |
|----------|--------|--------|--------|---------|------|
| recipe_gather01 | 산딸기 x5 | 야생 베리잼 (item_wild_berry_jam) | 26G | 1시간 | Lv.1 |
| recipe_gather02 | 도토리 x5 | 도토리묵 (item_acorn_jelly) | 20G | 1시간 | Lv.1 |
| recipe_gather03 | 능이버섯 x3 | 건조 버섯(능이) (item_dried_neungi) | 24G | 2시간 | Lv.2 |
| recipe_gather04 | 표고버섯(야생) x2 | 건조 버섯(표고) (item_dried_wild_shiitake) | 36G | 2시간 | Lv.2 |
| recipe_gather05 | 영지버섯 x2 | 건조 영지 (item_dried_reishi) | 84G | 2시간 | Lv.4 |
| recipe_gather06 | 황금 연꽃 x1 | 황금 연꽃차 (item_golden_lotus_tea) | 300G | 2시간 | Lv.7 |
| recipe_gather07 | 천년 영지 x1 | 천년 영지차 (item_millennium_reishi_tea) | 360G | 3시간 | Lv.7 |
| recipe_gather08 | 겨울 나무껍질 x5 | 나무껍질 차 (item_bark_tea) | 15G | 30분 | Lv.1 |
| recipe_gather09 | 동충하초 x2 | 동충하초 환 (item_cordyceps_pill) | 140G | 2시간 | Lv.5 |

### 발효실 — 2종

| 레시피 ID | 원재료 | 가공품 | 판매가 | 처리 시간 | 해금 |
|----------|--------|--------|--------|---------|------|
| recipe_gather10 | 머루 x5 | 머루 와인 (item_wild_grape_wine) | 90G | 48시간 | Lv.3 |
| recipe_gather11 | 산삼 x1 | 산삼주 (item_wild_ginseng_wine) | 280G | 72시간 | Lv.5 |

### 베이커리 — 2종

| 레시피 ID | 원재료 | 가공품 | 판매가 | 처리 시간 | 해금 |
|----------|--------|--------|--------|---------|------|
| recipe_gather12 | 달래 x2 + 봄나물 x1 | 봄나물 비빔밥 (item_spring_herb_bibimbap) | 30G | 30분 | Lv.2 |
| recipe_gather13 | 송이버섯 x1 | 송이 구이 (item_grilled_pine_mushroom) | 55G | 30분 | Lv.4 |

---

## ProcessingRecipeData 스키마 확장 (FIX-083)

### 변경 요약

| 구분 | 변경 내용 |
|------|---------|
| 제거 | `inputCategory` (CropCategory), `inputItemId` (string), `inputQuantity` (int) |
| 추가 | `inputs: RecipeInput[]`, `unlockType: RecipeUnlockType`, `unlockValue: int` |
| 신규 타입 | `RecipeInput` struct, `RecipeUnlockType` enum (3종) |
| ProcessingType 확장 | Dried, Tea, Pill, Food 4종 추가 (기존 7종 → 11종) |

### 핵심 결정: 복합 재료 배열 (inputs[])

기존 `inputItemId + inputQuantity` 단일 구조를 `inputs: RecipeInput[]` 배열로 교체.
- 봄나물 비빔밥(달래 x2 + 봄나물 x1) 등 복합 재료 레시피를 지원
- 기존 단일 재료 42종은 `inputs[0]`만 사용 (구조적 호환)
- 기존 `inputCategory`(CropCategory) 제거 — 채집물은 CropCategory가 아니므로 의미 없음

### 핵심 결정: GatheringMastery 해금 조건

`RecipeUnlockType.GatheringMastery = 2` 신규 추가.
- 채집 숙련도 Lv.1/2/3/4/5/7 기반 점진적 해금으로 채집 투자 보상 강화
- gathering-system.md 섹션 4의 독립 숙련도(Lv.1~10) 연계

---

## 보류 레시피 (미채택)

| 레시피 | 사유 |
|--------|------|
| 쑥떡 (쑥+쌀) | 쌀 획득 경로 [OPEN] |
| 연잎밥 (연잎+쌀) | 쌀 획득 경로 [OPEN] |
| 수정 장식품, 자수정 목걸이 | 광물 가공 별도 스코프 |
| 꽃다발 | 장식 아이템 시스템 미설계 |
| 나무껍질 차 가공 수익성 | 원재료 10G → 가공 15G (+50%), 연료 없음으로 유지 |

---

## Reviewer 지적 사항 및 수정

| 심각도 | 이슈 | 처리 |
|--------|------|------|
| CRITICAL | data-pipeline.md 에셋 수 18 (구버전) | 55로 갱신 |
| WARNING | data-pipeline.md 총 에셋 수 ~120 (구버전) | ~157로 갱신 |
| WARNING | processing-architecture.md 레시피 수 32 (구버전) | 55로 갱신 |
| WARNING | processing-architecture.md Cross-references 누락 (gathering 3종) | 4개 참조 추가 |

---

## 세션 후 활성 TODO

| ID | Priority | 상태 |
|----|----------|------|
| DES-017 | 2 | 채집 낫 업그레이드 경로 상세 |
| ARC-032 | 2 | 채집 MCP 태스크 문서화 |
| ARC-033 | 1 | 채집 SO 에셋 data-pipeline.md 반영 |
| CON-013 | 1 | 채집 퀘스트/업적 콘텐츠 |
| PATTERN-009/010 | - | self-improve 전용 |

---

*이 문서는 Claude Code가 FIX-083 채집물 가공 레시피 추가 작업에 따라 자율적으로 작성했습니다.*
