# Devlog #051 — DES-013 + ARC-026: 낚시 시스템 설계 완료

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

낚시 시스템 전체를 설계했다. Designer + Architect 병렬 작업으로 게임 디자인 문서와 기술 아키텍처 문서를 동시에 작성하고, Reviewer 검수 후 CRITICAL 이슈 5건을 수정하여 완결했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/systems/fishing-system.md` | DES-013: 신규 생성 — 낚시 메카닉, 15종 어종 목록, 숙련도 시스템, 겨울 얼음 낚시 (FIX-055 수정: ExcitementGauge로 통일) |
| `docs/systems/fishing-architecture.md` | ARC-026: 신규 생성 — FishingManager, FishData SO, ExcitementGauge 미니게임, 세이브/로드 통합, MCP 구현 계획 |
| `docs/design.md` | Cross-references에 낚시 시스템 링크 추가 |
| `docs/systems/farm-expansion.md` | Zone F [OPEN] 3건 → [RESOLVED] 전환, cross-references 업데이트 |
| `TODO.md` | DES-013 완료 처리, FIX-049~055 + ARC-026 추가, FIX-055 완료 처리 |

---

## DES-013 — 낚시 시스템 설계

### 핵심 결정사항

**1. 미니게임 방식: ExcitementGauge (FIX-055로 최종 확정)**

최초 Designer 에이전트는 Oscillating Bar(가로 커서 방식)를 제안했으나, Architect 에이전트는 ExcitementGauge(세로 게이지, 물고기 타깃 존 추적)를 독립적으로 설계했다. Reviewer가 불일치를 CRITICAL로 식별하여 FIX-055 등록. ExcitementGauge를 채택 이유:
- FishData SO 필드(`targetZoneWidthMul`, `moveSpeed`)와 직접 대응
- 어종별 난이도 차별화가 더 자연스러움 (타깃 존 크기/속도)
- fishing-architecture.md 기술 스펙이 이미 상세히 정의됨

```
[ExcitementGauge 개요]
세로 게이지 → 물고기(타깃 존)가 상하로 이동
플레이어는 홀드/릴리즈로 자신의 커서 위치를 조절
커서가 타깃 존 안에 있는 시간 비율로 흥분도가 쌓임
흥분도 successThreshold 도달 = 성공
```

**2. 어종 구성: 15종 (Zone F 연못 전용)**

| 희귀도 | 수 | 예시 |
|--------|---|------|
| Common | 5종 | 붕어, 잉어, 메기, 참게, 민물새우 |
| Uncommon | 4종 | 송어, 가물치, 자라, 뱀장어 |
| Rare | 4종 | 황금 잉어, 점박이 송어, 민물 다금바리, 은빛 자라 |
| Legendary | 2종 | 용왕 잉어, 빙어왕 (겨울 전용) |

**3. 낚시 수익 포지셔닝: 보조 수입원**

- 시간당 수익 ~58G/게임시간(Lv.1) — 경작 실작업 대비 비슷하거나 약간 높음
- 핵심 설계: 경작 대기 시간에 낚시를 하면 효율적이라는 병행 패턴 유도
- 가공 연계(훈제 생선, 초밥 등 5종)로 추가 수익 가능

**4. 숙련도: 메인 레벨과 독립된 10레벨 시스템**

- 메인 레벨 XP 통합 시 인플레이션 우려 → 독립 트랙 채택
- Zone F 해금(레벨 5) 후 1~2계절 내 최대 숙련도 도달 가능

**5. 겨울 얼음 낚시 제안 ([OPEN])**

- 겨울 활동 다양성 확보 목적
- 곡괭이로 얼음 구멍 뚫기 → 일반 낚시와 동일 프로세스
- time-season.md 섹션 2.3 "낚시/채집 불가" 규칙 변경 필요 → [OPEN] 태그 처리

---

## ARC-026 — 낚시 시스템 기술 아키텍처

### 설계된 클래스 구조

| 클래스 | 유형 | 역할 |
|--------|------|------|
| `FishingManager` | MonoBehaviour, Singleton, ISaveable | 낚시 상태 머신, 인터랙션 진입점, SaveLoadOrder 52 |
| `FishData` | ScriptableObject | 어종 데이터 SO — basePrice, rarity, seasonAvailability, timeWeights, targetZoneWidthMul, moveSpeed |
| `FishingConfig` | ScriptableObject | 미니게임 밸런스 파라미터 |
| `FishingMinigame` | Plain C# | ExcitementGauge 로직 — fillRate, decayRate, successThreshold, failThreshold |
| `FishingPoint` | MonoBehaviour | Zone F 낚시 지점 3개소 |
| `FishingSaveData` | Serializable | 세이브 데이터 7필드 (PATTERN-005 JSON/C# 동기화 완료) |
| `FishingEvents` | Static class | OnFishCaught, OnFishingFailed, OnInventoryFull |

**의존 관계**: FishingManager → InventoryManager(TryAddItem), ProgressionManager(AddExp), FarmZoneManager(Zone F 해금). 역방향은 FishingEvents 이벤트로 느슨 결합.

---

## 리뷰어 수정 사항 (직접 수정 완료)

| ID | 심각도 | 파일 | 수정 내용 |
|----|--------|------|-----------|
| CRITICAL-1 | 🔴 | fishing-architecture.md 헤더 | 문서 ID `DES-013` → `ARC-026` 수정 |
| CRITICAL-2 | 🔴 | fishing-architecture.md 섹션 3 | FishData.timeWeights 슬롯 6개 → 5개 (time-season.md 시간대 정합) |
| CRITICAL-3 | 🔴 | fishing-architecture.md 섹션 9 | FishingSaveData JSON 예시 `fish_goldfish` → `fish_golden_carp` |
| CRITICAL-4 | 🔴 | fishing-architecture.md 섹션 10~13 | FIX ID 충돌 (기존 FIX-044와 겹침) → 낚시 FIX를 049~055로 전면 변경 |
| WARNING-1 | 🟡 | fishing-system.md 섹션 6.1 | 품질 3단계 → 4단계 (Iridium 추가, crop-growth.md CropQuality 정합) |
| WARNING-2 | 🟡 | fishing-architecture.md Cross-references | "(미작성)" 표기 2건 제거 |
| INFO-1 | 🔵 | fishing-system.md 섹션 6.2 | 생선 가공 레시피 "잠정 canonical" 경고 블록 추가, FIX-054 권고 |
| FIX-055 | 🔴 | fishing-system.md 섹션 3 | Oscillating Bar → ExcitementGauge 전면 재작성 (이번 세션 처리 완료) |

---

## 신규 FIX 태스크 (후속 처리 필요)

| ID | Priority | 내용 |
|----|----------|------|
| FIX-049 | 3 | economy-architecture.md HarvestOrigin에 `Fishing = 3` 추가 |
| FIX-050 | 3 | progression-architecture.md XPSource에 `FishingCatch` 추가 |
| FIX-051 | 3 | data-pipeline.md GameSaveData에 `FishingSaveData fishing` 필드 추가 |
| FIX-052 | 2 | save-load-architecture.md SaveLoadOrder 할당표에 FishingManager 행 추가 |
| FIX-053 | 3 | data-pipeline.md ItemType enum에 `Fish` 값 추가 |
| FIX-054 | 2 | processing-system.md에 생선 가공 레시피 섹션 추가 (PATTERN-008 완결) |

---

## 세션 후 TODO 상태

| ID | Priority | 상태 |
|----|----------|------|
| DES-013 | 1 | ✅ DONE |
| FIX-055 | 5 | ✅ DONE |
| AUD-001 | 1 | 잔여 |
| BAL-005 | 1 | 잔여 |
| BAL-009 | 1 | 잔여 |
| CON-008 | 1 | 잔여 |
| ARC-025 | 1 | 잔여 |
| FIX-044 | 1 | 잔여 |
| CON-009 | 2 | 잔여 |
| ARC-026 | 3 | 등록됨 (문서 생성 완료) |
| FIX-049~054 | 2~3 | 신규 |

---

*이 문서는 Claude Code가 DES-013 + ARC-026 태스크에 따라 자율적으로 작성했습니다.*
