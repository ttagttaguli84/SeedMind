# Devlog #070 — BAL-014 + FIX-076~081: 낚시 XP 밸런스 확정 및 채집 시스템 Downstream 반영

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

이번 세션은 BAL-014(낚시 숙련도 XP 밸런스 검증)와 FIX-076~080(채집 시스템 enum/필드 downstream 반영)을 완료했다. Reviewer가 CRITICAL 2건(progression-architecture.md 이벤트 구독 누락, data-pipeline.md JSON-C# 필드명 불일치)을 발견하여 즉시 수정했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/balance/bal-014-fishing-xp-balance.md` | BAL-014 신규: 낚시 XP 2,250→4,500 상향 확정, 레벨 10 도달 39일 |
| `docs/systems/fishing-system.md` | 섹션 7.2 XP 테이블 교체, 섹션 7.3 도달 일수 39일로 수정 |
| `docs/systems/economy-architecture.md` | FIX-076: HarvestOrigin.Gathering=4, SupplyCategory.Forage=4 추가 |
| `docs/systems/progression-architecture.md` | FIX-077: XPSource.GatheringComplete + OnEnable() 구독 + 이벤트 흐름 다이어그램 |
| `docs/systems/inventory-architecture.md` | FIX-078: ItemType.Gathered 추가 |
| `docs/pipeline/data-pipeline.md` | FIX-078/080: ItemType.Gathered 테이블, GameSaveData.gathering, JSON 스키마 canonical 참조 |
| `docs/systems/save-load-architecture.md` | FIX-079: SaveLoadOrder=54 GatheringManager 추가 |
| `TODO.md` | BAL-014/FIX-072/076~080 완료 처리, FIX-081/ARC-032~033/BAL-015/CON-012~013/DES-017 신규 추가 |

---

## BAL-014 낚시 숙련도 XP 밸런스 결과

### 핵심 발견

기존 추산(레벨 10 도달 45일)은 과대평가였다. 실제 시뮬레이션에서:

| 시나리오 | 기존 XP(2,250) 기준 | 조정 후 XP(4,500) 기준 |
|----------|-------------------|----------------------|
| A — 일반 (하루 1세션) | **21일** (너무 빠름) | **39일** (목표 범위 내) |
| B — 캐주얼 (하루 0.5세션) | 37일 | 68일 |
| C — 최적화 (하루 2세션) | 13일 | 24일 |

기존 XP 설계는 실패 XP(1 XP/실패), Lv.8 에너지 반감 효과, 희귀도 혼합 효과를 미반영했다.

### 확정 XP 구조 (총 4,500 XP)

| 레벨 | Lv.간 XP | 누적 XP |
|------|----------|---------|
| 1→2 | 100 | 100 |
| 2→3 | 200 | 300 |
| 3→4 | 300 | 600 |
| 4→5 | 400 | 1,000 |
| 5→6 | 500 | 1,500 |
| 6→7 | 600 | 2,100 |
| 7→8 | 700 | 2,800 |
| 8→9 | 800 | 3,600 |
| 9→10 | 900 | 4,500 |

균등 증가 구조로 후반 레벨에 집중적인 투자가 필요하며, 낚시 전문화 플레이어에게 명확한 목표를 제공한다.

---

## FIX-076~080 채집 시스템 Downstream 반영

### FIX-076: economy-architecture.md

- `HarvestOrigin.Gathering = 4` 추가 → switch 전수 업데이트
- `SupplyCategory.Forage = 4` 추가 → 수급 파라미터 (minFactor ~0.7, maxFactor ~1.3)

### FIX-077: progression-architecture.md

- `XPSource.GatheringComplete` 추가 → GetExpForSource switch 업데이트
- OnEnable() 구독 목록: `GatheringEvents.OnGatheringCompleted += HandleGatheringXP` 추가
- 이벤트 흐름 다이어그램 섹션 1.2 업데이트

### FIX-078: inventory-architecture.md + data-pipeline.md

- `ItemType.Gathered` 추가 (Fish 다음)
- data-pipeline.md ItemType 테이블 동기화

### FIX-079: save-load-architecture.md

- SaveLoadOrder 할당표에 `GatheringManager | 54` 추가

### FIX-080: data-pipeline.md

- `GameSaveData`에 `public GatheringSaveData gathering;` 추가
- JSON 스키마 섹션 3.2에 canonical 참조 주석 추가 (CRITICAL-2 해소)

---

## CRITICAL 이슈 수정 (Reviewer 발견)

| ID | 심각도 | 이슈 | 수정 |
|----|--------|------|------|
| CRITICAL-1 | 🔴 | progression-architecture.md OnEnable() 구독 목록에 `GatheringEvents.OnGatheringCompleted` 누락 | 섹션 1.1 구독 블록 + 섹션 1.2 이벤트 흐름 다이어그램 동시 업데이트 |
| CRITICAL-2 | 🔴 | data-pipeline.md 섹션 3.2 JSON 스키마 메타 필드명이 C# 클래스와 불일치 (`version`≠`saveVersion`, `saveDate`≠`savedAt`, `playTime`≠`playTimeSeconds`) + `shippingBin` 잔존 | 메타 필드명 C# 기준으로 통일, `shippingBin` 제거, canonical 참조 주석 추가 |

### WARNING (미해소 — 후속 처리)

| 위치 | 내용 | 후속 |
|------|------|------|
| economy-architecture.md 섹션 3.7.1/3.7.2 | 구버전 `GetGreenhouseMultiplier(bool isGreenhouse)` pseudocode 잔존 | FIX-081 등록 |

---

## 세션 후 활성 TODO

| ID | Priority | 상태 |
|----|----------|------|
| FIX-081 | 2 | 신규 (economy-architecture.md 구버전 pseudocode 정리) |
| ARC-032 | 2 | 신규 (채집 MCP 태스크 독립 문서화) |
| BAL-015 | 2 | 신규 (채집 경제 밸런스 시트) |
| CON-012 | 2 | 신규 (채집 아이템 27종 콘텐츠 상세) |
| DES-017 | 2 | 신규 (채집 낫 업그레이드 경로 상세) |
| ARC-033 | 1 | 신규 (채집 SO 에셋 data-pipeline.md 반영) |
| FIX-082 | 1 | 신규 (gathering-system.md Cross-references 보완) |
| CON-013 | 1 | 신규 (채집 퀘스트/업적 콘텐츠) |
| PATTERN-009/010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 BAL-014 + FIX-076~081 태스크에 따라 자율적으로 작성했습니다.*
