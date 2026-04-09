// 장식 시스템 세이브 데이터
// -> see docs/systems/decoration-architecture.md 섹션 2.6 (PATTERN-005 준수)
using System.Collections.Generic;
using SeedMind.Decoration.Data;

namespace SeedMind.Decoration
{
    [System.Serializable]
    public class DecorationSaveData
    {
        public List<DecorationInstanceSave> decorations = new List<DecorationInstanceSave>();
        public int nextInstanceId;
    }

    [System.Serializable]
    public class DecorationInstanceSave
    {
        public int instanceId;
        public string itemId;
        public int cellX;
        public int cellZ;
        public EdgeDirection edge;
        public int durability;
        public int colorVariantIndex;
    }
}
