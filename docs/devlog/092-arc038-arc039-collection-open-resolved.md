# Devlog #092 — ARC-038 / ARC-039 / CON-015 / FIX-102: 수집 도감 아키텍처 OPEN 항목 확정

> 2026-04-08 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

세션 예산 2작업 내에서 수집 도감 아키텍처(`collection-architecture.md`)의 잔존 OPEN 항목 두 가지를 확정하고, 연계 문서를 일괄 동기화했다.

---

## Task 1: ARC-038 + ARC-039 + CON-015

### ARC-038 — GatheringRarity/FishRarity 통합 enum 확정 + ICatalogProvider 인터페이스 범위 결정

**결정: 분리 유지. ICatalogProvider 인터페이스 도입 없음.**

| 근거 | 내용 |
|------|------|
| 설계 원칙 준수 | "FishCatalogManager 변경 없음" 원칙 — FishRarity를 공통 네임스페이스로 이동하면 FishCatalogManager 내부 수정 필요, 원칙 위반 |
| 탭 독립 렌더링 | CollectionUIController가 FishPanel/GatheringPanel 탭별로 독립 렌더링 → 공통 enum 불필요 |
| 향후 독립 진화 | FishRarity: isGiant 연동 가능성, GatheringRarity: weatherBonus 연동 가능성 — 분리가 확장성에 유리 |

ICatalogProvider 인터페이스 도입 시 FishCatalogManager에 `implements ICatalogProvider`를 추가해야 하므로 "FishCatalogManager 변경 없음" 원칙에 위배. 현재 CollectionUIController가 두 매니저를 직접 참조하는 설계가 더 단순하고 원칙에 부합한다.

**연동 수정**: `gathering-architecture.md` 섹션 9 OPEN#2가 ARC-038 결정을 미반영 상태였음 → 리뷰 과정에서 취소선 처리 + `[ARC-038 확정]` 태그 추가 완료.

### ARC-039 — CollectionPanel/FishCatalogPanel 씬 마이그레이션 전략 확정

**결정: In-place migration (참조 재연결 방식), Q-4a~Q-4f 6단계 확정.**

| 단계 | MCP 명령 | 내용 |
|:----:|---------|------|
| Q-4a | 프리팹 이름 변경 | FishCatalogPanel → FishPanel |
| Q-4b | CollectionPanel 하위 이동 | 씬 계층 반영, 섹션 6.4 |
| Q-4c | CloseButton 비활성화 | CollectionPanel 공통 CloseButton 사용 |
| Q-4d | Header 완성도 표시 비활성화 | CollectionUIController에 위임 |
| Q-4e | Inspector 참조 재연결 | FishCatalogUI.cs 코드 변경 없음 |
| Q-4f | 구버전 프리팹 DEPRECATED 처리 | Legacy/ 폴더 이동 |

FishCatalogToast는 독립 프리팹으로 유지 (collection-architecture.md 섹션 6.4 현행 방침).

### CON-015 — collection-system.md OPEN#5 닫기

ARC-038/ARC-039 확정으로 `collection-system.md` OPEN#5("통합 도감 아키텍처(ICatalogProvider 인터페이스, CollectionManager 등)는 별도 ARC 태스크로 설계해야 한다")도 함께 해소됐다. RESOLVED 태그와 함께 collection-architecture.md Q-4a~Q-4f 전략으로의 참조 추가.

---

## Task 2: FIX-102 — save-load-architecture.md Cross-references 보완

`FIX-093`에서 `gatheringCatalog` 필드를 save-load-architecture.md에 추가하면서 collection-architecture.md가 연계 문서가 됐지만, Cross-references에 등재되지 않았다. 단순 참조 행 추가로 완료.

```
- docs/systems/collection-architecture.md (ARC-037) -- GatheringCatalogSaveData 구조,
  GatheringCatalogManager SaveLoadOrder 56 할당 (섹션 5.2, 7) — FIX-093에서 gatheringCatalog
  필드 추가로 연계됨
```

---

## 리뷰 결과 (ARC-038 / ARC-039)

Reviewer가 다음 이슈를 추가 발견·수정했다:

| 심각도 | 위치 | 이슈 | 처리 |
|--------|------|------|------|
| CRITICAL | gathering-architecture.md 섹션 9 OPEN#2 | ARC-038 결정 미반영 상태 | 취소선 + [ARC-038 확정] 태그 추가 |
| WARNING | collection-architecture.md 섹션 3.1 | 희귀도별 수치 (Common=5G/2XP 등) 직접 기재 — canonical 참조로 교체 필요 | canonical 참조 표기로 교체 |
| WARNING | collection-architecture.md Cross-references | collection-system.md 참조 누락 | 행 추가 |

계산식 및 PATTERN-005 검증 3건 전수 확인 — 모두 정확.

---

## 수정된 파일 목록

| 파일 | 변경 내용 |
|------|---------|
| `docs/systems/collection-architecture.md` | ARC-038 확정: OPEN#3 닫기, 섹션 8.6 주석 추가. ARC-039 확정: OPEN#4 닫기, Q-4→Q-4a~Q-4f 확장, Cross-references 마이그레이션 노트, collection-system.md 행 추가, 수치 canonical 참조 교체 |
| `docs/systems/gathering-architecture.md` | 섹션 9 OPEN#2 닫기 [ARC-038 확정] |
| `docs/systems/collection-system.md` | OPEN#5 닫기 [ARC-038/ARC-039 확정] |
| `docs/systems/save-load-architecture.md` | Cross-references에 collection-architecture.md 행 추가 (FIX-102) |
| `TODO.md` | ARC-038/039/CON-015/FIX-102 완료, 신규 항목 7개 추가 |

---

*이 문서는 Claude Code가 ARC-038/ARC-039/CON-015/FIX-102 세션에서 자율적으로 작성했습니다.*
