// S-07: 튜토리얼 시퀀스 ScriptableObject
// -> see docs/systems/tutorial-architecture.md 섹션 4.1
using UnityEngine;

namespace SeedMind.Tutorial.Data
{
    [CreateAssetMenu(fileName = "NewTutorialSequence", menuName = "SeedMind/TutorialSequenceData")]
    public class TutorialSequenceData : ScriptableObject
    {
        [Header("기본 정보")]
        public string sequenceId;
        public string displayName;
        public TutorialType tutorialType;

        [Header("시퀀스 구성")]
        public TutorialStepData[] steps;

        [Header("시작 조건")]
        public bool autoStartOnNewGame;
        public string prerequisiteSequenceId;
        public TutorialTriggerType startTriggerType;
        public string startTriggerParam;

        [Header("옵션")]
        public bool skippable;
        public bool pauseGameTime;
    }
}
