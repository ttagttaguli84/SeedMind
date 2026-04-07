# 도전 과제/업적 시스템 (Achievement System) 상세 설계

> 작성: Claude Code (Opus) | 2026-04-07  
> 문서 ID: DES-011

---

## Context

이 문서는 SeedMind의 도전 과제/업적(Achievement) 시스템을 상세히 기술한다. 업적 시스템은 퀘스트 시스템의 "농장 도전(FarmChallenge)" 카테고리와 보완적 관계에 있으나, **별도의 독립 시스템**으로 운영된다. 퀘스트 시스템이 "목표 제시 -> 수행 -> 보상"이라는 능동적 흐름이라면, 업적 시스템은 "플레이 -> 자동 감지 -> 기록 + 보상"이라는 수동적 축적 흐름이다.

**설계 목표**:
- 플레이어의 다양한 행동에 대해 누적적 성취감을 제공
- 칭호/기록 등 비경제적 보상으로 장기 동기 부여
- 숨겨진 업적으로 탐색/실험의 즐거움 유도
- 단계형 업적(Bronze/Silver/Gold)으로 점진적 목표 스케일 제시

**퀘스트 시스템과의 관계**:
- 퀘스트의 `FarmChallenge`(농장 도전)은 "게임 진행 마일스톤"에 초점 (첫 수확, 레벨 도달 등)
- 업적 시스템은 "행동의 깊이와 다양성"에 초점 (같은 작물 100회 수확, 모든 NPC 만남 등)
- 일부 농장 도전은 업적과 조건이 겹칠 수 있으나, 보상 체계가 다르다 (농장 도전 = 골드/XP, 업적 = 칭호/아이템/기록)
- 중복을 최소화하되, 겹치는 경우 양쪽 모두 달성으로 인정한다

### 본 문서가 canonical인 데이터

- 업적 카테고리 정의 및 분류 체계
- 전체 업적 목록 (ID, 이름, 조건, 보상, 단계)
- 칭호 시스템 정의 (칭호 목록, 장착 규칙)
- 업적 UI/UX 구조
- 업적 보상 체계 (칭호, 아이템, 기록)
- 진행도 추적 이벤트 정의
- 숨겨진 업적 목록 및 해금 조건

### 본 문서가 canonical이 아닌 데이터 (참조만)

| 데이터 종류 | 참조처 |
|------------|--------|
| 작물 이름, 씨앗 가격, 판매가, 성장일수 | `docs/design.md` 섹션 4.2 |
| XP 소스, 레벨별 필요 XP, 해금 테이블 | `docs/balance/progression-curve.md` |
| 골드 초기 지급, 가격 시스템 | `docs/systems/economy-system.md` |
| 퀘스트 분류, 농장 도전 목록 | `docs/systems/quest-system.md` 섹션 1, 6 |
| NPC 이름, 역할, 위치 | `docs/content/npcs.md` 섹션 2 |
| 시설 이름, 비용, 해금 레벨 | `docs/design.md` 섹션 4.6 |
| 도구 업그레이드 등급, 비용 | `docs/systems/tool-upgrade.md` 섹션 1~2 |
| 품질 등급 | `docs/systems/crop-growth.md` 섹션 4.3~4.4 |
| 시간/계절 | `docs/systems/time-season.md` |
| 가공품 레시피 | `docs/content/processing-system.md` |
| 키 바인딩 | `docs/design.md` 섹션 5 |

---

## 1. 업적 분류 체계

SeedMind의 업적은 **9개 카테고리**로 분류된다. 각 카테고리는 게임의 핵심 시스템과 대응한다.

| 카테고리 | 영문 ID | 대응 시스템 | 업적 수 |
|----------|---------|------------|---------|
| 농업 마스터 | `Farming` | 경작/작물 성장 | 5개 |
| 경제 달인 | `Economy` | 경제/판매 | 4개 |
| 시설 개척자 | `Facility` | 시설 건설/확장 | 4개 |
| 도구 장인 | `Tool` | 도구 업그레이드 | 3개 |
| 탐험가 | `Explorer` | NPC/상점/계절 | 4개 |
| 퀘스트 영웅 | `Quest` | 퀘스트/미션 | 4개 |
| 낚시사 | `Angler` | 낚시/어종 수집 | 4개 |
| 채집가 | `Gatherer` | 채집/채집물 수집 | 5개 |
| 숨겨진 업적 | `Hidden` | 특수 행동/이스터에그 | 7개 |

**총 업적 수: 40개**

### 1.1 업적 유형

| 유형 | 영문 ID | 설명 | 예시 |
|------|---------|------|------|
| 단일 업적 | `Single` | 1회 조건 충족으로 달성 | "첫 수확" |
| 단계형 업적 | `Tiered` | Bronze -> Silver -> Gold 3단계 | "누적 수확 50/200/1000" |

단계형 업적은 하위 단계를 달성해야 상위 단계로 진행된다. 각 단계마다 별도 보상을 지급한다.

---

## 2. 업적 데이터 모델

### 2.1 업적 기본 구조

| 필드 | 타입 | 설명 |
|------|------|------|
| `achievementId` | `string` | 고유 식별자 (예: `ach_farming_01`) |
| `category` | `AchievementCategory` | 7개 카테고리 중 하나 |
| `titleKR` | `string` | 한국어 제목 |
| `descriptionKR` | `string` | 한국어 설명 (숨겨진 업적은 달성 전 `"???"` 표시) |
| `type` | `AchievementType` | `Single` / `Tiered` |
| `tiers` | `AchievementTier[]` | 단계형: 3단계 배열, 단일: 1개 배열 |
| `isHidden` | `bool` | 숨김 여부 (true면 미달성 시 이름/조건 비공개) |
| `trackingEvent` | `string` | 추적할 이벤트 ID (섹션 7 참조) |

### 2.2 단계(Tier) 구조

| 필드 | 타입 | 설명 |
|------|------|------|
| `tier` | `AchievementTierLevel` | `Bronze` / `Silver` / `Gold` |
| `conditionValue` | `int` | 달성에 필요한 누적 값 |
| `rewardTitle` | `string?` | 칭호 보상 (null이면 없음) |
| `rewardItem` | `ItemReward?` | 아이템 보상 (null이면 없음) |
| `rewardGold` | `int` | 골드 보상 |
| `rewardXP` | `int` | XP 보상 |

### 2.3 업적 진행도

```
AchievementProgress {
    achievementId: string
    currentValue: int          // 현재 누적 값
    currentTier: "None" | "Bronze" | "Silver" | "Gold"
    unlockedAt: int?           // 최종 단계 달성 게임 일차 (null이면 미달성)
    tierHistory: [
        { tier: "Bronze", unlockedDay: 15 },
        { tier: "Silver", unlockedDay: 45 }
    ]
}
```

---

## 3. 카테고리별 업적 상세

### 3.1 농업 마스터 (Farming)

| ID | 이름 | 유형 | 조건 | 보상 | 숨김 |
|----|------|------|------|------|------|
| `ach_farming_01` | 씨앗의 시작 | Single | 첫 번째 작물 수확 | 칭호: "새싹 농부" | N |
| `ach_farming_02` | 수확의 대가 | Tiered | 누적 작물 수확 | 아래 참조 | N |
| `ach_farming_03` | 사계절 농부 | Single | 4계절(봄/여름/가을/겨울)에 각 1종 이상 작물 수확 (겨울은 온실 포함) | 칭호: "사계절 농부", 골드 (-> see 섹션 4.1), XP (-> see 섹션 4.1) | N |
| `ach_farming_04` | 작물 도감 완성 | Single | 게임 내 모든 작물 종류를 1개 이상 수확 (-> see `docs/design.md` 섹션 4.2 for 작물 목록) | 칭호: "작물 박사", 아이템: 황금 씨앗 x1 (-> see 섹션 4.3) | N |
| `ach_farming_05` | 품질의 끝 | Single | Iridium 품질 작물 수확 (-> see `docs/systems/crop-growth.md` 섹션 4.3 for 품질 등급) | 칭호: "전설의 경작자" | N |

**ach_farming_02 단계 상세**:

| 단계 | 조건 | 보상 |
|------|------|------|
| Bronze | 누적 50개 수확 | 골드 (-> see 섹션 4.1) |
| Silver | 누적 200개 수확 | 칭호: "숙련 농부", 골드 (-> see 섹션 4.1) |
| Gold | 누적 1,000개 수확 | 칭호: "수확의 대가", 아이템: 속성장 비료 x10, 골드 (-> see 섹션 4.1) |

### 3.2 경제 달인 (Economy)

| ID | 이름 | 유형 | 조건 | 보상 | 숨김 |
|----|------|------|------|------|------|
| `ach_economy_01` | 첫 수익 | Single | 첫 번째 출하/판매 수행 | 칭호: "초보 상인" | N |
| `ach_economy_02` | 부의 축적 | Tiered | 누적 판매 수익 | 아래 참조 | N |
| `ach_economy_03` | 대박 거래 | Single | 단일 출하에서 1,000G 이상 수익 달성 | 칭호: "거래의 귀재", 골드 (-> see 섹션 4.1) | N |
| `ach_economy_04` | 가공의 연금술 | Single | 가공품 판매로 누적 5,000G 수익 달성 | 칭호: "가공 마스터", 골드 (-> see 섹션 4.1) | N |

**ach_economy_02 단계 상세**:

| 단계 | 조건 | 보상 |
|------|------|------|
| Bronze | 누적 판매 수익 5,000G | 골드 (-> see 섹션 4.1) |
| Silver | 누적 판매 수익 25,000G | 칭호: "성공한 농부", 골드 (-> see 섹션 4.1) |
| Gold | 누적 판매 수익 100,000G | 칭호: "부의 축적자", 아이템: 금고 장식품 x1, 골드 (-> see 섹션 4.1) |

[OPEN] 경제 달인 Tiered 업적의 Gold 단계(100,000G)가 일반 플레이어의 2년차 누적 수익 범위 내에 있는지 검증 필요. (-> see `docs/balance/progression-curve.md` 섹션 3의 자금 곡선 시뮬레이션 참조)

### 3.3 시설 개척자 (Facility)

| ID | 이름 | 유형 | 조건 | 보상 | 숨김 |
|----|------|------|------|------|------|
| `ach_facility_01` | 첫 건축 | Single | 첫 번째 시설 건설 | 칭호: "건축 입문자" | N |
| `ach_facility_02` | 시설 왕국 | Single | 기본 4종 시설(물탱크, 창고, 온실, 가공소) 모두 건설 (-> see `docs/design.md` 섹션 4.6) | 칭호: "시설 왕", 골드 (-> see 섹션 4.1) | N |
| `ach_facility_03` | 가공 제국 | Single | 4종 가공소(가공소, 제분소, 발효실, 베이커리) 모두 건설 (-> see `docs/design.md` 섹션 4.6) | 칭호: "가공 제국의 주인", 아이템: 고급 연료 x20 | N |
| `ach_facility_04` | 가공의 달인 | Tiered | 누적 가공품 제작 | 아래 참조 | N |

**ach_facility_04 단계 상세**:

| 단계 | 조건 | 보상 |
|------|------|------|
| Bronze | 누적 20개 가공품 제작 | 골드 (-> see 섹션 4.1) |
| Silver | 누적 100개 가공품 제작 | 칭호: "가공 장인", 골드 (-> see 섹션 4.1) |
| Gold | 누적 300개 가공품 제작 | 칭호: "가공의 달인", 아이템: 레시피 (-> see [OPEN] 아래), 골드 (-> see 섹션 4.1) |

[OPEN] ach_facility_04 Gold 단계의 레시피 보상으로 업적 전용 특수 레시피를 제공할지, 기존 레시피 중 하나를 조기 해금할지 결정 필요. (-> see `docs/content/processing-system.md`와 연계)

### 3.4 도구 장인 (Tool)

| ID | 이름 | 유형 | 조건 | 보상 | 숨김 |
|----|------|------|------|------|------|
| `ach_tool_01` | 첫 강화 | Single | 도구 1개를 Reinforced로 업그레이드 (-> see `docs/systems/tool-upgrade.md` 섹션 1.1) | 칭호: "장인 견습생" | N |
| `ach_tool_02` | 완벽한 도구 세트 | Single | 3종 도구(호미, 물뿌리개, 낫) 모두 Reinforced 이상 달성 | 칭호: "도구의 달인", 골드 (-> see 섹션 4.1) | N |
| `ach_tool_03` | 전설의 장비 | Single | 3종 도구 모두 Legendary 달성 (-> see `docs/systems/tool-upgrade.md` 섹션 1.1) | 칭호: "전설의 장인", 아이템: 장인의 망치 장식품 x1 | N |

### 3.5 탐험가 (Explorer)

| ID | 이름 | 유형 | 조건 | 보상 | 숨김 |
|----|------|------|------|------|------|
| `ach_explorer_01` | 마을 인사 | Single | 4명의 NPC(하나, 철수, 목이, 바람이) 모두와 첫 대화 완료 (-> see `docs/content/npcs.md` 섹션 2.1) | 칭호: "사교적인 농부" | N |
| `ach_explorer_02` | 바람이의 단골 | Single | 바람이(여행 상인)에게서 누적 5회 이상 물건 구매 | 칭호: "여행자의 친구", 아이템: 바람이 특별 할인권 x1 (-> see 섹션 4.3) | N |
| `ach_explorer_03` | 사계절의 기억 | Single | 봄/여름/가을/겨울 4계절 모두 경험 (1년 완주) | 칭호: "계절의 증인", 골드 (-> see 섹션 4.1) | N |
| `ach_explorer_04` | 쇼핑 마니아 | Tiered | 상점에서 누적 물건 구매 횟수 | 아래 참조 | N |

**ach_explorer_04 단계 상세**:

| 단계 | 조건 | 보상 |
|------|------|------|
| Bronze | 누적 30회 구매 | 골드 (-> see 섹션 4.1) |
| Silver | 누적 100회 구매 | 칭호: "쇼핑 애호가", 골드 (-> see 섹션 4.1) |
| Gold | 누적 300회 구매 | 칭호: "쇼핑 마니아", 아이템: 상인의 뱃지 장식품 x1, 골드 (-> see 섹션 4.1) |

### 3.6 퀘스트 영웅 (Quest)

| ID | 이름 | 유형 | 조건 | 보상 | 숨김 |
|----|------|------|------|------|------|
| `ach_quest_01` | 첫 임무 완수 | Single | 첫 번째 퀘스트(카테고리 무관) 완료 | 칭호: "모험의 시작" | N |
| `ach_quest_02` | 퀘스트 수집가 | Tiered | 누적 퀘스트 완료 수 (모든 카테고리 합산) | 아래 참조 | N |
| `ach_quest_03` | NPC의 신뢰 | Single | 모든 NPC(하나, 철수, 목이, 바람이)의 의뢰를 각 1개 이상 완료 | 칭호: "마을의 해결사", 골드 (-> see 섹션 4.1) | N |
| `ach_quest_04` | 꾸준한 일꾼 | Single | 일일 목표를 연속 7일 완료 (2개 중 1개 이상 완료 기준) | 칭호: "근면한 농부", 골드 (-> see 섹션 4.1), XP (-> see 섹션 4.1) | N |

**ach_quest_02 단계 상세**:

| 단계 | 조건 | 보상 |
|------|------|------|
| Bronze | 누적 10개 퀘스트 완료 | 골드 (-> see 섹션 4.1) |
| Silver | 누적 30개 퀘스트 완료 | 칭호: "퀘스트 사냥꾼", 골드 (-> see 섹션 4.1) |
| Gold | 누적 100개 퀘스트 완료 | 칭호: "퀘스트 영웅", 아이템: 영웅의 증표 장식품 x1, 골드 (-> see 섹션 4.1) |

### 3.7 낚시사 (Angler)

| ID | 이름 | 유형 | 조건 | 보상 | 숨김 |
|----|------|------|------|------|------|
| `ach_fish_01` | 첫 낚시 | Single | 물고기 1마리 포획 | 칭호: "초보 낚시꾼" | N |
| `ach_fish_02` | 낚시 애호가 | Tiered | 누적 물고기 포획 | 아래 참조 | N |
| `ach_fish_03` | 낚시꾼 | Single | 누적 200마리 포획 | 칭호: "낚시꾼", 아이템: 숙련도 XP 보너스 | N |
| `ach_fish_04` | 전설의 낚시사 | Single | 어종 도감 15/15종 완성 (-> see `docs/systems/fishing-system.md` 섹션 4.2) | 칭호: "전설의 낚시사", 아이템: 황금 낚싯대 장식품 | N |

**ach_fish_02 단계 상세**: (-> see `docs/content/achievements.md` 섹션 9.2 for 확정 수치)

### 3.8 채집가 (Gatherer)

| ID | 이름 | 유형 | 조건 | 보상 | 숨김 |
|----|------|------|------|------|------|
| `ach_gather_01` | 첫 채집 | Single | 채집물 1개 수집 | 칭호: "초보 채집꾼" | N |
| `ach_gather_02` | 채집 애호가 | Tiered | 누적 채집물 수집 | 아래 참조 | N |
| `ach_gather_03` | 채집 도감 완성 | Single | 채집물 도감 27/27종 완성 (-> see `docs/systems/gathering-system.md` 섹션 3.9) | 칭호: "채집 박사", 아이템: 채집 도감 장식품 | N |
| `ach_gather_04` | 전설의 채집가 | Single | Legendary 채집물 누적 5개 수집 (-> see `docs/systems/gathering-system.md` 섹션 3.3~3.7) | 칭호: "전설의 채집가" | N |
| `ach_gather_05` | 채집 낫의 진화 | Single | 채집 낫을 Legendary 등급으로 업그레이드 (-> see `docs/systems/gathering-system.md` 섹션 5.2) | 칭호: "낫의 장인" | N |

**ach_gather_02 단계 상세**:

| 단계 | 조건 | 보상 |
|------|------|------|
| Bronze | 누적 20개 수집 | 골드 (-> see `docs/content/achievements.md` 섹션 9.5.2) |
| Silver | 누적 100개 수집 | 칭호: "채집 애호가", 골드 (-> see `docs/content/achievements.md` 섹션 9.5.2) |
| Gold | 누적 500개 수집 | 칭호: "숙련 채집꾼", 아이템: 채집 숙련도 XP 보너스, 골드 (-> see `docs/content/achievements.md` 섹션 9.5.2) |

### 3.9 숨겨진 업적 (Hidden)

숨겨진 업적은 달성 전까지 업적 패널에서 이름/조건이 `"???"` 으로 표시된다. 달성 시 전체 내용이 공개된다.

| ID | 이름 | 조건 | 보상 | 설명 |
|----|------|------|------|------|
| `ach_hidden_01` | 비 오는 날의 수확 | 비 오는 날에 작물 10개 이상 수확 | 칭호: "비의 농부", 골드 (-> see 섹션 4.1) | 날씨 시스템 탐색 유도 |
| `ach_hidden_02` | 밤의 방랑자 | 밤 시간대(22:00~24:00)에 농장 밖에서 활동 | 칭호: "야행성 농부" | 시간대 탐색 유도, 기절 위험과 트레이드오프 (-> see `docs/systems/time-season.md` 섹션 1.5) |
| `ach_hidden_03` | 물만 주는 농부 | 하루 동안 물주기만 30회 이상 수행 (다른 도구 사용 금지) | 칭호: "물의 정원사", 골드 (-> see 섹션 4.1) | 단일 행동 극한 플레이 |
| `ach_hidden_04` | 빈손의 부자 | 소지 골드 0G 상태에서 작물 판매로 단일 거래 500G 이상 달성 | 칭호: "역전의 농부", 골드 (-> see 섹션 4.1) | 위기 극복 상황 |
| `ach_hidden_05` | 거대 작물의 주인 | 거대 작물(Giant Crop) 수확 (-> see `docs/systems/crop-growth.md` 섹션 5.1) | 칭호: "거대 작물의 주인", 아이템: 거대 씨앗 x1 (-> see 섹션 4.3) | 희귀 이벤트 보상 |
| `ach_hidden_06` | 전부 다 팔아! | 인벤토리의 모든 슬롯에 작물이 있는 상태에서 한 번에 전부 출하 | 칭호: "통큰 농부", 골드 (-> see 섹션 4.1) | 대량 출하 이스터에그 |
| `ach_hidden_07` | 통합 수집 마스터 | 어종 도감 15/15종 완성(`ach_fish_04` 달성) + 채집 도감 27/27종 완성(`ach_gather_03` 달성). 두 업적 모두 달성 시 자동 해금 | 칭호: "수집의 대가", 아이템: 도감 배경: 전설의 자연 x1, 골드 (-> see 섹션 4.1) | 연쇄 해금 — `HandleAchievementChain` (-> see `docs/systems/achievement-architecture.md` 섹션 3.2) |

**숨겨진 업적 설계 원칙**:
- 모두 "특이한 상황" 또는 "의도적 극단 행동"을 요구
- 일반 플레이에서 우연히 달성 가능한 것(01, 05)과 의도적으로 시도해야 하는 것(03, 04, 06)을 혼합
- 어떤 숨겨진 업적도 게임 진행에 필수가 아님
- 보상은 칭호 중심이며, 게임 밸런스에 영향을 주는 강력한 아이템은 제외

---

## 4. 보상 체계

### 4.1 골드/XP 보상 테이블

업적의 골드/XP 보상은 단계와 난이도에 따라 차등 지급한다. 업적 보상이 퀘스트 보상이나 작물 판매 수익을 압도하지 않도록 억제한다.

**단일 업적 보상 범위**:

| 난이도 | 골드 | XP | 기준 |
|--------|------|-----|------|
| 쉬움 (초반 달성) | 50G | 20 XP | 첫 수확, 첫 건축 등 |
| 보통 (중반 달성) | 150G | 50 XP | NPC 전원 만남, 사계절 경험 등 |
| 어려움 (후반 달성) | 300G | 100 XP | 전설 도구 세트, 전 작물 수확 등 |

**단계형 업적 보상 범위**:

| 단계 | 골드 | XP | 기준 |
|------|------|-----|------|
| Bronze | 50G | 20 XP | 초반~중반에 자연스럽게 달성 |
| Silver | 150G | 50 XP | 중반~후반에 달성 |
| Gold | 300G | 100 XP | 후반 장기 플레이로 달성 |

**숨겨진 업적 보상**: 기본 100G + 30 XP (난이도 무관 균일). 숨겨진 업적의 가치는 칭호와 발견의 즐거움에 있으므로 골드/XP는 적정 수준으로 억제한다.

**업적 보상 XP 총량 추정**:
- 전 업적(40개) 완료 시 총 XP: (-> see `docs/content/achievements.md` 섹션 13.1 — 확정 합계 3,160 XP)
- 이는 전체 필요 누적 XP 9,029 (-> see `docs/balance/progression-curve.md` 섹션 1.3.2)의 약 35.0%에 해당 (-> see `docs/content/achievements.md` 섹션 2.4)
- 단, 전 업적 완료는 게임 후반(2년차 이후)에나 가능하므로 실질적 XP 가속 효과는 제한적

[RISK] 업적 XP가 퀘스트 XP(계절당 500~1,100 XP, -> see `docs/systems/quest-system.md` 섹션 7.3)와 합산 시 레벨업 속도를 과도하게 가속할 수 있다. 출시 전 시뮬레이션으로 검증 필요.

### 4.2 칭호 시스템

칭호는 플레이어 이름 옆에 표시되는 수식어이다. 업적 달성으로 해금되며, 해금된 칭호 중 1개를 선택하여 장착할 수 있다.

#### 4.2.1 칭호 표시 규칙

| 항목 | 규칙 |
|------|------|
| 표시 위치 | 플레이어 이름 앞 (예: `[전설의 경작자] 플레이어`) |
| 동시 장착 | 1개만 장착 가능 |
| 기본 칭호 | "농부" (게임 시작 시 자동 부여) |
| 변경 방법 | 업적 패널 -> 칭호 탭에서 선택 |
| 표시 영역 | HUD 좌측 상단, 업적 패널, 농장 게시판 |

#### 4.2.2 전체 칭호 목록

아래는 업적으로 해금되는 모든 칭호의 목록이다. 칭호는 순수 장식 요소이며, 게임플레이에 영향을 주지 않는다.

| 칭호 | 해금 업적 | 카테고리 |
|------|----------|----------|
| 농부 (기본) | 게임 시작 | - |
| 새싹 농부 | `ach_farming_01` | Farming |
| 숙련 농부 | `ach_farming_02` Silver | Farming |
| 수확의 대가 | `ach_farming_02` Gold | Farming |
| 사계절 농부 | `ach_farming_03` | Farming |
| 작물 박사 | `ach_farming_04` | Farming |
| 전설의 경작자 | `ach_farming_05` | Farming |
| 초보 상인 | `ach_economy_01` | Economy |
| 성공한 농부 | `ach_economy_02` Silver | Economy |
| 부의 축적자 | `ach_economy_02` Gold | Economy |
| 거래의 귀재 | `ach_economy_03` | Economy |
| 가공 마스터 | `ach_economy_04` | Economy |
| 건축 입문자 | `ach_facility_01` | Facility |
| 시설 왕 | `ach_facility_02` | Facility |
| 가공 제국의 주인 | `ach_facility_03` | Facility |
| 가공 장인 | `ach_facility_04` Silver | Facility |
| 가공의 달인 | `ach_facility_04` Gold | Facility |
| 장인 견습생 | `ach_tool_01` | Tool |
| 도구의 달인 | `ach_tool_02` | Tool |
| 전설의 장인 | `ach_tool_03` | Tool |
| 사교적인 농부 | `ach_explorer_01` | Explorer |
| 여행자의 친구 | `ach_explorer_02` | Explorer |
| 계절의 증인 | `ach_explorer_03` | Explorer |
| 쇼핑 애호가 | `ach_explorer_04` Silver | Explorer |
| 쇼핑 마니아 | `ach_explorer_04` Gold | Explorer |
| 모험의 시작 | `ach_quest_01` | Quest |
| 퀘스트 사냥꾼 | `ach_quest_02` Silver | Quest |
| 퀘스트 영웅 | `ach_quest_02` Gold | Quest |
| 마을의 해결사 | `ach_quest_03` | Quest |
| 근면한 농부 | `ach_quest_04` | Quest |
| 비의 농부 | `ach_hidden_01` | Hidden |
| 야행성 농부 | `ach_hidden_02` | Hidden |
| 물의 정원사 | `ach_hidden_03` | Hidden |
| 역전의 농부 | `ach_hidden_04` | Hidden |
| 거대 작물의 주인 | `ach_hidden_05` | Hidden |
| 통큰 농부 | `ach_hidden_06` | Hidden |
| 수집의 대가 | `ach_hidden_07` | Hidden |
| 초보 낚시꾼 | `ach_fish_01` | Angler |
| 낚시 애호가 | `ach_fish_02` Silver | Angler |
| 숙련 낚시꾼 | `ach_fish_02` Gold | Angler |
| 낚시꾼 | `ach_fish_03` | Angler |
| 전설의 낚시사 | `ach_fish_04` | Angler |
| 낚시 마스터 | 어종 도감 100% + 숙련도 Lv.10 | Angler |
| 초보 채집꾼 | `ach_gather_01` | Gatherer |
| 채집 애호가 | `ach_gather_02` Silver | Gatherer |
| 숙련 채집꾼 | `ach_gather_02` Gold | Gatherer |
| 채집 박사 | `ach_gather_03` | Gatherer |
| 전설의 채집가 | `ach_gather_04` | Gatherer |
| 낫의 장인 | `ach_gather_05` | Gatherer |
| 채집 마스터 | 채집 도감 100% + 숙련도 Lv.10 | Gatherer |

**총 칭호 수: 50개** (기본 칭호 포함, Hidden +1 "수집의 대가", Angler 6 + Gatherer 7 추가, 확정 목록은 -> see `docs/content/achievements.md` 섹션 11.1)

### 4.3 아이템 보상

업적 전용 아이템 보상은 다음과 같다. 게임 밸런스에 과도한 영향을 주지 않으면서, 해당 업적의 주제와 관련된 실용적 보상을 제공한다.

| 아이템 | 해금 업적 | 효과 |
|--------|----------|------|
| 황금 씨앗 x1 | `ach_farming_04` | 심으면 랜덤 Gold 품질 이상 작물 생성. 1회용 |
| 속성장 비료 x10 | `ach_farming_02` Gold | (-> see `docs/systems/farming-system.md` 섹션 5.1 for 비료 효과) |
| 금고 장식품 x1 | `ach_economy_02` Gold | 농장 배치 가능 장식 오브젝트. 기능 없음 |
| 고급 연료 x20 | `ach_facility_03` | (-> see `docs/content/processing-system.md` for 연료 시스템) |
| 특수 레시피 x1 | `ach_facility_04` Gold | [OPEN] 업적 전용 특수 레시피 vs 기존 레시피 조기 해금 미정. (-> see `docs/content/processing-system.md`) |
| 장인의 망치 장식품 x1 | `ach_tool_03` | 농장 배치 가능 장식 오브젝트. 기능 없음 |
| 바람이 특별 할인권 x1 | `ach_explorer_02` | 바람이 상점에서 1회 20% 할인 적용 |
| 상인의 뱃지 장식품 x1 | `ach_explorer_04` Gold | 농장 배치 가능 장식 오브젝트. 기능 없음 |
| 영웅의 증표 장식품 x1 | `ach_quest_02` Gold | 농장 배치 가능 장식 오브젝트. 기능 없음 |
| 미끼통 x1 | `ach_fish_02` Silver | 미끼 자동 장착 소품 [OPEN] (-> see `docs/systems/fishing-system.md` 섹션 8.3) |
| 낚시 숙련도 XP 보너스 | `ach_fish_03` | 다음 50회 포획 시 숙련도 XP +25% (-> see `docs/systems/fishing-system.md` 섹션 7) |
| 황금 낚싯대 장식품 x1 | `ach_fish_04` | 농장 배치 가능 장식 오브젝트. 기능 없음 |
| 채집 숙련도 XP 보너스 | `ach_gather_02` Gold | 다음 50회 채집 시 숙련도 XP +25% (-> see `docs/systems/gathering-system.md` 섹션 4) |
| 채집 도감 장식품 x1 | `ach_gather_03` | 농장 배치 가능 장식 오브젝트. 기능 없음 |
| 거대 씨앗 x1 | `ach_hidden_05` | 심으면 거대 작물 생성 확률 50% 상승 (해당 작물 1회). (-> see `docs/systems/crop-growth.md` 섹션 5.1 for 거대 작물) |

> 아이템 보상 전체 목록(10종) canonical 출처는 `docs/content/achievements.md` 섹션 11.

[OPEN] 장식 오브젝트(금고, 장인의 망치, 영웅의 증표, 상인의 뱃지)의 비주얼 사양 및 배치 시스템은 별도 설계 필요. 현재 농장 꾸미기 시스템이 미설계 상태.

### 4.4 농장 게시판 (Hall of Fame)

농장 게시판은 플레이어의 업적 달성 기록을 시각적으로 전시하는 인게임 오브젝트이다.

| 항목 | 사양 |
|------|------|
| 위치 | 농장 내 고정 위치 (초기 배치) |
| 상호작용 | E키로 열기 |
| 표시 내용 | 달성한 업적 수 / 전체 업적 수, 최근 달성 업적 3개, 현재 장착 칭호, 총 플레이 일수 |
| 시각 연출 | 업적 달성마다 게시판에 메달/배지 아이콘 추가 (Bronze/Silver/Gold 색상) |
| NPC 반응 | NPC가 농장 방문 시 게시판을 보고 반응하는 대사 (-> see [OPEN] 아래) |

[OPEN] NPC 농장 방문 시스템이 미설계. 게시판 NPC 반응은 해당 시스템 설계 후 연동.

---

## 5. UI/UX 설계

### 5.1 업적 패널 접근

| 항목 | 사양 |
|------|------|
| 단축키 | `Y` 키 |
| 대체 접근 | `Tab` (인벤토리) -> 상단 탭 "업적" 선택 |
| 대체 접근 2 | 농장 게시판 상호작용 -> "상세 보기" 버튼 |

> **키 바인딩 근거**: 기존 키 할당 (-> see `docs/design.md` 섹션 5)에서 WASD(이동), E(상호작용), Tab(인벤토리), J(퀘스트 로그, -> see `docs/systems/quest-system.md` 섹션 8.1), Esc(메뉴), 1~5(도구)가 사용 중이다. `A`키는 WASD 이동의 좌측에 해당하므로 부적절하여 `Y`키를 제안한다.

[OPEN] `Y`키 할당이 최종 확정되지 않았다. 전체 키 바인딩 문서를 별도로 정리하여 충돌 검증이 필요하다.

### 5.2 업적 패널 레이아웃

```
┌──────────────────────────────────────────────────────────┐
│  [인벤토리]  [퀘스트 로그]  [업적]  [칭호]                │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  필터: [전체] [농업] [경제] [시설] [도구] [탐험] [퀘스트] │
│        [달성] [미달성] [숨김]                             │
│                                                          │
│  ── 농업 마스터 ──                              [3/5]    │
│  [Gold] 수확의 대가         1000/1000 ██████████ 달성!   │
│  [Slv]  수확의 대가          200/200  ██████████ 달성!   │
│  [Brz]  수확의 대가           50/50   ██████████ 달성!   │
│  [ V ]  씨앗의 시작           달성                       │
│  [   ]  사계절 농부           2/4 계절 █████░░░░░        │
│  [   ]  작물 도감 완성        6/11 종류 █████░░░░░       │
│  [ V ]  품질의 끝             달성                       │
│                                                          │
│  ── 경제 달인 ──                                [1/4]    │
│  ...                                                     │
│                                                          │
│  진행도: 12/30 업적 달성  (40%)                          │
│  ████████████████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░     │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

### 5.3 칭호 탭 레이아웃

```
┌──────────────────────────────────────────────────────────┐
│  [인벤토리]  [퀘스트 로그]  [업적]  [칭호]                │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  현재 칭호: [전설의 경작자]  플레이어               변경  │
│                                                          │
│  ── 해금된 칭호 (8/36) ──                                │
│                                                          │
│  ( ) 농부              기본 칭호                         │
│  ( ) 새싹 농부         첫 수확 달성                      │
│  ( ) 숙련 농부         수확 200개 달성                   │
│  (O) 전설의 경작자     Iridium 품질 수확                 │
│  ( ) 초보 상인         첫 판매 달성                      │
│  ( ) 건축 입문자       첫 건축 달성                      │
│  ( ) 장인 견습생       첫 도구 강화                      │
│  ( ) 사교적인 농부     모든 NPC 만남                     │
│                                                          │
│  ── 미해금 (28개) ──                                     │
│  [잠김] ??????         업적 달성 시 해금                  │
│  ...                                                     │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

### 5.4 업적 달성 알림

업적 달성 시 화면에 토스트 알림을 표시한다. 퀘스트 완료 알림(화면 중앙 배너, -> see `docs/systems/quest-system.md` 섹션 8.5)과 시각적으로 구분되도록 별도 스타일을 사용한다.

| 이벤트 | 알림 방식 | 위치 | 지속 시간 |
|--------|----------|------|-----------|
| 단일 업적 달성 | 토스트 알림 | 화면 상단 중앙 | 4초 |
| 단계형 업적 단계 달성 | 토스트 알림 + 단계 아이콘 | 화면 상단 중앙 | 4초 |
| 숨겨진 업적 달성 | 특별 토스트 (반짝임 효과) + "숨겨진 업적 달성!" 텍스트 | 화면 상단 중앙 | 6초 |
| 칭호 해금 | 토스트 알림 하단에 "새 칭호: [칭호명]" 추가 | 화면 상단 중앙 | 4초 |

**토스트 알림 레이아웃**:

```
┌───────────────────────────────┐
│ [메달]  업적 달성!            │
│         "씨앗의 시작"         │
│         칭호 해금: 새싹 농부  │
└───────────────────────────────┘
```

**알림 큐잉**: 동시에 여러 업적이 달성될 경우, 알림을 2초 간격으로 순차 표시한다. 최대 큐 크기: 5개 (초과 시 가장 오래된 알림 제거).

### 5.5 진행도 표시 규칙

| 업적 유형 | 표시 형식 | 예시 |
|-----------|----------|------|
| 단일 (수량 기반) | `현재/목표` + 프로그레스바 | `6/11 종류 █████░░░░░` |
| 단일 (이진 기반) | 체크 아이콘 | `[V] 달성` / `[ ] 미달성` |
| 단계형 | 현재 단계 아이콘 + `현재/다음목표` | `[Brz] 120/200 ██████░░░░` |
| 숨겨진 (미달성) | `???` | `[?] ??? — ???` |
| 숨겨진 (달성) | 일반과 동일 | `[V] 비 오는 날의 수확` |

---

## 6. 퀘스트 시스템 농장 도전과의 관계

### 6.1 시스템 경계 정의

| 항목 | 퀘스트 농장 도전 (FarmChallenge) | 업적 (Achievement) |
|------|--------------------------------|---------------------|
| 목적 | 게임 진행 마일스톤 | 행동의 깊이/다양성 기록 |
| 보상 중심 | 골드 + XP (경제적) | 칭호 + 장식 아이템 (비경제적) |
| UI 위치 | 퀘스트 로그 -> 농장 도전 탭 | 업적 패널 (별도) |
| 상태 관리 | QuestProgress | AchievementProgress |
| 시점 | 조건 충족 시 자동 완료 | 조건 충족 시 자동 달성 |
| 숨김 기능 | 없음 (전체 공개) | 있음 (Hidden 카테고리) |
| 단계 | 없음 (1회성) | 있음 (Bronze/Silver/Gold) |

### 6.2 중복 관리

일부 농장 도전과 업적의 조건이 겹칠 수 있다. 원칙:

- **동일 이벤트에 의해 두 시스템 모두 진행**: 예를 들어 작물 100개 수확 시, 농장 도전 `fc_harvest_100`과 업적 `ach_farming_02` Bronze가 동시에 진행된다.
- **보상은 양쪽 모두 지급**: 중복 달성을 허용하여 플레이어에게 이중 성취감을 제공한다.
- **새로운 업적은 농장 도전과 조건이 완전히 동일하지 않도록 설계**: 업적은 농장 도전보다 더 높은 목표치를 갖거나, 추가 조건(품질, 계절 등)을 부여하여 차별화한다.

---

## 7. 진행도 추적 이벤트

업적 시스템은 다음 게임 이벤트를 구독하여 진행도를 갱신한다.

### 7.1 추적 이벤트 정의

| 이벤트 ID | 설명 | 파라미터 | 관련 업적 |
|-----------|------|----------|----------|
| `OnCropHarvested` | 작물 수확 시 발생 | cropId, quality, season | ach_farming_01~05, ach_hidden_01, ach_hidden_06 |
| `OnItemSold` | 아이템 판매 시 발생 | itemId, quantity, totalGold, isProcessed | ach_economy_01~04 |
| `OnSingleShipment` | 단일 출하 정산 시 발생 | totalGold, itemCount | ach_economy_03, ach_hidden_06 |
| `OnFacilityBuilt` | 시설 건설 완료 시 발생 | facilityId | ach_facility_01~03 |
| `OnItemProcessed` | 가공품 완성 시 발생 | recipeId, quantity | ach_facility_04 |
| `OnToolUpgraded` | 도구 업그레이드 완료 시 발생 | toolId, newTier | ach_tool_01~03 |
| `OnNPCFirstTalk` | NPC 첫 대화 시 발생 | npcId | ach_explorer_01 |
| `OnShopPurchase` | 상점 구매 시 발생 | shopId, itemId, quantity | ach_explorer_02, ach_explorer_04 |
| `OnSeasonChanged` | 계절 전환 시 발생 | newSeason, yearNumber | ach_explorer_03, ach_farming_03 |
| `OnQuestCompleted` | 퀘스트 완료 시 발생 | questId, category | ach_quest_01~04 |
| `OnDailyChallengeCompleted` | 일일 목표 완료 시 발생 | challengeId, consecutiveDays | ach_quest_04 |
| `OnWeatherActive` | 날씨 상태 확인 | weatherType | ach_hidden_01 |
| `OnTimeAdvanced` | 게임 시간 진행 시 발생 | currentHour, playerLocation | ach_hidden_02 |
| `OnToolUsed` | 도구 사용 시 발생 | toolId, usageCount | ach_hidden_03 |
| `OnGoldChanged` | 골드 변동 시 발생 | previousGold, currentGold, delta | ach_hidden_04 |
| `GatheringEvents.OnItemGathered` | 채집물 수집 시 발생 ([TODO] `GatherCount`, `GatherSpeciesCollected`, `GatherSickleUpgraded` enum 추가 후 구독 — -> see `docs/content/achievements.md` 섹션 9.5.1) | itemId, rarity, location | ach_gather_01~05 |
| `AchievementEvents.OnAchievementUnlocked` | 업적 달성 시 발행 (연쇄 해금 체크용) | achievementData | ach_hidden_07 |

### 7.2 복합 조건 추적

일부 업적은 단일 이벤트가 아닌 복합 조건을 추적한다.

| 업적 | 추적 방식 |
|------|----------|
| `ach_farming_03` (사계절 농부) | 4개의 boolean 플래그 (`harvestedInSpring`, `harvestedInSummer`, `harvestedInAutumn`, `harvestedInWinter`). `OnCropHarvested` 이벤트에서 현재 계절을 확인하여 플래그 설정 |
| `ach_farming_04` (작물 도감 완성) | `Set<string> harvestedCropTypes`. `OnCropHarvested`에서 cropId를 Set에 추가. Set 크기가 전체 작물 수와 일치하면 달성 |
| `ach_quest_04` (꾸준한 일꾼) | `int consecutiveDailyDays`. 매일 일일 목표 1개 이상 완료 시 +1, 미완료 시 0으로 리셋. 7 도달 시 달성 |
| `ach_hidden_03` (물만 주는 농부) | 하루 시작 시 `waterOnlyCount = 0`, `otherToolUsed = false` 초기화. 물주기 시 count++. 다른 도구 사용 시 flag = true. 하루 종료 시 `count >= 30 && !otherToolUsed` 확인 |
| `ach_hidden_07` (통합 수집 마스터) | `HandleAchievementChain` 내 하드코딩. `OnAchievementUnlocked` 이벤트 수신 시 `IsUnlocked("ach_fish_04") && IsUnlocked("ach_gather_03")` 양쪽 true 확인 후 해금. ach_fish_04 또는 ach_gather_03 달성 이벤트마다 체크 |

### 7.3 세이브/로드 연동

업적 진행도는 세이브 데이터에 포함되어야 한다.

**저장 데이터 구조**:

```
AchievementSaveData {
    achievements: [
        {
            achievementId: "ach_farming_02",
            currentValue: 523,
            currentTier: "Silver",
            tierHistory: [
                { tier: "Bronze", unlockedDay: 15 },
                { tier: "Silver", unlockedDay: 45 }
            ]
        },
        ...
    ],
    equippedTitle: "숙련 농부",
    unlockedTitles: ["농부", "새싹 농부", "숙련 농부", ...],
    
    // 복합 조건 추적 데이터
    harvestedCropTypes: ["potato", "carrot", "tomato", ...],
    seasonalHarvestFlags: { spring: true, summer: true, autumn: false, winter: false },
    consecutiveDailyDays: 3,
    dailyWaterOnlyCount: 0,
    dailyOtherToolUsed: false
}
```

**로드 시 복원**: 게임 로드 시 `AchievementSaveData`를 읽어 `AchievementProgress` 배열을 복원한다. 복합 조건의 중간 상태(Set, 플래그, 카운터)도 함께 복원한다.

[RISK] 세이브 데이터 마이그레이션: 업적 추가/수정 시 기존 세이브 파일과의 호환성 유지가 필요하다. 신규 업적은 로드 시 `currentValue = 0, currentTier = "None"`으로 초기화해야 한다.

---

## 8. 밸런스 고려사항

### 8.1 업적 달성 타임라인 (예상)

| 시점 | 예상 달성 업적 | 비고 |
|------|---------------|------|
| 봄 (Day 1~28) | 5~7개 | 첫 수확, 첫 판매, 첫 건축 등 초기 업적 |
| 여름 (Day 29~56) | 3~5개 | Bronze 단계 업적, NPC 만남 등 |
| 가을 (Day 57~84) | 2~4개 | Silver 단계, 가공 관련 업적 |
| 겨울 (Day 85~112) | 1~3개 | 사계절 경험, 도구 업그레이드 |
| 2년차~ | 나머지 | Gold 단계, 숨겨진 업적, 작물 도감 완성 |

**설계 의도**: 초반에 업적이 빈번하게 달성되어 성취감의 리듬을 형성하고, 후반으로 갈수록 달성 빈도가 줄어들어 희소성이 높아진다.

### 8.2 업적 보상과 경제 균형

업적 보상 골드 총합(전 업적 완료 기준): (-> see `docs/content/achievements.md` 섹션 13.1 — 확정 합계 11,050G, 40종)

이는 게임 전체 플레이(2년+)의 누적 수입 대비 미미한 수준이다 (-> see `docs/balance/progression-curve.md` 섹션 3 for 자금 곡선). 업적 보상은 "보너스" 성격이며, 경제 밸런스를 좌우하지 않는다.

### 8.3 단계형 업적 임계값 근거

| 업적 | Bronze | Silver | Gold | 근거 |
|------|--------|--------|------|------|
| 수확의 대가 | 50 | 200 | 1,000 | 봄 시즌 일반 플레이어 수확량 약 80~100개 기준. Bronze는 봄 중반, Silver는 여름~가을, Gold는 2년차 달성 목표 |
| 부의 축적 | 5,000G | 25,000G | 100,000G | 초기 시즌 누적 수익 약 3,000~5,000G 기준 (-> see `docs/balance/progression-curve.md` 섹션 3). Bronze는 봄 후반, Gold는 장기 플레이 |
| 가공의 달인 | 20 | 100 | 300 | 가공소 해금(레벨 7) 이후 일일 가공량 2~5개 기준. Bronze는 가공소 해금 1주 내, Gold는 2년차 |
| 쇼핑 마니아 | 30 | 100 | 300 | 매일 평균 1~2회 구매 기준. Bronze는 1시즌, Gold는 3~4시즌 |
| 퀘스트 수집가 | 10 | 30 | 100 | 시즌당 완료 가능 퀘스트 약 15~20개 기준 (메인+NPC+일일 합산). Bronze는 1시즌 초반, Gold는 다년 플레이 |

---

## Cross-references

| 관련 문서 | 관계 |
|----------|------|
| `docs/design.md` | 작물 목록(4.2), 시설 목록(4.6), 키 바인딩(5) 참조 |
| `docs/systems/quest-system.md` | 농장 도전(섹션 6)과의 중복 관리, 퀘스트 완료 이벤트 구독 |
| `docs/balance/progression-curve.md` | XP 테이블(1.3.2), 자금 곡선(3) 참조로 보상 밸런스 검증 |
| `docs/systems/economy-system.md` | 골드 보상이 경제 밸런스에 미치는 영향 검증 |
| `docs/systems/crop-growth.md` | 품질 등급(4.3), 거대 작물(5.1) 조건 참조 |
| `docs/systems/tool-upgrade.md` | 업그레이드 등급(1.1) 조건 참조 |
| `docs/content/npcs.md` | NPC 목록(2.1), 바람이 등장 조건 참조 |
| `docs/systems/time-season.md` | 계절 전환, 날씨 시스템, 기절 시간대 참조 |
| `docs/content/processing-system.md` | 가공품 레시피, 연료 시스템 참조 |
| `docs/systems/farming-system.md` | 비료 효과, 에너지 시스템 참조 |
| `docs/systems/gathering-system.md` | 채집물 27종 목록(섹션 3.9), Legendary 채집물(섹션 3.3~3.7), 채집 낫 등급(섹션 5.2), 채집 숙련도(섹션 4) — 채집가 업적 조건 참조 |
| `docs/content/gathering-items.md` | 채집 아이템 상세 (27종 아이템 속성) — 채집 도감 업적 조건 참조 |
| `docs/systems/achievement-architecture.md` | 기술 아키텍처 (AchievementManager, HandleAchievementChain, 이벤트 구독 구조) |

---

## Open Questions & Risks

### [OPEN] 항목

1. **[OPEN]** `Y`키 할당이 최종 확정되지 않음. 전체 키 바인딩 맵을 별도 문서로 정리하여 충돌 검증 필요 (-> see 섹션 5.1)
2. **[OPEN]** 경제 달인 Gold 단계(100,000G) 달성 가능 시점의 정밀 시뮬레이션 필요 (-> see 섹션 3.2)
3. **[OPEN]** `ach_facility_04` Gold 단계의 특수 레시피 보상 설계 미정 (-> see 섹션 3.3)
4. **[OPEN]** 장식 오브젝트의 비주얼 사양 및 농장 배치 시스템 미설계 (-> see 섹션 4.3)
5. **[OPEN]** NPC 농장 방문 시스템 미설계로 게시판 NPC 반응 연동 불가 (-> see 섹션 4.4)
6. **[OPEN]** 업적 전용 사운드 이펙트(달성음, 칭호 해금음) 사양 미정

### [RISK] 항목

1. **[RISK]** 업적 XP + 퀘스트 XP 합산 시 레벨업 속도 과도 가속 가능성. 출시 전 수치 시뮬레이션 필수 (-> see 섹션 4.1)
2. **[RISK]** 세이브 데이터 마이그레이션: 업적 추가/수정 시 기존 세이브 파일 호환성 유지 필요 (-> see 섹션 7.3)
3. **[RISK]** 숨겨진 업적의 조건이 커뮤니티에 의해 즉시 공유될 수 있어, "발견의 즐거움"이 제한될 수 있음. 패치별 숨겨진 업적 교체를 고려할 수 있으나 세이브 호환 문제 발생

---

> 이 문서는 Claude Code가 자율적으로 작성했습니다.
