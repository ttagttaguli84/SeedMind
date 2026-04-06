# Devlog #016 — 튜토리얼/온보딩 시스템 (DES-006)

> 2026-04-06 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### DES-006: 튜토리얼/온보딩 시스템 설계

Designer + Architect 병렬 실행 → Reviewer CRITICAL 3건·WARNING 1건 발견 후 전부 수정.

**신규 문서**:
1. `docs/systems/tutorial-system.md` — 튜토리얼 게임 디자인 canonical 문서 (12단계 플로우, UI 가이드 요소, NPC 연계, 컨텍스트 힌트 시스템)
2. `docs/systems/tutorial-architecture.md` — 기술 아키텍처 (Part I + Part II, ~550줄)

**수정된 문서**:
- `docs/design.md` — Cross-references에 tutorial-system.md 추가
- `docs/architecture.md` — Cross-references에 tutorial-architecture.md 추가 + Tutorial/ 폴더 구조 반영
- `docs/systems/project-structure.md` — Scripts/Tutorial/ 폴더, SeedMind.Tutorial 네임스페이스, 의존성 매트릭스 추가
- `docs/pipeline/data-pipeline.md` — TutorialSequenceData/TutorialStepData/ContextHintData/TutorialSaveData SO 에셋 추가, 총 에셋 수 ~87 → ~120
- `docs/systems/tutorial-system.md` — 섹션 4.4 세이브 파라미터 구조 → architecture canonical 참조로 교체 (리뷰 수정)
- `docs/systems/tutorial-architecture.md` — 단계 수 10→12 수정, NullRef 버그 수정, cross-ref "추가 필요" → "반영 완료" (리뷰 수정)

---

## 핵심 설계 내용

### 온보딩 철학 — "Do, Don't Tell"

6가지 원칙:
1. **컨텍스트 기반 안내**: 경작 타일 근처에서 경작 방법을 안내 (상황과 무관한 팝업 금지)
2. **NPC를 통한 전달**: 시스템 UI 메시지 대신 NPC 대사로 가이드
3. **단계적 복잡도 노출**: 핵심 루프 1회전(경작→파종→물주기→성장→수확→판매→재구매)으로 범위 한정
4. **실패 허용**: 튜토리얼 중 작물 고사 시 보상 지급, 에너지 기절 면제
5. **스킵 자유**: 어느 단계에서든 스킵 가능, 스킵 시 초기 아이템 일괄 지급
6. **복기 지원**: 설정 메뉴에서 단계별 재시청

### 메인 튜토리얼 플로우 (12단계)

| 단계 | 내용 | 가이드 NPC |
|------|------|-----------|
| S01 | 이동·인터랙션 기초 | — |
| S02 | 첫 타일 경작 (호미 사용) | 하나(하이라이트) |
| S03 | 씨앗 구매 (하나 상점 방문) | 하나 |
| S04 | 씨앗 심기 | 하나 |
| S05 | 인벤토리 기초 | — |
| S06 | 물주기 (물뿌리개 사용) | — |
| S07 | 성장 관찰 (작물 상태 확인) | — |
| S08 | 수확 (낫 사용) | — |
| S09 | 수확물 판매 (하나 상점) | 하나 |
| S10 | 비료 구매 및 사용 | 하나 |
| S11 | 레벨업 확인 및 창고 시설 안내 | 하나 |
| S12 | 튜토리얼 완료 — 자유 플레이 전환 | 하나 |

### 튜토리얼 보호 규칙
- 튜토리얼 중 에너지 기절 없음 (에너지 0 시 경고만)
- 작물 고사 발생 시 씨앗 1개 보상 지급
- 계절 전환 유예 (Day 35 상한)

### 컨텍스트 힌트 시스템
튜토리얼 이후에도 작동하는 상황별 자동 팁 — 경작/경제/시설/도구 4카테고리 × 총 16종 힌트 정의. 표시 규칙: 60초 간격, 동시 1개, 5초 표시.

### 기술 아키텍처 주요 결정

- **기존 시스템 무수정 원칙**: Tutorial 모듈은 FarmEvents/BuildingEvents/NPCEvents/ToolUpgradeEvents를 구독만 하며, 기존 시스템은 Tutorial의 존재를 모름 (단방향 의존)
- **SO 2단 구조**: TutorialSequenceData → TutorialStepData (시퀀스와 단계 분리)
- **PATTERN-005 준수**: TutorialSaveData JSON 스키마(섹션 7.1) ↔ C# 클래스(섹션 7.2) 필드 5개 동기화
- **TutorialManager + ContextHintSystem 분리**: 순차 시퀀스 관리와 반복 힌트를 독립 처리
- **네임스페이스**: SeedMind.Tutorial, SeedMind.Tutorial.Data

---

## 리뷰 결과

**CRITICAL 3건 (수정 완료)**:
1. [C-1] tutorial-architecture.md `SkipSequence()`에서 null 할당 후 이벤트 발행 → NullReferenceException — null 할당 전 sequenceId 지역 변수 보관으로 수정
2. [C-2] tutorial-system.md 섹션 4.4 세이브 파라미터 5개 필드가 tutorial-architecture.md C# 클래스와 완전히 다른 구조 → architecture canonical 참조로 교체
3. [C-3] tutorial-architecture.md 전반에서 10단계 기준 기재(섹션 1.2, MCP-3, UI 예시) vs canonical 12단계 불일치 → 12단계로 전수 수정

**WARNING 1건 (수정 완료)**:
1. [W-1] tutorial-architecture.md Cross-references에 "추가 필요" 메모 잔존 (이미 완료된 항목) → "반영 완료"로 수정

---

## 의사결정 기록

1. **12단계 플로우 선택**: 10단계로 초안 설계 후 비료 사용(S10)과 시설 안내(S11)를 추가하여 12단계로 확정. 이유: 비료 시스템과 시설 해금이 경제 진행의 첫 분기점이므로 튜토리얼에서 한 번 경험시키는 것이 이탈율 감소에 효과적.

2. **NPC 가이드 전략**: 하나(시장 상인)가 튜토리얼 전반을 담당. 철수(대장간)·목이(목공소)는 해금 이후 컨텍스트 힌트로만 등장. 이유: 튜토리얼 초반에 NPC가 너무 많으면 인지 부하 증가.

3. **단방향 의존성 원칙**: Tutorial 모듈이 기존 시스템에 의존하되, 기존 시스템은 Tutorial을 모르게 설계. 이유: Phase 2 Unity 구현 시 Tutorial 모듈을 독립적으로 추가/제거 가능하며, 기존 시스템 테스트에 Tutorial이 영향을 주지 않음.

4. **세이브 데이터 구조**: `completedSequenceIds[]` + `completedStepIds[]` (완료 기록) + `activeSequenceId/StepIndex` (진행 중 상태) + `contextHintCooldowns{}` (힌트 쿨다운) 5필드로 확정. 향후 메인 튜토리얼 외 시스템 튜토리얼 추가 시에도 completedSequenceIds 배열로 확장 가능.

---

## 미결 사항 ([OPEN])

- 계절 전환 유예 Day 35 상한의 어뷰징 가능성 검토 필요
- 비 오는 날 S06(물주기 단계) 처리 방식 미결정
- 칭호 시스템 본격 도입 여부
- NPC 대사 큐잉 시스템과 npcs.md 통합 방식
- NPC 없는 컨텍스트 힌트의 화자 처리 (시스템 메시지? 무명 화자?)

---

## 후속 작업

- `ARC-010`: 튜토리얼 MCP 태스크 시퀀스 독립 문서화
- `ARC-011`: 세이브/로드 시스템 기술 아키텍처
- `ARC-007/008/009`: 시설·도구·NPC MCP 태스크 시퀀스 (Phase 2 전환 준비)
- `DES-008`: 세이브/로드 UX 설계 (ARC-011과 병행)

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
