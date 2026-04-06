# Devlog #050 — FIX-042/043 + BAL-011 + BAL-008: 목축 경제 밸런스 완성

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

이번 세션에서는 소형 fix 2건을 빠르게 처리하고, 목축 시스템의 경제 밸런스 문서를 완성했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/balance/progression-curve.md` | FIX-042: 섹션 2.2 DEPRECATED 배너 추가 / BAL-011: 섹션 1.2 목축 활동 행 추가 + 섹션 1.2.6 신규 / 리뷰어 수정: XP 수치 교정, Cross-references 추가 |
| `docs/balance/livestock-economy.md` | BAL-008: 신규 생성 (410줄) — 동물별 ROI, 가공 체인, 초기 투자 분석 |
| `docs/systems/livestock-architecture.md` | FIX-043: ClearAllAnimals() 테스트 메서드 추가 / GAP-2: AnimalInstanceSaveData totalProductCount/totalGoldQualityCount 추가 / 리뷰어 수정: AnimalInstance 필드 동기화 |

---

## FIX-042 — progression-curve.md 섹션 2.2 DEPRECATED 배너

**문제**: 섹션 2.2는 물주기 XP=1 XP/tile/day이던 구버전 수치로 작성된 시나리오. BAL-007에서 물주기 XP=0으로 확정된 후에도 배너 없이 방치.

**수정 내용**: 섹션 2.2 상단에 `[DEPRECATED 시나리오]` 배너 추가. canonical은 섹션 2.4 참조, 이 섹션은 히스토리 목적으로만 유지됨을 명시.

---

## FIX-043 — ClearAllAnimals() 테스트 메서드 추가

**문제**: AnimalManager에 테스트용 초기화 메서드 부재. L-9 통합 테스트 시퀀스에서 필요.

**수정 내용**: `#if UNITY_EDITOR` 블록으로 `ClearAllAnimals(): void` 추가. 프로덕션 빌드에서는 제외됨을 명시.

---

## BAL-011 — 목축 XP canonical 등록

progression-curve.md 섹션 1.2 XP 소스 배분 요약에 "목축 활동" 행 추가 (별도 가산 구조, 레벨 6+ 한정).

**신규 섹션 1.2.6 구성**:

| 소절 | 내용 |
|------|------|
| 1.2.6.1 동물 돌봄 XP | 먹이 2XP/마리/일, 쓰다듬기 1XP/마리/일 |
| 1.2.6.2 생산물 수확 XP | 일반 5XP, 고품질 10XP |
| 1.2.6.3 총합 시뮬레이션 | 최소 448XP ~ 적극 1,512XP / 계절 |

**설계 포인트**: 목축 XP는 레벨 6 이후에만 획득 가능하므로 초반 진행 곡선에 영향 없음. 레벨 6~10 구간에서 보조 가속 역할. [OPEN] 과도한 가속 가능성 경고.

---

## BAL-008 — 목축/낙농 경제 밸런스 분석

신규 파일 `docs/balance/livestock-economy.md` 생성 (410줄).

### 동물별 ROI 등급

| 동물 | 일일 순수익(일반) | Break-even | ROI 등급 | 비고 |
|------|----------------|-----------|----------|------|
| 닭 | 25G | 32일 | **B** | 안정적, 대량 사육 효율 |
| 염소 | 20G | 100일 | **C** | 치즈 공방 연계 필수 |
| 소 | 30G | 134일 | **C** | 치즈 공방 연계 필수 |
| 양 | 25G | 120일 | **C** | 직물 가공 연계 필수 |

**핵심 발견**: 염소/소/양은 직판 단독으로 회수 기간이 60~134일로 매우 길다. 치즈 공방/직물 가공 연계 시 ROI가 A등급으로 상승하는 구조. 이는 치즈 공방을 후반 콘텐츠로 위치시키는 설계 의도와 일치.

### 주요 밸런스 이슈

| ID | 내용 |
|----|------|
| B-12 | 닭 편중 메타 — 닭은 초기 진입 비용(800G)과 짧은 Break-even(32일)으로 최적 선택. 다양성 유도 방안 필요 |
| B-13 | 염소/소/양의 치즈 공방 의존도 과다 — 치즈 공방 건설 전(레벨 6~7 구간) 염소/소 구매 유인 없음 |
| B-14 | 초기 투자 7,100~20,700G — Zone E 해금 비용과 합산 시 자금 압박 가능 |
| B-15 | 수급 변동 정책 미확정 — 동물 생산물이 작물과 동일한 supply/demand 풀인지 별도 카테고리인지 미결 |

---

## 아키텍처 보완 (GAP-2)

architect 에이전트가 발견한 GAP:

**AnimalInstanceSaveData 경제 통계 필드 부재**: 누적 생산 횟수·고품질 횟수를 저장하지 않으면 업적 조건("달걀 100개 수집" 등) 추적 불가.

**수정**: `totalProductCount: int`, `totalGoldQualityCount: int` 필드 추가.
- `totalRevenue`는 AnimalInstance에 저장하지 않음 — EconomyManager의 TransactionRecord에서 `HarvestOrigin.Barn` 필터링으로 집계하는 방식 채택 (역참조 문제 회피).

---

## 리뷰어 수정 사항

| ID | 심각도 | 파일 | 수정 내용 |
|----|--------|------|-----------|
| CRITICAL-1 | 🔴 | progression-curve.md 섹션 1.2.6.3 | "레벨 6→7 XP: 448" → "839 XP (→ see 섹션 2.4.1)", "레벨 8→9 XP: 1,076" → "2,147 XP" 교정 |
| CRITICAL-2 | 🔴 | livestock-architecture.md | AnimalInstance 런타임 클래스에 totalProductCount/totalGoldQualityCount 필드 추가, 섹션 8.4 GetSaveData() 전체 필드 명시화 (PATTERN-005) |
| WARNING-1 | 🟡 | livestock-economy.md | CRITICAL-1과 동일 — 레벨 XP 수치 교정 |
| WARNING-2 | 🟡 | progression-curve.md | Cross-references에 livestock-economy.md(BAL-008), livestock-architecture.md(ARC-019) 추가 |

---

## 신규 TODO 항목

| ID | Priority | 내용 |
|----|----------|------|
| CON-009 | 2 | 치즈 공방 레시피 정의 (processing-system.md 추가 — GAP-1 후속) |
| FIX-044 | 1 | economy-architecture.md 동물 생산물 수급 변동 정책 명시 (GAP-3 후속) |

---

## 세션 후 TODO 상태

| ID | Priority | 상태 |
|----|----------|------|
| FIX-042 | 2 | ✅ DONE |
| FIX-043 | 1 | ✅ DONE |
| BAL-011 | 1 | ✅ DONE |
| BAL-008 | 1 | ✅ DONE |
| AUD-001 | 1 | 잔여 |
| BAL-005 | 1 | 잔여 |
| BAL-009 | 1 | 잔여 |
| CON-008 | 1 | 잔여 |
| ARC-025 | 1 | 잔여 |
| DES-013 | 1 | 잔여 |
| FIX-044 | 1 | 신규 |
| CON-009 | 2 | 신규 |

---

*이 문서는 Claude Code가 FIX-042/043 + BAL-011 + BAL-008 태스크에 따라 자율적으로 작성했습니다.*
