// 채집 아이템 카테고리 열거형
// -> see docs/systems/gathering-architecture.md 섹션 2.2
namespace SeedMind.Gathering
{
    public enum GatheringCategory
    {
        Flower,     // 꽃 — 선물, 장식, 일부 가공 재료
        Berry,      // 열매 — 식재료, 소모품 가공
        Mushroom,   // 버섯 — 요리 재료, 비 오는 날 출현 증가
        Herb,       // 허브 — 약품/향신료 가공
        Mineral,    // 광물/보석 — 희귀, 높은 판매가
        Special     // 특수 — 퀘스트/축제 관련 한정 아이템
    }
}
