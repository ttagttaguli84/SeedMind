using UnityEngine;
using SeedMind.Core;

namespace SeedMind.Audio
{
    /// <summary>
    /// 계절/날씨/시간/장소 상태에 따라 BGM을 자동 결정한다.
    /// 우선순위: Forced > Location > Weather > Time > Season
    /// </summary>
    public class BGMScheduler
    {
        private SoundManager _soundManager;
        private BGMTrack _forcedTrack = BGMTrack.None;
        private BGMTrack _locationTrack = BGMTrack.None;
        private BGMTrack _weatherTrack = BGMTrack.None;
        private BGMTrack _timeTrack = BGMTrack.None;
        private BGMTrack _seasonTrack = BGMTrack.Spring;

        // BGM crossfade 시간 -> see docs/systems/sound-design.md 섹션 1.4
        private const float DefaultFadeDuration = 1.5f;

        public void Initialize(SoundManager soundManager)
        {
            _soundManager = soundManager;
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.RegisterOnSeasonChanged(90, OnSeasonChanged);
                TimeManager.Instance.OnDayPhaseChanged += OnDayPhaseChanged;
                // 초기 계절 적용
                _seasonTrack = SeasonToTrack(TimeManager.Instance.CurrentSeason);
                EvaluateAndApply();
            }
        }

        public void Shutdown()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.UnregisterOnSeasonChanged(OnSeasonChanged);
                TimeManager.Instance.OnDayPhaseChanged -= OnDayPhaseChanged;
            }
        }

        private void OnSeasonChanged(Season season)
        {
            _seasonTrack = SeasonToTrack(season);
            EvaluateAndApply();
        }

        private void OnDayPhaseChanged(DayPhase phase)
        {
            _timeTrack = phase == DayPhase.Night ? BGMTrack.NightTime : BGMTrack.None;
            EvaluateAndApply();
        }

        public void OnWeatherChanged(WeatherType weather)
        {
            _weatherTrack = weather switch
            {
                WeatherType.Storm    => BGMTrack.Storm,
                WeatherType.Blizzard => BGMTrack.Storm,
                WeatherType.Rain     => BGMTrack.Rain,
                WeatherType.HeavyRain => BGMTrack.Rain,
                _ => BGMTrack.None
            };
            EvaluateAndApply();
        }

        public void ForceTrack(BGMTrack track)
        {
            _forcedTrack = track;
            EvaluateAndApply();
        }

        public void SetLocationTrack(BGMTrack track)
        {
            _locationTrack = track;
            EvaluateAndApply();
        }

        private void EvaluateAndApply()
        {
            var resolved = ResolveTrack();
            Debug.Log($"[BGMScheduler] Resolved={resolved}");
            if (resolved != _soundManager.CurrentBGM)
                _soundManager.CrossfadeBGM(resolved, DefaultFadeDuration);
        }

        private BGMTrack ResolveTrack()
        {
            if (_forcedTrack   != BGMTrack.None) return _forcedTrack;
            if (_locationTrack != BGMTrack.None) return _locationTrack;
            if (_weatherTrack  != BGMTrack.None) return _weatherTrack;
            if (_timeTrack     != BGMTrack.None) return _timeTrack;
            if (_seasonTrack   != BGMTrack.None) return _seasonTrack;
            return BGMTrack.Spring;
        }

        private static BGMTrack SeasonToTrack(Season season) => season switch
        {
            Season.Spring => BGMTrack.Spring,
            Season.Summer => BGMTrack.Summer,
            Season.Autumn => BGMTrack.Autumn,
            Season.Winter => BGMTrack.Winter,
            _ => BGMTrack.Spring
        };
    }
}
