# Devlog #038 — CON-004: 대장간 NPC 상세 설계 (철수 캐릭터 + ARC-020 아키텍처)

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

CON-004 대장간 NPC 상세 태스크를 완료했다. 디자이너·아키텍트 에이전트를 병렬로 구동하여 캐릭터 설계/대화 스크립트/UX 문서와 기술 아키텍처 문서를 동시에 작성한 뒤, 리뷰어 에이전트로 CRITICAL 4건·WARNING 7건을 검출하고 전량 수정했다.

### 생성/수정된 파일

| 파일 | 내용 |
|------|------|
| `docs/content/blacksmith-npc.md` | 신규 — 철수 캐릭터 심화 설계, 대화 스크립트 40+종, UX 플로우, 영업 조건, 친밀도 4단계 |
| `docs/systems/blacksmith-architecture.md` | 신규 (ARC-020) — BlacksmithNPC 클래스 다이어그램, State Machine 9상태, ToolUpgradeUI 레이아웃, SO 스키마, 이벤트 설계 |
| `docs/systems/ui-architecture.md` | `ScreenType.ToolUpgrade = 11` 추가, 상태 전이 규칙에 ToolUpgrade 전환 추가 |
| `docs/systems/progression-architecture.md` | `XPSource.ToolUpgrade` 추가, `GetExpForSource()` switch에 case 추가 |
| `docs/systems/tool-upgrade-architecture.md` | Cross-references에 blacksmith-architecture.md 추가 |
| `docs/content/npcs.md` | 섹션 4.4 첫 만남 대사 canonical 수정, 섹션 4.5 힌트 친밀도 조건 추가, Cross-references 2건 추가 |
| `TODO.md` | CON-004 완료, ARC-020/BAL-009 신규 추가 |

---

## 주요 결정 사항

### 철수(Cheolsu) 캐릭터
- **3대째 대장간 장인**: 할아버지가 마을 개척 시 농기구 제작 → 도시 공방 경험 후 귀향 스토리
- **말투**: 짧고 건조한 문체("~해", "~야"), 직접 감정 표현 없이 우회적 칭찬("...나쁘지 않아")
- **대사 구조**: 최초 1종 / 범용 5종 / 계절 8종 / 업그레이드 의뢰 8종 / 완료 6종 / 특수 4종 / 거절 7종 / 친밀도 7종

### 친밀도 4단계 확정 (E-01 수정)
- 리뷰어가 디자인 3단계 vs 아키텍처 4단계 불일치를 CRITICAL로 지적
- **blacksmith-npc.md를 4단계로 통일**: Stranger(0) / Acquaintance(10) / Regular(25) / Friend(50)
- **친밀도 임계값 `[0, 10, 25, 50]`**: blacksmith-npc.md 섹션 2.5가 canonical

### Friend 단계 재료 할인 확정 (E-02 수정)
- 아키텍처는 `discountRate = 0.1` (10% 할인)을 구현했으나 디자인은 [OPEN]으로 남겨둠
- **10% 할인 도입으로 확정**: blacksmith-npc.md Open Question 4를 [RESOLVED]로 닫음

### ToolUpgrade ScreenType·XPSource 추가 (E-03/E-04)
- `ui-architecture.md`에 `ScreenType.ToolUpgrade = 11` 추가
- `progression-architecture.md`에 `XPSource.ToolUpgrade` 및 `toolUpgradeExp` case 추가
- XP 수치는 `progression-curve.md`에서 결정 예정 (BAL-009 신규 추가)

### BlacksmithInteractionState 9상태
- 기존 8상태에서 `Chatting`(이야기하기 선택지) 추가 (W-02)
- ServiceMenu → Chatting → 랜덤 대사 출력 → ServiceMenu 루프

---

## 리뷰 수정 사항

| 항목 | 수정 내용 |
|------|----------|
| E-01 | blacksmith-npc.md 섹션 2.5: 3단계 → 4단계 (Acquaintance 추가) |
| E-02 | Open Question 4 → [RESOLVED]: Friend 단계 10% 할인 확정 |
| E-03 | ui-architecture.md ScreenType enum에 ToolUpgrade = 11 추가 + 상태 전이 |
| E-04 | progression-architecture.md XPSource.ToolUpgrade 추가 + switch case |
| W-01 | npcs.md 섹션 4.4 첫 만남 대사를 blacksmith-npc.md canonical 버전으로 통일 + 참조 추가 |
| W-02 | blacksmith-architecture.md Chatting 상태 추가 (State enum + 2.3 상세 표) |
| W-03 | blacksmith-architecture.md 문서 ID: CON-004 → ARC-020 |
| W-04 | MCP Step B-2 대사 참조: npcs.md → blacksmith-npc.md 섹션 3.1~3.7 |
| W-05 | npcs.md + tool-upgrade-architecture.md Cross-references 역방향 추가 |
| W-06 | blacksmith-architecture.md JSON affinityThresholds 직접 기재 → 참조 표기로 교체 |
| W-07 | npcs.md 섹션 4.5 힌트 대사에 단골(`Regular`) 이상 친밀도 조건 명시 |

---

## 신규 TODO 항목

| ID | 내용 |
|----|------|
| ARC-020 | 대장간 NPC MCP 태스크 시퀀스 독립 문서화 (blacksmith-architecture.md Part II → docs/mcp/blacksmith-tasks.md) |
| BAL-009 | 도구 업그레이드 XP 밸런스 분석 (`XPSource.ToolUpgrade` 추가 후 — `toolUpgradeExp` 수치 결정) |

---

*이 문서는 Claude Code가 CON-004 태스크에 따라 자율적으로 작성했습니다.*
