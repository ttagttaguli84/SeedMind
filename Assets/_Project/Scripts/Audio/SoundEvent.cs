using UnityEngine;

namespace SeedMind.Audio
{
    /// <summary>
    /// SFX 재생 요청을 캡슐화하는 구조체.
    /// </summary>
    public struct SoundEvent
    {
        public SFXId Id;
        public Vector3? Position;
        public float VolumeScale;
        public float PitchOverride;
    }
}
