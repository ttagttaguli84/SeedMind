# Devlog #112 — DES-024 / ARC-044: 에너지 시스템 설계 및 아키텍처

> 작성: Claude Code (Sonnet 4.6) | 2026-04-08

---

## 작업 요약

**DES-024**: `docs/systems/energy-system.md` 신규 생성 — 에너지 시스템 canonical 설계 문서
**ARC-044**: `docs/systems/energy-architecture.md` 신규 생성 — 에너지 시스템 기술 아키텍처

farming/fishing/gathering/time-season 4개 문서에 분산되어 있던 에너지 소모 규칙을 단일 canonical 문서로 통합 확정.

---

## 신규 파일

| 파일 | 내용 |
|------|------|
| `docs/systems/energy-system.md` | 에너지 시스템 canonical 설계 (DES-024) |
| `docs/systems/energy-architecture.md` | 에너지 시스템 기술 아키텍처 (ARC-044) |

---

## 주요 확정 수치 (energy-system.md canonical)

| 항목 | 확정값 |
|------|--------|
| 기본 최대 에너지 | 100 |
| 레벨업 증가 (Lv.5/10/15/20) | +5씩, 최대 120 |
| 호미 기본 | 1 / 강화 1 / 전설 1 |
| 물뿌리개 기본 | 2 / 강화 1 / 전설 1 |
| 낫 기본 | 1 / 강화 1 / 전설 1 |
| 낚시 캐스팅 Lv.1~7 | 3 (BAL-024 확정 유지) |
| 낚시 캐스팅 Lv.8+ | 2 |
| 채집 (맨손/도구 Basic~Legend) | 0 / 1 / 1 / 1 |
| 수면 조기(20:00 전) | 100% + 보너스 (최대 +20) |
| 수면 일반·늦음(20:00~24:00) | 100% |
| 기절 회복 | 50% + 골드 5% 손실 |
| 에너지 경고 임계값 | ≤20 |
| 폭우 배수 | x1.1 |
| 폭풍 배수 | x1.25 |
| 야간(20:00~24:00) 배수 | x1.2 |

---

## 아키텍처 주요 결정 (energy-architecture.md)

- **EnergyManager**: `SeedMind.Player` 네임스페이스, MonoBehaviour Singleton
- **소모 통합**: IEnergyConsumer 인터페이스 대신 직접 API 호출 (`EnergyManager.TryConsume()`)
- **EnergyConfig SO**: 모든 27개 튜닝 파라미터 외부화, 수치는 energy-system.md 참조
- **EnergyEvents**: `OnEnergyChanged` 등 9개 정적 이벤트
- **SaveLoadOrder**: 51 할당 (PlayerController 50 직후)
- **연동 업데이트**: save-load-architecture.md SaveLoadOrder 51 추가, data-pipeline.md EnergyConfig SO 추가

---

## 리뷰 수정 사항 (CRITICAL 3건 + WARN 2건 수정 완료)

| 항목 | 파일 | 수정 내용 |
|------|------|----------|
| CRITICAL | energy-architecture.md | `IsDepletd` → `IsDepleted` 오타 수정 |
| CRITICAL | energy-system.md 섹션 7.1 | "≤20%" → "≤20 (energyWarningThreshold)" 절대값 통일 |
| CRITICAL | energy-system.md 섹션 9 | `fishingHighLevelThreshold` 파라미터 행 추가 (ARC 필드와 동기화) |
| WARN | energy-architecture.md 섹션 2.3 | `NormalSleep` 주석에 "일반·늦은 수면" 범위 명시 추가 |
| INFO | data-pipeline.md 섹션 2.3 | ToolData.energyCost canonical 참조 farming-system.md → energy-system.md 교체 |

---

## 후속 작업 (TODO 등록)

| ID | Priority | 내용 |
|----|----------|------|
| FIX-118 | 2 | 4개 원본 문서(farming/fishing/gathering/time-season) 에너지 수치를 energy-system.md canonical 참조로 교체 |
| ARC-047 | 1 | energy-tasks.md MCP 태스크 시퀀스 독립 문서화 |
| DES-025 | 2 | 음식/요리 아이템 에너지 회복 상세 설계 |

---

## Open Questions 신규

- **[OPEN]** 달리기 에너지 소모 설계 미결 (이동 시스템 미설계)
- **[OPEN]** 온천/휴식 시설 에너지 회복 경로 (시설 목록 미확정)
- **[OPEN]** 음식 아이템 구체 목록 및 회복량 (요리 시스템 설계 전)
- **[OPEN]** 레벨업 보너스 적용 레벨 — progression-curve.md와 동기화 필요
