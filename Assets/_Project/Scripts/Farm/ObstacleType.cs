// 장애물 종류를 정의하는 enum
// -> see docs/systems/farm-expansion-architecture.md 섹션 2.3
// -> see docs/systems/farm-expansion.md 섹션 3.1
namespace SeedMind.Farm
{
    public enum ObstacleType
    {
        Weed       = 0, // 잡초 (낫으로 제거, 1회)
        SmallRock  = 1, // 소형 돌 (호미, Basic 2회)
        LargeRock  = 2, // 대형 바위 2x2 (호미 Reinforced+ 필요)
        Stump      = 3, // 나무 그루터기 (호미, Basic 3회)
        SmallTree  = 4, // 소형 나무 (호미, Basic 2회)
        LargeTree  = 5, // 대형 나무 2x2 (호미 Reinforced+ 필요)
        Bush       = 6, // 덤불 (낫, Basic 2회)
    }
}
