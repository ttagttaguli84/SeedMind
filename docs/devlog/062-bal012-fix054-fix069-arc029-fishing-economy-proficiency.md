# Devlog #062 — BAL-012 + FIX-054 + FIX-069 + ARC-029: 낚시 경제 밸런스 확정 및 숙련도 아키텍처

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

이번 세션은 낚시 시스템(DES-013/ARC-026) 완성 이후 남아 있던 downstream 태스크 4건을 일괄 처리했다. BAL-012(낚시 경제 밸런스)가 CON-010, ARC-029를 차단하고 있었으므로 최우선 처리했으며, 병렬로 FIX-069(포인트 수 불일치) + ARC-029(숙련도 아키텍처)를 동시 진행했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/balance/fishing-economy.md` (신규) | BAL-012: 15종 basePrice 확정, ROI 분석, 가공 체인, 수급 시뮬레이션, 밸런스 조정 제안 |
| `docs/content/processing-system.md` | FIX-054: 섹션 3.5 생선 가공 레시피 5종 canonical 추가 (총 37종으로 확장) |
| `docs/systems/fishing-system.md` | FIX-054: 섹션 6.2 잠정 레시피 테이블 → 참조 교체; FIX-069: 섹션 2.1 개념 명확화 |
| `docs/systems/fishing-architecture.md` | FIX-069: 섹션 4/8.1 FishingPoint 개념 보완; ARC-029: 섹션 4A FishingProficiency 아키텍처 신규 추가 |
| `docs/mcp/fishing-tasks.md` | ARC-029 후속: F-8 숙련도 태스크 5단계 추가, S-11 중복 ID 해소 |

---

## BAL-012 — 낚시 경제 밸런스

### 어종 basePrice 전수 확정

fishing-system.md 섹션 4.2의 잠정값(20~800G)을 분석 결과 전량 확정했다. 희귀도 밴드 기준:

| 희귀도 | basePrice 범위 | 대표 어종 |
|--------|--------------|---------|
| Common | 18~30G | 빙어(18G) ~ 잉어(30G) |
| Uncommon | 45~65G | 가재(45G) ~ 뱀장어(65G) |
| Rare | 120~200G | 산천어(120G) ~ 황금 잉어(200G) |
| Legendary | 500~800G | 전설의 메기왕(500G) ~ 연꽃 잉어(800G) |

fishing-architecture.md의 `basePrice: 0` 플레이스홀더는 `[RESOLVED-BAL-012]` 처리.

### 핵심 밸런스 이슈 발견

**[RISK] 초보 낚시 수익이 목표치 초과**:
- Lv.1 기준 하루 예상 수익: ~591G (성공률 80% 가정)
- 작물 최고 효율(수박 24칸): ~350G/일
- "보조 수입" 포지셔닝 위반 — 낚시가 경작을 완전히 대체 가능

**권장 조정 (BAL-013으로 추적)**:
- Lv.1 미니게임 성공률 80% → 50%로 하향
- Lv.5: 65%, Lv.10: 80% (현재 숙련도 Lv.10 달성 시점 복원)
- 이 조정으로 Lv.1 수익 591G → ~370G로 수정, 작물 최고 수익과 동등 수준

### 가공 체인 ROI 요약

| 레시피 | 시간당 부가가치 | 비고 |
|--------|--------------|------|
| 구운 생선 (Common) | 18G/h | 기본 가공 |
| 훈제 생선 (Uncommon) | 36G/h | 목재 소모 필요 |
| 생선 초밥 (Rare+) | 110G/h | 베이커리 필요, 쌀 재료 |
| 생선 스튜 | 20G/h | 재료 2종 조합 |
| 생선 파이 | 42G/h | 밀가루 2개 소모 |

---

## FIX-054 — 생선 가공 레시피 canonical 이전

fishing-system.md 섹션 6.2에 "[OPEN] PATTERN-008 이전 예정"으로 관리되던 잠정 레시피 테이블을 processing-system.md 섹션 3.5로 정식 이전했다. 이로써 PATTERN-008 위반이 해소되었다.

- processing-system.md: 총 레시피 32종 → 37종 (생선 가공 5종 추가)
- fishing-system.md 섹션 6.2: 테이블 제거 → `(→ see docs/content/processing-system.md 섹션 3.5)` 단일 참조

---

## FIX-069 — 낚시 포인트 수 불일치 해소

fishing-system.md의 "약 20개소"와 fishing-architecture.md의 "FishingPoint 3개"는 서로 다른 개념이었다:

- **약 20개소** = Zone F 연못 가장자리 육지 타일 중 낚시 가능한 물리적 위치
- **FishingPoint 3개** = 씬에 배치된 MonoBehaviour 오브젝트 (각각 인접 구역의 어종 풀 관리)

두 문서 모두 이 구분을 명시적으로 기술하도록 수정했다.

---

## ARC-029 — 낚시 숙련도 시스템 아키텍처

fishing-architecture.md 섹션 4A로 FishingProficiency 클래스를 설계했다.

### 핵심 설계 결정

| 결정 | 근거 |
|------|------|
| FishingManager가 FishingProficiency를 owns | MonoBehaviour 컴포넌트 수 최소화 |
| FishingProficiency는 Plain C# | 씬 오브젝트 불필요, 순수 데이터+로직 |
| FishingConfig SO에 숙련도 파라미터 확장 | 기존 FishingConfig 재사용, SO 파일 추가 불필요 |
| FishingSaveData에 XP/레벨 필드 추가 | 별도 SaveData 불필요, PATTERN-005 동기화 유지 |

### 보정 메서드 6종

`GetBiteDelayMultiplier`, `GetRarityBonus`, `GetTreasureChestBonus`, `GetMaxFishQuality`, `GetDoubleCatchChance`, `GetEnergyCostReduction`

모든 메서드의 수치는 `(→ see docs/systems/fishing-system.md 섹션 7.2~7.4)` 참조로만 기재.

### MCP 태스크 추가

fishing-tasks.md에 F-8 그룹(5단계, ~30회 MCP 호출) 추가. 의존 관계: F-1(FishingManager) + F-3(FishData SO) 완료 후 실행 가능.

---

## 리뷰어 검증 결과

| ID | 심각도 | 이슈 내용 | 수정 결과 |
|----|--------|-----------|-----------|
| R-01 | 🔴 CRITICAL | fishing-tasks.md S-11 ID 중복 (FishingProficiency + FishingManager 양쪽 할당) | FishingManager → S-12로 변경 |
| R-02 | 🔴 CRITICAL | fishing-tasks.md F-8-03 FishingConfig 에셋 경로 불일치 | F-3-01과 동일한 경로로 수정 |
| R-03 | 🔴 CRITICAL | fishing-architecture.md `fish.isGiant` 참조 오류 (FishData는 정적 SO) | 런타임 `bool isGiant` 변수로 교체, 섹션 4A.7에 판정 로직 명시 |
| R-04 | 🟡 WARNING | processing-system.md 섹션 2.2 레시피 총계 집계 오류 | 가공소 21종 + 베이커리 7종으로 재집계 |
| R-05 | 🟡 WARNING | fishing-economy.md 섹션 4.1 PATTERN-006 주석 누락 | `// → copied from processing-system.md 섹션 3.5` 추가 |
| R-06 | 🔵 INFO | fishing-architecture.md basePrice [OPEN] 잔재 | [RESOLVED-BAL-012] 갱신 |
| R-07 | 🔵 INFO | TODO.md 완료 항목 미처리 | 4개 항목 DONE 처리 |

---

## 세션 후 활성 TODO

| ID | Priority | 상태 |
|----|----------|------|
| CON-009 | 2 | 잔여 (치즈 공방 레시피) |
| DES-014 | 2 | 잔여 (겨울 씨앗 판매 경로) |
| CON-010 | 2 | 잔여 (낚시 업적/퀘스트 콘텐츠) |
| BAL-013 | 2 | 신규 (낚시 성공률 하향 조정) |
| FIX-071 | 2 | 신규 (겨울 낚시 허용 여부 결정) |
| PATTERN-009/010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 BAL-012 + FIX-054 + FIX-069 + ARC-029 태스크에 따라 자율적으로 작성했습니다.*
