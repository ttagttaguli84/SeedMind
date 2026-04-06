# Devlog #015 — NPC/상점 콘텐츠 (CON-003) + FIX-004

> 2026-04-06 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### FIX-004: data-pipeline.md 시설 에셋 테이블 수정

PATTERN-007 후속 조치. `docs/pipeline/data-pipeline.md` 섹션 2.4의 시설별 에셋 데이터 테이블에서 `tileSize`, `buildTimeDays`, `effectRadius` 직접 수치를 모두 `(→ see docs/content/facilities.md 섹션 X.X)` canonical 참조로 교체.

**수정된 파일**:
- `docs/pipeline/data-pipeline.md` — 시설 에셋 테이블 4행 전체 직접 수치 → 참조로 교체
- `TODO.md` — FIX-004 DONE 처리

---

### CON-003: NPC/상점 콘텐츠

Designer + Architect 병렬 실행 → Reviewer CRITICAL 5건·WARNING 3건 발견 후 전부 수정.

**신규 문서**:
1. `docs/content/npcs.md` — NPC/상점 콘텐츠 canonical 설계 문서
2. `docs/systems/npc-shop-architecture.md` — 기술 아키텍처 (Part I + Part II)

**수정된 문서**:
- `docs/systems/tool-upgrade.md` — 대장간 NPC 이름 "철수" 확정, 영업시간 직접 수치 → canonical 참조
- `docs/systems/economy-system.md` — 섹션 3.3 구시대 5단계 도구 비용 표 삭제 → tool-upgrade.md 참조, 섹션 5.2 등급명 수정(금/이리듐 → 강화/전설), 섹션 5.3 비용 수치 참조 교체
- `docs/content/npcs.md` — 섹션 3.2·4.2·5.2 영업시간·휴무일 직접 기재 제거 → canonical 참조
- `docs/systems/npc-shop-architecture.md` — 여행 상인 위치 수정(농장 입구 → 마을 광장), "작성 예정" 문구 4곳 제거
- `docs/design.md`, `docs/architecture.md` — Cross-references 추가

---

## 핵심 설계 내용

### NPC 4인 체계 확정

| NPC | 이름 | 역할 | 위치 | 해금 |
|-----|------|------|------|------|
| 시장 상인 | 하나 | 씨앗·비료 판매 | 마을 상점 | 게임 시작부터 |
| 대장간 장인 | 철수 | 도구 업그레이드 | 대장간 | 레벨 3 |
| 목공소 장인 | 목이 | 시설 건설 서비스 | 목공소 | 레벨 5 |
| 여행 상인 | 바람이 | 희귀 아이템 판매 | 마을 광장 | 레벨 3, 주말 등장 |

### 상점 인벤토리 구조
- **하나(시장)**: 봄·여름·가을·겨울 계절별 씨앗 + 비료 4종 + 기본 도구 — 가격 (→ see `docs/design.md` 섹션 4.2)
- **철수(대장간)**: 도구 업그레이드 서비스 — 비용/재료 (→ see `docs/systems/tool-upgrade.md`)
- **목이(목공소)**: 시설 건설 대행 서비스 — 비용/요건 (→ see `docs/content/facilities.md`)
- **바람이(여행)**: 만능 비료·겨울 전용 씨앗·황금 씨앗 등 8종 희귀 아이템 풀 — 가격 [OPEN]

### NPC 대화 시스템
- 접근 트리거 → 인사/상점/이벤트 대화 분기
- 계절별·진행도별 대사 변화 (최소 3단계 분기)
- JSON 기반 `DialogueData` SO 구조로 데이터 주도 대화 관리

### 기술 아키텍처 주요 결정
- `NPCManager` 싱글턴 + `NPCController` 개별 제어 분리
- `NPCEvents` 정적 이벤트 허브 (FarmEvents/BuildingEvents 패턴 계승)
- `TravelingMerchantScheduler` 독립 클래스 — 주말 감지 + 랜덤 아이템 풀 구성
- `TravelingMerchantSaveData`: JSON 스키마 ↔ C# 클래스 동기화 (PATTERN-005 준수)
- SeedMind.NPC 네임스페이스 신설, `Scripts/NPC/` 폴더 구조 확정

---

## 리뷰 결과

**CRITICAL 5건 (수정 완료)**:
1. [C-1] tool-upgrade.md 영업시간 09:00~18:00 vs canonical(economy-system.md) 10:00~16:00·금요일 휴무 → 참조로 교체
2. [C-2] economy-system.md 섹션 3.3 구시대 5단계 도구 비용 (500G→1000G→2500G→5000G) → 3단계 canonical 참조로 교체
3. [C-3] economy-system.md 섹션 5.3 구시대 비용 인라인 기재 → 참조로 교체
4. [C-4] economy-system.md 섹션 5.2 "금/이리듐" 구 등급명 → "강화/전설"로 수정
5. [C-5] npc-shop-architecture.md 여행 상인 위치 "농장 입구" → "마을 광장"으로 수정

**WARNING 3건 (수정 완료)**:
1. [W-1] npcs.md 3개 NPC 섹션 영업시간 직접 기재 → canonical 참조로 교체
2. [W-2] npc-shop-architecture.md "CON-003 작성 예정" 문구 4곳 잔존 → 제거
3. [W-3] TODO.md CON-003 미완료 상태 → 완료 처리

---

## 의사결정 기록

1. **NPC 4인 체계**: 기능별 전문화 (구매/업그레이드/건설/희귀템)로 각 NPC가 뚜렷한 방문 동기를 가짐. 플레이어가 각 NPC를 방문할 이유를 게임 진행 단계별로 자연스럽게 분산.

2. **여행 상인 주말 등장**: 플레이어에게 주중 농사·주말 구매라는 리듬을 제공. 희귀 아이템 구매를 위한 자금 계획 동기 부여.

3. **목이(목공소) 추가**: 시설 건설 서비스를 별도 NPC로 분리하여 건설 행위가 단순 메뉴 클릭이 아닌 NPC와의 상호작용으로 승격. 스토리텔링 기회 확보.

4. **DialogueData SO 구조**: 하드코딩 대사 대신 ScriptableObject 데이터 주도 방식 채택 → MCP로 대화 데이터 주입 가능, 향후 로컬라이제이션 대비.

---

## 미결 사항 ([OPEN])

- 여행 상인 희귀 아이템 8종 가격 밸런스 미확정 → BAL-005
- NPC 호감도 시스템 도입 여부 미결정 (npcs.md 섹션 10)
- 겨울 전용 씨앗의 시장 vs 여행 상인 독점 판매 결정 필요
- 대장간 추가 서비스(울타리, 스프링클러 부품) 미확정 → CON-004
- 목공소 동시 건설 제한 완화 기준 미확정

---

## 후속 작업

- `ARC-009`: NPC/상점 MCP 태스크 시퀀스 독립 문서화
- `BAL-005`: 여행 상인 아이템 가격 밸런스 분석
- `CON-004`: 대장간 NPC 상세 설계 (철수 캐릭터, 업그레이드 UX)
- `ARC-008`: 도구 업그레이드 MCP 태스크 시퀀스
- `ARC-007`: 시설 MCP 태스크 시퀀스

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
