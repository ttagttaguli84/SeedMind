// S-09: 상황별 힌트 ScriptableObject
// -> see docs/systems/tutorial-architecture.md 섹션 8.2
using UnityEngine;

namespace SeedMind.Tutorial.Data
{
    [CreateAssetMenu(fileName = "NewContextHint", menuName = "SeedMind/ContextHintData")]
    public class ContextHintData : ScriptableObject
    {
        [Header("식별")]
        public string hintId;

        [Header("발동 조건")]
        public HintConditionType conditionType;
        public string conditionParam;

        [Header("표시")]
        [TextArea(2, 4)]
        public string messageText;
        public Sprite icon;
        public float displayDuration;

        [Header("빈도 제어")]
        public int cooldownDays;
        public int maxShowCount;
        public bool requireTutorialComplete;

        [Header("우선순위")]
        public int priority;
    }
}
