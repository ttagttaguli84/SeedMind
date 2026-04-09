// S-08: 개별 슬롯의 메타 정보 (UI 표시용)
// -> see docs/systems/save-load-architecture.md 섹션 4.4
namespace SeedMind.Save.Data
{
    [System.Serializable]
    public class SaveSlotInfo
    {
        public int slotIndex;
        public string slotName;         // 플레이어 지정 이름
        public string savedAt;          // ISO 8601
        public int year;                // 게임 내 연도
        public int seasonIndex;         // 게임 내 계절
        public int day;                 // 게임 내 일
        public int playTimeSeconds;     // 총 플레이 시간
        public int gold;                // 보유 골드
        public int level;               // 플레이어 레벨
        public bool exists;             // 세이브 데이터 존재 여부
    }
}
