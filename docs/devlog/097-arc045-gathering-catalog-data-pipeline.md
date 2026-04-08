# Devlog #097 — ARC-045: GatheringCatalogData SO 스키마 data-pipeline.md 추가

> 2026-04-08 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

ARC-041(collection-tasks.md)에서 도입된 `GatheringCatalogData` SO 타입이 `data-pipeline.md`에 누락된 문제를 해소했다. PATTERN-007 준수를 위해 섹션 2.13을 신규 추가하고, 섹션 1.1 테이블과 Cross-references도 갱신했다.

---

## ARC-045 — GatheringCatalogData SO 스키마 추가

### 변경 파일

| 파일 | 변경 내용 |
|------|---------|
| `docs/pipeline/data-pipeline.md` | 섹션 1.1 테이블에 GatheringCatalogData 행 추가 |
| `docs/pipeline/data-pipeline.md` | 총 예상 에셋 수 ~190개 → ~217개 갱신 |
| `docs/pipeline/data-pipeline.md` | 섹션 2.13 GatheringCatalogData 신규 추가 (9필드, 27에셋) |
| `docs/pipeline/data-pipeline.md` | Cross-references에 collection-architecture.md, collection-system.md, gathering-items.md 3개 추가 |

### 섹션 2.13 주요 내용

**GatheringCatalogData** (27종, `SO_GatherCatalog_<ID>` 패턴):

| 필드 | 타입 | 역할 |
|------|------|------|
| itemId | string | GatheringItemData.dataId 동일 키 |
| displayName | string | 도감 표시명 |
| hintLocked | string | 미발견 힌트 |
| descriptionUnlocked | string | 발견 후 설명 |
| rarity | GatheringRarity | 희귀도 |
| firstDiscoverGold | int | 초회 발견 골드 보상 |
| firstDiscoverXP | int | 초회 발견 XP 보상 |
| catalogIcon | Sprite | 에디터 전용, JSON 직렬화 제외 |
| sortOrder | int | 표시 순서 |

모든 콘텐츠 값은 canonical 참조(gathering-items.md, collection-system.md 섹션 3.3)로 대체. 직접 수치 기재 없음 (PATTERN-007 준수).

### canonical 출처
- 클래스 정의: `docs/systems/collection-architecture.md` 섹션 3
- 힌트/설명 텍스트: `docs/content/gathering-items.md`
- 초회 보상 수치: `docs/systems/collection-system.md` 섹션 3.3
- 희귀도 enum: `docs/systems/gathering-architecture.md` 섹션 2.2

---

## TODO 업데이트

- ARC-045 → DONE
- 활성 항목: 11개 (1개 완료)

---

*이 문서는 Claude Code가 ARC-045 세션에서 자율적으로 작성했습니다.*
