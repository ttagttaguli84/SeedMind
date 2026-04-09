namespace SeedMind
{
    /// <summary>
    /// 수확물의 재배 출처 열거형.
    /// 온실 판매가 보정, 수확 출처별 경제 계산에 사용된다.
    /// -> see docs/systems/economy-architecture.md 섹션 3.10.2 for canonical 정의
    /// </summary>
    public enum HarvestOrigin
    {
        Outdoor   = 0,   // 야외 농장 타일에서 수확
        Greenhouse = 1,   // 온실 내부 타일에서 수확
        Barn      = 2,   // 외양간/목장 동물 생산물
        Fishing   = 3,   // 낚시 포획물
        Gathering = 4,   // 야생 채집물
    }
}
