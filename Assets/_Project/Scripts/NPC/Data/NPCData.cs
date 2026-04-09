// S-08: NPC 정적 데이터 ScriptableObject
// -> see docs/systems/npc-shop-architecture.md 섹션 2.1
using UnityEngine;

namespace SeedMind.NPC.Data
{
    [CreateAssetMenu(fileName = "NewNPCData", menuName = "SeedMind/NPCData")]
    public class NPCData : ScriptableObject
    {
        [Header("기본 정보")]
        public string npcId;                     // -> see docs/content/npcs.md
        public string displayName;               // -> see docs/content/npcs.md
        public NPCType npcType;
        public Sprite portrait;

        [Header("위치/스케줄")]
        public Vector3 defaultPosition;          // -> see docs/content/npcs.md 섹션 2.2
        public int openHour;                     // -> see docs/systems/time-season.md 섹션 1.7
        public int closeHour;                    // -> see docs/systems/time-season.md 섹션 1.7
        public DayFlag closedDays;               // -> see docs/systems/economy-system.md 섹션 3.2

        [Header("대화")]
        public DialogueData greetingDialogue;
        public DialogueData closedDialogue;

        [Header("상점 연결")]
        public ScriptableObject shopData;        // ShopData SO 참조 (null이면 상점 없음)
                                                 // -> see docs/systems/economy-architecture.md 섹션 4.3

        [Header("시각")]
        public GameObject prefab;
        public float interactionRadius;          // -> see docs/content/npcs.md
    }
}