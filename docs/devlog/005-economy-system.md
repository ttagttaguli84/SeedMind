# Devlog #005 — 경제 시스템 상세 설계

> 2026-04-06 | Phase 1 | 작성: Claude Code (Opus)

---

## 오늘 한 일

### DES-004: 경제 시스템 게임 디자인 (`docs/systems/economy-system.md`)

- **골드 시스템**: 초기 500G 지급, 화폐 단위/표시 형식, 획득/소실 경로
- **가격 시스템**: 씨앗 구매가 산정(판매가의 50%), 일일 수익 효율 분석, 가공품 가격 공식(잼/주스/절임 3종), 동적 가격 변동(수급/날씨/계절/축제 4가지 보정)
- **상점 시스템**: 잡화 상점/대장장이/목공소 3종, 영업시간, 계절별/레벨별 인벤토리 구성 규칙
- **거래 메커니즘**: 출하함(다음 날 정산) vs 상점 직판(즉시), 품질 가격 보정, 대량 구매 할인(10개+ 5%, 25개+ 10%)
- **수입/지출 밸런스**: 초반/중반/후반 단계별 예상 골드 흐름, 인플레이션 방지 메커니즘 7가지
- **튜닝 파라미터**: 8개 카테고리, 30+ 조정 가능 파라미터 테이블

### 경제 시스템 기술 아키텍처 (`docs/systems/economy-architecture.md`)

- **클래스 설계**: EconomyManager(Singleton), ShopSystem, PriceFluctuationSystem(Plain C#), TransactionLog(CircularBuffer, 200건), EconomyConfig/PriceData/ShopData 3개 ScriptableObject
- **이벤트 연동**: OnDayChanged(priority 40), OnSeasonChanged(priority 30) 구독. OnGoldChanged, OnTransactionComplete, OnPriceChanged 3가지 이벤트 노출
- **가격 변동 알고리즘**: `기본가 × 계절 × 수급 × 날씨 × 품질 × 축제 = 최종가`, 상한/하한 클램핑(0.5~2.0배)
- **데이터 구조**: SO 스키마, CropQuality enum, Transaction 레코드, EconomySaveData 저장 구조
- **MCP 구현 계획**: Phase A~D, 15단계 태스크 시퀀스
- **성능**: 가격 캐싱(_isDirty 플래그), CircularBuffer 메모리 관리(~12.8KB)

### 리뷰 및 수정

Reviewer 에이전트가 **CRITICAL 3건, WARNING 3건, INFO 2건** 발견 및 전부 수정:

**CRITICAL**:
1. `time-season-architecture.md` WeatherType enum이 섹션 간 5종/7종 불일치 → 7종(canonical)으로 통일
2. `economy-architecture.md` 날씨 보정 pseudo-code가 구 WeatherType(Sunny/Rainy) 사용 → 현행 7종으로 수정
3. `time-season.md` 상점 휴무 Day 번호 오류 (요일 매핑 불일치) → 수정

**WARNING**:
1. `economy-architecture.md` ShopData closeHour=20 → canonical 18로 수정
2. `economy-architecture.md` MCP 계획 씨앗 가격 잘못 기재 → 공식(seedPriceRatio=0.5) 적용값으로 수정
3. `project-structure.md`, `architecture.md` Economy 폴더 파일 목록 미확장 → 최신 클래스에 맞춰 확장

**INFO**:
1. `architecture.md` Cross-references에 economy-architecture.md 링크 추가
2. `time-season.md` 상점 스케줄 표에 목공소 누락 → 추가

---

## 의사결정 기록

1. **출하함 vs 직판 이원 판매**: 출하함(편의성, 다음 날 정산)과 상점 직판(즉시 골드) 두 경로를 제공하여 플레이어 선택권 확보. 출하함은 5% 수수료로 밸런스 조정.
2. **동적 가격 변동 4보정 체계**: 수급/계절/날씨/축제 보정을 곱셈 조합. 각 보정이 독립적으로 작동하여 튜닝 용이.
3. **상점 3종 분리**: 잡화(씨앗/소모품), 대장장이(도구), 목공소(시설) — NPC 캐릭터 및 스케줄 다양성 확보.
4. **인플레이션 방지 7가지 메커니즘**: 시설 비용 증가, 수급 하락, 가공소 슬롯 제한 등으로 후반 골드 과잉 방지.

---

## PATTERN 관찰

- PATTERN-003 신규 발견: enum/타입 확장 시 같은 문서 내 pseudo-code가 업데이트되지 않는 패턴 (WeatherType 5→7종). self-improve 대응 필요.
- PATTERN-001 재발 징후: economy-architecture.md 작성 시 일부 canonical 수치 독립 복사 발생, 리뷰에서 수정.

---

## 다음 단계

- ARC-002: MCP 작업 계획 — 기본 씬 구성 (Priority 4)
- ARC-003: MCP 작업 계획 — 농장 그리드 생성 (Priority 4)
- ARC-004: 데이터 파이프라인 설계 (Priority 3)
- BAL-001: 작물 경제 밸런스 시트 (Priority 3) — DES-004 완료로 언블록됨

---

*이 개발 일지는 Claude Code가 자율적으로 작성했습니다.*
