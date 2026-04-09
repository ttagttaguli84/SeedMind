// FishingConfig ScriptableObject — 낚시 미니게임 전역 밸런스 설정
// -> see docs/systems/fishing-architecture.md 섹션 1
// -> see docs/systems/fishing-system.md 섹션 2.2, 2.4, 3.2, 3.3 for canonical 값
using UnityEngine;

namespace SeedMind.Fishing.Data
{
    [CreateAssetMenu(fileName = "SO_FishingConfig", menuName = "SeedMind/Fishing/FishingConfig")]
    public class FishingConfig : ScriptableObject
    {
        [Header("캐스팅 (섹션 2.4)")]
        // 캐스팅 애니메이션 소요 시간 범위 (초)
        // -> see docs/systems/fishing-system.md 섹션 2.4
        public Vector2 castDurationRange = new Vector2(1.5f, 3.0f);

        [Header("입질 (섹션 2.2)")]
        // 입질 대기 시간 범위 (초)
        // -> see docs/systems/fishing-system.md 섹션 2.2
        public Vector2 biteDelayRange = new Vector2(3f, 15f);

        // 입질 반응 창 지속 시간 (초)
        // -> see docs/systems/fishing-system.md 섹션 2.2
        public float biteWindowDuration = 1.0f;

        [Header("릴링 미니게임 (섹션 3.3)")]
        // 릴링 최대 시간 (초) — 기본은 Common 기준; 물고기별 moveSpeed로 조정
        // -> see docs/systems/fishing-system.md 섹션 3.3
        public float reelingDuration = 15f;

        // 흥분 게이지 감소율 (초당) — Common 기준
        // -> see docs/systems/fishing-system.md 섹션 3.3
        public float excitementDecayRate = 0.08f;

        // 입력당 흥분 게이지 증가량 — Common 기준
        // -> see docs/systems/fishing-system.md 섹션 3.3
        public float excitementGainPerInput = 0.20f;

        // 성공 임계값 (흥분 게이지 0~1)
        // -> see docs/systems/fishing-system.md 섹션 3.3
        [Range(0f, 1f)] public float successThreshold = 0.80f;

        // 실패 임계값 (흥분 게이지 0~1)
        // -> see docs/systems/fishing-system.md 섹션 3.3
        [Range(0f, 1f)] public float failThreshold = 0.00f;

        [Header("품질 임계값 (섹션 3.2)")]
        // [0]=Normal, [1]=Silver, [2]=Gold, [3]=Iridium 경계값 (흥분 게이지 기준)
        // -> see docs/systems/fishing-system.md 섹션 3.2
        public float[] qualityThresholds = new float[4] { 0f, 0.5f, 0.75f, 0.9f };
    }
}
