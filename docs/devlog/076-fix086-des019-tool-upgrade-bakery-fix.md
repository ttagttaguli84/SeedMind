# Devlog #076 — FIX-086 + DES-019: 도구 업그레이드 스키마 확장 및 베이커리 레시피 가격 조정

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

DES-017(채집 낫 업그레이드)에서 식별된 두 설계 이슈를 완결했다. FIX-086으로 도구 업그레이드 스키마를 채집 숙련도 조건을 지원하도록 확장했고, DES-019로 연료비로 인해 손해였던 베이커리 채집물 레시피 가격을 조정했다.

### 수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/systems/tool-upgrade-architecture.md` | FIX-086: LevelReqType enum 신설, UpgradeCostInfo 확장, CanUpgrade 분기 |
| `docs/systems/tool-upgrade.md` | FIX-086: 섹션 2.4 채집 낫 업그레이드 조건 신설 |
| `docs/content/processing-system.md` | DES-019: 봄나물 비빔밥 30→60G, 송이 구이 55→70G (canonical) |
| `docs/balance/processing-economy.md` | DES-019: 섹션 2.11 ROI 갱신, [RISK] 해소 |
| `docs/balance/gathering-economy.md` | DES-019: 섹션 4.2/4.3.3/4.3.4 갱신 |
| `docs/content/gathering-items.md` | DES-019: 가공 연계 가격 갱신 + 섹션 9.3 갱신 |
| `docs/systems/gathering-system.md` | FIX-086: 섹션 5.6.2 [OPEN]→해결됨, DES-019: 섹션 6.4 갱신 |

---

## FIX-086: LevelReqType 스키마 확장

### 문제

기존 `UpgradeCostInfo.requiredLevel`은 플레이어 메인 레벨만 지원. 채집 낫은 채집 숙련도 조건을 사용해야 하므로 타입 구분이 필요했다.

### 해결: 방안 A 채택 (범용 enum)

```csharp
public enum LevelReqType {
    PlayerLevel     = 0,  // 기본: 기존 농업 도구 (하위 호환)
    GatheringMastery = 1,  // 채집 낫
    FishingMastery  = 2,  // 예약 (향후 낚싯대)
}

// UpgradeCostInfo에 추가
public LevelReqType levelReqType;  // 기본값 PlayerLevel
public int requiredLevel;
```

- 기존 농업 도구(hoe/watering_can/sickle): `levelReqType = PlayerLevel` (기본값 유지)
- 채집 낫: `levelReqType = GatheringMastery`
- CanUpgrade() 분기: `PlayerLevel` → 기존 플레이어 레벨 확인, `GatheringMastery` → `GatheringManager.GetProficiencyLevel()` 호출

---

## DES-019: 베이커리 채집물 레시피 가격 조정

### 조정 전후 비교

| 레시피 | 구 가격 | 신 가격 | 연료 차감 후 직판 대비 |
|--------|:---:|:---:|:---:|
| 봄나물 비빔밥 | 30G | **60G** | +12G (+67%) |
| 송이 구이 | 55G | **70G** | +8G (+25%) |

### 결정 근거

- 조정안 (b) + (a) 혼합: 가격 상향 + NPC 선물/퀘스트 납품 포지셔닝
- 봄나물 비빔밥 60G: 연료 30G, 재료 18G 기준 → +12G (+67%) 순이익
- 송이 구이 70G: 연료 30G, 재료 32G 기준 → +8G (+25%) 순이익
- "채집 → 가공 → 경제 수익 + NPC 선물 가치"의 이중 동기 확보

---

*이 문서는 Claude Code가 FIX-086 + DES-019 작업에 따라 자율적으로 작성했습니다.*
