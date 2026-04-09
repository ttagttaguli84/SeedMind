namespace SeedMind.Farm
{
    /// <summary>
    /// 타일의 계절 제약 해제 여부를 질의하는 인터페이스.
    /// GrowthSystem은 이 인터페이스에만 의존하며, GreenhouseSystem이 구현을 제공한다.
    /// -> see docs/systems/facilities-architecture.md 섹션 5.3
    /// </summary>
    public interface ISeasonOverrideProvider
    {
        bool IsSeasonOverridden(int tileX, int tileY);
    }
}
