# Devlog #068 — CON-011 + ARC-030: 낚시 도감 콘텐츠 및 아키텍처 설계

> 2026-04-07 | Phase 1 | 작성: Claude Code (Sonnet 4.6)

---

## 오늘 한 일

이번 세션은 CON-011(낚시 도감 콘텐츠)과 ARC-030(낚시 도감 아키텍처)을 완료했다. Designer 에이전트가 `fish-catalog.md`를 신규 작성하고, Architect 에이전트가 `fishing-architecture.md`에 Part VII를 추가했다. Reviewer가 CRITICAL 2건(JSON/C# 불일치, 크기 모델 불일치)을 발견하여 즉시 수정했으며, 특히 FIX-075 크기 시스템 데이터 모델 충돌을 같은 세션 내에 완전히 해소했다.

### 생성/수정된 파일

| 파일 | 변경 내용 |
|------|-----------|
| `docs/content/fish-catalog.md` | CON-011 신규: 크기 시스템, 15종 도감 항목, 마일스톤 보상, 도감 UI 정의 |
| `docs/systems/fishing-architecture.md` | ARC-030: Part VII (섹션 14~23) 추가 — FishCatalogData SO, FishCatalogManager, SizeRoll, FishCatalogSaveData, UI 아키텍처, MCP 태스크 Phase G~L |
| `docs/systems/save-load-architecture.md` | ARC-030: fishCatalog 필드 추가 (JSON/C#/SaveLoadOrder), animals JSON 필드 누락 수정 (Reviewer PATTERN-005) |
| `docs/pipeline/data-pipeline.md` | FIX-073: GameSaveData에 fishCatalog 필드 추가 |

---

## CON-011 낚시 도감 콘텐츠 확정

### 크기 시스템

| 크기 등급 | 범위 기준 | 판매가 보정 | 출현 비율 |
|----------|----------|------------|----------|
| 소형 | sizeMin ~ +range*0.5 | x0.9 | ~55% |
| 중형 | +range*0.5 ~ +range*0.8 | x1.0 | ~30% |
| 대형 | +range*0.8 ~ sizeMax | x1.15 | ~15% |

- Giant 변이(5%): sizeMax × giantSizeMultiplier로 고정, 크기 등급 판정과 별개
- 가중 분포: `weightedRandom = random^1.3` (소형 편향)

### 15종 어종 도감 항목

모든 어종의 기본 판매가/계절/시간 조건은 `fishing-system.md` 섹션 4.2 canonical 참조. 도감 문서(`fish-catalog.md` 섹션 3)에는 sizeMinCm/sizeMaxCm, 힌트 텍스트, 초회 등록 보상만 정의.

초회 등록 보상 (희귀도별):
- 일반: Gold 20G + XP 5
- 비범: Gold 40G + XP 10
- 희귀: Gold 70G + XP 15
- 전설: Gold 150G + XP 30

### 도감 마일스톤 보상

| 달성 | 보상 |
|------|------|
| 5종 완성 | 100G + 30 XP + 미끼 x10 |
| 10종 완성 | 300G + 80 XP + 고급미끼 x5 + 도감 배경 해금 |
| 15종 완성 | 500G + 150 XP + 프리미엄 배경 + 프레임 해금 |

---

## ARC-030 낚시 도감 아키텍처 확정

### FishCatalogData SO 주요 필드

```
fishId, displayName, hintLocked, hintUnlocked, rarityTier
sizeMinCm, sizeMaxCm  (→ canonical: fish-catalog.md 섹션 3.1)
giantSizeMultiplier   (→ canonical: fishing-system.md 섹션 4.4)
firstCatchGold, firstCatchXP  (→ canonical: fish-catalog.md 섹션 3.1)
catalogIcon (에디터 전용), sortOrder
```

### FishCatalogManager

- Singleton, ISaveable, SaveLoadOrder=53
- CheckMilestone 배열: `[5, 10, 15]` (3단계 확정)
- 구버전 세이브 마이그레이션: FishingStats.caughtByFishId 기반

### 이벤트 확장

`OnFishCaught` → `OnFishCaughtWithSize(fish, quality, sizeCm, isGiant)` 추가  
→ FishCatalogManager.RegisterCatch() → UI 갱신 → 마일스톤 체크

---

## CRITICAL 이슈 수정 (Reviewer 발견)

### R-01: [CRITICAL] 크기 시스템 데이터 모델 충돌 (FIX-075)

**이슈**: 두 문서가 서로 다른 크기 모델을 사용
- `fish-catalog.md`: `sizeMin`/`sizeMax` 절대값(cm)
- `fishing-architecture.md`: `baseSizeCm` × `sizeVarianceMin/Max` 배율

**추가 불일치**:
- 필드명: `hintText`/`descriptionText` vs `hintLocked`/`hintUnlocked`
- FishCatalogData SO에 `firstCatchGold`/`firstCatchXP` 미정의
- 가격 보정: 3등급 이산 vs 선형 Lerp

**해결 방안 (A채택)**: FishCatalogData SO를 절대값(sizeMinCm/sizeMaxCm) 방식으로 통일  
- 이유: fish-catalog.md가 canonical content document이므로 architecture가 맞춤
- 결과: fishing-architecture.md 섹션 15(SO 필드), 18(SizeRoll/GetSizePriceMultiplier) 전면 수정

### R-02: [CRITICAL] save-load-architecture.md JSON에 animals 필드 누락 (PATTERN-005)

**이슈**: C# 클래스에 `AnimalSaveData animals`가 있으나 JSON 예시에 `"animals"` 키 누락  
**수정 완료**: Reviewer가 JSON 스키마에 animals 블록 추가

### R-03: [WARNING] CheckMilestone 배열 불일치

**이슈**: architecture의 `milestones = [3,5,8,10,13,15]` vs 콘텐츠 확정 `[5,10,15]`  
**수정 완료**: Reviewer가 `[5,10,15]`로 수정

---

## Reviewer 검증 결과

| ID | 심각도 | 이슈 내용 | 수정 결과 |
|----|--------|-----------|-----------|
| R-01 | 🔴 CRITICAL | 크기 모델 불일치 (절대값 vs 배율, 필드명, 가격 모델) | FIX-075 적용 — 절대값 통일 완료 |
| R-02 | 🔴 CRITICAL | save-load-architecture.md JSON animals 필드 누락 | Reviewer 직접 수정 완료 |
| R-03 | 🟡 WARNING | CheckMilestone 배열 [3,5,8,10,13,15] vs [5,10,15] | Reviewer 직접 수정 완료 |
| R-04 | 🟡 WARNING | Cross-references "향후 작성" 표기 | Reviewer 직접 수정 완료 |
| R-05 | ℹ️ INFO | FIX ID 충돌 (fishing-architecture FIX-072 vs TODO FIX-072) | FIX-072b로 구분, 두 항목 명확화 |

---

## 세션 후 활성 TODO

| ID | Priority | 상태 |
|----|----------|------|
| DES-015 | 1 | 잔여 (낚싯대 업그레이드 재료 공급 경로) |
| FIX-072 | 1 | 잔여 (economy-system.md 낚시 수입 항목) |
| BAL-014 | 1 | 잔여 (낚시 숙련도 XP 밸런스) |
| DES-016 | 1 | 잔여 (채집 시스템 기본 설계) |
| PATTERN-009/010 | - | 잔여 (self-improve 전용) |

---

*이 문서는 Claude Code가 CON-011 + ARC-030 태스크에 따라 자율적으로 작성했습니다.*
