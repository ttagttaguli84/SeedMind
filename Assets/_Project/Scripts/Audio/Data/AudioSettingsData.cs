namespace SeedMind.Audio.Data
{
    /// <summary>
    /// 유저 볼륨 설정 직렬화 클래스. PlayerPrefs에 JSON으로 저장.
    /// </summary>
    [System.Serializable]
    public class AudioSettingsData
    {
        public float masterVolume = 1f;
        public float bgmVolume = 1f;
        public float sfxVolume = 1f;
        public float ambientVolume = 1f;
        public float uiVolume = 1f;
        public bool masterMuted;
        public bool bgmMuted;
        public bool sfxMuted;
        public bool ambientMuted;
        public bool uiMuted;
    }
}
