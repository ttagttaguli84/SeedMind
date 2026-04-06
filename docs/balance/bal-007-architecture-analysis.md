# BAL-007 아키텍처 분석 리포트: XP 통합 시뮬레이션 및 영향 분석

> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: BAL-007-ARCH  
> 역할: Technical Architect 관점 분석

---

## Context

이 문서는 BAL-007(XP 통합 시뮬레이션 및 테이블 재검토) 작업의 아키텍처 관점 분석이다. XP 소스 3개(수확/경작, 퀘스트, 업적)가 독립 설계되어 합산 시 필요 XP의 347%가 발생하는 문제를 기술적으로 분석하고, 연쇄 수정이 필요한 문서/섹션/코드 예시를 식별한다.

**본 문서가 canonical인 데이터**:
- BAL-007 관련 연쇄 FIX 아이템 목록
- XP 테이블 변경 시 아키텍처 문서 영향 범위
- 수정 방향에 대한 기술적 복잡도 비교

**본 문서가 canonical이 아닌 데이터 (참조만)**:

| 데이터 종류 | 참조처 |
|------------|--------|
| XP 테이블, XP 소스별 배분, 레벨 시뮬레이션 | `docs/balance/progression-curve.md` |
| 퀘스트 XP 집계, 조정 제안 A/B/C | `docs/balance/quest-rewards.md` (BAL-006) |
| 업적 XP 총량, 보상 기준 테이블 | `docs/content/achievements.md` 섹션 2.4, 12.1 |
| 퀘스트 XP 예산 목표 | `docs/systems/quest-system.md` 섹션 7.3 |
| XPSource enum, ProgressionManager 클래스 | `docs/systems/progression-architecture.md` 섹션 2.2~2.4 |

---

## 1. progression-curve.md XP 소스 배분 현황

### 1.1 현재 배분 구조 (퀘스트/업적 미포함)

`docs/balance/progression-curve.md` 섹션 1.2는 XP 소스를 다음 4개로만 정의한다:

| 소스 | 비중 | 섹션 |
|------|------|------|
| 작물 수확 | ~60% | 섹션 1.2.1 |
| 경작 활동 | ~20% | 섹션 1.2.3 |
| 시설/진행 | ~15% | 섹션 1.2.4 |
| 가공/판매 | ~5% | 섹션 1.2.5 |

**이 4개 소스의 합계가 100%이다.** 퀘스트 XP와 업적 XP는 이 배분 설계에 전혀 포함되어 있지 않다.

### 1.2 퀘스트/업적이 누락된 근거

1. **progression-curve.md 섹션 2(레벨 시뮬레이션)**: 봄/여름/가을/겨울 시뮬레이션에서 계산하는 XP 소스는 수확, 물주기, 호미질, 비료, 시설 건설만 포함. "퀘스트 완료 XP" 또는 "업적 달성 XP" 항목이 없다.

2. **progression-curve.md에서 "퀘스트"/"업적" 언급**: 문서 전체에서 이 단어가 등장하는 곳은 섹션 4.3의 `[OPEN] 칭호/업적 시스템 도입 여부` 뿐이다. 퀘스트는 아예 언급되지 않는다. 이는 progression-curve.md가 퀘스트/업적 시스템 설계 이전에 작성되었기 때문이다.

3. **quest-system.md 섹션 7.3**: 퀘스트 XP를 "기존 소스에 **추가**"로 명시하면서, 전체의 10~15%를 차지하도록 설계한다고 명시. 그러나 이 10~15%가 progression-curve.md의 100% 배분에 반영되지 않았다.

### 1.3 퀘스트/업적 추가 시 총 XP 계산

(-> see `docs/balance/quest-rewards.md` 섹션 3.4 for 상세 집계)

| XP 소스 | 1년차 실현 XP | 전체(전 연차) XP |
|---------|-------------|----------------|
| 수확/경작/시설/가공 (progression-curve.md) | ~3,332 (1년차 시뮬 결과) | 4,609 설계 기준 |
| 퀘스트 전체 (quest-rewards.md) | ~6,247 | ~9,147 |
| 업적 전체 (achievements.md) | ~500 (1년차 추정) | (-> see `docs/content/achievements.md` 섹션 2.4) |
| **합산** | **~10,079** | **~16,006** |

1년차 합산 10,079 XP는 현행 레벨 10 필요 XP(-> see `docs/balance/progression-curve.md` 섹션 2.4.1)의 **약 219%**이다. 조정 후 테이블(누적 9,029) 기준으로도 **112%**로 1년차에 레벨 10 도달이 가능하다.

---

## 2. XP 테이블 변경 영향 분석

### 2.1 XP 테이블 파라미터가 직접 기재된 문서

| 문서 | 섹션 | 기재 내용 | 영향 유형 |
|------|------|----------|----------|
| `docs/balance/progression-curve.md` | 1.3.1, 2.4.1 | baseXP, growthFactor, 레벨 테이블 (canonical) | **원본 변경** |
| `docs/balance/progression-curve.md` | 2.4.2~2.4.3 | 계절별 시뮬레이션, 타임라인 요약 | **재계산 필요** |
| `docs/balance/progression-curve.md` | 2.5 | 최적화 플레이어 타임라인 | **재계산 필요** |
| `docs/balance/quest-rewards.md` | 3.1~3.2 | "전체 누적 XP 4,609" 기준으로 비율 계산 | **기준값 업데이트** |
| `docs/content/achievements.md` | 2.4, 12.2 | "전체 필요 XP 4,609의 약 49%" | **비율 재계산** |
| `docs/systems/quest-system.md` | 7.3 | "전체 XP의 10~15%" 설계 목표 | **목표 비율 재검토** |

### 2.2 XP 관련 코드 예시가 있는 아키텍처 문서

| 문서 | 섹션 | 코드 예시 내용 | 동기화 필요 여부 |
|------|------|--------------|----------------|
| `docs/systems/progression-architecture.md` | 2.2 | `XPSource` enum — QuestComplete/AchievementReward **미포함** (주석으로만 존재) | **enum 확장 필요** |
| `docs/systems/progression-architecture.md` | 2.3 | `GetExpForSource()` switch문 — Quest/Achievement case 없음 | **case 추가 필요** |
| `docs/systems/progression-architecture.md` | 2.4 | `AddExp()` 흐름도 | 변경 불필요 (범용 메서드) |
| `docs/systems/quest-architecture.md` | 섹션 내 | `GrantXP(int baseAmount, int playerLevel)` 메서드 시그니처 | 변경 불필요 (호출측) |
| `docs/systems/achievement-architecture.md` | 섹션 내 | `ProgressionManager.AddXP()` 호출 | 변경 불필요 (호출측) |

### 2.3 핵심 아키텍처 문제: XPSource enum 미확장

현재 `XPSource` enum (-> see `docs/systems/progression-architecture.md` 섹션 2.2):

```
CropHarvest, ToolUse, FacilityBuild, FacilityProcess, MilestoneReward
// 향후 확장: QuestComplete (주석 상태)
```

퀘스트 시스템(`quest-architecture.md`)은 `GrantXP()`를 호출하고, 업적 시스템(`achievement-architecture.md`)은 `ProgressionManager.AddXP()`를 호출하지만, **어떤 XPSource를 사용하는지 정의되지 않았다.** 이는 다음 문제를 야기한다:

- XP 획득 로그에서 퀘스트/업적 출처를 추적할 수 없음
- XP 소스별 통계(디버그/밸런싱용)에서 누락
- 일일 XP 제한 등의 소스별 정책 적용 불가

---

## 3. 연쇄 FIX 아이템 목록

### 3.1 XP 테이블 재조정 (어느 방향이든 필수)

| ID | 문서 | 섹션 | 필요 수정 내용 | 우선순위 |
|----|------|------|--------------|---------|
| FIX-013 | `docs/balance/progression-curve.md` | 1.2 전체 | XP 소스 배분에 퀘스트/업적 카테고리 추가. 6개 소스(수확/경작/시설/가공/퀘스트/업적)의 목표 비중 재설계 | **P1** |
| FIX-014 | `docs/balance/progression-curve.md` | 1.3.1, 2.4.1 | baseXP/growthFactor 재조정 또는 퀘스트 XP 삭감 반영 후 레벨 테이블 재계산 | **P1** |
| FIX-015 | `docs/balance/progression-curve.md` | 2.4.2~2.5 | 계절별 시뮬레이션에 퀘스트/업적 XP 포함한 통합 시뮬레이션 재작성 | **P1** |

### 3.2 참조 문서 비율/수치 업데이트

| ID | 문서 | 섹션 | 필요 수정 내용 | 우선순위 |
|----|------|------|--------------|---------|
| FIX-016 | `docs/balance/quest-rewards.md` | 3.1~3.2 | 기준 XP(현 4,609) 변경 시 모든 비율 재계산 | P2 |
| FIX-017 | `docs/content/achievements.md` | 2.4, 12.2 | "전체 XP 대비 49%" 비율 재계산 + XP 하향 시 업적 보상 수치 변경 | P2 |
| FIX-018 | `docs/systems/quest-system.md` | 7.3 | "10~15%" 목표를 확정 비율로 업데이트, 계절당 XP 예산 재정의 | P2 |

### 3.3 아키텍처 코드 예시 동기화

| ID | 문서 | 섹션 | 필요 수정 내용 | 우선순위 |
|----|------|------|--------------|---------|
| FIX-019 | `docs/systems/progression-architecture.md` | 2.2 | `XPSource` enum에 `QuestComplete`, `AchievementReward` 추가 (주석 해제 + 확장) | **P1** |
| FIX-020 | `docs/systems/progression-architecture.md` | 2.3 | `GetExpForSource()` switch문에 `QuestComplete`, `AchievementReward` case 추가 | **P1** |
| FIX-021 | `docs/systems/progression-architecture.md` | 1 (클래스 다이어그램) | 구독 목록에 `QuestEvents.OnQuestRewarded`, `AchievementEvents.OnAchievementUnlocked` 추가 | P2 |
| FIX-022 | `docs/systems/quest-architecture.md` | GrantXP 메서드 | `AddExp(amount, XPSource.QuestComplete)` 호출로 명시 | P3 |
| FIX-023 | `docs/systems/achievement-architecture.md` | GrantReward 흐름 | `ProgressionManager.AddExp(xp, XPSource.AchievementReward)` 호출로 명시 | P3 |

---

## 4. 권장 수정 방향

### 4.1 기술적 복잡도 비교

| 방향 | 기술적 복잡도 | 변경 문서 수 | 주요 리스크 |
|------|-------------|------------|-----------|
| **A: 퀘스트 XP 삭감** | **낮음** | 3~4개 (quest-system.md, achievements.md, quest-rewards.md, quest-architecture.md) | 퀘스트 보상 수치만 변경. 레벨 테이블/시뮬레이션 구조 유지 |
| **B: XP 테이블 상향** | **높음** | 6~8개 (progression-curve.md 전면 재작성 + 이를 참조하는 모든 문서) | 레벨 테이블 변경 -> 시뮬레이션 전면 재계산 -> 해금 타임라인 재검증 -> 참조 문서 전부 업데이트 |
| **C: 혼합 (소폭 삭감 + 소폭 상향)** | **중간** | 5~6개 | 양쪽 모두 변경하되 폭이 작음 |

### 4.2 권장: 제안 A(퀘스트 XP 삭감) + 소폭 테이블 조정

**기술적으로 제안 A가 가장 단순하다.** 근거:

1. **progression-curve.md의 시뮬레이션 구조 보존**: 현재 시뮬레이션(섹션 2.4.2~2.4.3)은 조정 후 테이블(baseXP 80, growthFactor 1.60)로 이미 합리적인 타임라인을 달성했다. 이 테이블을 다시 바꾸면 시뮬레이션 전체를 재작성해야 한다.

2. **변경 범위 최소화**: 퀘스트 XP 삭감은 `quest-system.md` 섹션 3~6의 보상 테이블과 `achievements.md` 섹션 3~9의 보상 테이블만 수정하면 된다. 반면 XP 테이블 상향은 progression-curve.md 전면 재작성 + 이를 참조하는 6개 이상 문서의 비율/수치 업데이트가 필요하다.

3. **아키텍처 관점 안전성**: 퀘스트/업적 XP가 전체의 10~15% 수준이면, 이들 시스템이 없는 상태(퀘스트 미완료, 업적 미달성)에서도 수확/경작 XP만으로 합리적 레벨업이 가능하다. 이는 시스템 간 **느슨한 결합**을 유지한다. XP 테이블을 20,000으로 올리면 퀘스트가 **필수**가 되어, 퀘스트 시스템에 대한 의존성이 핵심 진행 시스템(ProgressionManager)에 생긴다.

4. **소폭 테이블 조정 여지**: 퀘스트 XP를 ~692로 삭감하고 업적 XP를 ~1,000으로 삭감한 뒤에도 합산이 살짝 높다면, baseXP를 80 -> 90, growthFactor를 1.60 -> 1.65 정도로 미세 조정하는 것은 시뮬레이션 구조를 크게 변경하지 않으면서 여유를 확보할 수 있다.

### 4.3 구체적 수정 시퀀스 (권장 순서)

```
Step 1: [FIX-013] progression-curve.md 섹션 1.2에 퀘스트/업적 XP 배분 카테고리 추가
        목표 배분: 수확 55%, 경작 15%, 시설 12%, 가공 3%, 퀘스트 10%, 업적 5%
        + [FIX-014] 섹션 1.3.1에 [DEPRECATED] 태그 추가, 섹션 2.4.1을 canonical 확정

Step 2: [FIX-012 기존] quest-system.md 섹션 3~6 퀘스트 XP 수치 확정 (제안 A' 기준, 900 XP)
        + [FIX-017 신규] achievements.md 섹션 2.4 XP 비율 9,029 기준으로 정정

Step 3: [FIX-015] progression-curve.md 섹션 2.4에 퀘스트/업적 XP 포함 통합 시뮬레이션 추가
        (기존 시뮬레이션은 "수확/경작만" 시뮬로 유지하되, 통합 시뮬 섹션 신설)

Step 4: [FIX-016~018] quest-rewards.md, achievements.md, quest-system.md의 비율/참조 업데이트

Step 5: [FIX-019~020] progression-architecture.md의 XPSource enum + switch문 확장

Step 6: [FIX-021~023] 아키텍처 문서 간 호출 명시 (P3, 차기 리뷰에서 병합 가능)
```

---

## 5. [RISK] 항목

1. **[RISK]** progression-curve.md의 조정 후 레벨 테이블(섹션 2.4.1)은 이미 원본(섹션 1.3.2)과 달라 두 개의 테이블이 공존하고 있다. BAL-007 수정 시 어느 테이블이 canonical인지 확정하고 다른 하나를 폐기 또는 "이력" 명시해야 한다.

2. **[RISK]** `XPSource` enum에 `QuestComplete`/`AchievementReward`를 추가하면, `GetExpForSource()` switch문뿐 아니라 XP 통계 UI, 세이브 데이터의 소스별 XP 기록 등에도 영향이 파급된다. PATTERN-005(JSON 스키마와 C# 클래스 동기화) 준수 필요.

3. **[RISK]** 퀘스트 XP를 92% 삭감(제안 A)하면, 퀘스트 보상이 "XP 의미 없음 / 골드만 중요"로 전락할 수 있다. 메인 퀘스트에 최소 15~20 XP는 유지해야 "레벨업에 기여하는 느낌"을 보존할 수 있다. 삭감 비율의 최종 확정은 디자이너와 공동 결정 필요.

4. **[RISK]** 업적 XP(현 2,250)가 전체의 5%를 목표로 하면 ~450 XP로 삭감해야 하는데, 30개 업적 중 다수가 "100 XP" 보상을 가지고 있어 개별 업적당 평균 15 XP로 줄어든다. 어려운 업적(전 작물 수확, 모든 가공소 건설)의 보상이 15 XP이면 보상감이 부족할 수 있다.

---

## Cross-references

- `docs/balance/xp-integration.md` -- XP 통합 예산 설계 (BAL-007, 주 문서)
- `docs/balance/progression-curve.md` -- XP 테이블, 시뮬레이션 (canonical, BAL-002)
- `docs/balance/quest-rewards.md` -- 퀘스트 보상 집계, 조정 제안 (BAL-006)
- `docs/content/achievements.md` -- 업적 보상 확정 테이블 (CON-007)
- `docs/systems/quest-system.md` -- 퀘스트 XP 예산 설계 (DES-009)
- `docs/systems/progression-architecture.md` -- ProgressionManager, XPSource enum (ARC)
- `docs/systems/quest-architecture.md` -- QuestRewardGranter, GrantXP (ARC-016)
- `docs/systems/achievement-architecture.md` -- AchievementManager, GrantReward (ARC-017)

---

## Open Questions

- [OPEN] progression-curve.md에 "조정 전 테이블"(섹션 1.3.2)과 "조정 후 테이블"(섹션 2.4.1) 두 개가 공존한다. BAL-007 확정 후 어느 것을 canonical로 할지 결정 필요.
- [OPEN] 퀘스트 XP 삭감 비율의 최종 값. BAL-006 제안 A는 92% 삭감이나, 보상감 유지를 위해 80~85% 삭감이 적절할 수 있다. 디자이너와 공동 결정 필요.
- [OPEN] 업적 XP 목표 비중: 5%로 할 경우 ~450 XP, 8%로 할 경우 ~720 XP. 후자가 보상감 측면에서 유리하나, 퀘스트 XP 배분과 합산 시 초과 여부 재검증 필요.
