# Devlog #028 — 업적 MCP 태스크 시퀀스 + 업적 콘텐츠 상세 (ARC-017 + CON-007)

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### ARC-017: 업적 시스템 MCP 태스크 시퀀스

`docs/mcp/achievement-tasks.md` 신규 작성.

**태스크 맵 (6단계, ~548회 MCP 호출 / T-2-ALT 사용 시 ~102회)**:

| 단계 | 내용 | MCP 호출 수 |
|------|------|-------------|
| T-1 | 스크립트 생성 (enum 4종 + Serializable 3종 + AchievementManager + UI 3종) | ~57회 |
| T-2 | SO 에셋 생성 (30개 AchievementData) | ~451회 (ALT: ~5회) |
| T-3 | UI 프리팹/씬 오브젝트 | ~20회 |
| T-4 | 씬 배치 및 참조 연결 | ~8회 |
| T-5 | 세이브 통합 + 이벤트 검증 | ~7회 |
| T-6 | 통합 테스트 시퀀스 | ~5회 |

주요 설계 결정:
- **T-2-ALT 패턴**: 30개 SO를 개별 MCP 호출로 생성하는 대신, Editor 스크립트로 일괄 생성하는 대안 제공. 호출 수 451 → 5회로 절감.
- **SaveLoadOrder=90**: QuestManager(85)보다 나중에 로드하여 QuestCompleted 조건 정확히 복원.

---

### CON-007: 업적 콘텐츠 상세

`docs/content/achievements.md` 신규 작성.

**업적 30종 전체 확정**:

| 카테고리 | 업적 수 | 단계형 | 보상 총량 |
|----------|---------|--------|-----------|
| Farming | 5 | 1 (ach_farming_02) | 골드 1,150G + XP 500 |
| Economy | 4 | 1 (ach_economy_02) | 골드 900G + XP 400 |
| Facility | 4 | 1 (ach_facility_04) | 골드 1,050G + XP 450 |
| Tool | 3 | 0 | 골드 800G + XP 300 |
| Explorer | 4 | 1 (ach_explorer_04) | 골드 700G + XP 250 |
| Quest | 4 | 1 (ach_quest_02) | 골드 950G + XP 350 |
| Hidden | 6 | 0 | 골드 500G + 특수 |
| **합계** | **30** | **5** | **골드 6,050G / XP 2,250** |

**칭호 36종** canonical 테이블 정의 완료.

**아이템 보상 10종** 목록 확정 (특수 레시피 및 상인의 뱃지 장식품 포함).

---

## 리뷰 결과

**CRITICAL 2건 (수정 완료)**:
- [C-1] achievements.md 섹션 2.4 XP 추정치(`~1,690`) vs 확정 합산(`2,250`) 불일치 → **2,250으로 확정 수정 + [RISK] 목표 범위(33~43%) 초과 경고 추가**
- [C-2] achievement-tasks.md T-2-02 칭호 ID 오타: `title_newbie_farmer` → **`title_sprout_farmer`** (canonical 참조 주석 추가)

**WARNING 3건 (수정 완료)**:
- [W-1] achievement-tasks.md Economy04 conditionType 이중 기재(`ProcessingCount + GoldEarned`) → **단일 `GoldEarned` 확정, 필터는 핸들러 내부 처리 명시**
- [W-2] achievement-tasks.md Explorer [OPEN] 누락(GoldSpent semantic 불일치) → **[OPEN] 태그 추가, `PurchaseCount` 전용 타입 검토 필요 명시**
- [W-3] achievement-architecture.md 다이어그램 `OnGoldChanged` vs 본문 `OnGoldSpent` 불일치 → **`OnGoldSpent` 통일**

**INFO 4건 (수정 완료)**:
- [I-1] achievement-architecture.md 외부 참조 필드 방식(직렬화 필드 vs Singleton) 불일치 → **Singleton 직접 접근 방식으로 다이어그램 통일**
- [I-2] achievement-system.md 아이템 보상 8종 → 10종으로 확장 누락 → **2종 추가**
- [I-3] achievement-tasks.md 태스크 맵 호출 수 과소 추정(~192) → **~548(T-2-ALT: ~102)으로 수정**
- [I-4] achievement-tasks.md Cross-references에 achievements.md 누락 → **추가**

---

## 의사결정 기록

1. **T-2-ALT (Editor 스크립트 일괄 생성)**: 30개 SO 개별 생성은 451회 MCP 호출을 요구한다. Editor 스크립트 방식이 실용적으로 우월하나, 단계별 검증이 어려운 단점이 있어 두 방안을 모두 문서화하고 구현 시 선택 여지를 남김.

2. **XP 총량 초과 [RISK] 유지**: 업적 XP 합산 2,250이 progression-curve.md 목표 범위(33~43%)를 초과(49%). 억지로 낮추는 것보다 실제 구현 후 플레이테스트로 검증하는 것이 나을 것으로 판단하여 [RISK] 태그를 남기고 확정값으로 기재.

3. **PurchaseCount conditionType 미확정 → FIX-011로 이관**: Explorer 업적 2종(ach_explorer_02 상인 방문, ach_explorer_04 상점 구매)의 조건 추적 방식이 GoldSpent와 맞지 않는다. 아키텍처 변경이 필요한 사안이므로 별도 FIX 태스크로 이관.

---

## 미결 사항 ([OPEN])

- **FIX-010**: save-load-architecture.md GameSaveData에 `achievements: AchievementSaveData` 필드 추가 필요
- **FIX-011**: AchievementConditionType에 `PurchaseCount` 전용 타입 추가 검토
- **[RISK] 업적 XP 총량**: 2,250 XP가 전체 XP의 ~49%로 목표 범위 초과 — 플레이테스트 후 재조정 예정
- **ach_facility_04**: 특수 레시피 보상 내용 미확정 (`[OPEN]` 유지)

---

## 후속 작업

- `FIX-010`: save-load-architecture.md achievements 필드 추가
- `FIX-011`: PurchaseCount conditionType 결정
- `ARC-012`: 세이브/로드 MCP 태스크 시퀀스
- `ARC-010`: 튜토리얼 MCP 태스크 시퀀스
- `BAL-006`: 퀘스트 보상 밸런스 분석

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
