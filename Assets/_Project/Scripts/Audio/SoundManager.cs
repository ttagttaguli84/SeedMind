using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using SeedMind.Audio.Data;
using SeedMind.Core;

namespace SeedMind.Audio
{
    /// <summary>
    /// 사운드 시스템 싱글턴. BGM crossfade, SFX 풀, 볼륨 설정 저장/로드.
    /// -> see docs/systems/sound-architecture.md 섹션 2
    /// </summary>
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        [Header("AudioMixer")]
        [SerializeField] private AudioMixer _mixer;
        [SerializeField] private AudioMixerGroup _sfxMixerGroup;

        [Header("SO 참조")]
        [SerializeField] private SoundRegistry _soundRegistry;
        [SerializeField] private BGMRegistry _bgmRegistry;

        [Header("AudioSource 참조")]
        [SerializeField] private AudioSource _bgmSourceA;
        [SerializeField] private AudioSource _bgmSourceB;
        [SerializeField] private AudioSource _ambientSource;
        [SerializeField] private AudioSource _uiSource;

        [Header("SFX 풀 설정")]
        [SerializeField] private int _sfxPoolSize = 16; // -> see docs/systems/sound-design.md 섹션 3.5

        private SFXPool _sfxPool;
        private BGMScheduler _bgmScheduler;
        private BGMTrack _currentBGM = BGMTrack.None;
        private bool _isCrossfading;
        private bool _activeBGMIsA = true;

        public BGMTrack CurrentBGM => _currentBGM;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(transform.root.gameObject);

            if (_soundRegistry != null) _soundRegistry.Initialize();
            if (_bgmRegistry != null) _bgmRegistry.Initialize();

            _sfxPool = new SFXPool(transform, _sfxPoolSize, _sfxMixerGroup);

            _bgmScheduler = new BGMScheduler();
            _bgmScheduler.Initialize(this);
        }

        private void OnDestroy()
        {
            _bgmScheduler?.Shutdown();
        }

        // ── BGM ─────────────────────────────────────────────

        public void PlayBGM(BGMTrack track, float fadeTime = 1.5f)
        {
            if (track == _currentBGM) return;
            CrossfadeBGM(track, fadeTime);
        }

        public void CrossfadeBGM(BGMTrack to, float duration = 1.5f)
        {
            var entry = _bgmRegistry?.GetEntry(to);
            if (!entry.HasValue) { _currentBGM = to; return; }

            _currentBGM = to;
            var clip = entry.Value.clip;
            if (clip == null) return;

            if (_isCrossfading) StopAllCoroutines();
            StartCoroutine(CrossfadeCoroutine(clip, duration));
        }

        private IEnumerator CrossfadeCoroutine(AudioClip newClip, float duration)
        {
            _isCrossfading = true;
            var outSrc = _activeBGMIsA ? _bgmSourceA : _bgmSourceB;
            var inSrc  = _activeBGMIsA ? _bgmSourceB : _bgmSourceA;

            if (outSrc == null || inSrc == null) { _isCrossfading = false; yield break; }

            inSrc.clip = newClip;
            inSrc.volume = 0f;
            inSrc.Play();

            float elapsed = 0f;
            float startVol = outSrc.volume;
            while (elapsed < duration)
            {
                elapsed += UnityEngine.Time.deltaTime;
                float t = elapsed / duration;
                outSrc.volume = Mathf.Lerp(startVol, 0f, t);
                inSrc.volume  = Mathf.Lerp(0f, 1f, t);
                yield return null;
            }
            outSrc.Stop();
            outSrc.volume = 1f;
            inSrc.volume  = 1f;
            _activeBGMIsA = !_activeBGMIsA;
            _isCrossfading = false;
        }

        public void StopBGM(float fadeTime = 1f)
        {
            _currentBGM = BGMTrack.None;
            if (_bgmSourceA != null) _bgmSourceA.Stop();
            if (_bgmSourceB != null) _bgmSourceB.Stop();
        }

        // ── SFX ─────────────────────────────────────────────

        public void PlaySFX(SFXId id, Vector3? position = null)
        {
            if (_soundRegistry == null) return;
            var data = _soundRegistry.Get(id);
            if (data == null) return;
            _sfxPool?.Play(new SoundEvent { Id = id, Position = position, VolumeScale = 1f }, data);
        }

        public void PlaySFX(SoundEvent evt)
        {
            if (_soundRegistry == null) return;
            var data = _soundRegistry.Get(evt.Id);
            if (data == null) return;
            _sfxPool?.Play(evt, data);
        }

        public void PlaySFXWithDelay(SFXId id, float delay)
        {
            StartCoroutine(PlaySFXDelayed(id, delay));
        }

        private IEnumerator PlaySFXDelayed(SFXId id, float delay)
        {
            yield return new WaitForSeconds(delay);
            PlaySFX(id);
        }

        public void PlayUISound(SFXId id)
        {
            if (_uiSource == null || _soundRegistry == null) return;
            var data = _soundRegistry.Get(id);
            if (data?.clips == null || data.clips.Length == 0) return;
            _uiSource.clip = data.clips[Random.Range(0, data.clips.Length)];
            _uiSource.volume = data.baseVolume;
            _uiSource.pitch = 1f + Random.Range(-data.pitchVariation, data.pitchVariation);
            _uiSource.Play();
        }

        // ── Ambient ──────────────────────────────────────────

        public void PlayAmbient(AudioClip clip, float fadeTime = 1f)
        {
            if (_ambientSource == null) return;
            _ambientSource.clip = clip;
            _ambientSource.Play();
        }

        public void StopAmbient(float fadeTime = 1f)
        {
            _ambientSource?.Stop();
        }

        // ── 볼륨/설정 ─────────────────────────────────────────

        public void SetVolume(AudioChannel channel, float volume)
        {
            if (_mixer == null) return;
            float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
            _mixer.SetFloat(channel.ToString() + "Volume", dB);
        }

        public float GetVolume(AudioChannel channel)
        {
            if (_mixer == null) return 1f;
            if (_mixer.GetFloat(channel.ToString() + "Volume", out float dB))
                return Mathf.Pow(10f, dB / 20f);
            return 1f;
        }

        public void Mute(AudioChannel channel, bool muted)
        {
            if (_mixer == null) return;
            _mixer.SetFloat(channel.ToString() + "Volume", muted ? -80f : 0f);
        }

        public void SaveAudioSettings()
        {
            var s = new AudioSettingsData
            {
                masterVolume  = GetVolume(AudioChannel.Master),
                bgmVolume     = GetVolume(AudioChannel.BGM),
                sfxVolume     = GetVolume(AudioChannel.SFX),
                ambientVolume = GetVolume(AudioChannel.Ambient),
                uiVolume      = GetVolume(AudioChannel.UI)
            };
            PlayerPrefs.SetString("AudioSettings", JsonUtility.ToJson(s));
            PlayerPrefs.Save();
            Debug.Log("[SoundManager] AudioSettings saved");
        }

        public void LoadAudioSettings()
        {
            var json = PlayerPrefs.GetString("AudioSettings", null);
            if (string.IsNullOrEmpty(json)) return;
            var s = JsonUtility.FromJson<AudioSettingsData>(json);
            SetVolume(AudioChannel.Master,  s.masterVolume);
            SetVolume(AudioChannel.BGM,     s.bgmVolume);
            SetVolume(AudioChannel.SFX,     s.sfxVolume);
            SetVolume(AudioChannel.Ambient, s.ambientVolume);
            SetVolume(AudioChannel.UI,      s.uiVolume);
            Debug.Log("[SoundManager] AudioSettings loaded");
        }

        // ── BGMScheduler 외부 API ─────────────────────────────

        public void OnWeatherChanged(WeatherType weather) => _bgmScheduler?.OnWeatherChanged(weather);
        public void ForceTrack(BGMTrack track) => _bgmScheduler?.ForceTrack(track);
        public void SetLocationTrack(BGMTrack track) => _bgmScheduler?.SetLocationTrack(track);
    }
}
