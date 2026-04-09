// 미니게임 결과 enum
// -> see docs/systems/fishing-architecture.md 섹션 2
namespace SeedMind.Fishing
{
    public enum MinigameResult
    {
        InProgress, // 진행 중
        Success,    // 성공 — 흥분 게이지가 성공 임계값 이상
        Fail        // 실패 — 게이지가 실패 임계값 이하이거나 시간 초과
    }
}
