using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeedMind.Audio.Data
{
    /// <summary>
    /// BGMTrack → AudioClip + 루프 사양 SO.
    /// </summary>
    [CreateAssetMenu(menuName = "SeedMind/Audio/BGMRegistry", fileName = "BGMRegistry")]
    public class BGMRegistry : ScriptableObject
    {
        [Serializable]
        public struct BGMEntry
        {
            public BGMTrack track;
            public AudioClip clip;
            public float loopStartTime; // -> see docs/systems/sound-design.md
            public float loopEndTime;   // 0이면 클립 전체 루프
        }

        [SerializeField] private BGMEntry[] entries;

        private Dictionary<BGMTrack, BGMEntry> _lookup;

        public void Initialize()
        {
            _lookup = new Dictionary<BGMTrack, BGMEntry>(entries != null ? entries.Length : 0);
            if (entries == null) return;
            foreach (var entry in entries)
                _lookup.TryAdd(entry.track, entry);
        }

        public AudioClip GetClip(BGMTrack track)
        {
            if (_lookup == null) Initialize();
            return _lookup.TryGetValue(track, out var e) ? e.clip : null;
        }

        public BGMEntry? GetEntry(BGMTrack track)
        {
            if (_lookup == null) Initialize();
            return _lookup.TryGetValue(track, out var e) ? e : (BGMEntry?)null;
        }
    }
}
