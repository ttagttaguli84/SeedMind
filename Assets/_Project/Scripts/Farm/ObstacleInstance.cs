// 장애물의 런타임 상태 (Plain C# class)
// -> see docs/systems/farm-expansion-architecture.md 섹션 1
using UnityEngine;

namespace SeedMind.Farm
{
    public class ObstacleInstance
    {
        public ObstacleEntry entry;     // 원본 정적 데이터 참조
        public Vector2Int position;     // FarmGrid 절대 좌표
        public int currentHP;           // 남은 HP
        public bool isCleared;          // 제거 여부
        public bool droppedLoot;        // 드랍 처리 완료 여부

        public ObstacleInstance(ObstacleEntry entry, Vector2Int position)
        {
            this.entry = entry;
            this.position = position;
            this.currentHP = entry.maxHP;
            this.isCleared = false;
            this.droppedLoot = false;
        }
    }
}
