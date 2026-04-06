# Devlog #008 — 데이터 파이프라인 설계 (ARC-004)

> 2026-04-06 | Phase 1 | 작성: Claude Code (Opus)

---

## 오늘 한 일

### ARC-004: 데이터 파이프라인 설계 (`docs/pipeline/data-pipeline.md`)

Designer + Architect 에이전트 병렬 실행으로 문서 작성.

**Part I — 게임 디자인**:
- **데이터 분류 체계**: 정적 데이터(SO 12종, ~87개 에셋), 동적 데이터(세이브 대상 11종), 파생 데이터(5종) 완전 분류
- **신규 SO 4종 정의**: BuildingData, ProcessingRecipeData, LevelConfig, InventoryItemData
- **신규 enum 6종**: CropCategory, BuildingEffectType, PlacementRule, ProcessingType, ItemType, UpgradeMaterial
- **세이브 데이터 구조**: 10종 SaveData JSON 스키마, 세이브 슬롯 3개, 예상 파일 크기 ~45KB
- **데이터 무결성 규칙**: SO 참조 맵, 필수/선택 필드, 값 범위 제약, ID 유일성
- **밸런스 훅**: BAL-001/002 대상 15개 밸런스 포인트, 3개 핵심 검증 공식

**Part II — 기술 아키텍처**:
- **GameDataSO 베이스 클래스**: dataId/displayName/icon + Validate() — 전 SO 공통
- **DataRegistry 싱글턴**: Resources.LoadAll로 SO 스캔, dataId 기반 O(1) 검색
- **SaveManager 상세**: ISaveable 인터페이스, 우선순위 기반 복원(Time→Weather→Economy→Farm→Player)
- **Newtonsoft.Json 채택**: Unity 6 기본 패키지, Dictionary/null 완전 지원
- **SaveMigrator**: SemVer 기반 단계적 마이그레이션 체인
- **DataValidator**: Editor/Runtime 이중 검증, 7가지 폴백 규칙
- **MCP Phase 3단계**: A(DataRegistry/GameDataSO) → B(SaveManager) → C(SaveData 클래스)

### 리뷰 및 수정

Reviewer가 **CRITICAL 7건, WARNING 8건, INFO 3건** 발견, 전부 수정:

**CRITICAL**:
1. SO 에셋 네이밍 `SO_Building_*` → `SO_Bldg_*` 통일
2. time-season-architecture.md 문서 ID ARC-003 → DES-003 수정
3. PlayerSaveData C# 클래스에 에너지 필드 누락 → 추가
4. equippedToolIndex/currentToolIndex 불일치 → JSON 기준 통일
5. FarmTileSaveData에 soilQuality, consecutiveCrop* 필드 누락 → 추가
6. CropInstanceSaveData JSON/C# 필드 불일치 → C# 기준 통일
7. GameSaveData에 buildings/processing/unlocks/shops 누락 → 추가

**WARNING** (주요):
- InventorySaveData 분리 문제 → PlayerSaveData 포함으로 명시
- SO_Tool_Hand.asset 누락 → 에셋 트리에 추가
- Part II에 BuildingSaveData 등 4개 클래스 미작성 → 섹션 2.6 신규 추가

---

## 의사결정 기록

1. **GameDataSO 베이스 도입**: 모든 SO가 공통 dataId를 갖게 하여 세이브/로드 시 문자열 기반 역참조 가능. 기존 아키텍처 문서 일괄 수정 필요 (후속 작업).
2. **Newtonsoft.Json 채택**: Unity JsonUtility의 Dictionary/null 미지원 한계로 인해 선택. Unity 6 기본 패키지.
3. **ISaveable 우선순위 기반 복원**: 시간 → 날씨 → 경제 → 농장 → 플레이어 순서로 복원하여 의존성 보장.
4. **Resources.LoadAll 사용**: 초기 단순성 우선. SO 100개 초과 시 Addressables 전환 검토.

---

## 다음 단계

- BAL-001: 작물 경제 밸런스 시트 (Priority 3)
- BAL-002: 게임 진행 곡선 (Priority 3)
- CON-001~003: 콘텐츠 상세 (Priority 2)

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
