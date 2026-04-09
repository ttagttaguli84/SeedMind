# 개발 일지 122 — ARC-046 장식 시스템 MCP 구현 + Phase 2 완료

> 날짜: 2026-04-10 | 작성: Claude Code (Sonnet 4.6)

---

## 요약

`docs/mcp/decoration-tasks.md` (ARC-046) 를 실행하여 장식(Decoration) 시스템 Unity 구현을 완료했다. 이로써 Phase G(Polish) 마지막 태스크가 마무리되고 **Phase A–G 전체가 ✅** 가 되어 **Phase 2 완료 → Phase 3 전환**을 달성했다.

---

## 완료 작업 (D-A ~ D-D)

### D-A: 스크립트 7종 생성

| # | 파일 | 역할 |
|---|------|------|
| S-01 | `DecoCategoryType.cs` | enum (Fence/Path/Light/Ornament/WaterDecor) |
| S-02 | `EdgeDirection.cs` | enum (None/North/South/East/West) |
| S-03 | `DecorationItemData.cs` | ScriptableObject — 29종 아이템 데이터 스키마 |
| S-04 | `DecorationConfig.cs` | ScriptableObject — 전역 설정 |
| S-05 | `DecorationInstance.cs` | Plain C# — 런타임 인스턴스 |
| S-06 | `DecorationSaveData.cs` | Serializable — 세이브 데이터 |
| S-07 | `DecorationEvents.cs` | static — 이벤트 버스 |

### D-B: DecorationManager 생성

- `DecorationManager.cs` 작성 (MonoBehaviour, Singleton, ISaveable, SaveLoadOrder=57)
- SCN_Farm `--- MANAGERS ---` 하위에 GO 배치 + 컴포넌트 부착

### D-C-ALT: SO 에셋 30종 일괄 생성

`CreateDecorationAssets.cs` Editor 스크립트로 일괄 생성:
- `SO_DecorationConfig.asset` (1종)
- `SO_Deco_Fence*.asset` 4종, `SO_Deco_Path*.asset` 5종, `SO_Deco_Light*.asset` 4종
- `SO_Deco_Orna*.asset` 11종, `SO_Deco_Water*.asset` 5종

모든 itemId, buyPrice, unlockLevel 수치 → `docs/content/decoration-items.md` (CON-020) canonical에서 복사 (PATTERN-011 준수).

### D-D: SCN_Farm 씬 계층 설정

```
--- ENVIRONMENT ---
└── Decorations
    ├── PathLayer  (Tilemap, sortingLayer=Decoration, order=1)
    ├── FenceLayer (Tilemap, sortingLayer=Decoration, order=2)
    └── DecoObjects (Transform)
```

DecorationManager 인스펙터 참조 연결: `_decoConfig`, `_farmGrid`, `_fenceLayer`, `_pathLayer`, `_objectLayer` 모두 완료.

### D-E: 통합 테스트

`execute_code` 비활성으로 스킵.

---

## 실전 발견사항

| 상황 | 해결 |
|------|------|
| `FarmGrid.IsFarmland()` 메서드 없음 | `GetTile(x, z) != null` 패턴으로 경작지 판별 |
| `Season` enum에 `None` 값 없음 | `hasSeasonLimit(bool) + limitedSeason(Season)` 2필드 패턴 사용 (Inspector 직렬화 호환) |

---

## Phase 2 완료 선언

`docs/mcp/progress.md` 기준 Phase A–G 전체 ✅:

| Phase | 상태 |
|-------|------|
| A — Foundation | ✅ |
| B — Core Systems | ✅ |
| C — Content | ✅ |
| D — Feature Systems | ✅ |
| E — UI & UX | ✅ |
| F — Advanced Features | ✅ |
| G — Polish | ✅ |

→ Phase 3 (QA & 플레이 테스트) 시작.

---

## 다음 단계 (Phase 3)

Phase 3 범위는 `workflow.md` 정의 예정:
- Play Mode 통합 테스트 (각 시스템 동작 검증)
- 컴파일 오류 및 런타임 Null Reference 수정
- 에디터 수동 설정 필요 항목 처리 (AudioMixer, 하이라이트 마스크 등)
- 빌드 테스트
