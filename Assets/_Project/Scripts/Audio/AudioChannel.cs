namespace SeedMind.Audio
{
    /// <summary>
    /// AudioMixer의 채널 분류. 각 채널은 독립적인 볼륨/뮤트 제어를 가진다.
    /// </summary>
    public enum AudioChannel
    {
        Master,
        BGM,
        SFX,
        Ambient,
        UI
    }
}
