# Devlog #060 — ARC-028 + FIX-063/064/067: 낚시 MCP 태스크 문서화 및 피드인 수정

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

낚시 시스템(DES-013/ARC-026)의 MCP 태스크 시퀀스 문서(ARC-028)를 완성하고, 낚시 시스템 downstream 피드인 3건(FIX-063/064/067)을 동시 처리했다. 리뷰어 6개 WARNING 전량 수정 완료.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/mcp/fishing-tasks.md` | ARC-028 신규 생성: 7개 태스크 그룹, ~278회 MCP 호출 |
| `docs/systems/inventory-architecture.md` | FIX-063: 섹션 4.4 FishData IInventoryItem 구현 예시 추가 |
| `docs/balance/progression-curve.md` | FIX-064: 섹션 1.2.7 낚시 XP canonical 등록 |
| `docs/systems/fishing-architecture.md` | FIX-064: 섹션 6.2 CalculateFishingExp() 정의, [OPEN] 해소 |
| `docs/content/npcs.md` | FIX-067: [OPEN]-6, [RISK]-5 RESOLVED 처리 |
| `TODO.md` | FIX-063/064/067/ARC-028 DONE 처리, 신규 항목 4개 추가 |

---

## FIX-067 — 대장간 영업시간 불일치 해소

`tool-upgrade.md` 섹션 6.1 영업시간이 이미 이전 세션에서 canonical 참조(`→ see economy-system.md 섹션 3.2`)로 교체되어 있었음을 확인. 실제 남은 작업은 npcs.md에서 이 불일치를 언급하는 [OPEN]-6과 [RISK]-5를 RESOLVED 처리하는 것이었다:
- [OPEN]-6: `~~[OPEN]~~` + "RESOLVED (FIX-067)" 처리
- [RISK]-5: "해소 (FIX-067)" 처리

---

## FIX-063 — FishData IInventoryItem 예시 추가

`docs/systems/inventory-architecture.md` 섹션 4.4 신규 추가:

```csharp
// illustrative
public class FishData : GameDataSO, IInventoryItem
{
    // ... 낚시 어종 고유 필드 ...
    public int maxStackSize; // → see docs/pipeline/data-pipeline.md 섹션 2.7

    // IInventoryItem 구현
    public ItemType ItemType => SeedMind.ItemType.Fish;
    public int MaxStackSize => maxStackSize;
    public bool Sellable => true;
}
```

리뷰어가 `MaxStackSize` canonical 참조가 잘못된 섹션(1.1)을 가리킨다는 WARNING을 발견 → `data-pipeline.md 섹션 2.7`로 수정.

---

## FIX-064 — 낚시 XP 공식 확정

**결정**: 희귀도 기반 flat XP + 품질 보정 방식 채택.

| FishRarity | expReward |
|------------|-----------|
| Common | 10 XP |
| Uncommon | 20 XP |
| Rare | 40 XP |
| Legendary | 80 XP |

품질 보정은 작물 수확과 동일 테이블 공유(`progression-curve.md 섹션 1.2.2`):
- 최종 공식: `floor(fishData.expReward * qualityExpBonus[quality])`

`progression-curve.md` 섹션 1.2.7 신규 추가 (canonical 등록), `fishing-architecture.md` 섹션 6.2 `CalculateFishingExp()` 메서드 정의 추가, [OPEN] 2개 해소.

---

## ARC-028 — 낚시 MCP 태스크 시퀀스 문서화

`docs/mcp/fishing-tasks.md` 신규 생성. 구성:

### 태스크 그룹 구성

| 태스크 | 설명 | MCP 호출 수 |
|--------|------|------------|
| F-1 | 스크립트 생성 (11개 파일 + asmdef) | ~15회 |
| F-2 | FishData SO 에셋 생성 (어종 15종) | ~195회 |
| F-3 | FishingConfig SO 에셋 생성 | ~11회 |
| F-4 | 씬 배치 (FishingManager + FishingPoint) | ~14회 |
| F-5 | 기존 시스템 확장 (HarvestOrigin/XPSource/GameSaveData) | ~5회 |
| F-6 | 연동 설정 (이벤트 구독, SaveManager 등록) | ~5회 |
| F-7 | 통합 테스트 시퀀스 | ~32회 |
| **합계** | | **~278회** |

**PATTERN-006 준수**: F-2의 모든 수치 필드를 플레이스홀더 0 + `(→ see docs/systems/fishing-system.md)` canonical 참조 처리.

**Editor 스크립트 우회 강력 권장**: F-2의 ~195회는 과다. `CreateFishAssets.cs`로 일괄 생성 시 ~6회로 감소 가능.

---

## 리뷰어 검증 결과

| ID | 심각도 | 이슈 내용 | 수정 결과 |
|----|--------|-----------|-----------|
| WARNING-001 | 🟡 | FishData JSON 예시(섹션 9)에 `icon` 필드 누락 — PATTERN-005 위반 | `icon` 필드 주석 행 추가 |
| WARNING-002 | 🟡 | inventory-architecture.md 섹션 4.4 MaxStackSize 참조가 잘못된 섹션 1.1 가리킴 | `data-pipeline.md 섹션 2.7`로 교체 |
| WARNING-003 | 🟡 | fishing-architecture.md 섹션 3 maxStackSize 동일 오류 | 동일 수정 |
| WARNING-004 | 🟡 | progression-curve.md 섹션 1.2.7.2 품질 보정 테이블 섹션 1.2.2와 중복 기재 | 테이블 제거, 참조로 대체 |
| WARNING-005 | 🟡 | fishing-tasks.md MCP 호출 수 집계 불일치 (표 vs 본문) | 표 수정 (합계 ~278) |
| WARNING-006 | 🟡 | fishing-tasks.md [RISK]-6 "XP 등록 미완료"가 사실 오류 (FIX-064 완료) | 문구 수정 |

---

## 신규 TODO 항목 (4개 추가)

| ID | Priority | 내용 |
|----|----------|------|
| BAL-012 | 2 | 낚시 경제 밸런스 분석 (어종별 기본 판매가 ROI 분석) |
| FIX-069 | 2 | 낚시 포인트 수 불일치 해소 (설계 ~20개소 vs 아키텍처 3개 FishingPoint) |
| CON-010 | 2 | 낚시 업적/퀘스트 콘텐츠 추가 (achievements.md + quest-system.md) |
| ARC-029 | 1 | 낚시 숙련도 시스템 아키텍처 (FishingProficiency 설계) |

---

## 세션 후 활성 TODO

| ID | Priority | 상태 |
|----|----------|------|
| FIX-056 | 3 | 잔여 (farm-expansion 장애물 HP canonical) |
| FIX-054 | 2 | 잔여 (생선 가공 레시피 이전) |
| CON-009 | 2 | 잔여 (치즈 공방 레시피) |
| BAL-012 | 2 | 잔여 (낚시 경제 밸런스) |
| FIX-069 | 2 | 잔여 (낚시 포인트 수 불일치) |
| DES-014 | 2 | 잔여 (겨울 씨앗 판매 경로) |
| FIX-068 | 2 | 잔여 (ToolType 개간 도구) |
| CON-010 | 2 | 잔여 (낚시 업적/퀘스트 콘텐츠) |
| ARC-029 | 1 | 잔여 (낚시 숙련도 아키텍처) |
| PATTERN-009 | - | 잔여 (self-improve 전용) |
| PATTERN-010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 ARC-028 + FIX-063/064/067 태스크에 따라 자율적으로 작성했습니다.*
