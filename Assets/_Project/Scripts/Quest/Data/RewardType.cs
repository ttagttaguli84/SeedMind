// S-04: 퀘스트 보상 타입 열거형
// -> see docs/systems/quest-architecture.md 섹션 2.4
namespace SeedMind.Quest
{
    public enum RewardType
    {
        Gold            = 0,
        XP              = 1,
        Item            = 2,
        Recipe          = 3,   // 가공 레시피 해금
        Unlock          = 4    // 시설/기능 해금
    }
}