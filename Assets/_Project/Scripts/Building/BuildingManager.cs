using System;
using System.Collections.Generic;
using UnityEngine;
using SeedMind.Building.Data;
using SeedMind.Core;
using SeedMind.Farm;

namespace SeedMind.Building
{
    /// <summary>
    /// 시설 배치/건설/업그레이드/철거 생명주기를 통합 관리하는 매니저.
    /// -> see docs/systems/facilities-architecture.md 섹션 3.1
    /// </summary>
    public class BuildingManager : MonoBehaviour
    {
        [Header("시설 데이터")]
        [SerializeField] private BuildingData[] _allBuildingData;

        [Header("씬 참조")]
        [SerializeField] private Transform _buildingParent;

        // 서브시스템
        private WaterTankSystem _waterTankSystem;
        private GreenhouseSystem _greenhouseSystem;
        private StorageSystem _storageSystem;
        private ProcessingSystem _processingSystem;

        private readonly List<BuildingInstance> _buildings = new List<BuildingInstance>();

        public IReadOnlyList<BuildingInstance> Buildings => _buildings;
        public int BuildingCount => _buildings.Count;

        // 이벤트
        public event Action<BuildingInstance> OnBuildingPlaced;
        public event Action<BuildingInstance> OnBuildingCompleted;
        public event Action<BuildingInstance, int> OnBuildingUpgraded;
        public event Action<string> OnBuildingRemoved;

        private TimeManager _timeManager;

        private void Awake()
        {
            _waterTankSystem = new WaterTankSystem();
            _greenhouseSystem = new GreenhouseSystem();
            _storageSystem = new StorageSystem();
            _processingSystem = new ProcessingSystem();

            _timeManager = FindObjectOfType<TimeManager>();

            // Inspector에서 배열이 비어 있으면 Resources에서 자동 로드
            if (_allBuildingData == null || _allBuildingData.Length == 0)
                _allBuildingData = Resources.LoadAll<BuildingData>("Data/Buildings");
        }

        private void OnEnable()
        {
            if (_timeManager != null)
                _timeManager.RegisterOnDayChanged(50, OnDayChangedHandler);
        }

        private void OnDisable()
        {
            if (_timeManager != null)
                _timeManager.UnregisterOnDayChanged(OnDayChangedHandler);
        }

        private void Start()
        {
            var farmGrid = FindObjectOfType<FarmGrid>();
            _waterTankSystem.SetFarmGrid(farmGrid);
            _greenhouseSystem.SetFarmGrid(farmGrid);

            var growthSystem = FindObjectOfType<GrowthSystem>();
            if (growthSystem != null)
                growthSystem.SetSeasonOverrideProvider(_greenhouseSystem);
        }

        /// <summary>
        /// 시설 배치 가능 여부 검증.
        /// </summary>
        public bool CanPlace(BuildingData data, int gridX, int gridY)
        {
            if (data == null) return false;

            var farmGrid = FindObjectOfType<FarmGrid>();
            if (farmGrid == null) return false;

            for (int x = gridX; x < gridX + data.tileSize.x; x++)
            {
                for (int y = gridY; y < gridY + data.tileSize.y; y++)
                {
                    var tile = farmGrid.GetTile(x, y);
                    if (tile == null) return false;
                    if (tile.State != Farm.Data.TileState.Empty) return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 시설 배치 시작. 건설 중 상태로 등록.
        /// </summary>
        public BuildingInstance PlaceBuilding(BuildingData data, int gridX, int gridY)
        {
            if (!CanPlace(data, gridX, gridY)) return null;

            var inst = new BuildingInstance(data, gridX, gridY);

            var farmGrid = FindObjectOfType<FarmGrid>();
            foreach (var tileCoord in inst.GetOccupiedTiles())
            {
                var tile = farmGrid.GetTile(tileCoord.x, tileCoord.y);
                if (tile != null) tile.SetState(Farm.Data.TileState.Building);
            }

            if (data.constructionPrefab != null && _buildingParent != null)
            {
                var go = Instantiate(data.constructionPrefab, _buildingParent);
                go.transform.localPosition = new Vector3(gridX, 0, gridY);
                inst.SceneObject = go;
            }

            _buildings.Add(inst);
            BuildingEvents.RaiseBuildingPlaced(inst);
            OnBuildingPlaced?.Invoke(inst);
            return inst;
        }

        /// <summary>
        /// 시설 철거.
        /// </summary>
        public bool RemoveBuilding(BuildingInstance inst)
        {
            if (inst == null || !_buildings.Contains(inst)) return false;

            UnregisterFromSubsystem(inst);

            if (inst.SceneObject != null)
                Destroy(inst.SceneObject);

            var farmGrid = FindObjectOfType<FarmGrid>();
            foreach (var tileCoord in inst.GetOccupiedTiles())
            {
                var tile = farmGrid.GetTile(tileCoord.x, tileCoord.y);
                if (tile != null) tile.SetState(Farm.Data.TileState.Empty);
            }

            _buildings.Remove(inst);
            BuildingEvents.RaiseBuildingRemoved(inst.Data.dataId);
            OnBuildingRemoved?.Invoke(inst.Data.dataId);
            return true;
        }

        /// <summary>
        /// 시설 업그레이드.
        /// </summary>
        public bool UpgradeBuilding(BuildingInstance inst)
        {
            if (inst == null) return false;
            if (inst.UpgradeLevel >= inst.Data.maxUpgradeLevel) return false;

            inst.UpgradeLevel++;
            BuildingEvents.RaiseBuildingUpgraded(inst, inst.UpgradeLevel);
            OnBuildingUpgraded?.Invoke(inst, inst.UpgradeLevel);
            return true;
        }

        public BuildingInstance GetBuildingAt(int gridX, int gridY)
        {
            foreach (var b in _buildings)
                foreach (var tile in b.GetOccupiedTiles())
                    if (tile.x == gridX && tile.y == gridY) return b;
            return null;
        }

        public List<BuildingInstance> GetBuildingsByType(BuildingEffectType type)
        {
            var result = new List<BuildingInstance>();
            foreach (var b in _buildings)
                if (b.Data.effectType == type) result.Add(b);
            return result;
        }

        private void OnDayChangedHandler(int newDay)
        {
            AdvanceConstruction();
            _waterTankSystem.ProcessDailyWatering();
        }

        private void AdvanceConstruction()
        {
            foreach (var inst in _buildings)
            {
                if (inst.IsOperational) continue;
                if (inst.Data.buildTimeDays <= 0)
                {
                    CompleteBuilding(inst);
                    continue;
                }
                inst.BuildProgress += 1f / inst.Data.buildTimeDays;
                if (inst.BuildProgress >= 1f)
                    CompleteBuilding(inst);
            }
        }

        private void CompleteBuilding(BuildingInstance inst)
        {
            inst.IsOperational = true;
            inst.BuildProgress = 1f;

            if (inst.SceneObject != null)
                Destroy(inst.SceneObject);

            if (inst.Data.prefab != null && _buildingParent != null)
            {
                var go = Instantiate(inst.Data.prefab, _buildingParent);
                go.transform.localPosition = new Vector3(inst.GridX, 0, inst.GridY);
                inst.SceneObject = go;
            }

            RegisterToSubsystem(inst);
            BuildingEvents.RaiseBuildingCompleted(inst);
            OnBuildingCompleted?.Invoke(inst);
        }

        private void RegisterToSubsystem(BuildingInstance inst)
        {
            switch (inst.Data.effectType)
            {
                case BuildingEffectType.AutoWater:
                    _waterTankSystem.RegisterTank(inst);
                    break;
                case BuildingEffectType.SeasonBypass:
                    _greenhouseSystem.RegisterGreenhouse(inst);
                    break;
                case BuildingEffectType.Storage:
                    _storageSystem.RegisterStorage(inst);
                    break;
                case BuildingEffectType.Processing:
                    _processingSystem.RegisterProcessor(inst);
                    break;
            }
        }

        private void UnregisterFromSubsystem(BuildingInstance inst)
        {
            switch (inst.Data.effectType)
            {
                case BuildingEffectType.AutoWater:
                    _waterTankSystem.UnregisterTank(inst);
                    break;
                case BuildingEffectType.SeasonBypass:
                    _greenhouseSystem.UnregisterGreenhouse(inst);
                    break;
                case BuildingEffectType.Storage:
                    _storageSystem.UnregisterStorage(inst);
                    break;
                case BuildingEffectType.Processing:
                    _processingSystem.UnregisterProcessor(inst);
                    break;
            }
        }

        // 저장/로드
        public BuildingSaveData[] GetSaveData()
        {
            var result = new BuildingSaveData[_buildings.Count];
            for (int i = 0; i < _buildings.Count; i++)
            {
                var b = _buildings[i];
                result[i] = new BuildingSaveData
                {
                    buildingId = b.Data.dataId,
                    gridX = b.GridX,
                    gridY = b.GridY,
                    isOperational = b.IsOperational,
                    upgradeLevel = b.UpgradeLevel,
                    buildProgress = b.BuildProgress
                };
            }
            return result;
        }

        public void LoadSaveData(BuildingSaveData[] data)
        {
            if (data == null) return;
            foreach (var d in data)
            {
                BuildingData bd = FindBuildingData(d.buildingId);
                if (bd == null) continue;
                var inst = new BuildingInstance(bd, d.gridX, d.gridY);
                inst.IsOperational = d.isOperational;
                inst.UpgradeLevel = d.upgradeLevel;
                inst.BuildProgress = d.buildProgress;
                _buildings.Add(inst);
                if (inst.IsOperational) RegisterToSubsystem(inst);
            }
        }

        private BuildingData FindBuildingData(string dataId)
        {
            if (_allBuildingData == null) return null;
            foreach (var bd in _allBuildingData)
                if (bd != null && bd.dataId == dataId) return bd;
            return null;
        }

        // 디버그 메서드
        public BuildingInstance DebugBuildInstant(string dataId, int x, int y)
        {
            var bd = FindBuildingData(dataId);
            if (bd == null) { Debug.LogWarning($"[BuildingManager] DebugBuildInstant: {dataId} 없음"); return null; }
            var inst = new BuildingInstance(bd, x, y);
            _buildings.Add(inst);
            CompleteBuilding(inst);
            return inst;
        }

        public bool DebugUpgrade(string dataId)
        {
            foreach (var b in _buildings)
                if (b.Data.dataId == dataId) return UpgradeBuilding(b);
            return false;
        }

        public bool DebugDemolish(string dataId, int x, int y)
        {
            var inst = GetBuildingAt(x, y);
            if (inst != null && inst.Data.dataId == dataId) return RemoveBuilding(inst);
            return false;
        }

        public bool DebugStoreItem(string buildingId, string itemId, int count, string quality)
        {
            foreach (var b in _buildings)
                if (b.Data.dataId == buildingId && b.Data.effectType == BuildingEffectType.Storage)
                    return _storageSystem.StoreItem(b, itemId, count, quality);
            return false;
        }

        public bool DebugRetrieveItem(string buildingId, string itemId, int count)
        {
            foreach (var b in _buildings)
                if (b.Data.dataId == buildingId && b.Data.effectType == BuildingEffectType.Storage)
                    return _storageSystem.RetrieveItem(b, itemId, count, out _);
            return false;
        }
    }

    [System.Serializable]
    public class BuildingSaveData
    {
        public string buildingId;
        public int gridX;
        public int gridY;
        public bool isOperational;
        public int upgradeLevel;
        public float buildProgress;
    }
}
