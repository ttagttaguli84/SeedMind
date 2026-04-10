using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SeedMind.Tests
{
    /// <summary>
    /// Phase 3 QA — 씬별 자동화 플레이 모드 테스트.
    /// 유니티 Test Runner(PlayMode)에서 실행.
    /// </summary>
    public class QAPlayModeTests
    {
        // ── 공통 헬퍼 ────────────────────────────────────────────────────────

        /// <summary>Assembly-CSharp 기반 타입을 문자열로 룩업.</summary>
        static Type GameType(string fullName) =>
            Type.GetType($"{fullName}, Assembly-CSharp");

        /// <summary>씬 로드 + 초기화 대기.</summary>
        static IEnumerator LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
            yield return null;          // 첫 프레임 (Start 전)
            yield return null;          // Start 완료
            yield return new WaitForSeconds(0.3f); // 비동기 초기화 여유
        }

        // ════════════════════════════════════════════════════════════════════
        // 1. SCN_MainMenu
        // ════════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator SCN_MainMenu_LoadsWithoutErrors()
        {
            LogAssert.ignoreFailingMessages = false;
            yield return LoadScene("SCN_MainMenu");
            // 예외 없이 로드되면 통과
            Assert.AreEqual("SCN_MainMenu", SceneManager.GetActiveScene().name);
        }

        [UnityTest]
        public IEnumerator SCN_MainMenu_CanvasExists()
        {
            yield return LoadScene("SCN_MainMenu");
            var canvas = GameObject.Find("Canvas_MainMenu");
            Assert.IsNotNull(canvas, "Canvas_MainMenu 오브젝트가 없습니다.");
        }

        [UnityTest]
        public IEnumerator SCN_MainMenu_ControllerWired()
        {
            yield return LoadScene("SCN_MainMenu");
            var controllerType = GameType("SeedMind.UI.MainMenuController");
            Assert.IsNotNull(controllerType, "MainMenuController 타입 없음");

            var controller = GameObject.FindFirstObjectByType(controllerType);
            Assert.IsNotNull(controller, "MainMenuController 컴포넌트가 씬에 없습니다.");
        }

        // ════════════════════════════════════════════════════════════════════
        // 2. SCN_Farm — 초기화
        // ════════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator SCN_Farm_LoadsWithoutErrors()
        {
            LogAssert.ignoreFailingMessages = false;
            yield return LoadScene("SCN_Farm");
            Assert.AreEqual("SCN_Farm", SceneManager.GetActiveScene().name);
        }

        [UnityTest]
        public IEnumerator SCN_Farm_TimeManagerInitializes()
        {
            yield return LoadScene("SCN_Farm");
            var type = GameType("SeedMind.Core.TimeManager");
            Assert.IsNotNull(type, "TimeManager 타입 없음");

            var tm = GameObject.FindFirstObjectByType(type);
            Assert.IsNotNull(tm, "TimeManager가 씬에 없습니다.");
        }

        [UnityTest]
        public IEnumerator SCN_Farm_PlayerExists()
        {
            yield return LoadScene("SCN_Farm");
            var playerType = GameType("SeedMind.Player.PlayerController");
            Assert.IsNotNull(playerType, "PlayerController 타입 없음");

            var player = GameObject.FindFirstObjectByType(playerType);
            Assert.IsNotNull(player, "PlayerController가 씬에 없습니다.");
        }

        [UnityTest]
        public IEnumerator SCN_Farm_FarmGridExists()
        {
            yield return LoadScene("SCN_Farm");
            var gridType = GameType("SeedMind.Farm.FarmGrid");
            Assert.IsNotNull(gridType, "FarmGrid 타입 없음");

            var grid = GameObject.FindFirstObjectByType(gridType);
            Assert.IsNotNull(grid, "FarmGrid가 씬에 없습니다.");
        }

        [UnityTest]
        public IEnumerator SCN_Farm_EconomyManagerInitializes()
        {
            yield return LoadScene("SCN_Farm");
            var type = GameType("SeedMind.Economy.EconomyManager");
            Assert.IsNotNull(type, "EconomyManager 타입 없음");

            var em = GameObject.FindFirstObjectByType(type);
            Assert.IsNotNull(em, "EconomyManager가 씬에 없습니다.");
        }

        [UnityTest]
        public IEnumerator SCN_Farm_SaveManagerInitializes()
        {
            yield return LoadScene("SCN_Farm");
            var type = GameType("SeedMind.Save.SaveManager");
            Assert.IsNotNull(type, "SaveManager 타입 없음");

            var sm = GameObject.FindFirstObjectByType(type);
            Assert.IsNotNull(sm, "SaveManager가 씬에 없습니다.");
        }

        [UnityTest]
        public IEnumerator SCN_Farm_HUDControllerExists()
        {
            yield return LoadScene("SCN_Farm");
            var type = GameType("SeedMind.UI.HUDController");
            Assert.IsNotNull(type, "HUDController 타입 없음");

            var hud = GameObject.FindFirstObjectByType(type);
            Assert.IsNotNull(hud, "HUDController가 씬에 없습니다.");
        }

        // ════════════════════════════════════════════════════════════════════
        // 3. 씬 전환 — MainMenu → Farm
        // ════════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator SceneTransition_MainMenuToFarm()
        {
            yield return LoadScene("SCN_MainMenu");

            // MainMenuController.OnNewGame() 리플렉션 호출
            var controllerType = GameType("SeedMind.UI.MainMenuController");
            Assert.IsNotNull(controllerType, "MainMenuController 타입 없음");

            var controller = GameObject.FindFirstObjectByType(controllerType) as MonoBehaviour;
            Assert.IsNotNull(controller, "MainMenuController 인스턴스 없음");

            var onNewGame = controllerType.GetMethod(
                "OnNewGame",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);
            Assert.IsNotNull(onNewGame, "OnNewGame 메서드 없음");

            onNewGame.Invoke(controller, null);

            // 씬 전환 대기
            yield return new WaitForSeconds(1.0f);

            Assert.AreEqual("SCN_Farm", SceneManager.GetActiveScene().name,
                "MainMenu에서 새 게임 시작 후 SCN_Farm으로 전환되어야 합니다.");
        }

        // ════════════════════════════════════════════════════════════════════
        // 4. 농사 사이클 — FarmGrid 타일 상호작용
        // ════════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator FarmTile_SetState_Tilled()
        {
            yield return LoadScene("SCN_Farm");

            var gridType  = GameType("SeedMind.Farm.FarmGrid");
            var tileType  = GameType("SeedMind.Farm.FarmTile");
            var stateType = GameType("SeedMind.Farm.Data.TileState");
            Assert.IsNotNull(gridType,  "FarmGrid 타입 없음");
            Assert.IsNotNull(tileType,  "FarmTile 타입 없음");
            Assert.IsNotNull(stateType, "TileState 타입 없음");

            var grid = GameObject.FindFirstObjectByType(gridType);
            Assert.IsNotNull(grid, "FarmGrid 없음");

            // GetTile(0, 0) → FarmTile
            var getTile = gridType.GetMethod("GetTile");
            Assert.IsNotNull(getTile, "GetTile 메서드 없음");
            var tile = getTile.Invoke(grid, new object[] { 0, 0 });
            Assert.IsNotNull(tile, "타일 (0,0) 없음");

            // tile.SetState(TileState.Tilled)
            var setStateMethod = tileType.GetMethod("SetState");
            Assert.IsNotNull(setStateMethod, "FarmTile.SetState 없음");
            var tilledVal = Enum.Parse(stateType, "Tilled");
            setStateMethod.Invoke(tile, new[] { tilledVal });
            yield return null;

            // tile.State == Tilled
            var stateProp = tileType.GetProperty("State");
            Assert.IsNotNull(stateProp, "FarmTile.State 프로퍼티 없음");
            var state = stateProp.GetValue(tile);
            Assert.AreEqual(tilledVal, state, "SetState 후 TileState가 Tilled여야 합니다.");
        }

        // ════════════════════════════════════════════════════════════════════
        // 5. 저장/로드
        // ════════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator SaveLoad_SaveDoesNotThrow()
        {
            yield return LoadScene("SCN_Farm");

            var type = GameType("SeedMind.Save.SaveManager");
            Assert.IsNotNull(type);
            var sm = GameObject.FindFirstObjectByType(type);
            Assert.IsNotNull(sm, "SaveManager 없음");

            // AutoSaveAsync() 호출 — 예외 없이 시작되어야 함
            var saveMethod = type.GetMethod("AutoSaveAsync");
            Assert.IsNotNull(saveMethod, "AutoSaveAsync 메서드 없음");

            Assert.DoesNotThrow(() => saveMethod.Invoke(sm, null));
            yield return new WaitForSeconds(0.5f); // 비동기 완료 대기
        }
    }
}
