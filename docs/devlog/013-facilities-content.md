# Devlog #013 — 시설 콘텐츠 상세 (CON-002)

> 2026-04-06 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### CON-002: 시설 콘텐츠 상세

Designer + Architect 병렬 실행으로 2개 문서 신규 작성. Reviewer가 CRITICAL 6건·WARNING 4건 발견 후 전부 수정. 총 7개 파일 수정됨.

**신규 문서**:
1. `docs/content/facilities.md` — 시설 콘텐츠 상세 (Designer)
2. `docs/systems/facilities-architecture.md` — 시설 기술 아키텍처 (Architect)

**수정된 문서**:
- `docs/design.md` — Cross-references에 시설 문서 2종 추가
- `docs/architecture.md` — Cross-references에 facilities.md 추가
- `docs/systems/crop-growth.md` — 온실 계절 보정값 분기 정의 추가 (C-1)
- `docs/pipeline/data-pipeline.md` — tileSize/buildTimeDays 수정 3건, 레시피 수 15→18, 필드명 정합화 (C-2, C-4, C-6)

---

## 핵심 설계 내용

### 시설 4종 상세 확정

**물탱크 (Water Tank)** — `building_water_tank`
- 점유: 2×2 타일, 건설 1일, 레벨 3 해금 (→ see design.md 4.6)
- 범위: Lv.1 12타일 → Lv.2 24타일 → Lv.3 40타일 (맨해튼 거리 기반)
- 저수량: Lv.1 16 → Lv.2 32 → Lv.3 60 (비 오는 날 자동 충전)
- 업그레이드 비용: 750G / 1,500G

**온실 (Greenhouse)** — `building_greenhouse`
- 점유: 6×6 타일, 건설 2일, 레벨 5 해금 (→ see design.md 4.6)
- 내부 경작 타일: Lv.1 4×4(16) → Lv.2 5×5(25) → Lv.3 6×6(36)
- 비계절 작물 보정: 성장 ×0.85, 품질 ×0.9 (이 문서가 canonical)
- 겨울 전용 작물(겨울무·표고버섯·시금치): 온실 내 보정 없음 (×1.0)
- 업그레이드 비용: 3,000G / 5,000G

**창고 (Storage)** — `building_storage`
- 점유: 3×2 타일, 건설 1일, 레벨 4 해금 (→ see design.md 4.6)
- 기본 슬롯 30칸, 업그레이드 시 45칸 / 60칸
- 인벤토리와 별개의 장기 저장 공간 — 가격 변동 대응용
- 업그레이드 비용: 1,500G / 2,500G

**가공소 (Processing Plant)** — `building_processing`
- 점유: 4×3 타일, 건설 2일, 레벨 7 해금 (→ see design.md 4.6)
- 가공 레시피 18종 확정: 잼 7종 + 주스 3종 + 절임 5종 + 겨울작물 3종
- 동시 처리 슬롯: Lv.1 1개 → Lv.2 2개 → Lv.3 3개
- 가공 시간 단축: Lv.2 ×0.75, Lv.3 ×0.5
- 업그레이드 비용: 4,000G / 7,000G

### 시설 공통 규칙 확정
- 건설 자원: 골드 전용 (재료 조달 불필요)
- 건설 소요: 물탱크·창고 1일, 온실·가공소 2일
- 건설 중 타일 점유 시작, 사용 불가
- 철거 가능, 환급률 50%
- 경작지와 겹침 배치 불가

### 기술 아키텍처 주요 결정
- `ISeasonOverrideProvider` 인터페이스 도입 — Farm → Building 의존 없이 온실 계절 해제
- `BuildingEvents` 정적 이벤트 허브로 시스템 간 느슨한 결합
- `ProcessingRecipeData` ScriptableObject 9필드 (PATTERN-005 검증 통과)
- 철거 흐름: 건물 존재 확인 → 아이템 반환 → 타일 해제 → 50% 환급 → SaveData 제거

---

## 리뷰 결과

**CRITICAL 6건 (수정 완료)**:
1. [C-1] 온실 계절 보정 충돌 — crop-growth.md x1.0 vs facilities.md x0.85 → crop-growth.md를 분기 정의로 업데이트
2. [C-2] data-pipeline.md 시설 tileSize/buildTimeDays 3건 불일치 → canonical 기준으로 수정
3. [C-3] facilities-architecture.md JSON의 온실/가공소/창고 tileSize 오류 → 수정
4. [C-4] 가공 레시피 수 15 vs 18 불일치 → 18로 통일
5. [C-5] 철거 정책 모순 (facilities.md 50% 환급 vs architecture OPEN 미정) → architecture에 50% 환급 확정
6. [C-6] BuildingData 필드명 불일치 (buildingId vs dataId) → 부모 클래스 상속 표기로 통일

**WARNING 4건 (수정 완료)**:
1. [W-1] design.md Cross-references 시설 문서 누락 → 추가
2. [W-2] architecture.md Cross-references 누락 → 추가
3. [W-3] GreenhouseSystem 코드 주석 구식 tileSize → 수정
4. [W-4] facilities-architecture.md OPEN 항목 해소 미처리 → 처리

---

## 의사결정 기록

1. **비계절 온실 보정값 x0.85/x0.9 채택**: 온실이 단순한 "계절 무시 공간"이 아니라 적절한 리스크/보상이 있는 전략 선택지로 만들기 위함. 계절 전용 작물은 패널티 없이 최적 환경 제공.

2. **물탱크 맨해튼 거리 기반 범위**: 원형 거리 대비 타일 경계 처리가 단순하고, 농장 그리드 게임 특성상 더 자연스러운 사용자 경험.

3. **가공소 18종 레시피 확정**: 잼·주스·절임 세 카테고리 + 겨울 작물 가공 3종. 각 작물에 1개 이상의 가공 경로를 보장해 모든 작물의 경제적 활용도 확보.

4. **ISeasonOverrideProvider 인터페이스**: 의존성 역전으로 SeedMind.Farm 네임스페이스가 Building에 의존하지 않도록 설계. 테스트 용이성도 확보.

---

## 미결 사항 ([OPEN])

- **닭장/벌통**: MVP 이후 확장 후보. 동물 허기 시스템 연동 필요 여부 논의 필요.
- **온실 내부 씨앗 자동 공급**: 온실 레벨 3 특전 검토 중.
- **가공소 부산물 시스템**: 가공 시 씨앗/비료 등 부산물 생성 가능성 [OPEN] 유지.

---

## 후속 작업 필요

- `docs/balance/crop-economy.md`: 가공품 ROI 추가 → BAL-003 또는 BAL-004로 처리 필요
- PATTERN-007 발견: SO 에셋 테이블의 tileSize/buildTimeDays 직접 기재 패턴 → self-improve 대상

---

## 다음 단계

- BAL-003: 겨울 작물 밸런스 분석 (Priority 2)
- CON-003: NPC/상점 콘텐츠 (Priority 2)
- DES-006: 튜토리얼/온보딩 시스템 (Priority 2)
- PATTERN-007: self-improve 처리 (SO 에셋 테이블 수치 기재 규칙 강화)

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
