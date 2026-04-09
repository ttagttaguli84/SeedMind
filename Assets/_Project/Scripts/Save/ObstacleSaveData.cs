// 장애물 상태 세이브 데이터
// -> see docs/systems/farm-expansion-architecture.md 섹션 9.1
// PATTERN-005: JSON 필드 4개(posX, posY, isCleared, currentHP) = C# 필드 4개 — 일치
using System;

namespace SeedMind.Save
{
    [Serializable]
    public class ObstacleSaveData
    {
        public int posX;        // 타일 X 좌표
        public int posY;        // 타일 Y 좌표
        public bool isCleared;  // 제거 여부
        public int currentHP;   // 남은 HP (isCleared=true이면 0)
    }
}
