// 장식 런타임 인스턴스 (Plain C#)
// -> see docs/systems/decoration-architecture.md 섹션 2.4
using UnityEngine;
using SeedMind.Decoration.Data;

namespace SeedMind.Decoration
{
    /// <summary>배치된 장식 하나의 런타임 상태</summary>
    public class DecorationInstance
    {
        public int instanceId;
        public DecorationItemData data;
        public Vector3Int cell;
        public EdgeDirection edge;
        public int durability;
        public int colorVariantIndex;
        public GameObject runtimeObject;   // Light/Ornament/WaterDecor 전용; Fence/Path는 null
    }
}
