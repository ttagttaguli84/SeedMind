// SetupBGMRegistry — BGMRegistry entries 배열 초기화 (clip=null)
// 실행: Unity 메뉴 > SeedMind > Setup BGM Registry
using UnityEngine;
using UnityEditor;
using SeedMind.Audio;
using SeedMind.Audio.Data;

namespace SeedMind.Editor
{
    public static class SetupBGMRegistry
    {
        [MenuItem("SeedMind/Setup BGM Registry")]
        public static void Run()
        {
            var path = "Assets/_Project/Data/Audio/BGMRegistry.asset";
            var registry = AssetDatabase.LoadAssetAtPath<BGMRegistry>(path);
            if (registry == null) { Debug.LogError("[SetupBGMRegistry] BGMRegistry.asset 없음."); return; }

            // BGMTrack.None 제외한 12개 트랙
            var tracks = new BGMTrack[]
            {
                BGMTrack.Spring, BGMTrack.Summer, BGMTrack.Autumn, BGMTrack.Winter,
                BGMTrack.NightTime, BGMTrack.Festival,
                BGMTrack.IndoorHome, BGMTrack.Shop,
                BGMTrack.Rain, BGMTrack.Storm,
                BGMTrack.TitleScreen, BGMTrack.GameOver
            };

            var so = new SerializedObject(registry);
            var prop = so.FindProperty("entries");
            prop.arraySize = tracks.Length;
            for (int i = 0; i < tracks.Length; i++)
            {
                var element = prop.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("track").enumValueIndex = (int)tracks[i];
                element.FindPropertyRelative("clip").objectReferenceValue = null;
                element.FindPropertyRelative("loopStartTime").floatValue = 0f;
                element.FindPropertyRelative("loopEndTime").floatValue = 0f;
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            Debug.Log($"[SetupBGMRegistry] BGMRegistry에 {tracks.Length}개 BGMEntry 초기화 완료.");
        }
    }
}
