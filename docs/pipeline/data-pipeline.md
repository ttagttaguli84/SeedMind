# 데이터 파이프라인 설계 (Data Pipeline)

> 게임 데이터의 분류, ScriptableObject 스키마, 세이브 데이터 구조, 데이터 무결성 규칙, 밸런스 훅, 저장/로드 시스템 아키텍처  
> 작성: Claude Code (Opus) | 2026-04-06  
> 문서 ID: ARC-004

---

## Context

SeedMind의 모든 게임 데이터는 두 가지 범주로 나뉜다: 개발 시점에 확정되는 **정적 데이터**(ScriptableObject)와 플레이 중 변화하는 **동적 데이터**(런타임 상태/세이브). 이 문서는 Part I에서 게임 디자이너 관점의 데이터 분류/필드 정의/세이브 구조/무결성 규칙을, Part II에서 기술 아키텍처 관점의 클래스 설계/저장 시스템/MCP 태스크를 기술한다.

**설계 목표**: 모든 밸런스 조정이 코드 변경 없이 ScriptableObject 에셋 수정만으로 가능해야 한다. 세이브 데이터는 최소한의 정보만 저장하되, 게임 상태를 완벽히 복원할 수 있어야 한다.

---

## Part I -- 게임 디자인

---

### 1. 데이터 분류 체계

#### 1.1 정적 데이터 (ScriptableObject)

개발 시점에 확정되며, 빌드에 포함되어 런타임에 변경되지 않는 데이터. 밸런스 조정 시 에셋만 수정하면 된다.

| 카테고리 | SO 타입 | 에셋 수 (예상) | 설명 | Canonical 문서 |
|----------|---------|---------------|------|---------------|
| 작물 | CropData | 8 | 작물별 성장/가격/시각 데이터 | `docs/systems/farming-architecture.md` 4.1 |
| 비료 | FertilizerData | 4 | 비료별 효과/가격 데이터 | `docs/systems/farming-architecture.md` 4.2 |
| 도구 | ToolData | 17~22 (3종×5등급 + 2종 단일, 확장 시 최대 22) | 도구별 성능/업그레이드 데이터 | 본 문서 섹션 2.3 (신규) |
| 시설 | BuildingData | 4 | 시설별 비용/효과/해금 조건 | 본 문서 섹션 2.4 (신규) |
| 경제 설정 | EconomyConfig | 1 | 글로벌 경제 파라미터 | `docs/systems/economy-architecture.md` 4.1 |
| 가격 | PriceData | ~20 | 품목별 기본가/계절 보정/수급 설정 | `docs/systems/economy-architecture.md` 4.2 |
| 상점 | ShopData | 3 | 상점별 품목/운영 시간 | `docs/systems/economy-architecture.md` 4.3 |
| 시간 설정 | TimeConfig | 1 | 시간 진행 파라미터 | `docs/systems/time-season-architecture.md` 2.2 |
| 계절 | SeasonData | 4 | 계절별 환경/게임플레이 설정 | `docs/systems/time-season-architecture.md` 2.1 |
| 날씨 | WeatherData | 4 | 계절별 날씨 확률/효과 | `docs/systems/time-season-architecture.md` 2.4 |
| 가공 레시피 | ProcessingRecipeData | 55 | 가공 유형별 배수/시간 (작물 42종 + 채집물 13종, FIX-083, -> see `docs/content/processing-system.md` 섹션 3.8) | 본 문서 섹션 2.5 |
| 레벨 | ProgressionData | 1 | 레벨별 경험치/해금/마일스톤 설정 (LevelConfig 대체, BAL-002) | `docs/systems/progression-architecture.md` 섹션 2.1 |
| 튜토리얼 시퀀스 | TutorialSequenceData | 5 | 메인 + 시스템 튜토리얼 시퀀스 (DES-006) | `docs/systems/tutorial-architecture.md` 섹션 4.1 |
| 튜토리얼 단계 | TutorialStepData | ~20 | 시퀀스별 튜토리얼 단계 (DES-006) | `docs/systems/tutorial-architecture.md` 섹션 4.2 |
| 상황별 힌트 | ContextHintData | ~8 | 상황 감지 자동 힌트 (DES-006) | `docs/systems/tutorial-architecture.md` 섹션 8.2 |
| 채집 포인트 | GatheringPointData | 5 | 포인트 유형별 아이템 풀/리스폰 설정 (ARC-033) | `docs/systems/gathering-architecture.md` 섹션 2.1 |
| 채집 아이템 | GatheringItemData | 27 | 채집 아이템별 가격/희귀도/계절 데이터 (ARC-033) | `docs/systems/gathering-architecture.md` 섹션 2.2 |
| 채집 설정 | GatheringConfig | 1 | 채집 밸런스 파라미터 (에너지/숙련도/품질 등, ARC-033) | `docs/systems/gathering-architecture.md` 섹션 2.3 |
| 채집 도감 | GatheringCatalogData | 27 | 채집 아이템 도감 정적 표시 정보(힌트/보상, ARC-041/ARC-045) | `docs/systems/collection-architecture.md` 섹션 3 |

**총 예상 에셋 수**: ~217개 SO 에셋 (ARC-033 +33, ARC-045 GatheringCatalogData 27 추가, 갱신)

#### 1.2 동적 데이터 (런타임/세이브)

플레이 중 변화하며, 세이브 파일에 기록되어야 하는 데이터.

| 카테고리 | 데이터 클래스 | 설명 | 저장 필수 |
|----------|-------------|------|-----------|
| 플레이어 상태 | PlayerSaveData | 위치, 에너지, 소지 골드, 레벨, 경험치 | O |
| 인벤토리 | InventorySaveData | 소지 아이템 목록 (종류, 수량, 품질) — Part II에서는 PlayerSaveData에 포함 | O |
| 농장 타일 | FarmTileSaveData[] | 전체 타일 상태, 작물 인스턴스, 토양 품질 | O |
| 시간 | TimeSaveData | 현재 연/계절/일/시간 | O |
| 날씨 | WeatherSaveData | 현재 날씨, 내일 예보, 난수 시드 | O |
| 경제 | EconomySaveData | 골드, 거래 로그, 수급 누적 | O |
| 시설 | BuildingSaveData[] | 건설된 시설 목록, 위치, 가동 상태 | O |
| 가공 | ProcessingSaveData[] | 진행 중인 가공 작업 (슬롯, 잔여 시간) | O |
| 해금 상태 | UnlockSaveData | 해금된 작물/도구/시설 목록 | O |
| 상점 재고 | ShopStockSaveData[] | 일일 재고 잔여량 | O |
| 튜토리얼 진행 | TutorialSaveData | 완료 시퀀스/단계, 힌트 쿨다운 (DES-006) | O |
| 출하함 대기 목록 | ShippingBinSaveData | 당일 출하함에 넣은 아이템 (정산 전) | O |
| 낚시 | FishingSaveData | 낚시 진행 상태, 포획 기록 (→ see docs/systems/fishing-architecture.md, FIX-051) | O |
| NPC 관계 | (향후 확장) | 호감도 등 | 향후 |

#### 1.3 파생 데이터 (저장 불필요)

정적 + 동적 데이터로부터 실시간 계산되는 값. 저장하지 않는다.

| 데이터 | 계산 원천 | 설명 |
|--------|----------|------|
| 현재 판매가 | PriceData + 계절/수급/날씨/품질 보정 | 매일 아침 재계산 |
| 작물 성장 단계 | CropInstance.currentGrowthDays / CropData.growthDays | 렌더링용 |
| 시간대 (DayPhase) | TimeManager.currentHour | 조명/NPC 스케줄용 |
| 요일 | currentDay % 7 | 상점 휴무 판정용 |
| 총 경과 일수 | (year-1)*112 + season*28 + day | 수급 보정, 통계용 — **본 문서가 canonical 정의** |

---

### 2. ScriptableObject 필드 정의

이미 canonical 문서에 정의된 SO는 참조만 기재한다. 이 문서에서 **신규 정의**하는 SO만 상세 필드를 명시한다.

#### 2.1 CropData (기존)

(-> see `docs/systems/farming-architecture.md` 섹션 4.1 for canonical 필드 정의)

필드 요약: cropName, cropId, icon, seedPrice, sellPrice, growthDays, growthStageCount, allowedSeasons, baseYield, qualityChance, unlockLevel, growthStagePrefabs, soilMaterial

**데이터 파이프라인 추가 필드** (기존 정의 확장):

| 필드 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| cropCategory | CropCategory enum | Vegetable | 가공 유형 결정 (-> see `docs/systems/economy-system.md` 섹션 2.5) |
| isRepeating | bool | false | 반복 수확 가능 여부 (딸기 등) |
| regrowDays | int | 0 | 재수확까지 일수 (isRepeating=true일 때만 유효) |
| giantCropChance | float | 0.0 | 거대 작물 변이 확률 (-> see `docs/systems/crop-growth.md` 섹션 5.1) |
| giantCropPrefab | GameObject | null | 거대 작물 프리팹 참조 |
| harvestParticle | GameObject | null | 수확 시 파티클 프리팹 |
| description | string | "" | UI 표시용 작물 설명 텍스트 |

```
[Canonical 값 참조]
작물별 수치(seedPrice, sellPrice, growthDays, unlockLevel) -> see docs/design.md 섹션 4.2
작물별 재배 가능 계절(allowedSeasons) -> see docs/systems/crop-growth.md 섹션 3.1
```

**CropCategory enum** (신규):

| 값 | 설명 | 가공 가능 유형 |
|----|------|--------------|
| Vegetable | 채소 (감자, 당근, 옥수수, 호박) | 잼, 절임 |
| Fruit | 과일 (딸기, 수박) | 잼, 주스 |
| FruitVegetable | 과채겸용 (토마토) | 잼, 주스, 절임 |
| Special | 특수 (해바라기) | 가공 불가 |

(-> see `docs/systems/economy-system.md` 섹션 2.5 작물 분류 테이블)

#### 2.2 FertilizerData (기존)

(-> see `docs/systems/farming-architecture.md` 섹션 4.2 for canonical 필드 정의)

필드 요약: fertilizerName, fertilizerId, buyPrice, growthMultiplier, qualityBonus, effectDuration, soilTintColor

**데이터 파이프라인 추가 필드**:

| 필드 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| description | string | "" | UI 표시용 비료 설명 텍스트 |
| icon | Sprite | null | UI 아이콘 |
| isPermanent | bool | false | 영구 효과 여부 (유기 비료) |
| unlockLevel | int | 0 | 해금 레벨 (-> see `docs/systems/farming-system.md` 섹션 5.1) |
| applyParticle | GameObject | null | 비료 적용 시 파티클 프리팹 |

#### 2.3 ToolData (기존 + 확장)

기본 필드는 `docs/systems/farming-architecture.md` 섹션 4.3에 정의되어 있다. 여기서는 도구 업그레이드 체계와 통합하여 완전한 필드 세트를 정의한다.

(-> see `docs/systems/farming-architecture.md` 섹션 4.3 for 기존 필드)

**데이터 파이프라인 추가/확장 필드**:

| 필드 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| toolId | string | "" | 코드용 고유 식별자 (예: "hoe_iron") |
| description | string | "" | UI 표시용 도구 설명 |
| energyCost | int | 1 | 사용 당 에너지 소모 (-> see `docs/systems/farming-system.md` 섹션 3.1) |
| cooldown | float | 0.5 | 사용 후 쿨다운 시간(초) (-> see `docs/systems/farming-system.md` 섹션 3.3) |
| animationClip | AnimationClip | null | 도구 사용 애니메이션 참조 |
| useSFX | AudioClip | null | 도구 사용 효과음 |
| upgradeMaterials | UpgradeMaterial[] | empty | 업그레이드 필요 재료 목록 |
| upgradeGoldCost | int | 0 | 업그레이드 골드 비용 (-> see `docs/systems/tool-upgrade.md` 섹션 2.1) |
| upgradeTimeDays | int | 0 | 업그레이드 소요 일수 (-> see `docs/systems/tool-upgrade.md` 섹션 2.1) |
| specialEffect | string | "" | 등급별 특수 효과 설명 (-> see `docs/systems/tool-upgrade.md` 섹션 3) |

**UpgradeMaterial 구조체** (신규):

| 필드 | 타입 | 설명 |
|------|------|------|
| materialId | string | 재료 식별자 (예: "copper_ore") |
| materialName | string | 표시 이름 (예: "구리 광석") |
| quantity | int | 필요 수량 |

**도구 등급 에셋 구조**:

각 도구는 등급별로 별도 SO 에셋으로 존재한다. nextTier 필드로 체인 형태로 연결된다.

```
SO_Tool_Hoe_Basic        (tier=1, nextTier=SO_Tool_Hoe_Reinforced)
SO_Tool_Hoe_Reinforced   (tier=2, nextTier=SO_Tool_Hoe_Legendary)
SO_Tool_Hoe_Legendary    (tier=3, nextTier=null)
```

(→ see `docs/systems/tool-upgrade.md` 섹션 1.1 for 3단계 등급 체계 canonical 정의)

도구 5종(호미, 물뿌리개, 씨앗봉투, 낫, 손) 중 업그레이드 가능한 것은 호미, 물뿌리개, 낫 3종 x 3등급 = 9개. 씨앗봉투, 손은 각 1개 = 총 11개 에셋.

[OPEN] 씨앗봉투의 업그레이드 가능 여부. 현재는 업그레이드 없음으로 설계되어 있으나, "한 번에 여러 타일에 심기" 같은 확장 가능성 있음.

#### 2.4 BuildingData (신규)

시설(건물) 데이터를 정의하는 ScriptableObject. 기존에 클래스 레벨의 정의만 있었으므로 여기서 전체 필드를 신규 정의한다.

**참고**: BuildingData는 `GameDataSO`를 상속하므로, `dataId`/`displayName`/`icon` 3 필드는 부모 클래스에서 제공된다. (-> see `docs/systems/facilities-architecture.md` 섹션 2.2 for C# 클래스 정의)

| 필드 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| dataId (부모) | string | "" | 코드용 고유 식별자 (예: "greenhouse") — GameDataSO 상속 |
| displayName (부모) | string | "" | 표시 이름 (예: "온실") — GameDataSO 상속 |
| description | string | "" | UI 표시용 설명 텍스트 |
| icon (부모) | Sprite | null | UI 아이콘 — GameDataSO 상속 |
| buildCost | int | 0 | 건설 비용 (-> see `docs/design.md` 섹션 4.6) |
| requiredLevel | int | 0 | 해금 레벨 (-> see `docs/design.md` 섹션 4.6) |
| buildTimeDays | int | 1 | 건설 소요 일수 |
| tileSize | Vector2Int | (2,2) | 점유 타일 크기 (가로x세로) |
| placementRules | PlacementRule | FarmOnly | 배치 가능 위치 제한 |
| prefab | GameObject | null | 시설 3D 모델 프리팹 |
| constructionPrefab | GameObject | null | 건설 중 모델 프리팹 |
| effectType | BuildingEffectType | None | 시설 효과 종류 |
| effectRadius | int | 0 | 효과 적용 반경 (물탱크 등 인접 효과) |
| effectValue | float | 0 | 효과 수치 (효과별 의미 상이) |
| maxUpgradeLevel | int | 0 | 업그레이드 가능 횟수 (0 = 업그레이드 없음) |
| upgradeCosts | int[] | empty | 단계별 업그레이드 비용 |

**BuildingEffectType enum** (신규):

| 값 | 설명 | effectValue 의미 |
|----|------|-----------------|
| None | 효과 없음 | - |
| AutoWater | 인접 타일 자동 물주기 (물탱크) | effectRadius 범위 내 타일 자동 Watered |
| SeasonBypass | 계절 무관 재배 (온실) | 내부 타일에서 모든 계절 작물 재배 가능 |
| Storage | 작물 저장 (창고) | 최대 저장 슬롯 수 |
| Processing | 작물 가공 (가공소) | 초기 가공 슬롯 수 |

**PlacementRule enum** (신규):

| 값 | 설명 |
|----|------|
| FarmOnly | 농장 그리드 내부에만 배치 가능 |
| FarmEdge | 농장 그리드 가장자리에만 배치 가능 |
| Anywhere | 농장 영역 어디든 배치 가능 |

**시설별 에셋 데이터** (tileSize/buildTimeDays/effectRadius 등 콘텐츠 파라미터는 `docs/content/facilities.md` canonical 참조 — PATTERN-007):

| 에셋 이름 | buildingId | tileSize | effectType | effectRadius | buildTimeDays |
|-----------|------------|----------|------------|--------------|--------------|
| SO_Bldg_WaterTank | water_tank | (→ see `docs/content/facilities.md` 섹션 3.1) | AutoWater | (→ see `docs/content/facilities.md` 섹션 3.2) | (→ see `docs/content/facilities.md` 섹션 3.1) |
| SO_Bldg_Greenhouse | greenhouse | (→ see `docs/content/facilities.md` 섹션 4.1) | SeasonBypass | 0 (내부만) | (→ see `docs/content/facilities.md` 섹션 4.1) |
| SO_Bldg_Storage | storage | (→ see `docs/content/facilities.md` 섹션 5.1) | Storage | 0 | (→ see `docs/content/facilities.md` 섹션 5.1) |
| SO_Bldg_Processor | processor | (→ see `docs/content/facilities.md` 섹션 6.1) | Processing | 0 | (→ see `docs/content/facilities.md` 섹션 6.1) |

**물탱크 상세**: effectRadius 범위 내의 모든 경작 타일에 매일 아침 자동으로 Watered 상태를 부여한다. 물뿌리개로 물을 줄 필요가 없어진다. 수치: (→ see `docs/content/facilities.md` 섹션 3.1, 3.2).

**온실 상세**: SeasonBypass 효과로 겨울에도 모든 작물 재배 가능. 비계절 작물은 성장 패널티 적용. 면적 및 내부 경작 가능 타일 수치: (→ see `docs/content/facilities.md` 섹션 4.1, 4.3).

**창고 상세**: effectValue = 창고 1동당 슬롯 수 (→ see `docs/content/facilities.md` 섹션 5.1, 5.2). tileSize: (→ see `docs/content/facilities.md` 섹션 5.1).

**가공소 상세**: effectValue = 초기 가공 슬롯 수 (→ see `docs/systems/economy-system.md` 섹션 2.5). tileSize: (→ see `docs/content/facilities.md` 섹션 6.1).

[OPEN] 시설 업그레이드 시스템(upgradeCosts)의 상세 설계 미정. 예: 창고 슬롯 확장, 온실 크기 확장 등.

#### 2.5 ProcessingRecipeData (신규)

가공소에서 사용하는 가공 레시피를 정의하는 ScriptableObject. `GameDataSO`를 상속하며 `dataId`, `displayName`, `icon`은 부모 클래스에서 제공된다.

(-> see `docs/systems/processing-architecture.md` 섹션 2.1 for canonical 클래스 정의 및 PATTERN-005 검증)

| 필드 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| dataId | string | "" | 코드용 고유 식별자 (예: "recipe_jam_potato") — GameDataSO 상속 |
| displayName | string | "" | 표시 이름 (예: "감자 잼") — GameDataSO 상속 |
| icon | Sprite | null | 가공품 UI 아이콘 — GameDataSO 상속 |
| description | string | "" | UI 설명 텍스트 |
| processingType | ProcessingType | Jam | 가공 유형 (-> see ProcessingType enum 아래) |
| inputs | RecipeInput[] | [] | 복합 재료 배열 (FIX-083) — `[{itemId, quantity}, ...]`, 단일 재료도 배열 1개 원소 |
| priceMultiplier | float | 0.0 | 원재료 기본 판매가 대비 배수; 0이면 priceBonus만 사용 (-> see `docs/systems/economy-system.md` 섹션 2.5) |
| priceBonus | int | 0 | 고정 가격 보너스 (-> see `docs/systems/economy-system.md` 섹션 2.5) |
| processingTimeHours | float | 0.0 | 가공 소요 시간, 게임 내 시간 (-> see `docs/content/processing-system.md` 섹션 3) |
| fuelCost | int | 0 | 연료 비용 (0 = 연료 불필요, -> see `docs/content/processing-system.md` 섹션 4) |
| requiredFacilityTier | int | 0 | 필요 가공소 등급 (0 = Tier 1) |
| unlockType | RecipeUnlockType | 0 | 해금 조건 유형 (FIX-083): 0=FacilityBuild, 1=PlayerLevel, 2=GatheringMastery |
| unlockValue | int | 0 | 해금 조건 값 (숙련도 레벨 등, FIX-083) |
| outputItemId | string | "" | 출력 아이템 식별자 (예: "item_wild_berry_jam") |
| outputQuantity | int | 1 | 출력 수량 |

**ProcessingType enum** (FIX-083 확장 후):

| 값 | 설명 | 가공소 | 수치 참조 |
|----|------|--------|---------|
| Jam | 잼 | 가공소 (일반) | (-> see `docs/content/processing-system.md` 섹션 3.1) |
| Juice | 주스 | 가공소 (일반) | (-> see `docs/content/processing-system.md` 섹션 3.1) |
| Pickle | 절임 | 가공소 (일반) | (-> see `docs/content/processing-system.md` 섹션 3.1) |
| Mill | 제분 | 제분소 (building_mill) | (-> see `docs/content/processing-system.md` 섹션 3.2) |
| Fermentation | 발효 | 발효실 (building_fermentation) | (-> see `docs/content/processing-system.md` 섹션 3.3) |
| Bake | 요리 | 베이커리 (building_bakery) | (-> see `docs/content/processing-system.md` 섹션 3.4) |
| Cheese | 유제품 | 치즈 공방 (building_cheese_workshop) | (-> see `docs/content/processing-system.md` 섹션 3.6) |
| Dried | 건조 | 가공소 (일반) | (-> see `docs/content/processing-system.md` 섹션 3.7, FIX-083) |
| Tea | 차/음료 | 가공소 (일반) | (-> see `docs/content/processing-system.md` 섹션 3.7, FIX-083) |
| Pill | 환/약제 | 가공소 (일반) | (-> see `docs/content/processing-system.md` 섹션 3.7, FIX-083) |
| Food | 일반 음식 | 가공소 (일반) | (-> see `docs/content/processing-system.md` 섹션 3.7, FIX-083) |

(-> see `docs/content/processing-system.md` 섹션 3 for canonical 가공 공식, 파라미터, 레시피 전체 목록)
(-> see `docs/systems/economy-system.md` 섹션 2.5 for 가공 배수 공식 canonical)

**레시피 에셋 목록**: (-> see `docs/content/processing-system.md` 섹션 3.8 for 전체 레시피 55종 및 에셋 ID 목록 canonical)

총 55개 레시피 에셋 (가공소 30종 + 제분소 4종 + 발효실 7종 + 베이커리 9종 + 치즈 공방 5종, FIX-083 채집물 레시피 13종 포함). 에셋명 패턴: `SO_Recipe_<Type>_<ID>` (채집물 레시피: `SO_Recipe_Gather_01` ~ `SO_Recipe_Gather_13`).

[OPEN] 가공 레시피를 작물별 개별 에셋으로 할지, ProcessingType별 에셋(3개)으로 만들고 입력 카테고리로 필터할지. 개별 에셋이면 작물별 커스텀 아이콘/이름이 가능하지만 에셋 수가 많아진다. 현재는 개별 에셋 방식 채택.

#### 2.6 ProgressionData (BAL-002, LevelConfig 대체)

플레이어 레벨/경험치/해금/마일스톤 시스템의 설정을 정의하는 ScriptableObject. 기존 LevelConfig를 ProgressionData로 확장한다 (→ see `docs/systems/progression-architecture.md` 섹션 2.1 for 전체 필드 정의).

| 필드 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| maxLevel | int | 10 | 최대 레벨 (-> see `docs/design.md` 섹션 4.5) |
| expPerLevel | int[] | [10개] | 레벨별 필요 경험치 |
| harvestExpBase | int | 5 | 작물 수확 시 기본 경험치 |
| harvestExpPerGrowthDay | float | 1.0 | 성장일수당 추가 경험치 배수 |
| qualityExpBonus | float[] | [4개] | 품질별 경험치 보너스 배수 |

**레벨별 필요 경험치**: (→ see `docs/balance/progression-curve.md` 섹션 2.4.1 for canonical XP 테이블)

**경험치 획득 공식**:

```
수확_경험치 = (harvestExpBase + floor(cropData.growthDays * harvestExpPerGrowthDay)) * qualityExpBonus[quality]
```

[RISK] 위 공식(harvestExpBase=5, harvestExpPerGrowthDay=1.0)은 작물별 실제 XP 수치의 산출 기준선이다. 최종 작물별 XP는 progression-curve.md 섹션 1.2.1에서 수동 조정하여 확정된다. 공식 결과와 확정 XP 간 차이가 있는 경우, 확정 XP가 우선한다 (→ see `docs/balance/progression-curve.md` 섹션 1.2.1).

**품질별 경험치 보너스 배수** (→ see `docs/balance/progression-curve.md` 섹션 1.2.2 for canonical 값)

[RISK] 레벨 8~10의 해금 콘텐츠가 미정. 향후 CON 계열 태스크에서 확정 필요.

#### 2.7 EconomyConfig, PriceData, ShopData (기존)

(-> see `docs/systems/economy-architecture.md` 섹션 4.1~4.3 for canonical 필드 정의)

추가 필드 없음. 기존 정의가 데이터 파이프라인 요구사항을 충족한다.

#### 2.8 TimeConfig, SeasonData, WeatherData (기존)

(-> see `docs/systems/time-season-architecture.md` 섹션 2.1~2.4 for canonical 필드 정의)

추가 필드 없음. 기존 정의가 데이터 파이프라인 요구사항을 충족한다.

#### 2.9 InventoryItemData 설계 방향 (신규)

인벤토리에 저장 가능한 아이템의 공통 정보. 현재 두 가지 구현 방식을 검토 중이다.

**방식 A: 별도 SO 에셋** -- 각 아이템(작물, 씨앗, 도구, 비료, 가공품)마다 InventoryItemData SO를 별도로 생성.

**방식 B: IInventoryItem 인터페이스** -- CropData, ToolData, FertilizerData 등이 공통 인터페이스를 구현.

방식 B가 에셋 수를 줄이고 기존 SO와의 참조가 자연스러우므로 권장한다.

**IInventoryItem 공통 속성** (방식 B 기준):

| 속성 | 타입 | 설명 |
|------|------|------|
| ItemId | string | 전체 게임에서 유일한 식별자 |
| ItemName | string | 표시 이름 |
| ItemType | ItemType enum | 아이템 분류 |
| Icon | Sprite | UI 아이콘 |
| MaxStackSize | int | 슬롯당 최대 스택 수 |
| Sellable | bool | 판매 가능 여부 |

**ItemType enum** (신규):

| 값 | 설명 | 스택 가능 | 비고 |
|----|------|----------|------|
| Crop | 수확 작물 | O (99) | 품질별 별도 슬롯 |
| Seed | 씨앗 | O (99) | - |
| Tool | 도구 | X (1) | 도구 슬롯에만 배치 |
| Fertilizer | 비료 | O (50) | - |
| Processed | 가공품 | O (50) | - |
| Material | 재료 (광석 등) | O (99) | 향후 확장 |
| Fish | 물고기 | O (99) | 품질별 별도 슬롯 (→ see docs/systems/fishing-system.md, FIX-053) |
| Gathered | 채집물 | O (99) | 품질별 별도 슬롯 (→ see docs/systems/gathering-system.md, ARC-031) |
| Consumable | 소비형 아이템 | O (10) | 여행 상인 소비품(에너지 토닉, 성장 촉진제, 행운의 부적) — Special과 구분 |
| Special | 특수 아이템 | X (1) | 이벤트 보상 등 |

[OPEN] 최종 구현 방식(방식 A vs B)은 아키텍트 에이전트와 협의하여 Part II에서 확정.

#### 2.10 GatheringPointData (신규, ARC-033)

채집 포인트 유형별 데이터를 정의하는 ScriptableObject. 숲 바닥, 덤불, 동굴 입구, 연못가, 초원의 5가지 유형이 존재한다. `ScriptableObject`를 직접 상속한다.

(-> see `docs/systems/gathering-architecture.md` 섹션 2.1 for canonical 클래스 정의 및 내부 struct)

| 필드 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| pointId | string | "" | 코드용 고유 식별자 (예: "gp_forest_01") |
| displayName | string | "" | 표시 이름 (예: "숲 덤불") |
| description | string | "" | 포인트 설명 |
| zoneId | string | "" | 소속 Zone (예: "zone_east_forest") |
| requiredZoneUnlocked | bool | true | Zone 해금 필요 여부 |
| availableItems | GatheringItemEntry[] | [] | 이 포인트에서 등장하는 아이템 풀 (-> see `docs/systems/gathering-system.md` 섹션 3) |
| seasonOverrides | SeasonalItemOverride[] | [] | 계절별 아이템 풀 교체 (-> see `docs/systems/gathering-system.md` 섹션 3) |
| respawnDays | int | 0 | 수집 후 재생성까지 일수 (-> see `docs/systems/gathering-system.md` 섹션 2) |
| respawnVariance | int | 0 | 재생성 일수 변동폭 (-> see `docs/systems/gathering-system.md` 섹션 2) |
| pointPrefab | GameObject | null | 채집 포인트 프리팹 참조 |
| depletedPrefab | GameObject | null | 소진 상태 프리팹 (빈 덤불 등) |
| gatherVFX | GameObject | null | 채집 시 이펙트 |

**GatheringItemEntry** (Inner struct):

| 필드 | 타입 | 설명 |
|------|------|------|
| item | GatheringItemData | 아이템 SO 참조 |
| weight | float | 절대 가중치 (0.0~1.0 범위가 아닌 절대값) |

**SeasonalItemOverride** (Inner class):

| 필드 | 타입 | 설명 |
|------|------|------|
| season | Season | 적용 계절 |
| overrideItems | GatheringItemEntry[] | 해당 계절 전용 아이템 풀 |

**포인트 유형별 에셋 목록** (콘텐츠 파라미터는 canonical 참조 — PATTERN-007):

| 에셋 이름 | pointId | 포인트 수 | Zone | 아이템 풀 참조 |
|-----------|---------|----------|------|--------------|
| SO_GatherPoint_ForestFloor | gather_forest_floor | (-> see `docs/systems/gathering-system.md` 섹션 2) | Zone D | (-> see `docs/systems/gathering-system.md` 섹션 3) |
| SO_GatherPoint_Bush | gather_bush | (-> see `docs/systems/gathering-system.md` 섹션 2) | Zone D | (-> see `docs/systems/gathering-system.md` 섹션 3) |
| SO_GatherPoint_CaveEntrance | gather_cave_entrance | (-> see `docs/systems/gathering-system.md` 섹션 2) | Zone D | (-> see `docs/systems/gathering-system.md` 섹션 3.7) |
| SO_GatherPoint_PondEdge | gather_pond_edge | (-> see `docs/systems/gathering-system.md` 섹션 2) | Zone F | (-> see `docs/systems/gathering-system.md` 섹션 3) |
| SO_GatherPoint_Meadow | gather_meadow | (-> see `docs/systems/gathering-system.md` 섹션 2) | Zone E | (-> see `docs/systems/gathering-system.md` 섹션 3) |

#### 2.11 GatheringItemData (신규, ARC-033)

개별 채집 아이템을 정의하는 ScriptableObject. `GameDataSO`를 상속하므로 `dataId`/`displayName`/`description`/`icon` 4 필드는 부모 클래스에서 제공된다. `IInventoryItem` 인터페이스를 구현한다.

(-> see `docs/systems/gathering-architecture.md` 섹션 2.2 for canonical 클래스 정의)

| 필드 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| dataId (부모) | string | "" | 코드용 고유 식별자 (예: "gather_mushroom") — GameDataSO 상속 |
| displayName (부모) | string | "" | 표시 이름 (예: "숲 버섯") — GameDataSO 상속 |
| description (부모) | string | "" | 아이템 설명 텍스트 — GameDataSO 상속 |
| icon (부모) | Sprite | null | 인벤토리 아이콘 — GameDataSO 상속 |
| gatheringCategory | GatheringCategory | Flower | 채집 카테고리 (Flower/Berry/Mushroom/Herb/Mineral/Special) |
| rarity | GatheringRarity | Common | 희귀도 (Common/Uncommon/Rare/Legendary) |
| basePrice | int | 0 | 기본 판매가 (-> see `docs/systems/gathering-system.md` 섹션 3) |
| seasonAvailability | SeasonFlag | 0 | 출현 계절 비트 플래그 (-> see `docs/systems/gathering-system.md` 섹션 3) |
| weatherBonus | WeatherFlag | 0 | 보너스 날씨 (-> see `docs/systems/gathering-system.md` 섹션 3) |
| baseQuantityRange | Vector2Int | (1,1) | 기본 수량 범위 (-> see `docs/systems/gathering-system.md` 섹션 3) |
| qualityEnabled | bool | true | 품질 시스템 적용 여부 (false이면 항상 Normal) |
| maxStackSize | int | 0 | 인벤토리 스택 한도 (-> see 본 문서 섹션 2.9) |
| expReward | int | 0 | 채집 시 XP (-> see `docs/balance/progression-curve.md`) |
| gatherTimeSec | float | 0 | 채집 소요 시간 초 (-> see `docs/systems/gathering-system.md` 섹션 2) |
| requiredTool | ToolType | None | 필요 도구 (-> see `docs/systems/gathering-system.md` 섹션 2) |
| minProficiencyLevel | int | 0 | 최소 채집 숙련도 레벨 (-> see `docs/systems/gathering-system.md` 섹션 4) |

**IInventoryItem 구현 속성**:

| 속성 | 반환값 | 설명 |
|------|--------|------|
| ItemType | ItemType.Gathered | 아이템 분류 |
| MaxStackSize | maxStackSize | 슬롯당 최대 스택 |
| Sellable | true | 항상 판매 가능 |

**GatheringCategory enum**:

| 값 | 설명 |
|----|------|
| Flower | 꽃 -- 선물, 장식, 일부 가공 재료 |
| Berry | 열매 -- 식재료, 소모품 가공 |
| Mushroom | 버섯 -- 요리 재료, 비 오는 날 출현 증가 |
| Herb | 허브 -- 약품/향신료 가공 |
| Mineral | 광물/보석 -- 희귀, 높은 판매가 |
| Special | 특수 -- 퀘스트/축제 관련 한정 아이템 |

**GatheringRarity enum**:

| 값 | 설명 |
|----|------|
| Common | 흔함 -- 높은 출현율 |
| Uncommon | 보통 -- 중간 출현율 |
| Rare | 희귀 -- 낮은 출현율 |
| Legendary | 전설 -- 매우 희귀, 특수 조건 필요 |

[OPEN] `GatheringRarity`를 `FishRarity`와 통합하여 `SeedMind.ItemRarity`로 상위 네임스페이스에 둘지 여부 (-> see `docs/systems/gathering-architecture.md` 섹션 2.2).

**아이템 에셋 목록**: 총 27종 (봄 6종 + 여름 6종 + 가을 6종 + 겨울 3종 + 광물 6종). 에셋명 패턴: `SO_GatherItem_<ID>`. 전체 아이템 목록 및 수치: (-> see `docs/systems/gathering-system.md` 섹션 3 for canonical 아이템 카탈로그)

#### 2.12 GatheringConfig (신규, ARC-033)

채집 시스템의 글로벌 밸런스 파라미터를 정의하는 ScriptableObject. 게임 전체에서 1개 에셋만 존재한다.

(-> see `docs/systems/gathering-architecture.md` 섹션 2.3 for canonical 클래스 정의)

| 필드 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| baseGatherEnergy | int | 0 | 채집 1회 에너지 소모 (-> see `docs/systems/gathering-system.md` 섹션 2) |
| gatherAnimationDuration | float | 0 | 채집 애니메이션 시간 초 |
| maxActivePointsPerZone | int | 0 | Zone당 최대 활성 포인트 (-> see `docs/systems/gathering-system.md` 섹션 2) |
| defaultRespawnDays | int | 0 | 기본 재생성 일수 (-> see `docs/systems/gathering-system.md` 섹션 2) |
| seasonalRefreshOnChange | bool | false | 계절 전환 시 전체 리프레시 여부 |
| qualityThresholds | float[] | [] | [Normal, Silver, Gold, Iridium] 품질 판정 경계값 (-> see `docs/systems/gathering-system.md` 섹션 4.5) |
| proficiencyXPThresholds | int[] | [] | 레벨별 필요 누적 XP (-> see `docs/systems/gathering-system.md` 섹션 4.2) |
| proficiencyMaxLevel | int | 0 | 최대 레벨 (-> see `docs/systems/gathering-system.md` 섹션 4.2) |
| gatherXPByRarity | int[] | [] | [Common, Uncommon, Rare, Legendary] 희귀도별 XP (-> see `docs/systems/gathering-system.md` 섹션 4.3) |
| bonusQuantityByLevel | float[] | [] | 레벨별 추가 수량 확률 (-> see `docs/systems/gathering-system.md` 섹션 4.4) |
| rarityBonusByLevel | float[] | [] | 레벨별 희귀 아이템 확률 보정 (-> see `docs/systems/gathering-system.md` 섹션 4.4) |
| energyCostReductionByLevel | int[] | [] | 레벨별 에너지 소모 감소 (-> see `docs/systems/gathering-system.md` 섹션 4.2) |
| maxQualityByLevel | CropQuality[] | [] | 레벨별 최대 품질 (-> see `docs/systems/gathering-system.md` 섹션 4.5) |
| gatherSpeedMultiplierByLevel | float[] | [] | 레벨별 채집 속도 배율 (-> see `docs/systems/gathering-system.md` 섹션 4.4) |

**에셋**: `SO_GatherConfig_Default` (1개)

---

#### 2.13 GatheringCatalogData (신규, ARC-045)

채집 도감의 정적 표시 정보를 정의하는 ScriptableObject. GatheringItemData와 1:1 대응하며, 도감 UI에 표시할 힌트·보상 정보를 담는다. 게임 전체에서 아이템 수(27종)만큼 에셋이 존재한다.

(-> see `docs/systems/collection-architecture.md` 섹션 3 for canonical 클래스 정의)

| 필드 | 타입 | 기본값 | 설명 |
|------|------|--------|------|
| itemId | string | "" | GatheringItemData.dataId와 동일 키 (-> see `docs/systems/gathering-system.md` 섹션 3) |
| displayName | string | "" | 도감 표시명 (-> see `docs/content/gathering-items.md`) |
| hintLocked | string | "" | 미발견 시 표시 힌트 텍스트 (-> see `docs/content/gathering-items.md`) |
| descriptionUnlocked | string | "" | 발견 후 표시 설명 (-> see `docs/content/gathering-items.md`) |
| rarity | GatheringRarity | Common | 희귀도 enum (-> see `docs/systems/gathering-architecture.md` 섹션 2.2) |
| firstDiscoverGold | int | 0 | 초회 발견 골드 보상 (-> see `docs/systems/collection-system.md` 섹션 3.3) |
| firstDiscoverXP | int | 0 | 초회 발견 XP 보상 (-> see `docs/systems/collection-system.md` 섹션 3.3) |
| catalogIcon | Sprite | null | 도감 전용 아이콘 (Inspector 할당, JSON 직렬화 제외 — PATTERN-007) |
| sortOrder | int | 0 | 도감 내 표시 순서 |

**에셋 목록**: 총 27종. 에셋명 패턴: `SO_GatherCatalog_<ID>` (예: `SO_GatherCatalog_gather_mushroom`). 전체 아이템 ID 목록 및 콘텐츠 값: (-> see `docs/content/gathering-items.md`)

---

### 3. 세이브 데이터 구조

#### 3.1 세이브 슬롯 구조

| 파라미터 | 값 |
|----------|-----|
| 최대 세이브 슬롯 | 3 |
| 세이브 형식 | JSON |
| 저장 경로 | `Application.persistentDataPath/Saves/` |
| 파일 이름 | `save_{N}.json` (N = 0, 1, 2) |
| 메타 파일 | `save_meta.json` (슬롯별 요약: 플레이 시간, 날짜, 골드) |
| 자동 저장 | 매일 06:00 (하루 시작 배치 처리 완료 후) |
| 수동 저장 | 침대 수면 시 |

#### 3.2 최상위 세이브 스키마

> **주의**: 아래는 초기 설계 개요이다. 최신·완전한 JSON 스키마 및 C# 클래스 정의는
> `docs/systems/save-load-architecture.md` 섹션 2.2가 canonical이며,
> 메타 필드명(`saveVersion`, `savedAt`, `playTimeSeconds`)·시스템 필드 목록은
> 해당 문서를 따른다. (→ see docs/systems/save-load-architecture.md 섹션 2.2)

```json
{
  "saveVersion": "1.0.0",        // canonical 필드명: saveVersion (→ see save-load-architecture.md 섹션 2.2)
  "savedAt": "2026-04-06T15:30:00Z",
  "playTimeSeconds": 3600,

  "player": { },
  "inventory": { },
  "farm": { },
  "time": { },
  "weather": { },
  "economy": { },
  "buildings": [ ],
  "processing": [ ],
  "unlocks": { },
  "shops": [ ],
  "fishing": { },
  "fishCatalog": { },
  "npc": { },
  "gathering": { },              // FIX-080: (→ see docs/systems/gathering-system.md 섹션 숙련도)
  "gatheringCatalog": { }        // FIX-094: (→ see docs/systems/collection-architecture.md 섹션 5.2, ARC-037)
}
```

#### 3.3 세부 세이브 데이터 구조

##### PlayerSaveData

```json
{
  "positionX": 4.0,
  "positionY": 0.0,
  "positionZ": 3.0,
  "currentEnergy": 85,
  "maxEnergy": 100,
  "level": 3,
  "currentExp": 120,
  "equippedToolIndex": 0
}
```

##### InventorySaveData

```json
{
  "slots": [
    {
      "itemId": "potato",
      "itemType": "Crop",
      "quantity": 5,
      "quality": "Normal",
      "origin": "Outdoor"
    },
    {
      "itemId": "seed_tomato",
      "itemType": "Seed",
      "quantity": 10,
      "quality": "Normal",
      "origin": "Outdoor"
    }
  ],
  "maxSlots": 15,
  "toolbarSlots": [
    { "itemId": "hoe_basic", "quantity": 1 },
    { "itemId": "wateringcan_basic", "quantity": 1 },
    { "itemId": "seed_potato", "quantity": 5 },
    { "itemId": "sickle_basic", "quantity": 1 },
    { "itemId": "", "quantity": 0 },
    { "itemId": "", "quantity": 0 },
    { "itemId": "", "quantity": 0 },
    { "itemId": "", "quantity": 0 }
  ],
  "wateringCanCharges": 18,
  "toolbarSelectedIndex": 0
}
```

**인벤토리 슬롯 수**:

| 파라미터 | 값 |
|----------|-----|
| 초기 슬롯 수 | 15 (→ see docs/systems/inventory-system.md 섹션 2.1 for canonical 배낭 업그레이드 경로) |
| 최대 슬롯 수 | 30 (배낭 업그레이드 완료 시, → see inventory-system.md 섹션 2.1) |
| 툴바 슬롯 | 8 (범용, 씨앗/비료도 배치 가능, → see docs/systems/inventory-system.md 섹션 2.2) |
| 물뿌리개 기본 충전량 | 20 (→ see docs/systems/farming-system.md) — 위 예시의 18은 사용 후 잔여량 |

##### FarmTileSaveData

```json
{
  "gridWidth": 8,
  "gridHeight": 8,
  "tiles": [
    {
      "x": 0, "y": 0,
      "state": "Watered",
      "soilQuality": "Normal",
      "neglectDays": 0,
      "consecutiveCropId": "potato",
      "consecutiveCropCount": 2,
      "crop": {
        "cropDataId": "potato",
        "currentGrowthDays": 2,
        "growthStage": 1,
        "isWatered": true,
        "dryDays": 0,
        "fertilizerDataId": "basic_fertilizer",
        "fertilizerRemainingDays": 3,
        "totalWateredDays": 2
      }
    },
    {
      "x": 1, "y": 0,
      "state": "Empty",
      "soilQuality": "Normal",
      "neglectDays": 0,
      "consecutiveCropId": "",
      "consecutiveCropCount": 0,
      "crop": null
    }
  ]
}
```

**타일별 저장 항목**:

| 필드 | 저장 이유 |
|------|----------|
| state | 타일 상태 복원 |
| soilQuality | 토양 품질은 영구 속성 (-> see `docs/systems/farming-system.md` 섹션 6) |
| neglectDays | 방치/건조 연속일수 복원 |
| consecutiveCropId, Count | 연작 피해 판정 복원 (-> see `docs/systems/farming-system.md` 섹션 6.3) |
| crop (nullable) | 심어진 작물이 있는 경우만 |

**CropInstance 저장 항목**:

| 필드 | 저장 이유 |
|------|----------|
| cropDataId | CropData SO를 런타임에 역참조하기 위한 키 |
| currentGrowthDays | 성장 진행 상태 |
| growthStage | 현재 성장 단계 인덱스 |
| isWatered | 당일 물 줌 여부 |
| dryDays | 연속 건조 일수 |
| fertilizerDataId | 적용된 비료 (빈 문자열 = 없음) |
| fertilizerRemainingDays | 비료 잔여 효과 일수 |
| totalWateredDays | 총 물 준 일수 (품질 계산용) |

##### TimeSaveData

```json
{
  "currentYear": 1,
  "currentSeason": 0,
  "currentDay": 15,
  "currentHour": 14.5
}
```

(-> see `docs/systems/time-season-architecture.md` 섹션 2.1 for TimeManager 필드)

##### WeatherSaveData

```json
{
  "currentWeather": "Rain",
  "tomorrowWeather": "Clear",
  "weatherSeed": 42,
  "consecutiveSameWeatherDays": 1,
  "consecutiveExtremeWeatherDays": 0
}
```

##### EconomySaveData

(-> see `docs/systems/economy-architecture.md` 섹션 4.6 for canonical 구조)

```json
{
  "currentGold": 1250,
  "transactionLog": {
    "entries": [
      {
        "type": "Sell",
        "itemId": "potato",
        "quantity": 5,
        "unitPrice": 30,
        "totalPrice": 150,
        "day": 43,
        "season": 1
      }
    ],
    "totalEarned": 2500,
    "totalSpent": 1250
  },
  "priceFluctuation": {
    "seasonSalesCount": {
      "potato": 15,
      "tomato": 8
    }
  }
}
```

##### BuildingSaveData

```json
[
  {
    "buildingId": "water_tank",
    "gridX": 6,
    "gridY": 6,
    "isOperational": true,
    "upgradeLevel": 0,
    "buildProgress": 1.0,
    "storageSlots": null
  },
  {
    "buildingId": "storage",
    "gridX": 8,
    "gridY": 4,
    "isOperational": true,
    "upgradeLevel": 0,
    "buildProgress": 1.0,
    "storageSlots": [
      { "itemId": "crop_potato", "quantity": 20, "quality": "Normal" }
    ]
  }
]
```

| 필드 | 설명 |
|------|------|
| gridX, gridY | 좌하단 기준 배치 위치 |
| isOperational | 건설 완료 여부 (false = 건설 중) |
| upgradeLevel | 현재 업그레이드 단계 |
| buildProgress | 건설 진행률 (0.0~1.0, 1.0 = 완료) |

##### ProcessingSaveData

```json
[
  {
    "processorBuildingId": "building_processing_0",
    "slotIndex": 0,
    "recipeId": "jam_potato",
    "inputCropId": "potato",
    "inputQuantity": 1,
    "remainingHours": 2.5,
    "totalHours": 4.0,
    "slotState": 1,
    "outputItemId": "",
    "outputQuantity": 0
  }
]
```

##### UnlockSaveData

```json
{
  "unlockedCrops": ["potato", "carrot", "tomato", "corn"],
  "unlockedBuildings": ["water_tank"],
  "unlockedRecipes": ["jam_potato", "jam_carrot", "jam_tomato", "pickle_potato"]
}
```

**해금 판정 로직**: 레벨업 시 ProgressionData의 unlockTable을 참조하여 새로 해금된 항목을 UnlockSaveData에 추가한다. 한 번 해금된 항목은 영구적이다 (→ see `docs/systems/progression-architecture.md` 섹션 3.3).

[OPEN] 해금 데이터를 별도 저장할지, 플레이어 레벨로부터 역산할지. 별도 저장이 안전하지만 레벨 기반 역산이 더 단순하다. 이벤트 보상 등 레벨 외 해금이 추가될 경우 별도 저장이 필수.

##### ShopStockSaveData

```json
[
  {
    "shopId": "general_store",
    "dailyStock": {
      "seed_potato": 10,
      "basic_fertilizer": 5
    },
    "lastRefreshDay": 15
  }
]
```

##### ShippingBinSaveData

```json
{
  "pendingItems": [
    {
      "itemId": "crop_potato",
      "itemType": "Crop",
      "quantity": 10,
      "quality": "Normal",
      "origin": "Outdoor"
    },
    {
      "itemId": "proc_strawberry_juice",
      "itemType": "Processed",
      "quantity": 3,
      "quality": "Normal",
      "origin": "Outdoor"
    }
  ]
}
```

| 필드 | 설명 |
|------|------|
| pendingItems | 당일 넣은 아이템 목록 — 다음 날 06:00 정산 전까지 유지 |
| itemId | 아이템 고유 ID (→ see `docs/systems/inventory-system.md` 섹션 7) |
| itemType | 아이템 카테고리 (Crop, Seed, Tool, Consumable 등) |
| quantity | 수량 |
| quality | 품질 등급 (Normal / Silver / Gold) |

**설계 의도**: 출하함은 슬롯 제한이 없는 무한 저장소이므로(→ see `docs/systems/inventory-system.md` 섹션 2.4), 배열 길이 제한을 두지 않는다. 정산 후에는 `pendingItems`를 빈 배열로 초기화한다. 레벨 6 해금 2번째 출하함은 BuildingSaveData로 배치 위치를 저장하되, 출하 내용물은 이 ShippingBinSaveData에 통합 관리한다.

#### 3.4 세이브 파일 크기 예상

| 구성 요소 | 예상 크기 |
|----------|----------|
| PlayerSaveData | ~200 bytes |
| InventorySaveData | ~2 KB |
| FarmTileSaveData (16x16 최대) | ~30 KB |
| TimeSaveData | ~100 bytes |
| WeatherSaveData | ~200 bytes |
| EconomySaveData | ~10 KB (거래 로그 200건) |
| BuildingSaveData | ~1 KB |
| ProcessingSaveData | ~500 bytes |
| UnlockSaveData | ~500 bytes |
| ShopStockSaveData | ~1 KB |
| ShippingBinSaveData | ~500 bytes |
| FishingSaveData | ~500 bytes |
| **총계** | **~47 KB** |

JSON minify 적용 시 약 30KB 이하. 인디 게임 세이브 파일로서 충분히 작다.

---

### 4. 데이터 무결성 규칙

#### 4.1 SO 간 참조 관계

```
CropData
  +-- growthStagePrefabs[]: GameObject[] (필수, 길이 = growthStageCount)
  +-- soilMaterial: Material (선택)
  +-- giantCropPrefab: GameObject (선택, giantCropChance > 0일 때만)
  +-- harvestParticle: GameObject (선택)

ToolData
  +-- nextTier: ToolData (선택, null이면 최종 등급)

BuildingData
  +-- prefab: GameObject (필수)
  +-- constructionPrefab: GameObject (선택, 없으면 기본 건설 중 비주얼)

ProcessingRecipeData
  (참조 없음 -- inputCategory 필터로 CropData 간접 매칭)

ShopData
  +-- items[].priceData: PriceData (필수)
  +-- items[].itemReference: ScriptableObject (필수, CropData/ToolData/FertilizerData 중 하나)

PriceData
  (참조 없음 -- itemId 문자열로 간접 매칭)
```

#### 4.2 필수 vs 선택 필드

| SO 타입 | 필수 필드 | 선택 필드 |
|---------|----------|----------|
| CropData | cropName, cropId, seedPrice, sellPrice, growthDays, allowedSeasons, unlockLevel, growthStagePrefabs | icon, soilMaterial, giantCropPrefab, harvestParticle, description |
| FertilizerData | fertilizerName, fertilizerId, buyPrice, growthMultiplier | icon, soilTintColor, description, applyParticle |
| ToolData | toolName, toolId, toolType, tier | icon, modelPrefab, nextTier, animationClip, useSFX |
| BuildingData | buildingId, buildingName, buildCost, requiredLevel, tileSize, prefab, effectType | icon, constructionPrefab, description |
| ProcessingRecipeData | recipeId, recipeName, processingType, inputCategory, priceMultiplier, priceBonus, processingTimeHours | icon, description |
| ProgressionData | maxLevel, expPerLevel | (모든 필드 필수, → see docs/systems/progression-architecture.md 섹션 2.1) |
| EconomyConfig | startingGold, maxGold, sellPriceFloor, sellPriceCeiling, supplyDecayRate, transactionLogCapacity | (모든 필드 필수) |

#### 4.3 값 범위 제약

| 필드 | 최소값 | 최대값 | 비고 |
|------|--------|--------|------|
| CropData.growthDays | 1 | 30 | 1일 미만 성장은 의미 없음 |
| CropData.sellPrice | 1 | 9999 | 양수 필수 |
| CropData.seedPrice | 1 | 9999 | 양수 필수 |
| CropData.baseYield | 1 | 10 | 한 번 수확 시 최대 10개 |
| CropData.qualityChance | 0.0 | 1.0 | 확률값 |
| CropData.giantCropChance | 0.0 | 0.5 | 50% 초과는 밸런스 파괴 |
| FertilizerData.growthMultiplier | 1.0 | 3.0 | 1.0 미만은 성장 감소(의도하지 않음) |
| FertilizerData.qualityBonus | 0.0 | 1.0 | 확률 보정값 |
| ToolData.tier | 1 | 3 | 3등급(Legendary)이 최고 (→ see `docs/systems/tool-upgrade.md` 섹션 1.1) |
| ToolData.range | 1 | 25 | 5x5가 최대 |
| ToolData.energyCost | 0 | 10 | 손/씨앗봉투는 0 |
| BuildingData.buildCost | 0 | 99999 | - |
| BuildingData.tileSize (각 축) | 1 | 8 | 8x8 이상은 그리드 과점유 |
| EconomyConfig.sellPriceFloor | 0.1 | 1.0 | 1.0이면 가격 하락 불가 |
| EconomyConfig.sellPriceCeiling | 1.0 | 5.0 | 5배 초과는 밸런스 파괴 |
| ProgressionData.maxLevel | 1 | 100 | 현재 설계는 10 |

#### 4.4 ID 유일성 규칙

| ID 네임스페이스 | 범위 | 예시 |
|----------------|------|------|
| cropId | 모든 CropData 에셋에서 유일 | "potato", "tomato" |
| fertilizerId | 모든 FertilizerData 에셋에서 유일 | "basic_fertilizer" |
| toolId | 모든 ToolData 에셋에서 유일 | "hoe_basic", "hoe_copper" |
| buildingId | 모든 BuildingData 에셋에서 유일 | "greenhouse" |
| recipeId | 모든 ProcessingRecipeData 에셋에서 유일 | "jam_potato" |
| itemId (PriceData) | 모든 PriceData 에셋에서 유일 | "potato", "seed_potato" |
| shopId | 모든 ShopData 에셋에서 유일 | "general_store" |

**네이밍 규칙**: 모든 ID는 snake_case 소문자 영어. 공백 및 특수문자 금지. 작물 ID는 CropData.cropId와 PriceData.itemId가 동일해야 한다.

#### 4.5 세이브 데이터 검증 규칙

세이브 파일 로드 시 다음 검증을 수행한다:

| 검증 항목 | 실패 시 처리 |
|----------|------------|
| version 필드 존재 확인 | 미존재 시 마이그레이션 시도, 실패 시 로드 거부 |
| cropId가 유효한 CropData에 매핑되는지 | 매핑 실패 시 해당 작물 제거 (타일을 Empty로) |
| toolId가 유효한 ToolData에 매핑되는지 | 매핑 실패 시 기본 도구로 대체 |
| buildingId가 유효한 BuildingData에 매핑되는지 | 매핑 실패 시 해당 건물 제거 |
| currentGold 범위 (0 ~ maxGold) | 범위 초과 시 클램핑 |
| 타일 좌표가 그리드 범위 내인지 | 범위 외 타일은 무시 |
| currentSeason 범위 (0~3) | 범위 외 시 Spring(0)으로 리셋 |
| currentDay 범위 (1~28) | 범위 외 시 1로 리셋 |

[RISK] 세이브 파일 위변조에 대한 보호 장치가 없다. 싱글 플레이어 게임이므로 치팅 방지보다는 데이터 손상 복구에 집중한다.

---

### 5. 데이터 밸런스 훅

BAL-001(작물 경제 밸런스), BAL-002(전체 진행 밸런스)에서 활용할 수 있는 밸런스 관련 데이터 포인트를 명시한다.

#### 5.1 작물 경제 밸런스 포인트 (BAL-001 대상)

| 밸런스 포인트 | 데이터 원천 | 조정 방법 |
|-------------|-----------|----------|
| 일일 수익 효율 (G/일) | CropData.sellPrice, growthDays | sellPrice 또는 growthDays 조정 |
| 씨앗 투자 수익률 (ROI) | CropData.sellPrice / seedPrice | seedPrice 조정 (seedPriceRatio) |
| 비료 투자 효율 | FertilizerData.buyPrice vs 성장일수 단축 가치 | buyPrice, growthMultiplier 조정 |
| 가공 수익 배수 | ProcessingRecipeData.priceMultiplier, priceBonus | 배수/보너스 조정 |
| 품질별 수익 스펙트럼 | CropQuality multiplier (1.0/1.25/1.5/2.0) | 품질 배수 조정 |
| 재수확 작물 효율 | CropData.isRepeating, regrowDays | regrowDays 조정 |
| 계절 가격 변동폭 | PriceData.seasonMultipliers | 계절별 배수 조정 |
| 수급 하락 민감도 | PriceData.demandThreshold, EconomyConfig.supplyDecayRate | 임계값/감쇠율 조정 |

#### 5.2 진행 밸런스 포인트 (BAL-002 대상)

| 밸런스 포인트 | 데이터 원천 | 조정 방법 |
|-------------|-----------|----------|
| 레벨업 속도 | ProgressionData.expPerLevel, harvestExpBase | 경험치 테이블 조정 (→ see docs/balance/progression-curve.md 섹션 2.4.1) |
| 시설 해금 타이밍 | BuildingData.requiredLevel, buildCost | 레벨/비용 조정 |
| 작물 해금 순서 | CropData.unlockLevel | 레벨 재배치 |
| 도구 업그레이드 비용 곡선 | ToolData.upgradeCost + upgradeMaterials | 비용/재료 조정 |
| 골드 인플레이션 | EconomyConfig.startingGold + 전체 수입/지출 테이블 | 초기 골드, 가격 전반 조정 |
| 에너지 경제 | ToolData.energyCost, PlayerSaveData.maxEnergy | 도구별 에너지 비용 조정 |
| 농장 확장 비용 곡선 | FarmGrid 확장 비용 (-> see `docs/systems/farming-system.md` 섹션 1) | 단계별 비용 조정 |

#### 5.3 밸런스 검증 공식

밸런스 시트 작성 시 사용할 핵심 검증 공식:

**1. 최소 시급 (Minimum Hourly Income)**:

```
최소_시급 = (감자_순이익 * 타일수) / (감자_성장일수 * 10분)
         = (15G * 8) / (3 * 10분) = 120G / 30분 = 4G/분 (실시간)
```

레벨 1 플레이어가 8타일에 감자만 심었을 때의 기본 수익률. 모든 시설/업그레이드 비용이 이 수익률로 합리적인 시간 내에 달성 가능해야 한다.

**2. 시설 투자 회수 기간**:

```
회수_기간(일) = 시설_비용 / (시설_효과로_인한_일일_순이익_증가분)
```

모든 시설의 투자 회수 기간이 1계절(28일) 이내여야 투자 동기가 충분하다.

**3. 레벨업 소요 시간**:

```
레벨업_소요일 = 구간_경험치 / (일일_평균_수확_횟수 * 수확당_평균_경험치)
```

한 레벨을 올리는 데 3~7일(실시간 30~70분)이 적절하다.

---

## Part II -- 기술 아키텍처

---

### 1. ScriptableObject 클래스 설계

#### 1.1 공통 베이스 SO

모든 게임 데이터 SO가 상속하는 공통 베이스를 정의한다. ID 기반 참조 복원과 Editor-time 검증을 위한 인프라를 제공한다.

```csharp
// illustrative
namespace SeedMind.Core
{
    /// <summary>
    /// 모든 게임 데이터 ScriptableObject의 베이스 클래스.
    /// ID 기반 참조 복원과 에디터 검증 인터페이스를 제공한다.
    /// </summary>
    public abstract class GameDataSO : ScriptableObject
    {
        [Header("식별")]
        public string dataId;       // 고유 식별자 (예: "potato", "hoe_t1")
        public string displayName;  // UI 표시명 (한국어, 예: "감자", "호미")

        [Header("메타")]
        public Sprite icon;         // UI 아이콘 (nullable)

        /// <summary>
        /// Editor-time 유효성 검증. 하위 클래스에서 오버라이드하여
        /// 필드 검증 로직을 추가한다.
        /// </summary>
        public virtual bool Validate(out string errorMessage)
        {
            if (string.IsNullOrEmpty(dataId))
            {
                errorMessage = $"{name}: dataId가 비어 있습니다.";
                return false;
            }
            errorMessage = null;
            return true;
        }
    }
}
```

**설계 근거**:
- `dataId`는 세이브 데이터에서 SO 참조를 복원할 때 키로 사용된다 (섹션 4.3 참조)
- `displayName`은 기존 SO의 `cropName`, `toolName` 등을 통합하는 공통 필드
- `Validate()`는 MCP 자동 생성 후 에셋 무결성 검증에 활용 (섹션 5 참조)

#### 1.2 기존 SO 클래스와의 관계

기존에 정의된 SO 클래스들을 `GameDataSO`를 상속하도록 변경한다. 각 클래스의 상세 필드는 canonical 문서를 참조한다.

```
GameDataSO (abstract)
├── CropData           (→ see docs/systems/farming-architecture.md 섹션 4.1)
├── FertilizerData     (→ see docs/systems/farming-architecture.md 섹션 4.2)
├── ToolData           (→ see docs/systems/farming-architecture.md 섹션 4.3)
├── BuildingData       (→ see 본 문서 Part I 섹션 2.4 for canonical 필드 정의)
├── EconomyConfig      (→ see docs/systems/economy-architecture.md 섹션 1)
├── PriceData          (→ see docs/systems/economy-architecture.md 섹션 1.4)
├── ShopData           (→ see docs/systems/economy-architecture.md 섹션 1)
├── TimeConfig         (→ see docs/systems/time-season-architecture.md 섹션 2.2)
├── SeasonData         (→ see docs/systems/time-season-architecture.md 섹션 2.1)
└── WeatherData        (→ see docs/systems/time-season-architecture.md 섹션 2.4)
```

**변경 사항 요약** (기존 SO 클래스에 대해):
- `CropData`: `cropId` → `dataId`로 통합, `cropName` → `displayName`으로 통합. 나머지 필드 유지
- `FertilizerData`: `fertilizerId` → `dataId`, `fertilizerName` → `displayName`
- `ToolData`: `toolName` → `displayName`, 신규 `dataId` 필드 추가 (예: `"hoe_t1"`)
- Config 계열 SO (`EconomyConfig`, `TimeConfig`): `GameDataSO`를 상속하되, `dataId`는 시스템명을 사용 (예: `"config_economy"`)

[RISK] 기존 farming-architecture.md, economy-architecture.md 등에서 정의한 SO 클래스의 필드명이 변경된다. Phase 2(구현) 진입 전에 모든 아키텍처 문서의 코드 예시를 일괄 갱신해야 한다.

[OPEN] GameDataSO 도입으로 변경된 필드명(cropId→dataId, cropName→displayName 등)이 farming-architecture.md의 코드 예시에 미반영 상태. 후속 작업으로 farming-architecture.md 일괄 갱신 필요.

#### 1.3 SO 에셋 저장 경로

```
Assets/_Project/Data/                          # SO 인스턴스 최상위
├── Crops/                                     # 작물 데이터
│   ├── SO_Crop_Potato.asset                   # (→ see docs/design.md 4.2 for 수치)
│   ├── SO_Crop_Carrot.asset
│   ├── SO_Crop_Tomato.asset
│   ├── SO_Crop_Corn.asset
│   ├── SO_Crop_Strawberry.asset
│   ├── SO_Crop_Pumpkin.asset
│   ├── SO_Crop_Sunflower.asset
│   └── SO_Crop_Watermelon.asset
│
├── Fertilizers/                               # 비료 데이터
│   ├── SO_Fert_Basic.asset
│   ├── SO_Fert_Quality.asset
│   ├── SO_Fert_Speed.asset
│   └── SO_Fert_Premium.asset
│
├── Tools/                                     # 도구 데이터
│   ├── SO_Tool_Hoe_Basic.asset, SO_Tool_Hoe_Reinforced.asset, SO_Tool_Hoe_Legendary.asset
│   ├── SO_Tool_WateringCan_Basic.asset, SO_Tool_WateringCan_Reinforced.asset, SO_Tool_WateringCan_Legendary.asset
│   ├── SO_Tool_Sickle_Basic.asset, SO_Tool_Sickle_Reinforced.asset, SO_Tool_Sickle_Legendary.asset
│   ├── SO_Tool_SeedBag.asset
│   └── SO_Tool_Hand.asset
│   # 등급 체계: → see docs/systems/tool-upgrade.md 섹션 1.1
│
├── Buildings/                                 # 건물 데이터
│   ├── SO_Bldg_WaterTank.asset
│   ├── SO_Bldg_Greenhouse.asset
│   ├── SO_Bldg_Storage.asset
│   └── SO_Bldg_Processor.asset
│
├── Gathering/                                 # 채집 데이터 (ARC-033)
│   ├── SO_GatherConfig_Default.asset          # 글로벌 밸런스 설정 (1개)
│   ├── Points/                                # 포인트 유형별 SO (5개)
│   │   ├── SO_GatherPoint_ForestFloor.asset
│   │   ├── SO_GatherPoint_Bush.asset
│   │   ├── SO_GatherPoint_CaveEntrance.asset
│   │   ├── SO_GatherPoint_PondEdge.asset
│   │   └── SO_GatherPoint_Meadow.asset
│   └── Items/                                 # 채집 아이템 SO (27개)
│       ├── SO_GatherItem_Dandelion.asset      # → see docs/systems/gathering-system.md 섹션 3
│       ├── SO_GatherItem_WildGarlic.asset
│       └── ... (아이템당 1개, 총 27종)
│
├── Economy/                                   # 경제 관련 SO
│   ├── SO_EconomyConfig.asset
│   ├── SO_ShopData_General.asset
│   └── Prices/
│       ├── SO_Price_Potato.asset
│       ├── SO_Price_Carrot.asset
│       └── ... (작물/아이템당 1개)
│
├── Time/                                      # 시간/계절/날씨 SO
│   ├── SO_TimeConfig.asset
│   ├── Seasons/
│   │   ├── SO_Season_Spring.asset
│   │   ├── SO_Season_Summer.asset
│   │   ├── SO_Season_Autumn.asset
│   │   └── SO_Season_Winter.asset
│   └── Weather/
│       ├── SO_Weather_Spring.asset
│       ├── SO_Weather_Summer.asset
│       ├── SO_Weather_Autumn.asset
│       └── SO_Weather_Winter.asset
│
└── Config/                                    # 기타 시스템 설정
    └── SO_ProgressionData.asset         # LevelConfig 대체 (BAL-002)
```

경로 규칙: `(→ see docs/systems/project-structure.md 섹션 1, 6.1 for 네이밍 규칙)`

#### 1.4 SO 레지스트리 (런타임 참조 해소)

세이브 데이터에서 SO 참조를 복원하기 위해, 모든 `GameDataSO`를 런타임에 ID로 검색할 수 있는 레지스트리가 필요하다.

```csharp
// illustrative
namespace SeedMind.Core
{
    /// <summary>
    /// 모든 GameDataSO를 dataId로 검색하는 런타임 레지스트리.
    /// 게임 시작 시 초기화, Resources 또는 Addressables에서 로드.
    /// </summary>
    public class DataRegistry : Singleton<DataRegistry>
    {
        // 내부 저장소
        private Dictionary<string, GameDataSO> _registry
            = new Dictionary<string, GameDataSO>();

        /// <summary>
        /// 초기화: 모든 GameDataSO를 스캔하여 등록.
        /// Resources.LoadAll 또는 직접 참조 배열 사용.
        /// </summary>
        public void Initialize()
        {
            var allData = Resources.LoadAll<GameDataSO>("Data");
            // 또는 Addressables 사용 시 비동기 로드
            foreach (var data in allData)
            {
                if (!_registry.TryAdd(data.dataId, data))
                {
                    Debug.LogError($"[DataRegistry] 중복 dataId: {data.dataId}");
                    // → see 섹션 5.1 검증 시스템
                }
            }
        }

        /// <summary>ID로 SO 검색. 실패 시 null 반환 + 로그.</summary>
        public T Get<T>(string dataId) where T : GameDataSO
        {
            if (_registry.TryGetValue(dataId, out var so))
                return so as T;

            Debug.LogWarning($"[DataRegistry] 미등록 dataId: {dataId}");
            return null;
        }

        /// <summary>특정 타입의 모든 SO 반환.</summary>
        public List<T> GetAll<T>() where T : GameDataSO { /* ... */ }
    }
}
```

**초기화 전략**:

| 방식 | 장점 | 단점 | 채택 |
|------|------|------|:----:|
| `Resources.LoadAll` | 코드 단순, MCP 테스트 용이 | Resources 폴더 강제, 빌드 시 항상 포함 | O (초기) |
| SerializedField 배열 | 명시적, 빌드 최적화 | SO 추가 때마다 배열 수동 갱신 | X |
| Addressables | 비동기 로드, 유연함 | 초기 스코프에서 과도한 복잡도 | X (추후) |

[OPEN] Resources 폴더 사용은 프로젝트 규모가 커질 경우 Addressables로 전환해야 한다. 전환 시점 기준을 정해야 한다.

**Resources 폴더 배치**: DataRegistry가 `Resources.LoadAll<GameDataSO>("Data")`로 스캔하려면, 런타임에 필요한 SO를 `Assets/_Project/Resources/Data/` 하위에도 배치하거나, 기존 `Assets/_Project/Data/`를 `Resources/` 하위로 이동해야 한다.

제안: `Assets/_Project/Data/`를 에디터 전용 원본으로 유지하고, `Assets/_Project/Resources/Data/` 에 **심볼릭 링크 또는 에디터 스크립트로 자동 복사**하는 전략을 사용한다. 또는 더 간단하게, `_Project/Data/` 폴더 자체를 `_Project/Resources/Data/`로 이동한다.

[RISK] `Resources` 폴더 내 모든 에셋은 빌드에 포함된다. SO 수가 많아지면 빌드 크기 및 메모리에 영향. 현재 스코프(약 30~40개 SO)에서는 문제없다.

#### 1.5 MCP로 SO 에셋 생성 시 제약사항

| 제약 | 설명 | Workaround |
|------|------|------------|
| 배열 필드 설정 | MCP가 SO의 배열/리스트 필드를 직접 설정할 수 있는지 불확실 | C# 에디터 스크립트로 배열 필드를 설정하는 헬퍼 작성 후 MCP로 실행 |
| SO 간 참조 | ToolData.nextTier 같은 SO→SO 참조를 MCP로 설정 불가할 수 있음 | 에디터 스크립트에서 dataId 기반으로 참조를 자동 연결 |
| CreateAssetMenu 경로 | MCP에서 ScriptableObject.CreateInstance → AssetDatabase.CreateAsset 직접 호출 필요 | MCP 콘솔에서 C# 코드 실행으로 대응 |
| 프리팹 참조 | SO의 GameObject/Prefab 필드에 프리팹을 할당하려면 에셋 경로를 정확히 알아야 함 | 네이밍 규칙(→ see project-structure.md 6.2)을 활용한 경로 추론 |

[RISK] MCP for Unity의 ScriptableObject 조작 API 범위가 공식 문서로 확인되지 않음. Phase 2 진입 시 가장 먼저 MCP SO 생성/수정 테스트를 수행해야 한다.

---

### 2. 런타임 데이터 클래스 설계 (SaveData 구조)

#### 2.1 전체 세이브 데이터 구조

```csharp
// illustrative
namespace SeedMind.Core
{
    /// <summary>
    /// 하나의 세이브 슬롯에 저장되는 전체 게임 상태.
    /// JSON 직렬화 대상.
    /// </summary>
    [System.Serializable]
    public class GameSaveData
    {
        // 세이브 메타데이터
        public string saveVersion;          // 세이브 포맷 버전 (예: "1.0.0")
        public string savedAt;              // 저장 시각 (ISO 8601)
        public int playTimeSeconds;         // 총 플레이 시간 (초)

        // 시스템별 세이브 데이터
        public PlayerSaveData player;
        public FarmSaveData farm;
        public TimeSaveData time;           // (→ see docs/systems/time-season-architecture.md 섹션 7.1)
        public WeatherSaveData weather;     // (→ see docs/systems/time-season-architecture.md 섹션 7.2)
        public EconomySaveData economy;     // (→ see docs/systems/economy-architecture.md 섹션 4.6)
        public BuildingSaveData[] buildings; // 건설된 시설 목록
        public ProcessingSaveData[] processing; // 진행 중인 가공 작업
        public UnlockSaveData unlocks;      // 해금 상태
        public ShopStockSaveData[] shops;   // 상점 재고 상태
        // BAL-002: 진행 시스템 추가 (→ see docs/systems/progression-architecture.md 섹션 4.4, 5.1)
        public MilestoneSaveData milestones; // 마일스톤 진행 상태 (null 허용 — 구버전 세이브 호환)
        // ARC-011: 루트 레벨로 분리된 추가 필드
        public InventorySaveData inventory;  // 인벤토리 상태 (null 허용 — 구버전 호환, → see docs/systems/inventory-architecture.md 섹션 6.1)
        public NPCSaveData npc;              // NPC 상태 (null 허용, → see docs/systems/npc-shop-architecture.md 섹션 7.1)
        public TutorialSaveData tutorial;    // 튜토리얼 진행 (null 허용, → see docs/systems/tutorial-architecture.md 섹션 7)
        public FishingSaveData fishing;      // 낚시 상태 (null 허용 — 구버전 호환, → see docs/systems/fishing-architecture.md, FIX-051)
        public FishCatalogSaveData fishCatalog; // 낚시 도감 상태 (null 허용 — 구버전 호환, → see docs/systems/fishing-architecture.md 섹션 20, ARC-030)
        public GatheringSaveData gathering;   // 채집 상태 (null 허용 — 구버전 호환, → see docs/systems/gathering-system.md, ARC-031)
        public GatheringCatalogSaveData gatheringCatalog; // 채집 도감 상태 (null 허용 — 구버전 호환, → see docs/systems/collection-architecture.md 섹션 5.2, ARC-037)
    }
}
```

#### 2.2 PlayerSaveData

```csharp
// illustrative
namespace SeedMind.Player
{
    [System.Serializable]
    public class PlayerSaveData
    {
        // 위치
        public float posX, posY, posZ;        // 월드 좌표
        public float rotY;                    // Y축 회전

        // 에너지
        public int currentEnergy;             // 현재 에너지
        public int maxEnergy;                 // 최대 에너지

        // 인벤토리
        public InventorySlotSaveData[] inventorySlots;
        public int equippedToolIndex;         // 현재 선택된 도구 인덱스

        // 레벨
        public int level;
        public int currentExp;
    }

    [System.Serializable]
    public class InventorySlotSaveData
    {
        public string itemId;                 // GameDataSO.dataId 참조
        public int quantity;
        public int qualityIndex;              // (int)CropQuality, 작물인 경우만 유효
    }
}
```

#### 2.3 FarmSaveData

```csharp
// illustrative
namespace SeedMind.Farm
{
    [System.Serializable]
    public class FarmSaveData
    {
        public int gridWidth;                 // 현재 그리드 폭
        public int gridHeight;                // 현재 그리드 높이
        public FarmTileSaveData[] tiles;      // 모든 타일 상태 (gridWidth * gridHeight)
    }

    [System.Serializable]
    public class FarmTileSaveData
    {
        public int x, y;                      // 그리드 좌표
        public int stateIndex;                // (int)TileState
        public int neglectDays;               // 방치 일수
        public int soilQualityIndex;          // (int)SoilQuality (→ see docs/systems/farming-system.md 섹션 6)
        public string consecutiveCropId;      // 연작 중인 작물 ID (→ see docs/systems/farming-system.md 섹션 6.3)
        public int consecutiveCropCount;      // 연작 횟수

        // 작물 정보 (null이면 작물 없음 → JSON에서는 빈 문자열)
        public CropInstanceSaveData crop;
    }

    [System.Serializable]
    public class CropInstanceSaveData
    {
        public string cropDataId;             // CropData.dataId 참조 (→ see 섹션 4.3)
        public int currentGrowthDays;         // 현재 성장 일수
        public int growthStage;               // 현재 성장 단계 인덱스
        public bool isWatered;                // 물을 준 상태인지
        public int dryDays;                   // 연속 건조 일수

        // 비료 정보
        public string fertilizerDataId;       // FertilizerData.dataId (비료 없으면 빈 문자열)
        public int fertilizerRemainingDays;   // 비료 잔여 효과 일수

        // 품질 관련
        public int totalWateredDays;          // 총 물 준 일수 (품질 계산용)
    }
}
```

#### 2.4 TimeSaveData / WeatherSaveData

이미 정의됨. 변경 없이 사용한다.
- `TimeSaveData` (→ see `docs/systems/time-season-architecture.md` 섹션 7.1)
- `WeatherSaveData` (→ see `docs/systems/time-season-architecture.md` 섹션 7.2)

#### 2.5 EconomySaveData

이미 정의됨. 단, `PriceFluctuationSaveData`의 `Dictionary<string, int>` 직렬화 문제에 대한 대응이 필요하다 (→ see 섹션 3.2).

- `EconomySaveData` (→ see `docs/systems/economy-architecture.md` 섹션 4.6)

**변경 사항**: `PriceFluctuationSaveData`의 Dictionary를 직렬화 가능한 형태로 변환.

```csharp
// illustrative — economy-architecture.md의 PriceFluctuationSaveData 대체
namespace SeedMind.Economy
{
    [System.Serializable]
    public class PriceFluctuationSaveData
    {
        // Dictionary<string, int> 대신 직렬화 가능한 배열 사용
        public StringIntPair[] seasonSalesEntries;
    }

    [System.Serializable]
    public class StringIntPair
    {
        public string key;    // itemId
        public int value;     // 판매 수량
    }
}
```

#### 2.6 BuildingSaveData / ProcessingSaveData / UnlockSaveData / ShopStockSaveData

Part I 섹션 3.3에서 JSON 형태로 정의된 세이브 데이터의 C# 클래스 정의.

```csharp
// illustrative
namespace SeedMind.Building
{
    [System.Serializable]
    public class BuildingSaveData
    {
        public string buildingId;             // BuildingData.dataId 참조
        public int gridX;                     // 배치 위치 X (좌하단 기준)
        public int gridY;                     // 배치 위치 Y
        public bool isOperational;            // 건설 완료 여부
        public int upgradeLevel;              // 현재 업그레이드 단계
        public float buildProgress;           // 건설 진행률 (0.0~1.0)
        // Storage 건물에만 사용 — null이면 창고 없음 (→ see docs/systems/inventory-system.md 섹션 2.3)
        public ItemSlotSaveData[] storageSlots;
    }

    [System.Serializable]
    public class ItemSlotSaveData
    {
        public string itemId;             // IInventoryItem.ItemId
        public string itemType;           // ItemType enum 문자열 ("Crop", "Seed", "Tool", "Consumable" 등)
        public int quantity;
        public string quality;            // CropQuality enum 문자열 (Crop 카테고리만)
        public string origin;             // [FIX-034] HarvestOrigin enum 문자열 ("Outdoor" | "Greenhouse" | "Barn" | "Fishing" | "Gathering")
                                          // null/미지정 시 "Outdoor"로 역직렬화 (하위 호환)
                                          // → see docs/systems/economy-architecture.md 섹션 3.10
    }
}
```

```csharp
// illustrative
namespace SeedMind.Economy
{
    [System.Serializable]
    public class ProcessingSaveData
    {
        public string processorBuildingId;    // 가공소 BuildingInstance의 식별자
        public int slotIndex;                 // 가공 슬롯 인덱스
        public string recipeId;               // ProcessingRecipeData.dataId 참조
        public string inputCropId;            // 입력 작물 ID
        public int inputQuantity;             // 입력 수량
        public float remainingHours;          // 남은 시간 (게임 내 시간)
        public float totalHours;              // 총 소요 시간
        public int slotState;                 // ProcessingSlot.SlotState의 int 캐스팅
        public string outputItemId;           // 출력 아이템 ID (Completed 상태에서만 유효)
        public int outputQuantity;            // 출력 수량
        // (-> see docs/systems/processing-architecture.md 섹션 5.1 for canonical 필드 정의)
    }
}
```

```csharp
// illustrative
namespace SeedMind.Core
{
    [System.Serializable]
    public class UnlockSaveData
    {
        public string[] unlockedCrops;        // 해금된 작물 ID 목록
        public string[] unlockedBuildings;    // 해금된 시설 ID 목록
        public string[] unlockedRecipes;      // 해금된 가공 레시피 ID 목록
        // BAL-002: 진행 시스템 확장 필드 (→ see docs/systems/progression-architecture.md 섹션 3.5)
        public string[] unlockedFertilizers;  // 해금된 비료 ID 목록 (null 허용 — 구버전 세이브 호환)
        public string[] unlockedTools;        // 해금된 도구 등급 ID 목록 (null 허용)
        public string[] unlockedFarmExpansions; // 해금된 농장 확장 단계 ID 목록 (null 허용)
    }
}
```

```csharp
// illustrative
namespace SeedMind.Economy
{
    [System.Serializable]
    public class ShopStockSaveData
    {
        public string shopId;                 // ShopData.dataId 참조
        public StringIntPair[] dailyStock;    // 일일 재고 (itemId → 잔여 수량)
        public int lastRefreshDay;            // 마지막 재고 갱신 일자
    }
}
```

```csharp
// illustrative
namespace SeedMind.Core
{
    [System.Serializable]
    public class ShippingBinSaveData
    {
        // 당일 출하함에 넣은 아이템 목록 — 다음 날 06:00 정산 전까지 유지
        // (→ see docs/systems/inventory-system.md 섹션 2.4 for 출하함 동작 canonical)
        // (→ see 본 문서 Part I 섹션 3.3 ShippingBinSaveData for JSON 스키마)
        public ItemSlotSaveData[] pendingItems;
    }
}
```

---

### 3. 저장/로드 시스템 아키텍처

#### 3.1 SaveManager 상세 설계

```csharp
// illustrative
namespace SeedMind.Core
{
    /// <summary>
    /// 게임 상태의 저장/로드를 중앙 관리.
    /// DontDestroyOnLoad Singleton.
    /// </summary>
    public class SaveManager : Singleton<SaveManager>
    {
        // --- 설정 ---
        private const int MAX_SLOTS = 3;             // 최대 세이브 슬롯 수
        private const string SAVE_DIR = "Saves";     // Application.persistentDataPath 하위
        private const string FILE_PREFIX = "save_";  // save_0.json, save_1.json, save_2.json
        private const string FILE_EXT = ".json";
        private const string CURRENT_VERSION = "1.0.0"; // 현재 세이브 포맷 버전

        // --- 상태 ---
        private int _activeSlot = -1;                // 현재 활성 슬롯 (-1 = 없음)
        private bool _isSaving;
        private bool _isLoading;

        // --- 이벤트 ---
        public static event Action OnSaveStarted;
        public static event Action OnSaveCompleted;
        public static event Action OnLoadStarted;
        public static event Action OnLoadCompleted;
        public static event Action<string> OnSaveError;
        public static event Action<string> OnLoadError;

        // --- ISaveable 등록 ---
        private List<ISaveable> _saveables = new List<ISaveable>();

        public void Register(ISaveable saveable) { /* 등록 */ }
        public void Unregister(ISaveable saveable) { /* 해제 */ }

        // --- 핵심 메서드 ---
        public void Save(int slotIndex) { /* 아래 3.4 참조 */ }
        public void Load(int slotIndex) { /* 아래 3.5 참조 */ }
        public void AutoSave() { /* _activeSlot에 저장 */ }
        public bool HasSaveData(int slotIndex) { /* 파일 존재 여부 */ }
        public SaveSlotInfo GetSlotInfo(int slotIndex) { /* 메타 정보만 */ }
        public void DeleteSave(int slotIndex) { /* 파일 삭제 */ }

        // --- 유틸리티 ---
        private string GetSavePath(int slotIndex) { /* 경로 생성 */ }
    }
}
```

#### 3.2 JSON 직렬화 전략

| 방식 | 장점 | 단점 | 채택 |
|------|------|------|:----:|
| **Unity JsonUtility** | 별도 의존성 없음, 빠름 | Dictionary 미지원, 다형성 미지원, null 처리 제한적 | X |
| **Newtonsoft.Json (Json.NET)** | Dictionary/다형성/null 완전 지원, 풍부한 옵션 | 외부 패키지 의존성 | O |

**채택 근거**: `PriceFluctuationSaveData`의 Dictionary 직렬화, 향후 인벤토리 아이템 다형성 등을 고려하면 Json.NET이 필수적이다. Unity 6에는 `com.unity.nuget.newtonsoft-json` 패키지가 기본 제공되므로 외부 의존성 부담이 없다.

**직렬화 설정**:

```csharp
// illustrative — SaveManager 내부
private static readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
{
    Formatting = Formatting.Indented,           // 사람이 읽을 수 있는 포맷
    NullValueHandling = NullValueHandling.Include, // null도 명시적 기록
    DefaultValueHandling = DefaultValueHandling.Include,
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
};
```

[OPEN] Development 빌드에서는 `Formatting.Indented`, Release 빌드에서는 `Formatting.None`으로 전환할지.

#### 3.3 파일 경로 및 이름 규칙

```
{Application.persistentDataPath}/
└── Saves/
    ├── save_0.json         # 슬롯 0
    ├── save_1.json         # 슬롯 1
    ├── save_2.json         # 슬롯 2
    └── save_meta.json      # 슬롯 메타 정보 (이름, 마지막 저장 시각, 미리보기 데이터)
```

**save_meta.json 구조**:

```csharp
// illustrative
[System.Serializable]
public class SaveMetaFile
{
    public SaveSlotInfo[] slots;  // MAX_SLOTS 크기
}

[System.Serializable]
public class SaveSlotInfo
{
    public int slotIndex;
    public string slotName;         // 플레이어 지정 이름 (기본: "슬롯 1")
    public string savedAt;          // ISO 8601
    public int year;                // 표시용: 게임 내 연도
    public int seasonIndex;         // 표시용: 게임 내 계절
    public int day;                 // 표시용: 게임 내 일
    public int playTimeSeconds;     // 총 플레이 시간
    public int gold;                // 표시용: 보유 골드
    public bool exists;             // 세이브 데이터 존재 여부
}
```

**경로 예시 (Windows)**:
```
C:\Users\{username}\AppData\LocalLow\{CompanyName}\SeedMind\Saves\save_0.json
```

#### 3.4 저장 흐름

```
SaveManager.Save(slotIndex)
    │
    ├── 1) 상태 체크: _isSaving이면 중복 방지, return
    │
    ├── 2) OnSaveStarted 이벤트 발행
    │
    ├── 3) GameSaveData 인스턴스 생성
    │       ├── saveVersion = CURRENT_VERSION
    │       ├── savedAt = DateTime.UtcNow.ToString("o")  // ISO 8601
    │       └── playTimeSeconds = 누적 플레이 시간
    │
    ├── 4) 각 시스템에서 SaveData 수집
    │       ├── PlayerController → PlayerSaveData
    │       │   └── PlayerInventory.GetSaveData()
    │       ├── FarmGrid → FarmSaveData
    │       │   └── foreach tile: FarmTileSaveData
    │       │       └── if crop != null: CropInstanceSaveData
    │       ├── TimeManager.GetSaveData() → TimeSaveData
    │       ├── WeatherSystem.GetSaveData() → WeatherSaveData
    │       └── EconomyManager.GetSaveData() → EconomySaveData
    │           ├── TransactionLog.GetSaveData()
    │           └── PriceFluctuationSystem.GetSaveData()
    │
    ├── 5) JSON 직렬화
    │       json = JsonConvert.SerializeObject(gameSaveData, _jsonSettings)
    │
    ├── 6) 파일 쓰기 (비동기)
    │       path = GetSavePath(slotIndex)
    │       File.WriteAllTextAsync(path, json)
    │
    ├── 7) 메타 정보 갱신
    │       SaveMetaFile 로드 → slots[slotIndex] 갱신 → 재저장
    │
    ├── 8) OnSaveCompleted 이벤트 발행
    │
    └── 9) 에러 발생 시: OnSaveError 이벤트 발행 + Debug.LogError
```

#### 3.5 로드 흐름

```
SaveManager.Load(slotIndex)
    │
    ├── 1) 상태 체크: _isLoading이면 중복 방지
    │       파일 존재 여부 확인 → 없으면 OnLoadError
    │
    ├── 2) OnLoadStarted 이벤트 발행
    │
    ├── 3) 파일 읽기
    │       json = File.ReadAllText(GetSavePath(slotIndex))
    │
    ├── 4) JSON 역직렬화
    │       gameSaveData = JsonConvert.DeserializeObject<GameSaveData>(json, _jsonSettings)
    │
    ├── 5) 버전 검증 (→ see 섹션 3.7)
    │       if gameSaveData.saveVersion != CURRENT_VERSION:
    │           MigrateSaveData(gameSaveData)
    │
    ├── 6) 데이터 무결성 검증 (→ see 섹션 5.2)
    │       ValidateSaveData(gameSaveData)
    │
    ├── 7) 각 시스템에 데이터 복원 (순서 중요!)
    │       ├── [1] TimeManager.LoadSaveData(time)
    │       │       → 시간 복원 (다른 시스템이 현재 시간을 참조하므로 가장 먼저)
    │       ├── [2] WeatherSystem.LoadSaveData(weather)
    │       │       → 날씨 복원 (시드 기반 난수 상태 재현)
    │       ├── [3] EconomyManager.LoadSaveData(economy)
    │       │       → 골드, 거래 기록, 가격 보정 복원
    │       ├── [4] FarmGrid.LoadSaveData(farm)
    │       │       → 타일/작물 복원 (SO 참조 재연결 포함, → see 섹션 4.3)
    │       └── [5] PlayerController.LoadSaveData(player)
    │               → 위치, 인벤토리, 레벨 복원
    │
    ├── 8) _activeSlot = slotIndex
    │
    ├── 9) OnLoadCompleted 이벤트 발행
    │
    └── 10) 에러 발생 시: OnLoadError → 기본값 폴백 (→ see 섹션 5.3)
```

**복원 순서 근거**: TimeManager가 가장 먼저 복원되어야 WeatherSystem이 현재 계절을 알 수 있고, EconomyManager가 가격을 재계산할 때 시간/날씨 정보가 필요하다. FarmGrid는 CropData SO 참조를 DataRegistry에서 검색하므로 DataRegistry 초기화 후에 실행되어야 한다. Player는 인벤토리에 아이템 SO 참조가 필요하므로 마지막.

#### 3.6 ISaveable 인터페이스

```csharp
// illustrative
namespace SeedMind.Core
{
    /// <summary>
    /// 저장/로드 대상 시스템이 구현하는 인터페이스.
    /// SaveManager가 이 인터페이스를 통해 각 시스템과 통신한다.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>저장 시 호출. 복원 순서 (낮을수록 먼저 로드).</summary>
        int SaveLoadOrder { get; }

        /// <summary>현재 상태를 직렬화 가능한 객체로 반환.</summary>
        object GetSaveData();

        /// <summary>직렬화된 데이터에서 상태를 복원.</summary>
        void LoadSaveData(object data);
    }
}
```

**SaveLoadOrder 값 할당**:

| 시스템 | SaveLoadOrder | 근거 |
|--------|:------------:|------|
| TimeManager | 10 | 모든 시스템의 시간 기준 |
| WeatherSystem | 20 | 시간 의존 |
| EconomyManager | 30 | 시간/날씨 의존 |
| FarmGrid | 40 | SO 참조 복원 필요 |
| PlayerController | 50 | 인벤토리 SO 참조 필요 |

#### 3.7 자동 저장

자동 저장 트리거:

| 트리거 | 이벤트 소스 | 설명 |
|--------|-----------|------|
| 하루 종료 | `TimeManager.OnDayChanged` | 새 날이 시작될 때 이전 날 상태를 자동 저장 |
| 계절 변경 | `TimeManager.OnSeasonChanged` | 계절 전환은 중요한 게임 상태 변경 시점 |
| 수동 저장 | 플레이어 메뉴 조작 | 일시정지 메뉴에서 저장 선택 |

자동 저장은 현재 `_activeSlot`에 덮어쓴다. `_activeSlot == -1`이면 자동 저장을 건너뛴다.

```csharp
// illustrative — SaveManager 내부
private void OnDayChangedHandler(int newDay)
{
    if (_activeSlot >= 0)
    {
        AutoSave(); // 현재 슬롯에 저장
    }
}
```

[OPEN] 자동 저장 빈도가 높으면 프레임 드롭이 발생할 수 있다. 비동기 파일 쓰기로 대응하되, 매일 자동 저장이 과도한지 테스트 필요.

#### 3.8 세이브 버전 관리 (마이그레이션)

세이브 포맷이 변경될 때 이전 버전 세이브를 현재 버전으로 변환하는 시스템.

```csharp
// illustrative
namespace SeedMind.Core
{
    public static class SaveMigrator
    {
        /// <summary>
        /// 세이브 데이터 버전이 현재와 다르면 단계적으로 마이그레이션.
        /// 1.0.0 → 1.1.0 → 1.2.0 ... 순서로 처리.
        /// </summary>
        public static GameSaveData Migrate(GameSaveData data)
        {
            var version = new Version(data.saveVersion);

            // 버전별 마이그레이션 체인
            if (version < new Version("1.1.0"))
                data = Migrate_100_to_110(data);
            if (version < new Version("1.2.0"))
                data = Migrate_110_to_120(data);
            // ...

            data.saveVersion = SaveManager.CURRENT_VERSION;
            return data;
        }

        // 각 마이그레이션 함수는 필드 추가/제거/변환을 처리
        private static GameSaveData Migrate_100_to_110(GameSaveData data)
        {
            // 예: 1.1.0에서 CropInstanceSaveData에 totalWateredDays 필드 추가
            // → 기존 데이터에 기본값 0 설정
            return data;
        }
    }
}
```

**마이그레이션 원칙**:
- 버전은 Semantic Versioning (major.minor.patch) 사용
- Major 변경: 하위 호환 불가 (이전 세이브 로드 불가, 경고 표시)
- Minor 변경: 하위 호환 가능 (마이그레이션으로 변환)
- Patch 변경: 세이브 포맷 변경 없음

---

### 4. 데이터 흐름도

#### 4.1 저장 방향: SO → 런타임 → 세이브 → JSON

```
[ScriptableObject (에디터 에셋)]
  CropData, ToolData, EconomyConfig, ...
  ※ SO 자체는 저장하지 않음. dataId만 세이브에 기록.
         │
         │ 런타임 참조
         ▼
[런타임 인스턴스 (메모리)]
  CropInstance:
    ├── data: CropData (SO 참조)
    ├── currentGrowthDays: 3
    ├── growthStage: 2
    ├── isWatered: true
    └── ...
         │
         │ GetSaveData()
         ▼
[세이브 데이터 (직렬화 가능 클래스)]
  CropInstanceSaveData:
    ├── cropDataId: "tomato"       ← SO.dataId
    ├── currentGrowthDays: 3
    ├── growthStage: 2
    ├── isWatered: true
    └── ...
         │
         │ JsonConvert.SerializeObject()
         ▼
[JSON 파일]
  save_0.json:
    {
      "saveVersion": "1.0.0",
      "farm": {
        "tiles": [{
          "crop": {
            "cropDataId": "tomato",
            "currentGrowthDays": 3,
            ...
          }
        }]
      }
    }
```

#### 4.2 로드 방향: JSON → 세이브 → 런타임 (+ SO 재연결)

```
[JSON 파일]
  save_0.json 읽기
         │
         │ JsonConvert.DeserializeObject()
         ▼
[세이브 데이터]
  CropInstanceSaveData:
    cropDataId: "tomato"
         │
         │ DataRegistry.Get<CropData>("tomato")
         ▼
[SO 참조 재연결]
  CropData SO_Crop_Tomato 획득
         │
         │ new CropInstance(cropData) + 필드 복원
         ▼
[런타임 인스턴스 복원]
  CropInstance:
    data: SO_Crop_Tomato (SO 참조 복원 완료)
    currentGrowthDays: 3
    growthStage: 2
    ...
```

#### 4.3 SO 참조 복원 전략

SO는 에디터 에셋이므로 JSON에 직렬화하지 않는다. 대신 `dataId` 문자열만 저장하고, 로드 시 `DataRegistry`를 통해 재연결한다.

```csharp
// illustrative — FarmGrid.LoadSaveData 내부
public void LoadSaveData(FarmSaveData data)
{
    foreach (var tileSave in data.tiles)
    {
        var tile = GetTile(tileSave.x, tileSave.y);
        tile.SetState((TileState)tileSave.stateIndex);
        tile.SetNeglectDays(tileSave.neglectDays);

        if (tileSave.crop != null
            && !string.IsNullOrEmpty(tileSave.crop.cropDataId))
        {
            // SO 참조 복원: dataId → DataRegistry → CropData
            var cropData = DataRegistry.Instance.Get<CropData>(
                tileSave.crop.cropDataId
            );

            if (cropData != null)
            {
                var crop = new CropInstance(cropData);
                crop.RestoreFromSaveData(tileSave.crop);
                tile.SetCrop(crop);
            }
            else
            {
                // SO를 찾지 못함 → 기본값 폴백 (→ see 섹션 5.3)
                Debug.LogWarning(
                    $"[FarmGrid] 작물 SO 미발견: {tileSave.crop.cropDataId}"
                    + " → 해당 타일을 Tilled 상태로 복원"
                );
                tile.SetState(TileState.Tilled);
            }
        }
    }
}
```

**참조 복원이 필요한 SaveData 필드 목록**:

| SaveData 클래스 | 필드 | 참조 대상 SO |
|----------------|------|-------------|
| `CropInstanceSaveData.cropDataId` | CropData | `DataRegistry.Get<CropData>(id)` |
| `CropInstanceSaveData.fertilizerDataId` | FertilizerData | `DataRegistry.Get<FertilizerData>(id)` |
| `InventorySlotSaveData.itemId` | GameDataSO (다형) | `DataRegistry.Get<GameDataSO>(id)` |

---

### 5. 데이터 검증 시스템

#### 5.1 Editor-time 검증 (SO 필드 유효성)

MCP로 SO를 대량 생성한 후 필드가 올바르게 설정되었는지 검증하는 에디터 도구.

```csharp
// illustrative — Editor 스크립트
namespace SeedMind.Editor
{
    public static class DataValidator
    {
        /// <summary>
        /// 프로젝트 내 모든 GameDataSO를 검증.
        /// MCP 콘솔에서 실행 가능.
        /// </summary>
        [MenuItem("SeedMind/Validate All Data")]
        public static void ValidateAll()
        {
            var allData = Resources.LoadAll<GameDataSO>("Data");
            var errors = new List<string>();
            var idSet = new HashSet<string>();

            foreach (var data in allData)
            {
                // 1) 기본 검증 (GameDataSO.Validate)
                if (!data.Validate(out string error))
                    errors.Add(error);

                // 2) dataId 고유성 검증
                if (!idSet.Add(data.dataId))
                    errors.Add(
                        $"중복 dataId: {data.dataId} ({data.name})"
                    );

                // 3) 타입별 추가 검증
                if (data is CropData crop)
                    ValidateCropData(crop, errors);
                else if (data is ToolData tool)
                    ValidateToolData(tool, errors);
                // ...
            }

            // 결과 출력
            if (errors.Count == 0)
                Debug.Log("[DataValidator] 모든 데이터 검증 통과");
            else
                foreach (var e in errors)
                    Debug.LogError($"[DataValidator] {e}");
        }

        private static void ValidateCropData(
            CropData crop, List<string> errors)
        {
            // growthDays는 1 이상이어야 한다
            // growthStagePrefabs 배열 길이 == growthStageCount
            // seedPrice, sellPrice > 0
            // allowedSeasons != SeasonFlag.None
        }
    }
}
```

**검증 규칙 목록**:

| 대상 SO | 검증 규칙 |
|---------|----------|
| 모든 GameDataSO | `dataId` 비어있지 않음, 프로젝트 내 유일함 |
| CropData | growthDays >= 1, seedPrice > 0, sellPrice > 0, allowedSeasons != None, growthStagePrefabs.Length == growthStageCount |
| ToolData | toolType 유효, tier >= 1, nextTier 참조 체인에 순환 없음 |
| EconomyConfig | sellPriceFloor < sellPriceCeiling, supplyDecayRate >= 0 |
| PriceData | basePrice > 0, seasonMultipliers.Length == 4, demandThreshold >= 1 |
| TimeConfig | daysPerSeason >= 1, secondsPerGameHour > 0 |

#### 5.2 Runtime 검증 (로드 후 데이터 무결성)

세이브 데이터를 로드한 후 무결성을 확인하는 검증 단계.

```csharp
// illustrative
namespace SeedMind.Core
{
    public static class SaveDataValidator
    {
        public static List<string> Validate(GameSaveData data)
        {
            var warnings = new List<string>();

            // 1) 메타 검증
            if (string.IsNullOrEmpty(data.saveVersion))
                warnings.Add("saveVersion 누락");

            // 2) 시간 검증
            if (data.time != null)
            {
                if (data.time.day < 1 || data.time.day > 28)
                    // → see docs/systems/time-season.md for daysPerSeason
                    warnings.Add(
                        $"day 범위 초과: {data.time.day} (유효: 1~28)"
                    );
                if (data.time.seasonIndex < 0 || data.time.seasonIndex > 3)
                    warnings.Add(
                        $"season 범위 초과: {data.time.seasonIndex}"
                    );
            }

            // 3) 경제 검증
            if (data.economy != null)
            {
                if (data.economy.currentGold < 0)
                    warnings.Add("골드가 음수");
            }

            // 4) 농장 검증
            if (data.farm != null)
            {
                foreach (var tile in data.farm.tiles)
                {
                    // SO 참조 존재 확인
                    if (tile.crop != null
                        && !string.IsNullOrEmpty(tile.crop.cropDataId))
                    {
                        if (DataRegistry.Instance.Get<CropData>(
                                tile.crop.cropDataId) == null)
                            warnings.Add(
                                $"미등록 cropDataId: {tile.crop.cropDataId}"
                                + $" at ({tile.x},{tile.y})"
                            );
                    }
                }
            }

            return warnings;
        }
    }
}
```

#### 5.3 기본값 폴백 전략

데이터가 손상되었거나 누락되었을 때의 폴백 규칙.

| 상황 | 폴백 처리 |
|------|----------|
| `cropDataId`로 SO를 찾지 못함 | 해당 타일을 `Tilled` 상태로 복원, 작물 제거 |
| `fertilizerDataId`로 SO를 찾지 못함 | 비료 효과 무시, 기본 성장 속도 적용 |
| `itemId`로 인벤토리 아이템 SO를 찾지 못함 | 해당 슬롯을 비움 |
| `currentGold`가 음수 | 0으로 클램프 |
| `time.day` 범위 초과 | 1로 리셋, 경고 로그 |
| `growthDays`가 `growthDays(SO)`를 초과 | SO의 `growthDays` 값으로 클램프 |
| 세이브 파일 파싱 실패 | 새 게임 데이터로 시작, 플레이어에게 경고 UI 표시 |
| 세이브 버전이 major 불일치 | 로드 거부, "호환되지 않는 세이브" 메시지 표시 |

---

### 6. MCP 태스크 시퀀스

데이터 파이프라인을 MCP for Unity를 통해 구축하는 단계적 태스크. 상세 태스크는 `docs/mcp/data-pipeline-tasks.md`에 별도 작성 예정.

#### Phase A: SO 베이스 및 레지스트리 (MCP 4단계)

```
Step A-1: Scripts/Core/ 에 GameDataSO.cs 작성
          → abstract class, dataId/displayName/icon 필드
          → Validate() 가상 메서드

Step A-2: Scripts/Core/ 에 DataRegistry.cs 작성
          → Singleton<DataRegistry> 상속
          → Initialize(): Resources.LoadAll<GameDataSO>("Data")
          → Get<T>(string dataId), GetAll<T>() 메서드

Step A-3: 기존 SO 클래스 수정 (CropData, ToolData, FertilizerData 등)
          → ScriptableObject 상속 → GameDataSO 상속으로 변경
          → cropId/toolName 등 → dataId/displayName 통합
          → Validate() 오버라이드 추가

Step A-4: SCN_Farm 씬에 DataRegistry 오브젝트 생성
          → DataRegistry.cs 컴포넌트 부착
          → DontDestroyOnLoad 확인
          → Play Mode에서 SO 스캔 테스트
```

#### Phase B: SaveManager 구축 (MCP 5단계)

```
Step B-1: Scripts/Core/ 에 ISaveable.cs 인터페이스 작성
          → SaveLoadOrder, GetSaveData(), LoadSaveData() 정의

Step B-2: Scripts/Core/ 에 SaveManager.cs 작성
          → Singleton, DontDestroyOnLoad
          → Save/Load/AutoSave/DeleteSave 메서드
          → ISaveable 등록/해제

Step B-3: Scripts/Core/ 에 SaveMigrator.cs 작성
          → 버전 비교 및 마이그레이션 체인

Step B-4: 각 시스템에 ISaveable 구현 추가
          → TimeManager (order: 10)
          → WeatherSystem (order: 20)
          → EconomyManager (order: 30)
          → FarmGrid (order: 40)
          → PlayerController (order: 50)

Step B-5: Play Mode 통합 테스트
          → 게임 상태 변경 → Save → 씬 리로드 → Load
          → Console 로그로 복원 상태 확인
          → JSON 파일 내용 직접 확인
```

#### Phase C: SaveData 클래스 작성 (MCP 3단계)

```
Step C-1: 각 시스템 Data/ 폴더에 SaveData 클래스 작성
          → PlayerSaveData, InventorySlotSaveData
          → FarmSaveData, FarmTileSaveData, CropInstanceSaveData
          → (TimeSaveData, WeatherSaveData, EconomySaveData는 기존 정의 활용)

Step C-2: StringIntPair 헬퍼 클래스 작성
          → PriceFluctuationSaveData의 Dictionary 직렬화 대응

Step C-3: SaveDataValidator, DataValidator 에디터 스크립트 작성
          → Editor/ 폴더에 배치
          → MenuItem으로 MCP 콘솔에서 실행 가능하게 구성
```

---

### Open Questions

**Part I (게임 디자인)**:

- [OPEN] InventoryItemData를 별도 SO로 만들지, IInventoryItem 인터페이스로 기존 SO에 통합할지 (Part I 섹션 2.9 참조)
- [OPEN] 가공 레시피를 작물별 개별 에셋으로 할지, ProcessingType별 에셋으로 통합할지 (Part I 섹션 2.5 참조)
- [OPEN] 해금 데이터를 별도 저장할지, 플레이어 레벨로부터 역산할지 (Part I 섹션 3.3 참조)
- [OPEN] 온실 내부 경작 가능 영역 크기와 업그레이드 확장 가능성 (Part I 섹션 2.4 참조)
- [OPEN] 씨앗봉투의 업그레이드 가능 여부 (Part I 섹션 2.3 참조)
- [OPEN] 레벨 8~10 해금 콘텐츠 (Part I 섹션 2.6 참조)
- [OPEN] 세이브 파일 버전 마이그레이션 전략 -- 필드 추가/삭제 시 이전 세이브 호환성 처리 방법
- [OPEN] 시설 업그레이드 시스템(upgradeCosts)의 상세 설계 미정

**Part II (기술 아키텍처)**:

- [OPEN] DataRegistry 초기화에 `Resources.LoadAll`을 사용하므로, SO 에셋을 `_Project/Resources/Data/` 하위로 이동해야 한다. 기존 `_Project/Data/` 경로와 충돌하지 않도록 구조를 재배치해야 한다.
- [OPEN] 자동 저장 빈도(매일)가 성능에 미치는 영향. 비동기 쓰기로 충분한지, 별도 스레드 필요한지.
- [OPEN] Development 빌드에서 JSON 가독성(Indented) vs Release 빌드에서 크기 최적화(None) 전환 메커니즘.
- [OPEN] 세이브 슬롯 3개 외에 자동 저장 전용 슬롯을 별도로 둘지 (예: `save_auto.json`).
- [OPEN] MCP로 SO를 생성할 때 `GameDataSO` 베이스를 사용하려면, MCP가 커스텀 상속 SO의 CreateInstance를 지원하는지 확인 필요.

---

### Risks

**Part I (게임 디자인)**:

- [RISK] **SO 에셋 규모**: 약 87개 SO 에셋 -- MCP를 통한 대량 생성/설정의 정확성 검증 필요 (특히 배열/참조 필드)
- [RISK] **세이브 파일 위변조**: 보호 장치 부재 -- 데이터 손상 시 복구 로직만 존재 (Part I 섹션 4.5 참조)
- [RISK] **ID 문자열 기반 역참조**: 런타임에 SO를 찾지 못하면 게임 상태가 깨질 수 있음. ID 레지스트리 또는 에디터 검증 도구 필요
- [RISK] **레벨 경험치 테이블**: 플레이테스트 없이는 적절성을 검증할 수 없음 (Part I 섹션 2.6 참조)
- [RISK] **레벨 8~10 해금 콘텐츠 미정**: CON 계열 태스크에서 확정 필요

**Part II (기술 아키텍처)**:

- [RISK] **MCP SO 조작 한계**: MCP for Unity가 ScriptableObject의 배열/참조 필드를 안정적으로 설정할 수 있는지 미검증. Phase 2 최우선 검증 대상.
- [RISK] **Dictionary 직렬화**: `PriceFluctuationSaveData`의 Dictionary를 `StringIntPair[]`로 변환하는 방식은 성능 오버헤드가 있으나, 현재 스코프(수십 품목)에서는 무시 가능.
- [RISK] **Resources 폴더 빌드 포함**: `Resources.LoadAll`은 해당 폴더의 모든 에셋을 빌드에 포함시킨다. SO 수가 100개를 넘으면 Addressables 전환을 검토해야 한다.
- [RISK] **세이브 파일 손상**: 비동기 쓰기 중 앱 크래시 시 JSON이 불완전할 수 있다. 원자적 쓰기(임시 파일 -> rename) 패턴을 적용해야 한다.
- [RISK] **순환 참조 직렬화**: ToolData.nextTier 같은 SO->SO 참조가 런타임에서 순환을 형성하면 DataValidator가 감지해야 한다. 직렬화 자체에는 dataId만 기록하므로 JSON 순환 문제는 없다.
- [RISK] **GameDataSO 베이스 도입에 따른 기존 문서 수정**: farming-architecture.md, economy-architecture.md, time-season-architecture.md의 SO 클래스 정의와 코드 예시를 모두 갱신해야 한다. 갱신 누락 시 문서 간 불일치 발생.

---

### Cross-references

- `docs/design.md` -- 작물/시설 canonical 데이터 (섹션 4.2, 4.5, 4.6)
- `docs/architecture.md` -- 프로젝트 구조, 데이터 관리 개요 (섹션 3, 6)
- `docs/systems/farming-system.md` -- 타일 상태, 도구, 비료, 토양 품질 canonical (DES-001)
- `docs/systems/farming-architecture.md` -- CropData, FertilizerData, ToolData SO 정의 (ARC-001)
- `docs/systems/crop-growth.md` -- 성장 단계, 품질 시스템 canonical (DES-002)
- `docs/systems/economy-system.md` -- 경제 시스템 설계, 가공 공식 canonical (DES-004)
- `docs/systems/economy-architecture.md` -- EconomyConfig, PriceData, ShopData SO 정의, EconomySaveData (ARC-004)
- `docs/systems/time-season.md` -- 시간/계절/날씨 canonical (DES-003)
- `docs/systems/time-season-architecture.md` -- TimeConfig, SeasonData, WeatherData SO 정의, TimeSaveData/WeatherSaveData (DES-003)
- `docs/systems/project-structure.md` -- Unity 프로젝트 구조, 에셋 네이밍 규칙
- `docs/systems/gathering-system.md` -- 채집 시스템 canonical: 포인트/아이템 목록, 숙련도, 채집 낫 등급 (DES-015)
- `docs/systems/gathering-architecture.md` -- GatheringPointData, GatheringItemData, GatheringConfig SO 정의, GatheringManager 설계 (ARC-031/ARC-033)
- `docs/systems/collection-architecture.md` -- GatheringCatalogData SO 정의, GatheringCatalogManager 설계 (ARC-037/ARC-045)
- `docs/systems/collection-system.md` -- 수집 도감 canonical: 초회 발견 보상 기준 (섹션 3.3, DES-018)
- `docs/content/gathering-items.md` -- 채집 아이템 canonical: 힌트 텍스트, 설명, 아이템 ID (CON-012)
- `docs/balance/crop-economy.md` -- 작물 경제 밸런스 (작성 예정, BAL-001)
- `docs/balance/progression.md` -- 전체 진행 밸런스 (작성 예정, BAL-002)

---

*이 문서는 Claude Code가 기존 설계/아키텍처 문서들의 데이터 구조를 통합하고, 게임 디자인 관점의 데이터 분류 및 저장/로드 파이프라인의 기술 설계를 자율적으로 작성했습니다.*
