// S-08: 개별 튜토리얼 단계 ScriptableObject
// -> see docs/systems/tutorial-architecture.md 섹션 4.2
using UnityEngine;

namespace SeedMind.Tutorial.Data
{
    [CreateAssetMenu(fileName = "NewTutorialStep", menuName = "SeedMind/TutorialStepData")]
    public class TutorialStepData : ScriptableObject
    {
        [Header("식별")]
        public string stepId;

        [Header("UI 표시")]
        public TutorialUIType uiType;
        [TextArea(2, 5)]
        public string messageText;
        public Sprite iconOverride;
        public TutorialAnchorType anchorType;
        public string anchorTargetId;

        [Header("화살표/하이라이트")]
        public bool showArrow;
        public Vector2 arrowOffset;
        public bool showHighlight;
        public float highlightRadius;

        [Header("완료 조건")]
        public StepCompletionType completionType;
        public string completionEventType;
        public string completionParam;
        public float autoAdvanceDelay;

        [Header("입력 제어")]
        public bool blockOtherInput;
        public string[] allowedInputActions;
    }
}
