# SeedMind — Technical Architecture

> 기술 아키텍처 문서  
> 작성: Claude Code (Opus) | 2026-04-06

---

## 1. 엔진 및 프로젝트 설정

| 항목 | 선택 | 근거 |
|------|------|------|
| **엔진** | Unity 6 | 로컬 설치 완료, MCP 연동 가능 |
| **렌더 파이프라인** | URP (Universal Render Pipeline) | 가볍고 스타일라이즈드에 적합 |
| **스크립팅** | C# | Unity 표준 |
| **AI 연동** | MCP for Unity | Claude Code → Unity Editor 직접 제어 |
| **빌드 타겟** | Windows 64-bit (Mono) | 단일 플랫폼으로 복잡도 최소화 |

---

## 2. MCP 활용 전략

Claude Code가 Unity MCP를 통해 직접 수행하는 작업:

| 작업 | MCP 기능 |
|------|----------|
| 씬 구성 | GameObject 생성, 위치/회전/스케일 설정 |
| 프리팹 생성 | 오브젝트 조합 → 프리팹화 |
| 머티리얼 설정 | 색상, 셰이더 파라미터 조정 |
| 스크립트 연결 | 컴포넌트 추가, 프로퍼티 설정 |
| 씬 테스트 | Play Mode 진입, 런타임 상태 확인 |

코드 작성은 Claude Code가 직접 파일로 작성하고, MCP로 Unity에 반영.

---

## 3. 프로젝트 구조

```
Assets/_Project/                    # (→ see docs/systems/project-structure.md for details)
├── Scripts/
│   ├── Core/                    # 게임 프레임워크
│   │   ├── GameManager.cs       # 전체 게임 상태 관리
│   │   ├── TimeManager.cs       # 시간/계절 시스템
│   │   └── SaveManager.cs       # 저장/불러오기
│   │
│   ├── Farm/                    # 농장 시스템
│   │   ├── FarmGrid.cs          # 타일 그리드 관리
│   │   ├── FarmTile.cs          # 개별 타일 상태
│   │   ├── CropData.cs          # 작물 데이터 (ScriptableObject)
│   │   ├── CropInstance.cs      # 심어진 작물 인스턴스
│   │   └── GrowthSystem.cs      # 성장 로직
│   │
│   ├── Player/                  # 플레이어
│   │   ├── PlayerController.cs  # 이동, 입력 처리
│   │   ├── PlayerInventory.cs   # 인벤토리
│   │   └── ToolSystem.cs        # 도구 사용 로직
│   │
│   ├── Economy/                 # 경제 시스템 (→ see docs/systems/economy-architecture.md for details)
│   │   ├── EconomyManager.cs    # 골드, 거래, 가격 조회
│   │   ├── ShopSystem.cs        # 상점 로직
│   │   ├── PriceFluctuationSystem.cs  # 가격 변동 계산
│   │   ├── TransactionLog.cs    # 거래 기록
│   │   └── Data/
│   │       ├── EconomyConfig.cs # 경제 설정 ScriptableObject
│   │       ├── PriceData.cs     # 가격 데이터 ScriptableObject
│   │       └── ShopData.cs      # 상점 데이터 ScriptableObject
│   │
│   ├── Building/                # 건설 시스템
│   │   ├── BuildingManager.cs   # 시설 배치
│   │   ├── BuildingData.cs      # 시설 데이터
│   │   └── Buildings/           # 개별 시설 로직
│   │       ├── WaterTank.cs
│   │       ├── Greenhouse.cs
│   │       ├── Storage.cs
│   │       └── Processor.cs
│   │
│   ├── NPC/                     # NPC 시스템 (→ see docs/systems/npc-shop-architecture.md)
│   │   ├── NPCManager.cs       # NPC 레지스트리 관리
│   │   ├── NPCController.cs    # 개별 NPC 행동 제어
│   │   ├── DialogueSystem.cs   # 대화 흐름 관리
│   │   ├── TravelingMerchantScheduler.cs  # 여행 상인 스케줄
│   │   ├── NPCEvents.cs        # 정적 이벤트 허브
│   │   └── Data/
│   │       ├── NPCData.cs      # NPC 데이터 (ScriptableObject)
│   │       ├── DialogueData.cs # 대화 데이터 (ScriptableObject)
│   │       └── TravelingShopPoolData.cs
│   │
│   ├── Level/                   # 진행 시스템 (→ see docs/systems/progression-architecture.md)
│   │   ├── ProgressionManager.cs # XP/레벨/해금/마일스톤 관리
│   │   ├── UnlockRegistry.cs    # 해금 상태 관리
│   │   ├── MilestoneTracker.cs  # 마일스톤 추적
│   │   └── Data/
│   │       ├── ProgressionData.cs # 진행 설정 SO
│   │       └── MilestoneData.cs   # 마일스톤 데이터
│   │
│   ├── Tutorial/                # 튜토리얼 시스템 (→ see docs/systems/tutorial-architecture.md)
│   │   ├── TutorialManager.cs   # 싱글턴, 시퀀스 진행 제어
│   │   ├── TutorialTriggerSystem.cs  # 이벤트 기반 트리거
│   │   ├── ContextHintSystem.cs # 상황별 자동 힌트
│   │   ├── TutorialEvents.cs   # 정적 이벤트 허브
│   │   └── Data/
│   │       ├── TutorialSequenceData.cs  # 시퀀스 SO
│   │       ├── TutorialStepData.cs      # 단계 SO
│   │       └── ContextHintData.cs       # 힌트 SO
│   │
│   └── UI/                      # UI 시스템
│       ├── HUDController.cs     # 메인 HUD
│       ├── InventoryUI.cs       # 인벤토리 UI
│       ├── ShopUI.cs            # 상점 UI
│       ├── DialogueUI.cs        # 대화/알림 UI
│       └── ProcessingUI.cs     # 가공소 UI (→ see docs/systems/processing-architecture.md)
│
├── Data/                        # ScriptableObject 데이터
│   ├── Crops/                   # 작물별 데이터 에셋
│   ├── Buildings/               # 시설별 데이터 에셋
│   ├── Recipes/                 # 가공 레시피 데이터 에셋 (→ see docs/systems/processing-architecture.md)
│   ├── Tools/                   # 도구별 데이터 에셋
│   ├── NPCs/                    # NPC 데이터 에셋
│   └── Dialogues/               # 대화 데이터 에셋
│
├── Prefabs/
│   ├── Player/                  # 플레이어 프리팹
│   ├── Crops/                   # 작물 단계별 프리팹
│   ├── Buildings/               # 시설 프리팹
│   ├── Environment/             # 환경 오브젝트
│   └── UI/                      # UI 프리팹
│
├── Materials/
│   ├── Terrain/                 # 지형 머티리얼
│   ├── Crops/                   # 작물 머티리얼
│   └── Buildings/               # 시설 머티리얼
│
├── Scenes/
│   ├── SCN_MainMenu.unity       # 메인 메뉴
│   ├── SCN_Loading.unity        # 로딩 씬
│   └── SCN_Farm.unity           # 메인 농장 씬 (상점은 UI 오버레이로 처리, → see docs/systems/project-structure.md)
│
└── Resources/
    └── UI/                      # UI 스프라이트, 폰트
```

---

## 4. 핵심 시스템 상세

### 4.1 Farm Grid 시스템

```
농장 = N×M 타일 그리드

각 타일 상태:
├── Empty       (빈 땅)
├── Tilled      (경작된 땅)
├── Planted     (씨앗 심어짐)
├── Watered     (물 줌)
├── Dry         (물 증발, 성장 정지)
├── Harvestable (수확 가능)
├── Withered    (작물 고사)
└── Building    (시설 설치됨)

(→ see docs/systems/farming-system.md 섹션 2.1 for canonical state list)
```

- 초기 농장: 8×8 그리드
- 확장 가능: 최대 16×16
- 타일 크기: 1 Unity Unit (1m)

### 4.2 작물 성장

```csharp
// 핵심 데이터 구조 (ScriptableObject)
[CreateAssetMenu]
public class CropData : ScriptableObject
{
    public string cropName;
    public int growthDays;        // 성장에 필요한 일수
    public int sellPrice;         // 판매가
    public int seedPrice;         // 씨앗 가격
    public int unlockLevel;       // 해금 레벨
    public GameObject[] growthStagePrefabs;  // 단계별 외형
}
```

성장 단계: 4단계 (씨앗 → 새싹 → 성장 → 수확 가능)
- 매일 아침 성장 체크
- 전날 물을 줬으면 성장 +1
- 비료 사용 시 성장 +2

### 4.3 시간 시스템

```
TimeManager:
├── currentDay (1~28)
├── currentSeason (Spring/Summer/Autumn/Winter)
├── currentYear (1~)
├── currentHour (6:00~24:00)
└── timeScale (실시간 대비 배속)

이벤트:
├── OnHourChanged
├── OnDayChanged
├── OnSeasonChanged
└── OnYearChanged
```

- 게임 내 1시간 = 실시간 약 33초 (1일 = 10분)
- 하루는 6:00 시작, 24:00에 자동 종료 → 다음 날

### 4.4 입력 시스템

Unity New Input System 사용:

```
InputActions:
├── Player/Move (WASD, 방향키)
├── Player/UseTool (마우스 좌클릭)
├── Player/Interact (E)
├── Player/Inventory (Tab)
├── Player/ToolSelect (1~5)
├── Player/Zoom (마우스 휠)
└── Player/Menu (Esc)
```

---

## 5. 렌더링 전략

### URP 설정
- Forward Rendering
- Anti-Aliasing: MSAA 4x
- 그림자: Soft Shadow (단일 Directional Light)

### 머티리얼
- 모든 오브젝트: **단색 또는 2톤 머티리얼** (로우폴리 스타일)
- 셰이더: URP/Lit 기본, 필요 시 커스텀 Toon 셰이더
- 색상 팔레트: 따뜻한 파스텔 톤

### 라이팅
- 단일 Directional Light (태양)
- 시간대에 따라 색온도/각도 변경
- 간단한 앰비언트 라이트

---

## 6. 데이터 관리

- **ScriptableObject** 기반: 작물, 시설, 도구 등 모든 게임 데이터
- **JSON 저장**: 플레이어 진행 상태 저장/불러오기
- 하드코딩 최소화 → 데이터 드리븐 설계

---

## 7. 빌드 파이프라인

```
Unity Editor (개발/테스트)
    ↓
Development Build (디버깅 포함)
    ↓
Release Build (최적화)
    ↓
최종 산출물 (.exe + Data 폴더)
```

---

## Cross-references

- `docs/design.md` — 게임 디자인 마스터 문서 (시스템 개요, 작물/시설 데이터 canonical)
- `docs/systems/project-structure.md` — 프로젝트 구조 상세 (폴더, 네임스페이스, asmdef)
- `docs/systems/farming-architecture.md` — 경작 시스템 기술 아키텍처 상세
- `docs/systems/time-season-architecture.md` — 시간/계절/날씨 시스템 기술 아키텍처 상세 (DES-003)
- `docs/systems/economy-architecture.md` — 경제 시스템 기술 아키텍처 상세 (DES-004)
- `docs/mcp/scene-setup-tasks.md` — 기본 씬 구성 MCP 태스크 시퀀스 (ARC-002)
- `docs/mcp/farming-tasks.md` — 농장 그리드 MCP 태스크 시퀀스 (ARC-003)
- `docs/pipeline/data-pipeline.md` — 데이터 파이프라인 설계: SO 구조, 세이브/로드, JSON 스키마 (ARC-004)
- `docs/balance/crop-economy.md` — 작물 경제 밸런스 시트: ROI, 시뮬레이션, 조정 제안 (BAL-001)
- `docs/systems/progression-architecture.md` — 진행 시스템 기술 아키텍처: XP/레벨/해금/마일스톤 (BAL-002)
- `docs/mcp/progression-tasks.md` — 진행 시스템 MCP 태스크 시퀀스 (BAL-002-MCP)
- `docs/systems/facilities-architecture.md` — 시설 시스템 기술 아키텍처: 물탱크/온실/창고/가공소 (ARC-007)
- `docs/content/facilities.md` — 시설 콘텐츠 상세: 메카닉/업그레이드/가공 레시피 canonical (CON-002)
- `docs/systems/tool-upgrade-architecture.md` — 도구 업그레이드 시스템 기술 아키텍처: 업그레이드 흐름/효과 계산/세이브 확장 (DES-007)
- `docs/systems/npc-shop-architecture.md` — NPC/상점 시스템 기술 아키텍처: NPC 관리/대화/대장간 연계/여행 상인 (ARC-008)
- `docs/systems/tutorial-architecture.md` — 튜토리얼/온보딩 시스템 기술 아키텍처: 튜토리얼 매니저/트리거/UI/상황별 힌트 (DES-006)
- `docs/systems/save-load-architecture.md` — 세이브/로드 시스템 기술 아키텍처: SaveManager, GameSaveData 통합 루트, 자동저장 트리거, 비동기 API, 오류 처리 (ARC-011)
- `docs/systems/save-load-system.md` — 세이브/로드 UX 설계: 자동저장 트리거, 수동 저장, 멀티슬롯 구조, 게임 종료/시작 흐름 (DES-008)
- `docs/content/processing-system.md` — 가공/요리 시스템 콘텐츠: 레시피 32종 canonical, 특화 가공소 3종(제분소/발효실/베이커리), 연료 시스템, 가공품 퀄리티 (CON-005)
- `docs/systems/processing-architecture.md` — 가공/요리 시스템 기술 아키텍처: ProcessingSystem, RecipeData, 가공 큐/슬롯, 이벤트/저장 (ARC-012)

## Open Questions

- [OPEN] URP vs HDRP: 현재 URP 선택이나, 커스텀 Toon 셰이더 필요 시 재검토
- [OPEN] Addressables 도입 시점: 초기에는 Resources 폴더 사용, 에셋 규모 증가 시 전환

## Risks

- [RISK] MCP for Unity의 ScriptableObject 배열/참조 필드 설정 지원 범위가 불확실
- [RISK] Unity 6 + URP 조합에서 로우폴리 셰이더 호환성 사전 검증 필요

---

*이 문서는 Claude Code가 기술적 제약과 설계 목표를 고려하여 자율적으로 작성했습니다.*
