using System;
using System.Threading;
using MCPForUnity.Editor.Helpers;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace MCPForUnity.Editor.Tools
{
    /// <summary>
    /// Blocks until a Unity console log matching the given pattern appears,
    /// then returns immediately. Eliminates polling loops for test results
    /// or any other log-triggered events.
    /// </summary>
    [McpForUnityTool(
        "wait_for_log",
        Description = "Blocks until a Unity Editor console log message matching 'pattern' appears, then returns immediately. Use instead of polling read_console in a loop. Ideal for waiting on test results (e.g. pattern='[TEST_SUMMARY]') or any async Unity operation that writes to the log.",
        Group = "core"
    )]
    public static class WaitForLog
    {
        static volatile string          _waitPattern;
        static volatile string          _waitResult;
        static ManualResetEventSlim     _waitEvent;
        static readonly object          _lock = new();

        static WaitForLog()
        {
            Application.logMessageReceivedThreaded += OnLogReceived;
        }

        static void OnLogReceived(string message, string stackTrace, LogType type)
        {
            string pat = _waitPattern;
            if (pat == null) return;
            if (message.IndexOf(pat, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                lock (_lock)
                {
                    if (_waitPattern != null)
                    {
                        _waitResult = message;
                        _waitEvent?.Set();
                    }
                }
            }
        }

        public static object HandleCommand(JObject @params)
        {
            if (@params == null)
                return new ErrorResponse("Parameters cannot be null.");

            var p = new ToolParams(@params);

            string pattern   = p.Get("pattern");
            int    timeoutMs = p.GetInt("timeout") ?? 30000;

            if (string.IsNullOrEmpty(pattern))
                return new ErrorResponse("'pattern' parameter is required.");

            var evt = new ManualResetEventSlim(false);

            lock (_lock)
            {
                _waitPattern = pattern;
                _waitResult  = null;
                _waitEvent   = evt;
            }

            try
            {
                bool signaled = evt.Wait(timeoutMs);

                if (signaled)
                {
                    string matched = _waitResult;
                    return new SuccessResponse(
                        $"Log matched pattern '{pattern}'.",
                        new { matched }
                    );
                }
                return new ErrorResponse(
                    $"Timeout ({timeoutMs} ms) — no log matched pattern '{pattern}'."
                );
            }
            finally
            {
                lock (_lock)
                {
                    _waitPattern = null;
                    _waitResult  = null;
                    _waitEvent   = null;
                }
                evt.Dispose();
            }
        }
    }
}
