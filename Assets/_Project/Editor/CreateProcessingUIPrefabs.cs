#if UNITY_EDITOR
// Editor 전용: ProcessingPanel UI 프리팹 생성
// -> see docs/systems/processing-architecture.md 섹션 7.2
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeedMind.UI;

public static class CreateProcessingUIPrefabs
{
    [MenuItem("SeedMind/Create Processing UI Prefabs")]
    public static void CreateAll()
    {
        const string folder = "Assets/_Project/Prefabs/UI";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "UI");

        CreateRecipeSlotPrefab(folder);
        CreateProcessingSlotPrefab(folder);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateProcessingUIPrefabs] ProcessingUI 프리팹 생성 완료.");
    }

    private static void CreateRecipeSlotPrefab(string folder)
    {
        string path = $"{folder}/PFB_RecipeSlot.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log("[CreateProcessingUIPrefabs] PFB_RecipeSlot 이미 존재, 스킵.");
            return;
        }

        var root = new GameObject("PFB_RecipeSlot");
        root.AddComponent<RecipeSlotUI>();

        var icon = new GameObject("Icon");
        icon.transform.SetParent(root.transform, false);
        icon.AddComponent<Image>();

        var nameGo = new GameObject("RecipeName");
        nameGo.transform.SetParent(root.transform, false);
        nameGo.AddComponent<TextMeshProUGUI>();

        var material = new GameObject("MaterialText");
        material.transform.SetParent(root.transform, false);
        material.AddComponent<TextMeshProUGUI>();

        var time = new GameObject("TimeText");
        time.transform.SetParent(root.transform, false);
        time.AddComponent<TextMeshProUGUI>();

        var btnGo = new GameObject("SelectButton");
        btnGo.transform.SetParent(root.transform, false);
        var btn = btnGo.AddComponent<Button>();

        // SerializeField 연결
        var slotUI = root.GetComponent<RecipeSlotUI>();
        var so = new SerializedObject(slotUI);
        so.FindProperty("_icon").objectReferenceValue = icon.GetComponent<Image>();
        so.FindProperty("_recipeName").objectReferenceValue = nameGo.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_materialText").objectReferenceValue = material.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_timeText").objectReferenceValue = time.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_selectButton").objectReferenceValue = btn;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log($"[CreateProcessingUIPrefabs] 생성: {path}");
    }

    private static void CreateProcessingSlotPrefab(string folder)
    {
        string path = $"{folder}/PFB_ProcessingSlot.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null)
        {
            Debug.Log("[CreateProcessingUIPrefabs] PFB_ProcessingSlot 이미 존재, 스킵.");
            return;
        }

        var root = new GameObject("PFB_ProcessingSlot");
        root.AddComponent<ProcessingSlotUI>();

        var status = new GameObject("StatusText");
        status.transform.SetParent(root.transform, false);
        status.AddComponent<TextMeshProUGUI>();

        var progress = new GameObject("ProgressBar");
        progress.transform.SetParent(root.transform, false);
        progress.AddComponent<Slider>();

        var collectGo = new GameObject("CollectButton");
        collectGo.transform.SetParent(root.transform, false);
        var collectBtn = collectGo.AddComponent<Button>();

        // SerializeField 연결
        var slotUI = root.GetComponent<ProcessingSlotUI>();
        var so = new SerializedObject(slotUI);
        so.FindProperty("_statusText").objectReferenceValue = status.GetComponent<TextMeshProUGUI>();
        so.FindProperty("_progressBar").objectReferenceValue = progress.GetComponent<Slider>();
        so.FindProperty("_collectButton").objectReferenceValue = collectBtn;
        so.ApplyModifiedPropertiesWithoutUndo();

        PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        Debug.Log($"[CreateProcessingUIPrefabs] 생성: {path}");
    }
}
#endif
