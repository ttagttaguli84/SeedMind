# Devlog #003 — 작물 성장 시스템 상세 설계

> 2026-04-06 | Phase 1 | 작성: Claude Code (Opus)

---

## 오늘 한 일

### DES-002: 작물 성장 시스템 게임 디자인 (`docs/systems/crop-growth.md`)

- **성장 단계 상세**: 4단계(Seed → Sprout → Growing → Harvestable) 정의, 전환 기준(33%/100% 진행률), 8종 작물별 로우폴리 시각 표현
- **성장 공식**: 누적 소수점 방식, `effectiveGrowth = base * fertilizer * soil * season`
- **계절별 재배 제한**: 8종 작물-계절 매핑, 계절 전환 시 즉시 고사, 온실 규칙(4×4, 계절/날씨 무시)
- **수확 메카닉**: 단일 수확 7종 + 다중 수확 1종(딸기, 3일 재수확), 품질 4등급 결정 공식
- **특수 작물**: 거대 작물(3×3, 15% 확률, 호박/수박), 교잡/돌연변이(5%, 레벨 7+)
- **실패 조건**: 3일 연속 Dry 고사, 계절 불일치, 폭풍 10% 성장 후퇴
- **시각/청각 피드백**: 성장/수확/고사별 파티클, 애니메이션, 사운드 설계
- **튜닝 파라미터**: 29개 데이터 드리븐 파라미터 정의

### 작물 성장 기술 아키텍처 (`docs/systems/crop-growth-architecture.md`)

- **클래스 다이어그램**: CropInstance, CropData SO, GrowthSystem, GiantCropInstance, Quality/GrowthResult enum
- **CropInstance**: 12개 필드, 핵심 메서드(Grow, DetermineQuality, ResetForReharvest)
- **CropData SO 확장**: isReharvestable, reharvestDays, giantCropPrefab 등 추가 필드
- **GrowthSystem**: OnDayChanged 구독, 배치 처리 파이프라인, 성장 공식 구현
- **품질 결정 알고리즘**: qualityScore 기반 확률 테이블, 시나리오별 분석 3가지
- **이벤트 시스템**: FarmEvents에 6개 신규 이벤트 추가
- **다중 수확**: Harvestable → Dry 전환, reharvestDays 재성장
- **거대 작물**: 3×3 탐색 알고리즘, 병합 로직, 수확량 = base × 9 × 2
- **MCP 구현 계획**: 3단계 15스텝
- **성능**: 256타일 배치 처리, 코루틴 분산(BATCH_SIZE=16), 메모리 ~300KB

### 리뷰 및 수정

Reviewer 에이전트가 **CRITICAL 7건, WARNING 2건** 발견 및 전부 수정:

**CRITICAL**:
1. architecture.md 타일 상태 7개 → 8개 통일 (Growing → Dry + Withered 분리)
2. crop-growth.md 비료 이름/배수를 farming-system.md canonical 기준으로 통일
3. 토양 등급명 Good → Fertile 통일 (farming-system.md canonical)
4. 토양 배수 0.75~1.5 → 0.9~1.2 통일 (farming-system.md canonical)
5. 토마토 다중 수확 → 단일 수확으로 수정 (crop-growth.md canonical)
6. 딸기 재수확 주기 2일 → 3일 수정
7. 거대 작물 확률 1% → 15% 통일

**WARNING**:
1. architecture.md 씬 파일명 SCN_ 접두어 추가 (project-structure.md canonical)
2. 당근 재배 계절 Spring → Spring,Autumn 수정

### PATTERN 등록

- PATTERN-001: 신규 문서가 canonical 수치를 독립 복사하여 불일치 반복 → doc-standards.md 규칙 강화 필요
- PATTERN-002: 동일 문서 내 섹션 간 수치 불일치 → 검증 체크리스트 필요

---

## 의사결정 기록

1. **누적 소수점 성장 방식**: 정수 일수 기반이 아닌 소수점 진행률 누적. 비료/토양 효과가 자연스럽게 반영되고, 성장 기간이 비정수일 때도 처리 가능.
2. **계절 전환 시 즉시 고사 (유예 없음)**: 전략적 판단을 강제하여 긴장감 부여. 사전 경고 시스템(D-3)으로 플레이어 불만 완화.
3. **딸기만 다중 수확**: 스코프 관리. 토마토 등 추가 후보는 밸런스 테스트 후 결정.
4. **거대 작물 15% 확률**: 너무 높으면 일상적이 되고, 너무 낮으면 존재감 없음. 15%는 의도적 도전 시 2~3번에 1번 성공하는 수준.

---

## 다음 단계

- DES-003: 시간/계절 시스템 (Priority 4)
- DES-004: 경제 시스템 (Priority 4)
- ARC-002/003: MCP 작업 계획 (Priority 4)

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
