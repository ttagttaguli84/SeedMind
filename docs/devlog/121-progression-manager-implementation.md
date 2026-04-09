# Devlog 121 — ProgressionManager MCP 구현 (BAL-002-MCP)

> 날짜: 2026-04-10 | 작성: Claude Code

## 요약

progression-tasks.md (BAL-002-MCP)의 Phase A~B를 완료했다. ProgressionManager, UnlockRegistry, MilestoneTracker 등 진행 시스템 핵심 스크립트 9개를 작성하고, SO 에셋을 생성하여 SCN_Farm 씬에 배치했다.

## 구현 내역

### 스크립트 생성 (9개)

| 파일 | 위치 | 역할 |
|------|------|------|
| `XPSource.cs` | Scripts/Level/ | XP 획득 출처 enum (12종) |
| `UnlockType.cs` | Scripts/Level/ | 해금 항목 유형 enum (6종) |
| `MilestoneConditionType.cs` | Scripts/Level/ | 마일스톤 조건 유형 enum (10종) |
| `ProgressionData.cs` | Scripts/Level/Data/ | SO — 레벨/XP/해금/마일스톤 설정 데이터 |
| `MilestoneData.cs` | Scripts/Level/Data/ | 마일스톤 정의 데이터 클래스 |
| `UnlockRegistry.cs` | Scripts/Level/ | 런타임 해금 상태 관리 |
| `MilestoneTracker.cs` | Scripts/Level/ | 마일스톤 진행 추적 |
| `ProgressionManager.cs` | Scripts/Level/ | Singleton MonoBehaviour — XP/레벨/해금 통합 |
| `LevelBarUI.cs` | Scripts/UI/ | HUD 레벨/경험치 바 표시 |

### SO 에셋 생성 및 초기값 설정

- 경로: `Assets/_Project/Data/Config/SO_ProgressionData.asset`
- maxLevel = 10, harvestExpBase = 5, harvestExpPerGrowthDay = 1.0
- expPerLevel = [80, 128, 205, 328, 524, 839, 1342, 2147, 3436] (→ see docs/balance/progression-curve.md 섹션 2.4.1)
- qualityExpBonus = [1.0, 1.2, 1.5, 2.0]
- buildingConstructExp = 30, toolUseExp = 2, facilityProcessExp = 5
- toolUpgradeExp = 90, animalCareExp = 3, animalHarvestBaseExp = 5

### 씬 배치

- `ProgressionManager` GO → `--- MANAGERS ---` 하위, ProgressionManager 컴포넌트 + SO 참조 연결
- `LevelBar` → LevelBarUI 컴포넌트 추가

## 특이사항 / 우회 처리

- **validator 오탐**: `create_script` validator가 0-파라미터 메서드 2개(RunLevelUp, RunMilestoneCheck)를 중복으로 잘못 감지 → `Write` 툴로 디스크에 직접 쓰고 `refresh_unity` 로 해결. (CLAUDE.md 기록 패턴 참조)
- **TimeManager API 오탐**: `RegisterDayChanged` → 실제명 `RegisterOnDayChanged` / `UnregisterOnDayChanged` 로 수정
- **Phase C 스킵**: `execute_code` 비활성화 규칙(CLAUDE.md)으로 런타임 검증 미실행
- **unlockTable / milestones**: 중첩 배열 데이터는 현재 빈 상태. 향후 에디터에서 직접 입력하거나 JSON Import로 추가 예정

## 다음 단계

- `crop-content-tasks.md` (CON-001-ARC, Phase C-1)

## Cross-references

- `docs/systems/progression-architecture.md` — 아키텍처 설계
- `docs/balance/progression-curve.md` — XP 수치 canonical
- `docs/mcp/progress.md` — 진행 현황
