// 장식 시스템 중앙 매니저
// -> see docs/systems/decoration-architecture.md 섹션 2.1
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using SeedMind.Save;
using SeedMind.Farm;
using SeedMind.Decoration.Data;

namespace SeedMind.Decoration
{
    public class DecorationManager : MonoBehaviour, ISaveable
    {
        public static DecorationManager Instance { get; private set; }

        [SerializeField] private DecorationConfig _decoConfig;
        [SerializeField] private FarmGrid _farmGrid;
        [SerializeField] private Tilemap _fenceLayer;
        [SerializeField] private Tilemap _pathLayer;
        [SerializeField] private Transform _objectLayer;

        private Dictionary<int, DecorationInstance> _items = new Dictionary<int, DecorationInstance>();
        private int _nextInstanceId = 1;

        public int SaveLoadOrder => 57;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // ── ISaveable ──────────────────────────────────────────────────────────

        public object GetSaveData()
        {
            var save = new DecorationSaveData { nextInstanceId = _nextInstanceId };
            foreach (var kv in _items)
            {
                var inst = kv.Value;
                save.decorations.Add(new DecorationInstanceSave
                {
                    instanceId        = inst.instanceId,
                    itemId            = inst.data.itemId,
                    cellX             = inst.cell.x,
                    cellZ             = inst.cell.z,
                    edge              = inst.edge,
                    durability        = inst.durability,
                    colorVariantIndex = inst.colorVariantIndex
                });
            }
            return save;
        }

        public void LoadSaveData(object data)
        {
            if (data is not DecorationSaveData save) return;
            ClearAll();
            _nextInstanceId = save.nextInstanceId;
            // 아이템 복원은 DataRegistry 연동 후 확장 예정
            Debug.Log($"[DecorationManager] Loaded {save.decorations.Count} decorations.");
        }

        // ── 배치 흐름 ──────────────────────────────────────────────────────────

        public bool CanPlace(DecorationItemData item, Vector3Int cell, EdgeDirection edge = EdgeDirection.None)
        {
            if (item == null) return false;
            if (!IsZoneUnlocked(cell)) return false;
            if (IsFarmland(cell)) return false;
            if (IsBuildingTile(cell)) return false;

            if (item.isEdgePlaced)
                return !IsEdgeOccupied(cell, edge);
            else
                return !IsOccupied(cell, item.tileWidthX, item.tileHeightZ);
        }

        public int Place(DecorationItemData item, Vector3Int cell, EdgeDirection edge = EdgeDirection.None)
        {
            if (!CanPlace(item, cell, edge)) return -1;

            int id = _nextInstanceId++;
            var inst = new DecorationInstance
            {
                instanceId        = id,
                data              = item,
                cell              = cell,
                edge              = edge,
                durability        = item.durabilityMax,
                colorVariantIndex = 0,
                runtimeObject     = null
            };

            switch (item.category)
            {
                case DecoCategoryType.Fence:
                    var edgeTile = edge is EdgeDirection.East or EdgeDirection.West ? item.edgeTileH : item.edgeTileV;
                    if (_fenceLayer != null && edgeTile != null)
                        _fenceLayer.SetTile(cell, edgeTile);
                    break;

                case DecoCategoryType.Path:
                    if (_pathLayer != null && item.floorTile != null)
                        _pathLayer.SetTile(cell, item.floorTile);
                    break;

                default:
                    if (item.prefab != null && _objectLayer != null)
                        inst.runtimeObject = Instantiate(item.prefab, new Vector3(cell.x + 0.5f, 0, cell.z + 0.5f), Quaternion.identity, _objectLayer);
                    break;
            }

            _items[id] = inst;
            DecorationEvents.OnDecorationPlaced?.Invoke(new DecorationPlacedInfo { instanceId = id, itemId = item.itemId, cell = cell });
            Debug.Log($"[DecorationManager] Placed itemId={item.itemId} at {cell} (id={id})");
            return id;
        }

        public void Remove(int instanceId)
        {
            if (!_items.TryGetValue(instanceId, out var inst)) return;

            switch (inst.data.category)
            {
                case DecoCategoryType.Fence:
                    if (_fenceLayer != null) _fenceLayer.SetTile(inst.cell, null);
                    break;
                case DecoCategoryType.Path:
                    if (_pathLayer != null) _pathLayer.SetTile(inst.cell, null);
                    break;
                default:
                    if (inst.runtimeObject != null) Destroy(inst.runtimeObject);
                    break;
            }

            _items.Remove(instanceId);
            DecorationEvents.OnDecorationRemoved?.Invoke(instanceId);
        }

        public void ClearAll()
        {
            foreach (var id in new List<int>(_items.Keys))
                Remove(id);
            _items.Clear();
        }

        // ── 배치 가능 여부 내부 검사 ───────────────────────────────────────────

        private bool IsOccupied(Vector3Int cell, int width, int height)
        {
            for (int x = 0; x < width; x++)
            for (int z = 0; z < height; z++)
            {
                var c = new Vector3Int(cell.x + x, cell.y, cell.z + z);
                foreach (var inst in _items.Values)
                    if (!inst.data.isEdgePlaced && inst.cell == c) return true;
            }
            return false;
        }

        private bool IsFarmland(Vector3Int cell)
        {
            if (_farmGrid == null) return false;
            return _farmGrid.GetTile(cell.x, cell.z) != null;
        }

        private bool IsWaterSource(Vector3Int cell) => false; // 추후 확장

        private bool IsBuildingTile(Vector3Int cell) => false; // 추후 확장

        private bool IsZoneUnlocked(Vector3Int cell)
        {
            // FarmZoneManager 연동 (Zone F 등) — 추후 확장
            return true;
        }

        private bool IsEdgeOccupied(Vector3Int cell, EdgeDirection edge)
        {
            foreach (var inst in _items.Values)
                if (inst.data.isEdgePlaced && inst.cell == cell && inst.edge == edge) return true;
            return false;
        }
    }
}
