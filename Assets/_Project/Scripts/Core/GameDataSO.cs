using UnityEngine;

namespace SeedMind.Core
{
    /// <summary>
    /// 모든 게임 데이터 ScriptableObject의 베이스 클래스.
    /// ID 기반 참조 복원과 에디터 검증 인터페이스를 제공한다.
    /// -> see docs/pipeline/data-pipeline.md Part II 섹션 1
    /// </summary>
    public abstract class GameDataSO : ScriptableObject
    {
        [Header("식별")]
        public string dataId;       // 고유 식별자 (예: "crop_potato", "hoe_t1")
        public string displayName;  // UI 표시명 (한국어, 예: "감자", "호미")

        [Header("메타")]
        public Sprite icon;         // UI 아이콘 (nullable)

        /// <summary>
        /// Editor-time 유효성 검증. 하위 클래스에서 오버라이드하여
        /// 필드 검증 로직을 추가한다.
        /// </summary>
        public virtual bool Validate(out string errorMessage)
        {
            if (string.IsNullOrEmpty(dataId))
            {
                errorMessage = $"{name}: dataId가 비어 있습니다.";
                return false;
            }
            errorMessage = null;
            return true;
        }
    }
}
