// Editor 전용: AchievementItemUI 프리팹 일괄 생성
// -> see docs/systems/achievement-architecture.md 섹션 8.3
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class CreateAchievementItemPrefab
{
    [MenuItem("SeedMind/Create AchievementItemUI Prefab")]
    public static void CreatePrefab()
    {
        // 루트 오브젝트 생성
        var root = new GameObject("AchievementItemUI");
        var script = root.AddComponent<SeedMind.UI.AchievementItemUI>();

        // 자식: IconImage
        var iconGO = new GameObject("ItemIcon");
        iconGO.transform.SetParent(root.transform, false);
        var iconImg = iconGO.AddComponent<Image>();

        // 자식: TitleText
        var titleGO = new GameObject("ItemTitle");
        titleGO.transform.SetParent(root.transform, false);
        var titleTxt = titleGO.AddComponent<TextMeshProUGUI>();
        titleTxt.text = "업적 이름";

        // 자식: DescriptionText
        var descGO = new GameObject("ItemDescription");
        descGO.transform.SetParent(root.transform, false);
        var descTxt = descGO.AddComponent<TextMeshProUGUI>();
        descTxt.text = "업적 설명";

        // 자식: ProgressBar
        var progressGO = new GameObject("ProgressBar");
        progressGO.transform.SetParent(root.transform, false);
        var slider = progressGO.AddComponent<Slider>();

        // 자식: ProgressText
        var progressTextGO = new GameObject("ProgressText");
        progressTextGO.transform.SetParent(root.transform, false);
        var progressTxt = progressTextGO.AddComponent<TextMeshProUGUI>();
        progressTxt.text = "0/0";

        // 자식: CompletedOverlay
        var completedGO = new GameObject("CompletedOverlay");
        completedGO.transform.SetParent(root.transform, false);
        completedGO.AddComponent<Image>();
        completedGO.SetActive(false);

        // 자식: HiddenOverlay
        var hiddenGO = new GameObject("HiddenOverlay");
        hiddenGO.transform.SetParent(root.transform, false);
        hiddenGO.AddComponent<Image>();
        hiddenGO.SetActive(false);

        // SerializeField 연결 (SerializedObject 사용)
        var so = new SerializedObject(script);
        so.FindProperty("_iconImage").objectReferenceValue = iconImg;
        so.FindProperty("_titleText").objectReferenceValue = titleTxt;
        so.FindProperty("_descriptionText").objectReferenceValue = descTxt;
        so.FindProperty("_progressBar").objectReferenceValue = slider;
        so.FindProperty("_progressText").objectReferenceValue = progressTxt;
        so.FindProperty("_completedOverlay").objectReferenceValue = completedGO;
        so.FindProperty("_hiddenOverlay").objectReferenceValue = hiddenGO;
        so.ApplyModifiedProperties();

        // 프리팹으로 저장
        string prefabPath = "Assets/_Project/Prefabs/UI/AchievementItemUI.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[CreateAchievementItemPrefab] Prefab saved: {prefabPath}");
    }
}
#endif
