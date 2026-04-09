# 비주얼 시스템 기술 아키텍처

> 렌더링 파이프라인, 조명 시스템, 색상/팔레트, 작물 비주얼, 날씨 이펙트의 Unity 6 구현 설계  
> 작성: Claude Code (Opus) | 2026-04-07

---

## Context

이 문서는 `docs/architecture.md` 5절(렌더링 전략)을 기술적으로 상세화한다. SeedMind의 로우폴리 비주얼 시스템은 시간/계절 시스템(`TimeManager`, `SeasonData`)과 긴밀하게 연동되어, 시간대/계절/날씨에 따라 조명, 색상, 파티클을 실시간으로 전환한다. 이 문서는 그 전환을 제어하는 클래스 계층, ScriptableObject 데이터 구조, 컴포넌트 아키텍처, MCP 구현 태스크를 정의한다.

**설계 목표**:
- 로우폴리 스타일의 일관성: 단색/2톤 머티리얼 기반, 과도한 포스트프로세싱 배제
- 데이터 드리븐: 모든 비주얼 파라미터를 ScriptableObject로 외부화하여 코드 변경 없이 조정 가능
- 성능 우선: MaterialPropertyBlock 활용으로 Draw Call 최적화, 파티클 풀링
- TimeManager 이벤트 기반 전환: 폴링 없이 이벤트 구독으로 조명/색상 변경

---

## 1. 렌더링 파이프라인

### 1.1 URP 설정 방향

| 항목 | 설정값 | 근거 |
|------|--------|------|
| Render Pipeline | URP (Universal Render Pipeline) | 로우폴리 스타일라이즈드에 적합, 모바일/PC 범용 |
| Rendering Path | Forward Rendering | 단일 Directional Light 환경에서 최적 |
| Anti-Aliasing | MSAA 4x | 로우폴리 엣지 부드럽게 처리 |
| Shadow Type | Soft Shadow | 단일 Directional Light, Shadow Resolution 1024 |
| Shadow Cascade | 2 Cascade | 쿼터뷰 카메라 범위 대응, 3 이상은 과잉 |
| HDR | Off | 로우폴리 스타일에 HDR 불필요, 성능 절약 |
| Depth Texture | On | Outline [OPEN] 도입 시 활용. 현재는 포스트프로세싱 목적으로 유지 (섹션 1.3) |
| Opaque Texture | Off | 불필요 |

### 1.2 셰이더 전략

로우폴리 스타일을 구현하기 위한 셰이더 선택:

| 셰이더 | 용도 | 설명 |
|--------|------|------|
| URP/Lit | 기본 오브젝트 전체 | Smoothness=0으로 설정하여 Flat 느낌 구현 |
| URP/Unlit | 특수 이펙트, UI 3D 요소 | 조명 무시 필요 시 |
| Custom Toon Shader | [OPEN] 선택적 도입 | Cel Shading 2단계 음영 표현 |

**Flat Shading 구현 방식**:
- 메시 레벨에서 노멀을 face-normal로 설정 (vertex-normal 공유 X)
- MCP로 에셋 임포트 시 "Calculate → Normals Mode: Unweighted" 설정
- Smoothness = 0, Metallic = 0으로 모든 기본 머티리얼 통일

[OPEN] Custom Toon Shader 도입 여부: URP/Lit의 Smoothness=0만으로 충분한 로우폴리 룩이 나오는지 프로토타입 단계에서 검증 필요. Toon Shader 도입 시 Shader Graph 기반으로 2-step ramp 조명 구현.

### 1.3 Post-Processing 스택

로우폴리 스타일에 맞게 최소한의 포스트프로세싱만 적용한다.

| 효과 | 적용 여부 | 설정 방향 |
|------|:---------:|-----------|
| Bloom | X | 로우폴리 스타일에 불필요. 과도한 글로우 방지 |
| Color Grading | O | LUT 기반 계절별 색감 보정 (→ see docs/systems/visual-guide.md) |
| Vignette | O (미세) | 화면 가장자리 미세 어둡게, 아늑한 분위기 강화 |
| Depth of Field | X | 쿼터뷰 카메라에 불필요 |
| Motion Blur | X | 불필요 |
| Film Grain | X | 로우폴리 스타일과 불일치 |
| Outline (Edge Detection) | [OPEN] | Depth 기반 외곽선 -- 프로토타입 후 결정 |
| Ambient Occlusion | X | 로우폴리 메시에 SSAO는 부자연스러움 |
| Fog | O | 거리 안개로 씬 경계 자연스럽게 처리 |

**Volume Profile 구성**:

```
Assets/_Project/Settings/
├── VP_Global.asset              # 글로벌 Volume Profile (Vignette, Fog 등)
├── VP_Season_Spring.asset       # 계절별 Color Grading 오버라이드
├── VP_Season_Summer.asset
├── VP_Season_Autumn.asset
└── VP_Season_Winter.asset
```

[RISK] Volume Profile을 계절마다 교체하는 방식은 전환 순간 시각적 끊김이 발생할 수 있다. Volume의 Weight를 코루틴으로 보간하여 부드럽게 전환해야 한다.

---

## 2. 조명 시스템 아키텍처

### 2.1 씬 조명 구조

기존 `docs/systems/project-structure.md`의 SCN_Farm 씬 계층에서 정의된 Lighting 하위 구조를 확장한다.

```
--- ENVIRONMENT ---
├── Lighting
│   ├── Sun (Directional Light)          # 단일 메인 광원
│   ├── LightingManager (MonoBehaviour)  # 조명 전환 제어
│   └── AmbientProbe                     # 반사 프로브 (선택)
```

### 2.2 LightingManager 클래스 설계

```csharp
namespace SeedMind.Visual  // MonoBehaviour → SeedMind.Visual
{
    /// <summary>
    /// 시간대/계절에 따라 Directional Light와 앰비언트 조명을 보간 전환한다.
    /// TimeManager 이벤트를 구독하여 반응형으로 동작한다.
    /// </summary>
    public class LightingManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Light directionalLight;     // Sun
        [SerializeField] private Volume globalVolume;         // Post-Processing Volume

        [Header("Season Profiles")]
        [SerializeField] private SeasonLightingProfile[] seasonProfiles; // 4개 (Spring~Winter)

        [Header("Transition")]
        [SerializeField] private float phaseTransitionDuration; // → see docs/systems/visual-guide.md
        [SerializeField] private float seasonTransitionDuration; // → see docs/systems/visual-guide.md

        // 현재 상태
        private SeasonLightingProfile _currentProfile;
        private Coroutine _phaseTransition;
        private Coroutine _seasonTransition;

        // === 이벤트 구독 ===
        // OnEnable:  TimeManager.OnDayPhaseChanged += HandlePhaseChanged
        //            TimeManager.OnSeasonChanged   += HandleSeasonChanged
        // OnDisable: 구독 해제

        // === 핵심 메서드 ===
        // HandlePhaseChanged(DayPhase newPhase):
        //   1. _currentProfile.GetPhaseData(newPhase) → target 데이터 획득
        //   2. 기존 _phaseTransition 코루틴 중단
        //   3. 새 코루틴으로 보간 시작 (phaseTransitionDuration 초)
        //
        // HandleSeasonChanged(Season newSeason):
        //   1. seasonProfiles[newSeason] → _currentProfile 교체
        //   2. Volume Profile 교체 (Weight 보간)
        //   3. 현재 DayPhase에 맞는 조명으로 즉시 재적용
        //
        // TransitionCoroutine(LightingSnapshot from, LightingSnapshot to, float duration):
        //   매 프레임 Lerp:
        //     directionalLight.color      = Color.Lerp(...)
        //     directionalLight.intensity   = Mathf.Lerp(...)
        //     directionalLight.transform.rotation = Quaternion.Slerp(...)
        //     RenderSettings.ambientLight  = Color.Lerp(...)
        //     RenderSettings.fogColor      = Color.Lerp(...)
        //     RenderSettings.fogDensity    = Mathf.Lerp(...)
    }
}
```

**이벤트 연동 흐름**:

```
TimeManager
    ├── OnDayPhaseChanged(DayPhase)
    │       └── LightingManager.HandlePhaseChanged()
    │               └── Coroutine: Sun color/intensity/rotation 보간
    │                   └── RenderSettings ambient/fog 보간
    │
    └── OnSeasonChanged(Season)
            └── LightingManager.HandleSeasonChanged()
                    ├── SeasonLightingProfile 교체
                    ├── Volume Profile Weight 보간
                    └── 현재 DayPhase 조명 재적용
```

> **기존 설계와의 관계**: `time-season-architecture.md` 섹션 6에서 정의한 `DayPhaseVisual` 데이터를 이 시스템이 소비한다. `SeasonData.phaseOverrides[]`를 참조하여 조명을 전환하는 흐름은 동일하며, 이 문서는 그 전환을 담당하는 `LightingManager` 컴포넌트의 구체적 구현 구조를 정의한다.

### 2.3 SeasonLightingProfile ScriptableObject

`time-season-architecture.md`의 `SeasonData`에 이미 조명 관련 필드(`sunColor`, `sunIntensity`, `ambientColor`, `fogColor`, `fogDensity`, `phaseOverrides[]`)가 정의되어 있다. 비주얼 시스템은 이 데이터를 직접 참조하므로 별도 SO를 중복 정의하지 않고, `SeasonData`를 래핑하는 프로파일 어댑터 구조를 사용한다.

```csharp
namespace SeedMind.Visual.Data  // ScriptableObject 데이터 타입 → SeedMind.Visual.Data
{
    /// <summary>
    /// SeasonData를 래핑하여 조명 전환에 필요한 데이터만 추출하는 어댑터.
    /// 추후 Season-독립적 조명 오버라이드가 필요할 경우 확장 지점.
    /// </summary>
    [CreateAssetMenu(fileName = "SeasonLightingProfile", menuName = "SeedMind/Visual/SeasonLightingProfile")]
    public class SeasonLightingProfile : ScriptableObject
    {
        [Header("Season Reference")]
        public SeasonData seasonData; // → see docs/systems/time-season-architecture.md 섹션 2.1

        [Header("Post-Processing")]
        public VolumeProfile volumeProfile; // 계절별 Volume Profile 참조

        [Header("Additional Overrides")]
        public AnimationCurve sunIntensityCurve; // 시간(0~24) → intensity 보간 커브
        public Gradient ambientGradient;          // 시간(0~24) → ambient color 연속 보간

        // === 헬퍼 메서드 ===
        // GetPhaseData(DayPhase phase) → DayPhaseVisual:
        //   return seasonData.phaseOverrides[(int)phase];
        //
        // EvaluateSunIntensity(float hour) → float:
        //   return sunIntensityCurve.Evaluate(hour / 24f);
        //
        // EvaluateAmbientColor(float hour) → Color:
        //   return ambientGradient.Evaluate(hour / 24f);
    }
}
```

**AnimationCurve/Gradient vs DayPhaseVisual 이산 데이터 관계**:
- `DayPhaseVisual[]`(5개 시간대)은 이산적 전환 포인트를 정의한다 (→ see docs/systems/time-season-architecture.md 섹션 2.1)
- `AnimationCurve`/`Gradient`는 시간 축 전체에 걸친 연속 보간을 제공한다
- `LightingManager`는 두 데이터를 조합하여 사용: DayPhase 전환 이벤트 시 AnimationCurve로 매 프레임 부드럽게 보간

[RISK] SeasonData와 SeasonLightingProfile의 이중 참조 구조가 데이터 동기화 부담을 발생시킬 수 있다. SeasonData를 단일 출처로 유지하고, SeasonLightingProfile은 추가 비주얼 파라미터(커브, 그래디언트, Volume)만 보유하도록 역할을 엄격히 분리해야 한다.

### 2.4 LightingSnapshot 구조체

보간 연산의 from/to 상태를 캡슐화하는 값 타입:

```csharp
namespace SeedMind.Visual.Data  // 값 타입(struct) → SeedMind.Visual.Data
{
    [System.Serializable]
    public struct LightingSnapshot
    {
        public Color sunColor;
        public float sunIntensity;
        public Vector3 sunRotation;  // Euler angles
        public Color ambientColor;
        public Color fogColor;
        public float fogDensity;

        // Lerp 헬퍼
        // static LightingSnapshot Lerp(LightingSnapshot a, LightingSnapshot b, float t)
    }
}
```

---

## 3. 색상/팔레트 시스템

### 3.1 PaletteData ScriptableObject

계절별 색상 팔레트를 정의하는 SO. 지형, 식물, 물 등 카테고리별 색상을 한 곳에서 관리한다.

```csharp
namespace SeedMind.Visual.Data  // ScriptableObject 데이터 타입 → SeedMind.Visual.Data
{
    [CreateAssetMenu(fileName = "PaletteData", menuName = "SeedMind/Visual/PaletteData")]
    public class PaletteData : ScriptableObject
    {
        public string paletteName;       // "Spring", "Summer" 등
        public Season targetSeason;      // 적용 대상 계절

        [Header("Terrain")]
        public Color grassColor;         // → see docs/systems/visual-guide.md
        public Color soilColor;          // → see docs/systems/visual-guide.md
        public Color pathColor;          // → see docs/systems/visual-guide.md

        [Header("Vegetation")]
        public Color leafColor;          // → see docs/systems/visual-guide.md
        public Color trunkColor;         // → see docs/systems/visual-guide.md
        public Color flowerColor;        // → see docs/systems/visual-guide.md

        [Header("Water")]
        public Color waterColor;         // → see docs/systems/visual-guide.md
        public Color waterFoamColor;     // → see docs/systems/visual-guide.md

        [Header("Sky")]
        public Color skyZenithColor;     // → see docs/systems/visual-guide.md
        public Color skyHorizonColor;    // → see docs/systems/visual-guide.md
    }
}
```

**에셋 배치**:
```
Assets/_Project/Data/Visual/
├── SO_Palette_Spring.asset
├── SO_Palette_Summer.asset
├── SO_Palette_Autumn.asset
└── SO_Palette_Winter.asset
```

### 3.2 머티리얼 구조

공유 Material을 카테고리별로 관리하여 Draw Call을 최소화한다.

```
Assets/_Project/Materials/
├── Terrain/
│   ├── M_Grass.mat              # 잔디 (URP/Lit, Smoothness=0)
│   ├── M_Soil_Empty.mat         # 빈 토양
│   ├── M_Soil_Tilled.mat        # 경작된 토양
│   ├── M_Soil_Watered.mat       # 물 준 토양
│   └── M_Path.mat               # 길
│
├── Crops/
│   ├── M_Crop_Generic_Leaf.mat  # 공용 잎 머티리얼
│   ├── M_Crop_Generic_Stem.mat  # 공용 줄기 머티리얼
│   └── M_Crop_Fruit_[작물명].mat # 작물별 열매 머티리얼
│
├── Buildings/
│   ├── M_Bldg_Wood.mat          # 목재 건물
│   ├── M_Bldg_Stone.mat         # 석재 건물
│   └── M_Bldg_Roof.mat          # 지붕
│
├── Environment/
│   ├── M_Water.mat              # 물
│   ├── M_Tree_Leaf.mat          # 나뭇잎 (계절별 색상 교체 대상)
│   ├── M_Tree_Trunk.mat         # 나무 줄기
│   └── M_Fence.mat              # 울타리
│
└── FX/
    ├── M_Particle_Rain.mat      # 비 파티클
    ├── M_Particle_Snow.mat      # 눈 파티클
    └── M_Particle_Leaf.mat      # 낙엽 파티클
```

### 3.3 런타임 색상 교체: MaterialPropertyBlock 시스템

계절 전환 시 머티리얼 인스턴스를 복제하지 않고 `MaterialPropertyBlock`으로 색상만 교체한다.

```csharp
// PaletteApplier (MonoBehaviour) → namespace SeedMind.Visual
// PaletteTarget, PaletteColorCategory (데이터 타입) → namespace SeedMind.Visual.Data
namespace SeedMind.Visual
{
    /// <summary>
    /// 계절 전환 시 지정된 Renderer들의 색상을 PaletteData에 따라 교체한다.
    /// MaterialPropertyBlock을 사용하여 Material 인스턴스 복제를 방지한다.
    /// </summary>
    public class PaletteApplier : MonoBehaviour
    {
        [SerializeField] private PaletteData[] seasonPalettes; // 4개 (index = Season enum)
        [SerializeField] private PaletteTarget[] targets;      // 교체 대상 목록

        private MaterialPropertyBlock _propBlock;
        private static readonly int ColorPropID = Shader.PropertyToID("_BaseColor");

        // === 이벤트 구독 ===
        // OnEnable:  TimeManager.OnSeasonChanged += ApplyPalette
        // OnDisable: 구독 해제

        // === 핵심 메서드 ===
        // ApplyPalette(Season season):
        //   PaletteData palette = seasonPalettes[(int)season];
        //   foreach (target in targets):
        //     Color color = ResolveColor(palette, target.colorCategory);
        //     _propBlock.SetColor(ColorPropID, color);
        //     target.renderer.SetPropertyBlock(_propBlock);
    }

    [System.Serializable]
    public struct PaletteTarget
    {
        public Renderer renderer;
        public PaletteColorCategory colorCategory;
    }

    public enum PaletteColorCategory
    {
        Grass,
        Soil,
        Path,
        Leaf,
        Trunk,
        Flower,
        Water,
        WaterFoam
    }
}
```

**MaterialPropertyBlock의 장점**:
- 동일 Material을 공유하는 Renderer들이 개별 색상을 가질 수 있음
- Material 인스턴스 복제가 발생하지 않아 SRP Batcher 호환 유지
- Draw Call 최적화에 기여

[RISK] MaterialPropertyBlock은 SRP Batcher와 함께 사용할 때 일부 조건에서 배칭이 깨질 수 있다. URP 6에서의 호환성을 프로토타입 단계에서 반드시 검증해야 한다.

---

## 4. 작물 비주얼 컴포넌트

### 4.1 CropVisual 클래스 설계

작물의 성장 단계에 따라 메시, 스케일, 색상을 전환하는 컴포넌트. 각 `FarmTile`의 자식으로 배치된다.

```csharp
namespace SeedMind.Farm
{
    /// <summary>
    /// 작물의 시각적 표현을 관리한다.
    /// 성장 단계별 프리팹 활성화, 품질 티어 시각 효과를 처리한다.
    /// </summary>
    public class CropVisual : MonoBehaviour
    {
        [Header("Stage Visuals")]
        [SerializeField] private GameObject[] stagePrefabs; // index 0~3: Seed, Sprout, Growing, Harvestable
                                                            // → see docs/systems/crop-growth.md 섹션 1.1

        [Header("Quality FX")]
        [SerializeField] private GameObject qualityParticle;     // 품질 파티클 루트
        [SerializeField] private ParticleSystem qualityGlowFX;   // 고품질 시 반짝임
        [SerializeField] private Renderer fruitRenderer;          // 열매 Renderer (색상 변경 대상)

        [Header("Animation")]
        [SerializeField] private float harvestableSwaySpeed;  // → see docs/systems/visual-guide.md
        [SerializeField] private float harvestableSwayAngle;  // → see docs/systems/visual-guide.md

        private int _currentStageIndex = -1;
        private MaterialPropertyBlock _propBlock;

        // === 핵심 메서드 ===

        // UpdateStage(int stageIndex):  // 0=Seed, 1=Sprout, 2=Growing, 3=Harvestable
        //   모든 stagePrefabs 비활성화
        //   stagePrefabs[stageIndex] 활성화
        //   if stageIndex == 3: 흔들림 애니메이션 시작
        //   _currentStageIndex = stageIndex

        // SetQualityTier(QualityTier tier):  // → see docs/systems/crop-growth.md 섹션 5
        //   switch (tier):
        //     Normal:     qualityParticle.SetActive(false); fruitRenderer 기본 색상
        //     Good:       qualityParticle.SetActive(false); fruitRenderer 약간 채도 증가
        //     Excellent:  qualityParticle.SetActive(true);  qualityGlowFX 작은 반짝임
        //     Master:     qualityParticle.SetActive(true);  qualityGlowFX 강한 반짝임 + 색상 강조

        // PlayHarvestEffect():
        //   수확 시 파티클 버스트 재생 (잎/열매 흩어짐)

        // PlayWitherEffect():
        //   시든 상태 전환 시 색상을 갈색으로 변경
    }
}
```

**성장 단계 시각 전환 흐름**:

```
GrowthSystem.ProcessDailyGrowth()
    └── CropInstance.GrowthStage 변경
            └── CropVisual.UpdateStage(newStageIndex)
                    ├── 기존 단계 프리팹 비활성화
                    ├── 새 단계 프리팹 활성화
                    └── (Harvestable 시) 흔들림 애니 시작
```

### 4.2 품질 티어 시각적 구분

품질 정의는 `docs/systems/crop-growth.md`가 canonical 출처이다 (→ see docs/systems/crop-growth.md 섹션 5).

| 품질 티어 | 시각 효과 | 구현 방식 |
|-----------|-----------|-----------|
| Normal | 기본 외관 | 변경 없음 |
| Good | 채도 미세 증가 | MaterialPropertyBlock으로 `_BaseColor` 채도 보정 |
| Excellent | 작은 반짝임 파티클 | ParticleSystem 활성화 (작은 별 파티클, Rate=2/s) |
| Master | 강한 반짝임 + 색상 강조 | ParticleSystem 강화 (Rate=5/s) + Emissive 색상 추가 |

### 4.3 Giant Crop 비주얼

대형 작물은 3x3 타일(9타일)을 차지하며 별도의 비주얼 처리가 필요하다 (→ see docs/systems/crop-growth.md 섹션 5.1).

```csharp
// GiantCropVisual은 CropVisual을 상속하여 확장
// - 중앙 타일에만 비주얼 표시
// - 인접 3개 타일의 CropVisual은 비활성화
// - Scale = 2x (기본 크기의 2배)
// - 전용 Giant Mesh 사용 (작물별 giant 프리팹)
```

---

## 5. 날씨 비주얼 이펙트

### 5.1 WeatherVisualController 클래스 설계

```csharp
namespace SeedMind.Visual
{
    /// <summary>
    /// 날씨 상태에 따라 파티클 시스템, 조명 오버레이, 사운드를 제어한다.
    /// WeatherSystem의 이벤트를 구독하여 반응형으로 동작한다.
    /// </summary>
    public class WeatherVisualController : MonoBehaviour
    {
        [Header("Particle Systems")]
        [SerializeField] private ParticleSystem rainParticle;
        [SerializeField] private ParticleSystem heavyRainParticle;
        [SerializeField] private ParticleSystem snowParticle;
        [SerializeField] private ParticleSystem blizzardParticle;
        [SerializeField] private ParticleSystem windLeafParticle;   // 바람에 날리는 낙엽 (가을)

        [Header("Lighting Override")]
        [SerializeField] private float rainLightDimFactor;    // → see docs/systems/visual-guide.md
        [SerializeField] private float stormLightDimFactor;   // → see docs/systems/visual-guide.md
        [SerializeField] private float snowLightDimFactor;    // → see docs/systems/visual-guide.md

        [Header("Fog Override")]
        [SerializeField] private float stormFogDensityAdd;    // → see docs/systems/visual-guide.md
        [SerializeField] private float blizzardFogDensityAdd; // → see docs/systems/visual-guide.md

        [Header("Transition")]
        [SerializeField] private float weatherTransitionDuration; // → see docs/systems/visual-guide.md

        // === 이벤트 구독 ===
        // OnEnable:  WeatherSystem.OnWeatherChanged += HandleWeatherChanged
        // OnDisable: 구독 해제

        // === 핵심 메서드 ===

        // HandleWeatherChanged(WeatherType oldWeather, WeatherType newWeather):
        //   1. StopWeatherFX(oldWeather) -- 기존 파티클 페이드 아웃
        //   2. StartWeatherFX(newWeather) -- 새 파티클 페이드 인
        //   3. ApplyLightingOverride(newWeather) -- 조명 감쇄 적용
        //   4. ApplyFogOverride(newWeather) -- 안개 밀도 추가

        // StopWeatherFX(WeatherType weather):
        //   해당 파티클의 emission rate를 0으로 보간 → Stop()

        // StartWeatherFX(WeatherType weather):
        //   switch (weather):
        //     Clear/Cloudy: 파티클 없음
        //     Rain:         rainParticle.Play()
        //     HeavyRain:    heavyRainParticle.Play()
        //     Storm:        heavyRainParticle.Play() + 화면 흔들림(선택)
        //     Snow:         snowParticle.Play()
        //     Blizzard:     blizzardParticle.Play()
    }
}
```

**날씨별 파티클 파라미터 개요**:

| 날씨 | 파티클 시스템 | 주요 특징 |
|------|-------------|-----------|
| Clear | 없음 | - |
| Cloudy | 없음 | 조명만 미세 감쇄 |
| Rain | rainParticle | 하늘에서 떨어지는 빗방울, Emission Rate (→ see docs/systems/visual-guide.md) |
| HeavyRain | heavyRainParticle | Rain보다 밀도 높은 빗방울 + 바닥 물방울 튀김 sub-emitter |
| Storm | heavyRainParticle + 추가 효과 | 폭우 + 번개 플래시(Light flash) + 화면 미세 흔들림 |
| Snow | snowParticle | 천천히 떨어지는 눈송이, Gravity Modifier 낮음 |
| Blizzard | blizzardParticle | 빠르게 떨어지는 눈 + 옆으로 날리는 효과 + 시야 제한(Fog 강화) |

### 5.2 WeatherVisualController - TimeManager 연동

```
WeatherSystem (TimeManager 하위)
    └── WeatherSystem.OnWeatherChanged(oldType, newType)
            └── WeatherVisualController.HandleWeatherChanged()
                    ├── 파티클 전환
                    ├── LightingManager에 조명 오버라이드 요청
                    │     └── LightingManager.ApplyWeatherOverride(dimFactor)
                    └── RenderSettings.fogDensity 추가 보정
```

**LightingManager와의 협업**: 날씨에 의한 조명 감쇄는 `LightingManager`에 위임한다. `WeatherVisualController`는 감쇄 계수만 전달하고, 실제 Light 프로퍼티 변경은 `LightingManager`가 단독으로 수행한다. 이는 조명 제어 권한을 단일 클래스에 집중시키기 위함이다.

```csharp
// LightingManager 추가 메서드:
// ApplyWeatherOverride(float dimFactor):
//   _weatherDimFactor = dimFactor;
//   현재 sunIntensity에 dimFactor를 곱하여 재적용
//
// ClearWeatherOverride():
//   _weatherDimFactor = 1.0f;
//   원래 sunIntensity로 복원
```

### 5.3 파티클 시스템 씬 배치

```
--- ENVIRONMENT ---
├── WeatherFX (빈 GameObject)
│   ├── RainParticle         # 카메라 추적, 위에서 아래로
│   ├── HeavyRainParticle    # 카메라 추적, 밀도 높음
│   ├── SnowParticle         # 카메라 추적, 느린 낙하
│   ├── BlizzardParticle     # 카메라 추적, 빠른 낙하 + 바람
│   ├── WindLeafParticle     # 계절 파티클 (낙엽)
│   └── WeatherVisualController (MonoBehaviour)
```

모든 날씨 파티클은 카메라를 따라 이동(Camera Follow)하여 플레이어 시야 범위에서만 렌더링된다.

[RISK] 파티클이 카메라를 따라갈 때, 파티클 재시작 시 이전 위치의 입자가 순간 이동하는 현상이 발생할 수 있다. Simulation Space = World로 설정하되, Emission은 카메라 위치 기준으로 수행해야 한다.

---

## 6. 네임스페이스 및 폴더 구조 확장

기존 `docs/systems/project-structure.md`에 비주얼 시스템 관련 추가 사항:

### 6.1 네임스페이스

```
SeedMind.Visual              # 비주얼 시스템 (LightingManager, PaletteApplier, WeatherVisualController)
SeedMind.Visual.Data         # 비주얼 SO 정의 (SeasonLightingProfile, PaletteData, LightingSnapshot)
```

### 6.2 Scripts 폴더 추가

```
Assets/_Project/Scripts/Visual/
├── LightingManager.cs
├── PaletteApplier.cs
├── WeatherVisualController.cs
├── CropVisual.cs              # [OPEN] Farm/ 폴더에 배치할지 Visual/에 배치할지
└── Data/
    ├── SeasonLightingProfile.cs
    ├── PaletteData.cs
    ├── LightingSnapshot.cs
    └── PaletteColorCategory.cs  # enum
```

[OPEN] `CropVisual`의 폴더 배치: 작물 도메인 로직(`SeedMind.Farm`)에 가까우므로 `Scripts/Farm/`에 배치하는 것이 의존성 규칙상 자연스러울 수 있다. 반면 비주얼 전담 로직이므로 `Scripts/Visual/`에 배치하면 관심사 분리가 명확해진다. 프로토타입 후 결정.

### 6.3 Data 에셋 폴더 추가

```
Assets/_Project/Data/Visual/
├── SO_Palette_Spring.asset
├── SO_Palette_Summer.asset
├── SO_Palette_Autumn.asset
├── SO_Palette_Winter.asset
├── SO_LightProfile_Spring.asset
├── SO_LightProfile_Summer.asset
├── SO_LightProfile_Autumn.asset
└── SO_LightProfile_Winter.asset
```

### 6.4 Assembly Definition

| asmdef 파일 | 위치 | 참조하는 asmdef |
|-------------|------|----------------|
| `SeedMind.Visual.asmdef` | `Scripts/Visual/` | Core, Farm |

- Core 참조: `Season`, `DayPhase`, `TimeManager` 이벤트 등 사용
- Farm 참조: `CropVisual`이 `CropInstance` 데이터를 참조해야 할 경우 (단, `CropVisual`을 Farm에 배치하면 불필요)

---

## 7. MCP 태스크 요약 (Part II)

> 상세 MCP 태스크 시퀀스는 `docs/mcp/visual-tasks.md`에서 별도 문서로 분리 예정이다.

### 주요 구현 태스크 개요

| Step | 태스크 | MCP 주요 명령 | 산출물 |
|:----:|--------|--------------|--------|
| 1 | URP 설정 및 Volume Profile 생성 | `execute_menu_item` (URP Asset 생성), `set_property` | URP Asset, Global Volume, VP_Global.asset |
| 2 | LightingManager 생성 및 배치 | `create_object` (Lighting 하위), `add_component`, `set_property` | LightingManager GO + 컴포넌트, Sun Directional Light 참조 연결 |
| 3 | SeasonLightingProfile SO 에셋 생성 (4계절) | `create_scriptable_object` x4, 필드 설정 | SO_LightProfile_Spring~Winter.asset |
| 4 | PaletteData SO 에셋 생성 (4계절) | `create_scriptable_object` x4, 필드 설정 | SO_Palette_Spring~Winter.asset |
| 5 | 날씨 파티클 시스템 생성 | `create_object` (WeatherFX 하위), `add_component` (ParticleSystem) x5 | Rain/Snow/Blizzard 등 파티클 GO |
| 6 | WeatherVisualController 배치 | `add_component`, `set_property` (파티클 참조 연결) | WeatherVisualController GO |
| 7 | CropVisual 프리팹 구성 | 작물별 stage 프리팹 자식 배치, `CropVisual` 컴포넌트 추가 | PFB_Crop_[작물명] 프리팹 업데이트 |
| 8 | 머티리얼 생성 (카테고리별) | `create_material` x N, shader/color 설정 | M_Grass, M_Soil_*, M_Crop_* 등 |

---

## Open Questions

- [OPEN] Custom Toon Shader 도입 여부 -- URP/Lit Smoothness=0만으로 충분한 로우폴리 룩을 달성할 수 있는지 프로토타입에서 검증 필요 (섹션 1.2)
- [OPEN] Outline 포스트프로세싱 적용 여부 -- Depth 기반 Edge Detection으로 외곽선을 추가할지 프로토타입 후 결정 (섹션 1.3)
- [OPEN] `CropVisual` 클래스의 폴더 배치 -- `Scripts/Farm/` vs `Scripts/Visual/` (섹션 6.2)
- [OPEN] Volume Profile 계절 전환 방식 -- Profile 교체 vs 단일 Profile에서 파라미터 보간 (섹션 1.3)

## Risks

- [RISK] Volume Profile 계절 교체 시 시각적 끊김 가능 -- Volume Weight 보간으로 완화 필요 (섹션 1.3)
- [RISK] SeasonData와 SeasonLightingProfile 이중 참조로 데이터 동기화 부담 발생 가능 -- 역할 분리 엄격 유지 (섹션 2.3)
- [RISK] MaterialPropertyBlock이 URP SRP Batcher와 충돌하여 배칭 깨질 가능성 -- 프로토타입 검증 필수 (섹션 3.3)
- [RISK] 날씨 파티클 카메라 추적 시 입자 순간이동 현상 -- Simulation Space=World 설정으로 완화 (섹션 5.3)
- [RISK] DayPhaseVisual 20세트(5시간대 x 4계절) 수동 입력 부담 -- 템플릿 SO 복사 전략 필요 (→ see docs/systems/time-season-architecture.md)

---

## Cross-references

- `docs/systems/visual-guide.md` -- 비주얼 가이드 (색상 HEX값, 파라미터 수치 등 canonical 출처)
- `docs/systems/time-season-architecture.md` -- SeasonData, DayPhaseVisual 정의, 환경 시각 전환 흐름 (섹션 2, 6)
- `docs/systems/time-season.md` -- 계절/날씨 디자인 정의. 비주얼 파라미터 canonical 출처는 visual-guide.md를 경유
- `docs/systems/ui-architecture.md` -- UI 시스템 아키텍처 (UI Canvas 구조)
- `docs/systems/crop-growth.md` -- 작물 성장 단계 정의 (섹션 1), 품질 티어 (섹션 5), Giant Crop (섹션 5.1)
- `docs/systems/project-structure.md` -- 폴더 구조, 네임스페이스, asmdef 규칙
- `docs/pipeline/data-pipeline.md` -- ScriptableObject 파이프라인 구조
- `docs/architecture.md` 섹션 5 -- 렌더링 전략 개요
- `docs/mcp/scene-setup-tasks.md` -- URP 설정 및 머티리얼 생성 MCP 태스크
