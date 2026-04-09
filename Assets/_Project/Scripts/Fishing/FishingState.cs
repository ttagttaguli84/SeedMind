// 낚시 상태 머신 enum
// -> see docs/systems/fishing-architecture.md 섹션 11
namespace SeedMind.Fishing
{
    public enum FishingState
    {
        Idle,       // 대기 — 낚시 시작 가능
        Casting,    // 캐스팅 중 — 낚싯줄 던지는 애니메이션
        Waiting,    // 입질 대기 — 물속에서 물고기 대기
        Biting,     // 입질 발생 — 플레이어 반응 대기
        Reeling,    // 릴링 미니게임 진행 중
        Success,    // 낚시 성공 — 물고기 획득
        Fail        // 낚시 실패 — 물고기 도망
    }
}
