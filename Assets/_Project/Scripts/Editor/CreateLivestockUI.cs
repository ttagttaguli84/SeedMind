// CreateLivestockUI — Editor 전용: 축산 UI 패널 + PFB_AnimalSlot 프리팹 일괄 생성
// -> see docs/mcp/livestock-tasks.md L-7
// -> see docs/systems/livestock-architecture.md 섹션 6
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using SeedMind.UI;

namespace SeedMind.Editor
{
    public static class CreateLivestockUI
    {
        private const string PrefabPath = "Assets/_Project/Prefabs/UI/PFB_AnimalSlot.prefab";

        [MenuItem("SeedMind/Livestock/Create Livestock UI")]
        public static void CreateAll()
        {
            CreateAnimalShopPanel();
            CreateAnimalCarePanel();
            CreateAnimalSlotPrefab();

            EditorSceneManager.SaveOpenScenes();
            Debug.Log("[CreateLivestockUI] 축산 UI 생성 완료.");
        }

        // ────────────────────────────────────────────────
        // Panel_AnimalShop (Canvas_Overlay)
        // ────────────────────────────────────────────────

        private static void CreateAnimalShopPanel()
        {
            var canvasOverlay = FindCanvas("Canvas_Overlay");
            if (canvasOverlay == null) return;

            if (canvasOverlay.transform.Find("Panel_AnimalShop") != null)
            {
                Debug.Log("[CreateLivestockUI] Panel_AnimalShop 이미 존재 — 스킵");
                return;
            }

            // 루트 패널
            var panel = CreateUIPanel("Panel_AnimalShop", canvasOverlay.transform);
            var shopUI = panel.AddComponent<AnimalShopUI>();

            // 내부 구조
            var shopPanelInner = CreateUIPanel("ShopPanel", panel.transform);

            // 닫기 버튼
            var closeBtn = CreateButton("BtnClose", shopPanelInner.transform, "X");

            // 슬롯 컨테이너 (ScrollRect 내부)
            var scrollGO = new GameObject("ScrollView", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollGO.transform.SetParent(shopPanelInner.transform, false);
            var scrollRT = scrollGO.GetComponent<RectTransform>();
            scrollRT.anchorMin = new Vector2(0, 0.1f);
            scrollRT.anchorMax = new Vector2(1, 0.9f);
            scrollRT.offsetMin = scrollRT.offsetMax = Vector2.zero;

            var contentGO = new GameObject("Content", typeof(RectTransform));
            contentGO.transform.SetParent(scrollGO.transform, false);
            var contentRT = contentGO.GetComponent<RectTransform>();
            contentRT.anchorMin = Vector2.zero;
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.offsetMin = contentRT.offsetMax = Vector2.zero;
            contentGO.AddComponent<VerticalLayoutGroup>();
            contentGO.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = scrollGO.GetComponent<ScrollRect>();
            sr.content = contentRT;
            sr.vertical = true;
            sr.horizontal = false;

            // ShopUI 필드 연결
            SetPrivateField(shopUI, "_shopPanel", shopPanelInner);
            SetPrivateField(shopUI, "_animalSlotContainer", contentRT.transform);
            SetPrivateField(shopUI, "_closeButton", closeBtn.GetComponent<Button>());
            // _animalSlotPrefab은 PFB_AnimalSlot 생성 후 별도 연결

            panel.SetActive(false);
            EditorUtility.SetDirty(panel);
            Debug.Log("[CreateLivestockUI] Panel_AnimalShop 생성 완료.");
        }

        // ────────────────────────────────────────────────
        // Panel_AnimalCare (Canvas_HUD)
        // ────────────────────────────────────────────────

        private static void CreateAnimalCarePanel()
        {
            var canvasHUD = FindCanvas("Canvas_HUD");
            if (canvasHUD == null) return;

            if (canvasHUD.transform.Find("Panel_AnimalCare") != null)
            {
                Debug.Log("[CreateLivestockUI] Panel_AnimalCare 이미 존재 — 스킵");
                return;
            }

            var panel = CreateUIPanel("Panel_AnimalCare", canvasHUD.transform);
            var careUI = panel.AddComponent<AnimalCareUI>();

            var carePanel = CreateUIPanel("CarePanel", panel.transform);

            // 동물 이름
            var nameText = CreateTMPText("TxtAnimalName", carePanel.transform, "동물이름");

            // 행복도 슬라이더
            var sliderGO = new GameObject("SliderHappiness", typeof(RectTransform), typeof(Slider));
            sliderGO.transform.SetParent(carePanel.transform, false);
            var sliderRT = sliderGO.GetComponent<RectTransform>();
            sliderRT.sizeDelta = new Vector2(300, 30);
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGO.transform, false);
            var fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGO.transform.SetParent(fillArea.transform, false);
            var slider = sliderGO.GetComponent<Slider>();
            slider.fillRect = fillGO.GetComponent<RectTransform>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;

            // 버튼 3종
            var feedBtn     = CreateButton("BtnFeed",    carePanel.transform, "먹이주기");
            var petBtn      = CreateButton("BtnPet",     carePanel.transform, "쓰다듬기");
            var collectBtn  = CreateButton("BtnCollect", carePanel.transform, "수확");

            // AnimalCareUI 필드 연결
            SetPrivateField(careUI, "_carePanel",      carePanel);
            SetPrivateField(careUI, "_animalNameText", nameText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(careUI, "_happinessBar",   slider);
            SetPrivateField(careUI, "_feedButton",     feedBtn.GetComponent<Button>());
            SetPrivateField(careUI, "_petButton",      petBtn.GetComponent<Button>());
            SetPrivateField(careUI, "_collectButton",  collectBtn.GetComponent<Button>());

            panel.SetActive(false);
            EditorUtility.SetDirty(panel);
            Debug.Log("[CreateLivestockUI] Panel_AnimalCare 생성 완료.");
        }

        // ────────────────────────────────────────────────
        // PFB_AnimalSlot 프리팹
        // ────────────────────────────────────────────────

        private static void CreateAnimalSlotPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null)
            {
                Debug.Log("[CreateLivestockUI] PFB_AnimalSlot 이미 존재 — 스킵");
                return;
            }

            System.IO.Directory.CreateDirectory("Assets/_Project/Prefabs/UI");

            var root = new GameObject("PFB_AnimalSlot", typeof(RectTransform));
            root.AddComponent<HorizontalLayoutGroup>();
            var slot = root.AddComponent<AnimalSlotUI>();

            var icon    = CreateUIImage("ImgIcon",    root.transform);
            var nameText = CreateTMPText("TxtName",  root.transform, "동물이름");
            var price   = CreateTMPText("TxtPrice",  root.transform, "0G");
            var btn     = CreateButton("BtnSelect",  root.transform, "선택");

            SetPrivateField(slot, "_animalIcon",   icon.GetComponent<Image>());
            SetPrivateField(slot, "_nameText",     nameText.GetComponent<TextMeshProUGUI>());
            SetPrivateField(slot, "_priceText",    price.GetComponent<TextMeshProUGUI>());
            SetPrivateField(slot, "_selectButton", btn.GetComponent<Button>());

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            // Panel_AnimalShop에 prefab 연결
            var shopPanelGO = GameObject.Find("Panel_AnimalShop");
            if (shopPanelGO != null)
            {
                var shopUI = shopPanelGO.GetComponent<AnimalShopUI>();
                if (shopUI != null)
                    SetPrivateField(shopUI, "_animalSlotPrefab", prefab);
            }

            Debug.Log($"[CreateLivestockUI] PFB_AnimalSlot 생성: {PrefabPath}");
        }

        // ────────────────────────────────────────────────
        // 헬퍼
        // ────────────────────────────────────────────────

        private static GameObject FindCanvas(string canvasName)
        {
            var canvasGO = GameObject.Find(canvasName);
            if (canvasGO == null)
            {
                foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                {
                    if (c.name == canvasName) { canvasGO = c.gameObject; break; }
                }
            }
            if (canvasGO == null)
                Debug.LogError($"[CreateLivestockUI] {canvasName}를 찾을 수 없습니다.");
            return canvasGO;
        }

        private static GameObject CreateUIPanel(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return go;
        }

        private static GameObject CreateUIImage(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreateButton(string name, Transform parent, string label)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var textGO = CreateTMPText("Text", go.transform, label);
            var rt = textGO.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return go;
        }

        private static GameObject CreateTMPText(string name, Transform parent, string text)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            go.GetComponent<TextMeshProUGUI>().text = text;
            return go;
        }

        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var type = obj.GetType();
            while (type != null)
            {
                var field = type.GetField(fieldName,
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                if (field != null) { field.SetValue(obj, value); return; }
                type = type.BaseType;
            }
            Debug.LogWarning($"[CreateLivestockUI] 필드 없음: {fieldName}");
        }
    }
}
