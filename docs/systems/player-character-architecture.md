# 플레이어 캐릭터 아키텍처

> 작성: Claude Code (Sonnet 4.6) | 2026-04-09  
> 문서 ID: ARC-051  
> 연관 설계 문서: `docs/systems/player-character.md` (DES-012)

---

## 1. 개요 및 시스템 다이어그램

플레이어 캐릭터 시스템은 입력 처리, 이동, 인터랙션, 도구 사용, 카메라 추적의 다섯 가지 책임을 컴포넌트 단위로 분리한다. 모든 수치 (이동 속도, 인터랙션 반경, 카메라 파라미터 등)는 `docs/systems/player-character.md`가 canonical이며, 이 문서에 직접 기재하지 않는다.

```
┌─────────────────────────────────────────────────────────────────┐
│  Player (GameObject)                                            │
│  ├── PlayerController.cs   ← 입력 수신 + 이동 벡터 계산         │
│  ├── PlayerAnimator.cs     ← Animator 파라미터 동기화           │
│  ├── PlayerInteractor.cs   ← 반경 감지 + 우선순위 정렬          │
│  ├── ToolSystem.cs         ← 슬롯 관리 + 도구 사용 실행         │
│  └── Rigidbody             ← 물리 이동 (kinematic 아님)         │
│                                                                 │
│  PlayerModel (자식 GameObject)                                  │
│  └── SkinnedMeshRenderer / MeshRenderer                        │
└─────────────────────────────────────────────────────────────────┘

               ↕ (이동 상태 전달)          ↕ (에너지 소모 요청)
┌─────────────────────┐        ┌──────────────────────────────┐
│  PlayerAnimator.cs  │        │  EnergyManager (참조)        │
│  (Animator 제어)    │        │  (→ see energy-system.md)   │
└─────────────────────┘        └──────────────────────────────┘

               ↕ (도구 등급 조회)
┌───────────────────────────────────────┐
│  ToolUpgradeManager (참조)            │
│  (→ see tool-upgrade.md)             │
└───────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│  CinemachineVirtualCamera (별도 GameObject)         │
│  └── CameraController.cs   ← 줌 입력 + Follow 설정 │
└─────────────────────────────────────────────────────┘
```

---

## 2. 클래스 구조

### 2.1 PlayerController.cs

**역할**: 입력 수신, 쿼터뷰 방향 변환, Rigidbody 이동 적용, 이동 잠금 관리

```csharp
namespace SeedMind.Player
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerData playerData;  // → see docs/systems/player-character.md 섹션 2.2
        [SerializeField] private PlayerAnimator playerAnimator;

        // 외부 시스템 참조
        private Rigidbody _rb;
        private bool _isMovementLocked;   // Tool_* / Talk 상태 시 true

        // 공개 인터페이스
        public void LockMovement()   { _isMovementLocked = true; }
        public void UnlockMovement() { _isMovementLocked = false; }
        public Vector3 CurrentVelocity { get; private set; }

        private void FixedUpdate()
        {
            if (_isMovementLocked) return;
            Vector2 input = GetInputVector();
            Vector3 worldDir = ConvertToQuarterViewDirection(input);
            // 이동 속도: → see docs/systems/player-character.md 섹션 2.2
            _rb.linearVelocity = worldDir * playerData.MoveSpeed;
            CurrentVelocity = _rb.linearVelocity;
        }

        // 쿼터뷰 입력 변환 (→ see docs/systems/player-character.md 섹션 2.1 매핑 테이블)
        private Vector3 ConvertToQuarterViewDirection(Vector2 input)
        {
            // X=45°, Y=45° 쿼터뷰 기준 변환
            // W → (+Z -X), S → (-Z +X), A → (-Z -X), D → (+Z +X)
            Vector3 forward = new Vector3(-1f, 0f, 1f).normalized;
            Vector3 right   = new Vector3( 1f, 0f, 1f).normalized;
            return (forward * input.y + right * input.x).normalized;
        }
    }
}
```

**이동 잠금 흐름**:
- `PlayerInteractor` 또는 `ToolSystem`이 도구 사용 시작 시 `LockMovement()` 호출
- `PlayerAnimator`가 단발 애니메이션 완료 이벤트(`AnimationEvent`) 수신 시 `UnlockMovement()` 호출

### 2.2 PlayerAnimator.cs

**역할**: `PlayerController`와 `ToolSystem`의 상태를 Animator 파라미터로 변환

```csharp
namespace SeedMind.Player
{
    [RequireComponent(typeof(Animator))]
    public class PlayerAnimator : MonoBehaviour
    {
        private Animator _animator;
        private PlayerController _controller;

        // Animator 파라미터 이름 상수
        private static readonly int ParamIsWalking  = Animator.StringToHash("IsWalking");
        private static readonly int ParamToolTrigger = Animator.StringToHash("ToolTrigger");
        private static readonly int ParamToolIndex  = Animator.StringToHash("ToolIndex");
        private static readonly int ParamIsTalking  = Animator.StringToHash("IsTalking");

        // AnimationEvent에서 호출됨 (클립 끝 지점에 이벤트 삽입)
        public void OnToolAnimationComplete()
        {
            _controller.UnlockMovement();
        }

        public void PlayToolAnimation(int toolIndex)
        {
            _animator.SetInteger(ParamToolIndex, toolIndex);
            _animator.SetTrigger(ParamToolTrigger);
        }
    }
}
```

**애니메이션 파라미터 목록** (→ 섹션 7.2 참조):

| 파라미터 | 타입 | 용도 |
|----------|------|------|
| `IsWalking` | Bool | Walk ↔ Idle 전환 |
| `ToolTrigger` | Trigger | Tool_* 상태 진입 |
| `ToolIndex` | Int | 0=Dig, 1=Water, 2=Plant, 3=Harvest |
| `IsTalking` | Bool | Talk 상태 진입/종료 |

### 2.3 CameraController.cs (Cinemachine 연동)

**역할**: 마우스 휠 줌 입력 처리, Cinemachine Virtual Camera의 Orthographic Size 조정

```csharp
namespace SeedMind.Player
{
    public class CameraController : MonoBehaviour
    {
        [Header("Cinemachine")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;

        // 줌 범위: → see docs/systems/player-character.md 섹션 6.3
        [SerializeField] private PlayerData playerData;

        private void Update()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                float current = virtualCamera.m_Lens.OrthographicSize;
                float target  = Mathf.Clamp(
                    current - scroll * playerData.ZoomSpeed,
                    playerData.ZoomMin,   // → see docs/systems/player-character.md 섹션 6.3
                    playerData.ZoomMax    // → see docs/systems/player-character.md 섹션 6.3
                );
                virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(current, target, Time.deltaTime * 8f);
            }
        }
    }
}
```

**Cinemachine Virtual Camera 씬 설정**:
- `Follow` 타겟: `Player` GameObject
- Body: `Framing Transposer`
- Damping X/Y: (→ see `docs/systems/player-character.md` 섹션 6.3)
- Dead Zone X/Y: (→ see `docs/systems/player-character.md` 섹션 6.3)
- Soft Zone X/Y: (→ see `docs/systems/player-character.md` 섹션 6.3)

### 2.4 PlayerInteractor.cs (타일 인터랙션)

**역할**: 인터랙션 반경 내 대상 감지, 우선순위 정렬, 마우스 커서 레이캐스트 기반 타겟 결정

```csharp
namespace SeedMind.Player
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;  // interactionRadius → see docs/systems/player-character.md 섹션 3.1
        [SerializeField] private LayerMask interactableLayer;

        private ToolSystem _toolSystem;
        private IInteractable _currentTarget;

        private void Update()
        {
            // 1. 인터랙션 반경 내 후보 수집 (Physics.OverlapSphere)
            Collider[] candidates = Physics.OverlapSphere(
                transform.position,
                playerData.InteractionRadius,  // → see docs/systems/player-character.md 섹션 3.1
                interactableLayer
            );

            // 2. 마우스 레이캐스트로 커서 타겟 결정
            IInteractable cursorTarget = GetCursorTarget();

            // 3. 우선순위 정렬 → see 섹션 5.2
            _currentTarget = ResolvePriority(candidates, cursorTarget);

            // 4. 좌클릭/E키 입력 처리
            if (Input.GetMouseButtonDown(0) && _currentTarget != null)
                TryInteract(_currentTarget);
            if (Input.GetKeyDown(KeyCode.E) && _currentTarget != null)
                TryInteractE(_currentTarget);
        }
    }
}
```

### 2.5 ToolSystem.cs

**역할**: 도구 슬롯 관리 (1~5 숫자키), 현재 도구 상태 유지, 도구 사용 실행 위임

```csharp
namespace SeedMind.Player
{
    public class ToolSystem : MonoBehaviour
    {
        // 슬롯 수: → see docs/systems/player-character.md 섹션 4.1
        private ToolSlot[] _slots;
        private int _activeSlotIndex = 0;

        public ToolData ActiveTool => _slots[_activeSlotIndex]?.tool;
        public ToolGrade ActiveToolGrade => GetGradeFromUpgradeManager(ActiveTool);
        // 도구 등급 조회: → see docs/systems/tool-upgrade.md

        private void Update()
        {
            for (int i = 0; i < _slots.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    SelectSlot(i);
            }
        }

        public void UseTool(FarmTile targetTile)
        {
            // 에너지 소모 요청 → see 섹션 8
            // 애니메이션 트리거 → PlayerAnimator.PlayToolAnimation()
            // 타일 상태 전환 → FarmTile.ApplyTool()
        }
    }
}
```

---

## 3. 데이터 모델

### 3.1 PlayerData ScriptableObject

`Assets/_Project/Data/Player/PlayerData.asset`에 배치되는 SO. 모든 수치의 실제 값은 `docs/systems/player-character.md`가 canonical이다.

```csharp
namespace SeedMind.Player
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "SeedMind/Player/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [Header("이동")]
        public float MoveSpeed;        // → see docs/systems/player-character.md 섹션 2.2
        public float SprintSpeed;      // → see docs/systems/player-character.md 섹션 2.2 (달리기, OPEN)

        [Header("인터랙션")]
        public float InteractionRadius; // → see docs/systems/player-character.md 섹션 3.1

        [Header("카메라 줌")]
        public float ZoomMin;           // → see docs/systems/player-character.md 섹션 6.3
        public float ZoomMax;           // → see docs/systems/player-character.md 섹션 6.3
        public float ZoomDefault;       // → see docs/systems/player-character.md 섹션 6.3
        public float ZoomSpeed;         // Inspector 조정값, Phase 2 테스트 후 확정

        [Header("비주얼")]
        public float CharacterHeight;   // → see docs/systems/player-character.md 섹션 7.1
    }
}
```

**JSON 스키마 (SO 직렬화 기준)**:

```json
{
  "MoveSpeed":          "[→ see docs/systems/player-character.md 섹션 2.2]",
  "SprintSpeed":        "[→ see docs/systems/player-character.md 섹션 2.2]",
  "InteractionRadius":  "[→ see docs/systems/player-character.md 섹션 3.1]",
  "ZoomMin":            "[→ see docs/systems/player-character.md 섹션 6.3]",
  "ZoomMax":            "[→ see docs/systems/player-character.md 섹션 6.3]",
  "ZoomDefault":        "[→ see docs/systems/player-character.md 섹션 6.3]",
  "ZoomSpeed":          "[Phase 2 Inspector 조정]",
  "CharacterHeight":    "[→ see docs/systems/player-character.md 섹션 7.1]"
}
```

### 3.2 PlayerState enum

```csharp
namespace SeedMind.Player
{
    public enum PlayerState
    {
        Idle,
        Walk,
        Tool_Dig,
        Tool_Water,
        Tool_Plant,
        Tool_Harvest,
        Talk
    }
}
```

상태 전환 조건: (→ see `docs/systems/player-character.md` 섹션 5.1)

### 3.3 AnimationState enum

```csharp
namespace SeedMind.Player
{
    // Animator Controller 내 ToolIndex 파라미터와 매핑
    public enum ToolAnimIndex
    {
        Dig     = 0,   // Anim_Dig     → see docs/systems/player-character.md 섹션 5.2
        Water   = 1,   // Anim_Water   → see docs/systems/player-character.md 섹션 5.2
        Plant   = 2,   // Anim_Plant   → see docs/systems/player-character.md 섹션 5.2
        Harvest = 3    // Anim_Harvest → see docs/systems/player-character.md 섹션 5.2
    }
}
```

### 3.4 ToolSlot / ToolData

```csharp
namespace SeedMind.Player
{
    [System.Serializable]
    public class ToolSlot
    {
        public ToolData tool;
        public int quantity;   // 씨앗 등 소모성 아이템의 잔여 수량. 도구는 -1 (무한)
    }

    [CreateAssetMenu(fileName = "ToolData", menuName = "SeedMind/Player/ToolData")]
    public class ToolData : ScriptableObject
    {
        public string toolId;        // e.g. "Hoe", "WateringCan", "Seeds", "Scythe", "Hand"
        public string displayName;
        public ToolAnimIndex animIndex;  // Hand 도구는 전용 클립 없이 Harvest(3) 클립 재활용 (아이템 줍기 = 간단한 집기 동작)
        public bool consumesEnergy;  // 에너지 소모 여부 → see docs/systems/player-character.md 섹션 4.2
        public bool isConsumable;    // 씨앗 등 소모성 도구 여부
    }
}
```

### 3.5 IInteractable 인터페이스

```csharp
namespace SeedMind.Core
{
    public interface IInteractable
    {
        InteractionPriority Priority { get; }  // → see docs/systems/player-character-architecture.md 섹션 5.2 (우선순위 정렬 로직) / docs/systems/player-character.md 섹션 3.3 (우선순위 규칙)
        void Interact(PlayerController player);
        void InteractE(PlayerController player);
    }

    public enum InteractionPriority
    {
        Tile_Farm  = 4,   // 경작/물주기 타일
        Crop       = 3,   // 수확 가능 작물
        Structure  = 2,   // NPC / 시설
        CursorDirect = 1  // 마우스 커서 직접 지정 (최우선)
    }
}
```

---

## 4. 이동 시스템 구현

### 4.1 이동 벡터 계산 (쿼터뷰 변환)

쿼터뷰(X=45°, Y=45°) 카메라 기준 입력 변환 매핑:
(→ see `docs/systems/player-character.md` 섹션 2.1)

구현 방식:
```
forward = normalize(-1, 0, +1)   // 화면 "위" 방향
right   = normalize(+1, 0, +1)   // 화면 "오른쪽" 방향
worldDir = (forward * inputY + right * inputX).normalized
```

대각선 입력 시 normalize 필수. normalize 생략 시 대각선 방향에서 속도가 √2배 빨라지는 버그 발생.

### 4.2 Rigidbody vs CharacterController 선택

**Rigidbody (채택)**

| 비교 항목 | Rigidbody | CharacterController |
|----------|-----------|---------------------|
| 물리 충돌 | 자동 처리 | 수동 `Move()` 호출 |
| 중력 | 자동 적용 (Y 고정 필요) | 수동 구현 |
| Unity 6 MCP 호환성 | AddComponent로 즉시 추가 | 동일 |
| 슬로프/계단 처리 | 별도 설정 필요 | 내장 지원 |

Rigidbody 설정:
- `constraints.FreezeRotation = true` (X/Z/Y 회전 모두 잠금)
- `constraints.FreezePositionY = true` (농장 씬에서 Y축 이동 불필요)
- `collisionDetectionMode = Continuous` (고속 이동 시 터널링 방지)

### 4.3 충돌 레이어 설정

| 레이어 | 태그 | 용도 |
|--------|------|------|
| `Default` | - | 환경 정적 오브젝트 |
| `Player` | `Player` | 플레이어 전용 |
| `Interactable` | `Interactable` | `PlayerInteractor`가 OverlapSphere로 감지 |
| `FarmTile` | `FarmTile` | 마우스 레이캐스트 타겟 |

Physics Layer Matrix: `Player` ↔ `FarmTile` 충돌 비활성화 (통과 가능, → see `docs/systems/player-character.md` 섹션 2.3)

---

## 5. 인터랙션 시스템 구현

### 5.1 OverlapSphere 방식

```
감지 방식: Physics.OverlapSphere(playerPos, interactionRadius, interactableLayer)
인터랙션 반경: (→ see docs/systems/player-character.md 섹션 3.1)
레이어: Interactable (타일, NPC, 시설, 출하함, 상점)
```

마우스 커서 타겟 결정:
```
Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
Physics.Raycast(ray, out hit, maxDistance: 50f, layerMask: FarmTile | Interactable)
```

커서가 가리키는 대상이 OverlapSphere 결과에 포함된 경우 → 최우선 선택
커서 대상이 반경 밖인 경우 → 반경 내 대상 중 우선순위 정렬로 결정

### 5.2 인터랙션 우선순위 정렬 로직

우선순위 규칙: (→ see `docs/systems/player-character.md` 섹션 3.3)

```csharp
private IInteractable ResolvePriority(Collider[] candidates, IInteractable cursorTarget)
{
    // 커서 직접 지정 대상이 반경 내 있으면 최우선
    if (cursorTarget != null && IsInRadius(cursorTarget))
        return cursorTarget;

    // 우선순위 낮은 숫자(InteractionPriority enum 값)가 높은 우선순위
    return candidates
        .Select(c => c.GetComponent<IInteractable>())
        .Where(i => i != null)
        .OrderBy(i => (int)i.Priority)
        .FirstOrDefault();
}
```

---

## 6. Cinemachine 설정

### 6.1 Virtual Camera 파라미터

Cinemachine Virtual Camera GameObject: `VCam_Player`

| 파라미터 | 설정값 | 참조 |
|----------|--------|------|
| Lens > Projection | Orthographic | (→ see `docs/systems/player-character.md` 섹션 6.3) |
| Lens > Orthographic Size | (초기값) | (→ see `docs/systems/player-character.md` 섹션 6.3) |
| Lens > X Rotation | (쿼터뷰 각도) | (→ see `docs/systems/player-character.md` 섹션 6.1) |
| Lens > Y Rotation | (쿼터뷰 각도) | (→ see `docs/systems/player-character.md` 섹션 6.1) |
| Body | Framing Transposer | 플레이어 추적 |
| Aim | Do Nothing | 회전 고정 |

### 6.2 Dead Zone / Damping 값

모든 파라미터 값: (→ see `docs/systems/player-character.md` 섹션 6.3)

- `Damping X`, `Damping Y`: Framing Transposer Body 설정
- `Dead Zone Width`, `Dead Zone Height`: Framing Transposer Body 설정
- `Soft Zone Width`, `Soft Zone Height`: Framing Transposer Body 설정

[RISK] Dead Zone/Damping 수치는 실제 플레이 테스트 없이 확정 불가. Phase 2 테스트 후 조정 필요 (→ see `docs/systems/player-character.md` 섹션 11 Risks).

카메라 오프셋 (높이): (→ see `docs/systems/player-character.md` 섹션 6.3)

---

## 7. 애니메이션 구현

### 7.1 Animator Controller 구조

에셋명: `AC_Player.controller`  
위치: `Assets/_Project/Animation/Player/`

```
[Any State]
    ↓ ToolTrigger (Trigger)
    → Tool_Dig    (ToolIndex == 0)
    → Tool_Water  (ToolIndex == 1)
    → Tool_Plant  (ToolIndex == 2)
    → Tool_Harvest(ToolIndex == 3)

Idle ←──────────── (IsWalking == false)
  ↓
Walk ←──────────── (IsWalking == true)

Idle ←──────────── (IsTalking == false)
  ↓
Talk ←──────────── (IsTalking == true)

Tool_* → Idle      (Has Exit Time, 애니메이션 완료 후)
```

상태 전환 상세: (→ see `docs/systems/player-character.md` 섹션 5.1)

### 7.2 Trigger/Bool 파라미터 목록

| 파라미터명 | 타입 | 설명 | 담당 클래스 |
|-----------|------|------|------------|
| `IsWalking` | Bool | 이동 중 여부 | `PlayerAnimator` |
| `ToolTrigger` | Trigger | 도구 애니메이션 발동 | `PlayerAnimator` |
| `ToolIndex` | Int | 도구 종류 구분 (0~3) | `PlayerAnimator` |
| `IsTalking` | Bool | 대화 상태 여부 | `PlayerAnimator` |

애니메이션 클립 목록 (클립명, 길이): (→ see `docs/systems/player-character.md` 섹션 5.2)

**AnimationEvent 설정**:
- `Anim_Dig`, `Anim_Water`, `Anim_Plant`, `Anim_Harvest`: 클립 끝 지점에 `OnToolAnimationComplete` 이벤트 삽입
- 이 이벤트가 `PlayerAnimator.OnToolAnimationComplete()` → `PlayerController.UnlockMovement()` 체인 트리거

---

## 8. 에너지 연동 인터페이스

에너지 소모 수치 및 회복 규칙: (→ see `docs/systems/energy-system.md`)

```csharp
// ToolSystem.cs 내 에너지 소모 요청 패턴
public bool TryUseTool(FarmTile targetTile)
{
    if (!EnergyManager.Instance.TryConsume(ActiveTool.toolId))
    {
        // 에너지 부족 → 도구 사용 거부, UI 피드백
        return false;
    }
    // 에너지 소모 성공 → 애니메이션 + 타일 처리 진행
    ExecuteToolAction(targetTile);
    return true;
}
```

연동 지점:

| 상태 전환 | 에너지 이벤트 | 참조 |
|----------|-------------|------|
| `Tool_Dig` 진입 | `TryConsume("Hoe")` | (→ see `docs/systems/energy-system.md` 섹션 2) |
| `Tool_Water` 진입 | `TryConsume("WateringCan")` | (→ see `docs/systems/energy-system.md` 섹션 2) |
| `Tool_Harvest` 진입 | `TryConsume("Scythe")` | (→ see `docs/systems/energy-system.md` 섹션 2) |
| `Tool_Plant` 진입 | 에너지 무소모 | (→ see `docs/systems/player-character.md` 섹션 4.2) |
| 에너지 = 0 도달 | 강제 기절 → 씬 전환 | (→ see `docs/systems/energy-system.md`) |

에너지 = 0 처리 흐름:
```
EnergyManager.OnEnergyDepleted 이벤트
  → PlayerController.LockMovement()
  → PlayerAnimator (Idle 고정)
  → GameManager (다음 날 06:00 씬 재시작)
```

---

## 9. 도구 강화 연동 인터페이스

도구 등급 체계 및 업그레이드 조건: (→ see `docs/systems/tool-upgrade.md`)

도구 등급: `Basic` → `Reinforced` → `Legendary`

```csharp
// ToolSystem.cs 내 강화 등급 조회 패턴
private ToolGrade GetGradeFromUpgradeManager(ToolData tool)
{
    // → see docs/systems/tool-upgrade.md 등급 enum 정의
    return ToolUpgradeManager.Instance.GetGrade(tool.toolId);
}

// 범위 계산 (등급별 타일 영향 범위)
private Vector2Int[] GetAffectedTiles(FarmTile origin, ToolGrade grade)
{
    // Basic:      1x1 (origin만)
    // Reinforced: 1x3
    // Legendary:  3x3
    // 범위 수치: → see docs/systems/tool-upgrade.md 섹션 4
}
```

강화 등급별 연동 지점: (→ see `docs/systems/player-character.md` 섹션 8.2)

---

## 10. MCP 구현 계획 요약

상세 MCP 태스크 시퀀스: (→ see `docs/mcp/player-character-tasks.md`)

| Phase | 내용 | 예상 MCP 호출 |
|-------|------|-------------|
| A | 데이터/스크립트 기반 구축 | ~25회 |
| B | 프리팹/에셋 구축 | ~20회 |
| C | 씬 배치 및 통합 테스트 | ~12회 |
| 합계 | - | ~57회 (→ see `docs/mcp/player-character-tasks.md` 하단 집계 표) |

---

## 11. Cross-references

| 문서 | 관련 항목 |
|------|-----------|
| `docs/systems/player-character.md` | 모든 수치 canonical (이동 속도, 반경, 카메라 파라미터, 애니메이션 클립 길이) |
| `docs/architecture.md` 섹션 3 | 전체 프로젝트 스크립트 폴더 구조 (`Scripts/Player/`) |
| `docs/systems/energy-system.md` | 도구별 에너지 소모 수치 canonical |
| `docs/systems/tool-upgrade.md` | 도구 등급 정의, 업그레이드 조건, 범위 효과 canonical |
| `docs/systems/farming-system.md` | 타일 상태 전환 (`FarmTile.ApplyTool()` 수신 측) |
| `docs/mcp/player-character-tasks.md` | 구체적 MCP 태스크 시퀀스 |
| `docs/mcp/scene-setup-tasks.md` | 씬 전체 구성 맥락 (Player 배치 섹션 1.3) |
| `docs/systems/project-structure.md` | 폴더 구조, 네이밍 규칙 |
| `docs/systems/tutorial-system.md` | 첫 플레이 동선 — 이동 튜토리얼 연동 |
| `docs/content/npcs.md` | E키 인터랙션 NPC 목록 |

---

## 12. Open Questions

- [OPEN] 달리기(SprintSpeed) 기능 채택 여부 미결정 — Phase 2 테스트 후 결정 (→ see `docs/systems/player-character.md` 섹션 10)
- [OPEN] NPC 충돌 레이어 처리 방식 미결정 — 통과 허용 vs 밀어내기 (→ see `docs/systems/player-character.md` 섹션 2.3)
- [OPEN] 계절별 캐릭터 의상 자동 교체를 위한 MaterialSwapper 컴포넌트 필요 여부 — Phase 2 확장 항목
- [OPEN] 멀티 슬롯 확장(6번 이상) 구현 방식 — UI 시스템과 함께 Phase 2에서 결정

---

## 13. Risks

- [RISK] Cinemachine Dead Zone/Damping 파라미터는 실제 플레이 테스트 없이 확정 불가. `PlayerData` SO의 초기값으로 입력하되, Phase 2 반복 테스트 후 조정 필요.
- [RISK] `Physics.OverlapSphere` 기반 인터랙션 감지는 타일 밀도가 높은 경우(8x8 그리드) GC Alloc을 유발할 수 있다. `NonAlloc` 변형(`OverlapSphereNonAlloc`) 사용을 Phase 2 구현 시 적용 권장.
- [RISK] 로우폴리 캐릭터 모델을 MCP만으로 생성하는 데 품질 한계 존재. Unity Asset Store 외부 에셋 활용을 Phase 2 시작 시 검토 필요 (→ see `docs/systems/player-character.md` 섹션 11 Risks).
- [RISK] AnimationEvent 기반 `UnlockMovement()` 호출은 애니메이션 클립이 MCP로 생성된 placeholder인 경우 이벤트가 없을 수 있다. Phase B 클립 생성 단계에서 AnimationEvent 삽입 여부를 명시적으로 확인해야 한다.
- [RISK] Rigidbody `FreezePositionY` 설정 시 경사면이 있는 지형(미래 농장 확장 씬)에서 이동 불가 현상이 발생할 수 있다. 초기 씬(평탄 지형)에서는 문제없으나 Phase 2 농장 확장 시 재검토 필요.
