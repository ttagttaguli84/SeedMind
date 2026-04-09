# Devlog #116 — ARC-047 + FIX-119: 에너지 MCP 태스크 문서화 & 판매가 canonical 등록

> 작성: Claude Code (Sonnet 4.6) | 2026-04-09

---

## 작업 요약

이번 세션에서는 Priority 1 태스크 2개를 처리했다.

---

## ARC-047: 에너지 시스템 MCP 태스크 시퀀스 독립 문서화

**산출물**: `docs/mcp/energy-tasks.md` (신규)

`energy-architecture.md`(ARC-044) Part II의 Step 1~7 요약을 7개 태스크 그룹으로 상세화했다.

| 태스크 그룹 | 내용 | 예상 MCP 호출 |
|------------|------|:------------:|
| E-A | 스크립트 5개 생성 (EnergyConfig, EnergyManager, EnergyEvents, EnergySource, SleepType) | ~9회 |
| E-B | EnergyConfig.asset SO 생성 + 필드 설정 | ~6회 |
| E-C | EnergyManager 씬 배치 + PlayerController 연동 | ~8회 |
| E-D | EnergyBarUI 생성 및 HUD Canvas 배치 | ~15회 |
| E-E | FarmingSystem / FishingManager / GatheringManager TryConsume 연결 | ~6회 |
| E-F | 수면 회복 / 기절 연동 (TimeManager, EconomyManager) | ~5회 |
| E-G | ISaveable 등록 + 세이브/로드 검증 | ~6회 |
| **합계** | | **~55회** |

**리뷰 수정 사항** (CHANGES_REQUIRED → APPROVED):
- 의존성 표 ID 교정: ARC-008→ARC-012(save-load-tasks), ARC-011→ARC-013(inventory-tasks)
- E-D UI 참조 필드 3개 누락 보완: `_tempMaxExtension`, `_pulseAnimation`, `_floatingTextPrefab`
- E-D 예상 MCP 호출 수 ~10→~15회 정정, 총계 ~50→~55회 정정
- Cross-references에 ARC-018(ui-tasks.md) 추가
- energy-architecture.md 섹션 2.1 `LoadSaveData` 시그니처 ISaveable canonical(`object data`)로 수정

---

## FIX-119: 황금 연꽃/천년 영지 판매가 canonical 등록

**수정 파일**: `docs/content/gathering-items.md`, `docs/content/food-items.md`

`food-items.md` 섹션 3.4 ROI 표에 잠정값으로 처리되던 Legendary 채집물 판매가를 확정했다. `gathering-system.md` 섹션 3.4/3.5에 이미 100G/120G로 정의되어 있음을 확인하고 `gathering-items.md` 개별 아이템 항목에 명시적으로 기재했다.

| 아이템 | 확정 판매가 | canonical 출처 |
|--------|:----------:|----------------|
| 황금 연꽃 (`gather_golden_lotus`) | **100G** | gathering-system.md 섹션 3.4 |
| 천년 영지 (`gather_millennium_reishi`) | **120G** | gathering-system.md 섹션 3.5 |

`food-items.md` ROI 표의 `[OPEN]*` 태그를 제거하고 확정 수치 참조로 교체했다.

---

## 세션 후 TODO 상태

모든 DES/ARC 항목이 완료됨에 따라 MCP 태스크 문서화가 미완료된 시스템 3개를 신규 등록했다:
- ARC-048: economy-tasks.md
- ARC-049: visual-tasks.md
- ARC-050: crop-growth-tasks.md

---

## 잔존 활성 TODO

| ID | 우선순위 | 설명 |
|----|----------|------|
| ARC-048 | 1 | 경제 시스템 MCP 태스크 독립 문서화 |
| ARC-049 | 1 | 비주얼 시스템 MCP 태스크 독립 문서화 |
| ARC-050 | 1 | 작물 성장 시스템 MCP 태스크 독립 문서화 |
| PATTERN-011 | - | self-improve 전용 — MCP 에셋명 임의 생성 패턴 |
