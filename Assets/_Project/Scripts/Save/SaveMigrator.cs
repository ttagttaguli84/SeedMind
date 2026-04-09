// S-09: 세이브 버전 마이그레이션 체인
// -> see docs/systems/save-load-architecture.md 섹션 6.3
namespace SeedMind.Save
{
    using SeedMind.Save.Data;
    using UnityEngine;

    public static class SaveMigrator
    {
        public static GameSaveData Migrate(GameSaveData data)
        {
            Debug.Log($"[SaveMigrator] 마이그레이션: {data.saveVersion} -> {SaveManager.CURRENT_VERSION}");
            // 향후 버전 업 시 단계별 변환 로직 추가
            data.saveVersion = SaveManager.CURRENT_VERSION;
            return data;
        }
    }
}
