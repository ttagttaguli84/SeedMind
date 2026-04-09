// S-07: 세이브 슬롯 메타 파일 구조
// -> see docs/systems/save-load-architecture.md 섹션 4.4
namespace SeedMind.Save.Data
{
    [System.Serializable]
    public class SaveMetaFile
    {
        public SaveSlotInfo[] slots;  // MAX_SLOTS 크기
    }
}
