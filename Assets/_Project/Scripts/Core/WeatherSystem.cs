// S-09: 날씨 결정 및 게임 시스템 연동 — OnDayChanged priority 0
// -> see docs/systems/time-season-architecture.md 섹션 2.3, 5
using System;
using UnityEngine;
using SeedMind.Farm;
using SeedMind.Save;

namespace SeedMind.Core
{
    public class WeatherSystem : MonoBehaviour, ISaveable
    {
        [SerializeField] private WeatherData[] _weatherDataSet = new WeatherData[4];
        [SerializeField] private FarmGrid _farmGrid;

        private WeatherType _currentWeather = WeatherType.Clear;
        private WeatherType _tomorrowWeather = WeatherType.Clear;
        private int _weatherSeed;
        private System.Random _rng;
        private int _consecutiveSameWeatherDays;
        private int _totalElapsedDays;

        public WeatherType CurrentWeather => _currentWeather;
        public WeatherType TomorrowWeather => _tomorrowWeather;
        public bool IsRaining =>
            _currentWeather == WeatherType.Rain ||
            _currentWeather == WeatherType.HeavyRain ||
            _currentWeather == WeatherType.Storm;

        public event Action<WeatherType> OnWeatherChanged;

        // --- ISaveable ---
        public int SaveLoadOrder => 1;

        private void OnEnable()
        {
            _weatherSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            _rng = new System.Random(_weatherSeed);
            TimeManager.Instance?.RegisterOnDayChanged(0, ProcessDayWeather);
        }

        private void OnDisable()
        {
            TimeManager.Instance?.UnregisterOnDayChanged(ProcessDayWeather);
        }

        private void ProcessDayWeather(int newDay)
        {
            _currentWeather = _tomorrowWeather;
            OnWeatherChanged?.Invoke(_currentWeather);
            ApplyWeatherEffects();

            // 연속 카운트 갱신
            _consecutiveSameWeatherDays = (_currentWeather == _tomorrowWeather) ? _consecutiveSameWeatherDays + 1 : 1;
            _totalElapsedDays++;

            // 내일 날씨 결정
            Season season = TimeManager.Instance != null ? TimeManager.Instance.CurrentSeason : Season.Spring;
            _tomorrowWeather = DetermineWeather(season);

            Debug.Log($"[WeatherSystem] Day {newDay}: 오늘={_currentWeather}, 내일예보={_tomorrowWeather}");
        }

        private WeatherType DetermineWeather(Season season)
        {
            int idx = (int)season;
            if (_weatherDataSet == null || idx >= _weatherDataSet.Length || _weatherDataSet[idx] == null)
                return WeatherType.Clear;

            WeatherData data = _weatherDataSet[idx];
            float[] weights = {
                data.clearChance,
                data.cloudyChance,
                data.rainChance,
                data.heavyRainChance,
                data.stormChance,
                data.snowChance,
                data.blizzardChance
            };

            // 연속 보정
            if (_consecutiveSameWeatherDays >= data.maxConsecutiveSameWeatherDays)
            {
                int currentIdx = (int)_currentWeather;
                if (currentIdx < weights.Length)
                    weights[currentIdx] *= data.consecutivePenalty;
            }
            bool isExtreme = _currentWeather == WeatherType.Storm || _currentWeather == WeatherType.Blizzard;
            if (isExtreme && _consecutiveSameWeatherDays >= data.maxConsecutiveExtremeWeatherDays)
            {
                int currentIdx = (int)_currentWeather;
                if (currentIdx < weights.Length)
                    weights[currentIdx] = 0f;
            }

            // Weighted random
            float total = 0f;
            foreach (float w in weights) total += w;
            if (total <= 0f) return WeatherType.Clear;

            float roll = (float)_rng.NextDouble() * total;
            float cumulative = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                    return (WeatherType)i;
            }
            return WeatherType.Clear;
        }

        public void ApplyWeatherEffects()
        {
            if (IsRaining && _farmGrid != null)
                _farmGrid.WaterAllPlantedTiles();
        }

        public void SetWeatherSeed(int seed)
        {
            _weatherSeed = seed;
            _rng = new System.Random(seed);
        }

        // --- ISaveable ---

        public object GetSaveData()
        {
            return new WeatherSaveData
            {
                weatherSeed = _weatherSeed,
                currentWeatherIndex = (int)_currentWeather,
                tomorrowWeatherIndex = (int)_tomorrowWeather,
                consecutiveSameWeatherDays = _consecutiveSameWeatherDays,
                totalElapsedDays = _totalElapsedDays
            };
        }

        public void LoadSaveData(object data)
        {
            if (data is not WeatherSaveData d) return;

            _weatherSeed = d.weatherSeed;
            _consecutiveSameWeatherDays = d.consecutiveSameWeatherDays;
            _totalElapsedDays = d.totalElapsedDays;
            _currentWeather = (WeatherType)d.currentWeatherIndex;
            _tomorrowWeather = (WeatherType)d.tomorrowWeatherIndex;

            // 시드 기반 RNG 상태 재현
            _rng = new System.Random(_weatherSeed);
            for (int i = 0; i < _totalElapsedDays; i++)
                _rng.NextDouble();

            Debug.Log($"[WeatherSystem] 로드 완료: seed={_weatherSeed}, 현재={_currentWeather}, 내일={_tomorrowWeather}");
        }
    }
}
