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

        public void AdvanceDay()
        {
            if (cropData == null) return;
            currentGrowthDays++;
            int stageCount = cropData.growthStagePrefabs != null ? cropData.growthStagePrefabs.Length : 0;
            if (stageCount == 0) return;
            float progress = (float)currentGrowthDays / cropData.growthDays;
            int newStage = Mathf.Min(Mathf.FloorToInt(progress * stageCount), stageCount - 1);
            if (newStage == currentStageIndex) return;
            currentStageIndex = newStage;
            if (_stageObject != null) Destroy(_stageObject);
            if (cropData.growthStagePrefabs[currentStageIndex] != null)
                _stageObject = Instantiate(cropData.growthStagePrefabs[currentStageIndex], transform);
        }
    }
}
