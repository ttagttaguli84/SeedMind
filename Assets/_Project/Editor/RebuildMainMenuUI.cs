using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace SeedMind.Editor
{
    public static class RebuildMainMenuUI
    {
        [MenuItem("SeedMind/Rebuild MainMenu UI")]
        public static void Run()
        {
            // 1. MainMenu 씬 로드 확인
            var scene = SceneManager.GetActiveScene();
            if (scene.name != "SCN_MainMenu")
            {
                EditorUtility.DisplayDialog("오류", "SCN_MainMenu 씬을 먼저 열어주세요.", "확인");
                return;
            }

            // 2. --- UI --- 하위의 Canvas_MainMenu 찾기
            var uiRoot = GameObject.Find("--- UI ---");
            if (uiRoot == null) { Debug.LogError("[RebuildMainMenuUI] '--- UI ---' 오브젝트 없음"); return; }

            var canvasGO = uiRoot.transform.Find("Canvas_MainMenu")?.gameObject;
            if (canvasGO == null) { Debug.LogError("[RebuildMainMenuUI] Canvas_MainMenu 없음"); return; }

            // 3. 기존 자식 전부 삭제
            for (int i = canvasGO.transform.childCount - 1; i >= 0; i--)
                Undo.DestroyObjectImmediate(canvasGO.transform.GetChild(i).gameObject);

            // Canvas RectTransform 풀스크린
            var canvasRect = canvasGO.GetComponent<RectTransform>();
            if (canvasRect == null) canvasRect = canvasGO.AddComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.offsetMin = Vector2.zero;
            canvasRect.offsetMax = Vector2.zero;

            // CanvasScaler: 1920x1080
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            // 4. EventSystem 없으면 추가
            if (Object.FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
                Undo.RegisterCreatedObjectUndo(es, "Create EventSystem");
                Debug.Log("[RebuildMainMenuUI] EventSystem 생성");
            }

            // 5. 배경 이미지 (선택적: 단색 패널)
            var bg = CreateUIObject("Background", canvasGO.transform);
            StretchFull(bg);
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.12f, 0.08f, 1f); // 짙은 초록빛 배경

            // 6. TitleText
            var titleGO = CreateUIObject("TitleText", canvasGO.transform);
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.75f);
            titleRect.anchorMax = new Vector2(0.5f, 0.75f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(600, 120);
            var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
            titleTMP.text = "SeedMind";
            titleTMP.fontSize = 72;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = new Color(0.9f, 0.95f, 0.7f, 1f);

            // 7. ButtonPanel (Vertical Layout)
            var panel = CreateUIObject("ButtonPanel", canvasGO.transform);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.35f);
            panelRect.anchorMax = new Vector2(0.5f, 0.35f);
            panelRect.anchoredPosition = Vector2.zero;
            panelRect.sizeDelta = new Vector2(320, 300);
            var vlg = panel.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 16;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(0, 0, 0, 0);

            // 8. 버튼 생성
            CreateMenuButton("Btn_NewGame",  "새 게임",  panel.transform);
            CreateMenuButton("Btn_Continue", "이어하기", panel.transform);
            CreateMenuButton("Btn_Settings", "설정",     panel.transform);
            CreateMenuButton("Btn_Quit",     "종료",     panel.transform);

            // 9. SettingsPanel (비활성)
            var settingsPanel = CreateUIObject("SettingsPanel", canvasGO.transform);
            StretchFull(settingsPanel);
            var spImg = settingsPanel.AddComponent<Image>();
            spImg.color = new Color(0f, 0f, 0f, 0.85f);
            settingsPanel.SetActive(false);

            // 10. 씬 저장
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[RebuildMainMenuUI] 완료 — 씬 저장됨");
        }

        static GameObject CreateUIObject(string name, Transform parent)
        {
            var go = new GameObject(name);
            go.AddComponent<RectTransform>();
            go.transform.SetParent(parent, false);
            Undo.RegisterCreatedObjectUndo(go, "Create " + name);
            return go;
        }

        static void StretchFull(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        static void CreateMenuButton(string goName, string label, Transform parent)
        {
            var go = CreateUIObject(goName, parent);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(320, 56);

            // 배경
            var img = go.AddComponent<Image>();
            img.color = new Color(0.18f, 0.28f, 0.18f, 1f);

            // Button 컴포넌트
            var btn = go.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.18f, 0.28f, 0.18f, 1f);
            colors.highlightedColor = new Color(0.28f, 0.42f, 0.28f, 1f);
            colors.pressedColor = new Color(0.12f, 0.20f, 0.12f, 1f);
            btn.colors = colors;

            // 텍스트
            var textGO = CreateUIObject("Text", go.transform);
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 28;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
        }
    }
}
