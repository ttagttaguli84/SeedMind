// QA 자동 플레이 테스트 — Start()에서 동기 실행 (지연 없음)
using UnityEngine;
using SeedMind.Farm;
using SeedMind.Farm.Data;
using SeedMind.Player;
using SeedMind.Player.Data;
using SeedMind.Economy;

namespace SeedMind.QA
{
    /// <summary>
    /// 씬에 배치하면 Start()에서 농사 사이클 전체를 동기 실행하고 결과를 로그로 출력.
    /// 타일 상태 직접 조작 방식 — 프레임 대기 없음.
    /// </summary>
    public class AutoPlayTest : MonoBehaviour
    {
        [Header("테스트 타일 좌표")]
        public Vector2Int testTile = new Vector2Int(2, 2);

        private void Start()
        {
            Debug.Log("[APT] ======= AutoPlayTest 시작 =======");

            // 시스템 취득
            var grid    = Object.FindFirstObjectByType<FarmGrid>();
            var toolSys = Object.FindFirstObjectByType<ToolSystem>();
            var econ    = EconomyManager.Instance;

            if (grid == null)    { Debug.LogError("[APT] FarmGrid 없음");    return; }
            if (toolSys == null) { Debug.LogError("[APT] ToolSystem 없음");  return; }
            if (econ == null)    { Debug.LogError("[APT] EconomyManager 없음"); return; }

            Debug.Log($"[APT] 시스템 OK — 골드={econ.CurrentGold}G 도구={toolSys.tools?.Length}개");

            // 카메라 확인
            var cam = Camera.main;
            Debug.Log(cam != null
                ? $"[APT] Camera.main={cam.name} pos={cam.transform.position} rot={cam.transform.eulerAngles}"
                : "[APT] Camera.main 없음");

            // 플레이어 확인
            var player = Object.FindFirstObjectByType<PlayerController>();
            Debug.Log(player != null
                ? $"[APT] Player pos={player.transform.position}"
                : "[APT] PlayerController 없음");

            // 타일 취득
            var tile = grid.GetTile(testTile.x, testTile.y);
            if (tile == null) { Debug.LogError($"[APT] 타일({testTile.x},{testTile.y}) 없음"); return; }
            Debug.Log($"[APT] 타일({testTile.x},{testTile.y}) 초기 상태={tile.State}");

            // 1. 경작
            tile.SetState(TileState.Empty);
            UseTool(toolSys, ToolType.Hoe, testTile);
            Debug.Log($"[APT] [1] Hoe → {tile.State}  {(tile.State == TileState.Tilled ? "✓" : "✗ 실패")}");

            // 2. 파종
            UseTool(toolSys, ToolType.SeedBag, testTile);
            Debug.Log($"[APT] [2] SeedBag → {tile.State}  {(tile.State == TileState.Planted ? "✓" : "✗ 실패")}");

            // 3. 물주기
            UseTool(toolSys, ToolType.WateringCan, testTile);
            Debug.Log($"[APT] [3] WateringCan → {tile.State}  {(tile.State == TileState.Watered ? "✓" : "✗ 실패")}");

            // 4. 강제 성장
            tile.SetState(TileState.Harvestable);
            Debug.Log($"[APT] [4] Harvestable 강제 → {tile.State} ✓");

            // 5. 수확
            int goldBefore = econ.CurrentGold;
            UseTool(toolSys, ToolType.Sickle, testTile);
            int goldAfter = econ.CurrentGold;
            bool harvestOk = tile.State == TileState.Empty && goldAfter > goldBefore;
            Debug.Log($"[APT] [5] Sickle → {tile.State} 골드 {goldBefore}→{goldAfter}G  {(harvestOk ? "✓" : "✗ 실패")}");

            // HUD 확인
            var hud = Object.FindFirstObjectByType<SeedMind.UI.HUDController>();
            Debug.Log($"[APT] HUD={hud != null}");

            Debug.Log($"[APT] ======= 완료: {(harvestOk ? "전체 PASS" : "일부 FAIL")} =======");
        }

        private void UseTool(ToolSystem ts, ToolType type, Vector2Int pos)
        {
            if (ts.tools == null) { Debug.LogWarning($"[APT] tools 배열 null"); return; }
            for (int i = 0; i < ts.tools.Length; i++)
            {
                if (ts.tools[i] != null && ts.tools[i].toolType == type)
                {
                    ts.SelectTool(i);
                    bool result = ts.TryUseToolAt(pos);
                    if (!result) Debug.LogWarning($"[APT] TryUseToolAt({pos}) returned false for {type}");
                    return;
                }
            }
            Debug.LogWarning($"[APT] 도구 {type} tools 배열에 없음");
        }
    }
}
