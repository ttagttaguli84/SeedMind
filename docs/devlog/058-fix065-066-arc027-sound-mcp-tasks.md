# Devlog #058 — FIX-065/066 + ARC-027: 사운드 시스템 후속 수정 + MCP 태스크 문서화

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

AUD-001(사운드 시스템 설계) 직후 리뷰어가 잔류시킨 3개 항목을 처리하고, 사운드 MCP 태스크를 독립 문서로 분리했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/systems/sound-design.md` | FIX-065: 섹션 3.5에 SFX 풀 크기 테이블(poolSize=16) 추가 |
| `docs/systems/sound-design.md` | FIX-066: 섹션 1.3에 TitleScreen/GameOver BGM 2트랙 추가; 섹션 1.4 우선순위 스택 5→6단계로 확장 |
| `docs/systems/sound-architecture.md` | WARNING-001: 섹션 3.2 풀 크기 참조를 `섹션 3.5`로 구체화 |
| `docs/systems/sound-architecture.md` | WARNING-002: BGMTrack enum `// 시스템 BGM` 주석에 `→ see 섹션 1.3` 참조 + TitleScreen/GameOver 인라인 주석 추가 |
| `docs/mcp/sound-tasks.md` | ARC-027: 사운드 MCP 태스크 독립 문서 신규 생성 |
| `TODO.md` | FIX-065/066, ARC-027 DONE 처리 |

---

## FIX-065 — SFX 풀 크기 canonical 등록

sound-architecture.md 섹션 3.2가 `(-> see docs/systems/sound-design.md)`로만 참조하고 있었으나, sound-design.md에 실제 poolSize 수치가 없었다. 섹션 3.5 폴리포니 테이블 하단에 다음 근거로 **poolSize = 16**을 canonical 확정했다.

| 구분 | 수량 |
|------|------|
| SFX_Player | 4 |
| SFX_World | 8 |
| SFX_Jingle | 2 |
| 버퍼 | +2 |
| **총 poolSize** | **16** |

BGM(Dual-Source 2개), Ambient(1개), UI(1개)는 SFX 풀과 별도.

---

## FIX-066 — TitleScreen/GameOver BGM 정의

sound-architecture.md BGMTrack enum에 `TitleScreen`, `GameOver`가 있었으나 sound-design.md 섹션 1(BGM 설계)에 해당 트랙 정의가 없었다. 섹션 1.3(특수 상황 BGM)에 두 트랙을 추가했다:

| ID | 트랙명 | BPM | 분위기 |
|----|--------|-----|--------|
| `bgm_title_screen` | 씨앗의 노래 | 90~100 | 따뜻한 기대감, 편안한 시작 |
| `bgm_game_over` | 한 해의 끝 | 70~80 | 잔잔한 성찰, 다음 도전을 향한 여운 |

리뷰어가 CRITICAL-001로 식별: 섹션 1.4 BGM 우선순위 스택에 TitleScreen/GameOver가 없어 sound-architecture.md 섹션 6.2와 불일치. 우선순위 스택을 다음과 같이 6단계로 확장했다:

1. 시스템 BGM (TitleScreen, GameOver) — 강제 재생, 최우선
2. 축제 BGM
3. 실내 BGM
4. 날씨 BGM (Storm > Blizzard > Rain)
5. 밤 BGM
6. 계절 BGM (기본)

---

## ARC-027 — 사운드 MCP 태스크 독립 문서화

`docs/mcp/sound-tasks.md` 신규 생성. sound-architecture.md Part II(섹션 10)의 5단계 개요를 6단계 상세 시퀀스로 확장.

| 단계 | 내용 | 예상 MCP 호출 |
|------|------|--------------|
| S-1 | AudioMixer 에셋 생성 (MainMixer + 5 Group + 5 Exposed Parameter) | ~18회 |
| S-2 | SoundManager GameObject 구성 (스크립트, dual-source BGM, SFX 풀) | ~28회 |
| S-3 | ScriptableObject 에셋 생성 (SoundRegistry, BGMRegistry, SoundData SO) | ~45회 |
| S-4 | SoundEventBridge 연결 및 이벤트 구독 검증 | ~12회 |
| S-5 | BGMScheduler 통합 테스트 (계절/날씨/TitleScreen/GameOver 전환 검증) | ~10회 |
| S-6 | MVP 사운드 에셋 임포트 및 AudioClip 연결 | ~35회 |
| **합계** | | **~148회** |

모든 수치(poolSize, dB 값, 에셋 수량 등)는 PATTERN-006 준수 — canonical 문서 참조만 표기.

---

## 리뷰어 검증 결과

| ID | 심각도 | 수정 결과 |
|----|--------|-----------|
| CRITICAL-001 | 🔴 | sound-design.md 섹션 1.4 우선순위 스택 6단계로 확장 |
| WARNING-001 | 🟡 | sound-architecture.md 섹션 3.2 참조 `섹션 3.5`로 구체화 |
| WARNING-002 | 🟡 | BGMTrack enum `// 시스템 BGM` 주석 참조 + 인라인 주석 추가 |

---

## 세션 후 TODO 상태

| ID | Priority | 상태 |
|----|----------|------|
| FIX-065 | 2 | ✅ DONE |
| FIX-066 | 2 | ✅ DONE |
| ARC-027 | 2 | ✅ DONE |
| CON-009 | 2 | 잔여 (치즈 공방 레시피) |
| FIX-054 | 2 | 잔여 (생선 가공 레시피 이전) |
| FIX-056~064 | 2~3 | 잔여 (BAL-005 후속 + 낚시 보완) |
| PATTERN-009, 010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 FIX-065, FIX-066, ARC-027 태스크에 따라 자율적으로 작성했습니다.*
