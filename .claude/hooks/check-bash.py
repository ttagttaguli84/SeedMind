"""
PreToolUse hook — blocks inefficient Bash patterns.
Exit 2 = block tool call and show message to Claude.
"""
import sys, json, re

data = json.load(sys.stdin)
# Strip heredoc bodies before scanning to avoid false positives in commit messages
command = re.sub(
    r"<<['\"]?\w+['\"]?\n.*?\nEOF\b",
    "<<HEREDOC",
    data.get("tool_input", {}).get("command", ""),
    flags=re.DOTALL,
)

FORBIDDEN = [
    # Prefer dedicated tools
    (re.compile(r"\bfind\b"),                         "Use Glob tool instead of `find`"),
    (re.compile(r"\bgrep\b"),                         "Use Grep tool instead of `grep`"),
    (re.compile(r"\brg\b"),                           "Use Grep tool instead of `rg`"),
    (re.compile(r"\bls\b(?!-files|-remote|-tree)"),   "Use Glob tool instead of `ls`"),
    (re.compile(r"\bcat\b(?!\s*<<)"),                  "Use Read tool instead of `cat`"),
    (re.compile(r"\bhead\b"),                         "Use Read tool with limit parameter instead of `head`"),
    (re.compile(r"\btail\b"),                         "Use Read tool with offset+limit instead of `tail`"),
    (re.compile(r"\bsed\b"),                          "Use Edit tool instead of `sed`"),
    (re.compile(r"\bawk\b"),                          "Use Grep or Edit tool instead of `awk`"),
    (re.compile(r"\bwc\b"),                           "Use Read tool instead of `wc`"),
    (re.compile(r"^\s*echo\s+.+>"),                   "Use Write tool instead of `echo >`"),
    (re.compile(r"^\s*cp\s"),                         "Use Read + Write tools instead of `cp`"),
    (re.compile(r"^\s*stat\b"),                       "Use Read or Glob tool instead of `stat`"),
    # Project-critical: absolute path cd bypasses worktree isolation (see workflow.md)
    (re.compile(r"cd\s+[\"']?(?:C[:/]|/c/)"),        "CRITICAL: Never cd to absolute path for git ops — run git from cwd. See workflow.md Git Working Directory rules."),
]

for pattern, message in FORBIDDEN:
    if pattern.search(command):
        print(f"BLOCKED: {message}. Command was: {command[:200]}", file=sys.stderr)
        sys.exit(2)

sys.exit(0)
