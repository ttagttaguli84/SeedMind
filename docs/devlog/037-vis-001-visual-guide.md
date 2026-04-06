# Devlog #037 — VIS-001: 비주얼 가이드 & 비주얼 아키텍처 설계

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

VIS-001 비주얼 가이드 태스크를 완료했다. 게임의 아트 방향성·색상 팔레트·조명·캐릭터 스타일을 정의하는 디자인 문서와, Unity 6 구현 관점의 기술 아키텍처 문서를 병렬로 작성했다.

### 생성/수정된 파일

| 파일 | 내용 |
|------|------|
| `docs/systems/visual-guide.md` | 비주얼 가이드 신규 (VIS-001 — 색상 팔레트, 조명, 캐릭터/오브젝트 스타일, UI 비주얼, 성장 단계 시각화) |
| `docs/systems/visual-architecture.md` | 비주얼 시스템 기술 아키텍처 신규 (LightingManager, PaletteData, CropVisual, WeatherVisualController) |
| `TODO.md` | VIS-001 완료 처리 |

---

## 주요 결정 사항

### 아트 방향성
- **로우폴리 스타일라이즈드 3D** 채택. 텍스처리스(버텍스 컬러/단색 머티리얼) 기본.
- **Flat Shading**: 커스텀 셰이더 없이 URP/Lit Smoothness=0 + Face Normal 방식으로 구현.
- **Bloom 비활성화**: 로우폴리 스타일과 불일치. 열기/발광 표현은 Emissive 색상 강도 조정으로 대체.
- **레퍼런스**: Islanders/Townscaper(기하학) + A Short Hike/Ooblets(색감) + Stardew Valley(농장 정서)

### 색상 팔레트 (계절별 6색)
- 봄: 연두(`#A8D5A2`), 벚꽃 핑크, 하늘 블루 등
- 여름: 짙은 녹색(`#4CAF50`), 해바라기 노랑, 토마토 레드 등
- 가을: 단풍 주황(`#FF8A65`), 짙은 빨강, 황금 노랑 등
- 겨울: 눈 흰색(`#ECEFF1`), 차가운 파랑, 따뜻한 주황(창문) 등

### 조명 시스템 아키텍처
- `LightingManager`가 조명 제어 단일 권한자
- `TimeManager.OnDayPhaseChanged` / `TimeManager.OnSeasonChanged` 이벤트 구독
- `SeasonLightingProfile`은 기존 `SeasonData`를 래핑하는 어댑터 구조 (데이터 중복 정의 방지)
- 계절별 `Volume Profile` 교체 + Weight 보간으로 부드러운 전환

### 날씨 비주얼
- `WeatherSystem.OnWeatherChanged` 이벤트 구독으로 파티클 전환
- 날씨 조명 감쇄는 `LightingManager.ApplyWeatherOverride(dimFactor)`에 위임 (단일 책임)
- 7종 날씨(Clear/Cloudy/Rain/HeavyRain/Storm/Snow/Blizzard) 파티클 시스템 전체 설계

---

## 리뷰 수정 사항

| 항목 | 수정 내용 |
|------|----------|
| E-01 | visual-architecture.md Giant Crop "2x2 타일" → "3x3 타일(9타일)", 참조 섹션 6 → 5.1 수정 |
| E-02 | visual-architecture.md `TimeEvents.OnXxx` → `TimeManager.OnXxx` 전수 수정 (3곳) |
| E-03 | visual-architecture.md `WeatherEvents.OnWeatherChanged` → `WeatherSystem.OnWeatherChanged` (2곳) |
| E-04 | visual-guide.md 섹션 3.3 "Bloom 약하게/강하게" → Emissive 강도 조정 표기로 수정 (Bloom 비활성화와 충돌 해소) |
| W-01 | visual-architecture.md Cross-references에서 visual-guide.md "작성 예정" 문구 제거 |
| W-02 | visual-guide.md 섹션 7 "비주얼 파라미터 수치 테이블" 신규 추가 (phaseTransitionDuration, weatherTransitionDuration 등 9개 파라미터 기본값 정의) |
| W-03 | visual-guide.md Cross-references에 visual-architecture.md 추가 |
| W-04 | visual-architecture.md Cross-references에 time-season.md 추가 |
| W-05 | visual-guide.md Cross-references에 ui-architecture.md 추가 |
| W-06 | visual-architecture.md Depth Texture 비고: "Outline [OPEN] 도입 시 활용" 표기로 수정 |
| W-07 | visual-architecture.md Cross-references Giant Crop 참조 섹션 6 → 5.1 수정 |

---

## 패턴 관찰

**E-01(Giant Crop 2x2→3x3)**는 devlog/012에서 이미 한 차례 정정된 패턴이 다시 반복됐다. 신규 아키텍처 문서 작성 시 "Giant Crop = 3x3" canonical 정보를 사전에 확인하는 절차가 필요함을 재확인. 향후 `doc-standards.md` Canonical 데이터 매핑에 Giant Crop 타일 수 출처를 명시하는 방안 검토.

---

*이 문서는 Claude Code가 VIS-001 태스크에 따라 자율적으로 작성했습니다.*
