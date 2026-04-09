// S-05: 게임 이벤트를 감지하여 자동저장을 요청하는 컴포넌트
// -> see docs/systems/save-load-architecture.md 섹션 3
namespace SeedMind.Save
{
    using UnityEngine;

    public class AutoSaveTrigger : MonoBehaviour
    {
        // -> see docs/systems/save-load-architecture.md 섹션 3.3 for 쿨다운 파라미터
        [SerializeField] private float saveCooldownSeconds = 60f;

        private float _lastSaveTime;
        private bool _pendingSave;

        // 이벤트 구독은 TimeManager/BuildingEvents 구현 후 T-6에서 활성화
        private void OnEnable()
        {
            // TimeManager.OnDayChanged += OnDayChanged;
            // TimeManager.OnSeasonChanged += OnSeasonChanged;
            // BuildingEvents.OnConstructionCompleted += OnFacilityBuilt;
        }

        private void OnDisable()
        {
            // TimeManager.OnDayChanged -= OnDayChanged;
            // TimeManager.OnSeasonChanged -= OnSeasonChanged;
            // BuildingEvents.OnConstructionCompleted -= OnFacilityBuilt;
        }

        private void OnDayChanged(int newDay) => RequestAutoSave("DayChanged");
        private void OnSeasonChanged() => RequestAutoSave("SeasonChanged");
        private void OnFacilityBuilt() => RequestAutoSave("FacilityBuilt");

        private void RequestAutoSave(string reason)
        {
            if (Time.realtimeSinceStartup - _lastSaveTime < saveCooldownSeconds)
            {
                _pendingSave = true;
                return;
            }
            ExecuteAutoSave(reason);
        }

        private void ExecuteAutoSave(string reason)
        {
            _lastSaveTime = Time.realtimeSinceStartup;
            _pendingSave = false;
            SaveEvents.RaiseAutoSaveTriggered(reason);
            SaveManager.Instance.AutoSaveAsync();
        }

        private void Update()
        {
            if (_pendingSave && Time.realtimeSinceStartup - _lastSaveTime >= saveCooldownSeconds)
                ExecuteAutoSave("Deferred");
        }
    }
}
