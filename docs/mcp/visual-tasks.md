# 비주얼 시스템 MCP 태스크 시퀀스 (ARC-049)

> 작성: Claude Code (Sonnet 4.6) | 2026-04-09 | 문서 ID: ARC-049 | Phase 1

---

## Context

이 문서는 비주얼 시스템의 Unity MCP 구현 태스크 시퀀스를 정의한다. `docs/systems/visual-architecture.md` 섹션 7 "MCP 태스크 요약 (Part II)" (Step 1~8)을 독립 문서로 분리하여, URP/Volume 설정 → LightingManager 씬 배치 → SeasonLightingProfile/PaletteData SO 생성 → 날씨 파티클 이펙트 → CropVisual 프리팹 구성 → 머티리얼 생성까지의 전체 MCP 호출 시퀀스를 상세히 기술한다.

**상위 설계 문서**: `docs/systems/visual-architecture.md` (ARC-049 상위 설계)  
**비주얼 수치 canonical 출처**: `docs/systems/visual-guide.md`  
**패턴 참조**: `docs/mcp/energy-tasks.md` (ARC-047), `docs/mcp/economy-tasks.md` (ARC-048)

---

## 1. 개요

### 목적

이 문서는 `docs/systems/visual-architecture.md` 섹션 7에서 요약된 MCP 구현 계획(Step 1~8)을 **독립 태스크 문서**로 분리하여 상세화한다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 비주얼 시스템의 데이터 레이어(SO 에셋, Volume Profile, Material), 시스템 레이어(스크립트), 씬 배치, 파티클 이펙트, CropVisual 프리팹 구성까지 완성한다.

### 의존성

```
비주얼 시스템 MCP 태스크 의존 관계:
├── SeedMind.Core         (Season, DayPhase enum — TimeManager 이벤트 타입)
├── SeedMind.Time         (TimeManager.OnDayPhaseChanged, OnSeasonChanged)
├── SeedMind.Weather      (WeatherSystem.OnWeatherChanged — 날씨 파티클 전환)
├── SeedMind.Farm         (FarmTile — CropVisual 자식 배치, CropInstance 데이터)
└── SeedMind.Visual       (LightingManager, PaletteApplier, WeatherVisualController)
    └── SeedMind.Visual.Data  (SeasonLightingProfile, PaletteData, LightingSnapshot SO)
```

(→ see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 완료된 태스크 의존성

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조, SCN_Farm 기본 계층 (Managers, Environment, UI) |
| ARC-003 | `docs/mcp/farming-tasks.md` | Phase A~C 전체 | FarmingSystem, FarmTile GO, CropInstance 데이터 구조 |
| ARC-019 | `docs/mcp/time-season-tasks.md` | Phase A 이상 | TimeManager, OnDayPhaseChanged, OnSeasonChanged, Season/DayPhase enum |
| ARC-019 | `docs/mcp/time-season-tasks.md` | Phase B 이상 | WeatherSystem, OnWeatherChanged, WeatherType enum |

### 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 | 비고 |
|--------------|------|------|
| `--- ENVIRONMENT ---` (씬 계층 부모) | ARC-002 Phase B | Lighting, WeatherFX GO의 상위 |
| `Lighting` (씬 계층 부모 GO) | ARC-002 Phase B | Sun Directional Light 이미 배치됨 |
| `Sun` (Directional Light GO) | ARC-002 Phase B | VB-02에서 LightingManager 참조 연결만 추가 |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A | |
| `Assets/_Project/Scripts/` 폴더 구조 | ARC-002 Phase A | |
| `Assets/_Project/Materials/` 폴더 | ARC-002 Phase A 또는 scene-setup | VF에서 하위 카테고리 폴더 추가 |
| `TimeManager` (씬 오브젝트) | ARC-019 | VB에서 이벤트 구독 연결만 확인 |
| `WeatherSystem` (씬 오브젝트) | ARC-019 | VD에서 이벤트 구독 연결만 확인 |
| `FarmTile` GO 배열 | ARC-003 | VE에서 CropVisual 자식 프리팹 배치 대상 |

### 총 MCP 호출 예상 수

| 태스크 그룹 | 설명 | 예상 호출 수 | 복잡도 |
|-------------|------|:------------:|:------:|
| VA | URP Asset 설정 + Global/Season Volume Profile 생성 | ~10회 | 중 |
| VB | LightingManager 스크립트 생성 + 씬 배치 + 참조 연결 | ~9회 | 중 |
| VC | SeasonLightingProfile SO 생성 (4계절) + PaletteData SO 생성 (4계절) | ~20회 | 중 |
| VD | WeatherFX 파티클 GO 생성 (5종) + WeatherVisualController 배치 | ~18회 | 중 |
| VE | CropVisual 프리팹 구성 (스크립트 생성 + 프리팹 자식 구조) | ~12회 | 고 |
| VF | 머티리얼 생성 (카테고리별 총 ~15종) | ~20회 | 저 |
| **합계** | | **~89회** | |

[RISK] SO 에셋 필드 중 Color, AnimationCurve, Gradient 타입은 MCP `set_property`로 개별 설정 시 호출 수가 크게 증가한다. Editor 스크립트(`InitVisualSO.cs`) 방식으로 일괄 설정을 강력히 권장한다. 모든 수치는 반드시 `docs/systems/visual-guide.md` 참조 — 임의 기재 금지 (PATTERN-007).

---

## 2. MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋 폴더 및 스크립트 폴더 생성 | VA, VB, VC, VD, VE, VF |
| `create_script` | C# 스크립트 파일 생성 | VB, VD, VE |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | VC |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | VA, VB, VC, VD, VE |
| `create_object` | 빈 GameObject 생성 | VB, VD |
| `add_component` | MonoBehaviour/ParticleSystem 컴포넌트 부착 | VB, VD, VE |
| `set_parent` | 오브젝트 부모 설정 | VB, VD |
| `create_material` | Material 에셋 생성 | VF |
| `create_prefab` | 프리팹 에셋 생성 | VE |
| `save_scene` | 씬 저장 | VB, VD, VE |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | VD, VE |
| `execute_method` | 런타임 조명/날씨 전환 테스트 | VD, VE |
| `get_console_logs` | 콘솔 로그 확인 | VB, VD, VE |
| `execute_menu_item` | 컴파일 대기, Editor 스크립트 실행 | VA, VB, VD, VE, VC |

[RISK] `create_scriptable_object` 도구의 가용 여부 및 파라미터 형식 사전 검증 필요. SO 인스턴스 생성이 MCP에서 미지원인 경우, Editor 스크립트를 통한 우회 필요. (→ see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)

---

## 3. 필요 C# 스크립트 목록

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 태스크 순서대로 생성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| S-01 | `Scripts/Visual/Data/LightingSnapshot.cs` | `LightingSnapshot` (struct) | `SeedMind.Visual.Data` | VB |
| S-02 | `Scripts/Visual/Data/PaletteColorCategory.cs` | `PaletteColorCategory` (enum) | `SeedMind.Visual.Data` | VB |
| S-03 | `Scripts/Visual/Data/SeasonLightingProfile.cs` | `SeasonLightingProfile` (SO) | `SeedMind.Visual.Data` | VB |
| S-04 | `Scripts/Visual/Data/PaletteData.cs` | `PaletteData` (SO) | `SeedMind.Visual.Data` | VB |
| S-05 | `Scripts/Visual/LightingManager.cs` | `LightingManager` (MonoBehaviour) | `SeedMind.Visual` | VB |
| S-06 | `Scripts/Visual/PaletteApplier.cs` | `PaletteApplier` (MonoBehaviour) | `SeedMind.Visual` | VB |
| S-07 | `Scripts/Visual/WeatherVisualController.cs` | `WeatherVisualController` (MonoBehaviour) | `SeedMind.Visual` | VD |
| S-08 | `Scripts/Visual/CropVisual.cs` | `CropVisual` (MonoBehaviour) | `SeedMind.Farm` | VE |

(모든 경로 접두어: `Assets/_Project/`)

[OPEN] `CropVisual.cs` 폴더 배치: `Scripts/Visual/` vs `Scripts/Farm/` — `docs/systems/visual-architecture.md` 섹션 6.2 참조. 현재는 임시로 `Scripts/Visual/`에 배치하되 네임스페이스는 `SeedMind.Farm`으로 설정. 프로토타입 이후 결정 후 이동.

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: S-01~S-04(Data 계층) → S-05~S-06 → (VD에서) S-07 → (VE에서) S-08. 각 그룹 사이에 Unity 컴파일 대기(`execute_menu_item`)가 필요하다.

---

## 4. 태스크 그룹 VA: URP/Volume 설정

**목적**: URP Render Pipeline Asset 설정을 확인하고, Global Volume과 계절별 Volume Profile 에셋을 생성한다.

**전제 조건**:
- ARC-002 완료 (`Assets/_Project/Settings/` 폴더 존재 또는 생성)
- Unity 6 URP 패키지 설치 완료

**예상 MCP 호출**: ~10회

---

### VA-01: Settings 폴더 생성

```
create_folder
  path: "Assets/_Project/Settings"
```

- **MCP 호출**: 1회
- ARC-002에서 이미 생성된 경우 스킵 (중복 생성 금지)

---

### VA-02: URP Asset 설정 확인 및 조정

```
execute_menu_item
  menu: "Edit/Project Settings"
// Graphics 탭 → Scriptable Render Pipeline Settings → URP Asset 지정 확인
```

**URP Asset 설정 방향** (→ see `docs/systems/visual-architecture.md` 섹션 1.1):

| 설정 항목 | 파라미터명 | 참조 |
|-----------|-----------|------|
| Rendering Path | renderingPath | (→ see docs/systems/visual-architecture.md 섹션 1.1) |
| Anti-Aliasing | msaaSampleCount | (→ see docs/systems/visual-architecture.md 섹션 1.1) |
| Shadow Type / Resolution | shadowType, shadowResolution | (→ see docs/systems/visual-architecture.md 섹션 1.1) |
| HDR | hdr | (→ see docs/systems/visual-architecture.md 섹션 1.1) |
| Depth Texture | supportsCameraDepthTexture | (→ see docs/systems/visual-architecture.md 섹션 1.1) |

- **MCP 호출**: 1~2회 (확인 + 조정)

---

### VA-03: Global Volume GameObject 생성

```
create_object
  name: "GlobalVolume"
  parent: "--- ENVIRONMENT ---"

add_component
  gameObject: "GlobalVolume"
  component: "UnityEngine.Rendering.Volume"

set_property
  gameObject: "GlobalVolume"
  component: "Volume"
  property: "isGlobal"
  value: true
```

- **MCP 호출**: 3회

---

### VA-04: Volume Profile 에셋 생성 (Global + 계절별 4개)

```
// [권장] Editor 스크립트 방식으로 일괄 생성
create_script
  path: "Assets/_Project/Editor/InitVolumeProfiles.cs"
  // [MenuItem("SeedMind/Init/VolumeProfiles")]
  // 생성 대상:
  //   Assets/_Project/Settings/VP_Global.asset       (Vignette, Fog)
  //   Assets/_Project/Settings/VP_Season_Spring.asset (Color Grading LUT)
  //   Assets/_Project/Settings/VP_Season_Summer.asset
  //   Assets/_Project/Settings/VP_Season_Autumn.asset
  //   Assets/_Project/Settings/VP_Season_Winter.asset
  // 모든 파라미터 수치: → see docs/systems/visual-guide.md

execute_menu_item
  menu: "SeedMind/Init/VolumeProfiles"
```

- **MCP 호출**: 2회
- Volume Profile 파라미터(Vignette intensity, Fog density, Color Grading LUT 등)는 직접 기재 금지 (→ see `docs/systems/visual-guide.md`) (PATTERN-007)

---

### VA-05: GlobalVolume에 VP_Global 연결

```
set_property
  gameObject: "GlobalVolume"
  component: "Volume"
  property: "profile"
  value: "Assets/_Project/Settings/VP_Global.asset"

save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 2회

---

## 5. 태스크 그룹 VB: LightingManager 설정

**목적**: 비주얼 시스템 스크립트 전체(Data SO 스크립트 포함)를 생성하고, `LightingManager` MonoBehaviour를 씬에 배치하여 `Sun` Directional Light 및 `GlobalVolume`과 연결한다. `PaletteApplier`도 동시에 배치한다.

**전제 조건**:
- VA 완료 (GlobalVolume GO, VP_Global.asset 존재)
- ARC-002 완료 (`Lighting` GO 및 `Sun` Directional Light 존재)
- ARC-019 완료 (`TimeManager` 이벤트 인터페이스 컴파일 완료)

**예상 MCP 호출**: ~9회

---

### VB-01: Visual 스크립트 폴더 생성

```
create_folder
  path: "Assets/_Project/Scripts/Visual"

create_folder
  path: "Assets/_Project/Scripts/Visual/Data"
```

- **MCP 호출**: 2회

---

### VB-02: Data 계층 스크립트 생성 (S-01~S-04)

```
create_script
  path: "Assets/_Project/Scripts/Visual/Data/LightingSnapshot.cs"
  namespace: "SeedMind.Visual.Data"
  // [System.Serializable] struct
  // 필드: sunColor(Color), sunIntensity(float), sunRotation(Vector3),
  //       ambientColor(Color), fogColor(Color), fogDensity(float)
  // 헬퍼: static LightingSnapshot Lerp(a, b, t)
  // → see docs/systems/visual-architecture.md 섹션 2.4

create_script
  path: "Assets/_Project/Scripts/Visual/Data/PaletteColorCategory.cs"
  namespace: "SeedMind.Visual.Data"
  // enum: Grass(0), Soil(1), Path(2), Leaf(3), Trunk(4),
  //       Flower(5), Water(6), WaterFoam(7)
  // → see docs/systems/visual-architecture.md 섹션 3.3

create_script
  path: "Assets/_Project/Scripts/Visual/Data/SeasonLightingProfile.cs"
  namespace: "SeedMind.Visual.Data"
  // ScriptableObject 상속
  // [CreateAssetMenu(fileName = "SeasonLightingProfile",
  //   menuName = "SeedMind/Visual/SeasonLightingProfile")]
  // 필드: seasonData(SeasonData ref), volumeProfile(VolumeProfile ref),
  //       sunIntensityCurve(AnimationCurve), ambientGradient(Gradient)
  // 헬퍼: GetPhaseData(DayPhase), EvaluateSunIntensity(float), EvaluateAmbientColor(float)
  // → see docs/systems/visual-architecture.md 섹션 2.3

create_script
  path: "Assets/_Project/Scripts/Visual/Data/PaletteData.cs"
  namespace: "SeedMind.Visual.Data"
  // ScriptableObject 상속
  // [CreateAssetMenu(fileName = "PaletteData", menuName = "SeedMind/Visual/PaletteData")]
  // 필드: paletteName(string), targetSeason(Season),
  //   Terrain: grassColor, soilColor, pathColor,
  //   Vegetation: leafColor, trunkColor, flowerColor,
  //   Water: waterColor, waterFoamColor,
  //   Sky: skyZenithColor, skyHorizonColor
  // 모든 Color 수치: → see docs/systems/visual-guide.md
  // → see docs/systems/visual-architecture.md 섹션 3.1
```

- **MCP 호출**: 4회

---

### VB-03: Visual MonoBehaviour 스크립트 생성 (S-05, S-06)

```
create_script
  path: "Assets/_Project/Scripts/Visual/LightingManager.cs"
  namespace: "SeedMind.Visual"
  // MonoBehaviour
  // [Header("References")] directionalLight(Light), globalVolume(Volume)
  // [Header("Season Profiles")] seasonProfiles(SeasonLightingProfile[]) // 4개
  // [Header("Transition")] phaseTransitionDuration(float), seasonTransitionDuration(float)
  //   → see docs/systems/visual-guide.md
  // private: _currentProfile, _phaseTransition(Coroutine), _seasonTransition(Coroutine)
  // OnEnable: TimeManager.OnDayPhaseChanged += HandlePhaseChanged
  //           TimeManager.OnSeasonChanged += HandleSeasonChanged
  // OnDisable: 구독 해제
  // HandlePhaseChanged(DayPhase), HandleSeasonChanged(Season)
  // TransitionCoroutine(LightingSnapshot from, LightingSnapshot to, float duration)
  //   Lerp: sunColor, sunIntensity, sunRotation(Slerp), ambientLight, fogColor, fogDensity
  // ApplyWeatherOverride(float dimFactor), ClearWeatherOverride()
  // → see docs/systems/visual-architecture.md 섹션 2.2

create_script
  path: "Assets/_Project/Scripts/Visual/PaletteApplier.cs"
  namespace: "SeedMind.Visual"
  // MonoBehaviour
  // [SerializeField] seasonPalettes(PaletteData[]) // 4개
  // [SerializeField] targets(PaletteTarget[])
  // _propBlock(MaterialPropertyBlock)
  // static ColorPropID = Shader.PropertyToID("_BaseColor")
  // OnEnable: TimeManager.OnSeasonChanged += ApplyPalette
  // OnDisable: 구독 해제
  // ApplyPalette(Season): PaletteData 인덱싱 → foreach target → SetPropertyBlock
  // → see docs/systems/visual-architecture.md 섹션 3.3
```

- **MCP 호출**: 2회

---

### VB-04: 컴파일 대기

```
execute_menu_item
  menu: "Assets/Refresh"
```

- S-01~S-06 컴파일 완료 확인 후 VB-05 진행
- **MCP 호출**: 1회

---

### VB-05: LightingManager GO 배치 및 참조 연결

```
// LightingManager는 기존 Lighting GO에 컴포넌트로 추가
add_component
  gameObject: "Lighting"
  component: "SeedMind.Visual.LightingManager"

set_property
  gameObject: "Lighting"
  component: "LightingManager"
  property: "directionalLight"
  value: "Lighting/Sun"   // Sun Directional Light 참조

set_property
  gameObject: "Lighting"
  component: "LightingManager"
  property: "globalVolume"
  value: "GlobalVolume"

// PaletteApplier는 독립 GO로 배치
create_object
  name: "PaletteApplier"
  parent: "--- ENVIRONMENT ---"

add_component
  gameObject: "PaletteApplier"
  component: "SeedMind.Visual.PaletteApplier"

save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 6회 (add_component x2 + set_property x2 + create_object x1 + save_scene x1)

---

## 6. 태스크 그룹 VC: SeasonLightingProfile / PaletteData SO 생성

**목적**: 4계절 각각의 `SeasonLightingProfile` SO와 `PaletteData` SO를 생성하고, `LightingManager.seasonProfiles[]` 및 `PaletteApplier.seasonPalettes[]` 배열에 연결한다.

**전제 조건**:
- VB 완료 (SeasonLightingProfile.cs, PaletteData.cs 컴파일 완료)
- VA 완료 (VP_Season_Spring~Winter.asset 존재)
- ARC-019 완료 (SeasonData SO 에셋 4개 존재)

**예상 MCP 호출**: ~20회

---

### VC-01: Visual 데이터 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Visual"
```

- **MCP 호출**: 1회

---

### VC-02: SeasonLightingProfile SO 생성 (4계절)

```
// [권장] Editor 스크립트 방식으로 일괄 생성 및 필드 설정
create_script
  path: "Assets/_Project/Editor/InitSeasonLightingProfiles.cs"
  // [MenuItem("SeedMind/Init/SeasonLightingProfiles")]
  // 생성 대상 및 필드:
  //   SO_LightProfile_Spring.asset
  //     seasonData: SeasonData_Spring.asset 참조
  //     volumeProfile: VP_Season_Spring.asset 참조
  //     sunIntensityCurve: → see docs/systems/visual-guide.md
  //     ambientGradient: → see docs/systems/visual-guide.md
  //   (Summer, Autumn, Winter 동일 패턴)
  // 경로: Assets/_Project/Data/Visual/
  // 모든 수치: → see docs/systems/visual-guide.md

execute_menu_item
  menu: "SeedMind/Init/SeasonLightingProfiles"
```

- **MCP 호출**: 2회
- Color, AnimationCurve, Gradient 수치 직접 기재 금지 (→ see `docs/systems/visual-guide.md`) (PATTERN-007)

---

### VC-03: PaletteData SO 생성 (4계절)

```
create_script
  path: "Assets/_Project/Editor/InitPaletteData.cs"
  // [MenuItem("SeedMind/Init/PaletteData")]
  // 생성 대상:
  //   SO_Palette_Spring.asset
  //   SO_Palette_Summer.asset
  //   SO_Palette_Autumn.asset
  //   SO_Palette_Winter.asset
  // 각 SO 필드 (Color):
  //   targetSeason, grassColor, soilColor, pathColor,
  //   leafColor, trunkColor, flowerColor, waterColor, waterFoamColor,
  //   skyZenithColor, skyHorizonColor
  // 모든 Color HEX 수치: → see docs/systems/visual-guide.md
  // 경로: Assets/_Project/Data/Visual/

execute_menu_item
  menu: "SeedMind/Init/PaletteData"
```

- **MCP 호출**: 2회

---

### VC-04: LightingManager seasonProfiles 배열 연결

```
// seasonProfiles 배열 인덱스 순서: 0=Spring, 1=Summer, 2=Autumn, 3=Winter
set_property
  gameObject: "Lighting"
  component: "LightingManager"
  property: "seasonProfiles"
  value: ["SO_LightProfile_Spring", "SO_LightProfile_Summer",
          "SO_LightProfile_Autumn", "SO_LightProfile_Winter"]
  // → see docs/systems/visual-architecture.md 섹션 2.2
```

- **MCP 호출**: 1회 (배열 타입 set_property — MCP SO 배열 지원 여부 사전 검증 필요)

[RISK] MCP `set_property`로 SO 배열 참조 설정이 불가능한 경우, Editor 스크립트(`AssignLightingManagerRefs.cs`)로 우회. (→ see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)

---

### VC-05: PaletteApplier seasonPalettes 배열 연결

```
set_property
  gameObject: "PaletteApplier"
  component: "PaletteApplier"
  property: "seasonPalettes"
  value: ["SO_Palette_Spring", "SO_Palette_Summer",
          "SO_Palette_Autumn", "SO_Palette_Winter"]
  // → see docs/systems/visual-architecture.md 섹션 3.3

save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 2회

---

### VC-06: SO 수치 입력 참조 테이블

에셋의 모든 수치 필드는 Inspector에서 직접 입력하거나 위의 Editor 스크립트 방식으로 일괄 설정한다. 수치를 이 문서에 직접 기재하는 것은 PATTERN-007 위반이다.

| SO 필드 그룹 | 수치 참조 |
|------------|----------|
| Volume Profile 파라미터 (Vignette, Fog, Color Grading) | (→ see docs/systems/visual-guide.md) |
| SeasonLightingProfile AnimationCurve 제어점 | (→ see docs/systems/visual-guide.md) |
| SeasonLightingProfile Gradient 색상 키 | (→ see docs/systems/visual-guide.md) |
| PaletteData 지형 색상 (Grass, Soil, Path) | (→ see docs/systems/visual-guide.md) |
| PaletteData 식물 색상 (Leaf, Trunk, Flower) | (→ see docs/systems/visual-guide.md) |
| PaletteData 물 색상 (Water, WaterFoam) | (→ see docs/systems/visual-guide.md) |
| PaletteData 하늘 색상 (SkyZenith, SkyHorizon) | (→ see docs/systems/visual-guide.md) |
| LightingManager 전환 시간 (phase/season) | (→ see docs/systems/visual-guide.md) |

---

## 7. 태스크 그룹 VD: 날씨 비주얼 이펙트 (파티클 + WeatherVisualController)

**목적**: 씬에 `WeatherFX` 부모 GO를 생성하고, 날씨 종류별 ParticleSystem을 자식으로 배치한다. `WeatherVisualController` 스크립트를 생성·부착하고 파티클 참조를 연결한다.

**전제 조건**:
- ARC-002 완료 (`--- ENVIRONMENT ---` 씬 계층 존재)
- ARC-019 완료 (WeatherSystem, WeatherType enum 컴파일 완료)

**예상 MCP 호출**: ~18회

---

### VD-01: WeatherVisualController 스크립트 생성 (S-07)

```
create_script
  path: "Assets/_Project/Scripts/Visual/WeatherVisualController.cs"
  namespace: "SeedMind.Visual"
  // MonoBehaviour
  // [Header("Particle Systems")]
  //   rainParticle, heavyRainParticle, snowParticle, blizzardParticle,
  //   windLeafParticle (ParticleSystem 타입)
  // [Header("Lighting Override")]
  //   rainLightDimFactor, stormLightDimFactor, snowLightDimFactor (float)
  //   → see docs/systems/visual-guide.md
  // [Header("Fog Override")]
  //   stormFogDensityAdd, blizzardFogDensityAdd (float)
  //   → see docs/systems/visual-guide.md
  // [Header("Transition")]
  //   weatherTransitionDuration (float) → see docs/systems/visual-guide.md
  // [Header("References")]
  //   lightingManager (LightingManager)
  // OnEnable: WeatherSystem.OnWeatherChanged += HandleWeatherChanged
  // OnDisable: 구독 해제
  // HandleWeatherChanged(WeatherType old, WeatherType new)
  // StopWeatherFX(WeatherType), StartWeatherFX(WeatherType)
  //   switch: Clear/Cloudy → 없음, Rain → rainParticle.Play(),
  //           HeavyRain → heavyRainParticle.Play(),
  //           Storm → heavyRainParticle.Play() + LightingManager.ApplyWeatherOverride()
  //           Snow → snowParticle.Play(), Blizzard → blizzardParticle.Play()
  // ApplyLightingOverride(WeatherType), ApplyFogOverride(WeatherType)
  // → see docs/systems/visual-architecture.md 섹션 5.1

execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 2회 (create_script + 컴파일 대기)

---

### VD-02: WeatherFX 부모 GO 및 파티클 GO 생성

```
create_object
  name: "WeatherFX"
  parent: "--- ENVIRONMENT ---"

create_object
  name: "RainParticle"
  parent: "WeatherFX"

create_object
  name: "HeavyRainParticle"
  parent: "WeatherFX"

create_object
  name: "SnowParticle"
  parent: "WeatherFX"

create_object
  name: "BlizzardParticle"
  parent: "WeatherFX"

create_object
  name: "WindLeafParticle"
  parent: "WeatherFX"
```

- **MCP 호출**: 6회

---

### VD-03: ParticleSystem 컴포넌트 부착 (5개)

```
add_component
  gameObject: "WeatherFX/RainParticle"
  component: "UnityEngine.ParticleSystem"

add_component
  gameObject: "WeatherFX/HeavyRainParticle"
  component: "UnityEngine.ParticleSystem"

add_component
  gameObject: "WeatherFX/SnowParticle"
  component: "UnityEngine.ParticleSystem"

add_component
  gameObject: "WeatherFX/BlizzardParticle"
  component: "UnityEngine.ParticleSystem"

add_component
  gameObject: "WeatherFX/WindLeafParticle"
  component: "UnityEngine.ParticleSystem"
```

- **MCP 호출**: 5회
- ParticleSystem 파라미터 수치(Emission Rate, Gravity Modifier 등)는 직접 기재 금지 (→ see `docs/systems/visual-guide.md`) (PATTERN-007)

---

### VD-04: WeatherVisualController 컴포넌트 부착 및 참조 연결

```
add_component
  gameObject: "WeatherFX"
  component: "SeedMind.Visual.WeatherVisualController"

set_property
  gameObject: "WeatherFX"
  component: "WeatherVisualController"
  property: "rainParticle"
  value: "WeatherFX/RainParticle"

set_property
  gameObject: "WeatherFX"
  component: "WeatherVisualController"
  property: "heavyRainParticle"
  value: "WeatherFX/HeavyRainParticle"

set_property
  gameObject: "WeatherFX"
  component: "WeatherVisualController"
  property: "snowParticle"
  value: "WeatherFX/SnowParticle"

set_property
  gameObject: "WeatherFX"
  component: "WeatherVisualController"
  property: "blizzardParticle"
  value: "WeatherFX/BlizzardParticle"

set_property
  gameObject: "WeatherFX"
  component: "WeatherVisualController"
  property: "windLeafParticle"
  value: "WeatherFX/WindLeafParticle"

set_property
  gameObject: "WeatherFX"
  component: "WeatherVisualController"
  property: "lightingManager"
  value: "Lighting"

save_scene
  scene: "SCN_Farm"
```

- **MCP 호출**: 8회 (add_component x1 + set_property x6 + save_scene x1)

[RISK] 날씨 파티클 카메라 추적 구현 시, Simulation Space = World로 설정해야 입자 순간이동 현상을 방지할 수 있다 (→ see `docs/systems/visual-architecture.md` 섹션 5.3). 각 ParticleSystem의 `simulationSpace` 프로퍼티를 `set_property`로 World로 지정할 것.

---

### VD-05: 날씨 파티클 검증

```
enter_play_mode

execute_method
  // WeatherSystem.OnWeatherChanged 이벤트 수동 발화 (WeatherType.Rain → HeavyRain → Snow)
  // 파티클 전환 및 조명 감쇄 동작 확인

get_console_logs

exit_play_mode
```

- **MCP 호출**: 3회 (enter + execute_method + exit; get_console_logs는 추가)
- 검증 항목: 파티클 활성화/비활성화, LightingManager.ApplyWeatherOverride 호출 로그, 전환 코루틴 정상 동작

---

## 8. 태스크 그룹 VE: CropVisual 프리팹 구성

**목적**: `CropVisual` MonoBehaviour 스크립트를 생성하고, 작물 프리팹(`PFB_Crop_[작물명]`)에 성장 단계별 자식 GO 구조를 설정한다.

**전제 조건**:
- ARC-003 완료 (FarmTile GO, CropInstance 데이터 구조 존재)
- VB 완료 (PaletteApplier 등 Visual 시스템 스크립트 컴파일 완료)
- ARC-019 완료 (Season enum 컴파일 완료)
- 작물 프리팹 식별자: [OPEN - to be filled after CON-* 작물 콘텐츠 확정 시]

**예상 MCP 호출**: ~12회

---

### VE-01: CropVisual 스크립트 생성 (S-08)

```
create_script
  path: "Assets/_Project/Scripts/Visual/CropVisual.cs"
  namespace: "SeedMind.Farm"
  // MonoBehaviour
  // [Header("Stage Visuals")]
  //   stagePrefabs(GameObject[]) // index 0~3: Seed, Sprout, Growing, Harvestable
  //   → see docs/systems/crop-growth.md 섹션 1.1
  // [Header("Quality FX")]
  //   qualityParticle(GameObject), qualityGlowFX(ParticleSystem), fruitRenderer(Renderer)
  // [Header("Animation")]
  //   harvestableSwaySpeed(float), harvestableSwayAngle(float)
  //   → see docs/systems/visual-guide.md
  // _currentStageIndex(int), _propBlock(MaterialPropertyBlock)
  // UpdateStage(int stageIndex):
  //   모든 stagePrefabs 비활성화 → stagePrefabs[stageIndex] 활성화
  //   stageIndex==3 이면 흔들림 애니메이션 코루틴 시작
  // SetQualityTier(QualityTier tier):
  //   Normal → 기본, Good → 채도 증가(MaterialPropertyBlock),
  //   Excellent → qualityGlowFX 소규모 활성화,
  //   Master → qualityGlowFX 강화 + Emissive 색상
  //   → see docs/systems/crop-growth.md 섹션 5
  // PlayHarvestEffect(), PlayWitherEffect()
  // → see docs/systems/visual-architecture.md 섹션 4.1

execute_menu_item
  menu: "Assets/Refresh"
```

- **MCP 호출**: 2회

---

### VE-02: 작물 프리팹 기본 구조 설정 (공통 패턴)

작물 종류별 프리팹에 동일한 패턴을 적용한다. 아래는 단일 작물 프리팹 기준 예시이다.

```
// 예시: PFB_Crop_Turnip (순무) — 작물명은 [OPEN - CON-* 확정 후 교체]
create_object
  name: "Stage_0_Seed"
  parent: "PFB_Crop_[작물명]"

create_object
  name: "Stage_1_Sprout"
  parent: "PFB_Crop_[작물명]"

create_object
  name: "Stage_2_Growing"
  parent: "PFB_Crop_[작물명]"

create_object
  name: "Stage_3_Harvestable"
  parent: "PFB_Crop_[작물명]"

create_object
  name: "QualityFX"
  parent: "PFB_Crop_[작물명]"

add_component
  gameObject: "PFB_Crop_[작물명]"
  component: "SeedMind.Farm.CropVisual"

set_property
  gameObject: "PFB_Crop_[작물명]"
  component: "CropVisual"
  property: "stagePrefabs"
  value: ["Stage_0_Seed", "Stage_1_Sprout", "Stage_2_Growing", "Stage_3_Harvestable"]
```

- 초기 상태: Stage_0 활성, Stage_1~3 비활성 (`SetActive(false)`)
- **단일 작물 기준 MCP 호출**: ~8회 (create_object x5 + add_component x1 + set_property x1 + 비활성화 x1)

[OPEN - to be filled after CON-* 작물 콘텐츠 확정] 프리팹 생성 대상 작물 목록. 현재 `docs/design.md` 섹션 4.2 기준 작물 식별자를 사용하되, CON 문서 확정 후 동기화 필요.

[RISK] 성장 단계 프리팹의 메시/스케일은 작물별로 상이하다. 단계별 GO에는 임시로 빈 GameObject를 배치하고, 실제 메시는 에셋 제작 후 연결. 현 단계에서는 컴포넌트 구조 설정에 집중.

---

### VE-03: 씬 저장 및 검증

```
save_scene
  scene: "SCN_Farm"

enter_play_mode
// CropVisual.UpdateStage() 수동 호출 → 단계 전환 로그 확인
// CropVisual.SetQualityTier(QualityTier.Master) → 파티클 활성화 확인

get_console_logs

exit_play_mode
```

- **MCP 호출**: 4회

---

## 9. 태스크 그룹 VF: 머티리얼 생성

**목적**: 카테고리별 공유 Material 에셋을 생성하고, 셰이더 및 기본 속성을 설정한다. 모든 머티리얼은 URP/Lit 셰이더 기반이며, Flat Shading을 위해 Smoothness=0으로 통일한다.

**전제 조건**:
- ARC-002 완료 (`Assets/_Project/Materials/` 폴더 존재 또는 생성)

**예상 MCP 호출**: ~20회

---

### VF-01: 머티리얼 폴더 생성

```
create_folder
  path: "Assets/_Project/Materials/Terrain"

create_folder
  path: "Assets/_Project/Materials/Crops"

create_folder
  path: "Assets/_Project/Materials/Buildings"

create_folder
  path: "Assets/_Project/Materials/Environment"

create_folder
  path: "Assets/_Project/Materials/FX"
```

- **MCP 호출**: 5회

---

### VF-02: Terrain 머티리얼 생성

```
create_material
  path: "Assets/_Project/Materials/Terrain/M_Grass.mat"
  shader: "Universal Render Pipeline/Lit"

create_material
  path: "Assets/_Project/Materials/Terrain/M_Soil_Empty.mat"
  shader: "Universal Render Pipeline/Lit"

create_material
  path: "Assets/_Project/Materials/Terrain/M_Soil_Tilled.mat"
  shader: "Universal Render Pipeline/Lit"

create_material
  path: "Assets/_Project/Materials/Terrain/M_Soil_Watered.mat"
  shader: "Universal Render Pipeline/Lit"

create_material
  path: "Assets/_Project/Materials/Terrain/M_Path.mat"
  shader: "Universal Render Pipeline/Lit"
```

- **MCP 호출**: 5회
- 각 머티리얼의 Smoothness=0, Metallic=0, Color HEX 값: (→ see `docs/systems/visual-guide.md`) (PATTERN-007)

---

### VF-03: Crops 머티리얼 생성

```
create_material
  path: "Assets/_Project/Materials/Crops/M_Crop_Generic_Leaf.mat"
  shader: "Universal Render Pipeline/Lit"

create_material
  path: "Assets/_Project/Materials/Crops/M_Crop_Generic_Stem.mat"
  shader: "Universal Render Pipeline/Lit"
```

- **MCP 호출**: 2회
- 작물별 열매 머티리얼(`M_Crop_Fruit_[작물명].mat`)은 CON-* 작물 콘텐츠 확정 후 생성: [OPEN - to be filled after CON-* 확정]

---

### VF-04: Environment 및 FX 머티리얼 생성

```
create_material
  path: "Assets/_Project/Materials/Environment/M_Water.mat"
  shader: "Universal Render Pipeline/Lit"

create_material
  path: "Assets/_Project/Materials/Environment/M_Tree_Leaf.mat"
  shader: "Universal Render Pipeline/Lit"

create_material
  path: "Assets/_Project/Materials/Environment/M_Tree_Trunk.mat"
  shader: "Universal Render Pipeline/Lit"

create_material
  path: "Assets/_Project/Materials/Environment/M_Fence.mat"
  shader: "Universal Render Pipeline/Lit"

create_material
  path: "Assets/_Project/Materials/FX/M_Particle_Rain.mat"
  shader: "Universal Render Pipeline/Particles/Unlit"

create_material
  path: "Assets/_Project/Materials/FX/M_Particle_Snow.mat"
  shader: "Universal Render Pipeline/Particles/Unlit"

create_material
  path: "Assets/_Project/Materials/FX/M_Particle_Leaf.mat"
  shader: "Universal Render Pipeline/Particles/Unlit"
```

- **MCP 호출**: 7회
- Buildings 카테고리(M_Bldg_Wood, M_Bldg_Stone, M_Bldg_Roof)는 시설 시스템(ARC-031) 완료 후 생성

---

### VF-05: 머티리얼 수치 참조 테이블

| 머티리얼 그룹 | 수치 참조 |
|-------------|----------|
| Terrain 색상 (Grass, Soil variants, Path) | (→ see docs/systems/visual-guide.md) |
| Vegetation 색상 (Leaf, Stem, Trunk) | (→ see docs/systems/visual-guide.md) |
| Water/Foam 색상 | (→ see docs/systems/visual-guide.md) |
| FX Particle 색상 및 Opacity | (→ see docs/systems/visual-guide.md) |
| Smoothness, Metallic 공통값 | (→ see docs/systems/visual-architecture.md 섹션 1.2) |

---

## 10. 전체 완료 체크리스트

### VA: URP/Volume 설정

- [ ] `Assets/_Project/Settings/` 폴더 존재
- [ ] URP Asset 설정 확인 완료 (렌더링 경로, MSAA, 그림자)
- [ ] `GlobalVolume` GO 씬 배치 완료 (isGlobal=true)
- [ ] `VP_Global.asset` 생성 및 GlobalVolume에 연결 완료
- [ ] `VP_Season_Spring/Summer/Autumn/Winter.asset` 4개 생성 완료
- [ ] `SCN_Farm` 저장 완료

### VB: LightingManager 설정

- [ ] `Assets/_Project/Scripts/Visual/Data/` 폴더 존재
- [ ] S-01~S-04 (LightingSnapshot, PaletteColorCategory, SeasonLightingProfile, PaletteData) 컴파일 완료
- [ ] S-05~S-06 (LightingManager, PaletteApplier) 컴파일 완료
- [ ] LightingManager 컴포넌트가 `Lighting` GO에 부착됨
- [ ] LightingManager.directionalLight → Sun 연결 완료
- [ ] LightingManager.globalVolume → GlobalVolume 연결 완료
- [ ] PaletteApplier GO 씬 배치 완료
- [ ] `SCN_Farm` 저장 완료

### VC: SO 에셋 생성

- [ ] `Assets/_Project/Data/Visual/` 폴더 존재
- [ ] SO_LightProfile_Spring/Summer/Autumn/Winter.asset 4개 생성 완료
- [ ] 각 SeasonLightingProfile.seasonData → SeasonData SO 연결 완료
- [ ] 각 SeasonLightingProfile.volumeProfile → VP_Season_* 연결 완료
- [ ] SO_Palette_Spring/Summer/Autumn/Winter.asset 4개 생성 완료
- [ ] LightingManager.seasonProfiles[0~3] 배열 연결 완료
- [ ] PaletteApplier.seasonPalettes[0~3] 배열 연결 완료

### VD: 날씨 이펙트

- [ ] S-07 (WeatherVisualController) 컴파일 완료
- [ ] `WeatherFX` GO 씬 배치 완료 (`--- ENVIRONMENT ---` 하위)
- [ ] RainParticle, HeavyRainParticle, SnowParticle, BlizzardParticle, WindLeafParticle GO 배치 완료
- [ ] 각 ParticleSystem 컴포넌트 부착 완료 (simulationSpace=World)
- [ ] WeatherVisualController 컴포넌트 부착 및 5개 파티클 참조 연결 완료
- [ ] WeatherVisualController.lightingManager → Lighting GO 연결 완료
- [ ] 날씨 전환 런타임 테스트 통과 (콘솔 에러 없음)
- [ ] `SCN_Farm` 저장 완료

### VE: CropVisual 프리팹

- [ ] S-08 (CropVisual) 컴파일 완료
- [ ] 작물 프리팹별 Stage_0~3 자식 GO 구조 설정 완료
- [ ] CropVisual.stagePrefabs 배열 연결 완료
- [ ] UpdateStage() 런타임 테스트 통과
- [ ] SetQualityTier(Master) 파티클 활성화 확인
- [ ] `SCN_Farm` 저장 완료

### VF: 머티리얼

- [ ] `Materials/Terrain/`, `Crops/`, `Environment/`, `FX/` 폴더 생성 완료
- [ ] Terrain 머티리얼 5개 생성 완료 (URP/Lit, Smoothness=0)
- [ ] Crops 공통 머티리얼 2개 생성 완료
- [ ] Environment 머티리얼 4개 생성 완료
- [ ] FX/Particle 머티리얼 3개 생성 완료 (URP/Particles/Unlit)

---

## Open Questions

- [OPEN] Custom Toon Shader 도입 여부 — URP/Lit Smoothness=0만으로 충분한 Flat Shading이 가능한지 프로토타입 단계에서 검증 필요. 도입 시 Shader Graph 기반 2-step ramp 조명 태스크 추가 필요. (→ see `docs/systems/visual-architecture.md` 섹션 1.2)
- [OPEN] Outline 포스트프로세싱 적용 여부 — Depth 기반 Edge Detection 도입 시 VA 태스크에 추가 Volume Profile 설정 필요. (→ see `docs/systems/visual-architecture.md` 섹션 1.3)
- [OPEN] CropVisual 폴더 배치 확정 — `Scripts/Visual/` vs `Scripts/Farm/`. 현재 임시 배치, 프로토타입 후 결정. (→ see `docs/systems/visual-architecture.md` 섹션 6.2)
- [OPEN] Volume Profile 계절 전환 방식 — Profile 교체 vs 단일 Profile 파라미터 보간. 끊김 현상 발생 시 단일 Profile 방식으로 전환 검토. (→ see `docs/systems/visual-architecture.md` 섹션 1.3)
- [OPEN - to be filled after CON-* 확정] 작물별 프리팹 식별자 목록 (VE-02). `docs/design.md` 섹션 4.2 작물 목록 확정 후 VE 태스크에 전체 작물 반영 필요.

---

## Risks

- [RISK] `create_scriptable_object` MCP 도구의 SO 배열/Object 참조 필드 설정 지원 여부 미검증 — VC, VE에서 Editor 스크립트 우회 방식을 준비해야 한다. (→ see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정)
- [RISK] Volume Profile 계절 교체 시 시각적 끊김 가능 — `LightingManager.HandleSeasonChanged()`에서 Volume.weight를 코루틴으로 보간하여 완화. 전환 시간 수치: (→ see `docs/systems/visual-guide.md`) (섹션 1.3)
- [RISK] SeasonData와 SeasonLightingProfile 이중 참조 구조로 데이터 동기화 부담 발생 가능 — SeasonLightingProfile은 추가 비주얼 파라미터(AnimationCurve, Gradient, VolumeProfile)만 보유하도록 역할 분리 엄격 유지 (섹션 2.3)
- [RISK] MaterialPropertyBlock이 URP SRP Batcher와 충돌하여 배칭이 깨질 가능성 — URP 6에서 `PropertyBlock` + SRP Batcher 호환성을 VF 완료 후 프로토타입 단계에서 반드시 검증 (섹션 3.3)
- [RISK] 날씨 파티클 카메라 추적 시 입자 순간이동 현상 — Simulation Space=World, Emission Position을 카메라 위치 기준으로 매 프레임 갱신해야 한다 (섹션 5.3)
- [RISK] SO 에셋 수치 필드(Color, AnimationCurve, Gradient) 수동 입력 부담 — `docs/systems/visual-guide.md`가 canonical 출처로 완성된 후 Editor 스크립트로 일괄 적용. `visual-guide.md` 미완성 시 PATTERN-010 위반에 해당하므로 임의 수치 입력 금지.

---

## Cross-references

- `docs/systems/visual-architecture.md` — 상위 설계 문서: 클래스 구조, SO 스키마, 이벤트 연동 흐름 전체 정의
- `docs/systems/visual-guide.md` — 비주얼 수치 canonical 출처: 모든 Color, 강도, 크기, 전환 시간 수치
- `docs/systems/time-season-architecture.md` — SeasonData, DayPhaseVisual SO 정의 (VC 태스크 의존성)
- `docs/systems/crop-growth.md` — 작물 성장 단계 정의(섹션 1.1), 품질 티어(섹션 5) (VE 태스크 참조)
- `docs/systems/project-structure.md` — 폴더 구조, 네임스페이스 규칙, asmdef 배치
- `docs/mcp/scene-setup-tasks.md` (ARC-002) — 씬 기본 계층 구조, 폴더 초기화
- `docs/mcp/time-season-tasks.md` (ARC-019) — TimeManager, WeatherSystem 배치 완료 조건
- `docs/mcp/farming-tasks.md` (ARC-003) — FarmTile, CropInstance 배치 완료 조건
- `docs/pipeline/data-pipeline.md` — ScriptableObject 파이프라인 전반 구조
- `docs/architecture.md` 섹션 5 — 렌더링 전략 개요
