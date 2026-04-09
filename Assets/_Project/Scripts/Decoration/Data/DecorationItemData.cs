// 장식 아이템 데이터 ScriptableObject
// -> see docs/systems/decoration-architecture.md 섹션 2.2
// 콘텐츠 수치 canonical: docs/content/decoration-items.md (CON-020)
using UnityEngine;
using UnityEngine.Tilemaps;
using SeedMind.Core;

namespace SeedMind.Decoration.Data
{
    [CreateAssetMenu(menuName = "SeedMind/Decoration/DecorationItemData")]
    public class DecorationItemData : ScriptableObject
    {
        [Header("식별")]
        public string itemId;           // 예: "FenceWood" -> see docs/content/decoration-items.md
        public string displayName;
        public Sprite icon;

        [Header("카테고리")]
        public DecoCategoryType category;

        [Header("가격")]
        public int buyPrice;            // -> see docs/content/decoration-items.md 섹션별 buyPrice

        [Header("배치 방식")]
        public bool isEdgePlaced;       // true = 울타리(edge), false = 타일 점유
        public int tileWidthX;          // 점유 타일 X (isEdgePlaced=true 이면 0)
        public int tileHeightZ;         // 점유 타일 Z (isEdgePlaced=true 이면 0)

        [Header("해금")]
        public int unlockLevel;         // 0 = 시작부터 가능 -> see docs/content/decoration-items.md
        public string unlockZoneId;     // 해금 구역 ID, 없으면 "" -> see docs/systems/farm-expansion.md

        [Header("계절 제약")]
        public bool hasSeasonLimit;     // false = 항상 판매
        public Season limitedSeason;    // hasSeasonLimit=true일 때만 유효 -> see docs/systems/time-season.md

        [Header("카테고리별 파라미터")]
        public float lightRadius;       // Light 전용; 나머지 0 -> see docs/content/decoration-items.md
        public float moveSpeedBonus;    // Path 전용 (0.1 = +10%); 나머지 0
        public int durabilityMax;       // Fence 전용; 0 = 영구

        [Header("렌더링")]
        public GameObject prefab;       // Light/Ornament/WaterDecor 오브젝트 프리팹
        public TileBase floorTile;      // Path 전용 Tilemap 타일
        public TileBase edgeTileH;      // Fence 수평 스프라이트
        public TileBase edgeTileV;      // Fence 수직 스프라이트
        public TileBase edgeTileCorner; // Fence 코너 스프라이트

        [Header("UI")]
        [TextArea] public string description;
    }
}
