// Editor 스크립트: TutorialManager._allSequences 배열 연결
using UnityEditor;
using UnityEngine;
using SeedMind.Tutorial;
using SeedMind.Tutorial.Data;

public class ConnectTutorialManagerArrays
{
    [MenuItem("SeedMind/Connect TutorialManager Arrays")]
    public static void Connect()
    {
        // _allSequences 배열 구성
        var guids = AssetDatabase.FindAssets("t:TutorialSequenceData", new[] { "Assets/_Project/Data/Tutorial/Sequences" });
        var sequences = new TutorialSequenceData[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[i]);
            sequences[i] = AssetDatabase.LoadAssetAtPath<TutorialSequenceData>(path);
        }

        // ContextHintData 배열 구성
        var hintGuids = AssetDatabase.FindAssets("t:ContextHintData", new[] { "Assets/_Project/Data/Tutorial/Hints" });
        var hints = new ContextHintData[hintGuids.Length];
        for (int i = 0; i < hintGuids.Length; i++)
        {
            var path = AssetDatabase.GUIDToAssetPath(hintGuids[i]);
            hints[i] = AssetDatabase.LoadAssetAtPath<ContextHintData>(path);
        }

        // 씬에서 TutorialManager 찾기
        var manager = Object.FindObjectOfType<TutorialManager>();
        if (manager == null)
        {
            Debug.LogError("[ConnectTutorialManagerArrays] TutorialManager를 씬에서 찾을 수 없습니다.");
            return;
        }

        var soManager = new SerializedObject(manager);
        var seqProp = soManager.FindProperty("_allSequences");
        seqProp.arraySize = sequences.Length;
        for (int i = 0; i < sequences.Length; i++)
            seqProp.GetArrayElementAtIndex(i).objectReferenceValue = sequences[i];
        soManager.ApplyModifiedProperties();
        EditorUtility.SetDirty(manager);

        // ContextHintSystem 배열 연결
        var hintSystem = Object.FindObjectOfType<ContextHintSystem>();
        if (hintSystem != null)
        {
            var soHint = new SerializedObject(hintSystem);
            var hintProp = soHint.FindProperty("_allHints");
            hintProp.arraySize = hints.Length;
            for (int i = 0; i < hints.Length; i++)
                hintProp.GetArrayElementAtIndex(i).objectReferenceValue = hints[i];
            soHint.ApplyModifiedProperties();
            EditorUtility.SetDirty(hintSystem);
        }

        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log($"[ConnectTutorialManagerArrays] 완료: Sequences {sequences.Length}개, Hints {hints.Length}개 연결");
    }
}
