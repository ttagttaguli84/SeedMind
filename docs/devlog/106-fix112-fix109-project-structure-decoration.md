# Devlog #106 — FIX-112 + FIX-109: project-structure.md Decoration 반영 및 economy-architecture.md 키명 확인

> 작성: Claude Code (Sonnet 4.6) | 2026-04-08

---

## 작업 요약

**FIX-112**: `project-structure.md`에 `SeedMind.Decoration` 네임스페이스, `Scripts/Decoration/` 폴더, `SeedMind.Decoration.asmdef` 추가 (ARC-043 후속)

**FIX-109**: `economy-architecture.md`에 `fishGatherVsCropMax` → `fishGatherVsCropProcessingMax` 키명 직접 기재 여부 확인 → 해당 파일에 직접 기재 없음 확인 (canonical 참조 구조 이미 준수)

---

## 변경 내용

### `docs/systems/project-structure.md`

#### 섹션 1 — 폴더 구조
- `Scripts/Decoration/` 폴더 신규 추가
  - `DecorationManager.cs` (Singleton, ISaveable, SaveLoadOrder=60)
  - `DecorationInstance.cs` (런타임 상태)
  - `DecorationEvents.cs` (정적 이벤트 허브)
  - `Data/` 하위: `DecorationItemData.cs`, `DecorationConfig.cs`, `DecorationSaveData.cs`, `DecorationInstanceSave.cs`, `DecoCategoryType.cs`, `EdgeDirection.cs`
- `Data/Decorations/` 에셋 폴더 추가 (SO_Deco_*.asset 29종)
- `Data/Config/` 설명에 `SO_DecorationConfig.asset` 추가

#### 섹션 2 — 네임스페이스
- `SeedMind.Decoration` 및 `SeedMind.Decoration.Data` 항목 추가

#### 섹션 4 — Assembly Definition
- `SeedMind.Decoration.asmdef` 행 추가 (`Scripts/Decoration/`, 참조: Core)

#### Cross-references
- `docs/systems/decoration-architecture.md` (ARC-043) 추가

---

## FIX-109 조사 결과

`economy-architecture.md`에서 `fishGatherVsCropMax`, `fishGatherVsCropProcessingMax` 등 해당 키명 직접 기재 없음.
canonical 파라미터는 `docs/systems/economy-system.md` 섹션 8.6에서만 관리되며, economy-architecture.md는 이를 참조하는 구조로 이미 PATTERN-001 준수.
**수정 불필요 — 확인 완료**.
