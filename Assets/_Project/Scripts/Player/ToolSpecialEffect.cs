// T-01: 도구 특수 효과 [Flags] enum
// -> see docs/systems/tool-upgrade-architecture.md 섹션 3.5
// -> see docs/systems/tool-upgrade.md for 등급별 효과 canonical 값
namespace SeedMind.Player
{
    /// <summary>
    /// 고등급 도구의 특수 효과 (비트 플래그).
    /// 하나의 도구가 여러 효과를 동시에 가질 수 있다.
    /// 예: DoubleHarvest | QualityBoost | SeedRecovery = 56 (0b111000)
    /// </summary>
    [System.Flags]
    public enum ToolSpecialEffect
    {
        None          = 0,
        AreaEffect    = 1 << 0,   // 범위 효과 (3x3 등)
        ChargeAttack  = 1 << 1,   // 충전 사용 (긴 누르기로 범위 확대)
        AutoWater     = 1 << 2,   // 자동 물주기 효과
        QualityBoost  = 1 << 3,   // 수확 품질 보너스 (+1등급 확률)
        DoubleHarvest = 1 << 4,   // 이중 수확 (추가 드롭 확률)
        SeedRecovery  = 1 << 5,   // 씨앗 회수 확률
    }
}
