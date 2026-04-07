# Devlog #086 — DES-018: 수집 도감 시스템 설계 (통합 도감 채택)

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

DES-018 태스크: 채집 도감과 어종 도감의 통합 여부를 결정하고, 통합 수집 도감 시스템을 설계했다.

---

## 설계 결정: 통합 도감 채택

| 방안 | 내용 | 채택 |
|------|------|------|
| A. 통합 도감 | 어종/채집 탭 통합, CollectionUIController 신규 | ✓ |
| B. 분리 유지 | 어종 도감(기존), 채집 도감(신규) 별도 UI | - |

**채택 근거**: UX 일관성, 전체 수집 진행률 단일 표시, 향후 도감 확장성(요리 도감 등). 기존 FishCatalogManager는 변경 없이 유지하며 통합 UI가 참조한다.

---

## 채집 도감 핵심 수치

### 초회 보상 (희귀도별)

| 희귀도 | 골드 | XP | 해당 아이템 수 |
|--------|------|-----|--------------|
| Common | 5G | 2 XP | 12종 |
| Uncommon | 15G | 5 XP | 9종 |
| Rare | 50G | 15 XP | 4종 |
| Legendary | 200G | 50 XP | 2종 |
| **총합** | **1,275G** | **351 XP** | **27종** |

### 마일스톤 완성 보상

| 마일스톤 | 보상 |
|----------|------|
| 10종 완성 | 200G + 50 XP |
| 20종 완성 | 400G + 120 XP |
| 27종 완성 (전체) | 550G + 150 XP + "채집 학자" 칭호 |
| 도감 27종 완성 + 숙련도 Lv.10 | "채집 마스터" 칭호 (최종) |

**채집 도감 총 보상**: 초회 1,275G+351XP + 마일스톤 1,150G+320XP = **2,425G + 671 XP**

---

## 수정된/생성된 파일

| 파일 | 변경 내용 |
|------|----------|
| `docs/systems/collection-system.md` | **신규** — 통합 도감 설계, 채집 도감 27종 항목, 보상 체계 |
| `docs/systems/collection-architecture.md` | **신규** — ARC-037, GatheringCatalogManager/Data/Entry C# 아키텍처 |
| `docs/systems/gathering-system.md` | 섹션 9 OPEN#3 완료 처리 |

---

## 리뷰 수정 사항 (4건 CRITICAL)

| 이슈 | 수정 내용 |
|------|----------|
| PATTERN-010: OPEN 태그 미제거 | `firstDiscoverGold/XP`·`milestones` canonical 참조로 교체 |
| PATTERN-005: 필드명 불일치 | `hintUnlocked`→`descriptionUnlocked`, `firstCatchXxx`→`firstDiscoverXxx` 동기화 |
| PATTERN-006: 힌트 텍스트 직접 기재 | 54개 힌트 텍스트 삭제 → `gathering-items.md` canonical 참조 |
| 수치 하드코딩 | 15/27/42 등 어종·채집 수 → canonical 참조 주석 추가 |

---

## 후속 태스크 (TODO 추가)

- FIX-093: save-load-architecture.md SaveLoadOrder=56 추가
- FIX-094: data-pipeline.md GameSaveData 확장
- FIX-095: project-structure.md 네임스페이스 추가
- FIX-096: fish-catalog.md UI 경로 문구 수정

---

*이 문서는 Claude Code가 DES-018 세션에서 자율적으로 작성했습니다.*
