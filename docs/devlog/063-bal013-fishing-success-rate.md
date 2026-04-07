# Devlog #063 — BAL-013: 낚시 미니게임 성공률 확정 및 경제 재계산

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

BAL-012(낚시 경제 밸런스)에서 식별된 핵심 RISK — "Lv.1 낚시 일일 수익 591G가 수박 24칸(350G)을 초과"를 BAL-013으로 해소했다. 낚시 미니게임 성공률을 숙련도별로 공식 확정하고, 연관된 모든 문서를 동기화했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/systems/fishing-system.md` | 섹션 7.4: "미니게임 성공률" 행 canonical 추가 (Lv.1=50%, Lv.5=65%, Lv.10=80%) + 설계 의도 주석 |
| `docs/balance/fishing-economy.md` | 섹션 3.2~3.5, 5.3~5.4, 6.1, 6.6: 성공률 기반 시뮬레이션 전면 재계산 |
| `docs/systems/fishing-architecture.md` | 섹션 4A.2/4A.3/4A.6/4A.7: FishingProficiency GetMiniGameSuccessRate() 추가 |

---

## BAL-013 — 낚시 성공률 확정

### 확정 근거

낚시 성공률은 BAL-012 조정안 A를 수용하여 아래와 같이 확정한다:

| 숙련도 | 기존 추정 | 확정값 | 조정 이유 |
|--------|----------|--------|----------|
| Lv.1 | 70~80% | **50%** | Lv.1 수익 591G → 394G로 낮춰 작물 수준 근접 |
| Lv.5 | 80% | **65%** | 성장 곡선 유지, 숙련 투자의 체감 향상 |
| Lv.10 | 90% | **80%** | 마스터 단계의 보상으로 수용 (수급 적용 후 1,491G) |

성공률 수치 canonical: `docs/systems/fishing-system.md` 섹션 7.4.

### 수익 재계산 결과

| 시나리오 | 조정 전 수익 | 수급 미적용 | 수급 적용 (×0.8) |
|----------|------------|-----------|---------------|
| Lv.1 (성공률 50%) | 591G | 394G | **315G** |
| Lv.5 (성공률 65%) | 720G | 593G | **474G** |
| Lv.10 (성공률 80%) | 1,863G | 1,864G | **1,491G** |

**포지셔닝 재판정**:
- Lv.1 수급 적용 수익 315G < 수박 24칸 350G → "보조 수입" 포지셔닝 달성
- Lv.10 1,491G는 높지만, 1~2계절 투자 후 후반 보상으로 수용 가능

### 에너지 효율 재계산

성공률 변경에 따라 회당 평균 에너지도 바뀐다:

| 숙련도 | 에너지/회 | 계산 근거 |
|--------|----------|----------|
| Lv.1 | 2.5E | 2×0.5 + 3×0.5 |
| Lv.5 | 2.35E | 2×0.65 + 3×0.35 |
| Lv.8+ (Lv.10) | 1.2E | 1×0.8 + 2×0.2 |

---

## 아키텍처 반영 (ARC-029 후속)

### GetMiniGameSuccessRate() 추가

`fishing-architecture.md` 섹션 4A.6에 7번째 보정 메서드를 추가했다:

```
GetMiniGameSuccessRate(): float
    return _config.successRateByLevel[_currentLevel - 1]
    // Lv.1=0.50, Lv.5=0.65, Lv.10=0.80
    // -> see docs/systems/fishing-system.md 섹션 7.4
```

FishingConfig SO에 `successRateByLevel: float[]` 배열이 추가되었다.

### FishingManager 통합 지점 추가

섹션 4A.7에 6번 지점(미니게임 성공 판정)을 추가했다:

```
6) FishingMinigame.EvaluateResult():
    float successRate = _proficiency.GetMiniGameSuccessRate()
    bool minigameSuccess = minigame.IsTargetZoneReached() &&
                           (Random.value < successRate)
```

설계 의도: 낮은 숙련도(Lv.1 50%)에서는 게이지를 채워도 RNG로 실패 가능 → "낚시가 처음엔 어렵다"는 체감 구현.

---

## 리뷰어 검증 결과

| ID | 심각도 | 이슈 내용 | 수정 결과 |
|----|--------|-----------|-----------|
| R-01 | 🟡 WARNING | fishing-economy.md 섹션 2.5 에너지/회 수치가 BAL-013 이전 기준(70%)으로 잔존 | 2.5/2.35/1.2로 전수 정정 |
| R-02 | 🔵 INFO | fishing-economy.md 섹션 3.4 수익 1,861G → 정확한 계산값 1,864G | 1,864G로 정정, 5.4/6.1 연쇄 반영 |

---

## 세션 후 활성 TODO

| ID | Priority | 상태 |
|----|----------|------|
| CON-009 | 2 | 잔여 (치즈 공방 레시피) |
| DES-014 | 2 | 잔여 (겨울 씨앗 판매 경로) |
| CON-010 | 2 | 잔여 (낚시 업적/퀘스트 콘텐츠) |
| FIX-071 | 2 | 잔여 (겨울 낚시 허용 여부) |
| BAL-014 | 1 | 신규 (낚시 숙련도 XP 밸런스 — BAL-013 후속) |
| PATTERN-009/010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 BAL-013 태스크에 따라 자율적으로 작성했습니다.*
