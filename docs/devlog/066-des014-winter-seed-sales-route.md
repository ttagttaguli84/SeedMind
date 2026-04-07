# Devlog #066 — DES-014: 겨울 온실 전용 씨앗 판매 경로 확정

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

CON-010 완료 후 최고 우선순위 비블로킹 항목인 DES-014를 처리했다. 겨울 온실 전용 씨앗(겨울무/표고버섯/시금치)의 판매 경로를 3가지 옵션 분석 후 **옵션 C (혼합)** 로 확정했다. 리뷰 과정에서 `economy-system.md`의 "겨울 씨앗 판매 없음" 기재 등 CRITICAL 충돌 3건을 포함해 총 6건의 이슈를 발견·수정했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/content/npcs.md` | [OPEN] 1 → RESOLVED; 계절별 씨앗 판매 테이블 겨울 행 추가; 겨울 Day 1~7/Day 8~28 대사 3분기 분리 |
| `docs/content/crops.md` | 섹션 4.4 씨앗 구매 경로(DES-014 확정) 테이블 신설 |
| `docs/balance/traveler-economy.md` | 섹션 3.8 여행 상인 ROI 테이블 canonical 주석 추가; 섹션 6.1 제안 → RESOLVED |
| `docs/systems/npc-shop-architecture.md` | 섹션 15 "계절별 재고 필터링 시스템" 신설 (7개 하위 섹션); 시나리오 C SeasonFlag.All→Winter 수정; [OPEN] 10 → RESOLVED |
| `docs/systems/economy-system.md` | 섹션 3.3 겨울 씨앗 판매 테이블 DES-014 반영 (Day 1~7/Day 8~28 분리) |
| `docs/content/facilities.md` | 섹션 4.4 온실: 겨울 씨앗 구매 경로 안내 1줄 추가 |

---

## DES-014 결정 — 옵션 C (혼합) 확정

### 판매 경로 최종 구조

| 판매처 | 시기 | 가격 | 재고 | 조건 |
|--------|------|------|------|------|
| 여행 상인 (바람이) | 겨울 Day 1~ | 정가 ×1.5 | 1~3개 | 등장 확률 의존 |
| 잡화 상점 (하나) | 겨울 Day 8~ | 정가 | 무제한 | 온실 보유 |

### 선택 근거

- **플레이어 경험**: 여행 상인 독점(옵션 A)은 "등장 확률 + 아이템 선정 확률"의 2중 RNG로 겨울 콘텐츠 진입이 과도하게 불확실함. Day 8 잡화 상점 병행으로 확정적 구매 경로 보장
- **경제 밸런스**: 여행 상인 ×1.5 프리미엄은 총 수입 대비 약 7%로 적정. "빨리 시작+비싸게" vs "늦게 시작+싸게" 의미 있는 트레이드오프 형성
- **계절 리듬**: 겨울 1주차 = 준비/계획 기간, Day 8~ = 본격 재배. 자연스러운 리듬

### 아키텍처 핵심

기존 `ShopItemEntry.availableSeasons` (SeasonFlag 비트마스크) 필드가 이미 존재 → **데이터 스키마 변경 없이 SO 에셋 설정만으로 구현 가능**

---

## 리뷰어 검증 결과

| ID | 심각도 | 이슈 내용 | 수정 결과 |
|----|--------|-----------|-----------|
| R-01 | 🔴 CRITICAL | economy-system.md 섹션 3.3 "겨울에는 씨앗 판매 없음" — DES-014 혼합 모델과 완전 충돌 | Day 1~7/Day 8~28 분리로 교체 완료 |
| R-02 | 🔴 CRITICAL | npc-shop-architecture.md 섹션 15.4 시나리오 C `SeasonFlag.All` — 여행 상인이 비겨울에도 겨울 씨앗 판매로 설계되어 npcs.md와 모순 | `SeasonFlag.Winter`로 수정 완료 |
| R-03 | 🔴 CRITICAL | npc-shop-architecture.md [OPEN] 10 DES-014 미결 상태 잔존 | [RESOLVED]로 전환 완료 |
| R-04 | 🟡 WARNING | traveler-economy.md 섹션 3.8 씨앗 정가 수치 canonical 주석 누락 | `// → copied from docs/content/crops.md` 주석 추가 완료 |
| R-05 | 🔵 INFO | facilities.md 온실 섹션에 씨앗 구매 경로 안내 누락 | crops.md 섹션 4.4 참조 안내 추가 완료 |
| R-06 | 🔵 INFO | TODO.md DES-014 완료 처리 누락 | 취소선 및 확정 내용 기재 완료 |

---

## 세션 후 활성 TODO

| ID | Priority | 상태 |
|----|----------|------|
| CON-009 | 2 | 잔여 (치즈 공방 레시피 — BAL-008 선행) |
| BAL-008 | 1 | 잔여 (목축/낙농 경제 밸런스) |
| BAL-014 | 1 | 잔여 (낚시 숙련도 XP 밸런스) |
| DES-015 | 1 | 잔여 (낚싯대 업그레이드 재료 공급 경로) |
| CON-011 | 1 | 잔여 (낚시 도감 콘텐츠) |
| FIX-072 | 1 | 잔여 (economy-system.md 낚시 수입 항목) |
| ARC-030 | 1 | 잔여 (낚시 도감 아키텍처) |
| DES-016 | 1 | 잔여 (채집 시스템 기본 설계) |
| PATTERN-009/010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 DES-014 태스크에 따라 자율적으로 작성했습니다.*
