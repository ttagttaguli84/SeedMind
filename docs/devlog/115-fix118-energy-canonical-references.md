# Devlog #115 — FIX-118: 에너지 소모 수치 canonical 참조 교체

> 작성: Claude Code (Sonnet 4.6) | 2026-04-09

---

## 작업 요약

**FIX-118**: farming-system.md / fishing-system.md / gathering-system.md / time-season.md 4개 문서에 분산 기재되어 있던 에너지 소모 수치를 `docs/systems/energy-system.md` canonical 참조로 교체.

DES-024에서 `energy-system.md`가 에너지 시스템의 canonical 문서로 확정되었으나, 기존 시스템 문서들의 원본 에너지 수치가 그대로 잔존하고 있었다.

---

## 수정된 파일

| 파일 | 수정 내용 |
|------|----------|
| `docs/systems/farming-system.md` | 섹션 3.2 에너지 시스템 → canonical 참조로 교체, 튜닝 파라미터 4행(maxEnergy/hoeEnergy/waterEnergy/sickleEnergy) → 참조 1행 통합 |
| `docs/systems/fishing-system.md` | 섹션 2.3 에너지 소모 테이블 → canonical 참조로 교체, 설계 의도 내 farming-system.md 참조 → energy-system.md로 갱신, 튜닝 파라미터 2행(castEnergy/failExtraEnergy) → 참조 1행, Cross-references 갱신 |
| `docs/systems/gathering-system.md` | 플로우 다이어그램 에너지 소모 수치에 canonical 참조 추가, 설계 의도 텍스트 수치 → 참조로 교체, 튜닝 파라미터 toolGatherEnergy → 참조, Cross-references 갱신 |
| `docs/systems/time-season.md` | 시간대 테이블 Night 행 에너지 가중 수치 → 참조, 수면 회복 테이블 → canonical 참조 추가, 날씨 에너지 소모 영향 섹션 테이블 → canonical 참조로 교체, 튜닝 파라미터 6행(nightEnergyMultiplier/passOutEnergyRecovery + 날씨 4종) → 참조 2행, Cross-references 갱신 |

---

## 리뷰 결과

FIX-* 단순 수정 (단일 주제 참조 교체), reviewer 생략 조건 충족.

- 새로운 수치 도입 없음
- canonical 문서(energy-system.md) 기존 확정 수치의 참조 표기만 변경
- 총 4개 문서, 모두 에너지 관련 섹션/파라미터에 한정

---

## 잔존 활성 TODO

| ID | 우선순위 | 설명 |
|----|----------|------|
| ARC-047 | 1 | 에너지 시스템 MCP 태스크 시퀀스 독립 문서화 |
| FIX-119 | 1 | gathering-items.md 황금 연꽃/천년 영지 판매가 canonical 등록 |
| PATTERN-011 | - | self-improve 전용 — MCP 에셋명 임의 생성 패턴 |
