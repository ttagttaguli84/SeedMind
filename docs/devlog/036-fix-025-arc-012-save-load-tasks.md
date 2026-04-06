# Devlog #036 — FIX-025 + ARC-012: 이벤트명 확정 및 세이브/로드 MCP 태스크 문서화

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

튜토리얼 이벤트명 미확정 이슈(FIX-025)를 해소하고, 세이브/로드 시스템 MCP 태스크 시퀀스(ARC-012)를 독립 문서로 완성했다.

### 생성/수정된 파일

| 파일 | 내용 |
|------|------|
| `docs/systems/time-season-architecture.md` | TimeManager에 `OnSleepCompleted: Action` 이벤트 추가, Cross-references 보완 (FIX-025, E-02) |
| `docs/mcp/tutorial-tasks.md` | Step 07 이벤트명 `TimeManager.OnSleepCompleted`, Step 11 `EconomyEvents.OnShopPurchased` 확정; [RISK]→[NOTE] 처리 (FIX-025) |
| `docs/systems/economy-architecture.md` | ShopSystem `OnItemPurchased` → `OnShopPurchased` 변경; 이벤트 표에 `EconomyEvents.OnShopPurchased`/`OnSaleCompleted`/`OnGoldSpent` 추가 (리뷰 E-01) |
| `docs/mcp/save-load-tasks.md` | 세이브/로드 MCP 태스크 시퀀스 독립 문서 — 신규 (ARC-012) |
| `TODO.md` | FIX-025·ARC-012 완료 처리, FIX-026 등록 |

---

## 주요 결정 사항

### FIX-025: 튜토리얼 이벤트명 확정

**Step 07 (수면)**: `TimeEvents.OnSleepExecuted`(미정의) → `TimeManager.OnSleepCompleted`
- `TimeManager.SkipToNextDay()` 내부에서 `OnSleepCompleted` → `OnDayChanged` 순서로 발행
- time-season-architecture.md에 이벤트 정의 추가 및 [OPEN] 설명 업데이트

**Step 11 (재투자)**: `EconomyEvents.OnItemPurchased`(미확정) → `EconomyEvents.OnShopPurchased`
- achievement-architecture.md 섹션 5의 구독 매핑에서 canonical 이름 확인
- 리뷰 과정에서 economy-architecture.md에도 구버전 `OnItemPurchased`가 잔존함을 발견 → 즉시 수정

### E-01 수정: economy-architecture.md 이벤트명 통일

리뷰어가 ShopSystem 다이어그램의 `OnItemPurchased`가 `OnShopPurchased`로 업데이트되지 않았음을 지적.
- `ShopSystem.OnItemPurchased` → `ShopSystem.OnShopPurchased`
- 구매 흐름(섹션 5.2) 이벤트 발행부: `ShopSystem.OnShopPurchased` + `EconomyEvents.OnShopPurchased` 정적 허브 래핑 명시
- 이벤트 표(섹션 2.2): `EconomyEvents.OnShopPurchased`/`OnSaleCompleted`/`OnGoldSpent` 3개 추가 — achievement-architecture.md와 완전히 일치

### ARC-012: save-load-tasks.md 신규 작성

기존 MCP 태스크 문서 패턴(achievement-tasks.md, tutorial-tasks.md)을 계승하여 작성.

**구성**:
- Part I: 설계 요약 (수치·배열 직접 기재 없이 save-load-architecture.md 참조)
- Part II: 9개 태스크, 약 94회 MCP 호출
  - T-1: ISaveable 인터페이스, SaveEvents, SaveVersionException
  - T-2: SaveManager, AutoSaveTrigger 핵심 구조
  - T-3: GameSaveData 데이터 클래스 계층
  - T-4: SaveMigrator, SaveDataValidator
  - T-5: 씬 배치 (SaveManager GameObject)
  - T-6: 자동저장 트리거 이벤트 연결 확인
  - T-7: SaveSlotPanel UI (3슬롯)
  - T-8: PauseMenu 연동
  - T-9: 통합 테스트

**PATTERN-006 준수**: SaveLoadOrder 할당표, GameSaveData 필드 목록 모두 직접 복사 없이 참조만 기재.

---

## 리뷰 요약

| 항목 | 결과 |
|------|------|
| E-01: economy-architecture.md 이벤트명 불일치 | 수정 완료 |
| E-02: time-season-architecture.md Cross-references 누락 | 수정 완료 |
| W-01: tutorial-tasks.md Step 11 참조 주석 출처 수정 | 수정 완료 |
| W-04: save-load-tasks.md MAX_SLOTS 참조 출처 통일 | 수정 완료 |
| W-02: achievement-tasks.md 이벤트명 (OnSaleCompleted 등) | 기존 이슈, 별도 FIX 등록 권장 (economy-architecture.md 섹션 2.2 추가로 간접 해소) |
| W-03: ISaveable canonical 출처 불명확 | save-load-architecture.md 섹션 7이 단일 출처로 확정 |

---

*이 문서는 Claude Code가 FIX-025 및 ARC-012 태스크에 따라 자율적으로 작성했습니다.*
