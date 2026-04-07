# 업적 콘텐츠 상세 (Achievement Content Specification)

> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: CON-007

---

## 1. Context

이 문서는 SeedMind의 전체 업적 34종에 대한 콘텐츠 상세 정보를 기술한다. 각 업적의 고유 ID, 이름, 조건 타입, 목표 수치, 보상(골드/XP/칭호/아이템)을 확정 테이블로 정리하며, 칭호 42종의 canonical 목록과 보상 기준 테이블을 포함한다.

### 1.1 본 문서가 canonical인 데이터

- 업적 34종 전체 목록의 확정 보상 수치 (골드, XP, 아이템)
- 단계형 업적 Bronze/Silver/Gold 각 단계별 목표 수치 및 보상
- 칭호 42종 canonical 매핑 테이블 (칭호 ID, 이름, 해금 조건)
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

- 전 업적(34종) 완료 시 총 XP: **2,640 XP** (아래 섹션 3~10 및 섹션 13.1 확정 합산)
- 전체 필요 누적 XP **9,029**의 약 **29.2%**에 해당 (-> see `docs/balance/progression-curve.md` 섹션 2.4.1 — canonical XP 테이블)
- 전 업적 완료는 2년차 이후에나 가능하므로 실질적 XP 가속 효과는 제한적

[NOTE] 업적 XP 비중(29.2%)은 canonical 기준(9,029 XP) 대비 적정 수준이다. BAL-007 통합 시뮬레이션(-> see `docs/balance/xp-integration.md`)에서 퀘스트 900 XP + 업적 2,640 XP 포함 시 1년차 일반 플레이어 레벨 8~9 도달이 예상된다. 낚시 업적(390 XP)은 Zone F 해금(레벨 5) 이후 점진적으로 달성되므로 1년차 내 전부 달성은 어렵다.

### 2.5 골드 보상 총량 추정

- 전 업적 완료 시 총 골드: 약 8,350G (아래 섹션 3~10의 합산)
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

## 10. 숨겨진 업적 (Hidden) -- 6개

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

### 10.2 아이템 보상

숨겨진 업적 중 아이템 보상이 있는 것:

| achievementId | 아이템 | 효과 |
|---------------|--------|------|
| `ach_hidden_05` | 거대 씨앗 x1 | 심으면 거대 작물 생성 확률 50% 상승 (해당 작물 1회). (-> see `docs/systems/crop-growth.md` 섹션 5.1) |

나머지 5개 숨겨진 업적은 칭호+골드+XP만 지급한다.

**Hidden 카테고리 소계**: 골드 600G / XP 180

---

## 11. 칭호 전체 목록 (42종)

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
| 36 | `title_novice_angler` | 초보 낚시꾼 | `ach_fish_01` 달성 | Angler |
| 37 | `title_fishing_lover` | 낚시 애호가 | `ach_fish_02` Silver 달성 | Angler |
| 38 | `title_skilled_angler` | 숙련 낚시꾼 | `ach_fish_02` Gold 달성 | Angler |
| 39 | `title_angler` | 낚시꾼 | `ach_fish_03` 달성 | Angler |
| 40 | `title_legendary_angler` | 전설의 낚시사 | `ach_fish_04` 달성 | Angler |
| 41 | `title_fishing_master` | 낚시 마스터 | 어종 도감 100% + 숙련도 Lv.10 (-> see `docs/systems/fishing-system.md` 섹션 8.2) | Angler |

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
| Hidden | 6 |
| **합계** | **42** |

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

[OPEN] 장식 오브젝트(금고, 장인의 망치, 영웅의 증표, 상인의 뱃지, 황금 낚싯대)의 비주얼 사양 및 배치 시스템은 별도 설계 필요. 현재 농장 꾸미기 시스템이 미설계 상태.

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
| Hidden | 6 | 600G | 180 XP | 6 | 1 (거대 씨앗) |
| **합계** | **34** | **8,350G** | **2,640 XP** | **41 (+기본 1 = 42)** | **13** |

### 13.2 밸런스 검증 포인트

- **총 XP 2,640**: 전체 필요 누적 XP 4,609의 약 57% (-> see `docs/balance/progression-curve.md` 섹션 1.3.2). 낚시 업적 4종 추가로 기존 2,250에서 390 XP 증가
- **총 골드 8,350G**: 게임 2년차 추정 누적 수입 대비 소규모 보너스 (-> see `docs/balance/progression-curve.md` 섹션 3). 낚시 업적 추가로 기존 6,050G에서 2,300G 증가
- 전 업적 완료가 2년차 이후에나 가능하므로, 실질적 밸런스 영향은 제한적
- 낚시 업적의 골드 비중이 다소 높다 (2,300G / 8,350G = 27.5%). 이는 `ach_fish_04` (도감 완성)의 1,000G 보상이 주된 원인이며, 해당 업적은 15종 전종 포획이라는 극후반 도전이므로 적정 수준으로 판단

[RISK] 섹션 2.4에서 추정한 총 XP(~1,690)와 본 섹션의 합산(2,640)에 약 950 XP 차이가 있다. 이는 섹션 2.4가 기준 테이블의 최소치로 추정한 반면, 각 업적의 확정 보상이 일부 상향되었고 낚시 업적 4종(390 XP)이 추가되었기 때문이다. 섹션 2.4의 수치를 본 섹션(13.1)의 확정 합산으로 갱신한다: **총 XP = 2,640, 전체 XP 대비 약 57%**.

[RISK] 업적 XP 비중이 57%로 당초 목표(33~43%)를 크게 초과한다. 숨겨진 업적의 XP를 30 -> 20으로 하향하거나, 일부 보통 난이도 업적의 XP를 50 -> 40으로 하향 조정하는 것을 검토해야 한다. 단, 낚시 업적(390 XP)은 Zone F 해금(레벨 5) 이후에만 시작 가능하고 도감 완성은 극후반이므로 1년차 실질 영향은 제한적이다. 출시 전 시뮬레이션으로 최종 확정 필요.

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

### [RISK] 항목

1. **[RISK]** 업적 XP 총합(2,250)이 전체 필요 XP(4,609)의 약 49%로, 당초 목표 범위(33~43%)를 초과한다. XP 하향 조정 또는 레벨 테이블 상향 검토 필요 (-> see 섹션 12.2)
2. **[RISK]** 업적 XP + 퀘스트 XP 합산 시 레벨업 속도 과도 가속 가능성. 출시 전 수치 시뮬레이션 필수 (-> see `docs/balance/progression-curve.md`)
3. **[RISK]** 세이브 데이터 마이그레이션: 업적 추가/수정 시 기존 세이브 파일 호환성 유지 필요 (-> see `docs/systems/achievement-architecture.md`)

---

> 이 문서는 Claude Code가 자율적으로 작성했습니다.
