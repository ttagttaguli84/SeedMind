# Devlog #023 — FIX-007: ToolSpecialEffect [Flags] enum 적용

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### FIX-007: ToolSpecialEffect enum 다중 효과 처리 방안 확정

전설 낫(Legendary Sickle)이 보너스 수확 + 품질 상승 + 씨앗 회수 3가지 효과를 동시에 가지는데, 기존 `ToolSpecialEffect` enum은 단일 값만 표현 가능하여 이를 처리할 수 없었다. `[System.Flags]` 비트마스크 방식을 채택하여 해결.

**수정된 문서**:
- `docs/systems/tool-upgrade-architecture.md` — 섹션 3.5 enum 재정의, ToolData 필드 타입 변경, GetSpecialEffect 메서드 단순화, Phase B 에셋명 통일, 섹션 9.3 씬 배치 위치 수정, Open Questions RESOLVED 추가
- `docs/mcp/tool-upgrade-tasks.md` — T-1-02~T-1-09 specialEffect 값 enum 표기 통일, T-05 스텁 구현 방식 명시, Open Questions RESOLVED

---

## 핵심 설계 결정

### [System.Flags] 비트마스크 채택

```csharp
[System.Flags]
public enum ToolSpecialEffect
{
    None          = 0,
    AreaEffect    = 1 << 0,   // 범위 효과
    ChargeAttack  = 1 << 1,   // 충전 사용
    AutoWater     = 1 << 2,   // 자동 물주기
    QualityBoost  = 1 << 3,   // 품질 상승
    DoubleHarvest = 1 << 4,   // 이중 수확
    SeedRecovery  = 1 << 5,   // 씨앗 회수 (신규)
}
```

**채택 근거**:
- 하나의 `ToolSpecialEffect` 필드에 `|` 연산으로 복합 효과 표현 가능
- Unity Inspector에서 `[Flags]` enum은 멀티셀렉트 체크박스로 자동 표시 — 에디터 워크플로우 유지
- 소비자 코드는 `HasFlag(ToolSpecialEffect.DoubleHarvest)` 패턴으로 개별 효과 검사
- `ToolData.specialEffect` 필드 타입: `string` → `ToolSpecialEffect` (문자열 파싱 로직 제거)

**전설 낫 적용**:
`specialEffect = DoubleHarvest | QualityBoost | SeedRecovery` (비트값 = 56 = `0b111000`)

**기각된 대안**:
- `string[]` 방식: SO Inspector에서 배열 편집 불편, 파싱 비용
- 등급 분기(tier-based branching): ToolEffectResolver에 도구별 하드코딩 필요, 확장성 낮음

---

## 리뷰 결과

**CRITICAL 2건 (수정 완료)**:
1. [C-1] T-1-02~T-1-09 SO 블록에 문자열 리터럴(`""`, `"AreaEffect"` 등) 잔존 → 따옴표 제거, enum 이름 표기로 통일
2. [C-2] tool-upgrade-architecture.md Open Questions에 FIX-007 RESOLVED 기록 누락 → 추가

**WARNING 4건 (수정 완료)**:
1. [W-1] T-05 GetSpecialEffect 스텁 구현 방식 불명확 → flags enum 직접 반환 주석 명시
2. [W-2] `newTier` 주석 범위 오류 `(1~5)` → `(1~3)` 수정
3. [W-3] Phase B 물뿌리개 SO 에셋명 `SO_Tool_Water_*` → `SO_Tool_WateringCan_*` 통일
4. [W-4] ToolUpgradeSystem 씬 배치 위치 불일치 — `--- MANAGERS ---` 하위로 통일 (MCP 태스크 문서 T-4-01이 canonical)

---

## 미결 사항 ([OPEN])

- 대장간 ShopData SO 필요 여부 (재료 구매 탭 — ShopSystem 재사용 vs. 직접 구현)
- 재료 드롭 경로 추가 여부 (잡초 제거/돌 파괴 → 철 조각 드롭) — 밸런스 재조정 필요

---

## 후속 작업

- `ARC-009`: NPC/상점 MCP 태스크 시퀀스 독립 문서화
- `ARC-012`: 세이브/로드 MCP 태스크 시퀀스 독립 문서화
- `ARC-013`: 인벤토리 MCP 태스크 시퀀스 독립 문서화
- `ARC-016`: 퀘스트 MCP 태스크 시퀀스 독립 문서화
- `CON-004`: 대장간 NPC 상세 (캐릭터/대화/업그레이드 인터페이스 UX)
- `DES-010`: 도전 과제/업적 시스템 설계

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
