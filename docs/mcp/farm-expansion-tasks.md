# 농장 확장(Zone) 시스템 MCP 태스크 시퀀스 (ARC-025)

> FarmZoneManager, ZoneData SO, 구역 해금 흐름, 장애물 개간, 세이브/로드, ZoneData SO 에셋 생성을 MCP for Unity 태스크로 상세 정의
> 작성: Claude Code (Opus 4.6) | 2026-04-07
> Phase 1 | 문서 ID: ARC-025

---

## 1. 개요

### 1.1 목적

이 문서는 `docs/systems/farm-expansion-architecture.md`(ARC-023) Part II에서 요약된 MCP 구현 계획(Phase A~E)을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 농장 확장 시스템의 데이터 레이어(ZoneData SO), 시스템 레이어(FarmZoneManager, ZoneEvents), 씬 배치, FarmGrid 확장, 기존 시스템 연동(GameSaveData, ToolSystem), 장애물 개간 시스템을 완성한다.

### 1.2 의존성

```
농장 확장 시스템 MCP 태스크 의존 관계:
├── SeedMind.Core     (TimeManager, SaveManager, EventBus, ISaveable)
├── SeedMind.Farm     (FarmGrid, FarmTile, TileState, FarmEvents)
├── SeedMind.Level    (ProgressionManager, UnlockType)
├── SeedMind.Economy  (EconomyManager -- 구역 구매 골드 차감)
├── SeedMind.Player   (InventoryManager -- 개간 드랍 아이템 추가)
└── SeedMind.UI       (ZoneMapUI, ZoneUnlockPanel -- 본 태스크에서는 UI 생성 제외)
```

(-> see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 1.3 전제 조건 (완료된 태스크 의존성)

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조, SCN_Farm 기본 계층 (MANAGERS, FARM, PLAYER, UI), Canvas_HUD, Canvas_Overlay |
| ARC-003 | `docs/mcp/farming-tasks.md` | Phase A~C 전체 | FarmGrid, FarmTile, TileState, FarmEvents, ToolSystem |
| ARC-013 | `docs/mcp/inventory-tasks.md` | Phase A~C 전체 | InventoryManager, PlayerInventory -- 개간 드랍 수령 |

### 1.4 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `Canvas_HUD` (HUD UI 루트) | ARC-002 Phase B |
| `Canvas_Overlay` (오버레이 UI 루트) | ARC-002 Phase B |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A |
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `--- FARM ---` (씬 계층 부모) | ARC-002 Phase B |
| `FarmGrid` (경작 그리드) | ARC-003 |
| `FarmTile` (타일 컴포넌트) | ARC-003 |
| `EconomyManager` (경제) | economy-architecture.md 기반 |
| `ProgressionManager` (레벨/XP) | BAL-002 기반 |
| `SaveManager` (세이브) | save-load-architecture.md 기반 |
| `InventoryManager` (인벤토리) | ARC-013 |

### 1.5 총 MCP 호출 예상 수

| 태스크 | 설명 | MCP 호출 수 |
|--------|------|------------|
| Z-1 | 준비 작업 -- 폴더 구조, asmdef 확인 | 4회 |
| Z-2 | Enum/타입 정의 스크립트 생성 | 6회 |
| Z-3 | ZoneData ScriptableObject 스크립트 생성 | 3회 |
| Z-4 | FarmZoneManager 기본 구조 스크립트 생성 | 2회 |
| Z-5 | 구역 해금 흐름 -- FarmGrid 확장, ToolSystem 라우팅 | 3회 |
| Z-6 | 장애물 개간 시스템 -- 프리팹 생성, ObstacleContainer 배치 | 8회 |
| Z-7 | 세이브/로드 통합 -- ZoneSaveData, ISaveable, GameSaveData 확장 | 5회 |
| Z-8 | ZoneData SO 에셋 생성 -- Zone A~G 7개 SO + Inspector 설정 | 56회 |
| Z-9 | 씬 배치 및 통합 테스트 | 12회 |
| **합계** | | **~99회** |

[RISK] Z-8(SO 에셋 생성)의 56회가 전체의 과반이다. Editor 스크립트(CreateZoneAssets.cs)로 일괄 생성하면 ~8회로 감소 가능. 단, `set_property`로 Vector2Int 배열(tilePositions)을 설정할 수 있는지 MCP for Unity에서 확인 필요.

---

## 2. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋/스크립트 폴더 생성 | Z-1 |
| `create_script` | C# 스크립트 파일 생성 | Z-2, Z-3, Z-4, Z-5, Z-6, Z-7 |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | Z-8 |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | Z-5, Z-6, Z-8 |
| `create_object` | 빈 GameObject 생성 | Z-6, Z-9 |
| `add_component` | MonoBehaviour 컴포넌트 부착 | Z-9 |
| `set_parent` | 오브젝트 부모 설정 | Z-6, Z-9 |
| `create_primitive` | Placeholder 프리팹용 기본 도형 생성 | Z-6 |
| `save_scene` | 씬 저장 | Z-6, Z-9 |
| `save_prefab` | 장애물 Placeholder 프리팹 저장 | Z-6 |
| `modify_script` | 기존 스크립트 수정 (FarmGrid 확장, ToolSystem 라우팅) | Z-5 |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | Z-9 |
| `execute_method` | Editor/런타임 메서드 호출 (머티리얼 생성 포함) | Z-6, Z-9 |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | Z-9 |

> [RISK] `create_material`은 MCP for Unity 표준 도구가 아니다. Z-6-01 머티리얼 생성은 Editor 스크립트 + `execute_method` 방식으로 대체한다.

[RISK] `set_property`로 `Vector2Int[]` 배열 필드(ZoneData.tilePositions)를 직접 설정할 수 있는지 불확실. MCP for Unity가 배열/복합 구조체 설정을 지원하지 않을 경우, Editor 스크립트 또는 코드 내 초기화 fallback이 필요하다. (-> see `docs/systems/farm-expansion-architecture.md` Risks 섹션 항목 2)

---

## 3. 필요 C# 스크립트 목록

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 태스크 순서대로 작성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/Farm/ZoneState.cs` | `ZoneState` (enum) | `SeedMind.Farm` | Z-2 Phase 1 |
| S-02 | `Scripts/Farm/ZoneType.cs` | `ZoneType` (enum) | `SeedMind.Farm` | Z-2 Phase 1 |
| S-03 | `Scripts/Farm/ObstacleType.cs` | `ObstacleType` (enum) | `SeedMind.Farm` | Z-2 Phase 1 |
| S-04 | `Scripts/Farm/ClearResult.cs` | `ClearResult` (enum) | `SeedMind.Farm` | Z-2 Phase 1 |
| S-05 | `Scripts/Farm/ZoneUnlockFailReason.cs` | `ZoneUnlockFailReason` (enum) | `SeedMind.Farm` | Z-2 Phase 1 |
| S-06 | `Scripts/Farm/ZoneEvents.cs` | `ZoneEvents` (static class) | `SeedMind.Farm` | Z-2 Phase 2 |
| S-07 | `Scripts/Farm/ZoneData.cs` | `ZoneData` (SO) | `SeedMind.Farm` | Z-3 Phase 1 |
| S-08 | `Scripts/Farm/ObstacleEntry.cs` | `ObstacleEntry` (struct) | `SeedMind.Farm` | Z-3 Phase 1 |
| S-09 | `Scripts/Farm/ObstacleInstance.cs` | `ObstacleInstance` (Plain C#) | `SeedMind.Farm` | Z-3 Phase 2 |
| S-10 | `Scripts/Farm/ZoneRuntimeState.cs` | `ZoneRuntimeState` (Plain C#) | `SeedMind.Farm` | Z-3 Phase 2 |
| S-11 | `Scripts/Farm/FarmZoneManager.cs` | `FarmZoneManager` (MonoBehaviour Singleton) | `SeedMind.Farm` | Z-4 |
| S-12 | `Scripts/Save/ZoneSaveData.cs` | `ZoneSaveData` (Serializable) | `SeedMind.Save` | Z-7 Phase 1 |
| S-13 | `Scripts/Save/ZoneEntrySaveData.cs` | `ZoneEntrySaveData` (Serializable) | `SeedMind.Save` | Z-7 Phase 1 |
| S-14 | `Scripts/Save/ObstacleSaveData.cs` | `ObstacleSaveData` (Serializable) | `SeedMind.Save` | Z-7 Phase 1 |

(모든 경로 접두어: `Assets/_Project/`)

**컴파일 순서**: S-01~S-05 -> S-06 -> S-07~S-10 -> S-11 -> S-12~S-14. 각 Phase 사이에 Unity 컴파일 대기가 필요하다.

[RISK] S-11(FarmZoneManager)은 SeedMind.Core, SeedMind.Economy, SeedMind.Level에 대한 Singleton 런타임 접근이 필요하다. asmdef 의존성 누락 시 컴파일 실패한다. 현재 `SeedMind.Farm.asmdef`이 이미 존재하므로 별도 asmdef 생성은 불필요하나, 의존성 목록에 SeedMind.Economy, SeedMind.Level이 포함되어 있는지 확인 필요. (-> see `docs/systems/farm-expansion-architecture.md` 섹션 10.1)

---

## 4. 태스크 목록

---

### Z-1: 준비 작업 -- 폴더 구조 확인 및 생성

**목적**: 농장 확장 시스템에 필요한 에셋/프리팹 폴더를 생성하고, 기존 SeedMind.Farm asmdef 의존성을 확인한다.

**전제**: ARC-002(scene-setup-tasks.md)의 폴더 구조가 완성된 상태.

#### Z-1-01: 데이터 폴더 생성

**목적**: ZoneData SO 에셋을 저장할 폴더 생성
**MCP 호출**: `mcp__unity__create_folder`
**입력**:
- path: `"Assets/_Project/Data/Zones"`
**완료 기준**: 폴더가 생성되어 Project 패널에 표시됨

#### Z-1-02: 장애물 프리팹 폴더 생성

**목적**: 장애물 Placeholder 프리팹을 저장할 폴더 생성
**MCP 호출**: `mcp__unity__create_folder`
**입력**:
- path: `"Assets/_Project/Prefabs/Obstacles"`
**완료 기준**: 폴더가 생성되어 Project 패널에 표시됨

#### Z-1-03: 장애물 머티리얼 폴더 생성

**목적**: 장애물 종류별 Placeholder 머티리얼을 저장할 폴더 생성
**MCP 호출**: `mcp__unity__create_folder`
**입력**:
- path: `"Assets/_Project/Materials/Obstacles"`
**완료 기준**: 폴더가 생성되어 Project 패널에 표시됨

#### Z-1-04: SeedMind.Farm asmdef 의존성 확인

**목적**: 기존 `SeedMind.Farm.asmdef`에 SeedMind.Economy, SeedMind.Level 의존성이 포함되어 있는지 확인. 누락 시 추가.
**MCP 호출**: `mcp__unity__modify_script` (필요 시)
**입력**:
- path: `"Assets/_Project/Scripts/Farm/SeedMind.Farm.asmdef"`
- 확인 필드: `references` 배열에 `"SeedMind.Core"`, `"SeedMind.Economy"`, `"SeedMind.Level"` 포함 여부
**완료 기준**: asmdef references에 세 네임스페이스 모두 포함. 이미 포함되어 있으면 수정 불필요.

**Z-1 합계**: 4회

---

### Z-2: Enum/타입 정의 스크립트 생성

**목적**: 농장 확장 시스템에서 사용하는 모든 enum과 정적 이벤트 클래스를 생성한다.

**전제**: Z-1 완료 (폴더 구조 확인)

#### Z-2-01: ZoneState enum

**목적**: 구역의 현재 상태를 나타내는 enum 생성
**MCP 호출**: `mcp__unity__create_script`
**입력**:
- path: `"Assets/_Project/Scripts/Farm/ZoneState.cs"`
- content: ZoneState enum (Locked, Unlockable, Unlocked, FullyCleared)
  // -> see docs/systems/farm-expansion-architecture.md 섹션 2.1
**완료 기준**: 스크립트 생성 완료, Unity 컴파일 에러 없음

#### Z-2-02: ZoneType enum

**목적**: 구역의 용도/특성을 정의하는 enum 생성
**MCP 호출**: `mcp__unity__create_script`
**입력**:
- path: `"Assets/_Project/Scripts/Farm/ZoneType.cs"`
- content: ZoneType enum (Farmland, Orchard, Pasture, Greenhouse, Pond)
  // -> see docs/systems/farm-expansion-architecture.md 섹션 2.2
**완료 기준**: 스크립트 생성 완료, Unity 컴파일 에러 없음

#### Z-2-03: ObstacleType enum

**목적**: 장애물 종류를 정의하는 enum 생성
**MCP 호출**: `mcp__unity__create_script`
**입력**:
- path: `"Assets/_Project/Scripts/Farm/ObstacleType.cs"`
- content: ObstacleType enum (Weed, SmallRock, LargeRock, Stump, SmallTree, LargeTree, Bush)
  // -> see docs/systems/farm-expansion-architecture.md 섹션 2.3
  // 장애물 종류 및 도구 요건 -> see docs/systems/farm-expansion.md 섹션 3.1
**완료 기준**: 스크립트 생성 완료, Unity 컴파일 에러 없음

#### Z-2-04: ClearResult enum

**목적**: 장애물 개간 시도 결과를 나타내는 enum 생성
**MCP 호출**: `mcp__unity__create_script`
**입력**:
- path: `"Assets/_Project/Scripts/Farm/ClearResult.cs"`
- content: ClearResult enum (NoObstacle, AlreadyCleared, WrongTool, Hit, Cleared)
  // -> see docs/systems/farm-expansion-architecture.md 섹션 5.4
**완료 기준**: 스크립트 생성 완료, Unity 컴파일 에러 없음

#### Z-2-05: ZoneUnlockFailReason enum

**목적**: 구역 해금 실패 사유를 나타내는 enum 생성
**MCP 호출**: `mcp__unity__create_script`
**입력**:
- path: `"Assets/_Project/Scripts/Farm/ZoneUnlockFailReason.cs"`
- content: ZoneUnlockFailReason enum (LevelInsufficient, InsufficientGold, AlreadyUnlocked, PrerequisiteZone)
  // -> see docs/systems/farm-expansion-architecture.md 섹션 4.3
**완료 기준**: 스크립트 생성 완료, Unity 컴파일 에러 없음

#### Z-2-06: ZoneEvents 정적 이벤트 클래스

**목적**: 구역 해금/개간 관련 이벤트를 중앙 집중 관리하는 정적 클래스 생성
**MCP 호출**: `mcp__unity__create_script`
**입력**:
- path: `"Assets/_Project/Scripts/Farm/ZoneEvents.cs"`
- content: ZoneEvents (static class)
  이벤트:
  - `OnZoneUnlocked` -- Action<string, ZoneData>
  - `OnZoneUnlockFailed` -- Action<string, ZoneUnlockFailReason>
  - `OnObstacleHit` -- Action<Vector2Int, int>
  - `OnObstacleCleared` -- Action<Vector2Int, ObstacleType>
  - `OnZoneFullyCleared` -- Action<string>
  // -> see docs/systems/farm-expansion-architecture.md 섹션 7.1
**완료 기준**: 스크립트 생성 완료, Unity 컴파일 에러 없음

**Z-2 합계**: 6회
**검증**: Unity 컴파일 대기 -> 에러 없음 확인

---

### Z-3: ZoneData ScriptableObject 스크립트 생성

**목적**: 구역 정적 데이터(SO), 장애물 배치 데이터(struct), 런타임 상태 클래스를 생성한다.

**전제**: Z-2 완료 (ZoneState, ZoneType, ObstacleType enum 컴파일 완료)

**의존 태스크**: Z-2

#### Z-3-01: ZoneData SO + ObstacleEntry struct

**목적**: 구역 정적 데이터 ScriptableObject와 장애물 배치 구조체 생성
**MCP 호출**: `mcp__unity__create_script` x 2회
**입력**:
- path 1: `"Assets/_Project/Scripts/Farm/ZoneData.cs"`
  content: ZoneData ScriptableObject
  ```
  [CreateAssetMenu(fileName = "NewZone", menuName = "SeedMind/ZoneData")]
  필드:
    zoneId: string                  // 고유 식별자
    zoneName: string                // UI 표시용
    sortOrder: int                  // 해금 순서, UI 정렬용
    requiredLevel: int              // → see docs/systems/farm-expansion.md 섹션 2.1
    unlockCost: int                 // → see docs/systems/farm-expansion.md 섹션 2.1
    prerequisiteZoneId: string      // 선행 구역 ID (null이면 선행 없음)
    zoneType: ZoneType              // 구역 용도
    tilePositions: Vector2Int[]     // 이 구역에 속하는 타일 좌표 목록
    obstacleMap: ObstacleEntry[]    // 초기 장애물 배치
    lockedOverlayMaterial: Material // 잠금 상태 오버레이
    unlockVFXPrefab: GameObject     // 해금 시 이펙트
  ```
  // -> see docs/systems/farm-expansion-architecture.md 섹션 3

- path 2: `"Assets/_Project/Scripts/Farm/ObstacleEntry.cs"`
  content: ObstacleEntry (Serializable struct)
  ```
  필드:
    localPosition: Vector2Int       // 구역 내 상대 좌표
    type: ObstacleType              // 장애물 종류
    maxHP: int                      // 제거 필요 타격 수 → see docs/systems/farm-expansion.md 섹션 3.1
    lootDropIds: string[]           // 제거 시 드랍 아이템 ID → see docs/systems/farm-expansion.md 섹션 3.4
    obstaclePrefab: GameObject      // 장애물 3D 모델
  ```
  // -> see docs/systems/farm-expansion-architecture.md 섹션 3
**완료 기준**: 두 스크립트 생성 완료, Unity 컴파일 에러 없음

#### Z-3-02: ObstacleInstance + ZoneRuntimeState

**목적**: 장애물/구역의 런타임 상태를 관리하는 Plain C# 클래스 생성
**MCP 호출**: `mcp__unity__create_script` x 1회 (두 클래스를 별도 파일로)
**입력**:
- path 1: `"Assets/_Project/Scripts/Farm/ObstacleInstance.cs"`
  content: ObstacleInstance (Plain C# class)
  ```
  필드:
    entry: ObstacleEntry            // 원본 정적 데이터 참조
    position: Vector2Int            // FarmGrid 절대 좌표
    currentHP: int                  // 남은 HP
    isCleared: bool                 // 제거 여부
    droppedLoot: bool               // 드랍 처리 완료 여부
  ```
  // -> see docs/systems/farm-expansion-architecture.md 섹션 1

**MCP 호출 추가**: `mcp__unity__create_script` x 1회
- path 2: `"Assets/_Project/Scripts/Farm/ZoneRuntimeState.cs"`
  content: ZoneRuntimeState (Plain C# class)
  ```
  필드:
    zoneId: string
    state: ZoneState
    clearedObstacleCount: int
    totalObstacleCount: int
  ```
  // -> see docs/systems/farm-expansion-architecture.md 섹션 1
**완료 기준**: 두 스크립트 생성 완료, Unity 컴파일 에러 없음

**Z-3 합계**: 3회 (ZoneData 1 + ObstacleEntry 1 + ObstacleInstance/ZoneRuntimeState 동시 또는 순차 생성)
**검증**: Unity 컴파일 대기 -> 에러 없음 확인. 특히 ZoneData가 ObstacleEntry를 참조하므로 동일 Phase에서 함께 생성해야 한다.

---

### Z-4: FarmZoneManager 기본 구조 스크립트 생성

**목적**: 구역 해금/상태 관리, 개간 처리, 이벤트 발행, 세이브/로드를 담당하는 핵심 매니저 MonoBehaviour를 생성한다.

**전제**: Z-3 완료 (ZoneData, ObstacleEntry, ObstacleInstance, ZoneRuntimeState 컴파일 완료)

**의존 태스크**: Z-2, Z-3

#### Z-4-01: FarmZoneManager MonoBehaviour

**목적**: 농장 확장 시스템의 핵심 매니저 스크립트 생성
**MCP 호출**: `mcp__unity__create_script`
**입력**:
- path: `"Assets/_Project/Scripts/Farm/FarmZoneManager.cs"`
- content: FarmZoneManager (MonoBehaviour, Singleton, ISaveable)
  ```
  상태:
    _zones: ZoneData[]                              // Inspector에서 할당할 전체 구역 SO 목록
    _zoneStates: Dictionary<string, ZoneRuntimeState> // 런타임 상태
    _obstacleInstances: Dictionary<Vector2Int, ObstacleInstance> // 장애물 런타임 맵

  참조:
    _farmGrid: FarmGrid                             // 타일 활성화 위임

  메서드:
    Initialize(): void                              // 초기화 -- 구역 상태 딕셔너리 구축
    TryUnlockZone(string zoneId): bool              // 구역 해금 시도
    IsZoneUnlocked(string zoneId): bool             // 해금 여부 확인
    GetZoneState(string zoneId): ZoneState           // 구역 상태 조회
    ClearObstacle(Vector2Int tilePos, ToolType tool, int toolTier): ClearResult
    GetZoneForTile(Vector2Int tilePos): ZoneData     // 타일 좌표로 구역 조회
    GetObstacleAt(Vector2Int tilePos): ObstacleInstance // 타일의 장애물 조회
    SpawnObstacles(ZoneData zone): void              // 구역 해금 시 장애물 인스턴스 생성
    CanToolClear(ObstacleType obstacle, ToolType tool, int toolTier): bool
    CheckZoneFullyCleared(string zoneId): void       // 완전 개간 체크
    GetSaveData(): ZoneSaveData                      // ISaveable 구현
    LoadSaveData(ZoneSaveData data): void            // ISaveable 구현

  ISaveable:
    SaveLoadOrder => 45                              // → see docs/systems/farm-expansion-architecture.md 섹션 9.3
  ```
  // -> see docs/systems/farm-expansion-architecture.md 섹션 1, 4.2, 5.3
**완료 기준**: 스크립트 생성 완료, Unity 컴파일 에러 없음. 특히 asmdef 의존성(SeedMind.Core, SeedMind.Economy, SeedMind.Level) 확인

#### Z-4-02: FarmGrid partial class 확장 스크립트

**목적**: 기존 FarmGrid에 구역 시스템 지원 메서드를 추가하는 partial class 파일 생성
**MCP 호출**: `mcp__unity__create_script`
**입력**:
- path: `"Assets/_Project/Scripts/Farm/FarmGrid.Zone.cs"`
- content: FarmGrid (partial class)
  ```
  메서드:
    ActivateZoneTiles(Vector2Int[] positions): void   // 타일 활성화
    DeactivateZoneTiles(Vector2Int[] positions): void // 타일 비활성화
    InitializeFullGrid(int maxWidth, int maxHeight): void // 최대 크기 사전 할당
  ```
  // -> see docs/systems/farm-expansion-architecture.md 섹션 6
**완료 기준**: 스크립트 생성 완료, Unity 컴파일 에러 없음. 기존 FarmGrid.cs와 partial class로 결합 확인

**Z-4 합계**: 2회
**검증**: Unity 컴파일 대기 -> 에러 없음 확인

---

### Z-5: 구역 해금 흐름 -- ToolSystem 라우팅 및 EconomyManager/ProgressionManager 연동

**목적**: ToolSystem의 도구 사용 흐름에 장애물 체크 분기를 추가하고, FarmZoneManager의 TryUnlockZone이 EconomyManager/ProgressionManager와 올바르게 연동되도록 한다.

**전제**: Z-4 완료 (FarmZoneManager, FarmGrid.Zone 컴파일 완료)

**의존 태스크**: Z-4, ARC-003 (ToolSystem)

#### Z-5-01: ToolSystem에 장애물 라우팅 추가

**목적**: ToolSystem.UseCurrentTool() 흐름에서 장애물이 있는 타일을 감지하여 FarmZoneManager.ClearObstacle()로 분기
**MCP 호출**: `mcp__unity__modify_script`
**입력**:
- path: `"Assets/_Project/Scripts/Farm/ToolSystem.cs"` (기존 파일)
- 수정 내용:
  ```
  // UseCurrentTool(Vector3 worldPos) 내부, 기존 switch 문 이전에 삽입:
  // [NEW] 장애물 체크
  ObstacleInstance obs = FarmZoneManager.Instance.GetObstacleAt(gridPos);
  if (obs != null && !obs.isCleared)
  {
      ClearResult result = FarmZoneManager.Instance.ClearObstacle(
          gridPos, currentTool.toolType, currentTool.tier);
      // → see docs/systems/farm-expansion-architecture.md 섹션 8
      return; // 기존 타일 액션 실행하지 않음
  }
  ```
**완료 기준**: ToolSystem이 장애물 존재 시 ClearObstacle을 우선 호출하고, 기존 타일 액션을 건너뜀

#### Z-5-02: FarmZoneManager.TryUnlockZone 내부 연동 확인

**목적**: TryUnlockZone 메서드가 ProgressionManager.IsUnlocked()와 EconomyManager.SpendGold()를 올바르게 호출하는지 코드 검증
**MCP 호출**: `mcp__unity__modify_script` (필요 시 수정)
**입력**:
- path: `"Assets/_Project/Scripts/Farm/FarmZoneManager.cs"`
- 검증 항목:
  - `ProgressionManager.Instance.IsUnlocked(UnlockType.FarmExpansion, zoneId)` 호출 존재
  - `EconomyManager.Instance.SpendGold(zone.unlockCost, ...)` 호출 존재
  - 실패 시 `ZoneEvents.OnZoneUnlockFailed` 발행
  - 성공 시 `ZoneEvents.OnZoneUnlocked` 발행 + `ProgressionManager.Instance.RegisterUnlock()` 호출
  // -> see docs/systems/farm-expansion-architecture.md 섹션 4.2
**완료 기준**: TryUnlockZone의 흐름이 아키텍처 문서의 시퀀스 다이어그램(섹션 4.1)과 일치

#### Z-5-03: FarmZoneManager.Initialize에 선행 구역 체크 로직 추가

**목적**: Initialize() 시 각 구역의 prerequisiteZoneId를 검사하여 ZoneState를 올바르게 설정 (선행 구역 미해금 시 Locked 유지)
**MCP 호출**: `mcp__unity__modify_script`
**입력**:
- path: `"Assets/_Project/Scripts/Farm/FarmZoneManager.cs"`
- 수정 내용:
  ```
  // Initialize() 내부 구역 상태 결정 로직:
  // 1) zone.prerequisiteZoneId가 null/empty이면 → 레벨만 체크
  // 2) zone.prerequisiteZoneId가 있으면 → 선행 구역 해금 여부 추가 체크
  // → see docs/systems/farm-expansion.md 섹션 2.3 (분기 구조)
  ```
**완료 기준**: Zone D/E/F가 Zone C 미해금 시 Locked 상태 유지, Zone G가 Zone D 미해금 시 Locked 상태 유지

**Z-5 합계**: 3회
**검증**: Unity 컴파일 대기 -> 에러 없음 확인

---

### Z-6: 장애물 개간 시스템 -- 프리팹 생성 및 ObstacleContainer 배치

**목적**: 7종 장애물의 Placeholder 프리팹과 머티리얼을 생성하고, 씬에 ObstacleContainer 부모 오브젝트를 배치한다.

**전제**: Z-3 완료 (ObstacleType, ObstacleEntry 컴파일 완료)

**의존 태스크**: Z-1 (폴더 구조), Z-2 (ObstacleType enum)

#### Z-6-01: 장애물 머티리얼 생성 (4종)

**목적**: 장애물 종류별 Placeholder 머티리얼 생성
**MCP 호출**: `mcp__unity__create_script` + `mcp__unity__execute_method` (머티리얼 생성 전용 MCP 도구가 없으므로 Editor 스크립트 경유)

[RISK] `mcp__unity__create_material`은 MCP for Unity 표준 도구 목록에 없다. 대신 Editor 스크립트를 생성하고 `execute_method`로 호출하여 머티리얼을 프로그래밍 방식으로 생성해야 한다.
**입력**:
- `"Assets/_Project/Materials/Obstacles/M_Obstacle_Weed.mat"` -- 색상: 녹색 (0.2, 0.6, 0.1)
- `"Assets/_Project/Materials/Obstacles/M_Obstacle_Rock.mat"` -- 색상: 회색 (0.5, 0.5, 0.5)
- `"Assets/_Project/Materials/Obstacles/M_Obstacle_Wood.mat"` -- 색상: 갈색 (0.5, 0.3, 0.1)
- `"Assets/_Project/Materials/Obstacles/M_Obstacle_Bush.mat"` -- 색상: 진한 녹색 (0.1, 0.4, 0.05)
**완료 기준**: 4개 머티리얼이 Project 패널에 표시됨

#### Z-6-02: 장애물 Placeholder 프리팹 생성 (7종)

**목적**: 각 ObstacleType에 대응하는 Placeholder 프리팹 생성
**MCP 호출**: `mcp__unity__create_primitive` + `mcp__unity__save_prefab` (각 2회 x 3종 = 6회, 나머지 4종은 기존 프리팹 복제/수정)

실제로는 7종 모두 개별 생성:
- `PFB_Obstacle_Weed` -- Quad, scale (0.3, 0.3, 0.3), M_Obstacle_Weed
- `PFB_Obstacle_SmallRock` -- Sphere, scale (0.4, 0.3, 0.4), M_Obstacle_Rock
- `PFB_Obstacle_LargeRock` -- Sphere, scale (0.8, 0.6, 0.8), M_Obstacle_Rock
- `PFB_Obstacle_Stump` -- Cylinder, scale (0.5, 0.3, 0.5), M_Obstacle_Wood
- `PFB_Obstacle_SmallTree` -- Cylinder, scale (0.3, 0.8, 0.3), M_Obstacle_Wood
- `PFB_Obstacle_LargeTree` -- Cylinder, scale (0.5, 1.5, 0.5), M_Obstacle_Wood
- `PFB_Obstacle_Bush` -- Sphere, scale (0.5, 0.4, 0.5), M_Obstacle_Bush

저장 경로: `"Assets/_Project/Prefabs/Obstacles/"`

**완료 기준**: 7개 프리팹이 Project 패널에 표시됨

[RISK] `create_primitive`로 생성한 오브젝트에 머티리얼을 할당한 후 프리팹화하는 과정이 MCP 단일 호출로 불가능할 수 있다. 대안: `create_object` + MeshFilter/MeshRenderer 수동 설정.

#### Z-6-03: ObstacleContainer 씬 오브젝트 생성

**목적**: 장애물 프리팹 인스턴스의 부모가 될 빈 GameObject를 씬에 배치
**MCP 호출**: `mcp__unity__create_object` + `mcp__unity__set_parent`
**입력**:
- name: `"ObstacleContainer"`
- parent: `"--- FARM ---"`
**완료 기준**: SCN_Farm 씬 계층에 `--- FARM --- > ObstacleContainer` 표시

**Z-6 합계**: ~8회 (머티리얼 4 + 프리팹 일부 + ObstacleContainer 1)
**검증**: 모든 프리팹이 올바른 머티리얼을 참조하고, ObstacleContainer가 씬에 배치됨

---

### Z-7: 세이브/로드 통합

**목적**: 구역 해금 상태와 장애물 상태를 저장/로드하는 데이터 클래스를 생성하고, 기존 GameSaveData에 통합한다. PATTERN-005(JSON/C# 동기화)를 준수한다.

**전제**: Z-4 완료 (FarmZoneManager 컴파일 완료)

**의존 태스크**: Z-4

#### Z-7-01: ZoneSaveData 관련 클래스 생성 (PATTERN-005 준수)

**목적**: 구역 세이브 데이터의 JSON 스키마와 C# 클래스를 동기화된 형태로 생성
**MCP 호출**: `mcp__unity__create_script` x 3회
**입력**:
- path 1: `"Assets/_Project/Scripts/Save/ZoneSaveData.cs"`
  content: ZoneSaveData (Serializable)
  ```
  필드:
    zones: ZoneEntrySaveData[]    // 구역별 세이브 데이터 배열
  ```

- path 2: `"Assets/_Project/Scripts/Save/ZoneEntrySaveData.cs"`
  content: ZoneEntrySaveData (Serializable)
  ```
  필드:
    zoneId: string                // ZoneData.zoneId 참조
    isUnlocked: bool              // 해금 여부
    obstacles: ObstacleSaveData[] // 장애물 상태 배열
  ```

- path 3: `"Assets/_Project/Scripts/Save/ObstacleSaveData.cs"`
  content: ObstacleSaveData (Serializable)
  ```
  필드:
    posX: int                     // 타일 X 좌표
    posY: int                     // 타일 Y 좌표
    isCleared: bool               // 제거 여부
    currentHP: int                // 남은 HP (isCleared=true이면 0)
  ```

**PATTERN-005 검증**:
- ZoneSaveData: JSON 필드 1개(zones) = C# 필드 1개(zones) -- 일치
- ZoneEntrySaveData: JSON 필드 3개(zoneId, isUnlocked, obstacles) = C# 필드 3개 -- 일치
- ObstacleSaveData: JSON 필드 4개(posX, posY, isCleared, currentHP) = C# 필드 4개 -- 일치

// -> see docs/systems/farm-expansion-architecture.md 섹션 9.1

**완료 기준**: 3개 스크립트 생성 완료, Unity 컴파일 에러 없음

#### Z-7-02: GameSaveData에 farmZones 필드 추가

**목적**: 기존 GameSaveData 클래스에 ZoneSaveData 필드를 추가
**MCP 호출**: `mcp__unity__modify_script`
**입력**:
- path: `"Assets/_Project/Scripts/Save/GameSaveData.cs"` (기존 파일)
- 추가 필드:
  ```
  public ZoneSaveData farmZones;    // -> see docs/systems/farm-expansion-architecture.md 섹션 9.2
  // null 허용: 이전 세이브 호환. null 시 초기 구역만 해금된 기본 상태로 초기화
  ```
**완료 기준**: GameSaveData에 farmZones 필드 추가 완료, 기존 필드 미변경

#### Z-7-03: FarmZoneManager ISaveable 구현 검증

**목적**: Z-4-01에서 생성한 FarmZoneManager의 ISaveable 구현이 올바른지 검증
**MCP 호출**: `mcp__unity__modify_script` (필요 시 수정)
**입력**:
- path: `"Assets/_Project/Scripts/Farm/FarmZoneManager.cs"`
- 검증 항목:
  - `SaveLoadOrder => 45` 설정 확인 // -> see docs/systems/farm-expansion-architecture.md 섹션 9.3
  - `GetSaveData()`: 해금된 구역 + 미제거 장애물만 직렬화
  - `LoadSaveData()`: null-safe 처리 (farmZones == null 시 Zone A만 해금)
  - 로드 시 `FarmGrid.ActivateZoneTiles()` 호출하여 해금 구역 타일 활성화
  - 로드 시 장애물 인스턴스 복원 (savedObstacles 기반)
**완료 기준**: 세이브 → 로드 → 구역 상태 유지 확인 (Z-9 통합 테스트에서 최종 검증)

**Z-7 합계**: 5회
**검증**: Unity 컴파일 대기 -> 에러 없음 확인

---

### Z-8: ZoneData SO 에셋 생성 -- Zone A~G 7개 SO

**목적**: 7개 구역(Zone A~G)의 ZoneData ScriptableObject 에셋을 생성하고 필드값을 설정한다. 모든 수치(비용, 레벨 요건, 타일 수, 장애물 수)는 canonical 문서를 참조한다.

**전제**: Z-3 Phase 1 완료 (ZoneData.cs, ObstacleEntry.cs 컴파일 완료), Z-6 완료 (장애물 프리팹 생성 완료)

**의존 태스크**: Z-3, Z-6

#### Z-8-01: SO_Zone_Home (Zone A -- 초기 농장)

**목적**: 초기 농장 구역 SO 에셋 생성 및 설정
**MCP 호출**: `mcp__unity__create_scriptable_object` + `mcp__unity__set_property` x N회
**입력**:
```
create_scriptable_object
  type: "SeedMind.Farm.ZoneData"
  asset_path: "Assets/_Project/Data/Zones/SO_Zone_Home.asset"

set_property  target: "SO_Zone_Home"
  zoneId = "zone_home"
  zoneName = "초기 농장"
  sortOrder = 0                                    // -> see docs/systems/farm-expansion.md 섹션 1.3
  requiredLevel = 0                                // 시작 구역 (-> see docs/systems/farm-expansion.md 섹션 2.1)
  unlockCost = 0                                   // 시작 구역 (-> see docs/systems/farm-expansion.md 섹션 2.1)
  prerequisiteZoneId = ""                          // 선행 구역 없음
  zoneType = 0                                     // Farmland (-> see docs/systems/farm-expansion-architecture.md 섹션 2.2)
  tilePositions = [(0,0)~(7,7)]                    // 64타일 (-> see docs/systems/farm-expansion.md 섹션 1.3)
  obstacleMap = [장애물 배치]                       // -> see docs/systems/farm-expansion.md 섹션 3.2 (잡초 x5, 소형 돌 x3)
```
**완료 기준**: SO 에셋 생성 + 필드값 설정 완료, Inspector에서 값 확인 가능

#### Z-8-02: SO_Zone_SouthPlain (Zone B -- 남쪽 평야)

**목적**: 1단계 확장 구역 SO 에셋 생성 및 설정
**MCP 호출**: `mcp__unity__create_scriptable_object` + `mcp__unity__set_property` x N회
**입력**:
```
create_scriptable_object
  type: "SeedMind.Farm.ZoneData"
  asset_path: "Assets/_Project/Data/Zones/SO_Zone_SouthPlain.asset"

set_property  target: "SO_Zone_SouthPlain"
  zoneId = "zone_south_plain"
  zoneName = "남쪽 평야"
  sortOrder = 1                                    // -> see docs/systems/farm-expansion.md 섹션 1.3
  requiredLevel = 0                                // -> see docs/systems/farm-expansion.md 섹션 2.1 (레벨 요건 없음)
  unlockCost = 0                                   // -> see docs/systems/farm-expansion.md 섹션 2.1 (500G -- 실제 값은 canonical 참조)
  prerequisiteZoneId = "zone_home"                 // -> see docs/systems/farm-expansion.md 섹션 2.1
  zoneType = 0                                     // Farmland
  tilePositions = [...]                            // 64타일 (-> see docs/systems/farm-expansion.md 섹션 1.2, 1.3)
  obstacleMap = [장애물 배치]                       // -> see docs/systems/farm-expansion.md 섹션 3.2 (잡초 x8, 소형 돌 x5, 덤불 x3)
```
**완료 기준**: SO 에셋 생성 + 필드값 설정 완료

#### Z-8-03: SO_Zone_NorthPlain (Zone C -- 북쪽 평야)

**목적**: 2단계 확장 구역 SO 에셋 생성 및 설정
**MCP 호출**: `mcp__unity__create_scriptable_object` + `mcp__unity__set_property` x N회
**입력**:
```
create_scriptable_object
  type: "SeedMind.Farm.ZoneData"
  asset_path: "Assets/_Project/Data/Zones/SO_Zone_NorthPlain.asset"

set_property  target: "SO_Zone_NorthPlain"
  zoneId = "zone_north_plain"
  zoneName = "북쪽 평야"
  sortOrder = 2                                    // -> see docs/systems/farm-expansion.md 섹션 1.3
  requiredLevel = 0                                // -> see docs/systems/farm-expansion.md 섹션 2.1 (레벨 3 -- 실제 값은 canonical 참조)
  unlockCost = 0                                   // -> see docs/systems/farm-expansion.md 섹션 2.1 (1,000G -- 실제 값은 canonical 참조)
  prerequisiteZoneId = "zone_south_plain"          // -> see docs/systems/farm-expansion.md 섹션 2.1, 2.3
  zoneType = 0                                     // Farmland
  tilePositions = [...]                            // 64타일 (-> see docs/systems/farm-expansion.md 섹션 1.2, 1.3)
  obstacleMap = [장애물 배치]                       // -> see docs/systems/farm-expansion.md 섹션 3.2 (그루터기 x6, 소형 돌 x4, 잡초 x5)
```
**완료 기준**: SO 에셋 생성 + 필드값 설정 완료

#### Z-8-04: SO_Zone_EastForest (Zone D -- 동쪽 숲)

**목적**: 3단계 확장 구역 SO 에셋 생성 및 설정
**MCP 호출**: `mcp__unity__create_scriptable_object` + `mcp__unity__set_property` x N회
**입력**:
```
create_scriptable_object
  type: "SeedMind.Farm.ZoneData"
  asset_path: "Assets/_Project/Data/Zones/SO_Zone_EastForest.asset"

set_property  target: "SO_Zone_EastForest"
  zoneId = "zone_east_forest"
  zoneName = "동쪽 숲"
  sortOrder = 3                                    // -> see docs/systems/farm-expansion.md 섹션 1.3
  requiredLevel = 0                                // -> see docs/systems/farm-expansion.md 섹션 2.1 (레벨 5 -- 실제 값은 canonical 참조)
  unlockCost = 0                                   // -> see docs/systems/farm-expansion.md 섹션 2.1 (2,500G -- 실제 값은 canonical 참조)
  prerequisiteZoneId = "zone_north_plain"          // -> see docs/systems/farm-expansion.md 섹션 2.1, 2.3
  zoneType = 0                                     // Farmland
  tilePositions = [...]                            // 96타일 (-> see docs/systems/farm-expansion.md 섹션 1.2, 1.3)
  obstacleMap = [장애물 배치]                       // -> see docs/systems/farm-expansion.md 섹션 3.2 (대형 나무 x4, 소형 나무 x8, 대형 바위 x2, 덤불 x6)
```
**완료 기준**: SO 에셋 생성 + 필드값 설정 완료

[RISK] Zone D의 장애물 배치에 대형 나무(LargeTree)와 대형 바위(LargeRock)가 포함된다. 이들은 2x2 타일을 점유하므로 tilePositions 배열에서 4개 타일을 차지한다. ObstacleEntry.localPosition이 좌하단 기준인지 명확히 정의 필요.

#### Z-8-05: SO_Zone_SouthMeadow (Zone E -- 남쪽 초원/목장)

**목적**: 4단계 확장 구역(목장 전용) SO 에셋 생성 및 설정
**MCP 호출**: `mcp__unity__create_scriptable_object` + `mcp__unity__set_property` x N회
**입력**:
```
create_scriptable_object
  type: "SeedMind.Farm.ZoneData"
  asset_path: "Assets/_Project/Data/Zones/SO_Zone_SouthMeadow.asset"

set_property  target: "SO_Zone_SouthMeadow"
  zoneId = "zone_south_meadow"
  zoneName = "남쪽 초원"
  sortOrder = 4                                    // -> see docs/systems/farm-expansion.md 섹션 1.3
  requiredLevel = 0                                // -> see docs/systems/farm-expansion.md 섹션 2.1 (레벨 6 -- 실제 값은 canonical 참조)
  unlockCost = 0                                   // -> see docs/systems/farm-expansion.md 섹션 2.1 (4,000G -- 실제 값은 canonical 참조)
  prerequisiteZoneId = "zone_north_plain"          // -> see docs/systems/farm-expansion.md 섹션 2.1, 2.3
  zoneType = 2                                     // Pasture (-> see docs/systems/farm-expansion-architecture.md 섹션 2.2)
  tilePositions = [...]                            // 96타일 (-> see docs/systems/farm-expansion.md 섹션 1.2, 1.3)
  obstacleMap = [장애물 배치]                       // -> see docs/systems/farm-expansion.md 섹션 3.2 (잡초 x10, 덤불 x8, 소형 돌 x3)
```
**완료 기준**: SO 에셋 생성 + 필드값 설정 완료

#### Z-8-06: SO_Zone_Pond (Zone F -- 연못 구역)

**목적**: 5단계 확장 구역(연못/낚시) SO 에셋 생성 및 설정
**MCP 호출**: `mcp__unity__create_scriptable_object` + `mcp__unity__set_property` x N회
**입력**:
```
create_scriptable_object
  type: "SeedMind.Farm.ZoneData"
  asset_path: "Assets/_Project/Data/Zones/SO_Zone_Pond.asset"

set_property  target: "SO_Zone_Pond"
  zoneId = "zone_pond"
  zoneName = "연못 구역"
  sortOrder = 5                                    // -> see docs/systems/farm-expansion.md 섹션 1.3
  requiredLevel = 0                                // -> see docs/systems/farm-expansion.md 섹션 2.1 (레벨 5 -- 실제 값은 canonical 참조)
  unlockCost = 0                                   // -> see docs/systems/farm-expansion.md 섹션 2.1 (3,000G -- 실제 값은 canonical 참조)
  prerequisiteZoneId = "zone_north_plain"          // -> see docs/systems/farm-expansion.md 섹션 2.1, 2.3
  zoneType = 4                                     // Pond (-> see docs/systems/farm-expansion-architecture.md 섹션 2.2)
  tilePositions = [...]                            // 96타일 (-> see docs/systems/farm-expansion.md 섹션 1.2, 1.3)
  obstacleMap = [장애물 배치]                       // -> see docs/systems/farm-expansion.md 섹션 3.2 (잡초 x6, 소형 돌 x4, 연못 타일 30타일은 별도 처리)
```
**완료 기준**: SO 에셋 생성 + 필드값 설정 완료

[RISK] Zone F의 연못 타일 30타일은 개간 불가/경작 불가 상태여야 한다. 이를 ZoneData에서 어떻게 표현할지 결정 필요. 방안: (A) 연못 타일을 tilePositions에서 제외, (B) 별도 `pondTilePositions` 필드 추가, (C) 타일에 `TileType.Water` 상태 부여. 현재 아키텍처에서는 명시하지 않았으므로 [OPEN].

#### Z-8-07: SO_Zone_Orchard (Zone G -- 과수원)

**목적**: 6단계 확장 구역(과수원) SO 에셋 생성 및 설정
**MCP 호출**: `mcp__unity__create_scriptable_object` + `mcp__unity__set_property` x N회
**입력**:
```
create_scriptable_object
  type: "SeedMind.Farm.ZoneData"
  asset_path: "Assets/_Project/Data/Zones/SO_Zone_Orchard.asset"

set_property  target: "SO_Zone_Orchard"
  zoneId = "zone_orchard"
  zoneName = "과수원"
  sortOrder = 6                                    // -> see docs/systems/farm-expansion.md 섹션 1.3
  requiredLevel = 0                                // -> see docs/systems/farm-expansion.md 섹션 2.1 (레벨 7 -- 실제 값은 canonical 참조)
  unlockCost = 0                                   // -> see docs/systems/farm-expansion.md 섹션 2.1 (5,000G -- 실제 값은 canonical 참조)
  prerequisiteZoneId = "zone_east_forest"          // -> see docs/systems/farm-expansion.md 섹션 2.1, 2.3
  zoneType = 1                                     // Orchard (-> see docs/systems/farm-expansion-architecture.md 섹션 2.2)
  tilePositions = [...]                            // 96타일 (-> see docs/systems/farm-expansion.md 섹션 1.2, 1.3)
  obstacleMap = [장애물 배치]                       // -> see docs/systems/farm-expansion.md 섹션 3.2 (그루터기 x10, 소형 나무 x6, 잡초 x4)
```
**완료 기준**: SO 에셋 생성 + 필드값 설정 완료

**Z-8 합계**: ~56회 (7개 SO x (1 create + ~7 set_property))

[RISK] 각 SO에 대해 tilePositions 배열(64~96개 Vector2Int)을 MCP set_property로 개별 설정하면 호출 수가 폭발한다. Editor 스크립트(CreateZoneAssets.cs)를 생성하여 좌표를 코드로 할당하는 것이 현실적이다. 이 경우 Z-8은 Editor 스크립트 1개 생성 + execute_method 1회로 대체 가능하며, 총 MCP 호출이 ~56회에서 ~3회로 감소한다.

---

### Z-9: 씬 배치 및 통합 테스트

**목적**: FarmZoneManager를 씬에 배치하고, SO 참조를 연결하며, Play Mode에서 전체 흐름(초기화 → 해금 → 개간 → 세이브/로드)을 테스트한다.

**전제**: Z-1~Z-8 전체 완료

**의존 태스크**: Z-1~Z-8 전체

#### Z-9-01: FarmZoneManager GameObject 생성 및 배치

**목적**: SCN_Farm 씬에 FarmZoneManager를 배치하고 컴포넌트를 부착
**MCP 호출**: `mcp__unity__create_object` + `mcp__unity__add_component` + `mcp__unity__set_parent`
**입력**:
- `create_object`: name = `"FarmZoneManager"`
- `add_component`: type = `"SeedMind.Farm.FarmZoneManager"`
- `set_parent`: parent = `"--- MANAGERS ---"`
**완료 기준**: SCN_Farm 씬 계층에 `--- MANAGERS --- > FarmZoneManager` 표시, FarmZoneManager 컴포넌트 부착됨

#### Z-9-02: FarmZoneManager Inspector 참조 연결

**목적**: FarmZoneManager의 Inspector 필드에 SO 배열과 FarmGrid 참조를 할당
**MCP 호출**: `mcp__unity__set_property` x 2회
**입력**:
- `_zones` 배열: [SO_Zone_Home, SO_Zone_SouthPlain, SO_Zone_NorthPlain, SO_Zone_EastForest, SO_Zone_SouthMeadow, SO_Zone_Pond, SO_Zone_Orchard] (7개)
- `_farmGrid` 참조: FarmGrid 오브젝트
**완료 기준**: Inspector에서 _zones 배열 7개 항목과 _farmGrid 참조 확인

#### Z-9-03: 씬 저장

**목적**: 모든 배치 완료 후 씬 저장
**MCP 호출**: `mcp__unity__save_scene`
**입력**: 없음
**완료 기준**: SCN_Farm.unity 파일 저장 완료

#### Z-9-04: Play Mode 테스트 -- 초기화 확인

**목적**: FarmZoneManager 초기화가 정상 동작하는지 확인
**MCP 호출**: `mcp__unity__enter_play_mode` + `mcp__unity__get_console_logs`
**입력**: 없음
**완료 기준**:
- Console: `"FarmZoneManager initialized, zones=7"`
- `IsZoneUnlocked("zone_home")` == true
- `IsZoneUnlocked("zone_south_plain")` == false
- `GetZoneState("zone_south_plain")` 결과 확인 (Locked 또는 Unlockable -- 레벨 의존)

#### Z-9-05: Play Mode 테스트 -- 구역 해금

**목적**: TryUnlockZone 흐름이 정상 동작하는지 확인
**MCP 호출**: `mcp__unity__execute_method` + `mcp__unity__get_console_logs`
**입력**:
- EconomyManager에 충분한 골드 설정 (테스트용)
- `FarmZoneManager.Instance.TryUnlockZone("zone_south_plain")` 호출
**완료 기준**:
- Console: `"Zone zone_south_plain unlocked, activated N tiles"`
- `IsZoneUnlocked("zone_south_plain")` == true
- ZoneEvents.OnZoneUnlocked 이벤트 발행 확인 (로그)
- 골드 차감 확인

#### Z-9-06: Play Mode 테스트 -- 장애물 개간

**목적**: ClearObstacle 흐름이 정상 동작하는지 확인
**MCP 호출**: `mcp__unity__execute_method` + `mcp__unity__get_console_logs`
**입력**:
- 해금된 Zone B의 장애물이 있는 타일 좌표 지정
- `FarmZoneManager.Instance.ClearObstacle(tilePos, ToolType.Hoe, 1)` 호출
**완료 기준**:
- ClearResult 반환값 확인 (Hit 또는 Cleared)
- 장애물 HP 감소 로그
- 제거 완료 시 ZoneEvents.OnObstacleCleared 이벤트 발행 확인 (로그)
- 드랍 아이템 생성 확인 (로그)

#### Z-9-07: Play Mode 테스트 -- 세이브/로드

**목적**: 구역 상태와 장애물 상태가 저장/로드 후 유지되는지 확인
**MCP 호출**: `mcp__unity__execute_method` x 2회 + `mcp__unity__get_console_logs`
**입력**:
- `SaveManager.Instance.Save()` 호출
- `SaveManager.Instance.Load()` 호출
**완료 기준**:
- Console: `"FarmZoneManager loaded, unlocked zones: N/7"`
- 해금 구역 상태 유지
- 미제거 장애물의 currentHP 유지
- 제거된 장애물이 복원되지 않음

#### Z-9-08: Play Mode 종료

**목적**: 테스트 완료 후 Play Mode 종료
**MCP 호출**: `mcp__unity__exit_play_mode`
**입력**: 없음
**완료 기준**: Editor Mode 복귀

**Z-9 합계**: ~12회

---

## 5. 태스크 실행 순서 요약

```
Z-1: 준비 작업 (4회)
 │
 ▼
Z-2: Enum/타입 정의 (6회)
 │
 ▼
Z-3: ZoneData SO 스크립트 (3회)
 │
 ├──────────────────────┐
 ▼                      ▼
Z-4: FarmZoneManager    Z-6: 장애물 프리팹
     스크립트 (2회)          생성 (8회)
 │                      │
 ▼                      │
Z-5: 해금 흐름 연동     │
     (3회)              │
 │                      │
 ├──────────────────────┘
 ▼
Z-7: 세이브/로드 통합 (5회)
 │
 ▼
Z-8: ZoneData SO 에셋 생성 (56회)
 │
 ▼
Z-9: 씬 배치 및 통합 테스트 (12회)
```

**병렬 실행 가능**: Z-4와 Z-6은 서로 독립적이므로 동시 진행 가능. Z-4는 Z-3에만 의존하고, Z-6은 Z-1(폴더)과 Z-2(ObstacleType enum)에만 의존한다.

---

## Open Questions

1. [OPEN] `set_property`로 Vector2Int[] 배열을 MCP에서 직접 설정 가능한지 확인 필요. 불가 시 Editor 스크립트 fallback 필요 (Z-8)
2. [OPEN] Zone F(연못)의 30타일 물 타일을 ZoneData에서 어떻게 표현할지 미정. 별도 필드 추가 vs TileType 확장 (Z-8-06)
3. [OPEN] 2x2 점유 장애물(LargeRock, LargeTree)의 ObstacleEntry.localPosition 기준점(좌하단 vs 중심) 미정 (Z-8-04)
4. [OPEN] ToolSystem.UseCurrentTool()의 기존 메서드 시그니처가 아키텍처 문서와 일치하는지 ARC-003 확인 필요 (Z-5-01)
5. [OPEN] FarmGrid.InitializeFullGrid()의 maxWidth/maxHeight 값 -- farming-system.md 섹션 1의 16x16과 farm-expansion.md 섹션 1.1의 32x32가 충돌 (-> see docs/systems/farm-expansion.md [OPEN] 항목)

---

## Risks

1. [RISK] **Vector2Int[] 배열 MCP 설정**: tilePositions(64~96개 좌표) 설정이 MCP set_property로 불가능할 경우, Editor 스크립트 fallback이 필요하다. Z-8 전체에 영향 (섹션 2 MCP 도구 매핑)
2. [RISK] **asmdef 순환 참조**: FarmZoneManager가 EconomyManager/ProgressionManager를 Singleton 접근하면 컴파일 의존성은 없으나, 타입 참조(SpendGold 반환값 등)가 필요할 경우 인터페이스 분리 필요 (Z-1-04, -> see ARC-023 섹션 10.1)
3. [RISK] **ToolSystem 수정 범위**: Z-5-01에서 ToolSystem.UseCurrentTool()에 장애물 분기를 삽입하면 기존 경작 흐름에 영향을 줄 수 있다. 회귀 테스트 필수
4. [RISK] **static event 구독 누수**: ZoneEvents의 정적 이벤트가 씬 전환 시 초기화되지 않으면 메모리 누수 발생. FarmEvents와 동일한 초기화 루틴에 ZoneEvents 포함 필요 (-> see farming-architecture.md 섹션 6.3)
5. [RISK] **SO 에셋 56회 set_property**: Z-8의 MCP 호출 수(~56회)가 과다하다. Editor 스크립트(CreateZoneAssets.cs)로 일괄 생성 시 ~3회로 감소 가능. 성능/안정성 면에서 Editor 스크립트 방식 강력 권장

---

## Cross-references

| 관련 문서 | 연관 내용 |
|-----------|-----------|
| `docs/systems/farm-expansion-architecture.md` (ARC-023) | 클래스 다이어그램, enum, ZoneData SO, FarmZoneManager, 세이브/로드 아키텍처 전반 |
| `docs/systems/farm-expansion.md` (DES-012) | Zone A~G 구역 배치, 해금 조건, 장애물 목록, 개간 보상 (canonical) |
| `docs/systems/farming-architecture.md` | FarmGrid, FarmTile, TileState, ToolType, FarmEvents 기존 정의 |
| `docs/systems/farming-system.md` 섹션 1 | 농장 그리드 크기, 초기 확장 방식 |
| `docs/systems/economy-architecture.md` 섹션 1 | EconomyManager.SpendGold() API |
| `docs/systems/save-load-architecture.md` 섹션 2~8 | GameSaveData 구조, ISaveable, SaveLoadOrder |
| `docs/systems/progression-architecture.md` 섹션 3 | UnlockType.FarmExpansion, IsUnlocked API |
| `docs/systems/project-structure.md` 섹션 3, 4 | 네임스페이스, asmdef 구성 |
| `docs/mcp/livestock-tasks.md` (ARC-024) | 동일 패턴의 MCP 태스크 시퀀스 참고 (태스크 형식, SO 에셋 생성 패턴) |
| `docs/mcp/scene-setup-tasks.md` (ARC-002) | 폴더 구조, 씬 계층 (MANAGERS, FARM) |
| `docs/mcp/farming-tasks.md` (ARC-003) | FarmGrid, ToolSystem 기존 구현 |
| `docs/balance/progression-curve.md` | 농장 확장 XP 보상, 레벨별 해금 |

---

*이 문서는 Claude Code가 ARC-025 태스크에 따라 작성했습니다.*
