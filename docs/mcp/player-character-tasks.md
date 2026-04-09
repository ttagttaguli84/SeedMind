# 플레이어 캐릭터 MCP 태스크

> 작성: Claude Code (Sonnet 4.6) | 2026-04-09  
> 문서 ID: MCP-012  
> 연관 아키텍처: `docs/systems/player-character-architecture.md` (ARC-051)  
> 연관 설계: `docs/systems/player-character.md` (DES-012)

---

## Context

이 문서는 플레이어 캐릭터 시스템을 MCP for Unity를 통해 Unity Editor에 구현하기 위한 단계별 태스크 시퀀스를 정의한다.

수치값(이동 속도, 인터랙션 반경, 카메라 파라미터 등)은 이 문서에 직접 기재하지 않는다.
→ 모든 수치: see `docs/systems/player-character.md`  
→ 클래스 구조/인터페이스: see `docs/systems/player-character-architecture.md`

**사전 조건**:
- Unity 프로젝트가 열려 있을 것
- `SCN_Farm` 씬이 존재할 것 (→ see `docs/mcp/scene-setup-tasks.md`)
- Cinemachine 패키지가 패키지 매니저에 추가되어 있을 것

**에셋명 출처**:
- `Player`, `PlayerModel`: `docs/mcp/scene-setup-tasks.md` 섹션 1.3 확인
- `VCam_Player`: `docs/systems/player-character-architecture.md` 섹션 6.1 확인
- `AC_Player`, `Anim_*`: `docs/systems/player-character.md` 섹션 5.2 확인
- `PlayerData`: `docs/systems/player-character-architecture.md` 섹션 3.1 확인

---

## Phase A: 데이터/스크립트 기반 구축

스크립트 파일과 ScriptableObject를 먼저 생성한 후 씬 배치를 진행한다.

---

### A-1. PlayerData SO 생성

**목적**: 이동 속도, 인터랙션 반경, 줌 범위 등 플레이어 수치 데이터를 SO로 분리

```
Step A-1-01: create_script
  → path: "Assets/_Project/Scripts/Player/PlayerData.cs"
  → namespace: SeedMind.Player
  → 클래스: PlayerData : ScriptableObject
  → [CreateAssetMenu] 포함
  → 필드: MoveSpeed, SprintSpeed, InteractionRadius, ZoomMin, ZoomMax, ZoomDefault, ZoomSpeed, CharacterHeight
  → 각 필드에 [Header] 어트리뷰트 적용
  → 수치값은 스크립트에 직접 기재하지 않음. Inspector에서 설정
  → 참조: docs/systems/player-character-architecture.md 섹션 3.1

Step A-1-02: create_asset
  → type: ScriptableObject (PlayerData)
  → path: "Assets/_Project/Data/Player/PlayerData.asset"
  → Inspector에서 수치 입력
  → MoveSpeed: (→ see docs/systems/player-character.md 섹션 2.2)
  → InteractionRadius: (→ see docs/systems/player-character.md 섹션 3.1)
  → ZoomMin/Max/Default: (→ see docs/systems/player-character.md 섹션 6.3)
  → CharacterHeight: (→ see docs/systems/player-character.md 섹션 7.1)
```

---

### A-2. PlayerController 스크립트

**목적**: WASD 입력 수신, 쿼터뷰 방향 변환, Rigidbody 이동 적용, 이동 잠금 관리

```
Step A-2-01: create_script
  → path: "Assets/_Project/Scripts/Player/PlayerController.cs"
  → namespace: SeedMind.Player
  → [RequireComponent(typeof(Rigidbody))] 포함
  → 구현 내용:
      - SerializeField: PlayerData playerData, PlayerAnimator playerAnimator
      - _isMovementLocked bool 필드
      - LockMovement() / UnlockMovement() 공개 메서드
      - FixedUpdate(): 입력 → 쿼터뷰 변환 → Rigidbody.linearVelocity 적용
      - ConvertToQuarterViewDirection(): forward=(-1,0,+1).normalized, right=(+1,0,+1).normalized
  → 참조: docs/systems/player-character-architecture.md 섹션 2.1

Step A-2-02: create_script
  → path: "Assets/_Project/Scripts/Core/IInteractable.cs"
  → namespace: SeedMind.Core
  → interface IInteractable { Priority, Interact(), InteractE() }
  → enum InteractionPriority { Tile_Farm=4, Crop=3, Structure=2, CursorDirect=1 }
  → 참조: docs/systems/player-character-architecture.md 섹션 3.5
```

---

### A-3. PlayerAnimator 스크립트

**목적**: Animator 파라미터 동기화, 도구 애니메이션 발동, UnlockMovement 체인

```
Step A-3-01: create_script
  → path: "Assets/_Project/Scripts/Player/PlayerAnimator.cs"
  → namespace: SeedMind.Player
  → [RequireComponent(typeof(Animator))] 포함
  → 구현 내용:
      - Animator.StringToHash로 파라미터 상수 정의
        IsWalking, ToolTrigger, ToolIndex, IsTalking
      - PlayToolAnimation(int toolIndex) 메서드
      - OnToolAnimationComplete() 메서드 (AnimationEvent 수신)
        → _controller.UnlockMovement() 호출
      - PlayerController 참조를 Awake에서 GetComponent로 취득
  → 참조: docs/systems/player-character-architecture.md 섹션 2.2
```

---

### A-4. PlayerInteractor 스크립트

**목적**: OverlapSphere 감지, 마우스 레이캐스트 타겟 결정, 우선순위 정렬

```
Step A-4-01: create_script
  → path: "Assets/_Project/Scripts/Player/PlayerInteractor.cs"
  → namespace: SeedMind.Player
  → 구현 내용:
      - SerializeField: PlayerData playerData, LayerMask interactableLayer
      - Physics.OverlapSphere(transform.position, playerData.InteractionRadius, interactableLayer)
      - GetCursorTarget(): Camera.main.ScreenPointToRay → Physics.Raycast
      - ResolvePriority(Collider[] candidates, IInteractable cursorTarget)
      - Update(): 좌클릭 → TryInteract(), E키 → TryInteractE()
      - 이동 잠금은 PlayerController를 통해 처리
  → 참조: docs/systems/player-character-architecture.md 섹션 2.4, 5.1, 5.2

Step A-4-02: define_layers
  → Unity Project Settings > Tags and Layers에 다음 레이어 추가 (미존재 시):
    "Interactable", "FarmTile"
  → Physics Layer Matrix: Player ↔ FarmTile 충돌 비활성화
  → 참조: docs/systems/player-character-architecture.md 섹션 4.3
```

---

### A-5. ToolSystem 스크립트

**목적**: 1~5 슬롯 관리, 숫자키 입력, 도구 사용 실행 위임

```
Step A-5-01: create_script
  → path: "Assets/_Project/Scripts/Player/ToolData.cs"
  → namespace: SeedMind.Player
  → [CreateAssetMenu] 포함
  → 필드: toolId(string), displayName(string), animIndex(ToolAnimIndex), consumesEnergy(bool), isConsumable(bool)
  → enum ToolAnimIndex { Dig=0, Water=1, Plant=2, Harvest=3 }
  → 참조: docs/systems/player-character-architecture.md 섹션 3.4

Step A-5-02: create_script
  → path: "Assets/_Project/Scripts/Player/ToolSystem.cs"
  → namespace: SeedMind.Player
  → 구현 내용:
      - _slots: ToolSlot[] (슬롯 수: → see docs/systems/player-character.md 섹션 4.1)
      - _activeSlotIndex: int
      - Update(): Alpha1~Alpha5 입력 → SelectSlot()
      - ActiveTool 프로퍼티
      - TryUseTool(FarmTile targetTile):
          1. EnergyManager.Instance.TryConsume(toolId) 호출
          2. 성공 시: PlayerAnimator.PlayToolAnimation() + FarmTile.ApplyTool()
          3. 실패 시: UI 피드백만 (이동 잠금 없음)
  → 참조: docs/systems/player-character-architecture.md 섹션 2.5, 8

Step A-5-03: create_assets (ToolData SO 5개)
  → 각각 Assets/_Project/Data/Tools/ 하위에 생성
  → SO 에셋명 및 toolId: "Hoe", "WateringCan", "Seeds", "Scythe", "Hand"
  → consumesEnergy: Hoe=true, WateringCan=true, Seeds=false, Scythe=true, Hand=false
  → animIndex: Hoe=Dig(0), WateringCan=Water(1), Seeds=Plant(2), Scythe=Harvest(3), Hand=Harvest(3)
  → 에셋명 출처: docs/systems/player-character.md 섹션 4.2
```

---

### A-6. CameraController 스크립트

**목적**: 마우스 휠 줌 입력, Cinemachine Virtual Camera Orthographic Size 보간 조정

```
Step A-6-01: create_script
  → path: "Assets/_Project/Scripts/Player/CameraController.cs"
  → namespace: SeedMind.Player
  → 구현 내용:
      - SerializeField: CinemachineVirtualCamera virtualCamera, PlayerData playerData
      - Update(): Input.GetAxis("Mouse ScrollWheel")
      - Mathf.Clamp(current - scroll * playerData.ZoomSpeed, playerData.ZoomMin, playerData.ZoomMax)
      - Mathf.Lerp(current, target, Time.deltaTime * 8f) 보간
  → 참조: docs/systems/player-character-architecture.md 섹션 2.3
  → [RISK] Cinemachine namespace: using Cinemachine; (패키지 설치 확인 후 진행)
```

---

## Phase B: 프리팹/에셋 구축

---

### B-1. Player 프리팹 구축

**목적**: Player GameObject 계층 구조 생성 + 컴포넌트 연결 + 프리팹화

```
Step B-1-01: create_gameobject
  → name: "Player"
  → position: (→ 초기 위치 see docs/mcp/scene-setup-tasks.md 섹션 1.3 — Tile_3_3 부근)
  → tag: "Player"
  → layer: "Player"

Step B-1-02: add_component
  → target: "Player"
  → component: Rigidbody
  → 설정:
      constraints.FreezeRotation = true (X/Y/Z 모두)
      constraints.FreezePositionY = true
      collisionDetectionMode = Continuous

Step B-1-03: add_component
  → target: "Player"
  → component: CapsuleCollider
  → height: (→ see docs/systems/player-character.md 섹션 7.1 CharacterHeight)
  → radius: (CharacterHeight * 0.25 계산값 — see docs/systems/player-character.md 섹션 7.1)
  → center.y: (height / 2)

Step B-1-04: add_component
  → target: "Player"
  → scripts: PlayerController, PlayerAnimator, PlayerInteractor, ToolSystem
  → PlayerController.playerData: PlayerData.asset 연결
  → PlayerInteractor.playerData: PlayerData.asset 연결
  → PlayerInteractor.interactableLayer: "Interactable" 레이어 마스크 설정

Step B-1-05: create_gameobject (자식)
  → name: "PlayerModel"
  → parent: "Player"
  → position: (0, 0, 0) 로컬
  → 역할: 캐릭터 비주얼 메시 컨테이너
  → 초기 메시: 임시 Capsule primitive (Phase 2에서 실제 모델로 교체)
  → [RISK] 로우폴리 모델 MCP 생성 품질 한계 — see docs/systems/player-character.md 섹션 11 Risks

Step B-1-06: add_component
  → target: "PlayerModel"
  → component: Animator
  → controller: AC_Player (Step B-3에서 생성 후 연결)

Step B-1-07: link_reference
  → PlayerController.playerAnimator → PlayerAnimator 컴포넌트 연결

Step B-1-08: create_prefab
  → source: "Player" GameObject
  → path: "Assets/_Project/Prefabs/Player/Player.prefab"
  → 원본 씬 오브젝트는 프리팹 인스턴스로 유지
```

---

### B-2. Cinemachine Virtual Camera 설정

**목적**: 플레이어 추적 쿼터뷰 카메라 설정

```
Step B-2-01: create_gameobject
  → name: "VCam_Player"
  → component: CinemachineVirtualCamera
  → Follow target: "Player" GameObject

Step B-2-02: configure_cinemachine
  → VCam_Player Lens 설정:
      Orthographic = true
      X Rotation: (→ see docs/systems/player-character.md 섹션 6.1)
      Y Rotation: (→ see docs/systems/player-character.md 섹션 6.1)
      Orthographic Size (초기값): (→ see docs/systems/player-character.md 섹션 6.3)
  → Body: Framing Transposer
      Damping X: (→ see docs/systems/player-character.md 섹션 6.3)
      Damping Y: (→ see docs/systems/player-character.md 섹션 6.3)
      Dead Zone Width: (→ see docs/systems/player-character.md 섹션 6.3)
      Dead Zone Height: (→ see docs/systems/player-character.md 섹션 6.3)
      Soft Zone Width: (→ see docs/systems/player-character.md 섹션 6.3)
      Soft Zone Height: (→ see docs/systems/player-character.md 섹션 6.3)
      Camera Distance (오프셋 높이): (→ see docs/systems/player-character.md 섹션 6.3)
  → Aim: Do Nothing
  → Background Color: (→ see docs/systems/player-character.md 섹션 6.3)

Step B-2-03: add_component
  → target: "VCam_Player" 부모 오브젝트 또는 Main Camera
  → component: CameraController
  → CameraController.virtualCamera: VCam_Player 연결
  → CameraController.playerData: PlayerData.asset 연결

Step B-2-04: configure_main_camera
  → Main Camera에 CinemachineBrain 컴포넌트 추가 (미설정 시)
  → Update Method: FixedUpdate (물리 기반 이동과 동기화)
```

---

### B-3. 애니메이션 클립 + Animator Controller

**목적**: 상태 머신(Idle/Walk/Tool_*/Talk) 및 클립 생성

```
Step B-3-01: create_animator_controller
  → path: "Assets/_Project/Animation/Player/AC_Player.controller"
  → 에셋명 출처: docs/systems/player-character-architecture.md 섹션 7.1

Step B-3-02: create_animation_clips (7개)
  → 경로: "Assets/_Project/Animation/Player/"
  → 클립명 및 루프 설정:
      Anim_Idle.anim    — Loop = true  (길이: → see docs/systems/player-character.md 섹션 5.2)
      Anim_Walk.anim    — Loop = true  (길이: → see docs/systems/player-character.md 섹션 5.2)
      Anim_Dig.anim     — Loop = false (길이: → see docs/systems/player-character.md 섹션 5.2)
      Anim_Water.anim   — Loop = false (길이: → see docs/systems/player-character.md 섹션 5.2)
      Anim_Plant.anim   — Loop = false (길이: → see docs/systems/player-character.md 섹션 5.2)
      Anim_Harvest.anim — Loop = false (길이: → see docs/systems/player-character.md 섹션 5.2)
      Anim_Talk.anim    — Loop = true  (길이: → see docs/systems/player-character.md 섹션 5.2)
  → 초기 클립: Placeholder (이동/회전 없는 정지 포즈). Phase 2에서 실제 모션으로 교체

Step B-3-03: configure_animator_states
  → AC_Player에 다음 상태 추가:
      Idle (default), Walk, Tool_Dig, Tool_Water, Tool_Plant, Tool_Harvest, Talk
  → 각 상태에 해당 클립 할당

Step B-3-04: add_animator_parameters
  → IsWalking (Bool)
  → ToolTrigger (Trigger)
  → ToolIndex (Int)
  → IsTalking (Bool)

Step B-3-05: configure_transitions
  → Idle → Walk:     condition = IsWalking == true  | Has Exit Time = false
  → Walk → Idle:     condition = IsWalking == false | Has Exit Time = false
  → Any → Tool_Dig:  ToolTrigger + ToolIndex==0     | Has Exit Time = false
  → Any → Tool_Water: ToolTrigger + ToolIndex==1    | Has Exit Time = false
  → Any → Tool_Plant: ToolTrigger + ToolIndex==2    | Has Exit Time = false
  → Any → Tool_Harvest: ToolTrigger + ToolIndex==3  | Has Exit Time = false
  → Tool_* → Idle:   Has Exit Time = true (클립 완료 후 자동 전환)
  → Idle → Talk:     IsTalking == true  | Has Exit Time = false
  → Talk → Idle:     IsTalking == false | Has Exit Time = false

Step B-3-06: add_animation_events
  → Anim_Dig.anim     끝 지점에 AnimationEvent → Function: "OnToolAnimationComplete"
  → Anim_Water.anim   끝 지점에 AnimationEvent → Function: "OnToolAnimationComplete"
  → Anim_Plant.anim   끝 지점에 AnimationEvent → Function: "OnToolAnimationComplete"
  → Anim_Harvest.anim 끝 지점에 AnimationEvent → Function: "OnToolAnimationComplete"
  → 수신 컴포넌트: PlayerAnimator.cs (Player/PlayerModel에 Animator 부착되어 있어야 함)
  → [RISK] MCP placeholder 클립에 AnimationEvent 삽입이 지원되는지 확인 필요

Step B-3-07: assign_controller
  → Player > PlayerModel > Animator.runtimeAnimatorController = AC_Player
```

---

## Phase C: 씬 배치 및 통합 테스트

---

### C-1. SCN_Farm에 Player 배치

```
Step C-1-01: open_scene
  → scene: "Assets/_Project/Scenes/SCN_Farm.unity"

Step C-1-02: instantiate_prefab
  → prefab: "Assets/_Project/Prefabs/Player/Player.prefab"
  → position: Tile_3_3 기준 월드 좌표 (FarmGrid 중앙 부근)
              구체적 좌표: → see docs/mcp/scene-setup-tasks.md 섹션 1.3
  → name in hierarchy: "Player"

Step C-1-03: verify_hierarchy
  → Player (PlayerController, PlayerAnimator, PlayerInteractor, ToolSystem, Rigidbody, CapsuleCollider)
  └── PlayerModel (Animator → AC_Player 연결)
```

---

### C-2. Camera 추적 연결

```
Step C-2-01: link_camera_follow
  → VCam_Player.Follow 타겟 = 씬의 "Player" 인스턴스
  → VCam_Player.LookAt 타겟 = 없음 (Aim: Do Nothing)

Step C-2-02: verify_cinemachine_brain
  → Main Camera에 CinemachineBrain이 존재하는지 확인
  → 없으면 AddComponent

Step C-2-03: set_camera_background
  → Main Camera > Background Color: (→ see docs/systems/player-character.md 섹션 6.3)
```

---

### C-3. 통합 테스트 (이동/인터랙션/도구 전환)

```
Step C-3-01: play_mode_enter
  → MCP Play Mode 진입

Step C-3-02: test_movement
  → 검증 항목:
      □ WASD 8방향 이동 동작
      □ 쿼터뷰 방향 변환 — W키가 북서 방향으로 이동하는지 확인
      □ 대각선 이동 속도 일정 유지 (normalize 적용 확인)
      □ 울타리/건물 Collider 충돌 차단
      □ FarmTile 통과 가능 (IsTrigger 작동)

Step C-3-03: test_animation
  → 검증 항목:
      □ WASD 입력 시 Walk 상태 전환 + IsWalking=true
      □ 입력 중단 시 Idle 복귀
      □ 숫자키 1로 호미 선택 → 빈 타일 좌클릭 → Tool_Dig 애니메이션 재생
      □ 도구 애니메이션 완료 후 이동 잠금 해제 확인

Step C-3-04: test_interaction
  → 검증 항목:
      □ 인터랙션 반경 (→ see docs/systems/player-character.md 섹션 3.1) 내 타일만 감지
      □ 마우스 커서 직접 지정 타겟 우선순위 최우선 적용
      □ E키로 NPC/시설 인터랙션 트리거 (NPC 시스템 미구현 시 로그 출력 확인)

Step C-3-05: test_camera
  → 검증 항목:
      □ 플레이어 이동 시 VCam_Player 추적 동작
      □ Dead Zone: 소범위 이동에서 카메라 고정 확인
      □ 마우스 휠 → Orthographic Size 보간 증감 확인
      □ Zoom Min/Max 범위 이탈 방지 확인

Step C-3-06: test_tool_slots
  → 검증 항목:
      □ 숫자키 1~5로 슬롯 전환 — ActiveTool 변경 확인
      □ 호미(1)/물뿌리개(2)/낫(4) 사용 시 에너지 소모 이벤트 발생 로그
      □ 씨앗(3)/손(5) 사용 시 에너지 소모 없음 확인

Step C-3-07: play_mode_exit
  → MCP Play Mode 종료
  → 씬 저장
```

---

## 예상 MCP 호출 수

| Phase | 태스크 수 | 예상 MCP 호출 | 비고 |
|-------|----------|-------------|------|
| A (데이터/스크립트) | 12 steps | ~25회 | 스크립트 생성, SO 생성, 레이어 설정 |
| B (프리팹/에셋) | 15 steps | ~20회 | 프리팹 구성, Cinemachine 설정, 애니메이터 |
| C (배치/테스트) | 9 steps | ~12회 | 씬 배치, 링크 연결, 테스트 |
| **합계** | **36 steps** | **~57회** | |

---

## Open Questions

- [OPEN] `SCN_Farm` 씬의 Player 초기 월드 좌표 미확정 — `docs/mcp/scene-setup-tasks.md` 섹션 1.3의 "Tile_3_3 위치" 기준으로 구체적 (x, y, z) 좌표는 FarmGrid 배치 완료 후 결정 가능
- [OPEN] Cinemachine 패키지 버전 — Unity 6 기준 최신 안정 버전 사용. MCP 태스크 실행 전 Package Manager에서 확인 필요
- [OPEN] AnimationEvent를 MCP로 Animation Clip에 삽입하는 방식 지원 여부 미확인 — 미지원 시 Phase 2에서 Unity Editor에서 수동 삽입 필요
- [OPEN] 달리기(SprintSpeed) 기능 채택 여부 미결정 — Phase 2 플레이 테스트 후 결정 (→ see `docs/systems/player-character.md` 섹션 10)
