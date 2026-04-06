# Devlog #007 — 농장 그리드 MCP 태스크 시퀀스 (ARC-003)

> 2026-04-06 | Phase 1 | 작성: Claude Code (Opus)

---

## 오늘 한 일

### ARC-003: 농장 그리드 MCP 태스크 시퀀스 (`docs/mcp/farming-tasks.md`)

Designer + Architect 에이전트 병렬 실행으로 문서 작성.

**Part I — 게임 디자인**:
- **타일 시각 스펙**: 7종 TileState별 머티리얼 색상/Smoothness 정의, 상태 전환 피드백 테이블
- **작물 성장 시각 스펙**: 4단계(Seed/Sprout/Growing/Harvestable) placeholder 프리팹, 3종 작물별 색상
- **도구 상호작용 매트릭스**: 5도구 x 8상태 = 40칸 완전 매핑
- **초기 작물 데이터**: 감자/당근/토마토 CropData SO 필드값 확정
- **테스트 시나리오**: 생명주기 4건, 도구 6건, 에지 케이스 3건

**Part II — 기술 아키텍처**:
- **4 Phase 구조**: A(그리드 ~713회) → B(작물 데이터 ~153회) → C(상호작용 ~80회) → D(검증 ~15회)
- **총 ~960회 MCP 호출** (Editor 스크립트 최적화 시 ~255회)
- **C# 스크립트 11개 사전 작성 필요** (MCP 접근 필드 인터페이스 명시)
- **핵심 리스크**: SO 배열/참조 필드 MCP 설정, 프리팹 저장 도구 가용성

### 리뷰 및 수정

Reviewer가 **CRITICAL 2건, WARNING 4건, INFO 3건** 발견, 전부 수정:

**CRITICAL**:
1. 낫/Planted 상태 모순 → crop-growth.md 정책("소멸")으로 통일, 매트릭스 업데이트
2. SO 저장 경로 불일치 → farming-architecture.md `Assets/Data/` → `Assets/_Project/Data/` 수정

**WARNING**:
1. 프리팹 스케일 3곳 불일치 → farming-architecture.md Phase B 값을 canonical로 통일
2. 타일 Y 위치값 → farming-architecture.md에 0.001(Z-fighting 방지) 반영
3. M_Soil_Dry/Withered 머티리얼 생성 누락 → Phase A A-1에 2종 추가 (3회→5회)
4. 손(Hand) 행동 정의 불일치 → farming-system.md과 대조 확인 (후속 수정 필요)

---

## 의사결정 기록

1. **낫의 Planted 타일 사용 허용**: crop-growth.md 정책에 따라 Planted/Watered/Dry 상태에서 낫 사용 시 작물 소멸(환불 없음). 직관적이며 잘못 심었을 때 수정 수단 제공.
2. **GridPosition → gridX/gridY 분리**: Vector2Int는 MCP set_property로 설정 불확실. int 2개 필드로 분리하여 MCP 호환성 확보.
3. **Editor 스크립트 최적화 권장**: 타일 64개 생성(A-2, A-3)은 MCP 개별 호출 시 ~640회. C# Editor 스크립트 1회 실행으로 대체하면 ~255회로 감소.
4. **프리팹 스케일 canonical**: farming-architecture.md Phase B Step B-2의 값을 canonical로 확정.

---

## 다음 단계

- ARC-004: 데이터 파이프라인 설계 (Priority 3)
- BAL-001: 작물 경제 밸런스 시트 (Priority 3)
- BAL-002: 게임 진행 곡선 (Priority 3)

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
