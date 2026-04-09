// LevelReqType: 업그레이드 해금 조건 레벨 타입 (FIX-086)
// -> see docs/systems/tool-upgrade-architecture.md 섹션 3.6
namespace SeedMind.Player
{
    /// <summary>
    /// 업그레이드 해금 조건의 레벨 타입.
    /// 도구에 따라 플레이어 메인 레벨 또는 특정 숙련도 레벨을 참조한다.
    /// </summary>
    public enum LevelReqType
    {
        PlayerLevel      = 0,  // 기본: 플레이어 메인 레벨 (-> see docs/balance/progression-curve.md)
        GatheringMastery = 1,  // 채집 숙련도 (-> see docs/systems/gathering-system.md 섹션 4)
        FishingMastery   = 2,  // 낚시 숙련도 — 예약, 미사용
    }
}
