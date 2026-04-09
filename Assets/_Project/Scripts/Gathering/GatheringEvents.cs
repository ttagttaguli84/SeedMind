// 채집 시스템 정적 이벤트 허브
// -> see docs/systems/gathering-architecture.md 섹션 1
using System;
using SeedMind.Economy;

namespace SeedMind.Gathering
{
    public static class GatheringEvents
    {
        /// <summary>아이템 채집 완료 (아이템, 품질, 수량)</summary>
        public static Action<GatheringItemData, CropQuality, int> OnItemGathered;

        /// <summary>채집 포인트 소진됨</summary>
        public static Action<GatheringPoint> OnPointDepleted;

        /// <summary>채집 포인트 재생성됨</summary>
        public static Action<GatheringPoint> OnPointRespawned;

        /// <summary>채집 숙련도 레벨업 (newLevel)</summary>
        public static Action<int> OnProficiencyLevelUp;

        /// <summary>배낭 가득 — 채집 실패</summary>
        public static Action<GatheringItemData> OnInventoryFull;
    }
}
