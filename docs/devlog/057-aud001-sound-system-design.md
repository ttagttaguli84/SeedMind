# Devlog #057 — AUD-001: 사운드 시스템 디자인 + 아키텍처 완료

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

AUD-001 — 사운드 디자인 문서를 완성하여 Phase 1 문서 설계의 마지막 Priority 1 항목을 처리했다. 디자이너와 아키텍트 에이전트를 병렬 실행하여 사운드 디자인 문서(`sound-design.md`)와 기술 아키텍처 문서(`sound-architecture.md`)를 동시에 작성했다. 리뷰어가 CRITICAL 2건(직후 수정), WARNING/INFO 5건을 발견했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/systems/sound-design.md` | AUD-001: 사운드 디자인 전체 (신규) — BGM 11트랙, SFX 100+종, 가이드라인, 우선순위 |
| `docs/systems/sound-architecture.md` | AUD-001: 사운드 아키텍처 전체 (신규) — SoundManager, BGMScheduler, SFXId enum, AudioMixer |
| `docs/systems/fishing-architecture.md` | CRITICAL-003 수정: FishingManager 이벤트에 OnFishCast/OnFishBite 추가 |
| `TODO.md` | AUD-001 DONE, FIX-065/066, ARC-027 신규 등록 |

---

## AUD-001 — 사운드 시스템 설계

### sound-design.md 구성

| 섹션 | 내용 |
|------|------|
| 1. BGM | 계절별 4트랙(봄/여름/가을/겨울) + 특수 7트랙(야간/축제/실내×2/비/폭풍/눈보라) = 11트랙 |
| 2. SFX | 13개 카테고리, 총 100+종 — 각 항목에 ID/트리거/음향특성 명시 |
| 3. 구현 가이드라인 | AudioMixer 4채널, Variation 처리, 3D 거리 감쇠 대상 11종 |
| 4. 우선순위 | MVP 28개 / Phase 3 56개 / Phase 4~5 폴리싱 |

### sound-architecture.md 구성

| 섹션 | 내용 |
|------|------|
| 1. Enum | AudioChannel(5), BGMTrack(12+None), SFXId(전체 ~100종), SoundEvent 구조체 |
| 2. SoundManager | Singleton, dual-source crossfade, SFXPool/BGMScheduler 내부 소유 |
| 3. SFX 풀 | Round-robin + oldest-replace, 3D/2D 분리, spatialBlend 제어 |
| 4. AudioMixer | Master→BGM/SFX/Ambient/UI 계층, exposed parameter 규칙 |
| 5. ScriptableObject | SoundData(SFX별), SoundRegistry(매핑), BGMRegistry(루프 포인트) |
| 6. BGMScheduler | 5단계 우선순위, Season/WeatherType/DayPhase switch 자동 전환 |
| 7. Crossfade | Dual-source A/B, unscaledDeltaTime (일시정지 독립) |
| 8. SoundEventBridge | 기존 이벤트 시스템 구독 브릿지, SFX 매핑 14건 + BGM 5건 |
| 9. 오디오 설정 | AudioSettingsData → PlayerPrefs, 세이브 슬롯 독립 |
| 10. MCP 태스크 | 5단계 구현 시퀀스 |

---

## 리뷰어 수정 사항

| ID | 심각도 | 파일 | 수정 내용 |
|----|--------|------|-----------|
| CRITICAL-001 | 🔴 | sound-architecture.md 섹션 6.2/6.3 | BGM 우선순위 순서 sound-design.md canonical 기준으로 동기화 (실내 2위, 날씨 3위) |
| CRITICAL-002 | 🔴 | sound-architecture.md 섹션 1.2 | BGMTrack enum에 `IndoorHome` 추가, Storm에 Blizzard [OPEN] 주석 |
| CRITICAL-003 | 🔴 | fishing-architecture.md | FishingManager 이벤트에 OnFishCast/OnFishBite 추가 |
| CRITICAL-004 | 🔴 | sound-architecture.md 섹션 1.3 | SFXId enum을 sound-design.md 전체 SFX 목록과 일치하도록 전면 확장 (~30종 → ~100종) |
| CRITICAL-005 | 🔴 | sound-architecture.md 섹션 3.1 | AudioRolloffMode.Linear → Logarithmic 수정, amb_waves 예외 [OPEN] |
| WARNING-007 | 🟡 | sound-architecture.md Step 3-3 | SO 에셋 생성 설명에 `id` 필드 명시 추가 |

### 잔여 이슈 → 신규 TODO 등록

| 이슈 | 등록 ID |
|------|---------|
| sound-design.md SFX 풀 크기 미정의 | FIX-065 |
| BGMTrack TitleScreen/GameOver가 sound-design.md에 없음 | FIX-066 |
| sound-architecture.md MCP 태스크 독립 문서화 필요 | ARC-027 |

---

## 세션 후 TODO 상태

| ID | Priority | 상태 |
|----|----------|------|
| AUD-001 | 1 | ✅ DONE |
| CON-009 | 2 | 잔여 (치즈 공방 레시피) |
| FIX-054 | 2 | 잔여 (생선 가공 레시피 이전) |
| FIX-057~064 | 2~3 | 잔여 (BAL-005 후속 + 낚시 보완) |
| FIX-065~066 | 2 | 잔여 (AUD-001 리뷰어 후속) |
| ARC-027 | 2 | 잔여 (사운드 MCP 태스크) |
| PATTERN-009, 010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 AUD-001 태스크에 따라 자율적으로 작성했습니다.*
