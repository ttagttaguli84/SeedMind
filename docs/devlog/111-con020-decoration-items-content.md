# Devlog #111 — CON-020: 장식 아이템 콘텐츠 상세 문서

> 작성: Claude Code (Sonnet 4.6) | 2026-04-08

---

## 작업 요약

**CON-020**: `docs/content/decoration-items.md` 신규 생성 — 29종 장식 아이템 canonical 콘텐츠 스펙 확정

decoration-system.md에 분산된 5개 카테고리별 아이템 데이터를 통합하여, decoration-tasks.md MCP 에셋 생성 시 참조할 canonical 문서 확립. PATTERN-011 트리거가 된 임의 SO ID 생성 문제 해소.

---

## 신규 파일

`docs/content/decoration-items.md` (CON-020)

---

## 확정 아이템 목록 (29종)

| 카테고리 | 종수 | 주요 itemId |
|---------|------|-----------|
| Fence | 4 | FenceWood, FenceStone, FenceIron, FenceFloral |
| Path | 5 | PathDirt, PathGravel, PathStone, PathBrick, PathWood |
| Light | 4 | LightTorch, LightLantern, LightStreet, LightCrystal |
| Ornament | 11 | OrnaScareRaven, OrnaSignBoard, OrnaStatueGold 등 |
| WaterDecor | 5 | WaterLotus, WaterBridge, WaterFountainS/L, WaterDuck |

---

## 리뷰 수정 사항 (CRITICAL 2건 + WARNING 3건 + INFO 2건)

### CRITICAL 즉시 수정

| 파일 | 수정 내용 |
|------|----------|
| `docs/mcp/decoration-tasks.md` D-C-03~07 | canonical 참조 `decoration-system.md 섹션 2.X` → `decoration-items.md 섹션 X.1` |
| `docs/mcp/decoration-tasks.md` [RISK] 2건 | 동일 canonical 교체 |
| `docs/pipeline/data-pipeline.md` 섹션 2.14~2.15 | buyPrice/lightRadius/moveSpeedBonus/durabilityMax canonical 참조 교체 |

### WARNING 수정

| 파일 | 수정 내용 |
|------|----------|
| `docs/mcp/decoration-tasks.md` D-C-06 | Ornament 에셋명 `SO_Deco_OrnamentScarecrow` → `SO_Deco_OrnaScareRaven` (itemId 기반) |
| `docs/mcp/decoration-tasks.md` D-C-07 | WaterDecor 에셋명 `SO_Deco_WaterDecorPond` → `SO_Deco_WaterLotus` (itemId 기반) |
| `docs/mcp/decoration-tasks.md` Cross-references | `decoration-items.md` (CON-020) 항목 추가 |

### INFO 수정

| 파일 | 수정 내용 |
|------|----------|
| `decoration-items.md` 섹션 3 | `[OPEN#2]` → `[OPEN#C]` 정식 등록 |

---

## Open Questions 신규

- **[OPEN#A]** OrnaFlowerPotS/L 복수 시즌 Season 필드 처리 방법 (Flags enum vs Season[] 배열)
- **[OPEN#B]** LightCrystal 색상 팔레트 목록 미결정
- **[OPEN#C]** 야간 조명 가시성 메카닉 미정의 (time-season.md)
