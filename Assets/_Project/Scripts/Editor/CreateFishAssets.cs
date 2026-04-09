// Editor 스크립트: 어종 15종 FishData SO 일괄 생성
// -> see docs/systems/fishing-system.md 섹션 4.2 for canonical 값
// -> see docs/mcp/fishing-tasks.md F-2
using UnityEngine;
using UnityEditor;
using SeedMind.Fishing;
using SeedMind.Fishing.Data;
using SeedMind.Farm.Data;

public class CreateFishAssets : Editor
{
    [MenuItem("SeedMind/Fishing/Create Fish Assets")]
    public static void CreateAll()
    {
        // 출력 폴더 확인
        const string outputPath = "Assets/_Project/Data/Fish";
        if (!AssetDatabase.IsValidFolder(outputPath))
            AssetDatabase.CreateFolder("Assets/_Project/Data", "Fish");

        // 어종 데이터 정의
        // 수치 canonical: docs/systems/fishing-system.md 섹션 4.2, 3.3, 5.1, 5.2
        var fishList = new (
            string dataId, string displayName, string description,
            FishRarity rarity, int basePrice, SeasonFlag seasons,
            float[] timeWeights, // [Dawn, Morning, Afternoon, Evening, Night]
            WeatherFlag weather,
            float difficulty, float zoneWidthMul, float moveSpeed,
            int maxStack, int expReward
        )[]
        {
            // Common 어종
            ("fish_crucian_carp", "붕어", "연못에서 가장 흔히 잡히는 민물고기. 사계절 잡을 수 있다.",
                FishRarity.Common, 20,
                SeasonFlag.Spring | SeasonFlag.Summer | SeasonFlag.Autumn,
                new[]{0.2f,0.2f,0.2f,0.2f,0.2f},   // 전 시간대
                WeatherFlag.All,
                0.2f, 1.2f, 1.0f, 99, 5),

            ("fish_loach", "미꾸라지", "진흙 속에 숨어 사는 미꾸라지. 봄과 여름에 주로 활동한다.",
                FishRarity.Common, 25,
                SeasonFlag.Spring | SeasonFlag.Summer,
                new[]{0.05f,0.35f,0.35f,0.20f,0.05f},  // Morning~Evening
                WeatherFlag.All,
                0.2f, 1.2f, 1.0f, 99, 5),

            ("fish_carp", "잉어", "연못의 터줏대감. 봄부터 가을까지 꾸준히 잡힌다.",
                FishRarity.Common, 30,
                SeasonFlag.Spring | SeasonFlag.Summer | SeasonFlag.Autumn,
                new[]{0.2f,0.2f,0.2f,0.2f,0.2f},
                WeatherFlag.All,
                0.2f, 1.2f, 1.0f, 99, 5),

            ("fish_bluegill", "블루길", "여름과 가을에 활발히 움직이는 작은 물고기.",
                FishRarity.Common, 22,
                SeasonFlag.Summer | SeasonFlag.Autumn,
                new[]{0.05f,0.40f,0.40f,0.10f,0.05f},  // Morning~Afternoon
                WeatherFlag.All,
                0.2f, 1.2f, 1.0f, 99, 5),

            ("fish_smelt", "빙어", "겨울 새벽 맑은 날에 잡을 수 있는 투명한 물고기.",
                FishRarity.Common, 18,
                SeasonFlag.Winter,
                new[]{0.50f,0.35f,0.10f,0.03f,0.02f},  // Dawn~Morning
                WeatherFlag.Clear | WeatherFlag.Snow,
                0.2f, 1.2f, 1.0f, 99, 5),

            // Uncommon 어종
            ("fish_catfish", "메기", "비 오는 저녁 깊은 수심에서 잡히는 덩치 큰 물고기.",
                FishRarity.Uncommon, 55,
                SeasonFlag.Summer | SeasonFlag.Autumn,
                new[]{0.03f,0.05f,0.12f,0.40f,0.40f},  // Evening~Night
                WeatherFlag.Rain | WeatherFlag.HeavyRain,
                0.4f, 1.0f, 1.5f, 99, 10),

            ("fish_trout", "송어", "맑고 차가운 물을 좋아하는 봄가을 새벽 물고기.",
                FishRarity.Uncommon, 50,
                SeasonFlag.Spring | SeasonFlag.Autumn,
                new[]{0.45f,0.40f,0.08f,0.04f,0.03f},  // Dawn~Morning
                WeatherFlag.Clear | WeatherFlag.Cloudy,
                0.4f, 1.0f, 1.5f, 99, 10),

            ("fish_crawfish", "가재", "여름 오후 돌 틈에 숨어있는 민물 가재.",
                FishRarity.Uncommon, 45,
                SeasonFlag.Summer,
                new[]{0.05f,0.10f,0.40f,0.40f,0.05f},  // Afternoon~Evening
                WeatherFlag.All,
                0.4f, 1.0f, 1.5f, 99, 10),

            ("fish_eel", "뱀장어", "가을 폭풍우 밤에만 나타나는 신비로운 장어.",
                FishRarity.Uncommon, 65,
                SeasonFlag.Autumn,
                new[]{0.02f,0.03f,0.05f,0.15f,0.75f},  // Night
                WeatherFlag.Rain | WeatherFlag.HeavyRain | WeatherFlag.Storm,
                0.4f, 1.0f, 1.5f, 99, 10),

            // Rare 어종
            ("fish_cherry_salmon", "산천어", "봄 새벽 맑은 날 희귀하게 잡히는 붉은 연어.",
                FishRarity.Rare, 120,
                SeasonFlag.Spring,
                new[]{0.75f,0.15f,0.05f,0.03f,0.02f},  // Dawn
                WeatherFlag.Clear,
                0.7f, 0.75f, 2.2f, 99, 20),

            ("fish_golden_carp", "황금 잉어", "여름 오전 맑은 날 반짝이며 헤엄치는 황금빛 잉어.",
                FishRarity.Rare, 200,
                SeasonFlag.Summer,
                new[]{0.05f,0.40f,0.40f,0.10f,0.05f},  // Morning~Afternoon
                WeatherFlag.Clear,
                0.7f, 0.75f, 2.2f, 99, 20),

            ("fish_rainbow_trout", "무지개송어", "가을 흐린 새벽에 무지개처럼 빛나는 송어.",
                FishRarity.Rare, 150,
                SeasonFlag.Autumn,
                new[]{0.45f,0.35f,0.10f,0.05f,0.05f},  // Dawn~Morning
                WeatherFlag.Cloudy | WeatherFlag.Rain,
                0.7f, 0.75f, 2.2f, 99, 20),

            ("fish_ice_king_smelt", "얼음 빙어왕", "겨울 새벽 눈 오는 날에만 나타나는 전설의 빙어.",
                FishRarity.Rare, 180,
                SeasonFlag.Winter,
                new[]{0.70f,0.20f,0.05f,0.03f,0.02f},  // Dawn
                WeatherFlag.Snow,
                0.7f, 0.75f, 2.2f, 99, 20),

            // Legendary 어종
            ("fish_legend_catfish", "전설의 메기왕", "가을 폭풍 깊은 밤, 극히 드물게 나타나는 거대한 메기.",
                FishRarity.Legendary, 500,
                SeasonFlag.Autumn,
                new[]{0.01f,0.02f,0.02f,0.10f,0.85f},  // Night
                WeatherFlag.Storm,
                0.9f, 0.5f, 3.0f, 99, 50),

            ("fish_lotus_koi", "연꽃 잉어", "여름 새벽 맑은 날 연꽃 사이를 유유히 헤엄치는 신비의 잉어.",
                FishRarity.Legendary, 800,
                SeasonFlag.Summer,
                new[]{0.80f,0.12f,0.04f,0.02f,0.02f},  // Dawn
                WeatherFlag.Clear,
                0.9f, 0.5f, 3.0f, 99, 50),
        };

        int created = 0;
        int updated = 0;

        foreach (var f in fishList)
        {
            string assetPath = $"{outputPath}/SO_Fish_{PascalCase(f.dataId)}.asset";

            FishData so = AssetDatabase.LoadAssetAtPath<FishData>(assetPath);
            bool isNew = so == null;
            if (isNew)
            {
                so = ScriptableObject.CreateInstance<FishData>();
                AssetDatabase.CreateAsset(so, assetPath);
                created++;
            }
            else
            {
                updated++;
            }

            so.dataId               = f.dataId;
            so.fishId               = f.dataId;
            so.displayName          = f.displayName;
            so.rarity               = f.rarity;
            so.basePrice            = f.basePrice;
            so.seasonAvailability   = f.seasons;
            so.timeWeights          = f.timeWeights;
            so.weatherBonus         = f.weather;
            so.minigameDifficulty   = f.difficulty;
            so.targetZoneWidthMul   = f.zoneWidthMul;
            so.moveSpeed            = f.moveSpeed;
            so.maxStackSize         = f.maxStack;
            so.expReward            = f.expReward;

            EditorUtility.SetDirty(so);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[CreateFishAssets] 완료: {created}개 생성, {updated}개 업데이트 (총 {fishList.Length}종)");
    }

    // "fish_crucian_carp" → "Fish_CrucianCarp"
    private static string PascalCase(string snakeId)
    {
        var parts = snakeId.Split('_');
        var sb = new System.Text.StringBuilder();
        foreach (var p in parts)
        {
            if (p.Length == 0) continue;
            sb.Append(char.ToUpper(p[0]));
            sb.Append(p.Substring(1));
            sb.Append('_');
        }
        if (sb.Length > 0) sb.Length--; // 마지막 '_' 제거
        return sb.ToString();
    }
}
