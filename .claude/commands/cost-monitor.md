---
description: Analyze recent session token costs, detect regressions, and apply targeted optimizations to start.md and related command files.
---

You are the **cost monitor** for SeedMind. Your job is to read all unanalyzed cost reports in `logs/reports/`, detect regressions or waste patterns, and apply targeted improvements to `.claude/commands/start.md` and related rule files.

## Step 1 — Read All Cost Reports

Read every file matching `logs/reports/cost-monitor-*.md`. If no such files exist, print "분석할 리포트 없음" and stop.

Each report covers one batch of sessions (10개 단위). Extract from each report:
- 대상 기간 (date range)
- 총 세션 수
- 총 비용 / 세션 평균 비용
- 최고 비용 세션
- cache_creation 비율 (%)
- 세션별 상세 테이블 (파일명, 비용, stop_reason)

## Step 2 — Aggregate & Detect Anomalies

Combine all reports into a unified view, then check each threshold:

| Metric | Warning | Critical |
|--------|---------|----------|
| 세션 평균 비용 | > $6.00 | > $10.00 |
| 최고 비용 세션 | > $10.00 | > $15.00 |
| cache_creation % | > 12% | > 18% |
| 배치 내 Warning 초과 세션 수 | > 3/10 | > 6/10 |

For each threshold exceeded, record:
- 어느 배치(리포트)에서 발생했는가
- 어느 세션이 트리거했는가
- 추정 원인 (루프 과다, 대형 문서 주입, 병렬 에이전트 등)

**충분성 판단 — JSONL 폴백 조건:**

리포트만으로 분석이 충분하지 않은 경우:
- 배치가 1개뿐이라 추세 판단 불가
- Warning/Critical 임계치 초과 세션이 있으나 리포트 상세 테이블에 원인이 불명확
- 최고 비용 세션이 평균의 2배 이상이나 비용 외 데이터가 없음

이 경우 Step 2.5로 이동.

## Step 2.5 — JSONL 폴백 분석 (조건부)

Step 2에서 폴백 조건에 해당하는 경우에만 실행.

**분석 대상 JSONL 선택 기준:**
- 최고 비용 세션 → 해당 배치의 `logs/backup/<YYYYMMDD_HHMMSS>/run_*.jsonl` 중 해당 파일
- 평균 초과 세션이 여럿인 배치 → 해당 배치 폴더 전체 JSONL

**JSONL에서 추출할 정보:**

```python
import json

# 각 세션별 per-message 통계
for fp in target_jsonl_files:
    turns = []
    with open(fp) as f:
        for line in f:
            d = json.loads(line)
            if d.get('type') == 'assistant':
                u = d.get('message', {}).get('usage', {})
                cc = u.get('cache_creation_input_tokens', 0)
                cr = u.get('cache_read_input_tokens', 0)
                out = u.get('output_tokens', 0)
                # agent spawn 감지: cc가 갑자기 큰 턴
                turns.append({'cc': cc, 'cr': cr, 'out': out})
    # cc 급등 턴 = agent spawn 시점
    spikes = [i for i, t in enumerate(turns) if t['cc'] > 50_000]
    print(f"{fp.name}: {len(turns)} turns, {len(spikes)} agent spawns")
```

Run this with the Bash tool on the target files. Extract:
- 총 턴 수 (세션 길이)
- Agent spawn 횟수 (cc 급등 구간 수)
- 각 spawn 시점의 cc 크기 (주입 문서 규모 추정)
- output 집중 구간 (긴 문서 작성 = 높은 out)

**판단 기준:**
- Agent spawn 3회 이상 → Session Task Budget 초과 실행 의심
- 단일 spawn cc > 200,000 → 대형 문서 주입 의심
- 총 턴 > 300 → 루프 초과 실행 의심

## Step 3 — Root Cause Analysis

Cross-reference anomalies with current `start.md` rules:

| Symptom | Likely cause in start.md |
|---------|--------------------------|
| 평균 비용 상승 추세 | Session Task Budget 재완화 여부 확인 |
| 특정 배치에서 cc% 급등 | Agent Context Injection 규칙 위반 |
| 최고 비용 세션 반복 출현 | DES-* 순차 실행 미준수 (병렬 spawn) |
| Reviewer 턴 비정상적 증가 | Checklist 범위 한정 미준수 |

Read `start.md` to verify which rules are currently in place before drawing conclusions.

## Step 4 — Apply Optimizations (only if warranted)

Apply changes **only when data clearly supports them**. One root cause → one fix per run.

**Decision criteria:**
- 2개 이상 배치에서 동일 임계치 초과 → 해당 규칙 강화
- 단일 배치의 이상값 → 리포트에 기록만, 편집 없음
- 모든 임계치 정상 → 리포트만 작성, 편집 없음

**Allowed edits:**
- `start.md`: Session Task Budget, Agent Context Injection, Reviewer checklist scope
- `CLAUDE.md`: Agent 테이블 레이블
- `workflow.md`: Agent Collaboration 테이블, Reviewer Checklist 헤더

**Forbidden edits:**
- `doc-standards.md`, `docs/`, `TODO.md`, 설계 문서 일체
- Reviewer Checklist 14개 항목 자체 (scope 규칙만 변경 가능)

## Step 5 — Write Analysis Report

Write to `logs/reports/cost-monitor-analysis-<YYYYMMDD>.md`:

```markdown
# Cost Monitor 분석 리포트 — <date>

## 분석 대상
- 리포트 파일: cost-monitor-*.md N개
- 총 세션 수: N개 / 총 비용: $X.XX

## 배치별 요약
| 배치 | 기간 | 평균 비용 | 최고 비용 | cc% | 상태 |
|------|------|-----------|-----------|-----|------|
| cost-monitor-YYYYMMDD_HHMMSS.md | ... | $X.XX | $X.XX | X% | OK / WARNING / CRITICAL |

## 이상 감지
<임계치 초과 항목 목록, 없으면 "이상 없음">

## 추정 원인
<root cause 분석>

## 조치 내용
<편집한 파일과 변경 내용, 없으면 "편집 없음 — 모든 임계치 정상">
```

## Step 6 — Move Analyzed Reports to Backup

After writing the analysis report, move all `cost-monitor-*.md` files (분석 대상이었던 배치 리포트들) to their corresponding backup folders:

- `logs/reports/cost-monitor-YYYYMMDD_HHMMSS.md` → `logs/backup/YYYYMMDD_HHMMSS/cost-monitor-YYYYMMDD_HHMMSS.md`
- 백업 폴더가 없으면 생성

`cost-monitor-analysis-*.md`(분석 요약본)는 `logs/reports/`에 유지.

## Step 7 — Commit

```bash
git add logs/reports/ logs/backup/ .claude/commands/start.md .claude/rules/workflow.md CLAUDE.md
git commit -m "CHORE: cost-monitor 분석 — <한 줄 요약>"
git push
```

## Rules
- All output in Korean. Report content in Korean (technical terms in English where natural).
- 보수적으로 판단: 데이터가 명확하지 않으면 편집하지 않는다.
- 한 번 실행에 최대 1개 파일 편집.
