# Devlog #034 — DES-011 + ARC-018: UI/UX 시스템 설계 완료

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

UI/UX 시스템 상세 설계(DES-011)와 UI 시스템 기술 아키텍처(ARC-018)를 병렬로 작성하고, 리뷰어 검토 후 불일치 사항을 수정했다.

### 생성/수정된 파일

| 파일 | 내용 |
|------|------|
| `docs/systems/ui-system.md` | UI/UX 시스템 상세 설계 (DES-011) — 신규 |
| `docs/systems/ui-architecture.md` | UI 시스템 기술 아키텍처 (ARC-018) — 신규 |
| `docs/architecture.md` | Cross-reference 추가 |
| `docs/systems/project-structure.md` | UI 폴더/네임스페이스 확장 |
| `TODO.md` | DES-011·ARC-018 완료 처리, FIX-024 등록 |

---

## 주요 설계 결정

### DES-011: UI/UX 시스템 (ui-system.md)

**HUD 구조** — 7개 영역 확정:
- 좌상단: 날짜/시간 + 계절/날씨
- 우상단: 골드 표시
- 우측 중앙: 퀘스트 추적기 (최대 2개)
- 하단 좌측: 에너지 바
- 하단 중앙: 툴바 8칸 (→ see inventory-system.md)
- 하단 우측: 레벨/XP 표시

**주요 UI 패널**:
- 인벤토리: 툴바(8칸) + 백팩(5×6) 통합 패널, 드래그앤드롭
- 퀘스트 로그: 활성/완료/농장 도전 3탭 구조
- 상점: 상인별 인벤토리 + 구매/판매 플로우, 대화 박스
- 시설 건설: 건설 메뉴 → 배치 미리보기 → 건설 진행 표시

**알림 시스템**: 동시 최대 3개 토스트, 10단계 우선순위 큐

**접근성**: 텍스트 크기 3단계, 색약 대체 색상표, 화면 흔들림 끄기

### ARC-018: UI 기술 아키텍처 (ui-architecture.md)

**UIManager Screen FSM** — ScreenType 10개:
`None / Farming / Inventory / Shop / Quest / Achievement / Menu / SaveLoad / Dialogue / Processing`

**Canvas 계층 구조** (5개 분리):
| Canvas | Sort Order | 역할 |
|--------|-----------|------|
| Canvas_HUD | 0 | 항상 표시 HUD |
| Canvas_Screen | 10 | 메인 패널 |
| Canvas_Popup | 20 | 확인/오류 팝업 |
| Canvas_Notification | 30 | 토스트 알림 |
| Canvas_Tutorial | 40 | 튜토리얼 오버레이 |

**NotificationManager**: PriorityQueue 기반, 오브젝트 풀링, 12개 이벤트 구독
**PopupQueue**: Critical > High > Normal > Low, 동시 요청 시 우선순위 정렬

---

## 리뷰 결과

| 심각도 | 건수 | 처리 |
|--------|------|------|
| CRITICAL | 2 | 즉시 수정 완료 |
| WARNING | 4 | 3개 수정 완료, 1개 TODO(FIX-024) |
| INFO | 3 | 메모 |

### 수정 내역

| 문제 | 수정 내용 |
|------|----------|
| ARC-018에서 업적 패널 키 `K` → `Y` (3곳) | achievement-system.md canonical과 일치 |
| DES-011 동시 토스트 "최대 1개" → "최대 3개" | ARC-018 `_maxVisibleToasts=3`과 일치 |
| DES-011 Canvas Sorting Order를 개념 모델로 재정의 + ARC-018 참조 추가 | 두 문서 체계 충돌 해소 |
| Cross-references 상호 누락 | 양쪽 문서에 서로 추가 |

### FIX-024 등록

`achievement-system.md` 섹션 5.4의 토스트 위치 표기 "좌측 하단" → "상단 중앙" 수정 필요 (DES-011 canonical과 불일치).

---

## 다음 작업 후보

| ID | Priority | 내용 |
|----|----------|------|
| FIX-024 | 3 | achievement-system.md 토스트 위치 수정 |
| ARC-010 | 2 | 튜토리얼 MCP 태스크 시퀀스 |
| ARC-012 | 2 | 세이브/로드 MCP 태스크 시퀀스 |
| VIS-001 | 2 | 비주얼 가이드 |
| BAL-003 | 2 | 겨울 작물 ROI 분석 |

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
