// 구역의 런타임 상태 (Plain C# class)
// -> see docs/systems/farm-expansion-architecture.md 섹션 1
namespace SeedMind.Farm
{
    public class ZoneRuntimeState
    {
        public string zoneId;
        public ZoneState state;
        public int clearedObstacleCount;
        public int totalObstacleCount;

        public ZoneRuntimeState(string zoneId, ZoneState initialState, int totalObstacleCount)
        {
            this.zoneId = zoneId;
            this.state = initialState;
            this.clearedObstacleCount = 0;
            this.totalObstacleCount = totalObstacleCount;
        }
    }
}
