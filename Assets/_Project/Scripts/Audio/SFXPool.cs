using UnityEngine;
using UnityEngine.Audio;
using SeedMind.Audio.Data;

namespace SeedMind.Audio
{
    /// <summary>
    /// SFX 재생용 AudioSource 오브젝트 풀. Round-robin + oldest-replace 정책.
    /// poolSize -> see docs/systems/sound-design.md 섹션 3.5 (16)
    /// </summary>
    public class SFXPool
    {
        private readonly AudioSource[] _sources;
        private int _nextIndex;

        public SFXPool(Transform parent, int poolSize, AudioMixerGroup sfxGroup = null)
        {
            _sources = new AudioSource[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                var go = new GameObject($"SFX_Source_{i}");
                go.transform.SetParent(parent);
                var src = go.AddComponent<AudioSource>();
                src.playOnAwake = false;
                if (sfxGroup != null) src.outputAudioMixerGroup = sfxGroup;
                _sources[i] = src;
            }
        }

        public void Play(SoundEvent evt, SoundData data)
        {
            if (data.clips == null || data.clips.Length == 0) return;

            var source = _sources[_nextIndex];
            _nextIndex = (_nextIndex + 1) % _sources.Length;

            source.clip = data.clips[Random.Range(0, data.clips.Length)];
            source.volume = data.baseVolume * (evt.VolumeScale > 0f ? evt.VolumeScale : 1f);
            source.pitch = evt.PitchOverride > 0f
                ? evt.PitchOverride
                : 1f + Random.Range(-data.pitchVariation, data.pitchVariation);

            if (data.is3D && evt.Position.HasValue)
            {
                source.spatialBlend = 1f;
                source.transform.position = evt.Position.Value;
                source.maxDistance = data.maxDistance;
                source.rolloffMode = AudioRolloffMode.Logarithmic;
            }
            else
            {
                source.spatialBlend = 0f;
            }

            source.Play();
            Debug.Log($"[SoundManager] PlaySFX: {evt.Id} at {(evt.Position.HasValue ? evt.Position.Value.ToString() : "2D")}");
        }
    }
}
