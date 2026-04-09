// EconomyManager — 골드/경제 시스템 매니저 (스텁)
// 완전 구현: economy-tasks.md (ARC-008) Phase D 이후 처리
// -> see docs/systems/economy-system.md
using System;
using UnityEngine;
using SeedMind.Core;

namespace SeedMind.Economy
{
    /// <summary>
    /// 플레이어 골드 및 상점 거래를 관리한다.
    /// 현재: ToolUpgradeSystem 의존을 위한 최소 구현.
    /// 완전 구현: economy-tasks.md (ARC-008) 에서 진행.
    /// -> see docs/systems/economy-system.md
    /// </summary>
    public class EconomyManager : Singleton<EconomyManager>
    {
        [SerializeField] private int _startingGold = 500; // -> see docs/systems/economy-system.md

        private int _currentGold;

        public int CurrentGold => _currentGold;

        public event Action<int, int> OnGoldChanged; // (oldGold, newGold)

        protected override void Awake()
        {
            base.Awake();
            _currentGold = _startingGold;
        }

        /// <summary>
        /// 골드를 지불한다. 잔액 부족 시 false 반환.
        /// </summary>
        public bool TrySpendGold(int amount)
        {
            if (_currentGold < amount)
            {
                Debug.Log($"[EconomyManager] 골드 부족: 보유 {_currentGold}G, 필요 {amount}G");
                return false;
            }
            int old = _currentGold;
            _currentGold -= amount;
            OnGoldChanged?.Invoke(old, _currentGold);
            Debug.Log($"[EconomyManager] 골드 차감: -{amount}G → 잔액 {_currentGold}G");
            return true;
        }

        /// <summary>
        /// 골드를 획득한다.
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            int old = _currentGold;
            _currentGold += amount;
            OnGoldChanged?.Invoke(old, _currentGold);
            Debug.Log($"[EconomyManager] 골드 획득: +{amount}G → 잔액 {_currentGold}G");
        }
    }
}
