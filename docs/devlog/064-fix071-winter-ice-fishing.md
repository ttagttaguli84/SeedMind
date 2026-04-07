# Devlog #064 — FIX-071: 겨울 얼음 낚시 허용 확정

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

FIX-071(겨울 낚시 허용 여부 결정)을 처리했다. `fishing-system.md` 섹션 10에 이미 완성된 얼음 낚시 설계를 기반으로, `time-season.md`의 "낚시/채집 불가" 규칙을 "채집 불가 (낚시는 얼음 낚시로 가능)"으로 변경하고, 관련 아키텍처를 확장했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/systems/time-season.md` | 섹션 2.2 겨울 고유 메커닉 규칙 변경, [OPEN] 항목 5 → [RESOLVED], Risks 부분 완화 기록 |
| `docs/systems/fishing-system.md` | 섹션 4.3/10.1/11 [OPEN] → [RESOLVED], cross-reference 보강 |
| `docs/systems/fishing-architecture.md` | 섹션 8A 신규 추가 (얼음 낚시 아키텍처), FishingSaveData activeIceHoles 필드 추가, MCP 태스크 Phase D2 추가 |
| `docs/systems/time-season-architecture.md` | 섹션 4.3~4.4 FishingManager 구독자 추가, cross-reference 역방향 참조 추가 |

---

## FIX-071 — 겨울 낚시 허용 확정

### 결정 근거

| 항목 | 내용 |
|------|------|
| 설계 기반 | fishing-system.md 섹션 10에 얼음 낚시 메카닉이 이미 설계 완료 |
| 겨울 어종 | 빙어 (Common, 18G), 얼음 빙어왕 (Rare, 180G) — 2종 한정 |
| 게임 밸런스 | 어종 2종 한정으로 경제 영향 최소화; 수익성보다 활동 다양성 제공 목적 |
| 문제 해소 | time-season.md [OPEN] 항목 5 "겨울 28일 야외 활동 부족" 부분 해소 |
| 특수 메카닉 | 곡괭이 → 얼음 구멍 뚫기 → 낚시; 최대 3개 구멍 동시 유지 |

### time-season.md 변경

```
[이전] 겨울 고유 메커닉: 야외 경작 불가, 낚시/채집 불가
[이후] 겨울 고유 메커닉: 야외 경작 불가, 채집 불가 (낚시는 얼음 낚시로 가능)
```

겨울 낚시는 "제한적 허용" — 채집은 여전히 불가, 낚시만 얼음 낚시 형태로 가능.

---

## 아키텍처 확장 (fishing-architecture.md 섹션 8A)

### 신규 추가된 구성 요소

**IceHoleData** (Plain C# 데이터 클래스):
- `tilePosition: Vector2Int`
- `createdDay: int`
- `createdSeason: Season` (겨울=3)

**FishingManager 신규 메서드**:
- `CreateIceHole(Vector2Int)` — 에너지 소모, iceHoleMax 초과 시 거부
- `GetActiveHoleCount(): int`
- `RemoveExpiredHoles()` — OnDayChanged에서 호출 (iceHoleDuration 경과 구멍 제거)
- `RemoveAllIceHoles()` — OnSeasonChanged에서 겨울 종료 시 전체 제거
- `IsIceHole(Vector2Int): bool` — TryStartFishing 진입점에서 계절 체크

**SeasonManager 연동**:
- `time-season-architecture.md` 섹션 4.3: FishingManager priority 55로 OnDayChanged 구독 (일일 만료 구멍 제거)
- `time-season-architecture.md` 섹션 4.4: FishingManager priority 55로 OnSeasonChanged 구독 (겨울 시작 시 결빙 VFX, 겨울 종료 시 전체 구멍 제거)

### FishingSaveData 확장

```csharp
activeIceHoles: List<IceHoleData>  // 겨울 얼음 구멍 상태 저장
// PATTERN-005: JSON 10개 ↔ C# 10개 (activeIceHoles 포함)
```

---

## 리뷰어 검증 결과

| ID | 심각도 | 이슈 내용 | 수정 결과 |
|----|--------|-----------|-----------|
| R-01 | 🟡 WARNING | time-season-architecture.md cross-reference에 fishing-architecture.md 역방향 참조 누락 | 직접 추가 완료 |

나머지 검토 항목 6개 전부 통과.

---

## 세션 후 활성 TODO

| ID | Priority | 상태 |
|----|----------|------|
| CON-009 | 2 | 잔여 (치즈 공방 레시피 — BAL-008 선행) |
| DES-014 | 2 | 잔여 (겨울 씨앗 판매 경로) |
| CON-010 | 2 | 잔여 (낚시 업적/퀘스트 — FIX-071 완료로 블로커 해소됨) |
| BAL-014 | 1 | 잔여 (낚시 숙련도 XP 밸런스) |
| DES-015 | 1 | 잔여 (낚싯대 업그레이드 재료 공급 경로) |
| CON-011 | 1 | 잔여 (낚시 도감 콘텐츠 — FIX-071 완료로 블로커 해소됨) |
| FIX-072 | 1 | 잔여 (economy-system.md 낚시 수입 항목 추가) |
| ARC-030 | 1 | 잔여 (낚시 도감 아키텍처) |
| DES-016 | 1 | 잔여 (채집 시스템 기본 설계) |
| BAL-008 | 1 | 잔여 (목축/낙농 경제 밸런스) |
| PATTERN-009/010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 FIX-071 태스크에 따라 자율적으로 작성했습니다.*
