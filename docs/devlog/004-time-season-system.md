# Devlog #004 — 시간/계절 시스템 상세 설계

> 2026-04-06 | Phase 1 | 작성: Claude Code (Opus)

---

## 오늘 한 일

### DES-003: 시간/계절 시스템 게임 디자인 (`docs/systems/time-season.md`)

- **하루 흐름**: 5개 시간대(Dawn/Morning/Afternoon/Evening/Night) 구분, 시간대별 게임플레이 영향, 시간 가속/일시정지 규칙, 하루 종료 프로세스(수면 vs 기절), 날짜 전환 배치 처리 순서(11단계), NPC/상점 스케줄, 요일 시스템
- **계절 시스템**: 계절별 특성 표, 환경 변화(조명/색감/오브젝트/사운드), 계절 전환 메커니즘(10단계 처리 순서), 사전 경고 시스템(5단계)
- **날씨 시스템**: 7종 날씨(Clear/Cloudy/Rain/HeavyRain/Storm/Snow/Blizzard), 계절별 확률 테이블(canonical), 결정 알고리즘(연속 제한), 게임플레이 영향 4종, 예보 메커니즘 3가지
- **연간 이벤트**: 계절별 축제 4종(봄 씨앗/여름 불꽃/가을 수확/겨울 별빛)
- **튜닝 파라미터**: 시간/계절/날씨/축제 4카테고리 30+ 파라미터

### 시간/계절 기술 아키텍처 (`docs/systems/time-season-architecture.md`)

- **클래스 설계**: TimeManager 확장, WeatherSystem, WeatherData SO, SeasonData SO, DayPhaseVisual, FestivalManager, FestivalData SO
- **시간 진행 로직**: Time.deltaTime 기반 Update 루프, 정수 시간 경계 이벤트
- **이벤트 처리 순서**: 우선순위 기반 콜백 디스패처. OnDayChanged 순서: WeatherSystem(0) → GrowthSystem(10) → FarmGrid(20) → FestivalManager(30) → EconomyManager(40) → SaveManager(50) → HUD(90)
- **날씨 아키텍처**: Weighted Random with Correction 알고리즘, 시드 기반 결정론적 난수
- **저장/로드**: TimeSaveData, WeatherSaveData 직렬화 구조
- **MCP 구현 계획**: Phase A~E, 19단계 태스크 시퀀스
- **성능**: OnDayChanged 프레임 스파이크 대응, 조명 보간 비용, 계절 전환 GC 대응

### 리뷰 및 수정

Reviewer 에이전트가 **CRITICAL 2건, WARNING 2건, INFO 1건** 발견 및 전부 수정:

**CRITICAL**:
1. farming-system.md 날씨 영문 키 `Sunny/Rainy` → canonical `Clear/Rain` 등 7종으로 통일
2. farming-system.md의 존재하지 않는 파일 참조 `time-weather-system.md` → `time-season.md` 수정

**WARNING**:
1. time-season-architecture.md WeatherData SO 필드가 5종 → canonical 7종으로 수정
2. time-season-architecture.md FestivalManager 축제 3개 → canonical 4개로 수정

**INFO**:
1. WeatherSystem.IsRaining 조건을 Rain/HeavyRain/Storm 3종으로 수정

---

## 의사결정 기록

1. **날씨 7종 체계**: 기존 farming-system.md의 5종에서 HeavyRain, Blizzard를 추가하여 7종으로 확장. 계절별 날씨 다양성 확보 및 겨울 고유 날씨(Blizzard) 필요.
2. **이벤트 처리 우선순위 시스템**: OnDayChanged 구독자 간 레이스 컨디션 방지를 위해 우선순위 기반 디스패처 도입. 날씨 → 성장 → 농장 → 축제 → 경제 → 저장 순서로 처리.
3. **요일 시스템 도입**: 7일 주기로 NPC/상점 스케줄 변화를 주어 게임 리듬에 변화 부여.
4. **날씨 예보 메커니즘**: TV, 마을 게시판, 기상 달력(시설) 3단계로 정보 접근성을 점진적으로 개선.

---

## PATTERN 관찰

PATTERN-001 재발: 이번에도 아키텍처 문서가 디자인 canonical 수치를 독립 복사하여 불일치 3건 발생. self-improve 대응 필요성 증가.

---

## 다음 단계

- DES-004: 경제 시스템 상세 (Priority 4)
- ARC-002: MCP 작업 계획 — 기본 씬 구성 (Priority 4)
- ARC-003: MCP 작업 계획 — 농장 그리드 생성 (Priority 4)

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
