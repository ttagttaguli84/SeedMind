// FarmZoneManager — 농장 구역 해금/상태 관리, 개간 처리, 세이브/로드
// -> see docs/systems/farm-expansion-architecture.md 섹션 1, 4.2, 5.3, 9
using System.Collections.Generic;
using UnityEngine;
using SeedMind.Core;
using SeedMind.Economy;
using SeedMind.Level;
using SeedMind.Player.Data;
using SeedMind.Save;

namespace SeedMind.Farm
{
    public class FarmZoneManager : Singleton<FarmZoneManager>, ISaveable
    {
        // --- Inspector 필드 ---
        [SerializeField] private ZoneData[] _zones;
        [SerializeField] private FarmGrid _farmGrid;

        // --- 런타임 상태 ---
        private Dictionary<string, ZoneRuntimeState> _zoneStates
            = new Dictionary<string, ZoneRuntimeState>();
        private Dictionary<Vector2Int, ObstacleInstance> _obstacleInstances
            = new Dictionary<Vector2Int, ObstacleInstance>();

        // ISaveable
        public int SaveLoadOrder => 45; // -> see docs/systems/farm-expansion-architecture.md 섹션 9.3

        // -------------------------------------------------------------------
        // 초기화
        // -------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            Initialize();
        }

        public void Initialize()
        {
            _zoneStates.Clear();
            _obstacleInstances.Clear();

            if (_zones == null) return;

            foreach (var zone in _zones)
            {
                if (zone == null) continue;
                ZoneState initialState = DetermineInitialState(zone);
                var rts = new ZoneRuntimeState(zone.zoneId, initialState, zone.obstacleMap?.Length ?? 0);
                _zoneStates[zone.zoneId] = rts;

                // Zone A는 시작부터 해금 처리 (타일 활성화는 Load 이후 또는 여기서)
                if (initialState == ZoneState.Unlocked || initialState == ZoneState.FullyCleared)
                {
                    if (_farmGrid != null && zone.tilePositions != null)
                        _farmGrid.ActivateZoneTiles(zone.tilePositions);
                    SpawnObstacles(zone);
                }
            }

            Debug.Log($"[FarmZoneManager] initialized, zones={_zones.Length}");
        }

        private ZoneState DetermineInitialState(ZoneData zone)
        {
            // 선행 구역 없고 비용도 없으면 → 시작 구역 (Zone A)
            if (string.IsNullOrEmpty(zone.prerequisiteZoneId) && zone.unlockCost == 0 && zone.requiredLevel == 0)
                return ZoneState.Unlocked;

            // 선행 구역 미해금 시 Locked
            if (!string.IsNullOrEmpty(zone.prerequisiteZoneId))
            {
                if (!_zoneStates.ContainsKey(zone.prerequisiteZoneId) ||
                    _zoneStates[zone.prerequisiteZoneId].state == ZoneState.Locked)
                    return ZoneState.Locked;
            }

            return ZoneState.Locked; // 기본값: 잠금 (해금은 TryUnlockZone으로만)
        }

        // -------------------------------------------------------------------
        // 구역 해금
        // -------------------------------------------------------------------

        /// <summary>구역 해금 시도. 성공 시 true 반환.</summary>
        public bool TryUnlockZone(string zoneId)
        {
            if (!_zoneStates.TryGetValue(zoneId, out var rts))
            {
                Debug.LogWarning($"[FarmZoneManager] Unknown zoneId: {zoneId}");
                return false;
            }

            if (rts.state == ZoneState.Unlocked || rts.state == ZoneState.FullyCleared)
            {
                ZoneEvents.OnZoneUnlockFailed?.Invoke(zoneId, ZoneUnlockFailReason.AlreadyUnlocked);
                return false;
            }

            var zone = FindZoneData(zoneId);
            if (zone == null) return false;

            // 선행 구역 체크
            if (!string.IsNullOrEmpty(zone.prerequisiteZoneId) && !IsZoneUnlocked(zone.prerequisiteZoneId))
            {
                ZoneEvents.OnZoneUnlockFailed?.Invoke(zoneId, ZoneUnlockFailReason.PrerequisiteZone);
                return false;
            }

            // 레벨 체크
            var pm = ProgressionManager.Instance;
            if (pm != null && zone.requiredLevel > 0 && pm.CurrentLevel < zone.requiredLevel)
            {
                ZoneEvents.OnZoneUnlockFailed?.Invoke(zoneId, ZoneUnlockFailReason.LevelInsufficient);
                return false;
            }

            // 골드 체크 및 차감
            var em = EconomyManager.Instance;
            if (zone.unlockCost > 0)
            {
                if (em == null || !em.TrySpendGold(zone.unlockCost))
                {
                    ZoneEvents.OnZoneUnlockFailed?.Invoke(zoneId, ZoneUnlockFailReason.InsufficientGold);
                    return false;
                }
            }

            // 해금 처리
            rts.state = ZoneState.Unlocked;

            if (_farmGrid != null && zone.tilePositions != null)
                _farmGrid.ActivateZoneTiles(zone.tilePositions);

            SpawnObstacles(zone);

            int tileCount = zone.tilePositions?.Length ?? 0;
            Debug.Log($"[FarmZoneManager] Zone {zoneId} unlocked, activated {tileCount} tiles");

            ZoneEvents.OnZoneUnlocked?.Invoke(zoneId, zone);
            return true;
        }

        // -------------------------------------------------------------------
        // 장애물 개간
        // -------------------------------------------------------------------

        /// <summary>해당 타일의 장애물을 도구로 타격. ClearResult 반환.</summary>
        public ClearResult ClearObstacle(Vector2Int tilePos, ToolType tool, int toolTier)
        {
            if (!_obstacleInstances.TryGetValue(tilePos, out var obs))
                return ClearResult.NoObstacle;

            if (obs.isCleared)
                return ClearResult.AlreadyCleared;

            if (!CanToolClear(obs.entry.type, tool, toolTier))
                return ClearResult.WrongTool;

            obs.currentHP--;
            Debug.Log($"[FarmZoneManager] Obstacle at {tilePos} hit, HP={obs.currentHP}/{obs.entry.maxHP}");

            if (obs.currentHP <= 0)
            {
                obs.isCleared = true;

                // 드랍 처리 (간단 로그, 실제 드랍은 추후 InventoryManager 연동)
                if (!obs.droppedLoot && obs.entry.lootDropIds != null)
                {
                    obs.droppedLoot = true;
                    Debug.Log($"[FarmZoneManager] Obstacle cleared at {tilePos}, drops: {string.Join(", ", obs.entry.lootDropIds)}");
                }

                ZoneEvents.OnObstacleCleared?.Invoke(tilePos, obs.entry.type);
                CheckZoneFullyCleared(FindZoneIdForTile(tilePos));
                return ClearResult.Cleared;
            }

            ZoneEvents.OnObstacleHit?.Invoke(tilePos, obs.currentHP);
            return ClearResult.Hit;
        }

        public bool CanToolClear(ObstacleType obstacle, ToolType tool, int toolTier)
        {
            // 잡초/덤불: 낫(Sickle) 필요
            if (obstacle == ObstacleType.Weed || obstacle == ObstacleType.Bush)
                return tool == ToolType.Sickle;

            // 대형 바위/대형 나무: Reinforced+(tier 2+) 호미 필요
            if (obstacle == ObstacleType.LargeRock || obstacle == ObstacleType.LargeTree)
                return tool == ToolType.Hoe && toolTier >= 2;

            // 그 외 (소형 돌, 그루터기, 소형 나무): 호미 Basic+(tier 1+) 가능
            return tool == ToolType.Hoe;
        }

        public void SpawnObstacles(ZoneData zone)
        {
            if (zone.obstacleMap == null) return;

            // Zone의 타일 오프셋 계산 (첫 번째 tilePosition 기준)
            Vector2Int offset = zone.tilePositions != null && zone.tilePositions.Length > 0
                ? zone.tilePositions[0]
                : Vector2Int.zero;

            foreach (var entry in zone.obstacleMap)
            {
                Vector2Int worldPos = offset + entry.localPosition;
                if (!_obstacleInstances.ContainsKey(worldPos))
                {
                    var instance = new ObstacleInstance(entry, worldPos);
                    _obstacleInstances[worldPos] = instance;
                }
            }
        }

        // -------------------------------------------------------------------
        // 조회
        // -------------------------------------------------------------------

        public bool IsZoneUnlocked(string zoneId)
        {
            if (!_zoneStates.TryGetValue(zoneId, out var rts)) return false;
            return rts.state == ZoneState.Unlocked || rts.state == ZoneState.FullyCleared;
        }

        public ZoneState GetZoneState(string zoneId)
        {
            return _zoneStates.TryGetValue(zoneId, out var rts) ? rts.state : ZoneState.Locked;
        }

        public ZoneData GetZoneForTile(Vector2Int tilePos)
        {
            if (_zones == null) return null;
            foreach (var zone in _zones)
            {
                if (zone?.tilePositions == null) continue;
                foreach (var pos in zone.tilePositions)
                    if (pos == tilePos) return zone;
            }
            return null;
        }

        public ObstacleInstance GetObstacleAt(Vector2Int tilePos)
        {
            _obstacleInstances.TryGetValue(tilePos, out var obs);
            return obs;
        }

        public void CheckZoneFullyCleared(string zoneId)
        {
            if (string.IsNullOrEmpty(zoneId)) return;
            if (!_zoneStates.TryGetValue(zoneId, out var rts)) return;

            int cleared = 0;
            foreach (var kvp in _obstacleInstances)
            {
                // 이 구역에 속한 장애물인지 확인
                var zone = FindZoneData(zoneId);
                if (zone == null) continue;
                bool inZone = false;
                if (zone.tilePositions != null)
                    foreach (var pos in zone.tilePositions)
                        if (pos == kvp.Key) { inZone = true; break; }
                if (inZone && kvp.Value.isCleared) cleared++;
            }

            rts.clearedObstacleCount = cleared;
            if (rts.totalObstacleCount > 0 && cleared >= rts.totalObstacleCount)
            {
                rts.state = ZoneState.FullyCleared;
                ZoneEvents.OnZoneFullyCleared?.Invoke(zoneId);
                Debug.Log($"[FarmZoneManager] Zone {zoneId} fully cleared!");
            }
        }

        // -------------------------------------------------------------------
        // ISaveable
        // -------------------------------------------------------------------

        public object GetSaveData()
        {
            if (_zones == null) return new ZoneSaveData { zones = new ZoneEntrySaveData[0] };

            var entries = new List<ZoneEntrySaveData>();
            foreach (var zone in _zones)
            {
                if (zone == null) continue;
                bool unlocked = IsZoneUnlocked(zone.zoneId);
                if (!unlocked) continue; // 해금된 구역만 저장

                var obstacleList = new List<ObstacleSaveData>();
                if (zone.tilePositions != null)
                {
                    foreach (var pos in zone.tilePositions)
                    {
                        if (_obstacleInstances.TryGetValue(pos, out var obs) && !obs.isCleared)
                        {
                            obstacleList.Add(new ObstacleSaveData
                            {
                                posX = pos.x,
                                posY = pos.y,
                                isCleared = obs.isCleared,
                                currentHP = obs.currentHP
                            });
                        }
                    }
                }

                entries.Add(new ZoneEntrySaveData
                {
                    zoneId = zone.zoneId,
                    isUnlocked = true,
                    obstacles = obstacleList.ToArray()
                });
            }

            return new ZoneSaveData { zones = entries.ToArray() };
        }

        public void LoadSaveData(object data)
        {
            _zoneStates.Clear();
            _obstacleInstances.Clear();

            if (_zones == null) return;

            // 모든 구역 초기 상태 설정
            foreach (var zone in _zones)
            {
                if (zone == null) continue;
                _zoneStates[zone.zoneId] = new ZoneRuntimeState(zone.zoneId, ZoneState.Locked, zone.obstacleMap?.Length ?? 0);
            }

            var saveData = data as ZoneSaveData;
            if (saveData?.zones == null)
            {
                // null-safe: Zone A만 해금
                UnlockZoneFromSave("zone_home");
                Debug.Log("[FarmZoneManager] loaded (new game), unlocked zones: 1/7");
                return;
            }

            int unlockedCount = 0;
            foreach (var entry in saveData.zones)
            {
                if (!entry.isUnlocked) continue;
                UnlockZoneFromSave(entry.zoneId);
                unlockedCount++;

                // 장애물 상태 복원
                if (entry.obstacles != null)
                {
                    foreach (var obs in entry.obstacles)
                    {
                        var pos = new Vector2Int(obs.posX, obs.posY);
                        if (_obstacleInstances.TryGetValue(pos, out var inst))
                        {
                            inst.isCleared = obs.isCleared;
                            inst.currentHP = obs.currentHP;
                        }
                    }
                }
            }

            Debug.Log($"[FarmZoneManager] loaded, unlocked zones: {unlockedCount}/{(_zones?.Length ?? 0)}");
        }

        // -------------------------------------------------------------------
        // 내부 헬퍼
        // -------------------------------------------------------------------

        private ZoneData FindZoneData(string zoneId)
        {
            if (_zones == null) return null;
            foreach (var z in _zones)
                if (z != null && z.zoneId == zoneId) return z;
            return null;
        }

        private string FindZoneIdForTile(Vector2Int tilePos)
        {
            if (_zones == null) return null;
            foreach (var zone in _zones)
            {
                if (zone?.tilePositions == null) continue;
                foreach (var pos in zone.tilePositions)
                    if (pos == tilePos) return zone.zoneId;
            }
            return null;
        }

        private void UnlockZoneFromSave(string zoneId)
        {
            if (!_zoneStates.TryGetValue(zoneId, out var rts)) return;
            rts.state = ZoneState.Unlocked;

            var zone = FindZoneData(zoneId);
            if (zone == null) return;

            if (_farmGrid != null && zone.tilePositions != null)
                _farmGrid.ActivateZoneTiles(zone.tilePositions);

            SpawnObstacles(zone);
        }
    }
}
