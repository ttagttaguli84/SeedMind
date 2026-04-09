// S-04: 토스트 알림 우선순위 열거형
// -> see docs/systems/ui-architecture.md 섹션 3.1
namespace SeedMind.UI
{
    public enum NotificationPriority
    {
        Low      = 0,   // 일반 정보 (예: "씨앗을 심었습니다")
        Normal   = 1,   // 일반 성과 (예: "감자 수확 x3")
        High     = 2,   // 중요 성과 (예: 퀘스트 완료, 레벨업)
        Critical = 3    // 긴급 알림 (예: 작물 고사 경고, 저장 실패)
    }
}
