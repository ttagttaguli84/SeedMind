// S-05: 시간대별 조명 비주얼 데이터 (Serializable)
// -> see docs/systems/time-season-architecture.md 섹션 2.1 (DayPhaseVisual 박스)
using UnityEngine;

namespace SeedMind.Core
{
    [System.Serializable]
    public class DayPhaseVisual
    {
        public DayPhase phase;
        public Color lightColor = Color.white;
        public float lightIntensity = 1.0f;
        public Vector3 lightRotation = new Vector3(50f, -30f, 0f);
        public Color ambientColor = Color.grey;
        public float transitionDuration = 5f; // 보간 시간 (초)
    }
}
