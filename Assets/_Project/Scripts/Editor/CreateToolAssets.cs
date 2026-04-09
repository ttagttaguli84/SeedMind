// CreateToolAssets — ToolData SO 9종 + 재료 SO 2종 일괄 생성 에디터 스크립트
// -> see docs/mcp/tool-upgrade-tasks.md T-1
// 수치 canonical 출처: -> see docs/systems/tool-upgrade.md 섹션 2.1, 3.1~3.3
using UnityEditor;
using UnityEngine;
using SeedMind.Player.Data;
using SeedMind.Player;

namespace SeedMind.Editor
{
    public static class CreateToolAssets
    {
        [MenuItem("SeedMind/Create/Tool Assets")]
        public static void CreateAll()
        {
            EnsureFolders();
            CreateToolSOs();
            CreateMaterialSOs();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateToolAssets] ToolData SO 9종 + 재료 SO 2종 생성 완료.");
        }

        private static void EnsureFolders()
        {
            string[] folders =
            {
                "Assets/_Project/Resources",
                "Assets/_Project/Resources/Data",
                "Assets/_Project/Resources/Data/Tools",
                "Assets/_Project/Resources/Data/Materials",
            };
            foreach (var f in folders)
            {
                if (!AssetDatabase.IsValidFolder(f))
                {
                    var parts = f.Split('/');
                    string parent = string.Join("/", parts, 0, parts.Length - 1);
                    AssetDatabase.CreateFolder(parent, parts[parts.Length - 1]);
                }
            }
        }

        private static void CreateToolSOs()
        {
            // ── 호미 (Hoe) ─────────────────────────────────────────────
            // -> see docs/systems/tool-upgrade.md 섹션 3.1
            var hoeBasic = CreateOrLoad<ToolData>("Resources/Data/Tools/SO_Tool_Hoe_Basic.asset");
            hoeBasic.dataId = "hoe_basic";
            hoeBasic.displayName = "호미";
            hoeBasic.toolType = ToolType.Hoe;
            hoeBasic.tier = 1;
            hoeBasic.range = 1;
            hoeBasic.energyCost = 2;
            hoeBasic.cooldown = 0.6f;
            hoeBasic.useSpeed = 1.0f;
            hoeBasic.upgradeGoldCost = 800;     // -> see docs/systems/tool-upgrade.md 섹션 2.1
            hoeBasic.upgradeTimeDays = 1;
            hoeBasic.requiredLevel = 3;
            hoeBasic.levelReqType = LevelReqType.PlayerLevel;
            hoeBasic.upgradeMaterials = new[] { new UpgradeMaterial { materialId = "iron_scrap", quantity = 3 } };
            hoeBasic.specialEffect = ToolSpecialEffect.None;
            hoeBasic.description = "기본 호미. 땅을 갈아 경작지를 만든다.";
            EditorUtility.SetDirty(hoeBasic);

            var hoeReinforced = CreateOrLoad<ToolData>("Resources/Data/Tools/SO_Tool_Hoe_Reinforced.asset");
            hoeReinforced.dataId = "hoe_reinforced";
            hoeReinforced.displayName = "강화 호미";
            hoeReinforced.toolType = ToolType.Hoe;
            hoeReinforced.tier = 2;
            hoeReinforced.range = 3;
            hoeReinforced.energyCost = 3;
            hoeReinforced.cooldown = 0.8f;
            hoeReinforced.useSpeed = 1.0f;
            hoeReinforced.upgradeGoldCost = 3000;
            hoeReinforced.upgradeTimeDays = 2;
            hoeReinforced.requiredLevel = 7;
            hoeReinforced.levelReqType = LevelReqType.PlayerLevel;
            hoeReinforced.upgradeMaterials = new[] { new UpgradeMaterial { materialId = "refined_steel", quantity = 3 } };
            hoeReinforced.specialEffect = ToolSpecialEffect.AreaEffect;
            hoeReinforced.description = "강화된 호미. 1x3 범위로 경작하며 돌을 자동 제거한다.";
            EditorUtility.SetDirty(hoeReinforced);

            var hoeLegendary = CreateOrLoad<ToolData>("Resources/Data/Tools/SO_Tool_Hoe_Legendary.asset");
            hoeLegendary.dataId = "hoe_legendary";
            hoeLegendary.displayName = "전설 호미";
            hoeLegendary.toolType = ToolType.Hoe;
            hoeLegendary.tier = 3;
            hoeLegendary.range = 9;
            hoeLegendary.energyCost = 5;
            hoeLegendary.cooldown = 1.2f;
            hoeLegendary.useSpeed = 1.0f;
            hoeLegendary.upgradeGoldCost = 0;   // 최종 등급
            hoeLegendary.upgradeTimeDays = 0;
            hoeLegendary.requiredLevel = 0;
            hoeLegendary.levelReqType = LevelReqType.PlayerLevel;
            hoeLegendary.upgradeMaterials = null;
            hoeLegendary.specialEffect = ToolSpecialEffect.AreaEffect;
            hoeLegendary.description = "전설의 호미. 3x3 범위로 경작하며 돌과 잡초를 자동 제거한다.";
            EditorUtility.SetDirty(hoeLegendary);

            // nextTier 체인 연결
            hoeBasic.nextTier = hoeReinforced;
            hoeReinforced.nextTier = hoeLegendary;
            hoeLegendary.nextTier = null;
            EditorUtility.SetDirty(hoeBasic);
            EditorUtility.SetDirty(hoeReinforced);

            // ── 물뿌리개 (WateringCan) ──────────────────────────────────
            // -> see docs/systems/tool-upgrade.md 섹션 3.2
            var wcBasic = CreateOrLoad<ToolData>("Resources/Data/Tools/SO_Tool_WateringCan_Basic.asset");
            wcBasic.dataId = "wateringcan_basic";
            wcBasic.displayName = "물뿌리개";
            wcBasic.toolType = ToolType.WateringCan;
            wcBasic.tier = 1;
            wcBasic.range = 1;
            wcBasic.energyCost = 1;
            wcBasic.cooldown = 0.5f;
            wcBasic.useSpeed = 1.0f;
            wcBasic.upgradeGoldCost = 800;
            wcBasic.upgradeTimeDays = 1;
            wcBasic.requiredLevel = 3;
            wcBasic.levelReqType = LevelReqType.PlayerLevel;
            wcBasic.upgradeMaterials = new[] { new UpgradeMaterial { materialId = "iron_scrap", quantity = 3 } };
            wcBasic.specialEffect = ToolSpecialEffect.None;
            wcBasic.description = "기본 물뿌리개. 작물에 물을 준다.";
            EditorUtility.SetDirty(wcBasic);

            var wcReinforced = CreateOrLoad<ToolData>("Resources/Data/Tools/SO_Tool_WateringCan_Reinforced.asset");
            wcReinforced.dataId = "wateringcan_reinforced";
            wcReinforced.displayName = "강화 물뿌리개";
            wcReinforced.toolType = ToolType.WateringCan;
            wcReinforced.tier = 2;
            wcReinforced.range = 3;
            wcReinforced.energyCost = 2;
            wcReinforced.cooldown = 0.6f;
            wcReinforced.useSpeed = 1.0f;
            wcReinforced.upgradeGoldCost = 3000;
            wcReinforced.upgradeTimeDays = 2;
            wcReinforced.requiredLevel = 7;
            wcReinforced.levelReqType = LevelReqType.PlayerLevel;
            wcReinforced.upgradeMaterials = new[] { new UpgradeMaterial { materialId = "refined_steel", quantity = 3 } };
            wcReinforced.specialEffect = ToolSpecialEffect.None;
            wcReinforced.description = "강화된 물뿌리개. 1x3 범위에 물을 뿌린다.";
            EditorUtility.SetDirty(wcReinforced);

            var wcLegendary = CreateOrLoad<ToolData>("Resources/Data/Tools/SO_Tool_WateringCan_Legendary.asset");
            wcLegendary.dataId = "wateringcan_legendary";
            wcLegendary.displayName = "전설 물뿌리개";
            wcLegendary.toolType = ToolType.WateringCan;
            wcLegendary.tier = 3;
            wcLegendary.range = 9;
            wcLegendary.energyCost = 3;
            wcLegendary.cooldown = 0.9f;
            wcLegendary.useSpeed = 1.0f;
            wcLegendary.upgradeGoldCost = 0;
            wcLegendary.upgradeTimeDays = 0;
            wcLegendary.requiredLevel = 0;
            wcLegendary.levelReqType = LevelReqType.PlayerLevel;
            wcLegendary.upgradeMaterials = null;
            wcLegendary.specialEffect = ToolSpecialEffect.QualityBoost;
            wcLegendary.description = "전설의 물뿌리개. 3x3 범위에 물을 뿌리고 성장 속도를 높인다.";
            EditorUtility.SetDirty(wcLegendary);

            wcBasic.nextTier = wcReinforced;
            wcReinforced.nextTier = wcLegendary;
            wcLegendary.nextTier = null;
            EditorUtility.SetDirty(wcBasic);
            EditorUtility.SetDirty(wcReinforced);

            // ── 낫 (Sickle) ─────────────────────────────────────────────
            // -> see docs/systems/tool-upgrade.md 섹션 3.3
            var sickleBasic = CreateOrLoad<ToolData>("Resources/Data/Tools/SO_Tool_Sickle_Basic.asset");
            sickleBasic.dataId = "sickle_basic";
            sickleBasic.displayName = "낫";
            sickleBasic.toolType = ToolType.Sickle;
            sickleBasic.tier = 1;
            sickleBasic.range = 1;
            sickleBasic.energyCost = 1;
            sickleBasic.cooldown = 0.5f;
            sickleBasic.useSpeed = 1.0f;
            sickleBasic.upgradeGoldCost = 800;
            sickleBasic.upgradeTimeDays = 1;
            sickleBasic.requiredLevel = 3;
            sickleBasic.levelReqType = LevelReqType.PlayerLevel;
            sickleBasic.upgradeMaterials = new[] { new UpgradeMaterial { materialId = "iron_scrap", quantity = 3 } };
            sickleBasic.specialEffect = ToolSpecialEffect.None;
            sickleBasic.description = "기본 낫. 작물을 수확한다.";
            EditorUtility.SetDirty(sickleBasic);

            var sickleReinforced = CreateOrLoad<ToolData>("Resources/Data/Tools/SO_Tool_Sickle_Reinforced.asset");
            sickleReinforced.dataId = "sickle_reinforced";
            sickleReinforced.displayName = "강화 낫";
            sickleReinforced.toolType = ToolType.Sickle;
            sickleReinforced.tier = 2;
            sickleReinforced.range = 3;
            sickleReinforced.energyCost = 1;
            sickleReinforced.cooldown = 0.6f;
            sickleReinforced.useSpeed = 1.0f;
            sickleReinforced.upgradeGoldCost = 3000;
            sickleReinforced.upgradeTimeDays = 2;
            sickleReinforced.requiredLevel = 7;
            sickleReinforced.levelReqType = LevelReqType.PlayerLevel;
            sickleReinforced.upgradeMaterials = new[] { new UpgradeMaterial { materialId = "refined_steel", quantity = 3 } };
            sickleReinforced.specialEffect = ToolSpecialEffect.DoubleHarvest;
            sickleReinforced.description = "강화된 낫. 1x3 범위로 수확하며 보너스 수확 확률이 있다.";
            EditorUtility.SetDirty(sickleReinforced);

            var sickleLegendary = CreateOrLoad<ToolData>("Resources/Data/Tools/SO_Tool_Sickle_Legendary.asset");
            sickleLegendary.dataId = "sickle_legendary";
            sickleLegendary.displayName = "전설 낫";
            sickleLegendary.toolType = ToolType.Sickle;
            sickleLegendary.tier = 3;
            sickleLegendary.range = 9;
            sickleLegendary.energyCost = 2;
            sickleLegendary.cooldown = 0.9f;
            sickleLegendary.useSpeed = 1.0f;
            sickleLegendary.upgradeGoldCost = 0;
            sickleLegendary.upgradeTimeDays = 0;
            sickleLegendary.requiredLevel = 0;
            sickleLegendary.levelReqType = LevelReqType.PlayerLevel;
            sickleLegendary.upgradeMaterials = null;
            // DoubleHarvest | QualityBoost | SeedRecovery = 0b110000 | 0b001000 | 0b100000... wait
            // -> see docs/systems/tool-upgrade-architecture.md 섹션 3.5
            // DoubleHarvest = 1<<4 = 16, QualityBoost = 1<<3 = 8, SeedRecovery = 1<<5 = 32
            sickleLegendary.specialEffect =
                ToolSpecialEffect.DoubleHarvest | ToolSpecialEffect.QualityBoost | ToolSpecialEffect.SeedRecovery;
            sickleLegendary.description = "전설의 낫. 3x3 범위로 수확하며 보너스 수확, 품질 상승, 씨앗 회수 효과가 있다.";
            EditorUtility.SetDirty(sickleLegendary);

            sickleBasic.nextTier = sickleReinforced;
            sickleReinforced.nextTier = sickleLegendary;
            sickleLegendary.nextTier = null;
            EditorUtility.SetDirty(sickleBasic);
            EditorUtility.SetDirty(sickleReinforced);
        }

        private static void CreateMaterialSOs()
        {
            // -> see docs/systems/tool-upgrade.md 섹션 2.2
            var ironScrap = CreateOrLoad<MaterialItemData>("Resources/Data/Materials/SO_Material_IronScrap.asset");
            ironScrap.dataId = "iron_scrap";
            ironScrap.displayName = "철 조각";
            ironScrap.description = "도구 강화에 사용하는 재료. 대장간에서 구매할 수 있다.";
            ironScrap.isStackable = true;
            ironScrap.maxStack = 99;
            ironScrap.isSellable = false;
            ironScrap.baseSellPrice = 0;
            ironScrap.buyPrice = 100; // -> see docs/systems/tool-upgrade.md 섹션 2.2
            EditorUtility.SetDirty(ironScrap);

            var refinedSteel = CreateOrLoad<MaterialItemData>("Resources/Data/Materials/SO_Material_RefinedSteel.asset");
            refinedSteel.dataId = "refined_steel";
            refinedSteel.displayName = "정제 강철";
            refinedSteel.description = "고급 도구 제작에 필요한 재료. 대장간에서 구매할 수 있다.";
            refinedSteel.isStackable = true;
            refinedSteel.maxStack = 99;
            refinedSteel.isSellable = false;
            refinedSteel.baseSellPrice = 0;
            refinedSteel.buyPrice = 400; // -> see docs/systems/tool-upgrade.md 섹션 2.2
            EditorUtility.SetDirty(refinedSteel);
        }

        private static T CreateOrLoad<T>(string relativePath) where T : ScriptableObject
        {
            string fullPath = $"Assets/_Project/{relativePath}";
            var existing = AssetDatabase.LoadAssetAtPath<T>(fullPath);
            if (existing != null) return existing;

            var so = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(so, fullPath);
            return so;
        }
    }
}
