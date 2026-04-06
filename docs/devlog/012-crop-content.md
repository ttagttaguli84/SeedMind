# Devlog #012 — 작물 콘텐츠 상세 (CON-001)

> 2026-04-06 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### CON-001: 작물 콘텐츠 상세

Designer + Architect 병렬 실행으로 2개 문서 신규 작성. Reviewer가 CRITICAL 4건·WARNING 4건 발견 후 전부 수정.

**신규 문서**:
1. `docs/content/crops.md` — 전체 작물 콘텐츠 상세 (Designer)
2. `docs/mcp/crop-content-tasks.md` — 작물 MCP 구현 계획 (Architect)

**수정된 문서**: `docs/design.md` (섹션 4.2 씨앗가 열 추가 + 겨울 작물 3종 + 고구마 [OPEN] 처리, 섹션 4.3 고구마 언급 정정)

### 핵심 설계 내용

**기존 작물 8종 카탈로그 완성**:
- 감자, 당근, 토마토, 옥수수, 딸기, 호박, 해바라기, 수박
- 각각 영문 ID, 게임 내 설명 텍스트, 로우폴리 시각 묘사, 특수 메카닉, 수확물 설명 포함

**신규 겨울/온실 전용 작물 3종 `[NEW]`**:
- **겨울무** (`crop_winter_radish`): 온실 전용, 레벨 5 해금, 단일 수확
- **표고버섯** (`crop_shiitake`): 온실 전용, 레벨 6 해금, 다중 수확 (재수확 4일)
- **시금치** (`crop_spinach`): 온실 전용, 레벨 7 해금, 고단가 단일 수확

**특수 메카닉 확정**:
- 다중 수확: 딸기(봄) + 표고버섯(온실) 2종
- 씨앗 드롭: 해바라기 확정 (dropRate 0.50, 1~2개). 옥수수는 [OPEN] 유지
- 점보 성장: 호박/수박, 조건 3종(연속 물주기 5일 + 비료 Silver+ + 맑음) 필요, **9타일(3x3)**
- 온실 전용: 겨울 작물 3종 모두 requiresGreenhouse = true

**MCP 구현 계획** (5개 Phase):
- Phase A: CropData.cs 스크립트 생성
- Phase B: SO 에셋 생성 (8종 + 겨울 3종 = 11종)
- Phase C: 프리팹 구조 생성 (단계별 + Giant 3x3)
- Phase D: 머티리얼 생성 및 연결
- Phase E: DataRegistry 등록 확인

### 리뷰 결과

**CRITICAL 4건 (수정 완료)**:
1. [C-1] Giant Crop 크기 2x2/4타일 → 3x3/9타일 수정 (crop-growth.md canonical)
2. [C-2] PATTERN-005 필드 수 테이블 "icon" 중복 계산 오류 → 기본 필드에서 icon 제거 (count 12 유지)
3. [C-3] MaxStackSize 참조 주석이 존재하지 않는 섹션(2.7) 지시 → inventory-architecture.md 섹션 4.1로 수정
4. [C-4] design.md 섹션 4.2에 씨앗 구매가 열 누락 → 씨앗가 열 + 재배 계절 열 추가

**WARNING 4건 (수정 완료)**:
1. [W-1] design.md 4.2에 겨울 작물 3종 미반영 → 추가 (수치는 crops.md 참조 표기)
2. [W-2] farming-tasks.md Cross-references 누락 → 실제 확인 시 이미 포함되어 있었음 (false alarm)
3. [W-3] [OPEN] 겨울 작물 수 태그 → crops.md 확정 후 태그 제거, 구체적 파일명 기재
4. [W-4] design.md 4.3 "고구마" 언급 → "당근"으로 수정, 고구마는 [OPEN] 향후 확장 후보로 표기

---

## 의사결정 기록

1. **겨울 작물 온실 전용 정책**: 겨울 3종은 다른 계절 야외에서도 재배 불가. 온실의 게임 내 가치를 높이고 겨울 콘텐츠 공백을 메우기 위한 설계. 다른 계절 작물도 온실에서 재배 가능하되 계절 보정 1.0 고정.

2. **표고버섯 다중 수확 채택**: 딸기(봄 전용)의 다중 수확이 강력한 수익원임을 감안, 겨울에도 동일 메카닉을 제공해 계절 간 플레이 균형 확보.

3. **Giant Crop 9타일(3x3) 확정**: crop-growth.md canonical이 3x3으로 정의. 아키텍처 초안이 2x2로 잘못 설계해 C-1로 수정. 시각적 임팩트와 그리드 시스템 부담의 균형점.

4. **씨앗가를 design.md canonical로 통합**: 기존에는 씨앗가가 balance/crop-economy.md에만 있었고, design.md 4.2에는 씨앗가 열이 없었다. doc-standards.md canonical 매핑과 일치시키기 위해 design.md에 씨앗가 열 추가(C-4).

---

## 미결 사항 ([OPEN])

- **옥수수 씨앗 드롭 여부**: crops.md 4.2에 [OPEN] 유지. CON-002(시설) 설계 시 가공소 연계 여부와 함께 결정 예정.
- **고구마**: 가을 작물 확장 후보. 현재 스코프에는 미포함.
- **여름/가을 다중 수확 작물 부재**: 딸기(봄)와 표고버섯(겨울)만 다중 수확 가능. 여름/가을 균형 보완 필요 여부 논의 필요.

---

## 후속 작업 필요

- `docs/balance/crop-economy.md`: 겨울 작물 3종 ROI/밸런스 분석 추가 → BAL-003로 TODO 등록

---

## 다음 단계

- BAL-003: 겨울 작물 밸런스 분석 (Priority 2)
- CON-002: 시설 콘텐츠 상세 (Priority 2)
- CON-003: NPC/상점 콘텐츠 (Priority 2)

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
