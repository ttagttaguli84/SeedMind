using System.Collections.Generic;
using UnityEngine;

namespace SeedMind.Audio.Data
{
    /// <summary>
    /// SFXId → SoundData 런타임 조회 테이블 SO.
    /// </summary>
    [CreateAssetMenu(menuName = "SeedMind/Audio/SoundRegistry", fileName = "SoundRegistry")]
    public class SoundRegistry : ScriptableObject
    {
        [SerializeField] private SoundData[] entries;

        private Dictionary<SFXId, SoundData> _lookup;

        public void Initialize()
        {
            _lookup = new Dictionary<SFXId, SoundData>(entries != null ? entries.Length : 0);
            if (entries == null) return;
            foreach (var entry in entries)
            {
                if (entry == null) continue;
                if (!_lookup.TryAdd(entry.id, entry))
                    Debug.LogWarning($"[SoundRegistry] 중복 SFXId: {entry.id}");
            }
        }

        public SoundData Get(SFXId id)
        {
            if (_lookup == null) Initialize();
            if (_lookup.TryGetValue(id, out var data)) return data;
            Debug.LogWarning($"[SoundRegistry] 미등록 SFXId: {id}");
            return null;
        }
    }
}
