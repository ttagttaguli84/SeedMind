// 채집 포인트 MonoBehaviour — 씬 배치 컴포넌트
// -> see docs/systems/gathering-architecture.md 섹션 3
using UnityEngine;

namespace SeedMind.Gathering
{
    /// <summary>
    /// 씬에 배치되는 채집 포인트.
    /// 활성/비활성 상태는 GatheringManager가 중앙 관리한다.
    /// </summary>
    public class GatheringPoint : MonoBehaviour
    {
        [Header("데이터")]
        [SerializeField] private GatheringPointData _pointData;
        public Vector2Int tilePosition;

        // ── 상태 ────────────────────────────────────────────────────
        private bool _isActive = true;
        private GameObject _activeVisual;
        private GameObject _depletedVisual;

        // ── 읽기 전용 프로퍼티 ───────────────────────────────────────
        public GatheringPointData PointData => _pointData;
        public string PointId   => _pointData != null ? _pointData.pointId : string.Empty;
        public bool IsActive    => _isActive;
        public Vector2Int TilePosition => tilePosition;

        private void Awake()
        {
            // 프리팹 기반 비주얼 생성 (설정 시)
            if (_pointData != null)
            {
                if (_pointData.pointPrefab != null)
                    _activeVisual = Instantiate(_pointData.pointPrefab, transform);
                if (_pointData.depletedPrefab != null)
                {
                    _depletedVisual = Instantiate(_pointData.depletedPrefab, transform);
                    _depletedVisual.SetActive(false);
                }
            }
        }

        /// <summary>
        /// GatheringManager에서 호출. 활성/비활성 상태를 전환한다.
        /// </summary>
        public void SetActive(bool active)
        {
            _isActive = active;
            if (_activeVisual != null) _activeVisual.SetActive(active);
            if (_depletedVisual != null) _depletedVisual.SetActive(!active);
        }

        /// <summary>채집 완료 시 VFX 재생.</summary>
        public void PlayGatherEffect()
        {
            if (_pointData?.gatherVFX != null)
            {
                var vfx = Instantiate(_pointData.gatherVFX, transform.position, Quaternion.identity);
                Destroy(vfx, 3f);
            }
        }

        /// <summary>UI 표시용 인터랙션 프롬프트 반환.</summary>
        public string GetInteractionPrompt()
        {
            if (!_isActive) return string.Empty;
            return _pointData != null ? $"[E] {_pointData.displayName} 채집" : "[E] 채집";
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = _isActive ? Color.green : Color.gray;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            if (_pointData != null)
            {
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.7f,
                    _pointData.pointId);
            }
        }
#endif
    }
}
