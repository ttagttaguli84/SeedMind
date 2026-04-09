// S-11: 축제 날짜 판정 및 이벤트 발행 — OnDayChanged priority 30
// -> see docs/systems/time-season-architecture.md 섹션 2.5
// canonical 축제 목록: docs/systems/time-season.md 섹션 4.2
using System;
using UnityEngine;

namespace SeedMind.Core
{
    public class FestivalManager : MonoBehaviour
    {
        [SerializeField] private FestivalData[] _festivals;

        private FestivalData _activeFestival;

        public event Action<FestivalData> OnFestivalStarted;
        public event Action<FestivalData> OnFestivalEnded;

        private void OnEnable()
        {
            TimeManager.Instance?.RegisterOnDayChanged(30, OnDayChanged);
        }

        private void OnDisable()
        {
            TimeManager.Instance?.UnregisterOnDayChanged(OnDayChanged);
        }

        private void OnDayChanged(int newDay)
        {
            if (TimeManager.Instance == null) return;

            Season currentSeason = TimeManager.Instance.CurrentSeason;
            CheckFestival(currentSeason, newDay);
        }

        private void CheckFestival(Season season, int day)
        {
            // 이전 축제 종료 체크
            if (_activeFestival != null)
            {
                if (_activeFestival.season != season || _activeFestival.day != day)
                {
                    var ended = _activeFestival;
                    _activeFestival = null;
                    OnFestivalEnded?.Invoke(ended);
                    Debug.Log($"[FestivalManager] 축제 종료: {ended.festivalName}");
                }
            }

            // 새 축제 시작 체크
            if (_festivals == null) return;
            foreach (var festival in _festivals)
            {
                if (festival == null) continue;
                if (festival.season == season && festival.day == day)
                {
                    _activeFestival = festival;
                    OnFestivalStarted?.Invoke(festival);
                    Debug.Log($"[FestivalManager] 축제 시작: {festival.festivalName}");
                    break;
                }
            }
        }

        public FestivalData GetActiveFestival() => _activeFestival;

        public bool IsFestivalDay() => _activeFestival != null;
    }
}
