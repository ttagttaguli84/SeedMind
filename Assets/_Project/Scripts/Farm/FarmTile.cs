using UnityEngine;
using SeedMind.Farm.Data;

namespace SeedMind.Farm
{
    public class FarmTile : MonoBehaviour
    {
        [SerializeField] private TileState _state = TileState.Empty;
        public int gridX;
        public int gridY;
        public CropInstance cropInstance;

        private Renderer _renderer;

        public TileState State => _state;

        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
        }

        public void SetState(TileState newState)
        {
            _state = newState;
            FarmEvents.OnTileStateChanged?.Invoke(this, newState);
        }

        public void SetMaterial(Material mat)
        {
            if (_renderer != null) _renderer.material = mat;
        }
    }
}
