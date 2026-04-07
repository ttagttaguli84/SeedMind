# Devlog #065 — CON-010: 낚시 업적/퀘스트 콘텐츠 추가

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

FIX-071(겨울 낚시 허용) 완료 후 블로커가 해소된 CON-010을 처리했다. `achievements.md`에 낚시사(Angler) 카테고리 업적 4종, `quest-system.md`에 낚시 퀘스트 9종을 추가하고, downstream 문서(xp-integration.md, progression-curve.md) 동기화를 완료했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/content/achievements.md` | 섹션 9 낚시사 카테고리 신설, 업적 4종 추가 (총 30→34종, XP 2,250→2,640) |
| `docs/systems/quest-system.md` | NPC 의뢰 1종, 일일 퀘스트 2종, 농장 도전 6종 추가; XP ~900→~1,010 갱신 |
| `docs/systems/fishing-system.md` | 섹션 8.1 임시 업적 ID → achievements.md 섹션 9 참조 표기로 교체 |
| `docs/balance/xp-integration.md` | 업적 종수 30→34, XP 2,250→2,640, 퀘스트 XP ~900→~1,010 전면 갱신 |
| `docs/balance/progression-curve.md` | 섹션 1.2, 2.4.4 수치 갱신 |

---

## CON-010 — 낚시 업적 4종

### achievements.md 섹션 9 신설 (낚시사 카테고리)

| ID | 이름 | 조건 | 보상 |
|----|------|------|------|
| `ach_fish_01` | 첫 낚시 | 물고기 1마리 낚기 | 50G + 20 XP + 칭호 |
| `ach_fish_02` | 낚시 애호가 | 누적 50마리 (3단계: 10/50/200) | 200G + 50 XP + 미끼통 |
| `ach_fish_03` | 낚시꾼 | 200마리 누적 | 500G + 100 XP + 숙련도 XP 보너스 |
| `ach_fish_04` | 전설의 낚시사 | 도감 15/15종 완성 | 1,000G + 100 XP + 칭호 + 황금 낚싯대 장식품 |

conditionType `FishCaughtCount`, `FishSpeciesCollected` → ARC-030에서 enum 추가 예정 ([TODO]).

### quest-system.md 낚시 퀘스트 추가

| 유형 | ID | 내용 | 보상 |
|------|-----|------|------|
| NPC 의뢰 (하나) | `npc_hana_04` | 생선 5마리 납품 | 150G + 7 XP |
| 일일 목표 | `daily_fish_3` | 물고기 3마리 낚기 | 60G + 2 XP |
| 일일 목표 | `daily_fish_species` | 서로 다른 어종 2종 낚기 | 80G + 2 XP |
| 농장 도전 | `fc_first_fish` ~ `fc_summer_fish` | 6종 낚시 도전 | 합계 ~600G + ~50 XP |

---

## XP 통합 재조정

| 항목 | 변경 전 | 변경 후 |
|------|---------|---------|
| 업적 총 XP | 2,250 | 2,640 (+390) |
| 퀘스트 총 XP | ~900 | ~1,010 (+110) |
| 1년차 업적 XP (일반) | ~500 | ~540 |
| 업적 비중 (전체 대비) | 24.9% | 29.2% |

---

## 리뷰어 검증 결과

| ID | 심각도 | 이슈 내용 | 수정 결과 |
|----|--------|-----------|-----------|
| R-01 | 🔴 ERROR | fishing-system.md 섹션 8.1 임시 업적 ID(ach_first_catch 등 8종) — 확정 ID(ach_fish_01~04)와 불일치 | achievements.md 참조 표기로 교체 완료 |
| R-02 | 🔴 ERROR | xp-integration.md 업적 종수/XP 미갱신 | 34종, 2,640 XP로 전면 갱신 |
| R-03 | 🔴 ERROR | progression-curve.md 수치 미갱신 | 업적/퀘스트 XP 갱신 완료 |
| R-04 | 🟡 WARNING | quest-system.md 섹션 7.3 XP 비율 "10%(~900 XP)" 미갱신 | "11.2%(~1,010 XP)"로 갱신 |

---

## 세션 후 활성 TODO

| ID | Priority | 상태 |
|----|----------|------|
| CON-009 | 2 | 잔여 (치즈 공방 레시피 — BAL-008 선행) |
| DES-014 | 2 | 잔여 (겨울 씨앗 판매 경로) |
| BAL-008 | 1 | 잔여 (목축/낙농 경제 밸런스) |
| BAL-014 | 1 | 잔여 (낚시 숙련도 XP 밸런스) |
| DES-015 | 1 | 잔여 (낚싯대 업그레이드 재료 공급 경로) |
| CON-011 | 1 | 잔여 (낚시 도감 콘텐츠) |
| FIX-072 | 1 | 잔여 (economy-system.md 낚시 수입 항목) |
| ARC-030 | 1 | 잔여 (낚시 도감 아키텍처 + conditionType enum 추가) |
| DES-016 | 1 | 잔여 (채집 시스템 기본 설계) |
| PATTERN-009/010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 CON-010 태스크에 따라 자율적으로 작성했습니다.*
