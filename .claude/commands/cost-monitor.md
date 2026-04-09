---
description: Analyze recent session token costs, detect regressions, and apply targeted optimizations to start.md and related command files.
---

You are the **cost monitor** for SeedMind. Your job is to analyze JSONL session logs, detect regressions or waste patterns, and apply targeted improvements to `.claude/commands/start.md` and related rule files.

## Step 1 — Collect JSONL Session Logs

Scan `logs/run_*.jsonl` for unanalyzed session logs.

If no files exist, print "분석할 세션 로그 없음" and stop.

Also read any prior analysis reports (`docs/reports/cost-monitor-analysis-*.md`) for trend comparison in Step 4.

## Step 2 — Analyze Sessions

Run the following Python script via the Bash tool to extract metrics from all JSONL files:

```python
import json, sys
from pathlib import Path

PRICE_INP = 15.0
PRICE_CC  = 18.75
PRICE_CR  = 1.50
PRICE_OUT = 75.0

for fp in sorted(Path(p) for p in sys.argv[1:]):
    if not fp.exists():
        print(f"SKIP: {fp}")
        continue
    turns, seen = [], set()
    model, stop_reason, cost_from_result, error_results = "unknown", "unknown", None, 0
    try:
        with open(fp, encoding='utf-8') as f:
            for line in f:
                line = line.strip()
                if not line: continue
                try: d = json.loads(line)
                except: continue
                if d.get('type') == 'system' and d.get('subtype') == 'init':
                    model = d.get('model', 'unknown')
                elif d.get('type') == 'assistant':
                    mid = d.get('message', {}).get('id')
                    if mid and mid in seen: continue
                    if mid: seen.add(mid)
                    u = d.get('message', {}).get('usage', {})
                    turns.append({'cc': u.get('cache_creation_input_tokens', 0),
                                  'cr': u.get('cache_read_input_tokens', 0),
                                  'inp': u.get('input_tokens', 0),
                                  'out': u.get('output_tokens', 0)})
                elif d.get('type') == 'result':
                    sr = d.get('stop_reason', '')
                    if sr: stop_reason = sr
                    if sr == 'error': error_results += 1
                    v = d.get('total_cost_usd')
                    if v is not None: cost_from_result = v
    except OSError as e:
        print(f"SKIP: {fp} — {e}"); continue

    ti = sum(t['inp'] for t in turns)
    tc = sum(t['cc'] for t in turns)
    tr = sum(t['cr'] for t in turns)
    to = sum(t['out'] for t in turns)
    tt = ti + tc + tr
    cost = cost_from_result if cost_from_result is not None else (ti*PRICE_INP + tc*PRICE_CC + tr*PRICE_CR + to*PRICE_OUT)/1e6
    print(f"{fp.name}|{len(turns)}|{cost:.4f}|{tc/tt*100 if tt else 0:.1f}|{tr/tt*100 if tt else 0:.1f}|{to}|{sum(1 for t in turns if t['cc']>50000)}|{sum(1 for t in turns if t['cc']>200000)}|{sum(1 for t in turns if t['out']>5000)}|{stop_reason}|{error_results}|{model}")
```

Pass all `logs/run_*.jsonl` file paths as arguments. Parse the pipe-delimited output to build the session detail table.

## Step 3 — Aggregate & Detect Anomalies

Check every metric against thresholds:

| Metric | Warning | Critical |
|--------|---------|----------|
| Average cost per session | > $6.00 | > $10.00 |
| Highest-cost session | > $10.00 | > $15.00 |
| cache_creation % | > 12% | > 18% |
| cache_read % | < 50% | < 20% |
| output_tokens per session (avg) | > 30,000 | > 60,000 |
| stop_reason == error rate | > 5% | > 15% |
| Sessions exceeding $6.00 | > 3 of 10 | > 6 of 10 |

Per-session interpretation:
- 3+ agent spawns → Session Task Budget overrun
- cc > 200,000 in single turn → large document injection
- Total turns > 300 → loop overrun
- cr% < 50% → cache instability

## Step 4 — Root Cause Analysis

Read `.claude/commands/start.md` to confirm active rules.

| Symptom | Likely cause in start.md |
|---------|--------------------------|
| Rising average cost | Session Task Budget relaxed |
| High cc%, low cr% | Agent Context Injection violated |
| High-cost + many spawns | DES-* parallel spawns |
| High turn count | Reviewer scope not enforced |
| High output_tokens | Large doc generation |
| High error rate | Context overflow / rate limit loops |

Compare against prior analysis reports if they exist. State the failure point or "상관관계 없음".

## Step 5 — Apply Optimizations (only if warranted)

**Decision criteria:**
- Same threshold exceeded in 2+ analyses → tighten rule
- First occurrence → report only, no edits
- All normal → report only

Maximum one file edited per run. Allowed: `start.md`, `CLAUDE.md`, `workflow.md`.
Forbidden: `doc-standards.md`, `docs/`, `TODO.md`, the 14 Reviewer Checklist items.

## Step 6 — Write Analysis Report

Write to `docs/reports/cost-monitor-analysis-<YYYYMMDD>.md`. If exists, append `-2`, `-3`, etc.

```markdown
# Cost Monitor 분석 리포트 — <date>

## 분석 대상
- 데이터 소스: logs/run_*.jsonl N개
- 총 세션 수: N개 / 총 비용: $X.XX

## 세션별 요약
| 파일 | 모델 | 턴 | 비용 | cc% | cr% | output | spawns | stop | error |
|------|------|-----|------|-----|-----|--------|--------|------|-------|

## 임계치 체크
| Metric | 값 | 임계치 | 상태 |
|--------|-----|--------|------|

## 이상 감지
<초과 항목 또는 "이상 없음">

## 추정 원인

## 이전 분석 대비 트렌드
<이전 분석과 비교, 없으면 "첫 실행">

## 조치 내용
<편집 내용 또는 "편집 없음">
```

## Step 7 — Archive & Commit

Move analyzed JSONL files to `logs/archive/`:

```bash
mkdir -p logs/archive
mv logs/run_*.jsonl logs/archive/
```

Commit the analysis report (and edited rule files if any):

```bash
git add docs/reports/cost-monitor-analysis-<YYYYMMDD>.md
git commit -m "CHORE: cost-monitor 분석 — <한 줄 요약>"
git push
```

## Rules
- All output in Korean (technical terms in English where natural).
- Be conservative: do not edit unless data clearly and repeatedly supports it.
- Maximum one file edited per run.
