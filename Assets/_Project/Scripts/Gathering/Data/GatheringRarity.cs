// 채집 아이템 희귀도 열거형
// -> see docs/systems/gathering-architecture.md 섹션 2.2
namespace SeedMind.Gathering
{
    public enum GatheringRarity
    {
        Common,     // 흔함 — 높은 출현율
        Uncommon,   // 보통 — 중간 출현율
        Rare,       // 희귀 — 낮은 출현율
        Legendary   // 전설 — 매우 희귀, 특수 조건 필요
    }
}
