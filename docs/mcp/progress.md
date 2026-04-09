# SeedMind — MCP 태스크 실행 진행 상황

> 최종 업데이트: 2026-04-10 (blacksmith-tasks.md T-1~T-5 완료)
> **갱신 규칙**: 각 MCP 태스크 파일 실행 완료 직후 해당 항목을 ✅로 바꾸고 커밋. 세션 종료와 무관하게 완료 즉시 갱신한다.
> 새 세션 시작 시 `/start`가 이 파일을 읽어 진행 위치를 복원한다.

---

## 현재 위치

**Phase D — Feature Systems**
- 다음 실행 파일: `quest-tasks.md` (D-5)

---

## Phase A — Foundation ✅

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `scene-setup-tasks.md` (ARC-002) | ✅ 완료 | SCN_Farm, SCN_MainMenu, SCN_Loading, Build Settings 등록 완료 |

---

## Phase B — Core Systems

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `farming-tasks.md` (ARC-003) | ✅ 완료 | Phase A~C 완료 (그리드 64타일, 작물SO, 도구SO, 스크립트 컴포넌트, 레이어). Phase D(Play Mode 검증) 미실행 |
| `save-load-tasks.md` (ARC-012) | ✅ 완료 | T-1~T-5 완료 (Singleton/ISaveable/SaveEvents/SaveManager/AutoSaveTrigger/Data 클래스 생성, 씬 배치). T-6(이벤트 연결)은 TimeManager/BuildingEvents 구현 후 처리. T-7(SaveSlotPanel UI), T-8(PauseMenu 연동), T-9(통합 테스트)는 UI 시스템(Phase E) 이후 처리 |
| `time-season-tasks.md` (ARC-021) | ✅ 완료 | Phase A~D 완료 (스크립트 12개, SO 13개, TimeSystem GO 배치). HUDController 시간 표시 연결 및 E-2 저장/로드 테스트는 UI 시스템(Phase E) 이후 처리 |
| `progression-tasks.md` (BAL-002-MCP) | ✅ 완료 | Phase A~B 완료 (스크립트 9개, SO_ProgressionData 생성, ProgressionManager GO 배치, LevelBarUI 연결). Phase C(런타임 검증) execute_code 비활성으로 스킵. unlockTable/milestones 배열은 빈 상태(콘텐츠 확정 후 에디터에서 입력 예정) |

---

## Phase C — Content

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `crop-content-tasks.md` (CON-001-ARC) | ✅ 완료 | Phase A~E 완료. DataRegistry 기본 구조 생성(Resources 배치는 inventory-tasks에서). 검증(V-1~V-4)은 execute_code 비활성으로 스킵. |
| `facilities-tasks.md` (ARC-007) | ✅ 완료 | F-1~F-4 완료 (스크립트 16종, SO 7종, 프리팹 8종, BuildingManager 씬 배치). F-5/F-6(UI 패널), F-8(통합 테스트)는 UI Phase E 이후 처리. |
| `inventory-tasks.md` (ARC-013) | ✅ 완료 | T-1~T-4 완료. CropQuality/HarvestOrigin enum 신규 생성. CropData(이미완료)/FertilizerData/ToolData/ProcessingRecipeData IInventoryItem 구현. DataRegistry GetInventoryItem 추가. SlotUI프리팹/ToolbarPanel/InventoryPanel/TooltipPanel 생성. InventoryManager+PlayerInventory 씬배치. T-5(타시스템 연동)는 Economy/UI Phase 이후 처리. T-6(통합테스트) execute_code 비활성으로 스킵. |

---

## Phase D — Feature Systems

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `tool-upgrade-tasks.md` (ARC-015) | ✅ 완료 | T-1~T-4 완료. ToolSpecialEffect/LevelReqType/UpgradeMaterial 신규 enum/struct. ToolData 확장(energyCost/cooldown/useSpeed/nextTier/specialEffect 등). EconomyManager stub 생성(economy-tasks.md 완전구현 예정). ToolData SO 9종+재료 SO 2종 생성(Resources/Data/). ToolUpgradeSystem+EconomyManager 씬배치. BlacksmithPanel 프리팹 생성. T-5(통합테스트) execute_code 비활성으로 스킵. |
| `npc-shop-tasks.md` (ARC-009) | ✅ 완료 | T-1~T-5 완료. 스크립트 16종(NPCType/NPCActivityState/DayFlag/DialogueChoiceAction/DialogueChoice/DialogueNode/DialogueData/NPCData/TravelingShopPoolData/NPCSaveData/NPCEvents/NPCManager/NPCController/DialogueSystem/TravelingMerchantScheduler/DialogueUI). NPCData SO 4종+TravelingPool SO 1종+DialogueData SO 6종 생성. DialoguePanel 프리팹 생성(PFB_UI_DialoguePanel). NPC GO 3종+매니저 GO 3종 씬배치. DialogueUI→DialogueSystem 참조 연결. ShopSystem 확장(T-5-01)은 economy-tasks.md 이후 처리. T-6(통합테스트) execute_code 비활성으로 스킵. |
| `blacksmith-tasks.md` (ARC-020) | ✅ 완료 | T-1~T-5 완료. 스크립트 10종(ScreenBase stub 포함)+BlacksmithNPC/AffinityTracker/Events/BlacksmithNPCData/AffinitySaveData. SO 11종(CreateBlacksmithAssets Editor 스크립트). NPC_Blacksmith에 BlacksmithNPC+InteractionZone 배치. ToolUpgradeScreen UI 계층+SerializeField 연결. NPCAffinityTracker 씬배치. T-6(통합테스트) execute_code 비활성으로 스킵. |
| `processing-tasks.md` (ARC-014) | ✅ 완료 | T-1~T-5 완료. ProcessingRecipeData 필드 확장(inputItemId/Qty/outputQty/fuelCost/requiredTier), ProcessingSaveData 신규, ProcessingSystem 확장(Cancel/GetAvailableRecipes/SaveData), BuildingManager OnHourChanged 훅(ProcessTimeAdvance), BuildingEvents OnProcessingCancelled 추가. Editor 스크립트로 Building SO 4종+Recipe SO 32종(잼7/주스3/절임8/제분4/발효5/베이커리5) 생성. ProcessingUI/RecipeSlotUI/ProcessingSlotUI 스크립트+프리팹. Canvas_Overlay/ProcessingPanel 씬배치. T-6(통합테스트) execute_code 비활성으로 스킵. |
| `quest-tasks.md` (ARC-016) | ⬜ 미시작 | |
| `achievement-tasks.md` (ARC-017-MCP) | ⬜ 미시작 | |
| `tutorial-tasks.md` (ARC-010) | ⬜ 미시작 | |

---

## Phase E — UI & UX

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `ui-tasks.md` (ARC-022) | ⬜ 미시작 | |
| `sound-tasks.md` (ARC-027) | ⬜ 미시작 | |

---

## Phase F — Advanced Features

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `farm-expansion-tasks.md` (ARC-025) | ⬜ 미시작 | |
| `livestock-tasks.md` (ARC-024) | ⬜ 미시작 | |
| `fishing-tasks.md` (ARC-028) | ⬜ 미시작 | |
| `gathering-tasks.md` (ARC-032) | ⬜ 미시작 | |

---

## Phase G — Polish

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `collection-tasks.md` (ARC-041) | ⬜ 미시작 | |
| `decoration-tasks.md` (ARC-046) | ⬜ 미시작 | |

---

## 실전 메모 (세션 중 발견사항)

- crop-content-tasks.md: CropData.cs가 이미 단순 버전으로 존재 → GameDataSO+IInventoryItem 상속으로 전면 업데이트. Season [Flags] enum을 SeasonFlag.cs로 분리. Editor 스크립트(CreateCropPrefabs.cs)로 8종×4단계=32 프리팹 + 2 Giant프리팹 + 8 머티리얼 일괄 생성. SO 배열 참조 자동 설정.
- facilities-tasks.md: SO 배열 참조 set_property 불가 → Resources.LoadAll<BuildingData>("Data/Buildings") 자동 로드로 우회. SO 파일 위치를 Data/Buildings → Resources/Data/Buildings로 이동(1회성 에디터 스크립트). GrowthSystem에 SetSeasonOverrideProvider() 주입 패턴으로 온실-계절 연동. BuildingManager에 Singleton 없이 FindObjectOfType 패턴 사용.
- inventory-tasks.md: CropQuality(SeedMind.Economy)/HarvestOrigin(SeedMind) enum이 코드에 없어 신규 생성 필요했음. IInventoryItem/ItemType은 이미 존재(Scripts/Player/). FertilizerData/ToolData는 ScriptableObject 직접 상속→GameDataSO 상속으로 업그레이드. create_script validator 오탐(3파라미터 메서드 중복 감지)→Write 툴로 우회. 크롭/도구 SO를 Data/→Resources/Data/로 이동(MoveInventoryAssetsToResources 에디터 스크립트). UI 생성은 CreateInventoryUI 에디터 스크립트로 일괄 처리.
- tool-upgrade-tasks.md: EconomyManager가 미구현이었으므로 stub 생성(Scripts/Economy/). TimeManager.OnDayChanged는 static event 없이 RegisterOnDayChanged(priority, callback) 패턴 — ToolUpgradeSystem에서 priority=50으로 등록. create_from_gameobject는 비활성 GameObject에 search_inactive=true 파라미터 필요. ToolData 기존 필드 위에 신규 필드(energyCost/cooldown/specialEffect/nextTier 등) 추가 확장.
- farming-tasks.md: 타일 레이어(FarmTile, index 8), 작물 SO, 도구 SO, 프리팹 12개 등이 이전 세션에 이미 완성된 상태였음. GrowthSystem.farmGrid 참조(null→FarmGrid)만 2026-04-10 세션에서 보완.
- blacksmith-tasks.md: ScreenBase가 ui-tasks.md(Phase E) 미구현 상태였으므로 stub(MonoBehaviour 상속 추상 클래스)을 먼저 생성. create_script validator 오탐으로 Write 툴 우회. DialogueSystem.StartDialogue()가 (DialogueData, NPCController) 시그니처임 — npc 파라미터 전달 필요. DialogueChoiceAction.Exit 없음 → CloseDialogue로 교체. ToolData에 tierStats 배열 없음 — SO 체인 방식(nextTier)으로 직접 필드 참조. SO 배열 참조 설정은 CreateBlacksmithAssets Editor 스크립트로 일괄 처리.
- processing-tasks.md: ProcessingRecipeData에 inputItemId/inputQuantity/outputQuantity/fuelCost/requiredFacilityTier 필드 추가 필요했음(기존 스크립트 미포함). Recipe SO는 Resources/Data/Recipes/ 하위에 생성 — DataRegistry가 Resources.LoadAll<GameDataSO>("Data")로 자동 스캔하므로 P-6 별도 수정 불필요. create_script validator 오탐(Make 메서드 10파라미터 중복 감지) → Write 툴로 우회. CropCategory에 None 없음 — Vegetable(=0) 또는 Fruit로 대체. ProcessingUI 프리팹 SerializeField 연결은 CreateProcessingUIPrefabs Editor 스크립트로 일괄 처리.
- scene-setup-tasks.md: Canvas_Overlay는 비활성(SetActive=false) 상태이므로 find_gameobjects에서 검색 안 됨 — include_inactive=true 필요.
- Build Settings: SCN_Loading(0), SCN_MainMenu(1), SCN_Farm(2) 이미 등록 완료.

---

## 실행 순서 참조

`docs/mcp/build-order.md` 섹션 3 참조.
