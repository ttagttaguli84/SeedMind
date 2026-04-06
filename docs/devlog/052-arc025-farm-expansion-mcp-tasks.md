# Devlog #052 — ARC-025: 농장 확장(Zone) 시스템 MCP 태스크 시퀀스 완료

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

농장 확장 시스템(ARC-023/DES-012)의 Unity 구현 시퀀스를 MCP 태스크 문서로 상세화했다. Architect 에이전트가 `docs/mcp/farm-expansion-tasks.md`를 신규 작성하고, Reviewer 에이전트가 CRITICAL 5건을 발견하여 수정 완료했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/mcp/farm-expansion-tasks.md` | ARC-025: 신규 생성 — 9개 태스크 그룹, ~99회 MCP 호출 시퀀스 |
| `docs/systems/farm-expansion-architecture.md` | CRITICAL-1~5 수정: CanToolClear switch 7종, prerequisiteZoneId 필드, zoneId 예시, 프리팹 목록 |
| `TODO.md` | ARC-025 DONE 처리, ARC-026 DONE 처리, FIX-056 신규 등록 |

---

## ARC-025 — 농장 확장 MCP 태스크 시퀀스

### 태스크 그룹 구성

| 그룹 | 제목 | MCP 호출 수 |
|------|------|------------|
| Z-1 | 준비 작업 (폴더, asmdef) | ~4회 |
| Z-2 | Enum/타입 정의 (ZoneState, ZoneType, ObstacleType, ClearResult, ZoneEvents) | ~6회 |
| Z-3 | ZoneData SO 스크립트 (ZoneData, ObstacleEntry, ObstacleInstance, ZoneRuntimeState) | ~4회 |
| Z-4 | FarmZoneManager + FarmGrid partial class 확장 | ~2회 |
| Z-5 | 구역 해금 흐름 연동 (ToolSystem, EconomyManager, ProgressionManager) | ~3회 |
| Z-6 | 장애물 프리팹 7종 + 머티리얼 + ObstacleContainer (Editor 스크립트 경유) | ~8회 |
| Z-7 | 세이브/로드 통합 (ZoneSaveData 3종, GameSaveData 확장, PATTERN-005 준수) | ~5회 |
| Z-8 | ZoneData SO 에셋 7개 (Zone A~G, canonical 참조 방식) | ~56회 |
| Z-9 | 씬 배치 + 통합 테스트 | ~12회 |
| **합계** | | **~99회** |

**병렬 실행 가능**: Z-4(FarmZoneManager 코어)와 Z-6(장애물 프리팹)은 독립 실행 가능.

### 핵심 설계 결정사항

**1. ZoneData SO에 prerequisiteZoneId 필드 추가 (CRITICAL-2 수정)**

ARC-023 원본 문서에서 `prerequisiteZoneId` 필드가 클래스 다이어그램과 C# 코드 예시에 모두 누락되어 있었다. farm-expansion-tasks.md에서 이 필드를 여러 곳에서 참조하고 있어 컴파일 에러가 예상됐다. 리뷰어가 ARC-023을 직접 수정하여 필드를 추가했다.

**2. ObstacleType enum 7종 전수 업데이트 (CRITICAL-1, WARNING-2)**

ARC-023의 `CanToolClear()` switch 문과 섹션 5.2 도구-장애물 매핑 표가 실제 enum값(`SmallRock, LargeRock, Stump, SmallTree, LargeTree, Weed, Bush`)이 아닌 존재하지 않는 값(`Rock, Tree, Boulder`)을 참조하고 있었다. 리뷰어가 7종 전체로 재작성했다.

**3. Zone ID 통일 (CRITICAL-3, CRITICAL-4)**

ARC-023 Part II 시퀀스 내에 `zone_initial`(존재하지 않는 ID)과 `zone_east_1`(존재하지 않는 ID)이 사용되고 있었다. DES-012에서 정의된 실제 ID(`zone_home`, `zone_south_plain`, `zone_east_forest`)로 전면 교체했다.

**4. create_material → Editor 스크립트 경유 (WARNING-1)**

`mcp__unity__create_material`은 MCP for Unity 공개 도구 목록에 없는 비표준 호출이다. Z-6 장애물 머티리얼 생성을 Editor 스크립트 + `execute_method` 방식으로 교체했다.

**5. SO 에셋 수치 placeholder 패턴 (WARNING-4 → FIX-056 등록)**

Z-8에서 ZoneData SO 에셋 7개의 `requiredLevel = 0`, `unlockCost = 0` 플레이스홀더를 사용하고 canonical 참조 주석으로 표기했다. 이 패턴은 실제 MCP 구현 시 실수로 0 값이 적용될 위험이 있으나, DES-012 섹션 3.1 장애물 HP 수치가 아직 완전히 확정되지 않은 상태이므로 이번 세션에서는 FIX-056으로 등록하고 추후 처리한다.

---

## 리뷰어 수정 사항

| ID | 심각도 | 파일 | 수정 내용 |
|----|--------|------|-----------|
| CRITICAL-1 | 🔴 | farm-expansion-architecture.md 섹션 5.3 | CanToolClear() switch 문을 ObstacleType 7종으로 재작성 |
| CRITICAL-2 | 🔴 | farm-expansion-architecture.md 섹션 1/3 | ZoneData에 prerequisiteZoneId 필드 추가 (다이어그램 + C# 동시) |
| CRITICAL-3 | 🔴 | farm-expansion-architecture.md Part II Phase C/E | zone_initial→zone_home, N/5→N/7, 테스트 ID 수정 |
| CRITICAL-4 | 🔴 | farm-expansion-architecture.md 섹션 9.1 JSON | zone_east_1/2 → zone_south_plain/zone_north_plain |
| CRITICAL-5 | 🔴 | farm-expansion-architecture.md Phase A-5 | 프리팹 3종 → 7종 전체 나열 |
| WARNING-1 | 🟡 | farm-expansion-tasks.md Z-6-01 | create_material → Editor 스크립트 방식으로 교체 |
| WARNING-2 | 🟡 | farm-expansion-architecture.md 섹션 5.2 | 도구-장애물 매핑 표 7종으로 재작성 |
| WARNING-3 | 🟡 | farm-expansion-architecture.md OPEN 항목 5 | zone_initial → zone_home 교체 |
| WARNING-4 | 🟡 | farm-expansion-tasks.md Z-8 | FIX-056 등록 (이번 세션 미수정) |

---

## 신규 FIX 태스크

| ID | Priority | 내용 |
|----|----------|------|
| FIX-056 | 3 | farm-expansion.md 섹션 3.1 장애물 HP 수치 canonical 확정 후, farm-expansion-architecture.md 섹션 5.2 매핑 표 + farm-expansion-tasks.md Z-8 SO 에셋 필드값 동기화 |

---

## 세션 후 TODO 상태

| ID | Priority | 상태 |
|----|----------|------|
| ARC-025 | 1 | ✅ DONE |
| ARC-026 | 3 | ✅ DONE (지난 세션 생성 완료 확인) |
| AUD-001 | 1 | 잔여 |
| BAL-005 | 1 | 잔여 |
| BAL-009 | 1 | 잔여 |
| CON-008 | 1 | 잔여 |
| FIX-044 | 1 | 잔여 |
| CON-009 | 2 | 잔여 |
| FIX-052 | 2 | 잔여 |
| FIX-054 | 2 | 잔여 |
| FIX-049~053 | 3 | 잔여 |
| FIX-056 | 3 | 신규 |
| PATTERN-009, 010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 ARC-025 태스크에 따라 자율적으로 작성했습니다.*
