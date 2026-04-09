// S-10: 퀘스트 정적 정의 ScriptableObject
// -> see docs/systems/quest-architecture.md 섹션 4.1
using UnityEngine;
using SeedMind.Core;

namespace SeedMind.Quest.Data
{
    [CreateAssetMenu(fileName = "NewQuestData", menuName = "SeedMind/QuestData")]
    public class QuestData : ScriptableObject
    {
        [Header("기본 정보")]
        public string questId;
        public QuestCategory category;
        public string titleKR;
        [TextArea(2, 4)]
        public string descriptionKR;
        public string giverId;                    // "system"이면 시스템 자동 부여

        [Header("목표")]
        public QuestObjectiveData[] objectives;

        [Header("보상")]
        public QuestRewardData[] rewards;

        [Header("해금 조건")]
        public QuestUnlockCondition[] unlockConditions;

        [Header("제한")]
        public int timeLimitDays;                 // 0이면 무기한
                                                  // -> see docs/systems/quest-system.md 섹션 2.1
        public Season season;                     // None이면 전 계절
        public bool isRepeatable;

        [Header("UI")]
        public Sprite icon;                       // null이면 카테고리 기본 아이콘
    }
}