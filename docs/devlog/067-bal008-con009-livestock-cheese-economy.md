# Devlog #067 — BAL-008 + CON-009: 목축 경제 밸런스 확정 및 치즈 공방 레시피 추가

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

이번 세션은 BAL-008(목축/낙농 경제 밸런스)과 CON-009(치즈 공방 레시피)를 순차 완료했다. BAL-008은 Designer 에이전트가 `livestock-economy.md`를 대폭 확장했고, Reviewer가 CRITICAL 이슈(외양간 비용 누락)를 발견하여 즉시 수정했다. CON-009는 Architect 분석을 바탕으로 `processing-system.md`에 치즈 공방 레시피 5종을 추가하고, 관련 3개 아키텍처 문서의 ProcessingType enum을 동기화했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/balance/livestock-economy.md` | BAL-008 완료: 동물별 ROI, 가공 체인, 에너지 효율, 초기 투자 시나리오, 작물 병행 시뮬레이션 전면 확장 |
| `docs/content/livestock-system.md` | 섹션 8.2 시나리오 B 투자액 11,700G(외양간 포함) 수정, 섹션 8.3 ROI 테이블 수치 수정, 섹션 8.4 [OPEN] → [RESOLVED] |
| `docs/content/processing-system.md` | CON-009: 섹션 3.6 치즈 공방 레시피 5종 신설, 3.6→3.7 요약 테이블 37→42종, 가공소 테이블 치즈 공방 추가, 연료 테이블 업데이트 |
| `docs/systems/processing-architecture.md` | ProcessingType enum에 `Cheese` 추가 |
| `docs/systems/facilities-architecture.md` | ProcessingType enum에 `Cheese` 추가 |
| `docs/pipeline/data-pipeline.md` | ProcessingType 테이블 Cheese 행 추가, 레시피 에셋 수 32→42종 |

---

## BAL-008 목축 경제 밸런스 결과

### 동물별 일일 순수익 (직판 기준)

| 동물 | 일일 수입 | 일일 사료비 | 일일 순수익 |
|------|---------|-----------|-----------|
| 닭 x1 | 35G/일 (달걀) | 5G | 30G |
| 소 x1 | 60G/일 (우유 평균) | 10G | 50G |
| 염소 x1 | 40G/일 (염소젖 평균) | 7G | 33G |
| 양 x1 | ~55G/일 (양모 평균) | 7G | 48G |

> 수치 출처: (→ see `docs/content/livestock-system.md` 섹션 1.1, 4.1)

### 초기 투자 회수 시나리오 (수정 후 확정값)

| 시나리오 | 총 투자 | 일일 순수익 | 회수 기간 |
|---------|---------|-----------|----------|
| 최소 (닭 x2) | 7,100G | 50G | 142일 |
| 중간 (닭 x4 + 소 x1) | **15,700G** | 130G | **121일** |
| 적극 (닭 x4 + 소 x2 + 염소 x2) | **23,700G** | 230G | **103일** |
| D. 중간 + 치즈 공방 | **20,200G** | ~258G | **78일** |

> 이전 버전 대비 시나리오 B/C 외양간 3,000G 누락 수정 적용.

### 핵심 밸런스 결론

- 목축 ROI는 작물 전용(옥수수 20타일 ~7일 회수) 대비 회수 기간이 길지만, **한번 자리잡으면 씨앗 재투자 없이 매일 안정 수입** 제공
- 에너지 효율: 소(치즈 가공 시) 19.5G/E로 작물 최고(딸기 13.6G/E) 대비 43% 우수
- **권장 전략**: 작물로 현금 흐름 확보 → Zone E 해금 후 닭장+닭 x2로 시작 → 자금 축적 후 외양간+소 추가

---

## CRITICAL 이슈 수정: 외양간 건설 비용 누락 (Reviewer 발견)

리뷰어가 `livestock-economy.md` 섹션 4.2와 `livestock-system.md` 섹션 8.2~8.3에서 CRITICAL 이슈를 발견했다.

- **이슈**: 시나리오 B (닭 x4 + 소 x1)의 총 투자에서 **외양간 Lv.1 건설비(3,000G)가 누락**됨
  - 소 사육에는 외양간이 선행 필수 시설(livestock-system.md 섹션 3.2)
  - 12,700G → 15,700G (+3,000G), 회수 기간 98일 → 121일
  - 시나리오 C도 동일 수정: 20,700G → 23,700G, 회수 90일 → 103일

- **수정 파일**: livestock-system.md 섹션 8.2~8.3, livestock-economy.md 섹션 4.2/4.3/4.4/5.1/5.2

---

## CON-009 치즈 공방 레시피 5종 확정

### 레시피 목록

| 레시피 | 재료 | 처리 시간 | 판매가 | 역할 |
|--------|------|----------|--------|------|
| `recipe_cheese_basic` | 우유 x1 | 4시간 | 250G | 기본 유제품, 주력 레시피 |
| `recipe_cheese_goat` | 염소젖 x1 | 4시간 | 190G | 염소 전용 경로 |
| `recipe_butter` | 우유 x2 | 3시간 | 160G | 베이커리 연계 중간 가공재 |
| `recipe_cheese_aged` | 치즈 x1 | 12시간 | 680G | 2차 가공, 장기 투자 |
| `recipe_cream` | 우유 x1 + 달걀 x1 | 3시간 | 280G | 복합 재료, 베이커리 연계 |

### 설계 의도
- 버터/크림은 단독 판매 효율이 낮아 베이커리 연계 시 부가가치 극대화
- 에이지드 치즈: 우유(120G) → 치즈(4h, 250G) → 에이지드 치즈(12h, 680G) = 총 16시간 체인, 수익 5.7x
- 크림 제작에 닭(달걀) + 소(우유) 모두 필요 → 다종 동물 사육 유도

### 아키텍처 변경
- `ProcessingType` enum에 `Cheese` 추가 (processing-architecture.md, facilities-architecture.md, data-pipeline.md)
- 레시피 에셋 총 32종 → 42종
- [OPEN] 버터/크림 재료로 활용하는 베이커리 신규 레시피 추가 검토

---

## 리뷰어 검증 결과

| ID | 심각도 | 이슈 내용 | 수정 결과 |
|----|--------|-----------|-----------|
| R-01 | 🔴 CRITICAL | livestock-system.md 8.2~8.3 외양간 비용 누락 — 시나리오 B 12,700G는 3,000G 과소 | 15,700G로 수정 완료 |
| R-02 | 🟡 WARNING | livestock-economy.md 섹션 4의 소제목 번호 오류 (3.1~3.4 → 4.1~4.4) | 리뷰어가 직접 수정 완료 |
| R-03 | 🟡 WARNING | 섹션 1.1 파라미터 요약 테이블이 canonical 수치를 직접 재기재 | 패턴 인지 등록, 향후 변경 시 동기화 필요 |

---

## 세션 후 활성 TODO

| ID | Priority | 상태 |
|----|----------|------|
| DES-015 | 1 | 잔여 (낚싯대 업그레이드 재료 공급 경로) |
| CON-011 | 1 | 잔여 (낚시 도감 콘텐츠) |
| FIX-072 | 1 | 잔여 (economy-system.md 낚시 수입 항목) |
| ARC-030 | 1 | 잔여 (낚시 도감 아키텍처) |
| BAL-014 | 1 | 잔여 (낚시 숙련도 XP 밸런스) |
| DES-016 | 1 | 잔여 (채집 시스템 기본 설계) |
| PATTERN-009/010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 BAL-008 + CON-009 태스크에 따라 자율적으로 작성했습니다.*
