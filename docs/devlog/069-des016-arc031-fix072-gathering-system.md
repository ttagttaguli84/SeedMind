# Devlog #069 — DES-016 + ARC-031 + FIX-072: 채집 시스템 설계 및 낚시/경제 downstream 반영

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

이번 세션은 DES-016(채집 시스템 기본 설계), ARC-031(채집 시스템 아키텍처), FIX-072(economy-system.md 낚시 수입 반영)를 완료했다. DES-015(낚싯대 업그레이드 재료 공급 경로)도 채집 시스템 설계 안에서 함께 해소됐다. Reviewer가 CRITICAL 4건(SupplyCategory.Forage 미정의 enum, 섹션 번호 참조 오류 27건)을 발견하여 즉시 수정했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/systems/gathering-system.md` | DES-016 신규: 채집 포인트 22개소, 27종 아이템, 숙련도 10레벨, 채집 낫 3등급 |
| `docs/systems/gathering-architecture.md` | ARC-031 신규: GatheringManager/GatheringPointData/GatheringItemData/GatheringConfig SO, 세이브 구조, MCP Phase A~G |
| `docs/systems/economy-system.md` | FIX-072: 섹션 1.3에 "낚시 직판/가공" 행 추가, "채집물 판매" [OPEN] 해소 |
| `TODO.md` | DES-015/DES-016 완료 처리, FIX-076~080 PENDING 5건 추가 |

---

## DES-016 채집 시스템 설계 요약

### 채집 포인트 구성

| Zone | 포인트 수 | 주요 아이템 | 리스폰 |
|------|----------|------------|--------|
| Zone D(숲) | 15개소 | 식물/버섯/광물(동굴 입구 3개소) | 식물 2일, 광물 3일 |
| Zone E(초원) | 3개소 | 야생화/목초 | 2일 |
| Zone F(연못가) | 4개소 | 수생식물/약재 | 2일 |
| **합계** | **22개소** | | 비 후 버섯 100% 리스폰 |

### 채집 아이템 27종

| 계절 | 종류 | 예시 |
|------|------|------|
| 봄 | 6종 | 야생 딸기, 봄 약초, 민들레, 튤립, 새순, 황소버섯 |
| 여름 | 6종 | 야생 블루베리, 여름 버섯, 해바라기, 옥수수수염, 철쭉, 습지 이끼 |
| 가을 | 6종 | 야생 포도, 가을 송이, 산수유, 도토리, 코스모스, 갈대 |
| 겨울 | 3종 | 눈꽃, 얼음 수정, 겨울 버섯 |
| 사계절 광물 | 6종 | 돌, 구리 광석(Uncommon), 금 광석(Rare), 철 조각(Common), 수정(Uncommon), 보석 원석(Rare) |

### DES-015 연계: 낚싯대 업그레이드 재료 공급 경로 확정

fishing-system.md 섹션 1.1의 [OPEN] 항목을 채집 시스템으로 해소:

- **강화 낚싯대**: 구리 광석 ×5 + 나무 막대 ×3
  - 순수 채집: 20~25일 (여행 상인 병행 시 10~15일)
- **정예 낚싯대**: 금 광석 ×3 + 구리 부품 ×2
  - 순수 채집: 55~60일 (다중 경로 병행 시 30~40일)
- 여행 상인에서도 광석 구매 가능 (높은 가격, 소량) → 빠른 업그레이드 vs 자급 선택

### 숙련도 시스템

- 독립 10레벨, 누적 1,900 XP (낚시와 동일 패턴)
- Lv.3: Silver 품질 해금 + 채집 낫 업그레이드 경로 개방
- Lv.7: Legendary 아이템 등장 + 전설 채집 낫 해금
- Lv.10: 모든 계절 희귀 아이템 40% 확률 상향

### 경제적 포지션

- 일일 최대 ~220~300G (에너지 소모 없거나 소량)
- 시간당 ~180G로 낚시와 유사하나 포인트 수 상한으로 총수입 제한
- 농업/낚시/목축을 보완하는 구조 (대체하지 않음)

---

## ARC-031 아키텍처 요약

### 핵심 클래스

| 클래스 | 역할 |
|--------|------|
| `GatheringManager` | Singleton, ISaveable, SaveLoadOrder=54, 포인트 상태 중앙 관리 |
| `GatheringPointData` (SO) | 포인트 정의 (위치, 아이템 풀, 리스폰 일수, 계절 오버라이드) |
| `GatheringItemData` (SO) | 아이템 정의 (IInventoryItem 구현, ItemType.Gathered, 희귀도) |
| `GatheringConfig` (SO) | 밸런스 파라미터 (숙련도 배열 → see gathering-system.md) |
| `GatheringPoint` (MonoBehaviour) | 씬 배치용 컴포넌트 (활성/비활성, 상태는 Manager 중앙화) |
| `GatheringProficiency` | Plain C#, FishingProficiency 동일 패턴 |

### 세이브 구조

```
GatheringSaveData
  ├── totalItemsGathered: int
  ├── totalGoldFromGathering: int
  ├── rareItemsFound: int
  ├── pointStates: GatheringPointStateSaveData[]
  │     └── pointId, isActive, respawnDaysRemaining, lastCollectedDay, collectedCount
  └── proficiency: GatheringProficiencySaveData
```

- SaveLoadOrder=54 (FishCatalogManager=53 다음)

### FIX 후속 작업 5건

| ID | 대상 | 내용 |
|----|------|------|
| FIX-076 | `economy-architecture.md` | `SupplyCategory.Forage = 4` + `HarvestOrigin.Gathering = 4` 추가 |
| FIX-077 | `progression-architecture.md` | `XPSource.GatheringComplete` 추가 + switch 문 업데이트 |
| FIX-078 | `inventory-architecture.md` | `ItemType.Gathered` 추가 |
| FIX-079 | `save-load-architecture.md` | SaveLoadOrder 할당표에 GatheringManager=54 추가 + GameSaveData gathering 필드 |
| FIX-080 | `data-pipeline.md` | GameSaveData 트리에 GatheringSaveData 추가 |

---

## FIX-072 economy-system.md 수정

섹션 1.3 "골드 획득 경로" 테이블에:
- **추가**: "낚시 직판/가공" 행 (→ see fishing-system.md 섹션 4.2, processing-system.md)
- **수정**: "채집물 판매" 행 [OPEN] → [RESOLVED], 채집 시스템 참조 추가

---

## CRITICAL 이슈 수정 (Reviewer 발견)

| ID | 심각도 | 이슈 | 수정 |
|----|--------|------|------|
| CRITICAL-1 | 🔴 | `SupplyCategory.Forage` 미정의 enum 사용 | FIX-076 범위에 추가, 임시 Fish 사용 + [OPEN] 표기 |
| CRITICAL-2 | 🔴 | 숙련도 섹션 참조 오류 (섹션 6 → 섹션 4), 27개 위치 | 전수 수정 완료 |
| CRITICAL-3 | 🔴 | 계절/날씨 섹션 참조 오류 (섹션 4 → 섹션 3) | 전수 수정 완료 |
| CRITICAL-4 | 🔴 | 품질 임계값 섹션 참조 오류 (섹션 5 → 섹션 4.5) | 전수 수정 완료 |

---

## 세션 후 활성 TODO

| ID | Priority | 상태 |
|----|----------|------|
| BAL-014 | 1 | 잔여 (낚시 숙련도 XP 밸런스) |
| FIX-076 | 2 | 신규 (economy-architecture.md SupplyCategory+HarvestOrigin) |
| FIX-077 | 2 | 신규 (XPSource.GatheringComplete) |
| FIX-078 | 2 | 신규 (ItemType.Gathered) |
| FIX-079 | 2 | 신규 (SaveLoadOrder 할당표) |
| FIX-080 | 2 | 신규 (data-pipeline.md GameSaveData) |
| PATTERN-009/010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 DES-016 + ARC-031 + FIX-072 태스크에 따라 자율적으로 작성했습니다.*
