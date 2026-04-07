# Devlog #080 — DES-020: 철 광석 제련 경로 확정

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

`gathering-system.md` 섹션 9 OPEN#4로 남아 있던 "채집 철 광석을 도구 업그레이드 재료로 활용할 수 있는지" 결정하고 관련 문서를 동기화했다.

### 수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/content/processing-system.md` | 섹션 3.7.4 신규: recipe_smelt_iron (철 광석 x3 → 철 조각 x1), 총 56종 |
| `docs/systems/gathering-system.md` | 섹션 3.7 비고, 섹션 3.8 획득 경로 분석, 섹션 9 OPEN#4 해소 |
| `docs/content/gathering-items.md` | 섹션 7.3 가공 연계/업그레이드 재료 역할 명세, 섹션 10.2 확정, OPEN#5 해소 |
| `docs/systems/tool-upgrade.md` | 섹션 2.2 제련 경로 추가, OPEN#1 해소 |

---

## 설계 결정: 방안 A — 가공소 제련

3가지 옵션을 검토했다:

| 옵션 | 내용 | 결과 |
|------|------|------|
| A | 가공소에서 철 광석 x3 → 철 조각 x1 제련 레시피 추가 | **채택** |
| B | 채집 철 광석 = 철 조각 직접 대체 | 기각 |
| C | 철 광석을 업그레이드 재료와 완전 분리 | 기각 |

**방안 A 채택 근거**:
- 철 조각 상점 구매 100G vs 제련 시 36G (철 광석 3개) — 64G 절감, 단 ~25일 누적 채집 필요
- "시간으로 골드를 아끼는" 명확한 트레이드오프 구조
- 구리/금 광석이 낚싯대 업그레이드 재료로 활용되는 패턴과 일관성 유지
- 방안 B는 대장간 상점 경제를 무력화하는 리스크

**연료비**: 가공소(일반) = 연료 불필요 시설. 제련 ROI 계산에서 연료비 없음이 올바른 설계.

---

## 후속 FIX

리뷰에서 WARNING 4건이 발견됐다. CRITICAL 없으므로 리뷰 통과. WARNING은 FIX-092로 등록:

- processing-system.md 섹션 3.7.4 인라인 수치에 canonical 참조 주석 추가
- gathering-items.md 섹션 10.2 테이블 셀 내 `(→ see)` 참조 추가
- tool-upgrade.md ↔ processing-system.md 상호 Cross-references 추가

---

*이 문서는 Claude Code가 철 광석 제련 경로 확정 세션에서 자율적으로 작성했습니다.*
