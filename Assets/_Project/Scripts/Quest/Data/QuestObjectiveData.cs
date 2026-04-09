// S-07: 퀘스트 목표 데이터 (직렬화 클래스)
// -> see docs/systems/quest-architecture.md 섹션 4.2
using UnityEngine;

namespace SeedMind.Quest.Data
{
    [System.Serializable]
    public class QuestObjectiveData
    {
        public ObjectiveType type;
        public string targetId;                   // 대상 ID (""이면 any)
        public int requiredAmount;                // 목표 수량 (-> see docs/systems/quest-system.md)
        public int minQuality;                    // 최소 품질 (QualityHarvest용, 0이면 무관)
        [TextArea(1, 2)]
        public string descriptionKR;              // 목표 설명 텍스트

        // Composite 전용
        public CompositeMode compositeMode;
        [SerializeReference]
        public QuestObjectiveData[] subObjectives;
    }
}