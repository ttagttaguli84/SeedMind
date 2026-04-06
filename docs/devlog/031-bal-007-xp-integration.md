# Devlog #031 — BAL-007: XP 통합 재조정

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

### BAL-007: XP 통합 예산 설계

`docs/balance/xp-integration.md` 신규 작성 (BAL-007).  
`docs/balance/bal-007-architecture-analysis.md` 아키텍처 분석 지원 문서 작성.

---

## 핵심 발견: BAL-006의 XP 기준값 오류

`docs/balance/progression-curve.md`에는 두 개의 XP 테이블이 공존한다:

| 테이블 | baseXP | growthFactor | 레벨 10 누적 XP | 위치 |
|--------|--------|-------------|----------------|------|
| 원본 | 50 | 1.55 | 4,609 | 섹션 1.3.2 |
| **조정 후 (canonical)** | **80** | **1.60** | **9,029** | 섹션 2.4.1 |

BAL-006(퀘스트 보상 밸런스)은 원본 테이블(4,609)을 기준으로 비율을 분석했으나, 실제 canonical은 조정 후 테이블(9,029)이다. BAL-006의 "XP 인플레이션 198.5%" 판정은 **기준값이 잘못된 상태에서의 계산**이었다.

9,029 기준 재계산:
- 퀘스트 XP(제안 A 692) = 9,029의 7.7% → 목표 10~15% **미달**
- 업적 XP(2,250) = 9,029의 24.9% → 종전 "49%" 판정 오류

---

## 세 시나리오 시뮬레이션 결과

| 항목 | 시나리오 A | 시나리오 B | 시나리오 C |
|------|----------|----------|----------|
| XP 테이블 | 유지 (gF=1.60) | 하향 (gF=1.55) | 유지 |
| 퀘스트 XP | 692 XP | 692 XP | 692 XP |
| 업적 XP | 유지 (2,250) | 유지 | 삭감 (1,310) |
| 캐주얼 1년차 | 레벨 7 | 레벨 8 (과도) | 레벨 7 |
| 일반 1년차 | **레벨 8** | 레벨 8~9 | **레벨 8** |
| 보조 소스 가치 | 높음 | **낮음 (희석)** | 중간 |

**시나리오 B 탈락**: growthFactor 하향 시 캐주얼 플레이어가 수확만으로 레벨 8 도달 → 퀘스트/업적 보조 역할 무의미화.

---

## 확정 권장안: 시나리오 A' (제안 A 수정)

BAL-006 제안 A(692 XP)에서 소폭 상향하여 **퀘스트 XP 총량 900 XP** 채택.

| 항목 | 확정값 |
|------|--------|
| XP 테이블 | baseXP=80, growthFactor=1.60, 레벨 10=9,029 (유지) |
| 퀘스트 XP 총량 | **900 XP** (전체의 10%) |
| 업적 XP 총량 | **2,250 XP** (유지, 전체의 24.9%) |
| 1년차 목표 (캐주얼) | 레벨 8 갓 진입 |
| 1년차 목표 (일반) | **레벨 8 중반** |
| 1년차 목표 (적극적) | **레벨 9 직전** |
| 레벨 10 달성 | 적극적 2년차 중후반, 캐주얼 3년차 중반 |

### 카테고리별 퀘스트 XP 배분 (900 XP)

| 카테고리 | 배분 XP | 1년차 실현 |
|----------|---------|-----------|
| 메인 퀘스트 | ~280 XP | ~280 XP |
| NPC 의뢰 | ~140 XP | ~100 XP |
| 일일 목표 | ~280 XP | ~280 XP |
| 농장 도전 | ~200 XP | ~80 XP |
| **합계** | **~900 XP** | **~740 XP** |

---

## 아키텍처 발견: XPSource enum 미확장

`progression-architecture.md`의 `XPSource` enum에 `QuestComplete`/`AchievementReward`가 없음:
- `QuestComplete`는 주석 상태, `AchievementReward`는 아예 없음
- `GetExpForSource()` switch문에도 해당 case 누락
- XP 획득 로그에서 퀘스트/업적 출처 추적 불가

FIX-019~020으로 후속 작업 등록.

---

## 리뷰 수정 사항

리뷰어에서 FAIL 판정 → 수정 완료:
- CRITICAL: `bal-007-architecture-analysis.md` FIX 번호 충돌 해소 (FIX-012→FIX-013, 이하 +1)
- CRITICAL: Step 2 FIX 번호-내용 불일치 수정 (FIX-012 기존 + FIX-017 신규로 교체)
- WARNING: `xp-integration.md` 섹션 1.3/3.1/3.4 직접 수치 제거, 참조 전용으로 변경
- INFO: 두 문서 간 cross-reference 상호 추가

---

## 후속 작업 (TODO 추가)

- **FIX-012** (Priority 3 상향): quest-system.md 퀘스트 XP 수치 재확정 (제안 A' 900 XP)
- **FIX-013~015**: progression-curve.md 3개 수정 항목
- **FIX-016~018**: quest-rewards.md, achievements.md, quest-system.md 참조/비율 업데이트
- **FIX-019~023**: progression-architecture.md XPSource enum + 아키텍처 문서 호출 명시

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
