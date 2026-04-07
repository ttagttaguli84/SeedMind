# Devlog #061 — FIX-056 + FIX-068: 장애물 도구-HP canonical 등록 및 ToolType 확정

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

FIX-056(장애물 HP canonical 등록)과 FIX-068(ToolType Axe/Pickaxe 추가 여부 확정)을 통합 처리했다. 두 TODO는 모두 farm-expansion 장애물 시스템의 도구-등급 매핑을 다루므로 함께 해결하는 것이 효율적이었다. 리뷰어 5개 WARNING/INFO 중 3건은 별도 수정 수행, 2건은 INFO로 처리.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/systems/farm-expansion.md` | FIX-068: 섹션 3.1 곡괭이*/도끼* → 호미(Hoe), 섹션 3.3 에너지 소모 통합, 섹션 6 파라미터 이름 변경, [OPEN]/[RISK] 해소 |
| `docs/systems/farm-expansion-architecture.md` | FIX-056+068: 섹션 2.3 enum 주석, 섹션 5.2 테이블, CanToolClear() 코드, Open Questions, Risks 전수 업데이트 |
| `TODO.md` | FIX-056/068 DONE 처리 |

---

## FIX-068 — ToolType 확장 여부 결정

### 배경

`farm-expansion.md` 섹션 3.1에 장애물 제거 도구로 "곡괭이*(Pickaxe)", "도끼*(Axe)"가 기재되어 있었으나, `farming-architecture.md`의 ToolType enum은 `Hoe, WateringCan, SeedBag, Sickle, Hand` 5종만 정의하고 있었다. 두 접근법 중 하나를 선택해야 했다:

- **접근법 A**: ToolType 확장 없음. 기존 Hoe/Sickle로 모든 장애물 처리
- **접근법 B**: ToolType에 Axe, Pickaxe 추가

### 결정: 접근법 A 채택

| 근거 | 설명 |
|------|------|
| 복잡도 최소화 | 신규 ToolType 추가 시 tool-upgrade.md, UI 도구바, farming-architecture.md 전수 수정 필요 |
| 서사적 일관성 | 낫(Sickle)이 식물 장애물, 호미(Hoe)가 지형 장애물을 처리하는 것이 자연스럽다 |
| 진행 경제 유지 | Hoe 업그레이드가 경작 + 개간 양쪽에 영향 → 업그레이드 가치 상승 |
| 도구 슬롯 부담 없음 | 5개 슬롯(Hoe/WateringCan/SeedBag/Sickle/Hand) 유지 |

### 확정 매핑

| 장애물 | 도구 | 최소 등급 |
|--------|------|-----------|
| 잡초(Weed), 덤불(Bush) | 낫(Sickle) | Basic |
| 소형 돌(SmallRock), 그루터기(Stump), 소형 나무(SmallTree) | 호미(Hoe) | Basic |
| 대형 바위(LargeRock), 대형 나무(LargeTree) | 호미(Hoe) | Reinforced (tier 2+) |

---

## FIX-056 — HP Canonical 등록

`farm-expansion.md` 섹션 3.1이 이미 장애물별 HP(제거 횟수) canonical 테이블을 포함하고 있었다. 아키텍처 문서의 섹션 5.2와 enum 주석도 이미 `(→ see DES-012 섹션 3.1)` 참조를 사용하고 있었으나, 도구 이름이 불일치했다. FIX-068 처리와 함께 정합성이 확보되었다.

### HP 확정 값 (farm-expansion.md 섹션 3.1 canonical)

| 장애물 | Basic | Reinforced | Legendary |
|--------|-------|-----------|-----------|
| 잡초 | 1회 | 1회 | 1회 |
| 소형 돌 | 2회 | 1회 | 1회 |
| 대형 바위 | 불가 | 5회 | 2회 |
| 그루터기 | 3회 | 2회 | 1회 |
| 소형 나무 | 2회 | 1회 | 1회 |
| 대형 나무 | 불가 | 6회 | 3회 |
| 덤불 | 2회 | 1회 | 1회 |

---

## 리뷰어 검증 결과

| ID | 심각도 | 이슈 내용 | 수정 결과 |
|----|--------|-----------|-----------|
| WARNING-001 | 🟡 | farm-expansion.md 섹션 3.3/6에 곡괭이/도끼 잔재 | hoeEnergy 파라미터로 통합, 곡괭이/도끼 행 제거 |
| WARNING-002 | 🟡 | farm-expansion.md Open Questions 항목 2 [OPEN] 잔재 | [RESOLVED] FIX-068으로 업데이트 |
| INFO-001 | 🔵 | farm-expansion-architecture.md Open Questions 4 잔재 | [RESOLVED] DES-012 완성 처리 |
| INFO-002 | 🔵 | farm-expansion-architecture.md Risks 5 잔재 | [RESOLVED] DES-012 완성 처리 |
| INFO-003 | 🔵 | architecture Open Questions 2 Tree/Boulder 처리 | [RESOLVED] FIX-068으로 LargeTree 제거 가능 확정 |

---

## 에너지 소모 테이블 정리

FIX-068 후속으로 섹션 3.3 에너지 소모 테이블을 단순화했다:

| 도구 | Basic | Reinforced | Legendary |
|------|-------|-----------|-----------|
| 낫 | 1 | - | - |
| 호미 | 3 | 2 | 1 |

섹션 6 튜닝 파라미터도 `pickaxeEnergy_*`, `axeEnergy_*` → `hoeEnergy_basic/reinforced/legendary`로 통합.

---

## 세션 후 활성 TODO

| ID | Priority | 상태 |
|----|----------|------|
| BAL-012 | 2 | 잔여 (낚시 경제 밸런스) |
| FIX-069 | 2 | 잔여 (낚시 포인트 수 불일치) |
| CON-009 | 2 | 잔여 (치즈 공방 레시피) |
| DES-014 | 2 | 잔여 (겨울 씨앗 판매 경로) |
| FIX-054 | 2 | 잔여 (생선 가공 레시피 이전) |
| CON-010 | 2 | 잔여 (낚시 업적/퀘스트 콘텐츠) |
| ARC-029 | 1 | 잔여 (낚시 숙련도 아키텍처) |
| PATTERN-009/010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 FIX-056 + FIX-068 태스크에 따라 자율적으로 작성했습니다.*
