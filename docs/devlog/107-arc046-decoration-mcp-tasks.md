# Devlog #107 — ARC-046: 장식 시스템 MCP 태스크 시퀀스 문서화

> 작성: Claude Code (Sonnet 4.6) | 2026-04-08

---

## 작업 요약

**ARC-046**: `docs/mcp/decoration-tasks.md` 신규 생성 — 장식(Decoration) 시스템의 Unity MCP 구현 태스크 시퀀스 문서화 (DES-023 → ARC-043 → FIX-111/112/113 후속 최종 단계)

---

## 변경 내용

### 신규: `docs/mcp/decoration-tasks.md` (ARC-046)

**문서 구조**:
- Context: 상위 설계 문서(ARC-043) 및 패턴 참조(ARC-032/028) 명시
- 섹션 1: 개요 — 의존성, 이미 존재하는 오브젝트(중복 생성 금지), 총 MCP 호출 예상수
- 섹션 2: MCP 도구 매핑 테이블 (13개 MCP 명령 용도 분류)
- 섹션 3: 필요 C# 스크립트 목록 (S-01~S-08, 8개 파일)
- 섹션 4: 태스크 목록 (D-A~D-E 5개 그룹)
- 섹션 5: Cross-references (12개 문서)

**태스크 그룹 요약**:

| 그룹 | 내용 | 예상 MCP 호출 |
|------|------|:------------:|
| D-A | enum 2종 + SO 클래스 2종 + Plain C# 3종 (7 스크립트) | ~10회 |
| D-B | DecorationManager MonoBehaviour + 씬 배치 | ~8회 |
| D-C | DecorationItemData 29종 + DecorationConfig 1종 SO 에셋 | ~65회 |
| D-D | PathLayer/FenceLayer Tilemap + DecoObjects Transform + 참조 연결 | ~14회 |
| D-E | 플레이모드 배치 테스트 + 저장-로드 검증 + 제약 검사 단위 테스트 | ~8회 |
| **합계** | | **~105회** |

**PATTERN 준수 사항**:
- PATTERN-007: D-C 전 카테고리에서 buyPrice/unlockLevel/lightRadius 등 콘텐츠 수치 직접 기재 없음. `→ see decoration-system.md 섹션 2.X`로만 표기
- PATTERN-005: DecorationSaveData(2필드) / DecorationInstanceSave(7필드) JSON↔C# 일치 주석 포함
- 이미 완료된 FIX-111/112/113: "이미 존재하는 오브젝트" 섹션에 명시, 중복 태스크 없음

**리뷰 후 수정 사항** (CRITICAL 4건 + WARNING 3건):
1. [CRITICAL] FenceBrick → FenceFloral (canonical decoration-system.md 섹션 2.1 기준)
2. [CRITICAL] PathCobble/PathFlower → PathDirt/PathStone (canonical 섹션 2.2 기준)
3. [CRITICAL] LightFairy → LightStreet (canonical 섹션 2.3 기준)
4. [CRITICAL] DecoConfig 에셋 경로: `Data/Decoration/SO_DecoConfig.asset` → `Data/Config/SO_DecorationConfig.asset` (decoration-architecture.md 섹션 2.3 / data-pipeline.md 섹션 2.15 canonical)
5. [WARNING] `durability(float)` → `durability(int)` (DecorationInstance + DecorationInstanceSave 두 곳)
6. [WARNING] `itemId(int)` → `itemId(string)` (DecorationInstanceSave)
7. [WARNING] D-B-04 "이미 완료됨" 표기 → NOTE 블록으로 분리, SaveLoadOrder 설정 태스크 명칭 명확화
8. [WARNING] SaveLoadOrder canonical 참조 → decoration-architecture.md → save-load-architecture.md SaveLoadOrder 할당표

---

## 리뷰어 반복 패턴 감지

동일한 아이템 ID 불일치가 Fence/Path/Light 3개 카테고리에서 발생 (3건 이상).
→ "MCP 태스크 문서 예시 에셋명이 canonical 콘텐츠 문서 영문 ID와 불일치" 패턴 감지됨.
→ TODO.md에 PATTERN-011 등재 예정 (self-improve 대상, 임계값 3건 충족).

---

## 완료 상태

- ARC-046 DONE
- 장식 시스템 전체 체인 완료: DES-023 → ARC-043 → FIX-111/112/113 → ARC-046
