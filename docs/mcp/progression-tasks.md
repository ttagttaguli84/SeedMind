# MCP 진행 시스템 구성 태스크 시퀀스

> ProgressionManager 씬 배치, SO 에셋 생성, UI 연결을 MCP for Unity를 통해 자동화하기 위한 단계별 태스크 시퀀스  
> 작성: Claude Code (Opus) | 2026-04-06  
> 문서 ID: BAL-002-MCP

---

## Context

이 문서는 `docs/systems/progression-architecture.md` Part II의 MCP 구현 계획을 독립적인 태스크 시퀀스 문서로 정리한다. 기본 씬 구성(`docs/mcp/scene-setup-tasks.md`)이 완료된 이후에 실행한다.

---

## 사전 조건

1. SCN_Farm 씬이 `docs/mcp/scene-setup-tasks.md`에 따라 구성 완료
2. `--- MANAGERS ---` 계층이 존재 (GameManager, TimeManager, SaveManager)
3. `Canvas_HUD/LevelBar` UI 오브젝트가 존재
4. Scripts/Level/ 폴더에 필요한 스크립트 파일이 작성 완료

---

## Phase A: SO 에셋 생성 (씬 작업과 병렬 가능)

```
Step A-1: ProgressionData SO 에셋 생성
          MCP: create_scriptableobject_asset(
              type: "SeedMind.Level.Data.ProgressionData",
              path: "Assets/_Project/Data/Config/SO_ProgressionData.asset"
          )
          검증: 에셋 파일 존재 확인

Step A-2: 기본 레벨 설정 필드
          MCP: set_so_field("SO_ProgressionData", "maxLevel", 10)
               // → see docs/pipeline/data-pipeline.md 섹션 2.6
          MCP: set_so_field("SO_ProgressionData", "harvestExpBase", 5)
               // → see docs/pipeline/data-pipeline.md 섹션 2.6
          MCP: set_so_field("SO_ProgressionData", "harvestExpPerGrowthDay", 1.0)
               // → see docs/pipeline/data-pipeline.md 섹션 2.6
          검증: Inspector에서 값 확인

Step A-3: expPerLevel 배열 설정
          MCP: set_so_field("SO_ProgressionData", "expPerLevel",
               [80, 128, 205, 328, 524, 839, 1342, 2147, 3436])
               // → see docs/balance/progression-curve.md 섹션 2.4.1 for canonical values
               // baseXP=80, growthFactor=1.60 (조정 후 최종 확정값)
          검증: 배열 길이 = 9 (maxLevel - 1)

Step A-4: qualityExpBonus 배열 설정
          MCP: set_so_field("SO_ProgressionData", "qualityExpBonus",
               [1.0, 1.2, 1.5, 2.0])
               // → see docs/pipeline/data-pipeline.md 섹션 2.6
          검증: 배열 길이 = 4 (Normal, Silver, Gold, Iridium)

Step A-5: 비수확 XP 소스 설정
          MCP: set_so_field("SO_ProgressionData", "buildingConstructExp", 30)
               // → see docs/balance/progression-curve.md 섹션 1.2.4 (물탱크 기준값)
               // 실제 시설별 XP는 unlockTable JSON에서 per-building으로 오버라이드
          MCP: set_so_field("SO_ProgressionData", "toolUseExp", 2)
               // → see docs/balance/progression-curve.md 섹션 1.2.3 (호미질 2 XP 기준)
          MCP: set_so_field("SO_ProgressionData", "facilityProcessExp", 5)
               // → see docs/balance/progression-curve.md 섹션 1.2.5 (가공품 완성 5 XP)
          검증: 각 값 > 0

Step A-6: unlockTable 설정 (JSON Import 방식)
          → 해금 테이블은 중첩 배열 객체이므로 JSON Import 파이프라인 사용
          → JSON 파일 경로: Assets/_Project/Data/Config/progression_unlock_table.json
          → 데이터 (→ see docs/design.md 섹션 4.2, 4.5, 4.6):
            Level 1: Crop/potato, Crop/carrot
            Level 2: Crop/tomato
            Level 3: Crop/corn, Facility/water_tank, Fertilizer/advanced
            Level 4: Crop/strawberry, Facility/storage, Fertilizer/speed
            Level 5: Crop/pumpkin, Crop/sunflower, Facility/greenhouse
            Level 6: Fertilizer/organic
            Level 7: Crop/watermelon, Facility/processor
          검증: unlockTable 배열 길이 = 7, 각 레벨별 items 확인

Step A-7: milestones 초기 설정 (JSON Import 방식)
          → 마일스톤 데이터도 중첩 객체이므로 JSON Import 사용
          → 수치 (→ see docs/balance/progression-curve.md)
          검증: milestones 배열 비어있지 않음
```

[RISK] Step A-3, A-4: MCP에서 배열 필드 설정이 미지원일 경우, Step A-6과 동일하게 JSON Import로 우회.

---

## Phase B: 씬 오브젝트 배치 (Phase A와 병렬 가능)

```
Step B-1: SCN_Farm 씬 열기
          MCP: open_scene("Assets/_Project/Scenes/Main/SCN_Farm.unity")

Step B-2: ProgressionManager GameObject 생성
          MCP: create_gameobject(
              name: "ProgressionManager",
              parent: "--- MANAGERS ---"
          )
          → Transform: position (0, 0, 0), rotation (0, 0, 0), scale (1, 1, 1)
          검증: Hierarchy에서 --- MANAGERS --- 하위에 표시

Step B-3: ProgressionManager 컴포넌트 추가
          MCP: add_component(
              gameObject: "ProgressionManager",
              componentType: "SeedMind.Level.ProgressionManager"
          )
          검증: Inspector에서 컴포넌트 표시

Step B-4: ProgressionData SO 참조 연결 (Phase A Step A-1 완료 후)
          MCP: set_component_property(
              gameObject: "ProgressionManager",
              component: "SeedMind.Level.ProgressionManager",
              property: "_progressionData",
              value: "Assets/_Project/Data/Config/SO_ProgressionData.asset"
          )
          검증: Inspector에서 SO 참조 연결 확인

Step B-5: LevelBarUI 컴포넌트 추가
          MCP: add_component(
              gameObject: "Canvas_HUD/LevelBar",
              componentType: "SeedMind.UI.LevelBarUI"
          )
          검증: Inspector에서 컴포넌트 표시

Step B-6: 씬 저장
          MCP: save_scene()
```

---

## Phase C: 런타임 검증

```
Step C-1: Play Mode 진입
          MCP: enter_play_mode()

Step C-2: 초기 상태 검증
          MCP: execute_code("
              var pm = ProgressionManager.Instance;
              Debug.Log($\"Level: {pm.CurrentLevel}\");
              Debug.Log($\"Exp: {pm.CurrentExp}\");
              Debug.Log($\"IsMaxLevel: {pm.IsMaxLevel}\");
          ")
          기대값: Level=1, Exp=0, IsMaxLevel=false

Step C-3: XP 획득 테스트
          MCP: execute_code("
              var pm = ProgressionManager.Instance;
              pm.AddExp(50, XPSource.CropHarvest);
              Debug.Log($\"Level: {pm.CurrentLevel}, Exp: {pm.CurrentExp}\");
          ")
          기대값: Level=2, Exp=0 (레벨 1→2 필요 XP가 50이므로)
          // → see docs/balance/progression-curve.md for expected values

Step C-4: 해금 상태 검증
          MCP: execute_code("
              var pm = ProgressionManager.Instance;
              Debug.Log($\"Tomato unlocked: {pm.IsUnlocked(UnlockType.Crop, 'tomato')}\");
          ")
          기대값: true (레벨 2에서 토마토 해금)

Step C-5: Play Mode 종료
          MCP: exit_play_mode()
```

---

## 의존성 그래프

```
         Phase A (SO 에셋)              Phase B (씬 배치)
         ┌─────────────┐              ┌─────────────┐
         │ A-1: SO 생성 │              │ B-1: 씬 열기 │
         └──────┬──────┘              └──────┬──────┘
                │                            │
         ┌──────┴──────┐              ┌──────┴──────┐
         │ A-2~A-5:    │              │ B-2: GO 생성 │
         │ 필드 설정    │              └──────┬──────┘
         └──────┬──────┘                     │
                │                     ┌──────┴──────┐
         ┌──────┴──────┐              │ B-3: 컴포넌트│
         │ A-6~A-7:    │              └──────┬──────┘
         │ JSON Import │                     │
         └──────┬──────┘                     │
                │         ┌──────────────────┤
                └────────▶│ B-4: SO 참조 연결│
                          └──────┬───────────┘
                                 │
                          ┌──────┴──────┐
                          │ B-5: UI 연결 │
                          └──────┬──────┘
                                 │
                          ┌──────┴──────┐
                          │ B-6: 씬 저장 │
                          └──────┬──────┘
                                 │
                          ┌──────┴──────┐
                          │ Phase C:    │
                          │ 런타임 검증  │
                          └─────────────┘
```

---

## Cross-references

- `docs/systems/progression-architecture.md` — 진행 시스템 기술 아키텍처 (BAL-002)
- `docs/mcp/scene-setup-tasks.md` — 기본 씬 구성 태스크 (사전 조건)
- `docs/mcp/farming-tasks.md` — 농장 그리드 MCP 태스크 (참조 패턴)
- `docs/pipeline/data-pipeline.md` — SO 생성/JSON Import 파이프라인
- `docs/systems/project-structure.md` — 씬 계층 구조, 에셋 경로 규칙
- `docs/balance/progression-curve.md` — 모든 수치의 canonical 출처 (Designer 동시 작성 중)

## Open Questions

- [OPEN] JSON Import 파이프라인의 중첩 객체(unlockTable, milestones) 처리 방식이 data-pipeline.md에서 아직 상세 정의되지 않았을 수 있음. 확인 필요.

## Risks

- [RISK] MCP의 SO 참조 필드(Step B-4) 및 배열 필드(Step A-3, A-4) 설정 지원 범위 불확실. JSON Import 우회 방안 준비.
- [RISK] Step C-3의 기대값은 docs/balance/progression-curve.md가 확정된 후 갱신 필요. 현재는 data-pipeline.md 섹션 2.6의 임시 수치 사용.

---

*이 문서는 Claude Code가 기술적 제약과 설계 목표를 고려하여 자율적으로 작성했습니다.*
