// Editor 스크립트: DialogueData SO 6종 일괄 생성
// 대화 노드/선택지 중첩 배열을 코드로 직접 설정
using UnityEngine;
using UnityEditor;
using SeedMind.NPC.Data;

namespace SeedMind.Editor
{
    public static class CreateDialogueAssets
    {
        private const string DialoguePath = "Assets/_Project/Data/Dialogues/";

        [MenuItem("SeedMind/Tools/Create Dialogue Assets")]
        public static void CreateAll()
        {
            CreateGreetingMerchant();
            CreateGreetingBlacksmith();
            CreateGreetingCarpenter();
            CreateClosedMerchant();
            CreateClosedBlacksmith();
            CreateClosedCarpenter();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SeedMind] DialogueData SO 6종 생성 완료.");
        }

        private static DialogueData CreateDialogue(string fileName, string dialogueId)
        {
            var asset = ScriptableObject.CreateInstance<DialogueData>();
            asset.dialogueId = dialogueId;
            AssetDatabase.CreateAsset(asset, DialoguePath + fileName + ".asset");
            return asset;
        }

        private static DialogueNode MakeNode(string speaker, string text, DialogueChoice[] choices)
        {
            return new DialogueNode
            {
                speakerName = speaker,
                text = text,
                choices = choices
            };
        }

        private static DialogueChoice MakeChoice(string text, DialogueChoiceAction action, int jumpToNode = -1)
        {
            return new DialogueChoice
            {
                choiceText = text,
                action = action,
                jumpToNode = jumpToNode
            };
        }

        // --- 인사 대화 ---

        private static void CreateGreetingMerchant()
        {
            var dlg = CreateDialogue("SO_Dlg_Greeting_Merchant", "greeting_merchant");
            dlg.nodes = new[]
            {
                MakeNode("하나", "어서오세요! 오늘도 좋은 물건이 많이 들어왔어요. 무엇을 도와드릴까요?",
                    new[]
                    {
                        MakeChoice("물건 구매", DialogueChoiceAction.OpenShop),
                        MakeChoice("물건 판매", DialogueChoiceAction.OpenShop),
                        MakeChoice("대화 종료", DialogueChoiceAction.CloseDialogue)
                    })
            };
            EditorUtility.SetDirty(dlg);
        }

        private static void CreateGreetingBlacksmith()
        {
            var dlg = CreateDialogue("SO_Dlg_Greeting_Blacksmith", "greeting_blacksmith");
            dlg.nodes = new[]
            {
                MakeNode("철수", "어서 오쇼. 도구 업그레이드가 필요하면 말만 해요. 재료 판매도 하고 있소.",
                    new[]
                    {
                        MakeChoice("도구 업그레이드", DialogueChoiceAction.OpenUpgrade),
                        MakeChoice("재료 구매", DialogueChoiceAction.OpenShop),
                        MakeChoice("대화 종료", DialogueChoiceAction.CloseDialogue)
                    })
            };
            EditorUtility.SetDirty(dlg);
        }

        private static void CreateGreetingCarpenter()
        {
            var dlg = CreateDialogue("SO_Dlg_Greeting_Carpenter", "greeting_carpenter");
            dlg.nodes = new[]
            {
                MakeNode("목이", "안녕하세요! 새 시설이 필요하신가요? 설계도를 보여드릴게요.",
                    new[]
                    {
                        MakeChoice("시설 건설", DialogueChoiceAction.OpenBuild),
                        MakeChoice("대화 종료", DialogueChoiceAction.CloseDialogue)
                    })
            };
            EditorUtility.SetDirty(dlg);
        }

        // --- 휴무 대화 ---

        private static void CreateClosedMerchant()
        {
            var dlg = CreateDialogue("SO_Dlg_Closed_Merchant", "closed_merchant");
            dlg.nodes = new[]
            {
                MakeNode("하나", "지금은 쉬는 시간이에요. 나중에 다시 들러주세요!", new DialogueChoice[0])
            };
            EditorUtility.SetDirty(dlg);
        }

        private static void CreateClosedBlacksmith()
        {
            var dlg = CreateDialogue("SO_Dlg_Closed_Blacksmith", "closed_blacksmith");
            dlg.nodes = new[]
            {
                MakeNode("철수", "오늘은 문을 닫았소. 내일 다시 오시오.", new DialogueChoice[0])
            };
            EditorUtility.SetDirty(dlg);
        }

        private static void CreateClosedCarpenter()
        {
            var dlg = CreateDialogue("SO_Dlg_Closed_Carpenter", "closed_carpenter");
            dlg.nodes = new[]
            {
                MakeNode("목이", "지금은 영업 시간이 아니에요. 나중에 다시 방문해 주세요.", new DialogueChoice[0])
            };
            EditorUtility.SetDirty(dlg);
        }
    }
}
