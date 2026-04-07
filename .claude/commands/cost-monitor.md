---
description: Analyze recent session token costs, detect regressions, and apply targeted optimizations to start.md and related command files.
---

You are the **cost monitor** for SeedMind. Your job is to read all unanalyzed cost reports in `logs/reports/`, detect regressions or waste patterns, and apply targeted improvements to `.claude/commands/start.md` and related rule files.

## Step 1 — Read All Cost Reports

Read every file matching `logs/reports/cost-monitor-[0-9]*.md`. These are batch reports whose names begin with a date. Exclude any file matching `cost-monitor-analysis-*.md` — those are prior analysis summaries, not raw batch data.

If no matching batch report files exist, print "분석할 리포트 없음" and stop.

Each report covers one batch of sessions (typically 10 sessions). Extract from each report:
- Date range covered
- Total session count
- Total cost and average cost per session
- Highest-cost session (filename and cost)
- cache_creation token percentage (%)
- Per-session detail table (filename, cost, stop_reason)

## Step 2 — Aggregate & Detect Anomalies

Combine all extracted data into a unified view across all batches. Check every metric against the thresholds below:

| Metric | Warning | Critical |
|--------|---------|----------|
| Average cost per session | > $6.00 | > $10.00 |
| Highest-cost session | > $10.00 | > $15.00 |
| cache_creation % | > 12% | > 18% |
| Sessions exceeding Warning threshold within a batch | > 3 of 10 | > 6 of 10 |

For each threshold exceeded, record:
- Which batch (report file) triggered it
- Which session(s) triggered it
- Estimated cause (excessive loops, large document injection, parallel agent spawns, etc.)

**Sufficiency check — JSONL fallback conditions:**

Proceed to Step 2.5 if any of the following are true:
- Only one batch is available, making trend analysis impossible
- A Warning or Critical threshold was exceeded but the report detail table does not clarify the cause
- The highest-cost session is more than 2x the batch average but no breakdown data is available in the report

If none of these conditions apply, skip Step 2.5 and proceed directly to Step 3.

## Step 2.5 — JSONL Fallback Analysis (conditional)

Run this step only when Step 2's sufficiency check determined fallback is needed.

**Selecting target JSONL files:**
- For the single highest-cost session: locate the corresponding `logs/backup/<YYYYMMDD_HHMMSS>/run_*.jsonl` file from that session's batch folder.
- For batches with multiple above-average sessions: read all JSONL files in that batch's backup folder.
- If a JSONL file cannot be opened or is unreadable, skip it and note the filename in the analysis report. Do not abort the entire step.

**Run the following with the Bash tool:**

```python
import json, sys
from pathlib import Path

for path_str in sys.argv[1:]:
    fp = Path(path_str)
    if not fp.exists():
        print(f"SKIP (not found): {fp}")
        continue
    turns = []
    try:
        with open(fp, encoding='utf-8') as f:
            for line in f:
                line = line.strip()
                if not line:
                    continue
                try:
                    d = json.loads(line)
                except json.JSONDecodeError:
                    continue
                if d.get('type') == 'assistant':
                    u = d.get('message', {}).get('usage', {})
                    turns.append({
                        'cc': u.get('cache_creation_input_tokens', 0),
                        'cr': u.get('cache_read_input_tokens', 0),
                        'out': u.get('output_tokens', 0),
                    })
    except OSError as e:
        print(f"SKIP (unreadable): {fp} — {e}")
        continue
    spikes = [i for i, t in enumerate(turns) if t['cc'] > 50_000]
    large_injections = [i for i, t in enumerate(turns) if t['cc'] > 200_000]
    print(f"{fp.name}: {len(turns)} turns, {len(spikes)} agent spawns, "
          f"{len(large_injections)} large injections (cc>200k)")
```

From the output, extract:
- Total turn count (session length)
- Number of agent spawns (turns where cc > 50,000)
- cc size at each spawn (used to estimate injected document size)
- Output-heavy turns (high `out` values indicate large document generation)

**Interpretation thresholds:**
- 3 or more agent spawns → suspected Session Task Budget overrun
- Any single spawn with cc > 200,000 → suspected large document injection
- Total turns > 300 → suspected loop overrun

## Step 3 — Root Cause Analysis

Read `.claude/commands/start.md` first to confirm which rules are currently active before drawing any conclusions.

Cross-reference anomalies with `start.md` using the table below:

| Symptom | Likely cause in start.md |
|---------|--------------------------|
| Rising average cost trend across batches | Session Task Budget may have been relaxed |
| cc% spike in a specific batch | Agent Context Injection rule violated |
| Same session repeatedly appearing as highest-cost | DES-* sequential execution not enforced (parallel spawns) |
| Reviewer turn count abnormally high | Reviewer checklist scope limit not enforced |

State which specific rule in `start.md` appears to be the failure point, or state that no correlation was found.

## Step 4 — Apply Optimizations (only if warranted)

Apply edits **only when data clearly supports them**. Maximum one file edited per run.

**Decision criteria:**
- Same threshold exceeded across 2 or more batches → tighten the corresponding rule
- Anomaly appears in only one batch → record in report only, no edits
- All thresholds normal → write report only, no edits

**Allowed edits:**
- `start.md`: Session Task Budget, Agent Context Injection, Reviewer checklist scope
- `CLAUDE.md`: Agent table labels
- `workflow.md`: Agent Collaboration table, Reviewer Checklist header

**Forbidden edits (do not touch under any circumstances):**
- `doc-standards.md`
- Any file under `docs/`
- `TODO.md`
- The 14 Reviewer Checklist items themselves (only the scope enforcement rule around them may be changed)

## Step 5 — Write Analysis Report

Write to `logs/reports/cost-monitor-analysis-<YYYYMMDD>.md`. Use today's date for the filename. If a file with that name already exists, append a suffix (`-2`, `-3`, etc.) rather than overwriting.

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

After writing the analysis report, move each batch report file to its corresponding backup folder:

- Source: `logs/reports/cost-monitor-YYYYMMDD_HHMMSS.md`
- Destination: `logs/backup/YYYYMMDD_HHMMSS/cost-monitor-YYYYMMDD_HHMMSS.md`

The `YYYYMMDD_HHMMSS` segment in the destination path must match the segment in the source filename exactly.

If the destination folder does not exist, create it with `mkdir -p` before moving. If a file with the same name already exists at the destination, do not overwrite it — record a warning in the analysis report and leave the source file in place.

Do not move `cost-monitor-analysis-*.md` files. Those remain in `logs/reports/`.

## Step 7 — Commit

`.gitignore` rules for `logs/`:
- `logs/reports/*.md` and `logs/backup/**/*.md` — **tracked** (committed)
- `logs/backup/**/*.jsonl` and `logs/session_*.log` — ignored (not committed)

Always stage and commit the analysis report. If a rule file was also edited in Step 4, include it in the same commit.

```bash
# Always include the analysis report
git add logs/reports/cost-monitor-analysis-<YYYYMMDD>.md

# Add edited rule files only if Step 4 made changes
git add .claude/commands/start.md .claude/rules/workflow.md CLAUDE.md  # only if edited

git commit -m "CHORE: cost-monitor 분석 — <한 줄 요약>"
git push
```

## Rules
- All output in Korean. Report content in Korean (technical terms in English where natural).
- Be conservative: do not edit any file unless the data clearly and repeatedly supports it.
- Maximum one file edited per run.
