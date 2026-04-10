// T-07: 도구 업그레이드 시스템 (MonoBehaviour)
// -> see docs/systems/tool-upgrade-architecture.md 섹션 2, 5
using System.Collections.Generic;
using UnityEngine;
using SeedMind.Core;
using SeedMind.Economy;
using SeedMind.Player.Data;
using SeedMind.Level;

namespace SeedMind.Player
{
    /// <summary>
    /// 도구 업그레이드 검증, 실행, 대기 관리, 이벤트 발행을 담당하는 시스템.
    /// MANAGERS 계층 하위에 배치. InventoryManager, EconomyManager와 느슨하게 결합.
    /// -> see docs/systems/tool-upgrade-architecture.md 섹션 5
    /// </summary>
    public class ToolUpgradeSystem : MonoBehaviour
    {
        [Header("참조")]
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private EconomyManager _economyManager;

        [Header("도구 레지스트리")]
        [SerializeField] private ToolData[] _toolRegistry; // 전체 ToolData SO 배열

        // 진행 중인 업그레이드: ToolType → PendingUpgrade
        private readonly Dictionary<ToolType, PendingUpgrade> _pendingUpgrades
            = new Dictionary<ToolType, PendingUpgrade>();

        // ── 이벤트 구독 ────────────────────────────────────────────
        private void OnEnable()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.RegisterOnDayChanged(51, OnDayChanged);
        }

        private void OnDisable()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.UnregisterOnDayChanged(OnDayChanged);
        }

        private void OnDayChanged(int day) => ProcessUpgradeTimers();

        // ── 공개 API ────────────────────────────────────────────────

        /// <summary>
        /// 업그레이드 가능 여부와 비용 정보를 반환한다.
        /// -> see docs/systems/tool-upgrade-architecture.md 섹션 5.1
        /// </summary>
        public UpgradeCheckResult CanUpgrade(ToolData current)
        {
            var result = new UpgradeCheckResult();

            if (current == null || current.nextTier == null)
            {
                result.failReason = ToolUpgradeFailReason.AlreadyMaxTier;
                return result;
            }

            if (_pendingUpgrades.ContainsKey(current.toolType))
            {
                result.failReason = ToolUpgradeFailReason.AlreadyUpgrading;
                return result;
            }

            var cost = GetUpgradeCost(current);
            result.cost = cost;

            // 레벨 검증 (FIX-086: levelReqType 분기)
            // -> see docs/systems/tool-upgrade-architecture.md 섹션 5.1
            if (cost.levelReqType == LevelReqType.PlayerLevel)
            {
                int playerLevel = ProgressionManager.Instance != null
                    ? ProgressionManager.Instance.CurrentLevel : 1;
                if (playerLevel < cost.requiredLevel)
                {
                    result.failReason = ToolUpgradeFailReason.LevelTooLow;
                    return result;
                }
            }
            else
            {
                // GatheringMastery 등: 향후 구현 (gathering-tasks.md Phase F 이후)
                // result.failReason = ToolUpgradeFailReason.MasteryTooLow;
            }

            // 골드 검증
            if (_economyManager != null && _economyManager.CurrentGold < cost.goldCost)
            {
                result.failReason = ToolUpgradeFailReason.InsufficientGold;
                return result;
            }

            // 재료 검증 (인벤토리에 충분한 재료가 있는지)
            if (_inventoryManager != null && cost.materials != null)
            {
                foreach (var mat in cost.materials)
                {
                    if (mat == null) continue;
                    int owned = _inventoryManager.GetItemCount(mat.materialId);
                    if (owned < mat.quantity)
                    {
                        result.failReason = ToolUpgradeFailReason.InsufficientMaterials;
                        return result;
                    }
                }
            }

            result.canUpgrade = true;
            result.failReason = ToolUpgradeFailReason.None;
            return result;
        }

        /// <summary>
        /// 업그레이드를 시작한다. 골드/재료 차감 → 대기 등록 → 이벤트 발행.
        /// -> see docs/systems/tool-upgrade-architecture.md 섹션 5.1
        /// </summary>
        public bool StartUpgrade(ToolData current)
        {
            var check = CanUpgrade(current);
            if (!check.canUpgrade)
            {
                ToolUpgradeEvents.RaiseUpgradeFailed(check.failReason);
                return false;
            }

            var cost = check.cost;

            // 1) 골드 차감
            if (_economyManager != null && !_economyManager.TrySpendGold(cost.goldCost))
            {
                ToolUpgradeEvents.RaiseUpgradeFailed(ToolUpgradeFailReason.InsufficientGold);
                return false;
            }

            // 2) 재료 차감
            if (_inventoryManager != null && cost.materials != null)
            {
                foreach (var mat in cost.materials)
                {
                    if (mat == null) continue;
                    _inventoryManager.RemoveItem(mat.materialId, mat.quantity);
                }
            }

            // 3) 업그레이드 대기 등록
            var pending = new PendingUpgrade
            {
                toolType = current.toolType,
                currentToolId = current.dataId,
                targetToolId = current.nextTier.dataId,
                remainingDays = cost.timeDays,
                totalDays = cost.timeDays
            };
            _pendingUpgrades[current.toolType] = pending;

            // 4) 이벤트 발행
            ToolUpgradeEvents.RaiseUpgradeStarted(new ToolUpgradeInfo
            {
                toolType = current.toolType,
                previousTool = current,
                upgradedTool = current.nextTier,
                newTier = current.nextTier.tier
            });

            Debug.Log($"[ToolUpgradeSystem] {current.displayName} 업그레이드 시작. {cost.timeDays}일 소요.");
            return true;
        }

        /// <summary>
        /// 업그레이드를 완료한다. 도구 SO 교체 → 이벤트 발행.
        /// -> see docs/systems/tool-upgrade-architecture.md 섹션 5.1
        /// </summary>
        public void CompleteUpgrade(ToolType toolType)
        {
            if (!_pendingUpgrades.TryGetValue(toolType, out var pending)) return;

            // 1) DataRegistry에서 targetToolId → ToolData SO 조회
            ToolData targetTool = null;
            if (DataRegistry.Instance != null)
                targetTool = DataRegistry.Instance.Get<ToolData>(pending.targetToolId);

            // 2) InventoryManager 툴바에서 currentToolId 슬롯을 targetToolId로 교체
            if (_inventoryManager != null && targetTool != null)
            {
                var toolbarSlots = _inventoryManager.ToolbarSlots;
                for (int i = 0; i < toolbarSlots.Count; i++)
                {
                    if (!toolbarSlots[i].IsEmpty && toolbarSlots[i].itemId == pending.currentToolId)
                    {
                        _inventoryManager.SetToolbarItem(i, pending.targetToolId);
                        break;
                    }
                }
            }

            // 3) 대기 목록에서 제거
            _pendingUpgrades.Remove(toolType);

            // 4) 이벤트 발행
            ToolUpgradeEvents.RaiseUpgradeCompleted(new ToolUpgradeInfo
            {
                toolType = toolType,
                upgradedTool = targetTool,
                newTier = targetTool != null ? targetTool.tier : 0
            });

            Debug.Log($"[ToolUpgradeSystem] {toolType} 업그레이드 완료: → {pending.targetToolId}");
        }

        /// <summary>
        /// 업그레이드를 취소한다. 골드/재료는 환불하지 않는다.
        /// </summary>
        public bool CancelUpgrade(ToolType toolType)
        {
            if (!_pendingUpgrades.ContainsKey(toolType)) return false;
            _pendingUpgrades.Remove(toolType);
            Debug.Log($"[ToolUpgradeSystem] {toolType} 업그레이드 취소.");
            return true;
        }

        /// <summary>진행 중인 업그레이드 상태를 조회한다.</summary>
        public PendingUpgrade GetPendingUpgrade(ToolType toolType)
        {
            return _pendingUpgrades.TryGetValue(toolType, out var p) ? p : null;
        }

        /// <summary>
        /// 업그레이드 비용 정보를 반환한다. UI 표시에 사용.
        /// -> see docs/systems/tool-upgrade.md 섹션 2.1
        /// </summary>
        public UpgradeCostInfo GetUpgradeCost(ToolData current)
        {
            if (current == null || current.nextTier == null)
                return default;

            return new UpgradeCostInfo
            {
                goldCost = current.upgradeGoldCost,
                materials = current.upgradeMaterials,
                timeDays = current.upgradeTimeDays,
                levelReqType = current.levelReqType,
                requiredLevel = current.requiredLevel
            };
        }

        // ── 세이브/로드 ─────────────────────────────────────────────

        /// <summary>세이브 시 ToolUpgradeSystem 상태를 직렬화한다.</summary>
        public ToolUpgradeSaveData GetSaveData()
        {
            var entries = new PendingUpgradeSaveEntry[_pendingUpgrades.Count];
            int i = 0;
            foreach (var kv in _pendingUpgrades)
            {
                entries[i++] = new PendingUpgradeSaveEntry
                {
                    toolTypeIndex = (int)kv.Key,
                    currentToolId = kv.Value.currentToolId,
                    targetToolId = kv.Value.targetToolId,
                    remainingDays = kv.Value.remainingDays,
                    totalDays = kv.Value.totalDays
                };
            }
            return new ToolUpgradeSaveData { pendingUpgrades = entries };
        }

        /// <summary>로드 시 ToolUpgradeSystem 상태를 복원한다.</summary>
        public void LoadSaveData(ToolUpgradeSaveData data)
        {
            _pendingUpgrades.Clear();
            if (data?.pendingUpgrades == null) return;
            foreach (var entry in data.pendingUpgrades)
            {
                var toolType = (ToolType)entry.toolTypeIndex;
                _pendingUpgrades[toolType] = new PendingUpgrade
                {
                    toolType = toolType,
                    currentToolId = entry.currentToolId,
                    targetToolId = entry.targetToolId,
                    remainingDays = entry.remainingDays,
                    totalDays = entry.totalDays
                };
            }
        }

        // ── 내부 로직 ───────────────────────────────────────────────

        /// <summary>
        /// TimeManager.OnDayChanged 구독 핸들러.
        /// 매일 업그레이드 잔여 일수를 감소시킨다.
        /// -> see docs/systems/tool-upgrade-architecture.md 섹션 5.1
        /// </summary>
        private void ProcessUpgradeTimers()
        {
            var completed = new List<ToolType>();
            foreach (var kv in _pendingUpgrades)
            {
                kv.Value.remainingDays -= 1;
                if (kv.Value.remainingDays <= 0)
                    completed.Add(kv.Key);
            }
            foreach (var toolType in completed)
                CompleteUpgrade(toolType);
        }
    }
}
