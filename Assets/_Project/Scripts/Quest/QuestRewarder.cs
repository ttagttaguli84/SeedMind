// S-15: 퀘스트 보상 지급 처리
// -> see docs/systems/quest-architecture.md 섹션 3.4
using UnityEngine;
using SeedMind.Quest.Data;

namespace SeedMind.Quest
{
    public class QuestRewarder
    {
        // 외부 시스템 참조 (Initialize 시 주입)
        // private EconomyManager _economyManager;
        // private ProgressionManager _progressionManager;
        // private InventoryManager _inventoryManager;

        public void GrantRewards(QuestData questData, int playerLevel)
        {
            foreach (var reward in questData.rewards)
            {
                int amount = reward.scaledByLevel
                    ? ApplyLevelScale(reward.amount, playerLevel)
                    : reward.amount;
                // switch (reward.type) 분기 처리
                // -> see docs/systems/quest-architecture.md 섹션 3.4
            }
        }

        private int ApplyLevelScale(int baseValue, int playerLevel)
        {
            // -> see docs/systems/quest-system.md 섹션 5.2 for 스케일 공식
            float scale = 1f + (playerLevel - 1) * 0.1f; // -> see canonical
            scale = Mathf.Min(scale, 1.9f);               // -> see canonical
            return Mathf.RoundToInt(baseValue * scale);
        }
    }
}