// S-11: 튜토리얼 진행 상태 직렬화 클래스
// -> see docs/systems/tutorial-architecture.md 섹션 7.2 (PATTERN-005 준수)
using System.Collections.Generic;

namespace SeedMind.Tutorial
{
    [System.Serializable]
    public class TutorialSaveData
    {
        public List<string> completedSequenceIds;
        public List<string> completedStepIds;
        public string activeSequenceId;
        public int activeStepIndex;
        public Dictionary<string, int> contextHintCooldowns;
    }
}
