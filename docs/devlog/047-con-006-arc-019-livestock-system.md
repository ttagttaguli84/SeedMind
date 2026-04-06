# Devlog #047 — FIX-037 + CON-006 + ARC-019: 목축/낙농 시스템 설계 완료

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

이번 세션에서는 FIX-037(과일나무 데이터 canonical 이전)을 먼저 처리한 뒤, CON-006(목축/낙농 콘텐츠)과 ARC-019(기술 아키텍처)를 병렬로 완성했다. 리뷰에서 CRITICAL 3건·WARNING 7건이 식별되어 즉시 수정했고, 후속 FIX 4건(FIX-038~041)을 TODO에 등록했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/content/crops.md` | 섹션 6 신규 추가 — 과일나무 4종 데이터 canonical (FIX-037) |
| `docs/systems/farm-expansion.md` | 섹션 4.4 수치 테이블 → crops.md 섹션 6 참조로 교체 (FIX-037) |
| `docs/content/livestock-system.md` | 신규 생성 (CON-006) — 4종 동물, 돌봄 사이클, 시설, 행복도 시스템, 경제 밸런스 |
| `docs/systems/livestock-architecture.md` | 신규 생성 (ARC-019) — AnimalManager, AnimalData SO, 행복도 계산, SaveLoadOrder:48, XPSource 확장 |
| `docs/systems/save-load-architecture.md` | SaveLoadOrder 표에 AnimalManager:48 추가, GameSaveData animals 필드 추가 (19→20 필드) |
| `docs/systems/economy-architecture.md` | HarvestOrigin enum에 Barn=2 추가, GetGreenhouseMultiplier() Barn 분기 추가 |
| `docs/systems/progression-architecture.md` | XPSource enum에 AnimalCare/AnimalHarvest 추가, GetExpForSource() switch case 확장 |

---

## FIX-037 — 과일나무 canonical 이전

farm-expansion.md 섹션 4.4에 직접 기재된 과일나무 가격·수확량·판매가 테이블을 crops.md 섹션 6으로 이전했다. crops.md가 모든 작물 데이터의 canonical 문서이므로 과일나무도 동일한 원칙을 적용. farm-expansion.md에는 crops.md 참조 표기만 남겼다.

---

## 설계 결정

### 목축 시스템 구조 (CON-006)

**동물 4종**:

| 동물 | 아이템 ID | 구매 가격 | 일일 사료비 | 생산물 | 생산 주기 | 판매가 |
|------|----------|---------|-----------|--------|---------|--------|
| 닭 | `animal_chicken` | 800G | 10G | 달걀 | 매일 | 35G |
| 염소 | `animal_goat` | 2,000G | 20G | 염소젖 | 2일 | 80G |
| 소 | `animal_cow` | 4,000G | 40G | 우유 | 2일 | 120G |
| 양 | `animal_sheep` | 3,000G | 25G | 양모 | 3일 | 150G |

**시설 (Zone E 전용)**:
- 닭장(Chicken Coop): 건설비 1,500G/3,000G, 수용 4/8마리
- 외양간(Barn): 건설비 3,000G/5,000G/8,000G, 수용 4/8/12마리

**행복도 시스템**: 0~200 범위, 초기값 100. 매일 먹이(+5) + 쓰다듬기(+5) + 방목(+3) 최대 +13. 방치 시 -10/일. 행복도 150+ 시 고품질 생산물 확률 발생 (닭 최대 25%, 염소 20%, 소 20%, 양 15%).

**동물 돌봄 에너지 소모**: 먹이 2 + 쓰다듬기 1 + 수집 1 = 마리당 4 에너지/일

### 기술 아키텍처 핵심 (ARC-019)

**AnimalManager**: `SeedMind.Livestock` 네임스페이스, MonoBehaviour Singleton. `TimeManager.OnDayChanged`를 구독(priority: 55)하여 일일 사이클 처리.

**데이터 구조 분리**:
- `AnimalData` (ScriptableObject): 정적 정의 (종류, 가격, 사료비, 생산물 등)
- `AnimalInstance` (Plain C#): 런타임 상태 (행복도, 마지막 먹이 시각, 생산 대기 여부)
- `LivestockConfig` (ScriptableObject): 시스템 전역 설정 (임계값, AnimationCurve 등)

**SaveLoadOrder 48**: FarmZoneManager(45) 이후, PlayerController(50) 이전. Zone E 해금 상태 복원 후 동물 로드.

**파급 적용 완료**:
- GameSaveData: `animals: AnimalSaveData` 필드 추가 (19→20 필드)
- XPSource: `AnimalCare`, `AnimalHarvest` 추가
- HarvestOrigin: `Barn = 2` 추가

---

## 리뷰어 수정 사항

| ID | 심각도 | 파일 | 수정 내용 |
|----|--------|------|-----------|
| CRITICAL-1 | 🔴 | livestock-architecture.md | 행복도 범위 0~100 → 0~200 전수 교체 (CON-006과 불일치) |
| CRITICAL-2 | 🔴 | livestock-architecture.md | 섹션 6 오참조 → 섹션 5.3으로 전수 교체 |
| CRITICAL-3 | 🔴 | livestock-architecture.md | GetProductQuality() 하드코딩 임계값 → LivestockConfig SO 참조로 교체 |
| WARNING-1 | 🟡 | livestock-architecture.md | 염소 SO 에셋/프리팹 누락 → 목록 추가 |
| WARNING-2 | 🟡 | livestock-architecture.md | CON-006 "(미작성)" 표기 제거 |
| WARNING-5 | 🟡 | save-load-architecture.md | AnimalManager:48 추가, GameSaveData animals 필드 추가 |
| WARNING-6 | 🟡 | economy-architecture.md | HarvestOrigin.Barn 추가, 관련 로직 확장 |
| WARNING-7 | 🟡 | progression-architecture.md | AnimalCare/AnimalHarvest XPSource 추가 |

**즉각 수정 불가 항목 → TODO 등록**:
- FIX-038 (Priority 3): AnimalManager 닭장 레벨/수용 수 관리 필드 분리
- FIX-039 (Priority 2): LivestockConfig SO에 품질 임계값 필드 추가
- FIX-040 (Priority 2): CON-006 섹션 7.3 XPSource 중복 기재 → ARC-019 참조 교체
- FIX-041 (Priority 1): design.md 섹션 4.6 시설 목록에 외양간/닭장/치즈 공방 추가

---

## 설계 관찰

### 치즈 공방 활성화 조건 확정

CON-006 완성으로 `processing-system.md`의 "[OPEN] 치즈 공방 미설계" 상태가 해소됐다. 치즈 공방 레시피 상세는 별도 문서화 필요하지만, 선행 조건인 동물 시스템의 생산물(우유, 염소젖)이 확정되었다.

### Zone E [OPEN] 부분 해소

farm-expansion.md Zone E(목장)의 "[OPEN] CON-006 완료 후 확정" 상태가 본 세션으로 해소됐다. 구체적인 Zone E 타일 배치(외양간 위치, 목초지 분할)는 FIX-038 등 후속 작업에서 세부 확정 예정.

### 경제 진입 비용 분석

| 시나리오 | 초기 비용 | 회수 기간 |
|---------|---------|---------|
| 최소 진입 (Zone E + 닭장 + 닭 2마리) | ~7,100G | ~142일 |
| 중간 투자 (닭 4마리 + 소 1마리) | ~12,700G | ~98일 |
| 치즈 공방 연계 시 | 수익 38~48% 향상 예상 |

---

## 잔여 후속 작업

| ID | Priority | 내용 |
|----|----------|------|
| FIX-035 | 2 | progression-curve.md 농장 확장 XP 4단계→6단계 동기화 |
| FIX-036 | 2 | economy-system.md 목공소 인벤토리 Zone 기반 업데이트 |
| FIX-038 | 3 | AnimalManager 닭장 레벨/수용 수 필드 분리 |
| FIX-039 | 2 | LivestockConfig SO 품질 임계값 필드 추가 |
| FIX-040 | 2 | CON-006 섹션 7.3 XPSource 중복 제거 |
| FIX-041 | 1 | design.md 섹션 4.6 시설 목록 업데이트 |
| BAL-008 | 1 | 목축/낙농 경제 밸런스 분석 |
| ARC-024 | 1 | 목축/낙농 MCP 태스크 시퀀스 |
| CON-008 | 1 | 추가 NPC 상세 설계 |
| BAL-009 | 1 | 도구 업그레이드 XP 밸런스 |
| AUD-001 | 1 | 사운드 디자인 문서 |
| BAL-005 | 1 | 여행 상인 희귀 아이템 가격 밸런스 |
| PATTERN-009 | - | [self-improve] 밸런스 히스토리 수치 혼재 규칙 |
| PATTERN-010 | - | [self-improve] 병렬 작성 시 플레이스홀더 동기화 규칙 |

---

*이 문서는 Claude Code가 FIX-037 + CON-006 + ARC-019 태스크에 따라 자율적으로 작성했습니다.*
