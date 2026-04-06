# ARC-008 도구 업그레이드 MCP 태스크 설계를 위한 디자인 분석 보고서

> 작성: Claude Code (Opus) | 2026-04-06  
> 목적: ARC-008 MCP 태스크 시퀀스 작성 전, 디자인 관점의 요구사항 정리

---

## 1. Unity 씬에 배치되어야 하는 오브젝트 목록

### 1.1 데이터 에셋 (ScriptableObject)

| 에셋 | 설명 | 비고 |
|------|------|------|
| `SO_Tool_Hoe` | 호미 ToolData SO (Basic/Reinforced/Legendary 속성 포함) | 기존 ARC-003의 ToolData SO를 확장하거나 재생성 |
| `SO_Tool_WateringCan` | 물뿌리개 ToolData SO | 동일 |
| `SO_Tool_Sickle` | 낫 ToolData SO | 동일 |
| `SO_Material_IronScrap` | 철 조각 아이템 데이터 SO | 신규 아이템 — 카테고리 `Material` |
| `SO_Material_RefinedSteel` | 정제 강철 아이템 데이터 SO | 신규 아이템 — 카테고리 `Material` |

**ToolData SO 확장 필요성**: 기존 `ToolData`(ARC-003에서 생성)에는 `toolTier`가 단일 값으로 설정되어 있을 수 있다. 도구 업그레이드 시스템에서는 등급별 속성(범위, 에너지 소모, 쿨다운, 특수 효과)이 모두 달라지므로, ToolData SO가 **등급별 스탯 배열**을 가지거나 **등급별 별도 SO**를 가져야 한다.

[OPEN] ToolData SO 구조 결정 필요:
- **방식 A**: 단일 ToolData SO에 `TierStats[]` 배열로 등급별 수치를 내장
- **방식 B**: 등급별 별도 SO (`SO_Tool_Hoe_Basic`, `SO_Tool_Hoe_Reinforced`, `SO_Tool_Hoe_Legendary`)
- 방식 A가 에셋 수 최소화 및 업그레이드 로직 단순화에 유리. ARC-003의 기존 ToolData 스키마 확인 후 결정해야 한다.

### 1.2 씬 오브젝트

| 오브젝트 | 부모 계층 | 역할 | 비고 |
|----------|-----------|------|------|
| `ToolUpgradeManager` | `--- MANAGERS ---` | 업그레이드 상태 관리 (진행 중 도구, 남은 일수, 완료 판정) | MonoBehaviour, 싱글톤 |
| `NPC_Cheolsu` | `--- VILLAGE ---` 또는 `--- NPCs ---` | 대장간 NPC 오브젝트 (위치, 인터랙션 콜라이더) | 마을 외곽 배치 |
| `Blacksmith_Building` | `--- VILLAGE ---` | 대장간 건물 시각적 오브젝트 | 철수 NPC 인접 |
| `Blacksmith_InteractionZone` | `NPC_Cheolsu` 하위 | BoxCollider2D 트리거 — 플레이어 접근 감지 | E키 인터랙션 영역 |

### 1.3 UI 오브젝트

| 오브젝트 | 부모 계층 | 역할 |
|----------|-----------|------|
| `Panel_BlacksmithMenu` | `Canvas_Overlay` | 대장간 메인 메뉴 (업그레이드 / 수령 / 재료 구매 탭) |
| `Panel_UpgradeSelect` | `Panel_BlacksmithMenu` | 도구 선택 + 등급 정보 표시 패널 |
| `Panel_UpgradeConfirm` | `Panel_BlacksmithMenu` | 업그레이드 확인 팝업 (비용, 소요시간, 도구 부재 경고) |
| `Panel_UpgradeComplete` | `Panel_BlacksmithMenu` | 완성 도구 수령 연출 패널 |
| `Panel_MaterialShop` | `Panel_BlacksmithMenu` | 재료 구매 상점 UI |
| `Indicator_ToolLocked` | 툴바 슬롯 오버레이 | 업그레이드 중인 도구 슬롯에 잠금 아이콘 표시 |
| `Indicator_UpgradeReady` | NPC 또는 미니맵 | 업그레이드 완료 알림 표시 (느낌표 아이콘 등) |

---

## 2. 플레이어 UX 흐름 (대장간 NPC 인터랙션)

### 2.1 전체 흐름도

```
[농장에서 활동 중]
    │
    ├── 마을로 이동 → 대장간 외곽 도착
    │
    ├── 철수 NPC 근처 접근 → "E키로 대화" 프롬프트 표시
    │
    └── E키 입력 → [대장간 메뉴 열림]
            │
            ├── [1] "도구 업그레이드" ─────────────────────────┐
            │       │                                          │
            │       ├── 도구 3종 목록 표시                      │
            │       │   ├── 업그레이드 가능: 밝은 아이콘         │
            │       │   │   (등급, 비용, 재료 표시)              │
            │       │   ├── 조건 미충족: 회색 아이콘             │
            │       │   │   (미달 사유 툴팁: 레벨/골드/재료)     │
            │       │   └── 이미 최대 등급: "전설" 뱃지 표시     │
            │       │                                          │
            │       ├── 도구 선택 → 확인 팝업                   │
            │       │   ├── 도구명, 현재 등급 → 다음 등급        │
            │       │   ├── 비용: 골드 + 재료 (보유량 표시)      │
            │       │   ├── 소요 시간: N일                      │
            │       │   ├── 경고: "업그레이드 중 사용 불가"      │
            │       │   ├── [확인] → 골드/재료 차감, 도구 잠금   │
            │       │   └── [취소] → 메뉴로 복귀                │
            │       │                                          │
            │       └── 확인 시:                                │
            │           ├── 도구가 인벤토리에서 시각적으로 잠김   │
            │           ├── 툴바 슬롯에 잠금 아이콘 오버레이     │
            │           ├── 철수 대사: "맡겨. N일이면 될 거야."  │
            │           └── ToolUpgradeManager에 진행 등록      │
            │                                                  │
            ├── [2] "도구 수령" (완성 도구 있을 때만 활성화) ────┐
            │       │                                          │
            │       ├── 완성된 도구 정보 표시                    │
            │       │   (새 등급, 변경된 능력치 비교)            │
            │       ├── [수령] → 도구 잠금 해제, 등급 변경       │
            │       │   ├── 도구 외형 변경 (스프라이트 교체)     │
            │       │   ├── 수령 연출 (이펙트 + 사운드)         │
            │       │   ├── 철수 대사: "다 됐어. 잘 써."        │
            │       │   └── XP 획득 (→ see tool-upgrade.md 8)  │
            │       └── [닫기] → 메뉴로 복귀                    │
            │                                                  │
            └── [3] "재료 구매" ─────────────────────────────────┐
                    │                                          │
                    ├── 상점 UI (일반 상점과 동일 패턴)          │
                    │   ├── 철 조각 / 정제 강철 목록             │
                    │   ├── 수량 선택 (1/5/10)                  │
                    │   └── 구매 확인 → 골드 차감, 인벤토리 추가 │
                    └── [닫기] → 메뉴로 복귀                    │
```

### 2.2 시간 경과에 따른 업그레이드 상태 전이

```
[Idle] ──의뢰──> [InProgress] ──N일 경과──> [Ready] ──수령──> [Idle]
  │                  │                        │
  │                  ├── 도구 사용 불가         ├── NPC 위에 알림 표시
  │                  ├── 툴바 잠금 아이콘       ├── 대화 시 "수령" 탭 활성화
  │                  └── 매일 새벽 잔여일 -1    └── 수령 전까지 도구 계속 잠금
  │
  └── 도구 사용 가능 (정상)
```

**상태 데이터 구조** (ToolUpgradeManager가 관리):

| 필드 | 타입 | 설명 |
|------|------|------|
| `activeUpgrades` | `Dictionary<ToolType, UpgradeSlot>` | 현재 진행 중인 업그레이드 (도구당 최대 1개) |
| `UpgradeSlot.toolType` | `ToolType` | 업그레이드 대상 도구 |
| `UpgradeSlot.targetTier` | `ToolTier` | 목표 등급 |
| `UpgradeSlot.remainingDays` | `int` | 남은 제작 일수 |

### 2.3 일일 타임라인에서의 업그레이드 처리

- **새벽 이벤트** (`TimeManager.OnDayStart`): `ToolUpgradeManager`가 `remainingDays--` 처리. 0 도달 시 상태를 `Ready`로 전환
- **영업시간 외 방문**: 대장간 인터랙션 불가. 문 앞에 "영업 종료" 표시. 플레이어가 접근해도 E키 프롬프트 미표시
- **휴무일**: 동일하게 인터랙션 불가. 업그레이드 제작은 휴무일에도 진행됨 (제작은 NPC 영업과 무관하게 내부 타이머로 처리)

---

## 3. 도구별 효과 수치 (canonical 참조)

MCP 태스크에서 ToolData SO의 등급별 필드를 설정할 때, 아래 참조처에서 수치를 가져와야 한다.

### 3.1 호미 (Hoe)

- 범위, 에너지 소모, 쿨다운, 특수 효과: (-> see `docs/systems/tool-upgrade.md` 섹션 3.1)
- 애니메이션 타입: (-> see `docs/systems/tool-upgrade.md` 섹션 3.1 애니메이션 행)

### 3.2 물뿌리개 (Watering Can)

- 범위, 저수량, 에너지 소모, 쿨다운, 리필 시간, 특수 효과: (-> see `docs/systems/tool-upgrade.md` 섹션 3.2)
- 물탱크 시설과의 역할 분담: (-> see `docs/systems/tool-upgrade.md` 섹션 3.2 물탱크 시설 단락)

### 3.3 낫 (Sickle)

- 범위, 에너지 소모, 쿨다운, 보너스 수확 확률, 품질 상승 확률, 씨앗 회수 확률: (-> see `docs/systems/tool-upgrade.md` 섹션 3.3)

### 3.4 에너지 효율 종합표

- 전 도구 x 전 등급 타일당 에너지 비교: (-> see `docs/systems/tool-upgrade.md` 섹션 4)

### 3.5 튜닝 파라미터 전체 목록

- 조정 가능한 파라미터 15종의 현재 값, 조정 범위, 영향: (-> see `docs/systems/tool-upgrade.md` 섹션 9)

---

## 4. 업그레이드 비용 구조 (canonical 참조)

### 4.1 골드 + 재료 비용

- 등급별 골드 비용, 필요 재료 종류/수량, 플레이어 레벨 요건, 소요 시간: (-> see `docs/systems/tool-upgrade.md` 섹션 2.1)

### 4.2 재료 아이템 정보

- 철 조각(`iron_scrap`), 정제 강철(`refined_steel`)의 획득 경로, 단가: (-> see `docs/systems/tool-upgrade.md` 섹션 2.2)

### 4.3 대장간 상점 판매 품목

- 재료 판매 가격, 재고 규칙: (-> see `docs/systems/tool-upgrade.md` 섹션 6.3)

### 4.4 업그레이드 XP 보상

- 등급별 XP 획득량: (-> see `docs/systems/tool-upgrade.md` 섹션 8)

---

## 5. MCP 태스크 설계 시 반드시 포함해야 할 디자인 요소

### 5.1 도구 잠금/해제 메커니즘

MCP 구현에서 가장 핵심적이면서 누락 가능성이 높은 요소이다.

| 항목 | 설명 | 구현 필수 사항 |
|------|------|---------------|
| **도구 잠금 상태** | 업그레이드 의뢰 시 해당 도구를 인벤토리에서 사용 불가로 전환 | `InventoryManager`와 `ToolUpgradeManager` 간 이벤트 통신 필요. 도구 아이템의 `isLocked` 플래그 또는 별도 잠금 시스템 |
| **툴바 시각적 잠금** | 잠긴 도구의 툴바 슬롯에 자물쇠 아이콘 오버레이 | `InventoryUI`가 잠금 상태를 구독하여 오버레이 표시 |
| **잠금 중 도구 선택 방지** | 잠긴 슬롯을 선택해도 "업그레이드 중입니다" 메시지만 표시 | 툴바 선택 로직에 잠금 체크 삽입 |
| **잠금 해제** | 수령 시 잠금 해제 + 등급 변경 + 스프라이트 교체를 원자적으로 처리 | 수령 이벤트 한 번에 3가지 상태 변경을 동시에 수행 |

[RISK] 도구 잠금 상태가 세이브/로드에 올바르게 반영되어야 한다. 업그레이드 진행 중에 게임을 종료하고 다시 로드했을 때, 잠금 상태와 잔여 일수가 복원되어야 한다. `ToolUpgradeManager`의 `activeUpgrades`가 `SaveManager`와 연동되어야 한다.

### 5.2 NPC 인터랙션 시스템과의 연동

| 항목 | 설명 |
|------|------|
| **인터랙션 프롬프트** | 철수 NPC 근처 접근 시 "E키로 대화" 프롬프트 표시. 기존 NPC 인터랙션 패턴(하나, 목이)과 동일한 시스템 사용 |
| **영업시간 체크** | 인터랙션 가능 여부를 `TimeManager.CurrentHour`로 판단. 영업 외 시간에는 프롬프트 미표시 또는 "영업 종료" 대사 |
| **대화 시스템 연동** | 철수의 대사 표시(인사말, 업그레이드 관련 대사, 계절 대사, 진행도 대사)가 대화 UI를 통해 출력 |
| **메뉴 분기** | 대화 후 3가지 메뉴(업그레이드/수령/구매) 선택. 일반 상점 NPC(하나)와 달리 **업그레이드+수령 탭이 추가** |

[OPEN] NPC 인터랙션 공통 시스템(대화 트리거, 프롬프트, 대화 UI)이 아직 MCP 태스크로 정의되지 않았다. ARC-008이 NPC 인터랙션 공통 모듈을 직접 만들지, 별도 태스크(ARC-NPC?)에 의존할지 결정 필요. facilities-tasks.md(ARC-007)의 `BuildingInteraction`과 유사하지만 NPC 기반이라는 차이가 있다.

### 5.3 조건 검증 로직

업그레이드 UI에서 도구별 업그레이드 가능 여부를 판단하는 조건이 복합적이다.

| 조건 | 검증 대상 | 실패 시 UI 표현 |
|------|-----------|----------------|
| 플레이어 레벨 | `PlayerLevel >= requiredLevel` | 회색 표시 + "레벨 N 필요" 툴팁 |
| 골드 보유량 | `CurrentGold >= upgradeCost` | 비용 텍스트 빨간색 |
| 재료 보유량 | `InventoryManager.GetItemCount(materialId) >= requiredCount` | 재료 수량 빨간색 |
| 도구 현재 등급 | `currentTier < Legendary` | "최대 등급" 표시, 버튼 비활성화 |
| 이미 업그레이드 중 | `!activeUpgrades.ContainsKey(toolType)` | "업그레이드 진행 중" 표시 |
| 도구 미소유 | 발생 불가 (도구는 파괴/분실 불가, 게임 시작 시 지급) | - |

**복합 검증 순서**: 등급 체크 -> 진행 중 체크 -> 레벨 체크 -> 골드/재료 체크 순으로 우선순위를 부여하여 가장 근본적인 차단 사유를 먼저 표시한다.

### 5.4 UI 세부 요구사항

#### 5.4.1 업그레이드 선택 패널

| 요소 | 상세 |
|------|------|
| 도구 아이콘 | 현재 등급의 스프라이트 표시. 다음 등급 스프라이트를 화살표 우측에 미리보기 |
| 능력치 비교 | 현재 vs 다음 등급의 주요 수치 비교 (범위, 에너지, 특수 효과). 개선 수치는 초록색, 악화 수치는 빨간색 |
| 비용 표시 | 골드 아이콘 + 금액, 재료 아이콘 + 수량. 보유량/필요량 형식 (예: "2/3") |
| 소요 시간 | 달력 아이콘 + "N일" |

#### 5.4.2 수령 패널

| 요소 | 상세 |
|------|------|
| 도구 등장 연출 | 새 등급 도구가 중앙에 크게 표시, 발광 이펙트 |
| 능력치 변경 요약 | "범위 1x1 -> 1x3", "에너지 2 -> 3" 등 변경점 나열 |
| XP 획득 표시 | "+15 XP" 팝업 |

#### 5.4.3 등급 시각적 구분

- 툴바/인벤토리 슬롯의 등급 표시(테두리 색상, 아이콘): (-> see `docs/systems/tool-upgrade.md` 섹션 7.3)

### 5.5 이벤트 통신 설계

`ToolUpgradeManager`가 발행해야 하는 이벤트 목록:

| 이벤트 | 발행 시점 | 구독자 |
|--------|-----------|--------|
| `OnUpgradeStarted(ToolType, ToolTier)` | 업그레이드 의뢰 확인 시 | `InventoryManager`(도구 잠금), `InventoryUI`(잠금 표시) |
| `OnUpgradeCompleted(ToolType, ToolTier)` | 잔여일 0 도달 시(새벽) | `NPCUI`(알림 아이콘), `InventoryUI`(준비됨 표시) |
| `OnUpgradeCollected(ToolType, ToolTier)` | 도구 수령 시 | `InventoryManager`(도구 잠금 해제 + 등급 변경), `InventoryUI`(스프라이트 교체), `ProgressionManager`(XP 지급) |

### 5.6 세이브/로드 연동

| 저장 데이터 | 설명 |
|------------|------|
| 진행 중 업그레이드 목록 | `List<UpgradeSaveEntry>` — 각 항목에 toolType, targetTier, remainingDays |
| 현재 도구 등급 | InventoryManager의 도구 아이템에 `toolTier` 값으로 이미 저장됨 (-> see `docs/systems/inventory-architecture.md`) |

### 5.7 누락 가능한 인터랙션 및 엣지 케이스

| 케이스 | 설명 | 필요한 처리 |
|--------|------|------------|
| **동시 복수 업그레이드** | 도구 A를 업그레이드 중에 도구 B도 의뢰 가능한가? | 현재 설계상 가능해야 함. 3종 동시 업그레이드 시 도구 전부 잠금 — 플레이어가 농작업을 전혀 할 수 없음. 경고 UI 필요 |
| **모든 도구 잠금 상태** | 3종 모두 업그레이드 중일 때 경작/물주기/수확 불가 | 특별한 차단 없이 허용하되, 확인 팝업에서 "현재 사용 가능한 도구가 없게 됩니다" 추가 경고 |
| **업그레이드 중 계절 전환** | 가을 28일에 의뢰하여 겨울 1일에 완성 | 정상 처리. 제작은 계절/날씨와 무관하게 진행 |
| **업그레이드 중 잠자기** | 도구 부재 상태에서 잠자기 선택 | 정상 허용. 잠자기에 도구 요건 없음 |
| **수령 안 하고 다음 업그레이드 의뢰** | 완성된 도구를 수령하지 않은 채로 다른 도구 의뢰 | 허용. 대장간에 완성 도구 + 진행 중 도구가 동시에 존재 가능 |
| **영업시간 직전에 의뢰** | 영업 종료 5분 전 진입, 대화 중 영업 종료 | 대화/메뉴가 열린 상태에서는 영업시간 체크하지 않음. 대화 시작 시점에만 체크 |
| **범위 도구 사용 시 빈 타일** | 3x3 중 일부만 유효할 때 에너지 처리 | 전체 소모 (-> see `docs/systems/tool-upgrade.md` Open Question 4) |

### 5.8 기존 MCP 태스크와의 의존성 정리

| 의존 대상 | 의존 내용 | 비고 |
|-----------|-----------|------|
| ARC-002 (씬 셋업) | `--- MANAGERS ---`, `Canvas_Overlay` 계층 | 이미 존재해야 함 |
| ARC-003 (경작 시스템) | `ToolData` SO 스키마, 기존 도구 SO 에셋 | ToolData 확장 또는 교체 필요 |
| ARC-007 (시설 시스템) | `BuildingInteraction` 패턴 참조 | NPC 인터랙션은 별도 구현이지만 패턴 유사 |
| 미정 (NPC 공통 시스템) | NPC 대화 UI, 인터랙션 프롬프트, 영업시간 체크 | [OPEN] 별도 태스크로 분리할지 ARC-008에 포함할지 |
| 인벤토리 MCP 태스크 | `InventoryManager`, 슬롯 시스템 | 도구 잠금/해제 이벤트 수신 |

### 5.9 MCP 태스크 구조 제안 (Phase 분할)

facilities-tasks.md(ARC-007)의 패턴을 참고하여 다음과 같은 Phase 분할을 제안한다:

| Phase | 내용 | 예상 태스크 |
|-------|------|------------|
| **T-1** | ToolData SO 확장 + 재료 아이템 SO 생성 | ToolData에 TierStats 배열 추가, SO_Material 2종 생성 |
| **T-2** | ToolUpgradeManager 스크립트 생성 | 상태 관리, 일수 카운트다운, 이벤트 발행 |
| **T-3** | 대장간 NPC 프리팹 생성 + 씬 배치 | NPC 오브젝트, 인터랙션 존, 마을 외곽 배치 |
| **T-4** | 대장간 메뉴 UI 생성 | 업그레이드 선택/확인/수령 패널, 재료 상점 |
| **T-5** | 도구 잠금/해제 시스템 연결 | InventoryManager 연동, 툴바 잠금 오버레이 |
| **T-6** | 통합 테스트 시퀀스 | 업그레이드 의뢰 -> 일수 경과 -> 수령 -> 등급 변경 검증 |

---

## 6. Cross-references

| 문서 | 참조 내용 |
|------|-----------|
| `docs/systems/tool-upgrade.md` | 모든 도구 업그레이드 수치의 canonical 문서 |
| `docs/systems/inventory-system.md` 섹션 1.3 | 도구 아이템 추가 속성 (toolTier, energyCost, range) |
| `docs/systems/inventory-architecture.md` | 인벤토리 아키텍처 (IInventoryItem, 슬롯 시스템, 세이브 구조) |
| `docs/content/npcs.md` 섹션 4 | 대장간 NPC 철수 캐릭터 설정, 대화 스크립트 |
| `docs/mcp/facilities-tasks.md` | MCP 태스크 시퀀스 패턴 참조 (ARC-007) |
| `docs/systems/economy-system.md` 섹션 3.2 | 대장간 영업시간, 휴무일 |
| `docs/balance/crop-economy.md` | 경제 밸런스 수치 (업그레이드 비용 대비 수입 검증) |
| `docs/balance/progression-curve.md` 섹션 1.2.4 | 도구 업그레이드 XP 보상 |
| `docs/mcp/farming-tasks.md` (ARC-003) | 기존 ToolData SO 스키마 — 확장 필요 여부 확인 |

---

## 7. Open Questions 요약

1. [OPEN] **ToolData SO 구조**: 등급별 스탯을 단일 SO 내 배열로 관리할지, 등급별 별도 SO로 분리할지 (섹션 1.1 참조)
2. [OPEN] **NPC 공통 인터랙션 시스템**: 대화 트리거, 프롬프트 UI, 영업시간 체크를 ARC-008에 포함할지 별도 태스크로 분리할지 (섹션 5.2 참조)
3. [OPEN] **마을 씬 구조**: NPC와 마을 건물이 FarmScene에 포함되는지, 별도 VillageScene으로 분리되는지. 씬 전환 방식에 따라 NPC 배치 태스크가 달라짐
4. [OPEN] **재료 드롭 경로**: 상점 구매 외 드롭 획득 구현 여부 (-> see `docs/systems/tool-upgrade.md` Open Question 1). MCP 태스크 범위에 영향

---

*이 문서는 ARC-008 MCP 태스크 시퀀스 작성을 위한 디자인 분석 보고서이다. 수치의 직접 기재를 지양하고, 모든 게임플레이 파라미터는 canonical 문서 참조로 표기했다.*
