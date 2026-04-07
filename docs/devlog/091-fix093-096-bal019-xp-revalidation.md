# Devlog #091 — FIX-093~096 + BAL-019: ARC-037 후속 수정 + XP 비중 재검증

> 2026-04-08 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

두 개의 논리적 작업 단위를 처리했다.

---

## Task 1: FIX-093 / 094 / 095 / 096 — ARC-037 후속 배치 수정

DES-018(수집 도감 시스템) 이후 반영이 지연된 4개의 단순 FIX 항목을 일괄 처리했다.

### FIX-093: save-load-architecture.md — GatheringCatalogManager SaveLoadOrder 등록

| 변경 항목 | 내용 |
|---------|------|
| JSON 스키마 (섹션 2.2) | `"gatheringCatalog": { }` 필드 추가 (gathering 다음) |
| C# 클래스 (섹션 2.3) | `using SeedMind.Collection;` 추가, `public GatheringCatalogSaveData gatheringCatalog;` 필드 추가 |
| PATTERN-005 카운트 | 시스템 데이터 19→20개, 총 22→23개 |
| SaveLoadOrder 할당표 | GatheringCatalogManager \| 56 행 추가 (InventoryManager 55 → 다음, BuildingManager 60 이전) |

근거: SaveLoadOrder=56은 `docs/systems/collection-architecture.md` 섹션 5.2에서 확정된 값.

### FIX-094: data-pipeline.md — GameSaveData gatheringCatalog 필드 추가

| 변경 항목 | 내용 |
|---------|------|
| JSON 스키마 (섹션 3.2) | `"gatheringCatalog": { }` 추가 (gathering 다음) |
| C# GameSaveData (Part II) | `public GatheringCatalogSaveData gatheringCatalog;` 추가 |

PATTERN-005: JSON/C# 양쪽 동시 반영 완료.

### FIX-095: project-structure.md — SeedMind.Collection 네임스페이스·폴더 추가

| 변경 항목 | 내용 |
|---------|------|
| 네임스페이스 목록 (섹션 2) | `SeedMind.Collection`, `SeedMind.Collection.Data` 2행 추가 |
| Scripts/ 폴더 트리 | `Collection/` 폴더 + 주요 파일 5개 명시 |
| asmdef 테이블 | `SeedMind.Collection.asmdef` 행 추가 (Core, Player 의존) |

### FIX-096: fish-catalog.md — UI 경로 수정

| 변경 항목 | 내용 |
|---------|------|
| 섹션 5.1 접근 경로 | `메뉴 > 도감 > 어종 도감 탭` → `메뉴 > 수집 도감 > 어종 탭` |

DES-018에서 통합 수집 도감(CollectionUIController)이 채택되었으나 fish-catalog.md의 접근 경로가 구버전 표기로 잔존하고 있었다.

---

## Task 2: BAL-019 — 업적 XP 비중 재검증

### 배경

TODO 원문: "업적 39종 3,130 XP, 비중 ~68% — 목표 33~43% 초과 이슈."
이 수치는 이전 XP 테이블(레벨 10 = 4,609 XP) 기준 계산이었다.

### 분석 결과

현행 canonical XP 테이블(baseXP=80, growthFactor=1.60):

| 지표 | 수치 | 판정 |
|------|------|------|
| 레벨 10 목표 XP | 9,029 XP | canonical (progression-curve.md 섹션 2.4.1) |
| 업적 XP 합계 | 3,160 XP | canonical (achievements.md 섹션 13.1) |
| **업적 단독 비중** | **35.0%** | **목표 30~40% ✓ 범위 내** |
| 보조 소스 합산 | 4,626 XP | — |
| 보조 소스 비중 | 51.2% | 과다 여부 모니터링 필요 |

### 결정: 옵션 C + D (현상 유지 + 목표 범위 재정의)

업적 XP 하향이나 레벨 테이블 상향은 불필요. ~68%는 4,609 XP 기반의 오래된 수치였다.

**목표 범위 재정의:**
- 업적 XP 비중: 33~43% → **30~40%** (현실 35.0%를 중앙에 배치)
- 1년차 업적 비율 목표: 5~10% → **5~15%** (낚시·채집 업적 추가로 1년차 달성 가능 폭 확대)
- 도감 초회 보상: 신규 카테고리 1~3% 추가

### 후속 문서 수정

| 파일 | 내용 |
|------|------|
| `docs/balance/bal-019-xp-balance.md` | 신규 — 분석 및 결정 문서 |
| `docs/balance/xp-integration.md` | 섹션 2.3 PATTERN-009 히스토리 배너 추가, 비율 목표 갱신, 섹션 5.3 hard cap 1,010→1,115 정정 |
| `docs/content/achievements.md` | 섹션 2.4 목표 범위 30~40% 반영 |

---

## 리뷰 결과 (BAL-019)

Reviewer가 다음 이슈를 추가 발견·수정했다:

| 심각도 | 위치 | 이슈 | 처리 |
|--------|------|------|------|
| CRITICAL | progression-curve.md 섹션 2.4.4 | "업적 2,640 XP" — 출처 불명 수치. PATTERN-009 배너 누락 | 배너 추가 + 수치 정정 |
| WARNING | xp-integration.md 섹션 4.1 | 시나리오 비교표의 "2,250 XP" — CON-017 이전 추정치, 배너 누락 | 배너 추가 |
| WARNING | xp-integration.md 섹션 5.3 | 퀘스트 hard cap 1,010 XP → 정확한 값은 1,115 XP | 1,115로 정정 |

계산식 15개 전수 검증 완료 — 모두 정확.

---

## 수정된 파일 목록

| 파일 | 변경 내용 |
|------|---------|
| `docs/systems/save-load-architecture.md` | FIX-093: JSON/C# gatheringCatalog 추가, SaveLoadOrder 행 추가, 카운트 갱신 |
| `docs/pipeline/data-pipeline.md` | FIX-094: JSON/C# gatheringCatalog 필드 추가 |
| `docs/systems/project-structure.md` | FIX-095: SeedMind.Collection 네임스페이스/폴더/asmdef 추가 |
| `docs/content/fish-catalog.md` | FIX-096: UI 경로 수정 |
| `docs/balance/bal-019-xp-balance.md` | 신규: BAL-019 분석 문서 |
| `docs/balance/xp-integration.md` | BAL-019 결정 반영, 히스토리 배너, 수치 정정 |
| `docs/content/achievements.md` | 비중 목표 30~40% 반영 |
| `docs/balance/progression-curve.md` | 리뷰: 섹션 2.4.4 배너 추가 + 2,640 수치 정정 |
| `TODO.md` | FIX-093/094/095/096/BAL-019 완료, 신규 항목 5개 추가 |

---

*이 문서는 Claude Code가 FIX-093~096 + BAL-019 세션에서 자율적으로 작성했습니다.*
