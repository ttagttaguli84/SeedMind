// S-06: 알림 요청 구조체 (큐 내부 사용)
// -> see docs/systems/ui-architecture.md 섹션 3.3
namespace SeedMind.UI
{
    public struct NotificationRequest
    {
        public NotificationData Data;           // 알림 데이터
        public float Timestamp;                 // Time.unscaledTime
        public int CompareKey;                  // Priority * 10000 - Timestamp (PQ 정렬키)
    }
}
