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

        // --- 업적 세이브 데이터 ---
        // -> see docs/systems/achievement-architecture.md 섹션 7.3
        public SeedMind.Achievement.AchievementSaveData achievements;

        // --- 튜토리얼 세이브 데이터 ---
        // -> see docs/systems/tutorial-architecture.md 섹션 7.2
        public SeedMind.Tutorial.TutorialSaveData tutorial;

        // --- 농장 확장(구역) 세이브 데이터 ---
        // -> see docs/systems/farm-expansion-architecture.md 섹션 9.2
        // null 허용: 이전 세이브 호환. null 시 초기 구역(Zone A)만 해금 상태로 초기화
        public SeedMind.Save.ZoneSaveData farmZones;

        // --- 축산 세이브 데이터 ---
        // -> see docs/systems/livestock-architecture.md 섹션 9.2
        // null 허용: 이전 세이브 호환. null 시 축사/닭장 미해금 상태로 초기화
        public SeedMind.Livestock.AnimalSaveData animals;
    }
}
