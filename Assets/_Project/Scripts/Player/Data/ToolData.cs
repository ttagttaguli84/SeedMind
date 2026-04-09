using UnityEngine;

namespace SeedMind.Player.Data
{
    public enum ToolType
    {
        Hoe = 0,
        WateringCan = 1,
        SeedBag = 2,
        Sickle = 3,
        Hand = 4
    }

    [CreateAssetMenu(fileName = "SO_Tool", menuName = "SeedMind/Player/ToolData")]
    public class ToolData : ScriptableObject
    {
        public string toolName;
        public ToolType toolType;
        public int tier;
        public int range; // → see docs/mcp/farming-tasks.md section B-4
    }
}
