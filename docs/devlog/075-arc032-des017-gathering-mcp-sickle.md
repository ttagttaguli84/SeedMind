# Devlog #075 — ARC-032 + DES-017: 채집 MCP 태스크 문서화 및 낫 업그레이드 상세 설계

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

채집 시스템의 두 잔여 Priority 2 작업을 완료했다. ARC-032로 MCP 태스크 시퀀스를 독립 문서로 분리하고, DES-017로 채집 낫 업그레이드 경로의 상세 설계를 추가했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/mcp/gathering-tasks.md` | ARC-032 신규: Phase G-A~G-G, ~136회 MCP 호출, 16개 스크립트 정의 |
| `docs/systems/gathering-architecture.md` | 섹션 8 노트에 gathering-tasks.md 링크 추가 |
| `docs/systems/gathering-system.md` | DES-017: 섹션 5.4~5.6 신규 + 섹션 7.3 대장간 대화 확장 |

---

## ARC-032: 채집 MCP 태스크 시퀀스 (gathering-tasks.md)

### Phase 구성 (7개 Phase, ~136회 MCP 호출)

| Phase | 목적 | 예상 호출 |
|-------|------|---------|
| G-A | 16개 스크립트 생성 (데이터 레이어) | ~16회 |
| G-B | GatheringPoint + GatheringManager 생성 | ~4회 |
| G-C | SO 에셋 인스턴스 (Config/Item/Point/Price) | ~40회 |
| G-D | 씬 배치 (Manager + Zone별 Point) | ~20회 |
| G-E | 기존 시스템 확장 7건 | ~14회 |
| G-F | UI 연동 (프롬프트/결과/토스트) | ~18회 |
| G-G | 통합 검증 12개 항목 | ~24회 |

패턴: facilities-tasks.md (ARC-007), fishing-tasks.md (ARC-028) 참조

---

## DES-017: 채집 낫 업그레이드 상세 설계

### 핵심 발견: 강화/전설 낫 ROI 투자 회수 불가

| 등급 | 총 투자 | 일당 수입 향상 | 투자 회수 기간 |
|------|--------|-------------|-------------|
| 기본 낫 | 200G | +6.6G/일 (1.075x) | ~30일 (합리적) |
| 강화 낫 | 1,030G | +2.2G/일 (+0.025x) | ~468일 (불가) |
| 전설 낫 | 3,080G | +4.4G/일 (+0.05x) | ~350일 (불가) |

**의도된 설계**: 업그레이드 동기는 경제적 ROI가 아니라 ①품질 등급 해금 ②채집 도감 완성 ③성장 피드백 ④자원 순환 루프(광석→낫→채집). [OPEN] 강화 낫 비용 하향(500G) 또는 Gold 확률 상향 검토 필요.

### 재료 수급 전략

- **구리 광석 x3**: 동굴 채집 15~20일 or 여행 상인 100G/세트. 낚싯대 강화(x5)와 경쟁 — 총 8개 필요
- **금 광석 x2**: 동굴 채집 40~50일 or 여행 상인+보물 상자 혼합 25~35일. 낚싯대 전설(x3)과 경쟁

### 통합 방안 결정

채집 낫을 ToolData SO에 통합하되, ToolUpgradeRecipe.levelReq가 채집 숙련도를 지원하지 못하는 설계 부채 발견 → `levelReqType` enum 필드 추가 필요 (FIX-086 신규 등록).

---

*이 문서는 Claude Code가 ARC-032 + DES-017 작업에 따라 자율적으로 작성했습니다.*
