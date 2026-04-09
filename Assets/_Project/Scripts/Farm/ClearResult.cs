// 장애물 개간 시도 결과를 나타내는 enum
// -> see docs/systems/farm-expansion-architecture.md 섹션 5.4
namespace SeedMind.Farm
{
    public enum ClearResult
    {
        NoObstacle,    // 해당 타일에 장애물 없음
        AlreadyCleared, // 이미 제거된 장애물
        WrongTool,     // 부적합한 도구 (예: 낫으로 바위 시도)
        Hit,           // 타격 성공 (아직 제거 안 됨)
        Cleared,       // 완전히 제거됨
    }
}
