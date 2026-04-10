using UnityEngine;
using SeedMind.Farm.Data;

namespace SeedMind.Farm
{
    public class CropInstance : MonoBehaviour
    {
        public CropData cropData;
        public int currentGrowthDays;
        public int currentStageIndex;
        private GameObject _stageObject;

        public void Initialize(CropData data)
        {
            cropData = data;
            currentGrowthDays = 0;
            currentStageIndex = 0;
        }

        public bool IsFullyGrown => cropData != null && currentGrowthDays >= cropData.growthDays;

        /// <summary>
        /// 하루 성장 진행. 완전 성장 시 true 반환 — 호출자가 타일 상태를 Harvestable로 전환해야 한다.
        /// </summary>
        public bool AdvanceDay()
        {
            if (cropData == null) return false;
            currentGrowthDays++;

            int stageCount = cropData.growthStagePrefabs != null ? cropData.growthStagePrefabs.Length : 0;
            if (stageCount > 0)
            {
                float progress = (float)currentGrowthDays / cropData.growthDays;
                int newStage = Mathf.Min(Mathf.FloorToInt(progress * stageCount), stageCount - 1);
                if (newStage != currentStageIndex)
                {
                    currentStageIndex = newStage;
                    if (_stageObject != null) Destroy(_stageObject);
                    if (cropData.growthStagePrefabs[currentStageIndex] != null)
                        _stageObject = Instantiate(cropData.growthStagePrefabs[currentStageIndex], transform);
                }
            }

            return IsFullyGrown;
        }
    }
}
