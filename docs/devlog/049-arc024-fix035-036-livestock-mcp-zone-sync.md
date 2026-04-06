# Devlog #049 — ARC-024 + FIX-035/036: 목축 MCP 태스크 시퀀스 + Zone 확장 수치 동기화

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

지난 세션(#048)에서 FIX-038~041로 목축 아키텍처를 정합화한 후, 이번 세션에서는 3개 작업을 완수했다:

1. **FIX-035/036**: DES-012(농장 확장 Zone 방식) 확정에 따른 수치 동기화
2. **ARC-024**: 목축 MCP 태스크 시퀀스 문서 신규 생성
3. **리뷰 후속 수정**: CRITICAL 2건 + WARNING 3건 처리

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/balance/progression-curve.md` | FIX-035: 섹션 1.2.4 "4단계 100XP" → "6단계 150XP", 보조 마일스톤 "4단계" → "6단계 Zone G", 섹션 3.1 "농장 확장 4단계(최종)" → Zone E 방식 수정 |
| `docs/systems/economy-system.md` | FIX-036: 섹션 3.3 목공소 인벤토리 4x8 방식→Zone B~G 6단계 방식, 섹션 5.2 비용 표기 Zone 참조로 교체 |
| `docs/systems/farming-system.md` | FIX-036 연동: 섹션 1 확장 방식 및 비용 Zone 참조로 교체 |
| `docs/systems/livestock-architecture.md` | CRITICAL-1 수정: OnProductCollected 이벤트 역할 분리(인스턴스=UI용/정적=XP용) 주석 추가 |
| `docs/mcp/livestock-tasks.md` | ARC-024: 신규 생성 (9개 태스크 그룹 L-1~L-9, ~221회 MCP 호출) |
| `docs/mcp/livestock-tasks.md` | WARNING-2/3 수정: MCP 호출 수 테이블 실측값으로 갱신, Cross-references 문서 ID 수정 |

---

## FIX-035 — 농장 확장 XP 6단계로 업데이트

**문제**: DES-012에서 농장 확장이 4x8 블록 4단계 → Zone B~G 6단계 방식으로 재설계되었으나, progression-curve.md가 구 방식(4단계 = 총 100 XP)을 유지했다.

**수정 내용**:
- 섹션 1.2.4: `4단계 = 총 100 XP` → `6단계 = 총 150 XP (→ see farm-expansion.md 섹션 2.1)`
- 보조 마일스톤 테이블: "농장 확장 완료 (4단계)" → "농장 확장 완료 (6단계, Zone G)", 예상 시점 Day 140~150 → Day 160~200
- 섹션 3.1 자금 시뮬레이션: "농장 확장 4단계(최종): -4,000G" → "Zone E 해금 (목장 구역): -4,000G" 명시

---

## FIX-036 — 목공소 인벤토리 Zone 방식으로 업데이트

**문제**: economy-system.md 섹션 3.3 목공소 인벤토리에 "농장 확장 1단계(4x8) 500G ~ 4단계 4,000G" 구버전 4행이 잔존. farming-system.md 섹션 1도 16x16/4x8 방식 유지.

**수정 내용**:
- `economy-system.md` 섹션 3.3: Zone B~G 6행 테이블로 교체, canonical 참조(`→ see farm-expansion.md 섹션 2.1`) 표기
- `economy-system.md` 섹션 5.2: "단계별 2배 증가 (500G~4,000G)" → "Zone B~G 6단계 총 16,000G" 참조
- `farming-system.md` 섹션 1: 최대 크기 576타일/Zone 방식 참조, 확장 비용 canonical 참조로 교체

---

## ARC-024 — 목축/낙농 MCP 태스크 시퀀스

새 파일 `docs/mcp/livestock-tasks.md` 생성.

**태스크 그룹 구성**:

| 그룹 | 내용 | MCP 호출 수 |
|------|------|------------|
| L-1 | 스크립트 생성 (enum/struct/SO/MonoBehaviour) | 18회 |
| L-2 | AnimalData SO 4종 + LivestockConfig SO 에셋 생성 | 65회 |
| L-3 | FeedData 사료 아이템 SO 4종 등록 | 28회 |
| L-4 | 씬 배치 (AnimalManager 배치, 참조 연결) | 6회 |
| L-5 | 기존 시스템 확장 (XPSource, HarvestOrigin, GameSaveData) | 6회 |
| L-6 | Zone E 연동 (외양간/닭장 건설 핸들러 검증) | 8회 |
| L-7 | UI 생성 (동물 구매 팝업, AnimalSlot 프리팹, 돌봄 패널) | 53회 |
| L-8 | 이벤트 연동 (ProgressionManager, UI, NPC 구독) | 4회 |
| L-9 | 통합 테스트 시퀀스 | 33회 |
| **합계** | | **~221회** |

**최적화 여지**: L-2(65회) + L-3(28회)를 CreateAnimalAssets.cs Editor 스크립트로 일괄 생성 시 ~16회로 감소 가능.

---

## 리뷰어 수정 사항

| ID | 심각도 | 파일 | 수정 내용 |
|----|--------|------|-----------|
| CRITICAL-1 | 🔴 | livestock-architecture.md | AnimalManager 인스턴스 `OnProductCollected<AnimalInstance, ItemData, int>`(UI용)과 LivestockEvents 정적 `OnProductCollected<AnimalInstance, int>`(XP/퀘스트용) 역할 분리 주석 추가 |
| CRITICAL-2 | 🔴 | progression-curve.md | 섹션 3.1 "농장 확장 4단계(최종)" → "Zone E 해금" 수정 (FIX-035 연동) |
| WARNING-2 | 🟡 | livestock-tasks.md | 섹션 1.5 MCP 호출 수 예상 테이블을 실측값(~221회)으로 갱신 |
| WARNING-3 | 🟡 | livestock-tasks.md | Cross-references의 progression-architecture.md 문서 ID "BAL-002" 제거 (BAL-002는 progression-curve.md ID) |
| WARNING-5 | 🟡 | economy-system.md | 목공소 Zone 테이블 첫 줄에 "→ copied from farm-expansion.md, 변경 시 동시 수정 필요" 명시 강화 |

---

## 신규 TODO 항목

리뷰 과정에서 5개 신규 항목 식별:

| ID | Priority | 내용 |
|----|----------|------|
| FIX-042 | 2 | progression-curve.md 섹션 2.2 DEPRECATED 배너 추가 |
| FIX-043 | 1 | livestock-architecture.md ClearAllAnimals() 테스트 메서드 추가 |
| ARC-025 | 1 | 농장 확장 Zone MCP 태스크 시퀀스 문서화 |
| DES-013 | 1 | 낚시 시스템 설계 (Zone F 연못 활용) |
| BAL-011 | 1 | 목축 XP progression-curve.md canonical 등록 |

---

## 세션 후 TODO 상태

| ID | Priority | 상태 |
|----|----------|------|
| FIX-035 | 2 | ✅ DONE |
| FIX-036 | 2 | ✅ DONE |
| ARC-024 | 1 | ✅ DONE |
| BAL-008 | 1 | 잔여 |
| BAL-009 | 1 | 잔여 |
| AUD-001 | 1 | 잔여 |
| BAL-005 | 1 | 잔여 |
| CON-008 | 1 | 잔여 |
| FIX-042 | 2 | 신규 |
| FIX-043 | 1 | 신규 |
| ARC-025 | 1 | 신규 |
| DES-013 | 1 | 신규 |
| BAL-011 | 1 | 신규 |

---

*이 문서는 Claude Code가 ARC-024 + FIX-035/036 태스크에 따라 자율적으로 작성했습니다.*
