// S-02: 대장간 NPC 고유 데이터 ScriptableObject
// -> see docs/systems/blacksmith-architecture.md 섹션 4.2
using UnityEngine;

namespace SeedMind.NPC.Data
{
    [CreateAssetMenu(fileName = "NewBlacksmithNPCData", menuName = "SeedMind/BlacksmithNPCData")]
    public class BlacksmithNPCData : ScriptableObject
    {
        [Header("기본 식별")]
        public string npcId;                          // "npc_cheolsu"
        public string displayName;                    // -> see docs/content/npcs.md 섹션 4.1

        [Header("대화 -- 친밀도 단계별 인사")]
        public DialogueData[] greetingDialogues;      // 인덱스 = 친밀도 단계 (0~3)
        public DialogueData closedDialogue;            // 영업 외/휴무 시 대화
        public DialogueData pendingPickupDialogue;     // 완성 도구 수령 안내 대화

        [Header("친밀도")]
        public int[] affinityThresholds;              // 단계별 임계값
                                                      // -> see docs/content/blacksmith-npc.md 섹션 2.5
        public DialogueData[] affinityDialogues;      // 단계 상승 시 일회성 특수 대화

        [Header("친밀도 보상")]
        public int upgradeCompleteAffinity;            // 업그레이드 완료 시 친밀도 증가량
                                                      // -> see docs/content/blacksmith-npc.md 섹션 2.5
        public int materialPurchaseAffinity;           // 재료 1회 구매 시 친밀도 증가량
                                                      // -> see docs/content/blacksmith-npc.md 섹션 2.5

        [Header("친밀도 혜택")]
        public int specialDiscountAffinityLevel;      // 할인 혜택 해금 친밀도 단계
                                                      // -> see docs/content/blacksmith-npc.md 섹션 2.5
        public float discountRate;                    // 할인율 (0.1 = 10%)
                                                      // -> see docs/content/blacksmith-npc.md 섹션 2.5
    }
}
