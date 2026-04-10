using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using TMPro;
using System.Collections.Generic;

namespace SeedMind.Editor
{
    public static class SetupKoreanFont
    {
        [MenuItem("SeedMind/Setup Korean TMP Font")]
        public static void Run()
        {
            // 1. 폰트 임포트 확인
            const string fontPath = "Assets/_Project/Fonts/MalgunGothic.ttf";
            AssetDatabase.ImportAsset(fontPath, ImportAssetOptions.ForceUpdate);

            Font font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            if (font == null)
            {
                Debug.LogError("[SetupKoreanFont] MalgunGothic.ttf 로드 실패: " + fontPath);
                return;
            }

            // 2. Dynamic TMP FontAsset 생성
            const string outPath = "Assets/_Project/Fonts/MalgunGothic SDF.asset";
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outPath);
            if (existing == null)
            {
                TMP_FontAsset tmpFont = TMP_FontAsset.CreateFontAsset(
                    font, 90, 9,
                    GlyphRenderMode.SDFAA,
                    1024, 1024,
                    AtlasPopulationMode.Dynamic
                );
                tmpFont.name = "MalgunGothic SDF";
                AssetDatabase.CreateAsset(tmpFont, outPath);
                AssetDatabase.SaveAssets();
                existing = tmpFont;
                Debug.Log("[SetupKoreanFont] MalgunGothic SDF.asset 생성 완료");
            }
            else
            {
                Debug.Log("[SetupKoreanFont] MalgunGothic SDF.asset 이미 존재 — 재사용");
            }

            // 3. LiberationSans SDF fallback에 등록
            const string libPath = "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset";
            TMP_FontAsset liberation = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(libPath);
            if (liberation == null)
            {
                Debug.LogError("[SetupKoreanFont] LiberationSans SDF.asset 로드 실패: " + libPath);
                return;
            }

            if (liberation.fallbackFontAssetTable == null)
                liberation.fallbackFontAssetTable = new List<TMP_FontAsset>();

            if (!liberation.fallbackFontAssetTable.Contains(existing))
            {
                liberation.fallbackFontAssetTable.Add(existing);
                EditorUtility.SetDirty(liberation);
                AssetDatabase.SaveAssets();
                Debug.Log("[SetupKoreanFont] LiberationSans SDF fallback에 MalgunGothic SDF 등록 완료");
            }
            else
            {
                Debug.Log("[SetupKoreanFont] 이미 fallback에 등록되어 있음");
            }

            AssetDatabase.Refresh();
            Debug.Log("[SetupKoreanFont] 완료");
        }
    }
}
