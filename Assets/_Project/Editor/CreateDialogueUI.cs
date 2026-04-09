// Editor 스크립트: DialoguePanel UI 일괄 생성
// Canvas_Overlay 아래에 DialoguePanel 및 자식 요소 구성
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace SeedMind.Editor
{
    public static class CreateDialogueUI
    {
        [MenuItem("SeedMind/Tools/Create Dialogue UI")]
        public static void CreateAll()
        {
            // Canvas_Overlay 탐색 (비활성 포함)
            var canvasGO = GameObject.Find("Canvas_Overlay");
            if (canvasGO == null)
            {
                var all = Resources.FindObjectsOfTypeAll<GameObject>();
                foreach (var go in all)
                {
                    if (go.name == "Canvas_Overlay" && go.scene.IsValid())
                    {
                        canvasGO = go;
                        break;
                    }
                }
            }
            if (canvasGO == null)
            {
                Debug.LogError("[SeedMind] Canvas_Overlay를 찾을 수 없습니다.");
                return;
            }

            // 기존 DialoguePanel 탐색 후 재활용 or 신규 생성
            Transform existingPanel = canvasGO.transform.Find("DialoguePanel");
            if (existingPanel != null)
            {
                // 기존 오브젝트 삭제 후 재생성
                Object.DestroyImmediate(existingPanel.gameObject);
            }

            // --- DialoguePanel 루트 ---
            var panelGO = new GameObject("DialoguePanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            var panelRect = panelGO.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0.35f);
            panelRect.offsetMin = new Vector2(20f, 20f);
            panelRect.offsetMax = new Vector2(-20f, 0f);

            // --- BG_Dialogue (배경) ---
            var bgGO = new GameObject("BG_Dialogue");
            bgGO.transform.SetParent(panelGO.transform, false);
            var bgRect = bgGO.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            var bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            bgImg.raycastTarget = true;

            // --- PortraitImage (초상화) ---
            var portraitGO = new GameObject("PortraitImage");
            portraitGO.transform.SetParent(panelGO.transform, false);
            var portraitRect = portraitGO.AddComponent<RectTransform>();
            portraitRect.anchorMin = new Vector2(0f, 0.1f);
            portraitRect.anchorMax = new Vector2(0.2f, 0.9f);
            portraitRect.offsetMin = new Vector2(10f, 0f);
            portraitRect.offsetMax = Vector2.zero;
            var portraitImg = portraitGO.AddComponent<Image>();
            portraitImg.color = Color.white;

            // --- SpeakerNameText (화자 이름) ---
            var nameGO = new GameObject("SpeakerNameText");
            nameGO.transform.SetParent(panelGO.transform, false);
            var nameRect = nameGO.AddComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0.22f, 0.75f);
            nameRect.anchorMax = new Vector2(0.7f, 0.95f);
            nameRect.offsetMin = Vector2.zero;
            nameRect.offsetMax = Vector2.zero;
            var nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
            nameTmp.fontSize = 20f;
            nameTmp.fontStyle = FontStyles.Bold;
            nameTmp.color = new Color(1f, 0.9f, 0.6f, 1f);
            nameTmp.text = "화자 이름";

            // --- DialogueText (대사) ---
            var textGO = new GameObject("DialogueText");
            textGO.transform.SetParent(panelGO.transform, false);
            var textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.22f, 0.1f);
            textRect.anchorMax = new Vector2(0.98f, 0.72f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            var dialogueTmp = textGO.AddComponent<TextMeshProUGUI>();
            dialogueTmp.fontSize = 16f;
            dialogueTmp.color = Color.white;
            dialogueTmp.text = "대사 텍스트";

            // --- ChoiceContainer (선택지 컨테이너) ---
            var choiceGO = new GameObject("ChoiceContainer");
            choiceGO.transform.SetParent(panelGO.transform, false);
            var choiceRect = choiceGO.AddComponent<RectTransform>();
            choiceRect.anchorMin = new Vector2(0.22f, 0.1f);
            choiceRect.anchorMax = new Vector2(0.98f, 0.72f);
            choiceRect.offsetMin = Vector2.zero;
            choiceRect.offsetMax = Vector2.zero;
            var layout = choiceGO.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // --- DialogueUI 컴포넌트 부착 ---
            var dialogueUI = panelGO.AddComponent<SeedMind.UI.DialogueUI>();
            // Inspector 직렬화 필드는 SerializedObject로 설정
            var so = new SerializedObject(dialogueUI);
            so.FindProperty("_dialoguePanel").objectReferenceValue = panelGO;
            so.FindProperty("_portraitImage").objectReferenceValue = portraitImg;
            so.FindProperty("_speakerNameText").objectReferenceValue = nameTmp;
            so.FindProperty("_dialogueText").objectReferenceValue = dialogueTmp;
            so.FindProperty("_choiceContainer").objectReferenceValue = choiceGO.transform;
            so.ApplyModifiedProperties();

            // --- 프리팹 저장 ---
            string prefabPath = "Assets/_Project/Prefabs/UI/PFB_UI_DialoguePanel.prefab";
            PrefabUtility.SaveAsPrefabAssetAndConnect(panelGO, prefabPath, InteractionMode.AutomatedAction);

            // 씬 저장
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

            Debug.Log("[SeedMind] DialoguePanel UI 생성 및 프리팹 저장 완료: " + prefabPath);
        }
    }
}
