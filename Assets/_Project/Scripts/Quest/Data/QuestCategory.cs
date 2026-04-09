// S-01: 퀘스트 카테고리 열거형
// -> see docs/systems/quest-architecture.md 섹션 2.1
namespace SeedMind.Quest
{
    public enum QuestCategory
    {
        MainQuest        = 0,   // 계절별 메인 퀘스트
        NPCRequest       = 1,   // NPC 의뢰
        DailyChallenge   = 2,   // 일일 목표
        FarmChallenge    = 3    // 농장 도전
    }
}