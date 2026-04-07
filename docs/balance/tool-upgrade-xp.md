# 도구 업그레이드 XP 밸런스 분석 (Tool Upgrade XP Balance Analysis)

> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: BAL-009

---

## Context

이 문서는 SeedMind의 도구 업그레이드 시 획득하는 XP 보상의 밸런스를 분석한다. 도구 업그레이드 XP는 시설/진행 카테고리(-> see `docs/balance/progression-curve.md` 섹션 1.2.4)에 포함되며, 일회성 보상 성격을 갖는다.

**본 문서가 canonical인 데이터**:
- 도구 업그레이드 XP의 밸런스 분석 결과
- 1년차 실현 가능 업그레이드 횟수 시뮬레이션
- 도구 업그레이드 XP가 전체 XP 예산에서 차지하는 비중 분석
- toolUpgrade 카테고리 확정 권장값

**본 문서가 canonical이 아닌 데이터 (참조만)**:

| 데이터 종류 | 참조처 |
|------------|--------|
| 도구 업그레이드 단계/비용/재료/레벨 요건 | `docs/systems/tool-upgrade.md` 섹션 2.1~2.2 |
| 도구별 업그레이드 XP (각 15 XP) | `docs/systems/tool-upgrade.md` 섹션 8 |
| XP 테이블 (baseXP=80, growthFactor=1.60) | `docs/balance/progression-curve.md` 섹션 2.4.1 |
| 1년차 수확/경작 XP (~3,332 XP) | `docs/balance/progression-curve.md` 섹션 2.4.2 |
| XP 소스별 비율 목표 | `docs/balance/xp-integration.md` 섹션 2.3 |
| 1년차 통합 XP (~4,972 XP, 일반 플레이어) | `docs/balance/xp-integration.md` 섹션 3.2 |

---

## 1. 도구 업그레이드 XP 현황

### 1.1 업그레이드 구조 요약

도구 3종(호미/물뿌리개/낫) x 2단계 업그레이드(Basic->Reinforced, Reinforced->Legendary) = 최대 **6회** 업그레이드 (-> see `docs/systems/tool-upgrade.md` 섹션 1.1).

| 업그레이드 | XP | 골드 비용 | 재료 비용 | 총 실비용 | 레벨 요건 |
|-----------|-----|----------|----------|----------|----------|
| Basic -> Reinforced | 15 XP | (-> see `docs/systems/tool-upgrade.md` 섹션 2.1) | (-> see 섹션 2.2) | 1,100G | 레벨 3 |
| Reinforced -> Legendary | 15 XP | (-> see `docs/systems/tool-upgrade.md` 섹션 2.1) | (-> see 섹션 2.2) | 4,200G | 레벨 7 |

**총 도구 업그레이드 XP**: 15 XP x 6회 = **90 XP**

### 1.2 XP 소스 분류 위치

도구 업그레이드 XP는 progression-curve.md 섹션 1.2.4 "시설/진행 XP"에 이미 등록되어 있다:

> "도구 업그레이드 (각 단계) | 15 XP | 3도구 x 2단계 = 최대 90 XP"

이 90 XP는 시설/진행 카테고리(전체 XP의 약 12%)의 일부이다.

---

## 2. 전체 XP 예산에서의 비중 분석

### 2.1 시설/진행 XP 내역 분해

시설/진행 카테고리 전체 내역 (-> see `docs/balance/progression-curve.md` 섹션 1.2.4):

| 항목 | XP | 비고 |
|------|-----|------|
| 첫 수확 보너스 | 20 XP | 튜토리얼 (1회) |
| 물탱크 건설 | 30 XP | 1회 |
| 창고 건설 | 40 XP | 1회 |
| 온실 건설 | 50 XP | 1회 |
| 가공소 건설 | 60 XP | 1회 |
| 농장 확장 (6단계) | 150 XP | 25 XP x 6 |
| **도구 업그레이드 (6회)** | **90 XP** | **15 XP x 6** |
| **소계** | **440 XP** | |

### 2.2 전체 XP 예산 대비 비중

전체 레벨 10 누적 XP = 9,029 XP (-> see `docs/balance/progression-curve.md` 섹션 2.4.1).

| 분석 관점 | 수치 | 비중 |
|----------|------|------|
| 도구 업그레이드 XP / 전체 XP | 90 / 9,029 | **1.0%** |
| 도구 업그레이드 XP / 시설/진행 XP | 90 / 440 | **20.5%** |
| 시설/진행 XP / 전체 XP | 440 / 9,029 | **4.9%** |

### 2.3 비중 평가

도구 업그레이드 XP(90 XP, 전체의 1.0%)는 **매우 미미한 비중**이다. 이는 의도적 설계이다:

1. **일회성 보상은 보조 역할**: 도구 업그레이드는 6회뿐이며, 핵심 루프(수확/경작)가 XP의 75~80%를 담당해야 한다 (-> see `docs/balance/xp-integration.md` 섹션 2.3)
2. **투자 보상은 XP가 아닌 효율 향상**: 도구 업그레이드의 진짜 보상은 에너지 효율 개선과 범위 확대이다 (-> see `docs/systems/tool-upgrade.md` 섹션 4). XP는 상징적 "달성감" 보상일 뿐이다
3. **기존 비율 구조에 영향 없음**: 90 XP가 추가/제거되어도 전체 비율 배분(수확 55%, 경작 15%, 시설 12%, 가공 3%, 퀘스트 10%, 업적 5%)이 유의미하게 변하지 않는다

---

## 3. 1년차 실현 가능 업그레이드 시뮬레이션

### 3.1 경제적 제약 분석

업그레이드 가능 시점은 (1) 레벨 요건 충족과 (2) 골드 확보라는 이중 제약을 받는다.

**Reinforced 업그레이드 (레벨 3, 총비용 1,100G/개)**:
- 레벨 3 도달 시점: 봄 중후반 (-> see `docs/balance/progression-curve.md` 섹션 2.4.2, 봄 종료 시 ~700 XP, 레벨 3 = 208 XP이므로 봄 중반 도달)
- 봄 종료 시 누적 자금: 약 2,000~3,000G (-> see `docs/systems/tool-upgrade.md` 섹션 2.1 비용 근거)
- 봄~여름에 물탱크(500G), 창고(1,000G) 투자와 경합
- **1년차 Reinforced 실현 가능 횟수: 1~2개** (여유 자금에 따라 3개 가능하나, 시설 투자 포기 필요)

**Legendary 업그레이드 (레벨 7, 총비용 4,200G/개)**:
- 레벨 7 도달 시점: 가을 중후반 (-> see `docs/balance/progression-curve.md` 섹션 2.4.2, 가을 누적 ~2,442 XP, 레벨 7 = 2,104 XP)
- 가을~겨울 시점 자금: 온실(2,000G), 가공소(3,000G) 투자와 경합
- **1년차 Legendary 실현 가능 횟수: 0~1개** (시설 투자를 모두 마친 후 잔여 자금이 있을 때만)

### 3.2 시나리오별 도구 업그레이드 XP (1년차)

| 시나리오 | 업그레이드 내역 | XP | 비고 |
|---------|---------------|-----|------|
| **최소 (캐주얼)** | Reinforced x1 | 15 XP | 봄~여름에 1개만 업그레이드 |
| **일반** | Reinforced x2 | 30 XP | 여름까지 2개 업그레이드 |
| **적극적** | Reinforced x3 + Legendary x1 | 60 XP | 3종 모두 강화 + 1개 전설 |
| **최대 (이론)** | Reinforced x3 + Legendary x3 | 90 XP | 시설 투자 최소화 시에만 가능 |

### 3.3 일반 플레이어 1년차 도구 업그레이드 XP 권장값

일반 플레이어 시나리오(Reinforced x2 = 30 XP) 기준:

| XP 소스 | 1년차 XP | 비고 |
|---------|---------|------|
| 수확/경작 | ~3,332 XP | (-> see `docs/balance/progression-curve.md` 섹션 2.4.2) |
| 시설/진행 (도구 업그레이드 제외) | ~350 XP | 첫 수확 20 + 물탱크 30 + 창고 40 + 온실 50 + 가공소 60 + 농장 확장 2단계 50 = 250 + 기타 |
| **도구 업그레이드** | **~30 XP** | Reinforced x2 |
| 가공/판매 | ~100 XP | 보조 |
| 퀘스트 | ~900 XP | (-> see `docs/balance/xp-integration.md`) |
| 업적 | ~260 XP | 1년차 자연 달성분 (-> see `docs/content/achievements.md`) |
| **합계** | **~4,972 XP** | 레벨 8 중반 (-> see `docs/balance/xp-integration.md` 섹션 3.2) |

도구 업그레이드 30 XP는 전체 4,972 XP 중 **0.6%**. 시설/진행 카테고리(~380 XP) 내에서 **7.9%**.

---

## 4. XP 단가 비교 분석 (보상 적정성)

도구 업그레이드 XP가 투자 대비 적정한지 다른 일회성 XP 보상과 비교한다.

### 4.1 골드당 XP 효율

| 활동 | 비용 | XP | 골드당 XP |
|------|------|-----|----------|
| 도구 Reinforced 업그레이드 | 1,100G | 15 XP | 0.014 XP/G |
| 도구 Legendary 업그레이드 | 4,200G | 15 XP | 0.004 XP/G |
| 물탱크 건설 | 500G | 30 XP | 0.060 XP/G |
| 창고 건설 | 1,000G | 40 XP | 0.040 XP/G |
| 온실 건설 | 2,000G | 50 XP | 0.025 XP/G |
| 가공소 건설 | 3,000G | 60 XP | 0.020 XP/G |

도구 업그레이드의 골드당 XP 효율은 시설 건설 대비 **2~10배 낮다**. 이는 적절하다:

- 도구 업그레이드의 주된 보상은 XP가 아닌 **에너지 효율 향상**이다
- 시설 건설의 주된 보상은 **새 기능 해금**이며, XP가 더 높은 비중을 차지한다
- XP를 위해 도구를 업그레이드하는 것은 비효율적이므로, 도구 업그레이드 결정은 순수하게 **농장 효율** 관점에서 이루어진다 → 의미 있는 선택

### 4.2 단계별 XP 동일 여부 검토

현재 설계는 Reinforced와 Legendary 모두 15 XP로 동일하다 (-> see `docs/systems/tool-upgrade.md` 섹션 8).

**동일 XP 유지 근거**:
1. **단순성**: 업그레이드 단계별로 XP를 차등화할 필요가 없는 수준의 소량(15 XP)
2. **투자 보상 분리**: Legendary의 추가 보상은 XP가 아닌 특수 효과(물뿌리개 성장 +5%, 낫 품질 +10% 등)에 이미 반영
3. **전체 영향도 미미**: 15 XP -> 25 XP로 올려도 총 차이는 60 XP뿐 (6회 x 10 XP 차이). 전체 9,029 XP 대비 0.7% 변화

[OPEN] Legendary 업그레이드 XP를 25 XP로 상향하여 투자 대비 성취감을 높일지 검토. 총 영향 +30 XP(90 -> 120 XP)로 전체 밸런스에 미미하나, "전설 도구 완성"이라는 마일스톤에 더 큰 보상감을 줄 수 있다.

---

## 5. progression-curve.md toolUpgrade 카테고리 확정

### 5.1 현재 상태

progression-curve.md 섹션 1.2.4에 이미 "도구 업그레이드 (각 단계) 15 XP, 3도구 x 2단계 = 최대 90 XP"로 등록되어 있다. 이 값은 tool-upgrade.md 섹션 8과 일치한다.

### 5.2 확정 권장값

| 항목 | 값 | 근거 |
|------|-----|------|
| 카테고리명 | `toolUpgrade` | 시설/진행 XP 하위 |
| 단계별 XP | **15 XP** (유지) | 섹션 4.2 분석 근거 |
| 총 횟수 | **6회** (3도구 x 2단계) | (-> see `docs/systems/tool-upgrade.md` 섹션 1.1) |
| 총 XP | **90 XP** | 15 x 6 |
| 전체 예산 비중 | **1.0%** (90 / 9,029) | 일회성 보조 보상으로 적절 |
| 1년차 실현 기대값 | **~30 XP** (일반 플레이어) | Reinforced x2 시나리오 |

### 5.3 기존 카테고리 비중 충돌 검증

90 XP를 시설/진행 카테고리에서 차감하여 비율을 재계산:

| XP 소스 | 기존 비중 | 도구 업그레이드 90 XP 포함 후 비중 | 변화 |
|---------|----------|--------------------------------|------|
| 작물 수확 | ~55% | ~55% | 변화 없음 |
| 경작 활동 | ~15% | ~15% | 변화 없음 |
| 시설/진행 | ~12% | ~12% (도구 XP 이미 포함) | 변화 없음 |
| 가공/판매 | ~3% | ~3% | 변화 없음 |
| 퀘스트 | ~10% | ~10% | 변화 없음 |
| 업적 | ~5% | ~5% | 변화 없음 |

**결론: 도구 업그레이드 XP(90 XP)는 시설/진행 카테고리에 이미 포함되어 있으므로, 비율 충돌이 발생하지 않는다.** progression-curve.md 섹션 1.2의 기존 비율 구조를 변경할 필요가 없다.

---

## 6. 2년차 이후 도구 업그레이드 XP 영향

### 6.1 2년차 잔여 업그레이드

1년차에 일반 플레이어가 Reinforced x2를 완료하면, 2년차에 남는 업그레이드:

- Reinforced x1 (15 XP)
- Legendary x3 (45 XP)
- **2년차 잔여 도구 XP: 60 XP**

2년차 레벨 목표가 레벨 9~10 구간(필요 XP: 2,147~3,436)인 점을 감안하면, 60 XP는 레벨업에 유의미한 기여를 하지 않는다. 이는 적절하다 -- 2년차에서도 핵심 루프(수확/경작)가 주 소스여야 한다.

---

## Cross-references

| 문서 | 참조 내용 |
|------|-----------|
| `docs/systems/tool-upgrade.md` 섹션 1.1 | 도구 3종, 3단계(Basic/Reinforced/Legendary) 정의 |
| `docs/systems/tool-upgrade.md` 섹션 2.1~2.2 | 업그레이드 비용, 재료, 레벨 요건 (canonical) |
| `docs/systems/tool-upgrade.md` 섹션 4 | 에너지 효율 비교표 |
| `docs/systems/tool-upgrade.md` 섹션 8 | 도구 업그레이드 XP 보상 (15 XP/단계, canonical) |
| `docs/balance/progression-curve.md` 섹션 1.2.4 | 시설/진행 XP 목록 (도구 업그레이드 90 XP 포함) |
| `docs/balance/progression-curve.md` 섹션 2.4.1 | XP 테이블 (baseXP=80, growthFactor=1.60, 레벨 10=9,029 XP) |
| `docs/balance/progression-curve.md` 섹션 2.4.2 | 1년차 일반 플레이어 시뮬레이션 (~3,332 XP) |
| `docs/balance/xp-integration.md` 섹션 2.3 | XP 소스별 비율 목표 |
| `docs/balance/xp-integration.md` 섹션 3.2 | 1년차 통합 XP (~4,972 XP) |
| `docs/content/achievements.md` 섹션 2.4 | 업적 XP (2,250 XP 전체) |
| `docs/systems/tool-upgrade-architecture.md` 섹션 5.1.1 | ProgressionManager 연동 흐름 — 본 문서를 canonical XP 값 참조처로 지정 |

---

## Open Questions

1. [OPEN] **Legendary 업그레이드 XP 상향 여부**: 현재 Reinforced와 Legendary 모두 15 XP이나, Legendary를 25 XP로 상향하면 "전설 도구 완성"의 성취감이 높아진다. 총 영향은 +30 XP(90 -> 120 XP)로 미미. 다만 tool-upgrade.md 섹션 8과 progression-curve.md 섹션 1.2.4 양쪽 수정이 필요하므로, 결정 시 동시 업데이트 필수.

2. [OPEN] **재료 드롭 경로가 XP에 미치는 영향**: tool-upgrade.md Open Question 1에서 재료 드롭 경로 추가를 검토 중이다. 드롭이 추가되면 업그레이드 실비용이 낮아져 1년차 실현 횟수가 증가(일반: 2 -> 3회)하고, 1년차 도구 XP도 30 -> 45 XP로 소폭 상승한다. 전체 밸런스 영향은 미미하나, 경제 시뮬레이션(progression-curve.md 섹션 3)과의 정합성 재확인 필요.

3. [OPEN] **도구 업그레이드 업적과의 XP 중복**: "모든 도구 Reinforced" 또는 "모든 도구 Legendary" 업적이 achievements.md에 추가될 경우, 업적 XP와 도구 업그레이드 XP가 이중 보상이 된다. 이중 보상 자체는 문제가 아니지만(업적은 마일스톤 보상), 합산 XP가 과도하지 않은지 검증 필요.

---

## Risks

1. [RISK] **도구 업그레이드 XP가 너무 미미하여 무의미할 가능성**: 15 XP는 감자 3회 수확(15 XP)과 동일하다. 1,100G~4,200G를 투자한 대가치고 XP 보상이 너무 작다고 느낄 수 있다. 다만 도구 업그레이드의 핵심 보상은 에너지 효율이므로, XP는 "달성 알림" 수준으로 충분하다는 판단이다. 플레이테스트 후 체감 피드백에 따라 조정.

2. [RISK] **1년차 Legendary 업그레이드 접근성 불확실**: 레벨 7 + 4,200G/개 조건이 1년차 가을~겨울에 충족 가능한지는 개별 플레이 패턴에 크게 의존한다. 시설 투자(온실 2,000G, 가공소 3,000G)를 모두 마친 후 Legendary 1개를 올릴 여력이 있는지는 경제 시뮬레이션(progression-curve.md 섹션 3)의 자금 곡선에 따라 달라진다.

---

*이 문서는 도구 업그레이드 XP 밸런스 분석의 canonical 문서이다. 도구 업그레이드 단계별 XP 값 자체는 `docs/systems/tool-upgrade.md` 섹션 8이 canonical이며, 본 문서는 해당 값의 밸런스 적정성을 분석한다.*
