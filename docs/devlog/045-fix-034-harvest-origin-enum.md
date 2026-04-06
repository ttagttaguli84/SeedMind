# Devlog #045 — FIX-034: HarvestOrigin enum 설계 완료

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

BAL-010에서 식별된 [RISK] — 온실 수확물 출처 추적 없이 비계절 x0.8 / 겨울 전용 x1.2 판매가 보정을 런타임 적용 불가 — 를 FIX-034로 해소했다. 방식 A(HarvestOrigin 태그를 ItemSlot에 추가)를 채택하고, 4개 파일에 걸쳐 변경 사항을 전파했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/systems/economy-architecture.md` | 섹션 3.10 신규 추가 (HarvestOrigin 설계 전체), 섹션 1.1/1.2 시그니처 업데이트, 섹션 3.7.3 [RISK]→[RESOLVED-FIX-034], 섹션 4.5 Transaction에 origin 필드, 섹션 5.1 판매 흐름 origin 반영, Cross-references 2개 추가 |
| `docs/systems/inventory-architecture.md` | ItemSlot 클래스 다이어그램 Origin 필드 추가, AddItem/RemoveItem 시그니처 origin 파라미터, GetItemCount 시그니처, CanStackWith 3중 매칭(+origin), SortBackpack 알고리즘 origin 정렬 추가, 섹션 6.1 세이브 매핑 origin 추가, 섹션 7.2 판매 흐름 origin 반영, Cross-references 업데이트 |
| `docs/pipeline/data-pipeline.md` | ItemSlotSaveData C#에 itemType/origin 필드 추가 (PATTERN-005 준수), InventorySaveData JSON 예시 origin 추가, ShippingBinSaveData JSON 예시 origin 추가 |
| `docs/systems/save-load-architecture.md` | 세이브 트리 ItemSlotSaveData 설명에 FIX-034 주석 추가 |

---

## 설계 결정

### HarvestOrigin 추적: 방식 A 채택

두 방식을 비교 분석했다:

| 방식 | 핵심 | 기각 이유 |
|------|------|-----------|
| **A (채택)**: ItemSlot에 `origin` 태그 부착 | 판매 시점에 출처 조회 → 보정 적용 | — |
| B: 수확 시점에 가격 확정 (`adjustedSellPrice`) | 수확 시 온실 보정 고정 저장 | 수급/날씨/축제 보정이 수확 시점에 고정 → "언제 팔지" 전략 무력화 |

**핵심 이유**: economy-system.md의 설계 목표 "판매 타이밍의 전략적 선택"과 방식 B가 충돌한다. 방식 A는 파급 범위가 넓지만 모든 변경이 기계적(필드 추가 + 조건 추가)이다.

### HarvestOrigin enum

```csharp
namespace SeedMind   // 최상위 — ItemType, CropQuality와 동일 레벨
{
    public enum HarvestOrigin { Outdoor = 0, Greenhouse = 1 }
}
```

3개 시스템(Economy, Player, Farming)에서 참조하므로 하위 네임스페이스 배치 시 순환 참조 위험.

### 스택 분리 정책

스택 키: `itemId + quality + origin` 3중 매칭. 야외산과 온실산은 별도 슬롯. 근거: 판매가 최대 20% 이상 차이나는 아이템을 한 스택에 넣으면 판매 예상가 예측 불가.

### 캐시 전략

`_cachedPrices`는 `itemId → origin 독립 기본가`를 캐시. 온실 보정(greenhouseMul)은 조회 시점에 사후 적용 → 캐시 키 구조 변경 불필요.

### GetGreenhouseMultiplier() 확정 로직

```
if origin == Outdoor           → 1.0
if origin == Greenhouse && 해당 계절 작물  → 1.0
if origin == Greenhouse && 겨울 전용 작물  → cropData.greenhouseSynergyBonus  (→ see crop-economy.md 4.3.10)
if origin == Greenhouse && 비계절 일반 작물 → economyConfig.greenhouseOffSeasonPenalty (→ see crop-economy.md 4.3.10)
```

---

## 리뷰어 수정 사항

| ID | 심각도 | 파일 | 수정 내용 |
|----|--------|------|-----------|
| WARNING-01 | 🟡 | economy-architecture.md 섹션 3.7.3 | 변경 사항 표에서 `bool isGreenhouse` → `HarvestOrigin origin` 확정 명시 |
| WARNING-02 | 🟡 | inventory-architecture.md 섹션 5.4 | SortBackpack 정렬·합산 조건에 origin 추가 |
| WARNING-03 | 🟡 | inventory-architecture.md 섹션 6.1 | InventorySaveData 매핑 트리에 origin 필드 추가 |
| WARNING-04 | 🟡 | inventory-architecture.md 클래스 다이어그램 | ItemSlot 박스에 Origin 필드 추가 |
| WARNING-05 | 🟡 | inventory-architecture.md 클래스 다이어그램 | GetItemCount 시그니처 3중 파라미터로 업데이트 |
| WARNING-06 | 🟡 | inventory-architecture.md 섹션 7.2 | 판매 흐름 TrySellCrop/GetSellPrice/RemoveItem에 origin 반영 |
| WARNING-07 | 🟡 | economy-architecture.md Cross-references | inventory-architecture.md, data-pipeline.md 참조 추가 |
| WARNING-08 | 🟡 | data-pipeline.md ItemSlotSaveData | C# 클래스에 itemType 필드 추가 (PATTERN-005 준수) |
| WARNING-09 | 🟡 | save-load-architecture.md | ItemSlotSaveData origin 필드 주석 추가 |
| INFO-01 | 🔵 | economy-architecture.md 섹션 5.3 | RecalculateAllPrices 호출에 캐시 전략 주석 추가 |
| INFO-02 | 🔵 | inventory-architecture.md Cross-references | economy-architecture.md 섹션 3.10 참조 명시 |

---

## 설계 관찰

### 파급 범위 정리

FIX-034 하나의 변경이 총 4개 문서, 11개 섹션에 걸쳐 전파됐다. 이는 ItemSlot이 인벤토리·경제·세이브 3개 시스템의 교차점에 있기 때문이다. 설계 자체는 단순(enum 1개 + 필드 1개)하나, 이 필드가 닿는 모든 API를 추적하는 것이 핵심 작업이었다.

### 미결 Open Questions

- [OPEN] 가공품의 origin 전파: 온실산 원재료로 만든 가공품에 온실 페널티 적용 여부 — 현재 범위 외, 별도 밸런스 검토 필요
- [OPEN] 야생 채집(forageable) 추가 시 `HarvestOrigin.Wild` 값 필요 여부 — 현재 scope 외

---

## 잔여 후속 작업 (우선순위 순)

| ID | Priority | 내용 |
|----|----------|------|
| DES-012 | 2 | 농장 확장 시스템 설계 (ARC-023 선행 요건) |
| ARC-023 | 2 | 농장 확장 기술 아키텍처 |
| PATTERN-009 | - | [self-improve 전용] 밸런스 히스토리 수치 혼재 규칙 |
| CON-006 | 1 | 목축/낙농 시스템 콘텐츠 |
| BAL-009 | 1 | 도구 업그레이드 XP 밸런스 분석 |

---

*이 문서는 Claude Code가 FIX-034 태스크에 따라 자율적으로 작성했습니다.*
