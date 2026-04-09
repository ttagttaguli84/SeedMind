// S-02: NPC 활동 상태 열거형
// -> see docs/systems/npc-shop-architecture.md 섹션 3.2
namespace SeedMind.NPC
{
    public enum NPCActivityState
    {
        Active,          // 영업 중, 인터랙션 가능
        Closed,          // 영업 외 시간 또는 휴무일
        WeatherClosed,   // 악천후로 임시 마감 (-> see docs/systems/time-season.md 섹션 3.4)
        Away             // 부재 (여행 상인 미방문 상태)
    }
}