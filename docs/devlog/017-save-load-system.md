# Devlog #017 — 세이브/로드 시스템 (DES-008 + ARC-011)

> 2026-04-06 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### DES-008 + ARC-011: 세이브/로드 시스템 설계

Designer + Architect 병렬 실행 → Reviewer CRITICAL 4건·WARNING 3건 발견 후 전부 수정.

**신규 문서**:
1. `docs/systems/save-load-system.md` — 세이브/로드 UX 게임 디자인 canonical 문서 (9개 섹션)
2. `docs/systems/save-load-architecture.md` — 세이브/로드 기술 아키텍처 (Part I + Part II)

**수정된 문서**:
- `docs/systems/save-load-system.md` — 백업 파일명 통일(섹션 5.3), 쿨다운 30초→60초 수정(섹션 2.2)
- `docs/pipeline/data-pipeline.md` — 저장 경로·파일명 수정(섹션 3.1), GameSaveData에 inventory/npc/tutorial 필드 추가
- `docs/systems/project-structure.md` — Scripts/Save/ 폴더 트리 및 SeedMind.Save 네임스페이스 추가
- `docs/architecture.md` — ARC-011, DES-008 cross-reference 추가
- `docs/systems/inventory-architecture.md` — toolSlots[]→toolbarSlots[] 필드명 통일

---

## 핵심 설계 내용

### 세이브 철학 — 하이브리드 방식

**자동저장 중심 + 제한적 수동 저장** 채택:
- Stardew Valley 방식(하루 종료 시 저장)을 기반으로
- 10분 주기 백업 추가 (비정상 종료 대비)
- Save-scumming은 금지하지 않되, 시스템 자체(품질 시드 확정, 날씨 사전 결정)로 자연 억제

### 저장 슬롯 — 3슬롯 (data-pipeline.md 일치)
| 슬롯 표시 정보 |
|--------------|
| 농장 이름 |
| 날짜/계절 |
| 레벨/골드 스냅샷 |
| 플레이 시간 |
| 마지막 저장 일시 |

### 자동저장 트리거 (5종)
1. 하루 시작 (06:00) — 전날 종료 후 재개 시점 확정
2. 수면 실행 (침대 인터랙션)
3. 10분 주기 백업
4. 시설 건설/업그레이드 완료
5. 수동 저장 요청 (Esc 메뉴)

**쿨다운**: 60초 (AutoSaveTrigger.saveCooldownSeconds = 60f)

### GameSaveData 통합 구조 (16개 루트 필드)

흩어진 SaveData를 하나의 루트 클래스로 통합:
```
GameSaveData
├── version               (마이그레이션용)
├── slotName              (농장 이름)
├── totalPlaytimeSeconds  (플레이 시간)
├── farm                  → FarmSaveData
├── inventory             → InventorySaveData
├── progression           → ProgressionSaveData
├── economy               → EconomySaveData
├── time                  → TimeSaveData
├── facilities            → FacilitiesSaveData
├── npc                   → NPCSaveData
├── tools                 → ToolSaveData
├── tutorial              → TutorialSaveData
├── processing            → ProcessingSaveData
├── shops                 → ShopSaveData
├── unlocks               → UnlocksSaveData
└── milestones            → MilestonesSaveData
```

**PATTERN-005 준수**: JSON 스키마(섹션 2.2) ↔ C# 클래스(섹션 2.3) 16개 필드 완전 동기화

### 직렬화 방식 — Newtonsoft.Json 채택

- BinaryFormatter: 보안 취약점(CA2300)/deprecated로 배제
- JsonUtility: Dictionary 미지원으로 배제
- **Newtonsoft.Json**: Dictionary, 다형성, null 처리 모두 지원

### 원자적 쓰기 패턴
```
tmp → rename → .bak
save_N.json.tmp → save_N.json → 이전 save_N.json → save_N.json.bak
```
플랫폼별 파일 시스템 보장 여부는 [OPEN]으로 남김

### ISaveable 인터페이스 — 복원 순서 확정

| 순서 | 시스템 |
|------|--------|
| 10 | TimeManager |
| 20 | ProgressionManager |
| 30 | EconomyManager |
| 40 | FarmGrid |
| 50 | InventoryManager |
| 60 | FacilitiesManager |
| 65 | ProcessingManager |
| 70 | NPCManager |
| 75 | ToolUpgradeManager |
| 80 | TutorialManager |

---

## 리뷰 결과

**CRITICAL 4건 (수정 완료)**:

1. [C-1] save-load-system.md 섹션 5.3 백업 파일명 `save_slot_{N}.backup.json` → `save_{N}.json.bak` 통일
2. [C-2] data-pipeline.md 섹션 3.1 저장 경로(`saves/` vs `Saves/`)·파일명(`save_slot_{N}` vs `save_{N}`) 불일치 → 대소문자·인덱스 기준 통일
3. [C-3] 자동저장 쿨다운 save-load-system.md 30초 vs save-load-architecture.md 60초 → 60초로 통일, canonical 참조 추가
4. [C-4] data-pipeline.md GameSaveData 클래스에 inventory/npc/tutorial 3개 필드 누락 → 추가 (null 허용, 구버전 호환 주석)

**WARNING 3건 (수정 완료)**:

1. [W-1] project-structure.md에 Scripts/Save/ 폴더 미정의 → Save/ 폴더 트리 + SeedMind.Save 네임스페이스 추가
2. [W-2] architecture.md Cross-references에 새 문서 미등록 → ARC-011, DES-008 참조 추가
3. [W-3] inventory-architecture.md toolSlots[] vs data-pipeline.md toolbarSlots[] 필드명 불일치 → toolbarSlots[]로 통일

---

## 의사결정 기록

1. **하이브리드 세이브 방식 채택**: Stardew Valley 방식(하루 단위)을 축으로 하되 10분 백업을 추가. 순수 자동저장은 데이터 손실 위험이 있고, 완전 수동 저장은 save-scumming 유발. 하이브리드가 농장 시뮬레이션 장르의 표준이기도 함.

2. **Newtonsoft.Json 채택**: BinaryFormatter는 .NET 5+에서 deprecated + 보안 경고(CA2300). JsonUtility는 Dictionary를 지원하지 않아 NPCSaveData의 `relationLevels Dictionary<string,int>` 구조에 부적합. Newtonsoft.Json이 유일한 실질적 선택지.

3. **GameSaveData 통합 루트 클래스**: 기존 문서들(farming-, inventory-, tutorial-, progression-, economy-architecture)이 각자 SaveData를 정의했으나 루트 통합 클래스가 없어 슬롯 파일 구조가 불명확했음. 이번에 단일 루트로 통합하여 역직렬화 진입점을 명확히 함.

4. **복원 순서(SaveLoadOrder) 명시**: TimeManager → ProgressionManager → … → TutorialManager 순서를 ISaveable.SaveLoadOrder로 강제. 이유: FarmGrid 복원 시 TimeManager의 현재 계절이 먼저 필요하고, TutorialManager는 모든 시스템이 복원된 후 이벤트 구독을 재등록해야 함.

---

## 미결 사항 ([OPEN])

- 원자적 쓰기 패턴(tmp → rename)의 플랫폼별 파일 시스템 보장 여부 (특히 Android/iOS)
- Dictionary<string, int> 직렬화 방식 혼재 (Newtonsoft vs PlayerPrefs 백업)
- 수동 저장 제한 조건 세부화 (씬 전환 중 정확히 어느 프레임까지 차단할 것인가)
- save-load-tasks.md (ARC-012)에서 테스트 플레이어 세이브 사이클 검증 방식
- 멀티플레이 확장 가능성 (현재 완전 싱글 전제)

---

## 후속 작업

- `ARC-012`: 세이브/로드 MCP 태스크 시퀀스 독립 문서화 (`docs/mcp/save-load-tasks.md`)
- `ARC-007/008/009/010`: 시설·도구·NPC·튜토리얼 MCP 태스크 시퀀스 (Phase 2 전환 준비)
- `CON-005`: 가공/요리 시스템 콘텐츠 상세 (레시피 전체 목록)
- `DES-009`: 퀘스트/미션 시스템 설계

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
