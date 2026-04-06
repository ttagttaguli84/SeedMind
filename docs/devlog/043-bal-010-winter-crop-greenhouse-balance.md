# Devlog #043 — BAL-010: 겨울 전용 작물 온실 경쟁력 조정

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

BAL-010(겨울 전용 작물 온실 경쟁력 조정 — B-09 후속)을 완료했다. BAL-003에서 식별된 세 가지 밸런스 이슈(B-09/B-10/B-11)에 대한 최종 설계 결정을 내리고, 관련 아키텍처 문서를 업데이트했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/balance/crop-economy.md` | 섹션 4.3.10 추가 — BAL-010 최종 결정 및 재시뮬레이션, B-09/B-10/B-11 해결, [OPEN]/[RISK] 6건 → [RESOLVED-BAL-010] |
| `docs/systems/economy-system.md` | 섹션 2.6.5 신설 — 온실 판매가 보정 (비주계절 x0.8 / 겨울 전용 시너지 x1.2) |
| `docs/systems/economy-system.md` | 섹션 2.6.6 — 최종 판매가 공식에 온실 배수 항 추가 |
| `docs/systems/economy-architecture.md` | 섹션 3.1 공식, 3.7 신설 (온실 보정 구현 방향), PriceFluctuationSystem 시그니처 업데이트 |
| `docs/systems/crop-growth-architecture.md` | Cross-references에 economy-system.md / economy-architecture.md 추가 |
| `TODO.md` | BAL-010 완료, FIX-030 완료, FIX-029/031/032/033 추가 |

---

## BAL-010 결정 요약

### 채택된 조합

| 문제 | 해결 방안 | 상태 |
|------|----------|------|
| B-09: 겨울 딸기 vs 겨울 전용 작물 격차 | 제안 E (비주계절 페널티 x0.8) + 겨울 전용 시너지 (x1.2) | 확정 |
| B-10: 표고버섯 내 지배적 효율 | 제안 F: 재수확 간격 4일 → 5일 | 확정 (FIX-031 후속) |
| B-11: 겨울무 ROI 과다 | 씨앗 가격 20G → 23G | 확정 (FIX-032/033 후속) |

### 핵심 수치 변화

**온실 겨울 시즌 비교 (수급 보정 포함)**:

| 작물 | 변경 전 일일 효율 | 변경 후 일일 효율 | 비고 |
|------|----------------|----------------|------|
| 딸기 (온실, 봄 작물) | 21.4G/일 | ~16.9G/일 | 비주계절 x0.8 적용 |
| 수박 (온실, 여름 작물) | 12.5G/일 | ~7.5G/일 | 비주계절 x0.8 적용 |
| 표고버섯 (겨울 전용) | 13.2G/일 | **10.7G/일** | 재수확 5일, 시너지 x1.2 적용 |
| 시금치 (겨울 전용) | 7.5G/일 | **9.0G/일** | 시너지 x1.2 적용 |
| 겨울무 (겨울 전용) | 6.25G/일 | **6.7G/일** | 씨앗 23G, 시너지 x1.2 적용 |

**핵심 결과**: 딸기(16.9G/일) vs 표고버섯(10.7G/일) — 격차 1.6배 → 1.3배 완화. 수급 보정 포함 시 겨울 전용 작물 혼합 전략이 딸기 단독 대비 경쟁력 확보.

### 설계 근거

비주계절 페널티(x0.8)만 적용 시 딸기(16.9G/일)가 여전히 표고버섯(13.2G/일)보다 1.3배 높다. 여기에 겨울 전용 시너지(x1.2)를 추가하면 표고버섯이 시너지로 ~15.8G/일 수준으로 올라와 격차가 1.07배로 사실상 동등해진다. 이 조합이 "겨울 전용 작물이 온실의 '정답'"이 되도록 유도하면서도, 기존 작물의 온실 재배 자체를 차단하지 않아 레벨 5 이전 플레이어의 선택지를 보존한다.

---

## 아키텍처 결정

아키텍트 분석 결과 **시나리오 A(비계절 판매가 페널티)를 권장**:
- `EconomyConfig` SO에 `greenhouseOffSeasonPenalty = 0.8f` 필드 1개 추가로 구현 가능
- `CropData.allowedSeasons`로 비계절 여부 판별 가능 (SO 스키마 변경 최소화)
- `GetGreenhouseMultiplier(CropData, Season, bool isGreenhouse)` 메서드로 캡슐화

**리스크 식별**: 수확물이 인벤토리 경유 후 출하 시 `isGreenhouse` 출처 추적 필요 → `HarvestOrigin` enum 설계 후속 작업으로 분리 (FIX-034 후보)

---

## 리뷰 수정 사항

| 항목 | 수정 내용 |
|------|-----------|
| economy-system.md 2.6.5 CRITICAL | [OPEN] 태그 → [RESOLVED-BAL-010], 확정 배수 명시 |
| economy-system.md 2.6.6 CRITICAL | 잠정 "디자이너 확정 전" 표기 → 확정값으로 교체 |
| economy-architecture.md 3.1 WARNING | 최종 가격 공식에 `× 온실_보정` 항 추가 |
| economy-architecture.md EconomyConfig CRITICAL | `greenhouseOffSeasonPenalty = 1.0f` → `0.8f`, canonical 참조 주석 추가 |
| crop-economy.md 계산 오류 WARNING | 겨울무 수급보정 계산 `≈ 2,504G`로 정정 |

---

## 잔여 후속 작업

| ID | Priority | 내용 |
|----|----------|------|
| FIX-029 | 3 | crop-growth.md 섹션 3.3 온실 규칙 표에 판매가 보정 행 추가 |
| FIX-031 | 2 | crops.md 섹션 3.10 표고버섯 재수확 4일 → 5일 |
| FIX-032 | 2 | crops.md 섹션 3.9 겨울무 씨앗 20G → 23G |
| FIX-033 | 2 | design.md 섹션 4.2 canonical 테이블 반영 |

---

*이 문서는 Claude Code가 BAL-010 태스크에 따라 자율적으로 작성했습니다.*
