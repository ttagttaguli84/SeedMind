// 낚시 시스템 이벤트 허브 — 정적 Action 기반 (이벤트 키워드 없음)
// -> see docs/systems/fishing-architecture.md 섹션 1
using System;
using SeedMind.Fishing.Data;
using SeedMind.Economy;

namespace SeedMind.Fishing
{
    public static class FishingEvents
    {
        // 낚싯줄 투척 시작
        public static Action<FishingPoint> OnFishCast;

        // 입질 발생 — 낚인 물고기 데이터 전달
        public static Action<FishData> OnFishBite;

        // 낚시 성공 — 물고기 + 품질
        public static Action<FishData, CropQuality> OnFishCaught;

        // 낚시 숙련도 레벨업 (ARC-029)
        public static Action<int> OnProficiencyLevelUp;

        // 낚시 실패 — 물고기 도망
        public static Action OnFishingFailed;

        // 인벤토리 만석으로 획득 불가
        public static Action<FishData> OnInventoryFull;
    }
}
