// S-06: 조명/환경 전환 제어 — TimeManager 이벤트 구독
// -> see docs/systems/time-season-architecture.md 섹션 6
using System.Collections;
using UnityEngine;

namespace SeedMind.Core
{
    public class EnvironmentController : MonoBehaviour
    {
        [SerializeField] private Light _directionalLight;

        private Coroutine _transitionCoroutine;

        private void OnEnable()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayPhaseChanged += OnDayPhaseChanged;
                TimeManager.Instance.RegisterOnSeasonChanged(0, OnSeasonChanged);
            }
        }

        private void OnDisable()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayPhaseChanged -= OnDayPhaseChanged;
                TimeManager.Instance.UnregisterOnSeasonChanged(OnSeasonChanged);
            }
        }

        private void OnDayPhaseChanged(DayPhase newPhase)
        {
            SeasonData seasonData = TimeManager.Instance?.CurrentSeasonData;
            if (seasonData == null || seasonData.phaseOverrides == null) return;

            int idx = (int)newPhase;
            if (idx < 0 || idx >= seasonData.phaseOverrides.Length) return;

            DayPhaseVisual visual = seasonData.phaseOverrides[idx];
            if (visual == null) return;

            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = StartCoroutine(TransitionLighting(visual));
        }

        private IEnumerator TransitionLighting(DayPhaseVisual target)
        {
            if (_directionalLight == null) yield break;

            Color startColor = _directionalLight.color;
            float startIntensity = _directionalLight.intensity;
            Quaternion startRot = _directionalLight.transform.rotation;
            Quaternion targetRot = Quaternion.Euler(target.lightRotation);
            Color startAmbient = RenderSettings.ambientLight;

            float elapsed = 0f;
            float duration = Mathf.Max(target.transitionDuration, 0.01f);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                _directionalLight.color = Color.Lerp(startColor, target.lightColor, t);
                _directionalLight.intensity = Mathf.Lerp(startIntensity, target.lightIntensity, t);
                _directionalLight.transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
                RenderSettings.ambientLight = Color.Lerp(startAmbient, target.ambientColor, t);

                yield return null;
            }

            _directionalLight.color = target.lightColor;
            _directionalLight.intensity = target.lightIntensity;
            _directionalLight.transform.rotation = targetRot;
            RenderSettings.ambientLight = target.ambientColor;
        }

        private void OnSeasonChanged(Season newSeason)
        {
            SeasonData seasonData = TimeManager.Instance?.CurrentSeasonData;
            if (seasonData == null) return;

            // 안개 설정
            RenderSettings.fogColor = seasonData.fogColor;
            RenderSettings.fogDensity = seasonData.fogDensity;

            Debug.Log($"[EnvironmentController] 계절 전환: {newSeason} — 환경 설정 적용");
        }
    }
}
