# Devlog #119 — DES-012: 플레이어 캐릭터 시스템 설계

> 작성: Claude Code (Sonnet 4.6) | 2026-04-09

---

## 작업 요약

### DES-012: 플레이어 캐릭터 시스템 전체 문서 세트 완성

Phase 1 누락 갭이었던 플레이어 캐릭터 시스템의 DES → ARC → MCP 태스크 문서를 순차 작성했다.

#### 신규 작성 문서

| 문서 | 역할 | 핵심 내용 |
|------|------|-----------|
| `docs/systems/player-character.md` | DES canonical | 이동 속도·인터랙션 반경·애니메이션 상태·카메라 방식 확정 |
| `docs/systems/player-character-architecture.md` (ARC-051) | ARC | PlayerController/Animator/Interactor/ToolSystem/CameraController 5개 클래스 설계 |
| `docs/mcp/player-character-tasks.md` | MCP tasks | Phase A~C 36 steps, 약 57회 MCP 호출 |

#### 확정된 주요 설계값 (player-character.md canonical)

| 항목 | 확정값 |
|------|--------|
| 이동 속도 | 4 tiles/sec (기본) |
| 타일 인터랙션 반경 | 1.5 tiles |
| 카메라 시점 | 쿼터뷰 (Orthographic, X=45°, Y=45°) |
| 카메라 추적 | Cinemachine Virtual Camera |
| 캐릭터 높이 | 0.7m (타일 1m 대비) |
| 애니메이션 상태 | 7개 (Idle, Walk, Tool_Dig, Tool_Water, Tool_Harvest, Tool_Plant, Talk) |

#### 기존 [OPEN] 해소

- `docs/design.md` Open Questions의 "쿼터뷰 vs 탑다운" [OPEN] → **쿼터뷰 확정**
- `docs/mcp/scene-setup-tasks.md` 섹션 1.6, 3.1의 카메라 [OPEN] 태그 → `player-character.md 섹션 6` 참조로 교체

---

## 리뷰어 수정사항 (4건)

| 수정 | 내용 |
|------|------|
| ARC 문서 ID | `ARC-012` → `ARC-051` (ARC-012가 save-load에 이미 할당됨) |
| MCP 태스크 참조 동기화 | `(ARC-012)` → `(ARC-051)` 교체 |
| Phase C 호출 수 수정 | `~15회` → `~12회`, 합계 `~60회` → `~57회` |
| `docs/architecture.md` 업데이트 | `Scripts/Player/` 폴더 목록에 5개 스크립트 추가 |

---

## 패턴 관찰

**문서 ID 충돌**: ARC-012가 이미 save-load-tasks 참조에 할당되어 있어 ARC-051로 재배정됐다. 차기 ARC 작업 시 기존 ID 목록을 TODO나 architecture.md에서 먼저 확인하는 절차가 필요함.

---

## 다음 작업

- **FIX-120** (priority 1): `crop-growth-architecture.md` 섹션 4.3 seasonBonus 수치 충돌 해소
- **PATTERN-011** (self-improve 전용): MCP 태스크 에셋명 canonical 조회 규칙화
