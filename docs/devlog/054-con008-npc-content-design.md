# Devlog #054 — CON-008: 추가 NPC 상세 설계 완료

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

CON-008 — blacksmith NPC 외 추가 NPC 3인의 전체 콘텐츠를 상세 설계하고, npc-shop-architecture.md에 여행 상인 스케줄·힌트 시스템·운영 시간 스케줄 아키텍처를 확장했다. Reviewer가 CRITICAL 2건, WARNING 3건을 발견하여 수정 완료했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/content/merchant-npc.md` | CON-008a: 시장 상인 "하나" 전체 상세 설계 (신규) |
| `docs/content/carpenter-npc.md` | CON-008b: 목공소 장인 "목이" 전체 상세 설계 (신규) |
| `docs/content/traveler-npc.md` | CON-008c: 여행 상인 "바람이" 전체 상세 설계 (신규) |
| `docs/content/npcs.md` | 신규 파일 Cross-references 추가, 바람이 첫 등장 대사 canonical 동기화 |
| `docs/systems/npc-shop-architecture.md` | 섹션 9~11: 여행 상인 시스템/NPCHintSystem/운영 시간 스케줄 신규 추가 |
| `docs/architecture.md` | Cross-references 업데이트 |
| `TODO.md` | CON-008 DONE 처리 |

---

## CON-008 — NPC 콘텐츠 상세 설계

### 설계된 NPC 3인

#### 1. 시장 상인 "하나" (`merchant-npc.md`)

| 항목 | 내용 |
|------|------|
| 역할 | 씨앗·비료·일용품 판매, 작물 수매 |
| 친밀도 임계값 | Stranger/Acquaintance/Regular/Friend = [0, 10, 25, 50] |
| Friend 보상 | 씨앗 구매 10% 할인 |
| 대화 | 범용 5종 + 계절별 12종 + 구매/판매 10종 + 특수 8종 + 힌트 6종 + 친밀도별 14종 + 날씨별 5종 |
| 특수 이벤트 | 겨울 특별 대사 시리즈, 계절 추천 작물 배지 |

#### 2. 목공소 장인 "목이" (`carpenter-npc.md`)

| 항목 | 내용 |
|------|------|
| 역할 | 시설 건설·업그레이드 의뢰, 건설 진행률 관리 |
| 친밀도 임계값 | [0, 10, 25, 50] (건설 완료 +5, 확장 +3) |
| Friend 보상 | 건설 비용 10% 할인 |
| 대화 | 범용 5종 + 계절별 12종 + 건설 대사 12종 + 특수 7종 + 힌트 5종 + 친밀도별 14종 |
| 특수 규칙 | 겨울 야외 건설 +1일 지연, 농장 확장 불가 |

#### 3. 여행 상인 "바람이" (`traveler-npc.md`)

| 항목 | 내용 |
|------|------|
| 역할 | 희귀 아이템 판매 (만능 비료, 겨울 씨앗 등), 계절별 재고 |
| 방문 주기 | 매주 토·일 100% 고정 등장 (→ see npcs.md 섹션 6.2) |
| 친밀도 임계값 | [0, 8, 20, 40] (주말 등장 특성상 다른 NPC보다 낮게 설정) |
| Regular 보상 | 아이템 풀 +1개 공개 |
| Friend 보상 | 전 아이템 5% 할인 + 재고 +1 |
| 대화 | 범용 5종 + 계절별 12종 + 구매 7종 + 특수 7종 + 아이템 추천 6종 + 친밀도별 9종 + 퇴장 4종 + 여행 일지 22종 |
| 특수 이벤트 | 나귀 "구름이" 당근 인터랙션, 연말 봄 씨앗 세트 판매 |

---

## 아키텍처 확장 — npc-shop-architecture.md 섹션 9~11

### 섹션 9: 여행 상인 시스템

- **TravelingMerchantData SO**: 방문 스케줄, 계절별 확률, 가격 파라미터 별도 SO로 분리
- **시드 기반 결정론적 재고**: `stockSeedBase ^ year ^ season ^ day` 조합 — 동일 게임 상태에서 재현 가능
- **TravelingMerchantSaveData 6필드**: 기존 4필드에 `currentStockPrices`(가격 변동 보존)·`recentItemIds`(2주 쿨다운) 추가

### 섹션 10: NPCHintSystem (농업 전문가)

- **HintConditionType enum 17종**: 작물/시설/경제/계절/진행도 5개 카테고리
- **ContextHintSystem과 역할 분리**: 자동 팝업(Context) vs 능동 방문(NPC) 명확 구분
- **레벨 기반 해금**: `requiredPlayerLevel` → progression-curve.md와 연동

### 섹션 11: 운영 시간 스케줄 시스템

- **OperatingSchedule 구조체**: `openHour/closeHour/closedDays/immuneToWeather/specialOpenDays`
- **OperatingScheduleEvaluator**: 영업 상태 판정 순수 함수 유틸리티 (테스트 용이)
- **이벤트 기반**: `TimeManager.OnHourChanged` + `WeatherSystem.OnWeatherChanged` 구독

---

## 리뷰어 수정 사항

| ID | 심각도 | 파일 | 수정 내용 |
|----|--------|------|-----------|
| CRITICAL-1 | 🔴 | npc-shop-architecture.md 섹션 9.1/9.2 | 계절별 방문 확률 필드 — canonical(100% 고정)과 충돌. "미래 확장용" + "canonical 1.0f이면 항상 true" 주석 추가 |
| CRITICAL-2 | 🔴 | carpenter-npc.md 섹션 3.2 | 오탈자: "하나" → "목이" |
| WARNING-1 | 🟡 | npc-shop-architecture.md 섹션 2.1 | NPCData 주석에 VillageMerchant=4, AgricultureExpert=5 추가 |
| WARNING-2 | 🟡 | npcs.md 섹션 6.5 | 바람이 첫 등장 대사 3문장→4문장 canonical 동기화 |
| WARNING-3 | 🟡 | traveler-npc.md 섹션 6 | 친밀도 임계값 직접 기재 → 섹션 2.5 참조로 교체 |

---

## 세션 후 TODO 상태

| ID | Priority | 상태 |
|----|----------|------|
| CON-008 | 1 | ✅ DONE |
| AUD-001 | 1 | 잔여 |
| BAL-005 | 1 | 잔여 |
| BAL-009 | 1 | 잔여 |
| CON-009 | 2 | 잔여 |
| FIX-049~054 | 2~3 | 잔여 (낚시 연동 FIX 묶음) |
| FIX-056 | 3 | 잔여 |
| PATTERN-009, 010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 CON-008 태스크에 따라 자율적으로 작성했습니다.*
