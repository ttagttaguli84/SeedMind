// 장식 시스템 정적 이벤트 버스
// -> see docs/systems/decoration-architecture.md 섹션 2.7
// FarmEvents 패턴 사용 (event 키워드 없이 public static Action — CS0070 방지)
using System;
using UnityEngine;

namespace SeedMind.Decoration
{
    /// <summary>장식 배치 완료 정보</summary>
    public class DecorationPlacedInfo
    {
        public int instanceId;
        public string itemId;
        public Vector3Int cell;
    }

    /// <summary>장식 시스템 전역 이벤트 버스</summary>
    public static class DecorationEvents
    {
        public static Action<DecorationPlacedInfo> OnDecorationPlaced;
        public static Action<int> OnDecorationRemoved;  // instanceId
    }
}
