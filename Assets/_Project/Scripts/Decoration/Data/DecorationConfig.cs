// 장식 시스템 전역 설정 ScriptableObject
// -> see docs/systems/decoration-architecture.md 섹션 2.3
// 수치 canonical: docs/systems/decoration-system.md 섹션 2.1~2.2
using UnityEngine;

namespace SeedMind.Decoration.Data
{
    [CreateAssetMenu(menuName = "SeedMind/Decoration/DecorationConfig")]
    public class DecorationConfig : ScriptableObject
    {
        [Header("배치 UI 색상")]
        public Color validHighlightColor   = new Color(0f, 1f, 0f, 0.4f);   // 배치 가능 하이라이트
        public Color invalidHighlightColor = new Color(1f, 0f, 0f, 0.4f);   // 배치 불가 하이라이트

        [Header("울타리 내구도")]
        public float fenceDurabilityDecayPerSeason = 10f; // 계절당 감소량 -> see docs/systems/decoration-system.md 섹션 2.1
        public float fenceRepairCostRatio          = 0.2f; // 수리 비용 = buyPrice × ratio -> see docs/content/decoration-items.md 섹션 1.2

        [Header("경로 설정")]
        public bool pathSpeedBonusEnabled = true; // 이동속도 보너스 활성 여부 -> see docs/systems/decoration-system.md 섹션 2.2
    }
}
