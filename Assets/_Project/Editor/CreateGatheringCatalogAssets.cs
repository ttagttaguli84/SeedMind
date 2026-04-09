using UnityEngine;
using UnityEditor;
using SeedMind.Collection;
using SeedMind.Gathering;

namespace SeedMind.Editor
{
    /// <summary>
    /// GatheringCatalogData SO 27종 일괄 생성 및 GatheringCatalogManager 배열 자동 연결.
    /// -> see docs/content/gathering-items.md (아이템 목록)
    /// -> see docs/systems/collection-system.md 섹션 3.3 (희귀도별 보상)
    /// </summary>
    public static class CreateGatheringCatalogAssets
    {
        private const string FolderPath = "Assets/_Project/Data/GatheringCatalog";

        private struct ItemDef
        {
            public string itemId;
            public string displayName;
            public GatheringRarity rarity;
            public int sortOrder;
            public string hintLocked;
            public string descriptionUnlocked;
        }

        // 희귀도별 보상 (-> see docs/systems/collection-system.md 섹션 3.3)
        private static int GetGold(GatheringRarity r) => r switch
        {
            GatheringRarity.Common    => 5,
            GatheringRarity.Uncommon  => 15,
            GatheringRarity.Rare      => 50,
            GatheringRarity.Legendary => 200,
            _                         => 5
        };

        private static int GetXP(GatheringRarity r) => r switch
        {
            GatheringRarity.Common    => 2,
            GatheringRarity.Uncommon  => 5,
            GatheringRarity.Rare      => 15,
            GatheringRarity.Legendary => 50,
            _                         => 2
        };

        // 아이템 목록 (-> see docs/content/gathering-items.md)
        private static readonly ItemDef[] Items = new ItemDef[]
        {
            // 봄 꽃/나물
            new ItemDef { itemId="gather_dandelion",       displayName="민들레",     rarity=GatheringRarity.Common,    sortOrder=1,
                hintLocked="따뜻한 봄바람에 노란 꽃잎을 흔드는 작은 꽃. 숲 어디서나 만날 수 있다.",
                descriptionUnlocked="봄이면 숲 어디서나 볼 수 있는 노란 민들레. 꽃잎, 잎, 뿌리 모두 활용할 수 있는 유용한 식물이다." },
            new ItemDef { itemId="gather_wild_garlic",     displayName="달래",       rarity=GatheringRarity.Common,    sortOrder=2,
                hintLocked="봄 숲의 은은한 향기를 따라가면 만나는 작은 풀. 알싸한 맛이 요리에 깊이를 더한다.",
                descriptionUnlocked="봄 숲 가장자리에서 자라는 야생 달래. 독특한 향과 알싸한 맛이 각종 요리에 깊이를 더해준다." },
            new ItemDef { itemId="gather_spring_herb",     displayName="봄나물",     rarity=GatheringRarity.Uncommon,  sortOrder=3,
                hintLocked="이슬 맺힌 숲 가장자리에서 자라는 부드러운 나물. 봄의 향긋함이 가득하다.",
                descriptionUnlocked="봄 숲 가장자리에서 자라는 향긋한 나물. 살짝 데쳐 무치면 봄의 맛을 그대로 느낄 수 있다." },
            new ItemDef { itemId="gather_violet",          displayName="제비꽃",     rarity=GatheringRarity.Uncommon,  sortOrder=4,
                hintLocked="덤불 사이에 숨듯 피어나는 보라빛 작은 꽃. 선물하면 누구나 미소 짓는다.",
                descriptionUnlocked="봄 덤불 사이에 소담하게 피어나는 보라빛 꽃. 향기가 은은하고 색이 아름다워 선물용으로도 인기다." },
            new ItemDef { itemId="gather_spring_mushroom", displayName="송이 (봄)",  rarity=GatheringRarity.Rare,      sortOrder=5,
                hintLocked="비 내린 다음 날 숲에서 발견되는 귀한 버섯. 진한 향이 코끝을 간질인다.",
                descriptionUnlocked="봄비 내린 다음 날 숲에서 드물게 발견되는 귀한 버섯. 진한 향과 쫄깃한 식감이 특징이다." },
            new ItemDef { itemId="gather_wild_ginseng",   displayName="산삼",       rarity=GatheringRarity.Legendary, sortOrder=6,
                hintLocked="봄비 내린 깊은 숲속, 오랜 세월을 견딘 신비로운 뿌리. 숙련된 채집가만이 발견할 수 있다.",
                descriptionUnlocked="깊은 숲속에서 수십 년을 자란 산삼. 극히 드물게 발견되며, 그 약효는 전설적이다." },
            // 여름 열매/나물
            new ItemDef { itemId="gather_wild_berry",     displayName="산딸기",     rarity=GatheringRarity.Common,    sortOrder=7,
                hintLocked="여름 덤불에 주렁주렁 매달린 빨간 열매. 한 알 베어 물면 새콤달콤한 맛이 퍼진다.",
                descriptionUnlocked="여름 덤불에 주렁주렁 열리는 산딸기. 새콤달콤한 맛이 일품이며 잼이나 주스로 만들면 더욱 맛있다." },
            new ItemDef { itemId="gather_mugwort",        displayName="쑥",         rarity=GatheringRarity.Common,    sortOrder=8,
                hintLocked="숲과 초원 어디서든 만날 수 있는 질긴 약초. 독특한 향이 몸과 마음을 달래준다.",
                descriptionUnlocked="들판 어디서나 자라는 강인한 약초. 쑥국, 쑥떡 등 다양한 요리에 쓰이며 건강에도 좋다." },
            new ItemDef { itemId="gather_akebia_fruit",   displayName="으름 열매",  rarity=GatheringRarity.Uncommon,  sortOrder=9,
                hintLocked="여름 덤불 깊은 곳에 매달린 보라빛 열매. 껍질을 까면 달콤한 과육이 나온다.",
                descriptionUnlocked="여름 숲에서 자라는 으름덩굴의 열매. 껍질을 까면 달콤하고 부드러운 흰 과육이 나온다." },
            new ItemDef { itemId="gather_lotus_leaf",     displayName="연잎",       rarity=GatheringRarity.Uncommon,  sortOrder=10,
                hintLocked="연못 위에 둥글게 펼쳐진 초록 잎. 이슬을 담은 모양이 아름답다.",
                descriptionUnlocked="연못 위에 둥글게 펼쳐지는 초록 잎. 연잎밥이나 차 재료로 활용하며 독특한 향이 난다." },
            new ItemDef { itemId="gather_reishi",         displayName="영지버섯",   rarity=GatheringRarity.Rare,      sortOrder=11,
                hintLocked="비 온 다음 날 숲 깊은 곳에서 자라는 단단한 버섯. 윤기 나는 갈색 갓이 특징이다.",
                descriptionUnlocked="숲의 오래된 나무 밑동에서 자라는 귀한 버섯. 윤기 나는 갈색 갓이 특징이며 약재로 가치가 높다." },
            new ItemDef { itemId="gather_golden_lotus",   displayName="황금 연꽃",  rarity=GatheringRarity.Legendary, sortOrder=12,
                hintLocked="맑은 여름 새벽, 연못에서 금빛으로 빛나는 신비로운 꽃. 숙련된 눈만이 발견할 수 있다.",
                descriptionUnlocked="여름 새벽 연못에서 금빛으로 빛나는 전설의 꽃. 극히 드물게 피어나며 보는 것만으로도 행운이 찾아온다 한다." },
            // 가을 버섯/열매
            new ItemDef { itemId="gather_acorn",          displayName="도토리",     rarity=GatheringRarity.Common,    sortOrder=13,
                hintLocked="가을이 되면 숲 바닥에 수북이 쌓이는 작은 열매. 묵으로 만들면 쫄깃한 별미가 된다.",
                descriptionUnlocked="가을 숲에서 흔히 볼 수 있는 참나무 열매. 묵으로 만들면 쫄깃한 별미가 되며 다람쥐들의 겨울 식량이기도 하다." },
            new ItemDef { itemId="gather_neungi",         displayName="능이버섯",   rarity=GatheringRarity.Common,    sortOrder=14,
                hintLocked="가을 숲의 낙엽 사이에서 고개를 내미는 향긋한 버섯. 국물 재료로 최고다.",
                descriptionUnlocked="가을 숲 낙엽 사이에서 자라는 향기로운 버섯. 진한 국물 맛으로 요리에 깊이를 더하며 '버섯의 왕'으로 불린다." },
            new ItemDef { itemId="gather_wild_shiitake",  displayName="표고버섯 (야생)", rarity=GatheringRarity.Uncommon, sortOrder=15,
                hintLocked="숲속 고목에 붙어 자라는 야생 표고. 재배산보다 진한 향이 난다.",
                descriptionUnlocked="숲속 고목에서 자연적으로 자라는 야생 표고버섯. 재배산에 비해 향이 훨씬 진하고 맛이 깊다." },
            new ItemDef { itemId="gather_wild_grape",     displayName="머루",       rarity=GatheringRarity.Uncommon,  sortOrder=16,
                hintLocked="가을 덤불에 매달린 검푸른 작은 포도. 발효시키면 깊은 맛의 와인이 된다.",
                descriptionUnlocked="가을 덤불에 포도처럼 매달리는 야생 열매. 발효시키면 깊은 맛의 머루주가 되어 특별한 선물로 인기다." },
            new ItemDef { itemId="gather_pine_mushroom",  displayName="송이버섯",   rarity=GatheringRarity.Rare,      sortOrder=17,
                hintLocked="소나무 뿌리 아래 숨어 자라는 가을의 보물. 그윽한 향이 숲 전체에 퍼진다.",
                descriptionUnlocked="소나무 숲에서만 자라는 귀한 가을 버섯. 그윽한 솔향기가 배어있어 최고급 식재료로 손꼽힌다." },
            new ItemDef { itemId="gather_millennium_reishi", displayName="천년 영지", rarity=GatheringRarity.Legendary, sortOrder=18,
                hintLocked="흐린 가을 날, 숲의 가장 깊은 곳에서 발견되는 전설의 버섯. 천 년의 기운이 서려 있다.",
                descriptionUnlocked="천 년을 자란 전설의 영지버섯. 극히 드물게 발견되며 그 신비로운 효능은 숙련된 채집가들 사이에서만 전해진다." },
            // 겨울 나물/이끼
            new ItemDef { itemId="gather_winter_bark",    displayName="겨울 나무껍질", rarity=GatheringRarity.Common,  sortOrder=19,
                hintLocked="겨울 숲에서 벗겨진 마른 나무껍질. 차로 우리면 은은한 나무 향이 난다.",
                descriptionUnlocked="겨울 숲에서 자연적으로 떨어진 나무껍질. 차로 우려내면 은은한 향이 나며 민간요법에도 활용된다." },
            new ItemDef { itemId="gather_snow_moss",      displayName="눈꽃 이끼",  rarity=GatheringRarity.Uncommon,  sortOrder=20,
                hintLocked="눈 덮인 바위 틈에 피어나는 하얀 이끼. 눈이 내리는 날에만 모습을 보인다.",
                descriptionUnlocked="눈 오는 날 바위 틈에서만 볼 수 있는 희귀한 하얀 이끼. 보는 순간 행운이 깃든다는 전설이 있다." },
            new ItemDef { itemId="gather_cordyceps",      displayName="동충하초",   rarity=GatheringRarity.Rare,      sortOrder=21,
                hintLocked="겨울 눈 속에 숨은 희귀한 약재. 눈이 내린 날, 숙련된 채집가만이 찾아낼 수 있다.",
                descriptionUnlocked="겨울 눈 속에 숨어있는 귀한 약재. 특수한 환경에서만 자라며 그 약효는 예로부터 귀하게 여겨졌다." },
            // 광물
            new ItemDef { itemId="gather_stone_chip",     displayName="돌 조각",    rarity=GatheringRarity.Common,    sortOrder=22,
                hintLocked="동굴 입구에 흔하게 굴러다니는 작은 돌 조각. 쌓아두면 건축 재료로 쓸 수 있다.",
                descriptionUnlocked="동굴 주변에서 쉽게 찾을 수 있는 돌 조각. 건축 재료로 활용하거나 장식용으로도 쓸 수 있다." },
            new ItemDef { itemId="gather_copper_ore",     displayName="구리 광석",  rarity=GatheringRarity.Uncommon,  sortOrder=23,
                hintLocked="동굴 벽면에 붉은 빛을 띠는 광맥. 대장장이가 이 광석을 좋아한다고 한다.",
                descriptionUnlocked="동굴 벽면에서 발견되는 붉은빛 광석. 제련하면 구리가 되며 대장장이에게 가져가면 좋은 대가를 받을 수 있다." },
            new ItemDef { itemId="gather_iron_ore",       displayName="철 광석",    rarity=GatheringRarity.Uncommon,  sortOrder=24,
                hintLocked="무겁고 단단한 회색 광석. 녹여서 다듬으면 좋은 도구의 재료가 될 수 있다.",
                descriptionUnlocked="동굴 깊숙한 곳에서 발견되는 단단한 회색 광석. 제련하면 강철이 되어 도구 제작에 필수적인 재료다." },
            new ItemDef { itemId="gather_gold_ore",       displayName="금 광석",    rarity=GatheringRarity.Rare,      sortOrder=25,
                hintLocked="동굴 깊은 곳에서 금빛으로 반짝이는 귀한 광석. 전설적인 도구를 만드는 데 꼭 필요하다.",
                descriptionUnlocked="동굴 깊은 곳에서만 발견되는 귀한 광석. 전설급 도구 제작에 사용되며 상인들도 높은 가격을 치른다." },
            new ItemDef { itemId="gather_crystal",        displayName="수정 원석",  rarity=GatheringRarity.Rare,      sortOrder=26,
                hintLocked="투명하게 빛나는 원석. 빛을 받으면 무지갯빛 광채를 뿜는다.",
                descriptionUnlocked="동굴 깊은 곳에서 발견되는 투명한 원석. 빛을 받으면 아름다운 무지갯빛을 뿜어내어 장식품으로도 인기가 높다." },
            new ItemDef { itemId="gather_amethyst",       displayName="자수정",     rarity=GatheringRarity.Legendary, sortOrder=27,
                hintLocked="깊은 보라빛의 아름다운 보석. 선물하면 누구든 감동할 것이다. 동굴에서도 쉽게 만나기 어렵다.",
                descriptionUnlocked="동굴 최심부에서 극히 드물게 발견되는 보라빛 보석. 그 아름다움에 감동하지 않는 이가 없을 정도로 귀하다." },
        };

        [MenuItem("SeedMind/Create Gathering Catalog Assets")]
        public static void Create()
        {
            // 폴더 생성
            if (!AssetDatabase.IsValidFolder(FolderPath))
            {
                AssetDatabase.CreateFolder("Assets/_Project/Data", "GatheringCatalog");
            }

            var createdAssets = new System.Collections.Generic.List<GatheringCatalogData>();

            foreach (var item in Items)
            {
                string assetPath = $"{FolderPath}/GatheringCatalog_{item.itemId.Replace("gather_", "")}.asset";

                // 중복 생성 방지
                var existing = AssetDatabase.LoadAssetAtPath<GatheringCatalogData>(assetPath);
                if (existing != null)
                {
                    Debug.Log($"[CreateGatheringCatalogAssets] Already exists: {assetPath}");
                    createdAssets.Add(existing);
                    continue;
                }

                var so = ScriptableObject.CreateInstance<GatheringCatalogData>();
                so.itemId = item.itemId;
                so.hintLocked = item.hintLocked;
                so.descriptionUnlocked = item.descriptionUnlocked;
                so.rarity = item.rarity;
                so.firstDiscoverGold = GetGold(item.rarity);
                so.firstDiscoverXP = GetXP(item.rarity);
                so.sortOrder = item.sortOrder;

                AssetDatabase.CreateAsset(so, assetPath);
                createdAssets.Add(so);
                Debug.Log($"[CreateGatheringCatalogAssets] Created: {assetPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // GatheringCatalogManager에 배열 자동 연결
            ConnectManagerRefs(createdAssets.ToArray());

            Debug.Log($"[CreateGatheringCatalogAssets] Done! {createdAssets.Count} assets created.");
        }

        private static void ConnectManagerRefs(GatheringCatalogData[] assets)
        {
            var manager = Object.FindObjectOfType<GatheringCatalogManager>();
            if (manager == null)
            {
                Debug.LogWarning("[CreateGatheringCatalogAssets] GatheringCatalogManager not found in scene. Run Q-D first, then re-run this.");
                return;
            }

            var so = new SerializedObject(manager);
            var prop = so.FindProperty("_catalogDataRegistry");
            if (prop == null)
            {
                Debug.LogError("[CreateGatheringCatalogAssets] _catalogDataRegistry field not found.");
                return;
            }

            prop.arraySize = assets.Length;
            for (int i = 0; i < assets.Length; i++)
            {
                prop.GetArrayElementAtIndex(i).objectReferenceValue = assets[i];
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);

            Debug.Log($"[CreateGatheringCatalogAssets] Connected {assets.Length} assets to GatheringCatalogManager.");
        }
    }
}
