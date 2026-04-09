// T-2: 대장간 NPC 에셋 일괄 생성 Editor 스크립트
// -> see docs/mcp/blacksmith-tasks.md 섹션 3
// -> see docs/content/blacksmith-npc.md 섹션 2.5, 3.1~3.8
using UnityEngine;
using UnityEditor;
using SeedMind.NPC.Data;

namespace SeedMind.Editor
{
    public static class CreateBlacksmithAssets
    {
        private const string NPC_PATH      = "Assets/_Project/Data/NPCs";
        private const string DIALOGUE_PATH = "Assets/_Project/Data/Dialogues";

        [MenuItem("SeedMind/Create/Blacksmith Assets")]
        public static void CreateAll()
        {
            CreateDialogueAssets();
            CreateBlacksmithNPCData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[CreateBlacksmithAssets] 완료: SO 11종 생성/업데이트");
        }

        // ── DialogueData SO 10종 ───────────────────────────────────────

        private static void CreateDialogueAssets()
        {
            // A-02: 낯선 사이 인사 (Lv0)
            CreateDialogue("SO_Dlg_Blacksmith_Greet_Lv0",
                "dlg_blacksmith_greet_lv0",
                "철수",
                "왔나. 볼일이 있으면 말해.");
            // -> see docs/content/blacksmith-npc.md 섹션 3.2 general_01

            // A-03: 알고 지내는 사이 인사 (Lv1)
            CreateDialogue("SO_Dlg_Blacksmith_Greet_Lv1",
                "dlg_blacksmith_greet_lv1",
                "철수",
                "도구 상태는 어때? 무리하게 쓰지 마.");
            // -> see docs/content/blacksmith-npc.md 섹션 3.2 general_02

            // A-04: 단골 인사 (Lv2)
            CreateDialogue("SO_Dlg_Blacksmith_Greet_Lv2",
                "dlg_blacksmith_greet_lv2",
                "철수",
                "요즘 자주 오는군. 도구는 잘 쓰고 있어?");
            // -> see docs/content/blacksmith-npc.md 섹션 3.7 regular_01

            // A-05: 친구 인사 (Lv3)
            CreateDialogue("SO_Dlg_Blacksmith_Greet_Lv3",
                "dlg_blacksmith_greet_lv3",
                "철수",
                "...오늘은 일찍 왔네. 괜찮아, 좀 쉬었다 가.");
            // -> see docs/content/blacksmith-npc.md 섹션 3.7 friend_03

            // A-06: 영업 외 대화
            CreateDialogue("SO_Dlg_Blacksmith_Closed",
                "dlg_blacksmith_closed",
                "철수",
                "...지금은 쉬는 시간이야. 나중에 와.");
            // -> see docs/content/blacksmith-npc.md 섹션 3.7 (영업 외)

            // A-07: 도구 수령 안내
            CreateDialogue("SO_Dlg_Blacksmith_Pickup",
                "dlg_blacksmith_pickup",
                "철수",
                "다 됐어. 가져가.");
            // -> see docs/content/blacksmith-npc.md 섹션 3.4

            // A-08: 친밀도 Lv1 상승 특수 대화
            CreateDialogue("SO_Dlg_Blacksmith_Affinity_Lv1",
                "dlg_blacksmith_affinity_lv1",
                "철수",
                "첫 번째 강화 도구야. 차이를 느낄 거야. ...잘 쓰라고.");
            // -> see docs/content/blacksmith-npc.md 섹션 3.5

            // A-09: 친밀도 Lv2 상승 특수 대화
            CreateDialogue("SO_Dlg_Blacksmith_Affinity_Lv2",
                "dlg_blacksmith_affinity_lv2",
                "철수",
                "꽤 쓸 줄 아는 농부네.");
            // -> see docs/content/blacksmith-npc.md 섹션 3.5

            // A-10: 친밀도 Lv3 상승 특수 대화
            CreateDialogue("SO_Dlg_Blacksmith_Affinity_Lv3",
                "dlg_blacksmith_affinity_lv3",
                "철수",
                "사실 나, 젊을 때 도시에서 일한 적 있어. 공장에서 도구를 찍어내는 거지. ...맞지 않더라고.");
            // -> see docs/content/blacksmith-npc.md 섹션 3.7 friend_01

            // A-11: 최초 만남 대화
            CreateDialogue("SO_Dlg_Blacksmith_FirstMeet",
                "dlg_blacksmith_firstmeet",
                "철수",
                "...응. 새 농부구나.\n도구가 보이는데, 기본 도구로군.\n나중에 좀 더 쓸 만한 게 필요하면 재료를 가져와. 만들어 줄 테니.");
            // -> see docs/content/blacksmith-npc.md 섹션 3.1
        }

        private static DialogueData CreateDialogue(
            string assetName, string dialogueId, string speaker, string text)
        {
            string path = $"{DIALOGUE_PATH}/{assetName}.asset";
            var so = AssetDatabase.LoadAssetAtPath<DialogueData>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<DialogueData>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.dialogueId = dialogueId;
            so.nodes = new DialogueNode[]
            {
                new DialogueNode
                {
                    speakerName = speaker,
                    text        = text,
                    choices     = new DialogueChoice[0]
                }
            };
            EditorUtility.SetDirty(so);
            return so;
        }

        // ── BlacksmithNPCData SO ──────────────────────────────────────

        private static void CreateBlacksmithNPCData()
        {
            string path = $"{NPC_PATH}/SO_BlacksmithNPC_Cheolsu.asset";
            var so = AssetDatabase.LoadAssetAtPath<BlacksmithNPCData>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<BlacksmithNPCData>();
                AssetDatabase.CreateAsset(so, path);
            }

            so.npcId       = "npc_cheolsu";
            so.displayName = "철수";

            // 친밀도 임계값: -> see docs/content/blacksmith-npc.md 섹션 2.5 (canonical)
            so.affinityThresholds = new int[] { 0, 10, 25, 50 };

            // 친밀도 보상: -> see docs/content/blacksmith-npc.md 섹션 2.5
            so.upgradeCompleteAffinity  = 5;
            so.materialPurchaseAffinity = 1;

            // 친밀도 혜택: -> see docs/content/blacksmith-npc.md 섹션 2.5
            so.specialDiscountAffinityLevel = 3;
            so.discountRate = 0.1f;

            // DialogueData 참조 연결
            so.greetingDialogues = new DialogueData[]
            {
                Load<DialogueData>($"{DIALOGUE_PATH}/SO_Dlg_Blacksmith_Greet_Lv0.asset"),
                Load<DialogueData>($"{DIALOGUE_PATH}/SO_Dlg_Blacksmith_Greet_Lv1.asset"),
                Load<DialogueData>($"{DIALOGUE_PATH}/SO_Dlg_Blacksmith_Greet_Lv2.asset"),
                Load<DialogueData>($"{DIALOGUE_PATH}/SO_Dlg_Blacksmith_Greet_Lv3.asset"),
            };
            so.closedDialogue         = Load<DialogueData>($"{DIALOGUE_PATH}/SO_Dlg_Blacksmith_Closed.asset");
            so.pendingPickupDialogue   = Load<DialogueData>($"{DIALOGUE_PATH}/SO_Dlg_Blacksmith_Pickup.asset");
            so.affinityDialogues = new DialogueData[]
            {
                Load<DialogueData>($"{DIALOGUE_PATH}/SO_Dlg_Blacksmith_Affinity_Lv1.asset"),
                Load<DialogueData>($"{DIALOGUE_PATH}/SO_Dlg_Blacksmith_Affinity_Lv2.asset"),
                Load<DialogueData>($"{DIALOGUE_PATH}/SO_Dlg_Blacksmith_Affinity_Lv3.asset"),
            };

            EditorUtility.SetDirty(so);
        }

        private static T Load<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
                Debug.LogWarning($"[CreateBlacksmithAssets] 에셋 미발견: {path}");
            return asset;
        }
    }
}
