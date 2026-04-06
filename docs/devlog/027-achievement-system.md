# Devlog #027 — 도전 과제/업적 시스템 설계 (DES-010)

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### DES-010: 도전 과제/업적 시스템 설계

`docs/systems/achievement-system.md` 및 `docs/systems/achievement-architecture.md` 신규 작성.

#### 게임 디자인 (achievement-system.md)

**총 30개 업적**, 7개 카테고리:

| 카테고리 | 업적 수 | 단계형 | 단일 |
|----------|---------|--------|------|
| 농업 마스터 (Farming) | 5 | 1 | 4 |
| 경제 달인 (Economy) | 4 | 1 | 3 |
| 시설 개척자 (Facility) | 4 | 1 | 3 |
| 도구 장인 (Tool) | 3 | 0 | 3 |
| 탐험가 (Explorer) | 4 | 1 | 3 |
| 퀘스트 영웅 (Quest) | 4 | 1 | 3 |
| 숨겨진 업적 (Hidden) | 6 | 0 | 6 |

**칭호 시스템**: 36개 칭호 정의 (업적 달성 시 해금, 플레이어 이름 옆 표시)

**UI/UX**: Y키 토글, 토스트 알림(4초/숨김 6초), 카테고리 탭 필터링, 진행도 표시

**추적 이벤트 15종**: HarvestCount, GoldEarned, BuildingCount, ToolUpgradeCount, NPCMet, QuestCompleted, SpecificCropHarvested, GoldSpent, DaysPlayed, SeasonCompleted, SpecificBuildingBuilt, TotalItemsSold, QualityHarvestCount, ProcessingCount, Custom(숨겨진 업적)

#### 기술 아키텍처 (achievement-architecture.md)

**문서 ID**: ARC-017

**핵심 클래스 8개**:
- `AchievementManager` — MonoBehaviour, Singleton, ISaveable (SaveLoadOrder=90)
- `AchievementEvents` — static 이벤트 허브 (OnAchievementUnlocked, OnProgressUpdated)
- `AchievementData` — ScriptableObject (16필드 — 단계형 업적 지원 포함)
- `AchievementRecord` — [Serializable] 런타임 진행도 (8필드 — currentTier, tierHistory 포함)
- `AchievementTierData` — [Serializable] 단계(Bronze/Silver/Gold) 조건·보상 구조
- `TierUnlockRecord` — [Serializable] 단계 해금 기록
- `AchievementSaveData` — [Serializable] 세이브 구조
- UI 3종: `AchievementPanel`, `AchievementToastUI`, `AchievementItemUI`

**열거형 4개**: `AchievementType` (Single/Tiered), `AchievementCategory` (7값), `AchievementConditionType` (15값 + Custom=99), `AchievementRewardType` (None/Gold/XP/Item/Title=5값)

**이벤트 구독 10개**: FarmEvents, EconomyEvents×2, BuildingEvents, ToolEvents, NPCEvents, QuestEvents, ProcessingEvents, TimeManager×2

---

## 리뷰 결과

**CRITICAL 4건 (수정 완료)**:
- [C-1] 단축키 불일치: `achievement-system.md`는 Y키, `achievement-architecture.md`는 A키 → **architecture.md 전체 A키→Y키 통일** (5개소)
- [C-2] 업적 총 개수 불일치: 아키텍처의 `"12/50"` 예시가 디자인 문서의 30개와 불일치 → **`"12/30"` + canonical 참조 주석으로 교체**
- [C-3] 단계형 업적 SO 구조 미비: `AchievementData`에 `tiers[]` 필드 없음 → **`AchievementType` enum, `AchievementTierData` 클래스, `tiers` 필드 추가**
- [C-4] 데이터 모델 필드 불일치: `type`, `tiers`, `rewardTitleId` 필드 누락 → **SO 필드 및 PATTERN-005 검증 테이블 업데이트**

**WARNING 6건 (수정 완료)**:
- [W-1] 토스트 표시 시간 불일치 (아키텍처 3초 vs 디자인 4/6초) → **4f / 숨김 6f로 수정 + canonical 참조**
- [W-2] 숨겨진 업적 조건 타입 누락 → **`Custom = 99` 추가 + 적용 업적 주석**
- [W-3] 칭호 시스템 [OPEN] 오류 (이미 디자인 확정됨) → **[OPEN] 제거 + Y키 결정 반영**
- [W-4] 씬 계층에 Tab_Hidden 누락 → **CategoryTabs에 Tab_Hidden 추가**
- [W-5] 복합 보상 표현 불가 → **`AchievementRewardType`에 Title=4 추가, `rewardTitleId` 필드 추가**
- [W-6] AchievementRecord에 단계 진행 필드 누락 → **`currentTier`, `tierHistory`, `TierUnlockRecord` 추가**

---

## 의사결정 기록

1. **단계형 업적(Tiered) 구조**: 별도 SO를 3개 만드는 방안 대신, `AchievementData` 하나에 `AchievementTierData[] tiers` 배열을 포함하는 방안 채택. SO 파일 수 절감(5개 단계형 업적 × 3배 = 15개 → 5개) 및 단일 에셋에서 전체 업적 구조 파악 가능.

2. **Custom ConditionType**: 숨겨진 업적 4종(비 오는 날 수확, 밤 활동, 특정 도구만 사용, 잔액 0 판매)은 다중 조건이 얽혀 있어 단일 enum으로 표현 불가. `Custom = 99`를 선언하고 AchievementManager 내 하드코딩 핸들러로 처리. 향후 추가되는 복합 조건 업적에도 동일 패턴 적용.

3. **Y키 바인딩 확정**: WASD 이동키와의 충돌 방지. 퀘스트(J키), 인벤토리(I키) 등 기존 키바인딩 패턴과 일관성 유지.

4. **SaveLoadOrder=90**: QuestManager(85)보다 나중에 로드하여 QuestCompleted 조건 타입 업적의 진행도를 정확히 복원. 현재 할당표 최고값.

---

## 미결 사항 ([OPEN])

- 업적 총 개수 최종 확정: 향후 `docs/content/achievements.md`에서 30개 기준으로 정의 예정
- 업적 보상 수치(골드/XP): `docs/balance/progression-curve.md`에 업적 보상 기준 추가 필요
- `save-load-architecture.md` GameSaveData 루트에 `achievements` 필드 추가 필요 (PATTERN-005)

---

## 후속 작업

- `ARC-017`: 업적 MCP 태스크 시퀀스 (achievement-architecture.md Part II → docs/mcp/achievement-tasks.md)
- `CON-007`: 업적 콘텐츠 상세 (30종 목록, 단계별 수치, 보상 → docs/content/achievements.md)
- `ARC-012`: 세이브/로드 MCP 태스크 시퀀스
- `ARC-010`: 튜토리얼 MCP 태스크 시퀀스
- `BAL-006`: 퀘스트 보상 밸런스 분석

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
