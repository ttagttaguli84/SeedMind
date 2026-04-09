# Devlog 100 — CON-001-ARC: 작물 콘텐츠 MCP 구현

> 작성: 2026-04-10 | 세션: crop-content-tasks.md 실행

---

## 작업 내용

`docs/mcp/crop-content-tasks.md` (CON-001-ARC) Phase A~E 전체 실행 완료.

### Phase A — 전제 스크립트 생성

기존 `CropData.cs`가 단순 버전(ScriptableObject 직접 상속)으로 존재했고, 의존 클래스들이 모두 없었다.
다음 스크립트를 순서대로 생성:

| 파일 | 내용 |
|------|------|
| `GameDataSO.cs` | 모든 SO의 abstract 베이스 클래스 (dataId, displayName, icon) |
| `IInventoryItem.cs` | 인벤토리 아이템 공통 인터페이스 |
| `ItemType.cs` | 아이템 분류 enum (Crop, Seed, Tool, ...) |
| `SeasonFlag.cs` | [Flags] 재배 계절 비트마스크 (기존 CropData 내 Season에서 분리) |
| `CropCategory.cs` | 작물 분류 enum (Vegetable, Fruit, FruitVegetable, Fungi, Flower, Special) |
| `CropData.cs` | 전면 업데이트: GameDataSO 상속, IInventoryItem 구현, 전체 필드 추가 |

### Phase B — SO 에셋 11종 생성/업데이트

수치 수정 사항:
- 당근 씨앗가: 15G → 18G (canonical 반영)
- 토마토 씨앗가: 25G → 30G (canonical 반영)

YAML 직접 편집으로:
- 기존 3종(Potato/Carrot/Tomato): 새 필드 추가, 수치 수정, 기존 프리팹 참조 유지
- 신규 5종: Corn/Strawberry/Pumpkin/Sunflower/Watermelon (봄~가을)
- 겨울 3종: WinterRadish/Shiitake/Spinach (`requiresGreenhouse=true`, `allowedSeasons=8`)

특이 사항: isRepeating/regrowDays 필드 적용
- 딸기: `isRepeating=true`, `regrowDays=3`
- 표고버섯: `isRepeating=true`, `regrowDays=5`
- 호박/수박: `giantCropChance=0.05`

### Phase C+D — 프리팹 + 머티리얼 자동 생성

34개 개별 파일 작성 대신 **Editor 자동화 스크립트** (`CreateCropPrefabs.cs`) 방식 채택.

`[MenuItem("SeedMind/Create Crop Prefabs")]`로 트리거 → `execute_menu_item`으로 실행:
- 8종 머티리얼 생성 (M_Crop_<Name>.mat, 작물별 고유 색상)
- 8종 × 4단계 = 32 Stage 프리팹 생성 (Sphere/Capsule placeholder)
- Pumpkin/Watermelon Giant 프리팹 2개 생성
- SerializedObject를 통해 각 CropData SO의 `growthStagePrefabs` 배열 자동 연결

### Phase E — DataRegistry 기본 구조

`DataRegistry.cs` (Singleton 상속) 생성:
- `Resources.LoadAll<GameDataSO>("Data")`로 런타임 스캔
- `Get<T>(dataId)` / `GetAll<T>()` 조회 메서드
- 완전 구현(Resources 폴더 이전 포함)은 `inventory-tasks.md`에서 처리

---

## 기술적 발견

### Git 브랜치 구조 재확인
- `main` 브랜치: Unity 전체 프로젝트 (`C:\UE\SeedMind\`) — Unity 에셋 커밋 대상
- `wt-seedmind` 브랜치: 문서/AI 파일 전용 (worktree at `.claude\worktrees\seedmind\`)
- 두 브랜치는 `docs/mcp/progress.md` 1개 파일만 다름
- Unity 에셋 커밋은 `git -C "C:/UE/SeedMind" add/commit/push origin main` 패턴 사용

### Editor 스크립트 우회 패턴
- `manage_scriptable_object`로 배열 참조 설정 불가 → `SerializedObject` + `[MenuItem]` 조합으로 해결
- `create_script` 대신 `Write` 도구 + `refresh_unity` 패턴이 안정적

---

## 다음 단계

`docs/mcp/progress.md` 업데이트: 다음 실행 파일 = `facilities-tasks.md` (C-2)
