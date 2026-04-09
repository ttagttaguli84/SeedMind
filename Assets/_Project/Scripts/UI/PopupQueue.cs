// S-09: 팝업 우선순위 큐
// -> see docs/systems/ui-architecture.md 섹션 1.5
using System.Collections.Generic;

namespace SeedMind.UI
{
    /// <summary>
    /// PopupPriority 기반 정렬 큐. UIManager 내부에서 사용.
    /// </summary>
    public class PopupQueue
    {
        private readonly List<PopupRequest> _queue = new List<PopupRequest>();

        public int Count => _queue.Count;
        public bool IsEmpty => _queue.Count == 0;

        public void Enqueue(PopupBase popup, PopupPriority priority)
        {
            var req = new PopupRequest
            {
                Popup = popup,
                Priority = priority,
                Timestamp = UnityEngine.Time.unscaledTime
            };
            _queue.Add(req);
            // 우선순위 내림차순, 동순위는 Timestamp 오름차순
            _queue.Sort((a, b) =>
            {
                int cmp = b.Priority.CompareTo(a.Priority);
                if (cmp != 0) return cmp;
                return a.Timestamp.CompareTo(b.Timestamp);
            });
        }

        public PopupRequest? Dequeue()
        {
            if (_queue.Count == 0) return null;
            var req = _queue[0];
            _queue.RemoveAt(0);
            return req;
        }

        public PopupRequest? Peek()
        {
            if (_queue.Count == 0) return null;
            return _queue[0];
        }

        public void Clear() => _queue.Clear();
    }

    public struct PopupRequest
    {
        public PopupBase Popup;
        public PopupPriority Priority;
        public float Timestamp;    // Time.unscaledTime
    }
}
