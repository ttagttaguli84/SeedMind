# MCP 기본 씬 구성 태스크 시퀀스

> 기본 씬(SCN_Farm, SCN_MainMenu, SCN_Loading, SCN_Test_FarmGrid)을 MCP for Unity를 통해 구성하는 단계별 태스크 시퀀스  
> 작성: Claude Code (Opus) | 2026-04-06

---

## Context

이 문서는 SeedMind 프로젝트의 기본 씬 구성을 MCP for Unity를 통해 자동화하기 위한 전체 계획을 정의한다. 게임 디자인 관점에서 "무엇이 왜 있어야 하는가"를 정의하고, 기술 아키텍처 관점에서 "어떻게 MCP 명령으로 구현하는가"를 설계한다.

`docs/systems/project-structure.md` 섹션 5에 명시된 씬 계층 구조와 섹션 6의 네이밍 규칙을 준수하며, `docs/architecture.md`의 렌더링/라이팅 전략을 구현 수준으로 번역한다.

**목표**: Unity Editor를 열지 않고(또는 최소한의 수동 개입으로) MCP 명령만으로 전체 기본 씬 골격을 완성한다.

---

# Part I: 게임 디자인 — 씬 구성 요소 및 초기 경험

---

## 1. 기본 씬 구성 요소 목록

SCN_Farm에 배치되어야 할 게임 오브젝트를 계층별로 정의한다. 씬 계층 구조는 `docs/systems/project-structure.md` 섹션 5.4를 따른다.

### 1.1 MANAGERS (시스템 오브젝트)

플레이어에게 보이지 않지만, 게임 루프의 기반이 되는 매니저 오브젝트들.

| GameObject | 역할 | 게임 디자인 근거 |
|------------|------|------------------|
| `GameManager` | 전체 게임 상태 관리, DontDestroyOnLoad | 씬 전환 시에도 게임 상태 유지 필요 |
| `TimeManager` | 시간/계절/날씨 흐름 제어 | 06:00~24:00 하루 흐름, 28일/계절 순환이 모든 시스템의 기반 (-> see `docs/systems/time-season.md` 섹션 1) |
| `SaveManager` | 저장/불러오기 | DontDestroyOnLoad. 씬 진입 시 저장 데이터 적용 |
### 1.2 FARM (농장 핵심 오브젝트)

핵심 게임 루프의 물리적 기반.

| GameObject | 하위 구조 | 역할 | 게임 디자인 근거 |
|------------|-----------|------|------------------|
| `FarmSystem` | 최상위 농장 컨테이너 | 경작 관련 오브젝트 그룹핑 | -- |
| `FarmGrid` | `FarmSystem` 하위 | 8x8 타일 그리드 관리 | 초기 크기 8x8, 타일 크기 1m x 1m (-> see `docs/systems/farming-system.md` 섹션 1) |
| `Tile_{x}_{y}` | `FarmGrid` 하위, 64개 | 개별 타일. 초기 상태 `Empty` | 네이밍: `Tile_0_0` ~ `Tile_7_7`. 상태 머신: Empty -> Tilled -> Planted -> Watered -> Harvestable (-> see `docs/systems/farming-system.md` 섹션 2) |
| `GrowthSystem` | `FarmSystem` 하위 | 매일 아침 성장 계산 처리 | 06:00 배치 처리에서 Watered 타일의 성장일수 누적 (-> see `docs/systems/time-season.md` 섹션 1.6) |
| `Buildings` | `--- FARM ---` 하위 (FarmSystem과 형제) | 건물 동적 배치 컨테이너 | 초기에는 비어 있음. 물탱크(레벨3)/온실(레벨5) 등 해금 후 배치 (-> see `docs/design.md` 섹션 4.6, `docs/systems/project-structure.md` 섹션 5.4) |

### 1.3 PLAYER (플레이어)

| GameObject | 하위 구조 | 역할 | 게임 디자인 근거 |
|------------|-----------|------|------------------|
| `Player` | 최상위 플레이어 | 플레이어 캐릭터 루트 | 초기 위치: 농장 그리드 중앙 부근(약 Tile_3_3 위치) |
| `PlayerModel` | `Player` 하위 | 캐릭터 비주얼 메시 | 로우폴리 스타일. 쿼터뷰에서 식별 가능한 크기(타일 1m 대비 약 0.6~0.8m 높이) |
| `PlayerController` | `Player` 컴포넌트 | WASD 이동, 입력 처리 | (-> see `docs/design.md` 섹션 5, `docs/architecture.md` 섹션 4.4) |
| `ToolSystem` | `Player` 컴포넌트 | 도구 선택/사용 | 초기 도구: 호미, 물뿌리개, 씨앗(감자/당근), 낫, 손. 1~5 숫자키로 선택 |

### 1.4 ENVIRONMENT (환경)

씬의 시각적 배경과 분위기를 형성하는 오브젝트들.

| GameObject | 하위 구조 | 역할 | 게임 디자인 근거 |
|------------|-----------|------|------------------|
| `Terrain` | 바닥 지형 | 농장 영역 + 주변 환경 바닥 | 농장 그리드(8x8m) 영역을 포함하는 넓은 바닥. 잔디/풀 텍스처. 계절별 색상 변화 (-> see `docs/systems/time-season.md` 섹션 2.3) |
| `Lighting` | 조명 그룹 | -- | -- |
| `Sun` | `Lighting` 하위, Directional Light | 메인 조명원 | 시간대별 색온도/각도 변화: Dawn(주황-분홍) -> Morning(황금빛) -> Afternoon(백색) -> Evening(주황-빨강) -> Night(파란) (-> see `docs/systems/time-season.md` 섹션 1.2) |
| `AmbientProbe` | `Lighting` 하위 | 간접광 보조 | 계절별 Ambient Color 변화: 봄(#E8F5E9), 여름(#E3F2FD), 가을(#FFF3E0), 겨울(#E8EAF6) (-> see `docs/systems/time-season.md` 섹션 2.3) |
| `Decorations` | 환경 장식 그룹 | -- | -- |
| `Fences` | `Decorations` 하위 | 농장 경계 울타리 | 농장 그리드 외곽을 둘러싸는 울타리. 플레이 영역의 시각적 경계 표시 |
| `Trees` | `Decorations` 하위 | 배경 나무 3~5그루 | 농장 가장자리에 배치. 계절별 외형 변화 |

### 1.5 ECONOMY (경제)

| GameObject | 역할 | 게임 디자인 근거 |
|------------|------|------------------|
| `EconomyManager` | 경제 시스템 매니저 | 골드 관리, 가격 조회, 거래 처리. 초기 골드 500G 지급 (-> see `docs/systems/economy-system.md` 섹션 1.2) |
| `Shop` | 상점 상호작용 오브젝트 | 농장 근처에 배치. E키로 상호작용 시 ShopPanel UI 오픈. 운영 시간 08:00~18:00, 수요일 휴무 (-> see `docs/systems/time-season.md` 섹션 1.7) |
| `ShippingBin` | 출하함 (작물 판매 투입구) | 24시간 이용 가능. 투입한 작물은 다음 날 06:00 정산 (-> see `docs/systems/economy-system.md` 섹션 4.1) |

[OPEN] 상점/출하함의 정확한 배치 위치(농장 그리드 기준 오프셋)가 미정. 플레이 동선을 고려하여, 그리드 왼쪽 또는 아래쪽에 배치하는 것을 제안. 테스트 후 조정.

### 1.6 CAMERA

| GameObject | 역할 | 게임 디자인 근거 |
|------------|------|------------------|
| `Main Camera` | Orthographic, 쿼터뷰(45도 각도) | 고정 쿼터뷰, 줌 인/아웃 가능, 플레이어 중심 추적 (-> see `docs/design.md` 섹션 7) |

### 1.7 UI

| GameObject | 하위 구조 | 역할 | 게임 디자인 근거 |
|------------|-----------|------|------------------|
| `Canvas_HUD` | Screen Space - Overlay, 항상 표시 | 메인 HUD | -- |
| `TimeDisplay` | `Canvas_HUD` 하위 | 시간/날짜/계절/날씨 표시 | 화면 좌상단. "Day 1 / 봄 / 08:00 / 맑음" 형태 |
| `GoldDisplay` | `Canvas_HUD` 하위 | 소지 골드 표시 | 화면 우상단. "500G" (3자리 콤마 구분, -> see `docs/systems/economy-system.md` 섹션 1.1) |
| `ToolBar` | `Canvas_HUD` 하위 | 도구 슬롯 5칸 | 화면 하단 중앙. 1~5번 슬롯, 선택된 도구 하이라이트 |
| `LevelBar` | `Canvas_HUD` 하위 | 레벨 + 경험치 바 | 화면 하단 좌측. "Lv.1" + 경험치 게이지 |
| `Canvas_Overlay` | Screen Space - Overlay, 기본 비활성 | 서브 화면들 | -- |
| `InventoryPanel` | `Canvas_Overlay` 하위 | 인벤토리 UI (Tab 토글) | -- |
| `ShopPanel` | `Canvas_Overlay` 하위 | 상점 UI (E키 상호작용) | -- |
| `PausePanel` | `Canvas_Overlay` 하위 | 일시정지 메뉴 (Esc 토글) | 시간 정지 |
| `Canvas_Popup` | Screen Space - Overlay | 팝업 메시지 | 수확 알림, 레벨업 알림, 계절 경고 등 |

---

## 2. 초기 플레이 경험 정의

### 2.1 첫 화면 (0~3초)

1. **페이드 인**: 검은 화면에서 서서히 밝아짐 (약 1.5초)
2. **카메라**: 쿼터뷰(45도)로 농장 전체를 조망하는 위치에서 시작
3. **보이는 것**:
   - 중앙에 8x8 경작 가능 영역 (연한 갈색 토양, 모두 `Empty` 상태)
   - 영역 주변의 울타리
   - 배경에 나무 몇 그루와 잔디 (봄 시즌: 연두색)
   - 따뜻한 황금빛 조명 (봄 Morning)
4. **플레이어 캐릭터**: 농장 중앙 부근에 서 있음
5. **HUD 등장**: 페이드 인 완료 후 HUD 요소가 순차적으로 나타남

### 2.2 첫 HUD 상태

| HUD 요소 | 표시 내용 | 비고 |
|-----------|-----------|------|
| TimeDisplay | "Day 1 / 봄 / 08:00 / 맑음" | 게임은 봄 Day 1 아침에 시작 |
| GoldDisplay | "500G" | 초기 지급 골드 (-> see `docs/systems/economy-system.md` 섹션 1.2) |
| ToolBar | [호미] [물뿌리개] [감자씨앗x5] [낫] [손] | 초기 도구 + 시작 씨앗 |
| LevelBar | "Lv.1" + 빈 경험치 바 | 레벨 1, 경험치 0 |

[OPEN] 초기 지급 씨앗 수량 미정. 제안: 감자 씨앗 5개 + 당근 씨앗 5개. 첫 수확까지 3일(게임 내), 약 30분(실시간)에 핵심 루프 1회전 경험 가능.

### 2.3 첫 플레이 동선 (기대 경험)

```
08:00  농장 도착 -> HUD 확인 (골드 500G, 도구 확인)
       |
       도구바에서 호미 선택 (숫자키 1) -> 빈 땅에 좌클릭 -> 경작지 전환
       (직관적 피드백: 땅 색상 변화 Empty->Tilled, 흙 파는 효과음)
       |
       도구바에서 씨앗 선택 (숫자키 3) -> 경작지에 좌클릭 -> 씨앗 심기
       (피드백: 작은 씨앗 모델 출현, 상태 Planted)
       |
       도구바에서 물뿌리개 선택 (숫자키 2) -> 심은 타일에 좌클릭 -> 물주기
       (피드백: 물 파티클 효과, 타일 색상 진해짐 Planted->Watered)
       |
       남은 시간: 상점 방문(E키), 추가 씨앗 구매, 더 많은 타일 경작
       |
24:00  하루 종료 -> 다음 날 06:00
       |
       3일 후: 감자/당근 수확 가능 (Harvestable) -> 낫으로 수확 -> 출하함에 판매
       (성취감: 첫 수익 발생!)
```

### 2.4 첫인상 목표

| 감정 | 달성 수단 |
|------|-----------|
| **안도감** | 빈 땅이지만 밝고 따뜻한 색감. 위협 요소 없음 |
| **호기심** | 타일을 클릭하면 변화가 생김. "다른 것도 해볼까?" |
| **성취감** | 씨앗 심고 물 주는 즉각적 시각 피드백. 3일 후 첫 수확 |
| **방향성** | HUD의 골드, 레벨, 도구가 "다음에 무엇을 할지" 암시 |

---

## 3. 시각적 요구사항

### 3.1 카메라 설정

(-> see `docs/design.md` 섹션 7, `docs/architecture.md` 섹션 5)

| 파라미터 | 값 | 비고 |
|----------|-----|------|
| Projection | Orthographic | 로우폴리 스타일에 적합, 원근 왜곡 없음 |
| 각도 (X Rotation) | 45도 | 쿼터뷰(아이소메트릭) |
| Y Rotation | 45도 [OPEN] | 대각선 시점. MCP 테스트 후 확정 (-> see `docs/design.md` Open Questions) |
| Orthographic Size | 6 (초기값) | 8x8 그리드를 화면에 적절히 담는 크기 |
| Zoom 범위 | Size 5~12 | 줌인(개별 타일 상세) ~ 줌아웃(농장 전체 조망) |
| 추적 대상 | Player | 부드러운 추적 (SmoothDamp 또는 Cinemachine) |

[OPEN] 쿼터뷰 vs 탑다운 최종 확정은 MCP로 카메라 설정 후 테스트 필요 (-> see `docs/design.md` Open Questions)

### 3.2 라이팅

(-> see `docs/architecture.md` 섹션 5, `docs/systems/time-season.md` 섹션 1.2, 2.3)

| 요소 | 설정 | 비고 |
|------|------|------|
| Render Pipeline | URP (Universal Render Pipeline) | Forward Rendering |
| Directional Light (Sun) | 시간대별 색온도/강도/각도 변화 | 초기 씬 기본값: 봄 Morning (5500K, 밝은 황금빛) |
| Shadow | Soft Shadow, 단일 Directional Light | MSAA 4x |
| Ambient Light | 계절별 Ambient Color | 봄: #E8F5E9 (연한 연두) |

**초기 씬 기본 라이팅 프리셋 (봄 Morning)**:
- Sun 색온도: 5500K (따뜻한 백색)
- Sun 각도: X=50, Y=-30
- Sun 강도: 1.2
- Sun Color: #FFFAED (-> see `docs/systems/time-season-architecture.md` DayPhase 표, canonical)
- Ambient Color: #E8F5E9
- Shadow: Soft, Resolution 1024

### 3.3 지형 룩앤필

| 영역 | 시각적 표현 | 머티리얼 |
|------|-------------|----------|
| 경작 가능 영역 (8x8) | 연한 갈색 토양. 타일 경계가 미세하게 보임 | `M_Soil_Empty.mat` (-> see `docs/systems/project-structure.md` 섹션 6.2) |
| 경작된 타일 | 어두운 갈색, 고랑 표현 | `M_Soil_Tilled.mat` |
| 물 준 타일 | 젖은 느낌의 짙은 갈색 | `M_Soil_Watered.mat` |
| 주변 잔디 영역 | 연두색 잔디 (봄 기본) | `M_Grass.mat` |
| 울타리 | 밝은 나무색 로우폴리 울타리 | -- |
| 배경 나무 | 로우폴리 나무, 계절별 잎 색상 변화 | -- |

### 3.4 아트 스타일 가이드라인

(-> see `docs/design.md` 섹션 7)

- **로우폴리 3D**: 단순한 기하학적 형태, 적은 폴리곤
- **색감**: 밝고 따뜻한 파스텔 톤. 채도 높지 않음
- **머티리얼**: 단색 또는 2톤. URP/Lit 기본 셰이더
- **레퍼런스 톤**: Islanders, A Short Hike, Townscaper

---

## 4. UI 초기 상태 상세

### 4.1 Canvas_HUD 레이아웃

```
+------------------------------------------------------+
| [TimeDisplay]                        [GoldDisplay]    |
|  Day 1 / 봄 / 08:00 / 맑음              500G         |
|                                                       |
|                                                       |
|                    (게임 화면)                          |
|                                                       |
|                                                       |
| [LevelBar]         [ToolBar]                          |
|  Lv.1 ####....     [1:호미] [2:물뿌리개] [3:씨앗]     |
|  EXP: 0/100        [4:낫] [5:손]                      |
+------------------------------------------------------+
```

### 4.2 각 HUD 요소 초기값

| 요소 | 초기 표시 | 데이터 소스 |
|------|-----------|-------------|
| 날짜 | "Day 1" | `TimeManager.currentDay` = 1 |
| 계절 | "봄" | `TimeManager.currentSeason` = `Spring` |
| 시각 | "08:00" | `TimeManager.currentHour` = 8 (게임 시작 시각) |
| 날씨 | "맑음" | 첫 날은 항상 맑음으로 고정 (튜토리얼 배려) |
| 골드 | "500G" | `EconomyManager.currentGold` = 500 |
| 레벨 | "Lv.1" | `LevelSystem.currentLevel` = 1 |
| 경험치 | 0/100 (게이지 0%) | `LevelSystem.currentXP` = 0 |
| 도구 슬롯 1 | 호미 (선택됨) | 기본 선택 도구 |
| 도구 슬롯 2 | 물뿌리개 | -- |
| 도구 슬롯 3 | 씨앗 (감자) | 첫 번째 씨앗 |
| 도구 슬롯 4 | 낫 | -- |
| 도구 슬롯 5 | 손 (빈손) | 아이템 정보 보기/상호작용용 |

### 4.3 Canvas_Overlay 초기 상태

| 패널 | 초기 상태 | 활성화 조건 |
|------|-----------|-------------|
| `InventoryPanel` | 비활성 (SetActive false) | Tab 키 토글 |
| `ShopPanel` | 비활성 | 상점 오브젝트 E키 상호작용 |
| `PausePanel` | 비활성 | Esc 키 토글 |

모든 Overlay 패널 활성화 시 `TimeManager` 시간 정지. (-> see `docs/systems/time-season.md` 섹션 1.4)

---

## 5. 테스트 씬 (SCN_Test_FarmGrid) 요구사항

농장 그리드 시스템을 독립적으로 검증하기 위한 최소 구성 씬. 빌드에 포함하지 않는다.

### 5.1 목적

- FarmGrid/FarmTile의 상태 전환 검증 (Empty -> Tilled -> Planted -> Watered -> Harvestable -> 수확)
- 도구 인터랙션 검증 (호미, 물뿌리개, 씨앗, 낫)
- 성장 시스템 단독 테스트 (시간 경과 시뮬레이션)
- 타일 시각적 피드백 확인 (머티리얼 변화)

### 5.2 최소 구성 요소

| GameObject | 역할 | SCN_Farm 대비 차이 |
|------------|------|-------------------|
| `TestTimeManager` | 시간 흐름 제어 (가속 버튼 포함) | DontDestroyOnLoad 불필요. 테스트용 시간 가속 UI 추가 |
| `FarmGrid` | 4x4 타일 그리드 (축소 버전) | 8x8 대신 4x4 (16타일)로 빠른 검증 |
| `Tile_{x}_{y}` | 개별 타일 (16개) | `Tile_0_0` ~ `Tile_3_3` |
| `GrowthSystem` | 성장 계산 | SCN_Farm과 동일 로직 |
| `TestPlayer` | 간소화된 플레이어 | 이동 + 도구 사용만 지원. 인벤토리/레벨 시스템 없음 |
| `Main Camera` | Orthographic, 고정 위치 | 추적 없음. 4x4 그리드 전체가 보이는 고정 카메라 |
| `Canvas_TestHUD` | 테스트 정보 표시 | 현재 선택 타일 정보, 상태, 성장일수, 시간 가속 버튼 |
| `Directional Light` | 기본 조명 | 시간대 변화 없음. 고정 밝은 조명 |

### 5.3 테스트 전용 UI (Canvas_TestHUD)

```
+---------------------------------------------------+
| [시간: Day 1 / 08:00]  [x1] [x10] [x100] [다음날] |
|                                                    |
|           (4x4 FarmGrid)                           |
|                                                    |
| [선택 타일: Tile_2_1]                               |
| [상태: Watered]                                     |
| [작물: 감자]                                        |
| [성장일수: 1/3]                                     |
| [토양 품질: 50]                                     |
|                                                    |
| [도구: 호미 / 물뿌리개 / 감자씨앗 / 낫]              |
+---------------------------------------------------+
```

| 테스트 기능 | 설명 |
|------------|------|
| 시간 가속 (x1/x10/x100) | 성장 과정을 빠르게 관찰 |
| "다음날" 버튼 | 즉시 하루 배치 처리 실행 (06:00 로직) |
| 타일 정보 패널 | 클릭한 타일의 모든 내부 상태 실시간 표시 |
| 상태 강제 전환 | 디버그용: 드롭다운으로 타일 상태를 강제 변경 |

### 5.4 검증 시나리오

| 번호 | 시나리오 | 검증 항목 |
|------|----------|-----------|
| T-01 | Empty 타일에 호미 사용 | Empty -> Tilled 전환, 머티리얼 변화 |
| T-02 | Tilled 타일에 씨앗 사용 | Tilled -> Planted 전환, 씨앗 모델 출현 |
| T-03 | Planted 타일에 물뿌리개 사용 | Planted -> Watered 전환, 타일 색상 변화 |
| T-04 | "다음날" 3회 반복 (감자 기준) | 성장일수 0 -> 1 -> 2 -> 3, Watered -> Harvestable |
| T-05 | Harvestable 타일에 낫 사용 | 수확 처리, 타일 -> Tilled 복귀, 작물 아이템 획득 |
| T-06 | Planted 상태에서 물 안 주고 3일 경과 | Dry 3일 연속 -> Withered 전환 |
| T-07 | Tilled 상태에서 씨앗 안 심고 3일 경과 | Tilled -> Empty 복귀 |
| T-08 | Withered 타일에 낫 사용 | Withered -> Tilled 전환 (고사 작물 제거) |

---

# Part II: 기술 아키텍처 — MCP 태스크 시퀀스

---

## 6. 사전 조건 (Prerequisites)

MCP 태스크 실행 전 반드시 확보되어야 하는 Unity 프로젝트 상태:

| # | 조건 | 확인 방법 |
|---|------|----------|
| P-1 | Unity 6 프로젝트가 열려 있고 MCP for Unity 패키지가 설치/활성 상태 | MCP 연결 테스트 (`get_project_info` 호출) |
| P-2 | URP(Universal Render Pipeline) 패키지가 Package Manager를 통해 설치됨 | `com.unity.render-pipelines.universal` 패키지 존재 확인 |
| P-3 | Input System 패키지가 설치됨 (`com.unity.inputsystem`) | Active Input Handling = "Input System Package (New)" 또는 "Both" |
| P-4 | MCP for Unity가 핵심 도구를 지원 | MCP 도구 목록 조회로 사전 검증 |
| P-5 | 프로젝트에 기존 씬이 없거나, 빈 기본 씬만 존재 | 충돌 방지 |

[RESOLVED] URP Pipeline Asset 사전 생성: `manage_graphics`로 Pipeline Asset 생성/할당 가능 (v9.6.5 검증 완료).

[RESOLVED] MCP 도구 가용성: v9.6.5 기준 42개 툴 검증 완료. 본 문서의 MCP 호출 계획은 실행 가능.

---

## 7. MCP 도구 매핑

| MCP 도구 | 역할 | 주요 파라미터 |
|----------|------|--------------|
| `create_object` | 빈 GameObject 생성 | `name`, `parent` (선택), `primitiveType` (선택) |
| `create_primitive` | Primitive 형상 GameObject 생성 | `type` (Cube, Quad 등), `name`, `parent` |
| `set_property` | GameObject/Component 프로퍼티 설정 | `objectName`, `componentType`, `propertyName`, `value` |
| `add_component` | GameObject에 컴포넌트 추가 | `objectName`, `componentType` |
| `remove_component` | 불필요한 기본 컴포넌트 제거 | `objectName`, `componentType` |
| `create_material` | 새 Material 에셋 생성 | `name`, `shader`, `color` (선택) |
| `set_material` | Renderer에 Material 할당 | `objectName`, `materialName` |
| `create_scene` | 새 씬 생성 및 저장 | `name`, `path` |
| `open_scene` | 씬 열기 | `path` |
| `save_scene` | 현재 씬 저장 | (path) |
| `create_folder` | 프로젝트 폴더 생성 | `path` |
| `execute_menu_item` | Unity 메뉴 항목 실행 | `menuPath` |
| `enter_play_mode` | Play Mode 진입 | -- |
| `exit_play_mode` | Play Mode 종료 | -- |
| `get_object_info` | GameObject 정보 조회 | `objectName` |

> 도구명은 MCP for Unity의 일반적 인터페이스 기준. 실제 버전에 따라 다를 수 있으므로 Phase A 시작 전 검증 필수.

---

## 8. Phase별 태스크 시퀀스

### Phase A: 프로젝트 초기 설정

**목표**: 폴더 구조 생성, URP 기본 설정, Input System 기본 에셋 배치

#### A-1. 폴더 구조 생성

`docs/systems/project-structure.md` 섹션 1에 명시된 폴더를 순서대로 생성한다.

```
Step A-1-01 ~ A-1-47:
  Assets/_Project/
  Assets/_Project/Scripts/
  Assets/_Project/Scripts/Core/
  Assets/_Project/Scripts/Farm/
  Assets/_Project/Scripts/Farm/Data/
  Assets/_Project/Scripts/Player/
  Assets/_Project/Scripts/Player/Data/
  Assets/_Project/Scripts/Economy/
  Assets/_Project/Scripts/Economy/Data/
  Assets/_Project/Scripts/Building/
  Assets/_Project/Scripts/Building/Data/
  Assets/_Project/Scripts/Building/Buildings/
  Assets/_Project/Scripts/Level/
  Assets/_Project/Scripts/Level/Data/
  Assets/_Project/Scripts/UI/
  Assets/_Project/Data/
  Assets/_Project/Data/Crops/
  Assets/_Project/Data/Fertilizers/
  Assets/_Project/Data/Tools/
  Assets/_Project/Data/Buildings/
  Assets/_Project/Data/Config/
  Assets/_Project/Prefabs/
  Assets/_Project/Prefabs/Player/
  Assets/_Project/Prefabs/Crops/
  Assets/_Project/Prefabs/Buildings/
  Assets/_Project/Prefabs/Farm/
  Assets/_Project/Prefabs/Environment/
  Assets/_Project/Prefabs/UI/
  Assets/_Project/Materials/
  Assets/_Project/Materials/Terrain/
  Assets/_Project/Materials/Crops/
  Assets/_Project/Materials/Buildings/
  Assets/_Project/Materials/Environment/
  Assets/_Project/Textures/
  Assets/_Project/Textures/UI/
  Assets/_Project/Scenes/
  Assets/_Project/Scenes/Main/
  Assets/_Project/Scenes/Test/
  Assets/_Project/Audio/
  Assets/_Project/Audio/SFX/
  Assets/_Project/Audio/BGM/
  Assets/_Project/Animations/
  Assets/_Project/Animations/Player/
  Assets/_Project/Animations/Crops/
  Assets/_Project/Input/
  Assets/_Project/Resources/
  Assets/_Project/Resources/UI/
```

- **MCP 호출 수**: 47회
- **의존성**: 없음 (부모 폴더부터 순서대로)
- **검증**: 폴더 구조 트리 조회

#### A-2. URP 기본 설정

```
Step A-2-01: execute_menu_item -> "Assets/Create/Rendering/URP Asset (with Universal Renderer)"
             [RESOLVED] `manage_asset`으로 에셋 이름/경로 제어 가능. `execute_menu_item` + `manage_asset` rename 조합도 가능.
Step A-2-02: Graphics Settings에 URP Pipeline Asset 할당
             [RESOLVED] `manage_graphics`로 Graphics Settings 할당 가능.
Step A-2-03: URP Pipeline Asset 설정
             -> MSAA: 4x, Shadow Type: Soft Shadows, Shadow Resolution: 1024
```

- **MCP 호출 수**: 3회 (실패 시 수동 대체)

#### A-3. Input System 기본 에셋

```
Step A-3-01: SeedMindInputActions.inputactions 생성
             -> 저장 경로: Assets/_Project/Input/
             [WORKAROUND] Input Actions 내부 액션 맵은 .inputactions JSON 직접 작성으로 우회.
```

- **MCP 호출 수**: 1회

#### Phase A 검증 체크리스트

- [ ] `Assets/_Project/` 하위 폴더가 `project-structure.md` 섹션 1과 일치
- [ ] URP Pipeline Asset이 Graphics Settings에 할당됨
- [ ] MSAA 4x, Soft Shadow 설정 적용
- [ ] Input System 패키지 활성
- [ ] 콘솔에 에러 없음

---

### Phase B: SCN_Farm 씬 구성

**목표**: 핵심 게임플레이 씬의 계층 구조, 카메라, 라이팅, 매니저 오브젝트 배치

#### B-1. 씬 생성

```
Step B-1-01: create_scene -> path: "Assets/_Project/Scenes/Main/SCN_Farm.unity"
Step B-1-02: open_scene -> path: "Assets/_Project/Scenes/Main/SCN_Farm.unity"
```

#### B-2. 구분선 오브젝트 생성

`project-structure.md` 섹션 5.4의 `--- CATEGORY ---` 패턴.

```
Step B-2-01~07: 빈 GameObject 7개 생성
  "--- MANAGERS ---"
  "--- FARM ---"
  "--- PLAYER ---"
  "--- ENVIRONMENT ---"
  "--- ECONOMY ---"
  "--- CAMERA ---"
  "--- UI ---"
```

#### B-3. MANAGERS 섹션

```
Step B-3-01: create_object -> name: "GameManager", parent: "--- MANAGERS ---"
Step B-3-02: create_object -> name: "TimeManager", parent: "--- MANAGERS ---"
Step B-3-03: create_object -> name: "SaveManager", parent: "--- MANAGERS ---"
```

> 스크립트 컴포넌트는 Phase 2(코드 작성) 이후에 `add_component`로 부착. 현 단계는 빈 오브젝트로 계층 구조 확보.

#### B-4. FARM 섹션

```
Step B-4-01: create_object -> name: "FarmSystem", parent: "--- FARM ---"
Step B-4-02: create_object -> name: "FarmGrid", parent: "FarmSystem"
Step B-4-03: create_object -> name: "GrowthSystem", parent: "FarmSystem"
Step B-4-04: create_object -> name: "Buildings", parent: "--- FARM ---"
```

- FarmGrid 하위 개별 타일(Tile_0_0 ~ Tile_7_7)은 `docs/mcp/farming-tasks.md`(ARC-003)에서 별도 정의.

#### B-5. PLAYER 섹션

```
Step B-5-01: create_object -> name: "Player", parent: "--- PLAYER ---"
Step B-5-02: create_object -> name: "PlayerModel", parent: "Player"
Step B-5-03: create_object -> name: "PlayerController", parent: "Player"
Step B-5-04: create_object -> name: "ToolSystem", parent: "Player"
```

#### B-6. ENVIRONMENT 섹션

```
Step B-6-01: create_object -> name: "Terrain", parent: "--- ENVIRONMENT ---"
Step B-6-02: create_object -> name: "Lighting", parent: "--- ENVIRONMENT ---"
Step B-6-03: create_object -> name: "Decorations", parent: "--- ENVIRONMENT ---"
Step B-6-04: create_object -> name: "Fences", parent: "Decorations"
Step B-6-05: create_object -> name: "Trees", parent: "Decorations"
```

#### B-7. Terrain Placeholder (바닥면)

```
Step B-7-01: create_primitive -> type: "Quad", name: "GroundPlane", parent: "Terrain"
Step B-7-02: set_property -> Transform.localPosition = (4, -0.01, 4)
             (그리드 중심 아래 배치, z-fighting 방지)
Step B-7-03: set_property -> Transform.localRotation = (90, 0, 0)
             (Quad 눕히기)
Step B-7-04: set_property -> Transform.localScale = (20, 20, 1)
Step B-7-05: create_material -> name: "M_Grass", shader: "Universal Render Pipeline/Lit",
             color: #4CAF50
             -> 저장: Assets/_Project/Materials/Terrain/M_Grass.mat
Step B-7-06: set_material -> objectName: "GroundPlane", materialName: "M_Grass"
```

#### B-8. 라이팅 설정

```
Step B-8-01: create_object -> name: "Sun", parent: "Lighting"
Step B-8-02: add_component -> objectName: "Sun", componentType: "Light"
Step B-8-03: set_property -> Light.type = "Directional"
Step B-8-04: set_property -> Transform.localRotation = (50, -30, 0)
Step B-8-05: set_property -> Light.color = #FFFAED (-> see time-season-architecture.md)
Step B-8-06: set_property -> Light.intensity = 1.2
Step B-8-07: set_property -> Light.shadows = "Soft"
Step B-8-08: create_object -> name: "AmbientProbe", parent: "Lighting"
             [RESOLVED] `manage_graphics`로 RenderSettings/ambient 설정 가능.
```

#### B-9. 카메라 설정

```
Step B-9-01: Main Camera를 "--- CAMERA ---" 하위로 이동
             [RESOLVED] `find_gameobjects`로 기본 카메라 존재 여부 즉시 확인 가능. 조건부 실행.
Step B-9-02: set_property -> Transform.localPosition = (4, 10, -4)
Step B-9-03: set_property -> Transform.localRotation = (45, 45, 0)
Step B-9-04: set_property -> Camera.orthographic = true
Step B-9-05: set_property -> Camera.orthographicSize = 6
Step B-9-06: set_property -> Camera.backgroundColor = #87CEEB
```

#### B-10. ECONOMY 섹션

```
Step B-10-01: create_object -> name: "EconomyManager", parent: "--- ECONOMY ---"
Step B-10-02: create_object -> name: "Shop", parent: "--- ECONOMY ---"
Step B-10-03: create_object -> name: "ShippingBin", parent: "--- ECONOMY ---"
```

#### B-11. UI 섹션

```
Step B-11-01~05:  Canvas_HUD 생성 (Canvas + CanvasScaler + GraphicRaycaster)
Step B-11-06~09:  Canvas_HUD 하위: TimeDisplay, GoldDisplay, ToolBar, LevelBar
Step B-11-10~15:  Canvas_Overlay 생성 + 비활성 설정
Step B-11-16~18:  Canvas_Overlay 하위: InventoryPanel, ShopPanel, PausePanel
Step B-11-19~24:  Canvas_Popup 생성 + PopupMessage
```

- **총 UI MCP 호출**: ~24회

#### B-12. 기본 Directional Light 정리

```
씬 생성 시 자동 포함된 "Directional Light"가 있다면 삭제 (Sun과 중복 방지)
```

#### B-13. 씬 저장

```
Step B-13-01: save_scene -> "Assets/_Project/Scenes/Main/SCN_Farm.unity"
```

#### Phase B 검증 체크리스트

- [ ] Hierarchy에 7개 구분선 존재
- [ ] MANAGERS 하위: GameManager, TimeManager, SaveManager
- [ ] FARM 하위: FarmSystem > FarmGrid, GrowthSystem + Buildings
- [ ] PLAYER 하위: Player > PlayerModel, PlayerController, ToolSystem
- [ ] ECONOMY 하위: EconomyManager, Shop, ShippingBin
- [ ] Main Camera: Orthographic, Size=6, Position=(4,10,-4), Rotation=(45,45,0)
- [ ] Sun: Rotation=(50,-30,0), Color=#FFFAED, Soft Shadows
- [ ] GroundPlane: 녹색 바닥면 표시
- [ ] Canvas_HUD 활성, Canvas_Overlay 비활성
- [ ] 콘솔 에러 없음

---

### Phase C: SCN_MainMenu / SCN_Loading 씬 구성

**목표**: 메인 메뉴와 로딩 씬의 최소 골격 생성

#### C-1. SCN_MainMenu

```
Step C-1-01~02: 씬 생성 및 열기
Step C-1-03~07: 구분선 + 카메라 배경색 설정 (#2C3E50)
Step C-1-08~12: Canvas_MainMenu 생성
Step C-1-13~18: TitleText, ButtonPanel (Btn_NewGame, Btn_Continue, Btn_Settings, Btn_Quit)
Step C-1-19~20: SettingsPanel (비활성)
Step C-1-21: save_scene
```

- **MCP 호출 수**: ~21회

#### C-2. SCN_Loading

```
Step C-2-01~02: 씬 생성 및 열기
Step C-2-03~06: 구분선 + 카메라 배경색 (#1A1A2E)
Step C-2-07~13: Canvas_Loading + LoadingText + ProgressBar
Step C-2-14: save_scene
```

- **MCP 호출 수**: ~14회

#### Phase C 검증 체크리스트

- [ ] SCN_MainMenu: Canvas_MainMenu 하위에 TitleText, ButtonPanel(4버튼), SettingsPanel(비활성)
- [ ] SCN_Loading: Canvas_Loading 하위에 LoadingText, ProgressBar
- [ ] 콘솔 에러 없음

---

### Phase D: 테스트 씬 구성 (SCN_Test_FarmGrid)

**목표**: 경작 시스템 단독 테스트 최소 씬

```
Step D-01~02: 씬 생성 및 열기
Step D-03~08: TestTimeManager, FarmSystem > FarmGrid + GrowthSystem
Step D-09~14: 카메라 설정 (SCN_Farm과 동일 쿼터뷰)
Step D-15~18: Directional Light (고정, 시간 변화 없음)
Step D-19~21: TestPlayer 생성 (간소화된 플레이어, 이동 + 도구 사용만)
Step D-22: save_scene
```

- **MCP 호출 수**: ~19회

#### Phase D 검증 체크리스트

- [ ] FarmSystem > FarmGrid, GrowthSystem 계층 존재
- [ ] 카메라 쿼터뷰 설정
- [ ] 빌드 설정에 미포함

---

### Phase E: 검증 및 연결

**목표**: Build Settings에 씬 등록, 전체 무결성 검증

#### E-1. Build Settings 씬 등록

```
Index 0: Assets/_Project/Scenes/Main/SCN_Loading.unity    (부트스트랩)
Index 1: Assets/_Project/Scenes/Main/SCN_MainMenu.unity
Index 2: Assets/_Project/Scenes/Main/SCN_Farm.unity
(Test 씬은 미포함)
```

[RESOLVED] Build Settings 편집: `manage_build`로 build scenes 관리 가능. Editor 스크립트 불필요.

#### E-2. 최종 순회 검증

```
4개 씬 모두 open_scene -> enter_play_mode -> 에러 확인 -> exit_play_mode
```

#### Phase E 검증 체크리스트

- [ ] Build Settings에 3개 씬 등록 (Loading=0, MainMenu=1, Farm=2)
- [ ] Test 씬 미포함
- [ ] 4개 씬 Play Mode 진입/종료 시 에러 없음
- [ ] SCN_Farm 시각 확인: 녹색 바닥, 쿼터뷰 카메라, Directional Light

---

## 9. 의존성 그래프

```
Phase A ────────────────────────────────────────────────┐
  A-1 (폴더 구조)                                        │
    |                                                    │
    +-->  A-2 (URP 설정)     <-- P-2 (URP 패키지)        │
    |                                                    │
    +-->  A-3 (Input System) <-- P-3 (Input System)      │
                                                         │
Phase B <────────────────────────────────────────────────+
  B-1 (씬 생성) <-- A-1 (Scenes/Main 폴더)               │
    |                                                    │
    +--> B-2 (구분선) --> B-3~B-6, B-10 (섹션별 오브젝트) │
    |                      |                             │
    |                      +--> B-7 (Terrain)            │
    |                      +--> B-8 (라이팅)              │
    |                      +--> B-11 (UI)                │
    |                                                    │
    +--> B-9 (카메라)                                     │
    |                                                    │
    +--> B-13 (저장) <-- B-2~B-12 모두 완료               │
                                                         │
Phase C (B와 독립, A 완료 필요) <────────────────────────+
  C-1 (MainMenu) <-- A-1 (Scenes/Main 폴더)              │
  C-2 (Loading)  <-- A-1 (Scenes/Main 폴더)              │
                                                         │
Phase D (B, C와 독립, A 완료 필요) <─────────────────────+
  D (Test 씬) <-- A-1 (Scenes/Test 폴더)                 │
                                                         │
Phase E <── B-13, C-1, C-2, D 모두 완료 ────────────────+
  E-1 (Build Settings)
  E-2 (순회 검증)
```

### 병렬 실행 가능 구간

| 구간 | 병렬 가능 태스크 |
|------|-----------------|
| Phase A 내부 | A-2, A-3은 A-1 완료 후 병렬 실행 가능 |
| Phase B~D | C-1, C-2, D는 B 완료를 기다리지 않음 (A 완료만 필요) |
| Phase E | 모든 Phase 완료 후에만 실행 |

---

## 10. 예상 MCP 호출 수 및 시간

| Phase | 태스크 수 | MCP 호출 예상 | 예상 소요 시간 |
|-------|----------|--------------|---------------|
| A-1 (폴더) | 47 | 47회 | 2~3분 |
| A-2 (URP) | 3 | 3회 (수동 대체 가능) | 1분 |
| A-3 (Input) | 1 | 1회 | 30초 |
| B (SCN_Farm) | ~60 | ~60회 | 5~7분 |
| C-1 (MainMenu) | 21 | 21회 | 2~3분 |
| C-2 (Loading) | 14 | 14회 | 1~2분 |
| D (Test 씬) | 19 | 19회 | 2~3분 |
| E (검증) | ~10 | ~10회 | 2~3분 |
| **합계** | **~175** | **~175회** | **15~22분** |

---

## Open Questions

- [OPEN] 상점/출하함의 농장 그리드 기준 배치 위치 미정. 동선 테스트 필요.
- [OPEN] 초기 지급 씨앗 수량과 종류 미정. 제안: 감자 5개 + 당근 5개.
- [OPEN] Day 1~3 날씨를 맑음으로 고정할지 여부. 튜토리얼 경험 보장 관점에서 고정 제안.
- [OPEN] 경험치 테이블(레벨별 필요 XP) 미정. 밸런스 시트(BAL-001)에서 확정 필요.
- [OPEN] 게임 시작 시각을 06:00으로 할지 08:00으로 할지. 08:00 제안 (상점이 열려 있어 첫 구매 가능).
- [OPEN] MCP for Unity에서 `create_folder` 도구 지원 여부.
- [OPEN] 씬 생성 시 빈 씬(Empty Scene)으로 생성 가능한지, 기본 템플릿(Camera + Light 포함)만 지원하는지.
- [OPEN] CanvasScaler 세부 설정을 이 Phase에서 잡을지, UI 전용 Phase로 분리할지.

## Risks

> **2026-04-06 검증 완료**: com.coplaydev.unity-mcp v9.6.5 (42개 툴, 25개 리소스) 기준.
> farm-game-unity 프로젝트에서 실제 패키지 분석 후 대조.

- [RESOLVED] ~~URP Pipeline Asset을 MCP로 생성/할당 불가~~ -> `manage_graphics` (33+ 액션)로 pipeline info/settings/quality, URP renderer features 관리 가능.
- [RESOLVED] ~~RenderSettings(Ambient Light 등) MCP 접근 불가~~ -> `manage_graphics`로 lighting, volume/post-processing, light baking settings 모두 지원.
- [RESOLVED] ~~Build Settings 편집 MCP 미지원~~ -> `manage_build`로 build scenes 관리, 플랫폼 전환, player settings 설정 가능.
- [WORKAROUND] Input Actions 내부 액션 맵 MCP 편집 불가 -> `.inputactions`는 JSON 파일이므로 `manage_asset` create + 파일 직접 작성으로 우회 가능.
- [RESOLVED] ~~Canvas 컴포넌트 설정 시 MCP 프로퍼티 경로 불일치~~ -> `manage_ui`로 Canvas/Button/Text/Image/Slider/Layout Groups 전부 지원. `manage_components`로 세부 속성도 설정 가능.
- [RESOLVED] ~~씬 생성 시 기본 포함 오브젝트(Camera, Light)가 버전별 상이~~ -> `find_gameobjects`로 즉시 조회, `manage_scene` get hierarchy로도 확인 가능.
- [RESOLVED] ~~MCP 도구명/파라미터가 실제 설치 버전과 불일치~~ -> v9.6.5 전체 42개 툴 검증 완료.
- [RISK] 테스트 씬의 TestTimeManager가 본 게임 TimeManager와 다르게 동작할 위험. 핵심 로직 공유 필요.
- [RISK] 초기 플레이 경험에서 튜토리얼/가이드 부재 시 방향 상실 가능. 최소 팝업 가이드 검토 필요.

---

## Cross-references

- `docs/design.md` -- 게임 디자인 마스터 (UI 구성 섹션 6, 비주얼 방향 섹션 7, 작물 데이터 섹션 4.2)
- `docs/architecture.md` -- 기술 아키텍처 (MCP 전략 섹션 2, 프로젝트 구조 섹션 3, 렌더링 섹션 5)
- `docs/systems/project-structure.md` -- 폴더 구조 (섹션 1), 씬 구조 (섹션 5), 네이밍 (섹션 6)
- `docs/systems/farming-system.md` -- 농장 그리드 (섹션 1), 타일 상태 머신 (섹션 2)
- `docs/systems/farming-architecture.md` -- 경작 시스템 기술 아키텍처 (MCP Phase A)
- `docs/systems/time-season.md` -- 시간대 구분 (섹션 1.2), 날짜 배치 처리 (섹션 1.6), 계절별 환경 (섹션 2.3)
- `docs/systems/economy-system.md` -- 초기 골드 500G (섹션 1.2), 출하함 판매 (섹션 4.1)
- `docs/systems/crop-growth.md` -- 성장 단계, 계절별 성장 보정
- `docs/mcp/farming-tasks.md` -- 농장 그리드 MCP 태스크 (작성 예정, ARC-003)

---

*이 문서는 Claude Code가 게임 디자인 및 기술 아키텍처 관점에서 자율적으로 작성했습니다.*
