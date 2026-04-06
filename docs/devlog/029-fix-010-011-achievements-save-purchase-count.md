# Devlog #029 — FIX-010: 업적 세이브 필드 추가 + FIX-011: PurchaseCount conditionType 확정

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### FIX-010: GameSaveData에 achievements 필드 추가 (PATTERN-005)

`docs/systems/save-load-architecture.md` 수정.

ARC-017 리뷰 후속으로, GameSaveData 루트에 `AchievementSaveData` 필드가 누락된 PATTERN-005 위반을 해소했다.

**변경 내용**:
1. **트리 다이어그램 (섹션 2.1)**: `└── TutorialSaveData` → `├── TutorialSaveData`로 변경, `└── AchievementSaveData` 추가
2. **JSON 스키마 (섹션 2.2)**: `"achievements": { "records": [], "totalUnlocked": 0 }` 추가 (achievement-architecture.md AchievementSaveData 2필드와 일치)
3. **C# 클래스 (섹션 2.3)**: `using SeedMind.Achievement;` 추가, `public AchievementSaveData achievements;` 추가
4. **PATTERN-005 검증 주석**: 시스템 데이터 13 → 14개, 총 16 → 17개 필드로 업데이트
5. **SaveLoadOrder 할당표 (섹션 7)**: `AchievementManager | 90` 추가
6. **복원 순서 다이어그램**: `[85] QuestManager`와 `[90] AchievementManager` 추가
7. **Cross-references**: `docs/systems/achievement-architecture.md` 링크 추가

---

### FIX-011: PurchaseCount 전용 conditionType 추가

`docs/systems/achievement-architecture.md`, `docs/content/achievements.md` 수정.

ach_explorer_02 (바람이의 단골)과 ach_explorer_04 (쇼핑 마니아)의 conditionType이 `GoldSpent (7)`로 매핑되어 있었으나, 실제로 추적해야 하는 것은 "구매 횟수"다. Semantic 불일치를 해소하기 위해 전용 enum 값을 추가했다.

**설계 결정**: `PurchaseCount = 14` 추가 (기존 ProcessingCount = 13 다음 연번)

**변경 내용**:
- `achievement-architecture.md` AchievementConditionType에 `PurchaseCount = 14` 추가 (설명: 상점 구매 횟수, targetId=""이면 전체, 특정 상점 ID이면 필터)
- 이벤트 구독 매핑에 `EconomyEvents.OnShopPurchased → PurchaseCount → HandleShopPurchase` 추가
- 파일 목록의 enum 값 수 14 → 16으로 수정 (PurchaseCount 추가 후 Custom=99 포함 총 16개)
- `achievements.md` ach_explorer_02 conditionType `GoldSpent (7)` → `PurchaseCount (14)` 수정
- `achievements.md` ach_explorer_04 conditionType `GoldSpent (7)` → `PurchaseCount (14)` 수정
- `achievements.md` [OPEN] 항목 4번 RESOLVED로 마킹

---

## 리뷰 결과 (수정 전 → 수정 후)

**수정 전 리뷰 발견 이슈 (총 9건)**:

| 심각도 | 건수 | 내용 |
|--------|------|------|
| CRITICAL | 2 | JSON achievements 필드 누락(totalUnlocked), SaveLoadOrder 할당표 누락 |
| WARNING | 4 | 칭호 ID 불일치(title_farming_silver), Cross-references 누락, enum 값 수 오류(15개→16개), [OPEN] 미갱신 2건 |
| INFO | 3 | [RISK] 중복, [OPEN] 미갱신 2건 |

**모두 수정 완료.**

---

## 의사결정 기록

1. **PurchaseCount vs Custom**: Explorer 업적의 "구매 횟수" 추적에 `Custom = 99` 대신 `PurchaseCount = 14`를 신규 추가하기로 결정. Custom은 복합 조건이 불가피한 숨겨진 업적 전용으로 유지한다. PurchaseCount는 targetId 필터로 "특정 상점 구매"와 "전체 구매"를 모두 처리할 수 있어 하나의 enum 값으로 충분하다.

2. **SaveLoadOrder 연번 확정**: TutorialManager(80) → QuestManager(85) → AchievementManager(90). 기존 save-load-architecture.md의 할당표에 QuestManager(85)가 기재되어 있었으나 복원 순서 다이어그램에는 TutorialManager(80)이 마지막으로 기재되어 있었다. 두 섹션을 동기화하며 AchievementManager(90)를 명시적으로 추가했다.

---

## 미결 사항 ([OPEN])

- **BAL-006**: 퀘스트 보상 밸런스 분석 (다음 우선 작업 후보)
- **ARC-012**: 세이브/로드 MCP 태스크 시퀀스
- **ARC-010**: 튜토리얼 MCP 태스크 시퀀스
- **[RISK] 업적 XP 총량**: 2,250 XP가 전체 XP의 ~49%로 목표 범위 초과 — 플레이테스트 후 재조정 예정

---

## 후속 작업

- `BAL-006`: 퀘스트/미션 보상 밸런스 분석 → `docs/balance/quest-rewards.md`
- `ARC-012`: 세이브/로드 MCP 태스크 시퀀스
- `ARC-010`: 튜토리얼 MCP 태스크 시퀀스

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
