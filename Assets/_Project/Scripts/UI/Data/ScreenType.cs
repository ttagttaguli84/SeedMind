// S-01: 화면 유형 열거형
// -> see docs/systems/ui-architecture.md 섹션 1.2
namespace SeedMind.UI
{
    public enum ScreenType
    {
        None        = 0,   // 화면 없음 (HUD만 표시)
        Farming     = 1,   // 농장 기본 화면 (= None과 동일)
        Inventory   = 2,   // 인벤토리 화면
        Shop        = 3,   // 상점 화면
        Quest       = 4,   // 퀘스트 목록 화면
        Achievement = 5,   // 업적 화면
        Menu        = 6,   // 메뉴/설정 화면
        SaveLoad    = 7,   // 세이브/로드 슬롯 화면
        Dialogue    = 8,   // NPC 대화 화면
        Processing  = 9,   // 가공소 화면
        Crafting    = 10,  // 크래프팅 화면 (향후 확장)
        ToolUpgrade = 11,  // 대장간 도구 업그레이드 화면
        Collection  = 12   // 수집 도감 화면
    }
}