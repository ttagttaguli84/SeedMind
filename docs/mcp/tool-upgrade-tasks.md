# 도구 업그레이드 시스템 MCP 태스크 시퀀스 (ARC-015)

> 도구 업그레이드(ToolUpgradeSystem)의 ToolData SO 에셋 생성, 스크립트 생성, 대장간 UI 프리팹, 씬 배치, 통합 테스트를 MCP for Unity 태스크로 상세 정의  
> 작성: Claude Code (Opus) | 2026-04-07  
> Phase 1 | 문서 ID: ARC-015

---

## 개요

### 목적

이 문서는 `docs/systems/tool-upgrade-architecture.md`(DES-007 아키텍처 파트)에서 설계된 도구 업그레이드 시스템을 **Unity Editor에서 MCP 명령만으로 구현**하기 위한 독립 태스크 문서이다. 각 태스크는 MCP for Unity 도구 호출 수준의 구체적인 명세를 포함하며, 호출 순서, 전제 조건, 검증 체크리스트를 명시한다.

**목표**: Unity Editor를 열지 않고 MCP 명령만으로 도구 업그레이드 시스템의 데이터 레이어(ToolData SO 9종 + 재료 SO 2종), 시스템 레이어(스크립트), 대장간 UI 프리팹, 씬 배치, NPC 연동을 완성한다.

### 의존성

```
도구 업그레이드 시스템 MCP 태스크 의존 관계:
├── SeedMind.Core     (TimeManager, SaveManager, EventBus, DataRegistry)
├── SeedMind.Farm     (FarmGrid, FarmTile, ToolSystem -- 도구 사용 시 범위 효과 적용)
├── SeedMind.Player   (InventoryManager, PlayerController -- 도구 슬롯 교체, 재료 차감)
├── SeedMind.Economy  (EconomyManager -- 골드 차감)
├── SeedMind.NPC      (NPCController, DialogueSystem -- 대장간 NPC 연동)
└── SeedMind.UI       (UpgradeUI -- 대장간 업그레이드 패널)
```

(-> see `docs/systems/project-structure.md` 섹션 3, 4 for 의존성 규칙 및 asmdef 구성)

### 완료된 태스크 의존성

| 문서 ID | 문서 | 완료 필수 Phase | 핵심 결과물 |
|---------|------|----------------|------------|
| ARC-002 | `docs/mcp/scene-setup-tasks.md` | Phase A, B 전체 | 폴더 구조, SCN_Farm 기본 계층 (Managers, Farm, Player, UI) |
| ARC-003 | `docs/mcp/farming-tasks.md` | Phase A~C 전체 | FarmGrid 타일, CropData SO, ToolData SO 기본 구조, ToolSystem |
| ARC-007 | `docs/mcp/facilities-tasks.md` | (문서 참조) | BuildingManager, BuildingEvents 클래스 |

### 이미 존재하는 오브젝트 (중복 생성 금지)

| 오브젝트/에셋 | 출처 |
|--------------|------|
| `Canvas_Overlay` (UI 루트) | ARC-002 Phase B |
| `DataRegistry` (SO 로드 시스템) | `docs/pipeline/data-pipeline.md` Part II |
| `Assets/_Project/Data/` 폴더 구조 | ARC-002 Phase A |
| `--- MANAGERS ---` (씬 계층 부모) | ARC-002 Phase B |
| `--- PLAYER ---` (씬 계층 부모) | ARC-002 Phase B |
| `InventoryManager` (인벤토리 시스템) | `docs/systems/inventory-architecture.md` |
| `EconomyManager` (경제 시스템) | `docs/systems/economy-architecture.md` |
| `ToolSystem` (도구 사용 처리) | ARC-003 |
| `NPCController`, `DialogueSystem` | NPC 시스템 구현 (별도) |

### 총 MCP 호출 예상 수

| 태스크 | 호출 수 |
|--------|--------|
| T-1: ToolData SO 에셋 생성 (도구 9종 + 재료 2종) | 98회 |
| T-2: 스크립트 생성 (ToolUpgradeSystem, ToolEffectResolver 등) | 14회 |
| T-3: 대장간 UI 프리팹 생성 (BlacksmithPanel) | 32회 |
| T-4: 씬 배치 및 연결 | 12회 |
| T-5: 통합 테스트 시퀀스 | 16회 |
| **합계** | **~172회** |

[RISK] 총 172회 MCP 호출은 상당하다. ToolData SO 9종의 필드가 많아 set_property 호출이 빈번하다. Editor 스크립트(CreateToolAssets.cs)를 통한 SO 에셋 일괄 생성으로 T-1의 98회를 ~8회로 감소시킬 수 있다. MCP 단독 실행 시 시간 비용이 크므로 Editor 스크립트 우회를 권장한다.

---

## MCP 도구 매핑

| MCP 도구 | 용도 | 사용 태스크 |
|----------|------|-----------|
| `create_folder` | 에셋 폴더 생성 | T-1 |
| `create_scriptable_object` | SO 에셋 인스턴스 생성 | T-1 |
| `set_property` | SO 필드값 설정, 컴포넌트 프로퍼티 설정 | T-1~T-4 전체 |
| `create_script` | C# 스크립트 파일 생성 | T-2 |
| `create_object` | 빈 GameObject 생성 | T-3, T-4 |
| `add_component` | MonoBehaviour 컴포넌트 부착 | T-3, T-4 |
| `set_parent` | 오브젝트 부모 설정 | T-3, T-4 |
| `save_as_prefab` | GameObject를 프리팹으로 저장 | T-3 |
| `save_scene` | 씬 저장 | T-4, T-5 |
| `enter_play_mode` / `exit_play_mode` | 테스트 실행/종료 | T-5 |
| `execute_menu_item` | 편집기 명령 실행 (컴파일 대기 등) | T-2, T-3 |
| `execute_method` | 런타임 메서드 호출 (테스트) | T-5 |
| `get_console_logs` | 콘솔 로그 확인 (테스트) | T-5 |

[RISK] `create_scriptable_object`, `save_as_prefab` 도구의 가용 여부 및 파라미터 형식 사전 검증 필요. SO 인스턴스 생성이나 프리팹 저장이 MCP에서 미지원인 경우, Editor 스크립트를 통한 우회 필요. (-> see `docs/architecture.md` [RISK] MCP SO 배열/참조 설정 관련)

---

## Part I: 시스템 설계 요약

### 1.1 도구 업그레이드 개요

도구 3종(호미/물뿌리개/낫) x 3등급(Basic/Reinforced/Legendary) 체계. 플레이어가 대장간 NPC에게 골드 + 재료를 지불하여 상위 등급으로 업그레이드한다.

- 등급 체계: Basic(Lv.1) -> Reinforced(Lv.2) -> Legendary(Lv.3) (-> see `docs/systems/tool-upgrade.md` 섹션 1.1)
- 업그레이드 비용/재료/레벨 요건: (-> see `docs/systems/tool-upgrade.md` 섹션 2.1)
- 업그레이드 절차: 대장간 방문 -> 도구 선택 -> 골드/재료 차감 -> 소요 일수 경과 -> 수령 (-> see `docs/systems/tool-upgrade.md` 섹션 2.3)
- 업그레이드 중 해당 도구 사용 불가 (대장간에 맡긴 상태)
- 도구별 등급 효과(범위, 에너지, 특수 효과): (-> see `docs/systems/tool-upgrade.md` 섹션 3)

### 1.2 스크립트 목록

MCP `add_component`는 컴파일 완료된 스크립트만 부착할 수 있으므로, 아래 스크립트를 태스크 순서대로 작성해야 한다.

| # | 파일 경로 | 클래스 | 네임스페이스 | 생성 태스크 |
|---|----------|--------|-------------|-----------|
| T-01 | `Scripts/Player/Data/ToolSpecialEffect.cs` | `ToolSpecialEffect` (enum) | `SeedMind.Player` | T-2 Phase 1 |
| T-02 | `Scripts/Player/Data/PendingUpgrade.cs` | `PendingUpgrade` | `SeedMind.Player` | T-2 Phase 1 |
| T-03 | `Scripts/Player/Data/ToolUpgradeInfo.cs` | `ToolUpgradeInfo` (struct), `UpgradeCheckResult` (struct), `ToolUpgradeFailReason` (enum), `UpgradeCostInfo` (struct) | `SeedMind.Player` | T-2 Phase 1 |
| T-04 | `Scripts/Player/Data/ToolUpgradeSaveData.cs` | `ToolUpgradeSaveData`, `PendingUpgradeSaveEntry` | `SeedMind.Player` | T-2 Phase 1 |
| T-05 | `Scripts/Player/ToolEffectResolver.cs` | `ToolEffectResolver` (static) | `SeedMind.Player` | T-2 Phase 2 |
| T-06 | `Scripts/Player/ToolUpgradeEvents.cs` | `ToolUpgradeEvents` (static) | `SeedMind.Player` | T-2 Phase 2 |
| T-07 | `Scripts/Player/ToolUpgradeSystem.cs` | `ToolUpgradeSystem` (MonoBehaviour) | `SeedMind.Player` | T-2 Phase 3 |
| T-08 | `Scripts/UI/BlacksmithPanelUI.cs` | `BlacksmithPanelUI` (MonoBehaviour) | `SeedMind.UI` | T-3 |

(모든 경로 접두어: `Assets/_Project/`)

[OPEN] `docs/systems/tool-upgrade-architecture.md` 섹션 9.1에는 `ToolUpgradeSlotUI.cs`가 별도 스크립트로 등재되어 있으나, 본 문서 스크립트 목록에는 포함되지 않았다. T-3-03의 UpgradeSlot 오브젝트가 별도 MonoBehaviour를 필요로 하는지, BlacksmithPanelUI 내부에서 처리하는지 구현 시 결정 필요. 별도 스크립트가 필요하다면 T-09로 추가한다.

[RISK] 스크립트에 컴파일 에러가 있으면 MCP `add_component`가 실패한다. 컴파일 순서: T-01~T-04 -> T-05/T-06 -> T-07 -> T-08. 각 Phase 사이에 Unity 컴파일 대기가 필요하다.

### 1.3 SO 에셋 목록

#### 1.3.1 ToolData SO (도구 9종)

등급별 별도 SO + nextTier 참조 체인 구조 (-> see `docs/systems/tool-upgrade-architecture.md` 섹션 1.1).

| 에셋명 | dataId | 도구 | 등급 | nextTier 참조 |
|--------|--------|------|------|--------------|
| `SO_Tool_Hoe_Basic.asset` | `hoe_basic` | 호미 | Basic (tier=1) | -> SO_Tool_Hoe_Reinforced |
| `SO_Tool_Hoe_Reinforced.asset` | `hoe_reinforced` | 호미 | Reinforced (tier=2) | -> SO_Tool_Hoe_Legendary |
| `SO_Tool_Hoe_Legendary.asset` | `hoe_legendary` | 호미 | Legendary (tier=3) | null |
| `SO_Tool_WateringCan_Basic.asset` | `wateringcan_basic` | 물뿌리개 | Basic (tier=1) | -> SO_Tool_WateringCan_Reinforced |
| `SO_Tool_WateringCan_Reinforced.asset` | `wateringcan_reinforced` | 물뿌리개 | Reinforced (tier=2) | -> SO_Tool_WateringCan_Legendary |
| `SO_Tool_WateringCan_Legendary.asset` | `wateringcan_legendary` | 물뿌리개 | Legendary (tier=3) | null |
| `SO_Tool_Sickle_Basic.asset` | `sickle_basic` | 낫 | Basic (tier=1) | -> SO_Tool_Sickle_Reinforced |
| `SO_Tool_Sickle_Reinforced.asset` | `sickle_reinforced` | 낫 | Reinforced (tier=2) | -> SO_Tool_Sickle_Legendary |
| `SO_Tool_Sickle_Legendary.asset` | `sickle_legendary` | 낫 | Legendary (tier=3) | null |

저장 경로: `Assets/_Project/Data/Tools/`

**필드값**: 각 SO의 range, energyCost, cooldown, upgradeGoldCost, upgradeTimeDays, upgradeMaterials, specialEffect 수치는 MCP 실행 시점에 canonical 문서에서 읽어 입력한다 (-> see `docs/systems/tool-upgrade.md` 섹션 2.1, 3.1~3.3, 9). 본 문서에서 수치를 직접 기재하지 않는다 (PATTERN-006).

#### 1.3.2 재료 아이템 SO (2종)

업그레이드 재료 아이템. IInventoryItem을 구현하는 ScriptableObject로, 기존 아이템 SO 구조를 따른다.

| 에셋명 | dataId | 표시명 | 설명 |
|--------|--------|--------|------|
| `SO_Material_IronScrap.asset` | `iron_scrap` | 철 조각 | Reinforced 업그레이드 재료 |
| `SO_Material_RefinedSteel.asset` | `refined_steel` | 정제 강철 | Legendary 업그레이드 재료 |

저장 경로: `Assets/_Project/Data/Materials/`

**가격**: (-> see `docs/systems/tool-upgrade.md` 섹션 2.2)

### 1.4 프리팹/UI 목록

| 프리팹명 | 경로 | 설명 |
|---------|------|------|
| `PFB_UI_BlacksmithPanel.prefab` | `Assets/_Project/Prefabs/UI/` | 대장간 업그레이드 UI 패널 |
| `PFB_UI_UpgradeSlot.prefab` | `Assets/_Project/Prefabs/UI/` | 업그레이드 도구 슬롯 (3개 인스턴스) |
| `PFB_UI_UpgradeConfirmPopup.prefab` | `Assets/_Project/Prefabs/UI/` | 업그레이드 확인 팝업 |

---

## Part II: MCP 태스크 시퀀스

---

### Phase T-1: ToolData ScriptableObject 에셋 생성

**목적**: 도구 3종 x 3등급 = 9종의 ToolData SO 에셋과 업그레이드 재료 2종의 SO 에셋을 생성한다.

**전제**: ToolData.cs, ToolType enum이 컴파일 완료된 상태 (ARC-003 구현 완료). IInventoryItem 인터페이스 정의 완료.

**의존 태스크**: T-2 Phase 1 (데이터 구조 스크립트) 완료 후 실행

#### T-1-01: 에셋 폴더 생성

```
create_folder
  path: "Assets/_Project/Data/Tools"

create_folder
  path: "Assets/_Project/Data/Materials"
```

- **MCP 호출**: 2회

#### T-1-02: 호미 Basic ToolData SO

```
create_scriptable_object
  type: "SeedMind.Player.Data.ToolData"
  asset_path: "Assets/_Project/Data/Tools/SO_Tool_Hoe_Basic.asset"

set_property  target: "SO_Tool_Hoe_Basic"
  dataId = "hoe_basic"
  displayName = "호미"
  toolType = 0                                // Hoe (int cast)
  tier = 1                                    // Basic
  range = 0                                   // (-> see docs/systems/tool-upgrade.md 섹션 3.1)
  energyCost = 0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.1)
  cooldown = 0.0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.1)
  useSpeed = 1.0
  upgradeGoldCost = 0                         // (-> see docs/systems/tool-upgrade.md 섹션 2.1)
  upgradeTimeDays = 0                         // (-> see docs/systems/tool-upgrade.md 섹션 2.1)
  specialEffect = ""                          // Basic 등급: 특수 효과 없음
  description = "기본 호미. 땅을 갈아 경작지를 만든다."
```

> **주의**: `range`, `energyCost`, `cooldown`, `upgradeGoldCost`, `upgradeTimeDays`의 구체적 수치는 MCP 실행 시점에 canonical 문서(`docs/systems/tool-upgrade.md` 섹션 2.1, 3.1)에서 읽어 입력한다. 본 문서에서 수치를 직접 기재하지 않는다 (PATTERN-006, PATTERN-007).

- **MCP 호출**: 1(생성) + 10(필드 설정) = 11회

#### T-1-03: 호미 Reinforced ToolData SO

```
create_scriptable_object
  type: "SeedMind.Player.Data.ToolData"
  asset_path: "Assets/_Project/Data/Tools/SO_Tool_Hoe_Reinforced.asset"

set_property  target: "SO_Tool_Hoe_Reinforced"
  dataId = "hoe_reinforced"
  displayName = "강화 호미"
  toolType = 0                                // Hoe
  tier = 2                                    // Reinforced
  range = 0                                   // (-> see docs/systems/tool-upgrade.md 섹션 3.1)
  energyCost = 0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.1)
  cooldown = 0.0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.1)
  useSpeed = 1.0
  upgradeGoldCost = 0                         // (-> see docs/systems/tool-upgrade.md 섹션 2.1)
  upgradeTimeDays = 0                         // (-> see docs/systems/tool-upgrade.md 섹션 2.1)
  specialEffect = "AreaEffect"               // 돌 자동 제거 (-> see docs/systems/tool-upgrade.md 섹션 3.1)
  description = "강화된 호미. 1x3 범위로 경작하며 돌을 자동 제거한다."
```

- **MCP 호출**: 11회

#### T-1-04: 호미 Legendary ToolData SO

```
create_scriptable_object
  type: "SeedMind.Player.Data.ToolData"
  asset_path: "Assets/_Project/Data/Tools/SO_Tool_Hoe_Legendary.asset"

set_property  target: "SO_Tool_Hoe_Legendary"
  dataId = "hoe_legendary"
  displayName = "전설 호미"
  toolType = 0                                // Hoe
  tier = 3                                    // Legendary
  range = 0                                   // (-> see docs/systems/tool-upgrade.md 섹션 3.1)
  energyCost = 0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.1)
  cooldown = 0.0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.1)
  useSpeed = 1.0
  upgradeGoldCost = 0                         // Legendary가 최종 등급이므로 0
  upgradeTimeDays = 0                         // 최종 등급이므로 0
  specialEffect = "AreaEffect"               // 돌+잡초 자동 제거 (-> see docs/systems/tool-upgrade.md 섹션 3.1)
  description = "전설의 호미. 3x3 범위로 경작하며 돌과 잡초를 자동 제거한다."
```

- **MCP 호출**: 11회

#### T-1-05: 물뿌리개 Basic ToolData SO

```
create_scriptable_object
  type: "SeedMind.Player.Data.ToolData"
  asset_path: "Assets/_Project/Data/Tools/SO_Tool_WateringCan_Basic.asset"

set_property  target: "SO_Tool_WateringCan_Basic"
  dataId = "wateringcan_basic"
  displayName = "물뿌리개"
  toolType = 1                                // WateringCan (int cast)
  tier = 1                                    // Basic
  range = 0                                   // (-> see docs/systems/tool-upgrade.md 섹션 3.2)
  energyCost = 0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.2)
  cooldown = 0.0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.2)
  useSpeed = 1.0
  upgradeGoldCost = 0                         // (-> see docs/systems/tool-upgrade.md 섹션 2.1)
  upgradeTimeDays = 0                         // (-> see docs/systems/tool-upgrade.md 섹션 2.1)
  specialEffect = ""                          // Basic 등급: 특수 효과 없음
  description = "기본 물뿌리개. 작물에 물을 준다."
```

- **MCP 호출**: 11회

#### T-1-06: 물뿌리개 Reinforced ToolData SO

```
create_scriptable_object
  type: "SeedMind.Player.Data.ToolData"
  asset_path: "Assets/_Project/Data/Tools/SO_Tool_WateringCan_Reinforced.asset"

set_property  target: "SO_Tool_WateringCan_Reinforced"
  dataId = "wateringcan_reinforced"
  displayName = "강화 물뿌리개"
  toolType = 1                                // WateringCan
  tier = 2                                    // Reinforced
  range = 0                                   // (-> see docs/systems/tool-upgrade.md 섹션 3.2)
  energyCost = 0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.2)
  cooldown = 0.0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.2)
  useSpeed = 1.0
  upgradeGoldCost = 0                         // (-> see docs/systems/tool-upgrade.md 섹션 2.1)
  upgradeTimeDays = 0                         // (-> see docs/systems/tool-upgrade.md 섹션 2.1)
  specialEffect = ""                          // Reinforced 물뿌리개: 범위 확대만
  description = "강화된 물뿌리개. 1x3 범위에 물을 뿌린다."
```

- **MCP 호출**: 11회

#### T-1-07: 물뿌리개 Legendary ToolData SO

```
create_scriptable_object
  type: "SeedMind.Player.Data.ToolData"
  asset_path: "Assets/_Project/Data/Tools/SO_Tool_WateringCan_Legendary.asset"

set_property  target: "SO_Tool_WateringCan_Legendary"
  dataId = "wateringcan_legendary"
  displayName = "전설 물뿌리개"
  toolType = 1                                // WateringCan
  tier = 3                                    // Legendary
  range = 0                                   // (-> see docs/systems/tool-upgrade.md 섹션 3.2)
  energyCost = 0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.2)
  cooldown = 0.0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.2)
  useSpeed = 1.0
  upgradeGoldCost = 0                         // 최종 등급이므로 0
  upgradeTimeDays = 0                         // 최종 등급이므로 0
  specialEffect = "QualityBoost"             // 물 준 타일 성장 속도 보너스 (-> see docs/systems/tool-upgrade.md 섹션 3.2)
  description = "전설의 물뿌리개. 3x3 범위에 물을 뿌리고 성장 속도를 높인다."
```

- **MCP 호출**: 11회

#### T-1-08: 낫 Basic ToolData SO

```
create_scriptable_object
  type: "SeedMind.Player.Data.ToolData"
  asset_path: "Assets/_Project/Data/Tools/SO_Tool_Sickle_Basic.asset"

set_property  target: "SO_Tool_Sickle_Basic"
  dataId = "sickle_basic"
  displayName = "낫"
  toolType = 3                                // Sickle (int cast)
  tier = 1                                    // Basic
  range = 0                                   // (-> see docs/systems/tool-upgrade.md 섹션 3.3)
  energyCost = 0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.3)
  cooldown = 0.0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.3)
  useSpeed = 1.0
  upgradeGoldCost = 0                         // (-> see docs/systems/tool-upgrade.md 섹션 2.1)
  upgradeTimeDays = 0                         // (-> see docs/systems/tool-upgrade.md 섹션 2.1)
  specialEffect = ""                          // Basic 등급: 특수 효과 없음
  description = "기본 낫. 작물을 수확한다."
```

- **MCP 호출**: 11회

#### T-1-09: 낫 Reinforced ToolData SO

```
create_scriptable_object
  type: "SeedMind.Player.Data.ToolData"
  asset_path: "Assets/_Project/Data/Tools/SO_Tool_Sickle_Reinforced.asset"

set_property  target: "SO_Tool_Sickle_Reinforced"
  dataId = "sickle_reinforced"
  displayName = "강화 낫"
  toolType = 3                                // Sickle
  tier = 2                                    // Reinforced
  range = 0                                   // (-> see docs/systems/tool-upgrade.md 섹션 3.3)
  energyCost = 0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.3)
  cooldown = 0.0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.3)
  useSpeed = 1.0
  upgradeGoldCost = 0                         // (-> see docs/systems/tool-upgrade.md 섹션 2.1)
  upgradeTimeDays = 0                         // (-> see docs/systems/tool-upgrade.md 섹션 2.1)
  specialEffect = "DoubleHarvest"            // 보너스 수확 확률 (-> see docs/systems/tool-upgrade.md 섹션 3.3)
  description = "강화된 낫. 1x3 범위로 수확하며 보너스 수확 확률이 있다."
```

- **MCP 호출**: 11회

#### T-1-10: 낫 Legendary ToolData SO

```
create_scriptable_object
  type: "SeedMind.Player.Data.ToolData"
  asset_path: "Assets/_Project/Data/Tools/SO_Tool_Sickle_Legendary.asset"

set_property  target: "SO_Tool_Sickle_Legendary"
  dataId = "sickle_legendary"
  displayName = "전설 낫"
  toolType = 3                                // Sickle
  tier = 3                                    // Legendary
  range = 0                                   // (-> see docs/systems/tool-upgrade.md 섹션 3.3)
  energyCost = 0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.3)
  cooldown = 0.0                              // (-> see docs/systems/tool-upgrade.md 섹션 3.3)
  useSpeed = 1.0
  upgradeGoldCost = 0                         // 최종 등급이므로 0
  upgradeTimeDays = 0                         // 최종 등급이므로 0
  specialEffect = "DoubleHarvest"            // [OPEN] ToolSpecialEffect enum에 단일 값으로 표현. 전설 낫의 3가지 효과(보너스 수확+품질 상승+씨앗 회수)를 단일 enum 값으로 표현하는 방식은 tool-upgrade-architecture.md Open Question에서 결정 필요 (-> see docs/systems/tool-upgrade.md 섹션 3.3)
  description = "전설의 낫. 3x3 범위로 수확하며 보너스 수확, 품질 상승, 씨앗 회수 효과가 있다."
```

- **MCP 호출**: 11회

#### T-1-11: nextTier 참조 연결

ToolData SO 간의 nextTier 체인을 설정한다.

```
set_property  target: "SO_Tool_Hoe_Basic"
  nextTier = "SO_Tool_Hoe_Reinforced"         // SO 참조

set_property  target: "SO_Tool_Hoe_Reinforced"
  nextTier = "SO_Tool_Hoe_Legendary"

set_property  target: "SO_Tool_WateringCan_Basic"
  nextTier = "SO_Tool_WateringCan_Reinforced"

set_property  target: "SO_Tool_WateringCan_Reinforced"
  nextTier = "SO_Tool_WateringCan_Legendary"

set_property  target: "SO_Tool_Sickle_Basic"
  nextTier = "SO_Tool_Sickle_Reinforced"

set_property  target: "SO_Tool_Sickle_Reinforced"
  nextTier = "SO_Tool_Sickle_Legendary"
```

[RISK] SO 간 직접 참조를 MCP로 설정 불가할 수 있다. 이 경우 Editor 스크립트에서 dataId 기반으로 nextTier 참조를 자동 연결하는 헬퍼를 작성하여 MCP `execute_method`로 실행한다. (-> see `docs/pipeline/data-pipeline.md` Part II 섹션 3 [RISK] SO 간 참조)

- **MCP 호출**: 6회

#### T-1-12: upgradeMaterials 배열 설정

각 Basic/Reinforced 등급 ToolData에 upgradeaterials 배열을 설정한다.

```
// 호미 Basic -> Reinforced 재료
set_property  target: "SO_Tool_Hoe_Basic"
  upgradeMaterials = [{ materialId: "iron_scrap", quantity: 0 }]
  // quantity (-> see docs/systems/tool-upgrade.md 섹션 2.1)

// 호미 Reinforced -> Legendary 재료
set_property  target: "SO_Tool_Hoe_Reinforced"
  upgradeMaterials = [{ materialId: "refined_steel", quantity: 0 }]
  // quantity (-> see docs/systems/tool-upgrade.md 섹션 2.1)

// 물뿌리개 Basic -> Reinforced 재료
set_property  target: "SO_Tool_WateringCan_Basic"
  upgradeMaterials = [{ materialId: "iron_scrap", quantity: 0 }]

// 물뿌리개 Reinforced -> Legendary 재료
set_property  target: "SO_Tool_WateringCan_Reinforced"
  upgradeMaterials = [{ materialId: "refined_steel", quantity: 0 }]

// 낫 Basic -> Reinforced 재료
set_property  target: "SO_Tool_Sickle_Basic"
  upgradeMaterials = [{ materialId: "iron_scrap", quantity: 0 }]

// 낫 Reinforced -> Legendary 재료
set_property  target: "SO_Tool_Sickle_Reinforced"
  upgradeMaterials = [{ materialId: "refined_steel", quantity: 0 }]
```

> **주의**: `quantity` 수치는 MCP 실행 시점에 canonical 문서(`docs/systems/tool-upgrade.md` 섹션 2.1)에서 읽어 입력한다 (PATTERN-006).

[RISK] MCP가 SO의 배열/리스트 필드를 직접 설정할 수 있는지 불확실. 배열 설정 불가 시 Editor 스크립트 우회 필요. (-> see `docs/pipeline/data-pipeline.md` Part II 섹션 3)

- **MCP 호출**: 6회

#### T-1-13: 재료 아이템 SO 생성

```
create_scriptable_object
  type: "SeedMind.Player.Data.MaterialItemData"   // 또는 기존 IInventoryItem 구현 SO
  asset_path: "Assets/_Project/Data/Materials/SO_Material_IronScrap.asset"

set_property  target: "SO_Material_IronScrap"
  dataId = "iron_scrap"
  displayName = "철 조각"
  description = "도구 강화에 사용하는 재료. 대장간에서 구매할 수 있다."
  category = 4                                // Material (ItemCategory enum)
  isStackable = true
  maxStack = 99
  isSellable = false
  baseSellPrice = 0                           // 판매 불가
  buyPrice = 0                                // (-> see docs/systems/tool-upgrade.md 섹션 2.2)

create_scriptable_object
  type: "SeedMind.Player.Data.MaterialItemData"
  asset_path: "Assets/_Project/Data/Materials/SO_Material_RefinedSteel.asset"

set_property  target: "SO_Material_RefinedSteel"
  dataId = "refined_steel"
  displayName = "정제 강철"
  description = "고급 도구 제작에 필요한 재료. 대장간에서 구매할 수 있다."
  category = 4                                // Material
  isStackable = true
  maxStack = 99
  isSellable = false
  baseSellPrice = 0
  buyPrice = 0                                // (-> see docs/systems/tool-upgrade.md 섹션 2.2)
```

- **MCP 호출**: 2(생성) + 12(필드 설정) = 14회

#### T-1 검증 체크리스트

- [ ] ToolData SO 9종이 `Assets/_Project/Data/Tools/`에 생성되었는가
- [ ] 각 SO의 dataId가 유일하고 네이밍 규칙(`{도구}_{등급}`)을 따르는가
- [ ] nextTier 체인이 Basic -> Reinforced -> Legendary -> null로 올바르게 연결되었는가
- [ ] Legendary 등급의 nextTier가 null인가
- [ ] upgradeMaterials 배열이 올바르게 설정되었는가
- [ ] 재료 SO 2종이 `Assets/_Project/Data/Materials/`에 생성되었는가
- [ ] DataRegistry에서 모든 SO를 dataId로 조회 가능한가

---

### Phase T-2: 스크립트 생성

**목적**: 도구 업그레이드 시스템의 핵심 스크립트 8개를 생성한다.

**전제**: ToolData.cs, ToolType enum, IInventoryItem 인터페이스가 컴파일 완료된 상태 (ARC-003).

#### T-2 Phase 1: 데이터 구조 스크립트 (T-01 ~ T-04)

```
create_script
  path: "Assets/_Project/Scripts/Player/Data/ToolSpecialEffect.cs"
  content: |
    // T-01: 도구 특수 효과 enum
    // → see docs/systems/tool-upgrade-architecture.md 섹션 3.5
    namespace SeedMind.Player
    {
        public enum ToolSpecialEffect
        {
            None,
            AreaEffect,
            ChargeAttack,
            AutoWater,
            QualityBoost,
            DoubleHarvest
        }
    }
```

```
create_script
  path: "Assets/_Project/Scripts/Player/Data/PendingUpgrade.cs"
  content: |
    // T-02: 업그레이드 진행 상태
    // → see docs/systems/tool-upgrade-architecture.md 섹션 3.1
    namespace SeedMind.Player
    {
        [System.Serializable]
        public class PendingUpgrade
        {
            public ToolType toolType;
            public string currentToolId;
            public string targetToolId;
            public int remainingDays;       // → see docs/systems/tool-upgrade.md
            public int totalDays;           // → see docs/systems/tool-upgrade.md
        }
    }
```

```
create_script
  path: "Assets/_Project/Scripts/Player/Data/ToolUpgradeInfo.cs"
  content: |
    // T-03: 업그레이드 이벤트 페이로드 및 검증 구조체
    // → see docs/systems/tool-upgrade-architecture.md 섹션 3.2~3.4
    namespace SeedMind.Player
    {
        public struct ToolUpgradeInfo { ... }
        public struct UpgradeCheckResult { ... }
        public enum ToolUpgradeFailReason { ... }
        public struct UpgradeCostInfo { ... }
    }
    // 전체 필드 정의: → see docs/systems/tool-upgrade-architecture.md 섹션 3
```

```
create_script
  path: "Assets/_Project/Scripts/Player/Data/ToolUpgradeSaveData.cs"
  content: |
    // T-04: 업그레이드 세이브 데이터
    // → see docs/systems/tool-upgrade-architecture.md 섹션 7.2
    namespace SeedMind.Player
    {
        [System.Serializable]
        public class ToolUpgradeSaveData { ... }
        [System.Serializable]
        public class PendingUpgradeSaveEntry { ... }
    }
    // 전체 필드 정의: → see docs/systems/tool-upgrade-architecture.md 섹션 7.2
```

- **MCP 호출**: 4회
- **완료 후**: `execute_menu_item` -> Unity 컴파일 대기

#### T-2 Phase 2: 유틸리티/이벤트 스크립트 (T-05, T-06)

```
create_script
  path: "Assets/_Project/Scripts/Player/ToolEffectResolver.cs"
  content: |
    // T-05: 도구 등급별 효과 계산 유틸리티 (static)
    // → see docs/systems/tool-upgrade-architecture.md 섹션 4
    // 모든 수치의 canonical 출처: → see docs/systems/tool-upgrade.md
    namespace SeedMind.Player
    {
        public static class ToolEffectResolver
        {
            public static int GetEffectiveRange(ToolData tool) => tool.range;
            public static int GetEnergyCost(ToolData tool) => tool.energyCost;
            public static float GetUseSpeed(ToolData tool) => tool.useSpeed;
            public static int GetWateringCapacity(ToolData tool) { ... }
            public static ToolSpecialEffect GetSpecialEffect(ToolData tool) { ... }
            public static Vector2Int[] GetTilePattern(ToolData tool) { ... }
        }
    }
    // 전체 구현: → see docs/systems/tool-upgrade-architecture.md 섹션 4
```

```
create_script
  path: "Assets/_Project/Scripts/Player/ToolUpgradeEvents.cs"
  content: |
    // T-06: 도구 업그레이드 이벤트 허브 (static)
    // → see docs/systems/npc-shop-architecture.md 섹션 4 (기존 이벤트 패턴 계승)
    namespace SeedMind.Player
    {
        public static class ToolUpgradeEvents
        {
            public static event Action<ToolUpgradeInfo> OnUpgradeStarted;
            public static event Action<ToolUpgradeInfo> OnUpgradeCompleted;
            public static event Action<ToolUpgradeFailReason> OnUpgradeFailed;

            internal static void RaiseUpgradeStarted(ToolUpgradeInfo info) => OnUpgradeStarted?.Invoke(info);
            internal static void RaiseUpgradeCompleted(ToolUpgradeInfo info) => OnUpgradeCompleted?.Invoke(info);
            internal static void RaiseUpgradeFailed(ToolUpgradeFailReason reason) => OnUpgradeFailed?.Invoke(reason);
        }
    }
```

- **MCP 호출**: 2회
- **완료 후**: `execute_menu_item` -> Unity 컴파일 대기

#### T-2 Phase 3: 핵심 시스템 스크립트 (T-07)

```
create_script
  path: "Assets/_Project/Scripts/Player/ToolUpgradeSystem.cs"
  content: |
    // T-07: 도구 업그레이드 시스템 (MonoBehaviour)
    // → see docs/systems/tool-upgrade-architecture.md 섹션 2, 5
    namespace SeedMind.Player
    {
        public class ToolUpgradeSystem : MonoBehaviour
        {
            // [참조]
            // - _inventoryManager: InventoryManager
            // - _economyManager: EconomyManager
            // - _toolRegistry: ToolData[]
            //
            // [상태]
            // - _pendingUpgrades: Dictionary<ToolType, PendingUpgrade>
            //
            // [메서드]
            // + CanUpgrade(ToolData current): UpgradeCheckResult
            // + StartUpgrade(ToolData current): bool
            // + CompleteUpgrade(ToolType toolType): void
            // + CancelUpgrade(ToolType toolType): bool
            // + GetPendingUpgrade(ToolType toolType): PendingUpgrade?
            // + GetUpgradeCost(ToolData current): UpgradeCostInfo
            //
            // [구독]
            // + OnEnable(): TimeManager.OnDayChanged += ProcessUpgradeTimers
            //
            // 전체 구현: → see docs/systems/tool-upgrade-architecture.md 섹션 5
        }
    }
```

- **MCP 호출**: 1회
- **완료 후**: `execute_menu_item` -> Unity 컴파일 대기

#### T-2 Phase 4: UI 스크립트 (T-08)

```
create_script
  path: "Assets/_Project/Scripts/UI/BlacksmithPanelUI.cs"
  content: |
    // T-08: 대장간 UI 패널 (MonoBehaviour)
    // 도구 업그레이드, 재료 구매, 도구 수령 3가지 서비스 제공
    // → see docs/systems/tool-upgrade.md 섹션 6.2 (상호작용 흐름)
    // → see docs/systems/npc-shop-architecture.md 섹션 5 (NPC -> 서비스 위임)
    namespace SeedMind.UI
    {
        public class BlacksmithPanelUI : MonoBehaviour
        {
            // [참조]
            // - _upgradeSystem: ToolUpgradeSystem
            // - _inventoryManager: InventoryManager
            //
            // [UI 요소]
            // - _upgradeSlots: UpgradeSlotUI[]  (3개: 호미, 물뿌리개, 낫)
            // - _confirmPopup: GameObject
            // - _materialShopPanel: GameObject
            // - _collectPanel: GameObject
            //
            // [메서드]
            // + Show(): void
            // + Hide(): void
            // + ShowUpgradeMenu(): void
            // + ShowMaterialShop(): void
            // + ShowCollectMenu(): void
            // + OnToolSelected(ToolData tool): void
            // + OnUpgradeConfirmed(): void
            // + OnUpgradeCancelled(): void
            //
            // [이벤트 구독]
            // + ToolUpgradeEvents.OnUpgradeStarted -> RefreshUI
            // + ToolUpgradeEvents.OnUpgradeCompleted -> RefreshUI, 알림 표시
        }
    }
```

- **MCP 호출**: 1회
- **완료 후**: `execute_menu_item` -> Unity 컴파일 대기

#### T-2 검증 체크리스트

- [ ] 8개 스크립트가 모두 올바른 경로에 생성되었는가
- [ ] Unity에서 컴파일 에러 없이 통과하는가
- [ ] ToolUpgradeSystem이 ToolUpgradeEvents를 통해 이벤트를 발행하는가
- [ ] ToolEffectResolver가 ToolData SO를 읽어 효과 수치를 반환하는가
- [ ] ToolUpgradeSaveData가 PendingUpgrade 배열을 직렬화할 수 있는가

---

### Phase T-3: 대장간 UI 프리팹 생성 (BlacksmithPanel)

**목적**: 대장간 NPC 상호작용 시 표시되는 업그레이드 UI 프리팹을 생성한다.

**전제**: T-2 완료 (모든 스크립트 컴파일 성공), Canvas_Overlay 존재.

#### T-3-01: BlacksmithPanel 루트 생성

```
create_object
  name: "BlacksmithPanel"
  parent: "Canvas_Overlay"

add_component  target: "BlacksmithPanel"
  type: "RectTransform"

set_property  target: "BlacksmithPanel.RectTransform"
  anchorMin = { x: 0.15, y: 0.1 }
  anchorMax = { x: 0.85, y: 0.9 }
  offsetMin = { x: 0, y: 0 }
  offsetMax = { x: 0, y: 0 }

add_component  target: "BlacksmithPanel"
  type: "UnityEngine.UI.Image"

set_property  target: "BlacksmithPanel.Image"
  color = { r: 0.12, g: 0.1, b: 0.08, a: 0.95 }    // 대장간 어두운 톤 배경

add_component  target: "BlacksmithPanel"
  type: "SeedMind.UI.BlacksmithPanelUI"
```

- **MCP 호출**: 5회

#### T-3-02: 상단 탭 버튼 영역

```
create_object
  name: "TabBar"
  parent: "BlacksmithPanel"

// 3개 탭 버튼: 도구 업그레이드 | 재료 구매 | 도구 수령
// → see docs/systems/tool-upgrade.md 섹션 6.2 (상호작용 흐름)
create_object  name: "Tab_Upgrade"  parent: "TabBar"
  // Button + Text "도구 업그레이드"

create_object  name: "Tab_Materials"  parent: "TabBar"
  // Button + Text "재료 구매"

create_object  name: "Tab_Collect"  parent: "TabBar"
  // Button + Text "도구 수령" (완성된 도구가 있을 때만 활성화)
```

- **MCP 호출**: 4회 (부모 생성 + 탭 3개)

#### T-3-03: 업그레이드 메뉴 패널

```
create_object
  name: "UpgradePanel"
  parent: "BlacksmithPanel"

// 도구 3종에 대한 업그레이드 슬롯
create_object  name: "UpgradeSlot_Hoe"  parent: "UpgradePanel"
  // 도구 아이콘, 현재 등급, 다음 등급, 비용 표시, 상태(업그레이드 가능/불가/진행 중)

create_object  name: "UpgradeSlot_WateringCan"  parent: "UpgradePanel"

create_object  name: "UpgradeSlot_Sickle"  parent: "UpgradePanel"
```

각 UpgradeSlot의 내부 구조:
```
UpgradeSlot_XXX
├── ToolIcon (Image)                           // 현재 도구 아이콘
├── ToolName (TextMeshProUGUI)                 // "호미"
├── CurrentTier (TextMeshProUGUI)              // "기본" / "강화" / "전설"
├── Arrow (Image)                              // -> 화살표
├── NextTier (TextMeshProUGUI)                 // "강화" / "전설" / "최대 등급"
├── CostPanel
│   ├── GoldCost (TextMeshProUGUI)             // "800G" (-> see docs/systems/tool-upgrade.md 섹션 2.1)
│   ├── MaterialIcon (Image)
│   └── MaterialCost (TextMeshProUGUI)         // "철 조각 x3" (-> see docs/systems/tool-upgrade.md 섹션 2.1)
├── RequirementText (TextMeshProUGUI)          // "레벨 3 필요" (-> see docs/systems/tool-upgrade.md 섹션 2.1)
├── StatusText (TextMeshProUGUI)               // "업그레이드 가능" / "골드 부족" / "진행 중 (1일 남음)"
└── UpgradeButton (Button)                     // 조건 미충족 시 비활성화
```

- **MCP 호출**: 4(부모+3슬롯) + 9(슬롯당 내부 요소 3개 x 3) = 13회

#### T-3-04: 확인 팝업

```
create_object
  name: "UpgradeConfirmPopup"
  parent: "BlacksmithPanel"

// 초기 비활성화
set_property  target: "UpgradeConfirmPopup"
  activeSelf = false

// 내부 구조:
// - 배경 딤
// - 팝업 패널
//   - 제목: "호미를 강화 등급으로 업그레이드합니다."
//   - 비용 정보: "비용: 800G + 철 조각 x3" (수치는 런타임에 ToolUpgradeSystem에서 조회)
//   - 소요 시간: "소요 시간: 1일"
//   - 경고: "업그레이드 중 호미를 사용할 수 없습니다."
//   - [확인] [취소] 버튼
```

- **MCP 호출**: 5회

#### T-3-05: 재료 구매 패널 (기존 ShopUI 재사용)

```
create_object
  name: "MaterialShopPanel"
  parent: "BlacksmithPanel"

// 대장간 상점 판매 품목 표시 영역
// 기존 ShopUI 패턴을 재사용 (-> see docs/systems/economy-architecture.md 섹션 4.3)
// 철 조각, 정제 강철 2종만 표시
// 가격은 런타임에 SO에서 조회 (-> see docs/systems/tool-upgrade.md 섹션 6.3)
```

- **MCP 호출**: 2회

#### T-3-06: 도구 수령 패널

```
create_object
  name: "CollectPanel"
  parent: "BlacksmithPanel"

// 완성된 업그레이드 도구 목록 표시
// 수령 버튼 클릭 시 ToolUpgradeSystem.CompleteUpgrade() 호출
```

- **MCP 호출**: 2회

#### T-3-07: 프리팹 저장

```
save_as_prefab
  target: "BlacksmithPanel"
  path: "Assets/_Project/Prefabs/UI/PFB_UI_BlacksmithPanel.prefab"

set_property  target: "BlacksmithPanel"
  activeSelf = false                           // 기본 비활성화 (NPC 상호작용 시 활성화)
```

- **MCP 호출**: 2회

#### T-3 검증 체크리스트

- [ ] BlacksmithPanel 프리팹이 `Assets/_Project/Prefabs/UI/`에 저장되었는가
- [ ] BlacksmithPanelUI 컴포넌트가 부착되어 있는가
- [ ] 3개 탭(업그레이드/재료 구매/도구 수령)이 존재하는가
- [ ] 업그레이드 슬롯 3개(호미/물뿌리개/낫)가 UpgradePanel 하위에 존재하는가
- [ ] UpgradeConfirmPopup이 기본 비활성화 상태인가
- [ ] BlacksmithPanel이 기본 비활성화 상태인가

---

### Phase T-4: 씬 배치 및 연결

**목적**: SCN_Farm 씬에 ToolUpgradeSystem을 배치하고, 대장간 NPC(BlacksmithNPC)와 연결한다.

**전제**: T-1~T-3 완료. NPCController, DialogueSystem이 씬에 배치된 상태.

#### T-4-01: ToolUpgradeSystem GameObject 배치

```
create_object
  name: "ToolUpgradeSystem"

set_parent
  target: "ToolUpgradeSystem"
  parent: "--- MANAGERS ---"

add_component  target: "ToolUpgradeSystem"
  type: "SeedMind.Player.ToolUpgradeSystem"
```

- **MCP 호출**: 3회

#### T-4-02: ToolUpgradeSystem 참조 연결

```
set_property  target: "ToolUpgradeSystem.ToolUpgradeSystem"
  _inventoryManager = "InventoryManager"       // 씬 내 오브젝트 참조
  _economyManager = "EconomyManager"           // 씬 내 오브젝트 참조
  _toolRegistry = [                            // ToolData SO 배열 참조
    "SO_Tool_Hoe_Basic",
    "SO_Tool_Hoe_Reinforced",
    "SO_Tool_Hoe_Legendary",
    "SO_Tool_WateringCan_Basic",
    "SO_Tool_WateringCan_Reinforced",
    "SO_Tool_WateringCan_Legendary",
    "SO_Tool_Sickle_Basic",
    "SO_Tool_Sickle_Reinforced",
    "SO_Tool_Sickle_Legendary"
  ]
```

[RISK] SO 배열 참조 설정이 MCP로 불가할 수 있다. Editor 스크립트 우회 필요. (-> see `docs/pipeline/data-pipeline.md` Part II 섹션 3)

- **MCP 호출**: 3회

#### T-4-03: 대장간 NPC와 연결

```
// NPCController(Blacksmith)의 _upgradeSystem 참조 설정
set_property  target: "NPC_Blacksmith.NPCController"
  _upgradeSystem = "ToolUpgradeSystem"         // 씬 내 오브젝트 참조

// BlacksmithPanel UI를 NPCController에 연결
set_property  target: "NPC_Blacksmith.NPCController"
  _upgradePanel = "BlacksmithPanel"            // UI 패널 참조
```

- **MCP 호출**: 2회

#### T-4-04: BlacksmithPanelUI 참조 연결

```
set_property  target: "BlacksmithPanel.BlacksmithPanelUI"
  _upgradeSystem = "ToolUpgradeSystem"
  _inventoryManager = "InventoryManager"
```

- **MCP 호출**: 2회

#### T-4-05: 씬 저장

```
save_scene
```

- **MCP 호출**: 1회

#### T-4 검증 체크리스트

- [ ] ToolUpgradeSystem이 `--- MANAGERS ---` 하위에 배치되었는가
- [ ] ToolUpgradeSystem의 _inventoryManager, _economyManager 참조가 올바른가
- [ ] ToolUpgradeSystem의 _toolRegistry에 9종 ToolData SO가 모두 등록되었는가
- [ ] NPCController(Blacksmith)의 _upgradeSystem 참조가 ToolUpgradeSystem을 가리키는가
- [ ] BlacksmithPanelUI의 _upgradeSystem 참조가 올바른가
- [ ] 씬이 정상 저장되었는가

---

### Phase T-5: 통합 테스트 시퀀스

**목적**: 도구 업그레이드 시스템의 핵심 기능을 MCP 콘솔에서 검증한다.

**전제**: T-1~T-4 완료. 모든 스크립트 컴파일 성공, 씬 저장 완료.

#### T-5-01: 기본 검증 (데이터 무결성)

```
enter_play_mode

// ToolData SO 로드 확인
execute_method
  type: "SeedMind.Core.DataRegistry"
  method: "Get"
  params: ["hoe_basic"]
  // 기대: ToolData SO 반환, null이 아님

execute_method
  type: "SeedMind.Core.DataRegistry"
  method: "Get"
  params: ["hoe_legendary"]
  // 기대: ToolData SO 반환, tier=3

get_console_logs
  // 기대: 에러 로그 없음
```

- **MCP 호출**: 4회

#### T-5-02: nextTier 체인 검증

```
// 호미 체인: Basic -> Reinforced -> Legendary -> null
execute_method
  type: "SeedMind.Player.ToolUpgradeSystem"
  method: "CanUpgrade"
  params: ["hoe_basic"]
  // 기대: canUpgrade = true (골드/재료/레벨 충족 시)

execute_method
  type: "SeedMind.Player.ToolUpgradeSystem"
  method: "CanUpgrade"
  params: ["hoe_legendary"]
  // 기대: canUpgrade = false, failReason = AlreadyMaxTier

get_console_logs
```

- **MCP 호출**: 3회

#### T-5-03: 업그레이드 실행 테스트

```
// 테스트용 골드/재료 추가
execute_method
  type: "SeedMind.Economy.EconomyManager"
  method: "AddGold"
  params: [10000]                              // 충분한 골드

execute_method
  type: "SeedMind.Player.InventoryManager"
  method: "AddItem"
  params: ["iron_scrap", 10]                   // 충분한 재료

// 호미 Basic -> Reinforced 업그레이드 시작
execute_method
  type: "SeedMind.Player.ToolUpgradeSystem"
  method: "StartUpgrade"
  params: ["hoe_basic"]
  // 기대: true 반환, OnUpgradeStarted 이벤트 발행

get_console_logs
  // 기대: "[ToolUpgrade] 호미 업그레이드 시작: Basic -> Reinforced"
```

- **MCP 호출**: 4회

#### T-5-04: 업그레이드 완료 테스트 (시간 경과 시뮬레이션)

```
// 소요 일수만큼 시간 진행 시뮬레이션
execute_method
  type: "SeedMind.Core.TimeManager"
  method: "AdvanceDay"
  // 기대: ToolUpgradeSystem.ProcessUpgradeTimers() 호출됨

// 도구 수령 확인
execute_method
  type: "SeedMind.Player.ToolUpgradeSystem"
  method: "GetPendingUpgrade"
  params: [0]                                  // ToolType.Hoe
  // 기대: remainingDays가 감소했거나 null (완료 시)

get_console_logs
  // 기대: "[ToolUpgrade] 호미 업그레이드 완료: Reinforced"
```

- **MCP 호출**: 3회

#### T-5-05: 종료

```
exit_play_mode

save_scene
```

- **MCP 호출**: 2회

#### T-5 검증 체크리스트

- [ ] 모든 ToolData SO가 DataRegistry에서 조회 가능한가
- [ ] nextTier 체인이 올바르게 작동하는가 (Basic -> Reinforced -> Legendary -> null)
- [ ] CanUpgrade()가 최대 등급 도구에 대해 AlreadyMaxTier를 반환하는가
- [ ] StartUpgrade()가 골드/재료를 차감하고 도구를 잠그는가
- [ ] ProcessUpgradeTimers()가 매일 remainingDays를 감소시키는가
- [ ] CompleteUpgrade()가 인벤토리에 업그레이드된 도구를 추가하는가
- [ ] ToolUpgradeEvents 이벤트가 정상 발행되는가
- [ ] 콘솔에 에러 로그가 없는가

---

## Cross-references

| 문서 | 참조 내용 |
|------|-----------|
| `docs/systems/tool-upgrade.md` (DES-007) | 도구 업그레이드 설계 canonical (등급, 비용, 재료, 도구별 효과) |
| `docs/systems/tool-upgrade-architecture.md` | 기술 아키텍처 (클래스 설계, 데이터 흐름, 이벤트) |
| `docs/systems/inventory-architecture.md` | InventoryManager 클래스 (도구 슬롯 교체, 재료 차감) |
| `docs/systems/inventory-system.md` | 아이템 분류 (ItemType.Tool, toolTier 속성) |
| `docs/content/npcs.md` (CON-003) | 대장간 NPC 철수 (Cheolsu) 캐릭터 설정 |
| `docs/systems/npc-shop-architecture.md` (ARC-008) | NPCController, DialogueSystem, ToolUpgradeSystem 연동 흐름 |
| `docs/pipeline/data-pipeline.md` | ToolData SO 필드 정의 (섹션 2.3), GameDataSO 상속, DataRegistry |
| `docs/systems/project-structure.md` | 스크립트 폴더 구조, 네임스페이스, asmdef 의존성 |
| `docs/systems/farming-architecture.md` | ToolSystem, ToolData 기존 정의 |
| `docs/systems/economy-architecture.md` | EconomyManager (골드 차감), ShopSystem (재료 구매) |
| `docs/mcp/facilities-tasks.md` (ARC-007) | MCP 태스크 문서 패턴 참조 |
| `docs/mcp/processing-tasks.md` (ARC-014) | MCP 태스크 문서 패턴 참조 |

---

## Open Questions

1. [OPEN] **ToolData SO 에셋 수 확정**: `data-pipeline.md`에서 "17~22 (3종x5등급 + 2종 단일, 확장 시 최대 22)"로 기재되어 있으나, `tool-upgrade.md`에서 3등급으로 축소됨. 3종x3등급 + 씨앗봉투/손 각 1종 = 총 11종이 맞는지 확인 필요. data-pipeline.md의 에셋 수 갱신 필요.

2. [OPEN] **MaterialItemData SO 클래스**: 재료 아이템(철 조각, 정제 강철)을 위한 SO 클래스가 별도로 필요한지, 기존 GameDataSO + IInventoryItem으로 충분한지 결정 필요. 현재 설계에서는 범용 MaterialItemData SO를 가정했으나, 기존 아키텍처에서 아이템 종류별 SO 클래스 분리 여부를 확인해야 한다.

3. [OPEN] **ToolType enum 값**: 낫(Sickle)의 ToolType int 값이 3인지 확인 필요. `farming-architecture.md`의 ToolType enum 정의와 대조해야 한다. 호미=0, 물뿌리개=1, 씨앗봉투=2, 낫=3 가정.

4. [OPEN] **대장간 ShopData SO**: 대장간의 재료 판매 기능은 기존 ShopSystem을 재사용할 것인지, BlacksmithPanelUI에 직접 구현할 것인지. `npc-shop-architecture.md` 섹션 5.1에서 대장간이 ShopSystem과 연동되는 것으로 기술되어 있으므로, ShopData SO("SO_Shop_Blacksmith")를 별도로 생성해야 할 수 있다.

5. [OPEN] **전설 낫 specialEffect 단일 값 표현 한계**: 전설 낫은 보너스 수확 + 품질 상승 + 씨앗 회수 3가지 효과를 가진다 (-> see `docs/systems/tool-upgrade.md` 섹션 3.3). 현재 `ToolSpecialEffect` enum의 단일 값(`DoubleHarvest`)으로는 3가지 효과를 동시에 표현할 수 없다. 해결 방안: (a) enum을 [Flags] 비트마스크로 변경, (b) specialEffect 필드를 string[]으로 변경, (c) ToolEffectResolver에서 tier 값으로 직접 분기하는 방식 채택. `docs/systems/tool-upgrade-architecture.md` 섹션 3.5 및 4의 GetSpecialEffect() 구현과 함께 결정 필요.

---

## Risks

1. [RISK] **MCP SO 배열/참조 설정**: upgradeMaterials (UpgradeMaterial[]) 배열과 nextTier (ToolData 참조)를 MCP로 설정할 수 없을 가능성이 높다. Editor 스크립트 우회가 거의 확실히 필요하다. (-> see `docs/pipeline/data-pipeline.md` Part II 섹션 3)

2. [RISK] **컴파일 순서 의존성**: T-01~T-04 데이터 구조 스크립트가 먼저 컴파일되어야 T-05~T-07이 컴파일 가능하다. Phase 간 Unity 컴파일 대기가 필수이며, 한 Phase에서 컴파일 에러 발생 시 후속 Phase 전체가 차단된다.

3. [RISK] **기존 ToolData.cs 수정 필요**: `data-pipeline.md`에서 정의한 ToolData 확장 필드(upgradeMaterials, upgradeGoldCost, upgradeTimeDays, specialEffect)가 현재 ToolData.cs에 반영되어 있는지 확인 필요. ARC-003에서 생성한 ToolData.cs가 기본 필드만 가지고 있다면, 확장 필드를 추가하는 스크립트 수정 태스크가 T-2 이전에 필요하다.

4. [RISK] **NPC 시스템 미구현 시 T-4 차단**: Phase T-4의 대장간 NPC 연결은 NPCController, DialogueSystem이 씬에 배치된 상태를 전제한다. NPC 시스템 MCP 태스크가 별도로 진행되어야 하며, 미완료 시 T-4는 ToolUpgradeSystem 단독 배치만 가능하고 NPC 연결은 보류된다.

5. [RISK] **data-pipeline.md 에셋 수 불일치**: ToolData 에셋 수가 "17~22"로 기재되어 있으나 실제 3등급 체계에서는 11종이다. 문서 갱신이 필요하다.

---

*이 문서는 도구 업그레이드 시스템의 MCP 구현 태스크를 정의한다. 수치(비용, 효과, 재료 수량)의 canonical 출처는 `docs/systems/tool-upgrade.md`이다.*
