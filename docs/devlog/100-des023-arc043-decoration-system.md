# Devlog #100 — DES-023 + ARC-043: 농장 장식 시스템 설계 및 아키텍처

> 2026-04-08 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

DES-023(농장 장식/꾸미기 시스템 설계)과 ARC-043(기술 아키텍처)을 완성하고, 리뷰어 이슈 7건을 해소했다.

---

## DES-023 — 농장 장식 시스템 설계

### 설계 결정: 포함 확정

장식 시스템이 필요한 이유:
1. `design.md` 섹션 2에 "농장 꾸미기의 즐거움"이 핵심 감정으로 명시됨
2. 중후반 골드 잉여 구간(레벨 8~10)의 골드 소모처 부재 문제 해소
3. 핵심 루프와 독립된 표현 레이어로 구현 가능 (생산성 효과 없음)

### 장식 카테고리 5종

| 카테고리 | 배치 방식 | 기능 효과 |
|----------|----------|----------|
| Fence (울타리) | edge 배치 | 내구도 소모 (나무), 동물 차단 [OPEN#1] |
| Path (경로) | 타일 오버레이 | 이동 +10%, 잡초 억제 |
| Light (조명) | 1x1 타일 점유 | 야간 가시 반경 [OPEN#2] |
| Ornament (장식물) | 1x1~2x2 타일 점유 | 순수 미관 |
| WaterDecor (수경 장식) | 2x2~3x3 타일 점유 | Zone F 전용, 순수 미관 |

### 핵심 메카닉 결정

- 배치: 그리드 고정 (타일 시스템 일관성)
- 철거: 0% 환불 소모형 [OPEN#3: 50% 플레이테스트 후 재결정 가능]
- 경제 규모: 선택적 7,500~12,500G (1년차 후반~2년차 소모처)
- 구매 경로: 목공소/잡화점 직구매 + 이벤트/퀘스트 보상
- `economy-system.md` 섹션 1.4 [OPEN] 해소 — DES-023으로 확정

### 신규 문서

- `docs/systems/decoration-system.md` (DES-023)
- `docs/design.md` 섹션 4.6.1 장식 카테고리 테이블 추가

---

## ARC-043 — 기술 아키텍처

### DecorationManager 설계

- Singleton, ISaveable, `SaveLoadOrder = 57` (GatheringCatalogManager=56 직후)
- `Dictionary<int, DecorationInstance>` 인스턴스 상태 관리
- CanPlace() 6단계 우선순위 검사: Zone 해금 → 시설 → 경작지 → 수원 → 기존 장식 → 레벨/계절
- category별 렌더링 분기: Fence/Path → Tilemap.SetTile, 나머지 → Instantiate

### SO 에셋 스키마

- `DecorationItemData`: 29종 (Fence 4 / Path 5 / Light 4 / Ornament 11 / WaterDecor 5)
- 콘텐츠 수치 직접 기재 금지 (PATTERN-007) — 모두 decoration-system.md 참조
- `DecorationConfig`: 전역 설정 SO

### 세이브 구조 (PATTERN-005 준수)

```
DecorationSaveData {
  decorations: DecorationInstanceSave[]  // 7필드
  nextInstanceId: int
}
```
JSON 7개 필드 ↔ C# 클래스 7개 필드 완전 일치 확인.

### Tilemap 레이어 구조

```
Decorations (신규 GameObject)
├── PathLayer    (Order: 1) — 경로 바닥 오버레이
├── FenceLayer   (Order: 2) — Rule Tile auto-tiling
└── DecoObjects  (Transform) — 조명·장식물 오브젝트 부모
```

### 신규 문서

- `docs/systems/decoration-architecture.md` (ARC-043)
- `docs/systems/save-load-architecture.md` SaveLoadOrder 표에 `DecorationManager | 57` 추가

---

## 리뷰어 이슈 처리 (7건)

| 번호 | 심각도 | 이슈 | 처리 |
|------|--------|------|------|
| I-1 | CRITICAL | C# 코드 블록 내 `[OPEN - ...]` 태그 비문법적 형태 | `// [OPEN - ...]` 주석으로 수정 |
| I-2 | CRITICAL | SaveLoadOrder=57이 canonical 할당표에 미반영 | save-load-architecture.md 섹션 7에 행 추가 |
| I-3 | WARNING | decoration-system.md 섹션 7 구매가 범위 중복 | `(→ see 섹션 2.x)` 참조로 교체 |
| I-4 | WARNING | Cross-references에 quest-system.md 누락 | 추가 |
| I-5 | WARNING | Place() 흐름에 인벤토리 차감 연동 미명시 | InventoryManager.RemoveItem() 호출 + [OPEN] 추가 |
| I-6 | WARNING | economy-system.md 섹션 1.4 [OPEN] 미해소 | [RESOLVED - DES-023] 처리 |
| I-7 | INFO | OPEN 근거 문구 "DES-023 미확정" 부정확 | "Phase 2 성능 프로파일링 후 확정"으로 수정 |

---

## 후속 항목 등록

ARC-043 Open Questions에서 식별된 후속 FIX 태스크:

| ID | 우선순위 | 내용 |
|----|---------|------|
| FIX-111 | 2 | save-load-architecture.md GameSaveData에 decoration 필드 추가 |
| FIX-113 | 2 | data-pipeline.md DecorationItemData/DecorationConfig SO 스키마 추가 |
| FIX-112 | 1 | project-structure.md SeedMind.Decoration 네임스페이스 추가 |
| ARC-046 | 1 | decoration-tasks.md MCP 태스크 시퀀스 문서화 |

---

*이 문서는 Claude Code가 DES-023 + ARC-043 세션에서 자율적으로 작성했습니다.*
