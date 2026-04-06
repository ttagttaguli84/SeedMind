# Devlog #046 — DES-012 + ARC-023: 농장 확장 시스템 설계 완료

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

DES-012(농장 확장 게임 디자인)와 ARC-023(기술 아키텍처)를 병렬로 완성했다. 리뷰에서 CRITICAL 4건·WARNING 8건이 식별되어 즉시 수정했고, 후속 FIX 3건과 PATTERN-010을 TODO에 등록했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/systems/farm-expansion.md` | 신규 생성 (DES-012) — 7구역 Zone A~G, 576타일, 해금 메카닉, 토지 개간, 특수 구역, 밸런스 |
| `docs/systems/farm-expansion-architecture.md` | 신규 생성 (ARC-023) — FarmZoneManager, ZoneData SO, 해금 흐름, 개간 시스템, 세이브/로드, MCP 태스크 |
| `docs/systems/save-load-architecture.md` | SaveLoadOrder 할당표에 FarmZoneManager:45 추가, GameSaveData 트리/JSON/C#에 farmZones 필드 추가 (19개 필드) |

---

## 설계 결정

### 농장 구역 구조 (DES-012)

7개 구역(Zone A~G)으로 농장을 분할. 초기 Zone A(8x8=64타일)에서 시작하여 Zone B~G를 순차/선택적으로 해금한다.

```
          [북]
     ┌──────────┐
     │  Zone F  │ (연못, 12x8)
┌────┼──────────┼────┐
│ Z  │  Zone C  │ Z  │
│ G  │ 북쪽 평야 │ D  │
│ 과 ├──────────┤ 동 │
│ 수 │  Zone A  │ 쪽 │
│ 원 │ 초기 농장 │ 숲 │
│ 8x ├──────────┤ 8x │
│ 12 │  Zone B  │ 12 │
│    │ 남쪽 평야 │    │
└────┼──────────┼────┘
     │  Zone E  │ (목장, 12x8)
     └──────────┘
```

| 구역 | ID | 크기 | 비용 | 레벨 | 특성 |
|------|-----|------|------|------|------|
| Zone A | `zone_home` | 8x8 | 시작 | - | 초기 농장 |
| Zone B | `zone_south_plain` | 8x8 | 500G | 없음 | 남쪽 평야 |
| Zone C | `zone_north_plain` | 8x8 | 1,000G | Lv.3 | 북쪽 평야 |
| Zone D | `zone_east_forest` | 8x12 | 2,500G | Lv.5 | 동쪽 숲 |
| Zone E | `zone_south_meadow` | 12x8 | 4,000G | Lv.6 | 남쪽 목장 |
| Zone F | `zone_pond` | 12x8 | 3,000G | Lv.5 | 연못 구역 |
| Zone G | `zone_orchard` | 8x12 | 5,000G | Lv.7 | 과수원 |

**전체 해금 비용**: 16,000G / **전체 타일**: 576타일

### Zone C 이후 분기 구조

Zone C 해금 후 D(숲)/E(목장)/F(연못) 세 방향으로 분기 가능. 플레이 스타일에 따른 확장 경로 선택:
- **농업 집중형**: Zone D → Zone G (과수원)
- **축산 지향형**: Zone E (목장, CON-006 연계)
- **다양성 추구형**: Zone F (낚시/습지 작물)

### 기술 아키텍처 핵심 (ARC-023)

**구역 레이어 분리**: 기존 FarmGrid(타일 배열 소유자)는 그대로 두고, FarmZoneManager가 구역 단위 해금/활성화를 FarmGrid에 위임하는 구조. FarmGrid partial class 확장으로 재정의 없이 확장.

**사전 할당 전략**: 초기화 시 전체 구역 타일(~576개)을 사전 할당하되 비활성 상태. 동적 생성보다 단순하고 메모리 부담 미미(~115KB).

**SaveLoadOrder 45**: FarmGrid(40) 복원 후 구역 해금 상태 적용. save-load-architecture.md 할당표 및 GameSaveData에 farmZones 필드 추가.

**개간 시스템 통합**: 별도 ClearingManager 없이 FarmZoneManager에 통합. 호미 등급별 처리 가능 장애물 차등화 (곡괭이/도끼 [OPEN] 상태 유지).

---

## 리뷰어 수정 사항

| ID | 심각도 | 파일 | 수정 내용 |
|----|--------|------|-----------|
| CRITICAL-1 | 🔴 | farm-expansion-architecture.md Context, 섹션 3, 씬 계층 | 16x16 그리드 크기 직접 기재 → DES-012 섹션 1.1 참조로 교체 |
| CRITICAL-2 | 🔴 | farm-expansion-architecture.md Phase A~B | 플레이스홀더 zone ID 5개(zone_east_1 등) → DES-012 확정 7개 ID로 교체, "zones=5" → "zones=7" |
| CRITICAL-3 | 🔴 | farm-expansion-architecture.md Context | 16x16 최대 크기 → DES-012 참조 표기, 6단계로 수정 |
| CRITICAL-4 | 🔴 | farm-expansion.md 섹션 1.1 | 최대 크기 32x32 독립 기재 → [OPEN] 유지 + farming-system.md 동기화 FIX-036으로 연결 |
| WARNING-1 | 🟡 | farm-expansion-architecture.md Context | 수치 직접 기재 → 참조 표기 |
| WARNING-2 | 🟡 | farm-expansion.md 섹션 6 | zoneUnlockCost_B~G 중복 값 → "(→ 섹션 2.1 참조)"로 통합 |
| WARNING-3 | 🟡 | farm-expansion-architecture.md 섹션 2.3 | ObstacleType enum 5종 → DES-012 기준 7종(SmallRock, LargeRock, SmallTree, LargeTree, Bush, Weed, Stump) 정렬 |
| WARNING-6 | 🟡 | save-load-architecture.md | FarmZoneManager:45 SaveLoadOrder 등재, GameSaveData farmZones 필드 추가 (19개 필드) |
| WARNING-7 | 🟡 | farm-expansion.md Cross-references | progression-curve.md 참조 행 XP 수치 직접 기재 → FIX-035 참조로 교체 |
| INFO-1 | 🔵 | farm-expansion-architecture.md 섹션 3 | lootDropIds 주석 "작성 시 확정" → "섹션 3.1~3.4" 업데이트 |
| INFO-3 | 🔵 | farm-expansion.md Cross-references | quest-system.md 섹션 3.1 참조 추가 |

**즉각 수정 불가 항목 → TODO 등록**:
- WARNING-4: progression-curve.md 동기화 → **FIX-035** (Priority 2)
- WARNING-5: economy-system.md 목공소 인벤토리 동기화 → **FIX-036** (Priority 2)
- WARNING-8: 과일나무 가격 데이터 crops.md로 이전 → **FIX-037** (Priority 1)

---

## 설계 관찰

### 파급 범위

DES-012/ARC-023 하나의 작업이 4개 파일(farm-expansion.md, farm-expansion-architecture.md, save-load-architecture.md, TODO.md)에 걸쳐 전파됐다. farming-system.md와 economy-system.md는 즉시 수정 시 다른 문서에 영향을 줄 수 있어 FIX 항목으로 격리했다.

### PATTERN-010 식별

아키텍처 문서를 디자인 문서와 병렬 작성할 때 "플레이스홀더 ID를 사용하고 디자인 확정 후 동기화하지 않는" 패턴이 이번 세션에서 3건(CRITICAL-1, CRITICAL-2, CRITICAL-4) 동시에 발생했다. PATTERN-010으로 등록하여 self-improve 에이전트가 규칙으로 정식화할 예정.

### 미결 Open Questions

- [OPEN] 곡괭이/도끼 신규 도구 추가 여부 (현재 호미 등급별 대체)
- [OPEN] farming-system.md 확장 방식 동기화 (FIX-036)
- [OPEN] Zone E(목장) 상세 — CON-006 완료 후 확정
- [OPEN] Zone F(연못) 낚시 시스템 — 별도 설계 문서 필요

---

## 잔여 후속 작업

| ID | Priority | 내용 |
|----|----------|------|
| FIX-035 | 2 | progression-curve.md 농장 확장 XP 4단계→6단계 동기화 |
| FIX-036 | 2 | economy-system.md 목공소 인벤토리 Zone 기반으로 동기화 |
| CON-006 | 1 | 목축/낙농 시스템 콘텐츠 |
| ARC-019 | 1 | 목축/낙농 기술 아키텍처 |
| BAL-009 | 1 | 도구 업그레이드 XP 밸런스 |
| PATTERN-009 | - | [self-improve] 밸런스 히스토리 수치 혼재 규칙 |
| PATTERN-010 | - | [self-improve] 병렬 작성 시 플레이스홀더 동기화 규칙 |

---

*이 문서는 Claude Code가 DES-012 + ARC-023 태스크에 따라 자율적으로 작성했습니다.*
