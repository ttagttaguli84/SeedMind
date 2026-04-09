// S-03: 시간 진행 관리 — Singleton MonoBehaviour
// -> see docs/systems/time-season-architecture.md 섹션 2.1, 3, 4
using System;
using System.Collections.Generic;
using UnityEngine;
using SeedMind.Save;

namespace SeedMind.Core
{
    public class TimeManager : Singleton<TimeManager>, ISaveable
    {
        // --- 직렬화 필드 ---
        [SerializeField] private int _currentYear = 1;
        [SerializeField] private Season _currentSeason = Season.Spring;
        [SerializeField] private int _currentDay = 1;
        [SerializeField] private float _currentHour = 6f;
        [SerializeField] private DayPhase _currentDayPhase = DayPhase.Dawn;
        [SerializeField] private float _timeScale = 1f;
        [SerializeField] private bool _isPaused;

        // --- 설정 참조 ---
        [SerializeField] private TimeConfig _timeConfig;
        [SerializeField] private SeasonData[] _seasonDataSet = new SeasonData[4];

        // --- 읽기 전용 프로퍼티 ---
        public int CurrentYear => _currentYear;
        public Season CurrentSeason => _currentSeason;
        public int CurrentDay => _currentDay;
        public float CurrentHour => _currentHour;
        public DayPhase CurrentDayPhase => _currentDayPhase;
        public bool IsPaused => _isPaused;
        public int DaysInSeason => _timeConfig != null ? _timeConfig.daysPerSeason : 28;
        public SeasonData CurrentSeasonData =>
            _seasonDataSet != null && (int)_currentSeason < _seasonDataSet.Length
                ? _seasonDataSet[(int)_currentSeason] : null;
        public int TotalElapsedDays =>
            (_currentYear - 1) * (_timeConfig != null ? _timeConfig.DaysPerYear : 112)
            + (int)_currentSeason * DaysInSeason + _currentDay;

        // --- 우선순위 기반 이벤트 디스패처 ---
        // List<(priority, callback)> — SortedList 키 충돌 방지
        // -> see docs/systems/time-season-architecture.md 섹션 4.2
        private readonly List<(int priority, Action<int> cb)> _dayCallbacks = new();
        private readonly List<(int priority, Action<Season> cb)> _seasonCallbacks = new();

        // --- 단순 이벤트 (우선순위 불필요) ---
        public event Action<int> OnHourChanged;
        public event Action<DayPhase> OnDayPhaseChanged;
        public event Action<int> OnYearChanged;
        public event Action OnSleepCompleted;

        // --- ISaveable ---
        public int SaveLoadOrder => 0; // TimeManager 최우선 로드

        // --- 우선순위 이벤트 등록 ---

        /// <summary>
        /// OnDayChanged 우선순위 구독. priority 낮을수록 먼저 실행 (0 = 최우선).
        /// 중복 priority는 LogError 후 무시.
        /// </summary>
        public void RegisterOnDayChanged(int priority, Action<int> callback)
        {
            foreach (var e in _dayCallbacks)
            {
                if (e.priority == priority)
                {
                    Debug.LogError($"[TimeManager] OnDayChanged priority {priority} 중복 등록 시도 무시.");
                    return;
                }
            }
            _dayCallbacks.Add((priority, callback));
            _dayCallbacks.Sort((a, b) => a.priority.CompareTo(b.priority));
        }

        public void UnregisterOnDayChanged(Action<int> callback)
        {
            _dayCallbacks.RemoveAll(e => e.cb == callback);
        }

        public void RegisterOnSeasonChanged(int priority, Action<Season> callback)
        {
            foreach (var e in _seasonCallbacks)
            {
                if (e.priority == priority)
                {
                    Debug.LogError($"[TimeManager] OnSeasonChanged priority {priority} 중복 등록 시도 무시.");
                    return;
                }
            }
            _seasonCallbacks.Add((priority, callback));
            _seasonCallbacks.Sort((a, b) => a.priority.CompareTo(b.priority));
        }

        public void UnregisterOnSeasonChanged(Action<Season> callback)
        {
            _seasonCallbacks.RemoveAll(e => e.cb == callback);
        }

        // --- Update 루프 ---

        private void Update()
        {
            if (_isPaused || _timeConfig == null) return;

            float hoursToAdd = Time.deltaTime * _timeScale / _timeConfig.secondsPerGameHour;
            float prevHour = _currentHour;
            _currentHour += hoursToAdd;

            int prevHourInt = Mathf.FloorToInt(prevHour);
            int currHourInt = Mathf.FloorToInt(_currentHour);
            if (currHourInt > prevHourInt)
            {
                for (int h = prevHourInt + 1; h <= currHourInt; h++)
                    OnHourChanged?.Invoke(h);
                UpdateDayPhase();
            }

            if (_currentHour >= _timeConfig.dayEndHour)
            {
                _currentHour = _timeConfig.dayEndHour;
                AdvanceDay();
            }
        }

        private void UpdateDayPhase()
        {
            int h = Mathf.FloorToInt(_currentHour);
            DayPhase newPhase;
            if (h < 8)       newPhase = DayPhase.Dawn;
            else if (h < 12) newPhase = DayPhase.Morning;
            else if (h < 17) newPhase = DayPhase.Afternoon;
            else if (h < 20) newPhase = DayPhase.Evening;
            else             newPhase = DayPhase.Night;

            if (newPhase != _currentDayPhase)
            {
                _currentDayPhase = newPhase;
                OnDayPhaseChanged?.Invoke(newPhase);
            }
        }

        private void AdvanceDay()
        {
            _currentDay++;
            _currentHour = _timeConfig != null ? _timeConfig.dayStartHour : 6f;

            if (_currentDay > DaysInSeason)
            {
                _currentDay = 1;
                AdvanceSeason();
            }
            FireOnDayChanged(_currentDay);
        }

        private void AdvanceSeason()
        {
            _currentSeason = (Season)(((int)_currentSeason + 1) % 4);
            if (_currentSeason == Season.Spring)
            {
                _currentYear++;
                OnYearChanged?.Invoke(_currentYear);
            }
            FireOnSeasonChanged(_currentSeason);
        }

        private void FireOnDayChanged(int day)
        {
            foreach (var e in _dayCallbacks)
                e.cb.Invoke(day);
        }

        private void FireOnSeasonChanged(Season season)
        {
            foreach (var e in _seasonCallbacks)
                e.cb.Invoke(season);
        }

        // --- 공개 메서드 ---

        public void SetTimeScale(float scale)
        {
            _timeScale = Mathf.Clamp(scale, 0f, _timeConfig != null ? _timeConfig.maxTimeScale : 3f);
        }

        public void Pause() => _isPaused = true;
        public void Resume() => _isPaused = false;

        /// <summary>수면/디버그용 다음 날로 건너뜀. OnSleepCompleted → AdvanceDay 순서.</summary>
        public void SkipToNextDay()
        {
            OnSleepCompleted?.Invoke();
            if (_timeConfig != null)
                _currentHour = _timeConfig.dayEndHour;
            else
                _currentHour = 24f;
            // Update 루프가 다음 프레임에 AdvanceDay를 자동 호출
        }

        // --- ISaveable ---

        public object GetSaveData()
        {
            return new TimeSaveData
            {
                year = _currentYear,
                seasonIndex = (int)_currentSeason,
                day = _currentDay,
                hour = _currentHour,
                dayPhaseIndex = (int)_currentDayPhase,
                timeScale = _timeScale
            };
        }

        public void LoadSaveData(object data)
        {
            if (data is not TimeSaveData d) return;
            _currentYear = d.year;
            _currentSeason = (Season)d.seasonIndex;
            _currentDay = d.day;
            _currentHour = d.hour;
            _currentDayPhase = (DayPhase)d.dayPhaseIndex;
            _timeScale = d.timeScale;
            Debug.Log($"[TimeManager] 로드 완료: Year={_currentYear}, Season={_currentSeason}, Day={_currentDay}, Hour={_currentHour:F1}");
        }
    }
}
