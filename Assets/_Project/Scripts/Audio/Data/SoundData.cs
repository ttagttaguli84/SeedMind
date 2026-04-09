using UnityEngine;

namespace SeedMind.Audio.Data
{
    /// <summary>
    /// SFX 1개의 재생 파라미터 SO. id ↔ SFXId enum 1:1 매핑.
    /// 수치 출처: docs/systems/sound-design.md
    /// </summary>
    [CreateAssetMenu(menuName = "SeedMind/Audio/SoundData", fileName = "SD_New")]
    public class SoundData : ScriptableObject
    {
        [Header("식별")]
        public SFXId id;

        [Header("클립 (variation 배열)")]
        public AudioClip[] clips;

        [Header("재생 설정")]
        public float baseVolume = 1f;       // -> see docs/systems/sound-design.md
        public float pitchVariation = 0.05f; // -> see docs/systems/sound-design.md 섹션 3.3

        [Header("공간 설정")]
        public bool is3D;
        public float maxDistance = 10f;     // -> see docs/systems/sound-design.md 섹션 3.4
    }
}
