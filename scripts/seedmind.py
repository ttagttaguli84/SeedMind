"""
SeedMind Auto Runner
- /start를 반복 실행 (--print라서 매 실행마다 새 세션)
- 워크트리(wt-seedmind)에서 claude 실행
- tkinter GUI: Start / Stop 버튼
- 실시간 로그 뷰 (200줄 제한)
- Rate limit 시 자동 대기 후 재시작
"""

import subprocess, json, time, os, shutil
import tkinter as tk
from tkinter import scrolledtext
import threading
from datetime import datetime, timezone
from pathlib import Path

PROJECT_DIR = Path(__file__).absolute().parent.parent
LOG_DIR     = PROJECT_DIR / "logs"

MODELS        = ["claude-sonnet-4-6", "claude-opus-4-6", "claude-haiku-4-5-20251001"]
DEFAULT_MODEL = "claude-sonnet-4-6"
PROMPT        = "Output language: Korean only.\n/start"
RATE_LIMIT_BUF  = 30
RATE_LIMIT_MIN  = 60
MAX_LOG_LINES   = 200
INTER_RUN_DELAY    = 2
MAX_INLINE_TEXT_LEN = 120

WORKTREE_DIR = PROJECT_DIR / ".claude" / "worktrees" / "seedmind"

_NO_WIN   = {"creationflags": subprocess.CREATE_NO_WINDOW}
_GIT_LOCK = threading.Lock()


def _fmt_elapsed(td):
    s = int(td.total_seconds())
    return f"{s // 60}m{s % 60}s"


def _fmt_dhms(seconds):
    s = int(seconds)
    d, s = divmod(s, 86400)
    h, s = divmod(s, 3600)
    m, s = divmod(s, 60)
    parts = []
    if d: parts.append(f"{d}일")
    if h: parts.append(f"{h}시")
    if m: parts.append(f"{m}분")
    parts.append(f"{s}초")
    return " ".join(parts)


def _git_locked(cmd, cwd, timeout=30, env=None):
    with _GIT_LOCK:
        try:
            r = subprocess.run(
                ["git"] + cmd, cwd=str(cwd),
                stdout=subprocess.DEVNULL, stderr=subprocess.PIPE,
                text=True, encoding="utf-8", errors="replace",
                timeout=timeout, env=env, **_NO_WIN)
            return r.returncode, r.stderr.strip()
        except subprocess.TimeoutExpired:
            return 1, f"timeout after {timeout}s"


def _git_query(cmd, cwd, timeout=10):
    """stdout 결과가 필요한 git 명령용"""
    with _GIT_LOCK:
        try:
            r = subprocess.run(
                ["git"] + cmd, cwd=str(cwd),
                stdout=subprocess.PIPE, stderr=subprocess.PIPE,
                text=True, encoding="utf-8", errors="replace",
                timeout=timeout, **_NO_WIN)
            return r.returncode, r.stdout.strip()
        except subprocess.TimeoutExpired:
            return 1, ""


class App(tk.Tk):
    def __init__(self):
        super().__init__()
        self.title("SeedMind Runner")
        self.configure(bg="#1e1e2e")
        self.geometry("700x480")
        self.protocol("WM_DELETE_WINDOW", self._on_close)

        self._running = False
        self._stop_requested = False
        self._destroyed = False
        self._all_done = False
        self._proc = None
        self._thread = None
        self._rate_limit_resets_at = None
        self._model = DEFAULT_MODEL

        LOG_DIR.mkdir(exist_ok=True)
        session_log_path = LOG_DIR / f"session_{datetime.now():%Y%m%d_%H%M%S}.log"
        self._session_log = open(session_log_path, "w", encoding="utf-8", buffering=1)

        self._build_ui()

    # -- worktree ------------------------------------------------------------
    def _ensure_worktree(self):
        wt = WORKTREE_DIR
        if wt.exists():
            # 실제 git worktree인지 확인 (빈 디렉토리 등 무효 상태 검출)
            rc, _ = _git_query(["rev-parse", "--git-dir"], wt)
            if rc != 0:
                self._log_msg(f"[WT] 무효 디렉토리 감지 (git worktree 아님) — 제거 후 재생성")
                shutil.rmtree(str(wt), ignore_errors=True)
                _git_locked(["worktree", "prune"], PROJECT_DIR)
            else:
                # upstream이 origin/wt-seedmind인지 확인, 아니면 재설정
                rc, upstream = _git_query(
                    ["rev-parse", "--abbrev-ref", "--symbolic-full-name", "@{u}"], wt)
                if rc != 0 or upstream != "origin/wt-seedmind":
                    self._log_msg(f"[WT] upstream 재설정 중... (현재: {upstream or '없음'})")
                    _git_locked(["push", "-u", "origin", "wt-seedmind"], wt, timeout=30)
                return wt
        self._log_msg("[WT] worktree 생성 중...")
        _git_locked(["fetch", "origin", "main"], PROJECT_DIR, timeout=30)
        rc, err = _git_locked(
            ["worktree", "add", "--no-checkout", "-B", "wt-seedmind",
             str(wt), "origin/main"],
            PROJECT_DIR)
        if rc != 0 or not wt.exists():
            self._log_msg(f"[WT] worktree add 실패: {err[:80]}")
            return None
        env_skip = {**os.environ, "GIT_LFS_SKIP_SMUDGE": "1"}
        rc, err = _git_locked(["checkout"], wt, timeout=120, env=env_skip)
        if rc != 0:
            self._log_msg(f"[WT] checkout 실패: {err[:80]}")
            _git_locked(["worktree", "remove", "--force", str(wt)], PROJECT_DIR)
            return None
        # origin/wt-seedmind upstream 설정 (격리 브랜치로 push)
        rc, err = _git_locked(["push", "-u", "origin", "wt-seedmind"], wt, timeout=30)
        if rc != 0:
            self._log_msg(f"[WT] upstream 설정 실패: {err[:80]}")
        else:
            self._log_msg("[WT] origin/wt-seedmind upstream 설정 완료")
        self._log_msg("[WT] worktree 준비 완료")
        return wt

    def _sync_worktree(self, wt):
        """매 run 전 wt-seedmind를 origin/main으로 reset + 원격 브랜치도 동기화"""
        self._log_msg("[SYNC] origin/main → wt-seedmind reset 중...")
        _git_locked(["fetch", "origin", "main", "wt-seedmind"], PROJECT_DIR, timeout=30)
        rc, err = _git_locked(["reset", "--hard", "origin/main"], wt, timeout=30)
        if rc != 0:
            self._log_msg(f"[SYNC] ⚠️ reset 실패: {err[:80]}")
            return False
        # untracked 파일 제거 (이전 run 잔여물)
        _git_locked(["clean", "-fd"], wt, timeout=30)
        # 원격 wt-seedmind도 로컬과 일치시킴 (이전 세션 잔여 커밋 제거)
        rc, err = _git_locked(
            ["push", "--force", "origin", "wt-seedmind"], wt, timeout=30)
        if rc != 0:
            self._log_msg(f"[SYNC] ⚠️ remote sync 실패 (계속 진행): {err[:80]}")
        self._log_msg("[SYNC] ✓ 완료")
        return True

    def _check_all_done(self):
        """progress.md에 미완료(⬜) 항목이 없으면 True 반환 후 자동 종료."""
        progress_path = PROJECT_DIR / "docs" / "mcp" / "progress.md"
        try:
            text = progress_path.read_text(encoding="utf-8")
            if "⬜" not in text:
                self._log_msg("\n[DONE] ✅ 모든 MCP 태스크 완료 — 자동 종료합니다.")
                self._all_done = True
                self._safe_after(0, lambda: self._set_status("DONE ✅", "#a6e3a1"))
                return True
        except Exception as e:
            self._log_msg(f"[DONE] progress.md 읽기 실패: {e}")
        return False

    def _merge_to_main(self, wt):
        """run 후 wt-seedmind → main merge 후 push"""
        # 로컬 main을 origin/main으로 ff-only 업데이트 (run 중 외부 push 반영)
        rc, err = _git_locked(["merge", "--ff-only", "origin/main"], PROJECT_DIR, timeout=30)
        if rc != 0:
            self._log_msg(f"[MERGE] ⚠️ origin/main ff-only 실패 (local main diverged?): {err[:80]}")

        # 새 커밋이 있는지 확인 (없으면 스킵)
        rc, out = _git_query(["log", "wt-seedmind", "^main", "--oneline"], PROJECT_DIR)
        if rc != 0 or not out:
            return

        self._log_msg(f"[MERGE] wt-seedmind → main ({len(out.splitlines())}개 커밋) merge 중...")

        # wt-seedmind push (origin 백업)
        rc, err = _git_locked(["push"], wt, timeout=30)
        if rc != 0:
            self._log_msg(f"[MERGE] wt push 실패 (계속 진행): {err[:80]}")

        # main으로 merge
        rc, err = _git_locked(["merge", "wt-seedmind", "--no-edit"], PROJECT_DIR, timeout=30)
        if rc != 0:
            self._log_msg(f"[MERGE] ⚠️ main merge 실패 — abort 후 수동 처리 필요: {err[:80]}")
            _git_locked(["merge", "--abort"], PROJECT_DIR)
            return

        # main push
        rc, err = _git_locked(["push"], PROJECT_DIR, timeout=30)
        if rc != 0:
            self._log_msg(f"[MERGE] ⚠️ main push 실패: {err[:80]}")
            return

        self._log_msg("[MERGE] ✓ main 반영 완료")

    # -- UI ------------------------------------------------------------------
    def _build_ui(self):
        bar = tk.Frame(self, bg="#1e1e2e")
        bar.pack(fill="x", padx=8, pady=(8, 4))

        self._btn_start = tk.Button(
            bar, text="Start", bg="#a6e3a1", fg="#1e1e2e",
            font=("Segoe UI", 10, "bold"), relief="flat", width=10,
            command=self._on_start)
        self._btn_start.pack(side="left")

        self._btn_stop = tk.Button(
            bar, text="Stop", bg="#f38ba8", fg="#1e1e2e",
            font=("Segoe UI", 10, "bold"), relief="flat", width=10,
            state="disabled", command=self._on_stop)
        self._btn_stop.pack(side="left", padx=(6, 0))

        self._model_var = tk.StringVar(value=DEFAULT_MODEL)
        self._model_menu = tk.OptionMenu(bar, self._model_var, *MODELS)
        self._model_menu.config(
            bg="#313244", fg="#cdd6f4", font=("Segoe UI", 9),
            relief="flat", highlightthickness=0, width=20)
        self._model_menu["menu"].config(bg="#313244", fg="#cdd6f4")
        self._model_menu.pack(side="left", padx=(6, 0))

        self._lbl_status = tk.Label(
            bar, text="IDLE", bg="#1e1e2e", fg="#6c7086",
            font=("Segoe UI", 9))
        self._lbl_status.pack(side="left", padx=(12, 0))

        self._log = scrolledtext.ScrolledText(
            self, bg="#11111b", fg="#cdd6f4",
            font=("Consolas", 9), relief="flat", state="disabled",
            wrap="word")
        self._log.pack(fill="both", expand=True, padx=8, pady=(0, 8))

    # -- actions -------------------------------------------------------------
    def _on_start(self):
        if self._running:
            return
        self._running = True
        self._stop_requested = False
        self._all_done = False
        self._rate_limit_resets_at = None
        self._model = self._model_var.get()
        self._btn_start.config(state="disabled")
        self._btn_stop.config(state="normal")
        self._model_menu.config(state="disabled")
        self._set_status("RUNNING", "#a6e3a1")
        self._thread = threading.Thread(target=self._loop, daemon=True)
        self._thread.start()

    def _on_stop(self):
        if not self._running:
            return
        self._stop_requested = True
        self._btn_stop.config(state="disabled")
        self._set_status("STOPPING...", "#f9e2af")
        self._log_msg("[STOP] 현재 실행 완료 후 종료합니다...")

    def _on_close(self):
        self._destroyed = True  # _safe_after/_append_log 가드 — destroy() 전에 반드시 먼저 설정
        self._stop_requested = True
        proc = self._proc  # 로컬 캡처로 워커 스레드와의 재할당 경쟁 방지
        if proc and proc.poll() is None:
            proc.kill()
            try:
                proc.wait(timeout=2)
            except Exception:
                pass
        try:
            self._session_log.close()
        except Exception:
            pass
        self.destroy()

    # -- safe after ----------------------------------------------------------
    def _safe_after(self, ms, func):
        if not self._destroyed:
            try:
                self.after(ms, func)
            except tk.TclError:
                pass

    # -- main loop (worker thread) -------------------------------------------
    def _loop(self):
        wt = self._ensure_worktree()
        if wt is None:
            self._log_msg("[WT] worktree 준비 실패. 중단합니다.")
            self._safe_after(0, self._reset_ui)
            return
        cwd = str(wt)
        run_num = 0
        while not self._stop_requested:
            run_num += 1
            # rate limit 대기를 sync 전에 처리 (대기 후 최신 상태로 sync)
            if self._rate_limit_resets_at:
                self._wait_rate_limit()
                self._rate_limit_resets_at = None
                if self._stop_requested:
                    break
            if not self._sync_worktree(wt):
                self._log_msg("[SYNC] 동기화 실패, 이번 run 건너뜁니다.")
                time.sleep(INTER_RUN_DELAY)
                continue
            _, head_before = _git_query(["rev-parse", "origin/main"], PROJECT_DIR)
            self._run_once(run_num, cwd)
            # origin/main이 run 중에 변경됐으면 Claude가 main에 직접 push한 것
            _git_locked(["fetch", "origin", "main"], PROJECT_DIR, timeout=30)
            _, head_after = _git_query(["rev-parse", "origin/main"], PROJECT_DIR)
            if head_before != head_after:
                self._log_msg("[WARN] ⚠️ Claude가 origin/main에 직접 push 감지 — 워크트리 격리 우회됨")
            self._merge_to_main(wt)
            if self._check_all_done():
                break
            if not self._stop_requested and not self._rate_limit_resets_at:
                time.sleep(INTER_RUN_DELAY)

        self._log_msg("\nDone.")
        self._safe_after(0, self._reset_ui)

    def _run_once(self, run_num, cwd):
        ts = datetime.now()
        self._safe_after(0, lambda n=run_num: self._set_status(
            f"Run #{n}", "#a6e3a1"))
        self._log_msg(f"\n{'='*50}")
        self._log_msg(f"[{ts:%H:%M:%S}] Run #{run_num} START")
        self._log_msg(f"{'='*50}")

        if self._stop_requested:
            return

        log_path = LOG_DIR / f"run_{ts:%Y%m%d_%H%M%S_%f}.jsonl"

        try:
            self._proc = subprocess.Popen(
                ["claude", "--print", "--verbose",
                 "--output-format", "stream-json",
                 "--model", self._model],
                stdin=subprocess.PIPE,
                stdout=subprocess.PIPE,
                stderr=subprocess.STDOUT,
                text=True, encoding="utf-8", errors="replace",
                cwd=cwd, **_NO_WIN)
        except FileNotFoundError:
            self._log_msg("[ERROR] 'claude' command not found")
            self._stop_requested = True
            return

        try:
            self._proc.stdin.write(PROMPT)
            self._proc.stdin.close()
        except OSError:
            self._proc.kill()
            self._proc.wait()
            self._log_msg("[ERROR] stdin write failed")
            return

        cost = 0.0
        try:
            with open(log_path, "w", encoding="utf-8") as fh:
                for line in self._proc.stdout:
                    fh.write(line)
                    cost = self._parse(line, cost)
            self._proc.wait()
        except (OSError, ValueError):
            try:
                self._proc.kill()
                self._proc.wait()  # kill 후에는 즉시 종료되므로 timeout 불필요
            except Exception:
                pass
        code = self._proc.returncode if self._proc.returncode is not None else -1
        elapsed = datetime.now() - ts
        self._log_msg(
            f"\n[{datetime.now():%H:%M:%S}] Run #{run_num} DONE "
            f"(exit={code}, {_fmt_elapsed(elapsed)}, ${cost:.4f})")

    def _fmt_reset_time(self):
        return self._rate_limit_resets_at.astimezone().strftime("%H:%M")

    # -- rate limit wait -----------------------------------------------------
    def _wait_rate_limit(self):
        # _rate_limit_resets_at은 반드시 tzinfo 포함 aware datetime이어야 함 (_parse에서 보장)
        now = datetime.now(timezone.utc)
        wait = max(
            int((self._rate_limit_resets_at - now).total_seconds()) + RATE_LIMIT_BUF,
            RATE_LIMIT_MIN)
        self._log_msg(f"[RATE LIMIT] {self._fmt_reset_time()} 해제 예정. {_fmt_dhms(wait)} 대기...")

        end = time.monotonic() + wait
        while time.monotonic() < end and not self._stop_requested and not self._destroyed:
            rem = int(end - time.monotonic())
            self._safe_after(0, lambda r=rem: self._set_status(
                f"RATE LIMIT ({_fmt_dhms(r)})", "#f9e2af"))
            time.sleep(1)

    # -- parse ---------------------------------------------------------------
    def _parse(self, line, cost):
        try:
            obj = json.loads(line)
        except Exception:
            return cost

        etype = obj.get("type", "")
        sub   = obj.get("subtype", "")

        if etype == "assistant":
            for b in obj.get("message", {}).get("content", []):
                if b.get("type") == "tool_use":
                    name = b.get("name", "")
                    inp  = b.get("input", {})
                    self._log_msg(f"  {_tool_icon(name)} {_tool_summary(name, inp)}")
                elif b.get("type") == "text":
                    t = b.get("text", "").strip()
                    if t and len(t) < MAX_INLINE_TEXT_LEN:
                        self._log_msg(f"  >> {t}")

        elif etype == "system" and sub == "task_started":
            self._log_msg(f"  Agent: {obj.get('description', '')}")

        elif etype == "system" and sub == "task_notification":
            icon = "OK" if obj.get("status") == "completed" else "FAIL"
            self._log_msg(f"  [{icon}] {obj.get('summary', '')[:80]}")

        elif etype == "result":
            val = obj.get("total_cost_usd")
            cost = val if val is not None else cost
            dur  = obj.get("duration_ms", 0) / 1000
            self._log_msg(f"  ---- ${cost:.4f} / {dur:.0f}s ----")

        elif etype == "rate_limit_event":
            info = obj.get("rate_limit_info", {})
            if info.get("status") == "rejected" and info.get("resetsAt"):
                raw_ts = info["resetsAt"]
                if isinstance(raw_ts, str):
                    self._rate_limit_resets_at = datetime.fromisoformat(
                        raw_ts.replace("Z", "+00:00"))
                else:
                    if raw_ts > 1e12:  # ms → s 보정
                        raw_ts /= 1000
                    self._rate_limit_resets_at = datetime.fromtimestamp(
                        raw_ts, tz=timezone.utc)
                self._log_msg(f"  !! Rate limited. Resets at {self._fmt_reset_time()}")
            elif info.get("status") == "allowed_warning":
                self._log_msg(
                    f"  !! Rate limit warning: {info.get('utilization', 0):.0%}")

        return cost

    # -- UI helpers ----------------------------------------------------------
    def _log_msg(self, msg):
        try:
            self._session_log.write(f"[{datetime.now():%H:%M:%S}] {msg}\n")
        except Exception:
            pass
        self._safe_after(0, lambda m=msg: self._append_log(m))

    def _append_log(self, msg):
        if self._destroyed:
            return
        self._log.config(state="normal")
        self._log.insert("end", msg + "\n")
        total = int(self._log.index("end-1c").split(".")[0])
        if total > MAX_LOG_LINES:
            self._log.delete("1.0", f"{total - MAX_LOG_LINES}.0")
        self._log.see("end")
        self._log.config(state="disabled")

    def _set_status(self, text, color):
        if self._destroyed:
            return
        self._lbl_status.config(text=text, fg=color)

    def _reset_ui(self):
        if self._destroyed:
            return
        self._running = False
        self._btn_start.config(state="normal")
        self._btn_stop.config(state="disabled")
        self._model_menu.config(state="normal")
        if not self._all_done:
            self._set_status("IDLE", "#6c7086")


# -- helpers -----------------------------------------------------------------
def _tool_icon(name):
    return {"Read": "R", "Edit": "E", "Write": "W", "Bash": "$",
            "Glob": "?", "Grep": "G", "Agent": "A", "Skill": "S"}.get(name, "*")


def _tool_summary(name, inp):
    if name in ("Read", "Edit", "Write"):
        return f"{name} {Path(inp.get('file_path', '')).name}"
    if name == "Bash":
        return inp.get("description") or inp.get("command", "")[:60]
    if name == "Glob":
        return inp.get("pattern", "")
    if name == "Grep":
        p = Path(inp.get("path", "")).name
        return f"{inp.get('pattern', '')} in {p}" if p else inp.get("pattern", "")
    if name == "Agent":
        return f"Agent: {inp.get('description', '')}"
    if name == "Skill":
        return f"Skill: {inp.get('skill', '')}"
    return name


if __name__ == "__main__":
    App().mainloop()
