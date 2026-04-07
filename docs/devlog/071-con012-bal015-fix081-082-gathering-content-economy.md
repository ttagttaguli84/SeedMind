# Devlog #071 — CON-012 + BAL-015 + FIX-081/082: 채집 아이템 콘텐츠 및 경제 밸런스

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

채집 시스템(DES-016/ARC-031) 완료 후 downstream 작업으로, 27종 채집 아이템 콘텐츠 상세(CON-012)와 경제 밸런스 시트(BAL-015)를 완성했다. 또한 FIX-081(economy-architecture.md 구버전 pseudocode 정리)과 FIX-082(gathering-system.md Cross-references 보완)를 완료했다. Reviewer가 CRITICAL 2건을 발견하여 즉시 수정했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/content/gathering-items.md` | CON-012 신규: 27종 채집 아이템 maxStack/힌트텍스트/NPC선물/SFX/가공연계 확정 |
| `docs/balance/gathering-economy.md` | BAL-015 신규: 일일 수입 시뮬레이션, 조정안 D(판매가 40% 하향) 추천 |
| `docs/systems/economy-architecture.md` | FIX-081: 섹션 3.7.1/3.7.2에 `[SUPERSEDED by 섹션 3.10.3]` 배너 추가 |
| `docs/systems/gathering-system.md` | FIX-082: 섹션 8 Cross-references에 FIX-076~080 연계 6개 참조 추가 |
| `docs/systems/sound-design.md` | C-1 해소: 섹션 2.13 채집 SFX 11종 신규 추가, Phase 3 합계 56→67, 누적 84→95→151 |
| `docs/balance/gathering-economy.md` | C-2/W-2/W-4/W-5 수정: canonical 주석, 낚시 수입 불일치 수정, [OPEN] 보완, 목축 참조 추가 |
| `TODO.md` | FIX-081/082/CON-012/BAL-015 완료, BAL-016/FIX-083 신규 추가 |

---

## CON-012 채집 아이템 27종 콘텐츠 상세

### 핵심 결정사항

**인벤토리 분류**: 채집물은 `Gathered` (식물/열매/버섯), 광물은 `Mineral` 두 카테고리로 분류. `maxStack` 규칙:

| 분류 | maxStack | 예외 |
|------|----------|------|
| 일반 채집물 | 30 | Legendary 5개 |
| Legendary 채집물 | 5 | 산삼/황금연꽃/천년영지/동충하초 |
| 광물 | 30 | 자수정 10개 (선물 전용 희소성) |

**NPC 선물 적합도**: NPC 4인별 직업 성격 반영.
- 철수(대장간): 광석류 선호 + 식재료 좋아함
- 하나(잡화): 식재료/꽃/상품성 높은 아이템
- 목이(목공): 자연물 전반 (싫어하는 것 없음)
- 바람이(여행상인): 희귀/이국적 아이템 선호

**SFX 11종** canonical 등록 (sound-design.md 섹션 2.13):
`sfx_gather_plant`, `sfx_gather_herb`, `sfx_gather_berry`, `sfx_gather_mushroom`, `sfx_gather_leaf`, `sfx_gather_ore`, `sfx_gather_stone`, `sfx_gather_bark`, `sfx_gather_legendary`, `sfx_gather_point_empty`, `sfx_gather_respawn`

### 주요 발견: 가공 부가가치 역전 문제

gathering-system.md 섹션 9.1의 예시 가격(야생 베리잼 ~25G, 도토리묵 ~20G)은 원재료 판매가(산딸기 x5=50G, 도토리 x5=40G) 대비 손해가 발생한다. `[RISK]` 등록. FIX-083으로 processing-system.md 레시피 추가 시 가격 역전 해소가 필요하다.

---

## BAL-015 채집 경제 밸런스

### 핵심 발견: 채집 수입 비중 초과

현행 판매가 기준 채집 수입 비중이 설계 목표(15~20%)를 크게 초과한다:

| 시기 | 채집 일일 수입 (수급 적용) | 비중 | 목표 |
|------|--------------------------|------|------|
| 초기 (Zone D Lv.1 봄) | ~220G | ~45% | 15~20% |
| 중기 (전 구역 Lv.5 가을) | ~536G | ~38% | |
| 후기 (Lv.10 가을) | ~655G | ~17% | ✓ |

**근본 원인**:
1. 채집물 판매가가 상대적으로 높음 (Common 평균 10G)
2. 에너지 소모 0 (맨손) → 기회비용 無
3. 초기 작물 수입이 낮아 상대 비중이 높아지는 구조적 문제

### 조정안 D 추천 (미확정)

전체 채집물 판매가를 현재의 **40%로 하향** 시:

| 희귀도 | 현재 평균가 | 조정 후 |
|--------|-----------|---------|
| Common | 8~15G | 3~6G |
| Uncommon | 18~30G | 8~12G |
| Rare | 45~100G | 18~40G |
| Legendary | 150~300G | 60~120G |

조정 후 중기 채집 비중 ~20%로 설계 목표 달성. **BAL-016으로 확정 필요** (gathering-system.md 섹션 3 수정 포함).

---

## Reviewer 수정 사항

| ID | 심각도 | 이슈 | 수정 |
|----|--------|------|------|
| C-1 | 🔴 | sound-design.md에 채집 SFX 섹션 누락 | 섹션 2.13 채집 11종 신규 추가, Phase 3 합계 56→67 수정 |
| C-2 | 🔴 | gathering-economy.md 섹션 1.3 판매가 직접 기재 (PATTERN-006) | 분석 입력값 노트 추가 + canonical 출처 명시 |
| W-2 | ⚠️ | gathering-economy.md 섹션 8.3 낚시 수입 95G (실제 187G) 오류 | 187G로 수정, 비중 재계산 (36%→26%) |
| W-4 | ⚠️ | gathering-economy.md 섹션 8이 확정값처럼 보임 | `[OPEN]` 미확정 표기 추가 |
| W-5 | ⚠️ | gathering-economy.md 목축 수입 참조 누락 | livestock-economy.md 참조 추가 |

---

## 세션 후 활성 TODO

| ID | Priority | 상태 |
|----|----------|------|
| BAL-016 | 3 | **신규** (채집 아이템 판매가 40% 하향 확정 — gathering-system.md 수정) |
| ARC-032 | 2 | 잔여 (채집 MCP 태스크 독립 문서화) |
| FIX-083 | 2 | **신규** (processing-system.md 채집물 가공 레시피 공식 추가) |
| DES-017 | 2 | 잔여 (채집 낫 업그레이드 경로 상세 설계) |
| ARC-033 | 1 | 잔여 (채집 SO 에셋 data-pipeline.md 반영) |
| CON-013 | 1 | 잔여 (채집 퀘스트/업적 콘텐츠) |
| PATTERN-009/010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 CON-012 + BAL-015 + FIX-081/082 태스크에 따라 자율적으로 작성했습니다.*
