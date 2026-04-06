# Devlog #021 — 시설 MCP 태스크 시퀀스 (ARC-007)

> 2026-04-06 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### ARC-007: 시설 시스템 MCP 태스크 시퀀스 독립 문서화

facilities-architecture.md의 Part II를 독립 상세 문서로 분리·확장.

**신규 문서**:
- `docs/mcp/facilities-tasks.md` — 시설 시스템 MCP 태스크 시퀀스 (ARC-007)

**수정된 문서**:
- `docs/systems/facilities-architecture.md` — 리뷰 수정: JSON dataId 4종 교정, Phase A 7종 시설 확장 반영
- `docs/systems/facilities-architecture.md` — Phase A 목표를 "BuildingData SO 7개 + 레시피 SO 32개"로 정정, Step A-11~A-13(Mill/Fermentation/Bakery) 추가, 레시피 Step 번호 A-14~A-45로 재지정

---

## 핵심 설계 내용

### facilities-tasks.md 구성

**총 MCP 호출 예상**: ~232회 (Editor 스크립트 우회 시 대폭 감소)

| 태스크 | 내용 | 호출 수 |
|--------|------|--------|
| F-1 | BuildingData SO 에셋 생성 (7종 + 폴더 2개) | 79회 |
| F-2 | 스크립트 17종 생성 (enum/SO/런타임/매니저/서브시스템) | 12회 |
| F-3 | 시설 프리팹 생성 (7종 완성 + 건설중 공통) | 72회 |
| F-4 | 씬 배치 (BuildingManager GO + SO 배열 연결) | 8회 |
| F-5 | 건설 UI (BuildingShopPanel) | 26회 |
| F-6 | 업그레이드 UI (BuildingInfoPanel) | 14회 |
| F-7 | 시설 인터랙션 연동 | 10회 |
| F-8 | 통합 테스트 시퀀스 | 18회 |

**스크립트 목록 (S-01~S-17)**:
- S-01~S-06: 데이터/런타임 레이어 (BuildingEffectType, PlacementRule, BuildingData, BuildingInstance, BuildingEvents, BuildingManager)
- S-07~S-09: 물탱크·온실 서브시스템 + ISeasonOverrideProvider 인터페이스
- S-10~S-12: StorageSlot, StorageSlotContainer, StorageSystem
- S-13~S-14: BuildingShopUI, BuildingInfoUI
- **S-15~S-17** (신규): ProcessingSlot, ProcessingSystem, BuildingInteraction ← 리뷰 C-3 반영

---

## 리뷰 결과

**CRITICAL 3건 (수정 완료)**:
1. [C-1] facilities-architecture.md JSON 예시의 dataId 4종 불일치 (`"water_tank"` 등 → `"building_water_tank"` 등으로 수정)
2. [C-2] Phase A가 신규 3종 시설(Mill/Fermentation/Bakery) SO 누락 → Step A-11~A-13 추가, 레시피 스텝 A-14~A-45로 번호 재지정
3. [C-3] 스크립트 목록에 ProcessingSlot.cs, ProcessingSystem.cs, BuildingInteraction.cs 누락 → S-15~S-17 추가

**WARNING 6건 (수정 완료)**:
1. [W-1] Phase → 태스크 매핑 테이블 부재 → Cross-references 섹션에 Phase A~E 대응 테이블 추가
2. [W-2] F-2-06 BuildingManager에 Debug 메서드 5종 미명시 → DebugBuildInstant/Upgrade/Demolish/StoreItem/RetrieveItem 추가
3. [W-3] F-1-06~08의 buildCost/requiredLevel 참조처가 processing-system.md → design.md 섹션 4.6으로 수정
4. [W-4] F-5-07 SerializeField 연결에 _goldText, _buildingSlotPrefab 누락 → 추가
5. [W-5] F-3-02 MCP 호출 수 계산 오류 (9→10회), F-3-03~08 연쇄 수정 (54→60회)
6. [W-6] 의존성 다이어그램 SeedMind.Player 참조 방향 불명확 → Cross-references에 economy-system.md 추가 및 I-3 처리

**INFO 3건 (해결)**:
1. [I-1] F-8-05 StorageSystem 직접 호출(MonoBehaviour 아님) → BuildingManager.DebugStoreItem/RetrieveItem 프록시로 변경
2. [I-2] F-1-01에 Recipes 폴더 생성 누락 → 추가 (2회로 수정)
3. [I-3] Cross-references에 economy-system.md 섹션 2.5 추가

---

## 의사결정 기록

1. **dataId 네이밍 통일 (`building_*` 접두어)**: facilities.md와 facilities-tasks.md는 `building_water_tank` 등 접두어 형식을 사용하고 있었으나 facilities-architecture.md JSON 예시만 `"water_tank"` 단독 형식을 사용. DataRegistry 키로 사용되므로 전체 통일이 필수. canonical은 facilities.md의 영문 ID.

2. **StorageSystem 테스트를 BuildingManager 프록시로 변경**: StorageSystem은 Pure C# 클래스 (MonoBehaviour 미상속). MCP `execute_method`는 씬의 MonoBehaviour 대상이므로 직접 호출 불가. BuildingManager에 DebugStoreItem/DebugRetrieveItem을 추가하여 프록시 역할을 맡김.

3. **F-5 호출 수 정정 (24→26회)**: F-5-07에 _goldText, _buildingSlotPrefab 참조 연결 2회 추가. 총 합계 227→232회.

---

## 미결 사항 ([OPEN])

- 물탱크 범위 계산: 맨해튼 vs 체비셰프 거리 방식 확정 필요
- 온실 내부 진입 방식: 별도 씬 전환 vs 카메라 전환
- 창고↔인벤토리 아이템 이동 중재자 설계 (Player→Building 의존 금지 제약)
- F-5-07 SerializeField 참조를 Awake() 자동 탐색으로 대체 여부

---

## 후속 작업

- `ARC-008`: 도구 업그레이드 MCP 태스크 시퀀스 (tool-upgrade-tasks.md)
- `ARC-009`: NPC/상점 MCP 태스크 시퀀스 (npc-shop-tasks.md)
- `ARC-012`: 세이브/로드 MCP 태스크 시퀀스 (save-load-tasks.md)
- `BAL-003`: 겨울 작물 3종 ROI 분석 (crop-economy.md 추가)

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
