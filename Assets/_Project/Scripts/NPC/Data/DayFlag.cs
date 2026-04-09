// S-03: 요일 비트마스크 열거형
// -> see docs/systems/npc-shop-architecture.md 섹션 2.1
namespace SeedMind.NPC
{
    [System.Flags]
    public enum DayFlag
    {
        None      = 0,
        Monday    = 1 << 0,
        Tuesday   = 1 << 1,
        Wednesday = 1 << 2,
        Thursday  = 1 << 3,
        Friday    = 1 << 4,
        Saturday  = 1 << 5,
        Sunday    = 1 << 6
    }
}