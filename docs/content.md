# SeedMind — Content

SeedMind 프로젝트의 전체 콘텐츠 스펙을 통합한 아카이브입니다. 작물, 시설, NPC, 아이템, 가공 시스템 등 모든 콘텐츠 정의를 포함합니다.

---

# 업적 콘텐츠 상세 (Achievement Content Specification)

> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: CON-007

---

## 1. Context

이 문서는 SeedMind의 전체 업적 40종에 대한 콘텐츠 상세 정보를 기술한다. 각 업적의 고유 ID, 이름, 조건 타입, 목표 수치, 보상(골드/XP/칭호/아이템)을 확정 테이블로 정리하며, 칭호 50종의 canonical 목록과 보상 기준 테이블을 포함한다.

### 1.1 본 문서가 canonical인 데이터

- 업적 40종 전체 목록의 확정 보상 수치 (골드, XP, 아이템)
- 단계형 업적 Bronze/Silver/Gold 각 단계별 목표 수치 및 보상
- 칭호 43종 canonical 매핑 테이블 (칭호 ID, 이름, 해금 조건)
- 보상 기준 테이블 (난이도/단계별 골드/XP 범위)
- 아이템 보상 전체 목록 및 효과

### 1.2 본 문서가 canonical이 아닌 데이터 (참조만)

| 데이터 종류 | 참조처 |
|------------|--------|
| 업적 카테고리 정의, 분류 체계, UI/UX, 추적 이벤트 | `docs/systems/achievement-system.md` |
| AchievementData SO 필드, AchievementConditionType enum | `docs/systems/achievement-architecture.md` 섹션 2.3 |
| XP 소스, 레벨별 필요 XP, 해금 테이블 | `docs/balance/progression-curve.md` |
| 골드 초기 지급, 가격 시스템 | `docs/systems/economy-system.md` |
| 작물 이름, 씨앗 가격, 판매가, 성장일수 | `docs/design.md` 섹션 4.2 |
| 작물 ID, 계절별 분류, 겨울 전용 작물 | `docs/content/crops.md` |
| 시설 이름, 비용, 해금 레벨 | `docs/design.md` 섹션 4.6 |
| 시설 ID, 점유 타일, 건설 시간 | `docs/content/facilities.md` |
| NPC 이름, 역할 | `docs/content/npcs.md` 섹션 2 |
| 도구 업그레이드 등급, 비용 | `docs/systems/tool-upgrade.md` 섹션 1~2 |
| 품질 등급 정의 | `docs/systems/crop-growth.md` 섹션 4.3~4.4 |
| 가공품 레시피 | `docs/content/processing-system.md` |
| 퀘스트 분류, 농장 도전 목록 | `docs/systems/quest-system.md` 섹션 1, 6 |

---

## 2. 보상 기준 테이블

업적 보상은 난이도와 단계에 따라 차등 지급한다. 아래는 보상 결정의 기준이 되는 범위 테이블이다.

### 2.1 단일 업적 보상 범위

| 난이도 | 골드 | XP | 적용 기준 |
|--------|------|-----|-----------|
| 쉬움 (초반 달성) | 50G | 20 XP | 첫 수확, 첫 건축, 첫 판매 등 |
| 보통 (중반 달성) | 150G | 50 XP | NPC 전원 만남, 사계절 경험, 기본 시설 완성 등 |
| 어려움 (후반 달성) | 300G | 100 XP | 전설 도구 세트, 전 작물 수확, 모든 가공소 건설 등 |

### 2.2 단계형 업적 보상 범위

| 단계 | 골드 | XP | 적용 기준 |
|------|------|-----|-----------|
| Bronze | 50G | 20 XP | 초반~중반에 자연스럽게 달성 |
| Silver | 150G | 50 XP | 중반~후반에 달성 |
| Gold | 300G | 100 XP | 후반 장기 플레이로 달성 |

### 2.3 숨겨진 업적 보상

숨겨진 업적은 난이도와 무관하게 **균일 보상**: 100G + 30 XP. 숨겨진 업적의 핵심 가치는 칭호와 발견의 즐거움에 있다.

### 2.4 XP 보상 총량 추정

- 전 업적(40종) 완료 시 총 XP: **3,160 XP** (아래 섹션 3~10 및 섹션 13.1 확정 합산, 채집가 5종 490 XP + 통합 수집 마스터 1종 30 XP 포함)
- 전체 필요 누적 XP **9,029**의 약 **35.0%**에 해당 (-> see `docs/balance/progression-curve.md` 섹션 2.4.1 — canonical XP 테이블)
- 전 업적 완료는 2년차 이후에나 가능하므로 실질적 XP 가속 효과는 제한적

[NOTE -- CON-013/CON-017/BAL-019 갱신] 업적 XP 비중(35.0%)은 canonical 기준(9,029 XP) 대비 목표 범위(**30~40%**, BAL-019 조정) 내이다. BAL-007 통합 시뮬레이션(-> see `docs/balance/xp-integration.md`)에서 퀘스트 ~1,115 XP + 업적 3,160 XP 포함 시 1년차 일반 플레이어 레벨 8~9 도달이 예상된다. 낚시 업적(390 XP)과 채집 업적(490 XP)은 각각 Zone F/Zone D 해금(레벨 5) 이후 점진적으로 달성되므로 1년차 내 전부 달성은 어렵다. 통합 수집 마스터(`ach_hidden_07`, 30 XP)는 두 도감 모두 완성해야 하는 극후반 업적이므로 XP 영향 무시 가능. (-> see `docs/balance/bal-019-xp-balance.md`)

### 2.5 골드 보상 총량 추정

- 전 업적 완료 시 총 골드: 약 11,050G (아래 섹션 3~10의 합산, 채집가 2,600G + 통합 수집 마스터 100G 포함)
- 게임 전체 플레이(2년+) 누적 수입 대비 미미한 수준 (-> see `docs/balance/progression-curve.md` 섹션 3)

---

## 3. 농업 마스터 (Farming) -- 5개

### 3.1 업적 목록

| # | achievementId | 이름 | 유형 | conditionType | 조건 상세 | 숨김 |
|---|---------------|------|------|---------------|----------|------|
| 1 | `ach_farming_01` | 씨앗의 시작 | Single | `HarvestCount` (0) | 첫 번째 작물 수확 (conditionValue = 1) | N |
| 2 | `ach_farming_02` | 수확의 대가 | Tiered | `HarvestCount` (0) | 누적 작물 수확 (3단계) | N |
| 3 | `ach_farming_03` | 사계절 농부 | Single | `SeasonCompleted` (9) | 4계절에 각 1종 이상 작물 수확 (겨울은 온실 포함). conditionValue = 4 | N |
| 4 | `ach_farming_04` | 작물 도감 완성 | Single | `SpecificCropHarvested` (6) | 게임 내 모든 작물 종류를 1개 이상 수확. 전체 작물 수 = 11종 (-> see `docs/content/crops.md` 섹션 2). conditionValue = 11 | N |
| 5 | `ach_farming_05` | 품질의 끝 | Single | `QualityHarvestCount` (12) | Iridium 품질 작물 1개 수확. conditionValue = 1 (-> see `docs/systems/crop-growth.md` 섹션 4.3) | N |

### 3.2 ach_farming_02 단계 상세 (수확의 대가)

| 단계 | conditionValue | 골드 | XP | 칭호 | 아이템 |
|------|---------------|------|-----|------|--------|
| Bronze | 50 | 50G | 20 XP | - | - |
| Silver | 200 | 150G | 50 XP | 숙련 농부 (`title_skilled_farmer`) | - |
| Gold | 1,000 | 300G | 100 XP | 수확의 대가 (`title_farming_gold`) | 속성장 비료 x10 |

### 3.3 단일 업적 보상 상세

| achievementId | 골드 | XP | 칭호 | 아이템 |
|---------------|------|-----|------|--------|
| `ach_farming_01` | 50G | 20 XP | 새싹 농부 (`title_sprout_farmer`) | - |
| `ach_farming_03` | 150G | 50 XP | 사계절 농부 (`title_four_seasons`) | - |
| `ach_farming_04` | 300G | 100 XP | 작물 박사 (`title_crop_doctor`) | 황금 씨앗 x1 |
| `ach_farming_05` | 300G | 100 XP | 전설의 경작자 (`title_legendary_farmer`) | - |

**Farming 카테고리 소계**: 골드 1,300G / XP 510 (-> see `docs/balance/progression-curve.md` for XP 총량 맥락)

---

## 4. 경제 달인 (Economy) -- 4개

### 4.1 업적 목록

| # | achievementId | 이름 | 유형 | conditionType | 조건 상세 | 숨김 |
|---|---------------|------|------|---------------|----------|------|
| 1 | `ach_economy_01` | 첫 수익 | Single | `TotalItemsSold` (11) | 첫 번째 출하/판매 수행. conditionValue = 1 | N |
| 2 | `ach_economy_02` | 부의 축적 | Tiered | `GoldEarned` (1) | 누적 판매 수익 (3단계) | N |
| 3 | `ach_economy_03` | 대박 거래 | Single | `GoldEarned` (1) | 단일 출하에서 1,000G 이상 수익 달성. Custom 이벤트: `OnSingleShipment` totalGold >= 1,000 | N |
| 4 | `ach_economy_04` | 가공의 연금술 | Single | `GoldEarned` (1) | 가공품 판매로 누적 5,000G 수익 달성. Custom 추적: isProcessed 필터 적용 | N |

### 4.2 ach_economy_02 단계 상세 (부의 축적)

| 단계 | conditionValue | 골드 | XP | 칭호 | 아이템 |
|------|---------------|------|-----|------|--------|
| Bronze | 5,000G | 50G | 20 XP | - | - |
| Silver | 25,000G | 150G | 50 XP | 성공한 농부 (`title_successful_farmer`) | - |
| Gold | 100,000G | 300G | 100 XP | 부의 축적자 (`title_wealth_accumulator`) | 금고 장식품 x1 |

[OPEN] Gold 단계(100,000G)가 일반 플레이어의 2년차 누적 수익 범위 내에 있는지 검증 필요. (-> see `docs/balance/progression-curve.md` 섹션 3)

### 4.3 단일 업적 보상 상세

| achievementId | 골드 | XP | 칭호 | 아이템 |
|---------------|------|-----|------|--------|
| `ach_economy_01` | 50G | 20 XP | 초보 상인 (`title_novice_merchant`) | - |
| `ach_economy_03` | 150G | 50 XP | 거래의 귀재 (`title_trade_genius`) | - |
| `ach_economy_04` | 150G | 50 XP | 가공 마스터 (`title_processing_master`) | - |

**Economy 카테고리 소계**: 골드 850G / XP 340

---

## 5. 시설 개척자 (Facility) -- 4개

### 5.1 업적 목록

| # | achievementId | 이름 | 유형 | conditionType | 조건 상세 | 숨김 |
|---|---------------|------|------|---------------|----------|------|
| 1 | `ach_facility_01` | 첫 건축 | Single | `BuildingCount` (2) | 첫 번째 시설 건설. conditionValue = 1 | N |
| 2 | `ach_facility_02` | 시설 왕국 | Single | `SpecificBuildingBuilt` (10) | 기본 4종 시설(물탱크, 창고, 온실, 가공소) 모두 건설 (-> see `docs/design.md` 섹션 4.6). conditionValue = 4 | N |
| 3 | `ach_facility_03` | 가공 제국 | Single | `SpecificBuildingBuilt` (10) | 4종 가공소(가공소, 제분소, 발효실, 베이커리) 모두 건설 (-> see `docs/design.md` 섹션 4.6, `docs/content/facilities.md` 섹션 2.1). conditionValue = 4 | N |
| 4 | `ach_facility_04` | 가공의 달인 | Tiered | `ProcessingCount` (13) | 누적 가공품 제작 (3단계) | N |

### 5.2 ach_facility_04 단계 상세 (가공의 달인)

| 단계 | conditionValue | 골드 | XP | 칭호 | 아이템 |
|------|---------------|------|-----|------|--------|
| Bronze | 20 | 50G | 20 XP | - | - |
| Silver | 100 | 150G | 50 XP | 가공 장인 (`title_processing_artisan`) | - |
| Gold | 300 | 300G | 100 XP | 가공의 달인 (`title_processing_master_gold`) | 특수 레시피 (-> see [OPEN] 아래) |

[OPEN] ach_facility_04 Gold 단계의 레시피 보상으로 업적 전용 특수 레시피를 제공할지, 기존 레시피 중 하나를 조기 해금할지 결정 필요. (-> see `docs/content/processing-system.md`)

### 5.3 단일 업적 보상 상세

| achievementId | 골드 | XP | 칭호 | 아이템 |
|---------------|------|-----|------|--------|
| `ach_facility_01` | 50G | 20 XP | 건축 입문자 (`title_builder_novice`) | - |
| `ach_facility_02` | 150G | 50 XP | 시설 왕 (`title_facility_king`) | - |
| `ach_facility_03` | 300G | 100 XP | 가공 제국의 주인 (`title_processing_empire`) | 고급 연료 x20 |

**Facility 카테고리 소계**: 골드 1,000G / XP 340

---

## 6. 도구 장인 (Tool) -- 3개

### 6.1 업적 목록

| # | achievementId | 이름 | 유형 | conditionType | 조건 상세 | 숨김 |
|---|---------------|------|------|---------------|----------|------|
| 1 | `ach_tool_01` | 첫 강화 | Single | `ToolUpgradeCount` (3) | 도구 1개를 Reinforced로 업그레이드 (-> see `docs/systems/tool-upgrade.md` 섹션 1.1). conditionValue = 1 | N |
| 2 | `ach_tool_02` | 완벽한 도구 세트 | Single | `ToolUpgradeCount` (3) | 3종 도구(호미, 물뿌리개, 낫) 모두 Reinforced 이상 달성. conditionValue = 3 | N |
| 3 | `ach_tool_03` | 전설의 장비 | Single | `ToolUpgradeCount` (3) | 3종 도구 모두 Legendary 달성 (-> see `docs/systems/tool-upgrade.md` 섹션 1.1). conditionValue = 6 (3도구 x 2등급) | N |

### 6.2 단일 업적 보상 상세

| achievementId | 골드 | XP | 칭호 | 아이템 |
|---------------|------|-----|------|--------|
| `ach_tool_01` | 50G | 20 XP | 장인 견습생 (`title_apprentice_smith`) | - |
| `ach_tool_02` | 150G | 50 XP | 도구의 달인 (`title_tool_master`) | - |
| `ach_tool_03` | 300G | 100 XP | 전설의 장인 (`title_legendary_smith`) | 장인의 망치 장식품 x1 |

**Tool 카테고리 소계**: 골드 500G / XP 170

---

## 7. 탐험가 (Explorer) -- 4개

### 7.1 업적 목록

| # | achievementId | 이름 | 유형 | conditionType | 조건 상세 | 숨김 |
|---|---------------|------|------|---------------|----------|------|
| 1 | `ach_explorer_01` | 마을 인사 | Single | `NPCMet` (4) | 4명의 NPC(하나, 철수, 목이, 바람이) 모두와 첫 대화 완료 (-> see `docs/content/npcs.md` 섹션 2.1). conditionValue = 4 | N |
| 2 | `ach_explorer_02` | 바람이의 단골 | Single | `PurchaseCount` (14) | 바람이(여행 상인)에게서 누적 5회 이상 물건 구매. targetId = "merchant_baramyi" 필터. conditionValue = 5 | N |
| 3 | `ach_explorer_03` | 사계절의 기억 | Single | `SeasonCompleted` (9) | 봄/여름/가을/겨울 4계절 모두 경험 (1년 완주). conditionValue = 4 | N |
| 4 | `ach_explorer_04` | 쇼핑 마니아 | Tiered | `PurchaseCount` (14) | 상점에서 누적 물건 구매 횟수 (3단계). 이벤트: `EconomyEvents.OnShopPurchased` | N |

### 7.2 ach_explorer_04 단계 상세 (쇼핑 마니아)

| 단계 | conditionValue | 골드 | XP | 칭호 | 아이템 |
|------|---------------|------|-----|------|--------|
| Bronze | 30 | 50G | 20 XP | - | - |
| Silver | 100 | 150G | 50 XP | 쇼핑 애호가 (`title_shopping_lover`) | - |
| Gold | 300 | 300G | 100 XP | 쇼핑 마니아 (`title_shopping_maniac`) | 상인의 뱃지 장식품 x1 |

### 7.3 단일 업적 보상 상세

| achievementId | 골드 | XP | 칭호 | 아이템 |
|---------------|------|-----|------|--------|
| `ach_explorer_01` | 150G | 50 XP | 사교적인 농부 (`title_social_farmer`) | - |
| `ach_explorer_02` | 150G | 50 XP | 여행자의 친구 (`title_traveler_friend`) | 바람이 특별 할인권 x1 |
| `ach_explorer_03` | 150G | 50 XP | 계절의 증인 (`title_season_witness`) | - |

**Explorer 카테고리 소계**: 골드 950G / XP 370

---

## 8. 퀘스트 영웅 (Quest) -- 4개

### 8.1 업적 목록

| # | achievementId | 이름 | 유형 | conditionType | 조건 상세 | 숨김 |
|---|---------------|------|------|---------------|----------|------|
| 1 | `ach_quest_01` | 첫 임무 완수 | Single | `QuestCompleted` (5) | 첫 번째 퀘스트(카테고리 무관) 완료. conditionValue = 1 | N |
| 2 | `ach_quest_02` | 퀘스트 수집가 | Tiered | `QuestCompleted` (5) | 누적 퀘스트 완료 수 (모든 카테고리 합산, 3단계) | N |
| 3 | `ach_quest_03` | NPC의 신뢰 | Single | `QuestCompleted` (5) | 모든 NPC(하나, 철수, 목이, 바람이)의 의뢰를 각 1개 이상 완료. conditionValue = 4 (NPC 수) | N |
| 4 | `ach_quest_04` | 꾸준한 일꾼 | Single | `Custom` (99) | 일일 목표를 연속 7일 완료 (2개 중 1개 이상 완료 기준). conditionValue = 7 | N |

### 8.2 ach_quest_02 단계 상세 (퀘스트 수집가)

| 단계 | conditionValue | 골드 | XP | 칭호 | 아이템 |
|------|---------------|------|-----|------|--------|
| Bronze | 10 | 50G | 20 XP | - | - |
| Silver | 30 | 150G | 50 XP | 퀘스트 사냥꾼 (`title_quest_hunter`) | - |
| Gold | 100 | 300G | 100 XP | 퀘스트 영웅 (`title_quest_hero`) | 영웅의 증표 장식품 x1 |

### 8.3 단일 업적 보상 상세

| achievementId | 골드 | XP | 칭호 | 아이템 |
|---------------|------|-----|------|--------|
| `ach_quest_01` | 50G | 20 XP | 모험의 시작 (`title_adventure_start`) | - |
| `ach_quest_03` | 150G | 50 XP | 마을의 해결사 (`title_village_solver`) | - |
| `ach_quest_04` | 150G | 50 XP | 근면한 농부 (`title_diligent_farmer`) | - |

**Quest 카테고리 소계**: 골드 850G / XP 340

---

## 9. 낚시사 (Angler) -- 4개

### 9.1 업적 목록

| # | achievementId | 이름 | 유형 | conditionType | 조건 상세 | 숨김 |
|---|---------------|------|------|---------------|----------|------|
| 1 | `ach_fish_01` | 첫 낚시 | Single | `FishCaughtCount` ([TODO] ARC-030에서 enum 추가 필요) | 처음으로 물고기 1마리 낚기. conditionValue = 1 | N |
| 2 | `ach_fish_02` | 낚시 애호가 | Tiered | `FishCaughtCount` ([TODO] ARC-030) | 누적 물고기 포획 횟수 (3단계) | N |
| 3 | `ach_fish_03` | 낚시꾼 | Single | `FishCaughtCount` ([TODO] ARC-030) | 누적 200마리 낚기. conditionValue = 200 | N |
| 4 | `ach_fish_04` | 전설의 낚시사 | Single | `FishSpeciesCollected` ([TODO] ARC-030에서 enum 추가 필요) | 어종 도감 15/15종 완성 (-> see `docs/systems/fishing-system.md` 섹션 4.2 — 전체 어종 15종). conditionValue = 15 | N |

[TODO] `FishCaughtCount`와 `FishSpeciesCollected`는 현재 `AchievementConditionType` enum(-> see `docs/systems/achievement-architecture.md` 섹션 2.3)에 미등록 상태다. ARC-030에서 enum 값 추가 + `FishingEvents.OnFishCaught` 이벤트 구독 핸들러 구현이 필요하다.

### 9.2 ach_fish_02 단계 상세 (낚시 애호가)

| 단계 | conditionValue | 골드 | XP | 칭호 | 아이템 |
|------|---------------|------|-----|------|--------|
| Bronze | 10 | 50G | 20 XP | - | - |
| Silver | 50 | 200G | 50 XP | 낚시 애호가 (`title_fishing_lover`) | 미끼통 x1 |
| Gold | 200 | 500G | 100 XP | 숙련 낚시꾼 (`title_skilled_angler`) | - |

**설계 의도**: Bronze(10마리)는 Zone F 해금 직후 자연스럽게 달성. Silver(50마리)는 낚시를 정기적으로 즐기는 중반 플레이어 대상. Gold(200마리)는 후반 장기 플레이 목표. Silver 단계에서 미끼통(미끼를 자동 장착해 주는 소품, 인벤토리 슬롯 절약)을 지급하여 낚시 편의성 향상.

[OPEN] 미끼통(Bait Box) 아이템의 상세 효과 및 아이템 시스템 내 등록은 미끼(Bait) 시스템 설계와 함께 확정 필요 (-> see `docs/systems/fishing-system.md` 섹션 8.3 [OPEN]).

### 9.3 단일 업적 보상 상세

| achievementId | 골드 | XP | 칭호 | 아이템 |
|---------------|------|-----|------|--------|
| `ach_fish_01` | 50G | 20 XP | 초보 낚시꾼 (`title_novice_angler`) | - |
| `ach_fish_03` | 500G | 100 XP | 낚시꾼 (`title_angler`) | 낚시 숙련도 XP 보너스 (다음 50회 포획 시 숙련도 XP +25%) |
| `ach_fish_04` | 1,000G | 100 XP | 전설의 낚시사 (`title_legendary_angler`) | 황금 낚싯대 장식품 x1 |

**설계 의도**: `ach_fish_01`(첫 낚시)은 "쉬움" 난이도(50G/20 XP)로 즉각적 성취감 제공. `ach_fish_03`(200마리)은 "어려움" 난이도(500G/100 XP)로, 추가로 숙련도 XP 부스트를 지급하여 낚시 마스터리 가속. `ach_fish_04`(도감 완성)는 게임 내 최종 도전급(1,000G/100 XP)으로, 칭호와 장식 아이템을 보상으로 제공한다. 숙련도 XP 보너스의 구체적 메커니즘은 낚시 숙련도 시스템(-> see `docs/systems/fishing-system.md` 섹션 7)을 따른다.

**Angler 카테고리 소계**: 골드 2,300G / XP 390

[NOTE] 정확한 소계 계산:
- `ach_fish_01`: 50G + 20 XP
- `ach_fish_02` Bronze: 50G + 20 XP, Silver: 200G + 50 XP, Gold: 500G + 100 XP
- `ach_fish_03`: 500G + 100 XP
- `ach_fish_04`: 1,000G + 100 XP
- **합계: 2,300G / 390 XP**

---

## 9.5 채집가 (Gatherer) -- 5개

### 9.5.1 업적 목록

| # | achievementId | 이름 | 유형 | conditionType | 조건 상세 | 숨김 |
|---|---------------|------|------|---------------|----------|------|
| 1 | `ach_gather_01` | 첫 채집 | Single | `GatherCount` ([TODO] ARC에서 enum 추가 필요) | 처음으로 채집물 1개 수집. conditionValue = 1 | N |
| 2 | `ach_gather_02` | 채집 애호가 | Tiered | `GatherCount` ([TODO] ARC) | 누적 채집물 수집 횟수 (3단계) | N |
| 3 | `ach_gather_03` | 채집 도감 완성 | Single | `GatherSpeciesCollected` ([TODO] ARC에서 enum 추가 필요) | 채집물 도감 27/27종 완성 (-> see `docs/systems/gathering-system.md` 섹션 3.9 — 전체 채집물 27종). conditionValue = 27 | N |
| 4 | `ach_gather_04` | 전설의 채집가 | Single | `GatherCount` ([TODO] ARC) | Legendary 채집물 누적 5개 수집 (-> see `docs/systems/gathering-system.md` 섹션 3.3~3.7 — Legendary 4종). Custom 추적: rarityFilter = Legendary. conditionValue = 5 | N |
| 5 | `ach_gather_05` | 채집 낫의 진화 | Single | `GatherSickleUpgraded` ([TODO] ARC에서 enum 추가 필요) | 채집 낫을 Legendary 등급으로 업그레이드 (-> see `docs/systems/gathering-system.md` 섹션 5.2). conditionValue = 1 | N |

[TODO] `GatherCount`, `GatherSpeciesCollected`, `GatherSickleUpgraded`는 현재 `AchievementConditionType` enum(-> see `docs/systems/achievement-architecture.md` 섹션 2.3)에 미등록 상태다. ARC에서 enum 값 추가 + `GatheringEvents.OnItemGathered` 이벤트 구독 핸들러 구현이 필요하다.

### 9.5.2 ach_gather_02 단계 상세 (채집 애호가)

| 단계 | conditionValue | 골드 | XP | 칭호 | 아이템 |
|------|---------------|------|-----|------|--------|
| Bronze | 20 | 50G | 20 XP | - | - |
| Silver | 100 | 200G | 50 XP | 채집 애호가 (`title_gathering_lover`) | - |
| Gold | 500 | 500G | 100 XP | 숙련 채집꾼 (`title_skilled_gatherer`) | 채집 숙련도 XP 보너스 (다음 50회 채집 시 숙련도 XP +25%) |

**설계 의도**: Bronze(20개)는 Zone D 해금 직후 며칠 순회로 자연스럽게 달성. Silver(100개)는 채집을 꾸준히 즐기는 중반 플레이어 대상 (~7일). Gold(500개)는 후반 장기 플레이 목표 (~35일). Gold 단계에서 채집 숙련도 XP 보너스를 지급하여 숙련도 최대 레벨(Lv.10) 도달을 가속한다.

### 9.5.3 단일 업적 보상 상세

| achievementId | 골드 | XP | 칭호 | 아이템 |
|---------------|------|-----|------|--------|
| `ach_gather_01` | 50G | 20 XP | 초보 채집꾼 (`title_novice_gatherer`) | - |
| `ach_gather_03` | 1,000G | 100 XP | 채집 박사 (`title_gathering_doctor`) | 채집 도감 장식품 x1 |
| `ach_gather_04` | 500G | 100 XP | 전설의 채집가 (`title_legendary_gatherer`) | - |
| `ach_gather_05` | 300G | 100 XP | 낫의 장인 (`title_sickle_master`) | - |

**설계 의도**: `ach_gather_01`(첫 채집)은 "쉬움" 난이도(50G/20 XP)로 즉각적 성취감 제공. `ach_gather_03`(도감 완성)은 27종 전부 수집이라는 극후반 도전(최소 4계절 필요)으로 최고 보상(1,000G/100 XP + 장식품). `ach_gather_04`(Legendary 5개)는 특수 조건 충족이 반복적으로 필요한 후반 도전. `ach_gather_05`(채집 낫 전설)는 3,000G + 광석 투자를 요구하는 "어려움" 난이도 업적으로, 도구 장인(Tool) 카테고리와 차별화하여 채집 전용 도구에 초점을 맞춘다.

**Gatherer 카테고리 소계**: 골드 2,600G / XP 490

[NOTE] 정확한 소계 계산:
- `ach_gather_01`: 50G + 20 XP
- `ach_gather_02` Bronze: 50G + 20 XP, Silver: 200G + 50 XP, Gold: 500G + 100 XP
- `ach_gather_03`: 1,000G + 100 XP
- `ach_gather_04`: 500G + 100 XP
- `ach_gather_05`: 300G + 100 XP
- **합계: 2,600G / 490 XP**

---

## 10. 숨겨진 업적 (Hidden) -- 7개

숨겨진 업적은 달성 전까지 업적 패널에서 이름/조건이 `"???"` 으로 표시된다. 모두 `Custom` (99) conditionType을 사용하며, AchievementManager 내 전용 핸들러로 처리한다 (-> see `docs/systems/achievement-architecture.md` 섹션 2.3).

### 10.1 업적 목록

| # | achievementId | 이름 | 조건 상세 | 추적 이벤트 | 보상 | 칭호 |
|---|---------------|------|----------|------------|------|------|
| 1 | `ach_hidden_01` | 비 오는 날의 수확 | 비 오는 날에 작물 10개 이상 수확 | `OnCropHarvested` + `OnWeatherActive` (weatherType = Rain) | 100G / 30 XP | 비의 농부 (`title_rain_farmer`) |
| 2 | `ach_hidden_02` | 밤의 방랑자 | 밤 시간대(22:00~24:00)에 농장 밖에서 활동 | `OnTimeAdvanced` (currentHour >= 22, playerLocation != Farm) | 100G / 30 XP | 야행성 농부 (`title_nocturnal_farmer`) |
| 3 | `ach_hidden_03` | 물만 주는 농부 | 하루 동안 물주기만 30회 이상 수행 (다른 도구 사용 금지) | `OnToolUsed` (일일 추적) | 100G / 30 XP | 물의 정원사 (`title_water_gardener`) |
| 4 | `ach_hidden_04` | 빈손의 부자 | 소지 골드 0G 상태에서 작물 판매로 단일 거래 500G 이상 달성 | `OnGoldChanged` + `OnSingleShipment` | 100G / 30 XP | 역전의 농부 (`title_comeback_farmer`) |
| 5 | `ach_hidden_05` | 거대 작물의 주인 | 거대 작물(Giant Crop) 수확 (-> see `docs/systems/crop-growth.md` 섹션 5.1) | `OnCropHarvested` (isGiant = true) | 100G / 30 XP | 거대 작물의 주인 (`title_giant_crop_owner`) |
| 6 | `ach_hidden_06` | 전부 다 팔아! | 인벤토리의 모든 슬롯에 작물이 있는 상태에서 한 번에 전부 출하 | `OnSingleShipment` (fullInventory = true) | 100G / 30 XP | 통큰 농부 (`title_generous_farmer`) |
| 7 | `ach_hidden_07` | 통합 수집 마스터 | 어종 도감 15/15종 완성(`ach_fish_04` 달성) + 채집 도감 27/27종 완성(`ach_gather_03` 달성). 두 업적 모두 달성 시 자동 해금 | `OnAchievementUnlocked` (achievementId = `ach_fish_04` AND `ach_gather_03`) | 100G / 30 XP | 수집의 대가 (`title_collection_master`) |

### 10.2 아이템 보상

숨겨진 업적 중 아이템 보상이 있는 것:

| achievementId | 아이템 | 효과 |
|---------------|--------|------|
| `ach_hidden_05` | 거대 씨앗 x1 | 심으면 거대 작물 생성 확률 50% 상승 (해당 작물 1회). (-> see `docs/systems/crop-growth.md` 섹션 5.1) |
| `ach_hidden_07` | 도감 배경: 전설의 자연 x1 | 수집 도감 화면 배경이 전설의 자연 일러스트로 변경 (-> see `docs/systems/collection-system.md` 섹션 5.2 — 채집 도감 마일스톤 27종 보상과 동일 비주얼, 별도 획득 경로) |

나머지 5개 숨겨진 업적(`ach_hidden_01`~`ach_hidden_04`, `ach_hidden_06`)은 칭호+골드+XP만 지급한다.

**Hidden 카테고리 소계**: 골드 700G / XP 210

[NOTE] 정확한 소계 계산:
- `ach_hidden_01`~`ach_hidden_06`: 각 100G + 30 XP = 600G + 180 XP
- `ach_hidden_07`: 100G + 30 XP
- **합계: 700G / 210 XP**

---

## 11. 칭호 전체 목록 (43종)

칭호는 플레이어 이름 앞에 표시되는 순수 장식 수식어이다. 게임플레이에 영향을 주지 않는다. (-> see `docs/systems/achievement-system.md` 섹션 4.2 for 표시 규칙)

### 11.1 칭호 canonical 테이블

| # | 칭호 ID | 칭호 이름 | 해금 조건 | 카테고리 |
|---|---------|----------|----------|----------|
| 0 | `title_farmer` | 농부 | 게임 시작 (기본 칭호) | - |
| 1 | `title_sprout_farmer` | 새싹 농부 | `ach_farming_01` 달성 | Farming |
| 2 | `title_skilled_farmer` | 숙련 농부 | `ach_farming_02` Silver 달성 | Farming |
| 3 | `title_harvest_master` | 수확의 대가 | `ach_farming_02` Gold 달성 | Farming |
| 4 | `title_four_seasons` | 사계절 농부 | `ach_farming_03` 달성 | Farming |
| 5 | `title_crop_doctor` | 작물 박사 | `ach_farming_04` 달성 | Farming |
| 6 | `title_legendary_farmer` | 전설의 경작자 | `ach_farming_05` 달성 | Farming |
| 7 | `title_novice_merchant` | 초보 상인 | `ach_economy_01` 달성 | Economy |
| 8 | `title_successful_farmer` | 성공한 농부 | `ach_economy_02` Silver 달성 | Economy |
| 9 | `title_wealth_accumulator` | 부의 축적자 | `ach_economy_02` Gold 달성 | Economy |
| 10 | `title_trade_genius` | 거래의 귀재 | `ach_economy_03` 달성 | Economy |
| 11 | `title_processing_master` | 가공 마스터 | `ach_economy_04` 달성 | Economy |
| 12 | `title_builder_novice` | 건축 입문자 | `ach_facility_01` 달성 | Facility |
| 13 | `title_facility_king` | 시설 왕 | `ach_facility_02` 달성 | Facility |
| 14 | `title_processing_empire` | 가공 제국의 주인 | `ach_facility_03` 달성 | Facility |
| 15 | `title_processing_artisan` | 가공 장인 | `ach_facility_04` Silver 달성 | Facility |
| 16 | `title_processing_master_gold` | 가공의 달인 | `ach_facility_04` Gold 달성 | Facility |
| 17 | `title_apprentice_smith` | 장인 견습생 | `ach_tool_01` 달성 | Tool |
| 18 | `title_tool_master` | 도구의 달인 | `ach_tool_02` 달성 | Tool |
| 19 | `title_legendary_smith` | 전설의 장인 | `ach_tool_03` 달성 | Tool |
| 20 | `title_social_farmer` | 사교적인 농부 | `ach_explorer_01` 달성 | Explorer |
| 21 | `title_traveler_friend` | 여행자의 친구 | `ach_explorer_02` 달성 | Explorer |
| 22 | `title_season_witness` | 계절의 증인 | `ach_explorer_03` 달성 | Explorer |
| 23 | `title_shopping_lover` | 쇼핑 애호가 | `ach_explorer_04` Silver 달성 | Explorer |
| 24 | `title_shopping_maniac` | 쇼핑 마니아 | `ach_explorer_04` Gold 달성 | Explorer |
| 25 | `title_adventure_start` | 모험의 시작 | `ach_quest_01` 달성 | Quest |
| 26 | `title_quest_hunter` | 퀘스트 사냥꾼 | `ach_quest_02` Silver 달성 | Quest |
| 27 | `title_quest_hero` | 퀘스트 영웅 | `ach_quest_02` Gold 달성 | Quest |
| 28 | `title_village_solver` | 마을의 해결사 | `ach_quest_03` 달성 | Quest |
| 29 | `title_diligent_farmer` | 근면한 농부 | `ach_quest_04` 달성 | Quest |
| 30 | `title_rain_farmer` | 비의 농부 | `ach_hidden_01` 달성 | Hidden |
| 31 | `title_nocturnal_farmer` | 야행성 농부 | `ach_hidden_02` 달성 | Hidden |
| 32 | `title_water_gardener` | 물의 정원사 | `ach_hidden_03` 달성 | Hidden |
| 33 | `title_comeback_farmer` | 역전의 농부 | `ach_hidden_04` 달성 | Hidden |
| 34 | `title_giant_crop_owner` | 거대 작물의 주인 | `ach_hidden_05` 달성 | Hidden |
| 35 | `title_generous_farmer` | 통큰 농부 | `ach_hidden_06` 달성 | Hidden |
| 36 | `title_collection_master` | 수집의 대가 | `ach_hidden_07` 달성 | Hidden |
| 37 | `title_novice_angler` | 초보 낚시꾼 | `ach_fish_01` 달성 | Angler |
| 38 | `title_fishing_lover` | 낚시 애호가 | `ach_fish_02` Silver 달성 | Angler |
| 39 | `title_skilled_angler` | 숙련 낚시꾼 | `ach_fish_02` Gold 달성 | Angler |
| 40 | `title_angler` | 낚시꾼 | `ach_fish_03` 달성 | Angler |
| 41 | `title_legendary_angler` | 전설의 낚시사 | `ach_fish_04` 달성 | Angler |
| 42 | `title_fishing_master` | 낚시 마스터 | 어종 도감 100% + 숙련도 Lv.10 (-> see `docs/systems/fishing-system.md` 섹션 8.2) | Angler |
| 43 | `title_novice_gatherer` | 초보 채집꾼 | `ach_gather_01` 달성 | Gatherer |
| 44 | `title_gathering_lover` | 채집 애호가 | `ach_gather_02` Silver 달성 | Gatherer |
| 45 | `title_skilled_gatherer` | 숙련 채집꾼 | `ach_gather_02` Gold 달성 | Gatherer |
| 46 | `title_gathering_doctor` | 채집 박사 | `ach_gather_03` 달성 | Gatherer |
| 47 | `title_legendary_gatherer` | 전설의 채집가 | `ach_gather_04` 달성 | Gatherer |
| 48 | `title_sickle_master` | 낫의 장인 | `ach_gather_05` 달성 | Gatherer |
| 49 | `title_gathering_master` | 채집 마스터 | 채집 도감 100% + 숙련도 Lv.10 (-> see `docs/systems/gathering-system.md` 섹션 4.2) | Gatherer |

### 11.2 칭호 통계

| 카테고리 | 칭호 수 |
|----------|---------|
| 기본 | 1 |
| Farming | 6 |
| Economy | 5 |
| Facility | 5 |
| Tool | 3 |
| Explorer | 5 |
| Quest | 5 |
| Angler | 6 |
| Gatherer | 7 |
| Hidden | 7 |
| **합계** | **50** |

---

## 12. 아이템 보상 전체 목록

업적 전용 아이템 보상 8종의 canonical 목록이다. (-> see `docs/systems/achievement-system.md` 섹션 4.3 for 설계 원칙)

| # | 아이템 | 해금 업적 | 수량 | 효과 | 비고 |
|---|--------|----------|------|------|------|
| 1 | 황금 씨앗 | `ach_farming_04` | x1 | 심으면 랜덤 Gold 품질 이상 작물 생성. 1회용 | 작물 도감 완성 보상 |
| 2 | 속성장 비료 | `ach_farming_02` Gold | x10 | (-> see `docs/systems/farming-system.md` 섹션 5.1 for 비료 효과) | 대량 수확 보상 |
| 3 | 금고 장식품 | `ach_economy_02` Gold | x1 | 농장 배치 가능 장식 오브젝트. 기능 없음 | 부의 축적 보상 |
| 4 | 고급 연료 | `ach_facility_03` | x20 | (-> see `docs/content/processing-system.md` for 연료 시스템) | 가공소 건설 보상 |
| 5 | 특수 레시피 | `ach_facility_04` Gold | x1 | [OPEN] 업적 전용 특수 레시피 vs 기존 레시피 조기 해금 미정 | 가공 달인 보상 |
| 6 | 장인의 망치 장식품 | `ach_tool_03` | x1 | 농장 배치 가능 장식 오브젝트. 기능 없음 | 전설 도구 보상 |
| 7 | 바람이 특별 할인권 | `ach_explorer_02` | x1 | 바람이 상점에서 1회 20% 할인 적용 | 바람이 단골 보상 |
| 8 | 영웅의 증표 장식품 | `ach_quest_02` Gold | x1 | 농장 배치 가능 장식 오브젝트. 기능 없음 | 퀘스트 달인 보상 |
| 9 | 상인의 뱃지 장식품 | `ach_explorer_04` Gold | x1 | 농장 배치 가능 장식 오브젝트. 기능 없음 | 쇼핑 마니아 보상 |
| 10 | 거대 씨앗 | `ach_hidden_05` | x1 | 심으면 거대 작물 생성 확률 50% 상승 (해당 작물 1회) (-> see `docs/systems/crop-growth.md` 섹션 5.1) | 거대 작물 보상 |
| 11 | 미끼통 | `ach_fish_02` Silver | x1 | 미끼 자동 장착 소품. 인벤토리 슬롯 1개 절약. [OPEN] 미끼 시스템 설계 시 효과 확정 (-> see `docs/systems/fishing-system.md` 섹션 8.3) | 낚시 애호가 Silver 보상 |
| 12 | 낚시 숙련도 XP 보너스 | `ach_fish_03` | x1 | 다음 50회 포획 시 낚시 숙련도 XP +25% (일회성 버프, -> see `docs/systems/fishing-system.md` 섹션 7) | 낚시꾼 보상 |
| 13 | 황금 낚싯대 장식품 | `ach_fish_04` | x1 | 농장 배치 가능 장식 오브젝트. 기능 없음 | 전설의 낚시사 보상 |
| 14 | 채집 숙련도 XP 보너스 | `ach_gather_02` Gold | x1 | 다음 50회 채집 시 채집 숙련도 XP +25% (일회성 버프, -> see `docs/systems/gathering-system.md` 섹션 4) | 채집 애호가 Gold 보상 |
| 15 | 채집 도감 장식품 | `ach_gather_03` | x1 | 농장 배치 가능 장식 오브젝트. 기능 없음 | 채집 도감 완성 보상 |
| 16 | 도감 배경: 전설의 자연 | `ach_hidden_07` | x1 | 수집 도감 화면 배경이 전설의 자연 일러스트로 변경 (-> see `docs/systems/collection-system.md` 섹션 5.2) | 통합 수집 마스터 보상 |

[OPEN] 장식 오브젝트(금고, 장인의 망치, 영웅의 증표, 상인의 뱃지, 황금 낚싯대, 채집 도감)의 비주얼 사양 및 배치 시스템은 별도 설계 필요. 현재 농장 꾸미기 시스템이 미설계 상태.

---

## 13. 전체 보상 총괄 테이블

### 13.1 카테고리별 보상 합계

| 카테고리 | 업적 수 | 골드 합계 | XP 합계 | 칭호 수 | 아이템 수 |
|----------|---------|----------|---------|---------|----------|
| Farming | 5 | 1,300G | 510 XP | 6 | 2 (황금 씨앗, 속성장 비료) |
| Economy | 4 | 850G | 340 XP | 5 | 1 (금고 장식품) |
| Facility | 4 | 1,000G | 340 XP | 5 | 2 (고급 연료, 특수 레시피) |
| Tool | 3 | 500G | 170 XP | 3 | 1 (장인의 망치 장식품) |
| Explorer | 4 | 950G | 370 XP | 5 | 2 (할인권, 상인의 뱃지) |
| Quest | 4 | 850G | 340 XP | 5 | 1 (영웅의 증표) |
| Angler | 4 | 2,300G | 390 XP | 5 (+마스터 1 = 6) | 3 (미끼통, 숙련도 XP 보너스, 황금 낚싯대 장식품) |
| Gatherer | 5 | 2,600G | 490 XP | 6 (+마스터 1 = 7) | 2 (채집 숙련도 XP 보너스, 채집 도감 장식품) |
| Hidden | 7 | 700G | 210 XP | 7 | 2 (거대 씨앗, 도감 배경: 전설의 자연) |
| **합계** | **40** | **11,050G** | **3,160 XP** | **49 (+기본 1 = 50)** | **16** |

### 13.2 밸런스 검증 포인트

- **총 XP 3,160**: 전체 필요 누적 XP 9,029의 약 35.0% (-> see `docs/balance/progression-curve.md` 섹션 2.4.1 — canonical XP 테이블). CON-017에서 `ach_hidden_07`(30 XP) 추가로 기존 3,130에서 30 XP 증가
- **총 골드 11,050G**: 게임 2년차 추정 누적 수입 대비 소규모 보너스 (-> see `docs/balance/progression-curve.md` 섹션 3). CON-017에서 `ach_hidden_07`(100G) 추가로 기존 10,950G에서 100G 증가
- 전 업적 완료가 2년차 이후에나 가능하므로, 실질적 밸런스 영향은 제한적
- 낚시/채집 업적의 골드 비중이 다소 높다 (낚시 2,300G + 채집 2,600G = 4,900G / 11,050G = 44.3%). 이들은 각각 15종/27종 전종 수집이라는 극후반 도전이므로 적정 수준으로 판단

[RISK] 섹션 2.4에서 추정한 총 XP(~1,690)와 본 섹션의 합산(3,160)에 약 1,470 XP 차이가 있다. 이는 섹션 2.4가 기준 테이블의 최소치로 추정한 반면, 각 업적의 확정 보상이 일부 상향되었고 낚시 업적 4종(390 XP) + 채집 업적 5종(490 XP) + 통합 수집 마스터 1종(30 XP)이 추가되었기 때문이다. 섹션 2.4의 수치를 본 섹션(13.1)의 확정 합산으로 갱신한다: **총 XP = 3,160, 전체 XP 대비 약 35.0%** (canonical 기준 9,029 XP).

[NOTE — BAL-019 확정] canonical XP 기준 9,029 대비 업적 XP 비중 35.0%는 목표 범위(**30~40%**, BAL-019 조정) 내이다. 현상 유지 확정. (-> see `docs/balance/bal-019-xp-balance.md`)

---

## Cross-references

| 관련 문서 | 관계 |
|----------|------|
| `docs/systems/achievement-system.md` | 업적 시스템 설계 (카테고리, 데이터 모델, UI/UX, 추적 이벤트, 보상 체계 정의). 본 문서의 상위 설계 문서 |
| `docs/systems/achievement-architecture.md` | 기술 아키텍처 (AchievementData SO 필드, AchievementConditionType enum, 이벤트 구독 매핑) |
| `docs/balance/progression-curve.md` | XP 테이블(섹션 1.3.2), 자금 곡선(섹션 3) 참조. 업적 보상 XP/골드 밸런스 검증 |
| `docs/design.md` | 작물 목록(섹션 4.2), 시설 목록(섹션 4.6) 참조 |
| `docs/content/crops.md` | 작물 ID, 계절별 분류, 겨울 전용 작물 3종 포함 총 11종 확인 |
| `docs/content/facilities.md` | 시설 ID, 점유 타일, 건설 시간, 가공소 4종 확인 |
| `docs/content/npcs.md` | NPC 4명(하나, 철수, 목이, 바람이) 이름 확인 |
| `docs/systems/tool-upgrade.md` | 업그레이드 등급(Reinforced, Legendary) 확인 |
| `docs/systems/crop-growth.md` | 품질 등급(Normal~Iridium), 거대 작물(Giant Crop) 조건 확인 |
| `docs/content/processing-system.md` | 가공품 레시피, 연료 시스템 참조 |
| `docs/systems/quest-system.md` | 퀘스트 완료 이벤트, 농장 도전과의 중복 관리 |
| `docs/systems/fishing-system.md` | 어종 15종 목록(섹션 4.2), 낚시 숙련도(섹션 7), 업적 연계(섹션 8.1), 어종 도감(섹션 8.2) |
| `docs/systems/fishing-architecture.md` | FishingEvents.OnFishCaught 이벤트 — ARC-030에서 enum 추가 시 참조 |
| `docs/systems/collection-system.md` | 통합 수집 도감 시스템 (DES-018). `ach_hidden_07` 통합 수집 마스터 업적 조건 및 도감 배경 보상 참조 |
| `docs/systems/gathering-system.md` | 채집 시스템 (채집물 27종, 채집 포인트, 숙련도, 채집 낫 등급) — 채집가 업적 조건 참조 |
| `docs/content/gathering-items.md` | 채집 아이템 상세 (27종 아이템 속성) — 채집 도감 업적 조건 참조 |
| `docs/systems/economy-system.md` | 골드 보상이 경제 밸런스에 미치는 영향 |
| `docs/systems/time-season.md` | 계절 전환, 날씨 시스템, 밤 시간대(22:00~24:00) 참조 |
| `docs/systems/farming-system.md` | 비료 효과, 에너지 시스템 참조 |

---

## Open Questions & Risks

### [OPEN] 항목

1. **[OPEN]** 경제 달인 Gold 단계(100,000G) 달성 가능 시점의 정밀 시뮬레이션 필요 (-> see 섹션 4.2)
2. **[OPEN]** `ach_facility_04` Gold 단계의 특수 레시피 보상 설계 미정 (-> see 섹션 5.2)
3. **[OPEN]** 장식 오브젝트(금고, 장인의 망치, 영웅의 증표, 상인의 뱃지)의 비주얼 사양 및 농장 배치 시스템 미설계 (-> see 섹션 11)
4. ~~**[OPEN]** `ach_explorer_02`/`ach_explorer_04` conditionType 불일치~~ — **RESOLVED (FIX-011)**: `PurchaseCount` (14) 전용 enum 값 추가, `EconomyEvents.OnShopPurchased` 이벤트 구독으로 해소 (-> see `docs/systems/achievement-architecture.md` 섹션 2.3)
5. **[OPEN]** 낚시 업적의 `FishCaughtCount`, `FishSpeciesCollected` conditionType이 `AchievementConditionType` enum에 미등록 상태. ARC-030에서 enum 값 추가 + `FishingEvents.OnFishCaught` 이벤트 핸들러 구현 필요 (-> see 섹션 9.1)
6. **[OPEN]** 미끼통(Bait Box) 아이템 효과가 미끼 시스템 미설계로 미확정 (-> see 섹션 9.2, `docs/systems/fishing-system.md` 섹션 8.3)
7. **[OPEN]** `ach_fish_03`의 숙련도 XP 보너스(+25%, 50회) 메커니즘 상세 미확정. 버프 시스템 또는 낚시 숙련도 내부 처리로 구현할지 결정 필요
8. **[OPEN]** 채집 업적의 `GatherCount`, `GatherSpeciesCollected`, `GatherSickleUpgraded` conditionType이 `AchievementConditionType` enum에 미등록 상태. ARC에서 enum 값 추가 + `GatheringEvents.OnItemGathered` 이벤트 핸들러 구현 필요 (-> see 섹션 9.5.1)
9. **[OPEN]** `ach_gather_02` Gold 단계의 채집 숙련도 XP 보너스(+25%, 50회) 메커니즘은 `ach_fish_03`과 동일 패턴. 버프 시스템 공통화 설계 필요
10. **[OPEN]** `ach_hidden_07`(통합 수집 마스터)의 추적 방식: 기존 숨겨진 업적은 `Custom` (99) conditionType으로 개별 핸들러를 사용하지만, `ach_hidden_07`은 다른 업적 2종(`ach_fish_04`, `ach_gather_03`)의 달성 여부를 조합하는 메타 조건이다. `OnAchievementUnlocked` 이벤트가 기존 아키텍처에 존재하는지 확인 필요 (-> see `docs/systems/achievement-architecture.md` 섹션 2.3). 존재하지 않으면 ARC 태스크로 추가해야 한다.

### [RISK] 항목

1. ~~**[RISK]** 업적 XP 총합(2,250)이 전체 필요 XP(4,609)의 약 49%로, 당초 목표 범위(33~43%)를 초과한다.~~ — **RESOLVED (CON-013/CON-017)**: canonical XP 기준이 9,029로 변경되어 업적 XP 비중은 35.0%(3,160/9,029)로 목표 범위(33~43%) 내 (-> see 섹션 2.4)
2. **[RISK]** 업적 XP + 퀘스트 XP 합산 시 레벨업 속도 과도 가속 가능성. 출시 전 수치 시뮬레이션 필수 (-> see `docs/balance/progression-curve.md`)
3. **[RISK]** 세이브 데이터 마이그레이션: 업적 추가/수정 시 기존 세이브 파일 호환성 유지 필요 (-> see `docs/systems/achievement-architecture.md`)

---

> 이 문서는 Claude Code가 자율적으로 작성했습니다.

---

# 대장간 NPC 상세 문서 (Blacksmith NPC Detail Specification)

> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: CON-004 | Phase 1

---

## 1. Context

이 문서는 대장간 NPC "철수(Cheolsu)"의 캐릭터 심화 설계, 전체 대화 스크립트, 업그레이드 인터페이스 UX, 영업 조건을 상세히 기술한다. `docs/content/npcs.md` 섹션 4에 정의된 기본 설정을 확장하며, `docs/systems/tool-upgrade.md`의 업그레이드 메카닉과 완전히 연동된다.

**설계 목표**: 철수는 과묵한 장인이라는 캐릭터성을 통해, 짧은 대사에서도 도구에 대한 깊은 애착과 플레이어에 대한 은근한 관심을 전달한다. 대화가 단순한 메뉴 진입점이 아니라, 도구 업그레이드의 전략적 타이밍에 대한 간접 가이드 역할을 수행하도록 설계한다.

### 1.1 본 문서가 canonical인 데이터

- 철수 캐릭터 심화 설정 (배경 스토리, 성격 디테일, 관계 발전 단계)
- 대장간 전체 대화 스크립트 (상황별, 계절별, 친밀도별, 특수 이벤트별)
- 업그레이드 인터페이스 UX 설계 (화면 레이아웃, 전환 흐름, 피드백, 연출)
- 접근성 설계 (색맹 대응, 아이콘 체계)

### 1.2 본 문서가 canonical이 아닌 데이터 (참조만)

| 데이터 종류 | 참조처 |
|------------|--------|
| NPC 기본 정보 (이름, 나이, 외형 요약) | `docs/content/npcs.md` 섹션 4.1 |
| 영업시간, 휴무일 | `docs/systems/economy-system.md` 섹션 3.2 |
| 도구 업그레이드 비용, 재료, 레벨 요건, 소요 시간 | `docs/systems/tool-upgrade.md` 섹션 2 |
| 도구별 성능 수치 (범위, 에너지, 특수 효과) | `docs/systems/tool-upgrade.md` 섹션 3 |
| 대장간 상점 판매 품목, 가격 | `docs/systems/tool-upgrade.md` 섹션 6.3 |
| 상점 공통 UI 흐름 | `docs/content/npcs.md` 섹션 8 |
| NPC 대화 시스템 구조 (트리거, 우선순위) | `docs/content/npcs.md` 섹션 7 |

---

## 2. 캐릭터 설계

### 2.1 기본 정보

| 항목 | 내용 |
|------|------|
| 이름 | 철수 (Cheolsu) |
| 영문 ID | `npc_cheolsu` |
| 나이 | 40대 후반 |
| 성별 | 남성 |
| 역할 | 대장간 장인 (도구 업그레이드, 재료 판매) |
| 위치 | 마을 외곽, 대장간 건물 내부 |

기본 캐릭터 설정은 (-> see `docs/content/npcs.md` 섹션 4.1)을 따른다.

### 2.2 외형 상세

로우폴리 3D 스타일에 맞춘 캐릭터 디자인.

| 요소 | 묘사 |
|------|------|
| 체형 | 넓은 어깨와 굵은 팔. 상체가 발달한 체형 (장인의 육체노동 반영) |
| 피부 | 짙은 갈색으로 그을린 피부. 로우폴리 텍스처에서 얼굴 음영으로 표현 |
| 머리 | 짧은 흑갈색 머리, 진한 남색 두건으로 감싸고 있음 |
| 복장 | 두꺼운 가죽 앞치마 (진갈색), 안쪽에 무채색 작업복. 소매를 걷어 올린 상태 |
| 장갑 | 한 손에만 두꺼운 가죽 장갑 착용 (다른 손은 맨손 — 섬세한 작업용) |
| 특징적 요소 | 왼쪽 볼에 작은 화상 자국 (로우폴리에서는 약간 밝은 색 패치로 표현) |
| 대기 애니메이션 | 모루 옆에 서서 팔짱을 끼고 있거나, 집게로 쇠를 두드리는 동작 반복 |

**로우폴리 표현 가이드**: 디테일은 텍스처가 아닌 실루엣과 색상 대비로 전달한다. 넓은 어깨-좁은 허리 실루엣, 앞치마의 진갈색-작업복의 회색 대비, 두건의 남색 포인트가 원거리에서도 "대장장이"임을 즉시 인식시킨다.

### 2.3 성격 상세

| 측면 | 설명 |
|------|------|
| 말투 | 짧고 건조한 문장. 불필요한 수식어를 쓰지 않는다. 존댓말과 반말 사이의 중간 어투("~해", "~야", "~군") |
| 도구에 대한 태도 | 도구를 단순한 물건이 아닌 "작품"으로 여긴다. 업그레이드한 도구를 건넬 때 눈에 띄게 뿌듯해한다 |
| 플레이어에 대한 태도 | 처음에는 관심 없는 듯하지만, 업그레이드를 거듭할수록 실력을 인정하며 말이 조금씩 길어진다 |
| 감정 표현 | 직접적으로 감정을 드러내지 않는다. 칭찬도 "...나쁘지 않아"처럼 우회적으로 표현 |
| 숨은 면모 | 비 오는 날이나 겨울 한가한 시기에 방문하면 평소보다 조금 더 말이 많아진다 |

### 2.4 배경 스토리

철수는 이 마을에서 3대째 대장간을 이어온 장인이다. 할아버지가 마을을 개척할 때 농기구를 만들었고, 아버지가 기술을 이어받았으며, 철수가 그 뒤를 잇고 있다. "좋은 도구가 좋은 농사의 시작"이라는 할아버지의 말을 좌우명으로 삼고 있다.

젊은 시절 도시의 대형 공방에서 일한 적이 있으나, 대량 생산 방식에 회의를 느끼고 마을로 돌아왔다. 하나하나 정성을 들여 만드는 것이 자신의 방식이라고 믿는다. 마을에 새 농부가 오면 겉으로는 무관심한 척하지만, 그 농부의 도구 상태를 항상 주시하고 있다.

**게임 내 노출**: 배경 스토리는 친밀도 단계에 따라 단편적으로 드러난다. 초반에는 알 수 없고, 반복 방문과 업그레이드를 통해 조금씩 대화에서 과거 이야기가 흘러나온다.

### 2.5 관계 발전 단계

플레이어와 철수의 관계는 4단계로 발전한다. 단계 전환은 친밀도 포인트 누적(업그레이드 완료 +5, 재료 구매 +1, 일일 대화 +1)에 기반한다.

| 단계 | 영문 키 | 친밀도 임계값 | 관계 묘사 |
|------|---------|-------------|-----------|
| 낯선 사이 | `Stranger` | 0 (초기) | 최소한의 응대. 사무적 대화만 한다 |
| 알고 지내는 사이 | `Acquaintance` | 10 | 서비스를 여러 번 이용한 손님. 가끔 도구 상태에 대한 짧은 언급을 건넨다 |
| 단골 | `Regular` | 25 | 이름을 부르기 시작. 도구 관련 팁을 자발적으로 알려준다 |
| 친구 | `Friend` | 50 | 과거 이야기를 꺼낸다. 대사가 눈에 띄게 길어지고 따뜻해진다. **재료 구매 10% 할인 혜택** 제공 |

**임계값 canonical**: 이 테이블의 친밀도 임계값(`[0, 10, 25, 50]`)이 canonical이다. `docs/systems/blacksmith-architecture.md` 섹션 4의 `affinityThresholds`는 이 테이블을 참조한다.

**설계 의도**: 친밀도는 별도의 호감도 시스템(-> see `docs/content/npcs.md` Open Question 2, 미결정)과 독립적으로 동작하는 간단한 상호작용 카운터 기반 시스템이다. 복잡한 선물/퀘스트 없이, 자주 방문하고 서비스를 이용하는 것만으로 관계가 발전한다.

**[RESOLVED] 재료 구매 할인 도입 여부**: Friend 단계 보상으로 재료 구매 10% 할인(`discountRate = 0.1`)을 채택한다. 아키텍처 문서(`blacksmith-architecture.md` 섹션 5.3)에 구현 스펙이 확정되어 있다.

[OPEN] NPC 호감도 시스템이 도입될 경우, 관계 발전 단계를 호감도 시스템과 통합할지, 별도로 유지할지 결정 필요.

---

## 3. 대화 시스템 설계

### 3.1 최초 만남 대화

게임 시작 후 철수에게 처음 말을 걸 때 1회만 재생된다.

```
[철수] "...응. 새 농부구나.
        도구가 보이는데, 기본 도구로군.
        나중에 좀 더 쓸 만한 게 필요하면 재료를 가져와. 만들어 줄 테니."
```

**설계 의도**: 3문장으로 (1) 플레이어 인식, (2) 현재 도구 상태 파악, (3) 서비스 안내를 완료한다. 철수의 과묵한 성격을 반영하여 최소한의 말로 필요한 정보를 전달한다.

### 3.2 일반 방문 대화

재방문 시 랜덤으로 하나가 출력된다. 동일 대사 연속 재생 방지 규칙은 (-> see `docs/content/npcs.md` 섹션 7.3)을 따른다.

#### 범용 대사 (계절 무관, 5종)

| ID | 대사 |
|----|------|
| `general_01` | "왔나. 볼일이 있으면 말해." |
| `general_02` | "도구 상태는 어때? 무리하게 쓰지 마." |
| `general_03` | "...필요한 게 있으면." |
| `general_04` | "이 모루가 벌써 20년째야. 도구보다 오래 쓰는 건 모루뿐이지." |
| `general_05` | "농사는 잘 되고 있나. ...아, 상관없다면 됐고." |

#### 계절별 대사 (각 계절 2종, 총 8종)

| 계절 | ID | 대사 |
|------|-----|------|
| 봄 | `spring_01` | "봄이 오면 호미질할 일이 많겠지. 호미 상태 한번 확인해 봐." |
| 봄 | `spring_02` | "땅이 풀리니까 경작하기 좋을 거야. 도구 날이 안 무뎌졌나 보고." |
| 여름 | `summer_01` | "여름엔 작물이 많아지니까 낫을 올릴 때가 되지 않았나." |
| 여름 | `summer_02` | "더워서 대장간이 지옥이야. ...원래 그렇지만." |
| 가을 | `autumn_01` | "수확 시즌이니까 도구 점검은 미리 해 두는 게 좋아." |
| 가을 | `autumn_02` | "가을 바람이 불면 쇠가 잘 식어. 작업하기 좋은 계절이야." |
| 겨울 | `winter_01` | "겨울이라 한가하긴 한데... 도구 정비할 시간이긴 하지." |
| 겨울 | `winter_02` | "할 일이 없으면 앉아 있어도 돼. ...자리는 불편하겠지만." |

**대사 출력 규칙**: 현재 계절에 해당하는 계절 대사 2종 + 범용 대사 5종 = 총 7종 풀에서 랜덤 선택. 가중치는 계절 대사 40%, 범용 대사 60%.

### 3.3 업그레이드 요청 대화

플레이어가 "도구 업그레이드"를 선택하고 특정 도구+등급을 확정했을 때 출력된다.

#### 등급별 공통 대사

| 업그레이드 | 대사 |
|-----------|------|
| Basic -> Reinforced | "강화라... 좋아, 맡겨. 하루면 돼. 그동안 이 도구 없이 버텨야 해." |
| Reinforced -> Legendary | "전설급을 만들겠다고? ...각오는 된 거지. 이틀 걸려. 제대로 만들어 줄게." |

#### 도구별 추가 대사 (업그레이드 의뢰 직후 추가로 출력)

| 도구 | 등급 | 대사 |
|------|------|------|
| 호미 | Reinforced | "호미 날을 넓히면 경작이 한결 편해질 거야." |
| 호미 | Legendary | "이 호미면 돌도 잡초도 한 번에 밀어버릴 수 있어." |
| 물뿌리개 | Reinforced | "통을 키우고 분사구를 넓힐게. 물 주는 시간이 확 줄 거야." |
| 물뿌리개 | Legendary | "최고급 물뿌리개야. 물줄기가 작물을 조금 더 빨리 키워줄 거야." |
| 낫 | Reinforced | "날을 더 넓히면 한 번에 세 줄을 벨 수 있어." |
| 낫 | Legendary | "이 낫이면 베는 순간 작물이 더 좋은 상태로 떨어질 거야." |

### 3.4 업그레이드 완료 대화

완성된 도구를 수령할 때 출력된다. 도구 종류별로 차별화한다.

| 도구 | 등급 | 대사 |
|------|------|------|
| 호미 | Reinforced | "다 됐어. 날이 넓어졌지? 한 번 휘두르면 알 거야." |
| 호미 | Legendary | "...이건 내 최고작 중 하나야. 소중하게 써." |
| 물뿌리개 | Reinforced | "완성이야. 통이 커진 만큼 물도 많이 담겨. 리필 잊지 마." |
| 물뿌리개 | Legendary | "전설급 물뿌리개야. 물줄기에 기운을 담았어. 작물이 좋아할 거야." |
| 낫 | Reinforced | "날을 갈아 넣었어. 잘 베일 테니 조심해." |
| 낫 | Legendary | "이 낫은... 베는 게 아니라 거둔다고 해야겠지. 받아." |

### 3.5 특수 대화

특정 조건에서 1회만 트리거되는 대화.

| 조건 | 트리거 | 대사 |
|------|--------|------|
| 첫 번째 업그레이드 완료 (도구 무관) | 아무 도구든 첫 Reinforced 완성 시 | "첫 번째 강화 도구야. 차이를 느낄 거야. ...잘 쓰라고." |
| 첫 Legendary 달성 | 아무 도구든 첫 Legendary 완성 시 | "전설급이라고 대단한 이름을 붙였지만... 실은 내가 만들 수 있는 최선이란 뜻이야. 쓰면서 그 의미를 알게 될 거야." |
| 모든 도구 Reinforced 달성 | 호미+물뿌리개+낫 모두 Reinforced 이상 | "전부 강화했군. 꽤 쓸 줄 아는 농부네." |
| 모든 도구 Legendary 달성 | 호미+물뿌리개+낫 모두 Legendary | "...전설급 도구를 전부 갖추다니. 할아버지가 살아 계셨으면 좋아하셨을 텐데. 네 농장은 이 마을의 자랑이야." |

**설계 의도**: "모든 도구 Legendary 달성" 대사에서 할아버지 언급으로 배경 스토리를 자연스럽게 노출한다. 이 대사는 게임 내 도구 시스템 완전 정복의 보상으로, 감정적 만족감을 제공한다.

### 3.6 거절 대화

조건 미충족 시 출력되는 대사.

| 상황 | 대사 |
|------|------|
| 레벨 미달 | "...아직 이르군. 좀 더 경험을 쌓고 와." |
| 재료 부족 | "재료가 모자라. {재료이름}이(가) {부족수량}개 더 필요해." |
| 골드 부족 | "돈이 모자라는데. 더 모아서 와." |
| 골드 + 재료 둘 다 부족 | "돈도 재료도 모자라. 준비가 되면 다시 와." |
| 이미 도구가 작업 중 | "이미 작업 중인 도구가 있어. 먼저 가져가고 다시 말해." |
| 이미 Legendary (최고 등급) | "이미 최고야. 더 이상 할 수 있는 게 없어. ...잘 쓰고 있지?" |
| 완성 도구 미수령 상태에서 새 의뢰 시도 | "도구 다 됐어. 먼저 가져가." |

**{재료이름} 변수 치환 규칙**: 실제 부족한 재료명을 동적으로 삽입한다. 예: "재료가 모자라. 철 조각이 2개 더 필요해."

### 3.7 친밀도 단계별 대화

관계 발전 단계(섹션 2.5)에 따라 추가되는 대사.

#### 낯선 사이 (Stranger)

기본 범용/계절 대사만 출력. 추가 대사 없음.

#### 단골 (Regular)

기존 대사 풀에 다음이 추가된다.

| ID | 대사 |
|----|------|
| `regular_01` | "요즘 자주 오는군, {플레이어명}. 도구는 잘 쓰고 있어?" |
| `regular_02` | "비 오는 날엔 물뿌리개가 필요 없잖아. 그때 맡기면 똑똑한 거야." |
| `regular_03` | "수확 끝나고 심기 전에 호미를 맡기면 좋을 거야." |

**변화**: 플레이어 이름을 부르기 시작한다 (`{플레이어명}` 변수).

#### 친구 (Friend)

기존 대사 풀에 다음이 추가된다.

| ID | 대사 |
|----|------|
| `friend_01` | "사실 나, 젊을 때 도시에서 일한 적 있어. 공장에서 도구를 찍어내는 거지. ...맞지 않더라고." |
| `friend_02` | "할아버지가 그랬어. 좋은 도구는 쓰는 사람을 닮는다고. {플레이어명} 도구도 그렇게 되겠지." |
| `friend_03` | "...오늘은 일찍 왔네. 괜찮아, 좀 쉬었다 가." |
| `friend_04` | "이 대장간이 없어지면 마을에 농기구 만들 사람이 없어져. ...그래서 계속하는 거야." |

**변화**: 배경 스토리 단편이 대사에 자연스럽게 녹아든다. 말이 길어지고 감정이 드러난다.

### 3.8 전략 힌트 대사

특정 조건에서 도구 업그레이드 타이밍에 대한 간접적 힌트를 제공한다. (-> see `docs/content/npcs.md` 섹션 4.5의 기존 힌트를 확장)

| 힌트 대사 | 출력 조건 | 전략 의도 |
|-----------|----------|-----------|
| "비 오는 날엔 물뿌리개가 필요 없잖아. 그때 맡기면 똑똑한 거야." | 비 오는 날 + 물뿌리개 미업그레이드 + 단골 이상 | 도구 부재 기간 최소화 전략 유도 |
| "수확 끝나고 심기 전에 호미를 맡기면 좋을 거야." | 수확 시즌 + 호미 미업그레이드 + 단골 이상 | 호미 업그레이드 최적 타이밍 안내 |
| "낫은 작물이 다 자라기 전에 맡겨야 손해 안 봐." | 작물 성장 초기 + 낫 미업그레이드 + 단골 이상 | 낫 업그레이드 타이밍 안내 |
| "겨울에 도구를 맡기면 쓸 일이 없으니까 손해 볼 것도 없어." | 겨울 시즌 + 미업그레이드 도구 존재 + 단골 이상 | 겨울 비수기 활용 유도 |

**설계 의도**: 힌트 대사는 친밀도 "단골" 이상에서만 출력된다. 낯선 사이에서는 전략 힌트를 주지 않아, 관계 발전의 실질적 보상으로 기능한다.

---

## 4. 업그레이드 인터페이스 UX

### 4.1 대화에서 업그레이드 UI로의 전환 흐름

```
[1] 플레이어가 E키로 철수에게 말을 건다
    │
    ▼
[2] 인사말 출력 + 선택지 표시
    ┌─────────────────────────────────┐
    │ [철수 인사말 대사]              │
    │                                 │
    │  ► 도구 업그레이드              │
    │    재료 구매                    │
    │    도구 수령 (완성 시에만 표시)  │
    │    이야기하기                   │
    │    나가기                       │
    └─────────────────────────────────┘
    │
    ▼ ("도구 업그레이드" 선택)
[3] 업그레이드 선택 화면으로 전환 (섹션 4.2)
    │
    ▼ (도구+등급 선택)
[4] 확인 팝업 (섹션 4.4)
    │
    ▼ (확인)
[5] 의뢰 완료 연출 + 철수 대사 출력
    ├── 골드/재료 차감
    ├── 도구 잠금 (인벤토리에서 회색 처리)
    └── 대화 선택지로 복귀
```

### 4.2 업그레이드 선택 화면 레이아웃

```
┌──────────────────────────────────────────────────────────────┐
│  도구 업그레이드                                    [X 닫기] │
├──────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐                  │
│  │  [호미]   │  │[물뿌리개]│  │  [낫]    │                  │
│  │  아이콘   │  │  아이콘   │  │  아이콘  │                  │
│  │          │  │          │  │          │                  │
│  │ ★☆☆     │  │ ★★☆     │  │ ★☆☆     │  ← 현재 등급     │
│  │ Basic    │  │Reinforced│  │ Basic    │                  │
│  └──────────┘  └──────────┘  └──────────┘                  │
│       ▲                            ▲                        │
│    선택 가능                    선택 가능                    │
│  (파란 테두리)               (파란 테두리)                  │
│                                                              │
│  ※ 이미 최고 등급인 도구는 금색 체크 표시, 선택 불가         │
│  ※ 레벨 미달 도구는 자물쇠 아이콘 + 회색 처리               │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│  [하단: 선택된 도구의 상세 정보 패널 — 섹션 4.3 참조]       │
└──────────────────────────────────────────────────────────────┘
```

**도구 슬롯 상태 표시**:

| 상태 | 시각 표현 | 아이콘 |
|------|-----------|--------|
| 업그레이드 가능 | 파란 테두리, 밝은 배경 | 위쪽 화살표 (↑) |
| 레벨 미달 | 회색 처리, 어두운 배경 | 자물쇠 |
| 최고 등급 달성 | 금색 테두리, 체크 마크 | 금색 별 |
| 현재 작업 중 | 주황색 테두리, 모루+망치 아이콘 | 작업 진행 바 |

### 4.3 도구 상세 정보 패널 (스펙 비교)

도구를 선택하면 하단에 현재 등급과 다음 등급의 스펙을 나란히 비교한다.

```
┌──────────────────────────────────────────────────────────────┐
│  호미 업그레이드: Basic → Reinforced                         │
├─────────────────────────┬────────────────────────────────────┤
│   현재 (Basic)          │   업그레이드 후 (Reinforced)       │
│                         │                                    │
│   범위: 1x1             │   범위: 1x3           ▲ 3배       │
│   에너지: 2             │   에너지: 3           ▼ +1        │
│   쿨다운: 0.6초         │   쿨다운: 0.8초       ▼ +0.2초    │
│   특수: 없음            │   특수: 돌 자동 제거   ★ NEW      │
├─────────────────────────┴────────────────────────────────────┤
│                                                              │
│   필요 재료:  철 조각 x3  [✓ 보유: 5개]                      │
│   필요 골드:  800G       [✓ 보유: 1,200G]                    │
│   소요 시간:  1일                                            │
│   레벨 요건:  Lv.3       [✓ 현재: Lv.4]                      │
│                                                              │
│              [ 업그레이드 의뢰 ]    [ 취소 ]                  │
└──────────────────────────────────────────────────────────────┘
```

**스펙 비교 표시 규칙**:

| 변화 방향 | 표시 | 색상 |
|-----------|------|------|
| 수치 증가 (좋은 방향) | ▲ + 증가량 | 녹색 |
| 수치 증가 (나쁜 방향, 에너지/쿨다운) | ▼ + 증가량 | 주황색 |
| 신규 효과 추가 | ★ NEW | 금색 |

모든 수치는 (-> see `docs/systems/tool-upgrade.md` 섹션 3)에서 동적으로 로드한다. 이 UI에 수치를 하드코딩하지 않는다.

### 4.4 확인 팝업

```
┌──────────────────────────────────────────┐
│                                          │
│   호미를 강화 등급으로 업그레이드합니다.  │
│                                          │
│   비용: 800G + 철 조각 x3                │
│   소요 시간: 1일                         │
│                                          │
│   ⚠ 업그레이드 중 호미를 사용할 수       │
│     없습니다.                            │
│                                          │
│        [ 확인 ]      [ 취소 ]            │
│                                          │
└──────────────────────────────────────────┘
```

비용, 재료, 소요 시간: (-> see `docs/systems/tool-upgrade.md` 섹션 2.1)

### 4.5 재료 부족 시 피드백

업그레이드 선택 화면에서 조건이 미충족인 항목을 시각적으로 명확히 표시한다.

```
│   필요 재료:  철 조각 x3  [✗ 보유: 1개 — 2개 부족]          │
│   필요 골드:  800G       [✗ 보유: 500G — 300G 부족]          │
│   레벨 요건:  Lv.3       [✗ 현재: Lv.2]                      │
│                                                              │
│              [ 업그레이드 의뢰 ]    [ 취소 ]                  │
│                 (비활성, 회색)                                │
```

| 항목 | 충족 시 | 미충족 시 |
|------|---------|-----------|
| 재료 | 녹색 체크 + "보유: N개" | 적색 X + "보유: N개 -- M개 부족" |
| 골드 | 녹색 체크 + "보유: N G" | 적색 X + "보유: N G -- M G 부족" |
| 레벨 | 녹색 체크 + "현재: Lv.N" | 적색 X + "현재: Lv.N" |

**미충족 시 추가 동작**: "업그레이드 의뢰" 버튼이 회색으로 비활성화된다. 비활성 버튼을 클릭하면 가장 먼저 부족한 항목에 대한 철수의 거절 대사가 출력된다 (섹션 3.6).

### 4.6 업그레이드 성공 연출

업그레이드 의뢰 확인 후와 완성 도구 수령 시 각각 연출이 발생한다.

#### 의뢰 확인 시

| 요소 | 연출 |
|------|------|
| 사운드 | 쇠를 내려놓는 "쿵" 소리 + 동전 소리 |
| 화면 이펙트 | 도구 아이콘이 철수 쪽으로 슬라이드 이동 후 사라짐 |
| UI 변화 | 인벤토리의 해당 도구 슬롯이 자물쇠 아이콘 + "작업 중" 텍스트로 변경 |
| 철수 대사 | 섹션 3.3의 등급별+도구별 대사 순차 출력 |

#### 완성 도구 수령 시

| 요소 | 연출 |
|------|------|
| 사운드 | 모루 위에서 "딩!" 하는 맑은 금속음 + 짧은 팡파르 |
| 화면 이펙트 | 업그레이드된 도구가 화면 중앙에 크게 표시 + 광채 이펙트 (0.8초) |
| UI 변화 | 인벤토리 슬롯에 새 도구 아이콘 + 등급 테두리 표시 |
| 철수 대사 | 섹션 3.4의 도구별+등급별 완료 대사 출력 |
| 특수 이벤트 | 섹션 3.5의 조건 충족 시 추가 특수 대사 출력 |

**Legendary 등급 수령 시 추가 연출**: 도구 표시 시간이 1.2초로 연장되고, 금빛 파티클이 도구 주위를 감싼다. 사운드에 저음 공명이 추가된다.

### 4.7 접근성 고려

| 고려사항 | 대응 방안 |
|---------|-----------|
| 색맹 (적록색맹) | 충족/미충족 표시를 색상만이 아닌 아이콘(체크/X)으로 병행 표시 |
| 색맹 (전색맹) | 등급 구분을 테두리 색상 + 별 아이콘 개수(1/2/3)로 이중 표현 |
| 텍스트 가독성 | 스펙 비교의 증감 표시를 색상 + 방향 기호(▲/▼) + 수치로 3중 전달 |
| 도구 구분 | 각 도구 슬롯에 도구 아이콘 외에 도구명 텍스트 라벨 병행 |
| 상태 인지 | "작업 중" 상태를 색상(주황) + 아이콘(모루) + 텍스트("작업 중")로 3중 표현 |

---

## 5. 영업 조건

### 5.1 운영 시간

영업시간 및 휴무일: (-> see `docs/systems/economy-system.md` 섹션 3.2)

| 항목 | 내용 | 비고 |
|------|------|------|
| 영업시간 | (-> see `docs/systems/economy-system.md` 섹션 3.2) | canonical은 economy-system.md |
| 휴무일 | (-> see `docs/systems/economy-system.md` 섹션 3.2) | canonical은 economy-system.md |
| 폭풍/폭설 시 | 정상 영업 | 대장간은 실내 작업이므로 날씨 영향 없음 |

**설계 의도**: (-> see `docs/content/npcs.md` 섹션 4.2) 대장간은 영업시간이 짧고 휴무가 있어 방문 타이밍 계획이 필요하다. 도구를 맡기는 날과 수령하는 날의 일정을 미리 계획하게 만든다.

### 5.2 영업 개시 조건

| 조건 | 내용 |
|------|------|
| 대장간 건설 필요 여부 | 불필요 (게임 시작 시 마을에 이미 존재) |
| NPC 등장 조건 | 게임 시작 시 즉시 (-> see `docs/content/npcs.md` 섹션 2.1) |
| 상호작용 가능 시점 | 게임 시작 직후부터 대화 가능. 단, 업그레이드 서비스는 레벨 요건 충족 필요 |

### 5.3 레벨 잠금

업그레이드 서비스의 레벨 요건은 (-> see `docs/systems/tool-upgrade.md` 섹션 2.1)을 따른다.

| 서비스 | 접근 가능 시점 |
|--------|---------------|
| 대화, 이야기하기 | 레벨 1부터 (제한 없음) |
| 재료 구매 | 레벨 1부터 (제한 없음) |
| Basic -> Reinforced 업그레이드 | 플레이어 레벨 요건 충족 시 (-> see `docs/systems/tool-upgrade.md` 섹션 2.1) |
| Reinforced -> Legendary 업그레이드 | 플레이어 레벨 요건 충족 시 (-> see `docs/systems/tool-upgrade.md` 섹션 2.1) |

**레벨 미달 시 UX**: 업그레이드 선택 화면에서 해당 도구가 자물쇠 아이콘 + 회색 처리되며, 선택 시 "Lv.N 필요" 툴팁이 표시된다. 철수의 거절 대사("...아직 이르군. 좀 더 경험을 쌓고 와.")가 출력된다.

### 5.4 도구 작업 중 타이머

업그레이드 의뢰 후 소요 시간은 게임 내 일수로 카운트된다.

| 항목 | 규칙 |
|------|------|
| 타이머 시작 | 의뢰 확인 시점의 다음 날 오전 시작 (당일은 카운트하지 않음) |
| 타이머 종료 | 소요 일수 경과 후 해당 날 영업시간 시작 시 완성 |
| 수령 가능 시간 | 완성일의 영업시간 내 |
| 완성 후 미수령 | 페널티 없음. 다음 영업일에 수령 가능. 기간 제한 없음 |
| 영업시간 외 방문 | 철수 부재. 상호작용 불가 |

소요 시간: (-> see `docs/systems/tool-upgrade.md` 섹션 2.1)

---

## 6. 튜닝 파라미터 요약

본 문서에서 관리하는 파라미터. 도구 업그레이드 수치 파라미터는 (-> see `docs/systems/tool-upgrade.md` 섹션 9).

| 파라미터 | 현재 값 | 조정 범위 | 영향 |
|----------|---------|-----------|------|
| `strangerToRegularVisits` | 10 | 5~15 | 단골 전환까지 방문 횟수 |
| `strangerToRegularUpgrades` | 2 | 1~3 | 단골 전환 대체 조건 (업그레이드 횟수) |
| `regularToFriendVisits` | 25 | 15~35 | 친구 전환까지 방문 횟수 |
| `regularToFriendUpgrades` | 4 | 3~6 | 친구 전환 필요 업그레이드 횟수 |
| `seasonalDialogueWeight` | 0.4 | 0.2~0.6 | 계절 대사 출력 가중치 |
| `hintDialogueMinRelation` | `Regular` | Stranger/Regular | 힌트 대사 출력 최소 친밀도 |
| `legendaryPickupEffectDuration` | 1.2초 | 0.8~2.0초 | Legendary 수령 연출 시간 |
| `normalPickupEffectDuration` | 0.8초 | 0.5~1.2초 | 일반 수령 연출 시간 |

---

## Cross-references

| 문서 | 참조 내용 |
|------|-----------|
| `docs/content/npcs.md` 섹션 4 | 철수 기본 캐릭터 설정, 대화 예시, 상호작용 흐름 (본 문서가 확장) |
| `docs/content/npcs.md` 섹션 7 | 대화 시스템 구조 (트리거, 대사 반복 방지 규칙) |
| `docs/content/npcs.md` 섹션 8 | 상점 공통 UI 흐름 (본 문서의 UI 설계가 따르는 패턴) |
| `docs/systems/tool-upgrade.md` 섹션 1 | 업그레이드 등급 체계 (Basic/Reinforced/Legendary) |
| `docs/systems/tool-upgrade.md` 섹션 2 | 업그레이드 비용, 재료, 레벨 요건, 소요 시간 |
| `docs/systems/tool-upgrade.md` 섹션 3 | 도구별 성능 수치 (범위, 에너지, 쿨다운, 특수 효과) |
| `docs/systems/tool-upgrade.md` 섹션 6 | 대장간 NPC 기본 정보, 상호작용 흐름, 상점 품목 |
| `docs/systems/economy-system.md` 섹션 3.2 | 상점 영업시간, 휴무일 (canonical) |
| `docs/systems/inventory-system.md` 섹션 4.3 | 도구 등급 UI 표시 (슬롯 테두리, 아이콘) |
| `docs/balance/progression-curve.md` 섹션 1.3.2 | 레벨 테이블 (업그레이드 해금 레벨) |

---

## Open Questions

1. [OPEN] **NPC 호감도 시스템과의 통합**: 현재 관계 발전 단계(Stranger/Regular/Friend)는 방문 횟수+업그레이드 횟수 기반의 간단한 카운터 시스템이다. `docs/content/npcs.md` Open Question 2에서 NPC 호감도 시스템 도입이 검토 중인데, 도입 시 본 문서의 관계 발전 시스템을 호감도 시스템으로 대체할지, 병행할지 결정 필요.

2. [OPEN] **도구 수령 알림**: 완성된 도구가 있을 때 플레이어에게 알림(HUD 아이콘, 미니맵 표시 등)을 줄 것인지. 현재 설계에서는 대장간 방문 시에만 확인 가능. 알림 추가 시 편의성은 높아지지만, 방문 계획의 전략적 요소가 줄어든다.

3. [OPEN] **동시 업그레이드**: 현재 설계에서는 한 번에 하나의 도구만 맡길 수 있다 (섹션 3.6, "이미 작업 중인 도구가 있어"). 여러 도구 동시 의뢰를 허용할지 결정 필요. 허용 시 도구 2~3개가 동시에 사용 불가해지므로 리스크가 커지지만, 숙련 플레이어에게는 효율적.

4. [RESOLVED] **친구 단계 보상 추가**: 재료 구매 10% 할인(`discountRate = 0.1`)을 Friend 단계 보상으로 채택하여 확정. 섹션 2.5 참조.

---

## Risks

1. [RISK] **대사 반복 피로도**: 범용 대사 5종 + 계절 대사 2종 = 7종 풀은 장기 플레이 시 반복감이 느껴질 수 있다. 친밀도 단계별 추가 대사로 풀을 확장하되, 전체 대사 제작 비용과의 균형 필요.

2. [RISK] **UI 정보 과부하**: 스펙 비교 패널에 표시되는 정보(범위, 에너지, 쿨다운, 특수효과, 재료, 골드, 레벨)가 많아 첫 방문 시 부담될 수 있다. 단계적 정보 공개(첫 방문 시 간략 표시, 2회차부터 전체 표시)를 고려할 수 있으나 복잡도 증가.

3. [RISK] **관계 발전 인지 부족**: 친밀도 단계 전환이 명시적 UI 알림 없이 대사 변화로만 드러나므로, 플레이어가 관계 발전을 인지하지 못할 수 있다. 단계 전환 시 짧은 알림("철수와 더 가까워진 것 같다")을 표시할지 검토 필요.

4. [RISK] **npcs.md와의 대사 중복**: 본 문서의 대사 스크립트와 `docs/content/npcs.md` 섹션 4.4의 기존 대사 예시가 일부 중복된다. 본 문서가 대장간 NPC 대사의 canonical이며, npcs.md 섹션 4.4는 요약/예시 수준으로 유지하거나 본 문서 참조로 대체해야 한다.

---

*이 문서는 대장간 NPC "철수"의 캐릭터 심화, 전체 대화 스크립트, 업그레이드 UI/UX의 canonical 문서이다. 도구 업그레이드의 수치/비용/재료는 `docs/systems/tool-upgrade.md`를 정본으로 한다.*

---

# 목공소 장인 NPC 상세 문서 (Carpenter NPC Detail Specification)

> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: CON-008b | Phase 1

---

## 1. Context

이 문서는 목공소 장인 NPC "목이(Moki)"의 캐릭터 심화 설계, 전체 대화 스크립트, 건설 의뢰 인터페이스 UX, 관계 발전 단계를 상세히 기술한다. `docs/content/npcs.md` 섹션 5에 정의된 기본 설정을 확장하며, 시설 건설 시스템(`docs/content/facilities.md`)과 농장 확장 시스템(`docs/systems/economy-system.md`)과 완전히 연동된다.

**설계 목표**: 목이는 플레이어의 농장 성장을 물리적으로 구현하는 NPC로, 느긋하고 따뜻한 성격을 통해 "마을의 어른"이라는 느낌을 준다. 대화가 단순한 건설 메뉴 진입점이 아니라, 시설 건설 타이밍과 우선순위에 대한 간접 가이드 역할을 수행하고, 농장 확장의 성취감을 캐릭터 반응으로 증폭시키도록 설계한다.

### 1.1 본 문서가 canonical인 데이터

- 목이 캐릭터 심화 설정 (배경 스토리, 성격 디테일, 관계 발전 단계)
- 목공소 장인 전체 대화 스크립트 (상황별, 계절별, 친밀도별, 특수 이벤트별)
- 건설 의뢰 인터페이스 UX 세부 설계
- 목이의 관계 발전 단계 임계값 및 단계별 보상

### 1.2 본 문서가 canonical이 아닌 데이터 (참조만)

| 데이터 종류 | 참조처 |
|------------|--------|
| NPC 기본 정보 (이름, 나이, 외형 요약) | `docs/content/npcs.md` 섹션 5.1 |
| 영업시간, 휴무일 | `docs/systems/economy-system.md` 섹션 3.2 |
| 시설 건설 비용, 해금 레벨, 건설 시간 | `docs/content/facilities.md` 섹션 2 |
| 시설 목록 (물탱크, 창고, 온실, 가공소) | `docs/design.md` 섹션 4.6 |
| 농장 확장 단계, 비용 | `docs/systems/economy-system.md` 섹션 3.3 |
| 가공소 슬롯 확장 비용 | `docs/systems/economy-system.md` 섹션 2.5 |
| 상점 공통 UI 흐름 | `docs/content/npcs.md` 섹션 8 |
| NPC 대화 시스템 구조 (트리거, 우선순위) | `docs/content/npcs.md` 섹션 7 |
| 레벨별 해금 콘텐츠 | `docs/balance/progression-curve.md` 섹션 1.3.2 |

---

## 2. 캐릭터 설계

### 2.1 기본 정보

| 항목 | 내용 |
|------|------|
| 이름 | 목이 (Moki) |
| 영문 ID | `npc_moki` |
| 나이 | 50대 초반 |
| 성별 | 남성 |
| 역할 | 목공소 장인 (시설 건설, 농장 확장, 가공소 슬롯 확장) |
| 위치 | 마을 북쪽, 목공소 건물 내부 |

기본 캐릭터 설정은 (-> see `docs/content/npcs.md` 섹션 5.1)을 따른다.

### 2.2 외형 상세

로우폴리 3D 스타일에 맞춘 캐릭터 디자인.

| 요소 | 묘사 |
|------|------|
| 체형 | 넓은 어깨, 건장한 체형. 약간 배가 나온 중년 체형 (안정감 있는 인상) |
| 피부 | 적갈색으로 그을린 피부. 손등에 작업 흔적 (로우폴리에서 약간 거친 텍스처로 표현) |
| 머리 | 짧은 회색 머리, 뒤로 넘김. 짧은 회색 수염 |
| 복장 | 갈색 작업복 (내구성 있는 두꺼운 면직물), 허리에 연장 벨트 (망치, 자, 줄자 등) |
| 특징적 요소 | 왼쪽 귀에 연필 끼움 (설계도 작성용). 항상 어딘가에 나무 톱밥이 묻어 있다 |
| 대기 애니메이션 | 작업대 앞에서 나무를 대패질하거나, 설계도를 펼쳐 들여다보는 동작 반복 |
| 보조 요소 | 목공소 내부에 다양한 목재 샘플, 미니어처 건물 모형이 진열되어 있다 |

**로우폴리 표현 가이드**: 넓은 어깨와 연장 벨트의 실루엣이 "건축가"를 즉시 인식시킨다. 갈색 톤의 작업복이 목공소의 나무 색상과 조화를 이루며, 귀에 꽂힌 연필은 디테일 포인트로 캐릭터를 기억하게 만든다.

### 2.3 성격 상세

| 측면 | 설명 |
|------|------|
| 말투 | 느긋한 반말 + 존댓말 혼용("~하시게", "~하지", "~야"). 어르신 특유의 여유 있는 화법. 웃음이 많다("허허", "호호") |
| 건축에 대한 태도 | 건물 하나하나를 "자식"처럼 여긴다. 자기가 지은 건물의 안부를 물을 정도 |
| 플레이어에 대한 태도 | 처음부터 친근하게 대한다. 농장 발전을 보면서 자기 일처럼 기뻐한다 |
| 감정 표현 | 솔직하고 과장된 감정 표현. "오!" "허허!" "대단하군!" 같은 감탄사를 자주 사용 |
| 숨은 면모 | 젊은 시절 도시에서 큰 건물을 짓고 싶었지만, 아버지의 병환으로 마을로 돌아왔다. 그 결정을 후회하지 않지만, 가끔 큰 건물에 대한 동경이 나타난다 |

### 2.4 배경 스토리

목이는 이 마을에서 대대로 이어온 목수 집안의 3대손이다. 할아버지가 마을의 첫 번째 집을 지었고, 아버지가 마을 전체를 확장했으며, 목이가 현재 마을의 모든 건물 유지보수와 신규 건축을 담당하고 있다.

젊은 시절 도시의 건축 사무소에서 일한 적이 있다. 대형 건물의 기초를 설계하는 일을 했으나, 아버지가 병환으로 쓰러지자 마을로 돌아왔다. 돌아온 후에는 마을 건물을 하나하나 직접 손보며, 작은 건물에도 최선을 다하는 것이 자신의 길이라고 받아들였다.

하나의 잡화 상점 진열대도, 철수의 대장간 모루 받침대도 목이가 만들었다. 마을의 모든 나무 구조물에 목이의 손길이 닿아 있다. 새 농부의 농장에 시설을 하나씩 지어 주는 것이 요즘 가장 보람 있는 일이다.

**게임 내 노출**: 배경 스토리는 친밀도 단계에 따라 단편적으로 드러난다. 초반에는 마을 건물에 대한 자랑만 하고, 관계가 발전하면 도시 시절 이야기, 아버지 이야기가 나온다.

### 2.5 관계 발전 단계

플레이어와 목이의 관계는 4단계로 발전한다. 단계 전환은 친밀도 포인트 누적(시설 건설 완료 +5, 농장 확장 완료 +3, 일일 대화 +1)에 기반한다.

| 단계 | 영문 키 | 친밀도 임계값 | 관계 묘사 |
|------|---------|-------------|-----------|
| 새 손님 | `Stranger` | 0 (초기) | 친절하지만 일반적인 안내. 기본 서비스 대사 |
| 단골 | `Acquaintance` | 10 | 이름을 부르며 농장 상황에 관심을 가진다. 건설 우선순위 조언 시작 |
| 건축 동료 | `Regular` | 25 | 시설 건설에 대한 깊은 조언을 제공. 도시 시절 이야기를 꺼내기 시작 |
| 마을 가족 | `Friend` | 50 | 아버지 이야기를 꺼낸다. 농장에 대한 진심 어린 격려. **건설 비용 10% 할인 혜택** 제공 |

**임계값 canonical**: 이 테이블의 친밀도 임계값(`[0, 10, 25, 50]`)이 canonical이다. NPC 아키텍처의 `affinityThresholds`는 이 테이블을 참조한다.

**설계 의도**: 시설 건설 빈도가 도구 업그레이드나 씨앗 구매보다 낮으므로, 건설 완료 시 +5포인트로 높은 보상을 준다. 농장 확장도 +3으로 보정. 이를 통해 건설 의뢰가 적더라도 관계 발전이 가능하게 한다. Friend 할인(10%)은 고비용 건설에서 상당한 절감 효과를 준다.

---

## 3. 대화 시스템 설계

### 3.1 최초 만남 대화

게임 시작 후 목이에게 처음 말을 걸 때 1회만 재생된다.

```
[목이] "오! 새 농부님이시구먼!
        이 마을의 건물은 전부 내가 지었지. 허허.
        농장에 시설이 필요하면 언제든 찾아오시게.
        처음엔 물탱크부터 시작하는 게 좋을 거야."
```

**설계 의도**: 4문장으로 (1) 환영, (2) 자기소개, (3) 서비스 안내, (4) 초기 시설 가이드를 완료한다. 물탱크를 추천함으로써 플레이어에게 첫 건설의 방향을 제시한다(물탱크는 가장 저렴하고 즉시 효용이 있는 시설).

### 3.2 일반 방문 대화

재방문 시 랜덤으로 목이가 출력된다. 동일 대사 연속 재생 방지 규칙은 (-> see `docs/content/npcs.md` 섹션 7.4)을 따른다.

#### 범용 대사 (계절 무관, 5종)

| ID | 대사 |
|----|------|
| `moki_general_01` | "어서 오시게! 오늘은 뭘 지어 볼까?" |
| `moki_general_02` | "농장이 점점 커지고 있군! 좋아, 좋아." |
| `moki_general_03` | "건축 상담이면 내가 전문이지. 편하게 물어보시게." |
| `moki_general_04` | "나무 냄새가 좋지 않나? 이 목공소에서 50년째 이 냄새를 맡고 있어. 허허." |
| `moki_general_05` | "오, 왔구먼! 오늘도 농장에 뭔가 지을 계획인가?" |

#### 계절별 대사 (각 계절 3종, 총 12종)

| 계절 | ID | 대사 |
|------|-----|------|
| 봄 | `moki_spring_01` | "봄이야! 농장 확장하기 딱 좋은 계절이지. 물탱크 어때?" |
| 봄 | `moki_spring_02` | "봄비가 오면 나무가 잘 불어나. 목재 상태가 최고야." |
| 봄 | `moki_spring_03` | "새 계절이니까 농장 계획을 세워 봐. 어디에 뭘 지을지 생각해 두면 좋아." |
| 여름 | `moki_summer_01` | "여름에 창고를 지어 두면 가을 수확 때 든든하지." |
| 여름 | `moki_summer_02` | "더워서 야외 작업이 힘들긴 한데... 건물은 지어야지! 허허." |
| 여름 | `moki_summer_03` | "여름 목재는 단단해. 이 시기에 지은 건물이 오래 가지." |
| 가을 | `moki_autumn_01` | "가을 수확이 끝나면 온실을 생각해 봐. 겨울이 코앞이야." |
| 가을 | `moki_autumn_02` | "단풍이 참 예쁘지? 가끔 단풍나무로 뭔가 만들고 싶어져." |
| 가을 | `moki_autumn_03` | "가을에 시설 정비를 미리 해 두면 겨울에 편해. 점검할 거 있으면 말해." |
| 겨울 | `moki_winter_01` | "겨울이라 야외 공사가 힘들지만... 실내 시설은 문제없어!" |
| 겨울 | `moki_winter_02` | "눈이 많이 오면 지붕이 걱정이야. 내 건물은 튼튼하니까 괜찮지만." |
| 겨울 | `moki_winter_03` | "겨울에는 새 건물 설계를 해. 봄에 바로 지을 수 있게 미리 준비하는 거지." |

**대사 출력 규칙**: 현재 계절에 해당하는 계절 대사 3종 + 범용 대사 5종 = 총 8종 풀에서 랜덤 선택. 가중치는 계절 대사 40%, 범용 대사 60%.

### 3.3 건설 관련 대사

플레이어가 건설 서비스를 이용할 때 출력되는 대사.

#### 건설 의뢰

| 상황 | 대사 |
|------|------|
| 시설 건설 의뢰 수락 | "좋아! {시설이름} 건설 시작하겠네. {건설시간}일 뒤에 완성될 거야. 기대해도 좋아!" |
| 농장 확장 의뢰 수락 | "농장을 넓히려고? 좋지, 좋아! {건설시간}일이면 새 땅을 쓸 수 있을 거야." |
| 가공소 슬롯 확장 수락 | "슬롯을 늘리려고? 가공소가 바빠지겠군. 금방 해 줄게." |

#### 건설 완료

| 상황 | 대사 |
|------|------|
| 시설 건설 완료 (일반) | "다 지었네! {시설이름}이(가) 완성됐어. 가서 확인해 보시게. 마음에 들 거야!" |
| 물탱크 완료 | "물탱크 완성! 이제 물 걱정은 덜었지. 물뿌리개 리필이 편해질 거야." |
| 창고 완료 | "창고가 생겼으니 물건 보관이 한결 나아질 거야. 넉넉한 공간은 농부의 행복이지!" |
| 온실 완료 | "온실 완성이야! 겨울에도 농사를 할 수 있게 됐어. 대단한 농장이 되어 가는군!" |
| 가공소 완료 | "가공소 완성! 이제 작물을 가공해서 더 비싸게 팔 수 있어. 하나한테 가서 자랑해 봐!" |

#### 거절/불가

| 상황 | 대사 |
|------|------|
| 해금 레벨 미달 | "아직 이 시설을 짓기엔 이르네. 좀 더 경험을 쌓으면 내가 도와주지." |
| 골드 부족 | "음... 비용이 좀 모자라는군. {필요골드}G가 더 있어야 해. 좀 더 모아서 오시게." |
| 이미 건설 중 | "지금 {건설중시설}을(를) 짓고 있어서, 끝나고 다시 오시게. 동시에 두 개는 힘들어." |
| 이미 해당 시설 보유 | "이미 있는 건물인데? 다른 거 지을까?" |
| 농장 확장 최대 달성 | "농장이 이미 최대야! 더 넓히려면... 글쎄, 방법이 없네. 허허." |

### 3.4 특수 대화

특정 조건에서 1회만 트리거되는 대화.

| 조건 | 트리거 | 대사 |
|------|--------|------|
| 첫 시설 건설 완료 | `flag:firstBuildingComplete` | "첫 번째 건물이야! 농장이 점점 그럴듯해지는군. 내가 지은 건물 중에서도 특별하지. 허허." |
| 모든 기본 시설 건설 완료 | `flag:allBasicBuildings` | "허허, 시설을 전부 지었군! 이 정도면 마을 최고의 농장이야. 내 솜씨도 한몫했지?" |
| 첫 농장 확장 완료 | `flag:firstExpansion` | "농장이 넓어졌어! 새 땅에 뭘 할지 벌써 기대되는군." |
| 최대 농장 확장 달성 | `flag:maxExpansion` | "농장을 최대로 넓혔구먼! 내가 이 마을에서 이렇게 큰 농장을 본 건 처음이야. 대단해!" |
| 온실 미보유 + 겨울 진입 | `season:winter AND !flag:hasGreenhouse` | "겨울에 할 일이 없지? 온실을 지으면 겨울에도 작물을 키울 수 있어. 한번 생각해 봐." |
| 가공소 미보유 + 레벨 7 도달 | `minLevel:7 AND !flag:hasProcessor` | "이제 가공소를 지을 때가 됐어! 작물을 가공하면 수익이 크게 올라간다네. 하나가 그러더라고." |
| 창고 미보유 + 인벤토리 가득 | `!flag:hasWarehouse AND flag:inventoryFull` | "짐이 많아 보이는데... 창고를 지으면 보관 공간이 넓어져. 생각해 봐." |

### 3.5 시설 건설 힌트 대사

목이의 고유 역할은 "건설 타이밍 가이드"이다. 플레이어의 상황에 맞춰 적절한 시설을 추천한다.

| 힌트 대사 | 조건 | 친밀도 조건 |
|-----------|------|------------|
| "물탱크가 없으면 물 주는 게 고생이지. 처음엔 물탱크부터 시작하는 게 좋아." | 물탱크 미보유 + 레벨 2 이상 | 단골(`Acquaintance`) 이상 |
| "수확량이 늘어나면 창고가 필요해져. 가을 전에 지어 두면 좋지." | 창고 미보유 + 여름 | 단골(`Acquaintance`) 이상 |
| "겨울이 오기 전에 온실을 지으면 겨울에도 농사를 할 수 있어. 미리 준비해." | 온실 미보유 + 가을 | 건축 동료(`Regular`) 이상 |
| "가공소가 있으면 작물을 가공해서 몇 배로 팔 수 있어. 레벨이 되면 꼭 지어." | 가공소 미보유 + 레벨 6 이상 | 건축 동료(`Regular`) 이상 |
| "농장이 좁아 보이는데, 확장하면 더 많은 작물을 심을 수 있어." | 확장 가능 + 현재 타일 80% 이상 사용 | 단골(`Acquaintance`) 이상 |

**쿨다운**: 힌트 대사는 같은 힌트가 3일에 1번까지만 반복 (-> see `docs/content/npcs.md` 섹션 7.4).

### 3.6 친밀도 단계별 대화

관계 발전 단계(섹션 2.5)에 따라 추가되는 대사.

#### 새 손님 (Stranger)

기본 범용/계절 대사만 출력. 추가 대사 없음.

#### 단골 (Acquaintance)

단계 전환 시 1회성 특수 대사:

```
[목이] "자주 오는구먼, {플레이어명}!
        농장이 점점 그럴듯해지고 있어. 내가 지은 건물이 잘 쓰이고 있으니 기분 좋지.
        앞으로 뭘 지을지 고민되면 언제든 상담해. 전문가 조언은 무료야! 허허."
```

기존 대사 풀에 다음이 추가된다.

| ID | 대사 |
|----|------|
| `moki_acquaint_01` | "{플레이어명}, 오늘도 건축 상담이야? 좋지, 좋아!" |
| `moki_acquaint_02` | "농장 돌아보다가 내가 지은 건물 보면 기분이 좋아지지 않아? 허허." |
| `moki_acquaint_03` | "철수 녀석이 도구를 만들면, 내가 건물을 짓고, 하나가 씨앗을 팔고... 우리 셋이 이 마을의 기둥이야." |

#### 건축 동료 (Regular)

단계 전환 시 1회성 특수 대사:

```
[목이] "{플레이어명}, 네 농장을 보면 내가 젊었을 때 생각이 나.
        사실 나도 한때 도시에서 큰 건물을 짓고 싶었거든.
        고층 빌딩, 미술관, 경기장... 그런 걸 설계하는 게 꿈이었지.
        근데 여기서 네 농장 시설을 짓는 것도 나름 보람 있더라고. 허허."
```

기존 대사 풀에 다음이 추가된다.

| ID | 대사 |
|----|------|
| `moki_regular_01` | "도시에서 일할 때 배운 기술이 여기서도 쓸모가 있어. 기초 공사는 확실하게 하는 게 내 신조야." |
| `moki_regular_02` | "이 목공소 작업대는 아버지가 쓰시던 거야. 50년 넘었는데 아직도 튼튼해. 내가 만든 것도 이만큼 갔으면 좋겠어." |
| `moki_regular_03` | "가끔 하나가 차를 가져와. 같이 마시면서 마을 이야기를 하지. 그게 내 쉬는 시간이야." |

#### 마을 가족 (Friend)

단계 전환 시 1회성 특수 대사:

```
[목이] "{플레이어명}... 네가 이 마을에 온 건 정말 다행이야.
        사실 아버지가 돌아가시고 나서, 마을이 점점 조용해지더라고.
        젊은 사람들이 도시로 떠나고, 새로 오는 사람도 없고...
        근데 네가 와서 농장을 일구는 걸 보면, 이 마을이 아직 살아 있다는 느낌이 들어.
        앞으로 건설 비용 좀 깎아 줄게. 내 마음이니까 사양하지 마!"
```

기존 대사 풀에 다음이 추가된다.

| ID | 대사 |
|----|------|
| `moki_friend_01` | "아버지가 살아 계셨으면 네 농장을 보고 뭐라 하셨을까. '좋은 농장이로구나, 목이야' 하셨을 거야. 분명." |
| `moki_friend_02` | "도시에서의 꿈을 포기한 건 아니야. 여기서 새로운 꿈을 찾은 거지. 이 마을의 모든 건물이 내 작품이야." |
| `moki_friend_03` | "겨울 밤에 목공소에 혼자 있으면 좀 쓸쓸해. 그럴 때 농장 불빛이 보이면 마음이 따뜻해져." |
| `moki_friend_04` | "철수랑 하나, 그리고 너. 이 마을에 이만한 이웃이 있다는 게 행복이야. 허허." |

### 3.7 날씨별 특수 대사

특정 날씨 조건에서 추가 출력되는 대사.

| 날씨 | 대사 |
|------|------|
| 비 (Rain) | "비 오는 날에도 왔구먼. 비가 오면 나무가 잘 젖어서 작업하기 좋지는 않아. 실내에서 설계나 해야겠어." |
| 비 (Rain) | "빗소리 들으면서 나무를 깎으면 기분이 묘하게 좋아. 허허." |
| 폭풍 (Storm) | (임시 마감 -- 영업하지 않음) |
| 폭설 (Blizzard) | (임시 마감 -- 영업하지 않음) |
| 맑음 | "오늘 날씨 좋군! 야외 공사하기 딱이야." |

### 3.8 건설 의뢰 인터페이스 UX 세부

기본 UI 흐름은 (-> see `docs/content/npcs.md` 섹션 8.2)을 따른다. 목이 상점 고유 요소만 기술한다.

#### 건설 메뉴 레이아웃

```
┌─────────────────────────────────────────────┐
│  [목이의 목공소]                [소지금: XXG] │
├─────────────────────────────────────────────┤
│  [시설 건설] [농장 확장] [슬롯 확장]          │
├────────────────────────┼────────────────────┤
│  ┌──────────────────┐  │  시설 상세         │
│  │ 물탱크     [건설] │  │                   │
│  │ 창고       [건설] │  │  [온실]           │
│  │ 온실       [건설] │  │  "겨울에도 작물을  │
│  │ 가공소   [잠금]   │  │   키울 수 있는     │
│  │                  │  │   시설."           │
│  │                  │  │                   │
│  │                  │  │  비용: (→ see      │
│  │                  │  │  facilities.md)    │
│  │                  │  │  건설 시간: X일    │
│  │                  │  │  [건설 의뢰]       │
│  └──────────────────┘  │                   │
├─────────────────────────────────────────────┤
│  [목이] "온실은 겨울 대비의 핵심이지!"        │
└─────────────────────────────────────────────┘
```

#### 목이 고유 UX 요소

| 요소 | 설명 |
|------|------|
| 건설 진행 표시 | 건설 중인 시설에 진행률 바 표시 (잔여 일수 표기) |
| 건설 미리보기 | 시설 선택 시 농장 맵에서 건설 위치 프리뷰 (반투명 오버레이) |
| 잠금 표시 | 해금 레벨 미달 시설은 자물쇠 아이콘 + "레벨 X 필요" 텍스트 |
| 할인 표시 | `Friend` 단계 시 건설 비용에 취소선 + 할인가 표시 |
| 추천 배지 | 현재 농장 상황에 맞는 추천 시설에 별 아이콘 표시 (친밀도 `Acquaintance` 이상) |
| 대화 패널 | 화면 하단에 목이의 짧은 코멘트가 실시간 표시 |

---

## 4. 계절별 이벤트 연동

### 4.1 겨울 건설 제한

겨울에는 야외 건설에 제한이 있다.

| 건설 유형 | 겨울 가능 여부 | 비고 |
|-----------|---------------|------|
| 실내 시설 (창고, 가공소 슬롯) | 가능 | 정상 건설 |
| 야외 시설 (온실, 물탱크) | 가능 (건설 시간 +1일) | 겨울 패널티 |
| 농장 확장 | 불가 | 땅이 얼어서 작업 불가 |

[OPEN] 겨울 농장 확장 불가 규칙의 적절성. 겨울에 건설할 콘텐츠가 제한되는 문제가 있으나, 계절별 전략적 차별화에 기여한다.

#### 겨울 건설 대사

```
[목이] (야외 건설 시) "겨울이라 좀 더 걸릴 거야. 땅이 얼어서 기초 작업이 힘들거든. 하루만 더 기다려 주게."
[목이] (농장 확장 시도) "미안하네, 겨울엔 땅이 꽁꽁 얼어서 확장 공사를 못 해. 봄에 다시 오시게."
```

---

## 5. 튜닝 파라미터

| 파라미터 | 영문 키 | 현재 값 | 조정 범위 | 영향 |
|----------|---------|---------|-----------|------|
| 친밀도 임계값 | `mokiAffinityThresholds` | [0, 10, 25, 50] | - | 관계 발전 속도 |
| 시설 건설 친밀도 증가 | `mokiBuildAffinityGain` | 5 | 3~7 | 건설의 관계 기여도 |
| 농장 확장 친밀도 증가 | `mokiExpandAffinityGain` | 3 | 2~5 | 확장의 관계 기여도 |
| 일일 대화 친밀도 증가 | `mokiChatAffinityGain` | 1 | 0~1 | 방문만으로의 관계 기여도 |
| Friend 할인율 | `mokiFriendDiscount` | 0.10 | 0.05~0.15 | 건설 비용 할인 강도 |
| 겨울 건설 추가 일수 | `mokiWinterBuildPenalty` | 1 | 0~2 | 겨울 건설 패널티 |
| 계절 대사 풀 크기 | `mokiSeasonDialogueCount` | 3 | 2~5 | 대사 다양성 |

---

## Cross-references

| 문서 | 참조 내용 |
|------|-----------|
| `docs/content/npcs.md` 섹션 5 | 목이 기본 캐릭터 설정, 서비스 목록, 기본 대화 예시 |
| `docs/content/npcs.md` 섹션 7 | NPC 대화 시스템 공통 구조 (트리거, 우선순위, 반복 방지) |
| `docs/content/npcs.md` 섹션 8 | 상점 공통 UI 흐름 |
| `docs/content/facilities.md` 섹션 2 | 시설 건설 프로세스, 건설 시간, 비용 |
| `docs/design.md` 섹션 4.6 | 시설 목록, 해금 레벨 |
| `docs/systems/economy-system.md` 섹션 2.5 | 가공소 슬롯 확장 |
| `docs/systems/economy-system.md` 섹션 3.2 | 영업시간, 휴무일 |
| `docs/systems/economy-system.md` 섹션 3.3 | 목공소 인벤토리, 농장 확장 비용 |
| `docs/balance/progression-curve.md` 섹션 1.3.2 | 레벨별 해금 콘텐츠 |
| `docs/content/blacksmith-npc.md` | 철수 NPC 상세 (관계 발전 구조 동일 패턴) |
| `docs/content/merchant-npc.md` | 하나 NPC 상세 (관계 발전 구조 동일 패턴) |
| `docs/systems/npc-shop-architecture.md` | NPC/상점 시스템 기술 아키텍처 |

---

## Open Questions

1. [OPEN] **겨울 농장 확장 불가**: 겨울에 농장 확장을 완전히 차단하는 것이 적절한지. 겨울 콘텐츠 부족 문제와 연결. 대안: 겨울에도 확장 가능하되 비용 1.5배 + 건설 시간 2배 패널티.

2. [OPEN] **동시 건설 제한**: 현재 1건만 가능하도록 설계. 후반 플레이어의 대기 시간 불만 우려. `docs/content/npcs.md` Open Question 5와 동일.

3. [OPEN] **Friend 할인이 농장 확장에도 적용되는지**: 건설 비용 10% 할인이 시설 건설에만 적용되는지, 농장 확장 비용에도 적용되는지 결정 필요. 농장 확장은 비용이 높으므로 할인 시 경제 밸런스 영향이 크다.

4. [OPEN] **NPC 호감도 시스템 통합**: `docs/content/npcs.md` Open Question 2와 동일. 전체 NPC 호감도 시스템이 도입될 경우, 목이의 개별 친밀도 시스템을 통합하거나 대체할 수 있다.

5. [RISK] **건설 미리보기 구현 복잡도**: 건설 위치 프리뷰를 구현하려면 타일맵 위에 반투명 오버레이를 렌더링해야 한다. Phase 2 구현 시 기술적 난이도가 높을 수 있다.

---

# 작물 콘텐츠 상세 문서 (Crop Content Specification)

> 작성: Claude Code (Opus) | 2026-04-06  
> 문서 ID: CON-001

---

## 1. Context

이 문서는 SeedMind에 등장하는 모든 작물의 콘텐츠 상세 정보를 기술한다. 게임 내 표시될 설명 텍스트, 시각적 묘사, 특수 메카닉 세부 사항, 계절별 분류, 수확물 아이템 데이터를 포함한다.

### 1.1 본 문서가 canonical인 데이터

- 작물별 로어/설명 텍스트 (게임 내 표시 문구)
- 작물별 시각 묘사 힌트 (로우폴리 모델링 참고)
- 특수 메카닉 세부 파라미터 (씨앗 드롭 확률, 다중 수확 규칙 상세)
- 겨울/온실 전용 작물 신규 데이터 (`[NEW]` 태그 항목)
- 작물 수확물 ItemID 매핑

### 1.2 본 문서가 canonical이 아닌 데이터 (참조만)

| 데이터 종류 | 참조처 |
|------------|--------|
| 씨앗 가격, 판매가, 성장일수, 해금 조건 | `docs/design.md` 섹션 4.2 |
| 성장 단계 정의, 성장 공식, 품질 공식 | `docs/systems/crop-growth.md` |
| ROI, 일일 수익률, 밸런스 분석 | `docs/balance/crop-economy.md` |
| 타일 상태, 도구 상호작용, 비료 효과 | `docs/systems/farming-system.md` |
| 아이템 카테고리, 스택 규칙 | `docs/systems/inventory-system.md` |

---

## 2. 계절별 작물 분류 (Seasonal Classification)

### 2.1 계절별 재배 가능 작물

| 계절 | 재배 가능 작물 | 비고 |
|------|--------------|------|
| 봄 (Spring) | 감자, 당근, 딸기 | 당근은 가을에도 재배 가능 |
| 여름 (Summer) | 토마토, 옥수수, 해바라기, 수박 | 여름 보너스 x1.05 적용 (-> see `docs/systems/crop-growth.md` 섹션 2.4) |
| 가을 (Fall) | 당근, 호박 | 당근은 부 계절 (보정 x1.0) |
| 겨울 (Winter) | 야외 재배 불가 | 온실에서만 재배 가능 |

### 2.2 겨울/온실 재배

겨울에는 야외 경작이 완전히 불가능하다. 온실(레벨 5 해금, -> see `docs/design.md` 섹션 4.6)에서만 작물을 재배할 수 있다.

**온실에서 재배 가능한 작물**:
- 기존 8종 전체 (계절 보정 x1.0 고정, -> see `docs/systems/crop-growth.md` 섹션 3.3)
- 겨울 전용 작물 3종: 겨울무, 표고버섯, 시금치 (본 문서 섹션 3에서 신규 정의)

**겨울 전용 작물 특징**: 겨울 전용 작물은 온실 내에서만 재배 가능하며, 다른 계절의 야외에서도 재배할 수 없다. 이는 온실의 가치를 높이고 겨울 콘텐츠 공백을 채우기 위한 설계다.

### 2.3 계절 전환 시 작물 처리

- 계절 전환(매 28일째 24:00)이 발생하면, 새 계절에서 재배 불가능한 모든 야외 작물은 **즉시 Withered** 상태로 전환 (-> see `docs/systems/crop-growth.md` 섹션 3.2)
- 유예 기간 없음
- 사전 경고: 25일째부터 화면 상단 경고 배너 표시
- 다중 수확 작물(딸기 등)도 동일하게 고사

---

## 3. 전체 작물 카탈로그 (Crop Catalog)

### 3.1 감자 (Potato)

| 항목 | 내용 |
|------|------|
| **영문 ID** | `crop_potato` |
| **설명** (게임 내) | "땅 속에서 묵직하게 자라는 든든한 작물. 초보 농부의 첫 번째 친구." |
| **재배 계절** | 봄 |
| **기본 수치** | (-> see `docs/design.md` 섹션 4.2) |
| **특수 메카닉** | 없음 (단일 수확, 기본 작물) |
| **점보 성장** | 불가 |
| **Harvestable 시각 묘사** | 짙은 녹색 잎이 무성하게 솟아 있고, 흙 사이로 갈색 감자가 일부 노출. 높이 약 0.3m. |
| **수확물 설명** | 울퉁불퉁한 갈색 표면의 둥근 감자. 아이콘: 타원형 갈색 덩어리, 흙이 묻은 질감. |

### 3.2 당근 (Carrot)

| 항목 | 내용 |
|------|------|
| **영문 ID** | `crop_carrot` |
| **설명** (게임 내) | "봄과 가을, 두 계절을 아우르는 주황빛 보석. 뽑는 순간이 가장 짜릿하다." |
| **재배 계절** | 봄 (주 계절), 가을 (부 계절) |
| **기본 수치** | (-> see `docs/design.md` 섹션 4.2) |
| **특수 메카닉** | 없음 (단일 수확). 두 계절 재배 가능이 유일한 특성. |
| **점보 성장** | 불가 |
| **Harvestable 시각 묘사** | 풍성한 녹색 잎이 부채꼴로 솟아 있고, 주황색 당근 윗부분이 흙 위로 살짝 보임. 높이 약 0.35m. |
| **수확물 설명** | 선명한 주황색 원뿔형 당근. 아이콘: 주황색 원뿔 + 녹색 잎사귀 2장. |

### 3.3 토마토 (Tomato)

| 항목 | 내용 |
|------|------|
| **영문 ID** | `crop_tomato` |
| **설명** (게임 내) | "여름 햇살을 머금고 빨갛게 익어가는 탐스러운 열매. 지지대가 필요할 만큼 잘 자란다." |
| **재배 계절** | 여름 |
| **기본 수치** | (-> see `docs/design.md` 섹션 4.2) |
| **특수 메카닉** | 없음 (단일 수확) |
| **점보 성장** | 불가 |
| **Harvestable 시각 묘사** | 나무 지지대에 줄기가 감기고, 빨간 토마토 2~3개가 매달림. 줄기: 녹색, 열매: 빨강. 높이 약 0.6m. |
| **수확물 설명** | 탐스럽고 윤기 나는 빨간 토마토. 아이콘: 둥근 빨간 구체 + 꼭지 녹색 잎. |

### 3.4 옥수수 (Corn)

| 항목 | 내용 |
|------|------|
| **영문 ID** | `crop_corn` |
| **설명** (게임 내) | "하늘을 향해 곧게 뻗는 여름의 상징. 노란 이삭이 영글면 수확의 기쁨이 찾아온다." |
| **재배 계절** | 여름 |
| **기본 수치** | (-> see `docs/design.md` 섹션 4.2) |
| **특수 메카닉** | 씨앗 드롭 후보 (아래 섹션 4.2 참조) |
| **점보 성장** | 불가 |
| **Harvestable 시각 묘사** | 키 큰 녹색 줄기(높이 약 0.8m) 끝에 노란 이삭 1~2개. 넓은 잎이 줄기를 감싸듯 펼쳐짐. |
| **수확물 설명** | 껍질을 벗기면 드러나는 노란 옥수수 알갱이. 아이콘: 노란 이삭 + 연녹색 껍질 일부. |

### 3.5 딸기 (Strawberry)

| 항목 | 내용 |
|------|------|
| **영문 ID** | `crop_strawberry` |
| **설명** (게임 내) | "한 번 심으면 봄 내내 달콤한 열매를 선사하는 고마운 작물. 부지런히 물을 주자." |
| **재배 계절** | 봄 |
| **기본 수치** | (-> see `docs/design.md` 섹션 4.2) |
| **특수 메카닉** | **다중 수확** -- 첫 수확 후 재수확 간격으로 계속 열매를 맺음. 계절 종료 시 자동 고사. (상세: 섹션 4.1) |
| **점보 성장** | 불가 |
| **Harvestable 시각 묘사** | 낮은 덤불 형태로 퍼진 녹색 잎 사이에 빨간 딸기 여러 개가 매달림. 높이 약 0.2m. |
| **수확물 설명** | 새빨간 하트 모양의 딸기. 아이콘: 빨간 하트형 열매 + 녹색 꼭지. |

### 3.6 호박 (Pumpkin)

| 항목 | 내용 |
|------|------|
| **영문 ID** | `crop_pumpkin` |
| **설명** (게임 내) | "가을의 왕. 넓은 잎 아래 숨은 주황빛 거대 열매는 수확 축제의 주인공이다." |
| **재배 계절** | 가을 |
| **기본 수치** | (-> see `docs/design.md` 섹션 4.2) |
| **특수 메카닉** | **점보 성장 가능** (상세: 섹션 4.3) |
| **점보 성장** | 가능 (3x3, 수확물 15~21개) |
| **Harvestable 시각 묘사** | 넓은 녹색 잎이 방사형으로 펼쳐지고, 중앙에 큰 주황색 호박 1개. 높이 약 0.4m, 폭 약 0.5m. |
| **수확물 설명** | 묵직한 주황색 호박. 아이콘: 둥글납작한 주황 구체 + 녹색 꼭지. |

### 3.7 해바라기 (Sunflower)

| 항목 | 내용 |
|------|------|
| **영문 ID** | `crop_sunflower` |
| **설명** (게임 내) | "태양을 닮은 꽃이 피면, 덤으로 씨앗까지 얻을 수 있다. 자급자족의 상징." |
| **재배 계절** | 여름 |
| **기본 수치** | (-> see `docs/design.md` 섹션 4.2) |
| **특수 메카닉** | **씨앗 드롭** -- 수확 시 해바라기 씨앗 1~2개 추가 획득. (상세: 섹션 4.2) |
| **점보 성장** | 불가 |
| **Harvestable 시각 묘사** | 키 큰 녹색 줄기(높이 약 0.8m) 끝에 큰 노란 꽃 1송이. 꽃잎: 노랑, 중심: 갈색 원반. |
| **수확물 설명** | 노란 꽃잎이 달린 해바라기. 아이콘: 노란 꽃잎 원형 + 갈색 중심. |

### 3.8 수박 (Watermelon)

| 항목 | 내용 |
|------|------|
| **영문 ID** | `crop_watermelon` |
| **설명** (게임 내) | "여름 끝자락에 무르익는 최고급 과일. 기다림의 보상은 달콤하다." |
| **재배 계절** | 여름 |
| **기본 수치** | (-> see `docs/design.md` 섹션 4.2) |
| **특수 메카닉** | **점보 성장 가능** (상세: 섹션 4.3) |
| **점보 성장** | 가능 (3x3, 수확물 15~21개) |
| **Harvestable 시각 묘사** | 넓은 잎이 지면을 덮듯 펼쳐지고, 큰 수박 1개가 땅 위에 놓임. 짙은 녹색 + 연녹색 줄무늬. 높이 약 0.35m, 폭 약 0.6m. |
| **수확물 설명** | 줄무늬가 선명한 대형 수박. 아이콘: 타원형 짙은 녹색 + 연녹색 줄무늬. |

---

### 3.9 겨울무 (Winter Radish) `[NEW]`

겨울/온실 전용 작물. 겨울 콘텐츠 공백을 채우기 위해 신규 설계.

| 항목 | 내용 |
|------|------|
| **영문 ID** | `crop_winter_radish` |
| **설명** (게임 내) | "추위 속에서도 씩씩하게 자라는 흰색 뿌리채소. 온실의 첫 번째 수확을 책임진다." |
| **재배 계절** | 온실 전용 (겨울 시즌에 온실에서만 재배 가능) |
| **성장 기간** | 4일 `[NEW]` |
| **씨앗 구매가** | 23G `[NEW]` |
| **판매가** | 45G `[NEW]` |
| **해금 조건** | 레벨 5 (온실 해금과 동시) `[NEW]` |
| **특수 메카닉** | 없음 (단일 수확, 겨울 입문 작물) |
| **점보 성장** | 불가 |
| **Harvestable 시각 묘사** | 짙은 녹색 잎이 부채꼴로 펼쳐지고, 흙 위로 흰색 무 윗부분이 두툼하게 노출. 높이 약 0.3m. |
| **수확물 설명** | 단단하고 매끈한 흰색 무. 아이콘: 흰색 원기둥 + 녹색 잎사귀. |

**밸런스 설계 의도**: 감자/당근과 유사한 난이도의 입문 작물. 온실 면적(4x4 = 16타일)에서 재배하므로 회전율이 중요. 4일 성장으로 겨울 28일 동안 최대 7회 회전 가능.

```
겨울무 ROI 예상 (BAL-010 확정):
  씨앗비용: 23G, 판매가: 45G (겨울 온실 시너지 x1.2 적용 시 54G)
  기본 순이익: 22G, 일일 수익률: 5.5G/일 (22G/4일 기준)
  시너지 적용 순이익: 31G, 일일 수익률: 7.75G/일 (217G/28일 기준)
  28일 시즌 7회전: 기본 순이익 154G/타일, 시너지 217G/타일
```

### 3.10 표고버섯 (Shiitake Mushroom) `[NEW]`

겨울/온실 전용 다중 수확 작물. 겨울 시즌의 안정적 수입원 역할.

| 항목 | 내용 |
|------|------|
| **영문 ID** | `crop_shiitake` |
| **설명** (게임 내) | "온실의 그늘진 구석에서 조용히 피어나는 버섯. 한 번 심으면 꾸준히 수확할 수 있다." |
| **재배 계절** | 온실 전용 (겨울 시즌에 온실에서만 재배 가능) |
| **성장 기간** | 6일 (첫 수확), 재수확 간격 5일 `[NEW]` |
| **씨앗 구매가** | 50G `[NEW]` |
| **판매가** | 70G `[NEW]` |
| **해금 조건** | 레벨 6 `[NEW]` |
| **특수 메카닉** | **다중 수확** -- 첫 수확 후 5일 간격으로 재수확 가능. 겨울 시즌 종료 시 고사. (상세: 섹션 4.1) |
| **점보 성장** | 불가 |
| **Harvestable 시각 묘사** | 낮은 원목 형태의 베이스 위에 갈색 버섯 2~3개가 모여 자람. 갓: 갈색 반구, 줄기: 연한 베이지. 높이 약 0.15m. |
| **수확물 설명** | 갈색 갓이 탐스러운 표고버섯. 아이콘: 갈색 반구형 갓 + 베이지 줄기. |

**밸런스 설계 의도**: 딸기의 겨울 대응 작물이되, 딸기보다 효율을 낮게 설계하여 온실 면적 제한과 결합해 밸런스를 유지.

```
표고버섯 ROI 예상 (겨울 28일, BAL-010 확정):
  씨앗비용: 50G
  첫 수확 Day 6: 70G (시너지 x1.2 적용 시 84G)
  재수확 Day 11, 16, 21, 26: 4회 x 70G = 280G (시너지 적용 시 4회 x 84G = 336G)
  총 수확: 5회 x 70G = 350G (시너지 적용 시 5회 x 84G = 420G)
  기본 순이익: 300G, 일일 수익률: 10.7G/일
  시너지 적용 순이익: 370G, 일일 수익률: ~13.2G/일
```

### 3.11 시금치 (Spinach) `[NEW]`

겨울/온실 전용 고급 작물. 높은 판매가와 긴 성장 기간으로 리스크-리턴 트레이드오프 제공.

| 항목 | 내용 |
|------|------|
| **영문 ID** | `crop_spinach` |
| **설명** (게임 내) | "영양 가득한 녹색 잎채소. 시간은 걸리지만, 겨울 시장에서 제값을 받는다." |
| **재배 계절** | 온실 전용 (겨울 시즌에 온실에서만 재배 가능) |
| **성장 기간** | 8일 `[NEW]` |
| **씨앗 구매가** | 60G `[NEW]` |
| **판매가** | 130G `[NEW]` |
| **해금 조건** | 레벨 7 `[NEW]` |
| **특수 메카닉** | 없음 (단일 수확, 고가 단일 작물) |
| **점보 성장** | 불가 |
| **Harvestable 시각 묘사** | 짙은 녹색 잎이 로제트 형태로 둥글게 펼쳐짐. 잎의 표면이 약간 주름진 질감. 높이 약 0.2m. |
| **수확물 설명** | 싱싱한 녹색 시금치 다발. 아이콘: 짙은 녹색 잎 3~4장 묶음. |

**밸런스 설계 의도**: 온실 면적 16타일에서 겨울무(빠른 회전)와 시금치(높은 단가) 사이의 선택지를 제공. 8일 성장으로 28일간 3회전, 단일 수확이므로 재투자 비용이 큼.

```
시금치 ROI 예상:
  씨앗비용: 60G, 판매가: 130G
  순이익: 70G, ROI: 117%, 일일 수익률: 8.75G/일
  28일 시즌 3회전: 순이익 210G/타일
```

---

## 4. 특수 작물 메카닉 상세 (Special Mechanics)

### 4.1 다중 수확 (Multi-Harvest)

한 번 심어 여러 번 수확할 수 있는 작물. 수확 후 작물이 사라지지 않고 Growing 단계로 돌아가 다시 열매를 맺는다. (성장 흐름: -> see `docs/systems/crop-growth.md` 섹션 4.2)

**적용 작물**:

| 작물 | 첫 수확 | 재수확 간격 | 최대 수확 횟수 | 재배 시즌 |
|------|---------|-----------|--------------|----------|
| 딸기 | (-> see `docs/design.md` 섹션 4.2) | 3일 | 시즌 내 무제한 | 봄 |
| 표고버섯 `[NEW]` | 6일 | 5일 | 시즌 내 무제한 | 온실 (겨울) |

**공통 규칙**:
1. 수확 후 타일 상태는 Planted(Watered 가능)로 유지
2. 재수확까지 물주기 필요 (Dry/Watered 사이클 반복)
3. 비료 효과는 최초 적용 시 계속 지속 (재수확에도 동일 효과)
4. **계절 종료 시 자동 고사** -- 재수확 남은 횟수와 관계없이 즉시 Withered
5. 재수확 시에도 품질 판정은 매번 독립적으로 실행 (-> see `docs/systems/crop-growth.md` 섹션 4.4)

[OPEN] 파(Green Onion) 등 추가 다중 수확 작물의 도입 여부. 현재는 딸기(봄) + 표고버섯(겨울) 2종으로 계절 간 대응이 가능하나, 여름/가을에 다중 수확 작물이 없어 밸런스 격차가 발생할 수 있다. 여름/가을 다중 수확 후보: 고추(여름), 파(가을).

### 4.2 씨앗 드롭 (Seed Drop)

수확 시 작물 아이템 외에 해당 작물의 씨앗이 추가로 드롭되는 메카닉. 씨앗 재투자 비용을 줄여 장기 운영 효율을 높인다.

**적용 작물 및 파라미터**:

| 작물 | dropRate (드롭 확률) | dropMin | dropMax | 비고 |
|------|---------------------|---------|---------|------|
| 해바라기 | 0.70 | 1 | 2 | 기존 메카닉 |
| 옥수수 | 0.30 | 1 | 1 | 신규 적용 제안 |

**씨앗 드롭 공식**:
```
수확 시:
  if random() < dropRate:
    seedCount = random_int(dropMin, dropMax)
    inventory.add(seedItem, seedCount)
```

**해바라기 상세**:
- `dropRate`: 0.70 (70% 확률로 씨앗 드롭)
- `dropMin`: 1, `dropMax`: 2
- 기대 씨앗 수: 0.70 * 1.5 = 1.05개/수확
- 실질적으로 씨앗 비용을 대부분 회수 가능

**옥수수 씨앗 드롭 제안**:
- `dropRate`: 0.30 (30% 확률)
- `dropMin`: 1, `dropMax`: 1
- 기대 씨앗 수: 0.30개/수확
- 옥수수 씨앗 가격 대비 소폭 보조. 여름 중반 이후 재투자 부담 경감 용도.

[OPEN] 옥수수 씨앗 드롭 적용 여부 최종 확정 필요. 적용 시 `docs/systems/crop-growth.md` 섹션 4.5와 `docs/balance/crop-economy.md`에 반영해야 한다.

### 4.3 점보 성장 (Giant Crop)

특정 조건 충족 시 인접 3x3 범위의 같은 작물이 하나의 거대 작물로 변환된다. (변환 조건/확률: -> see `docs/systems/crop-growth.md` 섹션 5.1)

**적용 작물**:

| 작물 | 거대 크기 | 수확물 수량 | 수확물 품질 | 시각 묘사 |
|------|----------|-----------|-----------|----------|
| 호박 | 3x3 | 15~21개 | Gold 70%, Iridium 30% | 타일 3x3을 덮는 초대형 주황 호박. 덩굴이 주변으로 뻗음. |
| 수박 | 3x3 | 15~21개 | Gold 70%, Iridium 30% | 타일 3x3을 차지하는 거대 수박. 줄무늬가 더욱 선명. |

**변환 조건 요약** (-> see `docs/systems/crop-growth.md` 섹션 5.1 canonical):
1. 3x3 범위(9타일) 동일 작물, 모두 Harvestable
2. 수확하지 않고 1일 경과 후 변환 판정
3. 변환 확률: 15%/일
4. 실패 시 매일 재판정

**거대 작물 수확**:
- 수확 도구: [OPEN] 도끼 또는 낫 (-> see `docs/systems/crop-growth.md` 섹션 5.1)
- 수확 시 9타일 모두 Tilled 복귀
- 전용 수확 이펙트 (화면 흔들림 + 대형 파티클)

[OPEN] 겨울 전용 작물(겨울무/시금치)에 점보 성장을 적용할지 여부. 온실 면적이 4x4(16타일)이므로 3x3 배치 자체가 어려워 실질적으로 불가능에 가깝다. 현재는 미적용으로 결정하되, 온실 확장 메카닉 추가 시 재검토.

### 4.4 온실 전용 작물 (Greenhouse-Only)

겨울 시즌에 온실에서만 재배 가능한 작물 3종. 이 작물들은 다른 계절의 야외 농장에서 재배할 수 없다.

**온실 요건**:

| 항목 | 내용 |
|------|------|
| 온실 해금 | 레벨 5 + 건설 비용 (-> see `docs/design.md` 섹션 4.6) |
| 온실 면적 | 4x4 = 16타일 |
| 계절 보정 | x1.0 고정 (보너스/패널티 없음) |
| 물주기 | 수동 필수 (비 효과 없음) |
| 날씨 | 항상 맑음 취급 (폭풍 피해 면역) |

**전용 작물 목록**:

| 작물 | 해금 | 전략적 위치 |
|------|------|-----------|
| 겨울무 | 레벨 5 | 빠른 회전의 입문 작물 |
| 표고버섯 | 레벨 6 | 다중 수확으로 안정적 수입 |
| 시금치 | 레벨 7 | 높은 단가의 고급 작물 |

**씨앗 구매 경로 (DES-014 확정)**:

| 판매처 | 시기 | 가격 | 조건 |
|--------|------|------|------|
| 여행 상인 (바람이) | 겨울 전 기간 (Day 1~28) | 정가 x1.5 (→ see `docs/balance/traveler-economy.md` 섹션 3.8) | 등장 확률에 의존, 재고 1~3개 |
| 잡화 상점 (하나) | 겨울 2주차부터 (Day 8~28) | 정가 (→ see 본 문서 섹션 6) | 온실 보유 + 해금 레벨 충족 |

여행 상인은 1주 선점 + 프리미엄 가격, 잡화 상점은 안정적 접근 + 정가. 혼합 모델로 겨울 콘텐츠 진입 장벽과 접근성의 균형을 달성한다. (→ see `docs/content/npcs.md` 섹션 3.3, 6.3)

**겨울 온실 전략 분기점**: 플레이어는 16타일의 제한된 공간에서 세 작물을 조합해야 한다. 겨울무로 빠른 자금을 확보하며 표고버섯으로 안정적 수입을 유지하고, 시금치로 고수익을 노리는 것이 균형 잡힌 전략이다. 16타일 전부를 한 작물에 투자하는 것은 리스크가 있다 (물 관리 부담, 수확 타이밍 집중).

---

## 5. 작물 수확물 아이템 데이터 (Item Data)

각 작물 수확 시 인벤토리에 추가되는 아이템 정보.

### 5.1 공통 속성

| 속성 | 값 |
|------|---|
| 카테고리 | `Crop` |
| 스택 가능 | O |
| 최대 스택 | 99 (-> see `docs/systems/inventory-system.md`) |
| 판매 가능 | O |
| 품질 속성 | O (Normal / Silver / Gold / Iridium, -> see `docs/systems/crop-growth.md` 섹션 4.3) |

### 5.2 아이템 ID 매핑

| 작물 | 수확물 ItemID | 씨앗 ItemID | 판매가 |
|------|-------------|-----------|--------|
| 감자 | `crop_potato` | `seed_potato` | (-> see `docs/design.md` 섹션 4.2) |
| 당근 | `crop_carrot` | `seed_carrot` | (-> see `docs/design.md` 섹션 4.2) |
| 토마토 | `crop_tomato` | `seed_tomato` | (-> see `docs/design.md` 섹션 4.2) |
| 옥수수 | `crop_corn` | `seed_corn` | (-> see `docs/design.md` 섹션 4.2) |
| 딸기 | `crop_strawberry` | `seed_strawberry` | (-> see `docs/design.md` 섹션 4.2) |
| 호박 | `crop_pumpkin` | `seed_pumpkin` | (-> see `docs/design.md` 섹션 4.2) |
| 해바라기 | `crop_sunflower` | `seed_sunflower` | (-> see `docs/design.md` 섹션 4.2) |
| 수박 | `crop_watermelon` | `seed_watermelon` | (-> see `docs/design.md` 섹션 4.2) |
| 겨울무 `[NEW]` | `crop_winter_radish` | `seed_winter_radish` | 45G |
| 표고버섯 `[NEW]` | `crop_shiitake` | `seed_shiitake` | 70G |
| 시금치 `[NEW]` | `crop_spinach` | `seed_spinach` | 130G |

---

## 6. 신규 작물 canonical 수치 요약 `[NEW]`

겨울/온실 전용 작물 3종의 canonical 수치 (BAL-010 확정값). `docs/design.md` 섹션 4.2는 이 표를 `→ see docs/content/crops.md 섹션 3.9~3.11` 참조로 반영 완료.

| 작물 | 성장 기간 | 판매가 | 씨앗가 | 해금 조건 | 재배 계절 |
|------|-----------|--------|--------|-----------|----------|
| 겨울무 | 4일 | 45G | 23G | 레벨 5 | 온실 전용 |
| 표고버섯 | 6일 (재수확 5일) | 70G | 50G | 레벨 6 | 온실 전용 |
| 시금치 | 8일 | 130G | 60G | 레벨 7 | 온실 전용 |

Cross-reference 업데이트 필요 문서 목록:
- `docs/design.md` 섹션 4.2 -- 작물 테이블에 3종 추가
- `docs/balance/crop-economy.md` -- 신규 3종의 ROI/밸런스 분석 추가
- `docs/systems/crop-growth.md` 섹션 3.1 -- 작물-계절 매핑 테이블에 3종 추가
- `docs/systems/crop-growth.md` 섹션 4.2 -- 표고버섯 다중 수확 규칙 추가

---

## 6. 과일나무 (Fruit Trees)

> [CANONICAL] 과일나무 가격/성장/수확 데이터의 canonical 출처는 본 섹션이다.
> `docs/systems/farm-expansion.md` 섹션 4.4는 이 섹션을 참조한다.

### 6.1 과일나무 개요

과일나무는 일반 작물과 달리 **영구 설치** 방식이다. Zone G(과수원, `zone_orchard`)에서 배치 가능하며, 한번 심으면 매 해당 계절마다 자동 수확된다.

- **배치 방식**: 3x3 타일 점유 (중앙에 나무, 주변 8타일 접근 불가)
- **성숙 후**: 해당 수확 계절마다 자동으로 과일 생산, 별도 재배 작업 불필요
- **해금 조건**: Zone G(`zone_orchard`) 해금 (레벨 7, 5,000G — `docs/systems/farm-expansion.md` 섹션 2.1)

### 6.2 과일나무 데이터 테이블

| 과일나무 | 아이템 ID | 묘목 가격 | 성숙 기간 | 수확 계절 | 수확량/계절 | 과일 판매가 |
|---------|----------|----------|----------|----------|------------|-----------|
| 사과나무 | `tree_apple` | 500G | 1계절 (28일) | 가을 | 3개 | 50G/개 |
| 복숭아나무 | `tree_peach` | 800G | 1계절 (28일) | 여름 | 3개 | 70G/개 |
| 체리나무 | `tree_cherry` | 600G | 1계절 (28일) | 봄 | 4개 | 40G/개 |
| 감나무 | `tree_persimmon` | 1,000G | 2계절 (56일) | 가을 | 2개 | 120G/개 |

### 6.3 경제성 분석

장기 투자형 콘텐츠. 묘목 비용이 높고 성숙 대기가 길지만, 성숙 후 매년 추가 투자 없이 수확 가능.

| 과일나무 | 연간 수익 | 투자 회수 (계절) | 비고 |
|---------|---------|----------------|------|
| 사과나무 | 3 x 50G = 150G/년 | 약 4계절 (1년) | 가을 1회 |
| 복숭아나무 | 3 x 70G = 210G/년 | 약 4계절 (1년) | 여름 1회 |
| 체리나무 | 4 x 40G = 160G/년 | 약 4계절 (1년) | 봄 1회 |
| 감나무 | 2 x 120G = 240G/년 | 약 5계절 (1.25년, 성숙 2계절 포함) | 가을 1회 |

[RISK] 과일나무 ROI가 일반 작물 대비 불리할 수 있다. 과일 가공품(와인, 잼 — `docs/content/processing-system.md`)의 높은 가격 배수로 보완 예정. 정밀 밸런스 분석은 별도 BAL 항목으로 추후 수행.

---

## 7. Cross-references

| 문서 | 관련 내용 |
|------|----------|
| `docs/design.md` 섹션 4.2 | 작물 기본 데이터 canonical (가격, 성장일수, 해금 조건) |
| `docs/design.md` 섹션 4.3 | 시간/계절 시스템 (1계절 = 28일, 계절 전환 규칙) |
| `docs/design.md` 섹션 4.6 | 시설 목록 (온실 건설 요건, 가격) |
| `docs/systems/crop-growth.md` | 성장 단계/공식 canonical, 특수 작물 메카닉 프레임워크 |
| `docs/systems/farming-system.md` | 타일 상태 머신, 도구 상호작용, 비료/토양 효과 |
| `docs/systems/inventory-system.md` | 아이템 카테고리 체계, 스택 규칙, ItemID 컨벤션 |
| `docs/balance/crop-economy.md` | ROI/밸런스 분석, 시즌별 수익 시뮬레이션 |
| `docs/systems/economy-system.md` | 경제 시스템 (가격 보정, 수급 시스템) |
| `docs/systems/farm-expansion.md` | 과수원 구역(Zone G) 정의, 과일나무 배치 규칙 |

---

## 8. Open Questions / Risks

### Open Questions

- `[OPEN]` **여름/가을 다중 수확 작물 부재**: 현재 다중 수확은 봄(딸기)과 겨울(표고버섯)에만 존재. 여름과 가을에 다중 수확 작물이 없어 계절 간 플레이 패턴 차이가 클 수 있다. 후보: 고추(여름), 파(가을).

- `[OPEN]` **옥수수 씨앗 드롭 적용 여부**: 섹션 4.2에서 제안. 적용 시 여름 작물 간 차별화에 기여하나, 해바라기의 고유 특성이 희석될 우려.

- `[OPEN]` **겨울 전용 작물의 다른 계절 온실 재배 허용 여부**: 현재 설계는 "겨울에 온실에서만" 재배 가능으로 제한. 다른 계절에도 온실에서 재배 가능하게 하면 온실 활용도가 높아지나, 계절별 콘텐츠 구분이 약해질 수 있다.

- `[OPEN]` **거대 작물 수확 도구 결정**: 도끼 vs 낫. 현재 도구 목록에 도끼가 없음 (-> see `docs/systems/crop-growth.md` 섹션 5.1).

- `[OPEN]` **교잡/돌연변이 작물과 겨울 전용 작물의 관계**: 겨울 전용 작물이 교잡 레시피에 참여할 수 있는지 여부 (-> see `docs/systems/crop-growth.md` 섹션 5.2).

- `[OPEN]` **비료 종류 확장**: crop-growth.md에서 제기된 비료 4종 체계에서 배수 단계가 제한적인 문제. 겨울 전용 작물에 특화된 비료(온실 비료 등)를 추가할지 검토.

### Risks

- `[RISK]` **표고버섯 밸런스**: 다중 수확 특성상 딸기와 동일한 지배 전략 문제가 발생할 수 있음. 다만 온실 면적 16타일 제한이 자연스러운 캡 역할. crop-economy.md에서 정밀 검증 필요.

- `[RISK]` **겨울 콘텐츠 밀도**: 온실 16타일 + 작물 3종만으로 겨울 28일을 채우기에 콘텐츠가 빈약할 수 있음. 가공/건설/NPC 등 비경작 콘텐츠와의 연계가 필수적.

- `[RISK]` **신규 작물 데이터 동기화**: `[NEW]` 태그가 붙은 3종의 데이터가 design.md, crop-growth.md, crop-economy.md에 아직 반영되지 않았음. 다음 작업에서 반드시 동기화해야 canonical 충돌을 방지할 수 있다.

---

# 장식 아이템 콘텐츠 상세 (Decoration Items)

> 작성: Claude Code (Sonnet 4.6) | 2026-04-08
> 문서 ID: CON-020

---

## Context

이 문서는 SeedMind 농장 장식 시스템의 **canonical 콘텐츠 스펙 문서**다. 다음 데이터의 단일 출처(source of truth)이다:

| 데이터 | canonical 여부 |
|--------|--------------|
| `itemId` (영문 SO ID) | canonical |
| `buyPrice` (구매가) | canonical |
| `tileWidthX` / `tileHeightZ` (점유 타일) | canonical |
| `lightRadius` | canonical |
| `moveSpeedBonus` | canonical |
| `durabilityMax` | canonical |
| `unlockLevel` / `unlockZoneId` | canonical |
| `limitedSeason` | canonical |
| `repairCostRatio` | canonical |

**설계 근거(rationale)** — 카테고리별 시스템 의도, 배치 규칙, 경제 연동 원칙은 (→ see `docs/systems/decoration-system.md`) canonical이다. 이 문서는 수치를 확정하는 역할이며 설계 근거를 중복 기술하지 않는다.

**DecorationItemData 스키마** — SO 필드 정의는 (→ see `docs/systems/decoration-system.md` 섹션 6.1)를 참조한다. 이 문서의 테이블 수치는 해당 스키마의 필드값으로 직접 매핑된다.

**전체 아이템 수**: 29종 (Fence 4 + Path 5 + Light 4 + Ornament 11 + WaterDecor 5)

---

## 1. Fence (울타리) 카테고리

설계 근거: (→ see `docs/systems/decoration-system.md` 섹션 2.1)

**공통 규칙**:
- `isEdgePlaced = true` — 타일 점유 없음, 타일 경계(edge) 배치
- `tileWidthX = 0`, `tileHeightZ = 0`
- `moveSpeedBonus = 0`, `lightRadius = 0`

### 1.1 울타리 스펙 테이블

| itemId | displayName | buyPrice (G/단) | isEdgePlaced | unlockLevel | limitedSeason | durabilityMax | repairCostRatio |
|--------|-------------|----------------|--------------|-------------|---------------|---------------|----------------|
| `FenceWood` | 나무 울타리 | 5 | true | 0 | None | 100 | 0.20 |
| `FenceStone` | 돌 울타리 | 15 | true | 3 | None | 0 | - |
| `FenceIron` | 쇠 울타리 | 30 | true | 6 | None | 0 | - |
| `FenceFloral` | 꽃 울타리 | 20 | true | 5 | Spring | 0 | - |

### 1.2 내구도 상세

| itemId | durabilityMax 산출 근거 | 감소 규칙 | 수리 조건 |
|--------|----------------------|----------|---------|
| `FenceWood` | 10계절 × 10포인트/계절 = 100 | 매 계절 시작 시 -10 | durability = 0 → "부서진 울타리" 상태 전환. 클릭 → buyPrice × 0.20 = 1G/단 |
| `FenceStone` | 영구 (0 = 감소 없음) | 없음 | 해당 없음 |
| `FenceIron` | 영구 (0 = 감소 없음) | 없음 | 해당 없음 |
| `FenceFloral` | 영구 (0 = 감소 없음) | 없음 | 해당 없음 |

**`repairCostRatio` 적용**: `FenceWood`에만 `repairCostRatio = 0.20` (구매가의 20%). 영구 울타리 3종은 해당 없음(`-`).

### 1.3 계절 한정 구매

- `FenceFloral`: `limitedSeason = Spring` — 봄 시즌에만 상점 구매 가능. 구매 후 영구 보유. 타 계절에도 배치된 상태로 유지됨.

---

## 2. Path (경로) 카테고리

설계 근거: (→ see `docs/systems/decoration-system.md` 섹션 2.2)

**공통 규칙**:
- `isEdgePlaced = false` — 타일 위 오버레이 배치
- `tileWidthX = 1`, `tileHeightZ = 1` (1×1 타일 오버레이)
- `moveSpeedBonus = 0.1` (+10%) — 모든 경로 공통
- `durabilityMax = 0` (영구), `lightRadius = 0`

### 2.1 경로 스펙 테이블

| itemId | displayName | buyPrice (G/타일) | tileWidthX×tileHeightZ | unlockLevel | moveSpeedBonus | 특수 효과 |
|--------|-------------|-----------------|----------------------|-------------|----------------|---------|
| `PathDirt` | 흙 다짐 경로 | 2 | 1×1 | 0 | 0.1 | 잡초 억제, 비 후 진흙 방지 |
| `PathGravel` | 자갈 경로 | 5 | 1×1 | 2 | 0.1 | 잡초 억제, 비 후 진흙 방지 |
| `PathStone` | 돌판 경로 | 12 | 1×1 | 4 | 0.1 | 잡초 억제, 비 후 진흙 방지 |
| `PathBrick` | 벽돌 경로 | 18 | 1×1 | 6 | 0.1 | 잡초 억제, 비 후 진흙 방지 |
| `PathWood` | 목판 경로 | 10 | 1×1 | 3 | 0.1 | 잡초 억제, 비 후 진흙 방지 |

### 2.2 경로 특수 효과 정의

| 효과 | 적용 조건 | 비고 |
|------|---------|------|
| 잡초 억제 | 경로가 깔린 타일에 잡초 미발생 | 모든 경로 공통 |
| 비 후 진흙 방지 | 비 다음 날 해당 타일 진흙 상태 미전환 | 모든 경로 공통 |
| 이동 속도 보너스 | 경로 위 이동 시 +10% 속도 | `moveSpeedBonus = 0.1`, 모든 경로 공통 |

**배치 제약**: 경작지(Farmland) 타일 위에 경로 배치 불가. (→ see `docs/systems/decoration-system.md` 섹션 3.3)

---

## 3. Light (조명) 카테고리

설계 근거: (→ see `docs/systems/decoration-system.md` 섹션 2.3)

**공통 규칙**:
- `isEdgePlaced = false`
- `moveSpeedBonus = 0`, `durabilityMax = 0`
- 야간 가시성 규칙: (→ see `docs/systems/time-season.md` 야간 가시성 규칙) [OPEN#C — time-season.md에 야간 어두움 메카닉이 미정의; 미구현 시 조명은 순수 미관 오브젝트로만 존재]

### 3.1 조명 스펙 테이블

| itemId | displayName | buyPrice (G) | tileWidthX×tileHeightZ | lightRadius (타일) | unlockLevel | 날씨 반응 |
|--------|-------------|-------------|----------------------|------------------|-------------|---------|
| `LightTorch` | 횃불 | 30 | 1×1 | 2 | 2 | 비·눈 날씨에 꺼짐 (시각 이벤트) |
| `LightLantern` | 등롱 | 80 | 1×1 | 3 | 4 | 항상 켜짐 |
| `LightStreet` | 가로등 | 200 | 2×1 | 5 | 6 | 항상 켜짐 |
| `LightCrystal` | 마법 수정 조명 | 500 | 1×1 | 4 | 8 | 항상 켜짐, 색상 변경 가능 |

### 3.2 조명 상세 비고

| itemId | 비고 |
|--------|------|
| `LightTorch` | 비(`Rain`) / 눈(`Snow`) 날씨 타입 발생 시 꺼짐 상태로 전환 — 게임플레이 효과 없이 시각 이벤트로만 처리 |
| `LightLantern` | 날씨 무관 상시 점등 |
| `LightStreet` | `tileWidthX = 2`, `tileHeightZ = 1` (2×1 타일 점유). 날씨 무관 상시 점등 |
| `LightCrystal` | UI 팔레트로 색상 변경 가능. 구체적 색상 목록은 [OPEN] — 구현 시 확정 |

---

## 4. Ornament (장식물) 카테고리

설계 근거: (→ see `docs/systems/decoration-system.md` 섹션 2.4)

**공통 규칙**:
- `isEdgePlaced = false`
- `moveSpeedBonus = 0`, `lightRadius = 0`, `durabilityMax = 0`
- 생산성 보너스 없음 — 순수 미관 오브젝트

### 4.1 장식물 스펙 테이블

| itemId | displayName | buyPrice (G) | tileWidthX×tileHeightZ | unlockLevel | limitedSeason | 비고 |
|--------|-------------|-------------|----------------------|-------------|---------------|------|
| `OrnaScareRaven` | 나무 허수아비 | 100 | 1×1 | 0 | None | |
| `OrnaFlowerPotS` | 꽃 화분 (소) | 40 | 1×1 | 2 | Summer | 봄/여름 판매 [OPEN#A] |
| `OrnaFlowerPotL` | 꽃 화분 (대) | 80 | 1×1 | 3 | Summer | 봄/여름 판매 [OPEN#A] |
| `OrnaBenchWood` | 나무 벤치 | 120 | 2×1 | 3 | None | |
| `OrnaStatueStone` | 돌 조각상 | 300 | 1×1 | 5 | None | |
| `OrnaWindmillS` | 풍차 (소형) | 400 | 2×2 | 5 | None | |
| `OrnaWellDecor` | 우물 장식 | 250 | 2×2 | 4 | None | |
| `OrnaSignBoard` | 농장 표지판 | 60 | 1×1 | 0 | None | 텍스트 입력 최대 20자 |
| `OrnaPumpkinLantern` | 호박 등불 | 80 | 1×1 | 5 | Autumn | 가을 한정 판매 |
| `OrnaSnowman` | 눈사람 | 50 | 1×1 | 3 | Winter | 겨울 한정 판매 |
| `OrnaStatueGold` | 황금 조각상 | 2000 | 2×2 | 9 | None | |

### 4.2 계절 한정 판매 상세

| itemId | limitedSeason | 판매 시즌 설명 |
|--------|--------------|-------------|
| `OrnaFlowerPotS` | Summer | 봄/여름 판매 [OPEN#A] |
| `OrnaFlowerPotL` | Summer | 봄/여름 판매 [OPEN#A] |
| `OrnaPumpkinLantern` | Autumn | 가을 시즌에만 구매 가능 |
| `OrnaSnowman` | Winter | 겨울 시즌에만 구매 가능 |

[OPEN#A] `OrnaFlowerPotS`/`OrnaFlowerPotL`의 `limitedSeason`은 "봄/여름" 복수 시즌 판매로 기술되어 있으나, `DecorationItemData.limitedSeason` 필드는 `Season` 단일 열거형이다. 봄+여름 양 시즌 판매를 지원하려면 `Season` Flags enum 확장 또는 `limitedSeasons: Season[]` 배열 필드로 스키마 변경이 필요하다. 구현 확정 전까지 `Summer`를 임시값으로 기재하며, 실제 구현 시 decoration-architecture.md 및 data-pipeline.md 섹션 2.14와 함께 동기화해야 한다. (→ see `docs/systems/decoration-system.md` 섹션 2.4 "계절 한정 판매")

---

## 5. WaterDecor (수경 장식) 카테고리

설계 근거: (→ see `docs/systems/decoration-system.md` 섹션 2.5)

**공통 규칙**:
- `isEdgePlaced = false`
- `moveSpeedBonus = 0`, `lightRadius = 0`, `durabilityMax = 0`
- Zone F(연못 구역) 해금 선행 요건. (→ see `docs/systems/farm-expansion.md` 섹션 1.3)

### 5.1 수경 장식 스펙 테이블

| itemId | displayName | buyPrice (G) | tileWidthX×tileHeightZ | unlockLevel | unlockZoneId | 비고 |
|--------|-------------|-------------|----------------------|-------------|-------------|------|
| `WaterLotus` | 연꽃 군락 | 150 | 2×2 | 0 | `zone_f` | Zone F 해금만 필요 |
| `WaterBridge` | 나무 다리 | 300 | 1×3 | 0 | `zone_f` | Zone F 해금만 필요 |
| `WaterFountainS` | 분수 (소) | 500 | 2×2 | 6 | `zone_f` | 레벨 6 + Zone F |
| `WaterFountainL` | 분수 (대) | 1200 | 3×3 | 8 | `zone_f` | 레벨 8 + Zone F |
| `WaterDuck` | 오리 조각 | 80 | 1×1 | 0 | `zone_f` | Zone F 해금만 필요 |

### 5.2 해금 조건 상세

`unlockLevel = 0`이고 `unlockZoneId = "zone_f"`인 아이템은 Zone F 해금만으로 접근 가능하며 레벨 제약은 없다. `WaterFountainS`/`WaterFountainL`은 레벨 AND Zone F 양쪽 조건을 모두 충족해야 한다.

`unlockZoneId` 값 `"zone_f"` 는 (→ see `docs/systems/farm-expansion.md` 섹션 1.3) Zone F ID를 따른다.

---

## 6. 전체 아이템 요약 (29종)

### 6.1 카테고리별 원페이지 요약 테이블

| itemId | displayName | 카테고리 | buyPrice (G) | 해금 조건 |
|--------|-------------|---------|-------------|---------|
| `FenceWood` | 나무 울타리 | Fence | 5/단 | 시작 |
| `FenceStone` | 돌 울타리 | Fence | 15/단 | 레벨 3 |
| `FenceIron` | 쇠 울타리 | Fence | 30/단 | 레벨 6 |
| `FenceFloral` | 꽃 울타리 | Fence | 20/단 | 레벨 5, 봄 한정 |
| `PathDirt` | 흙 다짐 경로 | Path | 2/타일 | 시작 |
| `PathGravel` | 자갈 경로 | Path | 5/타일 | 레벨 2 |
| `PathStone` | 돌판 경로 | Path | 12/타일 | 레벨 4 |
| `PathBrick` | 벽돌 경로 | Path | 18/타일 | 레벨 6 |
| `PathWood` | 목판 경로 | Path | 10/타일 | 레벨 3 |
| `LightTorch` | 횃불 | Light | 30 | 레벨 2 |
| `LightLantern` | 등롱 | Light | 80 | 레벨 4 |
| `LightStreet` | 가로등 | Light | 200 | 레벨 6 |
| `LightCrystal` | 마법 수정 조명 | Light | 500 | 레벨 8 |
| `OrnaScareRaven` | 나무 허수아비 | Ornament | 100 | 시작 |
| `OrnaFlowerPotS` | 꽃 화분 (소) | Ornament | 40 | 레벨 2, 봄/여름 한정 |
| `OrnaFlowerPotL` | 꽃 화분 (대) | Ornament | 80 | 레벨 3, 봄/여름 한정 |
| `OrnaBenchWood` | 나무 벤치 | Ornament | 120 | 레벨 3 |
| `OrnaStatueStone` | 돌 조각상 | Ornament | 300 | 레벨 5 |
| `OrnaWindmillS` | 풍차 (소형) | Ornament | 400 | 레벨 5 |
| `OrnaWellDecor` | 우물 장식 | Ornament | 250 | 레벨 4 |
| `OrnaSignBoard` | 농장 표지판 | Ornament | 60 | 시작 |
| `OrnaPumpkinLantern` | 호박 등불 | Ornament | 80 | 레벨 5, 가을 한정 |
| `OrnaSnowman` | 눈사람 | Ornament | 50 | 레벨 3, 겨울 한정 |
| `OrnaStatueGold` | 황금 조각상 | Ornament | 2000 | 레벨 9 |
| `WaterLotus` | 연꽃 군락 | WaterDecor | 150 | Zone F |
| `WaterBridge` | 나무 다리 | WaterDecor | 300 | Zone F |
| `WaterFountainS` | 분수 (소) | WaterDecor | 500 | 레벨 6 + Zone F |
| `WaterFountainL` | 분수 (대) | WaterDecor | 1200 | 레벨 8 + Zone F |
| `WaterDuck` | 오리 조각 | WaterDecor | 80 | Zone F |

### 6.2 가격 범위 요약

| 카테고리 | 최소 buyPrice | 최대 buyPrice |
|---------|------------|------------|
| Fence | 5G/단 | 30G/단 |
| Path | 2G/타일 | 18G/타일 |
| Light | 30G | 500G |
| Ornament | 40G | 2,000G |
| WaterDecor | 80G | 1,200G |
| **전체** | **2G** | **2,000G** |

---

## 7. 계절 한정 아이템 목록

decoration-system.md 섹션 2.1~2.4에서 정의된 계절 한정 아이템 통합 목록.

| itemId | displayName | limitedSeason | 구매 가능 시즌 | 구매 후 |
|--------|-------------|--------------|-------------|-------|
| `FenceFloral` | 꽃 울타리 | Spring | 봄 시즌만 | 영구 보유 |
| `OrnaFlowerPotS` | 꽃 화분 (소) | Summer | 봄/여름 시즌 [OPEN#A] | 영구 보유 |
| `OrnaFlowerPotL` | 꽃 화분 (대) | Summer | 봄/여름 시즌 [OPEN#A] | 영구 보유 |
| `OrnaPumpkinLantern` | 호박 등불 | Autumn | 가을 시즌만 | 영구 보유 |
| `OrnaSnowman` | 눈사람 | Winter | 겨울 시즌만 | 영구 보유 |

**공통 규칙**: 계절 한정 아이템은 해당 시즌에 상점에서 구매 가능. 한 번 구매하면 영구 보유이며, 타 계절에도 배치 상태로 유지된다. 타 계절에는 배치·철거만 가능하며 재구매 불가.

---

## Cross-references

- `docs/systems/decoration-system.md` (DES-023) — 설계 근거(rationale), 배치 메카닉, 카테고리 개요, DecorationItemData SO 필드 예시
- `docs/systems/decoration-architecture.md` (ARC-043) — DecorationManager, DecorationItemData SO C# 스키마, DecorationSaveData
- `docs/mcp/decoration-tasks.md` (ARC-046) — SO 에셋 생성 MCP 태스크 시퀀스 (이 문서의 itemId를 canonical로 참조)
- `docs/pipeline/data-pipeline.md` 섹션 2.14 — DecorationItemData SO 에셋 스키마 (콘텐츠 수치는 이 문서 참조)
- `docs/systems/farm-expansion.md` 섹션 1.3 — Zone F(연못 구역) 해금 조건, `unlockZoneId = "zone_f"` 출처
- `docs/systems/time-season.md` — 계절 정의(`Season` enum), 야간 가시성 규칙, 날씨 타입(`Rain`/`Snow`)
- `docs/systems/economy-system.md` — 골드 소모처 역할 경제 연동 원칙

---

## Open Questions

- [OPEN#A] `OrnaFlowerPotS`/`OrnaFlowerPotL`의 복수 시즌(봄+여름) 판매를 `Season` 단일 필드로 표현하는 방법이 미결정이다. 옵션 1: `Season` Flags enum으로 확장 (`[System.Flags] Spring=1, Summer=2, Autumn=4, Winter=8`). 옵션 2: 스키마에 `limitedSeasons: Season[]` 배열 필드 추가. 구현 확정 후 decoration-architecture.md 및 data-pipeline.md 섹션 2.14와 동시 업데이트 필요.
- [OPEN#B] `LightCrystal` (마법 수정 조명) 색상 변경 팔레트 — 선택 가능한 색상 목록 및 UI 인터랙션 방식이 미결정이다. ui-system.md 색상 팔레트 설계 시 함께 확정 필요.
- [OPEN#C] 야간 조명 가시성 규칙 미결정 — `docs/systems/time-season.md`에 야간 어두움 메카닉이 정의되지 않았다. 야간 가시성 구현 여부에 따라 Light 카테고리 기능 효과(`lightRadius`)가 의미를 가지거나 순수 미관으로만 존재한다. ARC 단계에서 결정 필요. (→ see `docs/systems/decoration-system.md` 섹션 2.3 [OPEN#2])

---

## Risks

- [RISK] decoration-tasks.md (ARC-046) SO 에셋 생성 시 itemId를 임의 생성하지 않도록 이 문서의 섹션 6.1 요약 테이블을 반드시 참조해야 한다. PATTERN-011 이슈가 이를 계기로 등록됨.
- [RISK] `OrnaFlowerPotS`/`OrnaFlowerPotL` 복수 시즌 처리([OPEN#A])가 해소되지 않으면 스키마 변경이 decoration-architecture.md, data-pipeline.md, 구현 코드에 연쇄 영향을 준다. 조기 확정 권장.
- [RISK] `WaterBridge` (`tileWidthX = 1`, `tileHeightZ = 3`)는 비정형 점유(1×3)로 배치 충돌 검사 로직이 여타 아이템과 다를 수 있다. 구현 시 타일맵 충돌 처리 주의.

---

# 시설 콘텐츠 상세 (Facilities Content)

> 작성: Claude Code (Opus) | 2026-04-06
> 문서 ID: CON-002 | Phase 1

---

## 1. 개요

### 1.1 Context

이 문서는 SeedMind에 등장하는 모든 시설의 콘텐츠 상세 정보를 기술한다. 마스터 디자인 문서(`docs/design.md`) 섹션 4.6에 정의된 4종 시설의 기본 비용/해금 조건을 기반으로, 각 시설의 상세 메카닉, 업그레이드 경로, 건설 요건, 플레이어 인터랙션, 수익성 분석, 향후 확장 후보를 다룬다.

**설계 목표**: 시설은 핵심 루프(경작-수확-판매)를 자동화하거나 확장하여 게임 중후반의 깊이를 제공한다. 각 시설은 명확한 역할 분담과 트레이드오프를 가지며, 건설 순서에 따라 농장 운영 전략이 달라져야 한다.

### 1.2 본 문서가 canonical인 데이터

- 시설별 상세 메카닉 파라미터 (자동 물주기 범위, 온실 내부 타일 수, 창고 슬롯 수 등)
- 시설 업그레이드 경로 (레벨별 비용, 효과)
- 시설 건설 공통 규칙 (건설 시간, 타일 배치, 철거 규칙)
- 가공 레시피 목록 및 상세 파라미터
- 시설 점유 타일 수
- 향후 확장 후보 시설 목록

### 1.3 본 문서가 canonical이 아닌 데이터 (참조만)

| 데이터 종류 | 참조처 |
|------------|--------|
| 시설 기본 비용, 해금 레벨 | `docs/design.md` 섹션 4.6 |
| 가공품 가격 공식, 가공 유형별 배수 | `docs/systems/economy-system.md` 섹션 2.5 |
| 가공소 슬롯 수, 슬롯 확장 비용 | `docs/systems/economy-system.md` 섹션 2.5 |
| 창고 슬롯 수, 최대 건설 수, 추가 비용 | `docs/systems/inventory-system.md` 섹션 2.3 |
| 작물 가격, 성장일수 | `docs/design.md` 섹션 4.2 |
| 겨울 전용 작물 수치 | `docs/content/crops.md` 섹션 3.9~3.11 |
| 날씨 종류, 비 효과 | `docs/systems/time-season.md` |
| XP 획득량 (시설 건설) | `docs/balance/progression-curve.md` 섹션 1.2.4 |

---

## 2. 시설 공통 규칙

### 2.1 시설 일람 (요약)

| 시설 | 영문 ID | 비용/해금 | 점유 타일 | 역할 |
|------|---------|-----------|----------|------|
| 물탱크 | `building_water_tank` | (-> see `docs/design.md` 섹션 4.6) | 2x2 (4타일) | 인접 경작지 자동 물주기 |
| 창고 | `building_storage` | (-> see `docs/design.md` 섹션 4.6) | 3x2 (6타일) | 수확물 추가 저장 공간 |
| 온실 | `building_greenhouse` | (-> see `docs/design.md` 섹션 4.6) | 6x6 (36타일, 외벽 포함) | 계절 무관 재배 |
| 가공소 | `building_processing` | (-> see `docs/design.md` 섹션 4.6) | 4x3 (12타일) | 작물 → 잼/주스/절임 |
| 제분소 | `building_mill` | (-> see `docs/design.md` 섹션 4.6) | 3x2 (6타일) | 곡물/작물 → 가루/분말 (중간재) |
| 발효실 | `building_fermentation` | (-> see `docs/design.md` 섹션 4.6) | 3x3 (9타일) | 작물 → 와인/식초/발효품 |
| 베이커리 | `building_bakery` | (-> see `docs/design.md` 섹션 4.6) | 4x3 (12타일) | 가공 중간재 → 고급 요리 (연료 소모) |

### 2.2 건설 프로세스

**건설 주문 장소**: 목공소 (Carpenter) NPC에게 의뢰 (-> see `docs/systems/economy-system.md` 섹션 3.1)

**건설 절차**:

1. 목공소 방문 -> 건설 메뉴 열기
2. 시설 선택 -> 비용 확인 -> 골드 지불
3. 농장 맵으로 이동 -> 배치 위치 선택 (배치 프리뷰 표시)
4. 배치 확정 -> 건설 시작
5. 건설 완료 후 사용 가능

**건설 시간**:

| 시설 | 건설 소요 시간 |
|------|--------------|
| 물탱크 | 1일 (다음 날 06:00 완료) |
| 창고 | 1일 |
| 온실 | 2일 (건설 시작 + 2일 후 06:00 완료) |
| 가공소 | 2일 |
| 제분소 | 1일 |
| 발효실 | 2일 |
| 베이커리 | 2일 |

**설계 의도**: 건설 시간은 투자에 대한 지연 보상(delayed gratification)을 만들고, 시설 건설 타이밍이 전략적 선택이 되도록 한다. 온실과 가공소는 더 큰 투자이므로 건설 시간도 길다.

### 2.3 타일 배치 규칙

| 규칙 | 내용 |
|------|------|
| 경작지 겹침 | 시설은 경작지 위에 배치할 수 없음. 경작지를 먼저 빈 땅으로 되돌려야 함 |
| 다른 시설 겹침 | 시설끼리 겹칠 수 없음. 최소 1타일 간격 불필요 (인접 배치 가능) |
| 농장 경계 | 농장 영역 내에서만 배치 가능 |
| 출하함/집 겹침 | 기존 고정 오브젝트(출하함, 플레이어 집)와 겹칠 수 없음 |
| 배치 회전 | 불가. 고정 방향만 지원 (쿼터뷰 기준) |
| 배치 프리뷰 | 배치 가능 영역은 녹색, 불가 영역은 빨간색으로 표시 |

### 2.4 건설 자원

**MVP 기준**: 모든 시설 건설은 **골드만** 소비한다. 추가 재료(목재, 석재 등)는 요구하지 않는다.

**설계 근거**: 현재 design.md에 자원 채집 시스템이 없으므로, MVP에서는 골드 단일 자원으로 단순화한다. 자원 채집이 추가되면 건설 재료 요건을 도입할 수 있다.

[OPEN] 자원 채집 시스템 도입 시 시설 건설에 목재/석재 등 추가 재료 요건을 넣을지 검토. 현재 MVP에서는 골드만으로 충분하나, 게임 깊이 측면에서 추가 재료가 의미 있을 수 있다.

### 2.5 철거 규칙

| 파라미터 | 값 |
|----------|-----|
| 철거 가능 여부 | 가능 |
| 환급률 | 건설 비용의 50% |
| 철거 소요 시간 | 즉시 (같은 날 완료) |
| 내부 아이템 처리 | 철거 전 내부 아이템을 모두 회수해야 함. 아이템이 남아있으면 철거 불가 |
| 업그레이드 환급 | 기본 건설비 + 업그레이드 비용 합산의 50% |

**예시**: 물탱크 Lv.2 (기본 500G + 업그레이드 750G = 총 1,250G) 철거 시 환급 625G.

### 2.6 업그레이드 공통 규칙

- 업그레이드는 **목공소**에서 의뢰 (건설과 동일 NPC)
- 업그레이드 중에도 시설 기능은 **정상 작동** (기존 레벨 기능 유지)
- 업그레이드 소요 시간: 1일 (모든 시설 공통)
- 업그레이드는 순차적 (Lv.1 -> Lv.2 -> Lv.3, 건너뛰기 불가)
- 최대 레벨: 3 (모든 시설 공통)

---

## 3. 물탱크 (Water Tank)

### 3.1 기본 정보

| 항목 | 내용 |
|------|------|
| 영문 ID | `building_water_tank` |
| 건설 비용 / 해금 | (-> see `docs/design.md` 섹션 4.6): 500G, 레벨 3 |
| 점유 타일 | 2x2 (4타일) |
| 최대 건설 수 | 3기 |
| 건설 XP | (-> see `docs/balance/progression-curve.md` 섹션 1.2.4): 30 XP |
| 기능 요약 | 인접 경작지에 매일 자동으로 물을 준다 |

### 3.2 자동 물주기 메카닉

**작동 시점**: 매일 06:00 (하루 시작 시) 자동 실행

**작동 빈도**: 1일 1회 (06:00에만)

**작동 조건**:
- 물탱크에 저수량이 1 이상 남아 있어야 함
- 대상 타일이 Planted 상태 + Dry(건조) 상태일 때만 물을 줌
- 이미 Watered 상태인 타일에는 물을 주지 않음 (중복 소비 없음)

**범위 (레벨별)**:

| 레벨 | 유효 범위 | 커버 타일 수 (최대) | 비고 |
|------|----------|-------------------|------|
| Lv.1 | 물탱크 중심에서 맨해튼 거리 2 이내 | 12타일 | 물탱크 2x2 주변 1칸 |
| Lv.2 | 물탱크 중심에서 맨해튼 거리 3 이내 | 24타일 | 주변 2칸 |
| Lv.3 | 물탱크 중심에서 맨해튼 거리 4 이내 | 40타일 | 주변 3칸 |

```
Lv.1 범위 예시 (. = 범위, T = 물탱크, X = 범위 밖):

X . . . . X
. . . . . .
. . T T . .
. . T T . .
. . . . . .
X . . . . X
```

**설계 의도**: 물탱크를 경작지 중앙에 배치하면 주변 타일의 물주기를 자동화할 수 있다. 단, 물탱크가 4타일을 점유하므로 경작 가능 면적이 줄어드는 트레이드오프가 존재한다.

### 3.3 저수량 시스템

| 파라미터 | Lv.1 | Lv.2 | Lv.3 |
|----------|------|------|------|
| 최대 저수량 | 16 | 32 | 60 |
| 소모량 (타일당) | 1 | 1 | 1 |
| 보충 (비 오는 날) | 전량 회복 | 전량 회복 | 전량 회복 |
| 보충 (맑은 날) | 없음 | 없음 | 없음 |
| 수동 보충 | 물뿌리개로 물탱크 클릭 시 +4/회 | +4/회 | +4/회 |

**작동 흐름**:
1. 매일 06:00, 물탱크가 범위 내 Dry 경작지를 스캔
2. 각 타일에 물 제공 (저수량 1 소모)
3. 저수량이 0이 되면 남은 타일에는 물을 주지 못함
4. 비가 오면 저수량이 최대치로 회복

**수동 보충 메카닉**: 플레이어가 물뿌리개를 들고 물탱크를 클릭하면 저수량을 보충할 수 있다. 1회당 4 보충, 에너지 1 소모 (물뿌리개 사용과 동일). 이를 통해 가뭄이 길어질 때 대응 가능.

**날씨 상호작용**:

| 날씨 | 저수량 효과 | 자동 물주기 효과 |
|------|-----------|----------------|
| Rain | 전량 회복 | 실행하지 않음 (비가 직접 물을 주므로) |
| HeavyRain | 전량 회복 | 실행하지 않음 |
| Storm | 전량 회복 | 실행하지 않음 |
| Clear / Cloudy | 변동 없음 | 정상 실행 |
| Snow / Blizzard | 변동 없음 | 실행하지 않음 (겨울에는 야외 경작 불가) |

### 3.4 업그레이드 경로

| 레벨 | 업그레이드 비용 | 해금 조건 | 효과 |
|------|---------------|----------|------|
| Lv.1 | (기본 건설) | 레벨 3 | 범위 2, 저수량 16 |
| Lv.2 | 750G | 레벨 4 | 범위 3, 저수량 32 |
| Lv.3 | 1,500G | 레벨 6 | 범위 4, 저수량 60 |

**총 투자 비용** (Lv.3까지): 500G + 750G + 1,500G = 2,750G

### 3.5 사용 방법

| 조작 | 동작 |
|------|------|
| E키 (인접) | 물탱크 정보 패널 열기 (저수량, 범위, 레벨 표시) |
| 물뿌리개 + 좌클릭 | 저수량 수동 보충 (+4) |

**정보 패널 표시 내용**:
- 현재 저수량 / 최대 저수량 (바 형태)
- 현재 범위 (타일 수)
- 시설 레벨
- 업그레이드 버튼 (목공소 이동 안내)

### 3.6 수익성 분석

물탱크는 직접적인 수익을 생산하지 않으나, 플레이어의 에너지와 시간을 절약한다.

**에너지 절약 계산** (Lv.1 기준):
- 물뿌리개 에너지 소모: 1/타일 (-> see `docs/systems/farming-system.md`)
- 12타일 자동 물주기 = 일일 12 에너지 절약
- 에너지 100 기준 12% 절약 -> 추가 경작 활동에 투입 가능

**투자 회수 시점**: 직접 골드 회수는 없으나, 절약된 에너지로 추가 수확할 경우 간접 수익 발생. 감자 기준 12타일 추가 관리 = 일일 약 60G 추가 수확 기회 -> 약 8~9일 만에 간접 회수.

---

## 4. 온실 (Greenhouse)

### 4.1 기본 정보

| 항목 | 내용 |
|------|------|
| 영문 ID | `building_greenhouse` |
| 건설 비용 / 해금 | (-> see `docs/design.md` 섹션 4.6): 2,000G, 레벨 5 |
| 점유 타일 (외부) | 6x6 (36타일, 외벽 + 구조물 포함) |
| 내부 경작 타일 | 4x4 (16타일, Lv.1 기준) |
| 최대 건설 수 | 1기 (유일) |
| 건설 XP | (-> see `docs/balance/progression-curve.md` 섹션 1.2.4): 50 XP |
| 기능 요약 | 계절 무관 작물 재배, 겨울 전용 작물 재배 |

### 4.2 온실 내부 경작 메카닉

**진입 방식**: 온실 문 앞에서 E키 -> 온실 내부 씬으로 전환 (별도 맵)

**내부 경작 타일 (레벨별)**:

| 레벨 | 내부 경작 타일 | 배치 |
|------|-------------|------|
| Lv.1 | 16타일 | 4x4 그리드 |
| Lv.2 | 24타일 | 4x6 그리드 |
| Lv.3 | 36타일 | 6x6 그리드 |

**경작 규칙**: 온실 내부 타일은 야외 경작지와 동일한 메카닉을 따른다 (호미질 -> 심기 -> 물주기 -> 수확). 단, 아래 차이점 존재:

| 항목 | 야외 | 온실 내부 |
|------|------|----------|
| 계절 제한 | 해당 계절 작물만 | 모든 계절 작물 + 겨울 전용 작물 |
| 계절 전환 시 고사 | 예 (비재배 계절 작물 즉시 Withered) | 아니오 (계절 무관) |
| 날씨 영향 | 있음 (비 = 자동 물주기, 폭풍 = 피해) | 없음 (날씨 차단) |
| 비료 적용 | 가능 | 가능 (동일) |
| 자동 물주기 (물탱크) | 가능 (범위 내) | 불가 (온실 내부에 물탱크 배치 불가) |

### 4.3 계절 보정값

온실에서 재배하는 작물의 성장 속도와 품질은 다음과 같이 보정된다.

**성장 속도 보정**:

| 상황 | 성장 속도 배수 |
|------|-------------|
| 해당 계절 작물을 온실에서 재배 | x1.0 (변동 없음) |
| 비계절 작물을 온실에서 재배 | x0.85 (15% 성장 속도 감소) |
| 겨울 전용 작물을 온실에서 재배 | x1.0 (온실이 본래 환경이므로 패널티 없음) |

**품질 보정**:

| 상황 | 품질 배수 |
|------|----------|
| 해당 계절 작물 | x1.0 |
| 비계절 작물 | x0.9 (고품질 확률 10% 감소) |
| 겨울 전용 작물 | x1.0 |

**설계 의도**: 온실은 "무슨 계절이든 아무 작물이나 최고 효율로 재배"하는 만능 시설이 아니다. 비계절 작물은 소폭 패널티를 받아, "최적 계절에 야외에서 키우는 것"이 여전히 가장 효율적이다. 온실의 핵심 가치는 (1) 겨울에 유일하게 경작할 수 있는 곳, (2) 겨울 전용 작물 3종 재배, (3) 계절 전환 리스크 제거.

### 4.4 겨울 전용 작물 연계

온실에서만 재배할 수 있는 겨울 전용 작물 3종 (-> see `docs/content/crops.md` 섹션 3.9~3.11):

| 작물 | 해금 | 유형 | 핵심 특성 |
|------|------|------|----------|
| 겨울무 | 레벨 5 | 단일 수확, 빠른 회전 | 4일 성장, 입문 겨울 작물 |
| 표고버섯 | 레벨 6 | 다중 수확 | 6일 첫 수확 + 4일 재수확, 안정 수입 |
| 시금치 | 레벨 7 | 단일 수확, 고가 | 8일 성장, 높은 단가 |

**겨울 씨앗 구매 경로 (DES-014 확정)**: 겨울 전용 씨앗은 두 경로로 구매 가능. 상세 가격 및 조건: → see `docs/content/crops.md` 섹션 4.4.

**겨울 온실 운영 전략 (16타일 기준)**:

| 전략 | 타일 배분 | 시즌 예상 순이익 | 특성 |
|------|----------|----------------|------|
| 겨울무 올인 | 16타일 전부 | 2,800G (175G/타일 x 16) | 안정적, 관리 빈번 |
| 표고버섯 올인 | 16타일 전부 | 5,920G (370G/타일 x 16) | 다중 수확 효율 최고 |
| 시금치 올인 | 16타일 전부 | 3,360G (210G/타일 x 16) | 관리 편함, 씨앗 재투자 높음 |
| 혼합 (추천) | 무 6 / 버섯 6 / 시금치 4 | ~3,990G | 리스크 분산 |

[RISK] 표고버섯의 시즌 효율이 다른 겨울 작물 대비 높다. 온실 면적 제한(16타일)이 자연스러운 균형 장치이나, 표고버섯 올인 전략이 지배적일 수 있다. 재수확 간격을 4일 -> 5일로 조정하거나 총 수확 횟수를 제한하는 방안 검토 필요.

### 4.5 온실 내 물주기

- 온실 내부에는 물탱크를 배치할 수 없음 (경작 전용 공간)
- 플레이어가 직접 물뿌리개로 물주기 필요
- 비가 와도 온실 내부는 영향 없음 (플레이어가 매일 물주기 필요)
- 비료는 야외와 동일하게 적용 가능

**설계 의도**: 온실은 강력한 시설이지만, 자동화가 불가능하여 플레이어의 직접 관리를 요구한다. 이는 물탱크와의 기능 분리이며, "온실 + 물탱크 조합으로 완전 자동화"를 방지한다.

### 4.6 업그레이드 경로

| 레벨 | 업그레이드 비용 | 해금 조건 | 효과 |
|------|---------------|----------|------|
| Lv.1 | (기본 건설) | 레벨 5 | 내부 16타일 (4x4) |
| Lv.2 | 3,000G | 레벨 6 | 내부 24타일 (4x6) |
| Lv.3 | 5,000G | 레벨 8 | 내부 36타일 (6x6) |

**총 투자 비용** (Lv.3까지): 2,000G + 3,000G + 5,000G = 10,000G

**설계 근거**: 온실은 게임에서 가장 강력한 시설 중 하나이므로, 업그레이드 비용을 높게 설정하여 장기 투자 목표로 기능하게 한다. Lv.3의 36타일은 야외 소규모 농장에 맞먹는 경작 면적을 제공한다.

### 4.7 사용 방법

| 조작 | 동작 |
|------|------|
| E키 (온실 문 앞) | 온실 내부 진입 |
| E키 (내부 출구) | 온실 외부로 나가기 |
| 내부에서 도구 사용 | 야외와 동일 (호미, 물뿌리개, 씨앗, 낫) |

---

## 5. 창고 (Storage)

### 5.1 기본 정보

| 항목 | 내용 |
|------|------|
| 영문 ID | `building_storage` |
| 건설 비용 / 해금 | (-> see `docs/design.md` 섹션 4.6): 1,000G, 레벨 4 |
| 점유 타일 | 3x2 (6타일) |
| 최대 건설 수 | 3동 |
| 건설 XP | (-> see `docs/balance/progression-curve.md` 섹션 1.2.4): 40 XP |
| 기능 요약 | 수확물 추가 저장 공간 |

### 5.2 저장 메카닉

**슬롯 수 및 확장** (-> see `docs/systems/inventory-system.md` 섹션 2.3, canonical):

| 파라미터 | 값 |
|----------|-----|
| 슬롯 수 (1동당) | 30칸 |
| 최대 건설 수 | 3동 (총 90칸) |
| 추가 건설 비용 | 2동째: 2,000G, 3동째: 4,000G |

### 5.3 창고와 인벤토리의 차이

| 항목 | 배낭 인벤토리 | 창고 |
|------|-------------|------|
| 접근성 | 어디서든 열 수 있음 | 창고 건물 인접에서만 열 수 있음 |
| 이동 중 사용 | 가능 | 불가 |
| 도구 보관 | 가능 (툴바) | 불가 (도구 보관 제한) |
| 용도 | 당장 쓸 아이템 휴대 | 비축, 장기 보관 |
| 슬롯 초기 수 | 15칸 (업그레이드 가능, -> see `docs/systems/inventory-system.md`) | 30칸 (건설 시 고정) |

### 5.4 가격 변동 대응 전략

창고의 핵심 전략적 가치는 수급 변동 시스템(-> see `docs/systems/economy-system.md` 섹션 2.6)과의 연계에 있다.

**활용 시나리오**:

1. **대량 출하 회피**: 같은 작물을 주간 20개 이상 출하하면 가격이 하락한다. 창고에 분산 보관 후 주 단위로 나눠 출하하면 가격 하락을 최소화할 수 있다.
2. **축제 주간 비축**: 가을 수확 축제(Day 18~24, +15% 가격)에 맞춰 작물을 비축했다가 출하하면 최대 수익을 얻을 수 있다.
3. **악천후 후 출하**: 폭풍/폭설 다음 날 가격이 +5~8% 상승하므로, 악천후 예고 시 출하를 다음 날로 미룰 수 있다.
4. **계절 전환 대비**: 다음 계절에 쓸 씨앗이나 비료를 미리 구매하여 보관.

**물류 흐름**:

```
[수확] -> [배낭 인벤토리] -> [창고 보관] -> [적정 시점에 꺼내기]
                                                      |
                                            [출하함에 넣기] 또는 [상점 직접 판매]
```

### 5.5 업그레이드 경로

창고는 업그레이드 대신 **추가 건설**로 확장한다 (-> see `docs/systems/inventory-system.md` 섹션 2.3).

| 동 | 건설 비용 | 슬롯 | 누적 총 슬롯 |
|-----|----------|------|------------|
| 1동째 | 1,000G | 30칸 | 30칸 |
| 2동째 | 2,000G | 30칸 | 60칸 |
| 3동째 | 4,000G | 30칸 | 90칸 |

**총 투자 비용** (3동): 1,000G + 2,000G + 4,000G = 7,000G

**설계 의도**: 창고는 다른 시설과 달리 업그레이드가 아닌 추가 건설 방식이다. 이는 농장 공간(타일) 소비와 직결되어, "저장 공간 vs 경작 면적"의 트레이드오프를 만든다. 3동째 4,000G는 상당한 투자이므로 후반 전략적 결정이 된다.

### 5.6 사용 방법

| 조작 | 동작 |
|------|------|
| E키 (창고 인접) | 창고 UI 열기 (배낭 + 창고 나란히 표시) |
| 좌클릭 드래그 | 배낭 <-> 창고 아이템 이동 |
| Shift + 좌클릭 | 빠른 이동 (반대쪽으로 자동 이동) |

---

## 6. 가공소 (Processing Plant)

### 6.1 기본 정보

| 항목 | 내용 |
|------|------|
| 영문 ID | `building_processing` |
| 건설 비용 / 해금 | (-> see `docs/design.md` 섹션 4.6): 3,000G, 레벨 7 |
| 점유 타일 | 4x3 (12타일) |
| 최대 건설 수 | 1기 (유일) |
| 건설 XP | (-> see `docs/balance/progression-curve.md` 섹션 1.2.4): 60 XP |
| 기능 요약 | 작물을 가공품으로 변환하여 부가가치 창출 |

### 6.2 가공 메카닉

**진입 방식**: 가공소 건물 앞에서 E키 -> 가공 UI 열기

**가공 슬롯** (-> see `docs/systems/economy-system.md` 섹션 2.5, canonical):

| 파라미터 | 값 |
|----------|-----|
| 초기 슬롯 | 1 |
| 최대 슬롯 | 3 |
| 슬롯 확장 비용 | 2번째: 1,500G, 3번째: 3,000G |

**가공 공식** (-> see `docs/systems/economy-system.md` 섹션 2.5, canonical):

```
가공품_판매가 = floor(원재료_기본판매가 * 가공_배수 + 가공_고정보너스)
```

**가공 유형별 파라미터** (-> see `docs/systems/economy-system.md` 섹션 2.5):

| 가공 유형 | 배수 | 고정 보너스 | 가공 시간 | 적용 분류 |
|----------|------|-----------|----------|----------|
| 잼 (Jam) | x2.0 | +50G | 4시간 | 모든 과일/채소 |
| 주스 (Juice) | x2.5 | +30G | 6시간 | 과일류만 |
| 절임 (Pickle) | x2.0 | +30G | 4시간 | 채소류만 |

### 6.3 가공 레시피 목록

#### 6.3.1 잼 레시피

| 레시피 ID | 원재료 | 원재료 수량 | 가공품 | 가공품 판매가 | 가공 시간 |
|----------|--------|-----------|--------|-------------|----------|
| `recipe_jam_potato` | 감자 | 1개 | 감자 잼 | 110G | 4시간 |
| `recipe_jam_carrot` | 당근 | 1개 | 당근 잼 | 120G | 4시간 |
| `recipe_jam_tomato` | 토마토 | 1개 | 토마토 잼 | 170G | 4시간 |
| `recipe_jam_corn` | 옥수수 | 1개 | 옥수수 잼 | 250G | 4시간 |
| `recipe_jam_strawberry` | 딸기 | 1개 | 딸기 잼 | 210G | 4시간 |
| `recipe_jam_pumpkin` | 호박 | 1개 | 호박 잼 | 450G | 4시간 |
| `recipe_jam_watermelon` | 수박 | 1개 | 수박 잼 | 750G | 4시간 |

**판매가 검증** (공식: 기본판매가 x2.0 + 50G):
- 감자: 30 x 2.0 + 50 = 110G
- 당근: 35 x 2.0 + 50 = 120G
- 토마토: 60 x 2.0 + 50 = 170G
- 옥수수: 100 x 2.0 + 50 = 250G
- 딸기: 80 x 2.0 + 50 = 210G
- 호박: 200 x 2.0 + 50 = 450G
- 수박: 350 x 2.0 + 50 = 750G

#### 6.3.2 주스 레시피

| 레시피 ID | 원재료 | 원재료 수량 | 가공품 | 가공품 판매가 | 가공 시간 |
|----------|--------|-----------|--------|-------------|----------|
| `recipe_juice_tomato` | 토마토 | 1개 | 토마토 주스 | 180G | 6시간 |
| `recipe_juice_strawberry` | 딸기 | 1개 | 딸기 주스 | 230G | 6시간 |
| `recipe_juice_watermelon` | 수박 | 1개 | 수박 주스 | 905G | 6시간 |

**판매가 검증** (공식: 기본판매가 x2.5 + 30G):
- 토마토: 60 x 2.5 + 30 = 180G
- 딸기: 80 x 2.5 + 30 = 230G
- 수박: 350 x 2.5 + 30 = 905G

#### 6.3.3 절임 레시피

| 레시피 ID | 원재료 | 원재료 수량 | 가공품 | 가공품 판매가 | 가공 시간 |
|----------|--------|-----------|--------|-------------|----------|
| `recipe_pickle_potato` | 감자 | 1개 | 감자 절임 | 90G | 4시간 |
| `recipe_pickle_carrot` | 당근 | 1개 | 당근 절임 | 100G | 4시간 |
| `recipe_pickle_tomato` | 토마토 | 1개 | 토마토 절임 | 150G | 4시간 |
| `recipe_pickle_corn` | 옥수수 | 1개 | 옥수수 절임 | 230G | 4시간 |
| `recipe_pickle_pumpkin` | 호박 | 1개 | 호박 절임 | 430G | 4시간 |

**판매가 검증** (공식: 기본판매가 x2.0 + 30G):
- 감자: 30 x 2.0 + 30 = 90G
- 당근: 35 x 2.0 + 30 = 100G
- 토마토: 60 x 2.0 + 30 = 150G
- 옥수수: 100 x 2.0 + 30 = 230G
- 호박: 200 x 2.0 + 30 = 430G

#### 6.3.4 겨울 작물 가공 레시피

| 레시피 ID | 원재료 | 가공품 | 유형 | 가공품 판매가 | 비고 |
|----------|--------|--------|------|-------------|------|
| `recipe_pickle_winter_radish` | 겨울무 | 겨울무 절임 | 절임 | 120G | 45 x 2.0 + 30 |
| `recipe_jam_shiitake` | 표고버섯 | 표고버섯 잼 | 잼 | 190G | 70 x 2.0 + 50 |
| `recipe_pickle_spinach` | 시금치 | 시금치 절임 | 절임 | 290G | 130 x 2.0 + 30 |

**겨울 작물 분류**: 겨울무(채소), 표고버섯(채소/특수), 시금치(채소). 모두 채소로 분류하여 잼 또는 절임 가능. 주스는 과일류만 가공 가능하므로 겨울 작물에는 해당 없음.

### 6.4 가공 효율 분석

**직판 vs 가공 비교** (Normal 품질 기준):

| 작물 | 직판가 | 잼 판매가 | 주스 판매가 | 절임 판매가 | 최적 가공 | 순이익 증가분 |
|------|--------|----------|-----------|-----------|----------|-------------|
| 감자 | 30G | 110G | - | 90G | 잼 | +80G |
| 당근 | 35G | 120G | - | 100G | 잼 | +85G |
| 토마토 | 60G | 170G | 180G | 150G | 주스 | +120G |
| 옥수수 | 100G | 250G | - | 230G | 잼 | +150G |
| 딸기 | 80G | 210G | 230G | - | 주스 | +150G |
| 호박 | 200G | 450G | - | 430G | 잼 | +250G |
| 수박 | 350G | 750G | 905G | - | 주스 | +555G |

**가공 시간 대비 수익 효율** (시간당 순이익 증가분):

| 가공품 | 순이익 증가분 | 가공 시간 | 시간당 효율 |
|--------|-------------|----------|-----------|
| 감자 잼 | +80G | 4h | 20G/h |
| 당근 잼 | +85G | 4h | 21.25G/h |
| 토마토 주스 | +120G | 6h | 20G/h |
| 옥수수 잼 | +150G | 4h | 37.5G/h |
| 딸기 주스 | +150G | 6h | 25G/h |
| 호박 잼 | +250G | 4h | 62.5G/h |
| 수박 주스 | +555G | 6h | 92.5G/h |

**결론**: 고가 작물(수박, 호박)을 가공하는 것이 시간 대비 가장 효율적이다. 저가 작물(감자, 당근)은 가공해도 절대적 수익 증가분이 적으므로, 슬롯이 제한된 상황에서 고가 작물에 슬롯을 할당하는 것이 최적이다. 이는 "어떤 작물을 가공할지"에 대한 의미 있는 선택을 만든다.

### 6.5 품질과 가공의 관계

가공품 판매가는 원재료 품질에 **영향받지 않는다** (-> see `docs/systems/economy-system.md` 섹션 2.5).

| 상황 | 최적 행동 |
|------|----------|
| Normal 품질 작물 | 가공 추천 (직판가가 낮으므로 가공 이득이 큼) |
| Silver 품질 작물 | 가공 vs 직판 비교 필요 (작물에 따라 다름) |
| Gold/Iridium 품질 작물 | 직판 추천 (품질 배수 x1.5~x2.0이 가공보다 유리할 수 있음) |

**Gold 품질 직판 vs 가공 비교 예시**:
- Gold 수박 직판: 350G x 1.5 = 525G
- 수박 주스: 905G -> 가공이 여전히 유리 (+380G)
- Gold 감자 직판: 30G x 1.5 = 45G
- 감자 잼: 110G -> 가공이 유리 (+65G)

**결론**: 현재 가공 배수(x2.0~x2.5 + 고정 보너스)가 Gold 품질 배수(x1.5)보다 높으므로, 대부분의 경우 가공이 직판보다 유리하다. Iridium 품질(x2.0)에서만 일부 작물에서 직판이 유리해진다.

[RISK] 가공이 거의 항상 직판보다 유리한 것은 "가공 vs 직판" 트레이드오프를 약화시킨다. 가공 배수를 x1.5~x2.0으로 낮추거나, 가공 시간을 늘려 기회비용을 높이는 조정을 검토해야 한다. 또는 가공품에도 수급 변동을 적용하여 대량 가공 출하 시 가격 하락이 발생하게 할 수 있다.

### 6.6 업그레이드 경로

가공소 자체의 업그레이드는 슬롯 확장으로 이루어진다 (-> see `docs/systems/economy-system.md` 섹션 2.5).

추가로, 가공소 레벨 업그레이드를 통해 가공 속도를 개선할 수 있다.

| 레벨 | 업그레이드 비용 | 해금 조건 | 효과 |
|------|---------------|----------|------|
| Lv.1 | (기본 건설) | 레벨 7 | 가공 시간 x1.0 (기본) |
| Lv.2 | 4,000G | 레벨 8 | 가공 시간 x0.75 (25% 단축) |
| Lv.3 | 7,000G | 레벨 9 | 가공 시간 x0.5 (50% 단축) |

**총 투자 비용** (Lv.3 + 슬롯 3개): 3,000G + 4,000G + 7,000G + 1,500G + 3,000G = 18,500G

**Lv.3 가공 시간 예시**:
- 잼: 4시간 -> 2시간
- 주스: 6시간 -> 3시간
- 절임: 4시간 -> 2시간

**설계 의도**: 가공소는 게임에서 가장 비싼 시설(총 18,500G)이며, 레벨 7~9에 걸쳐 점진적으로 완성된다. 이는 엔드게임의 장기 투자 목표이자, 최고 효율 달성을 위한 최종 목표 중 하나이다.

### 6.7 사용 방법

| 조작 | 동작 |
|------|------|
| E키 (가공소 인접) | 가공 UI 열기 |
| 가공 UI: 슬롯에 작물 드래그 | 가공 유형 선택 메뉴 표시 |
| 가공 유형 선택 | 가공 시작 (진행 바 표시) |
| 가공 완료 시 | 가공품이 슬롯에 생성됨, 클릭하여 배낭으로 회수 |

**가공 UI 표시 내용**:
- 가공 슬롯 (1~3개, 확장에 따라)
- 각 슬롯: 입력 재료 + 진행 바 + 완성 시 가공품 아이콘
- 레시피 목록 (현재 소지 작물 기준으로 가공 가능 레시피 필터링)
- 예상 판매가 표시

---

## 7. 제분소 (Mill)

### 7.1 기본 정보

| 항목 | 내용 |
|------|------|
| 영문 ID | `building_mill` |
| 건설 비용 / 해금 | (-> see `docs/design.md` 섹션 4.6): 1,500G, 레벨 5 |
| 점유 타일 | 3x2 (6타일) |
| 최대 건설 수 | 1기 (유일) |
| 건설 XP | (-> see `docs/balance/progression-curve.md` 섹션 1.2.4): 30 XP |
| 기능 요약 | 곡물/작물을 가루 형태로 분쇄하여 중간 가공재 생산 |

### 7.2 가공 메카닉

**진입 방식**: 제분소 건물 앞에서 E키 → 제분 UI 열기

**가공 슬롯**: 1 (고정, 확장 불가)

**레시피 목록** (-> see `docs/content/processing-system.md` 섹션 3.2 for canonical 레시피 목록 및 판매가)

### 7.3 업그레이드 경로

제분소는 슬롯이 1개로 고정이며, 업그레이드는 가공 속도 개선만 제공한다.

| 레벨 | 업그레이드 비용 | 해금 조건 | 효과 |
|------|---------------|----------|------|
| Lv.1 | (기본 건설) | 레벨 5 | 가공 시간 x1.0 (기본) |
| Lv.2 | 2,000G | 레벨 6 | 가공 시간 x0.75 (25% 단축) |
| Lv.3 | 3,500G | 레벨 8 | 가공 시간 x0.5 (50% 단축) |

**총 투자 비용** (Lv.3까지): 1,500G + 2,000G + 3,500G = 7,000G

**설계 의도**: 제분소는 베이커리의 선행 시설이다. Lv.5 해금으로 온실과 동시에 열리며, 옥수수·버섯 가루를 생산하여 베이커리 레시피에 공급한다. 단일 슬롯이므로 레시피 우선순위 선택이 중요하다.

### 7.4 사용 방법

| 조작 | 동작 |
|------|------|
| E키 (제분소 인접) | 제분 UI 열기 |
| 슬롯에 작물 드래그 | 가능한 레시피 표시 |
| 레시피 선택 | 제분 시작 (진행 바 표시) |
| 완료 시 | 가루 아이템 생성, 클릭하여 배낭으로 회수 |

---

## 8. 발효실 (Fermentation Room)

### 8.1 기본 정보

| 항목 | 내용 |
|------|------|
| 영문 ID | `building_fermentation` |
| 건설 비용 / 해금 | (-> see `docs/design.md` 섹션 4.6): 4,000G, 레벨 8 |
| 점유 타일 | 3x3 (9타일) |
| 최대 건설 수 | 1기 (유일) |
| 건설 XP | (-> see `docs/balance/progression-curve.md` 섹션 1.2.4): 60 XP |
| 기능 요약 | 장시간 발효를 통해 고가 가공품(와인/식초/된장) 생산 |

### 8.2 가공 메카닉

**진입 방식**: 발효실 건물 앞에서 E키 → 발효 UI 열기

**가공 슬롯**: 2 (고정, 확장 불가)

**핵심 특징**: 가공 시간이 12~24시간으로 매우 길지만, 판매 수익이 그만큼 크다.

**레시피 목록** (-> see `docs/content/processing-system.md` 섹션 3.3 for canonical 레시피 목록 및 판매가)

### 8.3 업그레이드 경로

| 레벨 | 업그레이드 비용 | 해금 조건 | 효과 |
|------|---------------|----------|------|
| Lv.1 | (기본 건설) | 레벨 8 | 가공 시간 x1.0 (기본) |
| Lv.2 | 5,000G | 레벨 9 | 가공 시간 x0.75 (25% 단축) |
| Lv.3 | 8,000G | 레벨 10 | 가공 시간 x0.5 (50% 단축), 슬롯 +1 (총 3) |

**총 투자 비용** (Lv.3까지): 4,000G + 5,000G + 8,000G = 17,000G

**설계 의도**: 발효실은 "즉시 현금화(잼/주스) vs 장기 투자(와인/식초)"의 트레이드오프를 형성한다. 수박 와인 1,200G는 게임 내 단일 가공품 최고가 중 하나이지만, 24시간을 기다려야 한다. Lv.3 업그레이드 시 슬롯이 3개가 되어 장기 운영 효율이 극대화된다.

### 8.4 사용 방법

| 조작 | 동작 |
|------|------|
| E키 (발효실 인접) | 발효 UI 열기 |
| 슬롯에 작물 드래그 | 발효 가능 레시피 표시 |
| 레시피 선택 | 발효 시작 (남은 시간 시:분 표시) |
| 완료 시 | 발효품 생성, 클릭하여 배낭으로 회수 |

**주의**: 발효 중에는 중단할 수 없다. 슬롯을 잘못 배치하면 24시간을 낭비할 수 있다.

---

## 9. 베이커리 (Bakery)

### 9.1 기본 정보

| 항목 | 내용 |
|------|------|
| 영문 ID | `building_bakery` |
| 건설 비용 / 해금 | (-> see `docs/design.md` 섹션 4.6): 5,000G, 레벨 9 |
| 점유 타일 | 4x3 (12타일) |
| 최대 건설 수 | 1기 (유일) |
| 건설 XP | (-> see `docs/balance/progression-curve.md` 섹션 1.2.4): 80 XP |
| 기능 요약 | 가공 중간재 + 작물을 조합하여 최상위 가공품 생산 (연료 소모) |

### 9.2 가공 메카닉

**진입 방식**: 베이커리 건물 앞에서 E키 → 베이킹 UI 열기

**가공 슬롯**: 2 (고정, 확장 불가)

**연료 시스템**: 베이커리는 유일하게 연료(장작)를 소모한다.

| 아이템 | 연료 소모량 | 구매처 |
|--------|-----------|--------|
| 장작 (`item_firewood`) | 빵류 1~2개, 케이크류 2~3개 | 잡화상(하나), 30G/개 (-> see `docs/content/npcs.md`) |

**레시피 목록** (-> see `docs/content/processing-system.md` 섹션 3.4 for canonical 레시피 목록, 판매가, 연료 소모량)

**로열 타르트 가공 체인**: 옥수수(제분소→옥수수 가루) + 호박(가공소→호박 잼) + 딸기 → 베이커리 → 로열 타르트 2,100G (-> see `docs/content/processing-system.md` 섹션 3.4)

### 9.3 업그레이드 경로

| 레벨 | 업그레이드 비용 | 해금 조건 | 효과 |
|------|---------------|----------|------|
| Lv.1 | (기본 건설) | 레벨 9 | 가공 시간 x1.0, 슬롯 2 |
| Lv.2 | 6,000G | 레벨 10 | 가공 시간 x0.75, 연료 소모 -1 (최솟값 1) |
| Lv.3 | 10,000G | 레벨 11 | 가공 시간 x0.5, 슬롯 +1 (총 3) |

**총 투자 비용** (Lv.3까지): 5,000G + 6,000G + 10,000G = 21,000G

**설계 의도**: 베이커리는 가장 늦게 해금되는 최상위 가공소다. 로열 타르트(2,100G) 단일 가공품이 게임 내 최고가이며, 3단계 가공 체인(제분소 → 가공소 → 베이커리)을 완성해야만 얻을 수 있다. 연료 소모가 추가 운영 비용을 만들어 "연료 구매 vs 수익"의 트레이드오프를 형성한다.

### 9.4 사용 방법

| 조작 | 동작 |
|------|------|
| E키 (베이커리 인접) | 베이킹 UI 열기 |
| 슬롯에 중간재 드래그 | 가능한 레시피 표시 (연료 잔량 확인) |
| 레시피 선택 | 베이킹 시작 (연료 소모, 진행 바 표시) |
| 완료 시 | 가공품 생성, 클릭하여 배낭으로 회수 |

**연료 부족 시**: 레시피 시작 불가. 잡화상에서 장작 구매 후 시도.

---

## 10. 향후 확장 후보

아래 시설은 MVP 이후 콘텐츠 확장 시 고려할 후보이다. 현재 설계/구현 범위에 포함되지 않는다.

### 10.1 닭장 (Chicken Coop)

| 항목 | 구상 |
|------|------|
| 기능 | 닭 사육, 매일 달걀 수확, 달걀 판매/가공 |
| 예상 비용 | 2,500G |
| 예상 해금 | 레벨 6 |
| 게임 루프 확장 | 작물 외 수입원 다양화, 사료 관리라는 새 자원 소비 루프 |
| 우선순위 근거 | 작물 시스템이 충분히 안정화된 후 추가 |

### 10.2 벌통 (Beehive)

| 항목 | 구상 |
|------|------|
| 기능 | 꿀 자동 생산 (봄~가을), 인접 꽃 작물에 따라 꿀 종류 변화 |
| 예상 비용 | 1,500G |
| 예상 해금 | 레벨 5 |
| 게임 루프 확장 | 패시브 수입원, 꽃 재배와의 시너지 |
| 우선순위 근거 | 해바라기(꽃 작물)와의 연계가 자연스러움 |

### 10.3 우물 (Well)

| 항목 | 구상 |
|------|------|
| 기능 | 물뿌리개 리필 지점 (농장 내, 연못 대체) |
| 예상 비용 | 300G |
| 예상 해금 | 레벨 2 |
| 게임 루프 확장 | 동선 최적화, 초반 편의성 |
| 우선순위 근거 | 매우 간단한 시설, 초반 해금으로 적합 |

[OPEN] 향후 확장 시설의 우선순위 결정. 닭장은 새로운 시스템(사육)을 요구하므로 복잡도가 높고, 벌통과 우물은 기존 시스템 확장이므로 상대적으로 단순하다.

---

## Cross-references

| 문서 | 관계 |
|------|------|
| `docs/design.md` 섹션 4.6 | 시설 기본 비용/해금 canonical |
| `docs/systems/economy-system.md` 섹션 2.5 | 가공품 가격 공식, 가공소 슬롯 canonical |
| `docs/systems/economy-system.md` 섹션 3.3 | 목공소 인벤토리 (시설 건설 가격 목록) |
| `docs/systems/inventory-system.md` 섹션 2.3 | 창고 슬롯/확장 canonical |
| `docs/systems/farming-system.md` | 타일 상태, 물주기 메카닉, 에너지 소모 |
| `docs/systems/time-season.md` | 날씨 종류, 비 효과, 계절 전환 |
| `docs/systems/crop-growth.md` | 성장 보정, 온실 계절 보정 |
| `docs/content/crops.md` 섹션 3.9~3.11 | 겨울 전용 작물 3종 |
| `docs/balance/crop-economy.md` | 작물 ROI, 가공 수익 분석 |
| `docs/balance/progression-curve.md` 섹션 1.2.4 | 시설 건설 XP |
| `docs/content/processing-system.md` | 특화 가공소 3종 레시피 canonical (섹션 3.2~3.4) |
| `docs/systems/processing-architecture.md` | 가공 시스템 기술 아키텍처 |
| `docs/content/npcs.md` | 장작 판매 NPC (잡화상 하나) |

---

## Open Questions ([OPEN])

1. **[OPEN]** 자원 채집 시스템 도입 시 시설 건설에 목재/석재 등 추가 재료 요건을 넣을지 검토. 현재 MVP에서는 골드만으로 충분하나, 게임 깊이 측면에서 추가 재료가 의미 있을 수 있다. (섹션 2.4)

2. **[OPEN]** 향후 확장 시설(닭장, 벌통, 우물)의 우선순위 결정. 복잡도와 게임 루프 확장 기여도를 기준으로 판단 필요. (섹션 10)

3. **[OPEN]** 물탱크 다중 배치 시 범위 겹침 처리. 두 물탱크의 범위가 겹치는 타일에 대해 저수량을 이중 소비할지, 한 쪽에서만 소비할지 결정 필요. 제안: 한 쪽에서만 소비 (먼저 등록된 물탱크 우선).

4. **[OPEN]** 온실 Lv.3 (36타일) 확장 시 외부 점유 타일도 증가하는지, 아니면 내부만 확장되는지 결정 필요. 현재 설계: 외부 크기 6x6 고정, 내부 공간만 효율적으로 확장 (구조물 벽 두께 감소로 표현).

5. **[OPEN]** 가공소 슬롯 확장에 레벨 해금 조건을 추가할지 검토 (-> see `docs/systems/economy-system.md` 섹션 2.5의 동일 OPEN 항목).

---

## Risks ([RISK])

1. **[RISK]** 표고버섯 올인 전략이 겨울 온실에서 지배적일 수 있다. 재수확 간격 또는 총 수확 횟수 조정 검토 필요. (섹션 4.4)

2. **[RISK]** 가공이 거의 항상 직판보다 유리한 현재 밸런스는 트레이드오프를 약화시킨다. 가공 배수 하향, 가공 시간 연장, 또는 가공품 수급 변동 도입 검토 필요. (섹션 6.5)

3. **[RISK]** 온실 + 가공소 조합(겨울 전용 작물 재배 + 가공)이 겨울 수입을 과도하게 높일 수 있다. 겨울이 "쉬는 계절"이 아닌 "가장 수익적인 계절"이 되지 않도록 밸런스 검증 필요.

4. **[RISK]** 물탱크 Lv.3 (40타일 범위)가 에너지 절약 관점에서 너무 강력할 수 있다. 물탱크 3기 x Lv.3 = 최대 120타일 자동 물주기로, 물주기 에너지가 거의 무의미해질 수 있다. 물탱크 최대 건설 수 또는 범위 상한 재검토 필요.

---

*이 문서는 Claude Code가 SeedMind 프로젝트의 시설 콘텐츠를 상세 설계하여 자율적으로 작성했습니다.*

---

---
title: 낚시 도감 콘텐츠 상세
date: 2026-04-07
author: Claude Code (Opus 4.6)
---

# 낚시 도감 콘텐츠 상세 (Fish Catalog Content Specification)

> 작성: Claude Code (Opus 4.6) | 2026-04-07
> 문서 ID: CON-011

---

## 1. Context

이 문서는 SeedMind의 낚시 도감(Fish Catalog) 콘텐츠를 상세히 정의한다. 15종 어종 각각의 도감 항목(힌트 텍스트, 크기 범위, 희귀도 등급, 포획 보상), 도감 완성 마일스톤 보상, 크기 시스템 규칙, 도감 UI 플레이어 경험을 포함한다.

**설계 목표**: 도감은 "수집의 즐거움"을 핵심으로 한다. 단순히 잡은 물고기를 나열하는 것이 아니라, 미발견 어종에 대한 힌트를 제공하여 탐색 동기를 부여하고, 크기 기록 갱신과 마일스톤 보상으로 반복 낚시의 목적성을 강화한다.

### 1.1 본 문서가 canonical인 데이터

- 15종 어종 도감 항목 (힌트 텍스트, 크기 범위, 도감 등록 시 보상)
- 크기 시스템 규칙 (크기 등급 분류, 판매가 보정률)
- 도감 완성 마일스톤 보상 (5/10/15종)
- 도감 UI 상태 정의 (잠금/발견/크기 기록)

### 1.2 본 문서가 canonical이 아닌 데이터 (참조만)

| 데이터 종류 | 참조처 |
|------------|--------|
| 어종 목록 (이름, 영문 ID, 희귀도, 기본 판매가, 계절/시간/날씨 조건) | `docs/systems/fishing-system.md` 섹션 4.2 |
| Giant 변이 규칙 (발생 확률, 판매가 배수) | `docs/systems/fishing-system.md` 섹션 4.4 |
| 품질 시스템 (Normal/Silver/Gold/Iridium 판매가 배수) | `docs/systems/fishing-system.md` 섹션 6.1 |
| 낚시 숙련도 레벨, XP 획득량 | `docs/systems/fishing-system.md` 섹션 7 |
| 낚시 업적 (ach_fish_01~04) 보상 | `docs/content/achievements.md` 섹션 9 |
| 경제 수치 (초기 골드, 마진 비율) | `docs/systems/economy-system.md` |
| 칭호 canonical 목록 | `docs/content/achievements.md` 섹션 11 |
| FishData SO 필드 정의 | `docs/systems/fishing-architecture.md` 섹션 9 |

---

## 2. 크기 시스템

### 2.1 설계 원칙

같은 어종이라도 포획할 때마다 크기가 다르다. 크기는 판매가에 직접 영향을 주며, 도감에 최대 크기 기록이 갱신된다. 이는 "같은 물고기를 또 잡을 이유"를 부여하는 핵심 메카닉이다.

### 2.2 크기 결정 공식

포획 성공 시, 해당 어종의 `sizeMin`~`sizeMax` 범위에서 가중 랜덤으로 크기를 결정한다.

```
catchSize = sizeMin + (sizeMax - sizeMin) * weightedRandom()
```

**가중 분포**: 소형 쪽에 치우친 분포를 사용한다. `weightedRandom()` = `random(0~1)^1.3` (지수 1.3으로 소형 편향). 대형 개체는 자연스럽게 희소해진다.

### 2.3 크기 등급 분류

크기 범위를 3등급으로 나누어 도감 표시 및 판매가 보정에 사용한다.

| 크기 등급 | 영문 키 | 범위 기준 | 판매가 보정률 | 도감 표시 |
|----------|---------|----------|-------------|----------|
| 소형 | `Small` | sizeMin ~ sizeMin + range*0.5 | x0.9 | 작은 물고기 아이콘 |
| 중형 | `Medium` | sizeMin + range*0.5 ~ sizeMin + range*0.8 | x1.0 (기본) | 기본 물고기 아이콘 |
| 대형 | `Large` | sizeMin + range*0.8 ~ sizeMax | x1.15 | 큰 물고기 아이콘 + 반짝임 |

- `range` = `sizeMax - sizeMin`
- 가중 랜덤 분포 기준 대략적 출현 비율: 소형 ~55%, 중형 ~30%, 대형 ~15%

**Giant 변이와의 관계**: Giant 변이(-> see `docs/systems/fishing-system.md` 섹션 4.4)는 크기 등급과 별개의 판정이다. Giant 판정이 먼저 이루어지고(5% 확률), Giant가 아닌 경우에만 일반 크기 결정이 적용된다. Giant 개체의 크기는 `sizeMax * 1.5`로 고정되며, 도감에는 "Giant" 태그와 함께 별도 기록된다.

### 2.4 최종 판매가 공식

```
최종 판매가 = 기본 판매가 x 품질 배수 x 크기 보정률 x Giant 배수 x 수급 배수
```

- 기본 판매가: (-> see `docs/systems/fishing-system.md` 섹션 4.2)
- 품질 배수: (-> see `docs/systems/fishing-system.md` 섹션 6.1)
- 크기 보정률: 섹션 2.3의 테이블
- Giant 배수: x2.0 (Giant일 때) / x1.0 (일반) (-> see `docs/systems/fishing-system.md` 섹션 4.4)
- 수급 배수: (-> see `docs/systems/economy-system.md` 섹션 2.6.2.1)

**예시**: 잉어(기본 30G) + Gold 품질(x1.5) + 대형(x1.15) = 30 x 1.5 x 1.15 = 51G (소수점 이하 버림)

[OPEN] 크기 보정률과 품질 배수가 곱셈으로 중첩되면 최고 조건(Iridium + Large + Giant)에서 극단적 가격이 발생할 수 있다. 예: 연꽃 잉어(800G) x Iridium(x2.0) x Large(x1.15) x Giant(불가) = 1,840G. Giant 불가 어종이므로 문제없지만, 황금 잉어(200G) x Iridium(x2.0) x Large(x1.15) x Giant(x2.0) = 920G는 검토 필요. 밸런스 시트(-> see `docs/balance/fishing-economy.md`)에서 시뮬레이션 확인 필요.

---

## 3. 15종 어종 도감 항목

모든 어종의 이름, 영문 ID, 희귀도, 기본 판매가, 계절/시간대/날씨 조건은 `docs/systems/fishing-system.md` 섹션 4.2를 canonical로 참조한다. 본 섹션은 도감 고유 데이터(크기 범위, 힌트 텍스트, 초회 포획 보상)만 정의한다.

### 3.1 도감 항목 테이블

| # | 어종명 | 영문 ID | 희귀도 | sizeMin (cm) | sizeMax (cm) | Giant 크기 (cm) | 초회 등록 보상 |
|---|--------|---------|--------|:------------:|:------------:|:--------------:|--------------|
| 1 | 붕어 | `fish_crucian_carp` | Common | 10 | 25 | - | 5G + 2 XP |
| 2 | 미꾸라지 | `fish_loach` | Common | 8 | 20 | - | 5G + 2 XP |
| 3 | 잉어 | `fish_carp` | Common | 20 | 50 | 75 | 5G + 2 XP |
| 4 | 메기 | `fish_catfish` | Uncommon | 25 | 60 | - | 15G + 5 XP |
| 5 | 송어 | `fish_trout` | Uncommon | 20 | 45 | - | 15G + 5 XP |
| 6 | 가재 | `fish_crawfish` | Uncommon | 5 | 15 | - | 15G + 5 XP |
| 7 | 블루길 | `fish_bluegill` | Common | 10 | 22 | - | 5G + 2 XP |
| 8 | 뱀장어 | `fish_eel` | Uncommon | 30 | 80 | - | 15G + 5 XP |
| 9 | 빙어 | `fish_smelt` | Common | 5 | 12 | - | 5G + 2 XP |
| 10 | 산천어 | `fish_cherry_salmon` | Rare | 15 | 35 | 52 | 50G + 15 XP |
| 11 | 황금 잉어 | `fish_golden_carp` | Rare | 20 | 45 | 67 | 50G + 15 XP |
| 12 | 전설의 메기왕 | `fish_legend_catfish` | Legendary | 80 | 150 | - | 200G + 50 XP |
| 13 | 무지개송어 | `fish_rainbow_trout` | Rare | 20 | 50 | 75 | 50G + 15 XP |
| 14 | 연꽃 잉어 | `fish_lotus_koi` | Legendary | 30 | 70 | - | 200G + 50 XP |
| 15 | 얼음 빙어왕 | `fish_ice_king_smelt` | Rare | 10 | 25 | - | 50G + 15 XP |

**Giant 크기**: Giant 변이가 가능한 어종(잉어, 산천어, 황금 잉어, 무지개송어)만 해당. Giant 크기 = sizeMax x 1.5 (-> see `docs/systems/fishing-system.md` 섹션 4.4).

**초회 등록 보상 기준**: 도감에 해당 어종이 처음 등록될 때 1회 지급. 희귀도별 차등:
- Common: 5G + 2 XP
- Uncommon: 15G + 5 XP
- Rare: 50G + 15 XP
- Legendary: 200G + 50 XP

**초회 보상 총합**: 전 15종 등록 시 골드 625G, XP 162. 이는 업적 보상(-> see `docs/content/achievements.md` 섹션 9)과 별도로 지급되며, `ach_fish_04` (도감 완성 1,000G + 100 XP)와 합산하면 도감 완성 총 보상은 1,625G + 262 XP이다.

### 3.2 도감 힌트 텍스트

각 어종은 "미발견 힌트"와 "발견 후 설명" 두 가지 텍스트를 가진다.

#### 3.2.1 미발견 힌트 (어종을 아직 잡지 못한 상태)

미발견 힌트는 계절, 시간대, 날씨 조건을 간접적으로 암시하여 플레이어에게 탐색 방향을 제시한다. 구체적인 수치나 조건명은 노출하지 않는다.

| # | 어종명 | 미발견 힌트 텍스트 |
|---|--------|-------------------|
| 1 | 붕어 | "연못 어디서나 만날 수 있는 친근한 물고기. 초보 낚시꾼의 첫 친구." |
| 2 | 미꾸라지 | "따뜻한 계절의 낮에 활발하게 움직이는 작은 물고기." |
| 3 | 잉어 | "연못의 터줏대감. 언제든 만날 수 있지만, 정말 큰 녀석은 드물다." |
| 4 | 메기 | "비가 내리는 저녁, 어둠 속에서 수염을 흔드는 그림자." |
| 5 | 송어 | "새벽 안개가 걷힐 무렵, 맑은 물 위로 뛰어오르는 은빛 비늘." |
| 6 | 가재 | "한여름 오후, 연못 바닥에서 집게를 들고 기다리는 녀석." |
| 7 | 블루길 | "여름부터 가을까지 낮 시간에 모습을 보이는 알록달록한 작은 물고기." |
| 8 | 뱀장어 | "비 오는 가을밤, 어둠 속에서만 모습을 드러내는 긴 몸의 물고기." |
| 9 | 빙어 | "얼어붙은 연못 아래, 겨울 아침의 차가운 빛 속에서 반짝이는 작은 생명." |
| 10 | 산천어 | "봄의 첫 새벽, 맑은 하늘 아래에서만 나타나는 아름다운 점무늬 물고기." |
| 11 | 황금 잉어 | "한여름 맑은 낮, 수면 아래에서 금빛으로 빛나는 전설 같은 잉어." |
| 12 | 전설의 메기왕 | "가을 폭풍이 몰아치는 깊은 밤, 연못의 주인이 깨어난다고 한다..." |
| 13 | 무지개송어 | "흐린 가을 아침, 빗방울 사이로 일곱 빛깔 비늘이 스쳐 지나간다." |
| 14 | 연꽃 잉어 | "한여름 새벽, 연꽃이 피어날 때만 수면 위로 올라온다는 신비로운 잉어." |
| 15 | 얼음 빙어왕 | "눈 내리는 겨울 새벽, 얼음 아래 깊은 곳에서 빛나는 거대한 빙어." |

#### 3.2.2 발견 후 설명 (어종을 1회 이상 잡은 상태)

발견 후에는 구체적인 등장 조건이 공개되며, 어종의 세계관 설정도 포함된다.

| # | 어종명 | 발견 후 설명 텍스트 |
|---|--------|-------------------|
| 1 | 붕어 | "연못에서 가장 흔하게 볼 수 있는 민물고기. 봄부터 가을까지 시간과 날씨를 가리지 않고 잡힌다. 질리지 않는 담백한 맛이 특징." |
| 2 | 미꾸라지 | "봄과 여름 아침부터 저녁까지 활발하게 돌아다닌다. 작지만 쫄깃한 식감으로 가공하면 제법 괜찮은 요리가 된다." |
| 3 | 잉어 | "연못의 대표 어종. 봄~가을 언제든 잡을 수 있다. 가끔 놀랄 만큼 큰 개체가 걸리기도 한다. [Giant 변이 가능]" |
| 4 | 메기 | "비가 오는 여름~가을 저녁부터 밤에 활동한다. 긴 수염으로 먹이를 찾는 야행성 사냥꾼." |
| 5 | 송어 | "봄과 가을의 맑은 새벽~아침에 모습을 보인다. 깨끗한 물을 좋아하는 까다로운 성격." |
| 6 | 가재 | "여름 오후~저녁에만 잡히는 갑각류. 물고기는 아니지만 도감에는 포함된다. 식감이 좋아 가공 재료로 인기." |
| 7 | 블루길 | "여름과 가을 아침~오후에 잡히는 작은 물고기. 화려한 색상 덕분에 도감 수집가들에게 인기." |
| 8 | 뱀장어 | "가을 비 오는 밤에만 모습을 드러내는 신비한 물고기. 높은 가격에 판매할 수 있다." |
| 9 | 빙어 | "겨울 새벽~아침, 맑거나 눈 오는 날에 잡히는 겨울 전용 어종. 얼음 낚시의 기본." |
| 10 | 산천어 | "봄 새벽, 맑은 날에만 나타나는 희귀한 물고기. 등에 아름다운 점무늬가 있다. [Giant 변이 가능]" |
| 11 | 황금 잉어 | "맑은 여름 낮에만 모습을 보이는 금빛 잉어. 전설에 따르면 행운을 가져다준다고 한다. [Giant 변이 가능]" |
| 12 | 전설의 메기왕 | "가을 폭풍의 밤(20:00~24:00)에만 나타나는 연못의 전설. 엄청난 크기와 힘으로 숙련된 낚시꾼도 놀라게 한다." |
| 13 | 무지개송어 | "흐리거나 비 오는 가을 새벽~아침에 잡힌다. 비늘이 무지개빛으로 반짝여 이름 붙었다. [Giant 변이 가능]" |
| 14 | 연꽃 잉어 | "맑은 여름 새벽(06:00~08:00)에만 나타나는 최고 희귀 어종. 연꽃 무늬가 온몸에 펼쳐져 있다." |
| 15 | 얼음 빙어왕 | "눈 내리는 겨울 새벽에만 잡히는 거대한 빙어. 얼음 아래서 푸르게 빛나는 몸이 신비롭다." |

---

## 4. 도감 완성 마일스톤 보상

도감 등록 종 수에 따라 단계별 마일스톤 보상을 지급한다. 이 보상은 초회 등록 보상(섹션 3.1) 및 업적 보상(-> see `docs/content/achievements.md` 섹션 9)과 별도로 지급된다.

### 4.1 마일스톤 테이블

| 마일스톤 | 필요 종 수 | 골드 | XP | 특수 보상 | 비고 |
|----------|:--------:|:----:|:---:|----------|------|
| 초보 수집가 | 5종 | 100G | 30 XP | 미끼 x10 | 기본 어종 위주로 자연스럽게 달성 |
| 숙련 수집가 | 10종 | 300G | 80 XP | 고급 미끼 x5 + 도감 배경 "연못의 사계절" | Uncommon/Rare 포함 필요 |
| 도감 마스터 | 15종 (전종) | 500G | 150 XP | 도감 배경 "전설의 연못" + 도감 프레임 "황금 물결" | Legendary 2종 포함 필수 |

**설계 의도**:
- 5종 마일스톤은 Zone F 해금 후 1~2주(게임 내) 이내 달성 가능. Common 5종만으로도 달성 가능하므로 즉각적 보상감 부여.
- 10종 마일스톤은 다양한 계절/시간대/날씨 조건 탐색을 요구. 2~3계절 플레이 필요.
- 15종(전종) 마일스톤은 Legendary 2종(전설의 메기왕, 연꽃 잉어)의 극히 제한적 등장 조건 충족 필요. 사실상 게임 후반 장기 목표.

**마일스톤 보상 총합**: 골드 900G, XP 260. 도감 초회 등록 보상(625G, 162 XP)과 합산 시 도감 시스템 자체 보상 = 1,525G + 422 XP.

### 4.2 미끼 아이템 사양

마일스톤 보상으로 지급되는 미끼 아이템의 기본 사양이다.

| 아이템 | 영문 ID | 효과 | 소모 |
|--------|---------|------|------|
| 미끼 | `bait_normal` | 바이트 대기 시간 -20% | 낚시 1회 시도 시 1개 소모 |
| 고급 미끼 | `bait_premium` | 바이트 대기 시간 -30% + Uncommon 이상 출현 확률 +10% | 낚시 1회 시도 시 1개 소모 |

[OPEN] 미끼 시스템의 전체 설계(상점 판매 가격, 제작 레시피, 추가 미끼 종류)는 `docs/systems/fishing-system.md`에 미반영 상태. 도감 마일스톤 보상으로 미끼를 지급하는 것은 확정하되, 미끼 시스템 전체 설계는 별도 작업(TODO 등록 필요)으로 분리한다.

### 4.3 도감 장식 보상

| 아이템 | 영문 ID | 카테고리 | 효과 |
|--------|---------|---------|------|
| 도감 배경: 연못의 사계절 | `catalog_bg_four_seasons` | 도감 UI 커스텀 배경 | 도감 화면 배경이 사계절 연못 일러스트로 변경 |
| 도감 배경: 전설의 연못 | `catalog_bg_legendary_pond` | 도감 UI 커스텀 배경 | 도감 화면 배경이 야간 폭풍 연못 일러스트로 변경 |
| 도감 프레임: 황금 물결 | `catalog_frame_golden_wave` | 도감 UI 카드 프레임 | 각 어종 카드 테두리가 금색 물결 프레임으로 변경 |

**설계 의도**: 도감 장식 보상은 순수 시각적 요소로 게임플레이에 영향을 주지 않는다. 칭호 시스템(-> see `docs/content/achievements.md` 섹션 11)과 마찬가지로 수집 동기를 자극하는 "자랑 요소"이다.

---

## 5. 도감 UI 플레이어 경험

### 5.1 도감 접근 경로

- 메뉴 > 수집 도감 > 어종 탭
- 낚시 중 어종 포획 시 도감 등록 팝업에서 "도감 보기" 버튼
- 단축키: `C` 키 (→ see `docs/systems/ui-system.md` 섹션 11, canonical 키 바인딩 맵)

### 5.2 도감 상태 정의

각 어종 카드는 3가지 상태 중 하나를 가진다.

| 상태 | 영문 키 | 시각적 표현 | 표시 정보 |
|------|---------|-----------|----------|
| 잠금 (Locked) | `Locked` | 어종 실루엣(검은색) + "???" 이름 | 미발견 힌트 텍스트(섹션 3.2.1)만 표시 |
| 발견 (Discovered) | `Discovered` | 컬러 어종 이미지 + 실제 이름 | 발견 후 설명(섹션 3.2.2), 등장 조건, 크기 기록, 포획 횟수 |
| 기록 갱신 (NewRecord) | `NewRecord` | 발견 상태 + "NEW!" 뱃지 (3초간) | 최대 크기 기록 갱신 시 일시 표시 |

### 5.3 도감 카드 레이아웃

각 어종 카드에 표시되는 정보 구성:

```
+------------------------------------------+
| [희귀도 컬러 바] (Common=회, Uncommon=녹, |
|                  Rare=청, Legendary=금)   |
+------------------------------------------+
|                                          |
|        [어종 이미지 / 실루엣]              |
|                                          |
+------------------------------------------+
| 이름: 잉어                    희귀도: ★☆☆☆ |
| 크기 범위: 20~50cm                        |
| 내 최대 기록: 47.3cm [Large]              |
| 포획 횟수: 23회                           |
+------------------------------------------+
| [발견 후 설명 텍스트 영역]                  |
+------------------------------------------+
| 등장 조건: 봄/여름/가을 | 전 시간대 | 전 날씨|
+------------------------------------------+
```

- **잠금 상태**: 이미지 영역은 검은 실루엣, 이름은 "???", 크기/포획 횟수 영역은 비어 있음. 미발견 힌트만 표시.
- **등장 조건**: 발견 후에만 공개. 계절은 아이콘(꽃/해/단풍/눈), 시간대는 텍스트, 날씨는 아이콘으로 표시.

### 5.4 수집 진행률 표시

도감 상단에 전체 진행률 바를 표시한다.

```
[어종 도감]  ████████░░░░░░░  10/15종 (66.7%)
            [초보 수집가 ✓] [숙련 수집가 ✓] [도감 마스터 ○]
```

- 진행률 바: 발견 종 수 / 전체 15종
- 마일스톤 아이콘: 달성 시 체크 표시, 미달성 시 빈 원
- 마일스톤 아이콘 클릭 시 해당 마일스톤의 보상 목록 팝업

### 5.5 정렬 및 필터

| 정렬 기준 | 설명 |
|----------|------|
| 도감 번호순 | 기본 정렬 (섹션 3.1의 # 순서) |
| 희귀도순 | Common > Uncommon > Rare > Legendary |
| 포획 횟수순 | 많이 잡은 순 |
| 크기 기록순 | 최대 크기 기록 순 |

| 필터 | 설명 |
|------|------|
| 전체 | 15종 모두 표시 |
| 발견한 어종만 | Locked 제외 |
| 미발견 어종만 | Locked만 표시 |
| 계절별 | 선택한 계절에 등장하는 어종만 표시 |

### 5.6 초회 등록 팝업

어종을 처음 잡으면 도감 등록 팝업이 표시된다.

```
+------------------------------------------+
|        ★ 새로운 어종 발견! ★               |
+------------------------------------------+
|                                          |
|        [어종 컬러 이미지]                  |
|                                          |
|          잉어 (Common)                    |
|          크기: 35.2cm [Medium]            |
|                                          |
|  초회 등록 보상: 5G + 2 XP               |
|                                          |
|  [도감 보기]          [닫기]              |
+------------------------------------------+
```

- 팝업은 게임 시간 정지 상태에서 표시
- "도감 보기" 클릭 시 도감 화면으로 이동, 해당 어종 카드에 포커스
- 마일스톤 달성 시 추가 팝업 표시 (섹션 4.1의 보상 안내)

### 5.7 크기 기록 갱신 알림

기존 최대 크기 기록을 갱신하면 소형 토스트 알림을 표시한다.

```
[크기 기록 갱신!] 잉어 47.3cm → 49.1cm (Large)
```

- 게임 시간은 정지하지 않음 (토스트 알림은 비차단)
- 도감 해당 카드에 "NEW!" 뱃지가 3초간 표시

---

## 6. 튜닝 파라미터 요약

| 파라미터 | 기본값 | 설명 | 조정 범위 |
|----------|--------|------|----------|
| `sizeRandomExponent` | 1.3 | 크기 가중 랜덤 지수. 높을수록 소형 편향 | 1.0~2.0 |
| `smallSizeThreshold` | 0.5 | 소형/중형 경계 (range 비율) | 0.3~0.6 |
| `largeSizeThreshold` | 0.8 | 중형/대형 경계 (range 비율) | 0.7~0.9 |
| `smallPriceMultiplier` | 0.9 | 소형 판매가 보정률 | 0.8~1.0 |
| `mediumPriceMultiplier` | 1.0 | 중형 판매가 보정률 (기본) | 고정 |
| `largePriceMultiplier` | 1.15 | 대형 판매가 보정률 | 1.1~1.3 |
| `firstCatchReward_Common` | 5G, 2 XP | Common 초회 등록 보상 | - |
| `firstCatchReward_Uncommon` | 15G, 5 XP | Uncommon 초회 등록 보상 | - |
| `firstCatchReward_Rare` | 50G, 15 XP | Rare 초회 등록 보상 | - |
| `firstCatchReward_Legendary` | 200G, 50 XP | Legendary 초회 등록 보상 | - |
| `milestone_5_reward` | 100G, 30 XP | 5종 마일스톤 보상 | - |
| `milestone_10_reward` | 300G, 80 XP | 10종 마일스톤 보상 | - |
| `milestone_15_reward` | 500G, 150 XP | 15종 마일스톤 보상 | - |
| `newRecordBadgeDuration` | 3.0초 | "NEW!" 뱃지 표시 시간 | 2.0~5.0 |

---

## 7. FishData SO 확장 필드

현재 FishData SO(-> see `docs/systems/fishing-architecture.md` 섹션 9)에 크기 및 도감 관련 필드가 미정의 상태다. 아래 필드 추가가 필요하다.

| 필드명 | 타입 | 설명 | 비고 |
|--------|------|------|------|
| `sizeMin` | float | 최소 크기 (cm) | 섹션 3.1 테이블 값 |
| `sizeMax` | float | 최대 크기 (cm) | 섹션 3.1 테이블 값 |
| `hintLocked` | string | 미발견 힌트 텍스트 | 섹션 3.2.1 |
| `hintUnlocked` | string | 발견 후 설명 텍스트 | 섹션 3.2.2 |
| `firstCatchGold` | int | 초회 등록 골드 보상 | 섹션 3.1 |
| `firstCatchXP` | int | 초회 등록 XP 보상 | 섹션 3.1 |

[RISK] 본 섹션에서 제안한 "FishData SO(섹션 9)에 6개 필드 추가"는 ARC-030(fishing-architecture.md 섹션 14~23)과 충돌한다. ARC-030은 이 필드들을 FishData SO가 아닌 별도 **FishCatalogData SO**(섹션 15)에 배치한다. 단, 필드명 불일치도 존재: 본 문서의 `sizeMin/sizeMax`(절대값)이 ARC-030의 `baseSizeCm + sizeVarianceMin/Max`(배율 방식)와 다르며, `hintText/descriptionText`가 `hintLocked/hintUnlocked`와 다르다. FIX-075에서 통일 예정. `firstCatchGold/firstCatchXP`는 FishCatalogData SO에 아직 정의되지 않았으므로 FIX-075에서 추가 결정 필요.

---

## 8. 도감 세이브 데이터

도감 진행 상황은 FishingSaveData(-> see `docs/systems/fishing-architecture.md` 섹션 10)에 포함되어야 한다.

### 8.1 추가 필요 세이브 필드

| 필드명 | 타입 | 설명 |
|--------|------|------|
| `discoveredFishIds` | List\<string\> | 발견한 어종 ID 목록 |
| `maxSizeByFishId` | Dictionary\<string, float\> | 어종별 최대 크기 기록 |
| `catchCountByFishId` | Dictionary\<string, int\> | 어종별 포획 횟수 |
| `claimedMilestones` | List\<int\> | 수령한 마일스톤 (5, 10, 15) |

[RISK] 기존 FishingSaveData에 `caughtByFishId` (Dictionary\<string, int\>)가 이미 존재한다(-> see `docs/systems/fishing-architecture.md` 섹션 10). `catchCountByFishId`와 `caughtByFishId`가 중복될 수 있다. 아키텍트와 협의하여 기존 `caughtByFishId`를 재활용할지 결정 필요. `discoveredFishIds`도 `caughtByFishId.Keys`로 대체 가능할 수 있다.

---

## Cross-references

| 관련 문서 | 관계 |
|----------|------|
| `docs/systems/fishing-system.md` | 어종 목록(섹션 4.2), Giant 변이(섹션 4.4), 품질 시스템(섹션 6.1), 숙련도(섹션 7), 도감 개요(섹션 8.2) -- 본 문서의 상위 설계 문서 |
| `docs/systems/fishing-architecture.md` | FishData SO 필드(섹션 9), FishingSaveData(섹션 10) -- 섹션 7, 8의 필드 추가 반영 필요 |
| `docs/content/achievements.md` | 낚시 업적(섹션 9), 칭호(섹션 11) -- 도감 완성 업적(`ach_fish_04`) 보상과의 중복 관리 |
| `docs/balance/fishing-economy.md` | 생선 판매가, ROI 분석 -- 크기 보정률의 경제 영향 검증 필요 |
| `docs/systems/economy-system.md` | 수급 변동(섹션 2.6.2.1), 가격 공식 -- 크기 보정률이 기존 판매가 공식에 추가되는 영향 |
| `docs/systems/crop-growth.md` | 품질 등급 정의(섹션 4.3) -- 생선 품질은 동일 4단계 사용 |
| `docs/balance/progression-curve.md` | XP 테이블 -- 도감 XP 보상의 진행 속도 영향 |

---

## Open Questions & Risks

### [OPEN] 항목

1. **[OPEN]** 크기 보정률 x 품질 배수 x Giant 배수의 곱셈 중첩이 극단적 가격을 만들 수 있다. 황금 잉어 Iridium + Large + Giant = 920G. 밸런스 시트(-> see `docs/balance/fishing-economy.md`)에서 시뮬레이션 필요. (섹션 2.4)
2. **[OPEN]** 미끼 시스템 전체 설계 미완료. 마일스톤 보상으로 미끼를 지급하는 것은 확정하되, 상점 판매/제작 등은 별도 설계 필요. (섹션 4.2)
3. ~~**[OPEN]** 도감 단축키 바인딩 미확정.~~ — **RESOLVED (FIX-105)**: `C` 키로 확정. (→ see `docs/systems/ui-system.md` 섹션 11)
4. **[OPEN]** FishingSaveData의 기존 `caughtByFishId` 필드와 도감 세이브 데이터의 중복 가능성. 아키텍트와 협의 필요. (섹션 8.1)

### [RISK] 항목

1. **[RISK]** FishCatalogData SO에 필드 추가 필요 (`sizeMinCm`, `sizeMaxCm`, `firstCatchGold`, `firstCatchXP`). `docs/systems/fishing-architecture.md` 섹션 15 C# 클래스 및 JSON 스키마에 반영 완료 (ARC-030, FIX-075 적용). 필드명: `hintLocked`/`hintUnlocked` (기존 FishData와 동일 네이밍 패턴). (섹션 7 업데이트됨)
2. **[RISK]** 도감 초회 보상(625G, 162 XP) + 마일스톤 보상(900G, 260 XP) + 업적 보상(2,300G, 390 XP)의 합산이 낚시 관련 총 비전투 보상 3,825G + 812 XP에 달한다. 이는 경제 밸런스(-> see `docs/balance/fishing-economy.md`)에서 검증되어야 한다. 단, 전량 달성은 게임 극후반(2년차+)이므로 실질 영향은 제한적.
3. **[RISK]** 크기 시스템 추가로 판매가 공식이 복잡해진다. EconomyManager의 `GetSellPrice()` 로직에 크기 보정률 파라미터를 추가해야 하며, 이는 아키텍처 변경을 수반한다.

---

> 이 문서는 Claude Code가 자율적으로 작성했습니다.

---

# 음식 아이템 콘텐츠 상세 (Food Items Content Specification)

> 작성: Claude Code (Sonnet 4.6) | 2026-04-09
> 문서 ID: CON-021 | DES-025

---

## Context

이 문서는 SeedMind에서 플레이어가 섭취하여 에너지를 회복하는 **음식 아이템**의 canonical 명세다. `docs/systems/energy-system.md` 섹션 5.2의 [OPEN] 항목을 해소하는 후속 설계 문서로, 음식 등급 체계 확정·아이템 전체 목록·요리 시스템 통합 방안·에너지 밸런스 검증을 포함한다.

**이 문서가 canonical인 데이터**:
- 음식 아이템 전체 목록 (itemId, 이름, 등급, 회복량, 특수 효과)
- 음식 등급별 회복량 기준값 (확정)
- 음식 아이템 획득 경로 (상점 구매 여부, 제조 시설)
- 음식 제조 레시피 (재료, 가공 시설, 소요 시간, 연료)
- 음식 아이템 판매 가격

**이 문서가 canonical이 아닌 데이터 (참조만)**:

| 데이터 종류 | 참조처 |
|------------|--------|
| 에너지 회복 규칙 (maxEnergy 초과 처리, 임시 증가 상한 등) | `docs/systems/energy-system.md` 섹션 5.2 |
| 연료(장작) 구매가, 소모 규칙 | `docs/content/processing-system.md` 섹션 4 |
| 채집 아이템 기본 판매가 | `docs/systems/gathering-system.md` 섹션 3.3~3.7 |
| 작물 기본 판매가 | `docs/design.md` 섹션 4.2 |
| 수산물(생선) 기본 판매가 | `docs/systems/fishing-system.md` 섹션 4.2 |
| 가공품 판매가 (잼, 주스 등) | `docs/content/processing-system.md` 섹션 3 |
| 경제 초기 골드, 하루 수익 기준 | `docs/systems/economy-system.md` 섹션 1.2 |

---

## 1. 설계 결정 요약

### 1.1 요리 시스템 연동 방식

**결정: 기존 베이커리(가공 시스템) 확장 방식을 채택한다. 별도 요리 시설은 신설하지 않는다.**

근거:
- `docs/content/processing-system.md`의 베이커리는 이미 "고급 요리 생산" 시설로 정의되어 있으며, 설계 의도가 "가장 늦게 해금되는 최상위 가공소"다.
- 봄나물 비빔밥·송이 구이(`recipe_gather12`, `recipe_gather13`) 등 채집물 요리 레시피가 이미 베이커리에 배치되어 있다.
- 별도 요리소를 신설하면 시설 해금 트리가 복잡해지고 scope가 불필요하게 확장된다.
- 음식 아이템은 **베이커리 + 가공소(일반)** 두 시설에서 제조하되, 등급별로 시설 요건이 다르게 구성하여 점진적 복잡도를 유지한다.

**요리 시스템 신규 요소 없음**: `docs/design.md` 섹션 4.6 시설 목록에 추가 시설을 기재할 필요 없다.

### 1.2 공급 경로 확정

| 등급 | 획득 경로 |
|------|-----------|
| 기본 음식 | 채집 원물 직접 섭취 (제조 불필요) |
| 일반 요리 | 가공소(일반) 제조 OR 잡화 상점(하나) 구매 |
| 고급 요리 | 베이커리 제조 전용 (상점 판매 없음) |
| 최고급 요리 | 베이커리 제조 전용, 희귀 재료 필요 (상점 판매 없음) |

**상점 판매 범위를 일반 요리까지만 허용하는 이유**: 고급/최고급 요리를 상점에서 구매 가능하면 "골드 → 에너지" 직접 전환이 너무 쉬워져 에너지 시스템의 긴장감이 약화된다. 플레이어는 시설을 건설하고 재료를 수집하는 과정에서 에너지 회복 수단을 획득해야 한다.

---

## 2. 음식 등급 체계

에너지 시스템 canonical (-> see `docs/systems/energy-system.md` 섹션 5.2)에서 정의한 4등급을 기준으로 회복량과 특수 효과를 확정한다.

| 등급 | 등급 ID | 즉시 회복량 (확정) | 특수 효과 | 획득 경로 |
|------|---------|:-----------------:|-----------|-----------|
| 기본 음식 | `FoodTier.Basic` | +10 | 없음 | 채집 원물 |
| 일반 요리 | `FoodTier.Common` | +25 | 없음 | 가공소 / 상점 |
| 고급 요리 | `FoodTier.Advanced` | +45 | 임시 maxEnergy +20 (해당 날) | 베이커리 |
| 최고급 요리 | `FoodTier.Premium` | +60 | 임시 maxEnergy +20 + 이동 속도 +20% (해당 날) | 베이커리 (희귀 레시피) |

**등급별 회복량 설계 근거**:
- 기본(+10): 채집물 원물은 에너지 소모 0으로 획득하므로 회복 효율이 낮아야 한다. 하루 에너지 100 기준 10% 회복.
- 일반(+25): 가공 시간(2~4시간) 투자 대비 합리적 보상. 하루 에너지의 25%.
- 고급(+45): 베이커리 복합 가공 체인 완성 보상. 임시 maxEnergy +20 효과로 하루 총 에너지를 최대 120까지 확장 가능. 실질 최대 회복 가치 65(=45+20).
- 최고급(+60): 게임 내 최고 티어 아이템. 이동 속도 보너스는 낚시 포인트·채집 포인트 이동 효율을 높여 복합 활동 가치 증가.

**음식 섭취 규칙**: (-> see `docs/systems/energy-system.md` 섹션 5.2)

---

## 3. 음식 아이템 전체 목록

### 3.1 기본 음식 (FoodTier.Basic) — 6종

채집 원물을 그대로 섭취하는 형태. 별도 가공 없이 인벤토리에서 사용 가능.

| itemId | 이름 | 원재료 출처 | 회복량 | 판매가 | 비고 |
|--------|------|-----------|:------:|:------:|------|
| `item_food_wild_berry` | 야생 산딸기 | `gather_wild_berry` 채집 | +10 | (-> see gathering-system.md 섹션 3.3) | 봄/여름 |
| `item_food_wild_mushroom` | 야생 버섯 (능이) | `gather_neungi` 채집 | +10 | (-> see gathering-system.md 섹션 3.4) | 여름/가을 |
| `item_food_pine_mushroom` | 송이버섯 | `gather_pine_mushroom` 채집 | +10 | (-> see gathering-system.md 섹션 3.4) | 가을 |
| `item_food_wild_grape` | 머루 | `gather_wild_grape` 채집 | +10 | (-> see gathering-system.md 섹션 3.5) | 가을 |
| `item_food_spring_herb` | 봄나물 | `gather_spring_herb` 채집 | +10 | (-> see gathering-system.md 섹션 3.3) | 봄 |
| `item_food_wild_ginseng` | 산삼 | `gather_wild_ginseng` 채집 | +10 | (-> see gathering-system.md 섹션 3.7) | 봄 Legendary |

**설계 노트**: 채집 원물의 기본 판매가 canonical은 `docs/systems/gathering-system.md` 섹션 3.3~3.7. 직접 섭취 시 판매 기회를 포기하므로 기회비용이 음식 가치의 실질 비용이다.

**산삼 직접 섭취 설계 의도**: 산삼(80G)을 에너지 +10 회복에 쓰는 것은 손해지만, 에너지가 급하게 필요한 극한 상황에서 선택지로 존재한다는 의미가 있다. 산삼의 주 용도는 직판(80G) 또는 산삼주 제조(280G)이다.

### 3.2 일반 요리 (FoodTier.Common) — 8종

가공소(일반)에서 제조하거나 잡화 상점(하나)에서 구매 가능.

| itemId | 이름 | 제조 시설 | 재료 | 가공 시간 | 연료 | 회복량 | 판매가 | 상점 구매가 |
|--------|------|----------|------|:--------:|:----:|:------:|:------:|:-----------:|
| `item_food_roasted_corn` | 구운 옥수수 | 가공소 | 옥수수 x1 | 1시간 | 없음 | +25 | 80G | 120G |
| `item_food_potato_soup` | 감자 수프 | 가공소 | 감자 x2 | 1시간 | 없음 | +25 | 50G | 75G |
| `item_food_tomato_salad` | 토마토 샐러드 | 가공소 | 토마토 x1 | 30분 | 없음 | +25 | 55G | 80G |
| `item_food_grilled_fish` | 구운 생선 정식 | 가공소 | 생선(Common) x1 | 1시간 | 없음 | +25 | 60G | 90G |
| `item_food_carrot_stew` | 당근 스튜 | 가공소 | 당근 x2 | 1시간 | 없음 | +25 | 55G | 80G |
| `item_food_mushroom_soup` | 버섯 수프 | 가공소 | `gather_neungi` x2 또는 `gather_wild_shiitake` x1 | 1시간 | 없음 | +25 | 40G | 60G |
| `item_food_corn_porridge` | 옥수수 죽 | 가공소 | 옥수수 x1, 감자 x1 | 1시간 | 없음 | +25 | 75G | — |
| `item_food_herb_tea` | 약초 차 | 가공소 | `gather_spring_herb` x2 또는 `gather_wild_garlic` x2 | 30분 | 없음 | +25 | 20G | 30G |

**일반 요리 상점 구매가 설계 근거**:
- 상점 구매가 = 직접 제조 판매가 × 1.5 (약 50% 마크업)
- 이 마크업이 "직접 제조 vs 상점 구매" 선택의 트레이드오프를 만든다
- 옥수수 죽(재료 복합)은 상점 판매 없음: 제조의 편의성이 낮아 진입 장벽이 있는 레시피

**일반 요리 레시피 해금**: 가공소 건설 후 즉시 사용 가능 (추가 해금 조건 없음).

### 3.3 고급 요리 (FoodTier.Advanced) — 7종

베이커리 전용 제조. 상점 판매 없음. 베이커리 건설(레벨 9) 후 해금.

| itemId | 이름 | 재료 | 가공 시간 | 연료 | 회복량 | 특수 효과 | 판매가 |
|--------|------|------|:--------:|:----:|:------:|-----------|:------:|
| `item_food_pumpkin_stew` | 호박 스튜 | 호박 x1, 감자 x1 | 2시간 | 장작 x1 | +45 | 임시 maxEnergy +20 | 350G |
| `item_food_strawberry_jam_toast` | 딸기 잼 토스트 | `item_jam_strawberry` x1, 옥수수 가루 x1 | 2시간 | 장작 x1 | +45 | 임시 maxEnergy +20 | 320G |
| `item_food_fish_stew_deluxe` | 특제 생선 스튜 | 생선(Uncommon+) x1, 감자 x1, 당근 x1 | 3시간 | 장작 x1 | +45 | 임시 maxEnergy +20 | 380G |
| `item_food_spring_bibimbap` | 달걀 봄나물 비빔밥 | `gather_wild_garlic` x2, `gather_spring_herb` x1, 달걀 x1 | 2시간 | 장작 x1 | +45 | 임시 maxEnergy +20 | 300G |
| `item_food_autumn_mushroom_dish` | 가을 버섯 요리 | `gather_pine_mushroom` x1, `gather_reishi` x1 | 2시간 | 장작 x1 | +45 | 임시 maxEnergy +20 | 340G |
| `item_food_watermelon_sorbet` | 수박 셔벗 | 수박 x1, `item_juice_watermelon` x0.5개분(수박 주스 별도 제조 필요 없음, 수박 직투입) | 2시간 | 장작 x1 | +45 | 임시 maxEnergy +20 | 480G |
| `item_food_cheese_gratin` | 치즈 그라탱 | `item_cheese` x1, 감자 x2 | 3시간 | 장작 x1 | +45 | 임시 maxEnergy +20 | 420G |

**수박 셔벗 재료 정정**: 수박 x1 단일 재료로 단순화. 수박(기본 판매가 350G)을 에너지 아이템으로 전환 시 상당한 기회비용이 발생하므로 이 자체가 충분한 밸런스 요소다.

**고급 요리 판매가 설계 근거** (PATTERN-BAL-COST 준수):

재료 원가는 직판 기회비용 + 장작 비용(30G)을 합산한다.

| 음식 | 재료 기회비용 | 연료 비용 | 총 비용 | 판매가 | 순이익 | 배수 |
|------|:-----------:|:--------:|:-------:|:------:|:------:|:----:|
| 호박 스튜 | 호박 200G + 감자 30G = 230G | 30G | 260G | 350G | +90G | 1.35x |
| 딸기 잼 토스트 | 딸기 잼 210G + 옥수수 가루 170G = 380G | 30G | 410G | 320G | -90G | 0.78x |
| 특제 생선 스튜 | 생선Uncommon ~50G + 감자 30G + 당근 35G = 115G | 30G | 145G | 380G | +235G | 2.62x |
| 달걀 봄나물 비빔밥 | 달래 5Gx2 + 봄나물 8G + 달걀 35G = 53G | 30G | 83G | 300G | +217G | 3.61x |
| 가을 버섯 요리 | 송이 32G + 영지 24G = 56G | 30G | 86G | 340G | +254G | 3.95x |
| 수박 셔벗 | 수박 350G | 30G | 380G | 480G | +100G | 1.26x |
| 치즈 그라탱 | 치즈 250G + 감자 30Gx2 = 310G | 30G | 340G | 420G | +80G | 1.24x |

**딸기 잼 토스트 설계 노트**: 재료(잼+가루) 원가가 380G인데 판매가 320G로 직접 판매 시 손해다. 이 음식은 에너지 회복 목적으로 제조하는 것이 경제적이며, +45 에너지 + 임시 maxEnergy +20의 가치가 60G 이상의 비용을 정당화할 때만 제조 의미가 있다. 에너지를 "골드로 환산"하는 역방향 거래를 의도적으로 허용하되, 비효율적으로 설계하여 에너지 위기 상황에서만 선택하게 유도한다.

**생선 Uncommon 기준 판매가**: (-> see `docs/systems/fishing-system.md` 섹션 4.2) 약 40~60G.

### 3.4 최고급 요리 (FoodTier.Premium) — 4종

베이커리 전용, 희귀 재료 필요. 레벨 9 + 채집 숙련도 Lv.5 이상 해금.

| itemId | 이름 | 재료 | 가공 시간 | 연료 | 회복량 | 특수 효과 | 판매가 |
|--------|------|------|:--------:|:----:|:------:|-----------|:------:|
| `item_food_lotus_tea_feast` | 황금 연꽃 만찬 | `gather_golden_lotus` x1, 수박 x1 | 4시간 | 장작 x2 | +60 | 임시 maxEnergy +20, 이동 속도 +20%\* (해당 날) | 900G |
| `item_food_millennium_soup` | 천년 영지 보양식 | `gather_millennium_reishi` x1, 닭 수프(달걀 x2 + 당근 x1 가공소 제조) | 4시간 | 장작 x2 | +60 | 임시 maxEnergy +20, 이동 속도 +20%\* (해당 날) | 850G |
| `item_food_royal_harvest` | 왕실 수확 연회 | 호박 분말 x1, 수박 잼 x1, `gather_wild_ginseng` x1 | 5시간 | 장작 x2 | +60 | 임시 maxEnergy +20, 이동 속도 +20%\* (해당 날) | 1,200G |
| `item_food_ginseng_elixir` | 산삼 강정 | `gather_wild_ginseng` x2, 꿀(달래꽃 대용: `gather_wild_garlic` x3 + 설탕 상점 구매 50G) | 5시간 | 장작 x2 | +60 | 임시 maxEnergy +20, 이동 속도 +20%\* (해당 날) | 1,100G |

\* [OPEN] 이동 속도 +20%는 잠정값. 이동 속도 시스템 설계 후 최종 검증 필요 (Open Questions 3번).

**최고급 요리 판매가 설계 근거** (PATTERN-BAL-COST 준수):

| 음식 | 재료 기회비용 | 연료 비용 | 총 비용 | 판매가 | 순이익 |
|------|:-----------:|:--------:|:-------:|:------:|:------:|
| 황금 연꽃 만찬 | 황금 연꽃 100G (→ see gathering-items.md 섹션 4.6) + 수박 350G | 60G | 510G | 900G | +390G |
| 천년 영지 보양식 | 천년 영지 120G (→ see gathering-items.md 섹션 5.6) + 닭 수프 재료(달걀 35Gx2 + 당근 35G) | 60G | 285G | 850G | +565G |
| 왕실 수확 연회 | 호박 분말 320G + 수박 잼 750G + 산삼 80G = 1,150G | 60G | 1,210G | 1,200G | -10G |
| 산삼 강정 | 산삼 80Gx2 + 달래 5Gx3 + 설탕 50G = 225G | 60G | 285G | 1,100G | +815G |

**왕실 수확 연회 설계 노트**: 재료 원가가 1,210G로 판매 시 순이익 -10G(사실상 손익 분기점)다. 이는 의도적이다. 왕실 수확 연회는 판매용이 아니라 에너지 회복 목적으로만 제조 의미가 있는 아이템으로 설계한다. +60 에너지 + maxEnergy +20 + 이동 속도 보너스라는 효과가 1,210G의 재료비를 감수할 가치가 있는지는 플레이어 상황에 따른 선택이다.

**산삼 강정의 경제적 포지셔닝**: 재료비 285G 대비 판매가 1,100G로 순이익 +815G는 이 문서 내 음식 아이템 중 가장 높다. 단, 산삼 2개는 Legendary 채집물로 봄 시즌에만 0.5~1% 확률로 등장한다. 희소성이 높은 가격을 정당화한다.

**"닭 수프" 중간 가공재**: `item_food_millennium_soup`의 재료인 닭 수프는 가공소(일반)에서 달걀 x2 + 당근 x1으로 2시간 가공 제조한다. itemId: `item_chicken_soup`, 판매가: 120G, 회복량: +25(일반 요리 등급). 이 중간 가공재는 섹션 3.2 일반 요리 목록에도 추가 가능하나, 별도 음식 아이템으로 분류하는 것이 인벤토리 관리 측면에서 적절하다.

**설탕(Sugar)**: `item_sugar`, 잡화 상점(하나)에서 50G 구매. 산삼 강정 전용 재료. [OPEN] 설탕 아이템 ID 및 상점 정의는 별도 확정 필요.

---

## 4. 요리 시스템 통합 방안

### 4.1 최종 결론: 기존 가공 시스템 확장

별도 요리 시스템 없이, 기존 `docs/content/processing-system.md`의 **가공소(일반) + 베이커리** 두 시설에서 음식 아이템을 제조한다.

**가공소(일반)에서 제조 가능한 음식**: 일반 요리 8종 (섹션 3.2)
**베이커리에서 제조 가능한 음식**: 고급 요리 7종 + 최고급 요리 4종 (섹션 3.3, 3.4)

### 4.2 processing-system.md 확장 내용

이 문서의 레시피는 `docs/content/processing-system.md`에 다음과 같이 연동된다:

- **가공소(일반) 섹션**: 일반 요리(FoodTier.Common) 레시피 8종 추가
  - 새 섹션 번호: `3.1.5 일반 요리 레시피 (8종)`
  - 총 레시피 수: 기존 31종 → 39종으로 증가
- **베이커리 섹션**: 고급 요리(FoodTier.Advanced) 7종 + 최고급(FoodTier.Premium) 4종 추가
  - 새 섹션 번호: `3.4.2 고급/최고급 요리 레시피 (11종)`
  - 총 레시피 수: 기존 9종 → 20종으로 증가
- **전체 레시피 수**: 기존 56종 → 75종 (음식 레시피 19종 추가)

**[OPEN]** processing-system.md에 음식 레시피를 실제로 추가하는 작업은 별도 FIX/CON 태스크로 처리한다. 이 문서는 음식 아이템의 canonical 명세를 정의하며, 실제 레시피 테이블 통합은 후속 작업이다.

### 4.3 레시피 ID 체계

음식 레시피 ID는 `recipe_food_` 접두사를 사용한다.

| 등급 | ID 패턴 | 예시 |
|------|---------|------|
| 일반 요리 | `recipe_food_common_<아이템명>` | `recipe_food_common_roasted_corn` |
| 고급 요리 | `recipe_food_advanced_<아이템명>` | `recipe_food_advanced_pumpkin_stew` |
| 최고급 요리 | `recipe_food_premium_<아이템명>` | `recipe_food_premium_lotus_feast` |

### 4.4 신규 시설 필요 여부

**신규 시설 불필요**: design.md 섹션 4.6 시설 목록 수정 불필요.

단, 다음 사항은 기존 시설 정보에 반영이 필요하다:
- 베이커리 레시피 수 업데이트 (9종 → 20종)
- 가공소(일반) 레시피 수 업데이트 (31종 → 39종)
- 이는 processing-system.md 섹션 2.2의 레시피 수 항목에 해당한다.

---

## 5. 에너지 밸런스 검증

### 5.1 음식 ROI 분석 — 골드 비용 대비 에너지 회복 가치

"에너지 회복 가치"를 골드로 환산하여, 음식 섭취가 "골드를 에너지로 바꾸는 우회로"가 되는지 검증한다.

**에너지 1단위의 골드 가치 추산**:
- 하루 에너지 100으로 달성 가능한 최대 수익: 낚시 25회 시도 시 에너지 75 소모, 수익 약 25 × 40G = 1,000G/day
- 에너지 1 ≈ 13.3G (1,000G / 75 에너지)
- 보수적 추산 (경작 중심): 하루 200G 수익 / 100 에너지 = 에너지 1 ≈ 2G

현실적 에너지 가치는 게임 단계에 따라 2G~13G 범위다. 이를 기준으로 음식별 에너지 비용 효율을 검증한다.

### 5.2 등급별 비용 효율 분석

| 등급 | 평균 재료 비용 | 회복량 | 비용/에너지 | 에너지 가치 (낙관) | 에너지 가치 (보수) | 판정 |
|------|:------------:|:------:|:----------:|:-----------------:|:-----------------:|:----:|
| 기본 음식 | 5~80G (기회비용) | +10 | 0.5~8G/E | 133G | 20G | 일반 기회비용 → 대부분 합리적 |
| 일반 요리 | 50~200G (재료) | +25 | 2~8G/E | 333G | 50G | 상점 구매 시 75~120G → 에너지 가치 중간 범위 |
| 고급 요리 (제조) | 83~410G (재료+연료) | +45 | 1.8~9G/E | 598G | 90G | 직접 제조 시 효율 높음 |
| 최고급 요리 | 285~1,210G | +60+α | 4.75~20G/E | 799G | 120G | 특수 효과 포함 시 정당화 |

**우회로 위험 분석**:
- **상점 구매 일반 요리**: 75~120G로 +25 에너지. 에너지 가치 환산 시 333G~50G. 경작 중심 초반 플레이어(에너지 1 ≈ 2G 기준)에게는 75G를 써서 에너지 25를 사는 것(50G 가치)이 손해. 낚시 후반 플레이어(에너지 1 ≈ 13G 기준)에게는 에너지 25 = 333G 가치로 120G 상점 구매가 유리.
- **결론**: 일반 요리 상점 구매는 고수입 후반 플레이어에게만 합리적. 우회로 위험 제한적.

### 5.3 우회로 방지 설계

1. **고급/최고급 요리 상점 미판매**: 베이커리 제조 전용으로, 골드만 있어도 접근 불가.
2. **최고급 요리 재료의 희소성**: Legendary 채집물(황금 연꽃, 천년 영지, 산삼)이 재료이므로, 골드가 아무리 많아도 재료가 없으면 제조 불가.
3. **일반 요리 상점가 마크업 50%**: 직접 제조 대비 50% 비싸므로 시설 보유 플레이어는 제조가 유리.
4. **기본 음식의 낮은 회복량**: 채집물 직접 섭취는 +10으로 낮아, 채집만으로 에너지를 완전 충당하는 것은 불가능.

### 5.4 하루 에너지 시뮬레이션 (음식 포함)

**시나리오**: 기본 도구, 레벨 6, 가공소 보유, 베이커리 미보유

| 행동 | 소모/회복 | 누적 에너지 |
|------|:---------:|:-----------:|
| 시작 | — | 100 |
| 경작 10타일 (호미 기본) | -20 | 80 |
| 물주기 20타일 | -20 | 60 |
| 낚시 10회 (Lv.6) | -30 | 30 |
| 상점 구매 일반 요리 섭취 (1개, 80G) | +25 | 55 |
| 추가 채집 20회 (맨손) | 0 | 55 |
| 수확 10타일 | -10 | 45 |
| 취침 (22:00 일반 수면) | +100% | 100 |

음식 1개(80G)로 하루 활동이 10 행동가량 연장된다. 300G/day 수입 기준 플레이어에게 80G는 하루 수입의 27%로 부담이 있어, 매일 구매는 어려운 선택이다.

---

## Cross-references

| 관련 문서 | 연관 내용 |
|-----------|-----------|
| `docs/systems/energy-system.md` 섹션 5.2 | 음식 회복 규칙 canonical (회복량 체계, 임시 maxEnergy 증가 상한, 섭취 규칙) — 이 문서가 [OPEN] 해소 |
| `docs/content/processing-system.md` 섹션 3, 4 | 베이커리/가공소 레시피 체계, 연료(장작 30G) 비용 canonical |
| `docs/systems/gathering-system.md` 섹션 3.3~3.7 | 채집 원물 기본 판매가 (기본 음식 기회비용 계산 기준) |
| `docs/design.md` 섹션 4.2 | 작물 기본 판매가 (요리 재료 기회비용 계산 기준) |
| `docs/systems/fishing-system.md` 섹션 4.2 | 수산물 기본 판매가 (요리 재료 기회비용 계산 기준) |
| `docs/systems/economy-system.md` 섹션 1.2 | 초기 골드 500G, 300G/day 수익 기준 |
| `docs/content/gathering-items.md` | 채집 아이템 상세 (원물 기본 음식의 출처, 황금 연꽃·천년 영지 판매가 canonical) |
| `docs/content/livestock-system.md` 섹션 4 | 달걀 판매가 canonical (35G, 요리 재료 기회비용 계산 기준) |

---

## Open Questions

1. [OPEN] **설탕 아이템 (`item_sugar`) 정의**: 잡화 상점 상품 목록에 추가 필요 여부 및 가격(50G 잠정) 확정 — 상점 시스템 설계 문서 작성 후 동기화.
2. [OPEN] **닭 수프(`item_chicken_soup`) 분류**: 일반 요리 목록(섹션 3.2)에 정식 추가할지, 최고급 요리 전용 중간 가공재로만 존재할지 결정 필요.
3. [OPEN] **이동 속도 +20% 보너스 수치**: energy-system.md 섹션 5.2에서 "속도 보너스"로만 언급됨. 이 문서에서 +20%로 잠정 확정하나, 이동 속도 시스템 설계 문서 작성 후 최종 검증 필요.
4. [OPEN] **음식 섭취 UI/UX**: 인벤토리에서 음식 아이템 선택 → 즉시 섭취 방식인지, 핫바 배치 후 단축키 섭취인지 — UI 시스템 문서에서 확정.
5. ~~[OPEN] **processing-system.md 음식 레시피 통합**: 이 문서에서 정의한 음식 레시피 19종을 processing-system.md에 실제 추가하는 작업 필요 (별도 CON 태스크 권장).~~ **[CON-022에서 완료됨 — 2026-04-09]** processing-system.md 섹션 3.1.5(일반 요리 8종), 섹션 3.4.2(고급 7종 + 최고급 4종) 추가 완료.
6. ~~[OPEN] **베이커리 레시피 수 업데이트**: processing-system.md 섹션 2.2 레시피 수 (베이커리 9종 → 20종, 가공소 31종 → 39종) 반영 필요.~~ **[CON-022에서 완료됨 — 2026-04-09]** processing-system.md 섹션 2.2 및 섹션 3.8 요약 테이블(75종) 반영 완료.

---

## Risks

1. [RISK] **최고급 요리의 이동 속도 보너스 밸런스**: 이동 속도 +20% 효과가 맵 크기 및 이동 시스템과 맞지 않을 경우 체감 효과가 너무 강하거나 약할 수 있음. 이동 속도 시스템 설계 후 수치 재검토 권장.
2. [RISK] **산삼 강정의 높은 순이익(+815G)**: 최고급 요리이나 재료 원가(285G) 대비 판매가(1,100G)가 높아 요리를 에너지 목적이 아닌 판매 목적으로 남발할 가능성. 단, 산삼 2개(Legendary, 봄 전용)의 희소성이 이를 제한함. 판매가 조정(예: 700G)이 필요한지 플레이 테스트 후 확인 권장.
3. [RISK] **딸기 잼 토스트의 구조적 손해**: 재료(380G+30G)가 판매가(320G)를 초과해 직판 손해 구조. 에너지 목적 전용으로 설계했으나, 플레이어가 이를 인지 못하고 의도치 않게 손해를 볼 가능성. 게임 내 레시피 설명에 "에너지 회복 전용" 표시 또는 판매가 조정 검토.
4. [RISK] **일반 요리 8종의 가공소(일반) 슬롯 경쟁**: 가공소(일반) 슬롯은 잼/주스/절임 레시피와 공유된다. 음식 레시피 8종 추가로 "슬롯에 무엇을 넣을지" 선택 압박이 증가할 수 있다. 이는 설계 의도(의미 있는 선택)에 부합하지만, 초반 플레이어에게 과도한 복잡도를 줄 수 있음.

---

*이 문서는 Claude Code가 DES-025 태스크에 따라 작성했습니다. `docs/systems/energy-system.md` 섹션 5.2의 [OPEN] 항목을 해소하는 음식 아이템 canonical 명세입니다.*

---

---
title: 채집 아이템 콘텐츠 상세
date: 2026-04-07
author: Claude Code (Opus 4.6)
---

# 채집 아이템 콘텐츠 상세 (Gathering Items Content Specification)

> 작성: Claude Code (Opus 4.6) | 2026-04-07
> 문서 ID: CON-012

---

## 1. Context

이 문서는 SeedMind의 27종 채집 아이템 각각에 대한 콘텐츠 상세를 정의한다. 채집 시스템의 아이템 카탈로그(`docs/systems/gathering-system.md` 섹션 3)에 정의된 기본 정보(이름, 영문 ID, 희귀도, 판매가, 계절/위치 조건)를 기반으로, 인벤토리 속성, 가공 레시피 연계, NPC 선물 적합도, 도구/낚싯대 업그레이드 재료 역할, 힌트 텍스트, 수집 효과음을 아이템별로 명세한다.

**설계 목표**: 27종 채집 아이템 각각이 단순한 판매 대상이 아니라, 가공-NPC 관계-도구 업그레이드 등 다양한 용도로 연결되어 "무엇을 채집할지" 선택에 의미를 부여한다. 동시에 힌트 텍스트와 효과음을 통해 채집 행위 자체에 감각적 피드백을 더한다.

### 1.1 본 문서가 canonical인 데이터

- 27종 채집 아이템의 최대 스택 수 (`maxStack`)
- 채집 아이템 힌트 텍스트 (미발견 상태에서 표시)
- 채집 아이템별 NPC 선물 적합도 (좋아함/보통/싫어함)
- 채집 아이템별 가공 레시피 연계 방향 (실제 레시피 정의는 processing-system.md가 canonical)
- 채집 아이템별 도구/낚싯대 업그레이드 재료 역할
- 채집 아이템 수집 효과음 ID 제안 (확정은 sound-design.md가 canonical)

### 1.2 본 문서가 canonical이 아닌 데이터 (참조만)

| 데이터 종류 | 참조처 |
|------------|--------|
| 아이템명, 영문 ID, 희귀도, 기본 판매가, 계절/위치 조건 | `docs/systems/gathering-system.md` 섹션 3.3~3.7 |
| 채집 포인트 유형, 리스폰 주기, 등장 가중치 | `docs/systems/gathering-system.md` 섹션 2, 3.2 |
| 채집 숙련도 레벨, XP 획득량 | `docs/systems/gathering-system.md` 섹션 4~5 |
| 가공 레시피 상세 (재료, 시간, 판매가) | `docs/content/processing-system.md` |
| 낚싯대 업그레이드 재료 요건 | `docs/systems/fishing-system.md` 섹션 1.1 |
| 도구 업그레이드 재료 요건 | `docs/systems/tool-upgrade.md` 섹션 2 |
| NPC 캐릭터 설정, 역할 | `docs/content/npcs.md` |
| 인벤토리 카테고리 체계, 스택 규칙 | `docs/systems/inventory-system.md` 섹션 1.1 |
| SFX ID 네이밍 컨벤션, 전체 SFX 목록 | `docs/systems/sound-design.md` |
| 여행 상인 광석 판매 아이템 | `docs/systems/gathering-system.md` 섹션 7.1 |

---

## 2. 인벤토리 분류 및 스택 규칙

### 2.1 카테고리 확장 제안

현재 `docs/systems/inventory-system.md` 섹션 1.1의 아이템 카테고리에 채집물 전용 카테고리가 없다. 다음 카테고리 추가를 제안한다.

[OPEN] inventory-system.md에 아래 카테고리 추가 필요:

| 카테고리 | 영문 키 | 설명 | 스택 가능 | 최대 스택 | 드롭 가능 | 비고 |
|----------|---------|------|:---------:|:---------:|:---------:|------|
| 채집물 | `Gathered` | 야생 식물, 버섯, 열매 등 채집 아이템 | O | 30 | O | 품질 속성 없음 |
| 광물 | `Mineral` | 광석, 원석 등 광물 채집 아이템 | O | 30 | O | 업그레이드 재료 겸용 |

### 2.2 최대 스택 규칙

| 아이템 유형 | `maxStack` | 설계 근거 |
|------------|:----------:|-----------|
| 식물/꽃/열매/버섯/수생 식물 | 30 | 가공품(Processed)과 동일. 일일 최대 채집량 ~18개 기준, 2일치를 한 슬롯에 보관 가능 |
| 광물 (Common~Rare) | 30 | 리스폰이 느려(3일) 대량 축적이 어렵지만, 업그레이드용 비축 편의 제공 |
| 광물 (Legendary: 자수정) | 10 | 희소 아이템으로 스택 제한. NPC 선물/장식 용도의 특별한 느낌 유지 |
| Legendary 아이템 (산삼, 황금 연꽃, 천년 영지) | 5 | 극희소 아이템. 낮은 스택 상한으로 희소성 체감 강화 |

---

## 3. 봄 채집물 상세 (6종)

### 3.1 민들레 (#1)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_dandelion` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.3) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | 없음 (직접 판매 또는 NPC 선물용) |
| **NPC 선물** | 하나: 보통 / 철수: 싫어함 / 목이: 보통 / 바람이: 보통 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "따뜻한 봄바람에 노란 꽃잎을 흔드는 작은 꽃. 숲 어디서나 만날 수 있다." |
| **SFX 제안** | `sfx_gather_flower` |

### 3.2 달래 (#2)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_wild_garlic` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.3) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | **봄나물 비빔밥** 재료 (달래 x2 + 봄나물 x1 -> 봄나물 비빔밥). 베이커리에서 제작. (-> see `docs/content/processing-system.md` 섹션 3.7.3, `recipe_gather12`) |
| **NPC 선물** | 하나: 좋아함 / 철수: 보통 / 목이: 보통 / 바람이: 보통 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "봄 숲의 은은한 향기를 따라가면 만나는 작은 풀. 알싸한 맛이 요리에 깊이를 더한다." |
| **SFX 제안** | `sfx_gather_herb` |

### 3.3 봄나물 (#3)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_spring_herb` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.3) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | **봄나물 비빔밥** 재료 (달래 x2 + 봄나물 x1). 베이커리에서 제작. (-> see `docs/content/processing-system.md` 섹션 3.7.3, `recipe_gather12`) |
| **NPC 선물** | 하나: 좋아함 / 철수: 보통 / 목이: 좋아함 / 바람이: 보통 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "이슬 맺힌 숲 가장자리에서 자라는 부드러운 나물. 봄의 향긋함이 가득하다." |
| **SFX 제안** | `sfx_gather_herb` |

### 3.4 제비꽃 (#4)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_violet` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.3) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | [OPEN] **꽃다발** 재료 후보 (제비꽃 x3 -> 봄 꽃다발). 가공소에서 제작. FIX-083에서 미채택 (스코프 외). 추후 장식 레시피 확장 시 검토 |
| **NPC 선물** | 하나: 좋아함 / 철수: 보통 / 목이: 좋아함 / 바람이: 좋아함 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "덤불 사이에 숨듯 피어나는 보라빛 작은 꽃. 선물하면 누구나 미소 짓는다." |
| **SFX 제안** | `sfx_gather_flower` |

### 3.5 송이 (봄) (#5)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_spring_mushroom` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.3) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | [OPEN] **건조 버섯 (송이/봄)** 재료 (송이(봄) x2 -> 건조 버섯 x1). 가공소에서 제작. FIX-083에서 미채택 — 봄 송이는 Rare급으로 채집량이 적어 건조 가공보다 직판이 유리. 추후 봄 가공 레시피 확장 시 검토 |
| **NPC 선물** | 하나: 좋아함 / 철수: 좋아함 / 목이: 보통 / 바람이: 좋아함 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "비 내린 다음 날 숲에서 발견되는 귀한 버섯. 진한 향이 코끝을 간질인다." |
| **SFX 제안** | `sfx_gather_mushroom` |

### 3.6 산삼 (#6)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_wild_ginseng` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.3) |
| `maxStack` | 5 |
| 카테고리 | `Gathered` |
| **가공 연계** | **산삼주** (산삼 x1 -> 산삼주 x1, 발효실, 72시간, 280G). Legendary 아이템의 장기 투자 가공 경로. (-> see `docs/content/processing-system.md` 섹션 3.7.2, `recipe_gather11`) |
| **NPC 선물** | 하나: 좋아함 / 철수: 좋아함 / 목이: 좋아함 / 바람이: 좋아함 (전원 좋아함 — Legendary 특전) |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "봄비 내린 깊은 숲속, 오랜 세월을 견딘 신비로운 뿌리. 숙련된 채집가만이 발견할 수 있다." |
| **SFX 제안** | `sfx_gather_legendary` |

---

## 4. 여름 채집물 상세 (6종)

### 4.1 산딸기 (#7)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_wild_berry` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.4) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | **야생 베리잼** (산딸기 x5 -> 야생 베리잼 x1, 가공소, 1시간, 26G). (-> see `docs/content/processing-system.md` 섹션 3.7.1, `recipe_gather01`) |
| **NPC 선물** | 하나: 좋아함 / 철수: 보통 / 목이: 보통 / 바람이: 좋아함 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "여름 덤불에 주렁주렁 매달린 빨간 열매. 한 알 베어 물면 새콤달콤한 맛이 퍼진다." |
| **SFX 제안** | `sfx_gather_berry` |

### 4.2 쑥 (#8)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_mugwort` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.4) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | [OPEN] **쑥떡** 제안 보류 (쑥 x3 + 쌀 x1 -> 쑥떡). FIX-083에서 미채택 — 쌀 획득 경로가 미확정. 쌀 경로 확정 후 추가 예정 (-> see `docs/systems/fishing-system.md` 섹션 8 [OPEN]) |
| **NPC 선물** | 하나: 보통 / 철수: 보통 / 목이: 좋아함 / 바람이: 보통 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "숲과 초원 어디서든 만날 수 있는 질긴 약초. 독특한 향이 몸과 마음을 달래준다." |
| **SFX 제안** | `sfx_gather_herb` |

### 4.3 으름 열매 (#9)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_akebia_fruit` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.4) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | 없음 (직접 판매 전용). 여름 한정 채집물로 희소성 자체가 가치 |
| **NPC 선물** | 하나: 좋아함 / 철수: 보통 / 목이: 보통 / 바람이: 좋아함 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "여름 덤불 깊은 곳에 매달린 보라빛 열매. 껍질을 까면 달콤한 과육이 나온다." |
| **SFX 제안** | `sfx_gather_berry` |

### 4.4 연잎 (#10)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_lotus_leaf` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.4) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | [OPEN] **연잎밥** 제안 보류 (연잎 x1 + 쌀 x2 -> 연잎밥). FIX-083에서 미채택 — 쌀 획득 경로가 미확정. 쌀 경로 확정 후 추가 예정 (-> see `docs/systems/fishing-system.md` 섹션 8 [OPEN]) |
| **NPC 선물** | 하나: 보통 / 철수: 보통 / 목이: 좋아함 / 바람이: 보통 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "연못 위에 둥글게 펼쳐진 초록 잎. 이슬을 담은 모양이 아름답다." |
| **SFX 제안** | `sfx_gather_leaf` |

### 4.5 영지버섯 (#11)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_reishi` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.4) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | **건조 영지** (영지버섯 x2 -> 건조 영지 x1, 가공소, 2시간, 84G). Rare 버섯 별도 가공품으로 차별화. (-> see `docs/content/processing-system.md` 섹션 3.7.1, `recipe_gather05`) |
| **NPC 선물** | 하나: 좋아함 / 철수: 보통 / 목이: 좋아함 / 바람이: 좋아함 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "비 온 다음 날 숲 깊은 곳에서 자라는 단단한 버섯. 윤기 나는 갈색 갓이 특징이다." |
| **SFX 제안** | `sfx_gather_mushroom` |

### 4.6 황금 연꽃 (#12)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_golden_lotus` |
| 희귀도/판매가 | Legendary / **100G** (→ see `docs/systems/gathering-system.md` 섹션 3.4) |
| `maxStack` | 5 |
| 카테고리 | `Gathered` |
| **가공 연계** | **황금 연꽃차** (황금 연꽃 x1 -> 황금 연꽃차 x1, 가공소, 2시간, 300G). Legendary 아이템 고부가가치 가공 경로. (-> see `docs/content/processing-system.md` 섹션 3.7.1, `recipe_gather06`) |
| **NPC 선물** | 하나: 좋아함 / 철수: 좋아함 / 목이: 좋아함 / 바람이: 좋아함 (전원 좋아함 — Legendary 특전) |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "맑은 여름 새벽, 연못에서 금빛으로 빛나는 신비로운 꽃. 숙련된 눈만이 발견할 수 있다." |
| **SFX 제안** | `sfx_gather_legendary` |

---

## 5. 가을 채집물 상세 (6종)

### 5.1 도토리 (#13)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_acorn` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.5) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | **도토리묵** (도토리 x5 -> 도토리묵 x1, 가공소, 1시간, 20G). (-> see `docs/content/processing-system.md` 섹션 3.7.1, `recipe_gather02`) |
| **NPC 선물** | 하나: 보통 / 철수: 보통 / 목이: 좋아함 / 바람이: 보통 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "가을이 되면 숲 바닥에 수북이 쌓이는 작은 열매. 묵으로 만들면 쫄깃한 별미가 된다." |
| **SFX 제안** | `sfx_gather_nut` |

### 5.2 능이버섯 (#14)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_neungi` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.5) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | **건조 버섯 (능이)** (능이버섯 x3 -> 건조 버섯 x1, 가공소, 2시간, 24G). (-> see `docs/content/processing-system.md` 섹션 3.7.1, `recipe_gather03`) |
| **NPC 선물** | 하나: 좋아함 / 철수: 좋아함 / 목이: 보통 / 바람이: 보통 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "가을 숲의 낙엽 사이에서 고개를 내미는 향긋한 버섯. 국물 재료로 최고다." |
| **SFX 제안** | `sfx_gather_mushroom` |

### 5.3 표고버섯 (야생) (#15)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_wild_shiitake` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.5) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | **건조 버섯 (표고)** (표고버섯(야생) x2 -> 건조 버섯 x1, 가공소, 2시간, 36G). (-> see `docs/content/processing-system.md` 섹션 3.7.1, `recipe_gather04`) |
| **NPC 선물** | 하나: 좋아함 / 철수: 보통 / 목이: 보통 / 바람이: 좋아함 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "숲속 고목에 붙어 자라는 야생 표고. 재배산보다 진한 향이 난다." |
| **SFX 제안** | `sfx_gather_mushroom` |

### 5.4 머루 (#16)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_wild_grape` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.5) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | **머루 와인** (머루 x5 -> 머루 와인 x1, 발효실, 48시간, 90G). (-> see `docs/content/processing-system.md` 섹션 3.7.2, `recipe_gather10`) |
| **NPC 선물** | 하나: 좋아함 / 철수: 보통 / 목이: 보통 / 바람이: 좋아함 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "가을 덤불에 매달린 검푸른 작은 포도. 발효시키면 깊은 맛의 와인이 된다." |
| **SFX 제안** | `sfx_gather_berry` |

### 5.5 송이버섯 (#17)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_pine_mushroom` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.5) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | **송이 구이** (송이버섯 x1 -> 송이 구이 x1, 베이커리, 30분, 70G). Rare 버섯의 간편 고급 요리. DES-019 가격 조정. (-> see `docs/content/processing-system.md` 섹션 3.7.3, `recipe_gather13`) |
| **NPC 선물** | 하나: 좋아함 / 철수: 좋아함 / 목이: 좋아함 / 바람이: 좋아함 (전원 좋아함 — Rare 고급 식재료) |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "소나무 뿌리 아래 숨어 자라는 가을의 보물. 그윽한 향이 숲 전체에 퍼진다." |
| **SFX 제안** | `sfx_gather_mushroom` |

### 5.6 천년 영지 (#18)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_millennium_reishi` |
| 희귀도/판매가 | Legendary / **120G** (→ see `docs/systems/gathering-system.md` 섹션 3.5) |
| `maxStack` | 5 |
| 카테고리 | `Gathered` |
| **가공 연계** | **천년 영지차** (천년 영지 x1 -> 천년 영지차 x1, 가공소, 3시간, 360G). Legendary 아이템 최고 부가가치 가공. (-> see `docs/content/processing-system.md` 섹션 3.7.1, `recipe_gather07`) |
| **NPC 선물** | 하나: 좋아함 / 철수: 좋아함 / 목이: 좋아함 / 바람이: 좋아함 (전원 좋아함 — Legendary 특전) |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "흐린 가을 날, 숲의 가장 깊은 곳에서 발견되는 전설의 버섯. 천 년의 기운이 서려 있다." |
| **SFX 제안** | `sfx_gather_legendary` |

---

## 6. 겨울 채집물 상세 (3종)

### 6.1 겨울 나무껍질 (#19)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_winter_bark` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.6) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | **나무껍질 차** (겨울 나무껍질 x5 -> 나무껍질 차 x1, 가공소, 30분, 15G). 겨울 채집의 소소한 가공 콘텐츠. (-> see `docs/content/processing-system.md` 섹션 3.7.1, `recipe_gather08`) |
| **NPC 선물** | 하나: 보통 / 철수: 보통 / 목이: 좋아함 / 바람이: 보통 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "겨울 숲에서 벗겨진 마른 나무껍질. 차로 우리면 은은한 나무 향이 난다." |
| **SFX 제안** | `sfx_gather_bark` |

### 6.2 눈꽃 이끼 (#20)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_snow_moss` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.6) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | 없음 (직접 판매 전용). Snow 날에만 등장하는 희소성 자체가 가치 |
| **NPC 선물** | 하나: 좋아함 / 철수: 보통 / 목이: 좋아함 / 바람이: 좋아함 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "눈 덮인 바위 틈에 피어나는 하얀 이끼. 눈이 내리는 날에만 모습을 보인다." |
| **SFX 제안** | `sfx_gather_herb` |

### 6.3 동충하초 (#21)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_cordyceps` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.6) |
| `maxStack` | 30 |
| 카테고리 | `Gathered` |
| **가공 연계** | **동충하초 환** (동충하초 x2 -> 동충하초 환 x1, 가공소, 2시간, 140G). 겨울 Rare 아이템 가공 경로. (-> see `docs/content/processing-system.md` 섹션 3.7.1, `recipe_gather09`) |
| **NPC 선물** | 하나: 좋아함 / 철수: 좋아함 / 목이: 보통 / 바람이: 좋아함 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "겨울 눈 속에 숨은 희귀한 약재. 눈이 내린 날, 숙련된 채집가만이 찾아낼 수 있다." |
| **SFX 제안** | `sfx_gather_mushroom` |

---

## 7. 광물 상세 (6종, 사계절 공통)

### 7.1 돌 조각 (#22)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_stone_chip` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.7) |
| `maxStack` | 30 |
| 카테고리 | `Mineral` |
| **가공 연계** | 없음 (건축 자재로 사용). [OPEN] 시설 건설 시 건설 재료(`Material` 카테고리)로 활용 가능한지 검토 필요 |
| **NPC 선물** | 하나: 싫어함 / 철수: 보통 / 목이: 보통 / 바람이: 싫어함 |
| **업그레이드 재료** | 해당 없음 (기본 광물, 직접적 업그레이드 용도 없음) |
| **힌트 텍스트** | "동굴 입구에 흔하게 굴러다니는 작은 돌 조각. 쌓아두면 건축 재료로 쓸 수 있다." |
| **SFX 제안** | `sfx_gather_stone` |

### 7.2 구리 광석 (#23)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_copper_ore` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.7) |
| `maxStack` | 30 |
| 카테고리 | `Mineral` |
| **가공 연계** | 없음 (업그레이드 재료 전용) |
| **NPC 선물** | 하나: 보통 / 철수: 좋아함 / 목이: 보통 / 바람이: 보통 |
| **업그레이드 재료** | **강화 낚싯대 (`rod_reinforced`) 재료**: 구리 광석 x5 필요 (-> see `docs/systems/fishing-system.md` 섹션 1.1). 대장장이(철수)에게 의뢰 |
| **힌트 텍스트** | "동굴 벽면에 붉은 빛을 띠는 광맥. 대장장이가 이 광석을 좋아한다고 한다." |
| **SFX 제안** | `sfx_gather_ore` |

### 7.3 철 광석 (#24)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_iron_ore` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.7) |
| `maxStack` | 30 |
| 카테고리 | `Mineral` |
| **가공 연계** | 가공소(일반)에서 제련: 철 광석 x3 -> 철 조각 x1 (`recipe_smelt_iron`) (-> see `docs/content/processing-system.md` 섹션 3.7.4) |
| **NPC 선물** | 하나: 보통 / 철수: 좋아함 / 목이: 보통 / 바람이: 보통 |
| **업그레이드 재료** | [DES-020 확정] 철 광석 x3을 가공소(일반)에서 제련하여 철 조각(`iron_scrap`) x1 생산. 상점 구매(100G)의 보조 경로로, 직판 포기 36G + 가공 2시간의 트레이드오프. (-> see `docs/content/processing-system.md` 섹션 3.7.4) |
| **힌트 텍스트** | "무겁고 단단한 회색 광석. 녹여서 다듬으면 좋은 도구의 재료가 될 수 있다." |
| **SFX 제안** | `sfx_gather_ore` |

### 7.4 금 광석 (#25)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_gold_ore` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.7) |
| `maxStack` | 30 |
| 카테고리 | `Mineral` |
| **가공 연계** | 없음 (업그레이드 재료 전용) |
| **NPC 선물** | 하나: 좋아함 / 철수: 좋아함 / 목이: 보통 / 바람이: 좋아함 |
| **업그레이드 재료** | **전설 낚싯대 (`rod_legendary`) 재료**: 금 광석 x3 필요 (-> see `docs/systems/fishing-system.md` 섹션 1.1). 대장장이(철수)에게 의뢰 |
| **힌트 텍스트** | "동굴 깊은 곳에서 금빛으로 반짝이는 귀한 광석. 전설적인 도구를 만드는 데 꼭 필요하다." |
| **SFX 제안** | `sfx_gather_ore` |

### 7.5 수정 원석 (#26)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_crystal` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.7) |
| `maxStack` | 30 |
| 카테고리 | `Mineral` |
| **가공 연계** | [OPEN] **수정 장식품** 제안 보류 (수정 원석 x2 -> 수정 장식품). FIX-083에서 미채택 — 광물 가공 레시피 범주. 광물 가공 확장 시 검토 |
| **NPC 선물** | 하나: 좋아함 / 철수: 보통 / 목이: 좋아함 / 바람이: 좋아함 |
| **업그레이드 재료** | 해당 없음 |
| **힌트 텍스트** | "투명하게 빛나는 원석. 빛을 받으면 무지갯빛 광채를 뿜는다." |
| **SFX 제안** | `sfx_gather_crystal` |

### 7.6 자수정 (#27)

| 속성 | 값 |
|------|-----|
| 영문 ID | `gather_amethyst` |
| 희귀도/판매가 | (-> see `docs/systems/gathering-system.md` 섹션 3.7) |
| `maxStack` | 10 |
| 카테고리 | `Mineral` |
| **가공 연계** | [OPEN] **자수정 목걸이** 제안 보류 (자수정 x1 + 금 광석 x1 -> 자수정 목걸이). FIX-083에서 미채택 — 광물 가공 레시피 범주. 광물 가공 확장 시 검토 |
| **NPC 선물** | 하나: 좋아함 / 철수: 좋아함 / 목이: 좋아함 / 바람이: 좋아함 (전원 좋아함 — Legendary 특전. 특히 NPC 호감도 시스템 도입 시 핵심 선물 아이템) |
| **업그레이드 재료** | 해당 없음 (NPC 선물/장식 전용) |
| **힌트 텍스트** | "깊은 보라빛의 아름다운 보석. 선물하면 누구든 감동할 것이다. 동굴에서도 쉽게 만나기 어렵다." |
| **SFX 제안** | `sfx_gather_crystal` |

---

## 8. NPC 선물 적합도 요약

### 8.1 NPC별 선호 채집물 요약

NPC 호감도 시스템은 현재 미설계 상태이다 (-> see `docs/content/npcs.md` 섹션 하단 [OPEN] 항목). 본 섹션은 호감도 시스템 도입 시 적용할 선물 적합도 기준 데이터를 사전 정의한다.

**선물 등급 정의**:
- **좋아함**: 호감도 +3 (도입 시)
- **보통**: 호감도 +1
- **싫어함**: 호감도 -1

#### 하나 (잡화 상점 상인, `npc_hana`)

| 선호도 | 채집물 |
|--------|--------|
| 좋아함 | 달래, 봄나물, 제비꽃, 산딸기, 으름 열매, 송이(봄), 능이버섯, 표고버섯(야생), 머루, 송이버섯, 눈꽃 이끼, 동충하초, 영지버섯, 금 광석, 수정 원석, 자수정 + 전 Legendary |
| 보통 | 민들레, 쑥, 연잎, 겨울 나무껍질, 구리 광석, 철 광석, 도토리 |
| 싫어함 | 돌 조각 |

**성격 반영**: 상인인 하나는 식재료/꽃/보석 등 상품 가치가 있는 아이템을 좋아하고, 돌 조각처럼 상품 가치가 낮은 것을 싫어한다.

#### 철수 (대장간 장인, `npc_cheolsu`)

| 선호도 | 채집물 |
|--------|--------|
| 좋아함 | 송이(봄), 능이버섯, 송이버섯, 동충하초, 구리 광석, 철 광석, 금 광석, 자수정 + 전 Legendary |
| 보통 | 달래, 봄나물, 제비꽃, 산딸기, 쑥, 으름 열매, 연잎, 표고버섯(야생), 머루, 눈꽃 이끼, 돌 조각, 도토리, 수정 원석, 영지버섯 |
| 싫어함 | 민들레 |

**성격 반영**: 대장간 장인인 철수는 광석류와 고급 식재료(버섯)를 좋아하며, 꽃을 받으면 어색해한다.

#### 목이 (목공소 장인, `npc_moki`)

| 선호도 | 채집물 |
|--------|--------|
| 좋아함 | 봄나물, 제비꽃, 연잎, 눈꽃 이끼, 겨울 나무껍질, 도토리, 수정 원석, 자수정, 쑥, 영지버섯 + 전 Legendary |
| 보통 | 민들레, 달래, 산딸기, 으름 열매, 송이(봄), 능이버섯, 표고버섯(야생), 머루, 송이버섯, 동충하초, 돌 조각, 구리 광석, 철 광석, 금 광석 |
| 싫어함 | 없음 |

**성격 반영**: 목공소 장인인 목이는 자연물 전반을 좋아하며, 특히 나무/식물 관련 아이템과 장식용 원석을 선호한다. 싫어하는 것이 없는 온화한 성격.

#### 바람이 (여행 상인, `npc_barami`)

| 선호도 | 채집물 |
|--------|--------|
| 좋아함 | 제비꽃, 산딸기, 으름 열매, 송이(봄), 머루, 송이버섯, 눈꽃 이끼, 동충하초, 영지버섯, 표고버섯(야생), 금 광석, 수정 원석, 자수정 + 전 Legendary |
| 보통 | 민들레, 달래, 봄나물, 쑥, 연잎, 도토리, 능이버섯, 겨울 나무껍질, 구리 광석, 철 광석 |
| 싫어함 | 돌 조각 |

**성격 반영**: 여행 상인인 바람이는 희귀하고 이국적인 아이템을 좋아하며, 흔한 돌 조각에는 관심이 없다.

### 8.2 설계 원칙

- **Legendary 아이템은 전원 좋아함**: Legendary 채집물(산삼, 황금 연꽃, 천년 영지)은 모든 NPC가 좋아하도록 설정. 극희소 아이템을 선물하는 행위에 항상 보상을 보장한다.
- **NPC 직업 반영**: 철수는 광석, 하나는 상품성 높은 식재료/꽃, 목이는 자연물, 바람이는 희귀한 것을 선호하는 패턴.
- **싫어하는 아이템은 최소화**: 대부분의 아이템이 보통 이상. "선물이 역효과"인 상황을 최소화하여 채집 선물 행위를 부정적 경험으로 만들지 않는다.
- **자수정의 특별 지위**: 유일한 Legendary 광물로, 전 NPC가 좋아함. NPC 호감도 시스템 도입 시 핵심 선물 아이템.

---

## 9. 가공 레시피 연계 요약

### 9.1 확정 채집물 가공 레시피 (13종) — FIX-083 확정

아래 13종 레시피는 FIX-083(2026-04-07)에서 `docs/content/processing-system.md` 섹션 3.7에 canonical 정의가 완료되었다. 판매가는 BAL-016(40% 하향) 이후 원재료 가격을 기반으로 설계되었다.

| # | 가공품명 | 레시피 ID | 가공소 | 판매가 | 원재료 대비 배수 | 참조 |
|---|----------|----------|--------|:------:|:---------------:|------|
| 1 | 야생 베리잼 | `recipe_gather01` | 가공소 | 26G | 1.30x | 섹션 3.7.1 |
| 2 | 도토리묵 | `recipe_gather02` | 가공소 | 20G | 1.33x | 섹션 3.7.1 |
| 3 | 건조 버섯 (능이) | `recipe_gather03` | 가공소 | 24G | 1.33x | 섹션 3.7.1 |
| 4 | 건조 버섯 (표고) | `recipe_gather04` | 가공소 | 36G | 1.50x | 섹션 3.7.1 |
| 5 | 건조 영지 | `recipe_gather05` | 가공소 | 84G | 1.75x | 섹션 3.7.1 |
| 6 | 황금 연꽃차 | `recipe_gather06` | 가공소 | 300G | 3.00x | 섹션 3.7.1 |
| 7 | 천년 영지차 | `recipe_gather07` | 가공소 | 360G | 3.00x | 섹션 3.7.1 |
| 8 | 나무껍질 차 | `recipe_gather08` | 가공소 | 15G | 1.50x | 섹션 3.7.1 |
| 9 | 동충하초 환 | `recipe_gather09` | 가공소 | 140G | 1.75x | 섹션 3.7.1 |
| 10 | 머루 와인 | `recipe_gather10` | 발효실 | 90G | 2.00x | 섹션 3.7.2 |
| 11 | 산삼주 | `recipe_gather11` | 발효실 | 280G | 3.50x | 섹션 3.7.2 |
| 12 | 봄나물 비빔밥 | `recipe_gather12` | 베이커리 | 60G | 3.33x | 섹션 3.7.3 |
| 13 | 송이 구이 | `recipe_gather13` | 베이커리 | 70G | 2.19x | 섹션 3.7.3 |

모든 레시피의 재료, 수량, 가공 시간, 해금 조건 등 상세 사양은 (-> see `docs/content/processing-system.md` 섹션 3.7).

### 9.2 미채택 레시피 (보류/향후 검토)

FIX-083에서 채택되지 않은 레시피. 각각의 보류 사유를 명시한다.

| 가공품명 | 보류 사유 | 재검토 시점 |
|----------|----------|------------|
| 쑥떡 | 쌀(`rice`) 획득 경로 미확정 | 쌀 경로 확정 후 |
| 연잎밥 | 쌀(`rice`) 획득 경로 미확정 | 쌀 경로 확정 후 |
| 건조 버섯 (송이/봄) | 봄 송이는 Rare급으로 채집량 적어 가공보다 직판 유리 | 봄 가공 레시피 확장 시 |
| 꽃다발 (봄) | 장식/NPC 선물 전용. 스코프 외 | 장식 시스템 도입 시 |
| 수정 장식품 | 광물 가공 레시피 범주. 스코프 외 | 광물 가공 확장 시 |
| 자수정 목걸이 | 광물 가공 레시피 범주. 스코프 외 | 광물 가공 확장 시 |

### 9.3 가공 부가가치 분석 (FIX-083 + DES-019 확정 수치)

| 원재료 | 원재료 판매가 합계 (BAL-016) | 가공품 판매가 (확정) | 부가가치 비율 | 평가 |
|--------|:--------------------------:|:-------------------:|:------------:|------|
| 산딸기 x5 | 20G | 26G | 1.30x | 적정 (Common 최저 부가가치) |
| 도토리 x5 | 15G | 20G | 1.33x | 적정 (Common) |
| 능이버섯 x3 | 18G | 24G | 1.33x | 적정 (Common 버섯) |
| 표고(야생) x2 | 24G | 36G | 1.50x | 적정 (Uncommon) |
| 영지버섯 x2 | 48G | 84G | 1.75x | 적정 (Rare) |
| 머루 x5 | 45G | 90G | 2.00x | 적정 (Uncommon + 48시간 발효) |
| 달래x2+봄나물x1 | 18G | 60G | 3.33x | 적정 (복합 재료 + 연료 30G, DES-019 상향) |
| 송이버섯 x1 | 32G | 70G | 2.19x | 적정 (Rare + 연료 30G, DES-019 상향) |
| 동충하초 x2 | 80G | 140G | 1.75x | 적정 (겨울 Rare) |
| 겨울 나무껍질 x5 | 10G | 15G | 1.50x | 적정 (겨울 소소한 가공) |
| 산삼 x1 | 80G | 280G | 3.50x | 적정 (Legendary + 72시간 발효) |
| 황금 연꽃 x1 | 100G | 300G | 3.00x | 적정 (Legendary) |
| 천년 영지 x1 | 120G | 360G | 3.00x | 적정 (Legendary) |

**이전 대비 변화 (BAL-016 전후)**:
- BAL-016 이전의 예시 가격(~25G, ~40G 등)은 원재료 투입 대비 손해를 유발했다
- FIX-083에서 모든 레시피를 원재료 총 직판가 대비 1.3x 이상으로 재설정하여 가공 동기를 보장
- Legendary 아이템의 배수(3.0~3.5x)는 극희소 원재료의 가치를 반영하되, 이전 제안(~500~700G)보다 보수적으로 조정

---

## 10. 도구/낚싯대 업그레이드 재료 역할 요약

### 10.1 낚싯대 업그레이드 재료

| 업그레이드 | 필요 광석 | 수량 | 기타 비용 | 참조 |
|-----------|-----------|:----:|-----------|------|
| 기본 -> 강화 낚싯대 | 구리 광석 (`gather_copper_ore`) | x5 | 1,500G | (-> see `docs/systems/fishing-system.md` 섹션 1.1) |
| 강화 -> 전설 낚싯대 | 금 광석 (`gather_gold_ore`) | x3 | 4,000G | (-> see `docs/systems/fishing-system.md` 섹션 1.1) |

### 10.2 도구 업그레이드 재료 — 철 광석 제련 경로 (DES-020 확정)

도구 업그레이드 재료인 철 조각(`iron_scrap`)은 두 가지 경로로 획득 가능하다:

| 경로 | 방법 | 비용 | 소요 시간 |
|------|------|------|----------|
| **상점 구매** | 대장간에서 직접 구매 | 100G/개 (→ see `docs/systems/tool-upgrade.md` 섹션 6.3) | 즉시 |
| **채집 제련** | 철 광석 x3 -> 가공소 제련 | 36G 기회비용(철 광석 직판 포기, → see `docs/systems/gathering-system.md` 섹션 3.7) + 가공 2시간 | 철 광석 수집 ~25일/3개 (→ see `docs/systems/gathering-system.md` 섹션 3.8) |

**설계 의도**: 상점 구매는 "골드로 시간을 산다", 채집 제련은 "시간으로 골드를 아낀다". Reinforced 업그레이드(철 조각 x3) 전체를 제련으로 충당하면 192G 절감되지만 철 광석 9개 수집에 ~75일이 소요된다. 대부분의 플레이어는 상점 구매를 주 경로로 사용하되, 장기 채집으로 1~2개를 보조적으로 제련하는 혼합 전략이 최적이 된다.

레시피 상세: (-> see `docs/content/processing-system.md` 섹션 3.7.4)
도구 업그레이드 비용 상세: (-> see `docs/systems/tool-upgrade.md` 섹션 2.2)

---

## 11. 수집 효과음 ID 제안

### 11.1 SFX ID 컨벤션

sound-design.md의 SFX ID 패턴(`sfx_` prefix + 시스템_행위)을 따른다 (-> see `docs/systems/sound-design.md`).

### 11.2 채집 SFX 목록 제안

[OPEN] 아래 SFX ID는 제안이다. sound-design.md에 채집 카테고리 SFX를 추가할 때 채택 여부를 결정한다.

| SFX ID | 트리거 | 음향 특성 제안 | 대상 아이템 |
|--------|--------|--------------|-----------|
| `sfx_gather_flower` | 꽃 채집 시 | 부드러운 "톡" + 풀잎 스치는 소리, 0.3초 | 민들레, 제비꽃 |
| `sfx_gather_herb` | 식물/약초 채집 시 | 풀 뽑는 "쓱" + 흙 떨어지는 소리, 0.3초 | 달래, 봄나물, 쑥, 눈꽃 이끼 |
| `sfx_gather_berry` | 열매 채집 시 | 가지에서 따는 "뚝" + 잎사귀 흔들림, 0.3초 | 산딸기, 으름 열매, 머루 |
| `sfx_gather_mushroom` | 버섯 채집 시 | 부드러운 "폭" + 습한 흙 소리, 0.3초 | 송이(봄), 영지버섯, 능이버섯, 표고버섯(야생), 송이버섯, 동충하초 |
| `sfx_gather_leaf` | 잎/수생 식물 채집 시 | 물기 있는 잎 "사각" 소리, 0.3초 | 연잎 |
| `sfx_gather_nut` | 열매/도토리 채집 시 | 딱딱한 열매 줍는 "톡톡", 0.25초 | 도토리 |
| `sfx_gather_bark` | 나무껍질 채집 시 | 껍질 벗기는 "바삭", 0.3초 | 겨울 나무껍질 |
| `sfx_gather_stone` | 돌 채집 시 | 돌 줍는 "탁", 0.2초 | 돌 조각 |
| `sfx_gather_ore` | 광석 채집 시 | 금속성 "깡" + 파편 소리, 0.4초 | 구리 광석, 철 광석, 금 광석 |
| `sfx_gather_crystal` | 원석/보석 채집 시 | 맑은 결정 "딩" + 반짝임 레이어, 0.4초 | 수정 원석, 자수정 |
| `sfx_gather_legendary` | Legendary 아이템 채집 시 | 신비로운 상승 징글 + 반짝임 잔향, 1.0초 | 산삼, 황금 연꽃, 천년 영지 |

**설계 원칙**:
- 기본 채집음(꽃/약초/버섯 등)은 0.2~0.4초의 짧고 경쾌한 소리로 "수집했다"는 즉각적 피드백 제공
- 광물 채집음은 경작/도구 SFX와 유사한 금속성 질감으로 차별화
- Legendary 채집음은 `sfx_crop_golden`이나 `sfx_fish_catch_rare`와 유사한 "특별한 발견" 느낌의 징글로, 드문 이벤트의 감동을 강조
- 각 SFX는 Variation 2~3개를 권장하여 반복 채집 시 단조로움 방지 (-> see `docs/systems/sound-design.md` Variation 규칙)

---

## 12. 힌트 텍스트 요약 테이블

전 27종의 힌트 텍스트를 일괄 확인용으로 정리한다. 각 아이템 상세 섹션(3~7)이 canonical이다.

| # | 아이템명 | 힌트 텍스트 |
|---|---------|------------|
| 1 | 민들레 | "따뜻한 봄바람에 노란 꽃잎을 흔드는 작은 꽃. 숲 어디서나 만날 수 있다." |
| 2 | 달래 | "봄 숲의 은은한 향기를 따라가면 만나는 작은 풀. 알싸한 맛이 요리에 깊이를 더한다." |
| 3 | 봄나물 | "이슬 맺힌 숲 가장자리에서 자라는 부드러운 나물. 봄의 향긋함이 가득하다." |
| 4 | 제비꽃 | "덤불 사이에 숨듯 피어나는 보라빛 작은 꽃. 선물하면 누구나 미소 짓는다." |
| 5 | 송이 (봄) | "비 내린 다음 날 숲에서 발견되는 귀한 버섯. 진한 향이 코끝을 간질인다." |
| 6 | 산삼 | "봄비 내린 깊은 숲속, 오랜 세월을 견딘 신비로운 뿌리. 숙련된 채집가만이 발견할 수 있다." |
| 7 | 산딸기 | "여름 덤불에 주렁주렁 매달린 빨간 열매. 한 알 베어 물면 새콤달콤한 맛이 퍼진다." |
| 8 | 쑥 | "숲과 초원 어디서든 만날 수 있는 질긴 약초. 독특한 향이 몸과 마음을 달래준다." |
| 9 | 으름 열매 | "여름 덤불 깊은 곳에 매달린 보라빛 열매. 껍질을 까면 달콤한 과육이 나온다." |
| 10 | 연잎 | "연못 위에 둥글게 펼쳐진 초록 잎. 이슬을 담은 모양이 아름답다." |
| 11 | 영지버섯 | "비 온 다음 날 숲 깊은 곳에서 자라는 단단한 버섯. 윤기 나는 갈색 갓이 특징이다." |
| 12 | 황금 연꽃 | "맑은 여름 새벽, 연못에서 금빛으로 빛나는 신비로운 꽃. 숙련된 눈만이 발견할 수 있다." |
| 13 | 도토리 | "가을이 되면 숲 바닥에 수북이 쌓이는 작은 열매. 묵으로 만들면 쫄깃한 별미가 된다." |
| 14 | 능이버섯 | "가을 숲의 낙엽 사이에서 고개를 내미는 향긋한 버섯. 국물 재료로 최고다." |
| 15 | 표고버섯 (야생) | "숲속 고목에 붙어 자라는 야생 표고. 재배산보다 진한 향이 난다." |
| 16 | 머루 | "가을 덤불에 매달린 검푸른 작은 포도. 발효시키면 깊은 맛의 와인이 된다." |
| 17 | 송이버섯 | "소나무 뿌리 아래 숨어 자라는 가을의 보물. 그윽한 향이 숲 전체에 퍼진다." |
| 18 | 천년 영지 | "흐린 가을 날, 숲의 가장 깊은 곳에서 발견되는 전설의 버섯. 천 년의 기운이 서려 있다." |
| 19 | 겨울 나무껍질 | "겨울 숲에서 벗겨진 마른 나무껍질. 차로 우리면 은은한 나무 향이 난다." |
| 20 | 눈꽃 이끼 | "눈 덮인 바위 틈에 피어나는 하얀 이끼. 눈이 내리는 날에만 모습을 보인다." |
| 21 | 동충하초 | "겨울 눈 속에 숨은 희귀한 약재. 눈이 내린 날, 숙련된 채집가만이 찾아낼 수 있다." |
| 22 | 돌 조각 | "동굴 입구에 흔하게 굴러다니는 작은 돌 조각. 쌓아두면 건축 재료로 쓸 수 있다." |
| 23 | 구리 광석 | "동굴 벽면에 붉은 빛을 띠는 광맥. 대장장이가 이 광석을 좋아한다고 한다." |
| 24 | 철 광석 | "무겁고 단단한 회색 광석. 녹여서 다듬으면 좋은 도구의 재료가 될 수 있다." |
| 25 | 금 광석 | "동굴 깊은 곳에서 금빛으로 반짝이는 귀한 광석. 전설적인 도구를 만드는 데 꼭 필요하다." |
| 26 | 수정 원석 | "투명하게 빛나는 원석. 빛을 받으면 무지갯빛 광채를 뿜는다." |
| 27 | 자수정 | "깊은 보라빛의 아름다운 보석. 선물하면 누구든 감동할 것이다. 동굴에서도 쉽게 만나기 어렵다." |

---

## Cross-references

| 참조 문서 | 관련 섹션 | 연관 내용 |
|-----------|----------|-----------|
| `docs/systems/gathering-system.md` 섹션 3.3~3.7 | 본 문서 전체 | 27종 아이템 기본 정보 (이름, ID, 희귀도, 판매가) canonical |
| `docs/systems/gathering-system.md` 섹션 9.1 | 본 문서 섹션 9.1 | 가공 레시피 예시 방향 |
| `docs/systems/gathering-system.md` 섹션 7 | 본 문서 섹션 7.2, 7.4 | 여행 상인 광석 판매 |
| `docs/content/processing-system.md` | 본 문서 섹션 9 | 가공 레시피 canonical (신규 레시피 추가 대상) |
| `docs/systems/fishing-system.md` 섹션 1.1 | 본 문서 섹션 10.1 | 낚싯대 업그레이드 재료 요건 |
| `docs/systems/tool-upgrade.md` 섹션 2.2 | 본 문서 섹션 10.2 | 도구 업그레이드 재료 (철 광석 제련 경로 DES-020 확정) |
| `docs/content/processing-system.md` 섹션 3.7.4 | 본 문서 섹션 7.3, 10.2 | 철 광석 제련 레시피 canonical (DES-020) |
| `docs/content/npcs.md` | 본 문서 섹션 8 | NPC 캐릭터 설정, 호감도 시스템 [OPEN] |
| `docs/systems/inventory-system.md` 섹션 1.1 | 본 문서 섹션 2 | 아이템 카테고리 확장 제안 (Gathered, Mineral) |
| `docs/systems/sound-design.md` | 본 문서 섹션 11 | SFX ID 컨벤션, 채집 SFX 추가 대상 |
| `docs/content/fish-catalog.md` | 본 문서 구조 | 문서 형식 참고 (콘텐츠 카탈로그 패턴) |
| `docs/systems/economy-system.md` | 본 문서 섹션 9.3 | 가공 부가가치 밸런스 |

---

## Open Questions

1. [OPEN] **inventory-system.md 카테고리 확장**: `Gathered`, `Mineral` 카테고리 추가 필요. 또는 기존 카테고리(`Crop`, `Material`)에 통합할지 결정 필요.
2. ~~[OPEN] **채집물 가공 레시피 6종 상세**~~ — **[FIX-083 완료]** processing-system.md 섹션 3.7에 13종 채집물 가공 레시피 확정 완료.
3. ~~[OPEN] **신규 가공 레시피 13종 채택 여부**~~ — **[FIX-083 완료]** 13종 채택, 6종 보류 (섹션 9.2 참조).
4. ~~[OPEN] **가공 부가가치 재조정**~~ — **[FIX-083 완료]** 모든 레시피를 원재료 대비 1.3x 이상으로 재설정 (섹션 9.3 참조).
5. ~~[OPEN] **철 광석의 도구 업그레이드 재료 활용**~~ — **[DES-020 완료]** 방안 A(가공소 제련) 채택. 철 광석 x3 -> 철 조각 x1 (`recipe_smelt_iron`). (-> see `docs/content/processing-system.md` 섹션 3.7.4).
6. [OPEN] **쌀(`rice`) 획득 경로**: 쑥떡, 연잎밥 레시피는 쌀이 필요. fishing-system.md 섹션 8 [OPEN] 항목과 연계하여 확정 필요. 확정 시 processing-system.md에 추가 레시피 등록.
7. [OPEN] **NPC 호감도 시스템**: npcs.md에 호감도 시스템이 미설계. 본 문서의 선물 적합도 데이터는 호감도 시스템 도입 시 적용 예정.
8. [OPEN] **sound-design.md에 채집 SFX 추가**: 본 문서에서 제안한 11종 SFX ID를 sound-design.md에 정식 등록 필요.

---

## Risks

1. ~~[RISK] **가공 부가가치 역전**~~ — **[FIX-083 해소]** 모든 레시피가 원재료 대비 1.3x 이상으로 재설정됨. 가공이 항상 직판보다 유리하도록 보장.
2. [RISK] **Legendary 가공품 가격**: 산삼주(280G), 황금 연꽃차(300G), 천년 영지차(360G)가 게임 경제에 미치는 영향. FIX-083에서 이전 제안(500~700G)보다 보수적으로 조정했으나, Legendary 아이템 자체가 극희소(출현율 0.5~1%, 특수 조건 필요)이므로 단기 경제 파괴 위험은 낮음. 장기적으로 축적 판매 시 밸런스 검증 필요.
3. [RISK] **NPC 선물 적합도의 단조로움**: 현재 4 NPC 모두 Legendary를 좋아하고, 싫어하는 아이템이 극소수. NPC 간 차별화가 약할 수 있다. 호감도 시스템 도입 시 NPC별 "매우 좋아함(Love)" 등급을 추가하여 특정 NPC-아이템 조합의 특별한 반응을 만드는 것을 권장.

---

*이 문서는 27종 채집 아이템의 콘텐츠 상세(maxStack, 힌트 텍스트, NPC 선물 적합도, 가공 레시피 연계, 도구/낚싯대 재료 역할, SFX 제안)의 canonical 문서이다. 판매가/희귀도/계절 조건은 gathering-system.md가 canonical이며, 가공 레시피 정식 정의는 processing-system.md가 canonical이다.*

---

# 목축/낙농 시스템 콘텐츠 (Livestock & Dairy System)

> 작성: Claude Code (Opus 4.6) | 2026-04-07
> 문서 ID: CON-006 | Phase 1

---

## Context

이 문서는 SeedMind의 목축/낙농 시스템에 등장하는 **동물 종류, 돌봄 사이클, 생산물, 시설, 행복도 메카닉, 경제 밸런스**를 상세히 기술한다. Zone E(`zone_south_meadow`, 12x8, 96타일)를 기반으로 동물 사육 시스템을 정의하며, 치즈 공방(Cheese Workshop) 활성화의 선행 조건이 되는 문서다.

**설계 목표**:
- 경작 시스템과 병렬적인 수익원을 제공하여, 플레이 스타일 분기(작물 지향 vs 축산 지향)를 뒷받침
- "매일 동물을 돌봐야 한다"는 일일 루틴 의무감과, 그에 따른 보상의 리듬을 생성
- 초기 투자 비용이 높고, 수익이 시간에 걸쳐 축적되는 장기 투자 구조로 작물과 차별화
- 가공 시스템(치즈 공방)과 연계하여 중후반 경제 깊이를 확장

### 본 문서가 canonical인 데이터

- 동물 종류, 동물 ID, 구매 가격, 일일 사료 비용, 생산물, 생산 주기
- 생산물 목록, 생산물 ItemID, 기본 판매가
- 동물 돌봄 사이클 (먹이, 쓰다듬기, 수집), 에너지 소모
- 행복도(Happiness) 시스템 수치, 행복도-생산량 연동 공식
- 외양간/닭장 시설 기본 정보 (수용 수, 업그레이드 경로)
- 동물 시설 건설 비용
- 방치 패널티 규칙

### 본 문서가 canonical이 아닌 데이터 (참조만)

| 데이터 종류 | 참조처 |
|------------|--------|
| Zone E 크기, 해금 비용, 해금 레벨 | `docs/systems/farm-expansion.md` 섹션 2.1 |
| Zone E 구역 특성 (사용 가능 타일, 울타리) | `docs/systems/farm-expansion.md` 섹션 4.2 |
| 에너지 시스템 (최대 에너지, 회복, 도구 소모) | `docs/systems/farming-system.md` 섹션 3.2 |
| XPSource enum, 경험치 부여 로직 | `docs/systems/progression-architecture.md` 섹션 2.2~2.3 |
| 가격 시스템 (마진 비율, 수급 변동) | `docs/systems/economy-system.md` 섹션 2 |
| 가공품 레시피 (치즈 공방) | (미작성, 치즈 공방 문서에서 정의 예정) |
| 레벨별 해금 테이블, XP 요건 | `docs/balance/progression-curve.md` 섹션 2.4.1 |
| 시설 점유 타일, 건설 규칙 | `docs/content/facilities.md` 섹션 2 |

---

## 1. 동물 종류 및 기본 데이터

### 1.1 동물 목록

SeedMind에는 **4종의 동물**이 존재한다. 각 동물은 크기(소형/중형/대형), 생산물 종류, 생산 주기가 다르며, 플레이어에게 "어떤 동물을 우선 구매할지"의 전략적 선택을 제공한다.

| 동물 | ID | 크기 등급 | 구매가 | 일일 사료비 | 생산물 | 생산 주기 | 타일 점유 |
|------|-----|----------|--------|-----------|--------|----------|----------|
| 닭 (Chicken) | `animal_chicken` | 소형 | 800G | 10G | 달걀 | 매일 | 2x2 (4타일) |
| 염소 (Goat) | `animal_goat` | 중형 | 2,000G | 20G | 염소젖 | 2일 | 2x3 (6타일) |
| 소 (Cow) | `animal_cow` | 대형 | 4,000G | 30G | 우유 | 2일 | 3x3 (9타일) |
| 양 (Sheep) | `animal_sheep` | 중형 | 3,000G | 25G | 양모 | 3일 | 2x3 (6타일) |

**구매 장소**: NPC 목축상(Rancher) — 마을 상점에서 구매 (-> see `docs/content/npcs.md`)

**구매 가격 설계 근거**:
- 닭(800G): Zone E 해금(4,000G) 직후 닭장(1,500G)과 함께 구매 가능한 진입 동물. 봄~여름 수익으로 충당 가능
- 염소(2,000G): 닭보다 고가이나 치즈 공방 연계 시 부가가치 높음
- 소(4,000G): 가장 높은 초기 투자. 우유 직판가와 치즈 가공 수익이 이를 정당화
- 양(3,000G): 양모의 높은 단가로 3일 주기를 보상. 가공 연계 없이도 직판 효율 우수

### 1.2 동물 크기 등급 및 시설 요건

| 크기 등급 | 해당 동물 | 필요 시설 | 방목 필요 |
|----------|----------|----------|----------|
| 소형 | 닭 | 닭장 (Chicken Coop) | 불필요 (닭장 내 생활) |
| 중형 | 염소, 양 | 외양간 (Barn) | 맑은 날 목초지 방목 권장 |
| 대형 | 소 | 외양간 (Barn) | 맑은 날 목초지 방목 권장 |

---

## 2. 동물 돌봄 사이클

### 2.1 일일 루틴

플레이어는 매일 아침 동물을 돌봐야 한다. 돌봄 행위는 3단계로 구성된다.

```
[아침 시작] → [1. 먹이 주기(Feed)] → [2. 쓰다듬기(Pet)] → [3. 생산물 수집(Collect)]
```

**루틴 순서는 강제가 아닌 권장**: 어떤 순서로 해도 기능상 문제 없으나, 먹이를 주지 않으면 생산물이 나오지 않고, 쓰다듬기를 하지 않으면 행복도가 오르지 않는다.

### 2.2 먹이 주기 (Feed)

| 파라미터 | 값 |
|----------|-----|
| 조작 | 사료통(Feeder) 상호작용 (E키) |
| 에너지 소모 | 2 (동물 1마리당) |
| 사료 소모 | 사료 아이템 1개/마리/일 |
| 미급이 시 | 해당 동물 생산물 없음, 행복도 -10/일 |

**사료 구매**: 사료(Feed)는 NPC 목축상에서 구매한다. 단가는 동물별 일일 사료비 참조 (섹션 1.1 테이블).

| 사료 아이템 | ID | 가격 | 대상 동물 |
|------------|-----|------|----------|
| 모이 (Poultry Feed) | `item_poultry_feed` | 10G | 닭 |
| 건초 (Hay) | `item_hay` | 20G | 염소 |
| 프리미엄 건초 (Premium Hay) | `item_premium_hay` | 30G | 소 |
| 목초 (Pasture Grass) | `item_pasture_grass` | 25G | 양 |

[OPEN] 목초지(Zone E) 타일에 "풀"을 재배하여 사료 비용을 절감하는 메카닉을 도입할지 검토 필요. 도입 시 사료 직접 재배가 가능해져 운영비가 줄지만, 경작 가능 타일이 사료 재배에 소모되는 트레이드오프가 생긴다.

### 2.3 쓰다듬기 (Pet)

| 파라미터 | 값 |
|----------|-----|
| 조작 | 동물에게 다가가 상호작용 (E키) |
| 에너지 소모 | 1 (동물 1마리당) |
| 행복도 효과 | +5/일 (1일 1회만 유효) |
| 미실행 시 | 행복도 변화 없음 (감소하지는 않음) |

**쓰다듬기 UI 피드백**: 동물 머리 위에 하트 이펙트 표시. 행복도가 높을수록 하트 크기/색상 변화 (회색 → 분홍 → 빨강).

### 2.4 생산물 수집 (Collect)

| 파라미터 | 값 |
|----------|-----|
| 조작 | 동물 옆에서 상호작용 (E키) 또는 닭장/외양간 내 수집함 |
| 에너지 소모 | 1 (동물 1마리당) |
| 수집 가능 조건 | 전날 먹이 제공 + 생산 주기 충족 |
| 미수집 시 | 생산물 1일분까지 저장됨. 2일 이상 미수집 시 소멸 |

### 2.5 에너지 소모 요약

동물 1마리 기준 일일 돌봄 에너지 소모:

| 행위 | 에너지 |
|------|--------|
| 먹이 주기 | 2 |
| 쓰다듬기 | 1 |
| 생산물 수집 | 1 |
| **합계** | **4/마리/일** |

플레이어 최대 에너지: (-> see `docs/systems/farming-system.md` 섹션 3.2). 기본 100 에너지 기준, 동물 6마리 돌봄 = 24 에너지로 일일 에너지의 약 24%를 소모한다. 이는 작물 경작 활동과의 에너지 분배 선택을 강제한다.

---

## 3. 시설 (외양간, 닭장)

### 3.1 닭장 (Chicken Coop)

| 파라미터 | Lv.1 | Lv.2 |
|----------|------|------|
| 영문 ID | `building_chicken_coop` | `building_chicken_coop_2` |
| 건설 비용 | 1,500G | 3,000G (업그레이드) |
| 건설 시간 | 2일 | 3일 |
| 점유 타일 | 3x3 (9타일) | 3x3 (9타일) |
| 수용 수 | 4마리 | 8마리 |
| 내부 기능 | 사료통 x1, 수집함 x1 | 사료통 x2, 수집함 x2, 자동 급이기(하루 사료 1회 자동) |
| 해금 조건 | 레벨 6 (Zone E 해금과 동시) | 레벨 8 |

**Lv.2 자동 급이기**: 사료 아이템이 인벤토리에 있으면 아침에 자동으로 소모하여 먹이를 준다. 쓰다듬기와 수집은 여전히 수동.

### 3.2 외양간 (Barn)

| 파라미터 | Lv.1 | Lv.2 | Lv.3 |
|----------|------|------|------|
| 영문 ID | `building_barn` | `building_barn_2` | `building_barn_3` |
| 건설 비용 | 3,000G | 5,000G (업그레이드) | 8,000G (업그레이드) |
| 건설 시간 | 3일 | 4일 | 5일 |
| 점유 타일 | 4x4 (16타일) | 4x4 (16타일) | 4x4 (16타일) |
| 수용 수 | 4마리 (중/대형) | 8마리 (중/대형) | 12마리 (중/대형) |
| 내부 기능 | 사료통 x1, 수집함 x1 | 사료통 x2, 수집함 x2, 히터(겨울 행복도 감소 방지) | 전 Lv.2 + 자동 급이기 |
| 해금 조건 | 레벨 6 (Zone E 해금과 동시) | 레벨 8 | 레벨 9 |

**히터 효과 (Lv.2)**: 겨울철 외양간 내 동물의 행복도 감소 페널티(섹션 5.3)를 무효화한다. 연료 소모 없음 (시설 자체 기능).

### 3.3 시설 배치 규칙

- 닭장과 외양간은 **Zone E 내부에만** 건설 가능
- Zone E 사용 가능 타일: ~80타일 (-> see `docs/systems/farm-expansion.md` 섹션 4.2)
- 최대 배치 예시: 닭장(9타일) x1 + 외양간(16타일) x1 = 25타일 사용. 잔여 55타일은 방목 공간
- 닭장과 외양간은 인접 배치 불가 (최소 1타일 간격)
- 사료통은 시설 내부에 포함 (별도 배치 불필요)

[OPEN] Zone E 이외 구역에 동물 시설 건설 허용 여부. 현재는 Zone E 전용으로 제한하되, 게임 후반 "제2 목장" 확장이 필요할 수 있다.

---

## 4. 생산물 목록 및 판매가

### 4.1 기본 생산물

| 생산물 | ID | 생산 동물 | 기본 판매가 | 생산 주기 | 비고 |
|--------|-----|----------|-----------|----------|------|
| 달걀 (Egg) | `item_egg` | 닭 | 35G | 매일 | 가장 안정적 수입원 |
| 염소젖 (Goat Milk) | `item_goat_milk` | 염소 | 80G | 2일 | 치즈 공방 가공 가능 |
| 우유 (Milk) | `item_milk` | 소 | 120G | 2일 | 치즈 공방 가공 가능 |
| 양모 (Wool) | `item_wool` | 양 | 150G | 3일 | 가공 없이도 고가 |

### 4.2 고품질 생산물

행복도가 최대(200)일 때, 일정 확률로 고품질 생산물이 나온다.

| 생산물 | ID | 기본 대비 | 판매가 | 발생 확률 |
|--------|-----|----------|--------|----------|
| 대형 달걀 (Large Egg) | `item_egg_large` | 달걀 상위 | 70G | 행복도 200 시 25% |
| 고급 염소젖 (Gold Goat Milk) | `item_goat_milk_gold` | 염소젖 상위 | 160G | 행복도 200 시 20% |
| 고급 우유 (Gold Milk) | `item_milk_gold` | 우유 상위 | 240G | 행복도 200 시 20% |
| 고급 양모 (Gold Wool) | `item_wool_gold` | 양모 상위 | 300G | 행복도 200 시 15% |

**고품질 발생 확률 공식**:
```
고품질 확률(%) = max(0, (happiness - 150) * 0.5)
  - happiness 150 미만: 0%
  - happiness 175: 12.5%
  - happiness 200 (최대): 25% (닭) / 20% (염소, 소) / 15% (양)
```

[OPEN] 고품질 확률에 동물별 차등을 둘지, 통일할지 검토. 현재는 양모가 기본 판매가가 높으므로 고품질 확률을 낮게 설정하여 밸런스를 맞추는 방향으로 설계.

### 4.3 생산물 가격 설계 근거

생산물 가격은 **일일 순수익(판매가 - 사료비) / 생산 주기**가 작물 중급(토마토~옥수수) 수준과 비슷하되, 초기 투자(동물 구매 + 시설 건설) 회수에 시간이 걸리도록 설계했다.

| 동물 | 일일 순수익 | 계산 근거 |
|------|-----------|----------|
| 닭 | 25G/일 | (35G 판매 - 10G 사료) / 1일 |
| 염소 | 20G/일 | (80G 판매 - 20G x 2일 사료) / 2일 |
| 소 | 30G/일 | (120G 판매 - 30G x 2일 사료) / 2일 |
| 양 | 25G/일 | (150G 판매 - 25G x 3일 사료) / 3일 |

> 비교: 감자(3일 성장, 30G 판매 - 15G 씨앗 = 15G 순이익, 5G/일), 토마토(5일 성장, 60G 판매 - 30G 씨앗 = 30G 순이익, 6G/일). 동물은 일일 순수익이 훨씬 높으나, 초기 투자가 수천 골드 수준이라 ROI 회수에 수십 일이 필요하다.

---

## 5. 행복도(Happiness) 시스템

### 5.1 행복도 기본 규칙

| 파라미터 | 값 |
|----------|-----|
| 최소 행복도 | 0 |
| 최대 행복도 | 200 |
| 초기 행복도 (구매 직후) | 100 |
| 행복도 저장 단위 | 정수 (int) |

### 5.2 행복도 변동 요인

| 요인 | 변동량 | 조건 |
|------|--------|------|
| 먹이 주기 | +3/일 | 매일 먹이 제공 시 (기본 의무) |
| 쓰다듬기 | +5/일 | 1일 1회 유효 |
| 방목 (맑은 날) | +2/일 | 외양간 문 열기 + 맑은 날씨 |
| 먹이 미제공 | -10/일 | 하루라도 빼먹으면 |
| 비/폭풍 중 방목 | -3/일 | 비 오는 날 밖에 나갈 경우 |
| 겨울 실외 노출 | -5/일 | 겨울철 외양간 없이 방치 (Lv.2 히터로 해소) |
| 동물 과밀 | -2/일 | 시설 수용 한도의 80% 이상 사용 시 |

**행복도 최대치 도달 속도**: 초기 100에서 매일 먹이(+3) + 쓰다듬기(+5) + 방목(+2) = +10/일. 최대 200까지 10일 소요. 방치 1일(-10)은 약 1일치 증가를 소멸시킨다.

### 5.3 행복도 - 생산 연동

행복도는 생산물의 **수량 배수**와 **품질 등급**에 영향을 준다.

```
생산 배수 = happinessMultiplierCurve(happiness)

  happiness 0~49:    배수 0.0 (생산 불가)
  happiness 50~99:   배수 0.5 (절반 생산 -- 2회 중 1회 생산 스킵)
  happiness 100~149: 배수 1.0 (정상 생산)
  happiness 150~199: 배수 1.0 + 고품질 확률 적용 (섹션 4.2)
  happiness 200:     배수 1.0 + 고품질 확률 최대
```

**생산물 품질 등급 임계값 (LivestockConfig 캐노니컬 수치)**:

| 파라미터 | 값 | 설명 |
|----------|-----|------|
| `silverQualityThreshold` | **150** | 행복도 150 이상 시 Silver 품질 적용 시작 |
| `goldQualityThreshold` | **175** | 행복도 175 이상 시 Gold 품질 적용 시작 |

> 행복도 150~174: Normal 또는 Silver (섹션 4.2 확률 참조), 175~200: Gold 가능. 이 임계값은 `LivestockConfig.silverQualityThreshold` / `goldQualityThreshold` 필드의 canonical 출처이다.

**설계 의도**: 행복도 0~49는 "학대 수준"으로, 경고 UI와 함께 생산이 멈춘다. 50~99는 방치 페널티. 100 이상이 정상 운영이며, 150+ 구간에서 고품질 보너스가 시작되어 꾸준한 돌봄을 보상한다.

### 5.4 방치 패널티 상세

| 방치 일수 | 누적 효과 |
|----------|----------|
| 1일 | 행복도 -10, 당일 생산물 없음 |
| 2일 연속 | 행복도 -10/일 (누적 -20), 미수집 생산물 소멸 |
| 3일 연속 | 행복도 -10/일 (누적 -30), 경고 메시지 표시 |
| 7일 연속 | 행복도 0 도달 가능, 동물 "슬픔" 상태 (회복에 5일간 먹이+쓰다듬기 필요) |

**동물 사망 없음**: SeedMind에서 동물은 사망하지 않는다. 방치해도 행복도가 0으로 떨어지고 생산이 멈출 뿐이다. 이는 게임 톤(밝고 따뜻한 파스텔)과 핵심 감정(성장의 보람)에 부합하는 설계 결정이다.

---

## 6. Zone E 활용 가이드

### 6.1 구역 기본 정보

Zone E(`zone_south_meadow`)의 구조적 파라미터는 farm-expansion.md에서 정의한다.

| 파라미터 | 참조 |
|----------|------|
| 크기, 위치, 해금 비용/레벨 | (-> see `docs/systems/farm-expansion.md` 섹션 1.3, 2.1) |
| 사용 가능 타일, 울타리, 토양 품질 | (-> see `docs/systems/farm-expansion.md` 섹션 4.2) |

### 6.2 타일 용도 분류 (96타일 기준)

| 용도 | 타일 수 | 비고 |
|------|---------|------|
| 울타리 경계 | ~16타일 | 자동 배치, 변경 불가 |
| 닭장 (Lv.1) | 9타일 | 필수 시설 1동 |
| 외양간 (Lv.1) | 16타일 | 필수 시설 1동 |
| 시설 간 간격 | ~3타일 | 최소 1타일 이격 규칙 |
| 방목 공간 | ~52타일 | 동물 이동/방목 영역 |

**최대 동물 수 (기본 시설)**: 닭장 Lv.1(4마리) + 외양간 Lv.1(4마리) = **8마리**
**최대 동물 수 (최대 업그레이드)**: 닭장 Lv.2(8마리) + 외양간 Lv.3(12마리) = **20마리**

### 6.3 방목 시스템

- **방목 활성화**: 외양간/닭장의 문을 열면(상호작용) 동물이 Zone E 방목 공간에 나옴
- **방목 조건**: 맑은 날에만 행복도 보너스 (+2/일). 비/폭풍 시 페널티 (-3/일)
- **자동 귀환**: 18:00(저녁)에 동물이 자동으로 시설 내부로 복귀
- **방목 중 수집**: 방목 중인 동물에게 직접 상호작용하여 생산물 수집 가능

---

## 7. 치즈 공방 연계 (선행 조건)

### 7.1 치즈 공방 활성화 조건

`docs/content/processing-system.md` 섹션에서 "[OPEN] 치즈 공방은 동물 시스템이 없어 도입 불가"로 표기된 치즈 공방(Cheese Workshop)은 본 문서의 목축 시스템을 선행 조건으로 한다.

**치즈 공방 기본 사양 (예비)**:

| 파라미터 | 값 |
|----------|-----|
| 영문 ID | `building_cheese_workshop` |
| 건설 비용 | 4,500G |
| 해금 조건 | 레벨 8 + 외양간 Lv.1 보유 |
| 점유 타일 | 3x4 (12타일) |
| 배치 구역 | Zone E 또는 Zone A~D (미확정) |
| 연료 사용 | **없음** (-> see `docs/content/processing-system.md` 섹션 4.1) |

### 7.2 확정 가공 레시피 (CON-009 확정)

치즈 공방 레시피 5종은 `docs/content/processing-system.md` 섹션 3.6에 canonical로 확정되었다 (CON-009, 2026-04-07).

**연료비**: 없음. 치즈 공방은 연료 소모 없이 운영한다 (-> see `docs/content/processing-system.md` 섹션 4.1).

| 원재료 | 확정 가공품 | 실제 부가가치 |
|--------|-----------|------------|
| 우유 (`item_milk`) | 치즈 (Cheese) 250G | 직판 120G 대비 1.8x |
| 우유 (`item_milk`) | 버터 (Butter) 160G | 직판 대비 1.4x |
| 염소젖 (`item_goat_milk`) | 염소 치즈 (Goat Cheese) 190G | 직판 80G 대비 2.0x |
| 치즈 (`item_cheese`) | 에이지드 치즈 (Aged Cheese) 680G | 치즈 재투입 2차 가공 |
| 우유 + 달걀 | 크림 (Cream) 280G | 복합 재료, 베이커리 연계 |

레시피 상세(처리 시간, 해금 조건) → see `docs/content/processing-system.md` 섹션 3.6.

[RESOLVED-CON-009] 치즈 공방 레시피 5종이 `docs/content/processing-system.md` 섹션 3.6에 추가됨. 총 레시피 수 56종으로 확정.

### 7.3 XPSource 확장 필요

동물 돌봄 및 생산물 수확은 새로운 XP 소스로 추가되어야 한다.

XPSource enum 확장 정의 및 GetExpForSource() 추가 case는 기술 아키텍처를 참조한다:
**-> see `docs/systems/livestock-architecture.md` 섹션 7.1~7.3 (ARC-019)**

**XP 수치 제안**:
| 행위 | XP | 비고 |
|------|-----|------|
| 먹이 주기 | 2 XP/마리 | 일일 루틴 보상 |
| 쓰다듬기 | 1 XP/마리 | 선택적 돌봄 보상 |
| 생산물 수집 (일반) | 5 XP/개 | CropHarvest와 유사 수준 |
| 생산물 수집 (고품질) | 10 XP/개 | 고품질 보너스 |

[RISK] `XPSource` enum에 `AnimalCare`, `AnimalHarvest`를 추가할 경우, `progression-architecture.md` 섹션 2.2의 enum 정의와 섹션 2.3의 `GetExpForSource()` switch 문을 동시에 업데이트해야 한다.

---

## 8. 경제 밸런스 개요

### 8.1 초기 투자 비용

Zone E 해금 후 목축 시작까지의 최소 투자:

| 항목 | 비용 |
|------|------|
| Zone E 해금 | 4,000G (-> see `docs/systems/farm-expansion.md` 섹션 2.1) |
| 닭장 Lv.1 건설 | 1,500G |
| 닭 x2 구매 | 1,600G |
| **최소 시작 비용** | **7,100G** |

**확장 시나리오 (외양간 포함)**:

| 항목 | 비용 |
|------|------|
| Zone E 해금 | 4,000G |
| 외양간 Lv.1 건설 | 3,000G |
| 소 x1 + 닭장 Lv.1 + 닭 x2 | 4,000G + 1,500G + 1,600G |
| **확장 시작 비용** | **14,100G** |

### 8.2 일일/계절별 수익 시뮬레이션

**시나리오 A: 최소 투자 (닭 x2)**

| 기간 | 일일 수입 | 일일 사료비 | 일일 순수익 | 누적 순수익 |
|------|----------|-----------|-----------|-----------|
| Day 1~28 (첫 계절) | 70G (달걀 x2) | 20G | 50G | 1,400G |
| Day 29~56 (둘째 계절) | 70G | 20G | 50G | 2,800G |
| Day 57~84 (셋째 계절) | 70G | 20G | 50G | 4,200G |

닭 x2 초기 투자(3,100G, Zone E 제외) 회수: **62일** (약 2.2계절)

**시나리오 B: 중간 투자 (닭 x4 + 소 x1)**

| 기간 | 일일 수입 | 일일 사료비 | 일일 순수익 | 누적 순수익 |
|------|----------|-----------|-----------|-----------|
| Day 1~28 | 200G (달걀 x4 + 우유 x0.5) | 70G | 130G | 3,640G |
| Day 29~56 | 200G | 70G | 130G | 7,280G |

> 우유 수입은 2일 주기이므로 일평균 60G(120G/2일)

닭 x4 + 소 x1 초기 투자(11,700G, Zone E 제외, 외양간 포함) 회수: **90일** (약 3.2계절)

> 외양간 Lv.1(3,000G) 필수 포함. 소 사육에는 외양간이 선행 요건이다 (섹션 3.2).

### 8.3 Zone E 투자 총 ROI

**Zone E 전체 비용 (해금 + 최소 시설 + 동물) 대비 회수 기간**:

| 시나리오 | 총 투자 | 일일 순수익 | 회수 기간 | 비고 |
|---------|---------|-----------|----------|------|
| 최소 (닭 x2) | 7,100G | 50G/일 | 142일 (~5계절) | 보수적 진입, 외양간 불필요 |
| 중간 (닭 x4 + 소 x1) | 15,700G | 130G/일 | 121일 (~4.3계절) | 권장 경로 (외양간 포함) |
| 적극 (닭 x4 + 소 x2 + 염소 x2) | 23,700G | 230G/일 | 103일 (~3.7계절) | 빠른 확장 (외양간 포함) |

**비교 기준**: 작물 전용 Zone D(숲 지형, 2,500G)의 예상 ROI는 약 1~2계절 (-> see `docs/systems/farm-expansion.md` 섹션 5.3). 목축은 회수 기간이 길지만, 한번 자리잡으면 매일 안정적 수입을 제공하며 씨앗 재구매가 불필요하다.

### 8.4 가공 연계 시 수익 향상 (치즈 공방)

치즈 공방 가공이 활성화되면 우유/염소젖의 부가가치가 높아진다. 레시피 및 판매가는 (-> see `docs/content/processing-system.md` 섹션 3.6).

| 시나리오 | 일일 순수익 (직판) | 일일 순수익 (가공 포함) | 증가율 |
|---------|-----------------|----------------------|--------|
| 중간 (닭 x4 + 소 x1) | 130G/일 | ~258G/일 | +98% |
| 적극 (닭 x4 + 소 x2 + 염소 x2) | 230G/일 | ~410G/일 | +78% |

> 가공 포함 수치는 `docs/balance/livestock-economy.md` 섹션 2.1~2.2 기준. 치즈(250G) / 염소 치즈(190G) / 에이지드 치즈(680G, 16시간 체인) 반영.

[RESOLVED] CON-009 완료 — 치즈 공방 레시피 5종 확정 (2026-04-07)

---

## 9. Cross-references

| 문서 | 관련 내용 |
|------|----------|
| `docs/design.md` 섹션 4.6 | 시설 목록 -- 외양간/닭장/치즈 공방 추가 완료 (FIX-041) |
| `docs/systems/farm-expansion.md` 섹션 4.2 | Zone E 구역 상세 -- CON-006 완료로 [OPEN] 해소 |
| `docs/systems/farm-expansion-architecture.md` 섹션 2.2 | ZoneType.Pasture 활성화 |
| `docs/systems/farming-system.md` 섹션 3.2 | 에너지 시스템 -- 동물 돌봄 에너지 소모 |
| `docs/systems/economy-system.md` 섹션 2 | 가격 시스템 -- 생산물 가격 연동 |
| `docs/systems/progression-architecture.md` 섹션 2.2 | XPSource enum -- AnimalCare, AnimalHarvest 추가 |
| `docs/content/processing-system.md` 섹션 | 치즈 공방 [OPEN] 해소 -- 레시피 추가 필요 |
| `docs/content/npcs.md` | NPC 목축상(Rancher) 추가 필요 |
| `docs/balance/processing-economy.md` | 치즈 공방 ROI 분석 추가 필요 |
| `docs/content/facilities.md` 섹션 2 | 시설 건설 규칙 -- 외양간/닭장 포함 |
| `docs/balance/progression-curve.md` | 레벨 6~9 해금 콘텐츠에 목축 시설 반영 |

---

## 10. Open Questions / Risks

### Open Questions

1. [OPEN] **사료 자급자족**: 목초지(Zone E) 타일에 "풀"을 재배하여 사료 비용을 절감하는 메카닉 도입 여부 (섹션 2.2)
2. [OPEN] **고품질 확률 차등**: 동물별 고품질 생산물 확률에 차등을 둘지 통일할지 (섹션 4.2)
3. [RESOLVED-CON-009] **치즈 공방 문서 위치**: `docs/content/processing-system.md` 섹션 3.6에 레시피 5종 추가 확정. 연료비 없음 (섹션 4.1 canonical).
4. [OPEN] **치즈 공방 배치 구역**: Zone E 전용인지, 다른 Zone에도 건설 가능한지 (섹션 7.1)
5. [OPEN] **Zone E 이외 동물 시설**: 게임 후반 "제2 목장" 확장 필요성 (섹션 3.3)
6. [OPEN] **동물 구매 NPC**: 기존 NPC 시스템에 목축상(Rancher) 추가 필요. 상시 판매인지, 특정 요일/계절 한정인지 (섹션 1.1)
7. [OPEN] **겨울철 동물 관리**: 겨울에도 사료 구매 가능한지, 겨울 전용 사료가 필요한지 검토 (섹션 5.2)

### Risks

1. [RISK] **XPSource enum 확장 동기화**: AnimalCare, AnimalHarvest 추가 시 `progression-architecture.md` 섹션 2.2 enum 및 섹션 2.3 GetExpForSource() switch 문을 동시에 업데이트해야 한다 (섹션 7.3)
2. [RESOLVED] **design.md 시설 테이블 동기 완료**: 외양간/닭장/치즈 공방이 `docs/design.md` 섹션 4.6 시설 목록에 추가되었다 (FIX-041)
3. [RISK] **에너지 밸런스 붕괴 가능성**: 동물 수가 늘어나면 일일 돌봄 에너지(4/마리)가 작물 경작 에너지를 압박할 수 있다. 최대 20마리 시 80 에너지 = 기본 에너지의 80% 소모. 도구 업그레이드 에너지 절감과 연계 필요
4. [RISK] **Zone E 타일 부족**: 닭장 + 외양간 + 방목 공간 배분에서, 최대 업그레이드 시 동물 20마리의 방목 타일이 부족할 수 있다. 동물 타일 점유와 방목 공간 규칙의 시뮬레이션 필요
5. [RISK] **경제 밸런스 미검증**: ROI 시뮬레이션은 이론값. 작물 수익과의 교차 밸런스(동시 운영 시)는 별도 BAL 문서에서 종합 분석 필요

---

*이 문서는 Claude Code가 프로젝트 기존 설계와의 일관성을 분석하여 자율적으로 작성했습니다.*

---

# 시장 상인 NPC 상세 문서 (Merchant NPC Detail Specification)

> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: CON-008a | Phase 1

---

## 1. Context

이 문서는 시장 상인 NPC "하나(Hana)"의 캐릭터 심화 설계, 전체 대화 스크립트, 상점 인터페이스 UX, 관계 발전 단계를 상세히 기술한다. `docs/content/npcs.md` 섹션 3에 정의된 기본 설정을 확장하며, `docs/systems/economy-system.md`의 상점 메카닉과 완전히 연동된다.

**설계 목표**: 하나는 플레이어가 가장 자주 만나는 NPC로, 밝고 친근한 성격을 통해 "농사의 동반자"라는 느낌을 준다. 대화가 단순한 상점 진입점이 아니라, 계절별 작물 선택에 대한 간접 가이드 역할을 수행하고, 경제 시스템의 핵심 허브로서 플레이어의 구매/판매 전략을 자연스럽게 유도하도록 설계한다.

### 1.1 본 문서가 canonical인 데이터

- 하나 캐릭터 심화 설정 (배경 스토리, 성격 디테일, 관계 발전 단계)
- 시장 상인 전체 대화 스크립트 (상황별, 계절별, 친밀도별, 특수 이벤트별)
- 상점 인터페이스 UX 세부 설계 (구매/판매 화면 레이아웃, 피드백)
- 하나의 관계 발전 단계 임계값 및 단계별 보상

### 1.2 본 문서가 canonical이 아닌 데이터 (참조만)

| 데이터 종류 | 참조처 |
|------------|--------|
| NPC 기본 정보 (이름, 나이, 외형 요약) | `docs/content/npcs.md` 섹션 3.1 |
| 영업시간, 휴무일 | `docs/systems/economy-system.md` 섹션 3.2 |
| 씨앗 가격, 작물 판매가, 성장일수 | `docs/design.md` 섹션 4.2 |
| 비료 종류, 가격, 해금 | `docs/systems/farming-system.md` 섹션 5.1 |
| 대량 구매 할인율 | `docs/systems/economy-system.md` 섹션 4.3 |
| 잡화 상점 인벤토리 구성 | `docs/systems/economy-system.md` 섹션 3.3 |
| 상점 공통 UI 흐름 | `docs/content/npcs.md` 섹션 8 |
| NPC 대화 시스템 구조 (트리거, 우선순위) | `docs/content/npcs.md` 섹션 7 |
| 레벨별 해금 콘텐츠 | `docs/balance/progression-curve.md` 섹션 1.3.2 |

---

## 2. 캐릭터 설계

### 2.1 기본 정보

| 항목 | 내용 |
|------|------|
| 이름 | 하나 (Hana) |
| 영문 ID | `npc_hana` |
| 나이 | 30대 초반 |
| 성별 | 여성 |
| 역할 | 시장 상인 (씨앗/비료/소모품 판매, 작물/가공품 매입) |
| 위치 | 마을 중앙, 잡화 상점 내부 |

기본 캐릭터 설정은 (-> see `docs/content/npcs.md` 섹션 3.1)을 따른다.

### 2.2 외형 상세

로우폴리 3D 스타일에 맞춘 캐릭터 디자인.

| 요소 | 묘사 |
|------|------|
| 체형 | 평균 체형, 활동적인 인상. 항상 약간 앞으로 기울어진 자세 (손님을 맞이하는 자세) |
| 피부 | 건강한 밀색 피부. 볼에 살짝 홍조 (로우폴리에서 밝은 분홍 패치로 표현) |
| 머리 | 짧은 갈색 단발. 한쪽을 귀 뒤로 넘긴 스타일 |
| 모자 | 밀짚모자 (넓은 챙, 초록색 리본 장식) — 원거리 실루엣의 핵심 식별 요소 |
| 복장 | 초록색 앞치마 위에 크림색 블라우스. 앞치마 주머니에 작은 씨앗 봉투가 보인다 |
| 신발 | 갈색 가죽 부츠 (작업용) |
| 특징적 요소 | 앞치마 끈에 매달린 작은 저울 장식 (상인의 상징) |
| 대기 애니메이션 | 카운터 뒤에서 씨앗 봉투를 정리하거나, 손으로 턱을 받치고 밖을 바라보는 동작 반복 |

**로우폴리 표현 가이드**: 밀짚모자의 넓은 실루엣과 초록색 앞치마가 원거리 식별의 핵심이다. 밝은 색상 톤(크림+초록)이 마을 중앙의 생기를 나타내며, 대장간 철수의 어두운 톤과 시각적으로 대비된다.

### 2.3 성격 상세

| 측면 | 설명 |
|------|------|
| 말투 | 밝고 경쾌한 존댓말. 감탄사를 자주 사용("와!", "좋아요!", "대단해요!"). 문장 끝에 물결표(~) 느낌의 부드러운 어조 |
| 작물에 대한 태도 | 씨앗과 작물에 대한 풍부한 지식을 보유. 각 계절별 추천 작물을 자연스럽게 언급한다 |
| 플레이어에 대한 태도 | 처음부터 친절하고 환대한다. 관계가 발전하면 농사 파트너처럼 플레이어의 성장을 함께 기뻐한다 |
| 감정 표현 | 감정을 직접적으로 표현한다. 기쁨, 놀라움, 걱정을 솔직하게 드러낸다 |
| 숨은 면모 | 겨울에 장사가 안 될 때 살짝 울적해지는 모습. 비 오는 날에는 빗소리를 좋아한다고 말한다 |

### 2.4 배경 스토리

하나는 이 마을에서 3대째 이어온 잡화 상점 "들꽃상점"의 주인이다. 할머니가 마을 개척 시절에 처음 가게를 열었고, 어머니가 이어받았으며, 하나가 어린 시절부터 상점에서 일하며 자연스럽게 물려받았다.

어릴 때부터 씨앗과 작물에 관심이 많아, 직접 소규모 텃밭을 가꾸면서 작물 품종별 특성을 체득했다. 상점에서 파는 씨앗을 직접 길러 보고 품질을 확인한 뒤에야 매대에 올리는 것이 그녀의 원칙이다. "내가 키워 보지 않은 씨앗은 팔지 않아요"가 좌우명.

마을에 새 농부가 오면 진심으로 기뻐하며, 농사가 잘 되기를 바라는 마음에서 은근히 팁을 흘린다. 목이, 철수와는 오래된 이웃 사이로, 가끔 세 사람이 마을 광장에서 대화하는 모습을 볼 수 있다.

**게임 내 노출**: 배경 스토리는 친밀도 단계에 따라 단편적으로 드러난다. 초반에는 상인으로서의 면모만 보이고, 반복 방문과 거래를 통해 텃밭 이야기, 할머니 이야기 등이 조금씩 나온다.

### 2.5 관계 발전 단계

플레이어와 하나의 관계는 4단계로 발전한다. 단계 전환은 친밀도 포인트 누적(작물 판매 +1, 씨앗/비료 구매 +1, 일일 대화 +1)에 기반한다.

| 단계 | 영문 키 | 친밀도 임계값 | 관계 묘사 |
|------|---------|-------------|-----------|
| 첫 손님 | `Stranger` | 0 (초기) | 친절하지만 일반적인 상인-손님 관계. 기본 안내 대사 |
| 단골 손님 | `Acquaintance` | 10 | 이름을 부르기 시작. 계절별 추천 작물을 적극적으로 안내 |
| 농사 친구 | `Regular` | 25 | 작물 재배 팁을 자발적으로 제공. 텃밭 이야기를 꺼내기 시작 |
| 소꿉친구 | `Friend` | 50 | 할머니 이야기, 개인적인 고민을 털어놓음. **씨앗 구매 10% 할인 혜택** 제공 |

**임계값 canonical**: 이 테이블의 친밀도 임계값(`[0, 10, 25, 50]`)이 canonical이다. NPC 아키텍처의 `affinityThresholds`는 이 테이블을 참조한다.

**설계 의도**: 하나의 친밀도 시스템은 철수(blacksmith-npc.md 섹션 2.5)와 동일한 구조를 공유한다. 임계값도 동일하게 설정하여 플레이어가 NPC 간 관계 발전 속도를 비교할 수 있게 한다. 보상은 NPC별로 차별화한다(철수: 재료 할인, 하나: 씨앗 할인).

---

## 3. 대화 시스템 설계

### 3.1 최초 만남 대화

게임 시작 후 하나에게 처음 말을 걸 때 1회만 재생된다.

```
[하나] "어서 와요! 새로 농장을 시작한 분이군요.
        저는 하나, 이 마을의 잡화 상점을 하고 있어요.
        씨앗이 필요하면 언제든 들러요. 감자부터 시작하는 게 좋을 거예요!"
```

**설계 의도**: 3문장으로 (1) 환영, (2) 자기소개, (3) 초기 작물 가이드를 완료한다. 감자를 추천함으로써 플레이어에게 첫 작물 선택의 안전한 선택지를 제시한다(감자는 가장 낮은 비용과 짧은 성장기).

### 3.2 일반 방문 대화

재방문 시 랜덤으로 하나가 출력된다. 동일 대사 연속 재생 방지 규칙은 (-> see `docs/content/npcs.md` 섹션 7.4)을 따른다.

#### 범용 대사 (계절 무관, 5종)

| ID | 대사 |
|----|------|
| `hana_general_01` | "안녕하세요! 오늘은 뭘 찾으세요?" |
| `hana_general_02` | "좋은 아침이에요! 오늘 날씨가 농사하기 딱 좋네요." |
| `hana_general_03` | "또 오셨군요! 뭐가 필요하신지 구경해 보세요." |
| `hana_general_04` | "하루가 빨라요, 그쵸? 농사일이 그렇더라고요." |
| `hana_general_05` | "항상 들러 주셔서 기분 좋아요! 뭐 도와드릴까요?" |

#### 계절별 대사 (각 계절 3종, 총 12종)

| 계절 | ID | 대사 |
|------|-----|------|
| 봄 | `hana_spring_01` | "봄이에요! 감자랑 당근 씨앗이 잘 나가요. 올해 첫 수확이 기대되네요." |
| 봄 | `hana_spring_02` | "봄에는 뭐든 잘 자라요. 딸기 씨앗도 있는데, 좀 비싸지만 값어치를 해요!" |
| 봄 | `hana_spring_03` | "봄바람이 좋죠? 저도 뒤뜰 텃밭에 당근을 심었어요." |
| 여름 | `hana_summer_01` | "여름에는 토마토가 정말 잘 자라요. 옥수수도 도전해 보세요, 시간은 좀 걸리지만 값어치를 해요." |
| 여름 | `hana_summer_02` | "해바라기 씨앗 들어왔어요! 밭에 해바라기가 피면 정말 예뻐요." |
| 여름 | `hana_summer_03` | "더운 날에는 물 주는 걸 절대 빼먹으면 안 돼요! 작물이 말라 버려요." |
| 가을 | `hana_autumn_01` | "가을 호박은 정말 대단해요. 크게 키우면 대박이 날 수도 있다는 얘기 들어 봤어요?" |
| 가을 | `hana_autumn_02` | "수확의 계절이에요! 출하함이 꽉 차겠네요, 좋겠다~" |
| 가을 | `hana_autumn_03` | "가을 당근은 봄 당근이랑 맛이 다르대요. 저도 심어 봤는데 진짜 그래요!" |
| 겨울 | `hana_winter_01` | "겨울이라 씨앗은 없지만, 온실이 있으면 한겨울에도 농사할 수 있어요!" |
| 겨울 | `hana_winter_02` | "겨울에는 가게가 한산해요... 비료 정리나 해야겠어요." |
| 겨울 | `hana_winter_03` | "추운 날에는 따뜻한 차 한 잔이 최고예요. 농장 일도 중요하지만 쉬는 것도 중요해요!" |

**대사 출력 규칙**: 현재 계절에 해당하는 계절 대사 3종 + 범용 대사 5종 = 총 8종 풀에서 랜덤 선택. 가중치는 계절 대사 40%, 범용 대사 60%.

### 3.3 구매/판매 상황 대사

플레이어가 상점에서 거래할 때 출력되는 대사.

#### 구매 관련

| 상황 | 대사 |
|------|------|
| 씨앗 구매 시 | "좋은 선택이에요! 잘 키워 보세요~" |
| 비료 구매 시 | "비료를 쓰면 작물 품질이 확 달라져요. 추천!" |
| 대량 구매 시 (10개 이상) | "와, 많이 사시네요! 대량 할인 드릴게요~" |
| 해금 직후 아이템 첫 구매 | "드디어 이걸 살 수 있게 됐군요! 오래 기다리셨죠?" |

#### 판매 관련

| 상황 | 대사 |
|------|------|
| 일반 품질 작물 판매 | "고마워요! 잘 키우셨네요." |
| 고품질(Silver) 작물 판매 | "오, 은빛이에요! 잘 키우셨는데요? 좋은 가격 드릴게요." |
| 최고품질(Gold) 작물 판매 | "와! 금빛 작물이에요! 대단해요, 정말 잘 키우셨어요!" |
| Iridium 품질 작물 판매 | "이건... 처음 봐요! 이렇게 완벽한 작물이라니! 최고 가격 드릴게요!" |
| 가공품 판매 | "가공품이네요! 원재료보다 훨씬 좋은 가격이에요. 현명하시네요~" |
| 골드 부족으로 구매 실패 | "앗, 좀 부족하네요... 작물 팔아서 모으면 금방이에요!" |

### 3.4 특수 대화

특정 조건에서 1회만 트리거되는 대화.

| 조건 | 트리거 | 대사 |
|------|--------|------|
| 첫 번째 작물 수확 후 방문 | `flag:firstHarvest` | "첫 수확 축하드려요! 기분 어때요? 출하함에 넣으면 다음 날 정산돼요. 아니면 여기서 바로 팔 수도 있어요!" |
| 레벨 3 도달 | `minLevel:3` | "벌써 이만큼 성장하셨군요! 고급 비료도 들여놨으니 한번 써 보세요. 작물 품질이 확 올라갈 거예요." |
| 레벨 5 도달 | `minLevel:5` | "이제 진짜 농부시네요! 호박이랑 해바라기 씨앗도 준비됐어요. 도전해 보세요!" |
| 레벨 7 도달 | `minLevel:7` | "수박 씨앗, 드디어 입고됐어요! 비싸지만 그만한 가치가 있을 거예요. 여름에 심어 보세요!" |
| 겨울 진입 + 온실 미보유 | `season:winter AND !flag:hasGreenhouse` | "겨울에 할 일이 없죠? 온실을 지으면 겨울에도 농사할 수 있어요. 목이한테 가 보세요!" |
| 겨울 진입 + 온실 보유 | `season:winter AND flag:hasGreenhouse` | "온실이 있으니까 겨울에도 농사할 수 있겠네요! 여행 상인 바람이가 겨울 전용 씨앗을 가져올 수도 있어요." |
| 첫 Gold 품질 작물 판매 | `flag:firstGoldCrop` | "금빛 작물! 이건 정말 특별해요. 비료를 잘 쓰면 이런 작물이 더 나온대요!" |
| 누적 판매 100회 달성 | `flag:sales100` | "벌써 100번째 거래예요! 저한테 이렇게 자주 와 주는 분은 처음이에요. 정말 고마워요!" |

### 3.5 작물 추천 힌트 대사

하나의 고유 역할은 "간접 가이드"이다. 플레이어의 상황을 파악하고 적절한 작물/비료를 추천한다.

| 힌트 대사 | 조건 | 친밀도 조건 |
|-----------|------|------------|
| "당근은 가격은 낮지만 빨리 자라요. 현금이 급하면 당근이 최고예요!" | 플레이어 골드 < 300G + 봄/가을 | 단골(`Acquaintance`) 이상 |
| "옥수수는 오래 걸리지만 한 번 심으면 계속 수확할 수 있어요. 여유가 있으면 도전해 보세요!" | 여름 + 옥수수 미경험 | 단골(`Acquaintance`) 이상 |
| "비료를 쓰면 같은 작물이라도 은빛이나 금빛으로 나올 수 있어요. 이익이 확 달라져요!" | 비료 미사용 + 레벨 3 이상 | 농사 친구(`Regular`) 이상 |
| "호박은 가을에만 심을 수 있는데, 가끔 엄청 크게 자란대요. Giant라고 하던가?" | 가을 + 호박 미경험 | 농사 친구(`Regular`) 이상 |
| "수박은 여름 작물 중에 제일 비싸요. 비료까지 쓰면 대박이에요!" | 여름 + 레벨 7 이상 + 수박 미경험 | 농사 친구(`Regular`) 이상 |
| "가공소가 있으면 작물을 가공해서 훨씬 비싸게 팔 수 있어요. 목이한테 얘기해 보세요!" | 가공소 미보유 + 레벨 7 이상 | 농사 친구(`Regular`) 이상 |

**쿨다운**: 힌트 대사는 같은 힌트가 3일에 1번까지만 반복 (-> see `docs/content/npcs.md` 섹션 7.4).

### 3.6 친밀도 단계별 대화

관계 발전 단계(섹션 2.5)에 따라 추가되는 대사.

#### 첫 손님 (Stranger)

기본 범용/계절 대사만 출력. 추가 대사 없음.

#### 단골 손님 (Acquaintance)

단계 전환 시 1회성 특수 대사:

```
[하나] "요즘 자주 와 주시네요! 이름이... 맞다, {플레이어명}님!
        앞으로 좋은 씨앗 들어오면 제가 먼저 알려 드릴게요."
```

기존 대사 풀에 다음이 추가된다.

| ID | 대사 |
|----|------|
| `hana_acquaint_01` | "{플레이어명}님, 오늘도 오셨군요! 뭘 찾으세요?" |
| `hana_acquaint_02` | "요즘 농장이 잘 되고 있는 것 같아서 저까지 기뻐요!" |
| `hana_acquaint_03` | "다음 계절에 뭘 심을지 미리 생각해 두면 좋아요. 제가 도와드릴게요!" |

#### 농사 친구 (Regular)

단계 전환 시 1회성 특수 대사:

```
[하나] "{플레이어명}님, 사실 저도 뒤뜰에 작은 텃밭이 있거든요.
        이번에 키운 당근이 잘 됐어요! 가끔 농사 이야기 같이 해요."
```

기존 대사 풀에 다음이 추가된다.

| ID | 대사 |
|----|------|
| `hana_regular_01` | "오늘 제 텃밭의 토마토가 빨갛게 익었어요! 보고 싶으시면 언제든 구경 오세요." |
| `hana_regular_02` | "이건 비밀인데요... 씨앗을 심기 전에 하루 물에 불려 두면 싹이 더 빨리 나온대요. 게임에선 안 되지만요, 헤헤." |
| `hana_regular_03` | "철수 아저씨한테 도구 업그레이드 받으셨어요? 좋은 도구가 좋은 농사의 시작이래요. 철수 아저씨가 맨날 그러더라고요." |

#### 소꿉친구 (Friend)

단계 전환 시 1회성 특수 대사:

```
[하나] "{플레이어명}님... 솔직히 저, 처음에 새 농부가 온다고 했을 때 걱정했거든요.
        이전에도 왔다가 금방 떠난 사람들이 있었으니까요.
        근데 {플레이어명}님은 정말 진심으로 농사를 하시는 거잖아요. 정말 기뻐요.
        앞으로 씨앗 살 때 제가 좀 깎아 드릴게요. 단골 할인이에요!"
```

기존 대사 풀에 다음이 추가된다.

| ID | 대사 |
|----|------|
| `hana_friend_01` | "할머니가 이 가게를 처음 열었을 때, 마을에 집이 세 채밖에 없었대요. 그때부터 씨앗을 팔았다고 하시더라고요." |
| `hana_friend_02` | "가끔 생각해요. 저도 농장을 해 볼까 하고요. 근데 가게를 비울 수가 없잖아요? 그래서 텃밭으로 만족해요." |
| `hana_friend_03` | "겨울이 오면 좀 외로워요. 손님도 뜸하고... 그래도 {플레이어명}님이 와 주니까 겨울도 괜찮아요!" |
| `hana_friend_04` | "어머니가 돌아가시기 전에 이 가게를 꼭 지키라고 하셨어요. 그래서 저 여기 있는 거예요. 이 마을이 좋기도 하고요." |

### 3.7 날씨별 특수 대사

특정 날씨 조건에서 추가 출력되는 대사.

| 날씨 | 대사 |
|------|------|
| 비 (Rain) | "비 오는 날이에요! 물 안 줘도 되니까 다른 일 하기 좋은 날이에요. 비 소리 좋죠?" |
| 비 (Rain) | "우산은 없지만... 비 맞으면서 가게까지 오셨어요? 감사해요!" |
| 폭풍 (Storm) | (임시 마감 — 영업하지 않음) |
| 폭설 (Blizzard) | (임시 마감 — 영업하지 않음) |
| 맑음 + 봄/여름 | "오늘 날씨 최고예요! 작물이 쑥쑥 클 거예요~" |

### 3.8 상점 인터페이스 UX 세부

기본 UI 흐름은 (-> see `docs/content/npcs.md` 섹션 8.2)을 따른다. 하나 상점 고유 요소만 기술한다.

#### 상점 화면 레이아웃

```
┌─────────────────────────────────────────────┐
│  [들꽃상점]                    [소지금: XXG] │
├─────────────────────────────────────────────┤
│  [씨앗] [비료] [기타]  │  [판매]            │
├────────────────────────┼────────────────────┤
│  ┌──────────────────┐  │  아이템 상세       │
│  │ 감자 씨앗   30G  │  │                   │
│  │ 당근 씨앗   25G  │  │  [감자 씨앗]      │
│  │ 딸기 씨앗   60G  │  │  "봄에 심는 기본  │
│  │  ...             │  │   작물. 성장       │
│  │                  │  │   4일."            │
│  │                  │  │                   │
│  │                  │  │  수량: [- 1 +]    │
│  │                  │  │  합계: 30G        │
│  │                  │  │  [구매]           │
│  └──────────────────┘  │                   │
├─────────────────────────────────────────────┤
│  [하나] "좋은 선택이에요!"                   │
└─────────────────────────────────────────────┘
```

#### 하나 고유 UX 요소

| 요소 | 설명 |
|------|------|
| 계절 표시 | 씨앗 탭에 현재 계절 아이콘 표시. 비계절 씨앗은 "다음 계절" 배지 (여행 상인에서만 구매 가능) |
| 추천 배지 | 하나가 추천하는 씨앗에 별 아이콘 표시 (친밀도 `Acquaintance` 이상 시 활성) |
| 할인 표시 | `Friend` 단계 시 씨앗 가격에 취소선 + 할인가 표시 |
| 판매가 미리보기 | 판매 탭에서 작물 위에 마우스 오버 시 품질별 판매가 툴팁 |
| 대화 패널 | 화면 하단에 하나의 짧은 코멘트가 실시간 표시 |

---

## 4. 계절별 이벤트 연동

### 4.1 계절 전환 특별 판매

각 계절 첫 날, 하나의 상점에 새 계절 씨앗이 입고된다. 계절 첫 방문 시 특별 대사가 출력된다.

| 계절 전환 | 대사 |
|-----------|------|
| 봄 시작 | "새 봄이에요! 감자, 당근, 딸기 씨앗 준비했어요. 올해도 화이팅!" |
| 여름 시작 | "여름 씨앗 대량 입고! 토마토, 옥수수, 해바라기... 뭘 먼저 심을래요?" |
| 가을 시작 | "가을이네요! 당근이랑 호박 씨앗 왔어요. 호박은 크게 키워 보세요!" |
| 겨울 시작 | "겨울이라 씨앗은 없어요... 대신 비료 할인해 드릴까요? 봄 준비를 미리 해요!" |

### 4.2 겨울 특별 대사 시리즈

겨울에는 하나의 개인적인 이야기가 더 많이 나온다 (장사가 한산하므로).

| 겨울 주차 | 조건 | 대사 |
|-----------|------|------|
| 겨울 1주차 | 방문 시 | "겨울 시작이에요. 올해도 수고 많으셨어요. 봄까지 좀 쉬어요." |
| 겨울 2주차 | 방문 시 | "가게가 한산하니까 정리 좀 했어요. 진열대가 깨끗해졌죠?" |
| 겨울 3주차 | `Regular` 이상 | "할머니가 겨울이면 항상 짠한 무우국을 끓여 주셨어요. 지금도 그 맛이 그리워요." |
| 겨울 4주차 | 방문 시 | "곧 봄이에요! 새 씨앗 입고 준비 중이에요. 기대되시죠?" |

---

## 5. 튜닝 파라미터

| 파라미터 | 영문 키 | 현재 값 | 조정 범위 | 영향 |
|----------|---------|---------|-----------|------|
| 친밀도 임계값 | `hanaAffinityThresholds` | [0, 10, 25, 50] | - | 관계 발전 속도 |
| 작물 판매 친밀도 증가 | `hanaSellAffinityGain` | 1 | 1~2 | 판매 빈도의 관계 기여도 |
| 씨앗 구매 친밀도 증가 | `hanaBuyAffinityGain` | 1 | 1~2 | 구매 빈도의 관계 기여도 |
| 일일 대화 친밀도 증가 | `hanaChatAffinityGain` | 1 | 0~1 | 방문만으로의 관계 기여도 |
| Friend 할인율 | `hanaFriendDiscount` | 0.10 | 0.05~0.15 | 씨앗 할인 강도 |
| 추천 배지 활성 단계 | `hanaRecommendBadgeStage` | `Acquaintance` | - | 추천 표시 시작 시점 |
| 계절 대사 풀 크기 | `hanaSeasonDialogueCount` | 3 | 2~5 | 대사 다양성 |

---

## Cross-references

| 문서 | 참조 내용 |
|------|-----------|
| `docs/content/npcs.md` 섹션 3 | 하나 기본 캐릭터 설정, 상점 인벤토리, 기본 대화 예시 |
| `docs/content/npcs.md` 섹션 7 | NPC 대화 시스템 공통 구조 (트리거, 우선순위, 반복 방지) |
| `docs/content/npcs.md` 섹션 8 | 상점 공통 UI 흐름 |
| `docs/systems/economy-system.md` 섹션 2 | 가격 시스템, 수급 보정 |
| `docs/systems/economy-system.md` 섹션 3 | 상점 종류, 영업시간, 인벤토리 규칙 |
| `docs/systems/economy-system.md` 섹션 4 | 대량 할인, 거래 메커니즘 |
| `docs/design.md` 섹션 4.2 | 작물 목록, 씨앗 가격, 판매가 |
| `docs/systems/farming-system.md` 섹션 5.1 | 비료 종류, 가격, 해금 |
| `docs/balance/progression-curve.md` 섹션 1.3.2 | 레벨별 해금 콘텐츠 |
| `docs/content/blacksmith-npc.md` | 철수 NPC 상세 (관계 발전 구조 동일 패턴) |
| `docs/systems/npc-shop-architecture.md` | NPC/상점 시스템 기술 아키텍처 |

---

## Open Questions

1. [OPEN] **Friend 할인 범위**: 현재 씨앗에만 10% 할인을 적용하도록 설계했으나, 비료까지 확장할지 검토 필요. 비료까지 할인하면 경제 밸런스에 영향이 크다.

2. [OPEN] **추천 배지 알고리즘**: 하나가 추천하는 씨앗의 기준이 불명확하다. 현재 계절 최적 수익 작물인지, 플레이어 레벨에 맞춘 난이도 기반인지, 단순 랜덤인지 결정 필요.

3. [OPEN] **겨울 비료 할인 이벤트**: 겨울 시작 대사에서 "비료 할인"을 언급하는데, 실제 할인 수치와 기간을 경제 시스템과 조율해야 한다. 현재 경제 시스템에 계절 할인 메카닉이 없으므로 신규 설계 필요.

4. [OPEN] **NPC 호감도 시스템 통합**: `docs/content/npcs.md` Open Question 2와 동일. 전체 NPC 호감도 시스템이 도입될 경우, 하나의 개별 친밀도 시스템을 통합하거나 대체할 수 있다.

---

# NPC/상점 콘텐츠 상세 문서 (NPC & Shop Content Specification)

> 작성: Claude Code (Opus) | 2026-04-06  
> 문서 ID: CON-003 | Phase 1

---

## 1. Context

이 문서는 SeedMind에 등장하는 모든 NPC의 캐릭터 설정, 대화 시스템, 상점 인벤토리, 가격 정책, 재고 규칙을 상세히 기술한다. 경제 시스템(`docs/systems/economy-system.md`) 섹션 3에 정의된 상점 종류와 영업시간을 기반으로, NPC별 캐릭터성과 상호작용 디테일을 확장한다.

**설계 목표**: NPC는 단순한 상점 인터페이스가 아니라, 마을에 생동감을 부여하는 존재다. 각 NPC는 고유한 성격, 대사 패턴, 계절별 반응을 갖추어 플레이어가 "사람과 교류하는 느낌"을 받도록 한다. 동시에 게임 메카닉 측면에서 NPC 간 역할 분담이 명확하고, 상점 방문 타이밍이 전략적 의미를 갖도록 설계한다.

### 1.1 본 문서가 canonical인 데이터

- NPC 이름, 성격, 외형 묘사
- NPC별 대화 스크립트 (인사말, 계절별 대사, 진행도별 대사)
- 대화 시스템 구조 (트리거, 우선순위, 반복 방지)
- 여행 상인 등장 조건, 빈도, 판매 아이템 풀
- 상점 공통 UI 흐름 규칙

### 1.2 본 문서가 canonical이 아닌 데이터 (참조만)

| 데이터 종류 | 참조처 |
|------------|--------|
| 상점 종류, 영업시간, 휴무일 | `docs/systems/economy-system.md` 섹션 3.1~3.2 |
| 씨앗 가격, 작물 판매가, 성장일수 | `docs/design.md` 섹션 4.2 |
| 비료 종류, 가격, 해금 | `docs/systems/farming-system.md` 섹션 5.1 |
| 도구 업그레이드 비용, 재료, 레벨 요건 | `docs/systems/tool-upgrade.md` 섹션 2 |
| 시설 비용, 해금 레벨 | `docs/design.md` 섹션 4.6 |
| 레벨별 해금 콘텐츠 | `docs/balance/progression-curve.md` 섹션 1.3.2 |
| 대량 구매 할인율 | `docs/systems/economy-system.md` 섹션 4.3 |
| 잡화 상점 인벤토리 구성 | `docs/systems/economy-system.md` 섹션 3.3 |
| 대장간 상점 판매 품목 | `docs/systems/tool-upgrade.md` 섹션 6.3 |
| 목공소 인벤토리 | `docs/systems/economy-system.md` 섹션 3.3 |

---

## 2. NPC 일람

### 2.1 NPC 목록 요약

| NPC | 영문 ID | 역할 | 위치 | 등장 조건 |
|-----|---------|------|------|-----------|
| 하나 (Hana) | `npc_hana` | 시장 상인 (잡화 상점) | 마을 중앙 | 게임 시작 시 |
| 철수 (Cheolsu) | `npc_cheolsu` | 대장간 장인 | 마을 외곽 | 게임 시작 시 |
| 목이 (Moki) | `npc_moki` | 목공소 장인 | 마을 북쪽 | 게임 시작 시 |
| 바람이 (Barami) | `npc_barami` | 여행 상인 | 마을 광장 (임시) | 특수 조건 (섹션 5 참조) |

### 2.2 NPC 배치 맵 개요

```
              [목공소 - 목이]
                    │
     ┌──────────────┼──────────────┐
     │              │              │
 [대장간]      [마을 광장]     [잡화 상점]
 [철수]       [바람이 임시]      [하나]
     │              │              │
     └──────────────┼──────────────┘
                    │
              [농장 입구]
```

---

## 3. 시장 상인 — 하나 (Hana, Market Vendor)

### 3.1 캐릭터 설정

| 항목 | 내용 |
|------|------|
| 이름 | 하나 (Hana) |
| 나이 | 30대 초반 |
| 성격 | 밝고 활기찬 상인. 작물과 농사에 해박하며 손님을 반갑게 맞는다. 가끔 씨앗에 대한 소소한 팁을 흘린다. |
| 외형 | 밀짚모자, 초록색 앞치마, 짧은 갈색 머리. 상점 카운터 뒤에서 항상 웃는 얼굴. |
| 배경 | 마을에서 대대로 이어온 잡화 상점을 운영하는 집안. 작물 품종에 대한 풍부한 지식을 보유. |

### 3.2 영업 정보

영업시간, 휴무일: (-> see `docs/systems/economy-system.md` 섹션 3.2)

| 항목 | 내용 |
|------|------|
| 폭풍/폭설 시 | 임시 마감 |

### 3.3 상점 인벤토리

상점 인벤토리 구성 규칙은 `docs/systems/economy-system.md` 섹션 3.3을 따른다.

**항상 판매 품목**:
- 기본 비료 (Basic Fertilizer) — 가격: (-> see `docs/systems/farming-system.md` 섹션 5.1)
- 장작 (Firewood, `item_firewood`) — 구매가: 30G (베이커리 연료, -> see `docs/content/processing-system.md` 섹션 4.2)

**계절별 씨앗 판매** (해당 계절에만 판매):

| 계절 | 판매 씨앗 | 해금 조건 |
|------|----------|-----------|
| 봄 | 감자, 당근, 딸기 씨앗 | 딸기는 (-> see `docs/balance/progression-curve.md` 섹션 1.3.2) 레벨 해금 후 |
| 여름 | 토마토, 옥수수, 해바라기, 수박 씨앗 | 각 작물 해금 레벨 충족 후 |
| 가을 | 당근, 호박 씨앗 | 호박은 해금 레벨 충족 후 |
| 겨울 (Day 1~7) | 씨앗 판매 없음 | 온실 미보유 시 이전 계절 씨앗 잔여분 활용 |
| 겨울 (Day 8~28) | 겨울무, 표고버섯, 시금치 씨앗 | 온실 보유 + 각 작물 해금 레벨 충족 시. 정가 판매 (→ see `docs/content/crops.md` 섹션 6) |

씨앗 가격: (-> see `docs/design.md` 섹션 4.2)

**레벨 해금 추가 판매 아이템**:

| 해금 레벨 | 아이템 | 가격 |
|-----------|--------|------|
| 레벨 3 | 고급 비료 (Quality Fertilizer) | (-> see `docs/systems/farming-system.md` 섹션 5.1) |
| 레벨 4 | 속성장 비료 (Speed-Gro) | (-> see `docs/systems/farming-system.md` 섹션 5.1) |
| 레벨 6 | 유기 비료 (Organic Fertilizer) | (-> see `docs/systems/farming-system.md` 섹션 5.1) |

**재고 규칙**:

| 규칙 | 내용 |
|------|------|
| 씨앗 재고 | 무제한 (계절 내 해당 씨앗은 항상 구매 가능) |
| 비료 재고 | 무제한 |
| 재고 리셋 | 계절 전환 시 (새 계절 씨앗으로 교체) |

**겨울 특수 규칙**: 겨울 1주차(Day 1~7)에는 씨앗을 판매하지 않는다. 겨울 2주차(Day 8)부터 온실 보유 플레이어에 한해 겨울 전용 씨앗(겨울무/표고버섯/시금치)을 정가로 판매한다. 비료와 소모품은 겨울 전 기간 정상 판매. (→ see `docs/systems/economy-system.md` 섹션 3.3)

**겨울 씨앗 판매 설계 근거 (DES-014 확정)**:
- 여행 상인은 겨울 1주차(Day 1~2 주말)부터 x1.5 프리미엄으로 판매 가능 (→ see 섹션 6.3)
- 잡화 상점은 겨울 2주차(Day 8)부터 정가로 판매 시작
- 여행 상인의 "1주 선점" 가치를 보존하면서, 겨울 콘텐츠 접근성을 보장하는 혼합 모델
- 온실 미보유 플레이어에게는 씨앗이 표시되지 않음 (불필요한 정보 노출 방지)

### 3.4 대화 스크립트

전체 대사 스크립트, 친밀도 단계별 대화, 작물 추천 힌트 대사, UX 상세는 (-> see `docs/content/merchant-npc.md` 섹션 3~4).

#### 인사말 (첫 대화)

```
[하나] "어서 와요! 새로 농장을 시작한 분이군요.
        씨앗이 필요하면 언제든 들러요. 감자부터 시작하는 게 좋을 거예요!"
```

#### 일반 인사 (재방문)

```
[하나] "안녕하세요! 오늘은 뭘 찾으세요?"
[하나] "좋은 아침이에요! 오늘 날씨가 농사하기 딱 좋네요."
[하나] "또 오셨군요! 뭐가 필요하신지 구경해 보세요."
```

#### 계절별 대사

| 계절 | 대사 |
|------|------|
| 봄 | "봄이에요! 감자랑 당근 씨앗이 잘 나가요. 올해 첫 수확이 기대되네요." |
| 여름 | "여름에는 토마토가 정말 잘 자라요. 옥수수도 도전해 보세요, 시간은 좀 걸리지만 값어치를 해요." |
| 가을 | "가을 호박은 정말 대단해요. 크게 키우면 대박이 날 수도 있다는 얘기 들어 봤어요?" |
| 겨울 (Day 1~7) | "겨울이라 씨앗은 아직 없지만, 다음 주부터 온실용 씨앗을 들여놓을 거예요!" |
| 겨울 (Day 8~28, 온실 보유) | "온실용 겨울 씨앗이 들어왔어요! 겨울무, 표고버섯, 시금치... 골라 보세요!" |
| 겨울 (Day 8~28, 온실 미보유) | "겨울에 할 일이 없죠? 온실을 지으면 겨울에도 농사할 수 있어요. 목이한테 가 보세요." |

#### 진행도별 특수 대사

| 조건 | 대사 |
|------|------|
| 레벨 3 도달 | "벌써 이만큼 성장하셨군요! 고급 비료도 들여놨으니 한번 써 보세요." |
| 레벨 5 도달 | "호박이랑 해바라기 씨앗도 준비됐어요. 이제 진짜 농부시네요!" |
| 레벨 7 도달 | "수박 씨앗, 드디어 입고됐어요! 비싸지만 그만한 가치가 있을 거예요." |
| 첫 번째 작물 수확 후 | "첫 수확 축하드려요! 출하함에 넣으면 다음 날 정산돼요." |
| 겨울 & 온실 미보유 | "온실을 지으면 겨울에도 농사할 수 있어요. 목이한테 가 보세요." |
| 겨울 Day 8 & 온실 보유 & 씨앗 미구매 | "겨울 씨앗 들어왔는데 아직 안 사셨네요? 겨울무가 초보에게 딱이에요!" |

### 3.5 작물 판매 기능

잡화 상점에서 작물을 직접 판매할 수 있다.

| 항목 | 내용 |
|------|------|
| 판매 가격 | 최종 판매가 공식 적용 (-> see `docs/systems/economy-system.md` 섹션 2.6.5) |
| 정산 | 즉시 (-> see `docs/systems/economy-system.md` 섹션 4.1) |
| 수급 영향 | 상점 판매도 수급 카운터 누적 |

---

## 4. 대장간 장인 — 철수 (Cheolsu, Blacksmith)

### 4.1 캐릭터 설정

| 항목 | 내용 |
|------|------|
| 이름 | 철수 (Cheolsu) |
| 나이 | 40대 후반 |
| 성격 | 과묵하고 진지한 장인 기질. 말수가 적지만 도구에 대한 자부심이 강하다. 가끔 도구 관리 팁을 짧게 알려준다. |
| 외형 | 그을린 피부, 가죽 앞치마, 굵은 팔. 대장간 모루 옆에 서 있다. 머리에 두건을 두르고 있다. |
| 배경 | 마을의 유일한 대장장이. 3대째 이어온 대장간을 운영하며, 좋은 도구가 좋은 농사의 시작이라고 믿는다. |

### 4.2 영업 정보

영업시간, 휴무일: (-> see `docs/systems/economy-system.md` 섹션 3.2)

| 항목 | 내용 |
|------|------|
| 폭풍/폭설 시 | 정상 영업 (대장간은 실내 작업이므로 날씨 영향 없음) |

**설계 의도**: 대장간은 영업시간이 짧고, 금요일 휴무이므로 방문 타이밍 계획이 필요하다. 특히 도구 업그레이드 수령 시 영업시간 내 방문해야 하므로, 도구를 맡기는 날과 수령하는 날의 일정을 미리 계획하게 만든다.

### 4.3 상점 인벤토리

대장간은 두 가지 서비스를 제공한다: **도구 업그레이드**와 **재료 구매**.

#### 도구 업그레이드

도구 업그레이드의 비용, 재료, 레벨 요건, 소요 시간, 절차는 모두 `docs/systems/tool-upgrade.md` 섹션 2를 따른다.

**업그레이드 대상 도구**: 호미 (Hoe), 물뿌리개 (Watering Can), 낫 (Sickle), 채집 낫 (Gathering Sickle) — 4종 (-> see `docs/systems/tool-upgrade.md` 섹션 3, `docs/systems/gathering-system.md` 섹션 5.2)

**업그레이드 등급**: Basic -> Reinforced -> Legendary — 3단계 (-> see `docs/systems/tool-upgrade.md` 섹션 1.1)

**상호작용 흐름**: (-> see `docs/systems/tool-upgrade.md` 섹션 6.2)

#### 재료 구매

재료 가격 및 재고: (-> see `docs/systems/tool-upgrade.md` 섹션 6.3)

| 아이템 | 가격 | 재고 |
|--------|------|------|
| 철 조각 (Iron Scrap) | (-> see `docs/systems/tool-upgrade.md` 섹션 6.3) | 무제한 |
| 정제 강철 (Refined Steel) | (-> see `docs/systems/tool-upgrade.md` 섹션 6.3) | 무제한 |

[OPEN] 대장간에서 도구 외 추가 판매 아이템(스프링클러 부품, 울타리 등) 필요 여부. 현재는 도구 업그레이드와 재료 판매로 한정. 후반 골드 싱크가 필요할 경우 추가 아이템 도입 검토.

### 4.4 대화 예시

#### 인사말 (첫 대화)

전체 대사 스크립트는 (→ see `docs/content/blacksmith-npc.md` 섹션 3.1~3.7).

```
[철수] "...응. 새 농부구나.
        도구가 보이는데, 기본 도구로군.
        나중에 좀 더 쓸 만한 게 필요하면 재료를 가져와. 만들어 줄 테니."
```

#### 일반 인사 (재방문)

```
[철수] "왔나. 볼일이 있으면 말해."
[철수] "도구 상태는 어때? 무리하게 쓰지 마."
[철수] "...필요한 게 있으면."
```

#### 도구 업그레이드 관련 대사

| 상황 | 대사 |
|------|------|
| 업그레이드 의뢰 시 | "좋아, 맡겨. {소요시간}일이면 될 거야. 그동안 이 도구 없이 버텨야 해." |
| 업그레이드 완료 수령 시 | "다 됐어. 전보다 훨씬 나을 거야. 잘 써." |
| 레벨 미달 시 | "...아직 이르군. 좀 더 경험을 쌓고 와." |
| 재료 부족 시 | "재료가 모자라. {재료이름}이(가) 더 필요해." |
| 골드 부족 시 | "돈이 모자라는데. 더 모아서 와." |
| 도구가 이미 맡겨져 있을 때 | "이미 작업 중인 도구가 있어. 내일 와." |
| 도구가 완성되었을 때 (미수령) | "도구 다 됐어. 가져가." |

#### 채집 낫 관련 대사

(-> see `docs/systems/gathering-system.md` 섹션 7.3 — 확정 대화 동기화)

| 상황 | 대사 | 트리거 조건 |
|------|------|------------|
| Zone D 해금 직후 첫 방문 | "숲에 갔다고? 채집 낫이 있으면 좋은 것도 더 잘 찾을 수 있어. 하나 만들어 줄까?" | Zone D 해금 && 채집 낫 미보유 |
| 기본 채집 낫 구매 시 | "200골드면 돼. 날이 잘 서 있으니까 풀도 버섯도 깔끔하게 베어질 거야." | 구매 확인 |
| 기본 채집 낫 구매 완료 | "잘 써. 숲에서 뭔가 좋은 걸 찾으면 나한테도 보여줘." | 구매 직후 |
| 강화 업그레이드 조건 미달 시 | "이 낫을 더 좋게 만들려면 채집 경험이 좀 더 필요해. 숲을 더 돌아다녀 봐." | 채집 숙련도 < Lv.3 |
| 강화 업그레이드 요청 시 | "구리 광석 3개가 필요하고, 1일이면 완성이야. 그동안 맨손으로도 채집은 할 수 있으니까 걱정 마." | 조건 충족 + 의뢰 확인 |
| 강화 채집 낫 수령 시 | "다 됐어. 날에 구리를 입혀서 더 단단해졌지. 금별 품질도 나올 거야." | 완성 후 수령 |
| 전설 업그레이드 조건 미달 시 | "전설 등급이라... 숲의 모든 것을 알아야 만들 수 있어. 아직은 때가 아니야." | 채집 숙련도 < Lv.7 |
| 전설 업그레이드 요청 시 | "금 광석 2개에 수정 원석 1개... 쉬운 재료는 아니지만, 그만큼 대단한 물건이 될 거야. 2일 줘." | 조건 충족 + 의뢰 확인 |
| 전설 채집 낫 수령 시 | "... 내 최고 작품 중 하나야. 이 낫이면 숲의 무엇이든 최상의 상태로 거둘 수 있어. 잘 써줘." | 완성 후 수령 |

#### 계절별 대사

| 계절 | 대사 |
|------|------|
| 봄 | "봄이 오면 호미질할 일이 많겠지. 호미 상태 한번 확인해 봐." |
| 여름 | "여름엔 작물이 많아지니까 낫을 올릴 때가 되지 않았나." |
| 가을 | "수확 시즌이니까 도구 점검은 미리 해 두는 게 좋아." |
| 겨울 | "겨울이라 한가하긴 한데... 도구 정비할 시간이긴 하지." |

#### 진행도별 특수 대사

| 조건 | 대사 |
|------|------|
| 첫 번째 업그레이드 완료 | "첫 번째 강화 도구야. 차이를 느낄 거야." |
| 모든 도구 Reinforced 달성 | "전부 강화했군. 꽤 쓸 줄 아는 농부네." |
| 모든 도구 Legendary 달성 | "...전설급 도구를 전부 갖추다니. 대단한 농부야." |
| 비 오는 날 방문 | "비 오는 날에 왔군. 물뿌리개 맡기려고? 현명해." |

### 4.5 도구 업그레이드 전략 힌트

철수의 대사에는 도구 업그레이드 타이밍에 대한 간접적 힌트가 포함된다.

힌트 대사의 발동 조건 및 친밀도 요건은 (→ see `docs/content/blacksmith-npc.md` 섹션 3.8). 요약:

| 힌트 대사 | 조건 | 친밀도 조건 |
|-----------|------|------------|
| "비 오는 날엔 물뿌리개가 필요 없잖아. 그때 맡기면 똑똑한 거야." | 비 오는 날 + 물뿌리개 미업그레이드 | 단골(`Regular`) 이상 |
| "수확 끝나고 심기 전에 호미를 맡기면 좋을 거야." | 수확 시즌 + 호미 미업그레이드 | 단골(`Regular`) 이상 |
| "낫은 작물이 다 자라기 전에 맡겨야 손해 안 봐." | 작물 성장 초기 + 낫 미업그레이드 | 단골(`Regular`) 이상 |

---

## 5. 목공소 장인 — 목이 (Moki, Carpenter)

### 5.1 캐릭터 설정

| 항목 | 내용 |
|------|------|
| 이름 | 목이 (Moki) |
| 나이 | 50대 초반 |
| 성격 | 느긋하고 온화한 성격. 건축에 대한 열정이 넘치며, 시설 건설에 대해 자세하게 설명해 준다. 말이 길지만 친절하다. |
| 외형 | 넓은 어깨, 나무 톱밥이 묻은 작업복, 연장 벨트를 차고 있다. 짧은 회색 수염. |
| 배경 | 마을의 목수이자 건축가. 마을의 모든 건물을 지은 사람. 새 농부의 농장 발전을 진심으로 기대하고 있다. |

### 5.2 영업 정보

영업시간, 휴무일: (-> see `docs/systems/economy-system.md` 섹션 3.2)

| 항목 | 내용 |
|------|------|
| 폭풍/폭설 시 | 임시 마감 |

### 5.3 상점 인벤토리

목공소에서 제공하는 서비스: **농장 확장**, **시설 건설**, **가공소 슬롯 확장**.

모든 건설 비용, 해금 조건, 건설 시간은 canonical 문서를 참조한다.

| 서비스 종류 | 참조 문서 |
|------------|-----------|
| 농장 확장 (4단계) | (-> see `docs/systems/economy-system.md` 섹션 3.3, 목공소 인벤토리) |
| 시설 건설 (물탱크, 창고, 온실, 가공소) | (-> see `docs/design.md` 섹션 4.6, `docs/content/facilities.md` 섹션 2) |
| 가공소 슬롯 확장 | (-> see `docs/systems/economy-system.md` 섹션 2.5) |

**건설 절차**: (-> see `docs/content/facilities.md` 섹션 2.2)

### 5.4 대화 스크립트

전체 대사 스크립트, 친밀도 단계별 대화, 시설 건설 힌트 대사, UX 상세는 (-> see `docs/content/carpenter-npc.md` 섹션 3~4).

#### 인사말 (첫 대화)

```
[목이] "오! 새 농부님이시구먼!
        이 마을의 건물은 전부 내가 지었지. 허허.
        농장에 시설이 필요하면 언제든 찾아오시게.
        처음엔 물탱크부터 시작하는 게 좋을 거야."
```

#### 일반 인사 (재방문)

```
[목이] "어서 오시게! 오늘은 뭘 지어 볼까?"
[목이] "농장이 점점 커지고 있군! 좋아, 좋아."
[목이] "건축 상담이면 내가 전문이지. 편하게 물어보시게."
```

#### 시설 건설 관련 대사

| 상황 | 대사 |
|------|------|
| 건설 의뢰 시 | "좋아! {시설이름} 건설 시작하겠네. {건설시간}일 뒤에 완성될 거야." |
| 건설 완료 알림 | "다 지었네! {시설이름}이(가) 완성됐어. 가서 확인해 보시게." |
| 해금 레벨 미달 | "아직 이 시설을 짓기엔 이르네. 좀 더 경험을 쌓으면 내가 도와주지." |
| 골드 부족 | "음... 비용이 좀 모자라는군. {필요골드}G가 더 있어야 해." |
| 이미 건설 중 | "지금 다른 건물을 짓고 있어서, 끝나고 다시 오시게." |

#### 계절별 대사

| 계절 | 대사 |
|------|------|
| 봄 | "봄이야! 농장 확장하기 딱 좋은 계절이지. 물탱크 어때?" |
| 여름 | "여름에 창고를 지어 두면 가을 수확 때 든든하지." |
| 가을 | "가을 수확이 끝나면 온실을 생각해 봐. 겨울이 코앞이야." |
| 겨울 | "겨울이라 야외 공사가 힘들지만... 실내 시설은 문제없어!" |

#### 진행도별 특수 대사

| 조건 | 대사 |
|------|------|
| 첫 시설 건설 완료 | "첫 번째 건물이야! 농장이 점점 그럴듯해지는군." |
| 모든 시설 건설 완료 | "허허, 시설을 전부 지었군! 이 정도면 마을 최고의 농장이야." |
| 온실 미보유 + 겨울 진입 | "겨울에 할 일이 없지? 온실을 지으면 겨울에도 작물을 키울 수 있어." |
| 가공소 미보유 + 레벨 7 도달 | "이제 가공소를 지을 때가 됐어! 작물을 가공하면 수익이 크게 올라간다네." |

---

## 6. 여행 상인 — 바람이 (Barami, Traveling Merchant)

### 6.1 캐릭터 설정

| 항목 | 내용 |
|------|------|
| 이름 | 바람이 (Barami) |
| 나이 | 불명 (20대~40대로 보임) |
| 성격 | 신비롭고 자유분방한 방랑 상인. 유쾌하면서도 어딘가 수상한 매력. 희귀한 물건을 갖고 다니며, 가격은 비싸지만 독점 아이템을 판매한다. |
| 외형 | 색색의 천으로 장식된 망토, 큰 배낭, 반짝이는 귀걸이. 나귀 한 마리와 함께 다닌다. |
| 배경 | 여러 마을을 떠돌며 희귀 물건을 거래하는 방랑 상인. 어디서 물건을 구하는지는 아무도 모른다. |

### 6.2 등장 조건 및 빈도

| 항목 | 내용 |
|------|------|
| 등장 장소 | 마을 광장 (임시 좌판 설치) |
| 등장 요일 | 매주 토요일, 일요일 (Day 6~7, 13~14, 20~21, 27~28) |
| 등장 시간 | 09:00~17:00 (등장일에만) |
| 등장 확률 | 100% (고정 등장) |
| 플레이어 레벨 요건 | 레벨 2 이상 (레벨 1에서는 등장하지 않음) |
| 겨울 등장 | 정상 등장 (겨울에도 방문한다) |

**설계 의도**: 주말마다 정기적으로 등장하여 플레이어에게 "주말 방문"의 루틴을 형성한다. 레벨 2부터 등장하여, 게임에 익숙해진 시점에 새로운 쇼핑 옵션을 제공한다. 겨울에도 등장하여 겨울 콘텐츠 공백을 일부 채운다.

### 6.3 판매 아이템 풀

여행 상인은 매 등장 시 아래 풀에서 무작위로 4~6개 아이템을 선정하여 판매한다. 재고는 각 아이템당 1~3개로 제한된다.

#### 아이템 풀 (Item Pool)

| 카테고리 | 아이템 | 영문 ID | 가격 | 등장 확률 | 비고 |
|----------|--------|---------|------|-----------|------|
| 비계절 씨앗 | 다음 계절 씨앗 1종 (무작위) | `seed_*` | 정가 x1.5 | 30% | 미리 다음 계절 작물 준비 가능 (→ see docs/balance/traveler-economy.md) |
| 희귀 비료 | 만능 비료 (Universal Fertilizer) | `item_universal_fert` | 80G | 20% | 품질+성장 복합 효과 (섹션 6.4 참조, → see docs/balance/traveler-economy.md) |
| 장식품 | 정원 등불 (Garden Lantern) | `item_garden_lantern` | 300G | 15% | 농장 장식 아이템, 야간 조명 |
| 장식품 | 풍향계 (Weathervane) | `item_weathervane` | 500G | 10% | 농장 장식, 다음 날 날씨 예보 기능 |
| 특수 아이템 | 에너지 토닉 (Energy Tonic) | `item_energy_tonic` | 200G | 25% | 에너지 50 즉시 회복 (1일 1회 제한) |
| 특수 아이템 | 성장 촉진제 (Growth Accelerator) | `item_growth_accel` | 150G | 15% | 대상 작물 성장 2일 단축 (1회 사용, → see docs/balance/traveler-economy.md) |
| 특수 아이템 | 행운의 부적 (Lucky Charm) | `item_lucky_charm` | 250G | 10% | 하루 동안 Iridium 품질 확률 +15% (→ see docs/balance/traveler-economy.md) |
| 겨울 전용 | 온실 전용 씨앗 (겨울무/표고버섯/시금치 중 1종) | `seed_winter_*` | 정가 x1.5 | 20% (겨울만) | 겨울 온실 콘텐츠 진입 경로 |
| 광석 | 구리 광석 x3 세트 (Copper Ore Set) | `gather_copper_ore` | 100G | 20% | 강화 낚싯대/강화 채집 낫 재료 대안 공급 경로 (→ see `docs/systems/gathering-system.md` 섹션 7.1). 개당 ~33G, 채집 판매가(10G) 3.3배 프리미엄. 재고 1세트(3개) |
| 광석 | 금 광석 x1 (Gold Ore) | `gather_gold_ore` | 120G | 10% | 전설 낚싯대/전설 채집 낫 재료 대안 공급 경로 (→ see `docs/systems/gathering-system.md` 섹션 7.1). 채집 판매가(24G) 5배 프리미엄. 재고 1개 |
| 광석 | 수정 원석 (Crystal Gem) | `gather_crystal_gem` | 160G | 10% | 전설 채집 낫 재료 대안 공급 경로 (→ see `docs/systems/gathering-system.md` 섹션 5.5.2). 직판가 32G의 5배 프리미엄. 재고 1개 |

**아이템 선정 규칙**:
1. 매 등장 시 아이템 풀에서 등장 확률에 따라 4~6개를 무작위 선정
2. 같은 카테고리에서 최대 2개까지만 선정
3. 겨울 전용 아이템은 겨울 계절에만 풀에 포함
4. 한 번 등장한 아이템은 다음 등장(다음 주말)에서 제외 (2주 쿨다운)
5. 재고: 아이템당 1~3개 (무작위). 매진 시 해당 아이템 구매 불가

### 6.4 만능 비료 (Universal Fertilizer) 상세

| 항목 | 내용 |
|------|------|
| 영문 ID | `item_universal_fert` |
| 가격 | 80G (→ see docs/balance/traveler-economy.md) |
| 효과 | 품질 보정 +15% (고급 비료 수준) + 성장 속도 +10% (속성장 비료의 절반 수준) |
| 사용법 | 타일에 적용 (일반 비료와 동일) |
| 중첩 | 다른 비료와 중첩 불가. 기존 비료를 대체함 |
| 획득 경로 | 여행 상인 전용 (상시 판매 상점에서는 구매 불가) |

**설계 의도**: 만능 비료는 고급 비료와 속성장 비료의 복합 효과를 제공하지만, 각각의 전문 비료보다 효과가 약하다. "한 칸에 두 가지 효과"의 편의성에 프리미엄 가격을 지불하는 구조. 여행 상인 전용이므로 대량 확보가 어렵고, 전략적으로 소량만 사용하게 된다.

### 6.5 대화 스크립트

전체 대사 스크립트, 친밀도 단계별 대화, 여행 일지 대사, 아이템 추천 대사는 (-> see `docs/content/traveler-npc.md` 섹션 3~4).

#### 인사말 (첫 등장)

전체 대사(방문 주기 안내 포함 4문장)는 (-> see `docs/content/traveler-npc.md` 섹션 3.1). 아래는 요약본.

```
[바람이] "헤이! 이 마을에 새 농부가 있다는 소문 들었지~
          나는 바람이. 여기저기 떠돌면서 신기한 물건을 모으는 상인이야.
          비싼 편이지만, 다른 데선 못 구하는 물건이 있을 거야!
          매주 토요일, 일요일에 올 테니까 기다려~"
```

#### 일반 인사 (재등장)

```
[바람이] "또 만났네! 이번 주에도 좋은 물건 가져왔어~"
[바람이] "아하, 기다리고 있었어? 구경해 봐!"
[바람이] "이번엔 특별한 게 있어. 한번 볼래?"
```

#### 구매/비구매 대사

| 상황 | 대사 |
|------|------|
| 아이템 구매 시 | "좋은 선택이야! 후회 안 할 거야~" |
| 아무것도 안 사고 나갈 때 | "괜찮아, 다음에 또 올 테니까. 기다릴게~" |
| 매진된 아이템 선택 시 | "아, 그건 이미 다 팔렸어. 다음에 또 가져올게!" |
| 골드 부족 시 | "음... 좀 부족하네. 다음에 여유 있을 때 와!" |

#### 계절별 대사

| 계절 | 대사 |
|------|------|
| 봄 | "봄바람 타고 왔어! 이번에 진짜 좋은 거 가져왔다~" |
| 여름 | "여름엔 여행하기 좋지! 남쪽 마을에서 귀한 물건 건져 왔어." |
| 가을 | "가을 축제 시즌이네! 이맘때 장사가 잘 돼서 기분 좋아~" |
| 겨울 | "추운 겨울에도 왔지! 나 아니면 이 물건 어디서 구해?" |

### 6.6 여행 상인 퇴장 시 대사

```
[바람이] "오늘은 여기까지! 바람이 부는 대로 다음 마을로 갈게.
          다음 주말에 또 들를 테니 기대해~"
```

(17:00이 되면 좌판을 접고 마을 밖으로 걸어 나가는 연출)

---

## 7. NPC 대화 시스템

### 7.1 대화 트리거

NPC에게 말을 거는 방법:

| 트리거 | 설명 |
|--------|------|
| E키 (상호작용) | NPC 근접 시 E키로 대화 시작 |
| 대화 범위 | NPC 중심 반경 2타일 이내 |
| 상점 전환 | 대화 중 "물건 보기" 선택 시 상점 UI로 전환 |
| 대화 종료 | "나가기" 선택 또는 Esc 키 |

### 7.2 대화 선택지 구조

```
[NPC 인사말]
    │
    ├── "물건 보기" → 상점 UI 열기
    ├── "이야기하기" → 대화 대사 재생
    └── "나가기" → 대화 종료
```

대장간의 경우:

```
[철수 인사말]
    │
    ├── "도구 업그레이드" → 업그레이드 UI
    ├── "재료 구매" → 상점 UI
    ├── "도구 수령" → (완성된 도구가 있을 때만 표시)
    ├── "이야기하기" → 대화 대사 재생
    └── "나가기" → 대화 종료
```

### 7.3 대사 선택 우선순위

NPC가 같은 시점에 여러 대사 조건을 충족할 경우, 아래 우선순위에 따라 대사를 선택한다.

| 우선순위 | 카테고리 | 예시 |
|----------|----------|------|
| 1 (최우선) | 일회성 마일스톤 | 첫 수확 축하, 첫 업그레이드 완료 |
| 2 | 진행도 해금 | 레벨 3 도달 시 신규 아이템 안내 |
| 3 | 계절 전환 | 새 계절 시작 첫 방문 대사 |
| 4 | 상황 힌트 | 비 오는 날 도구 맡기기 제안, 겨울 온실 안내 |
| 5 (최하) | 일반 인사 | 랜덤 인사말 풀에서 선택 |

### 7.4 대사 반복 방지

| 규칙 | 내용 |
|------|------|
| 일회성 대사 | 한 번 재생된 후 다시 나오지 않음 (마일스톤, 진행도 해금) |
| 계절 대사 | 해당 계절 첫 방문 시 1회만 재생, 이후 일반 인사 |
| 일반 인사 | 3~4개 풀에서 순환 (같은 대사가 연속으로 나오지 않음) |
| 상황 힌트 | 같은 힌트는 3일에 1번까지만 반복 |

### 7.5 대사 데이터 구조

대사는 JSON 데이터 파일로 관리한다 (Phase 2 구현 시).

```
// 설계 참고용 구조 예시 (실제 구현은 Phase 2)
DialogueEntry {
    id: string,              // "hana_greet_spring_01"
    npcId: string,           // "npc_hana"
    category: string,        // "seasonal" | "milestone" | "hint" | "general"
    priority: int,           // 1~5 (낮을수록 우선)
    condition: {
        season: string?,     // "spring" | "summer" | "fall" | "winter"
        minLevel: int?,      // 최소 플레이어 레벨
        maxLevel: int?,      // 최대 플레이어 레벨 (구간 제한)
        flag: string?,       // "firstHarvest" | "allToolsLegendary" 등
        weather: string?,    // "Rain" | "Storm" 등
        dayOfWeek: string?,  // "Monday"~"Sunday"
    },
    text: string,            // 대사 본문
    oneTime: bool,           // true이면 1회만 재생
    cooldownDays: int,       // 재생 후 쿨다운 일수 (0이면 제한 없음)
}
```

---

## 8. 상점 공통 규칙

### 8.1 가격 표시

| 규칙 | 내용 |
|------|------|
| 구매 가격 | 아이템 옆에 `{가격}G` 표시 |
| 판매 가격 | 인벤토리 아이템 위에 마우스 오버 시 "판매가: {가격}G" 툴팁 |
| 대량 할인 | 수량 10개 이상 시 할인 가격 표시 (-> see `docs/systems/economy-system.md` 섹션 4.3) |
| 미해금 아이템 | 회색 표시 + "레벨 X 해금" 텍스트 (-> see `docs/systems/economy-system.md` 섹션 3.4) |
| 골드 부족 | 가격이 빨간색, 구매 불가 |

### 8.2 구매/판매 UI 흐름

#### 구매 흐름

```
[상점 UI 열기]
    │
    ├── 아이템 목록 (카테고리별 탭: 씨앗 / 비료 / 기타)
    │       │
    │       ├── 아이템 선택 → 상세 정보 패널 (설명, 효과, 가격)
    │       │
    │       ├── 수량 조절 (+/-, 마우스 휠, 1/5/10/전체 단위)
    │       │
    │       └── [구매] 버튼 → 확인 → 골드 차감, 인벤토리 추가
    │
    └── [닫기] → 대화 선택지로 복귀
```

#### 판매 흐름 (잡화 상점에서)

```
[판매 탭 전환]
    │
    ├── 플레이어 인벤토리 표시 (판매 가능 아이템만)
    │       │
    │       ├── 아이템 선택 → 판매가 표시 (품질별 가격 반영)
    │       │
    │       ├── 수량 조절
    │       │
    │       └── [판매] 버튼 → 확인 → 아이템 제거, 골드 획득 (즉시)
    │
    └── [닫기]
```

### 8.3 재고 한도 및 리셋

| 상점 | 재고 유형 | 리셋 주기 |
|------|----------|-----------|
| 잡화 상점 (하나) | 무제한 (씨앗, 비료) | 계절 전환 시 씨앗 교체 |
| 대장간 (철수) | 무제한 (재료) | 리셋 없음 |
| 목공소 (목이) | 서비스형 (1건씩 의뢰) | 건설 완료 후 재의뢰 가능 |
| 여행 상인 (바람이) | 제한 (아이템당 1~3개) | 매주 새 인벤토리 생성 |

### 8.4 영업시간 외 방문

| 상황 | 처리 |
|------|------|
| 영업시간 외 도착 | 문이 닫혀 있음. 상호작용 시 "영업시간이 아닙니다" 텍스트 표시 |
| 휴무일 | 문이 닫혀 있음. 상호작용 시 "오늘은 쉬는 날입니다" 텍스트 표시 |
| 폭풍/폭설 임시 마감 | "악천후로 문을 닫았습니다" 텍스트 표시 |
| NPC 위치 | 영업시간 외에는 NPC가 건물 안에 있어 보이지 않음 |

---

## 9. 튜닝 파라미터 요약

### 9.1 여행 상인 파라미터

| 파라미터 | 영문 키 | 현재 값 | 조정 범위 | 영향 |
|----------|---------|---------|-----------|------|
| 등장 요일 | `travelMerchantDays` | 토/일 | - | 등장 빈도 |
| 최소 플레이어 레벨 | `travelMerchantMinLevel` | 2 | 1~3 | 여행 상인 첫 등장 시기 |
| 판매 아이템 수 | `travelMerchantItemCount` | 4~6 | 3~8 | 쇼핑 선택지 다양성 |
| 아이템당 재고 | `travelMerchantStock` | 1~3 | 1~5 | 아이템 희소성 |
| 아이템 쿨다운 | `travelMerchantCooldown` | 2주 | 1~4주 | 아이템 갱신 주기 |
| 비계절 씨앗 가격 배수 | `offSeasonSeedPriceMult` | 1.5 | 1.5~3.0 | 사전 준비 비용 (→ see docs/balance/traveler-economy.md) |

### 9.2 만능 비료 파라미터

| 파라미터 | 영문 키 | 현재 값 | 조정 범위 | 영향 |
|----------|---------|---------|-----------|------|
| 가격 | `universalFertPrice` | 80 | 50~250 | 접근성 (→ see docs/balance/traveler-economy.md) |
| 품질 보정 | `universalFertQuality` | 0.15 | 0.10~0.25 | 비료 효과 |
| 성장 속도 보정 | `universalFertGrowth` | 0.10 | 0.05~0.15 | 비료 효과 |

### 9.3 특수 아이템 파라미터

| 파라미터 | 영문 키 | 현재 값 | 조정 범위 | 영향 |
|----------|---------|---------|-----------|------|
| 에너지 토닉 회복량 | `energyTonicAmount` | 50 | 30~70 | 일일 에너지 경제 |
| 에너지 토닉 일일 제한 | `energyTonicDailyLimit` | 1 | 1~3 | 에너지 시스템 밸런스 |
| 성장 촉진제 단축 일수 | `growthAccelDays` | 2 | 1~3 | 작물 성장 밸런스 (→ see docs/balance/traveler-economy.md) |
| 행운 부적 Iridium 확률 증가 | `luckyCharmIridiumBonus` | 0.15 | 0.03~0.20 | 품질 밸런스 (→ see docs/balance/traveler-economy.md) |

### 9.4 대화 시스템 파라미터

| 파라미터 | 영문 키 | 현재 값 | 조정 범위 | 영향 |
|----------|---------|---------|-----------|------|
| 대화 범위 (타일) | `dialogueRange` | 2 | 1~3 | 상호작용 편의성 |
| 힌트 대사 쿨다운 (일) | `hintCooldownDays` | 3 | 1~7 | 힌트 반복 빈도 |
| 일반 인사 풀 크기 | `greetingPoolSize` | 3~4 | 3~6 | 대사 다양성 |

---

## Cross-references

| 문서 | 참조 내용 |
|------|-----------|
| `docs/design.md` 섹션 4.2 | 작물 목록, 씨앗 가격, 판매가 (잡화 상점 인벤토리) |
| `docs/design.md` 섹션 4.4 | 경제 시스템 개요 |
| `docs/design.md` 섹션 4.6 | 시설 비용, 해금 레벨 (목공소 인벤토리) |
| `docs/systems/economy-system.md` 섹션 2 | 가격 시스템 전반 |
| `docs/systems/economy-system.md` 섹션 3 | 상점 종류, 영업시간, 인벤토리 규칙 |
| `docs/systems/economy-system.md` 섹션 4 | 거래 메커니즘 (구매/판매 UI) |
| `docs/systems/tool-upgrade.md` 섹션 2 | 도구 업그레이드 비용, 재료, 레벨 요건 |
| `docs/systems/tool-upgrade.md` 섹션 3 | 도구별 업그레이드 상세 |
| `docs/systems/tool-upgrade.md` 섹션 6 | 대장간 NPC 기본 정보, 상호작용 흐름 |
| `docs/content/blacksmith-npc.md` | 철수 캐릭터 심화 설정, 전체 대화 스크립트, UX 상세 (본 문서 섹션 4 확장) |
| `docs/content/merchant-npc.md` | 하나 캐릭터 심화 설정, 전체 대화 스크립트, UX 상세 (본 문서 섹션 3 확장) |
| `docs/content/carpenter-npc.md` | 목이 캐릭터 심화 설정, 전체 대화 스크립트, UX 상세 (본 문서 섹션 5 확장) |
| `docs/content/traveler-npc.md` | 바람이 캐릭터 심화 설정, 전체 대화 스크립트, 여행 일지 시스템 (본 문서 섹션 6 확장) |
| `docs/systems/blacksmith-architecture.md` | 대장간 NPC 기술 아키텍처 (ARC-020) |
| `docs/systems/farming-system.md` 섹션 5.1 | 비료 종류, 가격, 해금 |
| `docs/content/facilities.md` 섹션 2 | 시설 건설 프로세스, 건설 시간 |
| `docs/content/crops.md` | 작물 콘텐츠 상세 |
| `docs/balance/progression-curve.md` 섹션 1.3.2 | 레벨별 해금 콘텐츠 |
| `docs/systems/time-season.md` | 시간대, 날씨, 계절 정보 |

---

## Open Questions

1. ~~[OPEN] **겨울 온실 전용 씨앗 판매 경로**~~ -- **RESOLVED (DES-014)**: **옵션 C(혼합) 확정**. 여행 상인: 겨울 1주차부터 x1.5 프리미엄 판매 (기존 유지). 잡화 상점(하나): 겨울 2주차(Day 8)부터 정가 판매 (온실 보유 조건). 근거: (1) 접근성 -- 여행 상인 독점 시 "주말 방문 + 20% 등장 확률"에 의존하여 겨울 콘텐츠 진입이 과도하게 불확실함. (2) 여행 상인 가치 보존 -- 1주 선점 + 재고 한정의 희소성으로 x1.5 프리미엄 정당화. (3) 경제 밸런스 -- 잡화 상점 정가 판매가 온실 투자(→ see `docs/content/facilities.md`) 회수를 합리적으로 보장. 상세: 섹션 3.3, 6.3.

2. [OPEN] **NPC 호감도 시스템**: 현재 설계에는 NPC 호감도(friendship) 시스템이 없다. 호감도를 올리면 특별 할인이나 독점 아이템 해금이 가능한 시스템 도입 여부. 스코프 확장 우려가 있으므로 신중히 검토 필요.

3. [OPEN] **여행 상인 가격 밸런스**: 여행 상인의 아이템 가격이 전반적으로 높게 설정되어 있다. 에너지 토닉(200G)이 일일 에너지 시스템을 우회하는 정도가 적절한지 밸런스 테스트 필요.

4. [OPEN] **대장간 추가 서비스**: 대장간에서 울타리, 스프링클러 부품 등 농장 시설 관련 금속 제품을 추가 판매할지. 도입 시 목공소와의 역할 구분 재정립 필요.

5. [OPEN] **목공소 동시 건설 제한**: 현재 1건씩만 의뢰 가능하도록 설계했으나, 후반 플레이어의 대기 시간 불만이 우려됨. 동시 2건까지 허용할지, 혹은 추가 비용으로 급행 건설을 도입할지 검토 필요.

6. ~~[OPEN] **대장간 영업시간 (tool-upgrade.md와의 불일치 해소)**~~ — **RESOLVED (FIX-067)**: `docs/systems/tool-upgrade.md` 섹션 6.1 영업시간이 `(→ see docs/systems/economy-system.md 섹션 3.2)` canonical 참조로 교체됨. 불일치 해소.

---

## Risks

1. [RISK] **여행 상인 아이템 밸런스 파괴**: 에너지 토닉, 성장 촉진제, 행운의 부적 등 특수 아이템이 핵심 시스템(에너지, 성장, 품질)을 우회하여 밸런스를 해칠 수 있다. 완화책: 높은 가격 + 제한된 재고(1~3개) + 일일 사용 제한으로 영향 억제. 플레이테스트 후 수치 조정 필요.

2. [RISK] **NPC 대사 분량**: 현재 설계의 대사 분량이 최소 수준이다. Phase 2 구현 시 반복 방문에서 대사가 단조로워질 수 있다. 계절당 3~5개, 진행도별 5~8개 이상으로 대사 풀을 확장해야 할 수 있다.

3. [RISK] **여행 상인 무작위성 좌절감**: 원하는 아이템이 등장하지 않을 경우 플레이어가 좌절감을 느낄 수 있다. 완화책: 2주 쿨다운 규칙으로 순환 보장, 아이템 풀 크기를 적정 수준(8~10종)으로 유지.

4. [RISK] **영업시간/휴무일에 의한 접근성 문제**: 특히 대장간(10:00~16:00, 6시간 영업)이 너무 짧아 방문 기회가 제한될 수 있다. 게임 내 1일 = 실시간 10분이므로 대장간 영업시간은 실시간 약 2.5분에 해당. 편의성과 전략성의 균형 모니터링 필요.

5. ~~[RISK] **tool-upgrade.md와의 영업시간 불일치**~~ — **해소 (FIX-067)**: tool-upgrade.md 섹션 6.1이 canonical 참조로 교체됨.

---

*이 문서는 NPC/상점 콘텐츠의 canonical 문서이다. NPC 이름, 성격, 대화 스크립트, 여행 상인 아이템 풀, 대화 시스템 규칙은 이 문서만을 정본으로 한다.*

---

# 가공/요리 시스템 콘텐츠 상세 (Processing & Cooking Content Specification)

> 작성: Claude Code (Opus) | 2026-04-06 | FIX-083 채집물 가공 레시피 추가: 2026-04-07 | DES-019 베이커리 채집물 가격 조정: 2026-04-07 | DES-020 철 광석 제련 레시피 추가: 2026-04-07 | CON-022 음식 레시피 19종 통합: 2026-04-09
> 문서 ID: CON-005 | Phase 1

---

## 1. Context

이 문서는 SeedMind의 가공/요리 시스템에 등장하는 **전체 레시피, 가공소 유형, 연료 규칙, 배치/가동 규칙, 가공품 퀄리티 연동**을 상세히 기술한다. 기존 단일 가공소(`building_processing`)의 잼/주스/절임 레시피를 기반으로, **특화 가공소 4종**(제분소, 발효실, 베이커리)을 신규 도입하여 가공 시스템의 깊이를 확장한다.

**설계 목표**:
- 원재료를 가공품으로 변환하여 부가가치를 창출하는 경제적 동기 제공
- 가공소 유형별 역할 분화로 "어떤 가공소를 먼저 건설/업그레이드할 것인가"의 전략적 선택 유도
- 가공 vs 직판, 잼 vs 주스 등 모든 경로에 트레이드오프 부여
- BAL-004(가공품 ROI 밸런스 분석)의 canonical 선행 문서로 기능

### 1.1 본 문서가 canonical인 데이터

- 전체 레시피 목록 (레시피 ID, 재료, 결과물, 처리 시간, 해금 조건)
- **가공품 판매 가격** (이 문서가 유일한 출처)
- 채집물 가공 레시피 13종 (가공소 9종, 발효실 2종, 베이커리 2종)
- 음식 레시피 (일반 요리 8종 + 고급 요리 7종 + 최고급 요리 4종 = 19종) — 재료, 가공 시간, 연료, 해금 조건
- 특화 가공소 4종(제분소, 발효실, 베이커리) 기본 정보
- 연료 시스템 규칙
- 배치/가동 규칙 (작업 큐, 동시 가동 제한)
- 가공품 퀄리티 연동 규칙

### 1.2 본 문서가 canonical이 아닌 데이터 (참조만)

| 데이터 종류 | 참조처 |
|------------|--------|
| 작물 기본 판매가, 씨앗 가격, 성장일수 | `docs/design.md` 섹션 4.2 |
| 가공소(일반) 건설 비용, 해금 레벨 | `docs/design.md` 섹션 4.6 |
| 가공 공식 (배수 + 고정 보너스) | `docs/systems/economy-system.md` 섹션 2.5 |
| 가공소 슬롯 수, 슬롯 확장 비용 | `docs/systems/economy-system.md` 섹션 2.5 |
| 가공소(일반) 업그레이드 경로, 속도 배수 | `docs/content/facilities.md` 섹션 6.6 |
| 가공소(일반) 점유 타일, 건설 시간 | `docs/content/facilities.md` 섹션 6.1 |
| 겨울 전용 작물 수치 (겨울무, 표고버섯, 시금치) | `docs/content/crops.md` 섹션 3.9~3.11 |
| 품질 등급 정의, 가격 배수 | `docs/systems/crop-growth.md` 섹션 4.3~4.4 |
| 작물 분류 (과일/채소) | `docs/systems/economy-system.md` 섹션 2.5 |
| 채집 아이템 기본 판매가 (BAL-016 적용) | `docs/systems/gathering-system.md` 섹션 3.3~3.7 |
| 채집물별 가공 연계 방향, NPC 선물 적합도 | `docs/content/gathering-items.md` 섹션 9 |
| 채집 숙련도 레벨, XP 획득량 | `docs/systems/gathering-system.md` 섹션 4~5 |
| 음식 아이템 등급, 회복량, 특수 효과, 획득 경로, 판매가 | `docs/content/food-items.md` |

---

## 2. 가공 시스템 개요

### 2.1 원재료 -> 가공품 변환 메카닉

플레이어는 수확한 작물(원재료)을 가공소에 투입하여, 시간을 들여 가공품으로 변환한다. 가공품은 원재료보다 높은 판매가를 가지며, 이것이 가공의 핵심 경제적 동기다.

```
[원재료 투입] -> [가공 시간 경과] -> [가공품 완성] -> [판매 or 보관]
```

**가공의 경제적 동기**:
- 일반 품질(Normal) 작물은 직판보다 가공이 거의 항상 유리
- 고품질(Gold/Iridium) 작물은 직판이 유리할 수 있음 (-> see `docs/content/facilities.md` 섹션 6.5)
- 가공 시간이라는 기회비용이 존재하므로, "무엇을 가공할지"가 전략적 선택

### 2.2 가공소 유형 총괄

SeedMind에는 **4종의 가공소**가 존재한다. 1종은 기존 일반 가공소, 3종은 특화 가공소(신규)다.

| 가공소 | 영문 ID | 해금 | 건설 비용 | 역할 | 레시피 수 |
|--------|---------|------|----------|------|----------|
| 가공소 (일반) | `building_processing` | (-> see `docs/design.md` 섹션 4.6) | (-> see `docs/design.md` 섹션 4.6) | 잼, 주스, 절임, 생선 가공(구운/훈제/스튜), **채집물 가공**, **광석 제련** | 39종 |
| 제분소 | `building_mill` | 레벨 5 | 1,500G | 곡물/작물 -> 가루/분말 | 4종 |
| 발효실 | `building_fermentation` | 레벨 8 | 4,000G | 과일/채소 -> 와인, 식초, 된장 등, **채집물 발효** | 7종 |
| 베이커리 | `building_bakery` | 레벨 9 | 5,000G | 가공품 + 가공품 -> 고급 요리, 생선 가공(초밥/파이), **채집물 요리** | 20종 |
| 치즈 공방 | `building_cheese_workshop` | 레벨 8 + 외양간 Lv.1 | 4,500G | 우유/염소젖 -> 유제품, 2차 가공 | 5종 |

**총 레시피 수: 75종** (일반 가공소 39종[작물 가공 18 + 생선 가공소 분 3 + 채집물 9 + 광석 제련 1 + **일반 요리 8**] + 제분소 4종 + 발효실 7종[작물 5 + 채집물 2] + 베이커리 20종[요리 5 + 생선 베이커리 분 2 + 채집물 2 + **고급/최고급 음식 11**] + 치즈 공방 5종)

**시설 연쇄 설계**: 일부 특화 가공소의 레시피는 다른 가공소의 산출물을 재료로 요구한다. 예를 들어, 베이커리의 "빵"은 제분소에서 만든 "밀가루(옥수수 가루)"를 재료로 사용한다. 이 연쇄 구조가 시설 건설 순서에 전략적 의미를 부여한다.

```
[작물] -> [제분소] -> [가루류]
                         ↓
[작물] -> [가공소] -> [잼/주스] -> [베이커리] -> [고급 요리]
                                      ↑
[작물] -> [발효실] -> [와인/식초]
```

### 2.3 특화 가공소 기본 정보

#### 2.3.1 제분소 (Mill)

| 항목 | 내용 |
|------|------|
| 영문 ID | `building_mill` |
| 건설 비용 | 1,500G |
| 해금 조건 | 레벨 5 |
| 점유 타일 | 3x2 (6타일) |
| 최대 건설 수 | 1기 |
| 건설 소요 시간 | 1일 |
| 기능 요약 | 곡물/작물을 가루 형태로 분쇄 |
| 슬롯 | 1 (고정, 확장 불가) |

**설계 의도**: 제분소는 중간 가공재를 생산하는 저비용 시설이다. 그 자체로는 수익성이 크지 않지만, 베이커리의 선행 시설로서 가치가 있다. 온실 해금(레벨 5)과 동시에 건설 가능하여 중반 콘텐츠를 풍부하게 한다.

#### 2.3.2 발효실 (Fermentation Room)

| 항목 | 내용 |
|------|------|
| 영문 ID | `building_fermentation` |
| 건설 비용 | 4,000G |
| 해금 조건 | 레벨 8 |
| 점유 타일 | 3x3 (9타일) |
| 최대 건설 수 | 1기 |
| 건설 소요 시간 | 2일 |
| 기능 요약 | 시간을 들여 작물을 발효시켜 고가 가공품 생산 |
| 슬롯 | 2 (고정, 확장 불가) |

**설계 의도**: 발효실은 가공 시간이 매우 길지만(12~24시간) 보상이 큰 "장기 투자형" 가공소다. 가공소(일반)의 잼/주스가 4~6시간인 데 비해, 발효실의 와인/식초는 하루 이상 걸리며 그만큼 수익도 높다. "즉시 현금화(잼) vs 장기 투자(와인)"의 트레이드오프를 형성한다.

#### 2.3.3 베이커리 (Bakery)

| 항목 | 내용 |
|------|------|
| 영문 ID | `building_bakery` |
| 건설 비용 | 5,000G |
| 해금 조건 | 레벨 9 |
| 점유 타일 | 4x3 (12타일) |
| 최대 건설 수 | 1기 |
| 건설 소요 시간 | 2일 |
| 기능 요약 | 가공 중간재 + 작물을 조합하여 고급 요리 생산 |
| 슬롯 | 2 (고정, 확장 불가) |
| 연료 필요 | 예 (섹션 4 참조) |

**설계 의도**: 베이커리는 가장 늦게 해금되는 최상위 가공소다. 다른 가공소의 산출물(가루, 잼 등)을 재료로 요구하므로, 여러 가공 체인을 완성해야 최고 효율에 도달한다. 이것이 엔드게임의 장기 목표를 제공한다. 유일하게 연료를 소모하는 가공소다.

---

## 3. 가공소별 레시피 (Canonical)

### 3.1 가공소 (일반) — Processing Plant

기존 가공소의 레시피는 `docs/content/facilities.md` 섹션 6.3에 상세히 기재되어 있다. 본 문서에서는 canonical 판매 가격을 확정하고, 기존 레시피를 통합 정리한다.

**가공 공식** (-> see `docs/systems/economy-system.md` 섹션 2.5):
```
가공품_판매가 = floor(원재료_기본판매가 * 가공_배수 + 가공_고정보너스)
```

**가공 유형별 파라미터** (-> see `docs/systems/economy-system.md` 섹션 2.5):

| 가공 유형 | 배수 | 고정 보너스 | 기본 가공 시간 | 적용 분류 |
|----------|------|-----------|--------------|----------|
| 잼 (Jam) | x2.0 | +50G | 4시간 | 모든 과일/채소 |
| 주스 (Juice) | x2.5 | +30G | 6시간 | 과일류만 |
| 절임 (Pickle) | x2.0 | +30G | 4시간 | 채소류만 |

**작물 분류** (-> see `docs/systems/economy-system.md` 섹션 2.5):

| 작물 | 분류 | 가공 가능 유형 |
|------|------|--------------|
| 감자 | 채소 | 잼, 절임 |
| 당근 | 채소 | 잼, 절임 |
| 토마토 | 과일/채소 겸용 | 잼, 주스, 절임 |
| 옥수수 | 채소 | 잼, 절임 |
| 딸기 | 과일 | 잼, 주스 |
| 호박 | 채소 | 잼, 절임 |
| 해바라기 | 특수 (씨앗) | 가공 불가 |
| 수박 | 과일 | 잼, 주스 |
| 겨울무 | 채소 | 절임 |
| 표고버섯 | 채소/특수 | 잼 |
| 시금치 | 채소 | 절임 |

#### 3.1.1 잼 레시피 (7종)

| 레시피 ID | 재료 | 수량 | 결과물 | 판매 가격 | 가공 시간 | 해금 조건 |
|----------|------|------|--------|----------|----------|----------|
| `recipe_jam_potato` | 감자 | 1 | 감자 잼 | 110G | 4시간 | 가공소 건설 |
| `recipe_jam_carrot` | 당근 | 1 | 당근 잼 | 120G | 4시간 | 가공소 건설 |
| `recipe_jam_tomato` | 토마토 | 1 | 토마토 잼 | 170G | 4시간 | 가공소 건설 |
| `recipe_jam_corn` | 옥수수 | 1 | 옥수수 잼 | 250G | 4시간 | 가공소 건설 |
| `recipe_jam_strawberry` | 딸기 | 1 | 딸기 잼 | 210G | 4시간 | 가공소 건설 |
| `recipe_jam_pumpkin` | 호박 | 1 | 호박 잼 | 450G | 4시간 | 가공소 건설 |
| `recipe_jam_watermelon` | 수박 | 1 | 수박 잼 | 750G | 4시간 | 가공소 건설 |

**판매가 근거**: `원재료_기본판매가 x 2.0 + 50G`. 원재료 기본판매가는 (-> see `docs/design.md` 섹션 4.2).

#### 3.1.2 주스 레시피 (3종)

| 레시피 ID | 재료 | 수량 | 결과물 | 판매 가격 | 가공 시간 | 해금 조건 |
|----------|------|------|--------|----------|----------|----------|
| `recipe_juice_tomato` | 토마토 | 1 | 토마토 주스 | 180G | 6시간 | 가공소 건설 |
| `recipe_juice_strawberry` | 딸기 | 1 | 딸기 주스 | 230G | 6시간 | 가공소 건설 |
| `recipe_juice_watermelon` | 수박 | 1 | 수박 주스 | 905G | 6시간 | 가공소 건설 |

**판매가 근거**: `원재료_기본판매가 x 2.5 + 30G`.

#### 3.1.3 절임 레시피 (5종)

| 레시피 ID | 재료 | 수량 | 결과물 | 판매 가격 | 가공 시간 | 해금 조건 |
|----------|------|------|--------|----------|----------|----------|
| `recipe_pickle_potato` | 감자 | 1 | 감자 절임 | 90G | 4시간 | 가공소 건설 |
| `recipe_pickle_carrot` | 당근 | 1 | 당근 절임 | 100G | 4시간 | 가공소 건설 |
| `recipe_pickle_tomato` | 토마토 | 1 | 토마토 절임 | 150G | 4시간 | 가공소 건설 |
| `recipe_pickle_corn` | 옥수수 | 1 | 옥수수 절임 | 230G | 4시간 | 가공소 건설 |
| `recipe_pickle_pumpkin` | 호박 | 1 | 호박 절임 | 430G | 4시간 | 가공소 건설 |

**판매가 근거**: `원재료_기본판매가 x 2.0 + 30G`.

#### 3.1.4 겨울 작물 가공 레시피 (3종)

| 레시피 ID | 재료 | 수량 | 결과물 | 유형 | 판매 가격 | 가공 시간 | 해금 조건 |
|----------|------|------|--------|------|----------|----------|----------|
| `recipe_pickle_winter_radish` | 겨울무 | 1 | 겨울무 절임 | 절임 | 120G | 4시간 | 가공소 건설 |
| `recipe_jam_shiitake` | 표고버섯 | 1 | 표고버섯 잼 | 잼 | 190G | 4시간 | 가공소 건설 |
| `recipe_pickle_spinach` | 시금치 | 1 | 시금치 절임 | 절임 | 290G | 4시간 | 가공소 건설 |

**판매가 근거**: 겨울무 절임 = 45 x 2.0 + 30 = 120G. 표고버섯 잼 = 70 x 2.0 + 50 = 190G. 시금치 절임 = 130 x 2.0 + 30 = 290G. 원재료 판매가는 (-> see `docs/content/crops.md` 섹션 3.9~3.11).

#### 3.1.5 일반 요리 레시피 (8종) — CON-022

가공소(일반)에서 제조하는 에너지 회복 음식 아이템. 회복량/등급/특수효과 canonical은 (→ see `docs/content/food-items.md` 섹션 2~3.2).

**일반 요리 공통 사항**:
- 연료: 불필요
- 결과물 수량: 1개 (모두)
- 해금 조건: 가공소 건설 즉시
- 상점 판매가 = 직접 제조 판매가 × 1.5 (일부 예외)

| 레시피 ID | 재료 | 결과물 itemId | 이름 | 가공 시간 | 판매가 | 상점 구매가 |
|----------|------|-------------|------|:--------:|:------:|:-----------:|
| `recipe_food_common_roasted_corn` | 옥수수 x1 | `item_food_roasted_corn` | 구운 옥수수 | 1시간 | 80G | 120G |
| `recipe_food_common_potato_soup` | 감자 x2 | `item_food_potato_soup` | 감자 수프 | 1시간 | 50G | 75G |
| `recipe_food_common_tomato_salad` | 토마토 x1 | `item_food_tomato_salad` | 토마토 샐러드 | 30분 | 55G | 80G |
| `recipe_food_common_grilled_fish` | 생선(Common) x1 | `item_food_grilled_fish` | 구운 생선 정식 | 1시간 | 60G | 90G |
| `recipe_food_common_carrot_stew` | 당근 x2 | `item_food_carrot_stew` | 당근 스튜 | 1시간 | 55G | 80G |
| `recipe_food_common_mushroom_soup` | `gather_neungi` x2 또는 `gather_wild_shiitake` x1 | `item_food_mushroom_soup` | 버섯 수프 | 1시간 | 40G | 60G |
| `recipe_food_common_corn_porridge` | 옥수수 x1 + 감자 x1 | `item_food_corn_porridge` | 옥수수 죽 | 1시간 | 75G | — |
| `recipe_food_common_herb_tea` | `gather_spring_herb` x2 또는 `gather_wild_garlic` x2 | `item_food_herb_tea` | 약초 차 | 30분 | 20G | 30G |

**판매가/회복량 canonical**: 판매가 및 회복량은 (→ see `docs/content/food-items.md` 섹션 3.2).

**상점 미판매 예외**: 옥수수 죽(복합 재료) — 상점 판매 없음.

**설계 의도**: 일반 요리는 하루 에너지 100의 25%를 즉시 회복(+25)하는 중급 에너지원이다. 작물 하나로 30~80분 이내 가공하여 에너지 위기 상황에서 즉각 대응 가능하다. 채집물(버섯, 봄나물/달래)을 활용하는 레시피가 포함되어 채집 활동과의 연계를 강화한다. 상점 구매가(50% 마크업)는 "직접 제조 vs 골드 구매"의 트레이드오프를 형성한다.

---

### 3.2 제분소 (Mill) — 가루/분말 레시피 (4종)

제분소는 곡물과 일부 작물을 **가루/분말 형태의 중간 가공재**로 변환한다. 가루류는 그 자체로 판매할 수 있지만, 베이커리 레시피의 재료로 사용될 때 최대 가치를 발휘한다.

**제분 공식**:
```
가루_판매가 = floor(원재료_기본판매가 * 1.5 + 20G)
```

| 레시피 ID | 재료 | 수량 | 결과물 | 결과 수량 | 판매 가격 | 가공 시간 | 해금 조건 |
|----------|------|------|--------|----------|----------|----------|----------|
| `recipe_mill_corn_flour` | 옥수수 | 2 | 옥수수 가루 (Corn Flour) | 1 | 170G | 2시간 | 제분소 건설 |
| `recipe_mill_potato_starch` | 감자 | 3 | 감자 전분 (Potato Starch) | 1 | 85G | 2시간 | 제분소 건설 |
| `recipe_mill_pumpkin_powder` | 호박 | 1 | 호박 분말 (Pumpkin Powder) | 1 | 320G | 3시간 | 제분소 건설 |
| `recipe_mill_radish_powder` | 겨울무 | 2 | 무 분말 (Radish Powder) | 1 | 87G | 2시간 | 제분소 건설 |

**판매가 근거**:
- 옥수수 가루: 옥수수 2개(100G x2 = 200G 원가) -> 100 x 1.5 + 20 = 170G. 주의: 재료 2개 투입이므로 단순 판매(200G) 대비 손해. **베이커리 재료로 사용 시에만 최종 이득**이 발생하도록 설계.
- 감자 전분: 감자 3개(30G x3 = 90G 원가) -> 30 x 1.5 + 20 x 3개분 보정 = floor(30 * 1.5 + 20) = 65G가 아닌, 재료 3개 투입을 반영하여 85G로 조정.
- 호박 분말: 호박 1개(200G 원가) -> 200 x 1.5 + 20 = 320G. 직판(200G) 대비 +120G 이득.
- 무 분말: 겨울무 2개(45G x2 = 90G 원가) -> 45 x 1.5 + 20 x 2개분 보정 = floor(45 * 1.5 + 20) = 87G. 직판(90G) 대비 소폭 손해. 베이커리 연계용.

**설계 의도**: 제분소 가루류는 단독 판매 시 수익이 미미하거나 손해인 경우가 있다. 이는 의도적이다. 가루는 베이커리의 "원재료"로서 가치를 가지며, 제분소만으로는 큰 이득을 볼 수 없게 하여 "제분소 -> 베이커리" 연쇄를 유도한다. 호박 분말만 예외적으로 단독 판매가 유리한데, 이는 호박의 높은 원가와 느린 성장을 보상하기 위함이다.

---

### 3.3 발효실 (Fermentation Room) — 발효 레시피 (5종)

발효실은 작물을 장시간 발효시켜 **고가 가공품**을 생산한다. 가공 시간이 12~24시간으로 매우 길지만, 수익률이 높다.

**발효 공식**:
```
발효품_판매가 = floor(원재료_기본판매가 * 3.0 + 80G)
```

| 레시피 ID | 재료 | 수량 | 결과물 | 결과 수량 | 판매 가격 | 가공 시간 | 해금 조건 |
|----------|------|------|--------|----------|----------|----------|----------|
| `recipe_ferm_strawberry_wine` | 딸기 | 3 | 딸기 와인 (Strawberry Wine) | 1 | 320G | 24시간 | 발효실 건설 |
| `recipe_ferm_watermelon_wine` | 수박 | 2 | 수박 와인 (Watermelon Wine) | 1 | 1,130G | 24시간 | 발효실 건설 |
| `recipe_ferm_tomato_vinegar` | 토마토 | 3 | 토마토 식초 (Tomato Vinegar) | 1 | 260G | 12시간 | 발효실 건설 |
| `recipe_ferm_pumpkin_pickle` | 호박 | 1 | 호박 장아찌 (Pumpkin Preserve) | 1 | 680G | 18시간 | 발효실 건설 |
| `recipe_ferm_spinach_kimchi` | 시금치 | 2 | 시금치 겉절이 (Spinach Kimchi) | 1 | 470G | 12시간 | 발효실 건설 |

**판매가 근거** (공식: `원재료_기본판매가 x 3.0 + 80G`):
- 딸기 와인: 80 x 3.0 + 80 = 320G. 재료 3개(직판 240G) 대비 +80G. 24시간 투자 대비 보상.
- 수박 와인: 350 x 3.0 + 80 = 1,130G. 재료 2개(직판 700G) 대비 +430G. 최고가 발효품.
- 토마토 식초: 60 x 3.0 + 80 = 260G. 재료 3개(직판 180G) 대비 +80G.
- 호박 장아찌: 200 x 3.0 + 80 = 680G. 재료 1개(직판 200G) 대비 +480G.
- 시금치 겉절이: 130 x 3.0 + 80 = 470G. 재료 2개(직판 260G) 대비 +210G. 겨울무 판매가는 (-> see `docs/content/crops.md` 섹션 3.11).

**설계 의도**: 발효실은 "시간을 돈으로 바꾸는" 시설이다. 24시간짜리 와인은 하루에 한 번만 완성되므로 슬롯 2개를 꽉 채워도 하루 최대 2병. 이 희소성이 수박 와인의 높은 가격을 정당화한다. 단, 동일 슬롯에서 12시간짜리 식초를 2번 돌리는 것과 24시간짜리 와인 1번의 효율 비교가 전략적 선택이 된다.

---

### 3.4 베이커리 (Bakery) — 요리 레시피

베이커리는 **다른 가공소의 산출물**(가루, 잼 등)과 작물을 조합하여 최고가 요리를 생산한다. 복수 재료를 요구하는 유일한 가공소이며, 연료(장작)를 소모한다.

#### 3.4.1 기존 요리 레시피 (5종)

**요리 가격 산정**: 베이커리 레시피는 복수 재료 조합이므로 고정 공식 대신 **개별 설정**한다. 기본 원칙: `총 재료 원가 x 2.0 ~ 2.5` 범위.

| 레시피 ID | 재료 | 결과물 | 결과 수량 | 판매 가격 | 가공 시간 | 연료 | 해금 조건 |
|----------|------|--------|----------|----------|----------|------|----------|
| `recipe_bake_corn_bread` | 옥수수 가루 x1 | 옥수수 빵 (Corn Bread) | 1 | 350G | 3시간 | 장작 1 | 베이커리 건설 |
| `recipe_bake_pumpkin_pie` | 호박 분말 x1, 딸기 잼 x1 | 호박 파이 (Pumpkin Pie) | 1 | 1,200G | 4시간 | 장작 1 | 베이커리 건설 |
| `recipe_bake_strawberry_cake` | 옥수수 가루 x1, 딸기 x3 | 딸기 케이크 (Strawberry Cake) | 1 | 680G | 5시간 | 장작 2 | 베이커리 건설 |
| `recipe_bake_veggie_cookie` | 감자 전분 x1, 당근 x2 | 채소 쿠키 (Veggie Cookie) | 3 | 120G (개당) | 3시간 | 장작 1 | 베이커리 건설 |
| `recipe_bake_royal_tart` | 호박 분말 x1, 수박 잼 x1, 딸기 x2 | 로열 타르트 (Royal Tart) | 1 | 2,100G | 6시간 | 장작 2 | 베이커리 건설 |

**판매가 근거**:

| 레시피 | 총 재료 원가 (직판 기준) | 배수 | 판매가 |
|--------|------------------------|------|--------|
| 옥수수 빵 | 옥수수 가루 170G | x2.06 | 350G |
| 호박 파이 | 호박 분말 320G + 딸기 잼 210G = 530G | x2.26 | 1,200G |
| 딸기 케이크 | 옥수수 가루 170G + 딸기 3개 240G = 410G | x1.66 (+ 가공 체인 부가가치) | 680G |
| 채소 쿠키 x3 | 감자 전분 85G + 당근 2개 70G = 155G | x2.32 (총 360G) | 360G (120G x3) |
| 로열 타르트 | 호박 분말 320G + 수박 잼 750G + 딸기 2개 160G = 1,230G | x1.71 (+ 극도의 체인 부가가치) | 2,100G |

**주의: 재료 원가 계산 시 "가공 중간재의 판매가"를 원가로 사용**한다. 실제 원재료(옥수수 2개 + 딸기 3개 등)의 직판가 합산은 이보다 낮으므로, 가공 체인 전체를 거치면 원재료 대비 총 부가가치가 매우 높다.

**설계 의도**: 베이커리는 SeedMind의 최종 가공 체인이다. 로열 타르트(2,100G)는 게임 내 가장 비싼 가공품이며, 제분소 + 가공소(일반) + 베이커리 3단계 체인을 모두 가동해야 만들 수 있다. 이것이 엔드게임의 성취감을 제공한다.

#### 3.4.2 음식 아이템 레시피 — 고급/최고급 (11종) — CON-022

음식 아이템 회복량·등급·특수효과 canonical은 (→ see `docs/content/food-items.md` 섹션 2~3.4).

**고급 요리 공통 사항 (FoodTier.Advanced)**:
- 연료: 장작 x1
- 결과물: +45 에너지 회복 + 임시 maxEnergy +20 (해당 날) — (→ see `docs/content/food-items.md` 섹션 2)
- 해금 조건: 베이커리 건설 즉시

| 레시피 ID | 재료 | 결과물 itemId | 이름 | 가공 시간 | 판매가 |
|----------|------|-------------|------|:--------:|:------:|
| `recipe_food_advanced_pumpkin_stew` | 호박 x1 + 감자 x1 | `item_food_pumpkin_stew` | 호박 스튜 | 2시간 | 350G |
| `recipe_food_advanced_strawberry_jam_toast` | `item_jam_strawberry` x1 + 옥수수 가루 x1 | `item_food_strawberry_jam_toast` | 딸기 잼 토스트 | 2시간 | 320G |
| `recipe_food_advanced_fish_stew_deluxe` | 생선(Uncommon+) x1 + 감자 x1 + 당근 x1 | `item_food_fish_stew_deluxe` | 특제 생선 스튜 | 3시간 | 380G |
| `recipe_food_advanced_spring_bibimbap` | `gather_wild_garlic` x2 + `gather_spring_herb` x1 + 달걀 x1 | `item_food_spring_bibimbap` | 달걀 봄나물 비빔밥 | 2시간 | 300G |
| `recipe_food_advanced_autumn_mushroom` | `gather_pine_mushroom` x1 + `gather_reishi` x1 | `item_food_autumn_mushroom_dish` | 가을 버섯 요리 | 2시간 | 340G |
| `recipe_food_advanced_watermelon_sorbet` | 수박 x1 | `item_food_watermelon_sorbet` | 수박 셔벗 | 2시간 | 480G |
| `recipe_food_advanced_cheese_gratin` | `item_cheese` x1 + 감자 x2 | `item_food_cheese_gratin` | 치즈 그라탱 | 3시간 | 420G |

**판매가 및 ROI 근거** (재료 기회비용 + 연료 30G 합산):
| 레시피 | 재료 기회비용 | 연료 | 순이익 |
|--------|:-----------:|:----:|:------:|
| 호박 스튜 | 호박 200G + 감자 30G = 230G | 30G | +90G |
| 딸기 잼 토스트 | 딸기 잼 210G + 옥수수 가루 170G = 380G | 30G | -90G |
| 특제 생선 스튜 | 생선Uncommon ~50G + 감자 30G + 당근 35G = 115G | 30G | +235G |
| 달걀 봄나물 비빔밥 | 달래 5Gx2 + 봄나물 8G + 달걀 35G = 53G | 30G | +217G |
| 가을 버섯 요리 | 송이 32G + 영지 24G = 56G | 30G | +254G |
| 수박 셔벗 | 수박 350G | 30G | +100G |
| 치즈 그라탱 | 치즈 250G + 감자 30Gx2 = 310G | 30G | +80G |

**딸기 잼 토스트 설계 노트**: 재료 원가가 판매가보다 높아 직접 판매 시 손해(-90G). 에너지 회복(+45 + maxEnergy +20) 목적으로만 제조 의미 있음. "골드 → 에너지" 고비용 우회로를 의도적으로 허용하되 비효율적으로 설계.

**생선(Uncommon+) 기준 판매가**: (~40~60G) (→ see `docs/systems/fishing-system.md` 섹션 4.2).

---

**최고급 요리 공통 사항 (FoodTier.Premium)**:
- 연료: 장작 x2
- 결과물: +60 에너지 회복 + 임시 maxEnergy +20 + 이동 속도 +20%\* (해당 날) — (→ see `docs/content/food-items.md` 섹션 2)
- 해금 조건: 베이커리 건설 + 채집 숙련도 Lv.5 이상

\* [OPEN] 이동 속도 +20% 잠정값 — 이동 속도 시스템 설계 후 최종 검증 필요.

| 레시피 ID | 재료 | 결과물 itemId | 이름 | 가공 시간 | 판매가 |
|----------|------|-------------|------|:--------:|:------:|
| `recipe_food_premium_lotus_feast` | `gather_golden_lotus` x1 + 수박 x1 | `item_food_lotus_tea_feast` | 황금 연꽃 만찬 | 4시간 | 900G |
| `recipe_food_premium_millennium_soup` | `gather_millennium_reishi` x1 + 달걀 x2 + 당근 x1 (= `item_chicken_soup` 전처리 필요 — [OPEN] `docs/content/food-items.md` Open Questions 2번 해소 후 동기화) | `item_food_millennium_soup` | 천년 영지 보양식 | 4시간 | 850G |
| `recipe_food_premium_royal_harvest` | 호박 분말 x1 + 수박 잼 x1 + `gather_wild_ginseng` x1 | `item_food_royal_harvest` | 왕실 수확 연회 | 5시간 | 1,200G |
| `recipe_food_premium_ginseng_elixir` | `gather_wild_ginseng` x2 + `gather_wild_garlic` x3 + `item_sugar` x1 | `item_food_ginseng_elixir` | 산삼 강정 | 5시간 | 1,100G |

**판매가 및 ROI 근거** (재료 기회비용 + 연료 60G 합산):
| 레시피 | 재료 기회비용 | 연료 | 순이익 |
|--------|:-----------:|:----:|:------:|
| 황금 연꽃 만찬 | 황금 연꽃 ~100G [OPEN] + 수박 350G = ~450G | 60G | ~+390G |
| 천년 영지 보양식 | 천년 영지 ~120G [OPEN] + 달걀 35Gx2 + 당근 35G = ~225G | 60G | ~+565G |
| 왕실 수확 연회 | 호박 분말 320G + 수박 잼 750G + 산삼 80G = 1,150G | 60G | -10G |
| 산삼 강정 | 산삼 80Gx2 + 달래 5Gx3 + 설탕 50G = 225G | 60G | +815G |

[OPEN] 황금 연꽃(`gather_golden_lotus`) / 천년 영지(`gather_millennium_reishi`) 판매가 잠정값(100G/120G) — FIX-119에서 gathering-items.md canonical 확정 예정.
[OPEN] `item_sugar` 잡화 상점(하나) 판매가 50G 잠정값 — 별도 확정 필요.

**왕실 수확 연회 설계 노트**: 재료 원가(1,210G) > 판매가(1,200G). 에너지 목적 전용 아이템. 최고급 요리 중 유일하게 가공 체인(제분소 → 가공소 → 베이커리) 완성을 요구하여 엔드게임 성취 상징.

**산삼 강정 경제적 포지셔닝**: 순이익 +815G는 베이커리 전체 레시피 중 최고. 단, 산삼 x2는 봄 Legendary 0.5~1% 등장으로 희소성이 극도로 높음.

**설계 의도**: 고급/최고급 요리는 에너지 회복 목적 외에도 임시 maxEnergy 확장(+20) 효과로 하루 총 에너지를 120까지 늘려 일일 채집/낚시 횟수를 10~20% 증가시킨다. 최고급의 이동 속도 보너스는 채집 포인트 순환 효율 향상으로 이어져 채집/낚시 활동의 전략적 가치를 높인다.

---

### 3.5 생선 가공 레시피 (5종)

생선은 낚시 시스템(-> see `docs/systems/fishing-system.md` 섹션 4.2)에서 획득한 어종을 가공소 또는 베이커리에서 가공한다. 배수형 레시피(구운/훈제/초밥)는 원재료 어종의 basePrice에 비례하여 판매가가 결정되고, 고정가형 레시피(스튜/파이)는 어종과 무관한 고정 판매가를 가진다.

**생선 가공 공식 (배수형)**:
```
가공품_판매가 = floor(원재료_기본판매가 * 가공_배수 + 가공_고정보너스)
```

| 레시피 ID | 재료 | 수량 | 결과물 | 가공 유형 | 판매가 공식 | 가공 시간 | 가공소 | 해금 조건 |
|----------|------|------|--------|----------|-----------|----------|--------|----------|
| `recipe_grilled_fish` | 생선 (Common/Uncommon) | 1 | 구운 생선 (Grilled Fish) | 배수형 | basePrice x1.8 + 20G | 2시간 | 가공소 | 가공소 건설 |
| `recipe_smoked_fish` | 생선 (아무 등급) + 목재 | 1 + 2 | 훈제 생선 (Smoked Fish) | 배수형 | basePrice x2.2 + 40G | 4시간 | 가공소 | 가공소 건설 |
| `recipe_fish_sushi` | 생선 (Uncommon+) + 쌀 | 1 + 1 | 생선 초밥 (Fish Sushi) | 배수형 | basePrice x2.5 + 60G | 3시간 | 베이커리 | 베이커리 건설 |
| `recipe_fish_stew` | 생선 + 감자 | 2 + 1 | 생선 스튜 (Fish Stew) | 고정가 | 180G | 5시간 | 가공소 | 가공소 건설 |
| `recipe_fish_pie` | 생선 + 밀가루 | 1 + 2 | 생선 파이 (Fish Pie) | 고정가 | 250G | 6시간 | 베이커리 | 베이커리 건설 |

**판매가 근거 (배수형, 대표 어종 예시)**:
- 구운 붕어: 20 x 1.8 + 20 = 56G (부가가치 +36G, 2시간)
- 훈제 잉어: 30 x 2.2 + 40 = 106G (부가가치 +76G, 4시간, 목재 x2 추가)
- 초밥(송어): 50 x 2.5 + 60 = 185G (부가가치 +105G(쌀 30G 가정 시), 3시간)

**고정가형 설계 의도**: 생선 스튜(180G)와 생선 파이(250G)는 어떤 어종을 사용해도 동일 가격이다. 따라서 **저가 Common 어종으로 만들 때 효율적**이고, 고가 Rare 어종은 직판 또는 배수형 가공이 유리하다. 이 구조가 "어떤 생선을 어떤 레시피에 투입할 것인가"의 전략적 선택을 만든다.

**희귀도 제한**:
- 구운 생선: Common/Uncommon만 사용 가능 (Rare+ 어종은 가치가 높아 구이보다 직판/고급 가공이 유리하도록)
- 생선 초밥: Uncommon 이상만 사용 가능 (고급 재료 요구)
- 훈제 생선/스튜/파이: 모든 등급 사용 가능

[OPEN] 쌀(Rice)과 밀가루(Flour)는 현재 작물 목록에 없다. 상점 구매 전용 재료로 처리(잡화 상점에서 쌀 30~50G, 밀가루 25~40G) 또는 향후 곡물 작물 추가 시 자급 가능하게 할 수 있다. 가격 확정은 밸런스 분석(-> see `docs/balance/fishing-economy.md` 섹션 6.4)에 따른다.

**경제 밸런스**: 생선 가공 ROI 분석은 (-> see `docs/balance/fishing-economy.md` 섹션 4).

---

### 3.6 치즈 공방 (Cheese Workshop) — 유제품 레시피 (5종)

치즈 공방은 소/염소의 생산물(우유, 염소젖)을 **유제품**으로 가공한다. 연료 소모 없이 운영 가능하며, 발효실과 동일한 레벨 8에서 해금된다. 에이지드 치즈는 치즈를 재투입하는 **2차 가공** 레시피로, 긴 처리 시간 대신 높은 수익을 제공한다. 버터와 크림은 베이커리 레시피 연계용 중간 가공재다.

**원재료 출처**: 우유/염소젖은 동물 생산물이다 (-> see `docs/content/livestock-system.md` 섹션 1.1, 4.1).

**처리 구조**:
```
[소/염소] → 우유/염소젖
          ↓
      [치즈 공방] → 치즈/염소 치즈 (직판)
                   → 에이지드 치즈 (2차 가공, 장기 투자)
                   → 버터/크림 (베이커리 연계 재료)
```

| 레시피 ID | 재료 | 수량 | 결과물 | 처리 시간 | 연료 | 판매가 | 해금 조건 |
|----------|------|------|--------|----------|------|--------|----------|
| `recipe_cheese_basic` | 우유 (`item_milk`) | x1 | 치즈 (Cheese) | 4시간 | 없음 | 250G | 레벨 8 |
| `recipe_cheese_goat` | 염소젖 (`item_goat_milk`) | x1 | 염소 치즈 (Goat Cheese) | 4시간 | 없음 | 190G | 레벨 8 |
| `recipe_butter` | 우유 (`item_milk`) | x2 | 버터 (Butter) | 3시간 | 없음 | 160G | 레벨 8 |
| `recipe_cheese_aged` | 치즈 (`item_cheese`) | x1 | 에이지드 치즈 (Aged Cheese) | 12시간 | 없음 | 680G | 레벨 8 |
| `recipe_cream` | 우유 (`item_milk`) + 달걀 (`item_egg`) | x1 + x1 | 크림 (Cream) | 3시간 | 없음 | 280G | 레벨 8 |

**판매가 근거 (BAL-008 기준)**:
- 치즈: 우유 120G × 1.8 + 34G ≈ 250G (-> see `docs/balance/livestock-economy.md` 섹션 2.1~2.2)
- 염소 치즈: 염소젖 80G × 2.0 + 30G = 190G (염소젖 낮은 원가 → 높은 배수 보상)
- 버터: 우유 2개(240G) → 160G. 단독 판매 시 손해(-80G)이나 베이커리 연계용 중간 가공재
- 에이지드 치즈: 치즈(250G) × 2.5 + 55G ≈ 680G (12시간 장기 투자 보상 — 발효실 와인 패턴 유사)
- 크림: (우유 120G + 달걀 35G) × 1.8 ≈ 279G → 280G (복합 재료 프리미엄)

**슬롯 구성**:
| 파라미터 | 값 |
|----------|-----|
| 초기 슬롯 | 2 |
| 최대 슬롯 | 2 (확장 불가, 특화 가공소 고정 정책) |
| 연료 사용 | 없음 |

**설계 의도**: 치즈 공방은 목축 시스템의 핵심 수익 증폭기다. 기본 우유 직판(120G)을 치즈(250G)로 가공하면 약 2배 수익이 되며, 에이지드 치즈로 2차 가공 시 최대 6배 수익(우유 120G → 680G)에 도달한다. 버터와 크림은 단독 판매 효율이 낮지만 베이커리와의 연계로 가치를 발휘하며, 닭(달걀)과 소(우유)를 모두 보유해야 크림을 제조할 수 있어 다종 동물 사육을 유도한다.

[OPEN] 베이커리 레시피 목록에 버터/크림을 재료로 사용하는 레시피 추가 여부. 현재 베이커리 레시피는 밀가루/잼 기반이나, 버터 쿠키·크림 케이크 등의 추가로 가공 체인 깊이 확장 가능.

---

### 3.7 채집물 가공 레시피 (13종)

채집 아이템(-> see `docs/systems/gathering-system.md` 섹션 3)을 원재료로 사용하는 가공 레시피. BAL-016(2026-04-07) 전체 채집물 판매가 40% 하향 이후, 직판 대비 가공 부가가치가 과도해지지 않도록 판매가를 설계한다.

**채집물 가공 판매가 설계 기준**:
- Common/Uncommon 원재료: 원재료 총 직판가 대비 1.3~1.7배
- Rare 원재료: 원재료 총 직판가 대비 1.5~2.0배
- Legendary 원재료: 원재료 총 직판가 대비 2.5~4.0배 (희소성 프리미엄)
- 가공 시간은 기존 레시피 패턴을 참조하되, 채집물의 낮은 원가를 감안하여 짧게 설정

**원재료 판매가**: 모든 원재료 기본 판매가는 (-> see `docs/systems/gathering-system.md` 섹션 3.3~3.7).

#### 3.7.1 가공소 (일반) — 채집물 레시피 (9종)

| 레시피 ID | 재료 | 수량 | 결과물 | 결과 영문 ID | 판매 가격 | 가공 시간 | 해금 조건 |
|----------|------|------|--------|-------------|----------|----------|----------|
| `recipe_gather01` | 산딸기 (`gather_wild_berry`) | x5 | 야생 베리잼 | `item_wild_berry_jam` | 26G | 1시간 | 가공소 건설 + 채집 숙련도 Lv.1 |
| `recipe_gather02` | 도토리 (`gather_acorn`) | x5 | 도토리묵 | `item_acorn_jelly` | 20G | 1시간 | 가공소 건설 + 채집 숙련도 Lv.1 |
| `recipe_gather03` | 능이버섯 (`gather_neungi`) | x3 | 건조 버섯 (능이) | `item_dried_neungi` | 24G | 2시간 | 가공소 건설 + 채집 숙련도 Lv.2 |
| `recipe_gather04` | 표고버섯 (야생) (`gather_wild_shiitake`) | x2 | 건조 버섯 (표고) | `item_dried_wild_shiitake` | 36G | 2시간 | 가공소 건설 + 채집 숙련도 Lv.2 |
| `recipe_gather05` | 영지버섯 (`gather_reishi`) | x2 | 건조 영지 | `item_dried_reishi` | 84G | 2시간 | 가공소 건설 + 채집 숙련도 Lv.4 |
| `recipe_gather06` | 황금 연꽃 (`gather_golden_lotus`) | x1 | 황금 연꽃차 | `item_golden_lotus_tea` | 300G | 2시간 | 가공소 건설 + 채집 숙련도 Lv.7 |
| `recipe_gather07` | 천년 영지 (`gather_millennium_reishi`) | x1 | 천년 영지차 | `item_millennium_reishi_tea` | 360G | 3시간 | 가공소 건설 + 채집 숙련도 Lv.7 |
| `recipe_gather08` | 겨울 나무껍질 (`gather_winter_bark`) | x5 | 나무껍질 차 | `item_bark_tea` | 15G | 30분 | 가공소 건설 + 채집 숙련도 Lv.1 |
| `recipe_gather09` | 동충하초 (`gather_cordyceps`) | x2 | 동충하초 환 | `item_cordyceps_pill` | 140G | 2시간 | 가공소 건설 + 채집 숙련도 Lv.5 |

**판매가 근거 (원재료 총 직판가 대비 배수)**:

| 레시피 | 원재료 총 직판가 | 판매가 | 배수 | 설계 의도 |
|--------|:---------------:|:------:|:----:|-----------|
| 야생 베리잼 | 20G (4G x5) | 26G | 1.30x | Common 대량투입, 최저 부가가치 |
| 도토리묵 | 15G (3G x5) | 20G | 1.33x | Common 대량투입, 가을 기본 가공 |
| 건조 버섯 (능이) | 18G (6G x3) | 24G | 1.33x | Common 버섯 가공 기초 |
| 건조 버섯 (표고) | 24G (12G x2) | 36G | 1.50x | Uncommon 버섯, 소량 투입으로 효율 상승 |
| 건조 영지 | 48G (24G x2) | 84G | 1.75x | Rare 버섯 별도 가공품 차별화 |
| 황금 연꽃차 | 100G (100G x1) | 300G | 3.00x | Legendary, 높은 희소성 프리미엄 |
| 천년 영지차 | 120G (120G x1) | 360G | 3.00x | Legendary, 최고 부가가치 |
| 나무껍질 차 | 10G (2G x5) | 15G | 1.50x | 겨울 소소한 가공 콘텐츠 |
| 동충하초 환 | 80G (40G x2) | 140G | 1.75x | 겨울 Rare, Snow 조건의 보상 |

**설계 의도**: 채집물 가공 레시피는 기존 작물 가공(잼 x2.0, 주스 x2.5) 대비 배수가 낮다(1.3~1.75x). 이는 의도적이다. 채집 활동은 에너지 소모 없이 이동만으로 획득하므로, 가공 수익도 작물 가공보다 낮아야 "경작 > 채집" 우선순위가 유지된다. Legendary 아이템만 예외적으로 3.0x의 높은 배수를 적용하여, 극히 낮은 등장 확률(0.5~1%)에 대한 보상을 보장한다.

#### 3.7.2 발효실 — 채집물 레시피 (2종)

| 레시피 ID | 재료 | 수량 | 결과물 | 결과 영문 ID | 판매 가격 | 가공 시간 | 해금 조건 |
|----------|------|------|--------|-------------|----------|----------|----------|
| `recipe_gather10` | 머루 (`gather_wild_grape`) | x5 | 머루 와인 | `item_wild_grape_wine` | 90G | 48시간 | 발효실 건설 + 채집 숙련도 Lv.3 |
| `recipe_gather11` | 산삼 (`gather_wild_ginseng`) | x1 | 산삼주 | `item_wild_ginseng_wine` | 280G | 72시간 | 발효실 건설 + 채집 숙련도 Lv.5 |

**판매가 근거**:

| 레시피 | 원재료 총 직판가 | 판매가 | 배수 | 설계 의도 |
|--------|:---------------:|:------:|:----:|-----------|
| 머루 와인 | 45G (9G x5) | 90G | 2.00x | Uncommon 열매, 48시간 발효 보상. 기존 발효 패턴(3.0x)보다 낮지만 원재료가 채집물이므로 절제 |
| 산삼주 | 80G (80G x1) | 280G | 3.50x | Legendary + 72시간(3일) 발효. 극희소 원재료의 장기 투자 보상 |

**설계 의도**: 머루 와인은 가을 채집 콘텐츠의 핵심 목표 아이템이다. 48시간(게임 내 2일) 발효로 기존 와인(24시간)보다 길지만, 원재료가 채집물이므로 배수를 2.0x로 절제한다. 산삼주는 봄 Legendary 아이템의 최종 가공 경로로, 72시간(3일) 발효라는 게임 내 최장 가공 시간을 부여하여 "장기 투자의 결실"을 체감하게 한다.

#### 3.7.3 베이커리 — 채집물 레시피 (2종)

| 레시피 ID | 재료 | 결과물 | 결과 영문 ID | 판매 가격 | 가공 시간 | 연료 | 해금 조건 |
|----------|------|--------|-------------|----------|----------|------|----------|
| `recipe_gather12` | 달래 (`gather_wild_garlic`) x2 + 봄나물 (`gather_spring_herb`) x1 | 봄나물 비빔밥 | `item_spring_herb_bibimbap` | 60G | 30분 | 장작 1 | 베이커리 건설 + 채집 숙련도 Lv.2 |
| `recipe_gather13` | 송이버섯 (`gather_pine_mushroom`) x1 | 송이 구이 | `item_grilled_pine_mushroom` | 70G | 30분 | 장작 1 | 베이커리 건설 + 채집 숙련도 Lv.4 |

**판매가 근거**:

| 레시피 | 원재료 총 직판가 | 판매가 | 배수 | 설계 의도 |
|--------|:---------------:|:------:|:----:|-----------|
| 봄나물 비빔밥 | 18G (달래 5Gx2 + 봄나물 8Gx1) | 60G | 3.33x | 봄 채집 복합 재료 요리. 연료 30G 차감 후 직판 대비 +12G(+67%) 이득. DES-019 조정 |
| 송이 구이 | 32G (32G x1) | 70G | 2.19x | Rare 버섯의 간편 고급 요리. 연료 30G 차감 후 직판 대비 +8G(+25%) 이득. DES-019 조정 |

**설계 의도**: 베이커리 채집물 레시피는 기존 베이커리 레시피(2.0~2.5x)보다 낮은 배수를 적용하되, 연료비(30G) 차감 후에도 직판 대비 소폭 이득(+8~12G)이 발생하도록 설정한다. 두 레시피는 경제적 부가가치(+8~12G)와 더불어 NPC 선물/계절 퀘스트 납품 목적으로도 제작 가치가 있다. 봄나물 비빔밥은 봄 채집 경험의 마무리 요리로, 송이 구이는 가을 Rare 송이버섯의 소비 경로로 포지셔닝한다. (DES-019 확정)

~~[OPEN] 봄나물 비빔밥과 송이 구이의 연료 비용(장작 30G)을 감안하면 실질 수익이 매우 낮다.~~ **[DES-019 해결됨 -- 2026-04-07]** 판매가 상향(비빔밥 30G->60G, 송이 구이 55G->70G)으로 연료비 차감 후 직판 대비 +12G/+8G 이득 확보. NPC 선물/퀘스트 납품 포지셔닝 병행.

#### 3.7.4 가공소 (일반) — 광석 제련 레시피 (1종) (DES-020)

| 레시피 ID | 재료 | 수량 | 결과물 | 결과 영문 ID | 가공 시간 | 해금 조건 |
|----------|------|------|--------|-------------|----------|----------|
| `recipe_smelt_iron` | 철 광석 (`gather_iron_ore`) | x3 | 철 조각 (Iron Scrap) | `iron_scrap` | 2시간 | 가공소 건설 + 채집 숙련도 Lv.2 |

**설계 결정 (DES-020)**:
- 철 광석 3개를 가공소에서 제련하여 철 조각 1개를 생산한다.
- 결과물 `iron_scrap`은 대장간 상점에서 판매하는 것과 **동일한 아이템**이다. 도구 업그레이드 재료로 그대로 사용 가능.
- 이 레시피는 **판매용이 아닌 재료 변환용**이므로 판매 가격 열을 별도로 기재하지 않는다. 철 조각의 판매 가격은 대장간 시스템에서 정의한다.

**경제성 분석 (단계별)**:
1. 철 광석 직판 수입: 12G x 3 = **36G** (→ see `docs/systems/gathering-system.md` 섹션 3.7)
2. 철 조각 상점 구매가: **100G/개** (→ see `docs/systems/tool-upgrade.md` 섹션 6.3)
3. 제련 시 기회비용: 철 광석 직판 포기 36G + 가공 시간 2시간
4. 제련 시 순절감액: 100G(상점 구매 회피) - 36G(직판 포기) = **64G/개**
5. Reinforced 업그레이드 전체 재료(철 조각 x3) 제련 시: 64G x 3 = **192G 절감**
   - 단, 철 광석 9개 필요. 획득률 ~0.12개/일 기준 약 75일 소요 (→ see `docs/systems/gathering-system.md` 섹션 3.8)
6. **결론**: 제련은 "장기간 채집 + 가공 시간 투자"로 "골드 지출"을 대체하는 보조 경로. 상점 구매(즉시, 100G)와의 트레이드오프가 명확하다.

**옵션 B(완전 대체) 불채택 근거**: 도구 업그레이드 재료를 채집 광석으로 완전 대체하면, (1) 대장간 상점의 철 조각 판매가 무의미해지고, (2) 업그레이드 속도가 채집 RNG에 전적으로 의존하여 ~75일이라는 과도한 지연이 발생하며, (3) "골드 투자 vs 시간 투자" 선택지가 사라진다.

**옵션 A(현행 유지) 불채택 근거**: 철 광석 힌트 텍스트("녹여서 다듬으면 좋은 도구의 재료가 될 수 있다")가 플레이어에게 도구 재료 용도를 시사하는데 실제로 불가능하면 기대 불일치가 발생한다. 또한 채집 광물 6종 중 구리 광석(낚싯대), 금 광석(낚싯대), 수정 원석(채집 낫)은 업그레이드 재료로 쓰이는데 철 광석만 용도가 없으면 아이템 설계 일관성이 깨진다.

---

### 3.8 전체 레시피 요약 (75종)

| # | 가공소 | 레시피 ID | 결과물 | 판매 가격 |
|---|--------|----------|--------|----------|
| 1 | 가공소 | `recipe_jam_potato` | 감자 잼 | 110G |
| 2 | 가공소 | `recipe_jam_carrot` | 당근 잼 | 120G |
| 3 | 가공소 | `recipe_jam_tomato` | 토마토 잼 | 170G |
| 4 | 가공소 | `recipe_jam_corn` | 옥수수 잼 | 250G |
| 5 | 가공소 | `recipe_jam_strawberry` | 딸기 잼 | 210G |
| 6 | 가공소 | `recipe_jam_pumpkin` | 호박 잼 | 450G |
| 7 | 가공소 | `recipe_jam_watermelon` | 수박 잼 | 750G |
| 8 | 가공소 | `recipe_juice_tomato` | 토마토 주스 | 180G |
| 9 | 가공소 | `recipe_juice_strawberry` | 딸기 주스 | 230G |
| 10 | 가공소 | `recipe_juice_watermelon` | 수박 주스 | 905G |
| 11 | 가공소 | `recipe_pickle_potato` | 감자 절임 | 90G |
| 12 | 가공소 | `recipe_pickle_carrot` | 당근 절임 | 100G |
| 13 | 가공소 | `recipe_pickle_tomato` | 토마토 절임 | 150G |
| 14 | 가공소 | `recipe_pickle_corn` | 옥수수 절임 | 230G |
| 15 | 가공소 | `recipe_pickle_pumpkin` | 호박 절임 | 430G |
| 16 | 가공소 | `recipe_pickle_winter_radish` | 겨울무 절임 | 120G |
| 17 | 가공소 | `recipe_jam_shiitake` | 표고버섯 잼 | 190G |
| 18 | 가공소 | `recipe_pickle_spinach` | 시금치 절임 | 290G |
| 19 | 제분소 | `recipe_mill_corn_flour` | 옥수수 가루 | 170G |
| 20 | 제분소 | `recipe_mill_potato_starch` | 감자 전분 | 85G |
| 21 | 제분소 | `recipe_mill_pumpkin_powder` | 호박 분말 | 320G |
| 22 | 제분소 | `recipe_mill_radish_powder` | 무 분말 | 87G |
| 23 | 발효실 | `recipe_ferm_strawberry_wine` | 딸기 와인 | 320G |
| 24 | 발효실 | `recipe_ferm_watermelon_wine` | 수박 와인 | 1,130G |
| 25 | 발효실 | `recipe_ferm_tomato_vinegar` | 토마토 식초 | 260G |
| 26 | 발효실 | `recipe_ferm_pumpkin_pickle` | 호박 장아찌 | 680G |
| 27 | 발효실 | `recipe_ferm_spinach_kimchi` | 시금치 겉절이 | 470G |
| 28 | 베이커리 | `recipe_bake_corn_bread` | 옥수수 빵 | 350G |
| 29 | 베이커리 | `recipe_bake_pumpkin_pie` | 호박 파이 | 1,200G |
| 30 | 베이커리 | `recipe_bake_strawberry_cake` | 딸기 케이크 | 680G |
| 31 | 베이커리 | `recipe_bake_veggie_cookie` | 채소 쿠키 x3 | 360G (120G x3) |
| 32 | 베이커리 | `recipe_bake_royal_tart` | 로열 타르트 | 2,100G |
| 33 | 가공소 | `recipe_grilled_fish` | 구운 생선 | basePrice x1.8 + 20G |
| 34 | 가공소 | `recipe_smoked_fish` | 훈제 생선 | basePrice x2.2 + 40G |
| 35 | 베이커리 | `recipe_fish_sushi` | 생선 초밥 | basePrice x2.5 + 60G |
| 36 | 가공소 | `recipe_fish_stew` | 생선 스튜 | 180G |
| 37 | 베이커리 | `recipe_fish_pie` | 생선 파이 | 250G |
| 38 | 치즈 공방 | `recipe_cheese_basic` | 치즈 | 250G |
| 39 | 치즈 공방 | `recipe_cheese_goat` | 염소 치즈 | 190G |
| 40 | 치즈 공방 | `recipe_butter` | 버터 | 160G |
| 41 | 치즈 공방 | `recipe_cheese_aged` | 에이지드 치즈 | 680G |
| 42 | 치즈 공방 | `recipe_cream` | 크림 | 280G |
| 43 | 가공소 | `recipe_gather01` | 야생 베리잼 | 26G |
| 44 | 가공소 | `recipe_gather02` | 도토리묵 | 20G |
| 45 | 가공소 | `recipe_gather03` | 건조 버섯 (능이) | 24G |
| 46 | 가공소 | `recipe_gather04` | 건조 버섯 (표고) | 36G |
| 47 | 가공소 | `recipe_gather05` | 건조 영지 | 84G |
| 48 | 가공소 | `recipe_gather06` | 황금 연꽃차 | 300G |
| 49 | 가공소 | `recipe_gather07` | 천년 영지차 | 360G |
| 50 | 가공소 | `recipe_gather08` | 나무껍질 차 | 15G |
| 51 | 가공소 | `recipe_gather09` | 동충하초 환 | 140G |
| 52 | 발효실 | `recipe_gather10` | 머루 와인 | 90G |
| 53 | 발효실 | `recipe_gather11` | 산삼주 | 280G |
| 54 | 베이커리 | `recipe_gather12` | 봄나물 비빔밥 | 60G |
| 55 | 베이커리 | `recipe_gather13` | 송이 구이 | 70G |
| 56 | 가공소 | `recipe_smelt_iron` | 철 조각 (제련) | — (재료 변환용) |
| 57 | 가공소 | `recipe_food_common_roasted_corn` | 구운 옥수수 | 80G |
| 58 | 가공소 | `recipe_food_common_potato_soup` | 감자 수프 | 50G |
| 59 | 가공소 | `recipe_food_common_tomato_salad` | 토마토 샐러드 | 55G |
| 60 | 가공소 | `recipe_food_common_grilled_fish` | 구운 생선 정식 | 60G |
| 61 | 가공소 | `recipe_food_common_carrot_stew` | 당근 스튜 | 55G |
| 62 | 가공소 | `recipe_food_common_mushroom_soup` | 버섯 수프 | 40G |
| 63 | 가공소 | `recipe_food_common_corn_porridge` | 옥수수 죽 | 75G |
| 64 | 가공소 | `recipe_food_common_herb_tea` | 약초 차 | 20G |
| 65 | 베이커리 | `recipe_food_advanced_pumpkin_stew` | 호박 스튜 | 350G |
| 66 | 베이커리 | `recipe_food_advanced_strawberry_jam_toast` | 딸기 잼 토스트 | 320G |
| 67 | 베이커리 | `recipe_food_advanced_fish_stew_deluxe` | 특제 생선 스튜 | 380G |
| 68 | 베이커리 | `recipe_food_advanced_spring_bibimbap` | 달걀 봄나물 비빔밥 | 300G |
| 69 | 베이커리 | `recipe_food_advanced_autumn_mushroom` | 가을 버섯 요리 | 340G |
| 70 | 베이커리 | `recipe_food_advanced_watermelon_sorbet` | 수박 셔벗 | 480G |
| 71 | 베이커리 | `recipe_food_advanced_cheese_gratin` | 치즈 그라탱 | 420G |
| 72 | 베이커리 | `recipe_food_premium_lotus_feast` | 황금 연꽃 만찬 | 900G |
| 73 | 베이커리 | `recipe_food_premium_millennium_soup` | 천년 영지 보양식 | 850G |
| 74 | 베이커리 | `recipe_food_premium_royal_harvest` | 왕실 수확 연회 | 1,200G |
| 75 | 베이커리 | `recipe_food_premium_ginseng_elixir` | 산삼 강정 | 1,100G |

---

## 4. 연료 시스템

### 4.1 연료가 필요한 가공소

| 가공소 | 연료 필요 여부 |
|--------|--------------|
| 가공소 (일반) | 아니오 |
| 제분소 | 아니오 |
| 발효실 | 아니오 |
| 베이커리 | **예** |
| 치즈 공방 | 아니오 |

**설계 의도**: 연료 시스템은 베이커리에만 적용한다. 가공소(일반)와 제분소, 발효실, 치즈 공방은 연료 없이 운영하여 진입 장벽을 낮추고, 최상위 시설인 베이커리만 추가 비용 요소를 가져 엔드게임 복잡도를 높인다.

### 4.2 연료 종류

| 연료 | 영문 ID | 획득 방법 | 구매가 | 연소 횟수 |
|------|---------|----------|--------|----------|
| 장작 (Firewood) | `item_firewood` | 잡화 상점 구매 | 30G | 레시피 1회 소모 |

**장작**은 베이커리의 유일한 연료이다. 잡화 상점(하나)에서 구매하며, 레시피 1건 가동 시 1~2개를 소모한다 (각 레시피의 연료 소모량은 섹션 3.4 레시피 테이블 참조).

### 4.3 연료 소모 규칙

| 규칙 | 내용 |
|------|------|
| 소모 시점 | 가공 **시작** 시 즉시 소모 (중단해도 환불 없음) |
| 연료 부족 시 | 가공 시작 불가. UI에 "장작 부족" 경고 표시 |
| 인벤토리 자동 사용 | 플레이어 인벤토리에서 자동으로 장작을 차감 |
| 가공 중단 | 가공 중 장작을 추가로 소모하지 않음 (시작 시 1회 소모) |

**연료 비용이 수익에 미치는 영향 예시**:
- 옥수수 빵: 판매 350G - 재료(옥수수 가루 기회비용 170G) - 장작 30G = 순이익 150G
- 로열 타르트: 판매 2,100G - 재료(기회비용 1,230G) - 장작 60G = 순이익 810G

---

## 5. 배치/가동 규칙

### 5.1 동시 가동 제한

각 가공소는 **슬롯 수만큼** 동시에 레시피를 가동할 수 있다.

| 가공소 | 초기 슬롯 | 최대 슬롯 | 슬롯 확장 |
|--------|----------|----------|----------|
| 가공소 (일반) | 1 | 3 | (-> see `docs/systems/economy-system.md` 섹션 2.5) |
| 제분소 | 1 | 1 | 확장 불가 |
| 발효실 | 2 | 2 | 확장 불가 |
| 베이커리 | 2 | 2 | 확장 불가 |

**설계 의도**: 가공소(일반)만 슬롯 확장이 가능하다. 특화 가공소 4종은 슬롯이 고정되어, "어떤 레시피를 슬롯에 넣을지"의 선택을 강제한다. 발효실과 베이커리는 2슬롯으로 시작하여 동시에 2종의 레시피를 진행할 수 있지만, 24시간짜리 와인과 12시간짜리 식초 중 어느 쪽에 슬롯을 할당할지가 전략적 판단이다.

### 5.2 작업 큐 규칙

| 파라미터 | 값 |
|----------|-----|
| 작업 큐 최대 크기 | 슬롯당 3건 대기 가능 |
| 큐 동작 방식 | 현재 가공 완료 후 자동으로 다음 큐 항목 시작 |
| 큐 취소 | 대기 중인 큐 항목은 취소 가능 (재료 반환). 진행 중인 가공은 취소 불가 |
| 큐 재료 선소모 | 큐에 등록할 때 재료를 **즉시 차감** (등록 시점에 인벤토리에서 제거) |
| 큐 취소 시 환불 | 대기 상태의 큐 항목 취소 시 재료 100% 반환. 연료는 진행 중인 건만 소모되었으므로 대기 건의 연료도 반환 |

**큐 총 용량 예시**:
- 가공소(일반) Lv.1, 슬롯 1개: 진행 중 1건 + 대기 3건 = 총 4건
- 가공소(일반) 최대(슬롯 3개): 진행 중 3건 + 대기 9건 = 총 12건
- 발효실(슬롯 2개): 진행 중 2건 + 대기 6건 = 총 8건

### 5.3 자동 투입 vs 수동 투입

| 모드 | 설명 |
|------|------|
| **수동 투입** (기본) | 플레이어가 직접 가공 UI를 열어 재료를 투입하고 레시피를 선택 |
| **자동 수거** | 가공 완료된 결과물은 가공소 내부에 보관. 플레이어가 E키로 수거해야 인벤토리에 들어옴 |

**자동 투입은 지원하지 않는다**. 가공은 "어떤 작물을 어떤 가공품으로 만들 것인가"의 의사결정이 핵심이므로, 자동화하면 이 선택의 의미가 사라진다.

**결과물 보관 제한**:

| 파라미터 | 값 |
|----------|-----|
| 가공소 내부 보관 슬롯 | 슬롯당 1개 (완성품이 슬롯에 남아있는 상태) |
| 보관 초과 시 | 결과물이 슬롯에 남아있으면 다음 큐 항목이 시작되지 않음. 수거 필요. |

**설계 의도**: 플레이어가 주기적으로 가공소를 방문하여 완성품을 수거하도록 유도한다. "심어놓고 잊어버리는" 패턴을 방지하여 농장 관리의 능동적 참여를 유지.

### 5.4 가공 시간과 게임 시간의 관계

가공 시간은 **게임 내 시간** 기준이다 (-> see `docs/systems/time-season.md`).

```
게임 내 1시간 = 실시간 25초 (1일 = 10분 = 24시간 기준)
```

| 가공 시간 | 실시간 환산 |
|----------|-----------|
| 2시간 | 약 50초 |
| 3시간 | 약 75초 |
| 4시간 | 약 100초 (1분 40초) |
| 6시간 | 약 150초 (2분 30초) |
| 12시간 | 약 300초 (5분) |
| 18시간 | 약 450초 (7분 30초) |
| 24시간 | 약 600초 (10분 = 게임 내 1일) |

**가공 시간 단축**: 가공소(일반)는 업그레이드를 통해 가공 시간을 단축할 수 있다 (-> see `docs/content/facilities.md` 섹션 6.6). 특화 가공소 4종은 업그레이드 시스템이 없으며 가공 시간이 고정이다.

[OPEN] 특화 가공소(제분소, 발효실, 베이커리)에도 업그레이드 경로를 추가할지 검토. 현재 설계에서는 고정 시간으로 단순화했으나, 게임 후반의 투자 목표가 부족하다면 업그레이드를 추가할 수 있다.

---

## 6. 가공품 퀄리티

### 6.1 기본 규칙

가공품의 판매가는 **원재료의 품질(퀄리티)에 영향받지 않는다** (-> see `docs/systems/economy-system.md` 섹션 2.5). 가공 과정에서 품질 차이가 사라진다.

| 원재료 품질 | 가공품 품질 | 가공품 판매가 |
|-----------|-----------|-------------|
| Normal | Normal | 기본 가공품 가격 (이 문서 섹션 3 참조) |
| Silver | Normal | 기본 가공품 가격 |
| Gold | Normal | 기본 가공품 가격 |
| Iridium | Normal | 기본 가공품 가격 |

### 6.2 품질과 판매 전략

이 규칙으로 인해 다음 트레이드오프가 발생한다:

- **Normal 품질 작물**: 가공이 거의 항상 유리 (직판가가 낮으므로)
- **Silver/Gold 품질 작물**: 작물에 따라 직판 vs 가공 비교 필요
- **Iridium 품질 작물**: 직판이 유리할 수 있음 (품질 배수 x2.0)

**품질별 직판가 vs 가공 비교** (예: 수박, 품질 배수 -> see `docs/systems/crop-growth.md` 섹션 4.3):

| 품질 | 수박 직판가 | 수박 주스 | 수박 잼 | 최적 행동 |
|------|-----------|----------|---------|----------|
| Normal (x1.0) | 350G | 905G | 750G | 가공 (주스) |
| Silver (x1.25) | 437G | 905G | 750G | 가공 (주스) |
| Gold (x1.5) | 525G | 905G | 750G | 가공 (주스) |
| Iridium (x2.0) | 700G | 905G | 750G | 가공 (주스, 소폭 유리) |

**예: 감자의 경우**:

| 품질 | 감자 직판가 | 감자 잼 | 감자 절임 | 최적 행동 |
|------|-----------|---------|----------|----------|
| Normal (x1.0) | 30G | 110G | 90G | 가공 (잼) |
| Silver (x1.25) | 37G | 110G | 90G | 가공 (잼) |
| Gold (x1.5) | 45G | 110G | 90G | 가공 (잼) |
| Iridium (x2.0) | 60G | 110G | 90G | 가공 (잼) |

[RISK] 현재 가공 배수(x2.0~x3.0 + 고정 보너스)가 Iridium 품질 배수(x2.0)보다도 높아, **모든 품질에서 가공이 직판보다 유리**하다. 이는 "가공 vs 직판" 트레이드오프를 사실상 무효화한다. 해결 방안 후보: (a) 가공 배수를 x1.5~x2.0으로 하향, (b) 가공 시간을 더 늘려 기회비용 증가, (c) 가공품에도 수급 변동 적용. -> BAL-004에서 정밀 분석 후 확정. (-> see `docs/content/facilities.md` 섹션 6.5의 동일 RISK 항목)

---

## 7. design.md 섹션 4.6 확장 제안

현재 `docs/design.md` 섹션 4.6 시설 목록에는 가공소 1종만 존재한다. 특화 가공소 4종을 추가하려면 design.md 업데이트가 필요하다.

**확장 제안**:

| 시설 | 비용 | 기능 | 해금 |
|------|------|------|------|
| 물탱크 | 500G | 물주기 자동화 (인접 경작지) | 레벨 3 |
| 온실 | 2000G | 계절 무관 재배 | 레벨 5 |
| **제분소** | **1,500G** | **곡물 -> 가루** | **레벨 5** |
| 창고 | 1000G | 작물 저장 (가격 변동 대응) | 레벨 4 |
| 가공소 | 3000G | 작물 -> 가공품 | 레벨 7 |
| **발효실** | **4,000G** | **작물 -> 발효품** | **레벨 8** |
| **베이커리** | **5,000G** | **가공품 -> 고급 요리** | **레벨 9** |

[OPEN] design.md 섹션 4.6에 특화 가공소 4종을 반영해야 한다. 이 업데이트는 본 문서 승인 후 별도 작업으로 수행한다.

---

## 8. Cross-references

| 참조 문서 | 관련 내용 |
|----------|----------|
| `docs/design.md` 섹션 4.2 | 작물 기본 판매가, 성장일수 (가공품 가격 산출 기반) |
| `docs/design.md` 섹션 4.6 | 시설 목록 (가공소 건설 비용, 해금 레벨) |
| `docs/systems/economy-system.md` 섹션 2.5 | 가공 공식, 가공 유형별 배수, 가공소 슬롯 |
| `docs/content/facilities.md` 섹션 6 | 가공소(일반) 상세 메카닉, 업그레이드, 품질 관계 |
| `docs/content/crops.md` 섹션 3 | 전체 작물 카탈로그, 겨울 전용 작물 수치 |
| `docs/balance/crop-economy.md` | 작물 경제 밸런스 (직판 ROI 참조) |
| `docs/systems/crop-growth.md` 섹션 4.3~4.4 | 품질 등급, 가격 배수 |
| `docs/systems/time-season.md` | 게임 내 시간 정의 (가공 시간 환산 기반) |
| `docs/content/npcs.md` 섹션 3 | 하나(잡화 상점) — 장작 판매처 |
| `docs/balance/progression-curve.md` | 레벨별 해금 (가공소 해금 타이밍) |
| `docs/systems/fishing-system.md` 섹션 4.2 | 어종 목록, basePrice (생선 가공 레시피 원재료) |
| `docs/balance/fishing-economy.md` 섹션 4 | 생선 가공 ROI 밸런스 분석 |
| `docs/systems/gathering-system.md` 섹션 3.3~3.7 | 채집 아이템 기본 판매가 (채집물 가공 레시피 원재료) |
| `docs/content/gathering-items.md` 섹션 9 | 채집물별 가공 연계 방향, NPC 선물 적합도 |
| `docs/balance/gathering-economy.md` 섹션 4 | 채집물 가공 ROI 밸런스 분석 |
| `docs/systems/tool-upgrade.md` 섹션 2.2, 6.3 | 철 조각 재료 획득 경로, 대장간 판매가 (DES-020) |
| `docs/content/food-items.md` | 음식 아이템 canonical — 등급, 회복량, 특수 효과, 획득 경로, 판매가 (CON-022) |

**후속 문서 (본 문서가 canonical 선행)**:
- BAL-004: 가공품 ROI 밸런스 분석 (56종 레시피 수익성, 가공 체인 효율, 시간 대비 수익 — 채집물 13종 + 광석 제련 1종 포함)

---

## 9. Open Questions

1. **[OPEN]** 특화 가공소 4종(제분소, 발효실, 베이커리)을 `docs/design.md` 섹션 4.6에 반영하는 작업이 필요하다. 현재 design.md에는 가공소 1종만 기재되어 있다.

2. **[OPEN]** 특화 가공소에 업그레이드 경로를 추가할지 검토. 현재는 가공소(일반)만 Lv.1~3 업그레이드를 지원하고, 특화 가공소는 고정 스펙이다. 게임 후반 투자 목표 부족 시 추가를 검토한다.

3. **[OPEN]** 치즈 공방(Cheese Workshop)과 유제품 레시피는 현재 설계에 **동물/목축 시스템이 없어** 도입 불가. 동물 시스템 추가 시 치즈, 버터, 크림 등 유제품 레시피를 포함하는 치즈 공방을 확장 후보로 둔다.

4. **[OPEN]** 장작(Firewood) 외에 추가 연료 종류(석탄, 고급 장작 등)를 도입할지 검토. 현재는 단일 연료로 단순화.

5. **[OPEN]** 가공품에도 수급 변동(Dynamic Pricing)을 적용할지 검토 (-> see `docs/systems/economy-system.md` 섹션 2.6). 적용 시 대량 가공 출하의 가격 하락이 발생하여 "다양한 가공품을 만드는" 전략적 동기가 강화된다.

6. **[OPEN]** 베이커리 레시피의 "채소 쿠키"가 3개씩 생산되는 설계가 다른 레시피(1개 생산)와 일관성이 있는지 검토. 수량 조정으로 저가 레시피의 효율을 다르게 튜닝하려는 의도이나, 인벤토리 관리 복잡도가 증가할 수 있다.

---

## 10. Risks

1. **[RISK]** 가공이 모든 품질 등급에서 직판보다 유리한 문제. 가공 배수가 Iridium(x2.0)보다 높아 "가공 vs 직판" 트레이드오프가 사실상 없다. BAL-004에서 배수 조정 검토 필수. (-> see `docs/content/facilities.md` 섹션 6.5 동일 RISK)

2. **[RISK]** 가공 체인이 너무 복잡해질 경우(제분소 -> 베이커리 3단계), 신규 플레이어의 인지 부하가 높아질 수 있다. "점진적 복잡도(incremental complexity)" 원칙에 따라, 제분소(레벨 5) -> 가공소(레벨 7) -> 발효실(레벨 8) -> 베이커리(레벨 9)의 해금 순서로 한 번에 하나씩 학습하도록 설계했으나, 실제 플레이테스트에서 검증 필요.

3. **[RISK]** 수박 와인(1,130G)과 로열 타르트(2,100G)의 가격이 지나치게 높아 엔드게임 경제를 불안정하게 만들 수 있다. 수급 변동 시스템이 가공품에도 적용되지 않으면, 수박 와인만 대량 생산하는 지배 전략이 출현할 수 있다.

4. **[RISK]** 특화 가공소 4종의 건설 비용 합계(1,500G + 4,000G + 5,000G = 10,500G)가 기존 가공소 풀 업그레이드(18,500G)와 합치면 총 29,000G. 게임 전체 골드 유통량 대비 이 투자가 합리적인지 BAL-004에서 검증 필요.

5. **[RISK]** 베이커리의 연료 비용(장작 30G/개)이 고가 레시피 대비 너무 저렴하여 실질적 제약이 되지 않을 수 있다. 연료 가격 상향 또는 고급 연료 도입을 BAL-004에서 함께 분석.

---

# 여행 상인 NPC 상세 문서 (Traveling Merchant NPC Detail Specification)

> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: CON-008c | Phase 1

---

## 1. Context

이 문서는 여행 상인 NPC "바람이(Barami)"의 캐릭터 심화 설계, 전체 대화 스크립트, 독점 아이템 시스템, 계절별 방문 이벤트를 상세히 기술한다. `docs/content/npcs.md` 섹션 6에 정의된 기본 설정을 확장하며, 경제 시스템(`docs/systems/economy-system.md`)과 연동하여 골드 싱크 및 희소 자원 공급 역할을 수행한다.

**설계 목표**: 바람이는 게임에 "이벤트성 긴장감"을 부여하는 NPC로, 신비롭고 유쾌한 성격을 통해 "특별한 만남"의 느낌을 준다. 정기적 방문이지만 매번 다른 아이템을 가져와 "이번에는 뭘 가져왔을까"의 기대감을 형성한다. 여행 상인은 상시 상점에서 구할 수 없는 독점 아이템 경로이므로, 주말 방문 루틴을 자연스럽게 형성하며 골드 소비의 전략적 선택지를 제공한다.

### 1.1 본 문서가 canonical인 데이터

- 바람이 캐릭터 심화 설정 (배경 스토리, 성격 디테일, 관계 발전 단계)
- 여행 상인 전체 대화 스크립트 (상황별, 계절별, 친밀도별, 특수 이벤트별)
- 여행 일지 시스템 (방문 시 이야기 공유)
- 바람이의 관계 발전 단계 임계값 및 단계별 보상

### 1.2 본 문서가 canonical이 아닌 데이터 (참조만)

| 데이터 종류 | 참조처 |
|------------|--------|
| NPC 기본 정보 (이름, 나이, 외형 요약) | `docs/content/npcs.md` 섹션 6.1 |
| 등장 조건, 빈도, 등장 시간 | `docs/content/npcs.md` 섹션 6.2 |
| 판매 아이템 풀, 가격, 선정 규칙 | `docs/content/npcs.md` 섹션 6.3 |
| 만능 비료 상세 스펙 | `docs/content/npcs.md` 섹션 6.4 |
| 여행 상인 튜닝 파라미터 | `docs/content/npcs.md` 섹션 9.1~9.3 |
| 상점 공통 UI 흐름 | `docs/content/npcs.md` 섹션 8 |
| NPC 대화 시스템 구조 (트리거, 우선순위) | `docs/content/npcs.md` 섹션 7 |
| 경제 시스템 수급 보정 | `docs/systems/economy-system.md` 섹션 2 |

---

## 2. 캐릭터 설계

### 2.1 기본 정보

| 항목 | 내용 |
|------|------|
| 이름 | 바람이 (Barami) |
| 영문 ID | `npc_barami` |
| 나이 | 불명 (20대~40대로 보임) |
| 성별 | 불명 (중성적 외모) |
| 역할 | 여행 상인 (희귀 아이템 판매, 정보 제공) |
| 위치 | 마을 광장 (임시 좌판 -- 등장일에만) |

기본 캐릭터 설정은 (-> see `docs/content/npcs.md` 섹션 6.1)을 따른다.

### 2.2 외형 상세

로우폴리 3D 스타일에 맞춘 캐릭터 디자인.

| 요소 | 묘사 |
|------|------|
| 체형 | 가늘고 민첩한 체형. 여행자다운 날렵한 인상 |
| 피부 | 밝은 올리브색. 햇볕에 그을렸지만 건강한 광택 |
| 머리 | 중간 길이의 검은 머리, 바람에 흩날리는 스타일. 머리 한쪽에 깃털 장식 |
| 복장 | 색색의 천 패치워크 망토 (보라, 주황, 청록 등 여러 색상). 안에 가벼운 여행복 |
| 악세서리 | 반짝이는 금색 귀걸이 (한쪽만), 손목에 여러 개의 가죽 팔찌, 허리에 작은 주머니 여러 개 |
| 배낭 | 몸보다 큰 배낭. 다양한 물건이 매달려 있어 걸을 때마다 달각거린다 |
| 동반자 | 나귀 한 마리 ("구름이"). 등에 큰 상자 2개를 싣고 있다 |
| 대기 애니메이션 | 좌판 뒤에서 물건을 정리하거나, 나귀의 머리를 쓰다듬는 동작 반복 |
| 등장 연출 | 마을 입구에서 나귀와 함께 걸어 들어옴 -> 광장에 좌판 설치 (09:00) |
| 퇴장 연출 | 좌판을 접고 나귀에 짐을 싣고 마을 밖으로 걸어 나감 (17:00) |

**로우폴리 표현 가이드**: 패치워크 망토의 다채로운 색상이 원거리에서 즉시 "외부인"임을 알린다. 마을의 NPC들이 단색 위주인 것과 대비하여, 바람이만의 화려한 색감이 특별한 존재감을 준다. 큰 배낭과 나귀는 "여행자"의 실루엣을 완성한다.

### 2.3 성격 상세

| 측면 | 설명 |
|------|------|
| 말투 | 경쾌하고 유머러스한 반말. 감탄사와 느낌표를 많이 사용("헤이!", "와우!", "대박!"). 과장된 표현을 즐긴다 |
| 물건에 대한 태도 | 자기가 파는 물건에 대해 과장된 자부심. "세상에서 둘도 없는 물건"이라며 세일즈 화법을 구사하지만, 실제로 좋은 물건을 가져온다 |
| 플레이어에 대한 태도 | 처음부터 친근하게 다가간다. 관계가 발전하면 다른 마을의 재미있는 이야기를 들려준다 |
| 감정 표현 | 과장되고 연극적. 큰 몸짓과 함께 감정을 드러낸다. 하지만 진지한 순간에는 갑자기 차분해진다 |
| 숨은 면모 | 떠돌이 생활에 대한 외로움. 이 마을을 특별하게 생각한다. 정착하고 싶은 마음이 있지만, 자유를 포기하지 못한다 |

### 2.4 배경 스토리

바람이의 과거는 대부분 수수께끼에 싸여 있다. 본인이 말하는 이야기가 매번 조금씩 달라서, 어디까지가 진실인지 알 수 없다. 확실한 것은 여러 마을을 떠돌며 물건을 거래하는 방랑 상인이라는 것과, "구름이"라는 이름의 나귀가 유일한 동반자라는 것이다.

이 마을에는 1년 전쯤 처음 왔다고 한다(하지만 하나는 3년 전부터 봤다고 기억한다). 다른 마을에서 보기 힘든 희귀한 물건을 가져오는데, 어디서 조달하는지는 아무도 모른다. 본인은 "바람이 가르쳐 주는 대로 가면 좋은 물건이 있어"라고 말한다.

마을 사람들과의 관계는 독특하다. 철수는 바람이를 "수상한 놈"이라며 경계하지만, 바람이가 가져오는 희귀 금속을 은근히 탐낸다. 하나는 바람이의 유쾌한 성격을 좋아하며, 가끔 다른 마을의 씨앗 정보를 교환한다. 목이는 바람이가 올 때마다 다른 마을의 건축 양식에 대해 묻는다.

**게임 내 노출**: 배경 스토리는 친밀도 단계에 따라 "여행 일지"라는 형식으로 드러난다. 매 방문마다 짧은 여행 이야기를 들려주는데, 관계가 발전하면 이야기가 점점 개인적으로 바뀐다.

### 2.5 관계 발전 단계

플레이어와 바람이의 관계는 4단계로 발전한다. 단계 전환은 친밀도 포인트 누적(아이템 구매 +2, 방문 시 대화 +1)에 기반한다. 바람이는 주말에만 등장하므로 친밀도 상승 기회가 적어, 구매 시 +2로 보정한다.

| 단계 | 영문 키 | 친밀도 임계값 | 관계 묘사 |
|------|---------|-------------|-----------|
| 지나가는 손님 | `Stranger` | 0 (초기) | 유쾌한 세일즈 화법. 기본 판매 대사 |
| 얼굴을 아는 사이 | `Acquaintance` | 8 | 이름을 기억한다. 다른 마을의 가벼운 소문을 들려준다 |
| 단골 고객 | `Regular` | 20 | 아이템 추천을 해 준다. 여행 이야기가 깊어진다. **아이템 풀에서 +1개 추가 선택** |
| 바람의 친구 | `Friend` | 40 | 개인적인 이야기를 꺼낸다. 정착에 대한 고민을 털어놓음. **전 아이템 5% 할인 + 재고 +1개 추가** |

**임계값 canonical**: 이 테이블의 친밀도 임계값(`[0, 8, 20, 40]`)이 canonical이다. NPC 아키텍처의 `affinityThresholds`는 이 테이블을 참조한다.

**설계 의도**: 바람이는 주 2일만 등장하므로, 다른 NPC보다 친밀도 획득 기회가 적다. 이를 보정하기 위해 (1) 구매 시 +2 포인트, (2) 임계값을 다른 NPC(10/25/50)보다 낮게 설정(8/20/40)했다. Regular 보상으로 아이템 풀 +1은 더 다양한 쇼핑 경험을, Friend 보상으로 할인+재고 증가는 희소 자원 접근성 향상을 제공한다.

---

## 3. 대화 시스템 설계

### 3.1 최초 만남 대화

레벨 2에 도달하여 바람이가 처음 등장한 주말, 말을 걸 때 1회만 재생된다.

```
[바람이] "헤이! 이 마을에 새 농부가 있다는 소문 들었지~
          나는 바람이. 여기저기 떠돌면서 신기한 물건을 모으는 상인이야.
          비싼 편이지만, 다른 데선 못 구하는 물건이 있을 거야!
          매주 토요일, 일요일에 올 테니까 기다려~"
```

**설계 의도**: 4문장으로 (1) 존재 인지, (2) 자기소개, (3) 차별점 설명(비싸지만 독점), (4) 방문 주기 안내를 완료한다. 가격이 높다는 점을 미리 고지하여 기대치를 조절한다.

### 3.2 일반 방문 대화

등장일에 방문 시 랜덤으로 출력된다. 동일 대사 연속 재생 방지 규칙은 (-> see `docs/content/npcs.md` 섹션 7.4)을 따른다.

#### 범용 대사 (계절 무관, 5종)

| ID | 대사 |
|----|------|
| `barami_general_01` | "또 만났네! 이번 주에도 좋은 물건 가져왔어~" |
| `barami_general_02` | "아하, 기다리고 있었어? 구경해 봐!" |
| `barami_general_03` | "이번엔 특별한 게 있어. 한번 볼래?" |
| `barami_general_04` | "이 마을은 올 때마다 기분이 좋아져. 공기가 좋은 건가?" |
| `barami_general_05` | "구름이(나귀)가 오늘 유난히 빨리 왔어. 이 마을을 좋아하나 봐!" |

#### 계절별 대사 (각 계절 3종, 총 12종)

| 계절 | ID | 대사 |
|------|-----|------|
| 봄 | `barami_spring_01` | "봄바람 타고 왔어! 이번에 진짜 좋은 거 가져왔다~" |
| 봄 | `barami_spring_02` | "봄에는 새 씨앗이 많이 나오지. 다른 마을에서 구한 특별한 씨앗이 있어!" |
| 봄 | `barami_spring_03` | "벚꽃이 피는 마을에서 왔어. 거기 농부들이 쓰는 신기한 비료가 있는데..." |
| 여름 | `barami_summer_01` | "여름엔 여행하기 좋지! 남쪽 마을에서 귀한 물건 건져 왔어." |
| 여름 | `barami_summer_02` | "더워서 구름이가 힘들어해. 근데 물건은 끝내주거든!" |
| 여름 | `barami_summer_03` | "해변가 마을에서 온 거야. 거기 농부들은 소금으로 땅을 관리하더라!" |
| 가을 | `barami_autumn_01` | "가을 축제 시즌이네! 이맘때 장사가 잘 돼서 기분 좋아~" |
| 가을 | `barami_autumn_02` | "단풍이 예쁜 산골 마을에서 왔어. 거기서 진짜 좋은 물건 발견했지!" |
| 가을 | `barami_autumn_03` | "가을에는 수확 축제를 하는 마을이 많아. 이 마을도 뭔가 하면 좋을 텐데!" |
| 겨울 | `barami_winter_01` | "추운 겨울에도 왔지! 나 아니면 이 물건 어디서 구해?" |
| 겨울 | `barami_winter_02` | "눈 덮인 산을 넘어왔어. 구름이가 용감하지? 보상으로 당근 줘야 해." |
| 겨울 | `barami_winter_03` | "겨울에는 온실 전용 씨앗이 인기야. 준비됐으면 보여줄게!" |

**대사 출력 규칙**: 현재 계절에 해당하는 계절 대사 3종 + 범용 대사 5종 = 총 8종 풀에서 랜덤 선택. 가중치는 계절 대사 50%, 범용 대사 50%. (여행 상인은 계절감이 더 중요하므로 다른 NPC보다 계절 비중이 높다.)

### 3.3 구매/비구매 대사

| 상황 | 대사 |
|------|------|
| 아이템 구매 시 | "좋은 선택이야! 후회 안 할 거야~" |
| 고가 아이템 구매 시 (200G 이상) | "오! 큰 손이시네! 이건 진짜 특별한 거야, 잘 쓸 거지?" |
| 여러 개 구매 시 | "와, 많이 사네! 나도 기분 좋고, 너도 좋고! 완벽해~" |
| 아무것도 안 사고 나갈 때 | "괜찮아, 다음에 또 올 테니까. 기다릴게~" |
| 매진된 아이템 선택 시 | "아, 그건 이미 다 팔렸어. 다음에 또 가져올게!" |
| 골드 부족 시 | "음... 좀 부족하네. 다음에 여유 있을 때 와!" |
| 골드 아슬아슬하게 충분 시 | "딱 맞네! 운이 좋은 거야~" |

### 3.4 여행 일지 대사 (Travel Log Dialogue)

바람이의 고유 시스템. 매 방문마다 1개의 여행 이야기를 들려준다. "이야기하기" 선택지를 통해 접근한다.

#### 범용 여행 일지 (친밀도 무관, 계절별 풀에서 1개 선택)

**봄 여행 일지 (4종)**

| ID | 이야기 |
|----|--------|
| `travel_spring_01` | "지난주에 꽃 축제를 하는 마을에 갔었어. 거기 농부가 꽃으로 만든 향수를 팔더라. 엄청 비싸더라고!" |
| `travel_spring_02` | "동쪽에 강가 마을이 있는데, 거기서 낚시로 먹고산대. 나는 물고기보다 신기한 물건이 좋지만~" |
| `travel_spring_03` | "어떤 마을에서 거대한 감자를 키우는 대회를 하더라. 우승 감자가 사람 머리만 했어! 진짜야!" |
| `travel_spring_04` | "산 위에 약초만 키우는 할머니가 있어. 그분한테 신기한 비료 레시피를 좀 배워 왔지~" |

**여름 여행 일지 (4종)**

| ID | 이야기 |
|----|--------|
| `travel_summer_01` | "남쪽 해변 마을에 갔는데, 거기 농부들이 해초로 비료를 만들더라. 냄새는 좀 그렇지만 효과는 대단해!" |
| `travel_summer_02` | "사막 가장자리에 오아시스 마을이 있어. 거기서 선인장 열매를 파는데, 맛이 기가 막혀!" |
| `travel_summer_03` | "여름에 산불이 난 마을을 지나왔어. 다행히 다 꺼졌는데, 불 탄 땅에서 특이한 버섯이 자라더라." |
| `travel_summer_04` | "해변에서 조개를 캐는 아이들을 봤어. 그중 하나가 진주를 찾았대! 운이 좋은 녀석이지~" |

**가을 여행 일지 (4종)**

| ID | 이야기 |
|----|--------|
| `travel_autumn_01` | "포도밭 마을에서 와인 축제를 하더라. 농부들이 자기 와인이 최고라고 싸우는 게 재밌었어!" |
| `travel_autumn_02` | "단풍 산에서 길을 잃었어. 구름이가 아니었으면 큰일 날 뻔했지. 착한 당나귀야~" |
| `travel_autumn_03` | "호박 조각 대회를 하는 마을이 있어. 호박으로 사람 얼굴을 조각하는데, 진짜 닮았더라!" |
| `travel_autumn_04` | "밤 줍기 축제에 갔는데, 농부들이 수확한 밤을 나눠 먹더라. 따뜻하고 좋은 분위기였어." |

**겨울 여행 일지 (4종)**

| ID | 이야기 |
|----|--------|
| `travel_winter_01` | "눈 축제를 하는 마을에 갔어. 눈사람 만들기 대회를 하더라. 나도 참가했는데... 꼴찌했어." |
| `travel_winter_02` | "북쪽 마을에서 따뜻한 수프를 얻어먹었어. 추운 겨울에 따뜻한 음식이 얼마나 감사한지 몰라." |
| `travel_winter_03` | "겨울에도 온실 농사를 하는 마을이 있어. 이 마을도 온실이 있으면 겨울이 덜 심심할 텐데!" |
| `travel_winter_04` | "구름이가 겨울에 추워하거든. 그래서 구름이한테 담요를 만들어 줬어. 보여줄까? 귀엽지?" |

#### 친밀도별 특별 여행 일지

`Acquaintance` 이상부터 추가 풀이 개방된다.

| 친밀도 | ID | 이야기 |
|--------|-----|--------|
| `Acquaintance` | `travel_personal_01` | "사실 나, 원래는 한 마을에 정착해서 살고 있었어. 근데 어느 날 바람이 부는 걸 보고 '나도 저 바람처럼 자유롭고 싶다' 하고 떠났지. 그래서 이름이 바람이야." |
| `Acquaintance` | `travel_personal_02` | "구름이는 내 첫 번째 마을에서 데려왔어. 그때 아기 나귀였는데, 지금은 나보다 든든해. 유일한 가족이야." |
| `Regular` | `travel_personal_03` | "가끔 생각해. 떠돌지 않고 한 곳에 머물면 어떨까 하고. 근데 다음 마을에 뭐가 있을지 궁금해서 못 멈추겠어." |
| `Regular` | `travel_personal_04` | "이 마을이 좀 특별해. 다른 마을은 그냥 장사하고 떠나는데, 여기는... 돌아오고 싶어져." |
| `Friend` | `travel_personal_05` | "...솔직히 말하면, 나 원래 마을에서 쫓겨난 거야. 물건을 팔다가 사기꾼이라고 오해받아서. 억울했지. 그래서 떠돌기 시작한 거야. 근데 지금은 이 생활이 좋아." |
| `Friend` | `travel_personal_06` | "이 마을에 가게를 열면 어떨까 하는 생각을 해 봤어. 근데 그러면 여행을 못 하잖아. 어렵다, 어려워..." |

### 3.5 특수 대화

특정 조건에서 1회만 트리거되는 대화.

| 조건 | 트리거 | 대사 |
|------|--------|------|
| 첫 번째 구매 | `flag:firstBaramiBuy` | "첫 구매! 기념이다! 나한테 처음 사는 거잖아. 잘 쓸 거지? 다음에도 좋은 물건 가져올게~" |
| 누적 구매 10회 | `flag:baramiBuy10` | "벌써 10번이나 샀어? 진짜 단골이네! 앞으로 더 좋은 물건만 가져올게, 약속!" |
| 누적 구매 30회 | `flag:baramiBuy30` | "30번째 구매라니... 이 마을에서 내 최고의 고객이야. 아니, 다른 마을 통틀어서도! 진짜야!" |
| 겨울 + 온실 보유 | `season:winter AND flag:hasGreenhouse` | "오, 온실이 있네! 겨울 전용 씨앗 관심 있어? 다른 데서 못 구하는 거야~" |
| 겨울 + 온실 미보유 | `season:winter AND !flag:hasGreenhouse` | "겨울이라 씨앗은 별로 없는데... 에너지 토닉이나 부적은 어때?" |
| 퇴장 직전 (16:30 이후 방문) | `time >= 16:30` | "앗, 곧 출발해야 해! 빨리 골라! 서두르자~" |
| 비 오는 등장일 | `weather:Rain` | "비 오는 날에도 왔지! 나 프로의식이 대단하지 않아? 구름이는 불만이지만~" |

### 3.6 아이템 추천 대사

바람이가 현재 판매 인벤토리에서 아이템을 추천할 때 사용하는 대사.

| 아이템 종류 | 추천 대사 |
|------------|-----------|
| 비계절 씨앗 | "이건 다음 계절 씨앗인데, 미리 사 두면 시작하자마자 심을 수 있어! 하나 가게보다 비싸지만, 선점이 최고야." |
| 만능 비료 | "이 비료는 진짜 대박이야. 품질도 올리고 성장도 빨라지는 올인원! 많이 없으니까 서둘러~" |
| 에너지 토닉 | "하루가 모자라지? 이걸 마시면 에너지가 50 회복돼! 바쁜 날에 딱이야." |
| 성장 촉진제 | "작물 성장을 하루 단축시켜 줘! 비싸지만 수확일이 하루 빨라지면 그 가치는..." |
| 행운의 부적 | "이건 진짜 희귀해. 하루 동안 최고 품질 작물이 나올 확률이 올라가거든!" |
| 풍향계 | "이걸 농장에 세우면 내일 날씨를 미리 알 수 있어. 농부한테 정보는 무기야!" |

### 3.7 친밀도 단계별 대화

관계 발전 단계(섹션 2.5)에 따라 추가되는 대사.

#### 지나가는 손님 (Stranger)

기본 범용/계절 대사만 출력. 추가 대사 없음.

#### 얼굴을 아는 사이 (Acquaintance)

단계 전환 시 1회성 특수 대사:

```
[바람이] "아, 너 이름이 {플레이어명}이지? 기억해!
          이 마을에 올 때마다 네 농장이 조금씩 커지는 게 보여.
          다음에 올 때 재미있는 소문 가져올게~"
```

#### 단골 고객 (Regular)

단계 전환 시 1회성 특수 대사:

```
[바람이] "야, {플레이어명}! 너 진짜 단골이다!
          이렇게 자주 사 주는 사람이 많지 않거든.
          고마우니까 앞으로 물건을 하나 더 가져올게. 특별 서비스야!"
```

기존 대사 풀에 다음이 추가된다.

| ID | 대사 |
|----|------|
| `barami_regular_01` | "네가 올 줄 알고 특별히 좋은 물건 챙겨 왔어. 봐 봐!" |
| `barami_regular_02` | "다른 마을에서 네 얘기 했더니, '그 농장이요?' 하더라. 소문이 났나 봐!" |
| `barami_regular_03` | "구름이가 이 마을 길을 기억하더라. 나보다 먼저 출발하려고 해!" |

#### 바람의 친구 (Friend)

단계 전환 시 1회성 특수 대사:

```
[바람이] "{플레이어명}... 사실 나, 이 마을이 제일 좋아.
          다른 마을은 물건 팔고 바로 떠나거든. 근데 여기는 돌아오고 싶어져.
          이런 기분은 처음이야. 나한테 '친구'가 생긴 건가?
          앞으로 좀 깎아 줄게. 친구니까!"
```

기존 대사 풀에 다음이 추가된다.

| ID | 대사 |
|----|------|
| `barami_friend_01` | "가끔 다른 마을에서 네 농장 작물을 보거든? '이건 {플레이어명}님 마을에서 왔습니다' 하고 팔더라. 유명해졌어!" |
| `barami_friend_02` | "구름이가 다른 데보다 여기서 편안해 보여. 나도 그렇고. 이상한 거지?" |
| `barami_friend_03` | "언젠가 여행을 그만두면... 이 마을에 자리 잡을지도 몰라. 아직은 아니지만." |

### 3.8 퇴장 대사

17:00이 되면 좌판을 접으며 출력하는 대사.

| 상황 | 대사 |
|------|------|
| 일반 퇴장 | "오늘은 여기까지! 바람이 부는 대로 다음 마을로 갈게. 다음 주말에 또 들를 테니 기대해~" |
| 물건을 많이 판 날 | "오늘 장사 대박! 구름이한테 맛있는 당근 사 줘야겠다. 다음에 또 좋은 물건 가져올게!" |
| 아무것도 안 판 날 | "오늘은 인기가 없었네... 괜찮아, 다음에 더 좋은 물건 가져올 테니까! 기대해!" |
| 비 오는 날 퇴장 | "비가 와서 구름이가 짜증을 내네. 빨리 갈게! 다음에 또~" |

---

## 4. 계절별 특별 이벤트

### 4.1 계절별 특별 아이템

각 계절에 바람이가 가져오는 특별 아이템 테마가 있다.

| 계절 | 특별 테마 | 대사 |
|------|----------|------|
| 봄 | "봄의 선물" (씨앗/비료 위주) | "봄에는 새로운 시작을 위한 물건을 준비했어! 씨앗이랑 비료 위주야~" |
| 여름 | "여름의 보물" (에너지/성장 위주) | "여름엔 바쁘잖아! 에너지 토닉이랑 성장 촉진제 챙겨 왔어!" |
| 가을 | "가을의 행운" (부적/장식 위주) | "수확 시즌이니까 행운 아이템을 많이 가져왔어! 금빛 작물 확률 올려 봐!" |
| 겨울 | "겨울의 희망" (온실 씨앗/장식 위주) | "겨울에도 희망은 있어! 온실 씨앗이랑 예쁜 장식 가져왔지~" |

### 4.2 연말 특별 방문 (겨울 4주차)

겨울 마지막 주에는 바람이가 특별 인벤토리를 가져온다.

| 항목 | 내용 |
|------|------|
| 등장 시간 | 겨울 27~28일 (마지막 주말) |
| 특별 아이템 | "새해 축복 씨앗 세트" (다음 계절 봄 씨앗 3종 각 5개, 가격: 정가 x1.5) |
| 특별 대사 | "올해도 고마웠어! 새해 선물로 봄 씨앗 세트를 준비했어. 봄이 오면 바로 심을 수 있지!" |
| 한정 수량 | 1세트만 판매 |

**설계 의도**: 겨울 마지막 주에 봄 씨앗을 미리 구매할 수 있게 하여, 봄 시작과 동시에 농사를 시작할 수 있는 전략적 선택지를 제공한다. 가격이 1.5배이므로 "미리 준비하는 비용"의 트레이드오프가 존재한다.

---

## 5. 나귀 "구름이" (Cloud, Barami's Donkey)

바람이의 동반자인 나귀도 간단한 인터랙션을 제공한다.

### 5.1 구름이 인터랙션

| 인터랙션 | 조건 | 반응 |
|----------|------|------|
| 구름이에게 말 걸기 | 바람이 등장 시 나귀 근접 + E키 | "히이잉!" (나귀 울음 소리) + 머리를 흔드는 애니메이션 |
| 당근 주기 | 인벤토리에 당근 보유 + 구름이에게 사용 | 당근 1개 소비. 구름이가 기뻐하는 애니메이션. 바람이: "오, 구름이한테 당근을? 고마워! 구름이가 좋아해~" 친밀도 +1 |

[OPEN] 구름이에게 당근을 주는 것이 바람이 친밀도를 올리는 추가 경로로 적절한지. 다른 NPC에 비해 친밀도 상승 수단이 제한적인 바람이에게 보완 경로가 될 수 있지만, 시스템 복잡도가 증가한다.

### 5.2 구름이 외형

| 요소 | 묘사 |
|------|------|
| 크기 | 표준 나귀 크기 (플레이어 허리 높이) |
| 색상 | 밝은 회색 (구름을 연상시키는 색) |
| 장비 | 등에 큰 상자 2개 (바람이의 상품 보관), 상자에 색색의 천 장식 |
| 특징 | 큰 귀, 온순한 표정. 가끔 발로 땅을 긁는 동작 |

---

## 6. 튜닝 파라미터

기본 여행 상인 파라미터는 (-> see `docs/content/npcs.md` 섹션 9.1~9.3)에 정의되어 있다. 본 문서에서 추가로 정의하는 파라미터는 다음과 같다.

| 파라미터 | 영문 키 | 현재 값 | 조정 범위 | 영향 |
|----------|---------|---------|-----------|------|
| 친밀도 임계값 | `baramiAffinityThresholds` | (→ see 섹션 2.5 임계값 테이블) | - | 관계 발전 속도 |
| 구매 친밀도 증가 | `baramiBuyAffinityGain` | 2 | 1~3 | 구매의 관계 기여도 |
| 방문 대화 친밀도 증가 | `baramiChatAffinityGain` | 1 | 0~2 | 방문만으로의 관계 기여도 |
| 구름이 당근 친밀도 증가 | `baramiDonkeyCarrotGain` | 1 | 0~2 | 보완 친밀도 경로 |
| Regular 추가 아이템 수 | `baramiRegularBonusItems` | 1 | 0~2 | 단골 보상 강도 |
| Friend 할인율 | `baramiFriendDiscount` | 0.05 | 0.03~0.10 | 가격 할인 강도 |
| Friend 추가 재고 | `baramiFriendBonusStock` | 1 | 0~2 | 재고 증가 강도 |
| 여행 일지 계절당 풀 크기 | `baramiTravelLogPoolSize` | 4 | 3~6 | 이야기 다양성 |
| 연말 특별 세트 가격 배수 | `baramiNewYearSetMult` | 1.5 | 1.2~2.0 | 사전 준비 비용 |

---

## Cross-references

| 문서 | 참조 내용 |
|------|-----------|
| `docs/content/npcs.md` 섹션 6 | 바람이 기본 캐릭터 설정, 등장 조건, 아이템 풀, 가격 |
| `docs/content/npcs.md` 섹션 7 | NPC 대화 시스템 공통 구조 (트리거, 우선순위, 반복 방지) |
| `docs/content/npcs.md` 섹션 8 | 상점 공통 UI 흐름 |
| `docs/content/npcs.md` 섹션 9.1~9.3 | 여행 상인/만능 비료/특수 아이템 튜닝 파라미터 |
| `docs/systems/economy-system.md` 섹션 2 | 경제 시스템 수급 보정 |
| `docs/systems/economy-system.md` 섹션 3 | 상점 종류, 영업시간 |
| `docs/design.md` 섹션 4.2 | 작물/씨앗 가격 (비계절 씨앗 가격 기준) |
| `docs/balance/progression-curve.md` 섹션 1.3.2 | 레벨별 해금 콘텐츠 |
| `docs/content/blacksmith-npc.md` | 철수 NPC 상세 (관계 발전 구조 참고) |
| `docs/content/merchant-npc.md` | 하나 NPC 상세 (관계 발전 구조 참고) |
| `docs/content/carpenter-npc.md` | 목이 NPC 상세 (관계 발전 구조 참고) |
| `docs/systems/npc-shop-architecture.md` | NPC/상점 시스템 기술 아키텍처 |

---

## Open Questions

1. [OPEN] **구름이 당근 인터랙션**: 나귀에게 당근을 주는 것이 친밀도 경로로 적절한지. 시스템 복잡도 대비 재미 요소의 가치 판단 필요.

2. [OPEN] **여행 일지와 월드 빌딩**: 바람이의 여행 이야기에 등장하는 "다른 마을"이 게임 세계관에서 어떤 위치를 차지하는지. 현재는 분위기 텍스트에 불과하지만, 후속 콘텐츠에서 활용할 여지가 있다.

3. [OPEN] **연말 특별 세트 밸런스**: 봄 씨앗 3종 x 5개를 1.5배 가격에 제공하는 것이 적절한지. 봄 시작 시 하나 상점에서 정가 구매하는 것 대비 얼마나 이점이 있는지 경제 밸런스 검증 필요. 봄 1일차에 바로 심는 시간 이점의 가치가 50% 프리미엄을 정당화하는지.

4. [OPEN] **NPC 호감도 시스템 통합**: `docs/content/npcs.md` Open Question 2와 동일. 바람이의 친밀도 임계값이 다른 NPC와 다른 점(8/20/40 vs 10/25/50)이 통합 시 어떻게 처리될지.

5. [RISK] **여행 일지 텍스트 분량**: 계절당 4종 x 4계절 = 16종 + 친밀도별 6종 = 총 22종의 여행 일지는 Phase 2 번역/로컬라이제이션 시 부담이 될 수 있다.

---

