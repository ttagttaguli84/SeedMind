// 물고기 희귀도 enum
// -> see docs/systems/fishing-architecture.md 섹션 3
namespace SeedMind.Fishing
{
    public enum FishRarity
    {
        Common    = 0,  // 일반 — 흔하게 출현
        Uncommon  = 1,  // 비일반 — 조건 충족 시 출현
        Rare      = 2,  // 희귀 — 특정 계절/시간/날씨
        Legendary = 3   // 전설 — 극히 낮은 확률
    }
}
