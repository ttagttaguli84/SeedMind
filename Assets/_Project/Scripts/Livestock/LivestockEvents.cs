// LivestockEvents — 목축 시스템 정적 이벤트 버스
// -> see docs/systems/livestock-architecture.md 섹션 10
using System;

namespace SeedMind.Livestock
{
    public static class LivestockEvents
    {
        // 동물 구매
        public static Action<AnimalInstance> OnAnimalPurchased;

        // 돌봄 이벤트
        public static Action<AnimalInstance> OnAnimalFed;
        public static Action<AnimalInstance> OnAnimalPetted;
        public static Action<AnimalInstance> OnAnimalSad;   // 행복도 임계값 이하

        // 생산물 이벤트
        public static Action<AnimalInstance> OnProductReady;
        public static Action<AnimalInstance, AnimalProductInfo> OnProductCollected;

        // 시설 이벤트
        public static Action<int> OnBarnUpgraded;   // newLevel
        public static Action<int> OnCoopUpgraded;   // newLevel

        // Raise 헬퍼
        public static void RaiseAnimalPurchased(AnimalInstance a)                       => OnAnimalPurchased?.Invoke(a);
        public static void RaiseAnimalFed(AnimalInstance a)                             => OnAnimalFed?.Invoke(a);
        public static void RaiseAnimalPetted(AnimalInstance a)                          => OnAnimalPetted?.Invoke(a);
        public static void RaiseAnimalSad(AnimalInstance a)                             => OnAnimalSad?.Invoke(a);
        public static void RaiseProductReady(AnimalInstance a)                          => OnProductReady?.Invoke(a);
        public static void RaiseProductCollected(AnimalInstance a, AnimalProductInfo p) => OnProductCollected?.Invoke(a, p);
        public static void RaiseBarnUpgraded(int level)                                 => OnBarnUpgraded?.Invoke(level);
        public static void RaiseCoopUpgraded(int level)                                 => OnCoopUpgraded?.Invoke(level);
    }
}
