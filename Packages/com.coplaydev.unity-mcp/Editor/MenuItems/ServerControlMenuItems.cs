using MCPForUnity.Editor.Services;
using UnityEditor;
using UnityEngine;

namespace MCPForUnity.Editor.MenuItems
{
    /// <summary>
    /// Exposes MCP server start/stop/restart as menu items so external tools
    /// (e.g. Claude Code via execute_menu_item) can control the server lifecycle.
    /// </summary>
    public static class ServerControlMenuItems
    {
        [MenuItem("MCP For Unity/Server/Start Server")]
        public static void StartServer()
        {
            var svc = MCPServiceLocator.Server;
            if (svc == null)
            {
                Debug.LogError("[MCP] ServerManagementService not available.");
                return;
            }
            if (svc.IsLocalHttpServerRunning())
            {
                Debug.Log("[MCP] Server already running.");
                return;
            }
            bool ok = svc.StartLocalHttpServer(quiet: true);
            Debug.Log(ok ? "[MCP] Server started." : "[MCP] Server start failed.");
        }

        [MenuItem("MCP For Unity/Server/Stop Server")]
        public static void StopServer()
        {
            var svc = MCPServiceLocator.Server;
            if (svc == null)
            {
                Debug.LogError("[MCP] ServerManagementService not available.");
                return;
            }
            bool ok = svc.StopLocalHttpServer();
            Debug.Log(ok ? "[MCP] Server stopped." : "[MCP] Server stop failed (was it running?).");
        }

        [MenuItem("MCP For Unity/Server/Restart Server")]
        public static void RestartServer()
        {
            var svc = MCPServiceLocator.Server;
            if (svc == null)
            {
                Debug.LogError("[MCP] ServerManagementService not available.");
                return;
            }
            svc.StopLocalHttpServer();
            bool ok = svc.StartLocalHttpServer(quiet: true);
            Debug.Log(ok ? "[MCP] Server restarted." : "[MCP] Server restart failed.");
        }

        [MenuItem("MCP For Unity/Server/Server Status")]
        public static void ServerStatus()
        {
            var svc = MCPServiceLocator.Server;
            if (svc == null)
            {
                Debug.LogError("[MCP] ServerManagementService not available.");
                return;
            }
            bool running   = svc.IsLocalHttpServerRunning();
            bool reachable = svc.IsLocalHttpServerReachable();
            Debug.Log($"[MCP] Server running={running} reachable={reachable}");
        }
    }
}
