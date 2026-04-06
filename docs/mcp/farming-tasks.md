# MCP 농장 그리드 생성 태스크 시퀀스

> 농장 그리드 타일 64개 배치, 작물 데이터 SO 생성, 플레이어-타일 상호작용 연결을 MCP for Unity를 통해 단계적으로 구현하는 태스크 시퀀스  
> 작성: Claude Code (Opus) | 2026-04-06  
> 문서 ID: ARC-003

---

## Context

이 문서는 `docs/mcp/scene-setup-tasks.md`(ARC-002)의 Phase B에서 생성된 빈 FarmGrid 오브젝트 위에 실제 타일 64개를 배치하고, 작물 ScriptableObject 데이터를 생성하며, 플레이어-타일 상호작용을 연결하는 MCP 태스크 시퀀스를 정의한다.

ARC-002가 씬의 **골격**(빈 계층 구조, 카메라, 라이팅)을 담당했다면, 이 문서는 경작 시스템의 **살**(타일, 작물 데이터, 도구 연결)을 채운다.

---

# Part I: 게임 디자인 -- 타일, 작물, 도구 상호작용

---

## 1. 타일 시각적 표현 스펙

### 1.1 타일 기본 형태

- **메시**: Quad (1m x 1m, XZ 평면에 배치)
- **배치**: `FarmGrid` 하위에 64개(`Tile_0_0` ~ `Tile_7_7`), 위치 `(x * 1.0, 0.001, y * 1.0)`
- **Collider**: BoxCollider (레이캐스트 타일 선택용)
- **타일 간격**: Scale (0.95, 0.95, 1) — 0.05m 간격으로 그리드 시인성 확보

### 1.2 상태별 머티리얼/색상

각 TileState에 대응하는 URP/Lit 머티리얼. 모든 머티리얼은 `Assets/_Project/Materials/Terrain/` 에 배치.

| TileState | 머티리얼명 | Base Color (Hex) | Smoothness | 시각적 의도 |
|-----------|-----------|-----------------|------------|-------------|
| `Empty` | `M_Soil_Empty` | `#8B7355` | 0.2 | 마른 흙. 경작되지 않은 자연 상태 |
| `Tilled` | `M_Soil_Tilled` | `#5C4033` | 0.3 | 갈아엎은 어두운 흙 |
| `Planted` | `M_Soil_Tilled` | (동일) | (동일) | Tilled와 동일. 시각적 차이는 Seed 프리팹이 담당 |
| `Watered` | `M_Soil_Watered` | `#3B2A1A` | 0.5 | 젖은 진한 갈색. Smoothness로 촉촉한 광택 |
| `Dry` | `M_Soil_Dry` | `#A0896C` | 0.15 | 물 증발. Empty보다 약간 밝은 갈색 |
| `Harvestable` | `M_Soil_Watered` | (동일) | (동일) | Watered와 동일. 작물 프리팹이 시각적 차별화 |
| `Withered` | `M_Soil_Withered` | `#9E9E9E` | 0.1 | 회갈색. 생기 없는 토양 |

(-> see `docs/systems/farming-architecture.md` 섹션 7, Phase A Step A-4)

### 1.3 상태 전환 시각 피드백

| 전환 | Phase A 피드백 (머티리얼 교체) | 후속 추가 피드백 |
|------|------------------------------|-----------------|
| Empty -> Tilled | `M_Soil_Empty` -> `M_Soil_Tilled` | 흙 파티클 상승 |
| Tilled -> Planted | 머티리얼 유지 + Seed 프리팹 Instantiate | 씨앗 낙하 애니메이션 |
| Planted -> Watered | `M_Soil_Tilled` -> `M_Soil_Watered` | 물방울 파티클 |
| Watered -> Dry | `M_Soil_Watered` -> `M_Soil_Dry` | 수증기 파티클 |
| Dry -> Watered | `M_Soil_Dry` -> `M_Soil_Watered` | 물방울 파티클 |
| Watered -> Harvestable | 머티리얼 유지 + 프리팹 교체(Harvestable 단계) | 스케일 바운스 |
| Dry -> Withered | `M_Soil_Dry` -> `M_Soil_Withered` + 프리팹 갈변 | 잎 낙하 파티클 |
| Withered -> Tilled | `M_Soil_Withered` -> `M_Soil_Tilled` + 프리팹 Destroy | 먼지 파티클 |
| Harvestable -> Tilled | `M_Soil_Watered` -> `M_Soil_Tilled` + 프리팹 Destroy | 작물 튀어오름 + 아이템 아이콘 |

---

## 2. 작물 성장 시각적 스펙

### 2.1 공통 규칙

모든 작물은 4단계(Seed/Sprout/Growing/Harvestable)의 placeholder 프리팹을 갖는다. Unity 기본 프리미티브(Capsule, Sphere) 기반.

- **프리팹 네이밍**: `PFB_Crop_{CropId}_Stage{0~3}` (-> see `docs/systems/project-structure.md` 섹션 6)
- **단계 전환**: `currentGrowthDays / totalGrowthDays` 비율 기반 (-> see `docs/systems/crop-growth.md` 섹션 1.1)

### 2.2 단계별 공통 스케일

| 단계 | 영문 키 | 프리미티브 | Scale | 시각적 의도 |
|------|---------|-----------|-------|-------------|
| 씨앗 | `Seed` | Sphere | (0.1, 0.1, 0.1) | 작은 돌기 |
| 새싹 | `Sprout` | Capsule | (0.2, 0.3, 0.2) | 줄기가 올라온 모습 |
| 성장 | `Growing` | Capsule | (0.3, 0.5, 0.3) | 잎과 줄기 자람 |
| 수확 가능 | `Harvestable` | Capsule + Sphere(열매) | (0.4, 0.7, 0.4) | 열매가 달린 완성 모습 |

### 2.3 작물별 색상 스펙

(-> see `docs/systems/farming-architecture.md` 섹션 7, Phase B Step B-3)

| 작물 | 초기 머티리얼 (Green) | 성숙 머티리얼 (열매) |
|------|----------------------|---------------------|
| 감자 (Potato) | `M_Crop_Potato_Early` #4CAF50 | `M_Crop_Potato_Mature` #8D6E63 |
| 당근 (Carrot) | `M_Crop_Carrot_Early` #66BB6A | `M_Crop_Carrot_Mature` #FF9800 |
| 토마토 (Tomato) | `M_Crop_Tomato_Early` #81C784 | `M_Crop_Tomato_Mature` #F44336 |

### 2.4 고사(Withered) 시각 처리

별도 프리팹 없이 현재 단계 프리팹에 변형 적용:
- 색상: 모든 Renderer를 `#6D4C41` (짙은 갈색)으로 Lerp (t=0.8)
- 스케일 Y축: 현재 스케일의 0.7배
- 회전: Z축 15~25도 랜덤 기울임

---

## 3. 도구 상호작용 디자인

### 3.1 상호작용 요약 매트릭스

| | Empty | Tilled | Planted | Watered | Dry | Harvestable | Withered | Building |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| **호미** | Tilled | X | X | X | X | X | X | X |
| **물뿌리개** | X | X | Watered | (중복) | Watered | X | X | X |
| **씨앗** | X | Planted | X | X | X | X | X | X |
| **낫** | X | X | 소멸 | 소멸 | 소멸 | Tilled+수확 | Tilled | X |
| **손** | 정보 | 정보 | 정보 | 정보 | 정보 | 수확 | 정보 | 정보 |

(O = 상태 전환, X = 실패. 상세: `docs/systems/farming-system.md` 섹션 3)

### 3.2 성공 피드백 요약

| 도구 | 성공 피드백 | 슬롯 |
|------|-----------|------|
| 호미 | 흙 파티클 + 머티리얼 변경 + "쓱" 효과음 | 1 |
| 물뿌리개 | 물방울 파티클 + 머티리얼 Watered 교체 + "철철" | 2 |
| 씨앗봉투 | Seed 프리팹 출현 + "톡" + 인벤토리 씨앗 -1 | 3 |
| 낫 (수확) | 프리팹 튀어오름 + 아이템 아이콘 날아감 + 징글 | 4 |
| 낫 (고사 제거) | 갈색 먼지 파티클 + "바스락" | 4 |
| 손 (수확) | 낫과 동일 (에너지 소모 없음) | 5 |

---

## 4. 초기 작물 데이터

`docs/design.md` 섹션 4.2를 canonical source로 사용.

| 필드 | 감자 | 당근 | 토마토 |
|------|------|------|--------|
| cropName | "감자" | "당근" | "토마토" |
| cropId | "potato" | "carrot" | "tomato" |
| seedPrice | 15G | 15G | 25G |
| sellPrice | 30G | 35G | 60G |
| growthDays | 3 | 3 | 5 |
| allowedSeasons | Spring | Spring, Autumn | Summer |
| unlockLevel | 0 (시작) | 0 (시작) | 2 |
| growthStagePrefabs | Stage0~3 | Stage0~3 | Stage0~3 |

(seedPrice canonical: `docs/systems/farming-architecture.md` Phase B Step B-1)

[OPEN] design.md 4.2 테이블에 seedPrice 열을 추가하여 canonical source를 일원화할 필요가 있다.

---

## 5. 테스트 시나리오

### 5.1 전체 작물 생명주기

| ID | 시나리오 | 기대 결과 |
|----|----------|----------|
| T-F1 | 감자: 경작->심기->물주기->3일 성장->수확 | 상태 전환 8단계 완전 동작, 머티리얼/프리팹 교체 |
| T-F2 | 물 미제공 고사: Watered -> Dry 3일 -> Withered | 정확히 3일 Dry에서 Withered 전환 |
| T-F3 | 경작지 퇴화: Tilled 3일 방치 -> Empty | neglectDays=3에서 Empty 복귀 |
| T-F4 | 토마토 성장 단계: Day0(Seed)->Day1(Sprout)->Day2~4(Growing)->Day5(Harvestable) | 프리팹 4단계 순차 교체 |

### 5.2 도구 상호작용

| ID | 시나리오 | 기대 결과 |
|----|----------|----------|
| T-C1 | 호미 -> Empty 타일 | Empty -> Tilled, 머티리얼 변경 |
| T-C2 | 씨앗 -> Tilled 타일 | Tilled -> Planted, Seed 프리팹 |
| T-C3 | 물뿌리개 -> Planted 타일 | Planted -> Watered |
| T-C4 | 호미 -> Tilled 타일 (실패) | 상태 변화 없음 |
| T-C5 | 낫 -> Harvestable 타일 | 수확 + Tilled 복귀 |
| T-C6 | 낫 -> Withered 타일 | 고사체 제거 + Tilled 복귀 |

### 5.3 에지 케이스

| ID | 시나리오 | 기대 결과 |
|----|----------|----------|
| T-E1 | Watered 타일에 물뿌리개 재사용 | 에너지 소모, 상태 변화 없음 |
| T-E2 | 씨앗 0개 상태에서 심기 시도 | 실패, "씨앗이 없습니다" |
| T-E3 | 그리드 외부 클릭 | 아무 반응 없음 |

---

# Part II: 기술 아키텍처 -- MCP 태스크 시퀀스

---

## 6. 사전 조건

### 6.1 ARC-002 완료 상태

| Phase | 상태 | 핵심 결과물 |
|-------|------|------------|
| Phase A | 완료 필수 | 폴더 구조 (`Assets/_Project/` 전체) |
| Phase B | 완료 필수 | SCN_Farm: FarmSystem > FarmGrid, GrowthSystem, Player 등 |

### 6.2 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트 | ARC-002 Step |
|----------|-------------|
| `FarmGrid` | B-4-02 |
| `GrowthSystem` | B-4-03 |
| `Player`, `PlayerModel`, `PlayerController`, `ToolSystem` | B-5 |
| `Main Camera`, `Sun`, `GroundPlane`, `M_Grass` | B-7~B-9 |

### 6.3 필요 C# 스크립트 (MCP 실행 전 작성 필요)

| # | 파일 경로 | 클래스 | 네임스페이스 |
|---|----------|--------|-------------|
| S-01 | `Scripts/Farm/FarmGrid.cs` | `FarmGrid` | `SeedMind.Farm` |
| S-02 | `Scripts/Farm/FarmTile.cs` | `FarmTile` | `SeedMind.Farm` |
| S-03 | `Scripts/Farm/CropInstance.cs` | `CropInstance` | `SeedMind.Farm` |
| S-04 | `Scripts/Farm/GrowthSystem.cs` | `GrowthSystem` | `SeedMind.Farm` |
| S-05 | `Scripts/Farm/FarmEvents.cs` | `FarmEvents` | `SeedMind.Farm` |
| S-06 | `Scripts/Farm/Data/CropData.cs` | `CropData` | `SeedMind.Farm.Data` |
| S-07 | `Scripts/Farm/Data/TileState.cs` | `TileState` | `SeedMind.Farm.Data` |
| S-08 | `Scripts/Farm/Data/FertilizerData.cs` | `FertilizerData` | `SeedMind.Farm.Data` |
| S-09 | `Scripts/Player/ToolSystem.cs` | `ToolSystem` | `SeedMind.Player` |
| S-10 | `Scripts/Player/PlayerController.cs` | `PlayerController` | `SeedMind.Player` |
| S-11 | `Scripts/Player/Data/ToolData.cs` | `ToolData` | `SeedMind.Player.Data` |

(모든 경로 접두어: `Assets/_Project/`)

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: TileState.cs -> CropData.cs -> CropInstance.cs -> FarmTile.cs -> FarmGrid.cs -> GrowthSystem.cs -> FarmEvents.cs -> ToolData.cs -> ToolSystem.cs -> PlayerController.cs

---

## 7. MCP 도구 매핑

`scene-setup-tasks.md` 섹션 7 매핑 기반 확장.

| MCP 도구 | 용도 | 사용 Phase |
|----------|------|-----------|
| `create_material` | 머티리얼 에셋 생성 | A-1, B-1, C-4 |
| `create_primitive` | Quad/Capsule/Sphere 생성 | A-2, B-2, C-4 |
| `set_property` | 프로퍼티 설정 | A-2~C-5 전체 |
| `add_component` | 스크립트 컴포넌트 부착 | A-3~C-2 |
| `remove_component` | 불필요 컴포넌트 제거 | A-2, B-2 |
| `set_material` | Renderer에 머티리얼 할당 | A-2, B-2, C-4 |
| `save_scene` | 씬 저장 | A-6, B-5, C-6, D-7 |
| `enter_play_mode` / `exit_play_mode` | 검증 | D |
| `create_scriptable_object` (가칭) | SO 인스턴스 생성 | B-3, B-4 |
| `save_as_prefab` (가칭) | 프리팹 에셋 저장 | B-2 |

---

## 8. Phase별 태스크 시퀀스

### Phase A: 그리드 타일 생성

**목표**: FarmGrid 하위에 8x8 = 64개 Quad, FarmTile 컴포넌트, 토양 머티리얼 할당.

#### A-1. 토양 머티리얼 생성

```
A-1-01: create_material -> "M_Soil_Empty", #8B7355, Assets/_Project/Materials/Terrain/
A-1-02: create_material -> "M_Soil_Tilled", #5C4033
A-1-03: create_material -> "M_Soil_Watered", #3B2A1A
A-1-04: create_material -> "M_Soil_Dry", #A0896C
A-1-05: create_material -> "M_Soil_Withered", #9E9E9E
```

- **MCP 호출**: 5회

#### A-2. 그리드 타일 64개 생성

반복 패턴 (x: 0~7, y: 0~7, 총 64회):

```
각 타일 (7 MCP 호출):
  (a) create_primitive -> type: "Quad", name: "Tile_{x}_{y}", parent: "FarmGrid"
  (b) set_property -> Transform.localPosition = (x, 0.001, y)
  (c) set_property -> Transform.localRotation = (90, 0, 0)
  (d) set_property -> Transform.localScale = (0.95, 0.95, 1)
  (e) remove_component -> MeshCollider (Quad 기본)
  (f) add_component -> BoxCollider
  (g) set_material -> "M_Soil_Empty"
```

- **MCP 호출**: 64 x 7 = 448회
- [RISK] 호출량이 큼. Editor 스크립트로 대체 시 ~50회로 감소 가능.

#### A-3. FarmTile 컴포넌트 부착

```
각 타일 (3~4 MCP 호출):
  (a) add_component -> "SeedMind.Farm.FarmTile"
  (b) set_property -> FarmTile._state = 0 (Empty)
  (c) set_property -> FarmTile.gridX = {x}
  (d) set_property -> FarmTile.gridY = {y}
```

GridPosition은 MCP 호환을 위해 `gridX`/`gridY` 두 int 필드로 분리.

- **MCP 호출**: 64 x 4 = 256회

#### A-4. FarmGrid 컴포넌트 설정

```
A-4-01: add_component -> "FarmGrid", "SeedMind.Farm.FarmGrid"
A-4-02: set_property -> gridWidth = 8
A-4-03: set_property -> gridHeight = 8
```

- **MCP 호출**: 3회

#### A-5. GrowthSystem 연결

```
A-5-01: add_component -> "GrowthSystem", "SeedMind.Farm.GrowthSystem"
A-5-02: set_property -> farmGrid = [FarmGrid 참조]
        [RISK] 오브젝트 참조 설정 MCP 가능 여부 미확인. 대안: FindObjectOfType<FarmGrid>()
```

- **MCP 호출**: 2회

#### A-6. 씬 저장

- **MCP 호출**: 1회

#### Phase A 검증 체크리스트

- [ ] FarmGrid 하위에 `Tile_0_0` ~ `Tile_7_7` 총 64개
- [ ] 각 타일: Position (x, 0.001, y), Rotation (90,0,0), Scale (0.95,0.95,1)
- [ ] 모든 타일에 `M_Soil_Empty` 적용, FarmTile 컴포넌트 부착
- [ ] FarmGrid: gridWidth=8, gridHeight=8
- [ ] 콘솔 에러 없음

---

### Phase B: 작물 데이터 생성

**목표**: CropData SO 3종, placeholder 프리팹 12개, ToolData SO 5종.

#### B-1. 작물 머티리얼 6종

```
B-1-01~06: create_material (6회)
  M_Crop_Potato_Early (#4CAF50), M_Crop_Potato_Mature (#8D6E63)
  M_Crop_Carrot_Early (#66BB6A), M_Crop_Carrot_Mature (#FF9800)
  M_Crop_Tomato_Early (#81C784), M_Crop_Tomato_Mature (#F44336)
  저장: Assets/_Project/Materials/Crops/
```

#### B-2. Placeholder 프리팹 12개

3종 x 4단계. 각 프리팹 생성 -> 스케일/머티리얼 설정 -> 프리팹으로 저장 -> 씬에서 삭제.

```
단계별 규격:
  Stage0: Sphere (0.1, 0.1, 0.1) + Early 머티리얼
  Stage1: Capsule (0.2, 0.3, 0.2) + Early
  Stage2: Capsule (0.3, 0.5, 0.3) + Mature
  Stage3: Capsule (0.4, 0.7, 0.4) + Mature
  저장: Assets/_Project/Prefabs/Crops/PFB_Crop_{crop}_Stage{n}.prefab
```

- **MCP 호출**: 12 x 6 = 72회
- [RISK] `save_as_prefab` 도구 가용 여부 미확인. 대안: PrefabUtility Editor 스크립트.

#### B-3. CropData SO 3종

```
SO_Crop_Potato: cropName="감자", cropId="potato", seedPrice=15, sellPrice=30,
                growthDays=3, allowedSeasons=1(Spring), unlockLevel=0
SO_Crop_Carrot: cropName="당근", cropId="carrot", seedPrice=15, sellPrice=35,
                growthDays=3, allowedSeasons=5(Spring|Autumn), unlockLevel=0
SO_Crop_Tomato: cropName="토마토", cropId="tomato", seedPrice=25, sellPrice=60,
                growthDays=5, allowedSeasons=2(Summer), unlockLevel=2
저장: Assets/_Project/Data/Crops/
```

- **MCP 호출**: 3 x 13 = 39회
- [RISK] SO 배열/참조 필드(growthStagePrefabs) 설정이 최대 리스크 항목.

#### B-4. ToolData SO 5종

```
SO_Tool_Hoe_T1: toolName="호미", toolType=0(Hoe), tier=1, range=1
SO_Tool_WateringCan_T1: toolName="물뿌리개", toolType=1, tier=1, range=1
SO_Tool_SeedBag: toolName="씨앗봉투", toolType=2, tier=1, range=1
SO_Tool_Sickle_T1: toolName="낫", toolType=3, tier=1, range=1
SO_Tool_Hand: toolName="손", toolType=4, tier=1, range=1
저장: Assets/_Project/Data/Tools/
```

- **MCP 호출**: 5 x 7 = 35회

#### Phase B 검증 체크리스트

- [ ] 머티리얼 6개, 프리팹 12개, CropData 3개, ToolData 5개 에셋 존재
- [ ] CropData SO 필드값이 design.md 4.2와 일치
- [ ] growthStagePrefabs 배열에 프리팹 4개 연결
- [ ] 콘솔 에러 없음

---

### Phase C: 상호작용 연결

**목표**: Player에 컴포넌트 부착, 도구 데이터 연결, 레이캐스트 설정.

#### C-1. PlayerController 설정

```
C-1-01: add_component -> "Player", "SeedMind.Player.PlayerController"
C-1-02: set_property -> moveSpeed = 5.0
C-1-03: add_component -> "Player", "CharacterController"
C-1-04~06: CharacterController height=0.8, radius=0.3, center=(0, 0.4, 0)
```

- **MCP 호출**: 6회

#### C-2. ToolSystem 설정

```
C-2-01: add_component -> "Player", "SeedMind.Player.ToolSystem"
C-2-02: set_property -> tools = [SO_Tool_Hoe_T1, ..., SO_Tool_Hand]
        [RISK] SO 배열 참조 설정. B-3와 동일 리스크.
C-2-03: set_property -> currentToolIndex = 0
```

- **MCP 호출**: 4회

#### C-3. 타일 레이어 설정

```
64개 타일의 Layer를 "FarmTile" (User Layer)로 설정
[RISK] 커스텀 레이어 등록은 수동 필요. MCP는 할당만 수행.
```

- **MCP 호출**: 64회

#### C-4. PlayerModel Placeholder

```
C-4-01: create_primitive -> Capsule "PlayerBody", parent: "PlayerModel"
C-4-02~03: Scale (0.4, 0.4, 0.4), Position (0, 0.4, 0)
C-4-04~05: create_material "M_Player" #2196F3 + set_material
```

- **MCP 호출**: 5회

#### C-5. Player 초기 위치

```
C-5-01: set_property -> "Player", Transform.position = (3.5, 0, 3.5)
```

#### Phase C 검증 체크리스트

- [ ] Player: PlayerController + CharacterController + ToolSystem 컴포넌트
- [ ] ToolSystem.tools에 5종 ToolData 연결
- [ ] PlayerModel에 파란색 Capsule placeholder
- [ ] 모든 타일 레이어 = FarmTile
- [ ] Player 위치 (3.5, 0, 3.5)

---

### Phase D: 통합 검증

```
D-1: open_scene, get_object_info
D-2: enter_play_mode -> 콘솔 로그 확인
D-3: 도구-타일 상호작용 테스트 (콘솔 커맨드 또는 수동)
D-4: 머티리얼 전환 시각 검증
D-5: 프리팹 교체 시각 검증
D-6: 고사 경로 검증 (Dry 3일 -> Withered)
D-7: exit_play_mode, save_scene
```

[RISK] Play Mode 중 입력 시뮬레이션이 MCP로 불가능할 수 있음. 테스트용 Debug 메서드 필요.

#### Phase D 검증 체크리스트

- [ ] Play Mode 콘솔 에러 없음
- [ ] 경작 루프 완전 동작 (Empty->Tilled->Planted->Watered->Harvestable->Tilled)
- [ ] 고사 경로 동작 (Dry 3일 -> Withered -> 낫으로 Tilled)
- [ ] 머티리얼/프리팹 시각 전환 확인

---

## 9. 의존성 그래프

```
[사전 조건]
  ARC-002 Phase A+B ──────────────────────┐
  S-01~S-11 (스크립트 컴파일) ────────────┤
                                           │
[Phase A: 그리드 타일] ◀───────────────────┘
  A-1 (머티리얼) ──> A-2 (Quad 64개) ──> A-3 (FarmTile) ──> A-4/A-5 ──> A-6
                                                                         │
[Phase B: 작물 데이터] (A-1~B-1은 병렬 가능)                              │
  B-1 (머티리얼) ──> B-2 (프리팹) ──> B-3 (CropData SO)                  │
  B-4 (ToolData SO) (독립)                                               │
                                                                         │
[Phase C: 상호작용] ◀── A + B 모두 완료 ──────────────────────────────────┘
  C-1~C-5 ──> C-6 (저장)
                    │
[Phase D: 검증] ◀── C 완료 ──────────────────────────────────────────────┘
```

---

## 10. 예상 MCP 호출 수

| Phase | 호출 수 | 최적화 후 (Editor 스크립트) |
|-------|--------|--------------------------|
| A (그리드) | ~713 | ~60 |
| B (작물 데이터) | ~153 | ~100 |
| C (상호작용) | ~80 | ~80 |
| D (검증) | ~15 | ~15 |
| **합계** | **~960** | **~255** |

**권장**: 타일 64개 생성(A-2, A-3)은 Editor 스크립트 1회 실행으로 대체. 총 호출 ~255회, 예상 소요 15~25분.

---

## 11. 스크립트 MCP 접근 필드 요약

MCP `set_property`로 설정이 필요한 핵심 필드:

| 스크립트 | 필드 | 타입 | Phase | 설정 값 |
|----------|------|------|-------|---------|
| FarmGrid | gridWidth, gridHeight | int | A-4 | 8, 8 |
| FarmTile | _state | int | A-3 | 0 (Empty) |
| FarmTile | gridX, gridY | int | A-3 | 각 타일 좌표 |
| GrowthSystem | farmGrid | FarmGrid ref | A-5 | 오브젝트 참조 |
| PlayerController | moveSpeed | float | C-1 | 5.0 |
| ToolSystem | tools | ToolData[] | C-2 | SO 참조 5개 |
| ToolSystem | currentToolIndex | int | C-2 | 0 |

---

## Open Questions

- [OPEN] design.md 4.2에 seedPrice 열 추가 필요 (canonical source 일원화).
- [OPEN] MCP `create_primitive`에서 초기 위치/회전/스케일 파라미터 지원 여부. Phase A 최적화에 결정적.
- [OPEN] MCP `set_property`로 SO의 배열/오브젝트 참조 필드 설정 가능 여부.
- [OPEN] 타일 생성을 MCP 개별 호출 vs Editor 스크립트 일괄 실행 중 어느 쪽 채택할지.
- [OPEN] FarmTile.GridPosition을 Vector2Int 단일 필드 vs gridX/gridY 분리. MCP 호환성 관점에서 분리 제안.
- [해결] Planted/Watered/Dry 타일에 낫 사용 시 작물 소멸 (crop-growth.md 섹션 1.3 정책에 맞춰 통일). 도구 매트릭스 업데이트 완료.

## Risks

- [RISK] MCP 호출 볼륨 (~960회). Editor 스크립트 대체 강력 권장.
- [RISK] 프리팹 저장 도구(`save_as_prefab`) 부재 가능성.
- [RISK] SO 배열/참조 필드 설정 불가 가능성 (최대 리스크).
- [RISK] 커스텀 레이어 등록 수동 필요.
- [RISK] 스크립트 컴파일 에러 시 전체 add_component 실패.
- [RISK] Play Mode 입력 시뮬레이션 불가. 테스트 Debug 메서드 필요.
- [RISK] Empty(#8B7355)와 Dry(#A0896C) 색상 차이가 작아 모니터 환경에 따라 구분 어려울 수 있음.

---

## Cross-references

- `docs/design.md` 4.1절 (경작 시스템), 4.2절 (작물 종류/가격 canonical)
- `docs/architecture.md` 4.1절 (Farm Grid), 4.2절 (작물 성장)
- `docs/systems/farming-system.md` 섹션 1 (타일 크기), 섹션 2 (상태 머신), 섹션 3 (도구)
- `docs/systems/farming-architecture.md` 섹션 2 (TileState enum), 섹션 4 (CropData SO 스키마), 섹션 7 (MCP Phase A~C 개요)
- `docs/systems/crop-growth.md` 섹션 1 (성장 4단계), 섹션 1.2 (시각 변화)
- `docs/systems/crop-growth-architecture.md` 섹션 1~2 (CropInstance/CropData 클래스)
- `docs/systems/project-structure.md` 섹션 1 (폴더), 섹션 6 (네이밍)
- `docs/mcp/scene-setup-tasks.md` (ARC-002, 선행 작업)
- `docs/balance/crop-economy.md` (작물 경제 밸런스, 작성 예정 -- BAL-001)

---

*이 문서는 Claude Code가 게임 디자인 및 기술 아키텍처 관점에서 자율적으로 작성했습니다.*
