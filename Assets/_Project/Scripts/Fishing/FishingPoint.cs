// FishingPoint MonoBehaviour — 낚시 가능 지점 정의
// -> see docs/systems/fishing-architecture.md 섹션 4
using UnityEngine;
using SeedMind.Fishing.Data;

namespace SeedMind.Fishing
{
    /// <summary>
    /// 씬에 배치되는 낚시 포인트. Zone F 연못 가장자리에 3개 배치.
    /// FishingManager가 관리하며, 하루 최대 사용 횟수 및 점유 상태를 추적한다.
    /// </summary>
    public class FishingPoint : MonoBehaviour
    {
        [Header("식별")]
        public string pointId;              // ex) "fp_01"
        public Vector2Int tilePosition;     // Zone F 내 타일 좌표

        [Header("어종 설정")]
        // 이 포인트에서 낚을 수 있는 어종 목록
        public FishData[] availableFish;

        // 희귀도별 기본 가중치 [Common, Uncommon, Rare, Legendary]
        // -> see docs/systems/fishing-system.md 섹션 4.1 for canonical 값
        public float[] rarityWeights = new float[4] { 0.60f, 0.28f, 0.10f, 0.02f };

        // --- 런타임 상태 ---
        private bool _isOccupied;
        private int  _dailyUseCount;

        [Header("제한")]
        public int maxDailyUseCount = 10;   // -> see docs/systems/fishing-system.md

        public bool IsAvailable => !_isOccupied && _dailyUseCount < maxDailyUseCount;

        public bool CanFish() => IsAvailable;

        public void SetOccupied(bool occupied) => _isOccupied = occupied;

        public void RecordUse() => _dailyUseCount++;

        public void ResetDailyCount() => _dailyUseCount = 0;
    }
}
