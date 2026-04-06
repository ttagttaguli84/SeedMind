# Devlog #006 — MCP 기본 씬 구성 태스크 시퀀스 (ARC-002)

> 2026-04-06 | Phase 1 | 작성: Claude Code (Opus)

---

## 오늘 한 일

### ARC-002: MCP 기본 씬 구성 태스크 시퀀스 (`docs/mcp/scene-setup-tasks.md`)

Designer + Architect 에이전트를 병렬 실행하여 문서 작성.

**Part I — 게임 디자인 관점**:
- **씬 구성 요소 목록**: MANAGERS/FARM/PLAYER/ENVIRONMENT/ECONOMY/CAMERA/UI 7개 카테고리, 30+ 오브젝트 정의
- **초기 플레이 경험**: 첫 화면 연출, HUD 초기값, 첫 플레이 동선(08:00 시작 → 호미 → 씨앗 → 물주기 → 3일 후 수확)
- **시각적 요구사항**: 카메라(Orthographic, 45도 쿼터뷰, Size 6), 라이팅(봄 Morning 프리셋), 지형 룩앤필
- **테스트 씬**: SCN_Test_FarmGrid 최소 구성, 검증 시나리오 8건

**Part II — 기술 아키텍처 관점**:
- **5개 Phase 구조**: A(프로젝트 초기화 51회) → B(SCN_Farm ~60회) → C(MainMenu/Loading ~35회) → D(Test ~22회) → E(검증 ~10회)
- **총 175회 MCP 호출**, 예상 15~22분
- **의존성 그래프**: B~D는 A 완료 후 병렬 실행 가능, E는 전체 완료 후
- **7건 RISK**: URP 할당, RenderSettings, Build Settings 등 MCP 도구 가용성 불확실 항목

### 리뷰 및 수정

Reviewer 에이전트가 **CRITICAL 2건, WARNING 6건, INFO 5건** 발견, 전부 수정:

**CRITICAL**:
1. `scene-setup-tasks.md` Buildings 부모 불일치 (섹션 1.2 "FarmSystem 하위" vs Step B-4 "--- FARM --- 하위") → `project-structure.md` canonical 기준(`--- FARM ---` 하위)으로 통일
2. `scene-setup-tasks.md` 게임 시작 시각 불일치 (섹션 1.7 "06:00" vs 나머지 "08:00") → 08:00으로 통일

**WARNING**:
1. `architecture.md` Cross-references에 scene-setup-tasks.md 링크 추가
2. `project-structure.md` Cross-references "작성 예정" 표기 제거
3. `scene-setup-tasks.md` 출하함 5% 수수료 — canonical source(economy-system.md)에 없는 정보 제거, 참조 섹션 번호 수정 (3.2→4.1)
4. `scene-setup-tasks.md` 카메라 Y Rotation 확정/미정 모순 → [OPEN] 태그 추가
5. `project-structure.md` 폴더 트리에 Resources/ 누락 → 추가
6. `time-season-architecture.md` Dawn 시간 표기 "07:59" → "08:00 미만"으로 명확화

**INFO**:
1. Morning Sun Color #FFF4E0 → canonical #FFFAED로 수정
2. EconomyManager를 MANAGERS 섹션에서 제거, ECONOMY 섹션으로 이동
3. Cross-references economy-system.md 섹션 번호 오기재 수정
4. Phase D에 TestPlayer 생성 단계 추가
5. Phase B-9에 기본 카메라 존재 불확실성 RISK 태그 추가

### PATTERN-004 신규 등록

같은 문서 내 디자인 섹션과 MCP 구현 섹션 간 불일치 패턴 4건 발견. TODO.md에 PATTERN-004로 등록.

---

## 의사결정 기록

1. **Part I + Part II 통합 문서**: 게임 디자인("무엇이 왜")과 기술 구현("어떻게")을 하나의 문서에 배치. 두 관점이 같은 씬을 다루므로 분리하면 Cross-reference 비용이 증가.
2. **Phase B~D 병렬 구조**: SCN_Farm(B), MainMenu/Loading(C), Test(D)는 A 완료 후 병렬 진행 가능하도록 설계. 총 소요 시간 단축.
3. **MCP 도구 사전 검증 필수화**: 7건의 RISK가 모두 MCP 도구 가용성에 관한 것. Phase A 시작 전 도구 목록 검증을 필수 단계로 포함.
4. **ShippingBin 추가**: project-structure.md 씬 계층에 출하함 오브젝트가 누락되어 있었음. 추가 완료.

---

## 다음 단계

- ARC-003: MCP 작업 계획 — 농장 그리드 생성 태스크 시퀀스 (Priority 4)
- ARC-004: 데이터 파이프라인 설계 (Priority 3)
- BAL-001: 작물 경제 밸런스 시트 (Priority 3)

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
