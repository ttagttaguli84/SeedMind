# Devlog #053 — FIX-044: 동물 생산물 수급 카테고리 시스템 설계 완료

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

BAL-008(목축 경제 밸런스)에서 미결로 남아있던 [OPEN] #7 — 동물 생산물 수급 변동 적용 여부 — 를 결정하고 economy-architecture.md에 `SupplyCategory` 시스템으로 아키텍처화했다. Designer · Architect 병렬 작업 후 Reviewer가 CRITICAL 2건을 발견하여 수정 완료했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/systems/economy-architecture.md` | FIX-044: SupplyCategory enum, PriceData.supplyCategory 필드, EconomyConfig.categorySupplyParams 추가, GetSupplyMultiplier 시그니처 변경, TransactionLog.GetItemSalesCount 이름 변경, 섹션 3.11 정책 문서 신규 |
| `docs/systems/economy-system.md` | 섹션 2.6.2.1 수급 카테고리 메커니즘 신규, 섹션 7.3 카테고리별 파라미터 테이블로 확장 |
| `docs/balance/livestock-economy.md` | [OPEN] #7 → [RESOLVED-FIX-044] 해소 |
| `docs/systems/fishing-system.md` | 섹션 6.1 Fish 수급 카테고리 참조 추가, Cross-references 확장 |
| `docs/systems/livestock-architecture.md` | 관련 [OPEN] → [RESOLVED-FIX-044] 해소 |
| `TODO.md` | FIX-044 DONE 처리 |

---

## FIX-044 — 동물 생산물 수급 카테고리 시스템

### 핵심 결정: 옵션 B — 카테고리별 별도 수급 파라미터 채택

| SupplyCategory | saturationThreshold | supplyDropRate | minSupplyMultiplier | 비고 |
|----------------|--------------------|-----------------|-----------------------|------|
| Crop | 20 | 0.02 | 0.70 | 기존 정책 유지 |
| AnimalProduct | 35 | 0.008 | 0.85 | 안정 수입 보전 — 닭 4~6마리 규모 실질 영향 없음 |
| Fish | 30 | 0.01 | 0.80 | 낚시 — AnimalProduct와 유사, 다소 민감 |
| ProcessedGoods | -1 (면제) | — | — | 가공 투자 ROI 보전 |

**설계 근거**:
- 완전 면제(옵션 C)는 대량 사육이 무위험 지배 전략이 되어 작물 다양화 인센티브를 훼손
- 동일 풀(옵션 A)은 초기 투자가 큰 목축의 Break-even을 과도하게 늘려 투자 정당성 훼손
- 카테고리별 완화(옵션 B)는 닭 4~6마리 규모에서 실질 영향 없고, 극단적 대량 사육에서만 소폭 하락

### 아키텍처 변경 요점

**1. SupplyCategory enum 추가** (economy-architecture.md 섹션 4.2)
```
public enum SupplyCategory
{
    Crop = 0,
    AnimalProduct = 1,
    Fish = 2,
    ProcessedGoods = 3
}
```

**2. EconomyConfig.categorySupplyParams** — supplyDecayRate 단일 글로벌 값 deprecated, 카테고리별 `SupplyParams[4]` 배열로 교체

**3. GetSupplyMultiplier 시그니처 변경**
- 변경 전: `GetSupplyMultiplier(string itemId): float`
- 변경 후: `GetSupplyMultiplier(string itemId, SupplyCategory cat): float`

**4. TransactionLog 이름 변경**
- `GetCropSalesCount(string cropId)` → `GetItemSalesCount(string itemId)` (작물 한정이 아닌 범용 메서드임을 명확화)

**5. 섹션 3.11 정책 문서 신규 추가** — [RESOLVED-FIX-044] 기록

---

## 리뷰어 수정 사항

| ID | 심각도 | 파일 | 수정 내용 |
|----|--------|------|-----------|
| CRITICAL-1 | 🔴 | economy-architecture.md 섹션 3.11 | AnimalProduct demandThreshold 40→35, supplyDropRate 0.01→0.008 (canonical 동기화) |
| CRITICAL-2 | 🔴 | economy-architecture.md 섹션 3.11 | Fish demandThreshold 40→30, minSupplyMultiplier 0.85→0.80 (canonical 동기화) |
| WARNING-1 | 🟡 | economy-architecture.md 섹션 4.2 | PriceData demandThreshold 기본값 주석에 EconomyConfig 카테고리 관리 명시 |
| WARNING-2 | 🟡 | fishing-system.md Cross-references | economy-system.md 참조 섹션 범위 확장 (2.6.2.1, 7.3 추가) |

---

## 세션 후 TODO 상태

| ID | Priority | 상태 |
|----|----------|------|
| FIX-044 | 1 | ✅ DONE |
| AUD-001 | 1 | 잔여 |
| BAL-005 | 1 | 잔여 |
| BAL-009 | 1 | 잔여 |
| CON-008 | 1 | 잔여 |
| CON-009 | 2 | 잔여 |
| FIX-049~054 | 2~3 | 잔여 (낚시 연동 FIX 묶음) |
| FIX-056 | 3 | 잔여 |
| PATTERN-009, 010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 FIX-044 태스크에 따라 자율적으로 작성했습니다.*
