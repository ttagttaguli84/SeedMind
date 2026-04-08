# Devlog #105 — FIX-113: data-pipeline.md 장식 시스템 SO 스키마 추가

> 작성: Claude Code (Sonnet 4.6) | 2026-04-08

---

## 작업 요약

**FIX-113**: `data-pipeline.md` 섹션 2.14~2.15에 `DecorationItemData` / `DecorationConfig` SO 에셋 스키마 추가 (PATTERN-007 준수)

---

## 변경 내용

### `docs/pipeline/data-pipeline.md`

#### 섹션 1.1 SO 테이블 갱신
- `DecorationItemData` (29개) / `DecorationConfig` (1개) 행 추가
- 총 예상 에셋 수: ~217개 → **~247개** (ARC-043 +30)

#### 섹션 2.14 DecorationItemData (신규)
- canonical 클래스 정의: `docs/systems/decoration-architecture.md` 섹션 2.2 참조
- 필드 테이블 20개 정의 (itemId, displayName, icon, category, buyPrice, isEdgePlaced, tileWidthX, tileHeightZ, unlockLevel, unlockZoneId, limitedSeason, lightRadius, moveSpeedBonus, durabilityMax, prefab, floorTile, edgeTileH, edgeTileV, edgeTileCorner, description)
- buyPrice/unlockLevel 등 콘텐츠 수치는 모두 `decoration-system.md` 섹션 2.1~2.5 참조로만 기재 (PATTERN-007 준수)
- 에셋 총 29종 명시 (Fence 4 + Path 5 + Light 4 + Ornament 11 + WaterDecor 5)

#### 섹션 2.15 DecorationConfig (신규)
- canonical 클래스 정의: `docs/systems/decoration-architecture.md` 섹션 2.3 참조
- 필드 테이블 5개 정의 (validHighlightColor, invalidHighlightColor, fenceDurabilityDecayPerSeason, fenceRepairCostRatio, pathSpeedBonusEnabled)
- 콘텐츠 수치는 `decoration-system.md` 참조로만 기재 (PATTERN-007 준수)
- 에셋 경로: `Assets/_Project/Data/Config/SO_DecorationConfig.asset`

---

## 설계 메모

- ARC-043에서 도입된 SO 2종이 data-pipeline.md 섹션 1.1 테이블에 누락되어 있었음
- 기존 섹션 2.10~2.13 패턴(GatheringPointData/GatheringItemData/GatheringConfig/GatheringCatalogData) 그대로 준수
- 카테고리별 파라미터(lightRadius/moveSpeedBonus/durabilityMax)는 미사용 카테고리에서 0으로 유지되는 설계임을 주석으로 명시

---

## Cross-references
- `docs/systems/decoration-architecture.md` (ARC-043) — canonical SO 클래스 정의
- `docs/systems/decoration-system.md` (DES-023) — canonical 콘텐츠 수치
- `docs/pipeline/data-pipeline.md` — 본 수정 대상

---

## 다음 우선순위 (TODO 기준)
- FIX-106 (Priority 2): collection-architecture.md 섹션 2 GatheringCatalogManager 다이어그램 박스 추가
- FIX-112 (Priority 1): project-structure.md SeedMind.Decoration 네임스페이스/폴더 추가
- ARC-046 (Priority 1): 장식 시스템 MCP 태스크 시퀀스 문서화
