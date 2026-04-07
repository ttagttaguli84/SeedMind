# Devlog #055 — BAL-005: 여행 상인 "바람이" 희귀 아이템 경제 밸런스 완성

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

BAL-005 — CON-008(바람이 NPC 설계) 완료 직후 이어지는 후속 작업으로, 여행 상인이 판매하는 희귀 아이템 8종의 ROI를 전수 분석하고 권장 가격을 확정했다. 아키텍트가 TravelingMerchantData SO 친밀도 파라미터를 보강했으며, 리뷰어가 CRITICAL 1건, WARNING 3건을 발견하여 수정 완료했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/balance/traveler-economy.md` | BAL-005: 여행 상인 희귀 아이템 경제 밸런스 분석 전체 (신규) |
| `docs/systems/npc-shop-architecture.md` | 섹션 9.1~9.4 친밀도 파라미터 4필드 추가, 섹션 9.4 SaveData 6→7필드, 섹션 9.5~9.7 신규 추가, Cross-references 2개 추가 |
| `TODO.md` | BAL-005 DONE, FIX-057~062 신규 등록 |

---

## BAL-005 — 여행 상인 희귀 아이템 ROI 분석

### 분석 대상 8종

| 아이템 | 기존 가격 | 판정 | 권장 가격 |
|--------|----------|------|----------|
| 만능 비료 | 150G | 손해 (모든 작물에서 ROI 적자) | **80G** |
| 비계절 씨앗 | 정가 x2.0 | 손해 (시간 이점 미미) | **정가 x1.5** |
| 에너지 토닉 | 200G | 적정 (보험 아이템, ROI 계산 외) | **200G 유지** |
| 성장 촉진제 | 250G | 손해 (추가 회전 달성 불확실) | **150G + 2일 단축으로 효과 상향** |
| 행운의 부적 | 400G | 손해 (Iridium +5% 가치 부족) | **250G + Iridium +15%로 효과 상향** |
| 정원 등불 | 300G | 적정 (장식) | **300G 유지** |
| 풍향계 | 500G | 적정 (장기 기상 예보, 흑자) | **500G 유지** |
| 겨울 전용 씨앗 | 정가 x1.5 | 적정 (온실 수익 대비 합리적) | **정가 x1.5 유지** |

### 핵심 발견

1. **소비형 버프 아이템 3종 일괄 가성비 부족**: 만능 비료, 성장 촉진제, 행운의 부적이 모두 "비싸지만 가치 있는 선택"이라는 설계 목표를 미달. 가격 인하 또는 효과 상향이 필요.
2. **장식/유틸리티 아이템은 현행 유지**: 정원 등불·풍향계는 ROI 아이템이 아닌 라이프스타일 아이템으로 적정 범위.
3. **겨울 씨앗은 유지**: 온실 없이 겨울 재배 대안으로서 정가 x1.5 적합.

### [OPEN] 이슈 해소 제안

| 이슈 | 해소 방향 |
|------|-----------|
| 겨울 씨앗 판매 경로 독점 여부 | 여행 상인 독점 유지 + 잡화 상점 겨울 Day 8부터 정가 병행 판매 권장 |
| 에너지 토닉 밸런스 | 200G 유지, 에너지 시스템 우회 적절 수준 |
| 구름이(나귀) 당근 인터랙션 경로 확정 | 도입 권장 — 낮은 복잡도로 친밀도 보완 경로 제공 |
| 연말 봄 씨앗 세트 가격 | 정가 x1.5 유지 |

---

## 아키텍처 보강 — npc-shop-architecture.md 섹션 9

### 친밀도 파라미터 추가 (섹션 9.1)

TravelingMerchantData SO에 4개 친밀도 관련 필드 추가:
- `affinityThresholds` float[] — Regular/Friend 단계 전환 임계값
- `regularBonusItemCount` int — Regular 달성 시 공개 추가 아이템 수
- `friendDiscountRate` float — Friend 달성 시 전 아이템 할인율
- `friendBonusStockPerItem` int — Friend 달성 시 재고 보너스

### TravelingMerchantSaveData 확장 (섹션 9.4)

기존 6필드에 `affinityPoints` 추가 → 7필드. JSON/C# 동기화 완료 (PATTERN-005 준수).

### 가격 반영 경로 명시 (섹션 9.5 신규)

BAL-005 확정 가격 → TravelingMerchantData SO `basePrice` 필드 → `TravelingMerchantManager.LoadStock()` → 상점 UI의 명확한 데이터 흐름 기술.

---

## 리뷰어 수정 사항

| ID | 심각도 | 파일 | 수정 내용 |
|----|--------|------|-----------|
| CRITICAL-1 | 🔴 | npc-shop-architecture.md 섹션 7.1 | "6필드" → "7필드" 수정 (섹션 9.4 확장과 불일치 해소) |
| WARNING-1 | 🟡 | npcs.md 섹션 9.2~9.3 | BAL-005 권장 조정 미반영 → FIX-057/058 등록 |
| WARNING-2 | 🟡 | traveler-npc.md 섹션 3.6 | 성장 촉진제 대사 FIX-058 완료 시 동시 수정 예고 |
| WARNING-3 | 🟡 | npcs.md 섹션 9.3 | luckyCharmIridiumBonus 범위 0.10 → 0.20 확장 필요 → FIX-057 등록 |
| INFO-1 | ℹ️ | npc-shop-architecture.md Cross-references | traveler-npc.md, traveler-economy.md 추가 → 직접 수정 완료 |

---

## 세션 후 TODO 상태

| ID | Priority | 상태 |
|----|----------|------|
| BAL-005 | 1 | ✅ DONE |
| AUD-001 | 1 | 잔여 |
| BAL-009 | 1 | 잔여 |
| CON-009 | 2 | 잔여 |
| FIX-049~054 | 2~3 | 잔여 (낚시 연동 FIX 묶음) |
| FIX-056 | 3 | 잔여 |
| FIX-057~062 | 2~3 | 잔여 (BAL-005 후속 FIX 묶음) |
| PATTERN-009, 010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 BAL-005 태스크에 따라 자율적으로 작성했습니다.*
