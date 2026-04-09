// Editor 스크립트: 튜토리얼 SO 에셋 일괄 생성
// T-3: TutorialSequenceData + TutorialStepData SO
// T-4: 시스템 튜토리얼 SequenceData SO 4종
// T-5: ContextHintData SO 7종
using UnityEditor;
using UnityEngine;
using SeedMind.Tutorial.Data;

public class CreateTutorialAssets
{
    [MenuItem("SeedMind/Create Tutorial Assets")]
    public static void CreateAll()
    {
        CreateSequences();
        CreateSteps();
        CreateSystemSequences();
        CreateHints();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[CreateTutorialAssets] 완료: Sequences 5종, Steps 12종, Hints 7종 생성");
    }

    // ── T-3-01: 메인 시퀀스 SO ──────────────────────────────────────────
    static void CreateSequences()
    {
        var seq = ScriptableObject.CreateInstance<TutorialSequenceData>();
        seq.sequenceId = "SEQ_MainTutorial";
        seq.displayName = "메인 튜토리얼";
        seq.tutorialType = TutorialType.MainTutorial;
        seq.autoStartOnNewGame = true;
        seq.skippable = true;
        seq.pauseGameTime = false;
        seq.startTriggerType = TutorialTriggerType.NewGame;
        seq.prerequisiteSequenceId = "";
        seq.startTriggerParam = "";
        Save(seq, "Assets/_Project/Data/Tutorial/Sequences/SO_TutSeq_MainTutorial.asset");
    }

    // ── T-3-02: 메인 튜토리얼 단계 SO (12종) ───────────────────────────
    static void CreateSteps()
    {
        var steps = new (string id, string msg, TutorialUIType ui, StepCompletionType comp, string evt)[]
        {
            ("STEP_Main_01_Arrival",    "농장에 오신 것을 환영합니다!",          TutorialUIType.Popup,    StepCompletionType.ClickToContinue, ""),
            ("STEP_Main_02_Movement",   "WASD 또는 방향키로 이동할 수 있습니다.", TutorialUIType.Bubble,   StepCompletionType.EventBased,      "PlayerMoved"),
            ("STEP_Main_03_MeetHana",   "하나에게 인사해보세요.",                TutorialUIType.Arrow,    StepCompletionType.EventBased,      "NPCEvents.OnDialogueStarted"),
            ("STEP_Main_04_TillSoil",   "호미로 땅을 갈아보세요.",               TutorialUIType.Combined, StepCompletionType.EventBased,      "FarmEvents.OnTileTilled"),
            ("STEP_Main_05_PlantSeeds", "씨앗을 심어보세요.",                   TutorialUIType.Combined, StepCompletionType.EventBased,      "FarmEvents.OnCropPlanted"),
            ("STEP_Main_06_WaterCrops", "물을 줘서 작물이 자라게 해봐요.",        TutorialUIType.Combined, StepCompletionType.EventBased,      "FarmEvents.OnTileWatered"),
            ("STEP_Main_07_Sleep",      "오늘 하루도 수고했어요. 이제 자야 할 시간이에요.", TutorialUIType.Popup, StepCompletionType.EventBased, "TimeEvents.OnSleepTriggered"),
            ("STEP_Main_08_GrowthCheck","새싹이 자랐어요! 타일 위에 마우스를 올려보세요.", TutorialUIType.Bubble, StepCompletionType.ClickToContinue, ""),
            ("STEP_Main_09_Harvest",    "수확할 시기가 됐어요!",                 TutorialUIType.Combined, StepCompletionType.EventBased,      "FarmEvents.OnCropHarvested"),
            ("STEP_Main_10_FirstSale",  "하나의 상점에서 작물을 팔아봐요.",       TutorialUIType.Arrow,    StepCompletionType.EventBased,      "NPCEvents.OnShopOpened"),
            ("STEP_Main_11_Reinvest",   "번 돈으로 씨앗을 더 사봐요.",           TutorialUIType.Bubble,   StepCompletionType.ClickToContinue, ""),
            ("STEP_Main_12_Complete",   "튜토리얼 완료! 이제 자유롭게 농장을 키워보세요.", TutorialUIType.Popup, StepCompletionType.ClickToContinue, ""),
        };

        var stepNames = new string[]
        {
            "SO_TutStep_Main_01_Arrival", "SO_TutStep_Main_02_Movement", "SO_TutStep_Main_03_MeetHana",
            "SO_TutStep_Main_04_TillSoil", "SO_TutStep_Main_05_PlantSeeds", "SO_TutStep_Main_06_WaterCrops",
            "SO_TutStep_Main_07_Sleep", "SO_TutStep_Main_08_GrowthCheck", "SO_TutStep_Main_09_Harvest",
            "SO_TutStep_Main_10_FirstSale", "SO_TutStep_Main_11_Reinvest", "SO_TutStep_Main_12_Complete",
        };

        var soList = new TutorialStepData[steps.Length];
        for (int i = 0; i < steps.Length; i++)
        {
            var s = steps[i];
            var so = ScriptableObject.CreateInstance<TutorialStepData>();
            so.stepId = s.id;
            so.messageText = s.msg;
            so.uiType = s.ui;
            so.completionType = s.comp;
            so.completionEventType = s.evt;
            so.anchorType = TutorialAnchorType.ScreenPosition;
            so.blockOtherInput = false;
            Save(so, $"Assets/_Project/Data/Tutorial/Steps/{stepNames[i]}.asset");
            soList[i] = so;
        }

        // 메인 시퀀스에 단계 배열 할당
        var seqPath = "Assets/_Project/Data/Tutorial/Sequences/SO_TutSeq_MainTutorial.asset";
        var seqAsset = AssetDatabase.LoadAssetAtPath<TutorialSequenceData>(seqPath);
        if (seqAsset != null)
        {
            seqAsset.steps = soList;
            EditorUtility.SetDirty(seqAsset);
        }
    }

    // ── T-4: 시스템 튜토리얼 시퀀스 SO 4종 ────────────────────────────
    static void CreateSystemSequences()
    {
        var sequences = new (string id, string name, string trigger, string param)[]
        {
            ("SEQ_BuildingIntro",    "시설 소개",   "EventFired", "BuildingEvents.OnBuildingPlaced"),
            ("SEQ_ToolUpgradeIntro", "도구 업그레이드 소개", "EventFired", "ToolUpgradeEvents.OnUpgradeStarted"),
            ("SEQ_SeasonChange",     "계절 변화 안내", "EventFired", "TimeEvents.OnSeasonChanged"),
            ("SEQ_ProcessingIntro",  "가공 소개",   "EventFired", "BuildingEvents.OnProcessingComplete"),
        };
        var fileNames = new string[]
        {
            "SO_TutSeq_BuildingIntro", "SO_TutSeq_ToolUpgradeIntro",
            "SO_TutSeq_SeasonChange", "SO_TutSeq_ProcessingIntro",
        };
        for (int i = 0; i < sequences.Length; i++)
        {
            var s = sequences[i];
            var so = ScriptableObject.CreateInstance<TutorialSequenceData>();
            so.sequenceId = s.id;
            so.displayName = s.name;
            so.tutorialType = TutorialType.SystemTutorial;
            so.autoStartOnNewGame = false;
            so.skippable = true;
            so.startTriggerType = TutorialTriggerType.EventFired;
            so.startTriggerParam = s.param;
            so.prerequisiteSequenceId = "SEQ_MainTutorial";
            Save(so, $"Assets/_Project/Data/Tutorial/Sequences/{fileNames[i]}.asset");
        }
    }

    // ── T-5: ContextHintData SO 7종 ────────────────────────────────────
    static void CreateHints()
    {
        var hints = new (string id, string file, HintConditionType cond, string msg, int cd, int maxShow, bool req)[]
        {
            ("HINT_WaterReminder",   "SO_CtxHint_WaterReminder",   HintConditionType.DryTilesExist,      "물을 안 준 타일이 있어요! 작물이 말라죽을 수 있어요.", 3, 0, false),
            ("HINT_LowGold",         "SO_CtxHint_LowGold",         HintConditionType.LowGold,            "골드가 부족해요. 작물을 팔아 수입을 늘려보세요.", 5, 0, true),
            ("HINT_InventoryFull",   "SO_CtxHint_InventoryFull",   HintConditionType.InventoryFull,      "인벤토리가 가득 찼어요! 물건을 정리해보세요.", 2, 0, true),
            ("HINT_SeasonCrop",      "SO_CtxHint_SeasonCrop",      HintConditionType.SeasonMismatchCrop, "이 계절에 맞지 않는 작물이 있어요. 계절별 재배 작물을 확인해보세요.", 7, 3, true),
            ("HINT_HarvestReady",    "SO_CtxHint_HarvestReady",    HintConditionType.ReadyToHarvest,     "수확할 수 있는 작물이 있어요!", 1, 0, false),
            ("HINT_NightWarning",    "SO_CtxHint_NightWarning",    HintConditionType.NightTime,          "밤이 깊었어요. 수면을 취해야 다음날 활동할 수 있어요.", 3, 0, false),
            ("HINT_ProcessingReady", "SO_CtxHint_ProcessingReady", HintConditionType.ProcessingReady,    "가공이 완료됐어요! 결과물을 수령해주세요.", 2, 0, true),
        };
        foreach (var h in hints)
        {
            var so = ScriptableObject.CreateInstance<ContextHintData>();
            so.hintId = h.id;
            so.conditionType = h.cond;
            so.messageText = h.msg;
            so.cooldownDays = h.cd;
            so.maxShowCount = h.maxShow;
            so.requireTutorialComplete = h.req;
            so.displayDuration = 4f;
            so.priority = 1;
            Save(so, $"Assets/_Project/Data/Tutorial/Hints/{h.file}.asset");
        }
    }

    static void Save(ScriptableObject so, string path)
    {
        AssetDatabase.CreateAsset(so, path);
        EditorUtility.SetDirty(so);
    }
}
