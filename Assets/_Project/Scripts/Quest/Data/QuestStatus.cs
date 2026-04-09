// S-02: 퀘스트 상태 열거형
// -> see docs/systems/quest-architecture.md 섹션 2.2
namespace SeedMind.Quest
{
    public enum QuestStatus
    {
        Locked      = 0,   // 해금 조건 미충족
        Available   = 1,   // 해금됨, 수락 대기
        Active      = 2,   // 진행 중
        Completed   = 3,   // 목표 달성, 보상 수령 대기
        Rewarded    = 4,   // 보상 수령 완료
        Failed      = 5,   // 시간 초과 실패
        Expired     = 6    // 만료 (일일 목표 자동 소멸)
    }
}