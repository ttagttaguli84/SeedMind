// T-05: 도구 등급별 효과 계산 유틸리티 (static)
// -> see docs/systems/tool-upgrade-architecture.md 섹션 4
// 모든 수치의 canonical 출처: -> see docs/systems/tool-upgrade.md
using UnityEngine;
using SeedMind.Player.Data;

namespace SeedMind.Player
{
    /// <summary>
    /// 도구 등급별 효과 계산 유틸리티.
    /// ToolData SO의 필드를 읽어 가공된 효과 수치를 반환한다.
    /// 모든 시스템이 이 클래스를 통해 도구 효과를 조회하는 단일 진입점.
    /// -> see docs/systems/tool-upgrade-architecture.md 섹션 4.1 (소비자 목록)
    /// </summary>
    public static class ToolEffectResolver
    {
        /// <summary>
        /// 도구의 유효 범위(타일 수)를 반환한다.
        /// Hoe T1=1, T2=3, T3=9 (-> see docs/systems/tool-upgrade.md 섹션 3.1)
        /// </summary>
        public static int GetEffectiveRange(ToolData tool)
        {
            return tool != null ? tool.range : 1;
        }

        /// <summary>
        /// 도구 1회 사용 당 에너지 소모량을 반환한다.
        /// -> see docs/systems/tool-upgrade.md 섹션 3.1~3.3
        /// </summary>
        public static int GetEnergyCost(ToolData tool)
        {
            return tool != null ? tool.energyCost : 1;
        }

        /// <summary>
        /// 도구 사용 속도 배수를 반환한다 (기본 1.0).
        /// -> see docs/systems/tool-upgrade.md 섹션 3.1~3.3
        /// </summary>
        public static float GetUseSpeed(ToolData tool)
        {
            return tool != null ? tool.useSpeed : 1f;
        }

        /// <summary>
        /// 물뿌리개 전용: 한 번 충전당 뿌릴 수 있는 타일 수.
        /// 다른 도구는 0을 반환한다.
        /// -> see docs/systems/tool-upgrade.md 섹션 3.2
        /// </summary>
        public static int GetWateringCapacity(ToolData tool)
        {
            if (tool == null || tool.toolType != ToolType.WateringCan) return 0;
            return tool.range; // -> see docs/systems/tool-upgrade.md 물뿌리개 저수량
        }

        /// <summary>
        /// 도구의 특수 효과를 [Flags] enum으로 반환한다.
        /// HasFlag()로 복합 효과 검사 가능.
        /// 예: GetSpecialEffect(tool).HasFlag(ToolSpecialEffect.DoubleHarvest)
        /// -> see docs/systems/tool-upgrade.md 섹션 3.3
        /// </summary>
        public static ToolSpecialEffect GetSpecialEffect(ToolData tool)
        {
            return tool != null ? tool.specialEffect : ToolSpecialEffect.None;
        }

        /// <summary>
        /// 도구의 영향 타일 패턴을 반환한다 (그리드 오프셋 배열).
        /// range=1 → [(0,0)], range=3 → [(-1,0),(0,0),(1,0)], range=9 → 3x3
        /// -> see docs/systems/tool-upgrade-architecture.md 섹션 4
        /// </summary>
        public static Vector2Int[] GetTilePattern(ToolData tool)
        {
            int range = tool != null ? tool.range : 1;

            if (range <= 1)
                return new[] { Vector2Int.zero };

            if (range == 3)
            {
                // 1x3 가로 패턴
                return new[]
                {
                    new Vector2Int(-1, 0),
                    Vector2Int.zero,
                    new Vector2Int(1, 0)
                };
            }

            if (range == 9)
            {
                // 3x3 패턴
                var pattern = new Vector2Int[9];
                int idx = 0;
                for (int y = -1; y <= 1; y++)
                    for (int x = -1; x <= 1; x++)
                        pattern[idx++] = new Vector2Int(x, y);
                return pattern;
            }

            // 범용 직선 패턴
            var linear = new Vector2Int[range];
            int half = range / 2;
            for (int i = 0; i < range; i++)
                linear[i] = new Vector2Int(i - half, 0);
            return linear;
        }
    }
}
