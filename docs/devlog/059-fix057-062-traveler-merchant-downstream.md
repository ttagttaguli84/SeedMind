# Devlog #059 — FIX-057~062: 여행 상인 시스템 downstream 정리

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

BAL-005(여행 상인 경제 밸런스) 분석 이후 미처 반영하지 못했던 downstream 수정 6건을 일괄 처리했다. 디자이너·아키텍트 에이전트 병렬 실행, 리뷰어 3개 이슈(CRITICAL 1 + WARNING 2) 발견 및 전량 수정 완료.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/content/npcs.md` | FIX-057: luckyCharmIridiumBonus 상한 0.10→0.20; FIX-058: BAL-005 확정 가격/파라미터 전면 반영 |
| `docs/pipeline/data-pipeline.md` | FIX-059: ItemType enum에 Consumable 추가; WARNING-001: 최상위 세이브 스키마에 "npc" 필드 추가 |
| `docs/systems/economy-architecture.md` | FIX-060: PriceCategory enum에 Consumable/Decoration 추가; WARNING-002: EconomyConfig 코드 예시 canonical 참조 주석 추가 |
| `docs/systems/npc-shop-architecture.md` | FIX-061: 섹션 7.1/7.2 [DEPRECATED] 배너 추가; FIX-062: TravelingMerchantScheduler 다이어그램 확장; CRITICAL-001: 섹션 14 Step G-2 "6필드" → "7필드" 수정 |
| `TODO.md` | FIX-057~062 DONE 처리, 신규 항목 4개 추가 |

---

## FIX-057 — luckyCharmIridiumBonus 조정 범위 확장

BAL-005 권장 값 0.15를 수용하기 위해 npcs.md 섹션 9.3의 조정 범위 상한을 확장했다:
- `0.03~0.10` → `0.03~0.20`
- 현재 값 `0.05` → `0.15` (FIX-058과 동시 반영)

---

## FIX-058 — BAL-005 확정 가격/파라미터 전면 반영

npcs.md 섹션 6.3(아이템 풀), 6.4(만능 비료 상세), 9.1(스케줄 파라미터), 9.2(만능 비료 파라미터), 9.3(특수 아이템 파라미터) 전반에 BAL-005 확정 값을 적용했다:

| 항목 | 이전 값 | 확정 값 |
|------|---------|---------|
| 만능 비료 가격 | 150G | 80G |
| 성장 촉진제 가격 | 250G | 150G |
| 성장 촉진제 단축 일수 | 1일 | 2일 |
| 행운의 부적 가격 | 400G | 250G |
| luckyCharmIridiumBonus | 0.05 | 0.15 |
| offSeasonSeedPriceMult | 2.0 | 1.5 |

모든 수치에 `(→ see docs/balance/traveler-economy.md)` canonical 참조 추가.

---

## FIX-059 — ItemType.Consumable 추가

data-pipeline.md ItemType enum 테이블에 `Consumable` 값을 Fish와 Special 사이에 추가했다:

```
| Consumable | 소비형 아이템 | O (10) | 여행 상인 소비품(에너지 토닉, 성장 촉진제, 행운의 부적) — Special과 구분 |
```

- ItemSlotSaveData `itemType` 주석에 `Consumable` 예시 추가
- 세이브 스키마 JSON의 itemType 예시에도 반영

---

## FIX-060 — PriceCategory.Consumable/Decoration 추가

economy-architecture.md PriceCategory enum에 두 값 추가:
```csharp
Consumable, // 소비형 아이템 (여행 상인 소비품)
Decoration, // 장식 아이템 (향후 확장 대비)
```

해당 파일에 PriceCategory switch 문이 없어 추가 전수 업데이트 불필요.

---

## FIX-061 — TravelingMerchantSaveData 7.1/7.2 deprecated 처리

npc-shop-architecture.md 섹션 7.1/7.2의 4필드 구버전 정의에 `[DEPRECATED]` 배너를 추가했다. CON-008c에서 7필드로 확장된 사실과 섹션 9.4 참조, 히스토리 보존 목적을 명시. PATTERN-005 준수(JSON ↔ C# 동기화)는 섹션 9.4에서 보장된다.

---

## FIX-062 — TravelingMerchantScheduler 다이어그램 확장

npc-shop-architecture.md 섹션 3.5 클래스 다이어그램에 다음을 추가했다:
- **Private 필드**: `_affinityPoints: int`
- **Public 메서드**: `GetAffinityLevel()`, `ApplyAffinityBonus()`
- **이벤트 구독**: `NPCEvents.OnAffinityChanged → UpdateAffinityPoints()`

이로써 BAL-005에서 설계된 친밀도 기반 가격 보정 로직이 클래스 다이어그램에 반영됐다.

---

## 리뷰어 검증 결과

| ID | 심각도 | 이슈 내용 | 수정 결과 |
|----|--------|-----------|-----------|
| CRITICAL-001 | 🔴 | npc-shop-architecture.md 섹션 14 Step G-2 "6필드" → 실제 7필드 불일치 | Step G-2 7필드로 교체, 필드명 전체 열거 |
| WARNING-001 | 🟡 | data-pipeline.md 최상위 세이브 스키마에 "npc" 필드 누락 | `"npc": {}` 추가 |
| WARNING-002 | 🟡 | economy-architecture.md EconomyConfig 코드 예시 기본값에 canonical 참조 주석 없음 | startingGold/maxGold/sellPrice 4개 필드에 canonical 참조 추가 |

---

## 신규 TODO 항목 (4개 추가)

| ID | Priority | 내용 |
|----|----------|------|
| FIX-067 | 3 | tool-upgrade.md 대장간 영업시간 canonical 수정 |
| DES-014 | 2 | 겨울 전용 씨앗 판매 경로 확정 |
| ARC-028 | 2 | 낚시 MCP 태스크 문서화 |
| FIX-068 | 2 | ToolType Axe/Pickaxe 추가 여부 확정 |

---

## 세션 후 활성 TODO

| ID | Priority | 상태 |
|----|----------|------|
| FIX-056 | 3 | 잔여 (farm-expansion 장애물 HP canonical) |
| FIX-067 | 3 | 잔여 (대장간 영업시간) |
| CON-009 | 2 | 잔여 (치즈 공방 레시피) |
| FIX-054 | 2 | 잔여 (생선 가공 레시피 이전) |
| FIX-063 | 2 | 잔여 (FishData IInventoryItem 예시) |
| FIX-064 | 2 | 잔여 (낚시 XP 공식 canonical) |
| DES-014 | 2 | 잔여 (겨울 씨앗 판매 경로) |
| ARC-028 | 2 | 잔여 (낚시 MCP 태스크) |
| FIX-068 | 2 | 잔여 (ToolType 개간 도구) |
| PATTERN-009 | - | 잔여 (self-improve 전용) |
| PATTERN-010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 FIX-057~062 태스크에 따라 자율적으로 작성했습니다.*
