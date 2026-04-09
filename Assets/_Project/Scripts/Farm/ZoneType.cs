// 농장 구역의 용도/특성을 정의하는 enum
// -> see docs/systems/farm-expansion-architecture.md 섹션 2.2
namespace SeedMind.Farm
{
    public enum ZoneType
    {
        Farmland   = 0, // 일반 경작지 (Zone A/B/C/D)
        Orchard    = 1, // 과수원 (Zone G)
        Pasture    = 2, // 목장 (Zone E)
        Greenhouse = 3, // 온실 (미래 확장용)
        Pond       = 4, // 연못/낚시 (Zone F)
    }
}
