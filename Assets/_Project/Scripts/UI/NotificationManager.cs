// S-12: 토스트 알림 큐 관리자 (Singleton)
// -> see docs/systems/ui-architecture.md 섹션 3.2
using System.Collections.Generic;
using UnityEngine;

namespace SeedMind.UI
{
    /// <summary>
    /// 토스트 알림의 우선순위 큐와 동시 표시 수를 제어한다.
    /// </summary>
    public class NotificationManager : MonoBehaviour
    {
        public static NotificationManager Instance { get; private set; }

        // --- 설정 ---
        [SerializeField] private int _maxVisibleToasts = 3;
            // -> see docs/systems/ui-architecture.md 섹션 3.2
        [SerializeField] private float _defaultDuration = 3.0f;
            // -> see docs/systems/ui-architecture.md 섹션 3.2
        [SerializeField] private float _criticalDuration = 5.0f;
            // -> see docs/systems/ui-architecture.md 섹션 3.2
        [SerializeField] private float _slideInDuration = 0.25f;
            // -> see docs/systems/ui-architecture.md 섹션 3.2
        [SerializeField] private float _slideOutDuration = 0.2f;
            // -> see docs/systems/ui-architecture.md 섹션 3.2
        [SerializeField] private float _verticalSpacing = 8f;
            // -> see docs/systems/ui-architecture.md 섹션 3.2

        // --- 참조 ---
        [SerializeField] private GameObject _toastPrefab;
        [SerializeField] private Transform _toastContainer;

        // --- 상태 ---
        private readonly Queue<NotificationRequest> _pendingQueue = new Queue<NotificationRequest>();
        private readonly List<ToastUI> _activeToasts = new List<ToastUI>();
        private readonly Queue<ToastUI> _toastPool = new Queue<ToastUI>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // --- API ---
        public void ShowNotification(NotificationData data)
        {
            float ts = Time.unscaledTime;
            var req = new NotificationRequest
            {
                Data = data,
                Timestamp = ts,
                CompareKey = (int)data.Priority * 10000 - (int)(ts * 100)
            };
            _pendingQueue.Enqueue(req);
        }

        public void ShowNotification(string msg, NotificationPriority priority = NotificationPriority.Normal)
        {
            ShowNotification(new NotificationData
            {
                Message = msg,
                Priority = priority,
                Duration = priority == NotificationPriority.Critical ? _criticalDuration : _defaultDuration
            });
        }

        public void ClearAll()
        {
            _pendingQueue.Clear();
            foreach (var toast in _activeToasts)
                toast.ForceHide();
            _activeToasts.Clear();
            UIEvents.RaiseAllNotificationsCleared();
        }

        // --- 내부 메서드 ---
        private void Update()
        {
            ProcessQueue();
        }

        private void ProcessQueue()
        {
            while (_pendingQueue.Count > 0 && _activeToasts.Count < _maxVisibleToasts)
            {
                var req = _pendingQueue.Dequeue();
                SpawnToast(req);
            }

            for (int i = _activeToasts.Count - 1; i >= 0; i--)
            {
                if (!_activeToasts[i].Tick(Time.unscaledDeltaTime))
                {
                    RetireToast(_activeToasts[i]);
                    _activeToasts.RemoveAt(i);
                }
            }
        }

        private ToastUI SpawnToast(NotificationRequest req)
        {
            ToastUI toast;
            if (_toastPool.Count > 0)
            {
                toast = _toastPool.Dequeue();
                toast.gameObject.SetActive(true);
            }
            else if (_toastPrefab != null)
            {
                var go = Instantiate(_toastPrefab, _toastContainer);
                toast = go.GetComponent<ToastUI>();
            }
            else return null;

            toast.Setup(req.Data);
            _activeToasts.Add(toast);
            UIEvents.RaiseNotificationShown(req.Data);
            return toast;
        }

        private void RetireToast(ToastUI toast)
        {
            toast.gameObject.SetActive(false);
            _toastPool.Enqueue(toast);
        }
    }
}
