using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace SeedMind.Tests
{
    /// <summary>
    /// Phase 3 QA — 키보드/마우스 입력 시뮬레이션 게임플레이 테스트.
    /// InputTestFixture 없이 InputSystem.AddDevice + QueueStateEvent로 입력을 시뮬레이션.
    /// </summary>
    public class GameplayInputTests
    {
        Keyboard  _keyboard;
        Mouse     _mouse;

        static Type GameType(string n) =>
            Type.GetType($"{n}, Assembly-CSharp");

        [SetUp]
        public void SetUp()
        {
            // 가상 입력 장치 생성 — Keyboard.current / Mouse.current 에 반영됨
            _keyboard = InputSystem.AddDevice<Keyboard>();
            _mouse    = InputSystem.AddDevice<Mouse>();
        }

        [TearDown]
        public void TearDown()
        {
            // 가상 장치 제거 및 키 상태 초기화
            if (_keyboard != null) InputSystem.RemoveDevice(_keyboard);
            if (_mouse    != null) InputSystem.RemoveDevice(_mouse);
            _keyboard = null;
            _mouse    = null;
        }

        // ── 입력 헬퍼 ────────────────────────────────────────────────────────

        void PressKey(Key key)
        {
            var state = new KeyboardState(key);
            InputSystem.QueueStateEvent(_keyboard, state);
            InputSystem.Update();
        }

        void ReleaseKey(Key key)
        {
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState());
            InputSystem.Update();
        }

        void ScrollMouse(float y)
        {
            // InputSystem.Update()를 직접 호출하지 않음:
            // 수동 Update()는 delta를 소비해 버려 게임 프레임에서 0이 된다.
            // 이벤트만 큐에 넣고 yield return null로 Unity 루프가 처리하게 둔다.
            InputSystem.QueueStateEvent(_mouse, new MouseState { scroll = new Vector2(0, y) });
        }

        IEnumerator LoadFarm()
        {
            SceneManager.LoadScene("SCN_Farm");
            yield return null;
            yield return null;
            yield return new WaitForSeconds(0.5f);
        }

        // ════════════════════════════════════════════════════════════════════
        // 1. 플레이어 이동 — WASD
        // ════════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator Player_MovesForward_WhenWPressed()
        {
            yield return LoadFarm();

            var playerType = GameType("SeedMind.Player.PlayerController");
            var player     = (MonoBehaviour)GameObject.FindFirstObjectByType(playerType);
            Assert.IsNotNull(player, "PlayerController 없음");

            var startPos = player.transform.position;

            PressKey(Key.W);
            yield return new WaitForSeconds(0.3f);
            ReleaseKey(Key.W);
            yield return null;

            float moved = player.transform.position.z - startPos.z;
            Assert.Greater(moved, 0.01f,
                $"W 키: 앞(+Z) 이동 기대. 실제 이동량={moved:F3}");
        }

        [UnityTest]
        public IEnumerator Player_MovesBackward_WhenSPressed()
        {
            yield return LoadFarm();

            var playerType = GameType("SeedMind.Player.PlayerController");
            var player     = (MonoBehaviour)GameObject.FindFirstObjectByType(playerType);
            Assert.IsNotNull(player, "PlayerController 없음");

            var startPos = player.transform.position;

            PressKey(Key.S);
            yield return new WaitForSeconds(0.3f);
            ReleaseKey(Key.S);
            yield return null;

            float moved = player.transform.position.z - startPos.z;
            Assert.Less(moved, -0.01f,
                $"S 키: 뒤(-Z) 이동 기대. 실제 이동량={moved:F3}");
        }

        [UnityTest]
        public IEnumerator Player_MovesRight_WhenDPressed()
        {
            yield return LoadFarm();

            var playerType = GameType("SeedMind.Player.PlayerController");
            var player     = (MonoBehaviour)GameObject.FindFirstObjectByType(playerType);
            Assert.IsNotNull(player, "PlayerController 없음");

            var startPos = player.transform.position;

            PressKey(Key.D);
            yield return new WaitForSeconds(0.3f);
            ReleaseKey(Key.D);
            yield return null;

            float moved = player.transform.position.x - startPos.x;
            Assert.Greater(moved, 0.01f,
                $"D 키: 오른쪽(+X) 이동 기대. 실제 이동량={moved:F3}");
        }

        [UnityTest]
        public IEnumerator Player_MovesLeft_WhenAPressed()
        {
            yield return LoadFarm();

            var playerType = GameType("SeedMind.Player.PlayerController");
            var player     = (MonoBehaviour)GameObject.FindFirstObjectByType(playerType);
            Assert.IsNotNull(player, "PlayerController 없음");

            var startPos = player.transform.position;

            PressKey(Key.A);
            yield return new WaitForSeconds(0.3f);
            ReleaseKey(Key.A);
            yield return null;

            float moved = player.transform.position.x - startPos.x;
            Assert.Less(moved, -0.01f,
                $"A 키: 왼쪽(-X) 이동 기대. 실제 이동량={moved:F3}");
        }

        [UnityTest]
        public IEnumerator Player_StopsOnXZ_WhenKeyReleased()
        {
            yield return LoadFarm();

            var playerType = GameType("SeedMind.Player.PlayerController");
            var player     = (MonoBehaviour)GameObject.FindFirstObjectByType(playerType);
            Assert.IsNotNull(player, "PlayerController 없음");

            PressKey(Key.W);
            yield return new WaitForSeconds(0.2f);
            ReleaseKey(Key.W);
            yield return new WaitForSeconds(0.1f);

            var posAfter = player.transform.position;
            yield return new WaitForSeconds(0.3f);
            var posLater = player.transform.position;

            float xzDrift = Mathf.Sqrt(
                Mathf.Pow(posLater.x - posAfter.x, 2) +
                Mathf.Pow(posLater.z - posAfter.z, 2));
            Assert.Less(xzDrift, 0.05f,
                $"키 해제 후 XZ 정지 기대. drift={xzDrift:F3}");
        }

        // ════════════════════════════════════════════════════════════════════
        // 2. 도구 선택 — 마우스 스크롤
        // ════════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator ToolSystem_ScrollUp_SelectsNextTool()
        {
            yield return LoadFarm();

            var toolType   = GameType("SeedMind.Player.ToolSystem");
            var toolSystem = (MonoBehaviour)GameObject.FindFirstObjectByType(toolType);
            Assert.IsNotNull(toolSystem, "ToolSystem 없음");

            var toolsField = toolType.GetField("tools");
            var tools      = toolsField?.GetValue(toolSystem) as Array;
            if (tools == null || tools.Length < 2)
            {
                Assert.Ignore("도구가 2개 미만 — 스크롤 테스트 스킵");
                yield break;
            }

            var idxField   = toolType.GetField("currentToolIndex");
            int before     = (int)idxField.GetValue(toolSystem);

            ScrollMouse(120f);
            yield return null; // 게임 루프가 scroll 이벤트 처리 → ToolSystem.Update() 실행
            yield return null; // delta 자동 리셋 확인

            int after = (int)idxField.GetValue(toolSystem);
            int expected = (before + 1) % tools.Length;
            Assert.AreEqual(expected, after,
                $"스크롤 업: 도구 인덱스 {before}→{expected} 기대, 실제={after}");
        }

        [UnityTest]
        public IEnumerator ToolSystem_ScrollDown_SelectsPrevTool()
        {
            yield return LoadFarm();

            var toolType   = GameType("SeedMind.Player.ToolSystem");
            var toolSystem = (MonoBehaviour)GameObject.FindFirstObjectByType(toolType);
            Assert.IsNotNull(toolSystem, "ToolSystem 없음");

            var toolsField = toolType.GetField("tools");
            var tools      = toolsField?.GetValue(toolSystem) as Array;
            if (tools == null || tools.Length < 2)
            {
                Assert.Ignore("도구가 2개 미만 — 스크롤 테스트 스킵");
                yield break;
            }

            var idxField = toolType.GetField("currentToolIndex");
            int before   = (int)idxField.GetValue(toolSystem);

            ScrollMouse(-120f);
            yield return null; // 게임 루프가 scroll 이벤트 처리 → ToolSystem.Update() 실행
            yield return null; // delta 자동 리셋 확인

            int after    = (int)idxField.GetValue(toolSystem);
            int expected = (before - 1 + tools.Length) % tools.Length;
            Assert.AreEqual(expected, after,
                $"스크롤 다운: 도구 인덱스 {before}→{expected} 기대, 실제={after}");
        }

        // ════════════════════════════════════════════════════════════════════
        // 3. 농사 사이클 — Empty→Tilled→Planted→Watered→(성장 후)Harvestable
        // ════════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator FarmCycle_FullSequence_EmptyToWatered()
        {
            yield return LoadFarm();

            var gridType  = GameType("SeedMind.Farm.FarmGrid");
            var tileType  = GameType("SeedMind.Farm.FarmTile");
            var stateType = GameType("SeedMind.Farm.Data.TileState");
            var grid      = GameObject.FindFirstObjectByType(gridType);
            Assert.IsNotNull(grid, "FarmGrid 없음");

            var getTile   = gridType.GetMethod("GetTile");
            var setState  = tileType.GetMethod("SetState");
            var stateProp = tileType.GetProperty("State");

            var tile = getTile.Invoke(grid, new object[] { 0, 0 });
            Assert.IsNotNull(tile, "타일(0,0) 없음");

            void CheckState(string name)
            {
                var expected = Enum.Parse(stateType, name);
                setState.Invoke(tile, new[] { expected });
                Assert.AreEqual(expected, stateProp.GetValue(tile), $"{name} 전환 실패");
            }

            CheckState("Tilled");
            yield return null;
            CheckState("Planted");
            yield return null;
            CheckState("Watered");
            yield return null;

            Debug.Log("[QA] 농사 사이클 Empty→Tilled→Planted→Watered ✅");
        }

        // ════════════════════════════════════════════════════════════════════
        // 4. 경제 — 골드 획득
        // ════════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator Economy_AddGold_IncreasesBalance()
        {
            yield return LoadFarm();

            var econType = GameType("SeedMind.Economy.EconomyManager");
            var econ     = GameObject.FindFirstObjectByType(econType);
            Assert.IsNotNull(econ, "EconomyManager 없음");

            // Gold 필드/프로퍼티 동적 탐색
            MemberInfo goldMember =
                (MemberInfo)econType.GetProperty("Gold")       ??
                (MemberInfo)econType.GetProperty("CurrentGold")??
                (MemberInfo)econType.GetField("_gold",
                    BindingFlags.NonPublic | BindingFlags.Instance);

            if (goldMember == null) { Assert.Ignore("Gold 멤버 없음 — 스킵"); yield break; }

            int GetGold()
            {
                if (goldMember is PropertyInfo pi) return Convert.ToInt32(pi.GetValue(econ));
                return Convert.ToInt32(((FieldInfo)goldMember).GetValue(econ));
            }

            var addGold = econType.GetMethod("AddGold") ??
                          econType.GetMethod("EarnGold") ??
                          econType.GetMethod("Add");
            if (addGold == null) { Assert.Ignore("AddGold 메서드 없음 — 스킵"); yield break; }

            int before = GetGold();
            addGold.Invoke(econ, new object[] { 100 });
            yield return null;
            int after = GetGold();

            Assert.AreEqual(before + 100, after,
                $"골드 100 추가 후 {before}→{before + 100} 기대. 실제={after}");
        }

        // ════════════════════════════════════════════════════════════════════
        // 5. 씬 전환 + 플레이어 스폰 확인
        // ════════════════════════════════════════════════════════════════════

        [UnityTest]
        public IEnumerator MainMenu_NewGame_PlayerSpawnsInFarm()
        {
            SceneManager.LoadScene("SCN_MainMenu");
            yield return null;
            yield return new WaitForSeconds(0.3f);

            var ctrlType = GameType("SeedMind.UI.MainMenuController");
            var ctrl     = (MonoBehaviour)GameObject.FindFirstObjectByType(ctrlType);
            Assert.IsNotNull(ctrl, "MainMenuController 없음");

            var onNewGame = ctrlType.GetMethod("OnNewGame",
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.IsNotNull(onNewGame, "OnNewGame 없음");
            onNewGame.Invoke(ctrl, null);

            yield return new WaitForSeconds(1.0f);

            Assert.AreEqual("SCN_Farm", SceneManager.GetActiveScene().name,
                "MainMenu → SCN_Farm 전환 실패");

            var playerType = GameType("SeedMind.Player.PlayerController");
            var player     = GameObject.FindFirstObjectByType(playerType);
            Assert.IsNotNull(player, "SCN_Farm 전환 후 PlayerController 스폰 실패");
        }
    }
}
