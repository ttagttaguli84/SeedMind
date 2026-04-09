// S-06: 하나의 세이브 슬롯에 저장되는 전체 게임 상태
// -> see docs/systems/save-load-architecture.md 섹션 2.3 for 전체 필드 정의
namespace SeedMind.Save.Data
{
    [System.Serializable]
    public class GameSaveData
    {
        // --- 메타데이터 (3개) ---
        public string saveVersion;          // 세이브 포맷 버전
        public string savedAt;              // 저장 시각 (ISO 8601)
        public int playTimeSeconds;         // 총 플레이 시간 (초)

        // --- 시스템별 세이브 데이터 ---
        // -> see docs/systems/save-load-architecture.md 섹션 2.1 for 전체 트리
        public SeedMind.Core.TimeSaveData timeData;
        public SeedMind.Core.WeatherSaveData weatherData;

        // --- 퀘스트 세이브 데이터 ---
        // -> see docs/systems/quest-architecture.md 섹션 8.4
        public SeedMind.Quest.QuestSaveData quest;
    }
}
