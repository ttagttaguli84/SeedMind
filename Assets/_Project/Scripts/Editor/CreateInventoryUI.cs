using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using SeedMind.UI;

namespace SeedMind.Editor
{
    /// <summary>
    /// SCN_Farm에 인벤토리 UI 계층(ToolbarPanel, InventoryPanel, TooltipPanel)을 생성하고
    /// PFB_UI_SlotUI 프리팹을 생성한다.
    /// -> see docs/mcp/inventory-tasks.md T-3
    /// </summary>
    public static class CreateInventoryUI
    {
        private const string SlotPrefabPath = "Assets/_Project/Prefabs/UI/PFB_UI_SlotUI.prefab";

        [MenuItem("SeedMind/Create Inventory UI")]
        public static void CreateAll()
        {
            EnsureFolder("Assets/_Project/Prefabs");
            EnsureFolder("Assets/_Project/Prefabs/UI");

            CreateSlotPrefab();
            CreateToolbarPanel();
            CreateInventoryPanel();
            CreateTooltipPanel();

            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("[CreateInventoryUI] 인벤토리 UI 생성 완료.");
        }

        // ── SlotUI 프리팹 생성 ──────────────────────────────────────

        private static void CreateSlotPrefab()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(SlotPrefabPath) != null)
            {
                Debug.Log("[CreateInventoryUI] SlotUI 프리팹 이미 존재, 스킵.");
                return;
            }

            // 루트
            var root = new GameObject("SlotUI");
            root.AddComponent<RectTransform>();
            var rootRT = root.GetComponent<RectTransform>();
            rootRT.sizeDelta = new Vector2(64, 64);

            // Background
            var bg = CreateChild(root, "Background");
            var bgImg = bg.AddComponent<Image>();
            bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            // Icon
            var icon = CreateChild(root, "Icon");
            var iconImg = icon.AddComponent<Image>();
            iconImg.raycastTarget = false;
            iconImg.preserveAspect = true;
            SetRectFill(icon, 4f); // 4px 마진

            // QuantityText
            var qty = CreateChild(root, "QuantityText");
            var qtyTmp = qty.AddComponent<TextMeshProUGUI>();
            qtyTmp.alignment = TextAlignmentOptions.BottomRight;
            qtyTmp.fontSize = 14;
            qtyTmp.raycastTarget = false;
            SetRectFill(qty, 2f);

            // QualityBorder
            var border = CreateChild(root, "QualityBorder");
            var borderImg = border.AddComponent<Image>();
            borderImg.raycastTarget = false;
            borderImg.enabled = false;
            SetRectFull(border);

            // SelectedHighlight
            var highlight = CreateChild(root, "SelectedHighlight");
            var hlImg = highlight.AddComponent<Image>();
            hlImg.color = new Color(1f, 0.9f, 0.3f, 0.5f);
            hlImg.raycastTarget = false;
            hlImg.enabled = false;
            SetRectFull(highlight);

            // SlotUI 컴포넌트
            var slotUI = root.AddComponent<SlotUI>();

            // 참조 연결 (Reflection 우회: SerializedObject 활용)
            // 프리팹 저장 후 직접 SerializedObject로 연결
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, SlotPrefabPath);
            Object.DestroyImmediate(root);

            // SerializedObject로 필드 연결
            var so = new SerializedObject(prefab.GetComponent<SlotUI>());
            so.FindProperty("_icon").objectReferenceValue           = prefab.transform.Find("Icon").GetComponent<Image>();
            so.FindProperty("_quantityText").objectReferenceValue   = prefab.transform.Find("QuantityText").GetComponent<TextMeshProUGUI>();
            so.FindProperty("_qualityBorder").objectReferenceValue  = prefab.transform.Find("QualityBorder").GetComponent<Image>();
            so.FindProperty("_selectedHighlight").objectReferenceValue = prefab.transform.Find("SelectedHighlight").GetComponent<Image>();
            so.ApplyModifiedProperties();

            PrefabUtility.SavePrefabAsset(prefab);
            Debug.Log($"[CreateInventoryUI] SlotUI 프리팹 생성 완료: {SlotPrefabPath}");
        }

        // ── ToolbarPanel 생성 ────────────────────────────────────────

        private static void CreateToolbarPanel()
        {
            var canvasHUD = GameObject.Find("Canvas_HUD");
            if (canvasHUD == null) { Debug.LogError("[CreateInventoryUI] Canvas_HUD를 찾을 수 없습니다."); return; }

            if (canvasHUD.transform.Find("ToolbarPanel") != null)
            {
                Debug.Log("[CreateInventoryUI] ToolbarPanel 이미 존재, 스킵.");
                return;
            }

            var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlotPrefabPath);

            var panel = new GameObject("ToolbarPanel");
            panel.transform.SetParent(canvasHUD.transform, false);
            var panelRT = panel.AddComponent<RectTransform>();

            // 하단 중앙 앵커
            panelRT.anchorMin = new Vector2(0.5f, 0f);
            panelRT.anchorMax = new Vector2(0.5f, 0f);
            panelRT.pivot     = new Vector2(0.5f, 0f);
            panelRT.anchoredPosition = new Vector2(0f, 20f);

            var hlg = panel.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth  = false;
            hlg.childForceExpandHeight = false;

            var csf = panel.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

            // 툴바 슬롯 8개 생성
            for (int i = 0; i < 8; i++)
            {
                GameObject slot;
                if (slotPrefab != null)
                    slot = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab, panel.transform);
                else
                {
                    slot = new GameObject($"ToolSlot_{i}");
                    slot.transform.SetParent(panel.transform, false);
                    slot.AddComponent<RectTransform>().sizeDelta = new Vector2(64, 64);
                }
                slot.name = $"ToolSlot_{i}";
            }

            Debug.Log("[CreateInventoryUI] ToolbarPanel 생성 완료.");
        }

        // ── InventoryPanel 생성 ──────────────────────────────────────

        private static void CreateInventoryPanel()
        {
            var canvasHUD = GameObject.Find("Canvas_HUD");
            if (canvasHUD == null) return;

            if (canvasHUD.transform.Find("InventoryPanel") != null)
            {
                Debug.Log("[CreateInventoryUI] InventoryPanel 이미 존재, 스킵.");
                return;
            }

            var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlotPrefabPath);

            var panel = new GameObject("InventoryPanel");
            panel.transform.SetParent(canvasHUD.transform, false);
            var panelRT = panel.AddComponent<RectTransform>();
            panelRT.anchorMin = new Vector2(0.5f, 0.5f);
            panelRT.anchorMax = new Vector2(0.5f, 0.5f);
            panelRT.sizeDelta = new Vector2(360f, 460f);

            var bgImg = panel.AddComponent<Image>();
            bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

            // 배낭 그리드
            var grid = CreateChild(panel, "BackpackGrid");
            var gridRT = grid.GetComponent<RectTransform>();
            gridRT.anchorMin = new Vector2(0f, 0.1f);
            gridRT.anchorMax = new Vector2(1f, 1f);
            gridRT.offsetMin = new Vector2(10f, 0f);
            gridRT.offsetMax = new Vector2(-10f, -10f);

            var glg = grid.AddComponent<GridLayoutGroup>();
            glg.constraintCount = 5;                              // 5열 -> see docs/systems/inventory-system.md 섹션 2.1
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.cellSize  = new Vector2(64f, 64f);
            glg.spacing   = new Vector2(4f, 4f);

            // 배낭 슬롯 20개 생성 (기본값)
            for (int i = 0; i < 20; i++)
            {
                GameObject slot;
                if (slotPrefab != null)
                    slot = (GameObject)PrefabUtility.InstantiatePrefab(slotPrefab, grid.transform);
                else
                {
                    slot = new GameObject($"Slot_{i}");
                    slot.transform.SetParent(grid.transform, false);
                    slot.AddComponent<RectTransform>().sizeDelta = new Vector2(64, 64);
                }
                slot.name = $"Slot_{i}";
            }

            // 정렬 버튼
            var sortBtn = new GameObject("SortButton");
            sortBtn.transform.SetParent(panel.transform, false);
            var sortRT = sortBtn.AddComponent<RectTransform>();
            sortRT.anchorMin = new Vector2(0f, 0f);
            sortRT.anchorMax = new Vector2(0f, 0f);
            sortRT.anchoredPosition = new Vector2(50f, 20f);
            sortRT.sizeDelta = new Vector2(80f, 30f);
            sortBtn.AddComponent<Button>();
            sortBtn.AddComponent<Image>().color = new Color(0.3f, 0.3f, 0.3f);

            var sortText = CreateChild(sortBtn, "Text");
            var sortTmp = sortText.AddComponent<TextMeshProUGUI>();
            sortTmp.text = "정렬";
            sortTmp.fontSize = 14;
            sortTmp.alignment = TextAlignmentOptions.Center;

            // InventoryUI 컴포넌트 추가
            var invUI = panel.AddComponent<InventoryUI>();

            // 기본 비활성화
            panel.SetActive(false);

            Debug.Log("[CreateInventoryUI] InventoryPanel 생성 완료.");
        }

        // ── TooltipPanel 생성 ────────────────────────────────────────

        private static void CreateTooltipPanel()
        {
            var canvasOverlay = GameObject.Find("Canvas_Overlay");
            if (canvasOverlay == null)
            {
                // include_inactive=true: Canvas_Overlay는 비활성 상태일 수 있음
                // -> see docs/mcp/progress.md 실전 메모
                var all = Resources.FindObjectsOfTypeAll<Canvas>();
                foreach (var c in all)
                    if (c.name == "Canvas_Overlay") { canvasOverlay = c.gameObject; break; }
            }
            if (canvasOverlay == null) { Debug.LogError("[CreateInventoryUI] Canvas_Overlay를 찾을 수 없습니다."); return; }

            if (canvasOverlay.transform.Find("TooltipPanel") != null)
            {
                Debug.Log("[CreateInventoryUI] TooltipPanel 이미 존재, 스킵.");
                return;
            }

            var panel = new GameObject("TooltipPanel");
            panel.transform.SetParent(canvasOverlay.transform, false);
            var panelRT = panel.AddComponent<RectTransform>();
            panelRT.sizeDelta = new Vector2(200f, 120f);

            var bg = panel.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.05f, 0.95f);

            var cg = panel.AddComponent<CanvasGroup>();
            cg.alpha = 0f;

            // 아이템 이름
            var nameGO = CreateChild(panel, "ItemNameText");
            var nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
            nameTmp.fontSize = 16;
            nameTmp.fontStyle = FontStyles.Bold;
            SetRectTop(nameGO, 10f, 30f);

            // 카테고리
            var catGO = CreateChild(panel, "CategoryText");
            var catTmp = catGO.AddComponent<TextMeshProUGUI>();
            catTmp.fontSize = 12;
            catTmp.color = new Color(0.7f, 0.7f, 0.7f);
            SetRectBelow(catGO, nameGO, 20f);

            // 판매가
            var priceGO = CreateChild(panel, "PriceText");
            var priceTmp = priceGO.AddComponent<TextMeshProUGUI>();
            priceTmp.fontSize = 12;
            SetRectBelow(priceGO, catGO, 20f);

            // 품질 아이콘 (작은 색상 블록)
            var qualGO = CreateChild(panel, "QualityIcon");
            var qualImg = qualGO.AddComponent<Image>();
            var qualRT = qualGO.GetComponent<RectTransform>();
            qualRT.anchorMin = new Vector2(1f, 1f);
            qualRT.anchorMax = new Vector2(1f, 1f);
            qualRT.sizeDelta = new Vector2(12f, 12f);
            qualRT.anchoredPosition = new Vector2(-8f, -8f);

            // TooltipUI 컴포넌트
            var tooltip = panel.AddComponent<TooltipUI>();

            // SerializedObject로 필드 연결
            var so = new SerializedObject(tooltip);
            so.FindProperty("_itemNameText").objectReferenceValue  = nameTmp;
            so.FindProperty("_categoryText").objectReferenceValue  = catTmp;
            so.FindProperty("_priceText").objectReferenceValue     = priceTmp;
            so.FindProperty("_qualityIcon").objectReferenceValue   = qualImg;
            so.FindProperty("_canvasGroup").objectReferenceValue   = cg;
            so.ApplyModifiedProperties();

            Debug.Log("[CreateInventoryUI] TooltipPanel 생성 완료.");
        }

        // ── 헬퍼 ─────────────────────────────────────────────────────

        private static GameObject CreateChild(GameObject parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void SetRectFull(GameObject go)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static void SetRectFill(GameObject go, float margin)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) return;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(margin, margin);
            rt.offsetMax = new Vector2(-margin, -margin);
        }

        private static void SetRectTop(GameObject go, float topOffset, float height)
        {
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) return;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.offsetMin = new Vector2(10f, -topOffset - height);
            rt.offsetMax = new Vector2(-10f, -topOffset);
        }

        private static void SetRectBelow(GameObject go, GameObject above, float height)
        {
            var rtAbove = above.GetComponent<RectTransform>();
            if (rtAbove == null) return;
            var rt = go.GetComponent<RectTransform>();
            if (rt == null) return;
            rt.anchorMin = rtAbove.anchorMin;
            rt.anchorMax = rtAbove.anchorMax;
            rt.offsetMin = new Vector2(rtAbove.offsetMin.x, rtAbove.offsetMin.y - height - 4f);
            rt.offsetMax = new Vector2(rtAbove.offsetMax.x, rtAbove.offsetMin.y - 4f);
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                int lastSlash = path.LastIndexOf('/');
                string parent = path.Substring(0, lastSlash);
                string child  = path.Substring(lastSlash + 1);
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
