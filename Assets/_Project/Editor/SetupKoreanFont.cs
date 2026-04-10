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
                // 1) 아틀라스 텍스처 먼저 생성
                var atlasTex = new Texture2D(1024, 1024, TextureFormat.Alpha8, false);
                atlasTex.name = "MalgunGothic SDF Atlas";

                // 2) FontAsset 생성
                TMP_FontAsset tmpFont = TMP_FontAsset.CreateFontAsset(
                    font, 90, 9,
                    GlyphRenderMode.SDFAA,
                    1024, 1024,
                    AtlasPopulationMode.Dynamic
                );
                tmpFont.name = "MalgunGothic SDF";
                tmpFont.atlasTextures = new Texture2D[] { atlasTex };

                // 3) Material — null이면 직접 생성
                var mat = tmpFont.material;
                if (mat == null)
                {
                    var shader = Shader.Find("TextMeshPro/Distance Field");
                    mat = new Material(shader);
                }
                mat.name = "MalgunGothic SDF Material";
                mat.SetTexture(ShaderUtilities.ID_MainTex, atlasTex);
                tmpFont.material = mat;

                // 4) 에셋 저장 (주 에셋 → 서브에셋 순서 중요)
                AssetDatabase.CreateAsset(tmpFont, outPath);
                AssetDatabase.AddObjectToAsset(atlasTex, outPath);
                AssetDatabase.AddObjectToAsset(mat, outPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceUpdate);
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

            // null 항목 제거 후 등록
            liberation.fallbackFontAssetTable.RemoveAll(f => f == null);

            if (!liberation.fallbackFontAssetTable.Contains(existing))
            {
                liberation.fallbackFontAssetTable.Add(existing);
                EditorUtility.SetDirty(liberation);
                AssetDatabase.SaveAssets();
                Debug.Log("[SetupKoreanFont] LiberationSans SDF fallback에 MalgunGothic SDF 등록 완료");
            }
            else
            {
                EditorUtility.SetDirty(liberation);
                AssetDatabase.SaveAssets();
                Debug.Log("[SetupKoreanFont] 이미 fallback에 등록되어 있음");
            }

            AssetDatabase.Refresh();
            Debug.Log("[SetupKoreanFont] 완료");
        }
    }
}
