// Editor 스크립트: T-5 NPC 시스템 참조 연결
// DialogueUI → DialogueSystem 참조 설정
using UnityEngine;
using UnityEditor;
using SeedMind.NPC;
using SeedMind.UI;

namespace SeedMind.Editor
{
    public static class SetupNPCReferences
    {
        [MenuItem("SeedMind/Tools/Setup NPC References (T-5)")]
        public static void Setup()
        {
            GameObject FindInactive(string n)
            {
                foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                    if (go.name == n && go.scene.IsValid()) return go;
                return null;
            }

            // DialogueUI 컴포넌트 탐색
            var dialoguePanelGO = FindInactive("DialoguePanel");
            if (dialoguePanelGO == null)
            {
                Debug.LogError("[SeedMind] DialoguePanel을 찾을 수 없습니다.");
                return;
            }
            var dialogueUI = dialoguePanelGO.GetComponent<DialogueUI>();
            if (dialogueUI == null)
            {
                Debug.LogError("[SeedMind] DialogueUI 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            // DialogueSystem GO 탐색
            var dialogueSystemGO = FindInactive("DialogueSystem");
            if (dialogueSystemGO == null)
            {
                Debug.LogError("[SeedMind] DialogueSystem GO를 찾을 수 없습니다.");
                return;
            }
            var dialogueSystem = dialogueSystemGO.GetComponent<DialogueSystem>();
            if (dialogueSystem == null)
            {
                Debug.LogError("[SeedMind] DialogueSystem 컴포넌트를 찾을 수 없습니다.");
                return;
            }

            // SerializedObject로 참조 연결
            var so = new SerializedObject(dialogueUI);
            so.FindProperty("_dialogueSystem").objectReferenceValue = dialogueSystem;
            so.ApplyModifiedProperties();

            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("[SeedMind] T-5: DialogueUI → DialogueSystem 참조 연결 완료.");
        }
    }
}
