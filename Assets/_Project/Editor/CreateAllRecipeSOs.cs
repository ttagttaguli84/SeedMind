#if UNITY_EDITOR
// Editor 전용: 가공 레시피 SO 32종 일괄 생성
// 모든 레시피 수치는 docs/content/processing-system.md 섹션 3의 canonical 정의를 기반으로 함
// -> copied from docs/content/processing-system.md 섹션 3.1~3.4, 3.7
using System.IO;
using UnityEditor;
using UnityEngine;
using SeedMind.Building.Data;
using SeedMind.Farm.Data;

public static class CreateAllRecipeSOs
{
    [MenuItem("SeedMind/Create All Recipe SOs")]
    public static void CreateAll()
    {
        EnsureFolders();
        CreateProcessingRecipes();
        CreateMillRecipes();
        CreateFermentationRecipes();
        CreateBakeryRecipes();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateAllRecipeSOs] 레시피 SO 생성 완료.");
    }

    // ── 폴더 보장 ────────────────────────────────────────────────

    private static void EnsureFolders()
    {
        MakeFolder("Assets/_Project/Resources");
        MakeFolder("Assets/_Project/Resources/Data");
        MakeFolder("Assets/_Project/Resources/Data/Recipes");
        MakeFolder("Assets/_Project/Resources/Data/Recipes/Processing");
        MakeFolder("Assets/_Project/Resources/Data/Recipes/Mill");
        MakeFolder("Assets/_Project/Resources/Data/Recipes/Fermentation");
        MakeFolder("Assets/_Project/Resources/Data/Recipes/Bakery");
    }

    private static void MakeFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string child  = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    // ── 가공소(일반) 레시피 ──────────────────────────────────────
    // -> copied from docs/content/processing-system.md 섹션 3.1

    private static void CreateProcessingRecipes()
    {
        const string root = "Assets/_Project/Resources/Data/Recipes/Processing";

        // 잼 7종 (섹션 3.1.1) — 배수 x2.0 + 50G, 4시간
        // -> copied from docs/content/processing-system.md 섹션 3.1.1
        MakeRecipe(root, "SO_Recipe_Jam_Potato",
            "recipe_jam_potato", "감자 잼", ProcessingType.Jam,
            CropCategory.Vegetable, "potato", 1,
            "jam_potato", 1, 2.0f, 50, 4f, 0, 0);

        MakeRecipe(root, "SO_Recipe_Jam_Carrot",
            "recipe_jam_carrot", "당근 잼", ProcessingType.Jam,
            CropCategory.Vegetable, "carrot", 1,
            "jam_carrot", 1, 2.0f, 50, 4f, 0, 0);

        MakeRecipe(root, "SO_Recipe_Jam_Tomato",
            "recipe_jam_tomato", "토마토 잼", ProcessingType.Jam,
            CropCategory.Fruit, "tomato", 1,
            "jam_tomato", 1, 2.0f, 50, 4f, 0, 0);

        MakeRecipe(root, "SO_Recipe_Jam_Corn",
            "recipe_jam_corn", "옥수수 잼", ProcessingType.Jam,
            CropCategory.Vegetable, "corn", 1,
            "jam_corn", 1, 2.0f, 50, 4f, 0, 0);

        MakeRecipe(root, "SO_Recipe_Jam_Strawberry",
            "recipe_jam_strawberry", "딸기 잼", ProcessingType.Jam,
            CropCategory.Fruit, "strawberry", 1,
            "item_jam_strawberry", 1, 2.0f, 50, 4f, 0, 0);

        MakeRecipe(root, "SO_Recipe_Jam_Pumpkin",
            "recipe_jam_pumpkin", "호박 잼", ProcessingType.Jam,
            CropCategory.Vegetable, "pumpkin", 1,
            "jam_pumpkin", 1, 2.0f, 50, 4f, 0, 0);

        MakeRecipe(root, "SO_Recipe_Jam_Watermelon",
            "recipe_jam_watermelon", "수박 잼", ProcessingType.Jam,
            CropCategory.Fruit, "watermelon", 1,
            "jam_watermelon", 1, 2.0f, 50, 4f, 0, 0);

        // 주스 3종 (섹션 3.1.2) — 배수 x2.5 + 30G, 6시간
        // -> copied from docs/content/processing-system.md 섹션 3.1.2
        MakeRecipe(root, "SO_Recipe_Juice_Tomato",
            "recipe_juice_tomato", "토마토 주스", ProcessingType.Juice,
            CropCategory.Fruit, "tomato", 1,
            "juice_tomato", 1, 2.5f, 30, 6f, 0, 0);

        MakeRecipe(root, "SO_Recipe_Juice_Strawberry",
            "recipe_juice_strawberry", "딸기 주스", ProcessingType.Juice,
            CropCategory.Fruit, "strawberry", 1,
            "juice_strawberry", 1, 2.5f, 30, 6f, 0, 0);

        MakeRecipe(root, "SO_Recipe_Juice_Watermelon",
            "recipe_juice_watermelon", "수박 주스", ProcessingType.Juice,
            CropCategory.Fruit, "watermelon", 1,
            "juice_watermelon", 1, 2.5f, 30, 6f, 0, 0);

        // 절임 5종 (섹션 3.1.3) — 배수 x2.0 + 30G, 4시간
        // -> copied from docs/content/processing-system.md 섹션 3.1.3
        MakeRecipe(root, "SO_Recipe_Pickle_Potato",
            "recipe_pickle_potato", "감자 절임", ProcessingType.Pickle,
            CropCategory.Vegetable, "potato", 1,
            "pickle_potato", 1, 2.0f, 30, 4f, 0, 0);

        MakeRecipe(root, "SO_Recipe_Pickle_Carrot",
            "recipe_pickle_carrot", "당근 절임", ProcessingType.Pickle,
            CropCategory.Vegetable, "carrot", 1,
            "pickle_carrot", 1, 2.0f, 30, 4f, 0, 0);

        MakeRecipe(root, "SO_Recipe_Pickle_Tomato",
            "recipe_pickle_tomato", "토마토 절임", ProcessingType.Pickle,
            CropCategory.Vegetable, "tomato", 1,
            "pickle_tomato", 1, 2.0f, 30, 4f, 0, 0);

        MakeRecipe(root, "SO_Recipe_Pickle_Corn",
            "recipe_pickle_corn", "옥수수 절임", ProcessingType.Pickle,
            CropCategory.Vegetable, "corn", 1,
            "pickle_corn", 1, 2.0f, 30, 4f, 0, 0);

        MakeRecipe(root, "SO_Recipe_Pickle_Pumpkin",
            "recipe_pickle_pumpkin", "호박 절임", ProcessingType.Pickle,
            CropCategory.Vegetable, "pumpkin", 1,
            "pickle_pumpkin", 1, 2.0f, 30, 4f, 0, 0);

        // 겨울 작물 3종 (섹션 3.1.4)
        // -> copied from docs/content/processing-system.md 섹션 3.1.4
        MakeRecipe(root, "SO_Recipe_Pickle_WinterRadish",
            "recipe_pickle_winter_radish", "겨울무 절임", ProcessingType.Pickle,
            CropCategory.Vegetable, "winter_radish", 1,
            "pickle_winter_radish", 1, 2.0f, 30, 4f, 0, 0);

        MakeRecipe(root, "SO_Recipe_Jam_Shiitake",
            "recipe_jam_shiitake", "표고버섯 잼", ProcessingType.Jam,
            CropCategory.Vegetable, "shiitake", 1,
            "jam_shiitake", 1, 2.0f, 50, 4f, 0, 0);

        MakeRecipe(root, "SO_Recipe_Pickle_Spinach",
            "recipe_pickle_spinach", "시금치 절임", ProcessingType.Pickle,
            CropCategory.Vegetable, "spinach", 1,
            "pickle_spinach", 1, 2.0f, 30, 4f, 0, 0);
    }

    // ── 제분소 레시피 (4종) ──────────────────────────────────────
    // -> copied from docs/content/processing-system.md 섹션 3.2

    private static void CreateMillRecipes()
    {
        const string root = "Assets/_Project/Resources/Data/Recipes/Mill";

        // 옥수수 가루: 옥수수 2개 → 옥수수 가루 1개, 170G, 2시간
        MakeRecipe(root, "SO_Recipe_Mill_CornFlour",
            "recipe_mill_corn_flour", "옥수수 가루", ProcessingType.Mill,
            CropCategory.Vegetable, "corn", 2,
            "item_corn_flour", 1, 1.5f, 20, 2f, 0, 0);

        // 감자 전분: 감자 3개 → 감자 전분 1개, 85G, 2시간
        MakeRecipe(root, "SO_Recipe_Mill_PotatoStarch",
            "recipe_mill_potato_starch", "감자 전분", ProcessingType.Mill,
            CropCategory.Vegetable, "potato", 3,
            "item_potato_starch", 1, 1.5f, 20, 2f, 0, 0);

        // 호박 분말: 호박 1개 → 호박 분말 1개, 320G, 3시간
        MakeRecipe(root, "SO_Recipe_Mill_PumpkinPowder",
            "recipe_mill_pumpkin_powder", "호박 분말", ProcessingType.Mill,
            CropCategory.Vegetable, "pumpkin", 1,
            "item_pumpkin_powder", 1, 1.5f, 20, 3f, 0, 0);

        // 무 분말: 겨울무 2개 → 무 분말 1개, 87G, 2시간
        MakeRecipe(root, "SO_Recipe_Mill_RadishPowder",
            "recipe_mill_radish_powder", "무 분말", ProcessingType.Mill,
            CropCategory.Vegetable, "winter_radish", 2,
            "item_radish_powder", 1, 1.5f, 20, 2f, 0, 0);
    }

    // ── 발효실 레시피 (5종) ──────────────────────────────────────
    // -> copied from docs/content/processing-system.md 섹션 3.3

    private static void CreateFermentationRecipes()
    {
        const string root = "Assets/_Project/Resources/Data/Recipes/Fermentation";

        // 딸기 와인: 딸기 3개 → 320G, 24시간
        MakeRecipe(root, "SO_Recipe_Ferm_StrawberryWine",
            "recipe_ferm_strawberry_wine", "딸기 와인", ProcessingType.Fermentation,
            CropCategory.Fruit, "strawberry", 3,
            "item_strawberry_wine", 1, 3.0f, 80, 24f, 0, 0);

        // 수박 와인: 수박 2개 → 1,130G, 24시간
        MakeRecipe(root, "SO_Recipe_Ferm_WatermelonWine",
            "recipe_ferm_watermelon_wine", "수박 와인", ProcessingType.Fermentation,
            CropCategory.Fruit, "watermelon", 2,
            "item_watermelon_wine", 1, 3.0f, 80, 24f, 0, 0);

        // 토마토 식초: 토마토 3개 → 260G, 12시간
        MakeRecipe(root, "SO_Recipe_Ferm_TomatoVinegar",
            "recipe_ferm_tomato_vinegar", "토마토 식초", ProcessingType.Fermentation,
            CropCategory.Fruit, "tomato", 3,
            "item_tomato_vinegar", 1, 3.0f, 80, 12f, 0, 0);

        // 호박 장아찌: 호박 1개 → 680G, 18시간
        MakeRecipe(root, "SO_Recipe_Ferm_PumpkinPickle",
            "recipe_ferm_pumpkin_pickle", "호박 장아찌", ProcessingType.Fermentation,
            CropCategory.Vegetable, "pumpkin", 1,
            "item_pumpkin_pickle", 1, 3.0f, 80, 18f, 0, 0);

        // 시금치 겉절이: 시금치 2개 → 470G, 12시간
        MakeRecipe(root, "SO_Recipe_Ferm_SpinachKimchi",
            "recipe_ferm_spinach_kimchi", "시금치 겉절이", ProcessingType.Fermentation,
            CropCategory.Vegetable, "spinach", 2,
            "item_spinach_kimchi", 1, 3.0f, 80, 12f, 0, 0);
    }

    // ── 베이커리 레시피 (5종) ────────────────────────────────────
    // -> copied from docs/content/processing-system.md 섹션 3.4.1
    // 베이커리는 복수 재료 가능. 현재 스키마는 주재료 1종 지원
    // 부재료는 description에 명시 (schema 확장 전 임시 처리)

    private static void CreateBakeryRecipes()
    {
        const string root = "Assets/_Project/Resources/Data/Recipes/Bakery";

        // 옥수수 빵: 옥수수 가루 x1 → 350G, 3시간, 장작 1
        MakeRecipe(root, "SO_Recipe_Bake_CornBread",
            "recipe_bake_corn_bread", "옥수수 빵", ProcessingType.Bake,
            CropCategory.Vegetable, "item_corn_flour", 1,
            "item_corn_bread", 1, 0f, 350, 3f, 1, 0);

        // 호박 파이: 호박 분말 x1 + 딸기 잼 x1 → 1,200G, 4시간, 장작 1
        // 부재료: item_jam_strawberry x1 (복수 재료 스키마 미지원, description에 명시)
        MakeRecipe(root, "SO_Recipe_Bake_PumpkinPie",
            "recipe_bake_pumpkin_pie", "호박 파이", ProcessingType.Bake,
            CropCategory.Vegetable, "item_pumpkin_powder", 1,
            "item_pumpkin_pie", 1, 0f, 1200, 4f, 1, 0);

        // 딸기 케이크: 옥수수 가루 x1 + 딸기 x3 → 680G, 5시간, 장작 2
        MakeRecipe(root, "SO_Recipe_Bake_StrawberryCake",
            "recipe_bake_strawberry_cake", "딸기 케이크", ProcessingType.Bake,
            CropCategory.Fruit, "item_corn_flour", 1,
            "item_strawberry_cake", 1, 0f, 680, 5f, 2, 0);

        // 채소 쿠키: 감자 전분 x1 + 당근 x2 → 120G x3개, 3시간, 장작 1
        MakeRecipe(root, "SO_Recipe_Bake_VeggieCookie",
            "recipe_bake_veggie_cookie", "채소 쿠키", ProcessingType.Bake,
            CropCategory.Vegetable, "item_potato_starch", 1,
            "item_veggie_cookie", 3, 0f, 120, 3f, 1, 0);

        // 로열 타르트: 호박 분말 x1 + 수박 잼 x1 + 딸기 x2 → 2,100G, 6시간, 장작 2
        MakeRecipe(root, "SO_Recipe_Bake_RoyalTart",
            "recipe_bake_royal_tart", "로열 타르트", ProcessingType.Bake,
            CropCategory.Vegetable, "item_pumpkin_powder", 1,
            "item_royal_tart", 1, 0f, 2100, 6f, 2, 0);
    }

    // ── 공용 팩토리 ─────────────────────────────────────────────

    private static void MakeRecipe(
        string folder, string assetName,
        string dataId, string displayName,
        ProcessingType processingType,
        CropCategory inputCategory,
        string inputItemId, int inputQuantity,
        string outputItemId, int outputQuantity,
        float priceMultiplier, int priceBonus,
        float processingTimeHours, int fuelCost,
        int requiredFacilityTier)
    {
        string path = $"{folder}/{assetName}.asset";
        if (AssetDatabase.LoadAssetAtPath<ProcessingRecipeData>(path) != null)
        {
            Debug.Log($"[CreateAllRecipeSOs] 이미 존재, 스킵: {assetName}");
            return;
        }
        var so = ScriptableObject.CreateInstance<ProcessingRecipeData>();
        so.dataId               = dataId;
        so.displayName          = displayName;
        so.processingType       = processingType;
        so.inputCategory        = inputCategory;
        so.inputItemId          = inputItemId;
        so.inputQuantity        = inputQuantity;
        so.outputItemId         = outputItemId;
        so.outputQuantity       = outputQuantity;
        so.priceMultiplier      = priceMultiplier;
        so.priceBonus           = priceBonus;
        so.processingTimeHours  = processingTimeHours;
        so.fuelCost             = fuelCost;
        so.requiredFacilityTier = requiredFacilityTier;
        AssetDatabase.CreateAsset(so, path);
        Debug.Log($"[CreateAllRecipeSOs] 생성: {path}");
    }
}
#endif
