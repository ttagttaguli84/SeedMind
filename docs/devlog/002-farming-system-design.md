# Devlog #002 — 경작 시스템 상세 설계

> 2026-04-06 | Phase 1 | 작성: Claude Code (Opus)

---

## 오늘 한 일

### DES-001: 경작 시스템 게임 디자인 (`docs/systems/farming-system.md`)

- **타일 상태 머신**: 8개 상태(Empty, Tilled, Planted, Watered, Dry, Harvestable, Withered, Building) 및 전환 규칙 상세화
- **도구 인터랙션**: 호미/물뿌리개/씨앗봉투/낫/손 5종 도구의 사용 조건, 에너지 소모, 쿨다운, 에러 피드백
- **에너지 시스템**: 최대 100, 도구별 소모량, 고갈 패널티
- **물/비료 메카닉**: 4종 비료, 중첩 불가, 성장 속도 공식, 수확 품질 4등급
- **토양 품질**: 4등급(Poor~Rich), 연작 피해, 휴경 보너스
- **도구 업그레이드**: 5등급(기본~이리듐), 도구별 범위/효과 표
- **날씨 연동**: 5종 날씨, 비 자동 물주기, 폭풍 피해
- **실패 조건**: 3일 건조 고사, 계절 불일치, 폭풍 피해
- **플레이어 경험**: 시각/청각 피드백 설계, 연속 수확 콤보

### ARC-001: Unity 프로젝트 구조 (`docs/systems/project-structure.md`)

- `_Project/` 중간 폴더 도입, 7개 네임스페이스, Assembly Definition 설계
- 의존성 규칙: Core(최하층) → Farm/Player/Level → Economy/Building → UI(최상층)
- 3개 빌드 씬 + 2개 테스트 씬, 상점은 UI 오버레이
- 에셋 네이밍 접두어 체계 (PFB_, M_, SO_ 등)

### ARC-005: 경작 시스템 기술 아키텍처 (`docs/systems/farming-architecture.md`)

- 클래스 다이어그램 (FarmGrid → FarmTile → CropInstance)
- ScriptableObject 스키마 (CropData, FertilizerData, ToolData)
- 매일 아침 배치 처리 성장 전략
- 정적 이벤트 허브 패턴 (FarmEvents)
- MCP 구현 3단계 계획 (Phase A~C)

### 리뷰 및 수정

Reviewer 에이전트가 발견한 **HIGH 4건, MEDIUM 5건, LOW 3건** 전부 수정:
- 타일 상태 수/이름 통일 (Growing → Dry + Withered 추가)
- 고사 임계값 7일 → 3일 수정
- 고사 후 전환 대상 Empty → Tilled 수정
- 도구 등급 3단계 → 5단계 수정
- 비료 배수 2.0 → 1.25 수정
- Shop.unity 제거 (UI 오버레이로 결정)
- 마스터 문서에 표준 섹션 추가

---

## 의사결정 기록

1. **타일 상태에 Dry와 Withered 분리**: Growing 하나로 퉁치면 "물이 필요한 상태"와 "죽어가는 상태"의 구분이 안 됨. 플레이어 피드백 명확성을 위해 분리.
2. **도구 5등급 체계**: Stardew Valley 참고. 충분한 업그레이드 단계가 장기 목표를 제공.
3. **_Project/ 폴더 도입**: Unity 패키지 에셋과 프로젝트 에셋 분리를 위한 업계 관행.

---

## 다음 단계

- DES-002: 작물 성장 시스템 상세 (Priority 5)
- DES-003: 시간/계절 시스템 (Priority 4)
- ARC-002/003: MCP 작업 계획 (Priority 4)

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
