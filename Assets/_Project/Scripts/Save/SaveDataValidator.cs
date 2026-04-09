// S-10: 세이브 데이터 무결성 검증
// -> see docs/systems/save-load-architecture.md 섹션 6.4
namespace SeedMind.Save
{
    using SeedMind.Save.Data;
    using System.Collections.Generic;

    public static class SaveDataValidator
    {
        public static List<string> Validate(GameSaveData data)
        {
            var errors = new List<string>();

            if (data == null)
            {
                errors.Add("GameSaveData is null");
                return errors;
            }

            if (string.IsNullOrEmpty(data.saveVersion))
                errors.Add("saveVersion이 비어있음");

            if (data.playTimeSeconds < 0)
                errors.Add("playTimeSeconds가 음수");

            return errors;
        }
    }
}
