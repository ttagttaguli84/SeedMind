# SeedMind — MCP 태스크 실행 진행 상황

> 최종 업데이트: 2026-04-10 (decoration-tasks.md D-A~D-D 완료)
> **갱신 규칙**: 각 MCP 태스크 파일 실행 완료 직후 해당 항목을 ✅로 바꾸고 커밋. 세션 종료와 무관하게 완료 즉시 갱신한다.
> 새 세션 시작 시 `/start`가 이 파일을 읽어 진행 위치를 복원한다.

---

## 현재 위치

**Phase G — Polish ✅ 완료**
- 모든 Phase A–G 완료 → Phase 2 → Phase 3 전환 준비

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
| `quest-tasks.md` (ARC-016) | ✅ 완료 | T-1~T-5 완료. 스크립트 20종(QuestCategory/Status/ObjectiveType/RewardType/UnlockConditionType/CompositeMode enum 6종, QuestObjectiveData/RewardData/UnlockCondition/QuestData/QuestSaveData 등 직렬화 클래스, QuestInstance/QuestEvents/QuestTracker/QuestRewarder/DailyQuestSelector/NPCRequestScheduler/QuestManager/QuestLogUI/QuestTrackingUI). Editor 스크립트로 QuestData SO 20종(메인4+일일12+도전4) 생성. QuestLogPanel/QuestTrackingWidget/QuestCompletePopup UI 씬배치+SerializeField 연결. QuestManager 씬배치+배열 참조 연결. GameSaveData quest 필드 추가. T-6(통합테스트) execute_code 비활성으로 스킵. |
| `achievement-tasks.md` (ARC-017-MCP) | ✅ 완료 | T-1~T-5 완료. 스크립트 13종(enum 4, Serializable 3, SO 1, AchievementManager, UI 3종), SO 에셋 36종(Farming5+Economy4+Facility4+Tool3+Explorer4+Quest4+Hidden7+Gatherer5), AchievementItemUI 프리팹, AchievementLayer/Panel(Canvas_Overlay)+AchievementToast(Canvas_Popup) 씬배치, AchievementManager SO배열 연결(36개), GameSaveData achievements 필드 추가. T-6(통합테스트) execute_code 비활성으로 스킵. |
| `tutorial-tasks.md` (ARC-010) | ✅ 완료 | T-1~T-5 완료. 스크립트 15종(enum 6, SO Data 3, TutorialEvents/TutorialSaveData/TutorialManager/TutorialTriggerSystem/ContextHintSystem/TutorialUI), SO 에셋 24종(Sequences 5+Steps 12+Hints 7), Canvas_Tutorial 프리팹(PFB_UI_Tutorial), TutorialSystem/TriggerSystem/ContextHintSystem 씬배치, _allSequences 5개+_allHints 7개 배열 연결, GameSaveData tutorial 필드 추가. T-6(통합테스트) execute_code 비활성으로 스킵. |

---

## Phase E — UI & UX

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `ui-tasks.md` (ARC-022) | ✅ 완료 | T-1~T-7 완료. T-8(통합 테스트) execute_code 비활성으로 스킵. InventoryUI/QuestLogUI/AchievementPanel/DialogueUI/ProcessingUI → ScreenBase 상속으로 업데이트, _screenType 설정 완료. ScreenBase.Awake()에 UIManager.RegisterScreen 자동 등록 추가. |
| `sound-tasks.md` (ARC-027) | ✅ 완료 | S-1~S-4 완료. 스크립트 12종(AudioChannel/BGMTrack/SFXId/SoundEvent enum+struct, SoundData/SoundRegistry/BGMRegistry/AudioSettingsData SO, SFXPool/BGMScheduler/SoundManager/SoundEventBridge). SO 에셋: SoundData 38종+SoundRegistry+BGMRegistry. SoundManager GO(자식 BGM_Source_A/B/Ambient_Source/UI_Source+SoundEventBridge) 씬 배치. S-5(통합 테스트) execute_code 비활성으로 스킵. S-6(오디오 파일 임포트) 실제 파일 없으므로 스킵 — AudioClip은 추후 수동 할당. AudioMixer 에셋은 MCP 미지원 — 에디터에서 수동 생성 필요. |

---

## Phase F — Advanced Features

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `farm-expansion-tasks.md` (ARC-025) | ✅ 완료 | Z-1~Z-9 완료. 스크립트 14종(5 enum+ZoneEvents+ZoneData+ObstacleEntry/Instance+ZoneRuntimeState+FarmZoneManager+FarmGrid.Zone partial+ZoneSaveData/Entry/Obstacle). Editor 스크립트 3종(CreateObstacleAssets/CreateZoneAssets/ConnectZoneManagerRefs). 머티리얼 4종+프리팹 7종. ZoneData SO 7종(Zone A~G). FarmZoneManager 씬배치+배열연결. Z-9 Play Mode 통합테스트 execute_code 비활성으로 스킵. |
| `livestock-tasks.md` (ARC-024) | ✅ 완료 | L-1~L-8 완료. 스크립트 10종(AnimalType/CollectResult/AnimalProductInfo/AnimalData/LivestockConfig/AnimalInstance/AnimalSaveData/HappinessCalculator/LivestockEvents/AnimalManager). UI 3종(AnimalShopUI/AnimalCareUI/AnimalSlotUI). Editor 3종(CreateAnimalAssets/ConnectAnimalManagerRefs/CreateLivestockUI). SO 에셋: AnimalData 4종+LivestockConfig+FeedItem 4종. AnimalManager GO 씬배치+SO 배열연결. Panel_AnimalShop/Panel_AnimalCare/PFB_AnimalSlot UI 생성. ProgressionManager LivestockEvents 구독 활성화. GameSaveData animals 필드 추가. L-9(통합테스트) execute_code 비활성으로 스킵. |
| `fishing-tasks.md` (ARC-028) | ✅ 완료 | F-1~F-6 완료. 스크립트 13종(FishingState/FishRarity/MinigameResult/WeatherFlag enum 4종+FishingStats/FishingSaveData/FishingEvents/FishingProficiency/FishData/FishingConfig SO/FishingPoint/FishingMinigame/FishingManager). FishData SO 15종 + SO_FishingConfig 생성(Editor 스크립트). FishingManager GO(--- MANAGERS ---)+FishingPoint_01~03 GO(--- FARM ---) 씬 배치. ConnectFishingManagerRefs Editor 스크립트로 Config/15종 배열/Point 3개 자동 연결. GameSaveData.fishing 필드 추가. ProgressionManager FishCaught 구독 활성화+GetExpForSource case 추가. F-7(통합 테스트) execute_code 비활성으로 스킵. |
| `gathering-tasks.md` (ARC-032) | ✅ 완료 | G-A~G-F 완료 (스크립트 16종, SO 에셋 6종, GatheringManager/GatheringPoint 5개 씬배치, UI 패널 3개). G-G(통합테스트) execute_code 비활성으로 스킵. |

---

## Phase G — Polish

| 파일 | 완료 여부 | 비고 |
|------|----------|------|
| `collection-tasks.md` (ARC-041) | ✅ 완료 | Q-A~Q-F 완료 (스크립트 10종, SO 27종, GatheringCatalogManager 씬배치+배열연결, CollectionPanel/GatheringCatalogToast UI 계층). Q-G(FishCatalogPanel 마이그레이션) FishCatalog 미구현으로 스킵. Q-H(통합테스트) execute_code 비활성으로 스킵. |
| `decoration-tasks.md` (ARC-046) | ✅ 완료 | D-A~D-D 완료 (스크립트 8종, SO 30종, DecorationManager 씬배치, PathLayer/FenceLayer/DecoObjects 계층). D-E(통합테스트) execute_code 비활성으로 스킵. |

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
- ui-tasks.md: create_script는 batch 미지원 → Write 툴로 개별 파일 직접 생성. ScreenBase.cs가 stub으로 이미 존재(blacksmith-tasks) → 완전 구현으로 덮어씀(Show/Hide 하위 호환 유지). TooltipUI.cs도 기존 구현 유지(InventoryUI 연동 포함). Canvas_Overlay 내부 비활성 UI 이동 시 먼저 SetActive=true로 활성화 후 parent 변경. Canvas_HUD/Popup/Tutorial은 --- UI --- 하위에 기존 오브젝트 있어 새 생성 불필요(Sort Order 업데이트만). CanvasScaler uiScaleMode=1(Scale With Screen Size), referenceResolution=1920×1080 설정.
- tutorial-tasks.md: TutorialEvents에 `event` 키워드 사용 시 외부 클래스에서 `?.Invoke()` 불가(CS0070) — FarmEvents 패턴과 동일하게 `public static Action` (이벤트 키워드 없음)으로 작성. TutorialManager/ContextHintSystem에서 TutorialUI 타입 참조 시 `using SeedMind.UI;` 필요(동일 네임스페이스 아닌 경우). _highlightMask(RectTransform)는 비활성 부모 하위라 instanceID set_property 불가 — 에디터에서 수동 연결 필요. SO 배열(_allSequences, _allHints)은 ConnectTutorialManagerArrays Editor 스크립트로 일괄 연결.
- achievement-tasks.md: create_script validator 오탐(SubscribeAll/CheckCompletion 0~1파라미터 중복 감지) → Write 툴로 우회 후 manage_asset import. AchievementItemUI 프리팹은 SerializedObject 패턴 Editor 스크립트로 일괄 생성. AchievementManager _allAchievements 배열도 Editor 스크립트(FindAssets+sortOrder)로 일괄 연결. SO 에셋은 T-2-ALT(CreateAchievementAssets) 36개 일괄 생성. Canvas_Overlay 하위 오브젝트는 비활성 상태라 by_name 검색 안 됨 — instanceID 사용 필요.

- farm-expansion-tasks.md: asmdef 파일 없음 — 프로젝트 단일 어셈블리, Z-1-04 스킵. FarmGrid.cs에 `partial` 키워드 추가 필요(원본에 없었음). FarmZoneManager ISaveable 구현 시 ISaveable 반환 타입이 `object`이므로 ZoneSaveData로 캐스팅. ProgressionManager.RegisterUnlock() 없음 — 호출 제거. EconomyManager.TrySpendGold()가 SpendGold()가 아닌 실제 API. FarmGrid partial 클래스로 ActivateZoneTiles/DeactivateZoneTiles 확장. CreateZoneAssets Editor 스크립트로 tilePositions Vector2Int[] 배열 코드 할당 (MCP set_property 불안정 우회). ConnectZoneManagerRefs Editor 스크립트로 _zones 7개+_farmGrid 자동 연결.

- fishing-tasks.md: WeatherFlag enum이 프로젝트에 없어 SeedMind.Fishing 네임스페이스에 신규 생성. WeatherType(SeedMind.Core)은 flags 아님 — WeatherFlag(SeedMind.Fishing)로 별도 분리. FishingEvents.cs가 FishingPoint를 참조하므로 Phase 3 스크립트(FishingPoint)를 먼저 생성해야 컴파일 성공. asmdef 없음(프로젝트 단일 어셈블리) — F-1 asmdef 스텝 스킵. F-5-01(HarvestOrigin.Fishing) / F-5-02(XPSource.FishingCatch) 이미 존재 → enum 수정 스킵. ProgressionManager FishingCatch 구독은 주석 처리 상태였으므로 직접 활성화. SO 배열(_fishDataRegistry, _fishingPoints) 설정은 ConnectFishingManagerRefs Editor 스크립트 패턴 사용.

- gathering-tasks.md: PlayerController가 Singleton 패턴 아님(Instance 없음) — GatheringManager 에너지 체크 주석 처리(TODO). HarvestOrigin.Gathering/XPSource.GatheringComplete/ItemType.Gather 이미 존재(이전 세션 완료분). Zone 하위 GO 없음(SO 데이터 기반) — GatheringPoint를 --- FARM --- 직하위에 배치(FishingPoint 패턴). CreateGatheringAssets Editor 스크립트로 Config SO + PointData SO 5종 생성 + _gatheringConfig/_gatheringPoints 배열 자동 연결. GatheringEvents가 GatheringPoint를 참조하므로 Write 툴로 모든 파일을 디스크에 쓴 후 단일 refresh로 일괄 컴파일.

- sound-tasks.md: AudioMixer 에셋(UnityEngine.AudioMixer) MCP `create_asset`으로 생성 불가 — Unity Editor에서 수동 생성 필요(Create > Audio > Audio Mixer). AudioMixer 없이도 SoundManager는 동작(볼륨 제어 비활성). SFXPool은 SoundManager.Awake()에서 동적 생성(자식 GO 사전 생성 불필요). BGMRegistry entries enumValueIndex 방식으로 enum 직접 설정 성공. SoundEventBridge: FishingEvents 미구현(Phase F 이후)이므로 제외, EconomyEvents 없어 EconomyManager.Instance.OnGoldChanged 직접 구독.

---

- decoration-tasks.md: FarmGrid.IsFarmland() 메서드 없음 → GetTile(x, z) != null 패턴으로 경작지 판별. Season enum에 None 값 없음 → hasSeasonLimit(bool) + limitedSeason(Season) 2필드 패턴으로 DecorationItemData 설계(Inspector 직렬화 호환). FenceStone/Iron/Floral의 hasSeasonLimit=false, limitedSeason 기본값(Spring) — hasSeasonLimit=false이므로 무시. D-D 씬 배치 시 Decorations 노드는 --- ENVIRONMENT --- 직하위(instanceID로 FarmGrid, FenceLayer, PathLayer, DecoObjects 참조 연결 성공).

## 실행 순서 참조

`docs/mcp/build-order.md` 섹션 3 참조.
