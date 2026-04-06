# Devlog #048 — FIX-038/039/040/041: 목축 아키텍처 후속 수정 완료

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

지난 세션(#047)에서 CON-006 + ARC-019 목축/낙농 시스템 설계 직후 리뷰어가 식별한 4건의 후속 수정(FIX-038~041)을 처리했다. 모두 livestock-architecture.md 정합성 개선 작업이다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/systems/livestock-architecture.md` | FIX-038: 닭장 필드 분리, FIX-039: 품질 임계값 SO 필드 추가 |
| `docs/content/livestock-system.md` | FIX-040: 섹션 7.3 XPSource enum 중복 제거, 섹션 5.3 품질 임계값 canonical 정의 추가, 섹션 10 RISK 해소 처리 |
| `docs/design.md` | FIX-041: 섹션 4.6 닭장/외양간/치즈 공방 시설 목록 추가 |

---

## FIX-038 — AnimalManager 닭장/외양간 분리

**문제**: AnimalManager 클래스 다이어그램에 `_barnLevel`/`_maxCapacity` 필드만 존재하여, 닭(소형/가금류)과 중대형 동물이 같은 수용 카운터를 공유하는 구조 오류.

**수정 내용**:

- `_maxCapacity` → `_barnCapacity`로 필드명 전수 교체
- `_coopLevel: int`, `_coopCapacity: int` 상태 필드 신규 추가
- `CoopLevel`, `CoopCapacity`, `BarnCapacity` 읽기 전용 프로퍼티 추가
- `OnCoopUpgraded: Action<int>` 이벤트 추가
- `UpgradeCoop()`, `HandleCoopBuilt()` 메서드 추가
- `TryBuyAnimal()`: 단일 수용 체크 → 동물 타입 분기 (Poultry → `_coopCapacity`, 나머지 → `_barnCapacity`)
- Zone E 연동 흐름(섹션 6): 닭장 건설 핸들러 분기 추가
- `AnimalSaveData`: `coopLevel` 필드 추가
- 저장/복원 흐름: `_coopLevel`/`_coopCapacity` 복원 로직 추가
- `LivestockConfig` SO: `initialCoopCapacity`, `coopUpgradeCapacity[]`, `coopUpgradeCost[]` 필드 추가

**설계 결정**: 닭장과 외양간은 완전 분리된 수용 카운터를 갖는다. 닭은 닭장만, 중/대형 동물은 외양간만 수용한다. `AnimalType.Poultry` 분기로 런타임에서 적절한 시설 용량을 확인한다.

---

## FIX-039 — LivestockConfig 품질 임계값 필드 추가

**문제**: `GetProductQuality()`에서 `LivestockConfig.Instance.goldQualityThreshold`/`silverQualityThreshold`를 참조하는데, SO 클래스 정의에 해당 필드가 없었다.

**수정 내용**: `LivestockConfig` SO에 `[Header("생산물 품질 임계값")]` 블록 추가:
- `goldQualityThreshold: float` — Gold 품질 최소 행복도 (→ see livestock-system.md 섹션 5.3)
- `silverQualityThreshold: float` — Silver 품질 최소 행복도 (→ see livestock-system.md 섹션 5.3)

**리뷰 후속 수정**: livestock-system.md 섹션 5.3에 `silverQualityThreshold = 150`, `goldQualityThreshold = 175` canonical 테이블이 명시되지 않아 리뷰어가 추가함.

---

## FIX-040 — CON-006 XPSource 중복 제거

**문제**: livestock-system.md 섹션 7.3에 XPSource enum 전체가 기재되어 livestock-architecture.md 섹션 7.1과 중복 (PATTERN-001 위반).

**수정 내용**: 섹션 7.3의 enum 코드 블록 전체를 제거하고 `→ see livestock-architecture.md 섹션 7.1~7.3`으로 교체. XP 수치 제안 테이블과 RISK 태그는 유지.

---

## FIX-041 — design.md 시설 목록 업데이트

**문제**: design.md 섹션 4.6 시설 테이블에 외양간/닭장/치즈 공방이 누락됨.

**수정 내용**: 시설 테이블에 3개 항목 추가 (canonical 참조 방식으로 비용 표기):
- 닭장 (Chicken Coop) — Zone E 전용, 레벨 1~2
- 외양간 (Barn) — Zone E 전용, 레벨 1~3
- 치즈 공방 (Cheese Workshop) — 외양간 Lv.1 선행 필요

---

## 리뷰어 수정 사항

| ID | 심각도 | 파일 | 수정 내용 |
|----|--------|------|-----------|
| WARNING-1 | 🟡 | livestock-system.md | 섹션 5.3에 품질 임계값(silver=150, gold=175) canonical 정의 테이블 추가 |
| WARNING-2 | 🟡 | livestock-system.md | 섹션 10 RISK-2 "[RISK]" → "[RESOLVED] FIX-041" 처리 |
| WARNING-3 | 🟡 | livestock-system.md | Cross-references 갱신 ("외양간/닭장 추가 필요" → "추가 완료 (FIX-041)") |
| WARNING-4 | 🟡 | design.md | 섹션 4.6 치즈 공방 참조 섹션 번호 7.2 → 7.1 수정 |

---

## INFO (미처리 항목)

**I-1**: livestock-system.md 섹션 7.3 XP 수치 제안 테이블 — canonical 출처(progression-curve.md)에 AnimalCare/AnimalHarvest XP 항목 미등록. 현재는 "제안값"으로 유일 출처 역할. progression-curve.md에 canonical 추가 필요.

→ TODO BAL-009 연계: 목축 XP를 progression-curve.md에 포함하는 것이 적절한 처리 방향.

---

## 세션 후 TODO 상태

| ID | Priority | 상태 |
|----|----------|------|
| FIX-038 | 3 | ✅ DONE |
| FIX-039 | 2 | ✅ DONE |
| FIX-040 | 2 | ✅ DONE |
| FIX-041 | 1 | ✅ DONE |
| FIX-035 | 2 | 잔여 |
| FIX-036 | 2 | 잔여 |
| BAL-008 | 1 | 잔여 |
| ARC-024 | 1 | 잔여 |
| BAL-009 | 1 | 잔여 (목축 XP canonical 포함 필요) |

---

*이 문서는 Claude Code가 FIX-038~041 태스크에 따라 자율적으로 작성했습니다.*
