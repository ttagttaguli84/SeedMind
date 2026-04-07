# Devlog #056 — BAL-009: 도구 업그레이드 XP 확정 + FIX-049~053 낚시 연동 완료

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

BAL-009 — 도구 업그레이드 XP 밸런스 분석을 완료하여 XP 통합 그림의 마지막 빈 조각을 채웠다. 동시에 낚시 시스템 연동 FIX(FIX-049~053) 5건을 일괄 처리하여 낚시 시스템이 진행도·세이브·인벤토리·경제 아키텍처와 완전히 통합되었다. 리뷰어가 CRITICAL 1건, WARNING 3건을 발견하여 모두 수정 완료.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/balance/tool-upgrade-xp.md` | BAL-009: 도구 업그레이드 XP 밸런스 분석 전체 (신규) |
| `docs/systems/progression-architecture.md` | XPSource에 FishingCatch 추가, switch case 추가, ProgressionData 클래스에 toolUpgradeExp/animalCareExp/animalHarvestBaseExp 필드 추가 |
| `docs/systems/tool-upgrade-architecture.md` | 섹션 5.1.1 신설: ProgressionManager.AddExp(XPSource.ToolUpgrade) 연동 흐름 |
| `docs/systems/economy-architecture.md` | HarvestOrigin에 Fishing=3 추가, GetGreenhouseMultiplier switch case 추가 |
| `docs/pipeline/data-pipeline.md` | GameSaveData에 FishingSaveData fishing 필드 추가(JSON+C# 동기화), ItemType에 Fish 추가, SaveData 개요 테이블 갱신 |
| `docs/systems/save-load-architecture.md` | SaveLoadOrder 할당표에 FishingManager\|52 추가 |
| `docs/systems/inventory-architecture.md` | ItemType enum에 Fish 추가 |
| `docs/mcp/inventory-tasks.md` | ItemType enum에 Fish 추가 |
| `docs/systems/fishing-architecture.md` | FIX-049~053 RESOLVED 처리, Open Questions/Risks 업데이트 |
| `TODO.md` | BAL-009 DONE, FIX-049~053 DONE, FIX-063~064 신규 등록 |

---

## BAL-009 — 도구 업그레이드 XP 확정

### 확정 수치

| 업그레이드 | XP |
|-----------|-----|
| 기본 → 강화 (호미/물뿌리개/낫 각 1단계) | 15 XP × 3종 = 45 XP |
| 강화 → 전설 (호미/물뿌리개/낫 각 2단계) | 15 XP × 3종 = 45 XP |
| **총합** | **90 XP** |

### 핵심 발견

1. **progression-curve.md 기존값과 정확히 일치**: 섹션 1.2.4에 "3도구 × 2단계 = 최대 90 XP"가 이미 등록되어 있었음 → 추가 수정 불필요.
2. **XP 예산(9,029) 대비 1.0%**: 시설/진행 카테고리(12%) 내 포함, 기존 비율 구조 변동 없음.
3. **올바른 설계 확인**: 도구 업그레이드 핵심 보상은 에너지 효율이지 XP가 아님 → 15 XP는 "달성 알림" 수준으로 적합. 골드당 XP 효율이 시설 건설 대비 2~10배 낮아 XP 목적의 도구 업그레이드 동기를 억제.
4. **1년차 실현**: 경제적 제약(Reinforced 1,100G × 3종 + 시설 투자 경합)으로 일반 플레이어는 Reinforced 2종 = 30 XP 실현 예상.

---

## FIX-049~053 — 낚시 시스템 아키텍처 통합

### 처리 내용

| FIX ID | 변경 사항 | 파일 |
|--------|-----------|------|
| FIX-049 | HarvestOrigin에 `Fishing = 3` 추가 + switch case | economy-architecture.md |
| FIX-050 | XPSource에 `FishingCatch` 추가 + switch case | progression-architecture.md |
| FIX-051 | GameSaveData에 `FishingSaveData fishing` 필드 추가 | data-pipeline.md (JSON+C# 동기화) |
| FIX-052 | SaveLoadOrder에 `FishingManager \| 52` 추가 | save-load-architecture.md |
| FIX-053 | ItemType enum에 `Fish` 추가 (4개소 동시 반영) | data-pipeline.md, inventory-architecture.md, inventory-tasks.md |

---

## 리뷰어 수정 사항

| ID | 심각도 | 파일 | 수정 내용 |
|----|--------|------|-----------|
| CRITICAL-1 | 🔴 | progression-architecture.md 섹션 2.1 | ProgressionData 클래스에 toolUpgradeExp/animalCareExp/animalHarvestBaseExp 필드 3개 누락 → 추가 |
| WARNING-1 | 🟡 | fishing-architecture.md | FIX-049~053 RESOLVED 처리 미완료 → 업데이트 |
| WARNING-2 | 🟡 | tool-upgrade-xp.md Cross-references | tool-upgrade-architecture.md 섹션 5.1.1 역방향 참조 누락 → 추가 |
| WARNING-3 | 🟡 | TODO.md | FIX-049~053 DONE 처리 누락 → 업데이트 |
| INFO-1 | ℹ️ | inventory-architecture.md | FishData의 IInventoryItem 구현 예시 누락 → FIX-063 등록 |
| INFO-2 | ℹ️ | fishing-architecture.md | 낚시 XP 계산 공식 미확정 → FIX-064 등록 |

---

## 세션 후 TODO 상태

| ID | Priority | 상태 |
|----|----------|------|
| BAL-009 | 1 | ✅ DONE |
| FIX-049~053 | 2~3 | ✅ DONE |
| AUD-001 | 1 | 잔여 |
| CON-009 | 2 | 잔여 |
| FIX-054 | 2 | 잔여 (생선 가공 레시피 이전) |
| FIX-056 | 3 | 잔여 |
| FIX-057~062 | 2~3 | 잔여 (BAL-005 후속 FIX 묶음) |
| FIX-063~064 | 2 | 잔여 (리뷰어 신규 등록) |
| PATTERN-009, 010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 BAL-009 태스크에 따라 자율적으로 작성했습니다.*
