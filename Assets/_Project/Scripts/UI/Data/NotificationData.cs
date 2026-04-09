// S-05: 알림 데이터 구조체
// -> see docs/systems/ui-architecture.md 섹션 3.3
using UnityEngine;

namespace SeedMind.UI
{
    public struct NotificationData
    {
        public string Message;                  // 표시할 텍스트
        public NotificationPriority Priority;   // 우선순위
        public Sprite Icon;                     // null이면 기본 아이콘
        public float Duration;                  // 0이면 우선순위별 기본값
        public Color Color;                     // 배경색 힌트 (선택)
    }
}
