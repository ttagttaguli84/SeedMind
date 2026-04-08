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
- 야간 가시성 규칙: (→ see `docs/systems/time-season.md` 야간 가시성 규칙) [OPEN#2 — decoration-system.md 기준]

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

---

## Risks

- [RISK] decoration-tasks.md (ARC-046) SO 에셋 생성 시 itemId를 임의 생성하지 않도록 이 문서의 섹션 6.1 요약 테이블을 반드시 참조해야 한다. PATTERN-011 이슈가 이를 계기로 등록됨.
- [RISK] `OrnaFlowerPotS`/`OrnaFlowerPotL` 복수 시즌 처리([OPEN#A])가 해소되지 않으면 스키마 변경이 decoration-architecture.md, data-pipeline.md, 구현 코드에 연쇄 영향을 준다. 조기 확정 권장.
- [RISK] `WaterBridge` (`tileWidthX = 1`, `tileHeightZ = 3`)는 비정형 점유(1×3)로 배치 충돌 검사 로직이 여타 아이템과 다를 수 있다. 구현 시 타일맵 충돌 처리 주의.
