using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace SeedMind.Editor
{
    /// <summary>
    /// Assets/_Project/Audio/MainMixer.mixer 생성.
    /// AudioMixerController는 네이티브 타입이라 직접 생성 불가.
    /// Unity 에디터 "Assets/Create/Audio/Audio Mixer" 메뉴를
    /// 올바른 폴더 선택 상태로 호출하여 파일을 생성한 뒤 이름을 변경.
    /// </summary>
    public static class CreateAudioMixer
    {
        const string AudioFolder   = "Assets/_Project/Audio";
        const string FinalPath     = "Assets/_Project/Audio/MainMixer.mixer";

        [MenuItem("SeedMind/Create AudioMixer")]
        public static void Run()
        {
            if (AssetDatabase.LoadAssetAtPath<AudioMixer>(FinalPath) != null)
            {
                Debug.Log("[CreateAudioMixer] 이미 존재합니다: " + FinalPath);
                return;
            }

            // 1. 폴더 오브젝트를 선택 상태로 만든다
            var folderObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AudioFolder);
            if (folderObj == null)
            {
                AssetDatabase.CreateFolder("Assets/_Project", "Audio");
                folderObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(AudioFolder);
            }
            Selection.activeObject = folderObj;
            EditorGUIUtility.PingObject(folderObj);

            // 2. 메뉴 아이템 실행 → "New Audio Mixer.mixer" 이름으로 생성
            EditorApplication.ExecuteMenuItem("Assets/Create/Audio/Audio Mixer");

            // 3. 한 프레임 뒤에 이름 변경 (파일 생성 완료 대기)
            EditorApplication.delayCall += RenameNewMixer;
        }

        static void RenameNewMixer()
        {
            // 방금 생성된 New Audio Mixer.mixer 찾기
            var guids = AssetDatabase.FindAssets("t:AudioMixer", new[] { AudioFolder });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path != FinalPath)
                {
                    var err = AssetDatabase.RenameAsset(path, "MainMixer");
                    if (string.IsNullOrEmpty(err))
                        Debug.Log("[CreateAudioMixer] 생성 완료: " + FinalPath);
                    else
                        Debug.LogError("[CreateAudioMixer] 이름 변경 실패: " + err);
                    return;
                }
            }

            // 이미 FinalPath로 있으면 OK
            if (AssetDatabase.LoadAssetAtPath<AudioMixer>(FinalPath) != null)
                Debug.Log("[CreateAudioMixer] 완료: " + FinalPath);
            else
                Debug.LogWarning("[CreateAudioMixer] mixer 파일을 찾지 못했습니다. 수동으로 생성 필요: " + FinalPath);
        }
    }
}
