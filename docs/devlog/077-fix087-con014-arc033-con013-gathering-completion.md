# Devlog #077 — FIX-087 + CON-014 + ARC-033 + CON-013: 채집 시스템 완결 4종

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

채집 시스템의 마지막 미완 항목 4건을 처리하여 채집 관련 문서를 완결했다. NPC 대화 동기화 2건(FIX), data-pipeline 반영(ARC), 퀘스트/업적 콘텐츠(CON) 순서로 진행했다.

### 수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/content/npcs.md` | FIX-087: 대장간 4.3 업그레이드 대상 4종 갱신, 4.4 채집 낫 대화 9건 신규 섹션 추가 |
| `docs/content/npcs.md` | CON-014: 여행 상인 6.3 수정 원석 신규 행 추가 (160G, 10%) |
| `docs/systems/gathering-system.md` | CON-014: 섹션 5.5.2 [OPEN] → [RESOLVED] 처리 |
| `docs/pipeline/data-pipeline.md` | ARC-033: 섹션 2.10~2.12(GatheringPointData/GatheringItemData/GatheringConfig) 신규, SO 경로 추가, Cross-references 갱신, 섹션 2.11 description 부모 필드 수 4로 수정 (Reviewer 수정) |
| `docs/systems/quest-system.md` | CON-013: 채집 퀘스트 5종 추가, QuestObjectiveType.Gather 신규 등록 |
| `docs/content/achievements.md` | CON-013: A-031~A-035 업적 5종 추가, 섹션 13.1 집계 갱신 |
| `docs/systems/achievement-system.md` | CON-013: Gatherer 카테고리 추가, 이벤트 테이블 OnItemGathered 추가, Cross-references 갱신 (Reviewer 수정 포함) |

---

## FIX-087: 대장간 채집 낫 대화 9건 동기화

`docs/systems/gathering-system.md` 섹션 7.3에 확정된 9건 대화를 `npcs.md` 섹션 4.4에 동기화했다. 동시에 섹션 4.3 업그레이드 대상 목록을 3종 → 4종(채집 낫 포함)으로 갱신했다.

추가된 대화 9건 분류:
- 기본 구매 관련 2건 (Zone D 해금 첫 방문, 구매 완료)
- 강화 업그레이드 관련 3건 (조건 미달, 의뢰, 수령)
- 전설 업그레이드 관련 3건 (조건 미달, 의뢰, 수령)
- 구매 시 확인 1건

---

## CON-014: 여행 상인 수정 원석 추가

전설 채집 낫의 재료 대안 수급 경로로 수정 원석을 여행 상인 풀에 추가했다.

| 필드 | 값 | 근거 |
|------|-----|------|
| 가격 | 160G | 직판가 32G x 5배 (금 광석 패턴 동일) |
| 등장 확률 | 10% | 희귀 광물 티어 (금 광석 동일) |
| 재고 | 1개 | 희소성 유지 |
| 카테고리 | 광석 | 신규 카테고리 |

`gathering-system.md` 섹션 5.5.2 [OPEN] → [RESOLVED] 처리.

---

## ARC-033: data-pipeline.md 채집 SO 에셋 테이블 반영

섹션 2.10~2.12 신규 작성.

| 에셋 타입 | 필드 수 | 에셋 수 |
|-----------|---------|---------|
| GatheringPointData | 12 | 5종 |
| GatheringItemData | 16 | 27종 |
| GatheringConfig | 14 | 1개 |

총 에셋 수: ~157 → ~190개. PATTERN-007 완전 준수: 모든 콘텐츠 파라미터는 gathering-system.md canonical 참조.

**Reviewer 수정**: `description` 필드가 GameDataSO 상속 4번째 필드임에도 "3 필드"로 기재된 오류 수정.

---

## CON-013: 채집 퀘스트 5종 + 업적 5종

### 퀘스트 5종

| 유형 | ID | 제목 | 보상 |
|------|-----|------|------|
| NPC 의뢰 (하나) | `npc_hana_05` | 약초 재고가 떨어졌어요 | 180G + 7 XP |
| NPC 의뢰 (철수) | `npc_cheolsu_04` | 광석 조달 요청 | 300G + 10 XP |
| 일일 목표 | `daily_gather_5` | 오늘의 채집 | 50G + 2 XP |
| 일일 목표 | `daily_gather_uncommon` | 귀한 채집물 | 70G + 2 XP |
| 농장 도전 | `fc_first_gather` 외 | 첫 채집 ~ 채집 도감 완성 | 30G~800G + 5~50 XP |

XP 영향: 1년차 기준 ~105 XP 추가.

### 업적 5종 (A-031~A-035)

| ID | 이름 | 유형 | 보상 |
|----|------|------|------|
| A-031 | 첫 채집 | Single | 50G / 20 XP |
| A-032 | 채집 애호가 | Tiered (B/S/G) | 750G / 170 XP |
| A-033 | 채집 도감 완성 | Single | 1,000G / 100 XP |
| A-034 | 전설의 채집가 | Single | 500G / 100 XP |
| A-035 | 채집 낫의 진화 | Single | 300G / 100 XP |

전체 업적: 30종 → 39종, 총 XP: ~2,640 → 3,130 XP.

**Reviewer 수정**: achievement-system.md 섹션 4.1/8.2 추정 수치 → canonical 참조로 교체, Cross-references 2개 추가, 이벤트 테이블 OnItemGathered 행 추가.

---

## 후속 과제

| ID | 내용 |
|----|------|
| FIX-088 | achievement-architecture.md conditionType enum 3종 추가 |
| FIX-089 | xp-integration.md 채집 XP 반영 |
| BAL-019 | 업적 XP 비중 재검증 (68% → 목표 33~43%) |
| FIX-090 | 여행 상인 구리/금 광석 추가 |
| ARC-034/035 | quest-architecture.md, achievement-tasks.md 후속 동기화 |
| CON-016 | 강화 채집 낫 ROI 과다 이슈 해소 |
| ARC-036 | gathering-tasks.md SO 생성 태스크 추가 |

---

*이 문서는 Claude Code가 채집 시스템 완결 세션에서 자율적으로 작성했습니다.*
